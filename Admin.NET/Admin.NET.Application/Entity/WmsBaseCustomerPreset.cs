// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 客户信息预设
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsBaseCustomerPreset", "客户信息预设")]
public partial class WmsBaseCustomerPreset : EntityBaseDel
{
    /// <summary>
    /// 客户编码
    /// </summary>
    [SugarColumn(ColumnName = "CustomerCode", ColumnDescription = "客户编码", Length = 32)]
    public virtual string? CustomerCode { get; set; }
    
    /// <summary>
    /// 客户名称
    /// </summary>
    [SugarColumn(ColumnName = "CustomerName", ColumnDescription = "客户名称", Length = 32)]
    public virtual string? CustomerName { get; set; }
    
    /// <summary>
    /// 客户地址
    /// </summary>
    [SugarColumn(ColumnName = "CustomerAddress", ColumnDescription = "客户地址", Length = 32)]
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
    
    /// <summary>
    /// 审核状态，0=未审核，1=通过，2=驳回
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "ApprovalStatus", ColumnDescription = "审核状态，0=未审核，1=通过，2=驳回")]
    public virtual int ApprovalStatus { get; set; }
    
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
    [SugarColumn(ColumnName = "ApprovalRemark", ColumnDescription = "审核备注", Length = 255)]
    public virtual string? ApprovalRemark { get; set; }
    
}
