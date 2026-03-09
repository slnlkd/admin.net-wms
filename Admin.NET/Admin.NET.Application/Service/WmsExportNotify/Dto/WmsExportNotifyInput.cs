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
/// 出库单据表基础输入参数
/// </summary>
public class WmsExportNotifyBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 出库单据
    /// </summary>
    public virtual string? ExportBillCode { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    public virtual long? ExportBillType { get; set; }
    
    /// <summary>
    /// 出库批次
    /// </summary>
    public virtual string? ExportLotNo { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>
    public virtual long? MaterialId { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public virtual long? WarehouseId { get; set; }
    
    /// <summary>
    /// 出库序号
    /// </summary>
    public virtual string? ExportListNo { get; set; }
    
    /// <summary>
    /// 部门ID
    /// </summary>
    public virtual long? ExportDepartmentId { get; set; }
    
    /// <summary>
    /// 供应商ID
    /// </summary>
    public virtual long? ExportSupplierId { get; set; }
    
    /// <summary>
    /// 客户ID
    /// </summary>
    public virtual long? ExportCustomerId { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public virtual DateTime? ExportProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    public virtual DateTime? ExportLostDate { get; set; }
    
    /// <summary>
    /// 计划出库数量
    /// </summary>
    public virtual decimal? ExportQuantity { get; set; }
    
    /// <summary>
    /// 任务下发数量
    /// </summary>
    public virtual decimal? ExportFactQuantity { get; set; }
    
    /// <summary>
    /// 任务完成数量
    /// </summary>
    public virtual decimal? ExportCompleteQuantity { get; set; }
    
    /// <summary>
    /// 已上传数量
    /// </summary>
    public virtual decimal? ExportUploadQuantity { get; set; }
    
    /// <summary>
    /// 建单时间
    /// </summary>
    public virtual DateTime? ExportDate { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在分配、 2正在执行、3已完成、5已上传） 4作废
    /// </summary>
    public virtual int? ExportExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? ExportRemark { get; set; }
    
    /// <summary>
    /// 外部单据编码
    /// </summary>
    public virtual string? OuterBillCode { get; set; }
    
    /// <summary>
    /// 外部单据ID
    /// </summary>
    public virtual string? OuterMainId { get; set; }
    
    /// <summary>
    /// 来源（wms或sap）
    /// </summary>
    public virtual string? Source { get; set; }
    
    /// <summary>
    /// 拣货区
    /// </summary>
    public virtual string? PickingArea { get; set; }
    
    /// <summary>
    /// 拼箱状态（0：不拼箱1：拼箱）
    /// </summary>
    public virtual int? PXStatus { get; set; }
    
    /// <summary>
    /// 整托出库口
    /// </summary>
    public virtual string? WholeOutWare { get; set; }
    
    /// <summary>
    /// 分拣出库口
    /// </summary>
    public virtual string? SortOutWare { get; set; }
    
    /// <summary>
    /// 单据子类型编码
    /// </summary>
    public virtual long? DocumentSubtype { get; set; }
    
    /// <summary>
    /// 出库口id
    /// </summary>
    public virtual string? WayOutId { get; set; }
    
    /// <summary>
    /// 出库添加时间
    /// </summary>
    public virtual DateTime? ExportAddDateTime { get; set; }
    
}

/// <summary>
/// 出库单据表分页查询输入参数
/// </summary>
public class PageWmsExportNotifyInput : BasePageInput
{
    /// <summary>
    /// 出库单据
    /// </summary>
    public string? ExportBillCode { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    public long? ExportBillType { get; set; }
    
    /// <summary>
    /// 出库批次
    /// </summary>
    public string? ExportLotNo { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>
    public long? MaterialId { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 出库序号
    /// </summary>
    public string? ExportListNo { get; set; }
    
    /// <summary>
    /// 部门ID
    /// </summary>
    public long? ExportDepartmentId { get; set; }
    
    /// <summary>
    /// 供应商ID
    /// </summary>
    public long? ExportSupplierId { get; set; }
    
    /// <summary>
    /// 客户ID
    /// </summary>
    public long? ExportCustomerId { get; set; }
    
    /// <summary>
    /// 生产日期范围
    /// </summary>
     public DateTime?[] ExportProductionDateRange { get; set; }
    
    /// <summary>
    /// 失效日期范围
    /// </summary>
     public DateTime?[] ExportLostDateRange { get; set; }
    
    /// <summary>
    /// 计划出库数量
    /// </summary>
    public decimal? ExportQuantity { get; set; }
    
    /// <summary>
    /// 任务下发数量
    /// </summary>
    public decimal? ExportFactQuantity { get; set; }
    
    /// <summary>
    /// 任务完成数量
    /// </summary>
    public decimal? ExportCompleteQuantity { get; set; }
    
    /// <summary>
    /// 已上传数量
    /// </summary>
    public decimal? ExportUploadQuantity { get; set; }
    
    /// <summary>
    /// 建单时间范围
    /// </summary>
     public DateTime?[] ExportDateRange { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在分配、 2正在执行、3已完成、5已上传） 4作废
    /// </summary>
    public int? ExportExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? ExportRemark { get; set; }
    
    /// <summary>
    /// 外部单据编码
    /// </summary>
    public string? OuterBillCode { get; set; }
    
    /// <summary>
    /// 外部单据ID
    /// </summary>
    public string? OuterMainId { get; set; }
    
    /// <summary>
    /// 来源（wms或sap）
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// 拣货区
    /// </summary>
    public string? PickingArea { get; set; }
    
    /// <summary>
    /// 拼箱状态（0：不拼箱1：拼箱）
    /// </summary>
    public int? PXStatus { get; set; }
    
    /// <summary>
    /// 整托出库口
    /// </summary>
    public string? WholeOutWare { get; set; }
    
    /// <summary>
    /// 分拣出库口
    /// </summary>
    public string? SortOutWare { get; set; }
    
    /// <summary>
    /// 单据子类型编码
    /// </summary>
    public long? DocumentSubtype { get; set; }
    
    /// <summary>
    /// 出库口id
    /// </summary>
    public string? WayOutId { get; set; }
    
    /// <summary>
    /// 出库添加时间范围
    /// </summary>
     public DateTime?[] ExportAddDateTimeRange { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 出库单据表增加输入参数
/// </summary>
public class AddWmsExportNotifyInput
{
    /// <summary>
    /// 出库单据
    /// </summary>
    [MaxLength(100, ErrorMessage = "出库单据字符长度不能超过100")]
    public string? ExportBillCode { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    public long? ExportBillType { get; set; }
    
    /// <summary>
    /// 出库批次
    /// </summary>
    [MaxLength(200, ErrorMessage = "出库批次字符长度不能超过200")]
    public string? ExportLotNo { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>
    public long? MaterialId { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 出库序号
    /// </summary>
    [MaxLength(100, ErrorMessage = "出库序号字符长度不能超过100")]
    public string? ExportListNo { get; set; }
    
    /// <summary>
    /// 部门ID
    /// </summary>
    public long? ExportDepartmentId { get; set; }
    
    /// <summary>
    /// 供应商ID
    /// </summary>
    public long? ExportSupplierId { get; set; }
    
    /// <summary>
    /// 客户ID
    /// </summary>
    public long? ExportCustomerId { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ExportProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    public DateTime? ExportLostDate { get; set; }
    
    /// <summary>
    /// 计划出库数量
    /// </summary>
    public decimal? ExportQuantity { get; set; }
    
    /// <summary>
    /// 任务下发数量
    /// </summary>
    public decimal? ExportFactQuantity { get; set; }
    
    /// <summary>
    /// 任务完成数量
    /// </summary>
    public decimal? ExportCompleteQuantity { get; set; }
    
    /// <summary>
    /// 已上传数量
    /// </summary>
    public decimal? ExportUploadQuantity { get; set; }
    
    /// <summary>
    /// 建单时间
    /// </summary>
    public DateTime? ExportDate { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在分配、 2正在执行、3已完成、5已上传） 4作废
    /// </summary>
    public int? ExportExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(2147483647, ErrorMessage = "备注字符长度不能超过2147483647")]
    public string? ExportRemark { get; set; }
    
    /// <summary>
    /// 外部单据编码
    /// </summary>
    [MaxLength(100, ErrorMessage = "外部单据编码字符长度不能超过100")]
    public string? OuterBillCode { get; set; }
    
    /// <summary>
    /// 外部单据ID
    /// </summary>
    [MaxLength(100, ErrorMessage = "外部单据ID字符长度不能超过100")]
    public string? OuterMainId { get; set; }
    
    /// <summary>
    /// 来源（wms或sap）
    /// </summary>
    [MaxLength(100, ErrorMessage = "来源（wms或sap）字符长度不能超过100")]
    public string? Source { get; set; }
    
    /// <summary>
    /// 拣货区
    /// </summary>
    [MaxLength(100, ErrorMessage = "拣货区字符长度不能超过100")]
    public string? PickingArea { get; set; }
    
    /// <summary>
    /// 拼箱状态（0：不拼箱1：拼箱）
    /// </summary>
    public int? PXStatus { get; set; }
    
    /// <summary>
    /// 整托出库口
    /// </summary>
    [MaxLength(100, ErrorMessage = "整托出库口字符长度不能超过100")]
    public string? WholeOutWare { get; set; }
    
    /// <summary>
    /// 分拣出库口
    /// </summary>
    [MaxLength(100, ErrorMessage = "分拣出库口字符长度不能超过100")]
    public string? SortOutWare { get; set; }
    
    /// <summary>
    /// 单据子类型编码
    /// </summary>
    public long? DocumentSubtype { get; set; }
    
    /// <summary>
    /// 出库口id
    /// </summary>
    [MaxLength(100, ErrorMessage = "出库口id字符长度不能超过100")]
    public string? WayOutId { get; set; }
    
    /// <summary>
    /// 出库添加时间
    /// </summary>
    public DateTime? ExportAddDateTime { get; set; }
    
}

/// <summary>
/// 出库单据表删除输入参数
/// </summary>
public class DeleteWmsExportNotifyInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 出库单据表更新输入参数
/// </summary>
public class UpdateWmsExportNotifyInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 出库单据
    /// </summary>    
    [MaxLength(100, ErrorMessage = "出库单据字符长度不能超过100")]
    public string? ExportBillCode { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>    
    public long? ExportBillType { get; set; }
    
    /// <summary>
    /// 出库批次
    /// </summary>    
    [MaxLength(200, ErrorMessage = "出库批次字符长度不能超过200")]
    public string? ExportLotNo { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>    
    public long? MaterialId { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>    
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 出库序号
    /// </summary>    
    [MaxLength(100, ErrorMessage = "出库序号字符长度不能超过100")]
    public string? ExportListNo { get; set; }
    
    /// <summary>
    /// 部门ID
    /// </summary>    
    public long? ExportDepartmentId { get; set; }
    
    /// <summary>
    /// 供应商ID
    /// </summary>    
    public long? ExportSupplierId { get; set; }
    
    /// <summary>
    /// 客户ID
    /// </summary>    
    public long? ExportCustomerId { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>    
    public DateTime? ExportProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>    
    public DateTime? ExportLostDate { get; set; }
    
    /// <summary>
    /// 计划出库数量
    /// </summary>    
    public decimal? ExportQuantity { get; set; }
    
    /// <summary>
    /// 任务下发数量
    /// </summary>    
    public decimal? ExportFactQuantity { get; set; }
    
    /// <summary>
    /// 任务完成数量
    /// </summary>    
    public decimal? ExportCompleteQuantity { get; set; }
    
    /// <summary>
    /// 已上传数量
    /// </summary>    
    public decimal? ExportUploadQuantity { get; set; }
    
    /// <summary>
    /// 建单时间
    /// </summary>    
    public DateTime? ExportDate { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在分配、 2正在执行、3已完成、5已上传） 4作废
    /// </summary>    
    public int? ExportExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>    
    [MaxLength(2147483647, ErrorMessage = "备注字符长度不能超过2147483647")]
    public string? ExportRemark { get; set; }
    
    /// <summary>
    /// 外部单据编码
    /// </summary>    
    [MaxLength(100, ErrorMessage = "外部单据编码字符长度不能超过100")]
    public string? OuterBillCode { get; set; }
    
    /// <summary>
    /// 外部单据ID
    /// </summary>    
    [MaxLength(100, ErrorMessage = "外部单据ID字符长度不能超过100")]
    public string? OuterMainId { get; set; }
    
    /// <summary>
    /// 来源（wms或sap）
    /// </summary>    
    [MaxLength(100, ErrorMessage = "来源（wms或sap）字符长度不能超过100")]
    public string? Source { get; set; }
    
    /// <summary>
    /// 拣货区
    /// </summary>    
    [MaxLength(100, ErrorMessage = "拣货区字符长度不能超过100")]
    public string? PickingArea { get; set; }
    
    /// <summary>
    /// 拼箱状态（0：不拼箱1：拼箱）
    /// </summary>    
    public int? PXStatus { get; set; }
    
    /// <summary>
    /// 整托出库口
    /// </summary>    
    [MaxLength(100, ErrorMessage = "整托出库口字符长度不能超过100")]
    public string? WholeOutWare { get; set; }
    
    /// <summary>
    /// 分拣出库口
    /// </summary>    
    [MaxLength(100, ErrorMessage = "分拣出库口字符长度不能超过100")]
    public string? SortOutWare { get; set; }
    
    /// <summary>
    /// 单据子类型编码
    /// </summary>    
    public long? DocumentSubtype { get; set; }
    
    /// <summary>
    /// 出库口id
    /// </summary>    
    [MaxLength(100, ErrorMessage = "出库口id字符长度不能超过100")]
    public string? WayOutId { get; set; }
    
    /// <summary>
    /// 出库添加时间
    /// </summary>    
    public DateTime? ExportAddDateTime { get; set; }
    
}

/// <summary>
/// 出库单据表主键查询输入参数
/// </summary>
public class QueryByIdWmsExportNotifyInput : DeleteWmsExportNotifyInput
{
}

/// <summary>
/// 出库单据表数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsExportNotifyInput : BaseImportInput
{
    /// <summary>
    /// 出库单据
    /// </summary>
    [ImporterHeader(Name = "出库单据")]
    [ExporterHeader("出库单据", Format = "", Width = 25, IsBold = true)]
    public string? ExportBillCode { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    [ImporterHeader(Name = "单据类型")]
    [ExporterHeader("单据类型", Format = "", Width = 25, IsBold = true)]
    public long? ExportBillType { get; set; }
    
    /// <summary>
    /// 出库批次
    /// </summary>
    [ImporterHeader(Name = "出库批次")]
    [ExporterHeader("出库批次", Format = "", Width = 25, IsBold = true)]
    public string? ExportLotNo { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>
    [ImporterHeader(Name = "物料ID")]
    [ExporterHeader("物料ID", Format = "", Width = 25, IsBold = true)]
    public long? MaterialId { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    [ImporterHeader(Name = "仓库ID")]
    [ExporterHeader("仓库ID", Format = "", Width = 25, IsBold = true)]
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 出库序号
    /// </summary>
    [ImporterHeader(Name = "出库序号")]
    [ExporterHeader("出库序号", Format = "", Width = 25, IsBold = true)]
    public string? ExportListNo { get; set; }
    
    /// <summary>
    /// 部门ID
    /// </summary>
    [ImporterHeader(Name = "部门ID")]
    [ExporterHeader("部门ID", Format = "", Width = 25, IsBold = true)]
    public long? ExportDepartmentId { get; set; }
    
    /// <summary>
    /// 供应商ID
    /// </summary>
    [ImporterHeader(Name = "供应商ID")]
    [ExporterHeader("供应商ID", Format = "", Width = 25, IsBold = true)]
    public long? ExportSupplierId { get; set; }
    
    /// <summary>
    /// 客户ID
    /// </summary>
    [ImporterHeader(Name = "客户ID")]
    [ExporterHeader("客户ID", Format = "", Width = 25, IsBold = true)]
    public long? ExportCustomerId { get; set; }
    
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
    public DateTime? ExportLostDate { get; set; }
    
    /// <summary>
    /// 计划出库数量
    /// </summary>
    [ImporterHeader(Name = "计划出库数量")]
    [ExporterHeader("计划出库数量", Format = "", Width = 25, IsBold = true)]
    public decimal? ExportQuantity { get; set; }
    
    /// <summary>
    /// 任务下发数量
    /// </summary>
    [ImporterHeader(Name = "任务下发数量")]
    [ExporterHeader("任务下发数量", Format = "", Width = 25, IsBold = true)]
    public decimal? ExportFactQuantity { get; set; }
    
    /// <summary>
    /// 任务完成数量
    /// </summary>
    [ImporterHeader(Name = "任务完成数量")]
    [ExporterHeader("任务完成数量", Format = "", Width = 25, IsBold = true)]
    public decimal? ExportCompleteQuantity { get; set; }
    
    /// <summary>
    /// 已上传数量
    /// </summary>
    [ImporterHeader(Name = "已上传数量")]
    [ExporterHeader("已上传数量", Format = "", Width = 25, IsBold = true)]
    public decimal? ExportUploadQuantity { get; set; }
    
    /// <summary>
    /// 建单时间
    /// </summary>
    [ImporterHeader(Name = "建单时间")]
    [ExporterHeader("建单时间", Format = "", Width = 25, IsBold = true)]
    public DateTime? ExportDate { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在分配、 2正在执行、3已完成、5已上传） 4作废
    /// </summary>
    [ImporterHeader(Name = "执行标志（0待执行、1正在分配、 2正在执行、3已完成、5已上传） 4作废")]
    [ExporterHeader("执行标志（0待执行、1正在分配、 2正在执行、3已完成、5已上传） 4作废", Format = "", Width = 25, IsBold = true)]
    public int? ExportExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? ExportRemark { get; set; }
    
    /// <summary>
    /// 外部单据编码
    /// </summary>
    [ImporterHeader(Name = "外部单据编码")]
    [ExporterHeader("外部单据编码", Format = "", Width = 25, IsBold = true)]
    public string? OuterBillCode { get; set; }
    
    /// <summary>
    /// 外部单据ID
    /// </summary>
    [ImporterHeader(Name = "外部单据ID")]
    [ExporterHeader("外部单据ID", Format = "", Width = 25, IsBold = true)]
    public string? OuterMainId { get; set; }
    
    /// <summary>
    /// 来源（wms或sap）
    /// </summary>
    [ImporterHeader(Name = "来源（wms或sap）")]
    [ExporterHeader("来源（wms或sap）", Format = "", Width = 25, IsBold = true)]
    public string? Source { get; set; }
    
    /// <summary>
    /// 拣货区
    /// </summary>
    [ImporterHeader(Name = "拣货区")]
    [ExporterHeader("拣货区", Format = "", Width = 25, IsBold = true)]
    public string? PickingArea { get; set; }
    
    /// <summary>
    /// 拼箱状态（0：不拼箱1：拼箱）
    /// </summary>
    [ImporterHeader(Name = "拼箱状态（0：不拼箱1：拼箱）")]
    [ExporterHeader("拼箱状态（0：不拼箱1：拼箱）", Format = "", Width = 25, IsBold = true)]
    public int? PXStatus { get; set; }
    
    /// <summary>
    /// 整托出库口
    /// </summary>
    [ImporterHeader(Name = "整托出库口")]
    [ExporterHeader("整托出库口", Format = "", Width = 25, IsBold = true)]
    public string? WholeOutWare { get; set; }
    
    /// <summary>
    /// 分拣出库口
    /// </summary>
    [ImporterHeader(Name = "分拣出库口")]
    [ExporterHeader("分拣出库口", Format = "", Width = 25, IsBold = true)]
    public string? SortOutWare { get; set; }
    
    /// <summary>
    /// 单据子类型编码
    /// </summary>
    [ImporterHeader(Name = "单据子类型编码")]
    [ExporterHeader("单据子类型编码", Format = "", Width = 25, IsBold = true)]
    public long? DocumentSubtype { get; set; }
    
    /// <summary>
    /// 出库口id
    /// </summary>
    [ImporterHeader(Name = "出库口id")]
    [ExporterHeader("出库口id", Format = "", Width = 25, IsBold = true)]
    public string? WayOutId { get; set; }
    
    /// <summary>
    /// 出库添加时间
    /// </summary>
    [ImporterHeader(Name = "出库添加时间")]
    [ExporterHeader("出库添加时间", Format = "", Width = 25, IsBold = true)]
    public DateTime? ExportAddDateTime { get; set; }
    
}
