# PDA业务接口分析报告

## 文档说明

本文档详细分析WmsPdaService类下的所有PDA业务接口，包括接口功能、输入输出参数、业务流程、涉及的数据库表以及表之间的关联关系。

---

## 一、接口概览

WmsPdaService类包含30个接口，分为4大类：

### 1.1 入库相关接口（13个）

| 序号 | 接口名称 | 接口路径 | 功能描述 |
|-----|---------|---------|---------|
| 1 | GetStockSoltBoxInfo | /api/WmsPda/GetStockSoltBoxInfo | 入库组托信息查询 |
| 2 | GetScanQty | /api/WmsPda/GetScanQty | 无箱码组托数量计算 |
| 3 | SaveBoxInfo | /api/WmsPda/SaveBoxInfo | 无箱码保存载具绑定 |
| 4 | GetPalnoDetailInfo | /api/WmsPda/GetPalnoDetailInfo | 托盘明细查询 |
| 5 | DelStockSoltBoxInfo | /api/WmsPda/DelStockSoltBoxInfo | 删除单个箱码绑定 |
| 6 | RemoveBoxHolds | /api/WmsPda/RemoveBoxHolds | 托盘箱码解绑 |
| 7 | GetImportInfoByBoxCode | /api/WmsPda/GetImportInfoByBoxCode | 扫码获取箱托信息 |
| 8 | GetPalnoStatus | /api/WmsPda/GetPalnoStatus | 托盘状态查询 |
| 9 | BackConfirm | /api/WmsPda/BackConfirm | 绑定箱托关系 |
| 10 | KBackConfirm | /api/WmsPda/KBackConfirm | 空托盘组盘 |
| 11 | AddWmsImportOrder | /api/WmsPda/AddWmsImportOrder | 批量生成空托入库数据 |
| 12 | TemporaryStorage | /api/WmsPda/TemporaryStorage | 暂存入库生成 |
| 13 | StackingBindings | /api/WmsPda/StackingBindings | 叠箱绑定 |

### 1.2 出库相关接口（13个）

| 序号 | 接口名称 | 接口路径 | 功能描述 |
|-----|---------|---------|---------|
| 14 | RemoveManual | /api/WmsPda/RemoveManual | 删除拆跺信息 |
| 15 | GetManBoxInfo | /api/WmsPda/GetManBoxInfo | 拆跺箱码信息查询 |
| 16 | GetExprotCodeById | /api/WmsPda/GetExprotCodeById | 托盘关联出库单查询 |
| 17 | GetTrayInfo | /api/WmsPda/GetTrayInfo | 拆跺信息查询 |
| 18 | GetBoxSum | /api/WmsPda/GetBoxSum | 拆跺数量计算 |
| 19 | AddManualDepalletizing | /api/WmsPda/AddManualDepalletizing | 拆跺箱码绑定 |
| 20 | GetManualDepalletizing | /api/WmsPda/GetManualDepalletizing | 拆跺记录查询 |
| 21 | SaveManTask | /api/WmsPda/SaveManTask | 人工拆垛出库确认 |
| 22 | AddOutManualDepalletizing | /api/WmsPda/AddOutManualDepalletizing | 无箱码拆跺绑定 |
| 23 | RemoveOutManual | /api/WmsPda/RemoveOutManual | 无箱码拆跺删除 |
| 24 | GetOutManualDepalletizing | /api/WmsPda/GetOutManualDepalletizing | 无箱码拆跺查询 |
| 25 | OutSaveManTask | /api/WmsPda/OutSaveManTask | 无箱码人工拆垛出库确认 |
| 26 | AddOutPalno | /api/WmsPda/AddOutPalno | 空托盘出库 |
| 27 | GetExportPortList | /api/WmsPda/GetExportPortList | 出库口列表查询 |

### 1.3 盘点相关接口（6个）

