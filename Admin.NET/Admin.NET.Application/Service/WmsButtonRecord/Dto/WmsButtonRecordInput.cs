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
/// 按钮点击表基础输入参数
/// </summary>
public class WmsButtonRecordBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 用户Id
    /// </summary>
    public virtual long? UserId { get; set; }
    
    /// <summary>
    /// 按钮点击时间
    /// </summary>
    public virtual DateTime? ClickButtonDate { get; set; }
    
    /// <summary>
    /// 记录按钮信息（ 用户名+按钮点击时间+记录按钮信息）
    /// </summary>
    public virtual string? ButtonInformation { get; set; }
    
    /// <summary>
    /// 用户编码
    /// </summary>
    public virtual string? UserCode { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public virtual string? UserName { get; set; }
    
    /// <summary>
    /// 菜单名称
    /// </summary>
    public virtual string? PageName { get; set; }
    
}

/// <summary>
/// 按钮点击表分页查询输入参数
/// </summary>
public class PageWmsButtonRecordInput : BasePageInput
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public long? UserId { get; set; }
    
    /// <summary>
    /// 按钮点击时间范围
    /// </summary>
     public DateTime?[] ClickButtonDateRange { get; set; }
    
    /// <summary>
    /// 记录按钮信息（ 用户名+按钮点击时间+记录按钮信息）
    /// </summary>
    public string? ButtonInformation { get; set; }
    
    /// <summary>
    /// 用户编码
    /// </summary>
    public string? UserCode { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// 菜单名称
    /// </summary>
    public string? PageName { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 按钮点击表增加输入参数
/// </summary>
public class AddWmsButtonRecordInput
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// 按钮点击时间
    /// </summary>
    public DateTime? ClickButtonDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 记录按钮信息（ 用户名+按钮点击时间+记录按钮信息）
    /// </summary>
    [MaxLength(500, ErrorMessage = "记录按钮信息（ 用户名+按钮点击时间+记录按钮信息）字符长度不能超过500")]
    public string? ButtonInformation { get; set; }
    
    /// <summary>
    /// 用户编码
    /// </summary>
    [MaxLength(50, ErrorMessage = "用户编码字符长度不能超过50")]
    public string? UserCode { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(50, ErrorMessage = "用户名字符长度不能超过50")]
    public string? UserName { get; set; }
    
    /// <summary>
    /// 菜单名称
    /// </summary>
    [MaxLength(50, ErrorMessage = "菜单名称字符长度不能超过50")]
    public string? PageName { get; set; }
    
}

/// <summary>
/// 按钮点击表删除输入参数
/// </summary>
public class DeleteWmsButtonRecordInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 按钮点击表更新输入参数
/// </summary>
public class UpdateWmsButtonRecordInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 用户Id
    /// </summary>    
    public long? UserId { get; set; }
    
    /// <summary>
    /// 按钮点击时间
    /// </summary>    
    public DateTime? ClickButtonDate { get; set; }
    
    /// <summary>
    /// 记录按钮信息（ 用户名+按钮点击时间+记录按钮信息）
    /// </summary>    
    [MaxLength(500, ErrorMessage = "记录按钮信息（ 用户名+按钮点击时间+记录按钮信息）字符长度不能超过500")]
    public string? ButtonInformation { get; set; }
    
    /// <summary>
    /// 用户编码
    /// </summary>    
    [MaxLength(50, ErrorMessage = "用户编码字符长度不能超过50")]
    public string? UserCode { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>    
    [MaxLength(50, ErrorMessage = "用户名字符长度不能超过50")]
    public string? UserName { get; set; }
    
    /// <summary>
    /// 菜单名称
    /// </summary>    
    [MaxLength(50, ErrorMessage = "菜单名称字符长度不能超过50")]
    public string? PageName { get; set; }
    
}

/// <summary>
/// 按钮点击表主键查询输入参数
/// </summary>
public class QueryByIdWmsButtonRecordInput : DeleteWmsButtonRecordInput
{
}

/// <summary>
/// 按钮点击表数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsButtonRecordInput : BaseImportInput
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [ImporterHeader(Name = "用户Id")]
    [ExporterHeader("用户Id", Format = "", Width = 25, IsBold = true)]
    public long? UserId { get; set; }
    
    /// <summary>
    /// 按钮点击时间
    /// </summary>
    [ImporterHeader(Name = "按钮点击时间")]
    [ExporterHeader("按钮点击时间", Format = "", Width = 25, IsBold = true)]
    public DateTime? ClickButtonDate { get; set; }
    
    /// <summary>
    /// 记录按钮信息（ 用户名+按钮点击时间+记录按钮信息）
    /// </summary>
    [ImporterHeader(Name = "记录按钮信息（ 用户名+按钮点击时间+记录按钮信息）")]
    [ExporterHeader("记录按钮信息（ 用户名+按钮点击时间+记录按钮信息）", Format = "", Width = 25, IsBold = true)]
    public string? ButtonInformation { get; set; }
    
    /// <summary>
    /// 用户编码
    /// </summary>
    [ImporterHeader(Name = "用户编码")]
    [ExporterHeader("用户编码", Format = "", Width = 25, IsBold = true)]
    public string? UserCode { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    [ImporterHeader(Name = "用户名")]
    [ExporterHeader("用户名", Format = "", Width = 25, IsBold = true)]
    public string? UserName { get; set; }
    
    /// <summary>
    /// 菜单名称
    /// </summary>
    [ImporterHeader(Name = "菜单名称")]
    [ExporterHeader("菜单名称", Format = "", Width = 25, IsBold = true)]
    public string? PageName { get; set; }
    
}
