// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using SQLitePCL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.NET.Application;
public class ChartDataDto
{
    public string Title { get; set; }
    public ChartType ChartType { get; set; }
    public List<string> Labels { get; set; } // X轴或分类标签
    public List<string> xAxisData { get; set; }
    public List<ChartSeries> Series { get; set; }
}

public class ChartSeries
{
    public string Name { get; set; }
    public string Type { get; set; } // "bar", "line", "pie"
    public List<object> Data { get; set; }
    public Dictionary<string, object> Options { get; set; }
}

public enum ChartType
{
    [Description("柱状图")]
    Bar,
    [Description("折线图")]
    Line,
    [Description("饼图")]
    Pie,
    [Description("自定义图表")]
    Custom
}

