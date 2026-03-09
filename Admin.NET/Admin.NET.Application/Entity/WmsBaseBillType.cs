// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 单据类型表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsBaseBillType", "单据类型表")]
public partial class WmsBaseBillType : EntityBaseDel
{
    /// <summary>
    /// 单据类型编码
    /// </summary>
    [SugarColumn(ColumnName = "BillTypeCode", ColumnDescription = "单据类型编码", Length = 64)]
    public virtual string? BillTypeCode { get; set; }
    
    /// <summary>
    /// 单据类型名称
    /// </summary>
    [SugarColumn(ColumnName = "BillTypeName", ColumnDescription = "单据类型名称", Length = 64)]
    public virtual string? BillTypeName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark", ColumnDescription = "备注", Length = 255)]
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 类型(1入库2出库)
    /// </summary>
    [SugarColumn(ColumnName = "BillType", ColumnDescription = "类型(1入库2出库)")]
    public virtual int? BillType { get; set; }
    
    /// <summary>
    /// 质检状态（0待检，1合格，2不合格）
    /// </summary>
    [SugarColumn(ColumnName = "QualityInspectionStatus", ColumnDescription = "质检状态（0待检，1合格，2不合格）", Length = 64)]
    public virtual string? QualityInspectionStatus { get; set; }
    
    /// <summary>
    /// 代储状态（0：否；1：是）
    /// </summary>
    [SugarColumn(ColumnName = "ProxyStatus", ColumnDescription = "代储状态（0：否；1：是）")]
    public virtual int? ProxyStatus { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    [SugarColumn(ColumnName = "WareHouseId", ColumnDescription = "所属仓库")]
    public virtual long? WareHouseId { get; set; }
    
    /// <summary>
    /// 单据子类型上级编码
    /// </summary>
    [SugarColumn(ColumnName = "SubtypeID", ColumnDescription = "单据子类型上级编码", Length = 64)]
    public virtual string? SubtypeID { get; set; }
    
}
