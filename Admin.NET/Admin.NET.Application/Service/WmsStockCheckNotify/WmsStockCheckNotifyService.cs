// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsBaseOperLog;
using Admin.NET.Application.Service.WmsBaseOperLog.Enum;
using Admin.NET.Core;
using AngleSharp.Dom;
using Dm.util;
using Furion.FriendlyException;
using Furion.HttpRemote;
using Mapster;
using SqlSugar;
using System.ComponentModel;
using System.Text;

namespace Admin.NET.Application;

/// <summary>
/// 盘点单据服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsStockCheckNotifyService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsStockCheckNotify> _wmsStockCheckNotifyRep;
    private readonly SqlSugarRepository<WmsStockCheckNotifyDetail> _wmsStockCheckNotifyDetailRep;
    private readonly SqlSugarRepository<WmsStockCheckTask> _wmsStockCheckTaskRep;
    private readonly SqlSugarRepository<WmsStockCheckInfo> _wmsStockCheckInfoRep;
    private readonly SqlSugarRepository<WmsStockTray> _wmsStockTrayRep;
    private readonly SqlSugarRepository<WmsStockInfo> _wmsStockInfoRep;
    private readonly SqlSugarRepository<WmsStock> _wmsStockRep;
    private readonly SqlSugarRepository<WmsBaseMaterial> _wmsBaseMaterialRep;
    private readonly SqlSugarRepository<WmsBaseWareHouse> _wmsBaseWareHouseRep;
    private readonly SqlSugarRepository<WmsBaseSlot> _wmsBaseSlotRep;
    private readonly SqlSugarRepository<WmsBaseLaneway> _wmsBaseLanewayRep;
    private readonly IHttpRemoteService _httpRemoteService;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsStockCheckNotifyService(
        SqlSugarRepository<WmsStockCheckNotify> wmsStockCheckNotifyRep,
        SqlSugarRepository<WmsStockCheckNotifyDetail> wmsStockCheckNotifyDetailRep,
        SqlSugarRepository<WmsStockCheckTask> wmsStockCheckTaskRep,
        SqlSugarRepository<WmsStockCheckInfo> wmsStockCheckInfoRep,
        SqlSugarRepository<WmsStockTray> wmsStockTrayRep,
        SqlSugarRepository<WmsStockInfo> wmsStockInfoRep,
        SqlSugarRepository<WmsStock> wmsStockRep,
        SqlSugarRepository<WmsBaseMaterial> wmsBaseMaterialRep,
        SqlSugarRepository<WmsBaseWareHouse> wmsBaseWareHouseRep,
        SqlSugarRepository<WmsBaseSlot> wmsBaseSlotRep,
        SqlSugarRepository<WmsBaseLaneway> wmsBaseLanewayRep,
        IHttpRemoteService httpRemoteService,
        ISqlSugarClient sqlSugarClient)
    {
        _wmsStockCheckNotifyRep = wmsStockCheckNotifyRep;
        _wmsStockCheckNotifyDetailRep = wmsStockCheckNotifyDetailRep;
        _wmsStockCheckTaskRep = wmsStockCheckTaskRep;
        _wmsStockCheckInfoRep = wmsStockCheckInfoRep;
        _wmsStockTrayRep = wmsStockTrayRep;
        _wmsStockInfoRep = wmsStockInfoRep;
        _wmsStockRep = wmsStockRep;
        _wmsBaseMaterialRep = wmsBaseMaterialRep;
        _wmsBaseWareHouseRep = wmsBaseWareHouseRep;
        _wmsBaseSlotRep = wmsBaseSlotRep;
        _wmsBaseLanewayRep = wmsBaseLanewayRep;
        _httpRemoteService = httpRemoteService;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询盘点单据 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询盘点单据")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsStockCheckNotifyOutput>> Page(PageWmsStockCheckNotifyInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsStockCheckNotifyRep.AsQueryable()
            .LeftJoin<WmsBaseWareHouse>((u, h) => u.WarehouseId == h.Id.ToString())
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.CheckBillCode.Contains(input.Keyword) || u.CheckRemark.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.CheckBillCode), u => u.CheckBillCode.Contains(input.CheckBillCode.Trim()))
            .WhereIF(input.ExecuteFlag.HasValue, u => u.ExecuteFlag == input.ExecuteFlag)
            .WhereIF(input.StartTime.HasValue, u => u.CheckDate >= input.StartTime)
            .WhereIF(input.EndTime.HasValue, u => u.CheckDate <= input.EndTime)
            .WhereIF(!string.IsNullOrWhiteSpace(input.WarehouseId), u => u.WarehouseId == input.WarehouseId)
            .Where(u => u.IsDelete == false)
            .Select<WmsStockCheckNotifyOutput>();
        return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取盘点单明细列表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取盘点单明细列表")]
    [ApiDescriptionSettings(Name = "GetDetailList"), HttpPost]
    public async Task<SqlSugarPagedList<WmsStockCheckNotifyDetailOutput>> GetDetailList(GetStockCheckDetailListInput input)
    {
        var query = _wmsStockCheckNotifyDetailRep.AsQueryable()
            .LeftJoin<WmsBaseMaterial>((a, d) => a.MaterialId == d.Id)
            .LeftJoin<WmsBaseUnit>((a, d,u) => d.MaterialUnit == u.Id)
            .WhereIF(!string.IsNullOrWhiteSpace(input.BillCode), a => a.CheckBillCode == input.BillCode)
            .WhereIF(!string.IsNullOrWhiteSpace(input.StockCode), (a, d, u) => a.StockCode == input.StockCode)
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), (a, d, u) => d.MaterialCode == input.MaterialCode)
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialName), (a, d, u) => d.MaterialName.Contains(input.MaterialName))
            .Select((a, d,u) => new WmsStockCheckNotifyDetailOutput {
                Id=a.Id,
                CheckBillCode=a.CheckBillCode,
                InspectionStatus=a.InspectionStatus,
                ExecuteFlag=a.ExecuteFlag,
                AddDate=a.AddDate,
                CheckRemark=a.CheckRemark,
                CheckResult=a.CheckResult,
                CheckStockId=a.CheckStockId,
                CheckTaskNo=a.CheckTaskNo,
                MaterialId=a.MaterialId,
                MaterialCode=d.MaterialCode,
                MaterialName=d.MaterialName,
                UnitName=u.UnitName,
                MaterialStandard=d.MaterialStandard,
                StockCode=a.StockCode,
                StockLotNo=a.StockLotNo,
                RealQuantity=a.RealQuantity,
                StockQuantity=a.StockQuantity,
                StockSlot=a.StockSlot,
                TaskId=a.TaskId,
                WarehouseId=a.WarehouseId
            });
        return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取箱码明细 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取箱码明细")]
    [ApiDescriptionSettings(Name = "GetBoxDetail"), HttpPost]
    public async Task<SqlSugarPagedList<WmsStockCheckBoxDetailOutput>> GetBoxDetail(GetBoxDetailInput input)
    {
        var detail = await _wmsStockCheckNotifyDetailRep.AsQueryable().FirstAsync(o => o.Id == input.Id);
        if (detail == null)
        {
            throw Oops.Oh("获取箱码明细失败！");
        }
        var list = _wmsStockCheckInfoRep.AsQueryable()
            .Where(a => a.StockCheckId == detail.Id).Select(a => new WmsStockCheckBoxDetailOutput
        {
            Id = a.Id,
            BoxCode = a.BoxCode,
            CheckResult = a.CheckResult,
            MaterialCode = a.MaterialCode,
            MaterialName = a.MaterialName,
            LotNo = a.LotNo,
            Qty = a.Qty,
            RealQuantity=a.RealQuantity,
            BulkTank =a.PickingSlurry,
            ExtractStatus = a.ExtractStatus,
            InspectionStatus = a.InspectionStatus,
            ProductionDate = a.ProductionDate,
            ValidateDay = a.ValidateDay
        }).ToList();
        return list.OrderBy(a => a.MaterialCode).ToPagedList(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取库存列表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取库存列表")]
    [ApiDescriptionSettings(Name = "GetStockSlotList"), HttpPost]
    public async Task<List<WmsStockSlotOutput>> GetStockSlotsAsync(GetStockSlotInput input)
    {
        // 构建主查询 - 只查询状态正常的库存（StockStatusFlag=0 且 AbnormalStatu=1）
        var query = _sqlSugarClient.Queryable<WmsStockTray>()
            .LeftJoin<WmsBaseMaterial>((a, d) => a.MaterialId == d.Id.ToString())
            .Where((a, d) => a.StockQuantity > 0)
            .Where((a, d) => a.StockStatusFlag == 0 || a.StockStatusFlag == null)
            .Where((a, d) => a.AbnormalStatu == 0 || a.AbnormalStatu == null);

        // 添加条件过滤
        if (!string.IsNullOrWhiteSpace(input.WarehouseId))
            query = query.Where((a, d) => SqlFunc.ToString(d.WarehouseId) == input.WarehouseId);

        if (!string.IsNullOrWhiteSpace(input.LanewayId))
            query = query.Where((a, d) => a.LanewayId == input.LanewayId);

        if (!string.IsNullOrWhiteSpace(input.LotNo))
            query = query.Where((a, d) => a.LotNo.Contains(input.LotNo));

        if (!string.IsNullOrWhiteSpace(input.MaterialCode))
            query = query.Where((a, d) => d.MaterialCode == input.MaterialCode);

        if (!string.IsNullOrWhiteSpace(input.MaterialName))
            query = query.Where((a, d) => d.MaterialName.Contains(input.MaterialName));

        if (!string.IsNullOrWhiteSpace(input.SlotCode))
            query = query.Where((a, d) => a.StockSlotCode.Contains(input.SlotCode));

        if (!string.IsNullOrWhiteSpace(input.StockCode))
            query = query.Where((a, d) => a.StockCode.Contains(input.StockCode));

        // 添加盘查数据排除条件（只在有仓库ID时应用）
        if (!string.IsNullOrWhiteSpace(input.WarehouseId))
        {
            var stockCheckSubQuery = _sqlSugarClient.Queryable<WmsStockCheckNotifyDetail>()
                .Where(o => (o.ExecuteFlag == 0 || o.ExecuteFlag == 1)
                         && o.IsDelete == false
                         && o.WarehouseId == input.WarehouseId)
                 .WhereIF(input.allAddStockCode.Count>0, o => !input.allAddStockCode.Contains(o.StockCode))
                .Select(o => o.StockCode).ToList();

            query = query.Where((a, d) => !stockCheckSubQuery.Contains(a.StockCode));
        }

        if (input.allAddCheckStockId.Count > 0)
        {
            query = query.Where((a, d) => !input.allAddCheckStockId.Contains(a.Id));
        }

        // 选择需要的字段
        var result = query.Select((a, d) => new WmsStockSlotOutput
        {
            Id=a.Id,
            CheckStockId = a.Id,
            WarehouseId = a.WarehouseId,
            LanewayId = a.LanewayId,
            LotNo = a.LotNo,
            MaterialId=d.Id,
            MaterialCode = d.MaterialCode,
            MaterialName = d.MaterialName,
            MaterialStandard=d.MaterialStandard,
            InspectionStatus=a.InspectionStatus,
            SlotCode = a.StockSlotCode,
            StockCode = a.StockCode,
            StockQuantity = a.StockQuantity
        }).OrderBy(a=>a.StockCode);

        return await result.ToListAsync();
    }

    /// <summary>
    /// 添加盘点单 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("添加盘点单")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsStockCheckNotifyInput input)
    {
        var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
        var userName = App.User?.FindFirst(ClaimConst.RealName)?.Value;

        if (input.List.Any(a => string.IsNullOrWhiteSpace(a.StockSlot)))
        {
            throw Oops.Oh("选择的盘库储位不能为空！");
        }

        try
        {
            _sqlSugarClient.Ado.BeginTran();

            var entity = input.Adapt<WmsStockCheckNotify>();
            entity.CheckBillCode = await GetCheckBillCode();
            entity.CheckDate = DateTime.Now;
            entity.AddDate = DateTime.Now;
            entity.ExecuteFlag = 0;
            entity.CreateUserId = userId;
            entity.UpdateUserId = userId;
            entity.UpdateUserName = userName;
            entity.UpdateTime = DateTime.Now;
            entity.IsDelete = false;

            var res = await _wmsStockCheckNotifyRep.InsertAsync(entity);
            if (!res)
            {
                throw Oops.Oh("创建盘点单失败！");
            }

            foreach (var detail in input.List)
            {
                detail.CheckBillCode = entity.CheckBillCode;
                detail.WarehouseId = entity.WarehouseId;
                detail.ExecuteFlag = 0;
                detail.AddDate = DateTime.Now;
            }

            var details = input.List.Adapt<List<WmsStockCheckNotifyDetail>>();

            foreach (var item in input.List)
            {
                var stockcheckdetail = await _wmsStockCheckNotifyDetailRep.AsQueryable()
                    .Where(m => m.StockSlot == item.StockSlot
                        && m.IsDelete == false
                        && m.ExecuteFlag != 3
                        && m.MaterialId == item.MaterialId
                        && m.StockLotNo == item.StockLotNo
                        && m.InspectionStatus == item.InspectionStatus
                        && m.StockCode == item.StockCode)
                    .FirstAsync();

                if (stockcheckdetail != null)
                {
                    throw Oops.Oh($"物品 {item.StockLotNo} 批次正在盘库！");
                }
            }

            await _wmsStockCheckNotifyDetailRep.InsertRangeAsync(details);

            // 校验载具并添加箱码明细
            foreach (var item in input.List)
            {
                await ValidateAndCreateStockCheckInfo(item, entity.CheckBillCode, userId);
            }

            _sqlSugarClient.Ado.CommitTran();

            await WmsBaseOperLogHelper.RecordAsync(
                Module: "盘点单据",
                OperationType: OperationTypeEnum.Add.GetEnumDescription(),
                BusinessNo: entity.CheckBillCode,
                OperationContent: $"添加盘点单据：{entity.CheckBillCode} 成功",
                OperParam: input
            );

            return entity.Id;
        }
        catch (Exception ex)
        {
            _sqlSugarClient.Ado.RollbackTran();
            throw Oops.Oh($"添加盘点单失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 更新盘点单据 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新盘点单据")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsStockCheckNotifyInput input)
    {
        var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
        var userName = App.User?.FindFirst(ClaimConst.RealName)?.Value;

        if (input.List.Any(a => string.IsNullOrWhiteSpace(a.StockSlot)))
        {
            throw Oops.Oh("选择的盘库储位不能为空！");
        }

        try
        {
            _sqlSugarClient.Ado.BeginTran();

            var entity = await _wmsStockCheckNotifyRep.GetFirstAsync(u => u.Id == input.Id);
            if (entity == null)
            {
                throw Oops.Oh("盘点单据不存在");
            }

            if (entity.ExecuteFlag != 0)
            {
                throw Oops.Oh("只有待执行状态的盘点单才能编辑");
            }

            entity.WarehouseId = input.WarehouseId;
            entity.CheckRemark = input.CheckRemark;
            entity.UpdateUserId = userId;
            entity.UpdateUserName = userName;
            entity.UpdateTime = DateTime.Now;
            await _wmsStockCheckNotifyRep.AsUpdateable(entity).ExecuteCommandAsync();

            if (input.List != null && input.List.Count > 0)
            {
                var existingDetails = await _wmsStockCheckNotifyDetailRep.AsQueryable()
                    .Where(u => u.CheckBillCode == entity.CheckBillCode && u.IsDelete == false)
                    .ToListAsync();

                var existingDetailIds = existingDetails.Select(d => d.Id).ToList();
                var inputDetailIds = input.List.Where(d => d.Id > 0).Select(d => d.Id).ToList();

                // 处理删除的明细：解锁载具并删除箱码明细
                var detailsToDelete = existingDetails.Where(d => !inputDetailIds.Contains(d.Id)).ToList();
                if (detailsToDelete.Count > 0)
                {
                    var detailIds = detailsToDelete.Select(d => d.Id).ToList();
                    await _wmsStockCheckInfoRep.AsUpdateable()
                        .SetColumns(it => it.IsDelete == true)
                        .Where(it => detailIds.Contains(it.StockCheckId) && it.IsDelete == false)
                        .ExecuteCommandAsync();

                    foreach (var detail in detailsToDelete)
                    {
                        var stockTray = await _wmsStockTrayRep.GetFirstAsync(a => a.StockCode == detail.StockCode);
                        if (stockTray != null)
                        {
                            stockTray.StockStatusFlag = 0;
                            await _wmsStockTrayRep.AsUpdateable(stockTray).ExecuteCommandAsync();
                        }
                    }

                    foreach (var detail in detailsToDelete)
                    {
                        detail.IsDelete = true;
                        detail.UpdateUserId = userId;
                        detail.UpdateTime = DateTime.Now;
                    }
                    await _wmsStockCheckNotifyDetailRep.AsUpdateable(detailsToDelete).ExecuteCommandAsync();
                }

                foreach (var detailInput in input.List)
                {
                    if (detailInput.Id > 0)
                    {
                        // 更新现有明细
                        var existingDetail = existingDetails.FirstOrDefault(d => d.Id == detailInput.Id);
                        if (existingDetail != null)
                        {
                            // 校验关键字段是否变更（储位、载具、物料、批次、质检状态）
                            bool isKeyChanged = existingDetail.StockSlot != detailInput.StockSlot
                                || existingDetail.StockCode != detailInput.StockCode
                                || existingDetail.MaterialId != detailInput.MaterialId
                                || existingDetail.StockLotNo != detailInput.StockLotNo
                                || existingDetail.InspectionStatus != detailInput.InspectionStatus;

                            if (isKeyChanged)
                            {
                                // 关键字段变更：先解锁旧载具
                                var oldStockTray = await _wmsStockTrayRep.GetFirstAsync(a => a.StockCode == existingDetail.StockCode);
                                if (oldStockTray != null)
                                {
                                    oldStockTray.StockStatusFlag = 0;
                                    await _wmsStockTrayRep.AsUpdateable(oldStockTray).ExecuteCommandAsync();
                                }

                                // 删除旧的箱码明细
                                await _wmsStockCheckInfoRep.AsUpdateable()
                                    .SetColumns(it => it.IsDelete == true)
                                    .Where(it => it.StockCheckId == existingDetail.Id && it.IsDelete == false)
                                    .ExecuteCommandAsync();

                                // 检查新记录是否重复
                                var stockcheckdetail = await _wmsStockCheckNotifyDetailRep.AsQueryable()
                                    .Where(m => m.StockSlot == detailInput.StockSlot
                                        && m.IsDelete == false
                                        && m.ExecuteFlag != 3
                                        && m.MaterialId == detailInput.MaterialId
                                        && m.StockLotNo == detailInput.StockLotNo
                                        && m.InspectionStatus == detailInput.InspectionStatus
                                        && m.StockCode == detailInput.StockCode
                                        && m.Id != detailInput.Id)
                                    .FirstAsync();

                                if (stockcheckdetail != null)
                                {
                                    throw Oops.Oh($"物品 {detailInput.StockLotNo} 批次正在盘库！");
                                }

                                // 更新明细信息
                                existingDetail.StockSlot = detailInput.StockSlot;
                                existingDetail.StockCode = detailInput.StockCode;
                                existingDetail.StockQuantity = detailInput.StockQuantity;
                                existingDetail.StockLotNo = detailInput.StockLotNo;
                                existingDetail.MaterialId = detailInput.MaterialId;
                                existingDetail.InspectionStatus = detailInput.InspectionStatus;
                                existingDetail.WarehouseId = entity.WarehouseId;
                                existingDetail.CheckStockId = detailInput.CheckStockId;
                                existingDetail.UpdateUserId = userId;
                                existingDetail.UpdateTime = DateTime.Now;
                                await _wmsStockCheckNotifyDetailRep.AsUpdateable(existingDetail).ExecuteCommandAsync();

                                // 校验新载具、锁定并添加箱码明细
                                await ValidateAndCreateStockCheckInfo(detailInput, entity.CheckBillCode, userId);
                            }
                            else
                            {
                                // 非关键字段变更：直接更新
                                existingDetail.StockQuantity = detailInput.StockQuantity;
                                existingDetail.UpdateUserId = userId;
                                existingDetail.UpdateTime = DateTime.Now;
                                await _wmsStockCheckNotifyDetailRep.AsUpdateable(existingDetail).ExecuteCommandAsync();
                            }
                        }
                    }
                    else
                    {
                        // 新增明细：校验并锁定载具
                        var newDetail = new WmsStockCheckNotifyDetail
                        {
                            CheckBillCode = entity.CheckBillCode,
                            WarehouseId = entity.WarehouseId,
                            StockSlot = detailInput.StockSlot,
                            StockCode = detailInput.StockCode,
                            StockQuantity = detailInput.StockQuantity,
                            StockLotNo = detailInput.StockLotNo,
                            MaterialId = detailInput.MaterialId,
                            InspectionStatus = detailInput.InspectionStatus,
                            CheckStockId = detailInput.CheckStockId,
                            ExecuteFlag = 0,
                            AddDate = DateTime.Now,
                            CreateUserId = userId,
                            UpdateUserId = userId,
                            UpdateTime = DateTime.Now,
                            IsDelete = false
                        };

                        // 检查是否重复盘点
                        var stockcheckdetail = await _wmsStockCheckNotifyDetailRep.AsQueryable()
                            .Where(m => m.StockSlot == detailInput.StockSlot
                                && m.IsDelete == false
                                && m.ExecuteFlag != 3
                                && m.MaterialId == detailInput.MaterialId
                                && m.StockLotNo == detailInput.StockLotNo
                                && m.InspectionStatus == detailInput.InspectionStatus
                                && m.StockCode == detailInput.StockCode)
                            .FirstAsync();

                        if (stockcheckdetail != null)
                        {
                            throw Oops.Oh($"物品 {detailInput.StockLotNo} 批次正在盘库！");
                        }

                        await _wmsStockCheckNotifyDetailRep.InsertAsync(newDetail);

                        // 校验载具并添加箱码明细
                        await ValidateAndCreateStockCheckInfo(detailInput, entity.CheckBillCode, userId);
                    }
                }
            }

            _sqlSugarClient.Ado.CommitTran();

            await WmsBaseOperLogHelper.RecordAsync(
                Module: "盘点单据",
                OperationType: OperationTypeEnum.Update.GetEnumDescription(),
                BusinessNo: entity.CheckBillCode,
                OperationContent: $"更新盘点单据：{entity.CheckBillCode} 成功",
                OperParam: input
            );
        }
        catch (Exception ex)
        {
            _sqlSugarClient.Ado.RollbackTran();
            throw Oops.Oh($"更新盘点单据失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 校验载具并创建箱码明细（整合校验、锁定、添加箱码明细）
    /// </summary>
    /// <param name="detailInput">明细输入</param>
    /// <param name="checkBillCode">盘点单号</param>
    /// <param name="userId">用户ID</param>
    private async Task ValidateAndCreateStockCheckInfo(AddWmsStockCheckNotifyDetailInput detailInput, string checkBillCode, long userId)
    {
        var datetime = DateTime.Now;

        // 1. 校验并锁定载具
        var stockTray = await _wmsStockTrayRep.AsQueryable()
            .With(SqlWith.RowLock)
            .FirstAsync(a => a.Id == detailInput.CheckStockId.Value);

        if (stockTray == null)
        {
            throw Oops.Oh($"载具 {detailInput.StockCode} 不存在或已被删除！");
        }

        if (stockTray.StockQuantity <= 0)
        {
            throw Oops.Oh($"载具 {detailInput.StockCode} 库存数量为0，已被出库！");
        }

        if (stockTray.StockStatusFlag == 1)
        {
            throw Oops.Oh($"载具 {detailInput.StockCode} 已被占用，无法添加盘点单！");
        }

        // 2. 获取库存箱码信息
        var stockInfo = await _wmsStockInfoRep.AsQueryable()
            .Where(a => a.TrayId == stockTray.Id.ToString())
            .ToListAsync();

        if (stockInfo.Count == 0)
        {
            throw Oops.Oh($"载具 {detailInput.StockCode} 箱码信息不存在，已被出库！");
        }

        // 3. 锁定载具
        stockTray.StockStatusFlag = 1;
        await _wmsStockTrayRep.AsUpdateable(stockTray).ExecuteCommandAsync();

        // 4. 获取物料信息
        var material = await _wmsBaseMaterialRep.GetFirstAsync(a => a.Id.ToString() == stockTray.MaterialId);
        if (material == null)
        {
            throw Oops.Oh($"物料信息获取失败！");
        }

        // 5. 获取盘点明细ID
        var currentDetail = await _wmsStockCheckNotifyDetailRep.AsQueryable()
            .Where(a => a.CheckBillCode == checkBillCode
                && a.StockCode == detailInput.StockCode
                && a.MaterialId == detailInput.MaterialId
                && a.StockLotNo == detailInput.StockLotNo
                && a.StockSlot == detailInput.StockSlot)
            .FirstAsync();

        if (currentDetail == null)
        {
            throw Oops.Oh($"获取盘点明细失败！");
        }

        // 6. 创建并插入箱码明细
        var stockCheckInfos = stockInfo.Select(itemInfo => new WmsStockCheckInfo
        {
            BoxCode = itemInfo.BoxCode,
            StockCode = detailInput.StockCode,
            MaterialId = long.TryParse(itemInfo.MaterialId, out var matId) ? matId : 0,
            LotNo = itemInfo.LotNo,
            Qty = itemInfo.Qty,
            ProductionDate = itemInfo.ProductionDate,
            ValidateDay = itemInfo.ValidateDay,
            InspectionStatus = itemInfo.InspectionStatus,
            ExtractStatus = itemInfo.ExtractStatus,
            OutQty = itemInfo.outQty,
            OddMarking = itemInfo.OddMarking,
            CustomerId = itemInfo.CustomerId,
            SamplingDate = itemInfo.SamplingDate?.ToString("yyyy-MM-dd HH:mm:ss"),
            StaffCode = itemInfo.StaffCode,
            StaffName = itemInfo.StaffName,
            Weight = itemInfo.Weight,
            MaterialCode = material.MaterialCode,
            PickingSlurry = !string.IsNullOrWhiteSpace(itemInfo.PickingSlurry) ? Convert.ToInt32(itemInfo.PickingSlurry) : 0,
            StockCheckId = currentDetail.Id,
            MaterialName = material.MaterialName,
            Status = 0,
            State = 0,
            CreateUserId = userId,
            CreateTime = datetime,
            UpdateUserId = userId,
            UpdateTime = datetime,
            IsDelete = false
        }).ToList();

        await _wmsStockCheckInfoRep.InsertRangeAsync(stockCheckInfos);
    }

    #region 盘库出库
    /// <summary>
    /// 盘库出库 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("盘库出库")]
    [ApiDescriptionSettings(Name = "StockCheckIssueOutBound"), HttpPost]
    public async Task<string> StockCheckIssueOutBound(StockCheckIssueOutBoundInput input)
    {
        try
        {
            var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
            var userName = App.User?.FindFirst(ClaimConst.RealName)?.Value;

            if (string.IsNullOrWhiteSpace(input.BillCode) || string.IsNullOrWhiteSpace(input.WarehouseId))
            {
                throw Oops.Oh("获取数据失败");
            }

            var checkNotify = await _wmsStockCheckNotifyRep.GetFirstAsync(u => u.CheckBillCode == input.BillCode);
            if (checkNotify == null)
            {
                throw Oops.Oh("盘点单据不存在");
            }

            var warehouse = await _wmsBaseWareHouseRep.GetFirstAsync(u => u.Id.ToString() == input.WarehouseId);
            if (warehouse == null)
            {
                throw Oops.Oh("仓库信息不存在");
            }

            var list = new List<ExportLibraryDTO>();
            var warehouseType = warehouse.WarehouseType;

            if (warehouseType == "01")
            {
                var list1 = await IssueOutBoundNew3(input.BillCode);
                list.AddRange(list1);
            }
            else if (warehouseType == "05")
            {
                var list1 = await IssueOutBoundNew4(input.BillCode);
                list.AddRange(list1);
            }

            if (list.Count > 0)
            {
                var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented);
                string logAddress = @"D:\log\WCS\下发盘库出库信息" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                var sb = new System.Text.StringBuilder($"给WCS发送盘库数据：{jsonData}");
                await Admin.NET.Core.FileHelper.WriteToFileAsync(logAddress, sb);

                try
                {
                    var wcsHost = Admin.NET.Application.Service.WmsPort.Dto.WcsApiUrlDto.GetHost();
                    using (var client = new HttpClient())
                    {
                        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(wcsHost + Admin.NET.Application.Service.WmsPort.Dto.WcsApiUrlDto.TaskApiUrl, content);
                        var responseBody = await response.Content.ReadAsStringAsync();

                        var sbResponse = new System.Text.StringBuilder($"反馈数据：{responseBody}");
                        await Admin.NET.Core.FileHelper.WriteToFileAsync(logAddress, sbResponse);

                        var wcsModel = Newtonsoft.Json.JsonConvert.DeserializeObject<WcsReturnModel>(responseBody);
                        if (wcsModel.stateCode == "0")
                        {
                            foreach (var item in list)
                            {
                                await EditTaskStatus(item.TaskNo, 1);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var sbError = new System.Text.StringBuilder($"反馈异常数据：{ex.Message}");
                    await Admin.NET.Core.FileHelper.WriteToFileAsync(logAddress, sbError);
                    throw Oops.Oh($"下发出库失败：{ex.Message}");
                }

                await WmsBaseOperLogHelper.RecordAsync(
                    Module: "盘点单据",
                    OperationType: OperationTypeEnum.Update.GetEnumDescription(),
                    BusinessNo: input.BillCode,
                    OperationContent: $"盘库出库：{input.BillCode} 成功",
                    OperParam: input
                );

                return "下发出库成功";
            }
            else
            {
                return "无需下发出库";
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh($"盘库出库失败：{ex.Message}");
        }
    }

    private async Task<List<ExportLibraryDTO>> IssueOutBoundNew3(string billCode)
    {
        #region 初始化集合变量
        var outBeforeDto = new List<ExportLibraryDTO>(); // 先出库数据的集合（深度为1的储位）
        var outAfterDto = new List<ExportLibraryDTO>(); // 后出库数据的集合（深度为2的储位）
        var moveDto = new List<ExportLibraryDTO>(); // 要移库数据的集合
        var isErr = string.Empty;
        #endregion

        try
        {
            #region 开启数据库事务
            _sqlSugarClient.Ado.BeginTran();
            #endregion

            #region 获取盘点单据信息
            var listNotify = await _wmsStockCheckNotifyRep.GetFirstAsync(a => a.IsDelete == false && a.CheckBillCode == billCode && a.ExecuteFlag == 0);
            if (listNotify == null || string.IsNullOrWhiteSpace(listNotify.CheckBillCode))
            {
                throw Oops.Oh("获取当前" + billCode + " 单据信息获取失败");
            }
            listNotify.ExecuteFlag = 1; // 改为正在执行
            #endregion

            #region 获取盘点单明细信息
            var list = await _wmsStockCheckNotifyDetailRep.AsQueryable()
                .Where(a => a.CheckBillCode == listNotify.CheckBillCode && a.IsDelete == false)
                .ToListAsync();
            if (list.Count == 0)
            {
                throw Oops.Oh("获取当前" + billCode + " 单据明细信息获取失败");
            }
            #endregion

            #region 按托盘码、储位、仓库分组
            var listData = list.GroupBy(m => new { m.StockCode, m.StockSlot, m.WarehouseId });
            var taskList = new List<WmsStockCheckTask>();

            var warehouseId = long.TryParse(listNotify.WarehouseId, out var whId) ? whId : 0;
            #endregion

            #region 关联储位信息并按储位出入库深度排序
            var listDataWithSlot = from a in listData
                                   join g in _wmsBaseSlotRep.AsQueryable().ToList() on a.Key.StockSlot equals g.SlotCode
                                   orderby g.SlotInout
                                   select new { Group = a, Slot = g };
            #endregion

            #region 遍历每个分组
            foreach (var demo in listDataWithSlot)
            {
                #region 验证托盘是否在库内
                var tuoPan = await _wmsStockTrayRep.GetFirstAsync(m => m.StockCode == demo.Group.Key.StockCode);
                if (tuoPan == null || string.IsNullOrEmpty(tuoPan.StockSlotCode) || await _wmsBaseSlotRep.GetFirstAsync(m => m.SlotCode == tuoPan.StockSlotCode) == null)
                {
                    continue; // 托盘不在库内，直接跳出
                }
                #endregion

                #region 遍历分组中的每个明细项
                foreach (var item in demo.Group)
                {
                    #region 添加流水单
                    string checkTaskCode = string.Empty;
                    if (taskList.Count < 1)
                    {
                        checkTaskCode = await GetImExNo("PKR");
                    }
                    else
                    {
                        var lastCheckTask = taskList.LastOrDefault();
                        var num = Convert.ToInt64(lastCheckTask.CheckTaskNo.Substring(3));
                        num++;
                        checkTaskCode = $"PKR{num}";
                    }
                    #endregion

                    var outSlot = await _wmsBaseSlotRep.GetFirstAsync(m => m.SlotCode == item.StockSlot);
                    if (outSlot.SlotStatus == 1) // 验证当前的储位是否正是有货状态
                    {
                        #region 获取仓库信息
                        var HouseCode = await _wmsBaseWareHouseRep.GetFirstAsync(p => p.Id == warehouseId);
                        if (HouseCode == null)
                        {
                            throw Oops.Oh("获取当前仓库信息失败");
                        }
                        #endregion

                        if (outSlot.SlotInout == 2)
                        {
                            #region 获取要出库深度为2储位前面的储位信息
                            var slotBefore = await _wmsBaseSlotRep.GetFirstAsync(m => m.SlotInout == 1 && m.SlotRow == outSlot.SlotRow && m.SlotColumn == outSlot.SlotColumn && m.SlotLayer == outSlot.SlotLayer && m.WarehouseId == warehouseId && m.SlotLanewayId == outSlot.SlotLanewayId);
                            #endregion

                            if (slotBefore.SlotStatus == 1) // 前面的储位有物料、进行移库操作
                            {
                                #region 去库存表中找到储位对应的托盘码操作
                                var stocknew = await _wmsStockTrayRep.GetFirstAsync(m => m.StockSlotCode == slotBefore.SlotCode);
                                if (stocknew == null)
                                {
                                    var slotChange = await _wmsBaseSlotRep.GetFirstAsync(m => m.SlotCode == slotBefore.SlotCode);
                                    slotChange.SlotStatus = 0;
                                    await _wmsBaseSlotRep.AsUpdateable(slotChange).ExecuteCommandAsync();
                                }
                                else
                                {
                                    var newSlot = await MoveAddre(slotBefore.SlotCode); // 寻找移库库位
                                    if (!string.IsNullOrEmpty(newSlot))
                                    {
                                        #region 添加出库时发生的移库任务
                                        var VehicleSub = await _wmsStockTrayRep.AsQueryable()
                                            .Where(a => a.StockSlotCode == slotBefore.SlotCode)
                                            .ToListAsync();

                                        foreach (var itemSub in VehicleSub)
                                        {
                                            #region 判断主次托盘
                                            string zftp = "1";
                                            if (itemSub.StockCode == itemSub.VehicleSubId)
                                            {
                                                zftp = "0";
                                            }
                                            #endregion

                                            var exTaskList = taskList.FirstOrDefault(a => a.CheckTaskNo == checkTaskCode);
                                            if (exTaskList != null && taskList.Count() > 0)
                                            {
                                                var lastCheckTask = taskList.LastOrDefault();
                                                var num = Convert.ToInt64(lastCheckTask.CheckTaskNo.Substring(3));
                                                num++;
                                                checkTaskCode = $"PKR{num}";
                                            }

                                            var exYkTask1 = new WmsStockCheckTask
                                            {
                                                Id = SnowFlakeSingle.Instance.NextId(),
                                                CheckTaskNo = checkTaskCode,
                                                CheckTaskFlag = 0,
                                                Sender = "WMS",
                                                Receiver = "WCS",
                                                IsSuccess = 0,
                                                SendDate = DateTime.Now,
                                                BackDate = null,
                                                StockCode = stocknew.StockCode,
                                                Msg = $"由{slotBefore.SlotCode}储位移动到{newSlot}储位",
                                                IsDelete = false,
                                                WarehouseId = warehouseId.ToString(),
                                                StartLocation = slotBefore.SlotCode,
                                                EndLocation = newSlot,
                                                CreateTime = DateTime.Now,
                                                TaskType = 2,
                                                CheckNotifyId = item.Id.ToString(),
                                                CheckBillCode = item.CheckBillCode
                                            };

                                            taskList.Add(exYkTask1);

                                            moveDto.Add(new ExportLibraryDTO()
                                            {
                                                BillCode = item.CheckBillCode,
                                                TaskBegin = slotBefore.SlotCode,
                                                StockCode = itemSub.StockCode,
                                                TaskNo = checkTaskCode,
                                                TaskType = "3",
                                                HouseCode = HouseCode.WarehouseCode,
                                                TaskEnd = newSlot,
                                                IsExportStockTary = zftp
                                            });
                                        }
                                        #endregion
                                        #region 更新储位状态
                                        var SlotChange = await _wmsBaseSlotRep.GetFirstAsync(m => m.Id == slotBefore.Id);
                                        var slotChange2 = await _wmsBaseSlotRep.GetFirstAsync(m => m.SlotCode == newSlot);

                                        SlotChange.SlotStatus = 5; // 改变状态（正在移库）
                                        slotChange2.SlotStatus = 4; // 改变状态（正在移入）

                                        await _wmsBaseSlotRep.AsUpdateable(SlotChange).ExecuteCommandAsync();
                                        await _wmsBaseSlotRep.AsUpdateable(slotChange2).ExecuteCommandAsync();
                                        #endregion
                                    }
                                }
                                #endregion
                            }
                        }

                        #region 查询托盘信息（主副托盘）
                        int IsExportStockTary = 0;
                        var stockTrays = await _wmsStockTrayRep.AsQueryable()
                            .Where(m => m.VehicleSubId == item.StockCode)
                            .ToListAsync();

                        if (stockTrays == null || stockTrays.Count < 1)
                        {
                            stockTrays = await _wmsStockTrayRep.AsQueryable()
                                .Where(m => m.StockCode == item.StockCode)
                                .ToListAsync();

                            if (stockTrays != null && stockTrays.Count > 0)
                            {
                                var stockTray1 = await _wmsStockTrayRep.AsQueryable()
                                    .Where(m => m.VehicleSubId == stockTrays[0].VehicleSubId)
                                    .ToListAsync();

                                if (stockTray1 != null && stockTray1.Count > 0)
                                {
                                    stockTrays = stockTray1;
                                }
                            }
                        }
                        #endregion

                        if (stockTrays.Count() > 0)
                        {
                            foreach (var itemTary in stockTrays)
                            {
                                IsExportStockTary = 0;
                                if (itemTary.StockCode != itemTary.VehicleSubId)
                                    IsExportStockTary = 1;

                                var exTaskList = taskList.FirstOrDefault(a => a.CheckTaskNo == checkTaskCode);
                                if (exTaskList != null && taskList.Count() > 0)
                                {
                                    var lastcheckTask = taskList.LastOrDefault();
                                    var num = Convert.ToInt64(lastcheckTask.CheckTaskNo.Substring(3));
                                    num++;
                                    checkTaskCode = $"PKR{num}";
                                }

                                #region 添加出库任务
                                var exTask1 = new WmsStockCheckTask
                                {
                                    Id = SnowFlakeSingle.Instance.NextId(),
                                    CheckTaskNo = checkTaskCode,
                                    Sender = "WMS",
                                    Receiver = "WCS",
                                    CheckTaskFlag = 0,
                                    CheckNotifyId = item.Id.ToString(),
                                    CheckBillCode = item.CheckBillCode,
                                    IsSuccess = 0,
                                    SendDate = DateTime.Now,
                                    BackDate = null,
                                    StockCode = itemTary.StockCode,
                                    Msg = itemTary.StockCode + $" 目标出库口为A03出库口",
                                    IsDelete = false,
                                    WarehouseId = warehouseId.ToString(),
                                    StartLocation = outSlot.SlotCode,
                                    EndLocation = "A03",
                                    TaskType = 1,
                                    CreateTime = DateTime.Now
                                };

                                taskList.Add(exTask1);

                                #region 获取库存箱码信息
                                List<StockBoxInfo> boxInfos1 = new List<StockBoxInfo>();
                                var StockInfos = await _wmsStockInfoRep.AsQueryable()
                                    .Where(a => a.TrayId == itemTary.Id.ToString())
                                    .ToListAsync();

                                foreach (var stockInfo in StockInfos)
                                {
                                    var material = await _wmsBaseMaterialRep.GetFirstAsync(a => a.Id.ToString() == stockInfo.MaterialId);
                                    var stockBoxInfo = new StockBoxInfo
                                    {
                                        BoxCode = stockInfo.BoxCode,
                                        LotNo = stockInfo.LotNo,
                                        Qty = (decimal)stockInfo.Qty,
                                        MaterialCode = material?.MaterialCode ?? "",
                                        ProductionDate = stockInfo.ProductionDate?.ToString(),
                                        ValidateDay = stockInfo.ValidateDay?.ToString(),
                                        BulkTank = 0,
                                        SamplingDate = stockInfo.SamplingDate?.ToString(),
                                        StaffCode = stockInfo.StaffCode,
                                        StaffName = stockInfo.StaffName,
                                        PickingSlurry = string.IsNullOrWhiteSpace(stockInfo.PickingSlurry) ? "0" : stockInfo.PickingSlurry.ToString(),
                                        ExtractStatus = string.IsNullOrWhiteSpace(stockInfo.ExtractStatus?.ToString()) ? null : stockInfo.ExtractStatus.ToString()
                                    };
                                    boxInfos1.Add(stockBoxInfo);
                                }
                                #endregion

                                outBeforeDto.Add(new ExportLibraryDTO()
                                {
                                    TaskBegin = outSlot.SlotCode,
                                    StockCode = itemTary.StockCode,
                                    TaskNo = exTask1.CheckTaskNo,
                                    TaskType = "1",
                                    HouseCode = HouseCode.WarehouseCode,
                                    Qty = Convert.ToDecimal(itemTary.LockQuantity ?? 0),
                                    WholeBoxNum = (itemTary.LockQuantity ?? 0).ToString(),
                                    BillCode = item.CheckBillCode,
                                    TaskEnd = "A03",
                                    boxInfos = boxInfos1,
                                    IsExportStockTary = IsExportStockTary.ToString()
                                });

                                item.CheckTaskNo = exTask1.CheckTaskNo;
                                #endregion
                            }
                        }

                        #region 要出库的储位改变状态
                        var s = await _wmsBaseSlotRep.GetFirstAsync(m => m.SlotCode == outSlot.SlotCode);
                        s.SlotStatus = 3; // 正在出库
                        await _wmsBaseSlotRep.AsUpdateable(s).ExecuteCommandAsync();
                        item.ExecuteFlag = 1;
                        #endregion
                    }
                    else if (outSlot.SlotStatus == 3) // 正在出库的
                    {
                        item.ExecuteFlag = 1;
                    }
                }
                #endregion
            }
            #endregion

            #region 插入所有任务到数据库
            foreach (var itemTask in taskList)
            {
                await _wmsStockCheckTaskRep.InsertAsync(itemTask);
            }
            #endregion

            #region 更新盘点单据和明细
            await _wmsStockCheckNotifyRep.AsUpdateable(listNotify).ExecuteCommandAsync();
            await _wmsStockCheckNotifyDetailRep.AsUpdateable(list).ExecuteCommandAsync();
            #endregion

            #region 提交事务并返回结果
            _sqlSugarClient.Ado.CommitTran();

            #region 先移库后出库
            outBeforeDto.AddRange(moveDto);
            outBeforeDto.AddRange(outAfterDto);
            return outBeforeDto;
            #endregion
            #endregion
        }
        catch (Exception e)
        {
            _sqlSugarClient.Ado.RollbackTran();
            isErr = e.Message;
            throw Oops.Oh(e.Message);
        }
    }

    private async Task<string> MoveAddre(string slotCode)
    {
        var slot = await _wmsBaseSlotRep.GetFirstAsync(m => m.SlotCode == slotCode);
        if (slot == null)
        {
            return string.Empty;
        }

        var warehouseId = slot.WarehouseId;
        var chuweidizhi = ""; // 移库目标储位
        int maxLayer = 10; // 修改为10层

        #region 遍历每个层数分组，确定移库目标层数（支持10层）
        var currentLayer = (int)slot.SlotLayer;
        var searchedLayers = new List<int>();
        var layerDirections = new List<int> { 0, 1, -1, 2, -2, 3, -3, 4, -4, 5, -5, 6, -6, 7, -7, 8, -8, 9, -9 };

        foreach (var direction in layerDirections)
        {
            var layer = currentLayer + direction;

            if (layer < 1 || layer > maxLayer)
            {
                continue;
            }

            if (searchedLayers.Contains(layer))
            {
                continue;
            }

            searchedLayers.Add(layer);

            #region 在相邻巷道中查找空储位
            var outSlotSum = await _wmsBaseSlotRep.AsQueryable()
                .Where(a => a.WarehouseId == warehouseId
                    && a.SlotImlockFlag != 1
                    && a.SlotExlockFlag != 1
                    && a.SlotCloseFlag != 1)
                .ToListAsync();

            long max = outSlotSum.Where(a => a.SlotAreaId == slot.SlotAreaId).Max(a => a.SlotLanewayId.Value);
            long min = outSlotSum.Where(a => a.SlotAreaId == slot.SlotAreaId).Min(a => a.SlotLanewayId.Value);
            var num = 1;

            for (int i = 0; i < max; i++)
            {
                var newSlotLanewayId = slot.SlotLanewayId + num;

                #region 向右查找相邻巷道
                if (max >= newSlotLanewayId)
                {
                    var slotList = await _wmsBaseSlotRep.AsQueryable()
                        .Where(a => a.WarehouseId == warehouseId
                            && a.SlotLanewayId == newSlotLanewayId
                            && a.SlotAreaId == slot.SlotAreaId
                            && a.SlotStatus == 0
                            && a.SlotStatus != 7
                            && a.SlotStatus != 8
                            && a.SlotStatus != 9
                            && a.SlotImlockFlag != 1
                            && a.SlotExlockFlag != 1
                            && a.SlotCloseFlag != 1
                            && a.SlotLayer == layer)
                        .OrderByDescending(a => a.SlotInout)
                        .ToListAsync();

                    if (slotList.Count > 0)
                    {
                        chuweidizhi = slotList[0].SlotCode;
                        break;
                    }
                }
                #endregion

                #region 向左查找相邻巷道
                newSlotLanewayId = slot.SlotLanewayId - num;
                if (min <= newSlotLanewayId)
                {
                    var slotList = await _wmsBaseSlotRep.AsQueryable()
                        .Where(a => a.WarehouseId == warehouseId
                            && a.SlotLanewayId == newSlotLanewayId
                            && a.Make == "01"
                            && a.SlotAreaId == slot.SlotAreaId
                            && a.SlotStatus == 0
                            && a.SlotStatus != 7
                            && a.SlotStatus != 8
                            && a.SlotStatus != 9
                            && a.SlotImlockFlag != 1
                            && a.SlotExlockFlag != 1
                            && a.SlotCloseFlag != 1
                            && a.SlotLayer == layer)
                        .OrderByDescending(a => a.SlotInout)
                        .ToListAsync();

                    if (slotList.Count > 0)
                    {
                        chuweidizhi = slotList[0].SlotCode;
                        break;
                    }
                }
                #endregion

                num++;
            }
            #endregion

            if (!string.IsNullOrEmpty(chuweidizhi))
            {
                break;
            }
        }
        #endregion

        return chuweidizhi;
    }

    /// <summary>
    /// 自动化平库盘库出库（JC35 IssueOutBoundNew4方法迁移）
    /// </summary>
    /// <param name="billCode">盘点单号</param>
    /// <returns>出库任务列表</returns>
    private async Task<List<ExportLibraryDTO>> IssueOutBoundNew4(string billCode)
    {
        #region 初始化集合变量
        var slotDataNo = new List<string>(); // 因状态不能出库的储位集合
        var outBeforeDto = new List<ExportLibraryDTO>(); // 先出库数据的集合
        var moveDto = new List<ExportLibraryDTO>(); // 要移库数据的集合
        #endregion

        try
        {
            #region 开启数据库事务
            _sqlSugarClient.Ado.BeginTran();
            #endregion

            #region 获取盘点单据信息
            var listNotify = await _wmsStockCheckNotifyRep.GetFirstAsync(a => a.IsDelete == false && a.CheckBillCode == billCode && a.ExecuteFlag == 0);
            if (listNotify == null || string.IsNullOrWhiteSpace(listNotify.CheckBillCode))
            {
                throw Oops.Oh("获取当前" + billCode + " 单据信息获取失败");
            }
            listNotify.ExecuteFlag = 1; // 修改执行状态为正在执行
            #endregion

            #region 获取盘点单明细信息
            var list = await _wmsStockCheckNotifyDetailRep.AsQueryable()
                .Where(a => a.CheckBillCode == listNotify.CheckBillCode && a.IsDelete == false)
                .ToListAsync();
            if (list.Count == 0)
            {
                throw Oops.Oh("获取当前" + billCode + " 单据明细信息获取失败");
            }
            #endregion

            #region 按托盘码、储位、仓库分组
            var listData = list.GroupBy(m => new { m.StockCode, m.StockSlot, m.WarehouseId });
            
            var taskList = new List<WmsStockCheckTask>(); // 任务列表

            var warehouseId = long.TryParse(listNotify.WarehouseId, out var whId) ? whId : 0;
            #endregion

            #region 关联储位信息并按巷道分组
            var listData1 = from a in listData
                            join g in _wmsBaseSlotRep.AsQueryable().Where(a => a.WarehouseId == warehouseId).ToList() on a.Key.StockSlot equals g.SlotCode
                            select new { a.Key.StockCode, a.Key.StockSlot, g.SlotLanewayId, g.SlotInout, a.Key.WarehouseId };

            var listdata2 = listData1.GroupBy(a => new { a.SlotLanewayId, a.WarehouseId });
            #endregion

            #region 遍历每个巷道分组
            foreach (var item1 in listdata2)
            {
                #region 按巷道和仓库过滤并按出入库深度排序
                var newListData = from a in listData
                                  join g in _wmsBaseSlotRep.AsQueryable().Where(a => a.WarehouseId == warehouseId).ToList() on a.Key.StockSlot equals g.SlotCode
                                  orderby g.SlotInout
                                  where g.SlotLanewayId == item1.Key.SlotLanewayId && SqlFunc.ToString(g.WarehouseId) == item1.Key.WarehouseId
                                  select a;
                #endregion

                #region 遍历每个分组
                foreach (var demo in newListData)
                {
                    #region 验证托盘是否在库内
                    var tuoPan = await _wmsStockTrayRep.GetFirstAsync(m => m.StockCode == demo.Key.StockCode);
                    if (tuoPan == null || string.IsNullOrEmpty(tuoPan.StockSlotCode) || await _wmsBaseSlotRep.GetFirstAsync(m => m.SlotCode == tuoPan.StockSlotCode) == null)
                    {
                        continue; // 托盘不在库内，跳过
                    }
                    #endregion

                    #region 遍历分组中的每个明细项
                    foreach (var item in demo)
                    {
                        #region 判断流水中的储位和库存中的储位是否一致（检查是否被移库）
                        var outSlotSum = await _wmsBaseSlotRep.AsQueryable()
                            .Where(a => (a.SlotStatus != 8 && a.SlotStatus != 9) && a.Make != "02" && a.WarehouseId == warehouseId)
                            .ToListAsync();
                        var outSlot = outSlotSum.FirstOrDefault(m => m.SlotCode == item.StockSlot);
                        if (outSlot == null)
                        {
                            throw Oops.Oh(item.StockSlot + " 储位获取失败");
                        }

                        var HouseCode = await _wmsBaseWareHouseRep.GetFirstAsync(p => p.Id == warehouseId);
                        var stockTray = await _wmsStockTrayRep.GetFirstAsync(m => m.StockCode == item.StockCode);

                        if (stockTray.StockSlotCode != item.StockSlot)
                        {
                            outSlot = outSlotSum.FirstOrDefault(m => m.SlotCode == stockTray.StockSlotCode);
                        }

                        string CheckTaskNo;
                        #endregion

                        #region 储位状态为有货状态，创建出库任务
                        if (outSlot.SlotStatus == 1)
                        {
                            #region 生成盘点任务号
                            if (taskList.Count < 1)
                            {
                                CheckTaskNo = await GetImExNo("PKR");
                            }
                            else
                            {
                                var lastOrder = taskList.LastOrDefault();
                                var num = Convert.ToInt64(lastOrder.CheckTaskNo.Substring(3));
                                num++;
                                CheckTaskNo = $"PKR{num}";
                            }
                            #endregion

                            #region 创建出库任务
                            var exckTask1 = new WmsStockCheckTask
                            {
                                Id = SnowFlakeSingle.Instance.NextId(),
                                CheckTaskNo = CheckTaskNo,
                                Sender = "WMS",
                                Receiver = "WCS",
                                IsSuccess = 0,
                                Information = "",
                                SendDate = DateTime.Now,
                                BackDate = null,
                                StockCode = tuoPan.StockCode,
                                Msg = "由" + outSlot.SlotCode + "储位出库",
                                IsDelete = false,
                                CreateTime = DateTime.Now,
                                CreateUserName = "",
                                //UpdateTime = null,
                                UpdateUserName = "",
                                CheckNotifyId = item.Id.ToString(),
                                CheckBillCode = item.CheckBillCode,
                                WarehouseId = warehouseId.ToString(),
                                StartLocation = outSlot.SlotCode,
                                EndLocation = "A03",
                                TaskType = 1
                            };
                            taskList.Add(exckTask1);
                            #endregion

                            #region 查询需要移库的储位（挡路储位）
                            var dangLuSlot = outSlotSum.Where(a => a.SlotAreaId == outSlot.SlotAreaId && a.SlotStatus == 1 && a.SlotInout < outSlot.SlotInout && a.SlotLanewayId == outSlot.SlotLanewayId && a.SlotAreaId == outSlot.SlotAreaId).OrderBy(a => a.SlotInout).ToList();
                            if (dangLuSlot.Count > 0)
                            {
                                #region 遍历每个挡路储位，查找移库目标储位
                                foreach (var item2 in dangLuSlot)
                                {
                                    var num = 1;
                                    var chuweidizhi = ""; // 移库目标储位
                                    long max = outSlotSum.Where(a => a.SlotAreaId == outSlot.SlotAreaId).Max(a => a.SlotLanewayId.Value); // 最大巷道号
                                    long min = outSlotSum.Where(a => a.SlotAreaId == outSlot.SlotAreaId).Min(a => a.SlotLanewayId.Value); // 最小巷道号
                                    int SlotLayer = outSlotSum.Max(a => (int)a.SlotLayer); // 最大层数
                                    int layer = 1; // 当前查找的层数

                                    #region 遍历每个层数分组，确定移库目标层数（支持任意层数）
                                    var currentLayer = (int)outSlot.SlotLayer; // 当前出库层
                                    var searchedLayers = new List<int>(); // 已查找过的层数
                                    var layerDirections = new List<int> { 0, 1, -1, 2, -2, 3, -3, 4, -4, 5, -5, 6, -6, 7, -7, 8, -8, 9, -9 }; // 查找方向：0=当前层，1=上一层，-1=下一层，以此类推

                                    foreach (var direction in layerDirections)
                                    {
                                        layer = currentLayer + direction;

                                        if (layer < 1 || layer > SlotLayer)
                                        {
                                            continue; // 超出层数范围，跳过
                                        }

                                        if (searchedLayers.Contains(layer))
                                        {
                                            continue; // 已查找过该层，跳过
                                        }

                                        searchedLayers.Add(layer); // 记录已查找的层数

                                        #region 在相邻巷道中查找空储位
                                        for (int i = 0; i < max; i++)
                                        {
                                            var newSlotLanewayId = outSlot.SlotLanewayId + num;

                                            #region 向右查找相邻巷道
                                            if (max >= newSlotLanewayId)
                                            {
                                                var slotList = await _wmsBaseSlotRep.AsQueryable()
                                                    .LeftJoin<WmsBaseLaneway>((a, b) => a.SlotLanewayId == b.Id)
                                                    .Where((a, b) => a.WarehouseId == warehouseId
                                                        && a.SlotLanewayId == newSlotLanewayId
                                                        && a.SlotAreaId == item2.SlotAreaId
                                                        && a.SlotStatus == 0
                                                        && a.SlotStatus != 7
                                                        && a.SlotStatus != 8
                                                        && a.SlotStatus != 9
                                                        && a.SlotImlockFlag != 1
                                                        && a.SlotExlockFlag != 1
                                                        && a.SlotCloseFlag != 1
                                                        && a.SlotLayer == layer)
                                                    .OrderByDescending((a, b) => a.SlotInout)
                                                    .Select((a, b) => new YiKuModel
                                                    {
                                                        SlotCode = a.SlotCode,
                                                        WarehouseId = a.WarehouseId,
                                                        SlotLanewayId = a.SlotLanewayId,
                                                        SlotStatus = a.SlotStatus,
                                                        SlotInout = a.SlotInout,
                                                        SlotAreaId = a.SlotAreaId
                                                    })
                                                    .ToListAsync();

                                                if (slotList.Count > 0)
                                                {
                                                    chuweidizhi = slotList[0].SlotCode;
                                                    break;
                                                }
                                            }
                                            #endregion

                                            #region 向左查找相邻巷道
                                            newSlotLanewayId = outSlot.SlotLanewayId - num;
                                            if (min <= newSlotLanewayId)
                                            {
                                                var slotList = await _wmsBaseSlotRep.AsQueryable()
                                                    .LeftJoin<WmsBaseLaneway>((a, b) => a.SlotLanewayId == b.Id)
                                                    .Where((a, b) => a.WarehouseId == warehouseId
                                                        && a.SlotLanewayId == newSlotLanewayId
                                                        && a.Make == "01"
                                                        && a.SlotAreaId == item2.SlotAreaId
                                                        && a.SlotStatus == 0
                                                        && a.SlotStatus != 7
                                                        && a.SlotStatus != 8
                                                        && a.SlotStatus != 9
                                                        && a.SlotImlockFlag != 1
                                                        && a.SlotExlockFlag != 1
                                                        && a.SlotCloseFlag != 1
                                                        && a.SlotLayer == layer)
                                                    .OrderByDescending((a, b) => a.SlotInout)
                                                    .Select((a, b) => new YiKuModel
                                                    {
                                                        SlotCode = a.SlotCode,
                                                        WarehouseId = a.WarehouseId,
                                                        SlotLanewayId = a.SlotLanewayId,
                                                        SlotStatus = a.SlotStatus,
                                                        SlotInout = a.SlotInout,
                                                        SlotAreaId = a.SlotAreaId
                                                    })
                                                    .ToListAsync();

                                                if (slotList.Count > 0)
                                                {
                                                    chuweidizhi = slotList[0].SlotCode;
                                                    break;
                                                }
                                            }
                                            #endregion

                                            num++;
                                        }
                                        #endregion

                                        if (!string.IsNullOrEmpty(chuweidizhi))
                                        {
                                            break;
                                        }
                                    }
                                    #endregion

                                    #region 找到移库目标储位，创建移库任务
                                    if (!string.IsNullOrEmpty(chuweidizhi))
                                    {
                                        var tuopanhao = await _wmsStockTrayRep.GetFirstAsync(a => a.StockSlotCode == item2.SlotCode);

                                        #region 生成移库任务号
                                        var lastOrder = taskList.LastOrDefault();
                                        var num1 = Convert.ToInt64(lastOrder.CheckTaskNo.Substring(3));
                                        num1++;
                                        CheckTaskNo = $"PKR{num1}";
                                        #endregion

                                        #region 创建移库任务
                                        var exYkTask1 = new WmsStockCheckTask
                                        {
                                            Id = SnowFlakeSingle.Instance.NextId(),
                                            CheckTaskNo = CheckTaskNo,
                                            Sender = "WMS",
                                            Receiver = "WCS",
                                            IsSuccess = 0,
                                            Information = "",
                                            SendDate = DateTime.Now,
                                            BackDate = null,
                                            StockCode = tuopanhao.StockCode,
                                            Msg = $"由{item2.SlotCode}储位移动到{chuweidizhi}储位",
                                            IsDelete = false,
                                            CreateTime = DateTime.Now,
                                            CreateUserId = 0,
                                            CreateUserName ="",
                                            //UpdateTime = null,
                                            UpdateUserId = 0,
                                            UpdateUserName="",
                                            WarehouseId = warehouseId.ToString(),
                                            StartLocation = item2.SlotCode,
                                            EndLocation = chuweidizhi,
                                            TaskType = 3,
                                            CheckNotifyId = item.Id.ToString(),
                                            CheckBillCode = item.CheckBillCode
                                        };
                                        taskList.Add(exYkTask1);
                                        #endregion

                                        #region 更新储位状态
                                        var SlotChange = await _wmsBaseSlotRep.GetFirstAsync(m => m.SlotCode == item2.SlotCode);
                                        var slotChange2 = await _wmsBaseSlotRep.GetFirstAsync(m => m.SlotCode == chuweidizhi);

                                        SlotChange.SlotStatus = 5; // 正在移库
                                        slotChange2.SlotStatus = 4; // 正在移入

                                        await _wmsBaseSlotRep.AsUpdateable(SlotChange).ExecuteCommandAsync();
                                        await _wmsBaseSlotRep.AsUpdateable(slotChange2).ExecuteCommandAsync();
                                        #endregion

                                        #region 设置托盘占用状态
                                        if (tuopanhao != null)
                                        {
                                            tuopanhao.StockStatusFlag = 1; // 设置为占用状态
                                            await _wmsStockTrayRep.AsUpdateable(tuopanhao).ExecuteCommandAsync();
                                        }
                                        #endregion

                                        #region 添加移库数据到返回列表
                                        moveDto.Add(new ExportLibraryDTO()
                                        {
                                            BillCode = item.CheckBillCode,
                                            TaskBegin = item2.SlotCode,
                                            StockCode = tuopanhao.StockCode,
                                            TaskNo = exYkTask1.CheckNotifyId,
                                            TaskType = "3",
                                            HouseCode = HouseCode.WarehouseCode,
                                            TaskEnd = chuweidizhi,
                                            LanewayId = item2.SlotLanewayId?.ToString()??"",
                                        });
                                        #endregion
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            #endregion

                            #region 添加出库任务
                            List<StockBoxInfo> boxInfos1 = new List<StockBoxInfo>();
                            var StockInfos = await _wmsStockInfoRep.AsQueryable()
                                .Where(a => a.TrayId == tuoPan.Id.ToString())
                                .ToListAsync();

                            #region 遍历库存信息，构建箱码信息
                            foreach (var stockInfo in StockInfos)
                            {
                                var material = await _wmsBaseMaterialRep.GetFirstAsync(a => a.Id.ToString() == stockInfo.MaterialId);
                                var stockBoxInfo = new StockBoxInfo();
                                stockBoxInfo.BoxCode = stockInfo.BoxCode;
                                stockBoxInfo.LotNo = stockInfo.LotNo;
                                stockBoxInfo.Qty = (decimal)stockInfo.Qty;
                                stockBoxInfo.MaterialCode = material?.MaterialCode ?? "";
                                stockBoxInfo.ProductionDate = stockInfo.ProductionDate?.ToString();
                                stockBoxInfo.ValidateDay = stockInfo.ValidateDay?.ToString();
                                stockBoxInfo.BulkTank = (int)(stockInfo.IsFractionBox ?? 0);
                                stockBoxInfo.RFIDCode = stockInfo.RFIDCode;
                                stockBoxInfo.SamplingDate = stockInfo.SamplingDate?.ToString();
                                stockBoxInfo.StaffCode = stockInfo.StaffCode;
                                stockBoxInfo.StaffName = stockInfo.StaffName;
                                stockBoxInfo.PickingSlurry = "0";
                                stockBoxInfo.ExtractStatus = stockInfo.ExtractStatus?.ToString();
                                boxInfos1.Add(stockBoxInfo);
                            }
                            #endregion

                            #region 添加出库数据到返回列表
                            outBeforeDto.Add(new ExportLibraryDTO()
                            {
                                TaskBegin = outSlot.SlotCode,
                                StockCode = item.StockCode,
                                TaskNo = exckTask1.CheckTaskNo,
                                TaskType = "1",
                                HouseCode = HouseCode.WarehouseCode,
                                Qty = Convert.ToDecimal(item.StockQuantity),
                                WholeBoxNum = item.StockQuantity.ToString(),
                                BillCode = item.CheckBillCode,
                                TaskEnd = "A03",
                                boxInfos = boxInfos1,
                                LanewayId = outSlot.SlotLanewayId?.ToString()??"",
                            });
                            #endregion
                            #endregion

                            #region 更新明细项的任务号
                            foreach (var items in demo)
                            {
                                items.CheckTaskNo = exckTask1.CheckTaskNo;
                                exckTask1.CheckNotifyId = items.Id.ToString();
                                exckTask1.CheckBillCode = items.CheckBillCode;
                            }
                            #endregion

                            #region 更新储位状态和明细执行标志
                            var s = await _wmsBaseSlotRep.GetFirstAsync(m => m.SlotCode == outSlot.SlotCode);
                            s.SlotStatus = 3; // 正在出库
                            await _wmsBaseSlotRep.AsUpdateable(s).ExecuteCommandAsync();

                            item.ExecuteFlag = 1;
                            #endregion
                            
                        }
                        #endregion
                        #region 储位状态为正在出库，直接更新执行标志
                        else if (outSlot.SlotStatus == 3)
                        {
                            item.ExecuteFlag = 1;
                        }
                        #endregion
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #region 提交事务并返回结果
            outBeforeDto.AddRange(moveDto); // 先移库后出库

            #region 插入所有任务到数据库
            foreach (var itemTask in taskList)
            {
                itemTask.CheckTaskFlag = 0;
                await _wmsStockCheckTaskRep.InsertAsync(itemTask);
            }
            #endregion

            #region 更新盘点单据和明细
            await _wmsStockCheckNotifyRep.AsUpdateable(listNotify).ExecuteCommandAsync();
            await _wmsStockCheckNotifyDetailRep.AsUpdateable(list).ExecuteCommandAsync();
            #endregion

            _sqlSugarClient.Ado.CommitTran();
            return outBeforeDto;
            #endregion
        }
        catch (Exception ex)
        {
            _sqlSugarClient.Ado.RollbackTran();
            throw Oops.Oh(ex.Message);
        }
    }

    private async Task<string> GetImExNo(string prefix)
    {
        var dateStr = DateTime.Now.ToString("yyyyMMdd");
        var fullPrefix = $"{prefix}{dateStr}";
        var lastCode = await _wmsStockCheckTaskRep.AsQueryable()
            .Where(u => u.CheckTaskNo.StartsWith(fullPrefix))
            .OrderByDescending(u => u.CheckTaskNo)
            .Select(u => u.CheckTaskNo)
            .FirstAsync();

        if (string.IsNullOrWhiteSpace(lastCode))
        {
            return $"{fullPrefix}001";
        }

        var numberStr = lastCode.Substring(fullPrefix.Length);
        if (int.TryParse(numberStr, out var number))
        {
            return $"{fullPrefix}{(number + 1).ToString("D3")}";
        }

        return $"{fullPrefix}001";
    }

    private async Task EditTaskStatus(string taskNo, int status)
    {
        var task = await _wmsStockCheckTaskRep.GetFirstAsync(u => u.CheckTaskNo == taskNo);
        if (task != null)
        {
            task.CheckTaskFlag = status;
            await _wmsStockCheckTaskRep.AsUpdateable(task).ExecuteCommandAsync();
        }
    }
    #endregion

    /// <summary>
    /// 删除盘点单 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除盘点单")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsStockCheckNotifyInput input)
    {
        try
        {
            _sqlSugarClient.Ado.BeginTran();

            var entity = await _wmsStockCheckNotifyRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
            
            var checkBillCode = entity.CheckBillCode;

            var details = await _wmsStockCheckNotifyDetailRep.AsQueryable()
                .Where(u => u.CheckBillCode == checkBillCode && u.IsDelete == false)
                .ToListAsync();

            if (details.Count > 0)
            {
                var detailIds = details.Select(d => d.Id).ToList();
                await _wmsStockCheckInfoRep.AsUpdateable()
                    .SetColumns(it => it.IsDelete == true)
                    .Where(it => detailIds.Contains(it.StockCheckId) && it.IsDelete == false)
                    .ExecuteCommandAsync();

                var stockCodes = details.Select(d => d.StockCode).Distinct().ToList();
                if (stockCodes.Count > 0)
                {
                    var stockTrays = await _wmsStockTrayRep.AsQueryable()
                        .Where(u => stockCodes.Contains(u.StockCode))
                        .ToListAsync();
                    if (stockTrays.Count > 0)
                    {
                        foreach (var stockTray in stockTrays)
                        {
                            stockTray.StockStatusFlag = 0;
                        }
                        await _wmsStockTrayRep.AsUpdateable(stockTrays).ExecuteCommandAsync();
                    }
                }

                await _wmsStockCheckNotifyDetailRep.FakeDeleteAsync(details);
            }

            await _wmsStockCheckNotifyRep.FakeDeleteAsync(entity);

            _sqlSugarClient.Ado.CommitTran();

            await WmsBaseOperLogHelper.RecordAsync(
                Module: "盘点单据",
                OperationType: OperationTypeEnum.Delete.GetEnumDescription(),
                BusinessNo: entity.CheckBillCode,
                OperationContent: $"删除盘点单据：{entity.CheckBillCode} 成功",
                OperParam: input
            );
        }
        catch (Exception ex)
        {
            _sqlSugarClient.Ado.RollbackTran();
            throw Oops.Oh($"删除盘点单失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除盘点单据 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除盘点单据")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")] List<DeleteWmsStockCheckNotifyInput> input)
    {
        try
        {
            _sqlSugarClient.Ado.BeginTran();

            var exp = Expressionable.Create<WmsStockCheckNotify>();
            foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
            var list = await _wmsStockCheckNotifyRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
            var billies = string.Join(',', list.Select(a => a.CheckBillCode));

            var billCodes = list.Select(a => a.CheckBillCode).ToList();

            var details = await _wmsStockCheckNotifyDetailRep.AsQueryable()
                .Where(u => billCodes.Contains(u.CheckBillCode) && u.IsDelete == false)
                .ToListAsync();

            if (details.Count > 0)
            {
                var detailIds = details.Select(d => d.Id).ToList();
                await _wmsStockCheckInfoRep.AsUpdateable()
                    .SetColumns(it => it.IsDelete == true)
                    .Where(it => detailIds.Contains(it.StockCheckId) && it.IsDelete == false)
                    .ExecuteCommandAsync();

                var stockCodes = details.Select(d => d.StockCode).Distinct().ToList();
                if (stockCodes.Count > 0)
                {
                    var stockTrays = await _wmsStockTrayRep.AsQueryable()
                        .Where(u => stockCodes.Contains(u.StockCode))
                        .ToListAsync();
                    if (stockTrays.Count > 0)
                    {
                        foreach (var stockTray in stockTrays)
                        {
                            stockTray.StockStatusFlag = 0;
                        }
                        await _wmsStockTrayRep.AsUpdateable(stockTrays).ExecuteCommandAsync();
                    }
                }

                await _wmsStockCheckNotifyDetailRep.FakeDeleteAsync(details);
            }

            var result = await _wmsStockCheckNotifyRep.FakeDeleteAsync(list);

            _sqlSugarClient.Ado.CommitTran();

            await WmsBaseOperLogHelper.RecordAsync(
                Module: "盘点单据",
                OperationType: OperationTypeEnum.Deletes.GetEnumDescription(),
                BusinessNo: billies,
                OperationContent: $"批量删除盘点单据：{billies} 成功",
                OperParam: input
            );

            return result;
        }
        catch (Exception ex)
        {
            _sqlSugarClient.Ado.RollbackTran();
            throw Oops.Oh($"批量删除盘点单失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取盘点单据详情（包含明细列表）ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取盘点单据详情（包含明细列表）")]
    [ApiDescriptionSettings(Name = "Detail"), HttpPost]
    public async Task<WmsStockCheckNotifyOutput> Detail(QueryByIdWmsStockCheckNotifyInput input)
    {
        var entity = await _wmsStockCheckNotifyRep.GetFirstAsync(u => u.Id == input.Id);
        if (entity == null)
        {
            throw Oops.Oh("盘点单据不存在");
        }

        var detailList = await _wmsStockCheckNotifyDetailRep.AsQueryable()
            .LeftJoin<WmsBaseMaterial>((a, d) => a.MaterialId == d.Id)
            .Where(a => a.CheckBillCode == entity.CheckBillCode && a.IsDelete == false)
            .Select((a, d) => new WmsStockCheckNotifyDetailOutput
            {
                Id = a.Id,
                CheckBillCode = a.CheckBillCode,
                CheckStockId = a.CheckStockId,
                StockSlot = a.StockSlot,
                StockCode = a.StockCode,
                StockQuantity = a.StockQuantity,
                StockLotNo = a.StockLotNo,
                ExecuteFlag = a.ExecuteFlag,
                MaterialId = a.MaterialId,
                MaterialCode = d.MaterialCode,
                MaterialName = d.MaterialName,
                MaterialStandard = d.MaterialStandard,
                InspectionStatus = a.InspectionStatus,
                WarehouseId = a.WarehouseId,
                RealQuantity = a.RealQuantity,
                CheckResult = a.CheckResult,
                AddDate = a.AddDate,
                TaskId = a.TaskId,
                CheckTaskNo = a.CheckTaskNo,
                CheckRemark = a.CheckRemark
            })
            .ToListAsync();

        var output = entity.Adapt<WmsStockCheckNotifyOutput>();
        output.List = detailList;
        return output;
    }

    /// <summary>
    /// 生成盘库编码
    /// </summary>
    /// <returns></returns>
    private async Task<string> GetCheckBillCode()
    {
        var dateStr = DateTime.Now.ToString("yyyyMMdd");
        var prefix = $"PD{dateStr}";
        var lastCode = await _wmsStockCheckNotifyRep.AsQueryable()
            .Where(u => u.CheckBillCode.StartsWith(prefix))
            .OrderByDescending(u => u.CheckBillCode)
            .Select(u => u.CheckBillCode)
            .FirstAsync();

        if (string.IsNullOrWhiteSpace(lastCode))
        {
            return $"{prefix}001";
        }

        var numberStr = lastCode.Substring(prefix.Length);
        if (int.TryParse(numberStr, out var number))
        {
            return $"{prefix}{(number + 1).ToString("D3")}";
        }

        return $"{prefix}001";
    }

    /// <summary>
    /// 调整库存 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("调整库存")]
    [ApiDescriptionSettings(Name = "CancelStockEdit"), HttpPost]
    public async Task CancelStockEdit(CancelStockEditInput input)
    {
        try
        {
            var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
            var userName = App.User?.FindFirst(ClaimConst.RealName)?.Value;

            _sqlSugarClient.Ado.BeginTran();

            var check = await _wmsStockCheckNotifyRep.GetFirstAsync(u => u.Id == input.Id);
            if (check == null)
            {
                throw Oops.Oh("获取盘点单据失败！");
            }

            var checkDetail = await _wmsStockCheckNotifyDetailRep.AsQueryable()
                .Where(u => u.CheckBillCode == check.CheckBillCode && u.IsDelete == false)
                .ToListAsync();

            if (checkDetail.Count == 0)
            {
                throw Oops.Oh("获取盘点单据明细失败！");
            }

            // 校验所有盘点明细是否都已完成盘点（有盘点结果）
            var unCheckedDetails = checkDetail.Where(u => !u.CheckResult.HasValue || u.CheckResult == 0).ToList();
            if (unCheckedDetails.Count > 0)
            {
                var stockCodes = string.Join(", ", unCheckedDetails.Select(d => d.StockCode).Distinct());
                throw Oops.Oh($"存在未完成盘点的物料，托盘条码：{stockCodes}");
            }

            foreach (var item in checkDetail)
            {
                var warehouseId = long.TryParse(check.WarehouseId, out var whId) ? whId : 0;
                var materialId = item.MaterialId ?? 0;

                var stock = await _wmsStockRep.GetFirstAsync(u => u.WarehouseId == warehouseId && u.MaterialId == materialId && u.LotNo == item.StockLotNo);
                if (stock == null)
                {
                    throw Oops.Oh("获取库存主信息失败！");
                }

                var stockTary = await _wmsStockTrayRep.GetFirstAsync(u => u.MaterialId == item.MaterialId.ToString() && u.WarehouseId == check.WarehouseId && u.LotNo == item.StockLotNo && u.StockCode == item.StockCode);
                if (stockTary == null)
                {
                    throw Oops.Oh("获取库存托盘信息失败！");
                }

                var stockCheckInfo = await _wmsStockCheckInfoRep.AsQueryable()
                    .Where(u => u.StockCheckId == item.Id && u.StockCode == item.StockCode && u.LotNo == item.StockLotNo && u.MaterialId == materialId)
                    .ToListAsync();

                if (stockCheckInfo.Count == 0)
                {
                    throw Oops.Oh("获取盘点箱码信息失败！");
                }

                // 校验该托盘下所有箱码是否都已完成盘点
                var unCheckedBoxes = stockCheckInfo.Where(u => !u.CheckResult.HasValue || u.CheckResult == 0).ToList();
                if (unCheckedBoxes.Count > 0)
                {
                    throw Oops.Oh($"托盘 {item.StockCode} 存在未完成盘点的箱码");
                }

                foreach (var itemInfo in stockCheckInfo)
                {
                    if (itemInfo.CheckResult > 0 || itemInfo.RealQuantity.HasValue)
                    {
                        WmsStockInfo stockInfo;
                        if (!string.IsNullOrWhiteSpace(itemInfo.BoxCode))
                        {
                            stockInfo = await _wmsStockInfoRep.GetFirstAsync(u => u.TrayId == stockTary.Id.ToString() && u.BoxCode == itemInfo.BoxCode && u.LotNo == itemInfo.LotNo && u.MaterialId == materialId.ToString());
                        }
                        else
                        {
                            stockInfo = await _wmsStockInfoRep.GetFirstAsync(u => u.TrayId == stockTary.Id.ToString() && u.LotNo == itemInfo.LotNo && u.MaterialId == materialId.ToString());
                        }

                        if (stockInfo != null && itemInfo.RealQuantity.HasValue)
                        {
                            stockInfo.Qty = itemInfo.RealQuantity;
                            await _wmsStockInfoRep.AsUpdateable(stockInfo).ExecuteCommandAsync();
                        }
                    }
                }

                if (item.CheckResult > 0)
                {
                    var realQuantity = item.RealQuantity ?? 0;
                    var stockQuantity = stockTary.StockQuantity ?? 0;

                    if (item.CheckResult == 1)
                    {
                        var sum = stockQuantity - realQuantity;
                        stock.StockQuantity = (stock.StockQuantity ?? 0) - sum;
                    }
                    else if (item.CheckResult == 2)
                    {
                        var sum = realQuantity - stockQuantity;
                        stock.StockQuantity = (stock.StockQuantity ?? 0) + sum;
                    }
                    stockTary.StockQuantity = realQuantity;

                    await _wmsStockRep.AsUpdateable(stock).ExecuteCommandAsync();
                    await _wmsStockTrayRep.AsUpdateable(stockTary).ExecuteCommandAsync();
                }

                item.ExecuteFlag = 3;
                await _wmsStockCheckNotifyDetailRep.AsUpdateable(item).ExecuteCommandAsync();
            }

            check.ExecuteFlag = 3;
            check.UpdateUserId = userId;
            check.UpdateUserName = userName;
            check.UpdateTime = DateTime.Now;
            await _wmsStockCheckNotifyRep.AsUpdateable(check).ExecuteCommandAsync();

            _sqlSugarClient.Ado.CommitTran();

            await WmsBaseOperLogHelper.RecordAsync(
                Module: "盘点单据",
                OperationType: OperationTypeEnum.Update.GetEnumDescription(),
                BusinessNo: check.CheckBillCode,
                OperationContent: $"调整库存：{check.CheckBillCode} 成功",
                OperParam: input
            );
        }
        catch (Exception ex)
        {
            _sqlSugarClient.Ado.RollbackTran();
            throw Oops.Oh($"调整库存失败：{ex.Message}");
        }
    }
}
