// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵守 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 与 LICENSE-APACHE 文件中。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

namespace Admin.NET.Application.Service.WmsPort.Dto;

#region 常量定义

/// <summary>
/// 库位管理业务常量
/// </summary>
public static class PortSlotConstants
{
    /// <summary>
    /// 库位预留数量（用于移库）
    /// </summary>
    public const int ReservedSlotCount = 7;
}

/// <summary>
/// 库位状态常量
/// </summary>
public static class SlotStatusCode
{
    public const int Empty = 0;           // 空
    public const int HasItems = 1;        // 有物品
    public const int Processing = 2;      // 正在入库
    public const int Reserved = 3;        // 预留
    public const int MovingOut = 5;       // 正在移出
}

/// <summary>
/// 托盘类型
/// </summary>
public static class TrayType
{
    public const int StorageCage = 0;     // 仓储笼
    public const int NormalTray = 1;      // 托盘
    public const int InspectionTray = 2;  // 质检托盘
}

/// <summary>
/// 巷道类型（原项目LanewayType为string，新项目为int）
/// </summary>
public static class LanewayTypeCode
{
    public const int SinglePort = 0;      // 单口巷道（对应原"1"）
    public const int MultiPort = 1;       // 多口巷道（对应原"3"）
}

/// <summary>
/// AB面标识
/// </summary>
public static class ABSide
{
    public const string A = "A";  // A面（小深度入口）
    public const string B = "B";  // B面（大深度入口）
}

/// <summary>
/// 执行标志常量
/// </summary>
public static class ExecuteFlag
{
    public const string Invalid = "-1";    // 无效
    public const string Pending = "01";    // 待执行
    public const string Processing = "02"; // 执行中
    public const string Completed = "03";  // 已完成
}

#endregion

/// <summary>
/// 托盘信息
/// </summary>
public class TrayInformation
{
    /// <summary>
    /// 托盘码
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialCode { get; set; }

    /// <summary>
    /// 批次号
    /// </summary>
    public string LotNo { get; set; }

    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? WarehouseId { get; set; }

    /// <summary>
    /// 是否质检托盘
    /// </summary>
    public bool IsInspectionTray { get; set; }

    /// <summary>
    /// 是否回流入库
    /// </summary>
    public bool IsReturnFlow { get; set; }
}
