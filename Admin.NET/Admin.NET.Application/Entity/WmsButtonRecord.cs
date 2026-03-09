// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 按钮点击表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsButtonRecord", "按钮点击表")]
public partial class WmsButtonRecord : EntityBaseDel
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnName = "UserId", ColumnDescription = "用户Id")]
    public virtual long? UserId { get; set; }
    
    /// <summary>
    /// 按钮点击时间
    /// </summary>
    [SugarColumn(ColumnName = "ClickButtonDate", ColumnDescription = "按钮点击时间")]
    public virtual DateTime? ClickButtonDate { get; set; }
    
    /// <summary>
    /// 记录按钮信息（ 用户名+按钮点击时间+记录按钮信息）
    /// </summary>
    [SugarColumn(ColumnName = "ButtonInformation", ColumnDescription = "记录按钮信息（ 用户名+按钮点击时间+记录按钮信息）", Length = 500)]
    public virtual string? ButtonInformation { get; set; }
    
    /// <summary>
    /// 用户编码
    /// </summary>
    [SugarColumn(ColumnName = "UserCode", ColumnDescription = "用户编码", Length = 50)]
    public virtual string? UserCode { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    [SugarColumn(ColumnName = "UserName", ColumnDescription = "用户名", Length = 50)]
    public virtual string? UserName { get; set; }
    
    /// <summary>
    /// 菜单名称
    /// </summary>
    [SugarColumn(ColumnName = "PageName", ColumnDescription = "菜单名称", Length = 50)]
    public virtual string? PageName { get; set; }
    
}
