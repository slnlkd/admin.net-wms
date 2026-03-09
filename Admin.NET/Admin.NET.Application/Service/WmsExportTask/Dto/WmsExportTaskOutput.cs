// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using Magicodes.ExporterAndImporter.Core;
namespace Admin.NET.Application;

/// <summary>
/// 出库任务输出参数
/// </summary>
public class WmsExportTaskOutput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }    
    
    /// <summary>
    /// 出库任务号
    /// </summary>
    public string? ExportTaskNo { get; set; }    
    
    /// <summary>
    /// 发送方
    /// </summary>
    public string? Sender { get; set; }    
    
    /// <summary>
    /// 接收方
    /// </summary>
    public string? Receiver { get; set; }    
    
    /// <summary>
    /// 是否成功（0否、1是）
    /// </summary>
    public int? IsSuccess { get; set; }    
    
    /// <summary>
    /// 信息
    /// </summary>
    public string? Information { get; set; }    
    
    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime? SendDate { get; set; }    
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? BackDate { get; set; }    
    
    /// <summary>
    /// 托盘条码
    /// </summary>
    public string? StockCode { get; set; }    
    
    /// <summary>
    /// 储位编码
    /// </summary>
    public string? StockSoltCode { get; set; }    
    
    /// <summary>
    /// 描述信息
    /// </summary>
    public string? Msg { get; set; }    
    
    /// <summary>
    /// 执行状态（0：已下发；1：执行中；2：执行完毕）
    /// </summary>
    public int? ExportTaskFlag { get; set; }    
    
    /// <summary>
    /// 出库流水号
    /// </summary>
    public string? ExportOrderNo { get; set; }    
    
    /// <summary>
    /// 起始位置
    /// </summary>
    public string? StartLocation { get; set; }    
    
    /// <summary>
    /// 目标位置
    /// </summary>
    public string? EndLocation { get; set; }    
    
    /// <summary>
    /// 仓库Id
    /// </summary>
    public long? WarehouseId { get; set; }    
    
    /// <summary>
    /// 1：出库；2：出库移库
    /// </summary>
    public int? TaskType { get; set; }    
    
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
    
}

/// <summary>
/// 出库任务数据导入模板实体
/// </summary>
public class ExportWmsExportTaskOutput : ImportWmsExportTaskInput
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public override string Error { get; set; }
}
