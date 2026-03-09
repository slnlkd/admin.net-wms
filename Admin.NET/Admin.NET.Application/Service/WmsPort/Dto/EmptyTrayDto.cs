// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;
using System.Collections.Generic;

using Admin.NET.Application;

namespace Admin.NET.Application.Service.WmsPort.Dto;

#region 常量定义

/// <summary>
/// 空托盘业务常量（PortEmptyTrayBind 和 PortEmptyTray 共用）
/// </summary>
public static class EmptyTrayConstants
{
    /// <summary>
    /// 系统配置常量
    /// </summary>
    public static class SystemConstants
    {
        /// <summary>
        /// 默认创建用户名（与旧系统保持一致）
        /// </summary>
        public const string DefaultCreateUser = "Wcs";

        /// <summary>
        /// 操作结果代码：成功
        /// </summary>
        public const int ResultCodeSuccess = 1;

        /// <summary>
        /// 入库流水号前缀
        /// </summary>
        public const string ImportOrderNoPrefix = "RK";

        /// <summary>
        /// 出库任务编号前缀
        /// </summary>
        public const string ExportTaskPrefix = "CKR";

        /// <summary>
        /// 系统标识：发送方
        /// </summary>
        public const string SenderSystem = "WMS";

        /// <summary>
        /// 系统标识：接收方
        /// </summary>
        public const string ReceiverSystem = "WCS";

        /// <summary>
        /// 默认创建用户ID
        /// </summary>
        public const long DefaultCreateUserId = 0;

        /// <summary>
        /// 默认更新用户ID
        /// </summary>
        public const long DefaultUpdateUserId = 0;
    }

    /// <summary>
    /// 仓库类型编码常量
    /// </summary>
    public static class WarehouseTypes
    {
        /// <summary>
        /// 仓库类型A的编码（血浆库）
        /// </summary>
        public const string TypeA = "A";

        /// <summary>
        /// 仓库类型B的编码（原料库）
        /// </summary>
        public const string TypeB = "B";

        /// <summary>
        /// 获取仓库类型描述
        /// </summary>
        /// <param name="warehouseType">仓库类型编码</param>
        /// <returns>仓库类型描述</returns>
        public static string GetDescription(string warehouseType) => warehouseType switch
        {
            TypeA => "血浆库",
            TypeB => "原料库",
            _ => "未知仓库类型"
        };
    }

    /// <summary>
    /// 托盘状态常量
    /// </summary>
    public static class TrayStatus
    {
        /// <summary>
        /// 托盘状态：未使用
        /// </summary>
        public const int Unused = 0;

        /// <summary>
        /// 托盘状态：已使用
        /// </summary>
        public const int Used = 1;

        /// <summary>
        /// 获取状态描述
        /// </summary>
        /// <param name="status">状态值</param>
        /// <returns>状态描述文本</returns>
        public static string GetDescription(int status) => status switch
        {
            Unused => "未使用",
            Used => "已使用",
            _ => "未知状态"
        };
    }

    /// <summary>
    /// 箱码状态常量
    /// </summary>
    public static class BoxStatus
    {
        /// <summary>
        /// 箱码状态：未执行
        /// </summary>
        public const int Pending = 0;

        /// <summary>
        /// 箱码状态：正在入库（已分配库位）
        /// </summary>
        public const int InProgress = 1;

        /// <summary>
        /// 获取状态描述
        /// </summary>
        /// <param name="status">状态值</param>
        /// <returns>状态描述文本</returns>
        public static string GetDescription(int status) => status switch
        {
            Pending => "未执行",
            InProgress => "正在入库",
            _ => "未知状态"
        };
    }

    /// <summary>
    /// 操作类型常量
    /// </summary>
    public static class ActionTypes
    {
        /// <summary>
        /// 操作类型：绑定
        /// </summary>
        public const string Bind = "add";

        /// <summary>
        /// 操作类型：解绑
        /// </summary>
        public const string Unbind = "del";

        /// <summary>
        /// 默认操作类型
        /// </summary>
        public const string Default = Bind;

