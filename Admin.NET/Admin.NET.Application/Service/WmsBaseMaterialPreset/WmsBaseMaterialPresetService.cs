// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Application.Entity;
using Admin.NET.Core.Service;
using AngleSharp.Dom;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Mapster;
using Microsoft.AspNetCore.Http;
using SqlSugar;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static SKIT.FlurlHttpClient.Wechat.Api.Models.ChannelsECWarehouseGetResponse.Types;
namespace Admin.NET.Application;

/// <summary>
/// 申请物料信息服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsBaseMaterialPresetService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsBaseMaterialPreset> _wmsBaseMaterialPresetRep;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly SqlSugarRepository<WmsBaseMaterial> _wmsBaseMaterialRep;
    private readonly SysDictTypeService _sysDictTypeService;

    public WmsBaseMaterialPresetService(SqlSugarRepository<WmsBaseMaterialPreset> wmsBaseMaterialPresetRep, ISqlSugarClient sqlSugarClient, SysDictTypeService sysDictTypeService, SqlSugarRepository<WmsBaseMaterial> wmsBaseMaterialRep)
    {
        _wmsBaseMaterialPresetRep = wmsBaseMaterialPresetRep;
        _sqlSugarClient = sqlSugarClient;
        _sysDictTypeService = sysDictTypeService;
        _wmsBaseMaterialRep = wmsBaseMaterialRep;
    }

    /// <summary>
    /// 分页查询申请物料信息 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询申请物料信息")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsBaseMaterialPresetOutput>> Page(PageWmsBaseMaterialPresetInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var UserId = App.User?.FindFirst(ClaimConst.UserId)?.Value;
        var UserType = App.User?.FindFirst(ClaimConst.AccountType).Value;
        var query = _wmsBaseMaterialPresetRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.MaterialCode.Contains(input.Keyword) || u.MaterialName.Contains(input.Keyword) || u.MaterialStandard.Contains(input.Keyword) || u.BoxQuantity.Contains(input.Keyword) || u.MaterialOrigin.Contains(input.Keyword) || u.Remark.Contains(input.Keyword) || u.CreateUserName.Contains(input.Keyword) || u.UpdateUserName.Contains(input.Keyword) || u.EveryNumber.Contains(input.Keyword) || u.Vehicle.Contains(input.Keyword) || u.MaterialStockHigh.Contains(input.Keyword) || u.MaterialStockLow.Contains(input.Keyword) || u.MaterialAlarmDay.Contains(input.Keyword) || u.Labeling.Contains(input.Keyword) || u.ManageType.Contains(input.Keyword) || u.MixReserve.Contains(input.Keyword) || u.MaxReserve.Contains(input.Keyword) || u.ApprovalUserName.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), u => u.MaterialCode.Contains(input.MaterialCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialName), u => u.MaterialName.Contains(input.MaterialName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialStandard), u => u.MaterialStandard.Contains(input.MaterialStandard.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.BoxQuantity), u => u.BoxQuantity.Contains(input.BoxQuantity.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialOrigin), u => u.MaterialOrigin.Contains(input.MaterialOrigin.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Remark), u => u.Remark.Contains(input.Remark.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CreateUserName), u => u.CreateUserName.Contains(input.CreateUserName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.UpdateUserName), u => u.UpdateUserName.Contains(input.UpdateUserName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.EveryNumber), u => u.EveryNumber.Contains(input.EveryNumber.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Vehicle), u => u.Vehicle.Contains(input.Vehicle.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialStockHigh), u => u.MaterialStockHigh.Contains(input.MaterialStockHigh.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialStockLow), u => u.MaterialStockLow.Contains(input.MaterialStockLow.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.WarehouseId), u => u.WarehouseId.Equals(input.WarehouseId.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialAlarmDay), u => u.MaterialAlarmDay.Contains(input.MaterialAlarmDay.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Labeling), u => u.Labeling.Contains(input.Labeling.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ManageType), u => u.ManageType.Contains(input.ManageType.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MixReserve), u => u.MixReserve.Contains(input.MixReserve.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaxReserve), u => u.MaxReserve.Contains(input.MaxReserve.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ApprovalUserName), u => u.ApprovalUserName.Contains(input.ApprovalUserName.Trim()))
            .WhereIF(input.MaterialType != null, u => u.MaterialType == input.MaterialType)
            .WhereIF(input.MaterialUnit != null, u => u.MaterialUnit == input.MaterialUnit)
            .WhereIF(input.MaterialAreaId != null, u => u.MaterialAreaId == input.MaterialAreaId)
            .WhereIF(input.AlarmDay != null, u => u.AlarmDay == input.AlarmDay)
            .WhereIF(input.ApprovalStatus != null, u => u.ApprovalStatus == input.ApprovalStatus)
            // 超管+管理员 免疫数据筛选（其他账号只能看自己的数据）
            .WhereIF(UserType != "999" && UserType != "888" , u => u.CreateUserId.ToString() == UserId)
            .LeftJoin<WmsBaseUnit>((u, materialUnit) => u.MaterialUnit == materialUnit.Id)
            .LeftJoin<WmsBaseWareHouse>((u, materialUnit, warehouse) => u.WarehouseId == warehouse.Id)
            .Select((u, materialUnit, warehouse) => new WmsBaseMaterialPresetOutput
            {
                Id = u.Id,
                MaterialCode = u.MaterialCode,
                MaterialType = u.MaterialType,
                MaterialName = u.MaterialName,
                MaterialMcode = u.MaterialMcode,
                MaterialStandard = u.MaterialStandard,
                MaterialUnit = u.MaterialUnit,
                MaterialUnitFkDisplayName = $"{materialUnit.UnitName}",
                MaterialAreaId = u.MaterialAreaId,
                BoxQuantity = u.BoxQuantity,
                MaterialOrigin = u.MaterialOrigin,
                Remark = u.Remark,
                CreateUserName = u.CreateUserName,
                CreateTime = u.CreateTime,
                UpdateUserName = u.UpdateUserName,
                UpdateTime = u.UpdateTime,
                MaterialModel = u.MaterialModel,
                EveryNumber = u.EveryNumber,
                Vehicle = u.Vehicle,
                MaterialWeight = u.MaterialWeight,
                MaterialValidityDay1 = u.MaterialValidityDay1,
                MaterialValidityDay2 = u.MaterialValidityDay2,
                MaterialValidityDay3 = u.MaterialValidityDay3,
                MaterialTemp = u.MaterialTemp,
                IsDelete = u.IsDelete,
                CreateUserId = u.CreateUserId,
                UpdateUserId = u.UpdateUserId,
                MaterialStockHigh = u.MaterialStockHigh,
                MaterialStockLow = u.MaterialStockLow,
                WarehouseId = u.WarehouseId,
                WarehouseFkDisplayName = $"{warehouse.WarehouseName}",
                MaterialAlarmDay = u.MaterialAlarmDay,
                Labeling = u.Labeling,
                ManageType = u.ManageType,
                OuterInnerCode = u.OuterInnerCode,
                MixReserve = u.MixReserve,
                MaxReserve = u.MaxReserve,
                AlarmDay = u.AlarmDay,
                token = u.token,
                accountExec = u.accountExec,
                Status = u.Status,
                ApprovalStatus = u.ApprovalStatus,
                ApprovalUserName = u.ApprovalUserName,
                ApprovalUserId = u.ApprovalUserId,
                ApprovalRemark = u.ApprovalRemark,
            });
		var pagedList = await query.OrderBuilderToJoin(input).ToPagedListAsync(input.Page, input.PageSize);
        
        // 处理多选区域名称回显
        if (pagedList.Items.Any())
        {
            var allAreaIds = pagedList.Items
                .Where(u => !string.IsNullOrEmpty(u.MaterialAreaId))
                .SelectMany(u => u.MaterialAreaId.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Distinct()
                .Select(id => long.TryParse(id, out var val) ? val : 0)
                .Where(id => id > 0)
                .ToList();

            if (allAreaIds.Any())
            {
                var areas = await _wmsBaseMaterialPresetRep.Context.Queryable<WmsBaseArea>()
                    .Where(u => allAreaIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.AreaCode, u.AreaName })
                    .ToListAsync();

                var areaDict = areas.ToDictionary(u => u.Id, u => $"{u.AreaCode}-{u.AreaName}");

                foreach (var item in pagedList.Items)
                {
                    if (!string.IsNullOrEmpty(item.MaterialAreaId))
                    {
                        var ids = item.MaterialAreaId.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        var names = new List<string>();
                        foreach (var idStr in ids)
                        {
                            if (long.TryParse(idStr, out var id) && areaDict.ContainsKey(id))
                            {
                                names.Add(areaDict[id]);
                            }
                        }
                        item.MaterialAreaFkDisplayName = string.Join(",", names);
                    }
                }
            }
        }

        return pagedList;
    }

    /// <summary>
    /// 获取申请物料信息详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取申请物料信息详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsBaseMaterialPreset> Detail([FromQuery] QueryByIdWmsBaseMaterialPresetInput input)
    {
        return await _wmsBaseMaterialPresetRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加申请物料信息 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加申请物料信息")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsBaseMaterialPresetInput input)
    {
        var entity = input.Adapt<WmsBaseMaterialPreset>();
        entity.ApprovalStatus = 0;
        return await _wmsBaseMaterialPresetRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新申请物料信息 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新申请物料信息")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsBaseMaterialPresetInput input)
    {
        var entity = input.Adapt<WmsBaseMaterialPreset>();
        // 重新进入审核队列
        entity.ApprovalStatus = 0;
        await _wmsBaseMaterialPresetRep.AsUpdateable(entity)
        .IgnoreColumns(u => new {
            u.MaterialMcode,
            u.MaterialModel,
            u.MaterialValidityDay1,
            u.MaterialValidityDay2,
            u.MaterialValidityDay3,
            u.MaterialTemp,
            u.OuterInnerCode,
            u.token,
            u.accountExec,
            u.Status,
            u.ApprovalUserName,
            u.ApprovalUserId,
        })
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除申请物料信息 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除申请物料信息")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsBaseMaterialPresetInput input)
    {
        var entity = await _wmsBaseMaterialPresetRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        if(App.User?.FindFirst(ClaimConst.UserId)?.Value != entity.CreateUserId.ToString())
        {
            throw new Exception("只允许操作自己的数据");
        }
        if (entity.ApprovalStatus == 1)
        {
            throw Oops.Oh("审核通过的数据不能删除");
        }
        // 如果有已经审核通过的数据，同步删除物料信息表的数据
        if (entity.ApprovalStatus == 1)
        {
            var item = await _wmsBaseMaterialRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);

            await _wmsBaseMaterialRep.FakeDeleteAsync(item);
        }
        await _wmsBaseMaterialPresetRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsBaseMaterialPresetRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 审核物料信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("审核物料信息")]
    [ApiDescriptionSettings(Name = "Approval"), HttpPost]
    public async Task<long> Approval(ApprovalWmsBaseMaterialPresetInput input)
    {
        // 获取物料信息
        var u = await _wmsBaseMaterialPresetRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        // 修改当前物料审批状态
        u.ApprovalRemark = u.ApprovalStatus != 1 ? input.ApprovalRemark : "";
        u.ApprovalStatus = input.ApprovalStatus;
        u.ApprovalUserId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
        u.ApprovalUserName = App.User?.FindFirst(ClaimConst.RealName)?.Value;
        await _wmsBaseMaterialPresetRep.AsUpdateable(u)
        .IgnoreColumns(u => new {
            u.MaterialMcode,
            u.MaterialModel,
            u.MaterialValidityDay1,
            u.MaterialValidityDay2,
            u.MaterialValidityDay3,
            u.MaterialTemp,
            u.OuterInnerCode,
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
        // 审批通过新增物料
        return await _wmsBaseMaterialRep.InsertAsync(new WmsBaseMaterial()
        {
            Id = u.Id,
            MaterialCode = u.MaterialCode,
            MaterialType = u.MaterialType,
            MaterialName = u.MaterialName,
            MaterialMcode = u.MaterialMcode,
            MaterialStandard = u.MaterialStandard,
            MaterialUnit = u.MaterialUnit,
            MaterialAreaId = u.MaterialAreaId,
            BoxQuantity = u.BoxQuantity,
            MaterialOrigin = u.MaterialOrigin,
            Remark = u.Remark,
            CreateUserName = u.CreateUserName,
            CreateTime = u.CreateTime,
            UpdateUserName = u.UpdateUserName,
            UpdateTime = u.UpdateTime,
            MaterialModel = u.MaterialModel,
            EveryNumber = u.EveryNumber,
            Vehicle = u.Vehicle,
            MaterialWeight = u.MaterialWeight,
            MaterialValidityDay1 = u.MaterialValidityDay1,
            MaterialValidityDay2 = u.MaterialValidityDay2,
            MaterialValidityDay3 = u.MaterialValidityDay3,
            MaterialTemp = u.MaterialTemp,
            IsDelete = u.IsDelete,
            CreateUserId = u.CreateUserId,
            UpdateUserId = u.UpdateUserId,
            MaterialStockHigh = u.MaterialStockHigh,
            MaterialStockLow = u.MaterialStockLow,
            WarehouseId = u.WarehouseId,
            MaterialAlarmDay = u.MaterialAlarmDay,
            Labeling = u.Labeling,
            ManageType = u.ManageType,
            OuterInnerCode = u.OuterInnerCode,
            MixReserve = u.MixReserve,
            MaxReserve = u.MaxReserve,
            AlarmDay = u.AlarmDay,
            token = u.token,
            accountExec = u.accountExec,
            Status = u.Status,
        }) ? u.Id : 0;
    }

    /// <summary>
    /// 批量审核申请物料信息 🔖
    /// </summary>
    [DisplayName("批量审核申请物料信息")]
    [ApiDescriptionSettings(Name = "BatchApproval"), HttpPost]
    public async Task<int> BatchApproval(BatchApprovalWmsBaseMaterialPresetInput input)
    {
        var successCount = 0;
        var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
        var userName = App.User?.FindFirst(ClaimConst.RealName)?.Value;

        // 获取所有待审核的物料信息
        var materials = await _wmsBaseMaterialPresetRep.GetListAsync(u => input.Ids.Contains(u.Id));
        if (materials == null || materials.Count == 0)
            throw Oops.Oh(ErrorCodeEnum.D1002);

        // 批量更新审核状态
        foreach (var material in materials)
        {
            material.ApprovalRemark = input.ApprovalStatus != 1 ? input.ApprovalRemark : "";
            material.ApprovalStatus = input.ApprovalStatus;
            material.ApprovalUserId = userId;
            material.ApprovalUserName = userName;
        }

        await _wmsBaseMaterialPresetRep.AsUpdateable(materials)
            .IgnoreColumns(u => new {
                u.MaterialMcode,
                u.MaterialModel,
                u.MaterialValidityDay1,
                u.MaterialValidityDay2,
                u.MaterialValidityDay3,
                u.MaterialTemp,
                u.OuterInnerCode,
                u.token,
                u.accountExec,
                u.Status,
            })
            .ExecuteCommandAsync();

        // 只有审核通过才批量新增物料
        if (input.ApprovalStatus == 1)
        {
            var approvedMaterials = materials.Where(u => u.ApprovalStatus == 1).ToList();
            var baseMaterials = approvedMaterials.Select(u => new WmsBaseMaterial()
            {
                Id = u.Id,
                MaterialCode = u.MaterialCode,
                MaterialType = u.MaterialType,
                MaterialName = u.MaterialName,
                MaterialMcode = u.MaterialMcode,
                MaterialStandard = u.MaterialStandard,
                MaterialUnit = u.MaterialUnit,
                MaterialAreaId = u.MaterialAreaId,
                BoxQuantity = u.BoxQuantity,
                MaterialOrigin = u.MaterialOrigin,
                Remark = u.Remark,
                CreateUserName = u.CreateUserName,
                CreateTime = u.CreateTime,
                UpdateUserName = u.UpdateUserName,
                UpdateTime = u.UpdateTime,
                MaterialModel = u.MaterialModel,
                EveryNumber = u.EveryNumber,
                Vehicle = u.Vehicle,
                MaterialWeight = u.MaterialWeight,
                MaterialValidityDay1 = u.MaterialValidityDay1,
                MaterialValidityDay2 = u.MaterialValidityDay2,
                MaterialValidityDay3 = u.MaterialValidityDay3,
                MaterialTemp = u.MaterialTemp,
                IsDelete = u.IsDelete,
                CreateUserId = u.CreateUserId,
                UpdateUserId = u.UpdateUserId,
                MaterialStockHigh = u.MaterialStockHigh,
                MaterialStockLow = u.MaterialStockLow,
                WarehouseId = u.WarehouseId,
                MaterialAlarmDay = u.MaterialAlarmDay,
                Labeling = u.Labeling,
                ManageType = u.ManageType,
                OuterInnerCode = u.OuterInnerCode,
                MixReserve = u.MixReserve,
                MaxReserve = u.MaxReserve,
                AlarmDay = u.AlarmDay,
                token = u.token,
                accountExec = u.accountExec,
                Status = u.Status,
            }).ToList();

            await _wmsBaseMaterialRep.InsertRangeAsync(baseMaterials);
            successCount = approvedMaterials.Count;
        }
        else
        {
            successCount = materials.Count;
        }

        return successCount;
    }

    /// <summary>
    /// 批量删除申请物料信息 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除申请物料信息")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsBaseMaterialPresetInput> input)
    {
        var exp = Expressionable.Create<WmsBaseMaterialPreset>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsBaseMaterialPresetRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();

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

        // 同步删除物料信息表的数据
        foreach (var entity in list)
        {
            if (entity.ApprovalStatus == 1)
            {
                var item = await _wmsBaseMaterialRep.GetFirstAsync(u => u.Id == entity.Id);
                if (item != null)
                {
                    await _wmsBaseMaterialRep.FakeDeleteAsync(item);
                }
            }
        }

        return await _wmsBaseMaterialPresetRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsBaseMaterialPresetRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 获取下拉列表数据 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取下拉列表数据")]
    [ApiDescriptionSettings(Name = "DropdownData"), HttpPost]
    public async Task<Dictionary<string, dynamic>> DropdownData(DropdownDataWmsBaseMaterialPresetInput input)
    {
        var materialUnitData = await _wmsBaseMaterialPresetRep.Context.Queryable<WmsBaseUnit>()
            .InnerJoinIF<WmsBaseMaterialPreset>(input.FromPage, (u, r) => u.Id == r.MaterialUnit)
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.UnitName}"
            }).ToListAsync();
        var materialAreaIdData = await _wmsBaseMaterialPresetRep.Context.Queryable<WmsBaseArea>()
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.AreaCode}-{u.AreaName}"
            }).ToListAsync();
        var warehouseIdData = await _wmsBaseMaterialPresetRep.Context.Queryable<WmsBaseWareHouse>()
            .InnerJoinIF<WmsBaseMaterialPreset>(input.FromPage, (u, r) => u.Id == r.WarehouseId)
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.WarehouseName}"
            }).ToListAsync();
        return new Dictionary<string, dynamic>
        {
            { "materialUnit", materialUnitData },
            { "materialAreaId", materialAreaIdData },
            { "warehouseId", warehouseIdData },
        };
    }
    
    /// <summary>
    /// 导出申请物料信息记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出申请物料信息记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsBaseMaterialPresetInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsBaseMaterialPresetOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        var materialTypeDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "MaterialType" }).Result.ToDictionary(x => x.Value, x => x.Label);
        var vehicleDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "Vehicle" }).Result.ToDictionary(x => x.Value, x => x.Label);
        var labelingDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "Labeling" }).Result.ToDictionary(x => x.Value, x => x.Label);
        var manageTypeDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "ManageType" }).Result.ToDictionary(x => x.Value, x => x.Label);
        var approvalStatusDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "ApprovalStatus" }).Result.ToDictionary(x => x.Value, x => x.Label);
        list.ForEach(e => {
            e.MaterialTypeDictLabel = materialTypeDictMap.GetValueOrDefault(e.MaterialType.ToString() ?? "", e.MaterialType.ToString());
            e.VehicleDictLabel = vehicleDictMap.GetValueOrDefault(e.Vehicle ?? "", e.Vehicle);
            e.LabelingDictLabel = labelingDictMap.GetValueOrDefault(e.Labeling ?? "", e.Labeling);
            e.ManageTypeDictLabel = manageTypeDictMap.GetValueOrDefault(e.ManageType ?? "", e.ManageType);
            e.ApprovalStatusDictLabel = approvalStatusDictMap.GetValueOrDefault(e.ApprovalStatus.ToString() ?? "", e.ApprovalStatus.ToString());
        });
        return ExcelHelper.ExportTemplate(list, "申请物料信息导出记录");
    }
    
    /// <summary>
    /// 下载申请物料信息数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载申请物料信息数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsBaseMaterialPresetOutput>(), "申请物料信息导入模板", (_, info) =>
        {
            if (nameof(ExportWmsBaseMaterialPresetOutput.MaterialUnitFkDisplayName) == info.Name) return _wmsBaseMaterialPresetRep.Context.Queryable<WmsBaseUnit>().Select(u => $"{u.UnitName}").Distinct().ToList();
            if (nameof(ExportWmsBaseMaterialPresetOutput.MaterialAreaFkDisplayName) == info.Name) return _wmsBaseMaterialPresetRep.Context.Queryable<WmsBaseArea>().Select(u => $"{u.AreaCode}-{u.AreaName}").Distinct().ToList();
            if (nameof(ExportWmsBaseMaterialPresetOutput.WarehouseFkDisplayName) == info.Name) return _wmsBaseMaterialPresetRep.Context.Queryable<WmsBaseWareHouse>().Select(u => $"{u.WarehouseName}").Distinct().ToList();
            return null;
        });
    }
    
    private static readonly object _wmsBaseMaterialPresetImportLock = new object();
    /// <summary>
    /// 导入申请物料信息记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入申请物料信息记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsBaseMaterialPresetImportLock)
        {
            var materialTypeDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "MaterialType" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var vehicleDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "Vehicle" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var labelingDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "Labeling" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var manageTypeDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "ManageType" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var stream = ExcelHelper.ImportData<ImportWmsBaseMaterialPresetInput, WmsBaseMaterialPreset>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    // 链接 计量单位
                    var materialUnitLabelList = pageItems.Where(x => x.MaterialUnitFkDisplayName != null).Select(x => x.MaterialUnitFkDisplayName).Distinct().ToList();
                    if (materialUnitLabelList.Any()) {
                        var materialUnitLinkMap = _wmsBaseMaterialPresetRep.Context.Queryable<WmsBaseUnit>().Where(u => materialUnitLabelList.Contains($"{u.UnitName}")).ToList().ToDictionary(u => $"{u.UnitName}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.MaterialUnit = materialUnitLinkMap.GetValueOrDefault(e.MaterialUnitFkDisplayName ?? "");
                            if (e.MaterialUnit == null) e.Error = "计量单位链接失败";
                        });
                    }
                    // 链接 专属区域
                    var materialAreaIdLabelList = pageItems.Where(x => x.MaterialAreaFkDisplayName != null).Select(x => x.MaterialAreaFkDisplayName).Distinct().ToList();
                    if (materialAreaIdLabelList.Any()) {
                        var materialAreaIdLinkMap = _wmsBaseMaterialPresetRep.Context.Queryable<WmsBaseArea>().Where(u => materialAreaIdLabelList.Contains($"{u.AreaCode}-{u.AreaName}")).ToList().ToDictionary(u => $"{u.AreaCode}-{u.AreaName}", u => u.Id.ToString());
                        pageItems.ForEach(e => {
                            e.MaterialAreaId = materialAreaIdLinkMap.GetValueOrDefault(e.MaterialAreaFkDisplayName ?? "");
                            if (string.IsNullOrEmpty(e.MaterialAreaId)) e.Error = "专属区域链接失败";
                        });
                    }
                    // 链接 所属仓库
                    var warehouseIdLabelList = pageItems.Where(x => x.WarehouseFkDisplayName != null).Select(x => x.WarehouseFkDisplayName).Distinct().ToList();
                    if (warehouseIdLabelList.Any())
                    {
                        var warehouseIdLinkMap = _wmsBaseMaterialRep.Context.Queryable<WmsBaseWareHouse>().Where(u => warehouseIdLabelList.Contains($"{u.WarehouseName}")).ToList().ToDictionary(u => $"{u.WarehouseName}", u => u.Id);
                        pageItems.ForEach(e => {
                            e.WarehouseId = warehouseIdLinkMap.GetValueOrDefault(e.WarehouseFkDisplayName ?? "").ToString();
                            if (e.WarehouseId == null) e.Error = "所属仓库链接失败";
                        });
                    }

                    // 映射字典值
                    foreach (var item in pageItems) {
                        System.Text.StringBuilder sbError = new System.Text.StringBuilder();
                        if (!string.IsNullOrWhiteSpace(item.MaterialTypeDictLabel)) {
                            item.MaterialType = Convert.ToInt64(materialTypeDictMap.GetValueOrDefault(item.MaterialTypeDictLabel));
                            if (item.MaterialType == null) sbError.AppendLine("物料类型字典映射失败");
                        }
                        if (!string.IsNullOrWhiteSpace(item.VehicleDictLabel)) {
                            item.Vehicle = vehicleDictMap.GetValueOrDefault(item.VehicleDictLabel);
                            if (item.Vehicle == null) sbError.AppendLine("载具字典映射失败");
                        }
                        if (!string.IsNullOrWhiteSpace(item.LabelingDictLabel)) {
                            item.Labeling = labelingDictMap.GetValueOrDefault(item.LabelingDictLabel);
                            if (item.Labeling == null) sbError.AppendLine("贴标字典映射失败");
                        }
                        if (!string.IsNullOrWhiteSpace(item.ManageTypeDictLabel)) {
                            item.ManageType = manageTypeDictMap.GetValueOrDefault(item.ManageTypeDictLabel);
                            if (item.ManageType == null) sbError.AppendLine("管理方式字典映射失败");
                        }
                        item.Error = sbError.ToString();
                    }
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        if (x.MaterialType == null){
                            x.Error = "物料类型不能为空";
                            return false;
                        }
                        return true;
                    }).Adapt<List<WmsBaseMaterialPreset>>();
                    
                    var storageable = _wmsBaseMaterialPresetRep.Context.Storageable(rows)
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.MaterialCode), "物料编码不能为空")
                        .SplitError(it => it.Item.MaterialCode?.Length > 100, "物料编码长度不能超过100个字符")
                        .SplitError(it => it.Item.MaterialType == null, "物料类型不能为空")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.MaterialName), "物料名称不能为空")
                        .SplitError(it => it.Item.MaterialName?.Length > 255, "物料名称长度不能超过255个字符")
                        .SplitError(it => it.Item.MaterialStandard?.Length > 200, "物料规格长度不能超过200个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.BoxQuantity), "箱数量不能为空")
                        .SplitError(it => it.Item.BoxQuantity?.Length > 32, "箱数量长度不能超过32个字符")
                        .SplitError(it => it.Item.MaterialOrigin?.Length > 10, "物料来源长度不能超过10个字符")
                        .SplitError(it => it.Item.Remark?.Length > 255, "备注长度不能超过255个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.EveryNumber), "每件托数不能为空")
                        .SplitError(it => it.Item.EveryNumber?.Length > 32, "每件托数长度不能超过32个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Vehicle), "载具不能为空")
                        .SplitError(it => it.Item.Vehicle?.Length > 32, "载具长度不能超过32个字符")
                        .SplitError(it => it.Item.MaterialStockHigh?.Length > 32, "最高库存数量长度不能超过32个字符")
                        .SplitError(it => it.Item.MaterialStockLow?.Length > 32, "最低库存数量长度不能超过32个字符")
                        .SplitError(it => it.Item.MaterialAlarmDay?.Length > 32, "提前预警天数长度不能超过32个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Labeling), "贴标不能为空")
                        .SplitError(it => it.Item.Labeling?.Length > 32, "贴标长度不能超过32个字符")
                        .SplitError(it => it.Item.ManageType?.Length > 32, "管理方式长度不能超过32个字符")
                        .SplitError(it => it.Item.MixReserve?.Length > 32, "最低储备长度不能超过32个字符")
                        .SplitError(it => it.Item.MaxReserve?.Length > 32, "最高储备长度不能超过32个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.MaterialCode,
                        it.MaterialType,
                        it.MaterialName,
                        it.MaterialStandard,
                        it.MaterialUnit,
                        it.MaterialAreaId,
                        it.BoxQuantity,
                        it.MaterialOrigin,
                        it.Remark,
                        it.EveryNumber,
                        it.Vehicle,
                        it.MaterialWeight,
                        it.MaterialStockHigh,
                        it.MaterialStockLow,
                        it.WarehouseId,
                        it.MaterialAlarmDay,
                        it.Labeling,
                        it.ManageType,
                        it.MixReserve,
                        it.MaxReserve,
                        it.AlarmDay,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
