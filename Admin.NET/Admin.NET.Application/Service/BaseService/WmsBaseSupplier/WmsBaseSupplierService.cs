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
/// 供应商信息服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsBaseSupplierService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsBaseSupplier> _wmsBaseSupplierRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsBaseSupplierService(SqlSugarRepository<WmsBaseSupplier> wmsBaseSupplierRep, ISqlSugarClient sqlSugarClient)
    {
        _wmsBaseSupplierRep = wmsBaseSupplierRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询供应商信息 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询供应商信息")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsBaseSupplierOutput>> Page(PageWmsBaseSupplierInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsBaseSupplierRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.CustomerCode.Contains(input.Keyword) || u.CustomerName.Contains(input.Keyword) || u.CustomerAddress.Contains(input.Keyword) || u.CustomerLinkman.Contains(input.Keyword) || u.CustomerPhone.Contains(input.Keyword) || u.Remark.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CustomerCode), u => u.CustomerCode.Contains(input.CustomerCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CustomerName), u => u.CustomerName.Contains(input.CustomerName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CustomerAddress), u => u.CustomerAddress.Contains(input.CustomerAddress.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CustomerLinkman), u => u.CustomerLinkman.Contains(input.CustomerLinkman.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CustomerPhone), u => u.CustomerPhone.Contains(input.CustomerPhone.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Remark), u => u.Remark.Contains(input.Remark.Trim()))
            .Select<WmsBaseSupplierOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取供应商信息详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取供应商信息详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsBaseSupplier> Detail([FromQuery] QueryByIdWmsBaseSupplierInput input)
    {
        return await _wmsBaseSupplierRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加供应商信息 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加供应商信息")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsBaseSupplierInput input)
    {
        var entity = input.Adapt<WmsBaseSupplier>();
        return await _wmsBaseSupplierRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新供应商信息 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新供应商信息")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsBaseSupplierInput input)
    {
        var entity = input.Adapt<WmsBaseSupplier>();
        await _wmsBaseSupplierRep.AsUpdateable(entity)
        .IgnoreColumns(u => new {
            u.CustomerTypeId,
            u.token,
            u.accountExec,
            u.Status,
        })
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除供应商信息 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除供应商信息")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsBaseSupplierInput input)
    {
        var entity = await _wmsBaseSupplierRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsBaseSupplierRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsBaseSupplierRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除供应商信息 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除供应商信息")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsBaseSupplierInput> input)
    {
        var exp = Expressionable.Create<WmsBaseSupplier>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsBaseSupplierRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsBaseSupplierRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsBaseSupplierRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 导出供应商信息记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出供应商信息记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsBaseSupplierInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsBaseSupplierOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "供应商信息导出记录");
    }
    
    /// <summary>
    /// 下载供应商信息数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载供应商信息数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsBaseSupplierOutput>(), "供应商信息导入模板");
    }
    
    private static readonly object _wmsBaseSupplierImportLock = new object();
    /// <summary>
    /// 导入供应商信息记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入供应商信息记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsBaseSupplierImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportWmsBaseSupplierInput, WmsBaseSupplier>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<WmsBaseSupplier>>();
                    
                    var storageable = _wmsBaseSupplierRep.Context.Storageable(rows)
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.CustomerCode), "供应商编码不能为空")
                        .SplitError(it => it.Item.CustomerCode?.Length > 32, "供应商编码长度不能超过32个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.CustomerName), "供应商名称不能为空")
                        .SplitError(it => it.Item.CustomerName?.Length > 32, "供应商名称长度不能超过32个字符")
                        .SplitError(it => it.Item.CustomerAddress?.Length > 32, "供应商地址长度不能超过32个字符")
                        .SplitError(it => it.Item.CustomerLinkman?.Length > 32, "联系人长度不能超过32个字符")
                        .SplitError(it => it.Item.CustomerPhone?.Length > 32, "联系电话长度不能超过32个字符")
                        .SplitError(it => it.Item.Remark?.Length > 32, "备注长度不能超过32个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.CustomerCode,
                        it.CustomerName,
                        it.CustomerAddress,
                        it.CustomerLinkman,
                        it.CustomerPhone,
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
