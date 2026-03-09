// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 密码策略配置表 - 存储系统密码复杂度策略和安全性配置
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsPasswordPolicy", "密码策略配置表 - 存储系统密码复杂度策略和安全性配置")]
public partial class WmsPasswordPolicy : EntityBase
{
    /// <summary>
    /// 策略名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "Name", ColumnDescription = "策略名称", Length = 100)]
    public virtual string Name { get; set; }
    
    /// <summary>
    /// 策略描述
    /// </summary>
    [SugarColumn(ColumnName = "Description", ColumnDescription = "策略描述", Length = 500)]
    public virtual string? Description { get; set; }
    
    /// <summary>
    /// 普通用户密码最小长度
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "MinLength", ColumnDescription = "普通用户密码最小长度", DefaultValue = "((8))")]
    public virtual int MinLength { get; set; }
    
    /// <summary>
    /// 管理员密码最小长度
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "AdminMinLength", ColumnDescription = "管理员密码最小长度", DefaultValue = "((14))")]
    public virtual int AdminMinLength { get; set; }
    
    /// <summary>
    /// 需要包含的字符类别数量（大写、小写、数字、特殊字符）
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "RequireCategories", ColumnDescription = "需要包含的字符类别数量（大写、小写、数字、特殊字符）", DefaultValue = "((3))")]
    public virtual int RequireCategories { get; set; }
    
    /// <summary>
    /// 最大连续相同字符数
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "MaxConsecutive", ColumnDescription = "最大连续相同字符数", DefaultValue = "((3))")]
    public virtual int MaxConsecutive { get; set; }
    
    /// <summary>
    /// 是否必须包含小写字母
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "RequireLowercase", ColumnDescription = "是否必须包含小写字母", DefaultValue = "((1))")]
    public virtual bool RequireLowercase { get; set; }
    
    /// <summary>
    /// 是否必须包含大写字母
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "RequireUppercase", ColumnDescription = "是否必须包含大写字母", DefaultValue = "((1))")]
    public virtual bool RequireUppercase { get; set; }
    
    /// <summary>
    /// 是否必须包含数字
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "RequireDigit", ColumnDescription = "是否必须包含数字", DefaultValue = "((1))")]
    public virtual bool RequireDigit { get; set; }
    
    /// <summary>
    /// 是否必须包含特殊字符
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "RequireSpecial", ColumnDescription = "是否必须包含特殊字符", DefaultValue = "((1))")]
    public virtual bool RequireSpecial { get; set; }
    
    /// <summary>
    /// 允许的特殊字符集合
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "SpecialChars", ColumnDescription = "允许的特殊字符集合", Length = 100, DefaultValue = @"('!@#$%^&*()_+-=[]{};'':""|,.<>/?')")]
    public virtual string SpecialChars { get; set; }
    
    /// <summary>
    /// 是否检查密码不能包含用户名
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "CheckUserId", ColumnDescription = "是否检查密码不能包含用户名", DefaultValue = "((1))")]
    public virtual bool CheckUserId { get; set; }
    
    /// <summary>
    /// 是否检查连续字符
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "CheckConsecutive", ColumnDescription = "是否检查连续字符", DefaultValue = "((1))")]
    public virtual bool CheckConsecutive { get; set; }
    
    /// <summary>
    /// 是否检查常见模式（如123456、qwerty等）
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "CheckCommonPatterns", ColumnDescription = "是否检查常见模式（如123456、qwerty等）", DefaultValue = "((1))")]
    public virtual bool CheckCommonPatterns { get; set; }
    
    /// <summary>
    /// 是否检查重复字符
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "CheckRepeatedChars", ColumnDescription = "是否检查重复字符", DefaultValue = "((1))")]
    public virtual bool CheckRepeatedChars { get; set; }
    
    /// <summary>
    /// 密码历史记录数量，不能重复使用
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "RememberPasswords", ColumnDescription = "密码历史记录数量，不能重复使用", DefaultValue = "((5))")]
    public virtual int RememberPasswords { get; set; }
    
    /// <summary>
    /// 普通用户密码过期天数
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "PasswordExpiryDays", ColumnDescription = "普通用户密码过期天数", DefaultValue = "((90))")]
    public virtual int PasswordExpiryDays { get; set; }
    
    /// <summary>
    /// 管理员密码过期天数
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "AdminPasswordExpiryDays", ColumnDescription = "管理员密码过期天数", DefaultValue = "((90))")]
    public virtual int AdminPasswordExpiryDays { get; set; }
    
    /// <summary>
    /// 密码过期提前提醒天数
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "ExpiryNoticeDays", ColumnDescription = "密码过期提前提醒天数", DefaultValue = "((7))")]
    public virtual int ExpiryNoticeDays { get; set; }
    
    /// <summary>
    /// 是否强制首次登录修改密码
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "ForceChangeInitial", ColumnDescription = "是否强制首次登录修改密码", DefaultValue = "((1))")]
    public virtual bool ForceChangeInitial { get; set; }
    
    /// <summary>
    /// 策略是否启用
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "Enabled", ColumnDescription = "策略是否启用", DefaultValue = "((1))")]
    public virtual bool Enabled { get; set; }
    
}
