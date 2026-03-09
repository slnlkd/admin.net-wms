// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using System.ComponentModel.DataAnnotations;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

namespace Admin.NET.Application;

/// <summary>
/// 入库单据-表体基础输入参数
/// </summary>
public class WmsImportNotifyDetailBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }

    /// <summary>
    /// 入库单据ID
    /// </summary>
    public virtual long? ImportId { get; set; }

    /// <summary>
    /// 外部单据明细ID
    /// </summary>
    public virtual string? OuterDetailId { get; set; }

    /// <summary>
    /// 序号
    /// </summary>
    public virtual string? ImportListNo { get; set; }

    /// <summary>
    /// 物料信息
    /// </summary>
    public virtual long? MaterialId { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    public virtual string? LotNo { get; set; }

    /// <summary>
    /// 物料状态
    /// </summary>
    public virtual string? MaterialStatus { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    public virtual DateTime? ImportProductionDate { get; set; }

    /// <summary>
    /// 失效日期
    /// </summary>
    public virtual DateTime? ImportLostDate { get; set; }

    /// <summary>
    /// 计划数量
    /// </summary>
    public virtual decimal? ImportQuantity { get; set; }

    /// <summary>
    /// 完成数量
    /// </summary>
    public virtual decimal? ImportCompleteQuantity { get; set; }

    /// <summary>
    /// 组盘数量
    /// </summary>
    public virtual decimal? ImportFactQuantity { get; set; }

    /// <summary>
    /// 已上传数量
    /// </summary>
    public virtual decimal? UploadQuantity { get; set; }

    /// <summary>
    /// 执行状态
    /// </summary>
    public virtual string? ImportExecuteFlag { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remark { get; set; }

    /// <summary>
    /// 打印标识，1打印，0不打印
    /// </summary>
    public virtual int? PrintFlag { get; set; }

    /// <summary>
    /// 下发任务状态默认0，1下发成功
    /// </summary>
    public virtual int? TaskStatus { get; set; }

    /// <summary>
    /// 标签判断，1已打印，2或空未打印
    /// </summary>
    public virtual int? LabelJudgment { get; set; }

    /// <summary>
    /// 件数
    /// </summary>
    public virtual decimal? outQty { get; set; }

    /// <summary>
    /// 巷道编码
    /// </summary>
    public virtual string? LanewayCode { get; set; }

    /// <summary>
    /// 采浆公司
    /// </summary>
    public virtual string? Xj_HouseCode { get; set; }

    /// <summary>
    /// 血浆件数
    /// </summary>
    public virtual decimal? Xj_Qty { get; set; }

    /// <summary>
    /// 血浆重量
    /// </summary>
    public virtual decimal? Xj_Weight { get; set; }

    /// <summary>
    /// 血浆 箱总数
    /// </summary>
    public virtual decimal? Xj_BoxQty { get; set; }

    /// <summary>
    /// 血浆类型
    /// </summary>
    public virtual long? Xj_GoodCode { get; set; }

    /// <summary>
    /// 单据类型（1:入库；2：取消入库）
    /// </summary>
    public virtual int? Xj_Type { get; set; }

    /// <summary>
    /// 已验证数量
    /// </summary>
    public virtual decimal? VerifiedQty { get; set; }

    /// <summary>
    /// 千克数量
    /// </summary>
    public virtual decimal? KilogramQty { get; set; }

}

/// <summary>
/// 入库单据-表体分页查询输入参数
/// </summary>
public class PageWmsImportNotifyDetailInput : BasePageInput
{
    /// <summary>
    /// 入库单据ID
    /// </summary>
    public long? ImportId { get; set; }

    /// <summary>
    /// 箱码编号
    /// </summary>
    public string? BoxCode { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterialName { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    public string? LotNo { get; set; }

    /// <summary>
    /// 选中主键列表
    /// </summary>
    public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 入库单据-表体增加输入参数
/// </summary>
public class AddWmsImportNotifyDetailInput
{
    /// <summary>
    /// 入库单据ID
    /// </summary>
    public long? ImportId { get; set; }

    /// <summary>
    /// 外部单据明细ID
    /// </summary>
    [MaxLength(32, ErrorMessage = "外部单据明细ID字符长度不能超过32")]
    public string? OuterDetailId { get; set; }

    /// <summary>
    /// 序号
    /// </summary>
    [MaxLength(32, ErrorMessage = "序号字符长度不能超过32")]
    public string? ImportListNo { get; set; }

    /// <summary>
    /// 物料信息
    /// </summary>
    public long? MaterialId { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    [MaxLength(32, ErrorMessage = "批次字符长度不能超过32")]
    public string? LotNo { get; set; }

    /// <summary>
    /// 物料状态
    /// </summary>
    [MaxLength(32, ErrorMessage = "物料状态字符长度不能超过32")]
    public string? MaterialStatus { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ImportProductionDate { get; set; }

    /// <summary>
    /// 失效日期
    /// </summary>
    public DateTime? ImportLostDate { get; set; }

    /// <summary>
    /// 计划数量
    /// </summary>
    public decimal? ImportQuantity { get; set; }

    /// <summary>
    /// 完成数量
    /// </summary>
    public decimal? ImportCompleteQuantity { get; set; }

    /// <summary>
    /// 组盘数量
    /// </summary>
    public decimal? ImportFactQuantity { get; set; }

    /// <summary>
    /// 已上传数量
    /// </summary>
    public decimal? UploadQuantity { get; set; }

    /// <summary>
    /// 执行状态
    /// </summary>
    [MaxLength(2, ErrorMessage = "执行状态字符长度不能超过2")]
    public string? ImportExecuteFlag { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(50, ErrorMessage = "备注字符长度不能超过50")]
    public string? Remark { get; set; }

    /// <summary>
    /// 打印标识，1打印，0不打印
    /// </summary>
    public int? PrintFlag { get; set; }

    /// <summary>
    /// 下发任务状态默认0，1下发成功
    /// </summary>
    public int? TaskStatus { get; set; } = 0;

    /// <summary>
    /// 标签判断，1已打印，2或空未打印
    /// </summary>
    public int? LabelJudgment { get; set; } = 2;

    /// <summary>
    /// 件数
    /// </summary>
    public decimal? outQty { get; set; }

    /// <summary>
    /// 巷道编码
    /// </summary>
    [MaxLength(32, ErrorMessage = "巷道编码字符长度不能超过32")]
    public string? LanewayCode { get; set; }

    /// <summary>
    /// 采浆公司
    /// </summary>
    [MaxLength(50, ErrorMessage = "采浆公司字符长度不能超过50")]
    public string? Xj_HouseCode { get; set; }

    /// <summary>
    /// 血浆件数
    /// </summary>
    public decimal? Xj_Qty { get; set; }

    /// <summary>
    /// 血浆重量
    /// </summary>
    public decimal? Xj_Weight { get; set; }

    /// <summary>
    /// 血浆 箱总数
    /// </summary>
    public decimal? Xj_BoxQty { get; set; }

    /// <summary>
    /// 血浆类型
    /// </summary>
    public long? Xj_GoodCode { get; set; }

    /// <summary>
    /// 单据类型（1:入库；2：取消入库）
    /// </summary>
    public int? Xj_Type { get; set; }

    /// <summary>
    /// 已验证数量
    /// </summary>
    public decimal? VerifiedQty { get; set; }

    /// <summary>
    /// 千克数量
    /// </summary>
    public decimal? KilogramQty { get; set; }

    public int? ImportTaskStatus { get; set; } = 0;
    public bool IsDelete { get; set; } = false;


}

/// <summary>
/// 入库单据-表体删除输入参数
/// </summary>
public class DeleteWmsImportNotifyDetailInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }

}

/// <summary>
/// 入库单据-表体更新输入参数
/// </summary>
public class UpdateWmsImportNotifyDetailInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }

    /// <summary>
    /// 入库单据ID
    /// </summary>    
    public long? ImportId { get; set; }

    /// <summary>
    /// 外部单据明细ID
    /// </summary>    
    [MaxLength(32, ErrorMessage = "外部单据明细ID字符长度不能超过32")]
    public string? OuterDetailId { get; set; }

    /// <summary>
    /// 序号
    /// </summary>    
    [MaxLength(32, ErrorMessage = "序号字符长度不能超过32")]
    public string? ImportListNo { get; set; }

    /// <summary>
    /// 物料信息
    /// </summary>    
    public long? MaterialId { get; set; }

    /// <summary>
    /// 批次
    /// </summary>    
    [MaxLength(32, ErrorMessage = "批次字符长度不能超过32")]
    public string? LotNo { get; set; }

    /// <summary>
    /// 物料状态
    /// </summary>    
    [MaxLength(32, ErrorMessage = "物料状态字符长度不能超过32")]
    public string? MaterialStatus { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>    
    public DateTime? ImportProductionDate { get; set; }

    /// <summary>
    /// 失效日期
    /// </summary>    
    public DateTime? ImportLostDate { get; set; }

    /// <summary>
    /// 计划数量
    /// </summary>    
    public decimal? ImportQuantity { get; set; }

    /// <summary>
    /// 完成数量
    /// </summary>    
    public decimal? ImportCompleteQuantity { get; set; }

    /// <summary>
    /// 组盘数量
    /// </summary>    
    public decimal? ImportFactQuantity { get; set; }

    /// <summary>
    /// 已上传数量
    /// </summary>    
    public decimal? UploadQuantity { get; set; }

    /// <summary>
    /// 执行状态
    /// </summary>    
    [MaxLength(2, ErrorMessage = "执行状态字符长度不能超过2")]
    public string? ImportExecuteFlag { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(50, ErrorMessage = "备注字符长度不能超过50")]
    public string? Remark { get; set; }

    /// <summary>
    /// 打印标识，1打印，0不打印
    /// </summary>    
    public int? PrintFlag { get; set; }

    /// <summary>
    /// 下发任务状态默认0，1下发成功
    /// </summary>    
    public int? TaskStatus { get; set; }

    /// <summary>
    /// 标签判断，1已打印，2或空未打印
    /// </summary>    
    public int? LabelJudgment { get; set; }

    /// <summary>
    /// 件数
    /// </summary>    
    public decimal? outQty { get; set; }

    /// <summary>
    /// 巷道编码
    /// </summary>    
    [MaxLength(32, ErrorMessage = "巷道编码字符长度不能超过32")]
    public string? LanewayCode { get; set; }

    /// <summary>
    /// 采浆公司
    /// </summary>    
    [MaxLength(50, ErrorMessage = "采浆公司字符长度不能超过50")]
    public string? Xj_HouseCode { get; set; }

    /// <summary>
    /// 血浆件数
    /// </summary>    
    public decimal? Xj_Qty { get; set; }

    /// <summary>
    /// 血浆重量
    /// </summary>    
    public decimal? Xj_Weight { get; set; }

    /// <summary>
    /// 血浆 箱总数
    /// </summary>    
    public decimal? Xj_BoxQty { get; set; }

    /// <summary>
    /// 血浆类型
    /// </summary>    
    public long? Xj_GoodCode { get; set; }

    /// <summary>
    /// 单据类型（1:入库；2：取消入库）
    /// </summary>    
    public int? Xj_Type { get; set; }

    /// <summary>
    /// 已验证数量
    /// </summary>    
    public decimal? VerifiedQty { get; set; }

    /// <summary>
    /// 千克数量
    /// </summary>    
    public decimal? KilogramQty { get; set; }
    public bool IsDelete { get; set; } = false;

}

/// <summary>
/// 入库单据-表体主键查询输入参数
/// </summary>
public class QueryByIdWmsImportNotifyDetailInput : DeleteWmsImportNotifyDetailInput
{
}

/// <summary>
/// 下拉数据输入参数
/// </summary>
public class DropdownDataWmsImportNotifyDetailInput
{
    /// <summary>
    /// 是否用于分页查询
    /// </summary>
    public bool FromPage { get; set; }
}

/// <summary>
/// 获取入库单明细列表输入参数（兼容JC35接口）
/// </summary>
public class GetImportBillDetailsListInput
{
    /// <summary>
    /// 入库单据ID（必需）
    /// </summary>
    public long? ImportId { get; set; }

    /// <summary>
    /// 批次号（可选）
    /// </summary>
    public string? LotNo { get; set; }

    /// <summary>
    /// 明细ID（可选）
    /// </summary>
    public long? Id { get; set; }
}

/// <summary>
/// 入库单据-表体数据导出实体
/// </summary>
public class ExportWmsImportNotifyDetailDto
{
    /// <summary>
    /// 入库单号 用于关联主表（与主表中的入库单号对应）
    /// </summary>
    [ExporterHeader("入库单号", Format = "", Width = 25, IsBold = true)]
    public string? IdentifyCode { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    [ExporterHeader("物料编码", Format = "", Width = 25, IsBold = true)]
    public string MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    [ExporterHeader("物料名称", Format = "", Width = 25, IsBold = true)]
    public string MaterialName { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    [ExporterHeader("物料规格", Format = "", Width = 25, IsBold = true)]
    public string MaterialStandard { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    [ExporterHeader("批次", Format = "", Width = 25, IsBold = true)]
    public string? LotNo { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [ExporterHeader("状态", Format = "", Width = 25, IsBold = true)]
    public string? MaterialStatus { get; set; }

    /// <summary>
    /// 计量单位
    /// </summary>
    [ExporterHeader("计量单位", Format = "", Width = 15, IsBold = true)]
    public string UnitName { get; set; }

    /// <summary>
    /// 计划数量
    /// </summary>
    [ExporterHeader("计划数量", Format = "", Width = 25, IsBold = true)]
    public decimal? ImportQuantity { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    [ExporterHeader("生产日期", Format = "", Width = 25, IsBold = true)]
    public DateTime? ImportProductionDate { get; set; }

    /// <summary>
    /// 有效期至
    /// </summary>
    [ExporterHeader("有效期至", Format = "", Width = 25, IsBold = true)]
    public DateTime? ImportLostDate { get; set; }

    /// <summary>
    /// 完成数量
    /// </summary>
    [ExporterHeader("完成数量", Format = "", Width = 25, IsBold = true)]
    public decimal? ImportCompleteQuantity { get; set; }

    /// <summary>
    /// 已组盘数量
    /// </summary>
    [ExporterHeader("已组盘数量", Format = "", Width = 25, IsBold = true)]
    public decimal? ImportFactQuantity { get; set; }

    /// <summary>
    /// 执行状态
    /// </summary>
    [ExporterHeader("执行状态", Format = "", Width = 25, IsBold = true)]
    public string? ImportExecuteFlag { get; set; }

    /// <summary>
    /// 箱数量
    /// </summary>
    [ExporterHeader("箱数量", Format = "", Width = 15, IsBold = true)]
    public string BoxQuantity { get; set; }

    /// <summary>
    /// 贴标
    /// </summary>
    [ExporterHeader("贴标", Format = "", Width = 15, IsBold = true)]
    public string Labeling { get; set; }

    /// <summary>
    /// 物料备注
    /// </summary>
    [ExporterHeader("物料备注", Format = "", Width = 25, IsBold = true)]
    public string? Remark { get; set; }
}

/// <summary>
/// 入库单据-表体数据导入实体
/// </summary>
/// <remarks>
/// SheetIndex = 0 表示第一个sheet（主表和明细表横向合并在一起）
/// 模板结构：Sheet 0=入库单导入模板（第1行：主表表头|空列|明细表表头，第2行：主表数据|空列|明细表数据），Sheet 1=下拉数据
/// 明细表数据从第1行读取表头（横向布局），从第2行开始读取数据
/// </remarks>
[ExcelImporter(SheetIndex = 0, IsOnlyErrorRows = true)]
public class ImportWmsImportNotifyDetailInput : BaseImportInput
{
    /// <summary>
    /// 入库单据ID（后台自动关联）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? ImportId { get; set; }

    /// <summary>
    /// 标识列 用于关联主表（与主表中的标识列对应）
    /// </summary>
    [ImporterHeader(Name = "标识列")]
    [ExporterHeader("标识列", Format = "", Width = 25, IsBold = true)]
    public string? IdentifyCode { get; set; }

    /// <summary>
    /// 外部单据明细ID（不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? OuterDetailId { get; set; }

    /// <summary>
    /// 序号（后台自动生成，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? ImportListNo { get; set; }

    /// <summary>
    /// 物料信息 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? MaterialId { get; set; }

    /// <summary>
    /// 物料编码（用户输入物料编码）
    /// </summary>
    [ImporterHeader(Name = "物料编码")]
    [ExporterHeader("物料编码", Format = "", Width = 25, IsBold = true)]
    public string MaterialCode { get; set; }

    /// <summary>
    /// 批次（只允许输入数字）
    /// </summary>
    [ImporterHeader(Name = "批次")]
    [ExporterHeader("批次", Format = "", Width = 25, IsBold = true)]
    public string? LotNo { get; set; }

    /// <summary>
    /// 状态（下拉框选择：待检、合格、不合格）
    /// </summary>
    [ImporterHeader(Name = "状态")]
    [ExporterHeader("状态", Format = "", Width = 25, IsBold = true)]
    public string? MaterialStatus { get; set; }

    /// <summary>
    /// 数量（只允许输入数字）
    /// </summary>
    [ImporterHeader(Name = "数量")]
    [ExporterHeader("数量", Format = "", Width = 25, IsBold = true)]
    public decimal? ImportQuantity { get; set; }

    /// <summary>
    /// 生产日期（只能输入日期）
    /// </summary>
    [ImporterHeader(Name = "生产日期")]
    [ExporterHeader("生产日期", Format = "", Width = 25, IsBold = true)]
    public DateTime? ImportProductionDate { get; set; }

    /// <summary>
    /// 有效期至（不允许修改，根据生产日期自动生成，生产日期+180天）
    /// </summary>
    [ImporterHeader(Name = "有效期至", IsAllowRepeat = false)]
    [ExporterHeader("有效期至", Format = "", Width = 25, IsBold = true)]
    public DateTime? ImportLostDate { get; set; }

    /// <summary>
    /// 完成数量（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public decimal? ImportCompleteQuantity { get; set; }

    /// <summary>
    /// 组盘数量（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public decimal? ImportFactQuantity { get; set; }

    /// <summary>
    /// 已上传数量（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public decimal? UploadQuantity { get; set; }

    /// <summary>
    /// 执行状态（后台自动赋值"01"，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? ImportExecuteFlag { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? Remark { get; set; }

    /// <summary>
    /// 打印标识，1打印，0不打印（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public int? PrintFlag { get; set; }

    /// <summary>
    /// 下发任务状态默认0，1下发成功（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public int? TaskStatus { get; set; }

    /// <summary>
    /// 标签判断，1已打印，2或空未打印（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public int? LabelJudgment { get; set; }

    /// <summary>
    /// 件数（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public decimal? outQty { get; set; }

    /// <summary>
    /// 巷道编码（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? LanewayCode { get; set; }

    /// <summary>
    /// 采浆公司（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public string? Xj_HouseCode { get; set; }

    /// <summary>
    /// 血浆件数（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public decimal? Xj_Qty { get; set; }

    /// <summary>
    /// 血浆重量（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public decimal? Xj_Weight { get; set; }

    /// <summary>
    /// 血浆 箱总数（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public decimal? Xj_BoxQty { get; set; }

    /// <summary>
    /// 血浆类型（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? Xj_GoodCode { get; set; }

    /// <summary>
    /// 单据类型（1:入库；2：取消入库）（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public int? Xj_Type { get; set; }

    /// <summary>
    /// 已验证数量（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public decimal? VerifiedQty { get; set; }

    /// <summary>
    /// 千克数量（后台自动赋值，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public decimal? KilogramQty { get; set; }

    /// <summary>
    /// 下发任务状态（后台自动赋值0，不需要用户输入）
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public int? ImportTaskStatus { get; set; }

}
