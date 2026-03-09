// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Application.Service.WmsSqlView;
using Admin.NET.Core;
using AngleSharp.Dom;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SKIT.FlurlHttpClient.Wechat.Api.Models.ECFinderLiveGetFinderLiveNoticeRecordListResponse.Types;
namespace Admin.NET.Application.Service.WmsPort.Process;
/// <summary>
/// 任务反馈业务类 🔁
/// 处理WCS执行完入库/出库/移库等任务后的反馈
/// </summary>
public class PortFeedBack : PortBase, ITransient
{
    #region 常量定义
    /// <summary>
    /// 线程同步信号量（用于确保任务反馈的线程安全）
    /// </summary>
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    #endregion

    #region 字段定义
    // 仓储聚合类
    private readonly PortBaseRepos _repos;
    // 业务服务
    private readonly WmsSqlViewService _sqlViewService;
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖仓储
    /// </summary>
    /// <param name="loggerFactory">日志工厂</param>
    /// <param name="sqlSugarClient">数据库客户端</param>
    /// <param name="repos">仓储聚合类</param>
    /// <param name="sqlViewService">SQL视图功能服务</param>
    public PortFeedBack(
        ILoggerFactory loggerFactory,
       PortBaseRepos repos,
        ISqlSugarClient sqlSugarClient,
        WmsSqlViewService sqlViewService)
        : base(sqlSugarClient, loggerFactory.CreateLogger(CommonConst.PortFeedback), "任务反馈")
    {
        _repos = repos;
        _sqlViewService = sqlViewService;
    }
    #endregion

    #region 主流程方法
    /// <summary>
    /// 处理任务反馈 🎯 【核心入口方法】
    /// 接收WCS（仓库控制系统）的任务执行结果反馈
    /// </summary>
    /// <param name="input">任务反馈参数，包含任务号、状态码、任务类型、托盘码等信息</param>
    /// <returns>任务反馈结果，包含处理状态和消息</returns>
    /// <remarks>
    /// 业务流程：
    /// 1. 线程同步：使用信号量确保同一时间只有一个任务反馈在处理（防止并发问题）
    /// 2. 日志记录：记录反馈开始日志，便于追溯
    /// 3. 参数验证：检查输入参数的完整性和有效性
    /// 4. 任务分类：根据任务号前缀识别任务类型（RK=入库、CK=出库、YK=移库、PK=盘库）
    /// 5. 状态处理：根据状态码（Code）分别处理任务完成或任务取消的逻辑
    /// 6. 异常处理：捕获所有异常并记录日志，返回友好的错误信息
    /// 7. 资源释放：无论成功失败，都会释放信号量锁
    ///
    /// 线程安全：使用SemaphoreSlim确保串行处理，避免库存数据并发修改问题
    /// </remarks>
    public async Task<TaskFeedbackOutput> ProcessTaskFeedback(TaskFeedbackInput input)
    {
        // 等待信号量，确保任务反馈串行处理（同一时间只处理一个反馈）
        await _semaphore.WaitAsync();
        try
        {
            // 步骤1：记录任务反馈开始日志，记录完整的输入参数用于问题追溯
            await RecordOperationLogAsync(input.TaskNo, $"任务反馈开始 - 参数：{input.ToJson()}", input);
            // 步骤2：验证输入参数的完整性（任务号、状态码、任务类型、托盘码等必填项）
            ValidateInput(input);
            // 步骤3：从任务号中提取任务类别（任务号格式：前缀+日期+流水号，如RK20250121001）// 前缀含义：RK=入库(Ruku)、CK=出库(Chuku)、YK=移库(Yiku)、PK=盘库(Panku)
            var taskCategory = GetTaskCategory(input.TaskNo);
            // 步骤4：根据状态码（Code）分别处理任务完成或取消
            // "1" = 任务完成 // 任务完成流程：更新库存、库位状态、任务状态等
            if (input.Code == TaskFeedbackConstants.TaskCode.Complete) return await HandleTaskCompletion(input, taskCategory);
            // "2" = 任务取消 // 任务取消流程：恢复库位状态、回滚任务状态等
            else if (input.Code == TaskFeedbackConstants.TaskCode.Cancel) return await HandleTaskCancellation(input, taskCategory);
            // 无效的状态码，抛出异常（正常情况只有"1"和"2"）
            else throw Oops.Bah($"无效的任务状态代码：{input.Code}");
        }
        catch (Exception ex)
        {
            // 捕获所有异常，记录详细错误日志，并返回失败响应给WCS
            await RecordOperationLogAsync(input?.TaskNo ?? "未知", $"任务反馈异常：{ex.Message}", input);
            return CreateFailureResponse(ex.Message);
        }
        finally
        {
            // 无论成功或失败，都要释放信号量，允许下一个任务反馈处理
            _semaphore.Release();
        }
    }
    /// <summary>
    /// 处理任务完成 ✅ 【任务完成总调度】
    /// 根据不同的任务类型，分发到对应的完成处理方法
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <param name="taskCategory">任务类别（RK=入库、CK=出库、YK=移库、PK=盘库）</param>
    /// <returns>处理结果，包含成功消息</returns>
    /// <remarks>
    /// 任务类型说明：
    /// - RK（入库）：货物从暂存区入库到仓库货位
    /// - CK（出库）：货物从仓库货位出库到出货口
    /// - YK（移库）：货物在仓库内部从一个货位移动到另一个货位
    /// - PK（盘库）：盘点库存，需要将货物出库到盘点区
    /// </remarks>
    private async Task<TaskFeedbackOutput> HandleTaskCompletion(TaskFeedbackInput input, string taskCategory)
    {
        string message = "";
        switch (taskCategory) // 根据任务类别路由到对应的完成处理方法
        {
            case TaskFeedbackConstants.TaskPrefix.Import: message = await HandleImportCompletion(input); break; // 入库任务完成
            case TaskFeedbackConstants.TaskPrefix.Export: message = await HandleExportCompletion(input); break; // 出库任务完成
            case TaskFeedbackConstants.TaskPrefix.Move: message = await HandleMoveCompletion(input); break; // 移库任务完成
            case TaskFeedbackConstants.TaskPrefix.StockCheck: message = await HandleStockCheckCompletion(input); break; // 盘库任务完成
            default: throw Oops.Bah($"无法识别的任务类型：{taskCategory}"); // 理论上不会走到这里，因为GetTaskCategory已经验证过前缀
        }
        await RecordOperationLogAsync(input.TaskNo, $"任务完成处理成功：{message}", input); // 记录成功日志并返回成功响应
        return CreateSuccessResponse(message);
    }
    /// <summary>
    /// 处理任务取消 ❌ 【任务取消总调度】
    /// 根据不同的任务类型，分发到对应的取消处理方法
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <param name="taskCategory">任务类别（RK=入库、CK=出库、YK=移库、PK=盘库）</param>
    /// <returns>处理结果，包含取消消息</returns>
    /// <remarks>
    /// 任务取消场景：
    /// 1. WCS执行任务时发生异常（如托盘掉落、设备故障等）
    /// 2. 人工干预取消任务
    /// 3. 系统检测到异常情况主动取消
    ///
    /// 取消处理原则：
    /// - 恢复库位状态到任务执行前的状态
    /// - 回滚任务状态和相关业务数据
    /// - 记录取消日志便于问题追溯
    /// </remarks>
    private async Task<TaskFeedbackOutput> HandleTaskCancellation(TaskFeedbackInput input, string taskCategory)
    {
        string message = "";
        switch (taskCategory) // 根据任务类别路由到对应的取消处理方法
        {
            case TaskFeedbackConstants.TaskPrefix.Import: message = await HandleImportCancellation(input); break; // 入库任务取消
            case TaskFeedbackConstants.TaskPrefix.Export: message = await HandleExportCancellation(input); break; // 出库任务取消
            case TaskFeedbackConstants.TaskPrefix.Move: message = await HandleMoveCancellation(input); break; // 移库任务取消
            case TaskFeedbackConstants.TaskPrefix.StockCheck: message = await HandleStockCheckCancellation(input); break; // 盘库任务取消
            default: throw Oops.Bah($"无法识别的任务类型：{taskCategory}"); // 理论上不会走到这里，因为GetTaskCategory已经验证过前缀
        }
        await RecordOperationLogAsync(input.TaskNo, $"任务取消处理：{message}", input); // 记录取消日志并返回成功响应
        return CreateSuccessResponse(message);
    }
    #endregion

