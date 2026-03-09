// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护使用本项目应遵守相关法律法规和许可证的要求
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证(版本 2.0)进行分发和使用许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动!任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任!

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
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Admin.NET.Application.Service.WmsPda.Process;

/// <summary>
/// PDA入库组托业务处理类
/// <para>承载 JC35 <c>PdaInterfaceController</c> 入库组托相关接口迁移，提供入库组托相关业务功能</para>
/// </summary>
/// <remarks>
/// 主要功能包括：
/// <list type="bullet">
/// <item><description>入库组托查询：按物料条码或托盘查询组托信息</description></item>
/// <item><description>数量计算：计算无箱码绑定推荐数量</description></item>
/// <item><description>无箱码绑定：保存无箱码载具绑定信息</description></item>
/// </list>
/// </remarks>
public class PdaImportGroup : PdaImportBase, ITransient
{
    #region 字段定义
    private readonly ILogger _logger;                            //日志
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖
    /// </summary>
    public PdaImportGroup(
        ILoggerFactory loggerFactory,
        ISqlSugarClient sqlSugarClient,
        PdaBaseRepos repos,
        WmsSqlViewService sqlViewService) : base(sqlSugarClient, repos, sqlViewService)
    {
        _logger = loggerFactory.CreateLogger(CommonConst.PdaImportGroup);
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 查询入库组托信息
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
        // 验证输入参数：非托盘查询模式时需要物料条码和入库单号
        if (!string.Equals(input.Type, "1", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(input.MaterialBarcode))
                throw Oops.Bah("物料条码不能为空！");
            if (string.IsNullOrWhiteSpace(input.ImportBillCode))
                throw Oops.Bah("入库单号不能为空！");
        }
        // 返回结果
        var output = new PdaStockSlotQueryOutput();
        try
        {
            // 根据查询类型选择查询策略：type="1"按托盘查询，其他值按物料条码查询
            var items = string.Equals(input.Type, "1", StringComparison.OrdinalIgnoreCase)
                ? await QueryByStockAsync(input)
                : await QueryByMaterialBarcodeAsync(input);
            output.Success = true;
            output.Message = "查询成功";
            output.Items = items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询入库组托信息失败：{@Input}", input);
            output.Success = false;
            output.Message = ex.Message;
            output.Items = new List<PdaStockSlotItem>();
        }
        return output;
    }
    /// <summary>
    /// 计算无箱码绑定推荐数量
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
        // 验证输入参数完整性
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.MaterialBarcode))
            throw Oops.Bah("物料条码不能为空！");
        if (string.IsNullOrWhiteSpace(input.ImportBillCode))
            throw Oops.Bah("入库单信息不能为空！");
        if (string.IsNullOrWhiteSpace(input.StockCode))
            throw Oops.Bah("托盘编码不能为空！");
        // 返回结果
        var output = new PdaScanQtyOutput();
        // 托盘编码
        var stockCode = input.StockCode.Trim();
        // 入库单号
        var importBillCode = input.ImportBillCode.Trim();
        try
        {
            // 解析物料条码：明细ID-物料ID-批次-(可选)入库流水ID
            var barcode = ParseMaterialBarcode(input.MaterialBarcode);
            if (!barcode.DetailId.HasValue)
                throw Oops.Bah("物料条码格式错误，正确格式示例：明细ID-物料ID-批次");
            // 解析入库单号为主键ID（支持单号或ID两种格式）
            var resolvedImportId = await ResolveImportIdAsync(importBillCode);
            // 构建入库明细查询条件：匹配明细ID、入库单号、物料ID、批次号
            var importDetailQuery = _repos.ImportNotifyDetail.AsQueryable()
                .Where(detail => detail.Id == barcode.DetailId.Value && detail.IsDelete == false)
                .WhereIF(resolvedImportId.HasValue, detail => detail.ImportId == resolvedImportId.Value)
                .WhereIF(!resolvedImportId.HasValue, detail => SqlFunc.ToString(detail.ImportId) == importBillCode)
                .WhereIF(barcode.MaterialId.HasValue, detail => detail.MaterialId == barcode.MaterialId.Value)
                .WhereIF(!barcode.MaterialId.HasValue && !string.IsNullOrWhiteSpace(barcode.MaterialIdRaw), detail => SqlFunc.ToString(detail.MaterialId) == barcode.MaterialIdRaw)
                .WhereIF(!string.IsNullOrWhiteSpace(barcode.LotNo), detail => detail.LotNo == barcode.LotNo);
            // 获取并验证入库明细信息
            var importDetail = await importDetailQuery.FirstAsync();
            if (importDetail == null)
                throw Oops.Bah("未找到匹配的入库单明细，请确认条码信息是否正确");
            // 解析入库流水ID（用于后续查询已绑定托盘）
            long? importOrderId = long.TryParse(importBillCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedOrderId) ? parsedOrderId : null;
            // 校验目标托盘是否已绑定当前明细（Status=0表示待绑定状态）
            var existingQuery = _sqlViewService.QueryStockSlotBoxInfoView()
                .MergeTable()
                .Where(x => x.IsDelete == false && x.Status == 0 && x.StockCode == stockCode && x.ImportDetailId == importDetail.Id)
                .WhereIF(importOrderId.HasValue, x => x.ImportOrderId == importOrderId.Value)
                .WhereIF(!importOrderId.HasValue, x => SqlFunc.ToString(x.ImportOrderId) == importBillCode);
            var isBound = await existingQuery.AnyAsync();
            if (isBound)
                throw Oops.Bah("当前托盘已绑定！");
            // 获取物料基础信息（用于计算推荐数量）
            var material = (importDetail.MaterialId.HasValue ? await _repos.Material.GetByIdAsync(importDetail.MaterialId.Value) : null) ?? throw Oops.Bah("获取物料基础信息失败！");
            // 计算剩余可入库数量 = 计划入库数量 - 已组托数量
            var remainQty = Math.Max(0, (importDetail.ImportQuantity ?? 0) - (importDetail.ImportFactQuantity ?? 0));
            // 【第一步：获取明细的箱数量】尝试通过反射获取入库明细的BoxQuantity属性
            decimal? detailBoxQty = null;
            var boxQuantityProperty = importDetail.GetType().GetProperty("BoxQuantity");
            if (boxQuantityProperty != null)
            {
                var rawBoxQty = boxQuantityProperty.GetValue(importDetail);
                if (rawBoxQty != null && decimal.TryParse(Convert.ToString(rawBoxQty, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedDetailBoxQty) && parsedDetailBoxQty > 0)
                {
                    detailBoxQty = parsedDetailBoxQty;
                }
            }
            // 【第二步：箱数量回退方案】如果明细没有箱数量，使用物料基础数据的箱数量
            if ((!detailBoxQty.HasValue || detailBoxQty.Value <= 0) &&
                !string.IsNullOrWhiteSpace(material.BoxQuantity) &&
                decimal.TryParse(material.BoxQuantity, NumberStyles.Any, CultureInfo.InvariantCulture, out var materialBoxQty) &&
                materialBoxQty > 0)
            {
                detailBoxQty = materialBoxQty;
            }
            // 【第三步：获取物料基础件数】从物料基础数据中获取每箱件数
            decimal? everyNumber = null;
            if (!string.IsNullOrWhiteSpace(material.EveryNumber) &&
                decimal.TryParse(material.EveryNumber, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedEveryNumber) &&
                parsedEveryNumber > 0)
            {
                everyNumber = parsedEveryNumber;
            }
            // 【第四步：计算推荐数量】默认推荐剩余数量
            decimal recommendedQty = remainQty;
            // 情况1：同时有箱数量和件数时，计算 箱数 * 件数
            if (remainQty > 0 && detailBoxQty.HasValue && everyNumber.HasValue)
            {
                var plannedQty = detailBoxQty.Value * everyNumber.Value;
                if (plannedQty > 0)
                {
                    // 推荐数量 = min(剩余数量, 计划入库数量)
                    recommendedQty = Math.Min(remainQty, plannedQty);
                }
            }
            // 情况2：只有件数时，推荐数量 = min(剩余数量, 件数)
            else if (remainQty > 0 && everyNumber.HasValue)
            {
                recommendedQty = Math.Min(remainQty, everyNumber.Value);
            }
            output.Success = true;
            output.Message = "计算完成";
            output.Quantity = recommendedQty;
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
    /// 保存无箱码载具绑定信息
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
    /// <item><description>支持人工码垛场景(Sources不为空)</description></item>
    /// <item><description>同一托盘可续码码垛(相同物料和批次)</description></item>
    /// <item><description>自动更新托盘使用状态</description></item>
    /// <item><description>事务保证数据一致性</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> SaveNoBoxBindingAsync(PdaSaveBoxInfoInput input)
    {
        // 验证输入参数完整性
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.StockCode))
            throw Oops.Bah("托盘编码不能为空！");
        if (string.IsNullOrWhiteSpace(input.MaterialId))
            throw Oops.Bah("物料信息不能为空！");
        if (string.IsNullOrWhiteSpace(input.LotNo))
            throw Oops.Bah("批次信息不能为空！");
        if (string.IsNullOrWhiteSpace(input.ImportId))
            throw Oops.Bah("入库单信息不能为空！");
        if (string.IsNullOrWhiteSpace(input.ImportDetailId))
            throw Oops.Bah("入库单明细不能为空！");
        if (input.Qty <= 0)
            throw Oops.Bah("绑定数量不能为空！");
        var stockCode = input.StockCode.Trim();
        var lotNo = input.LotNo.Trim();
        var user = NormalizeUser(input.User);
        var now = DateTime.Now;
        // 入库单ID
        var importId = ParseRequiredLong(input.ImportId, "入库单信息");
        // 入库明细ID
        var importDetailId = ParseRequiredLong(input.ImportDetailId, "入库单明细");
        // 物料ID
        var materialId = ParseRequiredLong(input.MaterialId, "物料信息");
        // 绑定数量
        var quantity = input.Qty;
        var result = new PdaActionResult();
        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                WmsSysStockCode pallet;
                // 【第一步：获取托盘主数据】根据业务场景选择托盘查询策略
                if (!string.IsNullOrWhiteSpace(input.Sources))
                {
                    // 人工码垛场景：先查询是否存在库存托盘记录（同物料同批次可继续码垛）
                    var stockTray = await _repos.StockTray.GetFirstAsync(x =>
                        x.StockCode == stockCode &&
                        SqlFunc.ToString(x.MaterialId) == input.MaterialId.Trim() &&
                        x.LotNo == lotNo);
                    if (stockTray == null)
                    {
                        // 不存在库存托盘，查询空闲的托盘主数据（Status=0表示可用）
                        pallet = await _repos.SysStockCode.GetFirstAsync(x =>
                            x.StockCode == stockCode &&
                            x.IsDelete == false &&
                            x.Status == 0);
                    }
                    else
                    {
                        // 【新增】存在库存托盘，检查是否已分配库位（已上架不能续码）
                        if (!string.IsNullOrWhiteSpace(stockTray.StockSlotCode))
                        {
                            throw Oops.Bah("托盘码重复不能继续码垛！");
                        }

                        // 存在库存托盘且未分配库位，直接查询托盘主数据（允许续码，不检查Status）
                        pallet = await _repos.SysStockCode.GetFirstAsync(x => x.StockCode == stockCode && x.IsDelete == false);
                    }
                }
                else
                {
                    // 普通绑定场景：直接查询托盘主数据（不检查Status）
                    pallet = await _repos.SysStockCode.GetFirstAsync(x => x.StockCode == stockCode && x.IsDelete == false);
                }
                if (pallet == null)
                    throw Oops.Bah($"托盘({stockCode})不存在或已被删除！");
                // 【第二步：获取入库明细信息】使用视图查询入库单据明细
                var importDetail = await _sqlViewService.QueryImportNotifyDetailView()
                    .MergeTable()
                    .Where(x => x.Id == importDetailId && x.IsDel == false && x.ImportId == importId && x.MaterialId == materialId && x.LotNo == lotNo)
                    .FirstAsync() ?? throw Oops.Bah("未获取到入库单据！");
                // 【第三步：获取入库单据主单】
                var notify = await _repos.ImportNotify.GetFirstAsync(x => x.Id == importId && x.IsDelete == false) ?? throw Oops.Bah("未获取到入库单据！");
                // 解析检验状态（MaterialStatus字段）
                var inspectionStatus = !string.IsNullOrWhiteSpace(importDetail.MaterialStatus) &&
                                       int.TryParse(importDetail.MaterialStatus, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedInspectionStatus) ? parsedInspectionStatus : 0;
                // 【第四步：查询托盘现有的入库流水】（ImportExecuteFlag="01"表示待执行状态）
                var orderItems = await _sqlViewService.QueryImportOrderView()
                    .MergeTable()
                    .Where(x => x.StockCode == stockCode && x.ImportExecuteFlag == "01" && x.IsDel == false)
                    .OrderBy(x => x.CreateTime, OrderByType.Desc)
                    .ToListAsync();
                // 获取入库明细实体（用于后续更新）
                var detailForUpdate = await _repos.ImportNotifyDetail.GetFirstAsync(x => x.Id == importDetail.Id && x.IsDelete == false) ?? throw Oops.Bah("未获取到入库单据明细！");
                // 【第五步：超入验证】绑定前验证是否超出计划数量
                // 当前已组托数量
                var currentFactQuantity = detailForUpdate.ImportFactQuantity ?? 0;
                // 绑定后的总数量
                var newFactQuantity = currentFactQuantity + quantity;
                // 计划入库数量
                var planQuantity = detailForUpdate.ImportQuantity ?? 0;
                // 获取仓库配置，检查是否允许超入
                var warehouse = await _repos.Warehouse.GetFirstAsync(x => x.Id == notify.WarehouseId && x.IsDelete == false);
                // 如果绑定后超出计划数量，检查仓库是否允许超入
                if (newFactQuantity > planQuantity)
                {
                    if (!(warehouse?.IsOverbooking ?? false))
                    {
                        var material = await _repos.Material.GetFirstAsync(x => x.Id == materialId && x.IsDelete == false);
                        throw Oops.Bah($"物料【{material?.MaterialName ?? "未知"}】超入：计划入库 {planQuantity}，已组托 {currentFactQuantity}，本次绑定 {quantity}，将超出计划 {newFactQuantity - planQuantity}当前仓库不允许超入！");
                    }
                }
                // 入库流水
                WmsImportOrder importOrder;
                // 【第六步：处理入库流水】根据托盘是否已有流水决定创建或更新
                if (orderItems.Count == 0)
                {
                    // 情况1：托盘没有入库流水，创建新的入库流水记录
                    importOrder = new WmsImportOrder
                    {
                        Id = SnowFlakeSingle.Instance.NextId(),
                        ImportOrderNo = _commonMethod.GetImExNo("RK"),  // 生成入库流水号
                        ImportId = importDetail.ImportId,               // 入库单据ID
                        WareHouseId = notify.WarehouseId,               // 仓库ID
                        StockCodeId = pallet.Id,                        // 托盘ID
                        StockCode = pallet.StockCode,                   // 托盘编码
                        ImportQuantity = quantity,                      // 入库数量
                        ImportExecuteFlag = "01",                       // 执行标志：01=待执行
                        LotNo = importDetail.LotNo,                     // 批次
                        ImportProductionDate = importDetail.ImportProductionDate,  // 生产日期
                        CreateUserName = user,                          // 创建用户
                        UpdateUserName = user,                          // 更新用户
                        CreateUserId = 0,
                        UpdateUserId = 0,
                        CreateTime = now,                               // 创建时间
                        UpdateTime = now,                               // 更新时间
                        IsDelete = false,
                        ImportDetailId = importDetail.Id,               // 入库明细ID
                        InspectionStatus = inspectionStatus,            // 检验状态
                        YsOrTJ = "0"
                    };
                    await _repos.ImportOrder.InsertAsync(importOrder);
                    // 更新托盘使用状态（Status=1表示已占用）
                    pallet.Status = 1;
                    pallet.UpdateTime = now;
                    pallet.UpdateUserName = user;
                    await _repos.SysStockCode.AsUpdateable(pallet).UpdateColumns(x => new { x.Status, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();
                    // 创建箱托绑定记录（无箱码场景，BoxCode为null）
                    var boxEntity = new WmsStockSlotBoxInfo
                    {
                        Id = SnowFlakeSingle.Instance.NextId(),
                        ValidateDay = importDetail.ImportLostDate,       // 有效期
                        ProductionDate = importDetail.ImportProductionDate,  // 生产日期
                        StockCodeId = pallet.Id,                         // 托盘ID
                        StockCode = pallet.StockCode,                    // 托盘编码
                        LotNo = importDetail.LotNo,                      // 批次
                        BoxCode = null,                                  // 无箱码
                        MaterialId = importDetail.MaterialId,            // 物料ID
                        SupplierId = notify.SupplierId,                  // 供应商ID
                        ManufacturerId = notify.ManufacturerId,          // 制造商ID
                        IsDelete = false,
                        Status = 0,                                      // 状态：0=待绑定
                        ImportOrderId = importOrder.Id,                  // 入库流水ID
                        ImportId = importDetail.ImportId,                // 入库单据ID
                        ImportDetailId = importDetail.Id,                // 入库明细ID
                        Qty = quantity,                                  // 绑定数量
                        BulkTank = input.BulkTank,                       // 油罐号
                        CreateTime = now,
                        CreateUserName = user,
                        CreateUserId = 0,
                        UpdateTime = now,
                        UpdateUserName = user,
                        UpdateUserId = 0,
                        InspectionStatus = inspectionStatus,             // 检验状态
                    };
                    await _repos.StockSlotBoxInfo.InsertAsync(boxEntity);
                    // 累加入库明细的已组托数量
                    detailForUpdate.ImportFactQuantity = (detailForUpdate.ImportFactQuantity ?? 0) + quantity;
                }
                else
                {
                    // 情况2：托盘已有入库流水，更新现有流水或创建新箱托记录
                    var orderView = orderItems[0];
                    importOrder = await _repos.ImportOrder.GetFirstAsync(x => x.Id == orderView.Id && x.IsDelete == false);
                    if (importOrder == null)
                    {
                        throw Oops.Bah("获取入库流水信息失败！");
                    }
                    // 查询托盘上是否已有相同明细的箱托记录
                    var boxEntity = await _repos.StockSlotBoxInfo.GetFirstAsync(x => x.StockCodeId == pallet.Id && x.ImportOrderId == importOrder.Id);
                    // 子情况2.1：存在相同明细的箱托记录，更新数量
                    if (boxEntity != null &&
                        boxEntity.MaterialId == importDetail.MaterialId &&
                        string.Equals(boxEntity.LotNo ?? string.Empty, importDetail.LotNo ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
                        boxEntity.ImportDetailId == importDetail.Id)
                    {
                        // 更新现有箱托记录的数量（覆盖而非累加）
                        boxEntity.Qty = quantity;
                        boxEntity.UpdateTime = now;
                        boxEntity.UpdateUserName = user;
                        boxEntity.UpdateUserId = 0;
                        await _repos.StockSlotBoxInfo.AsUpdateable(boxEntity).UpdateColumns(x => new { x.Qty, x.UpdateTime, x.UpdateUserName, x.UpdateUserId }).ExecuteCommandAsync();
                        // 直接设置明细的已组托数量为当前数量
                        detailForUpdate.ImportFactQuantity = quantity;
                    }
                    // 子情况2.2：不存在相同明细的箱托记录，创建新记录
                    else
                    {
                        var newBox = new WmsStockSlotBoxInfo
                        {
                            Id = SnowFlakeSingle.Instance.NextId(),
                            ValidateDay = importDetail.ImportLostDate,       // 有效期
                            ProductionDate = importDetail.ImportProductionDate,  // 生产日期
                            StockCodeId = pallet.Id,                         // 托盘ID
                            StockCode = pallet.StockCode,                    // 托盘编码
                            LotNo = importDetail.LotNo,                      // 批次
                            BoxCode = null,                                  // 无箱码
                            MaterialId = importDetail.MaterialId,            // 物料ID
                            SupplierId = notify.SupplierId,                  // 供应商ID
                            ManufacturerId = notify.ManufacturerId,          // 制造商ID
                            IsDelete = false,
                            Status = 0,                                      // 状态：0=待绑定
                            ImportOrderId = importOrder.Id,                  // 入库流水ID
                            ImportId = importDetail.ImportId,                // 入库单据ID
                            ImportDetailId = importDetail.Id,                // 入库明细ID
                            Qty = quantity,                                  // 绑定数量
                            BulkTank = input.BulkTank,                       // 油罐号
                            CreateTime = now,
                            CreateUserName = user,
                            CreateUserId = 0,
                            UpdateTime = now,
                            UpdateUserName = user,
                            UpdateUserId = 0,
                            InspectionStatus = inspectionStatus,             // 检验状态
                        };
                        await _repos.StockSlotBoxInfo.InsertAsync(newBox);
                        // 累加入库明细的已组托数量
                        detailForUpdate.ImportFactQuantity = (detailForUpdate.ImportFactQuantity ?? 0) + quantity;
                    }
                    // 更新入库流水的时间戳
                    importOrder.UpdateTime = now;
                    importOrder.UpdateUserName = user;
                    await _repos.ImportOrder.AsUpdateable(importOrder).UpdateColumns(x => new { x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();
                }
                // 【第七步：更新入库明细执行状态】如果明细状态为待执行("01")，更新为执行中("02")
                if (string.Equals(detailForUpdate.ImportExecuteFlag, "01", StringComparison.OrdinalIgnoreCase))
                {
                    // 02=执行中
                    detailForUpdate.ImportExecuteFlag = "02";
                }
                detailForUpdate.UpdateTime = now;
                detailForUpdate.UpdateUserName = user;
                await _repos.ImportNotifyDetail.AsUpdateable(detailForUpdate).UpdateColumns(x => new { x.ImportFactQuantity, x.ImportExecuteFlag, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();
                // 【第八步：更新入库单据主单执行状态】标记主单为执行中("02")
                // 02=执行中
                notify.ImportExecuteFlag = "02";
                notify.UpdateTime = now;
                notify.UpdateUserName = user;
                await _repos.ImportNotify.AsUpdateable(notify).UpdateColumns(x => new { x.ImportExecuteFlag, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();
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
    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 按物料条码维度查询待组托箱码信息
    /// </summary>
    /// <param name="input">查询入参</param>
    /// <returns>聚合后的结果</returns>
    private async Task<List<PdaStockSlotItem>> QueryByMaterialBarcodeAsync(PdaStockSlotQueryInput input)
    {
        // 解析物料条码：明细ID-物料ID-批次-(可选)流水ID
        var barcode = ParseMaterialBarcode(input.MaterialBarcode);
        // 解析入库单号为主键ID（支持单号或ID两种格式）
        var importId = await ResolveImportIdAsync(input.ImportBillCode);
        // 构建基础查询条件：仅查询未删除且待绑定的记录（Status=0）
        var query = _sqlViewService.QueryStockSlotBoxInfoView()
            .MergeTable()
            .Where(x => x.IsDelete == false && x.Status == 0);
        // 【条件1：入库单号】优先使用解析后的ID，否则使用字符串匹配
        if (importId.HasValue)
        {
            query = query.Where(x => x.ImportId == importId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(input.ImportBillCode))
        {
            var billCode = input.ImportBillCode.Trim();
            query = query.Where(x => SqlFunc.ToString(x.ImportId) == billCode);
        }
        // 【条件2：物料ID】优先使用解析后的ID，否则使用字符串匹配
        if (barcode.MaterialId.HasValue)
        {
            query = query.Where(x => x.MaterialId == barcode.MaterialId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(barcode.MaterialIdRaw))
        {
            var rawMaterialId = barcode.MaterialIdRaw;
            query = query.Where(x => SqlFunc.ToString(x.MaterialId) == rawMaterialId);
        }
        // 【条件3：批次号】如果条码中包含批次号，添加批次匹配条件
        if (!string.IsNullOrWhiteSpace(barcode.LotNo))
        {
            var lotNo = barcode.LotNo;
            query = query.Where(x => x.LotNo == lotNo);
        }
        // 【条件4：明细ID】优先使用解析后的ID，否则使用字符串匹配
        if (barcode.DetailId.HasValue)
        {
            query = query.Where(x => x.ImportDetailId == barcode.DetailId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(barcode.DetailIdRaw))
        {
            var rawDetailId = barcode.DetailIdRaw;
            query = query.Where(x => SqlFunc.ToString(x.ImportDetailId) == rawDetailId);
        }
        // 执行查询并按托盘编码升序排序
        var typedRows = await query
            .OrderBy(x => x.StockCode, OrderByType.Asc)
            .Select(x => new PdaStockSlotRaw
            {
                Id = x.Id,
                StockCode = x.StockCode ?? string.Empty,
                BoxCode = SqlFunc.Coalesce(x.BoxCode, string.Empty),
                LotNo = SqlFunc.Coalesce(x.LotNo, string.Empty),
                MaterialCode = SqlFunc.Coalesce(x.MaterialCode, string.Empty),
                MaterialName = SqlFunc.Coalesce(x.MaterialName, string.Empty),
                ImportDetailId = x.ImportDetailId,
                Qty = SqlFunc.Coalesce(x.Qty, 0m) ?? 0m
            })
            .ToListAsync();
        // 对查询结果按托盘/箱码聚合数量后返回
        return AggregateQuantity(typedRows);
    }

    /// <summary>
    /// 按托盘维度查询待组托箱码信息
    /// </summary>
    /// <param name="input">查询入参</param>
    /// <returns>聚合后的结果</returns>
    private async Task<List<PdaStockSlotItem>> QueryByStockAsync(PdaStockSlotQueryInput input)
    {
        // 构建基础查询条件：仅查询未删除且有入库流水的记录
        var query = _sqlViewService.QueryStockSlotBoxInfoView()
            .MergeTable()
            .Where(x => x.IsDelete == false)
            .Where(x => x.ImportOrderId != null && x.ImportOrderId != 0);
        // 【条件1：托盘编码】如果提供了托盘编码，按托盘查询
        if (!string.IsNullOrWhiteSpace(input.StockCode))
        {
            var stockCode = input.StockCode.Trim();
            query = query.Where(x => x.StockCode == stockCode);
        }
        else
        {
            // 未提供托盘编码时的回退逻辑
            if (!string.IsNullOrWhiteSpace(input.ImportBillCode))
            {
                // 优先按入库单号查询（兼容前端场景）
                var importBillCode = input.ImportBillCode.Trim();
                var resolvedImportId = await ResolveImportIdAsync(importBillCode);
                if (resolvedImportId.HasValue)
                {
                    query = query.Where(x => x.ImportId == resolvedImportId.Value);
                }
                else
                {
                    // ID解析失败，尝试按单据号字符串匹配
                    query = query.Where(x => SqlFunc.ToString(x.ImportId) == importBillCode || x.ImportBillCode == importBillCode);
                }
            }
            else
            {
                // 既没有托盘编码也没有入库单号，查询有剔除原因的记录（异常托盘）
                query = query.Where(x => !SqlFunc.IsNullOrEmpty(x.ReasonsForExcl));
            }
        }
        // 【条件2：执行状态】type="1"时不过滤执行状态，其他情况仅查询待执行和执行中的记录
        if (!string.Equals(input.Type, "1", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.ImportExecuteFlag == "01" || x.ImportExecuteFlag == "02");
        }
        // 【条件3：检验单号】如果提供了检验单号，按检验单号匹配
        if (!string.IsNullOrWhiteSpace(input.InspectBillCode))
        {
            var rawInspect = input.InspectBillCode.Trim();
            query = query.Where(x => SqlFunc.ToString(x.ImportId) == rawInspect);
        }
        // 执行查询并按箱码倒序排序（最新的箱码在前）
        var typedRows = await query
            .OrderByDescending(x => x.BoxCode)
            .Select(x => new PdaStockSlotRaw
            {
                Id = x.Id,
                StockCode = x.StockCode ?? string.Empty,
                BoxCode = SqlFunc.Coalesce(x.BoxCode, string.Empty),
                LotNo = SqlFunc.Coalesce(x.LotNo, string.Empty),
                MaterialCode = SqlFunc.Coalesce(x.MaterialCode, string.Empty),
                MaterialName = SqlFunc.Coalesce(x.MaterialName, string.Empty),
                ImportDetailId = x.ImportDetailId,
                Qty = SqlFunc.Coalesce(x.Qty, 0m) ?? 0m
            }).ToListAsync();
        // 对查询结果按托盘/箱码聚合数量后返回
        return AggregateQuantity(typedRows);
    }

    /// <summary>
    /// 对查询结果按托盘/箱码聚合数量
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
        // 按托盘、箱码、物料、批次分组聚合数量
        return source
            .GroupBy(x => new { x.StockCode, x.BoxCode, x.MaterialCode, x.MaterialName, x.LotNo })
            .Select(group => new PdaStockSlotItem
            {
                Id = group.First().Id,                     // 记录主键（取第一条）
                StockCode = group.Key.StockCode,           // 托盘编码
                BoxCode = group.Key.BoxCode,               // 箱码
                MaterialCode = group.Key.MaterialCode,     // 物料编码
                MaterialName = group.Key.MaterialName,     // 物料名称
                LotNo = group.Key.LotNo,                   // 批次号
                ImportDetailId = group.First().ImportDetailId,  // 入库明细ID（取第一条）
                Qty = group.First().Qty,                   // 原始数量（取第一条）
                Quantity = group.Sum(item => item.Qty)     // 累加总量
            }).OrderByDescending(item => item.BoxCode).ToList();
    }

    #endregion
}
