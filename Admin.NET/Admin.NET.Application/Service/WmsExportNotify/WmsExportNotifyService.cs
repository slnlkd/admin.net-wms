// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.BaseService;
using Admin.NET.Core.Service;
using AngleSharp.Dom;
using Dm.util;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2013.Word;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using DocumentFormat.OpenXml.Wordprocessing;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Humanizer;
using Mapster;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson.IO;
using NewLife;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using SqlSugar;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq.Dynamic.Core;
using System.Reflection.Emit;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
namespace Admin.NET.Application;

/// <summary>
/// 出库单据表服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsExportNotifyService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsStockTray> _wmsStockTrayRep;
    private readonly SqlSugarRepository<WmsImportOrder> _wmsImportOrderRep;
    private readonly SqlSugarRepository<WmsExportOrder> _wmsExportOrderRep;
    private readonly SqlSugarRepository<WmsInspectOrder> _wmsInspectOrderRep;

    private readonly SqlSugarRepository<WmsExportNotify> _wmsExportNotifyRep;
    private readonly SqlSugarRepository<WmsExportNotifyDetail> _wmsExportNotifyDetailRep;
    private readonly SqlSugarRepository<WmsBaseBillType> _wmsBaseBillTypeRep;
    private readonly SqlSugarRepository<WmsBaseWareHouse> _wmsBaseWareHouseRep;
    private readonly SqlSugarRepository<WmsExportWayOut> _wmsExportWayOutRep;
    private readonly SqlSugarRepository<WmsBaseCustomer> _wmsBaseCustomerRep;
    private readonly SqlSugarRepository<WmsImportNotifyDetail> _wmsImportNotifyDetailRep;
    private readonly SqlSugarRepository<WmsBaseMaterial> _wmsBaseMaterialRep;
    private readonly SqlSugarRepository<WmsStock> _wmsStockRep;
    private readonly SqlSugarRepository<SysDictType> _sysDictTypeRep;
    private readonly SqlSugarRepository<SysDictData> _sysDictDataRep;
    private readonly SqlSugarRepository<WmsBaseUnit> _wmsBaseUnitRep;
    private readonly SqlSugarRepository<WmsBaseDepartment> _wmsBaseDepartmentRep;
    private readonly SqlSugarRepository<WmsBaseSupplier> _wmsBaseSupplierRep;
    private readonly SqlSugarRepository<WmsBaseArea> _wmsBaseAreaRep;
    private readonly SqlSugarRepository<WmsExportBoxInfo> _wmsExportBoxInfoRep;
    private readonly SqlSugarRepository<WmsImportNotify> _wmsImportNotifyRep;//_wmsSysStockCodeRep
    private readonly SqlSugarRepository<WmsExportTask> _wmsExportTaskRep;
    private readonly SqlSugarRepository<WmsInspectNotity> _wmsInspectNotityRep;
    private readonly SqlSugarRepository<WmsStockCheckNotify> _wmsStockCheckNotifyRep;
    private readonly SqlSugarRepository<WmsImportTask> _wmsImportTaskRep;
    private readonly SqlSugarRepository<WmsInterfaceLog> _wmsInterfaceLogRep;
    private readonly SqlSugarRepository<WmsInspectTask> _wmsInspectTaskRep;
    private readonly SqlSugarRepository<WmsStockCheckTask> _wmsStockCheckTaskRep;
    private readonly SqlSugarRepository<WmsMoveTask> _wmsMoveTaskRep;
    private readonly SqlSugarRepository<SysUser> _sysUserRep;
    private readonly SqlSugarRepository<WmsStockInfo> _wmsStockInfoRep;
    private readonly SqlSugarRepository<WmsBaseSlot> _wmsBaseSlotRep;
    private readonly SqlSugarRepository<WmsButtonRecord> _wmsButtonRecordRep;
    private readonly SqlSugarRepository<WmsSysStockCode> _wmsSysStockCodeRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsExportNotifyService(SqlSugarRepository<WmsStockTray> wmsStockTrayRep,
        SqlSugarRepository<WmsImportOrder> wmsImportOrderRep,
        SqlSugarRepository<WmsExportOrder> wmsExportOrderRep,
        SqlSugarRepository<WmsInspectOrder> wmsInspectOrderRep,
        SqlSugarRepository<WmsExportNotify> wmsExportNotifyRep,
        SqlSugarRepository<WmsExportNotifyDetail> wmsExportNotifyDetailRep,
        SqlSugarRepository<WmsBaseBillType> wmsBaseBillTypeRep,
        SqlSugarRepository<WmsBaseWareHouse> wmsBaseWareHouseRep,
        SqlSugarRepository<WmsExportWayOut> wmsExportWayOutRep,
        SqlSugarRepository<WmsBaseCustomer> wmsBaseCustomerRep,
        SqlSugarRepository<WmsImportNotifyDetail> wmsImportNotifyDetailRep,
        SqlSugarRepository<WmsBaseMaterial> wmsBaseMaterialRep,
        SqlSugarRepository<WmsStock> wmsStockRep,
        SqlSugarRepository<SysDictType> sysDictTypeRep,
        SqlSugarRepository<SysDictData> sysDictDataRep,
        SqlSugarRepository<WmsBaseUnit> wmsBaseUnitRep,
        SqlSugarRepository<WmsBaseDepartment> wmsBaseDepartmentRep,
        SqlSugarRepository<WmsBaseSupplier> wmsBaseSupplierRep,
        SqlSugarRepository<WmsBaseArea> wmsBaseAreaRep,
        SqlSugarRepository<WmsExportBoxInfo> wmsExportBoxInfoRep,
        SqlSugarRepository<WmsExportTask> wmsExportTaskRep,
        SqlSugarRepository<WmsImportNotify> wmsImportNotifyRep,
        SqlSugarRepository<WmsInspectNotity> wmsInspectNotityRep,
        SqlSugarRepository<WmsStockCheckNotify> wmsStockCheckNotifyRep,
        SqlSugarRepository<WmsImportTask> wmsImportTaskRep,
        SqlSugarRepository<WmsInterfaceLog> wmsInterfaceLogRep,
        SqlSugarRepository<WmsInspectTask> wmsInspectTaskRep,
        SqlSugarRepository<WmsStockCheckTask> wmsStockCheckTaskRep,
        SqlSugarRepository<WmsMoveTask> wmsMoveTaskRep,
        SqlSugarRepository<SysUser> sysUserRep,
        SqlSugarRepository<WmsBaseSlot> wmsBaseSlotRep,
    SqlSugarRepository<WmsStockInfo> wmsStockInfoRep,
    SqlSugarRepository<WmsButtonRecord> wmsButtonRecordRep,
    SqlSugarRepository<WmsSysStockCode> wmsSysStockCodeRep,
    ISqlSugarClient sqlSugarClient)
    {
        _wmsImportOrderRep = wmsImportOrderRep;
        _wmsSysStockCodeRep = wmsSysStockCodeRep;
        _wmsInspectOrderRep = wmsInspectOrderRep;
        _wmsExportNotifyRep = wmsExportNotifyRep;
        _wmsExportNotifyDetailRep = wmsExportNotifyDetailRep;
        _wmsBaseBillTypeRep = wmsBaseBillTypeRep;
        _wmsBaseWareHouseRep = wmsBaseWareHouseRep;
        _wmsExportWayOutRep = wmsExportWayOutRep;
        _wmsBaseCustomerRep = wmsBaseCustomerRep;
        _wmsImportNotifyDetailRep = wmsImportNotifyDetailRep;
        _wmsBaseMaterialRep = wmsBaseMaterialRep;
        _wmsStockRep = wmsStockRep;
        _sysDictTypeRep = sysDictTypeRep;
        _sysDictDataRep = sysDictDataRep;
        _wmsBaseUnitRep = wmsBaseUnitRep;
        _wmsBaseDepartmentRep = wmsBaseDepartmentRep;
        _wmsBaseSupplierRep = wmsBaseSupplierRep;
        _wmsBaseAreaRep = wmsBaseAreaRep;
        _wmsExportBoxInfoRep = wmsExportBoxInfoRep;
        _wmsExportOrderRep = wmsExportOrderRep;
        _wmsExportTaskRep = wmsExportTaskRep;
        _wmsImportNotifyRep = wmsImportNotifyRep;
        _wmsStockTrayRep = wmsStockTrayRep;
        _wmsInspectNotityRep = wmsInspectNotityRep;
        _wmsStockCheckNotifyRep = wmsStockCheckNotifyRep;
        _wmsImportTaskRep = wmsImportTaskRep;
        _wmsInterfaceLogRep = wmsInterfaceLogRep;
        _wmsInspectTaskRep = wmsInspectTaskRep;
        _wmsStockCheckTaskRep = wmsStockCheckTaskRep;
        _wmsMoveTaskRep = wmsMoveTaskRep;
        _sysUserRep = sysUserRep;
        _wmsStockInfoRep = wmsStockInfoRep;
        _wmsBaseSlotRep = wmsBaseSlotRep;
        _wmsButtonRecordRep = wmsButtonRecordRep;
        _sqlSugarClient = sqlSugarClient;
    }

    #region 自动生成
    /// <summary>
    /// 分页查询出库单据表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询出库单据表")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsExportNotifyOutput>> Page(PageWmsExportNotifyInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsExportNotifyRep.AsQueryable()
            //.WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.ExportBillCode.Contains(input.Keyword) || u.ExportBillType.Contains(input.Keyword) || u.ExportLotNo.Contains(input.Keyword) || u.MaterialId.Contains(input.Keyword) || u.WarehouseId.Contains(input.Keyword) || u.ExportListNo.Contains(input.Keyword) || u.ExportDepartmentId.Contains(input.Keyword) || u.ExportSupplierId.Contains(input.Keyword) || u.ExportCustomerId.Contains(input.Keyword) || u.ExportRemark.Contains(input.Keyword) || u.OuterBillCode.Contains(input.Keyword) || u.OuterMainId.Contains(input.Keyword) || u.Source.Contains(input.Keyword) || u.PickingArea.Contains(input.Keyword) || u.WholeOutWare.Contains(input.Keyword) || u.SortOutWare.Contains(input.Keyword))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.ExportBillCode), u => u.ExportBillCode.Contains(input.ExportBillCode.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.ExportBillType), u => u.ExportBillType.Contains(input.ExportBillType.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.ExportLotNo), u => u.ExportLotNo.Contains(input.ExportLotNo.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.MaterialId), u => u.MaterialId.Contains(input.MaterialId.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.WarehouseId), u => u.WarehouseId.Contains(input.WarehouseId.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.ExportListNo), u => u.ExportListNo.Contains(input.ExportListNo.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.ExportDepartmentId), u => u.ExportDepartmentId.Contains(input.ExportDepartmentId.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.ExportSupplierId), u => u.ExportSupplierId.Contains(input.ExportSupplierId.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.ExportCustomerId), u => u.ExportCustomerId.Contains(input.ExportCustomerId.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.ExportRemark), u => u.ExportRemark.Contains(input.ExportRemark.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.OuterBillCode), u => u.OuterBillCode.Contains(input.OuterBillCode.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.OuterMainId), u => u.OuterMainId.Contains(input.OuterMainId.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.Source), u => u.Source.Contains(input.Source.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.PickingArea), u => u.PickingArea.Contains(input.PickingArea.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.WholeOutWare), u => u.WholeOutWare.Contains(input.WholeOutWare.Trim()))
            //.WhereIF(!string.IsNullOrWhiteSpace(input.SortOutWare), u => u.SortOutWare.Contains(input.SortOutWare.Trim()))
            .WhereIF(input.ExportProductionDateRange?.Length == 2, u => u.ExportProductionDate >= input.ExportProductionDateRange[0] && u.ExportProductionDate <= input.ExportProductionDateRange[1])
            .WhereIF(input.ExportLostDateRange?.Length == 2, u => u.ExportLostDate >= input.ExportLostDateRange[0] && u.ExportLostDate <= input.ExportLostDateRange[1])
            .WhereIF(input.ExportDateRange?.Length == 2, u => u.ExportDate >= input.ExportDateRange[0] && u.ExportDate <= input.ExportDateRange[1])
            .WhereIF(input.ExportExecuteFlag != null, u => u.ExportExecuteFlag == input.ExportExecuteFlag)
            .WhereIF(input.PXStatus != null, u => u.PXStatus == input.PXStatus)
            .Select<WmsExportNotifyOutput>();
        return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取出库单据表详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取出库单据表详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsExportNotify> Detail([FromQuery] QueryByIdWmsExportNotifyInput input)
    {
        return await _wmsExportNotifyRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加出库单据表 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加出库单据表")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsExportNotifyInput input)
    {
        var entity = input.Adapt<WmsExportNotify>();
        return await _wmsExportNotifyRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新出库单据表 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新出库单据表")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task<int> Update(UpdateWmsExportNotifyInput input)
    {
        var entity = input.Adapt<WmsExportNotify>();
        return await _wmsExportNotifyRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除出库单据表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除出库单据表")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task<int> Delete(DeleteWmsExportNotifyInput input)
    {
        var entity = await _wmsExportNotifyRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        return await _wmsExportNotifyRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsExportNotifyRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除出库单据表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除出库单据表")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")] List<DeleteWmsExportNotifyInput> input)
    {
        var exp = Expressionable.Create<WmsExportNotify>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsExportNotifyRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();

        return await _wmsExportNotifyRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsExportNotifyRep.DeleteAsync(list);   //真删除
    }

    /// <summary>
    /// 导出出库单据表记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出出库单据表记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsExportNotifyInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsExportNotifyOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "出库单据表导出记录");
    }

    /// <summary>
    /// 下载出库单据表数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载出库单据表数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsExportNotifyOutput>(), "出库单据表导入模板");
    }

    private static readonly object _wmsExportNotifyImportLock = new object();
    /// <summary>
    /// 导入出库单据表记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入出库单据表记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsExportNotifyImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportWmsExportNotifyInput, WmsExportNotify>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {

                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x =>
                    {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<WmsExportNotify>>();

                    var storageable = _wmsExportNotifyRep.Context.Storageable(rows)
                        .SplitError(it => it.Item.ExportBillCode?.Length > 100, "出库单据长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportBillType > 100, "单据类型长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportLotNo?.Length > 200, "出库批次长度不能超过200个字符")
                        .SplitError(it => it.Item.MaterialId > 200, "物料ID长度不能超过200个字符")
                        .SplitError(it => it.Item.WarehouseId > 200, "仓库ID长度不能超过200个字符")
                        .SplitError(it => it.Item.ExportListNo?.Length > 100, "出库序号长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportDepartmentId > 100, "部门ID长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportSupplierId > 100, "供应商ID长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportCustomerId > 100, "客户ID长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportRemark?.Length > 2147483647, "备注长度不能超过2147483647个字符")
                        .SplitError(it => it.Item.OuterBillCode?.Length > 100, "外部单据编码长度不能超过100个字符")
                        .SplitError(it => it.Item.OuterMainId?.Length > 100, "外部单据ID长度不能超过100个字符")
                        .SplitError(it => it.Item.Source?.Length > 100, "来源（wms sap）长度不能超过100个字符")
                        .SplitError(it => it.Item.PickingArea?.Length > 100, "拣货区长度不能超过100个字符")
                        .SplitError(it => it.Item.WholeOutWare?.Length > 100, "整托出库口长度不能超过100个字符")
                        .SplitError(it => it.Item.SortOutWare?.Length > 100, "分拣出库口长度不能超过100个字符")
                        .SplitInsert(_ => true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();

                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.ExportBillCode,
                        it.ExportBillType,
                        it.ExportLotNo,
                        it.MaterialId,
                        it.WarehouseId,
                        it.ExportListNo,
                        it.ExportDepartmentId,
                        it.ExportSupplierId,
                        it.ExportCustomerId,
                        it.ExportProductionDate,
                        it.ExportLostDate,
                        it.ExportQuantity,
                        it.ExportFactQuantity,
                        it.ExportCompleteQuantity,
                        it.ExportUploadQuantity,
                        it.ExportDate,
                        it.ExportExecuteFlag,
                        it.ExportRemark,
                        it.OuterBillCode,
                        it.OuterMainId,
                        it.Source,
                        it.PickingArea,
                        it.PXStatus,
                        it.WholeOutWare,
                        it.SortOutWare,
                    }).ExecuteCommand();// 存在更新

                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });

            return stream;
        }
    }
    #endregion


    #region 旧的
    /// <summary>
    /// 查询单据类型表，给下拉框赋值
    /// </summary>
    /// <returns></returns>
    [DisplayName("查询单据类型表，给下拉框赋值")]
    [ApiDescriptionSettings(Name = "GetWmsBaseBillTypeForSelect"), HttpGet]
    public async Task<List<WmsBaseBillType>> GetWmsBaseBillTypeForSelect()
    {
        try
        {

            return await _wmsBaseBillTypeRep.AsQueryable()
                .Where(x => x.IsDelete == false)
                .ToListAsync();

            //查询字典中“单据类型”的数据
            //var dicQuery = await _sysDictDataRep.AsQueryable()
            //    .LeftJoin<SysDictType>((x, a) => x.DictTypeId == a.Id)
            //    .Where((x, a) => x.Status == StatusEnum.Enable)
            //    .Where((x, a) => a.Status == StatusEnum.Enable)
            //    .Where((x, a) => a.Code == "ImportBillType")  //“单据类型”的数据
            //    .ToListAsync();
        }
        catch (Exception ex)
        {
            return new List<WmsBaseBillType>();
        }
    }

    /// <summary>
    /// 查询部门表，给下拉框赋值
    /// </summary>
    /// <returns></returns>
    [DisplayName("查询部门表，给下拉框赋值")]
    [ApiDescriptionSettings(Name = "GetWmsBaseDepartmentForSelect"), HttpGet]
    public async Task<List<WmsBaseDepartment>> GetWmsBaseDepartmentForSelect()
    {
        try
        {
            return await _wmsBaseDepartmentRep.AsQueryable()
                .Where(x => x.IsDelete == false)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            return new List<WmsBaseDepartment>();
        }
    }


    /// <summary>
    /// 查询供应商表，给下拉框赋值
    /// </summary>
    /// <returns></returns>
    [DisplayName("查询供应商表，给下拉框赋值")]
    [ApiDescriptionSettings(Name = "GetWmsBaseSupplierForSelect"), HttpGet]
    public async Task<List<WmsBaseSupplier>> GetWmsBaseSupplierForSelect()
    {
        try
        {
            return await _wmsBaseSupplierRep.AsQueryable()
                .Where(x => x.IsDelete == false)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            return new List<WmsBaseSupplier>();
        }
    }


    /// <summary>
    /// 查询区域表，给下拉框赋值
    /// </summary>
    /// <returns></returns>
    [DisplayName("查询区域表，给下拉框赋值")]
    [ApiDescriptionSettings(Name = "GetWmsBaseAreaForSelect"), HttpGet]
    public async Task<List<WmsBaseArea>> GetWmsBaseAreaForSelect()
    {
        try
        {
            return await _wmsBaseAreaRep.AsQueryable()
                .Where(x => x.IsDelete == false)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            return new List<WmsBaseArea>();
        }
    }




    /// <summary>
    /// 查询单据子类型表，给下拉框赋值
    /// </summary>
    /// <returns></returns>
    [DisplayName("查询单据子类型表，给下拉框赋值")]
    [ApiDescriptionSettings(Name = "GetWmsBaseChildBillTypeForSelect"), HttpPost]
    public async Task<List<WmsBaseBillType>> GetWmsBaseChildBillTypeForSelect(WmsBaseChildBillTypeWhere dto)
    {
        try
        {

            return await _wmsBaseBillTypeRep.AsQueryable()
                .WhereIF(dto.Id != null && dto.Id is not null && dto.Id != -1, x => x.SubtypeID == dto.Id.toString())
                .Where(x => x.IsDelete == false)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            return new List<WmsBaseBillType>();
        }
    }




    /// <summary>
    /// 查询仓库表，给下拉框赋值
    /// </summary>
    /// <returns></returns>
    [DisplayName("查询仓库表，给下拉框赋值")]
    [ApiDescriptionSettings(Name = "GetWmsBaseWareHouseForSelect"), HttpGet]
    public async Task<List<WmsBaseWareHouse>> GetWmsBaseWareHouseForSelect()
    {
        try
        {
            return await _wmsBaseWareHouseRep.AsQueryable()
                .Where(x => x.IsDelete == false)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            return new List<WmsBaseWareHouse>();
        }
    }




    /// <summary>
    /// 分页查询出库单据表
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("查询出库单据--分页查询")]
    [ApiDescriptionSettings(Name = "ShowWmsExportNotifyOfPage"), HttpPost]
    public async Task<SqlSugarPagedList<WmsExportNotifyOfPage>> ShowWmsExportNotifyOfPage(WmsExportNotifyWhere dto)
    {
        try
        {
            dto.Keyword = dto.Keyword?.Trim();
            var listNofityDbOfPage = await _wmsExportNotifyRep.AsQueryable()
                .WhereIF(string.IsNullOrEmpty(dto.ExportBillCode) == false, x => x.ExportBillCode.Contains(dto.ExportBillCode))
                .WhereIF(dto.ExportExecuteFlag != -1 && dto.ExportExecuteFlag != null, x => x.ExportExecuteFlag == dto.ExportExecuteFlag)
                .WhereIF(dto.ExportBillType != -1 && dto.ExportBillType != null, x => x.ExportBillType == dto.ExportBillType)
                .WhereIF(dto.WarehouseId != -1 && dto.WarehouseId != null, x => x.WarehouseId == dto.WarehouseId)
                .WhereIF(dto.StartTime != null && dto.StartTime is not null, x => x.CreateTime >= dto.StartTime)
                .WhereIF(dto.EndTime != null && dto.EndTime is not null, x => x.CreateTime <= dto.EndTime)
                .Where(x => x.IsDelete == false)
                .LeftJoin<WmsBaseWareHouse>((x, a) => x.WarehouseId == a.Id)
                .LeftJoin<WmsBaseBillType>((x, a, b) => x.ExportBillType == b.Id)
                .LeftJoin<WmsBaseCustomer>((x, a, b, c) => x.ExportCustomerId == c.Id)
                .LeftJoin<WmsBaseBillType>((x, a, b, c, d) => x.DocumentSubtype == d.Id)
                .OrderBy((x, a, b, c, d) => x.CreateTime)
                .OrderBy((x, a, b, c, d) => x.ExportBillCode)
                .Select((x, a, b, c, d) => new WmsExportNotifyOfPage
                {
                    Id = x.Id,
                    Index = 0,
                    ExportBillCode = x.ExportBillCode,
                    ExportExecuteFlag = x.ExportExecuteFlag,
                    ExportExecuteFlagStr = "",
                    WarehouseId = x.WarehouseId,
                    WarehouseStr = a.WarehouseName,
                    ExportBillType = x.ExportBillType,
                    ExportBillTypeStr = b.BillTypeName,
                    DocumentSubtype = d.Id,
                    DocumentSubtypeStr = d.BillTypeName,
                    Source = x.Source,
                    OuterBillCode = x.OuterBillCode,
                    ExportCustomerId = x.ExportCustomerId,
                    ExportCustomerStr = c.CustomerName,
                    CreateUserName = x.CreateUserName,
                    CreateTime = x.CreateTime,
                    UpdateUserName = x.UpdateUserName,
                    UpdateTime = x.UpdateTime,
                    ExportRemark = x.ExportRemark

                }).ToPagedListAsync(dto.Page, dto.PageSize);
            //var listNofityDbOfPage = await query.OrderBuilder(dto).ToPagedListAsync(dto.Page, dto.PageSize);
            //获取表格展示的数据
            var listNofity = listNofityDbOfPage.Items;
            var index = 1;
            foreach (var item in listNofity)
            {
                item.Index = index;
                index++;
                switch (item.ExportExecuteFlag)
                {
                    case 0:
                        item.ExportExecuteFlagStr = "待执行";
                        break;
                    case 1:
                        item.ExportExecuteFlagStr = "正在分配";
                        break;
                    case 2:
                        item.ExportExecuteFlagStr = "正在执行";
                        break;
                    case 3:
                        item.ExportExecuteFlagStr = "已完成";
                        break;
                    case 4:
                        item.ExportExecuteFlagStr = "作废";
                        break;
                    case 5:
                        item.ExportExecuteFlagStr = "已上传";
                        break;
                    default:
                        item.ExportExecuteFlagStr = "状态出错";
                        break;
                }

            }
            listNofityDbOfPage.Items = listNofity;
            return listNofityDbOfPage;
        }
        catch (Exception ex)
        {
            SqlSugarPagedList<WmsExportNotifyOfPage> list = new SqlSugarPagedList<WmsExportNotifyOfPage>();
            list.Page = dto.Page;
            list.PageSize = dto.PageSize;
            return list;
        }
    }


    /// <summary>
    /// 根据仓库id查询出库口列表
    /// </summary>
    /// <param name="wareHouseId"></param>
    /// <returns></returns>
    [DisplayName("根据仓库id查询出库口列表")]
    [ApiDescriptionSettings(Name = "GetWmsExportWayOutByWareHouseId"), HttpPost]
    public async Task<List<WmsExportWayOut>> GetWmsExportWayOutByWareHouseId(WmsExportWayOutWhere dto)
    {
        try
        {
            var query = await _wmsExportWayOutRep.AsQueryable()
                .WhereIF(dto.WareHouseId != null && dto.WareHouseId != -1, x => x.BaseWareHouseId == dto.WareHouseId)
                .Where(x => x.IsDelete == false)
                .ToListAsync();
            return query;
        }
        catch (Exception ex)
        {
            return new List<WmsExportWayOut>();
        }
    }

    /// <summary>
    /// 查询领用单位
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("查询领用单位")]
    [ApiDescriptionSettings(Name = "ShowCompanyTable"), HttpPost]
    public async Task<SqlSugarPagedList<ShowCompanyTableOfPage>> ShowCompanyTable(ShowCompanyTableWhere dto)
    {
        try
        {
            return await _wmsBaseCustomerRep.AsQueryable()
                .WhereIF(!string.IsNullOrEmpty(dto.WhereStr), x => x.CustomerCode.Contains(dto.WhereStr) || x.CustomerName.Contains(dto.WhereStr) || x.CustomerAddress.Contains(dto.WhereStr))
                .Where(x => x.IsDelete == false)
                .OrderBy((x) => x.CreateTime)
                .OrderBy((x) => x.CustomerCode)
                .Select((x) => new ShowCompanyTableOfPage
                {
                    Id = x.Id,
                    CustomerCode = x.CustomerCode,
                    CustomerName = x.CustomerName,
                    CustomerAddress = x.CustomerAddress

                }).ToPagedListAsync(dto.Page, dto.PageSize);

        }
        catch (Exception ex)
        {
            return new SqlSugarPagedList<ShowCompanyTableOfPage>();
        }
    }


    /// <summary>
    /// 添加出库单 -- 展示明细表格
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("添加出库单 -- 展示明细表格")]
    [ApiDescriptionSettings(Name = "AddNotifyShowDetail"), HttpPost]
    public async Task<SqlSugarPagedList<AddNotifyShowDetailOfPage>> AddNotifyShowDetail(AddNotifyShowDetailWhere dto)
    {
        try
        {
            return new SqlSugarPagedList<AddNotifyShowDetailOfPage>();

        }
        catch (Exception ex)
        {
            return new SqlSugarPagedList<AddNotifyShowDetailOfPage>();
        }
    }


    /// <summary>
    /// 查询入库明细中的批次（要求必须是已上传的入库明细）
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("查询入库明细中的批次（要求必须是已上传的入库明细）")]
    [ApiDescriptionSettings(Name = "AddNotifyShowDetailOfPage"), HttpPost]
    public async Task<List<GetLotNoOfUpdatedOfPage>> GetLotNoOfUpdated()
    {
        try
        {
            //var query = await _wmsImportNotifyDetailRep.AsQueryable()
            //    .Where(x=>x.IsDelete == false)
            //    .Where(x=>x.ImportExecuteFlag == "4")
            return new List<GetLotNoOfUpdatedOfPage>();

        }
        catch (Exception ex)
        {
            return new List<GetLotNoOfUpdatedOfPage>();
        }
    }

    /// <summary>
    /// 选择物料信息查询
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("选择物料信息查询")]
    [ApiDescriptionSettings(Name = "SelectMaterial"), HttpPost]
    public async Task<SqlSugarPagedList<SelectMaterialOfPage>> SelectMaterial(ShowCompanyTableWhere dto)
    {
        try
        {



            //查询字典中“物料类型”的数据
            var dicQuery = await _sysDictDataRep.AsQueryable()
                .LeftJoin<SysDictType>((x, a) => x.DictTypeId == a.Id)
                .Where((x, a) => x.Status == StatusEnum.Enable)
                .Where((x, a) => a.Status == StatusEnum.Enable)
                .Where((x, a) => a.Code == "MaterialType")  //“物料类型”的数据
                .ToListAsync();


            //查询物料单位表
            var unitQuery = await _wmsBaseUnitRep.AsQueryable()
                .Where(x => x.IsDelete == false)
                .ToListAsync();

            var query = await _wmsStockTrayRep.AsQueryable()
                .LeftJoin<WmsBaseMaterial>((x, a) => x.MaterialId == a.Id.ToString())
                .LeftJoin<WmsBaseUnit>((x, a, b) => a.MaterialUnit == b.Id)
                .Where((x, a, b) => x.IsDelete == false)
                .Where((x, a, b) => a.IsDelete == false)
                .Where((x, a, b) => x.StockQuantity > 0 && x.AbnormalStatu==0 && a.IsEmpty != true)
                .WhereIF(!string.IsNullOrEmpty(dto.WhereStr), (x, a, b) => a.MaterialCode.Contains(dto.WhereStr) || a.MaterialName.Contains(dto.WhereStr) || a.MaterialStandard.Contains(dto.WhereStr) ||
                    x.LotNo.Contains(dto.WhereStr) || a.MaterialValidityDay1.ToString().Contains(dto.WhereStr) || a.MaterialValidityDay2.ToString().Contains(dto.WhereStr) ||
                    a.MaterialValidityDay3.ToString().Contains(dto.WhereStr) || a.BoxQuantity.Contains(dto.WhereStr) || x.StockQuantity.ToString().Contains(dto.WhereStr) ||
                    dto.WhereStr == "待检验" || dto.WhereStr == "合格" || dto.WhereStr == "不合格")
                .WhereIF(dto.WareHouseId != null && dto.WareHouseId != "-1", (x, a, b) => x.WarehouseId == dto.WareHouseId)
                //.WhereIF(dto.CustomerId != null && dto.CustomerId != "-1", (x, a, b) => x.CustomerId == dto.CustomerId)
                //.WhereIF(dto.SupplierId != null && dto.SupplierId != -1, (x, a, b) => x.SupplierId == dto.SupplierId)
                //.WhereIF(dto.ManufacturerId != null && dto.ManufacturerId != -1, (x, a, b) => x.ManufacturerId == dto.ManufacturerId)
                .OrderBy((x, a, b) => x.LotNo)
                .OrderBy((x, a, b) => a.MaterialName)
                .Select((x, a, b) => new SelectMaterialOfPage
                {
                    WmsBaseMaterialId = a.Id,
                    WmsStockId = x.Id,
                    MaterialCode = a.MaterialCode,
                    MaterialName = a.MaterialName,
                    MaterialStandard = a.MaterialStandard,
                    LotNo = x.LotNo,
                    MaterialValidityDay1 = a.MaterialValidityDay1,
                    MaterialValidityDay2 = a.MaterialValidityDay2,
                    MaterialValidityDay3 = a.MaterialValidityDay3,
                    BoxQuantity = a.BoxQuantity,
                    StockQuantity = x.StockQuantity,
                    InspectionStatus = x.InspectionStatus,
                    MaterialModel = a.MaterialModel,
                    MaterialType = a.MaterialType,
                    MaterialUnitId = a.MaterialUnit,
                    ProductionDate = x.ProductionDate,
                    ValidateDay = x.ValidateDay

                }).ToPagedListAsync(dto.Page, dto.PageSize);


            //获取表格展示的数据
            var listQuery = query.Items;
            foreach (var item in listQuery)
            {
                if (!string.IsNullOrEmpty(item.MaterialValidityDay1))
                {
                    item.MaterialValidityDayStr = item.MaterialValidityDay1;
                }
                else if (!string.IsNullOrEmpty(item.MaterialValidityDay2))
                {
                    item.MaterialValidityDayStr = item.MaterialValidityDay2;
                }
                else if (!string.IsNullOrEmpty(item.MaterialValidityDay3))
                {
                    item.MaterialValidityDayStr = item.MaterialValidityDay3;
                }

                switch (item.InspectionStatus)
                {
                    case 0:
                        item.InspectionStatusStr = "待检验";
                        break;
                    case 1:
                        item.InspectionStatusStr = "合格";
                        break;
                    case 2:
                        item.InspectionStatusStr = "不合格";
                        break;
                    default:
                        item.InspectionStatusStr = "未知";
                        break;
                }


                //根据字典，获取物料类型名称
                item.MaterialTypeStr = dicQuery.Where(x => item.MaterialType.toString() == x.Value).FirstOrDefault()?.Label;

                //根据单位表，获取物料单位
                item.MaterialUnitStr = unitQuery.Where(x => item.MaterialUnitId == x.Id).FirstOrDefault()?.UnitName;
            }

            query.Items = listQuery;
            return query;

        }
        catch (Exception ex)
        {
            return new SqlSugarPagedList<SelectMaterialOfPage>();
        }
    }




    /// <summary>
    /// 增加出库单据表 和 明细表 ()
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加出库单据表 和 明细表")]
    [ApiDescriptionSettings(Name = "AddExportNotifyAndDetiail"), HttpPost, UnitOfWork]
    public async Task<long> AddExportNotifyAndDetiail(AddExportNotifyAndDetiailWhere dto)
    {
        try
        {
            List<long> dbInt = new List<long>();

            //var entity = dto.WmsExportNotifyDto.Adapt<WmsExportNotify>();


            dto.WmsExportNotifyDto.Id = SnowFlakeSingle.Instance.NextId();
            var notifyAddDb = await _wmsExportNotifyRep.InsertAsync(dto.WmsExportNotifyDto) ? dto.WmsExportNotifyDto.Id : 0;

            dbInt.Add(notifyAddDb);


            var notifyAddDetiailList = dto.WmsExportNotifyDetailDtoList;
            foreach (var notifyAddDetiail in notifyAddDetiailList)
            {
                //var entityDetiail = notifyAddDetiail.Adapt<WmsExportNotifyDetail>();
                notifyAddDetiail.Id = SnowFlakeSingle.Instance.NextId();
                notifyAddDetiail.ExportBillCode = dto.WmsExportNotifyDto.ExportBillCode;

                var notifyAddDetiailDb = await _wmsExportNotifyDetailRep.InsertAsync(notifyAddDetiail) ? notifyAddDetiail.Id : 0;
                dbInt.Add(notifyAddDetiailDb);
            }

            if (dbInt.Count <= 0) { return 0; }
            else
            {
                foreach (var i in dbInt)
                {
                    if (i <= 0) { return 0; }
                }
            }
            return 1;

        }
        catch (Exception e)
        {
            throw;
        }
    }







    /// <summary>
    /// 删除出库单据表 和  明细
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除出库单据表 和  明细")]
    [ApiDescriptionSettings(Name = "DeleteNotifyAndDetiail"), HttpPost, UnitOfWork]
    public async Task<int> DeleteNotifyAndDetiail(DeleteNotifyAndDetiailWhere dto)
    {

        try
        {


            //var queryNotify = await _wmsExportNotifyRep.AsQueryable()
            //    .Where(x => x.Id == dto.Id)
            //    .ToListAsync();

            //var sqlNotify = await _wmsExportNotifyRep.FakeDeleteAsync(queryNotify);


            var updateN = await _sqlSugarClient.Updateable<WmsExportNotify>()
                .Where(x => x.Id == dto.Id)
                .SetColumns(x => x.IsDelete == true)
                .ExecuteCommandAsync();


            //var queryDetiail = await _wmsExportNotifyDetailRep.AsQueryable()
            //    .Where(x=>x.ExportBillCode == dto.Id)
            //    .ToListAsync();

            //var sqlDetiail = await _wmsExportNotifyDetailRep.FakeDeleteAsync(queryDetiail);

            var updateD = await _sqlSugarClient.Updateable<WmsExportNotifyDetail>()
                .Where(x => x.ExportBillCode == dto.ExportBillCode)
                .SetColumns(x => x.IsDelete == true)
                .ExecuteCommandAsync();


            return 1;

        }
        catch (Exception e)
        {
            return 0;
        }


    }



    /// <summary>
    /// 根据明细表id，删除出库单据明细
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("根据明细表id，删除出库单据明细")]
    [ApiDescriptionSettings(Name = "DeleteNotifyDetiailById"), HttpPost, UnitOfWork]
    public async Task<int> DeleteNotifyDetiailById(DeleteNotifyDetiailByIdWhere dto)
    {
        try
        {
            var updateSql = await _sqlSugarClient.Updateable<WmsExportNotifyDetail>()
                .Where(x => x.Id == dto.Id)
                .SetColumns(x => x.IsDelete == true)
                .ExecuteCommandAsync();

            return 1;
        }
        catch (Exception ex)
        {
            return 0;
        }
    }


    /// <summary>
    /// 根据出库单据id， 查询出库单据明细
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("根据出库单据id， 查询出库单据明细")]
    [ApiDescriptionSettings(Name = "ShowNotifyAndDetiailByNotifyId"), HttpPost]
    public async Task<SqlSugarPagedList<ShowNotifyAndDetiailByNotifyIdOfPage>> ShowNotifyAndDetiailByNotifyId(ShowNotifyAndDetiailByNotifyIdWhere dto)
    {

        try
        {
            var query = await _wmsExportNotifyDetailRep.AsQueryable()
                .LeftJoin<WmsExportNotify>((x, a) => x.ExportBillCode == a.ExportBillCode)
                .LeftJoin<WmsBaseMaterial>((x, a, b) => b.Id == x.MaterialId)
                //.LeftJoin<WmsStock>((x,a,b,c)=>c.MaterialId == b.Id)
                .LeftJoin<WmsBaseUnit>((x, a, b, d) => d.Id == b.MaterialUnit)
                .WhereIF(dto.ExportBillCode != null && dto.ExportBillCode != "-1", (x, a, b, d) => x.ExportBillCode == dto.ExportBillCode)
                .Where((x, a, b, d) => x.IsDelete == false)
                .Where((x, a, b, d) => a.IsDelete == false)
                .OrderBy((x, a, b, d) => x.MaterialName)
                .Select((x, a, b, d) => new ShowNotifyAndDetiailByNotifyIdOfPage
                {
                    NotifyDetailId = x.Id,
                    NotifyId = a.Id,
                    //WmsStockId = c.Id,
                    MaterialId = b.Id,
                    WmsBaseUnitId = d.Id,
                    ExportBillCode = a.ExportBillCode,
                    ExportExecuteFlag = a.ExportExecuteFlag,
                    MaterialCode = x.MaterialCode,
                    MaterialName = x.MaterialName,
                    MaterialStandard = x.MaterialStandard,
                    LotNo = x.LotNo,
                    //InspectionStatus = c.InspectionStatus,
                    ProductionDate = x.ProductionDate,
                    LostDate = x.LostDate,
                    UnitName = d.UnitName,
                    ExportQuantity = x.ExportQuantity,
                    AllocateQuantity = x.AllocateQuantity,
                    FactQuantity = x.FactQuantity,
                    CompleteQuantity = x.CompleteQuantity

                }).ToPagedListAsync(dto.Page, dto.PageSize);

            //获取表格展示的数据
            var listQuery = query.Items;

            foreach (var item in listQuery)
            {
                switch (item.ExportExecuteFlag)
                {
                    case 0:
                        item.ExportExecuteFlagStr = "待执行";
                        break;
                    case 1:
                        item.ExportExecuteFlagStr = "正在分配";
                        break;
                    case 2:
                        item.ExportExecuteFlagStr = "正在执行";
                        break;
                    case 3:
                        item.ExportExecuteFlagStr = "已完成";
                        break;
                    case 4:
                        item.ExportExecuteFlagStr = "作废";
                        break;
                    case 5:
                        item.ExportExecuteFlagStr = "已上传";
                        break;
                    default:
                        item.ExportExecuteFlagStr = "待执行";
                        break;

                }

                //switch (item.InspectionStatus)
                //{
                //    case 0:
                //        item.InspectionStatusStr = "待检验";
                //        break;
                //    case 1:
                //        item.InspectionStatusStr = "合格";
                //        break;
                //    case 2:
                //        item.InspectionStatusStr = "不合格";
                //        break;
                //    default:
                //        item.InspectionStatusStr = "待检验";
                //        break;
                //}
            }
            query.Items = listQuery;
            return query;

        }
        catch (Exception e)
        {
            return new SqlSugarPagedList<ShowNotifyAndDetiailByNotifyIdOfPage>();
        }


    }


    #region 测试提交Post数据
    /// <summary>
    /// 测试提交数据（）
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("测试提交数据")]
    [ApiDescriptionSettings(Name = "TestPostSomething"), HttpPost]
    public async Task<bool> TestPostSomething(TestDto dto)
    {
        try
        {
            long a = dto.TestId;
            string b = !string.IsNullOrEmpty(dto.TestCode) ? dto.TestCode : "空";
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    #endregion

    #region 测试生成id的接口
    /// <summary>
    /// 测试生成id的接口
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("测试生成id的接口")]
    [ApiDescriptionSettings(Name = "MakeMyId"), HttpGet]
    public async Task<string> MakeMyId()
    {
        try
        {
            var id = SnowFlakeSingle.Instance.NextId();
            return id.ToString();
        }
        catch (Exception ex)
        {
            return "生成id错误";
        }
    }
    #endregion

    #endregion

    #region 新的

    #region 其他方法
    #region 公共方法


    /// <summary>
    /// 获取入出库自增的单据号/流水号
    /// </summary>
    /// <param name="inOutFlag">入库单:RKS;出库单:CKS;入库流水:RK;出库流水:CK;</param>
    /// <returns></returns>
    public async Task<string> GetImExNo(string inOutFlag)
    {
        string maxNo = "";
        //获取流水号
        if (inOutFlag == EnmuValue.InOutFlag.CK.ToString() || inOutFlag == EnmuValue.InOutFlag.RK.ToString() || inOutFlag == EnmuValue.InOutFlag.TJ.ToString() || inOutFlag == EnmuValue.InOutFlag.YS.ToString() || inOutFlag == EnmuValue.InOutFlag.PK.ToString())
        {

            maxNo = await SelectOrderMaxNo(inOutFlag);
            if (!string.IsNullOrEmpty(maxNo))
            {
                maxNo = maxNo.Substring(2);
            }
        }
        else if (inOutFlag == EnmuValue.InOutFlag.RKS.ToString() || inOutFlag == EnmuValue.InOutFlag.CKS.ToString() || inOutFlag == EnmuValue.InOutFlag.YKS.ToString() || inOutFlag == EnmuValue.InOutFlag.TJS.ToString() || inOutFlag == EnmuValue.InOutFlag.YSS.ToString() || inOutFlag == EnmuValue.InOutFlag.PKS.ToString())//获取单据号
        {
            maxNo = await SelectNotityMaxNo(inOutFlag);
            if (!string.IsNullOrEmpty(maxNo))
            {
                maxNo = maxNo.Substring(3);
            }
        }
        else if (inOutFlag == EnmuValue.InOutFlag.RKR.ToString() || inOutFlag == EnmuValue.InOutFlag.CKR.ToString() || inOutFlag == EnmuValue.InOutFlag.YKR.ToString() || inOutFlag == EnmuValue.InOutFlag.PKR.ToString() || inOutFlag == EnmuValue.InOutFlag.IF.ToString() || inOutFlag == EnmuValue.InOutFlag.YSR.ToString() || inOutFlag == EnmuValue.InOutFlag.TJR.ToString() || inOutFlag == EnmuValue.InOutFlag.PKR.ToString())
        {
            maxNo = await SelectTaskMaxNo(inOutFlag);
            if (!string.IsNullOrEmpty(maxNo))
            {
                maxNo = maxNo.Substring(3);
            }
        }
        else
        {
            return "";
        }
        //获取数据库时间八位
        string date = GetDBTime().Trim();
        string no = "";
        if (string.IsNullOrEmpty(maxNo))
        {
            no = inOutFlag + date + "000001";
        }
        else
        {
            if (maxNo.Substring(0, 8) == date)
            {
                int lastNo = Convert.ToInt32(maxNo.Substring(8, 6)) + 1;
                no = inOutFlag + date + (lastNo.ToString().PadLeft(6, '0'));
            }
            else
            {
                no = inOutFlag + date + "000001";
            }
        }
        return no;
    }
    /// <summary>
    /// 获取最大流水号
    /// </summary>
    /// <returns></returns>
    private async Task<string> SelectOrderMaxNo(string inoutFlag)
    {
        try
        {
            string orderNo = "";
            if (inoutFlag == EnmuValue.InOutFlag.CK.ToString())
            {
                var list = await _wmsExportOrderRep.AsQueryable().Where(a => a.ExportOrderNo.ToString().StartsWith("CK")).ToListAsync();
                orderNo = list.Select(a => a.ExportOrderNo).Max().ToString();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.RK.ToString())
            {
                var list = await _wmsImportOrderRep.AsQueryable().Where(a => a.ImportOrderNo.StartsWith("RK")).ToListAsync();
                orderNo = list.Select(a => a.ImportOrderNo).Max();
            }

            else if (inoutFlag == EnmuValue.InOutFlag.YS.ToString())
            {
                var list = await _wmsInspectOrderRep.AsQueryable().Where(a => a.InspectOrderNo.StartsWith("YS")).ToListAsync();
                orderNo = list.Select(a => a.InspectOrderNo).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.PK.ToString())
            {
                var list = await _wmsInspectOrderRep.AsQueryable().Where(a => a.InspectOrderNo.StartsWith("PK")).ToListAsync();
                orderNo = list.Select(a => a.InspectOrderNo).Max();
            }
            return orderNo;

        }
        catch (Exception ex)
        {

            throw new Exception("获取最大流水号" + ex.Message);
        }
    }
    /// <summary>
    /// 获取最大单据号
    /// </summary>
    /// <returns></returns>
    private async Task<string> SelectNotityMaxNo(string inoutFlag)
    {
        try
        {

            string orderNo = "";
            if (inoutFlag == EnmuValue.InOutFlag.CKS.ToString())
            {
                var list = await _wmsExportNotifyRep.AsQueryable().Where(a => a.ExportBillCode.StartsWith("CKS")).ToListAsync();
                orderNo = list.Select(a => a.ExportBillCode).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.RKS.ToString())
            {
                var list = await _wmsImportNotifyRep.AsQueryable().Where(a => a.ImportBillCode.StartsWith("RKS")).ToListAsync();
                orderNo = list.Select(a => a.ImportBillCode).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.YKS.ToString())
            {
                //var list = DataContext.WmsMoveNotify.Where(a => a.MoveBillCode.StartsWith("YKS"));
                //orderNo = list.Select(a => a.MoveBillCode).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.TJS.ToString())
            {
                //var list = await WmsExtractNotify.Where(a => a.ExtractBillCode.StartsWith("TJS"));
                //orderNo = list.Select(a => a.ExtractBillCode).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.YSS.ToString())
            {
                var list = await _wmsInspectNotityRep.AsQueryable().Where(a => a.InspectBillCode.StartsWith("YSS")).ToListAsync();
                orderNo = list.Select(a => a.InspectBillCode).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.PKS.ToString())
            {
                var list = await _wmsStockCheckNotifyRep.AsQueryable().Where(a => a.CheckBillCode.StartsWith("PKS")).ToListAsync();
                orderNo = list.Select(a => a.CheckBillCode).Max();
            }
            return orderNo;

        }
        catch (Exception ex)
        {

            return "";
        }
    }
    /// <summary>
    /// 获取最大单据号(入库任务、出库任务、移库任务、盘库任务)
    /// </summary>
    /// <returns></returns>
    private async Task<string> SelectTaskMaxNo(string inoutFlag)
    {
        try
        {

            string taskNo = "";
            if (inoutFlag == EnmuValue.InOutFlag.RKR.ToString())
            {
                var list = await _wmsImportTaskRep.AsQueryable().Where(a => a.TaskNo.StartsWith("RKR")).ToListAsync();
                taskNo = list.Select(a => a.TaskNo).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.CKR.ToString())
            {
                var list = await _wmsExportTaskRep.AsQueryable().Where(a => a.ExportTaskNo.StartsWith("CKR")).ToListAsync();
                taskNo = list.Select(a => a.ExportTaskNo).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.IF.ToString())
            {
                var list = await _wmsInterfaceLogRep.AsQueryable().Where(a => a.TaskId.StartsWith("IF")).ToListAsync();
                taskNo = list.Select(a => a.TaskId).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.TJR.ToString())
            {
                //var list = DataContext.WmsExtractTask.Where(a => a.ExtractTaskNo.StartsWith("TJR"));
                //taskNo = list.Select(a => a.ExtractTaskNo).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.YSR.ToString())
            {
                var list = await _wmsInspectTaskRep.AsQueryable().Where(a => a.InspectTaskNo.StartsWith("YSR")).ToListAsync();
                taskNo = list.Select(a => a.InspectTaskNo).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.PKR.ToString())
            {
                var list = await _wmsStockCheckTaskRep.AsQueryable().Where(a => a.CheckTaskNo.StartsWith("PKR")).ToListAsync();
                taskNo = list.Select(a => a.CheckTaskNo).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.YKR.ToString())
            {
                var list = await _wmsMoveTaskRep.AsQueryable().Where(a => a.TaskNo.StartsWith("YKR")).ToListAsync();
                taskNo = list.Select(a => a.TaskNo).Max();
            }
            return taskNo;

        }
        catch (Exception ex)
        {

            return "";
        }
    }
    /// <summary>
    /// 获取服务器时间八位yyyyMMdd
    /// </summary>
    /// <returns></returns>
    public string GetDBTime()
    {
        try
        {
            var dt = _sqlSugarClient.Ado.GetDataTable("select * from View_GetServerTime");
            //var serverTime = DataContext.View_GetServerTime.Select(a => a.ServerTime).First();
            var date = Convert.ToDateTime(dt.Rows[0][0]);
            var serverTime = date.ToString("yyyyMMdd");
            return serverTime;

        }
        catch (Exception ex)
        {

            throw new Exception("获取服务器时间八位yyyyMMdd" + ex.Message);
        }
    }
    /// <summary>
    /// 添加按钮点击记录
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="ButtonInformation">记录信息</param>
    public async Task<string> InsertButtonRecord(string userId, string pageName, string ButtonInformation)
    {
        try
        {
            _sqlSugarClient.Ado.BeginTranAsync();
            //判断用户是否为空
            if (!string.IsNullOrWhiteSpace(userId))
            {
                //查询对应的用户
                var listList = await _sysUserRep.AsQueryable().ToListAsync();
                var list = listList.FirstOrDefault(a => a.Id.ToString() == userId);
                if (list != null && list.Id != null && list.Id > 0)
                {
                    WmsButtonRecord wmsButton = new WmsButtonRecord();
                    wmsButton.Id = SnowFlakeSingle.Instance.NextId();
                    wmsButton.UserId = list.Id;
                    wmsButton.ClickButtonDate = DateTime.Now;
                    wmsButton.PageName = pageName;

                    //用户名+按钮点击时间+记录按钮信息
                    wmsButton.ButtonInformation = list.RealName + " 在 " + DateTime.Now + " " + ButtonInformation;
                    //DataContext.WmsButtonRecord.InsertOnSubmit(wmsButton);
                    //DataContext.SubmitChanges();

                    await _wmsButtonRecordRep.InsertAsync(wmsButton);

                }
            }
            _sqlSugarClient.Ado.CommitTranAsync();
            return "成功";
        }
        catch (Exception ex)
        {
            _sqlSugarClient.Ado.RollbackTranAsync();
            throw new Exception("添加按钮点击记录功能失败：" + ex.Message);
        }
    }

    #region 批量生成空托入库流水
    //public string GetOrderNo(string inOutFlag)
    //{
    //    string maxNo = "";
    //    //获取流水号
    //    if (inOutFlag == InOutFlag.CK.ToString() || inOutFlag == InOutFlag.RK.ToString() || inOutFlag == InOutFlag.TJ.ToString() || inOutFlag == InOutFlag.YS.ToString() || inOutFlag == InOutFlag.PK.ToString())
    //    {
    //        maxNo = GetOrderMaxNo(inOutFlag);
    //        if (!string.IsNullOrEmpty(maxNo))
    //        {
    //            maxNo = maxNo.Substring(2);
    //        }
    //    }
    //    else
    //    {
    //        return "";
    //    }
    //    //获取数据库时间八位
    //    string date = GetDBTime().Trim();
    //    string no = "";
    //    if (string.IsNullOrEmpty(maxNo))
    //    {
    //        no = inOutFlag + date + "000001";
    //    }
    //    else
    //    {
    //        if (maxNo.Substring(0, 8) == date)
    //        {
    //            int lastNo = Convert.ToInt32(maxNo.Substring(8, 6)) + 1;
    //            no = inOutFlag + date + (lastNo.ToString().PadLeft(6, '0'));
    //        }
    //        else
    //        {
    //            no = inOutFlag + date + "000001";
    //        }
    //    }
    //    return no;
    //}
    //private static string GetOrderMaxNo(string inoutFlag)
    //{
    //    try
    //    {
    //        using (var conn = new SqlConnection(DataConnection.GetDataConnection))
    //        {
    //            conn.Open();

    //            string sql = "";
    //            if (inoutFlag == InOutFlag.RK.ToString())
    //            {
    //                sql = @"SELECT MAX(ImportOrderNo) 
    //                FROM WmsImportOrder WITH (NOLOCK) 
    //                WHERE ImportOrderNo LIKE @Prefix + '%'";
    //            }

    //            using (var cmd = new SqlCommand(sql, conn))
    //            {
    //                cmd.Parameters.AddWithValue("@Prefix", inoutFlag);
    //                cmd.CommandTimeout = 30; // 可调大

    //                var result = cmd.ExecuteScalar();
    //                return result == DBNull.Value ? "" : (string)result;
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {

    //        throw new Exception("获取最大流水号" + ex.Message);
    //    }
    //}

    #endregion

    #endregion



    #region  sql -- 模拟试图
    /// <summary>
    /// 模拟查询视图 -- View_WmsExportNotify
    /// </summary>
    /// <returns></returns>
    public async Task<List<View_WmsExportNotify>> View_WmsExportNotify()
    {
        var list = await _wmsExportNotifyRep.AsQueryable()
                .LeftJoin<WmsBaseWareHouse>((a, b) => a.WarehouseId == b.Id)
                .LeftJoin<WmsBaseBillType>((a, b, c) => a.ExportBillType == c.BillType)
                .LeftJoin<WmsBaseBillType>((a, b, c, e) => a.DocumentSubtype.toString() == e.BillTypeCode)
                .LeftJoin<SysUser>((a, b, c, e, d) => a.CreateUserId == d.Id)
                .LeftJoin<SysUser>((a, b, c, e, d, f) => a.UpdateUserId == f.Id)
                .LeftJoin<WmsBaseCustomer>((a, b, c, e, d, f, g) => g.Id == a.ExportCustomerId && g.CustomerTypeId == "3" && g.IsDelete == false)
                .Select((a, b, c, e, d, f, g) => new View_WmsExportNotify
                {
                    Id = a.Id,
                    WarehouseName = b.WarehouseName,
                    ExportBillCode = a.ExportBillCode,
                    ExportListNo = a.ExportListNo,
                    ExportBillType = a.ExportBillType,
                    MaterialId = a.MaterialId,
                    ExportLotNo = a.ExportLotNo,
                    ExportExecuteFlag = a.ExportExecuteFlag,
                    ExportDate = a.ExportDate,
                    Expr1 = b.WarehouseName,
                    WarehouseCode = b.WarehouseCode,
                    BillTypeCode = c.BillTypeCode,
                    BillTypeName = c.BillTypeName,
                    WarehouseId = a.WarehouseId,
                    ExportQuantity = a.ExportQuantity,
                    ExportFactQuantity = a.ExportFactQuantity,
                    ExportCompleteQuantity = a.ExportCompleteQuantity,
                    ExportRemark = a.ExportRemark,
                    OuterBillCode = a.OuterBillCode,
                    OuterMainId = a.OuterMainId,
                    Source = a.Source,
                    WholeOutWare = a.WholeOutWare,
                    SortOutWare = a.SortOutWare,
                    CreateUserId = a.CreateUserId,
                    CreateTime = a.CreateTime,
                    IsDelete = a.IsDelete,
                    UpdateUserId = a.UpdateUserId,
                    UpdateTime = a.UpdateTime,
                    CreateUserName = d.CreateUserName,
                    UpdateUserName = f.CreateUserName,
                    DocumentSubtypeName = e.BillTypeName,
                    DocumentSubtype = a.DocumentSubtype,
                    WarehouseType = b.WarehouseType,
                    ExportCustomerId = a.ExportCustomerId,
                    ExportSupplierId = a.ExportSupplierId,
                    ExportDepartmentId = a.ExportDepartmentId,
                    CustomerName = g.CustomerName,
                    CustomerCode = g.CustomerCode
                }).ToListAsync();

        return list;
    }


    /// <summary>
    /// 模拟查询视图 -- View_WmsExportNotifyDetail
    /// </summary>
    /// <returns></returns>
    public async Task<List<View_WmsExportNotifyDetail>> View_WmsExportNotifyDetail()
    {
        var list = await _wmsExportNotifyDetailRep.AsQueryable()
            .InnerJoin<WmsBaseMaterial>((a, c) => c.Id == a.MaterialId)
            .LeftJoin<WmsBaseUnit>((a, c, b) => c.MaterialUnit == b.Id)
            .InnerJoin<WmsBaseWareHouse>((a, c, b, f) => f.Id == a.WarehouseId)
            .Select((a, c, b, f) => new View_WmsExportNotifyDetail
            {
                Id = a.Id,
                ExportBillCode = a.ExportBillCode,
                WarehouseId = a.WarehouseId,
                AllocateQuantity = a.AllocateQuantity,
                MaterialCode = c.MaterialCode,
                MaterialName = c.MaterialName,
                MaterialStandard = c.MaterialStandard,
                LotNo = a.LotNo,
                MaterialUnit = c.MaterialUnit,
                ExportQuantity = a.ExportQuantity,
                FactQuantity = a.FactQuantity,
                CompleteQuantity = a.CompleteQuantity,
                ProductionDate = a.ProductionDate,
                MaterialId = a.MaterialId,
                MaterialModel = c.MaterialModel,
                MaterialTemp = c.MaterialTemp,
                MaterialType = c.MaterialType,
                LostDate = a.LostDate,
                ExportDetailFlag = a.ExportDetailFlag,
                OuterDetailId = a.OuterDetailId,
                IsDelete = a.IsDelete,
                LCLRemainderQTY = a.LCLRemainderQTY,
                UnitName = b.UnitName,
                UnitCode = b.UnitCode,
                InspectionStatus = a.InspectionStatus,
                WarehouseType = f.WarehouseType
            }).ToListAsync();

        return list;
    }

    /// <summary>
    /// 模拟查询视图 -- View_WmsExportBoxInfo
    /// </summary>
    /// <returns></returns>
    public async Task<List<View_WmsExportBoxInfo>> View_WmsExportBoxInfo()
    {
        var list = await _wmsExportBoxInfoRep.AsQueryable()
            .LeftJoin<WmsBaseMaterial>((a, b) => a.MaterialId == b.Id.toString())
            .LeftJoin<WmsBaseUnit>((a, b, c) => b.MaterialUnit == c.Id)
            .Select((a, b, c) => new View_WmsExportBoxInfo
            {
                MaterialName = b.MaterialName,
                Id = a.Id,
                BoxCode = a.BoxCode,

                Qty = a.Qty,
                StockCode = a.StockCodeCode,
                ExportOrderNo = a.ExportOrderNo,

                IsOut = a.IsOut,
                MaterialId = a.MaterialId,
                LotNo = a.LotNo,
                AddDate = a.AddDate,
                Status = a.Status,
                ProductionDate = a.ProductionDate,

                ExportLoseDate = a.ExportLoseDate,
                IsDelete = a.IsDelete,
                MaterialCode = b.MaterialCode,

                PickNum = a.PickNum,
                IsMustExport = a.IsMustExport,
                MaterialStandard = b.MaterialStandard,

                UnitName = c.UnitName,
                PickEdNum = a.PickNum,
            }).ToListAsync();
        return list;
    }



    /// <summary>
    /// 模拟查询视图 -- View_WmsExportOrder
    /// </summary>
    /// <returns></returns>
    public async Task<List<View_WmsExportOrder>> View_WmsExportOrder()
    {
        var list = await _wmsExportOrderRep.AsQueryable()
            .LeftJoin<WmsBaseBillType>((a, b) => a.ExportBillType.toString() == b.BillTypeCode)
            .LeftJoin<WmsBaseMaterial>((a, b, c) => a.ExportMaterialId == c.Id)
            .LeftJoin<WmsBaseUnit>((a, b, c, d) => c.MaterialUnit == d.Id)
            .LeftJoin<WmsBaseWareHouse>((a, b, c, d, e) => a.ExportWarehouseId == e.Id)
            .Select((a, b, c, d, e) => new View_WmsExportOrder
            {
                Id = a.Id,
                ExportOrderNo = a.ExportOrderNo,
                ExportBillCode = a.ExportBillCode,
                ExportBillType = a.ExportBillType,
                ExportQuantity = a.ExportQuantity,
                ExportSlotCode = a.ExportSlotCode,
                ExportExecuteFlag = a.ExportExecuteFlag,
                ExportDate = a.ExporOrederDate,
                ExportMaterialId = a.ExportMaterialId,
                ScanQuantity = a.ScanQuantity,
                ScanUserNames = a.ScanUserNames,
                IsDelete = a.IsDelete,
                ExportStockCode = a.ExportStockCode,
                CreateTime = a.CreateTime,
                BillTypeName = b.BillTypeName,
                CompleteDate = a.CompleteDate,
                OutType = a.OutType,
                PickedNum = a.PickedNum.ToString(),
                WholeBoxNum = a.WholeBoxNum,
                ExportDetailId = a.ExportDetailId,
                ExportMaterialCode = a.ExportMaterialCode,
                MaterialName = c.MaterialName,
                UnitName = d.UnitName,
                MaterialStandard = c.MaterialStandard,
                ExportType = a.ExportType,
                //ExportTypeName =a.ExportType,
                InspectionStatus = a.InspectionStatus,
                ExportLotNo = a.ExportLotNo,
                ExportWarehouseId = a.ExportWarehouseId,
                WarehouseType = e.WarehouseType,
                WarehouseCode = e.WarehouseCode,
                WarehouseName = e.WarehouseName,
                WholeOutWare = a.WholeOutWare,
            }).ToListAsync();

        foreach (var item in list)
        {
            switch (item.ExportType)
            {
                case 0:
                    item.ExportTypeName = "正常出库";
                    break;
                case 1:
                    item.ExportTypeName = "分拣出库";
                    break;
                case 2:
                    item.ExportTypeName = "转运出库";
                    break;
                case 3:
                    item.ExportTypeName = "托盘出库";
                    break;
                case 4:
                    item.ExportTypeName = "备货出库";
                    break;
                case 5:
                    item.ExportTypeName = "托盘调整";
                    break;
                default:
                    item.ExportTypeName = "正常出库";
                    break;
            }
        }


        return list;
    }


    /// <summary>
    /// 模拟查询视图 -- View_WmsExportTask
    /// </summary>
    /// <returns></returns>
    public async Task<List<View_WmsExportTask>> View_WmsExportTask()
    {
        var list = await _wmsExportTaskRep.AsQueryable()
            .LeftJoin<WmsBaseWareHouse>((a, b) => a.WarehouseId == b.Id)
            .Select((a, b) => new View_WmsExportTask
            {
                ExportTaskNo = a.ExportTaskNo,
                Sender = a.Sender,
                Receiver = a.Receiver,
                IsSuccess = a.IsSuccess,
                Information = a.Information,
                SendDate = a.SendDate,
                BackDate = a.BackDate,
                StockCode = a.StockCode,
                Msg = a.Msg,

                ExportTaskFlag = a.ExportTaskFlag,
                CreateTime = a.CreateTime,
                CreateUserId = a.CreateUserId,
                UpdateTime = a.UpdateTime,
                UpdateUserId = a.UpdateUserId,
                ExportOrderNo = a.ExportOrderNo,
                IsDelete = a.IsDelete,
                StartLocation = a.StartLocation,

                EndLocation = a.EndLocation,
                WarehouseId = a.WarehouseId,
                Id = b.Id,
                WarehouseCode = b.WarehouseCode,
                WarehouseName = b.WarehouseName,
            }).ToListAsync();

        return list;
    }

    /// <summary>
    /// 模拟查询视图 -- View_WmsBaseMaterial
    /// </summary>
    /// <returns></returns>
    public async Task<List<View_WmsBaseMaterial>> View_WmsBaseMaterial()
    {
        var list = await _wmsBaseMaterialRep.AsQueryable()
            .LeftJoin<WmsBaseWareHouse>((a, b) => a.WarehouseId == b.Id)
            .LeftJoin<SysDictType>((a, b, c) => a.MaterialType == c.Id)
            .LeftJoin<SysDictData>((a, b, c, dict) => dict.DictTypeId == c.Id)
            .LeftJoin<SysUser>((a, b, c, dict, d) => a.CreateUserId == d.Id)
            .LeftJoin<SysUser>((a, b, c, dict, d, dd) => a.UpdateUserId == dd.Id)
            .LeftJoin<WmsBaseUnit>((a, b, c, dict, d, dd, e) => a.MaterialUnit == e.Id)
            .LeftJoin<WmsBaseMaterialArea>((a, b, c, dict, d, dd, e, f) => a.Id == long.Parse(f.BaseMaterialId))
            .Select((a, b, c, dict, d, dd, e, f) => new View_WmsBaseMaterial
            {
                Id = a.Id,
                MaterialCode = a.MaterialCode,
                MaterialName = a.MaterialName,
                MaterialMcode = a.MaterialMcode,
                MaterialStandard = a.MaterialStandard,
                MaterialModel = a.MaterialModel,
                MaterialWeight = a.MaterialWeight,
                BsaeAreaName = f.BsaeAreaName,
                MaterialType = a.MaterialType,
                MaterialTypename = dict.Label,
                MaterialValidityDay1 = a.MaterialValidityDay1,
                MaterialValidityDay2 = a.MaterialValidityDay2,
                MaterialValidityDay3 = a.MaterialValidityDay3,
                MaterialUnit = a.MaterialUnit,
                UnitName = e.UnitName,
                Remark = a.Remark,
                MaterialTemp = a.MaterialTemp,
                IsDelete = a.IsDelete,
                MaterialAreaId = a.MaterialAreaId,
                MaterialOrigin = a.MaterialOrigin,
                CreateUserId = a.CreateUserId,
                CreateTime = a.CreateTime,
                UpdateUserId = a.UpdateUserId,
                UpdateTime = a.UpdateTime,
                CreateName = d.CreateUserName,
                UpdateName = dd.UpdateUserName,
                MaterialStockHigh = a.MaterialStockHigh,
                MaterialStockLow = a.MaterialStockLow,
                WarehouseId = a.WarehouseId,
                WarehouseName = b.WarehouseName,
                MaterialAlarmDay = a.MaterialAlarmDay,
                BoxQuantity = a.BoxQuantity,
                Labeling = a.Labeling,
                EveryNumber = a.EveryNumber,
                Vehicle = a.Vehicle,
                ManageType = a.ManageType,
                OuterInnerCode = a.OuterInnerCode
            }).ToListAsync();
        return list;
    }
    #endregion


    #region 自动分配
    #region 立体库自动分配
    /// <summary>
    /// 立体库自动分配
    /// </summary>
    /// <param name="billCode"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<string> LTKZDFP(long? billCode)
    {
        try
        {



            await _sqlSugarClient.Ado.BeginTranAsync();

            try
            {
                var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
                var notifyList = await _wmsExportNotifyRep.AsQueryable().Where(n => n.Id == billCode).ToListAsync();
                var notify = notifyList.FirstOrDefault();
                if (notify == null)
                {
                    return "操作失败";
                }
                if (notify.ExportExecuteFlag != 0)
                {
                    return "操作失败，出库单不允许分配！";
                }
                var detailList = await _wmsExportNotifyDetailRep.AsQueryable().Where(d => d.ExportBillCode == notify.ExportBillCode && d.ExportDetailFlag == 0 && d.IsDelete == false).ToListAsync();
                if (detailList == null || detailList.Count() < 1)
                {
                    return "操作失败，未找到指定出库单！";
                }
                //取订单相关物料库存
                var mIds = detailList.Select(d => d.MaterialId).ToList();
                var view_WmsBaseMaterial = await View_WmsBaseMaterial();
                var mList = view_WmsBaseMaterial.Where(p => mIds.Contains(p.Id)).ToList();
                ////排序：深位
                //var stockList = from p in DataContext.WmsStockTray
                //                join s in DataContext.WmsBaseSlot on p.StockSlotCode equals s.SlotCode into tmp
                //                from ss in tmp.DefaultIfEmpty()
                //                where mIds.Contains(p.MaterialId) && (p.StockQuantity > 0 || p.LockQuantity > 0) && p.WarehouseId == notify.WarehouseId && p.AbnormalStatu == 0 && ss.SlotStatus == 1//添加库位是否"有物料"状态
                //                orderby ss.SlotInout, p.StockQuantity
                //                select p;

                var stockList = await _wmsStockTrayRep.AsQueryable()
                    .LeftJoin<WmsBaseSlot>((p, s) => p.StockSlotCode == s.SlotCode)
                    .Where((p, s) => mIds.Contains(long.Parse(p.MaterialId)))
                    .Where((p, s) => p.StockQuantity > 0)// || p.LockQuantity > 0)
                    .Where((p, s) => p.WarehouseId == notify.WarehouseId.toString())
                    .Where((p, s) => p.AbnormalStatu == 0)
                    .Where((p, s) => s.SlotStatus == 1)
                    .OrderBy((p, s) => s.SlotInout)
                    .OrderBy((p, s) => p.StockQuantity)
                    .ToListAsync();

                #region 代储
                var billTypeList = await _wmsBaseBillTypeRep.AsQueryable().ToListAsync();
                var billType = billTypeList.FirstOrDefault(a => a.Id == notify.ExportBillType);
                if (billType != null)
                {
                    if (billType.ProxyStatus == 1)//判断当前单据类型是否为代储
                    {
                        if (notify.ExportCustomerId != null && notify.ExportCustomerId != -1)//判断客户是否为空
                        {
                            stockList = stockList.Where(s => s.CustomerId == notify.ExportCustomerId.toString()).ToList();
                        }
                        else
                        {
                            return "当前" + notify.ExportCustomerId + "库存信息中为空";
                        }
                    }

                }
                else
                {
                    return "出库单据信息获取失败";
                }
                #endregion

                var tIds = stockList.Select(s => s.Id).ToList();

                var boxList = await _wmsStockTrayRep.AsQueryable().Where(a => tIds.Contains(a.Id)).ToListAsync();

                if (boxList == null || boxList.Count() < 1)
                {
                    return "操作失败，物料库存不足！";
                }
                List<WmsStockTray> stocks = null;
                List<WmsExportOrder> exportOrderList = new List<WmsExportOrder>();
                //
                foreach (var detail in detailList)
                {
                    decimal numCounts = 0;  //已分配的总数量
                    var needQty = detail.ExportQuantity - detail.AllocateQuantity;
                    if (detail.AllocateQuantity >= detail.ExportQuantity)
                    {
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(detail.LotNo))
                    {
                        stocks = stockList.Where(s => s.MaterialId == detail.MaterialId.ToString() && s.LotNo == detail.LotNo && s.InspectionStatus.ToString() == detail.InspectionStatus && s.WarehouseId == detail.WarehouseId.ToString()).ToList();
                    }
                    else
                    {
                        stocks = await _wmsStockTrayRep.AsQueryable().Where(s => s.MaterialId == detail.MaterialId.ToString() && s.InspectionStatus.ToString() == detail.InspectionStatus && s.WarehouseId == detail.WarehouseId.toString()).ToListAsync();
                    }

                    if (stocks == null || stocks.Count < 1)
                    {
                        return "操作失败，无可出库库存！";
                    }
                    var materials = mList.Where(p => p.Id == detail.MaterialId).FirstOrDefault();
                    if (materials == null)
                    {
                        return "操作失败，物料信息未找到！";
                    }
                    if (Convert.ToDecimal(materials.BoxQuantity) == 0)
                    {
                        return "操作失败，物料数量为0，请维护！";
                    }
                    //stocks = stocks.OrderBy(p => p.StockQuantity).ToList();
                    var stockTary = GetDistributeLTK(stocks, (decimal)needQty);

                    foreach (var s in stockTary)
                    {
                        //var s = stocks.Where(p => p.Id == sc.Key).FirstOrDefault();
                        //添加流水单
                        string orderCode;
                        if (exportOrderList.Count < 1)
                        {
                            orderCode = await GetImExNo("CK");
                        }
                        else
                        {
                            var lastOrder = exportOrderList.LastOrDefault();
                            var num = Convert.ToInt64(lastOrder.ExportOrderNo);
                            num++;
                            orderCode = $"CK{num}";
                        }
                        //查询对应的箱码信息
                        var boxs = boxList.Where(a => a.Id == s.Id).ToList();
                        if (boxs == null || boxs.Count() < 1)
                        {
                            return "操作失败，物料对应箱码不存在！";
                        }
                        decimal boxcount = 0;
                        foreach (var b in boxs.OrderBy(a => a.StockQuantity))
                        {
                            WmsExportBoxInfo info = new WmsExportBoxInfo();
                            info.Id = SnowFlakeSingle.Instance.NextId();
                            info.BoxCode = b.StockCode;
                            info.Qty = (decimal)b.StockQuantity;
                            info.StockCodeCode = s.StockCode;
                            info.ExportOrderNo = orderCode;
                            info.IsOut = 0;
                            info.MaterialId = materials.Id.ToString();
                            info.LotNo = b.LotNo;
                            info.AddDate = DateTime.Now;
                            info.IsDelete = false;
                            info.Status = 0;
                            info.ProductionDate = b.ProductionDate;
                            info.ExportLoseDate = (DateTime)b.ValidateDay;
                            info.IsMustExport = 1;
                            decimal newQty = 0;
                            newQty = (decimal)needQty - numCounts - boxcount;//剩余需要出库数量=出库数量-已分配总数量-已分配箱总数量
                            if (newQty < b.StockQuantity) //如果剩余需要出库数量小于箱数量
                            {
                                info.PickNum = newQty;
                            }
                            else
                            {
                                info.PickNum = b.StockQuantity;
                            }

                            boxcount += (decimal)info.PickNum;
                            info.PickNum = 0;

                            //DataContext.WmsExportBoxInfo.InsertOnSubmit(info);
                            await _wmsExportBoxInfoRep.InsertAsync(info);
                        }
                        WmsExportOrder order = new WmsExportOrder();
                        order.Id = SnowFlakeSingle.Instance.NextId();
                        order.ExportOrderNo = orderCode;
                        order.ExportBillType = notify.ExportBillType;
                        order.ExportWarehouseId = notify.WarehouseId;
                        order.ExportSlotCode = s.StockSlotCode;
                        order.ExportStockCode = s.StockCode;
                        order.ExportQuantity = boxcount;
                        order.ExportWeight = "0";
                        order.ExportProductionDate = detail.ProductionDate;
                        order.ExportLoseDate = detail.LostDate;
                        order.ExportDepartmentCode = notify.ExportDepartmentId;
                        order.ExpotSupplierCode = notify.ExportSupplierId;
                        order.ExportCustomerCode = notify.ExportCustomerId;
                        order.ExportTaskNo = "";
                        order.ExportExecuteFlag = 0;
                        order.ExporOrederDate = DateTime.Now;
                        order.ExportRemark = "";
                        order.ExportLotNo = s.LotNo;
                        order.ExportBillCode = notify.ExportBillCode;
                        order.ExportAreaId = long.Parse(s.LanewayId);
                        order.IsDelete = false;
                        order.ExportMaterialId = materials.Id;
                        order.ExportMaterialCode = detail.MaterialCode;
                        order.ExportMaterialName = detail.MaterialName;
                        order.ExportMaterialStandard = detail.MaterialStandard;
                        order.ExportMaterialUnit = detail.MaterialUnit;
                        order.CreateUserId = userId;
                        order.ExportDetailId = detail.Id;
                        order.PickedNum = boxcount;
                        order.CreateTime = DateTime.Now;
                        order.StockWholeBoxNum = boxs.Count().toString();
                        order.StockQuantity = s.StockQuantity.ToString();
                        order.ExportType = 0;
                        order.InspectionStatus = s.InspectionStatus;

                        exportOrderList.Add(order);
                        //查询托盘表，给变量赋值
                        var StockTrayModelList = await _wmsStockTrayRep.AsQueryable().Where(a => a.Id == s.Id).ToListAsync();
                        var StockTrayModel = StockTrayModelList.FirstOrDefault();
                        StockTrayModel.StockQuantity -= boxcount;  //库存数量
                        StockTrayModel.LockQuantity += boxcount;   //锁定数量

                        await _wmsStockTrayRep.UpdateAsync(StockTrayModel);

                        numCounts += boxcount;//已分配的总数量

                        detail.AllocateQuantity += boxcount;   //分配数量
                        detail.ExportDetailFlag = 1;
                        await _wmsExportNotifyDetailRep.UpdateAsync(detail);
                        //数量大于等于出库数量
                        if (detail.AllocateQuantity >= detail.ExportQuantity || (detail.ExportQuantity - detail.AllocateQuantity) < 1)
                        {
                            break;
                        }
                    }
                }
                foreach (var o in exportOrderList)
                {
                    await _wmsExportOrderRep.InsertAsync(o);
                }
                notify.ExportExecuteFlag = 1;
                notify.UpdateUserId = userId;
                notify.UpdateTime = DateTime.Now;
                await _wmsExportNotifyRep.UpdateAsync(notify);

                await _sqlSugarClient.Ado.CommitTranAsync();
            }
            catch (Exception ex)
            {
                await _sqlSugarClient.Ado.RollbackTranAsync();
                return "分配失败";
            }
            return "1";

        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }



    /// <summary>
    /// 自动分配方法(立体库) 条件：同物料同批次同质检状态（1：优先找库外的托盘；2：找正好托；3：其次找托盘数量最少的物料；4：找有物料的托盘数量）不考虑单伸还是双伸
    /// </summary>
    /// <param name="wareHouseType"></param>
    /// <returns></returns>
    public List<WmsStockTray> GetDistributeLTK(List<WmsStockTray> stockTray, decimal needQty)
    {
        var stockTrayList = new List<WmsStockTray>();
        var stockTrays = new WmsStockTray();
        ////找在库外的托盘并且要大于或等于出库数量(只获取第一条)
        //stockTrays = stockTray.FirstOrDefault(a => (a.StockSlotCode == "" || a.StockSlotCode == string.Empty || a.StockSlotCode == null) && a.StockQuantity >= needQty);
        //if (stockTrays != null)
        //{
        //    stockTrayList.Add(stockTrays);
        //    return stockTrayList;
        //}
        //stockTrays = new WmsStockTray();
        //找正好托
        stockTrays = stockTray.FirstOrDefault(a => a.StockQuantity == needQty);
        if (stockTrays != null)
        {
            stockTrayList.Add(stockTrays);
            return stockTrayList;
        }
        //找到最少的托盘数量(如果出库数量大于最少的托盘数量继续找)
        var stockTrayLists = stockTray.OrderBy(a => a.StockQuantity).ToList();
        foreach (var item in stockTrayLists)
        {
            if (stockTrayList.Sum(a => a.StockQuantity) >= needQty)
            {
                break;
            }
            stockTrayList.Add(item);
        }
        return stockTrayList;
    }


    #endregion

    #region 平库自动分配
    /// <summary>
    /// 自动化平库自动分配逻辑
    /// </summary>
    /// <param name="detailList"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<string> PKZDFP(long? billCode)
    {
        try
        {
            var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
            var notifyList = await _wmsExportNotifyRep.AsQueryable().Where(n => n.Id == billCode).ToListAsync();
            var notify = notifyList.FirstOrDefault();
            if (notify == null)
            {
                return "操作失败，未找到指定出库单！";
            }
            if (notify.ExportExecuteFlag != 0)
            {
                return "操作失败，出库单不允许分配！";
            }
            var detailList = await _wmsExportNotifyDetailRep.AsQueryable().Where(d => d.ExportBillCode.ToString() == notify.ExportBillCode && d.ExportDetailFlag == 0 && d.IsDelete == false).ToListAsync();
            if (detailList == null || detailList.Count() < 1)
            {
                return "操作失败，未找到指定出库单！";
            }
            var billTypeList = await _wmsBaseBillTypeRep.AsQueryable().Where(a => a.BillTypeCode == notify.ExportBillType.toString()).ToListAsync();
            var billType = billTypeList.FirstOrDefault();
            if (billType == null)
            {
                return "操作失败，未找到指定单据类型！";
            }
            //取订单相关物料库存
            var gIds = detailList.Select(d => d.MaterialId).ToList();

            List<WmsExportOrder> exportOrderList = new List<WmsExportOrder>();
            List<WmsExportOrder> exportOrderList1 = new List<WmsExportOrder>();

            string sqlString = string.Empty;

           await  _sqlSugarClient.Ado.BeginTranAsync();

            try
            {
                string orderCode = "";
                foreach (var detail in detailList)
                {
                    List<WmsStockTray> stockTrayList = new List<WmsStockTray>();
                    decimal numCounts = 0;  //已分配的总数量
                    var needQty = detail.ExportQuantity - detail.AllocateQuantity;//计划出库数量-分配数量
                    if (detail.AllocateQuantity >= detail.ExportQuantity || (detail.ExportQuantity - detail.AllocateQuantity) < 1)
                    {
                        continue;
                    }
                    var stocks = new List<WmsStockTray>();
                    //根据物料id，批次，质检状态查询
                    if (!string.IsNullOrWhiteSpace(detail.InspectionStatus))
                    {
                        stocks = await _wmsStockTrayRep.AsQueryable().Where(s => s.MaterialId == detail.MaterialId.ToString() && s.LotNo == detail.LotNo && s.InspectionStatus.ToString() == detail.InspectionStatus).ToListAsync();
                    }
                    else
                    {
                        stocks = await _wmsStockTrayRep.AsQueryable().Where(s => s.MaterialId == detail.MaterialId.ToString() && s.LotNo == detail.LotNo).ToListAsync();
                    }
                    if (stocks == null || stocks.Count < 1)
                    {
                        return "操作失败，无可出库库存！";
                    }
                    //List<WmsStockTray> stocks = new List<WmsStockTray>();
                    //foreach (var item in ss)//添加库位是否"有物料"状态
                    //{
                    //    var slotList = await _wmsBaseSlotRep.AsQueryable().Where(a => a.SlotCode == item.StockSlotCode).ToListAsync();
                    //    var slot = slotList.FirstOrDefault();
                    //    if (slot.SlotStatus == 1)
                    //    {
                    //        if (string.IsNullOrEmpty(item.StockSlotCode))
                    //        {
                    //            item.StockSlotCode = "B10000000000";
                    //        }
                    //        stocks.Add(item);
                    //    }
                    //}
                    //if (stocks == null || stocks.Count < 1)
                    //{
                    //    return "操作失败，无可出库库存！";
                    //}
                    #region 代储
                    //单据类型中，代储状态为是，根据物料id、批次、质检状态、客户去查询
                    if (billType.ProxyStatus == 1)//判断当前单据类型是否为代储
                    {
                        if (notify.ExportCustomerId != null && notify.ExportCustomerId != -1)//判断客户是否为空
                        {
                            stocks = stocks.Where(s => s.CustomerId == notify.ExportCustomerId.toString()).ToList();
                        }
                        else
                        {
                            return "当前" + notify.ExportCustomerId + "库存信息中为空";
                        }
                    }

                    #endregion

                    var stockLanewayId = stocks.Select(s => s.Id).ToList();//获取当前托盘所有的巷道Id

                    //获取到所有的巷道数量(升序排序)只找有物料
                    //var slots = await _wmsBaseSlotRep.AsQueryable().Where(a => stockLanewayId.Contains(a.Id) && a.SlotStatus == 1).GroupBy(a => a.SlotLanewayId).Select(a => new { SlotLanewayId = a.SlotLanewayId}).OrderBy(a => Convert.ToInt32(a.SlotLanewayId)).ToListAsync();
                    string idsString = "'" + string.Join("','", stockLanewayId) + "'";
                    string sqlString2 = @" select a.LanewayId  SlotLanewayId,COUNT(a.LanewayId) as Count,SUM(a.StockQuantity) StockQuantity
                    from [WmsStockTray] a left join WmsBaseSlot b  where  Id in (" + idsString + ") and b.SlotStatus=1 group by a.LanewayId order by COUNT(a.LanewayId),a.LanewayId";
                    var slots = _sqlSugarClient.Ado.SqlQuery<sqlStringDto>(sqlString2);
                    //var slots = DataContext.ExecuteQuery<StockTrayLS>(sqlString2).ToList();
                    //detail.ExportQuantity;

                    var slots1 = slots.Where(p => Convert.ToDecimal(p.StockQuantity) == detail.ExportQuantity);//判断是否有巷道有数量合适的巷道，如果有的话直接选中巷道


                    foreach (var soltItem in slots)
                    {
                        //获取当前巷道的托盘并按托盘数量进行升序排序（先出库存数量少的托盘）
                        var stockTarys = stocks.Where(a => a.LanewayId == soltItem.SlotLanewayId.ToString()  && !string.IsNullOrEmpty(a.StockSlotCode)).OrderBy(a => a.StockQuantity).ThenBy(a => a.StockSlotCode.Substring(a.StockSlotCode.Length - 1)).ToList();
                        if (slots1 != null && slots1.Count() > 0)
                        {
                            stockTarys = stocks.Where(p => p.LanewayId == slots1.FirstOrDefault().SlotLanewayId).ToList();
                        }
                        if (stockTarys.Count() > 0)
                        {
                            foreach (var taryItem in stockTarys)
                            {
                                //判断库存数量大于出库数量
                                if (taryItem.StockQuantity >= needQty)
                                {
                                    //大于出库数量结束循环
                                    stockTrayList.Add(taryItem);
                                    break;
                                }
                                else
                                {
                                    stockTrayList.Add(taryItem);
                                }

                            }
                        }
                        //当储位循环的库存数量大于出库数量结束循环
                        if (stockTrayList.Sum(a => a.StockQuantity) >= needQty)
                        {
                            break;
                        }
                    }

                    foreach (var s in stockTrayList)
                    {
                        decimal xiangshu = 1;
                        //添加流水单
                        if (string.IsNullOrEmpty(orderCode))
                        {
                            orderCode = await GetImExNo("CK");
                        }
                        else
                        {
                            var num = Convert.ToInt64(orderCode.Substring(2));
                            num++;
                            orderCode = $"CK{num}";
                        }
                        //查询对应的箱码信息
                        var boxs = await _wmsStockInfoRep.AsQueryable().Where(a => a.TrayId == s.Id.ToString()).ToListAsync();
                        if (boxs == null || boxs.Count() < 1)
                        {
                            return "操作失败，物料对应箱码不存在！";
                        }
                        decimal boxcount = 0;
                        foreach (var b in boxs.OrderBy(a => a.Qty))
                        {
                            WmsExportBoxInfo info = new WmsExportBoxInfo()
                            {
                                Id = SnowFlakeSingle.Instance.NextId(),
                                BoxCode = b.BoxCode,
                                Qty = b.Qty,
                                StockCodeCode = s.StockCode,
                                ExportOrderNo = orderCode,
                                IsOut = 0,
                                MaterialId = b.MaterialId,
                                LotNo = b.LotNo,
                                AddDate = DateTime.Now,
                                IsDelete = false,
                                Status = 0,
                                ProductionDate = b.ProductionDate,
                                ExportLoseDate = b.ValidateDay,
                                //IsMustExport = 1,
                            };
                            decimal newQty = 0;
                            newQty = (decimal)needQty - numCounts - boxcount;//剩余需要出库数量=出库数量-已分配总数量-已分配箱总数量
                            if (newQty < b.Qty) //如果剩余需要出库数量小于箱数量
                            {
                                info.PickNum = newQty;
                            }
                            else
                            {
                                info.PickNum = b.Qty;
                            }

                            boxcount += (decimal)info.PickNum;
                            info.PickNum = null;
                            //DataContext.WmsExportBoxInfo.InsertOnSubmit(info);

                            await _wmsExportBoxInfoRep.InsertAsync(info);
                        }
                        //箱数量，根据托盘出库数量（boxcount）/库存物料箱数量（该字段待加入之后，把100改）
                        xiangshu = boxcount / (decimal)s.BoxQuantity;

                        WmsExportOrder order = new WmsExportOrder
                        {
                            Id = SnowFlakeSingle.Instance.NextId(),
                            ExportOrderNo = orderCode,
                            ExportBillType = notify.ExportBillType,
                            ExportWarehouseId = notify.WarehouseId,
                            ExportSlotCode = s.StockSlotCode,
                            ExportStockCode = s.StockCode,
                            ExportQuantity = boxcount,
                            ExportWeight = "0",
                            ExportProductionDate = detail.ProductionDate,
                            ExportLoseDate = detail.LostDate,
                            ExportDepartmentCode = notify.ExportDepartmentId,
                            ExpotSupplierCode = notify.ExportSupplierId,
                            ExportCustomerCode = notify.ExportCustomerId,
                            ExportTaskNo = "",
                            ExportExecuteFlag = 0,
                            ExporOrederDate = DateTime.Now,
                            ExportRemark = "",
                            ExportLotNo = s.LotNo,
                            ExportBillCode = notify.ExportBillCode,
                            ExportAreaId = long.Parse(s.LanewayId),
                            IsDelete = false,
                            ExportMaterialId = detail.MaterialId,
                            ExportMaterialCode = detail.MaterialCode,
                            ExportMaterialName = detail.MaterialName,
                            ExportMaterialStandard = detail.MaterialStandard,
                            ExportMaterialUnit = detail.MaterialUnit,
                            CreateUserId = userId,
                            ExportDetailId = detail.Id,
                            PickedNum = boxcount,
                            CreateTime = DateTime.Now,
                            StockWholeBoxNum = boxs.Count().ToString(),
                            StockQuantity = s.StockQuantity.ToString(),
                            ExportType = 0,
                            InspectionStatus = s.InspectionStatus,
                            OrderByStatus = 0
                        };
                        //DataContext.WmsExportOrder.InsertOnSubmit(order);
                        await _wmsExportOrderRep.InsertAsync(order);
                        //查询托盘表，给变量赋值
                        var StockTrayModelList = await _wmsStockTrayRep.AsQueryable().Where(a => a.StockSlotCode == order.ExportSlotCode && a.MaterialId == order.ExportMaterialId.ToString() && a.LotNo == order.ExportLotNo).ToListAsync();
                        var StockTrayModel = StockTrayModelList.FirstOrDefault();
                        StockTrayModel.StockQuantity -= boxcount;  //库存数量
                        if (string.IsNullOrWhiteSpace(StockTrayModel.LockQuantity.ToString()))
                        {
                            StockTrayModel.LockQuantity = 0;
                        }
                        StockTrayModel.LockQuantity = StockTrayModel.LockQuantity + boxcount;   //锁定数量

                        await _wmsStockTrayRep.UpdateAsync(StockTrayModel);

                        numCounts += boxcount;//已分配的总数量

                        detail.AllocateQuantity += boxcount;   //分配数量

                        detail.ExportDetailFlag = 1;
                        //DataContext.SubmitChanges();
                        //数量大于等于出库数量

                        await _wmsExportNotifyDetailRep.UpdateAsync(detail);
                        if (detail.AllocateQuantity >= detail.ExportQuantity || (detail.ExportQuantity - detail.AllocateQuantity) < 1)
                        {
                            break;
                        }
                    }

                    //foreach (var o in exportOrderList)
                    //{
                    //  
                    //    // DataContext.SubmitChanges();
                    //}


                    notify.ExportExecuteFlag = 1;
                    notify.UpdateUserId = userId;
                    notify.UpdateTime = DateTime.Now;
                    await _wmsExportNotifyRep.UpdateAsync(notify);

                }

                await _sqlSugarClient.Ado.CommitTranAsync();
            }
            catch (Exception ex)
            {
                await _sqlSugarClient.Ado.RollbackTranAsync();
                return ex.Message;
            }

            return "1";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
    #endregion

    #endregion


    #region 关单和作废
    /// <summary>
    /// 关单
    /// </summary>
    /// <param name="exportId"></param>
    /// <returns></returns>
    public async Task<int> TryFinishExportNotify(string exportId, string userId)
    {
        try
        {
            _sqlSugarClient.Ado.BeginTranAsync();
            var notifyList = await _wmsExportNotifyRep.AsQueryable().Where(n => n.ExportBillCode == exportId).ToListAsync();
            var notify = notifyList.FirstOrDefault();
            if (notify == null)
            {
                return 1;
            }
            if (notify.ExportExecuteFlag == 0 || notify.ExportExecuteFlag == 1)
            {
                return 5;
            }
            if (notify.ExportExecuteFlag == 2)
            {
                //关单还原未分配的库存数量
                var details = await _wmsExportNotifyDetailRep.AsQueryable().Where(d => d.ExportBillCode.toString() == exportId && d.IsDelete == false && d.ExportDetailFlag == 0).ToListAsync();
                var gIds = details.Select(d => d.MaterialId).ToList();
                var lotNo = details.Select(d => d.LotNo).ToList();
                var stocks = await _wmsStockRep.AsQueryable().Where(s => gIds.Contains(s.MaterialId) && lotNo.Contains(s.LotNo)).ToListAsync();
                foreach (var d in details)
                {
                    d.IsDelete = true;
                    //总库存
                    var sq = stocks.Where(s => s.MaterialId == d.MaterialId && s.WarehouseId == d.WarehouseId && s.InspectionStatus.ToString() == d.InspectionStatus);
                    if (!string.IsNullOrWhiteSpace(d.LotNo))
                    {
                        sq = sq.Where(s => s.LotNo == d.LotNo);
                    }
                    else
                    {
                        sq = sq.Where(s => string.IsNullOrWhiteSpace(s.LotNo));
                    }
                    var fd = sq.FirstOrDefault();
                    fd.LockQuantity -= d.ExportQuantity;
                    fd.StockQuantity += d.ExportQuantity;
                }
                //DataContext.SubmitChanges();
                //return 6;
            }
            if (notify.ExportExecuteFlag == 5)
            {
                return 3;
            }
            if (notify.ExportExecuteFlag == 4)
            {
                return 4;
            }
            notify.ExportExecuteFlag = 5;
            notify.UpdateUserId = long.Parse(userId);
            notify.UpdateTime = DateTime.Now;
            await InsertButtonRecord(userId, "出库单据", notify.ExportBillCode + "关单成功");
            //DataContext.SubmitChanges();
            _sqlSugarClient.Ado.CommitTranAsync();
            return 0;

        }
        catch (Exception ex)
        {
            _sqlSugarClient.Ado.RollbackTranAsync();
            throw new Exception("关单失败" + ex.Message);
        }
    }


    /// <summary>
    /// 作废
    /// </summary>
    /// <param name="exportId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<int> CancelExportNotify(string exportId, string userId)
    {
        try
        {
            _sqlSugarClient.Ado.BeginTranAsync();
            var notifyList = await _wmsExportNotifyRep.AsQueryable().Where(n => n.ExportBillCode == exportId).ToListAsync();
            var notify = notifyList.FirstOrDefault();
            if (notify == null || notify.ExportExecuteFlag == 1)
            {
                return 1;
            }
            if (notify.ExportExecuteFlag == 2)
            {
                return 6;
            }
            if (notify.ExportExecuteFlag == 3)
            {
                return 2;
            }
            if (notify.ExportExecuteFlag == 5)
            {
                return 3;
            }
            if (notify.ExportExecuteFlag == 4)
            {
                return 4;
            }
            //流水
            //var orderList = from n in DataContext.WmsExportNotify
            //                join o in DataContext.WmsExportOrder on n.ExportBillCode equals o.ExportBillCode
            //                where n.ExportBillCode == exportId && o.ExportExecuteFlag != 4
            //                select o;


            var orderList = await _wmsExportOrderRep.AsQueryable()
                .LeftJoin<WmsExportNotify>((o, n) => n.ExportBillCode == o.ExportBillCode.ToString())
                .Where((o, n) => n.ExportBillCode == exportId)
                .Where((o, n) => o.ExportExecuteFlag != 4)
                .ToListAsync();


            if (orderList.Where(o => o.ExportExecuteFlag != 0).Count() > 0)
            {
                return 6;
            }
            foreach (var o in orderList)
            {
                o.ExportExecuteFlag = 4;
            }
            notify.ExportExecuteFlag = 4;
            notify.UpdateUserId = long.Parse(userId);
            notify.UpdateTime = DateTime.Now;
            var details = await _wmsExportNotifyDetailRep.AsQueryable().Where(d => d.ExportBillCode.ToString() == exportId && d.IsDelete == false).ToListAsync();
            var gIds = details.Select(d => d.MaterialId).ToList();
            var stocks = await _wmsStockRep.AsQueryable().Where(s => gIds.Contains(s.MaterialId)).ToListAsync();
            foreach (var d in details)
            {
                d.IsDelete = true;
                //总库存
                var sq = stocks.Where(s => s.MaterialId == d.MaterialId && s.WarehouseId == d.WarehouseId && s.InspectionStatus.ToString() == d.InspectionStatus);
                if (!string.IsNullOrWhiteSpace(d.LotNo))
                {
                    sq = sq.Where(s => s.LotNo == d.LotNo);
                }
                else
                {
                    sq = sq.Where(s => string.IsNullOrWhiteSpace(s.LotNo));
                }
                var fd = sq.FirstOrDefault();
                fd.LockQuantity -= d.ExportQuantity;
                fd.StockQuantity += d.ExportQuantity;
            }
            await InsertButtonRecord(userId, "出库单据", notify.ExportBillCode + "作废成功");
            //DataContext.SubmitChanges();
            _sqlSugarClient.Ado.CommitTranAsync();
            return 0;

        }
        catch (Exception ex)
        {
            _sqlSugarClient.Ado.RollbackTranAsync();
            throw new Exception("出库单据取消" + ex.Message);
        }
    }


    #endregion





    #endregion


    #region 出库单据    
    /// <summary>
    /// 出库单据主数据
    /// </summary>
    /// <returns></returns>
    [DisplayName("出库单据主数据")]
    [ApiDescriptionSettings(Name = "GetWmsExportNotifyList"), HttpPost, UnitOfWork]
    public async Task<SqlSugarPagedList<View_WmsExportNotify>> GetWmsExportNotifyList(ExportModel model)
    {
        try
        {
            var view_WmsExportNotify = await View_WmsExportNotify();
            var list = view_WmsExportNotify.Where(a => a.IsDelete == false).ToList();




            if (list.Count() > 0)
            {
                //SysDal dal = new SysDal();
                //根据用户ID获取当前用户的权限
                //var ware = dal.GetRoleWarehouse(model.UserId).Select(a => a.WarehouseCode).Distinct().ToList();
                //if (ware.Count() > 0)
                //{
                //    list = list.Where(a => ware.Contains(a.WarehouseCode)).ToList();
                //    if (list.Count() > 0)
                //    {
                //        var bill = dal.GetRoleBillType(model.UserId, "2").Select(a => a.BillTypeCode).ToList();//1是入库2是出库
                //        if (bill.Count() > 0)
                //        {
                //            list = list.Where(a => bill.Contains(a.BillTypeCode)).ToList();
                //        }
                //        else
                //        {
                //            throw new Exception("单据权限获取失败！");
                //        }
                //    }
                //}
                //else
                //{
                //    throw new Exception("仓库权限获取失败！");
                //}
            }
            if (!string.IsNullOrWhiteSpace(model.WarehouseName))
            {
                list = list.Where(n => n.WarehouseCode == model.WarehouseName.Trim()).ToList();
            }

            if (!string.IsNullOrWhiteSpace(model.BillCode))//单号
            {
                list = list.Where(n => n.ExportBillCode.Contains(model.BillCode.Trim())).ToList();
            }
            if (!string.IsNullOrWhiteSpace(model.ExecuteFlag))//执行状态
            {
                list = list.Where(n => n.ExportExecuteFlag.toString() == model.ExecuteFlag).ToList();
            }
            else
            {
                list = list.Where(n => n.ExportExecuteFlag.toString() != "3" && n.ExportExecuteFlag.toString() != "4" && n.ExportExecuteFlag.toString() != "5").ToList();//写死了
            }
            if (model.StartTime.HasValue)//开始时间
            {
                list = list.Where(n => n.CreateTime >= model.StartTime).ToList();
            }
            if (model.EndTime.HasValue)//结束时间
            {
                list = list.Where(n => n.CreateTime < model.EndTime.Value.AddDays(1)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(model.BillType))//单据类型
            {
                list = list.Where(n => n.ExportBillType.toString() == model.BillType).ToList();
            }

            return list.OrderBy(a => a.ExportBillCode).ToPagedList(model.Page, model.PageSize);
        }
        catch (Exception ex)
        {
            throw;
        }
    }





    /// <summary>
    /// 获取出库单据明细数据
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取出库单据明细数据")]
    [ApiDescriptionSettings(Name = "GetWmsExportNotifyDetailList"), HttpPost, UnitOfWork]
    public async Task<SqlSugarPagedList<View_WmsExportNotifyDetail>> GetWmsExportNotifyDetailList(GetWmsExportNotifyDetailListWhere ExportBillCode)
    {
        try
        {
            var view_WmsExportNotifyDetail = await View_WmsExportNotifyDetail();
            var list = view_WmsExportNotifyDetail.Where(a => a.ExportBillCode == ExportBillCode.BillCode && a.IsDelete == false).ToPagedList(ExportBillCode.Page, ExportBillCode.PageSize);
            return list;
        }
        catch (Exception ex)
        {
            throw;
        }
    }




    /// <summary>
    /// 获取物料信息
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取物料信息")]
    [ApiDescriptionSettings(Name = "GetStockSlotList"), HttpPost, UnitOfWork]
    public async Task<SqlSugarPagedList<SelectStockGoodsDTO>> GetStockSlotList(GetStockSlotListWhere dto)
    {
        try
        {
            var list = new List<SelectStockGoodsDTO>();
            var listNew = new List<SelectStockGoodsDTO>();


            var query = await _wmsStockTrayRep.AsQueryable()
                .LeftJoin<WmsBaseMaterial>((s, g) => s.MaterialId == g.Id.toString())
                .LeftJoin<WmsBaseUnit>((s, g, u) => g.MaterialUnit == u.Id)
                .Where((s, g, u) => s.WarehouseId == dto.HouseId.toString())
                .Where((s, g, u) => s.StockQuantity > 0)
                .Where((s, g, u) => s.AbnormalStatu == 0)
                .Where((s, g, u) => g.IsEmpty != true)
                .Select((s, g, u) => new SelectStockGoodsDTOQuery
                {
                    Id = g.Id,
                    MaterialCode = g.MaterialCode,
                    MaterialName = g.MaterialName,
                    MaterialStandard = g.MaterialStandard,
                    LotNo = s.LotNo,
                    UnitName = u.UnitName,
                    BoxQuantity = g.BoxQuantity,
                    StockQuantity = s.StockQuantity,
                    CustomerId = s.CustomerId,
                    InspectionStatus = s.InspectionStatus,
                    ValidateDay = s.ValidateDay,
                    WarehouseId = s.WarehouseId,
                    ProductionDate = s.ProductionDate
                }).ToListAsync();


            if (!string.IsNullOrWhiteSpace(dto.Msg))
            {
                query = query.Where(s => s.MaterialCode.Contains(dto.Msg.Trim()) || s.MaterialName.Contains(dto.Msg.Trim()) || s.MaterialStandard.Contains(dto.Msg.Trim()) || s.LotNo.Contains(dto.Msg.Trim())).ToList();
            }

            if (dto.FilterType == "0") //0:根据批次筛选
            {
                list = query.GroupBy(g => new { g.Id, g.MaterialCode, g.MaterialName, g.MaterialStandard, g.UnitName, g.LotNo, g.CustomerId, g.BoxQuantity, g.InspectionStatus })
              .Select(i => new SelectStockGoodsDTO
              {
                  StockMaterialId = i.Key.Id.ToString(),
                  StockMaterialCode = i.Key.MaterialCode,
                  StockMaterialName = i.Key.MaterialName,
                  StockMaterialStandard = i.Key.MaterialStandard,
                  StockMaterialUnitName = i.Key.UnitName,
                  StockLotNo = i.Key.LotNo,
                  CustomerCode = i.Key.CustomerId,
                  ProductionDate = string.IsNullOrEmpty(i.FirstOrDefault().ProductionDate.ToString()) ? null : i.FirstOrDefault().ProductionDate,
                  ValidateDay = string.IsNullOrEmpty(i.FirstOrDefault().ValidateDay.ToString()) ? null : i.FirstOrDefault().ValidateDay,
                  StockQuantity = i.Sum(x => (decimal)x.StockQuantity),
                  BoxQuantity = Convert.ToDecimal(i.Key.BoxQuantity),
                  InspectionStatus = i.Key.InspectionStatus.ToString()
              }).ToList();
            }
            else//1:根据物料筛选
            {
                list = query.GroupBy(g => new { g.Id, g.MaterialCode, g.MaterialName, g.MaterialStandard, g.UnitName, g.CustomerId, g.BoxQuantity, g.InspectionStatus, g.WarehouseId })
              .Select(i => new SelectStockGoodsDTO
              {
                  StockMaterialId = i.Key.Id.ToString(),
                  StockMaterialCode = i.Key.MaterialCode,
                  StockMaterialName = i.Key.MaterialName,
                  StockMaterialStandard = i.Key.MaterialStandard,
                  StockMaterialUnitName = i.Key.UnitName,
                  CustomerCode = i.Key.CustomerId,
                  ProductionDate = string.IsNullOrEmpty(i.FirstOrDefault().ProductionDate.ToString()) ? null : i.FirstOrDefault().ProductionDate,
                  ValidateDay = string.IsNullOrEmpty(i.FirstOrDefault().ValidateDay.ToString()) ? null : i.FirstOrDefault().ValidateDay,
                  StockQuantity = i.Sum(x => (decimal)x.StockQuantity),
                  BoxQuantity = Convert.ToDecimal(i.Key.BoxQuantity),
                  WarehouseId = i.Key.WarehouseId,
                  InspectionStatus = i.Key.InspectionStatus.ToString()
              }).ToList();
            }
            //var stocks = DataContext.WmsStock.Where(s => s.WarehouseId == houseId);//先查询对应仓库的库存信息

            var stocks = await _wmsStockRep.AsQueryable()
                .Where(s => s.WarehouseId == dto.HouseId)
                .ToListAsync();

            listNew = list;
            for (int i = 0; i < list.Count(); i++)
            {
                var fq = new List<WmsStock>();
                if (!string.IsNullOrWhiteSpace(list[i].InspectionStatus))
                {
                    fq = stocks.Where(s => s.MaterialId.toString() == list[i].StockMaterialId && s.InspectionStatus.ToString() == list[i].InspectionStatus).ToList();
                }
                else
                {
                    fq = stocks.Where(s => s.MaterialId.toString() == list[i].StockMaterialId).ToList();
                }
                if (!string.IsNullOrWhiteSpace(list[i].StockLotNo))
                {
                    fq = fq.Where(s => s.LotNo == list[i].StockLotNo).ToList();
                }
                if (fq == null || fq.Count() <= 0)
                {
                    list[i].StockQuantity = 0;
                }
                else
                {
                    list[i].StockQuantity = fq.Sum(p => p.StockQuantity.Value);
                }
                if (list[i].StockQuantity == 0)
                {
                    listNew.RemoveAt(i);
                }
            }
                    ;
            return listNew.OrderBy(a => a.ValidateDay).ToPagedList(dto.Page, dto.PageSize);
        }
        catch (Exception ex)
        {
            throw;
        }
    }







    /// <summary>
    /// 添加出库信息。
    /// </summary>
    /// <returns></returns>
    [DisplayName("添加出库信息")]
    [ApiDescriptionSettings(Name = "AddExportBill"), HttpPost, UnitOfWork]
    public async Task<string> AddExportBill(AddExportNotifyModel model)
    {
        try
        {
            model.Source = "WMS";

            if (model == null || model.Detail.Count == 0)
            {
                return "参数异常";
            }
            List<string> materialCodes = model.Detail.Select(a => a.StockMaterialId).Distinct().ToList();
            var view_WmsBaseMaterial = await View_WmsBaseMaterial();
            List<View_WmsBaseMaterial> materialList = view_WmsBaseMaterial.Where(a => materialCodes.Contains(a.Id.toString())).ToList();
            //var stockList = DataContext.WmsStock.Where(s => materialCodes.Contains(s.MaterialId) && s.StockQuantity > 0).ToList();

            var stockList = await _wmsStockRep.AsQueryable().Where(s => materialCodes.Contains(s.MaterialId.toString()) && s.StockQuantity > 0).ToListAsync();

            //var palletList = DataContext.WmsStockTray.Where(s => materialCodes.Contains(s.MaterialId) && s.StockQuantity > 0 && s.AbnormalStatu == 0).ToList();//托盘信息
            //生成出库单据编号
            //

            var palletList = await _wmsStockTrayRep.AsQueryable().Where(s => materialCodes.Contains(s.MaterialId.toString()) && s.StockQuantity > 0 && s.AbnormalStatu == 0).ToListAsync();

            string billNo = await GetImExNo("CKS");
            //添加出库单
            foreach (var d in model.Detail)
            {
                if (d.ExportQuantity <= 0)
                {
                    return "出库数量必须大于0";
                }
                var material = materialList.Where(a => a.Id.toString() == d.StockMaterialId).FirstOrDefault();
                if (material == null)
                {
                    return "出库物料为空";
                }
                //库存
                List<WmsStock> stocks;
                List<WmsStockTray> pallets;
                string strStockLotNo = "";
                if (!string.IsNullOrWhiteSpace(d.StockLotNo))
                {
                    if (!string.IsNullOrWhiteSpace(d.InspectionStatus))
                    {
                        stocks = stockList.Where(s => s.InspectionStatus.ToString() == d.InspectionStatus).ToList();
                        pallets = palletList.Where(p => p.InspectionStatus.ToString() == d.InspectionStatus).ToList();
                    }
                    stocks = stockList.Where(s => s.MaterialId.toString() == d.StockMaterialId && s.LotNo == d.StockLotNo).ToList();
                    pallets = palletList.Where(p => p.MaterialId == d.StockMaterialId && p.LotNo == d.StockLotNo).ToList();

                    strStockLotNo = d.StockLotNo;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(d.InspectionStatus))
                    {
                        stocks = stockList.Where(s => s.InspectionStatus.ToString() == d.InspectionStatus).ToList();
                        pallets = palletList.Where(p => p.InspectionStatus.ToString() == d.InspectionStatus).ToList();
                    }
                    stocks = stockList.Where(s => s.MaterialId.toString() == d.StockMaterialId).ToList();
                    pallets = palletList.Where(p => p.MaterialId == d.StockMaterialId).ToList();
                }
                //if (!string.IsNullOrWhiteSpace(model.ExportCustomer))//判断客户是否为空
                //{
                //    stocks = stocks.Where(s => s.CustomerId == model.ExportCustomer).ToList();
                //    pallets = pallets.Where(p => p.CustomerId == model.ExportCustomer).ToList();
                //}
                #region 代储
                var wmsBaseBillTypeRep = await _wmsBaseBillTypeRep.AsQueryable().ToListAsync();
                var billType = wmsBaseBillTypeRep.FirstOrDefault(a => a.BillTypeCode == model.ExportBillType);
                if (billType != null)
                {
                    if (billType.ProxyStatus == 1)//判断当前单据类型是否为代储
                    {
                        if (!string.IsNullOrWhiteSpace(model.ExportCustomer))//判断客户是否为空
                        {
                            stocks = stocks.Where(s => s.CustomerId.toString() == model.ExportCustomer).ToList();
                            pallets = pallets.Where(p => p.CustomerId == model.ExportCustomer).ToList();
                        }
                        else
                        {
                            return "当前" + model.ExportCustomer + "库存信息中为空";
                        }
                    }
                    //else
                    //{
                    //    if (!string.IsNullOrWhiteSpace(model.ExportCustomer))
                    //    {
                    //        stocks = stocks.Where(s => s.CustomerId == model.ExportCustomer).ToList();
                    //        pallets = pallets.Where(p => p.CustomerId == model.ExportCustomer).ToList();
                    //    }
                    //    else
                    //    {
                    //        stocks = stocks.Where(s => s.CustomerId == string.Empty || s.CustomerId == "" || s.CustomerId == null).ToList();
                    //        pallets = pallets.Where(p => p.CustomerId == string.Empty || p.CustomerId == "" || p.CustomerId == null).ToList();
                    //    }
                    //}
                }
                else
                {
                    return "出库单据信息获取失败";
                }
                #endregion
                stocks = stocks.Where(s => s.WarehouseId.toString() == model.ExportWarehouseId).ToList();
                pallets = pallets.Where(p => p.WarehouseId == model.ExportWarehouseId).ToList();
                if (stocks == null || stocks.Count < 1)
                {
                    return "出库物料为空";
                }
                if (d.ExportQuantity > pallets.Sum(p => p.StockQuantity))
                {
                    return d.StockMaterialCode + "物料库存数量不足";
                }
                stocks = (from cv in stocks
                          orderby cv.WarehouseId descending, cv.ValidateDay
                          select cv).ToList();//排序找数据，仓库排序，从大到小排序，查找顺序为线边库→平库→多穿库→立体库
                decimal Qty = d.ExportQuantity;
                decimal sum = 0;
                decimal i = 0;
                var stocks1 = new List<WmsExportNotifyDetail>();
                foreach (var stock in stocks)//循环库存主表，锁定库存
                {
                    int stQty = 0;
                    stQty = (int)stock.StockQuantity;
                    WmsExportNotifyDetail item = new WmsExportNotifyDetail();
                    item.Id = SnowFlakeSingle.Instance.NextId();
                    item.ExportBillCode =billNo;
                    item.MaterialId = material.Id;
                    item.MaterialCode = material.MaterialCode;
                    item.MaterialName = material.MaterialName;
                    item.MaterialStandard = material.MaterialStandard;
                    item.MaterialModel = material.MaterialModel;
                    //item.MaterialType = material.MaterialType.ToString();
                    item.MaterialUnit = material.MaterialUnit.toString();
                    item.LotNo = stock.LotNo;
                    if (Qty > stock.StockQuantity)//如果出库数量大于库存数量，先减库存
                    {
                        i = Qty - stQty;
                        stock.StockQuantity -= stQty;
                        stock.LockQuantity += stQty;
                        item.ExportQuantity = stQty;
                        sum = stQty + sum;
                        Qty -= Qty;
                    }
                    else if (i > 0)//因为第一次循环出库数量大于库存数量，第二次循环i>0后直接扣除库存
                    {
                        if (i > stock.StockQuantity)//当i>库存数量，锁定剩余库存数量
                        {
                            i = i - stQty;
                            stock.StockQuantity -= stQty;
                            stock.LockQuantity += stQty;
                            item.ExportQuantity = stQty;
                            sum = stQty + sum;
                        }
                        else//当i小于库存数量，锁定全部库存
                        {
                            stock.StockQuantity -= i;
                            stock.LockQuantity += i;
                            item.ExportQuantity = i;
                            sum = i + sum;
                        }
                    }
                    else//出库数量小于库存数量直接锁定库存数量
                    {
                        stock.StockQuantity -= Qty;
                        stock.LockQuantity += Qty;
                        item.ExportQuantity = Qty;
                        sum = Qty + sum;
                    }

                    item.AllocateQuantity = 0;
                    item.FactQuantity = 0;
                    item.CompleteQuantity = 0;
                    item.WarehouseId = stock.WarehouseId;
                    item.IsDelete = false;
                    item.ExportDetailFlag = 0;
                    item.OuterDetailId = model.OuterBillCode;
                    item.ProductionDate = Convert.ToDateTime(stock.ProductionDate);
                    item.LostDate = Convert.ToDateTime(stock.ValidateDay);
                    item.InspectionStatus = d.InspectionStatus;
                    //DataContext.WmsExportNotifyDetail.InsertOnSubmit(item);
                    //DataContext.SubmitChanges();
                    await _wmsExportNotifyDetailRep.InsertAsync(item);
                    if (sum == d.ExportQuantity) break;//当sum等于出库数量结束循环

                }
            }
            //DataContext.SubmitChanges();
            WmsExportNotify export = new WmsExportNotify();
            export.Id = SnowFlakeSingle.Instance.NextId();
            export.ExportBillCode = billNo;
            export.ExportBillType = long.Parse(model.ExportBillType);
            export.ExportCustomerId = long.Parse(model.ExportCustomer);
            export.ExportQuantity = 0;
            export.ExportFactQuantity = 0;
            export.ExportCompleteQuantity = 0;
            export.ExportDate = DateTime.Now;
            export.WarehouseId = long.Parse(model.ExportWarehouseId);
            export.ExportExecuteFlag = 0;
            export.ExportAddDateTime = DateTime.Now;
            export.ExportProductionDate = DateTime.Now;
            export.IsDelete = false;
            export.Source = model.Source;
            export.PickingArea = model.PickingArea;
            export.OuterBillCode = model.OuterBillCode;
            export.CreateUserId = long.Parse(model.UserId);
            export.CreateTime = DateTime.Now;
            export.WholeOutWare = model.WholeOutWare;
            export.SortOutWare = model.SortOutWare;
            export.DocumentSubtype = long.Parse(model.DocumentSubtype);

            //DataContext.WmsExportNotify.InsertOnSubmit(export);
            await _wmsExportNotifyRep.InsertAsync(export);

            await InsertButtonRecord(model.UserId, "出库单据", billNo + "创建成功");
            //DataContext.SubmitChanges();
            //添加按钮点击记录           
            //tran.Commit();

            //DataContext.Dispose();
            await InsertButtonRecord(model.UserId, "出库单据", "添加出库单据：" + billNo + " 成功");

            return "ok";
        }
        catch (Exception ex)
        {
            throw;
        }


    }





    /// <summary>
    /// 自动分配
    /// </summary>
    /// <returns></returns>
    [DisplayName("自动分配")]
    [ApiDescriptionSettings(Name = "AddExportTaskAuto"), HttpPost, UnitOfWork]
    public async Task<string> AddExportTaskAuto(AddExportTaskAutoWhere model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.WareHouseType))
            {
                return "当前出库单据没有获取到仓库类型";
            }
            var houseList = await _wmsBaseWareHouseRep.AsQueryable().FirstAsync(s => s.Id == long.Parse(model.WareHouseType));
            if (houseList == null)
            {
                return "当前出库单据没有获取到仓库类型";
            }
            //判断当前自动分配的仓库
            if (houseList.WarehouseType == "01")
            {
                var ret = await LTKZDFP(model.Id);
                return ret;
            }
            else if (houseList.WarehouseType == "05" || houseList.WarehouseType == "02")
            {
                var ret = await PKZDFP(model.Id);
                return ret;
            }
            else
            {
                return "当前出库单据没有获取到仓库类型";
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }


    /// <summary>
    /// 手动分配，获取数据
    /// </summary>
    /// <returns></returns>
    [DisplayName("手动分配，获取数据")]
    [ApiDescriptionSettings(Name = "GetStockSlotHandList"), HttpPost, UnitOfWork]
    public async Task<SqlSugarPagedList<StockMaterialDTO>> GetStockSlotHandList(GetStockSlotHandListWhere model)
    {
        try
        {
            List<StockMaterialDTO> stockGoodsList = new List<StockMaterialDTO>();
            var detailList = await _wmsExportNotifyDetailRep.AsQueryable().Where(d => d.Id.ToString() == model.detailId).ToListAsync();
            var detail = detailList.FirstOrDefault();
            if (detail == null)
            {
                return null;// new { code = 0, count = 0, msg = "获取失败，未找到指定出库单！", data = "" };
            }
            if (detail.ExportDetailFlag != 0)
            {
                return null;//new { code = 0, count = 0, msg = "获取失败，出库单已分配完成！", data = "" };
            }
            if (detail.AllocateQuantity >= detail.ExportQuantity)
            {
                return null;//new { code = 0, count = 0, msg = "获取失败，出库单已下发完成！", data = "" };
            }
            var notifyList = await _wmsExportNotifyRep.AsQueryable().Where(a => a.ExportBillCode == detail.ExportBillCode.ToString()).ToListAsync();
            var notify = notifyList.FirstOrDefault();
            if (notify == null)
            {
                return null;//new { code = 0, count = 0, msg = "获取失败，未找到指定出库单！", data = "" };
            }
            if (notify.ExportExecuteFlag == 3 || notify.ExportExecuteFlag == 4 || notify.ExportExecuteFlag == 5)
            {
                return null;//new { code = 0, count = 0, msg = "获取失败，出库单状态不允许！", data = "" };
            }
            //var query = from s in DataContext.WmsStockTray
            //            join g in DataContext.WmsBaseMaterial on s.MaterialId equals g.Id
            //            join l in DataContext.WmsBaseLaneway on s.LanewayId equals l.Id
            //            join h in DataContext.WmsBaseWareHouse on l.WarehouseId equals h.Id
            //            join f in DataContext.WmsBaseUnit on g.MaterialUnit equals f.UnitCode into ac
            //            from ljac in ac.DefaultIfEmpty()
            //            join x in DataContext.WmsBaseSlot on s.StockSlotCode equals x.SlotCode into tmp
            //            from t in tmp.DefaultIfEmpty()
            //            where s.MaterialId == detail.MaterialId && s.StockQuantity > 0 && s.AbnormalStatu == 0
            //            select new StockMaterialDTO
            //            {
            //                Id = s.Id,
            //                MaterialCode = g.MaterialCode,
            //                MaterialName = g.MaterialName,
            //                MaterialStandard = g.MaterialStandard,
            //                MaterialUnit = ljac.UnitName,
            //                StockBatchNumber = s.LotNo,
            //                StockQuantity = (decimal)s.StockQuantity,
            //                SlotCode = s.StockSlotCode,
            //                PalletCode = s.StockCode,
            //                LanewayId = l.Id,
            //                LanewayName = l.LanewayName,
            //                SlotRow = t.SlotRow,
            //                SlotColumn = t.SlotColumn,
            //                SlotLayer = t.SlotLayer,
            //                WarehouseName = h.WarehouseName,
            //                CustomerId = s.CustomerId,
            //                IsSamolingTray = s.IsSamolingTray,
            //                InspectionStatus = s.InspectionStatus
            //            };

            var query = await _wmsStockTrayRep.AsQueryable()
                .LeftJoin<WmsBaseMaterial>((s, g) => s.MaterialId == g.Id.ToString())
                .LeftJoin<WmsBaseLaneway>((s, g, l) => s.LanewayId == l.Id.ToString())
                .LeftJoin<WmsBaseWareHouse>((s, g, l, h) => l.WarehouseId == h.Id)
                .LeftJoin<WmsBaseUnit>((s, g, l, h, f) => g.MaterialUnit.ToString() == f.UnitCode)
                .LeftJoin<WmsBaseSlot>((s, g, l, h, f, x) => s.StockSlotCode == x.SlotCode)
                .Where((s, g, l, h, f, x) => s.MaterialId == detail.MaterialId.ToString())
                .Where((s, g, l, h, f, x) => s.StockQuantity > 0)
                .Where((s, g, l, h, f, x) => s.AbnormalStatu == 0)
                .Select((s, g, l, h, f, x) => new StockMaterialDTO
                {
                    Id = s.Id.ToString(),
                    MaterialCode = g.MaterialCode,
                    MaterialName = g.MaterialName,
                    MaterialStandard = g.MaterialStandard,
                    MaterialUnit = f.UnitName,
                    StockBatchNumber = s.LotNo,
                    StockQuantity = (decimal)s.StockQuantity,
                    SlotCode = s.StockSlotCode,
                    PalletCode = s.StockCode,
                    LanewayId = l.Id.ToString(),
                    LanewayName = l.LanewayName,
                    SlotRow = x.SlotRow,
                    SlotColumn = x.SlotColumn,
                    SlotLayer = x.SlotLayer,
                    WarehouseName = h.WarehouseName,
                    CustomerId = s.CustomerId,
                    IsSamolingTray = s.IsSamolingTray,
                    InspectionStatus = s.InspectionStatus
                }).ToListAsync();


            #region 代储
            var billTypeList = await _wmsBaseBillTypeRep.AsQueryable().ToListAsync();
            var billType = billTypeList.FirstOrDefault(a => a.BillTypeCode == notify.ExportBillType.ToString());
            if (billType != null)
            {
                if (billType.ProxyStatus == 1)//判断当前单据类型是否为代储
                {
                    if (notify.ExportCustomerId != null && notify.ExportCustomerId != -1)//判断客户是否为空
                    {
                        query = query.Where(s => s.CustomerId == notify.ExportCustomerId.ToString()).ToList();
                    }
                    else
                    {
                        return null;//new { code = 0, count = 0, msg = "当前" + notify.ExportCustomerId + "库存信息中为空", data = "" };
                    }
                }
                //else
                //{
                //    if (!string.IsNullOrWhiteSpace(notify.ExportCustomerId))
                //    {
                //        query = query.Where(s => s.CustomerId == notify.ExportCustomerId);
                //    }
                //    else
                //    {
                //        query = query.Where(s => s.CustomerId == string.Empty || s.CustomerId == "" || s.CustomerId == null);
                //    }
                //}
            }
            else
            {
                return null;//new { code = 0, count = 0, msg = "出库单据信息获取失败", data = "" };
            }
            #endregion
            //质检状态
            query = query.Where(s => s.InspectionStatus == Convert.ToInt32(detail.InspectionStatus)).ToList();
            if (!string.IsNullOrWhiteSpace(detail.LotNo))
            {
                query = query.Where(s => s.StockBatchNumber == detail.LotNo).ToList();
            }
            if (!string.IsNullOrWhiteSpace(model.lanewayId))
            {
                query = query.Where(s => s.LanewayId == model.lanewayId).ToList();
            }
            if (model.slotRow.HasValue)
            {
                query = query.Where(s => s.SlotRow == model.slotRow).ToList();
            }
            if (model.slotColumn.HasValue)
            {
                query = query.Where(s => s.SlotColumn == model.slotColumn).ToList();
            }
            if (model.slotLayer.HasValue)
            {
                query = query.Where(s => s.SlotLayer == model.slotLayer).ToList();
            }
            if (!string.IsNullOrWhiteSpace(model.palletCode))
            {
                query = query.Where(s => s.PalletCode == model.palletCode).ToList();
            }
            if (!string.IsNullOrWhiteSpace(model.boxCode))
            {
                var pIdList = await _wmsStockInfoRep.AsQueryable().Where(b => b.BoxCode == model.boxCode).Select(b => b.TrayId).ToListAsync();
                var pId = pIdList.FirstOrDefault();
                query = query.Where(s => s.Id == pId).ToList();
            }
            if (query.Count() < 1)
            {
                return null;//new { code = 0, count = 0, msg = "获取失败，无可出库库存！", data = "" };
            }
            else
            {
                stockGoodsList.AddRange(query.ToList());
            }

            return stockGoodsList.ToPagedList(model.Page, model.PageSize);
        }
        catch (Exception ex)
        {
            return null;//new { code = 0, count = 0, msg = ex.Message, data = "" };
        }
    }






    /// <summary>
    /// 手动添加出库任务
    /// </summary>
    /// <returns></returns>
    [DisplayName("手动添加出库任务")]
    [ApiDescriptionSettings(Name = "AddExportTaskHand"), HttpPost, UnitOfWork]
    public async Task<string> AddExportTaskHand(ManualExportVO model)
    {
        if (model.StockList == null || model.StockList.Count < 1)
        {
            return "操作失败，提交数据异常！";
        }
        try
        {


            try
            {
                ////数据验证
                //var user = DataContext.WmsSysUser.Where(a => a.Id == model.UserId).FirstOrDefault();
                //if (user == null)
                //{
                //    return new { code = 0, count = 0, msg = "操作失败，用户无效！", data = "" };
                //}
                var detailList = await _wmsExportNotifyDetailRep.AsQueryable().Where(d => d.Id.ToString() == model.Id && d.IsDelete == false).ToListAsync();
                var detail = detailList.FirstOrDefault();
                if (detail == null)
                {
                    return "操作失败，未找到指定出库单详情！";
                }
                if (detail.AllocateQuantity >= detail.ExportQuantity || detail.ExportDetailFlag != 0)
                {
                    return "操作失败，出库单已下发完成！";
                }
                var view_WmsExportNotify = await View_WmsExportNotify();
                var notify = view_WmsExportNotify.Where(a => a.ExportBillCode == detail.ExportBillCode.ToString()).FirstOrDefault();
                if (notify == null)
                {
                    return "操作失败，未找到指定出库单！";
                }
                if (notify.ExportExecuteFlag == 3 || notify.ExportExecuteFlag == 4 || notify.ExportExecuteFlag == 5)
                {
                    return "操作失败，出库单已分配完成！";
                }
                //
                var needQty = detail.ExportQuantity - detail.AllocateQuantity;
                var outQty = model.StockList.Select(s => s.Qty).ToList().Sum();
                if (outQty != needQty)
                {
                    return "操作失败，出库数量与计划数量不一致！";
                }
                var stockIds = model.StockList.Select(a => a.StockId).ToList();
                var stockList = await _wmsStockTrayRep.AsQueryable().Where(a => stockIds.Contains(a.Id.ToString())).ToListAsync();


                List<WmsExportOrder> orderList = new List<WmsExportOrder>();
                List<WmsExportBoxInfo> orderBoxList = new List<WmsExportBoxInfo>();
                foreach (var st in model.StockList)
                {
                    var stock = stockList.Where(a => a.Id.ToString() == st.StockId).FirstOrDefault();
                    //判断叠托已有出库数量跳过当前循环（血浆库）
                    var orderTray = orderList.Where(a => a.ExportStockCode == stock.StockCode).ToList();
                    if (orderTray.Count() > 0)
                    {
                        continue;
                    }
                    if (stock == null)
                    {
                        return "操作失败，部分储位库存异常！";
                    }
                    if (st.Qty > stock.StockQuantity)
                    {
                        return "操作失败，出库数量超出库存数量！";
                    }
                    #region MyRegion
                    /* //本托全出
                     if (st.Qty == stock.StockQuantity)
                     {
                         foreach (var b in boxList)
                         {
                             var t = b.Qty;
                             if (t >= bNum)
                             {
                                 boxNum++;
                             }
                             else
                             {
                                 SaveDic(useBoxQtyDic, b.Id, (decimal)t);
                                 b.Qty -= t;
                                 b.LockQuantity += t;
                             }
                         }
                     }
                     else
                     {
                         var n = st.Qty / bNum;
                         var m = st.Qty % bNum;
                         decimal qty = 0;
                         List<string> boxIds = new List<string>();//整箱
                                                                  //有整箱
                         if (n > 0)
                         {
                             var bs = boxList.Where(b => b.Qty >= bNum).ToList();
                             bs = GetPalletWholeBoxs((decimal)stock.StockQuantity, bs, bNum);
                             int i = 0;
                             foreach (var b in bs)
                             {
                                 qty += (decimal)b.Qty;
                                 boxNum++;
                                 boxIds.Add(b.Id);
                                 i++;
                                 if (i >= n)
                                 {
                                     break;
                                 }
                             }
                         }
                         //有散箱
                         if (m > 0)
                         {
                             decimal i = 0;
                             var sbs = boxList.Where(b => b.Qty < bNum).OrderBy(b => b.Qty).ToList();
                             foreach (var b in sbs)
                             {
                                 var t = b.Qty;
                                 if (i + t >= m)
                                 {
                                     SaveDic(useBoxQtyDic, b.Id, m - i);
                                     b.Qty -= m - i;
                                     b.LockQuantity += m - i;
                                     i = m;
                                     break;
                                 }
                                 else
                                 {
                                     SaveDic(useBoxQtyDic, b.Id, (decimal)t);
                                     b.Qty -= t;
                                     b.LockQuantity += t;
                                     i += (decimal)t;
                                 }
                             }
                             qty += i;
                         }
                         //不够
                         if (qty < st.Qty)
                         {
                             var bs = boxList.Where(b => b.Qty > 0).OrderBy(b => b.Qty).ToList();
                             foreach (var b in bs)
                             {
                                 if (boxIds.Contains(b.Id))
                                 {
                                     continue;
                                 }
                                 if (useBoxQtyDic.ContainsKey(b.Id))
                                 {
                                     var t = b.Qty;
                                     if (qty + t >= st.Qty)
                                     {
                                         SaveDic(useBoxQtyDic, b.Id, st.Qty - qty);
                                         b.Qty -= st.Qty - qty;
                                         b.LockQuantity += st.Qty - qty;
                                         qty = st.Qty;
                                         break;
                                     }
                                     else
                                     {
                                         SaveDic(useBoxQtyDic, b.Id, (decimal)t);
                                         b.Qty -= t;
                                         b.LockQuantity += t;
                                         qty += (decimal)t;
                                     }
                                 }
                                 else
                                 {
                                     var t = b.Qty;
                                     if (qty + t >= st.Qty)
                                     {
                                         SaveDic(useBoxQtyDic, b.Id, st.Qty - qty);
                                         b.Qty -= st.Qty - qty;
                                         b.LockQuantity += st.Qty - qty;
                                         qty = st.Qty;
                                         break;
                                     }
                                     else
                                     {
                                         if (t >= bNum)
                                         {
                                             boxNum++;
                                         }
                                         else
                                         {
                                             SaveDic(useBoxQtyDic, b.Id, (decimal)t);
                                             b.Qty -= t;
                                             b.LockQuantity += t;
                                         }
                                         qty += (decimal)t;
                                     }
                                 }
                             }
                         }
                     }*/
                    #endregion

                    var stockTary = await _wmsStockTrayRep.AsQueryable().Where(a => a.StockSlotCode == stock.StockSlotCode && a.MaterialId == stock.MaterialId && a.LotNo == stock.LotNo && a.InspectionStatus == stock.InspectionStatus).ToListAsync();
                    var commonMethod = new CommonMethod(_sqlSugarClient);
                    foreach (var item in stockTary)
                    {
                        var boxList = await _wmsStockInfoRep.AsQueryable().Where(b => b.TrayId == item.Id.ToString() && b.Qty > 0 && b.Picked == 0).ToListAsync();
                        string orderCode = commonMethod.GetImExNo("CK");

                        //if (orderList.Count < 1)
                        //{
                        //    orderCode = await GetImExNo("CK");
                        //}
                        //else
                        //{
                        //    var lastOrder = orderList.LastOrDefault();
                        //    var num = lastOrder.ExportOrderNo;
                        //    num++;
                        //    orderCode = $"CK{num}";
                        //}
                        WmsExportOrder order = new WmsExportOrder();
                        order.Id = SnowFlakeSingle.Instance.NextId();
                        order.ExportOrderNo = orderCode;
                        order.ExportBillType = notify.ExportBillType;
                        order.ExportWarehouseId = long.Parse(item.WarehouseId);
                        order.ExportSlotCode = item.StockSlotCode;
                        order.ExportStockCode = item.StockCode;
                        order.ExportWeight = "0";
                        order.ExportDepartmentCode = notify.ExportDepartmentId;
                        order.ExpotSupplierCode = notify.ExportSupplierId;
                        order.ExportCustomerCode = notify.ExportCustomerId;
                        order.ExportTaskNo = "";
                        order.ExportExecuteFlag = 0;
                        order.ExporOrederDate = DateTime.Now;
                        //血浆库判断要是叠托情况下，相同的托盘编号代表要出的托盘      order.ExportRemark = "0";   0:出库拣货托盘;1:出库跟随托盘
                        if (item.StockSlotCode == stock.StockSlotCode && notify.WarehouseType == "LTK")
                        {
                            order.ExportRemark = "0";
                            order.ExportQuantity = item.StockQuantity;
                            var stockQuantity = item.StockQuantity;
                            item.StockQuantity -= stockQuantity;
                            item.LockQuantity += stockQuantity;
                            order.OutType = 0;
                            order.PickedNum = stockQuantity;
                        }
                        else
                        {
                            order.ExportRemark = "1";
                            order.ExportQuantity = st.Qty;
                            item.StockQuantity -= st.Qty;
                            item.LockQuantity += st.Qty;
                            order.OutType = st.Qty >= stock.StockQuantity ? 0 : 1;
                            order.PickedNum = st.Qty;
                        }

                        order.ExportLotNo = item.LotNo;
                        order.ExportBillCode = notify.ExportBillCode;
                        order.ExportMaterialId = long.Parse(item.MaterialId);
                        order.ExportMaterialCode = detail.MaterialCode;
                        order.ExportMaterialName = detail.MaterialName;
                        order.ExportMaterialStandard = detail.MaterialStandard;
                        order.ExportMaterialUnit = detail.MaterialUnit;
                        order.ExportAreaId = long.Parse(item.LanewayId);
                        order.WholeBoxNum = boxList.Count().ToString();
                        order.IsDelete = false;

                        order.ExportDetailId = detail.Id;
                        order.CreateUserId = 0;
                        order.CreateTime = DateTime.Now;
                        order.StockWholeBoxNum = boxList.Count().ToString();
                        order.StockQuantity = item.StockQuantity.ToString();
                        order.ExportType = 0;
                        order.InspectionStatus = item.InspectionStatus;
                        order.WholeOutWare = notify.WholeOutWare;
                        orderList.Add(order);
                        //查询对应的箱码信息
                        var boxs = boxList.Where(a => a.TrayId == item.Id.ToString()).ToList();
                        if (boxs == null || boxs.Count() < 1)
                        {
                            return "操作失败，物料对应箱码不存在！";
                        }
                        foreach (var b in boxs)
                        {
                            WmsExportBoxInfo info = new WmsExportBoxInfo()
                            {
                                Id = SnowFlakeSingle.Instance.NextId(),
                                BoxCode = b.BoxCode,
                                Qty = (decimal)b.Qty,
                                StockCodeCode = stock.StockCode,
                                ExportOrderNo = orderCode,
                                IsOut = 0,
                                MaterialId = b.MaterialId,
                                LotNo = b.LotNo,
                                AddDate = DateTime.Now,
                                IsDelete = false,
                                Status = 0,
                                ProductionDate = b.ProductionDate,
                                ExportLoseDate = b.ValidateDay,
                                IsMustExport = 1
                            };

                            //DataContext.WmsExportBoxInfo.InsertOnSubmit(info);

                            await _wmsExportBoxInfoRep.InsertAsync(info);
                        }

                    }
                }
                foreach (var o in orderList)
                {
                    //DataContext.WmsExportOrder.InsertOnSubmit(o);
                    await _wmsExportOrderRep.InsertAsync(o);
                }
                detail.AllocateQuantity += outQty;
                detail.ExportDetailFlag = 1;//已完成
                var notify1List = await _wmsExportNotifyRep.AsQueryable().Where(a => a.ExportBillCode == notify.ExportBillCode).ToListAsync();
                var notify1 = notify1List.FirstOrDefault();
                if (notify1.ExportExecuteFlag == 0)
                {
                    notify1.ExportExecuteFlag = 1;
                }
                notify1.UpdateUserId = 0;
                notify1.UpdateTime = DateTime.Now;

                return "操作成功！";
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
            finally
            {

            }

        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }




    /// <summary>
    ///  关单和作废出库单信息。
    /// </summary>
    /// <returns></returns>
    [DisplayName(" 关单和作废出库单信息。")]
    [ApiDescriptionSettings(Name = "EditExportBill"), HttpPost, UnitOfWork]
    public async Task<string> EditExportBill(EditExportBillWhere model)
    {
        try
        {
            string txt = "操作失败!";
            int rst = 0;
            if (model.status == 2)//完成 
            {
                rst = await TryFinishExportNotify(model.id.ToString(), model.userId); //关单
            }
            else if (model.status == -1)
            {
                rst = await CancelExportNotify(model.id.ToString(), model.userId); //作废
            }
            else
            {

                return "操作类型无法识别";
            }
            if (rst > 0)
            {
                switch (rst)
                {
                    case 1:
                        txt += "找不到出库单!";
                        break;
                    case 2:
                        txt += "出库单已完成!";
                        break;
                    case 3:
                        txt += "出库单已上传!";
                        break;
                    case 4:
                        txt += "出库单已作废!";
                        break;
                    case 5:
                        txt += "出库单有未完成的任务!";
                        break;
                    case 6:
                        txt += "出库单有已完成的流水!";
                        break;
                }

                return txt;
            }


            return "操作成功！";
        }
        catch (Exception ex)
        {

            return ex.Message;
        }
    }








    /// <summary>
    ///  根据仓库选择获取对应出库口。
    /// </summary>
    /// <returns></returns>
    [DisplayName(" 根据仓库选择获取对应出库口。")]
    [ApiDescriptionSettings(Name = "GetOutboundPortByWare"), HttpPost, UnitOfWork]
    public async Task<List<GetOutboundPortByWareOfPage>> GetOutboundPortByWare(GetOutboundPortByWareWhere model)
    {
        try
        {

            //var ware = DataContext.WmsBaseWareHouse.Where(a => a.Id == wareId).FirstOrDefault();
            //var list = DataContext.WmsDictionary.Where(a => a.TopCode == "ExportPort" && a.Code.Contains(ware.WarehouseCode) && a.IsDel == 0).OrderBy(a => a.Code).ToList();
            //return list;

            var wareList = await _wmsBaseWareHouseRep.AsQueryable().Where(a => a.Id.ToString() == model.wareId).ToListAsync();
            var ware = wareList.FirstOrDefault();

            var list = await _sysDictTypeRep.AsQueryable()
                .LeftJoin<SysDictData>((x, a) => x.Id == a.DictTypeId)
                .Where((x, a) => x.Code == "ExportPort")
                .Where((x, a) => x.Code.Contains(ware.WarehouseCode))
                .OrderBy((x, a) => a.Id)
                .Select((x, a) => new GetOutboundPortByWareOfPage
                {
                    Id = a.Id,
                    Label = a.Label,
                    Value = a.Value,
                    Code = a.Code
                }).ToListAsync();

            return list;

        }
        catch (Exception ex)
        {

            return null;
        }
    }





    #endregion

    #region 出库任务
    /// <summary>
    ///获取出库任务
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取出库任务")]
    [ApiDescriptionSettings(Name = "GetExportTaskList"), HttpPost, UnitOfWork]
    public async Task<SqlSugarPagedList<View_WmsExportTask>> GetExportTaskList(TaskModel model)
    {
        try
        {
            var view_WmsExportTask = await View_WmsExportTask();
            var query = view_WmsExportTask.ToList();
            if (!string.IsNullOrEmpty(model.ExportTaskFlag))
            {
                query = query.Where(a => a.ExportTaskFlag.ToString() == model.ExportTaskFlag).ToList();
            }
            else
            {
                query = query.Where(a => a.ExportTaskFlag == 0 || a.ExportTaskFlag == 1).ToList();
            }
            if (!string.IsNullOrEmpty(model.taskNo))
            {
                query = query.Where(a => a.ExportTaskNo.Contains(model.taskNo.Trim())).ToList();
            }
            if (!string.IsNullOrEmpty(model.StockCode))
            {
                query = query.Where(a => a.StockCode == model.StockCode.Trim()).ToList();
            }
            if (!string.IsNullOrEmpty(model.startDate.ToString()))
                query = query.Where(a => a.CreateTime >= Convert.ToDateTime(model.startDate)).ToList();
            if (!string.IsNullOrEmpty(model.endDate.ToString()))
            {
                var time = Convert.ToDateTime(model.endDate);
                query = query.Where(a => a.CreateTime < time.AddDays(1)).ToList();
            }
            return query.OrderBy(a => a.ExportTaskNo).ToPagedList(model.Page, model.PageSize);

        }
        catch (Exception ex)
        {
            throw new Exception("新表根据条件查询任务列表" + ex.Message);
        }
    }

    /// <summary>
    ///获取待办出库任务数量
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取待办出库任务数量")]
    [ApiDescriptionSettings(Name = "GetExportTask"), HttpPost, UnitOfWork]
    public async Task<int> GetExportTask()
    {
        try
        {
            var view_WmsExportTask = await View_WmsExportTask();
            int importTaskNum = view_WmsExportTask.Where(a => a.ExportTaskFlag != 2 && a.IsDelete == false).Count();
            return importTaskNum;
        }
        catch (Exception ex)
        {
            throw;
        }
    }



    /// <summary>
    /// 单天出库任务量
    /// </summary>
    /// <returns></returns>
    [DisplayName("单天出库任务量")]
    [ApiDescriptionSettings(Name = "ExportTaskDayList"), HttpPost, UnitOfWork]
    public async Task<Dictionary<string, int>> ExportTaskDayList()
    {
        try
        {

            //7天内的时间
            var dateNow6 = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd");
            var dateNow5 = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd");
            var dateNow4 = DateTime.Now.AddDays(-4).ToString("yyyy-MM-dd");
            var dateNow3 = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            var dateNow2 = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
            var dateNow1 = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            var dateNow0 = DateTime.Now.ToString("yyyy-MM-dd");
            //所有任务量
            var importTask = await _wmsExportTaskRep.AsQueryable().Where(a => a.IsDelete == false).ToListAsync();
            //所有任务量中7天内的数据
            if (!string.IsNullOrEmpty(dateNow6.ToString()))
            {
                importTask = importTask.Where(a => a.SendDate.Value.ToString("yyyy-MM-dd") == dateNow6 || a.SendDate.Value.ToString("yyyy-MM-dd") == dateNow5 || a.SendDate.Value.ToString("yyyy-MM-dd") == dateNow4 || a.SendDate.Value.ToString("yyyy-MM-dd") == dateNow3 || a.SendDate.Value.ToString("yyyy-MM-dd") == dateNow2 || a.SendDate.Value.ToString("yyyy-MM-dd") == dateNow1 || a.SendDate.Value.ToString("yyyy-MM-dd") == dateNow0).ToList();
            }
            //根据时间分组
            var days = importTask.GroupBy(m => new { V = m.SendDate.Value.ToString("yyyy-MM-dd") });

            Dictionary<string, int> taskNoList = new Dictionary<string, int>();
            taskNoList.Add(dateNow0, 0);
            taskNoList.Add(dateNow1, 0);
            taskNoList.Add(dateNow2, 0);
            taskNoList.Add(dateNow3, 0);
            taskNoList.Add(dateNow4, 0);
            taskNoList.Add(dateNow5, 0);
            taskNoList.Add(dateNow6, 0);
            foreach (var item in days)
            {
                var dangtian = 0;
                if (item.Key.V == (dateNow0))
                {
                    dangtian = item.Count();
                    taskNoList[dateNow0] = dangtian;
                }

                if (item.Key.V == (dateNow1))
                {
                    dangtian = item.Count();
                    taskNoList[dateNow1] = dangtian;
                }

                if (item.Key.V == (dateNow2))
                {
                    dangtian = item.Count();
                    taskNoList[dateNow2] = dangtian;
                }
                if (item.Key.V == (dateNow3))
                {
                    dangtian = item.Count();
                    taskNoList[dateNow3] = dangtian;
                }
                if (item.Key.V == (dateNow4))
                {
                    dangtian = item.Count();
                    taskNoList[dateNow4] = dangtian;
                }
                if (item.Key.V == (dateNow5))
                {
                    dangtian = item.Count();
                    taskNoList[dateNow5] = dangtian;
                }
                if (item.Key.V == (dateNow6))
                {
                    dangtian = item.Count();
                    taskNoList[dateNow6] = dangtian;
                }
            }

            var Last10MinsSortedDict = (from entry in taskNoList orderby entry.Key select entry)
           .ToDictionary(pair => pair.Key, pair => pair.Value);
            return Last10MinsSortedDict;

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    #endregion

    #endregion
}
