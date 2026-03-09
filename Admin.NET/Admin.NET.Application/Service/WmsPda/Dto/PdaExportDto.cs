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
/// PDA 手动拆垛箱码查询入参。
/// </summary>
public class PdaManualBoxQueryInput
{
    /// <summary>
    /// 箱码。
    /// </summary>
    [Required(ErrorMessage = "箱码不能为空！")]
    public string BoxCode { get; set; }

    /// <summary>
    /// 托盘编码。
    /// </summary>
    public string TrayCode { get; set; }

    /// <summary>
    /// 出库流水号。
    /// </summary>
    public string ExportOrderNo { get; set; }
}

/// <summary>
/// PDA 托盘出库单据查询入参。
/// </summary>
public class PdaExportOrderQueryInput
{
    /// <summary>
    /// 托盘编码。
    /// </summary>
    [Required(ErrorMessage = "托盘编码不能为空！")]
    public string StockCode { get; set; }

    /// <summary>
    /// 箱码标识："0"=有箱码；"1"=无箱码。
    /// </summary>
    public string IsBoxCode { get; set; }
}

/// <summary>
/// PDA 手动拆垛记录查询入参。
/// </summary>
public class PdaManualDepalletizingQueryInput
{
    /// <summary>
    /// 托盘编码。
    /// </summary>
    [Required(ErrorMessage = "托盘编码不能为空！")]
    public string TrayCode { get; set; }

    /// <summary>
    /// 出库流水号。
    /// </summary>
    [Required(ErrorMessage = "出库流水不能为空！")]
    public string ExportOrderNo { get; set; }
}

/// <summary>
/// PDA 手动拆垛记录新增入参（有箱码场景）。
/// </summary>
public class PdaManualDepalletizingCreateInput
{
    /// <summary>
    /// 箱码。
    /// </summary>
    public string BoxCode { get; set; }

    /// <summary>
    /// 拆垛数量。
    /// </summary>
    public decimal? ScanQty { get; set; }

    /// <summary>
    /// 绑定数量（库存数量）。
    /// </summary>
    public decimal? Qty { get; set; }

    /// <summary>
    /// 托盘编码。
    /// </summary>
    public string TrayCode { get; set; }

    /// <summary>
    /// 出库流水号。
    /// </summary>
    [Required(ErrorMessage = "出库流水不能为空！")]
    public string ExportOrderNo { get; set; }
}

/// <summary>
/// JC35【SaveManTask】→ Admin.NET 人工拆垛出库确认入参。
/// </summary>
public class PdaManualTaskSaveInput
{
    /// <summary>
    /// 托盘编码。
    /// </summary>
    [Required(ErrorMessage = "托盘编码不能为空！")]
    public string TrayCode { get; set; }

    /// <summary>
    /// 出库流水号。
    /// </summary>
    [Required(ErrorMessage = "出库流水不能为空！")]
    public string ExportOrderNo { get; set; }
}

/// <summary>
/// JC35【OutSaveManTask】→ Admin.NET 无箱码人工拆垛出库确认入参。
/// </summary>
public class PdaOutManualTaskSaveInput
{
    /// <summary>
    /// 托盘编码。
    /// </summary>
    [Required(ErrorMessage = "托盘编码不能为空！")]
    public string TrayCode { get; set; }

    /// <summary>
    /// 出库流水（或任务号）。
    /// </summary>
    [Required(ErrorMessage = "出库流水不能为空！")]
    public string ExportOrderNo { get; set; }

    /// <summary>
    /// 拆垛件数。
    /// </summary>
    public string OutQty { get; set; }

    /// <summary>
    /// 拆垛数量。
    /// </summary>
    public string ScanQty { get; set; }

    /// <summary>
    /// 类型：1=自动；2=人工；默认 2。
    /// </summary>
    public string OutType { get; set; }
}

/// <summary>
/// PDA 手动拆垛记录新增入参（无箱码场景）。
/// </summary>
public class PdaOutManualDepalletizingInput
{
    /// <summary>
    /// 托盘编码。
    /// </summary>
    [Required(ErrorMessage = "托盘编码不能为空！")]
    public string TrayCode { get; set; }

