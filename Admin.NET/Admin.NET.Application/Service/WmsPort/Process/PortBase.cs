// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证(版本 2.0)进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动!任何基于本项目二次开发而产生的一切法律纠纷和责任,我们不承担任何责任!
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Core;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlSugar;

namespace Admin.NET.Application.Service.WmsPort.Process;

/// <summary>
/// Port业务基类
/// 提供Port业务模块的通用功能,包括日志记录、事务处理、WCS集成等
/// </summary>
/// <remarks>
/// 功能概述:
/// 1. 统一日志记录 - 业务日志和异常日志的标准化记录
/// 2. 事务处理模板 - 简化事务操作,确保数据一致性
/// 3. WCS集成工具 - 与WCS系统交互的标准化方法
/// 4. 通用查询方法 - 实体查询和验证的复用逻辑
/// 5. 响应构建器 - 统一的成功/失败响应格式
///
/// 设计原则:
/// - DRY原则: 消除重复代码,提高维护效率
/// - 开闭原则: 对扩展开放,对修改关闭
/// - 依赖倒置: 依赖抽象接口而非具体实现
///
/// 使用方式:
/// 继承此基类并调用提供的保护方法:
/// <code>
/// public class PortImportOrder : PortBase
/// {
///     public PortImportOrder(ISqlSugarClient sqlSugarClient, ILogger logger)
///         : base(sqlSugarClient, logger, "入库流水") { }
///
///     public async Task ProcessOrder()
///     {
///         await RecordOperationLogAsync("ORDER001", "开始处理订单");
///         await ExecuteInTransactionAsync(async () =>
///         {
///             // 业务逻辑
///         }, "订单处理失败");
///     }
/// }
/// </code>
/// </remarks>
public abstract class PortBase
{
    #region 字段定义
    /// <summary>
    /// SqlSugar数据库客户端
    /// </summary>
    protected readonly ISqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 日志记录器
    /// </summary>
    protected readonly ILogger _logger;

    /// <summary>
    /// 业务模块名称(用于日志标识)
    /// </summary>
    protected readonly string _moduleName;
    #endregion

