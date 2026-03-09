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
namespace Admin.NET.Application;

/// <summary>
/// 用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsUserPasswordHistoryService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsUserPasswordHistory> _wmsUserPasswordHistoryRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsUserPasswordHistoryService(SqlSugarRepository<WmsUserPasswordHistory> wmsUserPasswordHistoryRep, ISqlSugarClient sqlSugarClient)
    {
        _wmsUserPasswordHistoryRep = wmsUserPasswordHistoryRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsUserPasswordHistoryOutput>> Page(PageWmsUserPasswordHistoryInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsUserPasswordHistoryRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.UserId.Contains(input.Keyword) || u.PasswordHash.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.UserId), u => u.UserId.Contains(input.UserId.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.PasswordHash), u => u.PasswordHash.Contains(input.PasswordHash.Trim()))
            .Select<WmsUserPasswordHistoryOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsUserPasswordHistory> Detail([FromQuery] QueryByIdWmsUserPasswordHistoryInput input)
    {
        return await _wmsUserPasswordHistoryRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 根据当前登录Token获取用户密码历史
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("根据当前登录Token获取用户密码历史 - 存储用户历史密码记录，用于防止密码重复使用详情")]
    [ApiDescriptionSettings(Name = "GetListByUser"), HttpGet]
    public List<WmsUserPasswordHistory> GetListByUser()
    {
        var userId = App.User?.FindFirst(ClaimConst.UserId)?.Value;
        return [_wmsUserPasswordHistoryRep.AsQueryable().OrderByDescending(m=>m.CreateTime).First()];
    }

    /// <summary>
    /// 增加用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsUserPasswordHistoryInput input)
    {
        var entity = input.Adapt<WmsUserPasswordHistory>();
        return await _wmsUserPasswordHistoryRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsUserPasswordHistoryInput input)
    {
        var entity = input.Adapt<WmsUserPasswordHistory>();
        await _wmsUserPasswordHistoryRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsUserPasswordHistoryInput input)
    {
        var entity = await _wmsUserPasswordHistoryRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsUserPasswordHistoryRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除用户密码历史表 - 存储用户历史密码记录，用于防止密码重复使用")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<bool> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsUserPasswordHistoryInput> input)
    {
        var exp = Expressionable.Create<WmsUserPasswordHistory>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsUserPasswordHistoryRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();

        return await _wmsUserPasswordHistoryRep.DeleteAsync(list);   //真删除
    }
}
