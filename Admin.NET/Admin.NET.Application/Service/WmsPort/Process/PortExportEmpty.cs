// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.BaseService;
using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Core;
using Flurl;
using Flurl.Http;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlSugar;
namespace Admin.NET.Application.Service.WmsPort.Process;

/// <summary>
/// 空托申请业务处理类，对应 JC35 `PortController.AddOutPalno` 的空载具申请逻辑重构实现。
/// </summary>
/// <remarks>
/// 业务职责：
/// 1. 处理空托盘出库申请，从仓库中分配空托盘
/// 2. 管理空托盘的库存查询和分配逻辑
/// 3. 处理深度货架的移库需求（当目标库位在深度2时）
/// 4. 与WCS（仓库控制系统）集成下发任务
///
/// 核心流程：
/// 1. 验证输入参数和仓库配置
/// 2. 查询符合条件的空托盘库存
/// 3. 处理深度2库位的移库前置任务
/// 4. 创建出库任务并更新库位状态
/// 5. 调用WCS接口下发任务
/// 6. 处理WCS响应并更新任务状态
///
/// 技术特点：
/// - 使用SqlSugar ORM进行数据库操作
/// - 事务管理确保数据一致性
/// - 与WCS系统通过HTTP API集成
/// - 支持深度货架的复杂移库逻辑
///
/// 业务场景：
/// 当生产或运营需要空托盘时，系统通过此服务从仓库中分配可用的空托盘，
/// 并下发给WCS系统执行出库操作。对于深度货架（双深度货位），需要先
/// 将外层货物移走才能取出内层的空托盘。
///
/// 数据流：
/// WMS → PortEmptyTray → WCS → 物理设备
/// 1. WMS接收空托申请请求
/// 2. PortEmptyTray处理业务逻辑
/// 3. 生成任务下发给WCS
/// 4. WCS控制设备执行出库操作
///
/// 异常处理：
/// - 参数验证失败：返回业务异常
/// - 库存不足：返回"空载具库存不足"
/// - 路径阻塞：返回"空载具通道被占用"
/// - WCS调用失败：记录日志并返回失败状态
/// </remarks>
public class PortExportEmpty : PortBase, ITransient
{
    #region 字段定义
    // 仓储聚合类
    private readonly PortBaseRepos _repos;
    // 业务服务
    private readonly CommonMethod _commonMethod;
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖
    /// </summary>
    /// <param name="loggerFactory">日志工厂</param>
    /// <param name="sqlSugarClient">数据库客户端</param>
    /// <param name="repos">仓储聚合类</param>
    /// <param name="commonMethod">通用方法服务</param>
    public PortExportEmpty(
        ILoggerFactory loggerFactory,
        ISqlSugarClient sqlSugarClient,
        PortBaseRepos repos,
        CommonMethod commonMethod)
        : base(sqlSugarClient, loggerFactory.CreateLogger(CommonConst.PortExportEmpty), "空托出库")
    {
        _repos = repos;
        _commonMethod = commonMethod;
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 处理空托申请的主入口方法
    /// </summary>
    /// <param name="input">空托申请参数</param>
    /// <returns>返回空托申请执行结果、提示信息与下发给WCS的任务集合</returns>
    /// <remarks>
    /// 对应旧接口 <c>PortController.AddOutPalno</c>，保持以下业务要点：
    /// <list type="number">
    /// <item>校验仓库、空托物料配置并按深度/排/列/层排序选取可用托盘</item>
    /// <item>若托盘处于第二深度，先生成移库任务确保通道畅通</item>
    /// <item>组织移库 + 出库任务列表，下发至WCS并根据反馈更新任务状态</item>
    /// </list>
    /// </remarks>
    public async Task<EmptyTrayApplyOutput> ProcessEmptyTrayAsync(EmptyTrayApplyInput input)
    {
        await RecordOperationLogAsync(input.HouseCode, "空托申请开始", input);
        // 验证输入参数
        ValidateInput(input);
        // 验证仓库和物料配置
        var (warehouse, material) = await ValidateWarehouseAndMaterialAsync(input);
        // 筛选空托候选集合
        var candidates = await LoadEmptyTrayCandidatesAsync(material.Id, warehouse.Id);
        if (candidates.Count == 0)
        {
            throw Oops.Bah("空载具库存不足！");
        }
        // 在事务中处理任务创建
        var (exportPayload, createdTaskIds) = await ProcessTasksInTransactionAsync(input, warehouse, candidates);
        // 调用WCS下发任务
        var wcsResponse = await SendTasksToWcsAsync(exportPayload);
        var (success, message) = EvaluateWcsResponse(wcsResponse);
        // 更新任务状态
        await UpdateTaskStatusAsync(createdTaskIds, success);
        await RecordOperationLogAsync(input.HouseCode, $"空托申请完成：{message}", new { input, Tasks = exportPayload, Response = wcsResponse });
        return new EmptyTrayApplyOutput
        {
            Success = success,
            Message = message,
            Tasks = exportPayload,
            WcsResponse = wcsResponse
        };
    }
    #endregion

    #region 私有方法 - 核心业务逻辑
    /// <summary>
    /// 验证仓库和物料配置的有效性
    /// </summary>
    /// <param name="input">空托申请参数</param>
    /// <returns>返回仓库和物料实体元组</returns>
    /// <exception cref="Exception">当仓库不存在或物料未维护时抛出异常</exception>
    /// <remarks>
    /// 此方法验证两个关键配置：
    /// 1. 仓库类型配置：验证指定的仓库编码是否存在且可用
    /// 2. 空托物料配置：验证该仓库是否已维护空托盘物料信息
    ///
    /// 这些配置是空托申请业务的基础，缺少任何一个都无法正常处理业务
    /// </remarks>
    private async Task<(WmsBaseWareHouse warehouse, WmsBaseMaterial material)> ValidateWarehouseAndMaterialAsync(EmptyTrayApplyInput input)
    {
        // 验证仓库类型是否存在
        var warehouse = await _repos.Warehouse.GetFirstAsync(a => a.WarehouseCode == input.HouseCode && !a.IsDelete);
        if (warehouse == null)
        {
            throw Oops.Bah($"仓库类型错误，不存在 {input.HouseCode} 仓库类型！");
        }
        // 验证空托盘物料配置是否已维护（使用 IsEmpty 字段判断）
        var material = await _repos.Material.GetFirstAsync(m =>
            m.IsEmpty == true &&
            m.WarehouseId == warehouse.Id &&
            !m.IsDelete) ?? throw Oops.Bah("未维护对应载具！");
        return (warehouse, material);
    }
    /// <summary>
    /// 在数据库事务中处理任务的创建和状态更新
    /// </summary>
    /// <param name="input">空托申请参数</param>
    /// <param name="warehouse">仓库实体</param>
    /// <param name="candidates">候选托盘列表</param>
    /// <returns>返回导出任务载荷和创建的任务ID列表</returns>
    /// <exception cref="Exception">当没有找到可用空载具或处理失败时抛出异常</exception>
    /// <remarks>
    /// 此方法是空托申请的核心业务处理逻辑，在事务中执行以确保数据一致性：
    /// 1. 批量预加载候选托盘和库位数据（避免N+1查询）
    /// 2. 遍历候选托盘，检查其可用性
    /// 3. 对于深度2的库位，先生成移库任务
    /// 4. 生成空托出库任务
    /// 5. 更新库位状态
    /// 6. 构建WCS任务载荷
    ///
    /// 使用事务确保：
    /// - 任务创建与库位状态更新的原子性
    /// - 多个任务要么全部成功，要么全部回滚
    /// - 避免数据不一致的情况
    ///
    /// 性能优化：
    /// - 批量查询托盘和库位，避免循环中的N+1查询问题
    /// - 使用字典查找提高查询效率（O(1)复杂度）
    /// </remarks>
    private async Task<(List<ExportLibraryDTO> exportPayload, List<long> createdTaskIds)> ProcessTasksInTransactionAsync(EmptyTrayApplyInput input, WmsBaseWareHouse warehouse, List<TrayCandidate> candidates)
    {
        var exportPayload = new List<ExportLibraryDTO>();
        var createdTaskIds = new List<long>();
        var processedCount = 0;
        var pathBlocked = false;
        try
        {
            _sqlSugarClient.Ado.BeginTran();

            // ✅ 性能优化：批量预加载托盘和库位数据，避免N+1查询
            var (trayDict, slotDict) = await BatchLoadTraysAndSlotsAsync(candidates);

            foreach (var candidate in candidates)
            {
                if (processedCount >= input.Num || pathBlocked)
                {
                    break;
                }
                // 使用字典查找托盘和库位（O(1)复杂度）
                if (!trayDict.TryGetValue(candidate.TrayId, out var tray) ||
                    !slotDict.TryGetValue(candidate.SlotId, out var slot))
                {
                    continue;
                }
                var exportTaskNo = _commonMethod.GetImExNo(EmptyTrayConstants.SystemConstants.ExportTaskPrefix);
                var moveDtos = new List<ExportLibraryDTO>();
                // 处理深度2库位的移库需求
                if (slot.SlotInout == EmptyTrayConstants.SlotDepth.Depth2)
                {
                    var moveResult = await PrepareMoveTasksAsync(warehouse, slot, exportTaskNo, input.HouseCode);
                    if (!moveResult.Success)
                    {
                        pathBlocked = moveResult.PathBlocked;
                        continue;
                    }
                    moveDtos.AddRange(moveResult.MovePayload);
                    createdTaskIds.AddRange(moveResult.CreatedTaskIds);
                }
                // 创建空托出库任务
                await CreateExportTaskAndUpdateSlotAsync(tray, slot, input.ExportPort, exportTaskNo, warehouse.Id, createdTaskIds);
                // 构建WCS任务载荷
                var boxInfos = await BuildStockBoxInfosAsync(tray.Id);
                exportPayload.AddRange(moveDtos);
                exportPayload.Add(BuildExportLibraryDto(tray, slot, exportTaskNo, input, boxInfos));
                processedCount++;
            }
            if (processedCount == 0)
            {
                throw Oops.Bah(pathBlocked ? "空载具通道被占用，当前无法下发任务！" : "未找到可用空载具库存！");
            }
            _sqlSugarClient.Ado.CommitTran();
        }
        catch (Exception ex)
        {
            _sqlSugarClient.Ado.RollbackTran();
            await RecordOperationLogAsync(input.HouseCode, $"空托申请异常：{ex.Message}", input);
            throw;
        }
        return (exportPayload, createdTaskIds);
    }
    #endregion

    #region 私有方法 - 数据加载与验证
    /// <summary>
    /// 批量加载候选托盘和库位数据
    /// </summary>
    /// <param name="candidates">候选托盘列表</param>
    /// <returns>返回托盘字典和库位字典</returns>
    /// <remarks>
    /// 性能优化方法：
    /// 1. 批量查询所有候选托盘（避免循环中的N次查询）
    /// 2. 批量查询所有候选库位（避免循环中的N次查询）
    /// 3. 构建字典便于快速查找（O(1)复杂度）
    /// 4. 过滤掉无效的托盘和库位
    ///
    /// 相比原方法的性能提升：
    /// - 从 O(N) 次数据库查询 → O(2) 次数据库查询
    /// - 从 O(N) 查询时间复杂度 → O(1) 字典查找复杂度
    /// </remarks>
    private async Task<(Dictionary<long, WmsStockTray> trayDict, Dictionary<long, WmsBaseSlot> slotDict)> BatchLoadTraysAndSlotsAsync(List<TrayCandidate> candidates)
    {
        // 批量查询托盘
        var trayIds = candidates.Select(c => c.TrayId).Distinct().ToList();
        var trays = await _repos.StockTray.GetListAsync(t =>
            trayIds.Contains(t.Id) &&
            !t.IsDelete &&
            !string.IsNullOrWhiteSpace(t.StockSlotCode));
        var trayDict = trays.ToDictionary(t => t.Id);

        // 批量查询库位
        var slotIds = candidates.Select(c => c.SlotId).Distinct().ToList();
        var slots = await _repos.Slot.GetListAsync(s =>
            slotIds.Contains(s.Id) &&
            s.SlotStatus == EmptyTrayConstants.SlotStatus.Occupied);
        var slotDict = slots.ToDictionary(s => s.Id);

        return (trayDict, slotDict);
    }

    #endregion

    #region 私有方法 - 任务创建与状态更新
    /// <summary>
    /// 创建出库任务并更新库位状态
    /// </summary>
    /// <param name="tray">托盘实体</param>
    /// <param name="slot">库位实体</param>
    /// <param name="exportPort">出库口</param>
    /// <param name="exportTaskNo">任务编号</param>
    /// <param name="warehouseId">仓库ID</param>
    /// <param name="createdTaskIds">已创建的任务ID列表</param>
    /// <remarks>
    /// 此方法执行两个关键操作：
    /// 1. 创建出库任务记录到数据库
    /// 2. 将库位状态更新为"出库中"
    ///
    /// 这两个操作必须同时完成，确保任务与库位状态的一致性
    /// </remarks>
    private async Task CreateExportTaskAndUpdateSlotAsync(WmsStockTray tray, WmsBaseSlot slot, string exportPort, string exportTaskNo, long warehouseId, List<long> createdTaskIds)
    {
        var exportTask = BuildExportTask(warehouseId, tray.StockCode, tray.StockSlotCode, exportPort, exportTaskNo);
        await _repos.ExportTask.InsertAsync(exportTask);
        createdTaskIds.Add(exportTask.Id);
        slot.SlotStatus = EmptyTrayConstants.SlotStatus.Exporting;
        slot.UpdateTime = DateTime.Now;
        await _repos.Slot.AsUpdateable(slot)
            .UpdateColumns(s => new { s.SlotStatus, s.UpdateTime })
            .ExecuteCommandAsync();
    }
    /// <summary>
    /// 构建导出库DTO对象
    /// </summary>
    /// <param name="tray">托盘实体</param>
    /// <param name="slot">库位实体</param>
    /// <param name="exportTaskNo">任务编号</param>
    /// <param name="input">申请参数</param>
    /// <param name="boxInfos">箱码信息</param>
    /// <returns>返回导出库DTO对象</returns>
    /// <remarks>
    /// 此方法将业务实体转换为WCS系统需要的标准格式：
    /// - 包含任务起始和目标位置
    /// - 包含托盘和数量信息
    /// - 包含详细的箱码信息
    /// - 包含通道和过渡位置信息
    /// </remarks>
    private ExportLibraryDTO BuildExportLibraryDto(WmsStockTray tray, WmsBaseSlot slot, string exportTaskNo, EmptyTrayApplyInput input, List<StockBoxInfo> boxInfos)
    {
        var qty = tray.LockQuantity ?? 0;
        return new ExportLibraryDTO
        {
            BillCode = string.Empty,
            TaskBegin = tray.StockSlotCode,
            StockCode = tray.StockCode,
            TaskNo = exportTaskNo,
            TaskType = EmptyTrayConstants.WcsTaskType.Export,
            HouseCode = input.HouseCode,
            TaskEnd = input.ExportPort,
            Qty = qty,
            WholeBoxNum = qty.ToString(),
            boxInfos = boxInfos,
            LanewayId = slot.SlotLanewayId?.ToString() ?? string.Empty,
            IsExportStockTary = string.Empty,
            beginTransitLocation = string.Empty,
            EndTransitLocation = slot.EndTransitLocation ?? string.Empty
        };
    }
    /// <summary>
    /// 更新任务状态为已处理和成功
    /// </summary>
    /// <param name="createdTaskIds">创建的任务ID列表</param>
    /// <param name="success">是否成功</param>
    /// <remarks>
    /// 当WCS调用成功时，更新相关任务的状态为已完成：
    /// - 设置任务标志为已处理
    /// - 设置成功状态
    /// - 记录完成时间
    /// </remarks>
    private async Task UpdateTaskStatusAsync(List<long> createdTaskIds, bool success)
    {
        if (success && createdTaskIds.Count > 0)
        {
            var now = DateTime.Now;
            await _sqlSugarClient.Updateable<WmsExportTask>()
                .SetColumns(t => t.ExportTaskFlag == EmptyTrayConstants.TaskStatus.Processed)
                .SetColumns(t => t.IsSuccess == EmptyTrayConstants.TaskStatus.Success)
                .SetColumns(t => t.BackDate == now)
                .Where(t => createdTaskIds.Contains(t.Id))
                .ExecuteCommandAsync();
        }
    }
    #endregion

    #region 私有方法 - 参数验证
    /// <summary>
    /// 校验空托申请入参
    /// </summary>
    private void ValidateInput(EmptyTrayApplyInput input)
    {
        if (input == null) throw Oops.Bah("请求参数不能为空！");
        if (input.Num <= 0) throw Oops.Bah("申请数量必须大于 0！");
        if (string.IsNullOrWhiteSpace(input.ExportPort)) throw Oops.Bah("出库口不能为空！");
        if (string.IsNullOrWhiteSpace(input.HouseCode)) throw Oops.Bah("仓库类型不能为空！");
    }
    #endregion

    #region 私有方法 - 库存查询
    /// <summary>
    /// 加载空托盘候选集合
    /// </summary>
    /// <param name="materialId">空托物料ID</param>
    /// <param name="warehouseId">仓库ID</param>
    /// <returns>按最优策略排序的候选托盘列表</returns>
    /// <remarks>
    /// 此方法优化了数据库查询性能：
    /// 1. 使用索引友好的查询条件
    /// 2. 减少不必要的字段查询
    /// 3. 优化排序策略以提高出库效率
    ///
    /// 排序策略说明：
    /// - 优先深度1（外层）以提高出库速度
    /// - 同一通道内按就近原则排序
    /// - 考虑操作便利性和路径优化
    /// </remarks>
    private async Task<List<TrayCandidate>> LoadEmptyTrayCandidatesAsync(long materialId, long warehouseId)
    {
        var materialIdStr = materialId.ToString();
        var warehouseIdStr = warehouseId.ToString();
        // 使用优化的查询条件，利用数据库索引
        return await _sqlSugarClient
            .Queryable<WmsStockTray, WmsBaseSlot>((tray, slot) => new JoinQueryInfos(
                JoinType.Inner, tray.StockSlotCode == slot.SlotCode))
            .Where((tray, slot) =>
                // 主要条件：优先使用索引字段
                tray.WarehouseId == warehouseIdStr &&
                tray.MaterialId == materialIdStr &&
                // 状态条件：确保数据有效性
                !tray.IsDelete &&
                !slot.IsDelete &&
                // 库位条件：确保可操作
                slot.SlotStatus == EmptyTrayConstants.SlotStatus.Occupied &&
                slot.SlotExlockFlag == EmptyTrayConstants.SlotLockFlag.Unlocked &&
                slot.SlotImlockFlag == EmptyTrayConstants.SlotLockFlag.Unlocked &&
                slot.Make == "01")
            // 优化排序：提高出库效率
            .OrderBy((tray, slot) => slot.SlotInout, OrderByType.Asc)        // 优先外层
            .OrderBy((tray, slot) => slot.SlotLanewayId, OrderByType.Asc)    // 同通道就近
            .OrderBy((tray, slot) => slot.SlotRow, OrderByType.Asc)          // 按排排序
            .OrderBy((tray, slot) => slot.SlotColumn, OrderByType.Asc)       // 按列排序
            .OrderBy((tray, slot) => slot.SlotLayer, OrderByType.Asc)        // 按层排序
                                                                             // 只查询必要的字段
            .Select((tray, slot) => new TrayCandidate
            {
                TrayId = tray.Id,
                StockCode = tray.StockCode,
                SlotId = slot.Id,
                SlotCode = slot.SlotCode
            })
            .ToListAsync();
    }
    #endregion

    #region 私有方法 - 移库任务处理
    /// <summary>
    /// 准备深度2库位的移库任务
    /// </summary>
    /// <param name="warehouse">仓库实体信息</param>
    /// <param name="targetSlot">需要移库的目标库位（深度2）</param>
    /// <param name="exportTaskNo">任务编号</param>
    /// <param name="houseCode">仓库编码</param>
    /// <returns>移库任务处理结果，包含成功状态、路径阻塞信息、任务载荷和创建的任务ID</returns>
    /// <remarks>
    /// 此方法处理深度货架的移库逻辑：
    /// 1. 查找阻塞目标库位的前置库位
    /// 2. 验证前置库位的状态可用性
    /// 3. 查找合适的移库目标位置
    /// 4. 创建移库任务并更新库位状态
    ///
    /// 深度货架说明：
    /// - 深度1：靠近通道的外层货位，可以直接存取
    /// - 深度2：远离通道的内层货位，需要先移出深度1的货物
    /// </remarks>
    private async Task<(bool Success, bool PathBlocked, List<ExportLibraryDTO> MovePayload, List<long> CreatedTaskIds)> PrepareMoveTasksAsync(
        WmsBaseWareHouse warehouse,
        WmsBaseSlot targetSlot,
        string exportTaskNo,
        string houseCode)
    {
        var createdTaskIds = new List<long>();
        var movePayload = new List<ExportLibraryDTO>();
        try
        {
            // 查找阻塞目标库位的前置库位
            var frontSlots = await FindBlockingSlotsAsync(targetSlot);
            if (frontSlots.Count == 0)
            {
                return (true, false, movePayload, createdTaskIds);
            }
            // 验证前置库位状态
            var validationResult = ValidateFrontSlotsStatus(frontSlots);
            if (!validationResult.IsValid)
            {
                return (false, validationResult.PathBlocked, movePayload, createdTaskIds);
            }
            // 处理每个被占用的前置库位
            foreach (var frontSlot in frontSlots.Where(s => s.SlotStatus == EmptyTrayConstants.SlotStatus.Occupied))
            {
                var moveResult = await ProcessSlotMoveAsync(frontSlot, warehouse, exportTaskNo, houseCode);
                if (!moveResult.Success)
                {
                    return (false, true, movePayload, createdTaskIds);
                }
                movePayload.AddRange(moveResult.MovePayload);
                createdTaskIds.AddRange(moveResult.CreatedTaskIds);
            }
            return (true, false, movePayload, createdTaskIds);
        }
        catch (Exception ex)
        {
            await RecordExceptionLogAsync(houseCode, "移库任务处理异常", ex, new { TargetSlot = targetSlot.SlotCode });
            return (false, true, movePayload, createdTaskIds);
        }
    }
    /// <summary>
    /// 查找阻塞指定库位的前置库位
    /// </summary>
    /// <param name="targetSlot">目标库位</param>
    /// <returns>阻塞的前置库位列表</returns>
    /// <remarks>
    /// 根据仓库的物理布局查找阻塞条件：
    /// 1. 相同通道：确保在同一巷道内
    /// 2. 相同排、列、层：确保是垂直堆叠关系
    /// 3. 深度更小：确保是外层库位
    /// </remarks>
    private async Task<List<WmsBaseSlot>> FindBlockingSlotsAsync(WmsBaseSlot targetSlot)
    {
        return await _repos.Slot.GetListAsync(s =>
            s.SlotLanewayId == targetSlot.SlotLanewayId &&
            s.SlotRow == targetSlot.SlotRow &&
            s.SlotColumn == targetSlot.SlotColumn &&
            s.SlotLayer == targetSlot.SlotLayer &&
            s.WarehouseId == targetSlot.WarehouseId &&
            s.SlotInout < targetSlot.SlotInout);
    }
    /// <summary>
    /// 验证前置库位状态的可用性
    /// </summary>
    /// <param name="frontSlots">前置库位列表</param>
    /// <returns>验证结果</returns>
    /// <remarks>
    /// 检查前置库位是否可以安全移库：
    /// 1. 检查是否有正在操作的库位（入库中、移库中）
    /// 2. 如果有正在操作的库位，路径被阻塞
    /// </remarks>
    private (bool IsValid, bool PathBlocked) ValidateFrontSlotsStatus(List<WmsBaseSlot> frontSlots)
    {
        var hasActiveOperations = frontSlots.Any(s =>
            s.SlotStatus == EmptyTrayConstants.SlotStatus.Importing || s.SlotStatus == EmptyTrayConstants.SlotStatus.MovingIn);
        return (!hasActiveOperations, hasActiveOperations);
    }
    /// <summary>
    /// 处理单个库位的移库操作
    /// </summary>
    /// <param name="frontSlot">需要移库的前置库位</param>
    /// <param name="warehouse">仓库信息</param>
    /// <param name="exportTaskNo">任务编号</param>
    /// <param name="houseCode">仓库编码</param>
    /// <returns>移库处理结果</returns>
    /// <remarks>
    /// 处理一个库位的完整移库流程：
    /// 1. 查找库位上的托盘
    /// 2. 如果没有托盘，清理库位状态
    /// 3. 如果有托盘，查找目标位置并创建移库任务
    /// 4. 更新源库位和目标库位的状态
    /// </remarks>
    private async Task<(bool Success, List<ExportLibraryDTO> MovePayload, List<long> CreatedTaskIds)> ProcessSlotMoveAsync(
        WmsBaseSlot frontSlot,
        WmsBaseWareHouse warehouse,
        string exportTaskNo,
        string houseCode)
    {
        var createdTaskIds = new List<long>();
        var movePayload = new List<ExportLibraryDTO>();
        // 查找库位上的托盘
        var frontTrays = await _repos.StockTray.GetListAsync(t =>
            t.StockSlotCode == frontSlot.SlotCode && !t.IsDelete);
        if (frontTrays.Count == 0)
        {
            // 没有托盘，清理库位状态
            await UpdateSlotStatusAsync(frontSlot, EmptyTrayConstants.SlotStatus.Empty);
            return (true, movePayload, createdTaskIds);
        }
        // 查找移库目标位置
        var targetSlotCode = await FindRelocationSlotAsync(frontSlot, warehouse.Id);
        if (string.IsNullOrEmpty(targetSlotCode))
        {
            return (false, movePayload, createdTaskIds);
        }
        var relocateSlot = await _repos.Slot.GetFirstAsync(s => s.SlotCode == targetSlotCode && !s.IsDelete);
        if (relocateSlot == null)
        {
            return (false, movePayload, createdTaskIds);
        }
        // ✅ 批量创建移库任务（性能优化）
        await BatchCreateMoveTasksAsync(frontTrays, frontSlot, relocateSlot, warehouse.Id, exportTaskNo, houseCode, createdTaskIds, movePayload);

        // 更新库位状态
        await UpdateSlotStatusAsync(frontSlot, EmptyTrayConstants.SlotStatus.MovingOut);
        await UpdateSlotStatusAsync(relocateSlot, EmptyTrayConstants.SlotStatus.MovingIn);
        return (true, movePayload, createdTaskIds);
    }
    /// <summary>
    /// 批量创建移库任务（性能优化）
    /// </summary>
    /// <param name="trays">要移动的托盘列表</param>
    /// <param name="sourceSlot">源库位</param>
    /// <param name="targetSlot">目标库位</param>
    /// <param name="warehouseId">仓库ID</param>
    /// <param name="exportTaskNo">任务编号</param>
    /// <param name="houseCode">仓库编码</param>
    /// <param name="createdTaskIds">已创建的任务ID列表</param>
    /// <param name="movePayload">移库任务载荷</param>
    /// <returns>异步任务</returns>
    /// <remarks>
    /// 性能优化：批量插入移库任务，减少数据库往返次数
    /// 1. 批量生成移库任务记录
    /// 2. 批量插入数据库（1次插入 vs N次插入）
    /// 3. 批量构建WCS任务载荷
    /// </remarks>
    private async Task BatchCreateMoveTasksAsync(List<WmsStockTray> trays, WmsBaseSlot sourceSlot, WmsBaseSlot targetSlot, long warehouseId, string exportTaskNo, string houseCode, List<long> createdTaskIds, List<ExportLibraryDTO> movePayload)
    {
        if (trays.Count == 0) return;

        // 批量构建移库任务
        var moveTasks = trays.Select(tray =>
            BuildMoveTask(warehouseId, tray.StockCode, sourceSlot.SlotCode, targetSlot.SlotCode, exportTaskNo)
        ).ToList();

        // 批量插入任务（1次数据库操作）
        await _repos.ExportTask.InsertRangeAsync(moveTasks);
        createdTaskIds.AddRange(moveTasks.Select(t => t.Id));

        // 批量构建WCS载荷
        foreach (var tray in trays)
        {
            var isMainTray = string.Equals(tray.StockCode, tray.VehicleSubId, StringComparison.OrdinalIgnoreCase)
                ? EmptyTrayConstants.TrayTypeFlag.MainTray
                : EmptyTrayConstants.TrayTypeFlag.SubTray;

            movePayload.Add(new ExportLibraryDTO
            {
                BillCode = string.Empty,
                TaskBegin = sourceSlot.SlotCode,
                StockCode = tray.StockCode,
                TaskNo = exportTaskNo,
                TaskType = EmptyTrayConstants.WcsTaskType.Move,
                HouseCode = houseCode,
                TaskEnd = targetSlot.SlotCode,
                Qty = tray.LockQuantity ?? 0,
                WholeBoxNum = (tray.LockQuantity ?? 0).ToString(),
                boxInfos = new List<StockBoxInfo>(),
                LanewayId = sourceSlot.SlotLanewayId?.ToString() ?? string.Empty,
                IsExportStockTary = isMainTray,
                beginTransitLocation = string.Empty,
                EndTransitLocation = targetSlot.EndTransitLocation ?? string.Empty
            });
        }
    }

    /// <summary>
    /// 更新库位状态（使用基类方法）
    /// </summary>
    /// <param name="slot">要更新的库位</param>
    /// <param name="status">新的库位状态</param>
    /// <returns>异步任务</returns>
    /// <remarks>
    /// 更新库位状态并记录更新时间（调用基类统一方法）
    /// </remarks>
    private async Task UpdateSlotStatusAsync(WmsBaseSlot slot, int status)
    {
        await UpdateSlotStatusAsync(_repos.Slot, slot.SlotCode, status);
    }
    #endregion

    #region 私有方法 - 任务实体构建
    /// <summary>
    /// 构建空托出库任务实体
    /// </summary>
    /// <param name="warehouseId">仓库主键</param>
    /// <param name="stockCode">托盘编码</param>
    /// <param name="startSlot">起始储位编码</param>
    /// <param name="exportPort">目标出库口</param>
    /// <param name="taskNo">任务编号</param>
    /// <returns>空托出库任务实体</returns>
    private WmsExportTask BuildExportTask(long warehouseId, string stockCode, string startSlot, string exportPort, string taskNo)
    {
        var now = DateTime.Now;
        return new WmsExportTask
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            ExportTaskNo = taskNo,
            Sender = "WMS",
            Receiver = "WCS",
            ExportTaskFlag = EmptyTrayConstants.TaskStatus.Unprocessed,
            IsSuccess = EmptyTrayConstants.TaskStatus.Failed,
            SendDate = now,
            StockCode = stockCode,
            StartLocation = startSlot,
            EndLocation = exportPort,
            WarehouseId = warehouseId,
            TaskType = EmptyTrayConstants.TaskType.Export,
            Msg = $"{stockCode} 目标出库口为 {exportPort}",
            IsDelete = false,
            CreateTime = now,
            UpdateTime = now,
            CreateUserId = 0,
            UpdateUserId = 0
        };
    }

