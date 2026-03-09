// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using System.ComponentModel.DataAnnotations;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

namespace Admin.NET.Application;

/// <summary>
/// 出库流水基础输入参数
/// </summary>
public class WmsExportOrderBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 出库流水单据
    /// </summary>
    public virtual string? ExportOrderNo { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    public virtual long? ExportBillType { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>
    public virtual long? ExportMaterialId { get; set; }
    
    /// <summary>
    /// 物料编码
    /// </summary>
    public virtual string? ExportMaterialCode { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    public virtual string? ExportMaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    public virtual string? ExportMaterialStandard { get; set; }
    
    /// <summary>
    /// 物料型号
    /// </summary>
    public virtual string? ExportMaterialModel { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    public virtual string? ExportMaterialType { get; set; }
    
    /// <summary>
    /// 物料单位
    /// </summary>
    public virtual string? ExportMaterialUnit { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public virtual long? ExportWarehouseId { get; set; }
    
    /// <summary>
    /// 区域ID
    /// </summary>
    public virtual long? ExportAreaId { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    public virtual string? ExportSlotCode { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    public virtual string? ExportStockCode { get; set; }
    
    /// <summary>
    /// 出库数量
    /// </summary>
    public virtual decimal? ExportQuantity { get; set; }
    
    /// <summary>
    /// 出库重量
    /// </summary>
    public virtual string? ExportWeight { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public virtual DateTime? ExportProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    public virtual DateTime? ExportLoseDate { get; set; }
    
    /// <summary>
    /// 部门编码
    /// </summary>
    public virtual long? ExportDepartmentCode { get; set; }
    
    /// <summary>
    /// 供应商编码
    /// </summary>
    public virtual long? ExpotSupplierCode { get; set; }
    
    /// <summary>
    /// 客户编码
    /// </summary>
    public virtual long? ExportCustomerCode { get; set; }
    
    /// <summary>
    /// 任务号
    /// </summary>
    public virtual string? ExportTaskNo { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在执行、2已完成、3已上传）
    /// </summary>
    public virtual int? ExportExecuteFlag { get; set; }
    
    /// <summary>
    /// 单据创建时间
    /// </summary>
    public virtual DateTime? ExporOrederDate { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? ExportRemark { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    public virtual string? ExportLotNo { get; set; }
    
    /// <summary>
    /// 出库单据
    /// </summary>
    public virtual string? ExportBillCode { get; set; }
    
    /// <summary>
    /// 出库序号
    /// </summary>
    public virtual string? ExportListNo { get; set; }
    
    /// <summary>
    /// 扫码数量
    /// </summary>
    public virtual decimal? ScanQuantity { get; set; }
    
    /// <summary>
    /// 扫码人
    /// </summary>
    public virtual string? ScanUserNames { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    public virtual DateTime? CompleteDate { get; set; }
    
    /// <summary>
    /// 拣货数量
    /// </summary>
    public virtual decimal? PickedNum { get; set; }
    
    /// <summary>
    /// 出库单据明细ID
    /// </summary>
    public virtual long? ExportDetailId { get; set; }
    
    /// <summary>
    /// 箱数
    /// </summary>
    public virtual string? WholeBoxNum { get; set; }
    
    /// <summary>
    /// 托盘类型
    /// </summary>
    public virtual int? OutType { get; set; }
    
    /// <summary>
    /// 托盘总箱数
    /// </summary>
    public virtual string? StockWholeBoxNum { get; set; }
    
    /// <summary>
    /// 托盘总数量
    /// </summary>
    public virtual string? StockQuantity { get; set; }
    
    /// <summary>
    /// 出库类型，0：正常出库，1：分拣出库，2：转运出库，3：托盘出库,4：备料出库
    /// </summary>
    public virtual int? ExportType { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>
    public virtual int? InspectionStatus { get; set; }
    
    /// <summary>
    /// WholeOutWare
    /// </summary>
    public virtual string? WholeOutWare { get; set; }
    
    /// <summary>
    /// 根据状态排序
    /// </summary>
    public virtual int? OrderByStatus { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    public virtual string? ExportStockSlotCode { get; set; }
    
}

/// <summary>
/// 出库流水分页查询输入参数
/// </summary>
public class PageWmsExportOrderInput : BasePageInput
{
    /// <summary>
    /// 出库流水单据
    /// </summary>
    public string? ExportOrderNo { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    public long? ExportBillType { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>
    public long? ExportMaterialId { get; set; }
    
    /// <summary>
    /// 物料编码
    /// </summary>
    public string? ExportMaterialCode { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    public string? ExportMaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    public string? ExportMaterialStandard { get; set; }
    
    /// <summary>
    /// 物料型号
    /// </summary>
    public string? ExportMaterialModel { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    public string? ExportMaterialType { get; set; }
    
    /// <summary>
    /// 物料单位
    /// </summary>
    public string? ExportMaterialUnit { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? ExportWarehouseId { get; set; }
    
    /// <summary>
    /// 区域ID
    /// </summary>
    public long? ExportAreaId { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    public string? ExportSlotCode { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    public string? ExportStockCode { get; set; }
    
    /// <summary>
    /// 出库数量
    /// </summary>
    public decimal? ExportQuantity { get; set; }
    
    /// <summary>
    /// 出库重量
    /// </summary>
    public string? ExportWeight { get; set; }
    
    /// <summary>
    /// 生产日期范围
    /// </summary>
     public DateTime?[] ExportProductionDateRange { get; set; }
    
    /// <summary>
    /// 失效日期范围
    /// </summary>
     public DateTime?[] ExportLoseDateRange { get; set; }
    
    /// <summary>
    /// 部门编码
    /// </summary>
    public long? ExportDepartmentCode { get; set; }
    
    /// <summary>
    /// 供应商编码
    /// </summary>
    public long? ExpotSupplierCode { get; set; }
    
    /// <summary>
    /// 客户编码
    /// </summary>
    public long? ExportCustomerCode { get; set; }
    
    /// <summary>
    /// 任务号
    /// </summary>
    public string? ExportTaskNo { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在执行、2已完成、3已上传）
    /// </summary>
    public int? ExportExecuteFlag { get; set; }
    
    /// <summary>
    /// 单据创建时间范围
    /// </summary>
     public DateTime?[] ExporOrederDateRange { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? ExportRemark { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    public string? ExportLotNo { get; set; }
    
    /// <summary>
    /// 出库单据
    /// </summary>
    public string? ExportBillCode { get; set; }
    
    /// <summary>
    /// 出库序号
    /// </summary>
    public string? ExportListNo { get; set; }
    
    /// <summary>
    /// 扫码数量
    /// </summary>
    public decimal? ScanQuantity { get; set; }
    
    /// <summary>
    /// 扫码人
    /// </summary>
    public string? ScanUserNames { get; set; }
    
    /// <summary>
    /// 完成时间范围
    /// </summary>
     public DateTime?[] CompleteDateRange { get; set; }
    
    /// <summary>
    /// 拣货数量
    /// </summary>
    public decimal? PickedNum { get; set; }
    
    /// <summary>
    /// 出库单据明细ID
    /// </summary>
    public long? ExportDetailId { get; set; }
    
    /// <summary>
    /// 箱数
    /// </summary>
    public string? WholeBoxNum { get; set; }
    
    /// <summary>
    /// 托盘类型
    /// </summary>
    public int? OutType { get; set; }
    
    /// <summary>
    /// 托盘总箱数
    /// </summary>
    public string? StockWholeBoxNum { get; set; }
    
    /// <summary>
    /// 托盘总数量
    /// </summary>
    public string? StockQuantity { get; set; }
    
    /// <summary>
    /// 出库类型，0：正常出库，1：分拣出库，2：转运出库，3：托盘出库,4：备料出库
    /// </summary>
    public int? ExportType { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// WholeOutWare
    /// </summary>
    public string? WholeOutWare { get; set; }
    
    /// <summary>
    /// 根据状态排序
    /// </summary>
    public int? OrderByStatus { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    public string? ExportStockSlotCode { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 出库流水增加输入参数
/// </summary>
public class AddWmsExportOrderInput
{
    /// <summary>
    /// 出库流水单据
    /// </summary>
    [MaxLength(32, ErrorMessage = "出库流水单据字符长度不能超过32")]
    public string? ExportOrderNo { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    public long? ExportBillType { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>
    public long? ExportMaterialId { get; set; }
    
    /// <summary>
    /// 物料编码
    /// </summary>
    [MaxLength(200, ErrorMessage = "物料编码字符长度不能超过200")]
    public string? ExportMaterialCode { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    [MaxLength(200, ErrorMessage = "物料名称字符长度不能超过200")]
    public string? ExportMaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    [MaxLength(200, ErrorMessage = "物料规格字符长度不能超过200")]
    public string? ExportMaterialStandard { get; set; }
    
    /// <summary>
    /// 物料型号
    /// </summary>
    [MaxLength(100, ErrorMessage = "物料型号字符长度不能超过100")]
    public string? ExportMaterialModel { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    [MaxLength(100, ErrorMessage = "物料类型字符长度不能超过100")]
    public string? ExportMaterialType { get; set; }
    
    /// <summary>
    /// 物料单位
    /// </summary>
    [MaxLength(100, ErrorMessage = "物料单位字符长度不能超过100")]
    public string? ExportMaterialUnit { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? ExportWarehouseId { get; set; }
    
    /// <summary>
    /// 区域ID
    /// </summary>
    public long? ExportAreaId { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    [MaxLength(100, ErrorMessage = "储位编码字符长度不能超过100")]
    public string? ExportSlotCode { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    [MaxLength(100, ErrorMessage = "托盘条码字符长度不能超过100")]
    public string? ExportStockCode { get; set; }
    
    /// <summary>
    /// 出库数量
    /// </summary>
    public decimal? ExportQuantity { get; set; }
    
    /// <summary>
    /// 出库重量
    /// </summary>
    [MaxLength(100, ErrorMessage = "出库重量字符长度不能超过100")]
    public string? ExportWeight { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ExportProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    public DateTime? ExportLoseDate { get; set; }
    
    /// <summary>
    /// 部门编码
    /// </summary>
    public long? ExportDepartmentCode { get; set; }
    
    /// <summary>
    /// 供应商编码
    /// </summary>
    public long? ExpotSupplierCode { get; set; }
    
    /// <summary>
    /// 客户编码
    /// </summary>
    public long? ExportCustomerCode { get; set; }
    
    /// <summary>
    /// 任务号
    /// </summary>
    [MaxLength(100, ErrorMessage = "任务号字符长度不能超过100")]
    public string? ExportTaskNo { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在执行、2已完成、3已上传）
    /// </summary>
    public int? ExportExecuteFlag { get; set; }
    
    /// <summary>
    /// 单据创建时间
    /// </summary>
    public DateTime? ExporOrederDate { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(2147483647, ErrorMessage = "备注字符长度不能超过2147483647")]
    public string? ExportRemark { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    [MaxLength(100, ErrorMessage = "批次字符长度不能超过100")]
    public string? ExportLotNo { get; set; }
    
    /// <summary>
    /// 出库单据
    /// </summary>
    [MaxLength(100, ErrorMessage = "出库单据字符长度不能超过100")]
    public string? ExportBillCode { get; set; }
    
    /// <summary>
    /// 出库序号
    /// </summary>
    [MaxLength(100, ErrorMessage = "出库序号字符长度不能超过100")]
    public string? ExportListNo { get; set; }
    
    /// <summary>
    /// 扫码数量
    /// </summary>
    public decimal? ScanQuantity { get; set; }
    
    /// <summary>
    /// 扫码人
    /// </summary>
    [MaxLength(100, ErrorMessage = "扫码人字符长度不能超过100")]
    public string? ScanUserNames { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompleteDate { get; set; }
    
    /// <summary>
    /// 拣货数量
    /// </summary>
    public decimal? PickedNum { get; set; }
    
    /// <summary>
    /// 出库单据明细ID
    /// </summary>
    public long? ExportDetailId { get; set; }
    
    /// <summary>
    /// 箱数
    /// </summary>
    [MaxLength(100, ErrorMessage = "箱数字符长度不能超过100")]
    public string? WholeBoxNum { get; set; }
    
    /// <summary>
    /// 托盘类型
    /// </summary>
    public int? OutType { get; set; }
    
    /// <summary>
    /// 托盘总箱数
    /// </summary>
    [MaxLength(100, ErrorMessage = "托盘总箱数字符长度不能超过100")]
    public string? StockWholeBoxNum { get; set; }
    
    /// <summary>
    /// 托盘总数量
    /// </summary>
    [MaxLength(100, ErrorMessage = "托盘总数量字符长度不能超过100")]
    public string? StockQuantity { get; set; }
    
    /// <summary>
    /// 出库类型，0：正常出库，1：分拣出库，2：转运出库，3：托盘出库,4：备料出库
    /// </summary>
    public int? ExportType { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// WholeOutWare
    /// </summary>
    [MaxLength(64, ErrorMessage = "WholeOutWare字符长度不能超过64")]
    public string? WholeOutWare { get; set; }
    
    /// <summary>
    /// 根据状态排序
    /// </summary>
    public int? OrderByStatus { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    [MaxLength(32, ErrorMessage = "储位编码字符长度不能超过32")]
    public string? ExportStockSlotCode { get; set; }
    
}

/// <summary>
/// 出库流水删除输入参数
/// </summary>
public class DeleteWmsExportOrderInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 出库流水更新输入参数
/// </summary>
public class UpdateWmsExportOrderInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 出库流水单据
    /// </summary>    
    [MaxLength(32, ErrorMessage = "出库流水单据字符长度不能超过32")]
    public string? ExportOrderNo { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>    
    public long? ExportBillType { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>    
    public long? ExportMaterialId { get; set; }
    
    /// <summary>
    /// 物料编码
    /// </summary>    
    [MaxLength(200, ErrorMessage = "物料编码字符长度不能超过200")]
    public string? ExportMaterialCode { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>    
    [MaxLength(200, ErrorMessage = "物料名称字符长度不能超过200")]
    public string? ExportMaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>    
    [MaxLength(200, ErrorMessage = "物料规格字符长度不能超过200")]
    public string? ExportMaterialStandard { get; set; }
    
    /// <summary>
    /// 物料型号
    /// </summary>    
    [MaxLength(100, ErrorMessage = "物料型号字符长度不能超过100")]
    public string? ExportMaterialModel { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>    
    [MaxLength(100, ErrorMessage = "物料类型字符长度不能超过100")]
    public string? ExportMaterialType { get; set; }
    
    /// <summary>
    /// 物料单位
    /// </summary>    
    [MaxLength(100, ErrorMessage = "物料单位字符长度不能超过100")]
    public string? ExportMaterialUnit { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>    
    public long? ExportWarehouseId { get; set; }
    
    /// <summary>
    /// 区域ID
    /// </summary>    
    public long? ExportAreaId { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>    
    [MaxLength(100, ErrorMessage = "储位编码字符长度不能超过100")]
    public string? ExportSlotCode { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>    
    [MaxLength(100, ErrorMessage = "托盘条码字符长度不能超过100")]
    public string? ExportStockCode { get; set; }
    
    /// <summary>
    /// 出库数量
    /// </summary>    
    public decimal? ExportQuantity { get; set; }
    
    /// <summary>
    /// 出库重量
    /// </summary>    
    [MaxLength(100, ErrorMessage = "出库重量字符长度不能超过100")]
    public string? ExportWeight { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>    
    public DateTime? ExportProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>    
    public DateTime? ExportLoseDate { get; set; }
    
    /// <summary>
    /// 部门编码
    /// </summary>    
    public long? ExportDepartmentCode { get; set; }
    
    /// <summary>
    /// 供应商编码
    /// </summary>    
    public long? ExpotSupplierCode { get; set; }
    
    /// <summary>
    /// 客户编码
    /// </summary>    
    public long? ExportCustomerCode { get; set; }
    
    /// <summary>
    /// 任务号
    /// </summary>    
    [MaxLength(100, ErrorMessage = "任务号字符长度不能超过100")]
    public string? ExportTaskNo { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在执行、2已完成、3已上传）
    /// </summary>    
    public int? ExportExecuteFlag { get; set; }
    
    /// <summary>
    /// 单据创建时间
    /// </summary>    
    public DateTime? ExporOrederDate { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>    
    [MaxLength(2147483647, ErrorMessage = "备注字符长度不能超过2147483647")]
    public string? ExportRemark { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>    
    [MaxLength(100, ErrorMessage = "批次字符长度不能超过100")]
    public string? ExportLotNo { get; set; }
    
    /// <summary>
    /// 出库单据
    /// </summary>    
    [MaxLength(100, ErrorMessage = "出库单据字符长度不能超过100")]
    public string? ExportBillCode { get; set; }
    
    /// <summary>
    /// 出库序号
    /// </summary>    
    [MaxLength(100, ErrorMessage = "出库序号字符长度不能超过100")]
    public string? ExportListNo { get; set; }
    
    /// <summary>
    /// 扫码数量
    /// </summary>    
    public decimal? ScanQuantity { get; set; }
    
    /// <summary>
    /// 扫码人
    /// </summary>    
    [MaxLength(100, ErrorMessage = "扫码人字符长度不能超过100")]
    public string? ScanUserNames { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>    
    public DateTime? CompleteDate { get; set; }
    
    /// <summary>
    /// 拣货数量
    /// </summary>    
    public decimal? PickedNum { get; set; }
    
    /// <summary>
    /// 出库单据明细ID
    /// </summary>    
    public long? ExportDetailId { get; set; }
    
    /// <summary>
    /// 箱数
    /// </summary>    
    [MaxLength(100, ErrorMessage = "箱数字符长度不能超过100")]
    public string? WholeBoxNum { get; set; }
    
    /// <summary>
    /// 托盘类型
    /// </summary>    
    public int? OutType { get; set; }
    
    /// <summary>
    /// 托盘总箱数
    /// </summary>    
    [MaxLength(100, ErrorMessage = "托盘总箱数字符长度不能超过100")]
    public string? StockWholeBoxNum { get; set; }
    
    /// <summary>
    /// 托盘总数量
    /// </summary>    
    [MaxLength(100, ErrorMessage = "托盘总数量字符长度不能超过100")]
    public string? StockQuantity { get; set; }
    
    /// <summary>
    /// 出库类型，0：正常出库，1：分拣出库，2：转运出库，3：托盘出库,4：备料出库
    /// </summary>    
    public int? ExportType { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>    
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// WholeOutWare
    /// </summary>    
    [MaxLength(64, ErrorMessage = "WholeOutWare字符长度不能超过64")]
    public string? WholeOutWare { get; set; }
    
    /// <summary>
    /// 根据状态排序
    /// </summary>    
    public int? OrderByStatus { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>    
    [MaxLength(32, ErrorMessage = "储位编码字符长度不能超过32")]
    public string? ExportStockSlotCode { get; set; }
    
}

/// <summary>
/// 出库流水主键查询输入参数
/// </summary>
public class QueryByIdWmsExportOrderInput : DeleteWmsExportOrderInput
{
}

/// <summary>
/// 出库流水数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsExportOrderInput : BaseImportInput
{
    /// <summary>
    /// 出库流水单据
    /// </summary>
    [ImporterHeader(Name = "出库流水单据")]
    [ExporterHeader("出库流水单据", Format = "", Width = 25, IsBold = true)]
    public string? ExportOrderNo { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    [ImporterHeader(Name = "单据类型")]
    [ExporterHeader("单据类型", Format = "", Width = 25, IsBold = true)]
    public long? ExportBillType { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>
    [ImporterHeader(Name = "物料ID")]
    [ExporterHeader("物料ID", Format = "", Width = 25, IsBold = true)]
    public long? ExportMaterialId { get; set; }
    
    /// <summary>
    /// 物料编码
    /// </summary>
    [ImporterHeader(Name = "物料编码")]
    [ExporterHeader("物料编码", Format = "", Width = 25, IsBold = true)]
    public string? ExportMaterialCode { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    [ImporterHeader(Name = "物料名称")]
    [ExporterHeader("物料名称", Format = "", Width = 25, IsBold = true)]
    public string? ExportMaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    [ImporterHeader(Name = "物料规格")]
    [ExporterHeader("物料规格", Format = "", Width = 25, IsBold = true)]
    public string? ExportMaterialStandard { get; set; }
    
    /// <summary>
    /// 物料型号
    /// </summary>
    [ImporterHeader(Name = "物料型号")]
    [ExporterHeader("物料型号", Format = "", Width = 25, IsBold = true)]
    public string? ExportMaterialModel { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    [ImporterHeader(Name = "物料类型")]
    [ExporterHeader("物料类型", Format = "", Width = 25, IsBold = true)]
    public string? ExportMaterialType { get; set; }
    
    /// <summary>
    /// 物料单位
    /// </summary>
    [ImporterHeader(Name = "物料单位")]
    [ExporterHeader("物料单位", Format = "", Width = 25, IsBold = true)]
    public string? ExportMaterialUnit { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    [ImporterHeader(Name = "仓库ID")]
    [ExporterHeader("仓库ID", Format = "", Width = 25, IsBold = true)]
    public long? ExportWarehouseId { get; set; }
    
    /// <summary>
    /// 区域ID
    /// </summary>
    [ImporterHeader(Name = "区域ID")]
    [ExporterHeader("区域ID", Format = "", Width = 25, IsBold = true)]
    public long? ExportAreaId { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    [ImporterHeader(Name = "储位编码")]
    [ExporterHeader("储位编码", Format = "", Width = 25, IsBold = true)]
    public string? ExportSlotCode { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    [ImporterHeader(Name = "托盘条码")]
    [ExporterHeader("托盘条码", Format = "", Width = 25, IsBold = true)]
    public string? ExportStockCode { get; set; }
    
    /// <summary>
    /// 出库数量
    /// </summary>
    [ImporterHeader(Name = "出库数量")]
    [ExporterHeader("出库数量", Format = "", Width = 25, IsBold = true)]
    public decimal? ExportQuantity { get; set; }
    
    /// <summary>
    /// 出库重量
    /// </summary>
    [ImporterHeader(Name = "出库重量")]
    [ExporterHeader("出库重量", Format = "", Width = 25, IsBold = true)]
    public string? ExportWeight { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [ImporterHeader(Name = "生产日期")]
    [ExporterHeader("生产日期", Format = "", Width = 25, IsBold = true)]
    public DateTime? ExportProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    [ImporterHeader(Name = "失效日期")]
    [ExporterHeader("失效日期", Format = "", Width = 25, IsBold = true)]
    public DateTime? ExportLoseDate { get; set; }
    
    /// <summary>
    /// 部门编码
    /// </summary>
    [ImporterHeader(Name = "部门编码")]
    [ExporterHeader("部门编码", Format = "", Width = 25, IsBold = true)]
    public long? ExportDepartmentCode { get; set; }
    
    /// <summary>
    /// 供应商编码
    /// </summary>
    [ImporterHeader(Name = "供应商编码")]
    [ExporterHeader("供应商编码", Format = "", Width = 25, IsBold = true)]
    public long? ExpotSupplierCode { get; set; }
    
    /// <summary>
    /// 客户编码
    /// </summary>
    [ImporterHeader(Name = "客户编码")]
    [ExporterHeader("客户编码", Format = "", Width = 25, IsBold = true)]
    public long? ExportCustomerCode { get; set; }
    
    /// <summary>
    /// 任务号
    /// </summary>
    [ImporterHeader(Name = "任务号")]
    [ExporterHeader("任务号", Format = "", Width = 25, IsBold = true)]
    public string? ExportTaskNo { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在执行、2已完成、3已上传）
    /// </summary>
    [ImporterHeader(Name = "执行标志（0待执行、1正在执行、2已完成、3已上传）")]
    [ExporterHeader("执行标志（0待执行、1正在执行、2已完成、3已上传）", Format = "", Width = 25, IsBold = true)]
    public int? ExportExecuteFlag { get; set; }
    
    /// <summary>
    /// 单据创建时间
    /// </summary>
    [ImporterHeader(Name = "单据创建时间")]
    [ExporterHeader("单据创建时间", Format = "", Width = 25, IsBold = true)]
    public DateTime? ExporOrederDate { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? ExportRemark { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    [ImporterHeader(Name = "批次")]
    [ExporterHeader("批次", Format = "", Width = 25, IsBold = true)]
    public string? ExportLotNo { get; set; }
    
    /// <summary>
    /// 出库单据
    /// </summary>
    [ImporterHeader(Name = "出库单据")]
    [ExporterHeader("出库单据", Format = "", Width = 25, IsBold = true)]
    public string? ExportBillCode { get; set; }
    
    /// <summary>
    /// 出库序号
    /// </summary>
    [ImporterHeader(Name = "出库序号")]
    [ExporterHeader("出库序号", Format = "", Width = 25, IsBold = true)]
    public string? ExportListNo { get; set; }
    
    /// <summary>
    /// 扫码数量
    /// </summary>
    [ImporterHeader(Name = "扫码数量")]
    [ExporterHeader("扫码数量", Format = "", Width = 25, IsBold = true)]
    public decimal? ScanQuantity { get; set; }
    
    /// <summary>
    /// 扫码人
    /// </summary>
    [ImporterHeader(Name = "扫码人")]
    [ExporterHeader("扫码人", Format = "", Width = 25, IsBold = true)]
    public string? ScanUserNames { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    [ImporterHeader(Name = "完成时间")]
    [ExporterHeader("完成时间", Format = "", Width = 25, IsBold = true)]
    public DateTime? CompleteDate { get; set; }
    
    /// <summary>
    /// 拣货数量
    /// </summary>
    [ImporterHeader(Name = "拣货数量")]
    [ExporterHeader("拣货数量", Format = "", Width = 25, IsBold = true)]
    public decimal? PickedNum { get; set; }
    
    /// <summary>
    /// 出库单据明细ID
    /// </summary>
    [ImporterHeader(Name = "出库单据明细ID")]
    [ExporterHeader("出库单据明细ID", Format = "", Width = 25, IsBold = true)]
    public long? ExportDetailId { get; set; }
    
    /// <summary>
    /// 箱数
    /// </summary>
    [ImporterHeader(Name = "箱数")]
    [ExporterHeader("箱数", Format = "", Width = 25, IsBold = true)]
    public string? WholeBoxNum { get; set; }
    
    /// <summary>
    /// 托盘类型
    /// </summary>
    [ImporterHeader(Name = "托盘类型")]
    [ExporterHeader("托盘类型", Format = "", Width = 25, IsBold = true)]
    public int? OutType { get; set; }
    
    /// <summary>
    /// 托盘总箱数
    /// </summary>
    [ImporterHeader(Name = "托盘总箱数")]
    [ExporterHeader("托盘总箱数", Format = "", Width = 25, IsBold = true)]
    public string? StockWholeBoxNum { get; set; }
    
    /// <summary>
    /// 托盘总数量
    /// </summary>
    [ImporterHeader(Name = "托盘总数量")]
    [ExporterHeader("托盘总数量", Format = "", Width = 25, IsBold = true)]
    public string? StockQuantity { get; set; }
    
    /// <summary>
    /// 出库类型，0：正常出库，1：分拣出库，2：转运出库，3：托盘出库,4：备料出库
    /// </summary>
    [ImporterHeader(Name = "出库类型，0：正常出库，1：分拣出库，2：转运出库，3：托盘出库,4：备料出库")]
    [ExporterHeader("出库类型，0：正常出库，1：分拣出库，2：转运出库，3：托盘出库,4：备料出库", Format = "", Width = 25, IsBold = true)]
    public int? ExportType { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>
    [ImporterHeader(Name = "质检状态")]
    [ExporterHeader("质检状态", Format = "", Width = 25, IsBold = true)]
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// WholeOutWare
    /// </summary>
    [ImporterHeader(Name = "WholeOutWare")]
    [ExporterHeader("WholeOutWare", Format = "", Width = 25, IsBold = true)]
    public string? WholeOutWare { get; set; }
    
    /// <summary>
    /// 根据状态排序
    /// </summary>
    [ImporterHeader(Name = "根据状态排序")]
    [ExporterHeader("根据状态排序", Format = "", Width = 25, IsBold = true)]
    public int? OrderByStatus { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    [ImporterHeader(Name = "储位编码")]
    [ExporterHeader("储位编码", Format = "", Width = 25, IsBold = true)]
    public string? ExportStockSlotCode { get; set; }
    
}
