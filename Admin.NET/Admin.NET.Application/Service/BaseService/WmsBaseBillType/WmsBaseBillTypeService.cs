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
/// 单据类型表服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsBaseBillTypeService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsBaseBillType> _wmsBaseBillTypeRep;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly SysDictTypeService _sysDictTypeService;

    public WmsBaseBillTypeService(SqlSugarRepository<WmsBaseBillType> wmsBaseBillTypeRep, ISqlSugarClient sqlSugarClient, SysDictTypeService sysDictTypeService)
    {
        _wmsBaseBillTypeRep = wmsBaseBillTypeRep;
        _sqlSugarClient = sqlSugarClient;
        _sysDictTypeService = sysDictTypeService;
    }

    /// <summary>
    /// 分页查询单据类型表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询单据类型表")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsBaseBillTypeOutput>> Page(PageWmsBaseBillTypeInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsBaseBillTypeRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.BillTypeCode.Contains(input.Keyword) || u.BillTypeName.Contains(input.Keyword) || u.Remark.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.BillTypeCode), u => u.BillTypeCode.Contains(input.BillTypeCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.BillTypeName), u => u.BillTypeName.Contains(input.BillTypeName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Remark), u => u.Remark.Contains(input.Remark.Trim()))
            .WhereIF(input.BillType != null, u => u.BillType == input.BillType)
            .WhereIF(!string.IsNullOrWhiteSpace(input.QualityInspectionStatus), u => u.QualityInspectionStatus.Contains(input.QualityInspectionStatus))
            .WhereIF(input.ProxyStatus != null, u => u.ProxyStatus == input.ProxyStatus)
            .WhereIF(input.WareHouseId != null, u => u.WareHouseId == input.WareHouseId)
            .LeftJoin<WmsBaseWareHouse>((u, wareHouse) => u.WareHouseId == wareHouse.Id)
            .Select((u, wareHouse) => new WmsBaseBillTypeOutput
            {
                Id = u.Id,
                BillTypeCode = u.BillTypeCode,
                BillTypeName = u.BillTypeName,
                Remark = u.Remark,
                BillType = u.BillType,
                IsDelete = u.IsDelete,
                QualityInspectionStatus = u.QualityInspectionStatus,
                ProxyStatus = u.ProxyStatus,
                WareHouseId = u.WareHouseId,
                WareHouseFkDisplayName = $"{wareHouse.WarehouseName}",
                SubtypeID = u.SubtypeID,
                CreateTime = u.CreateTime,
                UpdateTime = u.UpdateTime,
                CreateUserId = u.CreateUserId,
                CreateUserName = u.CreateUserName,
                UpdateUserId = u.UpdateUserId,
                UpdateUserName = u.UpdateUserName,
            });
        var result = await query.OrderBuilder(input, "u.").ToPagedListAsync(input.Page, input.PageSize);
        var qualityInspectionStatusDictMap = (await _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "QualityInspectionStatus" })).ToDictionary(x => x.Value?.Trim() ?? "", x => x.Label);
        foreach (var item in result.Items)
        {
            if (!string.IsNullOrWhiteSpace(item.QualityInspectionStatus))
            {
                var statusList = item.QualityInspectionStatus.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var nameList = statusList.Select(x => qualityInspectionStatusDictMap.GetValueOrDefault(x.Trim(), x)).ToList();
                item.QualityInspectionStatusName = string.Join("，", nameList);
            }
        }
        return result;
    }

    /// <summary>
    /// 获取单据类型表详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取单据类型表详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsBaseBillType> Detail([FromQuery] QueryByIdWmsBaseBillTypeInput input)
    {
        return await _wmsBaseBillTypeRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加单据类型表 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加单据类型表")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsBaseBillTypeInput input)
    {
        var entity = input.Adapt<WmsBaseBillType>();
        return await _wmsBaseBillTypeRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新单据类型表 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新单据类型表")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsBaseBillTypeInput input)
    {
        var entity = input.Adapt<WmsBaseBillType>();
        await _wmsBaseBillTypeRep.AsUpdateable(entity)
        .IgnoreColumns(u => new {
            u.SubtypeID,
        })
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除单据类型表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除单据类型表")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsBaseBillTypeInput input)
    {
        var entity = await _wmsBaseBillTypeRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsBaseBillTypeRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsBaseBillTypeRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除单据类型表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除单据类型表")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsBaseBillTypeInput> input)
    {
        var exp = Expressionable.Create<WmsBaseBillType>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsBaseBillTypeRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsBaseBillTypeRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsBaseBillTypeRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 获取下拉列表数据 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取下拉列表数据")]
    [ApiDescriptionSettings(Name = "DropdownData"), HttpPost]
    public async Task<Dictionary<string, dynamic>> DropdownData(DropdownDataWmsBaseBillTypeInput input)
    {
        var wareHouseIdData = await _wmsBaseBillTypeRep.Context.Queryable<WmsBaseWareHouse>()
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.WarehouseName}"
            }).ToListAsync();
        return new Dictionary<string, dynamic>
        {
            { "wareHouseId", wareHouseIdData },
        };
    }
    
    /// <summary>
    /// 导出单据类型表记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出单据类型表记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsBaseBillTypeInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsBaseBillTypeOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        var billTypeDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "BillType" }).Result.ToDictionary(x => x.Value, x => x.Label);
        var qualityInspectionStatusDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "QualityInspectionStatus" }).Result.ToDictionary(x => x.Value, x => x.Label);
        var proxyStatusDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "ProxyStatus" }).Result.ToDictionary(x => x.Value, x => x.Label);
        list.ForEach(e => {
            e.BillTypeDictLabel = billTypeDictMap.GetValueOrDefault(e.BillType.ToString() ?? "", e.BillType.ToString());
            e.QualityInspectionStatusDictLabel = e.QualityInspectionStatusDictLabel;
            e.ProxyStatusDictLabel = proxyStatusDictMap.GetValueOrDefault(e.ProxyStatus.ToString() ?? "", e.ProxyStatus.ToString());
        });
        return ExcelHelper.ExportTemplate(list, "单据类型表导出记录");
    }
    
    /// <summary>
    /// 下载单据类型表数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载单据类型表数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsBaseBillTypeOutput>(), "单据类型表导入模板", (_, info) =>
        {
            if (nameof(ExportWmsBaseBillTypeOutput.WareHouseFkDisplayName) == info.Name) return _wmsBaseBillTypeRep.Context.Queryable<WmsBaseWareHouse>().Select(u => $"{u.WarehouseName}").Distinct().ToList();
            return null;
        });
    }
    
    private static readonly object _wmsBaseBillTypeImportLock = new object();
    /// <summary>
    /// 导入单据类型表记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入单据类型表记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsBaseBillTypeImportLock)
        {
            var billTypeDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "BillType" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var qualityInspectionStatusDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "QualityInspectionStatus" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var proxyStatusDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "ProxyStatus" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var stream = ExcelHelper.ImportData<ImportWmsBaseBillTypeInput, WmsBaseBillType>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    // 链接 所属仓库
                    var wareHouseIdLabelList = pageItems.Where(x => x.WareHouseFkDisplayName != null).Select(x => x.WareHouseFkDisplayName).Distinct().ToList();
                    if (wareHouseIdLabelList.Any()) {
                        var wareHouseIdLinkMap = _wmsBaseBillTypeRep.Context.Queryable<WmsBaseWareHouse>().Where(u => wareHouseIdLabelList.Contains($"{u.WarehouseName}")).ToList().ToDictionary(u => $"{u.WarehouseName}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.WareHouseId = wareHouseIdLinkMap.GetValueOrDefault(e.WareHouseFkDisplayName ?? "");
                            if (e.WareHouseId == null) e.Error = "所属仓库链接失败";
                        });
                    }
                    
                    // 映射字典值
                    foreach(var item in pageItems) {
                        System.Text.StringBuilder sbError = new System.Text.StringBuilder();
                        if (!string.IsNullOrWhiteSpace(item.BillTypeDictLabel)) {
                            item.BillType = Convert.ToInt32(billTypeDictMap.GetValueOrDefault(item.BillTypeDictLabel));
                            if (item.BillType == null) sbError.AppendLine("类型字典映射失败");
                        }
                        if (!string.IsNullOrWhiteSpace(item.QualityInspectionStatusDictLabel)) {
                            var labels = item.QualityInspectionStatusDictLabel.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                            var ids = labels.Select(l => qualityInspectionStatusDictMap.GetValueOrDefault(l.Trim())).Where(id => id != null).ToList();
                            if (ids.Count != labels.Length) sbError.AppendLine("质检状态字典映射失败");
                            item.QualityInspectionStatus = string.Join(",", ids);
                        }
                        if (!string.IsNullOrWhiteSpace(item.ProxyStatusDictLabel)) {
                            item.ProxyStatus = Convert.ToInt32(proxyStatusDictMap.GetValueOrDefault(item.ProxyStatusDictLabel));
                            if (item.ProxyStatus == null) sbError.AppendLine("代储状态字典映射失败");
                        }
                        item.Error = sbError.ToString();
                    }
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        if (x.BillType == null){
                            x.Error = "类型不能为空";
                            return false;
                        }
                        if (string.IsNullOrWhiteSpace(x.QualityInspectionStatus)){
                            x.Error = "质检状态不能为空";
                            return false;
                        }
                        if (x.ProxyStatus == null){
                            x.Error = "代储状态不能为空";
                            return false;
                        }
                        if (x.WareHouseId == null){
                            x.Error = "所属仓库不能为空";
                            return false;
                        }
                        return true;
                    }).Adapt<List<WmsBaseBillType>>();
                    
                    var storageable = _wmsBaseBillTypeRep.Context.Storageable(rows)
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.BillTypeCode), "单据类型编码不能为空")
                        .SplitError(it => it.Item.BillTypeCode?.Length > 64, "单据类型编码长度不能超过64个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.BillTypeName), "单据类型名称不能为空")
                        .SplitError(it => it.Item.BillTypeName?.Length > 64, "单据类型名称长度不能超过64个字符")
                        .SplitError(it => it.Item.Remark?.Length > 255, "备注长度不能超过255个字符")
                        .SplitError(it => it.Item.BillType == null, "类型不能为空")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.QualityInspectionStatus), "质检状态不能为空")
                        .SplitError(it => it.Item.ProxyStatus == null, "代储状态不能为空")
                        .SplitError(it => it.Item.WareHouseId == null, "所属仓库不能为空")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.BillTypeCode,
                        it.BillTypeName,
                        it.Remark,
                        it.BillType,
                        it.QualityInspectionStatus,
                        it.ProxyStatus,
                        it.WareHouseId,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
