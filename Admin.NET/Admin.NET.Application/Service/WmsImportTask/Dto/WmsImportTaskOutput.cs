// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using Magicodes.ExporterAndImporter.Core;
namespace Admin.NET.Application;

/// <summary>
/// 入库任务表输出参数
/// </summary>
public class WmsImportTaskOutput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }    
    
    /// <summary>
    /// 任务号
    /// </summary>
    public string? TaskNo { get; set; }    
    
    /// <summary>
    /// 发送方
    /// </summary>
    public string? Sender { get; set; }    
    
    /// <summary>
    /// 接收方
    /// </summary>
    public string? Receiver { get; set; }    
    
    /// <summary>
    /// 是否成功
    /// </summary>
    public int? IsSuccess { get; set; }    
    
    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime? SendDate { get; set; }    
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? BackDate { get; set; }    
    
    /// <summary>
    /// 描述发送报文
    /// </summary>
    public string? Message { get; set; }    
    
    /// <summary>
    /// 托盘码
    /// </summary>
    public string? StockCode { get; set; }    
    
    /// <summary>
    /// 描述返回信息
    /// </summary>
    public string? Msg { get; set; }    
    
    /// <summary>
    /// 任务状态
    /// </summary>
    public int? Status { get; set; }    
    
    /// <summary>
    /// 取消时间
    /// </summary>
    public DateTime? CancelDate { get; set; }    
    
    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? FinishDate { get; set; }    
    
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? WareHouseId { get; set; }    
    
    /// <summary>
    /// 起始位置
    /// </summary>
    public string? StartLocation { get; set; }    
    
    /// <summary>
    /// 目标位置
    /// </summary>
    public string? EndLocation { get; set; }    
    
    /// <summary>
    /// 二次申请后目标位置
    /// </summary>
    public string? NewEnd { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    public int? IsDelete { get; set; }

    /// <summary>
    /// 创建者Id
    /// </summary>
    public long? CreateUserId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public string? CreateTime { get; set; }

    /// <summary>
    /// 修改者Id
    /// </summary>
    public long? UpdateUserId { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public string? UpdateTime { get; set; }

}

/// <summary>
/// 入库任务导出实体
/// </summary>
public class ExportWmsImportTaskDto
{
    /// <summary>
    /// 任务号
    /// </summary>
    [ExporterHeader("任务号", Format = "", Width = 25, IsBold = true)]
    public string? TaskNo { get; set; }

    /// <summary>
    /// 托盘码
    /// </summary>
    [ExporterHeader("托盘码", Format = "", Width = 20, IsBold = true)]
    public string? StockCode { get; set; }

    /// <summary>
    /// 起始位置
    /// </summary>
    [ExporterHeader("起始位置", Format = "", Width = 20, IsBold = true)]
    public string? StartLocation { get; set; }

    /// <summary>
    /// 目标位置
    /// </summary>
    [ExporterHeader("目标位置", Format = "", Width = 20, IsBold = true)]
    public string? EndLocation { get; set; }

    /// <summary>
    /// 二次申请位置
    /// </summary>
    [ExporterHeader("二次申请位置", Format = "", Width = 20, IsBold = true)]
    public string? NewEnd { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    [ExporterHeader("任务状态", Format = "", Width = 15, IsBold = true)]
    public string? Status { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    [ExporterHeader("是否成功", Format = "", Width = 15, IsBold = true)]
    public string? IsSuccess { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    [ExporterHeader("发送时间", Format = "yyyy-MM-dd HH:mm:ss", Width = 25, IsBold = true)]
    public DateTime? SendDate { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    [ExporterHeader("完成时间", Format = "yyyy-MM-dd HH:mm:ss", Width = 25, IsBold = true)]
    public DateTime? FinishDate { get; set; }

    /// <summary>
    /// 返回信息
    /// </summary>
    [ExporterHeader("返回信息", Format = "", Width = 30, IsBold = true)]
    public string? Msg { get; set; }

    /// <summary>
    /// 发送方
    /// </summary>
    [ExporterHeader("发送方", Format = "", Width = 15, IsBold = true)]
    public string? Sender { get; set; }

    /// <summary>
    /// 接收方
    /// </summary>
    [ExporterHeader("接收方", Format = "", Width = 15, IsBold = true)]
    public string? Receiver { get; set; }
}

/// <summary>
/// 入库任务表数据导入模板实体
/// </summary>
public class ExportWmsImportTaskOutput : ImportWmsImportTaskInput
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public override string Error { get; set; }
}
