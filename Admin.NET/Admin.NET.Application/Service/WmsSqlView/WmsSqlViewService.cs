// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;
using System.Linq;
using System.Threading.Tasks;
using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsPda.Dto;
using Admin.NET.Application.Service.WmsSqlView.Dto;
using Admin.NET.Core;
using Furion.FriendlyException;
using SqlSugar;

namespace Admin.NET.Application.Service.WmsSqlView;

/// <summary>
/// WMS SqlView服务类。
/// <para>封装原 JC35 数据库视图的 SqlSugar 实现和 JC20 的 SQL 函数实现。</para>
/// <para>包括：</para>
/// <list type="bullet">
/// <item>JC35 视图：View_WmsImportNotifyDetail、View_WmsImportOrder、View_WmsStockSlotBoxInfo、View_WmsManualDepalletizing、View_WmsExportBoxInfo</item>
/// <item>JC20 SQL 函数：F_GetImportOrderNo、F_GetImportTaskNew 等</item>
/// </list>
/// </summary>
public class WmsSqlViewService : ITransient
{
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly SqlSugarRepository<WmsImportOrder> _wmsImportOrderRep;
    private readonly SqlSugarRepository<WMSTaskNoPub> _wmsTaskNoPubRep;

    public WmsSqlViewService(
        ISqlSugarClient sqlSugarClient,
        SqlSugarRepository<WmsImportOrder> wmsImportOrderRep,
        SqlSugarRepository<WMSTaskNoPub> wmsTaskNoPubRep)
    {
        _sqlSugarClient = sqlSugarClient;
        _wmsImportOrderRep = wmsImportOrderRep;
        _wmsTaskNoPubRep = wmsTaskNoPubRep;
    }

    #region View_WmsImportNotifyDetail 视图实现