| 序号 | 接口名称 | 接口路径 | 功能描述 |
|-----|---------|---------|---------|
| 28 | GetWmsStockCheckList | /api/WmsPda/GetWmsStockCheckList | 盘点单列表查询 |
| 29 | GetStockCheckInfo | /api/WmsPda/GetStockCheckInfo | 盘点箱码列表查询 |
| 30 | UpdateStockCheckInfo | /api/WmsPda/UpdateStockCheckInfo | 盘点结果提交 |
| 31 | IsEnableOkStockCode | /api/WmsPda/IsEnableOkStockCode | 托盘验证 |
| 32 | GetMaterialInfoByStockCode | /api/WmsPda/GetMaterialInfoByStockCode | 根据托盘号获取物料信息 |
| 33 | SaveUnbindWithNoBoxCode | /api/WmsPda/SaveUnbindWithNoBoxCode | 无箱码托盘变更 |

### 1.4 音频通知接口（2个）

| 序号 | 接口名称 | 接口路径 | 功能描述 |
|-----|---------|---------|---------|
| 34 | GenerateStorageAudio | /api/WmsPda/GenerateStorageAudio | 生成货位操作音频通知 |
| 35 | GenerateTextAudio | /api/WmsPda/GenerateTextAudio | 生成文本转音频 |

---

## 二、接口详细说明

### 2.1 入库相关接口

#### 1. GetStockSoltBoxInfo - 入库组托信息查询

**功能**：查询入库组托信息，支持按物料条码或托盘查询

**输入参数**：
- `Type`: 查询类型（1=按托盘查询，其他=按物料条码查询）
- `MaterialBarcode`: 物料条码（type≠1时必填）
- `ImportBillCode`: 入库单号（type≠1时必填）
- `StockCode`: 托盘编码（type=1时使用）

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Items`: 组托明细列表

**业务流程**：
1. 验证输入参数
2. 根据查询类型选择查询策略
3. type=1：按托盘号检索，读取储位箱码信息
4. type≠1：按物料条码拆分明细、物料、批次、导入流水ID
5. 过滤条件：ImportExecuteFlag == 01/02（待执行/执行中）

**涉及的表**：
- WmsImportNotifyDetail（入库单据单明细）
- WmsImportOrder（入库单）
- WmsStockSlotBoxInfo（库存箱托信息）
- WmsBaseMaterial（物料主数据）

---

#### 2. GetScanQty - 无箱码组托数量计算

**功能**：计算无箱码绑定推荐数量

**输入参数**：
- `MaterialBarcode`: 物料条码（格式：明细ID-物料ID-批次-流水ID）
- `StockCode`: 托盘编码
- `ImportBillCode`: 入库单号

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Qty`: 推荐数量

**业务流程**：
1. 解析物料条码格式"明细ID-物料ID-批次-(可选)流水ID"
2. 校验目标托盘尚未绑定（Status==0）
3. 计算剩余数量 = 计划入库 - 已组托
4. 推荐值取 min(剩余, EveryNumber)

**涉及的表**：
- WmsImportNotifyDetail（入库单据单明细）
- WmsBaseMaterial（物料主数据）
- WmsStockSlotBoxInfo（库存箱托信息）
- WmsSysStockCode（托盘码主数据）

---

#### 3. SaveBoxInfo - 无箱码保存载具绑定

**功能**：保存无箱码载具绑定信息

**输入参数**：
- `StockCode`: 托盘编码
- `ImportBillCode`: 入库单号
- `MaterialBarcode`: 物料条码
- `Qty`: 绑定数量

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息

**业务流程**：
1. 校验托盘可用或人工码垛场景准入
2. 写入入库流水
3. 更新托盘状态与箱码明细
4. 同步入库单明细与主单执行状态

**涉及的表**：
- WmsImportOrder（入库单）
- WmsStockSlotBoxInfo（库存箱托信息）
- WmsSysStockCode（托盘码主数据）
- WmsImportNotifyDetail（入库单据单明细）
- WmsImportNotify（入库单据单）

---

#### 4. GetPalnoDetailInfo - 托盘明细查询

**功能**：查询托盘下箱码聚合结果

