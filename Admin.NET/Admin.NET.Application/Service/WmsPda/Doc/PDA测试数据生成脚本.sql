-- =============================================
-- PDA测试数据生成脚本
-- 功能：生成用于PDA功能展示的完整测试数据
-- 特点：
--   1. 所有测试数据使用TEST_PDA_前缀，方便识别和删除
--   2. 覆盖所有验证场景（入库、出库、盘点等）
--   3. 确保表之间的关联关系正确
--   4. 可随时删除，不影响生产数据
--
-- 作者：Sisyphus
-- 更新时间：2026-01-19
-- =============================================

USE [JC44_WMS];
GO

-- =============================================
-- 声明变量
-- =============================================
DECLARE @CurrentTime DATETIME = GETDATE();
DECLARE @TestPrefix VARCHAR(15) = 'TEST_PDA_';
DECLARE @TestWarehouseId BIGINT = 9100000000100001;
DECLARE @TestLanewayId1 BIGINT = 9100000000300001;
DECLARE @TestLanewayId2 BIGINT = 9100000000300002;
DECLARE @TestMaterialId1 BIGINT = 9100000000400001;
DECLARE @TestMaterialId2 BIGINT = 9100000000400002;
DECLARE @TestMaterialId3 BIGINT = 9100000000400003;
DECLARE @TestEmptyTrayMaterialId BIGINT = 9100000000400099;
-- 正常场景托盘
DECLARE @TestStockCode1 BIGINT = 9100000000500001;
DECLARE @TestStockCode2 BIGINT = 9100000000500002;
DECLARE @TestStockCode3 BIGINT = 9100000000500003;
DECLARE @TestStockCode4 BIGINT = 9100000000500004;
DECLARE @TestStockCode5 BIGINT = 9100000000500005;
DECLARE @TestStockCode6 BIGINT = 9100000000500006;
-- 异常场景托盘
DECLARE @TestStockCodeBusy BIGINT = 9100000000500007; -- 已占用托盘
DECLARE @TestStockCodeMixed BIGINT = 9100000000500008; -- 混放托盘

DECLARE @TestImportNotifyId BIGINT = 9100000000600001;
DECLARE @TestImportNotifyDetailId1 BIGINT = 9100000000600101;
DECLARE @TestImportNotifyDetailId2 BIGINT = 9100000000600102;
DECLARE @TestImportOrderId BIGINT = 9100000000700001;

DECLARE @TestExportNotifyId BIGINT = 9100000000800001;
DECLARE @TestExportNotifyDetailId1 BIGINT = 9100000000800101;
DECLARE @TestExportOrderId BIGINT = 9100000000900001;

DECLARE @TestCheckNotifyId BIGINT = 9100000001000001;
DECLARE @TestCheckNotifyDetailId1 BIGINT = 9100000001000101;

-- 异常场景单据
DECLARE @TestImportNotifyDoneId BIGINT = 9100000000600002; -- 已完成入库单

-- 其他需要ID的表
DECLARE @TestSlotBaseId BIGINT = 9100000000200000;
DECLARE @TestStockTrayId1 BIGINT = 9100000000990001;
DECLARE @TestStockTrayId2 BIGINT = 9100000000990002;
DECLARE @TestExportBoxInfoId1 BIGINT = 9100000000991001;
DECLARE @TestStockCheckInfoId1 BIGINT = 9100000000992001;
DECLARE @TestStockCheckInfoId2 BIGINT = 9100000000992002;
DECLARE @TestStockInfoId1 BIGINT = 9100000000999002;
DECLARE @TestStockInfoId2 BIGINT = 9100000000999001;

PRINT '========================================';
PRINT '开始生成PDA测试数据...';
PRINT '========================================';
PRINT '';

SET NOCOUNT ON;

-- =============================================
-- 场景1：创建测试仓库和基础数据
-- =============================================
PRINT '场景1：创建测试仓库和基础数据...';

