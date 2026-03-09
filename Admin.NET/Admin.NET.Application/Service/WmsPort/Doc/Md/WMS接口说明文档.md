# WMS仓库管理系统API接口说明文档

## 目录
- [1. WCS接口 (WmsPortService)](#1-wcs接口-wmsportservice)
- [2. PDA入库接口 (WmsPdaService)](#2-pda入库接口-wmspdaservice)
- [3. PDA出库接口 (WmsPdaService)](#3-pda出库接口-wmspdaservice)
- [4. PDA盘点接口 (WmsPdaService)](#4-pda盘点接口-wmspdaservice)
- [5. 测试数据说明](#5-测试数据说明)
- [6. 测试步骤](#6-测试步骤)

---

## 1. WCS接口 (WmsPortService)

### 1.1 入库单据下发 (CreateOrder)
**接口路径**: `/api/WmsPort/CreateOrder`  
**请求方式**: `POST`  
**功能**: 将入库单据下发给WCS系统，生成入库任务

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsImportNotifyDetail | 读取 | 查询入库单据明细信息 |
| WmsImportNotify | 读取 | 查询入库单据主信息 |
| WmsBaseMaterial | 读取 | 查询物料基本信息 |
| WmsImportNotifyDetail | 更新 | 更新明细状态为"02正在执行" |
| WmsImportNotify | 更新 | 更新主单状态为"02正在执行" |

**测试数据**: 使用入库单据明细ID `1450345932570664961`

---

### 1.2 入库申请 (CreateImportOrder)
**接口路径**: `/api/WmsPort/CreateImportOrder`  
**请求方式**: `POST`  
**功能**: 申请托盘入库，系统自动分配库位并下发任务给WCS

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsSysStockCode | 读取 | 验证托盘是否存在 |
| WmsStockTray | 读取 | 查询托盘库存信息 |
| WmsBaseSlot | 读取/更新 | 查询可用库位并锁定 |
| WmsImportOrder | 插入 | 生成入库流水记录 |
| WmsSysStockCode | 更新 | 更新托盘状态为使用中 |

**测试数据**: 使用托盘 `TP2024001`

---

### 1.3 二次入库申请 (CreateImportOrder2)
**接口路径**: `/api/WmsPort/CreateImportOrder2`  
**请求方式**: `POST`  
**功能**: 指定具体库位的二次入库操作

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsSysStockCode | 读取 | 验证托盘 |
| WmsBaseSlot | 读取/更新 | 验证并锁定指定库位 |
| WmsImportOrder | 插入 | 生成入库流水 |
| WmsStockTray | 读取 | 查询托盘上的物料信息 |

**测试数据**: 使用托盘 `TP2024001` 和库位 `A-01-02-03`

---

### 1.4 任务反馈 (TaskFeedback)
**接口路径**: `/api/WmsPort/TaskFeedback`  
**请求方式**: `POST`  
**功能**: WCS任务完成后的反馈，更新任务状态和库位信息

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsImportOrder | 读取/更新 | 查询并更新入库流水状态 |
| WmsExportOrder | 读取/更新 | 查询并更新出库流水状态 |
| WmsBaseSlot | 更新 | 更新库位状态（释放锁定/占用） |
| WmsStockTray | 插入/更新 | 更新库存载具信息 |
| WmsImportNotifyDetail | 更新 | 更新入库明细完成数量 |
| WmsImportNotify | 更新 | 更新入库主单状态 |
| WmsStockSlotBoxInfo | 更新 | 更新箱码状态为"已入库" |

**任务类型说明**:
- `TaskType = "0"`: 入库任务
- `TaskType = "1"`: 出库任务
- `TaskType = "3"`: 移库任务

**状态码说明**:
- `Code = "1"`: 任务完成
- `Code = "2"`: 任务取消

**测试数据**: 使用任务号 `RK20240101001`

---

### 1.5 组托反馈 (GenerateGroupPallets)
**接口路径**: `/api/WmsPort/GenerateGroupPallets`  
**请求方式**: `POST`  
**功能**: 批量绑定箱码到托盘，完成组托操作

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsSysStockCode | 读取/插入/更新 | 验证或创建托盘 |
| WmsImportNotifyDetail | 读取 | 查询入库单据明细 |
| WmsImportNotify | 读取 | 查询入库单据主信息 |
| WmsBaseMaterial | 读取 | 查询物料信息 |
| WmsStockSlotBoxInfo | 插入/更新 | 批量插入或更新箱码明细 |
| WmsImportNotifyDetail | 更新 | 更新明细组盘数量和状态 |
| WmsImportNotify | 更新 | 更新主单执行状态 |

**测试数据**: 使用托盘 `TP2024001` 和入库单 `RK20240101`

---

### 1.6 空载具组盘 (KBackConfirm)
**接口路径**: `/api/WmsPort/KBackConfirm`  
**请求方式**: `POST`  
**功能**: 空托盘组盘或解绑操作

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsSysStockCode | 读取/插入 | 验证或创建托盘 |
| WmsImportOrder | 插入 | 生成空托盘入库流水 |
| WmsStockSlotBoxInfo | 插入/删除 | 绑定或解绑空托盘箱码 |
| WmsBaseMaterial | 读取 | 查询空托盘物料信息 |

**操作类型**:
- `ActionType = "add"`: 绑定操作
- `ActionType = "del"`: 解绑操作

**测试数据**: 使用托盘 `TP2024001`

---

### 1.7 空载具申请 (AddOutPalno)
**接口路径**: `/api/WmsPort/AddOutPalno`  
**请求方式**: `POST`  
**功能**: 申请空托盘出库，用于补充空托盘

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsStockTray | 读取 | 查询空托盘库存 |
| WmsBaseSlot | 读取/更新 | 查询空托盘所在库位并锁定 |
| WmsExportOrder | 插入 | 生成空托盘出库流水 |
| WmsStockTray | 更新 | 锁定空托盘库存数量 |
| WmsBaseMaterial | 读取 | 查询空托盘物料信息 |

**测试数据**: 申请5个空托盘到出库口 `CK01`

---

## 2. PDA入库接口 (WmsPdaService)

### 2.1 入库组托信息查询 (GetStockSoltBoxInfo)
**接口路径**: `/api/WmsPda/GetStockSoltBoxInfo`  
**请求方式**: `POST`  
**功能**: PDA查询托盘下的箱码信息，用于入库组托

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsStockSlotBoxInfo | 读取 | 查询箱码明细信息 |
| WmsImportNotifyDetail | 读取 | 查询入库单据明细 |
| WmsBaseMaterial | 读取 | 查询物料信息 |
| WmsInspectNotity | 读取 | 查询验收单信息（验收场景） |

**查询类型**:
- `Type = "1"`: 按托盘查询
- `Type != "1"`: 按物料条码查询

**测试数据**: 使用托盘 `TP2024001` 或物料条码 `1450345932570664961-1450000000000000001-LOT202401`

---

### 2.2 无箱码组托数量计算 (GetScanQty)
**接口路径**: `/api/WmsPda/GetScanQty`  
**请求方式**: `POST`  
**功能**: 无箱码场景下计算推荐绑定数量

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsImportNotifyDetail | 读取 | 查询入库单据明细 |
| WmsBaseMaterial | 读取 | 查询物料件数信息 |
| WmsStockSlotBoxInfo | 读取 | 查询已绑定的箱码数量 |

**计算逻辑**: 
```
推荐数量 = (计划数量 - 已组盘数量) / 物料件数
```

**测试数据**: 使用物料条码 `1450345932570664961-1450000000000000001-LOT202401`

---

### 2.3 无箱码组托保存 (SaveBoxInfo)
**接口路径**: `/api/WmsPda/SaveBoxInfo`  
**请求方式**: `POST`  
**功能**: 保存无箱码绑定信息到托盘

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsSysStockCode | 读取 | 验证托盘状态 |
| WmsImportNotifyDetail | 读取/更新 | 查询并更新明细组盘数量 |
| WmsImportNotify | 读取/更新 | 查询并更新主单状态 |
| WmsStockSlotBoxInfo | 插入 | 插入无箱码绑定记录 |
| WmsImportOrder | 插入/更新 | 生成或更新入库流水 |

**测试数据**: 使用托盘 `TP2024001` 绑定10.5袋血浆

---

### 2.4 托盘明细查询 (GetPalnoDetailInfo)
**接口路径**: `/api/WmsPda/GetPalnoDetailInfo`  
**请求方式**: `POST`  
**功能**: 查询托盘下的所有箱码明细

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsStockSlotBoxInfo | 读取 | 查询箱码明细 |
| WmsBaseMaterial | 读取 | 查询物料信息 |

**测试数据**: 使用托盘 `TP2024003`（已有5个箱码）

---

### 2.5 删除箱码绑定 (DelStockSoltBoxInfo)
**接口路径**: `/api/WmsPda/DelStockSoltBoxInfo`  
**请求方式**: `POST`  
**功能**: 删除托盘上的单个箱码

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsStockSlotBoxInfo | 删除/更新 | 软删除或物理删除箱码 |
| WmsImportNotifyDetail | 更新 | 回退明细组盘数量 |

**测试数据**: 使用箱码ID `1450345932570665010`

---

### 2.6 托盘箱码解绑 (RemoveBoxHolds)
**接口路径**: `/api/WmsPda/RemoveBoxHolds`  
**请求方式**: `POST`  
**功能**: 托盘上所有箱码解绑

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsStockSlotBoxInfo | 删除/更新 | 批量解绑箱码 |
| WmsImportNotifyDetail | 更新 | 回退明细组盘数量 |
| WmsImportOrder | 删除 | 删除对应的入库流水 |

**测试数据**: 使用托盘 `TP2024001`

---

### 2.7 扫码获取箱托信息 (GetImportInfoByBoxCode)
**接口路径**: `/api/WmsPda/GetImportInfoByBoxCode`  
**请求方式**: `POST`  
**功能**: 通过箱码扫描获取箱托信息

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsStockSlotBoxInfo | 读取 | 查询箱码信息 |
| WmsImportLabelPrint | 读取 | 查询标签打印信息 |
| WmsBaseMaterial | 读取 | 查询物料信息 |
| WmsImportNotifyDetail | 读取 | 查询入库单据明细 |

**测试数据**: 使用箱码 `BOX2024001`

---

### 2.8 托盘状态查询 (GetPalnoStatus)
**接口路径**: `/api/WmsPda/GetPalnoStatus`  
**请求方式**: `POST`  
**功能**: 查询托盘是否有效

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsSysStockCode | 读取 | 查询托盘是否存在 |

**测试数据**: 使用托盘 `TP2024001`

---

### 2.9 箱托绑定确认 (BackConfirm)
**接口路径**: `/api/WmsPda/BackConfirm`  
**请求方式**: `POST`  
**功能**: PDA确认箱托绑定关系

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsStockSlotBoxInfo | 插入/更新 | 插入或更新箱码绑定 |
| WmsImportOrder | 插入/更新 | 生成或更新入库流水 |
| WmsImportNotifyDetail | 更新 | 更新明细组盘数量 |
| WmsImportNotify | 更新 | 更新主单状态 |

**测试数据**: 使用箱码 `BOX2024001` 和托盘 `TP2024001`

---

### 2.10 暂存入库 (TemporaryStorage)
**接口路径**: `/api/WmsPda/TemporaryStorage`  
**请求方式**: `POST`  
**功能**: 为主/副载具生成暂存入库流水

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsExportOrder | 读取 | 查询最新出库流水 |
| WmsImportOrder | 插入 | 生成暂存入库流水 |
| WmsStockSlotBoxInfo | 更新 | 回写箱码入库流水关联 |

**测试数据**: 使用主载具 `TP2024001` 和副载具 `TP2024002`

---

### 2.11 叠箱绑定 (StackingBindings)
**接口路径**: `/api/WmsPda/StackingBindings`  
**请求方式**: `POST`  
**功能**: 校验并绑定主/副载具叠箱关系

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsImportOrder | 读取/更新 | 查询最新入库流水并更新主载具编号 |
| WmsStockSlotBoxInfo | 更新 | 更新箱码主载具关联 |

**测试数据**: 使用主载具 `TP2024001` 和副载具 `TP2024002`

---

## 3. PDA出库接口 (WmsPdaService)

### 3.1 拆跺箱码查询 (GetManBoxInfo)
**接口路径**: `/api/WmsPda/GetManBoxInfo`  
**请求方式**: `POST`  
**功能**: 查询拆跺箱码信息

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsStockSlotBoxInfo | 读取 | 查询箱码库存信息 |
| WmsExportOrder | 读取 | 查询出库流水信息 |
| WmsStockTray | 读取 | 查询托盘库存信息 |
| WmsBaseMaterial | 读取 | 查询物料信息 |

**测试数据**: 使用箱码 `BOX2024001` 和出库流水 `CK20240101001`

---

### 3.2 托盘出库单查询 (GetExprotCodeById)
**接口路径**: `/api/WmsPda/GetExprotCodeById`  
**请求方式**: `POST`  
**功能**: 查询托盘关联的出库单据

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsExportOrder | 读取 | 查询出库流水 |
| WmsStockTray | 读取 | 查询托盘信息 |

**测试数据**: 使用托盘 `TP2024003`

---

### 3.3 拆跺箱码绑定 (AddManualDepalletizing)
**接口路径**: `/api/WmsPda/AddManualDepalletizing`  
**请求方式**: `POST`  
**功能**: 添加拆跺箱码绑定记录

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| 临时拆跺表 | 插入 | 插入拆跺记录（具体表名需确认） |
| WmsStockSlotBoxInfo | 读取 | 查询箱码库存 |
| WmsExportOrder | 读取 | 查询出库任务 |

**测试数据**: 使用箱码 `BOX2024001`，拆跺数量 `5.0`

---

### 3.4 人工拆垛出库确认 (SaveManTask)
**接口路径**: `/api/WmsPda/SaveManTask`  
**请求方式**: `POST`  
**功能**: 人工拆垛出库确认，更新库存和出库状态

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| 临时拆跺表 | 读取/删除 | 读取并清理拆跺记录 |
| WmsStockSlotBoxInfo | 更新 | 扣减箱码库存数量 |
| WmsExportBoxInfo | 插入/更新 | 更新出库箱码明细 |
| WmsExportOrder | 更新 | 更新出库流水状态 |
| WmsStockTray | 更新 | 更新托盘库存数量 |

**测试数据**: 使用托盘 `TP2024003` 和出库流水 `CK20240101001`

---

### 3.5 空托盘出库申请 (AddOutPalno)
**接口路径**: `/api/WmsPda/AddOutPalno`  
**请求方式**: `POST`  
**功能**: PDA端申请空托盘出库

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsStockTray | 读取 | 查询空托盘库存 |
| WmsBaseSlot | 读取/更新 | 查询并锁定库位 |
| WmsExportOrder | 插入 | 生成出库流水 |
| WmsBaseMaterial | 读取 | 查询空托盘物料信息 |

**测试数据**: 申请5个空托盘到出库口 `CK01`

---

### 3.6 出库口列表查询 (GetExportPortList)
**接口路径**: `/api/WmsPda/GetExportPortList`  
**请求方式**: `POST`  
**功能**: 查询系统配置的出库口列表

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| 出库口配置表 | 读取 | 查询出库口信息（具体表名需确认） |

---

## 4. PDA盘点接口 (WmsPdaService)

### 4.1 盘点单列表查询 (GetWmsStockCheckList)
**接口路径**: `/api/WmsPda/GetWmsStockCheckList`  
**请求方式**: `POST`  
**功能**: 查询盘点单列表

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| 盘点单主表 | 读取 | 查询盘点单信息（具体表名需确认） |

**测试数据**: 查询仓库ID为 `1` 的盘点单

---

### 4.2 盘点箱码查询 (GetStockCheckInfo)
**接口路径**: `/api/WmsPda/GetStockCheckInfo`  
**请求方式**: `POST`  
**功能**: 查询盘点单的箱码明细

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| 盘点明细表 | 读取 | 查询盘点箱码明细（具体表名需确认） |
| WmsStockSlotBoxInfo | 读取 | 查询箱码库存信息 |
| WmsBaseMaterial | 读取 | 查询物料信息 |

**测试数据**: 使用盘点单号 `PD20240101` 和托盘 `TP2024003`

---

### 4.3 盘点结果提交 (UpdateStockCheckInfo)
**接口路径**: `/api/WmsPda/UpdateStockCheckInfo`  
**请求方式**: `POST`  
**功能**: 提交盘点结果，更新盘点状态

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| 盘点明细表 | 更新 | 更新盘点实际数量 |
| 盘点单主表 | 更新 | 更新盘点单状态 |
| WmsStockTray | 更新 | 根据盘点结果调整库存（可选） |

**测试数据**: 提交托盘 `TP2024003` 的盘点结果

---

### 4.4 根据托盘号获取物料信息 (GetMaterialInfoByStockCode)
**接口路径**: `/api/WmsPda/GetMaterialInfoByStockCode`  
**请求方式**: `POST`  
**功能**: 根据托盘号获取托盘上的物料信息

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsStockTray | 读取 | 查询托盘库存 |
| WmsBaseMaterial | 读取 | 查询物料信息 |
| WmsStockSlotBoxInfo | 读取 | 查询箱码信息（有箱码场景） |

**测试数据**: 使用托盘 `TP2024003`

---

### 4.5 无箱码托盘变更 (SaveUnbindWithNoBoxCode)
**接口路径**: `/api/WmsPda/SaveUnbindWithNoBoxCode`  
**请求方式**: `POST`  
**功能**: 无箱码托盘变更，将库存从原托盘转移到新托盘

**涉及的数据库表**:
| 表名 | 操作类型 | 说明 |
|------|---------|------|
| WmsStockTray | 读取/更新 | 查询原托盘并扣减库存 |
| WmsStockTray | 插入/更新 | 新托盘增加库存 |
| WmsExportOrder | 插入 | 生成移库流水（可选） |

**测试数据**: 从托盘 `TP2024001` 转移5.0袋到托盘 `TP2024003`

---

## 5. 测试数据说明

### 5.1 基础数据
- **仓库**: 3个（血浆立体库A、原料立体库B、成品平库C）
- **储位**: 9个（包含入库口和出库口）
- **物料**: 6个（4种血浆类型 + 2种空托盘）
- **托盘**: 10个（TP2024001~TP2024010）

### 5.2 业务数据
- **入库单据**: 3条
  - RK20240101: 待执行
  - RK20240102: 待执行
  - RK20240103: 执行中
- **入库明细**: 4条
- **入库流水**: 1条（已完成）
- **库存箱码**: 5条（托盘TP2024003上）
- **库存载具**: 1条（托盘TP2024003在库位A-01-01-01）
- **出库流水**: 1条（待执行）
- **出库明细**: 3条

### 5.3 ID规则说明
```
基础表ID: 1450000000000000xxx (xxx从001开始)
业务表ID: 145034593257066xxxx (xxxx从4960开始)
托盘ID:   145000000000000xxxx (xxxx从1001开始)
```

---

## 6. 测试步骤

### 6.1 完整入库流程测试

#### 步骤1: 入库单据下发
```
POST /api/WmsPort/CreateOrder
{
  "Id": 1450345932570664961,
  "ReceivingDock": "1",
  "NetWeight": "50.5"
}
```
**预期结果**: 返回成功，单据状态变为"02正在执行"

#### 步骤2: 入库申请
```
POST /api/WmsPort/CreateImportOrder
{
  "HouseCode": "A",
  "StockCode": "TP2024001",
  "LaneWayCode": "LD01",
  "Location": "RK01",
  "WarehouseName": "血浆立体库",
  "Type": "3"
}
```
**预期结果**: 返回任务号和分配的库位

#### 步骤3: 组托反馈
```
POST /api/WmsPort/GenerateGroupPallets
{
  "VehicleCode": "TP2024001",
  "BillCode": "RK20240101",
  "List": [...]
}
```
**预期结果**: 箱码成功绑定到托盘

#### 步骤4: 任务反馈
```
POST /api/WmsPort/TaskFeedback
{
  "TaskNo": "RK20240101001",
  "Code": "1",
  "TaskType": "0",
  "StockCode": "TP2024001",
  "TaskEnd": "A-01-02-03"
}
```
**预期结果**: 库位状态更新，库存生成

---

### 6.2 PDA入库流程测试

#### 步骤1: 查询托盘信息
```
POST /api/WmsPda/GetPalnoStatus
{
  "PalletCode": "TP2024001"
}
```
**预期结果**: 返回托盘有效

#### 步骤2: 扫码获取箱码信息
```
POST /api/WmsPda/GetImportInfoByBoxCode
{
  "BoxCode": "BOX2024001",
  "PalletCode": "TP2024001",
  "ImportId": "1450345932570664960"
}
```
**预期结果**: 返回箱码详细信息

#### 步骤3: 箱托绑定确认
```
POST /api/WmsPda/BackConfirm
{
  "BoxCode": "BOX2024001",
  "PalletCode": "TP2024001",
  "ImportId": "1450345932570664960",
  "Weight": 0.666
}
```
**预期结果**: 绑定成功

---

### 6.3 出库流程测试

#### 步骤1: 查询托盘出库单
```
POST /api/WmsPda/GetExprotCodeById
{
  "StockCode": "TP2024003",
  "IsBoxCode": "0"
}
```
**预期结果**: 返回出库单列表

#### 步骤2: 查询拆跺箱码信息
```
POST /api/WmsPda/GetManBoxInfo
{
  "BoxCode": "BOX2024001",
  "TrayCode": "TP2024003",
  "ExportOrderNo": "CK20240101001"
}
```
**预期结果**: 返回箱码库存信息

#### 步骤3: 拆跺箱码绑定
```
POST /api/WmsPda/AddManualDepalletizing
{
  "BoxCode": "BOX2024001",
  "ScanQty": 5.0,
  "TrayCode": "TP2024003",
  "ExportOrderNo": "CK20240101001"
}
```
**预期结果**: 拆跺记录创建成功

#### 步骤4: 人工拆垛出库确认
```
POST /api/WmsPda/SaveManTask
{
  "TrayCode": "TP2024003",
  "ExportOrderNo": "CK20240101001"
}
```
**预期结果**: 库存扣减，出库完成

---

## 7. 常见问题排查

### 7.1 入库问题
- **托盘不存在**: 检查 WmsSysStockCode 表
- **库位不足**: 检查 WmsBaseSlot 表的可用库位
- **物料不存在**: 检查 WmsBaseMaterial 表
- **单据状态不对**: 检查 WmsImportNotify 和 WmsImportNotifyDetail 的状态字段

### 7.2 出库问题
- **库存不足**: 检查 WmsStockTray 表的库存数量
- **托盘锁定**: 检查 WmsStockTray 的 LockQuantity 字段
- **出库任务不存在**: 检查 WmsExportOrder 表

### 7.3 数据一致性检查
```sql
-- 检查托盘上的箱码总数量与库存是否一致
SELECT 
    st.StockCode,
    st.StockQuantity AS 库存数量,
    SUM(ssbi.Qty) AS 箱码总数量
FROM WmsStockTray st
LEFT JOIN WmsStockSlotBoxInfo ssbi ON st.StockCode = ssbi.StockCode AND ssbi.IsDelete = 0
WHERE st.IsDelete = 0
GROUP BY st.StockCode, st.StockQuantity
HAVING st.StockQuantity != ISNULL(SUM(ssbi.Qty), 0);
```

---

## 8. 附录

### 8.1 状态码说明

#### 执行标志 (ImportExecuteFlag / ExportExecuteFlag)
- `-1`: 无效
- `01`: 待执行
- `02`: 正在执行
- `03`: 已完成
- `04`: 已上传

#### 库位状态 (SlotStatus)
- `0`: 空储位
- `1`: 有物品
- `2`: 正在入库
- `3`: 正在出库
- `4`: 正在移入
- `5`: 正在移出
- `6`: 空托盘
- `7`: 屏蔽
- `8`: 储位不存在
- `9`: 异常储位

#### 质检状态 (InspectionStatus)
- `0`: 待检
- `1`: 合格
- `2`: 不合格

#### 托盘状态 (Status)
- `0`: 未使用
- `1`: 正在使用

### 8.2 重要字段说明

#### 物料条码格式
```
格式: {明细ID}-{物料ID}-{批次}-[可选:流水ID]
示例: 1450345932570664961-1450000000000000001-LOT202401
```

#### 任务号前缀
- `RK`: 入库任务
- `CK`: 出库任务
- `YK`: 移库任务
- `PK`: 盘库任务

---

**文档版本**: v1.0  
**生成日期**: 2024-01-01  
**维护者**: WMS开发团队

