// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application.Service.WmsPda.Dto;

/// <summary>
/// JC35【GetWmsStockCheckList】→ Admin.NET 盘点单列表查询参数。
/// </summary>
public class PdaStockCheckListInput
{
    /// <summary>
    /// 仓库主键，可空代表查询全部。
    /// </summary>
    public string WarehouseId { get; set; }
}

/// <summary>
/// 盘点单列表输出项。
/// </summary>
public class PdaStockCheckOrderItem
{
    /// <summary>
    /// 主键。
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 盘点单号。
    /// </summary>
    public string CheckBillCode { get; set; }

    /// <summary>
    /// 仓库主键。
    /// </summary>
    public string WarehouseId { get; set; }

    /// <summary>
    /// 执行标志。
    /// </summary>
    public int ExecuteFlag { get; set; }

    /// <summary>
    /// 盘点单创建时间。
    /// </summary>
    public DateTime? CheckDate { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    public string Remark { get; set; }
}

/// <summary>
/// JC35【GetStockCheckInfo】→ Admin.NET 盘点箱码查询参数。
/// </summary>
public class PdaStockCheckInfoInput
{   /// <summary>
    /// 盘点单据标识（支持 Id 或 CheckBillCode）。
    /// </summary>

    public string CheckBillCode { get; set; }
    /// <summary>
    /// 托盘编码。
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// 箱码，可空。
    /// </summary>
    public string BoxCode { get; set; }

    /// <summary>
    /// 操作类型：1=确认扫描，其它或空=仅查询。
    /// </summary>
    public string Status { get; set; }
}

/// <summary>
/// 盘点箱码输出项。
/// </summary>
public class PdaStockCheckBoxItem
{
    /// <summary>
    /// 主键。
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// 箱码。
    /// </summary>
    public string BoxCode { get; set; }
    /// <summary>
    /// 托盘编码。
    /// </summary>
    public string StockCode { get; set; }
    /// <summary>
    /// 物料ID。
    /// </summary>
    public string MaterialId { get; set; }
    /// <summary>
    /// 物料编码。
    /// </summary>
    public string MaterialCode { get; set; }
    /// <summary>
    /// 物料名称。
    /// </summary>
    public string MaterialName { get; set; }
    /// <summary>
    /// 批次号。
    /// </summary>
    public string LotNo { get; set; }
    /// <summary>
    /// 盘点前数量。
    /// </summary>
    public decimal? Qty { get; set; }
    /// <summary>
    /// 盘点实际数量。
    /// </summary>
    public decimal? RealQuantity { get; set; }
    /// <summary>
    /// 生产日期。
    /// </summary>
    public DateTime? ProductionDate { get; set; }
    /// <summary>
    /// 有效期。
    /// </summary>
    public DateTime? ValidateDay { get; set; }
    /// <summary>
    /// 检验状态。
    /// </summary>
    public int? InspectionStatus { get; set; }
    /// <summary>
    /// 提取状态。
    /// </summary>
    public int? ExtractStatus { get; set; }
    /// <summary>
    /// 采样日期。
    /// </summary>
    public string SamplingDate { get; set; }
    /// <summary>
    /// 员工编码。
    /// </summary>
    public string StaffCode { get; set; }
    /// <summary>
    /// 员工名称。
    /// </summary>
    public string StaffName { get; set; }
    /// <summary>
    /// 盘点单据标识。
    /// </summary>
    public string StockCheckId { get; set; }
    /// <summary>
    /// 状态。
    /// </summary>
    public int? Status { get; set; }
    /// <summary>
    /// 状态。
    /// </summary>
    public int? State { get; set; }
    /// <summary>
    /// 盘点结果。
    /// </summary>
    public int? CheckResult { get; set; }
    /// <summary>
    /// 重量。
    /// </summary>
    public decimal? Weight { get; set; }
    /// <summary>
    /// 拣选状态。
    /// </summary>
    public int? PickingSlurry { get; set; }
    /// <summary>
    /// 客户ID。
    /// </summary>
    public string CustomerId { get; set; }
    /// <summary>
    /// 奇异标记。
    /// </summary>
    public string OddMarking { get; set; }
    /// <summary>
    /// 拣选数量。
    /// </summary>
    public int? OutQty { get; set; }
}

/// <summary>
/// JC35【UpdateStockCheckInfo】→ Admin.NET 盘点结果提交参数。
/// </summary>
public class PdaStockCheckUpdateInput
{
    /// <summary>
    /// 盘点单据标识（支持 Id 或 CheckBillCode）。
    /// </summary>
    [Required(ErrorMessage = "盘点单据标识不能为空！")]
    public string BillCode { get; set; }

    /// <summary>
    /// 托盘编码。
    /// </summary>
    [Required(ErrorMessage = "托盘编码不能为空！")]
    public string StockCode { get; set; }

