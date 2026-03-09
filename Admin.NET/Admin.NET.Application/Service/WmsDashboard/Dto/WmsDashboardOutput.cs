// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Application;

/// <summary>
/// 出入库任务趋势数据输出
/// </summary>
public class WmsTaskTrendOutput
{
    /// <summary>
    /// 近7天趋势数据
    /// </summary>
    public List<DailyTaskCount> Last7Days { get; set; }

    /// <summary>
    /// 近30天趋势数据
    /// </summary>
    public List<DailyTaskCount> Last30Days { get; set; }

    /// <summary>
    /// 当前入库任务总数（未作废）
    /// </summary>
    public int CurrentImportTotal { get; set; }

    /// <summary>
    /// 当前出库任务总数（未作废）
    /// </summary>
    public int CurrentExportTotal { get; set; }

    /// <summary>
    /// 任务进度列表（近7天）
    /// </summary>
    public List<TaskProgressInfo> TaskProgress7Days { get; set; }

    /// <summary>
    /// 任务进度列表（近30天）
    /// </summary>
    public List<TaskProgressInfo> TaskProgress30Days { get; set; }
}

/// <summary>
/// 任务进度信息
/// </summary>
public class TaskProgressInfo
{
    /// <summary>
    /// 任务名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 待办数量
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 已完成数量
    /// </summary>
    public int Completed { get; set; }

    /// <summary>
    /// 总数量
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 颜色
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public string Priority { get; set; }
}

/// <summary>
/// 每日任务数量统计
/// </summary>
public class DailyTaskCount
{
    /// <summary>
    /// 日期
    /// </summary>
    public string Date { get; set; }

    /// <summary>
    /// 入库任务数量
    /// </summary>
    public int ImportCount { get; set; }

    /// <summary>
    /// 出库任务数量
    /// </summary>
    public int ExportCount { get; set; }
}