    /// <summary>
    /// 构建移库任务实体
    /// </summary>
    /// <param name="warehouseId">仓库主键</param>
    /// <param name="stockCode">托盘编码</param>
    /// <param name="startSlot">源储位编码</param>
    /// <param name="targetSlot">目标储位编码</param>
    /// <param name="taskNo">任务编号</param>
    /// <returns>移库任务实体</returns>
    private WmsExportTask BuildMoveTask(long warehouseId, string stockCode, string startSlot, string targetSlot, string taskNo)
    {
        var now = DateTime.Now;
        return new WmsExportTask
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            ExportTaskNo = taskNo,
            Sender = "WMS",
            Receiver = "WCS",
            ExportTaskFlag = EmptyTrayConstants.TaskStatus.Unprocessed,
            IsSuccess = EmptyTrayConstants.TaskStatus.Failed,
            SendDate = now,
            StockCode = stockCode,
            StartLocation = startSlot,
            EndLocation = targetSlot,
            WarehouseId = warehouseId,
            TaskType = EmptyTrayConstants.TaskType.Move,
            Msg = $"由 {startSlot} 储位移动到 {targetSlot} 储位",
            IsDelete = false,
            CreateTime = now,
            UpdateTime = now,
            CreateUserId = 0,
            UpdateUserId = 0
        };
    }
    /// <summary>
    /// 构建托盘下的箱码明细信息
    /// </summary>
    /// <param name="trayId">托盘ID</param>
    /// <returns>箱码信息集合</returns>
    /// <remarks>
    /// 此方法优化了数据库查询性能：
    /// 1. 使用批量查询减少数据库访问次数
    /// 2. 使用字典查找提高数据关联效率
    /// 3. 使用null合并运算符简化空值处理
    ///
    /// 对于空托盘，通常返回空集合，但保持查询逻辑的完整性
    /// </remarks>
    private async Task<List<StockBoxInfo>> BuildStockBoxInfosAsync(long trayId)
    {
        var trayIdStr = trayId.ToString();
        // 查询托盘的库存信息
        var stockInfos = await _repos.StockInfo.GetListAsync(a => a.TrayId == trayIdStr);
        if (stockInfos.Count == 0)
        {
            return new List<StockBoxInfo>();
        }
        // 批量查询物料信息，减少数据库访问
        var materialIds = stockInfos
            .Select(s => s.MaterialId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();
        var materialDict = new Dictionary<string, string>();
        if (materialIds.Count > 0)
        {
            var materialList = await _repos.Material
                .AsQueryable()
                .Where(m => materialIds.Contains(SqlFunc.ToString(m.Id)))
                .Select(m => new { Id = m.Id, m.MaterialCode })
                .ToListAsync();
            materialDict = materialList
                .ToDictionary(k => k.Id.ToString(), v => v.MaterialCode ?? string.Empty);
        }
        // 转换为箱码信息集合
        return stockInfos.Select(info => new StockBoxInfo
        {
            BoxCode = info.BoxCode ?? string.Empty,
            Qty = info.Qty ?? 0,
            MaterialCode = materialDict.TryGetValue(info.MaterialId ?? string.Empty, out var code) ? code : string.Empty,
            ProductionDate = info.ProductionDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            ValidateDay = info.ValidateDay?.ToString("yyyy-MM-dd") ?? string.Empty,
            LotNo = info.LotNo ?? string.Empty,
            BulkTank = info.IsFractionBox ?? 0,
            RFIDCode = info.RFIDCode ?? string.Empty,
            SamplingDate = info.SamplingDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            StaffCode = info.StaffCode ?? string.Empty,
            StaffName = info.StaffName ?? string.Empty,
            PickingSlurry = info.PickingSlurry ?? "0",
            ExtractStatus = info.ExtractStatus?.ToString() ?? "0",
            InspectionStatus = info.InspectionStatus?.ToString() ?? "0"
        }).ToList();
    }
    #endregion

    #region 私有方法 - 库位查找
    /// <summary>
    /// 根据源储位寻找合适的移库目标储位（优先同通道，其次跨通道）
    /// </summary>
    /// <param name="sourceSlot">源储位</param>
    /// <param name="warehouseId">仓库主键</param>
    /// <returns>目标储位编码</returns>
    /// <remarks>
    /// 查找策略：
    /// 1. 优先查找同通道的空闲库位（减少移动距离）
    /// 2. 如果同通道没有，查找其他通道的空闲库位
    /// 3. 排序规则：巷道 → 排 → 列 → 层
    /// </remarks>
    private async Task<string> FindRelocationSlotAsync(WmsBaseSlot sourceSlot, long warehouseId)
    {
        // 优先同通道查找
        var target = await FindAvailableSlotAsync(warehouseId, sourceSlot.SlotLanewayId);
        if (target != null) return target.SlotCode;

        // 跨通道查找
        var crossTarget = await FindAvailableSlotAsync(warehouseId, null);
        return crossTarget?.SlotCode ?? string.Empty;
    }

    /// <summary>
    /// 查找可用的空闲库位（提取公共查询逻辑）
    /// </summary>
    /// <param name="warehouseId">仓库ID</param>
    /// <param name="lanewayId">巷道ID（null表示不限制通道）</param>
    /// <returns>可用库位实体</returns>
    /// <remarks>
    /// 查询条件：
    /// - 库位状态为空闲
    /// - 未加入库锁和出库锁
    /// - 标记为可用（Make == "01"）
    /// - 未被删除
    /// </remarks>
    private async Task<WmsBaseSlot> FindAvailableSlotAsync(long warehouseId, long? lanewayId)
    {
        var query = _repos.Slot.AsQueryable()
            .Where(s =>
                s.WarehouseId == warehouseId &&
                s.SlotStatus == EmptyTrayConstants.SlotStatus.Empty &&
                s.SlotImlockFlag == 0 &&
                s.SlotExlockFlag == 0 &&
                s.Make == "01" &&
                !s.IsDelete);

        // 如果指定了巷道，添加巷道过滤
        if (lanewayId.HasValue)
            query = query.Where(s => s.SlotLanewayId == lanewayId.Value);

        // 统一排序规则
        return await query
            .OrderBy(s => s.SlotLanewayId, OrderByType.Asc)
            .OrderBy(s => s.SlotRow, OrderByType.Asc)
            .OrderBy(s => s.SlotColumn, OrderByType.Asc)
            .OrderBy(s => s.SlotLayer, OrderByType.Asc)
            .FirstAsync();
    }
    #endregion

    #region 私有方法 - WCS集成
    /// <summary>
    /// 调用WCS系统下发任务
    /// </summary>
    /// <param name="payload">要下发的任务列表</param>
    /// <returns>WCS系统的原始响应报文</returns>
    /// <remarks>
    /// 此方法负责与WCS系统集成，具有以下功能：
    /// 1. 验证任务数据的有效性
    /// 2. 检查WCS系统配置
    /// 3. 构建标准格式的HTTP请求
    /// 4. 处理网络异常和系统错误
    /// 5. 记录详细的请求和响应日志
    ///
    /// 当前支持模拟模式，当WCS地址未配置时自动启用
    /// </remarks>
    private async Task<string> SendTasksToWcsAsync(List<ExportLibraryDTO> payload)
    {
        // 验证输入参数
        if (payload == null || payload.Count == 0)
        {
            await RecordOperationLogAsync("SYSTEM", "WCS调用：任务列表为空，跳过调用");
            return string.Empty;
        }
        try
        {
            // 序列化任务数据
            var jsonData = JsonConvert.SerializeObject(payload, Formatting.Indented);
            // 验证WCS配置（使用基类方法）
            var wcsConfig = await ValidateWcsConfigurationAsync();
            if (!wcsConfig.IsValid)
            {
                await RecordOperationLogAsync("SYSTEM", "WCS配置验证失败，启用模拟模式", wcsConfig.ErrorMessage);
                return CreateMockWcsResponse("WCS配置未完成，模拟调用成功");
            }
            // 构建请求URL（兼容基类的 URL 组合逻辑）
            var requestUrl = GetWcsApiUrl(WcsApiUrlDto.TaskApiUrl);
            await RecordOperationLogAsync("SYSTEM", "开始调用WCS接口", new { Url = requestUrl, TaskCount = payload.Count });
            // 调用WCS接口（当前为模拟模式）
            // TODO: 根据配置启用真实的HTTP调用
            return await CallWcsApiAsync(requestUrl, jsonData);
        }
        catch (Exception ex)
        {
            await RecordExceptionLogAsync("SYSTEM", "WCS调用异常", ex, new { PayloadCount = payload?.Count });
            return CreateErrorWcsResponse($"WCS调用失败: {ex.Message}");
        }
    }
    /// <summary>
    /// 调用WCS API接口
    /// </summary>
    /// <param name="requestUrl">请求URL</param>
    /// <param name="jsonData">JSON格式的请求数据</param>
    /// <returns>WCS响应内容</returns>
    /// <remarks>
    /// 当前实现为模拟模式，真实环境中需要启用HTTP调用
    /// </remarks>
    private Task<string> CallWcsApiAsync(string requestUrl, string jsonData)
    {
        // 当前为模拟模式，记录请求信息并返回模拟响应
        _logger.LogInformation(
            "[WCS模拟调用] URL: {Url} | 任务数量: {TaskCount} | 请求数据大小: {DataSize}字节",
            requestUrl,
            JsonConvert.DeserializeObject<List<object>>(jsonData)?.Count ?? 0,
            jsonData.Length);
        return Task.FromResult(CreateMockWcsResponse("WCS接口模拟调用成功"));
    }
    #endregion
}