    /// <summary>
    /// 批次号（兼容旧结构，可选）。
    /// </summary>
    public string LotNo { get; set; }

    /// <summary>
    /// 盘点前数量（兼容旧结构，可选）。
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// 盘点实际数量（兼容旧结构，可选）。
    /// </summary>
    public decimal? RealQuantity { get; set; }

    /// <summary>
    /// 盘点箱码明细。
    /// </summary>
    [Required(ErrorMessage = "盘点明细不能为空！")]
    public List<PdaStockCheckUpdateOption> Options { get; set; } = new();
}

/// <summary>
/// 盘点结果提交明细项。
/// </summary>
public class PdaStockCheckUpdateOption
{
    /// <summary>
    /// 箱码，空代表无箱码托盘。
    /// </summary>
    public string BoxCode { get; set; }

    /// <summary>
    /// 托盘编码。
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// 批次号（兼容旧结构）。
    /// </summary>
    public string LotNo { get; set; }

    /// <summary>
    /// 盘点前数量（兼容旧结构）。
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// 实际数量。
    /// </summary>
    public decimal? RealQuantity { get; set; }
}

/// <summary>
/// JC35【IsEnableOkStockCode】→ Admin.NET 托盘验证参数。
/// </summary>
public class PdaTrayValidateInput
{
    /// <summary>
    /// 托盘编码。
    /// </summary>
    [Required(ErrorMessage = "托盘编码不能为空！")]
    public string StockCode { get; set; }
}

/// <summary>
/// JC35【GetMaterialInfoByStockCode】→ Admin.NET 根据托盘号获取物料信息参数。
/// </summary>
public class PdaMaterialInfoByStockCodeInput
{
    /// <summary>
    /// 托盘编码。
    /// </summary>
    [Required(ErrorMessage = "托盘编码不能为空！")]
    public string StockCode { get; set; }

    /// <summary>
    /// 判断是否有箱码：0=有箱码，1=无箱码。
    /// </summary>
    public string IsBoxCode { get; set; }
}

/// <summary>
/// 物料信息输出项。
/// </summary>
public class PdaMaterialInfoItem
{
    /// <summary>
    /// 物料ID。
    /// </summary>
    public string MaterialId { get; set; }

    /// <summary>
    /// 批次号。
    /// </summary>
    public string LotNo { get; set; }

    /// <summary>
    /// 锁定数量。
    /// </summary>
    public decimal? LockQuantity { get; set; }

    /// <summary>
    /// 库存数量。
    /// </summary>
    public decimal? StockQuantity { get; set; }

    /// <summary>
    /// 物料名称。
    /// </summary>
    public string MaterialName { get; set; }

    /// <summary>
    /// 物料编码。
    /// </summary>
    public string MaterialCode { get; set; }
}

/// <summary>
/// JC35【SaveUnbindWithNoBoxCode】→ Admin.NET 无箱码托盘变更参数。
/// </summary>
public class PdaTrayChangeInput
{
    /// <summary>
    /// 原托盘编码。
    /// </summary>
    [Required(ErrorMessage = "原托盘编码不能为空！")]
    public string StockCode { get; set; }

    /// <summary>
    /// 新托盘编码。
    /// </summary>
    [Required(ErrorMessage = "新托盘编码不能为空！")]
    public string StockCodeNew { get; set; }

    /// <summary>
    /// 物料ID。
    /// </summary>
    [Required(ErrorMessage = "物料ID不能为空！")]
    public string MaterialId { get; set; }

    /// <summary>
    /// 批次号。
    /// </summary>
    [Required(ErrorMessage = "批次号不能为空！")]
    public string LotNo { get; set; }

    /// <summary>
    /// 用户ID。
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空！")]
    public string UserId { get; set; }

    /// <summary>
    /// 变更数量。
    /// </summary>
    [Required(ErrorMessage = "变更数量不能为空！")]
    public decimal ExportNum { get; set; }
}

/// <summary>
/// 更新单个箱码实际数量参数。
/// </summary>
public class PdaUpdateBoxRealQuantityInput
{
    /// <summary>
    /// 盘点单据标识（支持 Id 或 CheckBillCode）。
    /// </summary>
    [Required(ErrorMessage = "盘点单据标识不能为空！")]
    public string CheckBillCode { get; set; }

    /// <summary>
    /// 托盘编码。
    /// </summary>
    [Required(ErrorMessage = "托盘编码不能为空！")]
    public string StockCode { get; set; }

    /// <summary>
    /// 箱码。
    /// </summary>
    [Required(ErrorMessage = "箱码不能为空！")]
    public string BoxCode { get; set; }

    /// <summary>
    /// 实际数量。
    /// </summary>
    [Required(ErrorMessage = "实际数量不能为空！")]
    public decimal? RealQuantity { get; set; }
}