**输入参数**：
- `StockCode`: 托盘编码

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Items`: 托盘明细列表

**业务流程**：
1. 根据托盘编码查询箱码信息
2. 按物料、批次分组聚合
3. 返回聚合结果

**涉及的表**：
- WmsStockSlotBoxInfo（库存箱托信息）
- WmsImportOrder（入库单）
- WmsBaseMaterial（物料主数据）

---

#### 5. DelStockSoltBoxInfo - 删除单个箱码绑定

**功能**：删除单个箱码绑定

**输入参数**：
- `Id`: 箱码ID
- `BoxCode`: 箱码
- `StockCode`: 托盘编码

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息

**业务流程**：
1. 验证输入参数
2. 查找待删除的箱码记录
3. 软删除箱码记录
4. 更新入库单明细状态

**涉及的表**：
- WmsStockSlotBoxInfo（库存箱托信息）
- WmsImportOrder（入库单）
- WmsImportNotifyDetail（入库单据单明细）

---

#### 6. RemoveBoxHolds - 托盘箱码解绑

**功能**：托盘箱码解绑

**输入参数**：
- `StockCode`: 托盘编码
- `ImportBillCode`: 入库单号

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息

**业务流程**：
1. 验证托盘编码和入库单号
2. 查询该托盘下的所有箱码
3. 软删除所有箱码记录
4. 恢复托盘状态为未使用

**涉及的表**：
- WmsStockSlotBoxInfo（库存箱托信息）
- WmsImportOrder（入库单）
- WmsSysStockCode（托盘码主数据）

---

#### 7. GetImportInfoByBoxCode - 扫码获取箱托信息

**功能**：扫码获取箱托信息及处理状态

**输入参数**：
- `BoxCode`: 箱码

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `BoxInfo`: 箱托信息

**业务流程**：
1. 根据箱码查询库存箱托信息
2. 返回箱码对应的托盘、物料、批次等信息
3. 判断箱码状态（待入库/已入库/已出库）

**涉及的表**：
- WmsStockSlotBoxInfo（库存箱托信息）
- WmsImportOrder（入库单）
- WmsBaseMaterial（物料主数据）
- WmsSysStockCode（托盘码主数据）

---

#### 8. GetPalnoStatus - 托盘状态查询

**功能**：查询托盘状态

**输入参数**：
- `PalletCode`: 托盘编码

**输出参数**：
- `Valid`: 是否有效
- `Message`: 状态描述

**业务流程**：
1. 验证托盘编码
2. 查询托盘状态
3. 返回托盘是否可用

**涉及的表**：
- WmsSysStockCode（托盘码主数据）

---

#### 9. BackConfirm - 绑定箱托关系

**功能**：在PDA端确认箱托绑定，生成/更新入库流水

**输入参数**：
- `StockCode`: 托盘编码
- `ImportBillCode`: 入库单号
- `BoxCode`: 箱码
- `Qty`: 数量

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息

**业务流程**：
1. 验证输入参数
2. 生成或更新入库流水
3. 绑定箱码到托盘
4. 更新入库单明细执行状态

**涉及的表**：
- WmsImportOrder（入库单）
- WmsStockSlotBoxInfo（库存箱托信息）
- WmsImportNotifyDetail（入库单据单明细）
- WmsSysStockCode（托盘码主数据）

---

#### 10. KBackConfirm - 空托盘组盘

**功能**：空托盘组盘（绑定/解绑）

**输入参数**：
- `PalletCode`: 托盘编码
- `Quantity`: 数量
- `ActionType`: 操作类型（add=绑定，del=解绑）

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息

**业务流程**：
- **绑定操作**：
  1. 验证托盘状态为未使用
  2. 创建入库流水记录
  3. 创建箱码信息记录
  4. 更新托盘状态为已使用

- **解绑操作**：
  1. 验证托盘已绑定且未分配库位
  2. 软删除箱码信息
  3. 软删除入库流水
  4. 更新托盘状态为未使用

**涉及的表**：
- WmsSysStockCode（托盘码主数据）
- WmsImportOrder（入库单）
- WmsStockSlotBoxInfo（库存箱托信息）
- WmsBaseMaterial（物料主数据）
- WmsBaseWareHouse（仓库）

---

#### 11. AddWmsImportOrder - 批量生成空托入库数据

**功能**：遍历闲置托盘（尾号为奇数），为每个托盘生成空托流水与箱码

**输入参数**：无

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息

**业务流程**：
1. 查询所有闲置托盘（尾号为奇数）
2. 为每个托盘生成空托流水
3. 为每个托盘生成箱码记录
4. 更新托盘状态

**涉及的表**：
- WmsSysStockCode（托盘码主数据）
- WmsImportOrder（入库单）
- WmsStockSlotBoxInfo（库存箱托信息）

---

#### 12. TemporaryStorage - 暂存入库生成

**功能**：基于最新出库流水，为主/副载具生成暂存入库流水

**输入参数**：
- `MainTrayCode`: 主载具编码
- `SubTrayCode`: 副载具编码

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息

**业务流程**：
1. 查询最新出库流水
2. 生成暂存入库流水
3. 回写箱码关联

**涉及的表**：
- WmsExportOrder（出库单）
- WmsImportOrder（入库单）
- WmsStockSlotBoxInfo（库存箱托信息）

---

#### 13. StackingBindings - 叠箱绑定

**功能**：校验主/副载具最新入库流水是否同业务，并统一写入主载具编号

**输入参数**：
- `MainTrayCode`: 主载具编码
- `SubTrayCode`: 副载具编码

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息

**业务流程**：
1. 查询主/副载具最新入库流水
2. 校验是否同业务
3. 统一写入主载具编号

**涉及的表**：
- WmsImportOrder（入库单）
- WmsStockSlotBoxInfo（库存箱托信息）
- WmsSysStockCode（托盘码主数据）

---

### 2.2 出库相关接口

#### 14. RemoveManual - 删除拆跺信息

**功能**：删除拆跺信息数据

**输入参数**：
- `Id`: 拆跺记录ID
- `BoxCode`: 箱码
- `TrayCode`: 托盘编码
- `ExportOrderNo`: 出库流水号

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息

**业务流程**：
1. 验证输入参数
2. 查找待删除的拆跺记录
3. 软删除记录

**涉及的表**：
- WmsManualDepalletizing（人工拆跺）

---

#### 15. GetManBoxInfo - 拆跺箱码信息查询

**功能**：查询拆跺箱码信息

**输入参数**：
- `BoxCode`: 箱码
- `TrayCode`: 托盘编码
- `ExportOrderNo`: 出库流水号

**输出参数**：
- `Code`: 状态码（0=失败，1=成功）
- `Message`: 提示信息
- `BoxInfo`: 箱码信息
- `Status`: 状态
- `InspectionStatus`: 质检状态

**业务流程**：
1. 验证输入参数
2. 根据箱码查询拆跺视图数据
3. 验证托盘与箱码的匹配关系
4. 检查托盘与订单的质检状态是否一致

**涉及的表**：
- WmsManualDepalletizing（人工拆跺）
- WmsStockTray（库存托盘）
- WmsExportOrder（出库单）

---

#### 16. GetExprotCodeById - 托盘关联出库单查询

**功能**：根据托盘编号查询对应的出库单号

**输入参数**：
- `StockCode`: 托盘编码
- `IsBoxCode`: 是否箱码标识

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Data`: 出库单信息列表