    #region 构造函数
    /// <summary>
    /// 基类构造函数
    /// </summary>
    /// <param name="sqlSugarClient">数据库客户端</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="moduleName">业务模块名称</param>
    protected PortBase(ISqlSugarClient sqlSugarClient, ILogger logger, string moduleName)
    {
        _sqlSugarClient = sqlSugarClient ?? throw new ArgumentNullException(nameof(sqlSugarClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _moduleName = string.IsNullOrWhiteSpace(moduleName) ? "未知模块" : moduleName;
    }
    #endregion

    #region 日志记录方法
    /// <summary>
    /// 记录业务操作日志(结构化日志)
    /// </summary>
    /// <param name="businessNo">业务编号(如订单号、任务号、托盘码等)</param>
    /// <param name="message">日志消息</param>
    /// <param name="param">附加参数对象(会被序列化为JSON)</param>
    /// <returns>异步任务</returns>
    /// <remarks>
    /// 使用场景:
    /// - 业务流程关键节点记录
    /// - 外部接口调用记录
    /// - 重要业务决策记录
    ///
    /// 日志格式:
    /// [模块名] 业务编号: xxx | 消息: xxx | 参数: {JSON} | 时间: yyyy-MM-dd HH:mm:ss.fff
    ///
    /// 示例:
    /// await RecordOperationLogAsync("RK20250121001", "入库任务创建成功", new { Qty = 100, SlotCode = "01-01-01-01" });
    /// </remarks>
    protected Task RecordOperationLogAsync(string businessNo, string message, object param = null)
    {
        var payload = param == null ? string.Empty : JsonConvert.SerializeObject(param, Formatting.Indented);
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        _logger.LogInformation(
            "[{ModuleName}] 业务编号: {BusinessNo} | 消息: {Message} | 参数: {Payload} | 时间: {Timestamp}",
            _moduleName, businessNo, message, payload, timestamp);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 记录异常日志(包含完整堆栈信息)
    /// </summary>
    /// <param name="businessNo">业务编号</param>
    /// <param name="operation">操作描述</param>
    /// <param name="exception">异常对象</param>
    /// <param name="additionalData">附加业务数据(可选)</param>
    /// <returns>异步任务</returns>
    /// <remarks>
    /// 使用场景:
    /// - 捕获异常时记录详细上下文
    /// - 外部系统调用失败
    /// - 数据验证失败
    ///
    /// 日志格式:
    /// [模块名异常] 业务编号: xxx | 操作: xxx | 异常类型: xxx | 异常消息: xxx | 附加数据: {JSON} | 时间: xxx
    ///
    /// 示例:
    /// catch (Exception ex)
    /// {
    ///     await RecordExceptionLogAsync("RK20250121001", "创建入库任务", ex, new { SlotCode = "01-01-01-01" });
    ///     throw;
    /// }
    /// </remarks>
    protected Task RecordExceptionLogAsync(string businessNo, string operation, Exception exception, object additionalData = null)
    {
        var additionalJson = additionalData == null ? string.Empty : JsonConvert.SerializeObject(additionalData, Formatting.Indented);
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        _logger.LogError(exception,
            "[{ModuleName}异常] 业务编号: {BusinessNo} | 操作: {Operation} | 异常类型: {ExceptionType} | 异常消息: {ExceptionMessage} | 附加数据: {AdditionalData} | 时间: {Timestamp}",
            _moduleName, businessNo, operation, exception.GetType().Name, exception.Message, additionalJson, timestamp);
        return Task.CompletedTask;
    }
    #endregion

    #region 事务处理方法
    /// <summary>
    /// 在事务中执行操作(无返回值)
    /// </summary>
    /// <param name="action">要执行的业务操作</param>
    /// <param name="errorMessage">失败时的错误提示</param>
    /// <returns>异步任务</returns>
    /// <remarks>
    /// 事务处理流程:
    /// 1. 开启数据库事务
    /// 2. 执行传入的业务操作
    /// 3. 提交事务
    /// 4. 如果发生异常,回滚事务并抛出友好异常
    ///
    /// 使用场景:
    /// 所有涉及多表操作的业务逻辑都应使用此方法包装,确保原子性
    ///
    /// 示例:
    /// await ExecuteInTransactionAsync(async () =>
    /// {
    ///     await _taskRep.InsertAsync(task);
    ///     await _slotRep.UpdateAsync(slot);
    /// }, "任务创建失败");
    /// </remarks>
    protected async Task ExecuteInTransactionAsync(Func<Task> action, string errorMessage)
    {
        var transaction = _sqlSugarClient.Ado;
        try
        {
            transaction.BeginTran();
            await action();
            transaction.CommitTran();
        }
        catch (Exception ex)
        {
            transaction.RollbackTran();
            throw Oops.Bah($"{errorMessage}: {ex.Message}");
        }
    }

    /// <summary>
    /// 在事务中执行操作(有返回值)
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="action">要执行的业务操作</param>
    /// <param name="errorMessage">失败时的错误提示</param>
    /// <returns>业务操作的返回值</returns>
    /// <remarks>
    /// 与无返回值版本的区别:
    /// - 支持返回业务处理结果
    /// - 适用于需要获取生成的ID、统计数据等场景
    ///
    /// 示例:
    /// var orderId = await ExecuteInTransactionAsync(async () =>
    /// {
    ///     var order = await _orderRep.InsertReturnEntityAsync(newOrder);
    ///     await _detailRep.InsertAsync(detail);
    ///     return order.Id;
    /// }, "订单创建失败");
    /// </remarks>
    protected async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, string errorMessage)
    {
        var transaction = _sqlSugarClient.Ado;
        try
        {
            transaction.BeginTran();
            var result = await action();
            transaction.CommitTran();
            return result;
        }
        catch (Exception ex)
        {
            transaction.RollbackTran();
            throw Oops.Bah($"{errorMessage}: {ex.Message}");
        }
    }
    #endregion

    #region 通用查询方法
    /// <summary>
    /// 查询实体,不存在则抛出异常
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="predicate">查询条件</param>
    /// <param name="errorMessage">实体不存在时的错误消息</param>
    /// <returns>查询到的实体</returns>
    /// <exception cref="Oops">实体不存在时抛出异常</exception>
    /// <remarks>
    /// 封装常见的"查询+验证存在性"模式,减少重复代码
    ///
    /// 使用场景:
    /// - 根据业务键查询必须存在的实体
    /// - 验证关联数据的完整性
    ///
    /// 示例:
    /// var task = await GetEntityOrThrowAsync(_taskRep,
    ///     t => t.TaskNo == input.TaskNo,
    ///     $"任务号不存在: {input.TaskNo}");
    /// </remarks>
    protected async Task<T> GetEntityOrThrowAsync<T>(
        SqlSugarRepository<T> repository,
        Expression<Func<T, bool>> predicate,
        string errorMessage) where T : class, new()
    {
        var entity = await repository.GetFirstAsync(predicate);
        if (entity == null)
            throw Oops.Bah(errorMessage);
        return entity;
    }
    #endregion

    #region WCS集成方法
    /// <summary>
    /// 验证WCS系统配置
    /// </summary>
    /// <returns>配置验证结果(是否有效, 服务地址, 错误消息)</returns>
    /// <remarks>
    /// 验证内容:
    /// 1. WCS服务地址是否已配置
    /// 2. 地址格式是否正确
    /// 3. 可选: 网络连通性检查
    ///
    /// 返回值说明:
    /// - IsValid: 配置是否有效
    /// - BaseHost: WCS服务基础地址
    /// - ErrorMessage: 验证失败时的错误信息
    ///
    /// 示例:
    /// var config = await ValidateWcsConfigurationAsync();
    /// if (!config.IsValid)
    /// {
    ///     return CreateMockWcsResponse("WCS未配置,模拟调用成功");
    /// }
    /// </remarks>
    protected Task<(bool IsValid, string BaseHost, string ErrorMessage)> ValidateWcsConfigurationAsync()
    {
        var host = WcsApiUrlDto.GetHost();
        if (string.IsNullOrWhiteSpace(host))
        {
            return Task.FromResult((false, string.Empty, "WCS服务地址未配置"));
        }
        if (!Uri.TryCreate(host, UriKind.Absolute, out var uri))
        {
            return Task.FromResult((false, string.Empty, $"WCS服务地址格式无效: {host}"));
        }
        return Task.FromResult((true, host, string.Empty));
    }

    /// <summary>
    /// 解析WCS返回结果
    /// </summary>
    /// <param name="response">WCS响应JSON字符串</param>
    /// <returns>解析结果(是否成功, 消息)</returns>
    /// <remarks>
    /// WCS标准响应格式:
    /// {
    ///     "stateCode": "1",    // 1=成功, 0=失败
    ///     "errMsg": "消息内容",
    ///     "timestamp": "2025-01-21 10:30:45"
    /// }
    ///
    /// 示例:
    /// var (success, message) = EvaluateWcsResponse(wcsResponse);
    /// if (!success)
    /// {
    ///     throw Oops.Bah($"WCS调用失败: {message}");
    /// }
    /// </remarks>
    protected (bool Success, string Message) EvaluateWcsResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return (false, "WCS未返回数据");

        try
        {
            var model = JsonConvert.DeserializeObject<WcsTaskResponse>(response);
            if (model == null)
                return (false, "WCS响应解析失败");

            var success = !string.Equals(model.stateCode, "0", StringComparison.OrdinalIgnoreCase);
            var message = string.IsNullOrWhiteSpace(model.errMsg)
                ? (success ? "操作成功" : "操作失败")
                : model.errMsg;

            return (success, message);
        }
        catch (Exception ex)
        {
            return (false, $"WCS响应解析异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建模拟的WCS成功响应
    /// </summary>
    /// <param name="message">成功消息</param>
    /// <returns>格式化的WCS响应JSON</returns>
    /// <remarks>
    /// 用于开发测试环境或WCS系统未就绪时的模拟响应
    ///
    /// 示例:
    /// return CreateMockWcsResponse("任务下发成功(模拟)");
    /// </remarks>
    protected string CreateMockWcsResponse(string message)
    {
        return JsonConvert.SerializeObject(new
        {
            stateCode = "1",
            errMsg = message,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    /// <summary>
    /// 创建错误格式的WCS响应
    /// </summary>
    /// <param name="errorMessage">错误消息</param>
    /// <returns>格式化的WCS错误响应JSON</returns>
    /// <remarks>
    /// 用于WCS调用异常时返回标准格式的错误响应
    ///
    /// 示例:
    /// return CreateErrorWcsResponse($"WCS调用超时: {ex.Message}");
    /// </remarks>
    protected string CreateErrorWcsResponse(string errorMessage)
    {
        return JsonConvert.SerializeObject(new
        {
            stateCode = "0",
            errMsg = errorMessage,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    /// <summary>
    /// 获取WCS API完整URL
    /// </summary>
    /// <param name="path">API路径(如: /api/task/create)</param>
    /// <returns>完整的API URL</returns>
    /// <remarks>
    /// 自动组合基础地址和API路径,处理斜杠问题
    ///
    /// 示例:
    /// var url = GetWcsApiUrl(WcsApiUrlDto.TaskApiUrl);
    /// // 返回: http://192.168.1.100:8080/api/task/create
    /// </remarks>
    protected string GetWcsApiUrl(string path)
    {
        var host = WcsApiUrlDto.GetHost();
        if (string.IsNullOrWhiteSpace(host))
            return string.Empty;

        return host.TrimEnd('/') + "/" + path.TrimStart('/');
    }
    #endregion 

    #region 响应构建方法
    /// <summary>
    /// 创建成功响应对象
    /// </summary>
    /// <param name="message">成功消息</param>
    /// <returns>标准成功响应对象</returns>
    /// <remarks>
    /// 标准响应格式:
    /// {
    ///     "code": 1,       // 1表示成功
    ///     "msg": "消息内容",
    ///     "count": 1,      // 处理记录数
    ///     "data": ""       // 附加数据
    /// }
    ///
    /// 示例:
    /// return CreateSuccessResult("入库任务创建成功");
    /// </remarks>
    protected object CreateSuccessResult(string message)
    {
        return new
        {
            code = 1,
            msg = message,
            count = 1,
            data = string.Empty
        };
    }

    /// <summary>
    /// 创建失败响应对象
    /// </summary>
    /// <param name="message">失败消息</param>
    /// <returns>标准失败响应对象</returns>
    /// <remarks>
    /// 标准响应格式:
    /// {
    ///     "code": 0,       // 0表示失败
    ///     "msg": "错误消息",
    ///     "count": 0,      // 处理记录数
    ///     "data": ""       // 附加数据
    /// }
    ///
    /// 示例:
    /// return CreateFailureResult("库存不足,无法创建任务");
    /// </remarks>
    protected object CreateFailureResult(string message)
    {
        return new
        {
            code = 0,
            msg = message,
            count = 0,
            data = string.Empty
        };
    }
     /// <summary>
    /// 解析状态字符串为整数
    /// </summary>
    /// <param name="value">状态字符串</param>
    /// <returns>状态整数值，如果解析失败则返回0</returns>
    protected static int ParseStatus(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? 0 : (int.TryParse(value, out var status) ? status : 0);
    }
    #endregion

    #region 实体软删除方法
    /// <summary>
    /// 软删除单个实体
    /// </summary>
    /// <typeparam name="T">实体类型，必须继承自EntityBaseDel</typeparam>
    /// <param name="repository">实体仓储</param>
    /// <param name="entityId">实体ID</param>
    /// <param name="errorMessage">实体不存在时的错误消息（可选）</param>
    /// <returns>异步任务</returns>
    /// <exception cref="Oops">当实体不存在且提供了errorMessage时抛出异常</exception>
    /// <remarks>
    /// 软删除是将实体的IsDelete字段设置为true，而不是从数据库中物理删除
    /// 这样可以保留数据历史记录，便于审计和数据恢复
    ///
    /// 使用场景:
    /// - 删除箱码信息
    /// - 删除入库流水
    /// - 删除其他业务数据
    ///
    /// 示例:
    /// await SoftDeleteEntityAsync(_boxInfoRep, boxInfo.Id, "箱码信息不存在");
    /// </remarks>
    protected async Task SoftDeleteEntityAsync<T>(SqlSugarRepository<T> repository,long entityId,string errorMessage = null) where T : EntityBaseDel, new()
    {
        var now = DateTime.Now;
        var affectedRows = await repository.AsUpdateable()
            .SetColumns(e => e.IsDelete == true)
            .SetColumns(e => e.UpdateTime == now)
            .Where(e => e.Id == entityId)
            .ExecuteCommandAsync();

        if (affectedRows == 0 && !string.IsNullOrEmpty(errorMessage))
            throw Oops.Bah(errorMessage);
    }

     
    #endregion

    #region 库位状态更新方法
    /// <summary>
    /// 更新库位状态
    /// </summary>
    /// <param name="slotRepository">库位仓储</param>
    /// <param name="slotCode">库位编码</param>
    /// <param name="newStatus">新的库位状态</param>
    /// <param name="errorMessage">库位不存在时的错误消息（可选，为空则不抛异常）</param>
    /// <returns>异步任务</returns>
    /// <exception cref="Oops">当库位不存在且提供了errorMessage时抛出异常</exception>
    /// <remarks>
    /// 库位状态说明:
    /// - 0: 空
    /// - 1: 有物品
    /// - 2: 入库中
    /// - 3: 出库中
    /// - 4: 移库出
    /// - 5: 移库入
    /// - 6: 空托盘
    ///
    /// 使用场景:
    /// - 入库完成后更新库位为"有物品"
    /// - 出库完成后更新库位为"空"
    /// - 移库时更新源库位和目标库位状态
    ///
    /// 示例:
    /// await UpdateSlotStatusAsync(_slotRep, "01-01-01-01", 1, "库位不存在");
    /// </remarks>
    protected async Task UpdateSlotStatusAsync(SqlSugarRepository<WmsBaseSlot> slotRepository,string slotCode,int newStatus,string errorMessage = null)
    {
        if (string.IsNullOrWhiteSpace(slotCode))
            return;

        var slot = await slotRepository.GetFirstAsync(s => s.SlotCode == slotCode);
        if (slot == null)
        {
            if (!string.IsNullOrEmpty(errorMessage))
                throw Oops.Bah(errorMessage ?? $"库位不存在: {slotCode}");
            return;
        }

        slot.SlotStatus = newStatus;
        slot.UpdateTime = DateTime.Now;
        await slotRepository.AsUpdateable(slot)
            .UpdateColumns(s => new { s.SlotStatus, s.UpdateTime })
            .ExecuteCommandAsync();
    } 
    
     
    /// <summary>
    /// 批量更新库位状态
    /// </summary>
    /// <param name="slotRepository">库位仓储</param>
    /// <param name="slotCodes">库位编码列表</param>
    /// <param name="newStatus">新的库位状态</param>
    /// <returns>异步任务</returns>
    /// <remarks>
    /// 批量更新多个库位的状态，提升性能（一次数据库操作）
    ///
    /// 使用场景:
    /// - 批量更新移库涉及的多个库位
    /// - 批量恢复任务取消后的库位状态
    ///
    /// 示例:
    /// await BatchUpdateSlotStatusAsync(_slotRep, new List&lt;string&gt; { "01-01-01-01", "01-01-01-02" }, 0);
    /// </remarks>
    protected async Task BatchUpdateSlotStatusAsync(SqlSugarRepository<WmsBaseSlot> slotRepository,List<string> slotCodes,int newStatus)
    {
        if (slotCodes == null || slotCodes.Count == 0)
            return;

        var now = DateTime.Now;
        await slotRepository.AsUpdateable()
            .SetColumns(s => s.SlotStatus == newStatus)
            .SetColumns(s => s.UpdateTime == now)
            .Where(s => slotCodes.Contains(s.SlotCode))
            .ExecuteCommandAsync();
    }
    #endregion

    #region 常量定义
    /// <summary>
    /// 库位恢复相关常量(用于任务取消时恢复库位状态)
    /// </summary>
    protected static class SlotRecovery
    {
        /// <summary>源库位状态(有物品)</summary>
        public const int SourceSlotStatus = 1;

        /// <summary>目标库位状态(空)</summary>
        public const int TargetSlotStatus = 0;
    }

    /// <summary>
    /// 库存操作相关常量
    /// </summary>
    protected static class StockOperations
    {
        /// <summary>托盘初始锁定数量</summary>
        public const decimal InitialLockQuantity = 0m;

        /// <summary>正常库存状态标识</summary>
        public const int NormalStatus = 0;

        /// <summary>非抽检托盘标识</summary>
        public const int NonSamplingTray = 0;

        /// <summary>整托标识</summary>
        public const int WholeTraySetting = 0;

        /// <summary>未拣货标识</summary>
        public const int NotPicked = 0;
    }

    /// <summary>
    /// 错误消息常量
    /// </summary>
    protected static class ErrorMessages
    {
        /// <summary>入库流水不存在</summary>
        public const string ImportOrderNotFound = "任务号{0}没有对应的入库流水";

        /// <summary>箱码未绑定</summary>
        public const string BoxNotBind = "请先绑定载具";

        /// <summary>物料信息不存在</summary>
        public const string MaterialNotFound = "物料信息不存在:MaterialId={0}";

        /// <summary>移库任务不存在</summary>
        public const string MoveTaskNotFound = "操作失败,移库任务不存在";

        /// <summary>操作失败前缀</summary>
        public const string OperationFailed = "操作失败,{0}";
    }

    /// <summary>
    /// 事务操作相关常量
    /// </summary>
    protected static class TransactionMessages
    {
        /// <summary>入库完成处理失败</summary>
        public const string ImportCompletionFail = "入库完成处理失败";

        /// <summary>移库任务取消处理失败</summary>
        public const string MoveCancellationFail = "移库任务取消处理失败";
    }

    /// <summary>
    /// 移库取消相关常量
    /// </summary>
    protected static class MoveCancellation
    {
        /// <summary>取消操作的消息类型</summary>
        public const string CancelMessageType = "取消移库任务";

        /// <summary>取消操作是否成功标识</summary>
        public const int CancelNotSuccessFlag = 0;

        /// <summary>取消操作未发送标识</summary>
        public const int CancelNotSentFlag = 0;

        /// <summary>取消操作未取消标识</summary>
        public const int CancelNotCancelledFlag = 0;

        /// <summary>取消操作未完成标识</summary>
        public const int CancelNotFinishedFlag = 0;

        /// <summary>取消操作不显示标识</summary>
        public const int CancelNotShowFlag = 0;
    }

    /// <summary>
    /// 响应消息相关常量
    /// </summary>
    protected static class ResponseMessages
    {
        /// <summary>入库完成消息</summary>
        public const string ImportComplete = "入库完成";

        /// <summary>入库移库完成消息</summary>
        public const string ImportMoveComplete = "入库移库完成";

        /// <summary>出库完成消息</summary>
        public const string ExportComplete = "出库完成";

        /// <summary>出库移库完成消息</summary>
        public const string ExportMoveComplete = "出库移库完成";

        /// <summary>移库完成消息</summary>
        public const string MoveComplete = "移库完成";

        /// <summary>盘库完成消息</summary>
        public const string StockCheckComplete = "盘库完成";

        /// <summary>入库任务已取消消息</summary>
        public const string ImportTaskCancelled = "入库任务已取消";

        /// <summary>出库任务已取消消息</summary>
        public const string ExportTaskCancelled = "出库任务已取消";

        /// <summary>移库任务已取消消息</summary>
        public const string MoveTaskCancelled = "移库任务已取消";

        /// <summary>盘库任务已取消消息</summary>
        public const string StockCheckTaskCancelled = "盘库任务已取消";

        /// <summary>取消成功消息</summary>
        public const string CancelSuccess = "取消成功";
    }
    #endregion
}
