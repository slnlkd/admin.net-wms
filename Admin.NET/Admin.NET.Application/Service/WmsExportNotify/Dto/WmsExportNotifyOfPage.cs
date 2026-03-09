// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.NET.Application;

/// <summary>
/// 出库单据分页查询的表格
/// </summary>
public class WmsExportNotifyOfPage
{
    /// <summary>
    /// 出库单据id
    /// </summary>
    public long? Id { get; set; }
    /// <summary>
    /// 序号
    /// </summary>
    public int? Index { get; set; }

    /// <summary>
    /// 出库单号
    /// </summary>
    public string? ExportBillCode {  get; set; }

    /// <summary>
    /// 执行状态（0待执行、1正在分配、2正在执行、3已完成、4作废、5已上传）
    /// </summary>
    public int? ExportExecuteFlag {  get; set; }

    /// <summary>
    /// 执行状态--名称
    /// </summary>
    public string? ExportExecuteFlagStr { get; set; }

    /// <summary>
    /// 所属仓库id
    /// </summary>
    public long? WarehouseId { get; set; }

    /// <summary>
    /// 所属仓库名称
    /// </summary>
    public string? WarehouseStr { get; set; }

    /// <summary>
    /// 单据类型id
    /// </summary>
    public long? ExportBillType { get; set; }
    /// <summary>
    /// 单据类型名称
    /// </summary>
    public string? ExportBillTypeStr { get; set; }

    /// <summary>
    /// 单据子类型id
    /// </summary>
    public long? DocumentSubtype { get; set; }

    /// <summary>
    /// 单据子类型名称
    /// </summary>
    public string? DocumentSubtypeStr { get; set; }

    /// <summary>
    /// 来源
    /// </summary>
    public string? Source {  get; set; }

    /// <summary>
    /// 外部单号（外部单据编码）
    /// </summary>
    public string? OuterBillCode {  get; set; }

    /// <summary>
    /// 客户id
    /// </summary>
    public long? ExportCustomerId {  get; set; }

    /// <summary>
    /// 客户姓名
    /// </summary>
    public string? ExportCustomerStr { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreateUserName {  get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime {  get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    public string? UpdateUserName { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? ExportRemark { get; set; }

    

}
