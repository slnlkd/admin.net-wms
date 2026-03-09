// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 出库流水明细表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsExportBoxInfo", "出库流水明细表")]
public partial class WmsExportBoxInfo : EntityBaseDel
{
    /// <summary>
    /// 箱码
    /// </summary>
    [SugarColumn(ColumnName = "BoxCode", ColumnDescription = "箱码", Length = 100)]
    public virtual string? BoxCode { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    [SugarColumn(ColumnName = "Qty", ColumnDescription = "数量", Length = 18, DecimalDigits=5)]
    public virtual decimal? Qty { get; set; }
    
    /// <summary>
    /// 托盘编码
    /// </summary>
    [SugarColumn(ColumnName = "StockCodeCode", ColumnDescription = "托盘编码", Length = 100)]
    public virtual string? StockCodeCode { get; set; }
    
    /// <summary>
    /// 出库流水号
    /// </summary>
    [SugarColumn(ColumnName = "ExportOrderNo", ColumnDescription = "出库流水号", Length = 100)]
    public virtual string? ExportOrderNo { get; set; }
    
    /// <summary>
    /// 是否出库
    /// </summary>
    [SugarColumn(ColumnName = "IsOut", ColumnDescription = "是否出库")]
    public virtual int? IsOut { get; set; }
    
    /// <summary>
    /// 物料编码
    /// </summary>
    [SugarColumn(ColumnName = "MaterialId", ColumnDescription = "物料编码", Length = 100)]
    public virtual string? MaterialId { get; set; }
    
    /// <summary>
    /// 批次号
    /// </summary>
    [SugarColumn(ColumnName = "LotNo", ColumnDescription = "批次号", Length = 100)]
    public virtual string? LotNo { get; set; }
    
    /// <summary>
    /// 添加时间
    /// </summary>
    [SugarColumn(ColumnName = "AddDate", ColumnDescription = "添加时间")]
    public virtual DateTime? AddDate { get; set; }
    
    /// <summary>
    /// 状态（1、0）
    /// </summary>
    [SugarColumn(ColumnName = "Status", ColumnDescription = "状态（1、0）")]
    public virtual int? Status { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [SugarColumn(ColumnName = "ProductionDate", ColumnDescription = "生产日期")]
    public virtual DateTime? ProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    [SugarColumn(ColumnName = "ExportLoseDate", ColumnDescription = "失效日期")]
    public virtual DateTime? ExportLoseDate { get; set; }
    
    /// <summary>
    /// 是否必出箱
    /// </summary>
    [SugarColumn(ColumnName = "IsMustExport", ColumnDescription = "是否必出箱")]
    public virtual int? IsMustExport { get; set; }
    
    /// <summary>
    /// 待拣数量
    /// </summary>
    [SugarColumn(ColumnName = "PickNum", ColumnDescription = "待拣数量", Length = 18, DecimalDigits=5)]
    public virtual decimal? PickNum { get; set; }
    
    /// <summary>
    /// 已拣数量
    /// </summary>
    [SugarColumn(ColumnName = "PickEdNum", ColumnDescription = "已拣数量", Length = 18, DecimalDigits=5)]
    public virtual decimal? PickEdNum { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "IsDelete", ColumnDescription = "软删除")]
    public virtual bool IsDelete { get; set; }
    
}
