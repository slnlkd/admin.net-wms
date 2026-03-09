// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Application.Entity;
using Admin.NET.Core.Service;
using Mapster;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System;
using System.ComponentModel;
using static SKIT.FlurlHttpClient.Wechat.Api.Models.ComponentTCBBatchCreateContainerServiceVersionRequest.Types;

namespace Admin.NET.Application;

/// <summary>
/// 库存预警服务
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class WmsStockSlotReportService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsStockTray> _wmsStockTrayRep;
    private readonly SqlSugarRepository<WmsStock> _wmsStockRep;
    private readonly SqlSugarRepository<SysConfig> _sysConfigRep;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly SysConfigService _sysConfigService;
    private readonly SysDictTypeService _sysDictTypeService;
    private readonly ILogger _logger;

    public WmsStockSlotReportService(
       SqlSugarRepository<WmsStockTray> wmsStockTrayRep,
       SqlSugarRepository<WmsStock> wmsStockRep,
       SqlSugarRepository<SysConfig> sysConfigRep,
       ISqlSugarClient sqlSugarClient,
       SysConfigService sysConfigService,
       SysDictTypeService sysDictTypeService,
       ILoggerFactory loggerFactory)
    {
        _wmsStockTrayRep = wmsStockTrayRep;
        _wmsStockRep = wmsStockRep;
        _sysConfigRep = sysConfigRep;
        _sqlSugarClient = sqlSugarClient;
        _sysConfigService= sysConfigService;
        _sysDictTypeService = sysDictTypeService;
        _logger = loggerFactory.CreateLogger("库存预警");
    }

    /// <summary>
    /// 效期超期预警 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("效期超期预警")]
    [ApiDescriptionSettings(Name = "ValidityExpireWarning"), HttpPost]
    public async Task<SqlSugarPagedList<WmsValidityWarningDto>> ValidityExpireWarning(WmsVEWarningInput input)
    {
        List<WmsReportOutPut> ReportModel = new List<WmsReportOutPut>();

        try
        {
            var query = _wmsStockTrayRep.AsQueryable()//库存载具表
     .LeftJoin<WmsBaseMaterial>((o, material) => o.MaterialId == SqlFunc.ToString(material.Id))//物料信息
     .LeftJoin<WmsBaseSlot>((o, material, slot) => slot.SlotCode == o.StockSlotCode)//储位管理表
     .LeftJoin<WmsBaseWareHouse>((o, material, slot, house) => slot.WarehouseId == house.Id)//仓库表
     .LeftJoin<WmsSysStockCode>((o, material, slot, house, stock) => stock.StockCode == o.StockCode)//托盘管理
     .LeftJoin<WmsBaseUnit>((o, material, slot, house, stock,unit) => unit.Id == material.MaterialUnit)//托盘管理
     .Where(o => o.ProductionDate != null);

            // 根据type在数据库层面筛选
            //近效期预警
            if (input.type == "Validity")
            {
                //query = query.Where((o, material, slot, house, stock) =>
                //    o.ValidateDay.HasValue &&
                //    SqlFunc.DateDiff(DateType.Day, DateTime.Now, o.ValidateDay.Value) > 0 &&
                //    SqlFunc.DateDiff(DateType.Day, DateTime.Now, o.ValidateDay.Value) <= 30);
                query = query.Where((o, material, slot, house, stock, unit) =>
                   o.ValidateDay.HasValue &&
                   o.ValidateDay.Value > DateTime.Now.Date &&
                   SqlFunc.DateDiff(DateType.Day, DateTime.Now.Date, o.ValidateDay.Value) <= input.warningDay);
            }
            //超期
            else if (input.type == "Expire")
            {
                //query = query.Where((o, material, slot, house, stock) =>
                //    o.ValidateDay.HasValue &&
                //    SqlFunc.DateDiff(DateType.Day, o.ValidateDay.Value, DateTime.Now) > 0);
                query = query.Where((o, material, slot, house, stock, unit) =>
                    o.ValidateDay.HasValue &&
                    o.ValidateDay.Value < DateTime.Now.Date);
            }
            //呆滞库存NactionStock
            else if (string.IsNullOrWhiteSpace(input.type))
            {
                query = query.Where((o, material, slot, house, stock, unit) =>
                    o.StockDate.HasValue &&
                    SqlFunc.DateDiff(DateType.Day, o.StockDate.Value, DateTime.Now) > 100);
            }

            // 仓库筛选
            if (input.wareHouseId.HasValue && input.wareHouseId.Value > 0)
            {
                query = query.Where((o, material, slot, house, stock, unit) =>
                    o.WarehouseId == input.wareHouseId.ToString());
            }

            // 关键词筛选
            if (!string.IsNullOrEmpty(input.keyWord))
            {
                query = query.Where((o, material, slot, house, stock, unit) =>
                    o.MaterialId.Contains(input.keyWord) ||
                    material.MaterialName.Contains(input.keyWord) ||
                    material.MaterialStandard.Contains(input.keyWord));
            }

            var into = query.Select((o, material, slot, house, stock, unit) => new WmsValidityWarningDto
            {
                Id=o.Id,
                DaysReamin = (!o.ValidateDay.HasValue) ? 0 :
                    SqlFunc.DateDiff(DateType.Day, DateTime.Now, o.ValidateDay.Value),//剩余天数
                DaysOverdue = (!o.ValidateDay.HasValue) ? 0 :
                    SqlFunc.DateDiff(DateType.Day, o.ValidateDay.Value, DateTime.Now),//逾期天数
                NactionStock = (!o.StockDate.HasValue) ? 0 :
                    SqlFunc.DateDiff(DateType.Day, o.StockDate.Value, DateTime.Now),//库存未处理天数
                ProductionDate = o.ProductionDate,
                ValidateDay= o.ValidateDay,
                WarehouseId = o.WarehouseId,
                MaterialCode = material.MaterialCode,
                MaterialName = material.MaterialName,
                MaterialStandard = material.MaterialStandard,
                LotNo=o.LotNo,
                StockQuantity=o.StockQuantity,
                UnitName=unit.UnitName,
                SlotCode=o.StockSlotCode,
                StockStockCode=o.StockCode,
                WarehouseName=house.WarehouseName
            }).ToList();

            return into.OrderBy(o => o.ProductionDate).ToPagedList(input.Page, input.PageSize);
        }
        catch (Exception e)
        {
            throw new Exception($"效期超期预警：{e.Message}");
        }
    }

    /// <summary>
    /// 低库存预警
    /// </summary>
    /// <returns></returns>
    [DisplayName("低库存预警")]
    [ApiDescriptionSettings(Name = "StockLowWarning"), HttpPost]
    public async Task<SqlSugarPagedList<WmsStockWarningDto>> StockLowWarning(WmsStockWarningInput input)
    {
        List<WmsStockWarningDto> ReportModel = new List<WmsStockWarningDto>();
        try
        {
            var stockWarningQuery = _wmsStockRep.AsQueryable()
                .Where(s => s.IsDelete == false)
                .WhereIF(!string.IsNullOrWhiteSpace(input.wareHouseId),
                         s => SqlFunc.ToString(s.WarehouseId) == input.wareHouseId)
                .GroupBy(s => s.MaterialId)
                .Select(s => new
                {
                    MaterialId = s.MaterialId,
                    StockQty = SqlFunc.AggregateSum(s.StockQuantity + s.LockQuantity)
                })
                .InnerJoin<WmsBaseMaterial>((s, material) => s.MaterialId == material.Id && material.IsDelete == false)//物料
                .LeftJoin<WmsBaseWareHouse>((s, material, warehouse) => material.WarehouseId == warehouse.Id)
                .LeftJoin<WmsBaseUnit>((s, material, warehouse, unit) => material.MaterialUnit == unit.Id)
                .Where((s, material, warehouse, unit) => SqlFunc.ToDecimal(material.MaterialStockLow) > 0 && SqlFunc.ToDecimal(material.MaterialStockLow) > s.StockQty)
                .Select((s, material, warehouse, unit) => new WmsStockWarningDto
                {
                    Id = material.Id,
                    WarehouseName = warehouse.WarehouseName,
                    MaterialCode = material.MaterialCode,
                    MaterialName = material.MaterialName,
                    MaterialStandard = material.MaterialStandard,
                    StockQuantity = s.StockQty.ToString(),
                    MaterialStockLow = material.MaterialStockLow,
                    MaterialLowCount = SqlFunc.Abs(SqlFunc.ToDecimal(material.MaterialStockLow) - SqlFunc.ToDecimal(s.StockQty)).ToString(),
                    MaterialUnit = unit.UnitName
                }).ToList();

            if (stockWarningQuery.Count() > 0)
                return stockWarningQuery.OrderByDescending(s => s.MaterialLowCount).ToPagedList(input.Page, input.PageSize);
            else
                return new SqlSugarPagedList<WmsStockWarningDto>();
        }
        catch (Exception e)
        {
            throw;
        }
    }

    /// <summary>
    /// 高库存预警
    /// </summary>
    /// <returns></returns>
    [DisplayName("高库存预警")]
    [ApiDescriptionSettings(Name = "StockHighWarning"), HttpPost]
    public async Task<SqlSugarPagedList<WmsStockHighWarningDto>> StockHighWarning(WmsStockWarningInput input)
    {
        List<WmsStockWarningDto> ReportModel = new List<WmsStockWarningDto>();
        try
        {
            var stockWarningQuery = _wmsStockRep.AsQueryable()
                .Where(s => s.IsDelete == false)
                .WhereIF(!string.IsNullOrWhiteSpace(input.wareHouseId),
                         s => SqlFunc.ToString(s.WarehouseId) == input.wareHouseId)
                .GroupBy(s => s.MaterialId)
                .Select(s => new
                {
                    MaterialId = s.MaterialId,
                    StockQty = SqlFunc.AggregateSum(s.StockQuantity + s.LockQuantity)
                })
                .InnerJoin<WmsBaseMaterial>((s, material) => s.MaterialId == material.Id && material.IsDelete == false)
                .LeftJoin<WmsBaseWareHouse>((s, material, warehouse) => material.WarehouseId == warehouse.Id)
                .LeftJoin<WmsBaseUnit>((s, material, warehouse, unit) => material.MaterialUnit == unit.Id)
                .Where((s, material, warehouse, unit) => SqlFunc.ToDecimal(material.MaterialStockHigh) > 0 && s.StockQty > SqlFunc.ToDecimal(material.MaterialStockHigh))
                .Select((s, material, warehouse, unit) => new WmsStockHighWarningDto
                {
                    Id = material.Id,
                    WarehouseName = warehouse.WarehouseName,
                    MaterialCode = material.MaterialCode,
                    MaterialName = material.MaterialName,
                    MaterialStandard = material.MaterialStandard,
                    StockQuantity = s.StockQty.ToString(),
                    MaterialStockHigh = material.MaterialStockHigh,
                    MaterialHighCount = SqlFunc.Abs(SqlFunc.ToDecimal(s.StockQty) - SqlFunc.ToDecimal(material.MaterialStockHigh)).ToString(),
                    MaterialUnit = unit.UnitName
                }).ToList();

            if (stockWarningQuery.Count() > 0)
                return stockWarningQuery.OrderByDescending(s => s.MaterialHighCount).ToPagedList(input.Page, input.PageSize);
            else
                return new SqlSugarPagedList<WmsStockHighWarningDto>();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 复验期预警
    /// </summary>
    /// <returns></returns>
    [DisplayName("复验期预警")]
    [ApiDescriptionSettings(Name = "ReInspectionPeriodWarning"), HttpPost]
    public async Task<SqlSugarPagedList<WmsReInspectionPeriodWarningDto>> ReInspectionPeriodWarning(WmsVEWarningInput input)
    {
        try
        {
            var query = _wmsStockTrayRep.AsQueryable()//库存载具表
     .LeftJoin<WmsBaseMaterial>((o, material) => o.MaterialId == SqlFunc.ToString(material.Id))//物料信息
     .LeftJoin<WmsBaseUnit>((o, material, unit) => unit.Id == material.MaterialUnit)
     .LeftJoin<WmsBaseSlot>((o, material, unit, slot) => slot.SlotCode == o.StockSlotCode)//储位管理表
     .LeftJoin<WmsBaseWareHouse>((o, material, unit, slot, house) => slot.WarehouseId == house.Id)//仓库表
     .LeftJoin<WmsSysStockCode>((o, material, unit, slot, house, stock) => stock.StockCode == o.StockCode)//托盘管理
     .Where(o => o.ProductionDate != null);

            var queryable = query.ToList();
            query = query.Where((o, material, unit, slot, house, stock) =>
                o.RetestDate.HasValue &&
                SqlFunc.DateDiff(DateType.Day, DateTime.Now, o.RetestDate.Value) > 0 &&
                SqlFunc.DateDiff(DateType.Day, DateTime.Now, o.RetestDate.Value) <= input.warningDay);

            var query1 = query.ToList();
            // 仓库筛选
            if (input.wareHouseId.HasValue && input.wareHouseId.Value > 0)
            {
                query = query.Where((o, material, unit, slot, house, stock) =>
                    o.WarehouseId == input.wareHouseId.Value.ToString());
            }

            // 关键词筛选
            if (!string.IsNullOrEmpty(input.keyWord))
            {
                query = query.Where((o, material, unit, slot, house, stock) =>
                    o.MaterialId.Contains(input.keyWord) ||
                    material.MaterialName.Contains(input.keyWord) ||
                    material.MaterialStandard.Contains(input.keyWord));
            }

            var into = query.Select((o, material, unit, slot, house, stock) => new WmsReInspectionPeriodWarningDto
            {
                Id = o.Id,
                StockQuantity =o.StockQuantity,
                UnitName=unit.UnitName,
                LotNo=o.LotNo,
                SlotCode = o.StockSlotCode,
                StockStockCode = o.StockCode,
                //DaysReamin = (!o.RetestDate.HasValue) || (o.RetestDate.HasValue&& SqlFunc.DateDiff(DateType.Day, DateTime.Now, o.RetestDate.Value)<0) ? 0 :
                //    SqlFunc.DateDiff(DateType.Day, DateTime.Now, o.RetestDate.Value),//剩余天数
                //DaysOverdue = (!o.RetestDate.HasValue) || (o.RetestDate.HasValue && SqlFunc.DateDiff(DateType.Day, o.RetestDate.Value, DateTime.Now) < 0) ? 0 :
                //    SqlFunc.DateDiff(DateType.Day, o.RetestDate.Value, DateTime.Now),//逾期天数

                // 修正后的剩余天数计算
                DaysReamin = o.RetestDate.HasValue ?
        (SqlFunc.DateDiff(DateType.Day, DateTime.Now, o.RetestDate.Value) > 0 ?
            SqlFunc.DateDiff(DateType.Day, DateTime.Now, o.RetestDate.Value) : 0)
        : 0,

                // 修正后的逾期天数计算
                DaysOverdue = o.RetestDate.HasValue ?
        (SqlFunc.DateDiff(DateType.Day, o.RetestDate.Value, DateTime.Now) > 0 ?
            SqlFunc.DateDiff(DateType.Day, o.RetestDate.Value, DateTime.Now) : 0)
        : 0,

                NactionStock = (!o.StockDate.HasValue) ? 0 :
                SqlFunc.DateDiff(DateType.Day, o.StockDate.Value, DateTime.Now),//库存未处理天数
                ProductionDate = o.ProductionDate,
                RetestDate = o.RetestDate,
                WarehouseId = o.WarehouseId,
                MaterialCode = material.MaterialCode,
                MaterialName = material.MaterialName,
                MaterialStandard = material.MaterialStandard
            }).ToList();

            return into.OrderBy(o => o.DaysOverdue).ToPagedList(input.Page, input.PageSize);
        }
        catch (Exception e)
        {
            throw new Exception($"复验期预警：{e.Message}");
        }
    }

    /// <summary>
    /// 库存预警天数
    /// </summary>
    /// <param name="Code"></param>
    /// <returns></returns>
    [DisplayName("库存预警天数")]
    [ApiDescriptionSettings(Name = "GetWarningConfig"), HttpGet]
    public virtual async Task<int> GetWarningConfig([FromQuery] string Code)
    {
        var config = await _sysConfigRep.AsQueryable().FirstAsync(u => u.Code == Code);
        if (config == null) return 0;
        return !string.IsNullOrWhiteSpace(config.Value)?Convert.ToInt32(config.Value):0;
    }

    /// <summary>
    /// 库存预警天数配置
    /// </summary>
    /// <param name="code"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [DisplayName("库存预警天数配置")]
    [ApiDescriptionSettings(Name = "WarningConfig"), HttpGet]
    public virtual async Task WarningConfig([FromQuery] WmsWarningConfigInput input)
    {
        var config = await _sysConfigRep.AsQueryable().FirstAsync(u => u.Code ==input.code);
        if (config == null) return;

        config.Value =input.value;
        await _sysConfigRep.AsUpdateable(config).ExecuteCommandAsync();
    }

    /// <summary>
    /// 导出信息 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出信息")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(WmsReportExportParamInput input)
    {
        switch (input.WarningType)
        {
            case 1:
                var list = (await ValidityExpireWarning(new WmsVEWarningInput { type=input.type, wareHouseId =input.wareHouseId, keyWord =input.keyWord , warningDay =input.warningDay,Page=input.Page,PageSize=input.PageSize })).Items?.Adapt<List<WmsValidityWarningExportDto>>() ?? new();
                if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
                return ExcelHelper.ExportTemplate(list, "超期预警报表导出记录");
            case 2:
                var list1 = (await ValidityExpireWarning(new WmsVEWarningInput { type = input.type, wareHouseId = input.wareHouseId, keyWord = input.keyWord, warningDay = input.warningDay, Page = input.Page, PageSize = input.PageSize })).Items?.Adapt<List<WmsValidityWarningLjExportDto>>() ?? new();
                if (input.SelectKeyList?.Count > 0) list1 = list1.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
                return ExcelHelper.ExportTemplate(list1, "近效期预警报表导出记录");
            case 3:
                var list2 = (await ReInspectionPeriodWarning(new WmsVEWarningInput { type = input.type, wareHouseId = input.wareHouseId, keyWord = input.keyWord, warningDay = input.warningDay, Page = input.Page, PageSize = input.PageSize })).Items?.Adapt<List<WmsRetestWarningExportDto>>() ?? new();
                if (input.SelectKeyList?.Count > 0) list2 = list2.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
                return ExcelHelper.ExportTemplate(list2, "复验期预警报表导出记录");
            case 4:
                var list3 = (await StockLowWarning(new WmsStockWarningInput {wareHouseId = input.wareHouseId.ToString(), Page = input.Page, PageSize = input.PageSize })).Items?.Adapt<List<WmsStockLowWarningExportDto>>() ?? new();
                if (input.SelectKeyList?.Count > 0) list3 = list3.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
                return ExcelHelper.ExportTemplate(list3, "低库存预警报表导出记录");
            case 5:
                var list4 = (await StockHighWarning(new WmsStockWarningInput { wareHouseId = input.wareHouseId.ToString(), Page = input.Page, PageSize = input.PageSize })).Items?.Adapt<List<WmsStockHighWarningExportDto>>() ?? new();
                if (input.SelectKeyList?.Count > 0) list4 = list4.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
                return ExcelHelper.ExportTemplate(list4, "高库存预警报表导出记录");
            default:
                return null;
        }
       
    }
}
