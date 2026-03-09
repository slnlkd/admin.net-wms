// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsBaseOperLog;
using Admin.NET.Application.Service.WmsBaseOperLog.Enum;
using Admin.NET.Core.Service;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Mapster;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SqlSugar;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reactive.Concurrency;
namespace Admin.NET.Application;

/// <summary>
/// 计量单位服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsBaseUnitService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsBaseUnit> _wmsBaseUnitRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsBaseUnitService(SqlSugarRepository<WmsBaseUnit> wmsBaseUnitRep, ISqlSugarClient sqlSugarClient)
    {
        _wmsBaseUnitRep = wmsBaseUnitRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询计量单位 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询计量单位")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsBaseUnitOutput>> Page(PageWmsBaseUnitInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsBaseUnitRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.UnitCode.Contains(input.Keyword) || u.UnitName.Contains(input.Keyword) || u.UnitAbbrevName.Contains(input.Keyword) || u.Remark.Contains(input.Keyword) || u.CreateUserName.Contains(input.Keyword) || u.UpdateUserName.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.UnitCode), u => u.UnitCode.Contains(input.UnitCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.UnitName), u => u.UnitName.Contains(input.UnitName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.UnitAbbrevName), u => u.UnitAbbrevName.Contains(input.UnitAbbrevName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Remark), u => u.Remark.Contains(input.Remark.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CreateUserName), u => u.CreateUserName.Contains(input.CreateUserName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.UpdateUserName), u => u.UpdateUserName.Contains(input.UpdateUserName.Trim()))
            .Select<WmsBaseUnitOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取计量单位详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取计量单位详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsBaseUnit> Detail([FromQuery] QueryByIdWmsBaseUnitInput input)
    {
        // 计时器
        var stopwatch = Stopwatch.StartNew();
        var data = await _wmsBaseUnitRep.GetFirstAsync(u => u.Id == input.Id);
        await WmsBaseOperLogHelper.RecordAsync(
            Module: "计量单位", // 模块名称
            OperationType: OperationTypeEnum.Detail.GetEnumDescription(), // 操作类型
            BusinessNo: input.Id.ToString(), // 主键Id
            OperationContent: $"查看计量单位名称为：【{data.UnitName} 】的详情数据", // 具体描述（自定义） 可以使用 WmsBaseOperLogHelper.GenerateDescription 自动根据 特性描述来拼接实体中文描述
            OperParam: input, // 输入参数
            ExtraInfo: $"执行时间：{stopwatch.ElapsedMilliseconds}ms", // 执行耗时内容 （自定义）
            ElapsedMs: stopwatch.ElapsedMilliseconds // 毫秒
        );
        stopwatch.Stop();
        return data;
    }

    /// <summary>
    /// 增加计量单位 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加计量单位")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsBaseUnitInput input)
    {
        var entity = input.Adapt<WmsBaseUnit>();
        // 单位编码自增方法 
        var list = _wmsBaseUnitRep.GetList(m=>m.IsDelete == false).OrderBy(m => m.CreateTime).ToList();
        var code = "01";
        if (list != null && list.Count() > 0)
        {
            var unitCode = list.Last().UnitCode;
            string str = unitCode.Substring(0, 1);
            string str2 = unitCode.Substring(1, 1);
            code = str == "0" ? (str2 == "9" ? (int.Parse(str2) + 1).ToString() : "0" + (int.Parse(str2) + 1))
                : (int.Parse(unitCode) + 1).ToString();
        }
        entity.UnitCode = code;
        var resId = await _wmsBaseUnitRep.InsertAsync(entity) ? entity.Id : 0;
        await WmsBaseOperLogHelper.RecordAsync(Module: "计量单位",
            OperationType: OperationTypeEnum.Add.GetEnumDescription(),
            BusinessNo: resId.ToString(),
            OperationContent: $"新增计量单位名称为：【{input.UnitName} 】的详情数据，Id为【{resId}】",
            OperParam: input
            //ExtraInfo: $"执行时间：{stopwatch.ElapsedMilliseconds}ms",
            //ElapsedMs: stopwatch.ElapsedMilliseconds
            );
        return resId;
    }

    /// <summary>
    /// 更新计量单位 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新计量单位")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsBaseUnitInput input)
    {
        WmsBaseUnit entity = input.Adapt<WmsBaseUnit>();
        WmsBaseUnit before = _sqlSugarClient.Queryable<WmsBaseUnit>().Where(u => u.Id == input.Id).Single();
        await WmsBaseOperLogHelper.RecordAsync(Module: "计量单位",
            OperationType: OperationTypeEnum.Update.GetEnumDescription(),
            BusinessNo: input.Id.ToString(),
            OperationContent: $"修改计量单位:\n{WmsBaseOperLogHelper.GenerateChangeDescription(before, entity)}",
            OperParam: input,
            BeforeData: before,
            AfterData: input
        );
        await _wmsBaseUnitRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除计量单位 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除计量单位")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsBaseUnitInput input)
    {
        var entity = await _wmsBaseUnitRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await WmsBaseOperLogHelper.RecordAsync(Module: "计量单位",
            OperationType: OperationTypeEnum.Delete.GetEnumDescription(),
            BusinessNo: input.Id.ToString(),
            OperationContent: $"删除计量单位:\n{WmsBaseOperLogHelper.GenerateDescription(entity)}",
            OperParam: input,
            BeforeData: _sqlSugarClient.Queryable<WmsBaseUnit>().Where(u => u.Id == input.Id).Single()
        );
        await _wmsBaseUnitRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsBaseUnitRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除计量单位 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除计量单位")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsBaseUnitInput> input)
    {
        var exp = Expressionable.Create<WmsBaseUnit>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsBaseUnitRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsBaseUnitRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsBaseUnitRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 导出计量单位记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出计量单位记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsBaseUnitInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsBaseUnitOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "计量单位导出记录");
    }
    
    /// <summary>
    /// 下载计量单位数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载计量单位数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsBaseUnitOutput>(), "计量单位导入模板");
    }
    
    private static readonly object _wmsBaseUnitImportLock = new object();
    /// <summary>
    /// 导入计量单位记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入计量单位记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsBaseUnitImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportWmsBaseUnitInput, WmsBaseUnit>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<WmsBaseUnit>>();

                    try
                    {
                        // 方案1：使用普通的插入和更新操作替代 Storageable
                        var existingCodes = _wmsBaseUnitRep.Context.Queryable<WmsBaseUnit>()
                            .Where(it => rows.Select(x => x.UnitCode).Contains(it.UnitCode))
                            .Select(it => it.UnitCode)
                            .ToList();

                        var toInsert = rows.Where(x => !existingCodes.Contains(x.UnitCode)).ToList();
                        var toUpdate = rows.Where(x => existingCodes.Contains(x.UnitCode)).ToList();

                        if (toInsert.Any())
                        {
                            _wmsBaseUnitRep.Context.Insertable(toInsert).ExecuteCommand();
                        }

                        if (toUpdate.Any())
                        {
                            _wmsBaseUnitRep.Context.Updateable(toUpdate)
                                .UpdateColumns(it => new
                                {
                                    it.UnitName,
                                    it.UnitAbbrevName,
                                    it.Remark
                                })
                                .ExecuteCommand();
                        }
                    }
                    catch (Exception ex)
                    {
                        // 处理异常
                        throw new Exception($"导入失败: {ex.Message}");
                    }
                });
            });

            return stream;
        }
    }
}
