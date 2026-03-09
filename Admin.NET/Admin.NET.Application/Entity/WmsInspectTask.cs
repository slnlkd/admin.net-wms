// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 验收任务表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsInspectTask", "验收任务表")]
public partial class WmsInspectTask : EntityBase
{
    /// <summary>
    /// 验收任务号
    /// </summary>
    [SugarColumn(ColumnName = "InspectTaskNo", ColumnDescription = "验收任务号", Length = 50)]
    public virtual string? InspectTaskNo { get; set; }
    
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
    /// 是否成功（0否、1是）
    /// </summary>
    [SugarColumn(ColumnName = "IsSuccess", ColumnDescription = "是否成功（0否、1是）")]
    public virtual int? IsSuccess { get; set; }
    
    /// <summary>
    /// 信息
    /// </summary>
    [SugarColumn(ColumnName = "Information", ColumnDescription = "信息", Length = -1)]
    public virtual string? Information { get; set; }
    
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
    /// 托盘条码
    /// </summary>
    [SugarColumn(ColumnName = "StockCode", ColumnDescription = "托盘条码", Length = 50)]
    public virtual string? StockCode { get; set; }
    
    /// <summary>
    /// 描述信息
    /// </summary>
    [SugarColumn(ColumnName = "Msg", ColumnDescription = "描述信息", Length = 200)]
    public virtual string? Msg { get; set; }
    
    /// <summary>
    /// 执行状态（0：已下发；1：执行中；2：执行完毕）
    /// </summary>
    [SugarColumn(ColumnName = "ExportTaskFlag", ColumnDescription = "执行状态（0：已下发；1：执行中；2：执行完毕）")]
    public virtual int? ExportTaskFlag { get; set; }
    
    /// <summary>
    /// 出库流水单据
    /// </summary>
    [SugarColumn(ColumnName = "ExportOrderNo", ColumnDescription = "出库流水单据", Length = 50)]
    public virtual string? ExportOrderNo { get; set; }
    
    /// <summary>
    /// 软删除
    /// </summary>
    [SugarColumn(ColumnName = "IsDelete", ColumnDescription = "软删除")]
    public virtual int? IsDelete { get; set; }
    
    /// <summary>
    /// 开始位置
    /// </summary>
    [SugarColumn(ColumnName = "StartLocation", ColumnDescription = "开始位置", Length = 50)]
    public virtual string? StartLocation { get; set; }
    
    /// <summary>
    /// 结束位置
    /// </summary>
    [SugarColumn(ColumnName = "EndLocation", ColumnDescription = "结束位置", Length = 50)]
    public virtual string? EndLocation { get; set; }
    
    /// <summary>
    /// 仓库id
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseId", ColumnDescription = "仓库id", Length = 32)]
    public virtual string? WarehouseId { get; set; }
    
}
