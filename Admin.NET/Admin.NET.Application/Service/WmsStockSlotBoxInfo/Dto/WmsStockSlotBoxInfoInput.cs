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
/// 库存箱码明细基础输入参数
/// </summary>
public class WmsStockSlotBoxInfoBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 箱码
    /// </summary>
    public virtual string? BoxCode { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    public virtual decimal? Qty { get; set; }
    
    /// <summary>
    /// 整箱数
    /// </summary>
    public virtual decimal? FullBoxQty { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    public virtual string? StockCode { get; set; }
    
    /// <summary>
    /// 托盘条码id
    /// </summary>
    public virtual string? StockCodeId { get; set; }
    
    /// <summary>
    /// 状态（0未执行；1正在入库；2已入库）
    /// </summary>
    public virtual int? Status { get; set; }
    
    /// <summary>
    /// 入库单id
    /// </summary>
    public virtual string? ImportId { get; set; }
    
    /// <summary>
    /// 入库单详情id
    /// </summary>
    public virtual string? ImportDetailId { get; set; }
    
    /// <summary>
    /// 入库流水id
    /// </summary>
    public virtual string? ImportOrderId { get; set; }
    
    /// <summary>
    /// 等级
    /// </summary>
    public virtual int? BoxLevel { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public virtual DateTime? ProductionDate { get; set; }
    
    /// <summary>
    /// 保质期
    /// </summary>
    public virtual DateTime? ValidateDay { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    public virtual string? LotNo { get; set; }
    
    /// <summary>
    /// 物料id
    /// </summary>
    public virtual string? MaterialId { get; set; }
    
    /// <summary>
    /// 供应商ID
    /// </summary>
    public virtual long? SupplierId { get; set; }

    /// <summary>
    /// 制造商ID
    /// </summary>
    public virtual long? ManufacturerId { get; set; }
    
    /// <summary>
    /// 是否零箱（0否；1是）
    /// </summary>
    public virtual int? BulkTank { get; set; }
    
    /// <summary>
    /// 是否抽检箱（0否；1是）
    /// </summary>
    public virtual int? IsSamplingBox { get; set; }
    
    /// <summary>
    /// 质检状态（0待检；1合格；2不合格）
    /// </summary>
    public virtual int? InspectionStatus { get; set; }
    
    /// <summary>
    /// erp箱二维码
    /// </summary>
    public virtual string? QRCode { get; set; }
    
    /// <summary>
    /// 采样日期
    /// </summary>
    public virtual DateTime? SamplingDate { get; set; }
    
    /// <summary>
    /// 浆员编码
    /// </summary>
    public virtual string? StaffCode { get; set; }
    
    /// <summary>
    /// 浆员姓名
    /// </summary>
    public virtual string? StaffName { get; set; }
    
    /// <summary>
    /// 血浆重量
    /// </summary>
    public virtual decimal? Weight { get; set; }
    
    /// <summary>
    /// 剔除原因（手持剔除异常血浆存放）
    /// </summary>
    public virtual string? ReasonsForExcl { get; set; }
    
    /// <summary>
    /// 检疫期状态（1检疫期满足；2检疫期不满足；3检疫期不合格）
    /// </summary>
    public virtual int? ExtractStatus { get; set; }
    
    /// <summary>
    /// 是否挑浆（0默认；1挑浆）
    /// </summary>
    public virtual string? PickingSlurry { get; set; }
    
    /// <summary>
    /// 血浆拒收类型id
    /// </summary>
    public virtual string? plasmaRejectTypeId { get; set; }
    
   
    

    
}

/// <summary>
/// 库存箱码明细分页查询输入参数
/// </summary>
public class PageWmsStockSlotBoxInfoInput : BasePageInput
{
    /// <summary>
    /// 箱码
    /// </summary>
    public string? BoxCode { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    public decimal? Qty { get; set; }
    
    /// <summary>
    /// 整箱数
    /// </summary>
    public decimal? FullBoxQty { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 托盘条码id
    /// </summary>
    public string? StockCodeId { get; set; }
    
    /// <summary>
    /// 状态（0未执行；1正在入库；2已入库）
    /// </summary>
    public int? Status { get; set; }
    
    /// <summary>
    /// 入库单id
    /// </summary>
    public string? ImportId { get; set; }
    
    /// <summary>
    /// 入库单详情id
    /// </summary>
    public string? ImportDetailId { get; set; }
    
    /// <summary>
    /// 入库流水id
    /// </summary>
    public string? ImportOrderId { get; set; }
    
    /// <summary>
    /// 等级
    /// </summary>
    public int? BoxLevel { get; set; }
    
    /// <summary>
    /// 生产日期范围
    /// </summary>
     public DateTime?[] ProductionDateRange { get; set; }
    
    /// <summary>
    /// 保质期范围
    /// </summary>
     public DateTime?[] ValidateDayRange { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 物料id
    /// </summary>
    public string? MaterialId { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterialName { get; set; }
    
    /// <summary>
    /// 供应商ID
    /// </summary>
    public long? SupplierId { get; set; }

    /// <summary>
    /// 制造商ID
    /// </summary>
    public long? ManufacturerId { get; set; }
    
    /// <summary>
    /// 是否零箱（0否；1是）
    /// </summary>
    public int? BulkTank { get; set; }
    
    /// <summary>
    /// 是否抽检箱（0否；1是）
    /// </summary>
    public int? IsSamplingBox { get; set; }
    
    /// <summary>
    /// 质检状态（0待检；1合格；2不合格）
    /// </summary>
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// erp箱二维码
    /// </summary>
    public string? QRCode { get; set; }
    
    /// <summary>
    /// 采样日期范围
    /// </summary>
     public DateTime?[] SamplingDateRange { get; set; }
    
    /// <summary>
    /// 浆员编码
    /// </summary>
    public string? StaffCode { get; set; }
    
    /// <summary>
    /// 浆员姓名
    /// </summary>
    public string? StaffName { get; set; }
    
    /// <summary>
    /// 血浆重量
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// 剔除原因（手持剔除异常血浆存放）
    /// </summary>
    public string? ReasonsForExcl { get; set; }
    
    /// <summary>
    /// 检疫期状态（1检疫期满足；2检疫期不满足；3检疫期不合格）
    /// </summary>
    public int? ExtractStatus { get; set; }
    
    /// <summary>
    /// 是否挑浆（0默认；1挑浆）
    /// </summary>
    public string? PickingSlurry { get; set; }
    
    /// <summary>
    /// 血浆拒收类型id
    /// </summary>
    public string? plasmaRejectTypeId { get; set; }
    
   
  
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 库存箱码明细增加输入参数
/// </summary>
public class AddWmsStockSlotBoxInfoInput
{
    /// <summary>
    /// 箱码
    /// </summary>
    [MaxLength(32, ErrorMessage = "箱码字符长度不能超过32")]
    public string? BoxCode { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    public decimal? Qty { get; set; }
    
    /// <summary>
    /// 整箱数
    /// </summary>
    public decimal? FullBoxQty { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    [MaxLength(50, ErrorMessage = "托盘条码字符长度不能超过50")]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 托盘条码id
    /// </summary>
    [MaxLength(32, ErrorMessage = "托盘条码id字符长度不能超过32")]
    public string? StockCodeId { get; set; }
    
    /// <summary>
    /// 状态（0未执行；1正在入库；2已入库）
    /// </summary>
    public int? Status { get; set; }
    
    /// <summary>
    /// 入库单id
    /// </summary>
    [MaxLength(32, ErrorMessage = "入库单id字符长度不能超过32")]
    public string? ImportId { get; set; }
    
    /// <summary>
    /// 入库单详情id
    /// </summary>
    [MaxLength(32, ErrorMessage = "入库单详情id字符长度不能超过32")]
    public string? ImportDetailId { get; set; }
    
    /// <summary>
    /// 入库流水id
    /// </summary>
    [MaxLength(32, ErrorMessage = "入库流水id字符长度不能超过32")]
    public string? ImportOrderId { get; set; }
    
    /// <summary>
    /// 等级
    /// </summary>
    public int? BoxLevel { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ProductionDate { get; set; }
    
    /// <summary>
    /// 保质期
    /// </summary>
    public DateTime? ValidateDay { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    [MaxLength(32, ErrorMessage = "批次字符长度不能超过32")]
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 物料id
    /// </summary>
    [MaxLength(32, ErrorMessage = "物料id字符长度不能超过32")]
    public string? MaterialId { get; set; }
    
    /// <summary>
    /// 供应商ID
    /// </summary>
    public long? SupplierId { get; set; }

    /// <summary>
    /// 制造商ID
    /// </summary>
    public long? ManufacturerId { get; set; }
    
    /// <summary>
    /// 是否零箱（0否；1是）
    /// </summary>
    public int? BulkTank { get; set; }
    
    /// <summary>
    /// 是否抽检箱（0否；1是）
    /// </summary>
    public int? IsSamplingBox { get; set; }
    
    /// <summary>
    /// 质检状态（0待检；1合格；2不合格）
    /// </summary>
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// erp箱二维码
    /// </summary>
    [MaxLength(32, ErrorMessage = "erp箱二维码字符长度不能超过32")]
    public string? QRCode { get; set; }
    
    /// <summary>
    /// 采样日期
    /// </summary>
    public DateTime? SamplingDate { get; set; }
    
    /// <summary>
    /// 浆员编码
    /// </summary>
    [MaxLength(32, ErrorMessage = "浆员编码字符长度不能超过32")]
    public string? StaffCode { get; set; }
    
    /// <summary>
    /// 浆员姓名
    /// </summary>
    [MaxLength(32, ErrorMessage = "浆员姓名字符长度不能超过32")]
    public string? StaffName { get; set; }
    
    /// <summary>
    /// 血浆重量
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// 剔除原因（手持剔除异常血浆存放）
    /// </summary>
    [MaxLength(100, ErrorMessage = "剔除原因（手持剔除异常血浆存放）字符长度不能超过100")]
    public string? ReasonsForExcl { get; set; }
    
    /// <summary>
    /// 检疫期状态（1检疫期满足；2检疫期不满足；3检疫期不合格）
    /// </summary>
    public int? ExtractStatus { get; set; }
    
    /// <summary>
    /// 是否挑浆（0默认；1挑浆）
    /// </summary>
    [MaxLength(1, ErrorMessage = "是否挑浆（0默认；1挑浆）字符长度不能超过1")]
    public string? PickingSlurry { get; set; }
    
    /// <summary>
    /// 血浆拒收类型id
    /// </summary>
    [MaxLength(50, ErrorMessage = "血浆拒收类型id字符长度不能超过50")]
    public string? plasmaRejectTypeId { get; set; }
    
    
  
    
}

/// <summary>
/// 库存箱码明细删除输入参数
/// </summary>
public class DeleteWmsStockSlotBoxInfoInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 库存箱码明细更新输入参数
/// </summary>
public class UpdateWmsStockSlotBoxInfoInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 箱码
    /// </summary>    
    [MaxLength(32, ErrorMessage = "箱码字符长度不能超过32")]
    public string? BoxCode { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>    
    public decimal? Qty { get; set; }
    
    /// <summary>
    /// 整箱数
    /// </summary>    
    public decimal? FullBoxQty { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>    
    [MaxLength(50, ErrorMessage = "托盘条码字符长度不能超过50")]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 托盘条码id
    /// </summary>    
    [MaxLength(32, ErrorMessage = "托盘条码id字符长度不能超过32")]
    public string? StockCodeId { get; set; }
    
    /// <summary>
    /// 状态（0未执行；1正在入库；2已入库）
    /// </summary>    
    public int? Status { get; set; }
    
    /// <summary>
    /// 入库单id
    /// </summary>    
    [MaxLength(32, ErrorMessage = "入库单id字符长度不能超过32")]
    public string? ImportId { get; set; }
    
    /// <summary>
    /// 入库单详情id
    /// </summary>    
    [MaxLength(32, ErrorMessage = "入库单详情id字符长度不能超过32")]
    public string? ImportDetailId { get; set; }
    
    /// <summary>
    /// 入库流水id
    /// </summary>    
    [MaxLength(32, ErrorMessage = "入库流水id字符长度不能超过32")]
    public string? ImportOrderId { get; set; }
    
    /// <summary>
    /// 等级
    /// </summary>    
    public int? BoxLevel { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>    
    public DateTime? ProductionDate { get; set; }
    
    /// <summary>
    /// 保质期
    /// </summary>    
    public DateTime? ValidateDay { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>    
    [MaxLength(32, ErrorMessage = "批次字符长度不能超过32")]
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 物料id
    /// </summary>    
    [MaxLength(32, ErrorMessage = "物料id字符长度不能超过32")]
    public string? MaterialId { get; set; }
    
    /// <summary>
    /// 供应商ID
    /// </summary>
    public long? SupplierId { get; set; }

    /// <summary>
    /// 制造商ID
    /// </summary>
    public long? ManufacturerId { get; set; }
    
    /// <summary>
    /// 是否零箱（0否；1是）
    /// </summary>    
    public int? BulkTank { get; set; }
    
    /// <summary>
    /// 是否抽检箱（0否；1是）
    /// </summary>    
    public int? IsSamplingBox { get; set; }
    
    /// <summary>
    /// 质检状态（0待检；1合格；2不合格）
    /// </summary>    
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// erp箱二维码
    /// </summary>    
    [MaxLength(32, ErrorMessage = "erp箱二维码字符长度不能超过32")]
    public string? QRCode { get; set; }
    
    /// <summary>
    /// 采样日期
    /// </summary>    
    public DateTime? SamplingDate { get; set; }
    
    /// <summary>
    /// 浆员编码
    /// </summary>    
    [MaxLength(32, ErrorMessage = "浆员编码字符长度不能超过32")]
    public string? StaffCode { get; set; }
    
    /// <summary>
    /// 浆员姓名
    /// </summary>    
    [MaxLength(32, ErrorMessage = "浆员姓名字符长度不能超过32")]
    public string? StaffName { get; set; }
    
    /// <summary>
    /// 血浆重量
    /// </summary>    
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// 剔除原因（手持剔除异常血浆存放）
    /// </summary>    
    [MaxLength(100, ErrorMessage = "剔除原因（手持剔除异常血浆存放）字符长度不能超过100")]
    public string? ReasonsForExcl { get; set; }
    
    /// <summary>
    /// 检疫期状态（1检疫期满足；2检疫期不满足；3检疫期不合格）
    /// </summary>    
    public int? ExtractStatus { get; set; }
    
    /// <summary>
    /// 是否挑浆（0默认；1挑浆）
    /// </summary>    
    [MaxLength(1, ErrorMessage = "是否挑浆（0默认；1挑浆）字符长度不能超过1")]
    public string? PickingSlurry { get; set; }
    
    /// <summary>
    /// 血浆拒收类型id
    /// </summary>    
    [MaxLength(50, ErrorMessage = "血浆拒收类型id字符长度不能超过50")]
    public string? plasmaRejectTypeId { get; set; }
    
   

    
}

/// <summary>
/// 库存箱码明细主键查询输入参数
/// </summary>
public class QueryByIdWmsStockSlotBoxInfoInput : DeleteWmsStockSlotBoxInfoInput
{
}

/// <summary>
/// 库存箱码明细数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsStockSlotBoxInfoInput : BaseImportInput
{
    /// <summary>
    /// 箱码
    /// </summary>
    [ImporterHeader(Name = "箱码")]
    [ExporterHeader("箱码", Format = "", Width = 25, IsBold = true)]
    public string? BoxCode { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    [ImporterHeader(Name = "数量")]
    [ExporterHeader("数量", Format = "", Width = 25, IsBold = true)]
    public decimal? Qty { get; set; }
    
    /// <summary>
    /// 整箱数
    /// </summary>
    [ImporterHeader(Name = "整箱数")]
    [ExporterHeader("整箱数", Format = "", Width = 25, IsBold = true)]
    public decimal? FullBoxQty { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    [ImporterHeader(Name = "托盘条码")]
    [ExporterHeader("托盘条码", Format = "", Width = 25, IsBold = true)]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 托盘条码id
    /// </summary>
    [ImporterHeader(Name = "托盘条码id")]
    [ExporterHeader("托盘条码id", Format = "", Width = 25, IsBold = true)]
    public string? StockCodeId { get; set; }
    
    /// <summary>
    /// 状态（0未执行；1正在入库；2已入库）
    /// </summary>
    [ImporterHeader(Name = "状态（0未执行；1正在入库；2已入库）")]
    [ExporterHeader("状态（0未执行；1正在入库；2已入库）", Format = "", Width = 25, IsBold = true)]
    public int? Status { get; set; }
    
    /// <summary>
    /// 入库单id
    /// </summary>
    [ImporterHeader(Name = "入库单id")]
    [ExporterHeader("入库单id", Format = "", Width = 25, IsBold = true)]
    public string? ImportId { get; set; }
    
    /// <summary>
    /// 入库单详情id
    /// </summary>
    [ImporterHeader(Name = "入库单详情id")]
    [ExporterHeader("入库单详情id", Format = "", Width = 25, IsBold = true)]
    public string? ImportDetailId { get; set; }
    
    /// <summary>
    /// 入库流水id
    /// </summary>
    [ImporterHeader(Name = "入库流水id")]
    [ExporterHeader("入库流水id", Format = "", Width = 25, IsBold = true)]
    public string? ImportOrderId { get; set; }
    
    /// <summary>
    /// 等级
    /// </summary>
    [ImporterHeader(Name = "等级")]
    [ExporterHeader("等级", Format = "", Width = 25, IsBold = true)]
    public int? BoxLevel { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [ImporterHeader(Name = "生产日期")]
    [ExporterHeader("生产日期", Format = "", Width = 25, IsBold = true)]
    public DateTime? ProductionDate { get; set; }
    
    /// <summary>
    /// 保质期
    /// </summary>
    [ImporterHeader(Name = "保质期")]
    [ExporterHeader("保质期", Format = "", Width = 25, IsBold = true)]
    public DateTime? ValidateDay { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    [ImporterHeader(Name = "批次")]
    [ExporterHeader("批次", Format = "", Width = 25, IsBold = true)]
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 物料id
    /// </summary>
    [ImporterHeader(Name = "物料id")]
    [ExporterHeader("物料id", Format = "", Width = 25, IsBold = true)]
    public string? MaterialId { get; set; }
    
    /// <summary>
    /// 供应商ID
    /// </summary>
    [ImporterHeader(Name = "供应商ID")]
    [ExporterHeader("供应商ID", Format = "", Width = 25, IsBold = true)]
    public long? SupplierId { get; set; }

    /// <summary>
    /// 制造商ID
    /// </summary>
    [ImporterHeader(Name = "制造商ID")]
    [ExporterHeader("制造商ID", Format = "", Width = 25, IsBold = true)]
    public long? ManufacturerId { get; set; }
    
    /// <summary>
    /// 是否零箱（0否；1是）
    /// </summary>
    [ImporterHeader(Name = "是否零箱（0否；1是）")]
    [ExporterHeader("是否零箱（0否；1是）", Format = "", Width = 25, IsBold = true)]
    public int? BulkTank { get; set; }
    
    /// <summary>
    /// 是否抽检箱（0否；1是）
    /// </summary>
    [ImporterHeader(Name = "是否抽检箱（0否；1是）")]
    [ExporterHeader("是否抽检箱（0否；1是）", Format = "", Width = 25, IsBold = true)]
    public int? IsSamplingBox { get; set; }
    
    /// <summary>
    /// 质检状态（0待检；1合格；2不合格）
    /// </summary>
    [ImporterHeader(Name = "质检状态（0待检；1合格；2不合格）")]
    [ExporterHeader("质检状态（0待检；1合格；2不合格）", Format = "", Width = 25, IsBold = true)]
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// erp箱二维码
    /// </summary>
    [ImporterHeader(Name = "erp箱二维码")]
    [ExporterHeader("erp箱二维码", Format = "", Width = 25, IsBold = true)]
    public string? QRCode { get; set; }
    
    /// <summary>
    /// 采样日期
    /// </summary>
    [ImporterHeader(Name = "采样日期")]
    [ExporterHeader("采样日期", Format = "", Width = 25, IsBold = true)]
    public DateTime? SamplingDate { get; set; }
    
    /// <summary>
    /// 浆员编码
    /// </summary>
    [ImporterHeader(Name = "浆员编码")]
    [ExporterHeader("浆员编码", Format = "", Width = 25, IsBold = true)]
    public string? StaffCode { get; set; }
    
    /// <summary>
    /// 浆员姓名
    /// </summary>
    [ImporterHeader(Name = "浆员姓名")]
    [ExporterHeader("浆员姓名", Format = "", Width = 25, IsBold = true)]
    public string? StaffName { get; set; }
    
    /// <summary>
    /// 血浆重量
    /// </summary>
    [ImporterHeader(Name = "血浆重量")]
    [ExporterHeader("血浆重量", Format = "", Width = 25, IsBold = true)]
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// 剔除原因（手持剔除异常血浆存放）
    /// </summary>
    [ImporterHeader(Name = "剔除原因（手持剔除异常血浆存放）")]
    [ExporterHeader("剔除原因（手持剔除异常血浆存放）", Format = "", Width = 25, IsBold = true)]
    public string? ReasonsForExcl { get; set; }
    
    /// <summary>
    /// 检疫期状态（1检疫期满足；2检疫期不满足；3检疫期不合格）
    /// </summary>
    [ImporterHeader(Name = "检疫期状态（1检疫期满足；2检疫期不满足；3检疫期不合格）")]
    [ExporterHeader("检疫期状态（1检疫期满足；2检疫期不满足；3检疫期不合格）", Format = "", Width = 25, IsBold = true)]
    public int? ExtractStatus { get; set; }
    
    /// <summary>
    /// 是否挑浆（0默认；1挑浆）
    /// </summary>
    [ImporterHeader(Name = "是否挑浆（0默认；1挑浆）")]
    [ExporterHeader("是否挑浆（0默认；1挑浆）", Format = "", Width = 25, IsBold = true)]
    public string? PickingSlurry { get; set; }
    
    /// <summary>
    /// 血浆拒收类型id
    /// </summary>
    [ImporterHeader(Name = "血浆拒收类型id")]
    [ExporterHeader("血浆拒收类型id", Format = "", Width = 25, IsBold = true)]
    public string? plasmaRejectTypeId { get; set; }
    
    
 
    
}
