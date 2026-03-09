// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 物料信息预设
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsBaseMaterialPreset", "物料信息预设")]
public partial class WmsBaseMaterialPreset : EntityBaseDel
{
    /// <summary>
    /// 物料编码
    /// </summary>
    [SugarColumn(ColumnName = "MaterialCode", ColumnDescription = "物料编码", Length = 100)]
    public virtual string? MaterialCode { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    [SugarColumn(ColumnName = "MaterialName", ColumnDescription = "物料名称", Length = 255)]
    public virtual string? MaterialName { get; set; }
    
    /// <summary>
    /// 物料助记码
    /// </summary>
    [SugarColumn(ColumnName = "MaterialMcode", ColumnDescription = "物料助记码", Length = 100)]
    public virtual string? MaterialMcode { get; set; }
    
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
    /// 物料重量
    /// </summary>
    [SugarColumn(ColumnName = "MaterialWeight", ColumnDescription = "物料重量", Length = 18, DecimalDigits=0)]
    public virtual decimal? MaterialWeight { get; set; }
    
    /// <summary>
    /// 区域ID
    /// </summary>
    [SugarColumn(ColumnName = "MaterialAreaId", ColumnDescription = "区域ID")]
    public virtual string? MaterialAreaId { get; set; }
    
    /// <summary>
    /// 物料类型（字典MaterialType）
    /// </summary>
    [SugarColumn(ColumnName = "MaterialType", ColumnDescription = "物料类型（字典MaterialType）")]
    public virtual long? MaterialType { get; set; }
    
    /// <summary>
    /// 有效期1
    /// </summary>
    [SugarColumn(ColumnName = "MaterialValidityDay1", ColumnDescription = "有效期1", Length = 32)]
    public virtual string? MaterialValidityDay1 { get; set; }
    
    /// <summary>
    /// 有效期2
    /// </summary>
    [SugarColumn(ColumnName = "MaterialValidityDay2", ColumnDescription = "有效期2", Length = 32)]
    public virtual string? MaterialValidityDay2 { get; set; }
    
    /// <summary>
    /// 有效期3
    /// </summary>
    [SugarColumn(ColumnName = "MaterialValidityDay3", ColumnDescription = "有效期3", Length = 32)]
    public virtual string? MaterialValidityDay3 { get; set; }
    
    /// <summary>
    /// 计量单位
    /// </summary>
    [SugarColumn(ColumnName = "MaterialUnit", ColumnDescription = "计量单位")]
    public virtual long? MaterialUnit { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark", ColumnDescription = "备注", Length = 255)]
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 温度
    /// </summary>
    [SugarColumn(ColumnName = "MaterialTemp", ColumnDescription = "温度", Length = 32)]
    public virtual string? MaterialTemp { get; set; }
    
    /// <summary>
    /// 物料来源
    /// </summary>
    [SugarColumn(ColumnName = "MaterialOrigin", ColumnDescription = "物料来源", Length = 10)]
    public virtual string? MaterialOrigin { get; set; }
    
    /// <summary>
    /// 最高库存数量
    /// </summary>
    [SugarColumn(ColumnName = "MaterialStockHigh", ColumnDescription = "最高库存数量", Length = 32)]
    public virtual string? MaterialStockHigh { get; set; }
    
    /// <summary>
    /// 最低库存数量
    /// </summary>
    [SugarColumn(ColumnName = "MaterialStockLow", ColumnDescription = "最低库存数量", Length = 32)]
    public virtual string? MaterialStockLow { get; set; }
    
    /// <summary>
    /// 所属库房ID
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseId", ColumnDescription = "所属库房ID")]
    public virtual long? WarehouseId { get; set; }
    
    /// <summary>
    /// 提前预警天数
    /// </summary>
    [SugarColumn(ColumnName = "MaterialAlarmDay", ColumnDescription = "提前预警天数", Length = 32)]
    public virtual string? MaterialAlarmDay { get; set; }
    
    /// <summary>
    /// 箱数量
    /// </summary>
    [SugarColumn(ColumnName = "BoxQuantity", ColumnDescription = "箱数量", Length = 32)]
    public virtual string? BoxQuantity { get; set; }
    
    /// <summary>
    /// 贴标（1是，0否）
    /// </summary>
    [SugarColumn(ColumnName = "Labeling", ColumnDescription = "贴标（1是，0否）", Length = 32)]
    public virtual string? Labeling { get; set; }
    
    /// <summary>
    /// 每件托数
    /// </summary>
    [SugarColumn(ColumnName = "EveryNumber", ColumnDescription = "每件托数", Length = 32)]
    public virtual string? EveryNumber { get; set; }
    
    /// <summary>
    /// 载具（0：托盘，1：血箱）
    /// </summary>
    [SugarColumn(ColumnName = "Vehicle", ColumnDescription = "载具（0：托盘，1：血箱）", Length = 32)]
    public virtual string? Vehicle { get; set; }
    
    /// <summary>
    /// 管理方式（0：按件数，1：按数量）
    /// </summary>
    [SugarColumn(ColumnName = "ManageType", ColumnDescription = "管理方式（0：按件数，1：按数量）", Length = 32)]
    public virtual string? ManageType { get; set; }
    
    /// <summary>
    /// 外部物品ID
    /// </summary>
    [SugarColumn(ColumnName = "OuterInnerCode", ColumnDescription = "外部物品ID", Length = 32)]
    public virtual string? OuterInnerCode { get; set; }
    
    /// <summary>
    /// 最低储备
    /// </summary>
    [SugarColumn(ColumnName = "MixReserve", ColumnDescription = "最低储备", Length = 32)]
    public virtual string? MixReserve { get; set; }
    
    /// <summary>
    /// 最高储备
    /// </summary>
    [SugarColumn(ColumnName = "MaxReserve", ColumnDescription = "最高储备", Length = 32)]
    public virtual string? MaxReserve { get; set; }
    
    /// <summary>
    /// 提前报警天数
    /// </summary>
    [SugarColumn(ColumnName = "AlarmDay", ColumnDescription = "提前报警天数")]
    public virtual int? AlarmDay { get; set; }
    
    /// <summary>
    /// 授权编码
    /// </summary>
    [SugarColumn(ColumnName = "token", ColumnDescription = "授权编码", Length = 64)]
    public virtual string? token { get; set; }
    
    /// <summary>
    /// 授权用户
    /// </summary>
    [SugarColumn(ColumnName = "accountExec", ColumnDescription = "授权用户", Length = 64)]
    public virtual string? accountExec { get; set; }
    
    /// <summary>
    /// 状态(1:启用,0:禁用)
    /// </summary>
    [SugarColumn(ColumnName = "Status", ColumnDescription = "状态(1:启用,0:禁用)")]
    public virtual bool? Status { get; set; }
    
    /// <summary>
    /// 0=待审核，1=审核通过，2=驳回
    /// </summary>
    [SugarColumn(ColumnName = "ApprovalStatus", ColumnDescription = "0=待审核，1=审核通过，2=驳回")]
    public virtual int? ApprovalStatus { get; set; }
    
    /// <summary>
    /// 审核人名称
    /// </summary>
    [SugarColumn(ColumnName = "ApprovalUserName", ColumnDescription = "审核人名称", Length = 64)]
    public virtual string? ApprovalUserName { get; set; }
    
    /// <summary>
    /// 审核ID
    /// </summary>
    [SugarColumn(ColumnName = "ApprovalUserId", ColumnDescription = "审核ID")]
    public virtual long? ApprovalUserId { get; set; }

    /// <summary>
    /// 审核备注
    /// </summary>
    [SugarColumn(ColumnName = "ApprovalRemark", ColumnDescription = "审核备注", Length = 64)]
    public virtual string? ApprovalRemark { get; set; }
}
