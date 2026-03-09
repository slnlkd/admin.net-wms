using Admin.NET.Application.Service.BaseService;
using Admin.NET.Application.Service.WmsDto;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Admin.NET.Application.Service.InBase;


/// <summary>
/// 入库
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class InBaseService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarClient sqlClient;
    private readonly ILogger logger;


    public InBaseService(ISqlSugarClient SqlSugarClient, ILoggerFactory loggerFactory)
    {
        sqlClient = SqlSugarClient;
        logger = loggerFactory.CreateLogger("入库日志");
    }


    /// <summary>
    /// 立体库入库申请
    /// </summary>
    /// <param name="wareHouseId">仓库id</param>
    /// <param name="stockCode">载具条码</param>
    /// <param name="slotHigh">储位高度，0不区分高度；3低货位；4高货位</param>
    /// <returns></returns>
    [DisplayName("立体库入库申请")]
    [ApiDescriptionSettings(Name = "InWareHouseAddStock"), HttpPost]
    public List<WmsToWcsInTaskModel> InWareHouseAddStock(int wareHouseId, string stockCode, int slotHigh)
    {
        //日志
        string logAddress = @"D:\log\wcs入库申请" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

        try
        {
            if (string.IsNullOrWhiteSpace(wareHouseId + "") || string.IsNullOrWhiteSpace(stockCode) || string.IsNullOrWhiteSpace(slotHigh + ""))
            {
                throw Oops.Bah("入库申请参数有空值，请修改后重试");
            }

            logger.LogCritical($@"wcs入库申请内容：wareHouseId（仓库id）:{wareHouseId}；stockCode（载具条码）:{stockCode}；slotHigh（储位高度）:{slotHigh}", logAddress);

            List<WmsToWcsInTaskModel> result = [];

            //托盘码表是否有该条码
            string tmhgSql = $@"select count(id) as num 
	from WmsSysStockCode 
	where IsDelete=0 and Status=0 and WarehouseId={wareHouseId} and StockCode='{stockCode}'";
            int tmhgEnd = sqlClient.Ado.GetInt(tmhgSql);
            if (tmhgEnd < 1)
            {
                throw Oops.Bah($@"载具条码{stockCode}不受wms管理不可入库！");
            }

            //该条码是否待执行或正在执行
            string tmExportSql = $@"select count(id) as num 
	from WmsExportOrder 
	where IsDelete=0 and ExportExecuteFlag in(0,1) and ExportWarehouseId={wareHouseId} and ExportStockCode='{stockCode}'";
            int tmExportEnd = sqlClient.Ado.GetInt(tmExportSql);
            if (tmExportEnd > 0)
            {
                throw Oops.Bah($@"载具条码{stockCode}待执行或正在执行出库不可入库！");
            }

            //条码是否绑定入库单，以及入库单状态
            string inOrderSql = $@"select id,ImportExecuteFlag
	from WmsImportOrder
	where IsDelete=0 and WareHouseId={wareHouseId} and StockCode='{stockCode}'";
            DataTable inOrderDt = sqlClient.Ado.GetDataTable(inOrderSql);

            if (inOrderDt != null && inOrderDt.Rows.Count > 0)
            {
                DataRow inOrderDr = inOrderDt.Rows[0];
                if ((inOrderDr["ImportExecuteFlag"] + "") == "-1")
                {
                    throw Oops.Bah($@"载具条码{stockCode}对应入库单已作废不可入库！");
                }
            }
            else
            {
                throw Oops.Bah($@"载具条码{stockCode}未绑定入库单不可入库！");
            }


            //创建wcs任务下发
            WmsToWcsInTaskModel taskModel = new();
            //任务号
            string taskNo = new CommonMethod(sqlClient).GetImExNo("RKR");
            List<string> sqlList = [];

            //立体库库存是否有该条码，判断是否回流入库
            string tmHaveSql = $@"select Id,StockCode,StockQuantity,LockQuantity
	from WmsStockTray 
	where IsDelete=0 and WarehouseId={wareHouseId} and StockCode='{stockCode}'";
            DataTable tmHaveDt = sqlClient.Ado.GetDataTable(tmHaveSql);
            if (tmHaveDt != null && tmHaveDt.Rows.Count > 0)
            {
                DataRow tmHaveDr = tmHaveDt.Rows[0];
                //回流入库
                if (!string.IsNullOrWhiteSpace(tmHaveDr["StockSlotCode"] + ""))
                {
                    string hlrkSql = $@"select MaterialId
	from WmsStockSlotBoxInfo 
	where IsDelte=0 and Status=2 and StockCode='{stockCode}'";
                    DataTable hlrkDt = sqlClient.Ado.GetDataTable(hlrkSql);
                    if (hlrkDt != null && hlrkDt.Rows.Count > 0)
                    {
                        //任务号
                        taskModel.TaskNo = taskNo;
                        //载具号
                        taskModel.StockCode = stockCode;
                        //开始储位
                        taskModel.TaskBegin = "";
                        //结束储位
                        taskModel.TaskEnd = tmHaveDr["StockSlotCode"] + "";
                        //物料编码
                        taskModel.GoodsCode = hlrkDt.Rows[0]["MaterialId"] + "";
                        //入库数量
                        taskModel.GoodsQTY = Convert.ToDecimal(tmHaveDr["StockQuantity"]) + Convert.ToDecimal(tmHaveDr["LockQuantity"]);

                        result.Add(taskModel);
                    }
                    //修改储位状态
                    string uptSlotSql = $@"update WmsBaseSlot set SlotStatus=2 where SlotCode='{tmHaveDr["StockSlotCode"] + ""}'";
                    sqlList.Add(uptSlotSql);

                    int uptSlotEnd = sqlClient.Ado.ExecuteSqlBatch(sqlList);

                    if (uptSlotEnd >= 0)
                    {
                        return result;
                    }
                    else
                    {
                        throw Oops.Bah($@"回流入库修改储位异常");
                    }

                }
            }


            //立体库入库
            string callBackBaseSql = $@"select sum(Qty) as sumQty,MaterialId
	from WmsStockSlotBoxInfo
	where IsDelte=0 and Status=0 and StockCode='{stockCode}'
	group by StockCode,MaterialId";
            DataTable callBackBaseDt = sqlClient.Ado.GetDataTable(callBackBaseSql);

            if (callBackBaseDt != null && callBackBaseDt.Rows.Count > 0)
            {
                DataRow callBackBaseDr = callBackBaseDt.Rows[0];
                //分配储位
                InRightSlotDto slotDto = GetRightSlot(wareHouseId, slotHigh);

                //任务号
                taskModel.TaskNo = taskNo;
                //载具号
                taskModel.StockCode = stockCode;
                //开始储位
                taskModel.TaskBegin = "";
                //结束储位
                taskModel.TaskEnd = slotDto.SlotStr;
                //物料编码
                taskModel.GoodsCode = callBackBaseDt.Rows[0]["MaterialId"] + "";
                //入库数量
                taskModel.GoodsQTY = Convert.ToDecimal(callBackBaseDr["sumQty"]);


                //修改储位状态
                string uptSlotSql = $@"update WmsBaseSlot set SlotStatus=2 where SlotCode='{slotDto.SlotStr}'";
                sqlList.Add(uptSlotSql);
                //插入入库任务表
                string addinTaskSql = $@"insert into WmsImportTask(Id,TaskNo,Sender,Receiver,IsSuccess,SendDate,Message,StockCode,Status,WareHouseId,EndLocation,Msg,FinishDate) value({SnowFlakeSingle.instance.NextId()},'{taskNo}','WMS','WCS',1,'{DateTime.Now}','申请库位指令','{slotDto.SlotStr}',1,{wareHouseId},'{stockCode}获取储位{slotDto.SlotStr}','{DateTime.Now}')";
                sqlList.Add(addinTaskSql);

                int uptSlotEnd = sqlClient.Ado.ExecuteSqlBatch(sqlList);

                if (uptSlotEnd >= 0)
                {
                    return result;
                }
                else
                {
                    throw Oops.Bah($@"立体库入库修改储位异常");
                }
            }
            else
            {
                throw Oops.Bah($@"入库查询箱码关系异常");
            }

        }
        catch (Exception ex)
        {
            //Log.SaveLogToFile($@"wcs入库申请异常：{ex.Message}！", logAddress);
            throw Oops.Bah($@"入库申请异常，异常信息：{ex.Message}");
        }


    }


    /// <summary>
    /// 分配储位，修改分配的储位状态
    /// </summary>
    /// <param name="wareHouseId">仓库id</param>
    /// <param name="slotHigh">高低货位</param>
    /// <returns></returns>
    public InRightSlotDto GetRightSlot(int wareHouseId, int slotHigh)
    {
        //上次分配的储位
        string lastSlotSql = $@"select top 1 WarehouseId,LanewayId,SlotCode,SlotRow,SlotColumn,SlotLayer,SlotInout
	from WmsSlotGetLog
	where IsDelete=0 and SlotHigh={slotHigh} and WarehouseId={wareHouseId}
	order by CreateTime desc";
        DataTable lastSlotDt = sqlClient.Ado.GetDataTable(lastSlotSql);
        //上次分配巷道
        string lastLaneway = (lastSlotDt != null && lastSlotDt.Rows.Count >= 1) ? lastSlotDt.Rows[0]["LanewayId"] + "" : "";

        //巷道优先级分组
        string hdGroupbySql = $@"select LanewayPriority,count(id) as num
	from WmsBaseLaneway
	where IsDelete=0 and LanewayStatus=1 and WarehouseId={wareHouseId}
	group by LanewayPriority
	order by LanewayPriority desc,count(id)";
        DataTable hdGroupbyDt = sqlClient.Ado.GetDataTable(hdGroupbySql);

        //取合适储位
        InRightSlotDto rightSlotDto = new InRightSlotDto();
        if (hdGroupbyDt != null)
        {
            string lanewaySql = "";
            //查询合适巷道
            if (hdGroupbyDt.Rows.Count > 1)
            {
                lanewaySql = $@"select Id,LanewayCode,LanewayName
	from WmsBaseLaneway
	where IsDelete=0 and LanewayStatus=1 and WarehouseId={wareHouseId} and LanewayPriority in(select max(LanewayPriority) from WmsBaseLaneway where IsDelete=0 and LanewayStatus=1 and WarehouseId={wareHouseId})
	order by LanewayPriority desc,LanewayCode";
            }
            else
            {
                lanewaySql = $@"select Id,LanewayCode,LanewayName
	from WmsBaseLaneway
	where IsDelete=0 and LanewayStatus=1 and WarehouseId={wareHouseId}
	order by LanewayPriority desc,LanewayCode";
            }
            DataTable lanewayDt = sqlClient.Ado.GetDataTable(lanewaySql);

            //取合适储位
            rightSlotDto = SelectRightSlot(wareHouseId, slotHigh, lanewayDt, lastLaneway);

            int result = sqlClient.Ado.ExecuteSqlBatch(rightSlotDto.SqlList);

            if (result > 0)
            {
                return rightSlotDto;
            }
            else
            {
                rightSlotDto.SlotStr = "";
                return rightSlotDto;
            }

        }
        else
        {
            rightSlotDto.SlotStr = "";
            return rightSlotDto;
        }
    }


    /// <summary>
    /// 找合适储位返回储位，并返回储位修改sql
    /// </summary>
    /// <param name="wareHouseId">仓库id</param>
    /// <param name="slotHigh">高低货位</param>
    /// <param name="lanewayDt">巷道dt</param>
    /// <param name="lastLaneway">上次分配巷道</param>
    /// <returns></returns>
    public InRightSlotDto SelectRightSlot(int wareHouseId, int slotHigh, DataTable lanewayDt, string lastLaneway)
    {
        string slotStr = "";
        List<string> sqlList = [];

        if (lanewayDt == null || lanewayDt.Rows.Count < 1)
        {
            return new InRightSlotDto()
            {
                SlotStr = slotStr,
                SqlList = sqlList
            };
        }
        else
        {
            if (lanewayDt.Rows.Count > 1)
            {
                DataRow[] lanewayDr = lanewayDt.Select($@"Id=={lastLaneway}");

                if (lanewayDr != null && lanewayDr.Length > 0)
                {
                    foreach (DataRow row in lanewayDr)
                    {
                        lanewayDt.Rows.Remove(row);
                    }
                }
            }

            foreach (DataRow xdRow in lanewayDt.Rows)
            {
                //空储位
                string slotSql = $@"select WarehouseId,SlotCode,SlotLanewayId,SlotRow,SlotColumn,SlotLayer,SlotInout,SlotHigh
	from WmsBaseSlot
	where SlotImlockFlag=0 and SlotExlockFlag=0 and SlotCloseFlag=0 and SlotStatus=0 and WarehouseId={wareHouseId} and SlotLanewayId={xdRow["Id"] + ""} and SlotHigh={slotHigh}
	order by SlotRow,SlotColumn,SlotLayer,SlotInout desc";
                DataTable slotDt = sqlClient.Ado.GetDataTable(slotSql);
                //如果没找到等于高度的就找大于等于高度的
                if (slotDt == null || slotDt.Rows.Count <= 0)
                {
                    slotSql = $@"select WarehouseId,SlotCode,SlotLanewayId,SlotRow,SlotColumn,SlotLayer,SlotInout,SlotHigh
	from WmsBaseSlot
	where SlotImlockFlag=0 and SlotExlockFlag=0 and SlotCloseFlag=0 and SlotStatus=0 and WarehouseId={wareHouseId} and SlotLanewayId={xdRow["Id"] + ""} and SlotHigh>={slotHigh}
	order by SlotRow,SlotColumn,SlotLayer,SlotInout desc";
                    slotDt = sqlClient.Ado.GetDataTable(slotSql);
                }

                //如果储位表还空说明没有合适储位换下一条数据
                if (slotDt == null || slotDt.Rows.Count <= 0)
                {
                    continue;
                }
                else
                {
                    foreach (DataRow slotRow in slotDt.Rows)
                    {
                        if (slotRow["SlotInout"] + "" == "1")
                        {
                            //深工位不能是2正在入库；3正在出库；4正在移入；5正在移出的
                            string shenSlotSQL = $@"select id,SlotCode
	from WmsBaseSlot
	where SlotCode='{string.Concat((slotRow["SlotCode"] + "").AsSpan(0, 9), "2")}' and SlotStatus not in (2,3,4,5)";
                            DataTable shenSlotDT = sqlClient.Ado.GetDataTable(shenSlotSQL);

                            if (shenSlotDT == null || shenSlotDT.Rows.Count <= 0)
                            {
                                continue;
                            }
                            else
                            {
                                slotStr = slotRow["SlotCode"] + "";
                            }
                        }
                        else
                        {
                            slotStr = slotRow["SlotCode"] + "";
                        }
                    }

                    //插入WmsSlotGetLog表
                    sqlList.Add($@"insert into WmsSlotGetLog(Id,WarehouseId,LanewayId,SlotCode,SlotRow,SlotColumn,SlotLayer,SlotInout,SlotHigh,IsDelete,CreateTime)
select {SnowFlakeSingle.Instance.NextId} as Id,WarehouseId,SlotLanewayId,SlotCode,SlotRow,SlotColumn,SlotLayer,SlotInout,SlotHigh,0 as IsDelete,getdate() as CreateTime
	from WmsBaseSlot
	where SlotCode='{slotStr}'");
                    //修改WmsBaseSlot表的SlotStatus值=2
                    sqlList.Add($@"update WmsBaseSlot set SlotStatus=2 where SlotCode='{slotStr}'");
                }

            }

        }

        return new InRightSlotDto()
        {
            SlotStr = slotStr,
            SqlList = sqlList
        };
    }

}