    /// <summary>
    /// 构造等价 View_WmsImportNotifyDetail 的查询。
    /// </summary>
    /// <remarks>
    /// 原视图 SQL 包含以下关联：
    /// <list type="bullet">
    /// <item>WmsImportNotifyDetail (主表 a)</item>
    /// <item>WmsImportNotify (b) - LEFT JOIN</item>
    /// <item>WmsBaseMaterial (c) - LEFT JOIN</item>
    /// <item>SysDictData + SysDictType (d) - LEFT JOIN (ImportExecuteFlag, DictType.Code='CDStatu')</item>
    /// <item>SysDictData + SysDictType (dd) - LEFT JOIN (ImportExecuteFlag, DictType.Code='CDStatu')</item>
    /// <item>SysDictData + SysDictType (ddd) - LEFT JOIN (MaterialStatus, DictType.Code='MaterialStatus')</item>
    /// <item>WmsBaseUnit (f) - LEFT JOIN</item>
    /// <item>WmsSysUser (g) - LEFT JOIN (CreateUser)</item>
    /// <item>WmsSysUser (gg) - LEFT JOIN (UpdateUser)</item>
    /// </list>
    /// </remarks>
    public ISugarQueryable<ViewImportNotifyDetail> QueryImportNotifyDetailView()
    {
        return _sqlSugarClient.Queryable<WmsImportNotifyDetail>()   //查询入库单据明细视图   
            .LeftJoin<WmsImportNotify>((detail, notify) => detail.ImportId == notify.Id)   //左连接
            .LeftJoin<WmsBaseMaterial>((detail, notify, material) => detail.MaterialId == material.Id)
            .LeftJoin<SysDictType>((detail, notify, material, dictType1) => dictType1.Code == "CDStatu")
            .LeftJoin<SysDictData>((detail, notify, material, dictType1, dict1) => dict1.DictTypeId == dictType1.Id && detail.ImportExecuteFlag == dict1.Value && dict1.Status == StatusEnum.Enable)
            .LeftJoin<SysDictType>((detail, notify, material, dictType1, dict1, dictType2) => dictType2.Code == "CDStatu")
            .LeftJoin<SysDictData>((detail, notify, material, dictType1, dict1, dictType2, dict2) => dict2.DictTypeId == dictType2.Id && notify.ImportExecuteFlag == dict2.Value && dict2.Status == StatusEnum.Enable)
            .LeftJoin<SysDictType>((detail, notify, material, dictType1, dict1, dictType2, dict2, dictType3) => dictType3.Code == "MaterialStatus")
            .LeftJoin<SysDictData>((detail, notify, material, dictType1, dict1, dictType2, dict2, dictType3, dict3) => dict3.DictTypeId == dictType3.Id && detail.MaterialStatus == dict3.Value && dict3.Status == StatusEnum.Enable)
            .LeftJoin<WmsBaseUnit>((detail, notify, material, dictType1, dict1, dictType2, dict2, dictType3, dict3, unit) => material.MaterialUnit == unit.Id)
            .LeftJoin<SysUser>((detail, notify, material, dictType1, dict1, dictType2, dict2, dictType3, dict3, unit, createUser) => detail.CreateUserId == createUser.Id)
            .LeftJoin<SysUser>((detail, notify, material, dictType1, dict1, dictType2, dict2, dictType3, dict3, unit, createUser, updateUser) => detail.UpdateUserId == updateUser.Id)
            .Select((detail, notify, material, dictType1, dict1, dictType2, dict2, dictType3, dict3, unit, createUser, updateUser) => new ViewImportNotifyDetail
            {
                Id = detail.Id, //设置ID
                ImportId = detail.ImportId, //设置入库单据ID
                WareHouseId = notify.WarehouseId, //设置仓库ID
                ImportBillCode = notify.ImportBillCode, //设置入库单据号
                OuterBillCode = notify.OuterBillCode, //设置外部单据号
                OuterDetailId = detail.OuterDetailId, //设置外部明细ID
                ImportListNo = detail.ImportListNo, //设置入库单据号
                ImportExecuteFlag = detail.ImportExecuteFlag,
                MaterialId = detail.MaterialId, //设置物料ID
                LotNo = detail.LotNo, //设置批次号
                MaterialCode = material.MaterialCode, //设置物料编码
                MaterialName = material.MaterialName, //设置物料名称
                MaterialStandard = material.MaterialStandard, //设置物料标准
                BoxQuantity = material.BoxQuantity, //设置箱量
                MaterialRemark = material.Remark, //设置物料备注
                MaterialStatus = detail.MaterialStatus, //设置物料状态
                MaterialStatusName = dict3.Label, //设置物料状态名称
                UnitName = unit.UnitName, //设置单位名称
                ImportProductionDate = detail.ImportProductionDate, //设置生产日期
                ImportLostDate = detail.ImportLostDate, //设置失效日期
                ImportQuantity = detail.ImportQuantity, //设置入库数量
                ImportFactQuantity = detail.ImportFactQuantity, //设置入库实际数量
                ImportCompleteQuantity = detail.ImportCompleteQuantity, //设置入库完成数量  
                TypeName = dict1.Label, //设置类型名称
                Remark = detail.Remark, //设置备注
                UploadQuantity = detail.UploadQuantity, //设置上传数量
                ZTypeName = dict2.Label, //设置类型名称
                Labeling = material.Labeling, //设置标签
                TaskStatus = detail.TaskStatus, //设置任务状态
                LabelJudgment = detail.LabelJudgment, //设置标签判断
                outQty = detail.outQty, //设置出库数量
                IsDel = detail.IsDelete, //设置是否删除
                CreateUser = detail.CreateUserId, //设置创建人ID
                CreateUserName = createUser.RealName, //设置创建人名称
                CreateTime = detail.CreateTime, //设置创建时间
                UpdateUser = detail.UpdateUserId, //设置更新人ID
                UpdateUserName = updateUser.RealName, //设置更新人名称
                UpdateTime = detail.UpdateTime, //设置更新时间
                LanewayCode = detail.LanewayCode, //设置巷道编码
                BillType = notify.BillType.HasValue ? notify.BillType.Value.ToString() : null, //设置单据类型
                CustomerId = notify.CustomerId, //设置客户ID    
                SupplierId = notify.SupplierId, //设置供应商ID
                ProduceId = notify.ProduceId, //设置生产ID
            });
    }

