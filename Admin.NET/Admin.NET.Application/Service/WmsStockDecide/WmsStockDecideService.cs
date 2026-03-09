// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
//
using Admin.NET.Core.Service;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using SqlSugar;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsBaseOperLog;
using Admin.NET.Application.Service.WmsBaseOperLog.Enum;


namespace Admin.NET.Application;

/// <summary>
/// 质检改判服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class WmsStockDecideService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsStockTray> _wmsStockTrayRep;
    private readonly SqlSugarRepository<WmsStockInfo> _wmsStockInfoRep;
    private readonly SqlSugarRepository<WmsStock> _wmsStockRep;
    private readonly SqlSugarRepository<WmsBaseMaterial> _wmsBaseMaterialRep;
    private readonly SqlSugarRepository<WmsBaseWareHouse> _wmsBaseWareHouseRep;
    private readonly SqlSugarRepository<WmsImportNotifyDetail> _wmsImportNotifyDetailRep;
    private readonly SqlSugarRepository<WmsExportNotifyDetail> _wmsExportNotifyDetailRep;
    private readonly SqlSugarRepository<WmsExportNotify> _wmsExportNotifyRep;
    private readonly SqlSugarRepository<WmsExportOrder> _wmsExportOrderRep;
    private readonly ISqlSugarClient _sqlSugarClient;


    public WmsStockDecideService(
        SqlSugarRepository<WmsStockTray> wmsStockTrayRep,
        SqlSugarRepository<WmsStockInfo> wmsStockInfoRep,
        SqlSugarRepository<WmsStock> wmsStockRep,
        SqlSugarRepository<WmsBaseMaterial> wmsBaseMaterialRep,
        SqlSugarRepository<WmsBaseWareHouse> wmsBaseWareHouseRep,
        SqlSugarRepository<WmsImportNotifyDetail> wmsImportNotifyDetailRep,
        SqlSugarRepository<WmsExportNotifyDetail> wmsExportNotifyDetailRep,
        SqlSugarRepository<WmsExportNotify> wmsExportNotifyRep,
        SqlSugarRepository<WmsExportOrder> wmsExportOrderRep,
        ISqlSugarClient sqlSugarClient)
    {
        _wmsStockTrayRep = wmsStockTrayRep;
        _wmsStockInfoRep = wmsStockInfoRep;
        _wmsStockRep = wmsStockRep;
        _wmsBaseMaterialRep = wmsBaseMaterialRep;
        _wmsBaseWareHouseRep = wmsBaseWareHouseRep;
        _wmsImportNotifyDetailRep = wmsImportNotifyDetailRep;
        _wmsExportNotifyDetailRep = wmsExportNotifyDetailRep;
        _wmsExportNotifyRep = wmsExportNotifyRep;
        _wmsExportOrderRep = wmsExportOrderRep;
        _sqlSugarClient = sqlSugarClient;
    }


    /// <summary>
    /// 质检改判列表（按物料、批次、仓库、质检状态分组）
    /// </summary>
    [DisplayName("质检改判列表")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsStockDecideOutput>> Page(PageWmsStockDecideInput input)
    {
        // 基于库存托盘表汇总改判列表
        var query = _wmsStockTrayRep.AsQueryable()

            .LeftJoin<WmsBaseMaterial>((t, m) => t.MaterialId == m.Id.ToString())
            .LeftJoin<WmsBaseWareHouse>((t, m, w) => t.WarehouseId == w.Id.ToString())
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), (t, m, w) => m.MaterialCode.Contains(input.MaterialCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialName), (t, m, w) => m.MaterialName.Contains(input.MaterialName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.LotNo), (t, m, w) => t.LotNo.Contains(input.LotNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.WarehouseId), (t, m, w) => t.WarehouseId == input.WarehouseId.Trim())
            .WhereIF(input.InspectionStatus != null, (t, m, w) => t.InspectionStatus == input.InspectionStatus)
            .GroupBy((t, m, w) => new
            {
                t.MaterialId,
                m.MaterialCode,
                m.MaterialName,
                m.MaterialStandard,
                m.MaterialType,
                t.WarehouseId,
                w.WarehouseName,
                t.LotNo,
                t.InspectionStatus,
                t.RetestDate,
                t.ReleaseStatus,
                t.ReleaseDate,
                t.RevisionDate,
                t.InspectionNumber
            })
            .Select((t, m, w) => new WmsStockDecideOutput
            {
                MaterialId = t.MaterialId,
                MaterialCode = m.MaterialCode,
                MaterialName = m.MaterialName,
                MaterialStandard = m.MaterialStandard,
                MaterialType = m.MaterialType,
                WarehouseId = t.WarehouseId,
                WarehouseName = w.WarehouseName,
                LotNo = t.LotNo,
                InspectionStatus = t.InspectionStatus,
                RetestDate = t.RetestDate,
                ReleaseStatus = t.ReleaseStatus,
                ReleaseDate = t.ReleaseDate,
                RevisionDate = t.RevisionDate,
                InspectionNumber = t.InspectionNumber,
                StockQuantity = SqlFunc.AggregateSum(t.StockQuantity)
            }).MergeTable().OrderBy(m => m.MaterialCode).ToPagedListAsync(input.Page, input.PageSize);

        return await query;
    }

    /// <summary>
    /// 根据条件反填明细
    /// </summary>
    [DisplayName("根据条件反填明细")]
    [ApiDescriptionSettings(Name = "DetailList"), HttpPost]
    public async Task<List<WmsStockTray>> DetailList(WmsStockDecideDetailInput input)
    {
        return await _wmsStockTrayRep.AsQueryable()
            .Where(t => t.MaterialId == input.MaterialId && t.WarehouseId == input.WarehouseId && t.LotNo == input.LotNo && t.InspectionStatus == input.InspectionStatus)
            .ToListAsync();
    }

    /// <summary>
    /// 改判前置校验
    /// </summary>
    [DisplayName("改判前置校验")]
    [ApiDescriptionSettings(Name = "Check"), HttpPost]
    public async Task<bool> Check(WmsStockDecideCheckInput input)
    {
        // 校验必要参数
        if (string.IsNullOrWhiteSpace(input.MaterialId) || string.IsNullOrWhiteSpace(input.LotNo))
            throw Oops.Oh("物料ID或批次不能为空");

        if (!long.TryParse(input.MaterialId, out var materialId))
            throw Oops.Oh("物料ID格式不正确");

        // 入库未关单

        var importExists = await _wmsImportNotifyDetailRep.AsQueryable()
            .Where(a => a.IsDelete == false && a.MaterialId == materialId && a.LotNo == input.LotNo)
            .Where(a => a.ImportExecuteFlag != "04" && (a.ImportExecuteFlag == "02" || a.ImportExecuteFlag == "03"))
            .AnyAsync();
        if (importExists) throw Oops.Oh("请先进行入库关单操作！");

        // 出库未关单
        var exportDetails = await _wmsExportNotifyDetailRep.AsQueryable()
            .Where(a => a.IsDelete == false && a.MaterialId == materialId && a.LotNo == input.LotNo)
            .ToListAsync();
        if (exportDetails.Count > 0)
        {
            foreach (var item in exportDetails)
            {
                var exportNotifyExists = await _wmsExportNotifyRep.AsQueryable()
                    .Where(a => a.IsDelete == false && a.ExportExecuteFlag == 3 && a.ExportBillCode == item.ExportBillCode)
                    .AnyAsync();
                if (exportNotifyExists) throw Oops.Oh("请先进行出库关单操作！");
            }
        }

        // 已生成出库流水
        var exportOrderExists = await _wmsExportOrderRep.AsQueryable()
            .Where(a => a.IsDelete == false && a.ExportMaterialId == materialId && a.ExportLotNo == input.LotNo && a.ExportExecuteFlag < 2)
            .AnyAsync();
        if (exportOrderExists) throw Oops.Oh("已生成出库流水，无法改判！");

        return true;
    }

    /// <summary>
    /// 放行
    /// </summary>
    [DisplayName("放行")]
    [ApiDescriptionSettings(Name = "Release"), HttpPost]
    public async Task<bool> Release(WmsStockDecideReleaseInput input)
    {
        if (input.RevisionDate == null) throw Oops.Oh("改判日期不能为空");

        var rows = await _wmsStockTrayRep.AsUpdateable()
            .SetColumns(t => new WmsStockTray
            {
                ReleaseStatus = input.ReleaseStatus,
                ReleaseDate = DateTime.Today
            })
            .Where(t => t.MaterialId == input.MaterialId
                        && t.WarehouseId == input.WarehouseId
                        && t.LotNo == input.LotNo
                        && t.InspectionStatus == input.InspectionStatus
                        && t.RevisionDate == input.RevisionDate)
            .ExecuteCommandAsync();

        if (rows <= 0) throw Oops.Oh("操作失败");

        var opContent = input.ReleaseStatus == 1 ? "质检放行" : "取消放行";
        await WmsBaseOperLogHelper.RecordAsync(
            Module: "质检改判",
            OperationType: OperationTypeEnum.Update.GetEnumDescription(),
            BusinessNo: $"{input.MaterialId}-{input.LotNo}",
            OperationContent: $"{opContent}：物料{input.MaterialId} 批次{input.LotNo} 仓库{input.WarehouseId}",
            OperParam: input
        );
        return true;

    }

    /// <summary>
    /// 改判
    /// </summary>
    [DisplayName("改判")]
    [ApiDescriptionSettings(Name = "Decide"), HttpPost]
    public async Task<bool> Decide(WmsStockDecideInput input)
    {
        await Check(new WmsStockDecideCheckInput { MaterialId = input.MaterialId, LotNo = input.LotNo });

        if (!long.TryParse(input.MaterialId, out var materialId))
            throw Oops.Oh("物料ID格式不正确");
        if (!long.TryParse(input.WarehouseId, out var warehouseId))
            throw Oops.Oh("仓库ID格式不正确");

        var tran = await _sqlSugarClient.Ado.UseTranAsync(async () =>
        {
            var trays = await _wmsStockTrayRep.AsQueryable()
                .Where(t => t.MaterialId == input.MaterialId && t.WarehouseId == input.WarehouseId && t.LotNo == input.LotNo)
                .Where(t => t.InspectionStatus == input.InspectionStatusUpt || t.InspectionStatus == input.InspectionStatus)
                .ToListAsync();

            if (trays.Count == 0) throw Oops.Oh("获取信息失败");

            var wareHouse = await _wmsBaseWareHouseRep.AsQueryable()
                .Where(w => w.Id == warehouseId)
                .FirstAsync();

            if (wareHouse == null)
            {
                throw Oops.Oh("未能获取到仓库信息");
            }

            foreach (var group in trays.GroupBy(t => t.StockCode))
            {
                var list = group.ToList();
                var first = list.First();

                if (wareHouse.WarehouseType != "06")
                {
                    var trayIds = list.Select(t => t.Id.ToString()).ToList();
                    var infos = await _wmsStockInfoRep.AsQueryable().Where(i => trayIds.Contains(i.TrayId)).ToListAsync();
                    if (infos.Count == 0) throw Oops.Oh("获取箱码信息失败");
                    infos.ForEach(i => i.InspectionStatus = input.InspectionStatus);
                    await _wmsStockInfoRep.AsUpdateable(infos).UpdateColumns(i => new { i.InspectionStatus }).ExecuteCommandAsync();
                }

                first.StockQuantity = list.Sum(t => t.StockQuantity ?? 0);
                first.InspectionNumber = input.InspectionNumber;
                first.RevisionDate = DateTime.Today;
                first.InspectionStatus = input.InspectionStatus;
                first.RetestDate = input.RetestDate;
                await _wmsStockTrayRep.AsUpdateable(first)
                    .UpdateColumns(t => new { t.StockQuantity, t.InspectionNumber, t.RevisionDate, t.InspectionStatus, t.RetestDate })
                    .ExecuteCommandAsync();

                var deleteList = list.Skip(1).ToList();
                if (deleteList.Count > 0)
                    await _wmsStockTrayRep.FakeDeleteAsync(deleteList);
            }



            var stockList = await _wmsStockRep.AsQueryable()
                .Where(a => a.MaterialId == materialId && a.LotNo == input.LotNo && a.WarehouseId == warehouseId)
                .Where(a => a.InspectionStatus == input.InspectionStatusUpt || a.InspectionStatus == input.InspectionStatus)
                .ToListAsync();

            if (stockList.Count == 0) throw Oops.Oh("获取信息失败");

            if (stockList.Count > 1)
            {
                var firstStock = stockList.First();
                firstStock.InspectionStatus = input.InspectionStatus;
                firstStock.StockQuantity = stockList.Sum(s => s.StockQuantity ?? 0);
                await _wmsStockRep.AsUpdateable(firstStock)
                    .UpdateColumns(s => new { s.InspectionStatus, s.StockQuantity })
                    .ExecuteCommandAsync();
                await _wmsStockRep.FakeDeleteAsync(stockList.Skip(1).ToList());
            }
            else
            {
                var firstStock = stockList.First();
                firstStock.InspectionStatus = input.InspectionStatus;
                await _wmsStockRep.AsUpdateable(firstStock)
                    .UpdateColumns(s => new { s.InspectionStatus })
                    .ExecuteCommandAsync();
            }
        });

        if (!tran.IsSuccess) throw Oops.Oh(tran.ErrorMessage ?? "改判失败");

        await WmsBaseOperLogHelper.RecordAsync(
            Module: "质检改判",
            OperationType: OperationTypeEnum.Update.GetEnumDescription(),
            BusinessNo: $"{input.MaterialId}-{input.LotNo}",
            OperationContent: $"质检改判：物料{input.MaterialId} 批次{input.LotNo} 仓库{input.WarehouseId} {input.InspectionStatusUpt}→{input.InspectionStatus}",
            OperParam: input
        );
        return true;

    }
}
