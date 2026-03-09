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
/// 密码策略基础输入参数
/// </summary>
public class WmsPasswordPolicyBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 策略名称
    /// </summary>
    [Required(ErrorMessage = "策略名称不能为空")]
    public virtual string Name { get; set; }
    
    /// <summary>
    /// 策略描述
    /// </summary>
    public virtual string? Description { get; set; }
    
    /// <summary>
    /// 普通用户密码最小长度
    /// </summary>
    [Required(ErrorMessage = "普通用户密码最小长度不能为空")]
    public virtual int? MinLength { get; set; }
    
    /// <summary>
    /// 管理员密码最小长度
    /// </summary>
    [Required(ErrorMessage = "管理员密码最小长度不能为空")]
    public virtual int? AdminMinLength { get; set; }
    
    /// <summary>
    /// 需要包含的字符类别数量（大写、小写、数字、特殊字符）
    /// </summary>
    [Required(ErrorMessage = "需要包含的字符类别数量（大写、小写、数字、特殊字符）不能为空")]
    public virtual int? RequireCategories { get; set; }
    
    /// <summary>
    /// 最大连续相同字符数
    /// </summary>
    [Required(ErrorMessage = "最大连续相同字符数不能为空")]
    public virtual int? MaxConsecutive { get; set; }
    
    /// <summary>
    /// 是否必须包含小写字母
    /// </summary>
    [Required(ErrorMessage = "是否必须包含小写字母不能为空")]
    public virtual bool? RequireLowercase { get; set; }
    
    /// <summary>
    /// 是否必须包含大写字母
    /// </summary>
    [Required(ErrorMessage = "是否必须包含大写字母不能为空")]
    public virtual bool? RequireUppercase { get; set; }
    
    /// <summary>
    /// 是否必须包含数字
    /// </summary>
    [Required(ErrorMessage = "是否必须包含数字不能为空")]
    public virtual bool? RequireDigit { get; set; }
    
    /// <summary>
    /// 是否必须包含特殊字符
    /// </summary>
    [Required(ErrorMessage = "是否必须包含特殊字符不能为空")]
    public virtual bool? RequireSpecial { get; set; }
    
    /// <summary>
    /// 允许的特殊字符集合
    /// </summary>
    [Required(ErrorMessage = "允许的特殊字符集合不能为空")]
    public virtual string SpecialChars { get; set; }
    
    /// <summary>
    /// 是否检查密码不能包含用户名
    /// </summary>
    [Required(ErrorMessage = "是否检查密码不能包含用户名不能为空")]
    public virtual bool? CheckUserId { get; set; }
    
    /// <summary>
    /// 是否检查连续字符
    /// </summary>
    [Required(ErrorMessage = "是否检查连续字符不能为空")]
    public virtual bool? CheckConsecutive { get; set; }
    
    /// <summary>
    /// 是否检查常见模式（如123456、qwerty等）
    /// </summary>
    [Required(ErrorMessage = "是否检查常见模式（如123456、qwerty等）不能为空")]
    public virtual bool? CheckCommonPatterns { get; set; }
    
    /// <summary>
    /// 是否检查重复字符
    /// </summary>
    [Required(ErrorMessage = "是否检查重复字符不能为空")]
    public virtual bool? CheckRepeatedChars { get; set; }
    
    /// <summary>
    /// 密码历史记录数量，不能重复使用
    /// </summary>
    [Required(ErrorMessage = "密码历史记录数量，不能重复使用不能为空")]
    public virtual int? RememberPasswords { get; set; }
    
    /// <summary>
    /// 普通用户密码过期天数
    /// </summary>
    [Required(ErrorMessage = "普通用户密码过期天数不能为空")]
    public virtual int? PasswordExpiryDays { get; set; }
    
    /// <summary>
    /// 管理员密码过期天数
    /// </summary>
    [Required(ErrorMessage = "管理员密码过期天数不能为空")]
    public virtual int? AdminPasswordExpiryDays { get; set; }
    
    /// <summary>
    /// 密码过期提前提醒天数
    /// </summary>
    [Required(ErrorMessage = "密码过期提前提醒天数不能为空")]
    public virtual int? ExpiryNoticeDays { get; set; }
    
    /// <summary>
    /// 是否强制首次登录修改密码
    /// </summary>
    [Required(ErrorMessage = "是否强制首次登录修改密码不能为空")]
    public virtual bool? ForceChangeInitial { get; set; }
    
    /// <summary>
    /// 策略是否启用
    /// </summary>
    [Required(ErrorMessage = "策略是否启用不能为空")]
    public virtual bool? Enabled { get; set; }
    
}

