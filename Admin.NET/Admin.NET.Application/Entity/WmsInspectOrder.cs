// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 验收流水表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsInspectOrder", "验收流水表")]
public partial class WmsInspectOrder : EntityBaseDel
{
    /// <summary>
    /// 验收流水单据
    /// </summary>
    [SugarColumn(ColumnName = "InspectOrderNo", ColumnDescription = "验收流水单据", Length = 32)]
    public virtual string? InspectOrderNo { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    [SugarColumn(ColumnName = "ExportBillType", ColumnDescription = "单据类型", Length = 32)]
    public virtual string? ExportBillType { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>
    [SugarColumn(ColumnName = "ExportMaterialId", ColumnDescription = "物料ID", Length = 32)]
    public virtual string? ExportMaterialId { get; set; }
    
    /// <summary>
    /// 物料编码
    /// </summary>
    [SugarColumn(ColumnName = "ExportMaterialCode", ColumnDescription = "物料编码", Length = 32)]
    public virtual string? ExportMaterialCode { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    [SugarColumn(ColumnName = "ExportMaterialName", ColumnDescription = "物料名称", Length = 100)]
    public virtual string? ExportMaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    [SugarColumn(ColumnName = "ExportMaterialStandard", ColumnDescription = "物料规格", Length = 100)]
    public virtual string? ExportMaterialStandard { get; set; }
    
    /// <summary>
    /// 物料型号
    /// </summary>
    [SugarColumn(ColumnName = "ExportMaterialModel", ColumnDescription = "物料型号", Length = 100)]
    public virtual string? ExportMaterialModel { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    [SugarColumn(ColumnName = "ExportMaterialType", ColumnDescription = "物料类型", Length = 32)]
    public virtual string? ExportMaterialType { get; set; }
    
    /// <summary>
    /// 物料单位
    /// </summary>
    [SugarColumn(ColumnName = "ExportMaterialUnit", ColumnDescription = "物料单位", Length = 32)]
    public virtual string? ExportMaterialUnit { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    [SugarColumn(ColumnName = "ExportWarehouseId", ColumnDescription = "仓库ID", Length = 32)]
    public virtual string? ExportWarehouseId { get; set; }
    
    /// <summary>
    /// 区域ID
    /// </summary>
    [SugarColumn(ColumnName = "ExportAreaId", ColumnDescription = "区域ID", Length = 32)]
    public virtual string? ExportAreaId { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    [SugarColumn(ColumnName = "ExportSlotCode", ColumnDescription = "储位编码", Length = 32)]
    public virtual string? ExportSlotCode { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    [SugarColumn(ColumnName = "ExportStockCode", ColumnDescription = "托盘条码", Length = 32)]
    public virtual string? ExportStockCode { get; set; }
    
    /// <summary>
    /// 出库数量
    /// </summary>
    [SugarColumn(ColumnName = "ExportQuantity", ColumnDescription = "出库数量", Length = 18, DecimalDigits=0)]
    public virtual decimal? ExportQuantity { get; set; }
    
    /// <summary>
    /// 出库重量
    /// </summary>
    [SugarColumn(ColumnName = "ExportWeight", ColumnDescription = "出库重量", Length = 32)]
    public virtual string? ExportWeight { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [SugarColumn(ColumnName = "ExportProductionDate", ColumnDescription = "生产日期")]
    public virtual DateTime? ExportProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    [SugarColumn(ColumnName = "ExportLoseDate", ColumnDescription = "失效日期")]
    public virtual DateTime? ExportLoseDate { get; set; }
    
    /// <summary>
    /// 部门编码
    /// </summary>
    [SugarColumn(ColumnName = "ExportDepartmentCode", ColumnDescription = "部门编码", Length = 32)]
    public virtual string? ExportDepartmentCode { get; set; }
    
    /// <summary>
    /// 供应商编码
    /// </summary>
    [SugarColumn(ColumnName = "ExpotSupplierCode", ColumnDescription = "供应商编码", Length = 32)]
    public virtual string? ExpotSupplierCode { get; set; }
    
    /// <summary>
    /// 客户编码
    /// </summary>
    [SugarColumn(ColumnName = "ExportCustomerCode", ColumnDescription = "客户编码", Length = 32)]
    public virtual string? ExportCustomerCode { get; set; }
    
    /// <summary>
    /// 任务号
    /// </summary>
    [SugarColumn(ColumnName = "InspectTaskNo", ColumnDescription = "任务号", Length = 32)]
    public virtual string? InspectTaskNo { get; set; }
    
    /// <summary>
    /// 执行标志（01待执行、02正在执行、03已完成、04已上传）
    /// </summary>
    [SugarColumn(ColumnName = "ExportExecuteFlag", ColumnDescription = "执行标志（01待执行、02正在执行、03已完成、04已上传）", Length = 2)]
    public virtual string? ExportExecuteFlag { get; set; }
    
    /// <summary>
    /// 单据创建时间
    /// </summary>
    [SugarColumn(ColumnName = "ExporOrederDate", ColumnDescription = "单据创建时间")]
    public virtual DateTime? ExporOrederDate { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "ExportRemark", ColumnDescription = "备注", Length = -1)]
    public virtual string? ExportRemark { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    [SugarColumn(ColumnName = "ExportLotNo", ColumnDescription = "批次", Length = 32)]
    public virtual string? ExportLotNo { get; set; }
    
    /// <summary>
    /// 验收单据
    /// </summary>
    [SugarColumn(ColumnName = "InspectBillCode", ColumnDescription = "验收单据", Length = 32)]
    public virtual string? InspectBillCode { get; set; }
    
    /// <summary>
    /// 出库序号
    /// </summary>
    [SugarColumn(ColumnName = "ExportListNo", ColumnDescription = "出库序号", Length = 32)]
    public virtual string? ExportListNo { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    [SugarColumn(ColumnName = "ExportStockSlotCode", ColumnDescription = "储位编码", Length = 32)]
    public virtual string? ExportStockSlotCode { get; set; }
    
    /// <summary>
    /// 扫码数量
    /// </summary>
    [SugarColumn(ColumnName = "ScanQuantity", ColumnDescription = "扫码数量", Length = 18, DecimalDigits=0)]
    public virtual decimal? ScanQuantity { get; set; }
    
    /// <summary>
    /// 扫码人
    /// </summary>
    [SugarColumn(ColumnName = "ScanUserNames", ColumnDescription = "扫码人", Length = 32)]
    public virtual string? ScanUserNames { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    [SugarColumn(ColumnName = "CompleteDate", ColumnDescription = "完成时间")]
    public virtual DateTime? CompleteDate { get; set; }
    
    /// <summary>
    /// 拣货数量
    /// </summary>
    [SugarColumn(ColumnName = "PickedNum", ColumnDescription = "拣货数量", Length = 32)]
    public virtual string? PickedNum { get; set; }
    
    /// <summary>
    /// 出库单据明ID
    /// </summary>
    [SugarColumn(ColumnName = "ExportDetailId", ColumnDescription = "出库单据明ID", Length = 32)]
    public virtual string? ExportDetailId { get; set; }
    
    /// <summary>
    /// 托盘总箱数
    /// </summary>
    [SugarColumn(ColumnName = "StockWholeBoxNum", ColumnDescription = "托盘总箱数")]
    public virtual int? StockWholeBoxNum { get; set; }
    
    /// <summary>
    /// 托盘总数量
    /// </summary>
    [SugarColumn(ColumnName = "StockQuantity", ColumnDescription = "托盘总数量", Length = 32)]
    public virtual string? StockQuantity { get; set; }
    
    /// <summary>
    /// 出库类型，0：正常出库，1：分拣出库，2：转运出库，3：托盘出库,4：备料出库，5：验收出库
    /// </summary>
    [SugarColumn(ColumnName = "ExportType", ColumnDescription = "出库类型，0：正常出库，1：分拣出库，2：转运出库，3：托盘出库,4：备料出库，5：验收出库", Length = 32)]
    public virtual string? ExportType { get; set; }
    
    /// <summary>
    /// 软删除
    /// </summary>
    [SugarColumn(ColumnName = "IsDelete", ColumnDescription = "软删除")]
    public virtual int? IsDelete { get; set; }
    
    /// <summary>
    /// 检查状态
    /// </summary>
    [SugarColumn(ColumnName = "InspectionStatus", ColumnDescription = "检查状态")]
    public virtual int? InspectionStatus { get; set; }
    
    /// <summary>
    /// 箱数
    /// </summary>
    [SugarColumn(ColumnName = "WholeBoxNum", ColumnDescription = "箱数", Length = 32)]
    public virtual string? WholeBoxNum { get; set; }
    
    /// <summary>
    /// 移出类型
    /// </summary>
    [SugarColumn(ColumnName = "OutType", ColumnDescription = "移出类型")]
    public virtual int? OutType { get; set; }
    
    /// <summary>
    /// 排序状态
    /// </summary>
    [SugarColumn(ColumnName = "OrderByStatus", ColumnDescription = "排序状态")]
    public virtual int? OrderByStatus { get; set; }
    
    /// <summary>
    /// 托盘id
    /// </summary>
    [SugarColumn(ColumnName = "TrayId", ColumnDescription = "托盘id", Length = 32)]
    public virtual string? TrayId { get; set; }
    
}
