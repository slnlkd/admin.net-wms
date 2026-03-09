// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Services.Description;

using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsSqlView;
using Admin.NET.Core;
using Admin.NET.Core.Service;

using AngleSharp.Dom;

using Furion.DatabaseAccessor;
using Furion.FriendlyException;

using Mapster;

using Microsoft.AspNetCore.Http;

using SqlSugar;
namespace Admin.NET.Application;

/// <summary>
/// 入库单据-表体服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsImportNotifyDetailService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsImportNotifyDetail> _wmsImportNotifyDetailRep;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly WmsSqlViewService _sqlViewService;

    public WmsImportNotifyDetailService(
        SqlSugarRepository<WmsImportNotifyDetail> wmsImportNotifyDetailRep,
        ISqlSugarClient sqlSugarClient,
        WmsSqlViewService sqlViewService)
    {
        _wmsImportNotifyDetailRep = wmsImportNotifyDetailRep;
        _sqlSugarClient = sqlSugarClient;
        _sqlViewService = sqlViewService;
    }

    /// <summary>
    /// 分页查询入库单据-表体 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询入库单据-表体")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsImportNotifyDetailOutput>> Page(PageWmsImportNotifyDetailInput input)
    {
        var query = _wmsImportNotifyDetailRep.AsQueryable()
            .Where(u => u.ImportId == input.ImportId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.LotNo), u => u.LotNo.Contains(input.LotNo))
            .LeftJoin<WmsBaseMaterial>((u, material) => u.MaterialId == material.Id)
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), (u, material) => material.MaterialCode.Contains(input.MaterialCode))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialName), (u, material) => material.MaterialName.Contains(input.MaterialName))
            .LeftJoin<WmsBaseUnit>((u, material, unit) => material.MaterialUnit == unit.Id)
            .Select((u, material, unit) => new WmsImportNotifyDetailOutput
            {
                Id = u.Id,
                ImportId = u.ImportId,
                OuterDetailId = u.OuterDetailId,
                ImportListNo = u.ImportListNo,
                MaterialId = u.MaterialId,
                MaterialCode = material.MaterialCode,
                MaterialName = material.MaterialName,
                MaterialStandard = material.MaterialStandard,
                BoxQuantity = material.BoxQuantity,
                Labeling = material.Labeling == "1" ? "是" : "否",
                UnitName = unit.UnitName,
                LotNo = u.LotNo,
                MaterialStatus = int.Parse(u.MaterialStatus),
                // 修改这里:使用条件表达式替代 ?.
                ImportProductionDate = u.ImportProductionDate == null ? null : u.ImportProductionDate.Value.ToString("yyyy-MM-dd"),
                ImportLostDate = u.ImportLostDate == null ? null : u.ImportLostDate.Value.ToString("yyyy-MM-dd"),
                ImportQuantity = u.ImportQuantity,
                ImportCompleteQuantity = u.ImportCompleteQuantity,
                ImportFactQuantity = u.ImportFactQuantity,
                UploadQuantity = u.UploadQuantity,
                ImportExecuteFlag = u.ImportExecuteFlag,
                Remark = u.Remark,
                PrintFlag = u.PrintFlag,
                TaskStatus = u.TaskStatus,
                LabelJudgment = u.LabelJudgment,
                outQty = u.outQty,
                LanewayCode = u.LanewayCode,
                Xj_HouseCode = u.Xj_HouseCode,
                Xj_Qty = u.Xj_Qty,
                Xj_Weight = u.Xj_Weight,
                Xj_BoxQty = u.Xj_BoxQty,
                Xj_GoodCode = u.Xj_GoodCode,
                Xj_Type = u.Xj_Type,
                VerifiedQty = u.VerifiedQty,
                KilogramQty = u.KilogramQty,
                ImportTaskStatus = u.ImportTaskStatus

            });
        return await query.OrderBuilderToJoin(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取入库单据-表体详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取入库单据-表体详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsImportNotifyDetail> Detail([FromQuery] QueryByIdWmsImportNotifyDetailInput input)
    {
        return await _wmsImportNotifyDetailRep.GetFirstAsync(u => u.Id == input.Id);
    }
    /// <summary>
    /// 获取入库单据-表体详情(关联物料表) ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取入库单据-表体详情(关联物料表)")]
    [ApiDescriptionSettings(Name = "DetailByMaterial"), HttpGet]
    public async Task<WmsImportNotifyDetailOutput> DetailJoinMaterial([FromQuery] QueryByIdWmsImportNotifyDetailInput input)
    {
        var query = _wmsImportNotifyDetailRep.AsQueryable()
            .Where(u => u.Id == input.Id)
            .LeftJoin<WmsBaseMaterial>((u, material) => u.MaterialId == material.Id)
            .LeftJoin<WmsBaseUnit>((u, material, unit) => material.MaterialUnit == unit.Id)
            .LeftJoin<WmsImportNotify>((u, material, unit, import) => u.ImportId == import.Id)
            .LeftJoin<WmsBaseManufacturer>((u, material, unit, import, manufacturer) => import.ManufacturerId == manufacturer.Id)
            .Select((u, material, unit, import, manufacturer) => new WmsImportNotifyDetailOutput
            {
                Id = u.Id,
                ImportId = u.ImportId,
                OuterDetailId = u.OuterDetailId,
                ImportListNo = u.ImportListNo,
                MaterialId = u.MaterialId,
                MaterialCode = material.MaterialCode,
                MaterialName = material.MaterialName,
                MaterialStandard = material.MaterialStandard,
                Vehicle = material.Vehicle,
                BoxQuantity = material.BoxQuantity,
                Labeling = material.Labeling == "1" ? "是" : "否",
                UnitName = unit.UnitName,
                LotNo = u.LotNo,
                MaterialStatus = int.Parse(u.MaterialStatus),
                ImportProductionDate = u.ImportProductionDate == null ? null : u.ImportProductionDate.Value.ToString("yyyy-MM-dd"),
                ImportLostDate = u.ImportLostDate == null ? null : u.ImportLostDate.Value.ToString("yyyy-MM-dd"),
                ImportQuantity = u.ImportQuantity,
                ImportCompleteQuantity = u.ImportCompleteQuantity,
                ImportFactQuantity = u.ImportFactQuantity,
                UploadQuantity = u.UploadQuantity,
                ImportExecuteFlag = u.ImportExecuteFlag,
                Remark = u.Remark,
                PrintFlag = u.PrintFlag,
                TaskStatus = u.TaskStatus,
                LabelJudgment = u.LabelJudgment,
                outQty = u.outQty,
                LanewayCode = u.LanewayCode,
                Xj_HouseCode = u.Xj_HouseCode,
                Xj_Qty = u.Xj_Qty,
                Xj_Weight = u.Xj_Weight,
                Xj_BoxQty = u.Xj_BoxQty,
                Xj_GoodCode = u.Xj_GoodCode,
                Xj_Type = u.Xj_Type,
                VerifiedQty = u.VerifiedQty,
                KilogramQty = u.KilogramQty,
                ImportBillCode = import.ImportBillCode,
                ManufacturerName = manufacturer.CustomerName,
                WarehouseId = import.WarehouseId,
                ImportTaskStatus = u.ImportTaskStatus
            });
        return await query.FirstAsync();
    }
    /// <summary>
    /// 增加入库单据-表体 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加入库单据-表体")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsImportNotifyDetailInput input)
    {
        var entity = input.Adapt<WmsImportNotifyDetail>();
        return await _wmsImportNotifyDetailRep.InsertAsync(entity) ? entity.Id : 0;
    }
    /// <summary>
    /// 批量增加入库单据-表体 ➕➕➕
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [DisplayName("批量增加入库单据-表体")]
    [ApiDescriptionSettings(Name = "AddBatch"), HttpPost]
    public async Task<bool> AddBatch(List<AddWmsImportNotifyDetailInput> list)
    {
        var details = list.Adapt<List<WmsImportNotifyDetail>>();
        var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
        var userName = App.User?.FindFirst(ClaimConst.RealName)?.Value;
        details = details.Where(a => !a.IsDelete).ToList();
        foreach (var entity in details)
        {
            entity.UpdateUserId = userId;
            entity.UpdateTime = DateTime.Now;
            entity.UpdateUserName = userName;
        }
        return await _wmsImportNotifyDetailRep.InsertRangeAsync(details);
    }

    /// <summary>
    /// 更新入库单据-表体 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新入库单据-表体")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsImportNotifyDetailInput input)
    {
        var entity = input.Adapt<WmsImportNotifyDetail>();
        await _wmsImportNotifyDetailRep.AsUpdateable(entity).ExecuteCommandAsync();
    }
    /// <summary>
    /// 批量更新入库单据-表体 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量更新入库单据-表体")]
    [ApiDescriptionSettings(Name = "UpdateBatch"), HttpPost]
    public async Task UpdateBatch(UpdateWmsImportNotifyInput input)
    {
        if (input.DetailList == null || input.DetailList.Count == 0)
            return;

        // 分离出需要更新和新增的数据
        var updateList = new List<WmsImportNotifyDetail>();
        var insertList = new List<WmsImportNotifyDetail>();

        // 获取所有已存在的ID
        var existingIds = input.DetailList.Where(x => x.Id > 0).Select(x => x.Id).ToList();
        var existingEntities = existingIds.Count > 0
            ? await _wmsImportNotifyDetailRep.GetListAsync(x => existingIds.Contains(x.Id) && x.ImportId == input.Id)
            : new List<WmsImportNotifyDetail>();

        var existingIdsSet = new HashSet<long>(existingEntities.Select(x => x.Id));

        foreach (var detail in input.DetailList)
        {
            var entity = detail.Adapt<WmsImportNotifyDetail>();

            if (detail.Id > 0 && existingIdsSet.Contains(detail.Id ?? 0))
            {
                // 存在且ID有效，执行更新
                updateList.Add(entity);
            }
            else
            {
                entity.ImportId = input.Id;
                entity.ImportTaskStatus = 0;
                // 不存在或ID无效，执行新增
                insertList.Add(entity);
            }
        }

        // 执行批量操作
        if (updateList.Count > 0)
            await _wmsImportNotifyDetailRep.UpdateRangeAsync(updateList);

        if (insertList.Count > 0)
            await _wmsImportNotifyDetailRep.InsertRangeAsync(insertList);
    }

    /// <summary>
    /// 删除入库单据-表体 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除入库单据-表体")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsImportNotifyDetailInput input)
    {
        var entity = await _wmsImportNotifyDetailRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsImportNotifyDetailRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsImportNotifyDetailRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除入库单据-表体 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除入库单据-表体")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")] List<DeleteWmsImportNotifyDetailInput> input)
    {
        var exp = Expressionable.Create<WmsImportNotifyDetail>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsImportNotifyDetailRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();

        return await _wmsImportNotifyDetailRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsImportNotifyDetailRep.DeleteAsync(list);   //真删除
    }
    /// <summary>
    /// 批量删除入库单据-表体 ❌
    /// </summary>
    /// <param name="ImportId"></param>
    /// <returns></returns>
    [DisplayName("批量删除入库单据-表体")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDeleteById(long ImportId)
    {
        var list = await _wmsImportNotifyDetailRep.AsQueryable().Where(a => a.ImportId == ImportId).ToListAsync();
        return await _wmsImportNotifyDetailRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsImportNotifyDetailRep.DeleteAsync(list);   //真删除
    }
    /// <summary>
    /// 批量更新入库单据-表体 ✏️
    /// </summary>
    /// <param name="ImportId"></param>
    /// <returns></returns>
    [DisplayName("批量更新入库单据-表体")]
    [ApiDescriptionSettings(Name = "BatchUpdateById"), HttpPost]
    public async Task<bool> BatchUpdateById(long ImportId)
    {
        var list = await _wmsImportNotifyDetailRep.AsQueryable().Where(a => a.ImportId == ImportId).ToListAsync();
        foreach (var ent in list)
        {
            ent.UpdateUserId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
            ent.UpdateUserName = App.User?.FindFirst(ClaimConst.RealName)?.Value;
            ent.UpdateTime = DateTime.Now;
            ent.ImportExecuteFlag = "04";
        }
        return await _wmsImportNotifyDetailRep.UpdateRangeAsync(list);
    }
    /// <summary>
    /// 获取入库单明细列表（兼容JC35接口）🔖
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>返回格式：{ code = 200, count = count, msg = "入库单信息列表", data = list }</returns>
    [DisplayName("获取入库单明细列表")]
    [ApiDescriptionSettings(Name = "GetImportBillDetailsList"), HttpPost, NonUnify]
    public async Task<object> GetImportBillDetailsList(GetImportBillDetailsListInput input)
    {
        if (input == null || !input.ImportId.HasValue)
        {
            return new { code = 0, count = 0, msg = "入库单据ID不能为空", data = new List<object>() };
        }

        try
        {
            // 使用 MergeTable() 将多表查询结果集变成单表，解决别名不一致问题
            var query = _sqlViewService.QueryImportNotifyDetailView()
                .MergeTable()
                .Where(x => !x.IsDel && x.ImportId == input.ImportId && x.ImportExecuteFlag != "-1");

            // 可选条件：批次号
            if (!string.IsNullOrWhiteSpace(input.LotNo))
            {
                query = query.Where(x => x.LotNo == input.LotNo);
            }

            // 可选条件：明细ID
            if (input.Id.HasValue)
            {
                query = query.Where(x => x.Id == input.Id.Value);
            }

            var list = await query.ToListAsync();
            var count = list.Count;

            return new { code = 200, count = count, msg = "入库单信息列表", data = list };
        }
        catch (Exception ex)
        {
            return new { code = 0, count = 0, msg = $"查询入库单明细信息失败：{ex.Message}", data = new List<object>() };
        }
    }

    /// <summary>
    /// 获取下拉列表数据 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取下拉列表数据")]
    [ApiDescriptionSettings(Name = "DropdownData"), HttpPost]
    public async Task<Dictionary<string, dynamic>> DropdownData(DropdownDataWmsImportNotifyDetailInput input)
    {
        var materialIdData = await _wmsImportNotifyDetailRep.Context.Queryable<WmsBaseMaterial>()
            .InnerJoinIF<WmsImportNotifyDetail>(input.FromPage, (u, r) => u.Id == r.MaterialId)
            .Select(u => new
            {
                Value = u.Id,
                Label = $"{u.MaterialCode}-{u.MaterialName}-{u.MaterialStandard}-{u.BoxQuantity}-{u.Labeling}"
            }).ToListAsync();
        return new Dictionary<string, dynamic>
        {
            { "materialId", materialIdData },
        };
    }

    /// <summary>
    /// 导出入库单据-表体记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出入库单据-表体记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsImportNotifyDetailInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsImportNotifyDetailOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "入库单据-表体导出记录");
    }

    /// <summary>
    /// 下载入库单据-表体数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载入库单据-表体数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsImportNotifyDetailOutput>(), "入库单据-表体导入模板", (_, info) =>
        {
            if (nameof(ExportWmsImportNotifyDetailOutput.MaterialCode) == info.Name) return _wmsImportNotifyDetailRep.Context.Queryable<WmsBaseMaterial>().Select(u => $"{u.MaterialCode}-{u.MaterialName}-{u.MaterialStandard}-{u.BoxQuantity}-{u.Labeling}").Distinct().ToList();
            return null;
        });
    }

    private static readonly object _wmsImportNotifyDetailImportLock = new object();
    /// <summary>
    /// 导入入库单据-表体记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入入库单据-表体记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsImportNotifyDetailImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportWmsImportNotifyDetailInput, WmsImportNotifyDetail>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    // 链接 物料信息
                    var materialIdLabelList = pageItems.Where(x => x.MaterialCode != null).Select(x => x.MaterialCode).Distinct().ToList();
                    if (materialIdLabelList.Any())
                    {
                        var materialIdLinkMap = _wmsImportNotifyDetailRep.Context.Queryable<WmsBaseMaterial>().Where(u => materialIdLabelList.Contains($"{u.MaterialCode}-{u.MaterialName}-{u.MaterialStandard}-{u.BoxQuantity}-{u.Labeling}")).ToList().ToDictionary(u => $"{u.MaterialCode}-{u.MaterialName}-{u.MaterialStandard}-{u.BoxQuantity}-{u.Labeling}", u => u.Id as long?);
                        pageItems.ForEach(e =>
                        {
                            e.MaterialId = materialIdLinkMap.GetValueOrDefault(e.MaterialCode ?? "");
                            if (e.MaterialId == null) e.Error = "物料信息链接失败";
                        });
                    }

                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x =>
                    {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<WmsImportNotifyDetail>>();

                    var stageable = _wmsImportNotifyDetailRep.Context.Storageable(rows)
                        .SplitError(it => it.Item.OuterDetailId?.Length > 32, "外部单据明细ID长度不能超过32个字符")
                        .SplitError(it => it.Item.ImportListNo?.Length > 32, "序号长度不能超过32个字符")
                        .SplitError(it => it.Item.LotNo?.Length > 32, "批次长度不能超过32个字符")
                        .SplitError(it => it.Item.MaterialStatus?.Length > 32, "物料状态长度不能超过32个字符")
                        .SplitError(it => it.Item.ImportExecuteFlag?.Length > 2, "执行状态长度不能超过2个字符")
                        .SplitError(it => it.Item.Remark?.Length > 50, "备注长度不能超过50个字符")
                        .SplitError(it => it.Item.LanewayCode?.Length > 32, "巷道编码长度不能超过32个字符")
                        .SplitError(it => it.Item.Xj_HouseCode?.Length > 50, "采浆公司长度不能超过50个字符")
                        .SplitInsert(_ => true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();

                    stageable.AsInsertable.ExecuteCommand();// 不存在插入
                    stageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.ImportId,
                        it.OuterDetailId,
                        it.ImportListNo,
                        it.MaterialId,
                        it.LotNo,
                        it.MaterialStatus,
                        it.ImportProductionDate,
                        it.ImportLostDate,
                        it.ImportQuantity,
                        it.ImportCompleteQuantity,
                        it.ImportFactQuantity,
                        it.UploadQuantity,
                        it.ImportExecuteFlag,
                        it.Remark,
                        it.PrintFlag,
                        it.TaskStatus,
                        it.LabelJudgment,
                        it.outQty,
                        it.LanewayCode,
                        it.Xj_HouseCode,
                        it.Xj_Qty,
                        it.Xj_Weight,
                        it.Xj_BoxQty,
                        it.Xj_GoodCode,
                        it.Xj_Type,
                        it.VerifiedQty,
                        it.KilogramQty,
                    }).ExecuteCommand();// 存在更新

                    // 标记错误信息
                    markerErrorAction.Invoke(stageable, pageItems, rows);
                });
            });

            return stream;
        }
    }
}
