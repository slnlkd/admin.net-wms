// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using System;
using System.IO;
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
using Admin.NET.Application.Service.WmsSqlView;
using Admin.NET.Application.Service.WmsSqlView.Dto;
using Admin.NET.Application.Service.WmsPda.Process;

namespace Admin.NET.Application.Service.WmsPda;
/// <summary>
/// PDA 出库业务处理服务。
/// <para>承载 JC35 <c>PdaInterfaceController</c> 出库接口迁移，提供完整的PDA出库相关业务功能。</para>
/// </summary>
/// <remarks>
/// 主要功能包括：
/// <list type="bullet">
/// <item><description>手动拆垛（有箱码）：绑定、查询、删除拆垛信息，人工拆垛出库确认</description></item>
/// <item><description>手动拆垛（无箱码）：绑定、查询、删除无箱码拆跺明细</description></item>
/// <item><description>空托出库：空托盘出库申请、出库口管理</description></item>
/// <item><description>库存管理：库存扣减、托盘释放、状态更新</description></item>
/// <item><description>任务管理：WCS任务下发、状态跟踪</description></item>
/// </list>
/// </remarks>
public class PdaExportProcess : PdaBase, ITransient
{
    #region 字段定义
    private readonly SqlSugarRepository<WmsManualDepalletizing> _manualRep;    //人工拆垛
    private readonly SqlSugarRepository<WmsExportOrder> _exportOrderRep;        //出库订单
    private readonly SqlSugarRepository<WmsExportBoxInfo> _exportBoxInfoRep;    //出库箱码信息
    private readonly SqlSugarRepository<WmsStockTray> _stockTrayRep;            //库存托盘
    private readonly SqlSugarRepository<WmsStockInfo> _stockInfoRep;            //库存信息
    private readonly SqlSugarRepository<WmsBaseMaterial> _materialRep;            //物料
    private readonly SqlSugarRepository<WmsExportNotify> _exportNotifyRep;            //出库通知
    private readonly SqlSugarRepository<WmsExportNotifyDetail> _exportNotifyDetailRep;    //出库通知明细
    private readonly SqlSugarRepository<WmsStock> _stockRep;                            //库存
    private readonly SqlSugarRepository<WmsSysStockCode> _sysStockCodeRep;            //库存编码
    private readonly SqlSugarRepository<WmsExportTask> _exportTaskRep;                //出库任务
    private readonly SqlSugarRepository<WmsBaseSlot> _baseSlotRep;                    //储位
    private readonly CommonMethod _commonMethod;                                        //通用方法
    private readonly ILogger _logger;                                //日志
    private readonly WmsSqlViewService _sqlViewService;                                        //SQL试图功能服务（视图查询和SQL函数）
    #endregion

    #region 常量定义
    // 常量定义

    private const string EmptyTrayOrderNo = "KT";                               //空托盘出库流水号
    private const string ExportTaskPrefix = "CKR";                              //出库任务号前缀
    // 状态常量
    private const int StateActive = 0;                                          //活跃状态
    private const int StateProcessed = 1;                                       //已处理状态
    private const int StatusSuccess = 1;                                        //成功状态
    private const int TaskFlagSuccess = 1;                                      //执行成功
    // 任务相关常量
    private const int ExportExecuteFlagCompleted = 2;                           //出库执行完成
    // 储位状态常量
    private const int SlotStatusInTask = 3;                                     //任务中
    // 其他业务常量
    private const int DefaultPageSize = 200;                                    //默认分页大小
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖
    /// </summary>
    public PdaExportProcess(
        ILoggerFactory loggerFactory,
        SqlSugarRepository<WmsManualDepalletizing> manualRepo,
        SqlSugarRepository<WmsExportOrder> exportOrderRepo,
        SqlSugarRepository<WmsExportBoxInfo> exportBoxInfoRepo,
        SqlSugarRepository<WmsStockTray> stockTrayRepo,
        SqlSugarRepository<WmsStockInfo> stockInfoRepo,
        SqlSugarRepository<WmsBaseMaterial> materialRepo,
        SqlSugarRepository<WmsExportNotify> exportNotifyRepo,
        SqlSugarRepository<WmsExportNotifyDetail> exportNotifyDetailRepo,
        SqlSugarRepository<WmsStock> stockRepo,
        SqlSugarRepository<WmsSysStockCode> sysStockCodeRepo,
        SqlSugarRepository<WmsExportTask> exportTaskRepo,
        SqlSugarRepository<WmsBaseSlot> baseSlotRepo,
        ISqlSugarClient sqlSugarClient,
        WmsSqlViewService sqlViewService) : base(sqlSugarClient)
    {
        _manualRep = manualRepo;
        _exportOrderRep = exportOrderRepo;
        _exportBoxInfoRep = exportBoxInfoRepo;
        _stockTrayRep = stockTrayRepo;
        _stockInfoRep = stockInfoRepo;
        _materialRep = materialRepo;
        _exportNotifyRep = exportNotifyRepo;
        _exportNotifyDetailRep = exportNotifyDetailRepo;
        _stockRep = stockRepo;
        _sysStockCodeRep = sysStockCodeRepo;
        _exportTaskRep = exportTaskRepo;
        _baseSlotRep = baseSlotRepo;
        _commonMethod = new CommonMethod(sqlSugarClient);
        _sqlViewService = sqlViewService;
        _logger = loggerFactory.CreateLogger(CommonConst.PdaExportProcess);
    }
    #endregion

