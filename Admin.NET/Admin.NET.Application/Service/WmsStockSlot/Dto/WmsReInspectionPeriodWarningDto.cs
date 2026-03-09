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
public class WmsReInspectionPeriodWarningDto : BasePageInput
{

    public System.DateTime MaterialValidityDay1{get;set;}

    public string GoodsAlarmDay{get;set;}

    public long Id{get;set;}

    public string MaterialId{get;set;}

    public string MaterialCode{get;set;}

    public string MaterialName{get;set;}

    public string MaterialStandard{get;set;}

    public string MaterialModel{get;set;}

    public string MaterialUnit{get;set;}

    public string UnitName{get;set;}

    public string ManageType{get;set;}

    public string WarehouseId{get;set;}

    public string SlotAreaId{get;set;}

    public string SlotCode{get;set;}

    public string StockStockCode{get;set;}

    public System.Nullable<decimal> StockQuantity{get;set;}

    public System.Nullable<System.DateTime> StockDate{get;set;}

    public System.Nullable<int> StockStatusFlag{get;set;}

    public string Remark{get;set;}

    public string LotNo{get;set;}

    public string WarehouseName{get;set;}

    public int DaysReamin{get;set;}

    public int DaysOverdue{get;set;}

    public int NactionStock{get;set;}

    public System.Nullable<System.DateTime> EndDate{get;set;}

    public System.Nullable<System.DateTime> ProductionDate{get;set;}

    public System.Nullable<System.DateTime> ValidateDay{get;set;}

    public System.Nullable<System.DateTime> RetestDate{get;set;}
}
