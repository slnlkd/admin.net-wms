using Admin.NET.Application.Entity;
using Admin.NET.Core;

namespace Admin.NET.Application;

/// <summary>
/// 入库单服务仓储聚合类
/// 将 WmsImportNotifyService 常用的仓储依赖统一管理，简化构造函数参数
/// </summary>
/// <remarks>
/// 包含的仓储类型：
/// - 入库单主表：WmsImportNotify
/// - 入库单明细：WmsImportNotifyDetail
/// - 基础数据：WmsBaseBillType、WmsBaseSupplier、WmsBaseWareHouse、WmsBaseManufacturer、WmsBaseMaterial
///
/// 使用场景：
/// - WmsImportNotifyService - 入库单主服务
/// </remarks>
public class ImportNotifyRepos : ITransient
{
    #region 入库单相关仓储
    /// <summary>
    /// 入库单据主表仓储
    /// </summary>
    public SqlSugarRepository<WmsImportNotify> ImportNotify { get; }

    /// <summary>
    /// 入库单据明细仓储
    /// </summary>
    public SqlSugarRepository<WmsImportNotifyDetail> ImportNotifyDetail { get; }
    #endregion

    #region 基础数据仓储
    /// <summary>
    /// 单据类型仓储
    /// </summary>
    public SqlSugarRepository<WmsBaseBillType> BillType { get; }

    /// <summary>
    /// 供应商仓储
    /// </summary>
    public SqlSugarRepository<WmsBaseSupplier> Supplier { get; }

    /// <summary>
    /// 仓库仓储
    /// </summary>
    public SqlSugarRepository<WmsBaseWareHouse> Warehouse { get; }

    /// <summary>
    /// 制造商仓储
    /// </summary>
    public SqlSugarRepository<WmsBaseManufacturer> Manufacturer { get; }

    /// <summary>
    /// 物料主数据仓储
    /// </summary>
    public SqlSugarRepository<WmsBaseMaterial> Material { get; }
    #endregion

    /// <summary>
    /// 构造函数 - 通过依赖注入初始化所有仓储
    /// </summary>
    public ImportNotifyRepos(
        SqlSugarRepository<WmsImportNotify> importNotifyRep,
        SqlSugarRepository<WmsImportNotifyDetail> importNotifyDetailRep,
        SqlSugarRepository<WmsBaseBillType> billTypeRep,
        SqlSugarRepository<WmsBaseSupplier> supplierRep,
        SqlSugarRepository<WmsBaseWareHouse> warehouseRep,
        SqlSugarRepository<WmsBaseManufacturer> manufacturerRep,
        SqlSugarRepository<WmsBaseMaterial> materialRep)
    {
        ImportNotify = importNotifyRep;
        ImportNotifyDetail = importNotifyDetailRep;
        BillType = billTypeRep;
        Supplier = supplierRep;
        Warehouse = warehouseRep;
        Manufacturer = manufacturerRep;
        Material = materialRep;
    }
}
