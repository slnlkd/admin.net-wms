// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;
namespace Admin.NET.Application.Entity;

/// <summary>
/// 储位管理表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("WmsBaseSlot", "储位管理表")]
public partial class WmsBaseSlot : EntityBaseDel
{
    /// <summary>
    /// 储位编码
    /// </summary>
    [SugarColumn(ColumnName = "SlotCode", ColumnDescription = "储位编码", Length = 20)]
    public virtual string? SlotCode { get; set; }
    
    /// <summary>
    /// 储位序号(双深位同侧储位公用一个序号)
    /// </summary>
    [SugarColumn(ColumnName = "SlotCodeIndex", ColumnDescription = "储位序号(双深位同侧储位公用一个序号)")]
    public virtual int? SlotCodeIndex { get; set; }
    
    /// <summary>
    /// 所属库房ID
    /// </summary>
    [SugarColumn(ColumnName = "WarehouseId", ColumnDescription = "所属库房ID")]
    public virtual long? WarehouseId { get; set; }
    
    /// <summary>
    /// 区域ID
    /// </summary>
    [SugarColumn(ColumnName = "SlotAreaId", ColumnDescription = "区域ID")]
    public virtual long? SlotAreaId { get; set; }
    
    /// <summary>
    /// 巷道ID
    /// </summary>
    [SugarColumn(ColumnName = "SlotLanewayId", ColumnDescription = "巷道ID")]
    public virtual long? SlotLanewayId { get; set; }
    
    /// <summary>
    /// 储位状态( 0空储位、1 有物品、2 正在入库、3正在出库、4 正在移入、5正在移出、6空托盘、7 屏蔽、8储位不存在、9异常储位）
    /// </summary>
    [SugarColumn(ColumnName = "SlotStatus", ColumnDescription = "储位状态( 0空储位、1 有物品、2 正在入库、3正在出库、4 正在移入、5正在移出、6空托盘、7 屏蔽、8储位不存在、9异常储位）")]
    public virtual int? SlotStatus { get; set; }
    
    /// <summary>
    /// 入库锁定
    /// </summary>
    [SugarColumn(ColumnName = "SlotImlockFlag", ColumnDescription = "入库锁定")]
    public virtual int? SlotImlockFlag { get; set; }
    
    /// <summary>
    /// 出库锁定
    /// </summary>
    [SugarColumn(ColumnName = "SlotExlockFlag", ColumnDescription = "出库锁定")]
    public virtual int? SlotExlockFlag { get; set; }
    
    /// <summary>
    /// 是否屏蔽
    /// </summary>
    [SugarColumn(ColumnName = "SlotCloseFlag", ColumnDescription = "是否屏蔽")]
    public virtual int? SlotCloseFlag { get; set; }
    
    /// <summary>
    /// 深度(1靠近堆垛机一侧储位，2远离堆垛机一侧储位)
    /// </summary>
    [SugarColumn(ColumnName = "SlotInout", ColumnDescription = "深度(1靠近堆垛机一侧储位，2远离堆垛机一侧储位)")]
    public virtual int? SlotInout { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark", ColumnDescription = "备注", Length = 100)]
    public virtual string? Remark { get; set; }
    
    /// <summary>
    /// 排
    /// </summary>
    [SugarColumn(ColumnName = "SlotRow", ColumnDescription = "排")]
    public virtual int? SlotRow { get; set; }
    
    /// <summary>
    /// 列
    /// </summary>
    [SugarColumn(ColumnName = "SlotColumn", ColumnDescription = "列")]
    public virtual int? SlotColumn { get; set; }
    
    /// <summary>
    /// 层
    /// </summary>
    [SugarColumn(ColumnName = "SlotLayer", ColumnDescription = "层")]
    public virtual int? SlotLayer { get; set; }
    
    /// <summary>
    /// 储位高度
    /// </summary>
    [SugarColumn(ColumnName = "SlotHigh", ColumnDescription = "储位高度")]
    public virtual int? SlotHigh { get; set; }
    
    /// <summary>
    /// 限重
    /// </summary>
    [SugarColumn(ColumnName = "SlotWeight", ColumnDescription = "限重", Length = 10)]
    public virtual string? SlotWeight { get; set; }
    
    /// <summary>
    /// 库位类型（01存储库位 02 中转库位）字典表类型Make
    /// </summary>
    [SugarColumn(ColumnName = "Make", ColumnDescription = "库位类型（01存储库位 02 中转库位）字典表类型Make", Length = 10)]
    public virtual string? Make { get; set; }
    
    /// <summary>
    /// 储位属性（01损坏 02 封存 03正常）字典表类型Property
    /// </summary>
    [SugarColumn(ColumnName = "Property", ColumnDescription = "储位属性（01损坏 02 封存 03正常）字典表类型Property", Length = 10)]
    public virtual string? Property { get; set; }
    
    /// <summary>
    /// 储位处理（01仅血箱 02 仅存托盘 03 其他）字典表类型Environment
    /// </summary>
    [SugarColumn(ColumnName = "Handle", ColumnDescription = "储位处理（01仅血箱 02 仅存托盘 03 其他）字典表类型Environment", Length = 10)]
    public virtual string? Handle { get; set; }
    
    /// <summary>
    /// 储位环境（01阴凉 02 常温 03 炎热）字典表类型Environment
    /// </summary>
    [SugarColumn(ColumnName = "Environment", ColumnDescription = "储位环境（01阴凉 02 常温 03 炎热）字典表类型Environment", Length = 10)]
    public virtual string? Environment { get; set; }
    
    /// <summary>
    /// 目的中转货位
    /// </summary>
    [SugarColumn(ColumnName = "EndTransitLocation", ColumnDescription = "目的中转货位", Length = 32)]
    public virtual string? EndTransitLocation { get; set; }
    
    /// <summary>
    /// 四向车位置
    /// </summary>
    [SugarColumn(ColumnName = "IsSxcLocation", ColumnDescription = "四向车位置")]
    public virtual int? IsSxcLocation { get; set; }
    
}
