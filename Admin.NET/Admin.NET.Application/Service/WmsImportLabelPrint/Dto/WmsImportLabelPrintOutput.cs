// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using Magicodes.ExporterAndImporter.Core;
namespace Admin.NET.Application;

/// <summary>
/// 入库标签打印输出参数
/// </summary>
public class WmsImportLabelPrintOutput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }    
    public int? Number { get; set; }
    /// <summary>
    /// 入库单Id
    /// </summary>
    public long? ImportId { get; set; }
    /// <summary>
    /// 入库明细Id
    /// </summary>
    public long? ImportDetailId { get; set; }
    /// <summary>
    /// 条码
    /// </summary>
    public string? LabelID { get; set; }
    /// <summary>
    /// 制造商
    /// </summary>
    public string? ImportCustomer { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? ImportBillCode { get; set; }
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
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }    
    
    /// <summary>
    /// 修改者Id
    /// </summary>
    public long? UpdateUserId { get; set; }    
    
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }    
    /// <summary>
    /// 制造商
    /// </summary>
    public string? ManufacturerName { get; set; }
    /// <summary>
    /// 生产日期
    /// </summary>
    public string ImportProductionDate { get; set; }
    /// <summary>
    /// 有效期至
    /// </summary>
    public string ImportLostDate { get; set; }


}

/// <summary>
/// 入库标签打印数据导入模板实体
/// </summary>
public class ExportWmsImportLabelPrintOutput : ImportWmsImportLabelPrintInput
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public override string Error { get; set; }
}
