// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// wms接口日志表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsInterfaceLog", "wms接口日志表")]
public partial class WmsInterfaceLog : EntityBaseDel
{
    /// <summary>
    /// 托盘转运表任务号
    /// </summary>
    [SugarColumn(ColumnName = "TaskId", ColumnDescription = "托盘转运表任务号", Length = 32)]
    public virtual string? TaskId { get; set; }
    
    /// <summary>
    /// 接口名称
    /// </summary>
    [SugarColumn(ColumnName = "Name", ColumnDescription = "接口名称", Length = 100)]
    public virtual string? Name { get; set; }
    
    /// <summary>
    /// 接口地址
    /// </summary>
    [SugarColumn(ColumnName = "Address", ColumnDescription = "接口地址", Length = 200)]
    public virtual string? Address { get; set; }
    
    /// <summary>
    /// 是否成功（0否；1是）
    /// </summary>
    [SugarColumn(ColumnName = "Status", ColumnDescription = "是否成功（0否；1是）", Length = 2)]
    public virtual string? Status { get; set; }
    
    /// <summary>
    /// 接口系统（wms；wcs；erp；血源系统等）
    /// </summary>
    [SugarColumn(ColumnName = "Os", ColumnDescription = "接口系统（wms；wcs；erp；血源系统等）", Length = 5)]
    public virtual string? Os { get; set; }
    
    /// <summary>
    /// 发送时间
    /// </summary>
    [SugarColumn(ColumnName = "DateTime", ColumnDescription = "发送时间", Length = 32)]
    public virtual string? DateTime { get; set; }
    
    /// <summary>
    /// 发送方
    /// </summary>
    [SugarColumn(ColumnName = "Sender", ColumnDescription = "发送方", Length = 20)]
    public virtual string? Sender { get; set; }
    
    /// <summary>
    /// 接收方
    /// </summary>
    [SugarColumn(ColumnName = "Receiver", ColumnDescription = "接收方", Length = 20)]
    public virtual string? Receiver { get; set; }
    
    /// <summary>
    /// 描述信息
    /// </summary>
    [SugarColumn(ColumnName = "Msg", ColumnDescription = "描述信息", Length = -1)]
    public virtual string? Msg { get; set; }
    
    /// <summary>
    /// 接口内容
    /// </summary>
    [SugarColumn(ColumnName = "Data", ColumnDescription = "接口内容", Length = -1)]
    public virtual string? Data { get; set; }
    
    /// <summary>
    /// 软删除
    /// </summary>
    [SugarColumn(ColumnName = "IsDelete", ColumnDescription = "软删除")]
    public virtual bool? IsDelete { get; set; }
    
}
