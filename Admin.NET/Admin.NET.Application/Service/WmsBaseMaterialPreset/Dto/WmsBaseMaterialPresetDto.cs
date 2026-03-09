// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Application;

/// <summary>
/// 申请物料信息输出参数
/// </summary>
public class WmsBaseMaterialPresetDto
{
    /// <summary>
    /// 计量单位
    /// </summary>
    public string MaterialUnitFkColumn { get; set; }
    
    /// <summary>
    /// 专属区域
    /// </summary>
    public string MaterialAreaIdFkColumn { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    public string WarehouseIdFkColumn { get; set; }
    
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
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
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
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
    /// 软删除
    /// </summary>
    public bool? IsDelete { get; set; }
    
    /// <summary>
    /// 创建者Id
    /// </summary>
    public long? CreateUserId { get; set; }
    
    /// <summary>
    /// 修改者Id
    /// </summary>
    public long? UpdateUserId { get; set; }
    
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
    public string? Labeling { get; set; }
    
    /// <summary>
    /// 管理方式
    /// </summary>
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
    /// 0=待审核，1=审核通过，2=驳回
    /// </summary>
    public int? ApprovalStatus { get; set; }
    
    /// <summary>
    /// 审核人名称
    /// </summary>
    public string? ApprovalUserName { get; set; }
    
    /// <summary>
    /// 审核ID
    /// </summary>
    public long? ApprovalUserId { get; set; }

    /// <summary>
    /// 审核备注
    /// </summary>
    public string ApprovalRemark { get; set; }

}
