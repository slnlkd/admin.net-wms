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

namespace Admin.NET.Application.Service.WmsPort.Dto;

#region 常量定义

/// <summary>
/// 入库单据下发业务常量（PortCreateOrder 使用）
/// </summary>
public static class ImportOrderConstants
{
    /// <summary>
    /// 执行标志常量（ImportExecuteFlag字段）
    /// </summary>
    public static class ExecuteFlag
    {
        public const string Invalid = "-1";     // 无效：单据已作废或无效
        public const string Pending = "01";     // 待执行：单据已创建，等待下发
        public const string Processing = "02";  // 执行中：已下发给WCS，正在执行
        public const string Completed = "03";   // 已完成：WCS已执行完成
    }

    /// <summary>
    /// 返回状态码（WCS响应代码）
    /// </summary>
    public static class ResponseCode
    {
        public const string Success = "1";      // 成功：WCS接受并处理单据
        public const string Failure = "0";      // 失败：WCS拒绝单据或处理失败
    }

    /// <summary>
    /// 任务状态常量（ImportTaskStatus字段）
    /// </summary>
    public static class TaskStatus
    {
        public const int Pending = 0;           // 待下发
        public const int Issued = 1;            // 已下发
        public const int Completed = 2;         // 已完成
    }

    /// <summary>
    /// 物料状态常量（MaterialStatus字段）
    /// </summary>
    public static class MaterialStatus
    {
        public const int ExemptFromInspection = 1;  // 免检物料
        public const int RequireInspection = 0;     // 需要质检
    }

    /// <summary>
    /// 标签状态常量（LabelingStatus字段）
    /// </summary>
    public static class LabelingStatus
    {
        public const string NoLabel = "0";      // 不贴标
        public const string WithLabel = "1";    // 贴标
    }

    /// <summary>
    /// 物料箱规验证常量
    /// </summary>
    public static class BoxQuantity
    {
        public const string Zero = "0";         // 箱规为0（无效）
    }
}

/// <summary>
/// 任务反馈业务常量（PortTaskFeedback 使用）
/// </summary>
public static class TaskFeedbackConstants
{
    /// <summary>
    /// 任务前缀常量（用于识别任务类型）
    /// </summary>
    public static class TaskPrefix
    {
        public const string Import = "RK";      // 入库任务（Ruku）
        public const string Export = "CK";      // 出库任务（Chuku）
        public const string Move = "YK";        // 移库任务（Yiku）
        public const string StockCheck = "PK";  // 盘库任务（Panku）
    }

    /// <summary>
    /// 任务状态代码常量
    /// </summary>
    public static class TaskCode
    {
        public const string Complete = "1";     // 完成
        public const string Cancel = "2";       // 取消
    }

    /// <summary>
    /// 任务类型常量（TaskType字段）
    /// </summary>
    public static class TaskTypeCode
    {
        public const string Import = "0";       // 入库
        public const string Export = "1";       // 出库
        public const string Move = "3";         // 移库
        public const string ExportSpecial = "4"; // 出库特殊类型
    }

    /// <summary>
    /// 执行标志常量（ImportExecuteFlag字段）
    /// </summary>
    public static class ExecuteFlag
    {
        public const string Invalid = "-1";     // 无效
        public const string Pending = "01";     // 待执行
        public const string Processing = "02";  // 执行中
        public const string Completed = "03";   // 已完成
    }

    /// <summary>
    /// 任务状态常量（Status字段）
    /// </summary>
    public static class TaskStatus
    {
        public const int Pending = 0;           // 待执行
        public const int Processing = 1;        // 执行中
        public const int Completed = 2;         // 已完成
        public const int Cancelled = 3;         // 已取消
    }

    /// <summary>
    /// 库位状态常量
    /// </summary>
    public static class SlotStatus
    {
        public const int Empty = 0;             // 空储位
        public const int HasItems = 1;          // 有物品
        public const int Importing = 2;         // 正在入库
        public const int Exporting = 3;         // 正在出库
        public const int MovingIn = 4;          // 正在移入
        public const int MovingOut = 5;         // 正在移出
        public const int EmptyTray = 6;         // 空托盘
    }

    /// <summary>
    /// 箱码状态常量
    /// </summary>
    public static class BoxStatus
    {
        public const int Pending = 0;           // 待入库
        public const int Importing = 1;         // 正在入库
        public const int Imported = 2;          // 已入库
        public const int Exported = 3;          // 已出库
    }

}

/// <summary>
/// 入库申请业务常量（PortImportApply 使用）
/// </summary>
public static class ImportApplyConstants
{
    /// <summary>
    /// 执行标志常量
    /// </summary>
    public static class ExecuteFlag
    {
        /// <summary>无效状态</summary>
        public const string Invalid = "-1";

        /// <summary>待执行状态</summary>
        public const string Pending = "01";

        /// <summary>执行中状态</summary>
        public const string Processing = "02";

        /// <summary>已完成状态</summary>
        public const string Completed = "03";
    }

    /// <summary>
    /// 任务状态常量
    /// </summary>
    public static class TaskStatus
    {
        /// <summary>已完成</summary>
        public const int Completed = 2;

        /// <summary>手动完成</summary>
        public const int ManualCompleted = 3;

        /// <summary>已取消</summary>
        public const int Cancelled = 4;
    }

    /// <summary>
    /// 库位状态常量
    /// </summary>
    public static class SlotStatus
    {
        /// <summary>空</summary>
        public const int Empty = 0;

        /// <summary>有物品</summary>
        public const int HasItems = 1;

        /// <summary>正在入库</summary>
        public const int Importing = 2;

        /// <summary>正在出库</summary>
        public const int Exporting = 3;

