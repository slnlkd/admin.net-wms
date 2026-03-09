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
/// PDA 出库有箱码拆垛业务处理类
/// <para>处理有箱码的手动拆垛出库相关业务功能</para>
/// </summary>
/// <remarks>
/// 主要功能包括:
/// <list type="bullet">
/// <item><description>绑定、查询、删除拆垛信息</description></item>
/// <item><description>人工拆垛出库确认</description></item>
/// <item><description>箱码信息查询与验证</description></item>
/// <item><description>库存扣减与托盘释放</description></item>
/// <item><description>任务状态更新</description></item>
/// </list>
/// </remarks>
public class PdaExportBox : PdaExportBase, ITransient
{
    #region 字段定义
    private readonly ILogger _logger;
    private readonly WmsSqlViewService _sqlViewService;
    #endregion

    #region 常量定义
    private const int StateActive = 0;
    private const int StateProcessed = 1;
    private const int StatusSuccess = 1;
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖
    /// </summary>
    public PdaExportBox(
        ILoggerFactory loggerFactory,
        ISqlSugarClient sqlSugarClient,
        PdaBaseRepos repos,
        WmsSqlViewService sqlViewService) : base(sqlSugarClient, repos)
    {
        _sqlViewService = sqlViewService;
        _logger = loggerFactory.CreateLogger(CommonConst.PdaExportBox);
    }
    #endregion

    #region 公共接口方法

    /// <summary>
    /// 删除拆跺信息数据
    /// <para>对应 JC35 接口:【RemoveManual】</para>
    /// </summary>
    /// <param name="input">删除拆跺信息的请求参数，包含拆跺记录ID、箱码、托盘编码、出库流水号等信息</param>
    /// <returns>操作结果，包含成功状态和消息</returns>
    /// 功能说明：
    /// <item><description>验证输入参数的有效性</description></item>
    /// <item><description>根据ID或条件查询拆跺记录</description></item>
    /// <item><description>将记录标记为已删除（软删除）</description></item>
    /// <item><description>更新记录的修改时间</description></item>
    public async Task<PdaActionResult> RemoveManualAsync(PdaManualRemoveInput input)
    {
        return await ExecuteWithLoggingAsync("删除拆跺信息", async () =>
        {
            // 验证输入参数
            ValidateInput(input);
            // 查找待删除的拆跺记录，不存在则抛出异常
            var manual = await FindManualDepalletizingForRemovalAsync(input) ?? throw Oops.Bah("拆跺信息不存在或已处理!");
            // 标记为软删除
            manual.IsDelete = true;
            manual.UpdateTime = DateTime.Now;
            // 更新数据库中的删除标志和更新时间
            await _repos.ManualDepalletizing.AsUpdateable(manual)
                .UpdateColumns(x => new { x.IsDelete, x.UpdateTime })
                .ExecuteCommandAsync();
            return CreateSuccessResult();
        }, new { input.Id, input.BoxCode, input.TrayCode, input.ExportOrderNo });
    }

    /// <summary>
    /// 查询拆跺箱码信息
    /// <para>对应 JC35 接口:【GetManBoxInfo】</para>
    /// </summary>
    /// <param name="input">查询拆跺箱码信息的请求参数，包含箱码、托盘编码、出库流水号等信息</param>
    /// <returns>查询结果，包含箱码详细信息、状态码和消息</returns>
    /// 功能说明：
    /// <item><description>验证输入参数的有效性</description></item>
    /// <item><description>根据箱码查询拆跺视图数据</description></item>
    /// <item><description>如果提供了托盘编码，验证托盘与箱码的匹配关系</description></item>
    /// <item><description>检查托盘与订单的质检状态是否一致</description></item>
    /// <item><description>返回格式化的箱码详细信息</description></item>
    public async Task<PdaManualBoxInfoResult> GetManualBoxInfoAsync(PdaManualBoxQueryInput input)
    {
        return await ExecuteWithLoggingAsync("查询拆跺箱码信息", async () =>
        {
            // 验证输入参数
            ValidateInput(input);
            var boxCode = ValidateString(input.BoxCode, "箱码");
            // 根据箱码查询拆跺视图数据
            var record = await _sqlViewService.QueryManualDepalletizingView()
                .MergeTable()
                .FirstAsync(x => x.BoxCode == boxCode);
            // 箱码不存在时返回特殊标志
            if (record == null)
                return CreateManualBoxInfoResult(1, "箱码信息不存在!", null, "0", 0);
            // 如果未提供托盘编码，直接返回箱码信息
            if (string.IsNullOrWhiteSpace(input.TrayCode))
                return CreateManualBoxInfoResult(1, "查询成功!", MapManualBoxInfo(record), "1", 1);
            // 验证托盘编码并查询箱码与托盘的匹配关系
            var tray = ValidateString(input.TrayCode, "托盘编码");
            var orderNo = input.ExportOrderNo?.Trim();
            record = await _sqlViewService.QueryManualDepalletizingView()
                .MergeTable()
                .FirstAsync(x => x.BoxCode == boxCode && x.StockCode == tray && x.ExportOrderNo == orderNo);
            // 箱码与托盘不匹配
            if (record == null)
                return CreateManualBoxInfoResult(0, "箱码与托盘不匹配!", null, "-1", 0);
            // 验证托盘质检状态与订单质检状态是否一致
            if (record.TrayInspectionStatus != record.OrderInspectionStatus)
                return CreateManualBoxInfoResult(0, "箱码与流水质检状态不匹配!", null, "-1", 0);
            return CreateManualBoxInfoResult(1, "查询成功!", MapManualBoxInfo(record), "1", 1);
        }, new { input.BoxCode, input.TrayCode, input.ExportOrderNo });
    }

