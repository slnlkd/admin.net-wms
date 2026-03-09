// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护使用本项目应遵守相关法律法规和许可证的要求
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证(版本 2.0)进行分发和使用许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动!任何基于本项目二次开发而产生的一切法律纠纷和责任,我们不承担任何责任!

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsPda.Dto;
using Admin.NET.Core;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Admin.NET.Application.Service.WmsPda.Process;

/// <summary>
/// PDA 业务处理基类服务
/// <para>提供PDA业务处理的公共辅助方法,消除代码重复,统一业务逻辑处理模式</para>
/// </summary>
/// <remarks>
/// 主要功能包括:
/// <list type="bullet">
/// <item><description>事务执行封装:统一SqlSugar事务处理</description></item>
/// <item><description>结果对象创建:PdaLegacyResult、PdaActionResult的构造</description></item>
/// <item><description>参数验证:输入参数、字符串、数值的验证</description></item>
/// <item><description>数据解析:字符串到数值的安全解析</description></item>
/// <item><description>用户信息规范化:统一操作人信息</description></item>
/// <item><description>日志记录与异常处理:带日志记录的异步操作包装器</description></item>
/// <item><description>实体操作辅助:软删除、查询验证、批量更新审计字段</description></item>
/// <item><description>日期时间处理:时间获取、格式化、数值格式化</description></item>
/// </list>
/// </remarks>
public abstract class PdaBase
{   
    protected readonly ISqlSugarClient _sqlSugarClient;  
    /// <summary>
    /// 构造函数,注入SqlSugar客户端和库存托盘仓储
    /// </summary>
    /// <param name="sqlSugarClient">SqlSugar客户端实例</param>
    protected PdaBase(ISqlSugarClient sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
    }

    #region 事务执行

    /// <summary>
    /// 封装 SqlSugar 事务执行,统一处理异常与兜底提示
    /// </summary>
    /// <param name="action">需要在事务中执行的逻辑</param>
    /// <param name="errorMessage">失败时的默认提示</param>
    /// <exception cref="Exception">事务执行失败时抛出异常</exception>
    /// <remarks>
    /// 功能说明:
    /// <list type="bullet">
    /// <item><description>自动开启SqlSugar事务</description></item>
    /// <item><description>执行指定的业务逻辑</description></item>
    /// <item><description>失败时抛出具体异常或默认错误消息</description></item>
    /// <item><description>确保事务的原子性和一致性</description></item>
    /// </list>
    /// </remarks>
    protected async Task ExecuteInTransactionAsync(Func<Task> action, string errorMessage)
    {
        var tranResult = await _sqlSugarClient.Ado.UseTranAsync(action);
        if (!tranResult.IsSuccess)
        {
            throw tranResult.ErrorException ?? new Exception(errorMessage);
        }
    }

    #endregion

    #region PDASQL 视图结果对象创建

    /// <summary>
    /// 创建 PDA SQL视图结果对象(失败状态)
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="errorMessage">错误消息</param>
    /// <param name="defaultData">默认数据,默认为default(T)</param>
    /// <returns>PDASQL视图结果对象</returns>
    /// <remarks>
    /// 返回格式:
    /// <list type="bullet">
    /// <item><description>Code: 0(表示失败)</description></item>
    /// <item><description>Count: 0</description></item>
    /// <item><description>Msg: 错误消息</description></item>
    /// <item><description>Data: 默认数据</description></item>
    /// </list>
    /// </remarks>
    protected static PdaLegacyResult<T> CreateLegacyResult<T>(string errorMessage, T defaultData = default)
    {
        return new PdaLegacyResult<T>
        {
            Code = 0,
            Count = 0,
            Msg = errorMessage,
            Data = defaultData
        };
    }

