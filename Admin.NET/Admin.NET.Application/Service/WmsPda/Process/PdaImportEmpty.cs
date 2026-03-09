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
using Admin.NET.Application.Service.WmsPort;
using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Application.Service.WmsPort.Process;
using Admin.NET.Application.Service.WmsSqlView;
using Admin.NET.Core;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Admin.NET.Application.Service.WmsPda.Process;

/// <summary>
/// PDA入库空托盘管理业务处理类
/// <para>承载 JC35 <c>PdaInterfaceController</c> 空托盘管理相关接口迁移，提供空托盘绑定、批量生成等业务功能</para>
/// </summary>
/// <remarks>
/// 主要功能包括：
/// <list type="bullet">
/// <item><description>空托盘组盘：绑定和解绑空托盘</description></item>
/// <item><description>批量生成：批量生成空托入库流水</description></item>
/// </list>
/// </remarks>
public class PdaImportEmpty : PdaImportBase, ITransient
{
    #region 字段定义
    private readonly ILogger _logger;                            //日志
    private readonly PortImportBind _emptyTrayBind;                                //空托盘绑定
    #endregion

    #region 常量定义
    private const int DefaultEmptyTrayQuantity = 2;                                //默认空托盘数量
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖
    /// </summary>
    public PdaImportEmpty(
        ILoggerFactory loggerFactory,
        ISqlSugarClient sqlSugarClient,
        PdaBaseRepos repos,
        PortImportBind emptyTrayBind,
        WmsSqlViewService sqlViewService) : base(sqlSugarClient, repos, sqlViewService)
    {
        _emptyTrayBind = emptyTrayBind;
        _logger = loggerFactory.CreateLogger(CommonConst.PdaImportEmpty);
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 空托盘组盘(绑定/解绑)
    /// <para>对应 JC35 接口：【/PdaInterface/KBackConfirm】</para>
    /// </summary>
    /// <param name="input">空托盘组盘的请求参数，包含托盘编码、数量、动作类型、用户等</param>
    /// <returns>组盘操作结果，包含成功状态和消息</returns>
    /// <exception cref="Exception">当参数验证失败、托盘不存在、组盘失败时抛出异常</exception>
    /// <remarks>
    /// 操作类型：
    /// <list type="bullet">
    /// <item><description>add：绑定空托盘，需要数量 > 0</description></item>
    /// <item><description>del：解绑空托盘，数量可为 0</description></item>
    /// </list>
    /// 业务规则：
    /// <list type="number">
    /// <item><description>绑定操作：验证托盘存在且可用，数量必须大于0</description></item>
    /// <item><description>解绑操作：移除空托盘绑定关系</description></item>
    /// <item><description>自动更新托盘状态和相关入库流水</description></item>
    /// <item><description>兼容JC35原项目的参数验证逻辑</description></item>
    /// </list>
    /// 应用场景：
    /// <list type="bullet">
    /// <item><description>空托盘入库前组盘操作</description></item>
    /// <item><description>空托盘状态管理</description></item>
    /// <item><description>托盘占用状态控制</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> KBackConfirmAsync(PdaEmptyTrayBindInput input)
    {
        // 验证输入参数完整性
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.PalletCode))
            throw Oops.Bah("托盘编码不能为空！");
        // 规范化动作类型（转为小写，默认为"add"）
        var actionType = NormalizeActionType(input.ActionType, "add");
        // 【绑定操作验证】仅在绑定操作时验证数量必须大于0（解绑时数量可为0或任意值）
        // 这是为了兼容JC35原项目的参数验证逻辑
        if (string.Equals(actionType, "add", StringComparison.OrdinalIgnoreCase) && input.Quantity <= 0)
            throw Oops.Bah("数量必须大于 0！");
        // 构建空托盘绑定请求参数
        var request = new EmptyTrayBindInput
        {
            PalletCode = input.PalletCode.Trim(),
            Quantity = input.Quantity, // 绑定时使用实际数量，解绑时此值不会被使用
            ActionType = actionType, // "add"=绑定, "del"=解绑
            UserId = NormalizeUser(input.User)
        };
        // 调用WCS端口服务处理空托盘组盘逻辑（实际的绑定/解绑操作在PortImportBind中完成）
        var result = await _emptyTrayBind.ProcessKBackConfirmAsync(request);
        // 转换为PDA标准返回格式
        return new PdaActionResult
        {
            Success = result.Success,
            Message = result.Message
        };
    }
    /// <summary>
    /// 批量生成空托入库流水
    /// <para>对应 JC35 接口：【AddWmsImportOrder】</para>
    /// </summary>
    /// <returns>批量生成操作结果，包含成功状态、消息和处理数量</returns>
    /// <exception cref="Exception">当批量处理过程中发生严重错误时抛出异常</exception>
    /// <remarks>
    /// 筛选条件：
    /// <list type="bullet">
    /// <item><description>托盘编码以1、3、5、7、9结尾的奇数托盘</description></item>
    /// <item><description>托盘状态为0(未使用)</description></item>
    /// <item><description>托盘未被其他组托记录占用</description></item>
    /// </list>
    /// 处理逻辑：
    /// <list type="number">
    /// <item><description>筛选符合条件的空托盘候选集合</description></item>
    /// <item><description>排除已被占用的托盘</description></item>
    /// <item><description>逐个调用空托盘绑定服务</description></item>
    /// <item><description>使用默认数量2进行绑定</description></item>
    /// <item><description>统计成功和失败的数量</description></item>
    /// </list>
    /// 容错机制：
    /// <list type="bullet">
    /// <item><description>单个托盘失败不影响其他托盘处理</description></item>
    /// <item><description>记录失败托盘的详细错误信息</description></item>
    /// <item><description>返回总体处理结果和成功数量</description></item>
    /// </list>
    /// </remarks>
    public async Task<PdaActionResult> BatchGenerateEmptyTrayOrdersAsync()
    {
        // 【第一步：查询候选托盘】筛选符合条件的空托盘
        // 条件1：未删除且状态为0（未使用）
        // 条件2：托盘编码以奇数结尾（1/3/5/7/9）- 这是业务规则，奇数托盘用于空托入库
        var candidates = await _repos.SysStockCode.AsQueryable()
            .Where(x => x.IsDelete == false && x.Status == 0)
            .Where(x => SqlFunc.EndsWith(x.StockCode, "1")
                     || SqlFunc.EndsWith(x.StockCode, "3")
                     || SqlFunc.EndsWith(x.StockCode, "5")
                     || SqlFunc.EndsWith(x.StockCode, "7")
                     || SqlFunc.EndsWith(x.StockCode, "9"))
            .Select(x => new { x.Id, x.StockCode })
            .ToListAsync();
        // 如果没有符合条件的托盘，直接返回失败
        if (candidates.Count == 0)
            return new PdaActionResult
            {
                Success = false,
                Message = "没有符合条件的托盘！"
            };
        // 【第二步：排除已占用托盘】查询已被箱托记录占用的托盘ID
        // 避免重复绑定或占用已有物料的托盘
        var candidateIds = candidates.Select(x => x.Id).ToList();
        var occupiedIds = await _repos.StockSlotBoxInfo.AsQueryable()
            .Where(x => x.IsDelete == false && (x.Status == 0 || x.Status == 1)) // Status=0待绑定或Status=1已申请入库
            .Where(x => x.StockCodeId != null && candidateIds.Contains(x.StockCodeId.Value))
            .Select(x => x.StockCodeId.Value)
            .Distinct() // 去重，每个托盘只统计一次
            .ToListAsync();
        var occupiedSet = new HashSet<long>(occupiedIds); // 转为HashSet提高查找效率
        // 【第三步：批量处理】遍历候选托盘，逐个调用绑定服务
        var successCount = 0;
        foreach (var candidate in candidates)
        {
            // 跳过已被占用的托盘
            if (occupiedSet.Contains(candidate.Id))
                continue;
            try
            {
                // 调用空托盘绑定服务，使用默认数量2
                var response = await _emptyTrayBind.ProcessKBackConfirmAsync(new EmptyTrayBindInput
                {
                    PalletCode = candidate.StockCode,
                    Quantity = DefaultEmptyTrayQuantity, // 默认数量为2（常量定义）
                    ActionType = "add", // 绑定操作
                    UserId = "PDA-BATCH" // 批量操作用户标识
                });
                // 统计成功数量
                if (response.Success)
                    successCount++;
                else
                    // 记录失败日志（不中断整个批量过程）
                    _logger.LogWarning("空托批量绑定失败：Pallet={Pallet}, Message={Message}", candidate.StockCode, response.Message);
            }
            catch (Exception ex)
            {
                // 捕获异常但不中断批量处理，记录警告日志
                _logger.LogWarning(ex, "空托批量绑定发生异常：Pallet={Pallet}", candidate.StockCode);
            }
        }
        // 【第四步：返回结果】根据成功数量判断整体操作是否成功
        return new PdaActionResult
        {
            Success = successCount > 0, // 至少成功一个就视为成功
            Message = successCount > 0
                ? $"批量绑定完成，共成功 {successCount} 条"
                : "没有可绑定的托盘！"
        };
    }
    #endregion
}
