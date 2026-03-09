// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application.Service.WmsPda.Dto;

/// <summary>
/// JC35【GetStockSoltBoxInfo】→ Admin.NET 入库组托查询参数。
/// </summary>
public class PdaStockSlotQueryInput
{
    /// <summary>
    /// 入库单号或入库单ID。
    /// </summary>
    public string ImportBillCode { get; set; }

    /// <summary>
    /// 托盘编码。
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// 物料条码（格式：明细ID-物料ID-批次-[可选：流水ID]）。
    /// </summary>
    public string MaterialBarcode { get; set; }

    /// <summary>
    /// 类型：1=按托盘查询，其它=按物料条码查询。
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// 单据类型。
    /// </summary>
    public string BillType { get; set; }

    /// <summary>
    /// 验收单号。
    /// </summary>
    public string InspectBillCode { get; set; }
}

/// <summary>
/// 入库组托查询返回结果。
/// </summary>
public class PdaStockSlotQueryOutput
{
    /// <summary>
    /// 是否成功。
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 提示消息。
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 结果集合。
    /// </summary>
    public List<PdaStockSlotItem> Items { get; set; } = new();
}

/// <summary>
/// 入库组托明细（托盘/箱码）。
/// </summary>
public class PdaStockSlotItem
{
    /// <summary>
    /// 箱托绑定记录主键（对应 WmsStockSlotBoxInfo.Id）。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 托盘编码。
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// 箱码。
    /// </summary>
    public string BoxCode { get; set; }

    /// <summary>
    /// 物料编码。
    /// </summary>
    public string MaterialCode { get; set; }

    /// <summary>
    /// 物料名称。
    /// </summary>
    public string MaterialName { get; set; }

    /// <summary>
    /// 批次。
    /// </summary>
    public string LotNo { get; set; }

    /// <summary>
    /// 入库单据明细主键（对应 WmsImportNotifyDetail.Id）。
    /// </summary>
    public long? ImportDetailId { get; set; }

    /// <summary>
    /// 当前箱数量。
    /// </summary>
    public decimal Qty { get; set; }

    /// <summary>
    /// 同批次累计数量。
    /// </summary>
    public decimal Quantity { get; set; }
}

/// <summary>
/// JC35【GetScanQty】→ Admin.NET 无箱码计算参数。
/// </summary>
public class PdaScanQtyInput
{
    /// <summary>
    /// 物料条码（格式：明细ID-物料ID-批次-[可选：流水ID]）。
    /// </summary>
    [Required(ErrorMessage = "物料条码不能为空！")]
    public string MaterialBarcode { get; set; }

    /// <summary>
    /// 入库单号或入库单ID。
    /// </summary>
    [Required(ErrorMessage = "入库单信息不能为空！")]
    public string ImportBillCode { get; set; }

    /// <summary>
    /// 托盘编码。
    /// </summary>
    [Required(ErrorMessage = "托盘编码不能为空！")]
    public string StockCode { get; set; }
}

/// <summary>
/// JC35【KBackConfirm】→ Admin.NET 空托盘组盘入参。
/// </summary>
public class PdaEmptyTrayBindInput
{
    /// <summary>
    /// 托盘编码。
    /// </summary>
    [Required(ErrorMessage = "托盘编码不能为空！")]
    public string PalletCode { get; set; }

    /// <summary>
    /// 数量。
    /// </summary>
    [Required(ErrorMessage = "数量不能为空！")]
    public int Quantity { get; set; }

    /// <summary>
    /// 操作类型：add=绑定，del=解绑。默认 add。
    /// </summary>
    public string ActionType { get; set; }

    /// <summary>
    /// 操作者。
    /// </summary>
    public string User { get; set; }
}

/// <summary>
/// JC35【TemporaryStorage】→ Admin.NET 暂存入库入参。
/// </summary>
public class PdaTemporaryStorageInput
{
    /// <summary>
    /// 主载具编码。
    /// </summary>
    [Required(ErrorMessage = "主载具不能为空！")]
    public string VehicleCode { get; set; }

    /// <summary>
    /// 副载具编码（可选）。
    /// </summary>
    public string VehicleSubCode { get; set; }

    /// <summary>
    /// 操作者。
    /// </summary>
    public string UserId { get; set; }
}