    /// <summary>
    /// 创建 PDA SQL视图结果对象(通用版本,支持自定义Code)
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="code">状态码,通常1表示成功,0表示失败</param>
    /// <param name="message">消息</param>
    /// <param name="data">数据</param>
    /// <param name="count">数量,可选,如果未指定则从data推断</param>
    /// <returns>PDASQL视图结果对象</returns>
    /// <remarks>
    /// Count推断规则:
    /// <list type="number">
    /// <item><description>如果count参数有值,使用指定值</description></item>
    /// <item><description>如果data是集合类型,使用集合的Count</description></item>
    /// <item><description>如果data不为null,设置为1</description></item>
    /// <item><description>否则设置为0</description></item>
    /// </list>
    /// </remarks>
    protected static PdaLegacyResult<T> CreateLegacyResult<T>(int code, string message, T data = default, int? count = null)
    {
        var result = new PdaLegacyResult<T>
        {
            Code = code,
            Msg = message,
            Data = data
        };

        // 如果 count 未指定,尝试从 data 推断(如果是集合)
        if (count.HasValue)
        {
            result.Count = count.Value;
        }
        else if (data is System.Collections.ICollection collection)
        {
            result.Count = collection.Count;
        }
        else if (data != null)
        {
            result.Count = 1;
        }
        else
        {
            result.Count = 0;
        }

        return result;
    }

    /// <summary>
    /// 创建 PDA SQL视图结果对象(成功状态)
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="data">数据</param>
    /// <param name="successMessage">成功消息,默认为空字符串</param>
    /// <returns>PDASQL视图结果对象</returns>
    /// <remarks>
    /// 返回格式:
    /// <list type="bullet">
    /// <item><description>Code: 1(表示成功)</description></item>
    /// <item><description>Count: 根据数据类型自动推断</description></item>
    /// <item><description>Msg: 成功消息</description></item>
    /// <item><description>Data: 实际数据</description></item>
    /// </list>
    /// </remarks>
    protected static PdaLegacyResult<T> CreateLegacyResultSuccess<T>(T data, string successMessage = "")
    {
        var count = data switch
        {
            System.Collections.ICollection collection => collection.Count,
            _ => 0
        };

        return new PdaLegacyResult<T>
        {
            Code = 1,
            Count = count,
            Msg = successMessage,
            Data = data
        };
    }

    /// <summary>
    /// 创建 PDA SQL视图结果对象(object版本,向后兼容)
    /// </summary>
    /// <param name="code">状态码</param>
    /// <param name="message">消息</param>
    /// <param name="data">数据</param>
    /// <param name="count">数量</param>
    /// <returns>PDASQL视图结果对象</returns>
    protected static PdaLegacyResult<object> CreateLegacyResultObject(int code, string message, object data = null, int count = 0)
    {
        return CreateLegacyResult<object>(code, message, data, count);
    }

    #endregion

    #region PDA操作结果对象创建

    /// <summary>
    /// 创建 PDA 操作结果对象(失败状态)
    /// </summary>
    /// <param name="errorMessage">错误消息</param>
    /// <returns>PDA操作结果对象</returns>
    /// <remarks>
    /// 返回格式:
    /// <list type="bullet">
    /// <item><description>Success: false</description></item>
    /// <item><description>Message: 错误消息</description></item>
    /// </list>
    /// </remarks>
    protected static PdaActionResult CreateActionResult(string errorMessage)
    {
        return new PdaActionResult
        {
            Success = false,
            Message = errorMessage
        };
    }

    /// <summary>
    /// 创建 PDA 操作结果对象(成功状态)
    /// </summary>
    /// <param name="successMessage">成功消息,默认为"操作成功"</param>
    /// <returns>PDA操作结果对象</returns>
    /// <remarks>
    /// 返回格式:
    /// <list type="bullet">
    /// <item><description>Success: true</description></item>
    /// <item><description>Message: 成功消息</description></item>
    /// </list>
    /// </remarks>
    protected static PdaActionResult CreateActionResultSuccess(string successMessage = "操作成功")
    {
        return new PdaActionResult
        {
            Success = true,
            Message = successMessage
        };
    }

    /// <summary>
    /// 创建成功操作结果(快捷方法)
    /// </summary>
    /// <param name="message">成功消息,默认为"操作成功"</param>
    /// <returns>操作结果</returns>
    protected static PdaActionResult CreateSuccessResult(string message = "操作成功")
    {
        return CreateActionResultSuccess(message);
    }

    #endregion

    #region 参数验证

