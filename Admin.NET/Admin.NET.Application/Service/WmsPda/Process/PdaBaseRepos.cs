using Admin.NET.Application.Entity;
using Admin.NET.Core;

namespace Admin.NET.Application.Service.WmsPda.Process;

/// <summary>
/// PDA 业务仓储聚合基类
/// 将 PDA 业务模块常用的仓储依赖统一管理，供各个 PDA 业务类复用
/// </summary>
/// <remarks>
/// 包含的仓储类型：
/// - 库存相关：WmsStockTray、WmsStockSlotBoxInfo、WmsStockInfo、WmsStock、WmsSysStockCode
/// - 入库相关：WmsImportNotify、WmsImportNotifyDetail、WmsImportOrder、WmsImportLabelPrint、WmsInspectNotity
/// - 出库相关：WmsExportOrder、WmsExportNotify、WmsExportNotifyDetail、WmsExportTask、WmsExportBoxInfo
/// - 基础数据：WmsBaseMaterial、WmsBaseWareHouse、WmsBaseSlot
/// - 盘点相关：WmsStockCheckNotify、WmsStockCheckNotifyDetail、WmsStockCheckInfo、WmsStockUnbind
/// - 特殊业务：WmsManualDepalletizing
///
/// 使用场景：
/// - PdaImportBase - 入库业务基类
/// - PdaExportBase - 出库业务基类
/// - PdaStockTake - 盘点业务
/// - 其他需要这些仓储的 PDA 业务类
/// </remarks>
public class PdaBaseRepos : ITransient
{
    #region 构造函数
    /// <summary>
    /// 构造函数 - 通过依赖注入初始化所有仓储
    /// </summary>
    public PdaBaseRepos(
        SqlSugarRepository<WmsStockTray> stockTrayRep,
        SqlSugarRepository<WmsStockSlotBoxInfo> stockSlotBoxInfoRep,
        SqlSugarRepository<WmsStockInfo> stockInfoRep,
        SqlSugarRepository<WmsStock> stockRep,
        SqlSugarRepository<WmsSysStockCode> sysStockCodeRep,
        SqlSugarRepository<WmsImportNotify> importNotifyRep,
        SqlSugarRepository<WmsImportNotifyDetail> importNotifyDetailRep,
        SqlSugarRepository<WmsImportOrder> importOrderRep,
        SqlSugarRepository<WmsImportLabelPrint> importLabelPrintRep,
        SqlSugarRepository<WmsInspectNotity> inspectNotifyRep,
        SqlSugarRepository<WmsExportOrder> exportOrderRep,
        SqlSugarRepository<WmsExportNotify> exportNotifyRep,
        SqlSugarRepository<WmsExportNotifyDetail> exportNotifyDetailRep,
        SqlSugarRepository<WmsExportTask> exportTaskRep,
        SqlSugarRepository<WmsExportBoxInfo> exportBoxInfoRep,
        SqlSugarRepository<WmsBaseMaterial> materialRep,
        SqlSugarRepository<WmsBaseWareHouse> warehouseRep,
        SqlSugarRepository<WmsBaseSlot> slotRep,
        SqlSugarRepository<WmsStockCheckNotify> stockCheckNotifyRep,
        SqlSugarRepository<WmsStockCheckNotifyDetail> stockCheckNotifyDetailRep,
        SqlSugarRepository<WmsStockCheckInfo> stockCheckInfoRep,
        SqlSugarRepository<WmsStockUnbind> stockUnbindRep,
        SqlSugarRepository<WmsManualDepalletizing> manualDepalletizingRep)
    {
        StockTray = stockTrayRep;
        StockSlotBoxInfo = stockSlotBoxInfoRep;
        StockInfo = stockInfoRep;
        Stock = stockRep;
        SysStockCode = sysStockCodeRep;
        ImportNotify = importNotifyRep;
        ImportNotifyDetail = importNotifyDetailRep;
        ImportOrder = importOrderRep;
        ImportLabelPrint = importLabelPrintRep;
        InspectNotify = inspectNotifyRep;
        ExportOrder = exportOrderRep;
        ExportNotify = exportNotifyRep;
        ExportNotifyDetail = exportNotifyDetailRep;
        ExportTask = exportTaskRep;
        ExportBoxInfo = exportBoxInfoRep;
        Material = materialRep;
        Warehouse = warehouseRep;
        Slot = slotRep;
        StockCheckNotify = stockCheckNotifyRep;
        StockCheckNotifyDetail = stockCheckNotifyDetailRep;
        StockCheckInfo = stockCheckInfoRep;
        StockUnbind = stockUnbindRep;
        ManualDepalletizing = manualDepalletizingRep;
    }
    #endregion