/// <summary>
/// JC35【StackingBindings】→ Admin.NET 叠箱绑定入参。
/// </summary>
public class PdaStackingBindingInput
{
    /// <summary>
    /// 主载具编码。
    /// </summary>
    [Required(ErrorMessage = "主载具不能为空！")]
    public string VehicleCode { get; set; }

    /// <summary>
    /// 副载具编码。
    /// </summary>
    [Required(ErrorMessage = "副载具不能为空！")]
    public string VehicleSubCode { get; set; }

    /// <summary>
    /// 操作者。
    /// </summary>
    public string UserId { get; set; }
}

/// <summary>
/// 无箱码绑定计算结果。
/// </summary>
public class PdaScanQtyOutput
{
    /// <summary>
    /// 是否成功。
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 提示消息。
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 推荐绑定数量。
    /// </summary>
    public decimal Quantity { get; set; }
}

/// <summary>
/// JC35【SaveBoxInfo】→ Admin.NET 无箱码组托保存入参。
/// </summary>
public class PdaSaveBoxInfoInput
{
    /// <summary>
    /// 托盘编码。
    /// </summary>
    [Required(ErrorMessage = "托盘编码不能为空！")]
    public string StockCode { get; set; }

    /// <summary>
    /// 物料主键（字符串形式）。
    /// </summary>
    [Required(ErrorMessage = "物料信息不能为空！")]
    public string MaterialId { get; set; }

    /// <summary>
    /// 批次号。
    /// </summary>
    [Required(ErrorMessage = "批次信息不能为空！")]
    public string LotNo { get; set; }

    /// <summary>
    /// 入库单主键。
    /// </summary>
    [Required(ErrorMessage = "入库单信息不能为空！")]
    public string ImportId { get; set; }

    /// <summary>
    /// 入库单明细主键。
    /// </summary>
    [Required(ErrorMessage = "入库单明细不能为空！")]
    public string ImportDetailId { get; set; }

    /// <summary>
    /// 绑定数量。
    /// </summary>
    [Required(ErrorMessage = "绑定数量不能为空！")]
    public decimal Qty { get; set; }

    /// <summary>
    /// 是否零箱标识（0 否；1 是）。
    /// </summary>
    public int? BulkTank { get; set; }

    /// <summary>
    /// 来源标识（人工码垛时有值）。
    /// </summary>
    public string Sources { get; set; }

    /// <summary>
    /// 操作者。
    /// </summary>
    public string User { get; set; }
}

/// <summary>
/// PDA 接口统一返回。
/// </summary>
public class PdaActionResult
{
    /// <summary>
    /// 是否成功。
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 提示消息。
    /// </summary>
    public string Message { get; set; }
}

/// <summary>
/// JC35【GetPalnoDetailInfo】→ Admin.NET 托盘明细查询入参。
/// </summary>
public class PdaPalletDetailInput
{
    /// <summary>
    /// 托盘编码。
    /// </summary>
    [Required(ErrorMessage = "托盘编码不能为空！")]
    public string PalletCode { get; set; }
}

/// <summary>
/// 托盘明细查询结果项。
/// </summary>
public class PdaPalletDetailItem
{
    public string LotNo { get; set; }

    public string BoxCode { get; set; }

    public string MaterialCode { get; set; }

    public string MaterialName { get; set; }

    public decimal Qty { get; set; }

    public decimal Quantity { get; set; }
}

/// <summary>
/// 托盘明细查询返回。
/// </summary>
public class PdaPalletDetailOutput : PdaActionResult
{
    public List<PdaPalletDetailItem> Items { get; set; } = new();
}

/// <summary>
/// JC35【DelStockSoltBoxInfo】→ Admin.NET 删除单个箱码入参。
/// </summary>
public class PdaDelBoxInput
{
    /// <summary>
    /// 箱码主键。
    /// </summary>
    [Required(ErrorMessage = "箱码主键不能为空！")]
    public string BoxId { get; set; }

    /// <summary>
    /// 操作者。
    /// </summary>
    public string User { get; set; }
}

/// <summary>
/// JC35【RemoveBoxHolds】→ Admin.NET 托盘解绑入参。
/// </summary>
public class PdaRemoveBoxInput
{
    /// <summary>
    /// 托盘编码（优先）。
    /// </summary>
    public string PalletCode { get; set; }

    /// <summary>
    /// 箱码（可选）。
    /// </summary>
    public string BoxCode { get; set; }

    /// <summary>
    /// 入库单主键（可选）。
    /// </summary>
    public string ImportId { get; set; }

