// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Application;

/// <summary>
/// 出库单据表输出参数
/// </summary>
public class WmsExportNotifyDto
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 出库单据
    /// </summary>
    public string? ExportBillCode { get; set; }
    
    /// <summary>
    /// 单据类型
    /// </summary>
    public long? ExportBillType { get; set; }
    
    /// <summary>
    /// 出库批次
    /// </summary>
    public string? ExportLotNo { get; set; }
    
    /// <summary>
    /// 物料ID
    /// </summary>
    public long? MaterialId { get; set; }
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? WarehouseId { get; set; }
    
    /// <summary>
    /// 出库序号
    /// </summary>
    public string? ExportListNo { get; set; }
    
    /// <summary>
    /// 部门ID
    /// </summary>
    public long? ExportDepartmentId { get; set; }
    
    /// <summary>
    /// 供应商ID
    /// </summary>
    public long? ExportSupplierId { get; set; }
    
    /// <summary>
    /// 客户ID
    /// </summary>
    public long? ExportCustomerId { get; set; }
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ExportProductionDate { get; set; }
    
    /// <summary>
    /// 失效日期
    /// </summary>
    public DateTime? ExportLostDate { get; set; }
    
    /// <summary>
    /// 计划出库数量
    /// </summary>
    public decimal? ExportQuantity { get; set; }
    
    /// <summary>
    /// 任务下发数量
    /// </summary>
    public decimal? ExportFactQuantity { get; set; }
    
    /// <summary>
    /// 任务完成数量
    /// </summary>
    public decimal? ExportCompleteQuantity { get; set; }
    
    /// <summary>
    /// 已上传数量
    /// </summary>
    public decimal? ExportUploadQuantity { get; set; }
    
    /// <summary>
    /// 建单时间
    /// </summary>
    public DateTime? ExportDate { get; set; }
    
    /// <summary>
    /// 执行标志（0待执行、1正在分配、 2正在执行、3已完成、5已上传） 4作废
    /// </summary>
    public int? ExportExecuteFlag { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? ExportRemark { get; set; }
    
    /// <summary>
    /// 外部单据编码
    /// </summary>
    public string? OuterBillCode { get; set; }
    
    /// <summary>
    /// 外部单据ID
    /// </summary>
    public string? OuterMainId { get; set; }
    
    /// <summary>
    /// 来源（wms或sap）
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// 拣货区
    /// </summary>
    public string? PickingArea { get; set; }
    
    /// <summary>
    /// 拼箱状态（0：不拼箱1：拼箱）
    /// </summary>
    public int? PXStatus { get; set; }
    
    /// <summary>
    /// 整托出库口
    /// </summary>
    public string? WholeOutWare { get; set; }
    
    /// <summary>
    /// 分拣出库口
    /// </summary>
    public string? SortOutWare { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
    /// <summary>
    /// 创建者Id
    /// </summary>
    public long? CreateUserId { get; set; }
    
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }
    
    /// <summary>
    /// 修改者Id
    /// </summary>
    public long? UpdateUserId { get; set; }
    
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
    
    /// <summary>
    /// 软删除
    /// </summary>
    public bool IsDelete { get; set; }
    
    /// <summary>
    /// 单据子类型编码
    /// </summary>
    public long? DocumentSubtype { get; set; }
    
    /// <summary>
    /// 出库口id
    /// </summary>
    public string? WayOutId { get; set; }
    
    /// <summary>
    /// 出库添加时间
    /// </summary>
    public DateTime? ExportAddDateTime { get; set; }
    
}
