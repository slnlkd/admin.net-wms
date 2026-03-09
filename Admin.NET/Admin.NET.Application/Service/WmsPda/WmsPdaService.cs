// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护使用本项目应遵守相关法律法规和许可证的要求
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Application.Service.WmsPda.Dto;
using Admin.NET.Application.Service.WmsPda.Process;
using Admin.NET.Core;
using Admin.NET.Core.Service;
using DocumentFormat.OpenXml.Wordprocessing;
using Furion.FriendlyException;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using static SKIT.FlurlHttpClient.Wechat.Api.Models.WxaICPGetICPEntranceInfoResponse.Types.EntranceInfo.Types;

namespace Admin.NET.Application.Service.WmsPda;

/// <summary>
/// PDA相关服务（语音播报等辅助功能 + JC35 接口迁移）
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class WmsPdaService : IDynamicApiController, ITransient
{
    private readonly PdaImportGroup _importGroup;        // 入库组托
    private readonly PdaImportBox _importBox;            // 箱托关系
    private readonly PdaImportEmpty _importEmpty;        // 空托盘
    private readonly PdaImportMisc _importMisc;          // 暂存入库、托盘明细
    private readonly PdaExportBox _exportBox;            // 有箱码出库处理
    private readonly PdaExportNoBox _exportNoBox;        // 无箱码出库处理
    private readonly PdaExportEmpty _exportEmpty;        // 空托盘出库
    private readonly PdaStockTake _stocktake;            // 盘点处理

    public WmsPdaService(
        PdaImportGroup importGroup,
        PdaImportBox importBox,
        PdaImportEmpty importEmpty,
        PdaImportMisc importMisc,
        PdaExportBox exportBox,
        PdaExportNoBox exportNoBox,
        PdaExportEmpty exportEmpty,
        PdaStockTake stocktake)
    {
        _importGroup = importGroup;
        _importBox = importBox;
        _importEmpty = importEmpty;
        _importMisc = importMisc;
        _exportBox = exportBox;
        _exportNoBox = exportNoBox;
        _exportEmpty = exportEmpty;
        _stocktake = stocktake;
    }
    #region 入库

    /// <summary>
    /// JC35 PDA【/PdaInterface/GetStockSoltBoxInfo】→ Admin.NET「入库组托信息查询」
    /// </summary>
    /// <param name="input">托盘/物料条码查询参数</param>
    /// <returns>托盘下箱码列表</returns>
    /// <remarks>
    /// 原方法 <c>PdaInterfaceController.GetStockSoltBoxInfo</c>：
    /// <list type="bullet">
    /// <item>type≠1：按 “明细ID-物料ID-批次(-导入流水ID)” 解析条码，返回待绑定箱码</item>
    /// <item>type=1：按托盘号读取储位箱码，同时保留异常剔除仓位逻辑</item>
    /// </list>
    /// </remarks>
    [DisplayName("PDA入库组托信息查询")]
    [ApiDescriptionSettings(Name = "GetStockSoltBoxInfo"), HttpPost]
    public async Task<PdaStockSlotQueryOutput> GetStockSoltBoxInfo(PdaStockSlotQueryInput input)
    {
        return await _importGroup.QueryInboundGroupAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/GetScanQty】→ Admin.NET「无箱码组托数量计算」
    /// </summary>
    /// <param name="input">无箱码计算参数</param>
    /// <returns>推荐绑定数量</returns>
    /// <remarks>
    /// 原方法 <c>PdaInterfaceController.GetScanQty</c>：按条码拆分入库明细、物料、批次，结合基础数据的件数计算默认绑定数量
    /// </remarks>
    [DisplayName("PDA无箱码组托数量计算")]
    [ApiDescriptionSettings(Name = "GetScanQty"), HttpPost]
    public async Task<PdaScanQtyOutput> GetScanQty(PdaScanQtyInput input)
    {
        return await _importGroup.CalculateNoBoxQtyAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/SaveBoxInfo】→ Admin.NET「无箱码保存载具绑定」
    /// </summary>
    /// <param name="input">无箱码绑定参数</param>
    /// <returns>操作结果</returns>
    /// <remarks>
    /// 原方法 <c>PdaInterfaceController.SaveBoxInfo</c>：
    /// <list type="bullet">
    /// <item>校验托盘可用或人工码垛场景准入；</item>
    /// <item>写入入库流水、更新托盘状态与箱码明细；</item>
    /// <item>同步入库单明细与主单执行状态</item>
    /// </list>
    /// </remarks>
    [DisplayName("PDA无箱码组托保存")]
    [ApiDescriptionSettings(Name = "SaveBoxInfo"), HttpPost]
    public async Task<PdaActionResult> SaveBoxInfo(PdaSaveBoxInfoInput input)
    {
        return await _importGroup.SaveNoBoxBindingAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/GetPalnoDetailInfo】→ Admin.NET「托盘明细查询」
    /// </summary>
    /// <param name="input">托盘明细查询参数</param>
    /// <returns>托盘下箱码聚合结果</returns>
    [DisplayName("PDA托盘明细查询")]
    [ApiDescriptionSettings(Name = "GetPalnoDetailInfo"), HttpPost]
    public async Task<PdaPalletDetailOutput> GetPalnoDetailInfo(PdaPalletDetailInput input)
    {
        return await _importMisc.QueryPalletDetailAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/DelStockSoltBoxInfo】→ Admin.NET「删除单个箱码绑定」
    /// </summary>
    /// <param name="input">箱码删除参数</param>
    /// <returns>操作结果</returns>
    [DisplayName("PDA删除箱码绑定")]
    [ApiDescriptionSettings(Name = "DelStockSoltBoxInfo"), HttpPost]
    public async Task<PdaActionResult> DelStockSoltBoxInfo(PdaDelBoxInput input)
    {
        return await _importBox.DeletePalletBoxAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/RemoveBoxHolds】→ Admin.NET「托盘箱码解绑」
    /// </summary>
    /// <param name="input">解绑参数</param>
    /// <returns>操作结果</returns>
    [DisplayName("PDA托盘箱码解绑")]
    [ApiDescriptionSettings(Name = "RemoveBoxHolds"), HttpPost]
    public async Task<PdaActionResult> RemoveBoxHolds(PdaRemoveBoxInput input)
    {
        return await _importBox.RemoveBoxHoldsAsync(input);
    }
    /// <summary>
    /// JC35 PDA【/PdaInterface/GetImportInfoByBoxCode】→ Admin.NET「扫码获取箱托信息」
    /// </summary>
    /// <param name="input">箱码扫描参数</param>
    /// <returns>箱托信息及处理状态</returns>
    [DisplayName("PDA扫码获取箱托信息")]
    [ApiDescriptionSettings(Name = "GetImportInfoByBoxCode"), HttpPost]
    public async Task<PdaBoxInfoOutput> GetImportInfoByBoxCode(PdaBoxInfoInput input)
    {
        return await _importBox.QueryBoxInfoAsync(input);
    }
    /// <summary>
    /// JC35 PDA【/PdaInterface/GetPalnoStatus】→ Admin.NET「托盘状态查询」
    /// </summary>
    /// <param name="input">托盘状态查询参数</param>
    /// <returns>托盘状态</returns>
    [DisplayName("PDA托盘状态查询")]
    [ApiDescriptionSettings(Name = "GetPalnoStatus"), HttpPost]
    public async Task<PalletStatusOutput> GetPalnoStatus(PalletStatusInput input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.PalletCode))
        {
            throw Oops.Bah("载具编码不能为空！");
        }

        var exists = await _importBox.CheckPalletStatusAsync(input.PalletCode.Trim());
        return new PalletStatusOutput
        {
            Valid = exists,
            Message = exists ? "有效载具" : "无效载具"
        };
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/BackConfirm】→ Admin.NET「绑定箱托关系」
    /// </summary>
    /// <param name="input">绑定参数</param>
    /// <returns>操作结果</returns>
    /// <remarks>
    /// 原方法 <c>PdaInterfaceController.BackConfirm</c>：在 PDA 端确认箱托绑定，生成/更新入库流水并回写明细执行状态当前迁移版本聚焦标准入库场景
    /// </remarks>
    [DisplayName("PDA箱托绑定确认")]
    [ApiDescriptionSettings(Name = "BackConfirm"), HttpPost]
    public async Task<PdaActionResult> BackConfirm(PdaBindBoxInput input)
    {
        return await _importBox.BindBoxAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/KBackConfirm】→ Admin.NET「空托盘组盘」
    /// </summary>
    /// <param name="input">解绑参数</param>
    /// <returns>操作结果</returns>
    [DisplayName("PDA空托盘组盘")]
    [ApiDescriptionSettings(Name = "KBackConfirm"), HttpPost]
    public async Task<PdaActionResult> KBackConfirm(PdaEmptyTrayBindInput input)
    {
        return await _importEmpty.KBackConfirmAsync(input);
    }

    /// <summary>
    /// PDA「批量入库确认」
    /// </summary>
    /// <param name="input">批量确认参数，包含待绑定箱子列表和已修改数量的箱子</param>
    /// <returns>批量确认结果</returns>
    /// <remarks>
    /// 支持两种操作：
    /// <list type="bullet">
    /// <item>批量绑定新扫描的箱子到托盘</item>
    /// <item>批量更新已绑定箱子的数量</item>
    /// </list>
    /// </remarks>
    [DisplayName("PDA批量入库确认")]
    [ApiDescriptionSettings(Name = "BatchImportConfirm"), HttpPost]
    public async Task<PdaBatchImportConfirmOutput> BatchImportConfirm(PdaBatchImportConfirmInput input)
    {
        return await _importBox.BatchImportConfirmAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/AddWmsImportOrder】→ Admin.NET「批量生成空托入库数据」
    /// </summary>
    /// <returns>操作结果</returns>
    /// <remarks>
    /// 原方法 <c>PdaInterfaceController.AddWmsImportOrder</c>：遍历闲置托盘（尾号为奇数），为每个托盘生成空托流水与箱码
    /// </remarks>
    [DisplayName("PDA空载具批量组盘")]
    [ApiDescriptionSettings(Name = "AddWmsImportOrder"), HttpPost]
    public async Task<PdaActionResult> AddWmsImportOrder()
    {
        return await _importEmpty.BatchGenerateEmptyTrayOrdersAsync();
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/TemporaryStorage】→ Admin.NET「暂存入库生成」
    /// </summary>
    /// <param name="input">暂存入库参数</param>
    /// <returns>操作结果</returns>
    /// <remarks>
    /// 原方法 <c>PdaInterfaceController.TemporaryStorage</c>：基于最新出库流水，为主/副载具生成暂存入库流水并回写箱码关联
    /// </remarks>
    [DisplayName("PDA暂存入库")]
    [ApiDescriptionSettings(Name = "TemporaryStorage"), HttpPost]
    public async Task<PdaActionResult> TemporaryStorage(PdaTemporaryStorageInput input)
    {
        return await _importMisc.TemporaryStorageAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/StackingBindings】→ Admin.NET「叠箱绑定」
    /// </summary>
    /// <param name="input">叠箱绑定参数</param>
    /// <returns>操作结果</returns>
    /// <remarks>
    /// 原方法 <c>PdaInterfaceController.StackingBindings</c>：校验主/副载具最新入库流水是否同业务，并统一写入主载具编号
    /// </remarks>
    [DisplayName("PDA叠箱绑定")]
    [ApiDescriptionSettings(Name = "StackingBindings"), HttpPost]
    public async Task<PdaActionResult> StackingBindings(PdaStackingBindingInput input)
    {
        return await _importMisc.StackingBindingsAsync(input);
    }
    #endregion

    #region 出库

    /// <summary>
    /// JC35 PDA【/PdaInterface/RemoveManual】→ Admin.NET「删除拆跺信息」
    /// </summary>
    [DisplayName("PDA删除拆跺记录")]
    [ApiDescriptionSettings(Name = "RemoveManual"), HttpPost]
    public async Task<PdaActionResult> RemoveManual(PdaManualRemoveInput input)
    {
        return await _exportBox.RemoveManualAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/GetManBoxInfo】→ Admin.NET「拆跺箱码信息查询」
    /// </summary>
    [DisplayName("PDA拆跺箱码查询")]
    [ApiDescriptionSettings(Name = "GetManBoxInfo"), HttpPost]
    public async Task<PdaManualBoxInfoResult> GetManBoxInfo(PdaManualBoxQueryInput input)
    {
        return await _exportBox.GetManualBoxInfoAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/GetExprotCodeById】→ Admin.NET「托盘关联出库单查询」
    /// </summary>
    [DisplayName("PDA托盘出库单查询")]
    [ApiDescriptionSettings(Name = "GetExprotCodeById"), HttpPost]
    public async Task<PdaLegacyResult<List<PdaExportOrderItem>>> GetExprotCodeById(PdaExportOrderQueryInput input)
    {
        return await _exportBox.GetExportOrderByTrayAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/GetTrayInfo】→ Admin.NET「拆跺信息查询」
    /// </summary>
    [DisplayName("PDA拆跺信息查询")]
    [ApiDescriptionSettings(Name = "GetTrayInfo"), HttpPost]
    public async Task<PdaManualBoxInfoResult> GetTrayInfo(PdaManualDepalletizingQueryInput input)
    {
        return await _exportBox.GetTrayInfoAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/GetBoxSum】→ Admin.NET「拆跺数量计算」
    /// </summary>
    [DisplayName("PDA拆跺数量计算")]
    [ApiDescriptionSettings(Name = "GetBoxSum"), HttpPost]
    public async Task<PdaLegacyResult<PdaBoxListQtyOutput>> GetBoxSum(PdaBoxListQtyInput input)
    {
        return await _exportBox.CalculateBoxQuantityAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/AddManualDepalletizing】→ Admin.NET「拆跺箱码绑定」
    /// </summary>
    [DisplayName("PDA拆跺箱码绑定")]
    [ApiDescriptionSettings(Name = "AddManualDepalletizing"), HttpPost]
    public async Task<PdaActionResult> AddManualDepalletizing(PdaManualDepalletizingCreateInput input)
    {
        return await _exportBox.AddManualDepalletizingAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/GetManualDepalletizing】→ Admin.NET「拆跺记录查询」
    /// </summary>
    [DisplayName("PDA拆跺记录查询")]
    [ApiDescriptionSettings(Name = "GetManualDepalletizing"), HttpPost]
    public async Task<PdaLegacyResult<List<PdaManualDepalletizingItem>>> GetManualDepalletizing(PdaManualDepalletizingQueryInput input)
    {
        return await _exportBox.GetManualDepalletizingAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/SaveManTask】→ Admin.NET「人工拆垛出库确认」
    /// </summary>
    [DisplayName("PDA人工拆垛出库确认")]
    [ApiDescriptionSettings(Name = "SaveManTask"), HttpPost]
    public async Task<PdaLegacyResult<object>> SaveManTask(PdaManualTaskSaveInput input)
    {
        return await _exportBox.SaveManualTaskAsync(input);
    }


    /// <summary>
    /// JC35 PDA【/PdaInterface/AddOutManualDepalletizing】→ Admin.NET「无箱码拆跺绑定」
    /// </summary>
    [DisplayName("PDA无箱码拆跺绑定")]
    [ApiDescriptionSettings(Name = "AddOutManualDepalletizing"), HttpPost]
    public async Task<PdaActionResult> AddOutManualDepalletizing(PdaOutManualDepalletizingInput input)
    {
        return await _exportNoBox.AddOutManualDepalletizingAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/RemoveOutManual】→ Admin.NET「无箱码拆跺删除」
    /// </summary>
    [DisplayName("PDA删除无箱码拆跺")]
    [ApiDescriptionSettings(Name = "RemoveOutManual"), HttpPost]
    public async Task<PdaActionResult> RemoveOutManual(PdaManualRemoveInput input)
    {
        return await _exportNoBox.RemoveOutManualAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/GetOutManualDepalletizing】→ Admin.NET「无箱码拆跺查询」
    /// </summary>
    [DisplayName("PDA无箱码拆跺查询")]
    [ApiDescriptionSettings(Name = "GetOutManualDepalletizing"), HttpPost]
    public async Task<PdaLegacyResult<List<PdaManualDepalletizingItem>>> GetOutManualDepalletizing(PdaManualDepalletizingQueryInput input)
    {
        return await _exportNoBox.GetOutManualDepalletizingAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/OutSaveManTask】→ Admin.NET「无箱码人工拆垛出库确认」
    /// </summary>
    [DisplayName("PDA无箱码出库确认")]
    [ApiDescriptionSettings(Name = "OutSaveManTask"), HttpPost]
    public async Task<PdaLegacyResult<object>> OutSaveManTask(PdaOutManualTaskSaveInput input)
    {
        return await _exportNoBox.SaveOutManualTaskAsync(input);
    }
    /// <summary>
    /// JC35 PDA【/PdaInterface/AddOutPalno】→ Admin.NET「空托盘出库」
    /// </summary>
    [DisplayName("PDA空托盘出库申请")]
    [ApiDescriptionSettings(Name = "AddOutPalno"), HttpPost]
    public async Task<PdaLegacyResult<object>> AddOutPalno(PdaOutPalnoInput input)
    {
        return await _exportEmpty.ApplyPdaEmptyTrayAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/GetExportPortList】→ Admin.NET「出库口列表查询」
    /// </summary>
    [DisplayName("PDA出库口列表查询")]
    [ApiDescriptionSettings(Name = "GetExportPortList"), HttpPost]
    public async Task<PdaLegacyResult<List<PdaExportPortItem>>> GetExportPortList()
    {
        return await _exportEmpty.GetExportPortListAsync();
    }

    #endregion

    #region 盘点

    /// <summary>
    /// JC35 PDA【/PdaInterface/GetWmsStockCheckList】→ Admin.NET「盘点单列表查询」
    /// </summary>
    [DisplayName("PDA盘点单列表查询")]
    [ApiDescriptionSettings(Name = "GetWmsStockCheckList"), HttpPost]
    public async Task<PdaLegacyResult<List<PdaStockCheckOrderItem>>> GetWmsStockCheckList(PdaStockCheckListInput input)
    {
        return await _stocktake.GetStockCheckListAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/GetStockCheckInfo】→ Admin.NET「盘点箱码列表查询」
    /// </summary>
    [DisplayName("PDA盘点箱码查询")]
    [ApiDescriptionSettings(Name = "GetStockCheckInfo"), HttpPost]
    public async Task<PdaLegacyResult<List<PdaStockCheckBoxItem>>> GetStockCheckInfo(PdaStockCheckInfoInput input)
    {
        return await _stocktake.GetStockCheckInfoAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/UpdateStockCheckInfo】→ Admin.NET「盘点结果提交」
    /// </summary>
    [DisplayName("PDA盘点结果提交")]
    [ApiDescriptionSettings(Name = "UpdateStockCheckInfo"), HttpPost]
    public async Task<PdaActionResult> UpdateStockCheckInfo(PdaStockCheckUpdateInput input)
    {
        return await _stocktake.UpdateStockCheckInfoAsync(input);
    }
    /// <summary>
    /// JC35 PDA【/PdaInterface/IsEnableOkStockCode】→ Admin.NET「托盘验证」
    /// </summary>
    [DisplayName("PDA托盘验证")]
    [ApiDescriptionSettings(Name = "IsEnableOkStockCode"), HttpPost]
    public async Task<PdaLegacyResult<object>> IsEnableOkStockCode(PdaTrayValidateInput input)
    {
        return await _stocktake.ValidateTrayAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/GetMaterialInfoByStockCode】→ Admin.NET「根据托盘号获取物料信息」
    /// </summary>
    [DisplayName("PDA根据托盘号获取物料信息")]
    [ApiDescriptionSettings(Name = "GetMaterialInfoByStockCode"), HttpPost]
    public async Task<PdaLegacyResult<List<PdaMaterialInfoItem>>> GetMaterialInfoByStockCode(PdaMaterialInfoByStockCodeInput input)
    {
        return await _stocktake.GetMaterialInfoByStockCodeAsync(input);
    }

    /// <summary>
    /// JC35 PDA【/PdaInterface/SaveUnbindWithNoBoxCode】→ Admin.NET「无箱码托盘变更」
    /// </summary>
    [DisplayName("PDA无箱码托盘变更")]
    [ApiDescriptionSettings(Name = "SaveUnbindWithNoBoxCode"), HttpPost]
    public async Task<PdaLegacyResult<object>> SaveUnbindWithNoBoxCode(PdaTrayChangeInput input)
    {
        return await _stocktake.SaveTrayChangeAsync(input);
    }

    /// <summary>
    /// 更新单个箱码实际数量
    /// </summary>
    [DisplayName("PDA更新箱码实际数量")]
    [ApiDescriptionSettings(Name = "UpdateBoxRealQuantity"), HttpPost]
    public async Task<PdaActionResult> UpdateBoxRealQuantity(PdaUpdateBoxRealQuantityInput input)
    {
        return await _stocktake.UpdateBoxRealQuantityAsync(input);
    }
    #endregion

    #region 生成货位操作音频通知
    /// <summary>
    /// 生成货位操作音频通知
    /// </summary>
    /// <param name="locationCode">货位编号</param>
    /// <param name="operation">操作类型</param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "GenerateStorageAudio"), HttpGet]
    [AllowAnonymous]
    public IActionResult GenerateStorageAudio(string locationCode = "A00001", string operation = "入库")
    {
        string text = $"{locationCode}号货位{operation}";

        return GenerateStorageAudio(text);
    }

    /// <summary>
    /// 生成文本转音频
    /// </summary>
    /// <param name="text">要转换的文本</param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "GenerateTextAudio"), HttpGet]
    [AllowAnonymous]
    public IActionResult GenerateTextAudio(string text)
    {
        try
        {

            using (var synth = new SpeechSynthesizer())
            using (var stream = new MemoryStream())
            {
                // 设置输出到内存流
                synth.SetOutputToWaveStream(stream);

                // 设置中文语音（如果有的话）
                try
                {
                    // 尝试选择中文语音
                    var chineseVoices = synth.GetInstalledVoices()
                        .Where(v => v.VoiceInfo.Culture.Name.StartsWith("zh"));

                    if (chineseVoices.Any())
                    {
                        synth.SelectVoice(chineseVoices.First().VoiceInfo.Name);
                    }
                }
                catch
                {
                    // 使用默认语音
                }

                // 设置语速和音量（可选）
                synth.Rate = 0;    // -10 到 10，0为正常
                synth.Volume = 100; // 0 到 100

                // 生成语音
                synth.Speak(text);

                return new FileContentResult(stream.ToArray(), "audio/wav")
                {
                    FileDownloadName = "audio_notification.wav"
                };
            }

        }
        catch (Exception ex)
        {
            throw Oops.Oh($"生成音频失败: {ex.Message}");
        }
    }
    #endregion
}

