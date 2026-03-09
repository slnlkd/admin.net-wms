// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.NET.Application;
public class WmsOrderParamInput : BasePageInput
{
    /// <summary>
    /// 仓库Id
    /// </summary>
    public string WareHouseId { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    public string StartDate { get; set; }

    /// <summary>
    /// 报表日期
    /// </summary>
    public string EndDate { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string MaterialName { get; set; }

    /// <summary>
    /// 选中主键列表
    /// </summary>
    public List<long> SelectKeyList { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialCode { get; set; }

    /// <summary>
    /// 物料Id
    /// </summary>
    public long? MaterialId { get; set; }

    /// <summary>
    /// 执行状态
    /// </summary>
    public string ImportExecuteFlag { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    public string LotNo { get; set; }

    /// <summary>
    /// 载具条码
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// 储位编码
    /// </summary>
    public string SlotCode { get; set; }
}

public class WmsOrderDetailParamInput : BasePageInput
{
    /// <summary>
    /// 储位编码
    /// </summary>
    public string SlotCode { get; set; }
    public string MaterialCode { get; set; }
    public string MaterialName { get; set; }
    public string LotNo { get; set; }
    public string ImportNo { get; set; }
    /// <summary>
    /// 载具条码
    /// </summary>
    public string StockCode { get; set; }
    /// <summary>
    /// 执行状态
    /// </summary>
    public string ImportExecuteFlag { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
}

public class WmsImOrderBoxParamInput : BasePageInput
{
    //public string LotNo { get; set; }
    //public string ImBillCode { get; set; }
    public string ImportOrderNo { get; set; }
    public string StockCode { get; set; }
    public string SlotCode { get; set; }
    public string BoxCode { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
}