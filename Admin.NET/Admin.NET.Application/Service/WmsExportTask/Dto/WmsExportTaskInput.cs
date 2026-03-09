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
/// 出库任务基础输入参数
/// </summary>
public class WmsExportTaskBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 出库任务号
    /// </summary>
    public virtual string? ExportTaskNo { get; set; }
    
    /// <summary>
    /// 发送方
    /// </summary>
    public virtual string? Sender { get; set; }
    
    /// <summary>
    /// 接收方
    /// </summary>
    public virtual string? Receiver { get; set; }
    
    /// <summary>
    /// 是否成功（0否、1是）
    /// </summary>
    public virtual int? IsSuccess { get; set; }
    
    /// <summary>
    /// 信息
    /// </summary>
    public virtual string? Information { get; set; }
    
    /// <summary>
    /// 发送时间
    /// </summary>
    public virtual DateTime? SendDate { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public virtual DateTime? BackDate { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    public virtual string? StockCode { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    public virtual string? StockSoltCode { get; set; }
    
    /// <summary>
    /// 描述信息
    /// </summary>
    public virtual string? Msg { get; set; }
    
    /// <summary>
    /// 执行状态（0：已下发；1：执行中；2：执行完毕）
    /// </summary>
    public virtual int? ExportTaskFlag { get; set; }
    
    /// <summary>
    /// 出库流水号
    /// </summary>
    public virtual string? ExportOrderNo { get; set; }
    
    /// <summary>
    /// 起始位置
    /// </summary>
    public virtual string? StartLocation { get; set; }
    
    /// <summary>
    /// 目标位置
    /// </summary>
    public virtual string? EndLocation { get; set; }
    
    /// <summary>
    /// 仓库Id
    /// </summary>
    public virtual long? WarehouseId { get; set; }
    
    /// <summary>
    /// 1：出库；2：出库移库
    /// </summary>
    public virtual int? TaskType { get; set; }
    
}

/// <summary>
/// 出库任务分页查询输入参数
/// </summary>
public class PageWmsExportTaskInput : BasePageInput
{
    /// <summary>
    /// 出库任务号
    /// </summary>
    public string? ExportTaskNo { get; set; }
    
    /// <summary>
    /// 是否成功（0否、1是）
    /// </summary>
    public int? IsSuccess { get; set; }
    
    /// <summary>
    /// 发送时间范围
    /// </summary>
     public DateTime?[] SendDateRange { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    public string? StockSoltCode { get; set; }
    
    /// <summary>
    /// 执行状态（0：已下发；1：执行中；2：执行完毕）
    /// </summary>
    public int? ExportTaskFlag { get; set; }
    
    /// <summary>
    /// 出库流水号
    /// </summary>
    public string? ExportOrderNo { get; set; }
    
    /// <summary>
    /// 起始位置
    /// </summary>
    public string? StartLocation { get; set; }
    
    /// <summary>
    /// 目标位置
    /// </summary>
    public string? EndLocation { get; set; }
    
    /// <summary>
    /// 仓库Id
    /// </summary>
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 1：出库；2：出库移库
    /// </summary>
    public int? TaskType { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 出库任务增加输入参数
/// </summary>
public class AddWmsExportTaskInput
{
    /// <summary>
    /// 出库任务号
    /// </summary>
    [MaxLength(100, ErrorMessage = "出库任务号字符长度不能超过100")]
    public string? ExportTaskNo { get; set; }
    
    /// <summary>
    /// 发送方
    /// </summary>
    [MaxLength(100, ErrorMessage = "发送方字符长度不能超过100")]
    public string? Sender { get; set; }
    
    /// <summary>
    /// 接收方
    /// </summary>
    [MaxLength(100, ErrorMessage = "接收方字符长度不能超过100")]
    public string? Receiver { get; set; }
    
    /// <summary>
    /// 是否成功（0否、1是）
    /// </summary>
    public int? IsSuccess { get; set; }
    
    /// <summary>
    /// 信息
    /// </summary>
    [MaxLength(2147483647, ErrorMessage = "信息字符长度不能超过2147483647")]
    public string? Information { get; set; }
    
    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime? SendDate { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? BackDate { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    [MaxLength(100, ErrorMessage = "托盘条码字符长度不能超过100")]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    [MaxLength(100, ErrorMessage = "储位编码字符长度不能超过100")]
    public string? StockSoltCode { get; set; }
    
    /// <summary>
    /// 描述信息
    /// </summary>
    [MaxLength(500, ErrorMessage = "描述信息字符长度不能超过500")]
    public string? Msg { get; set; }
    
    /// <summary>
    /// 执行状态（0：已下发；1：执行中；2：执行完毕）
    /// </summary>
    public int? ExportTaskFlag { get; set; }
    
    /// <summary>
    /// 出库流水号
    /// </summary>
    [MaxLength(100, ErrorMessage = "出库流水号字符长度不能超过100")]
    public string? ExportOrderNo { get; set; }
    
    /// <summary>
    /// 起始位置
    /// </summary>
    [MaxLength(100, ErrorMessage = "起始位置字符长度不能超过100")]
    public string? StartLocation { get; set; }
    
    /// <summary>
    /// 目标位置
    /// </summary>
    [MaxLength(100, ErrorMessage = "目标位置字符长度不能超过100")]
    public string? EndLocation { get; set; }
    
    /// <summary>
    /// 仓库Id
    /// </summary>
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 1：出库；2：出库移库
    /// </summary>
    public int? TaskType { get; set; }
    
}

/// <summary>
/// 出库任务删除输入参数
/// </summary>
public class DeleteWmsExportTaskInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 出库任务更新输入参数
/// </summary>
public class UpdateWmsExportTaskInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 出库任务号
    /// </summary>    
    [MaxLength(100, ErrorMessage = "出库任务号字符长度不能超过100")]
    public string? ExportTaskNo { get; set; }
    
    /// <summary>
    /// 发送方
    /// </summary>    
    [MaxLength(100, ErrorMessage = "发送方字符长度不能超过100")]
    public string? Sender { get; set; }
    
    /// <summary>
    /// 接收方
    /// </summary>    
    [MaxLength(100, ErrorMessage = "接收方字符长度不能超过100")]
    public string? Receiver { get; set; }
    
    /// <summary>
    /// 是否成功（0否、1是）
    /// </summary>    
    public int? IsSuccess { get; set; }
    
    /// <summary>
    /// 信息
    /// </summary>    
    [MaxLength(2147483647, ErrorMessage = "信息字符长度不能超过2147483647")]
    public string? Information { get; set; }
    
    /// <summary>
    /// 发送时间
    /// </summary>    
    public DateTime? SendDate { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>    
    public DateTime? BackDate { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>    
    [MaxLength(100, ErrorMessage = "托盘条码字符长度不能超过100")]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>    
    [MaxLength(100, ErrorMessage = "储位编码字符长度不能超过100")]
    public string? StockSoltCode { get; set; }
    
    /// <summary>
    /// 描述信息
    /// </summary>    
    [MaxLength(500, ErrorMessage = "描述信息字符长度不能超过500")]
    public string? Msg { get; set; }
    
    /// <summary>
    /// 执行状态（0：已下发；1：执行中；2：执行完毕）
    /// </summary>    
    public int? ExportTaskFlag { get; set; }
    
    /// <summary>
    /// 出库流水号
    /// </summary>    
    [MaxLength(100, ErrorMessage = "出库流水号字符长度不能超过100")]
    public string? ExportOrderNo { get; set; }
    
    /// <summary>
    /// 起始位置
    /// </summary>    
    [MaxLength(100, ErrorMessage = "起始位置字符长度不能超过100")]
    public string? StartLocation { get; set; }
    
    /// <summary>
    /// 目标位置
    /// </summary>    
    [MaxLength(100, ErrorMessage = "目标位置字符长度不能超过100")]
    public string? EndLocation { get; set; }
    
    /// <summary>
    /// 仓库Id
    /// </summary>    
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 1：出库；2：出库移库
    /// </summary>    
    public int? TaskType { get; set; }
    
}

/// <summary>
/// 出库任务主键查询输入参数
/// </summary>
public class QueryByIdWmsExportTaskInput : DeleteWmsExportTaskInput
{
}

/// <summary>
/// 出库任务数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsExportTaskInput : BaseImportInput
{
    /// <summary>
    /// 出库任务号
    /// </summary>
    [ImporterHeader(Name = "出库任务号")]
    [ExporterHeader("出库任务号", Format = "", Width = 25, IsBold = true)]
    public string? ExportTaskNo { get; set; }
    
    /// <summary>
    /// 发送方
    /// </summary>
    [ImporterHeader(Name = "发送方")]
    [ExporterHeader("发送方", Format = "", Width = 25, IsBold = true)]
    public string? Sender { get; set; }
    
    /// <summary>
    /// 接收方
    /// </summary>
    [ImporterHeader(Name = "接收方")]
    [ExporterHeader("接收方", Format = "", Width = 25, IsBold = true)]
    public string? Receiver { get; set; }
    
    /// <summary>
    /// 是否成功（0否、1是）
    /// </summary>
    [ImporterHeader(Name = "是否成功（0否、1是）")]
    [ExporterHeader("是否成功（0否、1是）", Format = "", Width = 25, IsBold = true)]
    public int? IsSuccess { get; set; }
    
    /// <summary>
    /// 信息
    /// </summary>
    [ImporterHeader(Name = "信息")]
    [ExporterHeader("信息", Format = "", Width = 25, IsBold = true)]
    public string? Information { get; set; }
    
    /// <summary>
    /// 发送时间
    /// </summary>
    [ImporterHeader(Name = "发送时间")]
    [ExporterHeader("发送时间", Format = "", Width = 25, IsBold = true)]
    public DateTime? SendDate { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    [ImporterHeader(Name = "结束时间")]
    [ExporterHeader("结束时间", Format = "", Width = 25, IsBold = true)]
    public DateTime? BackDate { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    [ImporterHeader(Name = "托盘条码")]
    [ExporterHeader("托盘条码", Format = "", Width = 25, IsBold = true)]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    [ImporterHeader(Name = "储位编码")]
    [ExporterHeader("储位编码", Format = "", Width = 25, IsBold = true)]
    public string? StockSoltCode { get; set; }
    
    /// <summary>
    /// 描述信息
    /// </summary>
    [ImporterHeader(Name = "描述信息")]
    [ExporterHeader("描述信息", Format = "", Width = 25, IsBold = true)]
    public string? Msg { get; set; }
    
    /// <summary>
    /// 执行状态（0：已下发；1：执行中；2：执行完毕）
    /// </summary>
    [ImporterHeader(Name = "执行状态（0：已下发；1：执行中；2：执行完毕）")]
    [ExporterHeader("执行状态（0：已下发；1：执行中；2：执行完毕）", Format = "", Width = 25, IsBold = true)]
    public int? ExportTaskFlag { get; set; }
    
    /// <summary>
    /// 出库流水号
    /// </summary>
    [ImporterHeader(Name = "出库流水号")]
    [ExporterHeader("出库流水号", Format = "", Width = 25, IsBold = true)]
    public string? ExportOrderNo { get; set; }
    
    /// <summary>
    /// 起始位置
    /// </summary>
    [ImporterHeader(Name = "起始位置")]
    [ExporterHeader("起始位置", Format = "", Width = 25, IsBold = true)]
    public string? StartLocation { get; set; }
    
    /// <summary>
    /// 目标位置
    /// </summary>
    [ImporterHeader(Name = "目标位置")]
    [ExporterHeader("目标位置", Format = "", Width = 25, IsBold = true)]
    public string? EndLocation { get; set; }
    
    /// <summary>
    /// 仓库Id
    /// </summary>
    [ImporterHeader(Name = "仓库Id")]
    [ExporterHeader("仓库Id", Format = "", Width = 25, IsBold = true)]
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 1：出库；2：出库移库
    /// </summary>
    [ImporterHeader(Name = "1：出库；2：出库移库")]
    [ExporterHeader("1：出库；2：出库移库", Format = "", Width = 25, IsBold = true)]
    public int? TaskType { get; set; }
    
}