    /// <summary>
    /// 出库流水号。
    /// </summary>
    [Required(ErrorMessage = "出库流水不能为空！")]
    public string ExportOrderNo { get; set; }

    /// <summary>
    /// 拆垛数量。
    /// </summary>
    [Required(ErrorMessage = "拆垛数量不能为空！")]
    public decimal ScanQty { get; set; }

    /// <summary>
    /// 拆垛件数。
    /// </summary>
    public decimal? OutQty { get; set; }
}

/// <summary>
/// PDA 手动拆垛记录移除入参。
/// </summary>
public class PdaManualRemoveInput
{
    /// <summary>
    /// 拆垛记录主键。
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// 箱码。
    /// </summary>
    public string BoxCode { get; set; }

    /// <summary>
    /// 托盘编码。
    /// </summary>
    public string TrayCode { get; set; }

    /// <summary>
    /// 出库流水号。
    /// </summary>
    public string ExportOrderNo { get; set; }
}

/// <summary>
/// PDA 手动拆垛数量计算入参。
/// </summary>
public class PdaBoxListQtyInput
{
    public string ExportGoodsCode { get; set; }
    public string LotNo { get; set; }
    public string StockStockCode { get; set; }
    public string BoxCode { get; set; }
    public string ExportOrderNo { get; set; }
    public decimal LockQuantity { get; set; }
    public decimal? Qty { get; set; }
    public decimal? GroupQuantity { get; set; }
    public decimal OutScatterQty { get; set; }
    public int OutQty { get; set; }
    public decimal? ExportQuantity { get; set; }
    public string TypeCode { get; set; }
}

/// <summary>
/// PDA 空托出库申请入参。
/// </summary>
public class PdaOutPalnoInput
{
    /// <summary>
    /// 数量。
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "数量必须大于 0！")]
    public int Num { get; set; }

    /// <summary>
    /// 出库口。
    /// </summary>
    [Required(ErrorMessage = "出库口不能为空！")]
    public string ExportPort { get; set; }

    /// <summary>
    /// 仓库类型编码（例如 A/B/C）。PDA 接口可不传，后台会依据出库口自动推断。
    /// </summary>
    public string HouseCode { get; set; }
}

/// <summary>
/// PDA 返回结构，与 JC35 `code/msg/data` 格式兼容。
/// </summary>
public class PdaLegacyResult<T>
{
    public int Code { get; set; }

    public int Count { get; set; }

    public string Msg { get; set; }

    public T Data { get; set; }
}

/// <summary>
/// PDA 手动拆垛箱码查询输出。
/// </summary>
public class PdaManualBoxInfoResult : PdaLegacyResult<PdaManualBoxInfoDetail>
{
    public string Flag { get; set; }
}

/// <summary>
/// 拆垛箱码明细。
/// </summary>
public class PdaManualBoxInfoDetail
{
    public string ExportMaterialName { get; set; }
    public string BoxCode { get; set; }
    public string LotNo { get; set; }
    public decimal? Qty { get; set; }
    public decimal? LockQuantity { get; set; }
    public int? OutQty { get; set; }
    public int? ToutQty { get; set; }
    public string StockSlotCode { get; set; }
    public string StockCode { get; set; }
    public decimal? ExportQuantity { get; set; }
    public string StockQuantity { get; set; }
    public string ExportOrderNo { get; set; }
    public decimal GroupQuantity { get; set; }
    public string QRCode { get; set; }
    public int? OrderInspectionStatus { get; set; }
    public int? TrayInspectionStatus { get; set; }
    public int? ExportExecuteFlag { get; set; }
    public string TrayId { get; set; }
    public string Id { get; set; }
    public string MaterialCode { get; set; }
}

/// <summary>
/// PDA 出库单据项。
/// </summary>
public class PdaExportOrderItem
{
    public string ExportOrderNo { get; set; }
    public string StockCode { get; set; }
    public string TrayId { get; set; }
    public string MaterialCode { get; set; }
    public string ExportMaterialName { get; set; }
}

