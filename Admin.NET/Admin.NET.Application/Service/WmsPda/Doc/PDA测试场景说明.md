# PDA测试场景说明文档

## 文档说明

本文档详细说明了PDA功能的所有测试场景，包括测试步骤、验证点和预期结果。每个场景都可以独立执行，覆盖了入库、出库、盘库等所有业务流程。

---

## 测试数据准备

在执行任何测试场景之前，请先执行测试数据生成脚本：

```sql
-- 执行测试数据生成脚本
-- 文件路径：PDA测试数据生成脚本.sql
```

执行完成后，系统将生成以下测试数据：

| 数据类型 | 数量 | 说明 |
|---------|------|------|
| 测试仓库 | 1 | TEST_PDA_WH001 |
| 测试巷道 | 2 | TEST_PDA_LW001（双深位）、TEST_PDA_LW002（单深位） |
| 测试库位 | 22 | 10个深度1，10个深度2，2个出库口 |
| 测试物料 | 3 | TEST_PDA_MAT001、TEST_PDA_MAT002、TEST_PDA_MAT003 |
| 空托盘物料 | 1 | 100099 |
| 测试托盘码 | 8 | TEST_PDA_T000001 ~ T000009，以及异常测试托盘 T_BUSY, T_MIXED |
| 入库单据单 | 2 | TEST_PDA_RK20260113001（正常），TEST_PDA_RK_DONE（已完成） |
| 出库通知单 | 1 | TEST_PDA_CK20260113001（1条明细） |
| 出库单 | 1 | TEST_PDA_CK20260113001 |
| 盘点通知单 | 1 | TEST_PDA_PD20260113001（1条明细） |
| 空托盘库存 | 1 | TEST_PDA_T000002 |

---

## 场景1：入库组托流程

### 场景描述
模拟PDA端的入库组托流程，包括查询组托信息、计算数量、保存绑定等操作。

### 测试步骤

#### 步骤1：入库组托信息查询（GetStockSoltBoxInfo）
**接口**：`POST /api/WmsPda/GetStockSoltBoxInfo`

