// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 入库单据
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsImportNotify", "入库单据")]
public partial class WmsImportNotify : EntityBaseDel
{
    /// <summary>
    /// 仓库ID
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseId", ColumnDescription = "仓库ID")]
    public virtual long? WarehouseId { get; set; }
    
    /// <summary>
    /// 单号
    /// </summary>
    [SugarColumn(ColumnName = "ImportBillCode", ColumnDescription = "单号", Length = 32)]
    public virtual string? ImportBillCode { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    [SugarColumn(ColumnName = "BillType", ColumnDescription = "单据类型")]
    public virtual long? BillType { get; set; }
    
    /// <summary>
    /// 部门ID
    /// </summary>
    [SugarColumn(ColumnName = "DepartmentId", ColumnDescription = "部门ID")]
    public virtual long? DepartmentId { get; set; }
    
    /// <summary>
    /// 供应商ID
    /// </summary>
    [SugarColumn(ColumnName = "SupplierId", ColumnDescription = "供应商ID")]
    public virtual long? SupplierId { get; set; }
    
    /// <summary>
    /// 客户ID
    /// </summary>
    [SugarColumn(ColumnName = "CustomerId", ColumnDescription = "客户ID")]
    public virtual long? CustomerId { get; set; }
    /// <summary>
    /// 制造商ID
    /// </summary>
    [SugarColumn(ColumnName = "ManufacturerId", ColumnDescription = "制造商ID")]
    public virtual long? ManufacturerId { get; set; }

    /// <summary>
    /// 执行标志（01待执行、02正在执行、03已完成、04已上传）
    /// </summary>
    [SugarColumn(ColumnName = "ImportExecuteFlag", ColumnDescription = "执行标志（01待执行、02正在执行、03已完成、04已上传）", Length = 2)]
    public virtual string? ImportExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark", ColumnDescription = "备注", Length = 50)]
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 外部单号
    /// </summary>
    [SugarColumn(ColumnName = "OuterBillCode", ColumnDescription = "外部单号", Length = 32)]
    public virtual string? OuterBillCode { get; set; }
    
    /// <summary>
    /// 外部单据ID
    /// </summary>
    [SugarColumn(ColumnName = "OuterMainId", ColumnDescription = "外部单据ID", Length = 32)]
    public virtual string? OuterMainId { get; set; }
    
    /// <summary>
    /// 来源（wms\sap）
    /// </summary>
    [SugarColumn(ColumnName = "Source", ColumnDescription = "来源", Length = 50)]
    public virtual string? Source { get; set; }
    
    /// <summary>
    /// 生产商ID
    /// </summary>
    [SugarColumn(ColumnName = "ProduceId", ColumnDescription = "生产商ID")]
    public virtual long? ProduceId { get; set; } 

}
