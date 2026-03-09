// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Application.Entity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.NET.Application;
public class CommonDal : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsStockTray> _wmsStockTrayRep;
    private readonly SqlSugarRepository<WmsImportOrder> _wmsImportOrderRep;
    private readonly SqlSugarRepository<WmsExportOrder> _wmsExportOrderRep;
    private readonly SqlSugarRepository<WmsInspectOrder> _wmsInspectOrderRep;

    private readonly SqlSugarRepository<WmsExportNotify> _wmsExportNotifyRep;
    private readonly SqlSugarRepository<WmsExportNotifyDetail> _wmsExportNotifyDetailRep;
    private readonly SqlSugarRepository<WmsBaseBillType> _wmsBaseBillTypeRep;
    private readonly SqlSugarRepository<WmsBaseWareHouse> _wmsBaseWareHouseRep;
    private readonly SqlSugarRepository<WmsExportWayOut> _wmsExportWayOutRep;
    private readonly SqlSugarRepository<WmsBaseCustomer> _wmsBaseCustomerRep;
    private readonly SqlSugarRepository<WmsImportNotifyDetail> _wmsImportNotifyDetailRep;
    private readonly SqlSugarRepository<WmsBaseMaterial> _wmsBaseMaterialRep;
    private readonly SqlSugarRepository<WmsStock> _wmsStockRep;
    private readonly SqlSugarRepository<SysDictType> _sysDictTypeRep;
    private readonly SqlSugarRepository<SysDictData> _sysDictDataRep;
    private readonly SqlSugarRepository<WmsBaseUnit> _wmsBaseUnitRep;
    private readonly SqlSugarRepository<WmsBaseDepartment> _wmsBaseDepartmentRep;
    private readonly SqlSugarRepository<WmsBaseSupplier> _wmsBaseSupplierRep;
    private readonly SqlSugarRepository<WmsBaseArea> _wmsBaseAreaRep;
    private readonly SqlSugarRepository<WmsExportBoxInfo> _wmsExportBoxInfoRep;
    private readonly SqlSugarRepository<WmsImportNotify> _wmsImportNotifyRep;//WmsMoveTask
    private readonly SqlSugarRepository<WmsExportTask> _wmsExportTaskRep;
    private readonly SqlSugarRepository<WmsInspectNotity> _wmsInspectNotityRep;
    private readonly SqlSugarRepository<WmsStockCheckNotify> _wmsStockCheckNotifyRep;
    private readonly SqlSugarRepository<WmsImportTask> _wmsImportTaskRep;
    private readonly SqlSugarRepository<WmsInterfaceLog> _wmsInterfaceLogRep;
    private readonly SqlSugarRepository<WmsInspectTask> _wmsInspectTaskRep;
    private readonly SqlSugarRepository<WmsStockCheckTask> _wmsStockCheckTaskRep;
    private readonly SqlSugarRepository<WmsMoveTask> _wmsMoveTaskRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public CommonDal(SqlSugarRepository<WmsStockTray> wmsStockTrayRep,
        SqlSugarRepository<WmsImportOrder> wmsImportOrderRep,
        SqlSugarRepository<WmsExportOrder> wmsExportOrderRep,
        SqlSugarRepository<WmsInspectOrder> wmsInspectOrderRep,
        SqlSugarRepository<WmsExportNotify> wmsExportNotifyRep,
        SqlSugarRepository<WmsExportNotifyDetail> wmsExportNotifyDetailRep,
        SqlSugarRepository<WmsBaseBillType> wmsBaseBillTypeRep,
        SqlSugarRepository<WmsBaseWareHouse> wmsBaseWareHouseRep,
        SqlSugarRepository<WmsExportWayOut> wmsExportWayOutRep,
        SqlSugarRepository<WmsBaseCustomer> wmsBaseCustomerRep,
        SqlSugarRepository<WmsImportNotifyDetail> wmsImportNotifyDetailRep,
        SqlSugarRepository<WmsBaseMaterial> wmsBaseMaterialRep,
        SqlSugarRepository<WmsStock> wmsStockRep,
        SqlSugarRepository<SysDictType> sysDictTypeRep,
        SqlSugarRepository<SysDictData> sysDictDataRep,
        SqlSugarRepository<WmsBaseUnit> wmsBaseUnitRep,
        SqlSugarRepository<WmsBaseDepartment> wmsBaseDepartmentRep,
        SqlSugarRepository<WmsBaseSupplier> wmsBaseSupplierRep,
        SqlSugarRepository<WmsBaseArea> wmsBaseAreaRep,
        SqlSugarRepository<WmsExportBoxInfo> wmsExportBoxInfoRep,
        SqlSugarRepository<WmsExportTask> wmsExportTaskRep,
        SqlSugarRepository<WmsImportNotify> wmsImportNotifyRep,
        SqlSugarRepository<WmsInspectNotity> wmsInspectNotityRep,
        SqlSugarRepository<WmsStockCheckNotify> wmsStockCheckNotifyRep,
        SqlSugarRepository<WmsImportTask> wmsImportTaskRep,
        SqlSugarRepository<WmsInterfaceLog> wmsInterfaceLogRep,
        SqlSugarRepository<WmsInspectTask> wmsInspectTaskRep,
        SqlSugarRepository<WmsStockCheckTask> wmsStockCheckTaskRep,
        SqlSugarRepository<WmsMoveTask> wmsMoveTaskRep,
        ISqlSugarClient sqlSugarClient
        )
    {
        _wmsImportOrderRep = wmsImportOrderRep;
        _wmsInspectOrderRep = wmsInspectOrderRep;
        _wmsExportNotifyRep = wmsExportNotifyRep;
        _wmsExportNotifyDetailRep = wmsExportNotifyDetailRep;
        _wmsBaseBillTypeRep = wmsBaseBillTypeRep;
        _wmsBaseWareHouseRep = wmsBaseWareHouseRep;
        _wmsExportWayOutRep = wmsExportWayOutRep;
        _wmsBaseCustomerRep = wmsBaseCustomerRep;
        _wmsImportNotifyDetailRep = wmsImportNotifyDetailRep;
        _wmsBaseMaterialRep = wmsBaseMaterialRep;
        _wmsStockRep = wmsStockRep;
        _sysDictTypeRep = sysDictTypeRep;
        _sysDictDataRep = sysDictDataRep;
        _wmsBaseUnitRep = wmsBaseUnitRep;
        _wmsBaseDepartmentRep = wmsBaseDepartmentRep;
        _wmsBaseSupplierRep = wmsBaseSupplierRep;
        _wmsBaseAreaRep = wmsBaseAreaRep;
        _wmsExportBoxInfoRep = wmsExportBoxInfoRep;
        _wmsExportOrderRep = wmsExportOrderRep;
        _wmsExportTaskRep = wmsExportTaskRep;
        _wmsImportNotifyRep = wmsImportNotifyRep;
        _wmsStockTrayRep = wmsStockTrayRep;
        _wmsInspectNotityRep = wmsInspectNotityRep;
        _wmsStockCheckNotifyRep = wmsStockCheckNotifyRep;
        _wmsImportTaskRep = wmsImportTaskRep;
        _wmsInterfaceLogRep = wmsInterfaceLogRep;
        _wmsInspectTaskRep = wmsInspectTaskRep;
        _wmsStockCheckTaskRep = wmsStockCheckTaskRep;
        _wmsMoveTaskRep = wmsMoveTaskRep;
        _sqlSugarClient = sqlSugarClient;
    }



    #region 公共方法


    /// <summary>
    /// 获取入出库自增的单据号/流水号
    /// </summary>
    /// <param name="inOutFlag">入库单:RKS;出库单:CKS;入库流水:RK;出库流水:CK;</param>
    /// <returns></returns>
    public async Task<string> GetImExNo(string inOutFlag)
    {
        string maxNo = "";
        //获取流水号
        if (inOutFlag == EnmuValue.InOutFlag.CK.ToString() || inOutFlag == EnmuValue.InOutFlag.RK.ToString() || inOutFlag == EnmuValue.InOutFlag.TJ.ToString() || inOutFlag == EnmuValue.InOutFlag.YS.ToString() || inOutFlag == EnmuValue.InOutFlag.PK.ToString())
        {

            maxNo = await SelectOrderMaxNo(inOutFlag);
            if (!string.IsNullOrEmpty(maxNo))
            {
                maxNo = maxNo.Substring(2);
            }
        }
        else if (inOutFlag == EnmuValue.InOutFlag.RKS.ToString() || inOutFlag == EnmuValue.InOutFlag.CKS.ToString() || inOutFlag == EnmuValue.InOutFlag.YKS.ToString() || inOutFlag == EnmuValue.InOutFlag.TJS.ToString() || inOutFlag == EnmuValue.InOutFlag.YSS.ToString() || inOutFlag == EnmuValue.InOutFlag.PKS.ToString())//获取单据号
        {
            maxNo = await SelectNotityMaxNo(inOutFlag);
            if (!string.IsNullOrEmpty(maxNo))
            {
                maxNo = maxNo.Substring(3);
            }
        }
        else if (inOutFlag == EnmuValue.InOutFlag.RKR.ToString() || inOutFlag == EnmuValue.InOutFlag.CKR.ToString() || inOutFlag == EnmuValue.InOutFlag.YKR.ToString() || inOutFlag == EnmuValue.InOutFlag.PKR.ToString() || inOutFlag == EnmuValue.InOutFlag.IF.ToString() || inOutFlag == EnmuValue.InOutFlag.YSR.ToString() || inOutFlag == EnmuValue.InOutFlag.TJR.ToString() || inOutFlag == EnmuValue.InOutFlag.PKR.ToString())
        {
            maxNo = await SelectTaskMaxNo(inOutFlag);
            if (!string.IsNullOrEmpty(maxNo))
            {
                maxNo = maxNo.Substring(3);
            }
        }
        else
        {
            return "";
        }
        //获取数据库时间八位
        string date = GetDBTime().Trim();
        string no = "";
        if (string.IsNullOrEmpty(maxNo))
        {
            no = inOutFlag + date + "000001";
        }
        else
        {
            if (maxNo.Substring(0, 8) == date)
            {
                int lastNo = Convert.ToInt32(maxNo.Substring(8, 6)) + 1;
                no = inOutFlag + date + (lastNo.ToString().PadLeft(6, '0'));
            }
            else
            {
                no = inOutFlag + date + "000001";
            }
        }
        return no;
    }
    /// <summary>
    /// 获取最大流水号
    /// </summary>
    /// <returns></returns>
    private async Task<string> SelectOrderMaxNo(string inoutFlag)
    {
        try
        {
            string orderNo = "";
            if (inoutFlag == EnmuValue.InOutFlag.CK.ToString())
            {
                var list = await _wmsExportOrderRep.AsQueryable().Where(a => a.ExportOrderNo.ToString().StartsWith("CK")).ToListAsync();
                orderNo = list.Select(a => a.ExportOrderNo).Max().ToString();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.RK.ToString())
            {
                var list = await _wmsImportOrderRep.AsQueryable().Where(a => a.ImportOrderNo.StartsWith("RK")).ToListAsync();
                orderNo = list.Select(a => a.ImportOrderNo).Max();
            }

            else if (inoutFlag == EnmuValue.InOutFlag.YS.ToString())
            {
                var list = await _wmsInspectOrderRep.AsQueryable().Where(a => a.InspectOrderNo.StartsWith("YS")).ToListAsync();
                orderNo = list.Select(a => a.InspectOrderNo).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.PK.ToString())
            {
                var list = await _wmsInspectOrderRep.AsQueryable().Where(a => a.InspectOrderNo.StartsWith("PK")).ToListAsync();
                orderNo = list.Select(a => a.InspectOrderNo).Max();
            }
            return orderNo;

        }
        catch (Exception ex)
        {

            throw new Exception("获取最大流水号" + ex.Message);
        }
    }
    /// <summary>
    /// 获取最大单据号
    /// </summary>
    /// <returns></returns>
    private async Task<string> SelectNotityMaxNo(string inoutFlag)
    {
        try
        {

            string orderNo = "";
            if (inoutFlag == EnmuValue.InOutFlag.CKS.ToString())
            {
                var list = await _wmsExportNotifyRep.AsQueryable().Where(a => a.ExportBillCode.StartsWith("CKS")).ToListAsync();
                orderNo = list.Select(a => a.ExportBillCode).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.RKS.ToString())
            {
                var list = await _wmsImportNotifyRep.AsQueryable().Where(a => a.ImportBillCode.StartsWith("RKS")).ToListAsync();
                orderNo = list.Select(a => a.ImportBillCode).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.YKS.ToString())
            {
                //var list = DataContext.WmsMoveNotify.Where(a => a.MoveBillCode.StartsWith("YKS"));
                //orderNo = list.Select(a => a.MoveBillCode).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.TJS.ToString())
            {
                //var list = await WmsExtractNotify.Where(a => a.ExtractBillCode.StartsWith("TJS"));
                //orderNo = list.Select(a => a.ExtractBillCode).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.YSS.ToString())
            {
                var list = await _wmsInspectNotityRep.AsQueryable().Where(a => a.InspectBillCode.StartsWith("YSS")).ToListAsync();
                orderNo = list.Select(a => a.InspectBillCode).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.PKS.ToString())
            {
                var list = await _wmsStockCheckNotifyRep.AsQueryable().Where(a => a.CheckBillCode.StartsWith("PKS")).ToListAsync();
                orderNo = list.Select(a => a.CheckBillCode).Max();
            }
            return orderNo;

        }
        catch (Exception ex)
        {

            return "";
        }
    }
    /// <summary>
    /// 获取最大单据号(入库任务、出库任务、移库任务、盘库任务)
    /// </summary>
    /// <returns></returns>
    private async Task<string> SelectTaskMaxNo(string inoutFlag)
    {
        try
        {

            string taskNo = "";
            if (inoutFlag == EnmuValue.InOutFlag.RKR.ToString())
            {
                var list = await _wmsImportTaskRep.AsQueryable().Where(a => a.TaskNo.StartsWith("RKR")).ToListAsync();
                taskNo = list.Select(a => a.TaskNo).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.CKR.ToString())
            {
                var list = await _wmsExportTaskRep.AsQueryable().Where(a => a.ExportTaskNo.StartsWith("CKR")).ToListAsync();
                taskNo = list.Select(a => a.ExportTaskNo).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.IF.ToString())
            {
                var list = await _wmsInterfaceLogRep.AsQueryable().Where(a => a.TaskId.StartsWith("IF")).ToListAsync();
                taskNo = list.Select(a => a.TaskId).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.TJR.ToString())
            {
                //var list = DataContext.WmsExtractTask.Where(a => a.ExtractTaskNo.StartsWith("TJR"));
                //taskNo = list.Select(a => a.ExtractTaskNo).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.YSR.ToString())
            {
                var list = await _wmsInspectTaskRep.AsQueryable().Where(a => a.InspectTaskNo.StartsWith("YSR")).ToListAsync();
                taskNo = list.Select(a => a.InspectTaskNo).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.PKR.ToString())
            {
                var list = await _wmsStockCheckTaskRep.AsQueryable().Where(a => a.CheckTaskNo.StartsWith("PKR")).ToListAsync();
                taskNo = list.Select(a => a.CheckTaskNo).Max();
            }
            else if (inoutFlag == EnmuValue.InOutFlag.YKR.ToString())
            {
                var list = await _wmsMoveTaskRep.AsQueryable().Where(a => a.TaskNo.StartsWith("YKR")).ToListAsync();
                taskNo = list.Select(a => a.TaskNo).Max();
            }
            return taskNo;

        }
        catch (Exception ex)
        {

            return "";
        }
    }
    /// <summary>
    /// 获取服务器时间八位yyyyMMdd
    /// </summary>
    /// <returns></returns>
    public string GetDBTime()
    {
        try
        {
            var dt = _sqlSugarClient.Ado.GetDataTable("select * from View_GetServerTime");
            //var serverTime = DataContext.View_GetServerTime.Select(a => a.ServerTime).First();

            var serverTime = dt.Rows[0][0] as String;
            return serverTime;

        }
        catch (Exception ex)
        {

            throw new Exception("获取服务器时间八位yyyyMMdd" + ex.Message);
        }
    }
    /// <summary>
    /// 添加按钮点击记录
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="ButtonInformation">记录信息</param>
    public void InsertButtonRecord(string userId, string pageName, string ButtonInformation)
    {
        //try
        //{
            
        //        //判断用户是否为空
        //        if (!string.IsNullOrWhiteSpace(userId))
        //        {
        //            //查询对应的用户
        //            var list = DataContext.WmsSysUser.FirstOrDefault(a => a.Id == userId);
        //            if (list != null && !string.IsNullOrWhiteSpace(list.Id))
        //            {
        //                WmsButtonRecord wmsButton = new WmsButtonRecord();
        //                wmsButton.Id = Guid.NewGuid().ToString("N");
        //                wmsButton.UserId = list.Id;
        //                wmsButton.ClickButtonDate = DateTime.Now;
        //                wmsButton.PageName = pageName;

        //                //用户名+按钮点击时间+记录按钮信息
        //                wmsButton.ButtonInformation = list.UserName + " 在 " + DateTime.Now + " " + ButtonInformation;
        //                DataContext.WmsButtonRecord.InsertOnSubmit(wmsButton);
        //                DataContext.SubmitChanges();
        //            }
        //        }
            
        //}
        //catch (Exception ex)
        //{
        //    throw new Exception("添加按钮点击记录功能失败：" + ex.Message);
        //}
    }

    #region 批量生成空托入库流水
    //public string GetOrderNo(string inOutFlag)
    //{
    //    string maxNo = "";
    //    //获取流水号
    //    if (inOutFlag == InOutFlag.CK.ToString() || inOutFlag == InOutFlag.RK.ToString() || inOutFlag == InOutFlag.TJ.ToString() || inOutFlag == InOutFlag.YS.ToString() || inOutFlag == InOutFlag.PK.ToString())
    //    {
    //        maxNo = GetOrderMaxNo(inOutFlag);
    //        if (!string.IsNullOrEmpty(maxNo))
    //        {
    //            maxNo = maxNo.Substring(2);
    //        }
    //    }
    //    else
    //    {
    //        return "";
    //    }
    //    //获取数据库时间八位
    //    string date = GetDBTime().Trim();
    //    string no = "";
    //    if (string.IsNullOrEmpty(maxNo))
    //    {
    //        no = inOutFlag + date + "000001";
    //    }
    //    else
    //    {
    //        if (maxNo.Substring(0, 8) == date)
    //        {
    //            int lastNo = Convert.ToInt32(maxNo.Substring(8, 6)) + 1;
    //            no = inOutFlag + date + (lastNo.ToString().PadLeft(6, '0'));
    //        }
    //        else
    //        {
    //            no = inOutFlag + date + "000001";
    //        }
    //    }
    //    return no;
    //}
    //private static string GetOrderMaxNo(string inoutFlag)
    //{
    //    try
    //    {
    //        using (var conn = new SqlConnection(DataConnection.GetDataConnection))
    //        {
    //            conn.Open();

    //            string sql = "";
    //            if (inoutFlag == InOutFlag.RK.ToString())
    //            {
    //                sql = @"SELECT MAX(ImportOrderNo) 
    //                FROM WmsImportOrder WITH (NOLOCK) 
    //                WHERE ImportOrderNo LIKE @Prefix + '%'";
    //            }

    //            using (var cmd = new SqlCommand(sql, conn))
    //            {
    //                cmd.Parameters.AddWithValue("@Prefix", inoutFlag);
    //                cmd.CommandTimeout = 30; // 可调大

    //                var result = cmd.ExecuteScalar();
    //                return result == DBNull.Value ? "" : (string)result;
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {

    //        throw new Exception("获取最大流水号" + ex.Message);
    //    }
    //}

    #endregion

    #endregion
}
