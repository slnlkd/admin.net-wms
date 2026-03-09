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
/// 申请客户信息基础输入参数
/// </summary>
public class WmsBaseCustomerPresetBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 客户编码
    /// </summary>
    public virtual string? CustomerCode { get; set; }
    
    /// <summary>
    /// 客户名称
    /// </summary>
    public virtual string? CustomerName { get; set; }
    
    /// <summary>
    /// 客户地址
    /// </summary>
    public virtual string? CustomerAddress { get; set; }
    
    /// <summary>
    /// 联系人
    /// </summary>
    public virtual string? CustomerLinkman { get; set; }
    
    /// <summary>
    /// 联系电话
    /// </summary>
    public virtual string? CustomerPhone { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remark { get; set; }
    
}

/// <summary>
/// 申请客户信息分页查询输入参数
/// </summary>
public class PageWmsBaseCustomerPresetInput : BasePageInput
{
    /// <summary>
    /// 客户编码
    /// </summary>
    public string? CustomerCode { get; set; }
    
    /// <summary>
    /// 客户名称
    /// </summary>
    public string? CustomerName { get; set; }
    
    /// <summary>
    /// 客户地址
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
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
    
    /// <summary>
    /// 审核状态
    /// </summary>
    [Dict("ApprovalStatus", AllowNullValue=true)]
    public int? ApprovalStatus { get; set; }
    
    /// <summary>
    /// 审核人
    /// </summary>
    public string? ApprovalUserName { get; set; }
    
    /// <summary>
    /// 审核备注
    /// </summary>
    public string? ApprovalRemark { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 申请客户信息增加输入参数
/// </summary>
public class AddWmsBaseCustomerPresetInput
{
    /// <summary>
    /// 客户编码
    /// </summary>
    [MaxLength(32, ErrorMessage = "客户编码字符长度不能超过32")]
    public string? CustomerCode { get; set; }
    
    /// <summary>
    /// 客户名称
    /// </summary>
    [MaxLength(32, ErrorMessage = "客户名称字符长度不能超过32")]
    public string? CustomerName { get; set; }
    
    /// <summary>
    /// 客户地址
    /// </summary>
    [MaxLength(32, ErrorMessage = "客户地址字符长度不能超过32")]
    public string? CustomerAddress { get; set; }
    
    /// <summary>
    /// 联系人
    /// </summary>
    [MaxLength(32, ErrorMessage = "联系人字符长度不能超过32")]
    public string? CustomerLinkman { get; set; }
    
    /// <summary>
    /// 联系电话
    /// </summary>
    [MaxLength(32, ErrorMessage = "联系电话字符长度不能超过32")]
    public string? CustomerPhone { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(32, ErrorMessage = "备注字符长度不能超过32")]
    public string? Remark { get; set; }
    
}

/// <summary>
/// 申请客户信息删除输入参数
/// </summary>
public class DeleteWmsBaseCustomerPresetInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 申请客户信息更新输入参数
/// </summary>
public class UpdateWmsBaseCustomerPresetInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 客户编码
    /// </summary>    
    [MaxLength(32, ErrorMessage = "客户编码字符长度不能超过32")]
    public string? CustomerCode { get; set; }
    
    /// <summary>
    /// 客户名称
    /// </summary>    
    [MaxLength(32, ErrorMessage = "客户名称字符长度不能超过32")]
    public string? CustomerName { get; set; }
    
    /// <summary>
    /// 客户地址
    /// </summary>    
    [MaxLength(32, ErrorMessage = "客户地址字符长度不能超过32")]
    public string? CustomerAddress { get; set; }
    
    /// <summary>
    /// 联系人
    /// </summary>    
    [MaxLength(32, ErrorMessage = "联系人字符长度不能超过32")]
    public string? CustomerLinkman { get; set; }
    
    /// <summary>
    /// 联系电话
    /// </summary>    
    [MaxLength(32, ErrorMessage = "联系电话字符长度不能超过32")]
    public string? CustomerPhone { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>    
    [MaxLength(32, ErrorMessage = "备注字符长度不能超过32")]
    public string? Remark { get; set; }
    
}

/// <summary>
/// 申请客户信息主键查询输入参数
/// </summary>
public class QueryByIdWmsBaseCustomerPresetInput : DeleteWmsBaseCustomerPresetInput
{
}

/// <summary>
/// 申请客户信息数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportWmsBaseCustomerPresetInput : BaseImportInput
{
    /// <summary>
    /// 客户编码
    /// </summary>
    [ImporterHeader(Name = "客户编码")]
    [ExporterHeader("客户编码", Format = "", Width = 25, IsBold = true)]
    public string? CustomerCode { get; set; }
    
    /// <summary>
    /// 客户名称
    /// </summary>
    [ImporterHeader(Name = "客户名称")]
    [ExporterHeader("客户名称", Format = "", Width = 25, IsBold = true)]
    public string? CustomerName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? Remark { get; set; }
    
    /// <summary>
    /// 审核状态 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public int? ApprovalStatus { get; set; }
    
    /// <summary>
    /// 审核状态 文本
    /// </summary>
    [Dict("ApprovalStatus")]
    [ImporterHeader(Name = "审核状态")]
    [ExporterHeader("审核状态", Format = "", Width = 25, IsBold = true)]
    public string ApprovalStatusDictLabel { get; set; }
    
    /// <summary>
    /// 审核人
    /// </summary>
    [ImporterHeader(Name = "审核人")]
    [ExporterHeader("审核人", Format = "", Width = 25, IsBold = true)]
    public string? ApprovalUserName { get; set; }
    
    /// <summary>
    /// 审核ID
    /// </summary>
    [ImporterHeader(Name = "审核ID")]
    [ExporterHeader("审核ID", Format = "", Width = 25, IsBold = true)]
    public long? ApprovalUserId { get; set; }
    
    /// <summary>
    /// 审核备注
    /// </summary>
    [ImporterHeader(Name = "审核备注")]
    [ExporterHeader("审核备注", Format = "", Width = 25, IsBold = true)]
    public string? ApprovalRemark { get; set; }
        
    }
    
    /// <summary>
    /// 申请客户信息审核输入参数
    /// </summary>
    public class ApprovalWmsBaseCustomerPresetInput
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [Required(ErrorMessage = "主键Id不能为空")]
        public long? Id { get; set; }
    
        /// <summary>
        /// 审核状态,1=通过，2=驳回
        /// </summary>
        [Required(ErrorMessage = "审核状态不能为空")]
        public int ApprovalStatus { get; set; }
    
        /// <summary>
        /// 审核备注
        /// </summary>
        public string ApprovalRemark { get; set; }
    }
    
    /// <summary>
    /// 申请客户信息批量审核输入参数
    /// </summary>
    public class BatchApprovalWmsBaseCustomerPresetInput
    {
        /// <summary>
        /// 主键Id列表
        /// </summary>
        [Required(ErrorMessage = "主键Id列表不能为空")]
        public List<long> Ids { get; set; }
    
        /// <summary>
        /// 审核状态,1=通过，2=驳回
        /// </summary>
        [Required(ErrorMessage = "审核状态不能为空")]
        public int ApprovalStatus { get; set; }
    
        /// <summary>
        /// 审核备注
        /// </summary>
        public string ApprovalRemark { get; set; }
    }
