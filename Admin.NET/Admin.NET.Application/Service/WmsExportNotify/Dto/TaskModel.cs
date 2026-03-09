// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.NET.Application;
public class TaskModel : BasePageInput
{
    /// <summary>
    /// 发送方
    /// </summary>
    public string Sender { get; set; }
    /// <summary>
    /// 接收方
    /// </summary>
    public string Receiver { get; set; }
    /// <summary>
    /// 任务ID
    /// </summary>
    public string TaskId { get; set; }
    /// <summary>
    /// 任务编码
    /// </summary>
    public string taskNo { get; set; }
    /// <summary>
    /// 执行状态
    /// </summary>
    public string ExportTaskFlag { get; set; }
    /// <summary>
    /// 托盘编码
    /// </summary>
    public string StockCode { get; set; }
    /// <summary>
    /// 仓库
    /// </summary>
    public string WarehouseId { get; set; }
    /// <summary>
    /// 巷道
    /// </summary>
    public string LanewayId { get; set; }
    public string Id { get; set; }
    /// <summary>
    /// 开始时间
    /// </summary>
    public string startDate { get; set; }
    /// <summary>
    /// 结束时间
    /// </summary>
    public string endDate { get; set; }
}