    #endregion

    #region View_WmsImportOrder 视图实现

    /// <summary>
    /// 构造等价 View_WmsImportOrder 的查询。
    /// </summary>
    /// <remarks>
    /// 原视图 SQL 包含以下关联：
    /// <list type="bullet">
    /// <item>WmsImportOrder (主表 a)</item>
    /// <item>WmsImportNotifyDetail (b) - LEFT JOIN</item>
    /// <item>WmsImportNotify (bb) - LEFT JOIN</item>
    /// <item>WmsBaseArea (c) - LEFT JOIN</item>
    /// <item>WmsBaseLaneway (d) - LEFT JOIN</item>
    /// <item>WmsImportTask (f) - LEFT JOIN</item>
    /// <item>SysDictData + SysDictType (g) - LEFT JOIN (ImportExecuteFlag, DictType.Code='CDStatu')</item>
    /// <item>WmsSysUser (h) - LEFT JOIN (CreateUser)</item>
    /// <item>WmsSysUser (hh) - LEFT JOIN (UpdateUser)</item>
    /// <item>WmsBaseMaterial (i) - LEFT JOIN</item>
    /// </list>
    /// </remarks>
    public ISugarQueryable<ViewImportOrder> QueryImportOrderView()
    {
        return _sqlSugarClient.Queryable<WmsImportOrder>()
            .LeftJoin<WmsImportNotifyDetail>((order, detail) => order.ImportDetailId == detail.Id)
            .LeftJoin<WmsImportNotify>((order, detail, notify) => order.ImportId == notify.Id)
            .LeftJoin<WmsBaseArea>((order, detail, notify, area) => order.ImportAreaId == area.Id)
            .LeftJoin<WmsBaseLaneway>((order, detail, notify, area, laneway) => order.ImportLanewayId == laneway.Id)
            .LeftJoin<WmsImportTask>((order, detail, notify, area, laneway, task) => order.ImportTaskId == task.Id)
            .LeftJoin<SysDictType>((order, detail, notify, area, laneway, task, dictType) => dictType.Code == "CDStatu")
            .LeftJoin<SysDictData>((order, detail, notify, area, laneway, task, dictType, dict) => dict.DictTypeId == dictType.Id && order.ImportExecuteFlag == dict.Value && dict.Status == StatusEnum.Enable)
            .LeftJoin<SysUser>((order, detail, notify, area, laneway, task, dictType, dict, createUser) => order.CreateUserId == createUser.Id)
            .LeftJoin<SysUser>((order, detail, notify, area, laneway, task, dictType, dict, createUser, updateUser) => order.UpdateUserId == updateUser.Id)
            .LeftJoin<WmsBaseMaterial>((order, detail, notify, area, laneway, task, dictType, dict, createUser, updateUser, material) => detail.MaterialId == material.Id)
            .Select((order, detail, notify, area, laneway, task, dictType, dict, createUser, updateUser, material) => new ViewImportOrder
            {
                Id = order.Id,
                ImportOrderNo = order.ImportOrderNo,
                ImportId = order.ImportId,
                MaterialId = detail.MaterialId,
                MaterialCode = material.MaterialCode,
                MaterialName = material.MaterialName,
                ImportAreaId = order.ImportAreaId,
                AreaName = area.AreaName,
                ImportLanewayId = order.ImportLanewayId,
                LanewayName = laneway.LanewayName,
                ImportSlotCode = order.ImportSlotCode,
                StockCodeId = order.StockCodeId,
                StockCode = order.StockCode,
                ImportQuantity = order.ImportQuantity,
                ImportWeight = order.ImportWeight,
                TaskId = order.ImportTaskId,
                TaskNo = task.TaskNo,
                ImportExecuteFlag = order.ImportExecuteFlag,
                TypeName = dict.Label,
                Remark = order.Remark,
                ImportClass = order.ImportClass,
                LotNo = order.LotNo,
                ImportProductionDate = order.ImportProductionDate,
                CompleteDate = order.CompleteDate,
                InspectionStatus = order.InspectionStatus,
                IsDel = order.IsDelete,
                CreateUser = order.CreateUserId,
                CreateUserName = createUser.RealName,
                CreateTime = order.CreateTime,
                UpdateUser = order.UpdateUserId,
                UpdateUserName = updateUser.RealName,
                UpdateTime = order.UpdateTime,
                SubVehicleCode = order.SubVehicleCode,
                InspectFlag = order.InspectFlag,
                WareHouseId = order.WareHouseId,
                IsTemporaryStorage = order.IsTemporaryStorage,
                ImportBillCode = notify.ImportBillCode
            });
    }

