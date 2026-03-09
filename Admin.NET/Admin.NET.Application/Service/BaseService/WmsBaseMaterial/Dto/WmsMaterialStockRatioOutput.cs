// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Application;

/// <summary>
/// 时间筛选器枚举
/// </summary>
public enum TimeFilter
{
    /// <summary>
    /// 本周
    /// </summary>
    Week = 0,

    /// <summary>
    /// 本月
    /// </summary>
    Month = 1,

    /// <summary>
    /// 本季度
    /// </summary>
    Quarter = 2
}

/// <summary>
/// 物料库存占比输出 📊
/// </summary>
public class WmsMaterialStockRatioOutput
{
    /// <summary>
    /// 物料数据列表
    /// </summary>
    public List<WmsMaterialStockItem> MaterialData { get; set; } = new();

    /// <summary>
    /// 库存总量
    /// </summary>
    public int TotalInventory { get; set; }
}

/// <summary>
/// 物料库存项
/// </summary>
public class WmsMaterialStockItem
{
    /// <summary>
    /// 物料名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 数量
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// 占比百分比
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// 颜色值
    /// </summary>
    public string Color { get; set; } = string.Empty;
}