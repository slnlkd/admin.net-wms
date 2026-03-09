using Admin.NET.Application.Service.BaseService;
using Admin.NET.Application.Service.WmsDto;
using Admin.NET.Core;
using Dm.util;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using NewLife;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SKIT.FlurlHttpClient.Wechat.Api.Models.ChannelsECQICInspectSubmitRequest.Types;
using static SKIT.FlurlHttpClient.Wechat.TenpayV3.Models.GetMarketingMemberCardOpenCardByCardIdResponse.Types;


namespace Admin.NET.Application.Service.OutBase;


/// <summary>
/// 出库
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class OutBaseService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarClient sqlClient;
    private readonly ILogger logger;


    public OutBaseService(ISqlSugarClient SqlSugarClient, ILoggerFactory loggerFactory)
    {
        sqlClient = SqlSugarClient;
        logger = loggerFactory.CreateLogger("出库日志");
    }


    /// <summary>
    /// 立体库出库自动分配
    /// </summary>
    /// <param name="billCode">要出库的流水id，1,2,3</param>
    /// <param name="userId">目的工位</param>
    /// <returns></returns>
    [DisplayName("立体库出库自动分配")]
    [ApiDescriptionSettings(Name = "OutWareHouseAutoStock"), HttpPost]
    public List<OutTaskModel> OutWareHouseAutoStock(string billCode, string userId)
    {
        List<OutTaskModel> result = [];
        if (string.IsNullOrWhiteSpace(billCode) || string.IsNullOrWhiteSpace(userId))
        {
            throw Oops.Bah("出库参数有空值，请修改后重试");
        }
        //用户信息
        string userSql = $@"select Id,RealName 
	from SysUser
	where Id={userId}";
        DataTable userDt = sqlClient.Ado.GetDataTable(userSql);
        if (userDt == null || userDt.Rows.Count < 1)
        {
            throw Oops.Bah("未找到有效用户信息");
        }

        //出库单据
        string exNotifySql = $@"select a.Id,a.WarehouseId,a.ExportExecuteFlag,a.ExportBillCode,a.ExportBillType,b.ProxyStatus,a.ExportCustomerId,a.WholeOutWare
	from WmsExportNotify a
	left join WmsBaseBillType b on b.Id=a.ExportBillType
	where a.IsDelete=0 and a.ExportExecuteFlag=0 and a.ExportBillCode='{billCode}'";
        DataTable exNotifyDt = sqlClient.Ado.GetDataTable(exNotifySql);
        if (exNotifyDt == null || exNotifyDt.Rows.Count < 1)
        {
            throw Oops.Bah("未找到指定出库单");
        }

        //出库单据详情
        string exNoDetailSql = $@"select a.Id,a.ExportBillCode,a.MaterialId,a.MaterialCode,a.LotNo,isnull(a.InspectionStatus,'') as zjStatus,isnull(a.ExportQuantity,0) as exQty,isnull(a.AllocateQuantity,0) as allocateQty,a.WarehouseId,b.MaterialName,b.MaterialStandard,b.MaterialModel,b.MaterialType,b.MaterialUnit
	from WmsExportNotifyDetail a
	left join WmsBaseMaterial b on b.Id=a.MaterialId
	where a.IsDelete=0 and a.ExportDetailFlag=0 and ExportBillCode={billCode}
	order by a.ExportBillCode desc,a.Id";
        DataTable exNoDetailDt = sqlClient.Ado.GetDataTable(exNoDetailSql);
        if (exNoDetailDt == null || exNoDetailDt.Rows.Count < 1)
        {
            throw Oops.Bah("未找到指定出库单明细");
        }

        string goodsId = "", goodsCode = "", lotNo = "", inspection = "";
        decimal exQty = 0M, allocateQty = 0M;

        //要出库托盘的所有位置
        List<OutSlotAndStockDto> exList = [];
        //要执行的sql
        List<string> sqlList = [];
        //出库单
        DataRow exNotifyDr = exNotifyDt.Rows[0];
        string exCustomer = exNotifyDr["ExportCustomerId"] + "";

        for (int i = 0; i < exNoDetailDt.Rows.Count; i++)
        {
            DataRow detailDr = exNoDetailDt.Rows[i];

            goodsId = detailDr["MaterialId"] + "";
            goodsCode = detailDr["MaterialCode"] + "";
            lotNo = detailDr["LotNo"] + "";
            inspection = detailDr["zjStatus"] + "";
            exQty = Convert.ToDecimal(detailDr["exQty"]);
            allocateQty = Convert.ToDecimal(detailDr["allocateQty"]);

            //需要出库数量
            decimal needQty = exQty - allocateQty;
            if (needQty > 0)
            {
                //物料箱数量
                string goodBoxQtySql = $@"select BoxQuantity from WmsBaseMaterial where isdelete=0 and MaterialCode='{goodsCode}'";
                decimal goodBoxQty = sqlClient.Ado.GetDecimal(goodBoxQtySql);

                //整托出库，整托盘数
                int fullNum = Math.Floor(exQty / goodBoxQty).ToInt();
                OutRightSlotDto fullExport = new();
                if (fullNum > 0)
                {
                    fullExport = GetOutBasePalletSlotCode(goodsId, lotNo, inspection, exNotifyDr["ProxyStatus"] + "", exCustomer, "0", fullNum * goodBoxQty, goodBoxQty);
                }

                //零托数
                decimal halfQty = exQty - (fullNum * goodBoxQty);
                OutRightSlotDto halfExport = new();

                //判断整托出库是否足够，不够出零托
                if (fullExport.SlotAndStockIE.Count() < fullNum)
                {
                    halfQty += exQty - (fullExport.SlotAndStockIE.Count() * goodBoxQty);
                }

                //零托出库
                if (halfQty > 0)
                {
                    //获取出库托盘的储位编码
                    halfExport = GetOutBasePalletSlotCode(goodsId, lotNo, inspection, exNotifyDr["ProxyStatus"] + "", exCustomer, "1", halfQty, goodBoxQty);
                }

                //流水号
                string exOrderNo = new CommonMethod(sqlClient).GetImExNo("CK");
                if (exNoDetailDt.Rows.Count > 2)
                {
                    exOrderNo = $@"CK{Convert.ToInt64(exOrderNo.Substring(2)) + i}";
                }

                //先改库存
                decimal outQty = fullExport.ExportQty + halfExport.ExportQty;
                string uptStockSql = $@"update WmsStockTray set StockQuantity=StockQuantity-{outQty},LockQuantity=LockQuantity+{outQty} where MaterialId='{goodsId}' and LotNo='{lotNo}'";
                int uptStockEnd = sqlClient.Ado.ExecuteCommand(uptStockSql);

                //要出库的所有托盘及位置
                exList = fullExport.SlotAndStockIE.Concat(halfExport.SlotAndStockIE).ToList();
                int exSlotNum = fullExport.SlotAndStockIE.Count() + halfExport.SlotAndStockIE.Count();

                for (int j = 0; j < exList.Count; j++)
                {
                    string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    OutSlotAndStockDto outEx = exList[j];
                    //出库流水
                    string exOrderSql = $@"insert into WmsExportOrder (Id,ExportOrderNo,ExportBillType,ExportMaterialId,ExportMaterialCode,ExportMaterialName,ExportMaterialStandard,ExportMaterialModel,ExportMaterialType,ExportMaterialUnit,ExportWarehouseId,ExportSlotCode,ExportStockCode,ExportQuantity,ExportCustomerCode,ExportExecuteFlag,ExportLotNo,ExportBillCode,StockWholeBoxNum,WholeBoxNum,ExportType,InspectionStatus,IsDelete,CreateTime,CreateUserId,WholeOutWare) values ({SnowFlakeSingle.Instance.NextId},'{exOrderNo}','{exNotifyDr["ExportBillType"] + ""}',{goodsId},'{goodsCode}','{detailDr["MaterialName"] + ""}','{detailDr["MaterialStandard"] + ""}','{detailDr["MaterialModel"] + ""}','{detailDr["MaterialType"] + ""}','{detailDr["MaterialUnit"] + ""}',{exNotifyDr["WarehouseId"] + ""},'{outEx.SlotCode}','{outEx.StockCode}',{outQty},{exCustomer},0,'{lotNo}','{billCode}','{exSlotNum}',{exQty},0,'{inspection}',0,'{now}','{userId}','{exNotifyDr["WholeOutWare"] + ""}')";
                    sqlList.Add(exOrderSql);
                }

            }
            //改出库详情
            sqlList.Add($@"update WmsExportNotifyDetail set allocateQty={exQty},ExportDetailFlag=1 where Id={detailDr["Id"] + ""}");
        }
        sqlList.Add($@"update WmsExportNotify set ExportExecuteFlag=1 where ExportBillCode='{billCode}'");

        //修改储位状态
        string uptSlotSql = $@"update WmsBaseSlot set SlotStatus=3 where SlotCode in('{string.Join("','", exList.Select(e => e.SlotCode))}')";
        sqlList.Add(uptSlotSql);

        int end = sqlClient.Ado.ExecuteSqlBatch(sqlList);
        if (end > 0)
        {
            return result;
        }
        else
        {
            throw Oops.Bah("出库修改储位信息失败");
        }

    }


    /// <summary>
    /// 出库流水生成出库任务
    /// </summary>
    /// <param name="exOrderIds">要出库的流水id，1,2,3</param>
    /// <returns></returns>
    [DisplayName("出库流水生成出库任务")]
    [ApiDescriptionSettings(Name = "ExOrderToExTask"), HttpPost]
    public List<WmsToWcsOutTaskModel> ExOrderToExTask(string exOrderIds)
    {
        if (string.IsNullOrWhiteSpace(exOrderIds))
        {
            throw Oops.Bah("出库参数异常，请修改后重试");
        }

        //出库流水
        string exOrderSql = $@"select a.Id,a.ExportOrderNo,a.ExportBillCode,a.ExportMaterialCode,a.ExportLotNo,a.ExportStockCode,a.ExportSlotCode,a.ExportQuantity,a.StockWholeBoxNum,a.WholeBoxNum,a.ExportWarehouseId,b.WarehouseCode,a.WholeOutWare,a.CreateUserId
	from WmsExportOrder a
	left join WmsBaseWareHouse b on b.Id=a.ExportWarehouseId
	where a.IsDelete=0 and a.ExportExecuteFlag=0 and a.Id in ({exOrderIds})
	order by a.CreateTime,a.ExportOrderNo";
        DataTable exOrderDt = sqlClient.Ado.GetDataTable(exOrderSql);

        List<WmsToWcsOutTaskModel> result = [];
        List<string> sqlList = [];
        string slotsCode = "";

        if (exOrderDt == null || exOrderDt.Rows.Count < 1)
        {
            throw Oops.Bah("未查询到对应流水");
        }
        else
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            for (int i = 0; i < exOrderDt.Rows.Count; i++)
            {
                DataRow outOrderDr = exOrderDt.Rows[i];
                string exSlotCode = outOrderDr["ExportSlotCode"] + "";
                //看是不是深储位，深储位可能要移库
                if (exSlotCode[exSlotCode.Length - 1] == '2')
                {
                    //看浅工位是否有货
                    string slotStatusSql = $@"select SlotStatus
	from WmsBaseSlot
	where SlotInout='1' and SlotImlockFlag=0 and SlotExlockFlag=0 and SlotCloseFlag=0 and SlotCode='{exSlotCode.Substring(0, exSlotCode.Length - 1) + "1"}'";
                    string slotStatusStr = sqlClient.Ado.GetString(slotStatusSql);

                    //1有物品，需要移库
                    if (slotStatusStr == "1")
                    {
                        //移库代码
                        //更改移库托盘库存
                        //有流水更改流水和任务
                    }

                }

                slotsCode += exSlotCode + ((i == exOrderDt.Rows.Count - 1) ? "" : ",");
                //流水号
                string exTaskNo = new CommonMethod(sqlClient).GetImExNo("CKR");
                if (exOrderDt.Rows.Count > 2)
                {
                    exTaskNo = $@"CKR{Convert.ToInt64(exTaskNo.Substring(3)) + i}";
                }
                string exTaskSql = $@"insert into WmsExportTask (Id,ExportTaskNo,Sender,Receiver,IsSuccess,SendDate,StockCode,StockSoltCode,Msg,ExportTaskFlag,ExportOrderNo,StartLocation,EndLocation,WarehouseId,TaskType,CreateTime,CreateUserId,IsDelete) values ({SnowFlakeSingle.Instance.NextId},'{exTaskNo}','wms','wcs',1,'{now}','{outOrderDr["ExportStockCode"] + ""}','{outOrderDr["ExportSlotCode"] + ""}','目标出库位置为{outOrderDr["WholeOutWare"] + ""}',1,{outOrderDr["ExportOrderNo"] + ""},'{outOrderDr["ExportSlotCode"] + ""}','{outOrderDr["WholeOutWare"] + ""}',{outOrderDr["ExportWarehouseId"] + ""},1,'{now}','{outOrderDr["CreateUserId"] + ""}','0')";
                sqlList.Add(exTaskSql);

                //下发出库任务
                WmsToWcsOutTaskModel outModel = new()
                {
                    TaskNo = exTaskNo,
                    TaskBegin = outOrderDr["ExportSlotCode"] + "",
                    TaskEnd = outOrderDr["WholeOutWare"] + "",
                    StockCode = outOrderDr["ExportStockCode"] + "",
                    TaskType = "1",
                    OutType = "1",
                    HouseCode = outOrderDr["WarehouseCode"] + "",
                    Qty = Convert.ToDecimal(outOrderDr["ExportQuantity"]),
                    StockNum = outOrderDr["StockWholeBoxNum"] + "",
                    BillCode = outOrderDr["ExportBillCode"] + "",
                    BoxInfos = [],
                    GoodsCode = outOrderDr["ExportMaterialCode"] + "",
                    GoodsQTY = Convert.ToInt32(outOrderDr["ExportQuantity"])
                };
                result.Add(outModel);
            }
            slotsCode = slotsCode.TrimEnd(',');
        }
        //修改流水状态
        string uptOrderSql = $@"update WmsExportOrder set ExportExecuteFlag=1 where IsDelete=0 and ExportExecuteFlag=0 and Id in ('{exOrderIds.Replace(",", "','")}')";
        sqlList.Add(uptOrderSql);

        //修改储位状态
        string uptSlotSql = $@"update WmsBaseSlot set SlotStatus=3 where SlotCode in('{slotsCode.Replace(",", "','")}')";
        sqlList.Add(uptSlotSql);

        int end = sqlClient.Ado.ExecuteSqlBatch(sqlList);
        if (end > 0)
        {
            return result;
        }
        else
        {
            throw Oops.Bah("出库修改储位信息失败");
        }

    }




    /// <summary>
    /// 获取出库托盘的储位编码
    /// </summary>
    /// <param name="goodsId">物料Id</param>
    /// <param name="lotNo">批次</param>
    /// <param name="inspection">质检状态</param>
    /// <param name="proxyStatus">代储</param>
    /// <param name="exCustomerId">出库客户id</param>
    /// <param name="fullHalf">整托零托标识，0整托；1零托</param>
    /// <param name="exQty">出库数量</param>
    /// <param name="boxNum">箱数量</param>
    /// <returns></returns>
    public OutRightSlotDto GetOutBasePalletSlotCode(string goodsId, string lotNo, string inspection, string proxyStatus, string exCustomerId, string fullHalf, decimal exQty, decimal boxNum)
    {
        IEnumerable<OutSlotAndStockDto> outDtoList = [];

        if (string.IsNullOrWhiteSpace(goodsId) || string.IsNullOrWhiteSpace(lotNo) || string.IsNullOrWhiteSpace(inspection) || string.IsNullOrWhiteSpace(fullHalf))
        {
            throw Oops.Bah("获取出库托盘的储位编码参数异常");
        }
        //条件
        string whereStr = (fullHalf == "0") ? $@" and a.StockQuantity={boxNum}" : $@" and a.StockQuantity<={boxNum}";
        whereStr += (proxyStatus == "1") ? $@" and a.OwnerCode='{exCustomerId}'" : $@" and isnull(a.OwnerCode,'')=''";
        //找库存
        string tpNumSql = $@"select a.Id,a.StockSlotCode,a.StockCode,a.StockQuantity
	from WmsStockTray a
	left join WmsBaseSlot b on b.SlotCode=a.StockSlotCode
	where a.StockStatusFlag=0 and a.MaterialId='{goodsId}' and a.LotNo='{lotNo}' and a.InspectionStatus='{inspection}' {whereStr}
	order by a.StockQuantity,b.SlotInout,b.SlotRow,b.SlotColumn,b.SlotLayer";
        DataTable tpNumDt = sqlClient.Ado.GetDataTable(tpNumSql);

        if (tpNumDt == null || tpNumDt.Rows.Count < 1)
        {
            throw Oops.Bah($@"查询{((fullHalf == "0") ? "整" : "零")}托库存失败");
        }
        else
        {
            decimal tempNum = 0M;
            foreach (DataRow tpNumDr in tpNumDt.Rows)
            {
                if (tempNum < exQty)
                {
                    OutSlotAndStockDto outDto = new()
                    {
                        SlotCode = tpNumDr["StockSlotCode"] + "",
                        StockCode = tpNumDr["StockCode"] + ""
                    };

                    tempNum += Convert.ToDecimal(tpNumDr["StockQuantity"]);
                    outDtoList.Append(outDto);
                }
                else
                {
                    break;
                }

            }

            //分配储位结果
            OutRightSlotDto result = new()
            {
                SlotAndStockIE = outDtoList,
                ExportQty = (tempNum < exQty) ? tempNum : exQty
            };
            return result;
        }

    }

}
