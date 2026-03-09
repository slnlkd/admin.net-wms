// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Core.Service;

/// <summary>
/// 系统角色菜单快捷方式配置表基础输入参数
/// </summary>
public class SysMenuQuickActionsBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 角色Id
    /// </summary>
    [Required(ErrorMessage = "角色Id不能为空")]
    public virtual long? RoleId { get; set; }
    
    /// <summary>
    /// 角色菜单Id
    /// </summary>
    [Required(ErrorMessage = "角色菜单Id不能为空")]
    public virtual long? MenuId { get; set; }
    
}

/// <summary>
/// 系统角色菜单快捷方式配置表分页查询输入参数
/// </summary>
public class PageSysMenuQuickActionsInput : BasePageInput
{
    /// <summary>
    /// 角色Id
    /// </summary>
    public long? RoleId { get; set; }
    
    /// <summary>
    /// 角色菜单Id
    /// </summary>
    public long? MenuId { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 系统角色菜单快捷方式配置表增加输入参数
/// </summary>
public class AddSysMenuQuickActionsInput
{
    /// <summary>
    /// 角色Id
    /// </summary>
    [Required(ErrorMessage = "角色Id不能为空")]
    public long? RoleId { get; set; }
    
    /// <summary>
    /// 角色菜单Id
    /// </summary>
    [Required(ErrorMessage = "角色菜单Id不能为空")]
    public long? MenuId { get; set; }
    
}

/// <summary>
/// 系统角色菜单快捷方式配置表删除输入参数
/// </summary>
public class DeleteSysMenuQuickActionsInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 系统角色菜单快捷方式配置表更新输入参数
/// </summary>
public class UpdateSysMenuQuickActionsInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 角色Id
    /// </summary>    
    [Required(ErrorMessage = "角色Id不能为空")]
    public long? RoleId { get; set; }
    
    /// <summary>
    /// 角色菜单Id
    /// </summary>    
    [Required(ErrorMessage = "角色菜单Id不能为空")]
    public long? MenuId { get; set; }
    
}

/// <summary>
/// 系统角色菜单快捷方式配置表主键查询输入参数
/// </summary>
public class QueryByIdSysMenuQuickActionsInput : DeleteSysMenuQuickActionsInput
{
}

/// <summary>
/// 系统角色菜单快捷方式配置表数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportSysMenuQuickActionsInput : BaseImportInput
{
    /// <summary>
    /// 角色Id
    /// </summary>
    [ImporterHeader(Name = "*角色Id")]
    [ExporterHeader("*角色Id", Format = "", Width = 25, IsBold = true)]
    public long? RoleId { get; set; }
    
    /// <summary>
    /// 角色菜单Id
    /// </summary>
    [ImporterHeader(Name = "*角色菜单Id")]
    [ExporterHeader("*角色菜单Id", Format = "", Width = 25, IsBold = true)]
    public long? MenuId { get; set; }
    
}
