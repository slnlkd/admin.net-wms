// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 仓库区域表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsBaseArea", "仓库区域表")]
public partial class WmsBaseArea : EntityBaseDel
{
    /// <summary>
    /// 区域编码
    /// </summary>
    [SugarColumn(ColumnName = "AreaCode", ColumnDescription = "区域编码", Length = 64)]
    public virtual string? AreaCode { get; set; }
    
    /// <summary>
    /// 区域名称
    /// </summary>
    [SugarColumn(ColumnName = "AreaName", ColumnDescription = "区域名称", Length = 64)]
    public virtual string? AreaName { get; set; }
    
    /// <summary>
    /// 所属库房ID
    /// </summary>
    [SugarColumn(ColumnName = "AreaWarehouseId", ColumnDescription = "所属库房ID")]
    public virtual long? AreaWarehouseId { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark", ColumnDescription = "备注", Length = 255)]
    public virtual string? Remark { get; set; }

    ///// <summary>
    ///// 是否禁用
    ///// </summary>
    //[SugarColumn(ColumnName = "IsDisable", ColumnDescription = "是否禁用", Length = 255)]
    //public virtual string? IsDisable { get; set; } 

    ///// <summary>
    ///// 是否质检托盘
    ///// </summary>
    //[SugarColumn(ColumnName = "IsBulkTank", ColumnDescription = "是否质检托盘", Length = 255)]
    //public virtual string? IsBulkTank { get; set; }
}
