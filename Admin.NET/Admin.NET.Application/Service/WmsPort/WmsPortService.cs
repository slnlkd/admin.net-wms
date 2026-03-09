// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.ComponentModel;
using System.Threading.Tasks;

using Admin.NET.Application.Service.WmsBaseOperLog;
using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Application.Service.WmsPort.Process;
using Admin.NET.Core;
using Furion.FriendlyException;

namespace Admin.NET.Application.Service.WmsPort;

/// <summary>
/// WCS接口 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class WmsPortService : IDynamicApiController, ITransient
{
    private readonly PortImportOrder _createOrder;
    private readonly PortImportApply _importApply;
    private readonly PortFeedBack _taskFeedBack;
    private readonly PortExportEmpty _emptyTray;
    private readonly PortImportBind _emptyTrayBind;
    private readonly PortImportGroup _groupPallets;

    public WmsPortService(
        PortImportOrder createOrder,
        PortImportApply importApply,
        PortFeedBack taskFeedBack,
        PortExportEmpty emptyTray,
        PortImportBind emptyTrayBind,
        PortImportGroup groupPallets)
    {
        _createOrder = createOrder;
        _importApply = importApply;
        _taskFeedBack = taskFeedBack;
        _emptyTray = emptyTray;
        _emptyTrayBind = emptyTrayBind;
        _groupPallets = groupPallets;
    }

    /// <summary>
    /// 入库单据下发 ✅
    /// 重构自《WcsController》《CreateOrder》接口
    /// </summary>
    /// <param name="input">入库单据明细</param>
    /// <returns>返回是否成功和消息</returns>
    [DisplayName("入库单据下发")]
    [ApiDescriptionSettings(Name = "CreateOrder"), HttpPost]
    public async Task<CreateOrderOutput> CreateOrder(ImportNotifyDetail input)
    {
        return await _createOrder.ProcessCreateOrder(input);
    }

    /// <summary>
    /// 入库申请 ✅
    /// 重构自JC20《ImportOrderController》《CreateImportOrderNew》接口
    /// </summary>
    /// <param name="input">入库申请参数</param>
    /// <returns></returns>
    [DisplayName("入库申请")]
    [ApiDescriptionSettings(Name = "CreateImportOrder"), HttpPost]
    public async Task<ImportApplyOutput> CreateImportOrder(ImportApplyInput input)
    {
        return await _importApply.ProcessCreateImportOrder(input);
    }

    /// <summary>
    /// 二次入库申请 ✅
    /// 重构自JC20《ImportOrderController》《CreateImportOrderNew2》接口
    /// </summary>
    /// <param name="input">二次入库申请参数</param>
    /// <returns></returns>
    [DisplayName("二次入库申请")]
    [ApiDescriptionSettings(Name = "CreateImportOrder2"), HttpPost]
    public async Task<ImportApplyOutput> CreateImportOrder2(ImportApply2Input input)
    {
        return await _importApply.ProcessCreateImportOrder2(input);
    }

    /// <summary>
    /// 任务反馈 ✅
    /// 重构自JC20《WcsController》《ReceiveWcsSignal》接口
    /// </summary>
    /// <param name="input">任务反馈参数</param>
    /// <returns></returns>
    [DisplayName("任务反馈")]
    [ApiDescriptionSettings(Name = "TaskFeedback"), HttpPost]
    public async Task<TaskFeedbackOutput> TaskFeedback(TaskFeedbackInput input)
    {
        return await _taskFeedBack.ProcessTaskFeedback(input);
    }

    /// <summary>
    /// 组托反馈 ✅
    /// 重构自JC35《PortController》《GenerateGroupPallets》接口
    /// </summary>
    /// <param name="input">组托反馈参数（载具、单号、箱码明细）。</param>
    /// <returns>组托结果</returns>
    [DisplayName("组托反馈")]
    [ApiDescriptionSettings(Name = "GenerateGroupPallets"), HttpPost]
    public async Task<GroupPalletFeedbackOutput> GenerateGroupPallets(GroupPalletFeedbackInput input)
    {
        return await _groupPallets.ProcessGroupPalletsAsync(input);
    }

    /// <summary>
    /// 空托盘组盘 ✅
    /// 重构自JC35《PortController》《KBackConfirm》接口
    /// </summary>
    /// <param name="input">空托组盘参数（托盘编码、数量、操作类型）。</param>
    /// <returns>组盘结果</returns>
    [DisplayName("空载具组盘")]
    [ApiDescriptionSettings(Name = "KBackConfirm"), HttpPost]
    public async Task<EmptyTrayBindOutput> KBackConfirm(EmptyTrayBindInput input)
    {
        return await _emptyTrayBind.ProcessKBackConfirmAsync(input);
    }

    /// <summary>
    /// 空托盘申请 ✅
    /// 重构自JC35《PortController》《AddOutPalno》接口
    /// </summary>
    /// <param name="input">空托申请参数（数量、出库口、仓库类型）。</param>
    /// <returns>空托申请下发执行结果。</returns>
    /// <remarks>
    /// 映射旧系统 <c>PortController.AddOutPalno</c>：保留空托库存筛选、移库处理及 WCS 回写逻辑。
    /// </remarks>
    [DisplayName("空载具申请")]
    [ApiDescriptionSettings(Name = "AddOutPalno"), HttpPost]
    public async Task<EmptyTrayApplyOutput> AddOutPalno(EmptyTrayApplyInput input)
    {
        return await _emptyTray.ProcessEmptyTrayAsync(input);
    }
}
