// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 入库单据明细
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsImportNotifyDetail", "入库单据明细")]
public partial class WmsImportNotifyDetail : EntityBaseDel
{
    /// <summary>
    /// 入库单据ID
    /// </summary>
    [SugarColumn(ColumnName = "ImportId", ColumnDescription = "入库单据ID")]
    public virtual long? ImportId { get; set; }
    
    /// <summary>
    /// 外部单据明细ID
    /// </summary>
    [SugarColumn(ColumnName = "OuterDetailId", ColumnDescription = "外部单据明细ID", Length = 32)]
    public virtual string? OuterDetailId { get; set; }
    
    /// <summary>
    /// 序号
    /// </summary>
    [SugarColumn(ColumnName = "ImportListNo", ColumnDescription = "序号", Length = 32)]
    public virtual string? ImportListNo { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>
    [SugarColumn(ColumnName = "MaterialId", ColumnDescription = "物料ID")]
    public virtual long? MaterialId { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    [SugarColumn(ColumnName = "LotNo", ColumnDescription = "批次", Length = 32)]
    public virtual string? LotNo { get; set; }
    
    /// <summary>
    /// 物料状态
    /// </summary>
    [SugarColumn(ColumnName = "MaterialStatus", ColumnDescription = "物料状态", Length = 32)]
    public virtual string? MaterialStatus { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    [SugarColumn(ColumnName = "ImportProductionDate", ColumnDescription = "生产日期")]
    public virtual DateTime? ImportProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    [SugarColumn(ColumnName = "ImportLostDate", ColumnDescription = "失效日期")]
    public virtual DateTime? ImportLostDate { get; set; }
    
    /// <summary>
    /// 计划入库数量
    /// </summary>
    [SugarColumn(ColumnName = "ImportQuantity", ColumnDescription = "计划入库数量", Length = 18, DecimalDigits=3)]
    public virtual decimal? ImportQuantity { get; set; }
    
    /// <summary>
    /// 完成数量
    /// </summary>
    [SugarColumn(ColumnName = "ImportCompleteQuantity", ColumnDescription = "完成数量", Length = 18, DecimalDigits=3)]
    public virtual decimal? ImportCompleteQuantity { get; set; }
    
    /// <summary>
    /// 组盘数量
    /// </summary>
    [SugarColumn(ColumnName = "ImportFactQuantity", ColumnDescription = "组盘数量", Length = 18, DecimalDigits=3)]
    public virtual decimal? ImportFactQuantity { get; set; }
    
    /// <summary>
    /// 已上传数量
    /// </summary>
    [SugarColumn(ColumnName = "UploadQuantity", ColumnDescription = "已上传数量", Length = 18, DecimalDigits=3)]
    public virtual decimal? UploadQuantity { get; set; }
    
    /// <summary>
    /// 执行标志（01待执行、02正在执行、03已完成、04已上传）
    /// </summary>
    [SugarColumn(ColumnName = "ImportExecuteFlag", ColumnDescription = "执行标志（01待执行、02正在执行、03已完成、04已上传）", Length = 2)]
    public virtual string? ImportExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark", ColumnDescription = "备注", Length = 50)]
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 打印标识，1打印，0不打印
    /// </summary>
    [SugarColumn(ColumnName = "PrintFlag", ColumnDescription = "打印标识，1打印，0不打印")]
    public virtual int? PrintFlag { get; set; }
    
    /// <summary>
    /// 下发任务状态默认0，1下发成功
    /// </summary>
    [SugarColumn(ColumnName = "TaskStatus", ColumnDescription = "下发任务状态默认0，1下发成功")]
    public virtual int? TaskStatus { get; set; }
    
    /// <summary>
    /// 标签判断，1已打印，2或空未打印
    /// </summary>
    [SugarColumn(ColumnName = "LabelJudgment", ColumnDescription = "标签判断，1已打印，2或空未打印")]
    public virtual int? LabelJudgment { get; set; }
    
    /// <summary>
    /// 件数
    /// </summary>
    [SugarColumn(ColumnName = "outQty", ColumnDescription = "件数", Length = 18, DecimalDigits=3)]
    public virtual decimal? outQty { get; set; }
    
    /// <summary>
    /// 巷道编码
    /// </summary>
    [SugarColumn(ColumnName = "LanewayCode", ColumnDescription = "巷道编码", Length = 32)]
    public virtual string? LanewayCode { get; set; }
    
    /// <summary>
    /// 采浆公司
    /// </summary>
    [SugarColumn(ColumnName = "Xj_HouseCode", ColumnDescription = "采浆公司", Length = 50)]
    public virtual string? Xj_HouseCode { get; set; }
    
    /// <summary>
    /// 血浆件数
    /// </summary>
    [SugarColumn(ColumnName = "Xj_Qty", ColumnDescription = "血浆件数", Length = 18, DecimalDigits=3)]
    public virtual decimal? Xj_Qty { get; set; }
    
    /// <summary>
    /// 血浆重量
    /// </summary>
    [SugarColumn(ColumnName = "Xj_Weight", ColumnDescription = "血浆重量", Length = 18, DecimalDigits=3)]
    public virtual decimal? Xj_Weight { get; set; }
    
    /// <summary>
    /// 血浆 箱总数
    /// </summary>
    [SugarColumn(ColumnName = "Xj_BoxQty", ColumnDescription = "血浆 箱总数", Length = 18, DecimalDigits=3)]
    public virtual decimal? Xj_BoxQty { get; set; }
    
    /// <summary>
    /// 血浆类型
    /// </summary>
    [SugarColumn(ColumnName = "Xj_GoodCode", ColumnDescription = "血浆类型")]
    public virtual long? Xj_GoodCode { get; set; }
    
    /// <summary>
    /// 单据类型（1:入库；2：取消入库）
    /// </summary>
    [SugarColumn(ColumnName = "Xj_Type", ColumnDescription = "单据类型（1:入库；2：取消入库）", DefaultValue = "('1')")]
    public virtual int? Xj_Type { get; set; }
    
    /// <summary>
    /// 已验证数量
    /// </summary>
    [SugarColumn(ColumnName = "VerifiedQty", ColumnDescription = "已验证数量", Length = 18, DecimalDigits=3)]
    public virtual decimal? VerifiedQty { get; set; }
    
    /// <summary>
    /// 千克数量
    /// </summary>
    [SugarColumn(ColumnName = "KilogramQty", ColumnDescription = "千克数量", Length = 18, DecimalDigits=3)]
    public virtual decimal? KilogramQty { get; set; }
    /// <summary>
    /// 下发任务状态
    /// </summary>
    [SugarColumn(ColumnName = "ImportTaskStatus", ColumnDescription = "下发任务状态")]
    public virtual int? ImportTaskStatus { get; set; }
    /// <summary>
    /// 皮重
    /// </summary>
    [SugarColumn(ColumnName = "NetWeight", ColumnDescription = "皮重", Length = 255)]
    public virtual string? NetWeight { get; set; }

    /// <summary>
    /// 码垛线显示，1号线，2号线
    /// </summary>
    [SugarColumn(ColumnName = "ReceivingDock", ColumnDescription = "码垛线显示，1号线，2号线", Length = 255)]
    public virtual string? ReceivingDock { get; set; }
}
