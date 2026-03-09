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
/// 制造商信息基础输入参数
/// </summary>
public class WmsBaseManufacturerBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 制造商编码
    /// </summary>
    [Required(ErrorMessage = "制造商编码不能为空")]
    public virtual string? CustomerCode { get; set; }
    
    /// <summary>
    /// 制造商名称
    /// </summary>
    [Required(ErrorMessage = "制造商名称不能为空")]
    public virtual string? CustomerName { get; set; }
    
    /// <summary>
    /// 制造商地址
    /// </summary>
    public virtual string? CustomerAddress { get; set; }
    
    /// <summary>
    /// 联系人
    /// </summary>
    public virtual string? CustomerLinkman { get; set; }
    
    /// <summary>
    /// 联系电话
    /// </summary>
    public virtual string? CustomerPhone { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remark { get; set; }
    
}

/// <summary>
/// 制造商信息分页查询输入参数
/// </summary>
public class PageWmsBaseManufacturerInput : BasePageInput
{
    /// <summary>
    /// 制造商编码
    /// </summary>
    public string? CustomerCode { get; set; }
    
    /// <summary>
    /// 制造商名称
    /// </summary>
    public string? CustomerName { get; set; }
    
    /// <summary>
    /// 制造商地址
    /// </summary>
    public string? CustomerAddress { get; set; }
    
    /// <summary>
    /// 联系人
    /// </summary>
    public string? CustomerLinkman { get; set; }
    
    /// <summary>
    /// 联系电话
    /// </summary>
    public string? CustomerPhone { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 制造商信息增加输入参数
/// </summary>
public class AddWmsBaseManufacturerInput
{
    /// <summary>
    /// 制造商编码
    /// </summary>
    [Required(ErrorMessage = "制造商编码不能为空")]
    [MaxLength(32, ErrorMessage = "制造商编码字符长度不能超过32")]
    public string? CustomerCode { get; set; }
    
    /// <summary>
    /// 制造商名称
    /// </summary>
    [Required(ErrorMessage = "制造商名称不能为空")]
    [MaxLength(32, ErrorMessage = "制造商名称字符长度不能超过32")]
    public string? CustomerName { get; set; }
    
    /// <summary>
    /// 制造商地址
    /// </summary>
    [MaxLength(32, ErrorMessage = "制造商地址字符长度不能超过32")]
    public string? CustomerAddress { get; set; }
    
    /// <summary>
    /// 联系人
    /// </summary>
    [MaxLength(32, ErrorMessage = "联系人字符长度不能超过32")]
    public string? CustomerLinkman { get; set; }
    
    /// <summary>
    /// 联系电话
    /// </summary>
    [MaxLength(32, ErrorMessage = "联系电话字符长度不能超过32")]
    public string? CustomerPhone { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(32, ErrorMessage = "备注字符长度不能超过32")]
    public string? Remark { get; set; }
    
}

/// <summary>
/// 制造商信息删除输入参数
/// </summary>
public class DeleteWmsBaseManufacturerInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 制造商信息更新输入参数
/// </summary>
public class UpdateWmsBaseManufacturerInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 制造商编码
    /// </summary>    
    [Required(ErrorMessage = "制造商编码不能为空")]
    [MaxLength(32, ErrorMessage = "制造商编码字符长度不能超过32")]
    public string? CustomerCode { get; set; }
    
    /// <summary>
    /// 制造商名称
    /// </summary>    
    [Required(ErrorMessage = "制造商名称不能为空")]
    [MaxLength(32, ErrorMessage = "制造商名称字符长度不能超过32")]
    public string? CustomerName { get; set; }
    
    /// <summary>
    /// 制造商地址
    /// </summary>    
    [MaxLength(32, ErrorMessage = "制造商地址字符长度不能超过32")]
    public string? CustomerAddress { get; set; }
    
    /// <summary>
    /// 联系人
    /// </summary>    
    [MaxLength(32, ErrorMessage = "联系人字符长度不能超过32")]
    public string? CustomerLinkman { get; set; }
    
    /// <summary>
    /// 联系电话
    /// </summary>    
    [MaxLength(32, ErrorMessage = "联系电话字符长度不能超过32")]
    public string? CustomerPhone { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>    
    [MaxLength(32, ErrorMessage = "备注字符长度不能超过32")]
    public string? Remark { get; set; }
    
}

/// <summary>
/// 制造商信息主键查询输入参数
/// </summary>
public class QueryByIdWmsBaseManufacturerInput : DeleteWmsBaseManufacturerInput
{
}

/// <summary>
/// 制造商信息数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsBaseManufacturerInput : BaseImportInput
{
    /// <summary>
    /// 制造商编码
    /// </summary>
    [ImporterHeader(Name = "*制造商编码")]
    [ExporterHeader("*制造商编码", Format = "", Width = 25, IsBold = true)]
    public string? CustomerCode { get; set; }
    
    /// <summary>
    /// 制造商名称
    /// </summary>
    [ImporterHeader(Name = "*制造商名称")]
    [ExporterHeader("*制造商名称", Format = "", Width = 25, IsBold = true)]
    public string? CustomerName { get; set; }
    
    /// <summary>
    /// 制造商地址
    /// </summary>
    [ImporterHeader(Name = "制造商地址")]
    [ExporterHeader("制造商地址", Format = "", Width = 25, IsBold = true)]
    public string? CustomerAddress { get; set; }
    
    /// <summary>
    /// 联系人
    /// </summary>
    [ImporterHeader(Name = "联系人")]
    [ExporterHeader("联系人", Format = "", Width = 25, IsBold = true)]
    public string? CustomerLinkman { get; set; }
    
    /// <summary>
    /// 联系电话
    /// </summary>
    [ImporterHeader(Name = "联系电话")]
    [ExporterHeader("联系电话", Format = "", Width = 25, IsBold = true)]
    public string? CustomerPhone { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? Remark { get; set; }
    
}
