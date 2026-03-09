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
/// 移库单据明细表基础输入参数
/// </summary>
public class WmsMoveNotifyBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 移库单据类型
    /// </summary>
    public virtual string? MoveBillCode { get; set; }
    
    /// <summary>
    /// 移库序号
    /// </summary>
    public virtual int? MoveListNo { get; set; }
    
    /// <summary>
    /// 移库批次
    /// </summary>
    public virtual string? MoveLotNo { get; set; }
    
    /// <summary>
    /// 物料id
    /// </summary>
    public virtual string? MaterialId { get; set; }
    
    /// <summary>
    /// 移库物料型号
    /// </summary>
    public virtual string? MoveMaterialModel { get; set; }
    
    /// <summary>
    /// 物料温度
    /// </summary>
    public virtual string? MoveMaterialTemp { get; set; }
    
    /// <summary>
    /// 物料类型（字典MaterialType）
    /// </summary>
    public virtual string? MoveMaterialType { get; set; }
    
    /// <summary>
    /// 物料状态
    /// </summary>
    public virtual string? MoveMaterialStatus { get; set; }
    
    /// <summary>
    /// 物料单位
    /// </summary>
    public virtual string? MoveMaterialUnit { get; set; }
    
    /// <summary>
    /// 物料品牌
    /// </summary>
    public virtual string? MoveMaterialBrand { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public virtual DateTime? MoveProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    public virtual DateTime? MoveLostDate { get; set; }
    
    /// <summary>
    /// 移库数量
    /// </summary>
    public virtual int? MoveQuantity { get; set; }
    
    /// <summary>
    /// 移库日期
    /// </summary>
    public virtual DateTime? MoveDate { get; set; }
    
    /// <summary>
    /// 仓库id
    /// </summary>
    public virtual long? MoveWarehouseId { get; set; }
    
    /// <summary>
    /// 移出巷道id
    /// </summary>
    public virtual long? MoveLanewayOutCode { get; set; }
    
    /// <summary>
    /// 移入巷道id
    /// </summary>
    public virtual string? MoveLanewayInCode { get; set; }
    
    /// <summary>
    /// 移出储位编码
    /// </summary>
    public virtual string? MoveOutSlotCode { get; set; }
    
    /// <summary>
    /// 移入储位编码
    /// </summary>
    public virtual string? MoveInSlotCode { get; set; }
    
    /// <summary>
    /// 执行标志（01待执行、02正在执行、03已完成、04已上传）
    /// </summary>
    public virtual string? MoveExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? MoveRemark { get; set; }
    
    /// <summary>
    /// 移库任务号
    /// </summary>
    public virtual string? MoveTaskNo { get; set; }
    
    /// <summary>
    /// 库存表id
    /// </summary>
    public virtual long? StockStockCodeId { get; set; }
    
}

/// <summary>
/// 移库单据明细表分页查询输入参数
/// </summary>
public class PageWmsMoveNotifyInput : BasePageInput
{
    /// <summary>
    /// 移库单据类型
    /// </summary>
    public string? MoveBillCode { get; set; }
    
    /// <summary>
    /// 移库序号
    /// </summary>
    public int? MoveListNo { get; set; }
    
    /// <summary>
    /// 移库批次
    /// </summary>
    public string? MoveLotNo { get; set; }
    
    /// <summary>
    /// 物料id
    /// </summary>
    public string? MaterialId { get; set; }
    
    /// <summary>
    /// 移库物料型号
    /// </summary>
    public string? MoveMaterialModel { get; set; }
    
    /// <summary>
    /// 物料温度
    /// </summary>
    public string? MoveMaterialTemp { get; set; }
    
    /// <summary>
    /// 物料类型（字典MaterialType）
    /// </summary>
    public string? MoveMaterialType { get; set; }
    
    /// <summary>
    /// 物料状态
    /// </summary>
    public string? MoveMaterialStatus { get; set; }
    
    /// <summary>
    /// 物料单位
    /// </summary>
    public string? MoveMaterialUnit { get; set; }
    
