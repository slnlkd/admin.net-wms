// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护使用本项目应遵守相关法律法规和许可证的要求
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsPda.Dto;
using Admin.NET.Core;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using SqlSugar;
namespace Admin.NET.Application.Service.WmsPda.Process;
/// <summary>
/// PDA 盘库相关业务处理服务
/// <para>承接 JC35 <c>PdaInterfaceController</c> 中盘点接口的迁移实现，提供完整的PDA盘库相关业务功能</para>
/// </summary>
/// <remarks>
/// 主要功能包括：
/// <list type="bullet">
/// <item><description>盘点单管理：查询盘点单列表、获取盘点单信息</description></item>
/// <item><description>盘点操作：查询盘点箱码列表、标记箱码扫描状态、提交盘点结果</description></item>
/// <item><description>托盘变更：无箱码托盘变更、库存转移、托盘状态管理</description></item>
/// <item><description>托盘验证：验证托盘存在性、检查托盘重复性</description></item>
/// <item><description>物料查询：根据托盘获取物料信息、关联库存数据</description></item>
/// <item><description>数据统计：盘点结果计算、盈亏分析、状态更新</description></item>
/// </list>
/// </remarks>
public class PdaStockTake : PdaBase, ITransient
{
    #region 字段定义
    private readonly PdaBaseRepos _repos;                                       //仓储聚合
    private readonly ILogger _logger;                                           //日志
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖
    /// </summary>
    public PdaStockTake(
        ILoggerFactory loggerFactory,
        ISqlSugarClient sqlSugarClient,
        PdaBaseRepos repos) : base(sqlSugarClient)
    {
        _repos = repos;
        _logger = loggerFactory.CreateLogger(CommonConst.PdaStockTake);
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 盘点单列表查询
    /// <para>对应 JC35 接口：【GetWmsStockCheckList】</para>
    /// </summary>
    /// <param name="input">盘点单列表查询的请求参数，包含仓库ID等过滤条件</param>
    /// <returns>盘点单列表查询结果，包含成功状态、消息和盘点单列表</returns>
    /// <exception cref="Exception">当参数验证失败或查询过程中发生错误时抛出异常</exception>
    /// <remarks>
    /// 查询条件：
    /// <list type="bullet">
    /// <item><description>查询执行状态为2的盘点通知（已启动盘点）</description></item>
    /// <item><description>可按仓库ID进行筛选</description></item>
    /// <item><description>按盘点日期降序排列，最新的盘点单在前</description></item>
    /// <item><description>返回字段包含盘点单号、仓库ID、执行状态、盘点日期等</description></item>
    /// </list>
    /// 业务场景：
    /// <list type="number">
    /// <item><description>PDA端选择要进行的盘点单</description></item>
    /// <item><description>查看当前可用的盘点任务</description></item>
    /// <item><description>支持按仓库筛选盘点单</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaLegacyResult<List<PdaStockCheckOrderItem>>> GetStockCheckListAsync(PdaStockCheckListInput input)
    {
        // 验证输入参数完整性
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        var result = CreateLegacyResult<List<PdaStockCheckOrderItem>>("查询失败", new List<PdaStockCheckOrderItem>());
        try
        {
            // 构建查询条件：查询执行状态为2（已启动盘点）且未删除的盘点通知
            var query = _repos.StockCheckNotify.AsQueryable().Where(x => x.IsDelete == false).Where(x => x.ExecuteFlag == 2);
            // 【可选条件：仓库ID】如果提供了仓库ID，按仓库筛选
            if (!string.IsNullOrWhiteSpace(input.WarehouseId))
            {
                var warehouseId = input.WarehouseId.Trim();
                query = query.Where(x => x.WarehouseId == warehouseId);
            }
            // 按盘点日期倒序查询，最新的盘点单在前
            var rawList = await query.OrderBy(x => x.CheckDate, OrderByType.Desc).ToListAsync();
            // 将实体转换为PDA展示对象
            var mappedList = rawList.Select(MapNotifyToOrderItem).ToList();
            result = CreateLegacyResultSuccess(mappedList, "盘点单列表");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取盘点单列表失败：{@Input}", input);
            result = CreateLegacyResult<List<PdaStockCheckOrderItem>>(ex.Message, []);
        }
        return result;
    }
    /// <summary>
    /// 盘点箱码列表查询
    /// <para>对应 JC35 接口：【GetStockCheckInfo】</para>
    /// </summary>
    /// <param name="input">盘点箱码列表查询的请求参数，包含盘点单号、托盘编码、状态等</param>
    /// <returns>盘点箱码列表查询结果，包含成功状态、消息和箱码明细列表</returns>
    /// <exception cref="Exception">当参数验证失败、盘点单不存在或查询过程中发生错误时抛出异常</exception>
    /// <remarks>
    /// 功能特性：
    /// <list type="bullet">
    /// <item><description>支持两种模式：查询模式和扫描确认模式</description></item>
    /// <item><description>查询模式（status≠1）：返回待盘点的箱码列表</description></item>
    /// <item><description>扫描确认模式（status=1）：将指定箱码标记为已扫描，然后返回剩余待盘点箱码</description></item>
    /// <item><description>只返回Status=0且State=0的记录（待盘点且待提交）</description></item>
    /// </list>
    /// 业务流程：
    /// <list type="number">
    /// <item><description>验证盘点单号有效性</description></item>
    /// <item><description>获取对应的盘点明细记录</description></item>
    /// <item><description>如果是扫描模式，标记指定箱码状态</description></item>
    /// <item><description>查询剩余待盘点的箱码列表</description></item>
    /// <item><description>返回格式化的箱码信息</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaLegacyResult<List<PdaStockCheckBoxItem>>> GetStockCheckInfoAsync(PdaStockCheckInfoInput input)
    {
        var result = CreateLegacyResult("查询失败", new List<PdaStockCheckBoxItem>());
        try
        {
            // 【第一步：验证盘点单有效性】根据盘点单号解析盘点通知实体
            var notify = await ResolveStockCheckNotifyAsync(input.CheckBillCode.Trim());
            if (notify == null)
                throw Oops.Bah("获取箱码明细失败！");
            var stockCode = input.StockCode.Trim();    //托盘编码
            // 【第二步：获取盘点明细】查询指定托盘的盘点明细记录
            var detail = await _repos.StockCheckNotifyDetail.GetFirstAsync(x => x.IsDelete == false && x.CheckBillCode == notify.CheckBillCode && x.StockCode == stockCode);
            if (detail == null)
                throw Oops.Bah("获取箱码明细失败！");
            // 【第三步：扫描确认处理】如果status="1"，标记指定箱码为已扫描（Status=1）
            // 已扫描的箱码在后续查询中不会返回，实现扫描即移除的效果
            if (string.Equals(input.Status, "1", StringComparison.OrdinalIgnoreCase))
            {
                await MarkStockCheckStatusAsync(detail.Id, stockCode, input.BoxCode);
            }
            // 【第四步：查询待盘点箱码】返回Status=0（待扫描）且State=0（待提交）的箱码列表
            // 注意：如果上一步标记了箱码，该箱码Status已为1，不会出现在结果中
            var query = _repos.StockCheckInfo.AsQueryable()
                .Where(x => x.IsDelete == false
                    && x.StockCheckId == detail.Id
                    && x.StockCode == stockCode
                    //&& x.Status == 0
                    //&& x.State == 0
                    );
            // 根据是否有箱码添加查询条件
            if (!string.IsNullOrWhiteSpace(input.BoxCode))
            {
                // 有箱码场景：按箱码精确匹配
                var normalizedBoxCode = input.BoxCode.Trim();
                query = query.Where(x => x.BoxCode == normalizedBoxCode);
            } 
            var infoList = await query.OrderBy(x => x.Id, OrderByType.Asc).ToListAsync();
            // 将实体转换为PDA展示对象
            var mappedList = infoList.Select(MapStockCheckInfo).ToList();
            result = CreateLegacyResultSuccess(mappedList, "PDA盘点箱码列表");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询盘点箱码列表失败：{@Input}", input);
            result = CreateLegacyResult(ex.Message, new List<PdaStockCheckBoxItem>());
        }
        return result;
    }
    /// <summary>
    /// 盘点结果提交
    /// <para>对应 JC35 接口：【UpdateStockCheckInfo】</para>
    /// </summary>
    /// <param name="input">盘点结果提交的请求参数，包含盘点单号、托盘编码、箱码明细列表等</param>
    /// <returns>盘点结果提交操作结果，包含成功状态和消息</returns>
    /// <exception cref="Exception">当参数验证失败、盘点单不存在、数据不一致或提交失败时抛出异常</exception>
    /// <remarks>
    /// 业务流程：
    /// <list type="number">
    /// <item><description>验证输入参数的完整性</description></item>
    /// <item><description>验证盘点单和明细的存在性</description></item>
    /// <item><description>在事务中处理所有数据更新</description></item>
    /// <item><description>更新每个箱码的实际盘点数量和状态</description></item>
    /// <item><description>计算盘点结果（相等/少盘/多盘）</description></item>
    /// <item><description>更新托盘盘点的实际总数量</description></item>
    /// <item><description>推进盘点单的整体执行状态</description></item>
    /// </list>
    /// 盘点结果编码：
    /// <list type="bullet">
    /// <item><description>0：数量相等（盘平）</description></item>
    /// <item><description>1：实际数量少于计划数量（少盘）</description></item>
    /// <item><description>2：实际数量多于计划数量（多盘）</description></item>
    /// </list>
    /// 数据更新：
    /// <list type="bullet">
    /// <item><description>WmsStockCheckInfo：更新实际数量、状态、盘点结果</description></item>
    /// <item><description>WmsStockCheckNotifyDetail：更新实际总数量、执行状态</description></item>
    /// <item><description>WmsStockCheckNotify：当所有明细完成时更新执行状态</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> UpdateStockCheckInfoAsync(PdaStockCheckUpdateInput input)
    {
        // 验证输入参数完整性
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.BillCode))
            throw Oops.Bah("盘点单据标识不能为空！");
        if (string.IsNullOrWhiteSpace(input.StockCode))
            throw Oops.Bah("托盘编码不能为空！");
        if (input.Options == null || input.Options.Count == 0)
            throw Oops.Bah("盘点箱码明细不能为空！");
        var result = CreateActionResult("提交失败");
        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                // 【第一步：获取盘点单据信息】验证盘点单和明细的存在性
                var notify = await ResolveStockCheckNotifyAsync(input.BillCode.Trim()) ?? throw Oops.Bah("获取盘点单据失败！");
                var stockCode = input.StockCode.Trim();    //托盘编码
                var detail = await _repos.StockCheckNotifyDetail.GetFirstAsync(x => x.IsDelete == false && x.CheckBillCode == notify.CheckBillCode && x.StockCode == stockCode) ?? throw Oops.Bah("获取盘点单据明细失败！");
                var now = DateTime.Now;    //当前时间
                // 【第二步：获取所有箱码记录】查询该托盘下的所有盘点箱码信息
                var infoList = await _repos.StockCheckInfo.AsQueryable().Where(x => x.IsDelete == false && x.StockCheckId == detail.Id && x.StockCode == stockCode).ToListAsync();
                if (infoList.Count == 0)
                    throw Oops.Bah("未找到盘点箱码信息！");
                decimal totalRealQuantity = 0;    //实际总数量
                var updates = new List<WmsStockCheckInfo>();    //待更新列表
                // 【第三步：处理每个箱码的盘点结果】验证并计算盘点结果
                foreach (var option in input.Options)
                {
                    // 验证明细托盘编码与提交托盘一致性
                    var optionStockCode = string.IsNullOrWhiteSpace(option.StockCode) ? stockCode : option.StockCode.Trim();
                    if (!string.Equals(optionStockCode, stockCode, StringComparison.OrdinalIgnoreCase))
                        throw Oops.Bah("明细托盘编码与提交托盘不一致！");
                    // 根据托盘和箱码定位盘点记录
                    var target = FindStockCheckInfo(infoList, stockCode, option.BoxCode) ?? throw Oops.Bah("获取盘点箱码明细失败！");
                    //var realQty = option.RealQuantity ?? 0;    //实际盘点数量 原JC35

                    #region JC44改为客户不操作 就是保持原库存数量
                    decimal realQty = 0;
                    if (target.Status != 1)
                        realQty = option.RealQuantity ?? target.Qty.Value;
                    else
                        realQty = option.RealQuantity ?? 0;
                    #endregion

                    if (realQty < 0)
                        throw Oops.Bah("实际数量不能小于 0！");
                    totalRealQuantity += realQty;    //累加实际总数量
                    target.RealQuantity = realQty;    //更新实际数量
                    target.State = 1;    //状态：1=已提交
                    target.CheckResult = EvaluateCheckResult(target.Qty, realQty);    //评估盘点结果：0=盘平，1=少盘，2=多盘
                    target.UpdateTime = now;
                    updates.Add(target);
                }
                // 【第四步：批量更新箱码盘点结果】提交所有箱码的盘点信息
                if (updates.Count > 0)
                {
                    await _repos.StockCheckInfo.AsUpdateable(updates)
                        .UpdateColumns(x => new { x.RealQuantity, x.State, x.CheckResult, x.UpdateTime })
                        .ExecuteCommandAsync();
                }
                // 【第五步：更新盘点明细汇总】计算托盘级别的盘点结果
                detail.RealQuantity = totalRealQuantity;    //托盘实际总数量
                detail.CheckResult = EvaluateCheckResult(detail.StockQuantity, totalRealQuantity);    //评估托盘盘点结果
                detail.ExecuteFlag = 2;    //执行状态：2=已完成
                detail.UpdateTime = now;
                await _repos.StockCheckNotifyDetail.AsUpdateable(detail)
                    .UpdateColumns(x => new { x.RealQuantity, x.CheckResult, x.ExecuteFlag, x.UpdateTime })
                    .ExecuteCommandAsync();
                // 【第六步：检查整单完成情况】如果所有明细都已完成，更新主单状态
                var totalCount = await _repos.StockCheckNotifyDetail.AsQueryable()
                    .Where(x => x.IsDelete == false && x.CheckBillCode == notify.CheckBillCode)
                    .CountAsync();    //明细总数
                var completedCount = await _repos.StockCheckNotifyDetail.AsQueryable().Where(x => x.IsDelete == false && x.CheckBillCode == notify.CheckBillCode && x.ExecuteFlag == 2).CountAsync();    //已完成明细数
                // 所有明细都已完成，标记主单为完成状态
                if (totalCount > 0 && completedCount == totalCount)
                {
                    notify.ExecuteFlag = 2;    //执行状态：2=已完成
                    notify.UpdateTime = now;
                    await _repos.StockCheckNotify.AsUpdateable(notify).UpdateColumns(x => new { x.ExecuteFlag, x.UpdateTime })
                        .ExecuteCommandAsync();
                }
            }, "提交盘点结果失败");
            result = CreateActionResultSuccess("提交成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "提交盘点结果失败：{@Input}", input);
            result = CreateActionResult(ex.Message);
        }
        return result;
    }
    /// <summary>
    /// 将盘点通知实体映射为 PDA 列表项，保持 JC35 字段含义   
    /// </summary>
    /// <param name="notify">盘点通知实体</param>
    /// <returns>PDA盘点单列表项</returns>
    private static PdaStockCheckOrderItem MapNotifyToOrderItem(WmsStockCheckNotify notify)
    {
        return new PdaStockCheckOrderItem    //创建PDA盘点单列表项 结果
        {
            Id = notify.Id.ToString(),    //ID
            CheckBillCode = notify.CheckBillCode,    //盘点单据标识
            WarehouseId = notify.WarehouseId,    //仓库ID
            ExecuteFlag = notify.ExecuteFlag ?? 0,    //执行状态
            CheckDate = notify.CheckDate,    //盘点日期
            Remark = notify.CheckRemark    //备注
        };
    }
    /// <summary>
    /// 将盘点箱码实体映射为 PDA 展示项
    /// </summary>
    /// <param name="info">盘点箱码实体</param>
    /// <returns>PDA盘点箱码列表项</returns>
    private static PdaStockCheckBoxItem MapStockCheckInfo(WmsStockCheckInfo info)
    {
        return new PdaStockCheckBoxItem    //创建PDA盘点箱码列表项结果  
        {
            Id = info.Id.ToString(),    //ID
            BoxCode = info.BoxCode,    //箱码
            StockCode = info.StockCode,    //托盘编码
            MaterialId = info.MaterialId?.ToString(),    //物料ID
            MaterialCode = info.MaterialCode,    //物料编码
            MaterialName = info.MaterialName,    //物料名称
            LotNo = info.LotNo,    //批号
            Qty = info.Qty,    //数量
            RealQuantity = info.RealQuantity,    //实际数量
            ProductionDate = info.ProductionDate,    //生产日期
            ValidateDay = info.ValidateDay,    //有效期
            InspectionStatus = info.InspectionStatus,    //检验状态
            ExtractStatus = info.ExtractStatus,    //提取状态
            SamplingDate = info.SamplingDate,    //采样日期
            StaffCode = info.StaffCode,    //员工编码
            StaffName = info.StaffName,    //员工名称
            StockCheckId = info.StockCheckId.ToString(),    //盘点ID
            Status = info.Status,    //状态
            State = info.State,    //状态
            CheckResult = info.CheckResult,    //盘点结果
            Weight = info.Weight,    //重量
            PickingSlurry = info.PickingSlurry,    //刮痧
            CustomerId = info.CustomerId,    //客户ID
            OddMarking = info.OddMarking,    //奇异标记
            OutQty = info.OutQty    //出库数量
        };
    }
    /// <summary>
    /// 根据盘点单号或数据库主键定位通知单
    /// </summary>
    /// <param name="identifier">盘点单号或数据库主键，支持数字ID或字符串单号</param>
    /// <returns>盘点通知实体，未找到时返回null</returns>
    /// <remarks>
    /// 解析策略：
    /// <list type="bullet">
    /// <item><description>首先尝试将输入解析为长整型ID，如果成功则按ID查询</description></item>
    /// <item><description>如果ID查询无结果，则按盘点单号（CheckBillCode）查询</description></item>
    /// <item><description>只查询未删除的记录（IsDelete == 0）</description></item>
    /// </list>
    /// 使用场景：
    /// <list type="number">
    /// <item><description>PDA扫描盘点单号时自动识别ID或单号</description></item>
    /// <item><description>提供灵活的查询方式，兼容不同输入格式</description></item>
    /// <item><description>优先使用ID查询提高性能</description></item>
    /// </list>
    /// </remarks>
    private async Task<WmsStockCheckNotify> ResolveStockCheckNotifyAsync(string identifier)
    {
        // 尝试将输入解析为长整型ID
        if (long.TryParse(identifier, out var idValue))
        {
            // 按ID查询盘点通知
            var notifyById = await _repos.StockCheckNotify.GetFirstAsync(x => x.Id == idValue && x.IsDelete == false);
            if (notifyById != null)
            {
                return notifyById;
            }
        }
        // ID查询无结果，按盘点单号（CheckBillCode）查询
        return await _repos.StockCheckNotify.GetFirstAsync(x => x.CheckBillCode == identifier && x.IsDelete == false);
    }
    /// <summary>
    /// PDA 标记箱码已扫描（Status=1）
    /// </summary>
    /// <param name="detailId">盘点单据明细ID，用于关联盘点明细</param>
    /// <param name="stockCode">托盘编码，用于定位托盘</param>
    /// <param name="boxCode">箱码，可为空用于无箱码托盘的盘点</param>
    /// <exception cref="Exception">当箱码明细不存在或更新失败时抛出异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>将盘点箱码的Status从0更新为1，表示已扫描确认</description></item>
    /// <item><description>支持有箱码和无箱码两种盘点模式</description></item>
    /// <item><description>已扫描的箱码不会出现在后续的待盘点列表中</description></item>
    /// </list>
    /// 查询逻辑：
    /// <list type="number">
    /// <item><description>根据盘点明细ID和托盘编码定位记录</description></item>
    /// <item><description>如果有箱码，则按箱码精确匹配</description></item>
    /// <item><description>如果无箱码（boxCode为空），则查询BoxCode为null或空字符串的记录</description></item>
    /// </list>
    /// 业务效果：
    /// <list type="bullet">
    /// <item><description>标记为已扫描的箱码在GetStockCheckInfo接口中不会再次返回</description></item>
    /// <item><description>防止重复扫描同一箱码</description></item>
    /// <item><description>记录扫描时间用于操作审计</description></item>
    /// </list>
    /// </remarks>
    private async Task MarkStockCheckStatusAsync(long detailId, string stockCode, string boxCode)
    {
        // 构建基础查询条件：根据盘点明细ID和托盘编码
        var query = _repos.StockCheckInfo.AsQueryable()
            .Where(x => x.IsDelete == false && x.StockCheckId == detailId && x.StockCode == stockCode);
        // 根据是否有箱码添加查询条件
        if (!string.IsNullOrWhiteSpace(boxCode))
        {
            // 有箱码场景：按箱码精确匹配
            var normalizedBoxCode = boxCode.Trim();
            query = query.Where(x => x.BoxCode == normalizedBoxCode);
        }
        else
        {
            // 无箱码场景：查询BoxCode为null或空字符串的记录
            query = query.Where(x => x.BoxCode == null || x.BoxCode == string.Empty);
        }
        // 获取盘点箱码记录
        var info = await query.FirstAsync() ?? throw Oops.Bah("获取箱码明细失败！");
        // 标记为已扫描（Status=1表示已扫描确认）
        info.Status = 1;
        info.UpdateTime = DateTime.Now;
        await _repos.StockCheckInfo.AsUpdateable(info)
            .UpdateColumns(x => new { x.Status, x.UpdateTime })
            .ExecuteCommandAsync();
    }
    /// <summary>
    /// 更新单个箱码实际数量
    /// </summary>
    /// <param name="input">更新单个箱码实际数量的请求参数，包含盘点单号、托盘编码、箱码、实际数量等</param>
    /// <returns>更新操作结果，包含成功状态和消息</returns>
    /// <exception cref="Exception">当参数验证失败、盘点单不存在、箱码明细不存在或更新失败时抛出异常</exception>
    /// <remarks>
    /// 业务流程：
    /// <list type="number">
    /// <item><description>验证输入参数的完整性</description></item>
    /// <item><description>验证盘点单和明细的存在性</description></item>
    /// <item><description>在事务中处理数据更新</description></item>
    /// <item><description>更新指定箱码的实际盘点数量和状态</description></item>
    /// <item><description>计算盘点结果（相等/少盘/多盘）</description></item>
    /// <item><description>将State业务状态字段改为1（已提交状态）</description></item>
    /// </list>
    /// 数据更新：
    /// <list type="bullet">
    /// <item><description>WmsStockCheckInfo：更新实际数量、状态、盘点结果、State</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> UpdateBoxRealQuantityAsync(PdaUpdateBoxRealQuantityInput input)
    {
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.CheckBillCode))
            throw Oops.Bah("盘点单据标识不能为空！");
        if (string.IsNullOrWhiteSpace(input.StockCode))
            throw Oops.Bah("托盘编码不能为空！");
        if (string.IsNullOrWhiteSpace(input.BoxCode))
            throw Oops.Bah("箱码不能为空！");

        var result = CreateActionResult("更新失败");
        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                var notify = await ResolveStockCheckNotifyAsync(input.CheckBillCode.Trim()) ?? throw Oops.Bah("获取盘点单据失败！");
                var stockCode = input.StockCode.Trim();
                var detail = await _repos.StockCheckNotifyDetail.GetFirstAsync(x => x.IsDelete == false && x.CheckBillCode == notify.CheckBillCode && x.StockCode == stockCode) ?? throw Oops.Bah("获取盘点单据明细失败！");
                var info = await _repos.StockCheckInfo.GetFirstAsync(x => x.IsDelete == false && x.StockCheckId == detail.Id && x.StockCode == stockCode && x.BoxCode == input.BoxCode.Trim()) ?? throw Oops.Bah("获取箱码明细失败！");
                var now = DateTime.Now;
                var realQty = input.RealQuantity ?? 0;
                if (realQty < 0)
                    throw Oops.Bah("实际数量不能小于 0！");
                info.RealQuantity = realQty;
                info.Status = 1;
                info.CheckResult = EvaluateCheckResult(info.Qty, realQty);
                info.UpdateTime = now;
                await _repos.StockCheckInfo.AsUpdateable(info)
                    .UpdateColumns(x => new { x.RealQuantity, x.Status, x.CheckResult, x.UpdateTime })
                    .ExecuteCommandAsync();
            }, "更新箱码实际数量失败");
            result = CreateActionResultSuccess("更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新箱码实际数量失败：{@Input}", input);
            result = CreateActionResult(ex.Message);
        }
        return result;
    }
    /// <summary>
    /// 在盘点箱码集合中查找指定托盘/箱码记录
    /// </summary>
    /// <param name="infos">盘点箱码集合</param>
    /// <param name="stockCode">托盘编码</param>
    /// <param name="boxCode">箱码</param>
    /// <returns>盘点箱码实体</returns>
    private static WmsStockCheckInfo FindStockCheckInfo(IEnumerable<WmsStockCheckInfo> infos, string stockCode, string boxCode)
    {
        var normalizedStockCode = stockCode?.Trim();    //托盘编码
        var normalizedBoxCode = boxCode?.Trim();    //箱码
        return string.IsNullOrWhiteSpace(normalizedBoxCode)
            ? infos.FirstOrDefault(x => x.StockCode == normalizedStockCode && string.IsNullOrWhiteSpace(x.BoxCode))    //查询盘点箱码信息
            : infos.FirstOrDefault(x => x.StockCode == normalizedStockCode && string.Equals(x.BoxCode, normalizedBoxCode, StringComparison.OrdinalIgnoreCase));
    }
    /// <summary>
    /// 评估盘点结果：0=相等，1=少盘，2=多盘
    /// </summary>
    /// <param name="planned">计划数量（系统库存数量），可为null，null时按0处理</param>
    /// <param name="actual">实际数量（盘点数量），可为null，null时按0处理</param>
    /// <returns>盘点结果编码，0=盘平，1=少盘，2=多盘</returns>
    /// <remarks>
    /// 评估规则：
    /// <list type="bullet">
    /// <item><description>0：实际数量等于计划数量（盘平）</description></item>
    /// <item><description>1：实际数量小于计划数量（少盘/盘亏）</description></item>
    /// <item><description>2：实际数量大于计划数量（多盘/盘盈）</description></item>
    /// </list>
    /// 处理逻辑：
    /// <list type="number">
    /// <item><description>将null值转换为0进行计算</description></item>
    /// <item><description>使用decimal.Compare进行精确的数值比较</description></item>
    /// <item><description>避免浮点数精度问题</description></item>
    /// </list>
    /// 应用场景：
    /// <list type="bullet">
    /// <item><description>盘点结果提交时自动计算差异类型</description></item>
    /// <item><description>生成盘点差异报告</description></item>
    /// <item><description>后续库存调整的依据</description></item>
    /// </list>
    /// </remarks>
    private static int EvaluateCheckResult(decimal? planned, decimal? actual)
    {
        var plannedQty = planned ?? 0;    //计划数量
        var actualQty = actual ?? 0;    //实际数量
        var compare = decimal.Compare(plannedQty, actualQty);    //比较计划数量和实际数量
        if (compare == 0)    //判断计划数量是否等于实际数量
            return 0;    //相等
        return compare > 0 ? 1 : 2;    //大于0则返回1，小于0则返回2
    }
    #endregion

    #region 托盘变更相关接口
    /// <summary>
    /// 托盘验证
    /// <para>对应 JC35 接口：【IsEnableOkStockCode】</para>
    /// </summary>
    /// <param name="input">托盘验证的请求参数，包含托盘编码</param>
    /// <returns>托盘验证结果，包含成功状态、消息和验证结果</returns>
    /// <exception cref="Exception">当参数验证失败时抛出异常</exception>
    /// <remarks>
    /// 验证内容：
    /// <list type="bullet">
    /// <item><description>托盘编码存在性验证</description></item>
    /// <item><description>托盘编码唯一性验证（防止重复）</description></item>
    /// <item><description>托盘状态有效性验证</description></item>
    /// </list>
    /// 验证结果：
    /// <list type="number">
    /// <item><description>成功：托盘存在且唯一，返回"托盘可用！"</description></item>
    /// <item><description>重复错误：存在多个相同托盘编码，返回"存在重复托盘号,请检查!"</description></item>
    /// <item><description>不存在错误：托盘编码不存在，返回"托盘号不存在!"</description></item>
    /// </list>
    /// 应用场景：
    /// <list type="bullet">
    /// <item><description>托盘变更前验证新托盘的可用性</description></item>
    /// <item><description>入库前检查托盘编码的有效性</description></item>
    /// <item><description>避免托盘编码重复导致的业务异常</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaLegacyResult<object>> ValidateTrayAsync(PdaTrayValidateInput input)
    {
        // 验证输入参数完整性
        if (input == null || string.IsNullOrWhiteSpace(input.StockCode))
            throw Oops.Bah("托盘编码不能为空！");
        var result = CreateLegacyResult<object>("托盘可用!", "");
        try
        {
            var stockCode = input.StockCode.Trim();
            // 查询托盘编码对应的托盘主数据
            var models = await _repos.SysStockCode.GetListAsync(m => m.IsDelete == false && m.StockCode == stockCode);
            // 验证托盘唯一性：存在多个相同托盘编码
            if (models.Count > 1)
            {
                result = CreateLegacyResult<object>("存在重复托盘号,请检查!", "");
                return result;
            }
            // 验证托盘存在性：托盘编码不存在
            if (models.Count <= 0)
            {
                result = CreateLegacyResult<object>("托盘号不存在!", "");
                return result;
            }
            // 验证通过：托盘存在且唯一
            result = CreateLegacyResultSuccess<object>("", "托盘可用!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证托盘失败：{@Input}", input);
            result = CreateLegacyResult<object>(ex.Message, "");
        }
        return result;
    }
    /// <summary>
    /// 根据托盘号获取物料信息
    /// <para>对应 JC35 接口：【GetMaterialInfoByStockCode】</para>
    /// </summary>
    /// <param name="input">获取物料信息的请求参数，包含托盘编码</param>
    /// <returns>物料信息列表查询结果，包含成功状态、消息和物料信息列表</returns>
    /// <exception cref="Exception">当参数验证失败或查询过程中发生错误时抛出异常</exception>
    /// <remarks>
    /// 查询逻辑：
    /// <list type="bullet">
    /// <item><description>根据托盘编码查询库存托盘信息</description></item>
    /// <item><description>提取托盘中的物料ID列表</description></item>
    /// <item><description>关联查询物料基础信息表</description></item>
    /// <item><description>返回物料编码、名称、批号、库存数量、锁定数量等</description></item>
    /// </list>
    /// 返回字段：
    /// <list type="number">
    /// <item><description>MaterialId：物料ID</description></item>
    /// <item><description>LotNo：批次号</description></item>
    /// <item><description>LockQuantity：锁定数量</description></item>
    /// <item><description>StockQuantity：库存数量</description></item>
    /// <item><description>MaterialName：物料名称</description></item>
    /// <item><description>MaterialCode：物料编码</description></item>
    /// </list>
    /// 应用场景：
    /// <list type="bullet">
    /// <item><description>托盘变更时显示当前托盘的物料信息</description></item>
    /// <item><description>盘点时确认托盘上的物料详情</description></item>
    /// <item><description>库存查询和物料追踪</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaLegacyResult<List<PdaMaterialInfoItem>>> GetMaterialInfoByStockCodeAsync(PdaMaterialInfoByStockCodeInput input)
    {
        // 验证输入参数完整性
        if (input == null || string.IsNullOrWhiteSpace(input.StockCode))
            throw Oops.Bah("托盘号不能为空");
        var result = CreateLegacyResult<List<PdaMaterialInfoItem>>("箱码获取失败！", new List<PdaMaterialInfoItem>());
        try
        {
            var stockCode = input.StockCode.Trim();    //托盘编码
            // 【第一步：查询库存托盘】获取指定托盘的所有库存托盘记录
            var trayList = await _repos.StockTray.GetListAsync(t => t.StockCode == stockCode);
            // 【第二步：提取物料ID集合】去重后收集所有物料ID
            var materialIds = trayList.Where(t => !string.IsNullOrEmpty(t.MaterialId)).Select(t => t.MaterialId).Distinct().ToList();
            // 【第三步：查询物料基础信息】批量获取物料详情
            var materials = materialIds.Count > 0
                ? await _repos.Material.GetListAsync(m => materialIds.Contains(m.Id.ToString()))
                : [];
            // 构建物料ID到物料实体的映射字典（用于快速查找）
            var materialDict = materials.ToDictionary(m => m.Id.ToString(), m => m);
            // 【第四步：构建返回列表】合并托盘和物料信息
            var list = trayList.Select(tray => new PdaMaterialInfoItem
            {
                MaterialId = tray.MaterialId,    //物料ID
                LotNo = tray.LotNo,    //批次号
                LockQuantity = tray.LockQuantity,    //锁定数量
                StockQuantity = tray.StockQuantity,    //库存数量
                MaterialName = materialDict.ContainsKey(tray.MaterialId ?? "") ? materialDict[tray.MaterialId].MaterialName : null,    //物料名称
                MaterialCode = materialDict.ContainsKey(tray.MaterialId ?? "") ? materialDict[tray.MaterialId].MaterialCode : null    //物料编码
            }).ToList();
            // 查询成功且有数据时返回
            if (list != null && list.Count > 0)
            {
                result = CreateLegacyResultSuccess(list, "");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据托盘号获取物料信息失败：{@Input}", input);
            result = CreateLegacyResult<List<PdaMaterialInfoItem>>(ex.Message, new List<PdaMaterialInfoItem>());
        }
        return result;
    }
    /// <summary>
    /// 无箱码托盘变更
    /// <para>对应 JC35 接口：【SaveUnbindWithNoBoxCode】</para>
    /// </summary>
    /// <param name="input">无箱码托盘变更的请求参数，包含源托盘、目标托盘、物料信息、转移数量等</param>
    /// <returns>托盘变更操作结果，包含成功状态、消息和操作结果</returns>
    /// <exception cref="Exception">当参数验证失败、托盘不存在、库存不足或变更失败时抛出异常</exception>
    /// <remarks>
    /// 核心功能：
    /// <list type="bullet">
    /// <item><description>将库存从一个托盘转移到另一个托盘</description></item>
    /// <item><description>支持新建目标托盘或使用已有托盘</description></item>
    /// <item><description>自动处理箱码信息的复制或更新</description></item>
    /// <item><description>维护出库订单状态一致性</description></item>
    /// <item><description>清理空托盘的占用状态</description></item>
    /// </list>
    /// 业务流程：
    /// <list type="number">
    /// <item><description>验证输入参数的完整性和有效性</description></item>
    /// <item><description>验证源托盘的存在性和库存充足性</description></item>
    /// <item><description>获取或创建目标托盘（确保为空托盘）</description></item>
    /// <item><description>执行库存转移（更新数量和箱码信息）</description></item>
    /// <item><description>更新相关出库订单状态</description></item>
    /// <item><description>清理空源托盘的占用状态和数据</description></item>
    /// <item><description>创建托盘变更记录用于审计追踪</description></item>
    /// </list>
    /// 数据更新：
    /// <list type="bullet">
    /// <item><description>WmsStockTray：更新托盘库存数量</description></item>
    /// <item><description>WmsStockInfo：更新箱码数量信息</description></item>
    /// <item><description>WmsSysStockCode：更新空托盘状态</description></item>
    /// <item><description>WmsExportOrder：更新出库订单执行状态</description></item>
    /// <item><description>WmsStockUnbind：记录变更历史</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaLegacyResult<object>> SaveTrayChangeAsync(PdaTrayChangeInput input)
    {
        // 【第一步：输入验证】验证源托盘、目标托盘、物料等必填参数
        ValidateTrayChangeInput(input);
        var result = CreateLegacyResult<object>("换绑失败！", "");
        try
        {
            // 【事务执行】在事务中完成托盘变更的所有操作，确保数据一致性
            await ExecuteInTransactionAsync(async () =>
            {
                // 【第二步：验证源托盘】查询源托盘并验证库存充足性
                var sourceTray = await GetAndValidateSourceTrayAsync(input);
                // 【第三步：准备目标托盘】查询或新建目标托盘（必须是空托盘或同物料托盘）
                var targetTray = await GetOrCreateTargetTrayAsync(input, sourceTray);
                // 【第四步：执行库存转移】从源托盘扣减数量，并更新箱码信息
                await TransferStockBetweenTraysAsync(sourceTray, input.ExportNum);
                // 【第五步：更新出库订单状态】将相关出库订单标记为已完成（ExportExecuteFlag=2）
                await UpdateExportOrderStatusAsync(input);
                // 【第六步：清理空源托盘】如果源托盘无库存且无锁定数量，释放托盘并删除相关数据
                await HandleEmptySourceTrayCleanupAsync(sourceTray);
                // 【第七步：创建变更记录】记录托盘变更历史，用于审计追踪
                await CreateStockUnbindRecordAsync(input);
            }, "无箱码托盘变更失败");
            // 所有步骤成功完成
            result = CreateLegacyResultSuccess<object>("", "换绑成功！");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "无箱码托盘变更失败：{@Input}", input);
            result = CreateLegacyResult<object>(ex.Message, "");
        }
        return result;
    }
    #endregion

    #region 托盘变更辅助方法
    /// <summary>
    /// 验证托盘变更输入参数
    /// </summary>
    /// <param name="input">托盘变更输入参数</param>
    /// <exception cref="Exception">当参数验证失败时抛出异常</exception>
    /// <remarks>
    /// 验证项目：
    /// <list type="bullet">
    /// <item><description>请求参数不能为空</description></item>
    /// <item><description>源托盘编码不能为空</description></item>
    /// <item><description>物料ID不能为空</description></item>
    /// <item><description>目标托盘编码不能为空</description></item>
    /// <item><description>批号不能为空</description></item>
    /// <item><description>源托盘和目标托盘不能相同</description></item>
    /// </list>
    /// </remarks>
    private static void ValidateTrayChangeInput(PdaTrayChangeInput input)
    {
        // 验证请求参数不为空
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        // 验证必填字段：源托盘、目标托盘、物料ID、批号
        if (string.IsNullOrWhiteSpace(input.StockCode) || string.IsNullOrWhiteSpace(input.MaterialId) || string.IsNullOrWhiteSpace(input.StockCodeNew) || string.IsNullOrWhiteSpace(input.LotNo))
            throw Oops.Bah("新旧托盘号及物料都不能为空");
        // 验证源托盘和目标托盘不能相同
        if (input.StockCode == input.StockCodeNew)
            throw Oops.Bah("新旧托盘号不能为相同");
    }
    /// <summary>
    /// 获取并验证源托盘信息
    /// </summary>
    /// <param name="input">托盘变更输入参数</param>
    /// <returns>源托盘实体</returns>
    private async Task<WmsStockTray> GetAndValidateSourceTrayAsync(PdaTrayChangeInput input)
    {
        // 提取输入参数：托盘编码、物料ID、批号
        var stockCode = input.StockCode.Trim();
        var materialId = input.MaterialId.Trim();
        var lotNo = input.LotNo.Trim();
        // 查询源托盘：根据托盘编码、物料ID、批号三者精确匹配
        var stockTray = await _repos.StockTray.GetFirstAsync(w => w.StockCode == stockCode && w.MaterialId == materialId && w.LotNo == lotNo) ?? throw Oops.Bah("原托盘库存托盘不存在");
        // 验证库存充足性：源托盘的库存数量必须大于等于转移数量
        if ((stockTray.StockQuantity ?? 0) < input.ExportNum)
            throw Oops.Bah("原托盘库存不足");
        return stockTray;
    }
    /// <summary>
    /// 获取或创建目标托盘
    /// </summary>
    /// <param name="input">托盘变更输入参数</param>
    /// <param name="sourceTray">源托盘实体</param>
    /// <returns>目标托盘实体</returns>
    private async Task<WmsStockTray> GetOrCreateTargetTrayAsync(PdaTrayChangeInput input, WmsStockTray sourceTray)
    {
        var stockCodeNew = input.StockCodeNew.Trim();    //目标托盘编码
        // 查询目标托盘：根据目标托盘编码、物料ID、批号匹配
        var targetTray = await _repos.StockTray.GetFirstAsync(w => w.StockCode == stockCodeNew && w.MaterialId == sourceTray.MaterialId && w.LotNo == sourceTray.LotNo);
        // 【情况1：目标托盘不存在】创建新的目标托盘
        if (targetTray == null)
            return await CreateNewTargetTrayAsync(input, sourceTray);
        // 【情况2：目标托盘已存在】更新已存在的目标托盘数量
        await UpdateExistingTargetTrayAsync(targetTray, input.ExportNum);
        return targetTray;
    }
    /// <summary>
    /// 创建新的目标托盘
    /// </summary>
    /// <param name="input">托盘变更输入参数</param>
    /// <param name="sourceTray">源托盘实体</param>
    /// <returns>新建的目标托盘实体</returns>
    private async Task<WmsStockTray> CreateNewTargetTrayAsync(PdaTrayChangeInput input, WmsStockTray sourceTray)
    {
        var stockCodeNew = input.StockCodeNew.Trim();    //新托盘编码
        // 【第一步：验证托盘主数据存在】查询新托盘的主数据信息
        var stockCodeModel = await _repos.SysStockCode.GetFirstAsync(w => w.StockCode == stockCodeNew) ?? throw Oops.Bah("未查询到新托盘号信息");
        // 【第二步：验证托盘为空托盘】新托盘不能存在任何库存信息
        var existingNewTray = await _repos.StockTray.GetFirstAsync(w => w.StockCode == stockCodeNew);
        if (existingNewTray != null)
            throw Oops.Bah("新托盘已存在库存信息，不能为空托盘！");
        // 【第三步：构建新托盘实体】继承源托盘的大部分属性（物料、批次、仓库等）
        var newTray = new WmsStockTray
        {
            StockSlotCode = "",    //库位编码为空（未入库）
            StockCode = stockCodeNew,    //新托盘编码
            StockQuantity = input.ExportNum,    //库存数量=转移数量
            StockDate = DateTime.Now,    //入库日期
            StockStatusFlag = 0,    //托盘状态标志（0=正常）
            LotNo = sourceTray.LotNo,    //继承批次号
            InspectionStatus = sourceTray.InspectionStatus,    //继承检验状态
            LanewayId = sourceTray.LanewayId,    //继承巷道ID
            WarehouseId = sourceTray.WarehouseId,    //继承仓库ID
            SupplierId = sourceTray.SupplierId,    //继承供应商ID
            ManufacturerId = sourceTray.ManufacturerId,    //继承制造商ID
            IsSamolingTray = 0,    //是否抽样托盘（0=否）
            AbnormalStatu = 0,    //冻结状态（0=正常 1 冻结）
            OwnerCode = "",    //货主编码
            ProductionDate = sourceTray.ProductionDate,    //继承生产日期
            ValidateDay = sourceTray.ValidateDay,    //继承有效期天数
            MaterialId = sourceTray.MaterialId,    //继承物料ID
            LockQuantity = 0,    //锁定数量为0
            BoxQuantity = sourceTray.BoxQuantity,    //继承箱数量
            InspectFlag = sourceTray.InspectFlag,    //继承检验标志
            OddMarking = sourceTray.OddMarking,    //继承尾数标记
            VehicleSubId = stockCodeNew    //载具子ID设置为新托盘编码
        };
        // 【第四步：插入新托盘记录】将新托盘保存到数据库
        await _repos.StockTray.InsertAsync(newTray);
        // 【第五步：复制箱码信息】将源托盘的箱码信息复制到新托盘
        await CopyBoxInfosToNewTrayAsync(sourceTray, newTray);
        return newTray;
    }
    /// <summary>
    /// 更新已存在的目标托盘
    /// </summary>
    /// <param name="targetTray">目标托盘实体</param>
    /// <param name="exportNum">转移数量</param>
    private async Task UpdateExistingTargetTrayAsync(WmsStockTray targetTray, decimal exportNum)
    {
        // 累加目标托盘库存数量
        targetTray.StockQuantity = (targetTray.StockQuantity ?? 0) + exportNum;
        // 查询目标托盘下的所有箱码记录（匹配托盘ID、物料ID、批号）
        var newBoxInfos = await _repos.StockInfo.GetListAsync(m => m.TrayId == targetTray.Id.ToString() && m.MaterialId == targetTray.MaterialId && m.LotNo == targetTray.LotNo);
        // 累加每个箱码的数量
        foreach (var boxInfo in newBoxInfos)
            boxInfo.Qty = (boxInfo.Qty ?? 0) + exportNum;
        // 批量更新箱码数量
        await _repos.StockInfo.AsUpdateable(newBoxInfos).ExecuteCommandAsync();
    }
    /// <summary>
    /// 复制箱信息到新托盘
    /// </summary>
    /// <param name="sourceTray">源托盘实体</param>
    /// <param name="targetTray">目标托盘实体</param>
    private async Task CopyBoxInfosToNewTrayAsync(WmsStockTray sourceTray, WmsStockTray targetTray)
    {
        // 查询源托盘的所有箱码记录（匹配托盘ID、物料ID、批号）
        var sourceBoxInfos = await _repos.StockInfo.GetListAsync(m => m.TrayId == sourceTray.Id.ToString() && m.MaterialId == sourceTray.MaterialId && m.LotNo == sourceTray.LotNo);
        // 构建新箱码记录列表：复制源箱码信息，并关联到新托盘
        var newBoxInfos = sourceBoxInfos.Select(item => new WmsStockInfo
        {
            BoxCode = item.BoxCode,    //箱码
            TrayId = targetTray.Id.ToString(),    //关联到新托盘
            MaterialId = item.MaterialId,    //物料ID
            MaterialStatus = item.InspectionStatus?.ToString(),    //物料状态
            Qty = targetTray.StockQuantity ?? 0,    //数量设置为新托盘的库存数量
            LotNo = item.LotNo,    //批次号
            ProductionDate = item.ProductionDate,    //生产日期
            ValidateDay = item.ValidateDay,    //有效期天数
            IsChangeBox = 0,    //是否换箱（0=否）
            IsSamplingBox = item.IsSamplingBox,    //是否抽样箱
            IsFractionBox = item.IsFractionBox,    //是否尾数箱
            Picked = 0,    //是否已拣（0=否）
            BoxQuantity = sourceTray.BoxQuantity ?? 0,    //箱数量
            OddMarking = item.OddMarking?.ToString(),    //尾数标记
            SamplingDate = item.SamplingDate,    //抽样日期
            StaffCode = item.StaffCode,    //员工编码
            StaffName = item.StaffName,    //员工姓名
            Weight = item.Weight,    //重量
            ExtractStatus = item.ExtractStatus,    //提取状态
            InspectionStatus = item.InspectionStatus,    //检验状态
            PickingSlurry = item.PickingSlurry    //拣选料浆
        }).ToList();
        // 批量插入新箱码记录
        if (newBoxInfos.Count > 0)
            await _repos.StockInfo.AsInsertable(newBoxInfos).ExecuteCommandAsync();
    }
    /// <summary>
    /// 执行库存转移
    /// </summary>
    /// <param name="sourceTray">源托盘实体</param>
    /// <param name="exportNum">转移数量</param>
    private async Task TransferStockBetweenTraysAsync(WmsStockTray sourceTray, decimal exportNum)
    {
        // 扣减源托盘库存数量
        sourceTray.StockQuantity = (sourceTray.StockQuantity ?? 0) - exportNum;
        // 查询源托盘下的所有箱码记录
        var sourceBoxInfos = await _repos.StockInfo.GetListAsync(b => b.TrayId == sourceTray.Id.ToString());
        // 扣减每个箱码的数量
        foreach (var boxInfo in sourceBoxInfos)
            boxInfo.Qty = (boxInfo.Qty ?? 0) - exportNum;
        // 更新源托盘库存数量
        await _repos.StockTray.AsUpdateable(sourceTray).ExecuteCommandAsync();
        // 批量更新源托盘箱码数量
        await _repos.StockInfo.AsUpdateable(sourceBoxInfos).ExecuteCommandAsync();
    }
    /// <summary>
    /// 更新出库订单状态
    /// </summary>
    /// <param name="input">托盘变更输入参数</param>
    private async Task UpdateExportOrderStatusAsync(PdaTrayChangeInput input)
    {
        // 解析物料ID：将字符串转换为长整型
        var materialIdLong = long.TryParse(input.MaterialId.Trim(), out var parsedMaterialId) ? parsedMaterialId : (long?)null;
        // 查询相关的出库订单：托盘编码、物料ID、批号匹配，且类型为5（托盘变更类型），状态为待执行或执行中
        var exportOrder = await _repos.ExportOrder.GetFirstAsync(m => m.ExportStockCode == input.StockCode.Trim() && m.ExportMaterialId == materialIdLong && m.ExportLotNo == input.LotNo.Trim() && m.ExportType == 5 && (m.ExportExecuteFlag == 0 || m.ExportExecuteFlag == 1) && m.IsDelete == false);
        // 如果找到匹配的出库订单，更新状态为已完成（ExportExecuteFlag=2表示已完成）
        if (exportOrder != null)
        {
            exportOrder.ExportExecuteFlag = 2;    //标记为已完成
            await _repos.ExportOrder.AsUpdateable(exportOrder).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 处理空源托盘清理
    /// </summary>
    /// <param name="sourceTray">源托盘实体</param>
    private async Task HandleEmptySourceTrayCleanupAsync(WmsStockTray sourceTray)
    {
        // 判断源托盘是否为空托盘：库存数量和锁定数量都为0或null
        if ((!sourceTray.StockQuantity.HasValue || sourceTray.StockQuantity <= 0) && (!sourceTray.LockQuantity.HasValue || sourceTray.LockQuantity <= 0))
        {
            // 【第一步：释放托盘主数据】查询托盘主数据记录
            var oldStockCode = await _repos.SysStockCode.GetFirstAsync(s => s.StockCode == sourceTray.StockCode);
            if (oldStockCode != null)
            {
                oldStockCode.Status = 0;    //将托盘状态设为空闲（Status=0表示空闲可用）
                await _repos.SysStockCode.AsUpdateable(oldStockCode).ExecuteCommandAsync();
            }
            // 【第二步：删除箱码记录】删除源托盘的所有箱码信息
            var boxes = await _repos.StockInfo.GetListAsync(b => b.TrayId == sourceTray.Id.ToString());
            if (boxes.Count > 0)
                await _repos.StockInfo.DeleteAsync(boxes);
            // 【第三步：删除托盘库存记录】删除源托盘的库存记录
            await _repos.StockTray.DeleteAsync(sourceTray);
        }
    }

    /// <summary>
    /// 创建托盘变更记录
    /// </summary>
    /// <param name="input">托盘变更输入参数</param>
    private async Task CreateStockUnbindRecordAsync(PdaTrayChangeInput input)
    {
        // 查询物料基础信息：用于记录物料名称和规格
        var baseGoods = await _repos.Material.GetFirstAsync(w => w.Id.ToString() == input.MaterialId.Trim());
        if (baseGoods is null)
            throw Oops.Bah("未查询物品信息");
        // 构建托盘变更记录：用于审计追踪和历史查询
        var bind = new WmsStockUnbind
        {
            UpbindStockCode = input.StockCode.Trim(),    //源托盘编码
            BindStockCode = input.StockCodeNew.Trim(),    //目标托盘编码
            StockLotNo = input.LotNo.Trim(),    //批次号
            MaterialId = input.MaterialId.Trim(),    //物料ID
            MaterialName = baseGoods.MaterialName,    //物料名称
            MaterialStandard = baseGoods.MaterialStandard,    //物料规格
            Qty = input.ExportNum,    //变更数量
            CreateUserId = 0,    //创建用户ID
            CreateTime = DateTime.Now    //创建时间
        };
        // 插入托盘变更记录
        await _repos.StockUnbind.InsertAsync(bind);
    }
    #endregion
}
