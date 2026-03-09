// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 任务号公共表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WMSTaskNoPub", "任务号公共表")]
public partial class WMSTaskNoPub
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [SugarColumn(ColumnName = "Id", ColumnDescription = "主键ID", IsPrimaryKey = true, IsIdentity = true)]
    public virtual int Id { get; set; }

    /// <summary>
    /// 类型名称
    /// </summary>
    [SugarColumn(ColumnName = "TypeName", ColumnDescription = "类型名称", Length = 50)]
    public virtual string? TypeName { get; set; }

    /// <summary>
    /// 任务号
    /// </summary>
    [SugarColumn(ColumnName = "TaskNo", ColumnDescription = "任务号", Length = 50)]
    public virtual string? TaskNo { get; set; }
}

