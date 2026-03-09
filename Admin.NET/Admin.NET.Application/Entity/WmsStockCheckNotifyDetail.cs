// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 盘点单明细。
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsStockCheckNotifyDetail", "盘点单明细")]
public partial class WmsStockCheckNotifyDetail : EntityBaseDel
{
    /// <summary>
    /// 盘点单号。
    /// </summary>
    [SugarColumn(ColumnName = "CheckBillCode", ColumnDescription = "盘点单号", Length = 64)]
    public string CheckBillCode { get; set; }

    /// <summary>
    /// 库存主键。
    /// </summary>
    [SugarColumn(ColumnName = "CheckStockId", ColumnDescription = "库存主键")]
    public long? CheckStockId { get; set; }

    /// <summary>
    /// 储位编码。
    /// </summary>
    [SugarColumn(ColumnName = "StockSlot", ColumnDescription = "储位编码", Length = 64)]
    public string StockSlot { get; set; }

    /// <summary>
    /// 托盘编码。
    /// </summary>
    [SugarColumn(ColumnName = "StockCode", ColumnDescription = "托盘编码", Length = 64)]
    public string StockCode { get; set; }

    /// <summary>
    /// 计划数量。
    /// </summary>
    [SugarColumn(ColumnName = "StockQuantity", ColumnDescription = "计划数量")]
    public decimal? StockQuantity { get; set; }

    /// <summary>
    /// 批次号。
    /// </summary>
    [SugarColumn(ColumnName = "StockLotNo", ColumnDescription = "批次号", Length = 128)]
    public string StockLotNo { get; set; }

    /// <summary>
    /// 执行标志（0待执行、1正在执行、2已完成）。
    /// </summary>
    [SugarColumn(ColumnName = "ExecuteFlag", ColumnDescription = "执行标志")]
    public int? ExecuteFlag { get; set; }

    /// <summary>
    /// 物料主键。
    /// </summary>
    [SugarColumn(ColumnName = "MaterialId", ColumnDescription = "物料主键")]
    public long? MaterialId { get; set; }

    /// <summary>
    /// 仓库主键。
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseId", ColumnDescription = "仓库主键", Length = 64)]
    public string WarehouseId { get; set; }

    /// <summary>
    /// 质检状态。
    /// </summary>
    [SugarColumn(ColumnName = "InspectionStatus", ColumnDescription = "质检状态")]
    public int? InspectionStatus { get; set; }

    /// <summary>
    /// 实盘数量。
    /// </summary>
    [SugarColumn(ColumnName = "RealQuantity", ColumnDescription = "实盘数量")]
    public decimal? RealQuantity { get; set; }

    /// <summary>
    /// 盘点结果：0 正常、1 盘亏、2 盘盈。
    /// </summary>
    [SugarColumn(ColumnName = "CheckResult", ColumnDescription = "盘点结果")]
    public int? CheckResult { get; set; }

    /// <summary>
    /// 业务时间。
    /// </summary>
    [SugarColumn(ColumnName = "AddDate", ColumnDescription = "业务时间")]
    public DateTime? AddDate { get; set; }
    /// <summary>
    /// 盘点任务Id
    /// </summary>
    [SugarColumn(ColumnName = "TaskId", ColumnDescription = "盘点任务Id", Length = 50)]
    public string TaskId { get; set; }
    /// <summary>
    /// 盘点任务编号
    /// </summary>
    [SugarColumn(ColumnName = "CheckTaskNo", ColumnDescription = "盘点任务编号", Length = 32)]
    public string CheckTaskNo { get; set; }
    /// <summary>
    /// 盘点备注
    /// </summary>
    [SugarColumn(ColumnName = "CheckRemark", ColumnDescription = "盘点备注", Length = 500)]
    public string CheckRemark { get; set; }
}

