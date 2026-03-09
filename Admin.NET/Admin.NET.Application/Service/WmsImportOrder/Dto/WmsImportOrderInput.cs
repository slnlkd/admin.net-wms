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
/// 入库流水基础输入参数
/// </summary>
public class WmsImportOrderBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 流水号
    /// </summary>
    public virtual string? ImportOrderNo { get; set; }
    
    /// <summary>
    /// 入库单ID
    /// </summary>
    public virtual long? ImportId { get; set; }
    
    /// <summary>
    /// 区域ID
    /// </summary>
    public virtual long? ImportAreaId { get; set; }
    
    /// <summary>
    /// 巷道ID
    /// </summary>
    public virtual long? ImportLanewayId { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    public virtual string? ImportSlotCode { get; set; }
    
    /// <summary>
    /// 托盘条码ID
    /// </summary>
    public virtual long? StockCodeId { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    public virtual decimal? ImportQuantity { get; set; }
    
    /// <summary>
    /// 重量
    /// </summary>
    public virtual decimal? ImportWeight { get; set; }
    
    /// <summary>
    /// 任务ID
    /// </summary>
    public virtual long? ImportTaskId { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在执行、2已完成、3已上传）
    /// </summary>
    public virtual string? ImportExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 班次
    /// </summary>
    public virtual string? ImportClass { get; set; }
    
    /// <summary>
    /// 批号
    /// </summary>
    public virtual string? LotNo { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public virtual DateTime? ImportProductionDate { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    public virtual DateTime? CompleteDate { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>
    public virtual int? InspectionStatus { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public virtual long? WareHouseId { get; set; }
    
    /// <summary>
    /// 入库单明细ID
    /// </summary>
    public virtual long? ImportDetailId { get; set; }
    
    /// <summary>
    /// 主子载具号
    /// </summary>
    public virtual string? SubVehicleCode { get; set; }
    
    /// <summary>
    /// 实际称重重量
    /// </summary>
    public virtual decimal? Weight { get; set; }
    
    /// <summary>
    /// 0未验收 1正在验收 2验收完成
    /// </summary>
    public virtual int? InspectFlag { get; set; }
    
    /// <summary>
    /// 是否暂存入库流水（不为空为是）
    /// </summary>
    public virtual string? IsTemporaryStorage { get; set; }
    
    /// <summary>
    /// 载具号
    /// </summary>
    public virtual string? StockCode { get; set; }
    
    /// <summary>
    /// 验收0 挑浆1
    /// </summary>
    public virtual string? YsOrTJ { get; set; }
    
}

/// <summary>
/// 入库流水分页查询输入参数
/// </summary>
public class PageWmsImportOrderInput : BasePageInput
{
    /// <summary>
    /// 流水号
    /// </summary>
    public string? ImportOrderNo { get; set; }
    
    /// <summary>
    /// 入库单ID
    /// </summary>
    public long? ImportId { get; set; }
    
    /// <summary>
    /// 区域ID
    /// </summary>
    public long? ImportAreaId { get; set; }
    
    /// <summary>
    /// 巷道ID
    /// </summary>
    public long? ImportLanewayId { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    public string? ImportSlotCode { get; set; }
    
    /// <summary>
    /// 托盘条码ID
    /// </summary>
    public long? StockCodeId { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    public decimal? ImportQuantity { get; set; }
    
    /// <summary>
    /// 重量
    /// </summary>
    public decimal? ImportWeight { get; set; }
    
    /// <summary>
    /// 任务ID
    /// </summary>
    public long? TaskId { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在执行、2已完成、3已上传）
    /// </summary>
    public string? ImportExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
    
    /// <summary>
    /// 班次
    /// </summary>
    public string? ImportClass { get; set; }
    
    /// <summary>
    /// 批号
    /// </summary>
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 生产日期范围
    /// </summary>
     public DateTime?[] ImportProductionDateRange { get; set; }
    
    /// <summary>
    /// 完成时间范围
    /// </summary>
     public DateTime?[] CompleteDateRange { get; set; }
    
    /// <summary>
    /// 创建时间范围
    /// </summary>
     public DateTime?[] CreateTimeRange { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? WareHouseId { get; set; }
    
    /// <summary>
    /// 入库单明细ID
    /// </summary>
    public long? ImportDetailId { get; set; }
    
    /// <summary>
    /// 主子载具号
    /// </summary>
    public string? SubVehicleCode { get; set; }
    
    /// <summary>
    /// 实际称重重量
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// 0未验收 1正在验收 2验收完成
    /// </summary>
    public int? InspectFlag { get; set; }
    
    /// <summary>
    /// 是否暂存入库流水（不为空为是）
    /// </summary>
    public string? IsTemporaryStorage { get; set; }
    
    /// <summary>
    /// 载具号
    /// </summary>
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 验收0 挑浆1
    /// </summary>
    public string? YsOrTJ { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 入库流水增加输入参数
/// </summary>
public class AddWmsImportOrderInput
{
    /// <summary>
    /// 流水号
    /// </summary>
    [MaxLength(32, ErrorMessage = "流水号字符长度不能超过32")]
    public string? ImportOrderNo { get; set; }
    
    /// <summary>
    /// 入库单ID
    /// </summary>
    public long? ImportId { get; set; }
    
    /// <summary>
    /// 区域ID
    /// </summary>
    public long? ImportAreaId { get; set; }
    
    /// <summary>
    /// 巷道ID
    /// </summary>
    public long? ImportLanewayId { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    [MaxLength(20, ErrorMessage = "储位编码字符长度不能超过20")]
    public string? ImportSlotCode { get; set; }
    
    /// <summary>
    /// 托盘条码ID
    /// </summary>
    public long? StockCodeId { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    public decimal? ImportQuantity { get; set; }
    
    /// <summary>
    /// 重量
    /// </summary>
    public decimal? ImportWeight { get; set; }
    
    /// <summary>
    /// 任务ID
    /// </summary>
    public long? TaskId { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在执行、2已完成、3已上传）
    /// </summary>
    [MaxLength(2, ErrorMessage = "执行标志（0待执行、1正在执行、2已完成、3已上传）字符长度不能超过2")]
    public string? ImportExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(50, ErrorMessage = "备注字符长度不能超过50")]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 班次
    /// </summary>
    [MaxLength(32, ErrorMessage = "班次字符长度不能超过32")]
    public string? ImportClass { get; set; }
    
    /// <summary>
    /// 批号
    /// </summary>
    [MaxLength(32, ErrorMessage = "批号字符长度不能超过32")]
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ImportProductionDate { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompleteDate { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? WareHouseId { get; set; }
    
    /// <summary>
    /// 入库单明细ID
    /// </summary>
    public long? ImportDetailId { get; set; }
    
    /// <summary>
    /// 主子载具号
    /// </summary>
    [MaxLength(32, ErrorMessage = "主子载具号字符长度不能超过32")]
    public string? SubVehicleCode { get; set; }
    
    /// <summary>
    /// 实际称重重量
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// 0未验收 1正在验收 2验收完成
    /// </summary>
    public int? InspectFlag { get; set; }
    
    /// <summary>
    /// 是否暂存入库流水（不为空为是）
    /// </summary>
    [MaxLength(255, ErrorMessage = "是否暂存入库流水（不为空为是）字符长度不能超过255")]
    public string? IsTemporaryStorage { get; set; }
    
    /// <summary>
    /// 载具号
    /// </summary>
    [MaxLength(50, ErrorMessage = "载具号字符长度不能超过50")]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 验收0 挑浆1
    /// </summary>
    [MaxLength(10, ErrorMessage = "验收0 挑浆1字符长度不能超过10")]
    public string? YsOrTJ { get; set; }
    
}

/// <summary>
/// 入库流水删除输入参数
/// </summary>
public class DeleteWmsImportOrderInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}
public class SaveOrderSlotInput
{
    /// <summary>
    /// 入库流水ID
    /// </summary>
    public long? OrderId { get; set; }
    /// <summary>
    /// 货位ID
    /// </summary>
    public long? SlotId { get; set; }
}
/// <summary>
/// 入库流水更新输入参数
/// </summary>
public class UpdateWmsImportOrderInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 流水号
    /// </summary>    
    [MaxLength(32, ErrorMessage = "流水号字符长度不能超过32")]
    public string? ImportOrderNo { get; set; }
    
    /// <summary>
    /// 入库单ID
    /// </summary>    
    public long? ImportId { get; set; }
    
    /// <summary>
    /// 区域ID
    /// </summary>    
    public long? ImportAreaId { get; set; }
    
    /// <summary>
    /// 巷道ID
    /// </summary>    
    public long? ImportLanewayId { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>    
    [MaxLength(20, ErrorMessage = "储位编码字符长度不能超过20")]
    public string? ImportSlotCode { get; set; }
    
    /// <summary>
    /// 托盘条码ID
    /// </summary>    
    public long? StockCodeId { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>    
    public decimal? ImportQuantity { get; set; }
    
    /// <summary>
    /// 重量
    /// </summary>    
    public decimal? ImportWeight { get; set; }
    
    /// <summary>
    /// 任务ID
    /// </summary>    
    public long? TaskId { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在执行、2已完成、3已上传）
    /// </summary>    
    [MaxLength(2, ErrorMessage = "执行标志（0待执行、1正在执行、2已完成、3已上传）字符长度不能超过2")]
    public string? ImportExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>    
    [MaxLength(50, ErrorMessage = "备注字符长度不能超过50")]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 班次
    /// </summary>    
    [MaxLength(32, ErrorMessage = "班次字符长度不能超过32")]
    public string? ImportClass { get; set; }
    
    /// <summary>
    /// 批号
    /// </summary>    
    [MaxLength(32, ErrorMessage = "批号字符长度不能超过32")]
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>    
    public DateTime? ImportProductionDate { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>    
    public DateTime? CompleteDate { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>    
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>    
    public long? WareHouseId { get; set; }
    
    /// <summary>
    /// 入库单明细ID
    /// </summary>    
    public long? ImportDetailId { get; set; }
    
    /// <summary>
    /// 主子载具号
    /// </summary>    
    [MaxLength(32, ErrorMessage = "主子载具号字符长度不能超过32")]
    public string? SubVehicleCode { get; set; }
    
    /// <summary>
    /// 实际称重重量
    /// </summary>    
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// 0未验收 1正在验收 2验收完成
    /// </summary>    
    public int? InspectFlag { get; set; }
    
    /// <summary>
    /// 是否暂存入库流水（不为空为是）
    /// </summary>    
    [MaxLength(255, ErrorMessage = "是否暂存入库流水（不为空为是）字符长度不能超过255")]
    public string? IsTemporaryStorage { get; set; }
    
    /// <summary>
    /// 载具号
    /// </summary>    
    [MaxLength(50, ErrorMessage = "载具号字符长度不能超过50")]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 验收0 挑浆1
    /// </summary>    
    [MaxLength(10, ErrorMessage = "验收0 挑浆1字符长度不能超过10")]
    public string? YsOrTJ { get; set; }
    
}

/// <summary>
/// 入库流水主键查询输入参数
/// </summary>
public class QueryByIdWmsImportOrderInput : DeleteWmsImportOrderInput
{
}

/// <summary>
/// 入库流水数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsImportOrderInput : BaseImportInput
{
    /// <summary>
    /// 流水号
    /// </summary>
    [ImporterHeader(Name = "流水号")]
    [ExporterHeader("流水号", Format = "", Width = 25, IsBold = true)]
    public string? ImportOrderNo { get; set; }
    
    /// <summary>
    /// 入库单ID
    /// </summary>
    [ImporterHeader(Name = "入库单ID")]
    [ExporterHeader("入库单ID", Format = "", Width = 25, IsBold = true)]
    public long? ImportId { get; set; }
    
    /// <summary>
    /// 区域ID
    /// </summary>
    [ImporterHeader(Name = "区域ID")]
    [ExporterHeader("区域ID", Format = "", Width = 25, IsBold = true)]
    public long? ImportAreaId { get; set; }
    
    /// <summary>
    /// 巷道ID
    /// </summary>
    [ImporterHeader(Name = "巷道ID")]
    [ExporterHeader("巷道ID", Format = "", Width = 25, IsBold = true)]
    public long? ImportLanewayId { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    [ImporterHeader(Name = "储位编码")]
    [ExporterHeader("储位编码", Format = "", Width = 25, IsBold = true)]
    public string? ImportSlotCode { get; set; }
    
    /// <summary>
    /// 托盘条码ID
    /// </summary>
    [ImporterHeader(Name = "托盘条码ID")]
    [ExporterHeader("托盘条码ID", Format = "", Width = 25, IsBold = true)]
    public long? StockCodeId { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    [ImporterHeader(Name = "数量")]
    [ExporterHeader("数量", Format = "", Width = 25, IsBold = true)]
    public decimal? ImportQuantity { get; set; }
    
    /// <summary>
    /// 重量
    /// </summary>
    [ImporterHeader(Name = "重量")]
    [ExporterHeader("重量", Format = "", Width = 25, IsBold = true)]
    public decimal? ImportWeight { get; set; }
    
    /// <summary>
    /// 任务ID
    /// </summary>
    [ImporterHeader(Name = "任务ID")]
    [ExporterHeader("任务ID", Format = "", Width = 25, IsBold = true)]
    public long? TaskId { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在执行、2已完成、3已上传）
    /// </summary>
    [ImporterHeader(Name = "执行标志（0待执行、1正在执行、2已完成、3已上传）")]
    [ExporterHeader("执行标志（0待执行、1正在执行、2已完成、3已上传）", Format = "", Width = 25, IsBold = true)]
    public string? ImportExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 班次
    /// </summary>
    [ImporterHeader(Name = "班次")]
    [ExporterHeader("班次", Format = "", Width = 25, IsBold = true)]
    public string? ImportClass { get; set; }
    
    /// <summary>
    /// 批号
    /// </summary>
    [ImporterHeader(Name = "批号")]
    [ExporterHeader("批号", Format = "", Width = 25, IsBold = true)]
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [ImporterHeader(Name = "生产日期")]
    [ExporterHeader("生产日期", Format = "", Width = 25, IsBold = true)]
    public DateTime? ImportProductionDate { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    [ImporterHeader(Name = "完成时间")]
    [ExporterHeader("完成时间", Format = "", Width = 25, IsBold = true)]
    public DateTime? CompleteDate { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>
    [ImporterHeader(Name = "质检状态")]
    [ExporterHeader("质检状态", Format = "", Width = 25, IsBold = true)]
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    [ImporterHeader(Name = "仓库ID")]
    [ExporterHeader("仓库ID", Format = "", Width = 25, IsBold = true)]
    public long? WareHouseId { get; set; }
    
    /// <summary>
    /// 入库单明细ID
    /// </summary>
    [ImporterHeader(Name = "入库单明细ID")]
    [ExporterHeader("入库单明细ID", Format = "", Width = 25, IsBold = true)]
    public long? ImportDetailId { get; set; }
    
    /// <summary>
    /// 主子载具号
    /// </summary>
    [ImporterHeader(Name = "主子载具号")]
    [ExporterHeader("主子载具号", Format = "", Width = 25, IsBold = true)]
    public string? SubVehicleCode { get; set; }
    
    /// <summary>
    /// 实际称重重量
    /// </summary>
    [ImporterHeader(Name = "实际称重重量")]
    [ExporterHeader("实际称重重量", Format = "", Width = 25, IsBold = true)]
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// 0未验收 1正在验收 2验收完成
    /// </summary>
    [ImporterHeader(Name = "0未验收 1正在验收 2验收完成")]
    [ExporterHeader("0未验收 1正在验收 2验收完成", Format = "", Width = 25, IsBold = true)]
    public int? InspectFlag { get; set; }
    
    /// <summary>
    /// 是否暂存入库流水（不为空为是）
    /// </summary>
    [ImporterHeader(Name = "是否暂存入库流水（不为空为是）")]
    [ExporterHeader("是否暂存入库流水（不为空为是）", Format = "", Width = 25, IsBold = true)]
    public string? IsTemporaryStorage { get; set; }
    
    /// <summary>
    /// 载具号
    /// </summary>
    [ImporterHeader(Name = "载具号")]
    [ExporterHeader("载具号", Format = "", Width = 25, IsBold = true)]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 验收0 挑浆1
    /// </summary>
    [ImporterHeader(Name = "验收0 挑浆1")]
    [ExporterHeader("验收0 挑浆1", Format = "", Width = 25, IsBold = true)]
    public string? YsOrTJ { get; set; }
    
}
