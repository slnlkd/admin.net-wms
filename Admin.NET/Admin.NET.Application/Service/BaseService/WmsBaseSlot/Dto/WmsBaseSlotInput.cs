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
/// 储位管理基础输入参数
/// </summary>
public class WmsBaseSlotBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    [Required(ErrorMessage = "所属仓库不能为空")]
    public virtual long? WarehouseId { get; set; }
    
    /// <summary>
    /// 所属区域
    /// </summary>
    [Required(ErrorMessage = "所属区域不能为空")]
    public virtual long? SlotAreaId { get; set; }
    
    /// <summary>
    /// 所属巷道
    /// </summary>
    [Required(ErrorMessage = "所属巷道不能为空")]
    public virtual long? SlotLanewayId { get; set; }
    
    /// <summary>
    /// 排
    /// </summary>
    public virtual int? SlotRow { get; set; }
    
    /// <summary>
    /// 列
    /// </summary>
    public virtual int? SlotColumn { get; set; }
    
    /// <summary>
    /// 层
    /// </summary>
    public virtual int? SlotLayer { get; set; }
    
    /// <summary>
    /// 储位深度
    /// </summary>
    [Dict("SlotInout", AllowNullValue=true)]
    public virtual int? SlotInout { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    public virtual string? SlotCode { get; set; }
    
    /// <summary>
    /// 储位状态
    /// </summary>
    [Dict("SlotStatus", AllowNullValue=true)]
    public virtual int? SlotStatus { get; set; }
    
    /// <summary>
    /// 库位类型
    /// </summary>
    [Dict("Make", AllowNullValue=true)]
    public virtual string? Make { get; set; }
    
    /// <summary>
    /// 储位属性
    /// </summary>
    [Dict("Property", AllowNullValue=true)]
    public virtual string? Property { get; set; }
    
    /// <summary>
    /// 储位处理
    /// </summary>
    [Dict("Handle", AllowNullValue=true)]
    public virtual string? Handle { get; set; }
    
    /// <summary>
    /// 储位环境
    /// </summary>
    [Dict("Environment", AllowNullValue=true)]
    public virtual string? Environment { get; set; }
    
    /// <summary>
    /// 入库锁定
    /// </summary>
    public virtual int? SlotImlockFlag { get; set; }
    
    /// <summary>
    /// 出库锁定
    /// </summary>
    public virtual int? SlotExlockFlag { get; set; }
    
    /// <summary>
    /// 是否屏蔽
    /// </summary>
    public virtual int? SlotCloseFlag { get; set; }
    /// <summary>
    /// 展示类型
    /// layer=按层展示
    /// row=按排展示
    /// </summary>
    public string ShowType { get; set; }


}
/// <summary>
/// 储位图例查询输入参数
/// </summary>
public class WmsBaseSlotBaseQueryInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }

    /// <summary>
    /// 所属仓库
    /// </summary>
    [Required(ErrorMessage = "所属仓库不能为空")]
    public virtual long? WarehouseId { get; set; }

    /// <summary>
    /// 所属巷道
    /// </summary>
    public virtual long? SlotLanewayId { get; set; }

    /// <summary>
    /// 排
    /// </summary>
    public virtual int? SlotRow { get; set; }

    /// <summary>
    /// 列
    /// </summary>
    public virtual int? SlotColumn { get; set; }

    /// <summary>
    /// 层
    /// </summary>
    public virtual int? SlotLayer { get; set; }

    /// <summary>
    /// 储位深度
    /// </summary>
    [Dict("SlotInout", AllowNullValue = true)]
    public virtual int? SlotInout { get; set; }

    /// <summary>
    /// 储位编码
    /// </summary>
    public virtual string? SlotCode { get; set; }

    /// <summary>
    /// 储位状态
    /// </summary>
    [Dict("SlotStatus", AllowNullValue = true)]
    public virtual int? SlotStatus { get; set; }

    /// <summary>
    /// 库位类型
    /// </summary>
    [Dict("Make", AllowNullValue = true)]
    public virtual string? Make { get; set; }

    /// <summary>
    /// 储位属性
    /// </summary>
    [Dict("Property", AllowNullValue = true)]
    public virtual string? Property { get; set; }

    /// <summary>
    /// 储位处理
    /// </summary>
    [Dict("Handle", AllowNullValue = true)]
    public virtual string? Handle { get; set; }

    /// <summary>
    /// 储位环境
    /// </summary>
    [Dict("Environment", AllowNullValue = true)]
    public virtual string? Environment { get; set; }

    /// <summary>
    /// 入库锁定
    /// </summary>
    public virtual int? SlotImlockFlag { get; set; }

    /// <summary>
    /// 出库锁定
    /// </summary>
    public virtual int? SlotExlockFlag { get; set; }

    /// <summary>
    /// 是否屏蔽
    /// </summary>
    public virtual int? SlotCloseFlag { get; set; }
    /// <summary>
    /// 展示类型
    /// layer=按层展示
    /// row=按排展示
    /// </summary>
    public string ShowType { get; set; }


}
/// <summary>
/// 储位管理分页查询输入参数
/// </summary>
public class PageWmsBaseSlotInput : BasePageInput
{
    /// <summary>
    /// 所属仓库
    /// </summary>
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 所属区域
    /// </summary>
    public long? SlotAreaId { get; set; }
    
