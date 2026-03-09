// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.BaseService;
using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Application.Service.WmsSqlView;
using Admin.NET.Core;
using Furion.FriendlyException;
using Newtonsoft.Json;
using SqlSugar;
using Microsoft.Extensions.Logging;
namespace Admin.NET.Application.Service.WmsPort.Process;
/// <summary>
/// 组托反馈业务类，对应 JC35 `PortController.GenerateGroupPallets`。
/// </summary>
public class PortImportGroup : PortBase, ITransient
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
    public PortImportGroup(
        ILoggerFactory loggerFactory,
        PortBaseRepos repos,
        ISqlSugarClient sqlSugarClient,
        WmsSqlViewService sqlViewService)
        : base(sqlSugarClient, loggerFactory.CreateLogger(CommonConst.PortImportGroup), "组托反馈")
    {
        _repos = repos;
        _sqlViewService = sqlViewService;
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 处理组托反馈。
    /// </summary>
    /// <param name="input">组托反馈参数。</param>
    /// <returns>组托结果。</returns>
    /// <remarks>
    /// - 仅实现 JC35 中针对入库单据（单号包含 RK）的逻辑。
    /// - 验收单据（YS）与挑浆单据（TJS）暂不实现，保持与旧系统一致的限制提示。
    /// </remarks>
    public async Task<GroupPalletFeedbackOutput> ProcessGroupPalletsAsync(GroupPalletFeedbackInput input)
    {
        // 1. 验证输入参数
        ValidateInput(input);
        // 2. 验证单据类型
        var billCode = input.BillCode.Trim();
        ValidateBillType(billCode);
        // 3. 记录开始日志
        await RecordOperationLogAsync(billCode, "组托反馈开始", input);
        // 4. 提取并规范化数据
        var materialCodes = ExtractMaterialCodes(input.List);
        var boxCodes = ExtractBoxCodes(input.List);
        var lotNos = ExtractLotNos(input.List);
        try
        {
            // 5. 开始事务
            _sqlSugarClient.Ado.BeginTran();
            // 6. 数据验证
            await ValidateMaterialsAsync(materialCodes);
            await ValidateBoxesAsync(boxCodes, input.VehicleCode);
            // 7. 准备托盘
            var stock = await PreparePalletAsync(input.VehicleCode);
            // 8. 加载入库单据明细
            var detailRecords = await LoadImportDetailsAsync(billCode, materialCodes, lotNos);
            if (detailRecords.Count == 0)
            {
                throw Oops.Bah("未能找到匹配的入库单，请核对单据与物料信息。");
            }
            // 9. 构建物料批次映射表（用于快速查找）
            var materialMap = BuildMaterialLotMap(detailRecords);
            // 10. 同步托盘所属仓库
            await SyncPalletWarehouseAsync(stock, detailRecords[0].Notify);
            // 11. 处理箱码明细
            var processResult = await ProcessBoxItemsAsync(input.List, materialMap, stock, detailRecords[0]);
            // 12. 更新相关状态
            await UpdateImportNotifyStatusAsync(detailRecords[0].Notify);
            await UpdatePalletStatusAsync(stock);
            // 13. 提交事务
            _sqlSugarClient.Ado.CommitTran();
            // 14. 记录成功日志
            var successMessage = $"组托完成，累计生成 {processResult.TotalBoxCount} 条箱码明细，重量合计 {processResult.TotalWeight:F3}。";
            await RecordOperationLogAsync(billCode, successMessage, new { input.VehicleCode, billCode, processResult.TotalWeight });
            return new GroupPalletFeedbackOutput
            {
                Success = true,
                Message = "组托完成"
            };
        }
        catch (Exception ex)
        {
            // 回滚事务并记录异常日志
            _sqlSugarClient.Ado.RollbackTran();
            await RecordOperationLogAsync(billCode, $"组托反馈异常：{ex.Message}", ex);
            throw;
        }
    }
    #endregion

    #region 私有方法 - 参数验证
    /// <summary>
    /// 验证输入参数
    /// </summary>
    /// <param name="input">输入参数</param>
    private static void ValidateInput(GroupPalletFeedbackInput input)
    {
        if (input == null)
        {
            throw Oops.Bah("请求参数不能为空！");
        }
        if (string.IsNullOrWhiteSpace(input.VehicleCode))
        {
            throw Oops.Bah("载具编码不能为空！");
        }
        if (string.IsNullOrWhiteSpace(input.BillCode))
        {
            throw Oops.Bah("单据编码不能为空！");
        }
        if (input.List == null || input.List.Count == 0)
        {
            throw Oops.Bah("箱码明细不能为空！");
        }
    }
    /// <summary>
    /// 验证单据类型是否为入库单据（RK）
    /// </summary>
    /// <param name="billCode">单据编码</param>
    private static void ValidateBillType(string billCode)
    {
        var billCodeUpper = billCode.ToUpperInvariant();
        if (!billCodeUpper.Contains(GroupPalletConstants.ImportBillTypePrefix))
        {
            throw Oops.Bah("不支持的单据类型，请确认单号是否正确。");
        }
    }
    /// <summary>
    /// 验证物料编码是否在系统中定义
    /// </summary>
    /// <param name="materialCodes">物料编码列表</param>
    private async Task ValidateMaterialsAsync(List<string> materialCodes)
    {
        if (materialCodes.Count == 0)
        {
            return;
        }
        var exists = await _repos.Material.AsQueryable()
            .Where(m => materialCodes.Contains(m.MaterialCode))
            .Select(m => m.MaterialCode)
            .ToListAsync();
        var missing = materialCodes.Except(exists, StringComparer.OrdinalIgnoreCase).ToList();
        if (missing.Count > 0)
        {
            var codes = string.Join(",", missing);
            throw Oops.Bah($"物品编码{codes}未定义，不可使用！");
        }
    }
    /// <summary>
    /// 验证箱码是否已存在于库存中（排除载具编码本身）
    /// </summary>
    /// <param name="boxCodes">箱码列表</param>
    /// <param name="vehicleCode">载具编码</param>
    private async Task ValidateBoxesAsync(List<string> boxCodes, string vehicleCode)
    {
        if (boxCodes.Count == 0)
        {
            return;
        }
        var duplicates = await _repos.StockInfo.AsQueryable()
            .Where(s => boxCodes.Contains(s.BoxCode))
            .Where(s => s.BoxCode != vehicleCode)
            .Select(s => s.BoxCode)
            .ToListAsync();
        if (duplicates.Count > 0)
        {
            var boxes = string.Join(",", duplicates);
            throw Oops.Bah($"箱条码{boxes}已存在，不可入库！");
        }
    }
    #endregion

    #region 私有方法 - 数据提取
    /// <summary>
    /// 从输入列表中提取物料编码（去重、去空、转大写）
    /// </summary>
    /// <param name="items">箱码明细列表</param>
    /// <returns>物料编码列表</returns>
    private static List<string> ExtractMaterialCodes(List<GroupPalletItemInput> items)
    {
        return items
            .Where(item => !string.IsNullOrWhiteSpace(item.MaterialCode))
            .Select(item => item.MaterialCode!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
    /// <summary>
    /// 从输入列表中提取箱码（去重、去空、转大写）
    /// </summary>
    /// <param name="items">箱码明细列表</param>
    /// <returns>箱码列表</returns>
    private static List<string> ExtractBoxCodes(List<GroupPalletItemInput> items)
    {
        return items
            .Where(item => !string.IsNullOrWhiteSpace(item.BoxCode))
            .Select(item => item.BoxCode!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
    /// <summary>
    /// 从输入列表中提取批次号（去重、去空）
    /// </summary>
    /// <param name="items">箱码明细列表</param>
    /// <returns>批次号列表</returns>
    private static List<string> ExtractLotNos(List<GroupPalletItemInput> items)
    {
        return items
            .Select(x => x.LotNo?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList()!;
    }
    #endregion

    #region 私有方法 - 托盘管理
    /// <summary>
    /// 准备托盘：查询或创建托盘，并验证其可用性
    /// </summary>
    /// <param name="vehicleCode">载具编码</param>
    /// <returns>托盘实体</returns>
    private async Task<WmsSysStockCode> PreparePalletAsync(string vehicleCode)
    {
        var stock = await _repos.StockCode.GetFirstAsync(s => s.StockCode == vehicleCode);
        // 如果载具编码长度大于8且托盘不存在，则创建新托盘
        if (vehicleCode.Length > GroupPalletConstants.PalletMinLength && stock == null)
        {
            stock = await CreateNewPalletAsync(vehicleCode);
        }
        // 如果托盘已存在，则重置其状态为空闲
        else if (stock != null)
        {
            await ResetPalletStatusAsync(stock);
        }
        // 验证托盘是否有效
        if (stock == null)
        {
            throw Oops.Bah("托盘条码不受WMS管理，不可使用！");
        }
        // 验证托盘是否正在使用
        await ValidatePalletAvailabilityAsync(stock, vehicleCode);
        return stock;
    }
    /// <summary>
    /// 创建新的托盘记录
    /// </summary>
    /// <param name="vehicleCode">载具编码</param>
    /// <returns>新创建的托盘实体</returns>
    private async Task<WmsSysStockCode> CreateNewPalletAsync(string vehicleCode)
    {
        var now = DateTime.Now;
        var stock = new WmsSysStockCode
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            StockCode = vehicleCode,
            Status = GroupPalletConstants.PalletStatusIdle,
            PrintCount = 0,
            StockType = 1,
            WarehouseId = GroupPalletConstants.DefaultWarehouseId,
            CreateTime = now,
            UpdateTime = now,
            CreateUserId = GroupPalletConstants.SystemUserId,
            CreateUserName = GroupPalletConstants.SystemUserName,
            UpdateUserId = GroupPalletConstants.SystemUserId,
            UpdateUserName = GroupPalletConstants.SystemUserName,
            IsDelete = false
        };
        await _repos.StockCode.InsertAsync(stock);
        return stock;
    }
    /// <summary>
    /// 重置托盘状态为空闲
    /// </summary>
    /// <param name="stock">托盘实体</param>
    private async Task ResetPalletStatusAsync(WmsSysStockCode stock)
    {
        var now = DateTime.Now;
        stock.Status = GroupPalletConstants.PalletStatusIdle;
        stock.UpdateTime = now;
        await _repos.StockCode.AsUpdateable(stock)
            .UpdateColumns(s => new { s.Status, s.UpdateTime })
            .ExecuteCommandAsync();
    }
    /// <summary>
    /// 验证托盘是否可用（未被使用）
    /// </summary>
    /// <param name="stock">托盘实体</param>
    /// <param name="vehicleCode">载具编码</param>
    private async Task ValidatePalletAvailabilityAsync(WmsSysStockCode stock, string vehicleCode)
    {
        var trays = await _repos.StockTray.AsQueryable()
            .Where(t => t.StockCode == vehicleCode && !t.IsDelete)
            .ToListAsync();
        if (stock.Status != GroupPalletConstants.PalletStatusIdle && trays.Count > 0)
        {
            throw Oops.Bah("托盘条码已经使用，不可使用！");
        }
    }
    /// <summary>
    /// 同步托盘所属仓库（如果托盘没有仓库信息，则从入库单据中获取）
    /// </summary>
    /// <param name="stock">托盘实体</param>
    /// <param name="notify">入库单据实体</param>
    private async Task SyncPalletWarehouseAsync(WmsSysStockCode stock, WmsImportNotify notify)
    {
        if (!stock.WarehouseId.HasValue && notify.WarehouseId.HasValue)
        {
            var now = DateTime.Now;
            stock.WarehouseId = notify.WarehouseId;
            stock.UpdateTime = now;
            await _repos.StockCode.AsUpdateable(stock)
                .UpdateColumns(s => new { s.WarehouseId, s.UpdateTime })
                .ExecuteCommandAsync();
        }
    }
    /// <summary>
    /// 更新托盘状态为使用中
    /// </summary>
    /// <param name="stock">托盘实体</param>
    private async Task UpdatePalletStatusAsync(WmsSysStockCode stock)
    {
        var now = DateTime.Now;
        stock.Status = GroupPalletConstants.PalletStatusInUse;
        stock.UpdateTime = now;
        stock.UpdateUserId = GroupPalletConstants.SystemUserId;
        stock.UpdateUserName = GroupPalletConstants.SystemUserName;
        await _repos.StockCode.AsUpdateable(stock)
            .UpdateColumns(s => new { s.Status, s.UpdateTime, s.UpdateUserId, s.UpdateUserName })
            .ExecuteCommandAsync();
    }
    #endregion

    #region 私有方法 - 数据加载
    /// <summary>
    /// 加载入库单据明细记录
    /// </summary>
    /// <param name="billCode">单据编码</param>
    /// <param name="materialCodes">物料编码列表</param>
    /// <param name="lotNos">批次号列表</param>
    /// <returns>入库单据明细记录列表</returns>
    private async Task<List<ImportDetailRecord>> LoadImportDetailsAsync(string billCode, List<string> materialCodes, List<string> lotNos)
    {
        if (materialCodes.Count == 0 || lotNos.Count == 0)
        {
            return new List<ImportDetailRecord>();
        }
        // 使用视图查询获取符合条件的记录ID
        var viewRecords = await _sqlViewService.QueryImportNotifyDetailView()
            .MergeTable()
            .Where(x => !x.IsDel && x.ImportBillCode == billCode)
            .Where(x => materialCodes.Contains(x.MaterialCode))
            .Where(x => x.ImportExecuteFlag == "01" || x.ImportExecuteFlag == "02" || x.ImportExecuteFlag == "03")
            .Where(x => lotNos.Contains(x.LotNo))
            .Select(x => new { x.Id, x.ImportId, x.MaterialId, x.MaterialCode })
            .ToListAsync();
        if (viewRecords.Count == 0)
        {
            return new List<ImportDetailRecord>();
        }
        // 批量查询实体数据
        var detailIds = viewRecords.Select(x => x.Id).ToList();
        var importIds = viewRecords.Select(x => x.ImportId)
            .Where(x => x.HasValue)
            .Select(x => x.Value)
            .Distinct()
            .ToList();
        var materialIds = viewRecords.Select(x => x.MaterialId)
            .Where(x => x.HasValue)
            .Select(x => x.Value)
            .Distinct()
            .ToList();
        var details = await _repos.ImportNotifyDetail.GetListAsync(d => detailIds.Contains(d.Id));
        var notifies = await _repos.ImportNotify.GetListAsync(n => importIds.Contains(n.Id));
        var materials = await _repos.Material.GetListAsync(m => materialIds.Contains(m.Id));

        // 构建字典以提升查找性能（O(1)复杂度）
        var detailDict = details.ToDictionary(d => d.Id);
        var notifyDict = notifies.ToDictionary(n => n.Id);
        var materialDict = materials.ToDictionary(m => m.Id);

        // 使用字典快速查找并构建结果列表
        var result = viewRecords
            .Where(vr => vr.ImportId.HasValue && vr.MaterialId.HasValue &&
                         detailDict.ContainsKey(vr.Id) &&
                         notifyDict.ContainsKey(vr.ImportId.Value) &&
                         materialDict.ContainsKey(vr.MaterialId.Value))
            .Select(vr => new ImportDetailRecord
            {
                Detail = detailDict[vr.Id],
                Notify = notifyDict[vr.ImportId.Value],
                Material = materialDict[vr.MaterialId.Value],
                MaterialCode = vr.MaterialCode
            })
            .ToList();

        return result;
    }
    /// <summary>
    /// 构建物料批次映射表（用于快速查找）
    /// </summary>
    /// <param name="detailRecords">入库单据明细记录列表</param>
    /// <returns>物料批次映射字典，Key格式：物料编码_批次号（大写），Value：入库单据明细记录</returns>
    private static Dictionary<string, ImportDetailRecord> BuildMaterialLotMap(List<ImportDetailRecord> detailRecords)
    {
        return detailRecords.ToDictionary(
            key => $"{key.MaterialCode}_{key.Detail.LotNo}".ToUpperInvariant(),
            value => value);
    }
    #endregion

    #region 私有方法 - 箱码处理
    /// <summary>
    /// 处理箱码明细：创建入库流水、箱码信息和更新入库单据
    /// </summary>
    /// <param name="boxItems">箱码明细列表</param>
    /// <param name="materialMap">物料批次映射表</param>
    /// <param name="stock">托盘实体</param>
    /// <param name="firstDetailInfo">第一个入库单据明细记录（用于创建入库流水）</param>
    /// <returns>处理结果（包含总箱数和总重量）</returns>
    private async Task<BoxProcessResult> ProcessBoxItemsAsync(List<GroupPalletItemInput> boxItems, Dictionary<string, ImportDetailRecord> materialMap, WmsSysStockCode stock,
        ImportDetailRecord firstDetailInfo)
    {
        var commonMethod = new CommonMethod(_sqlSugarClient);
        WmsImportOrder importOrder = null;
        var totalWeight = 0m;
        var processedCount = 0;
        foreach (var box in boxItems)
        {
            // 构建查找键：物料编码_批次号
            var key = $"{box.MaterialCode?.Trim()}_{box.LotNo?.Trim()}".ToUpperInvariant();
            // 如果找不到匹配的入库单据明细，则跳过
            if (!materialMap.TryGetValue(key, out var detailInfo))
            {
                continue;
            }
            // 创建入库流水（仅创建一次）
            if (importOrder == null)
            {
                importOrder = await CreateImportOrderAsync(commonMethod, stock, firstDetailInfo, boxItems);
            }
            // 创建箱码信息
            await CreateBoxInfoAsync(stock, importOrder, detailInfo, box);
            // 累加重量和计数
            totalWeight += box.Weight;
            processedCount++;
            // 更新入库单据明细
            await UpdateImportDetailAsync(detailInfo, box);
        }
        // 验证是否处理了至少一个箱码
        if (importOrder == null)
        {
            throw Oops.Bah("未生成有效的箱码明细，请检查批次与物料是否匹配。");
        }
        return new BoxProcessResult
        {
            TotalBoxCount = processedCount,
            TotalWeight = totalWeight
        };
    }
    /// <summary>
    /// 创建入库流水
    /// </summary>
    /// <param name="commonMethod">通用方法实例</param>
    /// <param name="stock">托盘实体</param>
    /// <param name="detailInfo">入库单据明细记录</param>
    /// <param name="items">箱码明细列表</param>
    /// <returns>入库流水实体</returns>
    private async Task<WmsImportOrder> CreateImportOrderAsync(CommonMethod commonMethod, WmsSysStockCode stock, ImportDetailRecord detailInfo, List<GroupPalletItemInput> items)
    {
        var now = DateTime.Now;
        var importOrder = new WmsImportOrder
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            ImportOrderNo = commonMethod.GetImExNo(GroupPalletConstants.ImportBillTypePrefix),
            ImportId = detailInfo.Detail.ImportId,
            ImportDetailId = detailInfo.Detail.Id,
            StockCodeId = stock.Id,
            StockCode = stock.StockCode,
            ImportSlotCode = string.Empty,
            ImportQuantity = items.Count,
            ImportWeight = items.Sum(x => x.Weight),
            ImportExecuteFlag = GroupPalletConstants.ExecuteFlagPending,
            Remark = "自动组托",
            LotNo = detailInfo.Detail.LotNo,
            ImportProductionDate = detailInfo.Detail.ImportProductionDate,
            InspectionStatus = ParseStatus(items.FirstOrDefault()?.InspectionStatus),
            WareHouseId = detailInfo.Notify.WarehouseId,
            SubVehicleCode = stock.StockCode,
            CreateTime = now,
            CreateUserId = GroupPalletConstants.SystemUserId,
            CreateUserName = GroupPalletConstants.SystemUserName,
            UpdateTime = now,
            UpdateUserId = GroupPalletConstants.SystemUserId,
            UpdateUserName = GroupPalletConstants.SystemUserName,
            IsDelete = false,
            YsOrTJ = "0"
        };
        await _repos.ImportOrder.InsertAsync(importOrder);
        return importOrder;
    }
    /// <summary>
    /// 创建箱码信息
    /// </summary>
    /// <param name="stock">托盘实体</param>
    /// <param name="importOrder">入库流水实体</param>
    /// <param name="detailInfo">入库单据明细记录</param>
    /// <param name="item">箱码明细</param>
    private async Task CreateBoxInfoAsync(WmsSysStockCode stock, WmsImportOrder importOrder, ImportDetailRecord detailInfo, GroupPalletItemInput item)
    {
        var now = DateTime.Now;
        var boxInfo = new WmsStockSlotBoxInfo
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            BoxCode = item.BoxCode,
            Qty = item.Qty <= 0 ? 1 : item.Qty,
            FullBoxQty = 0,
            StockCode = stock.StockCode,
            StockCodeId = stock.Id,
            Status = GroupPalletConstants.PalletStatusIdle,
            ImportId = detailInfo.Detail.ImportId,
            ImportDetailId = detailInfo.Detail.Id,
            ImportOrderId = importOrder.Id,
            BoxLevel = 0,
            ProductionDate = item.ProductionDate,
            ValidateDay = item.ValidateDay,
            LotNo = detailInfo.Detail.LotNo,
            MaterialId = detailInfo.Detail.MaterialId,
            SupplierId = detailInfo.Notify?.SupplierId,
            ManufacturerId = detailInfo.Notify?.ManufacturerId,
            BulkTank = item.BulkTank,
            IsSamplingBox = 0,
            InspectionStatus = ParseStatus(item.InspectionStatus),
            SamplingDate = item.SamplingDate,
            StaffCode = item.StaffCode,
            StaffName = item.StaffName,
            Weight = item.Weight,
            ExtractStatus = ParseStatus(item.ExtractStatus),
            CreateTime = now,
            CreateUserId = GroupPalletConstants.SystemUserId,
            CreateUserName = GroupPalletConstants.SystemUserName,
            UpdateTime = now,
            UpdateUserId = GroupPalletConstants.SystemUserId,
            UpdateUserName = GroupPalletConstants.SystemUserName,
            IsDelete = false
        };
        await _repos.BoxInfo.InsertAsync(boxInfo);
    }
    /// <summary>
    /// 更新入库单据明细：累加实际数量并更新执行状态
    /// </summary>
    /// <param name="detailInfo">入库单据明细记录</param>
    /// <param name="item">箱码明细</param>
    /// <remarks>
    /// 注意：仓库为血浆库（ID=1）时不累加FactQuantity，与旧系统保持一致。
    /// </remarks>
    private async Task UpdateImportDetailAsync(ImportDetailRecord detailInfo, GroupPalletItemInput item)
    {
        var now = DateTime.Now;
        var detail = detailInfo.Detail;
        var notify = detailInfo.Notify;
        var needUpdateQuantity = detail.ImportFactQuantity ?? 0;
        var qty = item.Qty <= 0 ? 1 : item.Qty;
        // 仓库为血浆库（ID=1）时不累加FactQuantity，与旧系统保持一致
        if (notify?.WarehouseId != GroupPalletConstants.PlasmaWarehouseId)
        {
            needUpdateQuantity += qty;
            detail.ImportFactQuantity = needUpdateQuantity;
        }
        // 如果执行状态为待执行，则更新为执行中
        if (string.Equals(detail.ImportExecuteFlag, GroupPalletConstants.ExecuteFlagPending, StringComparison.OrdinalIgnoreCase))
        {
            detail.ImportExecuteFlag = GroupPalletConstants.ExecuteFlagProcessing;
        }
        // 更新审计字段
        detail.UpdateTime = now;
        detail.UpdateUserId = GroupPalletConstants.SystemUserId;
        detail.UpdateUserName = GroupPalletConstants.SystemUserName;
        await _repos.ImportNotifyDetail.AsUpdateable(detail)
            .UpdateColumns(d => new
            {
                d.ImportFactQuantity,
                d.ImportExecuteFlag,
                d.UpdateTime,
                d.UpdateUserId,
                d.UpdateUserName
            })
            .ExecuteCommandAsync();
    }
    /// <summary>
    /// 更新入库单据状态：如果状态为待执行，则更新为执行中
    /// </summary>
    /// <param name="notify">入库单据实体</param>
    private async Task UpdateImportNotifyStatusAsync(WmsImportNotify notify)
    {
        if (string.Equals(notify.ImportExecuteFlag, GroupPalletConstants.ExecuteFlagPending, StringComparison.OrdinalIgnoreCase))
        {
            var now = DateTime.Now;
            notify.ImportExecuteFlag = GroupPalletConstants.ExecuteFlagProcessing;
            notify.UpdateTime = now;
            notify.UpdateUserId = GroupPalletConstants.SystemUserId;
            notify.UpdateUserName = GroupPalletConstants.SystemUserName;
            await _repos.ImportNotify.AsUpdateable(notify)
                .UpdateColumns(n => new { n.ImportExecuteFlag, n.UpdateTime, n.UpdateUserId, n.UpdateUserName })
                .ExecuteCommandAsync();
        }
    }
    #endregion

}
