// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsBaseOperLog;
using Admin.NET.Application.Service.WmsBaseOperLog.Enum;
using Admin.NET.Core.Service;

using Furion;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;

using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

using Mapster;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NewLife.Caching.Clusters;

using SqlSugar;
namespace Admin.NET.Application;

/// <summary>
/// 入库流水服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsImportOrderService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsImportOrder> _wmsImportOrderRep;
    private readonly SqlSugarRepository<WmsBaseSlot> _wmsBaseSlotRep;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly WmsBaseSlotService _wmsBaseSlotService;
    private readonly WmsImportNotifyHelper _helper;
    public WmsImportOrderService(SqlSugarRepository<WmsImportOrder> wmsImportOrderRep, ISqlSugarClient sqlSugarClient, WmsBaseSlotService wmsBaseSlotService, SqlSugarRepository<WmsBaseSlot> wmsBaseSlotRep, WmsImportNotifyHelper helper)
    {
        _wmsImportOrderRep = wmsImportOrderRep;
        _sqlSugarClient = sqlSugarClient;
        _wmsBaseSlotService = wmsBaseSlotService;
        _wmsBaseSlotRep = wmsBaseSlotRep;
        _helper = helper;
    }

    /// <summary>
    /// 分页查询入库流水 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询入库流水")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsImportOrderOutput>> Page(PageWmsImportOrderInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsImportOrderRep.AsQueryable()
            // 关联表
            .LeftJoin<WmsImportNotifyDetail>((u, detail) => u.ImportDetailId == detail.Id)
            .LeftJoin<WmsImportNotify>((u, detail, notify) => u.ImportId == notify.Id)
            .LeftJoin<WmsBaseArea>((u, detail, notify, area) => u.ImportAreaId == area.Id)
            .LeftJoin<WmsBaseLaneway>((u, detail, notify, area, laneway) => u.ImportLanewayId == laneway.Id)
            .LeftJoin<WmsImportTask>((u, detail, notify, area, laneway, task) => u.ImportTaskId == task.Id)
            .LeftJoin<WmsBaseMaterial>((u, detail, notify, area, laneway, task, material) => detail.MaterialId == material.Id)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.ImportOrderNo.Contains(input.Keyword) || u.ImportSlotCode.Contains(input.Keyword) || u.ImportExecuteFlag.Contains(input.Keyword) || u.LotNo.Contains(input.Keyword) || u.SubVehicleCode.Contains(input.Keyword) || u.StockCode.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ImportOrderNo), u => u.ImportOrderNo.Contains(input.ImportOrderNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ImportSlotCode), u => u.ImportSlotCode.Contains(input.ImportSlotCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ImportExecuteFlag), u => u.ImportExecuteFlag.Contains(input.ImportExecuteFlag.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.LotNo), u => u.LotNo.Contains(input.LotNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.SubVehicleCode), u => u.SubVehicleCode.Contains(input.SubVehicleCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StockCode), u => u.StockCode.Contains(input.StockCode.Trim()))
            .WhereIF(input.ImportProductionDateRange?.Length == 2, u =>
                u.ImportProductionDate >= input.ImportProductionDateRange[0] &&
                u.ImportProductionDate <= input.ImportProductionDateRange[1])
            .WhereIF(input.CompleteDateRange?.Length == 2, u =>
                u.CompleteDate >= input.CompleteDateRange[0] &&
                u.CompleteDate <= input.CompleteDateRange[1])
            .WhereIF(input.CreateTimeRange?.Length == 2, u =>
                u.CreateTime >= input.CreateTimeRange[0] &&
                u.CreateTime <= input.CreateTimeRange[1])
            // 选择字段映射
            .Select((u, detail, notify, area, laneway, task, material) => new WmsImportOrderOutput
            {
                Id = u.Id,
                ImportOrderNo = u.ImportOrderNo,
                ImportId = u.ImportId,
                MaterialId = detail.MaterialId,
                MaterialCode = material.MaterialCode,
                MaterialName = material.MaterialName,
                ImportAreaId = u.ImportAreaId,
                AreaName = area.AreaName,
                ImportLanewayId = u.ImportLanewayId,
                LanewayName = laneway.LanewayName,
                ImportSlotCode = u.ImportSlotCode,
                StockCodeId = u.StockCodeId,
                StockCode = u.StockCode,
                ImportQuantity = u.ImportQuantity,
                ImportWeight = u.ImportWeight,
                ImportTaskId = u.ImportTaskId,
                TaskNo = task.TaskNo,
                ImportExecuteFlag = u.ImportExecuteFlag,
                Remark = u.Remark,
                ImportClass = u.ImportClass,
                LotNo = u.LotNo,
                ImportProductionDate = u.ImportProductionDate,
                CompleteDate = Convert.ToDateTime(u.CompleteDate).ToString("yyyy-MM-dd HH:mm"),
                InspectionStatus = u.InspectionStatus,
                SubVehicleCode = u.SubVehicleCode,
                InspectFlag = u.InspectFlag,
                WareHouseId = u.WareHouseId,
                IsTemporaryStorage = u.IsTemporaryStorage,
                ImportBillCode = notify.ImportBillCode,
                CreateUserName = u.CreateUserName,
                CreateTime = u.CreateTime.ToString("yyyy-MM-dd HH:mm"),
                UpdateUserName = u.UpdateUserName,
                UpdateTime = u.UpdateTime.ToString("yyyy-MM-dd") == "1900-01-01" ? "" : u.UpdateTime.ToString("yyyy-MM-dd HH:mm"),
            });

        return await query.OrderBuilder(input, "u.").ToPagedListAsync(input.Page, input.PageSize);
    }
    /// <summary>
    /// 获取入库流水详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取入库流水详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsImportOrder> Detail([FromQuery] QueryByIdWmsImportOrderInput input)
    {
        return await _wmsImportOrderRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加入库流水 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加入库流水")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsImportOrderInput input)
    {
        var entity = input.Adapt<WmsImportOrder>();
        var res = await _wmsImportOrderRep.InsertAsync(entity);
        await WmsBaseOperLogHelper.RecordAsync(
          Module: "入库流水",
          OperationType: OperationTypeEnum.Add.GetEnumDescription(),
          BusinessNo: entity.ImportOrderNo,
          OperationContent: $"添加入库流水：{entity.ImportOrderNo} 成功",
          OperParam: input
        ); 
        return res ? entity.Id : 0;
    }

    /// <summary>
    /// 更新入库流水 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新入库流水")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsImportOrderInput input)
    {
        var entity = input.Adapt<WmsImportOrder>();
        await _wmsImportOrderRep.AsUpdateable(entity).ExecuteCommandAsync();
        WmsImportOrder before = _sqlSugarClient.Queryable<WmsImportOrder>().Where(u => u.Id == input.Id).Single();
        await WmsBaseOperLogHelper.RecordAsync(
          Module: "入库流水",
          OperationType: OperationTypeEnum.Update.GetEnumDescription(),
          BusinessNo: entity.ImportOrderNo,
          OperationContent: $"更新入库流水:\n{WmsBaseOperLogHelper.GenerateChangeDescription(before, entity)}",
          OperParam: input,
          BeforeData: before,
          AfterData: input
        ); 
    }
    /// <summary>
    /// 指定储位 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("指定储位")]
    [ApiDescriptionSettings(Name = "SaveSlot"), HttpPost]
    public async Task SaveSlot(SaveOrderSlotInput input)
    {
        // 1. 获取入库流水和目标储位
        var order = await _wmsImportOrderRep.GetFirstAsync(u => u.Id == input.OrderId);
        if (order == null)
            throw Oops.Oh("入库流水不存在");

        var slot = await _wmsBaseSlotRep.GetFirstAsync(u => u.Id == input.SlotId);
        if (slot == null)
            throw Oops.Oh("储位不存在");

        // 2. 验证储位状态（必须为空储位）
        if (slot.SlotStatus != 0)
            throw Oops.Oh($"储位 {slot.SlotCode} 状态不可用，仅可选择空储位");

        // 3. 验证储位是否被锁定
        if (slot.SlotImlockFlag != 0)
            throw Oops.Oh($"储位 {slot.SlotCode} 已被入库锁定，无法使用");

        // 4. 验证储位深度
        var depthError = await ValidateSlotDepth(slot, order);
        if (depthError != null)
            throw Oops.Oh(depthError);

        // 5. 释放旧储位状态
        if (!string.IsNullOrWhiteSpace(order.ImportSlotCode))
        {
            var olds = await _wmsBaseSlotRep.GetFirstAsync(m => m.SlotCode == order.ImportSlotCode);
            if (olds != null)
            {
                olds.SlotStatus = 0;
                await _wmsBaseSlotRep.AsUpdateable(olds).ExecuteCommandAsync();
            }
        }

        // 6. 更新新储位状态和入库流水
        order.ImportSlotCode = slot.SlotCode;
        slot.SlotStatus = 2;

        await _wmsImportOrderRep.AsUpdateable(order).ExecuteCommandAsync();
        await _wmsBaseSlotRep.AsUpdateable(slot).ExecuteCommandAsync();

        // 7. 记录操作日志
        await WmsBaseOperLogHelper.RecordAsync(
            Module: "入库流水",
            OperationType: OperationTypeEnum.Update.GetEnumDescription(),
            BusinessNo: order.ImportOrderNo,
            OperationContent: order.ImportOrderNo + ":" + order.StockCodeId + "指定库位" + order.ImportSlotCode + " 成功",
            OperParam: input
        );
    }

    /// <summary>
    /// 获取巷道内已占用的储位列表
    /// </summary>
    /// <param name="lanewayId">巷道ID</param>
    /// <param name="excludeOrderId">要排除的入库流水ID（当前正在修改的订单）</param>
    /// <returns>已占用储位列表</returns>
    private async Task<List<WmsBaseSlot>> GetOccupiedSlotsInLaneway(long lanewayId, long excludeOrderId)
    {
        var occupiedSlots = new List<WmsBaseSlot>();

        // 1. 从库存托盘表查询已入库的储位
        var stockSlots = await _sqlSugarClient.Queryable<WmsStockTray, WmsBaseSlot>(
            (t, s) => new JoinQueryInfos(JoinType.Left, t.StockSlotCode == s.SlotCode))
            .Where((t, s) =>
                s.SlotLanewayId == lanewayId &&
                s.SlotImlockFlag == 0 &&
                !t.IsDelete &&
                s.Make == "01")  // 01=存储库位
            .Select((t, s) => s)
            .ToListAsync();

        occupiedSlots.AddRange(stockSlots);

        // 2. 从入库流水表查询正在入库的储位（排除当前订单）
        var importSlots = await _sqlSugarClient.Queryable<WmsImportOrder, WmsBaseSlot>(
            (io, s) => new JoinQueryInfos(JoinType.Left, io.ImportSlotCode == s.SlotCode))
            .Where((io, s) =>
                s.SlotLanewayId == lanewayId &&
                !io.IsDelete &&
                io.Id != excludeOrderId &&  // 排除当前订单
                io.ImportExecuteFlag != "03" &&  // 不包括已完成的
                s.SlotImlockFlag == 0 &&
                s.Make == "01")
            .Select((io, s) => s)
            .ToListAsync();

        occupiedSlots.AddRange(importSlots);

        // 3. 去重并返回
        return occupiedSlots
            .Where(s => s != null)
            .GroupBy(s => s.Id)
            .Select(g => g.First())
            .ToList();
    }

    /// <summary>
    /// 验证储位深度是否符合规则（连续填充策略）
    /// </summary>
    /// <param name="targetSlot">目标储位</param>
    /// <param name="order">入库流水</param>
    /// <returns>验证结果消息，null表示验证通过</returns>
    private async Task<string?> ValidateSlotDepth(WmsBaseSlot targetSlot, WmsImportOrder order)
    {
        // 1. 基础验证
        if (!targetSlot.SlotLanewayId.HasValue)
            return null; // 无巷道ID，跳过深度验证

        if (!targetSlot.SlotInout.HasValue)
            return "储位深度信息缺失，无法验证深度规则";

        // 2. 查询巷道内已占用的储位及其深度
        var occupiedSlots = await GetOccupiedSlotsInLaneway(
            targetSlot.SlotLanewayId.Value,
            order.Id  // 排除当前入库流水的旧储位
        );

        // 3. 查询巷道的最大深度（排除中转区 Make='02'）
        var lanewayMaxDepth = await _sqlSugarClient.Queryable<WmsBaseSlot>()
            .Where(s => s.SlotLanewayId == targetSlot.SlotLanewayId && s.SlotInout.HasValue && s.Make == "01")
            .MaxAsync(s => s.SlotInout);

        if (!lanewayMaxDepth.HasValue)
            return "巷道深度信息缺失，无法验证深度规则";

        // 4. 提取所有已占用储位的深度值（去重并排序）
        var occupiedDepths = occupiedSlots
            .Where(s => s.SlotInout.HasValue)
            .Select(s => s.SlotInout.Value)
            .Distinct()
            .OrderByDescending(d => d) // 按深度降序排列（从内到外）
            .ToList();

        // 5. 如果巷道为空，必须选择最深处
        if (occupiedDepths.Count == 0)
        {
            if (targetSlot.SlotInout.Value != lanewayMaxDepth.Value)
            {
                return $"巷道为空时，必须选择最深处（深度{lanewayMaxDepth.Value}）的储位";
            }
            return null;
        }

        // 6. 如果巷道已有货物，必须按连续填充规则
        // 计算最小已有深度（防止空洞）
        var minOccupiedDepth = occupiedDepths.Min();

        // 新货位必须是(最小深度-1)
        var expectedDepth = minOccupiedDepth - 1;

        // 验证目标深度是否符合规则
        if (expectedDepth <= 0)
        {
            return $"该巷道已有货位深度为{string.Join("、", occupiedDepths)}，最小深度为{minOccupiedDepth}，无法继续指定储位";
        }

        if (targetSlot.SlotInout.Value != expectedDepth)
        {
            return $"储位深度不符合规则。该巷道已有货位深度为{string.Join("、", occupiedDepths)}，最小深度为{minOccupiedDepth}，新指定储位深度必须为{expectedDepth}，请选择深度为{expectedDepth}的储位";
        }

        return null; // 验证通过
    }

    /// <summary>
    /// 删除入库流水 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除入库流水")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsImportOrderInput input)
    {
        var entity = await _wmsImportOrderRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);

        await _wmsImportOrderRep.FakeDeleteAsync(entity);   //假删除
         
        await WmsBaseOperLogHelper.RecordAsync(
           Module: "入库流水",
           OperationType: OperationTypeEnum.Delete.GetEnumDescription(),
           BusinessNo: entity.ImportOrderNo,
           OperationContent: $"删除入库流水：{entity.ImportOrderNo} 成功",
           OperParam: input
         );
    }

    /// <summary>
    /// 批量删除入库流水 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除入库流水")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")] List<DeleteWmsImportOrderInput> input)
    {
        var exp = Expressionable.Create<WmsImportOrder>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsImportOrderRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
        var billows = string.Join(',', list.Select(a => a.ImportOrderNo));
        await WmsBaseOperLogHelper.RecordAsync(
            Module: "入库流水",
            OperationType: OperationTypeEnum.Delete.GetEnumDescription(),
            BusinessNo: billows,
            OperationContent: $"删除入库流水：{billows} 成功",
            OperParam: input
        ); 
        return await _wmsImportOrderRep.FakeDeleteAsync(list);   //假删除
    }

    /// <summary>
    /// 导出入库流水记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出入库流水记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsImportOrderInput input)
    {
        try
        {
            // 1. 获取选中的流水ID列表
            List<long> selectedIds = input.SelectKeyList ?? new List<long>();
            input.Keyword = input.Keyword?.Trim();

            // 2. 执行大查询，一次性获取主子表及关联信息，解决赋值问题
            var query = _wmsImportOrderRep.AsQueryable()
                .LeftJoin<WmsStockSlotBoxInfo>((o, b) => o.Id == b.ImportOrderId)
                .LeftJoin<WmsBaseMaterial>((o, b, m) => b.MaterialId == m.Id)
                .WhereIF(selectedIds.Any(), o => selectedIds.Contains(o.Id))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), o => o.ImportOrderNo.Contains(input.Keyword) || o.ImportSlotCode.Contains(input.Keyword) || o.ImportExecuteFlag.Contains(input.Keyword) || o.LotNo.Contains(input.Keyword) || o.SubVehicleCode.Contains(input.Keyword) || o.StockCode.Contains(input.Keyword))
                .WhereIF(!string.IsNullOrWhiteSpace(input.ImportOrderNo), o => o.ImportOrderNo.Contains(input.ImportOrderNo.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.ImportSlotCode), o => o.ImportSlotCode.Contains(input.ImportSlotCode.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.ImportExecuteFlag), o => o.ImportExecuteFlag.Contains(input.ImportExecuteFlag.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.LotNo), o => o.LotNo.Contains(input.LotNo.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.SubVehicleCode), o => o.SubVehicleCode.Contains(input.SubVehicleCode.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.StockCode), o => o.StockCode.Contains(input.StockCode.Trim()))
                .WhereIF(input.ImportProductionDateRange?.Length == 2, o => o.ImportProductionDate >= input.ImportProductionDateRange[0] && o.ImportProductionDate <= input.ImportProductionDateRange[1])
                .WhereIF(input.CompleteDateRange?.Length == 2, o => o.CompleteDate >= input.CompleteDateRange[0] && o.CompleteDate <= input.CompleteDateRange[1])
                .WhereIF(input.CreateTimeRange?.Length == 2, o => o.CreateTime >= input.CreateTimeRange[0] && o.CreateTime <= input.CreateTimeRange[1])
                .Select((o, b, m) => new {
                    o.Id, // 流水ID
                    o.ImportOrderNo,
                    o.StockCode,
                    o.ImportSlotCode,
                    o.ImportExecuteFlag,
                    o.CompleteDate,
                    o.CreateUserName,
                    o.CreateTime,
                    // 箱明细信息
                    b.BoxCode,
                    MaterialCode = m.MaterialCode ?? "",
                    MaterialName = m.MaterialName ?? "",
                    MaterialStandard = m.MaterialStandard ?? "",
                    b.LotNo,
                    b.Qty,
                    b.BulkTank,
                    b.Status,
                    b.ProductionDate,
                    b.ValidateDay
                });

            var rawDataList = await query.ToListAsync();

            if (!rawDataList.Any())
                throw Oops.Oh("无可导出数据");

            // 3. 获取字典数据
            var sysDictTypeService = App.GetService<SysDictTypeService>();
            var orderStatusDict = sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "OrderStatus" }).Result.ToDictionary(x => x.Value, x => x.Label);
            var executeFlagDict = sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "ExecuteFlag" }).Result.ToDictionary(x => x.Value, x => x.Label);
            var bulkTankDict = sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "BulkTank" }).Result.ToDictionary(x => x.Value, x => x.Label);

            // 4. 创建Excel
            using var package = new OfficeOpenXml.ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("入库流水导出记录");

            // 5. 获取属性列表并写入表头
            var mainProps = typeof(ExportWmsImportOrderMainDto).GetProperties()
                .Where(p => {
                    var attr = p.GetCustomAttribute<ExporterHeaderAttribute>();
                    return attr != null && !attr.IsIgnore && !string.IsNullOrWhiteSpace(attr.DisplayName);
                })
                .ToList();

            var detailProps = typeof(ExportWmsImportOrderSubDto).GetProperties()
                .Where(p => {
                    var attr = p.GetCustomAttribute<ExporterHeaderAttribute>();
                    return attr != null && !attr.IsIgnore && !string.IsNullOrWhiteSpace(attr.DisplayName);
                })
                .ToList();

            var mainHeaderColMap = _helper.WriteHeadersByExporterAttribute(sheet, mainProps, 1, 25);
            var detailHeaderColMap = _helper.WriteHeadersByExporterAttribute(sheet, detailProps, 10, 25);

            // 6. 写入数据
            int currentRow = 2;
            var groupedByOrder = rawDataList.GroupBy(x => x.Id).ToList();

            foreach (var group in groupedByOrder)
            {
                var details = group.ToList();
                for (int i = 0; i < details.Count; i++)
                {
                    var item = details[i];

                    // 写入主表列（仅第一行）
                    if (i == 0)
                    {
                        _helper.WriteMainTableExportRowForOrder(sheet, currentRow, item, mainHeaderColMap, executeFlagDict);
                    }

                    // 写入子表列
                    _helper.WriteDetailTableExportRowForOrder(sheet, currentRow, item, detailHeaderColMap, orderStatusDict, executeFlagDict, bulkTankDict);
                    
                    currentRow++;
                }
            }

            // 7. 设置样式和边框
            _helper.SetSheetStyle(sheet, 9);
            if (currentRow > 2)
            {
                var dataRange = sheet.Cells[1, 1, currentRow - 1, 20]; // 调整总列数
                dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            // 8. 返回文件
            var stream = new System.IO.MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;
            return new XlsxFileResult(stream: stream,
                fileDownloadName: $"入库流水导出记录-{DateTime.Now:yyyy-MM-dd_HHmmss}");
        }
        catch (Exception ex)
        {
            throw Oops.Oh($"导出失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 下载入库流水数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载入库流水数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsImportOrderOutput>(), "入库流水导入模板");
    }

    private static readonly object _wmsImportOrderImportLock = new object();
    /// <summary>
    /// 导入入库流水记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入入库流水记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsImportOrderImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportWmsImportOrderInput, WmsImportOrder>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {

                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x =>
                    {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<WmsImportOrder>>();

                    var storageable = _wmsImportOrderRep.Context.Storageable(rows)
                        .SplitError(it => it.Item.ImportOrderNo?.Length > 32, "流水号长度不能超过32个字符")
                        .SplitError(it => it.Item.ImportSlotCode?.Length > 20, "储位编码长度不能超过20个字符")
                        .SplitError(it => it.Item.ImportExecuteFlag?.Length > 2, "执行标志（0待执行、1正在执行、2已完成、3已上传）长度不能超过2个字符")
                        .SplitError(it => it.Item.Remark?.Length > 50, "备注长度不能超过50个字符")
                        .SplitError(it => it.Item.ImportClass?.Length > 32, "班次长度不能超过32个字符")
                        .SplitError(it => it.Item.LotNo?.Length > 32, "批号长度不能超过32个字符")
                        .SplitError(it => it.Item.SubVehicleCode?.Length > 32, "主子载具号长度不能超过32个字符")
                        .SplitError(it => it.Item.IsTemporaryStorage?.Length > 255, "是否暂存入库流水（不为空为是）长度不能超过255个字符")
                        .SplitError(it => it.Item.StockCode?.Length > 50, "载具号长度不能超过50个字符")
                        .SplitError(it => it.Item.YsOrTJ?.Length > 10, "验收0 挑浆1长度不能超过10个字符")
                        .SplitInsert(_ => true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();

                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.ImportOrderNo,
                        it.ImportId,
                        it.ImportAreaId,
                        it.ImportLanewayId,
                        it.ImportSlotCode,
                        it.StockCodeId,
                        it.ImportQuantity,
                        it.ImportWeight,
                        it.ImportTaskId,
                        it.ImportExecuteFlag,
                        it.Remark,
                        it.ImportClass,
                        it.LotNo,
                        it.ImportProductionDate,
                        it.CompleteDate,
                        it.InspectionStatus,
                        it.WareHouseId,
                        it.ImportDetailId,
                        it.SubVehicleCode,
                        it.Weight,
                        it.InspectFlag,
                        it.IsTemporaryStorage,
                        it.StockCode,
                        it.YsOrTJ,
                    }).ExecuteCommand();// 存在更新

                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });

            return stream;
        }
    }
}
