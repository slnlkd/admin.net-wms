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

namespace Admin.NET.Application.Service.WmsSqlView.Dto;
/// <summary>
/// View_WmsImportNotifyDetail 视图投影类。
/// </summary>
public  class ViewImportNotifyDetail
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    /// 入库单据ID
    /// </summary>
    public long? ImportId { get; set; }
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? WareHouseId { get; set; }
    /// <summary>
    /// 入库单据号
    /// </summary>
    public string? ImportBillCode { get; set; }
    /// <summary>
    /// 外部单据号
    /// </summary>
    public string? OuterBillCode { get; set; }
    /// <summary>
    /// 外部明细ID
    /// </summary>
    public string? OuterDetailId { get; set; }
    /// <summary>
    /// 入库单据号
    /// </summary>
    public string? ImportListNo { get; set; }
    /// <summary>
    /// 入库执行标识
    /// </summary>
    public string? ImportExecuteFlag { get; set; }
    /// <summary>
    /// 物料ID
    /// </summary>
    public long? MaterialId { get; set; }
    /// <summary>
    /// 批次号
    /// </summary>
    public string? LotNo { get; set; }
    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }
    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterialName { get; set; }
    /// <summary>
    /// 物料标准
    /// </summary>
    public string? MaterialStandard { get; set; }
    /// <summary>
    /// 箱量
    /// </summary>
    public string? BoxQuantity { get; set; }
    /// <summary>
    /// 物料备注
    /// </summary>
    public string? MaterialRemark { get; set; }
    /// <summary>
    /// 物料状态
    /// </summary>
    public string? MaterialStatus { get; set; }
    /// <summary>
    /// 物料状态名称
    /// </summary>
    public string? MaterialStatusName { get; set; }
    /// <summary>
    /// 单位名称
    /// </summary>
    public string? UnitName { get; set; }
    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ImportProductionDate { get; set; }
    /// <summary>
    /// 失效日期
    /// </summary>
    public DateTime? ImportLostDate { get; set; }
    /// <summary>
    /// 入库数量
    /// </summary>
    public decimal? ImportQuantity { get; set; }
    /// <summary>
    /// 入库实际数量
    /// </summary>
    public decimal? ImportFactQuantity { get; set; }
    /// <summary>
    /// 入库完成数量
    /// </summary>
    public decimal? ImportCompleteQuantity { get; set; }
    /// <summary>
    /// 类型名称
    /// </summary>
    public string? TypeName { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
    /// <summary>
    /// 上传数量
    /// </summary>
    public decimal? UploadQuantity { get; set; }
    /// <summary>
    /// 类型名称
    /// </summary>
    public string? ZTypeName { get; set; }
    /// <summary>
    /// 标签
    /// </summary>
    public string? Labeling { get; set; }
    /// <summary>
    /// 任务状态
    /// </summary>
    public int? TaskStatus { get; set; }
    /// <summary>
    /// 标签判断
    /// </summary>
    public int? LabelJudgment { get; set; }
    /// <summary>
    /// 出库数量
    /// </summary>
    public decimal? outQty { get; set; }
    /// <summary>
    /// 是否删除
    /// </summary>
    public bool IsDel { get; set; }
    /// <summary>
    /// 创建人ID
    /// </summary>
    public long? CreateUser { get; set; }
    /// <summary>
    /// 创建人名称
    /// </summary>
    public string? CreateUserName { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    /// <summary>
    /// 更新人ID
    /// </summary>
    public long? UpdateUser { get; set; }
    /// <summary>
    /// 更新人名称
    /// </summary>
    public string? UpdateUserName { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    /// <summary>
    /// 巷道编码
    /// </summary>
    public string? LanewayCode { get; set; }
    /// <summary>
    /// 单据类型
    /// </summary>
    public string? BillType { get; set; }
    /// <summary>
    /// 客户ID
    /// </summary>
    public long? CustomerId { get; set; }
    /// <summary>
    /// 供应商ID
    /// </summary>
    public long? SupplierId { get; set; }
    /// <summary>
    /// 生产ID
    /// </summary>
    public long? ProduceId { get; set; }
}
