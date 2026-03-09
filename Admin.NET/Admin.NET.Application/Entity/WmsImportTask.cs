// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 入库任务表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsImportTask", "入库任务表")]
public partial class WmsImportTask : EntityBaseDel
{
    /// <summary>
    /// 任务号
    /// </summary>
    [SugarColumn(ColumnName = "TaskNo", ColumnDescription = "任务号", Length = 255)]
    public virtual string? TaskNo { get; set; }
    
    /// <summary>
    /// 发送方
    /// </summary>
    [SugarColumn(ColumnName = "Sender", ColumnDescription = "发送方", Length = 255)]
    public virtual string? Sender { get; set; }
    
    /// <summary>
    /// 接收方
    /// </summary>
    [SugarColumn(ColumnName = "Receiver", ColumnDescription = "接收方", Length = 255)]
    public virtual string? Receiver { get; set; }
    
    /// <summary>
    /// 是否成功
    /// </summary>
    [SugarColumn(ColumnName = "IsSuccess", ColumnDescription = "是否成功")]
    public virtual int? IsSuccess { get; set; }
    
    /// <summary>
    /// 发送时间
    /// </summary>
    [SugarColumn(ColumnName = "SendDate", ColumnDescription = "发送时间")]
    public virtual DateTime? SendDate { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    [SugarColumn(ColumnName = "BackDate", ColumnDescription = "结束时间")]
    public virtual DateTime? BackDate { get; set; }
    
    /// <summary>
    /// 描述发送报文
    /// </summary>
    [SugarColumn(ColumnName = "Message", ColumnDescription = "描述发送报文", Length = 255)]
    public virtual string? Message { get; set; }
    
    /// <summary>
    /// 托盘码
    /// </summary>
    [SugarColumn(ColumnName = "StockCode", ColumnDescription = "托盘码", Length = 255)]
    public virtual string? StockCode { get; set; }
    
    /// <summary>
    /// 描述返回信息
    /// </summary>
    [SugarColumn(ColumnName = "Msg", ColumnDescription = "描述返回信息", Length = 255)]
    public virtual string? Msg { get; set; }
    
    /// <summary>
    /// 任务状态
    /// </summary>
    [SugarColumn(ColumnName = "Status", ColumnDescription = "任务状态")]
    public virtual int? Status { get; set; }
    
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
    /// 仓库ID
    /// </summary>
    [SugarColumn(ColumnName = "WareHouseId", ColumnDescription = "仓库ID")]
    public virtual long? WareHouseId { get; set; }
    
    /// <summary>
    /// 起始位置
    /// </summary>
    [SugarColumn(ColumnName = "StartLocation", ColumnDescription = "起始位置", Length = 50)]
    public virtual string? StartLocation { get; set; }
    
    /// <summary>
    /// 目标位置
    /// </summary>
    [SugarColumn(ColumnName = "EndLocation", ColumnDescription = "目标位置", Length = 50)]
    public virtual string? EndLocation { get; set; }
    
    /// <summary>
    /// 二次申请后目标位置
    /// </summary>
    [SugarColumn(ColumnName = "NewEnd", ColumnDescription = "二次申请后目标位置", Length = 32)]
    public virtual string? NewEnd { get; set; }
    
}