**业务流程**：
1. 根据托盘编码查询拆跺视图数据
2. 筛选出已执行完成的出库流水
3. 按出库单号分组并去重
4. 如果指定了IsBoxCode参数，过滤掉有箱码的托盘

**涉及的表**：
- WmsManualDepalletizing（人工拆跺）
- WmsStockInfo（库存明细）
- WmsExportOrder（出库单）

---

#### 17. GetTrayInfo - 拆跺信息查询

**功能**：查询拆跺信息

**输入参数**：
- `TrayCode`: 托盘编码
- `ExportOrderNo`: 出库流水号

**输出参数**：
- `Code`: 状态码
- `Message`: 提示信息
- `BoxInfo`: 箱码信息列表

**业务流程**：
1. 根据托盘编码和出库流水号查询拆跺信息
2. 返回该托盘下的所有箱码信息

**涉及的表**：
- WmsManualDepalletizing（人工拆跺）
- WmsStockTray（库存托盘）

---

#### 18. GetBoxSum - 拆跺数量计算

**功能**：计算拆跺数量

**输入参数**：
- `BoxCodeList`: 箱码列表
- `ExportOrderNo`: 出库流水号

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Data`: 数量计算结果

**业务流程**：
1. 根据箱码列表查询拆跺信息
2. 计算总数量
3. 返回计算结果

**涉及的表**：
- WmsManualDepalletizing（人工拆跺）

---

#### 19. AddManualDepalletizing - 拆跺箱码绑定

**功能**：绑定拆跺箱码

**输入参数**：
- `BoxCode`: 箱码
- `TrayCode`: 托盘编码
- `ExportOrderNo`: 出库流水号
- `Qty`: 数量

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息

**业务流程**：
1. 验证输入参数
2. 创建拆跺记录
3. 更新库存状态

**涉及的表**：
- WmsManualDepalletizing（人工拆跺）
- WmsStockInfo（库存明细）
- WmsStockTray（库存托盘）

---

#### 20. GetManualDepalletizing - 拆跺记录查询

**功能**：查询拆跺记录

**输入参数**：
- `TrayCode`: 托盘编码
- `ExportOrderNo`: 出库流水号

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Data`: 拆跺记录列表

