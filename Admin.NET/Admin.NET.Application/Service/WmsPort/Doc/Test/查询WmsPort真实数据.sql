-- =============================================
-- 查询WmsPort接口测试所需的数据库真实数据
-- 请执行此脚本并将结果提供给我
-- =============================================

USE [JC44_WMS];
GO

PRINT '==================== 1. 入库单据明细（用于CreateOrder） ====================';
-- 查询状态为待执行的入库单据明细
SELECT TOP 5
    Id,
    ImportId,
    MaterialId,
    ImportExecuteFlag,
    ReceivingDock,
    NetWeight,
    ImportQuantity
FROM WmsImportNotifyDetail
WHERE IsDelete = 0
    AND ImportExecuteFlag = '01' -- 待执行
ORDER BY CreateTime DESC;

PRINT '';
PRINT '==================== 2. 托盘信息（用于CreateImportOrder） ====================';
-- 查询空闲状态的托盘（没有库存记录，或状态为空闲）
SELECT TOP 10
    ssc.Id,
    ssc.StockCode,
    ssc.Status,
    ssc.WarehouseId
FROM WmsSysStockCode ssc
WHERE ssc.IsDelete = 0
    AND ssc.Status = 0 -- 空闲状态
    AND NOT EXISTS (
        SELECT 1 FROM WmsStockTray st
        WHERE st.StockCode = ssc.StockCode
        AND st.IsDelete = 0
        AND st.StockStatusFlag = 1 -- 排除已占用的
    )
ORDER BY ssc.CreateTime DESC;

PRINT '';
PRINT '==================== 3. 已有库存的托盘（用于出库测试） ====================';
-- 查询已有库存的托盘
SELECT TOP 5
    st.Id,
    st.StockCode,
    st.StockSlotCode,
    st.MaterialId,
    st.StockQuantity,
    st.LanewayId,
    st.WarehouseId,
    st.StockStatusFlag
FROM WmsStockTray st
WHERE st.IsDelete = 0
    AND st.StockQuantity > 0
ORDER BY st.CreateTime DESC;

PRINT '';
PRINT '==================== 4. 物料信息 ====================';
-- 查询启用状态的物料
SELECT TOP 5
    Id,
    MaterialCode,
    MaterialName,
    MaterialType,
    IsEmpty,
    Status
FROM WmsBaseMaterial
WHERE IsDelete = 0
    AND Status = 1 -- 启用
ORDER BY CreateTime DESC;

PRINT '';
PRINT '==================== 5. 空托盘物料 ====================';
-- 查询空托盘物料
SELECT TOP 3
    Id,
    MaterialCode,
    MaterialName
FROM WmsBaseMaterial
WHERE IsDelete = 0
    AND IsEmpty = 1 -- 是空托盘
    AND Status = 1
ORDER BY CreateTime DESC;

PRINT '';
PRINT '==================== 6. 仓库信息 ====================';
-- 查询仓库
SELECT TOP 5
    Id,
    WarehouseCode,
    WarehouseName,
    WarehouseType
FROM WmsBaseWareHouse
WHERE IsDelete = 0
ORDER BY CreateTime DESC;

PRINT '';
PRINT '==================== 7. 巷道信息 ====================';
-- 查询巷道
SELECT TOP 5
    Id,
    LanewayCode,
    LanewayName,
    WarehouseId,
    LanewayStatus
FROM WmsBaseLaneway
WHERE IsDelete = 0
ORDER BY CreateTime DESC;

PRINT '';
PRINT '==================== 8. 库位信息（用于CreateImportOrder2） ====================';
-- 查询空闲库位
SELECT TOP 10
    Id,
    SlotCode,
    SlotLanewayId,
    SlotStatus,
    SlotInout,
    Make
FROM WmsBaseSlot
WHERE IsDelete = 0
    AND SlotStatus = 0 -- 空闲
    AND Make = '01' -- 正常货位（存储库位）
ORDER BY CreateTime DESC;

PRINT '';
PRINT '==================== 9. 出入口库位 ====================';
-- 查询出入口
SELECT TOP 5
    Id,
    SlotCode,
    SlotLanewayId,
    Make
FROM WmsBaseSlot
WHERE IsDelete = 0
    AND Make = '02' -- 中转库位（出入口）
ORDER BY CreateTime DESC;

PRINT '';
PRINT '==================== 10. 入库任务（用于TaskFeedback） ====================';
-- 查询入库任务
SELECT TOP 5
    Id,
    TaskNo,
    StartLocation,
    EndLocation,
    StockCode,
    Status,
    IsSuccess
FROM WmsImportTask
WHERE IsDelete = 0
    AND Status = 1 -- 已下发
ORDER BY CreateTime DESC;

PRINT '';
PRINT '==================== 11. 入库单据 ====================';
-- 查询入库单据
SELECT TOP 5
    Id,
    ImportBillCode,
    ImportExecuteFlag,
    WarehouseId,
    BillType
FROM WmsImportNotify
WHERE IsDelete = 0
ORDER BY CreateTime DESC;

PRINT '';
PRINT '==================== 12. 箱码库存信息 ====================';
-- 查询箱码库存
SELECT TOP 10
    Id,
    BoxCode,
    MaterialId,
    Qty,
    LotNo,
    TrayId,
    ProductionDate,
    ValidateDay
FROM WmsStockInfo
WHERE IsDelete = 0
ORDER BY CreateTime DESC;

PRINT '';
PRINT '==================== 13. 出入口编码（ReceivingDock） ====================';
-- 查询已使用的ReceivingDock值
SELECT DISTINCT TOP 5
    ReceivingDock
FROM WmsImportNotifyDetail
WHERE IsDelete = 0
    AND ReceivingDock IS NOT NULL
    AND ReceivingDock != ''
ORDER BY ReceivingDock;

PRINT '';
PRINT '==================== 完成 ====================';
PRINT '请将以上查询结果提供给我，我会根据真实数据更新Postman测试集合';
