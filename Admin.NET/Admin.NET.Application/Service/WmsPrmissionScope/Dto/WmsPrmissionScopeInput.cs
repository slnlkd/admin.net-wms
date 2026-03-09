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
/// 仓储权限控制基础输入参数
/// </summary>
public class WmsPrmissionScopeBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 用户
    /// </summary>
    [Required(ErrorMessage = "用户不能为空")]
    public virtual long? UserId { get; set; }
    
    /// <summary>
    /// 功能名称
    /// </summary>
    
    [Required(ErrorMessage = "功能名称不能为空")]
    public virtual string? TableName { get; set; }
    
    /// <summary>
    /// 字段名
    /// </summary>
    
    [Required(ErrorMessage = "字段名不能为空")]
    public virtual string? FieldName { get; set; }
    
    /// <summary>
    /// 字段值
    /// </summary>
    
    [Required(ErrorMessage = "字段值不能为空")]
    public virtual string? FieldValue { get; set; }
    
}

/// <summary>
/// 仓储权限控制分页查询输入参数
/// </summary>
public class PageWmsPrmissionScopeInput : BasePageInput
{
    /// <summary>
    /// 用户
    /// </summary>
    public long? UserId { get; set; }
    
    /// <summary>
    /// 功能名称
    /// </summary>
    
    public string? TableName { get; set; }
    
    /// <summary>
    /// 字段名
    /// </summary>
    
    public string? FieldName { get; set; }
    
    /// <summary>
    /// 字段值
    /// </summary>
    
    public string? FieldValue { get; set; }
    /// <summary>
    /// 表功能名称
    /// </summary>

    public string TableDes { get; set; }
    /// <summary>
    /// 列名称
    /// </summary>
    public string FieldDes { get; set; }


}

/// <summary>
/// 仓储权限控制增加输入参数
/// </summary>
public class AddWmsPrmissionScopeInput
{
    /// <summary>
    /// 用户
    /// </summary>
    [Required(ErrorMessage = "用户不能为空")]
    public long? UserId { get; set; }
    
    /// <summary>
    /// 功能名称
    /// </summary>
    [Required(ErrorMessage = "功能名称不能为空")]
    [MaxLength(64, ErrorMessage = "功能名称字符长度不能超过64")]
    public string? TableName { get; set; }
    
    /// <summary>
    /// 字段名
    /// </summary>
    [Required(ErrorMessage = "字段名不能为空")]
    [MaxLength(64, ErrorMessage = "字段名字符长度不能超过64")]
    public string? FieldName { get; set; }
    
    /// <summary>
    /// 字段值
    /// </summary>
    [Required(ErrorMessage = "字段值不能为空")]
    [MaxLength(2048, ErrorMessage = "字段值字符长度不能超过2048")]
    public string? FieldValue { get; set; }
    /// <summary>
    /// 表功能名称
    /// </summary>

    public string TableDes { get; set; }
    /// <summary>
    /// 列名称
    /// </summary>
    public string FieldDes { get; set; }

}

/// <summary>
/// 仓储权限控制删除输入参数
/// </summary>
public class DeleteWmsPrmissionScopeInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 仓储权限控制更新输入参数
/// </summary>
public class UpdateWmsPrmissionScopeInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 用户
    /// </summary>    
    [Required(ErrorMessage = "用户不能为空")]
    public long? UserId { get; set; }
    
    /// <summary>
    /// 功能名称
    /// </summary>    
    
    [Required(ErrorMessage = "功能名称不能为空")]
    [MaxLength(64, ErrorMessage = "功能名称字符长度不能超过64")]
    public string? TableName { get; set; }
    
    /// <summary>
    /// 字段名
    /// </summary>    
    
    [Required(ErrorMessage = "字段名不能为空")]
    [MaxLength(64, ErrorMessage = "字段名字符长度不能超过64")]
    public string? FieldName { get; set; }
    
    /// <summary>
    /// 字段值
    /// </summary>    
    
    [Required(ErrorMessage = "字段值不能为空")]
    [MaxLength(2048, ErrorMessage = "字段值字符长度不能超过2048")]
    public string? FieldValue { get; set; }
    /// <summary>
    /// 表功能名称
    /// </summary>

    public string TableDes { get; set; }
    /// <summary>
    /// 列名称
    /// </summary>
    public string FieldDes { get; set; }

}

/// <summary>
/// 仓储权限控制主键查询输入参数
/// </summary>
public class QueryByIdWmsPrmissionScopeInput : DeleteWmsPrmissionScopeInput
{
}

/// <summary>
/// 下拉数据输入参数
/// </summary>
public class DropdownDataWmsPrmissionScopeInput
{
    /// <summary>
    /// 是否用于分页查询
    /// </summary>
    public bool FromPage { get; set; }
    /// <summary>
    /// 表名称
    /// </summary>
    public string TableName { get; set; }
}

