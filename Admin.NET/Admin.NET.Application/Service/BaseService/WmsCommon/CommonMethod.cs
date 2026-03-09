using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.NET.Application.Service.BaseService;


/// <summary>
/// 公共方法
/// </summary>
public class CommonMethod
{
    private ISqlSugarClient sqlClient;

    public CommonMethod(ISqlSugarClient SqlSugarClient)
    {
        sqlClient = SqlSugarClient;
    }



    /// <summary>
    /// 获取入出库自增的单据号、流水号、任务号
    /// </summary>
    /// <param name="inOutFlag">入库单RKS，出库单CKS，入库流水RK，出库流水CK</param>
    /// <returns></returns>
    public string GetImExNo(string inOutFlag)
    {
        string maxNo = "";

        if (string.IsNullOrWhiteSpace(inOutFlag))
        {
            return maxNo;
        }

        string flagStr = inOutFlag.ToUpper();
        //流水号集合
        List<string> orderNoList = ["CK", "RK", "TJ", "YS", "PK"];
        //单据号集合
        List<string> notityNoList = ["RKS", "CKS", "YKS", "TJS", "YSS", "PKS"];
        //任务号集合
        List<string> taskNoList = ["RKR", "CKR", "YKR", "PKR", "IF", "YSR", "TJR", "PKR"];


        if (orderNoList.Contains(flagStr))
        {
            //获取流水号
            maxNo = SelectOrderMaxNo(flagStr);
            if (!string.IsNullOrEmpty(maxNo))
            {
                maxNo = maxNo.Substring(2);
            }
        }
        else if (notityNoList.Contains(flagStr))
        {
            //获取单据号
            maxNo = SelectNotityMaxNo(flagStr);
            if (!string.IsNullOrEmpty(maxNo))
            {
                maxNo = maxNo.Substring(3);
            }
        }
        else if (taskNoList.Contains(flagStr))
        {
            //获取任务号
            maxNo = SelectTaskMaxNo(flagStr);
            if (!string.IsNullOrEmpty(maxNo))
            {
                maxNo = maxNo.Substring(3);
            }
        }
        else
        {

        }

        //获取数据库时间八位
        string date = GetDbData().Trim();
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
    /// <param name="inOutFlag">出入标志</param>
    /// <returns></returns>
    private string SelectOrderMaxNo(string inOutFlag)
    {
        string maxNoStr = "";
        if (string.IsNullOrWhiteSpace(inOutFlag))
        {
            return maxNoStr;
        }

        string maxNoSql;
        switch (inOutFlag.ToUpper())
        {
            case "CK": maxNoSql = $@"select max(ExportOrderNo) as maxNo
	from WmsExportOrder
	where ExportOrderNo like('CK%')"; break;
            case "RK": maxNoSql = $@"select max(ImportOrderNo) as maxNo
	from WmsImportOrder
	where ImportOrderNo like('RK%')"; break;
            //           case "TJ": maxNoSql = $@"select max(ExtractOrderNo) as maxNo
            //from WmsExtractOrder
            //where ExtractOrderNo like('TJ%')"; break;
            //           case "YS": maxNoSql = $@"select max(InspectOrderNo) as maxNo
            //from WmsInspectOrder
            //where InspectOrderNo like('YS%')"; break;
            //           case "PK": maxNoSql = $@"select max() as maxNo
            //from 
            //where  like('PK%')"; break;
            case "": maxNoSql = ""; break;
            default: maxNoSql = ""; break;
        }
        maxNoStr = sqlClient.Ado.GetString(maxNoSql);

        return maxNoStr;
    }


    /// <summary>
    /// 获取最大单据号
    /// </summary>
    /// <param name="inOutFlag">出入标志</param>
    /// <returns></returns>
    private string SelectNotityMaxNo(string inOutFlag)
    {
        string maxNoStr = "";
        if (string.IsNullOrWhiteSpace(inOutFlag))
        {
            return maxNoStr;
        }

        string maxNoSql;
        switch (inOutFlag.ToUpper())
        {
            case "CKS": maxNoSql = $@"select max(ExportBillCode) as maxNo
	from WmsExportNotify
	where ExportBillCode like('CKS%')"; break;
            case "RKS": maxNoSql = $@"select max(ImportBillCode) as maxNo
	from WmsImportNotify
	where ImportBillCode like('RKS%')"; break;
            case "YKS": maxNoSql = $@"select max(MoveBillCode) as maxNo
	from WmsMoveNotify
	where MoveBillCode like('YKS%')"; break;
            //           case "TJS": maxNoSql = $@"select max(ExtractBillCode) as maxNo
            //from WmsExtractNotify
            //where ExtractBillCode like('TJS%')"; break;
            //           case "YSS": maxNoSql = $@"select max(InspectBillCode) as maxNo
            //from WmsInspectNotity
            //where InspectBillCode like('YSS%')"; break;
            case "PKS": maxNoSql = $@"select max(CheckBillCode) as maxNo
	from WmsStockCheckNotify
	where CheckBillCode like('PKS%')"; break;
            case "": maxNoSql = ""; break;
            default: maxNoSql = ""; break;
        }
        maxNoStr = sqlClient.Ado.GetString(maxNoSql);

        return maxNoStr;
    }


    /// <summary>
    /// 获取最大任务号(入库任务、出库任务、移库任务、盘库任务)
    /// </summary>
    /// <param name="inOutFlag">出入标志</param>
    /// <returns></returns>
    private string SelectTaskMaxNo(string inOutFlag)
    {
        string maxNoStr = "";
        if (string.IsNullOrWhiteSpace(inOutFlag))
        {
            return maxNoStr;
        }

        string maxNoSql;
        switch (inOutFlag.ToUpper())
        {
            case "RKR": maxNoSql = $@"select max(TaskNo) as maxNo
	from WmsImportTask
	where TaskNo like('RKR%')"; break;
            case "CKR": maxNoSql = $@"select max(ExportTaskNo) as maxNo
	from WmsExportTask
	where ExportTaskNo like('CKR%')"; break;
            case "IF": maxNoSql = $@"select max(TaskId) as maxNo
	from WmsInterfaceLog
	where TaskId like('IF%')"; break;
            //           case "TJR": maxNoSql = $@"select max(ExtractTaskNo) as maxNo
            //from WmsExtractTask
            //where ExtractTaskNo like('TJR%')"; break;
            //           case "YSR": maxNoSql = $@"select max(InspectTaskNo) as maxNo
            //from WmsInspectTask
            //where InspectTaskNo like('YSR%')"; break;
            case "PKR": maxNoSql = $@"select max(CheckTaskNo) as maxNo
	from WmsStockCheckTask
	where CheckTaskNo like('PKR%')"; break;
            case "YKR": maxNoSql = $@"select max(TaskNo) as maxNo
	from WmsMoveTask
	where TaskNo like('YKR%')"; break;
            case "": maxNoSql = ""; break;
            default: maxNoSql = ""; break;
        }
        maxNoStr = sqlClient.Ado.GetString(maxNoSql);

        return maxNoStr;
    }


    /// <summary>
    /// 获取服务器时间八位yyyyMMdd
    /// </summary>
    /// <returns></returns>
    public string GetDbData()
    {
        string dataSql = $@"select convert(nvarchar(10),getdate(),112) as serverdate";
        return sqlClient.Ado.GetString(dataSql);
    }

}
