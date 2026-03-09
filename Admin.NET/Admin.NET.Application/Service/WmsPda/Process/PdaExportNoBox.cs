// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护使用本项目应遵守相关法律法规和许可证的要求
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证(版本 2.0)进行分发和使用许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动!任何基于本项目二次开发而产生的一切法律纠纷和责任,我们不承担任何责任!

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsPda.Dto;
using Admin.NET.Application.Service.WmsSqlView;
using Admin.NET.Application.Service.WmsSqlView.Dto;
using Admin.NET.Core;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Admin.NET.Application.Service.WmsPda.Process;

/// <summary>
/// PDA 出库无箱码拆垛业务处理类
/// <para>处理无箱码出库相关业务功能</para>
/// </summary>
/// <remarks>
/// 主要功能包括:
/// <list type="bullet">
/// <item><description>绑定、查询、删除无箱码拆垛明细</description></item>
/// <item><description>无箱码人工拆垛出库确认</description></item>
/// <item><description>支持自动拆垛和人工拆垛两种模式</description></item>
/// <item><description>库存数量回补与扣减</description></item>
/// <item><description>全托/部分出库处理</description></item>
/// </list>
/// </remarks>
public class PdaExportNoBox : PdaExportBase, ITransient
{
    #region 字段定义
    private readonly ILogger _logger;
    private readonly WmsSqlViewService _sqlViewService;
    #endregion

    #region 常量定义
    private const int StateActive = 0;
    private const int ExportExecuteFlagCompleted = 2;
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖
    /// </summary>
    public PdaExportNoBox(
        ILoggerFactory loggerFactory,
        ISqlSugarClient sqlSugarClient,
        PdaBaseRepos repos,
        WmsSqlViewService sqlViewService) : base(sqlSugarClient, repos)
    {
        _sqlViewService = sqlViewService;
        _logger = loggerFactory.CreateLogger(CommonConst.PdaExportNoBox);
    }
    #endregion

    #region 公共接口方法