    #endregion

    #region View_WmsStockSlotBoxInfo 视图实现

    /// <summary>   
    /// 构造等价 View_WmsStockSlotBoxInfo 的查询。
    /// </summary>
    /// <remarks>
    /// 原视图 SQL 包含以下关联：
    /// <list type="bullet">
    /// <item>WmsStockSlotBoxInfo (主表)</item>
    /// <item>WmsBaseMaterial - LEFT JOIN (MaterialId)</item>
    /// <item>WmsImportNotifyDetail - LEFT JOIN (ImportDetailId)</item>
    /// <item>WmsSysStockCode - LEFT JOIN (StockCodeId)</item>
    /// <item>WmsImportNotify - LEFT JOIN (ImportId)</item>
    /// <item>WmsImportOrder - LEFT JOIN (ImportOrderId)</item>
    /// </list>
    /// </remarks>
    public ISugarQueryable<ViewStockSlotBoxInfoView> QueryStockSlotBoxInfoView()
    {
        return _sqlSugarClient.Queryable<WmsStockSlotBoxInfo>()   //查询库存组托信息视图
            .LeftJoin<WmsBaseMaterial>((box, material) => box.MaterialId == material.Id)   //左连接物料
            .LeftJoin<WmsImportNotifyDetail>((box, material, detail) => box.ImportDetailId == detail.Id)   //左连接入库明细
            .LeftJoin<WmsSysStockCode>((box, material, detail, stockCode) => box.StockCodeId == stockCode.Id)   //左连接库存编码
            .LeftJoin<WmsImportNotify>((box, material, detail, stockCode, notify) => box.ImportId == notify.Id)   //左连接入库单据
            .LeftJoin<WmsImportOrder>((box, material, detail, stockCode, notify, order) => box.ImportOrderId == order.Id)   //左连接入库流水
                                                                                                                            //.LeftJoin<WmsInspectNotity>((box, material, detail, stockCode, notify, order, inspect) => box.ImportId == inspect.Id)   //左连接质检通知（暂未使用）
            .Select((box, material, detail, stockCode, notify, order) => new ViewStockSlotBoxInfoView
            {
                Id = box.Id,    //主键
                BoxCode = box.BoxCode,
                StockCodeId = box.StockCodeId,    //库存编码ID
                StockCode = box.StockCode,    //库存编码
                Qty = box.Qty,    //数量
                ImportOrderId = box.ImportOrderId,    //入库流水ID
                ImportOrderNo = order.ImportOrderNo,    //入库流水号
                Status = box.Status,    //状态
                BoxLevel = box.BoxLevel,    //箱层级
                LotNo = box.LotNo,    //批次号
                IsDelete = box.IsDelete,    //是否删除
                CreateTime = box.CreateTime,    //创建时间
                BulkTank = box.BulkTank,    //是否零箱标识
                ProductionDate = box.ProductionDate,    //生产日期
                ValidateDay = box.ValidateDay,    //有效期
                MaterialId = box.MaterialId,    //物料ID
                SupplierId = box.SupplierId,    //供应商ID
                ManufacturerId = box.ManufacturerId,    //制造商ID
                MaterialCode = material.MaterialCode,    //物料编码
                MaterialName = material.MaterialName,    //物料名称
                ImportDetailId = box.ImportDetailId,    //入库明细ID
                ImportExecuteFlag = detail.ImportExecuteFlag,    //入库执行标识
                ImportId = box.ImportId,    //入库单据ID
                ImportQuantity = detail.ImportQuantity,
                ImportFactQuantity = detail.ImportFactQuantity,    //入库实际数量
                ImportBillCode = notify.ImportBillCode,    //入库单据号
                InspectBillCode = null,    //质检单据号
                ExtractBillCode = string.Empty,    //提取单据号
                IsSamplingBox = box.IsSamplingBox,    //是否采样箱
                SamplingDate = box.SamplingDate,    //采样日期
                StaffCode = box.StaffCode,    //员工编码
                StaffName = box.StaffName,    //员工名称
                Weight = box.Weight,    //重量
                ReasonsForExcl = box.ReasonsForExcl,    //剔除原因
                ExtractStatus = box.ExtractStatus,    //提取状态
                InspectionStatus = box.InspectionStatus,    //质检状态
                PickingSlurry = box.PickingSlurry    //挑浆状态
            });
    }