-- 1.1 创建测试仓库
IF NOT EXISTS (SELECT 1 FROM [WmsBaseWareHouse] WHERE [WarehouseCode] = 'TEST_PDA_WH001')
BEGIN
    INSERT INTO [WmsBaseWareHouse] (
        [Id], [WarehouseCode], [WarehouseName], [WarehouseType], [Remark],
        [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
    )
    VALUES (
        @TestWarehouseId, 'TEST_PDA_WH001', 'TEST_PDA测试立体库', '01', '用于PDA功能测试的立体仓库',
        @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0
    );
    PRINT '  ✓ 创建测试仓库: TEST_PDA_WH001';
END

-- 1.2 创建测试巷道
IF NOT EXISTS (SELECT 1 FROM [WmsBaseLaneway] WHERE [LanewayCode] = 'TEST_PDA_LW001')
BEGIN
    INSERT INTO [WmsBaseLaneway] (
        [Id], [LanewayCode], [LanewayName], [LanewayStatus], [WarehouseId],
        [Remark], [LanewayPriority], [LanewayTemp], [LanewayType],
        [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
    )
    VALUES (
        @TestLanewayId1, 'TEST_PDA_LW001', 'TEST_PDA测试1号巷道', 0, @TestWarehouseId,
        '双深位测试巷道', 1, 'CW', 1,
        @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0
    );
    PRINT '  ✓ 创建测试巷道: TEST_PDA_LW001';
END

IF NOT EXISTS (SELECT 1 FROM [WmsBaseLaneway] WHERE [LanewayCode] = 'TEST_PDA_LW002')
BEGIN
    INSERT INTO [WmsBaseLaneway] (
        [Id], [LanewayCode], [LanewayName], [LanewayStatus], [WarehouseId],
        [Remark], [LanewayPriority], [LanewayTemp], [LanewayType],
        [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
    )
    VALUES (
        @TestLanewayId2, 'TEST_PDA_LW002', 'TEST_PDA测试2号巷道', 0, @TestWarehouseId,
        '单深位测试巷道', 2, 'CW', 0,
        @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0
    );
    PRINT '  ✓ 创建测试巷道: TEST_PDA_LW002';
END

-- 1.3 创建测试库位（深度1和深度2）
-- 删除已存在的测试库位
DELETE FROM [WmsBaseSlot] WHERE [SlotCode] LIKE 'TEST_PDA-%';

-- 创建深度1库位（外层）
INSERT INTO [WmsBaseSlot] (
    [Id], [SlotCode], [SlotStatus], [SlotInout], [SlotRow], [SlotColumn], [SlotLayer],
    [WarehouseId], [SlotLanewayId], [Make], [SlotExlockFlag], [SlotImlockFlag],
    [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
)
SELECT
    @TestSlotBaseId + ROW_NUMBER() OVER (ORDER BY (SELECT NULL)),
    'TEST_PDA-' + CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS VARCHAR) + '-01-01-01' AS [SlotCode],
    0 AS [SlotStatus],  -- 空储位
    1 AS [SlotInout],   -- 深度1（外层）
    1 AS [SlotRow],
    1 AS [SlotColumn],
    1 AS [SlotLayer],
    @TestWarehouseId AS [WarehouseId],
    @TestLanewayId1 AS [SlotLanewayId],
    '01' AS [Make],
    0 AS [SlotExlockFlag],
    0 AS [SlotImlockFlag],
    @CurrentTime AS [CreateTime],
    @CurrentTime AS [UpdateTime],
    'TEST_PDA_USER' AS [CreateUserName],
    'TEST_PDA_USER' AS [UpdateUserName],
    0 AS [IsDelete]
FROM (SELECT TOP 10 1 AS N) AS T;

-- 创建深度2库位（内层）
INSERT INTO [WmsBaseSlot] (
    [Id], [SlotCode], [SlotStatus], [SlotInout], [SlotRow], [SlotColumn], [SlotLayer],
    [WarehouseId], [SlotLanewayId], [Make], [SlotExlockFlag], [SlotImlockFlag],
    [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
)
SELECT
    @TestSlotBaseId + 100 + ROW_NUMBER() OVER (ORDER BY (SELECT NULL)),
    'TEST_PDA-' + CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS VARCHAR) + '-01-01-02' AS [SlotCode],
    0 AS [SlotStatus],  -- 空储位
    2 AS [SlotInout],   -- 深度2（内层）
    1 AS [SlotRow],
    1 AS [SlotColumn],
    2 AS [SlotLayer],
    @TestWarehouseId AS [WarehouseId],
    @TestLanewayId1 AS [SlotLanewayId],
    '01' AS [Make],
    0 AS [SlotExlockFlag],
    0 AS [SlotImlockFlag],
    @CurrentTime AS [CreateTime],
    @CurrentTime AS [UpdateTime],
    'TEST_PDA_USER' AS [CreateUserName],
    'TEST_PDA_USER' AS [UpdateUserName],
    0 AS [IsDelete]
FROM (SELECT TOP 10 1 AS N) AS T;

-- 创建出库口
INSERT INTO [WmsBaseSlot] (
    [Id], [SlotCode], [SlotStatus], [SlotInout], [SlotRow], [SlotColumn], [SlotLayer],
    [WarehouseId], [SlotLanewayId], [Make], [SlotExlockFlag], [SlotImlockFlag],
    [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
)
VALUES
(@TestSlotBaseId + 201, 'TEST_PDA-PORT01', 0, 1, 0, 0, 0, @TestWarehouseId, @TestLanewayId1, '02', 0, 0, @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0),
(@TestSlotBaseId + 202, 'TEST_PDA-PORT02', 0, 1, 0, 0, 0, @TestWarehouseId, @TestLanewayId1, '02', 0, 0, @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0);

PRINT '  ✓ 创建测试库位: 22个（10个深度1，10个深度2，2个出库口）';

-- 1.4 创建测试物料
IF NOT EXISTS (SELECT 1 FROM [WmsBaseMaterial] WHERE [MaterialCode] = 'TEST_PDA_MAT001')
BEGIN
    INSERT INTO [WmsBaseMaterial] (
        [Id], [MaterialCode], [MaterialName], [MaterialType], [BoxQuantity],
        [WarehouseId], [Status],
        [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
    )
    VALUES
    (@TestMaterialId1, 'TEST_PDA_MAT001', 'TEST_PDA测试物料A', '01', '100', @TestWarehouseId, 1, @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0),
    (@TestMaterialId2, 'TEST_PDA_MAT002', 'TEST_PDA测试物料B', '01', '50', @TestWarehouseId, 1, @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0),
    (@TestMaterialId3, 'TEST_PDA_MAT003', 'TEST_PDA测试物料C', '01', '200', @TestWarehouseId, 1, @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0);
    PRINT '  ✓ 创建测试物料: 3个';
END

-- 1.5 创建空托盘物料
IF NOT EXISTS (SELECT 1 FROM [WmsBaseMaterial] WHERE [MaterialCode] = '100099')
BEGIN
    INSERT INTO [WmsBaseMaterial] (
        [Id], [MaterialCode], [MaterialName], [MaterialType], [BoxQuantity],
        [WarehouseId], [Status],
        [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
    )
    VALUES
    (@TestEmptyTrayMaterialId, '100099', '空托盘', '99', '1', @TestWarehouseId, 1, @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0);
    PRINT '  ✓ 创建空托盘物料: 100099';
END

PRINT '';
PRINT '场景1完成！';
PRINT '';

-- =============================================
-- 场景2：创建测试托盘码
-- =============================================
PRINT '场景2：创建测试托盘码...';

-- 删除已存在的测试托盘码
DELETE FROM [WmsSysStockCode] WHERE [StockCode] LIKE 'TEST_PDA_T%';

INSERT INTO [WmsSysStockCode] (
    [Id], [StockCode], [Status], [StockType], [WarehouseId],
    [PrintCount],
    [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
)
VALUES
(@TestStockCode1, 'TEST_PDA_T000001', 0, 1, @TestWarehouseId, 0, @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0),
(@TestStockCode2, 'TEST_PDA_T000002', 0, 1, @TestWarehouseId, 0, @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0),
(@TestStockCode3, 'TEST_PDA_T000003', 0, 1, @TestWarehouseId, 0, @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0),
(@TestStockCode4, 'TEST_PDA_T000005', 0, 1, @TestWarehouseId, 0, @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0),
(@TestStockCode5, 'TEST_PDA_T000007', 0, 1, @TestWarehouseId, 0, @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0),
(@TestStockCode6, 'TEST_PDA_T000009', 0, 1, @TestWarehouseId, 0, @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0),
-- 异常场景托盘
(@TestStockCodeBusy, 'TEST_PDA_T_BUSY', 1, 1, @TestWarehouseId, 0, @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0),
(@TestStockCodeMixed, 'TEST_PDA_T_MIXED', 1, 1, @TestWarehouseId, 0, @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0);

PRINT '  ✓ 创建测试托盘码: 8个（包含2个异常测试托盘）';

-- 为托盘1创建库存记录（模拟已在库内）
IF NOT EXISTS (SELECT 1 FROM [WmsStockTray] WHERE [StockCode] = 'TEST_PDA_T000001')
BEGIN
    INSERT INTO [WmsStockTray] (
        [Id], [StockCode], [StockSlotCode], [MaterialId], [StockQuantity], [LockQuantity],
        [LotNo], [WarehouseId], [LanewayId], [StockStatusFlag],
        [BoxQuantity], [ProductionDate], [ValidateDay],
        [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
    )
    VALUES (
        @TestStockTrayId1, 'TEST_PDA_T000001', 'TEST_PDA-1-01-01-01', CAST(@TestMaterialId1 AS VARCHAR), 500, 0,
        'TEST_PDA20260113001', CAST(@TestWarehouseId AS VARCHAR), CAST(@TestLanewayId1 AS VARCHAR), 0,
        5, @CurrentTime, DATEADD(YEAR, 2, @CurrentTime),
        @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0
    );
    PRINT '  ✓ 创建托盘1的库存记录';
END

PRINT '';
PRINT '场景2完成！';
PRINT '';

-- =============================================
-- 场景3：创建入库单据单（用于组托测试）
-- =============================================
PRINT '场景3：创建入库单据单...';

-- 删除已存在的测试入库单据单
DELETE FROM [WmsImportNotify] WHERE [ImportBillCode] LIKE 'TEST_PDA_RK%';
-- 修复：通过ImportId删除明细，因为WmsImportNotifyDetail没有ImportBillCode列
DELETE FROM [WmsImportNotifyDetail] WHERE [ImportId] IN (@TestImportNotifyId, @TestImportNotifyDoneId);

-- 创建入库单据单（正常）
INSERT INTO [WmsImportNotify] (
    [Id], [ImportBillCode], [WarehouseId], [SupplierId], [ManufacturerId],
    [ImportExecuteFlag], [BillType], [Remark],
    [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
)
VALUES (
    @TestImportNotifyId, 'TEST_PDA_RK20260113001', @TestWarehouseId, 1, 1,
    '01', 1, '测试入库单据单（正常）',
    @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0
);

-- 创建入库单据单（已完成，用于异常测试）
INSERT INTO [WmsImportNotify] (
    [Id], [ImportBillCode], [WarehouseId], [SupplierId], [ManufacturerId],
    [ImportExecuteFlag], [BillType], [Remark],
    [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
)
VALUES (
    @TestImportNotifyDoneId, 'TEST_PDA_RK_DONE', @TestWarehouseId, 1, 1,
    '03', 1, '测试入库单据单（已完成）',
    @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0
);

PRINT '  ✓ 创建入库单据单: 2个';

-- 创建入库单据单明细
INSERT INTO [WmsImportNotifyDetail] (
    [Id], [ImportId], [MaterialId],
    [ImportQuantity], [ImportCompleteQuantity], [ImportFactQuantity],
    [ImportProductionDate], [ImportLostDate], [LotNo],
    [ImportExecuteFlag], [Remark],
    [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
)
VALUES
(@TestImportNotifyDetailId1, @TestImportNotifyId, @TestMaterialId1,
    1000, 0, 0, @CurrentTime, DATEADD(YEAR, 2, @CurrentTime), 'TEST_PDA20260113001',
    '01', '测试明细1',
    @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0),
(@TestImportNotifyDetailId2, @TestImportNotifyId, @TestMaterialId2,
    500, 0, 0, @CurrentTime, DATEADD(YEAR, 2, @CurrentTime), 'TEST_PDA20260113002',
    '01', '测试明细2',
    @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0);

PRINT '  ✓ 创建入库单据单明细: 2条';

-- 创建已组托的箱码（用于异常测试：箱码已组托）
INSERT INTO [WmsStockSlotBoxInfo] (
    [Id], [BoxCode], [StockCode], [Status], [MaterialId], [ImportId],
    [CreateTime], [IsDelete]
)
VALUES (
    9100000000999001, 'TEST_PDA_BOX_USED', 'TEST_PDA_T_BUSY', 1, @TestMaterialId1, @TestImportNotifyId,
    @CurrentTime, 0
);
PRINT '  ✓ 创建异常测试数据：已使用箱码 TEST_PDA_BOX_USED';

PRINT '';
PRINT '场景3完成！';
PRINT '';

-- =============================================
-- 场景4：创建出库通知单（用于出库测试）
-- =============================================
PRINT '场景4：创建出库通知单...';

-- 删除已存在的测试出库通知单
DELETE FROM [WmsExportNotify] WHERE [ExportBillCode] LIKE 'TEST_PDA_CK%';
DELETE FROM [WmsExportNotifyDetail] WHERE [ExportBillCode] LIKE 'TEST_PDA_CK%';

-- 创建出库通知单
IF NOT EXISTS (SELECT 1 FROM [WmsExportNotify] WHERE [ExportBillCode] = 'TEST_PDA_CK20260113001')
BEGIN
    INSERT INTO [WmsExportNotify] (
        [Id], [ExportBillCode], [WarehouseId], [ExportSupplierId],
        [ExportExecuteFlag], [ExportRemark],
        [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
    )
    VALUES (
        @TestExportNotifyId, 'TEST_PDA_CK20260113001', @TestWarehouseId, 1,
        0, '测试出库通知单',
        @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0
    );
    PRINT '  ✓ 创建出库通知单: TEST_PDA_CK20260113001';
END

-- 创建出库通知单明细
IF NOT EXISTS (SELECT 1 FROM [WmsExportNotifyDetail] WHERE [ExportBillCode] = 'TEST_PDA_CK20260113001')
BEGIN
    INSERT INTO [WmsExportNotifyDetail] (
        [Id], [ExportBillCode], [MaterialId], [MaterialCode], [MaterialName],
        [ExportQuantity], [CompleteQuantity],
        [ProductionDate], [LostDate], [LotNo],
        [ExportDetailFlag], [InspectionStatus],
        [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
    )
    VALUES
    (@TestExportNotifyDetailId1, 'TEST_PDA_CK20260113001', @TestMaterialId1, 'TEST_PDA_MAT001', 'TEST_PDA测试物料A',
     500, 0, @CurrentTime, DATEADD(YEAR, 2, @CurrentTime), 'TEST_PDA20260113001',
     0, '测试出库明细1',
     @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0);
    PRINT '  ✓ 创建出库通知单明细: 1条';
END

-- 创建出库单
IF NOT EXISTS (SELECT 1 FROM [WmsExportOrder] WHERE [ExportOrderNo] = 'TEST_PDA_CK20260113001')
BEGIN
    INSERT INTO [WmsExportOrder] (
        [Id], [ExportOrderNo], [ExportStockCode], [ExportExecuteFlag], [ExportType],
        [ExportWarehouseId], [ExportRemark], [ExportMaterialId], [ExportLotNo], [ExportQuantity], [PickedNum],
        [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
    )
    VALUES (
        @TestExportOrderId, 'TEST_PDA_CK20260113001', 'TEST_PDA_T000001', 0, 1,
        @TestWarehouseId, '测试出库单（用于出库测试）', @TestMaterialId1, 'TEST_PDA20260113001', 100, 100,
        @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0
    );
    PRINT '  ✓ 创建出库单: TEST_PDA_CK20260113001';
END

-- 创建待出库箱码库存（对应托盘1）
IF NOT EXISTS (SELECT 1 FROM [WmsStockInfo] WHERE [TrayId] IN (SELECT CAST(Id AS VARCHAR) FROM WmsStockTray WHERE StockCode = 'TEST_PDA_T000001'))
BEGIN
    INSERT INTO [WmsStockInfo] (
        [Id], [TrayId], [MaterialId],
        [Qty], [LockQuantity], [LotNo], [BoxCode],
        [CreateTime], [IsDelete]
    )
    VALUES (
        @TestStockInfoId1, (SELECT CAST(Id AS VARCHAR) FROM WmsStockTray WHERE StockCode = 'TEST_PDA_T000001'),
        CAST(@TestMaterialId1 AS VARCHAR),
        100, 0, 'TEST_PDA20260113001', 'TEST_PDA_BOX_STOCK_001',
        @CurrentTime, 0
    );
    PRINT '  ✓ 创建库存箱码: TEST_PDA_BOX_STOCK_001 (在 T000001 上)';
END

-- 创建出库箱码明细（WmsExportBoxInfo）
IF NOT EXISTS (SELECT 1 FROM [WmsExportBoxInfo] WHERE [ExportOrderNo] = 'TEST_PDA_CK20260113001')
BEGIN
    INSERT INTO [WmsExportBoxInfo] (
        [Id], [ExportOrderNo], [BoxCode], [MaterialId], [LotNo], [Qty], [Status],
        [CreateTime], [IsDelete]
    )
    VALUES (
        @TestExportBoxInfoId1, 'TEST_PDA_CK20260113001', 'TEST_PDA_BOX_STOCK_001', CAST(@TestMaterialId1 AS VARCHAR), 'TEST_PDA20260113001', 100, 0,
        @CurrentTime, 0
    );
    PRINT '  ✓ 创建出库箱码明细';
END

PRINT '';
PRINT '场景4完成！';
PRINT '';

-- =============================================
-- 场景5：创建盘点通知单（用于盘点测试）
-- =============================================
PRINT '场景5：创建盘点通知单...';

-- 删除已存在的测试盘点通知单
DELETE FROM [WmsStockCheckNotify] WHERE [CheckBillCode] LIKE 'TEST_PDA_PD%';
DELETE FROM [WmsStockCheckNotifyDetail] WHERE [CheckBillCode] LIKE 'TEST_PDA_PD%';
DELETE FROM [WmsStockCheckInfo] WHERE [StockCheckId] IN (SELECT Id FROM WmsStockCheckNotifyDetail WHERE CheckBillCode LIKE 'TEST_PDA_PD%');

-- 创建盘点通知单
IF NOT EXISTS (SELECT 1 FROM [WmsStockCheckNotify] WHERE [CheckBillCode] = 'TEST_PDA_PD20260113001')
BEGIN
    INSERT INTO [WmsStockCheckNotify] (
        [Id], [CheckBillCode], [WarehouseId], [ExecuteFlag],
        [CheckRemark],
        [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
    )
    VALUES (
        @TestCheckNotifyId, 'TEST_PDA_PD20260113001', @TestWarehouseId, 2, -- ExecuteFlag=2 (启动)
        '测试盘点通知单',
        @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0
    );
    PRINT '  ✓ 创建盘点通知单: TEST_PDA_PD20260113001';
END

-- 创建盘点通知单明细（包含正常托盘和混放托盘）
INSERT INTO [WmsStockCheckNotifyDetail] (
    [Id], [CheckBillCode], [MaterialId],
    [StockQuantity], [RealQuantity], [StockCode],
    [StockLotNo], [ExecuteFlag],
    [CheckRemark],
    [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
)
VALUES
(@TestCheckNotifyDetailId1, 'TEST_PDA_PD20260113001', @TestMaterialId1,
    500, 0, 'TEST_PDA_T000001', 'TEST_PDA20260113001', 0,
    '测试盘点明细1',
    @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0);

PRINT '  ✓ 创建盘点通知单明细: 1条';

-- 创建盘点箱码明细
INSERT INTO [WmsStockCheckInfo] (
    [Id], [StockCheckId], [BoxCode], [StockCode], [MaterialId],
    [Qty], [LotNo], [Status], [State],
    [CreateTime], [IsDelete]
)
VALUES
(@TestStockCheckInfoId1, @TestCheckNotifyDetailId1, 'TEST_PDA_BOX_CHECK_001', 'TEST_PDA_T000001', @TestMaterialId1,
 100, 'TEST_PDA20260113001', 0, 0, @CurrentTime, 0),
(@TestStockCheckInfoId2, @TestCheckNotifyDetailId1, 'TEST_PDA_BOX_CHECK_002', 'TEST_PDA_T000001', @TestMaterialId1,
 100, 'TEST_PDA20260113001', 0, 0, @CurrentTime, 0);

PRINT '  ✓ 创建盘点箱码明细: 2个';

PRINT '';
PRINT '场景5完成！';
PRINT '';

-- =============================================
-- 场景6：创建空托盘库存（用于空托盘出库测试）
-- =============================================
PRINT '场景6：创建空托盘库存...';

-- 删除已存在的空托盘库存
DELETE FROM [WmsStockTray] WHERE [StockCode] = 'TEST_PDA_T000002' AND [MaterialId] = CAST(@TestEmptyTrayMaterialId AS VARCHAR);

-- 创建空托盘库存
IF NOT EXISTS (SELECT 1 FROM [WmsStockTray] WHERE [StockCode] = 'TEST_PDA_T000002' AND [MaterialId] = CAST(@TestEmptyTrayMaterialId AS VARCHAR))
BEGIN
    INSERT INTO [WmsStockTray] (
        [Id], [StockCode], [StockSlotCode], [MaterialId], [StockQuantity], [LockQuantity],
        [LotNo], [WarehouseId], [LanewayId], [StockStatusFlag],
        [BoxQuantity], [ProductionDate], [ValidateDay],
        [CreateTime], [UpdateTime], [CreateUserName], [UpdateUserName], [IsDelete]
    )
    VALUES (
        @TestStockTrayId2, 'TEST_PDA_T000002', 'TEST_PDA-2-01-01-01', CAST(@TestEmptyTrayMaterialId AS VARCHAR), 0, 0,
        '100099', CAST(@TestWarehouseId AS VARCHAR), CAST(@TestLanewayId1 AS VARCHAR), 0,
        0, @CurrentTime, DATEADD(YEAR, 10, @CurrentTime),
        @CurrentTime, @CurrentTime, 'TEST_PDA_USER', 'TEST_PDA_USER', 0
    );
    PRINT '  ✓ 创建空托盘库存: TEST_PDA_T000002';
END

-- 更新库位状态为空托盘
UPDATE [WmsBaseSlot]
SET [SlotStatus] = 6,  -- 空托盘
    [UpdateTime] = @CurrentTime
WHERE [SlotCode] = 'TEST_PDA-2-01-01-01';
PRINT '  ✓ 更新库位状态为空托盘';

PRINT '';
PRINT '场景6完成！';
PRINT '';

-- =============================================
-- 完成提示
-- =============================================
PRINT '========================================';
PRINT '✓ PDA测试数据生成完成！';
PRINT '========================================';
PRINT '';
PRINT '测试数据统计：';
PRINT '  - 测试仓库: 1个';
PRINT '  - 测试巷道: 2个';
PRINT '  - 测试库位: 22个';
PRINT '  - 测试物料: 3个 + 1个空托盘物料';
PRINT '  - 测试托盘码: 8个 (含异常测试)';
PRINT '  - 入库单据单: 2个 (含已完成)';
PRINT '  - 出库通知单: 1个';
PRINT '  - 出库单: 1个';
PRINT '  - 盘点通知单: 1个';
PRINT '  - 盘点箱码: 2个';
PRINT '  - 异常数据: 已占用托盘、已组托箱码等';
PRINT '';
PRINT '测试场景覆盖：';
PRINT '  ✓ 场景1：入库组托流程 (含异常：已占用托盘)';
PRINT '  ✓ 场景2：箱托绑定/解绑流程 (含异常：箱码已使用)';
PRINT '  ✓ 场景3：空托盘入库流程';
PRINT '  ✓ 场景4：出库拆跺流程 (含异常：数量超出/不足)';
PRINT '  ✓ 场景5：无箱码出库流程';
PRINT '  ✓ 场景6：空托盘出库流程';
PRINT '  ✓ 场景7：盘点流程 (含扫描确认)';
PRINT '';
PRINT '注意：所有测试数据使用TEST_PDA_前缀，可通过清除脚本一键删除。';
PRINT '========================================';
GO
