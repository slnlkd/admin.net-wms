// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护使用本项目应遵守相关法律法规和许可证的要求
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！
using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.BaseService;
using Admin.NET.Application.Service.WmsPda.Dto;
using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Core;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlSugar;
using Admin.NET.Application.Service.WmsSqlView;
using Admin.NET.Application.Service.WmsSqlView.Dto;
namespace Admin.NET.Application.Service.WmsPda.Process;
/// <summary>
/// PDA 出库业务处理服务
/// <para>承载 JC35 <c>PdaInterfaceController</c> 出库接口迁移，提供完整的PDA出库相关业务功能</para>
/// </summary>
/// <remarks>
/// 主要功能包括：
/// <list type="bullet">
/// <item><description>手动拆垛（有箱码）：绑定、查询、删除拆垛信息，人工拆垛出库确认</description></item>
/// <item><description>手动拆垛（无箱码）：绑定、查询、删除无箱码拆跺明细</description></item>
/// <item><description>空托出库：空托盘出库申请、出库口管理</description></item>
/// <item><description>库存管理：库存扣减、托盘释放、状态更新</description></item>
/// <item><description>任务管理：WCS任务下发、状态跟踪</description></item>
/// </list>
/// </remarks>
public class PdaExportBase : PdaBase, ITransient
{
    #region 字段定义
    protected readonly PdaBaseRepos _repos;                                     //仓储聚合
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，注入所有必需的依赖
    /// </summary>
    public PdaExportBase(
        ISqlSugarClient sqlSugarClient,
        PdaBaseRepos repos) : base(sqlSugarClient)
    {
        _repos = repos;
    }
    #endregion

