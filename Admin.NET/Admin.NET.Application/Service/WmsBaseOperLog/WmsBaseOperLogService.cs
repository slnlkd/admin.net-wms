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
/// 业务操作日志服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsBaseOperLogService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsBaseOperLog> _wmsBaseOperLogRep;
    private readonly ISqlSugarClient _sqlSugarClient;


    public WmsBaseOperLogService(SqlSugarRepository<WmsBaseOperLog> wmsBaseOperLogRep, ISqlSugarClient sqlSugarClient = null)
    {
        _wmsBaseOperLogRep = wmsBaseOperLogRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询业务操作日志 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询业务操作日志")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsBaseOperLogOutput>> Page(PageWmsBaseOperLogInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsBaseOperLogRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.TraceId.Contains(input.Keyword) || u.Module.Contains(input.Keyword) || u.OperationType.Contains(input.Keyword) || u.Operator.Contains(input.Keyword) || u.OperateIp.Contains(input.Keyword) || u.BusinessNo.Contains(input.Keyword) || u.OperationContent.Contains(input.Keyword) || u.BeforeDataSummary.Contains(input.Keyword) || u.AfterDataSummary.Contains(input.Keyword) || u.Result.Contains(input.Keyword) || u.ExtraInfo.Contains(input.Keyword) || u.OperParam.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.TraceId), u => u.TraceId.Contains(input.TraceId.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Module), u => u.Module.Contains(input.Module.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.OperationType), u => u.OperationType.Contains(input.OperationType.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Operator), u => u.Operator.Contains(input.Operator.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.OperateIp), u => u.OperateIp.Contains(input.OperateIp.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.BusinessNo), u => u.BusinessNo.Contains(input.BusinessNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.OperationContent), u => u.OperationContent.Contains(input.OperationContent.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.BeforeDataSummary), u => u.BeforeDataSummary.Contains(input.BeforeDataSummary.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.AfterDataSummary), u => u.AfterDataSummary.Contains(input.AfterDataSummary.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Result), u => u.Result.Contains(input.Result.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExtraInfo), u => u.ExtraInfo.Contains(input.ExtraInfo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.OperParam), u => u.OperParam.Contains(input.OperParam.Trim()))
            .WhereIF(input.OperateTimeRange?.Length == 2, u => u.OperateTime >= input.OperateTimeRange[0] && u.OperateTime <= input.OperateTimeRange[1])
            .WhereIF(input.ElapsedMs != null, u => u.ElapsedMs == input.ElapsedMs)
            .Select<WmsBaseOperLogOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取业务操作日志详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取业务操作日志详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<SysLogOp> Detail([FromQuery] QueryByIdWmsBaseOperLogInput input)
    {
        var data = await _wmsBaseOperLogRep.GetFirstAsync(u => u.Id == input.Id);
        return await _sqlSugarClient.Queryable<SysLogOp>().Where(u => u.TraceId == data.TraceId).SingleAsync();
    }

    /// <summary>
    /// 增加业务操作日志 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加业务操作日志")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsBaseOperLogInput input)
    {
        var entity = input.Adapt<WmsBaseOperLog>();
        return await _wmsBaseOperLogRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新业务操作日志 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新业务操作日志")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsBaseOperLogInput input)
    {
        var entity = input.Adapt<WmsBaseOperLog>();
        await _wmsBaseOperLogRep.AsUpdateable(entity)
        .IgnoreColumns(u => new {
            u.TraceId,
            u.Module,
            u.OperationType,
            u.Operator,
            u.OperateTime,
            u.OperateIp,
            u.BusinessNo,
            u.OperationContent,
            u.BeforeDataSummary,
            u.AfterDataSummary,
            u.Result,
            u.ElapsedMs,
            u.ExtraInfo,
            u.OperParam,
        })
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除业务操作日志 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除业务操作日志")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsBaseOperLogInput input)
    {
        var entity = await _wmsBaseOperLogRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsBaseOperLogRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除业务操作日志 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除业务操作日志")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<bool> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsBaseOperLogInput> input)
    {
        var exp = Expressionable.Create<WmsBaseOperLog>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsBaseOperLogRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsBaseOperLogRep.DeleteAsync(list);   //真删除
    }
}
