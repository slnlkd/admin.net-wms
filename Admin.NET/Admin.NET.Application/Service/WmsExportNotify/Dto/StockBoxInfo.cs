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
public class StockBoxInfo
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
    public string MaterialCode { get; set; }
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
    /// <summary>
    /// RFID码
    /// </summary>
    public string RFIDCode { get; set; }
    /// <summary>
    /// 采样日期
    /// </summary>
    public string SamplingDate { get; set; }
    /// <summary>
    /// 浆员编码
    /// </summary>
    public string StaffCode { get; set; }
    /// <summary>
    /// 浆员姓名
    /// </summary>
    public string StaffName { get; set; }
    /// <summary>
    /// 是否挑浆（0:默认，1：挑浆）
    /// </summary>
    public string PickingSlurry { get; set; }
    /// <summary>
    /// 挑浆状态(1：检疫期满足，2：检疫期不满足，3：检疫期不合格 4.检验不合格)
    /// </summary>
    public string ExtractStatus { get; set; }
    /// <summary>
    /// 检验状态(0：待检 1：合格 2：不合格)
    /// </summary>
    public string InspectionStatus { get; set; }
}
