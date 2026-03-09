// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 库存箱信息表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsStockInfo", "库存箱信息表")]
public partial class WmsStockInfo : EntityBaseDel
{
    /// <summary>
    /// 箱编码
    /// </summary>
    [SugarColumn(ColumnName = "BoxCode", ColumnDescription = "箱编码", Length = 32)]
    public virtual string? BoxCode { get; set; }
    
    /// <summary>
    /// 托盘ID
    /// </summary>
    [SugarColumn(ColumnName = "TrayId", ColumnDescription = "托盘ID", Length = 32)]
    public virtual string? TrayId { get; set; }
    
    /// <summary>
    /// 物品ID
    /// </summary>
    [SugarColumn(ColumnName = "MaterialId", ColumnDescription = "物品ID", Length = 32)]
    public virtual string? MaterialId { get; set; }

    /// <summary>
    /// 物品状态
    /// </summary>
    [SugarColumn(ColumnName = "MaterialStatus", ColumnDescription = "物品状态", Length = 32)]
    public virtual string? MaterialStatus { get; set; }
    
    /// <summary>
    /// 批次号
    /// </summary>
    [SugarColumn(ColumnName = "LotNo", ColumnDescription = "批次号", Length = 32)]
    public virtual string? LotNo { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    [SugarColumn(ColumnName = "Qty", ColumnDescription = "数量", Length = 18, DecimalDigits=0)]
    public virtual decimal? Qty { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [SugarColumn(ColumnName = "ProductionDate", ColumnDescription = "生产日期")]
    public virtual DateTime? ProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    [SugarColumn(ColumnName = "ValidateDay", ColumnDescription = "失效日期")]
    public virtual DateTime? ValidateDay { get; set; }
    
    /// <summary>
    /// 质检状态（0 待检验 1 合格品 2 不合格）
    /// </summary>
    [SugarColumn(ColumnName = "InspectionStatus", ColumnDescription = "质检状态（0 待检验 1 合格品 2 不合格）")]
    public virtual int? InspectionStatus { get; set; }
    
    /// <summary>
    /// 检疫期状态（1：检疫期满足，2：检疫期不满足，3：检疫期不合格）
    /// </summary>
    [SugarColumn(ColumnName = "ExtractStatus", ColumnDescription = "检疫期状态（1：检疫期满足，2：检疫期不满足，3：检疫期不合格）")]
    public virtual int? ExtractStatus { get; set; }
    
    /// <summary>
    /// 是否变更箱（0 否 1是）
    /// </summary>
    [SugarColumn(ColumnName = "IsChangeBox", ColumnDescription = "是否变更箱（0 否 1是）")]
    public virtual int? IsChangeBox { get; set; }
    
    /// <summary>
    /// 是否抽检箱（0 否 1是）
    /// </summary>
    [SugarColumn(ColumnName = "IsSamplingBox", ColumnDescription = "是否抽检箱（0 否 1是）")]
    public virtual int? IsSamplingBox { get; set; }
    
    /// <summary>
    /// 是否零头箱（0 否 1是）
    /// </summary>
    [SugarColumn(ColumnName = "IsFractionBox", ColumnDescription = "是否零头箱（0 否 1是）")]
    public virtual int? IsFractionBox { get; set; }
    
    /// <summary>
    /// 锁定数量
    /// </summary>
    [SugarColumn(ColumnName = "LockQuantity", ColumnDescription = "锁定数量", Length = 18, DecimalDigits=0)]
    public virtual decimal? LockQuantity { get; set; }
    
    /// <summary>
    /// 拣货状态（0 待拣货 1 已拣货）
    /// </summary>
    [SugarColumn(ColumnName = "Picked", ColumnDescription = "拣货状态（0 待拣货 1 已拣货）")]
    public virtual int? Picked { get; set; }
    
    /// <summary>
    /// ERP箱二维码
    /// </summary>
    [SugarColumn(ColumnName = "QRCode", ColumnDescription = "ERP箱二维码", Length = 50)]
    public virtual string? QRCode { get; set; }
    
    /// <summary>
    /// ERP箱RFID码
    /// </summary>
    [SugarColumn(ColumnName = "RFIDCode", ColumnDescription = "ERP箱RFID码", Length = 50)]
    public virtual string? RFIDCode { get; set; }
    
    /// <summary>
    /// 件数
    /// </summary>
    [SugarColumn(ColumnName = "outQty", ColumnDescription = "件数")]
    public virtual int? outQty { get; set; }
    
    /// <summary>
    /// 物料箱数量
    /// </summary>
    [SugarColumn(ColumnName = "BoxQuantity", ColumnDescription = "物料箱数量", Length = 18, DecimalDigits=0)]
    public virtual decimal? BoxQuantity { get; set; }
    
    /// <summary>
    /// 零头标识（0：整托；1：零托）
    /// </summary>
    [SugarColumn(ColumnName = "OddMarking", ColumnDescription = "零头标识（0：整托；1：零托）", Length = 50)]
    public virtual string? OddMarking { get; set; }
    
    /// <summary>
    /// 客户ID
    /// </summary>
    [SugarColumn(ColumnName = "CustomerId", ColumnDescription = "客户ID", Length = 50)]
    public virtual string? CustomerId { get; set; }
    
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
    /// 重量
    /// </summary>
    [SugarColumn(ColumnName = "Weight", ColumnDescription = "重量", Length = 18, DecimalDigits=0)]
    public virtual decimal? Weight { get; set; }
    
    /// <summary>
    /// 是否挑浆：0：默认，1：挑浆
    /// </summary>
    [SugarColumn(ColumnName = "PickingSlurry", ColumnDescription = "是否挑浆：0：默认，1：挑浆", Length = 1)]
    public virtual string? PickingSlurry { get; set; }
    
 
    
}
