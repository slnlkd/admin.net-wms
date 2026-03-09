// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using System.ComponentModel.DataAnnotations;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

namespace Admin.NET.Application;

/// <summary>
/// 库存查询基础输入参数
/// </summary>
public class WmsStockDetailInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
}

/// <summary>
/// 库存查询分页查询输入参数
/// </summary>
public class PageWmsStockDetailInput : BasePageInput
{
    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialCode { get; set; }
    /// <summary>
    /// 物料id
    /// </summary>
    public string MaterialName { get; set; }

    /// <summary>
    /// 库房ID
    /// </summary>
    public long? WarehouseId { get; set; }
    /// <summary>
    /// 库房名称
    /// </summary>
    public string WarehouseName { get; set; }
        
}