    /// <summary>
    /// 根据托盘编号查询对应的出库单号
    /// <para>对应 JC35 接口:【GetExprotCodeById】</para>
    /// </summary>
    /// <param name="input">查询出库单的请求参数，包含托盘编码和是否箱码标识</param>
    /// <returns>出库单信息列表，包含出库单号、托盘编码、物料编码等信息</returns>
    /// 功能说明：
    /// <item><description>根据托盘编码查询拆跺视图数据</description></item>
    /// <item><description>筛选出已执行完成的出库流水（ExportExecuteFlag = 1）</description></item>
    /// <item><description>按出库单号分组并去重</description></item>
    /// <item><description>如果指定了IsBoxCode参数，过滤掉有箱码的托盘</description></item>
    /// <item><description>返回格式化的出库单信息列表</description></item>
    public async Task<PdaLegacyResult<List<PdaExportOrderItem>>> GetExportOrderByTrayAsync(PdaExportOrderQueryInput input)
    {
        return await ExecuteWithLoggingAsync("根据托盘查询出库单号", async () =>
        {
            // 验证输入参数
            ValidateInput(input);
            var trayCode = ValidateString(input.StockCode, "托盘编码");
            // 查询托盘对应的已执行完成的出库流水
            var rawList = await _sqlViewService.QueryManualDepalletizingView()
                .MergeTable()
                .Where(x => x.StockCode == trayCode && x.ExportExecuteFlag == StatusSuccess)
                .ToListAsync();
            // 按出库单号分组并取每组的第一条记录（去重）
            var manualOrders = rawList.GroupBy(x => x.ExportOrderNo).Select(g => g.First()).ToList();
            var result = new List<PdaExportOrderItem>();
            HashSet<string> trayHasBoxCodes = null;
            // 如果指定了IsBoxCode参数，需要过滤掉有箱码的托盘
            if (!string.IsNullOrWhiteSpace(input.IsBoxCode) && manualOrders.Count > 0)
            {
                // 收集所有托盘ID
                var trayIds = manualOrders
                    .Select(x => x.TrayId)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();
                if (trayIds.Count > 0)
                {
                    // 查询哪些托盘包含箱码
                    var trayIdList = await _repos.StockInfo.AsQueryable()
                        .Where(x => trayIds.Contains(x.TrayId) && !SqlFunc.IsNullOrEmpty(x.BoxCode))
                        .Select(x => x.TrayId)
                        .Distinct()
                        .ToListAsync();
                    trayHasBoxCodes = trayIdList.ToHashSet(StringComparer.OrdinalIgnoreCase);
                }
            }
            // 构建返回结果，根据IsBoxCode参数过滤
            foreach (var item in manualOrders)
            {
                var trayId = item.TrayId;
                // 如果指定了IsBoxCode且托盘有箱码，则跳过该托盘
                if (!string.IsNullOrWhiteSpace(input.IsBoxCode))
                {
                    if (trayHasBoxCodes != null &&
                        !string.IsNullOrWhiteSpace(trayId) &&
                        trayHasBoxCodes.Contains(trayId))
                    {
                        continue;
                    }
                }
                result.Add(new PdaExportOrderItem
                {
                    ExportOrderNo = item.ExportOrderNo,
                    StockCode = item.StockCode,
                    TrayId = item.TrayId,
                    MaterialCode = item.MaterialCode,
                    ExportMaterialName = item.ExportMaterialName
                });
            }
            return CreateLegacyResult(1, "出库单信息列表", result, result.Count);
        }, new { input.StockCode, input.IsBoxCode });
    }

    /// <summary>
    /// 查询拆跺信息
    /// <para>对应 JC35 接口:【GetTrayInfo】</para>
    /// </summary>
    /// <param name="input">查询拆跺信息的请求参数，必须包含托盘编码和出库流水号</param>
    /// <returns>拆跺信息查询结果，包含托盘详细信息或错误信息</returns>
    /// 功能说明：
    /// <item><description>验证输入参数，确保托盘编码和出库流水号不为空</description></item>
    /// <item><description>根据托盘编码和出库流水号查询拆跺视图数据</description></item>
    /// <item><description>返回第一条匹配的拆跺记录详细信息</description></item>
    /// <item><description>如果找不到记录则返回失败结果</description></item>
    public async Task<PdaManualBoxInfoResult> GetTrayInfoAsync(PdaManualDepalletizingQueryInput input)
    {
        ValidateInput(input);
        var trayCode = ValidateString(input.TrayCode, "托盘编码");
        var orderNo = ValidateString(input.ExportOrderNo, "出库流水");
        var list = await _sqlViewService.QueryManualDepalletizingView()
            .MergeTable()
            .Where(x => x.StockCode == trayCode && x.ExportOrderNo == orderNo)
            .ToListAsync();
        if (list == null || list.Count == 0)
            return CreateManualBoxInfoResult(0, "托盘信息不存在!", null, "-1", 0);
        return CreateManualBoxInfoResult(1, "查询成功!", MapManualBoxInfo(list[0]), "1", 1);
    }

    /// <summary>
    /// 计算出库绑定数量
    /// <para>对应 JC35 接口:【GetBoxSum】</para>
    /// </summary>
    /// <param name="input">计算绑定数量的请求参数，包含物料编码、批号、托盘编码、出库数量等信息</param>
    /// <returns>计算结果，包含绑定数量、零箱数量、整箱数量等信息</returns>
    /// 功能说明：
    /// <item><description>检查当前托盘是否已绑定到其他出库流水</description></item>
    /// <item><description>根据锁定数量和出库数量计算绑定数量</description></item>
    /// <item><description>如果物料有箱数量规格，计算整箱出库件数</description></item>
    /// <item><description>区分整箱和零箱数量的计算逻辑</description></item>
    /// <item><description>返回详细的数量分配结果</description></item>
    public async Task<PdaLegacyResult<PdaBoxListQtyOutput>> CalculateBoxQuantityAsync(PdaBoxListQtyInput input)
    {
        // 验证输入参数
        ValidateInput(input, "获取信息失败!");
        // 检查当前托盘是否已绑定到其他出库流水
        var existing = await _repos.ManualDepalletizing.GetFirstAsync(x =>
            x.ExportOrderNo == input.ExportOrderNo &&
            x.TrayCode == input.StockStockCode &&
            x.IsDelete == false &&
            x.State == StateActive);
        if (existing != null)
            return CreateLegacyResult<PdaBoxListQtyOutput>(0, "当前出库物料已绑定!", null, 1);
        // 构建输出对象，复制输入参数
        var output = new PdaBoxListQtyOutput
        {
            ExportGoodsCode = input.ExportGoodsCode,
            LotNo = input.LotNo,
            StockStockCode = input.StockStockCode,
            BoxCode = input.BoxCode,
            ExportOrderNo = input.ExportOrderNo,
            LockQuantity = input.LockQuantity,
            Qty = input.Qty,
            GroupQuantity = input.GroupQuantity,
            ExportQuantity = input.ExportQuantity,
            TypeCode = input.TypeCode,
        };
        // 计算零箱出库数量：根据库存数量和出库需求量计算
        if (input.Qty == input.ExportQuantity)
        {
            // 库存数量等于出库需求，全部零箱出库
            output.OutScatterQty = Convert.ToDecimal(input.Qty ?? 0);
        }
        else if (input.ExportQuantity >= input.GroupQuantity)
        {
            // 出库需求超过成组数量，计算剩余零箱部分
            var diff = (input.ExportQuantity ?? 0) - Convert.ToDecimal(input.GroupQuantity ?? 0);
            output.OutScatterQty = Math.Min(Convert.ToDecimal(input.Qty ?? 0), diff);
        }
        else
        {
            // 其他情况，全部作为零箱出库
            output.OutScatterQty = Convert.ToDecimal(input.Qty ?? 0);
        }
        // 计算整箱出库数量：如果有箱数量规格，按整箱计算
        if (!string.IsNullOrWhiteSpace(input.TypeCode))
        {
            // 查询物料的箱数量规格
            var material = await _repos.Material.GetFirstAsync(x => x.MaterialCode == input.ExportGoodsCode && x.IsDelete == false);
            if (material != null && !string.IsNullOrWhiteSpace(material.BoxQuantity))
            {
                // 如果物料有箱数量规格且出库数量足够一箱，计算整箱出库件数
                if (decimal.TryParse(material.BoxQuantity, out var boxQty) && boxQty > 0 && input.ExportQuantity >= boxQty)
                {
                    var qty = (double)(input.ExportQuantity ?? 0) / (double)boxQty;
                    output.OutQty = Convert.ToInt32(Math.Floor(qty));
                }
            }
        }
        return CreateLegacyResult<PdaBoxListQtyOutput>(1, "查询成功!", output, 1);
    }