        /// <summary>正在移入</summary>
        public const int MovingIn = 4;

        /// <summary>正在移出</summary>
        public const int MovingOut = 5;

        /// <summary>空托盘</summary>
        public const int EmptyTray = 6;
    }

    /// <summary>
    /// 仓库类型常量
    /// </summary>
    public static class WarehouseType
    {
        /// <summary>立体库</summary>
        public const string StereoWarehouse = "01";

        /// <summary>平库</summary>
        public const string FlatWarehouse = "05";
    }

    /// <summary>
    /// 任务模式常量
    /// </summary>
    public static class TaskMode
    {
        /// <summary>入库</summary>
        public const int Import = 1;

        /// <summary>出库</summary>
        public const int Export = 2;

        /// <summary>移库</summary>
        public const int Move = 3;

        /// <summary>盘库</summary>
        public const int StockCheck = 4;
    }

    /// <summary>
    /// 托盘状态常量
    /// </summary>
    public static class StockCodeStatus
    {
        /// <summary>未使用</summary>
        public const int Unused = 0;

        /// <summary>使用中</summary>
        public const int InUse = 1;
    }

    /// <summary>
    /// 托盘类型常量
    /// </summary>
    public static class StockCodeType
    {
        /// <summary>仓储笼</summary>
        public const int StorageCage = 0;

        /// <summary>质检托盘</summary>
        public const int QualityCheckTray = 2;
    }

    /// <summary>
    /// 回库状态常量
    /// </summary>
    public static class BackToWarehouseState
    {
        /// <summary>等待回库</summary>
        public const int WaitingReturn = 1;

        /// <summary>正在回库</summary>
        public const int Returning = 2;
    }

    /// <summary>
    /// 库位深度常量
    /// </summary>
    public static class SlotDepth
    {
        /// <summary>外层（深度1）</summary>
        public const int Outside = 1;

        /// <summary>内层（深度2）</summary>
        public const int Inside = 2;
    }

    /// <summary>
    /// 业务常量
    /// </summary>
    public static class BusinessConstants
    {
        /// <summary>错误任务号标记（兼容JC20逻辑）</summary>
        public const string ErrorTaskNo = "0";

        /// <summary>库位结果分隔符</summary>
        public const string SlotResultSeparator = "&";

        /// <summary>回库标识</summary>
        public const string ReturnFlowIdentifier = "huiku";
    }
}

#endregion

public class ViewWmsStockSlotBoxInfo
{
    public long? ImportId { get; set; }
    public string? MaterialId { get; set; }
    public long? MaterialLongId { get; set; }
    public long? SupplierId { get; set; }
    public long? ManufacturerId { get; set; }
    public string? LotNo { get; set; }
    public string? InspectBillCode { get; set; } = string.Empty;
    public long? ImportDetailId { get; set; }
    public string? StockCode { get; set; }
    public string? BoxCode { get; set; }
    public string? StaffCode { get; set; }
    public string? StaffName { get; set; }
    public decimal? Weight { get; set; }
    public int? InspectionStatus { get; set; }
}
public class ImportNotifyDetail
{
    public long Id { get; set; }

    public string ReceivingDock { get; set; }

    public string NetWeight { get; set; }
}
public class WCSOrderDto
{
    /// <summary>
    /// 入库口
    /// </summary>
    public string EquipmentNo { get; set; }
    /// <summary>
    /// 皮重
    /// </summary>
    public string TareQty { get; set; }
    /// <summary>
    /// 订单号
    /// </summary>
    public string BillCode { get; set; }
    /// <summary>
    /// 所属仓库
    /// </summary>
    public string HouseCode { get; set; }
    /// <summary>
    /// 箱数量
    /// </summary>
    public int WholeBoxNum { get; set; }

    /// <summary>
    /// 箱总数
    /// </summary>
    public double BoxQty { get; set; }
    /// <summary>
    /// 质检箱数
    /// </summary>
    public double QTBoxQty { get; set; }
    /// <summary>
    /// 数量
    /// </summary>
    public decimal Qty { get; set; }
    ///<summary>
    ///物料编码
    ///</summary>
    public string GoodCode { get; set; }
    ///<summary>
    ///物料名称
    ///</summary>
    public string GoodName { get; set; }
    ///<summary>
    ///生产日期
    ///</summary>
    public string ProductionDate { get; set; }

    ///<summary>
    ///有效期
    ///</summary>
    public string ValidateDay { get; set; }
    ///<summary>
    ///批次信息
    ///</summary>
    public string LotNo { get; set; }
    /// <summary>
    /// 载具类型
    /// </summary>
    public string VehicleType { get; set; }
    ///<summary>
    ///是否贴标,0不贴标，1贴标
    ///</summary>
    public string LabelingStatus { get; set; }
    public List<WmsOrderItem> WmsOrderItems { get; set; }
}
public class WmsOrderItem
{
    public string SerialCode { get; set; }
    public string OderId { get; set; }
    public string MaterialCode { get; set; }
    public string ProductionDate { get; set; }
    public string ValidateDay { get; set; }
    public string Qty { get; set; }
    public string LotNo { get; set; }
    public string QRCode { get; set; }
    public string RFIDCode { get; set; }
}

/// <summary>
/// 入库单据下发返回结果
/// </summary>
public class CreateOrderOutput
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 提示信息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// WCS订单数据
    /// </summary>
    public WCSOrderDto Data { get; set; }
}

/// <summary>
/// 出库口配置类（用于WCS API地址配置）
/// </summary>
public class ExprotPort
{
    /// <summary>
    /// WCS接口地址
    /// </summary>
    public string WCS { get; set; }
}
