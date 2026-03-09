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
/// 库存明细查询基础输入参数
/// </summary>
public class WmsStockTrayBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
}

/// <summary>
/// 库存明细查询分页查询输入参数
/// </summary>
public class PageWmsStockTrayInput : BasePageInput
{
    /// <summary>
    /// 储位位置
    /// </summary>
    public string? StockSlotCode { get; set; }
    
    /// <summary>
    /// 托盘编码
    /// </summary>
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 库存日期范围
    /// </summary>
     public DateTime?[] StockDateRange { get; set; }
    
    /// <summary>
    /// 物品编码
    /// </summary>
    public string? MaterialId { get; set; }
    
    /// <summary>
    /// 库存状态
    /// </summary>
    [Dict("SlotStatus", AllowNullValue=true)]
    public int? StockStatusFlag { get; set; }
    
    /// <summary>
    /// 库存批次
    /// </summary>
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>
    [Dict("QualityInspectionStatus", AllowNullValue=true)]
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// 巷道ID
    /// </summary>
    public string? LanewayId { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public string? WarehouseId { get; set; }

    /// <summary>
    /// 冻结状态（0 正常 1 冻结 2 ）
    /// </summary>
    public int? AbnormalStatu { get; set; }
    
}

/// <summary>
/// 库存明细查询增加输入参数
/// </summary>
public class AddWmsStockTrayInput
{
}

/// <summary>
/// 库存明细查询删除输入参数
/// </summary>
public class DeleteWmsStockTrayInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 库存明细查询更新输入参数
/// </summary>
public class UpdateWmsStockTrayInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 库存明细查询主键查询输入参数
/// </summary>
public class QueryByIdWmsStockTrayInput : DeleteWmsStockTrayInput
{
}

/// <summary>
/// 下拉数据输入参数
/// </summary>
public class DropdownDataWmsStockTrayInput
{
    /// <summary>
    /// 是否用于分页查询
    /// </summary>
    public bool FromPage { get; set; }
}

