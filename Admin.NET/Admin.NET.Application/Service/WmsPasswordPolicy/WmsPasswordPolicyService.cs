// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core.Service;
using Microsoft.AspNetCore.Http;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Mapster;
using SqlSugar;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Admin.NET.Application.Entity;
using NewLife.Reflection;
namespace Admin.NET.Application;

/// <summary>
/// 密码策略服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsPasswordPolicyService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsPasswordPolicy> _wmsPasswordPolicyRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsPasswordPolicyService(SqlSugarRepository<WmsPasswordPolicy> wmsPasswordPolicyRep, ISqlSugarClient sqlSugarClient)
    {
        _wmsPasswordPolicyRep = wmsPasswordPolicyRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询密码策略 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询密码策略")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsPasswordPolicyOutput>> Page(PageWmsPasswordPolicyInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsPasswordPolicyRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.Name.Contains(input.Keyword) || u.Description.Contains(input.Keyword) || u.SpecialChars.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Description), u => u.Description.Contains(input.Description.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.SpecialChars), u => u.SpecialChars.Contains(input.SpecialChars.Trim()))
            .WhereIF(input.MinLength != null, u => u.MinLength == input.MinLength)
            .WhereIF(input.AdminMinLength != null, u => u.AdminMinLength == input.AdminMinLength)
            .WhereIF(input.RequireCategories != null, u => u.RequireCategories == input.RequireCategories)
            .WhereIF(input.MaxConsecutive != null, u => u.MaxConsecutive == input.MaxConsecutive)
            .WhereIF(input.RequireLowercase.HasValue, u => u.RequireLowercase == input.RequireLowercase)
            .WhereIF(input.RequireUppercase.HasValue, u => u.RequireUppercase == input.RequireUppercase)
            .WhereIF(input.RequireDigit.HasValue, u => u.RequireDigit == input.RequireDigit)
            .WhereIF(input.RequireSpecial.HasValue, u => u.RequireSpecial == input.RequireSpecial)
            .WhereIF(input.CheckUserId.HasValue, u => u.CheckUserId == input.CheckUserId)
            .WhereIF(input.CheckConsecutive.HasValue, u => u.CheckConsecutive == input.CheckConsecutive)
            .WhereIF(input.CheckCommonPatterns.HasValue, u => u.CheckCommonPatterns == input.CheckCommonPatterns)
            .WhereIF(input.CheckRepeatedChars.HasValue, u => u.CheckRepeatedChars == input.CheckRepeatedChars)
            .WhereIF(input.RememberPasswords != null, u => u.RememberPasswords == input.RememberPasswords)
            .WhereIF(input.PasswordExpiryDays != null, u => u.PasswordExpiryDays == input.PasswordExpiryDays)
            .WhereIF(input.AdminPasswordExpiryDays != null, u => u.AdminPasswordExpiryDays == input.AdminPasswordExpiryDays)
            .WhereIF(input.ExpiryNoticeDays != null, u => u.ExpiryNoticeDays == input.ExpiryNoticeDays)
            .WhereIF(input.ForceChangeInitial.HasValue, u => u.ForceChangeInitial == input.ForceChangeInitial)
            .WhereIF(input.Enabled.HasValue, u => u.Enabled == input.Enabled)
            .Select<WmsPasswordPolicyOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取密码策略详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取密码策略详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsPasswordPolicy> Detail([FromQuery] QueryByIdWmsPasswordPolicyInput input)
    {
        return await _wmsPasswordPolicyRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取已启用的密码策略详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取密码策略详情")]
    [ApiDescriptionSettings(Name = "GetEnableDetail"), HttpGet]
    public async Task<WmsPasswordPolicy> GetEnableDetail()
    {
        return await _wmsPasswordPolicyRep.GetFirstAsync(u => u.Enabled);
    }

    /// <summary>
    /// 增加密码策略 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加密码策略")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsPasswordPolicyInput input)
    {
        await _sqlSugarClient.Ado.BeginTranAsync();
        if (input.Enabled == true)
        {
            var list = await _wmsPasswordPolicyRep.GetListAsync();
            foreach (var item in list)
            {
                item.Enabled = false;
            }
            await _wmsPasswordPolicyRep.UpdateRangeAsync(list);
        }
        var entity = input.Adapt<WmsPasswordPolicy>();
        var res = await _wmsPasswordPolicyRep.InsertAsync(entity) ? entity.Id : 0;
        await _sqlSugarClient.Ado.CommitTranAsync();
        return res;
    }

    /// <summary>
    /// 更新密码策略 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新密码策略")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsPasswordPolicyInput input)
    {
        await _sqlSugarClient.Ado.BeginTranAsync();
        if (input.Enabled == true)
        {
            var list = await _wmsPasswordPolicyRep.GetListAsync();
            foreach (var item in list)
            {
                item.Enabled = false;
            }
            await _wmsPasswordPolicyRep.UpdateRangeAsync(list);
        }
        var entity = input.Adapt<WmsPasswordPolicy>();
        await _wmsPasswordPolicyRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
        await _sqlSugarClient.Ado.CommitTranAsync();
    }

    /// <summary>
    /// 删除密码策略 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除密码策略")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsPasswordPolicyInput input)
    {
        var entity = await _wmsPasswordPolicyRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsPasswordPolicyRep.DeleteAsync(entity);   //真删除
    }

    public async Task<(bool isValid, string message)> ValidatePasswordAsync(string password)
    {
        var policy = await GetEnableDetail();
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
        if (policy.CheckRepeatedChars && HasRepeatedChars(password, 3))
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

    private bool HasRepeatedChars(string password, int maxRepeat)
    {
        if (password.Length < maxRepeat) return false;

        for (int i = 0; i <= password.Length - maxRepeat; i++)
        {
            var segment = password.Substring(i, maxRepeat);
            if (segment.Distinct().Count() == 1)
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

    /// <summary>
    /// 批量删除密码策略 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除密码策略")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<bool> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsPasswordPolicyInput> input)
    {
        var exp = Expressionable.Create<WmsPasswordPolicy>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsPasswordPolicyRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();

        return await _wmsPasswordPolicyRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 导出密码策略记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出密码策略记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsPasswordPolicyInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsPasswordPolicyOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "密码策略导出记录");
    }
    
    /// <summary>
    /// 下载密码策略数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载密码策略数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsPasswordPolicyOutput>(), "密码策略导入模板");
    }
    
    private static readonly object _wmsPasswordPolicyImportLock = new object();
    /// <summary>
    /// 导入密码策略记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入密码策略记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsPasswordPolicyImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportWmsPasswordPolicyInput, WmsPasswordPolicy>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        if (x.MinLength == null){
                            x.Error = "普通用户密码最小长度不能为空";
                            return false;
                        }
                        if (x.AdminMinLength == null){
                            x.Error = "管理员密码最小长度不能为空";
                            return false;
                        }
                        if (x.RequireCategories == null){
                            x.Error = "需要包含的字符类别数量（大写、小写、数字、特殊字符）不能为空";
                            return false;
                        }
                        if (x.MaxConsecutive == null){
                            x.Error = "最大连续相同字符数不能为空";
                            return false;
                        }
                        if (x.RequireLowercase == null){
                            x.Error = "是否必须包含小写字母不能为空";
                            return false;
                        }
                        if (x.RequireUppercase == null){
                            x.Error = "是否必须包含大写字母不能为空";
                            return false;
                        }
                        if (x.RequireDigit == null){
                            x.Error = "是否必须包含数字不能为空";
                            return false;
                        }
                        if (x.RequireSpecial == null){
                            x.Error = "是否必须包含特殊字符不能为空";
                            return false;
                        }
                        if (x.CheckUserId == null){
                            x.Error = "是否检查密码不能包含用户名不能为空";
                            return false;
                        }
                        if (x.CheckConsecutive == null){
                            x.Error = "是否检查连续字符不能为空";
                            return false;
                        }
                        if (x.CheckCommonPatterns == null){
                            x.Error = "是否检查常见模式（如123456、qwerty等）不能为空";
                            return false;
                        }
                        if (x.CheckRepeatedChars == null){
                            x.Error = "是否检查重复字符不能为空";
                            return false;
                        }
                        if (x.RememberPasswords == null){
                            x.Error = "密码历史记录数量，不能重复使用不能为空";
                            return false;
                        }
                        if (x.PasswordExpiryDays == null){
                            x.Error = "普通用户密码过期天数不能为空";
                            return false;
                        }
                        if (x.AdminPasswordExpiryDays == null){
                            x.Error = "管理员密码过期天数不能为空";
                            return false;
                        }
                        if (x.ExpiryNoticeDays == null){
                            x.Error = "密码过期提前提醒天数不能为空";
                            return false;
                        }
                        if (x.ForceChangeInitial == null){
                            x.Error = "是否强制首次登录修改密码不能为空";
                            return false;
                        }
                        if (x.Enabled == null){
                            x.Error = "策略是否启用不能为空";
                            return false;
                        }
                        return true;
                    }).Adapt<List<WmsPasswordPolicy>>();
                    
                    var storageable = _wmsPasswordPolicyRep.Context.Storageable(rows)
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Name), "策略名称不能为空")
                        .SplitError(it => it.Item.Name?.Length > 100, "策略名称长度不能超过100个字符")
                        .SplitError(it => it.Item.Description?.Length > 500, "策略描述长度不能超过500个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.SpecialChars), "允许的特殊字符集合不能为空")
                        .SplitError(it => it.Item.SpecialChars?.Length > 100, "允许的特殊字符集合长度不能超过100个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.Name,
                        it.Description,
                        it.MinLength,
                        it.AdminMinLength,
                        it.RequireCategories,
                        it.MaxConsecutive,
                        it.RequireLowercase,
                        it.RequireUppercase,
                        it.RequireDigit,
                        it.RequireSpecial,
                        it.SpecialChars,
                        it.CheckUserId,
                        it.CheckConsecutive,
                        it.CheckCommonPatterns,
                        it.CheckRepeatedChars,
                        it.RememberPasswords,
                        it.PasswordExpiryDays,
                        it.AdminPasswordExpiryDays,
                        it.ExpiryNoticeDays,
                        it.ForceChangeInitial,
                        it.Enabled,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
