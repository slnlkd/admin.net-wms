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
/// 移库任务表服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsMoveTaskService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsMoveTask> _wmsMoveTaskRep;

    public WmsMoveTaskService(SqlSugarRepository<WmsMoveTask> wmsMoveTaskRep)
    {
        _wmsMoveTaskRep = wmsMoveTaskRep;
    }

    /// <summary>
    /// 分页查询移库任务表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询移库任务表")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsMoveTaskOutput>> Page(PageWmsMoveTaskInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsMoveTaskRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.TaskNo.Contains(input.Keyword) || u.Sender.Contains(input.Keyword) || u.Receiver.Contains(input.Keyword) || u.StockCodeId.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.TaskNo), u => u.TaskNo.Contains(input.TaskNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Sender), u => u.Sender.Contains(input.Sender.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Receiver), u => u.Receiver.Contains(input.Receiver.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StockCodeId), u => u.StockCodeId.Contains(input.StockCodeId.Trim()))
            .WhereIF(input.IsSuccess != null, u => u.IsSuccess == input.IsSuccess)
            .WhereIF(input.SendDateRange?.Length == 2, u => u.SendDate >= input.SendDateRange[0] && u.SendDate <= input.SendDateRange[1])
            .WhereIF(input.FinishDateRange?.Length == 2, u => u.FinishDate >= input.FinishDateRange[0] && u.FinishDate <= input.FinishDateRange[1])
            .WhereIF(input.Status != null, u => u.Status == input.Status)
            .Select<WmsMoveTaskOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取移库任务表详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取移库任务表详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsMoveTask> Detail([FromQuery] QueryByIdWmsMoveTaskInput input)
    {
        return await _wmsMoveTaskRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加移库任务表 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加移库任务表")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsMoveTaskInput input)
    {
        var entity = input.Adapt<WmsMoveTask>();
        return await _wmsMoveTaskRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新移库任务表 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新移库任务表")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsMoveTaskInput input)
    {
        var entity = input.Adapt<WmsMoveTask>();
        await _wmsMoveTaskRep.AsUpdateable(entity)
        .IgnoreColumns(u => new {
            u.TaskNo,
            u.Sender,
            u.Receiver,
            u.IsSuccess,
            u.Information,
            u.SendDate,
            u.BackDate,
            u.MessageDate,
            u.StockCodeId,
            u.Msg,
            u.IsSend,
            u.IsCancel,
            u.IsFinish,
            u.CancelDate,
            u.FinishDate,
            u.IsShow,
            u.goupTaskId,
            u.Status,
        })
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除移库任务表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除移库任务表")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsMoveTaskInput input)
    {
        var entity = await _wmsMoveTaskRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsMoveTaskRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsMoveTaskRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除移库任务表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除移库任务表")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsMoveTaskInput> input)
    {
        var exp = Expressionable.Create<WmsMoveTask>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsMoveTaskRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsMoveTaskRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsMoveTaskRep.DeleteAsync(list);   //真删除
    }
}
