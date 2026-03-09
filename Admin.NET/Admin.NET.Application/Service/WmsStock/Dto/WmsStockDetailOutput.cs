// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using Magicodes.ExporterAndImporter.Core;
namespace Admin.NET.Application;

/// <summary>
/// 库存查询输出参数
/// </summary>
public class WmsStockOutput
{
    /// <summary>
    /// 物料id
    /// </summary>
    public string MaterialName { get; set; }    
    
    /// <summary>
    /// 库存数量
    /// </summary>
    public decimal? StockQuantity { get; set; }    
    
    /// <summary>
    /// 锁定数量
    /// </summary>
    public decimal? LockQuantity { get; set; }    
    
    /// <summary>
    /// 批次
    /// </summary>
    public string WarehouseName { get; set; }    
    
}
