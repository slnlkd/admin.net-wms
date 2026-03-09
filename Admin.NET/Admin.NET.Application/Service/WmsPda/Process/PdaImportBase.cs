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
using SqlSugar;

namespace Admin.NET.Application.Service.WmsPda.Process;

/// <summary>
/// PDA入库业务基类
/// <para>提供入库相关的公共方法，供各入库业务处理类继承使用</para>
/// </summary>
/// <remarks>
/// 公共功能包括：
/// <list type="bullet">
/// <item><description>物料条码解析：支持明细-物料-批次-流水格式</description></item>
/// <item><description>入库单号解析：支持单号和主键两种格式</description></item>
/// <item><description>数据验证：物料匹配、托盘混放、单据一致性验证</description></item>
/// <item><description>查询辅助：获取入库单据号、托盘主数据、入库流水等</description></item>
/// </list>
/// </remarks>
public abstract class PdaImportBase : PdaBase
{
    #region 字段定义
    protected readonly PdaBaseRepos _repos;                                         //仓储聚合
    protected readonly WmsSqlViewService _sqlViewService;                           //SQL视图功能服务(视图查询和SQL函数)
    protected readonly CommonMethod _commonMethod;                                  //通用方法
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的公共依赖
    /// </summary>
    protected PdaImportBase(
        ISqlSugarClient sqlSugarClient,
        PdaBaseRepos repos,
        WmsSqlViewService sqlViewService) : base(sqlSugarClient)
    {
        _repos = repos;
        _sqlViewService = sqlViewService;
        _commonMethod = new CommonMethod(sqlSugarClient);
    }
    #endregion

    #region 公共验证方法

    /// <summary>
    /// 校验箱码物料是否与单据匹配，避免跨物料绑定
    /// </summary>
    /// <param name="box">箱托信息</param>
    /// <returns>是否匹配</returns>
    protected async Task<bool> ValidateMaterialMatchAsync(WmsStockSlotBoxInfo box)
    {
        // 物料ID为空则无法验证，直接返回不匹配
        if (box.MaterialId == null)
            return false;
        // 如果已有批次号，说明已经绑定过物料，视为匹配（批次号是物料绑定后才生成的）
        if (!string.IsNullOrWhiteSpace(box.LotNo))
            return true;
        // 没有入库单据ID，无法验证物料是否在单据范围内
        if (!box.ImportId.HasValue)
            return false;
        // 验证物料是否在入库单据的明细中（防止绑定不在单据内的物料）
        return await _repos.ImportNotifyDetail.AsQueryable()
            .Where(x => x.ImportId == box.ImportId.Value &&
                x.MaterialId == box.MaterialId.Value &&
                x.IsDelete == false)
            .AnyAsync();
    }

    /// <summary>
    /// 校验托盘内是否混放不同单据的箱码
    /// </summary>
    /// <param name="palletCode">托盘编码</param>
    /// <param name="candidate">当前待绑定箱码</param>
    protected async Task ValidatePalletMixingAsync(string? palletCode, WmsStockSlotBoxInfo candidate)
    {
        // 托盘编码为空，无法验证混放，直接通过
        if (string.IsNullOrWhiteSpace(palletCode))
            return;
        // 查询托盘上最早绑定的箱码（Status=0表示待入库状态）
        var existing = await _repos.StockSlotBoxInfo.AsQueryable()
            .Where(x => x.StockCode == palletCode && x.IsDelete == false && x.Status == 0)
            .OrderBy(x => x.CreateTime, OrderByType.Asc) // 按创建时间升序，获取最早的箱码
            .FirstAsync();
        // 如果托盘上没有其他箱码，或者找到的就是当前箱码本身，允许绑定
        if (existing == null || existing.Id == candidate.Id)
            return;
        // 如果托盘上已有箱码且绑定了不同的入库单，则禁止混放（一个托盘只能属于一个入库单）
        if (existing.ImportId.HasValue && candidate.ImportId.HasValue && existing.ImportId != candidate.ImportId)
            throw Oops.Bah("当前托盘已绑定其他入库单，请更换托盘！");
    }

