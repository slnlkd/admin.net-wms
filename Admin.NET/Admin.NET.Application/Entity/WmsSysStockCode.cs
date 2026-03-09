// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 托盘管理
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsSysStockCode", "托盘管理")]
public partial class WmsSysStockCode : EntityBaseDel
{
    /// <summary>
    /// 托盘条码
    /// </summary>
    [SugarColumn(ColumnName = "StockCode", ColumnDescription = "托盘条码", Length = 50)]
    public virtual string? StockCode { get; set; }

    /// <summary>
    /// 状态(0:未使用,1正在使用)
    /// </summary>
    [SugarColumn(ColumnName = "Status", ColumnDescription = "状态(0:未使用,1正在使用)")]
    public virtual int? Status { get; set; } = 0;
    
    /// <summary>
    /// 打印次数
    /// </summary>
    [SugarColumn(ColumnName = "PrintCount", ColumnDescription = "打印次数")]
    public virtual int? PrintCount { get; set; }
    
    /// <summary>
    /// 托盘类型
    /// </summary>
    [SugarColumn(ColumnName = "StockType", ColumnDescription = "托盘类型")]
    public virtual long? StockType { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseId", ColumnDescription = "所属仓库")]
    public virtual long? WarehouseId { get; set; }
    
}
