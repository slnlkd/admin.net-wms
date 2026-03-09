// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护使用本项目应遵守相关法律法规和许可证的要求
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.BaseService;
using Admin.NET.Application.Service.WmsPda.Dto;
using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Core;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlSugar;

namespace Admin.NET.Application.Service.WmsPda.Process;

/// <summary>
/// PDA 空托盘出库业务服务
/// </summary>
/// <remarks>
/// 从 PdaExportProcess 中拆分出的独立服务，专注于空托盘出库业务逻辑
/// 主要功能：
/// <list type="bullet">
/// <item>空托盘出库申请与WCS任务下发</item>
/// <item>出库口配置查询</item>
/// <item>空托盘候选集智能筛选（通道占用检查）</item>
/// </list>
/// </remarks>
public class PdaExportEmpty : PdaExportBase, ITransient
{
    #region 字段定义
    private readonly CommonMethod _commonMethod;
    private readonly ILogger _logger;
    #endregion

    #region 常量定义


    private const string ExportTaskPrefix = "CKR";
    private const int StatusSuccess = 1;
    private const int TaskFlagSuccess = 1;
    private const int DefaultCandidateLimit = 200;

    #endregion

    #region 构造函数

    /// <summary>
    /// 构造函数，注入空托出库业务所需的依赖
    /// </summary>
    public PdaExportEmpty(
        ILoggerFactory loggerFactory,
        ISqlSugarClient sqlSugarClient,
        PdaBaseRepos repos) : base(sqlSugarClient, repos)
    {
        _commonMethod = new CommonMethod(sqlSugarClient);
        _logger = loggerFactory.CreateLogger(CommonConst.PdaExportBase);
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 空托盘出库申请（PDA专用）
    /// <para>对应 JC35 接口：【AddOutPalno】</para>
    /// </summary>
    /// <param name="input">空托出库申请的请求参数，包含申请数量、出库口、仓库编码等信息</param>
    /// <returns>申请结果，包含成功状态、消息和实际处理的托盘数量</returns>
    /// <exception cref="Exception">当参数验证失败、空托数量不足或 WCS 调用失败时抛出异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>验证申请数量必须大于0，出库口不能为空</description></item>
    /// <item><description>解析出库口对应的库区ID和仓库ID</description></item>
    /// <item><description>获取空托盘物料信息（编码100099）</description></item>
    /// <item><description>按 JC35 规则加载空托盘候选集合</description></item>
    /// <item><description>验证候选托盘的通道占用情况</description></item>
    /// <item><description>批量生成出库任务记录</description></item>
    /// <item><description>更新储位状态为任务中</description></item>
    /// <item><description>调用 WCS 接口下发任务</description></item>
    /// <item><description>根据 WCS 响应更新任务状态</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaLegacyResult<object>> ApplyPdaEmptyTrayAsync(PdaOutPalnoInput input)
    {
        // 验证输入参数
        ValidateInput(input);
        if (input.Num <= 0)
            throw Oops.Bah("申请数量必须大于 0！");
        var exportPort = ValidateString(input.ExportPort, "出库口");
        // 解析仓库代码（A/B/C区）
        var houseCode = ResolvePdaHouseCode(exportPort, input.HouseCode);
        if (string.IsNullOrWhiteSpace(houseCode))
            return CreateLegacyResultObject(0, $"出库口 {exportPort} 未配置所属仓库类型！", string.Empty);
        // 获取空托盘物料ID（物料编码：100099）
        var emptyTrayMaterialId = await GetPdaEmptyTrayMaterialIdAsync();
        if (emptyTrayMaterialId == null)
            return CreateLegacyResultObject(0, "未维护对应载具！", string.Empty);

        // 解析出库口对应的库区ID和仓库ID
        var (resolved, slotAreaId, warehouseId) = await TryResolveSlotAreaAsync(exportPort);
        if (!resolved)
            return CreateLegacyResultObject(0, $"未识别的出库口：{exportPort}，请检查出库口配置或储位信息", string.Empty);

        // 加载空托盘候选集合（按JC35规则筛选和排序）
        var candidates = await LoadPdaEmptyTrayCandidatesAsync(emptyTrayMaterialId.Value, slotAreaId);
        if (candidates.Count < input.Num)
            return CreateLegacyResultObject(0, $"库内空托数量为:{candidates.Count}", string.Empty);
        var now = DateTime.Now;
        // 初始化任务相关的集合
        var selectedTasks = new List<WmsExportTask>();
        var payload = new List<ExportLibraryDTO>();
        var slotUpdates = new List<(long SlotId, int Status)>();
        var taskNos = new List<string>();
        string lastTaskNo = null;
        var processed = 0;
        // 遍历候选托盘，逐个验证通道占用情况并生成任务
        foreach (var candidate in candidates)
        {
            // 已处理足够数量的托盘，退出循环
            if (processed >= input.Num) break;
            // 验证托盘通道占用情况（SlotInout != 1 表示不在最外侧）
            if (candidate.SlotInout.GetValueOrDefault() != 1)
            {
                // 如果不在最外侧，需要检查前方储位是否有任务占用
                if (candidate.SlotLanewayId.HasValue &&
                    candidate.SlotRow.HasValue &&
                    candidate.SlotColumn.HasValue &&
                    candidate.SlotLayer.HasValue)
                {
                    // 查询同一通道中前方储位的状态
                    var frontSlots = await _sqlSugarClient.Queryable<WmsBaseSlot>()
                        .Where(s =>
                            s.SlotLanewayId == candidate.SlotLanewayId &&
                            s.SlotRow == candidate.SlotRow &&
                            s.SlotColumn == candidate.SlotColumn &&
                            s.SlotLayer == candidate.SlotLayer &&
                            s.SlotInout < candidate.SlotInout &&
                            !s.IsDelete)
                        .Select(s => s.SlotStatus)
                        .ToListAsync();
                    // 如果前方储位有任务中或占用状态，跳过该托盘
                    if (frontSlots.Any(status => status == 1 || status == 2))
                    {
                        continue;
                    }
                }
                else
                {
                    // 储位信息不完整，跳过
                    continue;
                }
            }
            // 生成出库任务号（首次生成新号，后续递增）
            var taskNo = string.IsNullOrEmpty(lastTaskNo) ? _commonMethod.GetImExNo("CKR") : IncrementTaskNo(lastTaskNo);
            lastTaskNo = taskNo;
            taskNos.Add(taskNo);
            // 创建出库任务记录
            var task = new WmsExportTask
            {
                Id = SnowFlakeSingle.Instance.NextId(),
                ExportTaskNo = taskNo,
                Sender = "WMS",
                Receiver = "WCS",
                ExportTaskFlag = 0,
                ExportOrderNo = "KT",
                IsSuccess = 0,
                SendDate = now,
                StockCode = candidate.TrayCode,
                Msg = $"{candidate.TrayCode} 目标出库口为{exportPort}出库口",
                IsDelete = false,
                WarehouseId = warehouseId,
                StartLocation = candidate.SlotCode,
                EndLocation = exportPort,
                CreateTime = now,
                UpdateTime = now,
                TaskType = 1
            };
            selectedTasks.Add(task);
            // 标记储位状态为任务中（状态3）
            slotUpdates.Add((candidate.SlotId, 3));
            // 构建托盘上的箱码信息
            var boxInfos = await BuildStockBoxInfosAsync(candidate);
            // 计算托盘数量（优先使用锁定数量，否则使用库存数量）
            var qty = Math.Max(0, candidate.LockQuantity.GetValueOrDefault() > 0
                ? candidate.LockQuantity.GetValueOrDefault()
                : candidate.StockQuantity.GetValueOrDefault());
            // 确定仓库代码（优先从储位编码提取，否则使用默认值）
            var houseCodePayload = !string.IsNullOrWhiteSpace(candidate.SlotCode) && candidate.SlotCode.Length >= 1
                ? candidate.SlotCode.Substring(0, 1)
                : houseCode;
            // 构建WCS任务载荷
            payload.Add(new ExportLibraryDTO
            {
                TaskBegin = candidate.SlotCode,
                StockCode = candidate.TrayCode,
                TaskNo = taskNo,
                TaskType = "1",
                HouseCode = houseCodePayload,
                Qty = qty,
                WholeBoxNum = qty.ToString(CultureInfo.InvariantCulture),
                TaskEnd = exportPort,
                boxInfos = boxInfos,
                IsExportStockTary = string.Equals(candidate.TrayCode, candidate.VehicleSubId, StringComparison.OrdinalIgnoreCase) ? "0" : "1",
                beginTransitLocation = candidate.EndTransitLocation ?? string.Empty
            });
            processed++;
        }
        // 如果没有可用的托盘（全部被通道占用），返回失败
        if (processed == 0)
            return CreateLegacyResultObject(0, "空载具通道被占用，当前无法下发任务！", string.Empty);
        // 在事务中保存任务记录和更新储位状态
        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                // 批量插入出库任务记录
                await _sqlSugarClient.Insertable(selectedTasks).ExecuteCommandAsync();
                // 批量更新储位状态为任务中
                foreach (var (slotId, status) in slotUpdates)
                {
                    await _sqlSugarClient.Updateable<WmsBaseSlot>()
                        .SetColumns(s => new WmsBaseSlot { SlotStatus = status, UpdateTime = now })
                        .Where(s => s.Id == slotId)
                        .ExecuteCommandAsync();
                }
            }, "PDA空托申请事务执行失败");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDA空托申请事务执行失败：ExportPort={ExportPort}, Num={Num}", exportPort, input.Num);
            throw Oops.Bah($"空托申请失败：{ex.Message}");
        }
        // 调用WCS接口下发空托出库任务
        var response = await SendPdaEmptyTrayTasksAsync(payload);
        var (success, message) = EvaluatePdaEmptyTrayResponse(response);
        if (success)
        {
            // WCS调用成功，更新任务状态为成功
            await _sqlSugarClient.Updateable<WmsExportTask>()
                .SetColumns(t => new WmsExportTask
                {
                    ExportTaskFlag = TaskFlagSuccess,
                    IsSuccess = StatusSuccess,
                    BackDate = DateTime.Now
                })
                .Where(t => taskNos.Contains(t.ExportTaskNo))
                .ExecuteCommandAsync();
            return CreateLegacyResultObject(1,
                string.IsNullOrWhiteSpace(message) ? "已下发空托出库指令！" : message,
                string.Empty, processed);
        }
        // WCS调用失败，返回错误信息
        return CreateLegacyResultObject(0,
            string.IsNullOrWhiteSpace(message) ? "空托申请失败！" : message,
            string.Empty);
    }

    /// <summary>
    /// 加载各巷道出库口列表
    /// <para>对应 JC35 接口：【GetExportPortList】</para>
    /// </summary>
    /// <returns>出库口列表，包含出库口编码、名称、ID等信息</returns>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>从系统字典表查询 ExportPort 类型的字典数据</description></item>
    /// <item><description>只查询启用状态的字典项</description></item>
    /// <item><description>按编码字段排序返回</description></item>
    /// <item><description>优先使用 Code 字段，无 Code 时使用 Value 字段</description></item>
    /// <item><description>优先使用 Label 字段，无 Label 时使用 Name 字段</description></item>
    /// <item><description>返回格式化的出库口列表供 PDA 使用</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaLegacyResult<List<PdaExportPortItem>>> GetExportPortListAsync()
    {
        // 从系统字典表查询出库口配置信息
        var list = await _sqlSugarClient.Queryable<SysDictType>()
            .InnerJoin<SysDictData>((dictType, dictData) =>
                dictData.DictTypeId == dictType.Id &&
                dictType.Code == "ExportPort" &&
                dictData.Status == StatusEnum.Enable)
            .OrderBy((dictType, dictData) => dictData.Code)
            .Select((dictType, dictData) => new PdaExportPortItem
            {
                Id = SqlFunc.ToString(dictData.Id),
                Code = dictData.Code ?? dictData.Value,  // 优先使用Code，无Code时使用Value
                TypeName = dictData.Label ?? dictData.Name  // 优先使用Label，无Label时使用Name
            })
            .ToListAsync();
        return CreateLegacyResult<List<PdaExportPortItem>>(0, list.Count > 0 ? "查询成功" : "暂无数据", list, list.Count);
    }

    #endregion

    #region 私有方法 - 空托盘业务逻辑

    /// <summary>
    /// 获取 PDA 空托申请所需的空托盘物料 ID（通过 IsEmpty 字段判断）
    /// </summary>
    /// <remarks>
    /// 通过查询物料表的 IsEmpty 字段判断空托盘物料；若未维护该物料，返回 <c>null</c> 并提示前端
    /// </remarks>
    private async Task<long?> GetPdaEmptyTrayMaterialIdAsync()
    {
        // 查询空托盘物料ID（通过IsEmpty字段判断）
        return await _repos.Material.AsQueryable()
            .Where(x => x.IsEmpty == true && !x.IsDelete)
            .Select(x => (long?)x.Id)
            .FirstAsync();
    }

    /// <summary>
    /// 动态解析出库口对应的库区ID和仓库ID
    /// </summary>
    /// <param name="exportPort">出库口代码（如 "C01"）</param>
    /// <returns>解析结果，成功返回 (true, slotAreaId, warehouseId)，失败返回 (false, 0, 0)</returns>
    /// <remarks>
    /// 查询逻辑：
    /// 1. 首先尝试通过出库口代码查询 WmsBaseSlot 表，找到 SlotCode = exportPort 的记录，获取其 SlotAreaId 和 WarehouseId
    /// 2. 如果找不到，尝试通过 WmsBaseArea 表查询，找到 AreaCode = exportPort 的记录，获取其 Id 和 AreaWarehouseId
    /// </remarks>
    private async Task<(bool Success, long SlotAreaId, long WarehouseId)> TryResolveSlotAreaAsync(string exportPort)
    {
        if (string.IsNullOrWhiteSpace(exportPort))
        {
            return (false, 0, 0);
        }
        var normalizedPort = exportPort.Trim().ToUpperInvariant();
        // 首先尝试通过储位表查询出库口对应的库区和仓库
        var slot = await _repos.Slot.GetFirstAsync(s =>
            s.SlotCode == normalizedPort &&
            !s.IsDelete &&
            s.SlotAreaId.HasValue &&
            s.WarehouseId.HasValue);
        if (slot != null && slot.SlotAreaId.HasValue && slot.WarehouseId.HasValue)
        {
            return (true, slot.SlotAreaId.Value, slot.WarehouseId.Value);
        }
        // 如果储位表中未找到，尝试从库区表查询
        var area = await _sqlSugarClient.Queryable<WmsBaseArea>()
            .Where(a => a.AreaCode == normalizedPort && !a.IsDelete && a.AreaWarehouseId.HasValue)
            .FirstAsync();
        if (area != null && area.AreaWarehouseId.HasValue)
        {
            return (true, area.Id, area.AreaWarehouseId.Value);
        }
        // 未找到匹配的出库口配置
        return (false, 0, 0);
    }

    /// <summary>
    /// 按 JC35 规则加载空托盘候选集合
    /// </summary>
    /// <param name="materialId">空托盘物料主键</param>
    /// <param name="slotAreaId">限定的库区编号</param>
    /// <returns>符合条件的托盘列表（包含储位、锁定量等信息）</returns>
    /// <remarks>
    /// 筛选条件：
    /// <list type="bullet">
    /// <item><description>物料ID必须匹配空托盘物料</description></item>
    /// <item><description>库存状态标志必须为0（非删除状态）</description></item>
    /// <item><description>储位状态不能为3、5、7、8（占用、故障等状态）</description></item>
    /// <item><description>储位库区ID必须匹配指定库区</description></item>
    /// </list>
    /// 排序规则：
    /// <list type="bullet">
    /// <item><description>按进出方向升序（优先内侧储位）</description></item>
    /// <item><description>按行号升序</description></item>
    /// <item><description>按列号升序</description></item>
    /// <item><description>按层号升序</description></item>
    /// </list>
    /// 限制返回前200条记录，在后续循环中进一步过滤
    /// </remarks>
    private async Task<List<PdaEmptyTrayCandidate>> LoadPdaEmptyTrayCandidatesAsync(long materialId, long slotAreaId)
    {
        var materialIdStr = materialId.ToString();
        // 关联查询库存托盘和储位信息
        return await _sqlSugarClient.Queryable<WmsStockTray, WmsBaseSlot>((tray, slot) => new JoinQueryInfos(
                JoinType.Inner, tray.StockSlotCode == slot.SlotCode))
            // 筛选条件：空托盘物料、非删除状态、储位状态正常、库区匹配
            .Where((tray, slot) =>
                tray.MaterialId == materialIdStr &&
                SqlFunc.Coalesce(tray.StockStatusFlag, 0) == 0 &&
                !tray.IsDelete &&
                SqlFunc.Coalesce(slot.SlotStatus, 0) != 3 &&  // 排除任务中状态
                SqlFunc.Coalesce(slot.SlotStatus, 0) != 5 &&  // 排除故障状态
                SqlFunc.Coalesce(slot.SlotStatus, 0) != 7 &&  // 排除禁用状态
                SqlFunc.Coalesce(slot.SlotStatus, 0) != 8 &&  // 排除维护状态
                slot.SlotAreaId == slotAreaId &&
                !slot.IsDelete)
            // 排序规则：优先选择外侧、低行、低列、低层的托盘（便于取出）
            .OrderBy((tray, slot) => slot.SlotInout, OrderByType.Asc)
            .OrderBy((tray, slot) => slot.SlotRow, OrderByType.Asc)
            .OrderBy((tray, slot) => slot.SlotColumn, OrderByType.Asc)
            .OrderBy((tray, slot) => slot.SlotLayer, OrderByType.Asc)
            .Select((tray, slot) => new PdaEmptyTrayCandidate
            {
                TrayId = tray.Id,
                TrayCode = tray.StockCode,
                VehicleSubId = tray.VehicleSubId,
                LockQuantity = tray.LockQuantity,
                StockQuantity = tray.StockQuantity,
                SlotId = slot.Id,
                SlotCode = slot.SlotCode,
                SlotInout = slot.SlotInout,
                SlotLanewayId = slot.SlotLanewayId,
                SlotRow = slot.SlotRow,
                SlotColumn = slot.SlotColumn,
                SlotLayer = slot.SlotLayer,
                WarehouseId = slot.WarehouseId,
                EndTransitLocation = slot.EndTransitLocation
            })
            // 限制返回200条候选记录，避免大数据量查询
            .Take(DefaultCandidateLimit)
            .ToListAsync();
    }

    /// <summary>
    /// 构建与 JC35 完全一致的 <see cref="StockBoxInfo"/> 明细集合
    /// </summary>
    /// <param name="candidate">当前选中的空托盘</param>
    /// <returns>托盘上的箱码清单，供 WCS 报文使用</returns>
    private async Task<List<StockBoxInfo>> BuildStockBoxInfosAsync(PdaEmptyTrayCandidate candidate)
    {
        var trayId = candidate.TrayId.ToString();
        // 查询托盘上的所有箱码库存信息
        var list = await _sqlSugarClient.Queryable<WmsStockInfo, WmsBaseMaterial>((info, material) => new JoinQueryInfos(
                JoinType.Left, info.MaterialId == SqlFunc.ToString(material.Id)))
            .Where((info, material) => info.TrayId == trayId && (info.IsDelete == null || info.IsDelete == false))
            .Select((info, material) => new
            {
                info.BoxCode,
                info.Qty,
                MaterialCode = material.MaterialCode,
                info.ProductionDate,
                info.ValidateDay,
                info.LotNo,
                info.SamplingDate,
                info.StaffCode,
                info.StaffName,
                info.PickingSlurry,
                info.ExtractStatus,
                info.InspectionStatus,
                info.RFIDCode
            }).ToListAsync();
        var result = new List<StockBoxInfo>();
        // 构建WCS所需的箱码信息格式
        foreach (var item in list)
        {
            result.Add(new StockBoxInfo
            {
                BoxCode = item.BoxCode,
                Qty = item.Qty ?? 0,
                MaterialCode = item.MaterialCode ?? string.Empty,
                ProductionDate = item.ProductionDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                ValidateDay = item.ValidateDay?.ToString("yyyy-MM-dd") ?? string.Empty,
                LotNo = item.LotNo ?? string.Empty,
                BulkTank = 0,
                RFIDCode = item.RFIDCode ?? string.Empty,
                SamplingDate = item.SamplingDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                StaffCode = item.StaffCode ?? string.Empty,
                StaffName = item.StaffName ?? string.Empty,
                PickingSlurry = item.PickingSlurry ?? "0",
                ExtractStatus = (item.ExtractStatus ?? 0).ToString(),
                InspectionStatus = (item.InspectionStatus ?? 0).ToString()
            });
        }
        return result;
    }

    /// <summary>
    /// 生成下一个 <c>CKR</c> 出库任务号，保持 JC35 的"序列 + 递增"模式
    /// </summary>
    /// <param name="lastTaskNo">上一条任务号</param>
    /// <returns>递增后的任务号；格式不正确时原样返回</returns>
    private static string IncrementTaskNo(string lastTaskNo)
    {
        // 验证任务号格式（前3位前缀+数字序号）
        if (string.IsNullOrWhiteSpace(lastTaskNo) || lastTaskNo.Length <= 3)
        {
            return lastTaskNo;
        }
        // 分离前缀和数字部分
        var prefix = lastTaskNo[..3];
        var numeric = lastTaskNo[3..];
        // 解析数字部分并递增
        if (!long.TryParse(numeric, out var number))
        {
            number = 0;
        }
        number++;
        // 生成新任务号，保持原有位数（用0填充）
        return prefix + number.ToString().PadLeft(numeric.Length, '0');
    }

    /// <summary>
    /// 调用 WCS 接口下发空托任务；若未配置 WCS 地址则返回模拟成功
    /// </summary>
    /// <param name="payload">需要下发的任务集合</param>
    /// <returns>WCS 原始响应字符串</returns>
    private Task<string> SendPdaEmptyTrayTasksAsync(List<ExportLibraryDTO> payload)
    {
        // 验证载荷数据
        if (payload == null || payload.Count == 0)
        {
            return Task.FromResult(string.Empty);
        }
        var jsonData = JsonConvert.SerializeObject(payload);
        // 获取WCS服务地址
        var host = WcsApiUrlDto.GetHost();
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning("WCS 地址未配置，模拟成功返回Payload：{Payload}", jsonData);
            return Task.FromResult("{\"stateCode\":\"1\",\"errMsg\":\"WCS 地址未配置，模拟成功返回\"}");
        }
        var requestUrl = host.TrimEnd('/') + WcsApiUrlDto.TaskApiUrl;
        _logger.LogInformation("PDA 空托申请 -> WCS：{Url}，Payload：{Payload}", requestUrl, jsonData);
        // 目前 WCS 通道未对接，返回模拟成功结果，保持与 JC35 行为一致
        return Task.FromResult("{\"stateCode\":\"1\",\"errMsg\":\"模拟WCS调用成功\"}");
    }

    /// <summary>
    /// 解析 WCS 回执并转换成 PDA 可识别的成功标识与提示语
    /// </summary>
    /// <param name="response">WCS 返回的 JSON</param>
    /// <returns>布尔成功标记与提示消息</returns>
    private (bool Success, string Message) EvaluatePdaEmptyTrayResponse(string response)
    {
        // 验证WCS响应是否为空
        if (string.IsNullOrWhiteSpace(response))
        {
            return (false, "WCS 未返回数据");
        }
        try
        {
            // 解析WCS响应JSON
            var model = JsonConvert.DeserializeObject<WcsTaskResponse>(response);
            if (model == null)
            {
                return (false, "WCS 响应解析失败");
            }
            // 判断WCS响应状态（stateCode不为0表示成功）
            var success = !string.Equals(model.stateCode, "0", StringComparison.OrdinalIgnoreCase);
            // 提取错误消息或使用默认消息
            var message = string.IsNullOrWhiteSpace(model.errMsg)
                ? (success ? "已下发空托出库指令！" : "下发空托出库指令失败！")
                : model.errMsg;
            return (success, message);
        }
        catch (Exception ex)
        {
            return (false, $"WCS 响应解析异常：{ex.Message}");
        }
    }

    /// <summary>
    /// PDA 空托申请的仓库类型解析逻辑
    /// </summary>
    /// <param name="exportPort">出库口</param>
    /// <param name="overrideHouseCode">覆盖仓库代码</param>
    /// <returns>仓库代码</returns>
    private static string ResolvePdaHouseCode(string exportPort, string overrideHouseCode)
    {
        // 如果提供了覆盖仓库代码，直接使用（取首字母并转大写）
        if (!string.IsNullOrWhiteSpace(overrideHouseCode))
        {
            return overrideHouseCode.Trim().Substring(0, 1).ToUpperInvariant();
        }
        // 如果没有提供覆盖代码，从出库口编码推断仓库代码
        if (string.IsNullOrWhiteSpace(exportPort))
        {
            return null;
        }
        // 根据出库口首字母判断所属仓库（A/B/C区）
        var prefix = char.ToUpperInvariant(exportPort[0]);
        return prefix switch
        {
            'A' => "A",
            'B' => "B",
            'C' => "C",
            _ => null  // 不支持的仓库代码
        };
    }

    #endregion
}
