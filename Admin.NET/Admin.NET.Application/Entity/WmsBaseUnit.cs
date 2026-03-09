// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
using System.ComponentModel;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 计量单位
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsBaseUnit", "计量单位")]
public partial class WmsBaseUnit : EntityBaseDel
{
    /// <summary>
    /// 单位编码
    /// </summary>
    [Description("单位编码")]
    [SugarColumn(ColumnName = "UnitCode", ColumnDescription = "单位编码", Length = 64)]
    public virtual string? UnitCode { get; set; }
    
    /// <summary>
    /// 单位名称
    /// </summary>
    [SugarColumn(ColumnName = "UnitName", ColumnDescription = "单位名称", Length = 64)]
    [Description("单位名称")]
    public virtual string? UnitName { get; set; }
    
    /// <summary>
    /// 单位缩写名称
    /// </summary>
    [SugarColumn(ColumnName = "UnitAbbrevName", ColumnDescription = "单位缩写名称", Length = 32)]
    [Description("单位缩写名称")]
    public virtual string? UnitAbbrevName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark", ColumnDescription = "备注", Length = 255)]
    [Description("备注")]
    public virtual string? Remark { get; set; }
    
}
