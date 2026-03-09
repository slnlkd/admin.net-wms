// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 库存箱码明细
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsStockSlotBoxInfo", "库存箱码明细")]
public partial class WmsStockSlotBoxInfo : EntityBaseDel
{
    /// <summary>
    /// 箱码
    /// </summary>
    [SugarColumn(ColumnName = "BoxCode", ColumnDescription = "箱码", Length = 32)]
    public virtual string? BoxCode { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    [SugarColumn(ColumnName = "Qty", ColumnDescription = "数量", Length = 18, DecimalDigits=5)]
    public virtual decimal? Qty { get; set; }
    
    /// <summary>
    /// 整箱数
    /// </summary>
    [SugarColumn(ColumnName = "FullBoxQty", ColumnDescription = "整箱数", Length = 18, DecimalDigits=5)]
    public virtual decimal? FullBoxQty { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    [SugarColumn(ColumnName = "StockCode", ColumnDescription = "托盘条码", Length = 50)]
    public virtual string? StockCode { get; set; }
    
    /// <summary>
    /// 托盘条码id
    /// </summary>
    [SugarColumn(ColumnName = "StockCodeId", ColumnDescription = "托盘条码id", Length = 32)]
    public virtual long? StockCodeId { get; set; }
    
    /// <summary>
    /// 状态（0未执行；1正在入库；2已入库）
    /// </summary>
    [SugarColumn(ColumnName = "Status", ColumnDescription = "状态（0未执行；1正在入库；2已入库）")]
    public virtual int? Status { get; set; }
    
    /// <summary>
    /// 入库单id
    /// </summary>
    [SugarColumn(ColumnName = "ImportId", ColumnDescription = "入库单id", Length = 32)]
    public virtual long? ImportId { get; set; }
    
    /// <summary>
    /// 入库单详情id
    /// </summary>
    [SugarColumn(ColumnName = "ImportDetailId", ColumnDescription = "入库单详情id", Length = 32)]
    public virtual long? ImportDetailId { get; set; }
    
    /// <summary>
    /// 入库流水id
    /// </summary>
    [SugarColumn(ColumnName = "ImportOrderId", ColumnDescription = "入库流水id", Length = 32)]
    public virtual long? ImportOrderId { get; set; }
    
    /// <summary>
    /// 等级
    /// </summary>
    [SugarColumn(ColumnName = "BoxLevel", ColumnDescription = "等级")]
    public virtual int? BoxLevel { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [SugarColumn(ColumnName = "ProductionDate", ColumnDescription = "生产日期")]
    public virtual DateTime? ProductionDate { get; set; }
    
    /// <summary>
    /// 保质期
    /// </summary>
    [SugarColumn(ColumnName = "ValidateDay", ColumnDescription = "保质期")]
    public virtual DateTime? ValidateDay { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    [SugarColumn(ColumnName = "LotNo", ColumnDescription = "批次", Length = 32)]
    public virtual string? LotNo { get; set; }
    
    /// <summary>
    /// 物料id
    /// </summary>
    [SugarColumn(ColumnName = "MaterialId", ColumnDescription = "物料id", Length = 32)]
    public virtual long? MaterialId { get; set; }

    /// <summary>
    /// 供应商ID
    /// </summary>
    [SugarColumn(ColumnName = "SupplierId", ColumnDescription = "供应商ID")]
    public virtual long? SupplierId { get; set; }

    /// <summary>
    /// 制造商ID
    /// </summary>
    [SugarColumn(ColumnName = "ManufacturerId", ColumnDescription = "制造商ID")]
    public virtual long? ManufacturerId { get; set; }
    
    /// <summary>
    /// 是否零箱（0否；1是）
    /// </summary>
    [SugarColumn(ColumnName = "BulkTank", ColumnDescription = "是否零箱（0否；1是）")]
    public virtual int? BulkTank { get; set; }
    
    /// <summary>
    /// 是否抽检箱（0否；1是）
    /// </summary>
    [SugarColumn(ColumnName = "IsSamplingBox", ColumnDescription = "是否抽检箱（0否；1是）")]
    public virtual int? IsSamplingBox { get; set; }
    
    /// <summary>
    /// 质检状态（0待检；1合格；2不合格）
    /// </summary>
    [SugarColumn(ColumnName = "InspectionStatus", ColumnDescription = "质检状态（0待检；1合格；2不合格）")]
    public virtual int? InspectionStatus { get; set; }
    
    /// <summary>
    /// erp箱二维码
    /// </summary>
    [SugarColumn(ColumnName = "QRCode", ColumnDescription = "erp箱二维码", Length = 32)]
    public virtual string? QRCode { get; set; }
    
    /// <summary>
    /// 采样日期
    /// </summary>
    [SugarColumn(ColumnName = "SamplingDate", ColumnDescription = "采样日期")]
    public virtual DateTime? SamplingDate { get; set; }
    
    /// <summary>
    /// 浆员编码
    /// </summary>
    [SugarColumn(ColumnName = "StaffCode", ColumnDescription = "浆员编码", Length = 32)]
    public virtual string? StaffCode { get; set; }
    
    /// <summary>
    /// 浆员姓名
    /// </summary>
    [SugarColumn(ColumnName = "StaffName", ColumnDescription = "浆员姓名", Length = 32)]
    public virtual string? StaffName { get; set; }
    
    /// <summary>
    /// 血浆重量
    /// </summary>
    [SugarColumn(ColumnName = "Weight", ColumnDescription = "血浆重量", Length = 18, DecimalDigits=5)]
    public virtual decimal? Weight { get; set; }
    
    /// <summary>
    /// 剔除原因（手持剔除异常血浆存放）
    /// </summary>
    [SugarColumn(ColumnName = "ReasonsForExcl", ColumnDescription = "剔除原因（手持剔除异常血浆存放）", Length = 100)]
    public virtual string? ReasonsForExcl { get; set; }
    
    /// <summary>
    /// 检疫期状态（1检疫期满足；2检疫期不满足；3检疫期不合格）
    /// </summary>
    [SugarColumn(ColumnName = "ExtractStatus", ColumnDescription = "检疫期状态（1检疫期满足；2检疫期不满足；3检疫期不合格）")]
    public virtual int? ExtractStatus { get; set; }
    
    /// <summary>
    /// 是否挑浆（0默认；1挑浆）
    /// </summary>
    [SugarColumn(ColumnName = "PickingSlurry", ColumnDescription = "是否挑浆（0默认；1挑浆）", Length = 1)]
    public virtual string? PickingSlurry { get; set; }
    
    /// <summary>
    /// 血浆拒收类型id
    /// </summary>
    [SugarColumn(ColumnName = "plasmaRejectTypeId", ColumnDescription = "血浆拒收类型id", Length = 50)]
    public virtual string? plasmaRejectTypeId { get; set; }
    
}
