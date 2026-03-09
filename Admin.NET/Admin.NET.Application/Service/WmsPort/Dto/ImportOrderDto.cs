// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Application.Service.WmsPort.Dto;

/// <summary>
/// 入库申请输入参数
/// </summary>
public class ImportApplyInput
{
    /// <summary>
    /// 仓库编码
    /// 说明：用于标识入库目标仓库（如立体库、平库等）
    /// </summary>
    public string HouseCode { get; set; }

    /// <summary>
    /// 托盘码
    /// 说明：要入库的托盘唯一标识码，用于绑定托盘上的物料和箱码信息
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// 巷道编码
    /// 说明：指定入库的巷道编码，如果不指定则系统自动分配合适的巷道
    /// 适用场景：立体库、平库等需要明确巷道位置的仓库
    /// </summary>
    public string LaneWayCode { get; set; }

    /// <summary>
    /// 起始位置
    /// 说明：托盘当前所在位置或入库起点位置
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// 仓库名称
    /// 说明：仓库的业务名称，用于日志记录和业务描述
    /// </summary>
    public string WarehouseName { get; set; }

    /// <summary>
    /// 货位类型
    /// 说明：指定入库到哪种类型的货位
    /// 取值：3-低货位（适用于较低层级的存储位置），4-高货位（适用于较高层级的存储位置）
    /// 适用场景：立体库、平库等具有不同层高的仓库
    /// </summary>
    public string Type { get; set; }
}

/// <summary>
/// 二次入库申请输入参数
/// </summary>
public class ImportApply2Input
{
    /// <summary>
    /// 托盘码
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// AB面
    /// </summary>
    public string IsAB { get; set; }

    /// <summary>
    /// 巷道ID
    /// </summary>
    public string LaneWayId { get; set; }

    /// <summary>
    /// 储位地址
    /// </summary>
    public string SlotCode { get; set; }
}

/// <summary>
/// WCS任务模式输出
/// </summary>
public class WcsTaskModeOutput
{
    /// <summary>
    /// 任务号
    /// </summary>
    public string TaskNo { get; set; }

    /// <summary>
    /// 任务类型 (1:入库 3:移库 5:出库)
    /// </summary>
    public int TaskMode { get; set; }

    /// <summary>
    /// 托盘号
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// 开始储位号
    /// </summary>
    public string TaskBegin { get; set; }

    /// <summary>
    /// 结束储位号
    /// </summary>
    public string TaskEnd { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string GoodsCode { get; set; }

    /// <summary>
    /// 入库数量
    /// </summary>
    public decimal GoodsQTY { get; set; }

    /// <summary>
    /// AB面
    /// </summary>
    public string IsAB { get; set; }

    /// <summary>
    /// 巷道ID
    /// </summary>
    public string LaneWayId { get; set; }
}

/// <summary>
/// 入库申请响应结果
/// </summary>
public class ImportApplyOutput
{
    /// <summary>
    /// 状态码 (0:失败 1:成功)
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 数据条数
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 返回消息
    /// </summary>
    public string Msg { get; set; }

    /// <summary>
    /// 任务数据列表
    /// </summary>
    public List<WcsTaskModeOutput> Data { get; set; }
}

/// <summary>
/// 任务反馈输入参数
/// </summary>
public class TaskFeedbackInput
{
    /// <summary>
    /// 任务号
    /// 说明：入库申请时返回的任务号
    /// </summary>
    public string TaskNo { get; set; }

    /// <summary>
    /// 状态码
    /// 说明：1=完成，2=取消
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 任务类型
    /// 说明：0=入库，1=出库，3=移库
    /// </summary>
    public string TaskType { get; set; }

    /// <summary>
    /// 托盘码（载具码）
    /// 说明：执行任务的托盘编码
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// 目标库位
    /// 说明：任务执行的目标库位（入库时为TaskEnd，出库时为TaskBegin）
    /// </summary>
    public string TaskEnd { get; set; }

    /// <summary>
    /// 起始库位
    /// 说明：任务执行的起始库位（移库时为TaskBegin）
    /// </summary>
    public string TaskBegin { get; set; }
}

/// <summary>
/// 任务反馈输出结果
/// </summary>
public class TaskFeedbackOutput
{
    /// <summary>
    /// 状态码 (0:失败 1:成功)
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 数据条数
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 返回消息
    /// </summary>
    public string Msg { get; set; }

    /// <summary>
    /// 返回数据
    /// </summary>
    public string Data { get; set; }
}

