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
/// 托盘管理基础输入参数
/// </summary>
public class WmsSysStockCodeBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 载具号
    /// </summary>
    public virtual string? StockCode { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    [Required(ErrorMessage = "所属仓库不能为空")]
    public virtual long? WarehouseId { get; set; }
    
    /// <summary>
    /// 托盘类型
    /// </summary>
    [Dict("StockType", AllowNullValue=true)]
    [Required(ErrorMessage = "托盘类型不能为空")]
    public virtual long? StockType { get; set; }
    
}

/// <summary>
/// 托盘管理分页查询输入参数
/// </summary>
public class PageWmsSysStockCodeInput : BasePageInput
{
    /// <summary>
    /// 载具号
    /// </summary>
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 条码状态
    /// </summary>
    [Dict("StockIsUse", AllowNullValue=true)]
    public int? Status { get; set; }
    
    /// <summary>
    /// 托盘类型
    /// </summary>
    [Dict("StockType", AllowNullValue=true)]
    public long? StockType { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 托盘管理增加输入参数
/// </summary>
public class AddWmsSysStockCodeInput
{
    /// <summary>
    /// 载具号
    /// </summary>
    [Required(ErrorMessage = "载具号不能为空")]
    [MaxLength(50, ErrorMessage = "载具号字符长度不能超过50")]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    [Required(ErrorMessage = "所属仓库不能为空")]
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 托盘类型
    /// </summary>
    [Dict("StockType", AllowNullValue=true)]
    [Required(ErrorMessage = "托盘类型不能为空")]
    public long? StockType { get; set; }
    
}

/// <summary>
/// 托盘管理删除输入参数
/// </summary>
public class DeleteWmsSysStockCodeInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 托盘管理更新输入参数
/// </summary>
public class UpdateWmsSysStockCodeInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 载具号
    /// </summary>    
    [Required(ErrorMessage = "载具号不能为空")]
    [MaxLength(50, ErrorMessage = "载具号字符长度不能超过50")]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>    
    [Required(ErrorMessage = "所属仓库不能为空")]
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 托盘类型
    /// </summary>    
    [Dict("StockType", AllowNullValue=true)]
    [Required(ErrorMessage = "托盘类型不能为空")]
    public long? StockType { get; set; }
    
}

/// <summary>
/// 托盘管理主键查询输入参数
/// </summary>
public class QueryByIdWmsSysStockCodeInput : DeleteWmsSysStockCodeInput
{
}

/// <summary>
/// 下拉数据输入参数
/// </summary>
public class DropdownDataWmsSysStockCodeInput
{
    /// <summary>
    /// 是否用于分页查询
    /// </summary>
    public bool FromPage { get; set; }
}

/// <summary>
/// 托盘管理数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsSysStockCodeInput : BaseImportInput
{
    /// <summary>
    /// 载具号
    /// </summary>
    [ImporterHeader(Name = "*载具号")]
    [ExporterHeader("*载具号", Format = "", Width = 25, IsBold = true)]
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 所属仓库 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 所属仓库 文本
    /// </summary>
    [ImporterHeader(Name = "*所属仓库")]
    [ExporterHeader("*所属仓库", Format = "", Width = 25, IsBold = true)]
    public string WarehouseFkDisplayName { get; set; }
    
    /// <summary>
    /// 条码状态 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public int? Status { get; set; }
    
    /// <summary>
    /// 条码状态 文本
    /// </summary>
    [Dict("StockIsUse")]
    [ImporterHeader(Name = "条码状态")]
    [ExporterHeader("条码状态", Format = "", Width = 25, IsBold = true)]
    public string StatusDictLabel { get; set; }
    
    /// <summary>
    /// 托盘类型 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? StockType { get; set; }
    
    /// <summary>
    /// 托盘类型 文本
    /// </summary>
    [Dict("StockType")]
    [ImporterHeader(Name = "*托盘类型")]
    [ExporterHeader("*托盘类型", Format = "", Width = 25, IsBold = true)]
    public string StockTypeDictLabel { get; set; }
    
}
