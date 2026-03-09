// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 库存表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsStock", "库存表")]
public partial class WmsStock : EntityBaseDel
{
    /// <summary>
    /// 物料id
    /// </summary>
    [SugarColumn(ColumnName = "MaterialId", ColumnDescription = "物料id")]
    public virtual long? MaterialId { get; set; }
    
    /// <summary>
    /// 库存数量
    /// </summary>
    [SugarColumn(ColumnName = "StockQuantity", ColumnDescription = "库存数量", Length = 18, DecimalDigits=5)]
    public virtual decimal? StockQuantity { get; set; }
    
    /// <summary>
    /// 锁定数量
    /// </summary>
    [SugarColumn(ColumnName = "LockQuantity", ColumnDescription = "锁定数量", Length = 18, DecimalDigits=5)]
    public virtual decimal? LockQuantity { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    [SugarColumn(ColumnName = "LotNo", ColumnDescription = "批次", Length = 100)]
    public virtual string? LotNo { get; set; }
    
    /// <summary>
    /// 库房ID
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseId", ColumnDescription = "库房ID")]
    public virtual long? WarehouseId { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [SugarColumn(ColumnName = "ProductionDate", ColumnDescription = "生产日期")]
    public virtual DateTime? ProductionDate { get; set; }
    
    /// <summary>
    /// 有效日期
    /// </summary>
    [SugarColumn(ColumnName = "ValidateDay", ColumnDescription = "有效日期")]
    public virtual DateTime? ValidateDay { get; set; }
    
    /// <summary>
    /// 质检状态(0.待检验、1.合格、2.不合格)
    /// </summary>
    [SugarColumn(ColumnName = "InspectionStatus", ColumnDescription = "质检状态(0.待检验、1.合格、2.不合格)")]
    public virtual int? InspectionStatus { get; set; }
    
    /// <summary>
    /// 客户id
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "CustomerId", ColumnDescription = "客户id")]
    public virtual long? CustomerId { get; set; }

    /// <summary>
    /// 供应商id
    /// </summary>
    [SugarColumn(ColumnName = "SupplierId", ColumnDescription = "供应商id")]
    public virtual long? SupplierId { get; set; }

    /// <summary>
    /// 制造商id
    /// </summary>
    [SugarColumn(ColumnName = "ManufacturerId", ColumnDescription = "制造商id")]
    public virtual long? ManufacturerId { get; set; }

    /// <summary>
    /// 生产商id
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "ProduceId", ColumnDescription = "生产商id")]
    public virtual long? ProduceId { get; set; }

}
