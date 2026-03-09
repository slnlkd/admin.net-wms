// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 出库单据表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsExportNotify", "出库单据表")]
public partial class WmsExportNotify : EntityBaseDel
{
    /// <summary>
    /// 出库单据
    /// </summary>
    [SugarColumn(ColumnName = "ExportBillCode", ColumnDescription = "出库单据", Length = 100)]
    public virtual string? ExportBillCode { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    [SugarColumn(ColumnName = "ExportBillType", ColumnDescription = "单据类型")]
    public virtual long? ExportBillType { get; set; }
    
    /// <summary>
    /// 出库批次
    /// </summary>
    [SugarColumn(ColumnName = "ExportLotNo", ColumnDescription = "出库批次", Length = 200)]
    public virtual string? ExportLotNo { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>
    [SugarColumn(ColumnName = "MaterialId", ColumnDescription = "物料ID")]
    public virtual long? MaterialId { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseId", ColumnDescription = "仓库ID")]
    public virtual long? WarehouseId { get; set; }
    
    /// <summary>
    /// 出库序号
    /// </summary>
    [SugarColumn(ColumnName = "ExportListNo", ColumnDescription = "出库序号", Length = 100)]
    public virtual string? ExportListNo { get; set; }
    
    /// <summary>
    /// 部门ID
    /// </summary>
    [SugarColumn(ColumnName = "ExportDepartmentId", ColumnDescription = "部门ID")]
    public virtual long? ExportDepartmentId { get; set; }
    
    /// <summary>
    /// 供应商ID
    /// </summary>
    [SugarColumn(ColumnName = "ExportSupplierId", ColumnDescription = "供应商ID")]
    public virtual long? ExportSupplierId { get; set; }
    
    /// <summary>
    /// 客户ID
    /// </summary>
    [SugarColumn(ColumnName = "ExportCustomerId", ColumnDescription = "客户ID")]
    public virtual long? ExportCustomerId { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [SugarColumn(ColumnName = "ExportProductionDate", ColumnDescription = "生产日期")]
    public virtual DateTime? ExportProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    [SugarColumn(ColumnName = "ExportLostDate", ColumnDescription = "失效日期")]
    public virtual DateTime? ExportLostDate { get; set; }
    
    /// <summary>
    /// 计划出库数量
    /// </summary>
    [SugarColumn(ColumnName = "ExportQuantity", ColumnDescription = "计划出库数量", Length = 18, DecimalDigits=5)]
    public virtual decimal? ExportQuantity { get; set; }
    
    /// <summary>
    /// 任务下发数量
    /// </summary>
    [SugarColumn(ColumnName = "ExportFactQuantity", ColumnDescription = "任务下发数量", Length = 18, DecimalDigits=5)]
    public virtual decimal? ExportFactQuantity { get; set; }
    
    /// <summary>
    /// 任务完成数量
    /// </summary>
    [SugarColumn(ColumnName = "ExportCompleteQuantity", ColumnDescription = "任务完成数量", Length = 18, DecimalDigits=5)]
    public virtual decimal? ExportCompleteQuantity { get; set; }
    
    /// <summary>
    /// 已上传数量
    /// </summary>
    [SugarColumn(ColumnName = "ExportUploadQuantity", ColumnDescription = "已上传数量", Length = 18, DecimalDigits=5)]
    public virtual decimal? ExportUploadQuantity { get; set; }
    
    /// <summary>
    /// 建单时间
    /// </summary>
    [SugarColumn(ColumnName = "ExportDate", ColumnDescription = "建单时间")]
    public virtual DateTime? ExportDate { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在分配、 2正在执行、3已完成、5已上传） 4作废
    /// </summary>
    [SugarColumn(ColumnName = "ExportExecuteFlag", ColumnDescription = "执行标志（0待执行、1正在分配、 2正在执行、3已完成、5已上传） 4作废")]
    public virtual int? ExportExecuteFlag { get; set; }

    /// <summary>
    /// 出库添加时间
    /// </summary>
    [SugarColumn(ColumnName = "ExportAddDateTime", ColumnDescription = "出库添加时间")]
    public virtual DateTime? ExportAddDateTime { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "ExportRemark", ColumnDescription = "备注", Length = 2147483647)]
    public virtual string? ExportRemark { get; set; }
    
    /// <summary>
    /// 外部单据编码
    /// </summary>
    [SugarColumn(ColumnName = "OuterBillCode", ColumnDescription = "外部单据编码", Length = 100)]
    public virtual string? OuterBillCode { get; set; }
    
    /// <summary>
    /// 外部单据ID
    /// </summary>
    [SugarColumn(ColumnName = "OuterMainId", ColumnDescription = "外部单据ID", Length = 100)]
    public virtual string? OuterMainId { get; set; }
    
    /// <summary>
    /// 来源（wms\sap）
    /// </summary>
    [SugarColumn(ColumnName = "Source", ColumnDescription = "来源（wms或sap）", Length = 100)]
    public virtual string? Source { get; set; }
    
    /// <summary>
    /// 拣货区
    /// </summary>
    [SugarColumn(ColumnName = "PickingArea", ColumnDescription = "拣货区", Length = 100)]
    public virtual string? PickingArea { get; set; }
    
    /// <summary>
    /// 拼箱状态（0：不拼箱1：拼箱）
    /// </summary>
    [SugarColumn(ColumnName = "PXStatus", ColumnDescription = "拼箱状态（0：不拼箱1：拼箱）")]
    public virtual int? PXStatus { get; set; }
    
    /// <summary>
    /// 整托出库口
    /// </summary>
    [SugarColumn(ColumnName = "WholeOutWare", ColumnDescription = "整托出库口", Length = 100)]
    public virtual string? WholeOutWare { get; set; }
    
    /// <summary>
    /// 分拣出库口
    /// </summary>
    [SugarColumn(ColumnName = "SortOutWare", ColumnDescription = "分拣出库口", Length = 100)]
    public virtual string? SortOutWare { get; set; }
    
    /// <summary>
    /// 软删除
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "IsDelete", ColumnDescription = "软删除")]
    public virtual bool IsDelete { get; set; }
    
    /// <summary>
    /// 单据子类型编码
    /// </summary>
    [SugarColumn(ColumnName = "DocumentSubtype", ColumnDescription = "单据子类型编码")]
    public virtual long? DocumentSubtype { get; set; }
    
    /// <summary>
    /// 出库口id
    /// </summary>
    [SugarColumn(ColumnName = "WayOutId", ColumnDescription = "出库口id", Length = 1)]
    public virtual string? WayOutId { get; set; }
    
}
