// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core.Service;
using Microsoft.AspNetCore.Http;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Mapster;
using SqlSugar;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Admin.NET.Application.Entity;
namespace Admin.NET.Application;

/// <summary>
/// 库存箱码明细服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsStockSlotBoxInfoService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsStockSlotBoxInfo> _wmsStockSlotBoxInfoRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsStockSlotBoxInfoService(SqlSugarRepository<WmsStockSlotBoxInfo> wmsStockSlotBoxInfoRep, ISqlSugarClient sqlSugarClient)
    {
        _wmsStockSlotBoxInfoRep = wmsStockSlotBoxInfoRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询库存箱码明细 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询库存箱码明细")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsStockSlotBoxInfoOutput>> Page(PageWmsStockSlotBoxInfoInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsStockSlotBoxInfoRep.AsQueryable()
            .LeftJoin<WmsBaseMaterial>((u, material) => u.MaterialId == material.Id)
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), (u, material) => material.MaterialCode.Contains(input.MaterialCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialName), (u, material) => material.MaterialName.Contains(input.MaterialName.Trim()))
            .LeftJoin<WmsImportNotifyDetail>((u, material, detail) => u.ImportDetailId == detail.Id)
            .LeftJoin<WmsSysStockCode>((u, material, detail, stockCode) => u.StockCodeId == stockCode.Id)
            .LeftJoin<WmsImportNotify>((u, material, detail, stockCode, notify) => u.ImportId == notify.Id)
            .LeftJoin<WmsImportOrder>((u, material, detail, stockCode, notify, order) => u.ImportOrderId == order.Id)
            //.LeftJoin<WmsInspectNotity>((u, material, detail, stockCode, notify, order, inspect) => u.ImportId == inspect.Id)
            .LeftJoin<WmsBaseUnit>((u, material, detail, stockCode, notify, order, unit) => material.MaterialUnit == unit.Id)
            // 查询条件
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u =>
                u.BoxCode.Contains(input.Keyword) ||
                u.StockCode.Contains(input.Keyword) ||
                u.LotNo.Contains(input.Keyword) ||
                u.QRCode.Contains(input.Keyword) ||
                u.StaffCode.Contains(input.Keyword) ||
                u.StaffName.Contains(input.Keyword) ||
                u.ReasonsForExcl.Contains(input.Keyword) ||
                u.PickingSlurry.Contains(input.Keyword) ||
                u.plasmaRejectTypeId.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.BoxCode), u => u.BoxCode.Contains(input.BoxCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StockCode), u => u.StockCode.Contains(input.StockCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.LotNo), u => u.LotNo.Contains(input.LotNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.QRCode), u => u.QRCode.Contains(input.QRCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StaffCode), u => u.StaffCode.Contains(input.StaffCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StaffName), u => u.StaffName.Contains(input.StaffName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ReasonsForExcl), u => u.ReasonsForExcl.Contains(input.ReasonsForExcl.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.PickingSlurry), u => u.PickingSlurry.Contains(input.PickingSlurry.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.plasmaRejectTypeId), u => u.plasmaRejectTypeId.Contains(input.plasmaRejectTypeId.Trim()))
            // 添加 ImportOrderId 查询条件
            .WhereIF(input.ImportOrderId != null && Convert.ToInt64(input.ImportOrderId) > 0, u => u.ImportOrderId == Convert.ToInt64(input.ImportOrderId))
            .WhereIF(input.SupplierId != null, u => u.SupplierId == input.SupplierId)
            .WhereIF(input.ManufacturerId != null, u => u.ManufacturerId == input.ManufacturerId)
            .WhereIF(input.Status != null, u => u.Status == input.Status)
            .WhereIF(input.BoxLevel != null, u => u.BoxLevel == input.BoxLevel)
            .WhereIF(input.ProductionDateRange?.Length == 2, u => u.ProductionDate >= input.ProductionDateRange[0] && u.ProductionDate <= input.ProductionDateRange[1])
            .WhereIF(input.ValidateDayRange?.Length == 2, u => u.ValidateDay >= input.ValidateDayRange[0] && u.ValidateDay <= input.ValidateDayRange[1])
            .WhereIF(input.BulkTank != null, u => u.BulkTank == input.BulkTank)
            .WhereIF(input.IsSamplingBox != null, u => u.IsSamplingBox == input.IsSamplingBox)
            .WhereIF(input.InspectionStatus != null, u => u.InspectionStatus == input.InspectionStatus)
            .WhereIF(input.SamplingDateRange?.Length == 2, u => u.SamplingDate >= input.SamplingDateRange[0] && u.SamplingDate <= input.SamplingDateRange[1])
            .WhereIF(input.ExtractStatus != null, u => u.ExtractStatus == input.ExtractStatus)
            .Select((u, material, detail, stockCode, notify, order, unit) => new WmsStockSlotBoxInfoOutput
            {
                Id = u.Id,
                BoxCode = u.BoxCode,
                StockCodeId = u.StockCodeId,
                Qty = u.Qty,
                ImportOrderId = u.ImportOrderId,
                ImportOrderNo = order.ImportOrderNo,
                Status = u.Status,
                BoxLevel = u.BoxLevel,
                LotNo = u.LotNo,
                BulkTank = u.BulkTank,
                ProductionDate = u.ProductionDate.Value.ToString("yyyy-MM-dd"),
                ValidateDay = u.ValidateDay.Value.ToString("yyyy-MM-dd"),
                MaterialId = u.MaterialId,
                MaterialCode = material.MaterialCode,
                MaterialName = material.MaterialName,
                SupplierId = u.SupplierId,
                ManufacturerId = u.ManufacturerId,
                ImportDetailId = u.ImportDetailId,
                ImportExecuteFlag = detail.ImportExecuteFlag,
                ImportId = u.ImportId,
                ImportQuantity = detail.ImportQuantity,
                ImportFactQuantity = detail.ImportFactQuantity,
                StockCode = stockCode.StockCode,
                ImportBillCode = notify.ImportBillCode,
                //InspectBillCode = inspect.InspectBillCode,
                IsSamplingBox = u.IsSamplingBox,
                SamplingDate = u.SamplingDate,
                StaffCode = u.StaffCode,
                StaffName = u.StaffName,
                Weight = u.Weight,
                ReasonsForExcl = u.ReasonsForExcl,
                plasmaRejectTypeId = u.plasmaRejectTypeId,
                ExtractStatus = u.ExtractStatus,
                //InspectionStatus = u.InspectionStatus,
                PickingSlurry = u.PickingSlurry,
                MaterialStandard = material.MaterialStandard,
                UnitName = unit.UnitName,
                BoxQuantity = material.BoxQuantity,
                CreateUserName = u.CreateUserName,
                CreateTime = u.CreateTime,
                UpdateUserName = u.UpdateUserName,
                UpdateTime = u.UpdateTime,
            });

        var result = await query.OrderBuilder(input, "u.").ToPagedListAsync(input.Page, input.PageSize); 
        return result;
    }

    /// <summary>
    /// 获取库存箱码明细详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取库存箱码明细详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsStockSlotBoxInfo> Detail([FromQuery] QueryByIdWmsStockSlotBoxInfoInput input)
    {
        return await _wmsStockSlotBoxInfoRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加库存箱码明细 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加库存箱码明细")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsStockSlotBoxInfoInput input)
    {
        var entity = input.Adapt<WmsStockSlotBoxInfo>();
        return await _wmsStockSlotBoxInfoRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新库存箱码明细 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新库存箱码明细")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsStockSlotBoxInfoInput input)
    {
        var entity = input.Adapt<WmsStockSlotBoxInfo>();
        await _wmsStockSlotBoxInfoRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除库存箱码明细 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除库存箱码明细")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsStockSlotBoxInfoInput input)
    {
        var entity = await _wmsStockSlotBoxInfoRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsStockSlotBoxInfoRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsStockSlotBoxInfoRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除库存箱码明细 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除库存箱码明细")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")] List<DeleteWmsStockSlotBoxInfoInput> input)
    {
        var exp = Expressionable.Create<WmsStockSlotBoxInfo>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsStockSlotBoxInfoRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();

        return await _wmsStockSlotBoxInfoRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsStockSlotBoxInfoRep.DeleteAsync(list);   //真删除
    }

    /// <summary>
    /// 导出库存箱码明细记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出库存箱码明细记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsStockSlotBoxInfoInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsStockSlotBoxInfoOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "库存箱码明细导出记录");
    }

    /// <summary>
    /// 下载库存箱码明细数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载库存箱码明细数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsStockSlotBoxInfoOutput>(), "库存箱码明细导入模板");
    }

    private static readonly object _wmsStockSlotBoxInfoImportLock = new object();
    /// <summary>
    /// 导入库存箱码明细记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入库存箱码明细记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsStockSlotBoxInfoImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportWmsStockSlotBoxInfoInput, WmsStockSlotBoxInfo>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {

                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x =>
                    {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<WmsStockSlotBoxInfo>>();

                    var storageable = _wmsStockSlotBoxInfoRep.Context.Storageable(rows)
                        .SplitError(it => it.Item.BoxCode?.Length > 32, "箱码长度不能超过32个字符")
                        .SplitError(it => it.Item.StockCode?.Length > 50, "托盘条码长度不能超过50个字符")
                        .SplitError(it => it.Item.LotNo?.Length > 32, "批次长度不能超过32个字符")
                        .SplitError(it => it.Item.QRCode?.Length > 32, "erp箱二维码长度不能超过32个字符")
                        .SplitError(it => it.Item.StaffCode?.Length > 32, "浆员编码长度不能超过32个字符")
                        .SplitError(it => it.Item.StaffName?.Length > 32, "浆员姓名长度不能超过32个字符")
                        .SplitError(it => it.Item.ReasonsForExcl?.Length > 100, "剔除原因（手持剔除异常血浆存放）长度不能超过100个字符")
                        .SplitError(it => it.Item.PickingSlurry?.Length > 1, "是否挑浆（0默认；1挑浆）长度不能超过1个字符")
                        .SplitError(it => it.Item.plasmaRejectTypeId?.Length > 50, "血浆拒收类型id长度不能超过50个字符")
                        .SplitInsert(_ => true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();

                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.BoxCode,
                        it.Qty,
                        it.FullBoxQty,
                        it.StockCode,
                        it.StockCodeId,
                        it.Status,
                        it.ImportId,
                        it.ImportDetailId,
                        it.ImportOrderId,
                        it.BoxLevel,
                        it.ProductionDate,
                        it.ValidateDay,
                        it.LotNo,
                        it.MaterialId,
                        it.BulkTank,
                        it.IsSamplingBox,
                        it.InspectionStatus,
                        it.QRCode,
                        it.SamplingDate,
                        it.StaffCode,
                        it.StaffName,
                        it.Weight,
                        it.ReasonsForExcl,
                        it.ExtractStatus,
                        it.PickingSlurry,
                        it.plasmaRejectTypeId
                    }).ExecuteCommand();// 存在更新

                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });

            return stream;
        }
    }
}
