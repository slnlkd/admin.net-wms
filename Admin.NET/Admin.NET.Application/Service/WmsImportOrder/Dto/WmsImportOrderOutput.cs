// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using Magicodes.ExporterAndImporter.Core;
namespace Admin.NET.Application;

/// <summary>
/// 入库流水输出参数
/// </summary>
public class WmsImportOrderOutput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }    
    
    /// <summary>
    /// 流水号
    /// </summary>
    public string? ImportOrderNo { get; set; }    
    
    /// <summary>
    /// 入库单ID
    /// </summary>
    public long? ImportId { get; set; }    
    /// <summary>
    /// 入库单号
    /// </summary>
    public string? ImportBillCode { get; set; }

    /// <summary>
    /// 区域ID
    /// </summary>
    public long? ImportAreaId { get; set; }    
    
    /// <summary>
    /// 巷道ID
    /// </summary>
    public long? ImportLanewayId { get; set; }    
    
    /// <summary>
    /// 储位编码
    /// </summary>
    public string? ImportSlotCode { get; set; }   
    /// <summary>
    /// 物料Id
    /// </summary>
    public long? MaterialId { get; set; }
    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }
    /// <summary>
    /// 区域名称
    /// </summary>
    public string? AreaName { get; set; }
    /// <summary>
    /// 巷道名称
    /// </summary>
    public string? LanewayName { get; set; }
    /// <summary>
    /// 任务编号
    /// </summary>
    public string? TaskNo { get; set; }
    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterialName { get; set; }

    /// <summary>
    /// 托盘条码ID
    /// </summary>
    public long? StockCodeId { get; set; }    
    
    /// <summary>
    /// 数量
    /// </summary>
    public decimal? ImportQuantity { get; set; }    
    
    /// <summary>
    /// 重量
    /// </summary>
    public decimal? ImportWeight { get; set; }    
    
    /// <summary>
    /// 任务ID
    /// </summary>
    public long? ImportTaskId { get; set; }    
    
    /// <summary>
    /// 执行标志（0待执行、1正在执行、2已完成、3已上传）
    /// </summary>
    public string? ImportExecuteFlag { get; set; }    
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }    
    
    /// <summary>
    /// 班次
    /// </summary>
    public string? ImportClass { get; set; }    
    
    /// <summary>
    /// 批号
    /// </summary>
    public string? LotNo { get; set; }    
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ImportProductionDate { get; set; }    
    
    /// <summary>
    /// 完成时间
    /// </summary>
    public string? CompleteDate { get; set; }    
    
    /// <summary>
    /// 质检状态
    /// </summary>
    public int? InspectionStatus { get; set; }    
    
    /// <summary>
    /// 软删除
    /// </summary>
    public int? IsDelete { get; set; }    
    
    /// <summary>
    /// 创建者Id
    /// </summary>
    public long? CreateUserId { get; set; }    
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public string? CreateTime { get; set; }    
    
    /// <summary>
    /// 修改者Id
    /// </summary>
    public long? UpdateUserId { get; set; }    
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public string? UpdateTime { get; set; }    
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? WareHouseId { get; set; }    
    
    /// <summary>
    /// 入库单明细ID
    /// </summary>
    public long? ImportDetailId { get; set; }    
    
    /// <summary>
    /// 主子载具号
    /// </summary>
    public string? SubVehicleCode { get; set; }    
    
    /// <summary>
    /// 实际称重重量
    /// </summary>
    public decimal? Weight { get; set; }    
    
    /// <summary>
    /// 0未验收 1正在验收 2验收完成
    /// </summary>
    public int? InspectFlag { get; set; }    
    
    /// <summary>
    /// 是否暂存入库流水（不为空为是）
    /// </summary>
    public string? IsTemporaryStorage { get; set; }    
    
    /// <summary>
    /// 载具号
    /// </summary>
    public string? StockCode { get; set; }    
    
    /// <summary>
    /// 验收0 挑浆1
    /// </summary>
    public string? YsOrTJ { get; set; }    
    
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }    
    
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }    
    
}

/// <summary>
/// 入库流水导出-主表部分
/// </summary>
public class ExportWmsImportOrderMainDto
{
    /// <summary>
    /// 入库流水号
    /// </summary>
    [ExporterHeader("入库流水号", Format = "", Width = 25, IsBold = true)]
    public string? ImportOrderNo { get; set; }

    /// <summary>
    /// 载具条码
    /// </summary>
    [ExporterHeader("载具条码", Format = "", Width = 25, IsBold = true)]
    public string? StockCode { get; set; }

    /// <summary>
    /// 储位地址
    /// </summary>
    [ExporterHeader("储位地址", Format = "", Width = 25, IsBold = true)]
    public string? ImportSlotCode { get; set; }

    /// <summary>
    /// 执行状态
    /// </summary>
    [ExporterHeader("执行状态", Format = "", Width = 15, IsBold = true)]
    public string? ImportExecuteFlag { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    [ExporterHeader("完成时间", Format = "", Width = 25, IsBold = true)]
    public string? CompleteDate { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [ExporterHeader("创建人", Format = "", Width = 15, IsBold = true)]
    public string? CreateUserName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [ExporterHeader("创建时间", Format = "", Width = 25, IsBold = true)]
    public string? CreateTime { get; set; }
}

/// <summary>
/// 入库流水导出-子表部分
/// </summary>
public class ExportWmsImportOrderSubDto
{
    /// <summary>
    /// 入库流水号
    /// </summary>
    [ExporterHeader("入库流水号", Format = "", Width = 25, IsBold = true)]
    public string? ImportOrderNo { get; set; }

    /// <summary>
    /// 箱码编号
    /// </summary>
    [ExporterHeader("箱码编号", Format = "", Width = 25, IsBold = true)]
    public string? BoxCode { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    [ExporterHeader("物料编码", Format = "", Width = 25, IsBold = true)]
    public string MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    [ExporterHeader("物料名称", Format = "", Width = 25, IsBold = true)]
    public string MaterialName { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    [ExporterHeader("物料规格", Format = "", Width = 25, IsBold = true)]
    public string MaterialStandard { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    [ExporterHeader("批次", Format = "", Width = 25, IsBold = true)]
    public string? LotNo { get; set; }

    /// <summary>
    /// 实际数量
    /// </summary>
    [ExporterHeader("实际数量", Format = "", Width = 15, IsBold = true)]
    public decimal? Qty { get; set; }

    /// <summary>
    /// 零箱标识
    /// </summary>
    [ExporterHeader("零箱标识", Format = "", Width = 15, IsBold = true)]
    public string? BulkTank { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [ExporterHeader("状态", Format = "", Width = 15, IsBold = true)]
    public string? Status { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    [ExporterHeader("生产日期", Format = "", Width = 25, IsBold = true)]
    public string? ProductionDate { get; set; }

    /// <summary>
    /// 有效期至
    /// </summary>
    [ExporterHeader("有效期至", Format = "", Width = 25, IsBold = true)]
    public string? ValidateDay { get; set; }
}

/// <summary>
/// 入库流水数据导入模板实体
/// </summary>
public class ExportWmsImportOrderOutput : ImportWmsImportOrderInput
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public override string Error { get; set; }
}
