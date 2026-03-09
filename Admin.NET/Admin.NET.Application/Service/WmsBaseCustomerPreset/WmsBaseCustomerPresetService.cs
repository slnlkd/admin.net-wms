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
/// 申请客户信息服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsBaseCustomerPresetService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsBaseCustomerPreset> _wmsBaseCustomerPresetRep;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly SysDictTypeService _sysDictTypeService;
    private readonly SqlSugarRepository<WmsBaseCustomer> _wmsBaseCustomerRep;

    public WmsBaseCustomerPresetService(SqlSugarRepository<WmsBaseCustomerPreset> wmsBaseCustomerPresetRep, ISqlSugarClient sqlSugarClient, SysDictTypeService sysDictTypeService, SqlSugarRepository<WmsBaseCustomer> wmsBaseCustomerRep)
    {
        _wmsBaseCustomerPresetRep = wmsBaseCustomerPresetRep;
        _sqlSugarClient = sqlSugarClient;
        _sysDictTypeService = sysDictTypeService;
        _wmsBaseCustomerRep = wmsBaseCustomerRep;
    }

    /// <summary>
    /// 分页查询申请客户信息 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询申请客户信息")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsBaseCustomerPresetOutput>> Page(PageWmsBaseCustomerPresetInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var UserId = App.User?.FindFirst(ClaimConst.UserId)?.Value;
        var UserType = App.User?.FindFirst(ClaimConst.AccountType).Value;
        var query = _wmsBaseCustomerPresetRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.CustomerCode.Contains(input.Keyword) || u.CustomerName.Contains(input.Keyword) || u.CustomerAddress.Contains(input.Keyword) || u.CustomerLinkman.Contains(input.Keyword) || u.CustomerPhone.Contains(input.Keyword) || u.Remark.Contains(input.Keyword) || u.ApprovalUserName.Contains(input.Keyword) || u.ApprovalRemark.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CustomerCode), u => u.CustomerCode.Contains(input.CustomerCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CustomerName), u => u.CustomerName.Contains(input.CustomerName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CustomerAddress), u => u.CustomerAddress.Contains(input.CustomerAddress.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CustomerLinkman), u => u.CustomerLinkman.Contains(input.CustomerLinkman.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CustomerPhone), u => u.CustomerPhone.Contains(input.CustomerPhone.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Remark), u => u.Remark.Contains(input.Remark.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ApprovalUserName), u => u.ApprovalUserName.Contains(input.ApprovalUserName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ApprovalRemark), u => u.ApprovalRemark.Contains(input.ApprovalRemark.Trim()))
            .WhereIF(input.ApprovalStatus != null, u => u.ApprovalStatus == input.ApprovalStatus)
            // 超管+管理员 免疫数据筛选（其他账号只能看自己的数据）
            .WhereIF(UserType != "999" && UserType != "888" , u => u.CreateUserId.ToString() == UserId)
            .Select<WmsBaseCustomerPresetOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取申请客户信息详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取申请客户信息详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsBaseCustomerPreset> Detail([FromQuery] QueryByIdWmsBaseCustomerPresetInput input)
    {
        return await _wmsBaseCustomerPresetRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加申请客户信息 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加申请客户信息")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsBaseCustomerPresetInput input)
    {
        var entity = input.Adapt<WmsBaseCustomerPreset>();
        return await _wmsBaseCustomerPresetRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新申请客户信息 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新申请客户信息")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsBaseCustomerPresetInput input)
    {
        var entity = await _wmsBaseCustomerPresetRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        if (entity.ApprovalStatus == 1)
        {
            throw Oops.Oh("审核通过的数据不能修改");
        }
        // 重新进入审核队列
        entity.ApprovalStatus = 0;
        var updateEntity = input.Adapt<WmsBaseCustomerPreset>();
        await _wmsBaseCustomerPresetRep.AsUpdateable(updateEntity)
        .IgnoreColumns(u => new {
            u.CustomerTypeId,
            u.token,
            u.accountExec,
            u.Status,
            u.ApprovalUserName,
            u.ApprovalUserId,
        })
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除申请客户信息 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除申请客户信息")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsBaseCustomerPresetInput input)
    {
        var entity = await _wmsBaseCustomerPresetRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        if(App.User?.FindFirst(ClaimConst.UserId)?.Value != entity.CreateUserId.ToString())
        {
            throw new Exception("只允许操作自己的数据");
        }
        if (entity.ApprovalStatus == 1)
        {
            throw Oops.Oh("审核通过的数据不能删除");
        }
        // 如果有已经审核通过的数据，同步删除客户信息表的数据
        if (entity.ApprovalStatus == 1)
        {
            var item = await _wmsBaseCustomerRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
            await _wmsBaseCustomerRep.FakeDeleteAsync(item);
        }
        await _wmsBaseCustomerPresetRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsBaseCustomerPresetRep.DeleteAsync(entity);   //真删除
    }
    /// <summary>
    /// 批量删除申请客户信息 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除申请客户信息")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsBaseCustomerPresetInput> input)

    {
        var exp = Expressionable.Create<WmsBaseCustomerPreset>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsBaseCustomerPresetRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
        // 检查权限和状态
        foreach (var entity in list)
        {
            if(App.User?.FindFirst(ClaimConst.UserId)?.Value != entity.CreateUserId.ToString())
            {
                throw new Exception("只允许操作自己的数据");
            }
            if (entity.ApprovalStatus == 1)
            {
                throw Oops.Oh("审核通过的数据不能删除");
            }
        }
        // 同步删除客户信息表的数据
        foreach (var entity in list)
        {
            if (entity.ApprovalStatus == 1)
            {
                var item = await _wmsBaseCustomerRep.GetFirstAsync(u => u.Id == entity.Id);
                if (item != null)
                {
                    await _wmsBaseCustomerRep.FakeDeleteAsync(item);
                }
            }
        }
        return await _wmsBaseCustomerPresetRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsBaseCustomerPresetRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 导出申请客户信息记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出申请客户信息记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsBaseCustomerPresetInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsBaseCustomerPresetOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        var approvalStatusDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "ApprovalStatus" }).Result.ToDictionary(x => x.Value, x => x.Label);
        list.ForEach(e => {
            e.ApprovalStatusDictLabel = approvalStatusDictMap.GetValueOrDefault(e.ApprovalStatus.ToString() ?? "", e.ApprovalStatus.ToString());
        });
        return ExcelHelper.ExportTemplate(list, "申请客户信息导出记录");
    }
    
    /// <summary>
    /// 下载申请客户信息数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载申请客户信息数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsBaseCustomerPresetOutput>(), "申请客户信息导入模板");
    }
    
    private static readonly object _wmsBaseCustomerPresetImportLock = new object();
    /// <summary>
    /// 导入申请客户信息记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入申请客户信息记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsBaseCustomerPresetImportLock)
        {
            var approvalStatusDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "ApprovalStatus" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var stream = ExcelHelper.ImportData<ImportWmsBaseCustomerPresetInput, WmsBaseCustomerPreset>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    
                    // 映射字典值
                    foreach(var item in pageItems) {
                        System.Text.StringBuilder sbError = new System.Text.StringBuilder();
                        if (!string.IsNullOrWhiteSpace(item.ApprovalStatusDictLabel)) {
                            item.ApprovalStatus = Convert.ToInt32(approvalStatusDictMap.GetValueOrDefault(item.ApprovalStatusDictLabel));
                            if (item.ApprovalStatus == null) sbError.AppendLine("审核状态字典映射失败");
                        }
                        item.Error = sbError.ToString();
                    }
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<WmsBaseCustomerPreset>>();
                    
                    var storageable = _wmsBaseCustomerPresetRep.Context.Storageable(rows)
                        .SplitError(it => it.Item.CustomerCode?.Length > 32, "客户编码长度不能超过32个字符")
                        .SplitError(it => it.Item.CustomerName?.Length > 32, "客户名称长度不能超过32个字符")
                        .SplitError(it => it.Item.Remark?.Length > 32, "备注长度不能超过32个字符")
                        .SplitError(it => it.Item.ApprovalUserName?.Length > 64, "审核人长度不能超过64个字符")
                        .SplitError(it => it.Item.ApprovalRemark?.Length > 255, "审核备注长度不能超过255个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.CustomerCode,
                        it.CustomerName,
                        it.Remark,
                        it.ApprovalStatus,
                        it.ApprovalUserName,
                        it.ApprovalUserId,
                        it.ApprovalRemark,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }

    /// <summary>
    /// 审核申请客户信息 🔖
    /// </summary>
    [DisplayName("审核申请客户信息")]
    [ApiDescriptionSettings(Name = "Approval"), HttpPost]
    public async Task<long> Approval(ApprovalWmsBaseCustomerPresetInput input)
    {
        // 获取客户信息
        var u = await _wmsBaseCustomerPresetRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        // 修改当前客户审批状态
        u.ApprovalRemark = u.ApprovalStatus != 1 ? input.ApprovalRemark : "";
        u.ApprovalStatus = input.ApprovalStatus;
        u.ApprovalUserId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
        u.ApprovalUserName = App.User?.FindFirst(ClaimConst.RealName)?.Value;
        await _wmsBaseCustomerPresetRep.AsUpdateable(u)
        .IgnoreColumns(u => new {
            u.CustomerTypeId,
            u.token,
            u.accountExec,
            u.Status,
        })
        .ExecuteCommandAsync();
        // 除了通过一概不新增
        if (u.ApprovalStatus != 1)
        {
            return 0;
        }
        // 审批通过新增客户
        return await _wmsBaseCustomerRep.InsertAsync(new WmsBaseCustomer()
        {
            Id = u.Id,
            CustomerCode = u.CustomerCode,
            CustomerName = u.CustomerName,
            CustomerAddress = u.CustomerAddress,
            CustomerLinkman = u.CustomerLinkman,
            CustomerPhone = u.CustomerPhone,
            Remark = u.Remark,
            CreateUserName = u.CreateUserName,
            CreateTime = u.CreateTime,
            UpdateUserName = u.UpdateUserName,
            UpdateTime = u.UpdateTime,
            IsDelete = u.IsDelete,
            CreateUserId = u.CreateUserId,
            UpdateUserId = u.UpdateUserId,
        }) ? u.Id : 0;
    }

    /// <summary>
    /// 批量审核申请客户信息 🔖
    /// </summary>
    [DisplayName("批量审核申请客户信息")]
    [ApiDescriptionSettings(Name = "BatchApproval"), HttpPost]
    public async Task<int> BatchApproval(BatchApprovalWmsBaseCustomerPresetInput input)
    {
        var successCount = 0;
        var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
        var userName = App.User?.FindFirst(ClaimConst.RealName)?.Value;

        // 获取所有待审核的客户信息
        var customers = await _wmsBaseCustomerPresetRep.GetListAsync(u => input.Ids.Contains(u.Id));
        if (customers == null || customers.Count == 0)
            throw Oops.Oh(ErrorCodeEnum.D1002);

        // 批量更新审核状态
        foreach (var customer in customers)
        {
            customer.ApprovalRemark = input.ApprovalStatus != 1 ? input.ApprovalRemark : "";
            customer.ApprovalStatus = input.ApprovalStatus;
            customer.ApprovalUserId = userId;
            customer.ApprovalUserName = userName;
        }

        await _wmsBaseCustomerPresetRep.AsUpdateable(customers)
            .IgnoreColumns(u => new {
                u.CustomerTypeId,
                u.token,
                u.accountExec,
                u.Status,
            })
            .ExecuteCommandAsync();

        // 只有审核通过才批量新增客户
        if (input.ApprovalStatus == 1)
        {
            var approvedCustomers = customers.Where(u => u.ApprovalStatus == 1).ToList();
            var baseCustomers = approvedCustomers.Select(u => new WmsBaseCustomer()
            {
                Id = u.Id,
                CustomerCode = u.CustomerCode,
                CustomerName = u.CustomerName,
                CustomerAddress = u.CustomerAddress,
                CustomerLinkman = u.CustomerLinkman,
                CustomerPhone = u.CustomerPhone,
                Remark = u.Remark,
                CreateUserName = u.CreateUserName,
                CreateTime = u.CreateTime,
                UpdateUserName = u.UpdateUserName,
                UpdateTime = u.UpdateTime,
                IsDelete = u.IsDelete,
                CreateUserId = u.CreateUserId,
                UpdateUserId = u.UpdateUserId,
            }).ToList();

            await _wmsBaseCustomerRep.InsertRangeAsync(baseCustomers);
            successCount = approvedCustomers.Count;
        }
        else
        {
            successCount = customers.Count;
        }

        return successCount;
    }
}
