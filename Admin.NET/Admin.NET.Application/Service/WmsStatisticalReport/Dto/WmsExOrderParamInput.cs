// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.NET.Application;
public class WmsExOrderParamInput : BasePageInput
{
    /// <summary>
    /// 仓库Id
    /// </summary>
    public string WareHouseId { get; set; }

    /// <summary>
    /// 报表日期
    /// </summary>
    public string StartDate { get; set; }

    /// <summary>
    /// 报表日期
    /// </summary>
    public string EndDate { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialId { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string MaterialName { get; set; }

    /// <summary>
    /// 选中主键列表
    /// </summary>
    public List<long> SelectKeyList { get; set; }

    public string MaterialCode { get; set; }
}

public class WmsExOrderDetailParamInput : BasePageInput
{
    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialCode { get; set; }
    /// <summary>
    /// 物料名称
    /// </summary>
    public string MaterialName { get; set; }
    /// <summary>
    /// 批次
    /// </summary>
    public string LotNo { get; set; }
    /// <summary>
    /// 出库单据
    /// </summary>
    public string ExBillCode { get; set; }
    /// <summary>
    /// 托盘条码
    /// </summary>
    public string StockCode { get; set; }
    /// <summary>
    /// 储位编码
    /// </summary>
    public string SlotCode { get; set; }
    public string BoxCode { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
}

public class WmsExOrderBoxParamInput : BasePageInput
{
    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialCode { get; set; }
    /// <summary>
    /// 物料名称
    /// </summary>
    public string MaterialName { get; set; }
    /// <summary>
    /// 批次
    /// </summary>
    public string LotNo { get; set; }
    /// <summary>
    /// 出库单据
    /// </summary>
    public string ExBillCode { get; set; }
    /// <summary>
    /// 托盘条码
    /// </summary>
    public string StockCode { get; set; }
    /// <summary>
    /// 托盘条码
    /// </summary>
    public string SlotCode { get; set; }
    /// <summary>
    /// 箱码
    /// </summary>
    public string BoxCode { get; set; }
}

public class ExOrderDetailDto
{
    /// <summary>
    /// 出库流水单据
    /// </summary>
    public virtual string? ExportOrderNo { get; set; }

    /// <summary>
    /// 单据类型
    /// </summary>
    public virtual long? ExportBillType { get; set; }

    /// <summary>
    /// 物料ID
    /// </summary>
    public virtual long? ExportMaterialId { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public virtual string? ExportMaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public virtual string? ExportMaterialName { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    public virtual string? ExportMaterialStandard { get; set; }

    /// <summary>
    /// 物料型号
    /// </summary>
    public virtual string? ExportMaterialModel { get; set; }

    /// <summary>
    /// 物料类型
    /// </summary>
    public virtual string? ExportMaterialType { get; set; }

    /// <summary>
    /// 物料单位
    /// </summary>
    public virtual string? ExportMaterialUnit { get; set; }

    /// <summary>
    /// 仓库ID
    /// </summary>
    public virtual long? ExportWarehouseId { get; set; }

    /// <summary>
    /// 区域ID
    /// </summary>
    public virtual long? ExportAreaId { get; set; }

    /// <summary>
    /// 储位编码
    /// </summary>
    public virtual string? ExportSlotCode { get; set; }

    /// <summary>
    /// 托盘条码
    /// </summary>
    public virtual string? ExportStockCode { get; set; }

    /// <summary>
    /// 出库数量
    /// </summary>
    public virtual decimal? ExportQuantity { get; set; }

    /// <summary>
    /// 出库重量
    /// </summary>
    public virtual string? ExportWeight { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    public virtual DateTime? ExportProductionDate { get; set; }

    /// <summary>
    /// 失效日期
    /// </summary>
    public virtual DateTime? ExportLoseDate { get; set; }

    /// <summary>
    /// 部门编码
    /// </summary>
    public virtual long? ExportDepartmentCode { get; set; }

    /// <summary>
    /// 供应商编码
    /// </summary>
    public virtual long? ExpotSupplierCode { get; set; }

    /// <summary>
    /// 客户编码
    /// </summary>
    public virtual long? ExportCustomerCode { get; set; }

    /// <summary>
    /// 任务号
    /// </summary>
    public virtual string? ExportTaskNo { get; set; }

    /// <summary>
    /// 执行标志（0待执行、1正在执行、2已完成、3已上传）
    /// </summary>
    public virtual string ExportExecuteFlag { get; set; }

    /// <summary>
    /// 单据创建时间
    /// </summary>
    public virtual DateTime? ExporOrederDate { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? ExportRemark { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    public virtual string? ExportLotNo { get; set; }

    /// <summary>
    /// 出库单据
    /// </summary>
    public virtual string? ExportBillCode { get; set; }

    /// <summary>
    /// 出库序号
    /// </summary>
    public virtual string? ExportListNo { get; set; }

    /// <summary>
    /// 储位编码
    /// </summary>
    public virtual string? ExportStockSlotCode { get; set; }

    /// <summary>
    /// 扫码数量
    /// </summary>
    public virtual decimal? ScanQuantity { get; set; }

    /// <summary>
    /// 扫码人
    /// </summary>
    public virtual string? ScanUserNames { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public virtual DateTime? CompleteDate { get; set; }

    /// <summary>
    /// 拣货数量
    /// </summary>
    public virtual decimal? PickedNum { get; set; }

    /// <summary>
    /// 出库单据明细ID
    /// </summary>
    public virtual long? ExportDetailId { get; set; }

    /// <summary>
    /// 箱数
    /// </summary>
    public virtual string? WholeBoxNum { get; set; }

    /// <summary>
    /// 托盘类型
    /// </summary>
    public virtual int? OutType { get; set; }

    /// <summary>
    /// 托盘总箱数
    /// </summary>
    public virtual string? StockWholeBoxNum { get; set; }

    /// <summary>
    /// 托盘总数量
    /// </summary>
    public virtual string? StockQuantity { get; set; }

    /// <summary>
    /// 出库类型，0：正常出库，1：分拣出库，2：转运出库，3：托盘出库,4：备料出库
    /// </summary>
    public virtual int? ExportType { get; set; }

    /// <summary>
    /// 质检状态
    /// </summary>
    public virtual int? InspectionStatus { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    public virtual bool IsDelete { get; set; }

    /// <summary>
    /// 整托出库口
    /// </summary>
    public virtual string WholeOutWare { get; set; }

    /// <summary>
    /// 根据状态排序
    /// </summary>
    public virtual int OrderByStatus { get; set; }

    public virtual DateTime CreateTime { get; set; }
}

public class ExOrderBoxDto
{
    public string MaterialName{get;set;}

    public long Id{get;set;}

    public string BoxCode{get;set;}

    public System.Nullable<decimal> Qty{get;set;}

    public string StockCode{get;set;}

    public string ExportOrderNo{get;set;}

    public System.Nullable<int> IsOut{get;set;}

    public string MaterialId{get;set;}

    public string LotNo{get;set;}

    public System.Nullable<System.DateTime> AddDate{get;set;}

    public System.Nullable<int> Status{get;set;}

    public System.Nullable<System.DateTime> ProductionDate{get;set;}

    public System.Nullable<System.DateTime> ExportLoseDate{get;set;}

    public System.Nullable<bool> IsDel{get;set;}

    public string MaterialCode{get;set;}

    public System.Nullable<decimal> PickNum{get;set;}

    public System.Nullable<int> IsMustExport{get;set;}

    public string MaterialStandard{get;set;}

    public string UnitName{get;set;}

    public System.Nullable<decimal> PickEdNum{get;set;}
}