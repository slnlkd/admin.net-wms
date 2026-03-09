// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证(版本 2.0)进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsPda;
using Admin.NET.Application.Service.WmsPda.Dto;
using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Application.Service.WmsSqlView;
using Admin.NET.Core;

using Furion.FriendlyException;

using Microsoft.Extensions.Logging;

using SqlSugar;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static SKIT.FlurlHttpClient.Wechat.Api.Models.ChannelsECWarehouseGetResponse.Types;
namespace Admin.NET.Application.Service.WmsPort.Process;
/// <summary>
/// 入库申请业务处理类，对应 JC20《ImportOrderController》系列接口。
/// 优化要点：
/// 1. 提取常量到独立的静态类
/// 2. 简化依赖注入
/// 3. 抽取通用业务逻辑方法
/// 4. 优化异常处理和日志记录
/// 5. 改善代码可读性和可维护性
/// 6. 增强线程安全性和并发控制
/// 7. 改进事务管理和数据一致性
/// 8. 提供完整的XML文档注释
/// </summary>
/// <remarks>
/// 重构版本主要改进：
/// - 使用仓储辅助类减少构造函数参数
/// - 提取验证逻辑到独立方法
/// - 改进日志记录的结构化
/// - 增强错误处理的一致性
/// - 优化数据库查询性能
/// - 提高代码的可测试性
/// </remarks>
public class PortImportApply : PortBase, ITransient
{
    #region 字段定义
    // 仓储聚合类
    private readonly PortBaseRepos _repos;
    // 库位管理服务
    private readonly PortSlotAlloc _slotManager;
    // 空托盘绑定服务
    private readonly PortImportBind _emptyTrayBind;
    // SQL视图服务
    private readonly WmsSqlViewService _sqlViewService;
    /// <summary>
    /// 信号量（用于线程同步，确保入库申请的串行处理，防止并发冲突）
    /// </summary>
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖
    /// </summary>
    /// <param name="loggerFactory">日志工厂</param>
    /// <param name="sqlSugarClient">数据库客户端</param>
    /// <param name="repos">仓储聚合类</param>
    /// <param name="slotManager">库位管理服务</param>
    /// <param name="emptyTrayBind">空托盘绑定服务</param>
    /// <param name="sqlViewService">SQL视图功能服务</param>
    public PortImportApply(
        ILoggerFactory loggerFactory,
        ISqlSugarClient sqlSugarClient,
        PortBaseRepos repos,
        PortSlotAlloc slotManager,
        PortImportBind emptyTrayBind,
        WmsSqlViewService sqlViewService)
        : base(sqlSugarClient, loggerFactory.CreateLogger(CommonConst.PortImportApply), "入库申请")
    {
        _repos = repos;
        _slotManager = slotManager;
        _emptyTrayBind = emptyTrayBind;
        _sqlViewService = sqlViewService;
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 处理入库申请
    /// </summary>
    /// <param name="input">入库申请参数，包含托盘码、仓库编码等信息</param>
    /// <returns>入库申请结果，包含任务列表和执行状态</returns>
    /// <remarks>
    /// 主要流程：
    /// 1. 参数验证
    /// 2. 获取仓库信息并验证类型
    /// 3. 根据仓库类型执行不同的入库逻辑
    /// 4. 创建入库任务并返回结果
    /// 使用信号量确保线程安全，避免并发冲突
    /// </remarks>
    /// <exception cref="Oops">当参数验证失败或业务逻辑错误时抛出</exception>
    public async Task<ImportApplyOutput> ProcessCreateImportOrder(ImportApplyInput input)
    {
        // 获取信号量，确保串行处理，防止并发导致的数据冲突
        await _semaphore.WaitAsync();
        try
        {
            // 记录入库申请开始日志
            await RecordOperationLogAsync(input.StockCode, "入库申请开始", input);

            try
            {
                // 第一步：验证输入参数的完整性和有效性
                ValidateInput(input);

                // 第二步：获取仓库信息并验证仓库类型
                var warehouse = await GetWarehouseAsync(input.HouseCode);
                // 验证仓库类型与货位类型匹配
                ValidateWarehouseType(warehouse, input.Type);

                // 第三步：根据仓库类型执行相应的入库处理流程
                var taskInfoList = await ProcessImportByWarehouseTypeAsync(input, warehouse);

                // 第四步：检查是否有TaskNo="0"的错误项（兼容JC20逻辑）
                var errorTask = taskInfoList.FirstOrDefault(t => t.TaskNo == ImportApplyConstants.BusinessConstants.ErrorTaskNo);
                if (errorTask != null)
                {
                    await RecordOperationLogAsync(input.StockCode, $"入库申请失败：{errorTask.GoodsCode}", input);
                    return CreateFailureResponse(errorTask.GoodsCode);
                }

                // 第五步：记录成功日志并返回
                await RecordOperationLogAsync(input.StockCode, $"入库申请成功，生成{taskInfoList.Count}个任务", input);
                return CreateSuccessResponse(taskInfoList);
            }
            catch (Exception ex)
            {
                // 处理业务异常，记录日志
                await RecordOperationLogAsync(input.StockCode, $"入库申请失败：{ex.Message}", input);
                return CreateFailureResponse(ex.Message);
            }
        }
        catch (Exception ex)
        {
            // 处理系统级异常
            await RecordOperationLogAsync(input?.StockCode ?? "未知", $"入库申请系统异常：{ex.Message}", input);
            return CreateFailureResponse($"系统异常：{ex.Message}");
        }
        finally
        {
            // 释放信号量，允许下一个请求处理
            _semaphore.Release();
        }
    }
    /// <summary>
    /// 处理二次入库申请
    /// </summary>
    /// <param name="input">二次入库申请参数，包含托盘号、AB面、巷道ID和储位地址</param>
    /// <returns>二次入库申请结果，包含新分配的储位地址</returns>
    /// <remarks>
    /// 二次入库场景：
    /// 当第一次入库分配的储位因某些原因无法使用时，系统会触发二次入库申请，
    /// 重新为托盘分配合适的储位。
    /// 主要用于处理入库过程中的异常情况。
    /// </remarks>
    public async Task<ImportApplyOutput> ProcessCreateImportOrder2(ImportApply2Input input)
    {
        try
        {
            // 记录二次申请的详细参数
            await RecordOperationLogAsync(
                input.StockCode,
                // $"二次入库申请 - 托盘号：{input.StockCode}，AB面：{input.IsAB}，" +
                $"二次入库申请 - 托盘号：{input.StockCode}，" +
                $"巷道ID：{input.LaneWayId}，原储位：{input.SlotCode}",
                input);
            // 获取新的储位地址
            var newSlotCode = await GetSecondImportSlotCodeAsync(input);
            // 判断是否成功获取到新储位
            if (!string.IsNullOrEmpty(newSlotCode))
            {
                await RecordOperationLogAsync(input.StockCode, $"二次入库成功，新储位：{newSlotCode}", input);
                return CreateSuccessResponse(
                [
                    new() { TaskBegin = newSlotCode }
                ]);
            }
            // 未获取到储位，返回失败
            await RecordOperationLogAsync(input.StockCode, "二次入库失败：未找到可用储位", input);
            return CreateFailureResponse("获取储位失败");
        }
        catch (Exception ex)
        {
            // 记录异常并返回错误响应
            await RecordOperationLogAsync(input.StockCode, $"二次入库申请异常：{ex.Message}", input);
            return CreateFailureResponse($"二次入库异常：{ex.Message}");
        }
    }
    #endregion

    #region 私有方法 - 验证逻辑
    /// <summary>
    /// 验证入库申请输入参数
    /// </summary>
    /// <param name="input">输入参数</param>
    /// <exception cref="Oops">当参数验证失败时抛出</exception>
    /// <remarks>
    /// 验证必填字段：
    /// - input对象不能为空
    /// - 托盘码不能为空
    /// - 仓库编码不能为空
    /// </remarks>
    private static void ValidateInput(ImportApplyInput input)
    {
        if (input == null)
            throw Oops.Bah("输入参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.StockCode))
            throw Oops.Bah("托盘码不能为空！");
        if (string.IsNullOrWhiteSpace(input.HouseCode))
            throw Oops.Bah("仓库编码不能为空！");
    }
    /// <summary>
    /// 验证仓库类型与货位类型匹配
    /// </summary>
    /// <param name="warehouse">仓库信息</param>
    /// <param name="slotType">货位类型</param>
    /// <exception cref="Oops">当类型不匹配时抛出</exception>
    /// <remarks>
    /// 对于立体库和平库，必须指定货位类型。
    /// 这是因为这两种仓库类型有特定的货位管理要求。
    /// </remarks>
    private static void ValidateWarehouseType(WmsBaseWareHouse warehouse, string slotType)
    {
        var warehouseName = warehouse.WarehouseType switch
        {
            ImportApplyConstants.WarehouseType.StereoWarehouse => "立体库",  
            ImportApplyConstants.WarehouseType.FlatWarehouse => "平库",      
            _ => null
        };

        if (warehouseName != null && string.IsNullOrWhiteSpace(slotType))
        {
            throw Oops.Bah($"{warehouseName}入库时，货位类型不能为空！");
        }
    }
    /// <summary>
    /// 验证托盘是否正在执行出库
    /// </summary>
    /// <param name="stockCode">托盘码</param>
    /// <exception cref="Oops">当托盘正在出库时抛出</exception>
    /// <remarks>
    /// 检查出库流水中是否存在该托盘的活动出库任务。
    /// 如果存在，则不允许入库，防止数据冲突。
    /// </remarks>
    private async Task ValidateNoActiveExportAsync(string stockCode)
    {
        var hasActiveExport = await _repos.ExportOrder.IsAnyAsync(a =>
            a.ExportStockCode == stockCode &&
            (a.ExportExecuteFlag == 0 || a.ExportExecuteFlag == 1));
        if (hasActiveExport)
            throw Oops.Bah($"出库流水中存在托盘{stockCode}的活动任务，不可入库！");
    }
    /// <summary>
    /// 验证托盘是否已在库内
    /// </summary>
    /// <param name="stockCode">托盘码</param>
    /// <exception cref="Oops">当托盘已在库内时抛出</exception>
    /// <remarks>
    /// 检查库存托盘表，如果托盘已有库位编码，说明已在库内，
    /// 不允许重复入库。
    /// </remarks>
    private async Task ValidateTrayNotInWarehouseAsync(string stockCode)
    {
        var stockTray = await _repos.StockTray.GetFirstAsync(a => a.StockCode == stockCode);
        if (stockTray != null && !string.IsNullOrWhiteSpace(stockTray.StockSlotCode))
            throw Oops.Bah($"托盘{stockCode}已在库内（库位：{stockTray.StockSlotCode}），不可入库！");
    }
    /// <summary>
    /// 验证入库单据单状态
    /// </summary>
    /// <param name="importId">入库单据单ID</param>
    /// <exception cref="Oops">当通知单状态异常时抛出</exception>
    /// <remarks>
    /// 验证入库单据单的有效性：
    /// 1. 如果没有通知单ID，视为空托盘入库，直接通过
    /// 2. 检查通知单是否存在
    /// 3. 检查通知单是否已作废
    /// </remarks>
    private async Task ValidateImportNotifyStatusAsync(long? importId)
    {
        if (!importId.HasValue)
            return; // 空托盘入库，无需验证通知单
        var importNotify = await _repos.ImportNotify.GetFirstAsync(a =>
            a.Id == importId.Value && !a.IsDelete) ?? throw Oops.Bah("托盘未绑定有效的入库单，不可入库！");
        if (importNotify.ImportExecuteFlag == ImportApplyConstants.ExecuteFlag.Invalid)
            throw Oops.Bah("入库单已作废，不可入库！");
    }
    /// <summary>
    /// 验证托盘码是否受WMS管理
    /// </summary>
    /// <param name="stockCode">托盘码</param>
    /// <returns>托盘码实体</returns>
    /// <exception cref="Oops">当托盘码不受管理时抛出</exception>
    /// <remarks>
    /// 检查托盘码是否在系统中注册，只有注册的托盘才能进行入库操作。
    /// </remarks>
    private async Task<WmsSysStockCode> ValidateAndGetStockCodeAsync(string stockCode)
    {
        var stock = await _repos.StockCode.GetFirstAsync(a => a.StockCode == stockCode) ?? throw Oops.Bah($"托盘条码{stockCode}不受WMS管理，不可入库！");
        return stock;
    }
    #endregion

    #region 私有方法 - 仓库处理
    /// <summary>
    /// 获取仓库信息
    /// </summary>
    /// <param name="warehouseCode">仓库编码</param>
    /// <returns>仓库实体</returns>
    /// <exception cref="Oops">当仓库不存在时抛出</exception>
    private async Task<WmsBaseWareHouse> GetWarehouseAsync(string warehouseCode)
    {
        var warehouse = await _repos.Warehouse.GetFirstAsync(w => w.WarehouseCode == warehouseCode) ?? throw Oops.Bah($"仓库信息不存在，仓库编码：{warehouseCode}");
        return warehouse;
    }
    /// <summary>
    /// 根据仓库类型处理入库
    /// </summary>
    /// <param name="input">入库申请参数</param>
    /// <param name="warehouse">仓库信息</param>
    /// <returns>任务列表</returns>
    /// <exception cref="Oops">当仓库类型不支持时抛出</exception>
    /// <remarks>
    /// 根据仓库类型选择不同的处理策略：
    /// - 立体库：需要考虑库位深度、移库等复杂逻辑
    /// - 平库：处理相对简单，重点在于空托盘绑定和回流入库
    /// </remarks>
    private async Task<List<WcsTaskModeOutput>> ProcessImportByWarehouseTypeAsync(ImportApplyInput input, WmsBaseWareHouse warehouse)
    {
        try
        {
            return warehouse.WarehouseType switch
            {
                //处理立体库入库申请
                ImportApplyConstants.WarehouseType.StereoWarehouse => await ProcessStereoWarehouseImportAsync(input, warehouse),
                //处理平库入库申请
                ImportApplyConstants.WarehouseType.FlatWarehouse => await ProcessFlatWarehouseImportAsync(input, warehouse),
                //处理其他仓库类型
                _ => throw Oops.Bah($"不支持的仓库类型：{warehouse.WarehouseType}（{warehouse.WarehouseName}）")
            };
        }
        catch (Exception ex)
        {
            // 判断是否为库位分配失败（库位不足、储位不足、未找到）
            var isSlotError = ex.Message.Contains("库位不足") ||
                             ex.Message.Contains("储位不足") ||
                             ex.Message.Contains("未找到");
            if (!isSlotError) throw;
            // 库位分配失败时，返回TaskNo="0"的错误任务项（兼容JC20逻辑）
            await RecordOperationLogAsync(input.StockCode, $"库位分配失败：{ex.Message}", input);
            return
            [
                new WcsTaskModeOutput
                {
                    TaskNo = ImportApplyConstants.BusinessConstants.ErrorTaskNo,  // 错误标记（常量"0"）
                    TaskMode = ImportApplyConstants.TaskMode.Import,              // 任务类型
                    StockCode = input.StockCode,                                  // 托盘号
                    TaskBegin = string.Empty,                                     // 开始储位号
                    TaskEnd = string.Empty,                                       // 结束储位号
                    GoodsCode = ex.Message,                                       // 错误信息
                    GoodsQTY = 0
                }
            ];
        }
    }
    #endregion

    #region 私有方法 - 立体库入库
    /// <summary>
    /// 处理立体库入库申请
    /// </summary>
    /// <param name="input">入库申请参数</param>
    /// <param name="warehouse">仓库信息</param>
    /// <returns>任务列表</returns>
    /// <remarks>
    /// 立体库入库流程：
    /// 1. 验证托盘状态（不在出库流程、不在库内）
    /// 2. 获取箱码信息，判断是否为回流入库
    /// 3. 获取物料信息
    /// 4. 分配目标库位
    /// 5. 处理深度2库位的移库需求
    /// 6. 创建入库任务
    /// </remarks>
    private async Task<List<WcsTaskModeOutput>> ProcessStereoWarehouseImportAsync(ImportApplyInput input, WmsBaseWareHouse warehouse)
    {
        // 第一步：验证托盘的基本状态
        await ValidateNoActiveExportAsync(input.StockCode);
        await ValidateTrayNotInWarehouseAsync(input.StockCode);
        // 第二步：验证托盘码有效性
        var stock = await ValidateAndGetStockCodeAsync(input.StockCode);
        // 第三步：获取箱码信息
        var (boxViewList, boxList) = await GetBoxInfoByStockCodeAsync(input.StockCode);
        // 第四步：判断是否为回流入库（箱码为空表示回流）
        var isReturnFlow = boxList.Count == 0;
        // 第五步：获取物料信息和数量
        var (materialCode, sumQty) = await GetMaterialInfoAsync(boxViewList, boxList, isReturnFlow, input.StockCode);
        // 第六步：分配目标库位
        var targetSlot = await AllocateSlotAsync(input, warehouse, materialCode);
        // 第七步：处理深度2库位需要移库的情况
        var taskListResult = new List<WcsTaskModeOutput>();
        // 如果目标库位在内层（深度2），需要先将外层（深度1）的货物移走
        if (targetSlot.SlotInout == ImportApplyConstants.SlotDepth.Inside)
        {
            var moveTask = await HandleDeepSlotMoveAsync(targetSlot);
            if (moveTask != null)
            {
                taskListResult.Add(moveTask.Value.Task);
                targetSlot = moveTask.Value.NewTargetSlot;
            }
        }
        // 第八步：创建入库任务
        var importTasks = await CreateImportTaskAsync(input, targetSlot, boxList, sumQty, materialCode);
        taskListResult.AddRange(importTasks);
        return taskListResult;
    }
    /// <summary>
    /// 获取物料信息（编码和数量）
    /// </summary>
    /// <param name="boxViewList">箱码视图列表</param>
    /// <param name="boxList">箱码列表</param>
    /// <param name="isReturnFlow">是否为回流入库</param>
    /// <param name="stockCode">托盘码</param>
    /// <returns>物料编码和总数量</returns>
    /// <exception cref="Oops">当物料信息缺失或不完整时抛出</exception>
    /// <remarks>
    /// 根据不同场景获取物料信息：
    /// - 回流入库：从库存托盘表获取
    /// - 正常入库：从入库流水和通知单获取
    /// </remarks>
    private async Task<(string MaterialCode, decimal SumQty)> GetMaterialInfoAsync(List<ViewStockSlotBoxInfoView> boxViewList, List<WmsStockSlotBoxInfo> boxList, bool isReturnFlow, string stockCode)
    {
        // 场景1：回流入库（出库后再次入库）
        if (isReturnFlow)
        {
            var trayList = await _repos.StockTray.GetListAsync(m => m.StockCode == stockCode);
            if (trayList.Count == 0)
                throw Oops.Bah($"托盘{stockCode}不具有库存信息，不可入库！");
            var firstTray = trayList.First();
            var totalQty = (firstTray.StockQuantity ?? 0) + (firstTray.LockQuantity ?? 0);
            return (firstTray.MaterialId, totalQty);
        }
        // 场景2：正常入库，先验证是否已有库存记录
        var trayList2 = await _repos.StockTray.GetListAsync(m => m.StockCode == stockCode);
        if (trayList2.Count > 0)
            throw Oops.Bah($"库内已存在托盘{stockCode}，不可入库！");
        // 获取第一个箱码的入库流水ID
        var firstBoxView = boxViewList.FirstOrDefault();
        if (firstBoxView?.ImportOrderId == null)
            throw Oops.Bah("箱码信息缺少入库流水ID！");
        // 查询入库流水信息
        var orderView = await _sqlViewService.QueryImportOrderView()
            .MergeTable()
            .Where(x => !x.IsDel && x.Id == firstBoxView.ImportOrderId.Value)
            .FirstAsync() ?? throw Oops.Bah($"入库流水不存在，ID：{firstBoxView.ImportOrderId}");
        // 如果已分配库位，使用已有信息（后续流程会处理）
        if (!string.IsNullOrWhiteSpace(orderView.ImportSlotCode))
            return (string.Empty, 0);
        // 获取物料编码（从入库单据单明细）
        var materialCode = string.Empty;
        if (firstBoxView.ImportDetailId.HasValue)
        {
            var importDetailView = await _sqlViewService.QueryImportNotifyDetailView()
                .MergeTable()
                .Where(x => x.Id == firstBoxView.ImportDetailId.Value)
                .FirstAsync();
            if (importDetailView != null)
            {
                materialCode = importDetailView.MaterialId?.ToString() ?? string.Empty;
                await ValidateImportNotifyStatusAsync(firstBoxView.ImportId);
            }
        }
        return (materialCode, 0);
    }
    /// <summary>
    /// 处理深度2库位的移库逻辑
    /// </summary>
    /// <param name="targetSlot">目标库位（深度2）</param>
    /// <returns>移库任务和新的目标库位（如果需要移库）</returns>
    /// <remarks>
    /// 立体库的库位分为深度1（外层）和深度2（内层）：
    /// - 如果要将货物放入深度2，必须先确保深度1为空
    /// - 如果深度1有货，需要先执行移库操作
    /// - 移库后，原深度1成为新的目标库位
    /// 这个设计是为了确保货物的先进先出和操作安全性。
    /// </remarks>
    private async Task<(WcsTaskModeOutput Task, WmsBaseSlot NewTargetSlot)?> HandleDeepSlotMoveAsync(WmsBaseSlot targetSlot)
    {
        // 获取外层库位（深度1）
        var outsideSlot = await GetOutsideSlotAsync(targetSlot);
        // 如果外层库位不存在或为空，无需移库
        if (outsideSlot == null || outsideSlot.SlotStatus != ImportApplyConstants.SlotStatus.HasItems)
            return null;
        // 查询外层库位的托盘信息
        var stockInFront = await _repos.StockTray.GetFirstAsync(m =>
            m.StockSlotCode == outsideSlot.SlotCode);
        // 如果数据不一致（库位显示有货但查不到托盘），清空库位状态
        if (stockInFront == null)
        {
            outsideSlot.SlotStatus = ImportApplyConstants.SlotStatus.Empty;
            await _repos.Slot.AsUpdateable(outsideSlot).ExecuteCommandAsync();
            return null;
        }
        // 执行移库操作（在事务中完成）
        WcsTaskModeOutput moveTask = null;
        var tranResult = await _sqlSugarClient.Ado.UseTranAsync(async () =>
        {
            // 在事务中重新查询并锁定两个库位
            var lockedOutsideSlot = await _repos.Slot.AsQueryable()
                .Where(s => s.SlotCode == outsideSlot.SlotCode)
                .FirstAsync();
            var lockedTargetSlot = await _repos.Slot.AsQueryable()
                .Where(s => s.SlotCode == targetSlot.SlotCode)
                .FirstAsync();

            // 再次验证状态
            if (lockedOutsideSlot.SlotStatus != ImportApplyConstants.SlotStatus.HasItems)
                return;  // 状态已改变，无需移库

            // 创建移库任务：从外层移到内层
            moveTask = await CreateMoveTaskAsync(stockInFront, lockedOutsideSlot, lockedTargetSlot);
            // 更新库位状态
            lockedOutsideSlot.SlotStatus = ImportApplyConstants.SlotStatus.MovingOut;  // 外层正在移出
            lockedTargetSlot.SlotStatus = ImportApplyConstants.SlotStatus.MovingIn;    // 内层正在移入
            await _repos.Slot.AsUpdateable(new[] { lockedOutsideSlot, lockedTargetSlot })
                .ExecuteCommandAsync();
        });

        // 检查事务执行结果
        if (!tranResult.IsSuccess)
        {
            var errorMsg = tranResult.ErrorException?.Message ?? tranResult.ErrorMessage ?? "未知错误";
            throw Oops.Bah($"移库操作失败：{errorMsg}");
        }

        // 返回移库任务和新的目标库位（原外层库位）
        return (moveTask, outsideSlot);
    }
    #endregion

    #region 私有方法 - 平库入库
    /// <summary>
    /// 处理平库入库申请
    /// </summary>
    /// <param name="input">入库申请参数</param>
    /// <param name="warehouse">仓库信息</param>
    /// <returns>任务列表</returns>
    /// <remarks>
    /// 平库入库流程：
    /// 1. 验证托盘基本条件（是否在出库流程、托盘码是否有效）
    /// 2. 处理托盘状态（空托盘绑定与状态校验）
    /// 3. 判断是否为回流入库（根据箱码信息）
    /// 4. 根据不同场景执行相应的入库逻辑（回流入库或普通入库）
    /// </remarks>
    private async Task<List<WcsTaskModeOutput>> ProcessFlatWarehouseImportAsync(ImportApplyInput input, WmsBaseWareHouse warehouse)
    {
        // 第一步：验证托盘不在出库流程中
        await ValidateNoActiveExportAsync(input.StockCode);
        // 第二步：验证托盘码有效性
        var stock = await ValidateAndGetStockCodeAsync(input.StockCode);
        // 第三步：验证并处理托盘状态（空托盘绑定与状态校验）
        await ValidateAndHandleTrayStatusAsync(stock, input.StockCode);
        // 第四步：获取箱码信息
        var (boxViewList, boxList) = await GetBoxInfoByStockCodeAsync(input.StockCode);
        var isReturnFlow = boxList.Count == 0;
        // 第五步：验证托盘不在库内
        await ValidateTrayNotInWarehouseAsync(input.StockCode);
        // 第六步：根据是否回流执行不同的处理逻辑
        if (isReturnFlow)
        {
            return await ProcessReturnFlowImportAsync(input, boxList);
        }
        // 第七步：处理普通入库
        return await ProcessNormalImportAsync(input, boxViewList, boxList);
    }
    /// <summary>
    /// 验证并处理托盘状态
    /// </summary>
    /// <param name="stock">托盘码实体</param>
    /// <param name="stockCode">托盘码</param>
    /// <exception cref="Oops">当绑定失败或状态无效时抛出</exception>
    /// <remarks>
    /// 处理逻辑：
    /// 1. 未使用 + 仓储笼/质检托盘 → 自动绑定，状态变为"使用中"
    /// 2. 未使用 + 其他托盘类型 → 抛出异常，不允许入库
    /// 3. 其他状态 → 直接通过验证
    /// </remarks>
    private async Task ValidateAndHandleTrayStatusAsync(WmsSysStockCode stock, string stockCode)
    {
        var stockCodeType = stock.Status ?? 0;
        // 判断是否需要绑定：未使用状态 且 是仓储笼或质检托盘
        var needsBinding = stock.Status == ImportApplyConstants.StockCodeStatus.Unused &&
                          (stock.StockType == ImportApplyConstants.StockCodeType.StorageCage ||
                           stock.StockType == ImportApplyConstants.StockCodeType.QualityCheckTray);
        if (needsBinding)
        {
            // 调用空托盘绑定服务
            var bindResult = await _emptyTrayBind.ProcessKBackConfirmAsync(new EmptyTrayBindInput
            {
                PalletCode = stockCode,
                Quantity = 1,
                ActionType = "add"
            });
            // 检查绑定结果
            if (!bindResult.Success)
            {
                var message = string.IsNullOrWhiteSpace(bindResult.Message)
                    ? "空托盘绑定失败！"
                    : bindResult.Message;
                throw Oops.Bah(message);
            }
            // 绑定成功，状态已变为使用中，无需再验证
        }
        else if (stockCodeType == ImportApplyConstants.StockCodeStatus.Unused)
        {
            // 不满足绑定条件但状态为未使用，抛出异常
            throw Oops.Bah($"托盘{stockCode}状态为未使用，不可入库，请核实！");
        }
    }
    /// <summary>
    /// 处理回流入库
    /// </summary>
    /// <param name="input">入库申请参数</param>
    /// <param name="boxList">箱码列表</param>
    /// <returns>任务列表</returns>
    /// <exception cref="Oops">当验证失败时抛出</exception>
    /// <remarks>
    /// 回流入库场景：
    /// 1. 盘点回库：将盘点出库的托盘重新入库
    /// 2. 普通回流：出库后未完全拣货的托盘回库
    /// 需要特别处理锁定数量和未拣货物料的情况。
    /// </remarks>
    private async Task<List<WcsTaskModeOutput>> ProcessReturnFlowImportAsync(ImportApplyInput input, List<WmsStockSlotBoxInfo> boxList)
    {
        // 获取托盘的库存信息
        var trayList = await _repos.StockTray.GetListAsync(m => m.StockCode == input.StockCode);
        if (trayList.Count == 0)
            throw Oops.Bah($"托盘{input.StockCode}不具有库存信息，不可入库！");
        // 场景1：检查是否为盘点回库
        var backToWarehouseInfo = await _repos.BackToWarehouse.GetFirstAsync(a =>
            a.TrayNO == input.StockCode &&
            !a.IsDelete &&
            a.State == ImportApplyConstants.BackToWarehouseState.WaitingReturn);
        if (backToWarehouseInfo != null)
            return await ProcessStockCheckReturnAsync(input, backToWarehouseInfo);
        // 场景2：普通回流入库，验证业务规则
        // 2.1 检查是否有未拣货物料
        if (await HasUnpickedItemsAsync(input.StockCode))
            throw Oops.Bah($"托盘{input.StockCode}含有未拣货的货物，请核实");
        // 2.2 检查锁定数量
        if (trayList.Any(tray => tray.LockQuantity != 0))
            throw Oops.Bah($"托盘{input.StockCode}未扫码拆垛，不可入库！请先通过手持进行扫码拆垛出库。");
        // 获取物料信息
        var firstTray = trayList.First();
        var materialCode = firstTray.MaterialId;
        var sumQty = (firstTray.StockQuantity ?? 0) + (firstTray.LockQuantity ?? 0);
        // 根据数量判断是空托还是有货，分配相应的库位
        var slotResult = sumQty == 0
            ? await _slotManager.GetNullLocations(input.StockCode)  // 空托盘
            : await _slotManager.GetLocations(input.StockCode, ImportApplyConstants.BusinessConstants.ReturnFlowIdentifier);  // 有货托盘
        // 解析库位结果字符串
        var (slotCode, abSide) = ParseSlotResult(slotResult);
        // 根据库位编码获取库位信息
        var targetSlot = await GetSlotByCodeAsync(slotCode);
        // 创建入库任务
        // return await CreateImportTaskAsync(
        //     input, targetSlot, boxList, sumQty, materialCode, null, abSide, true);
        return await CreateImportTaskAsync(input, targetSlot, boxList, sumQty, materialCode, null, "", true);
    }
    /// <summary>
    /// 处理普通入库
    /// </summary>
    /// <param name="input">入库申请参数</param>
    /// <param name="boxViewList">箱码视图列表</param>
    /// <param name="boxList">箱码列表</param>
    /// <returns>任务列表</returns>
    /// <exception cref="Oops">当验证失败时抛出</exception>
    /// <remarks>
    /// 普通入库流程：
    /// 1. 验证库存托盘表中不存在该托盘
    /// 2. 获取入库流水信息
    /// 3. 判断是否已分配库位
    /// 4. 如果未分配，则进行库位分配
    /// 5. 创建入库任务
    /// </remarks>
    private async Task<List<WcsTaskModeOutput>> ProcessNormalImportAsync(ImportApplyInput input, List<ViewStockSlotBoxInfoView> boxViewList, List<WmsStockSlotBoxInfo> boxList)
    {
        // 验证托盘不在库内
        var trayList = await _repos.StockTray.GetListAsync(m => m.StockCode == input.StockCode);
        if (trayList.Count > 0)
            throw Oops.Bah($"库内已存在托盘{input.StockCode}，不可入库！");
        // 获取箱码关联的入库流水信息
        var firstBoxView = boxViewList.FirstOrDefault();
        if (firstBoxView?.ImportOrderId == null)
            throw Oops.Bah("箱码信息缺少入库流水ID！");
        var orderView = await _sqlViewService.QueryImportOrderView()
            .MergeTable()
            .Where(x => !x.IsDel && x.Id == firstBoxView.ImportOrderId.Value)
            .FirstAsync() ?? throw Oops.Bah($"入库流水不存在，ID：{firstBoxView.ImportOrderId}");
        var order = await _repos.ImportOrder.GetFirstAsync(m => m.Id == orderView.Id) ?? throw Oops.Bah($"入库流水信息不存在，ID：{orderView.Id}");
        // 场景1：如果已分配库位，直接使用
        if (!string.IsNullOrWhiteSpace(orderView.ImportSlotCode))
        {
            var targetSlot = await GetSlotByCodeAsync(orderView.ImportSlotCode);
            return await CreateImportTaskAsync(input, targetSlot, boxList, 0, string.Empty, order);
        }
        // 场景2：未分配库位，需要进行分配
        var (slotResult, materialCode) = await AllocateSlotForNormalImportAsync(input, boxViewList, firstBoxView);
        // 解析库位结果字符串
        var (slotCode, abSide) = ParseSlotResult(slotResult);
        // 根据库位编码获取库位信息
        var slot = await GetSlotByCodeAsync(slotCode);
        // return await CreateImportTaskAsync(input, slot, boxList, 0, materialCode, order, abSide, false);
        // 创建入库任务
        return await CreateImportTaskAsync(input, slot, boxList, 0, materialCode, order, "", false);
    }
    /// <summary>
    /// 为普通入库分配库位
    /// </summary>
    /// <param name="input">入库申请参数</param>
    /// <param name="boxViewList">箱码视图列表</param>
    /// <param name="firstBoxView">第一个箱码视图</param>
    /// <returns>库位结果字符串和物料编码</returns>
    /// <remarks>
    /// 分配策略：
    /// - 空托盘：分配空托盘专用库位
    /// - 有货托盘：根据物料信息分配合适的库位
    /// </remarks>
    private async Task<(string SlotResult, string MaterialCode)> AllocateSlotForNormalImportAsync(ImportApplyInput input, List<ViewStockSlotBoxInfoView> boxViewList, ViewStockSlotBoxInfoView firstBoxView)
    {
        string slotResult;
        string materialCode;
        // 判断是否为空托盘入库（没有入库单据单明细ID）
        var isEmptyTray = !firstBoxView.ImportDetailId.HasValue || firstBoxView.ImportDetailId.Value == 0;
        if (isEmptyTray)
        {
            // 空托盘入库
            slotResult = await _slotManager.GetNullLocations(input.StockCode);
            materialCode = firstBoxView.MaterialId?.ToString() ?? string.Empty;
        }
        else
        {
            // 有货托盘入库，需要获取物料信息
            var importDetailView = await _sqlViewService.QueryImportNotifyDetailView()
                .MergeTable()
                .Where(x => x.Id == firstBoxView.ImportDetailId.Value)
                .FirstAsync();
            if (importDetailView != null)
            {
                materialCode = importDetailView.MaterialId?.ToString() ?? string.Empty;
                await ValidateImportNotifyStatusAsync(firstBoxView.ImportId);
            }
            else
            {
                materialCode = string.Empty;
            }
            // 分配普通库位
            slotResult = await _slotManager.GetLocations(input.StockCode, string.Empty);
        }
        return (slotResult, materialCode);
    }
    /// <summary>
    /// 检查是否有未拣货物料
    /// </summary>
    /// <param name="stockCode">托盘码</param>
    /// <returns>是否存在未拣货物料</returns>
    /// <remarks>
    /// 查询出库流水，检查是否存在已拣货数量小于应拣货数量的记录。
    /// 这种情况表示托盘上还有未完成拣货的物料，不应该入库。
    /// </remarks>
    private async Task<bool> HasUnpickedItemsAsync(string stockCode)
    {
        var exportOrders = await _repos.ExportOrder.GetListAsync(a =>
            a.ExportStockCode == stockCode &&
            (a.ExportExecuteFlag == 1 || a.ExportExecuteFlag == 2) &&
            a.ExportBillType.HasValue &&
            !a.IsDelete);
        return exportOrders.Any(order =>
            (order.PickedNum ?? 0) < (order.ExportQuantity ?? 0));
    }
    /// <summary>
    /// 处理盘点回库
    /// </summary>
    /// <param name="input">入库申请参数</param>
    /// <param name="backInfo">回库信息</param>
    /// <returns>任务列表</returns>
    /// <exception cref="Oops">当暂存位置为空时抛出</exception>
    /// <remarks>
    /// 盘点回库是一个特殊的入库流程：
    /// 1. 托盘从盘点区域（暂存位置）回到正式库位（目标位置）
    /// 2. 需要创建盘点任务记录
    /// 3. 更新回库状态和库位状态
    /// </remarks>
    private async Task<List<WcsTaskModeOutput>> ProcessStockCheckReturnAsync(ImportApplyInput input, WmsBackToWareHouse backInfo)
    {
        // 验证暂存位置
        if (string.IsNullOrWhiteSpace(backInfo.TemporaryLocation))
            throw Oops.Bah("盘点回库的暂存位置为空，请核实！");
        var taskList = new List<WcsTaskModeOutput>();
        // 在事务中完成所有数据库操作
        var tranResult = await _sqlSugarClient.Ado.UseTranAsync(async () =>
        {
            // 在事务中重新查询并锁定暂存库位
            var tempSlot = await _repos.Slot.AsQueryable()
                .Where(s => s.SlotCode == backInfo.TemporaryLocation)
                .FirstAsync() ?? throw new InvalidOperationException($"暂存库位{backInfo.TemporaryLocation}不存在！");

            // 验证暂存库位状态（防止并发冲突）
            if (tempSlot.SlotStatus != ImportApplyConstants.SlotStatus.HasItems)
            {
                throw new InvalidOperationException($"暂存库位{tempSlot.SlotCode}状态异常（当前状态：{tempSlot.SlotStatus}），无法完成盘点回库！");
            }

            // 1. 创建盘点任务
            var taskNo = await _sqlViewService.GenerateTaskNo();
            var checkTask = new WmsStockCheckTask
            {
                CheckTaskNo = taskNo,
                Sender = "WMS",
                Receiver = "WCS",
                IsSuccess = 0,
                TaskType = 2,              // 盘点类型
                StockCode = backInfo.TrayNO,
                StartLocation = backInfo.TemporaryLocation,
                EndLocation = backInfo.TargetLocation,
                Information = "",
                SendDate = DateTime.Now,
                BackDate = null,
                Msg = "由" + backInfo.TemporaryLocation + "储位入库到" + backInfo.TargetLocation + "储位",
                IsDelete = false,
                CreateTime = DateTime.Now,
                //CheckNotifyId = backInfo.ExportOrderNo,
                CheckBillCode = backInfo.BillCode,
                //WarehouseId = warehouseId.ToString(),
            };
            await _repos.StockCheckTask.InsertAsync(checkTask);

            // 2. 更新回库状态为"正在回库"
            backInfo.State = ImportApplyConstants.BackToWarehouseState.Returning;
            await _repos.BackToWarehouse.AsUpdateable(backInfo).ExecuteCommandAsync();

            // 3. 更新暂存库位状态为"正在入库"
            tempSlot.SlotStatus = ImportApplyConstants.SlotStatus.Importing;
            await _repos.Slot.AsUpdateable(tempSlot).ExecuteCommandAsync();

            // 4. 创建WCS任务
            taskList.Add(new WcsTaskModeOutput
            {
                TaskNo = checkTask.CheckTaskNo,
                TaskMode = ImportApplyConstants.TaskMode.Import,
                StockCode = input.StockCode,
                TaskBegin = backInfo.TargetLocation,    // 从目标位置
                TaskEnd = backInfo.TemporaryLocation    // 到暂存位置
            });
        });

        // 检查事务执行结果
        if (!tranResult.IsSuccess)
        {
            var errorMsg = tranResult.ErrorException?.Message ?? tranResult.ErrorMessage ?? "未知错误";
            throw Oops.Bah($"盘点回库失败：{errorMsg}");
        }

        return taskList;
    }
    #endregion

    #region 私有方法 - 库位分配
    /// <summary>
    /// 分配库位
    /// </summary>
    /// <param name="input">入库申请参数</param>
    /// <param name="warehouse">仓库信息</param>
    /// <param name="materialCode">物料编码</param>
    /// <returns>分配的库位</returns>
    /// <exception cref="Oops">当无可用库位时抛出</exception>
    /// <remarks>
    /// 库位分配逻辑：
    /// 1. 根据物料编码获取物料信息
    /// 2. 根据物料获取对应的存储区域
    /// 3. 如果指定了巷道，在该巷道内分配
    /// 4. 否则在整个区域内分配
    /// 5. 优先分配深度为1的外层库位
    /// </remarks>
    private async Task<WmsBaseSlot> AllocateSlotAsync(ImportApplyInput input, WmsBaseWareHouse warehouse, string materialCode)
    {
        // 1. 获取物料信息
        var material = await GetMaterialAsync(materialCode);
        // 2. 获取物料对应的存储区域ID
        var areaId = await GetMaterialAreaIdAsync(material);
        // 3. 如果指定了巷道编码，在指定巷道内分配库位
        if (!string.IsNullOrWhiteSpace(input.LaneWayCode))
        {
            //获取巷道信息
            var laneway = await GetLanewayAsync(input.LaneWayCode, warehouse);
            await ValidateSlotAvailabilityAsync(warehouse.Id.ToString(), laneway.Id, areaId);
            //获取合适的库位
            return await GetRightSlotAsync(warehouse.Id.ToString(), areaId, laneway.Id, input.Type);
        }
        // 4. 在整个仓库的指定区域内分配库位
        await ValidateSlotAvailabilityAsync(warehouse.Id.ToString(), null, areaId);
        //获取合适的库位
        return await GetRightSlotAsync(warehouse.Id.ToString(), areaId, null, input.Type);
    }
    /// <summary>
    /// 获取物料信息
    /// </summary>
    /// <param name="materialCode">物料编码</param>
    /// <returns>物料实体，如果编码为空则返回null</returns>
    /// <exception cref="Oops">当物料不存在时抛出</exception>
    private async Task<WmsBaseMaterial> GetMaterialAsync(string materialCode)
    {
        if (string.IsNullOrWhiteSpace(materialCode))
            return null;
        // 将物料编码转换为ID并查询
        var materialId = Convert.ToInt64(materialCode);
        var material = await _repos.Material.GetFirstAsync(t => t.Id == materialId) ?? throw Oops.Bah($"物料信息不存在，物料编码：{materialCode}");
        return material;
    }
    /// <summary>
    /// 获取物料对应的区域ID（用于立体库入库）
    /// </summary>
    /// <param name="material">物料实体</param>
    /// <returns>区域ID，如果未配置则返回null</returns>
    /// <remarks>
    /// 查询优先级：
    /// 1. 优先使用物料自身的MaterialAreaId配置
    /// 2. 如果没有配置，则查询同MaterialType的其他物料的配置
    /// 3. 如果都没有配置，返回null（可放入任意区域）
    /// 多区域处理：取第一个区域ID
    /// </remarks>
    private async Task<long?> GetMaterialAreaIdAsync(WmsBaseMaterial material)
    {
        if (material == null || material.Id <= 0)
            return null;

        string? areaIdString = null;

        // 优先级1: 优先使用物料自身的区域配置
        if (!string.IsNullOrWhiteSpace(material.MaterialAreaId))
        {
            areaIdString = material.MaterialAreaId;
        }
        // 优先级2: 如果物料没有配置区域，查询同物料类型的其他物料配置
        else if (material.MaterialType.HasValue)
        {
            var sampleMaterial = await _repos.Material.GetFirstAsync(m =>
                m.MaterialType == material.MaterialType
                && !string.IsNullOrWhiteSpace(m.MaterialAreaId)
                && !m.IsDelete);

            if (sampleMaterial != null)
            {
                areaIdString = sampleMaterial.MaterialAreaId;
            }
        }

        // 如果没有找到任何区域配置，返回null
        if (string.IsNullOrWhiteSpace(areaIdString))
            return null;

        // 解析区域ID（可能配置了多个区域，用逗号分隔，取第一个）
        var areaIds = areaIdString.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return areaIds.Length > 0 && long.TryParse(areaIds[0].Trim(), out var parsedAreaId)
            ? parsedAreaId
            : null;
    }
    /// <summary>
    /// 获取巷道信息
    /// </summary>
    /// <param name="lanewayCode">巷道编码</param>
    /// <param name="warehouse">仓库信息</param>
    /// <returns>巷道实体</returns>
    /// <exception cref="Oops">当巷道不存在或不属于指定仓库时抛出</exception>
    private async Task<WmsBaseLaneway> GetLanewayAsync(string lanewayCode, WmsBaseWareHouse warehouse)
    {
        var laneway = await _repos.Laneway.GetFirstAsync(m => m.LanewayCode == lanewayCode) ?? throw Oops.Bah($"未找到巷道信息，巷道编码：{lanewayCode}");
        if (laneway.WarehouseId?.ToString() != warehouse.Id.ToString())
            throw Oops.Bah($"巷道{lanewayCode}不属于仓库{warehouse.WarehouseName}");
        return laneway;
    }
    /// <summary>
    /// 验证储位可用性
    /// </summary>
    /// <param name="warehouseId">仓库ID</param>
    /// <param name="lanewayId">巷道ID（可选）</param>
    /// <param name="areaId">区域ID（可选）</param>
    /// <exception cref="Oops">当没有可用储位时抛出</exception>
    /// <remarks>
    /// 检查指定条件下是否有可用的空储位。
    /// 如果没有可用储位，不允许入库。
    /// </remarks>
    private async Task ValidateSlotAvailabilityAsync(string warehouseId, long? lanewayId, long? areaId)
    {
        var availableSlots = await CheckAvailableSlotsAsync(warehouseId, lanewayId, areaId);
        if (availableSlots == 0)
        {
            var location = lanewayId.HasValue
                ? $"巷道ID {lanewayId}"
                : areaId.HasValue
                    ? $"区域ID {areaId}"
                    : "仓库";
            throw Oops.Bah($"{location}储位不足，不可入库！");
        }
    }
    /// <summary>
    /// 检查可用储位数量
    /// </summary>
    /// <param name="warehouseId">仓库ID</param>
    /// <param name="lanewayId">巷道ID（可选）</param>
    /// <param name="areaId">区域ID（可选）</param>
    /// <returns>可用储位数量</returns>
    /// <remarks>
    /// 查询条件：
    /// - 储位状态为空
    /// - 储位未关闭
    /// - 储位未被出库锁定
    /// - 储位未被入库锁定
    /// - 巷道状态正常（未禁用）
    /// </remarks>
    private async Task<int> CheckAvailableSlotsAsync(string warehouseId, long? lanewayId, long? areaId)
    {
        var query = BuildBaseSlotQuery(warehouseId);
        // 添加巷道条件
        if (lanewayId.HasValue)
            query = query.Where((s, l) => s.SlotLanewayId == lanewayId.Value);
        // 添加区域条件
        if (areaId.HasValue)
            query = query.Where((s, l) => s.SlotAreaId == areaId.Value);
        return await query.CountAsync();
    }
    /// <summary>
    /// 获取合适的库位
    /// </summary>
    /// <param name="warehouseId">仓库ID</param>
    /// <param name="areaId">区域ID（可选）</param>
    /// <param name="lanewayId">巷道ID（可选）</param>
    /// <param name="slotType">库位类型</param>
    /// <returns>分配的库位</returns>
    /// <exception cref="Oops">当未找到合适库位时抛出</exception>
    /// <remarks>
    /// 库位选择策略：
    /// 1. 优先选择深度为1的外层库位（便于存取）
    /// 2. 按库位编码升序排列（保证库位分配的有序性）
    /// </remarks>
    private async Task<WmsBaseSlot> GetRightSlotAsync(string warehouseId, long? areaId, long? lanewayId, string slotType)
    {
        var query = BuildBaseSlotQuery(warehouseId);
        // 添加巷道条件
        if (lanewayId.HasValue)
            query = query.Where((s, l) => s.SlotLanewayId == lanewayId.Value);
        // 添加区域条件
        if (areaId.HasValue)
            query = query.Where((s, l) => s.SlotAreaId == areaId.Value);
        // 排序：优先深度为1的库位，然后按库位编码升序
        var slot = await query
            .OrderBy((s, l) => s.SlotInout)      // 深度优先（1在前）
            .OrderBy((s, l) => s.SlotCode)        // 库位编码升序
            .Select((s, l) => s)
            .FirstAsync();
        if (slot == null)
        {
            var location = lanewayId.HasValue
                ? $"巷道ID {lanewayId}"
                : areaId.HasValue
                    ? $"区域ID {areaId}"
                    : "仓库";
            throw Oops.Bah($"未找到{location}的合适储位，不可入库！");
        }
        return slot;
    }
    /// <summary>
    /// 构建基础库位查询（包含巷道状态检查）
    /// </summary>
    /// <param name="warehouseId">仓库ID</param>
    /// <returns>库位查询对象</returns>
    /// <remarks>
    /// 基础查询条件：
    /// - 属于指定仓库
    /// - 库位状态为空
    /// - 库位未关闭（SlotCloseFlag = 0）
    /// - 库位未被出库锁定（SlotExlockFlag = 0）
    /// - 库位未被入库锁定（SlotImlockFlag = 0）
    /// - 巷道状态正常（LanewayStatus = false 或 null）
    /// </remarks>
    private ISugarQueryable<WmsBaseSlot, WmsBaseLaneway> BuildBaseSlotQuery(string warehouseId)
    {
        return _repos.Slot.AsQueryable()
            .LeftJoin<WmsBaseLaneway>((s, l) => s.SlotLanewayId == l.Id)
            .Where((s, l) =>
                s.WarehouseId.ToString() == warehouseId &&
                s.SlotStatus == ImportApplyConstants.SlotStatus.Empty &&           // 库位为空
                s.SlotCloseFlag == 0 &&                       // 库位未关闭
                s.SlotExlockFlag == 0 &&                      // 未被出库锁定
                s.SlotImlockFlag == 0 &&                      // 未被入库锁定
                (l.LanewayStatus == false || l.LanewayStatus == null));  // 巷道状态正常
    }
    #endregion

    #region 私有方法 - 任务创建
    /// <summary>
    /// 创建入库任务
    /// </summary>
    /// <param name="input">入库申请参数</param>
    /// <param name="targetSlot">目标库位</param>
    /// <param name="boxList">箱码列表</param>
    /// <param name="sumQty">总数量</param>
    /// <param name="materialCode">物料编码</param>
    /// <param name="order">入库流水（可选）</param>
    /// <param name="abSide">AB面（可选）</param>
    /// <param name="isReturnFlow">是否为回流入库</param>
    /// <returns>任务列表</returns>
    /// <remarks>
    /// 在事务中完成以下操作：
    /// 1. 使用数据库行锁锁定目标库位
    /// 2. 验证库位状态（防止并发冲突）
    /// 3. 创建入库任务记录
    /// 4. 更新箱码和订单状态
    /// 5. 更新库位状态为"正在入库"
    /// 6. 插入二次游标记录（回流入库时）
    /// 7. 查询并设置AB面和巷道信息
    /// 8. 创建WCS任务输出
    /// </remarks>
    private async Task<List<WcsTaskModeOutput>> CreateImportTaskAsync(ImportApplyInput input, WmsBaseSlot targetSlot, List<WmsStockSlotBoxInfo> boxList, decimal sumQty, string materialCode, WmsImportOrder order = null, string abSide = "", bool isReturnFlow = false)
    {
        var taskList = new List<WcsTaskModeOutput>();
        var taskNo = await _sqlViewService.GenerateTaskNo();

        // 在事务中完成所有数据库操作，使用行锁防止并发冲突
        var tranResult = await _sqlSugarClient.Ado.UseTranAsync(async () =>
        {
            // 1. 使用数据库行锁锁定目标库位（防止并发分配）
            // 通过重新查询并锁定行来确保数据一致性
            var lockedSlot = await _repos.Slot.AsQueryable()
                .Where(s => s.SlotCode == targetSlot.SlotCode)
                .FirstAsync() ?? throw new InvalidOperationException($"库位{targetSlot.SlotCode}不存在！");

            // 2. 再次验证库位状态（防止信号量保护期间状态已改变）
            //if (lockedSlot.SlotStatus != ImportApplyConstants.SlotStatus.Empty)
            //{
            //    throw new InvalidOperationException($"库位{lockedSlot.SlotCode}已被占用（状态：{lockedSlot.SlotStatus}），请重新分配！");
            //}

            // 3. 创建入库任务记录
            var importTask = new WmsImportTask
            {
                TaskNo = taskNo,
                Sender = "WCS",
                Receiver = "WMS",
                IsSuccess = 1,
                SendDate = DateTime.Now,
                Message = "申请库位指令",
                StockCode = input.StockCode,
                Status = 0,
                Msg = $"{input.StockCode}获取储位{lockedSlot.SlotCode}",
                EndLocation = lockedSlot.SlotCode
            };
            await _repos.ImportTask.InsertAsync(importTask);

            // 4. 更新箱码和订单状态（如果有）
            if (boxList.Count > 0 && order != null)
            {
                await UpdateBoxAndOrderStatusAsync(boxList, order, lockedSlot, importTask);
            }

            // 5. 更新库位状态为"正在入库"
            lockedSlot.SlotStatus = ImportApplyConstants.SlotStatus.Importing;
            await _repos.Slot.AsUpdateable(lockedSlot).ExecuteCommandAsync();

            // 6. 处理回流入库的二次游标
            // 二次游标用于记录库位的AB面信息，便于后续操作
            // if (isReturnFlow && !string.IsNullOrWhiteSpace(abSide) && lockedSlot.SlotLanewayId.HasValue)
            // {
            //     await InsertSecondCursorAsync(lockedSlot, abSide);
            // }

            // 7. 查询AB面和巷道信息
            // var (finalAbSide, finalLaneWayId) = await GetSlotLocationInfoAsync(lockedSlot.SlotCode, abSide);
            var (finalAbSide, finalLaneWayId) = await GetSlotLocationInfoAsync(lockedSlot.SlotCode, "");

            // 8. 创建WCS任务输出
            var taskMode = new WcsTaskModeOutput
            {
                TaskNo = importTask.TaskNo,
                TaskMode = ImportApplyConstants.TaskMode.Import,
                StockCode = input.StockCode,
                TaskBegin = string.Empty,                    // 入库起点为空
                TaskEnd = lockedSlot.SlotCode,               // 入库终点为目标库位
                GoodsCode = materialCode,
                GoodsQTY = sumQty > 0 ? sumQty : boxList.Sum(a => a.Qty ?? 0),
                // IsAB = finalAbSide,
                IsAB = "",
                LaneWayId = finalLaneWayId
            };
            taskList.Add(taskMode);
        });

        // 检查事务执行结果
        if (!tranResult.IsSuccess)
        {
            var errorMsg = tranResult.ErrorException?.Message ?? tranResult.ErrorMessage ?? "未知错误";
            throw Oops.Bah($"入库任务创建失败：{errorMsg}");
        }

        return taskList;
    }
    /// <summary>
    /// 更新箱码和订单状态
    /// </summary>
    /// <param name="boxList">箱码列表</param>
    /// <param name="order">入库流水</param>
    /// <param name="targetSlot">目标库位</param>
    /// <param name="importTask">入库任务</param>
    /// <remarks>
    /// 更新内容：
    /// - 箱码状态设为1（等待入库）
    /// - 订单执行标志设为"执行中"
    /// - 订单关联库位、区域和任务信息
    /// </remarks>
    private async Task UpdateBoxAndOrderStatusAsync(List<WmsStockSlotBoxInfo> boxList, WmsImportOrder order, WmsBaseSlot targetSlot, WmsImportTask importTask)
    {
        // 更新所有箱码状态为"等待入库"
        boxList.ForEach(box => box.Status = 1);
        await _repos.BoxInfo.AsUpdateable(boxList).ExecuteCommandAsync();
        // 更新入库流水信息
        order.ImportExecuteFlag = ImportApplyConstants.ExecuteFlag.Processing;           // 执行中
        order.ImportSlotCode = targetSlot.SlotCode;                 // 目标库位
        order.ImportAreaId = targetSlot.SlotAreaId ?? 0;           // 目标区域
        order.ImportTaskId = importTask.Id;                         // 关联任务
        await _repos.ImportOrder.AsUpdateable(order).ExecuteCommandAsync();
    }
    /// <summary>
    /// 插入二次游标记录
    /// </summary>
    /// <param name="targetSlot">目标库位</param>
    /// <param name="abSide">AB面</param>
    /// <remarks>
    /// 二次游标用于记录库位的上下层（AB面）信息，
    /// 在回流入库等场景中用于定位库位的具体位置。
    /// </remarks>
    private async Task InsertSecondCursorAsync(WmsBaseSlot targetSlot, string abSide)
    {
        var secondCursor = new WmsImportSecondCursor
        {
            LanewayId = targetSlot.SlotLanewayId.Value,
            // UpOrDown = abSide,                              // 上层或下层
            UpOrDown = "",                              // 上层或下层
            SlotCode = targetSlot.SlotCode
        };
        await _repos.SecondCursor.InsertAsync(secondCursor);
    }
    /// <summary>
    /// 获取库位位置信息（AB面和巷道ID）
    /// </summary>
    /// <param name="slotCode">库位编码</param>
    /// <param name="defaultAbSide">默认AB面</param>
    /// <returns>AB面和巷道ID</returns>
    /// <remarks>
    /// 查询优先级：
    /// 1. 优先从二次游标表查询
    /// 2. 如果没有记录，从库位表查询
    /// </remarks>
    private async Task<(string AbSide, string LaneWayId)> GetSlotLocationInfoAsync(string slotCode, string defaultAbSide)
    {
        // 优先从二次游标表查询
        var secondCursorData = await _repos.SecondCursor.GetFirstAsync(a => a.SlotCode == slotCode);
        if (secondCursorData != null)
        {
            return (
                // secondCursorData.UpOrDown,
                "",
                secondCursorData.LanewayId?.ToString() ?? string.Empty
            );
        }
        // 从库位表查询
        var slot = await _repos.Slot.GetFirstAsync(s => s.SlotCode == slotCode);
        return ("", slot?.SlotLanewayId?.ToString() ?? string.Empty);
    }
    /// <summary>
    /// 创建移库任务
    /// </summary>
    /// <param name="stock">库存托盘</param>
    /// <param name="fromSlot">源库位</param>
    /// <param name="toSlot">目标库位</param>
    /// <param name="billCode">单据编号（可选）</param>
    /// <returns>WCS移库任务</returns>
    /// <remarks>
    /// 移库场景：
    /// 当要将货物放入深度2的库位时，需要先将深度1的货物移走。
    /// 移库任务记录了源库位、目标库位和托盘信息。
    /// </remarks>
    private async Task<WcsTaskModeOutput> CreateMoveTaskAsync(WmsStockTray stock, WmsBaseSlot fromSlot, WmsBaseSlot toSlot, string billCode = "")
    {
        var taskNo = await _sqlViewService.GenerateTaskNo();
        var moveTask = new WmsImportTask
        {
            TaskNo = taskNo,
            Sender = "WMS",
            Receiver = "WMS",
            IsSuccess = 1,
            SendDate = DateTime.Now,
            Message = "入库前置移库操作",
            StockCode = stock.StockCode,
            Msg = $"{billCode}从{fromSlot.SlotCode}移至{toSlot.SlotCode}",
            StartLocation = fromSlot.SlotCode,
            EndLocation = toSlot.SlotCode,
            Status = 0
        };
        await _repos.ImportTask.InsertAsync(moveTask);
        return new WcsTaskModeOutput
        {
            TaskNo = moveTask.TaskNo,
            TaskMode = ImportApplyConstants.TaskMode.Move,               // 移库模式
            StockCode = stock.StockCode,
            TaskBegin = fromSlot.SlotCode,          // 移出库位
            TaskEnd = toSlot.SlotCode,              // 移入库位
            GoodsCode = stock.MaterialId,
            GoodsQTY = stock.StockQuantity ?? 0
        };
    }
    #endregion

    #region 私有方法 - 二次入库
    /// <summary>
    /// 获取二次入库申请的储位地址
    /// </summary>
    /// <param name="input">二次入库申请参数</param>
    /// <returns>新的储位地址，如果获取失败返回null</returns>
    /// <remarks>
    /// 二次入库的两种场景：
    /// 1. 已有入库流水：从二次游标中获取新库位
    /// 2. 无入库流水：使用原库位并创建新流水
    /// </remarks>
    private async Task<string> GetSecondImportSlotCodeAsync(ImportApply2Input input)
    {
        // 根据托盘码查找入库流水
        var importOrder = await FindImportOrderByStockCodeAsync(input.StockCode);
        if (importOrder != null)
        {
            // 场景1：存在入库流水，从二次游标获取新库位
            return await HandleExistingImportOrderAsync(input, importOrder);
        }
        // 场景2：不存在入库流水，使用原库位并创建新流水
        return await HandleMissingImportOrderAsync(input);
    }
    /// <summary>
    /// 根据托盘码查找入库流水
    /// </summary>
    /// <param name="stockCode">托盘码</param>
    /// <returns>入库流水实体，如果不存在返回null</returns>
    /// <remarks>
    /// 通过箱码信息关联查询入库流水。
    /// </remarks>
    private async Task<WmsImportOrder> FindImportOrderByStockCodeAsync(string stockCode)
    {
        return await _sqlSugarClient.Queryable<WmsImportOrder>()
            .LeftJoin<WmsStockSlotBoxInfo>((io, box) => io.Id == box.ImportOrderId)
            .Where((io, box) => box.StockCode == stockCode && !io.IsDelete)
            .Select((io, box) => io)
            .FirstAsync();
    }
    /// <summary>
    /// 处理已存在入库流水的情况
    /// </summary>
    /// <param name="input">二次入库申请参数</param>
    /// <param name="importOrder">入库流水</param>
    /// <returns>新的储位地址，如果获取失败返回null</returns>
    /// <remarks>
    /// 流程：
    /// 1. 验证参数完整性
    /// 2. 从二次游标表查询可用库位
    /// 3. 在事务中删除游标记录并更新流水
    /// </remarks>
    private async Task<string> HandleExistingImportOrderAsync(ImportApply2Input input, WmsImportOrder importOrder)
    {
        // 验证必填参数
        // if (string.IsNullOrWhiteSpace(input.SlotCode) ||
        //     string.IsNullOrWhiteSpace(input.LaneWayId) ||
        //     string.IsNullOrWhiteSpace(input.IsAB))
        if (string.IsNullOrWhiteSpace(input.SlotCode) ||
            string.IsNullOrWhiteSpace(input.LaneWayId))
        {
            // await RecordOperationLogAsync(input.StockCode, "二次入库申请参数验证失败：SlotCode、LaneWayId或IsAB为空", input);
            await RecordOperationLogAsync(input.StockCode, "二次入库申请参数验证失败：SlotCode或LaneWayId为空", input);
            return null;
        }
        // 查询二次游标表中的可用库位
        var lanewayIdLong = long.Parse(input.LaneWayId);
        // var secondCursorList = await _repos.SecondCursor.GetListAsync(a =>
        //     a.LanewayId == lanewayIdLong && a.UpOrDown == input.IsAB);
        var secondCursorList = await _repos.SecondCursor.GetListAsync(a =>
            a.LanewayId == lanewayIdLong);
        if (secondCursorList.Count == 0)
        {
            await RecordOperationLogAsync(input.StockCode,
                // $"二次游标表中未找到可用库位：巷道ID={input.LaneWayId}，AB面={input.IsAB}", input);
                $"二次游标表中未找到可用库位：巷道ID={input.LaneWayId}", input);
            return null;
        }
        // 按创建时间升序，取最早的记录
        var selectedCursor = secondCursorList.OrderBy(a => a.CreateTime).First();
        var newSlotCode = selectedCursor.SlotCode;
        await RecordOperationLogAsync(input.StockCode,
            $"从游标表获取新库位：{newSlotCode}（原库位：{input.SlotCode}）", input);
        // 在事务中更新数据
        await _sqlSugarClient.Ado.UseTranAsync(async () =>
        {
            // 删除已使用的游标记录
            await _repos.SecondCursor.DeleteAsync(selectedCursor);
            // 【修复P0】：更新入库流水的目标库位，使用ID精确定位
            // 旧代码使用 ImportStockCodeId 作为条件，新代码改用 Id
            await _sqlSugarClient.Updateable<WmsImportOrder>()
                .SetColumns(o => o.ImportSlotCode == newSlotCode)
                .Where(o => o.Id == importOrder.Id &&
                           o.ImportSlotCode == input.SlotCode)
                .ExecuteCommandAsync();
        });
        await RecordOperationLogAsync(input.StockCode,
            $"二次入库更新成功：入库流水ID={importOrder.Id}，新库位={newSlotCode}", input);
        return newSlotCode;
    }
    /// <summary>
    /// 处理不存在入库流水的情况
    /// </summary>
    /// <param name="input">二次入库申请参数</param>
    /// <returns>储位地址（使用原库位）</returns>
    /// <exception cref="Oops">当托盘不存在或状态异常时抛出</exception>
    /// <remarks>
    /// 流程：
    /// 1. 查找入库任务记录
    /// 2. 验证托盘有效性
    /// 3. 创建新的入库流水
    /// 4. 返回原库位地址
    /// </remarks>
    private async Task<string> HandleMissingImportOrderAsync(ImportApply2Input input)
    {
        // 查找入库任务记录
        var importTask = await _repos.ImportTask.GetFirstAsync(a =>
            a.StockCode == input.StockCode && a.EndLocation == input.SlotCode);
        if (importTask == null)
        {
            await RecordOperationLogAsync(input.StockCode,
                $"二次入库申请失败：未找到入库任务记录（StockCode={input.StockCode}, SlotCode={input.SlotCode}）", input);
            return null;
        }
        // 验证托盘存在且可用
        var stockTray = await _repos.StockCode.GetFirstAsync(a => a.StockCode == input.StockCode);
        if (stockTray == null)
        {
            await RecordOperationLogAsync(input.StockCode, $"二次入库申请失败：托盘{input.StockCode}不存在或状态异常", input);
            throw Oops.Bah($"托盘{input.StockCode}不存在或状态异常！");
        }
        // 生成新的入库流水号
        var importOrderNo = await _sqlViewService.GenerateImportOrderNo();
        await RecordOperationLogAsync(input.StockCode,
            $"二次入库创建新流水：流水号={importOrderNo}，库位={input.SlotCode}", input);
        // 创建新的入库流水
        var newImportOrder = new WmsImportOrder
        {
            ImportOrderNo = importOrderNo,
            ImportSlotCode = input.SlotCode,
            ImportAreaId = 0,
            StockCodeId = stockTray.Id,
            // 【修复P0】：二次入库补录流水时应设置为"已完成"状态
            // 旧代码使用 ImportExecuteFlag = 2（已完成），对应新常量 "03"
            // 因为二次入库是补录操作，实际入库动作已完成，只是补充流水记录
            ImportExecuteFlag = ImportApplyConstants.ExecuteFlag.Completed,  // "03" = 已完成
            ImportTaskId = importTask.Id,
            StockCode = input.StockCode
        };
        await _repos.ImportOrder.InsertAsync(newImportOrder);
        await RecordOperationLogAsync(input.StockCode,
            $"二次入库创建流水成功：流水ID={newImportOrder.Id}，状态=已完成", input);
        // 返回原库位地址
        return input.SlotCode;
    }
    #endregion

    #region 私有方法 - 辅助方法
    /// <summary>
    /// 根据托盘码获取箱码信息
    /// </summary>
    /// <param name="stockCode">托盘码</param>
    /// <returns>箱码视图列表和箱码实体列表</returns>
    /// <remarks>
    /// 查询未入库或待入库的箱码（Status为null或0）。
    /// 返回的视图列表包含关联的详细信息，实体列表用于后续更新操作。
    /// </remarks>
    private async Task<(List<ViewStockSlotBoxInfoView> BoxViewList, List<WmsStockSlotBoxInfo> BoxList)> GetBoxInfoByStockCodeAsync(string stockCode)
    {
        // 查询箱码视图（包含关联信息）
        var boxViewList = await _sqlViewService.QueryStockSlotBoxInfoView()
            .MergeTable()
            .Where(x => x.StockCode == stockCode &&(x.Status == null || x.Status == 0))  // 未入库或待入库
            .ToListAsync();
        // 如果有箱码，查询实体列表
        var boxIds = boxViewList.Select(x => x.Id).ToList();
        var boxList = boxIds.Count > 0
            ? await _repos.BoxInfo.GetListAsync(m => boxIds.Contains(m.Id))
            : new List<WmsStockSlotBoxInfo>();
        return (boxViewList, boxList);
    }
    /// <summary>
    /// 解析库位结果字符串
    /// </summary>
    /// <param name="slotResult">库位结果字符串（格式：库位编码AB面）</param>
    /// <returns>库位编码和AB面</returns>
    /// <exception cref="Oops">当结果为空或格式错误时抛出</exception>
    /// <remarks>
    /// 库位结果格式：SlotCodeABSide
    /// 例如：A01-01-01A 表示库位A01-01-01的A面
    /// </remarks>
    private static (string SlotCode, string AbSide) ParseSlotResult(string slotResult)
    {
        if (string.IsNullOrWhiteSpace(slotResult) ||
            slotResult == ImportApplyConstants.BusinessConstants.SlotResultSeparator)
            throw Oops.Bah("未找到合适储位，不可入库！");
        var parts = slotResult.Split(ImportApplyConstants.BusinessConstants.SlotResultSeparator);
        return (parts[0], parts.Length > 1 ? parts[1] : string.Empty);
    }
    /// <summary>
    /// 根据库位编码获取库位信息
    /// </summary>
    /// <param name="slotCode">库位编码</param>
    /// <returns>库位实体</returns>
    /// <exception cref="Oops">当库位不存在时抛出</exception>
    private async Task<WmsBaseSlot> GetSlotByCodeAsync(string slotCode)
    {
        var slot = await _repos.Slot.GetFirstAsync(s => s.SlotCode == slotCode) ?? throw Oops.Bah($"库位不存在，库位编码：{slotCode}");
        return slot;
    }
    /// <summary>
    /// 获取外层库位（深度1）
    /// </summary>
    /// <param name="slot">内层库位（深度2）</param>
    /// <returns>外层库位，如果不存在返回null</returns>
    /// <remarks>
    /// 根据内层库位的行、列、层信息查找对应的外层库位。
    /// 外层和内层库位的关系：同一排、同一列、同一层，但深度不同。
    /// </remarks>
    private async Task<WmsBaseSlot> GetOutsideSlotAsync(WmsBaseSlot slot)
    {
        return await _repos.Slot.GetFirstAsync(m =>
            m.SlotInout == ImportApplyConstants.SlotDepth.Outside &&         // 深度为1（外层）
            m.SlotRow == slot.SlotRow &&                // 同一排
            m.SlotColumn == slot.SlotColumn &&          // 同一列
            m.SlotLayer == slot.SlotLayer &&            // 同一层
            m.WarehouseId == slot.WarehouseId &&        // 同一仓库
            m.SlotLanewayId == slot.SlotLanewayId);     // 同一巷道
    }
    /// <summary>
    /// 创建成功响应
    /// </summary>
    /// <param name="taskList">任务列表</param>
    /// <returns>成功响应对象</returns>
    private static ImportApplyOutput CreateSuccessResponse(List<WcsTaskModeOutput> taskList)
    {
        return new ImportApplyOutput
        {
            Code = 1,                           // 成功代码
            Count = taskList?.Count ?? 0,       // 任务数量
            Msg = string.Empty,                 // 成功时消息为空
            Data = taskList ?? new List<WcsTaskModeOutput>()
        };
    }
    /// <summary>
    /// 创建失败响应
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <returns>失败响应对象</returns>
    private static ImportApplyOutput CreateFailureResponse(string message)
    {
        return new ImportApplyOutput
        {
            Code = 0,                           // 失败代码
            Count = 0,                          // 任务数量为0
            Msg = message,                      // 错误消息
            Data = new List<WcsTaskModeOutput>()
        };
    }
    #endregion
}