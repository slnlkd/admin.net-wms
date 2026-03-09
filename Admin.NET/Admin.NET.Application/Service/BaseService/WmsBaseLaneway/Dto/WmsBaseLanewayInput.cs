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
/// 巷道表基础输入参数
/// </summary>
public class WmsBaseLanewayBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    [Required(ErrorMessage = "所属仓库不能为空")]
    public virtual long? WarehouseId { get; set; }
    
    /// <summary>
    /// 巷道编码
    /// </summary>
    [Required(ErrorMessage = "巷道编码不能为空")]
    public virtual string? LanewayCode { get; set; }
    
    /// <summary>
    /// 巷道名称
    /// </summary>
    [Required(ErrorMessage = "巷道名称不能为空")]
    public virtual string? LanewayName { get; set; }
    
    /// <summary>
    /// 巷道状态
    /// </summary>
    public virtual bool? LanewayStatus { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 优先级
    /// </summary>
    public virtual int? LanewayPriority { get; set; }
    
    /// <summary>
    /// 储存条件
    /// </summary>
    [Dict("LanewayTemp", AllowNullValue=true)]
    public virtual string? LanewayTemp { get; set; }
    
    /// <summary>
    /// 巷道口状态
    /// </summary>
    [Dict("LanewayType", AllowNullValue=true)]
    public virtual int? LanewayType { get; set; }
    
}

/// <summary>
/// 巷道表分页查询输入参数
/// </summary>
public class PageWmsBaseLanewayInput : BasePageInput
{
    /// <summary>
    /// 所属仓库
    /// </summary>
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 巷道编码
    /// </summary>
    public string? LanewayCode { get; set; }
    
    /// <summary>
    /// 巷道名称
    /// </summary>
    public string? LanewayName { get; set; }
    
    /// <summary>
    /// 巷道状态
    /// </summary>
    public bool? LanewayStatus { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
    
    /// <summary>
    /// 优先级
    /// </summary>
    public int? LanewayPriority { get; set; }
    
    /// <summary>
    /// 储存条件
    /// </summary>
    [Dict("LanewayTemp", AllowNullValue=true)]
    public string? LanewayTemp { get; set; }
    
    /// <summary>
    /// 巷道口状态
    /// </summary>
    [Dict("LanewayType", AllowNullValue=true)]
    public int? LanewayType { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 巷道表增加输入参数
/// </summary>
public class AddWmsBaseLanewayInput
{
    /// <summary>
    /// 所属仓库
    /// </summary>
    [Required(ErrorMessage = "所属仓库不能为空")]
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 巷道编码
    /// </summary>
    [Required(ErrorMessage = "巷道编码不能为空")]
    [MaxLength(64, ErrorMessage = "巷道编码字符长度不能超过64")]
    public string? LanewayCode { get; set; }
    
    /// <summary>
    /// 巷道名称
    /// </summary>
    [Required(ErrorMessage = "巷道名称不能为空")]
    [MaxLength(64, ErrorMessage = "巷道名称字符长度不能超过64")]
    public string? LanewayName { get; set; }
    
    /// <summary>
    /// 巷道状态
    /// </summary>
    public bool? LanewayStatus { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(64, ErrorMessage = "备注字符长度不能超过64")]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 优先级
    /// </summary>
    public int? LanewayPriority { get; set; }
    
    /// <summary>
    /// 储存条件
    /// </summary>
    [Dict("LanewayTemp", AllowNullValue=true)]
    [MaxLength(64, ErrorMessage = "储存条件字符长度不能超过64")]
    public string? LanewayTemp { get; set; }
    
    /// <summary>
    /// 巷道口状态
    /// </summary>
    [Dict("LanewayType", AllowNullValue=true)]
    public int? LanewayType { get; set; }
    
}

/// <summary>
/// 巷道表删除输入参数
/// </summary>
public class DeleteWmsBaseLanewayInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 巷道表更新输入参数
/// </summary>
public class UpdateWmsBaseLanewayInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>    
    [Required(ErrorMessage = "所属仓库不能为空")]
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 巷道编码
    /// </summary>    
    [Required(ErrorMessage = "巷道编码不能为空")]
    [MaxLength(64, ErrorMessage = "巷道编码字符长度不能超过64")]
    public string? LanewayCode { get; set; }
    
    /// <summary>
    /// 巷道名称
    /// </summary>    
    [Required(ErrorMessage = "巷道名称不能为空")]
    [MaxLength(64, ErrorMessage = "巷道名称字符长度不能超过64")]
    public string? LanewayName { get; set; }
    
    /// <summary>
    /// 巷道状态
    /// </summary>    
    public bool? LanewayStatus { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>    
    [MaxLength(64, ErrorMessage = "备注字符长度不能超过64")]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 优先级
    /// </summary>    
    public int? LanewayPriority { get; set; }
    
    /// <summary>
    /// 储存条件
    /// </summary>    
    [Dict("LanewayTemp", AllowNullValue=true)]
    [MaxLength(64, ErrorMessage = "储存条件字符长度不能超过64")]
    public string? LanewayTemp { get; set; }
    
    /// <summary>
    /// 巷道口状态
    /// </summary>    
    [Dict("LanewayType", AllowNullValue=true)]
    public int? LanewayType { get; set; }
    
}

/// <summary>
/// 巷道表主键查询输入参数
/// </summary>
public class QueryByIdWmsBaseLanewayInput : DeleteWmsBaseLanewayInput
{
}

/// <summary>
/// 下拉数据输入参数
/// </summary>
public class DropdownDataWmsBaseLanewayInput
{
    /// <summary>
    /// 是否用于分页查询
    /// </summary>
    public bool FromPage { get; set; }
}

/// <summary>
/// 巷道表数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsBaseLanewayInput : BaseImportInput
{
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
    /// 巷道编码
    /// </summary>
    [ImporterHeader(Name = "*巷道编码")]
    [ExporterHeader("*巷道编码", Format = "", Width = 25, IsBold = true)]
    public string? LanewayCode { get; set; }
    
    /// <summary>
    /// 巷道名称
    /// </summary>
    [ImporterHeader(Name = "*巷道名称")]
    [ExporterHeader("*巷道名称", Format = "", Width = 25, IsBold = true)]
    public string? LanewayName { get; set; }
    
    /// <summary>
    /// 巷道状态
    /// </summary>
    [ImporterHeader(Name = "巷道状态")]
    [ExporterHeader("巷道状态", Format = "", Width = 25, IsBold = true)]
    public bool? LanewayStatus { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 优先级
    /// </summary>
    [ImporterHeader(Name = "优先级")]
    [ExporterHeader("优先级", Format = "", Width = 25, IsBold = true)]
    public int? LanewayPriority { get; set; }
    
    /// <summary>
    /// 储存条件 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? LanewayTemp { get; set; }
    
    /// <summary>
    /// 储存条件 文本
    /// </summary>
    [Dict("LanewayTemp")]
    [ImporterHeader(Name = "储存条件")]
    [ExporterHeader("储存条件", Format = "", Width = 25, IsBold = true)]
    public string LanewayTempDictLabel { get; set; }
    
    /// <summary>
    /// 巷道口状态 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public int? LanewayType { get; set; }
    
    /// <summary>
    /// 巷道口状态 文本
    /// </summary>
    [Dict("LanewayType")]
    [ImporterHeader(Name = "巷道口状态")]
    [ExporterHeader("巷道口状态", Format = "", Width = 25, IsBold = true)]
    public string LanewayTypeDictLabel { get; set; }
    
}
