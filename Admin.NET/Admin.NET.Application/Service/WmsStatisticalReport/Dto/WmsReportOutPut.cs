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
using Magicodes.ExporterAndImporter.Core;
namespace Admin.NET.Application;
public class WmsReportOutPut
{
    public long Id;
    /// <summary>
    /// 物料Id
    /// </summary>
    public string MaterialId;
    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialCode;
    /// <summary>
    /// 物料名称
    /// </summary>
    public string MaterialName;
    /// <summary>
    /// 物料规格
    /// </summary>
    public string MaterialStandard;
    /// <summary>
    /// 单位
    /// </summary>
    public string UnitName;
    /// <summary>
    /// 
    /// </summary>
    public System.Nullable<decimal> ImportQuantity;
    /// <summary>
    /// 入库量
    /// </summary>
    public System.Nullable<decimal> Qty;
    /// <summary>
    /// 出库量
    /// </summary>
    public System.Nullable<decimal> ExportQuantity;
}
