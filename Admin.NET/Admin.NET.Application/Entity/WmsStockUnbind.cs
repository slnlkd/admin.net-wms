// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 托盘变更记录表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsStockUnbind", "托盘变更记录表")]
public partial class WmsStockUnbind : EntityBase
{
    /// <summary>
    /// 原托盘编码
    /// </summary>
    [SugarColumn(ColumnName = "UpbindStockCode", ColumnDescription = "原托盘编码", Length = 32)]
    public virtual string? UpbindStockCode { get; set; }

    /// <summary>
    /// 新托盘编码
    /// </summary>
    [SugarColumn(ColumnName = "BindStockCode", ColumnDescription = "新托盘编码", Length = 32)]
    public virtual string? BindStockCode { get; set; }

    /// <summary>
    /// 批次号
    /// </summary>
    [SugarColumn(ColumnName = "StockLotNo", ColumnDescription = "批次号", Length = 32)]
    public virtual string? StockLotNo { get; set; }

    /// <summary>
    /// 物料ID
    /// </summary>
    [SugarColumn(ColumnName = "MaterialId", ColumnDescription = "物料ID", Length = 32)]
    public virtual string? MaterialId { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    [SugarColumn(ColumnName = "MaterialName", ColumnDescription = "物料名称", Length = 255)]
    public virtual string? MaterialName { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    [SugarColumn(ColumnName = "MaterialStandard", ColumnDescription = "物料规格", Length = 255)]
    public virtual string? MaterialStandard { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    [SugarColumn(ColumnName = "Qty", ColumnDescription = "数量", Length = 18, DecimalDigits = 3)]
    public virtual decimal? Qty { get; set; }

    

}

