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
/// View_WmsExportBoxInfo 视图投影类。
/// </summary>
public  class ViewExportBoxInfo
{
    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterialName { get; set; }
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    /// 箱码
    /// </summary>
    public string? BoxCode { get; set; }
    /// <summary>
    /// 数量
    /// </summary>
    public decimal? Qty { get; set; }
    /// <summary>
    /// 托盘编码
    /// </summary>
    public string? StockCode { get; set; }
    /// <summary>
    /// 出库流水号
    /// </summary>
    public string? ExportOrderNo { get; set; }
    /// <summary>
    /// 是否出库
    /// </summary>
    public int? IsOut { get; set; }
    /// <summary>
    /// 物料ID
    /// </summary>
    public string? MaterialId { get; set; }
    /// <summary>
    /// 批次号
    /// </summary>
    public string? LotNo { get; set; }
    /// <summary>
    /// 添加时间
    /// </summary>
    public DateTime? AddDate { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public int? Status { get; set; }
    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ProductionDate { get; set; }
    /// <summary>
    /// 失效日期
    /// </summary>
    public DateTime? ExportLoseDate { get; set; }
    /// <summary>
    /// 是否删除
    /// </summary>
    public bool IsDel { get; set; }
    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }
    /// <summary>
    /// 待拣数量
    /// </summary>
    public decimal? PickNum { get; set; }
    /// <summary>
    /// 是否必出箱
    /// </summary>
    public int? IsMustExport { get; set; }
    /// <summary>
    /// 物料规格
    /// </summary>
    public string? MaterialStandard { get; set; }
    /// <summary>
    /// 单位名称
    /// </summary>
    public string? UnitName { get; set; }
    /// <summary>
    /// 已拣数量
    /// </summary>
    public decimal? PickEdNum { get; set; }
}