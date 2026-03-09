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
/// 物料信息基础输入参数
/// </summary>
public class WmsBaseMaterialBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 物料编码
    /// </summary>
    [Required(ErrorMessage = "物料编码不能为空")]
    public virtual string? MaterialCode { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    [Dict("MaterialType", AllowNullValue=true)]
    [Required(ErrorMessage = "物料类型不能为空")]
    public virtual long? MaterialType { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    [Required(ErrorMessage = "物料名称不能为空")]
    public virtual string? MaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    public virtual string? MaterialStandard { get; set; }
    
    /// <summary>
    /// 计量单位
    /// </summary>
    public virtual long? MaterialUnit { get; set; }
    
    /// <summary>
    /// 专属区域
    /// </summary>
    public virtual string? MaterialAreaId { get; set; }
    
    /// <summary>
    /// 箱数量
    /// </summary>
    [Required(ErrorMessage = "箱数量不能为空")]
    public virtual string? BoxQuantity { get; set; }
    
    /// <summary>
    /// 物料来源
    /// </summary>
    public virtual string? MaterialOrigin { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 每件托数
    /// </summary>
    [Required(ErrorMessage = "每件托数不能为空")]
    public virtual string? EveryNumber { get; set; }
    
    /// <summary>
    /// 载具
    /// </summary>
    [Dict("Vehicle", AllowNullValue=true)]
    [Required(ErrorMessage = "载具不能为空")]
    public virtual string? Vehicle { get; set; }
    
    /// <summary>
    /// 物料重量
    /// </summary>
    public virtual decimal? MaterialWeight { get; set; }
    
    /// <summary>
    /// 最高库存数量
    /// </summary>
    public virtual string? MaterialStockHigh { get; set; }
    
    /// <summary>
    /// 最低库存数量
    /// </summary>
    public virtual string? MaterialStockLow { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    [Required(ErrorMessage = "所属仓库不能为空")]
    public virtual string? WarehouseId { get; set; }
    
    /// <summary>
    /// 提前预警天数
    /// </summary>
    public virtual string? MaterialAlarmDay { get; set; }
    
    /// <summary>
    /// 贴标
    /// </summary>
    [Dict("Labeling", AllowNullValue=true)]
    [Required(ErrorMessage = "贴标不能为空")]
    public virtual string? Labeling { get; set; }
    
    /// <summary>
    /// 管理方式
    /// </summary>
    [Dict("ManageType", AllowNullValue=true)]
    public virtual string? ManageType { get; set; }
    
    /// <summary>
    /// 最低储备
    /// </summary>
    public virtual string? MixReserve { get; set; }
    
    /// <summary>
    /// 最高储备
    /// </summary>
    public virtual string? MaxReserve { get; set; }
    
    /// <summary>
    /// 提前报警天数
    /// </summary>
    public virtual int? AlarmDay { get; set; }
    
    /// <summary>
    /// 是否空托
    /// </summary>
    public virtual bool? IsEmpty { get; set; }
    
}

/// <summary>
/// 物料信息分页查询输入参数
/// </summary>
public class PageWmsBaseMaterialInput : BasePageInput
{
    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    [Dict("MaterialType", AllowNullValue=true)]
    public long? MaterialType { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterialName { get; set; }
    
    /// <summary>
    /// 物料助记码
    /// </summary>
    public string? MaterialMcode { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    public string? MaterialStandard { get; set; }
    
    /// <summary>
    /// 计量单位
    /// </summary>
    public long? MaterialUnit { get; set; }
    
    /// <summary>
    /// 专属区域
    /// </summary>
    public string? MaterialAreaId { get; set; }
    
    /// <summary>
    /// 箱数量
    /// </summary>
    public string? BoxQuantity { get; set; }
    
    /// <summary>
    /// 物料来源
    /// </summary>
    public string? MaterialOrigin { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
    
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }
    
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
    
    /// <summary>
    /// 物料型号
    /// </summary>
    public string? MaterialModel { get; set; }
    
    /// <summary>
    /// 每件托数
    /// </summary>
    public string? EveryNumber { get; set; }
    
    /// <summary>
    /// 载具
    /// </summary>
    [Dict("Vehicle", AllowNullValue=true)]
    public string? Vehicle { get; set; }
    
    /// <summary>
    /// 物料重量
    /// </summary>
    public decimal? MaterialWeight { get; set; }
    
    /// <summary>
    /// 有效期1
    /// </summary>
    public string? MaterialValidityDay1 { get; set; }
    
    /// <summary>
    /// 有效期2
    /// </summary>
    public string? MaterialValidityDay2 { get; set; }
    
    /// <summary>
    /// 有效期3
    /// </summary>
    public string? MaterialValidityDay3 { get; set; }
    
    /// <summary>
    /// 温度
    /// </summary>
    public string? MaterialTemp { get; set; }
    
    /// <summary>
    /// 最高库存数量
    /// </summary>
    public string? MaterialStockHigh { get; set; }
    
    /// <summary>
    /// 最低库存数量
    /// </summary>
    public string? MaterialStockLow { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    public string? WarehouseId { get; set; }
    
    /// <summary>
    /// 提前预警天数
    /// </summary>
    public string? MaterialAlarmDay { get; set; }
    
    /// <summary>
    /// 贴标
    /// </summary>
    [Dict("Labeling", AllowNullValue=true)]
    public string? Labeling { get; set; }
    
    /// <summary>
    /// 管理方式
    /// </summary>
    [Dict("ManageType", AllowNullValue=true)]
    public string? ManageType { get; set; }
    
    /// <summary>
    /// 外部物品编码
    /// </summary>
    public string? OuterInnerCode { get; set; }
    
    /// <summary>
    /// 最低储备
    /// </summary>
    public string? MixReserve { get; set; }
    
    /// <summary>
    /// 最高储备
    /// </summary>
    public string? MaxReserve { get; set; }
    
    /// <summary>
    /// 提前报警天数
    /// </summary>
    public int? AlarmDay { get; set; }
    
    /// <summary>
    /// 授权编码
    /// </summary>
    public string? token { get; set; }
    
    /// <summary>
    /// 授权用户
    /// </summary>
    public string? accountExec { get; set; }
    
    /// <summary>
    /// 启用状态
    /// </summary>
    public bool? Status { get; set; }
    
    /// <summary>
    /// 是否空托
    /// </summary>
    public bool? IsEmpty { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 物料信息增加输入参数
/// </summary>
public class AddWmsBaseMaterialInput
{
    /// <summary>
    /// 物料编码
    /// </summary>
    [Required(ErrorMessage = "物料编码不能为空")]
    [MaxLength(100, ErrorMessage = "物料编码字符长度不能超过100")]
    public string? MaterialCode { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    [Dict("MaterialType", AllowNullValue=true)]
    [Required(ErrorMessage = "物料类型不能为空")]
    public long? MaterialType { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    [Required(ErrorMessage = "物料名称不能为空")]
    [MaxLength(255, ErrorMessage = "物料名称字符长度不能超过255")]
    public string? MaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    [MaxLength(200, ErrorMessage = "物料规格字符长度不能超过200")]
    public string? MaterialStandard { get; set; }
    
    /// <summary>
    /// 计量单位
    /// </summary>
    public long? MaterialUnit { get; set; }
    
    /// <summary>
    /// 专属区域
    /// </summary>
    public string? MaterialAreaId { get; set; }
    
    /// <summary>
    /// 箱数量
    /// </summary>
    [Required(ErrorMessage = "箱数量不能为空")]
    [MaxLength(32, ErrorMessage = "箱数量字符长度不能超过32")]
    public string? BoxQuantity { get; set; }
    
    /// <summary>
    /// 物料来源
    /// </summary>
    [MaxLength(10, ErrorMessage = "物料来源字符长度不能超过10")]
    public string? MaterialOrigin { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(255, ErrorMessage = "备注字符长度不能超过255")]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 每件托数
    /// </summary>
    [Required(ErrorMessage = "每件托数不能为空")]
    [MaxLength(32, ErrorMessage = "每件托数字符长度不能超过32")]
    public string? EveryNumber { get; set; }
    
    /// <summary>
    /// 载具
    /// </summary>
    [Dict("Vehicle", AllowNullValue=true)]
    [Required(ErrorMessage = "载具不能为空")]
    [MaxLength(32, ErrorMessage = "载具字符长度不能超过32")]
    public string? Vehicle { get; set; }
    
    /// <summary>
    /// 物料重量
    /// </summary>
    public decimal? MaterialWeight { get; set; }
    
    /// <summary>
    /// 最高库存数量
    /// </summary>
    [MaxLength(32, ErrorMessage = "最高库存数量字符长度不能超过32")]
    public string? MaterialStockHigh { get; set; }
    
    /// <summary>
    /// 最低库存数量
    /// </summary>
    [MaxLength(32, ErrorMessage = "最低库存数量字符长度不能超过32")]
    public string? MaterialStockLow { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    [Required(ErrorMessage = "所属仓库不能为空")]
    [MaxLength(32, ErrorMessage = "所属仓库字符长度不能超过32")]
    public string? WarehouseId { get; set; }
    
    /// <summary>
    /// 提前预警天数
    /// </summary>
    [MaxLength(32, ErrorMessage = "提前预警天数字符长度不能超过32")]
    public string? MaterialAlarmDay { get; set; }
    
    /// <summary>
    /// 贴标
    /// </summary>
    [Dict("Labeling", AllowNullValue=true)]
    [Required(ErrorMessage = "贴标不能为空")]
    [MaxLength(32, ErrorMessage = "贴标字符长度不能超过32")]
    public string? Labeling { get; set; }
    
    /// <summary>
    /// 管理方式
    /// </summary>
    [Dict("ManageType", AllowNullValue=true)]
    [MaxLength(32, ErrorMessage = "管理方式字符长度不能超过32")]
    public string? ManageType { get; set; }
    
    /// <summary>
    /// 最低储备
    /// </summary>
    [MaxLength(32, ErrorMessage = "最低储备字符长度不能超过32")]
    public string? MixReserve { get; set; }
    
    /// <summary>
    /// 最高储备
    /// </summary>
    [MaxLength(32, ErrorMessage = "最高储备字符长度不能超过32")]
    public string? MaxReserve { get; set; }
    
    /// <summary>
    /// 提前报警天数
    /// </summary>
    public int? AlarmDay { get; set; }
    
    /// <summary>
    /// 是否空托
    /// </summary>
    public bool? IsEmpty { get; set; }
    
}

/// <summary>
/// 物料信息删除输入参数
/// </summary>
public class DeleteWmsBaseMaterialInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 物料信息更新输入参数
/// </summary>
public class UpdateWmsBaseMaterialInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 物料编码
    /// </summary>    
    [Required(ErrorMessage = "物料编码不能为空")]
    [MaxLength(100, ErrorMessage = "物料编码字符长度不能超过100")]
    public string? MaterialCode { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>    
    [Dict("MaterialType", AllowNullValue=true)]
    [Required(ErrorMessage = "物料类型不能为空")]
    public long? MaterialType { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>    
    [Required(ErrorMessage = "物料名称不能为空")]
    [MaxLength(255, ErrorMessage = "物料名称字符长度不能超过255")]
    public string? MaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>    
    [MaxLength(200, ErrorMessage = "物料规格字符长度不能超过200")]
    public string? MaterialStandard { get; set; }
    
    /// <summary>
    /// 计量单位
    /// </summary>    
    public long? MaterialUnit { get; set; }
    
    /// <summary>
    /// 专属区域
    /// </summary>    
    public string? MaterialAreaId { get; set; }
    
    /// <summary>
    /// 箱数量
    /// </summary>    
    [Required(ErrorMessage = "箱数量不能为空")]
    [MaxLength(32, ErrorMessage = "箱数量字符长度不能超过32")]
    public string? BoxQuantity { get; set; }
    
    /// <summary>
    /// 物料来源
    /// </summary>    
    [MaxLength(10, ErrorMessage = "物料来源字符长度不能超过10")]
    public string? MaterialOrigin { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>    
    [MaxLength(255, ErrorMessage = "备注字符长度不能超过255")]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 每件托数
    /// </summary>    
    [Required(ErrorMessage = "每件托数不能为空")]
    [MaxLength(32, ErrorMessage = "每件托数字符长度不能超过32")]
    public string? EveryNumber { get; set; }
    
    /// <summary>
    /// 载具
    /// </summary>    
    [Dict("Vehicle", AllowNullValue=true)]
    [Required(ErrorMessage = "载具不能为空")]
    [MaxLength(32, ErrorMessage = "载具字符长度不能超过32")]
    public string? Vehicle { get; set; }
    
    /// <summary>
    /// 物料重量
    /// </summary>    
    public decimal? MaterialWeight { get; set; }
    
    /// <summary>
    /// 最高库存数量
    /// </summary>    
    [MaxLength(32, ErrorMessage = "最高库存数量字符长度不能超过32")]
    public string? MaterialStockHigh { get; set; }
    
    /// <summary>
    /// 最低库存数量
    /// </summary>    
    [MaxLength(32, ErrorMessage = "最低库存数量字符长度不能超过32")]
    public string? MaterialStockLow { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>    
    [Required(ErrorMessage = "所属仓库不能为空")]
    [MaxLength(32, ErrorMessage = "所属仓库字符长度不能超过32")]
    public string? WarehouseId { get; set; }
    
    /// <summary>
    /// 提前预警天数
    /// </summary>    
    [MaxLength(32, ErrorMessage = "提前预警天数字符长度不能超过32")]
    public string? MaterialAlarmDay { get; set; }
    
    /// <summary>
    /// 贴标
    /// </summary>    
    [Dict("Labeling", AllowNullValue=true)]
    [Required(ErrorMessage = "贴标不能为空")]
    [MaxLength(32, ErrorMessage = "贴标字符长度不能超过32")]
    public string? Labeling { get; set; }
    
    /// <summary>
    /// 管理方式
    /// </summary>    
    [Dict("ManageType", AllowNullValue=true)]
    [MaxLength(32, ErrorMessage = "管理方式字符长度不能超过32")]
    public string? ManageType { get; set; }
    
    /// <summary>
    /// 最低储备
    /// </summary>    
    [MaxLength(32, ErrorMessage = "最低储备字符长度不能超过32")]
    public string? MixReserve { get; set; }
    
    /// <summary>
    /// 最高储备
    /// </summary>    
    [MaxLength(32, ErrorMessage = "最高储备字符长度不能超过32")]
    public string? MaxReserve { get; set; }
    
    /// <summary>
    /// 提前报警天数
    /// </summary>    
    public int? AlarmDay { get; set; }
    
    /// <summary>
    /// 是否空托
    /// </summary>    
    public bool? IsEmpty { get; set; }
    
}

/// <summary>
/// 物料信息主键查询输入参数
/// </summary>
public class QueryByIdWmsBaseMaterialInput : DeleteWmsBaseMaterialInput
{
}

/// <summary>
/// 下拉数据输入参数
/// </summary>
public class DropdownDataWmsBaseMaterialInput
{
    /// <summary>
    /// 是否用于分页查询
    /// </summary>
    public bool FromPage { get; set; }
}

/// <summary>
/// 物料信息数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsBaseMaterialInput : BaseImportInput
{
    /// <summary>
    /// 物料编码
    /// </summary>
    [ImporterHeader(Name = "*物料编码")]
    [ExporterHeader("*物料编码", Format = "", Width = 25, IsBold = true)]
    public string? MaterialCode { get; set; }
    
    /// <summary>
    /// 物料类型 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? MaterialType { get; set; }
    
    /// <summary>
    /// 物料类型 文本
    /// </summary>
    [Dict("MaterialType")]
    [ImporterHeader(Name = "*物料类型")]
    [ExporterHeader("*物料类型", Format = "", Width = 25, IsBold = true)]
    public string MaterialTypeDictLabel { get; set; }
    
    /// <summary>
    /// 物料名称
    /// </summary>
    [ImporterHeader(Name = "*物料名称")]
    [ExporterHeader("*物料名称", Format = "", Width = 25, IsBold = true)]
    public string? MaterialName { get; set; }
    
    /// <summary>
    /// 物料规格
    /// </summary>
    [ImporterHeader(Name = "物料规格")]
    [ExporterHeader("物料规格", Format = "", Width = 25, IsBold = true)]
    public string? MaterialStandard { get; set; }
    
    /// <summary>
    /// 计量单位 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? MaterialUnit { get; set; }
    
    /// <summary>
    /// 计量单位 文本
    /// </summary>
    [ImporterHeader(Name = "计量单位")]
    [ExporterHeader("计量单位", Format = "", Width = 25, IsBold = true)]
    public string MaterialUnitFkDisplayName { get; set; }
    
    /// <summary>
    /// 专属区域 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? MaterialAreaId { get; set; }
    
    /// <summary>
    /// 专属区域 文本
    /// </summary>
    [ImporterHeader(Name = "专属区域")]
    [ExporterHeader("专属区域", Format = "", Width = 25, IsBold = true)]
    public string MaterialAreaFkDisplayName { get; set; }
    
    /// <summary>
    /// 箱数量
    /// </summary>
    [ImporterHeader(Name = "*箱数量")]
    [ExporterHeader("*箱数量", Format = "", Width = 25, IsBold = true)]
    public string? BoxQuantity { get; set; }
    
    /// <summary>
    /// 物料来源
    /// </summary>
    [ImporterHeader(Name = "物料来源")]
    [ExporterHeader("物料来源", Format = "", Width = 25, IsBold = true)]
    public string? MaterialOrigin { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 每件托数
    /// </summary>
    [ImporterHeader(Name = "*每件托数")]
    [ExporterHeader("*每件托数", Format = "", Width = 25, IsBold = true)]
    public string? EveryNumber { get; set; }
    
    /// <summary>
    /// 载具 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? Vehicle { get; set; }
    
    /// <summary>
    /// 载具 文本
    /// </summary>
    [Dict("Vehicle")]
    [ImporterHeader(Name = "*载具")]
    [ExporterHeader("*载具", Format = "", Width = 25, IsBold = true)]
    public string VehicleDictLabel { get; set; }
    
    /// <summary>
    /// 物料重量
    /// </summary>
    [ImporterHeader(Name = "物料重量")]
    [ExporterHeader("物料重量", Format = "", Width = 25, IsBold = true)]
    public decimal? MaterialWeight { get; set; }
    
    /// <summary>
    /// 最高库存数量
    /// </summary>
    [ImporterHeader(Name = "最高库存数量")]
    [ExporterHeader("最高库存数量", Format = "", Width = 25, IsBold = true)]
    public string? MaterialStockHigh { get; set; }
    
    /// <summary>
    /// 最低库存数量
    /// </summary>
    [ImporterHeader(Name = "最低库存数量")]
    [ExporterHeader("最低库存数量", Format = "", Width = 25, IsBold = true)]
    public string? MaterialStockLow { get; set; }
    
    /// <summary>
    /// 所属仓库 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? WarehouseId { get; set; }
    
    /// <summary>
    /// 所属仓库 文本
    /// </summary>
    [ImporterHeader(Name = "*所属仓库")]
    [ExporterHeader("*所属仓库", Format = "", Width = 25, IsBold = true)]
    public string WarehouseFkDisplayName { get; set; }
    
    /// <summary>
    /// 提前预警天数
    /// </summary>
    [ImporterHeader(Name = "提前预警天数")]
    [ExporterHeader("提前预警天数", Format = "", Width = 25, IsBold = true)]
    public string? MaterialAlarmDay { get; set; }
    
    /// <summary>
    /// 贴标 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? Labeling { get; set; }
    
    /// <summary>
    /// 贴标 文本
    /// </summary>
    [Dict("Labeling")]
    [ImporterHeader(Name = "*贴标")]
    [ExporterHeader("*贴标", Format = "", Width = 25, IsBold = true)]
    public string LabelingDictLabel { get; set; }
    
    /// <summary>
    /// 管理方式 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? ManageType { get; set; }
    
    /// <summary>
    /// 管理方式 文本
    /// </summary>
    [Dict("ManageType")]
    [ImporterHeader(Name = "管理方式")]
    [ExporterHeader("管理方式", Format = "", Width = 25, IsBold = true)]
    public string ManageTypeDictLabel { get; set; }
    
    /// <summary>
    /// 最低储备
    /// </summary>
    [ImporterHeader(Name = "最低储备")]
    [ExporterHeader("最低储备", Format = "", Width = 25, IsBold = true)]
    public string? MixReserve { get; set; }
    
    /// <summary>
    /// 最高储备
    /// </summary>
    [ImporterHeader(Name = "最高储备")]
    [ExporterHeader("最高储备", Format = "", Width = 25, IsBold = true)]
    public string? MaxReserve { get; set; }
    
    /// <summary>
    /// 提前报警天数
    /// </summary>
    [ImporterHeader(Name = "提前报警天数")]
    [ExporterHeader("提前报警天数", Format = "", Width = 25, IsBold = true)]
    public int? AlarmDay { get; set; }
    
}
