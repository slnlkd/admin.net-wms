# WMS仪表盘服务

## 📊 服务说明

`WmsDashboardService` 提供WMS系统仪表盘相关的统计数据接口，用于前端展示各类趋势图表和实时统计信息。

## 📁 文件结构

```
WmsDashboard/
├── WmsDashboardService.cs          # 仪表盘服务类
├── Dto/
│   └── WmsDashboardOutput.cs       # 输出DTO定义
├── API接口文档.md                  # 详细的API接口文档
└── README.md                        # 本文件
```

## 🔌 已实现的接口

### 1. 获取出入库任务趋势数据

**接口路径**: `GET /api/WmsDashboard/GetTaskTrend`

**功能说明**:
- 获取近7天和近30天的出入库任务数量趋势
- 获取当前系统中的入库和出库任务总数

**返回数据**:
```json
{
  "last7Days": [
    { "date": "2025-11-26", "importCount": 15, "exportCount": 12 }
  ],
  "last30Days": [...],
  "currentImportTotal": 150,
  "currentExportTotal": 120
}
```

**数据来源**:
- 入库数据: `WmsImportNotify` 表
- 出库数据: `WmsExportNotify` 表

## 📝 使用方式

详细的接口文档和前端调用示例请参考：[API接口文档.md](./API接口文档.md)

## ⚡ 性能优化

- 一次性查询30天数据，内存中筛选7天数据
- 使用分组统计，减少数据库访问次数
- 返回连续日期序列（包含空数据日期）

## 🔄 后续扩展

可以在此服务中添加更多仪表盘相关接口：
- [ ] 库存统计趋势
- [ ] 任务完成率统计
- [ ] 异常数据统计
- [ ] 实时监控数据
- [ ] 各仓库业务量对比

## 📌 注意事项

1. 入库和出库的作废状态值类型不同：
   - 入库: `ImportExecuteFlag = "-1"` (字符串)
   - 出库: `ExportExecuteFlag = 4` (整数)

2. 所有统计都基于 `CreateTime` 字段

3. 只统计未删除的数据 (`IsDelete = false`)
