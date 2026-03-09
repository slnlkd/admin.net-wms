// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护使用本项目应遵守相关法律法规和许可证的要求
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证(版本 2.0)进行分发和使用许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动!任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任!

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.BaseService;
using Admin.NET.Application.Service.WmsPda.Dto;
using Admin.NET.Application.Service.WmsSqlView;
using Admin.NET.Core;

using Furion.FriendlyException;

using Microsoft.Extensions.Logging;

using SqlSugar;

namespace Admin.NET.Application.Service.WmsPda.Process;

/// <summary>
/// PDA入库箱托关系管理业务处理类
/// <para>承载 JC35 <c>PdaInterfaceController</c> 箱托关系管理相关接口迁移，提供箱托绑定、解绑等业务功能</para>
/// </summary>
/// <remarks>
/// 主要功能包括：
/// <list type="bullet">
/// <item><description>箱码查询：查询箱码组托信息</description></item>
/// <item><description>箱托绑定：绑定箱码与托盘关系</description></item>
/// <item><description>托盘检查：检查托盘状态</description></item>
/// <item><description>箱托解绑：删除箱托绑定关系</description></item>
/// </list>
/// </remarks>
public class PdaImportBox : PdaImportBase, ITransient
{
    #region 字段定义
    private readonly ILogger _logger;                            //日志
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖
    /// </summary>
    public PdaImportBox(
        ILoggerFactory loggerFactory,
        ISqlSugarClient sqlSugarClient,
        PdaBaseRepos repos,
        WmsSqlViewService sqlViewService) : base(sqlSugarClient, repos, sqlViewService)
    {
        _logger = loggerFactory.CreateLogger(CommonConst.PdaImportBox);
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 根据箱码获取组托信息
    /// <para>对应 JC35 接口：【/PdaInterface/GetImportInfoByBoxCode】</para>
    /// </summary>
    /// <param name="input">查询箱码信息的请求参数，包含箱码、托盘编码、入库单号等</param>
    /// <returns>箱码组托信息查询结果，包含状态、箱码明细和相关信息</returns>
    /// <exception cref="Exception">当参数验证失败、托盘占用、单据状态异常时抛出异常</exception>
    /// <remarks>
    /// 核心流程：
    /// <list type="number">
    /// <item><description>校验托盘是否处于占用状态</description></item>
    /// <item><description>校验入库/验收/挑浆单据状态</description></item>
    /// <item><description>根据箱码获取有效库存箱码记录</description></item>
    /// <item><description>若箱码不存在，尝试根据标签打印信息自动补齐</description></item>
    /// <item><description>校验托盘内是否混放不同单据</description></item>
    /// <item><description>构建返回的组托明细信息</description></item>
    /// </list>
    /// 特殊处理：
    /// <list type="bullet">
    /// <item><description>自动补齐：基于标签打印数据生成箱托记录</description></item>
    /// <item><description>异常清理：检测到历史异常数据时自动清理</description></item>
    /// <item><description>重试机制：支持操作重试状态返回</description></item>
    /// <item><description>混放校验：防止不同单据混放同一托盘</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaBoxInfoOutput> QueryBoxInfoAsync(PdaBoxInfoInput input)
    {
        // 验证输入参数完整性
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.BoxCode))
            throw Oops.Bah("箱码不能为空！");
        // 规范化箱码和托盘编码（去除首尾空格）
        var normalizedBox = input.BoxCode.Trim();
        var normalizedPallet = string.IsNullOrWhiteSpace(input.PalletCode)
            ? null
            : input.PalletCode.Trim();
        // 初始化返回结果对象
        var output = new PdaBoxInfoOutput
        {
            State = PdaBoxInfoState.None, // 初始状态为无状态
            Box = null
        };
        try
        {
            // 校验托盘是否已被占用（已有库存托盘记录说明托盘已组托）
            var stockTray = await _repos.StockTray.AsQueryable()
                .Where(x => x.StockCode == normalizedPallet && x.IsDelete == false)
                .FirstAsync();
            if (stockTray != null)
            {
                // 【新增】检查是否已分配库位（已上架不能续码）
                if (!string.IsNullOrWhiteSpace(stockTray.StockSlotCode))
                {
                    throw Oops.Bah("托盘码重复不能继续码垛！");
                }
                // 如果库存托盘未分配库位，允许继续绑定（人工码垛续码场景）
            }
            // 校验入库/验收/挑浆单据状态是否允许继续绑定
            await ValidateImportStateAsync(input);
            // 根据箱码查询有效的箱托记录（仅查询未删除且未入库的数据，Status=0表示待绑定）
            var boxRecord = await _repos.StockSlotBoxInfo.AsQueryable()
                .Where(x => x.BoxCode == normalizedBox)
                .Where(x => x.IsDelete == false && (x.Status == null || x.Status == 0))
                .OrderBy(x => x.CreateTime, OrderByType.Asc) // 按创建时间升序，获取最早的记录
                .FirstAsync();
            // 如果箱码不存在，尝试根据标签打印信息自动补齐
            if (boxRecord == null)
            {
                // 调用自动补齐逻辑，基于标签打印数据生成箱托记录
                var (Success, RequiresRetry, Box, Message) = await TryCreateBoxFromLabelAsync(input, normalizedPallet);
                if (!Success)
                {
                    output.Success = false;
                    output.Message = Message ?? "箱码有误！";
                    // 根据补齐结果设置状态：需要重试或需要打印标签
                    output.State = RequiresRetry ? PdaBoxInfoState.Retry : PdaBoxInfoState.RequireLabel;
                    output.ExtraMessage = Message;
                    return output;
                }
                boxRecord = Box;
            }
            // 检测历史异常数据：箱码没有托盘编码，但PDA传入了托盘编码（说明数据异常）
            if (string.IsNullOrWhiteSpace(boxRecord.StockCode) && !string.IsNullOrWhiteSpace(normalizedPallet))
            {
                // 软删除异常记录，提示用户重新扫描
                await MarkBoxAsDeletedAsync(boxRecord, input.User);
                output.Success = false;
                output.Message = "请重试";
                output.State = PdaBoxInfoState.Retry;
                output.ExtraMessage = "检测到历史箱托数据异常，已清理原始记录，请重新扫描";
                return output;
            }
            // 特殊场景：ImportId="-1"表示仅查询箱码信息，此时如果箱码已组托则报错
            if (string.Equals(input.ImportId?.Trim(), "-1", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(boxRecord.StockCode))
                throw Oops.Bah("此箱码已组托！");
            // 验证箱码状态：Status=1表示已申请入库，Status=2表示已入库，都不允许再次绑定
            if (boxRecord.Status == 1 || boxRecord.Status == 2)
                throw Oops.Bah("此箱码已申请入库或已入库！");
            // 验证托盘一致性：箱码已绑定其他托盘，需要先解绑
            if (!string.IsNullOrWhiteSpace(boxRecord.StockCode) && !string.IsNullOrWhiteSpace(normalizedPallet) && !string.Equals(boxRecord.StockCode, normalizedPallet, StringComparison.OrdinalIgnoreCase))
                throw Oops.Bah($"箱码已存在于 ({boxRecord.StockCode}) 载具上，请先解绑后再操作");
            // 校验托盘内是否混放不同单据的箱码（一个托盘只能属于一个入库单）
            await ValidatePalletMixingAsync(normalizedPallet, boxRecord);
            // 构建PDA展示的箱托明细信息（包含物料编码、单据号等）
            var detail = await BuildBoxDetailAsync(boxRecord, normalizedPallet);
            output.Success = true;
            output.Message = "绑定成功";
            output.State = PdaBoxInfoState.Ready; // 状态Ready表示可以进行绑定
            output.Box = detail;
            return output;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询箱托信息失败：{@Input}", input);
            // 捕获所有异常，统一返回失败结果
            output.Success = false;
            output.Message = ex.Message;
            output.State = PdaBoxInfoState.None; // 异常时状态设为None
            output.Box = null;
            return output;
        }
    }
    /// <summary>
    /// 判断托盘是否为有效托盘
    /// </summary>
    /// <param name="palletCode">托盘编码</param>
    /// <returns>托盘是否存在且未删除</returns>
    public async Task<bool> CheckPalletStatusAsync(string palletCode)
    {
        // 托盘编码为空，返回无效
        if (string.IsNullOrWhiteSpace(palletCode))
            return false;
        // 查询托盘主数据是否存在且未删除（托盘必须在系统中注册才有效）
        return await _repos.SysStockCode.AsQueryable()
            .Where(x => x.StockCode == palletCode.Trim() && x.IsDelete == false)
            .AnyAsync();
    }
    /// <summary>
    /// 绑定箱托关系
    /// <para>对应 JC35 接口：【/PdaInterface/BackConfirm】</para>
    /// </summary>
    /// <param name="input">绑定箱托关系的请求参数，包含箱码、托盘编码、重量、拒绝原因等</param>
    /// <returns>绑定操作结果，包含成功状态和消息</returns>
    /// <exception cref="Exception">当参数验证失败、箱码不存在、绑定失败时抛出异常</exception>
    /// <remarks>
    /// 版本说明：
    /// <list type="bullet">
    /// <item><description>当前版本仅支持标准入库场景(type 空/0)</description></item>
    /// <item><description>验收与挑浆功能将在后续迭代补充</description></item>
    /// </list>
    /// 绑定流程：
    /// <list type="number">
    /// <item><description>验证输入参数和业务类型</description></item>
    /// <item><description>查询并验证箱码组托信息</description></item>
    /// <item><description>验证订单号一致性</description></item>
    /// <item><description>确保托盘存在待执行的入库流水</description></item>
    /// <item><description>更新箱托与托盘、入库流水的绑定关系</description></item>
    /// <item><description>更新入库明细与主单的执行状态和已组数量</description></item>
    /// </list>
    /// 数据更新：
    /// <list type="bullet">
    /// <item><description>箱码：更新托盘关联、入库流水、重量等信息</description></item>
    /// <item><description>托盘：更新使用状态为已占用</description></item>
    /// <item><description>入库流水：累加数量和重量</description></item>
    /// <item><description>入库明细：更新实际组托数量和执行状态</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> BindBoxAsync(PdaBindBoxInput input)
    {
        // 验证输入参数完整性
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.BoxCode))
            throw Oops.Bah("箱码不能为空！");
        if (string.IsNullOrWhiteSpace(input.PalletCode))
            throw Oops.Bah("托盘编码不能为空！");
        // 获取业务场景类型（0=标准入库, 1=验收, 3/4=挑浆）
        var scenario = string.IsNullOrWhiteSpace(input.Type) ? "0" : input.Type.Trim();
        // 当前版本仅支持标准入库场景
        if (!string.Equals(scenario, "0", StringComparison.OrdinalIgnoreCase))
            throw Oops.Bah("当前版本暂未迁移验收/挑浆绑定逻辑，请联系系统管理员");
        var normalizedPallet = input.PalletCode.Trim();
        // 先查询箱码组托信息，验证箱码是否可以绑定
        var boxInfoResult = await QueryBoxInfoAsync(input);
        if (!boxInfoResult.Success || boxInfoResult.Box == null)
            return new PdaActionResult
            {
                Success = false,
                Message = boxInfoResult.Message
            };
        // 验证箱码状态是否为Ready（可以绑定）
        if (boxInfoResult.State != PdaBoxInfoState.Ready)
            return new PdaActionResult
            {
                Success = false,
                Message = boxInfoResult.ExtraMessage ?? boxInfoResult.Message
            };
        // 重新查询箱码实体（确保数据最新）
        var boxEntity = await _repos.StockSlotBoxInfo.GetFirstAsync(x => x.Id == boxInfoResult.Box.BoxId && x.IsDelete == false) ?? throw Oops.Bah("箱码数据不存在，请重新扫描！");
        // 验证箱码所属单据是否与PDA传入的订单号一致
        await ValidateOrderConsistencyAsync(boxEntity, input);
        var user = NormalizeUser(input.User);
        var now = DateTime.Now;
        WmsSysStockCode? palletEntity = null;
        WmsImportOrder? importOrder = null;

        // 如果传入了有效数量，先更新 boxEntity.Qty，以便后续逻辑使用正确的数量
        if (!string.IsNullOrWhiteSpace(input.Qty) && decimal.TryParse(input.Qty, out var inputQty) && inputQty > 0)
        {
            boxEntity.Qty = inputQty;
        }

        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                // 获取托盘主数据
                palletEntity = await _repos.SysStockCode.GetFirstAsync(x => x.StockCode == normalizedPallet && x.IsDelete == false);
                // 保证托盘存在对应的入库流水记录
                importOrder = await EnsureImportOrderAsync(palletEntity, boxEntity, input, now, user);
                // 更新箱托绑定关系并刷新托盘状态
                await UpdateBoxBindingAsync(boxEntity, palletEntity, importOrder, input, now, user);
                // 更新入库明细与主单的执行状态/已组数
                await UpdateImportQuantitiesAsync(boxEntity, now, user);
            }, "绑定箱托失败");
            _logger.LogInformation("绑定箱托成功：BoxCode={BoxCode}, Pallet={Pallet}, ImportOrder={Order}", boxEntity.BoxCode, palletEntity?.StockCode, importOrder?.ImportOrderNo);
            return new PdaActionResult
            {
                Success = true,
                Message = "操作成功"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "绑定箱托失败：{@Input}", input);
            throw;
        }
    }

    /// <summary>
    /// 删除单个箱码绑定
    /// <para>对应 JC35 接口：【DelStockSoltBoxInfo】</para>
    /// </summary>
    /// <param name="input">删除箱码绑定的请求参数，包含箱码主键ID和操作用户</param>
    /// <returns>删除操作结果，包含成功状态和消息</returns>
    /// <exception cref="Exception">当参数验证失败、箱码不存在或已处理时抛出异常</exception>
    /// <remarks>
    /// 删除规则：
    /// <list type="number">
    /// <item><description>仅删除 Status=0 的箱托关系，避免已入库数据被清除</description></item>
    /// <item><description>当托盘无其余箱码时，软删除入库流水并释放储位/托盘状态</description></item>
    /// <item><description>回滚入库明细的已组数量，必要时将主单恢复为"待执行"</description></item>
    /// </list>
    /// 业务影响：
    /// <list type="bullet">
    /// <item><description>减少入库明细的实际组托数量</description></item>
    /// <item><description>可能触发入库单状态回退</description></item>
    /// <item><description>释放托盘占用状态</description></item>
    /// <item><description>释放相关储位状态</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> DeletePalletBoxAsync(PdaDelBoxInput input)
    {
        // 判断输入是否为空
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        // 箱码主键
        var boxId = ParseRequiredLong(input.BoxId, "箱码主键");
        // 查询入库箱信息
        var target = await _repos.StockSlotBoxInfo.GetFirstAsync(x =>
            x.Id == boxId && x.IsDelete == false && x.Status == 0) ?? throw Oops.Bah("箱托关系不存在或已处理！");
        // 查询入库箱信息
        var boxes = await _repos.StockSlotBoxInfo.AsQueryable()
            .Where(x => x.IsDelete == false && x.Status == 0 && (x.Id == boxId || x.StockCodeId == target.StockCodeId))
            .ToListAsync();
        // 处理箱托关系解绑
        return await ProcessRemoveBoxAsync(boxes, target, input.User, string.Empty);
    }

    /// <summary>
    /// 批量入库确认
    /// <para>支持批量绑定新箱子 + 批量更新已绑定箱子的数量</para>
    /// </summary>
    /// <param name="input">批量确认参数</param>
    /// <returns>批量确认结果</returns>
    /// <remarks>
    /// 核心流程：
    /// <list type="number">
    /// <item><description>遍历待绑定箱子列表，调用现有绑定逻辑</description></item>
    /// <item><description>遍历已修改数量的箱子，更新数量并重新计算入库明细</description></item>
    /// <item><description>汇总成功和失败的结果</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaBatchImportConfirmOutput> BatchImportConfirmAsync(PdaBatchImportConfirmInput input)
    {
        // 验证输入参数
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.PalletCode))
            throw Oops.Bah("托盘编码不能为空！");

        var output = new PdaBatchImportConfirmOutput
        {
            Success = true,
            Message = "操作成功",
            BindSuccessCount = 0,
            UpdateSuccessCount = 0,
            FailedItems = new List<PdaBatchFailedItem>()
        };

        var user = NormalizeUser(input.User);
        var now = DateTime.Now;
        var normalizedPallet = input.PalletCode.Trim();

        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                // 1. 处理待绑定的新箱子
                if (input.PendingBoxes != null && input.PendingBoxes.Count > 0)
                {
                    foreach (var pendingBox in input.PendingBoxes)
                    {
                        try
                        {
                            var bindInput = new PdaBindBoxInput
                            {
                                PalletCode = normalizedPallet,
                                BoxCode = pendingBox.BoxCode,
                                ImportId = input.ImportId,
                                OrderNo = input.OrderNo,
                                User = input.User,
                                Type = "0", // 标准入库
                                Qty = pendingBox.Qty.ToString()
                            };

                            var bindResult = await BindBoxAsync(bindInput);
                            if (bindResult.Success)
                            {
                                output.BindSuccessCount++;
                            }
                            else
                            {
                                output.FailedItems.Add(new PdaBatchFailedItem
                                {
                                    BoxCode = pendingBox.BoxCode,
                                    Reason = bindResult.Message
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            output.FailedItems.Add(new PdaBatchFailedItem
                            {
                                BoxCode = pendingBox.BoxCode,
                                Reason = ex.Message
                            });
                        }
                    }
                }

                // 2. 处理已修改数量的箱子
                if (input.ModifiedBoxes != null && input.ModifiedBoxes.Count > 0)
                {
                    foreach (var kvp in input.ModifiedBoxes)
                    {
                        var boxCode = kvp.Key;
                        var newQty = kvp.Value;

                        try
                        {
                            await UpdateBoxQuantityAsync(boxCode, normalizedPallet, newQty, now, user);
                            output.UpdateSuccessCount++;
                        }
                        catch (Exception ex)
                        {
                            output.FailedItems.Add(new PdaBatchFailedItem
                            {
                                BoxCode = boxCode,
                                Reason = ex.Message
                            });
                        }
                    }
                }
            }, "批量入库确认失败");

            // 设置最终结果
            if (output.FailedItems.Count > 0)
            {
                output.Success = output.BindSuccessCount > 0 || output.UpdateSuccessCount > 0;
                output.Message = $"操作完成：绑定成功 {output.BindSuccessCount} 个，更新成功 {output.UpdateSuccessCount} 个，失败 {output.FailedItems.Count} 个";
            }
            else
            {
                output.Message = $"操作成功：绑定 {output.BindSuccessCount} 个，更新 {output.UpdateSuccessCount} 个";
            }

            _logger.LogInformation("批量入库确认完成：Pallet={Pallet}, BindSuccess={BindSuccess}, UpdateSuccess={UpdateSuccess}, Failed={Failed}",
                normalizedPallet, output.BindSuccessCount, output.UpdateSuccessCount, output.FailedItems.Count);

            return output;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量入库确认失败：{@Input}", input);
            output.Success = false;
            output.Message = ex.Message;
            return output;
        }
    }

    /// <summary>
    /// 更新箱码数量
    /// </summary>
    /// <param name="boxCode">箱码</param>
    /// <param name="palletCode">托盘编码</param>
    /// <param name="newQty">新数量</param>
    /// <param name="now">当前时间</param>
    /// <param name="user">操作用户</param>
    private async Task UpdateBoxQuantityAsync(string boxCode, string palletCode, decimal newQty, DateTime now, string user)
    {
        // 查询箱码记录
        var boxRecord = await _repos.StockSlotBoxInfo.GetFirstAsync(x =>
            x.BoxCode == boxCode.Trim() &&
            x.StockCode == palletCode &&
            x.IsDelete == false &&
            (x.Status == null || x.Status == 0));

        if (boxRecord == null)
            throw Oops.Bah($"箱码 {boxCode} 在托盘 {palletCode} 上不存在！");

        // 计算数量差值
        var oldQty = boxRecord.Qty ?? 0;
        var qtyDelta = newQty - oldQty;

        if (qtyDelta == 0)
            return; // 数量未变化，无需更新

        // 【超入/少入验证】在更新数量前验证仓库配置
        if (boxRecord.ImportDetailId.HasValue)
        {
            var detail = await _repos.ImportNotifyDetail.GetFirstAsync(x => x.Id == boxRecord.ImportDetailId.Value && x.IsDelete == false);
            if (detail != null)
            {
                var planQuantity = detail.ImportQuantity ?? 0;
                var currentFactQuantity = detail.ImportFactQuantity ?? 0;
                var newFactQuantity = currentFactQuantity + qtyDelta;
                if (newFactQuantity < 0) newFactQuantity = 0;

                // 获取仓库配置
                WmsBaseWareHouse? warehouse = null;
                if (boxRecord.ImportId.HasValue)
                {
                    var notify = await _repos.ImportNotify.GetFirstAsync(x => x.Id == boxRecord.ImportId.Value && x.IsDelete == false);
                    if (notify?.WarehouseId != null)
                    {
                        warehouse = await _repos.Warehouse.GetFirstAsync(x => x.Id == notify.WarehouseId && x.IsDelete == false);
                    }
                }

                // 获取物料名称（用于错误提示）
                var materialName = boxRecord.MaterialId.HasValue
                    ? (await _repos.Material.GetFirstAsync(x => x.Id == boxRecord.MaterialId.Value))?.MaterialName ?? "未知物料"
                    : "未知物料";

                // 【超入验证】如果更新后实际数量超过计划数量，检查仓库是否允许超入
                if (newFactQuantity > planQuantity)
                {
                    if (!(warehouse?.IsOverbooking ?? false))
                    {
                        throw Oops.Bah($"物料【{materialName}】超入：计划入库 {planQuantity}，更新后将为 {newFactQuantity}，超出计划 {newFactQuantity - planQuantity}。当前仓库不允许超入，请联系管理员！");
                    }
                }

                // 【少入验证】如果更新后实际数量少于计划数量，检查仓库是否允许少入
                // 注意：少入验证通常在关单时检查，但如果用户主动减少数量，也需要提示
                if (newFactQuantity < planQuantity && qtyDelta < 0)
                {
                    if (!(warehouse?.IsEnterless ?? false))
                    {
                        // 少入仅作为警告记录日志，不阻止操作（关单时会最终验证）
                        _logger.LogWarning("物料【{MaterialName}】可能少入：计划入库 {PlanQty}，更新后将为 {NewFactQty}，少于计划 {Diff}。当前仓库不允许少入。",
                            materialName, planQuantity, newFactQuantity, planQuantity - newFactQuantity);
                    }
                }
            }
        }

        // 更新箱码数量
        boxRecord.Qty = newQty;
        boxRecord.UpdateTime = now;
        boxRecord.UpdateUserName = user;

        await _repos.StockSlotBoxInfo.AsUpdateable(boxRecord)
            .UpdateColumns(x => new { x.Qty, x.UpdateTime, x.UpdateUserName })
            .ExecuteCommandAsync();

        // 更新入库流水的数量
        if (boxRecord.ImportOrderId.HasValue)
        {
            var importOrder = await _repos.ImportOrder.GetFirstAsync(x => x.Id == boxRecord.ImportOrderId.Value && x.IsDelete == false);
            if (importOrder != null)
            {
                importOrder.ImportQuantity = (importOrder.ImportQuantity ?? 0) + qtyDelta;
                importOrder.UpdateTime = now;
                importOrder.UpdateUserName = user;
                await _repos.ImportOrder.AsUpdateable(importOrder)
                    .UpdateColumns(x => new { x.ImportQuantity, x.UpdateTime, x.UpdateUserName })
                    .ExecuteCommandAsync();
            }
        }

        // 更新入库明细的实际组托数量
        if (boxRecord.ImportDetailId.HasValue)
        {
            var detail = await _repos.ImportNotifyDetail.GetFirstAsync(x => x.Id == boxRecord.ImportDetailId.Value && x.IsDelete == false);
            if (detail != null)
            {
                var newFactQuantity = (detail.ImportFactQuantity ?? 0) + qtyDelta;
                if (newFactQuantity < 0)
                    newFactQuantity = 0;

                detail.ImportFactQuantity = newFactQuantity;
                detail.UpdateTime = now;
                detail.UpdateUserName = user;

                await _repos.ImportNotifyDetail.AsUpdateable(detail)
                    .UpdateColumns(x => new { x.ImportFactQuantity, x.UpdateTime, x.UpdateUserName })
                    .ExecuteCommandAsync();
            }
        }

        _logger.LogInformation("更新箱码数量：BoxCode={BoxCode}, OldQty={OldQty}, NewQty={NewQty}, Delta={Delta}",
            boxCode, oldQty, newQty, qtyDelta);
    }

    /// <summary>
    /// JC35【RemoveBoxHolds】迁移实现：托盘箱码解绑
    /// </summary>
    /// <param name="input">解绑参数</param>
    /// <returns>操作结果</returns>
    /// <remarks>
    /// <list type="number">
    /// <item>支持按托盘或入库单解绑(与 JC35 参数一致)</item>
    /// <item>默认回滚入库明细数量，`type=ys` 场景仅解绑托盘</item>
    /// <item>若托盘仅剩一个箱码，会同步释放入库流水、储位与托盘状态</item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> RemoveBoxHoldsAsync(PdaRemoveBoxInput input)
    {
        // 判断输入是否为空
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        // 查询入库箱信息
        var query = _repos.StockSlotBoxInfo.AsQueryable()
            .Where(x => x.IsDelete == false &&
                x.Status == 0 &&
                x.ImportOrderId != null &&
                x.ImportOrderId != 0);
        // 判断托盘编码是否为空
        bool hasPallet = !string.IsNullOrWhiteSpace(input.PalletCode);
        // 判断入库单号是否为空
        bool hasImport = !string.IsNullOrWhiteSpace(input.ImportId);
        // 判断托盘编码是否为空
        if (hasPallet)
        {
            // 托盘编码
            var palletCode = input.PalletCode.Trim();
            query = query.Where(x => x.StockCode == palletCode);
        }
        // 判断入库单号是否为空
        else if (hasImport)
        {
            // 入库单号
            var importId = ParseRequiredLong(input.ImportId, "入库单信息");
            query = query.Where(x => x.ImportId == importId);
        }
        else
        {
            throw Oops.Bah("托盘编码或入库单至少提供一项！");
        }
        var boxes = await query.ToListAsync();
        if (boxes.Count == 0)
            throw Oops.Bah("未找到可解绑的箱托关系！");
        WmsStockSlotBoxInfo target;
        // 判断箱码是否为空
        if (string.IsNullOrWhiteSpace(input.BoxCode))
        {
            target = boxes.First();
        }
        else
        {
            // 查询入库箱信息
            target = boxes.FirstOrDefault(x =>
                string.Equals(x.BoxCode ?? string.Empty, input.BoxCode.Trim(), StringComparison.OrdinalIgnoreCase));
            // 判断箱码是否存在
            if (target == null)
                throw Oops.Bah("目标箱码不存在！");
        }
        // 处理箱托关系解绑
        return await ProcessRemoveBoxAsync(boxes, target, input.User, input.Type);
    }
    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 校验入库/验收/挑浆单据状态，确保仍允许绑定操作
    /// </summary>
    /// <param name="input">PDA 扫码入参</param>
    private async Task ValidateImportStateAsync(PdaBoxInfoInput input)
    {
        var rawImport = input.ImportId?.Trim();
        // ImportId为空或为"-1"表示不验证单据状态（特殊场景）
        if (string.IsNullOrWhiteSpace(rawImport) || string.Equals(rawImport, "-1", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        // 获取业务场景类型
        var scenario = string.IsNullOrWhiteSpace(input.Type) ? "0" : input.Type.Trim();
        // 挑浆单业务（场景3和4）暂未迁移
        if (string.Equals(scenario, "3", StringComparison.OrdinalIgnoreCase) || string.Equals(scenario, "4", StringComparison.OrdinalIgnoreCase))
        {
            throw Oops.Bah("挑浆单业务暂未迁移，请联系系统管理员");
        }
        // 【场景1：验收单】验证验收单状态
        if (string.Equals(scenario, "1", StringComparison.OrdinalIgnoreCase))
        {
            var inspectQuery = _repos.InspectNotify.AsQueryable().Where(x => x.IsDelete == false);
            // 尝试解析为ID，如果失败则作为单据号查询
            var inspectId = TryParseLong(rawImport);
            if (inspectId.HasValue)
            {
                inspectQuery = inspectQuery.Where(x => x.Id == inspectId.Value);
            }
            else
            {
                inspectQuery = inspectQuery.Where(x => x.InspectBillCode == rawImport);
            }
            var inspect = await inspectQuery.Select(x => new { x.Id, x.Status }).FirstAsync();
            if (inspect == null)
            {
                throw Oops.Bah("验收单不存在!");
            }
            // Status=2表示验收单已完成，不允许继续绑定
            if (inspect.Status == 2)
            {
                throw Oops.Bah("验收单已完成!");
            }
            return;
        }
        // 【场景0：标准入库单】验证入库单状态
        var importQuery = _repos.ImportNotify.AsQueryable().Where(x => x.IsDelete == false);
        // 尝试解析为ID，如果失败则作为单据号查询
        var importId = TryParseLong(rawImport);
        if (importId.HasValue)
        {
            importQuery = importQuery.Where(x => x.Id == importId.Value);
        }
        else
        {
            importQuery = importQuery.Where(x => x.ImportBillCode == rawImport);
        }
        var notify = await importQuery.Select(x => new { x.Id, x.ImportExecuteFlag }).FirstAsync();
        if (notify == null)
            throw Oops.Bah("入库单不存在!");
        // ImportExecuteFlag="03"表示入库单已完成，不允许继续绑定
        if (string.Equals(notify.ImportExecuteFlag, "03", StringComparison.OrdinalIgnoreCase))
            throw Oops.Bah("入库单已完成!");
    }

    /// <summary>
    /// 尝试基于标签打印数据自动补齐箱托记录
    /// </summary>
    /// <param name="input">PDA 扫码入参</param>
    /// <param name="palletCode">待绑定托盘编码</param>
    /// <returns>补齐结果、是否需重试及补齐后的箱托</returns>
    private async Task<(bool Success, bool RequiresRetry, WmsStockSlotBoxInfo Box, string? Message)> TryCreateBoxFromLabelAsync(PdaBoxInfoInput input, string? palletCode)
    {
        // 场景
        var scenario = string.IsNullOrWhiteSpace(input.Type) ? "0" : input.Type.Trim();
        if (string.Equals(scenario, "3", StringComparison.OrdinalIgnoreCase) || string.Equals(scenario, "4", StringComparison.OrdinalIgnoreCase))
        {
            return (false, false, null, "挑浆单业务暂未迁移，请联系系统管理员");
        }
        // 查询标签打印信息
        var label = await _repos.ImportLabelPrint.AsQueryable()
            .Where(x => x.LabelID == input.BoxCode.Trim())
            .Where(x => x.IsDelete == false)
            .OrderBy(x => x.CreateTime, OrderByType.Desc)
            .Select(x => new
            {
                x.Id,              // ID
                x.LabelID,         // 标签ID
                x.ImportId,        // 入库单据ID
                x.ImportDetailId,  // 入库单据明细ID
                x.Quantity,        // 数量
                x.LotNo,           // 批次号
                x.MxFlag           // 标记
            }).FirstAsync();
        // 箱码有误
        if (label == null || string.IsNullOrWhiteSpace(label.LabelID))
            return (false, false, null, "箱码有误!");
        var detail = await _repos.ImportNotifyDetail.GetFirstAsync(x => x.Id == label.ImportDetailId && x.IsDelete == false);
        if (detail == null)
            return (false, false, null, "未找到入库单明细，无法生成箱码信息！");
        // 入库单据ID
        long? importId = label.ImportId ?? detail.ImportId;
        // 判断场景是否为1
        if (string.Equals(scenario, "1", StringComparison.OrdinalIgnoreCase))
        {
            // 查询验收通知
            var inspect = await _repos.InspectNotify.AsQueryable()
                .Where(x => x.ImportDetailId == detail.Id && x.IsDelete == false)
                .Select(x => new { x.Id, x.Status })
                .FirstAsync();
            if (inspect == null)
                return (false, false, null, "验收单不存在!");
            if (inspect.Status == 2)
                return (false, false, null, "验收单已完成!");
            importId = inspect.Id;
        }
        WmsSysStockCode? palletEntity = null;
        // 判断托盘编码是否为空
        if (!string.IsNullOrWhiteSpace(palletCode))
        {
            // 查询托盘信息
            palletEntity = await _repos.SysStockCode.GetFirstAsync(x =>
                x.StockCode == palletCode && x.IsDelete == false);
            if (palletEntity == null)
                throw Oops.Bah("当前扫描的托盘有误！");
        }
        // 当前时间
        var now = DateTime.Now;
        var user = NormalizeUser(input.User);
        // 检验状态
        var inspectionStatus = 0;
        // 判断物料状态是否为空
        if (!string.IsNullOrWhiteSpace(detail.MaterialStatus) &&
            int.TryParse(detail.MaterialStatus, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedStatus))
        {
            // 解析物料状态
            inspectionStatus = parsedStatus;
        }

        // 查询入库单据获取供应商和制造商信息
        WmsImportNotify? notify = null;
        if (detail?.ImportId != null)
        {
            notify = await _repos.ImportNotify.GetFirstAsync(n => n.Id == detail.ImportId && n.IsDelete == false);
        }
        decimal.TryParse(input.Qty, out decimal inputQty);
        var newBox = new WmsStockSlotBoxInfo
        {
            Id = SnowFlakeSingle.Instance.NextId(),                              // 生成ID
            StockCodeId = palletEntity?.Id,                                      // 库存编码ID
            StockCode = palletEntity?.StockCode,                                 // 库存编码
            BoxCode = label.LabelID,                                             // 箱码
            //Qty = decimal.TryParse(input.Qty, out var inputQty) ? inputQty : 0,// 数量
            Qty = label.Quantity ?? 0,                                           // 数量
            FullBoxQty = label.Quantity ?? 0,                                    // 满箱数量
            CreateUserName = user,                                               // 创建用户
            CreateTime = now,                                                    // 创建时间
            UpdateUserName = user,                                               // 更新用户
            UpdateTime = now,                                                    // 更新时间
            Status = 0,                                                          // 状态
            BoxLevel = 1,                                                        // 箱级
            ProductionDate = detail.ImportProductionDate,                        // 生产日期
            ValidateDay = detail.ImportLostDate,                                 // 失效日期
            LotNo = label.LotNo ?? detail.LotNo,                                 // 批次号
            MaterialId = detail.MaterialId,                                      // 物料ID
            SupplierId = notify?.SupplierId,                                     // 供应商ID（从入库单据获取）
            ManufacturerId = notify?.ManufacturerId,                             // 制造商ID（从入库单据获取）
            //BulkTank = inputQty < label.Quantity ? 1 : 0,                      // 零箱标识
            BulkTank = label.MxFlag ?? 0,                                        // 零箱标识
            ImportId = importId,                                                 // 入库单据ID
            ImportDetailId = detail.Id,                                          // 入库单据明细ID
            IsDelete = false,                                                    // 是否删除
            QRCode = input.BoxCode.Trim(),                                       // 二维码
            InspectionStatus = inspectionStatus                                  // 检验状态
        };
        if (!await ValidateMaterialMatchAsync(newBox))
        {
            return (false, false, null, "箱码与订单号不匹配!");
        }
        // 插入箱托信息
        await _repos.StockSlotBoxInfo.InsertAsync(newBox);
        // 根据标签打印信息自动生成箱托数据
        _logger.LogInformation("根据标签打印信息自动生成箱托数据：BoxCode={BoxCode}, ImportId={ImportId}", newBox.BoxCode, newBox.ImportId);
        // 查询箱托信息
        var persisted = await _repos.StockSlotBoxInfo.GetFirstAsync(x => x.Id == newBox.Id);
        // 返回结果
        return (true, false, persisted, null);
    }

    /// <summary>
    /// 软删除历史异常箱托记录，避免重复绑定
    /// </summary>
    /// <param name="box">目标箱托数据</param>
    /// <param name="user">操作人</param>
    private async Task MarkBoxAsDeletedAsync(WmsStockSlotBoxInfo box, string? user)
    {
        // 当前时间
        var now = DateTime.Now;
        // 操作人
        var operatorName = NormalizeUser(user);
        // 更新箱托信息
        await _repos.StockSlotBoxInfo.AsUpdateable()
            .SetColumns(x => new WmsStockSlotBoxInfo
            {
                IsDelete = true,        // 是否删除
                UpdateTime = now,       // 更新时间
                UpdateUserName = operatorName  // 更新用户
            }).Where(x => x.Id == box.Id).ExecuteCommandAsync();
    }

    /// <summary>
    /// 构建 PDA 端展示的箱托明细信息
    /// </summary>
    /// <param name="box">数据库箱托实体</param>
    /// <param name="fallbackPallet">缺失托盘时使用的回退编码</param>
    /// <returns>封装后的明细</returns>
    private async Task<PdaBoxInfoDetail> BuildBoxDetailAsync(WmsStockSlotBoxInfo box, string? fallbackPallet)
    {
        // 查询物料信息（优化为一次查询同时获取编码和名称）
        string materialCode = string.Empty;
        string materialName = string.Empty;
        // 判断物料ID是否为空
        if (box.MaterialId.HasValue)
        {
            // 查询物料信息
            var material = await _repos.Material.AsQueryable()
                .Where(x => x.Id == box.MaterialId.Value && x.IsDelete == false)
                .Select(x => new { x.MaterialCode, x.MaterialName })
                .FirstAsync();
            materialCode = material?.MaterialCode ?? string.Empty;
            materialName = material?.MaterialName ?? string.Empty;
        }

        // 查询入库明细信息（获取计划数量、实际已组数量、执行状态）
        decimal? importQuantity = null;
        decimal? importFactQuantity = null;
        string importExecuteFlag = null;
        // 判断入库明细ID是否为空
        if (box.ImportDetailId.HasValue)
        {
            // 查询入库明细
            var detail = await _repos.ImportNotifyDetail.AsQueryable()
                .Where(x => x.Id == box.ImportDetailId.Value && x.IsDelete == false)
                .Select(x => new
                {
                    x.ImportQuantity,      // 计划入库数量
                    x.ImportFactQuantity,  // 实际已组数量
                    x.ImportExecuteFlag    // 执行标识
                })
                .FirstAsync();
            if (detail != null)
            {
                importQuantity = detail.ImportQuantity;
                importFactQuantity = detail.ImportFactQuantity;
                importExecuteFlag = detail.ImportExecuteFlag;
            }
        }

        // 获取入库单据号
        var importBillCode = await GetImportBillCodeAsync(box);

        return new PdaBoxInfoDetail
        {
            BoxId = box.Id,                                        // 箱托ID
            BoxCode = box.BoxCode ?? string.Empty,                 // 箱码
            MaterialId = box.MaterialId,                           // 物料ID
            MaterialCode = materialCode,                           // 物料编码
            MaterialName = materialName,                           // 物料名称
            LotNo = box.LotNo ?? string.Empty,                     // 批次号
            Quantity = box.Qty ?? 0,                               // 数量
            PalletCode = !string.IsNullOrWhiteSpace(box.StockCode)
                ? box.StockCode
                : (fallbackPallet ?? string.Empty),                // 托盘编码
            ImportId = box.ImportId,                               // 入库单据ID
            ImportBillCode = importBillCode,                       // 入库单据号
            ImportDetailId = box.ImportDetailId,                   // 入库单据明细ID
            ImportQuantity = importQuantity,                       // 计划入库数量
            ImportFactQuantity = importFactQuantity,               // 实际已组数量
            ImportExecuteFlag = importExecuteFlag,                 // 执行标识
            InspectionStatus = box.InspectionStatus,               // 检验状态
            PickingSlurry = box.PickingSlurry ?? "0",              // 挑浆状态
            ExtractStatus = box.ExtractStatus,                     // 提取状态
            QrCode = box.QRCode ?? string.Empty                    // 二维码
        };
    }

    /// <summary>
    /// 确保托盘存在待执行的入库流水，不存在则自动创建
    /// </summary>
    /// <param name="pallet">托盘主数据</param>
    /// <param name="box">箱托信息</param>
    /// <param name="input">绑定请求</param>
    /// <param name="now">当前时间</param>
    /// <param name="user">操作人</param>
    /// <returns>可用的入库流水</returns>
    private async Task<WmsImportOrder> EnsureImportOrderAsync(WmsSysStockCode pallet, WmsStockSlotBoxInfo box, PdaBindBoxInput input, DateTime now, string user)
    {
        // 查询入库流水
        var orderQuery = _repos.ImportOrder.AsQueryable()
            .Where(x => x.IsDelete == false && x.ImportExecuteFlag == "01" && x.StockCode == pallet.StockCode)
            .OrderBy(x => x.CreateTime, OrderByType.Desc);
        var existingOrder = await orderQuery.FirstAsync();
        // 重量差值
        var weightDelta = input.Weight ?? box.Weight ?? 0.666m;
        if (existingOrder == null)
        {
            var newOrder = new WmsImportOrder
            {
                Id = SnowFlakeSingle.Instance.NextId(),         // 生成ID
                ImportOrderNo = _commonMethod.GetImExNo("RK"),  // 获取入库单据号
                ImportId = box.ImportId,                        // 入库单据ID
                WareHouseId = pallet.WarehouseId,               // 仓库ID
                StockCodeId = pallet.Id,                        // 库存编码ID
                StockCode = pallet.StockCode,                   // 库存编码
                ImportQuantity = box.Qty ?? 0,                  // 入库数量
                ImportWeight = weightDelta,                     // 入库重量
                ImportExecuteFlag = "01",                       // 入库执行状态
                LotNo = box.LotNo,                              // 批次号
                ImportProductionDate = box.ProductionDate,      // 生产日期
                CreateTime = now,                               // 创建时间
                CreateUserName = user,                          // 创建用户
                CreateUserId = 0,                               // 创建用户ID
                UpdateTime = now,                               // 更新时间
                UpdateUserName = user,                          // 更新用户
                UpdateUserId = 0,                               // 更新用户ID
                IsDelete = false,                               // 是否删除
                InspectFlag = 2,                                // 检验标志
                IsTemporaryStorage = "1",                       // 是否临时存储
                SubVehicleCode = pallet.StockCode               // 子车编码
            };
            // 插入入库流水
            await _repos.ImportOrder.InsertAsync(newOrder);
            return newOrder;
        }
        // 入库数量
        existingOrder.ImportQuantity = (existingOrder.ImportQuantity ?? 0) + (box.Qty ?? 0);
        // 入库重量
        existingOrder.ImportWeight = (existingOrder.ImportWeight ?? 0) + weightDelta;
        existingOrder.UpdateTime = now;
        existingOrder.UpdateUserName = user;
        // 更新入库流水
        await _repos.ImportOrder.AsUpdateable(existingOrder)
            .UpdateColumns(x => new { x.ImportQuantity, x.ImportWeight, x.UpdateTime, x.UpdateUserName })
            .ExecuteCommandAsync();
        return existingOrder;
    }

    /// <summary>
    /// 更新箱托与托盘、入库流水的绑定信息
    /// </summary>
    /// <param name="box">箱托信息</param>
    /// <param name="pallet">托盘信息</param>
    /// <param name="importOrder">入库流水</param>
    /// <param name="input">绑定请求</param>
    /// <param name="now">当前时间</param>
    /// <param name="user">操作人</param>
    private async Task UpdateBoxBindingAsync(WmsStockSlotBoxInfo box, WmsSysStockCode pallet, WmsImportOrder importOrder, PdaBindBoxInput input, DateTime now, string user)
    {
        // 重量
        var weight = input.Weight ?? box.Weight ?? 0.666m;
        box.StockCodeId = pallet.Id;
        box.StockCode = pallet.StockCode;
        box.ImportOrderId = importOrder.Id;
        box.Weight = weight;
        box.ReasonsForExcl = input.ReasonsForExcl;
        box.plasmaRejectTypeId = input.RejectTypeId;
        box.UpdateTime = now;
        box.UpdateUserName = user;

        // 如果传入了数量参数，更新箱码数量
        if (!string.IsNullOrWhiteSpace(input.Qty) && decimal.TryParse(input.Qty, out var inputQty) && inputQty > 0)
        {
            box.Qty = inputQty;
            // 更新箱托信息（包含数量）
            await _repos.StockSlotBoxInfo.AsUpdateable(box)
                .UpdateColumns(x => new { x.StockCodeId, x.StockCode, x.ImportOrderId, x.Weight, x.ReasonsForExcl, x.plasmaRejectTypeId, x.Qty, x.UpdateTime, x.UpdateUserName })
                .ExecuteCommandAsync();
        }
        else
        {
            // 更新箱托信息（不含数量）
            await _repos.StockSlotBoxInfo.AsUpdateable(box)
                .UpdateColumns(x => new { x.StockCodeId, x.StockCode, x.ImportOrderId, x.Weight, x.ReasonsForExcl, x.plasmaRejectTypeId, x.UpdateTime, x.UpdateUserName })
                .ExecuteCommandAsync();
        }

        pallet.Status = 1;
        pallet.UpdateTime = now;
        pallet.UpdateUserName = user;
        // 更新托盘信息
        await _repos.SysStockCode.AsUpdateable(pallet).UpdateColumns(x => new { x.Status, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 同步更新入库明细与主单的执行状态与已组数量
    /// </summary>
    /// <param name="box">箱托信息</param>
    /// <param name="now">当前时间</param>
    /// <param name="user">操作人</param>
    private async Task UpdateImportQuantitiesAsync(WmsStockSlotBoxInfo box, DateTime now, string user)
    {
        // 如果箱码绑定了入库明细，更新明细的实际组托数量
        if (box.ImportDetailId.HasValue)
        {
            var detail = await _repos.ImportNotifyDetail.GetFirstAsync(x => x.Id == box.ImportDetailId.Value && x.IsDelete == false);
            if (detail != null)
            {
                // 计算绑定后的实际入库数量
                var newFactQuantity = (detail.ImportFactQuantity ?? 0) + (box.Qty ?? 0);
                var planQuantity = detail.ImportQuantity ?? 0;

                // 获取仓库配置，验证是否允许超入
                if (box.ImportId.HasValue)
                {
                    var notify = await _repos.ImportNotify.GetFirstAsync(x => x.Id == box.ImportId.Value && x.IsDelete == false);
                    if (notify?.WarehouseId != null)
                    {
                        var warehouse = await _repos.Warehouse.GetFirstAsync(x => x.Id == notify.WarehouseId && x.IsDelete == false);

                        // 【超入验证】如果实际数量超过计划数量，检查仓库是否允许超入
                        if (newFactQuantity > planQuantity)
                        {
                            if (!(warehouse?.IsOverbooking ?? false))
                            {
                                var materialName = box.MaterialId.HasValue
                                    ? (await _repos.Material.GetFirstAsync(x => x.Id == box.MaterialId.Value))?.MaterialName ?? "未知物料"
                                    : "未知物料";
                                throw Oops.Bah($"物料【{materialName}】超入：计划入库 {planQuantity}，已组托 {detail.ImportFactQuantity ?? 0}，本次绑定 {box.Qty ?? 0}，将超出计划 {newFactQuantity - planQuantity}当前仓库不允许超入，请联系管理员！");
                            }
                        }
                    }
                }

                // 更新明细的实际组托数量
                detail.ImportFactQuantity = newFactQuantity;
                // 如果明细状态为待执行("01")或空，更新为执行中("02")
                if (string.IsNullOrWhiteSpace(detail.ImportExecuteFlag) || detail.ImportExecuteFlag == "01")
                {
                    // "02"表示执行中（已有箱码绑定）
                    detail.ImportExecuteFlag = "02"; 
                }
                detail.UpdateTime = now;
                detail.UpdateUserName = user;
                // 提交明细更新
                await _repos.ImportNotifyDetail.AsUpdateable(detail).UpdateColumns(x => new { x.ImportFactQuantity, x.ImportExecuteFlag, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();
            }
        }
        // 如果箱码绑定了入库单，更新入库单主单状态
        if (box.ImportId.HasValue)
        {
            var notify = await _repos.ImportNotify.GetFirstAsync(x => x.Id == box.ImportId.Value && x.IsDelete == false);
            // 如果主单状态为待执行("01")，更新为执行中("02")
            if (notify != null && string.Equals(notify.ImportExecuteFlag, "01", StringComparison.OrdinalIgnoreCase))
            {
                notify.ImportExecuteFlag = "02"; // "02"表示执行中（已有箱码绑定）
                notify.UpdateTime = now;
                notify.UpdateUserName = user;
                // 提交主单状态更新
                await _repos.ImportNotify.AsUpdateable(notify).UpdateColumns(x => new { x.ImportExecuteFlag, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();
            }
        }
    }

    /// <summary>
    /// 处理箱托解绑业务逻辑(被多个接口复用)
    /// </summary>
    /// <param name="boxes">同托盘的箱码列表</param>
    /// <param name="target">待解绑箱码</param>
    /// <param name="user">操作人</param>
    /// <param name="type">业务类型</param>
    private async Task<PdaActionResult> ProcessRemoveBoxAsync(List<WmsStockSlotBoxInfo> boxes, WmsStockSlotBoxInfo target, string? user, string? type)
    {
        var result = new PdaActionResult();
        var operatorName = NormalizeUser(user);
        var now = DateTime.Now;
        try
        {
            // 开启事务处理箱托解绑逻辑
            await ExecuteInTransactionAsync(async () =>
            {
                // 【第一步：处理入库流水】如果箱码绑定了入库流水
                if (target.ImportOrderId.HasValue)
                {
                    var order = await _repos.ImportOrder.GetFirstAsync(x => x.Id == target.ImportOrderId.Value && x.IsDelete == false);
                    if (order != null)
                    {
                        // 检查是否还有其他箱码绑定了相同的入库流水
                        var relatedBoxCount = boxes.Count(x => x.ImportOrderId == target.ImportOrderId.Value && x.Id != target.Id);
                        // 如果没有其他箱码了，删除入库流水并释放托盘状态
                        if (relatedBoxCount == 0)
                        {
                            // 软删除入库流水
                            order.IsDelete = true;
                            order.UpdateTime = now;
                            order.UpdateUserName = operatorName;
                            await _repos.ImportOrder.AsUpdateable(order).UpdateColumns(x => new { x.IsDelete, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();
                            // 如果托盘上没有库存托盘记录，释放托盘占用状态（Status=0表示可用）
                            if (target.StockCodeId.HasValue)
                            {
                                var trayExists = await _repos.StockTray.AsQueryable().Where(x => x.StockCode == target.StockCode).Where(x => x.IsDelete == false).CountAsync();
                                if (trayExists == 0)
                                {
                                    await _repos.SysStockCode.AsUpdateable()
                                        .SetColumns(code => new WmsSysStockCode
                                        {
                                            Status = 0, // 释放托盘状态
                                            UpdateTime = now,
                                            UpdateUserName = operatorName
                                        }).Where(code => code.Id == target.StockCodeId).ExecuteCommandAsync();
                                }
                            }
                        }
                    }
                }
                // 【第二步：回滚入库明细数量】如果不是验收场景（type!="ys"）且箱码绑定了入库明细
                if (!string.Equals(type, "ys", StringComparison.OrdinalIgnoreCase) && target.ImportDetailId.HasValue)
                {
                    var detail = await _repos.ImportNotifyDetail.GetFirstAsync(x => x.Id == target.ImportDetailId.Value && x.IsDelete == false);
                    if (detail != null)
                    {
                        // 减少入库明细的实际组托数量（回滚）
                        var factQty = (detail.ImportFactQuantity ?? 0) - (target.Qty ?? 0);
                        if (factQty < 0)
                        {
                            factQty = 0; // 确保数量不为负
                        }
                        detail.ImportFactQuantity = factQty;
                        // 如果明细的实际数量归零，恢复为待执行状态("01")
                        if (factQty == 0)
                        {
                            // "01"表示待执行（还没有箱码绑定）
                            detail.ImportExecuteFlag = "01"; 
                            // 同时恢复主单状态为待执行
                            if (detail.ImportId.HasValue)
                            {
                                await _repos.ImportNotify.AsUpdateable()
                                    .SetColumns(n => new WmsImportNotify
                                    {
                                        // 恢复为待执行
                                        ImportExecuteFlag = "01", 
                                        UpdateTime = now,
                                        UpdateUserName = operatorName
                                    }).Where(n => n.Id == detail.ImportId.Value).ExecuteCommandAsync();
                            }
                        }
                        detail.UpdateTime = now;
                        detail.UpdateUserName = operatorName;
                        // 提交明细数量回滚
                        await _repos.ImportNotifyDetail.AsUpdateable(detail).UpdateColumns(x => new { x.ImportFactQuantity, x.ImportExecuteFlag, x.UpdateTime, x.UpdateUserName }).ExecuteCommandAsync();
                    }
                }
                // 【第三步：清理箱码绑定关系】软删除箱码并清空相关绑定信息
                target.IsDelete = true;              // 软删除标志
                target.StockCode = null;             // 清空托盘编码
                target.StockCodeId = null;           // 清空托盘ID
                target.ImportOrderId = null;         // 清空入库流水ID
                target.Weight = 0;                   // 重置重量
                target.UpdateTime = now;             // 更新时间
                target.UpdateUserName = operatorName;// 更新用户
                // 提交箱码清理
                await _repos.StockSlotBoxInfo.AsUpdateable(target).UpdateColumns(x => new
                {
                    x.IsDelete,
                    x.StockCode,
                    x.StockCodeId,
                    x.ImportOrderId,
                    x.Weight,
                    x.UpdateTime,
                    x.UpdateUserName
                }).ExecuteCommandAsync();
            }, "箱托解绑失败");
            result.Success = true;
            result.Message = "操作成功";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "箱托解绑失败");
            result.Success = false;
            result.Message = ex.Message;
        }
        return result;
    }

    #endregion
}