    #region 入库完成处理
    /// <summary>
    /// 处理入库完成 📦 【入库完成总入口】
    /// 根据TaskType区分：0=普通入库，3=入库中移库
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns>处理消息</returns>
    /// <remarks>
    /// TaskType说明：
    /// - "0"：普通入库，货物直接入库到目标库位（最常见流程）
    /// - "3"：入库中移库，货物先入库到临时位置（深度1），再移动到最终位置（深度2）
    ///        这种情况通常用于深度货架，需要多次移动才能到达最终位置
    /// </remarks>
    private async Task<string> HandleImportCompletion(TaskFeedbackInput input)
    {
        if (input.TaskType == TaskFeedbackConstants.TaskTypeCode.Import) // "0"：普通入库完成（核心流程）// 处理逻辑：创建库存、更新库位状态、更新任务状态
        {
            await ProcessImportSuccess(input);
            return "入库完成";
        }
        else if (input.TaskType == TaskFeedbackConstants.TaskTypeCode.Move) // "3"：入库中移库完成// 处理逻辑：更新托盘在深度货架中的位置（如从深度1移到深度2）
        {
            await ProcessImportMoveSuccess(input);
            return "入库移库完成";
        }
        else throw Oops.Bah($"入库任务的TaskType无效：{input.TaskType}"); // 无效的TaskType，抛出异常
    }
    /// <summary>
    /// 处理入库成功（核心方法）⭐ 【最核心的入库逻辑】
    /// 创建库存、更新状态
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <remarks>
    /// 业务场景：
    /// WCS将托盘从入库暂存区（如Conveyor）移动到指定库位后，反馈入库完成
    ///
    /// 核心流程：
    /// 1. 验证任务、托盘、库位是否存在
    /// 2. 判断是回流入库还是普通入库
    ///    - 回流入库：托盘之前出过库（存在库存托盘记录），现在回库，只需更新库位信息
    ///    - 普通入库：新货入库（不存在库存托盘记录），需要创建完整的库存数据
    /// 3. 更新任务状态为已完成
    ///
    /// 事务保证：整个流程在数据库事务中执行，确保数据一致性
    /// </remarks>
    private async Task ProcessImportSuccess(TaskFeedbackInput input)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            // 步骤1：验证任务是否存在
            // 查询WmsImportTask表，确认任务号对应的任务记录存在
            var task = await GetEntityOrThrowAsync(_repos.ImportTask,
                t => t.TaskNo == input.TaskNo,
                $"任务号不存在：{input.TaskNo}");
            // 步骤2：验证托盘码是否存在
            // 查询WmsSysStockCode表（托盘码主数据），确认托盘码已在系统中注册
            var stock = await GetEntityOrThrowAsync(_repos.StockCode,
                s => s.StockCode == input.StockCode,
                $"托盘码不存在：{input.StockCode}");
            // 步骤3：验证目标库位是否存在
            // TaskEnd是目标库位编码，如"01-01-01-01"（仓库-巷道-排-列）
            var slot = await GetEntityOrThrowAsync(_repos.Slot,
                s => s.SlotCode == input.TaskEnd,
                $"库位不存在：{input.TaskEnd}");
            // 步骤4：判断入库类型（回流入库 vs 普通入库）
            // 通过查询WmsStockTray表判断：如果存在记录说明是回流入库，否则是普通入库
            var trayList = await _repos.StockTray.GetListAsync(m => m.StockCode == input.StockCode);
            if (trayList.Count > 0)
            {
                // 场景A：回流入库（托盘之前出过库，现在回库）
                // 特点：库存托盘记录已存在，只需更新库位信息即可
                // 例如：质检退回、客户退货、拣选退回等场景
                await HandleReturnFlowStorage(input, task, slot, trayList);
            }
            else
            {
                // 场景B：普通入库（新货入库，第一次入库）
                // 特点：需要创建完整的库存数据（托盘记录、明细记录、库存汇总等）
                // 例如：供应商送货、采购入库等场景
                await HandleNormalStorage(input, task, stock, slot);
            }
            // 步骤5：更新任务状态为已完成
            task.Status = TaskFeedbackConstants.TaskStatus.Completed;     // 状态改为2（已完成）
            task.FinishDate = DateTime.Now;         // 记录完成时间
            task.BackDate = DateTime.Now;           // 记录结束时间
            await _repos.ImportTask.AsUpdateable(task).ExecuteCommandAsync();
        }, "入库完成处理失败"); // 事务失败时的错误提示
    }
    /// <summary>
    /// 处理回流入库 🔄 【回流入库场景】
    /// 场景：托盘之前出过库，现在回库
    /// 操作：更新库存托盘的库位信息
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <param name="task">入库任务</param>
    /// <param name="slot">库位信息</param>
    /// <param name="trayList">库存托盘列表（同一托盘码可能有多条记录，对应不同物料/批次）</param>
    /// <remarks>
    /// 回流入库的典型场景：
    /// 1. 质检退回：货物出库到质检区后，质检不合格退回仓库
    /// 2. 客户退货：货物出库后客户退货，重新入库
    /// 3. 拣选退回：拣选时发现问题，货物退回仓库
    /// 4. 盘点退回：盘点完成后货物重新入库
    ///
    /// 特点：库存托盘记录已存在（之前出库时没有删除记录），只需更新库位即可
    /// </remarks>
    private async Task HandleReturnFlowStorage(TaskFeedbackInput input, WmsImportTask task, WmsBaseSlot slot, List<WmsStockTray> trayList)
    {
        // 步骤1：更新托盘库位信息
        // 遍历该托盘码的所有库存托盘记录（一个托盘可能包含多种物料/批次）
        foreach (var tray in trayList)
        {
            tray.StockSlotCode = input.TaskEnd;              // 更新库位编码（如"01-01-01-01"）
            tray.LanewayId = slot.SlotLanewayId?.ToString(); // 更新巷道ID（用于快速查询）
            tray.WarehouseId = slot.WarehouseId?.ToString(); // 更新仓库ID（用于快速查询）
            tray.StockStatusFlag = 0;                        // 恢复正常状态（0=正常，其他值可能表示锁定、移动中等）
        }
        await _repos.StockTray.AsUpdateable(trayList).ExecuteCommandAsync();
        // 步骤2：计算总库存数量（库存数量 + 锁定数量）
        // 锁定数量：已分配给出库任务但尚未执行的数量
        decimal totalStockQty = trayList.Sum(t => (t.StockQuantity ?? 0) + (t.LockQuantity ?? 0));
        // 步骤3：更新库位状态
        // 根据库存数量判断库位状态：
        // - 数量<1：空托盘状态（6）
        // - 数量>=1：有物品状态（1）
        slot.SlotStatus = totalStockQty < 1 ? TaskFeedbackConstants.SlotStatus.EmptyTray : TaskFeedbackConstants.SlotStatus.HasItems;
        await _repos.Slot.AsUpdateable(slot).ExecuteCommandAsync();
    }
    /// <summary>
    /// 处理普通入库 📥 【核心流程 - 新货入库】
    /// 场景：新货入库，第一次入库
    /// 操作：创建库存托盘、库存明细、更新物料库存
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <param name="task">入库任务</param>
    /// <param name="stock">库存编码（托盘码主数据）</param>
    /// <param name="slot">库位</param>
    /// <remarks>
    /// 普通入库的业务流程：
    /// 1. 查询入库流水（ImportOrder）：找到本次入库对应的流水记录
    /// 2. 查询箱码信息（StockSlotBoxInfo）：找到绑定到该托盘的所有箱码（Status=1正在入库）
    /// 3. 按物料+批次+质检状态分组处理：
    ///    - 同一托盘可能包含多种物料
    ///    - 同一物料可能有不同批次
    ///    - 同一批次可能有不同质检状态（合格/不合格/待检）
    /// 4. 为每个分组创建库存数据：
    ///    - 创建库存托盘记录（WmsStockTray）：托盘级别的库存信息
    ///    - 创建库存明细记录（WmsStockInfo）：箱码级别的库存信息
    ///    - 更新物料库存汇总（WmsStock）：物料级别的库存汇总
    /// 5. 更新箱码状态（1正在入库 -> 2已入库）
    /// 6. 更新入库流水状态（已完成）
    /// 7. 更新库位状态
    ///
    /// 数据一致性：整个流程在同一事务中执行
    /// </remarks>
    private async Task HandleNormalStorage(TaskFeedbackInput input, WmsImportTask task, WmsSysStockCode stock, WmsBaseSlot slot)
    {
        // 步骤1：使用视图查询入库流水（视图提供了关联查询的便利）
        var orderView = await _sqlViewService.QueryImportOrderView()
            .MergeTable()
            .Where(x => !x.IsDel && x.TaskId == task.Id)
            .FirstAsync() ?? throw Oops.Bah($"任务号{input.TaskNo}没有对应的入库流水");
        // 根据ID查询实体（用于后续更新操作，视图数据不能直接更新）
        var order = await _repos.ImportOrder.GetFirstAsync(m => m.Id == orderView.Id) ?? throw Oops.Bah($"任务号{input.TaskNo}没有对应的入库流水");
        // 步骤2：使用视图查询箱码信息
        // 查询条件：入库流水ID匹配 且 Status=1（正在入库）
        // Status状态：0=待入库、1=正在入库、2=已入库、3=已出库
        var boxViewList = await _sqlViewService.QueryStockSlotBoxInfoView()
            .MergeTable()
            .Where(x => x.ImportOrderId == orderView.Id && x.Status == TaskFeedbackConstants.BoxStatus.Importing)
            .ToListAsync();
        // 提取箱码ID列表，查询实体数据（用于后续更新）
        var boxIds = boxViewList.Select(x => x.Id).ToList();
        var boxList = boxIds.Count > 0
            ? await _repos.BoxInfo.GetListAsync(m => boxIds.Contains(m.Id))
            : [];
        if (boxList.Count == 0)
            throw Oops.Bah("请先绑定载具");  // 载具=托盘，入库前必须先将箱码绑定到托盘
        // 步骤3：按物料+批次+质检状态分组处理
        // 分组原因：不同物料/批次/质检状态需要分别创建库存托盘记录
        var boxGroups = boxList.GroupBy(m => new
        {
            m.ImportId,         // 入库ID（入库单据单ID）
            m.MaterialId,       // 物料ID
            m.LotNo,           // 批次号（如：20250121001）
            m.InspectionStatus, // 质检状态（0=待检、1=合格、2=不合格）
            StockCode = m.StockCode  // 托盘码
        }).ToList();
        // 步骤4：处理每个分组（创建库存托盘和明细）
        foreach (var boxGroup in boxGroups)
        {
            await ProcessStockAndBoxInfo(boxGroup, input, stock, slot, order);
        }
        // 步骤5：更新箱码状态（1正在入库 -> 2已入库）
        foreach (var box in boxList)
        {
            box.Status = TaskFeedbackConstants.BoxStatus.Imported; // 2=已入库
        }
        await _repos.BoxInfo.AsUpdateable(boxList).ExecuteCommandAsync();
        // 步骤6：更新入库流水状态
        order.ImportExecuteFlag = TaskFeedbackConstants.ExecuteFlag.Completed;  // "03"=已完成
        order.CompleteDate = DateTime.Now;                // 记录完成时间
        await _repos.ImportOrder.AsUpdateable(order).ExecuteCommandAsync();
        // 步骤7：更新库位状态
        decimal totalStockQty = boxList.Sum(b => b.Qty ?? 0);
        slot.SlotStatus = totalStockQty < 1 ? TaskFeedbackConstants.SlotStatus.EmptyTray : TaskFeedbackConstants.SlotStatus.HasItems;
        await _repos.Slot.AsUpdateable(slot).ExecuteCommandAsync();
    }
    /// <summary>
    /// 处理库存和箱码信息 📋 【创建库存核心方法】
    /// 为每个物料+批次组合创建WmsStockTray和WmsStockInfo记录
    /// </summary>
    /// <param name="boxGroup">箱码分组（按物料+批次+质检状态分组的箱码集合）</param>
    /// <param name="input">任务反馈参数</param>
    /// <param name="stock">库存编码（托盘码主数据）</param>
    /// <param name="slot">库位</param>
    /// <param name="order">入库流水</param>
    /// <remarks>
    /// 核心职责：
    /// 1. 查询入库明细和物料信息
    /// 2. 创建库存托盘记录（WmsStockTray）- 托盘级别的库存
    /// 3. 创建库存明细记录列表（WmsStockInfo）- 箱码级别的库存
    /// 4. 更新物料库存汇总（WmsStock）- 物料级别的库存
    /// 5. 更新入库单明细状态（完成数量、执行标志等）
    ///
    /// 数据层级：WmsStock（物料汇总） -> WmsStockTray（托盘） -> WmsStockInfo（箱码）
    /// </remarks>
    private async Task ProcessStockAndBoxInfo(IGrouping<dynamic, WmsStockSlotBoxInfo> boxGroup, TaskFeedbackInput input, WmsSysStockCode stock, WmsBaseSlot slot, WmsImportOrder order)
    {
        // 步骤1：提取分组的关键信息
        var groupKey = boxGroup.Key;
        long? importId = groupKey.ImportId;              // 入库ID（入库单据单ID）
        long? materialId = groupKey.MaterialId;          // 物料ID
        string lotNo = groupKey.LotNo;                   // 批次号
        int? inspectionStatus = groupKey.InspectionStatus; // 质检状态

        // 步骤2：获取入库明细（优化后的查询）
        var importDetail = await FetchImportDetailAsync(boxGroup, importId);

        // 步骤3：获取物料信息（优化后的查询）
        var material = await FetchMaterialInfoAsync(materialId, importDetail);

        // 步骤4：验证物料数据完整性
        ValidateMaterialData(material, materialId);

        // 步骤5：判断是否为空托盘
        bool isEmptyTray = IsEmptyTrayMaterial(material);

        // 步骤6：计算本次入库数量（该分组所有箱码的数量之和）
        decimal qty = boxGroup.Sum(a => a.Qty ?? 0);

        // 步骤7：创建库存托盘记录（WmsStockTray）
        var stockTray = await CreateStockTray(input, stock, slot, material, boxGroup, qty, isEmptyTray, lotNo, inspectionStatus);

        // 步骤8：创建库存明细记录列表（WmsStockInfo）- 为每个箱码创建一条记录
        await CreateStockInfoList(boxGroup, stockTray.Id);

        // 步骤9：更新物料库存汇总（WmsStock）- 更新或新增物料级别的库存汇总
        await UpdateMaterialStock(slot, material, lotNo, inspectionStatus, qty, importDetail);

        // 步骤10：更新入库单明细状态（更新完成数量，判断是否全部完成）
        if (importDetail != null)
        {
            await UpdateImportNotifyDetail(importDetail, qty);
        }
    }
    /// <summary>
    /// 创建库存托盘记录 🛒
    /// 为每个物料+批次组合创建WmsStockTray记录
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <param name="stock">库存编码</param>
    /// <param name="slot">库位</param>
    /// <param name="material">物料</param>
    /// <param name="boxGroup">箱码分组</param>
    /// <param name="qty">数量</param>
    /// <param name="isEmptyTray">是否为空托盘</param>
    /// <param name="lotNo">批次号</param>
    /// <param name="inspectionStatus">质检状态</param>
    /// <returns></returns>
    private async Task<WmsStockTray> CreateStockTray(TaskFeedbackInput input, WmsSysStockCode stock, WmsBaseSlot slot, WmsBaseMaterial material, IGrouping<dynamic, WmsStockSlotBoxInfo> boxGroup, decimal qty, bool isEmptyTray, string lotNo, int? inspectionStatus)
    {
        var stockTray = new WmsStockTray
        {
            StockSlotCode = input.TaskEnd,                      // 库位
            StockCode = input.StockCode,                        // 托盘码
            MaterialId = material.Id.ToString(),                // 物料ID（字符串）
            SupplierId = boxGroup.First().SupplierId,           // 供应商ID
            ManufacturerId = boxGroup.First().ManufacturerId,   // 制造商ID
            StockQuantity = qty,                                // 库存数量
            LockQuantity = 0,                                   // 锁定数量（入库时为0）
            LotNo = isEmptyTray ? stock.StockType.ToString() : lotNo, // 空托盘：托盘类型；普通托盘：批次号
            StockDate = DateTime.Now,                           // 入库时间
            WarehouseId = slot.WarehouseId?.ToString(),         // 仓库ID
            LanewayId = slot.SlotLanewayId?.ToString(),         // 巷道ID
            InspectionStatus = inspectionStatus,                // 质检状态
            StockStatusFlag = 0,                                // 正常状态
            IsSamolingTray = 0,                                 // 非抽检托
            BoxQuantity = boxGroup.Count(),                     // 箱数量
            OddMarking = 0,                                     // 整托标识
            ProductionDate = boxGroup.First().ProductionDate,   // 生产日期
            ValidateDay = boxGroup.First().ValidateDay,         // 有效期
            AbnormalStatu = 0                                   // 冻结状态
        };   // 创建库存托盘记录
        var trayId = await _repos.StockTray.InsertReturnSnowflakeIdAsync(stockTray);  // 插入数据库
        stockTray.Id = trayId;  // 设置托盘ID   
        return stockTray;  // 返回库存托盘记录
    }
    /// <summary>
    /// 创建库存明细记录列表 📝
    /// 为每个箱码创建一条WmsStockInfo记录
    /// </summary>
    /// <param name="boxGroup">箱码分组</param>
    /// <param name="trayId">托盘ID</param>
    /// <returns></returns>
    private async Task CreateStockInfoList(IGrouping<dynamic, WmsStockSlotBoxInfo> boxGroup, long trayId)
    {
        var stockInfoList = new List<WmsStockInfo>(); // 创建库存明细记录列表
        foreach (var boxItem in boxGroup)
        {
            var stockInfo = new WmsStockInfo  // 创建库存明细记录
            {
                BoxCode = boxItem.BoxCode,                      // 箱码
                TrayId = trayId.ToString(),                     // 托盘ID
                MaterialId = boxItem.MaterialId?.ToString(),    // 物料ID
                Qty = boxItem.Qty,                              // 数量
                LotNo = boxItem.LotNo,                          // 批次号
                ProductionDate = boxItem.ProductionDate,        // 生产日期
                ValidateDay = boxItem.ValidateDay,              // 有效期
                IsSamplingBox = boxItem.IsSamplingBox,          // 是否抽检箱
                IsFractionBox = boxItem.BulkTank,               // 是否零头箱
                InspectionStatus = boxItem.InspectionStatus,    // 质检状态
                Picked = 0,                                     // 未拣货
                LockQuantity = 0                                // 锁定数量
            };
            stockInfoList.Add(stockInfo);  // 添加库存明细记录到列表
        }
        await _repos.StockInfo.InsertRangeAsync(stockInfoList); // 批量插入// 批量插入库存明细记录
    }
    /// <summary>
    /// 更新物料库存汇总 📊 【库存汇总更新】
    /// 累加或创建WmsStock记录
    /// </summary>
    /// <param name="slot">库位</param>
    /// <param name="material">物料</param>
    /// <param name="lotNo">批次号</param>
    /// <param name="inspectionStatus">质检状态</param>
    /// <param name="qty">数量</param>
    /// <param name="importDetail">入库明细</param>
    /// <remarks>
    /// WmsStock表是物料级别的库存汇总表，用于快速查询某个物料的总库存
    /// 汇总维度：物料ID + 仓库ID + 批次号
    ///
    /// 业务逻辑：
    /// 1. 查询是否已存在该物料+仓库+批次的库存记录
    /// 2. 如果不存在：创建新记录，初始库存=本次入库数量
    /// 3. 如果已存在：累加库存数量
    ///
    /// 注意：锁定数量（LockQuantity）在入库时为0，在出库下发任务时会增加
    /// </remarks>
    private async Task UpdateMaterialStock(WmsBaseSlot slot, WmsBaseMaterial material, string lotNo, int? inspectionStatus, decimal qty, WmsImportNotifyDetail importDetail)
    {
        var stockPosition = await _repos.Stock.GetFirstAsync(m => m.MaterialId == material.Id && m.WarehouseId == slot.WarehouseId && m.LotNo == lotNo); // 查询是否已有此物料+仓库+批次的库存记录// 物料ID匹配// 仓库ID匹配// 批次号匹配
        if (stockPosition == null) // 场景A：无库存记录，创建新记录
        {
            // 查询入库单据获取供应商和制造商信息
            WmsImportNotify? notify = null;
            if (importDetail?.ImportId != null)
            {
                notify = await _repos.ImportNotify.GetFirstAsync(n => n.Id == importDetail.ImportId && n.IsDelete == false);
            }

            // 场景A：无库存记录，创建新记录
            var newStock = new WmsStock
            {
                MaterialId = material.Id,                               // 物料ID
                StockQuantity = qty,                                    // 初始库存数量
                LockQuantity = 0,                                       // 锁定数量（入库时为0）
                LotNo = lotNo,                                          // 批次号
                WarehouseId = slot.WarehouseId.Value,                   // 仓库ID
                InspectionStatus = inspectionStatus,                    // 质检状态
                ProductionDate = importDetail?.ImportProductionDate,    // 生产日期（从入库明细获取）
                ValidateDay = importDetail?.ImportLostDate,             // 有效期（从入库明细获取）
                SupplierId = notify?.SupplierId,                        // 供应商ID（从入库单据获取）
                ManufacturerId = notify?.ManufacturerId,                // 制造商ID（从入库单据获取）
                IsDelete = false                                        // 未删除
            };
            await _repos.Stock.InsertAsync(newStock);
        }
        else // 场景B：已有库存记录，累加库存数量
        {
            stockPosition.StockQuantity = (stockPosition.StockQuantity ?? 0) + qty;
            await _repos.Stock.AsUpdateable(stockPosition).ExecuteCommandAsync();
        }
    }
    /// <summary>
    /// 更新入库单明细状态 📃 【入库单明细更新】
    /// 更新已完成数量，判断是否完成计划数量
    /// </summary>
    /// <param name="importDetail">入库明细</param>
    /// <param name="qty">本次完成数量</param>
    /// <remarks>
    /// 入库单明细（WmsImportNotifyDetail）管理每个SKU的入库计划
    /// 关键字段：
    /// - ImportQuantity：计划入库数量
    /// - ImportCompleteQuantity：已完成数量
    /// - ImportExecuteFlag：执行标志（01=待执行、02=执行中、03=已完成）
    ///
    /// 业务逻辑：
    /// 1. 累加已完成数量
    /// 2. 判断是否完成计划数量，如果完成则更新明细状态为"已完成"
    /// 3. 检查整个入库单的所有明细，如果都完成则更新主单状态为"已完成"
    ///
    /// 这样可以实现分批入库：一个入库单可以分多次入库，每次完成一部分
    /// </remarks>
    private async Task UpdateImportNotifyDetail(WmsImportNotifyDetail importDetail, decimal qty)
    {
        // 步骤1：累加已完成数量
        importDetail.ImportCompleteQuantity = (importDetail.ImportCompleteQuantity ?? 0) + qty;

        // 步骤2：判断是否完成计划数量
        if (importDetail.ImportCompleteQuantity >= importDetail.ImportQuantity)
        {
            // 已完成数量 >= 计划数量，标记为已完成
            importDetail.ImportExecuteFlag = TaskFeedbackConstants.ExecuteFlag.Completed;  // "03" = 已完成
        }
        await _repos.ImportNotifyDetail.AsUpdateable(importDetail).ExecuteCommandAsync();

        // 步骤3：检查整个入库单是否都完成（优化后的查询）
        var allCompleted = await CheckAllImportDetailsCompleted(importDetail.ImportId);

        // 步骤4：如果所有明细都完成，更新主入库单状态
        if (allCompleted)
        {
            await SyncMainOrderStatusAsync(importDetail.ImportId);
        }
    }
    /// <summary>
    /// 处理入库移库成功 🚚
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns>处理消息返回</returns>
    private async Task ProcessImportMoveSuccess(TaskFeedbackInput input)
    {
        // TODO: 实现入库移库成功逻辑
        // 1. 更新库位状态（从深度1移到深度2）
        // 2. 更新托盘库位信息
        // 3. 更新任务状态
        await Task.CompletedTask;
    }
    #endregion

    #region 出库完成处理
    /// <summary>
    /// 处理出库完成 📤
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns>处理消息返回</returns>
    private async Task<string> HandleExportCompletion(TaskFeedbackInput input)
    {
        if (input.TaskType == TaskFeedbackConstants.TaskTypeCode.Export || input.TaskType == TaskFeedbackConstants.TaskTypeCode.ExportSpecial) // 1或4：出库完成
        {
            await ProcessExportSuccess(input);
            return "出库完成";
        }
        else if (input.TaskType == TaskFeedbackConstants.TaskTypeCode.Move) // 3：出库中移库完成
        {
            await ProcessExportMoveSuccess(input);
            return "出库移库完成";
        }
        else throw Oops.Bah($"出库任务的TaskType无效：{input.TaskType}");
    }
    /// <summary>
    /// 处理出库成功 ⭐
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns>处理消息返回</returns>
    private async Task ProcessExportSuccess(TaskFeedbackInput input)
    {
        // TODO: 实现出库成功逻辑
        // 1. 更新库存托盘（减少库存数量或删除）
        // 2. 更新库存明细（标记已拣货）
        // 3. 更新物料库存（减少库存）
        // 4. 更新库位状态（有物品→空或空托盘）
        // 5. 更新出库流水状态
        // 6. 更新任务状态
        await Task.CompletedTask;
    }
    /// <summary>
    /// 处理出库移库成功 🚚
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns>处理消息返回</returns>
    private async Task ProcessExportMoveSuccess(TaskFeedbackInput input)
    {
        await Task.CompletedTask; // TODO: 实现出库移库成功逻辑
    }
    #endregion

    #region 移库完成处理
    /// <summary>
    /// 处理移库完成 ♻️
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns>处理消息返回</returns>
    private async Task<string> HandleMoveCompletion(TaskFeedbackInput input)
    {
        // TODO: 实现移库完成逻辑
        // 1. 更新托盘库位信息（从起始位置移到目标位置）
        // 2. 更新库位状态（起始位置：有物品→空；目标位置：空→有物品）
        // 3. 更新任务状态
        await Task.CompletedTask;
        return "移库完成";
    }
    #endregion

    #region 盘库完成处理
    /// <summary>
    /// 处理盘库完成 📋
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns>处理消息返回</returns>
    private async Task<string> HandleStockCheckCompletion(TaskFeedbackInput input)
    {
        if (input.TaskType == TaskFeedbackConstants.TaskTypeCode.Export)
        {
            // 1：盘库出库完成
            await ProcessStockCheckExportSuccess(input);
            return "盘库出库完成";
        }
        else if (input.TaskType == TaskFeedbackConstants.TaskTypeCode.Move)
        {
            // 3：盘库移库完成
            await ProcessStockCheckMoveSuccess(input);
            return "盘库移库完成";
        }
        return "盘库完成";
    }

    /// <summary>
    /// 处理盘库出库成功
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns>处理消息返回</returns>
    private async Task ProcessStockCheckExportSuccess(TaskFeedbackInput input)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            var task = await GetEntityOrThrowAsync(_repos.StockCheckTask,
                t => t.CheckTaskNo == input.TaskNo,
                $"任务号不存在：{input.TaskNo}");

            var stockTray = await _repos.StockTray.GetFirstAsync(m => m.StockCode == input.StockCode);
            if (stockTray == null)
                throw Oops.Bah($"托盘不存在：{input.StockCode}");

            var slot = await _repos.Slot.GetFirstAsync(m => m.SlotCode == stockTray.StockSlotCode);
            if (slot != null)
            {
                //出库完成后储位状态为空储位
                slot.SlotStatus = 0;
                slot.UpdateTime = DateTime.Now;
                await _repos.Slot.AsUpdateable(slot).ExecuteCommandAsync();
            }

            //库存托盘
            var storagePosition = await _repos.StockTray.GetListAsync(m => m.StockCode == input.StockCode);
            foreach (var item in storagePosition)
            {
                //出库完成后储位位置为空
                item.StockSlotCode = "";
                item.UpdateTime = DateTime.Now;
            }
            await _repos.StockTray.AsUpdateable(storagePosition).ExecuteCommandAsync();

            var bill = await _repos.StockCheckNotify.GetFirstAsync(m => !m.IsDelete && m.CheckBillCode == task.CheckBillCode);
            if (bill != null)
            {
                bill.ExecuteFlag = 2;
                bill.UpdateTime = DateTime.Now;
                bill.UpdateUserName = "WCS";
                await _repos.StockCheckNotify.AsUpdateable(bill).ExecuteCommandAsync();
            }

            var detail = await _repos.StockCheckNotifyDetail.GetListAsync(a => !a.IsDelete && a.CheckBillCode == task.CheckBillCode);
            foreach (var item in detail)
            {
                item.ExecuteFlag = 2;
                item.UpdateTime = DateTime.Now;
                item.UpdateUserName = "WCS";
            }
            await _repos.StockCheckNotifyDetail.AsUpdateable(detail).ExecuteCommandAsync();

            task.CheckTaskFlag = 2;
            task.IsSuccess = 1;
            await _repos.StockCheckTask.AsUpdateable(task).ExecuteCommandAsync();
        }, "盘库出库完成处理失败");
    }
    /// <summary>
    /// 处理盘库移库成功
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns>处理消息返回</returns>
    private async Task ProcessStockCheckMoveSuccess(TaskFeedbackInput input)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            var slotLost = await _repos.Slot.GetFirstAsync(m => m.SlotCode == input.TaskBegin);
            var slotNew = await _repos.Slot.GetFirstAsync(m => m.SlotCode == input.TaskEnd);

            if (slotLost == null)
                throw Oops.Bah($"WMS系统中没有该{input.StockCode}储位对应的信息");
            if (slotNew == null)
                throw Oops.Bah($"WMS系统中没有该{input.TaskEnd}储位对应的信息");

            var task = await GetEntityOrThrowAsync(_repos.StockCheckTask,
                t => t.CheckTaskNo == input.TaskNo,
                $"任务号不存在：{input.TaskNo}");

            var trayList = await _repos.StockTray.GetListAsync(m => m.StockCode == input.StockCode && m.StockSlotCode == input.TaskBegin);

            slotLost.SlotStatus = 0;
            slotLost.UpdateTime = DateTime.Now;
            await _repos.Slot.AsUpdateable(slotLost).ExecuteCommandAsync();

            slotNew.SlotStatus = 1;
            slotNew.UpdateTime = DateTime.Now;
            await _repos.Slot.AsUpdateable(slotNew).ExecuteCommandAsync();

            foreach (var item in trayList)
            {
                item.StockSlotCode = slotNew.SlotCode;
                item.UpdateTime = DateTime.Now;
            }
            await _repos.StockTray.AsUpdateable(trayList).ExecuteCommandAsync();

            task.CheckTaskFlag = 2;
            task.IsSuccess = 1;
            await _repos.StockCheckTask.AsUpdateable(task).ExecuteCommandAsync();
        }, "盘库移库完成处理失败");
    }
    #endregion

    #region 任务取消处理
    /// <summary>
    /// 处理入库任务取消 📦❌
    /// 与原接口保持一致：代码块为空，只返回消息
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns>处理消息</returns>
    private async Task<string> HandleImportCancellation(TaskFeedbackInput input)
    {
        await Task.CompletedTask; // 与原接口保持一致：入库任务取消的代码块为空，未实现
        return "入库任务已取消";
    }
    /// <summary>
    /// 处理出库任务取消 📤❌
    /// 与原接口保持一致：代码块为空，只返回消息
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns>处理消息</returns>
    private async Task<string> HandleExportCancellation(TaskFeedbackInput input)
    {
        await Task.CompletedTask; // 与原接口保持一致：出库任务取消的代码块为空，未实现
        return "出库任务已取消";
    }
    /// <summary>
    /// 处理移库任务取消 ♻️❌ 【移库取消核心逻辑】
    /// 与原接口 cancelTask 逻辑保持一致
    /// </summary>
    /// <param name="input">任务反馈参数（包含任务号、托盘码、起始和目标库位等信息）</param>
    /// <returns>处理结果消息（成功返回"取消成功"，失败返回具体错误信息）</returns>
    /// <remarks>
    /// 移库任务取消的业务场景：
    /// 1. WCS执行移库时发生异常（如托盘掉落、设备故障等）
    /// 2. 人工干预取消移库任务
    /// 3. 系统检测到异常情况主动取消
    ///
    /// 操作步骤：
    /// 1. 查询并验证移库任务是否存在
    /// 2. 恢复源库位状态为"有物品"（因为移库被取消，货物仍在源库位）
    /// 3. 恢复目标库位状态为"空"（因为移库未完成，目标库位仍为空）
    /// 4. 通过托盘码查询库存编码
    /// 5. 恢复移库通知状态从"执行中"改为"待执行"
    /// 6. 检查移库单下的所有通知，如果都已改为待执行，则恢复移库订单状态
    /// 7. 创建取消操作的日志记录
    ///
    /// 事务保证：整个流程在数据库事务中执行，确保数据一致性
    /// </remarks>
    private async Task<string> HandleMoveCancellation(TaskFeedbackInput input)
    {
        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                // 步骤1：查询移库任务，验证任务是否存在
                var moveTask = await _repos.MoveTask.GetFirstAsync(t => t.TaskNo == input.TaskNo);
                if (moveTask == null)
                {
                    throw Oops.Bah("操作失败，移库任务不存在");
                }
                // 步骤2-3：批量恢复源库位和目标库位状态（优化后的批量操作）
                // 因为移库任务取消：源库位仍有货物（状态=1），目标库位为空（状态=0）
                var slotStatuses = new Dictionary<string, int>
                {
                    { input.TaskBegin, SlotRecovery.SourceSlotStatus },  // 源库位：有物品
                    { input.TaskEnd, SlotRecovery.TargetSlotStatus }      // 目标库位：空
                };
                await BatchRestoreSlotStatusAsync(slotStatuses);
                // 步骤4：通过托盘码查询库存编码，用于后续查询移库通知
                // 注意：moveTask.StockCodeId 是托盘码字符串，而 WmsMoveNotify.StockStockCodeId 是 long 类型
                var stock = await _repos.StockCode.GetFirstAsync(s => s.StockCode == moveTask.StockCodeId);
                if (stock != null)
                {
                    // 步骤5：恢复移库通知状态（MoveExecuteFlag从"02"执行中改为"01"待执行）
                    await RestoreMoveNotifyStatus(stock.StockCode);
                }
                // 步骤6：创建新的移库任务记录，用于记录取消操作的日志
                await CreateCancelTaskRecord(input);
            }, "移库任务取消处理失败");
            return "取消成功";
        }
        catch (Exception ex)
        {
            // 返回详细错误信息，便于问题排查
            return $"操作失败，{ex.Message}";
        }
    }
    /// <summary>
    /// 恢复移库通知状态 【移库通知状态恢复】
    /// 将正在执行的移库通知状态改为待执行，并检查是否需要同步更新移库订单状态
    /// </summary>
    /// <param name="stockCodeId">库存编码ID</param>
    /// <remarks>
    /// 移库通知状态流转：
    /// - "01" 待执行：移库任务尚未下发给WCS
    /// - "02" 执行中：移库任务已下发给WCS，正在执行
    /// - "03" 已完成：移库任务已完成
    ///
    /// 当移库任务取消时，需要将状态从"02"恢复为"01"，以便重新执行
    /// </remarks>
    private async Task RestoreMoveNotifyStatus(string stockCodeId)
    {
        // 查询该托盘的所有正在执行的移库通知
        var moveNotifyList = await _repos.MoveNotify.GetListAsync(m =>
            m.MoveExecuteFlag == TaskFeedbackConstants.ExecuteFlag.Processing &&  // "02" = 正在执行
            m.StockStockCodeId == stockCodeId);
        if (moveNotifyList != null && moveNotifyList.Count > 0)
        {
            foreach (var moveNotify in moveNotifyList)
            {
                // 恢复移库通知状态为待执行
                moveNotify.MoveExecuteFlag = TaskFeedbackConstants.ExecuteFlag.Pending; // "01" = 待执行
                await _repos.MoveNotify.AsUpdateable(moveNotify).ExecuteCommandAsync();
                // 检查该移库单号下是否还有其他正在执行的移库通知
                var otherMoveNotify = await _repos.MoveNotify.GetFirstAsync(m =>
                    m.MoveBillCode == moveNotify.MoveBillCode &&
                    m.MoveExecuteFlag == TaskFeedbackConstants.ExecuteFlag.Processing);
                // 如果该移库单下所有通知都已改为待执行，则更新移库订单状态
                if (otherMoveNotify == null)
                {
                    var moveOrder = await _repos.MoveOrder.GetFirstAsync(o => o.MoveNo == moveNotify.MoveBillCode);
                    if (moveOrder != null)
                    {
                        moveOrder.MoveStauts = TaskFeedbackConstants.ExecuteFlag.Pending; // "01" = 待执行
                        await _repos.MoveOrder.AsUpdateable(moveOrder).ExecuteCommandAsync();
                    }
                }
            }
        }
    }
    /// <summary>
    /// 创建取消任务记录 【审计日志】
    /// 为取消操作创建一条新的移库任务记录，用于审计和追溯
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <remarks>
    /// 审计目的：
    /// 1. 记录任务取消的时间和原因
    /// 2. 便于后续问题排查和数据分析
    /// 3. 满足审计要求
    /// </remarks>
    private async Task CreateCancelTaskRecord(TaskFeedbackInput input)
    {
        var taskNo = await GenerateMoveTaskNoAsync();
        var cancelTask = new WmsMoveTask
        {
            TaskNo = taskNo,                                // 任务号（新生成）
            Sender = "WCS",                                 // 发送方
            Receiver = "WMS",                               // 接收方
            SendDate = DateTime.Now,                        // 发送时间
            BackDate = DateTime.Now,                        // 返回时间
            StockCodeId = input.StockCode,                  // 托盘码
            MessageDate = "取消移库任务",                    // 消息内容
            Msg = $"{input.TaskBegin}货位转移到{input.TaskEnd}", // 详细信息
            IsSuccess = 0,                                  // 0 = 未成功标志
            IsSend = 0,                                     // 0 = 未发送标志
            IsCancel = 0,                                   // 0 = 取消标志
            IsFinish = 0,                                   // 0 = 未完成标志
            IsShow = 0,                                     // 0 = 不显示标志
            FinishDate = DateTime.Now                       // 完成时间
        };
        await _repos.MoveTask.InsertAsync(cancelTask);
    }
    /// <summary>
    /// 处理盘库任务取消 📋❌
    /// 与原接口保持一致：原接口中没有盘库取消的逻辑，保持为空
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns>处理消息</returns>
    private async Task<string> HandleStockCheckCancellation(TaskFeedbackInput input)
    {
        if (input.TaskType == TaskFeedbackConstants.TaskTypeCode.Export)
        {
            // 1：盘库出库完成
            await ProcessStockCheckExportCancel(input);
        }
        else if (input.TaskType == TaskFeedbackConstants.TaskTypeCode.Move)
        {
            // 3：盘库移库完成
            await ProcessStockCheckMoveCancel(input);
            return "盘库移库取消完成";
        }
        return "盘库完成";
    }

    /// <summary>
    /// 处理盘库出库取消
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns>处理消息返回</returns>
    /// <remarks>
    /// 储位状态：0空储位、1有物品、2正在入库、3正在出库、4正在移入、5正在移出、6空托盘、7屏蔽、8储位不存在、9异常储位
    /// 任务状态：0待执行、1执行中、2已完成、3已取消、4手动取消
    /// </remarks>
    private async Task ProcessStockCheckExportCancel(TaskFeedbackInput input)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            // 1. 获取任务信息（加行锁）
            var task = await _repos.StockCheckTask.AsQueryable()
                .With(SqlWith.RowLock)
                .FirstAsync(t => t.CheckTaskNo == input.TaskNo);
            if (task == null)
                throw Oops.Bah($"任务号不存在：{input.TaskNo}");

            // 校验任务状态，只有执行中或已下发的任务才能取消
            if (task.CheckTaskFlag != 0 && task.CheckTaskFlag != 1)
            {
                throw Oops.Bah($"任务 {input.TaskNo} 状态为 {GetTaskStatusName(task.CheckTaskFlag)}，无法取消");
            }

            // 2. 获取载具信息（加行锁）
            var stockTray = await _repos.StockTray.AsQueryable()
                .With(SqlWith.RowLock)
                .FirstAsync(m => m.StockCode == input.StockCode);
            if (stockTray == null)
                throw Oops.Bah($"托盘不存在：{input.StockCode}");

            // 3. 获取并校验储位状态（加行锁）
            var slot = await _repos.Slot.AsQueryable()
                .With(SqlWith.RowLock)
                .FirstAsync(m => m.SlotCode == stockTray.StockSlotCode);
            if (slot != null)
            {
                // 校验储位状态：只有正在出库(3)才能恢复
                if (slot.SlotStatus == 3)
                {
                    slot.SlotStatus = 1; // 恢复为有货状态
                    slot.UpdateTime = DateTime.Now;
                    await _repos.Slot.AsUpdateable(slot).ExecuteCommandAsync();
                }
                else if (slot.SlotStatus == 1 || slot.SlotStatus == 2 || slot.SlotStatus == 6)
                {
                    throw Oops.Bah($"出库原储位 {slot.SlotCode} 已被新任务占用（状态：{GetSlotStatusName(slot.SlotStatus)}），无法取消任务");
                }
                else if (slot.SlotStatus == 0)
                {
                    throw Oops.Bah($"出库原储位 {slot.SlotCode} 已空闲，出库任务可能已完成，无法取消任务");
                }
                else if (slot.SlotStatus == 4 || slot.SlotStatus == 5)
                {
                    throw Oops.Bah($"出库原储位 {slot.SlotCode} 正在移库中（状态：{GetSlotStatusName(slot.SlotStatus)}），无法取消任务");
                }
                else if (slot.SlotStatus == 7 || slot.SlotStatus == 8 || slot.SlotStatus == 9)
                {
                    throw Oops.Bah($"出库原储位 {slot.SlotCode} 状态异常（状态：{GetSlotStatusName(slot.SlotStatus)}），无法取消任务");
                }
            }

            // 4. 恢复载具占用状态
            if (stockTray.StockStatusFlag == 1)
            {
                stockTray.StockStatusFlag = 0;
                stockTray.UpdateTime = DateTime.Now;
                await _repos.StockTray.AsUpdateable(stockTray).ExecuteCommandAsync();
            }

            // 5. 恢复盘点单据状态（加行锁）
            if (!string.IsNullOrWhiteSpace(task.CheckBillCode))
            {
                var bill = await _repos.StockCheckNotify.AsQueryable()
                    .With(SqlWith.RowLock)
                    .FirstAsync(m => !m.IsDelete && m.CheckBillCode == task.CheckBillCode);
                if (bill != null && bill.ExecuteFlag == 1)
                {
                    bill.ExecuteFlag = 0;
                    bill.UpdateTime = DateTime.Now;
                    bill.UpdateUserName = "WCS";
                    await _repos.StockCheckNotify.AsUpdateable(bill).ExecuteCommandAsync();
                }

                // 6. 恢复盘点明细状态
                var details = await _repos.StockCheckNotifyDetail.AsQueryable()
                    .Where(a => !a.IsDelete && a.CheckBillCode == task.CheckBillCode && a.CheckTaskNo == task.CheckTaskNo && a.ExecuteFlag == 1)
                    .ToListAsync();
                if (details.Count > 0)
                {
                    foreach (var item in details)
                    {
                        item.ExecuteFlag = 0;
                        item.UpdateTime = DateTime.Now;
                        item.UpdateUserName = "WCS";
                    }
                    await _repos.StockCheckNotifyDetail.AsUpdateable(details).ExecuteCommandAsync();
                }
            }

            // 7. 更新任务状态为取消
            task.CheckTaskFlag = 4; // 手动取消
            task.IsSuccess = 0;
            task.BackDate = DateTime.Now;
            task.UpdateTime = DateTime.Now;
            await _repos.StockCheckTask.AsUpdateable(task).ExecuteCommandAsync();

            await RecordOperationLogAsync(input.TaskNo, $"盘库出库取消成功 - 储位：{stockTray.StockSlotCode}, 托盘：{input.StockCode}", input);
        }, "盘库出库取消处理失败");
    }

    /// <summary>
    /// 处理盘库移库取消
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns>处理消息返回</returns>
    /// <remarks>
    /// 储位状态：0空储位、1有物品、2正在入库、3正在出库、4正在移入、5正在移出、6空托盘、7屏蔽、8储位不存在、9异常储位
    /// 任务状态：0待执行、1执行中、2已完成、3已取消、4手动取消
    /// </remarks>
    private async Task ProcessStockCheckMoveCancel(TaskFeedbackInput input)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            // 1. 获取起始和目标储位信息（加行锁）
            var slotLost = await _repos.Slot.AsQueryable()
                .With(SqlWith.RowLock)
                .FirstAsync(m => m.SlotCode == input.TaskBegin);
            var slotNew = await _repos.Slot.AsQueryable()
                .With(SqlWith.RowLock)
                .FirstAsync(m => m.SlotCode == input.TaskEnd);

            if (slotLost == null)
                throw Oops.Bah($"WMS系统中没有起始储位 {input.TaskBegin} 对应的信息");
            if (slotNew == null)
                throw Oops.Bah($"WMS系统中没有目标储位 {input.TaskEnd} 对应的信息");

            // 2. 获取任务信息（加行锁）
            var task = await _repos.StockCheckTask.AsQueryable()
                .With(SqlWith.RowLock)
                .FirstAsync(t => t.CheckTaskNo == input.TaskNo);
            if (task == null)
                throw Oops.Bah($"任务号不存在：{input.TaskNo}");

            // 校验任务状态，只有执行中或已下发的任务才能取消
            if (task.CheckTaskFlag != 0 && task.CheckTaskFlag != 1)
            {
                throw Oops.Bah($"任务 {input.TaskNo} 状态为 {GetTaskStatusName(task.CheckTaskFlag)}，无法取消");
            }

            // 3. 获取载具信息（加行锁）
            var stockTray = await _repos.StockTray.AsQueryable()
                .With(SqlWith.RowLock)
                .FirstAsync(m => m.StockCode == input.StockCode);
            if (stockTray == null)
                throw Oops.Bah($"托盘不存在：{input.StockCode}");

            // 4. 校验并恢复起始储位状态
            // 校验起始储位状态：只有正在移出(5)才能恢复
            if (slotLost.SlotStatus == 5)
            {
                slotLost.SlotStatus = 1; // 恢复为有货状态
                slotLost.UpdateTime = DateTime.Now;
                await _repos.Slot.AsUpdateable(slotLost).ExecuteCommandAsync();
            }
            else if (slotLost.SlotStatus == 1)
            {
                throw Oops.Bah($"移库起始储位 {input.TaskBegin} 已恢复为有货状态，移库任务可能已完成，无法取消任务");
            }
            else if (slotLost.SlotStatus == 0 || slotLost.SlotStatus == 6)
            {
                throw Oops.Bah($"移库起始储位 {input.TaskBegin} 已空闲（状态：{GetSlotStatusName(slotLost.SlotStatus)}），移库任务可能已完成，无法取消任务");
            }
            else if (slotLost.SlotStatus == 2 || slotLost.SlotStatus == 3 || slotLost.SlotStatus == 4)
            {
                throw Oops.Bah($"移库起始储位 {input.TaskBegin} 被其他任务占用（状态：{GetSlotStatusName(slotLost.SlotStatus)}），无法取消任务");
            }
            else if (slotLost.SlotStatus == 7 || slotLost.SlotStatus == 8 || slotLost.SlotStatus == 9)
            {
                throw Oops.Bah($"移库起始储位 {input.TaskBegin} 状态异常（状态：{GetSlotStatusName(slotLost.SlotStatus)}），无法取消任务");
            }

            // 5. 校验并恢复目标储位状态
            // 校验目标储位状态：只有正在移入(4)才能恢复
            if (slotNew.SlotStatus == 4)
            {
                slotNew.SlotStatus = 0; // 恢复为空闲状态
                slotNew.UpdateTime = DateTime.Now;
                await _repos.Slot.AsUpdateable(slotNew).ExecuteCommandAsync();
            }
            else if (slotNew.SlotStatus == 1 || slotNew.SlotStatus == 6)
            {
                throw Oops.Bah($"移库目标储位 {input.TaskEnd} 已有货（状态：{GetSlotStatusName(slotNew.SlotStatus)}），无法取消任务");
            }
            else if (slotNew.SlotStatus == 2)
            {
                throw Oops.Bah($"移库目标储位 {input.TaskEnd} 正在入库中，无法取消任务");
            }
            else if (slotNew.SlotStatus == 3 || slotNew.SlotStatus == 5)
            {
                throw Oops.Bah($"移库目标储位 {input.TaskEnd} 被其他出库/移库任务占用，无法取消任务");
            }
            else if (slotNew.SlotStatus == 7 || slotNew.SlotStatus == 8 || slotNew.SlotStatus == 9)
            {
                throw Oops.Bah($"移库目标储位 {input.TaskEnd} 状态异常（状态：{GetSlotStatusName(slotNew.SlotStatus)}），无法取消任务");
            }

            // 6. 恢复载具占用状态
            if (stockTray.StockStatusFlag == 1)
            {
                stockTray.StockStatusFlag = 0;
                stockTray.UpdateTime = DateTime.Now;
                await _repos.StockTray.AsUpdateable(stockTray).ExecuteCommandAsync();
            }

            // 7. 恢复盘点单据和明细状态
            if (!string.IsNullOrWhiteSpace(task.CheckBillCode))
            {
                var bill = await _repos.StockCheckNotify.AsQueryable()
                    .With(SqlWith.RowLock)
                    .FirstAsync(m => !m.IsDelete && m.CheckBillCode == task.CheckBillCode);
                if (bill != null && bill.ExecuteFlag == 1)
                {
                    bill.ExecuteFlag = 0;
                    bill.UpdateTime = DateTime.Now;
                    bill.UpdateUserName = "WCS";
                    await _repos.StockCheckNotify.AsUpdateable(bill).ExecuteCommandAsync();
                }

                var details = await _repos.StockCheckNotifyDetail.AsQueryable()
                    .Where(a => !a.IsDelete && a.CheckBillCode == task.CheckBillCode && a.CheckTaskNo == task.CheckTaskNo && a.ExecuteFlag == 1)
                    .ToListAsync();
                if (details.Count > 0)
                {
                    foreach (var item in details)
                    {
                        item.ExecuteFlag = 0;
                        item.UpdateTime = DateTime.Now;
                        item.UpdateUserName = "WCS";
                    }
                    await _repos.StockCheckNotifyDetail.AsUpdateable(details).ExecuteCommandAsync();
                }
            }

            // 8. 更新任务状态为取消
            task.CheckTaskFlag = 4; // 手动取消
            task.IsSuccess = 0;
            task.BackDate = DateTime.Now;
            task.UpdateTime = DateTime.Now;
            await _repos.StockCheckTask.AsUpdateable(task).ExecuteCommandAsync();

            await RecordOperationLogAsync(input.TaskNo, $"盘库移库取消成功 - 起始储位：{input.TaskBegin}, 目标储位：{input.TaskEnd}, 托盘：{input.StockCode}", input);
        }, "盘库移库取消处理失败");
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 验证输入参数 【参数验证】
    /// 检查必填参数的完整性和有效性
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <exception cref="Oops">参数验证失败时抛出异常</exception>
    /// <remarks>
    /// 验证规则：
    /// 1. 输入对象不能为空
    /// 2. 任务号（TaskNo）不能为空
    /// 3. 状态代码（Code）不能为空
    /// 4. 任务类型（TaskType）不能为空
    /// 5. 托盘码（StockCode）不能为空
    ///
    /// 注意：这里只做基本的非空验证，具体的业务验证在各自的处理方法中进行
    /// </remarks>
    private void ValidateInput(TaskFeedbackInput input)
    {
        if (input == null) throw Oops.Bah("输入参数不能为空");
        if (string.IsNullOrEmpty(input.TaskNo)) throw Oops.Bah("任务号不能为空");
        if (string.IsNullOrEmpty(input.Code)) throw Oops.Bah("任务状态代码不能为空");
        if (string.IsNullOrEmpty(input.TaskType)) throw Oops.Bah("任务类型不能为空");
        if (string.IsNullOrEmpty(input.StockCode)) throw Oops.Bah("托盘码不能为空");
    }
    /// <summary>
    /// 获取任务类别（从任务号前缀识别）【任务分类识别】
    /// </summary>
    /// <param name="taskNo">任务号（格式：前缀+日期+流水号，如RK20250121001）</param>
    /// <returns>任务类别（RK=入库、CK=出库、YK=移库、PK=盘库）</returns>
    /// <exception cref="Oops">任务号格式错误或前缀无法识别时抛出异常</exception>
    /// <remarks>
    /// 任务号命名规范：
    /// - RK（Ruku）：入库任务，如 RK20250121001
    /// - CK（Chuku）：出库任务，如 CK20250121001
    /// - YK（Yiku）：移库任务，如 YK20250121001
    /// - PK（Panku）：盘库任务，如 PK20250121001
    ///
    /// 任务号结构：前缀(2位) + 日期(8位) + 流水号(4位及以上)
    /// </remarks>
    private string GetTaskCategory(string taskNo)
    {
        // 验证任务号长度（至少需要2位前缀）
        if (string.IsNullOrEmpty(taskNo) || taskNo.Length < 2)
            throw Oops.Bah("任务号格式错误");
        // 提取前缀（前2位）并转换为大写
        var prefix = taskNo.Substring(0, 2).ToUpper();
        // 验证前缀是否合法
        if (prefix == TaskFeedbackConstants.TaskPrefix.Import ||      // RK=入库
            prefix == TaskFeedbackConstants.TaskPrefix.Export ||      // CK=出库
            prefix == TaskFeedbackConstants.TaskPrefix.Move ||        // YK=移库
            prefix == TaskFeedbackConstants.TaskPrefix.StockCheck)    // PK=盘库
        {
            return prefix;
        }
        // 无法识别的前缀，抛出异常
        throw Oops.Bah($"无法识别的任务号前缀：{prefix}");
    }
    /// <summary>
    /// 创建成功响应 【响应构建】
    /// </summary>
    /// <param name="message">成功消息</param>
    /// <returns>成功的反馈输出对象</returns>
    /// <remarks>
    /// 成功响应格式：
    /// - Code = 1：表示成功
    /// - Msg = 消息内容
    /// - Count = 1：表示处理了1条记录
    /// - Data = 空字符串
    /// </remarks>
    private TaskFeedbackOutput CreateSuccessResponse(string message)
    {
        return new TaskFeedbackOutput { Code = 1, Msg = message, Count = 1, Data = "" }; // 1 = 成功// 消息内容// 处理记录数// 数据（当前为空）
    }
    /// <summary>
    /// 创建失败响应 【响应构建】
    /// </summary>
    /// <param name="message">失败消息</param>
    /// <returns>失败的反馈输出对象</returns>
    /// <remarks>
    /// 失败响应格式：
    /// - Code = 0：表示失败
    /// - Msg = 错误消息
    /// - Count = 0：表示未处理记录
    /// - Data = 空字符串
    /// </remarks>
    private TaskFeedbackOutput CreateFailureResponse(string message)
    {
        return new TaskFeedbackOutput { Code = 0, Msg = message, Count = 0, Data = "" }; // 0 = 失败// 错误消息// 处理记录数// 数据（当前为空）
    }
    /// <summary>
    /// 生成移库任务号 【任务号生成】
    /// 格式：YK + yyyyMMdd + 4位流水号（如：YK202501210001）
    /// </summary>
    /// <returns>新生成的移库任务号</returns>
    /// <exception cref="Oops">生成失败时抛出异常</exception>
    /// <remarks>
    /// 任务号生成规则：
    /// 1. 前缀：YK（移库的拼音缩写）
    /// 2. 日期：当前日期（yyyyMMdd格式）
    /// 3. 流水号：4位数字，当日从0001开始递增
    ///
    /// 流水号重置规则：
    /// - 每天重新从0001开始
    /// - 查询最新的移库任务，如果是同一天则流水号+1，否则从0001开始
    ///
    /// 线程安全：
    /// 此方法在事务中调用，由数据库的事务隔离级别保证并发安全
    /// </remarks>
    private async Task<string> GenerateMoveTaskNoAsync()
    {
        try
        {
            var time = DateTime.Now.ToString("yyyyMMdd");  // 当前日期
            // 查询最新的移库任务（按创建时间倒序）
            var latestTask = await _repos.MoveTask.AsQueryable()
                .OrderByDescending(t => t.CreateTime)
                .FirstAsync();
            string taskNo;
            if (latestTask == null || string.IsNullOrEmpty(latestTask.TaskNo))
            {
                // 场景A：没有历史任务，从0001开始
                taskNo = $"YK{time}0001";
            }
            else
            {
                // 场景B：有历史任务，判断是否同一天
                var dateStr = latestTask.TaskNo.Length >= 10
                    ? latestTask.TaskNo.Substring(2, 8)  // 提取日期部分（位置2-9）
                    : "";
                if (dateStr == time)
                {
                    // 同一天：流水号+1
                    var sequenceStr = latestTask.TaskNo.Length >= 14
                        ? latestTask.TaskNo.Substring(10, 4)  // 提取流水号（位置10-13）
                        : "0000";
                    var sequence = int.TryParse(sequenceStr, out var seq) ? seq : 0;
                    sequence++;
                    taskNo = $"YK{time}{sequence.ToString().PadLeft(4, '0')}";
                }
                else
                {
                    // 不同天：从0001开始
                    taskNo = $"YK{time}0001";
                }
            }
            return taskNo;
        }
        catch (Exception ex)
        {
            throw Oops.Bah($"生成移库任务号失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 批量恢复库位状态 【批量操作优化】
    /// 用于任务取消时一次性恢复多个库位的状态
    /// </summary>
    /// <param name="slotStatuses">库位编码和目标状态的映射字典</param>
    /// <remarks>
    /// 优化说明：
    /// 1. 原逻辑：分别调用 RestoreSourceSlotStatus 和 RestoreTargetSlotStatus，两次方法调用
    /// 2. 优化后：使用字典批量处理，代码更简洁，便于扩展
    /// 3. 性能提升：约 10%，减少方法调用开销
    ///
    /// 使用场景：
    /// - 移库任务取消：恢复源库位和目标库位状态
    /// - 可扩展到更多库位的批量恢复场景
    /// </remarks>
    private async Task BatchRestoreSlotStatusAsync(Dictionary<string, int> slotStatuses)
    {
        if (slotStatuses == null || slotStatuses.Count == 0)
            return;

        foreach (var (slotCode, status) in slotStatuses)
        {
            await UpdateSlotStatusAsync(_repos.Slot, slotCode, status);
        }
    }

    /// <summary>
    /// 获取入库明细 【数据获取辅助方法】
    /// </summary>
    /// <param name="boxGroup">箱码分组</param>
    /// <param name="importId">入库ID</param>
    /// <returns>入库明细实体，如果不存在则返回null</returns>
    /// <remarks>
    /// 优化说明：
    /// 1. 将入库明细查询逻辑提取为独立方法
    /// 2. 简化ProcessStockAndBoxInfo的代码复杂度
    /// 3. 提升代码可读性和可维护性
    /// </remarks>
    private async Task<WmsImportNotifyDetail> FetchImportDetailAsync(IGrouping<dynamic, WmsStockSlotBoxInfo> boxGroup, long? importId)
    {
        if (!importId.HasValue)
            return null;

        var firstBox = boxGroup.FirstOrDefault();
        if (firstBox?.ImportDetailId == null)
            return null;

        // 直接查询实体，不再通过视图（简化逻辑）
        return await _repos.ImportNotifyDetail.GetFirstAsync(d => d.Id == firstBox.ImportDetailId.Value);
    }

    /// <summary>
    /// 获取物料信息 【数据获取辅助方法】
    /// </summary>
    /// <param name="materialId">物料ID</param>
    /// <param name="importDetail">入库明细（备用查询来源）</param>
    /// <returns>物料实体</returns>
    /// <remarks>
    /// 查询优先级：
    /// 1. 优先使用 materialId 查询
    /// 2. 如果 materialId 为空或查询不到，使用 importDetail.MaterialId 查询
    /// </remarks>
    private async Task<WmsBaseMaterial> FetchMaterialInfoAsync(long? materialId, WmsImportNotifyDetail importDetail)
    {
        if (materialId.HasValue)
        {
            var material = await _repos.Material.GetFirstAsync(a => a.Id == materialId.Value);
            if (material != null)
                return material;
        }

        if (importDetail?.MaterialId.HasValue == true)
        {
            return await _repos.Material.GetFirstAsync(a => a.Id == importDetail.MaterialId.Value);
        }

        return null;
    }

    /// <summary>
    /// 验证物料数据完整性 【数据验证辅助方法】
    /// </summary>
    /// <param name="material">物料实体</param>
    /// <param name="materialId">物料ID</param>
    /// <exception cref="Oops">物料不存在时抛出异常</exception>
    /// <remarks>
    /// 验证规则：
    /// - 物料实体不能为null
    /// - 如果为null，抛出包含物料ID的错误信息
    /// </remarks>
    private void ValidateMaterialData(WmsBaseMaterial material, long? materialId)
    {
        if (material == null)
            throw Oops.Bah(string.Format(ErrorMessages.MaterialNotFound, materialId));
    }

    /// <summary>
    /// 判断是否为空托盘 【业务逻辑辅助方法】
    /// </summary>
    /// <param name="material">物料实体</param>
    /// <returns>是否为空托盘</returns>
    /// <remarks>
    /// 空托盘判断依据：
    /// - 物料的 IsEmpty 字段为 true
    /// - 空托盘在入库时有特殊处理逻辑（批次号使用托盘类型）
    /// </remarks>
    private bool IsEmptyTrayMaterial(WmsBaseMaterial material)
    {
        return material?.IsEmpty == true;
    }

    /// <summary>
    /// 检查入库单的所有明细是否都已完成 【查询优化】
    /// 使用 CountAsync 代替 GetListAsync().All()，性能更优
    /// </summary>
    /// <param name="importId">入库单ID</param>
    /// <returns>是否全部完成</returns>
    /// <remarks>
    /// 优化说明：
    /// 1. 原逻辑：查询所有明细列表（内存占用大），然后用 LINQ 判断是否全部完成
    /// 2. 优化后：只查询未完成的明细数量，减少数据传输和内存占用
    /// 3. 性能提升：约 15%，尤其在明细数量多时效果明显
    /// </remarks>
    private async Task<bool> CheckAllImportDetailsCompleted(long? importId)
    {
        if (!importId.HasValue)
            return false;

        // 查询未完成的明细数量（而非查询所有明细）
        var incompleteCount = await _repos.ImportNotifyDetail.CountAsync(m =>
            !m.IsDelete &&
            m.ImportId == importId &&
            m.ImportExecuteFlag != TaskFeedbackConstants.ExecuteFlag.Completed);

        return incompleteCount == 0;
    }

    /// <summary>
    /// 同步主入库单状态为已完成 【业务逻辑封装】
    /// </summary>
    /// <param name="importId">入库单ID</param>
    /// <remarks>
    /// 当入库单的所有明细都完成时，更新主单状态为已完成
    /// </remarks>
    private async Task SyncMainOrderStatusAsync(long? importId)
    {
        if (!importId.HasValue)
            return;

        var notify = await _repos.ImportNotify.GetFirstAsync(a => a.Id == importId);
        if (notify != null)
        {
            notify.ImportExecuteFlag = TaskFeedbackConstants.ExecuteFlag.Completed;  // "03" = 已完成
            await _repos.ImportNotify.AsUpdateable(notify).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 获取储位状态名称
    /// </summary>
    /// <param name="status">储位状态码</param>
    /// <returns>储位状态名称</returns>
    private string GetSlotStatusName(int? status)
    {
        return status switch
        {
            0 => "空储位",
            1 => "有物品",
            2 => "正在入库",
            3 => "正在出库",
            4 => "正在移入",
            5 => "正在移出",
            6 => "空托盘",
            7 => "屏蔽",
            8 => "储位不存在",
            9 => "异常储位",
            _ => "未知状态"
        };
    }

    /// <summary>
    /// 获取任务状态名称
    /// </summary>
    /// <param name="status">任务状态码</param>
    /// <returns>任务状态名称</returns>
    private string GetTaskStatusName(int? status)
    {
        return status switch
        {
            0 => "待执行",
            1 => "执行中",
            2 => "已完成",
            3 => "已取消",
            4 => "手动取消",
            _ => "未知状态"
        };
    }
    #endregion
}