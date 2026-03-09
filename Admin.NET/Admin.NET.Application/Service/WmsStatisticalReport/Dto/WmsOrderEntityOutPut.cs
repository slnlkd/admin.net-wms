// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Magicodes.ExporterAndImporter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SKIT.FlurlHttpClient.Wechat.Api.Models.ECFinderLiveGetFinderLiveNoticeRecordListResponse.Types;

namespace Admin.NET.Application;
public class WmsOrderEntityOutPut
{
    public long MaterialId { get; set; }
    public string MaterialCode { get; set; }   
    public string MaterialName { get; set; }
    public string MaterialStandard { get; set; }
    public string UnitName { get; set; }
    public decimal? Qty { get; set; }
    public string BsaeAreaName { get; set; }
}

public class ImOrderDto
{
    public string Id { get; set; }
    public string ImportOrderNo { get; set; }
    public string ImportNo { get; set; }
    public string StockCode { get; set; }
    public string SlotCode { get; set; }
    public string Status { get; set; }
    public DateTime? CreateTime { get; set; }
    public DateTime? CompleteTime { get; set; }
}

public class OrderBoxDto
{
    public long Id { get; set; }
    public string BoxCode { get; set; }
    public long? StockCodeId { get; set; }
    public decimal? Qty { get; set; }
    public long? ImportOrderId { get; set; }
    public int? Status { get; set; }
    public int? BoxLevel { get; set; }
    public string LotNo { get; set; }
    public bool IsDelete { get; set; }
    public DateTime? CreateTime { get; set; }
    public int? BulkTank { get; set; }
    public DateTime? ProductionDate { get; set; }
    public DateTime? ValidateDay { get; set; }
    public long? MaterialId { get; set; }
    public int? IsSamplingBox { get; set; }
    public DateTime? SamplingDate { get; set; }
    public string StaffCode { get; set; }
    public string StaffName { get; set; }
    public decimal? Weight { get; set; }
    public string ReasonsForExcl { get; set; }
    public string plasmaRejectTypeId { get; set; }
    public int? ExtractStatus { get; set; }
    public int? InspectionStatus { get; set; }
    public string PickingSlurry { get; set; }
    public long? ImportDetailId { get; set; }
    public long? ImportId { get; set; }
    public string StockCode { get; set; }
    public string ImportOrderNo { get; set; }
    public string MaterialCode { get; set; }
    public string MaterialName { get; set; }
    public string ImportExecuteFlag { get; set; }
    public decimal? ImportQuantity { get; set; }
    public decimal? ImportFactQuantity { get; set; }
    public string Reamrk { get; set; }
    public string ImportBillCode { get; set; }
    public string InspectBillCode { get; set; }
    //J.ExtractBillCode{ get; set; }
    public string MaterialStandard { get; set; }
    public string UnitName { get; set; }
    public string BoxQuantity { get; set; }
}


public class WmsOrderExport
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long Id { get; set; }
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long MaterialId { get; set; }
    /// <summary>
    /// 物料编码
    /// </summary>
    [ImporterHeader(Name = "*物料编码")]
    [ExporterHeader("*物料编码", Format = "", Width = 25, IsBold = true)]
    public string MaterialCode { get; set; }
    /// <summary>
    /// 物料名称
    /// </summary>
    [ImporterHeader(Name = "*物料名称")]
    [ExporterHeader("*物料名称", Format = "", Width = 25, IsBold = true)]
    public string MaterialName { get; set; }
    /// <summary>
    /// 物料规格
    /// </summary>
    [ImporterHeader(Name = "*物料规格")]
    [ExporterHeader("*物料规格", Format = "", Width = 25, IsBold = true)]
    public string MaterialStandard { get; set; }
    /// <summary>
    /// 计量单位
    /// </summary>
    [ImporterHeader(Name = "*计量单位")]
    [ExporterHeader("*计量单位", Format = "", Width = 25, IsBold = true)]
    public string UnitName { get; set; }
    /// <summary>
    /// 入库数量
    /// </summary>
    [ImporterHeader(Name = "*入库数量")]
    [ExporterHeader("*入库数量", Format = "", Width = 25, IsBold = true)]
    public decimal? Qty { get; set; }
}