    /// <summary>
    /// 物料品牌
    /// </summary>
    public string? MoveMaterialBrand { get; set; }
    
    /// <summary>
    /// 生产日期范围
    /// </summary>
     public DateTime?[] MoveProductionDateRange { get; set; }
    
    /// <summary>
    /// 失效日期范围
    /// </summary>
     public DateTime?[] MoveLostDateRange { get; set; }
    
    /// <summary>
    /// 移库数量
    /// </summary>
    public int? MoveQuantity { get; set; }
    
    /// <summary>
    /// 移库日期范围
    /// </summary>
     public DateTime?[] MoveDateRange { get; set; }
    
    /// <summary>
    /// 仓库id
    /// </summary>
    public long? MoveWarehouseId { get; set; }
    
    /// <summary>
    /// 移出巷道id
    /// </summary>
    public long? MoveLanewayOutCode { get; set; }
    
    /// <summary>
    /// 移入巷道id
    /// </summary>
    public string? MoveLanewayInCode { get; set; }
    
    /// <summary>
    /// 移出储位编码
    /// </summary>
    public string? MoveOutSlotCode { get; set; }
    
    /// <summary>
    /// 移入储位编码
    /// </summary>
    public string? MoveInSlotCode { get; set; }
    
    /// <summary>
    /// 执行标志（01待执行、02正在执行、03已完成、04已上传）
    /// </summary>
    public string? MoveExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? MoveRemark { get; set; }
    
    /// <summary>
    /// 移库任务号
    /// </summary>
    public string? MoveTaskNo { get; set; }
    