    #endregion

    #region View_WmsManualDepalletizing 视图实现

    /// <summary>
    /// 构造等价 View_WmsManualDepalletizing 的查询。
    /// </summary>
    /// <remarks>
    /// 原视图 SQL 包含以下关联：
    /// <list type="bullet">
    /// <item>WmsExportOrder (主表)</item>
    /// <item>WmsStockTray - LEFT JOIN (TrayCode, LotNo, InspectionStatus, MaterialId)</item>
    /// <item>WmsStockInfo - LEFT JOIN (TrayId)</item>
    /// <item>WmsBaseMaterial - LEFT JOIN (MaterialId)</item>
    /// <item>WmsManualDepalletizing - LEFT JOIN (TrayCode, ExportOrderNo, State, IsDelete)</item>
    /// </list>
    /// </remarks>
    public ISugarQueryable<ViewManualDepalletizing> QueryManualDepalletizingView()
    {
        return _sqlSugarClient.Queryable<WmsExportOrder>()   //查询出库订单
            .InnerJoin<WmsStockTray>((order, tray) =>   //内连接库存托盘
                tray.StockCode == order.ExportStockCode &&   //托盘编码等于出库流水编码
                tray.LotNo == order.ExportLotNo &&   //托盘批次号等于出库流水批次号
                tray.InspectionStatus == order.InspectionStatus &&   //托盘检验状态等于出库流水检验状态
                tray.MaterialId == SqlFunc.ToString(order.ExportMaterialId))   //托盘物料ID等于出库流水物料ID
            .LeftJoin<WmsStockInfo>((order, tray, info) => info.TrayId == SqlFunc.ToString(tray.Id))   //左连接库存箱码信息
            .LeftJoin<WmsBaseMaterial>((order, tray, info, material) => material.Id == order.ExportMaterialId)   //左连接物料
            .Where((order, tray, info, material) => order.ExportExecuteFlag == 1 && order.IsDelete == false)   //过滤条件
            .Select((order, tray, info, material) => new ViewManualDepalletizing
            {
                ExportMaterialName = order.ExportMaterialName,//设置物料名称
                BoxCode = info.BoxCode,//设置箱码
                LotNo = info.LotNo,//设置批次号
                Qty = info.Qty,//设置数量
                LockQuantity = info.LockQuantity,//设置锁定数量
                OutQty = info.outQty,//设置出库数量
                ToutQty = tray.outQty,//设置出库数量
                StockSlotCode = tray.StockSlotCode,//设置储位编码
                StockCode = tray.StockCode,//设置托盘编码
                ExportQuantity = order.ExportQuantity,//设置出库数量
                StockQuantity = order.StockQuantity,//设置库存数量
                ExportOrderNo = order.ExportOrderNo,//设置出库流水号
                GroupQuantity = SqlFunc.IsNull(//设置组数量
                    SqlFunc.Subqueryable<WmsManualDepalletizing>()//查询拆跺明细
                        .Where(man => man.TrayCode == tray.StockCode && man.ExportOrderNo == order.ExportOrderNo && man.State == 0 && man.IsDelete == false)
                        .Sum(man => SqlFunc.ToDecimal(man.ScanQty)),//设置扫描数量
                    0m),//设置默认数量
                QRCode = info.QRCode,//设置二维码
                OrderInspectionStatus = order.InspectionStatus,//设置检验状态
                TrayInspectionStatus = tray.InspectionStatus,//设置托盘检验状态
                ExportExecuteFlag = order.ExportExecuteFlag,//设置执行标志
                TrayId = info.TrayId,//设置托盘ID
                Id = SqlFunc.ToString(tray.Id),//设置ID
                MaterialCode = material.MaterialCode//设置物料编码
            });
    }

