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
/// 出库任务服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsExportTaskService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsExportTask> _wmsExportTaskRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsExportTaskService(SqlSugarRepository<WmsExportTask> wmsExportTaskRep, ISqlSugarClient sqlSugarClient)
    {
        _wmsExportTaskRep = wmsExportTaskRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询出库任务 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询出库任务")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsExportTaskOutput>> Page(PageWmsExportTaskInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsExportTaskRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.ExportTaskNo.Contains(input.Keyword) || u.StockCode.Contains(input.Keyword) || u.StockSoltCode.Contains(input.Keyword) || u.ExportOrderNo.Contains(input.Keyword) || u.StartLocation.Contains(input.Keyword) || u.EndLocation.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportTaskNo), u => u.ExportTaskNo.Contains(input.ExportTaskNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StockCode), u => u.StockCode.Contains(input.StockCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StockSoltCode), u => u.StockSoltCode.Contains(input.StockSoltCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportOrderNo), u => u.ExportOrderNo.Contains(input.ExportOrderNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StartLocation), u => u.StartLocation.Contains(input.StartLocation.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.EndLocation), u => u.EndLocation.Contains(input.EndLocation.Trim()))
            .WhereIF(input.IsSuccess != null, u => u.IsSuccess == input.IsSuccess)
            .WhereIF(input.SendDateRange?.Length == 2, u => u.SendDate >= input.SendDateRange[0] && u.SendDate <= input.SendDateRange[1])
            .WhereIF(input.ExportTaskFlag != null, u => u.ExportTaskFlag == input.ExportTaskFlag)
            .WhereIF(input.WarehouseId != null, u => u.WarehouseId == input.WarehouseId)
            .WhereIF(input.TaskType != null, u => u.TaskType == input.TaskType)
            .Select<WmsExportTaskOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取出库任务详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取出库任务详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsExportTask> Detail([FromQuery] QueryByIdWmsExportTaskInput input)
    {
        return await _wmsExportTaskRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加出库任务 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加出库任务")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsExportTaskInput input)
    {
        var entity = input.Adapt<WmsExportTask>();
        return await _wmsExportTaskRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新出库任务 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新出库任务")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsExportTaskInput input)
    {
        var entity = input.Adapt<WmsExportTask>();
        await _wmsExportTaskRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除出库任务 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除出库任务")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsExportTaskInput input)
    {
        var entity = await _wmsExportTaskRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsExportTaskRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsExportTaskRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除出库任务 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除出库任务")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsExportTaskInput> input)
    {
        var exp = Expressionable.Create<WmsExportTask>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsExportTaskRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsExportTaskRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsExportTaskRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 导出出库任务记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出出库任务记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsExportTaskInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsExportTaskOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "出库任务导出记录");
    }
    
    /// <summary>
    /// 下载出库任务数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载出库任务数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsExportTaskOutput>(), "出库任务导入模板");
    }
    
    private static readonly object _wmsExportTaskImportLock = new object();
    /// <summary>
    /// 导入出库任务记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入出库任务记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsExportTaskImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportWmsExportTaskInput, WmsExportTask>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<WmsExportTask>>();
                    
                    var storageable = _wmsExportTaskRep.Context.Storageable(rows)
                        .SplitError(it => it.Item.ExportTaskNo?.Length > 100, "出库任务号长度不能超过100个字符")
                        .SplitError(it => it.Item.Sender?.Length > 100, "发送方长度不能超过100个字符")
                        .SplitError(it => it.Item.Receiver?.Length > 100, "接收方长度不能超过100个字符")
                        .SplitError(it => it.Item.Information?.Length > 2147483647, "信息长度不能超过2147483647个字符")
                        .SplitError(it => it.Item.StockCode?.Length > 100, "托盘条码长度不能超过100个字符")
                        .SplitError(it => it.Item.StockSoltCode?.Length > 100, "储位编码长度不能超过100个字符")
                        .SplitError(it => it.Item.Msg?.Length > 500, "描述信息长度不能超过500个字符")
                        .SplitError(it => it.Item.ExportOrderNo?.Length > 100, "出库流水号长度不能超过100个字符")
                        .SplitError(it => it.Item.StartLocation?.Length > 100, "起始位置长度不能超过100个字符")
                        .SplitError(it => it.Item.EndLocation?.Length > 100, "目标位置长度不能超过100个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.ExportTaskNo,
                        it.Sender,
                        it.Receiver,
                        it.IsSuccess,
                        it.Information,
                        it.SendDate,
                        it.BackDate,
                        it.StockCode,
                        it.StockSoltCode,
                        it.Msg,
                        it.ExportTaskFlag,
                        it.ExportOrderNo,
                        it.StartLocation,
                        it.EndLocation,
                        it.WarehouseId,
                        it.TaskType,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
