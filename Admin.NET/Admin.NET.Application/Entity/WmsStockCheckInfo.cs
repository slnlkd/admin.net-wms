// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 盘点箱码明细。
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsStockCheckInfo", "盘点箱码明细")]
public partial class WmsStockCheckInfo : EntityBaseDel
{
    /// <summary>
    /// 箱码。
    /// </summary>
    [SugarColumn(ColumnName = "BoxCode", ColumnDescription = "箱码", Length = 128)]
    public string BoxCode { get; set; }

    /// <summary>
    /// 盘点结果：0 正常、1 盘亏、2 盘盈。
    /// </summary>
    [SugarColumn(ColumnName = "CheckResult", ColumnDescription = "盘点结果")]
    public int? CheckResult { get; set; }

    /// <summary>
    /// 托盘编码。
    /// </summary>
    [SugarColumn(ColumnName = "StockCode", ColumnDescription = "托盘编码", Length = 64)]
    public string StockCode { get; set; }

    /// <summary>
    /// 物料主键。
    /// </summary>
    [SugarColumn(ColumnName = "MaterialId", ColumnDescription = "物料主键")]
    public long? MaterialId { get; set; }

    /// <summary>
    /// 物料编码。
    /// </summary>
    [SugarColumn(ColumnName = "MaterialCode", ColumnDescription = "物料编码", Length = 64)]
    public string MaterialCode { get; set; }

    /// <summary>
    /// 物料名称。
    /// </summary>
    [SugarColumn(ColumnName = "MaterialName", ColumnDescription = "物料名称", Length = 128)]
    public string MaterialName { get; set; }

    /// <summary>
    /// 批次号。
    /// </summary>
    [SugarColumn(ColumnName = "LotNo", ColumnDescription = "批次号", Length = 128)]
    public string LotNo { get; set; }

    /// <summary>
    /// 库存数量。
    /// </summary>
    [SugarColumn(ColumnName = "Qty", ColumnDescription = "库存数量")]
    public decimal? Qty { get; set; }

    /// <summary>
    /// 实盘数量。
    /// </summary>
    [SugarColumn(ColumnName = "RealQuantity", ColumnDescription = "实盘数量")]
    public decimal? RealQuantity { get; set; }

    /// <summary>
    /// 生产日期。
    /// </summary>
    [SugarColumn(ColumnName = "ProductionDate", ColumnDescription = "生产日期")]
    public DateTime? ProductionDate { get; set; }

    /// <summary>
    /// 失效日期。
    /// </summary>
    [SugarColumn(ColumnName = "ValidateDay", ColumnDescription = "失效日期")]
    public DateTime? ValidateDay { get; set; }

    /// <summary>
    /// 质检状态。
    /// </summary>
    [SugarColumn(ColumnName = "InspectionStatus", ColumnDescription = "质检状态")]
    public int? InspectionStatus { get; set; }

    /// <summary>
    /// 抽检状态。
    /// </summary>
    [SugarColumn(ColumnName = "ExtractStatus", ColumnDescription = "抽检状态")]
    public int? ExtractStatus { get; set; }

    /// <summary>
    /// 采样日期。
    /// </summary>
    [SugarColumn(ColumnName = "SamplingDate", ColumnDescription = "采样日期", Length = 32)]
    public string SamplingDate { get; set; }

    /// <summary>
    /// 人员编号。
    /// </summary>
    [SugarColumn(ColumnName = "StaffCode", ColumnDescription = "人员编号", Length = 64)]
    public string StaffCode { get; set; }

    /// <summary>
    /// 人员姓名。
    /// </summary>
    [SugarColumn(ColumnName = "StaffName", ColumnDescription = "人员姓名", Length = 64)]
    public string StaffName { get; set; }

    /// <summary>
    /// 托盘箱码关联的盘点明细主键。
    /// </summary>
    [SugarColumn(ColumnName = "StockCheckId", ColumnDescription = "盘点明细主键")]
    public long StockCheckId { get; set; }

    /// <summary>
    /// 状态（0 待盘点、1 已确认）。
    /// </summary>
    [SugarColumn(ColumnName = "Status", ColumnDescription = "状态")]
    public int? Status { get; set; }

    /// <summary>
    /// 业务状态（0 待提交、1 已提交）。
    /// </summary>
    [SugarColumn(ColumnName = "State", ColumnDescription = "业务状态")]
    public int? State { get; set; }

    /// <summary>
    /// 记录重量。
    /// </summary>
    [SugarColumn(ColumnName = "Weight", ColumnDescription = "重量")]
    public decimal? Weight { get; set; }

    /// <summary>
    /// 是否零箱标识。
    /// </summary>
    [SugarColumn(ColumnName = "PickingSlurry", ColumnDescription = "是否零箱标识")]
    public int? PickingSlurry { get; set; }

    /// <summary>
    /// 客户主键。
    /// </summary>
    [SugarColumn(ColumnName = "CustomerId", ColumnDescription = "客户主键", Length = 64)]
    public string CustomerId { get; set; }

    /// <summary>
    /// 异常标识。
    /// </summary>
    [SugarColumn(ColumnName = "OddMarking", ColumnDescription = "异常标识", Length = 64)]
    public string OddMarking { get; set; }

    /// <summary>
    /// 已出库数量。
    /// </summary>
    [SugarColumn(ColumnName = "OutQty", ColumnDescription = "已出库数量")]
    public int? OutQty { get; set; }
}

