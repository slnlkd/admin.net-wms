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
public class ExprotOrderModel
{
    public List<ExprotOrderModelInfo> DataList { get; set; }
}

public class ExprotOrderModelInfo
{
    /// <summary>
    /// 出库流水ID
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// 出库流水仓库类型
    /// </summary>
    public string WarehouseType { get; set; }
}
public class ExportLibraryDTO
{
    /// <summary>
    /// 储位编码
    /// </summary>
    public string TaskBegin { get; set; }
    /// <summary>
    /// 任务号
    /// </summary>
    public string TaskNo { get; set; }
    /// <summary>
    /// 任务类型
    /// </summary>
    public string TaskType { get; set; }
    /// <summary>
    /// 托盘编码
    /// </summary>
    public string StockCode { get; set; }
    /// <summary>
    /// 目标地址（可能是仓库口、可能是移库后的储位）
    /// </summary>
    public string TaskEnd { get; set; }
    /// <summary>
    /// 所属仓库
    /// </summary>
    public string HouseCode { get; set; }
    /// <summary>
    /// 出库单号
    /// </summary>
    public string BillCode { get; set; }
    /// <summary>
    /// 拣选数量
    /// </summary>
    public decimal Qty { get; set; }
    /// <summary>
    /// 托盘数量
    /// </summary>
    public string WholeBoxNum { get; set; }
    public List<StockBoxInfo> boxInfos { get; set; }
    public string LanewayId { get; set; }
    /// <summary>
    /// 0：主载具；1：副载具
    /// </summary>
    public string IsExportStockTary { get; set; }
    /// <summary>
    /// 起始中转位置
    /// </summary>
    public string beginTransitLocation { get; set; }
    /// <summary>
    /// 目的中转位置，移库时不能为空
    /// </summary>
    public string EndTransitLocation { get; set; }
}
