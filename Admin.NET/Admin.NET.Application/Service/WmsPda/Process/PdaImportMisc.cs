// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护使用本项目应遵守相关法律法规和许可证的要求
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证(版本 2.0)进行分发和使用许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动!任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任!

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsPda.Dto;
using Admin.NET.Application.Service.WmsSqlView;
using Admin.NET.Core;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Admin.NET.Application.Service.WmsPda.Process;

/// <summary>
/// PDA入库杂项业务处理类
/// <para>承载 JC35 <c>PdaInterfaceController</c> 入库辅助功能相关接口迁移，提供托盘明细查询、暂存入库和叠箱绑定等业务功能</para>
/// </summary>
/// <remarks>
/// 主要功能包括：
/// <list type="bullet">
/// <item><description>托盘明细查询：按托盘编码查询箱码聚合结果</description></item>
/// <item><description>暂存入库：生成暂存入库流水</description></item>
/// <item><description>叠箱绑定：主/副载具合并</description></item>
/// </list>
/// </remarks>
public class PdaImportMisc : PdaImportBase, ITransient
{
    #region 字段定义
    private readonly ILogger _logger;                            //日志
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖
    /// </summary>
    public PdaImportMisc(
        ILoggerFactory loggerFactory,
        ISqlSugarClient sqlSugarClient,
        PdaBaseRepos repos,
        WmsSqlViewService sqlViewService) : base(sqlSugarClient, repos, sqlViewService)
    {
        _logger = loggerFactory.CreateLogger(CommonConst.PdaImportMisc);
    }
    #endregion

    #region 托盘明细查询
    /// <summary>
    /// 托盘明细查询
    /// <para>对应 JC35 接口：【GetPalnoDetailInfo】</para>
    /// </summary>
    /// <param name="input">托盘明细查询的请求参数，包含托盘编码</param>
    /// <returns>托盘下箱码聚合结果，包含成功状态、消息和明细列表</returns>
    /// <exception cref="Exception">当参数验证失败或查询过程中发生错误时抛出异常</exception>
    /// <remarks>
    /// 查询条件：
    /// <list type="number">
    /// <item><description>仅检索 Status=0、IsDelete=0 的箱码记录</description></item>
    /// <item><description>按托盘编码精确匹配</description></item>
    /// <item><description>按箱码降序排列</description></item>
    /// </list>
    /// 聚合规则：
    /// <list type="bullet">
    /// <item><description>按箱码、物料编码、物料名称、批号分组</description></item>
    /// <item><description>相同分组内累加数量</description></item>
    /// <item><description>返回分组的第一条记录的详细信息和累加数量</description></item>
    /// </list>
    /// 兼容性：
    /// <list type="bullet">
    /// <item><description>返回与JC35相同的字段结构</description></item>
    /// <item><description>保留原中文提示文案</description></item>
    /// <item><description>查询失败时仍返回友好的错误信息</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaPalletDetailOutput> QueryPalletDetailAsync(PdaPalletDetailInput input)
    {
        // 验证输入参数完整性
        if (input == null || string.IsNullOrWhiteSpace(input.PalletCode))
        {
            throw Oops.Bah("托盘编码不能为空！");
        }
        var output = new PdaPalletDetailOutput();
        try
        {
            // 托盘编码
            var palletCode = input.PalletCode.Trim();
            // 查询托盘下所有未删除且待绑定的箱码记录（Status=0表示待绑定）
            var rows = await _sqlViewService.QueryStockSlotBoxInfoView()
                .MergeTable()
                .Where(x => x.IsDelete == false && x.Status == 0)
                .Where(x => x.StockCode == palletCode)
                .OrderBy(x => x.BoxCode, OrderByType.Desc)
                .Select(x => new
                {
                    LotNo = SqlFunc.Coalesce(x.LotNo, string.Empty),
                    BoxCode = SqlFunc.Coalesce(x.BoxCode, string.Empty),
                    Qty = SqlFunc.Coalesce(x.Qty, 0m),
                    MaterialCode = SqlFunc.Coalesce(x.MaterialCode, string.Empty),
                    MaterialName = SqlFunc.Coalesce(x.MaterialName, string.Empty)
                })
                .ToListAsync();
            // 按箱码、物料、批次分组聚合数量
            output.Items = rows
                .GroupBy(x => new { x.BoxCode, x.MaterialCode, x.MaterialName, x.LotNo })
                .Select(group => new PdaPalletDetailItem
                {
                    BoxCode = group.Key.BoxCode,
                    MaterialCode = group.Key.MaterialCode,
                    MaterialName = group.Key.MaterialName,
                    LotNo = group.Key.LotNo,
                    Qty = group.First().Qty ?? 0m,          // 原始数量（取第一条）
                    Quantity = group.Sum(item => item.Qty ?? 0m)  // 累加总量
                }).OrderByDescending(item => item.BoxCode).ToList();
            output.Success = true;
            output.Message = output.Items.Count > 0 ? "查询成功" : "未找到相关记录";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询托盘明细失败：{@Input}", input);
            output.Success = false;
            output.Message = ex.Message;
        }
        return output;
    }
    #endregion