/// <summary>
/// 密码策略分页查询输入参数
/// </summary>
public class PageWmsPasswordPolicyInput : BasePageInput
{
    /// <summary>
    /// 策略名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 策略描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 普通用户密码最小长度
    /// </summary>
    public int? MinLength { get; set; }
    
    /// <summary>
    /// 管理员密码最小长度
    /// </summary>
    public int? AdminMinLength { get; set; }
    
    /// <summary>
    /// 需要包含的字符类别数量（大写、小写、数字、特殊字符）
    /// </summary>
    public int? RequireCategories { get; set; }
    
    /// <summary>
    /// 最大连续相同字符数
    /// </summary>
    public int? MaxConsecutive { get; set; }
    
    /// <summary>
    /// 是否必须包含小写字母
    /// </summary>
    public bool? RequireLowercase { get; set; }
    
    /// <summary>
    /// 是否必须包含大写字母
    /// </summary>
    public bool? RequireUppercase { get; set; }
    
    /// <summary>
    /// 是否必须包含数字
    /// </summary>
    public bool? RequireDigit { get; set; }
    
    /// <summary>
    /// 是否必须包含特殊字符
    /// </summary>
    public bool? RequireSpecial { get; set; }
    
    /// <summary>
    /// 允许的特殊字符集合
    /// </summary>
    public string SpecialChars { get; set; }
    
    /// <summary>
    /// 是否检查密码不能包含用户名
    /// </summary>
    public bool? CheckUserId { get; set; }
    
    /// <summary>
    /// 是否检查连续字符
    /// </summary>
    public bool? CheckConsecutive { get; set; }
    
    /// <summary>
    /// 是否检查常见模式（如123456、qwerty等）
    /// </summary>
    public bool? CheckCommonPatterns { get; set; }
    
    /// <summary>
    /// 是否检查重复字符
    /// </summary>
    public bool? CheckRepeatedChars { get; set; }
    
    /// <summary>
    /// 密码历史记录数量，不能重复使用
    /// </summary>
    public int? RememberPasswords { get; set; }
    
    /// <summary>
    /// 普通用户密码过期天数
    /// </summary>
    public int? PasswordExpiryDays { get; set; }
    
    /// <summary>
    /// 管理员密码过期天数
    /// </summary>
    public int? AdminPasswordExpiryDays { get; set; }
    
    /// <summary>
    /// 密码过期提前提醒天数
    /// </summary>
    public int? ExpiryNoticeDays { get; set; }
    
    /// <summary>
    /// 是否强制首次登录修改密码
    /// </summary>
    public bool? ForceChangeInitial { get; set; }
    