    /// <summary>
    /// 绑定拆跺箱码
    /// <para>对应 JC35 接口:【AddManualDepalletizing】</para>
    /// </summary>
    /// <param name="input">绑定拆跺箱码的请求参数，包含出库流水号、箱码、托盘编码、数量等信息</param>
    /// <returns>绑定操作结果，包含成功状态和消息</returns>
    /// 功能说明：
    /// <item><description>验证出库流水号的有效性</description></item>
    /// <item><description>加载对应的库存托盘信息</description></item>
    /// <item><description>判断是否为全托出库（出库数量等于库存总数量）</description></item>
    /// <item><description>全托出库时自动绑定该托盘下所有箱码</description></item>
    /// <item><description>部分出库时只绑定指定的单个箱码</description></item>
    /// <item><description>创建拆跺记录并保存到数据库</description></item>
    public async Task<PdaActionResult> AddManualDepalletizingAsync(PdaManualDepalletizingCreateInput input)
    {
        // 验证输入参数
        ValidateInput(input);
        ValidateString(input.ExportOrderNo, "出库流水");
        // 查询出库流水信息
        var exportOrder = await _repos.ExportOrder.GetFirstAsync(x =>
            x.ExportOrderNo == input.ExportOrderNo && x.IsDelete == false) ?? throw Oops.Bah("获取出库流水信息失败!");
        // 加载对应的库存托盘信息
        var stockTray = await LoadExportTrayAsync(exportOrder) ?? throw Oops.Bah("获取库存托盘信息失败!");
        var trayId = stockTray.Id.ToString();
        // 判断是否为全托出库：出库数量等于托盘总库存数量（库存+锁定）
        var isFullTray = (exportOrder.ExportQuantity ?? 0) == (stockTray.StockQuantity ?? 0) + (stockTray.LockQuantity ?? 0);
        if (isFullTray)
        {
            // 全托出库：自动绑定该托盘下所有箱码
            var materialId = exportOrder.ExportMaterialId?.ToString();
            // 查询该出库流水对应的所有出库箱码
            var exportBoxes = await _repos.ExportBoxInfo.AsQueryable()
                .Where(x => x.ExportOrderNo == exportOrder.ExportOrderNo && x.MaterialId == materialId && x.LotNo == exportOrder.ExportLotNo)
                .ToListAsync();
            var manualList = new List<WmsManualDepalletizing>();
            // 遍历所有箱码，为每个箱码创建拆跺记录
            foreach (var item in exportBoxes)
            {
                var info = await _repos.StockInfo.GetFirstAsync(x => x.BoxCode == item.BoxCode && x.TrayId == trayId);
                if (info == null) continue;
                // 计算箱码的总数量（库存数量 + 锁定数量）
                var totalQty = FormatDecimal(info.Qty + (info.LockQuantity ?? 0));
                manualList.Add(new WmsManualDepalletizing
                {
                    Id = SnowFlakeSingle.Instance.NextId(),
                    BoxCode = item.BoxCode,
                    Qty = totalQty,
                    ScanQty = totalQty,
                    TrayCode = item.StockCodeCode,
                    State = StateActive,
                    ExportOrderNo = exportOrder.ExportOrderNo,
                    IsDelete = false,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                });
            }
            // 批量插入拆跺记录
            if (manualList.Count > 0)
            {
                await _repos.ManualDepalletizing.InsertRangeAsync(manualList);
            }
        }
        else
        {
            // 部分出库：只绑定指定的单个箱码
            var manual = new WmsManualDepalletizing
            {
                Id = SnowFlakeSingle.Instance.NextId(),
                BoxCode = input.BoxCode,
                Qty = FormatDecimal(input.Qty),
                ScanQty = FormatDecimal(input.ScanQty),
                TrayCode = input.TrayCode,
                State = 0,
                ExportOrderNo = exportOrder.ExportOrderNo,
                IsDelete = false,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };
            await _repos.ManualDepalletizing.InsertAsync(manual);
        }
        return CreateSuccessResult("绑定成功");
    }

