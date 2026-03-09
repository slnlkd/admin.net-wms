// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 仓储权限控制
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsPrmissionScope", "仓储权限控制")]
public partial class WmsPrmissionScope : EntityBase
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnName = "UserId", ColumnDescription = "用户Id")]
    public virtual long? UserId { get; set; }
    
    /// <summary>
    /// 表名
    /// </summary>
    [SugarColumn(ColumnName = "TableName", ColumnDescription = "表名", Length = 64)]
    public virtual string? TableName { get; set; }
    
    /// <summary>
    /// 字段名
    /// </summary>
    [SugarColumn(ColumnName = "FieldName", ColumnDescription = "字段名", Length = 64)]
    public virtual string? FieldName { get; set; }
    
    /// <summary>
    /// 字段值(多个用逗号分割)
    /// </summary>
    [SugarColumn(ColumnName = "FieldValue", ColumnDescription = "字段值(多个用逗号分割)", Length = 2048)]
    public virtual string? FieldValue { get; set; }
    /// <summary>
    /// 表功能名称
    /// </summary>

    [SugarColumn(ColumnName = "TableDes", ColumnDescription = "表功能名称", Length = 255)]
    public string TableDes { get; set; }
    /// <summary>
    /// 列名称
    /// </summary>
    [SugarColumn(ColumnName = "FieldDes", ColumnDescription = "列名称", Length = 255)]
    public string FieldDes { get; set; }

}