    /// <summary>
    /// 操作者。
    /// </summary>
    public string User { get; set; }

    /// <summary>
    /// 业务类型：空/默认=标准入库，"ys"=验收。
    /// </summary>
    public string Type { get; set; }
}

/// <summary>
/// JC35【GetImportInfoByBoxCode】→ Admin.NET 扫码获取箱托信息入参。
/// </summary>
public class PdaBoxInfoInput
{
    /// <summary>
    /// 扫描的箱码。
    /// </summary>
    [Required(ErrorMessage = "箱码不能为空！")]
    public string BoxCode { get; set; }

    /// <summary>
    /// 载具编码（托盘号）。
    /// </summary>
    public string PalletCode { get; set; }

    /// <summary>
    /// 入库单主键或单号，"-1" 表示只校验箱码，不校验单据。
    /// </summary>
    public string ImportId { get; set; }

    /// <summary>
    /// 单据类型：空/默认=标准入库，"1"=验收，"3"=挑浆，"4"=挑浆新逻辑（暂不支持）。
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// 业务单号（用于二次校验箱码所属单据）。
    /// </summary>
    public string OrderNo { get; set; }
    /// <summary>
    /// 数量
    /// </summary>
    public string Qty { get; set; }

    /// <summary>
    /// 操作人。
    /// </summary>
    public string User { get; set; }
}

/// <summary>
/// JC35【BackConfirm】→ Admin.NET 绑定箱托关系入参。
/// </summary>
public class PdaBindBoxInput : PdaBoxInfoInput
{
    /// <summary>
    /// 血浆/物料重量（若无称重则默认 0.666）。
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// 剔除原因（兼容 JC35 剔除逻辑）。
    /// </summary>
    public string ReasonsForExcl { get; set; }

    /// <summary>
    /// 血浆拒收类型标识。
    /// </summary>
    public string RejectTypeId { get; set; }
}

/// <summary>
/// 扫码获取箱托信息返回结果。
/// </summary>
public class PdaBoxInfoOutput : PdaActionResult
{
    /// <summary>
    /// 处理阶段。
    /// </summary>
    public PdaBoxInfoState State { get; set; }

    /// <summary>
    /// 箱托明细。
    /// </summary>
    public PdaBoxInfoDetail Box { get; set; }

    /// <summary>
    /// 附加描述（例如提示需重新扫描）。
    /// </summary>
    public string ExtraMessage { get; set; }
}

/// <summary>
/// 箱托明细。
/// </summary>
public class PdaBoxInfoDetail
{
    /// <summary>
    /// 主键。
    /// </summary>
    public long BoxId { get; set; }

    /// <summary>
    /// 箱码。
    /// </summary>
    public string BoxCode { get; set; }

    /// <summary>
    /// 物料主键。
    /// </summary>
    public long? MaterialId { get; set; }

    /// <summary>
    /// 物料编码。
    /// </summary>
    public string MaterialCode { get; set; }
    /// <summary>
    /// 物料名称
    /// </summary>
    public string MaterialName { get; set; }

    /// <summary>
    /// 批次号。
    /// </summary>
    public string LotNo { get; set; }

    /// <summary>
    /// 数量。
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// 所属托盘。
    /// </summary>
    public string PalletCode { get; set; }

    /// <summary>
    /// 入库单主键。
    /// </summary>
    public long? ImportId { get; set; }

    /// <summary>
    /// 入库单据号。
    /// </summary>
    public string ImportBillCode { get; set; }

    /// <summary>
    /// 入库明细主键。
    /// </summary>
    public long? ImportDetailId { get; set; }

    /// <summary>
    /// 质检状态（0 待检、1 合格、2 不合格）。
    /// </summary>
    public int? InspectionStatus { get; set; }

    /// <summary>
    /// 是否挑浆（0 默认，1 挑浆）。
    /// </summary>
    public string PickingSlurry { get; set; }

    /// <summary>
    /// 检疫状态（1 满足、2 不满足、3 不合格）。
    /// </summary>
    public int? ExtractStatus { get; set; }

    /// <summary>
    /// ERP 原始二维码。
    /// </summary>
    public string QrCode { get; set; }

    /// <summary>
    /// 计划入库数量（来自入库明细）。
    /// </summary>
    public decimal? ImportQuantity { get; set; }

    /// <summary>
    /// 实际已组托数量（来自入库明细）。
    /// </summary>
    public decimal? ImportFactQuantity { get; set; }

