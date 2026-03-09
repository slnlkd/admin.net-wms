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
public class ShowNotifyAndDetiailByNotifyIdOfPage
{
    /// <summary>
    /// 出库单据明细表id
    /// </summary>
    public long? NotifyDetailId { get; set; } 

    /// <summary>
    /// 出库单据id
    /// </summary>
    public long? NotifyId { get; set; }

    ///// <summary>
    ///// 库存表id
    ///// </summary>
    //public long? WmsStockId { get; set; }

    /// <summary>
    /// 物料表id
    /// </summary>
    public long? MaterialId { get; set; }

    /// <summary>
    /// 物料单位表id
    /// </summary>
    public long? WmsBaseUnitId { get; set; }

    /// <summary>
    /// 出库单据--单据中的编号
    /// </summary>
    public string? ExportBillCode { get; set; }

    /// <summary>
    /// 执行标志（0待执行、1正在分配、2正在执行、3已完成、4作废、5已上传
    /// </summary>
    public int? ExportExecuteFlag { get; set; }

    /// <summary>
    /// 执行标志名称
    /// </summary>
    public string? ExportExecuteFlagStr { get; set; }

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
    /// 批次
    /// </summary>
    public string? LotNo { get; set; }

    ///// <summary>
    ///// 质检状态(0.待检验、1.合格、2.不合格)
    ///// </summary>
    //public int? InspectionStatus { get; set; }

    ///// <summary>
    ///// 质检状态名称
    ///// </summary>
    //public string? InspectionStatusStr { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ProductionDate { get; set; }

    /// <summary>
    /// 失效日期
    /// </summary>
    public DateTime? LostDate { get; set; }

    /// <summary>
    /// 物料单位名称
    /// </summary>
    public string? UnitName { get; set; }

    /// <summary>
    /// 计划出库数量
    /// </summary>
    public decimal? ExportQuantity { get; set; }

    /// <summary>
    /// 分配数量
    /// </summary>
    public decimal? AllocateQuantity { get; set; }

    /// <summary>
    /// 下发数量
    /// </summary>
    public decimal? FactQuantity { get; set; }

    /// <summary>
    /// 完成数量
    /// </summary>
    public decimal? CompleteQuantity { get; set; }
}
