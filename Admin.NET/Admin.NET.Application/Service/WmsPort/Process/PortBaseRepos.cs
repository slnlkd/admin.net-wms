using Admin.NET.Application.Entity;
using Admin.NET.Core;

namespace Admin.NET.Application.Service.WmsPort.Process;

/// <summary>
/// Port 业务仓储聚合基类
/// 将 Port 业务模块常用的仓储依赖统一管理，供各个 Port 业务类复用
/// </summary>
/// <remarks>
/// 包含的仓储类型：
/// - 任务相关：WmsImportTask、WMSTaskNoPub
/// - 库存相关：WmsStockTray、WmsStockSlotBoxInfo、WmsSysStockCode、WmsStockCheckTask2
/// - 库位相关：WmsBaseSlot
/// - 主数据：WmsBaseMaterial、WmsBaseLaneway、WmsBaseWareHouse
/// - 入库单据单：WmsImportNotify、WmsImportNotifyDetail、WmsImportOrder
/// - 特殊业务：WmsExportOrder、WmsImportSecondCursor、WmsBackToWareHouse
///
/// 使用场景：
/// - PortImportApply - 入库申请业务
/// - PortExportEmpty - 出库业务
/// - PortFeedback - 反馈业务
/// - 其他需要这些仓储的 Port 业务类
/// </remarks>
public class PortBaseRepos : ITransient
{
    #region 构造函数
    /// <summary>
    /// 构造函数 - 通过依赖注入初始化所有仓储
    /// </summary>
    public PortBaseRepos(
        SqlSugarRepository<WmsImportTask> importTaskRep,
        SqlSugarRepository<WmsImportNotify> importNotifyRep,
        SqlSugarRepository<WmsImportNotifyDetail> importNotifyDetailRep,
        SqlSugarRepository<WmsImportOrder> importOrderRep,
        SqlSugarRepository<WmsImportLabelPrint> importLabelPrintRep,
        SqlSugarRepository<WmsImportSecondCursor> secondCursorRep,
        SqlSugarRepository<WMSTaskNoPub> taskNoPubRep,
        SqlSugarRepository<WmsStockTray> stockTrayRep,
        SqlSugarRepository<WmsStockSlotBoxInfo> boxInfoRep,
        SqlSugarRepository<WmsStockInfo> stockInfoRep,
        SqlSugarRepository<WmsStock> stockRep,
        SqlSugarRepository<WmsSysStockCode> stockCodeRep,
        SqlSugarRepository<WmsStockCheckTask> stockCheckTaskRep,
        SqlSugarRepository<WmsBaseArea> areaRep,
        SqlSugarRepository<WmsBaseSlot> slotRep,
        SqlSugarRepository<WmsBaseMaterial> materialRep,
        SqlSugarRepository<WmsBaseLaneway> lanewayRep,
        SqlSugarRepository<WmsBaseWareHouse> warehouseRep,
        SqlSugarRepository<WmsExportOrder> exportOrderRep,
        SqlSugarRepository<WmsExportTask> exportTaskRep,
        SqlSugarRepository<WmsBackToWareHouse> backToWarehouseRep,
        SqlSugarRepository<WmsMoveTask> moveTaskRep,
        SqlSugarRepository<WmsMoveNotify> moveNotifyRep,
        SqlSugarRepository<WmsMoveOrder> moveOrderRep,
        SqlSugarRepository<WmsStockCheckNotify> stockCheckNotifyRep,
        SqlSugarRepository<WmsStockCheckNotifyDetail> stockCheckNotifyDetailRep
        )
    {
        ImportTask = importTaskRep;
        ImportNotify = importNotifyRep;
        ImportNotifyDetail = importNotifyDetailRep;
        ImportOrder = importOrderRep;
        ImportLabelPrint = importLabelPrintRep;
        TaskNoPub = taskNoPubRep;
        StockTray = stockTrayRep;
        BoxInfo = boxInfoRep;
        StockCode = stockCodeRep;
        StockCheckTask = stockCheckTaskRep;
        StockInfo = stockInfoRep;
        Stock = stockRep;
        Slot = slotRep;
        Area = areaRep;
        Material = materialRep;
        Laneway = lanewayRep;
        Warehouse = warehouseRep;
        ExportOrder = exportOrderRep;
        ExportTask = exportTaskRep;
        SecondCursor = secondCursorRep;
        BackToWarehouse = backToWarehouseRep;
        MoveTask = moveTaskRep;
        MoveNotify = moveNotifyRep;
        MoveOrder = moveOrderRep;
        StockCheckNotify = stockCheckNotifyRep;
        StockCheckNotifyDetail = stockCheckNotifyDetailRep;
    }
    #endregion

