// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Application;

/// <summary>
/// 移库单据明细表输出参数
/// </summary>
public class WmsMoveNotifyDto
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 移库单据类型
    /// </summary>
    public string? MoveBillCode { get; set; }
    
    /// <summary>
    /// 移库序号
    /// </summary>
    public int? MoveListNo { get; set; }
    
    /// <summary>
    /// 移库批次
    /// </summary>
    public string? MoveLotNo { get; set; }
    
    /// <summary>
    /// 物料id
    /// </summary>
    public string? MaterialId { get; set; }
    
    /// <summary>
    /// 移库物料型号
    /// </summary>
    public string? MoveMaterialModel { get; set; }
    
    /// <summary>
    /// 物料温度
    /// </summary>
    public string? MoveMaterialTemp { get; set; }
    
    /// <summary>
    /// 物料类型（字典MaterialType）
    /// </summary>
    public string? MoveMaterialType { get; set; }
    
    /// <summary>
    /// 物料状态
    /// </summary>
    public string? MoveMaterialStatus { get; set; }
    
    /// <summary>
    /// 物料单位
    /// </summary>
    public string? MoveMaterialUnit { get; set; }
    
    /// <summary>
    /// 物料品牌
    /// </summary>
    public string? MoveMaterialBrand { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? MoveProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    public DateTime? MoveLostDate { get; set; }
    
    /// <summary>
    /// 移库数量
    /// </summary>
    public int? MoveQuantity { get; set; }
    
    /// <summary>
    /// 移库日期
    /// </summary>
    public DateTime? MoveDate { get; set; }
    
    /// <summary>
    /// 仓库id
    /// </summary>
    public long? MoveWarehouseId { get; set; }
    
    /// <summary>
    /// 移出巷道id
    /// </summary>
    public long? MoveLanewayOutCode { get; set; }
    
    /// <summary>
    /// 移入巷道id
    /// </summary>
    public string? MoveLanewayInCode { get; set; }
    
    /// <summary>
    /// 移出储位编码
    /// </summary>
    public string? MoveOutSlotCode { get; set; }
    
    /// <summary>
    /// 移入储位编码
    /// </summary>
    public string? MoveInSlotCode { get; set; }
    
    /// <summary>
    /// 执行标志（01待执行、02正在执行、03已完成、04已上传）
    /// </summary>
    public string? MoveExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? MoveRemark { get; set; }
    
    /// <summary>
    /// 移库任务号
    /// </summary>
    public string? MoveTaskNo { get; set; }
    
    /// <summary>
    /// 库存表id
    /// </summary>
    public long? StockStockCodeId { get; set; }
    
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
    /// 软删除
    /// </summary>
    public bool IsDelete { get; set; }
    
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }
    
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
    
}
