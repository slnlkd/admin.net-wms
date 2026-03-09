// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
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
using Admin.NET.Application.Service.WmsSqlView;
using Admin.NET.Core;
using Admin.NET.Application.Service.WmsPort;
using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Application.Service.WmsPort.Process;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using SqlSugar;
using Admin.NET.Application.Service.WmsPda.Process;

namespace Admin.NET.Application.Service.WmsPda;
/// <summary>
/// PDA 入库业务处理服务。
/// <para>承载 JC35 <c>PdaInterfaceController</c> 入库组托相关接口迁移，提供完整的PDA入库相关业务功能。</para>
/// </summary>
/// <remarks>
/// 主要功能包括：
/// <list type="bullet">
/// <item><description>入库组托管理：查询组托信息、计算绑定数量、保存载具绑定</description></item>
/// <item><description>箱托关系管理：箱码绑定、解绑、删除等操作</description></item>
/// <item><description>空托盘管理：空托盘组盘、批量生成入库流水</description></item>
/// <item><description>托盘明细查询：按托盘编码查询箱码信息</description></item>
/// <item><description>暂存入库管理：生成暂存入库流水、叠箱绑定</description></item>
/// <item><description>数据验证：单据状态验证、物料匹配验证、托盘混放验证</description></item>
/// </list>
/// </remarks>
public class PdaImportProcess : PdaBase, ITransient
{
    #region 字段定义
    private readonly SqlSugarRepository<WmsSysStockCode> _sysStockCodeRep;   //库存编码
    private readonly SqlSugarRepository<WmsStockSlotBoxInfo> _stockSlotBoxInfoRep;    //库存组托信息
    private readonly SqlSugarRepository<WmsImportNotify> _importNotifyRep;        //入库单据
    private readonly SqlSugarRepository<WmsImportNotifyDetail> _importNotifyDetailRep;    //入库单据明细
    private readonly SqlSugarRepository<WmsBaseMaterial> _materialRep;            //物料
    private readonly SqlSugarRepository<WmsStockTray> _stockTrayRep;            //库存托盘
    private readonly SqlSugarRepository<WmsImportOrder> _importOrderRep;        //入库流水
    private readonly SqlSugarRepository<WmsExportOrder> _exportOrderRep;        //出库订单
    private readonly SqlSugarRepository<WmsExportBoxInfo> _exportBoxInfoRep;    //出库箱码信息
    private readonly SqlSugarRepository<WmsBaseWareHouse> _warehouseRep;        //仓库
    private readonly SqlSugarRepository<WmsBaseSlot> _baseSlotRep;            //库位
    private readonly SqlSugarRepository<WmsImportLabelPrint> _importLabelPrintRep;    //入库标签打印
    private readonly SqlSugarRepository<WmsInspectNotity> _inspectNotifyRep;        //检验通知
    private readonly ILogger _logger;                            //日志
    private readonly CommonMethod _commonMethod;                                    //通用方法
    private readonly PortImportBind _emptyTrayBind;                                //空托盘绑定
    private readonly WmsSqlViewService _sqlViewService;                                    //SQL视图功能服务（视图查询和SQL函数）
    #endregion