    /// <summary>
    /// 校验 PDA 输入的订单号是否与箱码归属一致
    /// </summary>
    /// <param name="box">箱托信息</param>
    /// <param name="input">绑定入参</param>
    protected async Task ValidateOrderConsistencyAsync(WmsStockSlotBoxInfo box, PdaBindBoxInput input)
    {
        // PDA未输入订单号，跳过验证
        if (string.IsNullOrWhiteSpace(input.OrderNo))
            return;
        // 获取箱码实际归属的入库单据号
        var expectedOrder = await GetImportBillCodeAsync(box);
        // 箱码未绑定任何单据，跳过验证
        if (string.IsNullOrWhiteSpace(expectedOrder))
            return;
        // 验证PDA输入的订单号是否与箱码归属的单据号一致（不区分大小写）
        if (!string.Equals(expectedOrder, input.OrderNo.Trim(), StringComparison.OrdinalIgnoreCase))
            throw Oops.Bah("箱码与订单号不匹配!");
    }

    #endregion

    #region 查询辅助方法

    /// <summary>
    /// 根据入库单号(或主键)解析真实的入库单主键
    /// </summary>
    /// <param name="importBillCode">入库单据号或主键</param>
    /// <returns>入库单主键，未找到时返回null</returns>
    protected async Task<long?> ResolveImportIdAsync(string? importBillCode)
    {
        // 输入为空，返回null
        if (string.IsNullOrWhiteSpace(importBillCode))
            return null;
        // 尝试直接解析为主键ID（如果输入的是纯数字）
        if (long.TryParse(importBillCode, out var id))
            return id;
        // 输入的是单据号，查询对应的主键ID
        var notify = await _repos.ImportNotify.GetFirstAsync(x => x.ImportBillCode == importBillCode && x.IsDelete == false);
        // 返回主键ID（ID为0视为无效，返回null）
        return notify?.Id == 0 ? null : notify?.Id;
    }

    /// <summary>
    /// 获取箱码对应的入库/验收单据号
    /// </summary>
    /// <param name="box">箱托信息</param>
    /// <returns>单据号</returns>
    protected async Task<string> GetImportBillCodeAsync(WmsStockSlotBoxInfo box)
    {
        // 箱码未绑定任何单据，返回空字符串
        if (!box.ImportId.HasValue)
            return string.Empty;
        // 优先查询入库单据单据号（box.ImportId可能指向入库单据或验收通知）
        var notifyEntity = await _repos.ImportNotify.GetFirstAsync(x => x.Id == box.ImportId.Value && x.IsDelete == false);
        var notifyCode = notifyEntity?.ImportBillCode;
        // 如果找到入库单据单据号，直接返回
        if (!string.IsNullOrWhiteSpace(notifyCode))
            return notifyCode;
        // 未找到入库单据，尝试查询验收通知单据号（验收通知也是入库的一种）
        var inspectEntity = await _repos.InspectNotify.GetFirstAsync(x => x.Id == box.ImportId.Value && x.IsDelete == false);
        return inspectEntity?.InspectBillCode ?? string.Empty;
    }

    /// <summary>
    /// 获取托盘主数据，若不存在则抛出异常
    /// </summary>
    /// <param name="trayCode">托盘编码</param>
    /// <returns>托盘主数据</returns>
    protected async Task<WmsSysStockCode> ResolveStockCodeAsync(string trayCode)
    {
        // 查询托盘主数据，不存在则抛出异常（托盘必须先在系统中注册）
        var stockCode = await _repos.SysStockCode.GetFirstAsync(x => x.StockCode == trayCode && x.IsDelete == false) ?? throw Oops.Bah($"托盘({trayCode})信息不存在！");
        return stockCode;
    }

    /// <summary>
    /// 获取指定托盘最新的待执行入库流水
    /// </summary>
    /// <param name="stockCode">库存编码</param>
    /// <param name="roleLabel">角色标签</param>
    /// <returns>入库流水</returns>
    protected async Task<WmsImportOrder> ResolveLatestPendingImportOrderAsync(string stockCode, string roleLabel)
    {
        // 查询指定托盘的待执行入库流水（ImportExecuteFlag="01"表示待执行状态）
        var order = await _repos.ImportOrder.AsQueryable()
            .Where(x => x.IsDelete == false && x.ImportExecuteFlag == "01" && x.StockCode == stockCode)
            .OrderBy(x => x.CreateTime, OrderByType.Desc) // 按创建时间倒序，获取最新的流水
            .FirstAsync() ?? throw Oops.Bah($"{roleLabel}({stockCode})未找到待执行入库流水！");
        return order;
    }

