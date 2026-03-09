// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Application;

/// <summary>
/// 密码策略输出参数
/// </summary>
public class WmsPasswordPolicyDto
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
    
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
    public int MinLength { get; set; }
    
    /// <summary>
    /// 管理员密码最小长度
    /// </summary>
    public int AdminMinLength { get; set; }
    
    /// <summary>
    /// 需要包含的字符类别数量（大写、小写、数字、特殊字符）
    /// </summary>
    public int RequireCategories { get; set; }
    
    /// <summary>
    /// 最大连续相同字符数
    /// </summary>
    public int MaxConsecutive { get; set; }
    
    /// <summary>
    /// 是否必须包含小写字母
    /// </summary>
    public bool RequireLowercase { get; set; }
    
    /// <summary>
    /// 是否必须包含大写字母
    /// </summary>
    public bool RequireUppercase { get; set; }
    
    /// <summary>
    /// 是否必须包含数字
    /// </summary>
    public bool RequireDigit { get; set; }
    
    /// <summary>
    /// 是否必须包含特殊字符
    /// </summary>
    public bool RequireSpecial { get; set; }
    
    /// <summary>
    /// 允许的特殊字符集合
    /// </summary>
    public string SpecialChars { get; set; }
    
    /// <summary>
    /// 是否检查密码不能包含用户名
    /// </summary>
    public bool CheckUserId { get; set; }
    
    /// <summary>
    /// 是否检查连续字符
    /// </summary>
    public bool CheckConsecutive { get; set; }
    
    /// <summary>
    /// 是否检查常见模式（如123456、qwerty等）
    /// </summary>
    public bool CheckCommonPatterns { get; set; }
    
    /// <summary>
    /// 是否检查重复字符
    /// </summary>
    public bool CheckRepeatedChars { get; set; }
    
    /// <summary>
    /// 密码历史记录数量，不能重复使用
    /// </summary>
    public int RememberPasswords { get; set; }
    
    /// <summary>
    /// 普通用户密码过期天数
    /// </summary>
    public int PasswordExpiryDays { get; set; }
    
    /// <summary>
    /// 管理员密码过期天数
    /// </summary>
    public int AdminPasswordExpiryDays { get; set; }
    
    /// <summary>
    /// 密码过期提前提醒天数
    /// </summary>
    public int ExpiryNoticeDays { get; set; }
    
    /// <summary>
    /// 是否强制首次登录修改密码
    /// </summary>
    public bool ForceChangeInitial { get; set; }
    
    /// <summary>
    /// 策略是否启用
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// 创建者Id
    /// </summary>
    public long CreateUserId { get; set; }
    
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }
    
    /// <summary>
    /// 修改者Id
    /// </summary>
    public long UpdateUserId { get; set; }
    
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdateTime { get; set; }
    
}