    /// <summary>
    /// 库存表id
    /// </summary>
    public string? StockStockCodeId { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 移库单据明细表增加输入参数
/// </summary>
public class AddWmsMoveNotifyInput
{
    /// <summary>
    /// 移库单据类型
    /// </summary>
    [MaxLength(50, ErrorMessage = "移库单据类型字符长度不能超过50")]
    public string? MoveBillCode { get; set; }
    
    /// <summary>
    /// 移库序号
    /// </summary>
    public int? MoveListNo { get; set; }
    
    /// <summary>
    /// 移库批次
    /// </summary>
    [MaxLength(50, ErrorMessage = "移库批次字符长度不能超过50")]
    public string? MoveLotNo { get; set; }
    
    /// <summary>
    /// 物料id
    /// </summary>
    [MaxLength(50, ErrorMessage = "物料id字符长度不能超过50")]
    public string? MaterialId { get; set; }
    
    /// <summary>
    /// 移库物料型号
    /// </summary>
    [MaxLength(100, ErrorMessage = "移库物料型号字符长度不能超过100")]
    public string? MoveMaterialModel { get; set; }
    
    /// <summary>
    /// 物料温度
    /// </summary>
    [MaxLength(50, ErrorMessage = "物料温度字符长度不能超过50")]
    public string? MoveMaterialTemp { get; set; }
    
    /// <summary>
    /// 物料类型（字典MaterialType）
    /// </summary>
    [MaxLength(100, ErrorMessage = "物料类型（字典MaterialType）字符长度不能超过100")]
    public string? MoveMaterialType { get; set; }
    
    /// <summary>
    /// 物料状态
    /// </summary>
    [MaxLength(100, ErrorMessage = "物料状态字符长度不能超过100")]
    public string? MoveMaterialStatus { get; set; }
    
    /// <summary>
    /// 物料单位
    /// </summary>
    [MaxLength(50, ErrorMessage = "物料单位字符长度不能超过50")]
    public string? MoveMaterialUnit { get; set; }
    
    /// <summary>
    /// 物料品牌
    /// </summary>
    [MaxLength(100, ErrorMessage = "物料品牌字符长度不能超过100")]
    public string? MoveMaterialBrand { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? MoveProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    public DateTime? MoveLostDate { get; set; }
    
    /// <summary>
    /// 移库数量
    /// </summary>
    public int? MoveQuantity { get; set; }
    
    /// <summary>
    /// 移库日期
    /// </summary>
    public DateTime? MoveDate { get; set; }
    
    /// <summary>
    /// 仓库id
    /// </summary>
    public long? MoveWarehouseId { get; set; }
    
    /// <summary>
    /// 移出巷道id
    /// </summary>
    public long? MoveLanewayOutCode { get; set; }
    
    /// <summary>
    /// 移入巷道id
    /// </summary>
    [MaxLength(50, ErrorMessage = "移入巷道id字符长度不能超过50")]
    public string? MoveLanewayInCode { get; set; }
    
    /// <summary>
    /// 移出储位编码
    /// </summary>
    [MaxLength(50, ErrorMessage = "移出储位编码字符长度不能超过50")]
    public string? MoveOutSlotCode { get; set; }
    
    /// <summary>
    /// 移入储位编码
    /// </summary>
    [MaxLength(50, ErrorMessage = "移入储位编码字符长度不能超过50")]
    public string? MoveInSlotCode { get; set; }
    
    /// <summary>
    /// 执行标志（01待执行、02正在执行、03已完成、04已上传）
    /// </summary>
    [MaxLength(50, ErrorMessage = "执行标志（01待执行、02正在执行、03已完成、04已上传）字符长度不能超过50")]
    public string? MoveExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(100, ErrorMessage = "备注字符长度不能超过100")]
    public string? MoveRemark { get; set; }
    
    /// <summary>
    /// 移库任务号
    /// </summary>
    [MaxLength(100, ErrorMessage = "移库任务号字符长度不能超过100")]
    public string? MoveTaskNo { get; set; }
    
    /// <summary>
    /// 库存表id
    /// </summary>
    public long? StockStockCodeId { get; set; }
    
}

/// <summary>
/// 移库单据明细表删除输入参数
/// </summary>
public class DeleteWmsMoveNotifyInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 移库单据明细表更新输入参数
/// </summary>
public class UpdateWmsMoveNotifyInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 移库单据类型
    /// </summary>    
    [MaxLength(50, ErrorMessage = "移库单据类型字符长度不能超过50")]
    public string? MoveBillCode { get; set; }
    
    /// <summary>
    /// 移库序号
    /// </summary>    
    public int? MoveListNo { get; set; }
    
    /// <summary>
    /// 移库批次
    /// </summary>    
    [MaxLength(50, ErrorMessage = "移库批次字符长度不能超过50")]
    public string? MoveLotNo { get; set; }
    
    /// <summary>
    /// 物料id
    /// </summary>    
    [MaxLength(50, ErrorMessage = "物料id字符长度不能超过50")]
    public string? MaterialId { get; set; }
    
    /// <summary>
    /// 移库物料型号
    /// </summary>    
    [MaxLength(100, ErrorMessage = "移库物料型号字符长度不能超过100")]
    public string? MoveMaterialModel { get; set; }
    
    /// <summary>
    /// 物料温度
    /// </summary>    
    [MaxLength(50, ErrorMessage = "物料温度字符长度不能超过50")]
    public string? MoveMaterialTemp { get; set; }
    
    /// <summary>
    /// 物料类型（字典MaterialType）
    /// </summary>    
    [MaxLength(100, ErrorMessage = "物料类型（字典MaterialType）字符长度不能超过100")]
    public string? MoveMaterialType { get; set; }
    
    /// <summary>
    /// 物料状态
    /// </summary>    
    [MaxLength(100, ErrorMessage = "物料状态字符长度不能超过100")]
    public string? MoveMaterialStatus { get; set; }
    
    /// <summary>
    /// 物料单位
    /// </summary>    
    [MaxLength(50, ErrorMessage = "物料单位字符长度不能超过50")]
    public string? MoveMaterialUnit { get; set; }
    
    /// <summary>
    /// 物料品牌
    /// </summary>    
    [MaxLength(100, ErrorMessage = "物料品牌字符长度不能超过100")]
    public string? MoveMaterialBrand { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>    
    public DateTime? MoveProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>    
    public DateTime? MoveLostDate { get; set; }
    
    /// <summary>
    /// 移库数量
    /// </summary>    
    public int? MoveQuantity { get; set; }
    
    /// <summary>
    /// 移库日期
    /// </summary>    
    public DateTime? MoveDate { get; set; }
    
    /// <summary>
    /// 仓库id
    /// </summary>    
    public long? MoveWarehouseId { get; set; }
    
    /// <summary>
    /// 移出巷道id
    /// </summary>    
    public long? MoveLanewayOutCode { get; set; }
    
    /// <summary>
    /// 移入巷道id
    /// </summary>    
    [MaxLength(50, ErrorMessage = "移入巷道id字符长度不能超过50")]
    public string? MoveLanewayInCode { get; set; }
    
    /// <summary>
    /// 移出储位编码
    /// </summary>    
    [MaxLength(50, ErrorMessage = "移出储位编码字符长度不能超过50")]
    public string? MoveOutSlotCode { get; set; }
    
    /// <summary>
    /// 移入储位编码
    /// </summary>    
    [MaxLength(50, ErrorMessage = "移入储位编码字符长度不能超过50")]
    public string? MoveInSlotCode { get; set; }
    
    /// <summary>
    /// 执行标志（01待执行、02正在执行、03已完成、04已上传）
    /// </summary>    
    [MaxLength(50, ErrorMessage = "执行标志（01待执行、02正在执行、03已完成、04已上传）字符长度不能超过50")]
    public string? MoveExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>    
    [MaxLength(100, ErrorMessage = "备注字符长度不能超过100")]
    public string? MoveRemark { get; set; }
    
    /// <summary>
    /// 移库任务号
    /// </summary>    
    [MaxLength(100, ErrorMessage = "移库任务号字符长度不能超过100")]
    public string? MoveTaskNo { get; set; }
    
    /// <summary>
    /// 库存表id
    /// </summary>    
    public long? StockStockCodeId { get; set; }
    
}

/// <summary>
/// 移库单据明细表主键查询输入参数
/// </summary>
public class QueryByIdWmsMoveNotifyInput : DeleteWmsMoveNotifyInput
{
}

/// <summary>
/// 移库单据明细表数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsMoveNotifyInput : BaseImportInput
{
    /// <summary>
    /// 移库单据类型
    /// </summary>
    [ImporterHeader(Name = "移库单据类型")]
    [ExporterHeader("移库单据类型", Format = "", Width = 25, IsBold = true)]
    public string? MoveBillCode { get; set; }
    
    /// <summary>
    /// 移库序号
    /// </summary>
    [ImporterHeader(Name = "移库序号")]
    [ExporterHeader("移库序号", Format = "", Width = 25, IsBold = true)]
    public int? MoveListNo { get; set; }
    
    /// <summary>
    /// 移库批次
    /// </summary>
    [ImporterHeader(Name = "移库批次")]
    [ExporterHeader("移库批次", Format = "", Width = 25, IsBold = true)]
    public string? MoveLotNo { get; set; }
    
    /// <summary>
    /// 物料id
    /// </summary>
    [ImporterHeader(Name = "物料id")]
    [ExporterHeader("物料id", Format = "", Width = 25, IsBold = true)]
    public string? MaterialId { get; set; }
    
    /// <summary>
    /// 移库物料型号
    /// </summary>
    [ImporterHeader(Name = "移库物料型号")]
    [ExporterHeader("移库物料型号", Format = "", Width = 25, IsBold = true)]
    public string? MoveMaterialModel { get; set; }
    
    /// <summary>
    /// 物料温度
    /// </summary>
    [ImporterHeader(Name = "物料温度")]
    [ExporterHeader("物料温度", Format = "", Width = 25, IsBold = true)]
    public string? MoveMaterialTemp { get; set; }
    
    /// <summary>
    /// 物料类型（字典MaterialType）
    /// </summary>
    [ImporterHeader(Name = "物料类型（字典MaterialType）")]
    [ExporterHeader("物料类型（字典MaterialType）", Format = "", Width = 25, IsBold = true)]
    public string? MoveMaterialType { get; set; }
    
    /// <summary>
    /// 物料状态
    /// </summary>
    [ImporterHeader(Name = "物料状态")]
    [ExporterHeader("物料状态", Format = "", Width = 25, IsBold = true)]
    public string? MoveMaterialStatus { get; set; }
    
    /// <summary>
    /// 物料单位
    /// </summary>
    [ImporterHeader(Name = "物料单位")]
    [ExporterHeader("物料单位", Format = "", Width = 25, IsBold = true)]
    public string? MoveMaterialUnit { get; set; }
    
    /// <summary>
    /// 物料品牌
    /// </summary>
    [ImporterHeader(Name = "物料品牌")]
    [ExporterHeader("物料品牌", Format = "", Width = 25, IsBold = true)]
    public string? MoveMaterialBrand { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [ImporterHeader(Name = "生产日期")]
    [ExporterHeader("生产日期", Format = "", Width = 25, IsBold = true)]
    public DateTime? MoveProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    [ImporterHeader(Name = "失效日期")]
    [ExporterHeader("失效日期", Format = "", Width = 25, IsBold = true)]
    public DateTime? MoveLostDate { get; set; }
    
    /// <summary>
    /// 移库数量
    /// </summary>
    [ImporterHeader(Name = "移库数量")]
    [ExporterHeader("移库数量", Format = "", Width = 25, IsBold = true)]
    public int? MoveQuantity { get; set; }
    
    /// <summary>
    /// 移库日期
    /// </summary>
    [ImporterHeader(Name = "移库日期")]
    [ExporterHeader("移库日期", Format = "", Width = 25, IsBold = true)]
    public DateTime? MoveDate { get; set; }
    
    /// <summary>
    /// 仓库id
    /// </summary>
    [ImporterHeader(Name = "仓库id")]
    [ExporterHeader("仓库id", Format = "", Width = 25, IsBold = true)]
    public long? MoveWarehouseId { get; set; }
    
    /// <summary>
    /// 移出巷道id
    /// </summary>
    [ImporterHeader(Name = "移出巷道id")]
    [ExporterHeader("移出巷道id", Format = "", Width = 25, IsBold = true)]
    public long? MoveLanewayOutCode { get; set; }
    
    /// <summary>
    /// 移入巷道id
    /// </summary>
    [ImporterHeader(Name = "移入巷道id")]
    [ExporterHeader("移入巷道id", Format = "", Width = 25, IsBold = true)]
    public string? MoveLanewayInCode { get; set; }
    
    /// <summary>
    /// 移出储位编码
    /// </summary>
    [ImporterHeader(Name = "移出储位编码")]
    [ExporterHeader("移出储位编码", Format = "", Width = 25, IsBold = true)]
    public string? MoveOutSlotCode { get; set; }
    
    /// <summary>
    /// 移入储位编码
    /// </summary>
    [ImporterHeader(Name = "移入储位编码")]
    [ExporterHeader("移入储位编码", Format = "", Width = 25, IsBold = true)]
    public string? MoveInSlotCode { get; set; }
    
    /// <summary>
    /// 执行标志（01待执行、02正在执行、03已完成、04已上传）
    /// </summary>
    [ImporterHeader(Name = "执行标志（01待执行、02正在执行、03已完成、04已上传）")]
    [ExporterHeader("执行标志（01待执行、02正在执行、03已完成、04已上传）", Format = "", Width = 25, IsBold = true)]
    public string? MoveExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? MoveRemark { get; set; }
    
    /// <summary>
    /// 移库任务号
    /// </summary>
    [ImporterHeader(Name = "移库任务号")]
    [ExporterHeader("移库任务号", Format = "", Width = 25, IsBold = true)]
    public string? MoveTaskNo { get; set; }
    
    /// <summary>
    /// 库存表id
    /// </summary>
    [ImporterHeader(Name = "库存表id")]
    [ExporterHeader("库存表id", Format = "", Width = 25, IsBold = true)]
    public long? StockStockCodeId { get; set; }
    
}
