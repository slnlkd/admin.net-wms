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

namespace Admin.NET.Application;
/// <summary>
/// 选择出库库存物料明细
/// </summary>
public class SelectStockGoodsDTO
{
    public string StockMaterialId { get; set; }
    public string StockMaterialCode { get; set; }
    public string StockMaterialName { get; set; }
    public string StockMaterialStandard { get; set; }
    public string StockMaterialStatus { get; set; }
    public string StockMaterialUnitName { get; set; }
    public string StockLotNo { get; set; }
    public decimal StockQuantity { get; set; }
    public decimal ExportQuantity { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public DateTime? ValidateDay { get; set; }
    public decimal BoxQuantity { get; set; }
    public string GoodsStampdown { get; set; }
    public string Goodsdevanning { get; set; }
    public string InspectionStatus { get; set; }
    public string WarehouseId { get; set; }
    public string OuterDetailId { get; set; }
    public DateTime? ProductionDate { get; set; }
}

/// <summary>
/// 临时查询的类
/// </summary>
public class SelectStockGoodsDTOQuery
{
    public long? Id { get; set; }
    public string? MaterialCode { get; set; }
    public string? MaterialName { get; set; }
    public string? MaterialStandard { get; set; }
    public string? LotNo { get; set; }
    public string? UnitName { get; set; }
    public string? BoxQuantity { get; set; }
    public decimal? StockQuantity { get; set; }
    public string? CustomerId { get; set; }
    public int? InspectionStatus { get; set; }
    public DateTime? ValidateDay { get; set; }
    public string? WarehouseId { get; set; }
    public DateTime? ProductionDate { get; set; }
}
