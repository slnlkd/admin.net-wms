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
/// 用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用基础输入参数
/// </summary>
public class WmsUserPasswordHistoryBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public virtual string UserId { get; set; }
    
    /// <summary>
    /// 密码哈希值
    /// </summary>
    [Required(ErrorMessage = "密码哈希值不能为空")]
    public virtual string PasswordHash { get; set; }
    
}

/// <summary>
/// 用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用分页查询输入参数
/// </summary>
public class PageWmsUserPasswordHistoryInput : BasePageInput
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public string UserId { get; set; }
    
    /// <summary>
    /// 密码哈希值
    /// </summary>
    public string PasswordHash { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用增加输入参数
/// </summary>
public class AddWmsUserPasswordHistoryInput
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    [MaxLength(50, ErrorMessage = "用户ID字符长度不能超过50")]
    public string UserId { get; set; }
    
    /// <summary>
    /// 密码哈希值
    /// </summary>
    [Required(ErrorMessage = "密码哈希值不能为空")]
    [MaxLength(255, ErrorMessage = "密码哈希值字符长度不能超过255")]
    public string PasswordHash { get; set; }
    
}

/// <summary>
/// 用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用删除输入参数
/// </summary>
public class DeleteWmsUserPasswordHistoryInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用更新输入参数
/// </summary>
public class UpdateWmsUserPasswordHistoryInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 用户ID
    /// </summary>    
    [Required(ErrorMessage = "用户ID不能为空")]
    [MaxLength(50, ErrorMessage = "用户ID字符长度不能超过50")]
    public string UserId { get; set; }
    
    /// <summary>
    /// 密码哈希值
    /// </summary>    
    [Required(ErrorMessage = "密码哈希值不能为空")]
    [MaxLength(255, ErrorMessage = "密码哈希值字符长度不能超过255")]
    public string PasswordHash { get; set; }
    
}

/// <summary>
/// 用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用主键查询输入参数
/// </summary>
public class QueryByIdWmsUserPasswordHistoryInput : DeleteWmsUserPasswordHistoryInput
{
}

/// <summary>
/// 用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsUserPasswordHistoryInput : BaseImportInput
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [ImporterHeader(Name = "*用户ID")]
    [ExporterHeader("*用户ID", Format = "", Width = 25, IsBold = true)]
    public string UserId { get; set; }
    
    /// <summary>
    /// 密码哈希值
    /// </summary>
    [ImporterHeader(Name = "*密码哈希值")]
    [ExporterHeader("*密码哈希值", Format = "", Width = 25, IsBold = true)]
    public string PasswordHash { get; set; }
    
}
