// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Application;

/// <summary>
/// 入库流水输出参数
/// </summary>
public class WmsImportOrderDto
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
    public DateTime? CompleteDate { get; set; }
    
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
    public DateTime? CreateTime { get; set; }
    
    /// <summary>
    /// 修改者Id
    /// </summary>
    public long? UpdateUserId { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
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