    /// <summary>
    /// 策略是否启用
    /// </summary>
    public bool? Enabled { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 密码策略增加输入参数
/// </summary>
public class AddWmsPasswordPolicyInput
{
    /// <summary>
    /// 策略名称
    /// </summary>
    [Required(ErrorMessage = "策略名称不能为空")]
    [MaxLength(100, ErrorMessage = "策略名称字符长度不能超过100")]
    public string Name { get; set; }
    
    /// <summary>
    /// 策略描述
    /// </summary>
    [MaxLength(500, ErrorMessage = "策略描述字符长度不能超过500")]
    public string? Description { get; set; }
    
    /// <summary>
    /// 普通用户密码最小长度
    /// </summary>
    [Required(ErrorMessage = "普通用户密码最小长度不能为空")]
    public int? MinLength { get; set; }
    
    /// <summary>
    /// 管理员密码最小长度
    /// </summary>
    [Required(ErrorMessage = "管理员密码最小长度不能为空")]
    public int? AdminMinLength { get; set; }
    
    /// <summary>
    /// 需要包含的字符类别数量（大写、小写、数字、特殊字符）
    /// </summary>
    [Required(ErrorMessage = "需要包含的字符类别数量（大写、小写、数字、特殊字符）不能为空")]
    public int? RequireCategories { get; set; }
    
    /// <summary>
    /// 最大连续相同字符数
    /// </summary>
    [Required(ErrorMessage = "最大连续相同字符数不能为空")]
    public int? MaxConsecutive { get; set; }
    
    /// <summary>
    /// 是否必须包含小写字母
    /// </summary>
    [Required(ErrorMessage = "是否必须包含小写字母不能为空")]
    public bool? RequireLowercase { get; set; }
    
    /// <summary>
    /// 是否必须包含大写字母
    /// </summary>
    [Required(ErrorMessage = "是否必须包含大写字母不能为空")]
    public bool? RequireUppercase { get; set; }
    
    /// <summary>
    /// 是否必须包含数字
    /// </summary>
    [Required(ErrorMessage = "是否必须包含数字不能为空")]
    public bool? RequireDigit { get; set; }
    
    /// <summary>
    /// 是否必须包含特殊字符
    /// </summary>
    [Required(ErrorMessage = "是否必须包含特殊字符不能为空")]
    public bool? RequireSpecial { get; set; }
    
    /// <summary>
    /// 允许的特殊字符集合
    /// </summary>
    [Required(ErrorMessage = "允许的特殊字符集合不能为空")]
    [MaxLength(100, ErrorMessage = "允许的特殊字符集合字符长度不能超过100")]
    public string SpecialChars { get; set; }
    
    /// <summary>
    /// 是否检查密码不能包含用户名
    /// </summary>
    [Required(ErrorMessage = "是否检查密码不能包含用户名不能为空")]
    public bool? CheckUserId { get; set; }
    
    /// <summary>
    /// 是否检查连续字符
    /// </summary>
    [Required(ErrorMessage = "是否检查连续字符不能为空")]
    public bool? CheckConsecutive { get; set; }
    
    /// <summary>
    /// 是否检查常见模式（如123456、qwerty等）
    /// </summary>
    [Required(ErrorMessage = "是否检查常见模式（如123456、qwerty等）不能为空")]
    public bool? CheckCommonPatterns { get; set; }
    
    /// <summary>
    /// 是否检查重复字符
    /// </summary>
    [Required(ErrorMessage = "是否检查重复字符不能为空")]
    public bool? CheckRepeatedChars { get; set; }
    
    /// <summary>
    /// 密码历史记录数量，不能重复使用
    /// </summary>
    [Required(ErrorMessage = "密码历史记录数量，不能重复使用不能为空")]
    public int? RememberPasswords { get; set; }
    
    /// <summary>
    /// 普通用户密码过期天数
    /// </summary>
    [Required(ErrorMessage = "普通用户密码过期天数不能为空")]
    public int? PasswordExpiryDays { get; set; }
    
    /// <summary>
    /// 管理员密码过期天数
    /// </summary>
    [Required(ErrorMessage = "管理员密码过期天数不能为空")]
    public int? AdminPasswordExpiryDays { get; set; }
    
    /// <summary>
    /// 密码过期提前提醒天数
    /// </summary>
    [Required(ErrorMessage = "密码过期提前提醒天数不能为空")]
    public int? ExpiryNoticeDays { get; set; }
    
    /// <summary>
    /// 是否强制首次登录修改密码
    /// </summary>
    [Required(ErrorMessage = "是否强制首次登录修改密码不能为空")]
    public bool? ForceChangeInitial { get; set; }
    
    /// <summary>
    /// 策略是否启用
    /// </summary>
    [Required(ErrorMessage = "策略是否启用不能为空")]
    public bool? Enabled { get; set; }
    
}

/// <summary>
/// 密码策略删除输入参数
/// </summary>
public class DeleteWmsPasswordPolicyInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 密码策略更新输入参数
/// </summary>
public class UpdateWmsPasswordPolicyInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 策略名称
    /// </summary>    
    [Required(ErrorMessage = "策略名称不能为空")]
    [MaxLength(100, ErrorMessage = "策略名称字符长度不能超过100")]
    public string Name { get; set; }
    
    /// <summary>
    /// 策略描述
    /// </summary>    
    [MaxLength(500, ErrorMessage = "策略描述字符长度不能超过500")]
    public string? Description { get; set; }
    
    /// <summary>
    /// 普通用户密码最小长度
    /// </summary>    
    [Required(ErrorMessage = "普通用户密码最小长度不能为空")]
    public int? MinLength { get; set; }
    
    /// <summary>
    /// 管理员密码最小长度
    /// </summary>    
    [Required(ErrorMessage = "管理员密码最小长度不能为空")]
    public int? AdminMinLength { get; set; }
    
    /// <summary>
    /// 需要包含的字符类别数量（大写、小写、数字、特殊字符）
    /// </summary>    
    [Required(ErrorMessage = "需要包含的字符类别数量（大写、小写、数字、特殊字符）不能为空")]
    public int? RequireCategories { get; set; }
    
    /// <summary>
    /// 最大连续相同字符数
    /// </summary>    
    [Required(ErrorMessage = "最大连续相同字符数不能为空")]
    public int? MaxConsecutive { get; set; }
    
    /// <summary>
    /// 是否必须包含小写字母
    /// </summary>    
    [Required(ErrorMessage = "是否必须包含小写字母不能为空")]
    public bool? RequireLowercase { get; set; }
    
    /// <summary>
    /// 是否必须包含大写字母
    /// </summary>    
    [Required(ErrorMessage = "是否必须包含大写字母不能为空")]
    public bool? RequireUppercase { get; set; }
    
    /// <summary>
    /// 是否必须包含数字
    /// </summary>    
    [Required(ErrorMessage = "是否必须包含数字不能为空")]
    public bool? RequireDigit { get; set; }
    
    /// <summary>
    /// 是否必须包含特殊字符
    /// </summary>    
    [Required(ErrorMessage = "是否必须包含特殊字符不能为空")]
    public bool? RequireSpecial { get; set; }
    
    /// <summary>
    /// 允许的特殊字符集合
    /// </summary>    
    [Required(ErrorMessage = "允许的特殊字符集合不能为空")]
    [MaxLength(100, ErrorMessage = "允许的特殊字符集合字符长度不能超过100")]
    public string SpecialChars { get; set; }
    
    /// <summary>
    /// 是否检查密码不能包含用户名
    /// </summary>    
    [Required(ErrorMessage = "是否检查密码不能包含用户名不能为空")]
    public bool? CheckUserId { get; set; }
    
    /// <summary>
    /// 是否检查连续字符
    /// </summary>    
    [Required(ErrorMessage = "是否检查连续字符不能为空")]
    public bool? CheckConsecutive { get; set; }
    
    /// <summary>
    /// 是否检查常见模式（如123456、qwerty等）
    /// </summary>    
    [Required(ErrorMessage = "是否检查常见模式（如123456、qwerty等）不能为空")]
    public bool? CheckCommonPatterns { get; set; }
    
    /// <summary>
    /// 是否检查重复字符
    /// </summary>    
    [Required(ErrorMessage = "是否检查重复字符不能为空")]
    public bool? CheckRepeatedChars { get; set; }
    
    /// <summary>
    /// 密码历史记录数量，不能重复使用
    /// </summary>    
    [Required(ErrorMessage = "密码历史记录数量，不能重复使用不能为空")]
    public int? RememberPasswords { get; set; }
    
    /// <summary>
    /// 普通用户密码过期天数
    /// </summary>    
    [Required(ErrorMessage = "普通用户密码过期天数不能为空")]
    public int? PasswordExpiryDays { get; set; }
    
    /// <summary>
    /// 管理员密码过期天数
    /// </summary>    
    [Required(ErrorMessage = "管理员密码过期天数不能为空")]
    public int? AdminPasswordExpiryDays { get; set; }
    
    /// <summary>
    /// 密码过期提前提醒天数
    /// </summary>    
    [Required(ErrorMessage = "密码过期提前提醒天数不能为空")]
    public int? ExpiryNoticeDays { get; set; }
    
    /// <summary>
    /// 是否强制首次登录修改密码
    /// </summary>    
    [Required(ErrorMessage = "是否强制首次登录修改密码不能为空")]
    public bool? ForceChangeInitial { get; set; }
    
    /// <summary>
    /// 策略是否启用
    /// </summary>    
    [Required(ErrorMessage = "策略是否启用不能为空")]
    public bool? Enabled { get; set; }
    
}

/// <summary>
/// 密码策略主键查询输入参数
/// </summary>
public class QueryByIdWmsPasswordPolicyInput : DeleteWmsPasswordPolicyInput
{
}

/// <summary>
/// 密码策略数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsPasswordPolicyInput : BaseImportInput
{
    /// <summary>
    /// 策略名称
    /// </summary>
    [ImporterHeader(Name = "*策略名称")]
    [ExporterHeader("*策略名称", Format = "", Width = 25, IsBold = true)]
    public string Name { get; set; }
    
    /// <summary>
    /// 策略描述
    /// </summary>
    [ImporterHeader(Name = "策略描述")]
    [ExporterHeader("策略描述", Format = "", Width = 25, IsBold = true)]
    public string? Description { get; set; }
    
    /// <summary>
    /// 普通用户密码最小长度
    /// </summary>
    [ImporterHeader(Name = "*普通用户密码最小长度")]
    [ExporterHeader("*普通用户密码最小长度", Format = "", Width = 25, IsBold = true)]
    public int? MinLength { get; set; }
    
    /// <summary>
    /// 管理员密码最小长度
    /// </summary>
    [ImporterHeader(Name = "*管理员密码最小长度")]
    [ExporterHeader("*管理员密码最小长度", Format = "", Width = 25, IsBold = true)]
    public int? AdminMinLength { get; set; }
    
    /// <summary>
    /// 需要包含的字符类别数量（大写、小写、数字、特殊字符）
    /// </summary>
    [ImporterHeader(Name = "*需要包含的字符类别数量（大写、小写、数字、特殊字符）")]
    [ExporterHeader("*需要包含的字符类别数量（大写、小写、数字、特殊字符）", Format = "", Width = 25, IsBold = true)]
    public int? RequireCategories { get; set; }
    
    /// <summary>
    /// 最大连续相同字符数
    /// </summary>
    [ImporterHeader(Name = "*最大连续相同字符数")]
    [ExporterHeader("*最大连续相同字符数", Format = "", Width = 25, IsBold = true)]
    public int? MaxConsecutive { get; set; }
    
    /// <summary>
    /// 是否必须包含小写字母
    /// </summary>
    [ImporterHeader(Name = "*是否必须包含小写字母")]
    [ExporterHeader("*是否必须包含小写字母", Format = "", Width = 25, IsBold = true)]
    public bool? RequireLowercase { get; set; }
    
    /// <summary>
    /// 是否必须包含大写字母
    /// </summary>
    [ImporterHeader(Name = "*是否必须包含大写字母")]
    [ExporterHeader("*是否必须包含大写字母", Format = "", Width = 25, IsBold = true)]
    public bool? RequireUppercase { get; set; }
    
    /// <summary>
    /// 是否必须包含数字
    /// </summary>
    [ImporterHeader(Name = "*是否必须包含数字")]
    [ExporterHeader("*是否必须包含数字", Format = "", Width = 25, IsBold = true)]
    public bool? RequireDigit { get; set; }
    
    /// <summary>
    /// 是否必须包含特殊字符
    /// </summary>
    [ImporterHeader(Name = "*是否必须包含特殊字符")]
    [ExporterHeader("*是否必须包含特殊字符", Format = "", Width = 25, IsBold = true)]
    public bool? RequireSpecial { get; set; }
    
    /// <summary>
    /// 允许的特殊字符集合
    /// </summary>
    [ImporterHeader(Name = "*允许的特殊字符集合")]
    [ExporterHeader("*允许的特殊字符集合", Format = "", Width = 25, IsBold = true)]
    public string SpecialChars { get; set; }
    
    /// <summary>
    /// 是否检查密码不能包含用户名
    /// </summary>
    [ImporterHeader(Name = "*是否检查密码不能包含用户名")]
    [ExporterHeader("*是否检查密码不能包含用户名", Format = "", Width = 25, IsBold = true)]
    public bool? CheckUserId { get; set; }
    
    /// <summary>
    /// 是否检查连续字符
    /// </summary>
    [ImporterHeader(Name = "*是否检查连续字符")]
    [ExporterHeader("*是否检查连续字符", Format = "", Width = 25, IsBold = true)]
    public bool? CheckConsecutive { get; set; }
    
    /// <summary>
    /// 是否检查常见模式（如123456、qwerty等）
    /// </summary>
    [ImporterHeader(Name = "*是否检查常见模式（如123456、qwerty等）")]
    [ExporterHeader("*是否检查常见模式（如123456、qwerty等）", Format = "", Width = 25, IsBold = true)]
    public bool? CheckCommonPatterns { get; set; }
    
    /// <summary>
    /// 是否检查重复字符
    /// </summary>
    [ImporterHeader(Name = "*是否检查重复字符")]
    [ExporterHeader("*是否检查重复字符", Format = "", Width = 25, IsBold = true)]
    public bool? CheckRepeatedChars { get; set; }
    
    /// <summary>
    /// 密码历史记录数量，不能重复使用
    /// </summary>
    [ImporterHeader(Name = "*密码历史记录数量，不能重复使用")]
    [ExporterHeader("*密码历史记录数量，不能重复使用", Format = "", Width = 25, IsBold = true)]
    public int? RememberPasswords { get; set; }
    
    /// <summary>
    /// 普通用户密码过期天数
    /// </summary>
    [ImporterHeader(Name = "*普通用户密码过期天数")]
    [ExporterHeader("*普通用户密码过期天数", Format = "", Width = 25, IsBold = true)]
    public int? PasswordExpiryDays { get; set; }
    
    /// <summary>
    /// 管理员密码过期天数
    /// </summary>
    [ImporterHeader(Name = "*管理员密码过期天数")]
    [ExporterHeader("*管理员密码过期天数", Format = "", Width = 25, IsBold = true)]
    public int? AdminPasswordExpiryDays { get; set; }
    
    /// <summary>
    /// 密码过期提前提醒天数
    /// </summary>
    [ImporterHeader(Name = "*密码过期提前提醒天数")]
    [ExporterHeader("*密码过期提前提醒天数", Format = "", Width = 25, IsBold = true)]
    public int? ExpiryNoticeDays { get; set; }
    
    /// <summary>
    /// 是否强制首次登录修改密码
    /// </summary>
    [ImporterHeader(Name = "*是否强制首次登录修改密码")]
    [ExporterHeader("*是否强制首次登录修改密码", Format = "", Width = 25, IsBold = true)]
    public bool? ForceChangeInitial { get; set; }
    
    /// <summary>
    /// 策略是否启用
    /// </summary>
    [ImporterHeader(Name = "*策略是否启用")]
    [ExporterHeader("*策略是否启用", Format = "", Width = 25, IsBold = true)]
    public bool? Enabled { get; set; }
    
}
