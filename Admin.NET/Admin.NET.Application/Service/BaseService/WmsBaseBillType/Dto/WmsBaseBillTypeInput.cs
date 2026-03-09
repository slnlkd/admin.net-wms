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
/// 单据类型表基础输入参数
/// </summary>
public class WmsBaseBillTypeBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 单据类型编码
    /// </summary>
    [Required(ErrorMessage = "单据类型编码不能为空")]
    public virtual string? BillTypeCode { get; set; }
    
    /// <summary>
    /// 单据类型名称
    /// </summary>
    [Required(ErrorMessage = "单据类型名称不能为空")]
    public virtual string? BillTypeName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 类型
    /// </summary>
    [Dict("BillType", AllowNullValue=true)]
    [Required(ErrorMessage = "类型不能为空")]
    public virtual int? BillType { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>
    [Required(ErrorMessage = "质检状态不能为空")]
    public virtual string? QualityInspectionStatus { get; set; }
    
    /// <summary>
    /// 代储状态
    /// </summary>
    [Dict("ProxyStatus", AllowNullValue=true)]
    [Required(ErrorMessage = "代储状态不能为空")]
    public virtual int? ProxyStatus { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    [Required(ErrorMessage = "所属仓库不能为空")]
    public virtual long? WareHouseId { get; set; }
    
}

/// <summary>
/// 单据类型表分页查询输入参数
/// </summary>
public class PageWmsBaseBillTypeInput : BasePageInput
{
    /// <summary>
    /// 单据类型编码
    /// </summary>
    public string? BillTypeCode { get; set; }
    
    /// <summary>
    /// 单据类型名称
    /// </summary>
    public string? BillTypeName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
    
    /// <summary>
    /// 类型
    /// </summary>
    [Dict("BillType", AllowNullValue=true)]
    public int? BillType { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>
    public string? QualityInspectionStatus { get; set; }
    
    /// <summary>
    /// 代储状态
    /// </summary>
    [Dict("ProxyStatus", AllowNullValue=true)]
    public int? ProxyStatus { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    public long? WareHouseId { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 单据类型表增加输入参数
/// </summary>
public class AddWmsBaseBillTypeInput
{
    /// <summary>
    /// 单据类型编码
    /// </summary>
    [Required(ErrorMessage = "单据类型编码不能为空")]
    [MaxLength(64, ErrorMessage = "单据类型编码字符长度不能超过64")]
    public string? BillTypeCode { get; set; }
    
    /// <summary>
    /// 单据类型名称
    /// </summary>
    [Required(ErrorMessage = "单据类型名称不能为空")]
    [MaxLength(64, ErrorMessage = "单据类型名称字符长度不能超过64")]
    public string? BillTypeName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(255, ErrorMessage = "备注字符长度不能超过255")]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 类型
    /// </summary>
    [Dict("BillType", AllowNullValue=true)]
    [Required(ErrorMessage = "类型不能为空")]
    public int? BillType { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>
    [Required(ErrorMessage = "质检状态不能为空")]
    public string? QualityInspectionStatus { get; set; }
    
    /// <summary>
    /// 代储状态
    /// </summary>
    [Dict("ProxyStatus", AllowNullValue=true)]
    [Required(ErrorMessage = "代储状态不能为空")]
    public int? ProxyStatus { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    [Required(ErrorMessage = "所属仓库不能为空")]
    public long? WareHouseId { get; set; }
    
}

/// <summary>
/// 单据类型表删除输入参数
/// </summary>
public class DeleteWmsBaseBillTypeInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 单据类型表更新输入参数
/// </summary>
public class UpdateWmsBaseBillTypeInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 单据类型编码
    /// </summary>    
    [Required(ErrorMessage = "单据类型编码不能为空")]
    [MaxLength(64, ErrorMessage = "单据类型编码字符长度不能超过64")]
    public string? BillTypeCode { get; set; }
    
    /// <summary>
    /// 单据类型名称
    /// </summary>    
    [Required(ErrorMessage = "单据类型名称不能为空")]
    [MaxLength(64, ErrorMessage = "单据类型名称字符长度不能超过64")]
    public string? BillTypeName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>    
    [MaxLength(255, ErrorMessage = "备注字符长度不能超过255")]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 类型
    /// </summary>    
    [Dict("BillType", AllowNullValue=true)]
    [Required(ErrorMessage = "类型不能为空")]
    public int? BillType { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>    
    [Required(ErrorMessage = "质检状态不能为空")]
    public string? QualityInspectionStatus { get; set; }
    
    /// <summary>
    /// 代储状态
    /// </summary>    
    [Dict("ProxyStatus", AllowNullValue=true)]
    [Required(ErrorMessage = "代储状态不能为空")]
    public int? ProxyStatus { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>    
    [Required(ErrorMessage = "所属仓库不能为空")]
    public long? WareHouseId { get; set; }
    
}

/// <summary>
/// 单据类型表主键查询输入参数
/// </summary>
public class QueryByIdWmsBaseBillTypeInput : DeleteWmsBaseBillTypeInput
{
}

/// <summary>
/// 下拉数据输入参数
/// </summary>
public class DropdownDataWmsBaseBillTypeInput
{
    /// <summary>
    /// 是否用于分页查询
    /// </summary>
    public bool FromPage { get; set; }
}

/// <summary>
/// 单据类型表数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsBaseBillTypeInput : BaseImportInput
{
    /// <summary>
    /// 单据类型编码
    /// </summary>
    [ImporterHeader(Name = "*单据类型编码")]
    [ExporterHeader("*单据类型编码", Format = "", Width = 25, IsBold = true)]
    public string? BillTypeCode { get; set; }
    
    /// <summary>
    /// 单据类型名称
    /// </summary>
    [ImporterHeader(Name = "*单据类型名称")]
    [ExporterHeader("*单据类型名称", Format = "", Width = 25, IsBold = true)]
    public string? BillTypeName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 类型 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public int? BillType { get; set; }
    
    /// <summary>
    /// 类型 文本
    /// </summary>
    [Dict("BillType")]
    [ImporterHeader(Name = "*类型")]
    [ExporterHeader("*类型", Format = "", Width = 25, IsBold = true)]
    public string BillTypeDictLabel { get; set; }
    
    /// <summary>
    /// 质检状态 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? QualityInspectionStatus { get; set; }
    
    /// <summary>
    /// 质检状态 文本
    /// </summary>
    [ImporterHeader(Name = "*质检状态")]
    [ExporterHeader("*质检状态", Format = "", Width = 25, IsBold = true)]
    public string QualityInspectionStatusDictLabel { get; set; }
    
    /// <summary>
    /// 代储状态 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public int? ProxyStatus { get; set; }
    
    /// <summary>
    /// 代储状态 文本
    /// </summary>
    [Dict("ProxyStatus")]
    [ImporterHeader(Name = "*代储状态")]
    [ExporterHeader("*代储状态", Format = "", Width = 25, IsBold = true)]
    public string ProxyStatusDictLabel { get; set; }
    
    /// <summary>
    /// 所属仓库 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? WareHouseId { get; set; }
    
    /// <summary>
    /// 所属仓库 文本
    /// </summary>
    [ImporterHeader(Name = "*所属仓库")]
    [ExporterHeader("*所属仓库", Format = "", Width = 25, IsBold = true)]
    public string WareHouseFkDisplayName { get; set; }
    
}
