// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 移库单据明细表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsMoveNotify", "移库单据明细表")]
public partial class WmsMoveNotify : EntityBaseDel
{
    /// <summary>
    /// 移库单据类型 -- 单号
    /// </summary>
    [SugarColumn(ColumnName = "MoveBillCode", ColumnDescription = "移库单据类型", Length = 50)]
    public virtual string? MoveBillCode { get; set; }
    
    /// <summary>
    /// 移库序号
    /// </summary>
    [SugarColumn(ColumnName = "MoveListNo", ColumnDescription = "移库序号")]
    public virtual int? MoveListNo { get; set; }
    
    /// <summary>
    /// 移库批次
    /// </summary>
    [SugarColumn(ColumnName = "MoveLotNo", ColumnDescription = "移库批次", Length = 50)]
    public virtual string? MoveLotNo { get; set; }
    
    /// <summary>
    /// 物料id
    /// </summary>
    [SugarColumn(ColumnName = "MaterialId", ColumnDescription = "物料id", Length = 50)]
    public virtual string? MaterialId { get; set; }
    
    /// <summary>
    /// 移库物料型号
    /// </summary>
    [SugarColumn(ColumnName = "MoveMaterialModel", ColumnDescription = "移库物料型号", Length = 100)]
    public virtual string? MoveMaterialModel { get; set; }
    
    /// <summary>
    /// 物料温度
    /// </summary>
    [SugarColumn(ColumnName = "MoveMaterialTemp", ColumnDescription = "物料温度", Length = 50)]
    public virtual string? MoveMaterialTemp { get; set; }
    
    /// <summary>
    /// 物料类型（字典MaterialType）
    /// </summary>
    [SugarColumn(ColumnName = "MoveMaterialType", ColumnDescription = "物料类型（字典MaterialType）", Length = 100)]
    public virtual string? MoveMaterialType { get; set; }
    
    /// <summary>
    /// 物料状态
    /// </summary>
    [SugarColumn(ColumnName = "MoveMaterialStatus", ColumnDescription = "物料状态", Length = 100)]
    public virtual string? MoveMaterialStatus { get; set; }
    
    /// <summary>
    /// 物料单位
    /// </summary>
    [SugarColumn(ColumnName = "MoveMaterialUnit", ColumnDescription = "物料单位", Length = 50)]
    public virtual string? MoveMaterialUnit { get; set; }
    
    /// <summary>
    /// 物料品牌
    /// </summary>
    [SugarColumn(ColumnName = "MoveMaterialBrand", ColumnDescription = "物料品牌", Length = 100)]
    public virtual string? MoveMaterialBrand { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [SugarColumn(ColumnName = "MoveProductionDate", ColumnDescription = "生产日期")]
    public virtual DateTime? MoveProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    [SugarColumn(ColumnName = "MoveLostDate", ColumnDescription = "失效日期")]
    public virtual DateTime? MoveLostDate { get; set; }
    
    /// <summary>
    /// 移库数量
    /// </summary>
    [SugarColumn(ColumnName = "MoveQuantity", ColumnDescription = "移库数量")]
    public virtual int? MoveQuantity { get; set; }
    
    /// <summary>
    /// 移库日期
    /// </summary>
    [SugarColumn(ColumnName = "MoveDate", ColumnDescription = "移库日期")]
    public virtual DateTime? MoveDate { get; set; }
    
    /// <summary>
    /// 仓库id
    /// </summary>
    [SugarColumn(ColumnName = "MoveWarehouseId", ColumnDescription = "仓库id")]
    public virtual long? MoveWarehouseId { get; set; }
    
    /// <summary>
    /// 移出巷道id
    /// </summary>
    [SugarColumn(ColumnName = "MoveLanewayOutCode", ColumnDescription = "移出巷道id")]
    public virtual long? MoveLanewayOutCode { get; set; }
    
    /// <summary>
    /// 移入巷道id
    /// </summary>
    [SugarColumn(ColumnName = "MoveLanewayInCode", ColumnDescription = "移入巷道id", Length = 50)]
    public virtual string? MoveLanewayInCode { get; set; }
    
    /// <summary>
    /// 移出储位编码
    /// </summary>
    [SugarColumn(ColumnName = "MoveOutSlotCode", ColumnDescription = "移出储位编码", Length = 50)]
    public virtual string? MoveOutSlotCode { get; set; }
    
    /// <summary>
    /// 移入储位编码
    /// </summary>
    [SugarColumn(ColumnName = "MoveInSlotCode", ColumnDescription = "移入储位编码", Length = 50)]
    public virtual string? MoveInSlotCode { get; set; }
    
    /// <summary>
    /// 执行标志（01待执行、02正在执行、03已完成、04已上传）
    /// </summary>
    [SugarColumn(ColumnName = "MoveExecuteFlag", ColumnDescription = "执行标志（01待执行、02正在执行、03已完成、04已上传）", Length = 50)]
    public virtual string? MoveExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "MoveRemark", ColumnDescription = "备注", Length = 100)]
    public virtual string? MoveRemark { get; set; }
    
    /// <summary>
    /// 移库任务号
    /// </summary>
    [SugarColumn(ColumnName = "MoveTaskNo", ColumnDescription = "移库任务号", Length = 100)]
    public virtual string? MoveTaskNo { get; set; }
    
    /// <summary>
    /// 库存表id
    /// </summary>
    [SugarColumn(ColumnName = "StockStockCodeId", ColumnDescription = "库存表id")]
    public virtual string? StockStockCodeId { get; set; }
    
    /// <summary>
    /// 软删除
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "IsDelete", ColumnDescription = "软删除")]
    public virtual bool IsDelete { get; set; }
    
}