**业务流程**：
1. 根据托盘编码和出库流水号查询拆跺记录
2. 返回拆跺记录列表

**涉及的表**：
- WmsManualDepalletizing（人工拆跺）

---

#### 21. SaveManTask - 人工拆垛出库确认

**功能**：人工拆垛出库确认

**输入参数**：
- `TrayCode`: 托盘编码
- `ExportOrderNo`: 出库流水号
- `BoxCodeList`: 箱码列表

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Data`: 出库结果

**业务流程**：
1. 验证输入参数
2. 扣减库存
3. 释放托盘
4. 更新任务状态

**涉及的表**：
- WmsManualDepalletizing（人工拆跺）
- WmsStockInfo（库存明细）
- WmsStockTray（库存托盘）
- WmsStock（库存汇总）
- WmsExportTask（出库任务）

---

#### 22. AddOutManualDepalletizing - 无箱码拆跺绑定

**功能**：无箱码拆跺绑定

**输入参数**：
- `TrayCode`: 托盘编码
- `ExportOrderNo`: 出库流水号
- `Qty`: 数量

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息

**业务流程**：
1. 验证输入参数
2. 创建无箱码拆跺记录
3. 更新库存状态

**涉及的表**：
- WmsManualDepalletizing（人工拆跺）
- WmsStockTray（库存托盘）

---

#### 23. RemoveOutManual - 无箱码拆跺删除

**功能**：删除无箱码拆跺

**输入参数**：
- `Id`: 拆跺记录ID
- `TrayCode`: 托盘编码
- `ExportOrderNo`: 出库流水号

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息

**业务流程**：
1. 验证输入参数
2. 查找待删除的拆跺记录
3. 软删除记录

**涉及的表**：
- WmsManualDepalletizing（人工拆跺）

---

#### 24. GetOutManualDepalletizing - 无箱码拆跺查询

**功能**：查询无箱码拆跺记录

**输入参数**：
- `TrayCode`: 托盘编码
- `ExportOrderNo`: 出库流水号

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Data`: 拆跺记录列表

**业务流程**：
1. 根据托盘编码和出库流水号查询拆跺记录
2. 返回拆跺记录列表

**涉及的表**：
- WmsManualDepalletizing（人工拆跺）

---

#### 25. OutSaveManTask - 无箱码人工拆垛出库确认

**功能**：无箱码人工拆垛出库确认

**输入参数**：
- `TrayCode`: 托盘编码
- `ExportOrderNo`: 出库流水号
- `Qty`: 数量

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Data`: 出库结果

**业务流程**：
1. 验证输入参数
2. 扣减库存
3. 释放托盘
4. 更新任务状态

**涉及的表**：
- WmsManualDepalletizing（人工拆跺）
- WmsStockTray（库存托盘）
- WmsStock（库存汇总）
- WmsExportTask（出库任务）

---

#### 26. AddOutPalno - 空托盘出库

**功能**：空托盘出库申请

**输入参数**：
- `Num`: 申请数量
- `ExportPort`: 出库口
- `HouseCode`: 仓库类型编码

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Data`: 出库结果