    /// <summary>
    /// 入库执行标识（01 待执行、02 执行中、03 已完成）。
    /// </summary>
    public string ImportExecuteFlag { get; set; }
}

/// <summary>
/// 箱托信息处理阶段。
/// </summary>
public enum PdaBoxInfoState
{
    /// <summary>
    /// 未知/默认。
    /// </summary>
    None = 0,

    /// <summary>
    /// 需通过标签补齐箱托信息。
    /// </summary>
    RequireLabel = 1,

    /// <summary>
    /// 可直接绑定。
    /// </summary>
    Ready = 2,

    /// <summary>
    /// 数据异常，需重新扫描。
    /// </summary>
    Retry = 3
}
/// <summary>
/// 查询库存箱码信息
/// </summary>
/// <returns>库存箱码信息</returns>
public  class PdaStockSlotRaw
{
    /// <summary>
    /// 箱托绑定记录主键。
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    /// 库存编码。
    /// </summary>
    public string StockCode { get; set; } = string.Empty;
    /// <summary>
    /// 箱码。
    /// </summary>
    public string BoxCode { get; set; } = string.Empty;
    /// <summary>
    /// 物料编码。
    /// </summary>
    public string MaterialCode { get; set; } = string.Empty;
    /// <summary>
    /// 物料名称。
    /// </summary>
    public string MaterialName { get; set; } = string.Empty;
    /// <summary>
    /// 批次号。
    /// </summary>
    public string LotNo { get; set; } = string.Empty;
    /// <summary>
    /// 入库单据明细主键。
    /// </summary>
    public long? ImportDetailId { get; set; }
    /// <summary>
    /// 数量。
    /// </summary>
    public decimal Qty { get; set; }
}

/// <summary>
/// 查询库存箱码信息视图。
/// </summary>
public sealed class ViewStockSlotBoxInfoView
{
    /// <summary>
    /// 主键。
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    /// 箱码。
    /// </summary>
    public string? BoxCode { get; set; }
    /// <summary>
    /// 库存编码ID。
    /// </summary>
    public long? StockCodeId { get; set; }
    /// <summary>
    /// 库存编码。
    /// </summary>
    public string? StockCode { get; set; }
    public decimal? Qty { get; set; }
    /// <summary>
    /// 入库流水ID。
    /// </summary>
    public long? ImportOrderId { get; set; }
    /// <summary>
    /// 入库流水号。
    /// </summary>
    public string? ImportOrderNo { get; set; }
    /// <summary>
    /// 状态。
    /// </summary>
    public int? Status { get; set; }
    /// <summary>
    /// 箱层级。
    /// </summary>
    public int? BoxLevel { get; set; }
    public string? LotNo { get; set; }
    /// <summary>
    /// 是否删除。
    /// </summary>
    public bool IsDelete { get; set; }
    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreateTime { get; set; }
    /// <summary>
    /// 是否零箱标识。
    /// </summary>
    public int? BulkTank { get; set; }
    /// <summary>
    /// 生产日期。
    /// </summary>
    public DateTime? ProductionDate { get; set; }
    /// <summary>
    /// 有效期。
    /// </summary>
    public DateTime? ValidateDay { get; set; }
    /// <summary>
    /// 物料ID。
    /// </summary>
    public long? MaterialId { get; set; }
    /// <summary>
    /// 供应商ID。
    /// </summary>
    public long? SupplierId { get; set; }
    /// <summary>
    /// 制造商ID。
    /// </summary>
    public long? ManufacturerId { get; set; }
    /// <summary>
    /// 物料编码。
    /// </summary>
    public string? MaterialCode { get; set; }
    /// <summary>
    /// 物料名称。
    /// </summary>
    public string? MaterialName { get; set; }
    /// <summary>
    /// 入库明细ID。
    /// </summary>
    public long? ImportDetailId { get; set; }
    /// <summary>
    /// 入库执行标识。
    /// </summary>
    public string? ImportExecuteFlag { get; set; }
    /// <summary>
    /// 入库ID。
    /// </summary>
    public long? ImportId { get; set; }
    public decimal? ImportQuantity { get; set; }
    /// <summary>
    /// 入库实际数量。
    /// </summary>
    public decimal? ImportFactQuantity { get; set; }
    /// <summary>
    /// 入库单据号。
    /// </summary>
    public string? ImportBillCode { get; set; }
    /// <summary>
    /// 质检单据号。
    /// </summary>
    public string? InspectBillCode { get; set; }
    /// <summary>
    /// 提取单据号。
    /// </summary>
    public string? ExtractBillCode { get; set; }
    /// <summary>
    /// 是否采样箱。
    /// </summary>
    public int? IsSamplingBox { get; set; }
    /// <summary>
    /// 采样日期。
    /// </summary>
    public DateTime? SamplingDate { get; set; }
    /// <summary>
    /// 员工编码。
    /// </summary>
    public string? StaffCode { get; set; }
    /// <summary>
    /// 员工名称。
    /// </summary>
    public string? StaffName { get; set; }
    /// <summary>
    /// 重量。
    /// </summary>
    public decimal? Weight { get; set; }
    /// <summary>
    /// 剔除原因。
    /// </summary>
    public string? ReasonsForExcl { get; set; }
    /// <summary>
    /// 提取状态。
    /// </summary>
    public int? ExtractStatus { get; set; }
    /// <summary>
    /// 质检状态。
    /// </summary>
    public int? InspectionStatus { get; set; }
    /// <summary>
    /// 挑浆状态。
    /// </summary>
    public string? PickingSlurry { get; set; }
}
/// <summary>
/// 物料条码解析结果承载结构。
/// </summary>
public class ParsedBarcode
{
    /// <summary>
    /// 明细ID。
    /// </summary>
    public long? DetailId { get; set; }
    /// <summary>
    /// 明细ID原始值。
    /// </summary>
    public string DetailIdRaw { get; set; }
    /// <summary>
    /// 物料ID。
    /// </summary>
    public long? MaterialId { get; set; }
    /// <summary>
    /// 物料ID原始值。
    /// </summary>
    public string MaterialIdRaw { get; set; }
    /// <summary>
    /// 批次号。
    /// </summary>
    public string LotNo { get; set; }
    /// <summary>
    /// 入库流水ID。
    /// </summary>
    public long? OrderId { get; set; }
    /// <summary>
    /// 入库流水ID原始值。
    /// </summary>
    public string OrderIdRaw { get; set; }
}