    /// <summary>
    /// 根据出库流水加载对应库存托盘
    /// </summary>
    /// <param name="exportOrder">出库流水信息，包含托盘编码、物料ID、批号等</param>
    /// <param name="expectWarehouseMatch">是否强制仓库一致，默认为false</param>
    /// <returns>匹配的库存托盘信息，未找到时返回null</returns>
    /// <remarks>
    /// 查询条件：
    /// <list type="bullet">
    /// <item><description>托盘编码必须匹配出库流水的ExportStockCode</description></item>
    /// <item><description>批号必须匹配出库流水的ExportLotNo</description></item>
    /// <item><description>如果提供了物料ID，则必须匹配</description></item>
    /// <item><description>如果提供了检验状态，则必须匹配</description></item>
    /// <item><description>可选：仓库ID必须匹配（expectWarehouseMatch=true时）</description></item>
    /// </list>
    /// </remarks>
    protected async Task<WmsStockTray> LoadExportTrayAsync(WmsExportOrder exportOrder, bool expectWarehouseMatch = false)
    {
        // 出库流水为空，无法查询
        if (exportOrder == null) return null;
        // 构建基础查询条件：托盘编码和批次号必须匹配
        var query = _repos.StockTray.AsQueryable()
            .Where(x => x.StockCode == exportOrder.ExportStockCode && x.LotNo == exportOrder.ExportLotNo);
        // 如果出库流水指定了物料ID，添加物料匹配条件
        if (exportOrder.ExportMaterialId.HasValue)
        {
            query = query.Where(x => x.MaterialId == exportOrder.ExportMaterialId.Value.ToString());
        }
        // 如果出库流水指定了检验状态，添加检验状态匹配条件
        if (exportOrder.InspectionStatus.HasValue)
        {
            query = query.Where(x => x.InspectionStatus == exportOrder.InspectionStatus.Value);
        }
        // 如果要求仓库匹配且出库流水指定了仓库ID，添加仓库匹配条件
        if (expectWarehouseMatch && exportOrder.ExportWarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == exportOrder.ExportWarehouseId.Value.ToString());
        }
        // 执行查询并返回第一个匹配的托盘
        return await query.FirstAsync();
    }
    /// <summary>
    /// 更新出库通知明细及主单的执行状态
    /// </summary>
    /// <param name="exportOrder">出库订单</param>
    /// <param name="totalScanQty">总扫描数量</param>
    /// <param name="now">当前时间</param>
    protected async Task UpdateExportNotifyAsync(WmsExportOrder exportOrder, decimal totalScanQty, DateTime now)
    {
        // 出库单据编码为空，无法更新出库通知
        if (string.IsNullOrWhiteSpace(exportOrder.ExportBillCode))
        {
            return;
        }
        // 物料ID为空，无法匹配出库通知明细
        if (!exportOrder.ExportMaterialId.HasValue)
        {
            return;
        }
        // 根据是否有检验状态决定查询条件（检验状态是精确匹配的维度之一）
        WmsExportNotifyDetail notifyDetail = null;
        // 【情况1：有检验状态】查询时需要匹配单据号、物料、批次、检验状态
        if (exportOrder.InspectionStatus.HasValue && !string.IsNullOrWhiteSpace(exportOrder.InspectionStatus.Value.ToString()))
        {
            var inspectionStatusStr = exportOrder.InspectionStatus.Value.ToString();
            notifyDetail = await _repos.ExportNotifyDetail.GetFirstAsync(x =>
                x.ExportBillCode == exportOrder.ExportBillCode &&
                x.MaterialId == exportOrder.ExportMaterialId &&
                (!string.IsNullOrWhiteSpace(exportOrder.ExportLotNo) ? x.LotNo == exportOrder.ExportLotNo : true) && // 批次号存在时才匹配
                x.InspectionStatus == inspectionStatusStr &&
                (x.IsDelete == false || x.IsDelete == null));
        }
        // 【情况2：无检验状态】查询时只需匹配单据号、物料、批次
        else
        {
            notifyDetail = await _repos.ExportNotifyDetail.GetFirstAsync(x =>
                x.ExportBillCode == exportOrder.ExportBillCode &&
                x.MaterialId == exportOrder.ExportMaterialId &&
                (!string.IsNullOrWhiteSpace(exportOrder.ExportLotNo) ? x.LotNo == exportOrder.ExportLotNo : true) && // 批次号存在时才匹配
                (x.IsDelete == false || x.IsDelete == null));
        }
        // 如果找到了匹配的出库通知明细，更新完成数量和状态
        if (notifyDetail != null)
        {
            // 累加完成数量（本次扫描数量）
            notifyDetail.CompleteQuantity = (notifyDetail.CompleteQuantity ?? 0) + totalScanQty;
            notifyDetail.UpdateTime = now;
            // 检查是否还有其他正在执行的出库流水（ExportExecuteFlag == 1 表示正在执行）
            // 如果有，说明该明细还未完全出库，不能标记为完成
            var hasOtherExecutingOrders = await _repos.ExportOrder.AsQueryable()
                .Where(o => o.ExportBillCode == exportOrder.ExportBillCode &&
                           o.ExportMaterialId == exportOrder.ExportMaterialId &&
                           (!string.IsNullOrWhiteSpace(exportOrder.ExportLotNo) ? o.ExportLotNo == exportOrder.ExportLotNo : true) && // 批次号匹配
                           o.IsDelete == false &&
                           o.ExportOrderNo != exportOrder.ExportOrderNo && // 排除当前流水
                           o.ExportExecuteFlag == 1) // 正在执行状态
                .AnyAsync();
            // 如果没有其他正在执行的流水，标记明细为完成（ExportDetailFlag=3）
            if (!hasOtherExecutingOrders)
            {
                notifyDetail.ExportDetailFlag = 3; // 状态3表示明细已完成
            }
            // 更新出库通知明细的完成数量、执行标志和更新时间
            await _repos.ExportNotifyDetail.AsUpdateable(notifyDetail)
                .UpdateColumns(x => new { x.CompleteQuantity, x.ExportDetailFlag, x.UpdateTime })
                .ExecuteCommandAsync();
        }
        // 检查该出库单据下是否还有未完成的明细（ExportDetailFlag < 3 表示未完成）
        var hasPendingDetail = await _repos.ExportNotifyDetail.AsQueryable()
            .Where(x => x.ExportBillCode == exportOrder.ExportBillCode &&
                       (x.IsDelete == false || x.IsDelete == null) &&
                       (x.ExportDetailFlag == null || x.ExportDetailFlag < 3)) // null或<3都视为未完成
            .AnyAsync();
        // 如果所有明细都已完成，更新出库通知主单状态为完成
        if (!hasPendingDetail)
        {
            var notify = await _repos.ExportNotify.GetFirstAsync(x => x.ExportBillCode == exportOrder.ExportBillCode && x.IsDelete == false);
            if (notify != null)
            {
                notify.ExportExecuteFlag = 3; // 状态3表示整单已完成
                notify.UpdateTime = now;
                // 更新出库通知主单的执行标志和更新时间
                await _repos.ExportNotify.AsUpdateable(notify)
                    .UpdateColumns(x => new { x.ExportExecuteFlag, x.UpdateTime })
                    .ExecuteCommandAsync();
            }
        }
    }
}
