// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 移库任务表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsMoveTask", "移库任务表")]
public partial class WmsMoveTask : EntityBaseDel
{
    /// <summary>
    /// 任务号
    /// </summary>
    [SugarColumn(ColumnName = "TaskNo", ColumnDescription = "任务号", Length = 50)]
    public virtual string? TaskNo { get; set; }
    
    /// <summary>
    /// 发送方
    /// </summary>
    [SugarColumn(ColumnName = "Sender", ColumnDescription = "发送方", Length = 50)]
    public virtual string? Sender { get; set; }
    
    /// <summary>
    /// 接收方
    /// </summary>
    [SugarColumn(ColumnName = "Receiver", ColumnDescription = "接收方", Length = 50)]
    public virtual string? Receiver { get; set; }
    
    /// <summary>
    /// 是否成功
    /// </summary>
    [SugarColumn(ColumnName = "IsSuccess", ColumnDescription = "是否成功")]
    public virtual int? IsSuccess { get; set; }
    
    /// <summary>
    /// 信息
    /// </summary>
    [SugarColumn(ColumnName = "Information", ColumnDescription = "信息", Length = 50)]
    public virtual string? Information { get; set; }
    
    /// <summary>
    /// 发送时间
    /// </summary>
    [SugarColumn(ColumnName = "SendDate", ColumnDescription = "发送时间")]
    public virtual DateTime? SendDate { get; set; }
    
    /// <summary>
    /// 返回时间
    /// </summary>
    [SugarColumn(ColumnName = "BackDate", ColumnDescription = "返回时间")]
    public virtual DateTime? BackDate { get; set; }
    
    /// <summary>
    /// 消息时间
    /// </summary>
    [SugarColumn(ColumnName = "MessageDate", ColumnDescription = "消息时间", Length = 100)]
    public virtual string? MessageDate { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    [SugarColumn(ColumnName = "StockCodeId", ColumnDescription = "托盘条码", Length = 50)]
    public virtual string? StockCodeId { get; set; }
    
    /// <summary>
    /// 描述信息
    /// </summary>
    [SugarColumn(ColumnName = "Msg", ColumnDescription = "描述信息", Length = 100)]
    public virtual string? Msg { get; set; }
    
    /// <summary>
    /// 是否发送
    /// </summary>
    [SugarColumn(ColumnName = "IsSend", ColumnDescription = "是否发送")]
    public virtual int? IsSend { get; set; }
    
    /// <summary>
    /// 是否取消
    /// </summary>
    [SugarColumn(ColumnName = "IsCancel", ColumnDescription = "是否取消")]
    public virtual int? IsCancel { get; set; }
    
    /// <summary>
    /// 是否完成
    /// </summary>
    [SugarColumn(ColumnName = "IsFinish", ColumnDescription = "是否完成")]
    public virtual int? IsFinish { get; set; }
    
    /// <summary>
    /// 取消时间
    /// </summary>
    [SugarColumn(ColumnName = "CancelDate", ColumnDescription = "取消时间")]
    public virtual DateTime? CancelDate { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    [SugarColumn(ColumnName = "FinishDate", ColumnDescription = "完成时间")]
    public virtual DateTime? FinishDate { get; set; }
    
    /// <summary>
    /// 是否展示
    /// </summary>
    [SugarColumn(ColumnName = "IsShow", ColumnDescription = "是否展示")]
    public virtual int? IsShow { get; set; }
    
    /// <summary>
    /// 组任务id
    /// </summary>
    [SugarColumn(ColumnName = "goupTaskId", ColumnDescription = "组任务id", Length = 100)]
    public virtual string? goupTaskId { get; set; }
    
    /// <summary>
    /// 任务状态（0未开始；1执行中；2执行完成；3手动完成；4手动取消）
    /// </summary>
    [SugarColumn(ColumnName = "Status", ColumnDescription = "任务状态（0未开始；1执行中；2执行完成；3手动完成；4手动取消）")]
    public virtual int? Status { get; set; }
    
    /// <summary>
    /// 软删除
    /// </summary>
    [SugarColumn(ColumnName = "IsDelete", ColumnDescription = "软删除")]
    public virtual int? IsDelete { get; set; }
    
}
