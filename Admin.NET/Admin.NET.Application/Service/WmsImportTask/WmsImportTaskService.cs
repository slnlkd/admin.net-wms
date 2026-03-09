// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Furion;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Mapster;
using SqlSugar;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.BaseService;
using Admin.NET.Application.Service.WmsBaseOperLog;
using Admin.NET.Application.Service.WmsBaseOperLog.Enum;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
namespace Admin.NET.Application;

/// <summary>
/// 入库任务表服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsImportTaskService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsImportTask> _wmsImportTaskRep;
    private readonly SqlSugarRepository<WmsBaseSlot> _wmsBaseSlotRep;
    private readonly SqlSugarRepository<WmsImportOrder> _wmsImportOrderRep;
    private readonly SqlSugarRepository<WmsStockSlotBoxInfo> _wmsStockSlotBoxInfoRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    private const int TaskStatusPending = 0;
    private const int TaskStatusProcessing = 1;
    private const int TaskStatusCancelled = 3;
    private const int SlotStatusEmpty = 0;
    private const int BoxStatusPending = 0;

    private const string ExecuteFlagPending = "01";

    public WmsImportTaskService(SqlSugarRepository<WmsImportTask> wmsImportTaskRep, SqlSugarRepository<WmsBaseSlot> wmsBaseSlotRep, SqlSugarRepository<WmsImportOrder> wmsImportOrderRep, SqlSugarRepository<WmsStockSlotBoxInfo> wmsStockSlotBoxInfoRep, ISqlSugarClient sqlSugarClient)
    {
        _wmsImportTaskRep = wmsImportTaskRep;
        _wmsBaseSlotRep = wmsBaseSlotRep;
        _wmsImportOrderRep = wmsImportOrderRep;
        _wmsStockSlotBoxInfoRep = wmsStockSlotBoxInfoRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询入库任务表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询入库任务表")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsImportTaskOutput>> Page(PageWmsImportTaskInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsImportTaskRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.TaskNo.Contains(input.Keyword) || u.Sender.Contains(input.Keyword) || u.Receiver.Contains(input.Keyword) || u.Message.Contains(input.Keyword) || u.StockCode.Contains(input.Keyword) || u.Msg.Contains(input.Keyword) || u.StartLocation.Contains(input.Keyword) || u.EndLocation.Contains(input.Keyword) || u.NewEnd.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.TaskNo), u => u.TaskNo.Contains(input.TaskNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Sender), u => u.Sender.Contains(input.Sender.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Receiver), u => u.Receiver.Contains(input.Receiver.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Message), u => u.Message.Contains(input.Message.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StockCode), u => u.StockCode.Contains(input.StockCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Msg), u => u.Msg.Contains(input.Msg.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StartLocation), u => u.StartLocation.Contains(input.StartLocation.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.EndLocation), u => u.EndLocation.Contains(input.EndLocation.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.NewEnd), u => u.NewEnd.Contains(input.NewEnd.Trim()))
            .WhereIF(input.IsSuccess != null, u => u.IsSuccess == input.IsSuccess)
            .WhereIF(input.SendDateRange?.Length == 2, u => u.SendDate >= input.SendDateRange[0] && u.SendDate <= input.SendDateRange[1])
            .WhereIF(input.BackDateRange?.Length == 2, u => u.BackDate >= input.BackDateRange[0] && u.BackDate <= input.BackDateRange[1])
            .WhereIF(input.Status != null, u => u.Status == input.Status)
            .WhereIF(input.CancelDateRange?.Length == 2, u => u.CancelDate >= input.CancelDateRange[0] && u.CancelDate <= input.CancelDateRange[1])
            .WhereIF(input.FinishDateRange?.Length == 2, u => u.FinishDate >= input.FinishDateRange[0] && u.FinishDate <= input.FinishDateRange[1])
            .WhereIF(input.WareHouseId != null, u => u.WareHouseId == input.WareHouseId)
            .Select<WmsImportTaskOutput>();
        return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取入库任务表详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取入库任务表详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsImportTask> Detail([FromQuery] QueryByIdWmsImportTaskInput input)
    {
        return await _wmsImportTaskRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加入库任务表 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加入库任务表")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsImportTaskInput input)
    {
        var entity = input.Adapt<WmsImportTask>();
        await _wmsImportTaskRep.InsertAsync(entity);
        await WmsBaseOperLogHelper.RecordAsync(
          Module: "入库任务",
          OperationType: OperationTypeEnum.Add.GetEnumDescription(),
          BusinessNo: entity.TaskNo,
          OperationContent: $"添加入库任务：{entity.TaskNo} 成功",
          OperParam: input
        );
        return entity.Id;
    }

    /// <summary>
    /// 更新入库任务表 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新入库任务表")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsImportTaskInput input)
    {
        var entity = input.Adapt<WmsImportTask>();
        await _wmsImportTaskRep.AsUpdateable(entity).ExecuteCommandAsync();
        WmsImportTask before = _sqlSugarClient.Queryable<WmsImportTask>().Where(u => u.Id == input.Id).Single();
        await WmsBaseOperLogHelper.RecordAsync(
          Module: "入库任务",
          OperationType: OperationTypeEnum.Update.GetEnumDescription(),
          BusinessNo: entity.TaskNo,
          OperationContent: $"更新入库任务:\n{WmsBaseOperLogHelper.GenerateChangeDescription(before, entity)}",
          OperParam: input,
          BeforeData: before,
          AfterData: input
        );
    }

    /// <summary>
    /// 删除入库任务表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除入库任务表")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsImportTaskInput input)
    {
        var entity = await _wmsImportTaskRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsImportTaskRep.DeleteAsync(entity);   //真删除
        await WmsBaseOperLogHelper.RecordAsync(
           Module: "入库任务",
           OperationType: OperationTypeEnum.Delete.GetEnumDescription(),
           BusinessNo: entity.TaskNo,
           OperationContent: $"删除入库任务：{entity.TaskNo} 成功",
           OperParam: input
         );
    }

    /// <summary>
    /// 批量删除入库任务表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除入库任务表")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<bool> BatchDelete([Required(ErrorMessage = "主键列表不能为空")] List<DeleteWmsImportTaskInput> input)
    {
        var exp = Expressionable.Create<WmsImportTask>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsImportTaskRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
        var billies = string.Join(',', list.Select(a => a.TaskNo));
        await WmsBaseOperLogHelper.RecordAsync(
            Module: "入库任务",
            OperationType: OperationTypeEnum.Deletes.GetEnumDescription(),
            BusinessNo: billies,
            OperationContent: $"删除入库任务：{billies} 成功",
            OperParam: input
        );
        return await _wmsImportTaskRep.DeleteAsync(list);   //真删除
    }

    /// <summary>
    /// 导出入库任务表记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出入库任务表记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsImportTaskInput input)
    {
        try
        {
            input.Keyword = input.Keyword?.Trim();
            var query = _wmsImportTaskRep.AsQueryable()
                .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.TaskNo.Contains(input.Keyword) || u.Sender.Contains(input.Keyword) || u.Receiver.Contains(input.Keyword) || u.Message.Contains(input.Keyword) || u.StockCode.Contains(input.Keyword) || u.Msg.Contains(input.Keyword) || u.StartLocation.Contains(input.Keyword) || u.EndLocation.Contains(input.Keyword) || u.NewEnd.Contains(input.Keyword))
                .WhereIF(!string.IsNullOrWhiteSpace(input.TaskNo), u => u.TaskNo.Contains(input.TaskNo.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Sender), u => u.Sender.Contains(input.Sender.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Receiver), u => u.Receiver.Contains(input.Receiver.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Message), u => u.Message.Contains(input.Message.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.StockCode), u => u.StockCode.Contains(input.StockCode.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Msg), u => u.Msg.Contains(input.Msg.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.StartLocation), u => u.StartLocation.Contains(input.StartLocation.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.EndLocation), u => u.EndLocation.Contains(input.EndLocation.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.NewEnd), u => u.NewEnd.Contains(input.NewEnd.Trim()))
                .WhereIF(input.IsSuccess != null, u => u.IsSuccess == input.IsSuccess)
                .WhereIF(input.SendDateRange?.Length == 2, u => u.SendDate >= input.SendDateRange[0] && u.SendDate <= input.SendDateRange[1])
                .WhereIF(input.BackDateRange?.Length == 2, u => u.BackDate >= input.BackDateRange[0] && u.BackDate <= input.BackDateRange[1])
                .WhereIF(input.Status != null, u => u.Status == input.Status)
                .WhereIF(input.CancelDateRange?.Length == 2, u => u.CancelDate >= input.CancelDateRange[0] && u.CancelDate <= input.CancelDateRange[1])
                .WhereIF(input.FinishDateRange?.Length == 2, u => u.FinishDate >= input.FinishDateRange[0] && u.FinishDate <= input.FinishDateRange[1])
                .WhereIF(input.WareHouseId != null, u => u.WareHouseId == input.WareHouseId);

            if (input.SelectKeyList?.Count > 0)
                query = query.Where(x => input.SelectKeyList.Contains(x.Id));

            var list = await query.ToListAsync();

            if (!list.Any())
                throw Oops.Oh("无可导出数据");

            // 获取字典数据用于翻译
            var sysDictTypeService = App.GetService<SysDictTypeService>();
            var taskStatusDict = sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "TaskStatus" }).Result.ToDictionary(x => x.Value, x => x.Label);

            var exportList = list.Select(u => new ExportWmsImportTaskDto
            {
                TaskNo = u.TaskNo,
                StockCode = u.StockCode,
                StartLocation = u.StartLocation,
                EndLocation = u.EndLocation,
                NewEnd = u.NewEnd,
                Status = taskStatusDict.GetValueOrDefault(u.Status.ToString(), u.Status.ToString()),
                IsSuccess = u.IsSuccess == 1 ? "成功" : (u.IsSuccess == 0 ? "失败" : "未知"),
                SendDate = u.SendDate,
                FinishDate = u.FinishDate,
                Msg = u.Msg,
                Sender = u.Sender,
                Receiver = u.Receiver
            }).ToList();

            using var package = new OfficeOpenXml.ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("入库任务导出记录");

            var props = typeof(ExportWmsImportTaskDto).GetProperties()
                .Where(p => p.GetCustomAttribute<ExporterHeaderAttribute>() != null)
                .ToList();

            var helper = App.GetService<WmsImportNotifyHelper>();
            var headerMap = helper.WriteHeadersByExporterAttribute(sheet, props, 1, 20);

            int currentRow = 2;
            foreach (var item in exportList)
            {
                int col = 1;
                foreach (var prop in props)
                {
                    var val = prop.GetValue(item);
                    sheet.Cells[currentRow, col].Value = val;
                    if (val is DateTime)
                    {
                        sheet.Cells[currentRow, col].Style.Numberformat.Format = "yyyy-mm-dd HH:mm:ss";
                    }
                    col++;
                }
                currentRow++;
            }

            // 设置边框
            if (currentRow > 2)
            {
                var dataRange = sheet.Cells[1, 1, currentRow - 1, props.Count];
                dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                dataRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }

            var stream = new System.IO.MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;
            return new XlsxFileResult(stream: stream,
                fileDownloadName: $"入库任务导出记录-{DateTime.Now:yyyy-MM-dd_HHmmss}");
        }
        catch (Exception ex)
        {
            throw Oops.Oh($"导出失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 下载入库任务表数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载入库任务表数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsImportTaskOutput>(), "入库任务表导入模板");
    }

    private static readonly object _wmsImportTaskImportLock = new object();
    /// <summary>
    /// 导入入库任务表记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入入库任务表记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsImportTaskImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportWmsImportTaskInput, WmsImportTask>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {

                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x =>
                    {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<WmsImportTask>>();

                    var storageable = _wmsImportTaskRep.Context.Storageable(rows)
                        .SplitError(it => it.Item.TaskNo?.Length > 32, "任务号长度不能超过32个字符")
                        .SplitError(it => it.Item.Sender?.Length > 10, "发送方长度不能超过10个字符")
                        .SplitError(it => it.Item.Receiver?.Length > 10, "接收方长度不能超过10个字符")
                        .SplitError(it => it.Item.StockCode?.Length > 10, "托盘码长度不能超过10个字符")
                        .SplitError(it => it.Item.StartLocation?.Length > 50, "起始位置长度不能超过50个字符")
                        .SplitError(it => it.Item.EndLocation?.Length > 50, "目标位置长度不能超过50个字符")
                        .SplitError(it => it.Item.NewEnd?.Length > 32, "二次申请后目标位置长度不能超过32个字符")
                        .SplitInsert(_ => true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();

                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.TaskNo,
                        it.Sender,
                        it.Receiver,
                        it.IsSuccess,
                        it.SendDate,
                        it.BackDate,
                        it.Message,
                        it.StockCode,
                        it.Msg,
                        it.Status,
                        it.CancelDate,
                        it.FinishDate,
                        it.WareHouseId,
                        it.StartLocation,
                        it.EndLocation,
                        it.NewEnd,
                    }).ExecuteCommand();// 存在更新

                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            return stream;
        }
    }

    /// <summary>
    /// 取消入库任务
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("取消入库任务")]
    [ApiDescriptionSettings(Name = "CancelTask"), HttpPost]
    public async Task CancelTask(WmsImportTaskCancelInput input)
    {
        if (input.Id == null && string.IsNullOrWhiteSpace(input.TaskNo))
        {
            throw Oops.Bah("请提供任务Id或任务号");
        }

        var task = await _wmsImportTaskRep.AsQueryable()
            .WhereIF(input.Id != null, t => t.Id == input.Id)
            .WhereIF(!string.IsNullOrWhiteSpace(input.TaskNo), t => t.TaskNo == input.TaskNo)
            .FirstAsync();

        if (task == null)
        {
            throw Oops.Bah("未找到对应的入库任务");
        }

        if (task.Status != TaskStatusPending && task.Status != TaskStatusProcessing)
        {
            throw Oops.Bah("仅支持取消待执行或执行中的任务");
        }

        var tranResult = await _sqlSugarClient.Ado.UseTranAsync(async () =>
        {
            task.Status = TaskStatusCancelled;
            task.CancelDate = DateTime.Now;
            task.IsSuccess = 0;
            await _wmsImportTaskRep.AsUpdateable(task)
                .UpdateColumns(t => new { t.Status, t.CancelDate, t.IsSuccess })
                .ExecuteCommandAsync();

            if (!string.IsNullOrWhiteSpace(task.EndLocation))
            {
                var slot = await _wmsBaseSlotRep.GetFirstAsync(s => s.SlotCode == task.EndLocation && !s.IsDelete);
                if (slot != null)
                {
                    slot.SlotStatus = SlotStatusEmpty;
                    slot.SlotImlockFlag = 0;
                    slot.SlotExlockFlag = 0;
                    await _wmsBaseSlotRep.AsUpdateable(slot)
                        .UpdateColumns(s => new { s.SlotStatus, s.SlotImlockFlag, s.SlotExlockFlag })
                        .ExecuteCommandAsync();
                }
            }

            var importOrder = await _wmsImportOrderRep.GetFirstAsync(o => !o.IsDelete && o.ImportTaskId == task.Id);
            if (importOrder != null)
            {
                importOrder.ImportExecuteFlag = ExecuteFlagPending;
                importOrder.ImportSlotCode = null;
                importOrder.ImportTaskId = null;
                await _wmsImportOrderRep.AsUpdateable(importOrder)
                    .UpdateColumns(o => new { o.ImportExecuteFlag, o.ImportSlotCode, o.ImportTaskId })
                    .ExecuteCommandAsync();

                var boxList = await _wmsStockSlotBoxInfoRep.GetListAsync(b => !b.IsDelete && b.ImportOrderId == importOrder.Id && b.Status != BoxStatusPending);
                if (boxList.Count > 0)
                {
                    foreach (var box in boxList)
                    {
                        box.Status = BoxStatusPending;
                    }
                    await _wmsStockSlotBoxInfoRep.AsUpdateable(boxList)
                        .UpdateColumns(b => new { b.Status })
                        .ExecuteCommandAsync();
                }
            }
        });

        if (!tranResult.IsSuccess)
        {
            throw Oops.Bah($"取消入库任务失败：{tranResult.ErrorMessage}");
        }

        await WmsBaseOperLogHelper.RecordAsync(
            Module: "入库任务",
            OperationType: "取消",
            BusinessNo: task.TaskNo,
            OperationContent: $"取消入库任务：{task.TaskNo}",
            OperParam: input
        );
    }
}