    #region 手动拆垛（有箱码）
    /// <summary>
    /// 删除拆跺信息数据。
    /// <para>对应 JC35 接口：【RemoveManual】</para>
    /// </summary>
    /// <param name="input">删除拆跺信息的请求参数，包含拆跺记录ID、箱码、托盘编码、出库流水号等信息</param>
    /// <returns>操作结果，包含成功状态和消息</returns>
    /// <exception cref="Exception">当拆跺信息不存在或已处理时抛出异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>验证输入参数的有效性</description></item>
    /// <item><description>根据ID或条件查询拆跺记录</description></item>
    /// <item><description>将记录标记为已删除（软删除）</description></item>
    /// <item><description>更新记录的修改时间</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> RemoveManualAsync(PdaManualRemoveInput input)
    {
        return await ExecuteWithLoggingAsync("删除拆跺信息", async () =>
        {
            ValidateInput(input);
            var manual = await FindManualDepalletizingForRemovalAsync(input) ?? throw Oops.Bah("拆跺信息不存在或已处理！");
            manual.IsDelete = true;
            manual.UpdateTime = DateTime.Now;
            await _manualRep.AsUpdateable(manual)
                .UpdateColumns(x => new { x.IsDelete, x.UpdateTime })
                .ExecuteCommandAsync();
            return CreateSuccessResult();
        }, new { input.Id, input.BoxCode, input.TrayCode, input.ExportOrderNo });
    }
    /// <summary>
    /// 查询拆跺箱码信息。
    /// <para>对应 JC35 接口：【GetManBoxInfo】</para>
    /// </summary>
    /// <param name="input">查询拆跺箱码信息的请求参数，包含箱码、托盘编码、出库流水号等信息</param>
    /// <returns>查询结果，包含箱码详细信息、状态码和消息</returns>
    /// <exception cref="Exception">当参数验证失败时抛出异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>验证输入参数的有效性</description></item>
    /// <item><description>根据箱码查询拆跺视图数据</description></item>
    /// <item><description>如果提供了托盘编码，验证托盘与箱码的匹配关系</description></item>
    /// <item><description>检查托盘与订单的质检状态是否一致</description></item>
    /// <item><description>返回格式化的箱码详细信息</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaManualBoxInfoResult> GetManualBoxInfoAsync(PdaManualBoxQueryInput input)
    {
        return await ExecuteWithLoggingAsync("查询拆跺箱码信息", async () =>
        {
            ValidateInput(input);
            var boxCode = ValidateString(input.BoxCode, "箱码");
            var record = await _sqlViewService.QueryManualDepalletizingView()
                .MergeTable()
                .FirstAsync(x => x.BoxCode == boxCode);
            if (record == null)
                return CreateManualBoxInfoResult(1, "箱码信息不存在！", null, "0", 0);
            if (string.IsNullOrWhiteSpace(input.TrayCode))
                return CreateManualBoxInfoResult(1, "查询成功！", MapManualBoxInfo(record), "1", 1);
            var tray = ValidateString(input.TrayCode, "托盘编码");
            var orderNo = input.ExportOrderNo?.Trim();
            record = await _sqlViewService.QueryManualDepalletizingView()
                .MergeTable()
                .FirstAsync(x => x.BoxCode == boxCode && x.StockCode == tray && x.ExportOrderNo == orderNo);
            if (record == null)
                return CreateManualBoxInfoResult(0, "箱码与托盘不匹配！", null, "-1", 0);
            if (record.TrayInspectionStatus != record.OrderInspectionStatus)
                return CreateManualBoxInfoResult(0, "箱码与流水质检状态不匹配！", null, "-1", 0);
            return CreateManualBoxInfoResult(1, "查询成功！", MapManualBoxInfo(record), "1", 1);
        }, new { input.BoxCode, input.TrayCode, input.ExportOrderNo });
    }
    /// <summary>
    /// 根据托盘编号查询对应的出库单号。
    /// <para>对应 JC35 接口：【GetExprotCodeById】</para>
    /// </summary>
    /// <param name="input">查询出库单的请求参数，包含托盘编码和是否箱码标识</param>
    /// <returns>出库单信息列表，包含出库单号、托盘编码、物料编码等信息</returns>
    /// <exception cref="Exception">当参数验证失败时抛出异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>根据托盘编码查询拆跺视图数据</description></item>
    /// <item><description>筛选出已执行完成的出库流水（ExportExecuteFlag = 1）</description></item>
    /// <item><description>按出库单号分组并去重</description></item>
    /// <item><description>如果指定了IsBoxCode参数，过滤掉有箱码的托盘</description></item>
    /// <item><description>返回格式化的出库单信息列表</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaLegacyResult<List<PdaExportOrderItem>>> GetExportOrderByTrayAsync(PdaExportOrderQueryInput input)
    {
        return await ExecuteWithLoggingAsync("根据托盘查询出库单号", async () =>
        {
            ValidateInput(input);
            var trayCode = ValidateString(input.StockCode, "托盘编码");
            var rawList = await _sqlViewService.QueryManualDepalletizingView()
                .MergeTable()
                .Where(x => x.StockCode == trayCode && x.ExportExecuteFlag == StatusSuccess)
                .ToListAsync();
            var manualOrders = rawList.GroupBy(x => x.ExportOrderNo).Select(g => g.First()).ToList();
            var result = new List<PdaExportOrderItem>();
            HashSet<string> trayHasBoxCodes = null;
            if (!string.IsNullOrWhiteSpace(input.IsBoxCode) && manualOrders.Count > 0)
            {
                // 预先批量加载托盘是否存在箱码，避免在循环中出现 N+1 查询
                var trayIds = manualOrders
                    .Select(x => x.TrayId)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();
                if (trayIds.Count > 0)
                {
                    var trayIdList = await _stockInfoRep.AsQueryable()
                        .Where(x => trayIds.Contains(x.TrayId) && !SqlFunc.IsNullOrEmpty(x.BoxCode))
                        .Select(x => x.TrayId)
                        .Distinct()
                        .ToListAsync();
                    trayHasBoxCodes = trayIdList.ToHashSet(StringComparer.OrdinalIgnoreCase);
                }
            }
            foreach (var item in manualOrders)
            {
                var trayId = item.TrayId;
                if (!string.IsNullOrWhiteSpace(input.IsBoxCode))
                {
                    if (trayHasBoxCodes != null &&
                        !string.IsNullOrWhiteSpace(trayId) &&
                        trayHasBoxCodes.Contains(trayId))
                    {
                        continue;
                    }
                }
                result.Add(new PdaExportOrderItem
                {
                    ExportOrderNo = item.ExportOrderNo,
                    StockCode = item.StockCode,
                    TrayId = item.TrayId,
                    MaterialCode = item.MaterialCode,
                    ExportMaterialName = item.ExportMaterialName
                });
            }
            return CreateLegacyResult(1, "出库单信息列表", result, result.Count);
        }, new { input.StockCode, input.IsBoxCode });
    }
    /// <summary>
    /// 查询拆跺信息。
    /// <para>对应 JC35 接口：【GetTrayInfo】</para>
    /// </summary>
    /// <param name="input">查询拆跺信息的请求参数，必须包含托盘编码和出库流水号</param>
    /// <returns>拆跺信息查询结果，包含托盘详细信息或错误信息</returns>
    /// <exception cref="Exception">当参数为空或必要字段为空时抛出异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>验证输入参数，确保托盘编码和出库流水号不为空</description></item>
    /// <item><description>根据托盘编码和出库流水号查询拆跺视图数据</description></item>
    /// <item><description>返回第一条匹配的拆跺记录详细信息</description></item>
    /// <item><description>如果找不到记录则返回失败结果</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaManualBoxInfoResult> GetTrayInfoAsync(PdaManualDepalletizingQueryInput input)
    {
        ValidateInput(input);
        var trayCode = ValidateString(input.TrayCode, "托盘编码");
        var orderNo = ValidateString(input.ExportOrderNo, "出库流水");
        var list = await _sqlViewService.QueryManualDepalletizingView()
            .MergeTable()
            .Where(x => x.StockCode == trayCode && x.ExportOrderNo == orderNo)
            .ToListAsync();
        if (list == null || list.Count == 0)
            return CreateManualBoxInfoResult(0, "托盘信息不存在！", null, "-1", 0);
        return CreateManualBoxInfoResult(1, "查询成功！", MapManualBoxInfo(list[0]), "1", 1);
    }
    /// <summary>
    /// 计算出库绑定数量。
    /// <para>对应 JC35 接口：【GetBoxSum】</para>
    /// </summary>
    /// <param name="input">计算绑定数量的请求参数，包含物料编码、批号、托盘编码、出库数量等信息</param>
    /// <returns>计算结果，包含绑定数量、散装数量、整箱数量等信息</returns>
    /// <exception cref="Exception">当参数为空或存在重复绑定时抛出异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>检查当前托盘是否已绑定到其他出库流水</description></item>
    /// <item><description>根据锁定数量和出库数量计算绑定数量</description></item>
    /// <item><description>如果物料有箱数量规格，计算整箱出库件数</description></item>
    /// <item><description>区分整箱和散装数量的计算逻辑</description></item>
    /// <item><description>返回详细的数量分配结果</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaLegacyResult<PdaBoxListQtyOutput>> CalculateBoxQuantityAsync(PdaBoxListQtyInput input)
    {
        ValidateInput(input, "获取信息失败！");
        var existing = await _manualRep.GetFirstAsync(x => 
            x.ExportOrderNo == input.ExportOrderNo && 
            x.TrayCode == input.StockStockCode && 
            x.IsDelete == false && 
            x.State == StateActive);
        if (existing != null)
            return CreateLegacyResult<PdaBoxListQtyOutput>(0, "当前出库物料已绑定！", null, 1);
        var output = new PdaBoxListQtyOutput
        {
            ExportGoodsCode = input.ExportGoodsCode,
            LotNo = input.LotNo,
            StockStockCode = input.StockStockCode,
            BoxCode = input.BoxCode,
            ExportOrderNo = input.ExportOrderNo,
            LockQuantity = input.LockQuantity,
            Qty = input.Qty,
            GroupQuantity = input.GroupQuantity,
            ExportQuantity = input.ExportQuantity,
            TypeCode = input.TypeCode,
        };
        if (input.Qty == input.ExportQuantity)
        {
            output.OutScatterQty = Convert.ToDecimal(input.Qty ?? 0);
        }
        else if (input.ExportQuantity >= input.GroupQuantity)
        {
            var diff = (input.ExportQuantity ?? 0) - Convert.ToDecimal(input.GroupQuantity ?? 0);
            output.OutScatterQty = Math.Min(Convert.ToDecimal(input.Qty ?? 0), diff);
        }
        else
        {
            output.OutScatterQty = Convert.ToDecimal(input.Qty ?? 0);
        }
        if (!string.IsNullOrWhiteSpace(input.TypeCode))
        {
            var material = await _materialRep.GetFirstAsync(x => x.MaterialCode == input.ExportGoodsCode && x.IsDelete == false);
            if (material != null && !string.IsNullOrWhiteSpace(material.BoxQuantity))
            {
                if (decimal.TryParse(material.BoxQuantity, out var boxQty) && boxQty > 0 && input.ExportQuantity >= boxQty)
                {
                    var qty = (double)(input.ExportQuantity ?? 0) / (double)boxQty;
                    output.OutQty = Convert.ToInt32(Math.Floor(qty));
                }
            }
        }
        return CreateLegacyResult<PdaBoxListQtyOutput>(1, "查询成功！", output, 1);
    }
    /// <summary>
    /// 绑定拆跺箱码。
    /// <para>对应 JC35 接口：【AddManualDepalletizing】</para>
    /// </summary>
    /// <param name="input">绑定拆跺箱码的请求参数，包含出库流水号、箱码、托盘编码、数量等信息</param>
    /// <returns>绑定操作结果，包含成功状态和消息</returns>
    /// <exception cref="Exception">当参数为空、出库流水不存在或库存托盘信息无效时抛出异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>验证出库流水号的有效性</description></item>
    /// <item><description>加载对应的库存托盘信息</description></item>
    /// <item><description>判断是否为全托出库（出库数量等于库存总数量）</description></item>
    /// <item><description>全托出库时自动绑定该托盘下所有箱码</description></item>
    /// <item><description>部分出库时只绑定指定的单个箱码</description></item>
    /// <item><description>创建拆跺记录并保存到数据库</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> AddManualDepalletizingAsync(PdaManualDepalletizingCreateInput input)
    {
        ValidateInput(input);
        ValidateString(input.ExportOrderNo, "出库流水");
        var exportOrder = await _exportOrderRep.GetFirstAsync(x => 
            x.ExportOrderNo == input.ExportOrderNo && x.IsDelete == false) ?? throw Oops.Bah("获取出库流水信息失败！");
        var stockTray = await LoadExportTrayAsync(exportOrder) ?? throw Oops.Bah("获取库存托盘信息失败！");
        var trayId = stockTray.Id.ToString();
        var isFullTray = (exportOrder.ExportQuantity ?? 0) == (stockTray.StockQuantity ?? 0) + (stockTray.LockQuantity ?? 0);
        if (isFullTray)
        {
            var materialId = exportOrder.ExportMaterialId?.ToString();
            var exportBoxes = await _exportBoxInfoRep.AsQueryable()
                .Where(x => x.ExportOrderNo == exportOrder.ExportOrderNo && x.MaterialId == materialId && x.LotNo == exportOrder.ExportLotNo)
                .ToListAsync();
            var manualList = new List<WmsManualDepalletizing>();
            foreach (var item in exportBoxes)
            {
                var info = await _stockInfoRep.GetFirstAsync(x => x.BoxCode == item.BoxCode && x.TrayId == trayId);
                if (info == null) continue;
                var totalQty = FormatDecimal(info.Qty + (info.LockQuantity ?? 0));
                manualList.Add(new WmsManualDepalletizing
                {
                    Id = SnowFlakeSingle.Instance.NextId(),
                    BoxCode = item.BoxCode,
                    Qty = totalQty,
                    ScanQty = totalQty,
                    TrayCode = item.StockCodeCode,
                    State = StateActive,
                    ExportOrderNo = exportOrder.ExportOrderNo,
                    IsDelete = false,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                });
            }
            if (manualList.Count > 0)
            {
                await _manualRep.InsertRangeAsync(manualList);
            }
        }
        else
        {
            var manual = new WmsManualDepalletizing
            {
                Id = SnowFlakeSingle.Instance.NextId(),
                BoxCode = input.BoxCode,
                Qty = FormatDecimal(input.Qty),
                ScanQty = FormatDecimal(input.ScanQty),
                TrayCode = input.TrayCode,
                State = 0,
                ExportOrderNo = exportOrder.ExportOrderNo,
                IsDelete = false,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };
            await _manualRep.InsertAsync(manual);
        }
        return CreateSuccessResult("绑定成功");
    }
    /// <summary>
    /// 查询拆跺记录。
    /// <para>对应 JC35 接口：【GetManualDepalletizing】</para>
    /// </summary>
    /// <param name="input">查询拆跺记录的请求参数，必须包含托盘编码和出库流水号</param>
    /// <returns>拆跺记录列表，包含箱码、物料信息、数量等详细信息</returns>
    /// <exception cref="Exception">当参数为空时抛出异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>关联查询拆跺记录、库存信息、物料信息和托盘信息</description></item>
    /// <item><description>筛选活跃状态且未删除的拆跺记录</description></item>
    /// <item><description>按ID降序排列，返回最新的记录</description></item>
    /// <item><description>返回格式化的拆跺记录列表，包含物料名称、编码等关联信息</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaLegacyResult<List<PdaManualDepalletizingItem>>> GetManualDepalletizingAsync(PdaManualDepalletizingQueryInput input)
    {
        ValidateInput(input);
        var trayCode = ValidateString(input.TrayCode, "托盘编码");
        var orderNo = ValidateString(input.ExportOrderNo, "出库流水");
        var list = await _sqlSugarClient.Queryable<WmsManualDepalletizing, WmsStockInfo, WmsBaseMaterial, WmsStockTray>(
                (manual, info, material, tray) => new JoinQueryInfos(
                    JoinType.Inner, manual.BoxCode == info.BoxCode,
                    JoinType.Left, info.MaterialId == SqlFunc.ToString(material.Id),
                    JoinType.Left, info.TrayId == SqlFunc.ToString(tray.Id)))
            .Where((manual, info, material, tray) => 
                manual.TrayCode == trayCode && 
                manual.ExportOrderNo == orderNo && 
                manual.State == StateActive && 
                manual.IsDelete == false)
            .OrderBy((manual, info, material, tray) => manual.Id, OrderByType.Desc)
            .Select((manual, info, material, tray) => new PdaManualDepalletizingItem
            {
                BoxCode = manual.BoxCode,
                Qty = info.Qty,
                ScanQty = info.LockQuantity ?? info.Qty,
                TrayCode = manual.TrayCode,
                MaterialName = material.MaterialName,
                MaterialCode = material.MaterialCode,
                StockLotNo = tray.LotNo,
                Id = SqlFunc.ToString(manual.Id),
                OutQty = manual.OutQty == null ? null : SqlFunc.ToString(manual.OutQty)
            }).ToListAsync();
        return CreateLegacyResult<List<PdaManualDepalletizingItem>>(1, "查询成功！", list, list.Count);
    }
    /// <summary>
    /// 人工拆垛出库确认。
    /// <para>对应 JC35 接口：【SaveManTask】</para>
    /// </summary>
    /// <param name="input">人工拆垛确认的请求参数，包含托盘编码和出库流水号</param>
    /// <returns>确认操作结果，包含成功状态、消息和数据</returns>
    /// <exception cref="Exception">当参数验证失败、库存不足或数量不匹配时抛出异常</exception>
    /// <remarks>
    /// 业务流程：
    /// <list type="number">
    /// <item><description>校验托盘编码和出库流水号的合法性</description></item>
    /// <item><description>加载并聚合拆垛明细信息</description></item>
    /// <item><description>验证PDA扫描数量与流水拣货数量是否一致</description></item>
    /// <item><description>在事务内更新以下表的数据：
    /// <list type="bullet">
    /// <item><description>WmsManualDepalletizing - 标记拆垛记录为已处理</description></item>
    /// <item><description>WmsExportBoxInfo - 更新出库箱码拣货状态和数量</description></item>
    /// <item><description>WmsStockInfo - 扣减或删除库存箱码</description></item>
    /// <item><description>WmsStockTray - 更新托盘库存数量和状态</description></item>
    /// <item><description>WmsStock - 更新库位维度的锁定数量</description></item>
    /// <item><description>WmsBaseSlot - 更新储位状态</description></item>
    /// <item><description>WmsExportOrder - 推进出库流水状态</description></item>
    /// <item><description>WmsExportNotify/WmsExportNotifyDetail - 更新出库通知状态</description></item>
    /// </list>
    /// </description></item>
    /// <item><description>释放未被扫描的必出箱码库存</description></item>
    /// <item><description>返回与JC35兼容的确认结果</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaLegacyResult<object>> SaveManualTaskAsync(PdaManualTaskSaveInput input)
    {
        if (input == null)
        {
            throw Oops.Bah("请求参数不能为空！");
        }
        var trayCode = ValidateString(input.TrayCode, "托盘编码");
        var orderNo = ValidateString(input.ExportOrderNo, "出库流水");
        var trayViewList = await _sqlViewService.QueryManualDepalletizingView()
            .MergeTable()
            .Where(x => x.StockCode == trayCode && x.ExportOrderNo == orderNo)
            .ToListAsync();
        if (trayViewList == null || trayViewList.Count == 0)
            return CreateLegacyResultObject(0, "托盘信息不存在！");
        var manualDetails = await LoadManualTaskDetailsAsync(trayCode, orderNo);
        if (manualDetails.Count == 0)
            return CreateLegacyResultObject(1, "未绑定出库的箱码！", string.Empty);
        var exportOrder = await _exportOrderRep.GetFirstAsync(x => 
            x.ExportOrderNo == orderNo && x.IsDelete == false) ?? throw Oops.Bah("获取出库流水信息失败！");
        var exportBoxMap = await LoadExportBoxInfoMapAsync(orderNo, manualDetails);
        var stockInfoMap = await LoadStockInfoMapAsync(manualDetails);
        var updatedExportBoxCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stockReduceTracker = new Dictionary<long, WmsStockInfo>();
        var stockDeleteTracker = new Dictionary<long, WmsStockInfo>();
        var stockTray = await LoadExportTrayAsync(exportOrder);
        if (stockTray == null)
            throw Oops.Bah("获取库存托盘信息失败！");
        var exportMaterial = exportOrder.ExportMaterialId.HasValue ? await _materialRep.GetFirstAsync(x => x.Id == exportOrder.ExportMaterialId.Value && x.IsDelete == false) : null;
        var expected = exportOrder.PickedNum ?? 0m;
        var actual = CalculateManualScanSum(manualDetails, exportMaterial?.MaterialCode, exportOrder.ExportLotNo);
        if (expected != 0 && actual != expected)
            return CreateLegacyResultObject(0, "出库数量不相等！", string.Empty);
        var now = DateTime.Now;
        var tranResult = await _sqlSugarClient.Ado.UseTranAsync(async () =>
        {
            try
            {
                decimal totalScanQty = 0m;
                foreach (var detail in manualDetails)
                {
                    var scanQty = detail.EffectiveScanQty;
                    if (scanQty <= 0) continue;
                    totalScanQty += scanQty;
                    _logger.LogInformation("处理拆垛明细: ManualId={ManualId}, BoxCode={BoxCode}, ScanQty={ScanQty}",
                        detail.ManualId, detail.BoxCode, scanQty);
                    await UpdateManualRecordAsync(detail.ManualId, now);
                    var normalizedBoxCode = detail.BoxCode?.Trim();
                    if (!string.IsNullOrWhiteSpace(normalizedBoxCode))
                    {
                        if (exportBoxMap.TryGetValue(normalizedBoxCode, out var exportBox))
                        {
                            exportBox.Status = StatusSuccess;
                            exportBox.PickEdNum = scanQty;
                            exportBox.PickNum = scanQty;
                            exportBox.UpdateTime = now;
                            updatedExportBoxCodes.Add(normalizedBoxCode);
                        }
                        else
                        {
                            await UpdateExportBoxInfoAsync(orderNo, normalizedBoxCode, scanQty, now);
                        }
                    }
                    if (detail.StockInfoId.HasValue && stockInfoMap.TryGetValue(detail.StockInfoId.Value, out var stockInfo))
                    {
                        var qtyInfo = (stockInfo.Qty ?? 0m) - scanQty;
                        if (qtyInfo < 0) throw Oops.Bah("数量异常");
                        if (qtyInfo > 0)
                        {
                            stockInfo.Qty = qtyInfo;
                            stockInfo.UpdateTime = now;
                            stockReduceTracker[stockInfo.Id] = stockInfo;
                        }
                        else
                        {
                            stockInfo.Qty = 0;
                            stockInfo.IsDelete = true;
                            stockInfo.UpdateTime = now;
                            stockDeleteTracker[stockInfo.Id] = stockInfo;
                        }
                    }
                    else
                    {
                        await UpdateStockInfoAsync(detail, scanQty, now);
                    }
                }
                _logger.LogInformation("拆垛明细处理完成，总扫描数量: {TotalScanQty}", totalScanQty);
                await FlushExportBoxUpdatesAsync(updatedExportBoxCodes, exportBoxMap);
                await FlushStockInfoUpdatesAsync(stockReduceTracker.Values, stockDeleteTracker.Values);
                await ReleaseMandatoryBoxesAsync(orderNo, manualDetails, now);
                await UpdateStockTrayAsync(stockTray, exportOrder, totalScanQty, now);
                await UpdateStockRecordAsync(exportOrder, totalScanQty, now);
                await UpdateSlotStatusAsync(exportOrder, now);
                await UpdateExportOrderAsync(exportOrder, totalScanQty, now);
                await UpdateExportNotifyAsync(exportOrder, totalScanQty, now);
                _logger.LogInformation("出库拆垛确认完成: TrayCode={TrayCode}, ExportOrderNo={ExportOrderNo}, TotalScanQty={TotalScanQty}",
                    trayCode, orderNo, totalScanQty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "事务执行过程中发生错误: TrayCode={TrayCode}, ExportOrderNo={ExportOrderNo}", trayCode, orderNo);
                throw;
            }
        });
        if (!tranResult.IsSuccess)
        {
            _logger.LogError("事务提交失败: {ErrorMessage}, TrayCode={TrayCode}, ExportOrderNo={ExportOrderNo}",
                tranResult.ErrorMessage, trayCode, orderNo);
            throw Oops.Oh($"提交失败：{tranResult.ErrorMessage}");
        }
        return CreateLegacyResultObject(1, "提交完成！", string.Empty);
    }
    /// <summary>
    /// 无箱码人工拆垛出库确认。
    /// <para>对应 JC35 接口：【OutSaveManTask】</para>
    /// </summary>
    /// <param name="input">无箱码拆垛确认的请求参数，包含托盘编码、出库流水号、拆垛数量、出库类型等信息</param>
    /// <returns>确认操作结果，包含成功状态、消息和数据</returns>
    /// <exception cref="Exception">当参数验证失败、库存不足、流水已完成或托盘任务未结束时抛出异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>支持自动拆垛（OutType=1）和人工拆垛两种模式</description></item>
    /// <item><description>自动拆垛从任务表反查出库流水号</description></item>
    /// <item><description>验证出库流水是否已完成，已完成则直接返回</description></item>
    /// <item><description>检查托盘出库任务状态，未结束时不能拣货</description></item>
    /// <item><description>验证绑定数量不大于库存数量</description></item>
    /// <item><description>根据拆垛数量与流水数量的关系更新库存：</description></item>
    /// <list type="circle">
    /// <item><description>拆垛数量 > 流水数量：扣减库存数量</description></item>
    /// <item><description>拆垛数量 ≤ 流水数量：回补锁定或库存数量</description></item>
    /// </list>
    /// <item><description>全托出库时清空托盘信息并软删除库存记录</description></item>
    /// <item><description>部分出库时设置差异标识</description></item>
    /// <item><description>更新出库箱码拣货数量和状态</description></item>
    /// <item><description>推进出库流水和通知状态</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaLegacyResult<object>> SaveOutManualTaskAsync(PdaOutManualTaskSaveInput input)
    {
        ValidateInput(input);
        var trayCode = ValidateString(input.TrayCode, "托盘编码");
        var requestOrderNo = ValidateString(input.ExportOrderNo, "出库流水");
        var isAuto = string.Equals(input.OutType?.Trim(), "1", StringComparison.OrdinalIgnoreCase);
        decimal scanQty;
        string resolvedOrderNo = requestOrderNo;
        WmsManualDepalletizing manualRecord = null;
        if (isAuto)
        {
            scanQty = ParseRequiredDecimal(input.ScanQty, "拆垛数量");
            var exportTask = await _exportTaskRep.GetFirstAsync(x => 
                x.ExportTaskNo == resolvedOrderNo && x.IsDelete == false);
            if (exportTask == null || string.IsNullOrWhiteSpace(exportTask.ExportOrderNo))
                throw Oops.Bah("获取绑定物料信息失败！");
            resolvedOrderNo = exportTask.ExportOrderNo.Trim();
        }
        else
        {
            manualRecord = await _manualRep.GetFirstAsync(x => 
                x.TrayCode == trayCode && 
                x.ExportOrderNo == resolvedOrderNo && 
                x.State == StateActive && 
                x.IsDelete == false);
            if (manualRecord == null)
                throw Oops.Bah("请绑定物料！");
            scanQty = ParseRequiredDecimal(manualRecord.ScanQty, "拆垛数量");
        }
        var exportOrder = await _exportOrderRep.GetFirstAsync(x => 
            x.ExportOrderNo == resolvedOrderNo && x.IsDelete == false) ?? throw Oops.Bah($"找不到单号为 {resolvedOrderNo} 的出库流水数据");
        if ((exportOrder.ExportExecuteFlag ?? 0) >= ExportExecuteFlagCompleted)
        {
            await UpdateExportTaskMessageAsync(requestOrderNo, $"流水已完成，流水号：{resolvedOrderNo}");
            return CreateLegacyResultObject(1, "操作成功！", true);
        }
        var exportNotify = await _exportNotifyRep.GetFirstAsync(x => 
            x.ExportBillCode == exportOrder.ExportBillCode && 
            x.IsDelete == false && 
            (x.ExportExecuteFlag ?? 0) <= ExportExecuteFlagCompleted) ?? throw Oops.Bah($"找不到单号为 {exportOrder.ExportBillCode} 的出库单数据，或单据已结束");
        var exportBoxInfo = await _exportBoxInfoRep.GetFirstAsync(x => 
            x.ExportOrderNo == resolvedOrderNo && 
            x.StockCodeCode == trayCode && 
            x.IsDelete == false) ?? throw Oops.Bah("获取出库流水明细信息失败！");
        var stockTray = await _stockTrayRep.AsQueryable()
            .Where(x => x.StockCode == trayCode && x.IsDelete == false)
            .WhereIF(exportOrder.ExportMaterialId.HasValue, x => 
                x.MaterialId == SqlFunc.ToString(exportOrder.ExportMaterialId))
            .WhereIF(!string.IsNullOrWhiteSpace(exportOrder.ExportLotNo), x => 
                x.LotNo == exportOrder.ExportLotNo)
            .WhereIF(exportOrder.InspectionStatus.HasValue, x => 
                x.InspectionStatus == exportOrder.InspectionStatus)
            .FirstAsync() ?? throw Oops.Bah("获取库存托盘信息失败！");
        var availableQty = (stockTray.StockQuantity ?? 0) + (stockTray.LockQuantity ?? 0);
        if (availableQty < scanQty)
            throw Oops.Bah("绑定数量不得大于库存数量！");
        if (!string.IsNullOrWhiteSpace(stockTray.StockSlotCode))
            throw Oops.Bah("托盘出库任务未结束，不能拣货！");
        var stockRecord = await _stockRep.AsQueryable()
            .Where(x => x.WarehouseId == exportOrder.ExportWarehouseId && x.IsDelete == false)
            .WhereIF(exportOrder.ExportMaterialId.HasValue, x => x.MaterialId == exportOrder.ExportMaterialId)
            .WhereIF(!string.IsNullOrWhiteSpace(exportOrder.ExportLotNo), x => x.LotNo == exportOrder.ExportLotNo)
            .WhereIF(exportOrder.InspectionStatus.HasValue, x => x.InspectionStatus == exportOrder.InspectionStatus)
            .FirstAsync() ?? throw Oops.Bah("获取库存信息失败！");
        var trayId = stockTray.Id.ToString();
        var stockInfo = await _stockInfoRep.AsQueryable()
            .Where(x => x.TrayId == trayId && x.IsDelete == false)
            .WhereIF(!string.IsNullOrWhiteSpace(stockTray.MaterialId), x => x.MaterialId == stockTray.MaterialId)
            .WhereIF(!string.IsNullOrWhiteSpace(stockTray.LotNo), x => x.LotNo == stockTray.LotNo)
            .FirstAsync() ?? throw Oops.Bah("获取库存箱码信息失败！");
        var now = DateTime.Now;
        try
        {
            var result = await _sqlSugarClient.Ado.UseTranAsync(async () =>
            {
                if (manualRecord != null)
                {
                    await UpdateManualRecordAsync(manualRecord.Id, now);
                }
                var pickedQty = exportOrder.PickedNum ?? 0m;
                var trayStockQty = stockTray.StockQuantity ?? 0m;
                var trayLockQty = stockTray.LockQuantity ?? 0m;
                var stockQty = stockRecord.StockQuantity ?? 0m;
                var stockLockQty = stockRecord.LockQuantity ?? 0m;
                if (scanQty > pickedQty)
                {
                    var newQty = Math.Max(0, trayStockQty - (scanQty - pickedQty));
                    stockTray.StockQuantity = newQty;
                    stockInfo.Qty = newQty;
                }
                else
                {
                    var remainder = pickedQty - scanQty;
                    stockTray.StockQuantity = trayStockQty + remainder;
                    var infoLockQty = stockInfo.LockQuantity ?? 0m;
                    if (infoLockQty > 0)
                    {
                        stockInfo.LockQuantity = Math.Max(0, infoLockQty - scanQty);
                    }
                    else
                    {
                        stockInfo.Qty = Math.Max(0, (stockInfo.Qty ?? 0m) - scanQty);
                    }
                }
                stockTray.LockQuantity = Math.Max(0, trayLockQty - pickedQty);
                stockTray.UpdateTime = now;
                var lockDiff = stockLockQty - scanQty;
                var ss = scanQty - pickedQty;
                if (lockDiff >= 0)
                {
                    stockRecord.LockQuantity = lockDiff;
                    stockRecord.StockQuantity = Math.Max(0, stockQty - ss);
                }
                else
                {
                    stockRecord.LockQuantity = 0;
                    stockRecord.StockQuantity = Math.Max(0, stockQty + lockDiff);
                }
                stockRecord.UpdateTime = now;
                var shouldRemoveTray = (stockTray.StockQuantity ?? 0) <= 0;
                if (shouldRemoveTray)
                {
                    var hasPendingOrder = await _exportOrderRep.AsQueryable()
                        .Where(p => p.ExportStockCode == trayCode &&
                                    p.ExportMaterialId == exportOrder.ExportMaterialId &&
                                    p.ExportLotNo == exportOrder.ExportLotNo &&
                                    p.InspectionStatus == exportOrder.InspectionStatus &&
                                    (p.ExportExecuteFlag ?? 0) <= 1 &&
                                    p.ExportOrderNo != resolvedOrderNo &&
                                    p.IsDelete == false).AnyAsync();
                    if (hasPendingOrder)
                    {
                        throw Oops.Bah("还有未出库单据不能全出！");
                    }
                    stockTray.StockSlotCode = string.Empty;
                    stockTray.StockQuantity = 0;
                    stockTray.LockQuantity = 0;
                    stockTray.IsDelete = true;
                    stockInfo.Qty = 0;
                    stockInfo.LockQuantity = 0;
                    stockInfo.IsDelete = true;
                    stockInfo.UpdateTime = now;
                    if ((stockRecord.StockQuantity ?? 0) <= 0 && (stockRecord.LockQuantity ?? 0) <= 0)
                    {
                        stockRecord.IsDelete = true;
                    }
                    await _stockTrayRep.AsUpdateable(stockTray)
                        .UpdateColumns(x => new { x.StockSlotCode, x.StockQuantity, x.LockQuantity, x.IsDelete, x.UpdateTime })
                        .ExecuteCommandAsync();
                    await _stockInfoRep.AsUpdateable(stockInfo)
                        .UpdateColumns(x => new { x.Qty, x.LockQuantity, x.IsDelete, x.UpdateTime })
                        .ExecuteCommandAsync();
                    await _stockRep.AsUpdateable(stockRecord)
                        .UpdateColumns(x => new { x.StockQuantity, x.LockQuantity, x.IsDelete, x.UpdateTime })
                        .ExecuteCommandAsync();
                    var hasOtherTray = await _stockTrayRep.AsQueryable()
                        .Where(x => x.StockCode == stockTray.StockCode && x.Id != stockTray.Id && x.IsDelete == false)
                        .AnyAsync();
                    if (!hasOtherTray)
                    {
                        await _sysStockCodeRep.AsUpdateable()
                            .SetColumns(x => new WmsSysStockCode { Status = 0, UpdateTime = now })
                            .Where(x => x.StockCode == stockTray.StockCode && x.IsDelete == false)
                            .ExecuteCommandAsync();
                    }
                }
                else
                {
                    stockTray.OddMarking = 1;
                    stockInfo.OddMarking = "1";
                    stockInfo.UpdateTime = now;
                    await _stockTrayRep.AsUpdateable(stockTray)
                        .UpdateColumns(x => new { x.StockQuantity, x.LockQuantity, x.OddMarking, x.UpdateTime })
                        .ExecuteCommandAsync();
                    await _stockInfoRep.AsUpdateable(stockInfo)
                        .UpdateColumns(x => new { x.Qty, x.LockQuantity, x.OddMarking, x.UpdateTime })
                        .ExecuteCommandAsync();
                    await _stockRep.AsUpdateable(stockRecord)
                        .UpdateColumns(x => new { x.StockQuantity, x.LockQuantity, x.UpdateTime })
                        .ExecuteCommandAsync();
                }
                exportBoxInfo.PickEdNum = (exportBoxInfo.PickEdNum ?? 0) + scanQty;
                var newPickNum = exportBoxInfo.PickNum ?? 0m;
                if ((exportBoxInfo.IsMustExport ?? 0) == 0)
                {
                    newPickNum += scanQty;
                }
                exportBoxInfo.PickNum = newPickNum;
                exportBoxInfo.Status = 1;
                exportBoxInfo.UpdateTime = now;
                await _exportBoxInfoRep.AsUpdateable(exportBoxInfo)
                    .UpdateColumns(x => new { x.PickEdNum, x.PickNum, x.Status, x.UpdateTime })
                    .ExecuteCommandAsync();
                await UpdateExportOrderAsync(exportOrder, scanQty, now);
                await UpdateExportNotifyAsync(exportOrder, scanQty, now);
            });
            if (!result.IsSuccess)
            {
                await UpdateExportTaskMessageAsync(requestOrderNo, result.ErrorMessage);
                throw Oops.Oh($"提交失败：{result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            await UpdateExportTaskMessageAsync(requestOrderNo, ex.Message);
            throw;
        }
        return CreateLegacyResultObject(1, "操作成功！", true);
    }
    #endregion

    #region 手动拆垛（无箱码）
    /// <summary>
    /// 绑定无箱码拆跺明细。
    /// <para>对应 JC35 接口：【AddOutManualDepalletizing】</para>
    /// </summary>
    /// <param name="input">绑定无箱码拆跺的请求参数，包含出库流水号、托盘编码、扫描数量、出库数量等信息</param>
    /// <returns>绑定操作结果，包含成功状态和消息</returns>
    /// <exception cref="Exception">当参数为空、出库流水不存在、库存托盘或箱码信息无效时抛出异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>验证出库流水号的有效性</description></item>
    /// <item><description>加载对应的库存托盘信息（强制匹配仓库）</description></item>
    /// <item><description>查询托盘下的库存箱码信息</description></item>
    /// <item><description>创建无箱码拆跺记录，BoxCode字段设置为null</description></item>
    /// <item><description>设置数量为库存总数量（库存数量+锁定数量）</description></item>
    /// <item><description>保存拆跺记录到数据库</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> AddOutManualDepalletizingAsync(PdaOutManualDepalletizingInput input)
    {
        ValidateInput(input);
        var exportOrder = await _exportOrderRep.GetFirstAsync(x => 
            x.ExportOrderNo == input.ExportOrderNo && x.IsDelete == false) ?? throw Oops.Bah("获取出库流水信息失败！");
        var stockTray = await LoadExportTrayAsync(exportOrder, expectWarehouseMatch: true) ?? throw Oops.Bah("获取库存托盘信息失败！");
        var trayId = stockTray.Id.ToString();
        var stockInfo = await _stockInfoRep.AsQueryable()
            .Where(x => x.TrayId == trayId)
            .FirstAsync() ?? throw Oops.Bah("没有找到对应物料明细！");
        var manual = new WmsManualDepalletizing
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            BoxCode = null,
            Qty = FormatDecimal(stockInfo.Qty + (stockInfo.LockQuantity ?? 0)),
            ScanQty = FormatDecimal(input.ScanQty),
            TrayCode = input.TrayCode?.Trim(),
            State = StateActive,
            OutQty = input.OutQty,
            ExportOrderNo = input.ExportOrderNo.Trim(),
            IsDelete = false,
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now
        };
        await _manualRep.InsertAsync(manual);
        return CreateSuccessResult("绑定成功");
    }
    /// <summary>
    /// 删除无箱码拆跺记录。
    /// <para>对应 JC35 接口：【RemoveOutManual】</para>
    /// </summary>
    /// <param name="input">删除无箱码拆跺记录的请求参数，必须包含拆跺记录主键ID</param>
    /// <returns>删除操作结果，包含成功状态和消息</returns>
    /// <exception cref="Exception">当参数为空、缺少拆跺记录主键或记录不存在时抛出异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>验证输入参数的完整性</description></item>
    /// <item><description>验证拆跺记录主键ID的存在</description></item>
    /// <item><description>查询活跃状态且未删除的拆跺记录</description></item>
    /// <item><description>将记录标记为已删除（软删除）</description></item>
    /// <item><description>更新记录的修改时间</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> RemoveOutManualAsync(PdaManualRemoveInput input)
    {
        ValidateInput(input);
        if (!input.Id.HasValue)
            throw Oops.Bah("缺少拆跺记录主键！");
        var manual = await _manualRep.AsQueryable()
            .Where(x => x.Id == input.Id.Value && x.State == StateActive && x.IsDelete == false)
            .FirstAsync() ?? throw Oops.Bah("获取拆跺信息失败！");
        manual.IsDelete = true;
        manual.UpdateTime = DateTime.Now;
        await _manualRep.AsUpdateable(manual)
            .UpdateColumns(x => new { x.IsDelete, x.UpdateTime })
            .ExecuteCommandAsync();
        return CreateSuccessResult();
    }
    /// <summary>
    /// 获取无箱码拆跺信息。
    /// <para>对应 JC35 接口：【GetOutManualDepalletizing】</para>
    /// </summary>
    /// <param name="input">查询无箱码拆跺信息的请求参数，必须包含托盘编码和出库流水号</param>
    /// <returns>无箱码拆跺信息列表，包含物料信息、数量、托盘编码等详细信息</returns>
    /// <exception cref="Exception">当参数为空时抛出异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>从拆跺视图查询无箱码拆跺信息（GroupQuantity > 0）</description></item>
    /// <item><description>查询对应的手动拆跺记录</description></item>
    /// <item><description>组合视图数据和手动记录数据</description></item>
    /// <item><description>返回格式化的拆跺记录列表，BoxCode字段为null</description></item>
    /// <item><description>包含物料名称、编码、批号、出库数量等完整信息</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaLegacyResult<List<PdaManualDepalletizingItem>>> GetOutManualDepalletizingAsync(PdaManualDepalletizingQueryInput input)
    {
        ValidateInput(input);
        var trayCode = ValidateString(input.TrayCode, "托盘编码");
        var orderNo = ValidateString(input.ExportOrderNo, "出库流水");
        var list = await _sqlViewService.QueryManualDepalletizingView()
            .MergeTable()
            .Where(x => x.ExportOrderNo == orderNo && x.StockCode == trayCode && x.GroupQuantity > 0)
            .ToListAsync();
        var manualRecord = await _manualRep.AsQueryable()
            .Where(x => x.TrayCode == trayCode && 
                x.ExportOrderNo == orderNo && 
                x.State == StateActive && 
                x.IsDelete == false)
            .FirstAsync();
        
        if (manualRecord == null)
            return CreateLegacyResult<List<PdaManualDepalletizingItem>>(1, "操作成功！", new List<PdaManualDepalletizingItem>(), 0);
        var result = list.Select(item => new PdaManualDepalletizingItem
        {
            BoxCode = null,
            Qty = item.Qty,
            ScanQty = item.GroupQuantity,
            TrayCode = item.StockCode,
            MaterialName = item.ExportMaterialName,
            MaterialCode = item.MaterialCode,
            StockLotNo = item.LotNo,
            Id = manualRecord.Id.ToString(),
            OutQty = manualRecord.OutQty?.ToString()
        }).ToList();
        return CreateLegacyResult<List<PdaManualDepalletizingItem>>(1, "操作成功！", result, result.Count);
    }
    #endregion

    #region 空托出库
    /// <summary>
    /// 空托盘出库申请（PDA专用）。
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
        ValidateInput(input);
        if (input.Num <= 0)
            throw Oops.Bah("申请数量必须大于 0！");
        var exportPort = ValidateString(input.ExportPort, "出库口");
        var houseCode = ResolvePdaHouseCode(exportPort, input.HouseCode);
        if (string.IsNullOrWhiteSpace(houseCode))
            return CreateLegacyResultObject(0, $"出库口 {exportPort} 未配置所属仓库类型！", string.Empty);
        var emptyTrayMaterialId = await GetPdaEmptyTrayMaterialIdAsync();
        if (emptyTrayMaterialId == null)
            return CreateLegacyResultObject(0, "未维护对应载具！", string.Empty);
        
        var (resolved, slotAreaId, warehouseId) = await TryResolveSlotAreaAsync(exportPort);
        if (!resolved)
            return CreateLegacyResultObject(0, $"未识别的出库口：{exportPort}，请检查出库口配置或储位信息", string.Empty);
        
        var candidates = await LoadPdaEmptyTrayCandidatesAsync(emptyTrayMaterialId.Value, slotAreaId);
        if (candidates.Count < input.Num)
            return CreateLegacyResultObject(0, $"库内空托数量为:{candidates.Count}", string.Empty);
        var now = DateTime.Now;
        var selectedTasks = new List<WmsExportTask>();
        var payload = new List<ExportLibraryDTO>();
        var slotUpdates = new List<(long SlotId, int Status)>();
        var taskNos = new List<string>();
        string lastTaskNo = null;
        var processed = 0;
        foreach (var candidate in candidates)
        {
            if (processed >= input.Num) break;
            if (candidate.SlotInout.GetValueOrDefault() != 1)
            {
                if (candidate.SlotLanewayId.HasValue &&
                    candidate.SlotRow.HasValue &&
                    candidate.SlotColumn.HasValue &&
                    candidate.SlotLayer.HasValue)
                {
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
                    if (frontSlots.Any(status => status == 1 || status == 2))
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
            }
            var taskNo = string.IsNullOrEmpty(lastTaskNo) ? _commonMethod.GetImExNo("CKR") : IncrementTaskNo(lastTaskNo);
            lastTaskNo = taskNo;
            taskNos.Add(taskNo);
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
            slotUpdates.Add((candidate.SlotId, 3));
            var boxInfos = await BuildStockBoxInfosAsync(candidate);
            var qty = Math.Max(0, candidate.LockQuantity.GetValueOrDefault() > 0 
                ? candidate.LockQuantity.GetValueOrDefault() 
                : candidate.StockQuantity.GetValueOrDefault());
            var houseCodePayload = !string.IsNullOrWhiteSpace(candidate.SlotCode) && candidate.SlotCode.Length >= 1
                ? candidate.SlotCode.Substring(0, 1)
                : houseCode;
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
        if (processed == 0)
            return CreateLegacyResultObject(0, "空载具通道被占用，当前无法下发任务！", string.Empty);
        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                await _sqlSugarClient.Insertable(selectedTasks).ExecuteCommandAsync();
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
        var response = await SendPdaEmptyTrayTasksAsync(payload);
        var (success, message) = EvaluatePdaEmptyTrayResponse(response);
        if (success)
        {
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
        return CreateLegacyResultObject(0, 
            string.IsNullOrWhiteSpace(message) ? "空托申请失败！" : message, 
            string.Empty);
    }

    /// <summary>
    /// 加载各巷道出库口列表。
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
        var list = await _sqlSugarClient.Queryable<SysDictType>()
            .InnerJoin<SysDictData>((dictType, dictData) => dictData.DictTypeId == dictType.Id && dictType.Code == "ExportPort" && dictData.Status == StatusEnum.Enable)
            .OrderBy((dictType, dictData) => dictData.Code)
            .Select((dictType, dictData) => new PdaExportPortItem
            {
                Id = SqlFunc.ToString(dictData.Id),
                Code = dictData.Code ?? dictData.Value,
                TypeName = dictData.Label ?? dictData.Name
            })
            .ToListAsync();
        return CreateLegacyResult<List<PdaExportPortItem>>(0, list.Count > 0 ? "查询成功" : "暂无数据", list, list.Count);
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 根据出库流水加载对应库存托盘。
    /// </summary>
    /// <param name="exportOrder">出库流水信息，包含托盘编码、物料ID、批号等</param>
    /// <param name="expectWarehouseMatch">是否强制仓库一致，默认为false</param>
    /// <returns>匹配的库存托盘信息，未找到时返回null</returns>
    /// <remarks>
    /// 查询条件：
    /// <list type="bullet">
    /// <item><description>托盘编码必须匹配出库流水的ExportStockCode</description></item>
    /// <item><description>批号必须匹配出库流水的ExportLotNo</description></item>
    /// <item><description>如果提供了物料ID，则必须匹配</description></item>
    /// <item><description>如果提供了检验状态，则必须匹配</description></item>
    /// <item><description>可选：仓库ID必须匹配（expectWarehouseMatch=true时）</description></item>
    /// </list>
    /// </remarks>
    private async Task<WmsStockTray> LoadExportTrayAsync(WmsExportOrder exportOrder, bool expectWarehouseMatch = false)
    {
        if (exportOrder == null) return null;
        var query = _stockTrayRep.AsQueryable()
            .Where(x => x.StockCode == exportOrder.ExportStockCode && x.LotNo == exportOrder.ExportLotNo);
        if (exportOrder.ExportMaterialId.HasValue)
        {
            query = query.Where(x => x.MaterialId == exportOrder.ExportMaterialId.Value.ToString());
        }
        if (exportOrder.InspectionStatus.HasValue)
        {
            query = query.Where(x => x.InspectionStatus == exportOrder.InspectionStatus.Value);
        }
        if (expectWarehouseMatch && exportOrder.ExportWarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == exportOrder.ExportWarehouseId.Value.ToString());
        }
        return await query.FirstAsync();
    }

    /// <summary>
    /// 获取 PDA 空托申请所需的空托盘物料 ID（通过 IsEmpty 字段判断）。
    /// </summary>
    /// <remarks>
    /// 通过查询物料表的 IsEmpty 字段判断空托盘物料；若未维护该物料，返回 <c>null</c> 并提示前端。
    /// </remarks>
    private async Task<long?> GetPdaEmptyTrayMaterialIdAsync()
    {
        return await _materialRep.AsQueryable()
            .Where(x => x.IsEmpty == true && !x.IsDelete)
            .Select(x => (long?)x.Id)
            .FirstAsync();
    }

    /// <summary>
    /// 动态解析出库口对应的库区ID和仓库ID。
    /// </summary>
    /// <param name="exportPort">出库口代码（如 "C01"）。</param>
    /// <returns>解析结果，成功返回 (true, slotAreaId, warehouseId)，失败返回 (false, 0, 0)。</returns>
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
        var slot = await _baseSlotRep.GetFirstAsync(s =>
            s.SlotCode == normalizedPort &&
            !s.IsDelete &&
            s.SlotAreaId.HasValue &&
            s.WarehouseId.HasValue);
        if (slot != null && slot.SlotAreaId.HasValue && slot.WarehouseId.HasValue)
        {
            return (true, slot.SlotAreaId.Value, slot.WarehouseId.Value);
        }
        var area = await _sqlSugarClient.Queryable<WmsBaseArea>()
            .Where(a => a.AreaCode == normalizedPort && !a.IsDelete && a.AreaWarehouseId.HasValue)
            .FirstAsync();
        if (area != null && area.AreaWarehouseId.HasValue)
        {
            return (true, area.Id, area.AreaWarehouseId.Value);
        }
        return (false, 0, 0);
    }

    /// <summary>
    /// 按 JC35 规则加载空托盘候选集合。
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
        var materialIdStr = materialId.ToString();//设置物料ID  
        return await _sqlSugarClient.Queryable<WmsStockTray, WmsBaseSlot>((tray, slot) => new JoinQueryInfos(
                JoinType.Inner, tray.StockSlotCode == slot.SlotCode))
            .Where((tray, slot) =>
                tray.MaterialId == materialIdStr && SqlFunc.Coalesce(tray.StockStatusFlag, 0) == 0 && !tray.IsDelete && SqlFunc.Coalesce(slot.SlotStatus, 0) != 3 && SqlFunc.Coalesce(slot.SlotStatus, 0) != 5 && SqlFunc.Coalesce(slot.SlotStatus, 0) != 7 && SqlFunc.Coalesce(slot.SlotStatus, 0) != 8 && slot.SlotAreaId == slotAreaId && !slot.IsDelete)//根据物料ID和库区ID查询
            .OrderBy((tray, slot) => slot.SlotInout, OrderByType.Asc)//按进出方向排序   
            .OrderBy((tray, slot) => slot.SlotRow, OrderByType.Asc)//按行排序   
            .OrderBy((tray, slot) => slot.SlotColumn, OrderByType.Asc)//按列排序   
            .OrderBy((tray, slot) => slot.SlotLayer, OrderByType.Asc)//按层排序   
            .Select((tray, slot) => new PdaEmptyTrayCandidate//创建空托盘候选集合
            {
                TrayId = tray.Id,//设置托盘ID
                TrayCode = tray.StockCode,//设置托盘编码
                VehicleSubId = tray.VehicleSubId,//设置车辆子ID
                LockQuantity = tray.LockQuantity,//设置锁定数量
                StockQuantity = tray.StockQuantity,//设置库存数量
                SlotId = slot.Id,//设置储位ID
                SlotCode = slot.SlotCode,//设置储位编码
                SlotInout = slot.SlotInout,//设置进出方向
                SlotLanewayId = slot.SlotLanewayId,//设置通道ID
                SlotRow = slot.SlotRow,//设置行
                SlotColumn = slot.SlotColumn,//设置列
                SlotLayer = slot.SlotLayer,//设置层
                WarehouseId = slot.WarehouseId,//设置仓库ID
                EndTransitLocation = slot.EndTransitLocation//设置结束位置
            })
            .Take(200) // 足量候选，循环中再过滤
            .ToListAsync();
    }

