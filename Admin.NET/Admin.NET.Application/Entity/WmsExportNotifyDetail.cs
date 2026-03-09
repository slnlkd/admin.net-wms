// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 出库单据明细表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsExportNotifyDetail", "出库单据明细表")]
public partial class WmsExportNotifyDetail : EntityBaseDel
{
    /// <summary>
    /// 出库单据
    /// </summary>
    [SugarColumn(ColumnName = "ExportBillCode", ColumnDescription = "出库单据", Length = 32)]
    public virtual string? ExportBillCode { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>
    [SugarColumn(ColumnName = "MaterialId", ColumnDescription = "物料ID", Length = 32)]
    public virtual long? MaterialId { get; set; }
    
    /// <summary>
    /// 物料编码
    /// </summary>
    [SugarColumn(ColumnName = "MaterialCode", ColumnDescription = "物料编码", Length = 50)]
    public virtual string? MaterialCode { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    [SugarColumn(ColumnName = "MaterialName", ColumnDescription = "物料名称", Length = -1)]
    public virtual string? MaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    [SugarColumn(ColumnName = "MaterialStandard", ColumnDescription = "物料规格", Length = 200)]
    public virtual string? MaterialStandard { get; set; }
    
    /// <summary>
    /// 物料型号
    /// </summary>
    [SugarColumn(ColumnName = "MaterialModel", ColumnDescription = "物料型号", Length = 32)]
    public virtual string? MaterialModel { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    [SugarColumn(ColumnName = "MaterialType", ColumnDescription = "物料类型", Length = 32)]
    public virtual string? MaterialType { get; set; }
    
    /// <summary>
    /// 物料单位
    /// </summary>
    [SugarColumn(ColumnName = "MaterialUnit", ColumnDescription = "物料单位", Length = 32)]
    public virtual string? MaterialUnit { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    [SugarColumn(ColumnName = "LotNo", ColumnDescription = "批次", Length = 50)]
    public virtual string? LotNo { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [SugarColumn(ColumnName = "ProductionDate", ColumnDescription = "生产日期")]
    public virtual DateTime? ProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    [SugarColumn(ColumnName = "LostDate", ColumnDescription = "失效日期")]
    public virtual DateTime? LostDate { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseId", ColumnDescription = "仓库ID", Length = 32)]
    public virtual long? WarehouseId { get; set; }
    
    /// <summary>
    /// 计划出库数量
    /// </summary>
    [SugarColumn(ColumnName = "ExportQuantity", ColumnDescription = "计划出库数量", Length = 18, DecimalDigits=0)]
    public virtual decimal? ExportQuantity { get; set; }
    
    /// <summary>
    /// 分配数量
    /// </summary>
    [SugarColumn(ColumnName = "AllocateQuantity", ColumnDescription = "分配数量", Length = 18, DecimalDigits=0)]
    public virtual decimal? AllocateQuantity { get; set; }
    
    /// <summary>
    /// 下发数量
    /// </summary>
    [SugarColumn(ColumnName = "FactQuantity", ColumnDescription = "下发数量", Length = 18, DecimalDigits=0)]
    public virtual decimal? FactQuantity { get; set; }
    
    /// <summary>
    /// 完成数量
    /// </summary>
    [SugarColumn(ColumnName = "CompleteQuantity", ColumnDescription = "完成数量", Length = 18, DecimalDigits=0)]
    public virtual decimal? CompleteQuantity { get; set; }
    
    /// <summary>
    /// 执行标志
    /// </summary>
    [SugarColumn(ColumnName = "ExportDetailFlag", ColumnDescription = "执行标志")]
    public virtual int? ExportDetailFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "InspectionStatus", ColumnDescription = "备注", Length = 32)]
    public virtual string? InspectionStatus { get; set; }
    
    /// <summary>
    /// 外部单据编码
    /// </summary>
    [SugarColumn(ColumnName = "LCLRemainderQTY", ColumnDescription = "外部单据编码", Length = 32)]
    public virtual string? LCLRemainderQTY { get; set; }
    
    /// <summary>
    /// 外部单据ID
    /// </summary>
    [SugarColumn(ColumnName = "OuterDetailId", ColumnDescription = "外部单据ID", Length = 32)]
    public virtual string? OuterDetailId { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "IsDelete", ColumnDescription = "软删除")]
    public virtual bool? IsDelete { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(ColumnName = "KilogramQty", ColumnDescription = "", Length = 18, DecimalDigits=0)]
    public virtual decimal? KilogramQty { get; set; }
    
}
