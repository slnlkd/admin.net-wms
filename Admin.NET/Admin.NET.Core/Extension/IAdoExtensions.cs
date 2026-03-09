using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSugar;


/// <summary>
/// SqlSugar.IAdo扩展
/// </summary>
public static class IAdoExtensions
{
    /// <summary>
    /// 执行sql批处理
    /// </summary>
    /// <param name="ado">sqlSugarClient的ado对象</param>
    /// <param name="sqlList">要执行的sql的IEnumerable集合</param>
    /// <returns></returns>
    public static int ExecuteSqlBatch(this IAdo ado, IEnumerable<string> sqlList)
    {
        ado.BeginTran();
        try
        {
            foreach (string sql in sqlList)
            {
                // 执行每条SQL语句
                int result = ado.ExecuteCommand(sql);

                if (result == -1)
                {
                    return 0;
                }
            }
            // 提交事务  
            ado.CommitTran();//.CommitTransaction();
            return 1;
        }
        catch (Exception ex)
        {
            // 发生异常，回滚事务  
            ado.RollbackTran();
            return 0;
        }
    }


    /// <summary>
    /// 异步执行sql批处理
    /// </summary>
    /// <param name="ado">sqlSugarClient的ado对象</param>
    /// <param name="sqlList">要执行的sql的IEnumerable集合</param>
    /// <returns></returns>
    public static async Task<int> ExecuteSqlBatchAsync(this IAdo ado, IEnumerable<string> sqlList)
    {
        ado.BeginTran();
        try
        {
            foreach (string sql in sqlList)
            {
                // 执行每条SQL语句
                int result = await ado.ExecuteCommandAsync(sql);

                if (result == -1)
                {
                    return 0;
                }
            }
            // 提交事务  
            ado.CommitTran();//.CommitTransaction();
            return 1;
        }
        catch (Exception ex)
        {
            // 发生异常，回滚事务  
            ado.RollbackTran();
            return 0;
        }
    }

}

