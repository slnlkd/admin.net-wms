// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using DocumentFormat.OpenXml.Wordprocessing;
using NewLife.Data;

namespace Admin.NET.Core.Service;

/// <summary>
/// 系统用户服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 490)]
public class SysUserService : IDynamicApiController, ITransient
{
    private readonly UserManager _userManager;
    private readonly SysOrgService _sysOrgService;
    private readonly SysUserExtOrgService _sysUserExtOrgService;
    private readonly SysUserRoleService _sysUserRoleService;
    private readonly SysConfigService _sysConfigService;
    private readonly SysOnlineUserService _sysOnlineUserService;
    private readonly SysUserMenuService _sysUserMenuService;
    private readonly SysCacheService _sysCacheService;
    private readonly SysUserLdapService _sysUserLdapService;
    private readonly SqlSugarRepository<SysUser> _sysUserRep;
    private readonly IEventPublisher _eventPublisher;
    private readonly ISqlSugarClient _sqlSugarClient;


    public SysUserService(UserManager userManager,
        SysOrgService sysOrgService,
        SysUserExtOrgService sysUserExtOrgService,
        SysUserRoleService sysUserRoleService,
        SysConfigService sysConfigService,
        SysOnlineUserService sysOnlineUserService,
        SysCacheService sysCacheService,
        SysUserLdapService sysUserLdapService,
        SqlSugarRepository<SysUser> sysUserRep,
        SysUserMenuService sysUserMenuService,
        IEventPublisher eventPublisher,
        ISqlSugarClient sqlSugarClient)
    {
        _userManager = userManager;
        _sysOrgService = sysOrgService;
        _sysUserExtOrgService = sysUserExtOrgService;
        _sysUserRoleService = sysUserRoleService;
        _sysConfigService = sysConfigService;
        _sysOnlineUserService = sysOnlineUserService;
        _sysCacheService = sysCacheService;
        _sysUserLdapService = sysUserLdapService;
        _sysUserMenuService = sysUserMenuService;
        _sysUserRep = sysUserRep;
        _eventPublisher = eventPublisher;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 获取用户分页列表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取用户分页列表")]
    public virtual async Task<SqlSugarPagedList<UserOutput>> Page(PageUserInput input)
    {
        //获取子节点Id集合(包含自己)
        var orgList = await _sysOrgService.GetChildIdListWithSelfById(input.OrgId);

        return await _sysUserRep.AsQueryable()
            .LeftJoin<SysOrg>((u, a) => u.OrgId == a.Id)
            .LeftJoin<SysPos>((u, a, b) => u.PosId == b.Id)
            .Where(u => u.AccountType != AccountTypeEnum.SuperAdmin)
            .WhereIF(input.OrgId > 0, u => orgList.Contains(u.OrgId))
            .WhereIF(!_userManager.SuperAdmin, u => u.AccountType != AccountTypeEnum.SysAdmin)
            .WhereIF(_userManager.SuperAdmin && input.TenantId > 0, u => u.TenantId == input.TenantId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Account), u => u.Account.Contains(input.Account))
            .WhereIF(!string.IsNullOrWhiteSpace(input.RealName), u => u.RealName.Contains(input.RealName))
            .WhereIF(!string.IsNullOrWhiteSpace(input.PosName), (u, a, b) => b.Name.Contains(input.PosName))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Phone), u => u.Phone.Contains(input.Phone))
            .OrderBy(u => new { u.OrderNo, u.Id })
            .Select((u, a, b) => new UserOutput
            {
                OrgName = a.Name,
                PosName = b.Name,
                RoleName = SqlFunc.Subqueryable<SysUserRole>().LeftJoin<SysRole>((m, n) => m.RoleId == n.Id).Where(m => m.UserId == u.Id).SelectStringJoin((m, n) => n.Name, ","),
                DomainAccount = SqlFunc.Subqueryable<SysUserLdap>().Where(m => m.UserId == u.Id).Select(m => m.Account)
            }, true)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加用户 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    [DisplayName("增加用户")]
    public virtual async Task<long> AddUser(AddUserInput input)
    {
        var isExist = await _sysUserRep.AsQueryable().ClearFilter().AnyAsync(u => u.Account == input.Account);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.D1003);

        if (!string.IsNullOrWhiteSpace(input.Phone) && await _sysUserRep.AsQueryable().ClearFilter().AnyAsync(u => u.Phone == input.Phone))
            throw Oops.Oh(ErrorCodeEnum.D1032);

        // 禁止越权新增超级管理员和系统管理员
        if (_userManager.AccountType != AccountTypeEnum.SuperAdmin && input.AccountType is AccountTypeEnum.SuperAdmin or AccountTypeEnum.SysAdmin) throw Oops.Oh(ErrorCodeEnum.D1038);

        // 若没有设置密码则取默认密码
        var password = !string.IsNullOrWhiteSpace(input.Password) ? input.Password : await _sysConfigService.GetConfigValue<string>(ConfigConst.SysPassword);
        var user = input.Adapt<SysUser>();
        user.Password = CryptogramUtil.Encrypt(password);
        var newUser = await _sysUserRep.AsInsertable(user).ExecuteReturnEntityAsync();

        input.Id = newUser.Id;
        await UpdateRoleAndExtOrg(input);

        // 增加域账号
        if (!string.IsNullOrWhiteSpace(input.DomainAccount))
            await _sysUserLdapService.AddUserLdap(newUser.TenantId!.Value, newUser.Id, newUser.Account, input.DomainAccount);

        // 发布系统用户操作事件
        await _eventPublisher.PublishAsync(SysUserEventTypeEnum.Add, new
        {
            Entity = newUser,
            Input = input
        });

        return newUser.Id;
    }

    /// <summary>
    /// 注册用户 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [NonAction]
    public virtual async Task<long> RegisterUser(AddUserInput input)
    {
        var isExist = await _sysUserRep.AsQueryable().ClearFilter().AnyAsync(u => u.Account == input.Account);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.D1003);

        if (!string.IsNullOrWhiteSpace(input.Phone) && await _sysUserRep.AsQueryable().ClearFilter().AnyAsync(u => u.Phone == input.Phone))
            throw Oops.Oh(ErrorCodeEnum.D1032);

        // 禁止越权注册
        if (input.AccountType is AccountTypeEnum.SuperAdmin or AccountTypeEnum.SysAdmin) throw Oops.Oh(ErrorCodeEnum.D1038);

        if (string.IsNullOrWhiteSpace(input.Password))
        {
            var password = await _sysConfigService.GetConfigValue<string>(ConfigConst.SysPassword);
            input.Password = CryptogramUtil.Encrypt(password);
        }

        var user = input.Adapt<SysUser>();
        var newUser = await _sysUserRep.AsInsertable(user).ExecuteReturnEntityAsync();

        input.Id = newUser.Id;
        await UpdateRoleAndExtOrg(input);

        // 增加域账号
        if (!string.IsNullOrWhiteSpace(input.DomainAccount))
            await _sysUserLdapService.AddUserLdap(newUser.TenantId!.Value, newUser.Id, newUser.Account, input.DomainAccount);

        // 发布系统用户操作事件
        await _eventPublisher.PublishAsync(SysUserEventTypeEnum.Register, new
        {
            Entity = newUser,
            Input = input
        });

        return newUser.Id;
    }

    /// <summary>
    /// 更新用户 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    [DisplayName("更新用户")]
    public virtual async Task UpdateUser(UpdateUserInput input)
    {
        if (await _sysUserRep.AsQueryable().ClearFilter().AnyAsync(u => u.Account == input.Account && u.Id != input.Id))
            throw Oops.Oh(ErrorCodeEnum.D1003);

        if (!string.IsNullOrWhiteSpace(input.Phone) && await _sysUserRep.AsQueryable().ClearFilter().AnyAsync(u => u.Phone == input.Phone && u.Id != input.Id))
            throw Oops.Oh(ErrorCodeEnum.D1032);

        // 禁止越权更新超级管理员或系统管理员信息
        if (_userManager.AccountType != AccountTypeEnum.SuperAdmin && input.AccountType is AccountTypeEnum.SuperAdmin or AccountTypeEnum.SysAdmin) throw Oops.Oh(ErrorCodeEnum.D1038);

        await _sysUserRep.AsUpdateable(input.Adapt<SysUser>()).IgnoreColumns(true)
            .IgnoreColumns(u => new { u.Password, u.Status, u.TenantId }).ExecuteCommandAsync();

        await UpdateRoleAndExtOrg(input);

        // 删除用户机构缓存
        SqlSugarFilter.DeleteUserOrgCache(input.Id, _sysUserRep.Context.CurrentConnectionConfig.ConfigId.ToString());

        // 若账号的角色和组织架构发生变化,则强制下线账号进行权限更新
        var user = await _sysUserRep.AsQueryable().ClearFilter().FirstAsync(u => u.Id == input.Id);
        var roleIds = await GetOwnRoleList(input.Id);
        if (input.OrgId != user.OrgId || !input.RoleIdList.OrderBy(u => u).SequenceEqual(roleIds.OrderBy(u => u)))
            await _sysOnlineUserService.ForceOffline(input.Id);
        // 更新域账号
        await _sysUserLdapService.AddUserLdap(user.TenantId!.Value, user.Id, user.Account, input.DomainAccount);

        // 发布系统用户操作事件
        await _eventPublisher.PublishAsync(SysUserEventTypeEnum.Update, new
        {
            Entity = user,
            Input = input
        });
    }

    /// <summary>
    /// 更新当前用户语言 🔖
    /// </summary>
    /// <param name="langCode"></param>
    /// <returns></returns>
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "SetLangCode"), HttpPost]
    [DisplayName("更新当前用户语言")]
    public virtual async Task SetLangCode(string langCode)
    {
        var user = await _sysUserRep.AsQueryable().ClearFilter().FirstAsync(u => u.Id == _userManager.UserId) ?? throw Oops.Oh(ErrorCodeEnum.D1011).StatusCode(401);
        user.LangCode = langCode;
        await _sysUserRep.AsUpdateable(user).UpdateColumns(it => it.LangCode).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新角色和扩展机构
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task UpdateRoleAndExtOrg(AddUserInput input)
    {
        await GrantRole(new UserRoleInput { UserId = input.Id, RoleIdList = input.RoleIdList });

        await _sysUserExtOrgService.UpdateUserExtOrg(input.Id, input.ExtOrgIdList);
    }

    /// <summary>
    /// 删除用户 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    [DisplayName("删除用户")]
    public virtual async Task DeleteUser(DeleteUserInput input)
    {
        var user = await _sysUserRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D0009);
        user.ValidateIsSuperAdminAccountType();
        user.ValidateIsUserId(_userManager.UserId);

        // 若账号为租户默认账号则禁止删除
        var isTenantUser = await _sysUserRep.ChangeRepository<SqlSugarRepository<SysTenant>>().IsAnyAsync(u => u.UserId == input.Id);
        if (isTenantUser) throw Oops.Oh(ErrorCodeEnum.D1029);

        // 若账号为开放接口绑定账号则禁止删除
        var isOpenAccessUser = await _sysUserRep.ChangeRepository<SqlSugarRepository<SysOpenAccess>>().IsAnyAsync(u => u.BindUserId == input.Id);
        if (isOpenAccessUser) throw Oops.Oh(ErrorCodeEnum.D1030);

        // 强制下线
        await _sysOnlineUserService.ForceOffline(user.Id);

        await _sysUserRep.DeleteAsync(user);

        // 删除用户角色
        await _sysUserRoleService.DeleteUserRoleByUserId(input.Id);

        // 删除用户扩展机构
        await _sysUserExtOrgService.DeleteUserExtOrgByUserId(input.Id);

        // 删除域账号
        await _sysUserLdapService.DeleteUserLdapByUserId(input.Id);

        // 删除用户收藏菜单
        await _sysUserMenuService.DeleteUserMenuList(input.Id);

        // 发布系统用户操作事件
        await _eventPublisher.PublishAsync(SysUserEventTypeEnum.Delete, new
        {
            Entity = user,
            Input = input
        });
    }

    /// <summary>
    /// 查看用户基本信息 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("查看用户基本信息")]
    public virtual async Task<SysUser> GetBaseInfo()
    {
        return await _sysUserRep.AsQueryable().ClearFilter().FirstAsync(c => c.Id == _userManager.UserId);
    }

    /// <summary>
    /// 查询用户组织机构信息 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("查询用户组织机构信息")]
    public virtual async Task<List<OrgTreeOutput>> GetOrgInfo()
    {
        return await _sysOrgService.GetTree(new OrgInput { Id = 0 });
    }

    /// <summary>
    /// 更新用户基本信息 🔖
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "BaseInfo"), HttpPost]
    [DisplayName("更新用户基本信息")]
    public virtual async Task<int> UpdateBaseInfo(SysUser user)
    {
        return await _sysUserRep.AsUpdateable(user)
            .IgnoreColumns(u => new { u.CreateTime, u.Account, u.Password, u.AccountType, u.OrgId, u.PosId }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 设置用户状态 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [DisplayName("设置用户状态")]
    public virtual async Task<int> SetStatus(UserInput input)
    {
        if (_userManager.UserId == input.Id)
            throw Oops.Oh(ErrorCodeEnum.D1026);

        var user = await _sysUserRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D0009);
        user.ValidateIsSuperAdminAccountType(ErrorCodeEnum.D1015);
        if (!Enum.IsDefined(typeof(StatusEnum), input.Status))
            throw Oops.Oh(ErrorCodeEnum.D3005);

        // 账号禁用则增加黑名单，账号启用则移除黑名单
        var sysCacheService = App.GetRequiredService<SysCacheService>();
        if (input.Status == StatusEnum.Disable)
        {
            sysCacheService.Set($"{CacheConst.KeyBlacklist}{user.Id}", $"{user.RealName}-{user.Phone}");

            // 强制下线
            await _sysOnlineUserService.ForceOffline(user.Id);
        }
        else
        {
            sysCacheService.Remove($"{CacheConst.KeyBlacklist}{user.Id}");
        }

        user.Status = input.Status;
        var rows = await _sysUserRep.AsUpdateable(user).UpdateColumns(u => new { u.Status }).ExecuteCommandAsync();

        // 发布系统用户操作事件
        if (rows > 0)
            await _eventPublisher.PublishAsync(SysUserEventTypeEnum.SetStatus, new
            {
                Entity = user,
                Input = input
            });

        return rows;
    }

    /// <summary>
    /// 授权用户角色 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [DisplayName("授权用户角色")]
    public async Task GrantRole(UserRoleInput input)
    {
        //var user = await _sysUserRep.GetFirstAsync(u => u.Id == input.UserId) ?? throw Oops.Oh(ErrorCodeEnum.D0009);
        //if (user.AccountType == AccountTypeEnum.SuperAdmin)
        //    throw Oops.Oh(ErrorCodeEnum.D1022);

        await _sysUserRoleService.GrantUserRole(input);

        // 发布系统用户操作事件
        await _eventPublisher.PublishAsync(SysUserEventTypeEnum.UpdateRole, input);
    }

    /// <summary>
    /// 修改用户密码 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("修改用户密码")]
    public virtual async Task<int> ChangePwd(ChangePwdInput input)
    {
        // 国密SM2解密（前端密码传输SM2加密后的）
        input.PasswordOld = CryptogramUtil.SM2Decrypt(input.PasswordOld);
        input.PasswordNew = CryptogramUtil.SM2Decrypt(input.PasswordNew);

        var user = await _sysUserRep.AsQueryable().ClearFilter().FirstAsync(c => c.Id == _userManager.UserId) ?? throw Oops.Oh(ErrorCodeEnum.D0009);
        if (CryptogramUtil.CryptoType == CryptogramEnum.MD5.ToString())
        {
            if (user.Password != MD5Encryption.Encrypt(input.PasswordOld))
                throw Oops.Oh(ErrorCodeEnum.D1004);
        }
        else
        {
            if (CryptogramUtil.Decrypt(user.Password) != input.PasswordOld)
                throw Oops.Oh(ErrorCodeEnum.D1004);
        }

        if (input.PasswordOld == input.PasswordNew)
            throw Oops.Oh(ErrorCodeEnum.D1028);

        // 根据密码策略进行校验
        var policy = await _sqlSugarClient.Queryable<dynamic>().AS("WmsPasswordPolicy").Where("Enabled=@Enabled", new { Enabled = true }).FirstAsync();

        var valid = await ValidatePasswordAsync(input.PasswordNew);
        if (!valid.isValid)
            throw Oops.Oh(valid.message);

        user.Password = CryptogramUtil.Encrypt(input.PasswordNew);
        //if (CryptogramUtil.StrongPassword)
        //{
        //    if (!valid.isValid)
        //        throw Oops.Oh(valid.message);

        //    user.Password = CryptogramUtil.Encrypt(input.PasswordNew);
        //}
        //else
        //{
        //    user.Password = CryptogramUtil.Encrypt(input.PasswordNew);
        //}
        // 获取最大历史记录数
        int num = Convert.ToInt32(policy.RememberPasswords);
        var recentHistories = await _sqlSugarClient.Queryable<dynamic>()
            .AS("WmsUserPasswordHistory")
            .Where("UserId = @UserId", new { UserId = user.Id })
            .OrderBy("CreateTime DESC")
            .Take(num)
            .ToListAsync();

        // 使用安全字符串比较，避免时序攻击
        bool isPasswordDuplicate = recentHistories.Any(history => 
        {
            var decryptedHistoryPassword = CryptogramUtil.Decrypt(history.PasswordHash);
            var decryptedNewPassword = CryptogramUtil.Decrypt(user.Password);
            return string.Equals(decryptedHistoryPassword, decryptedNewPassword, StringComparison.Ordinal);
        });

        if (isPasswordDuplicate)
        {
            throw Oops.Oh("新密码不能与近期已修改过的旧密码相同");
        }

        var rows = await _sysUserRep.AsUpdateable(user).UpdateColumns(u => u.Password).ExecuteCommandAsync();
        // 插入历史记录
        _sqlSugarClient.InsertableByDynamic(new
        {
            Id = SnowFlakeSingle.instance.NextId(),
            UserId = App.User.FindFirst(ClaimConst.UserId).Value,  // nvarchar(50) 类型，必填
            PasswordHash = CryptogramUtil.Encrypt(input.PasswordOld),  // nvarchar(255) 类型，必填
            CreateTime = DateTime.Now  // datetime2(7) 类型，有默认值但这里显式提供
        })
        .AS("WmsUserPasswordHistory")
        .ExecuteCommand();
        // 发布系统用户操作事件
        if (rows > 0)
            await _eventPublisher.PublishAsync(SysUserEventTypeEnum.ChangePwd, new
            {
                Entity = user,
                Input = input
            });

        return rows;
    }

    /// <summary>
    /// 重置用户密码 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("重置用户密码")]
    public virtual async Task<string> ResetPwd(ResetPwdUserInput input)
    {
        var user = await _sysUserRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D0009);
        string randomPassword = new(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", 6).Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        user.Password = CryptogramUtil.Encrypt(randomPassword);
        await _sysUserRep.AsUpdateable(user).UpdateColumns(u => u.Password).ExecuteCommandAsync();

        // 清空密码错误次数
        var keyErrorPasswordCount = $"{CacheConst.KeyPasswordErrorTimes}{user.Account}";
        _sysCacheService.Remove(keyErrorPasswordCount);

        // 发布系统用户操作事件
        await _eventPublisher.PublishAsync(SysUserEventTypeEnum.ResetPwd, new
        {
            Entity = user,
            Input = input
        });

        return randomPassword;
    }

    /// <summary>
    /// 解除登录锁定 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("解除登录锁定")]
    public virtual async Task UnlockLogin(UnlockLoginInput input)
    {
        var user = await _sysUserRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D0009);

        // 清空密码错误次数
        var keyPasswordErrorTimes = $"{CacheConst.KeyPasswordErrorTimes}{user.Account}";
        _sysCacheService.Remove(keyPasswordErrorTimes);

        // 发布系统用户操作事件
        await _eventPublisher.PublishAsync(SysUserEventTypeEnum.UnlockLogin, new
        {
            Entity = user,
            Input = input
        });
    }

    /// <summary>
    /// 获取用户拥有角色集合 🔖
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [DisplayName("获取用户拥有角色集合")]
    public async Task<List<long>> GetOwnRoleList(long userId)
    {
        return await _sysUserRoleService.GetUserRoleIdList(userId);
    }

    /// <summary>
    /// 获取用户扩展机构集合 🔖
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [DisplayName("获取用户扩展机构集合")]
    public async Task<List<SysUserExtOrg>> GetOwnExtOrgList(long userId)
    {
        return await _sysUserExtOrgService.GetUserExtOrgList(userId);
    }
    /// <summary>
    /// 密码策略校验规则
    /// </summary>
    /// <param name="password"></param>
    /// <param name="policy"></param>
    /// <returns></returns>
    public async Task<(bool isValid, string message)> ValidatePasswordAsync(string password)
    {
        var policy = await _sqlSugarClient.Queryable<dynamic>().AS("WmsPasswordPolicy").Where("Enabled=@Enabled", new { Enabled = true }).FirstAsync();
        // 无策略 直接过
        if (policy == null)
        {
            return (true,"");
        }
        var errors = new List<string>();
        long? userId = Convert.ToInt64(App.User.FindFirst(ClaimConst.UserId).Value);
        string userName = App.User.FindFirst(ClaimConst.Account).Value;
        bool isAdmin = (App.User.FindFirst(ClaimConst.AccountType).Value == AccountTypeEnum.SuperAdmin.ToString()) ? true : false;

        // 基础长度检查
        var minLength = isAdmin ? policy.AdminMinLength : policy.MinLength;
        if (password.Length < minLength)
        {
            errors.Add($"密码长度至少需要 {minLength} 个字符");
        }

        // 字符类型检查
        var categories = 0;
        var hasLower = false;
        var hasUpper = false;
        var hasDigit = false;
        var hasSpecial = false;

        foreach (var c in password)
        {
            if (char.IsLower(c)) hasLower = true;
            else if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else if (policy.SpecialChars.Contains(c)) hasSpecial = true;
        }

        if (policy.RequireLowercase && !hasLower)
        {
            errors.Add("密码必须包含小写字母");
        }
        if (policy.RequireUppercase && !hasUpper)
        {
            errors.Add("密码必须包含大写字母");
        }
        if (policy.RequireDigit && !hasDigit)
        {
            errors.Add("密码必须包含数字");
        }
        if (policy.RequireSpecial && !hasSpecial)
        {
            errors.Add($"密码必须包含特殊字符: {policy.SpecialChars}");
        }

        // 计算满足的类别数
        if (hasLower) categories++;
        if (hasUpper) categories++;
        if (hasDigit) categories++;
        if (hasSpecial) categories++;

        if (categories < policy.RequireCategories)
        {
            errors.Add($"密码必须包含至少 {policy.RequireCategories} 种不同类型的字符");
        }

        // 连续字符检查
        if (policy.CheckConsecutive && HasConsecutiveChars(password, policy.MaxConsecutive))
        {
            errors.Add($"密码不能包含超过 {policy.MaxConsecutive} 个连续字符");
        }

        // 重复字符检查
        if (policy.CheckRepeatedChars && HasRepeatedChars(password, 4))
        {
            errors.Add("密码包含过多重复字符");
        }

        // 常见模式检查
        if (policy.CheckCommonPatterns && IsCommonPattern(password))
        {
            errors.Add("密码不能使用常见模式（如123456, qwerty等）");
        }

        // 用户信息检查
        if (policy.CheckUserId && userId.HasValue && !string.IsNullOrEmpty(userName))
        {
            if (password.Contains(userId.Value.ToString()) ||
                password.Contains(userName, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("密码不能包含用户ID或用户名");
            }
        }

        return (errors.Count == 0, errors.Count == 0 ? "密码强度符合要求" : string.Join("; ", errors));
    }

    private bool HasConsecutiveChars(string password, int maxConsecutive)
    {
        if (password.Length < maxConsecutive) return false;

        for (int i = 0; i <= password.Length - maxConsecutive; i++)
        {
            var segment = password.Substring(i, maxConsecutive);
            if (IsConsecutive(segment) || IsConsecutiveKeyboard(segment))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsConsecutive(string segment)
    {
        // 检查数字或字母连续
        var chars = segment.ToCharArray();
        var ascending = true;
        var descending = true;

        for (int i = 1; i < chars.Length; i++)
        {
            if (chars[i] - chars[i - 1] != 1) ascending = false;
            if (chars[i - 1] - chars[i] != 1) descending = false;
        }

        return ascending || descending;
    }

    private bool IsConsecutiveKeyboard(string segment)
    {
        // 简单键盘连续检查（可扩展）
        var commonSequences = new[] { "qwerty", "asdfgh", "zxcvbn", "123456", "654321" };
        return commonSequences.Any(seq => segment.ToLower().Contains(seq));
    }

    // 统一的重复字符检查方法
    private bool HasRepeatedChars(string password, int maxRepeat)
    {
        if (password.Length < maxRepeat) return false;

        for (int i = 0; i <= password.Length - maxRepeat; i++)
        {
            var segment = password.Substring(i, maxRepeat);
            // 检查是否所有字符都相同
            if (segment.All(c => c == segment[0]))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsCommonPattern(string password)
    {
        var commonPasswords = new[]
        {
            "123456", "password", "12345678", "qwerty", "123456789",
            "12345", "1234", "111111", "1234567", "dragon"
        };

        return commonPasswords.Contains(password.ToLower());
    }
}