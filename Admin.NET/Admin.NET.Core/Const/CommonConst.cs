// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Core;

/// <summary>
/// 通用常量
/// </summary>
[Const("平台配置")]
public class CommonConst
{
    /// <summary>
    /// PDA 出库有箱码拆垛业务
    /// </summary>
    public const string PdaExportBox = "PdaExportBox";
    /// <summary>
    /// PDA 出库无箱码拆垛业务
    /// </summary>
    public const string PdaExportNoBox = "PdaExportNoBox";
    /// <summary>
    /// 任务反馈
    /// </summary>
    public const string PortFeedback = "PortFeedback";
    /// <summary>
    /// PDA 出库业务
    /// </summary>
    public const string PdaExportBase = "PdaExportBase";
     /// <summary>
    /// PDA 出库业务
    /// </summary>
    public const string PdaExportProcess = "PdaExportProcess";
    /// <summary>
    /// PDA 盘库业务
    /// </summary>
    public const string PdaStockTake = "PdaStockTake";
    
     /// <summary>
    /// PDA 入库业务
    /// </summary>
    public const string PdaImportProcess = "PdaImportProcess";
    /// <summary>
    /// PDA 入库组托业务
    /// </summary>
    public const string PdaImportGroup = "PdaImportGroup";
    /// <summary>
    /// PDA 入库箱托关系业务
    /// </summary>
    public const string PdaImportBox = "PdaImportBox";
    /// <summary>
    /// PDA 入库空托盘业务
    /// </summary>
    public const string PdaImportEmpty = "PdaImportEmpty";
    /// <summary>
    /// PDA 入库杂项业务（托盘明细、暂存入库、叠箱绑定）
    /// </summary>
    public const string PdaImportMisc = "PdaImportMisc";
    /// <summary>
    /// 库位分配
    /// </summary>
    public const string PortSlotAlloc = "PortSlotAlloc";
    /// <summary>
    /// 组托反馈
    /// </summary>
    public const string PortImportGroup = "PortImportGroup";
    /// <summary>
    /// 空托绑定
    /// </summary>
    public const string PortImportBind = "PortImportBind";
    /// <summary>
    /// 空托出库
    /// </summary>
    public const string PortExportEmpty = "PortExportEmpty";
    /// <summary>
    /// 入库流水下发
    /// </summary>
    public const string PortImportOrder = "PortImportOrder";
    /// <summary>
    /// 入库申请
    /// </summary>
    public const string PortImportApply = "PortImportApply";
    
    /// <summary>
    /// 日志分组名称
    /// </summary>
    public const string SysLogCategoryName = "System.Logging.LoggingMonitor";

    /// <summary>
    /// 事件-增加异常日志
    /// </summary>
    public const string AddExLog = "Add:ExLog";

    /// <summary>
    /// 事件-发送异常邮件
    /// </summary>
    public const string SendErrorMail = "Send:ErrorMail";

    /// <summary>
    /// 默认基本角色名称
    /// </summary>
    public const string DefaultBaseRoleName = "默认基本角色";

    /// <summary>
    /// 默认基本角色编码
    /// </summary>
    public const string DefaultBaseRoleCode = "default_base_role";
}