    /// <summary>
    /// 所属巷道
    /// </summary>
    public long? SlotLanewayId { get; set; }
    
    /// <summary>
    /// 排
    /// </summary>
    public int? SlotRow { get; set; }
    
    /// <summary>
    /// 列
    /// </summary>
    public int? SlotColumn { get; set; }
    
    /// <summary>
    /// 层
    /// </summary>
    public int? SlotLayer { get; set; }
    
    /// <summary>
    /// 储位深度
    /// </summary>
    [Dict("SlotInout", AllowNullValue=true)]
    public int? SlotInout { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    public string? SlotCode { get; set; }
    
    /// <summary>
    /// 储位状态
    /// </summary>
    [Dict("SlotStatus", AllowNullValue=true)]
    public int? SlotStatus { get; set; }
    
    /// <summary>
    /// 库位类型
    /// </summary>
    [Dict("Make", AllowNullValue=true)]
    public string? Make { get; set; }
    
    /// <summary>
    /// 储位属性
    /// </summary>
    [Dict("Property", AllowNullValue=true)]
    public string? Property { get; set; }
    
    /// <summary>
    /// 储位处理
    /// </summary>
    [Dict("Handle", AllowNullValue=true)]
    public string? Handle { get; set; }
    
    /// <summary>
    /// 储位环境
    /// </summary>
    [Dict("Environment", AllowNullValue=true)]
    public string? Environment { get; set; }
    
    /// <summary>
    /// 入库锁定
    /// </summary>
    public int? SlotImlockFlag { get; set; }
    
    /// <summary>
    /// 出库锁定
    /// </summary>
    public int? SlotExlockFlag { get; set; }
    
    /// <summary>
    /// 是否屏蔽
    /// </summary>
    public int? SlotCloseFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
    
    /// <summary>
    /// 储位高度
    /// </summary>
    public int? SlotHigh { get; set; }
    
    /// <summary>
    /// 限重
    /// </summary>
    public string? SlotWeight { get; set; }
    
    /// <summary>
    /// 目的中转货位
    /// </summary>
    public string? EndTransitLocation { get; set; }
    
    /// <summary>
    /// 四向车位置
    /// </summary>
    public int? IsSxcLocation { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 储位管理增加输入参数
/// </summary>
public class AddWmsBaseSlotInput
{
    /// <summary>
    /// 所属仓库
    /// </summary>
    [Required(ErrorMessage = "所属仓库不能为空")]
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 所属区域
    /// </summary>
    [Required(ErrorMessage = "所属区域不能为空")]
    public long? SlotAreaId { get; set; }
    
    /// <summary>
    /// 所属巷道
    /// </summary>
    [Required(ErrorMessage = "所属巷道不能为空")]
    public long? SlotLanewayId { get; set; }
    
    /// <summary>
    /// 排
    /// </summary>
    public int? SlotRow { get; set; }
    
    /// <summary>
    /// 列
    /// </summary>
    public int? SlotColumn { get; set; }
    
    /// <summary>
    /// 层
    /// </summary>
    public int? SlotLayer { get; set; }
    
    /// <summary>
    /// 储位深度
    /// </summary>
    [Dict("SlotInout", AllowNullValue=true)]
    public int? SlotInout { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    [MaxLength(20, ErrorMessage = "储位编码字符长度不能超过20")]
    public string? SlotCode { get; set; }
    
    /// <summary>
    /// 储位状态
    /// </summary>
    [Dict("SlotStatus", AllowNullValue=true)]
    public int? SlotStatus { get; set; }
    
    /// <summary>
    /// 库位类型
    /// </summary>
    [Dict("Make", AllowNullValue=true)]
    [MaxLength(10, ErrorMessage = "库位类型字符长度不能超过10")]
    public string? Make { get; set; }
    
    /// <summary>
    /// 储位属性
    /// </summary>
    [Dict("Property", AllowNullValue=true)]
    [MaxLength(10, ErrorMessage = "储位属性字符长度不能超过10")]
    public string? Property { get; set; }
    
    /// <summary>
    /// 储位处理
    /// </summary>
    [Dict("Handle", AllowNullValue=true)]
    [MaxLength(10, ErrorMessage = "储位处理字符长度不能超过10")]
    public string? Handle { get; set; }
    
    /// <summary>
    /// 储位环境
    /// </summary>
    [Dict("Environment", AllowNullValue=true)]
    [MaxLength(10, ErrorMessage = "储位环境字符长度不能超过10")]
    public string? Environment { get; set; }
    
    /// <summary>
    /// 入库锁定
    /// </summary>
    public int? SlotImlockFlag { get; set; }
    
    /// <summary>
    /// 出库锁定
    /// </summary>
    public int? SlotExlockFlag { get; set; }
    
    /// <summary>
    /// 是否屏蔽
    /// </summary>
    public int? SlotCloseFlag { get; set; }
    
}

/// <summary>
/// 储位管理删除输入参数
/// </summary>
public class DeleteWmsBaseSlotInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 储位管理更新输入参数
/// </summary>
public class UpdateWmsBaseSlotInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>    
    [Required(ErrorMessage = "所属仓库不能为空")]
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 所属区域
    /// </summary>    
    [Required(ErrorMessage = "所属区域不能为空")]
    public long? SlotAreaId { get; set; }
    
    /// <summary>
    /// 所属巷道
    /// </summary>    
    [Required(ErrorMessage = "所属巷道不能为空")]
    public long? SlotLanewayId { get; set; }
    
    /// <summary>
    /// 排
    /// </summary>    
    public int? SlotRow { get; set; }
    
    /// <summary>
    /// 列
    /// </summary>    
    public int? SlotColumn { get; set; }
    
    /// <summary>
    /// 层
    /// </summary>    
    public int? SlotLayer { get; set; }
    
    /// <summary>
    /// 储位深度
    /// </summary>    
    [Dict("SlotInout", AllowNullValue=true)]
    public int? SlotInout { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>    
    [MaxLength(20, ErrorMessage = "储位编码字符长度不能超过20")]
    public string? SlotCode { get; set; }
    
    /// <summary>
    /// 储位状态
    /// </summary>    
    [Dict("SlotStatus", AllowNullValue=true)]
    public int? SlotStatus { get; set; }
    
    /// <summary>
    /// 库位类型
    /// </summary>    
    [Dict("Make", AllowNullValue=true)]
    [MaxLength(10, ErrorMessage = "库位类型字符长度不能超过10")]
    public string? Make { get; set; }
    
    /// <summary>
    /// 储位属性
    /// </summary>    
    [Dict("Property", AllowNullValue=true)]
    [MaxLength(10, ErrorMessage = "储位属性字符长度不能超过10")]
    public string? Property { get; set; }
    
    /// <summary>
    /// 储位处理
    /// </summary>    
    [Dict("Handle", AllowNullValue=true)]
    [MaxLength(10, ErrorMessage = "储位处理字符长度不能超过10")]
    public string? Handle { get; set; }
    
    /// <summary>
    /// 储位环境
    /// </summary>    
    [Dict("Environment", AllowNullValue=true)]
    [MaxLength(10, ErrorMessage = "储位环境字符长度不能超过10")]
    public string? Environment { get; set; }
    
    /// <summary>
    /// 入库锁定
    /// </summary>    
    public int? SlotImlockFlag { get; set; }
    
    /// <summary>
    /// 出库锁定
    /// </summary>    
    public int? SlotExlockFlag { get; set; }
    
    /// <summary>
    /// 是否屏蔽
    /// </summary>    
    public int? SlotCloseFlag { get; set; }
    
}

/// <summary>
/// 储位管理主键查询输入参数
/// </summary>
public class QueryByIdWmsBaseSlotInput : DeleteWmsBaseSlotInput
{
}

/// <summary>
/// 下拉数据输入参数
/// </summary>
public class DropdownDataWmsBaseSlotInput
{
    /// <summary>
    /// 是否用于分页查询
    /// </summary>
    public bool FromPage { get; set; }
    /// <summary>
    /// 仓库id
    /// </summary>
    public long WarehouseId { get; set; }
    /// <summary>
    /// 巷道id
    /// </summary>
    public long SlotLanewayId { get; set; }
}

/// <summary>
/// 储位管理数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsBaseSlotInput : BaseImportInput
{
    /// <summary>
    /// 所属仓库 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 所属仓库 文本
    /// </summary>
    [ImporterHeader(Name = "*所属仓库")]
    [ExporterHeader("*所属仓库", Format = "", Width = 25, IsBold = true)]
    public string WarehouseFkDisplayName { get; set; }
    
    /// <summary>
    /// 所属区域 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? SlotAreaId { get; set; }
    
    /// <summary>
    /// 所属区域 文本
    /// </summary>
    [ImporterHeader(Name = "*所属区域")]
    [ExporterHeader("*所属区域", Format = "", Width = 25, IsBold = true)]
    public string SlotAreaFkDisplayName { get; set; }
    
    /// <summary>
    /// 所属巷道 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? SlotLanewayId { get; set; }
    
    /// <summary>
    /// 所属巷道 文本
    /// </summary>
    [ImporterHeader(Name = "*所属巷道")]
    [ExporterHeader("*所属巷道", Format = "", Width = 25, IsBold = true)]
    public string SlotLanewayFkDisplayName { get; set; }
    
    /// <summary>
    /// 排
    /// </summary>
    [ImporterHeader(Name = "排")]
    [ExporterHeader("排", Format = "", Width = 25, IsBold = true)]
    public int? SlotRow { get; set; }
    
    /// <summary>
    /// 列
    /// </summary>
    [ImporterHeader(Name = "列")]
    [ExporterHeader("列", Format = "", Width = 25, IsBold = true)]
    public int? SlotColumn { get; set; }
    
    /// <summary>
    /// 层
    /// </summary>
    [ImporterHeader(Name = "层")]
    [ExporterHeader("层", Format = "", Width = 25, IsBold = true)]
    public int? SlotLayer { get; set; }
    
    /// <summary>
    /// 储位深度 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public int? SlotInout { get; set; }
    
    /// <summary>
    /// 储位深度 文本
    /// </summary>
    //[Dict("SlotInout")]
    [ImporterHeader(Name = "储位深度")]
    [ExporterHeader("储位深度", Format = "", Width = 25, IsBold = true)]
    public string SlotInoutDictLabel { get; set; }
    
    /// <summary>
    /// 储位编码
    /// </summary>
    [ImporterHeader(Name = "储位编码")]
    [ExporterHeader("储位编码", Format = "", Width = 25, IsBold = true)]
    public string? SlotCode { get; set; }
    
    /// <summary>
    /// 储位状态 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public int? SlotStatus { get; set; }
    
    /// <summary>
    /// 储位状态 文本
    /// </summary>
    //[Dict("SlotStatus")]
    [ImporterHeader(Name = "储位状态")]
    [ExporterHeader("储位状态", Format = "", Width = 25, IsBold = true)]
    public string SlotStatusDictLabel { get; set; }
    
    /// <summary>
    /// 库位类型 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? Make { get; set; }
    
    /// <summary>
    /// 库位类型 文本
    /// </summary>
    //[Dict("Make")]
    [ImporterHeader(Name = "库位类型")]
    [ExporterHeader("库位类型", Format = "", Width = 25, IsBold = true)]
    public string MakeDictLabel { get; set; }
    
    /// <summary>
    /// 储位属性 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? Property { get; set; }
    
    /// <summary>
    /// 储位属性 文本
    /// </summary>
    //[Dict("Property")]
    [ImporterHeader(Name = "储位属性")]
    [ExporterHeader("储位属性", Format = "", Width = 25, IsBold = true)]
    public string PropertyDictLabel { get; set; }
    
    /// <summary>
    /// 储位处理 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? Handle { get; set; }
    
    /// <summary>
    /// 储位处理 文本
    /// </summary>
    //[Dict("Handle")]
    [ImporterHeader(Name = "储位处理")]
    [ExporterHeader("储位处理", Format = "", Width = 25, IsBold = true)]
    public string HandleDictLabel { get; set; }
    
    /// <summary>
    /// 储位环境 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? Environment { get; set; }
    
    /// <summary>
    /// 储位环境 文本
    /// </summary>
    //[Dict("Environment")]
    [ImporterHeader(Name = "储位环境")]
    [ExporterHeader("储位环境", Format = "", Width = 25, IsBold = true)]
    public string EnvironmentDictLabel { get; set; }

    /// <summary>
    /// 入库锁定
    /// </summary>
    [ImporterHeader(Name = "入库锁定")]
    [ExporterHeader("入库锁定", Format = "", Width = 25, IsBold = true)]
    public int? SlotImlockFlag { get; set; }
    
    /// <summary>
    /// 出库锁定
    /// </summary>
    [ImporterHeader(Name = "出库锁定")]
    [ExporterHeader("出库锁定", Format = "", Width = 25, IsBold = true)]
    public int? SlotExlockFlag { get; set; }
    
    /// <summary>
    /// 是否屏蔽
    /// </summary>
    [ImporterHeader(Name = "是否屏蔽")]
    [ExporterHeader("是否屏蔽", Format = "", Width = 25, IsBold = true)]
    public int? SlotCloseFlag { get; set; }
    
}
