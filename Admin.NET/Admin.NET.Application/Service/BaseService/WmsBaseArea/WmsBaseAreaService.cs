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
/// 仓库区域表服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsBaseAreaService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsBaseArea> _wmsBaseAreaRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsBaseAreaService(SqlSugarRepository<WmsBaseArea> wmsBaseAreaRep, ISqlSugarClient sqlSugarClient)
    {
        _wmsBaseAreaRep = wmsBaseAreaRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询仓库区域表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询仓库区域表")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsBaseAreaOutput>> Page(PageWmsBaseAreaInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsBaseAreaRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.AreaCode.Contains(input.Keyword) || u.AreaName.Contains(input.Keyword) || u.Remark.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.AreaCode), u => u.AreaCode.Contains(input.AreaCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.AreaName), u => u.AreaName.Contains(input.AreaName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Remark), u => u.Remark.Contains(input.Remark.Trim()))
            .WhereIF(input.AreaWarehouseId != null, u => u.AreaWarehouseId == input.AreaWarehouseId)
            .LeftJoin<WmsBaseWareHouse>((u, areaWarehouse) => u.AreaWarehouseId == areaWarehouse.Id)
            .Select((u, areaWarehouse) => new WmsBaseAreaOutput
            {
                Id = u.Id,
                AreaCode = u.AreaCode,
                AreaName = u.AreaName,
                AreaWarehouseId = u.AreaWarehouseId,
                AreaWarehouseFkDisplayName = $"{areaWarehouse.WarehouseName}",
                Remark = u.Remark,
                IsDelete = u.IsDelete,
                CreateTime = u.CreateTime,
                UpdateTime = u.UpdateTime,
                CreateUserId = u.CreateUserId,
                CreateUserName = u.CreateUserName,
                UpdateUserId = u.UpdateUserId,
                UpdateUserName = u.UpdateUserName,
            });
        // 代码生成多表关联的时候,需要加前缀,u. => 代指 以主表的字段排序
		return await query.OrderBuilder(input,"u.").ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取仓库区域表详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取仓库区域表详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsBaseArea> Detail([FromQuery] QueryByIdWmsBaseAreaInput input)
    {
        return await _wmsBaseAreaRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加仓库区域表 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加仓库区域表")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsBaseAreaInput input)
    {
        var entity = input.Adapt<WmsBaseArea>();
        return await _wmsBaseAreaRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新仓库区域表 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新仓库区域表")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsBaseAreaInput input)
    {
        var entity = input.Adapt<WmsBaseArea>();
        await _wmsBaseAreaRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除仓库区域表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除仓库区域表")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsBaseAreaInput input)
    {
        var entity = await _wmsBaseAreaRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsBaseAreaRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsBaseAreaRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除仓库区域表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除仓库区域表")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsBaseAreaInput> input)
    {
        var exp = Expressionable.Create<WmsBaseArea>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsBaseAreaRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsBaseAreaRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsBaseAreaRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 获取下拉列表数据 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取下拉列表数据")]
    [ApiDescriptionSettings(Name = "DropdownData"), HttpPost]
    public async Task<Dictionary<string, dynamic>> DropdownData(DropdownDataWmsBaseAreaInput input)
    {
        var areaWarehouseIdData = await _wmsBaseAreaRep.Context.Queryable<WmsBaseWareHouse>()
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.WarehouseName}"
            }).ToListAsync();
        return new Dictionary<string, dynamic>
        {
            { "areaWarehouseId", areaWarehouseIdData },
        };
    }
    
    /// <summary>
    /// 导出仓库区域表记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出仓库区域表记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsBaseAreaInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsBaseAreaOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "仓库区域表导出记录");
    }
    
    /// <summary>
    /// 下载仓库区域表数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载仓库区域表数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsBaseAreaOutput>(), "仓库区域表导入模板", (_, info) =>
        {
            if (nameof(ExportWmsBaseAreaOutput.AreaWarehouseFkDisplayName) == info.Name) return _wmsBaseAreaRep.Context.Queryable<WmsBaseWareHouse>().Select(u => $"{u.WarehouseName}").Distinct().ToList();
            return null;
        });
    }
    
    private static readonly object _wmsBaseAreaImportLock = new object();
    /// <summary>
    /// 导入仓库区域表记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入仓库区域表记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsBaseAreaImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportWmsBaseAreaInput, WmsBaseArea>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    // 链接 所属仓库
                    var areaWarehouseIdLabelList = pageItems.Where(x => x.AreaWarehouseFkDisplayName != null).Select(x => x.AreaWarehouseFkDisplayName).Distinct().ToList();
                    if (areaWarehouseIdLabelList.Any()) {
                        var areaWarehouseIdLinkMap = _wmsBaseAreaRep.Context.Queryable<WmsBaseWareHouse>().Where(u => areaWarehouseIdLabelList.Contains($"{u.WarehouseName}")).ToList().ToDictionary(u => $"{u.WarehouseName}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.AreaWarehouseId = areaWarehouseIdLinkMap.GetValueOrDefault(e.AreaWarehouseFkDisplayName ?? "");
                            if (e.AreaWarehouseId == null) e.Error = "所属仓库链接失败";
                        });
                    }
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        if (x.AreaWarehouseId == null){
                            x.Error = "所属仓库不能为空";
                            return false;
                        }
                        return true;
                    }).Adapt<List<WmsBaseArea>>();
                    
                    var storageable = _wmsBaseAreaRep.Context.Storageable(rows)
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.AreaCode), "区域编码不能为空")
                        .SplitError(it => it.Item.AreaCode?.Length > 64, "区域编码长度不能超过64个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.AreaName), "区域名称不能为空")
                        .SplitError(it => it.Item.AreaName?.Length > 64, "区域名称长度不能超过64个字符")
                        .SplitError(it => it.Item.AreaWarehouseId == null, "所属仓库不能为空")
                        .SplitError(it => it.Item.Remark?.Length > 255, "备注长度不能超过255个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.AreaCode,
                        it.AreaName,
                        it.AreaWarehouseId,
                        it.Remark,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