**业务流程**：
1. 验证仓库和物料配置
2. 筛选空托盘候选集合
3. 处理深度2库位的移库需求
4. 创建出库任务并更新库位状态

**涉及的表**：
- WmsBaseWareHouse（仓库）
- WmsBaseMaterial（物料主数据）
- WmsStockTray（库存托盘）
- WmsBaseSlot（基础库位）
- WmsExportTask（出库任务）
- WmsStockInfo（库存明细）

---

#### 27. GetExportPortList - 出库口列表查询

**功能**：查询出库口列表

**输入参数**：无

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Data`: 出库口列表

**业务流程**：
1. 查询所有出库口
2. 返回出库口列表

**涉及的表**：
- WmsBaseSlot（基础库位）

---

### 2.3 盘点相关接口

#### 28. GetWmsStockCheckList - 盘点单列表查询

**功能**：查询盘点单列表

**输入参数**：
- `Status`: 状态筛选

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Data`: 盘点单列表

**业务流程**：
1. 查询盘点通知单
2. 根据状态筛选
3. 返回盘点单列表

**涉及的表**：
- WmsStockCheckNotify（盘点通知）
- WmsStockCheckNotifyDetail（盘点通知明细）

---

#### 29. GetStockCheckInfo - 盘点箱码列表查询

**功能**：查询盘点箱码列表

**输入参数**：
- `CheckTaskNo`: 盘点任务号

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Data`: 盘点箱码列表

**业务流程**：
1. 根据盘点任务号查询盘点明细
2. 返回盘点箱码列表

**涉及的表**：
- WmsStockCheckNotifyDetail（盘点通知明细）
- WmsStockCheckInfo（盘点箱码信息）

---

#### 30. UpdateStockCheckInfo - 盘点结果提交

**功能**：提交盘点结果

**输入参数**：
- `CheckTaskNo`: 盘点任务号
- `BoxCode`: 箱码
- `ActualQty`: 实际数量
- `Remark`: 备注

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息

**业务流程**：
1. 验证输入参数
2. 更新盘点箱码信息
3. 更新盘点任务状态

**涉及的表**：
- WmsStockCheckInfo（盘点箱码信息）
- WmsStockCheckNotifyDetail（盘点通知明细）

---

#### 31. IsEnableOkStockCode - 托盘验证

**功能**：验证托盘是否可用

**输入参数**：
- `StockCode`: 托盘编码

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Data`: 验证结果

**业务流程**：
1. 验证托盘编码
2. 查询托盘状态
3. 返回验证结果

**涉及的表**：
- WmsSysStockCode（托盘码主数据）
- WmsStockTray（库存托盘）

---

#### 32. GetMaterialInfoByStockCode - 根据托盘号获取物料信息

**功能**：根据托盘号获取物料信息

