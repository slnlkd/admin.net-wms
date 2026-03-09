// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application;

/// <summary>
/// 计量单位基础输入参数
/// </summary>
public class WmsBaseUnitBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 单位编码
    /// </summary>
    public virtual string? UnitCode { get; set; }
    
    /// <summary>
    /// 单位名称
    /// </summary>
    [Required(ErrorMessage = "单位名称不能为空")]
    public virtual string? UnitName { get; set; }
    
    /// <summary>
    /// 单位缩写名称
    /// </summary>
    public virtual string? UnitAbbrevName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remark { get; set; }
    
}

/// <summary>
/// 计量单位分页查询输入参数
/// </summary>
public class PageWmsBaseUnitInput : BasePageInput
{
    /// <summary>
    /// 单位编码
    /// </summary>
    public string? UnitCode { get; set; }
    
    /// <summary>
    /// 单位名称
    /// </summary>
    public string? UnitName { get; set; }
    
    /// <summary>
    /// 单位缩写名称
    /// </summary>
    public string? UnitAbbrevName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
    
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }
    
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 计量单位增加输入参数
/// </summary>
public class AddWmsBaseUnitInput
{
    /// <summary>
    /// 单位编码
    /// </summary>
    [MaxLength(64, ErrorMessage = "单位编码字符长度不能超过64")]
    public string? UnitCode { get; set; }
    
    /// <summary>
    /// 单位名称
    /// </summary>
    [Required(ErrorMessage = "单位名称不能为空")]
    [MaxLength(64, ErrorMessage = "单位名称字符长度不能超过64")]
    public string? UnitName { get; set; }
    
    /// <summary>
    /// 单位缩写名称
    /// </summary>
    [MaxLength(32, ErrorMessage = "单位缩写名称字符长度不能超过32")]
    public string? UnitAbbrevName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(255, ErrorMessage = "备注字符长度不能超过255")]
    public string? Remark { get; set; }
    
}

/// <summary>
/// 计量单位删除输入参数
/// </summary>
public class DeleteWmsBaseUnitInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 计量单位更新输入参数
/// </summary>
public class UpdateWmsBaseUnitInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 单位编码
    /// </summary>    
    [Description("单位编码")]
    [MaxLength(64, ErrorMessage = "单位编码字符长度不能超过64")]
    public string? UnitCode { get; set; }
    
    /// <summary>
    /// 单位名称
    /// </summary>    
    [Description("单位名称")]
    [Required(ErrorMessage = "单位名称不能为空")]
    [MaxLength(64, ErrorMessage = "单位名称字符长度不能超过64")]
    public string? UnitName { get; set; }
    
    /// <summary>
    /// 单位缩写名称
    /// </summary>    
    [Description("单位缩写名称")]
    [MaxLength(32, ErrorMessage = "单位缩写名称字符长度不能超过32")]
    public string? UnitAbbrevName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>    
    [Description("备注")]
    [MaxLength(255, ErrorMessage = "备注字符长度不能超过255")]
    public string? Remark { get; set; }
    
}

/// <summary>
/// 计量单位主键查询输入参数
/// </summary>
public class QueryByIdWmsBaseUnitInput : DeleteWmsBaseUnitInput
{
}

/// <summary>
/// 计量单位数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsBaseUnitInput : BaseImportInput
{
    /// <summary>
    /// 单位编码
    /// </summary>
    [ImporterHeader(Name = "单位编码")]
    [ExporterHeader("单位编码", Format = "", Width = 25, IsBold = true)]
    public string? UnitCode { get; set; }
    
    /// <summary>
    /// 单位名称
    /// </summary>
    [ImporterHeader(Name = "*单位名称")]
    [ExporterHeader("*单位名称", Format = "", Width = 25, IsBold = true)]
    public string? UnitName { get; set; }
    
    /// <summary>
    /// 单位缩写名称
    /// </summary>
    [ImporterHeader(Name = "单位缩写名称")]
    [ExporterHeader("单位缩写名称", Format = "", Width = 25, IsBold = true)]
    public string? UnitAbbrevName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? Remark { get; set; }
    
}
