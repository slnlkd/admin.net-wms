using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.NET.Application.Service.WmsDto;


/// <summary>
/// 出库Dto
/// </summary>
public class OutBaseDto
{

}


/// <summary>
/// wms下发wcs出库任务model
/// </summary>
public class WmsToWcsOutTaskModel
{
    /// <summary>
    /// 出库单号
    /// </summary>
    public string BillCode { get; set; }

    /// <summary>
    /// 箱码明细
    /// </summary>
    public List<StockBoxInfoDto> BoxInfos { get; set; } = [];

    /// <summary>
    /// 物料编码
    /// </summary>
    public string GoodsCode { get; set; }

    /// <summary>
    /// 出库数量
    /// </summary>
    public int GoodsQTY { get; set; } = 0;

    /// <summary>
    /// 所属仓库
    /// </summary>
    public string HouseCode { get; set; }

    /// <summary>
    /// 拣选数量
    /// </summary>
    public decimal Qty { get; set; }

    /// <summary>
    /// 托盘编码
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// 托盘数量
    /// </summary>
    public string StockNum { get; set; }

    /// <summary>
    /// 起始地址
    /// </summary>
    public string TaskBegin { get; set; }

    /// <summary>
    /// 目标地址
    /// </summary>
    public string TaskEnd { get; set; }

    /// <summary>
    /// 任务号
    /// </summary>
    public string TaskNo { get; set; }

    /// <summary>
    /// 出库类型，1分拣出库；2预拣；3周转库备料（自动）；4周转库备料（人工）
    /// </summary>
    public string OutType { get; set; } = "1";

    /// <summary>
    /// 任务类型
    /// </summary>
    public string TaskType { get; set; } = "1";
}


/// <summary>
/// 箱明细
/// </summary>
public class StockBoxInfoDto
{
    ///<summary>
    ///箱条码
    ///</summary>
    public string BoxCode { get; set; }

    ///<summary>
    ///数量
    ///</summary>
    public decimal Qty { get; set; }

    ///<summary>
    ///物料编码
    ///</summary>
    public string GoodCode { get; set; }

    ///<summary>
    ///生产日期
    ///</summary>
    public string ProductionDate { get; set; }

    ///<summary>
    ///保质期
    ///</summary>
    public string ValidateDay { get; set; }

    ///<summary>
    ///批次
    ///</summary>
    public string LotNo { get; set; }

    ///<summary>
    ///是否零箱（0否、1是）
    ///</summary>
    public int BulkTank { get; set; }

    ///<summary>
    ///任务ID
    ///</summary>
    public string TaskId { get; set; }

    /// <summary>
    /// 二维码
    /// </summary>
    public string QRCode { get; set; }

    /// <summary>
    /// RFID码
    /// </summary>
    public string RFIDCode { get; set; }
}


/// <summary>
/// 出库储位和托盘dto
/// </summary>
public class OutSlotAndStockDto
{
    /// <summary>
    /// 储位编码
    /// </summary>
    public string SlotCode { get; set; }

    /// <summary>
    /// 托盘码
    /// </summary>
    public string StockCode { get; set; }
}


/// <summary>
/// 出库储位dto
/// </summary>
public class OutRightSlotDto
{
    /// <summary>
    /// 出库数量
    /// </summary>
    public decimal ExportQty { get; set; } = 0M;

    /// <summary>
    /// 出库储位和托盘集合
    /// </summary>
    public IEnumerable<OutSlotAndStockDto> SlotAndStockIE { get; set; }
}


/// <summary>
/// 出库分配储位dto
/// </summary>
public class OutTaskModel
{
    /// <summary>
    /// 单号
    /// </summary>
    public string BillCode { get; set; }

    /// <summary>
    /// 出库流水号
    /// </summary>
    public string ExOrderNo { get; set; }

    /// <summary>
    /// 仓库id
    /// </summary>
    public string HouseId { get; set; }

    /// <summary>
    /// 仓库编码
    /// </summary>
    public string HouseCode { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string GoodsCode { get; set; }

    /// <summary>
    /// 储位编码
    /// </summary>
    public string SlotCode { get; set; }

    /// <summary>
    /// 托盘码
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// 出库数量
    /// </summary>
    public decimal ExQty { get; set; } = 0M;

    /// <summary>
    /// 整箱数量
    /// </summary>
    public decimal WholeBoxNum { get; set; } = 0M;
}