    /// <summary>
    /// 查询拆跺记录
    /// <para>对应 JC35 接口:【GetManualDepalletizing】</para>
    /// </summary>
    /// <param name="input">查询拆跺记录的请求参数，必须包含托盘编码和出库流水号</param>
    /// <returns>拆跺记录列表，包含箱码、物料信息、数量等详细信息</returns>
    /// 功能说明：
    /// <item><description>关联查询拆跺记录、库存信息、物料信息和托盘信息</description></item>
    /// <item><description>筛选活跃状态且未删除的拆跺记录</description></item>
    /// <item><description>按ID降序排列，返回最新的记录</description></item>
    /// <item><description>返回格式化的拆跺记录列表，包含物料名称、编码等关联信息</description></item>
    public async Task<PdaLegacyResult<List<PdaManualDepalletizingItem>>> GetManualDepalletizingAsync(PdaManualDepalletizingQueryInput input)
    {
        ValidateInput(input);
        var trayCode = ValidateString(input.TrayCode, "托盘编码");
        var orderNo = ValidateString(input.ExportOrderNo, "出库流水");
        var list = await _sqlSugarClient.Queryable<WmsManualDepalletizing, WmsStockInfo, WmsBaseMaterial, WmsStockTray>(
                (manual, info, material, tray) => new JoinQueryInfos(
                    JoinType.Inner, manual.BoxCode == info.BoxCode,
                    JoinType.Left, info.MaterialId == SqlFunc.ToString(material.Id),
                    JoinType.Left, info.TrayId == SqlFunc.ToString(tray.Id)))
            .Where((manual, info, material, tray) =>
                manual.TrayCode == trayCode &&
                manual.ExportOrderNo == orderNo &&
                manual.State == StateActive &&
                manual.IsDelete == false)
            .OrderBy((manual, info, material, tray) => manual.Id, OrderByType.Desc)
            .Select((manual, info, material, tray) => new PdaManualDepalletizingItem
            {
                BoxCode = manual.BoxCode,
                Qty = info.Qty,
                ScanQty = info.LockQuantity ?? info.Qty,
                TrayCode = manual.TrayCode,
                MaterialName = material.MaterialName,
                MaterialCode = material.MaterialCode,
                StockLotNo = tray.LotNo,
                Id = SqlFunc.ToString(manual.Id),
                OutQty = manual.OutQty == null ? null : SqlFunc.ToString(manual.OutQty)
            }).ToListAsync();
        return CreateLegacyResult<List<PdaManualDepalletizingItem>>(1, "查询成功!", list, list.Count);
    }
   /// <summary>
    /// 人工拆垛出库确认
    /// <para>对应 JC35 接口：【SaveManTask】</para>
    /// </summary>
    /// <param name="input">人工拆垛确认的请求参数，包含托盘编码和出库流水号</param>
    /// <returns>确认操作结果，包含成功状态、消息和数据</returns>
    /// <exception cref="Exception">当参数验证失败、库存不足或数量不匹配时抛出异常</exception>
    /// <remarks>
    /// 业务流程：
    /// <list type="number">
    /// <item><description>校验托盘编码和出库流水号的合法性</description></item>
    /// <item><description>加载并聚合拆垛明细信息</description></item>
    /// <item><description>验证PDA扫描数量与流水拣货数量是否一致</description></item>
    /// <item><description>在事务内更新以下表的数据：
    /// <list type="bullet">
    /// <item><description>WmsManualDepalletizing - 标记拆垛记录为已处理</description></item>
    /// <item><description>WmsExportBoxInfo - 更新出库箱码拣货状态和数量</description></item>
    /// <item><description>WmsStockInfo - 扣减或删除库存箱码</description></item>
    /// <item><description>WmsStockTray - 更新托盘库存数量和状态</description></item>
    /// <item><description>WmsStock - 更新库位维度的锁定数量</description></item>
    /// <item><description>WmsBaseSlot - 更新储位状态</description></item>
    /// <item><description>WmsExportOrder - 推进出库流水状态</description></item>
    /// <item><description>WmsExportNotify/WmsExportNotifyDetail - 更新出库通知状态</description></item>
    /// </list>
    /// </description></item>
    /// <item><description>释放未被扫描的必出箱码库存</description></item>
    /// <item><description>返回与JC35兼容的确认结果</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaLegacyResult<object>> SaveManualTaskAsync(PdaManualTaskSaveInput input)
    {
        // 验证输入参数
        if (input == null)
        {
            throw Oops.Bah("请求参数不能为空！");
        }
        var trayCode = ValidateString(input.TrayCode, "托盘编码");
        var orderNo = ValidateString(input.ExportOrderNo, "出库流水");
        // 查询托盘视图数据验证托盘信息是否存在
        var trayViewList = await _sqlViewService.QueryManualDepalletizingView()
            .MergeTable()
            .Where(x => x.StockCode == trayCode && x.ExportOrderNo == orderNo)
            .ToListAsync();
        if (trayViewList == null || trayViewList.Count == 0)
            return CreateLegacyResultObject(0, "托盘信息不存在！");
        // 加载拆垛明细数据（包含拆垛记录、库存箱码、物料等信息）
        var manualDetails = await LoadManualTaskDetailsAsync(trayCode, orderNo);
        if (manualDetails.Count == 0)
            return CreateLegacyResultObject(1, "未绑定出库的箱码！", string.Empty);
        // 查询出库流水信息
        var exportOrder = await _repos.ExportOrder.GetFirstAsync(x =>
            x.ExportOrderNo == orderNo && x.IsDelete == false) ?? throw Oops.Bah("获取出库流水信息失败！");
        // 预加载出库箱码信息和库存信息，避免在事务中重复查询
        var exportBoxMap = await LoadExportBoxInfoMapAsync(orderNo, manualDetails);
        var stockInfoMap = await LoadStockInfoMapAsync(manualDetails);
        // 初始化跟踪集合，用于批量更新
        var updatedExportBoxCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stockReduceTracker = new Dictionary<long, WmsStockInfo>();
        var stockDeleteTracker = new Dictionary<long, WmsStockInfo>();
        // 加载库存托盘信息
        var stockTray = await LoadExportTrayAsync(exportOrder) ?? throw Oops.Bah("获取库存托盘信息失败！");
        // 查询出库物料信息
        var exportMaterial = exportOrder.ExportMaterialId.HasValue ? await _repos.Material.GetFirstAsync(x => x.Id == exportOrder.ExportMaterialId.Value && x.IsDelete == false) : null;
        // 计算计划数量和实际扫描数量
        var expected = exportOrder.PickedNum ?? 0m;
        var actual = CalculateManualScanSum(manualDetails, exportMaterial?.MaterialCode, exportOrder.ExportLotNo);

        // 根据仓库配置验证超出/少出
        if (expected != 0 && actual != expected)
        {
            // 获取仓库配置
            var warehouse = exportOrder.ExportWarehouseId.HasValue
                ? await _repos.Warehouse.GetFirstAsync(x => x.Id == exportOrder.ExportWarehouseId.Value && x.IsDelete == false)
                : null;

            if (actual > expected)  // 超出
            {
                // 检查仓库是否允许超出
                if (!(warehouse?.IsExceeding ?? false))
                {
                    return CreateLegacyResultObject(0,
                        $"出库数量超出：计划 {expected}，实际扫描 {actual}，超出 {actual - expected}当前仓库不允许超出，请重新扫描！",
                        string.Empty);
                }
            }
            else  // 少出
            {
                // 检查仓库是否允许少出
                if (!(warehouse?.IsUnderpay ?? false))
                {
                    return CreateLegacyResultObject(0,
                        $"出库数量不足：计划 {expected}，实际扫描 {actual}，少出 {expected - actual}当前仓库不允许少出，请继续扫描！",
                        string.Empty);
                }
            }
        }
        var now = DateTime.Now;
        // 开启数据库事务，确保所有更新操作的原子性
        var tranResult = await _sqlSugarClient.Ado.UseTranAsync(async () =>
        {
            try
            {
                decimal totalScanQty = 0m;
                // 遍历所有拆垛明细，处理每个箱码的扫描数据
                foreach (var detail in manualDetails)
                {
                    var scanQty = detail.EffectiveScanQty;
                    if (scanQty <= 0) continue;
                    totalScanQty += scanQty;
                    _logger.LogInformation("处理拆垛明细: ManualId={ManualId}, BoxCode={BoxCode}, ScanQty={ScanQty}",
                        detail.ManualId, detail.BoxCode, scanQty);
                    // 1. 更新拆垛记录状态为已处理
                    await UpdateManualRecordAsync(detail.ManualId, now);
                    // 2. 更新出库箱码信息
                    var normalizedBoxCode = detail.BoxCode?.Trim();
                    if (!string.IsNullOrWhiteSpace(normalizedBoxCode))
                    {
                        if (exportBoxMap.TryGetValue(normalizedBoxCode, out var exportBox))
                        {
                            // 如果箱码在预加载的Map中，更新内存对象
                            exportBox.Status = StatusSuccess;
                            exportBox.PickEdNum = scanQty;
                            exportBox.PickNum = scanQty;
                            exportBox.UpdateTime = now;
                            updatedExportBoxCodes.Add(normalizedBoxCode);
                        }
                        else
                        {
                            // 如果箱码不在Map中，直接更新数据库
                            await UpdateExportBoxInfoAsync(orderNo, normalizedBoxCode, scanQty, now);
                        }
                    }
                    // 3. 扣减或删除库存箱码
                    if (detail.StockInfoId.HasValue && stockInfoMap.TryGetValue(detail.StockInfoId.Value, out var stockInfo))
                    {
                        // 如果库存信息在预加载的Map中，更新内存对象
                        var qtyInfo = (stockInfo.Qty ?? 0m) - scanQty;
                        if (qtyInfo < 0) throw Oops.Bah("数量异常");
                        if (qtyInfo > 0)
                        {
                            // 剩余数量大于0，仅扣减数量
                            stockInfo.Qty = qtyInfo;
                            stockInfo.UpdateTime = now;
                            stockReduceTracker[stockInfo.Id] = stockInfo;
                        }
                        else
                        {
                            // 剩余数量为0，标记删除
                            stockInfo.Qty = 0;
                            stockInfo.IsDelete = true;
                            stockInfo.UpdateTime = now;
                            stockDeleteTracker[stockInfo.Id] = stockInfo;
                        }
                    }
                    else
                    {
                        // 如果库存信息不在Map中，直接更新数据库
                        await UpdateStockInfoAsync(detail, scanQty, now);
                    }
                }
                _logger.LogInformation("拆垛明细处理完成，总扫描数量: {TotalScanQty}", totalScanQty);
                // 批量提交出库箱码更新
                await FlushExportBoxUpdatesAsync(updatedExportBoxCodes, exportBoxMap);
                // 批量提交库存信息更新
                await FlushStockInfoUpdatesAsync(stockReduceTracker.Values, stockDeleteTracker.Values);
                // 释放未被扫描的必出箱码库存
                await ReleaseMandatoryBoxesAsync(orderNo, manualDetails, now);
                // 更新托盘库存数量和状态
                await UpdateStockTrayAsync(stockTray, exportOrder, totalScanQty, now);
                // 更新库位维度的库存记录
                await UpdateStockRecordAsync(exportOrder, totalScanQty, now);
                // 更新储位状态
                await UpdateSlotStatusAsync(exportOrder, now);
                // 更新出库流水状态和数量
                await UpdateExportOrderAsync(exportOrder, totalScanQty, now);
                // 更新出库通知状态
                await UpdateExportNotifyAsync(exportOrder, totalScanQty, now);
                _logger.LogInformation("出库拆垛确认完成: TrayCode={TrayCode}, ExportOrderNo={ExportOrderNo}, TotalScanQty={TotalScanQty}",
                    trayCode, orderNo, totalScanQty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "事务执行过程中发生错误: TrayCode={TrayCode}, ExportOrderNo={ExportOrderNo}", trayCode, orderNo);
                throw;
            }
        });
        if (!tranResult.IsSuccess)
        {
            _logger.LogError("事务提交失败: {ErrorMessage}, TrayCode={TrayCode}, ExportOrderNo={ExportOrderNo}",
                tranResult.ErrorMessage, trayCode, orderNo);
            throw Oops.Oh($"提交失败：{tranResult.ErrorMessage}");
        }
        return CreateLegacyResultObject(1, "提交完成！", string.Empty);
    }
    #endregion

