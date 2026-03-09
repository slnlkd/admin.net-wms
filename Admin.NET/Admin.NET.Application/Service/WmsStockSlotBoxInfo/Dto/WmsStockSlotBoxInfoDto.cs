// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Application;

/// <summary>
/// 库存箱码明细输出参数
/// </summary>
public class WmsStockSlotBoxInfoDto
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 箱码
    /// </summary>
    public string? BoxCode { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    public decimal? Qty { get; set; }
    
    /// <summary>
    /// 整箱数
    /// </summary>
    public decimal? FullBoxQty { get; set; }
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    public string? StockCode { get; set; }
    
    /// <summary>
    /// 托盘条码id
    /// </summary>
    public string? StockCodeId { get; set; }
    
    /// <summary>
    /// 状态（0未执行；1正在入库；2已入库）
    /// </summary>
    public int? Status { get; set; }
    
    /// <summary>
    /// 入库单id
    /// </summary>
    public string? ImportId { get; set; }
    
    /// <summary>
    /// 入库单详情id
    /// </summary>
    public string? ImportDetailId { get; set; }
    
    /// <summary>
    /// 入库流水id
    /// </summary>
    public string? ImportOrderId { get; set; }
    
    /// <summary>
    /// 等级
    /// </summary>
    public int? BoxLevel { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ProductionDate { get; set; }
    
    /// <summary>
    /// 保质期
    /// </summary>
    public DateTime? ValidateDay { get; set; }
    
    /// <summary>
    /// 批次
    /// </summary>
    public string? LotNo { get; set; }
    
    /// <summary>
    /// 物料id
    /// </summary>
    public string? MaterialId { get; set; }

    /// <summary>
    /// 供应商ID
    /// </summary>
    public long? SupplierId { get; set; }

    /// <summary>
    /// 制造商ID
    /// </summary>
    public long? ManufacturerId { get; set; }
    
    /// <summary>
    /// 是否零箱（0否；1是）
    /// </summary>
    public int? BulkTank { get; set; }
    
    /// <summary>
    /// 是否抽检箱（0否；1是）
    /// </summary>
    public int? IsSamplingBox { get; set; }
    
    /// <summary>
    /// 质检状态（0待检；1合格；2不合格）
    /// </summary>
    public int? InspectionStatus { get; set; }
    
    /// <summary>
    /// erp箱二维码
    /// </summary>
    public string? QRCode { get; set; }
    
    /// <summary>
    /// 采样日期
    /// </summary>
    public DateTime? SamplingDate { get; set; }
    
    /// <summary>
    /// 浆员编码
    /// </summary>
    public string? StaffCode { get; set; }
    
    /// <summary>
    /// 浆员姓名
    /// </summary>
    public string? StaffName { get; set; }
    
    /// <summary>
    /// 血浆重量
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// 剔除原因（手持剔除异常血浆存放）
    /// </summary>
    public string? ReasonsForExcl { get; set; }
    
    /// <summary>
    /// 检疫期状态（1检疫期满足；2检疫期不满足；3检疫期不合格）
    /// </summary>
    public int? ExtractStatus { get; set; }
    
    /// <summary>
    /// 是否挑浆（0默认；1挑浆）
    /// </summary>
    public string? PickingSlurry { get; set; }
    
    /// <summary>
    /// 血浆拒收类型id
    /// </summary>
    public string? plasmaRejectTypeId { get; set; }
    
    
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    
   
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
    
    
}