    /// <summary>
    /// 验证输入参数不为空
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="input">输入参数</param>
    /// <param name="errorMessage">自定义错误消息,默认为"请求参数不能为空!"</param>
    /// <exception cref="Exception">当输入参数为null时抛出异常</exception>
    /// <remarks>
    /// 使用场景:
    /// <list type="bullet">
    /// <item><description>验证API接口的输入DTO不为null</description></item>
    /// <item><description>确保业务逻辑处理前数据完整性</description></item>
    /// </list>
    /// </remarks>
    protected static void ValidateInput<T>(T input, string errorMessage = "请求参数不能为空!")
    {
        if (input == null)
            throw Oops.Bah(errorMessage);
    }

    /// <summary>
    /// 验证字符串参数不为空
    /// </summary>
    /// <param name="value">字符串值</param>
    /// <param name="fieldName">字段名称,用于生成默认错误消息</param>
    /// <param name="errorMessage">自定义错误消息,为空时使用默认消息</param>
    /// <returns>验证并去除空格后的字符串</returns>
    /// <exception cref="Exception">当字符串为null、空或仅包含空白字符时抛出异常</exception>
    /// <remarks>
    /// 功能说明:
    /// <list type="bullet">
    /// <item><description>验证字符串不为null、空或纯空格</description></item>
    /// <item><description>自动去除前后空格</description></item>
    /// <item><description>支持自定义或默认错误消息</description></item>
    /// </list>
    /// </remarks>
    protected static string ValidateString(string value, string fieldName, string errorMessage = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw Oops.Bah(errorMessage ?? $"{fieldName}不能为空!");
        return value.Trim();
    }

    #endregion

    #region 数据解析

