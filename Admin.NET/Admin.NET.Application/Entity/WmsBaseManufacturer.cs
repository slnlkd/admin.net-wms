// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 制造商信息
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsBaseManufacturer", "制造商信息")]
public partial class WmsBaseManufacturer : EntityBaseDel
{
    /// <summary>
    /// 制造商编码
    /// </summary>
    [SugarColumn(ColumnName = "CustomerCode", ColumnDescription = "制造商编码", Length = 32)]
    public virtual string? CustomerCode { get; set; }
    
    /// <summary>
    /// 制造商名称
    /// </summary>
    [SugarColumn(ColumnName = "CustomerName", ColumnDescription = "制造商名称", Length = 32)]
    public virtual string? CustomerName { get; set; }
    
    /// <summary>
    /// 制造商地址
    /// </summary>
    [SugarColumn(ColumnName = "CustomerAddress", ColumnDescription = "制造商地址", Length = 32)]
    public virtual string? CustomerAddress { get; set; }
    
    /// <summary>
    /// 联系人
    /// </summary>
    [SugarColumn(ColumnName = "CustomerLinkman", ColumnDescription = "联系人", Length = 32)]
    public virtual string? CustomerLinkman { get; set; }
    
    /// <summary>
    /// 联系电话
    /// </summary>
    [SugarColumn(ColumnName = "CustomerPhone", ColumnDescription = "联系电话", Length = 32)]
    public virtual string? CustomerPhone { get; set; }
    
    /// <summary>
    /// 客户类型
    /// </summary>
    [SugarColumn(ColumnName = "CustomerTypeId", ColumnDescription = "客户类型", Length = 32)]
    public virtual string? CustomerTypeId { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark", ColumnDescription = "备注", Length = 32)]
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 授权编码
    /// </summary>
    [SugarColumn(ColumnName = "token", ColumnDescription = "授权编码", Length = 32)]
    public virtual string? token { get; set; }
    
    /// <summary>
    /// 授权用户
    /// </summary>
    [SugarColumn(ColumnName = "accountExec", ColumnDescription = "授权用户", Length = 32)]
    public virtual string? accountExec { get; set; }
    
    /// <summary>
    /// 状态 0启用，1禁用
    /// </summary>
    [SugarColumn(ColumnName = "Status", ColumnDescription = "状态 0启用，1禁用")]
    public virtual int? Status { get; set; }
    
}
