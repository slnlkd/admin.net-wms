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

namespace Admin.NET.Application.Service.WmsSqlView.Dto;
/// <summary>
/// View_WmsManualDepalletizing 视图投影类。
/// </summary>
public sealed class ViewManualDepalletizing
{
    /// <summary>
    /// 物料名称
    /// </summary>
    public string ExportMaterialName { get; set; }
    /// <summary>
    /// 箱码
    /// </summary>
    public string BoxCode { get; set; }
    /// <summary>
    /// 批次号
    /// </summary>
    public string LotNo { get; set; }
    /// <summary>
    /// 数量
    /// </summary>
    public decimal? Qty { get; set; }
    /// <summary>
    /// 锁定数量
    /// </summary>
    public decimal? LockQuantity { get; set; }
    /// <summary>
    /// 出库数量
    /// </summary>
    public int? OutQty { get; set; }
    /// <summary>
    /// 托盘出库数量
    /// </summary>
    public int? ToutQty { get; set; }
    /// <summary>
    /// 库存储位编码
    /// </summary>
    public string StockSlotCode { get; set; }
    /// <summary>
    /// 库存编码
    /// </summary>
    public string StockCode { get; set; }
    /// <summary>
    /// 出库数量
    /// </summary>
    public decimal? ExportQuantity { get; set; }
    /// <summary>
    /// 库存数量
    /// </summary>
    public string StockQuantity { get; set; }
    /// <summary>
    /// 出库订单号
    /// </summary>
    public string ExportOrderNo { get; set; }
    /// <summary>
    /// 组数量
    /// </summary>
    public decimal GroupQuantity { get; set; }
    /// <summary>
    /// 二维码
    /// </summary>
    public string QRCode { get; set; }
    /// <summary>
    /// 订单检验状态
    /// </summary>
    public int? OrderInspectionStatus { get; set; }
    /// <summary>
    /// 托盘检验状态
    /// </summary>
    public int? TrayInspectionStatus { get; set; }
    /// <summary>
    /// 出库执行标识
    /// </summary>
    public int? ExportExecuteFlag { get; set; }
    /// <summary>
    /// 托盘ID
    /// </summary>
    public string TrayId { get; set; }
    /// <summary>
    /// 主键ID
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialCode { get; set; }
}
