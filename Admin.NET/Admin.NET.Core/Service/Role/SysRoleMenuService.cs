// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Core.Service;

/// <summary>
/// 系统角色菜单服务
/// </summary>
public class SysRoleMenuService : ITransient
{
    private readonly SqlSugarRepository<SysRoleMenu> _sysRoleMenuRep;
    private readonly SysCacheService _sysCacheService;
    private readonly SqlSugarRepository<SysMenuQuickActions> _sysMenuQuickActionsRep;
    private readonly SqlSugarRepository<SysUserRole> _sysUserRoleRep;

    public SysRoleMenuService(SqlSugarRepository<SysRoleMenu> sysRoleMenuRep,
        SysCacheService sysCacheService,
        SqlSugarRepository<SysMenuQuickActions> sysMenuQuickActionsRep,
        SqlSugarRepository<SysUserRole> sysUserRoleRep)
    {
        _sysRoleMenuRep = sysRoleMenuRep;
        _sysCacheService = sysCacheService;
        _sysMenuQuickActionsRep = sysMenuQuickActionsRep;
        _sysUserRoleRep = sysUserRoleRep;
    }

    /// <summary>
    /// 根据角色Id集合获取快捷菜单集合(角色管理 反选查询）
    /// </summary>
    /// <param name="roleIdList"></param>
    /// <returns></returns>
    public async Task<List<long>> GetRoleQuickActionMenuList(List<long> roleIdList)
    {
        return await _sysMenuQuickActionsRep.AsQueryable()
            .Where(u => roleIdList.Contains(u.RoleId))
            .Select(u => u.MenuId).ToListAsync();
    }
    /// <summary>
    /// 根据角色Id集合获取菜单Id集合
    /// </summary>
    /// <param name="roleIdList"></param>
    /// <returns></returns>
    public async Task<List<long>> GetRoleMenuIdList(List<long> roleIdList)
    {
        return await _sysRoleMenuRep.AsQueryable()
            .Where(u => roleIdList.Contains(u.RoleId))
            .Select(u => u.MenuId).ToListAsync();
    }

    /// <summary>
    /// 授权角色菜单
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task GrantRoleMenu(RoleMenuInput input)
    {
        await _sysRoleMenuRep.DeleteAsync(u => u.RoleId == input.Id);

        // 追加父级菜单
        var allIdList = await _sysRoleMenuRep.Context.Queryable<SysMenu>().Select(u => new { u.Id, u.Pid }).ToListAsync();
        var pIdList = allIdList.ToChildList(u => u.Pid, u => u.Id, u => input.MenuIdList.Contains(u.Id)).Select(u => u.Pid).Distinct().ToList();
        input.MenuIdList = input.MenuIdList.Concat(pIdList).Distinct().Where(u => u != 0).ToList();

        // 保存授权数据
        var menus = input.MenuIdList.Select(u => new SysRoleMenu
        {
            RoleId = input.Id,
            MenuId = u
        }).ToList();
        await _sysRoleMenuRep.InsertRangeAsync(menus);

        // 清除缓存
        // _sysCacheService.RemoveByPrefixKey(CacheConst.KeyUserMenu);
        _sysCacheService.RemoveByPrefixKey(CacheConst.KeyUserButton);
    }

    /// <summary>
    /// 授权角色快捷菜单
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task GranQuickActionMenu(RoleMenuInput input)
    {
        await _sysMenuQuickActionsRep.DeleteAsync(u => u.RoleId == input.Id);
        // 允许无快捷方式
        if (input.MenuIdList == null || input.MenuIdList.Count < 1) return;

        // 追加父级菜单
        var allIdList = await _sysMenuQuickActionsRep.Context.Queryable<SysMenu>().Select(u => new { u.Id, u.Pid }).ToListAsync();
        var pIdList = allIdList.ToChildList(u => u.Pid, u => u.Id, u => input.MenuIdList.Contains(u.Id)).Select(u => u.Pid).Distinct().ToList();
        input.MenuIdList = input.MenuIdList.Concat(pIdList).Distinct().Where(u => u != 0).ToList();

        // 保存授权数据
        var menus = input.MenuIdList.Select(u => new SysMenuQuickActions
        {
            RoleId = input.Id,
            MenuId = u
        }).ToList();
        await _sysMenuQuickActionsRep.InsertRangeAsync(menus);

        // 清除缓存
        // _sysCacheService.RemoveByPrefixKey(CacheConst.KeyUserMenu);
        _sysCacheService.RemoveByPrefixKey(CacheConst.KeyUserQuickAction);
    }

    /// <summary>
    /// 根据菜单Id集合删除角色菜单
    /// </summary>
    /// <param name="menuIdList"></param>
    /// <returns></returns>
    public async Task DeleteRoleMenuByMenuIdList(List<long> menuIdList)
    {
        await _sysRoleMenuRep.DeleteAsync(u => menuIdList.Contains(u.MenuId));
    }

    /// <summary>
    /// 根据角色Id删除角色菜单
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public async Task DeleteRoleMenuByRoleId(long roleId)
    {
        await _sysRoleMenuRep.DeleteAsync(u => u.RoleId == roleId);
    }
}