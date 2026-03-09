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
/// 仓库表服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsBaseWareHouseService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsBaseWareHouse> _wmsBaseWareHouseRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsBaseWareHouseService(SqlSugarRepository<WmsBaseWareHouse> wmsBaseWareHouseRep, ISqlSugarClient sqlSugarClient)
    {
        _wmsBaseWareHouseRep = wmsBaseWareHouseRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询仓库表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询仓库表")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsBaseWareHouseOutput>> Page(PageWmsBaseWareHouseInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsBaseWareHouseRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.WarehouseCode.Contains(input.Keyword) || u.WarehouseName.Contains(input.Keyword) || u.WarehouseType.Contains(input.Keyword) || u.Remark.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.WarehouseCode), u => u.WarehouseCode.Contains(input.WarehouseCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.WarehouseName), u => u.WarehouseName.Contains(input.WarehouseName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.WarehouseType), u => u.WarehouseType.Contains(input.WarehouseType.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Remark), u => u.Remark.Contains(input.Remark.Trim()))
            .WhereIF(input.IsExceeding.HasValue, u => u.IsExceeding == input.IsExceeding)
            .WhereIF(input.IsOverbooking.HasValue, u => u.IsOverbooking == input.IsOverbooking)
            .WhereIF(input.IsUnderpay.HasValue, u => u.IsUnderpay == input.IsUnderpay)
            .WhereIF(input.IsEnterless.HasValue, u => u.IsEnterless == input.IsEnterless)
            .Select<WmsBaseWareHouseOutput>();
		return await query.OrderBy(u => u.Id, OrderByType.Asc).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取仓库表详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取仓库表详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsBaseWareHouse> Detail([FromQuery] QueryByIdWmsBaseWareHouseInput input)
    {
        return await _wmsBaseWareHouseRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加仓库表 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加仓库表")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsBaseWareHouseInput input)
    {
        var entity = input.Adapt<WmsBaseWareHouse>();
        return await _wmsBaseWareHouseRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新仓库表 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新仓库表")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsBaseWareHouseInput input)
    {
        var entity = input.Adapt<WmsBaseWareHouse>();
        await _wmsBaseWareHouseRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除仓库表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除仓库表")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsBaseWareHouseInput input)
    {
        var entity = await _wmsBaseWareHouseRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsBaseWareHouseRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsBaseWareHouseRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除仓库表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除仓库表")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsBaseWareHouseInput> input)
    {
        var exp = Expressionable.Create<WmsBaseWareHouse>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsBaseWareHouseRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsBaseWareHouseRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsBaseWareHouseRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 导出仓库表记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出仓库表记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsBaseWareHouseInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsBaseWareHouseOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "仓库表导出记录");
    }
    
    /// <summary>
    /// 下载仓库表数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载仓库表数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsBaseWareHouseOutput>(), "仓库表导入模板");
    }
    
    private static readonly object _wmsBaseWareHouseImportLock = new object();
    /// <summary>
    /// 导入仓库表记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入仓库表记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsBaseWareHouseImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportWmsBaseWareHouseInput, WmsBaseWareHouse>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<WmsBaseWareHouse>>();
                    
                    var storageable = _wmsBaseWareHouseRep.Context.Storageable(rows)
                        .SplitError(it => it.Item.WarehouseCode?.Length > 32, "仓库编码长度不能超过32个字符")
                        .SplitError(it => it.Item.WarehouseName?.Length > 32, "仓库名称长度不能超过32个字符")
                        .SplitError(it => it.Item.WarehouseType?.Length > 32, "仓库类型长度不能超过32个字符")
                        .SplitError(it => it.Item.Remark?.Length > 32, "备注长度不能超过32个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.WarehouseCode,
                        it.WarehouseName,
                        it.WarehouseType,
                        it.Remark,
                        it.IsExceeding,
                        it.IsOverbooking,
                        it.IsUnderpay,
                        it.IsEnterless,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
