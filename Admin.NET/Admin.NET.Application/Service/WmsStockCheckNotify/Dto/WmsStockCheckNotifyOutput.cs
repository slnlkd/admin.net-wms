// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Magicodes.ExporterAndImporter.Core;

namespace Admin.NET.Application;

/// <summary>
/// 盘点单据输出参数
/// </summary>
public class WmsStockCheckNotifyOutput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 盘库编号
    /// </summary>
    public string? CheckBillCode { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CheckDate { get; set; }

    /// <summary>
    /// 仓库Id
    /// </summary>
    public string? WarehouseId { get; set; }

    /// <summary>
    /// 仓库类型
    /// </summary>
    public string? WarehouseType { get; set; }

    /// <summary>
    /// 仓库名称
    /// </summary>
    public string? WarehouseName { get; set; }

    /// <summary>
    /// 执行标志（0待执行、1正在执行、2待调整、3执行完毕）
    /// </summary>
    public int? ExecuteFlag { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    public int? IsDelete { get; set; }

    /// <summary>
    /// 盘库备注
    /// </summary>
    public string? CheckRemark { get; set; }

    /// <summary>
    /// 添加时间
    /// </summary>
    public DateTime? AddDate { get; set; }

    /// <summary>
    /// 创建者Id
    /// </summary>
    public long? CreateUserId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// 修改者Id
    /// </summary>
    public long? UpdateUserId { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }

    /// <summary>
    /// 明细列表
    /// </summary>
    public List<WmsStockCheckNotifyDetailOutput>? List { get; set; }
}

/// <summary>
/// 盘点单明细输出参数
/// </summary>
public class WmsStockCheckNotifyDetailOutput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 盘点单号
    /// </summary>
    public string? CheckBillCode { get; set; }

    /// <summary>
    /// 库存主键
    /// </summary>
    public long? CheckStockId { get; set; }

    /// <summary>
    /// 储位编码
    /// </summary>
    public string? StockSlot { get; set; }

    /// <summary>
    /// 托盘编码
    /// </summary>
    public string? StockCode { get; set; }

    /// <summary>
    /// 计划数量
    /// </summary>
    public decimal? StockQuantity { get; set; }

    /// <summary>
    /// 批次号
    /// </summary>
    public string? StockLotNo { get; set; }

    /// <summary>
    /// 执行标志（0待执行、1正在执行、2已完成）
    /// </summary>
    public int? ExecuteFlag { get; set; }

    /// <summary>
    /// 物料主键
    /// </summary>
    public long? MaterialId { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterialName { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    public string? MaterialStandard { get; set; }


    /// <summary>
    /// 计量单位
    /// </summary>
    public string? UnitName { get; set; }
    
    /// <summary>
    /// 仓库主键
    /// </summary>
    public string? WarehouseId { get; set; }

    /// <summary>
    /// 质检状态
    /// </summary>
    public int? InspectionStatus { get; set; }

    /// <summary>
    /// 实盘数量
    /// </summary>
    public decimal? RealQuantity { get; set; }

    /// <summary>
    /// 盘点结果：0 正常、1 盘亏、2 盘盈
    /// </summary>
    public int? CheckResult { get; set; }

    /// <summary>
    /// 业务时间
    /// </summary>
    public DateTime? AddDate { get; set; }

    /// <summary>
    /// 盘点任务Id
    /// </summary>
    public string? TaskId { get; set; }

    /// <summary>
    /// 盘点任务编号
    /// </summary>
    public string? CheckTaskNo { get; set; }

    /// <summary>
    /// 盘点备注
    /// </summary>
    public string? CheckRemark { get; set; }
}

/// <summary>
/// 库存输出参数
/// </summary>
public class WmsStockSlotOutput
{
    /// <summary>
    /// 主键Id（载具库存ID，用于盘点单的CheckStockId）
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 载具库存ID（盘点单CheckStockId）
    /// </summary>
    public long CheckStockId { get; set; }

    /// <summary>
    /// 仓库Id
    /// </summary>
    public string? WarehouseId { get; set; }

    /// <summary>
    /// 巷道Id
    /// </summary>
    public string? LanewayId { get; set; }

    /// <summary>
    /// 批次号
    /// </summary>
    public string? LotNo { get; set; }

    public long MaterialId { get; set; }
    
    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterialName { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    public string? MaterialStandard { get; set; }

    /// <summary>
    /// 质检状态
    /// </summary>
    public int? InspectionStatus { get; set; }

    /// <summary>
    /// 储位编码
    /// </summary>
    public string? SlotCode { get; set; }

    /// <summary>
    /// 托盘编码
    /// </summary>
    public string? StockCode { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    public decimal? StockQuantity { get; set; }
}

/// <summary>
/// 盘点任务输出参数
/// </summary>
public class WmsStockCheckTaskOutput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 盘库任务号
    /// </summary>
    public string? CheckTaskNo { get; set; }

    /// <summary>
    /// 发送方
    /// </summary>
    public string? Sender { get; set; }

    /// <summary>
    /// 接收方
    /// </summary>
    public string? Receiver { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public int? IsSuccess { get; set; }

    /// <summary>
    /// 信息
    /// </summary>
    public string? Information { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime? SendDate { get; set; }

    /// <summary>
    /// 返回时间
    /// </summary>
    public DateTime? BackDate { get; set; }

    /// <summary>
    /// 消息时间
    /// </summary>
    public string? MessageDate { get; set; }

    /// <summary>
    /// 托盘条码
    /// </summary>
    public string? StockCode { get; set; }

    /// <summary>
    /// 描述信息
    /// </summary>
    public string? Msg { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? FinishDate { get; set; }

    /// <summary>
    /// 出库流水号
    /// </summary>
    public string? WarehouseId { get; set; }

    /// <summary>
    /// 任务状态（0未开始；1执行中；2执行完成；3手动完成；4手动取消）
    /// </summary>
    public int? CheckTaskFlag { get; set; }

    /// <summary>
    /// 盘库单据明细Id
    /// </summary>
    public string? CheckNotifyId { get; set; }

    /// <summary>
    /// 盘库单号
    /// </summary>
    public string? CheckBillCode { get; set; }

    /// <summary>
    /// 起始位置
    /// </summary>
    public string? StartLocation { get; set; }

    /// <summary>
    /// 目标位置
    /// </summary>
    public string? EndLocation { get; set; }

    /// <summary>
    /// 任务类型（1出库；2出库移库）
    /// </summary>
    public int? TaskType { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    public int? IsDelete { get; set; }
}

/// <summary>
/// 盘点单据数据导入模板实体
/// </summary>
public class ExportWmsStockCheckNotifyOutput : ImportWmsStockCheckNotifyInput
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public override string Error { get; set; }
}