    #endregion

    #region View_WmsExportBoxInfo 视图实现

    /// <summary>
    /// 构造等价 View_WmsExportBoxInfo 的查询。
    /// </summary>
    /// <remarks>
    /// 原视图 SQL 包含以下关联：
    /// <list type="bullet">
    /// <item>WmsExportBoxInfo (主表)</item>
    /// <item>WmsBaseMaterial - LEFT JOIN (MaterialId)</item>
    /// <item>WmsBaseUnit - LEFT JOIN (MaterialUnit)</item>
    /// </list>
    /// </remarks>
    public ISugarQueryable<ViewExportBoxInfo> QueryExportBoxInfoView()
    {
        return _sqlSugarClient.Queryable<WmsExportBoxInfo>()   //查询出库箱码信息视图
            .LeftJoin<WmsBaseMaterial>((box, material) => box.MaterialId == SqlFunc.ToString(material.Id))   //左连接物料
            .LeftJoin<WmsBaseUnit>((box, material, unit) => material.MaterialUnit == unit.Id)   //左连接单位
            .Select((box, material, unit) => new ViewExportBoxInfo
            {
                Id = box.Id,   //设置ID
                BoxCode = box.BoxCode,   //设置箱码
                Qty = box.Qty,   //设置数量
                StockCode = box.StockCodeCode,   //设置托盘编码
                ExportOrderNo = box.ExportOrderNo,   //设置出库流水号
                IsOut = box.IsOut,   //设置是否出库
                MaterialId = box.MaterialId,   //设置物料ID
                LotNo = box.LotNo,   //设置批次号
                AddDate = box.AddDate,   //设置添加时间
                Status = box.Status,   //设置状态
                ProductionDate = box.ProductionDate,   //设置生产日期
                ExportLoseDate = box.ExportLoseDate,   //设置失效日期
                IsDel = box.IsDelete,   //设置是否删除
                PickNum = box.PickNum,   //设置待拣数量
                IsMustExport = box.IsMustExport,   //设置是否必出箱
                PickEdNum = box.PickEdNum,   //设置已拣数量
                MaterialName = material.MaterialName,   //设置物料名称
                MaterialCode = material.MaterialCode,   //设置物料编码
                MaterialStandard = material.MaterialStandard,   //设置物料规格
                UnitName = unit.UnitName   //设置单位名称
            });
    }

    #endregion

    #region JC20 SQL 函数实现

