// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Web.Services.Description;

using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.BaseService;
using Admin.NET.Application.Service.WmsBaseOperLog;
using Admin.NET.Application.Service.WmsBaseOperLog.Enum;
using Admin.NET.Core.Service;
using Yitter.IdGenerator;

using AngleSharp.Dom;

using Furion.DatabaseAccessor;
using Furion.FriendlyException;

using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

using Mapster;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using SqlSugar;
namespace Admin.NET.Application;

/// <summary>
/// 入库单服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsImportNotifyService : IDynamicApiController, ITransient
{
    private readonly ImportNotifyRepos _repos;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly SysDictTypeService _sysDictTypeService;
    private readonly WmsImportNotifyDetailService _wmsImportNotifyDetailService;
    private readonly WmsImportNotifyHelper _helper;
    private readonly ILogger<WmsImportNotifyService> _logger;

    public WmsImportNotifyService(
        ImportNotifyRepos repos,
        ISqlSugarClient sqlSugarClient,
        SysDictTypeService sysDictTypeService,
        WmsImportNotifyDetailService wmsImportNotifyDetailService,
        WmsImportNotifyHelper helper,
        ILogger<WmsImportNotifyService> logger)
    {
        _repos = repos;
        _sqlSugarClient = sqlSugarClient;
        _sysDictTypeService = sysDictTypeService;
        _wmsImportNotifyDetailService = wmsImportNotifyDetailService;
        _helper = helper;
        _logger = logger;
    }

    /// <summary>
    /// 分页查询入库单 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询入库单")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsImportNotifyOutput>> Page(PageWmsImportNotifyInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _repos.ImportNotify.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.ImportBillCode.Contains(input.Keyword) || u.ImportExecuteFlag.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ImportBillCode), u => u.ImportBillCode.Contains(input.ImportBillCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ImportExecuteFlag), u => u.ImportExecuteFlag.Contains(input.ImportExecuteFlag.Trim()))
            .WhereIF(input.BillType != null, u => u.BillType == input.BillType)
            .WhereIF(input.CreateTimeRange?.Length == 2, u => u.CreateTime >= input.CreateTimeRange[0] && u.CreateTime <= input.CreateTimeRange[1])
            .LeftJoin<WmsBaseWareHouse>((u, warehouse) => u.WarehouseId == warehouse.Id)
            .LeftJoin<WmsBaseSupplier>((u, warehouse, supplier) => u.SupplierId == supplier.Id)
            .LeftJoin<WmsBaseCustomer>((u, warehouse, supplier, customer) => u.CustomerId == customer.Id)
            .LeftJoin<WmsBaseManufacturer>((u, warehouse, supplier, customer, manufacturer) => u.ManufacturerId == manufacturer.Id)
            .LeftJoin<WmsBaseBillType>((u, warehouse, supplier, customer, manufacturer, billy) => u.BillType == billy.Id)
            .Select((u, warehouse, supplier, customer, manufacturer, billy) => new WmsImportNotifyOutput
            {
                Id = u.Id,
                WarehouseId = u.WarehouseId,
                WarehouseFkDisplayName = $"{warehouse.WarehouseName}",
                ImportBillCode = u.ImportBillCode,
                ImportExecuteFlag = u.ImportExecuteFlag,
                BillType = u.BillType,
                BillTypeDisplayName = billy.BillTypeName,
                DepartmentId = u.DepartmentId,
                SupplierId = u.SupplierId,
                SupplierName = $"{supplier.CustomerName}",
                CustomerId = u.CustomerId,
                CustomerName = $"{customer.CustomerName}",
                ManufacturerId = u.ManufacturerId,
                ManufacturerName = $"{manufacturer.CustomerName}",
                Source = u.Source,
                OuterBillCode = u.OuterBillCode,
                OuterMainId = u.OuterMainId,
                IsDelete = u.IsDelete,
                CreateUserId = u.CreateUserId,
                UpdateUserId = u.UpdateUserId,
                ProduceId = u.ProduceId,
                CreateUserName = u.CreateUserName,
                CreateTime = u.CreateTime.ToString("yyyy-MM-dd HH:mm"),
                UpdateUserName = u.UpdateUserName,
                UpdateTime = u.UpdateTime.ToString("yyyy-MM-dd") == "1900-01-01" ? "" : u.UpdateTime.ToString("yyyy-MM-dd HH:mm"),
                Remark = u.Remark,
            });
        return await query.OrderBuilder(input, "u.").ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取入库单详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取入库单详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsImportNotifyOutput> Detail([FromQuery] QueryByIdWmsImportNotifyInput input)
    {
        var res = await _repos.ImportNotify.AsQueryable().Where(u => u.Id == input.Id)
            .LeftJoin<WmsBaseSupplier>((u, supplier) => u.SupplierId == supplier.Id)
            .LeftJoin<WmsBaseCustomer>((u, supplier, customer) => u.CustomerId == customer.Id)
            .LeftJoin<WmsBaseManufacturer>((u, supplier, customer, manufacturer) => u.ManufacturerId == manufacturer.Id)
            .Select((u, supplier, customer, manufacturer) => new WmsImportNotifyOutput
            {
                Id = u.Id,
                WarehouseId = u.WarehouseId,
                ImportBillCode = u.ImportBillCode,
                ImportExecuteFlag = u.ImportExecuteFlag,
                BillType = u.BillType,
                DepartmentId = u.DepartmentId,
                SupplierId = u.SupplierId,
                SupplierName = $"{supplier.CustomerName}",
                CustomerId = u.CustomerId,
                CustomerName = $"{customer.CustomerName}",
                ManufacturerId = u.ManufacturerId,
                ManufacturerName = $"{manufacturer.CustomerName}",
                Source = u.Source,
                OuterBillCode = u.OuterBillCode,
                OuterMainId = u.OuterMainId,
                IsDelete = u.IsDelete,
                CreateUserId = u.CreateUserId,
                UpdateUserId = u.UpdateUserId,
                ProduceId = u.ProduceId,
                CreateUserName = u.CreateUserName,
                CreateTime = u.CreateTime.ToString("yyyy-MM-dd HH:mm"),
                UpdateUserName = u.UpdateUserName,
                UpdateTime = u.UpdateTime.ToString("yyyy-MM-dd") == "1900-01-01" ? "" : u.UpdateTime.ToString("yyyy-MM-dd HH:mm"),
                Remark = u.Remark,
            }).FirstAsync();
        return res;
    }

    /// <summary>
    /// 增加入库单 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加入库单")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsImportNotifyInput input)
    {
        var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
        var orgId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.OrgId)?.Value);
        var userName = App.User?.FindFirst(ClaimConst.RealName)?.Value;
        var entity = input.Adapt<WmsImportNotify>();
        entity.ImportBillCode = new CommonMethod(_sqlSugarClient).GetImExNo("RKS");
        entity.DepartmentId = orgId;
        entity.ImportExecuteFlag = "01";
        entity.Source = "WMS";
        entity.CreateUserId = userId;
        entity.UpdateUserId = userId;
        entity.UpdateTime = DateTime.Now;
        entity.UpdateUserName = userName;
        var res = _repos.ImportNotify.InsertAsync(entity);
        if (await res)
        {
            foreach (var d in input.DetailList)
            {
                d.ImportId = entity.Id;
                d.ImportTaskStatus = 0;
            }
            await _wmsImportNotifyDetailService.AddBatch(input.DetailList);
            await WmsBaseOperLogHelper.RecordAsync(
                Module: "入库单据",
                OperationType: OperationTypeEnum.Add.GetEnumDescription(),
                BusinessNo: entity.ImportBillCode,
                OperationContent: $"添加入库单据：{entity.ImportBillCode} 成功",
                OperParam: input
            );
            return entity.Id;
        }
        return 0;
    }

    /// <summary>
    /// 更新入库单 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新入库单")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task<long> Update(UpdateWmsImportNotifyInput input)
    {
        var entity = input.Adapt<WmsImportNotify>();
        entity.UpdateUserId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
        entity.UpdateTime = DateTime.Now;
        entity.UpdateUserName = App.User?.FindFirst(ClaimConst.RealName)?.Value;
        var res = _repos.ImportNotify.AsUpdateable(entity).ExecuteCommandAsync();
        if (await res > 0)
        {
            await _wmsImportNotifyDetailService.UpdateBatch(input);
            WmsImportNotify before = _sqlSugarClient.Queryable<WmsImportNotify>().Where(u => u.Id == input.Id).Single();
            await WmsBaseOperLogHelper.RecordAsync(
               Module: "入库单据",
               OperationType: OperationTypeEnum.Update.GetEnumDescription(),
               BusinessNo: entity.ImportBillCode,
               OperationContent: $"修改入库单据:\n{WmsBaseOperLogHelper.GenerateChangeDescription(before, entity)}",
               OperParam: input,
               BeforeData: before,
               AfterData: input
            );
            return entity.Id;
        }
        return 0;
    }

    /// <summary>
    /// 删除入库单 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除入库单")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsImportNotifyInput input)
    {
        var entity = await _repos.ImportNotify.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _repos.ImportNotify.FakeDeleteAsync(entity);   //假删除
        await WmsBaseOperLogHelper.RecordAsync(
            Module: "入库单据",
            OperationType: OperationTypeEnum.Delete.GetEnumDescription(),
            BusinessNo: entity.ImportBillCode,
            OperationContent: $"删除入库单据：{entity.ImportBillCode} 成功",
            OperParam: input
        );
    }

    /// <summary>
    /// 批量删除入库单 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除入库单")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")] List<DeleteWmsImportNotifyInput> input)
    {
        var exp = Expressionable.Create<WmsImportNotify>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _repos.ImportNotify.AsQueryable().Where(exp.ToExpression()).ToListAsync();
        var billies = string.Join(',', list.Select(a => a.ImportBillCode));
        await WmsBaseOperLogHelper.RecordAsync(
            Module: "入库单据",
            OperationType: OperationTypeEnum.Deletes.GetEnumDescription(),
            BusinessNo: billies,
            OperationContent: $"批量删除入库单据：{billies} 成功",
            OperParam: input
        );
        return await _repos.ImportNotify.FakeDeleteAsync(list);   //假删除
    }
    /// <summary>
    /// 入库单作废 ❌
    /// 重构自《ImportController》《Invalid》接口
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("入库单作废")]
    [ApiDescriptionSettings(Name = "Invalid"), HttpPost]
    public async Task Invalid(OperationWmsImportNotifyInput input)
    {
        var entity = await _repos.ImportNotify.GetFirstAsync(u => u.Id == input.ImportId) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _repos.ImportNotify.FakeDeleteAsync(entity);   //假删除
        var list = await _repos.ImportNotifyDetail.AsQueryable().Where(a => a.ImportId == input.ImportId).ToListAsync();
        await _repos.ImportNotifyDetail.FakeDeleteAsync(list);   //假删除
        await WmsBaseOperLogHelper.RecordAsync(
           Module: "入库单据",
           OperationType: OperationTypeEnum.Invalid.GetEnumDescription(),
           BusinessNo: entity.ImportBillCode,
           OperationContent: $"入库单据作废：{entity.ImportBillCode} 成功",
           OperParam: input
        );
    }
    /// <summary>
    /// 入库单关单 ✏️
    /// 重构自《ImportController》《Complete》接口
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("入库单关单")]
    [ApiDescriptionSettings(Name = "Complete"), HttpPost]
    public async Task Complete(OperationWmsImportNotifyInput input)
    {
        var entity = await _repos.ImportNotify.GetFirstAsync(u => u.Id == input.ImportId) ?? throw Oops.Oh(ErrorCodeEnum.D1002);

        // 获取仓库配置并验证超入/少入
        var warehouse = entity.WarehouseId.HasValue
            ? await _repos.Warehouse.GetFirstAsync(x => x.Id == entity.WarehouseId.Value && x.IsDelete == false)
            : null;

        var details = await _repos.ImportNotifyDetail.AsQueryable()
            .Where(a => a.ImportId == input.ImportId && a.IsDelete == false)
            .ToListAsync();

        var validationErrors = new List<string>();

        foreach (var detail in details)
        {
            decimal planQty = detail.ImportQuantity ?? 0;
            decimal factQty = detail.ImportFactQuantity ?? 0;
            decimal completeQty = detail.ImportCompleteQuantity ?? 0;

            if (completeQty > planQty)  // 超入
            {
                if (!(warehouse?.IsOverbooking ?? false))
                {
                    var material = detail.MaterialId.HasValue
                        ? await _repos.Material.GetFirstAsync(x => x.Id == detail.MaterialId.Value)
                        : null;
                    validationErrors.Add($"物料【{material?.MaterialName ?? detail.MaterialId?.ToString() ?? "未知"}】超入：计划入库 {planQty}，已组托 {factQty}，实际完成 {completeQty}，超入 {completeQty - planQty}");
                }
            }
            else if (completeQty < planQty)  // 少入
            {
                if (!(warehouse?.IsEnterless ?? false))
                {
                    var material = detail.MaterialId.HasValue
                        ? await _repos.Material.GetFirstAsync(x => x.Id == detail.MaterialId.Value)
                        : null;
                    validationErrors.Add($"物料【{material?.MaterialName ?? detail.MaterialId?.ToString() ?? "未知"}】少入：计划入库 {planQty}，已组托 {factQty}，实际完成 {completeQty}，少入 {planQty - completeQty}");
                }
            }
        }

        if (validationErrors.Count > 0)
        {
            throw Oops.Bah($"入库单关单验证失败，当前仓库不允许以下操作：\n{string.Join("\n", validationErrors)}");
        }

        // 通过验证后执行关单
        entity.UpdateUserId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
        entity.UpdateUserName = App.User?.FindFirst(ClaimConst.RealName)?.Value;
        entity.ImportExecuteFlag = "04";
        entity.UpdateTime = DateTime.Now;
        await _repos.ImportNotify.AsUpdateable(entity).ExecuteCommandAsync();

        foreach (var ent in details)
        {
            ent.UpdateUserId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
            ent.UpdateUserName = App.User?.FindFirst(ClaimConst.RealName)?.Value;
            ent.UpdateTime = DateTime.Now;
            ent.ImportExecuteFlag = "04";
        }
        await _repos.ImportNotifyDetail.UpdateRangeAsync(details);
        await WmsBaseOperLogHelper.RecordAsync(
          Module: "入库单据",
          OperationType: OperationTypeEnum.Invalid.GetEnumDescription(),
          BusinessNo: entity.ImportBillCode,
          OperationContent: $"入库单据关单：{entity.ImportBillCode} 成功",
          OperParam: input
        );
    }
    /// <summary>
    /// 获取下拉列表数据 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取下拉列表数据")]
    [ApiDescriptionSettings(Name = "DropdownData"), HttpPost]
    public async Task<Dictionary<string, dynamic>> DropdownData(DropdownDataWmsImportNotifyInput input)
    {
        var warehouseIdData = await _repos.ImportNotify.Context.Queryable<WmsBaseWareHouse>()
            .InnerJoinIF<WmsImportNotify>(input.FromPage, (u, r) => u.Id == r.WarehouseId)
            .Select(u => new
            {
                Value = u.Id,
                Label = $"{u.WarehouseName}"
            }).ToListAsync();
        var supplierIdData = await _repos.ImportNotify.Context.Queryable<WmsBaseSupplier>()
            .InnerJoinIF<WmsImportNotify>(input.FromPage, (u, r) => u.Id == r.SupplierId)
            .Select(u => new
            {
                Value = u.Id,
                Label = $"{u.CustomerName}"
            }).ToListAsync();
        var customerIdData = await _repos.ImportNotify.Context.Queryable<WmsBaseCustomer>()
            .InnerJoinIF<WmsImportNotify>(input.FromPage, (u, r) => u.Id == r.CustomerId)
            .Select(u => new
            {
                Value = u.Id,
                Label = $"{u.CustomerName}"
            }).ToListAsync();
        return new Dictionary<string, dynamic>
        {
            { "warehouseId", warehouseIdData },
            { "supplierId", supplierIdData },
            { "customerId", customerIdData },
        };
    }

    /// <summary>
    /// 导出入库单记录（主表+明细表横向布局）🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出入库单记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsImportNotifyInput input)
    {
        try
        {
            // 1. 查询主表数据（复用Page方法，已包含关联名称）
            var pageResult = await Page(input);
            var mainList = pageResult.Items?.ToList() ?? new List<WmsImportNotifyOutput>();

            // 2. 支持部分导出
            if (input.SelectKeyList?.Count > 0)
                mainList = mainList.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();

            if (!mainList.Any())
                throw Oops.Oh("无可导出数据");

            // 3. 批量查询所有明细数据（避免N+1问题）
            var mainIds = mainList.Select(x => x.Id).ToList();
            var detailList = await _repos.ImportNotifyDetail.AsQueryable()
                .Where(d => mainIds.Contains(d.ImportId ?? 0) && d.IsDelete == false)
                .LeftJoin<WmsBaseMaterial>((d, m) => d.MaterialId == m.Id)
                .LeftJoin<WmsBaseUnit>((d, m, u) => m.MaterialUnit == u.Id)
                .Select((d, m, u) => new {
                    d.ImportId,
                    MaterialCode = m.MaterialCode ?? "",
                    MaterialName = m.MaterialName ?? "",
                    MaterialStandard = m.MaterialStandard ?? "",
                    d.LotNo,
                    d.MaterialStatus,
                    UnitName = u.UnitName ?? "",
                    d.ImportQuantity,
                    d.ImportCompleteQuantity,
                    d.ImportFactQuantity,
                    d.UploadQuantity,
                    d.ImportProductionDate,
                    d.ImportLostDate,
                    d.ImportExecuteFlag,
                    BoxQuantity = m.BoxQuantity ?? "",
                    Labeling = m.Labeling ?? "",
                    d.Remark
                })
                .ToListAsync();

            // 4. 建立主表ID到明细列表的映射
            var detailGroupMap = detailList.GroupBy(x => x.ImportId)
                .ToDictionary(g => g.Key ?? 0, g => g.ToList());

            // 5. 获取字典数据
            var materialStatusDict = _sysDictTypeService.GetDataList(
                new GetDataDictTypeInput { Code = "MaterialStatus" }).Result
                .ToDictionary(x => x.Value, x => x.Label);

            var executeFlagDict = _sysDictTypeService.GetDataList(
                new GetDataDictTypeInput { Code = "ExecuteFlag" }).Result
                .ToDictionary(x => x.Value, x => x.Label);

            // 6. 创建Excel
            using var package = new OfficeOpenXml.ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("入库单导出记录");

            // 7. 获取属性列表并写入表头（使用新的导出DTO）
            var mainProps = typeof(ExportWmsImportNotifyDto).GetProperties()
                .Where(p => {
                    var attr = p.GetCustomAttribute<ExporterHeaderAttribute>();
                    return attr != null && !attr.IsIgnore && !string.IsNullOrWhiteSpace(attr.DisplayName);
                })
                .ToList();

            var detailProps = typeof(ExportWmsImportNotifyDetailDto).GetProperties()
                .Where(p => {
                    var attr = p.GetCustomAttribute<ExporterHeaderAttribute>();
                    return attr != null && !attr.IsIgnore && !string.IsNullOrWhiteSpace(attr.DisplayName);
                })
                .ToList();

            var mainHeaderColMap = _helper.WriteHeadersByExporterAttribute(sheet, mainProps, 1, 25);
            var detailHeaderColMap = _helper.WriteHeadersByExporterAttribute(sheet, detailProps, 10, 25);

            // 8. 写入数据
            int currentRow = 2; // 从第2行开始写数据

            foreach (var mainItem in mainList)
            {
                var identifyCode = mainItem.ImportBillCode; // 使用入库单号作为标识列

                // 获取明细列表
                if (detailGroupMap.TryGetValue(mainItem.Id, out var detailItems) && detailItems.Any())
                {
                    // 有明细的情况
                    for (int i = 0; i < detailItems.Count; i++)
                    {
                        var detail = detailItems[i];

                        // 第一条明细：写入主表数据
                        if (i == 0)
                        {
                            _helper.WriteMainTableExportRowCustom(sheet, currentRow, mainItem,
                                mainHeaderColMap, identifyCode);
                        }
                        // 后续明细：主表列保持空白

                        // 写入明细数据
                        _helper.WriteDetailTableExportRowCustom(sheet, currentRow, detail,
                            detailHeaderColMap, identifyCode, materialStatusDict, executeFlagDict);

                        currentRow++;
                    }
                }
                else
                {
                    // 无明细的情况：只写入主表数据
                    _helper.WriteMainTableExportRow(sheet, currentRow, mainItem,
                        mainHeaderColMap, identifyCode);
                    currentRow++;
                }
            }

            // 9. 设置样式
            _helper.SetSheetStyle(sheet, 9); // 第9列为分隔列

            // 10. 设置数据范围边框
            if (currentRow > 2)
            {
                var dataRange = sheet.Cells[1, 1, currentRow - 1, 25];
                dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            // 11. 设置日期列格式
            if (detailHeaderColMap.TryGetValue("生产日期", out int productionDateCol))
            {
                for (int r = 2; r < currentRow; r++)
                    sheet.Cells[r, productionDateCol].Style.Numberformat.Format = "yyyy-mm-dd";
            }
            if (detailHeaderColMap.TryGetValue("有效期至", out int lostDateCol))
            {
                for (int r = 2; r < currentRow; r++)
                    sheet.Cells[r, lostDateCol].Style.Numberformat.Format = "yyyy-mm-dd";
            }

            // 12. 返回文件
            var stream = new System.IO.MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;
            return new XlsxFileResult(stream: stream,
                fileDownloadName: $"入库单导出记录-{DateTime.Now:yyyy-MM-dd_HHmmss}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "入库单导出失败: FileName={FileName}, Error={ErrorMessage}",
                $"入库单导出记录-{DateTime.Now:yyyy-MM-dd_HHmmss}", ex.Message);
            throw Oops.Oh($"导出失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 下载入库单数据导入模板 ⬇️ (主表和子表横向布局在一个sheet中，带下拉框和数据验证)
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载入库单数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        using var package = new OfficeOpenXml.ExcelPackage();

        // 获取下拉数据
        var billTypeList = _repos.BillType.AsQueryable().Where(x => x.IsDelete == false && x.BillType == 1).Select(x => x.BillTypeName).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        var warehouseList = _repos.Warehouse.AsQueryable().Where(x => x.IsDelete == false).Select(x => x.WarehouseName).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        var supplierList = _repos.Supplier.AsQueryable().Where(x => x.IsDelete == false).Select(x => x.CustomerName).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        var manufacturerList = _repos.Manufacturer.AsQueryable().Where(x => x.IsDelete == false).Select(x => x.CustomerName).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        var materialStatusList = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "MaterialStatus" }).Result.ToDictionary(x => x.Value, x => x.Label);

        var combinedSheet = package.Workbook.Worksheets.Add("入库单导入模板");
        var mainProps = typeof(ImportWmsImportNotifyInput).GetProperties().Where(p => p.GetCustomAttribute<ImporterHeaderAttribute>() != null && !p.GetCustomAttribute<ImporterHeaderAttribute>().IsIgnore).ToList();
        var detailProps = typeof(ImportWmsImportNotifyDetailInput).GetProperties().Where(p => p.GetCustomAttribute<ImporterHeaderAttribute>() != null && !p.GetCustomAttribute<ImporterHeaderAttribute>().IsIgnore).ToList();

        // 写入表头并建立映射
        var mainHeaderColMap = _helper.WriteHeaders(combinedSheet, mainProps, 1, 15);
        int separatorCol = 9;
        combinedSheet.Column(separatorCol).Width = 2;
        var detailHeaderColMap = _helper.WriteHeaders(combinedSheet, detailProps, 10, 15);

        // 写入示例数据
        int mainDataRow = 2, detailDataRow = 2;
        _helper.WriteMainTableSampleData(combinedSheet, mainProps, mainDataRow, mainHeaderColMap, billTypeList, warehouseList, supplierList, manufacturerList);
        _helper.WriteDetailTableSampleData(combinedSheet, detailProps, detailDataRow, detailHeaderColMap, materialStatusList);

        // 设置样式
        _helper.SetSheetStyle(combinedSheet, separatorCol);

        // 添加下拉数据验证
        var dropdownSheet = package.Workbook.Worksheets.Add("下拉数据");
        dropdownSheet.Hidden = OfficeOpenXml.eWorkSheetHidden.Visible;
        int dropdownCol = 1;
        _helper.AddDropdownValidation(dropdownSheet, combinedSheet, "单据类型", billTypeList, mainHeaderColMap, mainDataRow, ref dropdownCol);
        _helper.AddDropdownValidation(dropdownSheet, combinedSheet, "所属仓库", warehouseList, mainHeaderColMap, mainDataRow, ref dropdownCol);
        _helper.AddDropdownValidation(dropdownSheet, combinedSheet, "供货单位", supplierList, mainHeaderColMap, mainDataRow, ref dropdownCol);
        _helper.AddDropdownValidation(dropdownSheet, combinedSheet, "制造商单位", manufacturerList, mainHeaderColMap, mainDataRow, ref dropdownCol);
        _helper.AddDropdownValidation(dropdownSheet, combinedSheet, "状态", materialStatusList?.Values.ToList(), detailHeaderColMap, detailDataRow, ref dropdownCol);

        // 设置下拉数据sheet样式
        if (dropdownSheet.Dimension != null && dropdownCol > 1)
        {
            var headerStyle = dropdownSheet.Cells[1, 1, 1, dropdownCol - 1].Style;
            headerStyle.Font.Bold = true;
            headerStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            headerStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
        }

        var stream = new System.IO.MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0;
        return new XlsxFileResult(stream: stream, fileDownloadName: $"入库单导入模板-{DateTime.Now:yyyy-MM-dd_HHmmss}");
    }

    private static readonly object _wmsImportNotifyImportLock = new object();
    /// <summary>
    /// 导入入库单记录 💾 (支持主子表同时导入，根据标识列关联)
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入入库单记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsImportNotifyImportLock)
        {
            // 在事务外先完成文件上传和读取，避免事务中使用.Result导致死锁
            var (tempFileId, filePath) = _helper.UploadAndGetExcelPath(file).Result;

            try
            {
                // 在事务外读取Excel数据，避免事务中使用.Result
                // 直接使用文件路径读取，避免CommonUtil.ImportExcelDataAsync内部的数据库操作
                IImporter importer = new ExcelImporter();
                var importRes = importer.Import<ImportWmsImportNotifyInput>(filePath).Result;

                if (importRes == null)
                    throw Oops.Oh("导入数据为空");
                if (importRes.Exception != null)
                    throw Oops.Oh("导入异常:" + importRes.Exception.Message);
                if (importRes.TemplateErrors?.Count > 0)
                    throw Oops.Oh("模板异常:" + string.Join("\n", importRes.TemplateErrors.Select(x => $"[{x.RequireColumnName}]{x.Message}")));

                var mainTableList = importRes.Data?.ToList() ?? throw Oops.Oh("有效数据为空");

                // 过滤主表数据：只保留主表关键字段不为空的记录
                // 判断标准：至少有一个主表关键字段（所属仓库、单据类型、标识列等）不为空
                mainTableList = mainTableList.Where(x =>
                    !string.IsNullOrWhiteSpace(x.IdentifyCode) ||
                    !string.IsNullOrWhiteSpace(x.WarehouseFkDisplayName) ||
                    !string.IsNullOrWhiteSpace(x.BillTypeName) ||
                    !string.IsNullOrWhiteSpace(x.SupplierFkDisplayName) ||
                    !string.IsNullOrWhiteSpace(x.ManufacturerFkDisplayName) ||
                    !string.IsNullOrWhiteSpace(x.OuterBillCode)
                ).ToList();

                if (!mainTableList.Any())
                    throw Oops.Oh("未找到有效的主表数据，请确保至少有一条完整的主表记录");

                // 根据标识列分组，每个唯一标识列对应一个主表记录
                // 如果同一标识列有多条记录，选择主表字段最完整的那条
                var groupedMainTableList = mainTableList
                    .Where(x => !string.IsNullOrWhiteSpace(x.IdentifyCode))
                    .GroupBy(x => x.IdentifyCode)
                    .Select(g =>
                    {
                        // 对于同一标识列的多条记录，选择主表字段最完整的那条
                        var bestRecord = g.OrderByDescending(x =>
                            (!string.IsNullOrWhiteSpace(x.WarehouseFkDisplayName) ? 1 : 0) +
                            (!string.IsNullOrWhiteSpace(x.BillTypeName) ? 1 : 0) +
                            (!string.IsNullOrWhiteSpace(x.SupplierFkDisplayName) ? 1 : 0) +
                            (!string.IsNullOrWhiteSpace(x.ManufacturerFkDisplayName) ? 1 : 0) +
                            (!string.IsNullOrWhiteSpace(x.OuterBillCode) ? 1 : 0) +
                            (!string.IsNullOrWhiteSpace(x.Remark) ? 1 : 0)
                        ).First();

                        // 合并同一标识列的其他记录的字段（如果bestRecord中某个字段为空，但其他记录有值）
                        foreach (var record in g)
                        {
                            // 跳过bestRecord本身
                            if (ReferenceEquals(record, bestRecord))
                                continue;

                            if (string.IsNullOrWhiteSpace(bestRecord.WarehouseFkDisplayName) && !string.IsNullOrWhiteSpace(record.WarehouseFkDisplayName))
                                bestRecord.WarehouseFkDisplayName = record.WarehouseFkDisplayName;
                            if (string.IsNullOrWhiteSpace(bestRecord.BillTypeName) && !string.IsNullOrWhiteSpace(record.BillTypeName))
                                bestRecord.BillTypeName = record.BillTypeName;
                            if (string.IsNullOrWhiteSpace(bestRecord.SupplierFkDisplayName) && !string.IsNullOrWhiteSpace(record.SupplierFkDisplayName))
                                bestRecord.SupplierFkDisplayName = record.SupplierFkDisplayName;
                            if (string.IsNullOrWhiteSpace(bestRecord.ManufacturerFkDisplayName) && !string.IsNullOrWhiteSpace(record.ManufacturerFkDisplayName))
                                bestRecord.ManufacturerFkDisplayName = record.ManufacturerFkDisplayName;
                            if (string.IsNullOrWhiteSpace(bestRecord.OuterBillCode) && !string.IsNullOrWhiteSpace(record.OuterBillCode))
                                bestRecord.OuterBillCode = record.OuterBillCode;
                            if (string.IsNullOrWhiteSpace(bestRecord.Remark) && !string.IsNullOrWhiteSpace(record.Remark))
                                bestRecord.Remark = record.Remark;
                        }

                        return bestRecord;
                    })
                    .ToList();

                // 如果没有标识列的主表数据（可能的情况），保留所有有效主表数据，但需要去重
                if (!groupedMainTableList.Any())
                {
                    // 没有标识列的情况，根据主表关键字段组合去重
                    groupedMainTableList = mainTableList
                        .GroupBy(x => $"{x.WarehouseFkDisplayName}_{x.BillTypeName}_{x.OuterBillCode}")
                        .Select(g => g.OrderByDescending(x =>
                            (!string.IsNullOrWhiteSpace(x.WarehouseFkDisplayName) ? 1 : 0) +
                            (!string.IsNullOrWhiteSpace(x.BillTypeName) ? 1 : 0) +
                            (!string.IsNullOrWhiteSpace(x.SupplierFkDisplayName) ? 1 : 0) +
                            (!string.IsNullOrWhiteSpace(x.ManufacturerFkDisplayName) ? 1 : 0) +
                            (!string.IsNullOrWhiteSpace(x.OuterBillCode) ? 1 : 0)
                        ).First())
                        .ToList();
                }

                mainTableList = groupedMainTableList;
                mainTableList.ForEach(u => u.Id = YitIdHelper.NextId());

                // 确保在开始事务前，所有异步操作都已完成，数据库连接已释放
                // 等待一小段时间，确保所有数据库连接都已释放
                System.Threading.Thread.Sleep(50);

                // 手动开始事务
                IActionResult result = null;
                _sqlSugarClient.Ado.BeginTran();
                try
                {
                    var errorItems = new List<ImportWmsImportNotifyInput>();
                    var allSavedRows = new List<WmsImportNotify>(); // 保存所有已保存的主表记录
                    var allPageItems = new List<ImportWmsImportNotifyInput>(); // 保存所有主表数据项

                    // 批量预加载关联数据
                    var linkMaps = _helper.LoadMainTableLinkMaps(mainTableList);
                    var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
                    var orgId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.OrgId)?.Value);
                    var userName = App.User?.FindFirst(ClaimConst.RealName)?.Value;
                    var commonMethod = new CommonMethod(_sqlSugarClient);

                    // 逐条处理并保存主表数据
                    foreach (var pageItem in mainTableList)
                    {
                        _helper.LinkMainTableFields(pageItem, linkMaps);
                        if (!string.IsNullOrWhiteSpace(pageItem.Error))
                        {
                            errorItems.Add(pageItem);
                            allPageItems.Add(pageItem);
                            continue;
                        }

                        var row = pageItem.Adapt<WmsImportNotify>();
                        var importBillCode = commonMethod.GetImExNo("RKS");
                        row.ImportBillCode = importBillCode;
                        pageItem.ImportBillCode = importBillCode;
                        _helper.SetMainTableDefaults(row, orgId, userId, userName);

                        try
                        {
                            // 记录映射后的值用于调试
                            _logger.LogDebug("准备插入主表: WarehouseId={WarehouseId}, BillType={BillType}, SupplierId={SupplierId}, ManufacturerId={ManufacturerId}",
                                row.WarehouseId, row.BillType, row.SupplierId, row.ManufacturerId);

                            var insertedId = _repos.ImportNotify.InsertReturnIdentity(row);
                            row.Id = insertedId > 0 ? insertedId : _repos.ImportNotify.GetFirst(u => u.ImportBillCode == importBillCode)?.Id ?? 0;
                            if (row.Id == 0) throw new Exception($"主表保存后无法获取ID - 入库单号: {importBillCode}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "主表插入失败 - WarehouseId={WarehouseId}, BillType={BillType}, 错误:{Error}",
                                row.WarehouseId, row.BillType, ex.Message);
                            pageItem.Error = $"保存主表失败: {ex.Message}";
                            errorItems.Add(pageItem);
                            continue;
                        }

                        allSavedRows.Add(row);
                        allPageItems.Add(pageItem);
                    }

                    // 在主表数据全部保存后，统一处理子表数据（只调用一次）
                    _helper.ImportDetailData(filePath, allPageItems, allSavedRows);

                    // 提交事务
                    _sqlSugarClient.Ado.CommitTran();

                    // 仅导出错误记录
                    var errorList = mainTableList.Where(u => !string.IsNullOrWhiteSpace(u.Error)).ToList();
                    if (errorList.Any())
                    {
                        result = ExcelHelper.ExportData(errorList, "导入记录");
                    }
                    else
                    {
                        result = new JsonResult(AdminResultProvider.Ok("导入成功"));
                    }
                }
                catch (Exception ex)
                {
                    // 回滚事务
                    _sqlSugarClient.Ado.RollbackTran();
                    _logger.LogError(ex, "入库单导入失败: FileName={FileName}, MainCount={MainCount}, Error={ErrorMessage}",
                        file.FileName, mainTableList?.Count ?? 0, ex.Message);
                    throw;
                }

                return result;
            }
            finally
            {
                // 清理临时文件
                _helper.CleanupTempExcelFile(tempFileId, filePath).Wait();
            }
        }
    }

}
