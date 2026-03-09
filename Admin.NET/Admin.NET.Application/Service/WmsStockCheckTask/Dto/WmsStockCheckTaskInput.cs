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
/// 盘点任务表分页查询输入参数
/// </summary>
public class PageWmsStockCheckTaskInput : BasePageInput
{
    /// <summary>
    /// 关键字
    /// </summary>
    public new string? Keyword { get; set; }

    /// <summary>
    /// 盘库任务号
    /// </summary>
    public string? CheckTaskNo { get; set; }

    /// <summary>
    /// 盘库单号
    /// </summary>
    public string? CheckBillCode { get; set; }

    /// <summary>
    /// 托盘条码
    /// </summary>
    public string? StockCode { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    public int? CheckTaskFlag { get; set; }

    /// <summary>
    /// 任务类型
    /// </summary>
    public int? TaskType { get; set; }

    /// <summary>
    /// 发送时间范围
    /// </summary>
    public DateTime[]? SendDateRange { get; set; }

    /// <summary>
    /// 返回时间范围
    /// </summary>
    public DateTime[]? BackDateRange { get; set; }

    /// <summary>
    /// 选中主键列表
    /// </summary>
    public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 取消盘点任务输入参数
/// </summary>
public class CancelWmsStockCheckTaskInput
{
    /// <summary>
    /// 盘库任务号
    /// </summary>
    [Required(ErrorMessage = "盘库任务号不能为空")]
    public string? CheckTaskNo { get; set; }
}