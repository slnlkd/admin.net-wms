// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Admin.NET.Application.Entity;
using Admin.NET.Core.Service;


using Furion.DatabaseAccessor;
using Furion.FriendlyException;

using Mapster;

using Microsoft.AspNetCore.Http;

using SqlSugar;
namespace Admin.NET.Application;

/// <summary>
/// 按钮点击表服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
[Obsolete("请使用[WmsBaseOperLogHelper.RecordAsync]代替")]
public partial class WmsButtonRecordService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsButtonRecord> _wmsButtonRecordRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsButtonRecordService(SqlSugarRepository<WmsButtonRecord> wmsButtonRecordRep, ISqlSugarClient sqlSugarClient)
    {
        _wmsButtonRecordRep = wmsButtonRecordRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 增加按钮点击表 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加按钮点击表")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsButtonRecordInput input)
    {
        input.UserId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
        input.UserName = App.User?.FindFirst(ClaimConst.RealName)?.Value;
        input.ButtonInformation = input.UserName + " 在 " + DateTime.Now + " " + input.ButtonInformation;
        var entity = input.Adapt<WmsButtonRecord>();
        return await _wmsButtonRecordRep.InsertAsync(entity) ? entity.Id : 0;
    }
    /// <summary>
    /// 批量增加按钮点击表 ➕
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    [DisplayName("批量增加按钮点击表")]
    [ApiDescriptionSettings(Name = "AddList"), HttpPost]
    public async Task<bool> AddList(List<AddWmsButtonRecordInput> inputs)
    {
        foreach (var input in inputs)
        {
            input.UserId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
            input.UserName = App.User?.FindFirst(ClaimConst.RealName)?.Value;
            input.ButtonInformation = input.UserName + " 在 " + DateTime.Now + " " + input.ButtonInformation;
        }
        var entitys = inputs.Adapt<List<WmsButtonRecord>>();
        return await _wmsButtonRecordRep.InsertRangeAsync(entitys);
    }

}
