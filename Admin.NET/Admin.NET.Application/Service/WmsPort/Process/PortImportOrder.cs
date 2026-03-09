// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证(版本 2.0)进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Core;
using Flurl;
using Flurl.Http;
using Furion.DatabaseAccessor;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlSugar;
namespace Admin.NET.Application.Service.WmsPort.Process;
/// <summary>
/// 入库单据下发业务类 📋
/// 负责将WMS系统的入库单据下发给WCS（仓库控制系统）执行
/// </summary>
/// <remarks>
/// 业务职责：
/// 1. 接收入库单据明细，构建WCS订单数据
/// 2. 调用WCS接口下发入库任务
/// 3. 处理WCS响应，更新任务状态
/// 4. 记录完整的操作日志
///
/// 系统对接：
/// - WMS → WCS：下发入库单据
/// - WCS → WMS：反馈执行结果
///
/// 注意事项：
/// - 当前处于开发测试阶段，使用模拟响应
/// - 生产环境需要启用实际的WCS接口调用
/// </remarks>
public class PortImportOrder : PortBase, ITransient
{
    #region 字段定义
    // 仓储聚合类
    private readonly PortBaseRepos _repos;
    // 业务服务
    private readonly WmsImportNotifyDetailService _importNotifyDetailService;
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖仓储
    /// </summary>
    /// <param name="sqlSugarClient">数据库客户端</param>
    /// <param name="loggerFactory">日志工厂</param>
    /// <param name="repos">仓储聚合类</param>
    /// <param name="importNotifyDetailService">入库单据明细服务</param>
    public PortImportOrder(
        ISqlSugarClient sqlSugarClient,
        ILoggerFactory loggerFactory,
        PortBaseRepos repos,
        WmsImportNotifyDetailService importNotifyDetailService)
        : base(sqlSugarClient, loggerFactory.CreateLogger(CommonConst.PortImportOrder), "入库流水")
    {
        _repos = repos;
        _importNotifyDetailService = importNotifyDetailService;
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 处理入库单据下发 🎯 【核心入口方法】
    /// 将WMS的入库单据下发给WCS系统执行
    /// </summary>
    /// <param name="input">输入参数（包含入库单据明细ID、入库口、皮重等信息）</param>
    /// <returns>下发结果（成功/失败、消息、WCS订单数据）</returns>
    /// <remarks>
    /// 业务流程：
    /// 1. 参数验证：检查入库口、皮重等必填参数
    /// 2. 获取WCS订单数据：根据入库明细构建WCS订单
    /// 3. 调用WCS接口：通过HTTP POST发送订单数据
    /// 4. 处理响应：解析WCS响应，更新任务状态
    /// 5. 记录日志：记录完整的操作过程
    ///
    /// 开发测试模式：
    /// - 当前使用模拟响应（response = "\"1\""）
    /// - 生产环境需要取消注释实际的HTTP调用代码
    ///
    /// 异常处理：
    /// - HTTP异常：捕获Flurl异常，记录状态码和响应内容
    /// - 解析异常：捕获JSON解析异常，返回原始响应
    /// - 通用异常：捕获所有异常，返回友好错误消息
    /// </remarks>
    public async Task<CreateOrderOutput> ProcessCreateOrder(ImportNotifyDetail input)
    {
        try
        {
            // 步骤1：参数验证
            // 验证入库口、皮重等必填参数
            var resMsg = ValidateCreateOrderInput(input);
            if (!string.IsNullOrEmpty(resMsg))
            {
                await RecordOperationLogAsync(input.Id.ToString(), resMsg, input);
                return new CreateOrderOutput { Success = false, Message = resMsg, Data = null };
            }
            // 步骤2：获取WCS订单数据
            // 根据入库明细构建符合WCS接口规范的订单数据
            var res = await GetImportBillDistribute(input);
            if (res == null || res.Item1 == null)
            {
                resMsg = res?.Item2 ?? "获取入库单据信息失败";
                await RecordOperationLogAsync(input.Id.ToString(), resMsg, input);
                return new CreateOrderOutput { Success = false, Message = resMsg, Data = null };
            }
            // 步骤3：准备发送数据
            // 将订单数据序列化为JSON格式
            var jsonData = res.Item1.ToJson();
            await RecordOperationLogAsync(input.Id.ToString(), "入库单据下发给WCS发送数据：" + jsonData, input);
            // 步骤4：调用WCS接口（当前为开发测试模式）
            // 【开发测试阶段】：使用模拟响应
            string response = "\"1\"";  // 模拟成功响应（JSON格式的字符串"1"）
            // 【生产环境】：取消注释以下代码，启用实际WCS接口调用
            /*
            string response;
            try
            {
                // 使用Flurl库发送HTTP POST请求到WCS接口
                response = await GetWcsApiUrl(WcsApiUrlDto.PortCreateOrderUrl)
                    .WithHeader("Content-Type", "application/json")
                    .PostStringAsync(jsonData)
                    .ReceiveString();
            }
            catch (FlurlHttpException httpEx)
            {
                // Flurl HTTP异常处理（包含HTTP状态码、响应内容等详细信息）
                var statusCode = httpEx.StatusCode ?? 0;
                var errorResponse = await httpEx.GetResponseStringAsync();
                resMsg = $"入库单据下发失败 HTTP状态码：{statusCode}，反馈数据：{errorResponse}，反馈时间：{DateTime.Now}";
                await RecordOperationLogAsync(input.Id.ToString(), resMsg, input);
                return new CreateOrderOutput { Success = false, Message = resMsg, Data = res.Item1 };
            }
            catch (Exception httpEx)
            {
                // 其他HTTP请求异常（网络异常、超时等）
                resMsg = $"入库单据下发HTTP请求异常：{httpEx.Message}";
                await RecordOperationLogAsync(input.Id.ToString(), resMsg, input);
                return new CreateOrderOutput { Success = false, Message = resMsg, Data = res.Item1 };
            }
            */
            // 步骤5：处理WCS响应
            try
            {
                // 解析WCS响应（JSON格式的字符串，如"1"或"0"）
                var wcsModel = JsonConvert.DeserializeObject<string>(response);
                if (wcsModel == ImportOrderConstants.ResponseCode.Success)  // "1" = 成功
                {
                    // 更新入库单据明细状态
                    var entity = await _repos.ImportNotifyDetail.GetFirstAsync(a => a.Id == input.Id);
                    entity.ImportTaskStatus = ImportOrderConstants.TaskStatus.Issued;
                    await _repos.ImportNotifyDetail.AsUpdateable(entity).ExecuteCommandAsync();
                    resMsg = $"入库单据下发成功 反馈数据：{response}反馈时间{DateTime.Now}";
                    await RecordOperationLogAsync(input.Id.ToString(), resMsg, input);
                    return new CreateOrderOutput { Success = true, Message = resMsg, Data = res.Item1 };
                }
                // WCS返回失败（"0"或其他非"1"的值）
                resMsg = $"入库单据下发失败 反馈数据：{response}反馈时间{DateTime.Now}";
                return new CreateOrderOutput { Success = false, Message = resMsg, Data = res.Item1 };
            }
            catch (Exception parseEx)
            {
                // JSON解析异常（响应格式不符合预期）
                resMsg = $"入库单据下发响应解析异常：{parseEx.Message}，原始响应：{response}";
                return new CreateOrderOutput { Success = false, Message = resMsg, Data = res.Item1 };
            }
        }
        catch (Exception ex)
        {
            // 捕获所有未处理的异常
            await RecordExceptionLogAsync(input.Id.ToString(), "入库单据下发", ex, input);
            return new CreateOrderOutput { Success = false, Message = $"入库单据下发异常：{ex.Message}", Data = null };
        }
    }
    #endregion

    #region 私有方法 - 核心业务逻辑
    /// <summary>
    /// 入库单据下发获取WCS订单数据 📦 【数据转换方法】
    /// 根据入库单据明细构建符合WCS接口规范的订单数据
    /// </summary>
    /// <param name="input">输入参数（包含入库单据明细ID、入库口、皮重等）</param>
    /// <returns>WCS订单数据和错误消息的元组（成功时Item2为空，失败时Item1为null）</returns>
    /// <exception cref="Exception">查询数据异常时抛出</exception>
    /// <remarks>
    /// 数据转换流程：
    /// 1. 参数验证：检查输入参数的有效性
    /// 2. 查询入库明细：关联查询物料等信息
    /// 3. 更新入库数据：更新皮重、入库口等信息
    /// 4. 查询物料信息：获取箱规等物料数据
    /// 5. 构建订单主信息：设备号、单据号、数量等
    /// 6. 计算质检数量：根据箱数计算抽检数量
    /// 7. 查询标签信息：获取标签明细数据
    /// 8. 构建订单明细：将标签信息转换为订单明细项
    ///
    /// 质检数量计算规则（MaterialStatus != 1时）：
    /// - 箱数小于5：全检（质检数量 = 箱数）
    /// - 5 = 箱数  100：抽检5箱
    /// - 100 = 箱数 = 1000：抽检5%
    /// - 箱数 > 1000：抽检（箱数-1000）*1% + 50
    ///
    /// WCS订单数据结构：
    /// - 主信息：设备号、皮重、单据号、仓库、数量、箱数等
    /// - 明细信息：标签ID、物料编码、数量、批次、二维码等
    /// </remarks>
    public async Task<Tuple<WCSOrderDto, string>> GetImportBillDistribute(ImportNotifyDetail input)
    {
        try
        {
            // 步骤1：参数验证
            var errorMsg = ValidateGetImportBillInput(input);
            if (!string.IsNullOrEmpty(errorMsg))
                return await ReturnErrorAsync(input, errorMsg);
            // 步骤2：查询入库单据明细（关联查询物料等信息）
            var detail = await _importNotifyDetailService.DetailJoinMaterial(
                new QueryByIdWmsImportNotifyDetailInput { Id = input.Id });
            if (detail != null && detail.ImportExecuteFlag != ImportOrderConstants.ExecuteFlag.Invalid)
            {
                // 步骤3：更新入库单据明细信息（皮重、入库口）
                await _repos.ImportNotifyDetail.AsUpdateable()
                    .SetColumns(a => a.NetWeight == input.NetWeight)
                    .SetColumns(a => a.ReceivingDock == input.ReceivingDock)
                    .Where(a => a.Id == input.Id && a.ImportExecuteFlag != ImportOrderConstants.ExecuteFlag.Invalid)
                    .ExecuteCommandAsync();
                // 步骤4：查询物料信息并验证
                var good = await _repos.Material.GetFirstAsync(p => p.Id == detail.MaterialId);
                if (good == null)
                    return await ReturnErrorAsync(input, "操作失败，物资信息未找到！");
                
                if (good.BoxQuantity == ImportOrderConstants.BoxQuantity.Zero)
                    return await ReturnErrorAsync(input, "操作失败，物资数量为0，请维护！");

                // 步骤5：提前转换日期（避免重复转换）
                var productionDateStr = detail.ImportProductionDate.ToString();
                var validateDateStr = detail.ImportLostDate.ToString();

                // 步骤6：构建WCS订单主信息
                var wCSOrderDTO = new WCSOrderDto
                {
                    EquipmentNo = input.ReceivingDock,                          // 设备号（入库口）
                    TareQty = input.NetWeight,                                  // 皮重
                    BillCode = detail.ImportBillCode,                           // 入库单据号
                    HouseCode = detail.WarehouseId.ToString(),                  // 仓库编码
                    WholeBoxNum = Convert.ToInt32(detail.BoxQuantity),          // 整箱数
                    Qty = (decimal)detail.ImportQuantity,                       // 数量
                    BoxQty = Math.Ceiling(Convert.ToDouble(detail.ImportQuantity) /
                             Convert.ToDouble(detail.BoxQuantity)),             // 箱数（向上取整）
                    GoodCode = detail.MaterialCode,                             // 物料编码
                    GoodName = detail.MaterialName,                             // 物料名称
                    ProductionDate = productionDateStr,                         // 生产日期
                    ValidateDay = validateDateStr,                              // 失效日期
                    LotNo = detail.LotNo,                                       // 批次号
                    LabelingStatus = ImportOrderConstants.LabelingStatus.NoLabel, // 标签状态（不贴标）
                    VehicleType = detail.Vehicle                                // 车辆类型
                };
                // 步骤7：计算质检数量（抽检数量）
                // 质检规则：根据箱数和物料状态计算需要抽检的箱数
                wCSOrderDTO.QTBoxQty = detail.MaterialStatus == ImportOrderConstants.MaterialStatus.ExemptFromInspection ? 0 :  // 免检物料
                    wCSOrderDTO.BoxQty < 5 ? wCSOrderDTO.BoxQty :              // < 5箱：全检
                    wCSOrderDTO.BoxQty < 100 ? 5 :                             // 5~99箱：抽检5箱
                    wCSOrderDTO.BoxQty <= 1000 ? Math.Ceiling(wCSOrderDTO.BoxQty * 0.05) :  // 100~1000箱：抽检5%
                    Math.Ceiling((wCSOrderDTO.BoxQty - 1000) * 0.01 + 50);     // >1000箱：抽检(箱数-1000)*1% + 50
                // 步骤8：查询标签信息并构建订单明细列表
                var label = await _repos.ImportLabelPrint.AsQueryable()
                    .Where(a => a.ImportBillCode == detail.ImportBillCode)
                    .ToListAsync();

                wCSOrderDTO.WmsOrderItems = label.Select(item => new WmsOrderItem
                {
                    SerialCode = item.LabelID,
                    OderId = item.ImportBillCode,
                    MaterialCode = item.MaterialCode,
                    ProductionDate = productionDateStr,
                    ValidateDay = validateDateStr,
                    Qty = item.Quantity.ToString(),
                    LotNo = item.LotNo,
                    QRCode = item.LabelID,
                    RFIDCode = item.LabelID
                }).ToList();
                await RecordOperationLogAsync(input.Id.ToString(), "成功生成WCS订单数据", input);
                return new Tuple<WCSOrderDto, string>(wCSOrderDTO, "");
            }
            // 入库单据单不存在或已失效
            return await ReturnErrorAsync(input, "操作失败，入库单据单信息未找到！");
        }
        catch (Exception ex)
        {
            // 捕获查询异常，记录日志并抛出
            await RecordExceptionLogAsync(input.Id.ToString(), "查询入库单明细信息", ex, input);
            throw new Exception($"查询入库单明细信息异常：{ex.Message}");
        }
    }
    #endregion

    #region 私有方法 - 参数验证
    /// <summary>
    /// 验证入库单据下发输入参数 ✅ 【参数验证】
    /// 检查下发单据时的必填参数
    /// </summary>
    /// <param name="input">输入参数</param>
    /// <returns>验证失败返回错误消息，验证成功返回null</returns>
    /// <remarks>
    /// 验证规则（按优先级排序）：
    /// 1. 输入对象不能为空
    /// 2. 单据ID必须大于0
    /// 3. 入库口（ReceivingDock）不能为空
    /// 4. 皮重（NetWeight）不能为空
    ///
    /// 使用三元运算符链式验证，提高代码简洁性
    /// </remarks>
    public string ValidateCreateOrderInput(ImportNotifyDetail input)
    {
        return input == null ? "入库单据下发失败：输入参数为空！"
             : input.Id == 0 ? "入库单据下发失败：单据ID为空或无效！"
             : string.IsNullOrEmpty(input.ReceivingDock) ? "入库单据下发失败：入库口为空！"
             : string.IsNullOrEmpty(input.NetWeight) ? "入库单据下发失败：皮重数据无效！"
             : null;
    }
    /// <summary>
    /// 验证获取入库单据输入参数 ✅ 【参数验证】
    /// 检查获取单据数据时的必填参数
    /// </summary>
    /// <param name="input">输入参数</param>
    /// <returns>验证失败返回错误消息，验证成功返回null</returns>
    /// <remarks>
    /// 验证规则（按优先级排序）：
    /// 1. 输入对象不能为空
    /// 2. 单据ID必须大于0
    ///
    /// 此方法用于GetImportBillDistribute方法的参数验证
    /// 验证项少于ValidateCreateOrderInput，因为只需要查询数据
    /// </remarks>
    public string ValidateGetImportBillInput(ImportNotifyDetail input)
    {
        return input == null ? "获取入库单据失败：输入参数为空！"
             : input.Id == 0 ? "获取入库单据失败：单据ID为空或无效！"
             : null;
    }
    #endregion

    #region 私有方法 - 辅助方法
    /// <summary>
    /// 返回错误结果（辅助方法）【错误处理】
    /// 统一处理错误记录日志并返回失败结果
    /// </summary>
    /// <param name="input">输入参数</param>
    /// <param name="errorMessage">错误消息</param>
    /// <returns>包含错误信息的元组</returns>
    private async Task<Tuple<WCSOrderDto, string>> ReturnErrorAsync(ImportNotifyDetail input, string errorMessage)
    {
        await RecordOperationLogAsync(input.Id.ToString(), errorMessage, input);
        return new Tuple<WCSOrderDto, string>(null, errorMessage);
    }
    #endregion
}
