// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 巷道表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsBaseLaneway", "巷道表")]
public partial class WmsBaseLaneway : EntityBaseDel
{
    /// <summary>
    /// 巷道编码
    /// </summary>
    [SugarColumn(ColumnName = "LanewayCode", ColumnDescription = "巷道编码", Length = 64)]
    public virtual string? LanewayCode { get; set; }
    
    /// <summary>
    /// 巷道名称
    /// </summary>
    [SugarColumn(ColumnName = "LanewayName", ColumnDescription = "巷道名称", Length = 64)]
    public virtual string? LanewayName { get; set; }
    
    /// <summary>
    /// 巷道状态
    /// </summary>
    [SugarColumn(ColumnName = "LanewayStatus", ColumnDescription = "巷道状态")]
    public virtual bool? LanewayStatus { get; set; }
    
    /// <summary>
    /// 所属库房ID
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseId", ColumnDescription = "所属库房ID")]
    public virtual long? WarehouseId { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark", ColumnDescription = "备注", Length = 64)]
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 优先级
    /// </summary>
    [SugarColumn(ColumnName = "LanewayPriority", ColumnDescription = "优先级")]
    public virtual int? LanewayPriority { get; set; }
    
    /// <summary>
    /// 巷道环境
    /// </summary>
    [SugarColumn(ColumnName = "LanewayTemp", ColumnDescription = "巷道环境", Length = 64)]
    public virtual string? LanewayTemp { get; set; }
    
    /// <summary>
    /// 巷道口状态（0:单口；1：多口）
    /// </summary>
    [SugarColumn(ColumnName = "LanewayType", ColumnDescription = "巷道口状态（0:单口；1：多口）")]
    public virtual int? LanewayType { get; set; }
    
}
