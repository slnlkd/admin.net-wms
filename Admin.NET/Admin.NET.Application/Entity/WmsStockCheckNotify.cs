// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 盘点单据
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsStockCheckNotify", "盘点单据")]
public partial class WmsStockCheckNotify : EntityBaseDel
{
    /// <summary>
    /// 盘库编号
    /// </summary>
    [SugarColumn(ColumnName = "CheckBillCode", ColumnDescription = "盘库编号", Length = 32)]
    public virtual string? CheckBillCode { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnName = "CheckDate", ColumnDescription = "创建时间")]
    public virtual DateTime? CheckDate { get; set; }
    
    /// <summary>
    /// 仓库Id
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseId", ColumnDescription = "仓库Id", Length = 32)]
    public virtual string? WarehouseId { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在执行、2待调整、3执行完毕）
    /// </summary>
    [SugarColumn(ColumnName = "ExecuteFlag", ColumnDescription = "执行标志（0待执行、1正在执行、2待调整、3执行完毕）")]
    public virtual int? ExecuteFlag { get; set; }
    /// <summary>
    /// 盘库备注
    /// </summary>
    [SugarColumn(ColumnName = "CheckRemark", ColumnDescription = "盘库备注", Length = 500)]
    public virtual string? CheckRemark { get; set; }
    
    /// <summary>
    /// 添加时间
    /// </summary>
    [SugarColumn(ColumnName = "AddDate", ColumnDescription = "添加时间")]
    public virtual DateTime? AddDate { get; set; }
    
}
