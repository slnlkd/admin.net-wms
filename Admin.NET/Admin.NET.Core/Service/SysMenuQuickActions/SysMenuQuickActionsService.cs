// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！


namespace Admin.NET.Core.Service;

/// <summary>
/// 系统角色菜单快捷方式配置表服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public partial class SysMenuQuickActionsService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysMenuQuickActions> _sysMenuQuickActionsRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public SysMenuQuickActionsService(SqlSugarRepository<SysMenuQuickActions> sysMenuQuickActionsRep, ISqlSugarClient sqlSugarClient)
    {
        _sysMenuQuickActionsRep = sysMenuQuickActionsRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询系统角色菜单快捷方式配置表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询系统角色菜单快捷方式配置表")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<SysMenuQuickActionsOutput>> Page(PageSysMenuQuickActionsInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _sysMenuQuickActionsRep.AsQueryable()
            .WhereIF(input.RoleId != null, u => u.RoleId == input.RoleId)
            .WhereIF(input.MenuId != null, u => u.MenuId == input.MenuId)
            .Select<SysMenuQuickActionsOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取系统角色菜单快捷方式配置表详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取系统角色菜单快捷方式配置表详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<SysMenuQuickActions> Detail([FromQuery] QueryByIdSysMenuQuickActionsInput input)
    {
        return await _sysMenuQuickActionsRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加系统角色菜单快捷方式配置表 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加系统角色菜单快捷方式配置表")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddSysMenuQuickActionsInput input)
    {
        var entity = input.Adapt<SysMenuQuickActions>();
        return await _sysMenuQuickActionsRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新系统角色菜单快捷方式配置表 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新系统角色菜单快捷方式配置表")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateSysMenuQuickActionsInput input)
    {
        var entity = input.Adapt<SysMenuQuickActions>();
        await _sysMenuQuickActionsRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除系统角色菜单快捷方式配置表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除系统角色菜单快捷方式配置表")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteSysMenuQuickActionsInput input)
    {
        var entity = await _sysMenuQuickActionsRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _sysMenuQuickActionsRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除系统角色菜单快捷方式配置表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除系统角色菜单快捷方式配置表")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<bool> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteSysMenuQuickActionsInput> input)
    {
        var exp = Expressionable.Create<SysMenuQuickActions>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _sysMenuQuickActionsRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();

        return await _sysMenuQuickActionsRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 导出系统角色菜单快捷方式配置表记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出系统角色菜单快捷方式配置表记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageSysMenuQuickActionsInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportSysMenuQuickActionsOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "系统角色菜单快捷方式配置表导出记录");
    }
    
    /// <summary>
    /// 下载系统角色菜单快捷方式配置表数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载系统角色菜单快捷方式配置表数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportSysMenuQuickActionsOutput>(), "系统角色菜单快捷方式配置表导入模板");
    }
    
    private static readonly object _sysMenuQuickActionsImportLock = new object();
    /// <summary>
    /// 导入系统角色菜单快捷方式配置表记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入系统角色菜单快捷方式配置表记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_sysMenuQuickActionsImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportSysMenuQuickActionsInput, SysMenuQuickActions>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        if (x.RoleId == null){
                            x.Error = "角色Id不能为空";
                            return false;
                        }
                        if (x.MenuId == null){
                            x.Error = "角色菜单Id不能为空";
                            return false;
                        }
                        return true;
                    }).Adapt<List<SysMenuQuickActions>>();
                    
                    var storageable = _sysMenuQuickActionsRep.Context.Storageable(rows)
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.RoleId,
                        it.MenuId,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
