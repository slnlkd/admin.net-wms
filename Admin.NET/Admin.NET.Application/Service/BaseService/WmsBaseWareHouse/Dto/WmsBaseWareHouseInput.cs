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
/// 仓库表基础输入参数
/// </summary>
public class WmsBaseWareHouseBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 仓库编码
    /// </summary>
    public virtual string? WarehouseCode { get; set; }
    
    /// <summary>
    /// 仓库名称
    /// </summary>
    public virtual string? WarehouseName { get; set; }
    
    /// <summary>
    /// 仓库类型
    /// </summary>
    public virtual string? WarehouseType { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 是否允许超出
    /// </summary>
    public virtual bool? IsExceeding { get; set; }
    
    /// <summary>
    /// 是否允许超入
    /// </summary>
    public virtual bool? IsOverbooking { get; set; }
    
    /// <summary>
    /// 是否允许少出
    /// </summary>
    public virtual bool? IsUnderpay { get; set; }
    
    /// <summary>
    /// 是否允许少入
    /// </summary>
    public virtual bool? IsEnterless { get; set; }
    
}

/// <summary>
/// 仓库表分页查询输入参数
/// </summary>
public class PageWmsBaseWareHouseInput : BasePageInput
{
    /// <summary>
    /// 仓库编码
    /// </summary>
    public string? WarehouseCode { get; set; }
    
    /// <summary>
    /// 仓库名称
    /// </summary>
    public string? WarehouseName { get; set; }
    
    /// <summary>
    /// 仓库类型
    /// </summary>
    public string? WarehouseType { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
    
    /// <summary>
    /// 是否允许超出
    /// </summary>
    public bool? IsExceeding { get; set; }
    
    /// <summary>
    /// 是否允许超入
    /// </summary>
    public bool? IsOverbooking { get; set; }
    
    /// <summary>
    /// 是否允许少出
    /// </summary>
    public bool? IsUnderpay { get; set; }
    
    /// <summary>
    /// 是否允许少入
    /// </summary>
    public bool? IsEnterless { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 仓库表增加输入参数
/// </summary>
public class AddWmsBaseWareHouseInput
{
    /// <summary>
    /// 仓库编码
    /// </summary>
    [MaxLength(32, ErrorMessage = "仓库编码字符长度不能超过32")]
    public string? WarehouseCode { get; set; }
    
    /// <summary>
    /// 仓库名称
    /// </summary>
    [MaxLength(32, ErrorMessage = "仓库名称字符长度不能超过32")]
    public string? WarehouseName { get; set; }
    
    /// <summary>
    /// 仓库类型
    /// </summary>
    [MaxLength(32, ErrorMessage = "仓库类型字符长度不能超过32")]
    public string? WarehouseType { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(32, ErrorMessage = "备注字符长度不能超过32")]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 是否允许超出
    /// </summary>
    public bool? IsExceeding { get; set; }
    
    /// <summary>
    /// 是否允许超入
    /// </summary>
    public bool? IsOverbooking { get; set; }
    
    /// <summary>
    /// 是否允许少出
    /// </summary>
    public bool? IsUnderpay { get; set; }
    
    /// <summary>
    /// 是否允许少入
    /// </summary>
    public bool? IsEnterless { get; set; }
    
}

/// <summary>
/// 仓库表删除输入参数
/// </summary>
public class DeleteWmsBaseWareHouseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 仓库表更新输入参数
/// </summary>
public class UpdateWmsBaseWareHouseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 仓库编码
    /// </summary>    
    [MaxLength(32, ErrorMessage = "仓库编码字符长度不能超过32")]
    public string? WarehouseCode { get; set; }
    
    /// <summary>
    /// 仓库名称
    /// </summary>    
    [MaxLength(32, ErrorMessage = "仓库名称字符长度不能超过32")]
    public string? WarehouseName { get; set; }
    
    /// <summary>
    /// 仓库类型
    /// </summary>    
    [MaxLength(32, ErrorMessage = "仓库类型字符长度不能超过32")]
    public string? WarehouseType { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>    
    [MaxLength(32, ErrorMessage = "备注字符长度不能超过32")]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 是否允许超出
    /// </summary>    
    public bool? IsExceeding { get; set; }
    
    /// <summary>
    /// 是否允许超入
    /// </summary>    
    public bool? IsOverbooking { get; set; }
    
    /// <summary>
    /// 是否允许少出
    /// </summary>    
    public bool? IsUnderpay { get; set; }
    
    /// <summary>
    /// 是否允许少入
    /// </summary>    
    public bool? IsEnterless { get; set; }
    
}

/// <summary>
/// 仓库表主键查询输入参数
/// </summary>
public class QueryByIdWmsBaseWareHouseInput : DeleteWmsBaseWareHouseInput
{
}

/// <summary>
/// 仓库表数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsBaseWareHouseInput : BaseImportInput
{
    /// <summary>
    /// 仓库编码
    /// </summary>
    [ImporterHeader(Name = "仓库编码")]
    [ExporterHeader("仓库编码", Format = "", Width = 25, IsBold = true)]
    public string? WarehouseCode { get; set; }
    
    /// <summary>
    /// 仓库名称
    /// </summary>
    [ImporterHeader(Name = "仓库名称")]
    [ExporterHeader("仓库名称", Format = "", Width = 25, IsBold = true)]
    public string? WarehouseName { get; set; }
    
    /// <summary>
    /// 仓库类型
    /// </summary>
    [ImporterHeader(Name = "仓库类型")]
    [ExporterHeader("仓库类型", Format = "", Width = 25, IsBold = true)]
    public string? WarehouseType { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 是否允许超出
    /// </summary>
    [ImporterHeader(Name = "是否允许超出")]
    [ExporterHeader("是否允许超出", Format = "", Width = 25, IsBold = true)]
    public bool? IsExceeding { get; set; }
    
    /// <summary>
    /// 是否允许超入
    /// </summary>
    [ImporterHeader(Name = "是否允许超入")]
    [ExporterHeader("是否允许超入", Format = "", Width = 25, IsBold = true)]
    public bool? IsOverbooking { get; set; }
    
    /// <summary>
    /// 是否允许少出
    /// </summary>
    [ImporterHeader(Name = "是否允许少出")]
    [ExporterHeader("是否允许少出", Format = "", Width = 25, IsBold = true)]
    public bool? IsUnderpay { get; set; }
    
    /// <summary>
    /// 是否允许少入
    /// </summary>
    [ImporterHeader(Name = "是否允许少入")]
    [ExporterHeader("是否允许少入", Format = "", Width = 25, IsBold = true)]
    public bool? IsEnterless { get; set; }
    
}