    #region 库存相关仓储
    /// <summary>
    /// 库存托盘仓储
    /// </summary>
    public SqlSugarRepository<WmsStockTray> StockTray { get; }

    /// <summary>
    /// 库存箱托信息仓储
    /// </summary>
    public SqlSugarRepository<WmsStockSlotBoxInfo> StockSlotBoxInfo { get; }

    /// <summary>
    /// 库存箱信息仓储
    /// </summary>
    public SqlSugarRepository<WmsStockInfo> StockInfo { get; }

    /// <summary>
    /// 库存信息仓储
    /// </summary>
    public SqlSugarRepository<WmsStock> Stock { get; }

    /// <summary>
    /// 系统库存编码仓储
    /// </summary>
    public SqlSugarRepository<WmsSysStockCode> SysStockCode { get; }
    #endregion

    #region 入库相关仓储
    /// <summary>
    /// 入库单据仓储
    /// </summary>
    public SqlSugarRepository<WmsImportNotify> ImportNotify { get; }

    /// <summary>
    /// 入库单据明细仓储
    /// </summary>
    public SqlSugarRepository<WmsImportNotifyDetail> ImportNotifyDetail { get; }

    /// <summary>
    /// 入库流水仓储
    /// </summary>
    public SqlSugarRepository<WmsImportOrder> ImportOrder { get; }

    /// <summary>
    /// 入库标签打印仓储
    /// </summary>
    public SqlSugarRepository<WmsImportLabelPrint> ImportLabelPrint { get; }

    /// <summary>
    /// 检验通知仓储
    /// </summary>
    public SqlSugarRepository<WmsInspectNotity> InspectNotify { get; }
    #endregion

    #region 出库相关仓储
    /// <summary>
    /// 出库订单仓储
    /// </summary>
    public SqlSugarRepository<WmsExportOrder> ExportOrder { get; }

    /// <summary>
    /// 出库通知仓储
    /// </summary>
    public SqlSugarRepository<WmsExportNotify> ExportNotify { get; }

    /// <summary>
    /// 出库通知明细仓储
    /// </summary>
    public SqlSugarRepository<WmsExportNotifyDetail> ExportNotifyDetail { get; }

    /// <summary>
    /// 出库任务仓储
    /// </summary>
    public SqlSugarRepository<WmsExportTask> ExportTask { get; }

    /// <summary>
    /// 出库箱码信息仓储
    /// </summary>
    public SqlSugarRepository<WmsExportBoxInfo> ExportBoxInfo { get; }
    #endregion

    #region 基础数据仓储
    /// <summary>
    /// 物料主数据仓储
    /// </summary>
    public SqlSugarRepository<WmsBaseMaterial> Material { get; }

    /// <summary>
    /// 仓库仓储
    /// </summary>
    public SqlSugarRepository<WmsBaseWareHouse> Warehouse { get; }

    /// <summary>
    /// 基础库位仓储
    /// </summary>
    public SqlSugarRepository<WmsBaseSlot> Slot { get; }
    #endregion

    #region 盘点相关仓储
    /// <summary>
    /// 盘点通知仓储
    /// </summary>
    public SqlSugarRepository<WmsStockCheckNotify> StockCheckNotify { get; }

    /// <summary>
    /// 盘点通知明细仓储
    /// </summary>
    public SqlSugarRepository<WmsStockCheckNotifyDetail> StockCheckNotifyDetail { get; }

    /// <summary>
    /// 盘点箱码信息仓储
    /// </summary>
    public SqlSugarRepository<WmsStockCheckInfo> StockCheckInfo { get; }

    /// <summary>
    /// 托盘变更记录仓储
    /// </summary>
    public SqlSugarRepository<WmsStockUnbind> StockUnbind { get; }
    #endregion

    #region 其他特殊业务仓储
    /// <summary>
    /// 手动拆垛仓储
    /// </summary>
    public SqlSugarRepository<WmsManualDepalletizing> ManualDepalletizing { get; }
    #endregion
}
