// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 业务操作日志表 - 客户可见的操作记录
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsBaseOperLog", "业务操作日志表 - 客户可见的操作记录")]
public partial class WmsBaseOperLog : EntityBaseId
{
    /// <summary>
    /// 关联的技术日志TraceId，用于开发人员追踪
    /// </summary>
    [SugarColumn(ColumnName = "TraceId", ColumnDescription = "关联的技术日志TraceId，用于开发人员追踪", Length = 50)]
    public virtual string? TraceId { get; set; }
    
    /// <summary>
    /// 模块名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "Module", ColumnDescription = "模块名称", Length = 50)]
    public virtual string Module { get; set; }
    
    /// <summary>
    /// 操作类型：新增、修改、删除、审核等
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "OperationType", ColumnDescription = "操作类型：新增、修改、删除、审核等", Length = 20)]
    public virtual string OperationType { get; set; }
    
    /// <summary>
    /// 操作人员
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "Operator", ColumnDescription = "操作人员", Length = 50)]
    public virtual string Operator { get; set; }
    
    /// <summary>
    /// 操作时间
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "OperateTime", ColumnDescription = "操作时间", DefaultValue = "(getdate())")]
    public virtual DateTime OperateTime { get; set; }
    
    /// <summary>
    /// 操作IP地址
    /// </summary>
    [SugarColumn(ColumnName = "OperateIp", ColumnDescription = "操作IP地址", Length = 50)]
    public virtual string? OperateIp { get; set; }
    
    /// <summary>
    /// 业务单据号/ID
    /// </summary>
    [SugarColumn(ColumnName = "BusinessNo", ColumnDescription = "业务单据号/ID", Length = 50)]
    public virtual string? BusinessNo { get; set; }
    
    /// <summary>
    /// 操作内容（客户可读的详细描述）
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "OperationContent", ColumnDescription = "操作内容（客户可读的详细描述）", Length = -1)]
    public virtual string OperationContent { get; set; }
    
    /// <summary>
    /// 修改前数据摘要（客户可读格式）
    /// </summary>
    [SugarColumn(ColumnName = "BeforeDataSummary", ColumnDescription = "修改前数据摘要（客户可读格式）", Length = -1)]
    public virtual string? BeforeDataSummary { get; set; }
    
    /// <summary>
    /// 修改后数据摘要（客户可读格式）
    /// </summary>
    [SugarColumn(ColumnName = "AfterDataSummary", ColumnDescription = "修改后数据摘要（客户可读格式）", Length = -1)]
    public virtual string? AfterDataSummary { get; set; }
    
    /// <summary>
    /// 操作结果：成功/失败
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "Result", ColumnDescription = "操作结果：成功/失败", Length = 10, DefaultValue = "('成功')")]
    public virtual string Result { get; set; }
    
    /// <summary>
    /// 操作耗时（毫秒）
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "ElapsedMs", ColumnDescription = "操作耗时（毫秒）", DefaultValue = "((0))")]
    public virtual long ElapsedMs { get; set; }
    
    /// <summary>
    /// 额外信息
    /// </summary>
    [SugarColumn(ColumnName = "ExtraInfo", ColumnDescription = "额外信息", Length = -1)]
    public virtual string? ExtraInfo { get; set; }
    
    /// <summary>
    /// 租户ID
    /// </summary>
    [SugarColumn(ColumnName = "TenantId", ColumnDescription = "租户ID")]
    public virtual long? TenantId { get; set; }
    
    /// <summary>
    /// 隐藏的入参参数（JSON格式，开发人员使用，不对客户显示）
    /// </summary>
    [SugarColumn(ColumnName = "OperParam", ColumnDescription = "隐藏的入参参数（JSON格式，开发人员使用，不对客户显示）", Length = -1)]
    public virtual string? OperParam { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "CreateTime", ColumnDescription = "", DefaultValue = "(getdate())")]
    public virtual DateTime CreateTime { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(ColumnName = "UpdateTime", ColumnDescription = "")]
    public virtual DateTime? UpdateTime { get; set; }
    
}
