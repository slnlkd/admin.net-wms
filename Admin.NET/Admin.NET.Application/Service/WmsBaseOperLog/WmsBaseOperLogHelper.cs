// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Microsoft.AspNetCore.Http;
using NetTaste;
using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Admin.NET.Application.Service.WmsBaseOperLog;

/// <summary>
/// 业务操作日志静态帮助类
/// </summary>
public static class WmsBaseOperLogHelper
{
    private static IServiceScopeFactory _serviceScopeFactory;
    private static IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 初始化（在程序启动时调用）
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    }

    /// <summary>
    /// 记录操作日志（异步）
    /// （推荐使用）
    /// </summary>
    /// <param name="Module">模块名称</param>
    /// <param name="OperationType">操作类型：新增、修改、删除、审核等</param>
    /// <param name="BusinessNo">业务单据号/ID</param>
    /// <param name="OperationContent">操作内容（客户可读的详细描述）</param>
    /// <param name="BeforeData">修改前数据摘要</param>
    /// <param name="AfterData">修改后数据摘要</param>
    /// <param name="OperParam">隐藏的入参参数（JSON格式，开发人员使用，不对客户显示）</param>
    /// <param name="ExtraInfo">额外信息</param>
    /// <param name="ElapsedMs">操作耗时（毫秒）</param>
    /// <param name="IsOK">当前日志记录成功状态：true=成功，false=失败</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task RecordAsync(string Module, string OperationType, string BusinessNo,
        string OperationContent, object BeforeData = null, object AfterData = null,
        object OperParam = null, string ExtraInfo = null, long ElapsedMs = 0, bool IsOK = true)
    {
        if (_serviceScopeFactory == null)
            throw new InvalidOperationException("WmsBaseOperLogHelper 未初始化，请在程序启动时调用 Initialize 方法");

        using var scope = _serviceScopeFactory.CreateScope();
        var operLogService = scope.ServiceProvider.GetRequiredService<WmsBaseOperLogService>();

        await operLogService.Add(new AddWmsBaseOperLogInput()
        {
            Module = Module, // 模块名称
            OperationType = OperationType, // 操作类型
            BusinessNo = BusinessNo, // 单据号 （一般放主键Id） 也可自定义
            OperationContent = OperationContent, // 操作内容（自定义内容）
            BeforeDataSummary = BeforeData != null ? JsonConvert.SerializeObject(BeforeData, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            }) : null, // 只用于修改，修改【前】 数据结果
            AfterDataSummary = AfterData != null ? JsonConvert.SerializeObject(AfterData, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            }) : null, // 只用于修改， 修改【后】 数据结果
            OperParam = OperParam != null ? JsonConvert.SerializeObject(OperParam, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            }) : null, // 入参参数
            ExtraInfo = ExtraInfo, // 扩展信息
            ElapsedMs = ElapsedMs, // 毫秒
            CreateTime = DateTime.Now, // 创建时间 （无意义 符合框架实体）
            Id = SnowFlakeSingle.instance.NextId(), // 使用雪花Id生成器
            OperateIp = $"{Environment.MachineName}【{GetClientIp()}】", // 机器名称 + IP地址
            OperateTime = DateTime.Now, // 操作时间
            Operator = App.User?.FindFirst(ClaimConst.RealName)?.Value ?? "系统", // 使用用户名而不是用户ID
            Result = IsOK ? "成功" : "失败", // 操作结果
            TenantId = App.User?.FindFirst(ClaimConst.TenantId)?.ParseToLong(), // 租户ID （当前项目 暂时没有 这个概念）
            TraceId = App.GetTraceId(), // 请求追踪ID（可以追踪到系统自带的操作日志，前端已实现查看详情）
            UpdateTime = DateTime.Now // 修改时间 （无意义 符合框架实体）
        });
    }

    /// <summary>
    /// 执行并记录操作
    /// </summary>
    public static async Task<T> ExecuteAndLogAsync<T>(string Module, string OperationType,
        Func<Task<T>> operation, Func<T, string> getBusinessNo = null,
        Func<T, string> getOperationContent = null, object OperParam = null,
        Func<Task<object>> getBeforeData = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 获取修改前的数据
            object beforeData = null;
            if (getBeforeData != null)
            {
                beforeData = await getBeforeData();
            }

            // 执行操作
            var result = await operation();
            stopwatch.Stop();

            // 获取业务单据号
            string businessNo = getBusinessNo?.Invoke(result) ?? result?.ToString();

            // 获取操作内容
            string operationContent = getOperationContent?.Invoke(result) ??
                                    $"{OperationType}{Module}：{businessNo}";

            // 记录成功日志
            await RecordAsync(
                Module: Module,
                OperationType: OperationType,
                BusinessNo: businessNo,
                OperationContent: operationContent,
                BeforeData: beforeData,
                AfterData: result,
                OperParam: OperParam,
                ExtraInfo: $"执行时间：{stopwatch.ElapsedMilliseconds}ms",
                ElapsedMs: stopwatch.ElapsedMilliseconds
            );

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // 记录失败日志
            await RecordAsync(
                Module: Module,
                OperationType: OperationType,
                BusinessNo: null,
                OperationContent: $"{OperationType}{Module}失败",
                OperParam: OperParam,
                ExtraInfo: $"错误信息：{ex.Message}，执行时间：{stopwatch.ElapsedMilliseconds}ms",
                ElapsedMs: stopwatch.ElapsedMilliseconds,
                IsOK: false
            );
            throw;
        }
    }

    /// <summary>
    /// 执行并记录操作（无返回值）
    /// </summary>
    public static async Task ExecuteAndLogAsync(string Module, string OperationType,
        Func<Task> operation, string BusinessNo = null, string OperationContent = null,
        object OperParam = null, Func<Task<object>> getBeforeData = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 获取修改前的数据
            object beforeData = null;
            if (getBeforeData != null)
            {
                beforeData = await getBeforeData();
            }

            // 执行操作
            await operation();
            stopwatch.Stop();

            // 记录成功日志
            await RecordAsync(
                Module: Module,
                OperationType: OperationType,
                BusinessNo: BusinessNo,
                OperationContent: OperationContent ?? $"{OperationType}{Module}：{BusinessNo}",
                BeforeData: beforeData,
                OperParam: OperParam,
                ExtraInfo: $"执行时间：{stopwatch.ElapsedMilliseconds}ms",
                ElapsedMs: stopwatch.ElapsedMilliseconds
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // 记录失败日志
            await RecordAsync(
                Module: Module,
                OperationType: OperationType,
                BusinessNo: BusinessNo,
                OperationContent: $"{OperationType}{Module}失败：{BusinessNo}",
                OperParam: OperParam,
                ExtraInfo: $"错误信息：{ex.Message}，执行时间：{stopwatch.ElapsedMilliseconds}ms",
                ElapsedMs: stopwatch.ElapsedMilliseconds,
                IsOK: false
            );

            throw;
        }
    }

    /// <summary>
    /// 获取当前登录人IP地址
    /// </summary>
    /// <returns></returns>
    private static string GetClientIp()
    {
        return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// 简单提取注释（使用Description特性）
    /// </summary>
    public static string GetComments<T>() where T : class
    {
        var type = typeof(T);
        var properties = type.GetProperties();
        var comments = properties
            .Select(p => p.GetCustomAttribute<DescriptionAttribute>())
            .Where(attr => attr != null)
            .Select(attr => attr.Description)
            .ToArray();

        return string.Join("，", comments);
    }

    /// <summary>
    /// 根据属性值生成描述
    /// 属性想生效需要使用 Description 特性进行标注 下方是实例
    /// [Description("单位缩写名称")]
    /// 会根据属性值生成类似如下描述：
    /// UnitName = "测试单位名称",
    /// UnitCode = "TEST001"
    /// 输出：单位名称：测试单位名称，单位编码：TEST001
    /// </summary>
    public static string GenerateDescription<T>(T entity) where T : class
    {
        if (entity == null) return string.Empty;

        var type = typeof(T);
        var properties = type.GetProperties();
        var descriptions = new List<string>();

        foreach (var property in properties)
        {
            var descriptionAttr = property.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttr != null)
            {
                var value = property.GetValue(entity);
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    descriptions.Add($"{descriptionAttr.Description}：{value}");
                }
            }
        }

        return string.Join("，", descriptions);
    }

    /// <summary>
    /// 根据属性值生成变更描述
    /// 只显示被修改的字段，格式：字段名：旧值 → 新值
    /// </summary>
    public static string GenerateChangeDescription<T>(T before, T after) where T : class
    {
        if (before == null || after == null)
            return "数据为空，无法比较";

        var changes = new List<string>();
        var properties = typeof(T).GetProperties();

        // 定义要跳过的属性
        var skipProperties = new[] { "Id", "CreateTime", "CreateUserId", "CreateUserName",
                               "UpdateTime", "UpdateUserId", "UpdateUserName" };

        foreach (var property in properties)
        {
            if (skipProperties.Contains(property.Name) || !property.CanRead)
                continue;

            var beforeValue = property.GetValue(before);
            var afterValue = property.GetValue(after);

            if (!IsEqual(beforeValue, afterValue))
            {
                var displayName = GetPropertyDisplayName(property);
                changes.Add($"{displayName}：{FormatValue(beforeValue)} → {FormatValue(afterValue)}");
            }
        }

        return $"{string.Join("\n", changes)}";
    }

    /// <summary>
    /// 比较两个值是否相等
    /// </summary>
    private static bool IsEqual(object value1, object value2)
    {
        if (value1 == null && value2 == null) return true;
        if (value1 == null || value2 == null) return false;

        // 处理字符串特殊情况
        if (value1 is string str1 && value2 is string str2)
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);

        // 处理数值类型
        if (value1.GetType().IsValueType && value2.GetType().IsValueType)
            return value1.Equals(value2);

        // 默认比较
        return Equals(value1, value2);
    }

    /// <summary>
    /// 获取属性显示名称
    /// </summary>
    private static string GetPropertyDisplayName(PropertyInfo property)
    {
        // 优先使用Description特性
        var descriptionAttr = property.GetCustomAttribute<DescriptionAttribute>();
        if (descriptionAttr != null)
            return descriptionAttr.Description;

        // 其次使用DisplayName特性
        var displayNameAttr = property.GetCustomAttribute<DisplayNameAttribute>();
        if (displayNameAttr != null)
            return displayNameAttr.DisplayName;

        // 最后使用属性名
        return property.Name;
    }

    /// <summary>
    /// 格式化值显示
    /// </summary>
    private static string FormatValue(object value)
    {
        if (value == null) return "空";

        switch (value)
        {
            case string str when string.IsNullOrWhiteSpace(str):
                return "空";
            case DateTime date:
                return date.ToString("yyyy-MM-dd HH:mm");
            case bool boolVal:
                return boolVal ? "是" : "否";
            case decimal decimalVal:
                return decimalVal.ToString("F2");
            case double doubleVal:
                return doubleVal.ToString("F2");
            case float floatVal:
                return floatVal.ToString("F2");
            default:
                return value.ToString();
        }
    }
    private static object obj = new object();
    /// <summary>
    /// 写LOG到本地文件方法,message="存入信息",path="存储路径"
    /// </summary>
    /// <param name="message"></param>
    /// <param name="path"></param>
    public static void SaveLogToFile(string message, string path = @"D:/log/defaultlog.txt")
    {
        if (path == null) return;
        lock (obj)
        {
            FileInfo fi = null;
            StreamWriter logWriter = null;
            try
            {
                int fileLength = 1024 * 1024 * 5;//5MB
                fi = new FileInfo(path);
                if (!fi.Directory.Exists) { fi.Directory.Create(); }
                if (fi.Exists && fi.Length > fileLength)
                {
                    string newpath = path.Insert(path.LastIndexOf('.'), "_" + DateTime.Now.ToString("HHmmss"));
                    File.Move(path, newpath);
                }
                logWriter = new StreamWriter(path, true);
                logWriter.WriteLine(DateTime.Now.ToString() + ">>>" + message);
                logWriter.WriteLine("----->.");
            }
            catch (Exception ex)
            {
                //throw ex;
            }
            finally
            {
                try
                {
                    fi = null;
                    if (logWriter != null)
                    {
                        logWriter.Close();
                        logWriter.Dispose();
                    }
                }
                catch { }
            }
        }
    }
}