// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 拆跺信息表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsManualDepalletizing", "拆跺信息表")]
public partial class WmsManualDepalletizing : EntityBaseDel
{
    /// <summary>
    /// 箱码表
    /// </summary>
    [SugarColumn(ColumnName = "BoxCode", ColumnDescription = "箱码表", Length = 100)]
    public virtual string? BoxCode { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    [SugarColumn(ColumnName = "Qty", ColumnDescription = "数量", Length = 100)]
    public virtual string? Qty { get; set; }
    
    /// <summary>
    /// 拆跺数量
    /// </summary>
    [SugarColumn(ColumnName = "ScanQty", ColumnDescription = "拆跺数量", Length = 100)]
    public virtual string? ScanQty { get; set; }
    
    /// <summary>
    /// 托盘号
    /// </summary>
    [SugarColumn(ColumnName = "TrayCode", ColumnDescription = "托盘号", Length = 100)]
    public virtual string? TrayCode { get; set; }
    
    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnName = "State", ColumnDescription = "状态")]
    public virtual int? State { get; set; }
    
    /// <summary>
    /// 流水编号
    /// </summary>
    [SugarColumn(ColumnName = "ExportOrderNo", ColumnDescription = "流水编号", Length = 100)]
    public virtual string? ExportOrderNo { get; set; }
    
    /// <summary>
    /// 拆跺件数
    /// </summary>
    [SugarColumn(ColumnName = "OutQty", ColumnDescription = "拆跺件数", Length = 18, DecimalDigits=5)]
    public virtual decimal? OutQty { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "IsDelete", ColumnDescription = "软删除")]
    public virtual bool IsDelete { get; set; }
    
}
