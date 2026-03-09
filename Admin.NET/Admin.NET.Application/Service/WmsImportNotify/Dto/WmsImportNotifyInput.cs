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

/// <summary>
/// 入库单基础输入参数
/// </summary>
public class WmsImportNotifyBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    public virtual long? WarehouseId { get; set; }
    
    /// <summary>
    /// 入库单号
    /// </summary>
    public virtual string? ImportBillCode { get; set; }
    
    /// <summary>
    /// 执行状态
    /// </summary>
    public virtual string? ImportExecuteFlag { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    [Dict("BillType", AllowNullValue=true)]
    public virtual long? BillType { get; set; }
    
    /// <summary>
    /// 部门ID
    /// </summary>
    public virtual long? DepartmentId { get; set; }
    
    /// <summary>
    /// 供货单位
    /// </summary>
    public virtual long? SupplierId { get; set; }
    
    /// <summary>
    /// 客户单位
    /// </summary>
    public virtual long? CustomerId { get; set; }

    /// <summary>
    /// 制造商单位
    /// </summary>
    public virtual long? ManufacturerId { get; set; }

    /// <summary>
    /// 来源
    /// </summary>
    public virtual string? Source { get; set; }
    
    /// <summary>
    /// 外部单号
    /// </summary>
    public virtual string? OuterBillCode { get; set; }
    
    /// <summary>
    /// 外部单据ID
    /// </summary>
    public virtual string? OuterMainId { get; set; }
    
    /// <summary>
    /// 生产商ID
    /// </summary>
    public virtual long? ProduceId { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remark { get; set; }
    
}

/// <summary>
/// 入库单分页查询输入参数
/// </summary>
public class PageWmsImportNotifyInput : BasePageInput
{
    /// <summary>
    /// 入库单号
    /// </summary>
    public string? ImportBillCode { get; set; }
    
    /// <summary>
    /// 执行状态
    /// </summary>
    public string? ImportExecuteFlag { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    [Dict("BillType", AllowNullValue=true)]
    public long? BillType { get; set; }
    
    /// <summary>
    /// 创建时间范围
    /// </summary>
     public DateTime?[] CreateTimeRange { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 入库单增加输入参数
/// </summary>
public class AddWmsImportNotifyInput
{
    /// <summary>
    /// 所属仓库
    /// </summary>
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 入库单号
    /// </summary>
    [MaxLength(32, ErrorMessage = "入库单号字符长度不能超过32")]
    public string? ImportBillCode { get; set; }
    
    /// <summary>
    /// 执行状态
    /// </summary>
    [MaxLength(2, ErrorMessage = "执行状态字符长度不能超过2")]
    public string? ImportExecuteFlag { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    public long? BillType { get; set; }
    
    /// <summary>
    /// 部门ID
    /// </summary>
    public long? DepartmentId { get; set; }
    
    /// <summary>
    /// 供货单位
    /// </summary>
    public long? SupplierId { get; set; }
    
    /// <summary>
    /// 客户单位
    /// </summary>
    public long? CustomerId { get; set; }

    /// <summary>
    /// 制造商单位
    /// </summary>
    public long? ManufacturerId { get; set; }
    /// <summary>
    /// 来源
    /// </summary>
    [MaxLength(50, ErrorMessage = "来源字符长度不能超过50")]
    public string? Source { get; set; }
    
    /// <summary>
    /// 外部单号
    /// </summary>
    [MaxLength(32, ErrorMessage = "外部单号字符长度不能超过32")]
    public string? OuterBillCode { get; set; }
    
    /// <summary>
    /// 外部单据ID
    /// </summary>
    [MaxLength(32, ErrorMessage = "外部单据ID字符长度不能超过32")]
    public string? OuterMainId { get; set; }
    
    /// <summary>
    /// 生产商ID
    /// </summary>
    public long? ProduceId { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(50, ErrorMessage = "备注字符长度不能超过50")]
    public string? Remark { get; set; }
    /// <summary>
    /// 入库明细
    /// </summary>
    public List<AddWmsImportNotifyDetailInput> DetailList { get; set; } = new List<AddWmsImportNotifyDetailInput>();
    
}

/// <summary>
/// 入库单删除输入参数
/// </summary>
public class DeleteWmsImportNotifyInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}
public class OperationWmsImportNotifyInput
{
    /// <summary>
    /// 入库单Id
    /// </summary>
    [Required(ErrorMessage = "入库单Id不能为空")]
    public long ImportId { get; set; }
}

/// <summary>
/// 入库单更新输入参数
/// </summary>
public class UpdateWmsImportNotifyInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>    
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 入库单号
    /// </summary>    
    [MaxLength(32, ErrorMessage = "入库单号字符长度不能超过32")]
    public string? ImportBillCode { get; set; }
    
    /// <summary>
    /// 执行状态
    /// </summary>    
    [MaxLength(2, ErrorMessage = "执行状态字符长度不能超过2")]
    public string? ImportExecuteFlag { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>    
    public long? BillType { get; set; }
    
    /// <summary>
    /// 部门ID
    /// </summary>    
    public long? DepartmentId { get; set; }
    
    /// <summary>
    /// 供货单位
    /// </summary>    
    public long? SupplierId { get; set; }
    
    /// <summary>
    /// 客户单位
    /// </summary>    
    public long? CustomerId { get; set; }

    /// <summary>
    /// 制造商单位
    /// </summary>
    public long? ManufacturerId { get; set; }

    /// <summary>
    /// 来源
    /// </summary>    
    [MaxLength(50, ErrorMessage = "来源字符长度不能超过50")]
    public string? Source { get; set; }
    
    /// <summary>
    /// 外部单号
    /// </summary>    
    [MaxLength(32, ErrorMessage = "外部单号字符长度不能超过32")]
    public string? OuterBillCode { get; set; }
    
    /// <summary>
    /// 外部单据ID
    /// </summary>    
    [MaxLength(32, ErrorMessage = "外部单据ID字符长度不能超过32")]
    public string? OuterMainId { get; set; }

    /// <summary>
    /// 生产商ID
    /// </summary>
    public long? ProduceId { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>    
    [MaxLength(50, ErrorMessage = "备注字符长度不能超过50")]
    public string? Remark { get; set; }

    /// <summary>
    /// 入库明细
    /// </summary>
    public List<UpdateWmsImportNotifyDetailInput> DetailList { get; set; } = new List<UpdateWmsImportNotifyDetailInput>();

}

/// <summary>
/// 入库单主键查询输入参数
/// </summary>
public class QueryByIdWmsImportNotifyInput : DeleteWmsImportNotifyInput
{
}

/// <summary>
/// 下拉数据输入参数
/// </summary>
public class DropdownDataWmsImportNotifyInput
{
    /// <summary>
    /// 是否用于分页查询
    /// </summary>
    public bool FromPage { get; set; }
}

/// <summary>
/// 入库单数据导出实体
/// </summary>
public class ExportWmsImportNotifyDto
{
    /// <summary>
    /// 入库单号 用于主子表关联
    /// </summary>
    [ExporterHeader("入库单号", Format = "", Width = 25, IsBold = true)]
    public string? IdentifyCode { get; set; }

    /// <summary>
    /// 所属仓库
    /// </summary>
    [ExporterHeader("所属仓库", Format = "", Width = 25, IsBold = true)]
    public string WarehouseFkDisplayName { get; set; }

    /// <summary>
    /// 单据类型
    /// </summary>
    [ExporterHeader("单据类型", Format = "", Width = 25, IsBold = true)]
    public string BillTypeName { get; set; }

    /// <summary>
    /// 供货单位
    /// </summary>
    [ExporterHeader("供货单位", Format = "", Width = 25, IsBold = true)]
    public string SupplierFkDisplayName { get; set; }

    /// <summary>
    /// 制造商单位
    /// </summary>
    [ExporterHeader("制造商单位", Format = "", Width = 25, IsBold = true)]
    public string ManufacturerFkDisplayName { get; set; }

    /// <summary>
    /// 来源
    /// </summary>
    [ExporterHeader("来源", Format = "", Width = 25, IsBold = true)]
    public string? Source { get; set; }

    /// <summary>
    /// 外部单号
    /// </summary>
    [ExporterHeader("外部单号", Format = "", Width = 25, IsBold = true)]
    public string? OuterBillCode { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? Remark { get; set; }
}

/// <summary>
/// 入库单数据导入实体
/// </summary>
/// <remarks>
/// SheetIndex = 0 表示第一个sheet（主表和明细表横向合并在一起）
/// 模板结构：Sheet 0=入库单导入模板（第1行：主表表头|空列|明细表表头，第2行：主表数据|空列|明细表数据），Sheet 1=下拉数据
/// 主表数据从第1行开始（第1行是表头，第2行开始是数据，横向布局）
/// </remarks>
[ExcelImporter(SheetIndex = 0, IsOnlyErrorRows = true)]
public class ImportWmsImportNotifyInput : BaseImportInput
{
    /// <summary>
    /// 标识列 用于主子表关联
    /// </summary>
    [ImporterHeader(Name = "标识列")]
    [ExporterHeader("标识列", Format = "", Width = 25, IsBold = true)]
    public string? IdentifyCode { get; set; }
    
    /// <summary>
    /// 所属仓库 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 所属仓库 文本（下拉框选择）
    /// </summary>
    [ImporterHeader(Name = "所属仓库")]
    [ExporterHeader("所属仓库", Format = "", Width = 25, IsBold = true)]
    public string WarehouseFkDisplayName { get; set; }
    
    /// <summary>
    /// 入库单号（后台自动生成，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? ImportBillCode { get; set; }
    
    /// <summary>
    /// 执行状态（后台自动赋值"01"，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? ImportExecuteFlag { get; set; }
    
    /// <summary>
    /// 单据类型 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? BillType { get; set; }
    
    /// <summary>
    /// 单据类型 文本（单据类型名称，下拉框选择）
    /// </summary>
    [ImporterHeader(Name = "单据类型")]
    [ExporterHeader("单据类型", Format = "", Width = 25, IsBold = true)]
    public string BillTypeName { get; set; }
    
    /// <summary>
    /// 部门ID（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? DepartmentId { get; set; }
    
    /// <summary>
    /// 供货单位 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? SupplierId { get; set; }
    
    /// <summary>
    /// 供货单位 文本（下拉框选择）
    /// </summary>
    [ImporterHeader(Name = "供货单位")]
    [ExporterHeader("供货单位", Format = "", Width = 25, IsBold = true)]
    public string SupplierFkDisplayName { get; set; }
    
    /// <summary>
    /// 客户单位 关联值（已去掉，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? CustomerId { get; set; }
    
    /// <summary>
    /// 制造商单位 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? ManufacturerId { get; set; }
    
    /// <summary>
    /// 制造商单位 文本（下拉框选择）
    /// </summary>
    [ImporterHeader(Name = "制造商单位")]
    [ExporterHeader("制造商单位", Format = "", Width = 25, IsBold = true)]
    public string ManufacturerFkDisplayName { get; set; }
    
    /// <summary>
    /// 来源
    /// </summary>
    [ImporterHeader(Name = "来源")]
    [ExporterHeader("来源", Format = "", Width = 25, IsBold = true)]
    public string? Source { get; set; }
    
    /// <summary>
    /// 外部单号
    /// </summary>
    [ImporterHeader(Name = "外部单号")]
    [ExporterHeader("外部单号", Format = "", Width = 25, IsBold = true)]
    public string? OuterBillCode { get; set; }
    
    /// <summary>
    /// 外部单据ID（暂不处理）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? OuterMainId { get; set; }
    
    /// <summary>
    /// 生产商ID（暂不处理）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? ProduceId { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? Remark { get; set; }
    
}
