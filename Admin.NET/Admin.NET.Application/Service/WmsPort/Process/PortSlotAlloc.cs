// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsPda.Dto;
using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Application.Service.WmsSqlView;
using Admin.NET.Core;
using Admin.NET.Core.Service;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
namespace Admin.NET.Application.Service.WmsPort.Process;
/// <summary>
/// 库位分配辅助类
/// 用于处理库位分配、查询等复杂逻辑
///
/// <para>【AB面计算状态说明】</para>
/// <para>本类实现了完整的AB面计算逻辑（CalculateABSide和CalculateABSideForEmptyTray方法），</para>
/// <para>用于标识多口巷道中货物的入出方向（A面=从大深度入，B面=从小深度入）。</para>
/// <para>但在当前系统版本中，AB面计算结果在下游（PortImportApply.cs）中被置为空字符串，</para>
/// <para>未被传递到WCS系统或存储到数据库。因此AB面计算虽然执行，但结果未被实际使用。</para>
/// <para>相关代码保留是为了未来可能的功能恢复。</para>
/// </summary>
public class PortSlotAlloc : PortBase, ITransient
{
    #region 字段定义
    // 仓储聚合类
    private readonly PortBaseRepos _repos;
    // 业务服务
    private readonly WmsSqlViewService _sqlViewService;
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖仓储
    /// </summary>
    /// <param name="loggerFactory">日志工厂</param>
    /// <param name="repos">仓储聚合类</param>
    /// <param name="sqlSugarClient">数据库客户端</param>
    /// <param name="sqlViewService">SQL视图功能服务</param>
    public PortSlotAlloc(
        ILoggerFactory loggerFactory,
        PortBaseRepos repos,
        ISqlSugarClient sqlSugarClient,
        WmsSqlViewService sqlViewService)
        : base(sqlSugarClient, loggerFactory.CreateLogger(CommonConst.PortSlotAlloc), "库位分配")
    {
        _repos = repos;
        _sqlViewService = sqlViewService;
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 获取库位分配结果（主方法）
    /// </summary>
    /// <param name="stockCode">托盘码</param>
    /// <param name="locationType">库位类型（huiku:回库，其他:普通入库）</param>
    /// <returns>储位地址和AB面，格式：储位地址&amp;AB面
    /// <para>注意：返回的AB面值在下游PortImportApply.cs中被置为空字符串，未实际使用</para>
    /// </returns>
    public async Task<string> GetLocations(string stockCode, string locationType = "")
    {
        try
        {
            // 1. 获取托盘上的箱码信息
            var boxViewList = await _sqlViewService.QueryStockSlotBoxInfoView()
                .MergeTable()
                .Where(x => x.StockCode == stockCode && (x.Status == null || x.Status == 0))
                .ToListAsync();
            var boxIds = boxViewList.Select(x => x.Id).ToList();
            var boxList = boxIds.Count > 0 ? await _repos.BoxInfo.GetListAsync(a => boxIds.Contains(a.Id)) : new List<WmsStockSlotBoxInfo>();
            // 2. 获取托盘的库存信息（回流入库）
            var trayList = await _repos.StockTray.GetListAsync(a => a.StockCode == stockCode);
            if ((boxList == null || boxList.Count == 0) && (trayList == null || trayList.Count == 0))
                throw Oops.Bah("非法托盘,拒绝分配储位！");
            // 3. 检查库内空储位是否足够
            if (!await CheckAvailableSlotCount())
                throw Oops.Bah("已达库存上限！");
            // 4. 获取托盘信息和物料信息
            var trayInfo = await GetTrayInformation(stockCode, boxList, trayList, boxViewList);
            if (trayInfo == null)
                throw Oops.Bah("无法获取托盘信息！");
            // 5. 获取物料对应的区域
            var areaId = await GetMaterialArea(trayInfo.MaterialCode, trayInfo.IsInspectionTray, trayInfo.WarehouseId);
            if (string.IsNullOrEmpty(areaId))
                throw Oops.Bah("该物料未分配区域信息,无法入库,请去物料信息维护页面进行维护！");
            // 6. 先尝试查找同物料批次的库位组
            var slotResult = await FindSlotInSameBatchGroup(trayInfo, areaId, locationType);
            if (!string.IsNullOrEmpty(slotResult))
                return slotResult;
            // 7. 查找空储位
            return await FindEmptySlot(trayInfo, areaId, locationType);
        }
        catch (Exception ex)
        {
            throw Oops.Bah($"库位分配失败：{ex.Message}");
        }
    }
    /// <summary>
    /// 获取空托盘库位
    /// </summary>
    /// <param name="stockCode">托盘码</param>
    /// <returns>储位地址和AB面，格式：储位地址&amp;AB面
    /// <para>注意：返回的AB面值在下游PortImportApply.cs中被置为空字符串，未实际使用</para>
    /// </returns>
    public async Task<string> GetNullLocations(string stockCode)
    {
        try
        {
            // 1. 获取托盘信息
            var trayInfo = await _repos.StockCode.GetFirstAsync(b => b.StockCode == stockCode) ?? throw Oops.Bah("托盘信息不存在！");
            // 2. 检查库内空储位是否足够
            if (!await CheckAvailableSlotCount())
                throw Oops.Bah("已达库存上限！");
            // 3. 获取空托盘对应的区域
            var areaCode = trayInfo.StockType == TrayType.InspectionTray ? "01" : "00";
            var area = await _repos.Area.GetFirstAsync(a => a.AreaCode == areaCode);
            var areaId = area?.Id.ToString() ?? "";
            // 4. 优先查找已有空托存放的巷道
            var slotResult = await FindSlotForEmptyTray(stockCode, trayInfo, areaId);
            if (!string.IsNullOrEmpty(slotResult))
                return slotResult;
            // 5. 查找空储位
            var tempSlots = await _repos.Slot.AsQueryable()
                .Where(s => (s.SlotStatus == SlotStatusCode.Empty || s.SlotStatus == SlotStatusCode.Reserved || s.SlotStatus == SlotStatusCode.MovingOut) &&s.SlotImlockFlag == 0 && s.WarehouseId == trayInfo.WarehouseId && s.Make == "01")
                .ToListAsync();
            var emptySlots = tempSlots.OrderBy(s => s.SlotLanewayId).ThenByDescending(s => s.SlotInout).ToList();
            if (emptySlots.Count == 0)
                throw Oops.Bah("未找到合适储位！");
            //返回第一个合适的库位
            var targetSlot = emptySlots.First();
            // 计算空托盘的AB面（特殊规则）
            var stockType = trayInfo.StockType.HasValue ? (int?)Convert.ToInt32(trayInfo.StockType.Value) : null;
            // 注意：此处计算的abSide值在下游PortImportApply.cs中被置为空字符串，未实际使用
            var abSide = await CalculateABSideForEmptyTray(targetSlot, stockType);
            //返回空托盘对应的库位和AB面
            return $"{targetSlot.SlotCode}&{abSide}";
        }
        catch (Exception ex)
        {
            throw Oops.Bah($"空托盘库位分配失败：{ex.Message}");
        }
    }
    #endregion

    #region 私有辅助方法
    /// <summary>
    /// 检查可用库位数量是否足够
    /// </summary>
    private async Task<bool> CheckAvailableSlotCount()
    {
        var emptySlotCount = await _repos.Slot.AsQueryable()
            .Where(s => s.SlotStatus == SlotStatusCode.Empty && s.SlotImlockFlag == 0)
            .CountAsync();
        return emptySlotCount > PortSlotConstants.ReservedSlotCount;
    }
    /// <summary>
    /// 获取托盘信息
    /// </summary>
    /// <param name="stockCode">托盘码</param>
    /// <param name="boxList">箱码列表</param>
    /// <param name="trayList">托盘列表</param>
    /// <param name="boxViewList">箱码视图列表</param>
    /// <returns>托盘信息</returns>
    private async Task<TrayInformation> GetTrayInformation(string stockCode, List<WmsStockSlotBoxInfo> boxList,
        List<WmsStockTray> trayList, List<ViewStockSlotBoxInfoView> boxViewList)
    {
        var trayInfo = new TrayInformation { StockCode = stockCode };
        if (boxList != null && boxList.Count > 0) // 普通入库
        {
            var firstBoxView = boxViewList.FirstOrDefault();
            if (firstBoxView?.ImportDetailId.HasValue == true)
            {
                //使用视图查询入库单据明细信息
                var importDetailView = await _sqlViewService.QueryImportNotifyDetailView()
                    .MergeTable()
                    .Where(x => x.Id == firstBoxView.ImportDetailId.Value)
                    .FirstAsync();
                //如果入库单据明细信息不为空，则获取托盘信息
                if (importDetailView != null)
                {
                    trayInfo.MaterialCode = importDetailView.MaterialId?.ToString();
                    trayInfo.LotNo = importDetailView.LotNo;
                    trayInfo.WarehouseId = importDetailView.WareHouseId;
                }
            }
            if (firstBoxView?.ImportOrderId.HasValue == true)
            {
                var importOrderView = await _sqlViewService.QueryImportOrderView()
                    .MergeTable()
                    .Where(x => x.Id == firstBoxView.ImportOrderId.Value)
                    .FirstAsync();
                trayInfo.IsInspectionTray = importOrderView?.InspectionStatus == 1;
            }
            trayInfo.IsReturnFlow = false;
        }//如果托盘列表不为空，则获取托盘信息
        else if (trayList != null && trayList.Count > 0) // 回库
        {
            var tray = trayList.First();
            trayInfo.MaterialCode = tray.MaterialId;
            trayInfo.LotNo = tray.LotNo;
            trayInfo.WarehouseId = long.TryParse(tray.WarehouseId, out var warehouseId) ? warehouseId : (long?)null;
            trayInfo.IsReturnFlow = true;
            trayInfo.IsInspectionTray = false;
        }
        return trayInfo;
    }
    /// <summary>
    /// 获取物料对应的区域（用于平库库位分配）
    /// </summary>
    /// <param name="materialCode">物料ID</param>
    /// <param name="isInspectionTray">是否是质检托盘</param>
    /// <param name="warehouseId">仓库ID</param>
    /// <returns>区域ID</returns>
    /// <remarks>
    /// 多区域处理：筛选符合条件的区域（非质检区域、非禁用、所属仓库匹配）
    /// </remarks>
    private async Task<string> GetMaterialArea(string materialCode, bool isInspectionTray, long? warehouseId)
    {
        // 特殊处理：质检托盘直接查找质检区域
        //if (isInspectionTray)
        //{
        //    var inspectionArea = await _repos.Area.GetFirstAsync(a =>
        //        a.IsBulkTank == "1" && a.IsDisable == "0" && a.AreaWarehouseId == warehouseId);
        //    return inspectionArea?.Id.ToString();
        //}

        // 解析物料ID
        if (!long.TryParse(materialCode, out var materialId))
            throw Oops.Bah("该物料未分配区域信息,无法入库,请去物料信息维护页面进行维护！");

        // 查询物料信息
        var material = await _repos.Material.GetFirstAsync(m => m.Id == materialId)
            ?? throw Oops.Bah("该物料未分配区域信息,无法入库,请去物料信息维护页面进行维护！");

        string? areaIdString = null;

        // 优先级1: 优先使用物料自身的区域配置
        if (!string.IsNullOrWhiteSpace(material.MaterialAreaId))
        {
            areaIdString = material.MaterialAreaId;
        }
        // 优先级2: 如果物料没有配置区域，查询同物料类型的其他物料配置
        else if (material.MaterialType.HasValue)
        {
            var sampleMaterial = await _repos.Material.GetFirstAsync(m =>
                m.MaterialType == material.MaterialType
                && !string.IsNullOrWhiteSpace(m.MaterialAreaId)
                && !m.IsDelete);

            if (sampleMaterial != null)
            {
                areaIdString = sampleMaterial.MaterialAreaId;
            }
        }

        // 如果找到了区域配置，进行筛选
        if (!string.IsNullOrWhiteSpace(areaIdString))
        {
            // MaterialAreaId可能包含多个区域（逗号分隔），需要进一步筛选
            var areaIds = areaIdString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .ToList();

            // 查询符合条件的区域（非质检区域、非禁用、所属仓库匹配）
            var area = await _repos.Area.GetFirstAsync(a =>
                areaIds.Contains(a.Id.ToString())
                //&& a.IsBulkTank == "0"
                //&& a.IsDisable == "0"
                && a.AreaWarehouseId == warehouseId);

            if (area != null)
                return area.Id.ToString();

            throw Oops.Bah("未找到合适的区域,储位为空。");
        }

        // 降级方案：查找默认区域（AreaCode = "00"）
        var defaultArea = await _repos.Area.GetFirstAsync(a =>
            a.AreaCode == "A" 
            //&& a.IsDisable == "0" 
            && a.AreaWarehouseId == warehouseId);

        if (defaultArea != null)
            return defaultArea.Id.ToString();

        throw Oops.Bah("该物料未分配区域信息,无法入库,请去物料信息维护页面进行维护！");
    }
    /// <summary>
    /// 在同物料批次库位组中查找
    /// </summary>
    /// <param name="trayInfo">托盘信息</param>
    /// <param name="areaId">区域ID</param>
    /// <param name="locationType">库位类型（huiku:回库）</param>
    /// <returns>库位地址</returns>
    private async Task<string> FindSlotInSameBatchGroup(TrayInformation trayInfo, string areaId, string locationType)
    {
        // 查找已有同物料同批次的托盘所在巷道
        var query = _repos.StockTray.AsQueryable()
            .LeftJoin<WmsBaseSlot>((t, s) => t.StockSlotCode == s.SlotCode)
            .Where((t, s) => t.MaterialId == trayInfo.MaterialCode && t.LotNo == trayInfo.LotNo &&
                            t.WarehouseId == trayInfo.WarehouseId.ToString() && s.SlotImlockFlag == 0 && s.Make == "01")
            .GroupBy((t, s) => s.SlotLanewayId)
            .Select((t, s) => new
            {
                LanewayId = s.SlotLanewayId,
                Count = SqlFunc.AggregateCount(s.SlotLanewayId)
            }).MergeTable()
            .OrderBy(t => t.LanewayId);

        var lanewayGroups = await query.ToListAsync();
        //在这些巷道中查找空库位遍历巷道组
        foreach (var group in lanewayGroups)
        {
            if (group.LanewayId.HasValue) //如果巷道ID不为空，则查找空库位
            {
                var emptySlot = await _repos.Slot.GetFirstAsync(s =>
                    s.SlotLanewayId == group.LanewayId.Value && s.SlotStatus == SlotStatusCode.Empty &&
                    s.SlotImlockFlag == 0 && s.Make == "01");
                if (emptySlot != null)
                {
                    // 注意：此处计算的abSide值在下游PortImportApply.cs中被置为空字符串，未实际使用
                    var abSide = await CalculateABSide(emptySlot, trayInfo, locationType); //计算AB面
                    return $"{emptySlot.SlotCode}&{abSide}";
                }
            }
        }
        return null;
    }
    /// <summary>
    /// 查找空储位
    /// </summary>
    /// <param name="trayInfo">托盘信息</param>
    /// <param name="areaId">区域ID</param>
    /// <param name="locationType">库位类型（huiku:回库）</param>
    /// <returns>库位地址</returns>
    private async Task<string> FindEmptySlot(TrayInformation trayInfo, string areaId, string locationType)
    {
        var slots = await _repos.Slot.AsQueryable()
            .Where(s => (s.SlotStatus == SlotStatusCode.Empty || s.SlotStatus == SlotStatusCode.Reserved || s.SlotStatus == SlotStatusCode.MovingOut) &&
                       s.SlotImlockFlag == 0 && s.WarehouseId == trayInfo.WarehouseId && s.Make == "01")
            .ToListAsync();
        // 根据是否回库选择排序方式
        var emptySlots = locationType == "huiku"
            ? slots.OrderBy(s => s.SlotLanewayId).ThenByDescending(s => s.SlotInout).ToList() // 回库按巷道正序
            : slots.OrderByDescending(s => s.SlotLayer).ThenByDescending(s => s.SlotLanewayId).ThenByDescending(s => s.SlotInout).ToList(); // 普通入库按层号倒序
        if (emptySlots.Count == 0)
            throw Oops.Bah("未找到合适储位！");
        var targetSlot = emptySlots.First(); //返回第一个合适的库位
        // 注意：此处计算的abSide值在下游PortImportApply.cs中被置为空字符串，未实际使用
        var abSide = await CalculateABSide(targetSlot, trayInfo, locationType); //计算AB面
        return $"{targetSlot.SlotCode}&{abSide}";
    }
    /// <summary>
    /// 为空托盘查找库位
    /// </summary>
    /// <param name="stockCode">托盘码</param>
    /// <param name="trayInfo">托盘信息</param>
    /// <param name="areaId">区域ID</param>
    /// <returns>库位地址</returns>
    private async Task<string> FindSlotForEmptyTray(string stockCode, WmsSysStockCode trayInfo, string areaId)
    {
        // 查找已有空托存放的巷道
        var query = _repos.StockTray.AsQueryable()
            .LeftJoin<WmsSysStockCode>((t, sc) => t.StockCode == sc.StockCode) //左连接库存编码信息
            .LeftJoin<WmsBaseSlot>((t, sc, s) => t.StockSlotCode == s.SlotCode) //左连接库位信息
            .Where((t, sc, s) => SqlFunc.Subqueryable<WmsBaseMaterial>()
                                     .Where(m => m.Id.ToString() == t.MaterialId && m.IsEmpty == true && !m.IsDelete)
                                     .Any() && sc.StockType == trayInfo.StockType &&
                                t.WarehouseId == trayInfo.WarehouseId.ToString() && s.SlotImlockFlag == 0)
            .GroupBy((t, sc, s) => s.SlotLanewayId)
            .Select((t, sc, s) => new
            {
                LanewayId = s.SlotLanewayId,
                Count = SqlFunc.AggregateCount(s.SlotLanewayId)
            }).MergeTable().OrderBy(t => t.LanewayId);
        var lanewayGroups = await query.ToListAsync();
        // 在这些巷道中查找空库位
        foreach (var group in lanewayGroups)
        {
            if (group.LanewayId.HasValue)
            {
                var emptySlot = await _repos.Slot.GetFirstAsync(s =>
                    s.SlotLanewayId == group.LanewayId.Value && s.SlotStatus == SlotStatusCode.Empty &&
                    s.SlotImlockFlag == 0 && s.Make == "01");
                if (emptySlot != null)
                {
                    // 注意：此处计算的abSide值在下游PortImportApply.cs中被置为空字符串，未实际使用
                    var stockType = trayInfo.StockType.HasValue ? (int?)Convert.ToInt32(trayInfo.StockType.Value) : null;
                    var abSide = await CalculateABSideForEmptyTray(emptySlot, stockType);
                    return $"{emptySlot.SlotCode}&{abSide}";
                }
            }
        }
        return null;
    }
    /// <summary>
    /// 计算AB面（通用方法）
    /// </summary>
    /// <param name="targetSlot">目标库位</param>
    /// <param name="trayInfo">托盘信息</param>
    /// <param name="locationType">库位类型（huiku:回库）</param>
    /// <returns>AB面标识（A或B）</returns>
    /// <remarks>
    /// 【当前使用状态】：此方法计算的AB面值虽然被返回，但在PortImportApply.cs中被置为空字符串，未实际使用。
    ///
    /// 【计算规则】：
    /// 1. 无巷道ID → 返回A面（默认）
    /// 2. 单口巷道 → 返回A面
    /// 3. 多口巷道 → 根据目标库位深度与已占用库位深度比较：
    ///    - 目标深度 &lt; 最小已占用深度 → A面（从小深度入）
    ///    - 目标深度 &gt; 最大已占用深度 → B面（从大深度入）
    ///    - 目标深度在中间 → A面（默认）
    ///
    /// 【调用位置】：
    /// - FindSlotInSameBatchGroup (第288行)
    /// - FindEmptySlot (第315行)
    /// </remarks>
    private async Task<string> CalculateABSide(WmsBaseSlot targetSlot, TrayInformation trayInfo, string locationType)
    {
        if (!targetSlot.SlotLanewayId.HasValue)
            return ABSide.A;

        var laneway = await _repos.Laneway.GetFirstAsync(l => l.Id == targetSlot.SlotLanewayId.Value);
        if (laneway == null || laneway.LanewayType == LanewayTypeCode.SinglePort) //如果巷道信息为空，则返回默认A面
            return ABSide.A;

        if (laneway.LanewayType == LanewayTypeCode.MultiPort) // 双口巷道，根据深度判断
        {
            var lanewaySlots = await _repos.Slot.GetListAsync(s => s.SlotLanewayId == targetSlot.SlotLanewayId.Value && s.Make == "01");
            if (lanewaySlots.Count == 0) //如果巷道中已有的库位使用情况为空，则返回默认A面  没有找到空库位
                return ABSide.A;

            var occupiedSlots = await GetOccupiedSlotsInLaneway(laneway.Id, trayInfo);// 获取该巷道已有物料的库位（通过库存托盘和入库流水）
            if (occupiedSlots.Count == 0) //如果巷道中已有的库位使用情况为空，则返回默认A面  没有找到空库位
                return ABSide.A; // 空巷道,从小深度入
            // 获取已占用库位的最大深度和最小深度
            var maxDepth = occupiedSlots.Max(s => s.SlotInout);
            var minDepth = occupiedSlots.Min(s => s.SlotInout);

            if (targetSlot.SlotInout < minDepth)
                return ABSide.A; // 在最小深度之前 → A面
            else if (targetSlot.SlotInout > maxDepth)
                return ABSide.B; // 在最大深度之后 → B面
            else
                return ABSide.A; // 在中间位置,默认A面
        }
        return ABSide.A;
    }
    /// <summary>
    /// 计算空托盘的AB面（特殊规则）
    /// </summary>
    /// <param name="targetSlot">目标库位</param>
    /// <param name="stockType">托盘类型</param>
    /// <returns>AB面标识（A或B）</returns>
    /// <remarks>
    /// 【当前使用状态】：此方法计算的AB面值虽然被返回，但在PortImportApply.cs中被置为空字符串，未实际使用。
    ///
    /// 【计算规则】：
    /// 1. 无巷道ID → 返回A面
    /// 2. 仓储笼特殊规则：
    ///    - 非1层 → B面（从小深度入）
    ///    - 1层且库位数&gt;2 → B面（从小深度入）
    ///    - 1层且库位数≤2 → A面（从大深度入）
    /// 3. 普通托盘和质检托盘 → A面（从大深度入）
    ///
    /// 【调用位置】：
    /// - GetNullLocations (第134行)
    /// - FindSlotForEmptyTray (第351行)
    /// </remarks>
    private async Task<string> CalculateABSideForEmptyTray(WmsBaseSlot targetSlot, int? stockType)
    {
        if (!targetSlot.SlotLanewayId.HasValue)
            return ABSide.A;
        // 获取该巷道所有库位数量
        var lanewaySlots = await _repos.Slot.GetListAsync(s => s.SlotLanewayId == targetSlot.SlotLanewayId.Value && s.Make == "01");
        var slotCount = lanewaySlots.Count;
        var slotLayer = targetSlot.SlotLayer ?? 0;

        // 仓储笼特殊规则
        if (stockType == TrayType.StorageCage)
        {
            if (slotLayer != 1) return ABSide.B; // 非1层：从小深度入(B面)
            else if (slotCount > 2) return ABSide.B; // 1层且库位数>2：从小深度入(B面)
            else return ABSide.A; // 1层且库位数≤2：从大深度入(A面)
        }
        return ABSide.A; // 普通托盘和质检托盘：从大深度入(A面)
    }
    /// <summary>
    /// 获取巷道中已占用的库位
    /// </summary>
    /// <param name="lanewayId">巷道ID</param>
    /// <param name="trayInfo">托盘信息</param>
    /// <returns>已占用的库位列表</returns>
    private async Task<List<WmsBaseSlot>> GetOccupiedSlotsInLaneway(long lanewayId, TrayInformation trayInfo)
    {
        var occupiedSlots = new List<WmsBaseSlot>();

        // 1. 从库存托盘表查询已入库的库位
        var stockTrays = await _sqlSugarClient.Queryable<WmsStockTray, WmsBaseSlot>((t, s) => new JoinQueryInfos(
                JoinType.Left, t.StockSlotCode == s.SlotCode))
            .Where((t, s) => s.SlotLanewayId == lanewayId && t.MaterialId == trayInfo.MaterialCode &&
                            t.LotNo == trayInfo.LotNo && s.SlotImlockFlag == 0)
            .Select((t, s) => s)
            .ToListAsync();
        occupiedSlots.AddRange(stockTrays);

        // 2. 从入库流水表查询正在入库的库位
        var importOrders = await _sqlSugarClient.Queryable<WmsImportOrder, WmsBaseSlot, WmsStockSlotBoxInfo>((io, s, box) => new JoinQueryInfos(
                JoinType.Left, io.ImportSlotCode == s.SlotCode,
                JoinType.Left, io.Id == box.ImportOrderId))
            .Where((io, s, box) => s.SlotLanewayId == lanewayId && !io.IsDelete &&
                                   io.ImportExecuteFlag != ExecuteFlag.Completed && s.SlotImlockFlag == 0)
            .Select((io, s, box) => s)
            .ToListAsync();
        occupiedSlots.AddRange(importOrders);

        return occupiedSlots.Distinct().ToList();
    }
    #endregion

}