    /// <summary>
    /// 构建与 JC35 完全一致的 <see cref="StockBoxInfo"/> 明细集合。
    /// </summary>
    /// <param name="candidate">当前选中的空托盘。</param>
    /// <returns>托盘上的箱码清单，供 WCS 报文使用。</returns>
    private async Task<List<StockBoxInfo>> BuildStockBoxInfosAsync(PdaEmptyTrayCandidate candidate)
    {
        var trayId = candidate.TrayId.ToString();//设置托盘ID   
        var list = await _sqlSugarClient.Queryable<WmsStockInfo, WmsBaseMaterial>((info, material) => new JoinQueryInfos(
                JoinType.Left, info.MaterialId == SqlFunc.ToString(material.Id)))//根据物料ID和材料ID查询
            .Where((info, material) => info.TrayId == trayId && (info.IsDelete == null || info.IsDelete == false))//根据托盘ID和是否删除查询
            .Select((info, material) => new//创建库存箱码信息
            {
                info.BoxCode,//设置箱码
                info.Qty,//设置数量
                MaterialCode = material.MaterialCode,//设置物料编码
                info.ProductionDate,//设置生产日期
                info.ValidateDay,//设置有效期
                info.LotNo,//设置批次号
                info.SamplingDate,//设置采样日期
                info.StaffCode,//设置员工编码
                info.StaffName,//设置员工姓名   
                info.PickingSlurry,//设置拣选泥浆
                info.ExtractStatus,//设置提取状态
                info.InspectionStatus,//设置检验状态
                info.RFIDCode//设置RFID码
            }).ToListAsync();
        var result = new List<StockBoxInfo>();
        foreach (var item in list)
        {
            result.Add(new StockBoxInfo//创建库存箱码信息
            {
                BoxCode = item.BoxCode,//设置箱码
                Qty = item.Qty ?? 0,//设置数量
                MaterialCode = item.MaterialCode ?? string.Empty,//设置物料编码
                ProductionDate = item.ProductionDate?.ToString("yyyy-MM-dd") ?? string.Empty,//设置生产日期
                ValidateDay = item.ValidateDay?.ToString("yyyy-MM-dd") ?? string.Empty,//设置有效期
                LotNo = item.LotNo ?? string.Empty,//设置批次号
                BulkTank = 0,//设置油罐
                RFIDCode = item.RFIDCode ?? string.Empty,//设置RFID码
                SamplingDate = item.SamplingDate?.ToString("yyyy-MM-dd") ?? string.Empty,//设置采样日期
                StaffCode = item.StaffCode ?? string.Empty,//设置员工编码
                StaffName = item.StaffName ?? string.Empty,//设置员工姓名   
                PickingSlurry = item.PickingSlurry ?? "0",//设置拣选泥浆
                ExtractStatus = (item.ExtractStatus ?? 0).ToString(),//设置提取状态
                InspectionStatus = (item.InspectionStatus ?? 0).ToString()//设置检验状态
            });
        }
        return result;
    }

