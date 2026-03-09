// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using System.ComponentModel.DataAnnotations;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

namespace Admin.NET.Application;

public class WmsImportLabelPrintInput 
{
    public string? LabelID { get; set; } = "";
    public long? ImportDetailId { get; set; }
    public string? BoxCode { get; set; } = "";
   
    public string? Isprint { get; set; } = "0";
    public int? Number { get; set; } = 1;
}
 
/// <summary>
/// 入库标签打印基础输入参数
/// </summary>
public class WmsImportLabelPrintBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 入库单Id
    /// </summary>
    public virtual long? ImportId { get; set; }
    
    /// <summary>
    /// 标签序号
    /// </summary>
    public virtual string? LabelStream { get; set; }
    
    /// <summary>
    /// 物料编号
    /// </summary>
    public virtual string? MaterialCode { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    public virtual string? MaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    public virtual string? MaterialStandard { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    public virtual string? LotNo { get; set; }
    
    /// <summary>
    /// 是否使用
    /// </summary>
    public virtual int? IsUse { get; set; }
    
    /// <summary>
    /// 箱编码
    /// </summary>
    public virtual string? BoxCode { get; set; }
    
    /// <summary>
    /// 箱数量
    /// </summary>
    public virtual decimal? Quantity { get; set; }
    
    /// <summary>
    /// 满箱标识
    /// </summary>
    public virtual int? MxFlag { get; set; }
    
}

/// <summary>
/// 入库标签打印分页查询输入参数
/// </summary>
public class PageWmsImportLabelPrintInput : BasePageInput
{
    /// <summary>
    /// 入库单Id
    /// </summary>
    public long? ImportId { get; set; }
    /// <summary>
    /// 入库单明细Id
    /// </summary>
    public long? ImportDetailId { get; set; }

    /// <summary>
    /// 标签序号
    /// </summary>
    public string? LabelStream { get; set; }
    
    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterialCode { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    public string? MaterialStandard { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 是否使用
    /// </summary>
    public int? IsUse { get; set; }
    
    /// <summary>
    /// 箱编码
    /// </summary>
    public string? BoxCode { get; set; }
    
    /// <summary>
    /// 箱数量
    /// </summary>
    public decimal? Quantity { get; set; }
    
    /// <summary>
    /// 满箱标识
    /// </summary>
    public int? MxFlag { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 入库标签打印增加输入参数
/// </summary>
public class AddWmsImportLabelPrintInput
{
    /// <summary>
    /// 入库单Id
    /// </summary>
    public long? ImportId { get; set; }
    
    /// <summary>
    /// 标签序号
    /// </summary>
    [MaxLength(32, ErrorMessage = "标签序号字符长度不能超过32")]
    public string? LabelStream { get; set; }
    
    /// <summary>
    /// 物料编号
    /// </summary>
    [MaxLength(32, ErrorMessage = "物料编号字符长度不能超过32")]
    public string? MaterialCode { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    [MaxLength(32, ErrorMessage = "物料名称字符长度不能超过32")]
    public string? MaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    [MaxLength(32, ErrorMessage = "物料规格字符长度不能超过32")]
    public string? MaterialStandard { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    [MaxLength(32, ErrorMessage = "批次字符长度不能超过32")]
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 是否使用
    /// </summary>
    public int? IsUse { get; set; }
    
    /// <summary>
    /// 箱编码
    /// </summary>
    [MaxLength(32, ErrorMessage = "箱编码字符长度不能超过32")]
    public string? BoxCode { get; set; }
    
    /// <summary>
    /// 箱数量
    /// </summary>
    public decimal? Quantity { get; set; }
    
    /// <summary>
    /// 满箱标识
    /// </summary>
    public int? MxFlag { get; set; }
    
}

/// <summary>
/// 入库标签打印删除输入参数
/// </summary>
public class DeleteWmsImportLabelPrintInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}
/// <summary>
/// 入库标签打印删除输入参数
/// </summary>
public class BathDeleteWmsImportLabelPrintInput
{
    public long? ImportDetailId { get; set; }

}
/// <summary>
/// 入库标签打印更新输入参数
/// </summary>
public class UpdateWmsImportLabelPrintInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 入库单Id
    /// </summary>    
    public long? ImportId { get; set; }
    
    /// <summary>
    /// 标签序号
    /// </summary>    
    [MaxLength(32, ErrorMessage = "标签序号字符长度不能超过32")]
    public string? LabelStream { get; set; }
    
    /// <summary>
    /// 物料编号
    /// </summary>    
    [MaxLength(32, ErrorMessage = "物料编号字符长度不能超过32")]
    public string? MaterialCode { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>    
    [MaxLength(32, ErrorMessage = "物料名称字符长度不能超过32")]
    public string? MaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>    
    [MaxLength(32, ErrorMessage = "物料规格字符长度不能超过32")]
    public string? MaterialStandard { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>    
    [MaxLength(32, ErrorMessage = "批次字符长度不能超过32")]
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 是否使用
    /// </summary>    
    public int? IsUse { get; set; }
    
    /// <summary>
    /// 箱编码
    /// </summary>    
    [MaxLength(32, ErrorMessage = "箱编码字符长度不能超过32")]
    public string? BoxCode { get; set; }
    
    /// <summary>
    /// 箱数量
    /// </summary>    
    public decimal? Quantity { get; set; }
    
    /// <summary>
    /// 满箱标识
    /// </summary>    
    public int? MxFlag { get; set; }
    
}

/// <summary>
/// 入库标签打印主键查询输入参数
/// </summary>
public class QueryByIdWmsImportLabelPrintInput : DeleteWmsImportLabelPrintInput
{
}

/// <summary>
/// 入库标签打印数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsImportLabelPrintInput : BaseImportInput
{
    /// <summary>
    /// 入库单Id
    /// </summary>
    [ImporterHeader(Name = "入库单Id")]
    [ExporterHeader("入库单Id", Format = "", Width = 25, IsBold = true)]
    public long? ImportId { get; set; }
    
    /// <summary>
    /// 标签序号
    /// </summary>
    [ImporterHeader(Name = "标签序号")]
    [ExporterHeader("标签序号", Format = "", Width = 25, IsBold = true)]
    public string? LabelStream { get; set; }
    
    /// <summary>
    /// 物料编号
    /// </summary>
    [ImporterHeader(Name = "物料编号")]
    [ExporterHeader("物料编号", Format = "", Width = 25, IsBold = true)]
    public string? MaterialCode { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    [ImporterHeader(Name = "物料名称")]
    [ExporterHeader("物料名称", Format = "", Width = 25, IsBold = true)]
    public string? MaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    [ImporterHeader(Name = "物料规格")]
    [ExporterHeader("物料规格", Format = "", Width = 25, IsBold = true)]
    public string? MaterialStandard { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    [ImporterHeader(Name = "批次")]
    [ExporterHeader("批次", Format = "", Width = 25, IsBold = true)]
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 是否使用
    /// </summary>
    [ImporterHeader(Name = "是否使用")]
    [ExporterHeader("是否使用", Format = "", Width = 25, IsBold = true)]
    public int? IsUse { get; set; }
    
    /// <summary>
    /// 箱编码
    /// </summary>
    [ImporterHeader(Name = "箱编码")]
    [ExporterHeader("箱编码", Format = "", Width = 25, IsBold = true)]
    public string? BoxCode { get; set; }
    
    /// <summary>
    /// 箱数量
    /// </summary>
    [ImporterHeader(Name = "箱数量")]
    [ExporterHeader("箱数量", Format = "", Width = 25, IsBold = true)]
    public decimal? Quantity { get; set; }
    
    /// <summary>
    /// 满箱标识
    /// </summary>
    [ImporterHeader(Name = "满箱标识")]
    [ExporterHeader("满箱标识", Format = "", Width = 25, IsBold = true)]
    public int? MxFlag { get; set; }
    
}
