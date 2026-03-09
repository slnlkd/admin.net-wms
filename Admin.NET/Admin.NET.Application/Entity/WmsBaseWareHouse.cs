// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 仓库表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsBaseWareHouse", "仓库表")]
public partial class WmsBaseWareHouse : EntityBaseDel
{
    /// <summary>
    /// 仓库编码
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseCode", ColumnDescription = "仓库编码", Length = 32)]
    public virtual string? WarehouseCode { get; set; }
    
    /// <summary>
    /// 仓库名称
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseName", ColumnDescription = "仓库名称", Length = 32)]
    public virtual string? WarehouseName { get; set; }
    
    /// <summary>
    /// 仓库类型
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseType", ColumnDescription = "仓库类型", Length = 32)]
    public virtual string? WarehouseType { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark", ColumnDescription = "备注", Length = 32)]
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 是否允许超出
    /// </summary>
    [SugarColumn(ColumnName = "IsExceeding", ColumnDescription = "是否允许超出", DefaultValue = "((0))")]
    public virtual bool? IsExceeding { get; set; }
    
    /// <summary>
    /// 是否允许超入
    /// </summary>
    [SugarColumn(ColumnName = "IsOverbooking", ColumnDescription = "是否允许超入", DefaultValue = "((0))")]
    public virtual bool? IsOverbooking { get; set; }
    
    /// <summary>
    /// 是否允许少出
    /// </summary>
    [SugarColumn(ColumnName = "IsUnderpay", ColumnDescription = "是否允许少出", DefaultValue = "((0))")]
    public virtual bool? IsUnderpay { get; set; }
    
    /// <summary>
    /// 是否允许少入
    /// </summary>
    [SugarColumn(ColumnName = "IsEnterless", ColumnDescription = "是否允许少入", DefaultValue = "((0))")]
    public virtual bool? IsEnterless { get; set; }
    
}
