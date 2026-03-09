// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Application;

/// <summary>
/// 入库单输出参数
/// </summary>
public class WmsImportNotifyDto
{
    /// <summary>
    /// 所属仓库
    /// </summary>
    public string WarehouseIdFkColumn { get; set; }
    
    /// <summary>
    /// 供货单位
    /// </summary>
    public string SupplierIdFkColumn { get; set; }
    
    /// <summary>
    /// 客户单位
    /// </summary>
    public string CustomerIdFkColumn { get; set; }
    
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 所属仓库
    /// </summary>
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 入库单号
    /// </summary>
    public string? ImportBillCode { get; set; }
    
    /// <summary>
    /// 执行状态
    /// </summary>
    public string? ImportExecuteFlag { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    public long? BillType { get; set; }
    
    /// <summary>
    /// 部门ID
    /// </summary>
    public long? DepartmentId { get; set; }
    
    /// <summary>
    /// 供货单位
    /// </summary>
    public long? SupplierId { get; set; }
    
    /// <summary>
    /// 客户单位
    /// </summary>
    public long? CustomerId { get; set; }
    
    /// <summary>
    /// 来源
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// 外部单号
    /// </summary>
    public string? OuterBillCode { get; set; }
    
    /// <summary>
    /// 外部单据ID
    /// </summary>
    public string? OuterMainId { get; set; }
    
    /// <summary>
    /// 软删除
    /// </summary>
    public int? IsDelete { get; set; }
    
    /// <summary>
    /// 创建者Id
    /// </summary>
    public long? CreateUserId { get; set; }
    
    /// <summary>
    /// 修改者Id
    /// </summary>
    public long? UpdateUserId { get; set; }
    
    /// <summary>
    /// 生产商ID
    /// </summary>
    public long? ProduceId { get; set; }
    
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
    /// 修改时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
    
}