    /// <summary>
    /// 生成下一个 <c>CKR</c> 出库任务号，保持 JC35 的"序列 + 递增"模式。
    /// </summary>
    /// <param name="lastTaskNo">上一条任务号。</param>
    /// <returns>递增后的任务号；格式不正确时原样返回。</returns>
    private static string IncrementTaskNo(string lastTaskNo)
    {
        if (string.IsNullOrWhiteSpace(lastTaskNo) || lastTaskNo.Length <= 3)//判断任务号是否为空或长度小于3
        {
            return lastTaskNo;//返回任务号
        }
        var prefix = lastTaskNo[..3];//设置前缀
        var numeric = lastTaskNo[3..];//设置数字
        if (!long.TryParse(numeric, out var number))
        {
            number = 0;//设置数字
        }
        number++;
        return prefix + number.ToString().PadLeft(numeric.Length, '0');//返回任务号
    }

    /// <summary>
    /// 调用 WCS 接口下发空托任务；若未配置 WCS 地址则返回模拟成功。
    /// </summary>
    /// <param name="payload">需要下发的任务集合。</param>
    /// <returns>WCS 原始响应字符串。</returns>
    private Task<string> SendPdaEmptyTrayTasksAsync(List<ExportLibraryDTO> payload)
    {
        if (payload == null || payload.Count == 0)//判断任务集合是否为空
        {
            return Task.FromResult(string.Empty);//返回空
        }
        var jsonData = JsonConvert.SerializeObject(payload);//序列化任务集合
        var host = WcsApiUrlDto.GetHost();//获取WCS地址
        if (string.IsNullOrWhiteSpace(host))//判断WCS地址是否为空
        {
            _logger.LogWarning("WCS 地址未配置，模拟成功返回。Payload：{Payload}", jsonData);//记录警告日志
            return Task.FromResult("{\"stateCode\":\"1\",\"errMsg\":\"WCS 地址未配置，模拟成功返回\"}");
        }
        var requestUrl = host.TrimEnd('/') + WcsApiUrlDto.TaskApiUrl;//设置请求URL
        _logger.LogInformation("PDA 空托申请 -> WCS：{Url}，Payload：{Payload}", requestUrl, jsonData);
        // 目前 WCS 通道未对接，返回模拟成功结果，保持与 JC35 行为一致。
        return Task.FromResult("{\"stateCode\":\"1\",\"errMsg\":\"模拟WCS调用成功\"}");//返回模拟成功结果
    }