/// <summary>
/// 批量入库确认输入参数
/// </summary>
public class PdaBatchImportConfirmInput
{
    /// <summary>
    /// 托盘编码
    /// </summary>
    [Required(ErrorMessage = "托盘编码不能为空！")]
    public string PalletCode { get; set; }

    /// <summary>
    /// 入库单ID
    /// </summary>
    public string ImportId { get; set; }

    /// <summary>
    /// 入库单号
    /// </summary>
    public string OrderNo { get; set; }

    /// <summary>
    /// 操作用户
    /// </summary>
    public string User { get; set; }

    /// <summary>
    /// 待绑定的新箱子列表
    /// </summary>
    public List<PdaPendingBoxItem> PendingBoxes { get; set; } = new();

    /// <summary>
    /// 已修改数量的箱子（箱码:新数量）
    /// </summary>
    public Dictionary<string, decimal> ModifiedBoxes { get; set; } = new();
}

/// <summary>
/// 待绑定箱子信息
/// </summary>
public class PdaPendingBoxItem
{
    /// <summary>
    /// 箱码
    /// </summary>
    public string BoxCode { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialCode { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    public string LotNo { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public decimal Qty { get; set; }

    /// <summary>
    /// 箱子ID（从服务器返回的）
    /// </summary>
    public long? BoxId { get; set; }

    /// <summary>
    /// 入库明细ID
    /// </summary>
    public long? ImportDetailId { get; set; }
}

/// <summary>
/// 批量入库确认输出结果
/// </summary>
public class PdaBatchImportConfirmOutput : PdaActionResult
{
    /// <summary>
    /// 成功绑定的箱子数量
    /// </summary>
    public int BindSuccessCount { get; set; }

    /// <summary>
    /// 成功更新数量的箱子数量
    /// </summary>
    public int UpdateSuccessCount { get; set; }

    /// <summary>
    /// 失败的箱子列表
    /// </summary>
    public List<PdaBatchFailedItem> FailedItems { get; set; } = new();
}

/// <summary>
/// 批量操作失败项
/// </summary>
public class PdaBatchFailedItem
{
    /// <summary>
    /// 箱码
    /// </summary>
    public string BoxCode { get; set; }

    /// <summary>
    /// 失败原因
    /// </summary>
    public string Reason { get; set; }
}


