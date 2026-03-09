// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Application.Entity;
using Admin.NET.Core.Service;
using Dm.util;
using DocumentFormat.OpenXml.Office2010.Excel;
using Mapster;
using Microsoft.Extensions.Logging;
using NewLife;
using SqlSugar;
using System.ComponentModel;
using System.Xml;
using static SKIT.FlurlHttpClient.Wechat.Api.Models.ECFinderLiveGetFinderLiveNoticeRecordListResponse.Types;
using static SKIT.FlurlHttpClient.Wechat.TenpayV3.Models.CreateApplyForSubjectApplymentRequest.Types;

namespace Admin.NET.Application;

/// <summary>
/// 统计报表服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsStatisticalReportService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsImportOrder> _wmsImportOrderRep;
    private readonly SqlSugarRepository<WmsExportOrder> _wmsExportOrderRep;
    private readonly SqlSugarRepository<WmsImportTask> _wmsImportTaskRep;
    private readonly SqlSugarRepository<WmsBaseMaterial> _wmsBaseMaterialRep;
    private readonly SqlSugarRepository<WmsExportTask> _wmsExportTaskRep;
    private readonly SqlSugarRepository<WmsBaseSlot> _wmsBaseSlotRep;
    private readonly SqlSugarRepository<WmsBaseWareHouse> _wmsBaseWaareHouseRep;
    private readonly SqlSugarRepository<WmsStockSlotBoxInfo> _wmsStockSlotBoxInfoRep;
    private readonly SqlSugarRepository<WmsExportBoxInfo> _wmsExportBoxInfoRep;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly SysDictTypeService _sysDictTypeService;
    private readonly ILogger _logger;

    public WmsStatisticalReportService(
       SqlSugarRepository<WmsImportOrder> wmsImportOrderRep,
       SqlSugarRepository<WmsExportOrder> wmsExportOrderRep,
       SqlSugarRepository<WmsStockSlotBoxInfo> wmsStockSlotBoxInfoRep,
       SqlSugarRepository<WmsBaseMaterial> wmsBaseMaterialRep,
       SqlSugarRepository<WmsExportBoxInfo> wmsExportBoxInfoRep,
       ISqlSugarClient sqlSugarClient,
       SysDictTypeService sysDictTypeService,
       ILoggerFactory loggerFactory)
    {
        _wmsImportOrderRep = wmsImportOrderRep;
        _wmsExportOrderRep = wmsExportOrderRep;
        _wmsStockSlotBoxInfoRep = wmsStockSlotBoxInfoRep;
        _wmsBaseMaterialRep = wmsBaseMaterialRep;
        _sqlSugarClient = sqlSugarClient;
        _sysDictTypeService = sysDictTypeService;
        _wmsExportBoxInfoRep = wmsExportBoxInfoRep;
        _logger = loggerFactory.CreateLogger("统计报表");
    }

    /// <summary>
    /// 查询出入库报表信息 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("出入库报表信息")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsReportOutPut>> Page(WmsReportParamInput input)
    {
        List<WmsReportOutPut> ReportModel = new List<WmsReportOutPut>();
        input.Keyword = input.Keyword?.Trim();

        var V1 = _wmsImportOrderRep.AsQueryable() //入库流水表
          .LeftJoin<WmsStockSlotBoxInfo>((o, sb) => o.Id == sb.ImportOrderId) //库存箱码明细
          .LeftJoin<WmsBaseMaterial>((o, sb, bm) => bm.Id == sb.MaterialId) //物料信息
          .LeftJoin<WmsImportNotify>((o, sb, bm, notify) => o.ImportId == notify.Id) //入库单据
          .LeftJoin<WmsImportNotifyDetail>((o, sb, bm, notify, detail) => o.ImportDetailId == detail.Id) //入库单据明细
          .LeftJoin<WmsSysStockCode>((o, sb, bm, notify, detail, stock) => o.StockCodeId == stock.Id) //托盘管理
          .LeftJoin<WmsInspectNotity>((o, sb, bm, notify, detail, stock, notity) => sb.ImportId == notity.Id) //验收单据
          .LeftJoin<WmsBaseUnit>((o, sb, bm, notify, detail, stock, notity, unit) => bm.MaterialUnit == unit.Id) //计量单位
          .Where(o => o.IsDelete == false && o.ImportExecuteFlag == "03")

          .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), o => o.StockCode.Contains(input.Keyword))
          .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), (o, sb, bm, notify, detail, stock, notity, unit) => bm.MaterialCode == input.MaterialCode)
          .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialName), (o, sb, bm, notify, detail, stock, notity, unit) => bm.MaterialName.contains(input.MaterialName))

          .WhereIF(!string.IsNullOrWhiteSpace(input.StartDate) && input.ReportType == 1, o =>
              o.CompleteDate >= Convert.ToDateTime(input.StartDate + "-01-01") &&
              o.CompleteDate < Convert.ToDateTime(input.StartDate + "-01-01").AddYears(1))

          .WhereIF(!string.IsNullOrWhiteSpace(input.StartDate) && input.ReportType == 2, o =>
              o.CompleteDate >= Convert.ToDateTime(input.StartDate + "-01") &&
              o.CompleteDate < Convert.ToDateTime(input.StartDate + "-01").AddMonths(1))

          .WhereIF(!string.IsNullOrWhiteSpace(input.StartDate) && input.ReportType == 3, o =>
              o.CompleteDate >= Convert.ToDateTime(input.StartDate) &&
              o.CompleteDate <= Convert.ToDateTime(input.StartDate).AddHours(23).AddMinutes(59).AddSeconds(59))

          // 先选择需要的字段
          .Select((o, sb, bm, notify, detail, stock, notity, unit) => new
          {
              MaterialId = bm.Id,
              MaterialCode = bm.MaterialCode,
              MaterialName = bm.MaterialName,
              MaterialStandard = bm.MaterialStandard,
              UnitName = unit.UnitName,
              Qty = sb.Qty
          }).ToList();


        var importQuery = V1.GroupBy(x => new
        {
            x.MaterialId,
            x.MaterialCode,
            x.MaterialName,
            x.MaterialStandard,
            x.UnitName
        })
            .Select(g => new WmsReportOutPut()
            {
                Id = g.Key.MaterialId,
                MaterialCode = g.Key.MaterialCode,
                MaterialName = g.Key.MaterialName,
                MaterialStandard = g.Key.MaterialStandard,
                UnitName = g.Key.UnitName,
                Qty = g.Sum(x => x.Qty)
            });

        ReportModel = importQuery.ToList();

        var V2 = _wmsExportOrderRep.AsQueryable() //流水表
           .LeftJoin<WmsBaseBillType>((o, b) => o.ExportBillType.ToString() == b.BillTypeCode) //库存箱码明细
           .LeftJoin<WmsBaseMaterial>((o, b, m) => o.ExportMaterialId == m.Id) //物料信息
           .LeftJoin<WmsBaseUnit>((o, b, m, unit) => m.MaterialUnit == unit.Id) //计量单位
           .LeftJoin<WmsBaseWareHouse>((o, b, m, unit, warehouse) => o.ExportWarehouseId == warehouse.Id)
           .Where(o => o.IsDelete == false && o.ExportExecuteFlag == 2)

           .WhereIF(!string.IsNullOrWhiteSpace(input.WareHouseId), o => o.ExportWarehouseId.ToString() == input.WareHouseId)
           .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), o => o.ExportMaterialCode == input.MaterialCode)
           .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialName), o => o.ExportMaterialName.contains(input.MaterialName))
           .WhereIF(!string.IsNullOrWhiteSpace(input.StartDate) && input.ReportType == 1, o =>
              o.CompleteDate >= Convert.ToDateTime(input.StartDate + "-01-01") &&
              o.CompleteDate < Convert.ToDateTime(input.StartDate + "-01-01").AddYears(1))
           .WhereIF(!string.IsNullOrWhiteSpace(input.StartDate) && input.ReportType == 2, o =>
              o.CompleteDate >= Convert.ToDateTime(input.StartDate + "-01") &&
              o.CompleteDate < Convert.ToDateTime(input.StartDate + "-01").AddMonths(1))
           .WhereIF(!string.IsNullOrWhiteSpace(input.StartDate) && input.ReportType == 3, o =>
              o.CompleteDate >= Convert.ToDateTime(input.StartDate) &&
              o.CompleteDate <= Convert.ToDateTime(input.StartDate).AddHours(23).AddMinutes(59).AddSeconds(59))

           .Select((o, b, m, unit, warehouse) => new
           {
               o.ExportMaterialId,
               o.ExportMaterialCode,
               m.MaterialName,
               m.MaterialStandard,
               unit.UnitName,
               o.ScanQuantity
           }).ToList();

        var exportQuery = V2.GroupBy(x => new
        {
            x.ExportMaterialId,
            x.ExportMaterialCode,
            x.MaterialName,
            x.MaterialStandard,
            x.UnitName
        })
           .Select(g => new WmsReportOutPut()
           {
               Id = g.Key.ExportMaterialId.Value,
               MaterialCode = g.Key.ExportMaterialCode,
               MaterialName = g.Key.MaterialName,
               MaterialStandard = g.Key.MaterialStandard,
               UnitName = g.Key.UnitName,
               ExportQuantity = g.Sum(g => g.ScanQuantity)
           }).ToList();

        foreach (var c in exportQuery)
        {
            var rf = ReportModel.Where(m => m.MaterialId == c.Id.ToString()).FirstOrDefault();
            if (rf != null)
            {
                rf.ExportQuantity = c.ExportQuantity;
            }
            else
            {
                ReportModel.Add(new WmsReportOutPut { MaterialId = c.Id.ToString(), MaterialCode = c.MaterialCode, MaterialName = c.MaterialName, MaterialStandard = c.MaterialStandard, UnitName = c.UnitName, ImportQuantity = 0, ExportQuantity = c.ExportQuantity });
            }
        }
        return ReportModel.OrderBy(a => a.MaterialCode).ToPagedList(input.Page, input.PageSize);
    }

    /// <summary>
    /// 导出出入库报表信息 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出出入库报表信息")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(WmsReportParamInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<WmsReportExportOutPut>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "出入库报表导出记录");
    }

    /// <summary>
    /// 报表图表信息 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出出入库报表信息")]
    [ApiDescriptionSettings(Name = "Charts"), HttpPost]
    public async Task<ChartDataDto> Charts(WmsReportParamInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<WmsReportExportOutPut>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();

        var data = new ChartDataDto
        {
            //Title = input.ReportType == 3 ? $"{input.StartDate}出入库日报 - {input.ChartsType.GetDescription()}" :
            //input.ReportType == 2 ? $"{input.StartDate}出入库月报 - {input.ChartsType.GetDescription()}" :
            //input.ReportType == 1 ? $"{input.StartDate}出入库年报 - {input.ChartsType.GetDescription()}" :
            //                       $"出入库 - {input.ChartsType.GetDescription()}",
            //ChartType = input.ChartsType,
            Title = input.ReportType == 3 ? $"{input.StartDate}出入库日报" :
            input.ReportType == 2 ? $"{input.StartDate}出入库月报" :
            input.ReportType == 1 ? $"{input.StartDate}出入库年报" :
                                   $"出入库 ",
            ChartType = input.ChartsType,
            //Labels = input.ChartsType == ChartType.Pie ? null :
            //     list.Select(c=>c.MaterialName).Distinct().ToList(),
            Labels = input.ChartsType == ChartType.Pie ? null :
                list.Select(c => c.MaterialName).ToList(),
            xAxisData = list.Select(c => c.MaterialName).ToList(),
            Series = new List<ChartSeries>()
        };

        var inboundData = new List<object>();
        var outboundData = new List<object>();
        switch (input.ChartsType)
        {
            case ChartType.Bar:

                foreach (var item in list)
                {
                    inboundData.Add(item.Qty);
                    outboundData.Add(item.ExportQuantity);
                }
                data.Series.Add(new ChartSeries
                {
                    Name = "入库量",
                    Type = "bar",
                    Data = inboundData
                });
                data.Series.Add(new ChartSeries
                {
                    Name = "出库量",
                    Type = "bar",
                    Data = outboundData
                });
                break;

            case ChartType.Line:
                foreach (var item in list)
                {
                    inboundData.Add(item.Qty);
                    outboundData.Add(item.ExportQuantity);
                }

                data.Series.Add(new ChartSeries
                {
                    Name = "入库量",
                    Type = "line",
                    Data = inboundData,
                    Options = new Dictionary<string, object>
                    {
                        { "smooth", true },
                        { "lineStyle", new { width = 3 } }
                    }
                });
                data.Series.Add(new ChartSeries
                {
                    Name = "出库量",
                    Type = "line",
                    Data = outboundData,
                    Options = new Dictionary<string, object>
                    {
                        { "smooth", true },
                        { "lineStyle", new { width = 3 } }
                    }
                });
                break;

            case ChartType.Pie:
                if (list.Select(c => c.MaterialName).Count() > 1)
                {
                    foreach (var item in list)
                    {
                        inboundData.Add(new { name = item.MaterialName, value = !string.IsNullOrEmpty(item.Qty) ? item.Qty : "0" });
                        outboundData.Add(new { name = item.MaterialName, value = !string.IsNullOrEmpty(item.ExportQuantity) ? item.Qty : "0" });
                    }

                    data.Series.Add(new ChartSeries
                    {
                        Name = "入库占比",
                        Type = "pie",
                        Data = inboundData
                    });
                    data.Series.Add(new ChartSeries
                    {
                        Name = "出库占比",
                        Type = "pie",
                        Data = outboundData
                    });
                }
                else
                {
                    data.Series.Add(new ChartSeries
                    {
                        Name = "出入库占比",
                        Type = "pie",
                        Data = new List<object>
                        {
                            new { name = "入库量", value = list.FirstOrDefault().Qty },
                            new { name = "出库量", value = list.FirstOrDefault().ExportQuantity }
                        }
                    });
                }

                break;
        }

        return data;
    }

    /// <summary>
    /// 入库流水总单 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("入库流水总单")]
    [ApiDescriptionSettings(Name = "GetSearchImOrder"), HttpPost]
    public async Task<SqlSugarPagedList<WmsOrderEntityOutPut>> GetSearchImOrder(WmsOrderParamInput input)
    {
        try
        {
            #region JC35
            List<long> Ids = new List<long>();
            if (!string.IsNullOrEmpty(input.SlotCode))
            {
                Ids = _wmsImportOrderRep.AsQueryable()
                    .Where(s => s.IsDelete == false)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.SlotCode), s => s.ImportSlotCode.Contains(input.SlotCode))
                    .Select(s => s.Id).ToList();
            }

            var into = _wmsStockSlotBoxInfoRep.AsQueryable()
             .LeftJoin<WmsBaseMaterial>((o, mater) => o.MaterialId == mater.Id)//物料信息
             .LeftJoin<WmsImportNotify>((o, mater, notify) => o.ImportId == notify.Id)//入库单据
             .LeftJoin<WmsImportNotifyDetail>((o, mater, notify, detail) => o.ImportDetailId == detail.Id)//入库单据明细
             .LeftJoin<WmsSysStockCode>((o, mater, notify, detail, stock) => o.StockCodeId == stock.Id)//托盘管理
             .LeftJoin<WmsImportOrder>((o, mater, notify, detail, stock, order) => o.ImportOrderId == order.Id)//入库流水
             .LeftJoin<WmsInspectNotity>((o, mater, notify, detail, stock, order, innotify) => o.ImportId == innotify.Id)//验收单据表
             .LeftJoin<WmsBaseWareHouse>((o, mater, notify, detail, stock, order, innotify, warehouse) => mater.WarehouseId == warehouse.Id)//仓库表
             .LeftJoin<WmsBaseUnit>((o, mater, notify, detail, stock, order, innotify, warehouse, bm) => mater.MaterialUnit == bm.Id) //计量单位

             .Where((o, mater, notify, detail, stock, order, innotify, warehouse, bm) => o.IsDelete == false && order.IsDelete == false)

             .WhereIF(Ids.Count > 0, (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => Ids.Contains(o.ImportOrderId.Value))

             .WhereIF(input.MaterialId.HasValue && input.MaterialId.Value > 0, (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => mater.Id==input.MaterialId)
             .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => mater.MaterialCode.Contains(input.MaterialCode))
             .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialName), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => mater.MaterialName == input.MaterialName)
             .WhereIF(!string.IsNullOrWhiteSpace(input.WareHouseId), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => SqlFunc.ToString(mater.WarehouseId) == input.WareHouseId)
             .WhereIF(!string.IsNullOrWhiteSpace(input.StartDate), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => o.CreateTime >= Convert.ToDateTime(input.StartDate))
             .WhereIF(!string.IsNullOrWhiteSpace(input.EndDate), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => o.CreateTime < Convert.ToDateTime(input.EndDate).AddDays(1))
             .WhereIF(!string.IsNullOrWhiteSpace(input.ImportExecuteFlag), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => detail.ImportExecuteFlag == input.ImportExecuteFlag)
             .WhereIF(string.IsNullOrWhiteSpace(input.ImportExecuteFlag), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => detail.ImportExecuteFlag == "03" || detail.ImportExecuteFlag == "04")
             .GroupBy((o, mater, notify, detail, stock, order, innotify, warehouse, bm) => new { mater.Id, mater.MaterialCode, mater.MaterialName, mater.MaterialStandard, bm.UnitName })
             .Select((o, mater, notify, detail, stock, order, innotify, warehouse, bm) => new WmsOrderEntityOutPut
             {
                 MaterialId = mater.Id,
                 MaterialCode = mater.MaterialCode,
                 MaterialName = mater.MaterialName,
                 MaterialStandard = mater.MaterialStandard,
                 UnitName = bm.UnitName,
                 Qty = SqlFunc.AggregateSum(o.Qty),
             })
             .ToList();
            #endregion

            #region JC36
            //var into = _wmsStockSlotBoxInfoRep.AsQueryable()
            // .LeftJoin<WmsBaseMaterial>((o, mater) => o.MaterialId == mater.Id)//物料信息
            // .LeftJoin<WmsBaseUnit>((o, mater,bm) => mater.MaterialUnit == bm.Id) //计量单位
            // .LeftJoin<WmsBaseWareHouse>((o, mater, bm, warehouse) => mater.WarehouseId == warehouse.Id)//仓库表
            // .Where((o, mater, bm) => o.IsDelete == false)
            // .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), (o, mater, bm) => mater.MaterialCode.Contains(input.MaterialCode))
            // .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialName), (o, mater, bm) => mater.MaterialName == input.MaterialName)
            // .WhereIF(!string.IsNullOrWhiteSpace(input.WareHouseId), (o, mater, bm, warehouse) => SqlFunc.ToString(mater.WarehouseId) == input.WareHouseId)
            // .WhereIF(!string.IsNullOrWhiteSpace(input.StartDate), (o, mater, bm, warehouse) => o.CreateTime >= Convert.ToDateTime(input.StartDate))
            // .WhereIF(!string.IsNullOrWhiteSpace(input.EndDate), (o, mater, bm, warehouse) => o.CreateTime < Convert.ToDateTime(input.EndDate).AddDays(1))
            // .GroupBy((o, mater, bm, warehouse) => new { mater.Id, mater.MaterialCode, mater.MaterialName, mater.MaterialStandard, bm.UnitName })
            // .Select((o, mater, bm, warehouse) => new WmsOrderEntityOutPut
            // {
            //     MaterialId = mater.Id,
            //     MaterialCode = mater.MaterialCode,
            //     MaterialName = mater.MaterialName,
            //     MaterialStandard = mater.MaterialStandard,
            //     UnitName = bm.UnitName,
            //     Qty = SqlFunc.AggregateSum(o.Qty),
            // })
            // .ToList();
            #endregion
            return into.OrderBy(a => a.MaterialCode).ToPagedList(input.Page, input.PageSize);
        }
        catch (Exception e)
        {
            throw new Exception($"查询入库流水总单失败：{e.Message}");
        }
    }

    /// <summary>
    /// 入库流水中得的明细
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [DisplayName("入库流水中得的明细")]
    [ApiDescriptionSettings(Name = "GetSearchImOrderDetail"), HttpPost]
    public async Task<SqlSugarPagedList<ImOrderDto>> GetSearchImOrderDetail(WmsOrderDetailParamInput input)
    {
        try
        {
            List<long> Ids = new List<long>();
            if (!string.IsNullOrEmpty(input.SlotCode))
            {
                Ids = _wmsImportOrderRep.AsQueryable()
                    .Where(s => s.IsDelete == false)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.SlotCode), s => s.ImportSlotCode.Contains(input.SlotCode))
                    .Select(s => s.Id).ToList();
            }

            var into = _wmsStockSlotBoxInfoRep.AsQueryable()
             .LeftJoin<WmsBaseMaterial>((o, mater) => o.MaterialId == mater.Id)//物料信息
             .LeftJoin<WmsImportNotify>((o, mater, notify) => o.ImportId == notify.Id)//入库单据
             .LeftJoin<WmsImportNotifyDetail>((o, mater, notify, detail) => o.ImportDetailId == detail.Id)//入库单据明细
             .LeftJoin<WmsSysStockCode>((o, mater, notify, detail, stock) => o.StockCodeId == stock.Id)//托盘管理
             .LeftJoin<WmsImportOrder>((o, mater, notify, detail, stock, order) => o.ImportOrderId == order.Id)//入库流水
             .LeftJoin<WmsInspectNotity>((o, mater, notify, detail, stock, order, innotify) => o.ImportId == innotify.Id)//验收单据表
             .LeftJoin<WmsBaseWareHouse>((o, mater, notify, detail, stock, order, innotify, warehouse) => mater.WarehouseId == warehouse.Id)//仓库表
             .LeftJoin<WmsBaseUnit>((o, mater, notify, detail, stock, order, innotify, warehouse, bm) => mater.MaterialUnit == bm.Id) //计量单位

             .WhereIF(Ids.Count > 0, (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => Ids.Contains(o.ImportOrderId.Value))

             .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => mater.MaterialCode.Contains(input.MaterialCode))

             .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialName), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => mater.MaterialName == input.MaterialName)

             .WhereIF(!string.IsNullOrWhiteSpace(input.ImportNo), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => notify.ImportBillCode == input.ImportNo)

             .WhereIF(!string.IsNullOrWhiteSpace(input.LotNo), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => o.LotNo == input.LotNo)

             .WhereIF(!string.IsNullOrWhiteSpace(input.SlotCode), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => order.ImportSlotCode == input.SlotCode)

             .WhereIF(!string.IsNullOrWhiteSpace(input.StockCode), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => stock.StockCode == input.StockCode)

              .WhereIF(!string.IsNullOrWhiteSpace(input.ImportExecuteFlag), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => detail.ImportExecuteFlag == input.ImportExecuteFlag)
              .WhereIF(string.IsNullOrWhiteSpace(input.ImportExecuteFlag), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => detail.ImportExecuteFlag == "03" || detail.ImportExecuteFlag == "04")

             .WhereIF(!string.IsNullOrWhiteSpace(input.StartDate), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => o.CreateTime >= Convert.ToDateTime(input.StartDate))
             .WhereIF(!string.IsNullOrWhiteSpace(input.EndDate), (o, mater, notify, detail, stock, order, innotify, warehouse, bm) => o.CreateTime < Convert.ToDateTime(input.EndDate).AddDays(1))
              
             .GroupBy((o, mater, notify, detail, stock, order, innotify, warehouse, bm) => new { order.Id, order.ImportOrderNo, order.StockCode, order.ImportSlotCode, notify.ImportBillCode, mater.MaterialCode, mater.MaterialName, mater.MaterialStandard, bm.UnitName, detail.ImportExecuteFlag, order.CreateTime, order.CompleteDate, o.ValidateDay })
              .Select((o, mater, notify, detail, stock, order, innotify, warehouse, bm) => new ImOrderDto
              {
                  Id = order.Id.ToString(),
                  ImportOrderNo = order.ImportOrderNo,
                  ImportNo = notify.ImportBillCode,
                  StockCode = order.StockCode,
                  SlotCode = order.ImportSlotCode,
                  Status = detail.ImportExecuteFlag == "00" ? "待执行"
                    : detail.ImportExecuteFlag == "01" ? "正在执行"
                    : detail.ImportExecuteFlag == "02" ? "已完成"
                    : detail.ImportExecuteFlag == "03" ? "已上传"
                    : "",
                  CreateTime = order.CreateTime,
                  CompleteTime = order.CompleteDate
              }).ToList();


            return into.OrderBy(a => a.CreateTime).ToPagedList(input.Page, input.PageSize);
        }
        catch (Exception e)
        {
            throw new Exception($"查询入库流水明细失败：{e.Message}");
        }
    }
    
    /// <summary>
    /// 入库流水箱码信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [DisplayName("入库流水箱码信息")]
    [ApiDescriptionSettings(Name = "GetSearchImOrderBoxInfo"), HttpPost]
    public async Task<SqlSugarPagedList<OrderBoxDto>> GetSearchImOrderBoxInfo(WmsImOrderBoxParamInput input)
    {
        try
        {
            List<long> Ids = new List<long>();
            if (!string.IsNullOrEmpty(input.SlotCode))
            {
                Ids = _wmsImportOrderRep.AsQueryable()
                    .Where(s => s.IsDelete == false)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.SlotCode), s => s.ImportSlotCode.Contains(input.SlotCode))
                    .Select(s => s.Id).ToList();
            }

            var into = _wmsStockSlotBoxInfoRep.AsQueryable()
             .LeftJoin<WmsBaseMaterial>((o, material) => o.MaterialId == material.Id) //物料信息表
             .LeftJoin<WmsImportNotifyDetail>((o, material, detail) => o.ImportDetailId == detail.Id)//入库单据明细
             .LeftJoin<WmsSysStockCode>((o, material, detail, stock) => o.StockCode == SqlFunc.ToString(stock.Id))//入库单据
             .LeftJoin<WmsImportNotify>((o, material, detail, stock, notify) => o.ImportId == notify.Id)//入库单据
             .LeftJoin<WmsImportOrder>((o, material, detail, stock, notify, order) => o.ImportOrderId == order.Id)//入库单据
             .LeftJoin<WmsInspectNotity>((o, material, detail, stock, notify, order, innotify) => o.ImportId == innotify.Id)//入库单据
             .LeftJoin<WmsBaseUnit>((o, material, detail, stock, notify, order, innotify, unit) => material.MaterialUnit == unit.Id) //巷道表             
             .Where(o => o.IsDelete == false)
             .WhereIF(Ids.Count > 0, (o, material, detail, stock, notify, order, innotify, unit) => Ids.Contains(o.ImportOrderId.Value))
             //.WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), (o, material, detail, stock, notify, order, innotify,unit) => material.MaterialCode.Contains(input.MaterialCode))
             //.WhereIF(!string.IsNullOrWhiteSpace(input.MaterialName), (o, material, detail, stock, notify, order, innotify,unit) => material.MaterialName == input.MaterialName)
             //.WhereIF(!string.IsNullOrWhiteSpace(input.LotNo), (o, material, detail, stock, notify, order, innotify,unit) => o.LotNo == input.LotNo)
             //.WhereIF(!string.IsNullOrWhiteSpace(input.SlotCode), (o, material, detail, stock, notify, order, innotify,unit) => order.ImportSlotCode == input.SlotCode)
             .WhereIF(!string.IsNullOrWhiteSpace(input.StockCode), (o, material, detail, stock, notify, order, innotify, unit) => stock.StockCode == input.StockCode)
             .WhereIF(!string.IsNullOrWhiteSpace(input.BoxCode), (o, material, detail, stock, notify, order, innotify, unit) => o.BoxCode == input.BoxCode)
             .WhereIF(!string.IsNullOrWhiteSpace(input.StartDate), (o, material, detail, stock, notify, order, innotify, unit) => o.CreateTime >= Convert.ToDateTime(input.StartDate))
             .WhereIF(!string.IsNullOrWhiteSpace(input.EndDate), (o, material, detail, stock, notify, order, innotify, unit) => o.CreateTime < Convert.ToDateTime(input.EndDate).AddDays(1))
             .WhereIF(!string.IsNullOrWhiteSpace(input.ImportOrderNo), (o, material, detail, stock, notify, order, innotify, unit) => order.ImportOrderNo == input.ImportOrderNo)
             .Select((o, material, detail, stock, notify, order, innotify, unit) => new OrderBoxDto
             {
                 Id = o.Id,
                 BoxCode = o.BoxCode,
                 StockCodeId = o.StockCodeId,
                 Qty = o.Qty,
                 ImportOrderId = o.ImportOrderId,
                 Status = o.Status,
                 BoxLevel = o.BoxLevel,
                 LotNo = o.LotNo,
                 IsDelete = o.IsDelete,
                 CreateTime = o.CreateTime,
                 BulkTank = o.BulkTank,
                 ProductionDate = o.ProductionDate,
                 ValidateDay = o.ValidateDay,
                 MaterialId = o.MaterialId,
                 IsSamplingBox = o.IsSamplingBox,
                 SamplingDate = o.SamplingDate,
                 StaffCode = o.StaffCode,
                 StaffName = o.StaffName,
                 Weight = o.Weight,
                 ReasonsForExcl = o.ReasonsForExcl,
                 plasmaRejectTypeId = o.plasmaRejectTypeId,
                 ExtractStatus = o.ExtractStatus,
                 InspectionStatus = o.InspectionStatus,
                 PickingSlurry = o.PickingSlurry,
                 ImportDetailId = o.ImportDetailId,
                 ImportId = o.ImportId,
                 StockCode = o.StockCode,
                 ImportOrderNo = order.ImportOrderNo,
                 MaterialCode = material.MaterialCode,
                 MaterialName = material.MaterialName,
                 ImportExecuteFlag = detail.ImportExecuteFlag,
                 ImportQuantity = detail.ImportQuantity,
                 ImportFactQuantity = detail.ImportFactQuantity,
                 ImportBillCode = notify.ImportBillCode,
                 InspectBillCode = innotify.InspectBillCode,
                 MaterialStandard = material.MaterialStandard,
                 UnitName = unit.UnitName,
                 BoxQuantity = material.BoxQuantity
             });

            var into1 = into.ToList();
            return into.OrderBy(o => o.CreateTime).ToPagedList(input.Page, input.PageSize);
        }
        catch (Exception e)
        {
            throw new Exception($"查询入库流水明细失败：{e.Message}");
        }
    }

    /// <summary>
    /// 获取出库中流水总单数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [DisplayName("获取出库中流水总单数据")]
    [ApiDescriptionSettings(Name = "GetSearchExOrder"), HttpPost]
    public async Task<SqlSugarPagedList<WmsOrderEntityOutPut>> GetSearchExOrder(WmsExOrderParamInput input)
    {
        try
        {
            var into = _wmsExportOrderRep.AsQueryable()
             .LeftJoin<WmsBaseMaterial>((o, material) => o.ExportMaterialId == material.Id)//物料信息
             .LeftJoin<WmsBaseWareHouse>((o, material, warehouse) => material.WarehouseId == warehouse.Id)//仓库表
             .LeftJoin<WmsBaseUnit>((o, material, warehouse, bm) => material.MaterialUnit == bm.Id) //计量单位
             .Where(o => o.IsDelete == false && o.ExportExecuteFlag>=2)
             .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), (o, material, warehouse, bm) => material.MaterialCode.Contains(input.MaterialCode))
             .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialName), (o, material, warehouse, bm) => material.MaterialName == input.MaterialName)
             .WhereIF(!string.IsNullOrWhiteSpace(input.WareHouseId), (o, material, warehouse, bm) => SqlFunc.ToString(material.WarehouseId) == input.WareHouseId)
              .WhereIF(!string.IsNullOrWhiteSpace(input.StartDate), (o, material, warehouse, bm) => o.CreateTime >= Convert.ToDateTime(input.StartDate))
             .WhereIF(!string.IsNullOrWhiteSpace(input.EndDate), (o, material, warehouse, bm) => o.CreateTime < Convert.ToDateTime(input.EndDate).AddDays(1))
            .GroupBy((o, material, warehouse, bm) => new { material.Id, material.MaterialName,material.MaterialCode,material.MaterialStandard,bm.UnitName})
            .Select((o, material, warehouse, bm) =>new WmsOrderEntityOutPut
            {
                MaterialId = material.Id,
                MaterialCode = material.MaterialCode,
                MaterialName = material.MaterialName,
                MaterialStandard = material.MaterialStandard,
                UnitName = bm.UnitName,
                Qty = SqlFunc.AggregateSum(o.ScanQuantity)
            }).ToList();

            return into.OrderBy(a => a.MaterialCode).ToPagedList(input.Page, input.PageSize);
        }
        catch (Exception e)
        {
            throw new Exception($"查询入库流水总单失败：{e.Message}");
        }
    }

    /// <summary>
    /// 获取出库中流水明细数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [DisplayName("获取出库中流水明细数据")]
    [ApiDescriptionSettings(Name = "GetSearchExOrderDetail"), HttpPost]
    public async Task<SqlSugarPagedList<ExOrderDetailDto>> GetSearchExOrderDetail(WmsExOrderDetailParamInput input)
    {
        try
        {
            var into = _wmsExportOrderRep.AsQueryable()
             .Where(s => s.IsDelete == false && s.ExportExecuteFlag >= 2)
             .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), s => s.ExportMaterialCode == input.MaterialCode)
             .WhereIF(!string.IsNullOrWhiteSpace(input.LotNo), s => s.ExportLotNo == input.LotNo)
             .WhereIF(!string.IsNullOrWhiteSpace(input.ExBillCode), s => s.ExportBillCode == input.ExBillCode)
             .WhereIF(!string.IsNullOrWhiteSpace(input.StockCode), s => s.ExportStockCode == input.StockCode)
             .WhereIF(!string.IsNullOrWhiteSpace(input.SlotCode), s => s.ExportSlotCode == input.SlotCode)
             .WhereIF(!string.IsNullOrWhiteSpace(input.StartDate), s => s.CreateTime >= Convert.ToDateTime(input.StartDate))
             .WhereIF(!string.IsNullOrWhiteSpace(input.EndDate), s => s.CreateTime < Convert.ToDateTime(input.EndDate).AddDays(1))
             .Select(s => new ExOrderDetailDto
             {
                 ExportOrderNo = s.ExportOrderNo,
                 ExportBillCode = s.ExportBillCode,
                 ExportMaterialCode = s.ExportMaterialCode,
                 ExportMaterialName = s.ExportMaterialName,
                 ExportStockCode = s.ExportStockCode,
                 ExportSlotCode = s.ExportSlotCode,
                 ExportLotNo = s.ExportLotNo,
                 PickedNum = s.ScanQuantity,
                 ExportExecuteFlag = s.ExportExecuteFlag == 0 ? "待执行"
                    : s.ExportExecuteFlag == 1 ? "正在执行"
                    : s.ExportExecuteFlag == 2 ? "已完成"
                    : s.ExportExecuteFlag == 3 ? "已上传"
                    : "",
                 CreateTime = s.CreateTime,
                 CompleteDate = s.CompleteDate
             });

            var into1 = into.ToList();
            return into.OrderBy(a => a.CreateTime).ToPagedList(input.Page, input.PageSize);
        }
        catch (Exception e)
        {
            throw new Exception($"查询入库流水明细失败：{e.Message}");
        }
    }

    /// <summary>
    /// 获取出库流水箱码数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [DisplayName("获取出库流水箱码数据")]
    [ApiDescriptionSettings(Name = "GetSearchExOrderBoxInfo"), HttpPost]
    public async Task<SqlSugarPagedList<ExOrderBoxDto>> GetSearchExOrderBoxInfo(WmsExOrderBoxParamInput input)
    {
        try
        {
            var into = _wmsExportBoxInfoRep.AsQueryable()
             .LeftJoin<WmsExportOrder>((o, order) => o.ExportOrderNo == order.ExportOrderNo) //物料信息表
             .LeftJoin<WmsBaseMaterial>((o, order, material) => o.MaterialId == SqlFunc.ToString(material.Id)) //物料信息表
             .LeftJoin<WmsBaseUnit>((o, order, material, unit) => material.MaterialUnit == unit.Id) //计量单位             
            .Where((o, order, material, unit) => o.IsDelete == false && o.Status == 1)
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), (o, order, material, unit) => material.MaterialCode.Contains(input.MaterialCode))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialName), (o, order, material, unit) => material.MaterialName.Contains(input.MaterialName))
            .WhereIF(!string.IsNullOrWhiteSpace(input.BoxCode), (o, order, material, unit) => o.BoxCode.Contains(input.BoxCode))
            .WhereIF(!string.IsNullOrWhiteSpace(input.LotNo), (o, order, material, unit) => o.LotNo.Contains(input.LotNo))
            .WhereIF(!string.IsNullOrWhiteSpace(input.SlotCode), (o, order, material, unit) => order.ExportSlotCode.Contains(input.SlotCode))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StockCode), (o, order, material, unit) => o.StockCodeCode.Contains(input.StockCode))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExBillCode), (o, order, material, unit) => order.ExportBillCode.Contains(input.ExBillCode))
            .Select((o, order, material, unit) => new ExOrderBoxDto
            {
                Id = o.Id,
                MaterialName = material.MaterialName,
                BoxCode = o.BoxCode,
                StockCode = order.ExportStockCode,
                Qty = o.Qty,
                ExportOrderNo = o.ExportOrderNo,
                Status = o.Status,
                IsOut = o.IsOut,
                LotNo = o.LotNo,
                MaterialId = o.MaterialId,
                AddDate = o.AddDate,
                ProductionDate = o.ProductionDate,
                ExportLoseDate = o.ExportLoseDate,
                IsDel = o.IsDelete,
                MaterialCode = material.MaterialCode,
                PickNum = o.PickNum,
                IsMustExport = o.IsMustExport,
                MaterialStandard = material.MaterialStandard,
                UnitName = unit.UnitName,
                PickEdNum = o.PickEdNum
            });

            return into.OrderBy(o => o.AddDate).ToPagedList(input.Page, input.PageSize);
        }
        catch (Exception e)
        {
            throw new Exception($"查询入库流水明细失败：{e.Message}");
        }
    }

    /// <summary>
    /// 导出入库流水信息 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出入库流水信息")]
    [ApiDescriptionSettings(Name = "ImExport"), HttpPost, NonUnify]
    public async Task<IActionResult> ImExport(WmsImReportParamInput input)
    {
        switch (input.activeName)
        {
            case "first":
                var list = (await GetSearchImOrder(new WmsOrderParamInput 
                { 
                    WareHouseId = input.WareHouseId,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    MaterialCode = input.MaterialCode,
                    MaterialName = input.MaterialName,
                    ImportExecuteFlag = input.ImportExecuteFlag,
                    LotNo = input.LotNo,
                    StockCode = input.StockCode,
                    SlotCode = input.SlotCode,
                    Page = input.Page,
                    PageSize = input.PageSize
                })).Items?.Adapt<List<WmsOrderExport>>() ?? new();
                if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.MaterialId)).ToList();
                return ExcelHelper.ExportTemplate(list, "入库流水总单导出记录");
            case "second":
                var list1 = (await GetSearchImOrderDetail(new WmsOrderDetailParamInput 
                {
                    SlotCode = input.SlotCode,
                    MaterialCode = input.MaterialCode,
                    MaterialName = input.MaterialName,
                    LotNo = input.LotNo,
                    ImportNo = input.ImportNo,
                    StockCode = input.StockCode,
                    ImportExecuteFlag = input.ImportExecuteFlag,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    Page = input.Page,
                    PageSize = input.PageSize
                })).Items?.Adapt<List<WmsOrderDetailExport>>() ?? new();
                if (input.SelectKeyList?.Count > 0) list1 = list1.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
                return ExcelHelper.ExportTemplate(list1, "入库流水明细导出记录");
            case "third":
                var list2 = (await GetSearchImOrderBoxInfo(new WmsImOrderBoxParamInput 
                {
                    ImportOrderNo = input.ImportOrderNo,
                    StockCode = input.StockCode,
                    SlotCode = input.SlotCode,
                    BoxCode = input.BoxCode,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    Page = input.Page,
                    PageSize = input.PageSize
                })).Items?.Adapt<List<WmsOrderBoxExport>>() ?? new();
                if (input.SelectKeyList?.Count > 0) list2 = list2.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
                return ExcelHelper.ExportTemplate(list2, "入库流水箱码导出记录");
            default:
                return null;
        }
    }

    /// <summary>
    /// 导出出库流水信息 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出出库流水信息")]
    [ApiDescriptionSettings(Name = "ExExport"), HttpPost, NonUnify]
    public async Task<IActionResult> ExExport(WmsExReportParamInput input)
    {
        switch (input.activeName)
        {
            case "first":
                var list = (await GetSearchExOrder(new WmsExOrderParamInput 
                { 
                    WareHouseId = input.WareHouseId,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    MaterialCode = input.MaterialCode,
                    MaterialName = input.MaterialName,
                    Page = input.Page,
                    PageSize = input.PageSize
                })).Items?.Adapt<List<WmsExportOrderExport>>() ?? new();
                if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.MaterialId)).ToList();
                return ExcelHelper.ExportTemplate(list, "出库流水总单导出记录");
            case "second":
                var list1 = (await GetSearchExOrderDetail(new WmsExOrderDetailParamInput 
                {
                    MaterialCode = input.MaterialCode,
                    LotNo = input.LotNo,
                    ExBillCode = input.ExBillCode,
                    StockCode = input.StockCode,
                    SlotCode = input.SlotCode,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    Page = input.Page,
                    PageSize = input.PageSize
                })).Items?.Adapt<List<WmsExportOrderDetailExport>>() ?? new();
                if (input.SelectKeyList?.Count > 0) list1 = list1.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
                return ExcelHelper.ExportTemplate(list1, "出库流水明细导出记录");
            case "third":
                var list2 = (await GetSearchExOrderBoxInfo(new WmsExOrderBoxParamInput 
                {
                    MaterialCode = input.MaterialCode,
                    MaterialName = input.MaterialName,
                    BoxCode = input.BoxCode,
                    LotNo = input.LotNo,
                    SlotCode = input.SlotCode,
                    StockCode = input.StockCode,
                    ExBillCode = input.ExBillCode,
                    Page = input.Page,
                    PageSize = input.PageSize
                })).Items?.Adapt<List<WmsExportOrderBoxExport>>() ?? new();
                if (input.SelectKeyList?.Count > 0) list2 = list2.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
                return ExcelHelper.ExportTemplate(list2, "出库流水箱码导出记录");
            default:
                return null;
        }
    }
}
