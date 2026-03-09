// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using Magicodes.ExporterAndImporter.Core;
namespace Admin.NET.Application;

/// <summary>
/// 入库单据-表体输出参数
/// </summary>
public class WmsImportNotifyDetailOutput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }    
    
    /// <summary>
    /// 入库单据ID
    /// </summary>
    public long? ImportId { get; set; }
    /// <summary>
    /// 所属仓库
    /// </summary>
    public long? WarehouseId { get; set; }
    /// <summary>
    /// 外部单据明细ID
    /// </summary>
    public string? OuterDetailId { get; set; }    
    /// <summary>
    /// 
    /// </summary>
    public string? ImportBillCode { get; set; }
    /// <summary>
    /// 序号
    /// </summary>
    public string? ImportListNo { get; set; }    
    
    /// <summary>
    /// 物料信息
    /// </summary>
    public long? MaterialId { get; set; }    
    
    /// <summary>
    /// 制造商
    /// </summary>
    public string? ManufacturerName { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialCode { get; set; }
    /// <summary>
    /// 物料名称
    /// </summary>
    public string MaterialName { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    public string MaterialStandard { get; set; }
    /// <summary>
    /// 载具类型
    /// </summary>
    public string Vehicle { get; set; }
    /// <summary>
    /// 箱数量
    /// </summary>
    public string BoxQuantity { get; set; }
    /// <summary>
    /// 贴标
    /// </summary>

    public string Labeling { get; set; }

    /// <summary>
    /// 计量单位
    /// </summary>
    public string UnitName { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    public string? LotNo { get; set; }    
    
    /// <summary>
    /// 物料状态
    /// </summary>
    public int? MaterialStatus { get; set; }    
    
    /// <summary>
    /// 生产日期
    /// </summary>
    public string ImportProductionDate { get; set; }    
    
    /// <summary>
    /// 失效日期
    /// </summary>
    public string ImportLostDate { get; set; }    
    
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
    public string? ImportExecuteFlag { get; set; }    
    
    /// <summary>
    /// 备注
    /// </summary>
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
    /// 软删除
    /// </summary>
    public bool IsDelete { get; set; }    
    
    /// <summary>
    /// 创建者Id
    /// </summary>
    public long? CreateUserId { get; set; }    
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public string CreateTime { get; set; }    
    
    /// <summary>
    /// 修改者Id
    /// </summary>
    public long? UpdateUserId { get; set; }    
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public string UpdateTime { get; set; }    
    
    /// <summary>
    /// 件数
    /// </summary>
    public decimal? outQty { get; set; }    
    
    /// <summary>
    /// 巷道编码
    /// </summary>
    public string? LanewayCode { get; set; }    
    
    /// <summary>
    /// 采浆公司
    /// </summary>
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
    
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }    
    
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }

    /// <summary>
    /// 下发任务状态
    /// </summary>
    public  int? ImportTaskStatus { get; set; }

}

/// <summary>
/// 入库单据-表体数据导入模板实体
/// </summary>
public class ExportWmsImportNotifyDetailOutput : ImportWmsImportNotifyDetailInput
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public override string Error { get; set; }
}