    #region 常量定义
    private const int DefaultEmptyTrayQuantity = 2;                                //默认空托盘数量
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖
    /// </summary>
    public PdaImportProcess(
        ILoggerFactory loggerFactory,
        SqlSugarRepository<WmsSysStockCode> sysStockCodeRep,
        SqlSugarRepository<WmsStockSlotBoxInfo> stockSlotBoxInfoRep,
        SqlSugarRepository<WmsImportNotify> importNotifyRep,
        SqlSugarRepository<WmsImportNotifyDetail> importNotifyDetailRep,
        SqlSugarRepository<WmsBaseMaterial> materialRep,
        SqlSugarRepository<WmsStockTray> stockTrayRep,
        SqlSugarRepository<WmsImportOrder> importOrderRep,
        SqlSugarRepository<WmsExportOrder> exportOrderRep,
        SqlSugarRepository<WmsExportBoxInfo> exportBoxInfoRep,
        SqlSugarRepository<WmsBaseWareHouse> warehouseRep,
        SqlSugarRepository<WmsBaseSlot> baseSlotRep,
        SqlSugarRepository<WmsImportLabelPrint> importLabelPrintRep,
        SqlSugarRepository<WmsInspectNotity> inspectNotifyRep,
        ISqlSugarClient sqlSugarClient,
        PortImportBind emptyTrayBind,
        WmsSqlViewService sqlViewService) : base(sqlSugarClient)
    {
        _sysStockCodeRep = sysStockCodeRep;
        _stockSlotBoxInfoRep = stockSlotBoxInfoRep;
        _importNotifyRep = importNotifyRep;
        _importNotifyDetailRep = importNotifyDetailRep;
        _materialRep = materialRep;
        _stockTrayRep = stockTrayRep;
        _importOrderRep = importOrderRep;
        _exportOrderRep = exportOrderRep;
        _exportBoxInfoRep = exportBoxInfoRep;
        _warehouseRep = warehouseRep;
        _baseSlotRep = baseSlotRep;
        _importLabelPrintRep = importLabelPrintRep;
        _inspectNotifyRep = inspectNotifyRep;
        _commonMethod = new CommonMethod(sqlSugarClient);
        _emptyTrayBind = emptyTrayBind;
        _sqlViewService = sqlViewService;
        _logger = loggerFactory.CreateLogger(CommonConst.PdaImportProcess);
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 查询入库组托信息。
    /// <para>对应 JC35 接口：【GetStockSoltBoxInfo】</para>
    /// </summary>
    /// <param name="input">查询入库组托信息的请求参数，包含查询类型、物料条码、托盘编码、入库单号等</param>
    /// <returns>入库组托信息查询结果，包含成功状态、消息和组托明细列表</returns>
    /// <exception cref="Exception">当参数验证失败或查询过程中发生错误时抛出异常</exception>
    /// <remarks>
    /// 查询逻辑：
    /// <list type="number">
    /// <item><description>type≠1 时按物料条码拆分明细、物料、批次、导入流水 ID</description></item>
    /// <item><description>type=1 时按托盘号检索，不填托盘则默认读取剔除原因不为空的数据</description></item>
    /// <item><description>所有查询均保留 ImportExecuteFlag == 01/02 的过滤</description></item>
    /// </list>
    /// 业务场景：
    /// <list type="bullet">
    /// <item><description>按物料条码查询：返回该物料在指定入库单下的组托情况</description></item>
    /// <item><description>按托盘查询：返回该托盘上所有已组托的箱码信息</description></item>
    /// <item><description>按单据查询：返回指定入库单下所有已组托的信息</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaStockSlotQueryOutput> QueryInboundGroupAsync(PdaStockSlotQueryInput input)
    {
        if (!string.Equals(input.Type, "1", StringComparison.OrdinalIgnoreCase))    //判断类型是否为1
        {
            if (string.IsNullOrWhiteSpace(input.MaterialBarcode))    //判断物料条码是否为空
                throw Oops.Bah("物料条码不能为空！");
            if (string.IsNullOrWhiteSpace(input.ImportBillCode))    //判断入库单号是否为空
                throw Oops.Bah("入库单号不能为空！");
        }
        var output = new PdaStockSlotQueryOutput();    //返回结果
        try
        {
            // 旧系统：type == "1" 代表"按托盘号查询"，其它值走物料条码分支。
            var items = string.Equals(input.Type, "1", StringComparison.OrdinalIgnoreCase) ? await QueryByStockAsync(input) : await QueryByMaterialBarcodeAsync(input);    //查询入库组托信息   
            output.Success = true;    //设置成功
            output.Message = "查询成功";    //设置消息
            output.Items = items;    //设置结果
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询入库组托信息失败：{@Input}", input);
            output.Success = false;    //设置失败
            output.Message = ex.Message;    //设置消息
            output.Items = new List<PdaStockSlotItem>();    //设置结果
        }
        return output;    //返回结果
    }
    /// <summary>
    /// 计算无箱码绑定推荐数量。
    /// <para>对应 JC35 接口：【GetScanQty】</para>
    /// </summary>
    /// <param name="input">计算无箱码绑定数量的请求参数，包含物料条码、托盘编码、入库单号等信息</param>
    /// <returns>计算结果，包含成功状态、消息和推荐数量</returns>
    /// <exception cref="Exception">当参数验证失败、条码格式错误或托盘已绑定时抛出异常</exception>
    /// <remarks>
    /// 核心逻辑：
    /// <list type="number">
    /// <item><description>解析物料条码格式"明细-物料-批次(-流水)"</description></item>
    /// <item><description>校验目标托盘尚未绑定 (Status==0)</description></item>
    /// <item><description>计算剩余数量 = 计划入库 - 已组托</description></item>
    /// <item><description>推荐值取 min(剩余, EveryNumber)</description></item>
    /// <item><description>保留所有异常提示文案</description></item>
    /// </list>
    /// 业务规则：
    /// <list type="bullet">
    /// <item><description>支持明细ID-物料ID-批号的条码格式</description></item>
    /// <item><description>可选的第4位参数表示入库流水ID</description></item>
    /// <item><description>推荐数量不超过剩余可入库数量</description></item>
    /// <item><description>考虑物料基础数据中的件数规格</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaScanQtyOutput> CalculateNoBoxQtyAsync(PdaScanQtyInput input)
    {
        if (input == null)    //判断输入是否为空
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.MaterialBarcode))    //判断物料条码是否为空
            throw Oops.Bah("物料条码不能为空！");
        if (string.IsNullOrWhiteSpace(input.ImportBillCode))    //判断入库单号是否为空
            throw Oops.Bah("入库单信息不能为空！");
        if (string.IsNullOrWhiteSpace(input.StockCode))    //判断托盘编码是否为空
            throw Oops.Bah("托盘编码不能为空！");
        var output = new PdaScanQtyOutput();    //返回结果
        var stockCode = input.StockCode.Trim();    //托盘编码
        var importBillCode = input.ImportBillCode.Trim();    //入库单号
        try
        {
            // 条码解析：明细ID-物料ID-批次-（可选）入库流水
            var barcode = ParseMaterialBarcode(input.MaterialBarcode);    //解析物料条码
            if (!barcode.DetailId.HasValue)    //判断明细ID是否存在
                throw Oops.Bah("物料条码格式错误，正确格式示例：明细ID-物料ID-批次。");
            var resolvedImportId = await ResolveImportIdAsync(importBillCode);    //解析入库单号
            var importDetailQuery = _importNotifyDetailRep.AsQueryable()    //查询入库单据明细
                .Where(detail => detail.Id == barcode.DetailId.Value && detail.IsDelete == false)    //判断明细ID是否存在
                .WhereIF(resolvedImportId.HasValue, detail => detail.ImportId == resolvedImportId.Value)    //判断入库单号是否存在
                .WhereIF(!resolvedImportId.HasValue, detail => SqlFunc.ToString(detail.ImportId) == importBillCode)
                .WhereIF(barcode.MaterialId.HasValue, detail => detail.MaterialId == barcode.MaterialId.Value)
                .WhereIF(!barcode.MaterialId.HasValue && !string.IsNullOrWhiteSpace(barcode.MaterialIdRaw), detail => SqlFunc.ToString(detail.MaterialId) == barcode.MaterialIdRaw)    //判断物料ID是否存在
                .WhereIF(!string.IsNullOrWhiteSpace(barcode.LotNo), detail => detail.LotNo == barcode.LotNo);   //判断批次是否存在
            // 获取要绑定的入库明细
            var importDetail = await importDetailQuery.FirstAsync();    //获取要绑定的入库明细  
            if (importDetail == null)    //判断入库明细是否存在
                throw Oops.Bah("未找到匹配的入库单明细，请确认条码信息是否正确。");
            // 判断当前物料是否存在
            long? importOrderId = long.TryParse(importBillCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedOrderId) ? parsedOrderId : null;    //入库流水ID
            // JC35：slot = View_WmsStockSlotBoxInfo.FirstOrDefault(... StockCode == 参数 && Status == 0 && IsDel == 0 && ImportDetailId == 条码明细)。
            var existingQuery = _sqlViewService.QueryStockSlotBoxInfoView()
                .MergeTable()
                .Where(x => x.IsDelete == false && x.Status == 0 && x.StockCode == stockCode && x.ImportDetailId == importDetail.Id)
                .WhereIF(importOrderId.HasValue, x => x.ImportOrderId == importOrderId.Value)
                .WhereIF(!importOrderId.HasValue, x => SqlFunc.ToString(x.ImportOrderId) == importBillCode);
            var isBound = await existingQuery.AnyAsync();
            if (isBound)
                throw Oops.Bah("当前托盘已绑定！");
            // 获取物料基础信息
            var material = (importDetail.MaterialId.HasValue ? await _materialRep.GetByIdAsync(importDetail.MaterialId.Value) : null) ?? throw Oops.Bah("获取物料基础信息失败！");    //获取物料基础信息
                                                                                                                                                                           // 入库数量-入库组托数量                                                                                                                                                           
            var remainQty = Math.Max(0, (importDetail.ImportQuantity ?? 0) - (importDetail.ImportFactQuantity ?? 0));    //入库数量-入库组托数量                    
            decimal? detailBoxQty = null;    //入库箱数量               
            var boxQuantityProperty = importDetail.GetType().GetProperty("BoxQuantity");    //获取入库箱数量属性
            if (boxQuantityProperty != null)
            {
                var rawBoxQty = boxQuantityProperty.GetValue(importDetail);    //获取入库箱数量
                if (rawBoxQty != null && decimal.TryParse(Convert.ToString(rawBoxQty, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedDetailBoxQty) && parsedDetailBoxQty > 0)    //判断入库箱数量是否大于0
                {
                    detailBoxQty = parsedDetailBoxQty;    //入库箱数量
                }
            }
            if ((!detailBoxQty.HasValue || detailBoxQty.Value <= 0) &&
                !string.IsNullOrWhiteSpace(material.BoxQuantity) &&    //判断物料箱数量是否为空
                decimal.TryParse(material.BoxQuantity, NumberStyles.Any, CultureInfo.InvariantCulture, out var materialBoxQty) &&
                materialBoxQty > 0)    //判断物料箱数量是否大于0
            {
                // 入库箱数量*基础数据件数
                detailBoxQty = materialBoxQty;    //入库箱数量
            }
            decimal? everyNumber = null;    //基础数据件数
            if (!string.IsNullOrWhiteSpace(material.EveryNumber) &&
                decimal.TryParse(material.EveryNumber, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedEveryNumber) &&    //判断基础数据件数是否大于0
                parsedEveryNumber > 0)
            {
                everyNumber = parsedEveryNumber;    //基础数据件数
            }
            decimal recommendedQty = remainQty;    //推荐数量
            if (remainQty > 0 && detailBoxQty.HasValue && everyNumber.HasValue)
            {
                var plannedQty = detailBoxQty.Value * everyNumber.Value;    //计划入库数量      
                if (plannedQty > 0)    //判断计划入库数量是否大于0
                {
                    // 计划入库数量
                    recommendedQty = Math.Min(remainQty, plannedQty);    //推荐数量
                }
            }
            else if (remainQty > 0 && everyNumber.HasValue)
            {
                recommendedQty = Math.Min(remainQty, everyNumber.Value);    //推荐数量
            }
            output.Success = true;    //设置成功
            output.Message = "计算完成";    //设置消息
            output.Quantity = recommendedQty;    //设置推荐数量
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "计算无箱码绑定数量失败：{@Input}", input);
            output.Success = false;
            output.Message = ex.Message;
            output.Quantity = 0;
        }
        return output;
    }
    /// <summary>
    /// 保存无箱码载具绑定信息。
    /// <para>对应 JC35 接口：【SaveBoxInfo】</para>
    /// </summary>
    /// <param name="input">保存无箱码绑定的请求参数，包含托盘编码、物料信息、数量、用户等</param>
    /// <returns>绑定操作结果，包含成功状态和消息</returns>
    /// <exception cref="Exception">当参数验证失败、托盘不存在、入库明细不存在或绑定失败时抛出异常</exception>
    /// <remarks>
    /// 业务流程：
    /// <list type="number">
    /// <item><description>验证输入参数的完整性和有效性</description></item>
    /// <item><description>验证托盘存在且可用</description></item>
    /// <item><description>获取并验证入库明细信息</description></item>
    /// <item><description>检查或创建入库流水记录</description></item>
    /// <item><description>创建或更新库存组托信息</description></item>
    /// <item><description>更新入库明细的实际组托数量</description></item>
    /// <item><description>推进入库单据的执行状态</description></item>
    /// </list>
    /// 业务规则：
    /// <list type="bullet">
    /// <item><description>支持人工码垛场景（Sources不为空）</description></item>
    /// <item><description>同一托盘可续码码垛（相同物料和批次）</description></item>
    /// <item><description>自动更新托盘使用状态</description></item>
    /// <item><description>事务保证数据一致性</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> SaveNoBoxBindingAsync(PdaSaveBoxInfoInput input)
    {
        if (input == null)    //判断输入是否为空
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.StockCode))    //判断托盘编码是否为空
            throw Oops.Bah("托盘编码不能为空！");
        if (string.IsNullOrWhiteSpace(input.MaterialId))    //判断物料ID是否为空
            throw Oops.Bah("物料信息不能为空！");
        if (string.IsNullOrWhiteSpace(input.LotNo))    //判断批次是否为空
            throw Oops.Bah("批次信息不能为空！");
        if (string.IsNullOrWhiteSpace(input.ImportId))    //判断入库单号是否为空
            throw Oops.Bah("入库单信息不能为空！");
        if (string.IsNullOrWhiteSpace(input.ImportDetailId))    //判断入库单明细ID是否为空
            throw Oops.Bah("入库单明细不能为空！");
        if (input.Qty <= 0)    //判断绑定数量是否大于0
            throw Oops.Bah("绑定数量不能为空！");
        var stockCode = input.StockCode.Trim();    //托盘编码   
        var lotNo = input.LotNo.Trim();
        var user = NormalizeUser(input.User);    //用户
        var now = DateTime.Now;    //当前时间
        var importId = ParseRequiredLong(input.ImportId, "入库单信息");    //入库单号
        var importDetailId = ParseRequiredLong(input.ImportDetailId, "入库单明细");    //入库单明细ID
        var materialId = ParseRequiredLong(input.MaterialId, "物料信息");    //物料ID
        var quantity = input.Qty;    //绑定数量 
        var result = new PdaActionResult();    //返回结果
        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                WmsSysStockCode pallet;
                if (!string.IsNullOrWhiteSpace(input.Sources))    //判断来源是否为空
                {
                    //判断有值代表人工码垛,可以在库存托盘继续码垛
                    var stockTray = await _stockTrayRep.GetFirstAsync(x =>
                        x.StockCode == stockCode &&
                        SqlFunc.ToString(x.MaterialId) == input.MaterialId.Trim() &&
                        x.LotNo == lotNo);    //查询库存托盘
                    if (stockTray == null)
                    {
                        //判断托盘是否存在或者使用
                        // 对照 JC35 逻辑：DataContext.WmsSysStockCode.FirstOrDefault(a => a.StockCode == boxInfo.StockCode && a.Status == 0 && a.IsDel == 0)
                        pallet = await _sysStockCodeRep.GetFirstAsync(x =>
                            x.StockCode == stockCode &&
                            x.IsDelete == false &&
                            x.Status == 0);    //查询库存托盘
                    }
                    else
                    {
                        // 对照 JC35 逻辑：当找到库存托盘时，查询托盘主数据（不检查Status）
                        pallet = await _sysStockCodeRep.GetFirstAsync(x => x.StockCode == stockCode && x.IsDelete == false);    //查询库存托盘
                    }
                }
                else
                {
                    //判断托盘是否存在或者使用
                    // 对照 JC35 逻辑：DataContext.WmsSysStockCode.FirstOrDefault(a => a.StockCode == boxInfo.StockCode && a.IsDel == 0)
                    pallet = await _sysStockCodeRep.GetFirstAsync(x => x.StockCode == stockCode && x.IsDelete == false);    //查询库存托盘
                }
                if (pallet == null)
                    throw Oops.Bah($"托盘({stockCode})不存在或已被删除！");
                //获取要绑定的入库明细（使用视图查询）
                var importDetail = await _sqlViewService.QueryImportNotifyDetailView()
                    .MergeTable()
                    .Where(x => x.Id == importDetailId && x.IsDel == false && x.ImportId == importId && x.MaterialId == materialId && x.LotNo == lotNo)
                    .FirstAsync() ?? throw Oops.Bah("未获取到入库单据！");    //获取要绑定的入库明细

                var notify = await _importNotifyRep.GetFirstAsync(x => x.Id == importId && x.IsDelete == false) ?? throw Oops.Bah("未获取到入库单据！");    //查询入库单据

                var inspectionStatus = !string.IsNullOrWhiteSpace(importDetail.MaterialStatus) &&
                                       int.TryParse(importDetail.MaterialStatus, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedInspectionStatus) ? parsedInspectionStatus : 0;    //获取检验状态
                // 获取托盘对应的入库流水信息（使用视图查询）
                var orderItems = await _sqlViewService.QueryImportOrderView()
                    .MergeTable()
                    .Where(x => x.StockCode == stockCode && x.ImportExecuteFlag == "01" && x.IsDel == false)
                    .OrderBy(x => x.CreateTime, OrderByType.Desc)
                    .ToListAsync();    //获取托盘对应的入库流水信息
                var detailForUpdate = await _importNotifyDetailRep.GetFirstAsync(x => x.Id == importDetail.Id && x.IsDelete == false) ?? throw Oops.Bah("未获取到入库单据明细！");    //获取入库明细实体

                WmsImportOrder importOrder;    //入库流水
                if (orderItems.Count == 0)    //判断入库流水是否存在
                {
                    importOrder = new WmsImportOrder
                    {
                        Id = SnowFlakeSingle.Instance.NextId(),
                        ImportOrderNo = _commonMethod.GetImExNo("RK"), //流水号
                        ImportId = importDetail.ImportId,    //入库单号
                        WareHouseId = notify.WarehouseId,    //仓库ID
                        StockCodeId = pallet.Id,    //托盘ID
                        StockCode = pallet.StockCode,    //托盘编码
                        ImportQuantity = quantity,    //入库数量
                        ImportExecuteFlag = "01", //执行标志（状态）（0待执行、1正在执行、2已完成、3已上传）
                        LotNo = importDetail.LotNo,    //批次
                        ImportProductionDate = importDetail.ImportProductionDate,    //生产日期
                        CreateUserName = user,    //创建用户
                        UpdateUserName = user,    //更新用户
                        CreateUserId = 0,    //创建用户ID
                        UpdateUserId = 0,    //更新用户ID
                        CreateTime = now,    //创建时间
                        UpdateTime = now,    //更新时间
                        IsDelete = false,    //是否删除
                        ImportDetailId = importDetail.Id,    //入库单明细ID
                        InspectionStatus = inspectionStatus,    //检验状态
                        YsOrTJ = "0"
                    };
                    await _importOrderRep.InsertAsync(importOrder);// 生成入库流水
                    // 更改托盘使用状态
                    pallet.Status = 1;    //托盘使用状态
                    pallet.UpdateTime = now;    //更新时间
                    pallet.UpdateUserName = user;    //更新用户
                    await _sysStockCodeRep.AsUpdateable(pallet).UpdateColumns(x => new { x.Status, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();    //更新托盘使用状态
                    var boxEntity = new WmsStockSlotBoxInfo    //生成入库箱信息
                    {
                        Id = SnowFlakeSingle.Instance.NextId(),
                        ValidateDay = importDetail.ImportLostDate,    //有效期
                        ProductionDate = importDetail.ImportProductionDate,    //生产日期
                        StockCodeId = pallet.Id,    //托盘ID
                        StockCode = pallet.StockCode,    //托盘编码
                        LotNo = importDetail.LotNo,    //批次
                        BoxCode = null,    //箱码
                        MaterialId = importDetail.MaterialId,    //物料ID
                        SupplierId = notify.SupplierId,    //供应商
                        ManufacturerId = notify.ManufacturerId,    //制造商
                        IsDelete = false,    //是否删除
                        Status = 0,    //状态
                        ImportOrderId = importOrder.Id,    //入库流水ID
                        ImportId = importDetail.ImportId,    //入库单号
                        ImportDetailId = importDetail.Id,    //入库单明细ID
                        Qty = quantity,    //入库数量
                        BulkTank = input.BulkTank,    //油罐
                        CreateTime = now,    //创建时间
                        CreateUserName = user,    //创建用户
                        CreateUserId = 0,    //创建用户ID
                        UpdateTime = now,    //更新时间
                        UpdateUserName = user,    //更新用户
                        UpdateUserId = 0,    //更新用户ID
                        InspectionStatus = inspectionStatus,    //检验状态
                    };
                    await _stockSlotBoxInfoRep.InsertAsync(boxEntity);    //插入入库箱信息
                    detailForUpdate.ImportFactQuantity = (detailForUpdate.ImportFactQuantity ?? 0) + quantity;    //入库组托数量
                }
                else
                {
                    var orderView = orderItems[0];    //入库流水视图
                    importOrder = await _importOrderRep.GetFirstAsync(x => x.Id == orderView.Id && x.IsDelete == false);    //查询入库流水实体
                    if (importOrder == null)
                    {
                        throw Oops.Bah("获取入库流水信息失败！");
                    }
                    var boxEntity = await _stockSlotBoxInfoRep.GetFirstAsync(x => x.StockCodeId == pallet.Id && x.ImportOrderId == importOrder.Id);    //查询入库箱信息
                    if (boxEntity != null &&
                        boxEntity.MaterialId == importDetail.MaterialId &&    //判断物料ID是否一致
                        string.Equals(boxEntity.LotNo ?? string.Empty, importDetail.LotNo ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
                        boxEntity.ImportDetailId == importDetail.Id)    //判断入库单明细ID是否一致
                    {
                        boxEntity.Qty = quantity;    //入库数量
                        boxEntity.UpdateTime = now;    //更新时间
                        boxEntity.UpdateUserName = user;    //更新用户
                        boxEntity.UpdateUserId = 0;    //更新用户ID
                        await _stockSlotBoxInfoRep.AsUpdateable(boxEntity).UpdateColumns(x => new { x.Qty, x.UpdateTime, x.UpdateUserName, x.UpdateUserId }).ExecuteCommandAsync();
                        detailForUpdate.ImportFactQuantity = quantity;
                    }
                    else
                    {
                        var newBox = new WmsStockSlotBoxInfo    //生成入库箱信息
                        {
                            Id = SnowFlakeSingle.Instance.NextId(),
                            ValidateDay = importDetail.ImportLostDate,    //有效期
                            ProductionDate = importDetail.ImportProductionDate,    //生产日期
                            StockCodeId = pallet.Id,    //托盘ID
                            StockCode = pallet.StockCode,    //托盘编码
                            LotNo = importDetail.LotNo,    //批次
                            BoxCode = null,    //箱码
                            MaterialId = importDetail.MaterialId,    //物料ID
                            SupplierId = notify.SupplierId,    //供应商
                            ManufacturerId = notify.ManufacturerId,    //制造商
                            IsDelete = false,    //是否删除
                            Status = 0,    //状态
                            ImportOrderId = importOrder.Id,    //入库流水ID
                            ImportId = importDetail.ImportId,    //入库单号
                            ImportDetailId = importDetail.Id,    //入库单明细ID
                            Qty = quantity,    //入库数量
                            BulkTank = input.BulkTank,    //油罐
                            CreateTime = now,    //创建时间
                            CreateUserName = user,    //创建用户
                            CreateUserId = 0,    //创建用户ID
                            UpdateTime = now,    //更新时间
                            UpdateUserName = user,    //更新用户
                            UpdateUserId = 0,    //更新用户ID
                            InspectionStatus = inspectionStatus,    //检验状态
                        };
                        await _stockSlotBoxInfoRep.InsertAsync(newBox);    //插入入库箱信息
                        detailForUpdate.ImportFactQuantity = (detailForUpdate.ImportFactQuantity ?? 0) + quantity;    //入库组托数量
                    }
                    importOrder.UpdateTime = now;    //更新时间
                    importOrder.UpdateUserName = user;    //更新用户
                    await _importOrderRep.AsUpdateable(importOrder).UpdateColumns(x => new { x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();    //更新入库流水
                }
                if (string.Equals(detailForUpdate.ImportExecuteFlag, "01", StringComparison.OrdinalIgnoreCase))
                {
                    detailForUpdate.ImportExecuteFlag = "02";    //执行标志（状态）（0待执行、1正在执行、2已完成、3已上传）
                }
                detailForUpdate.UpdateTime = now;    //更新时间
                detailForUpdate.UpdateUserName = user;    //更新用户
                await _importNotifyDetailRep.AsUpdateable(detailForUpdate).UpdateColumns(x => new { x.ImportFactQuantity, x.ImportExecuteFlag, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();    //更新入库单据明细
                notify.ImportExecuteFlag = "02";    //执行标志（状态）（0待执行、1正在执行、2已完成、3已上传）
                notify.UpdateTime = now;    //更新时间
                notify.UpdateUserName = user;    //更新用户
                await _importNotifyRep.AsUpdateable(notify).UpdateColumns(x => new { x.ImportExecuteFlag, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();    //更新入库单据
            }, "无箱码保存载具绑定失败");
            result.Success = true;
            result.Message = "操作成功";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "无箱码保存载具绑定失败：{@Input}", input);
            result.Success = false;
            result.Message = ex.Message;
        }
        return result;
    }
    /// <summary>
    /// 删除单个箱码绑定。
    /// <para>对应 JC35 接口：【DelStockSoltBoxInfo】</para>
    /// </summary>
    /// <param name="input">删除箱码绑定的请求参数，包含箱码主键ID和操作用户</param>
    /// <returns>删除操作结果，包含成功状态和消息</returns>
    /// <exception cref="Exception">当参数验证失败、箱码不存在或已处理时抛出异常</exception>
    /// <remarks>
    /// 删除规则：
    /// <list type="number">
    /// <item><description>仅删除 Status=0 的箱托关系，避免已入库数据被清除</description></item>
    /// <item><description>当托盘无其余箱码时，软删除入库流水并释放储位/托盘状态</description></item>
    /// <item><description>回滚入库明细的已组数量，必要时将主单恢复为"待执行"</description></item>
    /// </list>
    /// 业务影响：
    /// <list type="bullet">
    /// <item><description>减少入库明细的实际组托数量</description></item>
    /// <item><description>可能触发入库单状态回退</description></item>
    /// <item><description>释放托盘占用状态</description></item>
    /// <item><description>释放相关储位状态</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> DeletePalletBoxAsync(PdaDelBoxInput input)
    {
        if (input == null)    //判断输入是否为空
            throw Oops.Bah("请求参数不能为空！");
        var boxId = ParseRequiredLong(input.BoxId, "箱码主键");    //箱码主键
        var target = await _stockSlotBoxInfoRep.GetFirstAsync(x =>
            x.Id == boxId && x.IsDelete == false && x.Status == 0) ?? throw Oops.Bah("箱托关系不存在或已处理！");    //查询入库箱信息
        var boxes = await _stockSlotBoxInfoRep.AsQueryable()    //查询入库箱信息
            .Where(x => x.IsDelete == false && x.Status == 0 && (x.Id == boxId || x.StockCodeId == target.StockCodeId))
            .ToListAsync();    //查询入库箱信息
        // 处理箱托关系解绑
        return await ProcessRemoveBoxAsync(boxes, target, input.User, string.Empty);    //处理箱托关系解绑
    }
    /// <summary>
    /// JC35【RemoveBoxHolds】迁移实现：托盘箱码解绑。
    /// </summary>
    /// <param name="input">解绑参数。</param>
    /// <returns>操作结果。</returns>
    /// <remarks>
    /// <list type="number">
    /// <item>支持按托盘或入库单解绑（与 JC35 参数一致）。</item>
    /// <item>默认回滚入库明细数量，`type=ys` 场景仅解绑托盘。</item>
    /// <item>若托盘仅剩一个箱码，会同步释放入库流水、储位与托盘状态。</item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> RemoveBoxHoldsAsync(PdaRemoveBoxInput input)
    {
        if (input == null)    //判断输入是否为空
            throw Oops.Bah("请求参数不能为空！");
        var query = _stockSlotBoxInfoRep.AsQueryable()
            .Where(x => x.IsDelete == false &&
                x.Status == 0 &&
                x.ImportOrderId != null &&
                x.ImportOrderId != 0);    //查询入库箱信息
        bool hasPallet = !string.IsNullOrWhiteSpace(input.PalletCode);    //判断托盘编码是否为空
        bool hasImport = !string.IsNullOrWhiteSpace(input.ImportId);    //判断入库单号是否为空
        if (hasPallet)    //判断托盘编码是否为空
        {
            var palletCode = input.PalletCode.Trim();    //托盘编码
            query = query.Where(x => x.StockCode == palletCode);    //查询入库箱信息
        }
        else if (hasImport)    //判断入库单号是否为空
        {
            var importId = ParseRequiredLong(input.ImportId, "入库单信息");    //入库单号
            query = query.Where(x => x.ImportId == importId);    //查询入库箱信息
        }
        else
        {
            throw Oops.Bah("托盘编码或入库单至少提供一项！");
        }
        var boxes = await query.ToListAsync();
        if (boxes.Count == 0)
            throw Oops.Bah("未找到可解绑的箱托关系！");
        WmsStockSlotBoxInfo target;
        if (string.IsNullOrWhiteSpace(input.BoxCode))    //判断箱码是否为空
        {
            target = boxes.First();    //查询入库箱信息
        }
        else
        {
            target = boxes.FirstOrDefault(x =>    //查询入库箱信息
                string.Equals(x.BoxCode ?? string.Empty, input.BoxCode.Trim(), StringComparison.OrdinalIgnoreCase));
            if (target == null)    //判断箱码是否存在
                throw Oops.Bah("目标箱码不存在！");
        }
        // 处理箱托关系解绑
        return await ProcessRemoveBoxAsync(boxes, target, input.User, input.Type);
    }
    /// <summary>
    /// 根据箱码获取组托信息。
    /// <para>对应 JC35 接口：【/PdaInterface/GetImportInfoByBoxCode】</para>
    /// </summary>
    /// <param name="input">查询箱码信息的请求参数，包含箱码、托盘编码、入库单号等</param>
    /// <returns>箱码组托信息查询结果，包含状态、箱码明细和相关信息</returns>
    /// <exception cref="Exception">当参数验证失败、托盘占用、单据状态异常时抛出异常</exception>
    /// <remarks>
    /// 核心流程：
    /// <list type="number">
    /// <item><description>校验托盘是否处于占用状态</description></item>
    /// <item><description>校验入库/验收/挑浆单据状态</description></item>
    /// <item><description>根据箱码获取有效库存箱码记录</description></item>
    /// <item><description>若箱码不存在，尝试根据标签打印信息自动补齐</description></item>
    /// <item><description>校验托盘内是否混放不同单据</description></item>
    /// <item><description>构建返回的组托明细信息</description></item>
    /// </list>
    /// 特殊处理：
    /// <list type="bullet">
    /// <item><description>自动补齐：基于标签打印数据生成箱托记录</description></item>
    /// <item><description>异常清理：检测到历史异常数据时自动清理</description></item>
    /// <item><description>重试机制：支持操作重试状态返回</description></item>
    /// <item><description>混放校验：防止不同单据混放同一托盘</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaBoxInfoOutput> QueryBoxInfoAsync(PdaBoxInfoInput input)
    {
        if (input == null)    //判断输入是否为空
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.BoxCode))    //判断箱码是否为空
            throw Oops.Bah("箱码不能为空！");
        var normalizedBox = input.BoxCode.Trim();    //箱码
        var normalizedPallet = string.IsNullOrWhiteSpace(input.PalletCode)
            ? null
            : input.PalletCode.Trim();    //托盘编码
        var output = new PdaBoxInfoOutput    //生成组托信息
        {
            State = PdaBoxInfoState.None,    //状态
            Box = null    //箱码
        };
        try
        {
            // 校验托盘是否处于占用状态
            var exists = await _stockTrayRep.AsQueryable()
                .Where(x => x.StockCode == normalizedPallet && x.IsDelete == false)
                .AnyAsync();
            if (exists)    //判断托盘是否处于占用状态
                throw Oops.Bah("当前托盘已组托!");
            // 校验入库/验收/挑浆单据状态
            await ValidateImportStateAsync(input);
            //根据箱码获取有效库存箱码记录（仅未删除、未入库数据）
            var boxRecord = await _stockSlotBoxInfoRep.AsQueryable()
                .Where(x => x.BoxCode == normalizedBox)
                .Where(x => x.IsDelete == false && (x.Status == null || x.Status == 0))
                .OrderBy(x => x.CreateTime, OrderByType.Asc)
                .FirstAsync();
            if (boxRecord == null)
            {
                // 根据标签打印信息尝试自动补齐箱托数据
                var creationResult = await TryCreateBoxFromLabelAsync(input, normalizedPallet);
                if (!creationResult.Success)
                {
                    output.Success = false;    //失败
                    output.Message = creationResult.Message ?? "箱码有误！";    //箱码有误
                    output.State = creationResult.RequiresRetry ? PdaBoxInfoState.Retry : PdaBoxInfoState.RequireLabel;
                    output.ExtraMessage = creationResult.Message;
                    return output;    //返回组托信息
                }
                boxRecord = creationResult.Box;    //查询入库箱信息
            }
            if (string.IsNullOrWhiteSpace(boxRecord.StockCode) && !string.IsNullOrWhiteSpace(normalizedPallet))    //判断托盘编码是否为空
            {
                // 将历史异常箱托记录软删除
                await MarkBoxAsDeletedAsync(boxRecord, input.User);
                output.Success = false;    //失败
                output.Message = "请重试";    //请重试
                output.State = PdaBoxInfoState.Retry;    //重试
                output.ExtraMessage = "检测到历史箱托数据异常，已清理原始记录，请重新扫描。";    //检测到历史箱托数据异常，已清理原始记录，请重新扫描。
                return output;    //返回组托信息
            }
            if (string.Equals(input.ImportId?.Trim(), "-1", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(boxRecord.StockCode))    //判断入库单号是否为空
                throw Oops.Bah("此箱码已组托！");
            if (boxRecord.Status == 1 || boxRecord.Status == 2)
                throw Oops.Bah("此箱码已申请入库或已入库！");
            if (!string.IsNullOrWhiteSpace(boxRecord.StockCode) && !string.IsNullOrWhiteSpace(normalizedPallet) && !string.Equals(boxRecord.StockCode, normalizedPallet, StringComparison.OrdinalIgnoreCase))    //判断托盘编码是否一致
                throw Oops.Bah($"箱码已存在于 ({boxRecord.StockCode}) 载具上，请先解绑后再操作。");
            // 校验托盘内是否混放不同单据
            await ValidatePalletMixingAsync(normalizedPallet, boxRecord);
            // 组装箱托返回明细，补充物料编码/单据号等信息供 PDA 展示。
            var detail = await BuildBoxDetailAsync(boxRecord, normalizedPallet);
            output.Success = true;    //成功
            output.Message = "绑定成功";
            output.State = PdaBoxInfoState.Ready;    //准备
            output.Box = detail;    //组托信息
            return output;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询箱托信息失败：{@Input}", input);    //查询箱托信息失败
            output.Success = false;
            output.Message = ex.Message;    //错误信息
            output.State = PdaBoxInfoState.None;    //无状态
            output.Box = null;    //组托信息
            return output;    //返回组托信息
        }
    }
    /// <summary>
    /// 判断托盘是否为有效托盘。
    /// </summary>
    /// <param name="palletCode">托盘编码。</param>
    /// <returns>托盘是否存在且未删除。</returns>
    public async Task<bool> CheckPalletStatusAsync(string palletCode)
    {
        if (string.IsNullOrWhiteSpace(palletCode))    //判断托盘编码是否为空
            return false;
        return await _sysStockCodeRep.AsQueryable()
            .Where(x => x.StockCode == palletCode.Trim() && x.IsDelete == false)
            .AnyAsync();    //判断托盘是否存在且未删除
    }
    /// <summary>
    /// 绑定箱托关系。
    /// <para>对应 JC35 接口：【/PdaInterface/BackConfirm】</para>
    /// </summary>
    /// <param name="input">绑定箱托关系的请求参数，包含箱码、托盘编码、重量、拒绝原因等</param>
    /// <returns>绑定操作结果，包含成功状态和消息</returns>
    /// <exception cref="Exception">当参数验证失败、箱码不存在、绑定失败时抛出异常</exception>
    /// <remarks>
    /// 版本说明：
    /// <list type="bullet">
    /// <item><description>当前版本仅支持标准入库场景（type 空/0）</description></item>
    /// <item><description>验收与挑浆功能将在后续迭代补充</description></item>
    /// </list>
    /// 绑定流程：
    /// <list type="number">
    /// <item><description>验证输入参数和业务类型</description></item>
    /// <item><description>查询并验证箱码组托信息</description></item>
    /// <item><description>验证订单号一致性</description></item>
    /// <item><description>确保托盘存在待执行的入库流水</description></item>
    /// <item><description>更新箱托与托盘、入库流水的绑定关系</description></item>
    /// <item><description>更新入库明细与主单的执行状态和已组数量</description></item>
    /// </list>
    /// 数据更新：
    /// <list type="bullet">
    /// <item><description>箱码：更新托盘关联、入库流水、重量等信息</description></item>
    /// <item><description>托盘：更新使用状态为已占用</description></item>
    /// <item><description>入库流水：累加数量和重量</description></item>
    /// <item><description>入库明细：更新实际组托数量和执行状态</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> BindBoxAsync(PdaBindBoxInput input)
    {
        if (input == null)    //判断输入是否为空
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.BoxCode))    //判断箱码是否为空
            throw Oops.Bah("箱码不能为空！");
        if (string.IsNullOrWhiteSpace(input.PalletCode))    //判断托盘编码是否为空
            throw Oops.Bah("托盘编码不能为空！");
        var scenario = string.IsNullOrWhiteSpace(input.Type) ? "0" : input.Type.Trim();    //场景
        if (!string.Equals(scenario, "0", StringComparison.OrdinalIgnoreCase))    //判断场景是否为标准入库
            throw Oops.Bah("当前版本暂未迁移验收/挑浆绑定逻辑，请联系系统管理员。");
        var normalizedPallet = input.PalletCode.Trim();    //托盘编码
        // 根据箱码获取组托信息
        var boxInfoResult = await QueryBoxInfoAsync(input);    //根据箱码获取组托信息
        if (!boxInfoResult.Success || boxInfoResult.Box == null)    //判断组托信息是否成功
            return new PdaActionResult    //生成组托信息结果
            {
                Success = false,    //失败
                Message = boxInfoResult.Message    //错误信息
            };
        if (boxInfoResult.State != PdaBoxInfoState.Ready)    //判断组托信息状态是否为准备
            return new PdaActionResult    //生成组托信息结果
            {
                Success = false,    //失败
                Message = boxInfoResult.ExtraMessage ?? boxInfoResult.Message    //错误信息
            };
        var boxEntity = await _stockSlotBoxInfoRep.GetFirstAsync(x => x.Id == boxInfoResult.Box.BoxId && x.IsDelete == false) ?? throw Oops.Bah("箱码数据不存在，请重新扫描！");    //查询入库箱信息
        // 验箱码所属单据是否与前端传入的订单号一致                                                                                                                                                             
        await ValidateOrderConsistencyAsync(boxEntity, input);
        var user = NormalizeUser(input.User);    //用户
        var now = DateTime.Now;    //当前时间
        WmsSysStockCode? palletEntity = null;
        WmsImportOrder? importOrder = null;
        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                // 获取托盘主数据
                palletEntity = await _sysStockCodeRep.GetFirstAsync(x => x.StockCode == normalizedPallet && x.IsDelete == false);
                // 保证托盘存在对应的入库流水记录
                importOrder = await EnsureImportOrderAsync(palletEntity, boxEntity, input, now, user);
                // 更新箱托绑定关系并刷新托盘状态
                await UpdateBoxBindingAsync(boxEntity, palletEntity, importOrder, input, now, user);
                // 更新入库明细与主单的执行状态/已组数
                await UpdateImportQuantitiesAsync(boxEntity, now, user);
            }, "绑定箱托失败");
            _logger.LogInformation("绑定箱托成功：BoxCode={BoxCode}, Pallet={Pallet}, ImportOrder={Order}", boxEntity.BoxCode, palletEntity?.StockCode, importOrder?.ImportOrderNo);
            return new PdaActionResult
            {
                Success = true,
                Message = "操作成功"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "绑定箱托失败：{@Input}", input);
            throw;
        }
    }
    /// <summary>
    /// 空托盘组盘（绑定/解绑）。
    /// <para>对应 JC35 接口：【/PdaInterface/KBackConfirm】</para>
    /// </summary>
    /// <param name="input">空托盘组盘的请求参数，包含托盘编码、数量、动作类型、用户等</param>
    /// <returns>组盘操作结果，包含成功状态和消息</returns>
    /// <exception cref="Exception">当参数验证失败、托盘不存在、组盘失败时抛出异常</exception>
    /// <remarks>
    /// 操作类型：
    /// <list type="bullet">
    /// <item><description>add：绑定空托盘，需要数量 > 0</description></item>
    /// <item><description>del：解绑空托盘，数量可为 0</description></item>
    /// </list>
    /// 业务规则：
    /// <list type="number">
    /// <item><description>绑定操作：验证托盘存在且可用，数量必须大于0</description></item>
    /// <item><description>解绑操作：移除空托盘绑定关系</description></item>
    /// <item><description>自动更新托盘状态和相关入库流水</description></item>
    /// <item><description>兼容JC35原项目的参数验证逻辑</description></item>
    /// </list>
    /// 应用场景：
    /// <list type="bullet">
    /// <item><description>空托盘入库前组盘操作</description></item>
    /// <item><description>空托盘状态管理</description></item>
    /// <item><description>托盘占用状态控制</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> KBackConfirmAsync(PdaEmptyTrayBindInput input)
    {
        if (input == null)    //判断输入是否为空
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.PalletCode))    //判断托盘编码是否为空
            throw Oops.Bah("托盘编码不能为空！");
        var actionType = NormalizeActionType(input.ActionType, "add");    //规范化动作类型
        // 对照 JC35 逻辑：绑定（add）时需要验证数量 > 0，解绑（del）时不需要（原项目解绑时 num 可为 0）
        if (string.Equals(actionType, "add", StringComparison.OrdinalIgnoreCase) && input.Quantity <= 0)    //判断数量是否大于0（仅绑定操作）
            throw Oops.Bah("数量必须大于 0！");
        var request = new EmptyTrayBindInput    //生成空托盘组盘参数
        {
            PalletCode = input.PalletCode.Trim(),    //托盘编码
            Quantity = input.Quantity,    //数量（解绑时可为 0，不会被使用）
            ActionType = actionType,    //动作类型
            UserId = NormalizeUser(input.User)    //用户
        };
        var result = await _emptyTrayBind.ProcessKBackConfirmAsync(request);    //处理空托盘组盘
        return new PdaActionResult    //生成组托信息结果
        {
            Success = result.Success,    //成功
            Message = result.Message    //错误信息
        };
    }
    /// <summary>
    /// 批量生成空托入库流水。
    /// <para>对应 JC35 接口：【AddWmsImportOrder】</para>
    /// </summary>
    /// <returns>批量生成操作结果，包含成功状态、消息和处理数量</returns>
    /// <exception cref="Exception">当批量处理过程中发生严重错误时抛出异常</exception>
    /// <remarks>
    /// 筛选条件：
    /// <list type="bullet">
    /// <item><description>托盘编码以1、3、5、7、9结尾的奇数托盘</description></item>
    /// <item><description>托盘状态为0（未使用）</description></item>
    /// <item><description>托盘未被其他组托记录占用</description></item>
    /// </list>
    /// 处理逻辑：
    /// <list type="number">
    /// <item><description>筛选符合条件的空托盘候选集合</description></item>
    /// <item><description>排除已被占用的托盘</description></item>
    /// <item><description>逐个调用空托盘绑定服务</description></item>
    /// <item><description>使用默认数量2进行绑定</description></item>
    /// <item><description>统计成功和失败的数量</description></item>
    /// </list>
    /// 容错机制：
    /// <list type="bullet">
    /// <item><description>单个托盘失败不影响其他托盘处理</description></item>
    /// <item><description>记录失败托盘的详细错误信息</description></item>
    /// <item><description>返回总体处理结果和成功数量</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> BatchGenerateEmptyTrayOrdersAsync()
    {
        var candidates = await _sysStockCodeRep.AsQueryable()    //查询库存编码
            .Where(x => x.IsDelete == false && x.Status == 0)    //查询库存编码
            .Where(x => SqlFunc.EndsWith(x.StockCode, "1")
                     || SqlFunc.EndsWith(x.StockCode, "3")    //查询库存编码
                     || SqlFunc.EndsWith(x.StockCode, "5")    //查询库存编码
                     || SqlFunc.EndsWith(x.StockCode, "7")    //查询库存编码
                     || SqlFunc.EndsWith(x.StockCode, "9"))    //查询库存编码
            .Select(x => new { x.Id, x.StockCode })    //查询库存编码
            .ToListAsync();    //查询库存编码
        if (candidates.Count == 0)    //判断库存编码是否为空
            return new PdaActionResult    //生成组托信息结果
            {
                Success = false,    //失败
                Message = "没有符合条件的托盘！"    //没有符合条件的托盘！
            };
        var candidateIds = candidates.Select(x => x.Id).ToList();    //查询库存编码ID
        var occupiedIds = await _stockSlotBoxInfoRep.AsQueryable()    //查询库存箱码信息
            .Where(x => x.IsDelete == false && (x.Status == 0 || x.Status == 1))    //查询库存箱码信息
            .Where(x => x.StockCodeId != null && candidateIds.Contains(x.StockCodeId.Value))    //查询库存箱码信息
            .Select(x => x.StockCodeId.Value)    //查询库存箱码信息
            .Distinct()    //查询库存箱码信息
            .ToListAsync();    //查询库存箱码信息
        var occupiedSet = new HashSet<long>(occupiedIds);    //查询库存箱码信息
        var successCount = 0;    //查询库存箱码信息
        foreach (var candidate in candidates)    //查询库存编码
        {
            if (occupiedSet.Contains(candidate.Id))    //判断库存编码是否为空
                continue;
            try
            {
                var response = await _emptyTrayBind.ProcessKBackConfirmAsync(new EmptyTrayBindInput    //生成空托盘组盘参数
                {
                    PalletCode = candidate.StockCode,    //托盘编码
                    Quantity = DefaultEmptyTrayQuantity,    //默认空托盘数量
                    ActionType = "add",    //动作类型
                    UserId = "PDA-BATCH"    //用户
                });
                if (response.Success)    //判断是否成功
                    successCount++;    //成功数量
                else
                    _logger.LogWarning("空托批量绑定失败：Pallet={Pallet}, Message={Message}", candidate.StockCode, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "空托批量绑定发生异常：Pallet={Pallet}", candidate.StockCode);
            }
        }
        return new PdaActionResult
        {
            Success = successCount > 0,
            Message = successCount > 0
                ? $"批量绑定完成，共成功 {successCount} 条"
                : "没有可绑定的托盘！"
        };
    }
    /// <summary>
    /// JC35【TemporaryStorage】迁移实现：生成暂存入库流水。
    /// </summary>
    /// <param name="input">暂存入库参数。</param>
    /// <returns>操作结果。</returns>
    public async Task<PdaActionResult> TemporaryStorageAsync(PdaTemporaryStorageInput input)
    {
        if (input == null)    //判断输入是否为空
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.VehicleCode))    //判断主载具是否为空
            throw Oops.Bah("主载具不能为空！");
        var user = NormalizeUser(input.UserId);    //用户
        var now = DateTime.Now;    //当前时间
        var trayCodes = CollectTrayCodes(input.VehicleCode, input.VehicleSubCode);    //收集托盘编码
        var exportOrders = new List<WmsExportOrder>();    //查询出库订单
        foreach (var trayCode in trayCodes)    //查询托盘编码
        {
            var order = await ResolveLatestExportOrderAsync(trayCode);    //查询最新出库订单
            exportOrders.Add(order);    //添加出库订单  
        }
        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                foreach (var order in exportOrders)    //查询出库订单
                {
                    var stockCode = await ResolveStockCodeAsync(order.ExportStockCode);    //查询库存编码
                    var importOrder = await CreateTemporaryImportOrderAsync(order, stockCode, user, now, input.VehicleCode.Trim());    //创建临时入库流水
                    await RelinkBoxesToImportOrderAsync(order.ExportOrderNo, importOrder.Id, now, user);    //重新绑定箱码到入库流水
                }
            }, "暂存入库生成失败！");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "暂存入库生成失败：{@Input}", input);
            throw;
        }
        var message = exportOrders.Count > 0
            ? $"绑定完成，共生成 {exportOrders.Count} 条入库流水"
            : "未生成任何入库流水！";
        return new PdaActionResult
        {
            Success = exportOrders.Count > 0,
            Message = message
        };
    }
    /// <summary>
    /// JC35【StackingBindings】迁移实现：叠箱绑定（主/副载具合并）。
    /// </summary>
    /// <param name="input">叠箱绑定参数。</param>
    /// <returns>操作结果。</returns>
    public async Task<PdaActionResult> StackingBindingsAsync(PdaStackingBindingInput input)
    {
        if (input == null)    //判断输入是否为空
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.VehicleCode))    //判断主载具是否为空
            throw Oops.Bah("主载具不能为空！");
        if (string.IsNullOrWhiteSpace(input.VehicleSubCode))    //判断副载具是否为空
            throw Oops.Bah("副载具不能为空！");
        var primaryCode = input.VehicleCode.Trim();    //主载具编码
        var secondaryCode = input.VehicleSubCode.Trim();    //副载具编码
        var user = NormalizeUser(input.UserId);    //用户
        var now = DateTime.Now;    //当前时间
        var primaryOrder = await ResolveLatestPendingImportOrderAsync(primaryCode, "主载具");    //查询最新待入库流水
        var secondaryOrder = await ResolveLatestPendingImportOrderAsync(secondaryCode, "副载具");    //查询最新待入库流水
        var primaryBiz = primaryOrder.YsOrTJ ?? string.Empty;    //主载具业务
        var secondaryBiz = secondaryOrder.YsOrTJ ?? string.Empty;    //副载具业务
        if (!string.Equals(primaryBiz, secondaryBiz, StringComparison.OrdinalIgnoreCase))    //判断主载具业务是否与副载具业务一致
            throw Oops.Bah("两个载具业务不同！");
        primaryOrder.SubVehicleCode = primaryCode;    //主载具编码
        primaryOrder.UpdateTime = now;    //更新时间
        primaryOrder.UpdateUserName = user;    //更新用户
        secondaryOrder.SubVehicleCode = primaryCode;    //主载具编码
        secondaryOrder.UpdateTime = now;    //更新时间
        secondaryOrder.UpdateUserName = user;    //更新用户
        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                await _importOrderRep.AsUpdateable(primaryOrder)    //更新主载具订单
                    .UpdateColumns(x => new { x.SubVehicleCode, x.UpdateTime, x.UpdateUserName })
                    .ExecuteCommandAsync();
                await _importOrderRep.AsUpdateable(secondaryOrder)    //更新副载具订单
                    .UpdateColumns(x => new { x.SubVehicleCode, x.UpdateTime, x.UpdateUserName })
                    .ExecuteCommandAsync();
            }, "叠箱绑定失败！");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "叠箱绑定失败：{@Input}", input);
            throw;
        }
        return new PdaActionResult
        {
            Success = true,
            Message = "操作成功"
        };
    }
    /// <summary>
    /// 托盘明细查询。
    /// <para>对应 JC35 接口：【GetPalnoDetailInfo】</para>
    /// </summary>
    /// <param name="input">托盘明细查询的请求参数，包含托盘编码</param>
    /// <returns>托盘下箱码聚合结果，包含成功状态、消息和明细列表</returns>
    /// <exception cref="Exception">当参数验证失败或查询过程中发生错误时抛出异常</exception>
    /// <remarks>
    /// 查询条件：
    /// <list type="number">
    /// <item><description>仅检索 Status=0、IsDelete=0 的箱码记录</description></item>
    /// <item><description>按托盘编码精确匹配</description></item>
    /// <item><description>按箱码降序排列</description></item>
    /// </list>
    /// 聚合规则：
    /// <list type="bullet">
    /// <item><description>按箱码、物料编码、物料名称、批号分组</description></item>
    /// <item><description>相同分组内累加数量</description></item>
    /// <item><description>返回分组的第一条记录的详细信息和累加数量</description></item>
    /// </list>
    /// 兼容性：
    /// <list type="bullet">
    /// <item><description>返回与JC35相同的字段结构</description></item>
    /// <item><description>保留原中文提示文案</description></item>
    /// <item><description>查询失败时仍返回友好的错误信息</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaPalletDetailOutput> QueryPalletDetailAsync(PdaPalletDetailInput input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.PalletCode))    //判断输入是否为空
        {
            throw Oops.Bah("托盘编码不能为空！");
        }
        var output = new PdaPalletDetailOutput();
        try
        {
            var palletCode = input.PalletCode.Trim();    //托盘编码
            var rows = await _sqlViewService.QueryStockSlotBoxInfoView()
                .MergeTable()
                .Where(x => x.IsDelete == false && x.Status == 0)
                .Where(x => x.StockCode == palletCode)
                .OrderBy(x => x.BoxCode, OrderByType.Desc)
                .Select(x => new
                {
                    LotNo = SqlFunc.Coalesce(x.LotNo, string.Empty),
                    BoxCode = SqlFunc.Coalesce(x.BoxCode, string.Empty),
                    Qty = SqlFunc.Coalesce(x.Qty, 0m),
                    MaterialCode = SqlFunc.Coalesce(x.MaterialCode, string.Empty),
                    MaterialName = SqlFunc.Coalesce(x.MaterialName, string.Empty)
                })
                .ToListAsync();
            output.Items = rows
                .GroupBy(x => new { x.BoxCode, x.MaterialCode, x.MaterialName, x.LotNo })
                .Select(group => new PdaPalletDetailItem
                {
                    BoxCode = group.Key.BoxCode,
                    MaterialCode = group.Key.MaterialCode,
                    MaterialName = group.Key.MaterialName,
                    LotNo = group.Key.LotNo,
                    Qty = group.First().Qty ?? 0m,
                    Quantity = group.Sum(item => item.Qty ?? 0m)
                }).OrderByDescending(item => item.BoxCode).ToList();
            output.Success = true;
            output.Message = output.Items.Count > 0 ? "查询成功" : "未找到相关记录";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询托盘明细失败：{@Input}", input);
            output.Success = false;
            output.Message = ex.Message;
        }
        return output;
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 处理托盘箱码解绑的入口封装，统一包裹事务与错误处理。
    /// </summary>
    /// <param name="boxes">同托盘待解绑的箱码集合</param>
    /// <param name="target">当前指定的箱码对象</param>
    /// <param name="user">操作人</param>
    /// <param name="type">解绑类型（验收/常规）</param>
    /// <returns>执行结果，包含成功状态和消息</returns>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>统一管理事务边界，确保数据一致性</description></item>
    /// <item><description>集中处理异常，记录详细日志</description></item>
    /// <item><description>调用核心解绑逻辑</description></item>
    /// <item><description>标准化返回结果格式</description></item>
    /// </list>
    /// </remarks>
    private async Task<PdaActionResult> ProcessRemoveBoxAsync(List<WmsStockSlotBoxInfo> boxes, WmsStockSlotBoxInfo target, string user, string type)
    {
        var result = new PdaActionResult();
        try
        {
            await ExecuteInTransactionAsync(() => RemoveBoxInternalAsync(boxes, target, user, type), "解除箱托关系失败");
            result.Success = true;
            result.Message = "操作成功";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解除箱托关系失败：BoxId={BoxId}", target.Id);
            result.Success = false;
            result.Message = ex.Message;
        }
        return result;
    }
    /// <summary>
    /// 执行箱托解绑核心逻辑：回滚明细、释放托盘、删除流水等。
    /// </summary>
    /// <param name="boxes">托盘剩余箱码集合。</param>
    /// <param name="target">目标箱码。</param>
    /// <param name="user">操作人。</param>
    /// <param name="type">解绑类型（验收/常规）。</param>
    private async Task RemoveBoxInternalAsync(List<WmsStockSlotBoxInfo> boxes, WmsStockSlotBoxInfo target, string user, string type)
    {
        if (boxes == null || boxes.Count == 0)    //判断箱码集合是否为空
            throw Oops.Bah("箱托关系不存在！");
        if (target.Status > 0)    //判断托盘状态是否大于0
            throw Oops.Bah("托盘已入库，无法解绑！");
        var now = DateTime.Now;    //当前时间
        user = NormalizeUser(user);
        var otherBoxes = boxes.Where(x => x.Id != target.Id).ToList();    //查询其他箱码
        WmsImportOrder? order = null;    //查询入库流水
        if (target.ImportOrderId.HasValue && target.ImportOrderId.Value != 0)    //判断入库流水ID是否为空
        {
            order = await _importOrderRep.GetFirstAsync(x => x.Id == target.ImportOrderId.Value && x.IsDelete == false);    //查询入库流水
        }
        if (otherBoxes.Count == 0 && order != null)    //判断其他箱码是否为空
        {
            order.IsDelete = true;    //删除入库流水
            order.UpdateTime = now;    //更新时间
            order.UpdateUserName = user;    //更新用户
            await _importOrderRep.AsUpdateable(order).UpdateColumns(x => new { x.IsDelete, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();    //更新入库流水
            if (!string.IsNullOrWhiteSpace(order.ImportSlotCode))    //判断入库库位编码是否为空
            {
                await _baseSlotRep.AsUpdateable().SetColumns(slot => new WmsBaseSlot { SlotStatus = 0, UpdateTime = now })
                    .Where(slot => slot.SlotCode == order.ImportSlotCode).ExecuteCommandAsync();    //更新入库库位
            }
            var remainOrders = await _importOrderRep.AsQueryable().Where(x => x.StockCodeId == target.StockCodeId && x.IsDelete == false).CountAsync();    //查询剩余入库流水
            if (remainOrders == 0)
            {
                var trayExists = await _stockTrayRep.AsQueryable().Where(x => x.StockCode == target.StockCode).Where(x => x.IsDelete == false).CountAsync();    //查询托盘是否存在      
                if (trayExists == 0)    //判断托盘是否存在
                {
                    await _sysStockCodeRep.AsUpdateable()
                        .SetColumns(code => new WmsSysStockCode    //更新库存编码
                        {
                            Status = 0,    //状态
                            UpdateTime = now,    //更新时间
                            UpdateUserName = user    //更新用户
                        }).Where(code => code.Id == target.StockCodeId).ExecuteCommandAsync();    //更新库存编码
                }
            }
        }
        if (!string.Equals(type, "ys", StringComparison.OrdinalIgnoreCase) && target.ImportDetailId.HasValue)    //判断解绑类型是否为验收
        {
            var detail = await _importNotifyDetailRep.GetFirstAsync(x => x.Id == target.ImportDetailId.Value && x.IsDelete == false);    //查询入库单据明细
            if (detail != null)
            {
                var factQty = (detail.ImportFactQuantity ?? 0) - (target.Qty ?? 0);    //计算实际数量
                if (factQty < 0)    //判断实际数量是否小于0
                {
                    factQty = 0;    //实际数量为0
                }
                detail.ImportFactQuantity = factQty;    //更新实际数量
                if (factQty == 0)    //判断实际数量是否为0
                {
                    detail.ImportExecuteFlag = "01";    //更新执行标志
                    if (detail.ImportId.HasValue)    //判断入库单据ID是否为空
                    {
                        await _importNotifyRep.AsUpdateable()    //更新入库单据
                            .SetColumns(n => new WmsImportNotify
                            {
                                ImportExecuteFlag = "01",    //执行标志
                                UpdateTime = now,    //更新时间
                                UpdateUserName = user    //更新用户
                            }).Where(n => n.Id == detail.ImportId.Value).ExecuteCommandAsync();    //更新入库单据
                    }
                }
                detail.UpdateTime = now;    //更新时间
                detail.UpdateUserName = user;    //更新用户
                await _importNotifyDetailRep.AsUpdateable(detail).UpdateColumns(x => new { x.ImportFactQuantity, x.ImportExecuteFlag, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();    //更新入库单据明细
            }
        }
        target.IsDelete = true;    //删除箱码
        target.StockCode = null;    //库存编码
        target.StockCodeId = null;    //库存编码ID
        target.ImportOrderId = null;    //入库流水ID
        target.Weight = 0;    //重量
        target.UpdateTime = now;    //更新时间
        target.UpdateUserName = user;    //更新用户
        await _stockSlotBoxInfoRep.AsUpdateable(target).UpdateColumns(x => new    //更新库存箱码信息
        {
            x.IsDelete,    //是否删除
            x.StockCode,    //库存编码
            x.StockCodeId,    //库存编码ID
            x.ImportOrderId,    //入库流水ID
            x.Weight,    //重量
            x.UpdateTime,    //更新时间
            x.UpdateUserName    //更新用户
        }).ExecuteCommandAsync();    //更新库存箱码信息
    }

    /// <summary>
    /// 校验入库/验收/挑浆单据状态，确保仍允许绑定操作。
    /// </summary>
    /// <param name="input">PDA 扫码入参。</param>
    private async Task ValidateImportStateAsync(PdaBoxInfoInput input)
    {
        var rawImport = input.ImportId?.Trim();    //入库单据ID
        if (string.IsNullOrWhiteSpace(rawImport) || string.Equals(rawImport, "-1", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        var scenario = string.IsNullOrWhiteSpace(input.Type) ? "0" : input.Type.Trim();    //场景
        if (string.Equals(scenario, "3", StringComparison.OrdinalIgnoreCase) || string.Equals(scenario, "4", StringComparison.OrdinalIgnoreCase))    //判断场景是否为3或4
        {
            throw Oops.Bah("挑浆单业务暂未迁移，请联系系统管理员。");
        }
        if (string.Equals(scenario, "1", StringComparison.OrdinalIgnoreCase))
        {
            var inspectQuery = _inspectNotifyRep.AsQueryable().Where(x => x.IsDelete == false);    //查询验收通知
            var inspectId = TryParseLong(rawImport);    //解析验收通知ID
            if (inspectId.HasValue)
            {
                inspectQuery = inspectQuery.Where(x => x.Id == inspectId.Value);    //查询验收通知
            }
            else
            {
                inspectQuery = inspectQuery.Where(x => x.InspectBillCode == rawImport);    //查询验收通知   
            }
            var inspect = await inspectQuery.Select(x => new { x.Id, x.Status }).FirstAsync();    //查询验收通知
            if (inspect == null)
            {
                throw Oops.Bah("验收单不存在!");
            }
            if (inspect.Status == 2)
            {
                throw Oops.Bah("验收单已完成!");
            }
            return;
        }
        var importQuery = _importNotifyRep.AsQueryable().Where(x => x.IsDelete == false);    //查询入库单据
        var importId = TryParseLong(rawImport);    //解析入库单据ID
        if (importId.HasValue)
        {
            importQuery = importQuery.Where(x => x.Id == importId.Value);    //查询入库单据
        }
        else
        {
            importQuery = importQuery.Where(x => x.ImportBillCode == rawImport);    //查询入库单据
        }
        var notify = await importQuery.Select(x => new { x.Id, x.ImportExecuteFlag }).FirstAsync();    //查询入库单据
        if (notify == null)
            throw Oops.Bah("入库单不存在!");
        if (string.Equals(notify.ImportExecuteFlag, "03", StringComparison.OrdinalIgnoreCase))
            throw Oops.Bah("入库单已完成!");
    }

    /// <summary>
    /// 尝试基于标签打印数据自动补齐箱托记录。
    /// </summary>
    /// <param name="input">PDA 扫码入参。</param>
    /// <param name="palletCode">待绑定托盘编码。</param>
    /// <returns>补齐结果、是否需重试及补齐后的箱托。</returns>
    private async Task<(bool Success, bool RequiresRetry, WmsStockSlotBoxInfo Box, string? Message)> TryCreateBoxFromLabelAsync(PdaBoxInfoInput input, string? palletCode)
    {
        var scenario = string.IsNullOrWhiteSpace(input.Type) ? "0" : input.Type.Trim();    //场景
        if (string.Equals(scenario, "3", StringComparison.OrdinalIgnoreCase) || string.Equals(scenario, "4", StringComparison.OrdinalIgnoreCase))
        {
            return (false, false, null, "挑浆单业务暂未迁移，请联系系统管理员。");
        }
        var label = await _importLabelPrintRep.AsQueryable()    //查询标签打印信息
            .Where(x => x.LabelID == input.BoxCode.Trim())
            .Where(x => x.IsDelete == false)    //查询标签打印信息
            .OrderBy(x => x.CreateTime, OrderByType.Desc)    //按创建时间降序
            .Select(x => new    //查询标签打印信息
            {
                x.Id,    //ID
                x.LabelID,    //标签ID
                x.ImportId,    //入库单据ID
                x.ImportDetailId,    //入库单据明细ID
                x.Quantity,    //数量
                x.LotNo,    //批次号
                x.MxFlag    //标记
            }).FirstAsync();    //查询标签打印信息
        if (label == null || string.IsNullOrWhiteSpace(label.LabelID))
            return (false, false, null, "箱码有误!");    //箱码有误
        var detail = await _importNotifyDetailRep.GetFirstAsync(x => x.Id == label.ImportDetailId && x.IsDelete == false);
        if (detail == null)
            return (false, false, null, "未找到入库单明细，无法生成箱码信息！");
        long? importId = label.ImportId ?? detail.ImportId;    //入库单据ID
        if (string.Equals(scenario, "1", StringComparison.OrdinalIgnoreCase))    //判断场景是否为1
        {
            var inspect = await _inspectNotifyRep.AsQueryable()
                .Where(x => x.ImportDetailId == detail.Id && x.IsDelete == false)
                .Select(x => new { x.Id, x.Status })
                .FirstAsync();    //查询验收通知
            if (inspect == null)
                return (false, false, null, "验收单不存在!");
            if (inspect.Status == 2)
                return (false, false, null, "验收单已完成!");
            importId = inspect.Id;
        }
        WmsSysStockCode? palletEntity = null;
        if (!string.IsNullOrWhiteSpace(palletCode))    //判断托盘编码是否为空
        {
            palletEntity = await _sysStockCodeRep.GetFirstAsync(x =>
                x.StockCode == palletCode && x.IsDelete == false);    //查询托盘信息
            if (palletEntity == null)
                throw Oops.Bah("当前扫描的托盘有误！");    //当前扫描的托盘有误
        }
        var now = DateTime.Now;    //当前时间
        var user = NormalizeUser(input.User);
        var inspectionStatus = 0;    //检验状态
        if (!string.IsNullOrWhiteSpace(detail.MaterialStatus) &&    //判断物料状态是否为空
            int.TryParse(detail.MaterialStatus, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedStatus))
        {
            inspectionStatus = parsedStatus;    //解析物料状态
        }
        var newBox = new WmsStockSlotBoxInfo
        {
            Id = SnowFlakeSingle.Instance.NextId(),    //生成ID
            StockCodeId = palletEntity?.Id,    //库存编码ID
            StockCode = palletEntity?.StockCode,    //库存编码  
            BoxCode = label.LabelID,    //箱码
            Qty = label.Quantity ?? 0,    //数量
            FullBoxQty = label.Quantity ?? 0,    //满箱数量
            CreateUserName = user,    //创建用户
            CreateTime = now,    //创建时间
            UpdateUserName = user,    //更新用户
            UpdateTime = now,    //更新时间
            Status = 0,    //状态
            BoxLevel = 1,    //箱级
            ProductionDate = detail.ImportProductionDate,    //生产日期
            ValidateDay = detail.ImportLostDate,    //失效日期
            LotNo = label.LotNo ?? detail.LotNo,    //批次号
            MaterialId = detail.MaterialId,
            BulkTank = label.MxFlag ?? 0,    //罐号
            ImportId = importId,    //入库单据ID
            ImportDetailId = detail.Id,    //入库单据明细ID
            IsDelete = false,    //是否删除
            QRCode = input.BoxCode.Trim(),    //二维码
            InspectionStatus = inspectionStatus
        };
        if (!await ValidateMaterialMatchAsync(newBox))
        {
            return (false, false, null, "箱码与订单号不匹配!");
        }
        await _stockSlotBoxInfoRep.InsertAsync(newBox);    //插入箱托信息
        _logger.LogInformation("根据标签打印信息自动生成箱托数据：BoxCode={BoxCode}, ImportId={ImportId}", newBox.BoxCode, newBox.ImportId);    //根据标签打印信息自动生成箱托数据
        var persisted = await _stockSlotBoxInfoRep.GetFirstAsync(x => x.Id == newBox.Id);    //查询箱托信息
        return (true, false, persisted, null);    //返回结果
    }

    /// <summary>
    /// 软删除历史异常箱托记录，避免重复绑定。
    /// </summary>
    /// <param name="box">目标箱托数据。</param>
    /// <param name="user">操作人。</param>
    private async Task MarkBoxAsDeletedAsync(WmsStockSlotBoxInfo box, string? user)
    {
        var now = DateTime.Now;    //当前时间
        var operatorName = NormalizeUser(user);    //操作人
        await _stockSlotBoxInfoRep.AsUpdateable()    //更新箱托信息
            .SetColumns(x => new WmsStockSlotBoxInfo
            {
                IsDelete = true,    //是否删除
                UpdateTime = now,    //更新时间
                UpdateUserName = operatorName    //更新用户
            }).Where(x => x.Id == box.Id).ExecuteCommandAsync();    //更新箱托信息
    }

    /// <summary>
    /// 校验托盘内是否混放不同单据的箱码。
    /// </summary>
    /// <param name="palletCode">托盘编码。</param>
    /// <param name="candidate">当前待绑定箱码。</param>
    private async Task ValidatePalletMixingAsync(string? palletCode, WmsStockSlotBoxInfo candidate)
    {
        if (string.IsNullOrWhiteSpace(palletCode))    //判断托盘编码是否为空
            return;    //托盘编码为空
        var existing = await _stockSlotBoxInfoRep.AsQueryable()    //查询箱托信息
            .Where(x => x.StockCode == palletCode && x.IsDelete == false && x.Status == 0)    //查询箱托信息
            .OrderBy(x => x.CreateTime, OrderByType.Asc)    //按创建时间升序    
            .FirstAsync();    //查询箱托信息
        if (existing == null || existing.Id == candidate.Id)    //判断箱托信息是否为空或ID是否相同
            return;    //箱托信息为空或ID相同
        if (existing.ImportId.HasValue && candidate.ImportId.HasValue && existing.ImportId != candidate.ImportId)
            throw Oops.Bah("当前托盘已绑定其他入库单，请更换托盘！");
    }

    /// <summary>
    /// 构建 PDA 端展示的箱托明细信息。
    /// </summary>
    /// <param name="box">数据库箱托实体。</param>
    /// <param name="fallbackPallet">缺失托盘时使用的回退编码。</param>
    /// <returns>封装后的明细。</returns>
    private async Task<PdaBoxInfoDetail> BuildBoxDetailAsync(WmsStockSlotBoxInfo box, string? fallbackPallet)
    {
        string materialCode = string.Empty;    //物料编码
        if (box.MaterialId.HasValue)    //判断物料ID是否为空
        {
            materialCode = await _materialRep.AsQueryable()    //查询物料信息
                .Where(x => x.Id == box.MaterialId.Value && x.IsDelete == false)    //查询物料信息
                .Select(x => x.MaterialCode)    //查询物料编码
                .FirstAsync() ?? string.Empty;    //查询物料编码
        }
        var importBillCode = await GetImportBillCodeAsync(box);    //获取入库单据号
        return new PdaBoxInfoDetail
        {
            BoxId = box.Id,    //箱托ID
            BoxCode = box.BoxCode ?? string.Empty,    //箱码
            MaterialId = box.MaterialId,    //物料ID
            MaterialCode = materialCode,    //物料编码
            LotNo = box.LotNo ?? string.Empty,    //批次号
            Quantity = box.Qty ?? 0,    //数量  
            PalletCode = !string.IsNullOrWhiteSpace(box.StockCode)
                ? box.StockCode
                : (fallbackPallet ?? string.Empty),
            ImportId = box.ImportId,    //入库单据ID
            ImportBillCode = importBillCode,    //入库单据号
            ImportDetailId = box.ImportDetailId,    //入库单据明细ID
            InspectionStatus = box.InspectionStatus,    //检验状态
            PickingSlurry = box.PickingSlurry ?? "0",    //挑浆状态
            ExtractStatus = box.ExtractStatus,    //提取状态
            QrCode = box.QRCode ?? string.Empty    //二维码
        };
    }

    /// <summary>
    /// 获取箱码对应的入库/验收单据号。
    /// </summary>
    /// <param name="box">箱托信息。</param>
    /// <returns>单据号。</returns>
    private async Task<string> GetImportBillCodeAsync(WmsStockSlotBoxInfo box)
    {
        if (!box.ImportId.HasValue)    //判断入库单据ID是否为空     
            return string.Empty;    //入库单据ID为空
        var notifyEntity = await _importNotifyRep.GetFirstAsync(x => x.Id == box.ImportId.Value && x.IsDelete == false);    //查询入库单据信息
        var notifyCode = notifyEntity?.ImportBillCode;    //入库单据单据号
        if (!string.IsNullOrWhiteSpace(notifyCode))
            return notifyCode;    //入库单据单据号
        var inspectEntity = await _inspectNotifyRep.GetFirstAsync(x => x.Id == box.ImportId.Value && x.IsDelete == false);    //查询验收通知信息
        return inspectEntity?.InspectBillCode ?? string.Empty;    //验收通知单据号
    }

    /// <summary>
    /// 校验 PDA 输入的订单号是否与箱码归属一致。
    /// </summary>
    /// <param name="box">箱托信息。</param>
    /// <param name="input">绑定入参。</param>
    private async Task ValidateOrderConsistencyAsync(WmsStockSlotBoxInfo box, PdaBindBoxInput input)
    {
        if (string.IsNullOrWhiteSpace(input.OrderNo))    //判断订单号是否为空
            return;
        var expectedOrder = await GetImportBillCodeAsync(box);    //获取入库单据号
        if (string.IsNullOrWhiteSpace(expectedOrder))    //判断入库单据号是否为空
            return;
        if (!string.Equals(expectedOrder, input.OrderNo.Trim(), StringComparison.OrdinalIgnoreCase))    //判断入库单据号是否与订单号匹配
            throw Oops.Bah("箱码与订单号不匹配!");
    }

    /// <summary>
    /// 确保托盘存在待执行的入库流水，不存在则自动创建。
    /// </summary>
    /// <param name="pallet">托盘主数据。</param>
    /// <param name="box">箱托信息。</param>
    /// <param name="input">绑定请求。</param>
    /// <param name="now">当前时间。</param>
    /// <param name="user">操作人。</param>
    /// <returns>可用的入库流水。</returns>
    private async Task<WmsImportOrder> EnsureImportOrderAsync(WmsSysStockCode pallet, WmsStockSlotBoxInfo box, PdaBindBoxInput input, DateTime now, string user)
    {
        var orderQuery = _importOrderRep.AsQueryable()    //查询入库流水
            .Where(x => x.IsDelete == false && x.ImportExecuteFlag == "01" && x.StockCode == pallet.StockCode)
            .OrderBy(x => x.CreateTime, OrderByType.Desc);    //按创建时间降序
        var existingOrder = await orderQuery.FirstAsync();    //查询入库流水
        var weightDelta = input.Weight ?? box.Weight ?? 0.666m;    //重量差值
        if (existingOrder == null)
        {
            var newOrder = new WmsImportOrder
            {
                Id = SnowFlakeSingle.Instance.NextId(),    //生成ID
                ImportOrderNo = _commonMethod.GetImExNo("RK"),    //获取入库单据号
                ImportId = box.ImportId,    //入库单据ID
                WareHouseId = pallet.WarehouseId,    //仓库ID
                StockCodeId = pallet.Id,    //库存编码ID
                StockCode = pallet.StockCode,    //库存编码
                ImportQuantity = box.Qty ?? 0,    //入库数量
                ImportWeight = weightDelta,    //入库重量
                ImportExecuteFlag = "01",    //入库执行状态
                LotNo = box.LotNo,    //批次号
                ImportProductionDate = box.ProductionDate,    //生产日期
                CreateTime = now,    //创建时间
                CreateUserName = user,    //创建用户
                CreateUserId = 0,    //创建用户ID
                UpdateTime = now,    //更新时间
                UpdateUserName = user,    //更新用户
                UpdateUserId = 0,    //更新用户ID
                IsDelete = false,    //是否删除
                InspectFlag = 2,    //检验标志
                IsTemporaryStorage = "1",    //是否临时存储
                SubVehicleCode = pallet.StockCode    //子车编码
            };
            await _importOrderRep.InsertAsync(newOrder);    //插入入库流水
            return newOrder;
        }
        existingOrder.ImportQuantity = (existingOrder.ImportQuantity ?? 0) + (box.Qty ?? 0);    //入库数量
        existingOrder.ImportWeight = (existingOrder.ImportWeight ?? 0) + weightDelta;    //入库重量
        existingOrder.UpdateTime = now;
        existingOrder.UpdateUserName = user;    //更新用户
        await _importOrderRep.AsUpdateable(existingOrder)
            .UpdateColumns(x => new { x.ImportQuantity, x.ImportWeight, x.UpdateTime, x.UpdateUserName })    //更新入库流水
            .ExecuteCommandAsync();
        return existingOrder;
    }

    /// <summary>
    /// 更新箱托与托盘、入库流水的绑定信息。
    /// </summary>
    /// <param name="box">箱托信息。</param>
    /// <param name="pallet">托盘信息。</param>
    /// <param name="importOrder">入库流水。</param>
    /// <param name="input">绑定请求。</param>
    /// <param name="now">当前时间。</param>
    /// <param name="user">操作人。</param>
    private async Task UpdateBoxBindingAsync(WmsStockSlotBoxInfo box, WmsSysStockCode pallet, WmsImportOrder importOrder, PdaBindBoxInput input, DateTime now, string user)
    {
        var weight = input.Weight ?? box.Weight ?? 0.666m;    //重量
        box.StockCodeId = pallet.Id;
        box.StockCode = pallet.StockCode;    //库存编码
        box.ImportOrderId = importOrder.Id;    //入库流水ID
        box.Weight = weight;    //重量
        box.ReasonsForExcl = input.ReasonsForExcl;    //原因
        box.plasmaRejectTypeId = input.RejectTypeId;    //plasma拒绝类型ID  
        box.UpdateTime = now;    //更新时间
        box.UpdateUserName = user;    //更新用户
        await _stockSlotBoxInfoRep.AsUpdateable(box)    //更新箱托信息
            .UpdateColumns(x => new { x.StockCodeId, x.StockCode, x.ImportOrderId, x.Weight, x.ReasonsForExcl, x.plasmaRejectTypeId, x.UpdateTime, x.UpdateUserName })    //更新箱托信息
            .ExecuteCommandAsync();    //更新箱托信息
        pallet.Status = 1;    //状态
        pallet.UpdateTime = now;    //更新时间
        pallet.UpdateUserName = user;    //更新用户
        await _sysStockCodeRep.AsUpdateable(pallet).UpdateColumns(x => new { x.Status, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();    //更新托盘信息
    }    //更新箱托信息与托盘信息

    /// <summary>
    /// 同步更新入库明细与主单的执行状态与已组数量。
    /// </summary>
    /// <param name="box">箱托信息。</param>
    /// <param name="now">当前时间。</param>
    /// <param name="user">操作人。</param>
    private async Task UpdateImportQuantitiesAsync(WmsStockSlotBoxInfo box, DateTime now, string user)
    {
        if (box.ImportDetailId.HasValue)    //判断入库单据明细ID是否为空
        {
            var detail = await _importNotifyDetailRep.GetFirstAsync(x => x.Id == box.ImportDetailId.Value && x.IsDelete == false);    //查询入库单据明细信息
            if (detail != null)    //判断入库单据明细信息是否为空
            {
                detail.ImportFactQuantity = (detail.ImportFactQuantity ?? 0) + (box.Qty ?? 0);    //入库数量
                if (string.IsNullOrWhiteSpace(detail.ImportExecuteFlag) || detail.ImportExecuteFlag == "01")    //判断入库执行状态是否为空或为01
                {
                    detail.ImportExecuteFlag = "02";    //入库执行状态
                }
                detail.UpdateTime = now;    //更新时间
                detail.UpdateUserName = user;    //更新用户
                await _importNotifyDetailRep.AsUpdateable(detail).UpdateColumns(x => new { x.ImportFactQuantity, x.ImportExecuteFlag, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();    //更新入库单据明细信息
            }
        }
        if (box.ImportId.HasValue)    //判断入库单据ID是否为空
        {
            var notify = await _importNotifyRep.GetFirstAsync(x => x.Id == box.ImportId.Value && x.IsDelete == false);    //查询入库单据信息
            if (notify != null && string.Equals(notify.ImportExecuteFlag, "01", StringComparison.OrdinalIgnoreCase))    //判断入库执行状态是否为01
            {
                notify.ImportExecuteFlag = "02";    //入库执行状态
                notify.UpdateTime = now;    //更新时间
                notify.UpdateUserName = user;    //更新用户     
                await _importNotifyRep.AsUpdateable(notify).UpdateColumns(x => new { x.ImportExecuteFlag, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();    //更新入库单据信息
            }
        }
    }

    /// <summary>
    /// 校验箱码物料是否与单据匹配，避免跨物料绑定。
    /// </summary>
    /// <param name="box">箱托信息。</param>
    /// <returns>是否匹配。</returns>
    private async Task<bool> ValidateMaterialMatchAsync(WmsStockSlotBoxInfo box)
    {
        if (box.MaterialId == null)    //判断物料ID是否为空
            return false;
        if (!string.IsNullOrWhiteSpace(box.LotNo))    //判断批次号是否为空
            return true;
        if (!box.ImportId.HasValue)    //判断入库单据ID是否为空
            return false;
        return await _importNotifyDetailRep.AsQueryable()
            .Where(x => x.ImportId == box.ImportId.Value &&
                x.MaterialId == box.MaterialId.Value &&
                x.IsDelete == false)
            .AnyAsync();    //查询入库单据明细信息
    }

    /// <summary>
    /// 按物料条码维度查询待组托箱码信息。
    /// </summary>
    /// <param name="input">查询入参。</param>
    /// <returns>聚合后的结果。</returns>
    private async Task<List<PdaStockSlotItem>> QueryByMaterialBarcodeAsync(PdaStockSlotQueryInput input)
    {
        var barcode = ParseMaterialBarcode(input.MaterialBarcode);    //解析物料条码
        var importId = await ResolveImportIdAsync(input.ImportBillCode);    //获取入库单据ID
        var query = _sqlViewService.QueryStockSlotBoxInfoView()
            .MergeTable()
            .Where(x => x.IsDelete == false && x.Status == 0);
        if (importId.HasValue)    //判断入库单据ID是否为空
        {
            query = query.Where(x => x.ImportId == importId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(input.ImportBillCode))    //判断入库单据号是否为空
        {
            var billCode = input.ImportBillCode.Trim();    //入库单据号
            query = query.Where(x => SqlFunc.ToString(x.ImportId) == billCode);
        }
        if (barcode.MaterialId.HasValue)    //判断物料ID是否为空
        {
            query = query.Where(x => x.MaterialId == barcode.MaterialId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(barcode.MaterialIdRaw))    //判断物料ID是否为空
        {
            var rawMaterialId = barcode.MaterialIdRaw;    //物料ID
            query = query.Where(x => SqlFunc.ToString(x.MaterialId) == rawMaterialId);
        }
        if (!string.IsNullOrWhiteSpace(barcode.LotNo))    //判断批次号是否为空
        {
            var lotNo = barcode.LotNo;    //批次号
            query = query.Where(x => x.LotNo == lotNo);
        }
        if (barcode.DetailId.HasValue)    //判断入库单据明细ID是否为空
        {
            query = query.Where(x => x.ImportDetailId == barcode.DetailId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(barcode.DetailIdRaw))    //判断入库单据明细ID是否为空
        {
            var rawDetailId = barcode.DetailIdRaw;    //入库单据明细ID
            query = query.Where(x => SqlFunc.ToString(x.ImportDetailId) == rawDetailId);
        }
        var typedRows = await query
            .OrderBy(x => x.StockCode, OrderByType.Asc)
            .Select(x => new PdaStockSlotRaw
            {
                StockCode = x.StockCode ?? string.Empty,
                BoxCode = SqlFunc.Coalesce(x.BoxCode, string.Empty),
                LotNo = SqlFunc.Coalesce(x.LotNo, string.Empty),
                MaterialCode = SqlFunc.Coalesce(x.MaterialCode, string.Empty),
                MaterialName = SqlFunc.Coalesce(x.MaterialName, string.Empty),
                Qty = SqlFunc.Coalesce(x.Qty, 0m) ?? 0m
            })
            .ToListAsync();
        return AggregateQuantity(typedRows);
    }

    /// <summary>
    /// 按托盘维度查询待组托箱码信息。
    /// </summary>
    /// <param name="input">查询入参。</param>
    /// <returns>聚合后的结果。</returns>
    private async Task<List<PdaStockSlotItem>> QueryByStockAsync(PdaStockSlotQueryInput input)
    {
        // 对照 JC35 PdaDal.GetStockSoltBoxInfo 逻辑
        var query = _sqlViewService.QueryStockSlotBoxInfoView()
            .MergeTable()
            .Where(x => x.IsDelete == false)
            .Where(x => x.ImportOrderId != null && x.ImportOrderId != 0);    // 对应 JC35: ImportOrderId != ""
        if (!string.IsNullOrWhiteSpace(input.StockCode))    //判断库存编码是否为空
        {
            var stockCode = input.StockCode.Trim();    //库存编码
            query = query.Where(x => x.StockCode == stockCode);
        }
        else
        {
            // JC35逻辑：StockCode为空时，查询ReasonsForExcl不为空的记录
            // 但如果同时传入了ImportBillCode，则优先按ImportBillCode过滤（兼容前端场景）
            if (!string.IsNullOrWhiteSpace(input.ImportBillCode))
            {
                var importBillCode = input.ImportBillCode.Trim();    //入库单号
                // 尝试解析为ImportId，如果失败则作为字符串匹配
                var resolvedImportId = await ResolveImportIdAsync(importBillCode);
                if (resolvedImportId.HasValue)
                {
                    query = query.Where(x => x.ImportId == resolvedImportId.Value);
                }
                else
                {
                    query = query.Where(x => SqlFunc.ToString(x.ImportId) == importBillCode || x.ImportBillCode == importBillCode);
                }
            }
            else
            {
                query = query.Where(x => !SqlFunc.IsNullOrEmpty(x.ReasonsForExcl));
            }
        }
        // JC35逻辑：type != "1" 时才过滤ImportExecuteFlag；type == "1" 时不过滤
        if (!string.Equals(input.Type, "1", StringComparison.OrdinalIgnoreCase))    //判断类型是否为1
        {
            query = query.Where(x => x.ImportExecuteFlag == "01" || x.ImportExecuteFlag == "02");
        }
        // JC35逻辑：InspectBillCode直接作为ImportId字符串匹配，不解析
        if (!string.IsNullOrWhiteSpace(input.InspectBillCode))    //判断检验单据号是否为空
        {
            var rawInspect = input.InspectBillCode.Trim();    //检验单据号
            query = query.Where(x => SqlFunc.ToString(x.ImportId) == rawInspect);    //直接字符串匹配，对应JC35: a.ImportId == InspectBillCode
        }
        var typedRows = await query
            .OrderByDescending(x => x.BoxCode)    // JC35逻辑：OrderByDescending(a => a.BoxCode)
            .Select(x => new PdaStockSlotRaw
            {
                StockCode = x.StockCode ?? string.Empty,
                BoxCode = SqlFunc.Coalesce(x.BoxCode, string.Empty),
                LotNo = SqlFunc.Coalesce(x.LotNo, string.Empty),
                MaterialCode = SqlFunc.Coalesce(x.MaterialCode, string.Empty),
                MaterialName = SqlFunc.Coalesce(x.MaterialName, string.Empty),
                Qty = SqlFunc.Coalesce(x.Qty, 0m) ?? 0m
            }).ToListAsync();
        return AggregateQuantity(typedRows);
    }

    /// <summary>
    /// 对查询结果按托盘/箱码聚合数量。
    /// </summary>
    /// <param name="source">原始查询结果集合</param>
    /// <returns>聚合后的组托信息列表</returns>
    /// <remarks>
    /// 聚合逻辑：
    /// <list type="bullet">
    /// <item><description>按托盘编码、箱码、物料编码、物料名称、批号分组</description></item>
    /// <item><description>相同分组内累加数量，但保留第一条记录的原始数量</description></item>
    /// <item><description>按箱码降序排列，最新的记录在前</description></item>
    /// </list>
    /// 聚合场景：
    /// <list type="number">
    /// <item><description>同一托盘上同一物料的不同批次分别显示</description></item>
    /// <item><description>同一托盘上同一物料同一批次的多次记录合并显示</description></item>
    /// <item><description>保留原始记录的Qty字段值</description></item>
    /// <item><description>Quantity字段为累加后的总量</description></item>
    /// </list>
    /// </remarks>
    private static List<PdaStockSlotItem> AggregateQuantity(IEnumerable<PdaStockSlotRaw> source)
    {
        return source    //查询箱托信息与物料信息与入库单据信息
            .GroupBy(x => new { x.StockCode, x.BoxCode, x.MaterialCode, x.MaterialName, x.LotNo })    //分组
            .Select(group => new PdaStockSlotItem
            {
                StockCode = group.Key.StockCode,    //库存编码
                BoxCode = group.Key.BoxCode,    //箱码
                MaterialCode = group.Key.MaterialCode,    //物料编码
                MaterialName = group.Key.MaterialName,    //物料名称
                LotNo = group.Key.LotNo,    //批次号
                Qty = group.First().Qty,    //数量
                Quantity = group.Sum(item => item.Qty)    //数量
            }).OrderByDescending(item => item.BoxCode).ToList();    //排序
    }
    /// <summary>
    /// 根据入库单号（或主键）解析真实的入库单主键。
    /// </summary>
    private async Task<long?> ResolveImportIdAsync(string? importBillCode)
    {
        if (string.IsNullOrWhiteSpace(importBillCode))    //判断入库单据号是否为空      
            return null;    //入库单据号为空
        if (long.TryParse(importBillCode, out var id))    //判断入库单据号是否为长整型
            return id;    //入库单据号为长整型
        var notify = await _importNotifyRep.GetFirstAsync(x => x.ImportBillCode == importBillCode && x.IsDelete == false);    //查询入库单据信息
        return notify?.Id == 0 ? null : notify?.Id;    //入库单据ID
    }

    /// <summary>
    /// 解析物料条码（明细-物料-批次-流水）。
    /// </summary>
    /// <param name="barcode">原始条码字符串</param>
    /// <returns>解析后的条码结构，包含各部分ID和原始值</returns>
    /// <exception cref="Exception">当条码格式不正确时抛出异常</exception>
    /// <remarks>
    /// 条码格式：
    /// <list type="bullet">
    /// <item><description>基本格式：明细ID-物料ID-批次</description></item>
    /// <item><description>扩展格式：明细ID-物料ID-批次-流水ID</description></item>
    /// <item><description>使用"-"作为分隔符</description></item>
    /// </list>
    /// 解析规则：
    /// <list type="number">
    /// <item><description>至少包含3个段落（明细、物料、批次）</description></item>
    /// <item><description>明细ID和物料ID尝试解析为长整型</description></item>
    /// <item><description>保留原始字符串值用于查询</description></item>
    /// <item><description>第4段落为可选的入库流水ID</description></item>
    /// </list>
    /// 示例：
    /// <list type="bullet">
    /// <item><description>"123-456-LOT001" → 明细ID=123, 物料ID=456, 批次=LOT001</description></item>
    /// <item><description>"123-456-LOT001-789" → 明细ID=123, 物料ID=456, 批次=LOT001, 流水ID=789</description></item>
    /// </list>
    /// </remarks>
    private static ParsedBarcode ParseMaterialBarcode(string barcode)
    {
        var segments = (barcode ?? string.Empty).Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);    //分割条码
        if (segments.Length < 3)    //判断条码长度是否小于3
        {
            throw Oops.Bah("物料条码格式错误，正确格式示例：明细ID-物料ID-批次。");    //物料条码格式错误
        }
        long? detailId = long.TryParse(segments[0], out var detailVal) ? detailVal : null;    //明细ID
        long? materialId = long.TryParse(segments[1], out var materialVal) ? materialVal : null;    //物料ID
        string lotNo = segments[2];    //批次号
        var parsed = new ParsedBarcode    //解析结果
        {
            DetailId = detailId,    //明细ID
            DetailIdRaw = segments[0],    //明细ID
            MaterialId = materialId,    //物料ID
            MaterialIdRaw = segments[1],    //物料ID
            LotNo = lotNo    //批次号
        };
        if (segments.Length > 3 && long.TryParse(segments[3], out var orderId))    //判断条码长度是否大于3  
        {
            parsed.OrderId = orderId;    //入库流水ID
            parsed.OrderIdRaw = segments[3];    //入库流水ID
        }
        return parsed;
    }
    /// <summary>
    /// 规范化动作类型字符串。
    /// </summary>
    private static string NormalizeActionType(string? actionType, string defaultValue) =>    //规范化动作类型
        string.IsNullOrWhiteSpace(actionType)
            ? defaultValue
            : actionType.Trim().ToLowerInvariant();    //规范化动作类型 小写

    /// <summary>
    /// 收集主副载具托盘编码，去重后返回。
    /// </summary>
    private static HashSet<string> CollectTrayCodes(string primaryCode, string? secondaryCode)    //收集主副载具托盘编码
    {
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { primaryCode.Trim() };    //主载具托盘编码
        if (!string.IsNullOrWhiteSpace(secondaryCode))    //判断副载具托盘编码是否为空
        {
            codes.Add(secondaryCode.Trim());    //副载具托盘编码
        }
        return codes;    //返回主副载具托盘编码
    }
    /// <summary>
    /// 获取指定托盘最新的待执行入库流水。    
    /// </summary>
    /// <param name="stockCode">库存编码。</param>
    /// <param name="roleLabel">角色标签。</param>
    /// <returns>入库流水。</returns>
    private async Task<WmsImportOrder> ResolveLatestPendingImportOrderAsync(string stockCode, string roleLabel)    //获取指定托盘最新的待执行入库流水
    {
        var order = await _importOrderRep.AsQueryable()    //查询入库流水
            .Where(x => x.IsDelete == false && x.ImportExecuteFlag == "01" && x.StockCode == stockCode)    //查询入库流水
            .OrderBy(x => x.CreateTime, OrderByType.Desc)    //排序
            .FirstAsync() ?? throw Oops.Bah($"{roleLabel}({stockCode})未找到待执行入库流水！");    //获取第一个
        return order;    //返回入库流水
    }

    /// <summary>
    /// 获取托盘最新的出库流水。
    /// </summary>
    /// <param name="trayCode">托盘编码。</param>
    /// <returns>出库流水。</returns>
    private async Task<WmsExportOrder> ResolveLatestExportOrderAsync(string trayCode)    //获取托盘最新的出库流水               
    {
        var order = await _exportOrderRep.AsQueryable()    //查询出库流水
            .Where(x => x.IsDelete == false && x.ExportStockCode == trayCode)    //查询出库流水
            .OrderBy(x => x.CreateTime, OrderByType.Desc)    //排序
            .FirstAsync() ?? throw Oops.Bah($"未找到托盘({trayCode})对应的出库流水！");    //获取第一个

        return order;    //返回出库流水
    }

    /// <summary>
    /// 获取托盘主数据，若不存在则抛出异常。
    /// </summary>
    /// <param name="trayCode">托盘编码。</param>
    /// <returns>托盘主数据。</returns>
    private async Task<WmsSysStockCode> ResolveStockCodeAsync(string trayCode)    //获取托盘主数据
    {
        var stockCode = await _sysStockCodeRep.GetFirstAsync(x => x.StockCode == trayCode && x.IsDelete == false) ?? throw Oops.Bah($"托盘({trayCode})信息不存在！");
        return stockCode;    //返回托盘主数据
    }

    /// <summary>
    /// 依据出库流水生成暂存入库流水。
    /// </summary>
    /// <param name="exportOrder">出库订单。</param>
    /// <param name="stockCode">库存编码。</param>
    /// <param name="user">用户。</param>
    /// <param name="now">当前时间。</param>
    /// <param name="primaryVehicle">主载具托盘编码。</param>
    /// <returns>入库流水。</returns>
    private async Task<WmsImportOrder> CreateTemporaryImportOrderAsync(WmsExportOrder exportOrder, WmsSysStockCode stockCode, string user, DateTime now, string primaryVehicle)    //依据出库流水生成暂存入库流水       
    {
        var weight = ParseDecimalOrZero(exportOrder.ExportWeight);    //获取出库重量
        var importOrder = new WmsImportOrder    //创建入库流水
        {
            Id = SnowFlakeSingle.Instance.NextId(),    //生成ID
            ImportOrderNo = _commonMethod.GetImExNo("RK"),    //获取入库单据号
            WareHouseId = stockCode.WarehouseId,    //仓库ID    
            StockCodeId = stockCode.Id,    //库存编码ID
            StockCode = stockCode.StockCode,    //库存编码
            ImportQuantity = exportOrder.ExportQuantity ?? 0,    //入库数量
            ImportWeight = weight,    //入库重量
            Weight = weight,    //重量
            ImportExecuteFlag = "01",    //入库执行标志
            LotNo = exportOrder.ExportLotNo,    //批次号
            ImportProductionDate = exportOrder.ExportProductionDate,    //生产日期
            CreateTime = now,    //创建时间
            CreateUserName = user,    //创建用户
            CreateUserId = 0,    //创建用户ID
            UpdateTime = now,    //更新时间
            UpdateUserName = user,    //更新用户
            UpdateUserId = 0,    //更新用户ID
            IsDelete = false,    //是否删除
            InspectFlag = 2,    //检验标志
            IsTemporaryStorage = "1",    //是否暂存
            SubVehicleCode = primaryVehicle    //副载具托盘编码
        };
        await _importOrderRep.InsertAsync(importOrder);    //插入入库流水
        return importOrder;    //返回入库流水
    }

    /// <summary>
    /// 将出库箱码重新关联到新生成的入库流水。
    /// </summary>
    /// <param name="exportOrderNo">出库单据号。</param>
    /// <param name="importOrderId">入库流水ID。</param>
    /// <param name="now">当前时间。</param>
    /// <param name="user">用户。</param>
    private async Task RelinkBoxesToImportOrderAsync(string? exportOrderNo, long importOrderId, DateTime now, string user)
    {
        if (string.IsNullOrWhiteSpace(exportOrderNo))    //判断出库单据号是否为空
            return;    //出库单据号为空
        var codeSet = (await _exportBoxInfoRep.AsQueryable()
                .Where(x => x.IsDelete == false && x.ExportOrderNo == exportOrderNo)
                .Select(x => x.BoxCode)
                .ToListAsync())    //查询出库箱码信息
            .Where(code => !string.IsNullOrWhiteSpace(code))    //判断箱码是否为空
            .Select(code => code.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);    //转换为HashSet
        if (codeSet.Count == 0)    //判断箱码集合是否为空
            return;    //箱码集合为空
        var slotBoxes = await _stockSlotBoxInfoRep.AsQueryable()    //查询库存组托信息
            .Where(x => x.IsDelete == false && codeSet.Contains(x.BoxCode))
            .OrderBy(x => x.CreateTime, OrderByType.Desc)    //排序
            .ToListAsync();    //获取列表
        foreach (var box in slotBoxes.GroupBy(x => x.BoxCode).Select(g => g.First()))    //遍历箱码集合
        {
            box.ImportOrderId = importOrderId;    //入库流水ID
            box.Status = 0;    //状态
            box.UpdateTime = now;    //更新时间
            box.UpdateUserName = user;    //更新用户
            await _stockSlotBoxInfoRep.AsUpdateable(box)
                .UpdateColumns(x => new { x.ImportOrderId, x.Status, x.UpdateTime, x.UpdateUserName })
                .ExecuteCommandAsync();    //更新库存组托信息 
        }
    }
    #endregion
}