    /// <summary>
    /// 生成入库流水号
    /// </summary>
    /// <remarks>
    /// 参考JC20的F_GetImportOrderNo函数实现
    /// 格式：RK + yyyyMMdd + 6位序列号
    /// 示例：RK20241106000001
    /// </remarks>
    public async Task<string> GenerateImportOrderNo()
    {
        try
        {
            const string prefix = "RK";    //前缀
            var dateStr = DateTime.Now.ToString("yyyyMMdd");    //日期
            var latestOrderNoList = await _wmsImportOrderRep.AsQueryable()    //查询入库流水信息
                .Where(o => o.ImportOrderNo != null    //入库流水号不为空
                    && o.ImportOrderNo.Length >= 16    //入库流水号长度大于等于16
                    && o.ImportOrderNo.StartsWith(prefix + dateStr))    //入库流水号以前缀+日期开头
                .OrderByDescending(o => o.ImportOrderNo)    //按入库流水号降序排序
                .Select(o => o.ImportOrderNo)    //选择入库流水号
                .Take(1)    //取第一条
                .ToListAsync();    //转换为列表

            if (latestOrderNoList.Count > 0)    //如果入库流水号列表不为空，则获取入库流水号
            {
                var maxOrderNo = latestOrderNoList[0];    //获取入库流水号
                var numericPart = maxOrderNo.Substring(maxOrderNo.Length - 6);    //获取入库流水号中的数字部分
                var nextNumber = int.Parse(numericPart) + 1;    //获取下一个数字
                var nextNumberStr = nextNumber.ToString().PadLeft(6, '0');    //获取下一个数字的字符串

                return $"{prefix}{dateStr}{nextNumberStr}";    //返回入库流水号
            }
            return $"{prefix}{dateStr}000001";    //返回入库流水号
        }
        catch (Exception ex)
        {
            throw Oops.Bah($"生成入库流水号失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 生成任务号
    /// </summary>
    /// <remarks>
    /// 参考JC20的F_GetImportTaskNew函数实现
    /// 格式：RK + yyyyMMdd + 5位序列号
    /// 示例：RK2024110600001
    /// 注意：使用事务和行锁确保并发安全
    /// </remarks>
    public async Task<string> GenerateTaskNo()
    {
        try
        {
            string billCode = string.Empty;
            // 1. 格式化当前日期为yyyyMMdd（如20241106）
            var dateStr = DateTime.Now.ToString("yyyyMMdd");

            await _sqlSugarClient.AsTenant().BeginTranAsync();
            try
            {
                // 2. 查询WMSTaskNoPub表，获取当天的最大任务号
                // SQL逻辑：SUBSTRING(TaskNo,3,8) = CONVERT(varchar(100), GETDATE(), 112)
                // 即：从第3位开始取8位（日期部分）= 今天日期
                // 使用行锁防止并发冲突
                var taskNoPub = await _sqlSugarClient.Queryable<WMSTaskNoPub>()
                    .Where(t => t.TypeName == "入库任务号")
                    .With(SqlWith.UpdLock)
                    .FirstAsync();

                if (taskNoPub?.TaskNo != null &&
                    taskNoPub.TaskNo.Length >= 10 &&
                    taskNoPub.TaskNo.Substring(2, 8) == dateStr)  // SUBSTRING(TaskNo,3,8)在C#中是Substring(2,8)
                {
                    // 3. 如果找到当天的任务号，取最大值
                    // 4. 提取后5位序列号（RIGHT(@MaxBillCode,5)）
                    var maxNoStr = taskNoPub.TaskNo.Substring(taskNoPub.TaskNo.Length - 5);
                    // 5. 转换为数字+1
                    var noInt = int.Parse(maxNoStr) + 1;
                    // 6. 左补0到5位（REPLICATE('0',5-LEN(@NoInt))）
                    var noVarchar = noInt.ToString().PadLeft(5, '0');
                    // 7. 拼接：RK + 日期 + 序列号
                    billCode = $"RK{dateStr}{noVarchar}";
                }
                else
                {
                    // 8. 如果当天没有任务号，从00001开始
                    billCode = $"RK{dateStr}00001";
                }

                // 9. 更新WMSTaskNoPub表（保持与JC20一致）
                await _sqlSugarClient.Updateable<WMSTaskNoPub>()
                    .SetColumns(o => o.TaskNo == billCode)
                    .Where(o => o.TypeName == "入库任务号")
                    .ExecuteCommandAsync();

                await _sqlSugarClient.AsTenant().CommitTranAsync();
            }
            catch
            {
                await _sqlSugarClient.AsTenant().RollbackTranAsync();
                throw;
            }

            return billCode;
        }
        catch (Exception ex)
        {
            throw Oops.Bah($"生成入库任务号失败：{ex.Message}");
        }
    }

    #endregion
}