    /// <summary>
    /// 解析必填十进制数
    /// </summary>
    /// <param name="raw">原始字符串</param>
    /// <param name="fieldName">字段名称,用于生成错误消息</param>
    /// <returns>解析后的十进制数</returns>
    /// <exception cref="Exception">当解析失败或值小于0时抛出异常</exception>
    /// <remarks>
    /// 验证规则:
    /// <list type="bullet">
    /// <item><description>字符串必须能解析为decimal类型</description></item>
    /// <item><description>解析后的值不能小于0</description></item>
    /// <item><description>使用InvariantCulture确保跨文化兼容性</description></item>
    /// </list>
    /// </remarks>
    protected static decimal ParseRequiredDecimal(string raw, string fieldName)
    {
        if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            throw Oops.Bah($"{fieldName}格式错误!");
        }
        if (value < 0)
        {
            throw Oops.Bah($"{fieldName}不能小于 0!");
        }
        return value;
    }

    /// <summary>
    /// 解析必填长整型字段,失败时抛出友好异常
    /// </summary>
    /// <param name="value">原始字符串</param>
    /// <param name="fieldName">字段说明,用于生成错误消息</param>
    /// <returns>解析后的长整型值</returns>
    /// <exception cref="Exception">当字符串为空或解析失败时抛出异常</exception>
    /// <remarks>
    /// 验证规则:
    /// <list type="bullet">
    /// <item><description>字符串不能为null或空白</description></item>
    /// <item><description>字符串必须能解析为long类型</description></item>
    /// <item><description>解析失败时提供友好的错误提示</description></item>
    /// </list>
    /// </remarks>
    protected static long ParseRequiredLong(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value) || !long.TryParse(value, out var result))
        {
            throw Oops.Bah($"{fieldName}格式错误!");
        }
        return result;
    }

    /// <summary>
    /// 安全解析 decimal,解析失败返回 0
    /// </summary>
    /// <param name="value">原始字符串,可为null</param>
    /// <returns>解析后的decimal值,失败时返回0</returns>
    /// <remarks>
    /// 功能说明:
    /// <list type="bullet">
    /// <item><description>null或空字符串返回0</description></item>
    /// <item><description>解析失败返回0而不抛出异常</description></item>
    /// <item><description>使用InvariantCulture确保跨文化兼容性</description></item>
    /// <item><description>适用于可选数值字段的解析</description></item>
    /// </list>
    /// </remarks>
    protected static decimal ParseDecimalOrZero(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0m;

        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0m;
    }

    /// <summary>
    /// 尝试将字符串解析为长整型,失败时返回null
    /// </summary>
    /// <param name="value">原始字符串</param>
    /// <returns>解析成功时返回long值,失败时返回null</returns>
    /// <remarks>
    /// 使用场景:
    /// <list type="bullet">
    /// <item><description>解析可能是ID或编码的字符串</description></item>
    /// <item><description>先尝试按ID查询,失败再按编码查询</description></item>
    /// <item><description>不抛出异常,适合条件分支逻辑</description></item>
    /// </list>
    /// </remarks>
    protected static long? TryParseLong(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return long.TryParse(value, out var result) ? result : null;
    }

    #endregion

    #region 用户信息规范化

    /// <summary>
    /// 统一处理操作人,默认回退到 PDA
    /// </summary>
    /// <param name="user">原始用户名,可能为空或包含空格</param>
    /// <returns>规范化后的用户名,如果为空则返回"PDA"</returns>
    /// <remarks>
    /// 规范化规则:
    /// <list type="bullet">
    /// <item><description>如果用户名为空、null或仅包含空白字符,返回"PDA"</description></item>
    /// <item><description>如果用户名不为空,去除前后空格后返回</description></item>
    /// <item><description>用于统一系统中操作人的记录格式</description></item>
    /// </list>
    /// </remarks>
    protected static string NormalizeUser(string user)
    {
        return string.IsNullOrWhiteSpace(user) ? "PDA" : user.Trim();
    }

    #endregion

    #region 数值格式化

    /// <summary>
    /// 将数量格式化为字符串，保留四位小数
    /// </summary>
    /// <param name="value">值</param>
    /// <returns>格式化后的值</returns>
    protected static string FormatDecimal(decimal? value)
    {
        if (!value.HasValue)
        {
            return null;
        }
        return value.Value.ToString("0.####");
    }

    #endregion

    #region 日志记录与异常处理

    /// <summary>
    /// 执行带日志记录的异步操作，统一日志格式与异常处理
    /// </summary>
    /// <typeparam name="T">操作返回值类型</typeparam>
    /// <param name="logger">日志记录器实例</param>
    /// <param name="operationName">操作名称，用于日志记录</param>
    /// <param name="operation">需要执行的异步操作</param>
    /// <param name="parameters">操作参数，可选，用于日志记录</param>
    /// <returns>操作执行结果</returns>
    /// <exception cref="Exception">操作执行失败时重新抛出原始异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>执行前记录开始日志，包含操作名称和参数</description></item>
    /// <item><description>执行指定的异步操作函数</description></item>
    /// <item><description>执行成功时记录结果日志</description></item>
    /// <item><description>执行失败时记录错误日志并重新抛出异常</description></item>
    /// <item><description>统一日志格式，便于问题追踪和性能分析</description></item>
    /// </list>
    /// 使用示例：
    /// <code>
    /// return await ExecuteWithLoggingAsync(_logger, "删除拆跺信息", async () =>
    /// {
    ///     // 业务逻辑
    ///     return CreateSuccessResult();
    /// }, new { input.Id, input.BoxCode });
    /// </code>
    /// </remarks>
    protected static async Task<T> ExecuteWithLoggingAsync<T>(
        ILogger logger,
        string operationName,
        Func<Task<T>> operation,
        object parameters = null)
    {
        if (parameters != null)
            logger.LogInformation("开始执行{Operation}，参数：{@Parameters}", operationName, parameters);
        else
            logger.LogInformation("开始执行{Operation}", operationName);

        try
        {
            var result = await operation();
            if (result != null)
                logger.LogInformation("{Operation}执行成功，结果：{@Result}", operationName, result);
            else
                logger.LogInformation("{Operation}执行成功", operationName);
            return result;
        }
        catch (Exception ex)
        {
            if (parameters != null)
                logger.LogError(ex, "{Operation}执行失败，参数：{@Parameters}", operationName, parameters);
            else
                logger.LogError(ex, "{Operation}执行失败", operationName);
            throw;
        }
    }

    #endregion

    #region 实体操作辅助方法

     

    /// <summary>
    /// 查询实体，不存在时抛出异常
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="repository">实体仓储</param>
    /// <param name="predicate">查询条件表达式</param>
    /// <param name="errorMessage">实体不存在时的错误消息</param>
    /// <returns>查询到的实体</returns>
    /// <exception cref="Exception">当实体不存在时抛出异常</exception>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>根据指定条件查询实体</description></item>
    /// <item><description>实体不存在时抛出友好的业务异常</description></item>
    /// <item><description>简化"查询-验证存在性"的重复代码</description></item>
    /// <item><description>统一异常处理逻辑</description></item>
    /// </list>
    /// 使用示例：
    /// <code>
    /// var notify = await GetEntityOrThrowAsync(
    ///     _importNotifyRepo,
    ///     x => x.ImportBillCode == billCode && x.IsDelete == false,
    ///     "入库单据单不存在");
    /// </code>
    /// </remarks>
    protected static async Task<T> GetEntityOrThrowAsync<T>(
        SqlSugarRepository<T> repository,
        Expression<Func<T, bool>> predicate,
        string errorMessage)
        where T : class, new()
    {
        var entity = await repository.GetFirstAsync(predicate);
        if (entity == null)
            throw Oops.Bah(errorMessage);
        return entity;
    }

    /// <summary>
    /// 批量更新实体的审计字段（UpdateTime、UpdateUserId、UpdateUserName）
    /// </summary>
    /// <typeparam name="T">实体类型，必须继承自 EntityBase</typeparam>
    /// <param name="entities">要更新的实体集合</param>
    /// <param name="userId">更新用户ID，默认为0</param>
    /// <param name="userName">更新用户名，默认为"PDA"</param>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>批量设置实体的更新时间为当前时间</description></item>
    /// <item><description>批量设置实体的更新用户ID和用户名</description></item>
    /// <item><description>减少重复代码，确保审计字段的一致性</description></item>
    /// <item><description>不执行数据库更新，仅修改内存中的对象</description></item>
    /// </list>
    /// 使用示例：
    /// <code>
    /// var items = new List{...};
    /// UpdateAuditFields(items, userId: 123, userName: "张三");
    /// await _repository.AsUpdateable(items).ExecuteCommandAsync();
    /// </code>
    /// </remarks>
    protected static void UpdateAuditFields<T>(
        IEnumerable<T> entities,
        long userId = 0,
        string userName = "PDA")
        where T : EntityBase
    {
        var now = DateTime.Now;
        foreach (var entity in entities)
        {
            entity.UpdateTime = now;
            entity.UpdateUserId = userId;
            entity.UpdateUserName = userName;
        }
    }

    #endregion

    #region 日期时间辅助方法

    /// <summary>
    /// 获取当前时间（便于单元测试时模拟时间）
    /// </summary>
    /// <returns>当前日期时间</returns>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>返回当前系统时间</description></item>
    /// <item><description>使用 virtual 修饰，便于单元测试时通过继承进行 mock</description></item>
    /// <item><description>统一时间获取逻辑，便于未来扩展（如时区处理）</description></item>
    /// </list>
    /// 使用场景：
    /// <list type="bullet">
    /// <item><description>设置实体的创建时间和更新时间</description></item>
    /// <item><description>记录操作日志的时间戳</description></item>
    /// <item><description>单元测试时可以固定时间值</description></item>
    /// </list>
    /// </remarks>
    protected virtual DateTime GetNow() => DateTime.Now;

    /// <summary>
    /// 格式化日期时间为标准字符串
    /// </summary>
    /// <param name="dateTime">日期时间，可为null</param>
    /// <param name="format">格式化字符串，默认为"yyyy-MM-dd HH:mm:ss"</param>
    /// <returns>格式化后的日期时间字符串，如果输入为null则返回空字符串</returns>
    /// <remarks>
    /// 功能说明：
    /// <list type="bullet">
    /// <item><description>将日期时间格式化为指定格式的字符串</description></item>
    /// <item><description>支持自定义格式化字符串</description></item>
    /// <item><description>安全处理 null 值，返回空字符串</description></item>
    /// <item><description>统一日期时间的显示格式</description></item>
    /// </list>
    /// 使用示例：
    /// <code>
    /// var dateStr = FormatDateTime(entity.CreateTime); // "2025-12-02 14:30:00"
    /// var dateOnly = FormatDateTime(entity.CreateTime, "yyyy-MM-dd"); // "2025-12-02"
    /// </code>
    /// </remarks>
    protected static string FormatDateTime(DateTime? dateTime, string format = "yyyy-MM-dd HH:mm:ss")
    {
        return dateTime?.ToString(format) ?? string.Empty;
    } 
    #endregion  
}
