// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Magicodes.ExporterAndImporter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.NET.Application;
public class WmsValidityWarningExportDto
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public virtual long Id { get; set; }

    /// <summary>
    /// 所属仓库
    /// </summary>
    [ImporterHeader(Name = "*所属仓库")]
    [ExporterHeader("*所属仓库", Format = "", Width = 25, IsBold = true)]
    public string? WarehouseName { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    [ImporterHeader(Name = "*物料编码")]
    [ExporterHeader("*物料编码", Format = "", Width = 25, IsBold = true)]
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    [ImporterHeader(Name = "*物料名称")]
    [ExporterHeader("*物料名称", Format = "", Width = 25, IsBold = true)]
    public string? MaterialName { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    [ImporterHeader(Name = "*物料规格")]
    [ExporterHeader("*物料规格", Format = "", Width = 25, IsBold = true)]
    public string? MaterialStandard { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    [ImporterHeader(Name = "*批次")]
    [ExporterHeader("*批次", Format = "", Width = 25, IsBold = true)]
    public string LotNo { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    [ImporterHeader(Name = "*库存数量")]
    [ExporterHeader("*库存数量", Format = "", Width = 25, IsBold = true)]
    public string StockQuantity { get; set; }

    /// <summary>
    /// 计量单位
    /// </summary>
    [ImporterHeader(Name = "*计量单位")]
    [ExporterHeader("*计量单位", Format = "", Width = 25, IsBold = true)]
    public string UnitName { get; set; }

    /// <summary>
    /// 储位编码
    /// </summary>
    [ImporterHeader(Name = "*储位编码")]
    [ExporterHeader("*储位编码", Format = "", Width = 25, IsBold = true)]
    public string SlotCode { get; set; }

    /// <summary>
    /// 托盘条码
    /// </summary>
    [ImporterHeader(Name = "*托盘条码")]
    [ExporterHeader("*托盘条码", Format = "", Width = 25, IsBold = true)]
    public string StockStockCode { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    [ImporterHeader(Name = "*生产日期")]
    [ExporterHeader("*生产日期", Format = "", Width = 25, IsBold = true)]
    public string ProductionDate { get; set; }

    /// <summary>
    /// 有效期
    /// </summary>
    [ImporterHeader(Name = "*有效期")]
    [ExporterHeader("*有效期", Format = "", Width = 25, IsBold = true)]
    public string ValidateDay { get; set; }

    /// <summary>
    /// 超出天数
    /// </summary>
    [ImporterHeader(Name = "*超出天数")]
    [ExporterHeader("*超出天数", Format = "", Width = 25, IsBold = true)]
    public string DaysOverdue { get; set; }
}

public class WmsValidityWarningLjExportDto
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public virtual long Id { get; set; }

    /// <summary>
    /// 所属仓库
    /// </summary>
    [ImporterHeader(Name = "*所属仓库")]
    [ExporterHeader("*所属仓库", Format = "", Width = 25, IsBold = true)]
    public string? WarehouseName { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    [ImporterHeader(Name = "*物料编码")]
    [ExporterHeader("*物料编码", Format = "", Width = 25, IsBold = true)]
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    [ImporterHeader(Name = "*物料名称")]
    [ExporterHeader("*物料名称", Format = "", Width = 25, IsBold = true)]
    public string? MaterialName { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    [ImporterHeader(Name = "*物料规格")]
    [ExporterHeader("*物料规格", Format = "", Width = 25, IsBold = true)]
    public string? MaterialStandard { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    [ImporterHeader(Name = "*批次")]
    [ExporterHeader("*批次", Format = "", Width = 25, IsBold = true)]
    public string LotNo { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    [ImporterHeader(Name = "*库存数量")]
    [ExporterHeader("*库存数量", Format = "", Width = 25, IsBold = true)]
    public string StockQuantity { get; set; }

    /// <summary>
    /// 计量单位
    /// </summary>
    [ImporterHeader(Name = "*计量单位")]
    [ExporterHeader("*计量单位", Format = "", Width = 25, IsBold = true)]
    public string UnitName { get; set; }

    /// <summary>
    /// 储位编码
    /// </summary>
    [ImporterHeader(Name = "*储位编码")]
    [ExporterHeader("*储位编码", Format = "", Width = 25, IsBold = true)]
    public string SlotCode { get; set; }

    /// <summary>
    /// 托盘条码
    /// </summary>
    [ImporterHeader(Name = "*托盘条码")]
    [ExporterHeader("*托盘条码", Format = "", Width = 25, IsBold = true)]
    public string StockStockCode { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    [ImporterHeader(Name = "*生产日期")]
    [ExporterHeader("*生产日期", Format = "", Width = 25, IsBold = true)]
    public string ProductionDate { get; set; }

    /// <summary>
    /// 有效期
    /// </summary>
    [ImporterHeader(Name = "*有效期")]
    [ExporterHeader("*有效期", Format = "", Width = 25, IsBold = true)]
    public string ValidateDay { get; set; }

    /// <summary>
    /// 剩余天数
    /// </summary>
    [ImporterHeader(Name = "*剩余天数")]
    [ExporterHeader("*剩余天数", Format = "", Width = 25, IsBold = true)]
    public string DaysReamin { get; set; }
}

public class WmsRetestWarningExportDto
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public virtual long Id { get; set; }

    /// <summary>
    /// 所属仓库
    /// </summary>
    [ImporterHeader(Name = "*所属仓库")]
    [ExporterHeader("*所属仓库", Format = "", Width = 25, IsBold = true)]
    public string? WarehouseName { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    [ImporterHeader(Name = "*物料编码")]
    [ExporterHeader("*物料编码", Format = "", Width = 25, IsBold = true)]
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    [ImporterHeader(Name = "*物料名称")]
    [ExporterHeader("*物料名称", Format = "", Width = 25, IsBold = true)]
    public string? MaterialName { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    [ImporterHeader(Name = "*物料规格")]
    [ExporterHeader("*物料规格", Format = "", Width = 25, IsBold = true)]
    public string? MaterialStandard { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    [ImporterHeader(Name = "*批次")]
    [ExporterHeader("*批次", Format = "", Width = 25, IsBold = true)]
    public string LotNo { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    [ImporterHeader(Name = "*库存数量")]
    [ExporterHeader("*库存数量", Format = "", Width = 25, IsBold = true)]
    public string StockQuantity { get; set; }

    /// <summary>
    /// 计量单位
    /// </summary>
    [ImporterHeader(Name = "*计量单位")]
    [ExporterHeader("*计量单位", Format = "", Width = 25, IsBold = true)]
    public string UnitName { get; set; }

    /// <summary>
    /// 储位编码
    /// </summary>
    [ImporterHeader(Name = "*储位编码")]
    [ExporterHeader("*储位编码", Format = "", Width = 25, IsBold = true)]
    public string SlotCode { get; set; }

    /// <summary>
    /// 托盘条码
    /// </summary>
    [ImporterHeader(Name = "*托盘条码")]
    [ExporterHeader("*托盘条码", Format = "", Width = 25, IsBold = true)]
    public string StockStockCode { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    [ImporterHeader(Name = "*生产日期")]
    [ExporterHeader("*生产日期", Format = "", Width = 25, IsBold = true)]
    public string ProductionDate { get; set; }

    /// <summary>
    /// 复验日期
    /// </summary>
    [ImporterHeader(Name = "*复验日期")]
    [ExporterHeader("*复验日期", Format = "", Width = 25, IsBold = true)]
    public string RetestDate { get; set; }

    /// <summary>
    /// 逾期天数
    /// </summary>
    [ImporterHeader(Name = "*逾期天数")]
    [ExporterHeader("*逾期天数", Format = "", Width = 25, IsBold = true)]
    public string DaysOverdue { get; set; }

    /// <summary>
    /// 剩余天数
    /// </summary>
    [ImporterHeader(Name = "*剩余天数")]
    [ExporterHeader("*剩余天数", Format = "", Width = 25, IsBold = true)]
    public string DaysReamin { get; set; }
}

public class WmsStockLowWarningExportDto
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public virtual long Id { get; set; }

    /// <summary>
    /// 所属仓库
    /// </summary>
    [ImporterHeader(Name = "*所属仓库")]
    [ExporterHeader("*所属仓库", Format = "", Width = 25, IsBold = true)]
    public string? WarehouseName { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    [ImporterHeader(Name = "*物料编码")]
    [ExporterHeader("*物料编码", Format = "", Width = 25, IsBold = true)]
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    [ImporterHeader(Name = "*物料名称")]
    [ExporterHeader("*物料名称", Format = "", Width = 25, IsBold = true)]
    public string? MaterialName { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    [ImporterHeader(Name = "*物料规格")]
    [ExporterHeader("*物料规格", Format = "", Width = 25, IsBold = true)]
    public string? MaterialStandard { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    [ImporterHeader(Name = "*库存数量")]
    [ExporterHeader("*库存数量", Format = "", Width = 25, IsBold = true)]
    public string StockQuantity { get; set; }

    /// <summary>
    /// 最低储量
    /// </summary>
    [ImporterHeader(Name = "*最低储量")]
    [ExporterHeader("*最低储量", Format = "", Width = 25, IsBold = true)]
    public string materialStockLow { get; set; }

    /// <summary>
    /// 低出数量
    /// </summary>
    [ImporterHeader(Name = "*低出数量")]
    [ExporterHeader("*低出数量", Format = "", Width = 25, IsBold = true)]
    public string MaterialLowCount { get; set; }

    /// <summary>
    /// 计量单位
    /// </summary>
    [ImporterHeader(Name = "*计量单位")]
    [ExporterHeader("*计量单位", Format = "", Width = 25, IsBold = true)]
    public string MaterialUnit { get; set; }
}

public class WmsStockHighWarningExportDto
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public virtual long Id { get; set; }

    /// <summary>
    /// 所属仓库
    /// </summary>
    [ImporterHeader(Name = "*所属仓库")]
    [ExporterHeader("*所属仓库", Format = "", Width = 25, IsBold = true)]
    public string? WarehouseName { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    [ImporterHeader(Name = "*物料编码")]
    [ExporterHeader("*物料编码", Format = "", Width = 25, IsBold = true)]
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    [ImporterHeader(Name = "*物料名称")]
    [ExporterHeader("*物料名称", Format = "", Width = 25, IsBold = true)]
    public string? MaterialName { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    [ImporterHeader(Name = "*物料规格")]
    [ExporterHeader("*物料规格", Format = "", Width = 25, IsBold = true)]
    public string? MaterialStandard { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    [ImporterHeader(Name = "*库存数量")]
    [ExporterHeader("*库存数量", Format = "", Width = 25, IsBold = true)]
    public string StockQuantity { get; set; }

    /// <summary>
    /// 最高储量
    /// </summary>
    [ImporterHeader(Name = "*最高储量")]
    [ExporterHeader("*最高储量", Format = "", Width = 25, IsBold = true)]
    public string MaterialStockHigh { get; set; }

    /// <summary>
    /// 高出数量
    /// </summary>
    [ImporterHeader(Name = "*高出数量")]
    [ExporterHeader("*高出数量", Format = "", Width = 25, IsBold = true)]
    public string MaterialHighCount { get; set; }

    /// <summary>
    /// 计量单位
    /// </summary>
    [ImporterHeader(Name = "*计量单位")]
    [ExporterHeader("*计量单位", Format = "", Width = 25, IsBold = true)]
    public string MaterialUnit { get; set; }
}
