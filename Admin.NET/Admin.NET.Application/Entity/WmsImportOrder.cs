// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 入库流水表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsImportOrder", "入库流水表")]
public partial class WmsImportOrder : EntityBaseDel
{
    /// <summary>
    /// 流水号
    /// </summary>
    [SugarColumn(ColumnName = "ImportOrderNo", ColumnDescription = "流水号", Length = 32)]
    public virtual string? ImportOrderNo { get; set; }
    
    /// <summary>
    /// 入库单ID
    /// </summary>
    [SugarColumn(ColumnName = "ImportId", ColumnDescription = "入库单ID")]
    public virtual long? ImportId { get; set; }
    
    /// <summary>
    /// 区域ID
    /// </summary>
    [SugarColumn(ColumnName = "ImportAreaId", ColumnDescription = "区域ID")]
    public virtual long? ImportAreaId { get; set; }
    
    /// <summary>
    /// 巷道ID
    /// </summary>
    [SugarColumn(ColumnName = "ImportLanewayId", ColumnDescription = "巷道ID")]
    public virtual long? ImportLanewayId { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    [SugarColumn(ColumnName = "ImportSlotCode", ColumnDescription = "储位编码", Length = 20)]
    public virtual string? ImportSlotCode { get; set; }
    
    /// <summary>
    /// 托盘条码ID
    /// </summary>
    [SugarColumn(ColumnName = "StockCodeId", ColumnDescription = "托盘条码ID")]
    public virtual long? StockCodeId { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    [SugarColumn(ColumnName = "ImportQuantity", ColumnDescription = "数量", Length = 18, DecimalDigits=0)]
    public virtual decimal? ImportQuantity { get; set; }
    
    /// <summary>
    /// 重量
    /// </summary>
    [SugarColumn(ColumnName = "ImportWeight", ColumnDescription = "重量", Length = 18, DecimalDigits=0)]
    public virtual decimal? ImportWeight { get; set; }
    
    /// <summary>
    /// 任务ID
    /// </summary>
    [SugarColumn(ColumnName = "ImportTaskId", ColumnDescription = "任务ID")]
    public virtual long? ImportTaskId { get; set; }
    
    /// <summary>
    /// 执行标志（1待执行、2正在执行、3执行完毕、4已上传）
    /// </summary>
    [SugarColumn(ColumnName = "ImportExecuteFlag", ColumnDescription = "执行标志（1待执行、2正在执行、3执行完毕、4已上传）", Length = 2)]
    public virtual string? ImportExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark", ColumnDescription = "备注", Length = 50)]
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 班次
    /// </summary>
    [SugarColumn(ColumnName = "ImportClass", ColumnDescription = "班次", Length = 32)]
    public virtual string? ImportClass { get; set; }
    
    /// <summary>
    /// 批号
    /// </summary>
    [SugarColumn(ColumnName = "LotNo", ColumnDescription = "批号", Length = 32)]
    public virtual string? LotNo { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [SugarColumn(ColumnName = "ImportProductionDate", ColumnDescription = "生产日期")]
    public virtual DateTime? ImportProductionDate { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    [SugarColumn(ColumnName = "CompleteDate", ColumnDescription = "完成时间")]
    public virtual DateTime? CompleteDate { get; set; }
    
    /// <summary>
    /// 质检状态
    /// </summary>
    [SugarColumn(ColumnName = "InspectionStatus", ColumnDescription = "质检状态")]
    public virtual int? InspectionStatus { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    [SugarColumn(ColumnName = "WareHouseId", ColumnDescription = "仓库ID")]
    public virtual long? WareHouseId { get; set; }
    
    /// <summary>
    /// 入库单明细ID
    /// </summary>
    [SugarColumn(ColumnName = "ImportDetailId", ColumnDescription = "入库单明细ID")]
    public virtual long? ImportDetailId { get; set; }
    
    /// <summary>
    /// 主子载具号
    /// </summary>
    [SugarColumn(ColumnName = "SubVehicleCode", ColumnDescription = "主子载具号", Length = 32)]
    public virtual string? SubVehicleCode { get; set; }
    
    /// <summary>
    /// 实际称重重量
    /// </summary>
    [SugarColumn(ColumnName = "Weight", ColumnDescription = "实际称重重量", Length = 18, DecimalDigits=0)]
    public virtual decimal? Weight { get; set; }
    
    /// <summary>
    /// 0未验收 1正在验收 2验收完成
    /// </summary>
    [SugarColumn(ColumnName = "InspectFlag", ColumnDescription = "0未验收 1正在验收 2验收完成")]
    public virtual int? InspectFlag { get; set; }
    
    /// <summary>
    /// 是否暂存入库流水（不为空为是）
    /// </summary>
    [SugarColumn(ColumnName = "IsTemporaryStorage", ColumnDescription = "是否暂存入库流水（不为空为是）", Length = 255)]
    public virtual string? IsTemporaryStorage { get; set; }
    
    /// <summary>
    /// 载具号
    /// </summary>
    [SugarColumn(ColumnName = "StockCode", ColumnDescription = "载具号", Length = 50)]
    public virtual string? StockCode { get; set; }
    
    /// <summary>
    /// 验收0 挑浆1
    /// </summary>
    [SugarColumn(ColumnName = "YsOrTJ", ColumnDescription = "验收0 挑浆1", Length = 10)]
    public virtual string? YsOrTJ { get; set; }
    
}
