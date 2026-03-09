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
using RazorEngine.Compilation.ImpromptuInterface.Optimization;
namespace Admin.NET.Application;

/// <summary>
/// 仓储权限控制服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsPrmissionScopeService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsPrmissionScope> _wmsPrmissionScopeRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsPrmissionScopeService(SqlSugarRepository<WmsPrmissionScope> wmsPrmissionScopeRep, ISqlSugarClient sqlSugarClient)
    {
        _wmsPrmissionScopeRep = wmsPrmissionScopeRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询仓储权限控制 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询仓储权限控制")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsPrmissionScopeOutput>> Page(PageWmsPrmissionScopeInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsPrmissionScopeRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.TableName.Contains(input.Keyword) || u.FieldName.Contains(input.Keyword) || u.FieldValue.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.TableName), u => u.TableName.Contains(input.TableName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.FieldName), u => u.FieldName.Contains(input.FieldName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.FieldValue), u => u.FieldValue.Contains(input.FieldValue.Trim()))
            .WhereIF(input.UserId != null, u => u.UserId == input.UserId)
            .LeftJoin<SysUser>((u, user) => u.UserId == user.Id)
            .Select((u, user) => new WmsPrmissionScopeOutput
            {
                Id = u.Id,
                UserId = u.UserId,
                UserFkDisplayName = $"{user.Account}-{user.RealName}-{user.NickName}",
                TableName = u.TableDes,
                FieldName = u.FieldDes,
                FieldValue = u.FieldValue,
                CreateTime = u.CreateTime,
                UpdateTime = u.UpdateTime,
                CreateUserId = u.CreateUserId,
                CreateUserName = u.CreateUserName,
                UpdateUserId = u.UpdateUserId,
                UpdateUserName = u.UpdateUserName,
            });
		return await query.OrderBuilder(input,"u.").ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取仓储权限控制详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取仓储权限控制详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsPrmissionScope> Detail([FromQuery] QueryByIdWmsPrmissionScopeInput input)
    {
        return await _wmsPrmissionScopeRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加仓储权限控制 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加仓储权限控制")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsPrmissionScopeInput input)
    {
        var entity = input.Adapt<WmsPrmissionScope>();
        var res = await _wmsPrmissionScopeRep.InsertAsync(entity) ? entity.Id : 0;
        UpdateFillter(entity.UserId);
        return res;
    }

    /// <summary>
    /// 更新仓储权限控制 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新仓储权限控制")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsPrmissionScopeInput input)
    {
        var entity = input.Adapt<WmsPrmissionScope>();
        await _wmsPrmissionScopeRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
        UpdateFillter(entity.UserId);
    }

    /// <summary>
    /// 删除仓储权限控制 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除仓储权限控制")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsPrmissionScopeInput input)
    {
        var entity = await _wmsPrmissionScopeRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsPrmissionScopeRep.DeleteAsync(entity);   //真删除
        UpdateFillter(entity.UserId);
    }
    /// <summary>
    /// 权限发生改变时调用此方法 更新当前用户的数据过滤条件
    /// </summary>
    public void UpdateFillter(long? userId)
    {
        if (userId == null)
        {
            return;
        }
        var tenantId = App.User?.FindFirst(ClaimConst.TenantId)?.Value;
        // 获取当前租户数据库底层 db 
        var db = App.GetRequiredService<SysTenantService>().GetTenantDbConnectionScope();

        db.QueryFilter.Clear<WmsBaseWareHouse>();

        SqlSugarFilter.DeleteCustomCache(Convert.ToInt64(userId), tenantId);
        SqlSugarFilter.SetCustomEntityFilter(db,userId.ToString());
    }

    /// <summary>
    /// 批量删除仓储权限控制 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除仓储权限控制")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<bool> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsPrmissionScopeInput> input)
    {
        var exp = Expressionable.Create<WmsPrmissionScope>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsPrmissionScopeRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();

        return await _wmsPrmissionScopeRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 获取下拉列表数据 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取下拉列表数据")]
    [ApiDescriptionSettings(Name = "DropdownData"), HttpPost]
    public async Task<Dictionary<string, dynamic>> DropdownData(DropdownDataWmsPrmissionScopeInput input)
    {
        // 根据当前租户ID 获取当前数据库 sugar底层DB
        var db = App.GetRequiredService<SysTenantService>().GetTenantDbConnectionScope();
        
        var userList = await _wmsPrmissionScopeRep.Context.Queryable<SysUser>()
            .Where(u=>u.AccountType != AccountTypeEnum.SuperAdmin)
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.Account}-{u.RealName}-{u.NickName}"
            }).ToListAsync();
        var tableList = db.DbMaintenance.GetTableInfoList().Where(t => t.Name.StartsWith("W"))
            .Select(u => new {
                Value = u.Name,
                Label = u.Description
            }).ToList();
        return new Dictionary<string, dynamic>
        {
            { "userId", userList },
            { "tableList", tableList },
        };
    }

    /// <summary>
    /// 获取功能下拉列表数据 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取功能下拉列表数据")]
    [ApiDescriptionSettings(Name = "DropdownTableData"), HttpPost]
    public Dictionary<string, dynamic> DropdownTableData(DropdownDataWmsPrmissionScopeInput input)
    {
        var db = App.GetRequiredService<SysTenantService>().GetTenantDbConnectionScope();
        // 获取表列
        var columnList = db.DbMaintenance.GetColumnInfosByTableName(input.TableName)
            .Select(u => new {
                Value = u.DbColumnName,
                Label = u.ColumnDescription
            }).ToList();
        // 查询表数据
        var tableData = new List<dynamic>();
        // 获取列
        var columns = db.DbMaintenance.GetColumnInfosByTableName(input.TableName);
        // 判断当前表是否存在 IsDelete 字段
        if (columns.Any(c => c.DbColumnName.Equals("IsDelete", StringComparison.OrdinalIgnoreCase)))
        {
            // 只取未删除 数据
            tableData = db.Queryable<dynamic>().AS(input.TableName).Where("IsDelete = 0").ToList();
        }
        else
        {
            tableData = db.Queryable<dynamic>().AS(input.TableName).ToList();
        }

        return new Dictionary<string, dynamic>
        {
            { "columnList", columnList },
            { "tableData", tableData },
        };
    }
}
