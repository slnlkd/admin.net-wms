// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using System.ComponentModel.DataAnnotations;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

namespace Admin.NET.Application;

/// <summary>
/// 盘点单据基础输入参数
/// </summary>
public class WmsStockCheckNotifyBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }

    /// <summary>
    /// 盘库编号
    /// </summary>
    public virtual string? CheckBillCode { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime? CheckDate { get; set; }

    /// <summary>
    /// 仓库Id
    /// </summary>
    public virtual string? WarehouseId { get; set; }

    /// <summary>
    /// 执行标志（0待执行、1正在执行、2待调整、3执行完毕）
    /// </summary>
    public virtual int? ExecuteFlag { get; set; }

    /// <summary>
    /// 盘库备注
    /// </summary>
    public virtual string? CheckRemark { get; set; }

    /// <summary>
    /// 添加时间
    /// </summary>
    public virtual DateTime? AddDate { get; set; }
}

/// <summary>
/// 盘点单据分页查询输入参数
/// </summary>
public class PageWmsStockCheckNotifyInput : BasePageInput
{
    /// <summary>
    /// 盘库编号
    /// </summary>
    public string? CheckBillCode { get; set; }

    /// <summary>
    /// 执行标志
    /// </summary>
    public int? ExecuteFlag { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 仓库Id
    /// </summary>
    public string? WarehouseId { get; set; }

    /// <summary>
    /// 选中主键列表
    /// </summary>
    public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 获取未完成盘点单输入参数
/// </summary>
public class GetNotFinishWmsStockCheckNotifyInput
{
    /// <summary>
    /// 盘库编号
    /// </summary>
    public string? BillCode { get; set; }

    /// <summary>
    /// 执行标志
    /// </summary>
    public int? ExecuteFlag { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// 获取盘点单明细列表输入参数
/// </summary>
public class GetStockCheckDetailListInput : BasePageInput
{
    /// <summary>
    /// 盘库编号
    /// </summary>
    public string? BillCode { get; set; }

    /// <summary>
    /// 托盘条码
    /// </summary>
    public string? StockCode { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterialName { get; set; }
}

/// <summary>
/// 获取库存列表输入参数
/// </summary>
public class GetStockSlotInput
{
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

    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterialName { get; set; }

    /// <summary>
    /// 储位编码
    /// </summary>
    public string? SlotCode { get; set; }

    /// <summary>
    /// 托盘条码
    /// </summary>
    public string? StockCode { get; set; }

    public List<string> allAddStockCode { get; set; }
    public List<long> allAddCheckStockId { get; set; }
}

/// <summary>
/// 盘点单据增加输入参数
/// </summary>
public class AddWmsStockCheckNotifyInput
{
    /// <summary>
    /// 仓库Id
    /// </summary>
    [Required(ErrorMessage = "仓库Id不能为空")]
    public string? WarehouseId { get; set; }

    /// <summary>
    /// 盘库备注
    /// </summary>
    public string? CheckRemark { get; set; }

    /// <summary>
    /// 盘点明细列表
    /// </summary>
    [Required(ErrorMessage = "盘点明细列表不能为空")]
    public List<AddWmsStockCheckNotifyDetailInput> List { get; set; }
}

/// <summary>
/// 盘点单明细增加输入参数
/// </summary>
public class AddWmsStockCheckNotifyDetailInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long? Id { get; set; }

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
    [Required(ErrorMessage = "储位编码不能为空")]
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
    /// 物料主键
    /// </summary>
    public long? MaterialId { get; set; }

    /// <summary>
    /// 仓库主键
    /// </summary>
    public string? WarehouseId { get; set; }

    /// <summary>
    /// 质检状态
    /// </summary>
    public int? InspectionStatus { get; set; }

    /// <summary>
    /// 执行标志
    /// </summary>
    public int? ExecuteFlag { get; set; }

    /// <summary>
    /// 添加时间
    /// </summary>
    public DateTime? AddDate { get; set; }

    /// <summary>
    /// 盘点结果
    /// </summary>
    public int? CheckResult { get; set; }
}

/// <summary>
/// 根据ID获取盘点单输入参数
/// </summary>
public class GetStockCheckByIdInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
}

/// <summary>
/// 盘库出库输入参数
/// </summary>
public class StockCheckIssueOutBoundInput
{
    /// <summary>
    /// 盘库编号
    /// </summary>
    [Required(ErrorMessage = "盘库编号不能为空")]
    public string? BillCode { get; set; }

    /// <summary>
    /// 仓库Id
    /// </summary>
    [Required(ErrorMessage = "仓库Id不能为空")]
    public string? WarehouseId { get; set; }
}

/// <summary>
/// 创建盘点任务输入参数
/// </summary>
public class CreateStockCheckTaskInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }

    /// <summary>
    /// 盘库编号
    /// </summary>
    public string? BillCode { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    public long? UserId { get; set; }
}

/// <summary>
/// 库存盘点-库存数量调整输入参数
/// </summary>
public class CancelStockEditInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }

    /// <summary>
    /// 盘库编号
    /// </summary>
    //[Required(ErrorMessage = "盘库编号不能为空")]
    public string? BillCode { get; set; }
}

/// <summary>
/// 获取盘点任务列表输入参数
/// </summary>
public class GetCheckTaskListInput
{
    /// <summary>
    /// 盘库任务号
    /// </summary>
    public string? CheckTaskNo { get; set; }

    /// <summary>
    /// 盘库单号
    /// </summary>
    public string? CheckBillCode { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    public int? CheckTaskFlag { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// 盘点单据删除输入参数
/// </summary>
public class DeleteWmsStockCheckNotifyInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
}

/// <summary>
/// 盘点单据更新输入参数
/// </summary>
public class UpdateWmsStockCheckNotifyInput : WmsStockCheckNotifyBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public override long? Id { get; set; }

    /// <summary>
    /// 盘点明细列表
    /// </summary>
    public List<AddWmsStockCheckNotifyDetailInput>? List { get; set; }
}

/// <summary>
/// 盘点单据主键查询输入参数
/// </summary>
public class QueryByIdWmsStockCheckNotifyInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
}

/// <summary>
/// 盘点单据数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsStockCheckNotifyInput : BaseImportInput
{
    /// <summary>
    /// 盘库编号
    /// </summary>
    [ImporterHeader(Name = "盘库编号")]
    [ExporterHeader("盘库编号", Format = "", Width = 25, IsBold = true)]
    public string? CheckBillCode { get; set; }

    /// <summary>
    /// 仓库Id
    /// </summary>
    [ImporterHeader(Name = "仓库Id")]
    [ExporterHeader("仓库Id", Format = "", Width = 25, IsBold = true)]
    public string? WarehouseId { get; set; }

    /// <summary>
    /// 执行标志
    /// </summary>
    [ImporterHeader(Name = "执行标志")]
    [ExporterHeader("执行标志", Format = "", Width = 25, IsBold = true)]
    public int? ExecuteFlag { get; set; }

    /// <summary>
    /// 盘库备注
    /// </summary>
    [ImporterHeader(Name = "盘库备注")]
    [ExporterHeader("盘库备注", Format = "", Width = 25, IsBold = true)]
    public string? CheckRemark { get; set; }
}