**输入参数**：
- `StockCode`: 托盘编码

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Data`: 物料信息列表

**业务流程**：
1. 根据托盘编码查询库存托盘
2. 返回托盘下的物料信息

**涉及的表**：
- WmsStockTray（库存托盘）
- WmsBaseMaterial（物料主数据）

---

#### 33. SaveUnbindWithNoBoxCode - 无箱码托盘变更

**功能**：无箱码托盘变更

**输入参数**：
- `OldStockCode`: 原托盘编码
- `NewStockCode`: 新托盘编码
- `CheckTaskNo`: 盘点任务号

**输出参数**：
- `Success`: 是否成功
- `Message`: 提示信息
- `Data`: 变更结果

**业务流程**：
1. 验证输入参数
2. 创建托盘变更记录
3. 更新库存托盘信息

**涉及的表**：
- WmsStockUnbind（托盘变更记录）
- WmsStockTray（库存托盘）
- WmsSysStockCode（托盘码主数据）

---

### 2.4 音频通知接口

#### 34. GenerateStorageAudio - 生成货位操作音频通知

**功能**：生成货位操作音频通知

**输入参数**：
- `locationCode`: 货位编号
- `operation`: 操作类型

**输出参数**：音频文件

**业务流程**：
1. 组合文本：货位编号+操作类型
2. 调用文本转语音
3. 返回音频文件

**涉及的表**：无

---

#### 35. GenerateTextAudio - 生成文本转音频

**功能**：生成文本转音频

**输入参数**：
- `text`: 要转换的文本

**输出参数**：音频文件

**业务流程**：
1. 调用系统语音合成
2. 生成音频文件
3. 返回音频文件

**涉及的表**：无

---

## 三、数据库表结构分析

### 3.1 核心业务表

#### 库存相关表

| 表名 | 说明 | 关键字段 | 关联关系 |
|-----|------|---------|---------|
| WmsStock | 库存汇总（物料级别） | MaterialId, WarehouseId, LotNo, StockQuantity | 关联WmsBaseMaterial, WmsBaseWareHouse |
| WmsStockTray | 库存托盘（托盘级别） | StockCode, StockSlotCode, MaterialId, StockQuantity | 关联WmsSysStockCode, WmsBaseSlot, WmsBaseMaterial |
| WmsStockInfo | 库存明细（箱码级别） | BoxCode, TrayId, MaterialId, Qty | 关联WmsStockTray, WmsBaseMaterial |
| WmsStockSlotBoxInfo | 库存箱托信息 | BoxCode, StockCode, ImportOrderId, Status | 关联WmsImportOrder, WmsSysStockCode |
| WmsSysStockCode | 托盘码主数据 | StockCode, Status, WarehouseId | 关联WmsBaseWareHouse |

#### 入库业务表

| 表名 | 说明 | 关键字段 | 关联关系 |
|-----|------|---------|---------|
| WmsImportNotify | 入库单据单 | ImportBillCode, WarehouseId, SupplierId | 关联WmsBaseWareHouse |
| WmsImportNotifyDetail | 入库单据单明细 | ImportId, MaterialId, ImportQuantity, ImportCompleteQuantity | 关联WmsImportNotify, WmsBaseMaterial |
| WmsImportOrder | 入库单 | ImportOrderNo, StockCodeId, ImportQuantity, ImportExecuteFlag | 关联WmsSysStockCode, WmsImportNotify |
| WmsImportLabelPrint | 入库标签打印 | LabelID, ImportBillCode, MaterialCode | 关联WmsImportNotify |
| WmsInspectNotity | 检验通知 | - | - |

#### 出库业务表

| 表名 | 说明 | 关键字段 | 关联关系 |
|-----|------|---------|---------|
| WmsExportOrder | 出库单 | ExportOrderNo, ExportStockCode, ExportExecuteFlag | 关联WmsSysStockCode |
| WmsExportNotify | 出库通知 | ExportNotifyNo, WarehouseId | 关联WmsBaseWareHouse |
| WmsExportNotifyDetail | 出库通知明细 | ExportNotifyId, MaterialId, ExportQuantity | 关联WmsExportNotify, WmsBaseMaterial |
| WmsExportTask | 出库任务 | ExportTaskNo, StockCode, TaskType | 关联WmsSysStockCode, WmsBaseSlot |
| WmsExportBoxInfo | 出库箱码信息 | BoxCode, ExportOrderId, Qty | 关联WmsExportOrder |

#### 盘点业务表

| 表名 | 说明 | 关键字段 | 关联关系 |
|-----|------|---------|---------|
| WmsStockCheckNotify | 盘点通知 | CheckNotifyNo, WarehouseId | 关联WmsBaseWareHouse |
| WmsStockCheckNotifyDetail | 盘点通知明细 | CheckNotifyId, MaterialId, CheckQuantity | 关联WmsStockCheckNotify, WmsBaseMaterial |
| WmsStockCheckInfo | 盘点箱码信息 | BoxCode, CheckNotifyDetailId, ActualQty | 关联WmsStockCheckNotifyDetail |
| WmsStockUnbind | 托盘变更记录 | OldStockCode, NewStockCode, CheckTaskNo | 关联WmsSysStockCode |

#### 特殊业务表

| 表名 | 说明 | 关键字段 | 关联关系 |
|-----|------|---------|---------|
| WmsManualDepalletizing | 人工拆跺 | BoxCode, TrayCode, ExportOrderNo, Qty | 关联WmsSysStockCode, WmsExportOrder |

#### 基础数据表

| 表名 | 说明 | 关键字段 | 关联关系 |
|-----|------|---------|---------|
| WmsBaseMaterial | 物料主数据 | MaterialCode, BoxQuantity, WarehouseId | 关联WmsBaseWareHouse |
| WmsBaseWareHouse | 仓库 | WarehouseCode, WarehouseType | - |
| WmsBaseSlot | 基础库位 | SlotCode, SlotStatus, SlotInout, WarehouseId | 关联WmsBaseWareHouse, WmsBaseLaneway |

---

### 3.2 表关联关系图

```
WmsBaseWareHouse (仓库)
    ├── WmsBaseLaneway (巷道)
    │   └── WmsBaseSlot (库位)
    ├── WmsBaseMaterial (物料)
    ├── WmsImportNotify (入库单据单)
    │   └── WmsImportNotifyDetail (入库单据单明细)
    │       └── WmsImportOrder (入库单)
    │           └── WmsStockSlotBoxInfo (箱托信息)
    │               └── WmsStockInfo (库存明细)
    │                   └── WmsStockTray (库存托盘)
    │                       └── WmsStock (库存汇总)
    ├── WmsExportNotify (出库通知)
    │   └── WmsExportNotifyDetail (出库通知明细)
    │       └── WmsExportOrder (出库单)
    │           ├── WmsExportBoxInfo (出库箱码信息)
    │           └── WmsManualDepalletizing (人工拆跺)
    ├── WmsStockCheckNotify (盘点通知)
    │   └── WmsStockCheckNotifyDetail (盘点通知明细)
    │       └── WmsStockCheckInfo (盘点箱码信息)
    └── WmsSysStockCode (托盘码)
        ├── WmsStockTray (库存托盘)
        ├── WmsImportOrder (入库单)
        ├── WmsExportOrder (出库单)
        ├── WmsExportTask (出库任务)
        └── WmsStockUnbind (托盘变更记录)
