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
/// 移库任务表基础输入参数
/// </summary>
public class WmsMoveTaskBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
}

/// <summary>
/// 移库任务表分页查询输入参数
/// </summary>
public class PageWmsMoveTaskInput : BasePageInput
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
    /// 托盘条码
    /// </summary>
    public string? StockCodeId { get; set; }
    
    /// <summary>
    /// 完成时间范围
    /// </summary>
     public DateTime?[] FinishDateRange { get; set; }
    
    /// <summary>
    /// 任务状态
    /// </summary>
    [Dict("ExecuteFlag", AllowNullValue=true)]
    public int? Status { get; set; }
    
}

/// <summary>
/// 移库任务表增加输入参数
/// </summary>
public class AddWmsMoveTaskInput
{
}

/// <summary>
/// 移库任务表删除输入参数
/// </summary>
public class DeleteWmsMoveTaskInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 移库任务表更新输入参数
/// </summary>
public class UpdateWmsMoveTaskInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 移库任务表主键查询输入参数
/// </summary>
public class QueryByIdWmsMoveTaskInput : DeleteWmsMoveTaskInput
{
}