public class WmsOrderDetailExport
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long Id { get; set; }
    /// <summary>
    /// 入库流水号
    /// </summary>
    [ImporterHeader(Name = "*入库流水号")]
    [ExporterHeader("*入库流水号", Format = "", Width = 25, IsBold = true)]
    public string ImportOrderNo { get; set; }
    /// <summary>
    /// 入库单号
    /// </summary>
    [ImporterHeader(Name = "*入库单号")]
    [ExporterHeader("*入库单号", Format = "", Width = 25, IsBold = true)]
    public string ImportNo { get; set; }
    /// <summary>
    /// 载具条码
    /// </summary>
    [ImporterHeader(Name = "*载具条码")]
    [ExporterHeader("*载具条码", Format = "", Width = 25, IsBold = true)]
    public string StockCode { get; set; }
    /// <summary>
    /// 储位地址
    /// </summary>
    [ImporterHeader(Name = "*储位地址")]
    [ExporterHeader("*储位地址", Format = "", Width = 25, IsBold = true)]
    public string SlotCode { get; set; }
    /// <summary>
    /// 执行状态
    /// </summary>
    [ImporterHeader(Name = "*执行状态")]
    [ExporterHeader("*执行状态", Format = "", Width = 25, IsBold = true)]
    public string Status { get; set; }
    /// <summary>
    /// 添加时间
    /// </summary>
    [ImporterHeader(Name = "*添加时间")]
    [ExporterHeader("*添加时间", Format = "", Width = 25, IsBold = true)]
    public DateTime? CreateTime { get; set; }
    /// <summary>
    /// 完成时间
    /// </summary>
    [ImporterHeader(Name = "*完成时间")]
    [ExporterHeader("*完成时间", Format = "", Width = 25, IsBold = true)]
    public DateTime? CompleteTime { get; set; }
}

