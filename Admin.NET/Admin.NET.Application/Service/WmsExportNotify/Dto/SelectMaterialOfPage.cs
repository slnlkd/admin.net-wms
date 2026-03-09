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
public class SelectMaterialOfPage
{

    /// <summary>
    /// 物料表
    /// </summary>
    public long? WmsBaseMaterialId { get; set; }

    /// <summary>
    /// 库存表id
    /// </summary>
    public long? WmsStockId { get; set; }

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
    /// 物料型号
    /// </summary>
    public string? MaterialModel { get; set; }

    /// <summary>
    /// 物料类型（根据字典获取）
    /// </summary>
    public long? MaterialType { get; set; }
    /// <summary>
    /// 物料类型名称
    /// </summary>
    public string? MaterialTypeStr { get; set; }
    /// <summary>
    /// 物料单位id
    /// </summary>
    public long? MaterialUnitId { get; set; }
    /// <summary>
    /// 物料单位名称
    /// </summary>
    public string? MaterialUnitStr { get; set; }
    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ProductionDate { get; set; }

    /// <summary>
    /// 失效日期
    /// </summary>
    public DateTime? ValidateDay { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    public string? LotNo { get; set; }

    /// <summary>
    /// 有效期1
    /// </summary>
    public string? MaterialValidityDay1 { get; set; }

    /// <summary>
    /// 有效期2
    /// </summary>
    public string? MaterialValidityDay2 { get; set; }

    /// <summary>
    /// 有效期3
    /// </summary>
    public string? MaterialValidityDay3 { get; set; }

    /// <summary>
    /// 有效期 -- 显示的内容
    /// </summary>
    public string? MaterialValidityDayStr { get; set; }

    /// <summary>
    /// 整箱数量
    /// </summary>
    public string? BoxQuantity { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    public decimal? StockQuantity { get; set; }

    /// <summary>
    /// 质检状态(0.待检验、1.合格、2.不合格)
    /// </summary>
    public int? InspectionStatus { get; set; }

    /// <summary>
    /// 质检状态名称(0.待检验、1.合格、2.不合格)
    /// </summary>
    public string? InspectionStatusStr { get; set; }
}
