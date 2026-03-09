// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 入库标签打印
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsImportLabelPrint", "入库标签打印")]
public partial class WmsImportLabelPrint : EntityBaseDel
{

    /// <summary>
    /// 入库单Id
    /// </summary>
    [SugarColumn(ColumnName = "ImportId", ColumnDescription = "入库单Id")]
    public virtual long? ImportId { get; set; }
    /// <summary>
    /// 入库单号
    /// </summary>
    [SugarColumn(ColumnName = "ImportBillCode", ColumnDescription = "入库单号", Length = 200)]
    public virtual string? ImportBillCode { get; set; }
    
    /// <summary>
    /// 入库明细Id
    /// </summary>
    [SugarColumn(ColumnName = "ImportDetailId", ColumnDescription = "入库明细Id")]
    public virtual long? ImportDetailId { get; set; }
    
    /// <summary>
    /// 条码
    /// </summary>
    [SugarColumn(ColumnName = "LabelID", ColumnDescription = "条码", Length = 200)]
    public virtual string? LabelID { get; set; }
    /// <summary>
    /// 制造商
    /// </summary>
    [SugarColumn(ColumnName = "ImportCustomer", ColumnDescription = "制造商", Length = 200)]
    public virtual string? ImportCustomer { get; set; }
    /// <summary>
    /// 标签序号
    /// </summary>
    [SugarColumn(ColumnName = "LabelStream", ColumnDescription = "标签序号", Length = 200)]
    public virtual string? LabelStream { get; set; }
    
    /// <summary>
    /// 物料编号
    /// </summary>
    [SugarColumn(ColumnName = "MaterialCode", ColumnDescription = "物料编号", Length = 200)]
    public virtual string? MaterialCode { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    [SugarColumn(ColumnName = "MaterialName", ColumnDescription = "物料名称", Length = 200)]
    public virtual string? MaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    [SugarColumn(ColumnName = "MaterialStandard", ColumnDescription = "物料规格", Length = 200)]
    public virtual string? MaterialStandard { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    [SugarColumn(ColumnName = "LotNo", ColumnDescription = "批次", Length = 200)]
    public virtual string? LotNo { get; set; }
    
    /// <summary>
    /// 是否使用
    /// </summary>
    [SugarColumn(ColumnName = "IsUse", ColumnDescription = "是否使用")]
    public virtual int? IsUse { get; set; }
    
    /// <summary>
    /// 箱编码
    /// </summary>
    [SugarColumn(ColumnName = "BoxCode", ColumnDescription = "箱编码", Length = 200)]
    public virtual string? BoxCode { get; set; }
    
    /// <summary>
    /// 箱数量
    /// </summary>
    [SugarColumn(ColumnName = "Quantity", ColumnDescription = "箱数量", Length = 200, DecimalDigits=0)]
    public virtual decimal? Quantity { get; set; }
    
    /// <summary>
    /// 满箱标识
    /// </summary>
    [SugarColumn(ColumnName = "MxFlag", ColumnDescription = "满箱标识")]
    public virtual int? MxFlag { get; set; }
    
}