    /// <summary>
    /// 绑定无箱码拆跺明细
    /// <para>对应 JC35 接口:【AddOutManualDepalletizing】</para>
    /// </summary>
    /// <param name="input">绑定无箱码拆跺的请求参数，包含出库流水号、托盘编码、扫描数量、出库数量等信息</param>
    /// <returns>绑定操作结果，包含成功状态和消息</returns>
    public async Task<PdaActionResult> AddOutManualDepalletizingAsync(PdaOutManualDepalletizingInput input)
    {
        // 验证输入参数
        ValidateInput(input);
        // 根据出库流水号查询出库订单，不存在则抛出异常
        var exportOrder = await _repos.ExportOrder.GetFirstAsync(x =>
            x.ExportOrderNo == input.ExportOrderNo && x.IsDelete == false) ?? throw Oops.Bah("获取出库流水信息失败!");
        // 加载出库订单对应的库存托盘，并验证仓库匹配
        var stockTray = await LoadExportTrayAsync(exportOrder, expectWarehouseMatch: true) ?? throw Oops.Bah("获取库存托盘信息失败!");
        // 根据托盘ID查询库存明细信息
        var trayId = stockTray.Id.ToString();
        var stockInfo = await _repos.StockInfo.AsQueryable()
            .Where(x => x.TrayId == trayId)
            .FirstAsync() ?? throw Oops.Bah("没有找到对应物料明细!");
        // 创建无箱码拆垛记录（BoxCode为空，因为是无箱码出库）
        var manual = new WmsManualDepalletizing
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            BoxCode = null, // 无箱码标志
            Qty = FormatDecimal(stockInfo.Qty + (stockInfo.LockQuantity ?? 0)), // 托盘总数量（库存+锁定）
            ScanQty = FormatDecimal(input.ScanQty), // 本次扫描数量
            TrayCode = input.TrayCode?.Trim(),
            State = StateActive, // 状态0表示待处理
            OutQty = input.OutQty,
            ExportOrderNo = input.ExportOrderNo.Trim(),
            IsDelete = false,
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now
        };
        // 插入拆垛记录到数据库
        await _repos.ManualDepalletizing.InsertAsync(manual);
        return CreateSuccessResult("绑定成功");
    }

    /// <summary>
    /// 删除无箱码拆跺记录
    /// <para>对应 JC35 接口:【RemoveOutManual】</para>
    /// </summary>
    /// <param name="input">删除无箱码拆跺记录的请求参数，必须包含拆跺记录主键ID</param>
    /// <returns>删除操作结果，包含成功状态和消息</returns>
    public async Task<PdaActionResult> RemoveOutManualAsync(PdaManualRemoveInput input)
    {
        // 验证输入参数
        ValidateInput(input);
        // 验证必须提供拆跺记录ID
        if (!input.Id.HasValue)
            throw Oops.Bah("缺少拆跺记录主键!");
        // 查找待删除的拆跺记录（必须是待处理状态且未删除）
        var manual = await _repos.ManualDepalletizing.AsQueryable()
            .Where(x => x.Id == input.Id.Value && x.State == StateActive && x.IsDelete == false)
            .FirstAsync() ?? throw Oops.Bah("获取拆跺信息失败!");
        // 执行软删除（标记为已删除，而非物理删除）
        manual.IsDelete = true;
        manual.UpdateTime = DateTime.Now;
        // 更新数据库中的删除标志和更新时间
        await _repos.ManualDepalletizing.AsUpdateable(manual)
            .UpdateColumns(x => new { x.IsDelete, x.UpdateTime })
            .ExecuteCommandAsync();
        return CreateSuccessResult();
    }

    /// <summary>
    /// 获取无箱码拆跺信息
    /// <para>对应 JC35 接口:【GetOutManualDepalletizing】</para>
    /// </summary>
    /// <param name="input">查询无箱码拆跺信息的请求参数，必须包含托盘编码和出库流水号</param>
    /// <returns>无箱码拆跺信息列表，包含物料信息、数量、托盘编码等详细信息</returns>
    public async Task<PdaLegacyResult<List<PdaManualDepalletizingItem>>> GetOutManualDepalletizingAsync(PdaManualDepalletizingQueryInput input)
    {
        // 验证输入参数
        ValidateInput(input);
        // 验证托盘编码和出库流水号必填
        var trayCode = ValidateString(input.TrayCode, "托盘编码");
        var orderNo = ValidateString(input.ExportOrderNo, "出库流水");
        // 通过SQL视图查询拆垛明细，筛选指定托盘和出库流水的数据
        var list = await _sqlViewService.QueryManualDepalletizingView()
            .MergeTable()
            .Where(x => x.ExportOrderNo == orderNo && x.StockCode == trayCode && x.GroupQuantity > 0)
            .ToListAsync();
        // 查询拆垛记录表，获取当前待处理的拆垛记录
        var manualRecord = await _repos.ManualDepalletizing.AsQueryable()
            .Where(x => x.TrayCode == trayCode &&
                x.ExportOrderNo == orderNo &&
                x.State == StateActive &&
                x.IsDelete == false)
            .FirstAsync();

        // 如果没有找到拆垛记录，返回空列表
        if (manualRecord == null)
            return CreateLegacyResult<List<PdaManualDepalletizingItem>>(1, "操作成功!", new List<PdaManualDepalletizingItem>(), 0);
        // 将视图查询结果转换为PDA返回格式
        var result = list.Select(item => new PdaManualDepalletizingItem
        {
            BoxCode = null,                             // 无箱码标志
            Qty = item.Qty,                             // 托盘总数量
            ScanQty = item.GroupQuantity,               // 已扫描数量
            TrayCode = item.StockCode,                  // 托盘编码
            MaterialName = item.ExportMaterialName,     // 物料名称
            MaterialCode = item.MaterialCode,           // 物料编码
            StockLotNo = item.LotNo,                    // 批次号
            Id = manualRecord.Id.ToString(),            // 拆垛记录ID
            OutQty = manualRecord.OutQty?.ToString()    // 计划出库数量
        }).ToList();
        return CreateLegacyResult<List<PdaManualDepalletizingItem>>(1, "操作成功!", result, result.Count);
    }

    /// <summary>
    /// 无箱码人工拆垛出库确认
    /// <para>对应 JC35 接口:【OutSaveManTask】</para>
    /// </summary>
    /// <param name="input">无箱码拆垛确认的请求参数，包含托盘编码、出库流水号、拆垛数量、出库类型等信息</param>
    /// <returns>确认操作结果，包含成功状态、消息和数据</returns>
    public async Task<PdaLegacyResult<object>> SaveOutManualTaskAsync(PdaOutManualTaskSaveInput input)
    {
        // 验证输入参数
        ValidateInput(input);
        // 验证托盘编码和出库流水号必填
        var trayCode = ValidateString(input.TrayCode, "托盘编码");
        var requestOrderNo = ValidateString(input.ExportOrderNo, "出库流水");
        // 判断是否为自动拆垛模式（OutType="1"表示自动拆垛，否则为人工拆垛）
        var isAuto = string.Equals(input.OutType?.Trim(), "1", StringComparison.OrdinalIgnoreCase);
        decimal scanQty;
        string resolvedOrderNo = requestOrderNo;
        WmsManualDepalletizing manualRecord = null;
        // 【自动拆垛模式】从出库任务获取数据
        if (isAuto)
        {
            // 解析扫描数量
            scanQty = ParseRequiredDecimal(input.ScanQty, "拆垛数量");
            // 根据任务号查询出库任务记录
            var exportTask = await _repos.ExportTask.GetFirstAsync(x =>
                x.ExportTaskNo == resolvedOrderNo && x.IsDelete == false);
            if (exportTask == null || string.IsNullOrWhiteSpace(exportTask.ExportOrderNo))
                throw Oops.Bah("获取绑定物料信息失败!");
            // 将任务号映射为实际的出库流水号
            resolvedOrderNo = exportTask.ExportOrderNo.Trim();
        }
        // 【人工拆垛模式】从人工拆垛记录获取数据
        else
        {
            // 查询人工绑定的拆垛记录
            manualRecord = await _repos.ManualDepalletizing.GetFirstAsync(x =>
                x.TrayCode == trayCode &&
                x.ExportOrderNo == resolvedOrderNo &&
                x.State == StateActive &&
                x.IsDelete == false);
            if (manualRecord == null)
                throw Oops.Bah("请绑定物料!");
            // 从拆垛记录中获取扫描数量
            scanQty = ParseRequiredDecimal(manualRecord.ScanQty, "拆垛数量");
        }
        // 查询出库流水记录，验证流水号是否存在
        var exportOrder = await _repos.ExportOrder.GetFirstAsync(x =>
            x.ExportOrderNo == resolvedOrderNo && x.IsDelete == false) ?? throw Oops.Bah($"找不到单号为 {resolvedOrderNo} 的出库流水数据");
        // 检查流水是否已经完成（状态>=2表示已完成）
        if ((exportOrder.ExportExecuteFlag ?? 0) >= ExportExecuteFlagCompleted)
        {
            // 更新任务消息并直接返回成功
            await UpdateExportTaskMessageAsync(requestOrderNo, $"流水已完成，流水号:{resolvedOrderNo}");
            return CreateLegacyResultObject(1, "操作成功!", true);
        }
        // 查询出库单据，验证单据状态
        var exportNotify = await _repos.ExportNotify.GetFirstAsync(x =>
            x.ExportBillCode == exportOrder.ExportBillCode &&
            x.IsDelete == false &&
            (x.ExportExecuteFlag ?? 0) <= ExportExecuteFlagCompleted) ?? throw Oops.Bah($"找不到单号为 {exportOrder.ExportBillCode} 的出库单数据，或单据已结束");
        // 查询出库流水明细（箱码信息），获取本次出库的具体托盘信息
        var exportBoxInfo = await _repos.ExportBoxInfo.GetFirstAsync(x =>
            x.ExportOrderNo == resolvedOrderNo &&
            x.StockCodeCode == trayCode &&
            x.IsDelete == false) ?? throw Oops.Bah("获取出库流水明细信息失败!");
        // 查询库存托盘，根据物料、批次、检验状态筛选
        var stockTray = await _repos.StockTray.AsQueryable()
            .Where(x => x.StockCode == trayCode && x.IsDelete == false)
            .WhereIF(exportOrder.ExportMaterialId.HasValue, x =>
                x.MaterialId == SqlFunc.ToString(exportOrder.ExportMaterialId))
            .WhereIF(!string.IsNullOrWhiteSpace(exportOrder.ExportLotNo), x =>
                x.LotNo == exportOrder.ExportLotNo)
            .WhereIF(exportOrder.InspectionStatus.HasValue, x =>
                x.InspectionStatus == exportOrder.InspectionStatus)
            .FirstAsync() ?? throw Oops.Bah("获取库存托盘信息失败!");
        // 计算托盘可用数量（库存数量+锁定数量）
        var availableQty = (stockTray.StockQuantity ?? 0) + (stockTray.LockQuantity ?? 0);
        // 验证扫描数量不能超过托盘可用数量
        if (availableQty < scanQty)
            throw Oops.Bah("绑定数量不得大于库存数量!");
        // 验证托盘必须已出库（StockSlotCode为空表示已出库到暂存区）
        if (!string.IsNullOrWhiteSpace(stockTray.StockSlotCode))
            throw Oops.Bah("托盘出库任务未结束，不能拣货!");
        // 查询库存主记录（仓库级别的库存汇总）
        var stockRecord = await _repos.Stock.AsQueryable()
            .Where(x => x.WarehouseId == exportOrder.ExportWarehouseId && x.IsDelete == false)
            .WhereIF(exportOrder.ExportMaterialId.HasValue, x => x.MaterialId == exportOrder.ExportMaterialId)
            .WhereIF(!string.IsNullOrWhiteSpace(exportOrder.ExportLotNo), x => x.LotNo == exportOrder.ExportLotNo)
            .WhereIF(exportOrder.InspectionStatus.HasValue, x => x.InspectionStatus == exportOrder.InspectionStatus)
            .FirstAsync() ?? throw Oops.Bah("获取库存信息失败!");
        // 查询库存明细（托盘级别的库存明细）
        var trayId = stockTray.Id.ToString();
        var stockInfo = await _repos.StockInfo.AsQueryable()
            .Where(x => x.TrayId == trayId && x.IsDelete == false)
            .WhereIF(!string.IsNullOrWhiteSpace(stockTray.MaterialId), x => x.MaterialId == stockTray.MaterialId)
            .WhereIF(!string.IsNullOrWhiteSpace(stockTray.LotNo), x => x.LotNo == stockTray.LotNo)
            .FirstAsync() ?? throw Oops.Bah("获取库存箱码信息失败!");
        var now = DateTime.Now;
        try
        {
            // 开启数据库事务，确保所有库存更新操作的原子性
            var result = await _sqlSugarClient.Ado.UseTranAsync(async () =>
            {
                // 如果是人工拆垛模式，将拆垛记录标记为已处理
                if (manualRecord != null)
                {
                    await UpdateManualRecordAsync(manualRecord.Id, now);
                }
                // 获取当前库存相关数量
                var pickedQty = exportOrder.PickedNum ?? 0m;        // 计划出库数量（已分配数量）
                var trayStockQty = stockTray.StockQuantity ?? 0m;   // 托盘库存数量
                var trayLockQty = stockTray.LockQuantity ?? 0m;     // 托盘锁定数量
                var stockQty = stockRecord.StockQuantity ?? 0m;     // 仓库库存数量
                var stockLockQty = stockRecord.LockQuantity ?? 0m;  // 仓库锁定数量

                // 获取仓库配置，判断是否允许超出/少出
                var warehouse = exportOrder.ExportWarehouseId.HasValue
                    ? await _repos.Warehouse.GetFirstAsync(x => x.Id == exportOrder.ExportWarehouseId.Value && x.IsDelete == false)
                    : null;

                // 【情况1：超出】实际扫描数量 > 计划出库数量
                if (scanQty > pickedQty)
                {
                    // 检查仓库是否允许超出
                    if (!(warehouse?.IsExceeding ?? false))
                    {
                        throw Oops.Bah($"出库数量超出：计划 {pickedQty}，实际扫描 {scanQty}，超出 {scanQty - pickedQty}当前仓库不允许超出！");
                    }

                    // 超出部分需要扣减托盘库存（因为多出库了）
                    var newQty = Math.Max(0, trayStockQty - (scanQty - pickedQty));
                    stockTray.StockQuantity = newQty;
                    stockInfo.Qty = newQty;
                }
                // 【情况2：少出】实际扫描数量 < 计划出库数量
                else if (scanQty < pickedQty)
                {
                    // 检查仓库是否允许少出
                    if (!(warehouse?.IsUnderpay ?? false))
                    {
                        throw Oops.Bah($"出库数量不足：计划 {pickedQty}，实际扫描 {scanQty}，少出 {pickedQty - scanQty}当前仓库不允许少出！");
                    }

                    // 少出部分需要回补托盘库存（因为没有全部出库）
                    var remainder = pickedQty - scanQty;
                    stockTray.StockQuantity = trayStockQty + remainder;
                    // 优先扣减锁定数量，如果锁定数量不足再扣减库存数量
                    var infoLockQty = stockInfo.LockQuantity ?? 0m;
                    if (infoLockQty > 0)
                    {
                        stockInfo.LockQuantity = Math.Max(0, infoLockQty - scanQty);
                    }
                    else
                    {
                        stockInfo.Qty = Math.Max(0, (stockInfo.Qty ?? 0m) - scanQty);
                    }
                }
                // 【情况3：相等】实际扫描数量 = 计划出库数量
                else
                {
                    // 理论上不需要调整，但代码中仍有余量处理（可能是历史逻辑）
                    var remainder = pickedQty - scanQty; // 这里remainder = 0
                    stockTray.StockQuantity = trayStockQty + remainder;
                    // 扣减锁定数量或库存数量
                    var infoLockQty = stockInfo.LockQuantity ?? 0m;
                    if (infoLockQty > 0)
                    {
                        stockInfo.LockQuantity = Math.Max(0, infoLockQty - scanQty);
                    }
                    else
                    {
                        stockInfo.Qty = Math.Max(0, (stockInfo.Qty ?? 0m) - scanQty);
                    }
                }
                // 更新托盘锁定数量（释放已出库数量对应的锁定）
                stockTray.LockQuantity = Math.Max(0, trayLockQty - pickedQty);
                stockTray.UpdateTime = now;
                // 更新仓库级别的库存和锁定数量
                var lockDiff = stockLockQty - scanQty; // 锁定差值
                var ss = scanQty - pickedQty; // 超出/少出差值
                if (lockDiff >= 0)
                {
                    // 锁定数量足够，直接扣减锁定数量
                    stockRecord.LockQuantity = lockDiff;
                    stockRecord.StockQuantity = Math.Max(0, stockQty - ss);
                }
                else
                {
                    // 锁定数量不够，需要同时扣减库存
                    stockRecord.LockQuantity = 0;
                    stockRecord.StockQuantity = Math.Max(0, stockQty + lockDiff);
                }
                stockRecord.UpdateTime = now;
                // 判断托盘是否已全部出库（库存数量<=0）
                var shouldRemoveTray = (stockTray.StockQuantity ?? 0) <= 0;
                if (shouldRemoveTray)
                {
                    // 检查是否还有其他未完成的出库单据绑定了相同托盘
                    var hasPendingOrder = await _repos.ExportOrder.AsQueryable()
                        .Where(p => p.ExportStockCode == trayCode &&
                                    p.ExportMaterialId == exportOrder.ExportMaterialId &&
                                    p.ExportLotNo == exportOrder.ExportLotNo &&
                                    p.InspectionStatus == exportOrder.InspectionStatus &&
                                    (p.ExportExecuteFlag ?? 0) <= 1 &&
                                    p.ExportOrderNo != resolvedOrderNo &&
                                    p.IsDelete == false).AnyAsync();
                    if (hasPendingOrder)
                    {
                        throw Oops.Bah("还有未出库单据不能全出!");
                    }
                    // 【全托出库】清空托盘数据并标记为已删除
                    stockTray.StockSlotCode = string.Empty;
                    stockTray.StockQuantity = 0;
                    stockTray.LockQuantity = 0;
                    stockTray.IsDelete = true;
                    stockInfo.Qty = 0;
                    stockInfo.LockQuantity = 0;
                    stockInfo.IsDelete = true;
                    stockInfo.UpdateTime = now;
                    // 如果仓库库存也清空了，标记库存记录为已删除
                    if ((stockRecord.StockQuantity ?? 0) <= 0 && (stockRecord.LockQuantity ?? 0) <= 0)
                    {
                        stockRecord.IsDelete = true;
                    }
                    // 批量更新托盘、库存明细、库存主记录
                    await _repos.StockTray.AsUpdateable(stockTray)
                        .UpdateColumns(x => new { x.StockSlotCode, x.StockQuantity, x.LockQuantity, x.IsDelete, x.UpdateTime })
                        .ExecuteCommandAsync();
                    await _repos.StockInfo.AsUpdateable(stockInfo)
                        .UpdateColumns(x => new { x.Qty, x.LockQuantity, x.IsDelete, x.UpdateTime })
                        .ExecuteCommandAsync();
                    await _repos.Stock.AsUpdateable(stockRecord)
                        .UpdateColumns(x => new { x.StockQuantity, x.LockQuantity, x.IsDelete, x.UpdateTime })
                        .ExecuteCommandAsync();
                    // 检查是否还有其他托盘使用相同的托盘编码
                    var hasOtherTray = await _repos.StockTray.AsQueryable()
                        .Where(x => x.StockCode == stockTray.StockCode && x.Id != stockTray.Id && x.IsDelete == false)
                        .AnyAsync();
                    // 如果没有其他托盘使用该编码，释放托盘编码（状态设为0=可用）
                    if (!hasOtherTray)
                    {
                        await _repos.SysStockCode.AsUpdateable()
                            .SetColumns(x => new WmsSysStockCode { Status = 0, UpdateTime = now })
                            .Where(x => x.StockCode == stockTray.StockCode && x.IsDelete == false)
                            .ExecuteCommandAsync();
                    }
                }
                else
                {
                    // 【部分出库】标记托盘为零散托盘（OddMarking=1）
                    stockTray.OddMarking = 1;
                    stockInfo.OddMarking = "1";
                    stockInfo.UpdateTime = now;
                    // 更新托盘、库存明细、库存主记录的数量和零散标志
                    await _repos.StockTray.AsUpdateable(stockTray)
                        .UpdateColumns(x => new { x.StockQuantity, x.LockQuantity, x.OddMarking, x.UpdateTime })
                        .ExecuteCommandAsync();
                    await _repos.StockInfo.AsUpdateable(stockInfo)
                        .UpdateColumns(x => new { x.Qty, x.LockQuantity, x.OddMarking, x.UpdateTime })
                        .ExecuteCommandAsync();
                    await _repos.Stock.AsUpdateable(stockRecord)
                        .UpdateColumns(x => new { x.StockQuantity, x.LockQuantity, x.UpdateTime })
                        .ExecuteCommandAsync();
                }
                // 更新出库流水明细的拣货完成数量
                exportBoxInfo.PickEdNum = (exportBoxInfo.PickEdNum ?? 0) + scanQty;
                // 更新出库流水明细的拣货数量（如果不是必出箱码，则累加扫描数量）
                var newPickNum = exportBoxInfo.PickNum ?? 0m;
                if ((exportBoxInfo.IsMustExport ?? 0) == 0)
                {
                    newPickNum += scanQty;
                }
                exportBoxInfo.PickNum = newPickNum;
                exportBoxInfo.Status = 1; // 状态1表示已拣货
                exportBoxInfo.UpdateTime = now;
                // 提交出库流水明细更新
                await _repos.ExportBoxInfo.AsUpdateable(exportBoxInfo)
                    .UpdateColumns(x => new { x.PickEdNum, x.PickNum, x.Status, x.UpdateTime })
                    .ExecuteCommandAsync();
                // 更新出库流水状态和扫描数量
                await UpdateExportOrderAsync(exportOrder, scanQty, now);
                // 更新出库单据的完成数量和状态
                await UpdateExportNotifyAsync(exportOrder, scanQty, now);
            });
            // 检查事务执行结果
            if (!result.IsSuccess)
            {
                // 事务失败，更新任务消息并抛出异常
                await UpdateExportTaskMessageAsync(requestOrderNo, result.ErrorMessage);
                throw Oops.Oh($"提交失败:{result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            // 捕获所有异常，记录错误消息到任务表
            await UpdateExportTaskMessageAsync(requestOrderNo, ex.Message);
            throw;
        }
        return CreateLegacyResultObject(1, "操作成功!", true);
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 将指定人工拆垛记录标记为已处理
    /// </summary>
    /// <param name="manualId">人工拆垛记录ID</param>
    /// <param name="now">当前时间</param>
    private async Task UpdateManualRecordAsync(long manualId, DateTime now)
    {
        // 构造拆垛记录对象，只更新状态和时间字段
        var manual = new WmsManualDepalletizing
        {
            Id = manualId,
            State = 1, // 状态1表示已处理
            UpdateTime = now
        };
        // 仅更新State和UpdateTime字段，避免覆盖其他字段
        await _repos.ManualDepalletizing.AsUpdateable(manual).UpdateColumns(x => new { x.State, x.UpdateTime }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 推进出库流水状态并累计完成数量
    /// </summary>
    /// <param name="exportOrder">出库流水信息</param>
    /// <param name="totalScanQty">本次扫描总数量</param>
    /// <param name="now">当前时间</param>
    private async Task UpdateExportOrderAsync(WmsExportOrder exportOrder, decimal totalScanQty, DateTime now)
    {
        // 将出库流水标记为已完成（状态2）
        exportOrder.ExportExecuteFlag = 2;
        // 累加扫描数量到出库流水的总扫描数量
        exportOrder.ScanQuantity = (exportOrder.ScanQuantity ?? 0) + totalScanQty;
        // 记录完成时间
        exportOrder.CompleteDate = now;
        exportOrder.UpdateTime = now;
        // 仅更新指定字段
        await _repos.ExportOrder.AsUpdateable(exportOrder).UpdateColumns(x => new { x.ExportExecuteFlag, x.ScanQuantity, x.CompleteDate, x.UpdateTime }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新出库任务消息
    /// </summary>
    /// <param name="key">任务号或流水号</param>
    /// <param name="message">消息内容</param>
    private async Task UpdateExportTaskMessageAsync(string key, string message)
    {
        // 验证参数有效性
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        // 根据任务号或流水号更新任务消息（用于记录错误信息或状态提示）
        await _repos.ExportTask.AsUpdateable().SetColumns(t => new WmsExportTask { Information = message }).Where(t => t.ExportTaskNo == key || t.ExportOrderNo == key).ExecuteCommandAsync();
    }

    #endregion
}
