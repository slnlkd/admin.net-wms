// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;
using System.Collections.Generic;
using Admin.NET.Application.Entity;

namespace Admin.NET.Application.Service.WmsPort.Dto;

#region 常量定义

/// <summary>
/// 组托反馈业务常量
/// </summary>
public static class GroupPalletConstants
{
    /// <summary>
    /// 入库单据类型标识（RK）
    /// </summary>
    public const string ImportBillTypePrefix = "RK";

    /// <summary>
    /// 执行状态：待执行
    /// </summary>
    public const string ExecuteFlagPending = "01";

    /// <summary>
    /// 执行状态：执行中
    /// </summary>
    public const string ExecuteFlagProcessing = "02";

    /// <summary>
    /// 血浆库仓库ID
    /// </summary>
    public const int PlasmaWarehouseId = 1;

    /// <summary>
    /// 托盘状态：空闲
    /// </summary>
    public const int PalletStatusIdle = 0;

    /// <summary>
    /// 托盘状态：使用中
    /// </summary>
    public const int PalletStatusInUse = 1;

    /// <summary>
    /// 托盘最小长度（用于判断是否为WMS管理的托盘）
    /// </summary>
    public const int PalletMinLength = 8;

    /// <summary>
    /// 默认仓库ID（创建新托盘时使用）
    /// </summary>
    public const int DefaultWarehouseId = 1;

    /// <summary>
    /// 系统用户名称（WCS系统自动操作）
    /// </summary>
    public const string SystemUserName = "WCS";

    /// <summary>
    /// 系统用户ID（WCS系统自动操作）
    /// </summary>
    public const long SystemUserId = 0;
}

#endregion

#region 内部类

/// <summary>
/// 入库单据明细记录（包含明细、通知和物料信息）
/// </summary>
public class ImportDetailRecord
{
    /// <summary>
    /// 入库单据明细
    /// </summary>
    public WmsImportNotifyDetail Detail { get; set; }

    /// <summary>
    /// 入库单据
    /// </summary>
    public WmsImportNotify Notify { get; set; }

    /// <summary>
    /// 物料
    /// </summary>
    public WmsBaseMaterial Material { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialCode { get; set; }
}

/// <summary>
/// 箱码处理结果
/// </summary>
public class BoxProcessResult
{
    /// <summary>
    /// 总箱数
    /// </summary>
    public int TotalBoxCount { get; set; }

    /// <summary>
    /// 总重量
    /// </summary>
    public decimal TotalWeight { get; set; }
}

#endregion

/// <summary>
/// 组托反馈入参
/// </summary>
public class GroupPalletFeedbackInput
{
    /// <summary>
    /// 载具编码
    /// </summary>
    public string VehicleCode { get; set; }

    /// <summary>
    /// 单据编码
    /// </summary>
    public string BillCode { get; set; }

    /// <summary>
    /// 箱码明细集合
    /// </summary>
    public List<GroupPalletItemInput> List { get; set; } = new();
}

/// <summary>
/// 组托反馈箱码明细
/// </summary>
public class GroupPalletItemInput
{
    public string BoxCode { get; set; }
    public decimal Qty { get; set; }
    public string MaterialCode { get; set; }
    public DateTime? ProductionDate { get; set; }
    public DateTime? ValidateDay { get; set; }
    public string LotNo { get; set; }
    public int BulkTank { get; set; }
    public DateTime? SamplingDate { get; set; }
    public string StaffCode { get; set; }
    public string StaffName { get; set; }
    public string InspectionStatus { get; set; }
    public decimal Weight { get; set; }
    public string ExtractStatus { get; set; }
}

/// <summary>
/// 组托反馈出参
/// </summary>
public class GroupPalletFeedbackOutput
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 提示信息
    /// </summary>
    public string Message { get; set; }
}

