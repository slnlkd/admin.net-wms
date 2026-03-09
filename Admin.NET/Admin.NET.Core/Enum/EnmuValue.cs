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

namespace Admin.NET.Core;
public class EnmuValue
{
    /// <summary>
    /// 入库单:RKS;出库单:CKS;入库流水:RK;出库流水:CK;
    /// </summary>
    public enum InOutFlag
    {
        /// <summary>
        /// 入库单
        /// </summary>
        [Description("入库单")]
        RKS,
        /// <summary>
        /// 出库单
        /// </summary>
        [Description("出库单")]
        CKS,
        /// <summary>
        /// 入库流水
        /// </summary>
        [Description("入库流水")]
        RK,
        /// <summary>
        /// 出库流水
        /// </summary>
        [Description("出库流水")]
        CK,
        /// <summary>
        /// 移库单
        /// </summary>
        [Description("移库单")]
        YKS,
        /// <summary>
        /// 出库拼箱
        /// </summary>
        [Description("出库拼箱单")]
        CKPX,
        /// <summary>
        /// 拼箱明细
        /// </summary>
        [Description("拼箱明细单")]
        PXM,
        /// <summary>
        /// 拼箱流水
        /// </summary>
        [Description("拼箱流水号")]
        PXOR,
        /// <summary>
        /// 备货流水号
        /// </summary>
        [Description("备货流水号")]
        BH,
        /// <summary>
        /// 入库任务
        /// </summary>
        [Description("入库任务")]
        RKR,
        /// <summary>
        /// 出库任务
        /// </summary>
        [Description("出库任务")]
        CKR,
        /// <summary>
        /// 移库任务
        /// </summary>
        [Description("移库任务")]
        YKR,
        /// <summary>
        /// 接口任务
        /// </summary>
        [Description("接口任务")]
        IF,
        /// <summary>
        /// 挑浆单
        /// </summary>
        [Description("挑浆单")]
        TJS,
        /// <summary>
        /// 挑浆流水
        /// </summary>
        [Description("挑浆流水")]
        TJ,
        /// <summary>
        /// 挑浆任务
        /// </summary>
        [Description("挑浆任务")]
        TJR,
        /// <summary>
        /// 验收单
        /// </summary>
        [Description("验收单")]
        YSS,
        /// <summary>
        /// 验收流水
        /// </summary>
        [Description("验收流水")]
        YS,
        /// <summary>
        /// 验收任务
        /// </summary>
        [Description("验收任务")]
        YSR,
        /// <summary>
        /// 盘库单据
        /// </summary>
        [Description("盘库单据")]
        PKS,
        /// <summary>
        /// 盘库流水
        /// </summary>
        [Description("盘库流水")]
        PK,
        /// <summary>
        /// 盘库任务
        /// </summary>
        [Description("盘库任务")]
        PKR
    }
}
