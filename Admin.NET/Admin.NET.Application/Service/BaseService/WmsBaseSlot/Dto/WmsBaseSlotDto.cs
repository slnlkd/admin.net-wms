// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Application;

/// <summary>
/// 储位管理输出参数
/// </summary>
public class WmsBaseSlotDto
{
    /// <summary>
    /// 所属仓库
    /// </summary>
    public string WarehouseIdFkColumn { get; set; }
    
    /// <summary>
    /// 所属区域
    /// </summary>
    public string SlotAreaIdFkColumn { get; set; }
    
    /// <summary>
    /// 所属巷道
    /// </summary>
    public string SlotLanewayIdFkColumn { get; set; }
    
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
    
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
    public int? SlotInout { get; set; }
    /// <summary>
    /// 储位深度名称
    /// </summary>
    public string SlotInoutName { get; set; }


    /// <summary>
    /// 储位编码
    /// </summary>
    public string? SlotCode { get; set; }
    
    /// <summary>
    /// 储位状态
    /// </summary>
    public int? SlotStatus { get; set; }
    /// <summary>
    /// 储位状态名称
    /// </summary>
    public string SlotStatusName { get; set; }


    /// <summary>
    /// 库位类型
    /// </summary>
    public string? Make { get; set; }
    /// <summary>
    /// 库位类型名称
    /// </summary>
    public string MakeName { get; set; }


    /// <summary>
    /// 储位属性
    /// </summary>
    public string? Property { get; set; }
    /// <summary>
    /// 储位属性名称
    /// </summary>
    public string PropertyName { get; set; }


    /// <summary>
    /// 储位处理
    /// </summary>
    public string? Handle { get; set; }
    /// <summary>
    /// 储位处理名称
    /// </summary>
    public string HandleName { get; set; }


    /// <summary>
    /// 储位环境
    /// </summary>
    public string? Environment { get; set; }
    /// <summary>
    /// 储位环境名称
    /// </summary>
    public string EnvironmentName { get; set; }


    /// <summary>
    /// 储位序号(双深位同侧储位公用一个序号)
    /// </summary>
    public int? SlotCodeIndex { get; set; }
    
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
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }
    
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
    
    /// <summary>
    /// 创建者Id
    /// </summary>
    public long CreateUserId { get; set; }
    
    /// <summary>
    /// 修改者Id
    /// </summary>
    public long UpdateUserId { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdateTime { get; set; }
    /// <summary>
    /// 托盘编码
    /// </summary>
    public string StockCode { get; set; }
    /// <summary>
    /// 物料名称
    /// </summary>
    public string MaterialName { get; set; }
    /// <summary>
    /// 批次号
    /// </summary>
    public string BatchNo { get; set; }
    /// <summary>
    /// 重量/数量
    /// </summary>
    public decimal? Weight { get; set; }


}