/// <summary>
/// PDA 手动拆垛记录返回项。
/// </summary>
public class PdaManualDepalletizingItem
{
    public string BoxCode { get; set; }
    public decimal? Qty { get; set; }
    public decimal? ScanQty { get; set; }
    public string TrayCode { get; set; }
    public string MaterialName { get; set; }
    public string MaterialCode { get; set; }
    public string Id { get; set; }
    public string OutQty { get; set; }
    public string StockLotNo { get; set; }
}

/// <summary>
/// PDA 出库口信息。
/// </summary>
public class PdaExportPortItem
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string TypeName { get; set; }
}

/// <summary>
/// PDA 出库数量计算返回。
/// </summary>
public class PdaBoxSumResult : PdaLegacyResult<PdaBoxListQtyOutput>
{
}

/// <summary>
/// PDA 出库数量计算数据。
/// </summary>
public class PdaBoxListQtyOutput
{
    public string ExportGoodsCode { get; set; }
    public string LotNo { get; set; }
    public string StockStockCode { get; set; }
    public string BoxCode { get; set; }
    public string ExportOrderNo { get; set; }
    public decimal LockQuantity { get; set; }
    public decimal? Qty { get; set; }
    public decimal? GroupQuantity { get; set; }
    public decimal OutScatterQty { get; set; }
    public int OutQty { get; set; }
    public decimal? ExportQuantity { get; set; }
    public string TypeCode { get; set; }
}
/// <summary>
/// 人工拆垛确认所需的聚合信息。
/// </summary>
public class ManualTaskDetail
{
    public long ManualId { get; set; }//设置人工拆垛ID
    public string BoxCode { get; set; }//设置箱码
    public string ManualQtyRaw { get; set; }//设置人工拆垛数量
    public string ManualScanQtyRaw { get; set; }//设置人工扫描数量
    public long? StockInfoId { get; set; }//设置库存箱码ID
    public decimal? StockQty { get; set; }//设置库存数量
    public decimal? StockLockQty { get; set; }//设置锁定数量
    public string TrayId { get; set; }//设置托盘ID
    public string MaterialCode { get; set; }//设置物料编码
    public string MaterialId { get; set; }//设置物料ID
    public string StockLotNo { get; set; }//设置批次号
    public decimal? ManualOutQty { get; set; }//设置人工出库数量
    public decimal EffectiveScanQty
    {
        get
        {
            var scan = ParseDecimal(ManualScanQtyRaw);//设置扫描数量
            return scan > 0 ? scan : ParseDecimal(ManualQtyRaw);//返回扫描数量
        }
    }
    private static decimal ParseDecimal(string raw)
    {
        return decimal.TryParse(raw, out var value) ? value : 0m;//返回扫描数量
    }
}
/// <summary>
/// PDA 空托申请的候选托盘信息，复刻 JC35 选托结果字段。
/// </summary>
public class PdaEmptyTrayCandidate
{
    /// <summary>
    /// 托盘主键 ID。
    /// </summary>
    public long TrayId { get; set; }
    /// <summary>
    /// 托盘编码。
    /// </summary>
    public string TrayCode { get; set; }
    public string VehicleSubId { get; set; }
    /// <summary>
    /// 锁定数量。
    /// </summary>
    public decimal? LockQuantity { get; set; }
    /// <summary>
    /// 库存数量。
    /// </summary>
    public decimal? StockQuantity { get; set; }
    public long SlotId { get; set; }
    /// <summary>
    /// 储位编码。
    /// </summary>
    public string SlotCode { get; set; }
    /// <summary>
    /// 进出方向。
    /// </summary>
    public int? SlotInout { get; set; }
    /// <summary>
    /// 通道ID。
    /// </summary>
    public long? SlotLanewayId { get; set; }
    /// <summary>
    /// 行。
    /// </summary>
    public int? SlotRow { get; set; }
    /// <summary>
    /// 列。
    /// </summary>
    public int? SlotColumn { get; set; }
    /// <summary>
    /// 层。
    /// </summary>
    public int? SlotLayer { get; set; }
    /// <summary>
    /// 所属仓库编号。
    /// </summary>
    public long? WarehouseId { get; set; }
    /// <summary>
    /// 目的中转位编码（为空时表示无需中转）。
    /// </summary>
    public string EndTransitLocation { get; set; }
}