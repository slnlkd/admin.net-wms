// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Application.Service.WmsPda.Dto;

/// <summary>
/// 手工绑定箱码请求参数
/// </summary>
public class BackConfirmInput
{
    /// <summary>
    /// 入库单号
    /// </summary>
    public string OrderNo { get; set; }

    /// <summary>
    /// 箱码（或二维码）
    /// </summary>
    public string BoxCode { get; set; }

    /// <summary>
    /// 托盘编码
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// 物料编码（可选）
    /// </summary>
    public string GoodCode { get; set; }

    /// <summary>
    /// 批次（可选）
    /// </summary>
    public string LotNo { get; set; }

    /// <summary>
    /// 数量（可选，兼容自动组托）
    /// </summary>
    public string Qty { get; set; }

    /// <summary>
    /// 件数（可选，兼容自动组托）
    /// </summary>
    public string WholeBoxNum { get; set; }

    /// <summary>
    /// 质检标识（可选）
    /// </summary>
    public string BulkTank { get; set; }

    /// <summary>
    /// 生产日期（可选）
    /// </summary>
    public string ProductionDate { get; set; }

    /// <summary>
    /// 失效日期（可选）
    /// </summary>
    public string ValidateDay { get; set; }
}

