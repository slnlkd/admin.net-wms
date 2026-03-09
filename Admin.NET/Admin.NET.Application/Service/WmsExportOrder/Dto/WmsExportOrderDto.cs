// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Application;

/// <summary>
/// 出库流水输出参数
/// </summary>
public class WmsExportOrderDto
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
    
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
    /// 软删除
    /// </summary>
    public bool IsDelete { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    
    /// <summary>
    /// 创建者Id
    /// </summary>
    public long? CreateUserId { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
    /// <summary>
    /// 修改者Id
    /// </summary>
    public long? UpdateUserId { get; set; }
    
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
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }
    
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
    
}
