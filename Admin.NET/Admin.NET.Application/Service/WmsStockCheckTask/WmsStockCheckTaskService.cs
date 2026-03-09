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
using Admin.NET.Application.Service.WmsBaseOperLog;
using Admin.NET.Application.Service.WmsBaseOperLog.Enum;
using Admin.NET.Core;

namespace Admin.NET.Application;

/// <summary>
/// 盘点任务表服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsStockCheckTaskService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsStockCheckTask> _wmsStockCheckTaskRep;
    private readonly SqlSugarRepository<WmsStockCheckNotify> _wmsStockCheckNotifyRep;
    private readonly SqlSugarRepository<WmsStockCheckNotifyDetail> _wmsStockCheckNotifyDetailRep;
    private readonly SqlSugarRepository<WmsBaseSlot> _wmsBaseSlotRep;
    private readonly SqlSugarRepository<WmsStockTray> _wmsStockTrayRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsStockCheckTaskService(
        SqlSugarRepository<WmsStockCheckTask> wmsStockCheckTaskRep,
        SqlSugarRepository<WmsStockCheckNotify> wmsStockCheckNotifyRep,
        SqlSugarRepository<WmsStockCheckNotifyDetail> wmsStockCheckNotifyDetailRep,
        SqlSugarRepository<WmsBaseSlot> wmsBaseSlotRep,
        SqlSugarRepository<WmsStockTray> wmsStockTrayRep,
        ISqlSugarClient sqlSugarClient)
    {
        _wmsStockCheckTaskRep = wmsStockCheckTaskRep;
        _wmsStockCheckNotifyRep = wmsStockCheckNotifyRep;
        _wmsStockCheckNotifyDetailRep = wmsStockCheckNotifyDetailRep;
        _wmsBaseSlotRep = wmsBaseSlotRep;
        _wmsStockTrayRep = wmsStockTrayRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询盘点任务表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询盘点任务表")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsStockCheckTaskOutput>> Page(PageWmsStockCheckTaskInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsStockCheckTaskRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.CheckTaskNo.Contains(input.Keyword) || u.CheckBillCode.Contains(input.Keyword) || u.StockCode.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CheckTaskNo), u => u.CheckTaskNo.Contains(input.CheckTaskNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CheckBillCode), u => u.CheckBillCode.Contains(input.CheckBillCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StockCode), u => u.StockCode.Contains(input.StockCode.Trim()))
            .WhereIF(input.CheckTaskFlag.HasValue, u => u.CheckTaskFlag == input.CheckTaskFlag)
            .WhereIF(input.SendDateRange?.Length == 2, u => u.SendDate >= input.SendDateRange[0] && u.SendDate <= input.SendDateRange[1])
            .WhereIF(input.BackDateRange?.Length == 2, u => u.BackDate >= input.BackDateRange[0] && u.BackDate <= input.BackDateRange[1])
            .WhereIF(input.TaskType.HasValue, u => u.TaskType == input.TaskType)
            .Where(u => u.IsDelete == false)
            .Select<WmsStockCheckTaskOutput>();
        return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 取消盘点任务 🚫
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("取消盘点任务")]
    [ApiDescriptionSettings(Name = "CancelTask"), HttpPost]
    public async Task CancelTask(CancelWmsStockCheckTaskInput input)
    {
        try
        {
            _sqlSugarClient.Ado.BeginTran();

            var entity = await _wmsStockCheckTaskRep.AsQueryable()
                .With(SqlWith.RowLock)
                .FirstAsync(u => u.CheckTaskNo == input.CheckTaskNo) ?? throw Oops.Oh(ErrorCodeEnum.D1002);

            // 校验任务状态，只有已下发但未完成的任务才能取消
            if (entity.CheckTaskFlag != 0 && entity.CheckTaskFlag != 1)
            {
                throw Oops.Oh("只有已下发或执行中的任务才能取消！");
            }

            #region 根据任务类型恢复储位状态
            // 储位状态：0空储位、1有物品、2正在入库、3正在出库、4正在移入、5正在移出、6空托盘、7屏蔽、8储位不存在、9异常储位
            if (entity.TaskType == 1) // 出库任务
            {
                var startSlot = await _wmsBaseSlotRep.AsQueryable()
                    .With(SqlWith.RowLock)
                    .FirstAsync(u => u.SlotCode == entity.StartLocation);
                if (startSlot != null)
                {
                    // 校验储位状态：只有正在出库状态(3)才能恢复
                    if (startSlot.SlotStatus == 3)
                    {
                        startSlot.SlotStatus = 1; // 恢复为有货状态
                        await _wmsBaseSlotRep.AsUpdateable(startSlot).ExecuteCommandAsync();
                    }
                    else if (startSlot.SlotStatus == 1 || startSlot.SlotStatus == 2 || startSlot.SlotStatus == 6)
                    {
                        // 储位已被新任务占用（重新入库/有货/空托盘），不能取消
                        throw Oops.Oh($"出库原储位 {entity.StartLocation} 已被新任务占用（状态：{GetSlotStatusName(startSlot.SlotStatus)}），无法取消任务！");
                    }
                    else if (startSlot.SlotStatus == 0)
                    {
                        // 储位已空闲，说明出库已完成或已被其他出库任务处理
                        throw Oops.Oh($"出库原储位 {entity.StartLocation} 已空闲，出库任务可能已完成，无法取消任务！");
                    }
                    else if (startSlot.SlotStatus == 4 || startSlot.SlotStatus == 5)
                    {
                        // 储位正在移库中
                        throw Oops.Oh($"出库原储位 {entity.StartLocation} 正在移库中（状态：{GetSlotStatusName(startSlot.SlotStatus)}），无法取消任务！");
                    }
                    else if (startSlot.SlotStatus == 7 || startSlot.SlotStatus == 8 || startSlot.SlotStatus == 9)
                    {
                        // 储位异常状态
                        throw Oops.Oh($"出库原储位 {entity.StartLocation} 状态异常（状态：{GetSlotStatusName(startSlot.SlotStatus)}），无法取消任务！");
                    }
                }

                // 恢复载具占用状态
                var stockTray = await _wmsStockTrayRep.AsQueryable()
                    .With(SqlWith.RowLock)
                    .FirstAsync(u => u.StockCode == entity.StockCode);
                if (stockTray != null && stockTray.StockStatusFlag == 1)
                {
                    stockTray.StockStatusFlag = 0;
                    await _wmsStockTrayRep.AsUpdateable(stockTray).ExecuteCommandAsync();
                }
            }
            else if (entity.TaskType == 3) // 移库任务
            {
                var startSlot = await _wmsBaseSlotRep.AsQueryable()
                    .With(SqlWith.RowLock)
                    .FirstAsync(u => u.SlotCode == entity.StartLocation);
                if (startSlot != null)
                {
                    // 校验起始储位状态：只有正在移出(5)才能恢复
                    if (startSlot.SlotStatus == 5)
                    {
                        startSlot.SlotStatus = 1; // 恢复为有货状态
                        await _wmsBaseSlotRep.AsUpdateable(startSlot).ExecuteCommandAsync();
                    }
                    else if (startSlot.SlotStatus == 1)
                    {
                        // 起始储位已恢复为有货，可能是移库已完成
                        throw Oops.Oh($"移库起始储位 {entity.StartLocation} 已恢复为有货状态，移库任务可能已完成，无法取消任务！");
                    }
                    else if (startSlot.SlotStatus == 0 || startSlot.SlotStatus == 6)
                    {
                        // 起始储位已空闲/空托盘
                        throw Oops.Oh($"移库起始储位 {entity.StartLocation} 已空闲（状态：{GetSlotStatusName(startSlot.SlotStatus)}），移库任务可能已完成，无法取消任务！");
                    }
                    else if (startSlot.SlotStatus == 2 || startSlot.SlotStatus == 3 || startSlot.SlotStatus == 4)
                    {
                        // 起始储位被其他任务占用
                        throw Oops.Oh($"移库起始储位 {entity.StartLocation} 被其他任务占用（状态：{GetSlotStatusName(startSlot.SlotStatus)}），无法取消任务！");
                    }
                    else if (startSlot.SlotStatus == 7 || startSlot.SlotStatus == 8 || startSlot.SlotStatus == 9)
                    {
                        // 起始储位异常状态
                        throw Oops.Oh($"移库起始储位 {entity.StartLocation} 状态异常（状态：{GetSlotStatusName(startSlot.SlotStatus)}），无法取消任务！");
                    }
                }

                var endSlot = await _wmsBaseSlotRep.AsQueryable()
                    .With(SqlWith.RowLock)
                    .FirstAsync(u => u.SlotCode == entity.EndLocation);
                if (endSlot != null)
                {
                    // 校验目标储位状态：只有正在移入(4)才能恢复
                    if (endSlot.SlotStatus == 4)
                    {
                        endSlot.SlotStatus = 0; // 恢复为空闲状态
                        await _wmsBaseSlotRep.AsUpdateable(endSlot).ExecuteCommandAsync();
                    }
                    else if (endSlot.SlotStatus == 1 || endSlot.SlotStatus == 6)
                    {
                        // 目标储位已有货/空托盘，说明移库已完成或已被新入库任务占用
                        throw Oops.Oh($"移库目标储位 {entity.EndLocation} 已有货（状态：{GetSlotStatusName(endSlot.SlotStatus)}），无法取消任务！");
                    }
                    else if (endSlot.SlotStatus == 2)
                    {
                        // 目标储位正在入库中
                        throw Oops.Oh($"移库目标储位 {entity.EndLocation} 正在入库中，无法取消任务！");
                    }
                    else if (endSlot.SlotStatus == 3 || endSlot.SlotStatus == 5)
                    {
                        // 目标储位被其他出库/移库任务占用
                        throw Oops.Oh($"移库目标储位 {entity.EndLocation} 被其他出库/移库任务占用，无法取消任务！");
                    }
                    else if (endSlot.SlotStatus == 7 || endSlot.SlotStatus == 8 || endSlot.SlotStatus == 9)
                    {
                        // 目标储位异常状态
                        throw Oops.Oh($"移库目标储位 {entity.EndLocation} 状态异常（状态：{GetSlotStatusName(endSlot.SlotStatus)}），无法取消任务！");
                    }
                }

                // 恢复载具占用状态
                var stockTray = await _wmsStockTrayRep.AsQueryable()
                    .With(SqlWith.RowLock)
                    .FirstAsync(u => u.StockCode == entity.StockCode);
                if (stockTray != null && stockTray.StockStatusFlag == 1)
                {
                    stockTray.StockStatusFlag = 0;
                    await _wmsStockTrayRep.AsUpdateable(stockTray).ExecuteCommandAsync();
                }
            }
            #endregion

            #region 恢复盘点单据和明细状态
            if (!string.IsNullOrWhiteSpace(entity.CheckBillCode))
            {
                var notify = await _wmsStockCheckNotifyRep.AsQueryable()
                    .With(SqlWith.RowLock)
                    .FirstAsync(u => u.CheckBillCode == entity.CheckBillCode);
                if (notify != null && notify.ExecuteFlag == 1)
                {
                    notify.ExecuteFlag = 0; // 恢复为待执行状态
                    await _wmsStockCheckNotifyRep.AsUpdateable(notify).ExecuteCommandAsync();
                }

                var details = await _wmsStockCheckNotifyDetailRep.AsQueryable()
                    .Where(u => u.CheckBillCode == entity.CheckBillCode && u.CheckTaskNo == entity.CheckTaskNo && u.ExecuteFlag == 1)
                    .ToListAsync();

                if (details.Count > 0)
                {
                    foreach (var detail in details)
                    {
                        detail.ExecuteFlag = 0; // 恢复为待执行状态
                    }
                    await _wmsStockCheckNotifyDetailRep.AsUpdateable(details).ExecuteCommandAsync();
                }
            }
            #endregion

            #region 更新任务状态
            entity.CheckTaskFlag = 4; // 手动取消
            entity.UpdateTime = DateTime.Now;
            await _wmsStockCheckTaskRep.AsUpdateable(entity).ExecuteCommandAsync();
            #endregion

            _sqlSugarClient.Ado.CommitTran();

            await WmsBaseOperLogHelper.RecordAsync(
                Module: "盘点任务",
                OperationType: OperationTypeEnum.Update.GetEnumDescription(),
                BusinessNo: entity.CheckTaskNo,
                OperationContent: $"取消盘点任务：{entity.CheckTaskNo} 成功",
                OperParam: input
            );
        }
        catch (Exception ex)
        {
            _sqlSugarClient.Ado.RollbackTran();
            throw Oops.Oh($"取消盘点任务失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取储位状态名称
    /// </summary>
    /// <param name="status">储位状态码</param>
    /// <returns>储位状态名称</returns>
    private string GetSlotStatusName(int? status)
    {
        return status switch
        {
            0 => "空储位",
            1 => "有物品",
            2 => "正在入库",
            3 => "正在出库",
            4 => "正在移入",
            5 => "正在移出",
            6 => "空托盘",
            7 => "屏蔽",
            8 => "储位不存在",
            9 => "异常储位",
            _ => "未知状态"
        };
    }
}
