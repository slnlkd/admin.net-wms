// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using Magicodes.ExporterAndImporter.Core;
namespace Admin.NET.Application;

/// <summary>
/// 供应商信息输出参数
/// </summary>
public class WmsBaseSupplierOutput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }    
    
    /// <summary>
    /// 供应商编码
    /// </summary>
    public string? CustomerCode { get; set; }    
    
    /// <summary>
    /// 供应商名称
    /// </summary>
    public string? CustomerName { get; set; }    
    
    /// <summary>
    /// 供应商地址
    /// </summary>
    public string? CustomerAddress { get; set; }    
    
    /// <summary>
    /// 联系人
    /// </summary>
    public string? CustomerLinkman { get; set; }    
    
    /// <summary>
    /// 联系电话
    /// </summary>
    public string? CustomerPhone { get; set; }    
    
    /// <summary>
    /// 客户类型
    /// </summary>
    public string? CustomerTypeId { get; set; }    
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }    
    
    /// <summary>
    /// 软删除
    /// </summary>
    public bool IsDelete { get; set; }    
    
    /// <summary>
    /// 授权编码
    /// </summary>
    public string? token { get; set; }    
    
    /// <summary>
    /// 授权用户
    /// </summary>
    public string? accountExec { get; set; }    
    
    /// <summary>
    /// 状态 0启用，1禁用
    /// </summary>
    public int? Status { get; set; }    
    
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
    
}

/// <summary>
/// 供应商信息数据导入模板实体
/// </summary>
public class ExportWmsBaseSupplierOutput : ImportWmsBaseSupplierInput
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public override string Error { get; set; }
}
