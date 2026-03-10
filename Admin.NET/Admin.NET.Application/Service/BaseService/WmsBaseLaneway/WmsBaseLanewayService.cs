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
/// 巷道表服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsBaseLanewayService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsBaseLaneway> _wmsBaseLanewayRep;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly SysDictTypeService _sysDictTypeService;

    public WmsBaseLanewayService(SqlSugarRepository<WmsBaseLaneway> wmsBaseLanewayRep, ISqlSugarClient sqlSugarClient, SysDictTypeService sysDictTypeService)
    {
        _wmsBaseLanewayRep = wmsBaseLanewayRep;
        _sqlSugarClient = sqlSugarClient;
        _sysDictTypeService = sysDictTypeService;
    }

    /// <summary>
    /// 分页查询巷道表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询巷道表")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsBaseLanewayOutput>> Page(PageWmsBaseLanewayInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsBaseLanewayRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.LanewayCode.Contains(input.Keyword) || u.LanewayName.Contains(input.Keyword) || u.Remark.Contains(input.Keyword) || u.LanewayTemp.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.LanewayCode), u => u.LanewayCode.Contains(input.LanewayCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.LanewayName), u => u.LanewayName.Contains(input.LanewayName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Remark), u => u.Remark.Contains(input.Remark.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.LanewayTemp), u => u.LanewayTemp.Contains(input.LanewayTemp.Trim()))
            .WhereIF(input.WarehouseId != null, u => u.WarehouseId == input.WarehouseId)
            .WhereIF(input.LanewayStatus.HasValue, u => u.LanewayStatus == input.LanewayStatus)
            .WhereIF(input.LanewayPriority != null, u => u.LanewayPriority == input.LanewayPriority)
            .WhereIF(input.LanewayType != null, u => u.LanewayType == input.LanewayType)
            .LeftJoin<WmsBaseWareHouse>((u, warehouse) => u.WarehouseId == warehouse.Id)
            .Select((u, warehouse) => new WmsBaseLanewayOutput
            {
                Id = u.Id,
                WarehouseId = u.WarehouseId,
                WarehouseFkDisplayName = $"{warehouse.WarehouseName}",
                LanewayCode = u.LanewayCode,
                LanewayName = u.LanewayName,
                LanewayStatus = u.LanewayStatus,
                Remark = u.Remark,
                LanewayPriority = u.LanewayPriority,
                LanewayTemp = u.LanewayTemp,
                LanewayType = u.LanewayType,
                IsDelete = u.IsDelete,
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
    /// 获取巷道表详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取巷道表详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsBaseLaneway> Detail([FromQuery] QueryByIdWmsBaseLanewayInput input)
    {
        return await _wmsBaseLanewayRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加巷道表 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加巷道表")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsBaseLanewayInput input)
    {
        var entity = input.Adapt<WmsBaseLaneway>();
        return await _wmsBaseLanewayRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新巷道表 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新巷道表")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsBaseLanewayInput input)
    {
        var entity = input.Adapt<WmsBaseLaneway>();
        await _wmsBaseLanewayRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除巷道表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除巷道表")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsBaseLanewayInput input)
    {
        var entity = await _wmsBaseLanewayRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsBaseLanewayRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsBaseLanewayRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除巷道表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除巷道表")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsBaseLanewayInput> input)
    {
        var exp = Expressionable.Create<WmsBaseLaneway>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsBaseLanewayRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsBaseLanewayRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsBaseLanewayRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 获取下拉列表数据 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取下拉列表数据")]
    [ApiDescriptionSettings(Name = "DropdownData"), HttpPost]
    public async Task<Dictionary<string, dynamic>> DropdownData(DropdownDataWmsBaseLanewayInput input)
    {
        var warehouseIdData = await _wmsBaseLanewayRep.Context.Queryable<WmsBaseWareHouse>()
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.WarehouseName}"
            }).ToListAsync();
        return new Dictionary<string, dynamic>
        {
            { "warehouseId", warehouseIdData },
        };
    }
    
    /// <summary>
    /// 导出巷道表记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出巷道表记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsBaseLanewayInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsBaseLanewayOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        var lanewayTempDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "LanewayTemp" }).Result.ToDictionary(x => x.Value, x => x.Label);
        var lanewayTypeDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "LanewayType" }).Result.ToDictionary(x => x.Value, x => x.Label);
        list.ForEach(e => {
            e.LanewayTempDictLabel = lanewayTempDictMap.GetValueOrDefault(e.LanewayTemp ?? "", e.LanewayTemp);
            e.LanewayTypeDictLabel = lanewayTypeDictMap.GetValueOrDefault(e.LanewayType.ToString() ?? "", e.LanewayType.ToString());
        });
        return ExcelHelper.ExportTemplate(list, "巷道表导出记录");
    }
    
    /// <summary>
    /// 下载巷道表数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载巷道表数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsBaseLanewayOutput>(), "巷道表导入模板", (_, info) =>
        {
            if (nameof(ExportWmsBaseLanewayOutput.WarehouseFkDisplayName) == info.Name) return _wmsBaseLanewayRep.Context.Queryable<WmsBaseWareHouse>().Select(u => $"{u.WarehouseName}").Distinct().ToList();
            return null;
        });
    }
    
    private static readonly object _wmsBaseLanewayImportLock = new object();
    /// <summary>
    /// 导入巷道表记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入巷道表记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsBaseLanewayImportLock)
        {
            var lanewayTempDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "LanewayTemp" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var lanewayTypeDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "LanewayType" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var stream = ExcelHelper.ImportData<ImportWmsBaseLanewayInput, WmsBaseLaneway>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    // 链接 所属仓库
                    var warehouseIdLabelList = pageItems.Where(x => x.WarehouseFkDisplayName != null).Select(x => x.WarehouseFkDisplayName).Distinct().ToList();
                    if (warehouseIdLabelList.Any()) {
                        var warehouseIdLinkMap = _wmsBaseLanewayRep.Context.Queryable<WmsBaseWareHouse>().Where(u => warehouseIdLabelList.Contains($"{u.WarehouseName}")).ToList().ToDictionary(u => $"{u.WarehouseName}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.WarehouseId = warehouseIdLinkMap.GetValueOrDefault(e.WarehouseFkDisplayName ?? "");
                            if (e.WarehouseId == null) e.Error = "所属仓库链接失败";
                        });
                    }
                    
                    // 映射字典值
                    foreach(var item in pageItems) {
                        System.Text.StringBuilder sbError = new System.Text.StringBuilder();
                        if (!string.IsNullOrWhiteSpace(item.LanewayTempDictLabel)) {
                            item.LanewayTemp = lanewayTempDictMap.GetValueOrDefault(item.LanewayTempDictLabel);
                            if (item.LanewayTemp == null) sbError.AppendLine("储存条件字典映射失败");
                        }
                        if (!string.IsNullOrWhiteSpace(item.LanewayTypeDictLabel)) {
                            item.LanewayType = Convert.ToInt32(lanewayTypeDictMap.GetValueOrDefault(item.LanewayTypeDictLabel));
                            if (item.LanewayType == null) sbError.AppendLine("巷道口状态字典映射失败");
                        }
                        item.Error = sbError.ToString();
                    }
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        if (x.WarehouseId == null){
                            x.Error = "所属仓库不能为空";
                            return false;
                        }
                        return true;
                    }).Adapt<List<WmsBaseLaneway>>();
                    
                    var storageable = _wmsBaseLanewayRep.Context.Storageable(rows)
                        .SplitError(it => it.Item.WarehouseId == null, "所属仓库不能为空")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.LanewayCode), "巷道编码不能为空")
                        .SplitError(it => it.Item.LanewayCode?.Length > 64, "巷道编码长度不能超过64个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.LanewayName), "巷道名称不能为空")
                        .SplitError(it => it.Item.LanewayName?.Length > 64, "巷道名称长度不能超过64个字符")
                        .SplitError(it => it.Item.Remark?.Length > 64, "备注长度不能超过64个字符")
                        .SplitError(it => it.Item.LanewayTemp?.Length > 64, "储存条件长度不能超过64个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.WarehouseId,
                        it.LanewayCode,
                        it.LanewayName,
                        it.LanewayStatus,
                        it.Remark,
                        it.LanewayPriority,
                        it.LanewayTemp,
                        it.LanewayType,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
