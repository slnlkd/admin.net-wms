// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Application;

/// <summary>
/// 库存明细查询输出参数
/// </summary>
public class WmsStockTrayDto
{
    /// <summary>
    /// 物品编码
    /// </summary>
    public string MaterialIdFkColumn { get; set; }
    
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 储位位置
    /// </summary>
    public string? StockSlotCode { get; set; }
    
    /// <summary>
    /// 托盘编码
    /// </summary>
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 库存数量
    /// </summary>
    public decimal? StockQuantity { get; set; }
    
    /// <summary>
    /// 库存日期
    /// </summary>
    public DateTime? StockDate { get; set; }
    
    /// <summary>
    /// 物品编码
    /// </summary>
    public string? MaterialId { get; set; }
    
    /// <summary>
    /// 库存状态
    /// </summary>
    public int? StockStatusFlag { get; set; }
    
    /// <summary>
    /// 库存批次
    /// </summary>
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// 巷道ID
    /// </summary>
    public string? LanewayId { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public string? WarehouseId { get; set; }
    
    /// <summary>
    /// 库存备注
    /// </summary>
    public string? Remark { get; set; }
    
    /// <summary>
    /// 抽检托
    /// </summary>
    public int? IsSamolingTray { get; set; }
    
    /// <summary>
    /// 锁定数量
    /// </summary>
    public decimal? LockQuantity { get; set; }
    
    /// <summary>
    /// 出库单号
    /// </summary>
    public string? OwnerCode { get; set; }

    /// <summary>
    /// 冻结状态（0 正常 1 冻结 2 ）
    /// </summary>
    public int? AbnormalStatu { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    public DateTime? ValidateDay { get; set; }
    
    /// <summary>
    /// 放行状态（0 正常 1 放行）
    /// </summary>
    public int? ReleaseStatus { get; set; }
    
    /// <summary>
    /// 复验日期
    /// </summary>
    public DateTime? RetestDate { get; set; }
    
    /// <summary>
    /// 质检单号
    /// </summary>
    public string? InspectionNumber { get; set; }
    
    /// <summary>
    /// 件数
    /// </summary>
    public int? outQty { get; set; }
    
    /// <summary>
    /// 物料箱数量
    /// </summary>
    public decimal? BoxQuantity { get; set; }
    
    /// <summary>
    /// 零头标识
    /// </summary>
    public int? OddMarking { get; set; }
    
    /// <summary>
    /// 客户ID
    /// </summary>
    public string? CustomerId { get; set; }
    
    /// <summary>
    /// 改判日期
    /// </summary>
    public DateTime? RevisionDate { get; set; }
    
    /// <summary>
    /// 血浆主子关系对应，存放主血箱ID
    /// </summary>
    public string? VehicleSubId { get; set; }
    
    /// <summary>
    /// 0未验收 1验收完成
    /// </summary>
    public int? InspectFlag { get; set; }
    
    /// <summary>
    /// 改判日期
    /// </summary>
    public DateTime? ReleaseDate { get; set; }
    
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }
    
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
    
    /// <summary>
    /// 软删除
    /// </summary>
    public bool? IsDelete { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
    /// <summary>
    /// 创建者Id
    /// </summary>
    public long? CreateUserId { get; set; }
    
    /// <summary>
    /// 修改者Id
    /// </summary>
    public long? UpdateUserId { get; set; }
    
    /// <summary>
    /// 供应商ID
    /// </summary>
    public long? SupplierId { get; set; }
    
    /// <summary>
    /// 制造商ID
    /// </summary>
    public long? ManufacturerId { get; set; }
    
}
