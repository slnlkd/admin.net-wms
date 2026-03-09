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
/// 业务操作日志基础输入参数
/// </summary>
public class WmsBaseOperLogBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
}

/// <summary>
/// 业务操作日志分页查询输入参数
/// </summary>
public class PageWmsBaseOperLogInput : BasePageInput
{
    /// <summary>
    /// 关联的技术日志TraceId，用于开发人员追踪
    /// </summary>
    public string? TraceId { get; set; }
    
    /// <summary>
    /// 模块名称
    /// </summary>
    public string Module { get; set; }
    
    /// <summary>
    /// 操作类型：新增、修改、删除、审核等
    /// </summary>
    public string OperationType { get; set; }
    
    /// <summary>
    /// 操作人员
    /// </summary>
    public string Operator { get; set; }
    
    /// <summary>
    /// 操作时间范围
    /// </summary>
     public DateTime?[] OperateTimeRange { get; set; }
    
    /// <summary>
    /// 操作IP地址
    /// </summary>
    public string? OperateIp { get; set; }
    
    /// <summary>
    /// 业务单据号/ID
    /// </summary>
    public string? BusinessNo { get; set; }
    
    /// <summary>
    /// 操作内容（客户可读的详细描述）
    /// </summary>
    public string OperationContent { get; set; }
    
    /// <summary>
    /// 修改前数据摘要（客户可读格式）
    /// </summary>
    public string? BeforeDataSummary { get; set; }
    
    /// <summary>
    /// 修改后数据摘要（客户可读格式）
    /// </summary>
    public string? AfterDataSummary { get; set; }
    
    /// <summary>
    /// 操作结果：成功/失败
    /// </summary>
    public string Result { get; set; }
    
    /// <summary>
    /// 操作耗时（毫秒）
    /// </summary>
    public long? ElapsedMs { get; set; }
    
    /// <summary>
    /// 额外信息
    /// </summary>
    public string? ExtraInfo { get; set; }
    
    /// <summary>
    /// 隐藏的入参参数（JSON格式，开发人员使用，不对客户显示）
    /// </summary>
    public string? OperParam { get; set; }
    
}

/// <summary>
/// 业务操作日志增加输入参数
/// </summary>
public class AddWmsBaseOperLogInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 关联的技术日志TraceId，用于开发人员追踪
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// 模块名称
    /// </summary>
    public string Module { get; set; }

    /// <summary>
    /// 操作类型：新增、修改、删除、审核等
    /// </summary>
    public string OperationType { get; set; }

    /// <summary>
    /// 操作人员
    /// </summary>
    public string Operator { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    public DateTime OperateTime { get; set; }

    /// <summary>
    /// 操作IP地址
    /// </summary>
    public string? OperateIp { get; set; }

    /// <summary>
    /// 业务单据号/ID
    /// </summary>
    public string? BusinessNo { get; set; }

    /// <summary>
    /// 操作内容（客户可读的详细描述）
    /// </summary>
    public string OperationContent { get; set; }

    /// <summary>
    /// 修改前数据摘要（客户可读格式）
    /// </summary>
    public string? BeforeDataSummary { get; set; }

    /// <summary>
    /// 修改后数据摘要（客户可读格式）
    /// </summary>
    public string? AfterDataSummary { get; set; }

    /// <summary>
    /// 操作结果：成功/失败
    /// </summary>
    public string Result { get; set; }

    /// <summary>
    /// 操作耗时（毫秒）
    /// </summary>
    public long ElapsedMs { get; set; }

    /// <summary>
    /// 额外信息
    /// </summary>
    public string? ExtraInfo { get; set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 隐藏的入参参数（JSON格式，开发人员使用，不对客户显示）
    /// </summary>
    public string? OperParam { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime? UpdateTime { get; set; }
}

/// <summary>
/// 业务操作日志删除输入参数
/// </summary>
public class DeleteWmsBaseOperLogInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 业务操作日志更新输入参数
/// </summary>
public class UpdateWmsBaseOperLogInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 业务操作日志主键查询输入参数
/// </summary>
public class QueryByIdWmsBaseOperLogInput : DeleteWmsBaseOperLogInput
{
    /// <summary>
    /// 追踪ID
    /// </summary>
    public string TraceId { get; set; }
}

