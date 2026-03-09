// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsBackToWareHouse", "")]
public partial class WmsBackToWareHouse : EntityBaseDel
{
    /// <summary>
    /// 托盘号
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "TrayNO", ColumnDescription = "托盘号", Length = 10)]
    public virtual string TrayNO { get; set; }
    
    /// <summary>
    /// 回库暂存地址
    /// </summary>
    [SugarColumn(ColumnName = "TemporaryLocation", ColumnDescription = "回库暂存地址", Length = 50)]
    public virtual string? TemporaryLocation { get; set; }
    
    /// <summary>
    /// 回库中转口
    /// </summary>
    [SugarColumn(ColumnName = "TemporaryGroup", ColumnDescription = "回库中转口", Length = 50)]
    public virtual string? TemporaryGroup { get; set; }
    
    /// <summary>
    /// 回移目标地址
    /// </summary>
    [SugarColumn(ColumnName = "TargetLocation", ColumnDescription = "回移目标地址", Length = 50)]
    public virtual string? TargetLocation { get; set; }
    
    /// <summary>
    /// 回移中转
    /// </summary>
    [SugarColumn(ColumnName = "TargetGroup", ColumnDescription = "回移中转", Length = 50)]
    public virtual string? TargetGroup { get; set; }
    
    /// <summary>
    /// 优先级
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "Sequence", ColumnDescription = "优先级")]
    public virtual int Sequence { get; set; }
    
    /// <summary>
    /// 同组尾托标记//1是尾盘 0非尾盘
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "EndMark", ColumnDescription = "同组尾托标记//1是尾盘 0非尾盘", DefaultValue = "((0))")]
    public virtual int EndMark { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(ColumnName = "State", ColumnDescription = "")]
    public virtual int? State { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(ColumnName = "IfMoveBack", ColumnDescription = "")]
    public virtual int? IfMoveBack { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(ColumnName = "BillCode", ColumnDescription = "", Length = 50)]
    public virtual string? BillCode { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(ColumnName = "SlotInout", ColumnDescription = "")]
    public virtual int? SlotInout { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(ColumnName = "SlotLanewayId", ColumnDescription = "")]
    public virtual int? SlotLanewayId { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(ColumnName = "ExportOrderNo", ColumnDescription = "", Length = 30)]
    public virtual string? ExportOrderNo { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(ColumnName = "OrderByStatus", ColumnDescription = "")]
    public virtual int? OrderByStatus { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(ColumnName = "ProduceId", ColumnDescription = "", Length = 32)]
    public virtual string? ProduceId { get; set; }
    
}