**请求参数**：
```json
{
  "Type": "0",
  "MaterialBarcode": "9100000000600101-9100000000400001-TEST_PDA20260113001",
  "ImportBillCode": "TEST_PDA_RK20260113001"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 返回组托明细列表
- ✅ 包含物料编码、批次号、数量等信息

**预期结果**：
```json
{
  "Success": true,
  "Message": "查询成功",
  "Items": [
    {
      "DetailId": 9100000000600101,
      "MaterialId": 9100000000400001,
      "MaterialCode": "TEST_PDA_MAT001",
      "LotNo": "TEST_PDA20260113001",
      "Qty": 1000
    }
  ]
}
```

---

#### 步骤2：无箱码组托数量计算（GetScanQty）
**接口**：`POST /api/WmsPda/GetScanQty`

**请求参数**：
```json
{
  "MaterialBarcode": "9100000000600101-9100000000400001-TEST_PDA20260113001",
  "StockCode": "TEST_PDA_T000002",
  "ImportBillCode": "TEST_PDA_RK20260113001"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 返回推荐数量
- ✅ 推荐数量不超过剩余可入库数量

**预期结果**：
```json
{
  "Success": true,
  "Message": "计算成功",
  "Qty": 100
}
```

---

#### 步骤3：无箱码保存载具绑定（SaveBoxInfo）
**接口**：`POST /api/WmsPda/SaveBoxInfo`

**请求参数**：
```json
{
  "StockCode": "TEST_PDA_T000002",
  "ImportBillCode": "TEST_PDA_RK20260113001",
  "MaterialBarcode": "9100000000600101-9100000000400001-TEST_PDA20260113001",
  "Qty": 100
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 创建入库流水（WmsImportOrder）
- ✅ 创建箱码信息（WmsStockSlotBoxInfo）
- ✅ 更新托盘状态（Status=1，已使用）
- ✅ 更新入库单明细状态

**预期结果**：
```json
{
  "Success": true,
  "Message": "绑定成功"
}
```

---

#### 步骤4：托盘明细查询（GetPalnoDetailInfo）
**接口**：`POST /api/WmsPda/GetPalnoDetailInfo`

**请求参数**：
```json
{
  "StockCode": "TEST_PDA_T000002"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 返回托盘下的箱码信息列表
- ✅ 按物料、批次分组聚合

**预期结果**：
```json
{
  "Success": true,
  "Message": "查询成功",
  "Items": [
    {
      "MaterialCode": "TEST_PDA_MAT001",
      "MaterialName": "TEST_PDA测试物料A",
      "LotNo": "TEST_PDA20260113001",
      "TotalQty": 100
    }
  ]
}
```

---

### 数据验证

执行以下SQL验证数据正确性：

```sql
-- 验证入库流水
SELECT * FROM WmsImportOrder WHERE StockCode = 'TEST_PDA_T000002';

-- 验证箱码信息
SELECT * FROM WmsStockSlotBoxInfo WHERE StockCode = 'TEST_PDA_T000002';

-- 验证托盘状态
SELECT * FROM WmsSysStockCode WHERE StockCode = 'TEST_PDA_T000002';
```

---

## 场景2：箱托绑定/解绑流程

### 场景描述
测试箱码与托盘的绑定和解绑操作。

### 测试步骤

#### 步骤1：扫码获取箱托信息（GetImportInfoByBoxCode）
**接口**：`POST /api/WmsPda/GetImportInfoByBoxCode`

**请求参数**：
```json
{
  "BoxCode": "TEST_PDA_BOX001"
}
```

**说明**：需要先通过SaveBoxInfo创建箱码记录

**验证点**：
- ✅ 返回Success=true
- ✅ 返回箱码对应的托盘、物料、批次等信息
- ✅ 判断箱码状态

**预期结果**：
```json
{
  "Success": true,
  "Message": "查询成功",
  "BoxInfo": {
    "BoxCode": "TEST_PDA_BOX001",
    "StockCode": "TEST_PDA_T000002",
    "MaterialCode": "TEST_PDA_MAT001",
    "Status": 0
  }
}
```

---

#### 步骤2：托盘状态查询（GetPalnoStatus）
**接口**：`POST /api/WmsPda/GetPalnoStatus`

**请求参数**：
```json
{
  "PalletCode": "TEST_PDA_T000002"
}
```

**验证点**：
- ✅ 返回Valid=true（托盘有效）
- ✅ 返回状态描述

**预期结果**：
```json
{
  "Valid": true,
  "Message": "有效载具"
}
```

---

#### 步骤3：绑定箱托关系（BackConfirm）
**接口**：`POST /api/WmsPda/BackConfirm`

**请求参数**：
```json
{
  "StockCode": "TEST_PDA_T000003",
  "ImportBillCode": "TEST_PDA_RK20260113001",
  "BoxCode": "TEST_PDA_BOX002",
  "Qty": 50
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 生成或更新入库流水
- ✅ 绑定箱码到托盘
- ✅ 更新入库单明细执行状态

**预期结果**：
```json
{
  "Success": true,
  "Message": "绑定成功"
}
```

---

#### 步骤4：删除单个箱码绑定（DelStockSoltBoxInfo）
**接口**：`POST /api/WmsPda/DelStockSoltBoxInfo`

**请求参数**：
```json
{
  "BoxCode": "TEST_PDA_BOX002",
  "StockCode": "TEST_PDA_T000003"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 软删除箱码记录
- ✅ 更新入库单明细状态

**预期结果**：
```json
{
  "Success": true,
  "Message": "删除成功"
}
```

---

#### 步骤5：托盘箱码解绑（RemoveBoxHolds）
**接口**：`POST /api/WmsPda/RemoveBoxHolds`

**请求参数**：
```json
{
  "StockCode": "TEST_PDA_T000003",
  "ImportBillCode": "TEST_PDA_RK20260113001"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 软删除所有箱码记录
- ✅ 恢复托盘状态为未使用

**预期结果**：
```json
{
  "Success": true,
  "Message": "解绑成功"
}
```

---

### 数据验证

```sql
-- 验证箱码信息状态
SELECT * FROM WmsStockSlotBoxInfo WHERE StockCode = 'TEST_PDA_T000003';

-- 验证托盘状态
SELECT * FROM WmsSysStockCode WHERE StockCode = 'TEST_PDA_T000003';
```

---

## 场景3：空托盘入库流程

### 场景描述
测试空托盘的入库流程，包括空托盘组盘、批量生成空托入库数据等操作。

### 测试步骤

#### 步骤1：空托盘组盘（KBackConfirm - 绑定）
**接口**：`POST /api/WmsPda/KBackConfirm`

**请求参数**：
```json
{
  "PalletCode": "TEST_PDA_T000003",
  "Quantity": 1,
  "ActionType": "add",
  "UserId": "WCS"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 创建入库流水
- ✅ 创建箱码信息
- ✅ 更新托盘状态为已使用

**预期结果**：
```json
{
  "Success": true,
  "Message": "绑定成功！"
}
```

---

#### 步骤2：批量生成空托入库数据（AddWmsImportOrder）
**接口**：`POST /api/WmsPda/AddWmsImportOrder`

**请求参数**：无

**验证点**：
- ✅ 返回Success=true
- ✅ 遍历闲置托盘（尾号为奇数）
- ✅ 为每个托盘生成空托流水与箱码
- ✅ 更新托盘状态

**预期结果**：
```json
{
  "Success": true,
  "Message": "批量生成空托入库数据成功"
}
```

**说明**：此接口会为TEST_PDA_T000003、TEST_PDA_T000005、TEST_PDA_T000007、TEST_PDA_T000009生成空托入库数据。

---

#### 步骤3：暂存入库生成（TemporaryStorage）
**接口**：`POST /api/WmsPda/TemporaryStorage`

**请求参数**：
```json
{
  "MainTrayCode": "TEST_PDA_T000001",
  "SubTrayCode": "TEST_PDA_T000002"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 基于最新出库流水生成暂存入库流水
- ✅ 回写箱码关联

**预期结果**：
```json
{
  "Success": true,
  "Message": "暂存入库成功"
}
```

---

#### 步骤4：叠箱绑定（StackingBindings）
**接口**：`POST /api/WmsPda/StackingBindings`

**请求参数**：
```json
{
  "MainTrayCode": "TEST_PDA_T000001",
  "SubTrayCode": "TEST_PDA_T000002"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 校验主/副载具最新入库流水是否同业务
- ✅ 统一写入主载具编号

**预期结果**：
```json
{
  "Success": true,
  "Message": "叠箱绑定成功"
}
```

---

### 数据验证

```sql
-- 验证空托盘入库流水
SELECT * FROM WmsImportOrder WHERE StockCode IN ('TEST_PDA_T000003', 'TEST_PDA_T000005', 'TEST_PDA_T000007', 'TEST_PDA_T000009');

-- 验证空托盘箱码信息
SELECT * FROM WmsStockSlotBoxInfo WHERE StockCode IN ('TEST_PDA_T000003', 'TEST_PDA_T000005', 'TEST_PDA_T000007', 'TEST_PDA_T000009');
```

---

## 场景4：出库拆跺流程

### 场景描述
测试有箱码的拆跺出库流程。

### 测试步骤

#### 步骤1：拆跺箱码信息查询（GetManBoxInfo）
**接口**：`POST /api/WmsPda/GetManBoxInfo`

**请求参数**：
```json
{
  "BoxCode": "TEST_PDA_BOX001",
  "TrayCode": "TEST_PDA_T000001",
  "ExportOrderNo": "TEST_PDA_CK20260113001"
}
```

**说明**：需要先通过其他接口创建拆跺记录

**验证点**：
- ✅ 返回Code=1（成功）
- ✅ 返回箱码详细信息
- ✅ 验证托盘与箱码的匹配关系
- ✅ 检查托盘与订单的质检状态是否一致

**预期结果**：
```json
{
  "Code": 1,
  "Message": "查询成功!",
  "BoxInfo": {
    "BoxCode": "TEST_PDA_BOX001",
    "TrayCode": "TEST_PDA_T000001",
    "MaterialCode": "TEST_PDA_MAT001",
    "Qty": 100
  },
  "Status": "1",
  "InspectionStatus": "0"
}
```

---

#### 步骤2：托盘关联出库单查询（GetExprotCodeById）
**接口**：`POST /api/WmsPda/GetExprotCodeById`

**请求参数**：
```json
{
  "StockCode": "TEST_PDA_T000001",
  "IsBoxCode": ""
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 返回出库单信息列表
- ✅ 筛选出已执行完成的出库流水

**预期结果**：
```json
{
  "Success": true,
  "Message": "查询成功",
  "Data": [
    {
      "ExportOrderNo": "TEST_PDA_CK20260113001",
      "StockCode": "TEST_PDA_T000001",
      "MaterialCode": "TEST_PDA_MAT001"
    }
  ]
}
```

---

#### 步骤3：拆跺信息查询（GetTrayInfo）
**接口**：`POST /api/WmsPda/GetTrayInfo`

**请求参数**：
```json
{
  "TrayCode": "TEST_PDA_T000001",
  "ExportOrderNo": "TEST_PDA_CK20260113001"
}
```

**验证点**：
- ✅ 返回Code=1（成功）
- ✅ 返回该托盘下的所有箱码信息

**预期结果**：
```json
{
  "Code": 1,
  "Message": "查询成功",
  "BoxInfo": [
    {
      "BoxCode": "TEST_PDA_BOX001",
      "Qty": 100
    }
  ]
}
```

---

#### 步骤4：拆跺数量计算（GetBoxSum）
**接口**：`POST /api/WmsPda/GetBoxSum`

**请求参数**：
```json
{
  "BoxCodeList": ["TEST_PDA_BOX001", "TEST_PDA_BOX002"],
  "ExportOrderNo": "TEST_PDA_CK20260113001"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 返回数量计算结果

**预期结果**：
```json
{
  "Success": true,
  "Message": "计算成功",
  "Data": {
    "TotalQty": 200
  }
}
```

---

#### 步骤5：拆跺箱码绑定（AddManualDepalletizing）
**接口**：`POST /api/WmsPda/AddManualDepalletizing`

**请求参数**：
```json
{
  "BoxCode": "TEST_PDA_BOX001",
  "TrayCode": "TEST_PDA_T000001",
  "ExportOrderNo": "TEST_PDA_CK20260113001",
  "Qty": 100
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 创建拆跺记录
- ✅ 更新库存状态

**预期结果**：
```json
{
  "Success": true,
  "Message": "绑定成功"
}
```

---

#### 步骤6：拆跺记录查询（GetManualDepalletizing）
**接口**：`POST /api/WmsPda/GetManualDepalletizing`

**请求参数**：
```json
{
  "TrayCode": "TEST_PDA_T000001",
  "ExportOrderNo": "TEST_PDA_CK20260113001"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 返回拆跺记录列表

**预期结果**：
```json
{
  "Success": true,
  "Message": "查询成功",
  "Data": [
    {
      "BoxCode": "TEST_PDA_BOX001",
      "TrayCode": "TEST_PDA_T000001",
      "ExportOrderNo": "TEST_PDA_CK20260113001",
      "Qty": 100
    }
  ]
}
```

---

#### 步骤7：人工拆垛出库确认（SaveManTask）
**接口**：`POST /api/WmsPda/SaveManTask`

**请求参数**：
```json
{
  "TrayCode": "TEST_PDA_T000001",
  "ExportOrderNo": "TEST_PDA_CK20260113001",
  "BoxCodeList": ["TEST_PDA_BOX001"]
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 扣减库存
- ✅ 释放托盘
- ✅ 更新任务状态

**预期结果**：
```json
{
  "Success": true,
  "Message": "出库确认成功",
  "Data": {}
}
```

---

#### 步骤8：删除拆跺信息（RemoveManual）
**接口**：`POST /api/WmsPda/RemoveManual`

**请求参数**：
```json
{
  "BoxCode": "TEST_PDA_BOX001",
  "TrayCode": "TEST_PDA_T000001",
  "ExportOrderNo": "TEST_PDA_CK20260113001"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 软删除拆跺记录

**预期结果**：
```json
{
  "Success": true,
  "Message": "删除成功"
}
```

---

### 数据验证

```sql
-- 验证拆跺记录
SELECT * FROM WmsManualDepalletizing WHERE ExportOrderNo = 'TEST_PDA_CK20260113001';

-- 验证库存扣减
SELECT * FROM WmsStockTray WHERE StockCode = 'TEST_PDA_T000001';
```

---

## 场景5：无箱码出库流程

### 场景描述
测试无箱码的拆跺出库流程。

### 测试步骤

#### 步骤1：无箱码拆跺绑定（AddOutManualDepalletizing）
**接口**：`POST /api/WmsPda/AddOutManualDepalletizing`

**请求参数**：
```json
{
  "TrayCode": "TEST_PDA_T000001",
  "ExportOrderNo": "TEST_PDA_CK20260113001",
  "Qty": 100
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 创建无箱码拆跺记录
- ✅ 更新库存状态

**预期结果**：
```json
{
  "Success": true,
  "Message": "绑定成功"
}
```

---

#### 步骤2：无箱码拆跺查询（GetOutManualDepalletizing）
**接口**：`POST /api/WmsPda/GetOutManualDepalletizing`

**请求参数**：
```json
{
  "TrayCode": "TEST_PDA_T000001",
  "ExportOrderNo": "TEST_PDA_CK20260113001"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 返回拆跺记录列表

**预期结果**：
```json
{
  "Success": true,
  "Message": "查询成功",
  "Data": [
    {
      "TrayCode": "TEST_PDA_T000001",
      "ExportOrderNo": "TEST_PDA_CK20260113001",
      "Qty": 100
    }
  ]
}
```

---

#### 步骤3：无箱码人工拆垛出库确认（OutSaveManTask）
**接口**：`POST /api/WmsPda/OutSaveManTask`

**请求参数**：
```json
{
  "TrayCode": "TEST_PDA_T000001",
  "ExportOrderNo": "TEST_PDA_CK20260113001",
  "Qty": 100
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 扣减库存
- ✅ 释放托盘
- ✅ 更新任务状态

**预期结果**：
```json
{
  "Success": true,
  "Message": "出库确认成功",
  "Data": {}
}
```

---

#### 步骤4：无箱码拆跺删除（RemoveOutManual）
**接口**：`POST /api/WmsPda/RemoveOutManual`

**请求参数**：
```json
{
  "TrayCode": "TEST_PDA_T000001",
  "ExportOrderNo": "TEST_PDA_CK20260113001"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 软删除拆跺记录

**预期结果**：
```json
{
  "Success": true,
  "Message": "删除成功"
}
```

---

### 数据验证

```sql
-- 验证拆跺记录
SELECT * FROM WmsManualDepalletizing WHERE ExportOrderNo = 'TEST_PDA_CK20260113001';

-- 验证库存扣减
SELECT * FROM WmsStockTray WHERE StockCode = 'TEST_PDA_T000001';
```

---

## 场景6：空托盘出库流程

### 场景描述
测试空托盘的出库申请流程。

### 测试步骤

#### 步骤1：空托盘出库申请（AddOutPalno）
**接口**：`POST /api/WmsPda/AddOutPalno`

**请求参数**：
```json
{
  "Num": 1,
  "ExportPort": "TEST_PDA-PORT01",
  "HouseCode": "TEST_PDA_WH001"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 查询空托盘库存（TEST_PDA_T000002）
- ✅ 创建出库任务
- ✅ 更新库位状态

**预期结果**：
```json
{
  "Success": true,
  "Message": "空托申请完成",
  "Data": {}
}
```

---

#### 步骤2：出库口列表查询（GetExportPortList）
**接口**：`POST /api/WmsPda/GetExportPortList`

**请求参数**：无

**验证点**：
- ✅ 返回Success=true
- ✅ 返回出库口列表

**预期结果**：
```json
{
  "Success": true,
  "Message": "查询成功",
  "Data": [
    {
      "PortCode": "TEST_PDA-PORT01",
      "PortName": "测试出库口1"
    },
    {
      "PortCode": "TEST_PDA-PORT02",
      "PortName": "测试出库口2"
    }
  ]
}
```

---

### 数据验证

```sql
-- 验证出库任务
SELECT * FROM WmsExportTask WHERE ExportTaskNo LIKE 'TEST_PDA_CK%';

-- 验证空托盘库存
SELECT * FROM WmsStockTray WHERE StockCode = 'TEST_PDA_T000002';
```

---

## 场景7：盘点流程

### 场景描述
测试盘点任务的查询、提交和托盘验证流程。

### 测试步骤

#### 步骤1：盘点单列表查询（GetWmsStockCheckList）
**接口**：`POST /api/WmsPda/GetWmsStockCheckList`

**请求参数**：
```json
{
  "Status": 0
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 返回盘点单列表
- ✅ 根据状态筛选

**预期结果**：
```json
{
  "Success": true,
  "Message": "查询成功",
  "Data": [
    {
      "CheckNotifyNo": "TEST_PDA_PD20260113001",
      "WarehouseName": "TEST_PDA测试立体库",
      "CheckStatus": 0
    }
  ]
}
```

---

#### 步骤2：盘点箱码列表查询（GetStockCheckInfo）
**接口**：`POST /api/WmsPda/GetStockCheckInfo`

**请求参数**：
```json
{
  "CheckTaskNo": "TEST_PDA_PD20260113001"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 返回盘点箱码列表

**预期结果**：
```json
{
  "Success": true,
  "Message": "查询成功",
  "Data": [
    {
      "BoxCode": "TEST_PDA_BOX001",
      "MaterialCode": "TEST_PDA_MAT001",
      "CheckQuantity": 500,
      "ActualQuantity": 0
    }
  ]
}
```

---

#### 步骤3：盘点结果提交（UpdateStockCheckInfo）
**接口**：`POST /api/WmsPda/UpdateStockCheckInfo`

**请求参数**：
```json
{
  "CheckTaskNo": "TEST_PDA_PD20260113001",
  "BoxCode": "TEST_PDA_BOX001",
  "ActualQty": 500,
  "Remark": "盘点正常"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 更新盘点箱码信息
- ✅ 更新盘点任务状态

**预期结果**：
```json
{
  "Success": true,
  "Message": "盘点结果提交成功"
}
```

---

#### 步骤4：托盘验证（IsEnableOkStockCode）
**接口**：`POST /api/WmsPda/IsEnableOkStockCode`

**请求参数**：
```json
{
  "StockCode": "TEST_PDA_T000001"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 验证托盘是否可用

**预期结果**：
```json
{
  "Success": true,
  "Message": "验证成功",
  "Data": {
    "Valid": true
  }
}
```

---

#### 步骤5：根据托盘号获取物料信息（GetMaterialInfoByStockCode）
**接口**：`POST /api/WmsPda/GetMaterialInfoByStockCode`

**请求参数**：
```json
{
  "StockCode": "TEST_PDA_T000001"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 返回托盘下的物料信息

**预期结果**：
```json
{
  "Success": true,
  "Message": "查询成功",
  "Data": [
    {
      "MaterialCode": "TEST_PDA_MAT001",
      "MaterialName": "TEST_PDA测试物料A",
      "StockQuantity": 500
    }
  ]
}
```

---

#### 步骤6：无箱码托盘变更（SaveUnbindWithNoBoxCode）
**接口**：`POST /api/WmsPda/SaveUnbindWithNoBoxCode`

**请求参数**：
```json
{
  "OldStockCode": "TEST_PDA_T000001",
  "NewStockCode": "TEST_PDA_T000002",
  "CheckTaskNo": "TEST_PDA_PD20260113001"
}
```

**验证点**：
- ✅ 返回Success=true
- ✅ 创建托盘变更记录
- ✅ 更新库存托盘信息

**预期结果**：
```json
{
  "Success": true,
  "Message": "托盘变更成功",
  "Data": {}
}
```

---

### 数据验证

```sql
-- 验证盘点箱码信息
SELECT * FROM WmsStockCheckInfo WHERE CheckNotifyNo = 'TEST_PDA_PD20260113001';

-- 验证托盘变更记录
SELECT * FROM WmsStockUnbind WHERE CheckTaskNo = 'TEST_PDA_PD20260113001';
```

---

## 场景8：异常场景验证

### 8.1 托盘已占用 (入库)
**场景描述**: 尝试使用一个已经占用的托盘进行入库组托。
**测试接口**: `GetScanQty` 或 `SaveBoxInfo`
**输入**: 
- 托盘: `TEST_PDA_T_BUSY` (状态为1)
- 物料条码: 任意有效条码
- 入库单: `TEST_PDA_RK20260113001`
**预期结果**: 
- 返回失败
- 消息: "当前托盘已绑定！"

### 8.2 箱码已组托 (入库)
**场景描述**: 尝试扫描一个已经组托的箱码。
**测试接口**: `GetImportInfoByBoxCode`
**输入**: 
- 箱码: `TEST_PDA_BOX_USED` (已在 StockSlotBoxInfo 中，状态为1)
**预期结果**: 
- 返回失败
- 消息: "此箱码已申请入库或已入库！"

### 8.3 入库单已完成 (绑定)
**场景描述**: 尝试向一个已完成的入库单进行绑定。
**测试接口**: `BackConfirm`
**输入**: 
- 入库单: `TEST_PDA_RK_DONE` (ImportExecuteFlag='03')
- 托盘: `TEST_PDA_T000002` (空闲)
- 箱码: 任意新箱码
**预期结果**: 
- 返回失败
- 消息: "入库单已完成!"

---

## 测试数据清理

完成所有测试后，执行测试数据清除脚本：

```sql
-- 执行测试数据清除脚本
-- 文件路径：PDA测试数据清除脚本.sql
```

执行完成后，所有测试数据将被删除，数据库恢复到测试前的状态。

---

## 常见问题排查

### 问题1：入库组托查询失败，提示"物料条码不能为空"
**原因**：type≠1时必须提供物料条码和入库单号
**解决**：检查请求参数是否完整

### 问题2：无箱码组托数量计算失败，提示"物料条码格式错误"
**原因**：物料条码格式不正确，正确格式为"明细ID-物料ID-批次"
**解决**：检查物料条码格式

### 问题3：空托盘组盘失败，提示"此载具无效或已使用"
**原因**：托盘状态不为未使用（Status≠0）
**解决**：检查托盘状态

### 问题4：拆跺箱码查询失败，提示"箱码与托盘不匹配"
**原因**：箱码与托盘不匹配或质检状态不一致
**解决**：检查箱码与托盘的关联关系

### 问题5：空托盘出库失败，提示"空载具库存不足"
**原因**：空托盘库存未正确创建
**解决**：检查WmsStockTray表中是否存在空托盘记录

### 问题6：盘点箱码查询失败，提示"盘点任务号不存在"
**原因**：盘点任务号不正确或任务已被删除
**解决**：检查WmsStockCheckNotify表中是否存在对应的盘点任务

---

## 附录

### A. 测试数据ID范围

| 数据类型 | ID范围 |
|---------|--------|
| 仓库 | 9100000000100001 |
| 巷道 | 9100000000300001 ~ 9100000000300002 |
| 物料 | 9100000000400001 ~ 9100000000400099 |
| 托盘码 | 9100000000500001 ~ 9100000000500009 |
| 入库单据单 | 9100000000600001 ~ 9100000000600002 |
| 入库单据单明细 | 9100000000600101 ~ 9100000000600102 |
| 出库通知单 | 9100000000800001 |
| 出库通知单明细 | 9100000000800101 |
| 出库单 | 9100000000900001 |
| 盘点通知单 | 9100000001000001 |
| 盘点通知单明细 | 9100000001000101 |
| 库位 | TEST_PDA-1 ~ TEST_PDA-22 |

### B. 状态码说明

| 表名 | 字段 | 状态值 | 说明 |
|-----|------|--------|------|
| WmsImportNotifyDetail | ImportExecuteFlag | 01 | 待执行 |
| WmsImportNotifyDetail | ImportExecuteFlag | 02 | 执行中 |
| WmsImportNotifyDetail | ImportExecuteFlag | 03 | 已完成 |
| WmsImportOrder | ImportExecuteFlag | 01 | 待执行 |
| WmsImportOrder | ImportExecuteFlag | 02 | 执行中 |
| WmsImportOrder | ImportExecuteFlag | 03 | 已完成 |
| WmsStockSlotBoxInfo | Status | 0 | 待入库 |
| WmsStockSlotBoxInfo | Status | 1 | 正在入库 |
| WmsStockSlotBoxInfo | Status | 2 | 已入库 |
| WmsStockSlotBoxInfo | Status | 3 | 已出库 |
| WmsSysStockCode | Status | 0 | 未使用 |
| WmsSysStockCode | Status | 1 | 已使用 |
| WmsBaseSlot | SlotStatus | 0 | 空储位 |
| WmsBaseSlot | SlotStatus | 1 | 有物品 |
| WmsBaseSlot | SlotStatus | 2 | 正在入库 |
| WmsBaseSlot | SlotStatus | 3 | 正在出库 |
| WmsBaseSlot | SlotStatus | 4 | 正在移入 |
| WmsBaseSlot | SlotStatus | 5 | 正在移出 |
| WmsBaseSlot | SlotStatus | 6 | 空托盘 |
| WmsExportOrder | ExportExecuteFlag | 0 | 待执行 |
| WmsExportOrder | ExportExecuteFlag | 1 | 已执行 |
| WmsStockCheckNotify | CheckStatus | 0 | 待盘点 |
| WmsStockCheckNotify | CheckStatus | 1 | 盘点中 |
| WmsStockCheckNotify | CheckStatus | 2 | 已完成 |

### C. 任务号前缀说明

| 前缀 | 说明 |
|-----|------|
| TEST_PDA_RK | 入库任务 |
| TEST_PDA_CK | 出库任务 |
| TEST_PDA_PD | 盘点任务 |

---

**文档版本**：v1.1
**最后更新**：2026-01-19
**作者**：Sisyphus
