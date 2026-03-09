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
public class View_WmsBaseMaterial
{
    public long? Id{get; set;}
    public string? MaterialCode {get; set;}
    public string? MaterialName {get; set;}
    public string? MaterialMcode {get; set;}
    public string? MaterialStandard {get; set;}
    public string? MaterialModel {get; set;}
    public decimal? MaterialWeight {get; set;}

    public string? BsaeAreaName {get; set;}
    public long? MaterialType {get; set;}

    public string? MaterialTypename {get; set;}
    public string? MaterialValidityDay1 {get; set;}
    public string? MaterialValidityDay2 {get; set;}

    public string? MaterialValidityDay3 {get; set;}
    public long? MaterialUnit {get; set;}
    public string? UnitName {get; set;}
    public string? Remark {get; set;}
    public string? MaterialTemp {get; set;}
    public bool? IsDelete {get; set;}
    public string? MaterialAreaId {get; set;}
    public string? MaterialOrigin {get; set;}

    public long? CreateUserId {get; set;}
    public DateTime? CreateTime {get; set;}
    public long? UpdateUserId {get; set;}
    public DateTime? UpdateTime {get; set;}

    public string? CreateName {get; set;}
    public string? UpdateName {get; set;}

    public string? MaterialStockHigh {get; set;}
    public string? MaterialStockLow {get; set;}
    public long? WarehouseId {get; set;}
    public string? WarehouseName {get; set;}
    public string? MaterialAlarmDay {get; set;}
    public string? BoxQuantity {get; set;}

    public string? Labeling {get; set;}
    public string? EveryNumber {get; set;}
    public string? Vehicle {get; set;}
    public string? ManageType {get; set;}
    public string? OuterInnerCode { get; set; } 
}
