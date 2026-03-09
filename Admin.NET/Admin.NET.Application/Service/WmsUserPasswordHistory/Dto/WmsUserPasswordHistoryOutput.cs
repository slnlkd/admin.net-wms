// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using Magicodes.ExporterAndImporter.Core;
namespace Admin.NET.Application;

/// <summary>
/// 用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用输出参数
/// </summary>
public class WmsUserPasswordHistoryOutput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }    
    
    /// <summary>
    /// 用户ID
    /// </summary>
    public string UserId { get; set; }    
    
    /// <summary>
    /// 密码哈希值
    /// </summary>
    public string PasswordHash { get; set; }    
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }    
    
}

/// <summary>
/// 用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用数据导入模板实体
/// </summary>
public class ExportWmsUserPasswordHistoryOutput : ImportWmsUserPasswordHistoryInput
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public override string Error { get; set; }
}