    #region 辅助方法

    /// <summary>
    /// 将视图数据映射为 PDA 拆垛箱码明细
    /// </summary>
    /// <param name="view">视图数据</param>
    /// <returns>拆垛箱码明细</returns>
    private static PdaManualBoxInfoDetail MapManualBoxInfo(ViewManualDepalletizing view)
    {
        if (view == null)
        {
            return null;
        }
        return new PdaManualBoxInfoDetail
        {
            ExportMaterialName = view.ExportMaterialName,
            BoxCode = view.BoxCode,
            LotNo = view.LotNo,
            Qty = view.Qty,
            LockQuantity = view.LockQuantity,
            OutQty = view.OutQty,
            ToutQty = view.ToutQty,
            StockSlotCode = view.StockSlotCode,
            StockCode = view.StockCode,
            ExportQuantity = view.ExportQuantity,
            StockQuantity = view.StockQuantity,
            ExportOrderNo = view.ExportOrderNo,
            GroupQuantity = view.GroupQuantity,
            QRCode = view.QRCode,
            OrderInspectionStatus = view.OrderInspectionStatus,
            TrayInspectionStatus = view.TrayInspectionStatus,
            ExportExecuteFlag = view.ExportExecuteFlag,
            TrayId = view.TrayId,
            Id = view.Id,
            MaterialCode = view.MaterialCode,
        };
    }

    /// <summary>
    /// 查找待删除的拆垛记录
    /// </summary>
    /// <param name="input">删除拆垛信息的请求参数</param>
    /// <returns>待删除的拆垛记录</returns>
    private async Task<WmsManualDepalletizing> FindManualDepalletizingForRemovalAsync(PdaManualRemoveInput input)
    {
        if (input.Id.HasValue)
        {
            return await _repos.ManualDepalletizing.GetFirstAsync(x => x.Id == input.Id.Value && x.State == StateActive && x.IsDelete == false);
        }
        var boxCode = ValidateString(input.BoxCode, "箱码");
        var trayCode = ValidateString(input.TrayCode, "托盘编码");
        var orderNo = ValidateString(input.ExportOrderNo, "出库流水");
        return await _repos.ManualDepalletizing.GetFirstAsync(x =>
            x.BoxCode == boxCode &&
            x.TrayCode == trayCode &&
            x.ExportOrderNo == orderNo &&
            x.State == StateActive &&
            x.IsDelete == false);
    }

    /// <summary>
    /// 执行带日志记录的异步操作（子类包装方法）
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="operationName">操作名称</param>
    /// <param name="operation">异步操作</param>
    /// <param name="parameters">日志参数</param>
    /// <returns>操作结果</returns>
    private async Task<T> ExecuteWithLoggingAsync<T>(string operationName, Func<Task<T>> operation, object parameters = null)
    {
        return await ExecuteWithLoggingAsync(_logger, operationName, operation, parameters);
    }

    /// <summary>
    /// 构造 PDA 手动拆垛箱码查询返回结果
    /// </summary>
    /// <param name="code">状态码</param>
    /// <param name="message">消息</param>
    /// <param name="data">箱码明细数据</param>
    /// <param name="flag">标志</param>
    /// <param name="count">数量</param>
    /// <returns>查询结果</returns>
    private static PdaManualBoxInfoResult CreateManualBoxInfoResult(int code, string message, PdaManualBoxInfoDetail data = null, string flag = null, int? count = null)
    {
        var result = new PdaManualBoxInfoResult
        {
            Code = code,
            Msg = message,
            Flag = flag ?? (code == 1 ? "1" : "0"),
            Data = data
        };
        if (count.HasValue)
        {
            result.Count = count.Value;
        }
        else
        {
            result.Count = data != null ? 1 : 0;
        }
        return result;
    }

    /// <summary>
    /// 构建人工拆垛确认所需的明细集合
    /// </summary>
    /// <param name="trayCode">托盘编码</param>
    /// <param name="exportOrderNo">出库流水号</param>
    /// <returns>拆垛记录、库存箱码、物料信息的聚合结果</returns>
    /// <remarks>
    /// 关联查询：
    /// <list type="bullet">
    /// <item><description>WmsManualDepalletizing - 人工拆垛记录（主表）</description></item>
    /// <item><description>WmsStockInfo - 库存箱码信息（左连接）</description></item>
    /// <item><description>WmsStockTray - 库存托盘信息（左连接）</description></item>
    /// <item><description>WmsExportBoxInfo - 出库箱码信息（左连接）</description></item>
    /// <item><description>WmsBaseMaterial - 物料基础信息（左连接）</description></item>
    /// </list>
    /// 筛选条件：
    /// <list type="bullet">
    /// <item><description>托盘编码和出库流水号必须匹配</description></item>
    /// <item><description>只查询活跃状态且未删除的拆垛记录</description></item>
    /// </list>
    /// </remarks>
    /// <returns>拆垛明细集合</returns>
    private async Task<List<ManualTaskDetail>> LoadManualTaskDetailsAsync(string trayCode, string exportOrderNo)
    {
        return await _sqlSugarClient.Queryable<WmsManualDepalletizing, WmsStockInfo, WmsStockTray, WmsExportBoxInfo, WmsBaseMaterial>(//查询人工拆垛确认所需的明细集合
                (manual, info, tray, box, material) => new JoinQueryInfos(
                    JoinType.Left, manual.BoxCode == info.BoxCode,//左连接
                    JoinType.Left, info.TrayId == SqlFunc.ToString(tray.Id),//左连接
                    JoinType.Left, manual.BoxCode == box.BoxCode && manual.ExportOrderNo == box.ExportOrderNo,//左连接
                    JoinType.Left, box.MaterialId == SqlFunc.ToString(material.Id)))//左连接
            .Where((manual, info, tray, box, material) => manual.TrayCode == trayCode && manual.ExportOrderNo == exportOrderNo && manual.State == StateActive && manual.IsDelete == false)
            .Select((manual, info, tray, box, material) => new ManualTaskDetail //创建人工拆垛确认所需的明细集合
            {
                ManualId = manual.Id,//设置ID
                BoxCode = manual.BoxCode,//设置箱码
                ManualQtyRaw = manual.Qty,//设置数量
                ManualScanQtyRaw = manual.ScanQty,//设置扫描数量
                StockInfoId = info.Id,//设置库存箱码ID
                StockQty = info.Qty,//设置库存数量
                StockLockQty = info.LockQuantity,//设置锁定数量
                TrayId = info.TrayId,//设置托盘ID
                MaterialCode = material.MaterialCode,//设置物料编码
                MaterialId = box.MaterialId,//设置物料ID
                StockLotNo = SqlFunc.IIF(SqlFunc.IsNullOrEmpty(info.LotNo), tray.LotNo, info.LotNo),//设置批次号
                ManualOutQty = manual.OutQty//设置出库数量
            }).ToListAsync();
    }