    /// <summary>
    /// 解析 WCS 回执并转换成 PDA 可识别的成功标识与提示语。
    /// </summary>
    /// <param name="response">WCS 返回的 JSON。</param>
    /// <returns>布尔成功标记与提示消息。</returns>
    private (bool Success, string Message) EvaluatePdaEmptyTrayResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))//判断响应是否为空
        {
            return (false, "WCS 未返回数据");//返回失败结果
        }
        try
        {
            var model = JsonConvert.DeserializeObject<WcsTaskResponse>(response);//反序列化响应
            if (model == null)
            {
                return (false, "WCS 响应解析失败");//返回失败结果
            }
            var success = !string.Equals(model.stateCode, "0", StringComparison.OrdinalIgnoreCase);//判断是否成功
            var message = string.IsNullOrWhiteSpace(model.errMsg)
                ? (success ? "已下发空托出库指令！" : "下发空托出库指令失败！")
                : model.errMsg;//设置消息
            return (success, message);
        }
        catch (Exception ex)
        {
            return (false, $"WCS 响应解析异常：{ex.Message}");
        }
    }

    /// <summary>
    /// PDA 空托申请的仓库类型解析逻辑。        
    /// </summary>
    /// <param name="exportPort">出库口。</param>
    /// <param name="overrideHouseCode">覆盖仓库代码。</param>
    /// <returns>仓库代码。</returns>
    private static string ResolvePdaHouseCode(string exportPort, string overrideHouseCode)
    {
        if (!string.IsNullOrWhiteSpace(overrideHouseCode))//判断仓库代码是否存在
        {
            return overrideHouseCode.Trim().Substring(0, 1).ToUpperInvariant();//返回仓库代码
        }
        if (string.IsNullOrWhiteSpace(exportPort))//判断出库口是否为空
        {
            return null;//返回空
        }
        var prefix = char.ToUpperInvariant(exportPort[0]);//设置前缀
        return prefix switch
        {
            'A' => "A",//返回A
            'B' => "B",//返回B
            'C' => "C",//返回C
            _ => null//返回空
        };
    }

    

    /// <summary>
    /// 将视图数据映射为 PDA 拆垛箱码明细。
    /// </summary>
    /// <param name="view">视图数据。</param>
    /// <returns>拆垛箱码明细。</returns>
    private static PdaManualBoxInfoDetail MapManualBoxInfo(ViewManualDepalletizing view)
    {
        if (view == null)
        {
            return null;//返回空
        }
        return new PdaManualBoxInfoDetail//创建拆垛箱码明细
        {
            ExportMaterialName = view.ExportMaterialName,//设置物料名称
            BoxCode = view.BoxCode,//设置箱码
            LotNo = view.LotNo,//设置批次号
            Qty = view.Qty,//设置数量
            LockQuantity = view.LockQuantity,//设置锁定数量
            OutQty = view.OutQty,//设置出库数量
            ToutQty = view.ToutQty,//设置出库数量
            StockSlotCode = view.StockSlotCode,//设置储位编码
            StockCode = view.StockCode,//设置托盘编码
            ExportQuantity = view.ExportQuantity,//设置出库数量
            StockQuantity = view.StockQuantity,//设置库存数量
            ExportOrderNo = view.ExportOrderNo,//设置出库流水号
            GroupQuantity = view.GroupQuantity,//设置组数量
            QRCode = view.QRCode,//设置二维码
            OrderInspectionStatus = view.OrderInspectionStatus,//设置检验状态
            TrayInspectionStatus = view.TrayInspectionStatus,//设置托盘检验状态
            ExportExecuteFlag = view.ExportExecuteFlag,//设置执行标志
            TrayId = view.TrayId,//设置托盘ID
            Id = view.Id,//设置ID
            MaterialCode = view.MaterialCode,//设置物料编码
        };
    }

    /// <summary>
    /// 构建人工拆垛确认所需的明细集合。
    /// </summary>
    /// <param name="trayCode">托盘编码</param>
    /// <param name="exportOrderNo">出库流水号</param>
    /// <returns>拆垛记录、库存箱码、物料信息的聚合结果</returns>
    /// <remarks>
    /// 关联查询：
    /// <list type="bullet">
    /// <item><description>WmsManualDepalletizing - 人工拆垛记录（主表）</description></item>
    /// <item><description>WmsStockInfo - 库存箱码信息（左连接）</description></item>
    /// <item><description>WmsStockTray - 库存托盘信息（左连接）</description></item>
    /// <item><description>WmsExportBoxInfo - 出库箱码信息（左连接）</description></item>
    /// <item><description>WmsBaseMaterial - 物料基础信息（左连接）</description></item>
    /// </list>
    /// 筛选条件：
    /// <list type="bullet">
    /// <item><description>托盘编码和出库流水号必须匹配</description></item>
    /// <item><description>只查询活跃状态且未删除的拆垛记录</description></item>
    /// </list>
    /// </remarks>
    private async Task<List<ManualTaskDetail>> LoadManualTaskDetailsAsync(string trayCode, string exportOrderNo)
    {
        return await _sqlSugarClient.Queryable<WmsManualDepalletizing, WmsStockInfo, WmsStockTray, WmsExportBoxInfo, WmsBaseMaterial>(//查询人工拆垛确认所需的明细集合
                (manual, info, tray, box, material) => new JoinQueryInfos(
                    JoinType.Left, manual.BoxCode == info.BoxCode,//左连接
                    JoinType.Left, info.TrayId == SqlFunc.ToString(tray.Id),//左连接
                    JoinType.Left, manual.BoxCode == box.BoxCode && manual.ExportOrderNo == box.ExportOrderNo,//左连接
                    JoinType.Left, box.MaterialId == SqlFunc.ToString(material.Id)))//左连接
            .Where((manual, info, tray, box, material) => manual.TrayCode == trayCode && manual.ExportOrderNo == exportOrderNo && manual.State == StateActive && manual.IsDelete == false)
            .Select((manual, info, tray, box, material) => new ManualTaskDetail //创建人工拆垛确认所需的明细集合
            {
                ManualId = manual.Id,//设置ID
                BoxCode = manual.BoxCode,//设置箱码
                ManualQtyRaw = manual.Qty,//设置数量
                ManualScanQtyRaw = manual.ScanQty,//设置扫描数量
                StockInfoId = info.Id,//设置库存箱码ID
                StockQty = info.Qty,//设置库存数量
                StockLockQty = info.LockQuantity,//设置锁定数量
                TrayId = info.TrayId,//设置托盘ID
                MaterialCode = material.MaterialCode,//设置物料编码
                MaterialId = box.MaterialId,//设置物料ID
                StockLotNo = SqlFunc.IIF(SqlFunc.IsNullOrEmpty(info.LotNo), tray.LotNo, info.LotNo),//设置批次号
                ManualOutQty = manual.OutQty//设置出库数量
            }).ToListAsync();
    }

    /// <summary>
    /// 批量加载拆垛明细关联的库存箱码信息，避免在事务过程中重复查询。
    /// </summary>
    /// <param name="manualDetails">拆垛明细集合。</param>
    /// <returns>以 <c>StockInfoId</c> 为键的库存字典。</returns>
    private async Task<Dictionary<long, WmsStockInfo>> LoadStockInfoMapAsync(IEnumerable<ManualTaskDetail> manualDetails)
    {
        var stockIds = manualDetails?
            .Where(x => x.StockInfoId.HasValue)
            .Select(x => x.StockInfoId.Value)
            .Distinct()
            .ToList() ?? new List<long>();

        if (stockIds.Count == 0)
        {
            return new Dictionary<long, WmsStockInfo>();
        }

        var stockInfos = await _stockInfoRep.AsQueryable()
            .Where(x => stockIds.Contains(x.Id) && (x.IsDelete == false || x.IsDelete == null))
            .ToListAsync();

        return stockInfos.ToDictionary(x => x.Id);
    }

    /// <summary>
    /// 批量加载拆垛明细对应的出库箱码，提升后续状态更新效率。
    /// </summary>
    /// <param name="exportOrderNo">出库流水号。</param>
    /// <param name="manualDetails">拆垛明细集合。</param>
    /// <returns>根据箱码分组的出库箱码信息。</returns>
    private async Task<Dictionary<string, WmsExportBoxInfo>> LoadExportBoxInfoMapAsync(string exportOrderNo, IEnumerable<ManualTaskDetail> manualDetails)
    {
        var boxCodes = manualDetails?
            .Select(x => x.BoxCode?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        if (boxCodes.Count == 0)
        {
            return new Dictionary<string, WmsExportBoxInfo>(StringComparer.OrdinalIgnoreCase);
        }

        var exportBoxes = await _exportBoxInfoRep.AsQueryable()
            .Where(x => x.ExportOrderNo == exportOrderNo && boxCodes.Contains(x.BoxCode) && x.IsDelete == false)
            .ToListAsync();

        return exportBoxes.ToDictionary(x => x.BoxCode, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 将已修改的出库箱码一次性落库，保证与拆垛事务一致。
    /// </summary>
    /// <param name="updatedBoxCodes">需要更新的箱码集合。</param>
    /// <param name="exportBoxMap">箱码到出库信息的映射。</param>
    private async Task FlushExportBoxUpdatesAsync(IEnumerable<string> updatedBoxCodes, IDictionary<string, WmsExportBoxInfo> exportBoxMap)
    {
        if (exportBoxMap == null || updatedBoxCodes == null)
        {
            return;
        }

        var boxesToUpdate = new List<WmsExportBoxInfo>();
        foreach (var code in updatedBoxCodes)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                continue;
            }
            if (exportBoxMap.TryGetValue(code, out var exportBox))
            {
                boxesToUpdate.Add(exportBox);
            }
        }

        if (boxesToUpdate.Count > 0)
        {
            await _exportBoxInfoRep.AsUpdateable(boxesToUpdate)
                .UpdateColumns(x => new { x.Status, x.PickEdNum, x.PickNum, x.UpdateTime })
                .ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 将累计的库存扣减或删除操作批量提交，降低数据库压力。
    /// </summary>
    /// <param name="reduceItems">仅需扣减数量的库存记录。</param>
    /// <param name="deleteItems">需要标记删除的库存记录。</param>
    private async Task FlushStockInfoUpdatesAsync(IEnumerable<WmsStockInfo> reduceItems, IEnumerable<WmsStockInfo> deleteItems)
    {
        var reduceList = reduceItems?.ToList();
        var deleteList = deleteItems?.ToList();

        if (reduceList != null && reduceList.Count > 0)
        {
            await _stockInfoRep.AsUpdateable(reduceList)
                .UpdateColumns(x => new { x.Qty, x.UpdateTime })
                .ExecuteCommandAsync();
        }

        if (deleteList != null && deleteList.Count > 0)
        {
            await _stockInfoRep.AsUpdateable(deleteList)
                .UpdateColumns(x => new { x.Qty, x.IsDelete, x.UpdateTime })
                .ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 将指定人工拆垛记录标记为已处理。
    /// </summary>
    /// <param name="manualId">人工拆垛ID。</param>
    /// <param name="now">当前时间。</param>
    private async Task UpdateManualRecordAsync(long manualId, DateTime now)
    {
        var manual = new WmsManualDepalletizing //创建人工拆垛记录
        {
            Id = manualId,
            State = 1,
            UpdateTime = now
        };
        await _manualRep.AsUpdateable(manual).UpdateColumns(x => new { x.State, x.UpdateTime }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新出库流水箱码的拣货状态与数量。
    /// </summary>
    /// <param name="exportOrderNo">出库流水号。</param>
    /// <param name="boxCode">箱码。</param>
    /// <param name="scanQty">扫描数量。</param>
    /// <param name="now">当前时间。</param>
    private async Task UpdateExportBoxInfoAsync(string exportOrderNo, string boxCode, decimal scanQty, DateTime now)
    {
        var exportBox = await _exportBoxInfoRep.GetFirstAsync(x => x.ExportOrderNo == exportOrderNo && x.BoxCode == boxCode && x.IsDelete == false);//查询出库流水箱码;
        if (exportBox == null)//判断出库流水箱码是否存在
        {
            return;
        }
        exportBox.Status = 1;//设置拣货状态
        exportBox.PickEdNum = scanQty;//设置拣货数量
        exportBox.PickNum = scanQty;//设置拣货数量
        exportBox.UpdateTime = now;//设置更新时间
        await _exportBoxInfoRep.AsUpdateable(exportBox).UpdateColumns(x => new { x.Status, x.PickEdNum, x.PickNum, x.UpdateTime }).ExecuteCommandAsync();//更新出库流水箱码
    }

    /// <summary>
    /// 扣减或删除 `WmsStockInfo` 中的库存箱码。
    /// <para>对照原接口逻辑：只更新 Qty，如果 qtyInfo 小于等于 0 则删除记录，不更新 LockQuantity。</para>
    /// </summary>
    /// <param name="detail">人工拆垛确认所需的明细。</param>
    /// <param name="scanQty">扫描数量。</param>
    /// <param name="now">当前时间。</param>
    private async Task UpdateStockInfoAsync(ManualTaskDetail detail, decimal scanQty, DateTime now)
    {
        if (!detail.StockInfoId.HasValue)//判断库存箱码ID是否存在
        {
            return;
        }
        var stockInfo = await _stockInfoRep.GetFirstAsync(x => x.Id == detail.StockInfoId.Value && (x.IsDelete == false || x.IsDelete == null));//查询库存箱码
        if (stockInfo == null)//判断库存箱码是否存在
        {
            return;
        }
        // 对照原接口逻辑：var qtyInfo = (decimal)stockInfo.Qty - Convert.ToDecimal(Item.ScanQty);
        var qtyInfo = (stockInfo.Qty ?? 0m) - scanQty;//设置剩余数量
        if (qtyInfo < 0)//判断剩余数量是否小于0
        {
            throw Oops.Bah("数量异常");//抛出异常
        }
        if (qtyInfo > 0)//如果箱码库存数量减去出库数量后仍然有剩余，不删除库存表
        {
            stockInfo.Qty = qtyInfo;//设置数量
            stockInfo.UpdateTime = now;//设置更新时间
            await _stockInfoRep.AsUpdateable(stockInfo)
                .UpdateColumns(x => new { x.Qty, x.UpdateTime })//更新库存箱码
                .ExecuteCommandAsync();
        }
        else//否则删除记录（对照原接口：DataContext.WmsStockInfo.DeleteOnSubmit）
        {
            stockInfo.Qty = 0;//设置数量
            stockInfo.IsDelete = true;//设置删除标志
            stockInfo.UpdateTime = now;//设置更新时间
            await _stockInfoRep.AsUpdateable(stockInfo)
                .UpdateColumns(x => new { x.Qty, x.IsDelete, x.UpdateTime })//更新库存箱码
                .ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 若存在"必出箱"但本次 PDA 未扫描，则恢复对应锁定库存。
    /// </summary>
    /// <param name="exportOrderNo">出库流水号。</param>
    /// <param name="confirmedBoxes">已确认箱码。</param>
    /// <param name="now">当前时间。</param>
    private async Task ReleaseMandatoryBoxesAsync(string exportOrderNo, IEnumerable<ManualTaskDetail> confirmedBoxes, DateTime now)
    {
        var mandatoryBoxes = await _sqlViewService.QueryExportBoxInfoView()//查询必出箱（使用视图）
            .MergeTable()
            .Where(x => x.ExportOrderNo == exportOrderNo && x.PickNum != null && x.IsMustExport == StatusSuccess && x.IsDel == false)
            .Select(x => new { x.BoxCode, x.LotNo, x.MaterialId })//选择必出箱
            .ToListAsync();
        if (mandatoryBoxes.Count == 0)//判断必出箱是否存在
        {
            return;
        }
        var confirmedCodes = new HashSet<string>(//创建已确认箱码集合
            confirmedBoxes
                .Where(x => !string.IsNullOrWhiteSpace(x.BoxCode))//过滤箱码
                .Select(x => x.BoxCode.Trim()),//选择箱码
            StringComparer.OrdinalIgnoreCase);//创建已确认箱码集合
        var pendingBoxes = mandatoryBoxes
            .Where(m => !string.IsNullOrWhiteSpace(m.BoxCode) && !confirmedCodes.Contains(m.BoxCode.Trim()))
            .ToList();
        if (pendingBoxes.Count == 0)
        {
            return;
        }

        var pendingCodes = pendingBoxes
            .Select(x => x.BoxCode.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // 统一加载待恢复的库存箱码，避免逐个必出箱再次 hitting 数据库
        var pendingStockInfos = await _stockInfoRep.AsQueryable()
            .Where(x => pendingCodes.Contains(x.BoxCode) && (x.IsDelete != true))
            .ToListAsync();

        var stockLookup = pendingStockInfos
            .GroupBy(x => x.BoxCode ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);
        var needUpdates = new List<WmsStockInfo>();

        foreach (var mandatory in pendingBoxes)//遍历必出箱
        {
            var normalizedCode = mandatory.BoxCode.Trim();
            if (!stockLookup.TryGetValue(normalizedCode, out var candidates))
            {
                _logger.LogWarning("释放必出箱码时未找到库存箱码: BoxCode={BoxCode}, LotNo={LotNo}, MaterialId={MaterialId}",
                    mandatory.BoxCode, mandatory.LotNo, mandatory.MaterialId);
                continue;
            }

            var stockInfo = candidates.FirstOrDefault(x =>
                (string.IsNullOrWhiteSpace(mandatory.LotNo) || string.Equals(x.LotNo, mandatory.LotNo, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(mandatory.MaterialId) || string.Equals(x.MaterialId, mandatory.MaterialId, StringComparison.OrdinalIgnoreCase)));

            if (stockInfo == null)
            {
                _logger.LogWarning("释放必出箱码时未匹配具体库存: BoxCode={BoxCode}, LotNo={LotNo}, MaterialId={MaterialId}",
                    mandatory.BoxCode, mandatory.LotNo, mandatory.MaterialId);
                continue;
            }

            stockInfo.Qty = (stockInfo.Qty ?? 0) + (stockInfo.LockQuantity ?? 0);//设置数量
            stockInfo.LockQuantity = 0;//设置锁定数量
            stockInfo.UpdateTime = now;//设置更新时间
            needUpdates.Add(stockInfo);
        }

        if (needUpdates.Count > 0)
        {
            await _stockInfoRep.AsUpdateable(needUpdates)
                .UpdateColumns(x => new { x.Qty, x.LockQuantity, x.UpdateTime })
                .ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 按拆垛确认结果更新托盘库存/锁定数量，必要时释放托盘。
    /// </summary>
    private async Task UpdateStockTrayAsync(WmsStockTray stockTray, WmsExportOrder exportOrder, decimal totalScanQty, DateTime now)
    {
        if (stockTray == null)//判断托盘是否存在
        {
            return;
        }
        var exportType = exportOrder.ExportType ?? 0;//设置出口类型
        if (exportType == 3) // 空托出库
        {
            // 更新库存箱码：将托盘下的所有箱码标记为删除
            var trayIdStr = stockTray.Id.ToString();//将托盘ID转换为字符串
            await _stockInfoRep.AsUpdateable()
                .SetColumns(x => new WmsStockInfo { Qty = 0, LockQuantity = 0, IsDelete = true, UpdateTime = now })
                .Where(x => x.TrayId == trayIdStr && (x.IsDelete == false || x.IsDelete == null))
                .ExecuteCommandAsync();//更新库存箱码
            stockTray.StockSlotCode = string.Empty;//设置储位编码
            stockTray.StockQuantity = 0;//设置库存数量
            stockTray.LockQuantity = 0;//设置锁定数量
            stockTray.IsDelete = true;//设置删除标志
            stockTray.UpdateTime = now;//设置更新时间
            await _stockTrayRep.AsUpdateable(stockTray).UpdateColumns(x => new { x.StockSlotCode, x.StockQuantity, x.LockQuantity, x.IsDelete, x.UpdateTime }).ExecuteCommandAsync();//更新托盘
            await _sysStockCodeRep.AsUpdateable()
                .SetColumns(x => new WmsSysStockCode { Status = 0, UpdateTime = now })
                .Where(x => x.StockCode == stockTray.StockCode && x.IsDelete == false)
                .ExecuteCommandAsync();//更新库存编码
            return;
        }
        var newLock = (stockTray.LockQuantity ?? 0) - totalScanQty;//设置剩余锁定数量   
        if (newLock < 0)//判断剩余锁定数量是否小于0
        {
            newLock = 0;//设置剩余锁定数量
        }
        stockTray.LockQuantity = newLock;//设置锁定数量
        stockTray.UpdateTime = now;//设置更新时间
        if ((stockTray.StockQuantity ?? 0) <= 0 && newLock <= 0)//判断库存数量是否小于0且剩余锁定数量是否小于0
        {
            stockTray.StockSlotCode = string.Empty;//设置储位编码
            stockTray.IsDelete = true;//设置删除标志
            stockTray.UpdateTime = now;//设置更新时间
            await _stockTrayRep.AsUpdateable(stockTray)
                .UpdateColumns(x => new { x.LockQuantity, x.StockSlotCode, x.IsDelete, x.UpdateTime }).ExecuteCommandAsync();//更新托盘
            await _sysStockCodeRep.AsUpdateable().SetColumns(x => new WmsSysStockCode { Status = 0, UpdateTime = now }).Where(x => x.StockCode == stockTray.StockCode && x.IsDelete == false).ExecuteCommandAsync();//更新库存编码
        }
        else//否则
        {
            await _stockTrayRep.AsUpdateable(stockTray).UpdateColumns(x => new { x.LockQuantity, x.UpdateTime }).ExecuteCommandAsync();//更新托盘
        }
    }

    /// <summary>
    /// 更新 `WmsStock`（库位维度）锁定数量，必要时软删除记录。
    /// </summary>
    private async Task UpdateStockRecordAsync(WmsExportOrder exportOrder, decimal totalScanQty, DateTime now)
    {
        if (!exportOrder.ExportMaterialId.HasValue)//判断物料ID是否存在
        {
            return;
        }
        // 构建查询条件（避免在 SQL 中使用 C# 方法，可能导致语法错误）
        var query = _stockRep.AsQueryable()
            .Where(x => x.MaterialId == exportOrder.ExportMaterialId &&
                       x.WarehouseId == exportOrder.ExportWarehouseId &&
                       x.IsDelete == false);
        // 批次号条件：如果 ExportLotNo 不为空，则必须匹配
        if (!string.IsNullOrWhiteSpace(exportOrder.ExportLotNo))
        {
            query = query.Where(x => x.LotNo == exportOrder.ExportLotNo);
        }
        // 检验状态条件：如果 InspectionStatus 有值，则必须匹配
        if (exportOrder.InspectionStatus.HasValue)
        {
            query = query.Where(x => x.InspectionStatus == exportOrder.InspectionStatus);
        }
        var stockRecord = await query.FirstAsync();//查询库存记录
        if (stockRecord == null)//判断库存记录是否存在
        {
            return;
        }
        var newLock = (stockRecord.LockQuantity ?? 0) - totalScanQty;//设置剩余锁定数量 
        if (newLock < 0)//判断剩余锁定数量是否小于0
        {
            newLock = 0;//设置剩余锁定数量
        }
        stockRecord.LockQuantity = newLock;//设置锁定数量
        stockRecord.UpdateTime = now;//设置更新时间
        if ((stockRecord.StockQuantity ?? 0) <= 0 && newLock <= 0)//判断库存数量是否小于0且剩余锁定数量是否小于0
        {
            stockRecord.IsDelete = true;//设置删除标志
            stockRecord.UpdateTime = now;//设置更新时间
            await _stockRep.AsUpdateable(stockRecord).UpdateColumns(x => new { x.LockQuantity, x.IsDelete, x.UpdateTime }).ExecuteCommandAsync();
        }
        else//否则
        {
            await _stockRep.AsUpdateable(stockRecord).UpdateColumns(x => new { x.LockQuantity, x.UpdateTime }).ExecuteCommandAsync();//更新库存记录
        }
    }

    /// <summary>
    /// 更新储位状态（对照原接口逻辑：如果储位状态不为4，则设置为0）。
    /// </summary>
    private async Task UpdateSlotStatusAsync(WmsExportOrder exportOrder, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(exportOrder.ExportSlotCode))//判断储位编码是否存在
        {
            return;
        }
        var slot = await _baseSlotRep.GetFirstAsync(x => x.SlotCode == exportOrder.ExportSlotCode && x.IsDelete == false);//查询储位信息
        if (slot != null && slot.SlotStatus != 4)//判断储位是否存在且状态不为4
        {
            slot.SlotStatus = 0;//更改当前任务中的储位状态（改为0空储位）
            slot.UpdateTime = now;//设置更新时间
            await _baseSlotRep.AsUpdateable(slot)
                .UpdateColumns(x => new { x.SlotStatus, x.UpdateTime })
                .ExecuteCommandAsync();//更新储位状态
        }
    }

    /// <summary>
    /// 推进出库流水状态并累计完成数量。
    /// </summary>
    /// <param name="exportOrder">出库订单。</param>
    /// <param name="totalScanQty">总扫描数量。</param>
    /// <param name="now">当前时间。</param>
    private async Task UpdateExportOrderAsync(WmsExportOrder exportOrder, decimal totalScanQty, DateTime now)
    {
        exportOrder.ExportExecuteFlag = 2;//设置执行标志
        exportOrder.ScanQuantity = (exportOrder.ScanQuantity ?? 0) + totalScanQty;//设置扫描数量
        exportOrder.CompleteDate = now;//设置完成时间   
        exportOrder.UpdateTime = now;//设置更新时间
        await _exportOrderRep.AsUpdateable(exportOrder).UpdateColumns(x => new { x.ExportExecuteFlag, x.ScanQuantity, x.CompleteDate, x.UpdateTime }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新出库通知明细及主单的执行状态。
    /// </summary>
    /// <param name="exportOrder">出库订单。</param>
    /// <param name="totalScanQty">总扫描数量。</param>
    /// <param name="now">当前时间。</param>
    private async Task UpdateExportNotifyAsync(WmsExportOrder exportOrder, decimal totalScanQty, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(exportOrder.ExportBillCode))//判断出库单据编码是否存在
        {
            return;
        }
        if (!exportOrder.ExportMaterialId.HasValue)//判断物料ID是否存在
        {
            return;
        }
        // 对照原接口逻辑：根据 InspectionStatus 是否存在来决定查询条件
        WmsExportNotifyDetail notifyDetail = null;//设置出库通知明细
        if (exportOrder.InspectionStatus.HasValue && !string.IsNullOrWhiteSpace(exportOrder.InspectionStatus.Value.ToString()))//判断检验状态是否存在
        {
            var inspectionStatusStr = exportOrder.InspectionStatus.Value.ToString();//转换为字符串
            notifyDetail = await _exportNotifyDetailRep.GetFirstAsync(x =>
                x.ExportBillCode == exportOrder.ExportBillCode &&
                x.MaterialId == exportOrder.ExportMaterialId &&
                (!string.IsNullOrWhiteSpace(exportOrder.ExportLotNo) ? x.LotNo == exportOrder.ExportLotNo : true) &&
                x.InspectionStatus == inspectionStatusStr &&
                (x.IsDelete == false || x.IsDelete == null));//查询出库通知明细（有检验状态）
        }
        else
        {
            notifyDetail = await _exportNotifyDetailRep.GetFirstAsync(x =>
                x.ExportBillCode == exportOrder.ExportBillCode &&
                x.MaterialId == exportOrder.ExportMaterialId &&
                (!string.IsNullOrWhiteSpace(exportOrder.ExportLotNo) ? x.LotNo == exportOrder.ExportLotNo : true) &&
                (x.IsDelete == false || x.IsDelete == null));//查询出库通知明细（无检验状态）
        }
        if (notifyDetail != null)//判断出库通知明细是否存在
        {
            notifyDetail.CompleteQuantity = (notifyDetail.CompleteQuantity ?? 0) + totalScanQty;//设置完成数量
            notifyDetail.UpdateTime = now;//设置更新时间
            // 判断是否有其他执行订单（ExportExecuteFlag == 1 表示正在执行）
            var hasOtherExecutingOrders = await _exportOrderRep.AsQueryable()
                .Where(o => o.ExportBillCode == exportOrder.ExportBillCode &&
                           o.ExportMaterialId == exportOrder.ExportMaterialId &&
                           (!string.IsNullOrWhiteSpace(exportOrder.ExportLotNo) ? o.ExportLotNo == exportOrder.ExportLotNo : true) &&
                           o.IsDelete == false &&
                           o.ExportOrderNo != exportOrder.ExportOrderNo &&
                           o.ExportExecuteFlag == 1)
                .AnyAsync();//判断是否有其他执行订单
            if (!hasOtherExecutingOrders)//判断是否有其他执行订单
            {
                notifyDetail.ExportDetailFlag = 3;//设置执行标志
            }
            await _exportNotifyDetailRep.AsUpdateable(notifyDetail)
                .UpdateColumns(x => new { x.CompleteQuantity, x.ExportDetailFlag, x.UpdateTime })
                .ExecuteCommandAsync();//更新出库通知明细
        }
        // 判断是否有未完成明细（ExportDetailFlag < 3 表示未完成）
        var hasPendingDetail = await _exportNotifyDetailRep.AsQueryable()
            .Where(x => x.ExportBillCode == exportOrder.ExportBillCode &&
                       (x.IsDelete == false || x.IsDelete == null) &&
                       (x.ExportDetailFlag == null || x.ExportDetailFlag < 3))
            .AnyAsync();//判断是否有未完成明细    
        if (!hasPendingDetail)//判断是否有未完成明细
        {
            var notify = await _exportNotifyRep.GetFirstAsync(x => x.ExportBillCode == exportOrder.ExportBillCode && x.IsDelete == false);//查询出库通知
            if (notify != null)//判断出库通知是否存在
            {
                notify.ExportExecuteFlag = 3;//设置执行标志
                notify.UpdateTime = now;//设置更新时间
                await _exportNotifyRep.AsUpdateable(notify)
                    .UpdateColumns(x => new { x.ExportExecuteFlag, x.UpdateTime })
                    .ExecuteCommandAsync();//更新出库通知
            }
        }
    }

    /// <summary>
    /// 依据物料编码、批次汇总 PDA 扫描数量。
    /// </summary>
    private static decimal CalculateManualScanSum(IEnumerable<ManualTaskDetail> details, string materialCode, string lotNo)
    {
        var query = details;//设置查询
        if (!string.IsNullOrWhiteSpace(materialCode))//判断物料编码是否存在
        {
            query = query.Where(x => string.Equals(x.MaterialCode, materialCode, StringComparison.OrdinalIgnoreCase));//设置查询
        }
        if (!string.IsNullOrWhiteSpace(lotNo))//判断批次号是否存在
        {
            query = query.Where(x => string.Equals(x.StockLotNo, lotNo, StringComparison.OrdinalIgnoreCase));//设置查询
        }
        return query.Sum(x => x.EffectiveScanQty);//返回扫描数量
    }

    /// <summary>
    /// 构造 PDA 手动拆垛箱码查询返回结果。
    /// </summary>
    /// <param name="code">状态码。</param>
    /// <param name="message">消息。</param>
    /// <param name="data">数据。</param>
    /// <param name="flag">标志。</param>
    /// <param name="count">数量。</param>
    /// <returns>PDA返回结构。</returns>
    private static PdaManualBoxInfoResult CreateManualBoxInfoResult(int code, string message, PdaManualBoxInfoDetail data = null, string flag = null, int? count = null)
    {
        var result = new PdaManualBoxInfoResult//返回结果
        {
            Code = code,//成功 0失败
            Msg = message,//消息
            Flag = flag ?? (code == 1 ? "1" : "0"),//标志（默认：成功="1"，失败="0"）
            Data = data//数据
        };
        // 如果 count 未指定，根据 data 是否存在推断
        if (count.HasValue)
        {
            result.Count = count.Value;//数量
        }
        else
        {
            result.Count = data != null ? 1 : 0;//数量
        }
        return result;
    }

    /// <summary>
    /// 更新出库任务消息。
    /// </summary>
    /// <param name="key">关键字。</param>
    /// <param name="message">消息。</param>
    private async Task UpdateExportTaskMessageAsync(string key, string message)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(message))
        {
            return;//返回
        }
        await _exportTaskRep.AsUpdateable().SetColumns(t => new WmsExportTask { Information = message }).Where(t => t.ExportTaskNo == key || t.ExportOrderNo == key).ExecuteCommandAsync();//更新出库任务
    }

    #region 优化辅助方法

    /// <summary>
    /// 执行带日志记录的异步操作（子类包装方法）
    /// </summary>
    private async Task<T> ExecuteWithLoggingAsync<T>(string operationName, Func<Task<T>> operation, object parameters = null)
    {
        return await ExecuteWithLoggingAsync(_logger, operationName, operation, parameters);
    }

    /// <summary>
    /// 根据条件查询手动拆垛记录
    /// </summary>
    /// <param name="input">查询条件</param>
    /// <returns>手动拆垛记录</returns>
    private async Task<WmsManualDepalletizing> FindManualDepalletizingForRemovalAsync(PdaManualRemoveInput input)
    {
        var query = _manualRep.AsQueryable().Where(x => x.IsDelete == false && x.State == StateActive);

        if (input.Id.HasValue)
        {
            return await query.FirstAsync(x => x.Id == input.Id.Value);
        }

        return await query.FirstAsync(x =>
            x.BoxCode == input.BoxCode.Trim() &&
            x.TrayCode == input.TrayCode.Trim() &&
            x.ExportOrderNo == input.ExportOrderNo.Trim());
    }

     
    #endregion
    #endregion
}