    #endregion

    #region 静态工具方法

    /// <summary>
    /// 解析物料条码(明细-物料-批次-流水)
    /// </summary>
    /// <param name="barcode">原始条码字符串</param>
    /// <returns>解析后的条码结构，包含各部分ID和原始值</returns>
    /// <exception cref="Exception">当条码格式不正确时抛出异常</exception>
    /// <remarks>
    /// 条码格式：
    /// <list type="bullet">
    /// <item><description>基本格式：明细ID-物料ID-批次</description></item>
    /// <item><description>扩展格式：明细ID-物料ID-批次-流水ID</description></item>
    /// <item><description>使用"-"作为分隔符</description></item>
    /// </list>
    /// 解析规则：
    /// <list type="number">
    /// <item><description>至少包含3个段落(明细、物料、批次)</description></item>
    /// <item><description>明细ID和物料ID尝试解析为长整型</description></item>
    /// <item><description>保留原始字符串值用于查询</description></item>
    /// <item><description>第4段落为可选的入库流水ID</description></item>
    /// </list>
    /// 示例：
    /// <list type="bullet">
    /// <item><description>"123-456-LOT001" → 明细ID=123, 物料ID=456, 批次=LOT001</description></item>
    /// <item><description>"123-456-LOT001-789" → 明细ID=123, 物料ID=456, 批次=LOT001, 流水ID=789</description></item>
    /// </list>
    /// </remarks>
    protected static ParsedBarcode ParseMaterialBarcode(string barcode)
    {
        // 按"-"分隔符拆分条码字符串，去除空段
        var segments = (barcode ?? string.Empty).Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
        // 验证条码段数，至少需要3段（明细ID-物料ID-批次号）
        if (segments.Length < 3)
        {
            throw Oops.Bah("物料条码格式错误，正确格式示例：明细ID-物料ID-批次");
        }
        // 解析第1段：明细ID（尝试转为长整型，失败则保留原始字符串）
        long? detailId = long.TryParse(segments[0], out var detailVal) ? detailVal : null;
        // 解析第2段：物料ID（尝试转为长整型，失败则保留原始字符串）
        long? materialId = long.TryParse(segments[1], out var materialVal) ? materialVal : null;
        // 解析第3段：批次号（保持字符串格式）
        string lotNo = segments[2];
        // 构造解析结果对象
        var parsed = new ParsedBarcode
        {
            DetailId = detailId, // 明细ID（数值型，可能为null）
            DetailIdRaw = segments[0], // 明细ID原始字符串
            MaterialId = materialId, // 物料ID（数值型，可能为null）
            MaterialIdRaw = segments[1], // 物料ID原始字符串
            LotNo = lotNo // 批次号
        };
        // 解析第4段（可选）：入库流水ID
        if (segments.Length > 3 && long.TryParse(segments[3], out var orderId))
        {
            parsed.OrderId = orderId; // 流水ID（数值型）
            parsed.OrderIdRaw = segments[3]; // 流水ID原始字符串
        }
        return parsed;
    }

    /// <summary>
    /// 规范化动作类型字符串
    /// </summary>
    /// <param name="actionType">动作类型原始值</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>规范化后的动作类型</returns>
    protected static string NormalizeActionType(string? actionType, string defaultValue) =>
        // 如果动作类型为空，返回默认值；否则转为小写并去除首尾空格（统一动作类型格式）
        string.IsNullOrWhiteSpace(actionType)
            ? defaultValue
            : actionType.Trim().ToLowerInvariant();

    /// <summary>
    /// 收集主副载具托盘编码，去重后返回
    /// </summary>
    /// <param name="primaryCode">主载具托盘编码</param>
    /// <param name="secondaryCode">副载具托盘编码</param>
    /// <returns>去重后的托盘编码集合</returns>
    protected static HashSet<string> CollectTrayCodes(string primaryCode, string? secondaryCode)
    {
        // 创建不区分大小写的HashSet，先添加主载具托盘编码
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { primaryCode.Trim() };
        // 如果有副载具托盘编码，添加到集合中（HashSet会自动去重，避免主副载具相同时重复）
        if (!string.IsNullOrWhiteSpace(secondaryCode))
        {
            codes.Add(secondaryCode.Trim());
        }
        return codes;
    }

    #endregion
}