        /// <summary>
        /// 判断是否为绑定操作
        /// </summary>
        /// <param name="actionType">操作类型字符串</param>
        /// <returns>是否为绑定操作</returns>
        public static bool IsBind(string actionType) =>
            string.Equals(actionType, Bind, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 判断是否为解绑操作
        /// </summary>
        /// <param name="actionType">操作类型字符串</param>
        /// <returns>是否为解绑操作</returns>
        public static bool IsUnbind(string actionType) =>
            string.Equals(actionType, Unbind, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 入库执行标志常量
    /// </summary>
    public static class ImportExecuteFlags
    {
        /// <summary>
        /// 入库执行标志：待执行
        /// </summary>
        public const string Pending = "01";
    }

    /// <summary>
    /// 库位状态常量（PortEmptyTray 使用）
    /// </summary>
    public static class SlotStatus
    {
        /// <summary>
        /// 空储位：库位为空，可以存放货物
        /// </summary>
        public const int Empty = 0;

        /// <summary>
        /// 占用：库位已有货物占用，状态正常
        /// </summary>
        public const int Occupied = 1;

        /// <summary>
        /// 入库中：货物正在入库到此库位，暂时锁定
        /// </summary>
        public const int Importing = 2;

        /// <summary>
        /// 出库中：货物正在从此库位出库，暂时锁定
        /// </summary>
        public const int Exporting = 3;

        /// <summary>
        /// 移库中（入库）：货物正在移入此库位
        /// </summary>
        public const int MovingIn = 4;

        /// <summary>
        /// 移库中（出库）：货物正在移出此库位
        /// </summary>
        public const int MovingOut = 5;

        /// <summary>
        /// 空托盘：库位存放的是空托盘，特殊占用状态
        /// </summary>
        public const int EmptyTray = 6;

        /// <summary>
        /// 获取状态描述
        /// </summary>
        /// <param name="status">状态值</param>
        /// <returns>状态描述文本</returns>
        public static string GetDescription(int status) => status switch
        {
            Empty => "空储位",
            Occupied => "占用",
            Importing => "入库中",
            Exporting => "出库中",
            MovingIn => "移入中",
            MovingOut => "移出中",
            EmptyTray => "空托盘",
            _ => "未知状态"
        };
    }

    /// <summary>
    /// 任务类型常量（PortEmptyTray 使用）
    /// </summary>
    public static class TaskType
    {
        /// <summary>
        /// 出库任务：将货物从仓库出库到指定位置
        /// </summary>
        public const int Export = 1;

        /// <summary>
        /// 移库任务：在仓库内部移动货物位置
        /// </summary>
        public const int Move = 2;

        /// <summary>
        /// 获取任务类型描述
        /// </summary>
        /// <param name="taskType">任务类型值</param>
        /// <returns>任务类型描述</returns>
        public static string GetDescription(int taskType) => taskType switch
        {
            Export => "出库任务",
            Move => "移库任务",
            _ => "未知任务类型"
        };
    }

    /// <summary>
    /// WCS任务类型常量（PortEmptyTray 使用）
    /// </summary>
    public static class WcsTaskType
    {
        /// <summary>
        /// 出库任务类型
        /// </summary>
        public const string Export = "1";

        /// <summary>
        /// 移库任务类型
        /// </summary>
        public const string Move = "3";

        /// <summary>
        /// 判断是否为有效的WCS任务类型
        /// </summary>
        /// <param name="taskType">任务类型字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string taskType) =>
            taskType == Export || taskType == Move;
    }

    /// <summary>
    /// 库位锁定标志常量（PortEmptyTray 使用）
    /// </summary>
    public static class SlotLockFlag
    {
        /// <summary>
        /// 未锁定
        /// </summary>
        public const int Unlocked = 0;

        /// <summary>
        /// 已锁定
        /// </summary>
        public const int Locked = 1;
    }

    /// <summary>
    /// 库位深度常量（PortEmptyTray 使用）
    /// </summary>
    public static class SlotDepth
    {
        /// <summary>
        /// 深度1：靠近通道，可直接存取
        /// </summary>
        public const int Depth1 = 1;

        /// <summary>
        /// 深度2：远离通道，需要移库操作
        /// </summary>
        public const int Depth2 = 2;
    }

    /// <summary>
    /// 托盘类型标志（PortEmptyTray 使用）
    /// </summary>
    public static class TrayTypeFlag
    {
        /// <summary>
        /// 主托盘
        /// </summary>
        public const string MainTray = "0";

        /// <summary>
        /// 副托盘
        /// </summary>
        public const string SubTray = "1";
    }

    /// <summary>
    /// 任务状态常量
    /// </summary>
    public static class TaskStatus
    {
        /// <summary>
        /// 任务标志：已处理
        /// </summary>
        public const int Processed = 1;

        /// <summary>
        /// 任务标志：未处理
        /// </summary>
        public const int Unprocessed = 0;

        /// <summary>
        /// 执行状态：成功
        /// </summary>
        public const int Success = 1;

        /// <summary>
        /// 执行状态：失败
        /// </summary>
        public const int Failed = 0;
    }
}

#endregion

#region 内部类

/// <summary>
/// 空托候选对象（PortEmptyTray 使用）
/// </summary>
public class TrayCandidate
{
    /// <summary>
    /// 托盘主键ID
    /// </summary>
    public long TrayId { get; set; }

    /// <summary>
    /// 托盘编码
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// 库位主键ID
    /// </summary>
    public long SlotId { get; set; }

    /// <summary>
    /// 库位编码
    /// </summary>
    public string SlotCode { get; set; }
}

#endregion

/// <summary>
/// 空托申请入参
/// </summary>
public class EmptyTrayApplyInput
{
    /// <summary>
    /// 申请空托数量
    /// </summary>
    public int Num { get; set; }

    /// <summary>
    /// 下发的出库口
    /// </summary>
    public string ExportPort { get; set; }

    /// <summary>
    /// 仓库类型编码（例如：A/B/C）
    /// </summary>
    public string HouseCode { get; set; }
}

/// <summary>
/// 空托申请出参
/// </summary>
public class EmptyTrayApplyOutput
{
    /// <summary>
    /// 是否申请成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 提示信息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 下发给 WCS 的任务明细
    /// </summary>
    public IReadOnlyCollection<ExportLibraryDTO> Tasks { get; set; }

    /// <summary>
    /// WCS 原始返回内容
    /// </summary>
    public string WcsResponse { get; set; }
}

/// <summary>
/// 空托组盘入参
/// </summary>
public class EmptyTrayBindInput
{
    /// <summary>
    /// 托盘编码
    /// </summary>
    public string PalletCode { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 操作类型（add=绑定，del=解绑）
    /// </summary>
    public string ActionType { get; set; } = "add";

    /// <summary>
    /// 操作人员标识（保持与旧系统一致）
    /// </summary>
    public string UserId { get; set; } = "Wcs";
}

/// <summary>
/// 空托组盘出参
/// </summary>
public class EmptyTrayBindOutput
{
    /// <summary>
    /// 是否操作成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 提示信息
    /// </summary>
    public string Message { get; set; }
}

/// <summary>
/// WCS 通用返回结构
/// </summary>
public class WcsTaskResponse
{
    /// <summary>
    /// 结果状态码，约定 0 表示失败，其余视为成功
    /// </summary>
    public string stateCode { get; set; }

    /// <summary>
    /// 返回消息
    /// </summary>
    public string errMsg { get; set; }
}

/// <summary>
/// 空托组盘响应模型（KBackConfirm）
/// </summary>
public class KBackConfirmResponse
{
    /// <summary>
    /// 结果状态码，1 表示成功
    /// </summary>
    public int code { get; set; }

    /// <summary>
    /// 数量（保留字段，用于兼容旧系统）
    /// </summary>
    public int count { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string msg { get; set; }

    /// <summary>
    /// 响应数据（当前未使用）
    /// </summary>
    public string data { get; set; }
}

