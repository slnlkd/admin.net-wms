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

public class WmsImportTaskCancelInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// 任务编码
    /// </summary>
    public string? TaskNo { get; set; }
}
public class WmsImportTaskFeedbackInput
{
    /// <summary>
    /// 任务号 
    /// </summary>
    public string TaskNo { get; set; }
    /// <summary>
    /// 状态码 （判断是完成还是取消）
    /// </summary>
    public string Code { get; set; }
    /// <summary>
    /// 任务类型
    /// </summary>
    public string TaskType { get; set; }
    /// <summary>
    /// 载具码
    /// </summary>
    public string VehicleCode { get; set; }
    /// <summary>
    /// 目标储位编码
    /// </summary>
    public string TaskEnd { get; set; }
    /// <summary>
    /// 是否手动完成 1是 0否
    /// </summary>
    public string IsDoneManually { get; set; }
}
/// <summary>
/// 入库任务表基础输入参数
/// </summary>
public class WmsImportTaskBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 任务号
    /// </summary>
    public virtual string? TaskNo { get; set; }
    
    /// <summary>
    /// 发送方
    /// </summary>
    public virtual string? Sender { get; set; }
    
    /// <summary>
    /// 接收方
    /// </summary>
    public virtual string? Receiver { get; set; }
    
    /// <summary>
    /// 是否成功
    /// </summary>
    public virtual int? IsSuccess { get; set; }
    
    /// <summary>
    /// 发送时间
    /// </summary>
    public virtual DateTime? SendDate { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public virtual DateTime? BackDate { get; set; }
    
    /// <summary>
    /// 描述发送报文
    /// </summary>
    public virtual string? Message { get; set; }
    
    /// <summary>
    /// 托盘码
    /// </summary>
    public virtual string? StockCode { get; set; }
    
    /// <summary>
    /// 描述返回信息
    /// </summary>
    public virtual string? Msg { get; set; }
    
    /// <summary>
    /// 任务状态
    /// </summary>
    public virtual int? Status { get; set; }
    
    /// <summary>
    /// 取消时间
    /// </summary>
    public virtual DateTime? CancelDate { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    public virtual DateTime? FinishDate { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public virtual long? WareHouseId { get; set; }
    
    /// <summary>
    /// 起始位置
    /// </summary>
    public virtual string? StartLocation { get; set; }
    
    /// <summary>
    /// 目标位置
    /// </summary>
    public virtual string? EndLocation { get; set; }
    
    /// <summary>
    /// 二次申请后目标位置
    /// </summary>
    public virtual string? NewEnd { get; set; }
    
}

/// <summary>
/// 入库任务表分页查询输入参数
/// </summary>
public class PageWmsImportTaskInput : BasePageInput
{
    /// <summary>
    /// 任务号
    /// </summary>
    public string? TaskNo { get; set; }
    
    /// <summary>
    /// 发送方
    /// </summary>
    public string? Sender { get; set; }
    
    /// <summary>
    /// 接收方
    /// </summary>
    public string? Receiver { get; set; }
    
    /// <summary>
    /// 是否成功
    /// </summary>
    public int? IsSuccess { get; set; }
    
    /// <summary>
    /// 发送时间范围
    /// </summary>
     public DateTime?[] SendDateRange { get; set; }
    
    /// <summary>
    /// 结束时间范围
    /// </summary>
     public DateTime?[] BackDateRange { get; set; }
    
    /// <summary>
    /// 描述发送报文
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// 托盘码
    /// </summary>
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 描述返回信息
    /// </summary>
    public string? Msg { get; set; }
    
    /// <summary>
    /// 任务状态
    /// </summary>
    public int? Status { get; set; }
    
    /// <summary>
    /// 取消时间范围
    /// </summary>
     public DateTime?[] CancelDateRange { get; set; }
    
    /// <summary>
    /// 完成时间范围
    /// </summary>
     public DateTime?[] FinishDateRange { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? WareHouseId { get; set; }
    
    /// <summary>
    /// 起始位置
    /// </summary>
    public string? StartLocation { get; set; }
    
    /// <summary>
    /// 目标位置
    /// </summary>
    public string? EndLocation { get; set; }
    
    /// <summary>
    /// 二次申请后目标位置
    /// </summary>
    public string? NewEnd { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 入库任务表增加输入参数
/// </summary>
public class AddWmsImportTaskInput
{
    /// <summary>
    /// 任务号
    /// </summary>
    [MaxLength(32, ErrorMessage = "任务号字符长度不能超过32")]
    public string? TaskNo { get; set; }
    
    /// <summary>
    /// 发送方
    /// </summary>
    [MaxLength(10, ErrorMessage = "发送方字符长度不能超过10")]
    public string? Sender { get; set; }
    
    /// <summary>
    /// 接收方
    /// </summary>
    [MaxLength(10, ErrorMessage = "接收方字符长度不能超过10")]
    public string? Receiver { get; set; }
    
    /// <summary>
    /// 是否成功
    /// </summary>
    public int? IsSuccess { get; set; }
    
    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime? SendDate { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? BackDate { get; set; }
    
    /// <summary>
    /// 描述发送报文
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// 托盘码
    /// </summary>
    [MaxLength(10, ErrorMessage = "托盘码字符长度不能超过10")]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 描述返回信息
    /// </summary>
    public string? Msg { get; set; }
    
    /// <summary>
    /// 任务状态
    /// </summary>
    public int? Status { get; set; }
    
    /// <summary>
    /// 取消时间
    /// </summary>
    public DateTime? CancelDate { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? FinishDate { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? WareHouseId { get; set; }
    
    /// <summary>
    /// 起始位置
    /// </summary>
    [MaxLength(50, ErrorMessage = "起始位置字符长度不能超过50")]
    public string? StartLocation { get; set; }
    
    /// <summary>
    /// 目标位置
    /// </summary>
    [MaxLength(50, ErrorMessage = "目标位置字符长度不能超过50")]
    public string? EndLocation { get; set; }
    
    /// <summary>
    /// 二次申请后目标位置
    /// </summary>
    [MaxLength(32, ErrorMessage = "二次申请后目标位置字符长度不能超过32")]
    public string? NewEnd { get; set; }
    
}

/// <summary>
/// 入库任务表删除输入参数
/// </summary>
public class DeleteWmsImportTaskInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 入库任务表更新输入参数
/// </summary>
public class UpdateWmsImportTaskInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 任务号
    /// </summary>    
    [MaxLength(32, ErrorMessage = "任务号字符长度不能超过32")]
    public string? TaskNo { get; set; }
    
    /// <summary>
    /// 发送方
    /// </summary>    
    [MaxLength(10, ErrorMessage = "发送方字符长度不能超过10")]
    public string? Sender { get; set; }
    
    /// <summary>
    /// 接收方
    /// </summary>    
    [MaxLength(10, ErrorMessage = "接收方字符长度不能超过10")]
    public string? Receiver { get; set; }
    
    /// <summary>
    /// 是否成功
    /// </summary>    
    public int? IsSuccess { get; set; }
    
    /// <summary>
    /// 发送时间
    /// </summary>    
    public DateTime? SendDate { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>    
    public DateTime? BackDate { get; set; }
    
    /// <summary>
    /// 描述发送报文
    /// </summary>    
    public string? Message { get; set; }
    
    /// <summary>
    /// 托盘码
    /// </summary>    
    [MaxLength(10, ErrorMessage = "托盘码字符长度不能超过10")]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 描述返回信息
    /// </summary>    
    public string? Msg { get; set; }
    
    /// <summary>
    /// 任务状态
    /// </summary>    
    public int? Status { get; set; }
    
    /// <summary>
    /// 取消时间
    /// </summary>    
    public DateTime? CancelDate { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>    
    public DateTime? FinishDate { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>    
    public long? WareHouseId { get; set; }
    
    /// <summary>
    /// 起始位置
    /// </summary>    
    [MaxLength(50, ErrorMessage = "起始位置字符长度不能超过50")]
    public string? StartLocation { get; set; }
    
    /// <summary>
    /// 目标位置
    /// </summary>    
    [MaxLength(50, ErrorMessage = "目标位置字符长度不能超过50")]
    public string? EndLocation { get; set; }
    
    /// <summary>
    /// 二次申请后目标位置
    /// </summary>    
    [MaxLength(32, ErrorMessage = "二次申请后目标位置字符长度不能超过32")]
    public string? NewEnd { get; set; }
    
}

/// <summary>
/// 入库任务表主键查询输入参数
/// </summary>
public class QueryByIdWmsImportTaskInput : DeleteWmsImportTaskInput
{
}

/// <summary>
/// 入库任务表数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsImportTaskInput : BaseImportInput
{
    /// <summary>
    /// 任务号
    /// </summary>
    [ImporterHeader(Name = "任务号")]
    [ExporterHeader("任务号", Format = "", Width = 25, IsBold = true)]
    public string? TaskNo { get; set; }
    
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
    /// 是否成功
    /// </summary>
    [ImporterHeader(Name = "是否成功")]
    [ExporterHeader("是否成功", Format = "", Width = 25, IsBold = true)]
    public int? IsSuccess { get; set; }
    
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
    /// 描述发送报文
    /// </summary>
    [ImporterHeader(Name = "描述发送报文")]
    [ExporterHeader("描述发送报文", Format = "", Width = 25, IsBold = true)]
    public string? Message { get; set; }
    
    /// <summary>
    /// 托盘码
    /// </summary>
    [ImporterHeader(Name = "托盘码")]
    [ExporterHeader("托盘码", Format = "", Width = 25, IsBold = true)]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 描述返回信息
    /// </summary>
    [ImporterHeader(Name = "描述返回信息")]
    [ExporterHeader("描述返回信息", Format = "", Width = 25, IsBold = true)]
    public string? Msg { get; set; }
    
    /// <summary>
    /// 任务状态
    /// </summary>
    [ImporterHeader(Name = "任务状态")]
    [ExporterHeader("任务状态", Format = "", Width = 25, IsBold = true)]
    public int? Status { get; set; }
    
    /// <summary>
    /// 取消时间
    /// </summary>
    [ImporterHeader(Name = "取消时间")]
    [ExporterHeader("取消时间", Format = "", Width = 25, IsBold = true)]
    public DateTime? CancelDate { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    [ImporterHeader(Name = "完成时间")]
    [ExporterHeader("完成时间", Format = "", Width = 25, IsBold = true)]
    public DateTime? FinishDate { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    [ImporterHeader(Name = "仓库ID")]
    [ExporterHeader("仓库ID", Format = "", Width = 25, IsBold = true)]
    public long? WareHouseId { get; set; }
    
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
    /// 二次申请后目标位置
    /// </summary>
    [ImporterHeader(Name = "二次申请后目标位置")]
    [ExporterHeader("二次申请后目标位置", Format = "", Width = 25, IsBold = true)]
    public string? NewEnd { get; set; }
    
}