    #region 任务相关仓储
    /// <summary>
    /// 入库任务仓储
    /// </summary>
    public SqlSugarRepository<WmsImportTask> ImportTask { get; }

    /// <summary>
    /// 未发布任务仓储
    /// </summary>
    public SqlSugarRepository<WMSTaskNoPub> TaskNoPub { get; }
    #endregion

    #region 库存相关仓储
    /// <summary>
    /// 库存托盘仓储
    /// </summary>
    public SqlSugarRepository<WmsStockTray> StockTray { get; }
    /// <summary>
    /// 库存箱信息仓储
    /// </summary>
    public SqlSugarRepository<WmsStockInfo> StockInfo { get; }

    /// <summary>
    /// 库存信息仓库
    /// </summary>
    public SqlSugarRepository<WmsStock> Stock { get; }

    /// <summary>
    /// 库存箱码信息仓储
    /// </summary>
    public SqlSugarRepository<WmsStockSlotBoxInfo> BoxInfo { get; }

    /// <summary>
    /// 系统库存编码仓储
    /// </summary>
    public SqlSugarRepository<WmsSysStockCode> StockCode { get; }

    /// <summary>
    /// 盘点任务仓储
    /// </summary>
    public SqlSugarRepository<WmsStockCheckTask> StockCheckTask { get; }
    #endregion

    #region 库位相关仓储
    /// <summary>
    /// 基础库位仓储
    /// </summary>
    public SqlSugarRepository<WmsBaseSlot> Slot { get; }
    #endregion

    #region 基础数据仓储
    /// <summary>
    /// 区域仓储
    /// </summary>
    /// <value></value>
    public SqlSugarRepository<WmsBaseArea> Area { get; }
    /// <summary>
    /// 物料主数据仓储
    /// </summary>
    public SqlSugarRepository<WmsBaseMaterial> Material { get; }

    /// <summary>
    /// 巷道仓储
    /// </summary>
    public SqlSugarRepository<WmsBaseLaneway> Laneway { get; }

    /// <summary>
    /// 仓库仓储
    /// </summary>
    public SqlSugarRepository<WmsBaseWareHouse> Warehouse { get; }
    #endregion

    #region 入库单据单相关仓储
    /// <summary>
    /// 入库单据单仓储
    /// </summary>
    public SqlSugarRepository<WmsImportNotify> ImportNotify { get; }

    /// <summary>
    /// 入库单据单明细仓储
    /// </summary>
    public SqlSugarRepository<WmsImportNotifyDetail> ImportNotifyDetail { get; }

    /// <summary>
    /// 入库单仓储
    /// </summary>
    public SqlSugarRepository<WmsImportOrder> ImportOrder { get; }
    /// <summary>
    /// 入库标签打印仓储
    /// </summary>
    /// <value></value>
    public SqlSugarRepository<WmsImportLabelPrint> ImportLabelPrint { get; }
    #endregion

    #region 出库业务仓储
    /// <summary>
    /// 出库单仓储（用于入库业务中的出库关联场景）
    /// </summary>
    public SqlSugarRepository<WmsExportOrder> ExportOrder { get; }

    /// <summary>
    /// 出库任务仓储
    /// </summary>
    public SqlSugarRepository<WmsExportTask> ExportTask { get; }
    #endregion

    #region 其他特殊业务仓储
    /// <summary>
    /// 移库任务仓储
    /// </summary>
    /// <value></value>
    public SqlSugarRepository<WmsMoveTask> MoveTask { get; }
    /// <summary>
    /// 移库通知仓储
    /// </summary>
    /// <value></value>
    public SqlSugarRepository<WmsMoveNotify> MoveNotify { get; }
    /// <summary>
    /// 移库订单仓储
    /// </summary>
    /// <value></value>
    public SqlSugarRepository<WmsMoveOrder> MoveOrder { get; }
    /// <summary>
    /// 二次入库游标仓储
    /// </summary>
    public SqlSugarRepository<WmsImportSecondCursor> SecondCursor { get; }

    /// <summary>
    /// 回仓仓储
    /// </summary>
    public SqlSugarRepository<WmsBackToWareHouse> BackToWarehouse { get; }

    /// <summary>
    /// 盘点单据仓储
    /// </summary>
    public SqlSugarRepository<WmsStockCheckNotify> StockCheckNotify { get; }

    /// <summary>
    /// 盘点单明细仓储
    /// </summary>
    public SqlSugarRepository<WmsStockCheckNotifyDetail> StockCheckNotifyDetail { get; }
    #endregion
}
