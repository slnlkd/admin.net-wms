using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.NET.Application.Service.WmsDto;


/// <summary>
/// 入库Dto
/// </summary>
public class InBaseDto
{

}


/// <summary>
/// wms下发wcs入库任务model
/// </summary>
public class WmsToWcsInTaskModel
{
    /// <summary>
    /// 任务号
    /// </summary>
    public string TaskNo { get; set; }

    /// <summary>
    /// 载具号
    /// </summary>
    public string StockCode { get; set; }

    /// <summary>
    /// 开始储位
    /// </summary>
    public string TaskBegin { get; set; }

    /// <summary>
    /// 结束储位
    /// </summary>
    public string TaskEnd { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string GoodsCode { get; set; }

    /// <summary>
    /// 入库数量
    /// </summary>
    public decimal GoodsQTY { get; set; }
}


/// <summary>
/// 入库分配储位dto
/// </summary>
public class InRightSlotDto
{
    /// <summary>
    /// 储位编码
    /// </summary>
    public string SlotStr { get; set; }

    /// <summary>
    /// 入库数量
    /// </summary>
    public IEnumerable<string> SqlList { get; set; }
}



