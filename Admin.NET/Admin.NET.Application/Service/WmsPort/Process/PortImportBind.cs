// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using System;
using System.Threading.Tasks;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.BaseService;
using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Core;
using Furion.FriendlyException;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using SqlSugar;
namespace Admin.NET.Application.Service.WmsPort.Process;
/// <summary>
/// 空托盘组盘业务处理类，对应 JC35 `PortController.KBackConfirm` 的重构实现。
/// </summary>
/// <remarks>
/// 业务职责：
/// 1. 处理空托盘的绑定操作，将未使用的托盘标记为已使用
/// 2. 处理空托盘的解绑操作，将已绑定但未分配库位的托盘恢复为未使用状态
/// 3. 创建和管理入库流水记录
/// 4. 维护箱码信息和托盘状态的一致性
/// 5. 根据仓库类型自动选择合适的空托盘物料
///
/// 核心流程：
/// 1. 参数验证：验证输入参数的有效性和完整性
/// 2. 操作类型判断：区分绑定和解绑操作
/// 3. 事务处理：确保数据操作的原子性
/// 4. 业务逻辑执行：执行相应的绑定或解绑操作
/// 5. 结果返回：统一的响应格式
///
/// 技术特点：
/// - 使用SqlSugar ORM进行数据库操作
/// - 事务管理确保数据一致性
/// - 依赖注入提高可测试性
/// - 结构化日志记录
/// - 详细的参数验证和异常处理
/// </remarks>
public class PortImportBind : PortBase, ITransient
{
    #region 字段定义
    // 仓储聚合类
    private readonly PortBaseRepos _repos;
    // 业务服务
    private readonly CommonMethod _commonMethod;
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖仓储
    /// </summary>
    /// <param name="loggerFactory">日志工厂</param>
    /// <param name="repos">仓储聚合类</param>
    /// <param name="sqlSugarClient">数据库客户端</param>
    /// <param name="commonMethod">通用方法服务</param>
    public PortImportBind(
        ILoggerFactory loggerFactory,
        PortBaseRepos repos,
        ISqlSugarClient sqlSugarClient,
        CommonMethod commonMethod)
        : base(sqlSugarClient, loggerFactory.CreateLogger(CommonConst.PortImportBind), "空托绑定")
    {
        _repos = repos;
        _commonMethod = commonMethod;
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 处理空托盘组盘操作的主入口方法
    /// </summary>
    /// <param name="input">空托盘组盘输入参数</param>
    /// <returns>操作结果，包含是否成功和相关消息</returns>
    /// <remarks>
    /// 对应旧系统 <c>PortController.KBackConfirm</c>，支持以下操作：
    /// <list type="number">
    /// <item>绑定操作（actionType=add）：将未使用的托盘标记为已使用</item>
    /// <item>解绑操作（actionType=del）：将已绑定但未分配库位的托盘恢复为未使用状态</item>
    /// </list>
    ///
    /// 业务流程：
    /// 1. 参数预处理和验证
    /// 2. 根据操作类型执行相应的业务逻辑
    /// 3. 记录操作日志
    /// 4. 统一结果返回格式
    ///
    /// 技术特点：
    /// - 使用事务确保数据一致性
    /// - 详细的参数验证
    /// - 结构化的日志记录
    /// - 统一的异常处理机制
    /// </remarks>
    public async Task<EmptyTrayBindOutput> ProcessKBackConfirmAsync(EmptyTrayBindInput input)
    {
        var actionType = NormalizeActionType(input.ActionType);
        ValidateInputParameters(input, actionType);
        await RecordOperationLogAsync(input.PalletCode, "空托盘组盘开始", input);
        try
        {
            var result = await ExecuteEmptyTrayOperationAsync(input.PalletCode.Trim(), input.Quantity, actionType);
            var serializedResult = JsonConvert.SerializeObject(result, Formatting.Indented);
            await RecordOperationLogAsync(input.PalletCode, $"空托盘组盘成功：{serializedResult}", new { input, result });
            var (success, message) = ParseOperationResult(result);
            return new EmptyTrayBindOutput
            {
                Success = success,
                Message = message
            };
        }
        catch (Exception ex)
        {
            await RecordOperationLogAsync(input.PalletCode, $"空托盘组盘异常：{ex.Message}", input);
            _logger.LogError(ex, "空托盘组盘操作失败，托盘编码：{PalletCode}，操作类型：{ActionType}", input.PalletCode, actionType);
            throw;
        }
    }
    #endregion

    #region 私有方法 - 核心业务逻辑
    /// <summary>
    /// 执行空托盘操作的内部方法
    /// </summary>
    /// <param name="stockCode">托盘编码</param>
    /// <param name="quantity">数量（仅绑定操作时使用）</param>
    /// <param name="actionType">操作类型（add=绑定，del=解绑）</param>
    /// <returns>操作结果对象</returns>
    /// <remarks>
    /// 该方法使用事务确保数据一致性，根据操作类型调用相应的处理方法
    /// </remarks>
    private async Task<object> ExecuteEmptyTrayOperationAsync(string stockCode, int quantity, string actionType)
    {
        var ado = _sqlSugarClient.Ado;
        try
        {
            ado.BeginTran();
            if (EmptyTrayConstants.ActionTypes.IsBind(actionType))
            {
                await BindEmptyTrayAsync(stockCode, quantity);
                ado.CommitTran();
                return CreateSuccessResult("绑定成功！");
            }
            if (EmptyTrayConstants.ActionTypes.IsUnbind(actionType))
            {
                await UnbindEmptyTrayAsync(stockCode);
                ado.CommitTran();
                return CreateSuccessResult("解绑成功！");
            }
            throw Oops.Bah($"不支持的操作类型：{actionType}");
        }
        catch (Exception)
        {
            ado.RollbackTran();
            throw;
        }
    }
    #endregion

    #region 私有方法 - 辅助功能
    /// <summary>
    /// 标准化操作类型
    /// </summary>
    /// <param name="actionType">原始操作类型</param>
    /// <returns>标准化后的操作类型</returns>
    private static string NormalizeActionType(string actionType)
    {
        return string.IsNullOrWhiteSpace(actionType) ? EmptyTrayConstants.ActionTypes.Default : actionType.Trim().ToLowerInvariant();
    }
    /// <summary>
    /// 验证输入参数的有效性
    /// </summary>
    /// <param name="input">输入参数</param>
    /// <param name="actionType">操作类型</param>
    /// <exception cref="ArgumentException">当参数无效时抛出</exception>
    /// <remarks>
    /// 验证规则：
    /// 1. 输入参数不能为空
    /// 2. 托盘编码不能为空
    /// 3. 绑定操作时数量必须大于0
    /// </remarks>
    private static void ValidateInputParameters(EmptyTrayBindInput input, string actionType)
    {
        if (input == null)
            throw Oops.Bah("请求参数不能为空！");
        if (string.IsNullOrWhiteSpace(input.PalletCode))
            throw Oops.Bah("托盘编码不能为空！");
        if (EmptyTrayConstants.ActionTypes.IsBind(actionType) && input.Quantity <= 0)
            throw Oops.Bah("数量必须大于0！");
    }
    /// <summary>
    /// 解析操作结果
    /// </summary>
    /// <param name="result">操作结果对象</param>
    /// <returns>是否成功和消息的元组</returns>
    private static (bool Success, string Message) ParseOperationResult(object result)
    {
        if (result == null)
            return (false, "空托盘组盘失败：无返回结果");
        try
        {
            var json = JsonConvert.SerializeObject(result);
            var model = JsonConvert.DeserializeObject<KBackConfirmResponse>(json);
            if (model == null)
                return (false, "空托盘组盘失败：结果解析失败");
            var success = model.code == EmptyTrayConstants.SystemConstants.ResultCodeSuccess;
            var message = string.IsNullOrWhiteSpace(model.msg) ? (success ? "操作成功" : "操作失败") : model.msg;
            return (success, message);
        }
        catch
        {
            return (true, result.ToString());
        }
    }

    #endregion

    #region 私有方法 - 业务逻辑实现
    /// <summary>
    /// 绑定空托盘操作
    /// </summary>
    /// <param name="stockCode">托盘编码</param>
    /// <param name="quantity">绑定数量</param>
    /// <remarks>
    /// 绑定操作包含以下步骤：
    /// 1. 验证托盘的合法性（状态为未使用）
    /// 2. 检查托盘是否已经绑定
    /// 3. 创建入库流水记录
    /// 4. 根据仓库类型获取对应的空托盘物料
    /// 5. 创建箱码信息记录
    /// 6. 更新托盘状态为已使用
    /// </remarks>
    private async Task BindEmptyTrayAsync(string stockCode, int quantity)
    {
        var stockCodeEntity = await ValidateTrayForBindingAsync(stockCode);
        await CheckTrayNotBoundAsync(stockCode);
        var importOrder = await CreateImportOrderAsync(stockCodeEntity);
        var material = await GetEmptyTrayMaterialAsync(stockCodeEntity.WarehouseId);

        // 查询入库单据获取供应商和制造商信息（空托盘场景允许为null）
        WmsImportNotify? notify = null;
        if (importOrder?.ImportId != null)
        {
            notify = await _repos.ImportNotify.GetFirstAsync(n => n.Id == importOrder.ImportId && n.IsDelete == false);
        }

        await CreateBoxInfoAsync(importOrder.Id, stockCode, stockCodeEntity.Id, quantity, material.Id, notify?.SupplierId, notify?.ManufacturerId);
        await UpdateTrayStatusAsync(stockCodeEntity, EmptyTrayConstants.TrayStatus.Used);
    }

    /// <summary>
    /// 解绑空托盘操作
    /// </summary>
    /// <param name="stockCode">托盘编码</param>
    /// <remarks>
    /// 解绑操作包含以下步骤：
    /// 1. 验证托盘的合法性
    /// 2. 检查托盘是否已绑定且未分配库位
    /// 3. 软删除箱码信息
    /// 4. 软删除对应的入库流水
    /// 5. 更新托盘状态为未使用
    /// </remarks>
    private async Task UnbindEmptyTrayAsync(string stockCode)
    {
        var stockCodeEntity = await ValidateTrayForUnbindingAsync(stockCode);
        var boxInfo = await ValidateBoxInfoForUnbindingAsync(stockCode);
        await SoftDeleteBoxInfoAsync(boxInfo);
        await SoftDeleteImportOrderAsync(boxInfo.ImportOrderId);
        await UpdateTrayStatusAsync(stockCodeEntity, EmptyTrayConstants.TrayStatus.Unused);
    }

    /// <summary>
    /// 验证托盘是否可以绑定
    /// </summary>
    /// <param name="stockCode">托盘编码</param>
    /// <returns>托盘实体信息</returns>
    private async Task<WmsSysStockCode> ValidateTrayForBindingAsync(string stockCode)
    {
        var stockCodeEntity = await _repos.StockCode.GetFirstAsync(a =>
            a.StockCode == stockCode &&
            a.Status == EmptyTrayConstants.TrayStatus.Unused &&
            !a.IsDelete) ?? throw Oops.Bah("此载具无效或已使用！");
        return stockCodeEntity;
    }

    /// <summary>
    /// 验证托盘是否可以解绑
    /// </summary>
    /// <param name="stockCode">托盘编码</param>
    /// <returns>托盘实体信息</returns>
    private async Task<WmsSysStockCode> ValidateTrayForUnbindingAsync(string stockCode)
    {
        var stockCodeEntity = await _repos.StockCode.GetFirstAsync(a =>
            a.StockCode == stockCode &&
            !a.IsDelete) ?? throw Oops.Bah("此载具无效！");
        return stockCodeEntity;
    }

    /// <summary>
    /// 检查托盘是否未绑定
    /// </summary>
    /// <param name="stockCode">托盘编码</param>
    private async Task CheckTrayNotBoundAsync(string stockCode)
    {
        var existingBox = await _repos.BoxInfo.GetFirstAsync(a =>
            !a.IsDelete &&
            a.StockCode == stockCode &&
            (a.Status == EmptyTrayConstants.BoxStatus.Pending || a.Status == EmptyTrayConstants.BoxStatus.InProgress));

        if (existingBox != null)
            throw Oops.Bah("此托盘已绑定！");
    }

    /// <summary>
    /// 验证箱码信息是否可以解绑
    /// </summary>
    /// <param name="stockCode">托盘编码</param>
    /// <returns>箱码信息</returns>
    private async Task<WmsStockSlotBoxInfo> ValidateBoxInfoForUnbindingAsync(string stockCode)
    {
        var boxInfo = await _repos.BoxInfo.GetFirstAsync(a =>
            !a.IsDelete &&
            a.StockCode == stockCode &&
            (a.Status == EmptyTrayConstants.BoxStatus.Pending || a.Status == EmptyTrayConstants.BoxStatus.InProgress)) ?? throw Oops.Bah("此托盘还未绑定！");
        if (boxInfo.Status == EmptyTrayConstants.BoxStatus.InProgress)
            throw Oops.Bah("此托盘已分配库位地址不可解绑！");
        return boxInfo;
    }

    /// <summary>
    /// 创建入库流水记录
    /// </summary>
    /// <param name="stockCodeEntity">托盘实体信息</param>
    /// <returns>创建的入库流水实体</returns>
    private async Task<WmsImportOrder> CreateImportOrderAsync(WmsSysStockCode stockCodeEntity)
    {
        var now = DateTime.Now;
        var importOrderNo = _commonMethod.GetImExNo(EmptyTrayConstants.SystemConstants.ImportOrderNoPrefix);
        var importOrder = new WmsImportOrder
        {
            ImportOrderNo = importOrderNo,
            StockCodeId = stockCodeEntity.Id,
            StockCode = stockCodeEntity.StockCode,
            ImportExecuteFlag = EmptyTrayConstants.ImportExecuteFlags.Pending,
            WareHouseId = stockCodeEntity.WarehouseId,
            CreateUserName = EmptyTrayConstants.SystemConstants.DefaultCreateUser,
            CreateTime = now,
            IsDelete = false,
            SubVehicleCode = stockCodeEntity.StockCode
        };
        return await _repos.ImportOrder.InsertReturnEntityAsync(importOrder);
    }

    /// <summary>
    /// 根据仓库ID获取空托盘物料
    /// </summary>
    /// <param name="warehouseId">仓库ID</param>
    /// <returns>物料实体信息</returns>
    private async Task<WmsBaseMaterial> GetEmptyTrayMaterialAsync(long? warehouseId)
    {
        var wareHouse = await _repos.Warehouse.GetFirstAsync(a => a.Id == warehouseId);
        // 使用 IsEmpty 字段查询空托盘物料
        var material = await _repos.Material.GetFirstAsync(m =>
            m.IsEmpty == true && !m.IsDelete &&
            (m.WarehouseId == warehouseId || m.WarehouseId == null)) ?? throw Oops.Bah("未维护对应载具！");
        return material;
    }

    /// <summary>
    /// 创建箱码信息记录
    /// </summary>
    /// <param name="importOrderId">入库流水ID</param>
    /// <param name="stockCode">托盘编码</param>
    /// <param name="stockCodeId">托盘ID</param>
    /// <param name="quantity">数量</param>
    /// <param name="materialId">物料ID</param>
    /// <param name="supplierId">供应商ID（可选，空托盘场景允许为null）</param>
    /// <param name="manufacturerId">制造商ID（可选，空托盘场景允许为null）</param>
    private async Task CreateBoxInfoAsync(long importOrderId, string stockCode, long stockCodeId, int quantity, long materialId, long? supplierId = null, long? manufacturerId = null)
    {
        var now = DateTime.Now;
        var boxInfo = new WmsStockSlotBoxInfo
        {
            ImportOrderId = importOrderId,
            StockCode = stockCode,
            StockCodeId = stockCodeId,
            Qty = quantity,
            Status = EmptyTrayConstants.BoxStatus.Pending,
            MaterialId = materialId,
            SupplierId = supplierId,
            ManufacturerId = manufacturerId,
            IsDelete = false,
            CreateTime = now
        };
        await _repos.BoxInfo.InsertAsync(boxInfo);
    }

    /// <summary>
    /// 更新托盘状态
    /// </summary>
    /// <param name="stockCodeEntity">托盘实体</param>
    /// <param name="status">新状态</param>
    private async Task UpdateTrayStatusAsync(WmsSysStockCode stockCodeEntity, int status)
    {
        var now = DateTime.Now;
        await _repos.StockCode.AsUpdateable()
            .SetColumns(a => a.Status == status)
            .SetColumns(a => a.UpdateTime == now)
            .Where(a => a.Id == stockCodeEntity.Id)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 软删除箱码信息（使用基类方法）
    /// </summary>
    /// <param name="boxInfo">箱码信息</param>
    private async Task SoftDeleteBoxInfoAsync(WmsStockSlotBoxInfo boxInfo)
    {
        await SoftDeleteEntityAsync(_repos.BoxInfo, boxInfo.Id);
    }

    /// <summary>
    /// 软删除入库流水（使用基类方法）
    /// </summary>
    /// <param name="importOrderId">入库流水ID</param>
    private async Task SoftDeleteImportOrderAsync(long? importOrderId)
    {
        if (!importOrderId.HasValue)
            return;

        await SoftDeleteEntityAsync(_repos.ImportOrder, importOrderId.Value);
    }
    #endregion
}
