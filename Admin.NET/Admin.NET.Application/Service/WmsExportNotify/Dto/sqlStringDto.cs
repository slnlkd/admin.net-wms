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
public class sqlStringDto
{
    public string LanewayId { get; set; }

    public string SlotLanewayId {get; set; }

    public string Count { get; set; }

    public string StockQuantity { get; set; }

    public string SlotLayer { get; set; }


}

public class addreClass
{
    public string slotCode { get; set; }               // 主键ID
    public int? SlotRow { get; set; }                  // 行
    public int? SlotColumn { get; set; }               // 列
    public int? SlotLayer { get; set; }                // 层
    public int? distNum { get; set; }                  // 距离点位
}

public class listData1
{
    public string ExportStockCode { get; set; } 
    public string ExportSlotCode { get; set; } 
    
    public long? SlotLanewayId { get; set; } 
    
    public int? SlotInout { get; set; } 
    public long? ExportWarehouseId { get; set; } 
    public int? SlotLayer { get; set; }
}
