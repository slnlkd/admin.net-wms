// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
//
using Admin.NET.Core;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application;


/// <summary>
/// 质检改判分页查询输入参数
/// </summary>
public class PageWmsStockDecideInput : BasePageInput
{
    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterialName { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    public string? LotNo { get; set; }

    /// <summary>
    /// 仓库ID
    /// </summary>
    public string? WarehouseId { get; set; }

    /// <summary>
    /// 质检状态
    /// </summary>
    [Dict("QualityInspectionStatus", AllowNullValue = true)]
    public int? InspectionStatus { get; set; }
}

/// <summary>
/// 质检改判明细查询输入参数
/// </summary>
public class WmsStockDecideDetailInput
{
    [Required(ErrorMessage = "物料ID不能为空")]
    public string MaterialId { get; set; }

    [Required(ErrorMessage = "仓库ID不能为空")]
    public string WarehouseId { get; set; }

    [Required(ErrorMessage = "批次不能为空")]
    public string LotNo { get; set; }

    [Required(ErrorMessage = "质检状态不能为空")]
    public int InspectionStatus { get; set; }
}

/// <summary>
/// 改判前置校验输入参数
/// </summary>
public class WmsStockDecideCheckInput
{
    [Required(ErrorMessage = "物料ID不能为空")]
    public string MaterialId { get; set; }

    [Required(ErrorMessage = "批次不能为空")]
    public string LotNo { get; set; }
}

/// <summary>
/// 放行输入参数
/// </summary>
public class WmsStockDecideReleaseInput
{
    [Required(ErrorMessage = "物料ID不能为空")]
    public string MaterialId { get; set; }

    [Required(ErrorMessage = "仓库ID不能为空")]
    public string WarehouseId { get; set; }

    [Required(ErrorMessage = "批次不能为空")]
    public string LotNo { get; set; }

    [Required(ErrorMessage = "质检状态不能为空")]
    public int InspectionStatus { get; set; }

    [Required(ErrorMessage = "改判日期不能为空")]
    public DateTime? RevisionDate { get; set; }

    [Required(ErrorMessage = "放行状态不能为空")]
    public int ReleaseStatus { get; set; }
}

/// <summary>
/// 改判输入参数
/// </summary>
public class WmsStockDecideInput
{
    [Required(ErrorMessage = "物料ID不能为空")]
    public string MaterialId { get; set; }

    [Required(ErrorMessage = "仓库ID不能为空")]
    public string WarehouseId { get; set; }

    [Required(ErrorMessage = "批次不能为空")]
    public string LotNo { get; set; }

    [Required(ErrorMessage = "改判前质检状态不能为空")]
    public int InspectionStatusUpt { get; set; }

    [Required(ErrorMessage = "改判后质检状态不能为空")]
    public int InspectionStatus { get; set; }

    /// <summary>
    /// 复验日期
    /// </summary>
    public DateTime? RetestDate { get; set; }

    /// <summary>
    /// 质检单号
    /// </summary>
    public string? InspectionNumber { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    public decimal? StockQuantity { get; set; }

    /// <summary>
    /// 放行状态
    /// </summary>
    public int? ReleaseStatus { get; set; }

    /// <summary>
    /// 改判人
    /// </summary>
    public string? InspectionUser { get; set; }
}
