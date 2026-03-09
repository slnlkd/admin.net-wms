// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Admin.NET.Application.Service.WmsBaseOperLog.Enum;

/// <summary>
/// 操作类型枚举（字符串值）
/// </summary>
public enum OperationTypeEnum
{
    [Description("查询列表")]
    List,
    [Description("查询详情")]
    Detail,

    [Description("新增")]
    Add,

    [Description("修改")]
    Update,

    [Description("删除")]
    Delete,
    [Description("批量删除")]
    Deletes,
    [Description("审核")]
    Audit,

    [Description("提交")]
    Submit,

    [Description("取消")]
    Cancel,

    [Description("导入")]
    Import,

    [Description("导出")]
    Export,

    [Description("登录")]
    Login,

    [Description("登出")]
    Logout,
    [Description("反馈")]
    FeedBack,
    [Description("下发")]
    CreateOrder,
    [Description("作废")]
    Invalid


}

