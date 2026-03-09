// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;

namespace Admin.NET.Application.Service.WmsPort.Dto;

/******************************
* 创建人：wangyulong
* 创建时间：2022/1/24 14:16:24
******************************/
public class WcsApiUrlDto
{
    /// <summary>
    /// 获取Wcs接口地址主机部分
    /// </summary>
    /// <returns></returns>
    public static string GetHost()
    {
        var host = App.GetConfig<ExprotPort>("ExportPort", true);
        return host.WCS ?? string.Empty;
    }

    /// <summary>
    /// 获取追溯系统箱码关系地址
    /// </summary>
    /// <returns></returns>
    public static string GetBoxNoHost()
    {
        var host = App.GetConfig<string>("BoxNoUrlHost", true);
        return host ?? string.Empty;
    }

    /// <summary>
    /// 获取追溯系统箱码关系地址
    /// </summary>
    /// <returns></returns>
    public static string GetFLHost()
    {
        var host = App.GetConfig<string>("FLApiUrlHost", true);
        return host ?? string.Empty;
    }

    /// <summary>
    /// 空闲码垛机接口地址
    /// </summary>
    public static readonly string IdleStockerApiUrl = "/";

    /// <summary>
    /// 下发任务
    /// </summary>
    public static readonly string TaskApiUrl = "/api/wmsinterface/OutStockTask";

    /// <summary>
    /// 下发出库任务new
    /// </summary>
    public static readonly string TaskApiUrlNew = "/Wcs/WmsSendTask";

    /// <summary>
    /// 五楼回库物料调用wcs接口
    /// </summary>
    public static readonly string HuiKuUrl = "/Wcs/WmsPalletNobyTaskNo";

    /// <summary>
    /// 入库单据下发任务
    /// </summary>
    //public static readonly string PortCreateOrderUrl = "/api/wmsinterface/WmsCreateOrder";
    public static readonly string PortCreateOrderUrl = "/Wcs/WmsCreateOrder";

    /// <summary>
    /// 下发任务
    /// </summary>
    public static readonly string EmptyCallApiUrl = "/Wcs/EmptyCallForMaterial";

    /// <summary>
    /// 下发物料组
    /// </summary>
    public static readonly string WMSaterialLocation = "/Wcs/WmsMaterialLocationTask";

    /// <summary>
    /// 下发出库（给cs库位位置）
    /// </summary>
    public static readonly string OutBountApiUrl = "/";

    /// <summary>
    /// 备货组托获取箱明细
    /// </summary>
    public static readonly string GetBoxInfoApiUrl = "/api/wmsinterface/GetBoxInfo";

    /// <summary>
    /// 同步物料
    /// </summary>
    public static readonly string ImportNotifyDistributeApiUrl = "/api/wmsinterface/CreateOrder";

    /// <summary>
    /// 备料单据同步
    /// </summary>
    public static readonly string ChoiceTaskApiUrl = "/api/wmsinterface/ChoiceTask";

    /// <summary>
    /// 一楼接驳位空托入库绑卡
    /// </summary>
    public static readonly string SetPalletNobyPos = "/Wcs/SetPalletNobyPos";

    #region 富勒接口

    /// <summary>
    /// 物料主数据同步
    /// </summary>
    public static readonly string CRSJWCS01 = "/JsonApi?messageId=CRSJWCS01&sign=";

    /// <summary>
    /// 客商主数据同步
    /// </summary>
    public static readonly string CRSJWCS02 = "/JsonApi?messageId=CRSJWCS02&sign=";

    /// <summary>
    /// 自动码垛信息/拆垛信息反馈
    /// </summary>
    public static readonly string CRSJWCS05 = "/JsonApi?messageId=CRSJWCS05&sign=";

    /// <summary>
    /// 移动任务反馈
    /// </summary>
    public static readonly string CRSJWCS06 = "/JsonApi?messageId=CRSJWCS06&sign=";

    /// <summary>
    /// 托盘信息反馈
    /// </summary>
    public static readonly string CRSJWCS07 = "/JsonApi?messageId=CRSJWCS07&sign=";

    /// <summary>
    /// 人工拆垛信息/人工码垛信息
    /// </summary>
    public static readonly string CRSJWCS15 = "/JsonApi?messageId=CRSJWCS15&sign=";

    /// <summary>
    /// WCS设备状态同步(巷道状态同步)
    /// </summary>
    public static readonly string CRSJWCS09 = "/JsonApi?messageId=CRSJWCS09&sign=";

    #endregion
}

/// <summary>
/// 接口箱信息 
/// </summary>
public class WcsTaskBox
{
    public string Id { get; set; }

    public string ExportOrderId { get; set; }

    /// <summary>
    /// 箱条码
    /// </summary>
    public string BoxCode { get; set; }

    /// <summary>
    /// 箱数量
    /// </summary>
    public string Qty { get; set; }

    /// <summary>
    /// 拣货数量
    /// </summary>
    public string ScanQty { get; set; }

    public string GoodsCode { get; set; }

    public string StockLotNo { get; set; }
}

public class PutEmpty
{
    public string Pos { get; set; }

    public string PalletNo { get; set; }
}

