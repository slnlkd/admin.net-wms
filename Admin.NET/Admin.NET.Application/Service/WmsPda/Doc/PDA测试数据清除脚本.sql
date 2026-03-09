-- =============================================
-- PDA测试数据清除脚本
-- 功能：一键删除所有PDA测试数据
-- 特点：
--   1. 按照依赖关系逆序删除，避免外键约束错误
--   2. 使用TEST_PDA_前缀识别测试数据
--   3. 不影响生产数据
--   4. 包含安全确认机制
--
-- 作者：Sisyphus
-- 更新时间：2026-01-19
-- =============================================

USE [JC44_WMS];
GO

-- =============================================
-- 安全确认（防止误操作）
-- =============================================
PRINT '========================================';
PRINT '警告：此脚本将删除所有PDA测试数据！';
PRINT '========================================';
PRINT '';
PRINT '即将删除的数据包括：';
PRINT '  - 所有以TEST_PDA_开头的测试数据';
PRINT '  - 所有ID以910000000开头的测试数据';
PRINT '';
PRINT '请确认您要继续执行此操作。';
PRINT '按 Ctrl+C 取消，或按任意键继续...';
PRINT '========================================';
PRINT '';

-- =============================================
-- 按照依赖关系逆序删除测试数据
-- =============================================
PRINT '开始删除PDA测试数据...';
PRINT '';

SET NOCOUNT ON;

-- =============================================
-- 1. 删除业务数据（从子表到父表）
-- =============================================
PRINT '步骤1：删除业务数据...';

-- 1.1 删除托盘变更记录
DELETE FROM [WmsStockUnbind] WHERE [UpbindStockCode] LIKE 'TEST_PDA_T%' OR [BindStockCode] LIKE 'TEST_PDA_T%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除托盘变更记录: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.2 删除盘点箱码信息
DELETE FROM [WmsStockCheckInfo] WHERE [StockCheckId] IN (SELECT [Id] FROM [WmsStockCheckNotifyDetail] WHERE [CheckBillCode] LIKE 'TEST_PDA_PD%');
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除盘点箱码信息: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.3 删除盘点通知单明细
DELETE FROM [WmsStockCheckNotifyDetail] WHERE [CheckBillCode] LIKE 'TEST_PDA_PD%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除盘点通知单明细: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.4 删除盘点通知单
DELETE FROM [WmsStockCheckNotify] WHERE [CheckBillCode] LIKE 'TEST_PDA_PD%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除盘点通知单: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.5 删除出库任务
DELETE FROM [WmsExportTask] WHERE [ExportTaskNo] LIKE 'TEST_PDA_CK%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除出库任务: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.6 删除出库箱码信息
DELETE FROM [WmsExportBoxInfo] WHERE [ExportOrderNo] LIKE 'TEST_PDA_CK%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除出库箱码信息: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.7 删除人工拆跺
DELETE FROM [WmsManualDepalletizing] WHERE [ExportOrderNo] LIKE 'TEST_PDA_CK%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除人工拆跺: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.8 删除出库单
DELETE FROM [WmsExportOrder] WHERE [ExportOrderNo] LIKE 'TEST_PDA_CK%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除出库单: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.9 删除出库通知单明细
DELETE FROM [WmsExportNotifyDetail] WHERE [ExportBillCode] LIKE 'TEST_PDA_CK%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除出库通知单明细: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.10 删除出库通知单
DELETE FROM [WmsExportNotify] WHERE [ExportBillCode] LIKE 'TEST_PDA_CK%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除出库通知单: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.11 删除入库流水（箱码信息）
DELETE FROM [WmsStockSlotBoxInfo] WHERE [StockCode] LIKE 'TEST_PDA_T%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除入库流水（箱码信息）: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.12 删除入库单
DELETE FROM [WmsImportOrder] WHERE [ImportOrderNo] LIKE 'TEST_PDA_RK%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除入库单: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.13 删除入库单据单明细
-- 修复：通过子查询 ImportId 删除明细，因为 WmsImportNotifyDetail 没有 ImportBillCode 列
DELETE FROM [WmsImportNotifyDetail] WHERE [ImportId] IN (SELECT [Id] FROM [WmsImportNotify] WHERE [ImportBillCode] LIKE 'TEST_PDA_RK%');
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除入库单据单明细: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.14 删除入库单据单
DELETE FROM [WmsImportNotify] WHERE [ImportBillCode] LIKE 'TEST_PDA_RK%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除入库单据单: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.15 删除标签打印数据
DELETE FROM [WmsImportLabelPrint] WHERE [ImportBillCode] LIKE 'TEST_PDA_RK%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除标签打印数据: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.16 删除库存明细
DELETE FROM [WmsStockInfo] WHERE [BoxCode] LIKE 'TEST_PDA_%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除库存明细: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.17 删除库存托盘
DELETE FROM [WmsStockTray] WHERE [StockCode] LIKE 'TEST_PDA_T%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除库存托盘: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 1.18 删除库存汇总
DELETE FROM [WmsStock] WHERE [MaterialId] BETWEEN 9100000000400001 AND 9100000000400099;
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除库存汇总: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

PRINT '';
PRINT '步骤1完成！';
PRINT '';

-- =============================================
-- 2. 删除基础数据
-- =============================================
PRINT '步骤2：删除基础数据...';

-- 2.1 删除托盘码
DELETE FROM [WmsSysStockCode] WHERE [StockCode] LIKE 'TEST_PDA_T%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除托盘码: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 2.2 删除库位
DELETE FROM [WmsBaseSlot] WHERE [SlotCode] LIKE 'TEST_PDA-%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除库位: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 2.3 删除物料
DELETE FROM [WmsBaseMaterial] WHERE [MaterialCode] LIKE 'TEST_PDA_%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除物料: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 2.4 删除巷道
DELETE FROM [WmsBaseLaneway] WHERE [LanewayCode] LIKE 'TEST_PDA_%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除巷道: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

-- 2.5 删除仓库
DELETE FROM [WmsBaseWareHouse] WHERE [WarehouseCode] LIKE 'TEST_PDA_%';
IF @@ROWCOUNT > 0 PRINT '  ✓ 删除仓库: ' + CAST(@@ROWCOUNT AS VARCHAR) + '条';

PRINT '';
PRINT '步骤2完成！';
PRINT '';

-- =============================================
-- 完成提示
-- =============================================
PRINT '========================================';
PRINT '✓ PDA测试数据清除完成！';
PRINT '========================================';
PRINT '';
PRINT '已删除的测试数据包括：';
PRINT '  - 业务数据（入库、出库、盘点、拆跺等）';
PRINT '  - 基础数据（仓库、巷道、库位、物料、托盘等）';
PRINT '  - 所有以TEST_PDA_开头或ID以910000000开头的数据';
PRINT '';
PRINT '数据库已恢复到测试前的状态。';
PRINT '========================================';
GO