```

---

## 四、测试数据生成方案

### 4.1 测试场景设计

为了覆盖所有验证场景，设计以下测试场景：

#### 场景1：入库组托流程
1. 创建入库单据单和明细
2. 入库组托信息查询
3. 无箱码组托数量计算
4. 无箱码保存载具绑定
5. 托盘明细查询

#### 场景2：箱托绑定/解绑流程
1. 扫码获取箱托信息
2. 托盘状态查询
3. 绑定箱托关系
4. 删除单个箱码绑定
5. 托盘箱码解绑

#### 场景3：空托盘入库流程
1. 空托盘组盘（绑定）
2. 批量生成空托入库数据
3. 暂存入库生成
4. 叠箱绑定

#### 场景4：出库拆跺流程
1. 拆跺箱码信息查询
2. 托盘关联出库单查询
3. 拆跺信息查询
4. 拆跺数量计算
5. 拆跺箱码绑定
6. 拆跺记录查询
7. 人工拆垛出库确认

#### 场景5：无箱码出库流程
1. 无箱码拆跺绑定
2. 无箱码拆跺查询
3. 无箱码人工拆垛出库确认
4. 无箱码拆跺删除

#### 场景6：空托盘出库流程
1. 空托盘出库申请
2. 出库口列表查询

#### 场景7：盘点流程
1. 盘点单列表查询
2. 盘点箱码列表查询
3. 盘点结果提交
4. 托盘验证
5. 根据托盘号获取物料信息
6. 无箱码托盘变更

### 4.2 测试数据特点

1. **可随时删除**：所有测试数据使用统一的命名规则（前缀：TEST_PDA_），方便批量删除
2. **覆盖全场景**：覆盖所有接口和业务场景
3. **数据完整性**：确保表之间的关联关系正确
4. **可重复性**：可以多次执行，不会产生冲突

---

## 五、后续步骤

1. ✅ 完成接口和数据库表分析
2. 🔄 生成测试数据SQL脚本
3. ⏳ 生成测试数据清除SQL脚本
4. ⏳ 创建测试场景说明文档

---

**文档版本**：v1.0
**最后更新**：2026-01-13
**作者**：iFlow CLI