    /// <summary>
    /// 批量加载拆垛明细关联的库存箱码信息，避免在事务过程中重复查询
    /// </summary>
    /// <param name="manualDetails">拆垛明细集合</param>
    /// <returns>以 <c>StockInfoId</c> 为键的库存字典</returns>
    private async Task<Dictionary<long, WmsStockInfo>> LoadStockInfoMapAsync(IEnumerable<ManualTaskDetail> manualDetails)
    {
        var stockIds = manualDetails?
            .Where(x => x.StockInfoId.HasValue)
            .Select(x => x.StockInfoId.Value)
            .Distinct()
            .ToList() ?? new List<long>();

        if (stockIds.Count == 0)
        {
            return new Dictionary<long, WmsStockInfo>();
        }

        var stockInfos = await _repos.StockInfo.AsQueryable()
            .Where(x => stockIds.Contains(x.Id) && (x.IsDelete == false || x.IsDelete == null))
            .ToListAsync();

        return stockInfos.ToDictionary(x => x.Id);
    }

    /// <summary>
    /// 批量加载拆垛明细对应的出库箱码，提升后续状态更新效率
    /// </summary>
    /// <param name="exportOrderNo">出库流水号</param>
    /// <param name="manualDetails">拆垛明细集合</param>
    /// <returns>根据箱码分组的出库箱码信息</returns>
    private async Task<Dictionary<string, WmsExportBoxInfo>> LoadExportBoxInfoMapAsync(string exportOrderNo, IEnumerable<ManualTaskDetail> manualDetails)
    {
        var boxCodes = manualDetails?
            .Select(x => x.BoxCode?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        if (boxCodes.Count == 0)
        {
            return new Dictionary<string, WmsExportBoxInfo>(StringComparer.OrdinalIgnoreCase);
        }

        var exportBoxes = await _repos.ExportBoxInfo.AsQueryable()
            .Where(x => x.ExportOrderNo == exportOrderNo && boxCodes.Contains(x.BoxCode) && x.IsDelete == false)
            .ToListAsync();

        return exportBoxes.ToDictionary(x => x.BoxCode, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 将已修改的出库箱码一次性落库，保证与拆垛事务一致
    /// </summary>
    /// <param name="updatedBoxCodes">需要更新的箱码集合</param>
    /// <param name="exportBoxMap">箱码到出库信息的映射</param>
    private async Task FlushExportBoxUpdatesAsync(IEnumerable<string> updatedBoxCodes, IDictionary<string, WmsExportBoxInfo> exportBoxMap)
    {
        if (exportBoxMap == null || updatedBoxCodes == null)
        {
            return;
        }

        var boxesToUpdate = new List<WmsExportBoxInfo>();
        foreach (var code in updatedBoxCodes)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                continue;
            }
            if (exportBoxMap.TryGetValue(code, out var exportBox))
            {
                boxesToUpdate.Add(exportBox);
            }
        }

        if (boxesToUpdate.Count > 0)
        {
            await _repos.ExportBoxInfo.AsUpdateable(boxesToUpdate)
                .UpdateColumns(x => new { x.Status, x.PickEdNum, x.PickNum, x.UpdateTime })
                .ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 将累计的库存扣减或删除操作批量提交，降低数据库压力
    /// </summary>
    /// <param name="reduceItems">仅需扣减数量的库存记录</param>
    /// <param name="deleteItems">需要标记删除的库存记录</param>
    private async Task FlushStockInfoUpdatesAsync(IEnumerable<WmsStockInfo> reduceItems, IEnumerable<WmsStockInfo> deleteItems)
    {
        var reduceList = reduceItems?.ToList();
        var deleteList = deleteItems?.ToList();

        if (reduceList != null && reduceList.Count > 0)
        {
            await _repos.StockInfo.AsUpdateable(reduceList)
                .UpdateColumns(x => new { x.Qty, x.UpdateTime })
                .ExecuteCommandAsync();
        }

        if (deleteList != null && deleteList.Count > 0)
        {
            await _repos.StockInfo.AsUpdateable(deleteList)
                .UpdateColumns(x => new { x.Qty, x.IsDelete, x.UpdateTime })
                .ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 将指定人工拆垛记录标记为已处理
    /// </summary>
    /// <param name="manualId">人工拆垛ID</param>
    /// <param name="now">当前时间</param>
    private async Task UpdateManualRecordAsync(long manualId, DateTime now)
    {
        var manual = new WmsManualDepalletizing //创建人工拆垛记录
        {
            Id = manualId,
            State = 1,
            UpdateTime = now
        };
        await _repos.ManualDepalletizing.AsUpdateable(manual).UpdateColumns(x => new { x.State, x.UpdateTime }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新出库流水箱码的拣货状态与数量
    /// </summary>
    /// <param name="exportOrderNo">出库流水号</param>
    /// <param name="boxCode">箱码</param>
    /// <param name="scanQty">扫描数量</param>
    /// <param name="now">当前时间</param>

    private async Task UpdateExportBoxInfoAsync(string exportOrderNo, string boxCode, decimal scanQty, DateTime now)
    {
        var exportBox = await _repos.ExportBoxInfo.GetFirstAsync(x => x.ExportOrderNo == exportOrderNo && x.BoxCode == boxCode && x.IsDelete == false);//查询出库流水箱码;
        if (exportBox == null)//判断出库流水箱码是否存在
        {
            return;
        }
        exportBox.Status = 1;//设置拣货状态
        exportBox.PickEdNum = scanQty;//设置拣货数量
        exportBox.PickNum = scanQty;//设置拣货数量
        exportBox.UpdateTime = now;//设置更新时间
        await _repos.ExportBoxInfo.AsUpdateable(exportBox).UpdateColumns(x => new { x.Status, x.PickEdNum, x.PickNum, x.UpdateTime }).ExecuteCommandAsync();//更新出库流水箱码
    }

    /// <summary>
    /// 扣减或删除 `WmsStockInfo` 中的库存箱码
    /// <para>对照原接口逻辑：只更新 Qty，如果 qtyInfo 小于等于 0 则删除记录，不更新 LockQuantity</para>
    /// </summary>
    /// <param name="detail">人工拆垛确认所需的明细</param>
    /// <param name="scanQty">扫描数量</param>
    /// <param name="now">当前时间</param>
    private async Task UpdateStockInfoAsync(ManualTaskDetail detail, decimal scanQty, DateTime now)
    {
        if (!detail.StockInfoId.HasValue)//判断库存箱码ID是否存在
        {
            return;
        }
        var stockInfo = await _repos.StockInfo.GetFirstAsync(x => x.Id == detail.StockInfoId.Value && (x.IsDelete == false || x.IsDelete == null));//查询库存箱码
        if (stockInfo == null)//判断库存箱码是否存在
        {
            return;
        }
        // 对照原接口逻辑：var qtyInfo = (decimal)stockInfo.Qty - Convert.ToDecimal(Item.ScanQty);
        var qtyInfo = (stockInfo.Qty ?? 0m) - scanQty;//设置剩余数量
        if (qtyInfo < 0)//判断剩余数量是否小于0
        {
            throw Oops.Bah("数量异常");//抛出异常
        }
        if (qtyInfo > 0)//如果箱码库存数量减去出库数量后仍然有剩余，不删除库存表
        {
            stockInfo.Qty = qtyInfo;//设置数量
            stockInfo.UpdateTime = now;//设置更新时间
            await _repos.StockInfo.AsUpdateable(stockInfo)
                .UpdateColumns(x => new { x.Qty, x.UpdateTime })//更新库存箱码
                .ExecuteCommandAsync();
        }
        else//否则删除记录（对照原接口：DataContext.WmsStockInfo.DeleteOnSubmit）
        {
            stockInfo.Qty = 0;//设置数量
            stockInfo.IsDelete = true;//设置删除标志
            stockInfo.UpdateTime = now;//设置更新时间
            await _repos.StockInfo.AsUpdateable(stockInfo)
                .UpdateColumns(x => new { x.Qty, x.IsDelete, x.UpdateTime })//更新库存箱码
                .ExecuteCommandAsync();
        }
    }

  
    /// <summary>
    /// 若存在"必出箱"但本次 PDA 未扫描，则恢复对应锁定库存
    /// </summary>
    /// <param name="exportOrderNo">出库流水号</param>
    /// <param name="confirmedBoxes">已确认箱码</param>
    /// <param name="now">当前时间</param>
    private async Task ReleaseMandatoryBoxesAsync(string exportOrderNo, IEnumerable<ManualTaskDetail> confirmedBoxes, DateTime now)
    {
        // 查询该出库流水的所有必出箱码
        var mandatoryBoxes = await _sqlViewService.QueryExportBoxInfoView()
            .MergeTable()
            .Where(x => x.ExportOrderNo == exportOrderNo && x.PickNum != null && x.IsMustExport == StatusSuccess && x.IsDel == false)
            .Select(x => new { x.BoxCode, x.LotNo, x.MaterialId })
            .ToListAsync();
        if (mandatoryBoxes.Count == 0)
        {
            return;
        }
        // 构建已确认箱码的集合
        var confirmedCodes = new HashSet<string>(
            confirmedBoxes
                .Where(x => !string.IsNullOrWhiteSpace(x.BoxCode))
                .Select(x => x.BoxCode.Trim()),
            StringComparer.OrdinalIgnoreCase);
        // 筛选出未被扫描的必出箱码
        var pendingBoxes = mandatoryBoxes
            .Where(m => !string.IsNullOrWhiteSpace(m.BoxCode) && !confirmedCodes.Contains(m.BoxCode.Trim()))
            .ToList();
        if (pendingBoxes.Count == 0)
        {
            return;
        }

        var pendingCodes = pendingBoxes
            .Select(x => x.BoxCode.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        // 统一加载待恢复的库存箱码，避免逐个必出箱再次查询数据库
        var pendingStockInfos = await _repos.StockInfo.AsQueryable()
            .Where(x => pendingCodes.Contains(x.BoxCode) && (x.IsDelete != true))
            .ToListAsync();

        // 构建箱码到库存信息的映射
        var stockLookup = pendingStockInfos
            .GroupBy(x => x.BoxCode ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);
        var needUpdates = new List<WmsStockInfo>();

        // 遍历未被扫描的必出箱码，恢复其锁定库存
        foreach (var mandatory in pendingBoxes)
        {
            var normalizedCode = mandatory.BoxCode.Trim();
            if (!stockLookup.TryGetValue(normalizedCode, out var candidates))
            {
                _logger.LogWarning("释放必出箱码时未找到库存箱码: BoxCode={BoxCode}, LotNo={LotNo}, MaterialId={MaterialId}",
                    mandatory.BoxCode, mandatory.LotNo, mandatory.MaterialId);
                continue;
            }

            // 根据批次号和物料ID匹配具体的库存记录
            var stockInfo = candidates.FirstOrDefault(x =>
                (string.IsNullOrWhiteSpace(mandatory.LotNo) || string.Equals(x.LotNo, mandatory.LotNo, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(mandatory.MaterialId) || string.Equals(x.MaterialId, mandatory.MaterialId, StringComparison.OrdinalIgnoreCase)));

            if (stockInfo == null)
            {
                _logger.LogWarning("释放必出箱码时未匹配具体库存: BoxCode={BoxCode}, LotNo={LotNo}, MaterialId={MaterialId}",
                    mandatory.BoxCode, mandatory.LotNo, mandatory.MaterialId);
                continue;
            }

            // 将锁定数量恢复到可用数量，清零锁定数量
            stockInfo.Qty = (stockInfo.Qty ?? 0) + (stockInfo.LockQuantity ?? 0);
            stockInfo.LockQuantity = 0;
            stockInfo.UpdateTime = now;
            needUpdates.Add(stockInfo);
        }

        // 批量更新恢复的库存信息
        if (needUpdates.Count > 0)
        {
            await _repos.StockInfo.AsUpdateable(needUpdates)
                .UpdateColumns(x => new { x.Qty, x.LockQuantity, x.UpdateTime })
                .ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 按拆垛确认结果更新托盘库存/锁定数量，必要时释放托盘
    /// </summary>
    /// <param name="stockTray">库存托盘信息</param>
    /// <param name="exportOrder">出库流水信息</param>
    /// <param name="totalScanQty">本次扫描总数量</param>
    /// <param name="now">当前时间</param>
    private async Task UpdateStockTrayAsync(WmsStockTray stockTray, WmsExportOrder exportOrder, decimal totalScanQty, DateTime now)
    {
        if (stockTray == null)
        {
            return;
        }
        var exportType = exportOrder.ExportType ?? 0;
        // 空托出库特殊处理：清空托盘所有库存并释放托盘
        if (exportType == 3)
        {
            var trayIdStr = stockTray.Id.ToString();
            // 删除托盘下所有库存箱码
            await _repos.StockInfo.AsUpdateable()
                .SetColumns(x => new WmsStockInfo { Qty = 0, LockQuantity = 0, IsDelete = true, UpdateTime = now })
                .Where(x => x.TrayId == trayIdStr && (x.IsDelete == false || x.IsDelete == null))
                .ExecuteCommandAsync();
            // 清空托盘信息并标记删除
            stockTray.StockSlotCode = string.Empty;
            stockTray.StockQuantity = 0;
            stockTray.LockQuantity = 0;
            stockTray.IsDelete = true;
            stockTray.UpdateTime = now;
            await _repos.StockTray.AsUpdateable(stockTray).UpdateColumns(x => new { x.StockSlotCode, x.StockQuantity, x.LockQuantity, x.IsDelete, x.UpdateTime }).ExecuteCommandAsync();
            // 释放托盘编码，设置为空闲状态
            await _repos.SysStockCode.AsUpdateable()
                .SetColumns(x => new WmsSysStockCode { Status = 0, UpdateTime = now })
                .Where(x => x.StockCode == stockTray.StockCode && x.IsDelete == false)
                .ExecuteCommandAsync();
            return;
        }
        // 正常出库：扣减托盘锁定数量
        var newLock = (stockTray.LockQuantity ?? 0) - totalScanQty;
        if (newLock < 0)
        {
            newLock = 0;
        }
        stockTray.LockQuantity = newLock;
        stockTray.UpdateTime = now;
        // 如果托盘库存和锁定数量都为0，则释放托盘
        if ((stockTray.StockQuantity ?? 0) <= 0 && newLock <= 0)
        {
            stockTray.StockSlotCode = string.Empty;
            stockTray.IsDelete = true;
            stockTray.UpdateTime = now;
            await _repos.StockTray.AsUpdateable(stockTray)
                .UpdateColumns(x => new { x.LockQuantity, x.StockSlotCode, x.IsDelete, x.UpdateTime }).ExecuteCommandAsync();
            // 释放托盘编码
            await _repos.SysStockCode.AsUpdateable().SetColumns(x => new WmsSysStockCode { Status = 0, UpdateTime = now }).Where(x => x.StockCode == stockTray.StockCode && x.IsDelete == false).ExecuteCommandAsync();
        }
        else
        {
            // 托盘仍有库存，只更新锁定数量
            await _repos.StockTray.AsUpdateable(stockTray).UpdateColumns(x => new { x.LockQuantity, x.UpdateTime }).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 更新 `WmsStock`（库位维度）锁定数量，必要时软删除记录
    /// </summary>
    /// <param name="exportOrder">出库流水信息</param>
    /// <param name="totalScanQty">本次扫描总数量</param>
    /// <param name="now">当前时间</param>
    private async Task UpdateStockRecordAsync(WmsExportOrder exportOrder, decimal totalScanQty, DateTime now)
    {
        if (!exportOrder.ExportMaterialId.HasValue)
        {
            return;
        }
        // 构建查询条件：按物料、仓库查询库存记录
        var query = _repos.Stock.AsQueryable()
            .Where(x => x.MaterialId == exportOrder.ExportMaterialId &&
                       x.WarehouseId == exportOrder.ExportWarehouseId &&
                       x.IsDelete == false);
        // 如果有批次号，添加批次条件
        if (!string.IsNullOrWhiteSpace(exportOrder.ExportLotNo))
        {
            query = query.Where(x => x.LotNo == exportOrder.ExportLotNo);
        }
        // 如果有质检状态，添加质检状态条件
        if (exportOrder.InspectionStatus.HasValue)
        {
            query = query.Where(x => x.InspectionStatus == exportOrder.InspectionStatus);
        }
        var stockRecord = await query.FirstAsync();
        if (stockRecord == null)
        {
            return;
        }
        // 扣减库位维度的锁定数量
        var newLock = (stockRecord.LockQuantity ?? 0) - totalScanQty;
        if (newLock < 0)
        {
            newLock = 0;
        }
        stockRecord.LockQuantity = newLock;
        stockRecord.UpdateTime = now;
        // 如果库存数量和锁定数量都为0，软删除库存记录
        if ((stockRecord.StockQuantity ?? 0) <= 0 && newLock <= 0)
        {
            stockRecord.IsDelete = true;
            stockRecord.UpdateTime = now;
            await _repos.Stock.AsUpdateable(stockRecord).UpdateColumns(x => new { x.LockQuantity, x.IsDelete, x.UpdateTime }).ExecuteCommandAsync();
        }
        else
        {
            // 仍有库存，只更新锁定数量
            await _repos.Stock.AsUpdateable(stockRecord).UpdateColumns(x => new { x.LockQuantity, x.UpdateTime }).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 更新储位状态（对照原接口逻辑：如果储位状态不为4，则设置为0）
    /// </summary>
    /// <param name="exportOrder">出库流水信息</param>
    /// <param name="now">当前时间</param>
    private async Task UpdateSlotStatusAsync(WmsExportOrder exportOrder, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(exportOrder.ExportSlotCode))
        {
            return;
        }
        // 查询出库储位信息
        var slot = await _repos.Slot.GetFirstAsync(x => x.SlotCode == exportOrder.ExportSlotCode && x.IsDelete == false);
        // 如果储位状态不为4（锁定状态），则设置为0（空闲状态）
        if (slot != null && slot.SlotStatus != 4)
        {
            slot.SlotStatus = 0;
            slot.UpdateTime = now;
            await _repos.Slot.AsUpdateable(slot)
                .UpdateColumns(x => new { x.SlotStatus, x.UpdateTime })
                .ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 推进出库流水状态并累计完成数量
    /// </summary>
    /// <param name="exportOrder">出库流水信息</param>
    /// <param name="totalScanQty">本次扫描总数量</param>
    /// <param name="now">当前时间</param>
    private async Task UpdateExportOrderAsync(WmsExportOrder exportOrder, decimal totalScanQty, DateTime now)
    {
        exportOrder.ExportExecuteFlag = 2;//设置执行标志
        exportOrder.ScanQuantity = (exportOrder.ScanQuantity ?? 0) + totalScanQty;//设置扫描数量
        exportOrder.CompleteDate = now;//设置完成时间
        exportOrder.UpdateTime = now;//设置更新时间
        await _repos.ExportOrder.AsUpdateable(exportOrder).UpdateColumns(x => new { x.ExportExecuteFlag, x.ScanQuantity, x.CompleteDate, x.UpdateTime }).ExecuteCommandAsync();
    }
    /// <summary>
    /// 依据物料编码、批次汇总 PDA 扫描数量
    /// </summary>
    /// <param name="details">拆垛明细集合</param>
    /// <param name="materialCode">物料编码</param>
    /// <param name="lotNo">批次号</param>
    /// <returns>扫描数量汇总</returns>
    private static decimal CalculateManualScanSum(IEnumerable<ManualTaskDetail> details, string materialCode, string lotNo)
    {
        var query = details;//设置查询
        if (!string.IsNullOrWhiteSpace(materialCode))//判断物料编码是否存在
        {
            query = query.Where(x => string.Equals(x.MaterialCode, materialCode, StringComparison.OrdinalIgnoreCase));//设置查询
        }
        if (!string.IsNullOrWhiteSpace(lotNo))//判断批次号是否存在
        {
            query = query.Where(x => string.Equals(x.StockLotNo, lotNo, StringComparison.OrdinalIgnoreCase));//设置查询
        }
        return query.Sum(x => x.EffectiveScanQty);//返回扫描数量
    }
    #endregion
}
