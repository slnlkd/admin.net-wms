// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 库存载具表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsStockTray", "库存载具表")]
public partial class WmsStockTray : EntityBaseDel
{
    /// <summary>
    /// 储位位置
    /// </summary>
    [SugarColumn(ColumnName = "StockSlotCode", ColumnDescription = "储位位置", Length = 32)]
    public virtual string? StockSlotCode { get; set; }
    
    /// <summary>
    /// 托盘编码
    /// </summary>
    [SugarColumn(ColumnName = "StockCode", ColumnDescription = "托盘编码", Length = 32)]
    public virtual string? StockCode { get; set; }
    
    /// <summary>
    /// 库存数量
    /// </summary>
    [SugarColumn(ColumnName = "StockQuantity", ColumnDescription = "库存数量", Length = 18, DecimalDigits=3)]
    public virtual decimal? StockQuantity { get; set; }
    
    /// <summary>
    /// 库存日期
    /// </summary>
    [SugarColumn(ColumnName = "StockDate", ColumnDescription = "库存日期")]
    public virtual DateTime? StockDate { get; set; }
    
    /// <summary>
    /// 库存物品编码
    /// </summary>
    [SugarColumn(ColumnName = "MaterialId", ColumnDescription = "库存物品编码", Length = 32)]
    public virtual string? MaterialId { get; set; }

    /// <summary>
    /// 供应商ID
    /// </summary>
    [SugarColumn(ColumnName = "SupplierId", ColumnDescription = "供应商ID")]
    public virtual long? SupplierId { get; set; }

    /// <summary>
    /// 制造商ID
    /// </summary>
    [SugarColumn(ColumnName = "ManufacturerId", ColumnDescription = "制造商ID")]
    public virtual long? ManufacturerId { get; set; }

    /// <summary>
    /// 库存状态（0正常1占用）
    /// </summary>
    [SugarColumn(ColumnName = "StockStatusFlag", ColumnDescription = "库存状态（0正常1占用）")]
    public virtual int? StockStatusFlag { get; set; }
    
    /// <summary>
    /// 库存批次
    /// </summary>
    [SugarColumn(ColumnName = "LotNo", ColumnDescription = "库存批次", Length = 32)]
    public virtual string? LotNo { get; set; }
    
    /// <summary>
    /// 质检状态（0 待检验 1 合格品 2 不合格）
    /// </summary>
    [SugarColumn(ColumnName = "InspectionStatus", ColumnDescription = "质检状态（0 待检验 1 合格品 2 不合格）")]
    public virtual int? InspectionStatus { get; set; }
    
    /// <summary>
    /// 巷道ID
    /// </summary>
    [SugarColumn(ColumnName = "LanewayId", ColumnDescription = "巷道ID", Length = 32)]
    public virtual string? LanewayId { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseId", ColumnDescription = "仓库ID", Length = 32)]
    public virtual string? WarehouseId { get; set; }
    
    /// <summary>
    /// 库存备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark", ColumnDescription = "库存备注", Length = 50)]
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 抽检托（0 否 1是）
    /// </summary>
    [SugarColumn(ColumnName = "IsSamolingTray", ColumnDescription = "抽检托（0 否 1是）")]
    public virtual int? IsSamolingTray { get; set; }
    
    /// <summary>
    /// 锁定数量
    /// </summary>
    [SugarColumn(ColumnName = "LockQuantity", ColumnDescription = "锁定数量", Length = 18, DecimalDigits=3)]
    public virtual decimal? LockQuantity { get; set; }
    
    /// <summary>
    /// 出库单号
    /// </summary>
    [SugarColumn(ColumnName = "OwnerCode", ColumnDescription = "出库单号", Length = 32)]
    public virtual string? OwnerCode { get; set; }
    
    /// <summary>
    /// 冻结状态（0 正常 1 冻结 2 ）
    /// </summary>
    [SugarColumn(ColumnName = "AbnormalStatu", ColumnDescription = "冻结状态（0 正常 1 冻结 2 ）")]
    public virtual int? AbnormalStatu { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [SugarColumn(ColumnName = "ProductionDate", ColumnDescription = "生产日期")]
    public virtual DateTime? ProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    [SugarColumn(ColumnName = "ValidateDay", ColumnDescription = "失效日期")]
    public virtual DateTime? ValidateDay { get; set; }
    
    /// <summary>
    /// 放行状态（0 正常 1 放行）
    /// </summary>
    [SugarColumn(ColumnName = "ReleaseStatus", ColumnDescription = "放行状态（0 正常 1 放行）")]
    public virtual int? ReleaseStatus { get; set; }
    
    /// <summary>
    /// 复验日期
    /// </summary>
    [SugarColumn(ColumnName = "RetestDate", ColumnDescription = "复验日期")]
    public virtual DateTime? RetestDate { get; set; }
    
    /// <summary>
    /// 质检单号
    /// </summary>
    [SugarColumn(ColumnName = "InspectionNumber", ColumnDescription = "质检单号", Length = 50)]
    public virtual string? InspectionNumber { get; set; }
    
    /// <summary>
    /// 件数
    /// </summary>
    [SugarColumn(ColumnName = "outQty", ColumnDescription = "件数")]
    public virtual int? outQty { get; set; }
    
    /// <summary>
    /// 物料箱数量
    /// </summary>
    [SugarColumn(ColumnName = "BoxQuantity", ColumnDescription = "物料箱数量", Length = 18, DecimalDigits=3)]
    public virtual decimal? BoxQuantity { get; set; }
    
    /// <summary>
    /// 零头标识（0：整托；1：零托）
    /// </summary>
    [SugarColumn(ColumnName = "OddMarking", ColumnDescription = "零头标识（0：整托；1：零托）")]
    public virtual int? OddMarking { get; set; }
    
    /// <summary>
    /// 客户ID
    /// </summary>
    [SugarColumn(ColumnName = "CustomerId", ColumnDescription = "客户ID", Length = 50)]
    public virtual string? CustomerId { get; set; }
    
    /// <summary>
    /// 改判日期
    /// </summary>
    [SugarColumn(ColumnName = "RevisionDate", ColumnDescription = "改判日期")]
    public virtual DateTime? RevisionDate { get; set; }
    
    /// <summary>
    /// 血浆主子关系对应，存放主血箱ID
    /// </summary>
    [SugarColumn(ColumnName = "VehicleSubId", ColumnDescription = "血浆主子关系对应，存放主血箱ID", Length = 32)]
    public virtual string? VehicleSubId { get; set; }
    
    /// <summary>
    /// 0未验收 1验收完成
    /// </summary>
    [SugarColumn(ColumnName = "InspectFlag", ColumnDescription = "0未验收 1验收完成")]
    public virtual int? InspectFlag { get; set; }
    
    /// <summary>
    /// 放行日期
    /// </summary>
    [SugarColumn(ColumnName = "ReleaseDate", ColumnDescription = "放行日期")]
    public virtual DateTime? ReleaseDate { get; set; }
    
}