public class WmsOrderBoxExport
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long Id { get; set; }

    /// <summary>
    /// 箱码号
    /// </summary>
    [ImporterHeader(Name = "*箱码号")]
    [ExporterHeader("*箱码号", Format = "", Width = 25, IsBold = true)]
    public string BoxCode { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    [ImporterHeader(Name = "*批次")]
    [ExporterHeader("*批次", Format = "", Width = 25, IsBold = true)]
    public string LotNo { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    [ImporterHeader(Name = "*生产日期")]
    [ExporterHeader("*生产日期", Format = "", Width = 25, IsBold = true)]
    public DateTime? ProductionDate { get; set; }

    /// <summary>
    /// 有效期
    /// </summary>
    [ImporterHeader(Name = "*有效期")]
    [ExporterHeader("*有效期", Format = "", Width = 25, IsBold = true)]
    public DateTime? ValidateDay { get; set; }

    /// <summary>
    /// 载具码
    /// </summary>
    [ImporterHeader(Name = "*载具码")]
    [ExporterHeader("*载具码", Format = "", Width = 25, IsBold = true)]
    public string StockCode { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    [ImporterHeader(Name = "*物料编码")]
    [ExporterHeader("*载具码", Format = "", Width = 25, IsBold = true)]
    public string MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    [ImporterHeader(Name = "*物料名称")]
    [ExporterHeader("*物料名称", Format = "", Width = 25, IsBold = true)]
    public string MaterialName { get; set; }

    /// <summary>
    /// 实际数量
    /// </summary>
    [ImporterHeader(Name = "*实际数量")]
    [ExporterHeader("*实际数量", Format = "", Width = 25, IsBold = true)]
    public decimal? Qty { get; set; }

    /// <summary>
    /// 满箱数量
    /// </summary>
    [ImporterHeader(Name = "*满箱数量")]
    [ExporterHeader("*满箱数量", Format = "", Width = 25, IsBold = true)]
    public string BoxQuantity { get; set; }
}

public class WmsExportOrderExport
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long Id { get; set; }

    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long MaterialId { get; set; }
    /// <summary>
    /// 物料编码
    /// </summary>
    [ImporterHeader(Name = "*物料编码")]
    [ExporterHeader("*物料编码", Format = "", Width = 25, IsBold = true)]
    public string MaterialCode { get; set; }
    /// <summary>
    /// 物料名称
    /// </summary>
    [ImporterHeader(Name = "*物料名称")]
    [ExporterHeader("*物料名称", Format = "", Width = 25, IsBold = true)]
    public string MaterialName { get; set; }
    /// <summary>
    /// 物料规格
    /// </summary>
    [ImporterHeader(Name = "*物料规格")]
    [ExporterHeader("*物料规格", Format = "", Width = 25, IsBold = true)]
    public string MaterialStandard { get; set; }
    /// <summary>
    /// 计量单位
    /// </summary>
    [ImporterHeader(Name = "*计量单位")]
    [ExporterHeader("*计量单位", Format = "", Width = 25, IsBold = true)]
    public string UnitName { get; set; }
    /// <summary>
    /// 出库数量
    /// </summary>
    [ImporterHeader(Name = "*出库数量")]
    [ExporterHeader("*出库数量", Format = "", Width = 25, IsBold = true)]
    public decimal? Qty { get; set; }
}

public class WmsExportOrderDetailExport
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long Id { get; set; }
    /// <summary>
    /// 出库流水号
    /// </summary>
    [ImporterHeader(Name = "*出库流水号")]
    [ExporterHeader("*出库流水号", Format = "", Width = 25, IsBold = true)]
    public string ExportOrderNo { get; set; }
    /// <summary>
    /// 出库单号
    /// </summary>
    [ImporterHeader(Name = "*出库单号")]
    [ExporterHeader("*出库单号", Format = "", Width = 25, IsBold = true)]
    public string ExportBillCode { get; set; }
    /// <summary>
    /// 载具条码
    /// </summary>
    [ImporterHeader(Name = "*载具条码")]
    [ExporterHeader("*载具条码", Format = "", Width = 25, IsBold = true)]
    public string ExportStockCode { get; set; }
    /// <summary>
    /// 储位地址
    /// </summary>
    [ImporterHeader(Name = "*储位地址")]
    [ExporterHeader("*储位地址", Format = "", Width = 25, IsBold = true)]
    public string ExportSlotCode { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    [ImporterHeader(Name = "*批次")]
    [ExporterHeader("*批次", Format = "", Width = 25, IsBold = true)]
    public string ExportLotNo { get; set; }

    /// <summary>
    /// 出库数量
    /// </summary>
    [ImporterHeader(Name = "*出库数量")]
    [ExporterHeader("*出库数量", Format = "", Width = 25, IsBold = true)]
    public decimal? pickedNum { get; set; }

    /// <summary>
    /// 执行状态
    /// </summary>
    [ImporterHeader(Name = "*执行状态")]
    [ExporterHeader("*执行状态", Format = "", Width = 25, IsBold = true)]
    public string ExportExecuteFlag { get; set; }

    /// <summary>
    /// 添加时间
    /// </summary>
    [ImporterHeader(Name = "*添加时间")]
    [ExporterHeader("*添加时间", Format = "", Width = 25, IsBold = true)]
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    [ImporterHeader(Name = "*完成时间")]
    [ExporterHeader("*完成时间", Format = "", Width = 25, IsBold = true)]
    public DateTime? CompleteTime { get; set; }
}

public class WmsExportOrderBoxExport
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long Id { get; set; }

    /// <summary>
    /// 箱码号
    /// </summary>
    [ImporterHeader(Name = "*箱码号")]
    [ExporterHeader("*箱码号", Format = "", Width = 25, IsBold = true)]
    public string BoxCode { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    [ImporterHeader(Name = "*批次")]
    [ExporterHeader("*批次", Format = "", Width = 25, IsBold = true)]
    public string LotNo { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    [ImporterHeader(Name = "*生产日期")]
    [ExporterHeader("*生产日期", Format = "", Width = 25, IsBold = true)]
    public DateTime? ProductionDate { get; set; }

    /// <summary>
    /// 有效期
    /// </summary>
    [ImporterHeader(Name = "*有效期")]
    [ExporterHeader("*有效期", Format = "", Width = 25, IsBold = true)]
    public DateTime? ExportLoseDate { get; set; }

    /// <summary>
    /// 载具码
    /// </summary>
    [ImporterHeader(Name = "*载具码")]
    [ExporterHeader("*载具码", Format = "", Width = 25, IsBold = true)]
    public string StockCode { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    [ImporterHeader(Name = "*物料编码")]
    [ExporterHeader("*载具码", Format = "", Width = 25, IsBold = true)]
    public string MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    [ImporterHeader(Name = "*物料名称")]
    [ExporterHeader("*物料名称", Format = "", Width = 25, IsBold = true)]
    public string MaterialName { get; set; }

    /// <summary>
    /// 箱内数量
    /// </summary>
    [ImporterHeader(Name = "*箱内数量")]
    [ExporterHeader("*箱内数量", Format = "", Width = 25, IsBold = true)]
    public decimal? Qty { get; set; }

    /// <summary>
    /// 实际数量
    /// </summary>
    [ImporterHeader(Name = "*实际数量")]
    [ExporterHeader("*实际数量", Format = "", Width = 25, IsBold = true)]
    public decimal? pickEdNum { get; set; }
}