    #region 暂存入库管理
    /// <summary>
    /// JC35【TemporaryStorage】迁移实现：生成暂存入库流水
    /// </summary>
    /// <param name="input">暂存入库参数</param>
    /// <returns>操作结果</returns>
    public async Task<PdaActionResult> TemporaryStorageAsync(PdaTemporaryStorageInput input)
    {
        // 验证输入参数完整性
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.VehicleCode))
            throw Oops.Bah("主载具不能为空！");
        // 操作用户
        var user = NormalizeUser(input.UserId);
        // 当前时间
        var now = DateTime.Now;
        // 【第一步：收集托盘编码】支持主副载具，去重后收集所有托盘编码
        var trayCodes = CollectTrayCodes(input.VehicleCode, input.VehicleSubCode);
        // 【第二步：查询出库流水】获取每个托盘最新的出库流水记录
        var exportOrders = new List<WmsExportOrder>();
        foreach (var trayCode in trayCodes)
        {
            var order = await ResolveLatestExportOrderAsync(trayCode);
            exportOrders.Add(order);
        }
        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                // 【第三步：生成暂存入库流水】为每个出库流水创建对应的暂存入库流水
                foreach (var order in exportOrders)
                {
                    // 获取托盘主数据
                    var stockCode = await ResolveStockCodeAsync(order.ExportStockCode);
                    // 创建暂存入库流水（IsTemporaryStorage="1"）
                    var importOrder = await CreateTemporaryImportOrderAsync(order, stockCode, user, now, input.VehicleCode.Trim());
                    // 将出库箱码重新关联到新生成的入库流水
                    await RelinkBoxesToImportOrderAsync(order.ExportOrderNo, importOrder.Id, now, user);
                }
            }, "暂存入库生成失败！");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "暂存入库生成失败：{@Input}", input);
            throw;
        }
        var message = exportOrders.Count > 0
            ? $"绑定完成，共生成 {exportOrders.Count} 条入库流水"
            : "未生成任何入库流水！";
        return new PdaActionResult
        {
            Success = exportOrders.Count > 0,
            Message = message
        };
    }
    /// <summary>
    /// JC35【StackingBindings】迁移实现：叠箱绑定（主/副载具合并）
    /// </summary>
    /// <param name="input">叠箱绑定参数</param>
    /// <returns>操作结果</returns>
    public async Task<PdaActionResult> StackingBindingsAsync(PdaStackingBindingInput input)
    {
        // 验证输入参数完整性
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.VehicleCode))
            throw Oops.Bah("主载具不能为空！");
        if (string.IsNullOrWhiteSpace(input.VehicleSubCode))
            throw Oops.Bah("副载具不能为空！");
        // 主载具编码
        var primaryCode = input.VehicleCode.Trim();
        // 副载具编码
        var secondaryCode = input.VehicleSubCode.Trim();
        // 操作用户
        var user = NormalizeUser(input.UserId);
        // 当前时间
        var now = DateTime.Now;
        // 【第一步：获取主副载具的入库流水】查询待执行状态的入库流水（ImportExecuteFlag="01"）
        var primaryOrder = await ResolveLatestPendingImportOrderAsync(primaryCode, "主载具");
        var secondaryOrder = await ResolveLatestPendingImportOrderAsync(secondaryCode, "副载具");
        // 【第二步：验证业务类型一致性】主副载具必须属于同一业务类型（YsOrTJ字段）
        var primaryBiz = primaryOrder.YsOrTJ ?? string.Empty;
        var secondaryBiz = secondaryOrder.YsOrTJ ?? string.Empty;
        if (!string.Equals(primaryBiz, secondaryBiz, StringComparison.OrdinalIgnoreCase))
            throw Oops.Bah("两个载具业务不同！");
        // 【第三步：设置副载具编码】将主副载具的SubVehicleCode都设为主载具编码
        primaryOrder.SubVehicleCode = primaryCode;
        primaryOrder.UpdateTime = now;
        primaryOrder.UpdateUserName = user;
        // 副载具也指向主载具
        secondaryOrder.SubVehicleCode = primaryCode;
        secondaryOrder.UpdateTime = now;
        secondaryOrder.UpdateUserName = user;
        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                // 【第四步：更新入库流水】提交主副载具的SubVehicleCode更新
                await _repos.ImportOrder.AsUpdateable(primaryOrder)
                    .UpdateColumns(x => new { x.SubVehicleCode, x.UpdateTime, x.UpdateUserName })
                    .ExecuteCommandAsync();
                await _repos.ImportOrder.AsUpdateable(secondaryOrder)
                    .UpdateColumns(x => new { x.SubVehicleCode, x.UpdateTime, x.UpdateUserName })
                    .ExecuteCommandAsync();
            }, "叠箱绑定失败！");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "叠箱绑定失败：{@Input}", input);
            throw;
        }
        return new PdaActionResult
        {
            Success = true,
            Message = "操作成功"
        };
    }
    #endregion

    #region 私有辅助方法
    /// <summary>
    /// 获取托盘最新的出库流水
    /// </summary>
    /// <param name="trayCode">托盘编码</param>
    /// <returns>出库流水</returns>
    private async Task<WmsExportOrder> ResolveLatestExportOrderAsync(string trayCode)
    {
        // 查询指定托盘最新的出库流水记录（按创建时间倒序）
        var order = await _repos.ExportOrder.AsQueryable()
            .Where(x => x.IsDelete == false && x.ExportStockCode == trayCode)
            .OrderBy(x => x.CreateTime, OrderByType.Desc)
            .FirstAsync() ?? throw Oops.Bah($"未找到托盘({trayCode})对应的出库流水！");

        return order;
    }

    /// <summary>
    /// 依据出库流水生成暂存入库流水
    /// </summary>
    /// <param name="exportOrder">出库订单</param>
    /// <param name="stockCode">库存编码</param>
    /// <param name="user">用户</param>
    /// <param name="now">当前时间</param>
    /// <param name="primaryVehicle">主载具托盘编码</param>
    /// <returns>入库流水</returns>
    private async Task<WmsImportOrder> CreateTemporaryImportOrderAsync(WmsExportOrder exportOrder, WmsSysStockCode stockCode, string user, DateTime now, string primaryVehicle)
    {
        // 解析出库重量（失败时返回0）
        var weight = ParseDecimalOrZero(exportOrder.ExportWeight);
        // 根据出库流水信息构建暂存入库流水
        var importOrder = new WmsImportOrder
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            ImportOrderNo = _commonMethod.GetImExNo("RK"),       // 生成入库流水号
            WareHouseId = stockCode.WarehouseId,                 // 仓库ID
            StockCodeId = stockCode.Id,                          // 托盘ID
            StockCode = stockCode.StockCode,                     // 托盘编码
            ImportQuantity = exportOrder.ExportQuantity ?? 0,    // 入库数量（复制出库数量）
            ImportWeight = weight,                               // 入库重量
            Weight = weight,                                     // 重量
            ImportExecuteFlag = "01",                            // 执行标志：01=待执行
            LotNo = exportOrder.ExportLotNo,                     // 批次号
            ImportProductionDate = exportOrder.ExportProductionDate,  // 生产日期
            CreateTime = now,
            CreateUserName = user,
            CreateUserId = 0,
            UpdateTime = now,
            UpdateUserName = user,
            UpdateUserId = 0,
            IsDelete = false,
            InspectFlag = 2,                                     // 检验标志：2=免检
            IsTemporaryStorage = "1",                            // 标记为暂存入库
            SubVehicleCode = primaryVehicle                      // 主载具编码
        };
        await _repos.ImportOrder.InsertAsync(importOrder);
        return importOrder;
    }

    /// <summary>
    /// 将出库箱码重新关联到新生成的入库流水
    /// </summary>
    /// <param name="exportOrderNo">出库单据号</param>
    /// <param name="importOrderId">入库流水ID</param>
    /// <param name="now">当前时间</param>
    /// <param name="user">用户</param>
    private async Task RelinkBoxesToImportOrderAsync(string? exportOrderNo, long importOrderId, DateTime now, string user)
    {
        // 出库单据号为空，跳过处理
        if (string.IsNullOrWhiteSpace(exportOrderNo))
            return;
        // 【第一步：查询出库箱码集合】获取该出库流水关联的所有箱码
        var codeSet = (await _repos.ExportBoxInfo.AsQueryable()
                .Where(x => x.IsDelete == false && x.ExportOrderNo == exportOrderNo)
                .Select(x => x.BoxCode)
                .ToListAsync())
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            // 不区分大小写的HashSet
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        // 箱码集合为空，跳过处理
        if (codeSet.Count == 0)
            return;
        // 【第二步：查询箱托绑定记录】获取这些箱码对应的箱托绑定信息
        var slotBoxes = await _repos.StockSlotBoxInfo.AsQueryable()
            .Where(x => x.IsDelete == false && codeSet.Contains(x.BoxCode))
            .OrderBy(x => x.CreateTime, OrderByType.Desc)
            .ToListAsync();
        // 【第三步：更新箱托关联】将箱码重新关联到新的入库流水，并重置状态为待绑定
        // 按箱码分组，取最新的记录
        foreach (var box in slotBoxes.GroupBy(x => x.BoxCode).Select(g => g.First()))
        {
            // 关联到新的入库流水
            box.ImportOrderId = importOrderId;
            // 重置为待绑定状态
            box.Status = 0;
            box.UpdateTime = now;
            box.UpdateUserName = user;
            await _repos.StockSlotBoxInfo.AsUpdateable(box)
                .UpdateColumns(x => new { x.ImportOrderId, x.Status, x.UpdateTime, x.UpdateUserName })
                .ExecuteCommandAsync();
        }
    }
    #endregion
}
