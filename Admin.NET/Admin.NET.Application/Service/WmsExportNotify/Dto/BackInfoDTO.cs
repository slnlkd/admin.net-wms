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
using Admin.NET.Application.Entity;

namespace Admin.NET.Application;
public class BackInfoDTO
{
    public WmsBaseSlot MoveSlot { get; set; }
    public YiKuModel YiKuSlot { get; set; }
    public string TaskNo { get; set; }
    public string TrayNo { get; set; }
    public string ExportNo { get; set; }
    public string ExportBillCode { get; set; }
}


public class YiKuModel
{
    /// <summary>
    /// 库房ID
    /// </summary>
    public long? WarehouseId { get; set; }

    /// <summary>
    /// 储位地址
    /// </summary>
    public string SlotCode { get; set; }

    /// <summary>
    /// 巷道ID
    /// </summary>
    public long? SlotLanewayId { get; set; }

    /// <summary>
    /// 储位状态
    /// </summary>
    public int? SlotStatus { get; set; }

    /// <summary>
    /// 深度
    /// </summary>
    public int? SlotInout { get; set; }

    /// <summary>
    /// 巷道类型，1:单口出入（先进后出）2：单口入单口出（先进先出，一个方向进一个方向出）3：双向出入（双向口）
    /// </summary>
    public string LanewayType { get; set; }

    public string EndTransitLocation { get; set; }

    public int SlotLayer { get; set; }

    /// <summary>
    /// 区域ID
    /// </summary>
    public long? SlotAreaId { get; set; }

}
