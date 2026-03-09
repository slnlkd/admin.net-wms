# 首页仪表盘 - 前端集成说明

## 📊 功能说明

首页仪表盘集成了出入库任务趋势数据，通过调用后端 `/api/WmsDashboard/GetTaskTrend` 接口获取数据。

## 🔧 已集成的功能

### 1. 出入库任务趋势图表

- **数据来源**: `wmsDashboardApi.getTaskTrend()`
- **API 路径**: `/api/WmsDashboard/GetTaskTrend`
- **支持切换**: 近7天 / 近30天
- **图表类型**: ECharts 折线图
- **展示内容**:
  - 入库任务数量趋势（红色线）
  - 出库任务数量趋势（蓝色线）

### 2. 当前任务统计

- **当前入库任务**: 显示系统中所有未作废的入库任务总数
- **当前出库任务**: 显示系统中所有未作废的出库任务总数

### 3. 任务进度卡片

- 自动更新"入库任务"的数量为当前入库任务总数

## 📁 文件修改记录

### `index.vue`

**新增功能**:
1. 导入 `useWmsDashboardApi` API hooks
2. 初始化 `wmsDashboardApi` 实例
3. 新增 `loadingChartData` 加载状态
4. 新增 `trendData` 存储原始趋势数据
5. 重写 `loadDashboardData()` 方法调用后端 API
6. 新增 `updateChartByTimeRange()` 方法实现时间范围切换
7. 修改 `handleTimeRangeChange()` 支持实时切换
8. 添加图表加载状态 UI

**数据流程**:
```
1. 页面加载 → loadDashboardData()
2. 调用 API → /api/WmsDashboard/GetTaskTrend
3. 获取数据 → trendData (包含 last7Days, last30Days, currentImportTotal, currentExportTotal)
4. 根据 timeRange → updateChartByTimeRange()
5. 更新 taskData → 触发图表重新渲染
```

## 🎨 数据格式

### API 返回格式

```typescript
{
  last7Days: [
    { date: "2025-11-26", importCount: 15, exportCount: 12 },
    { date: "2025-11-27", importCount: 20, exportCount: 18 },
    // ... 共7天
  ],
  last30Days: [
    { date: "2025-11-03", importCount: 12, exportCount: 10 },
    // ... 共30天
  ],
  currentImportTotal: 150,
  currentExportTotal: 120
}
```

### 图表数据格式

```typescript
{
  dates: ['11-26', '11-27', '11-28', ...],  // 月-日格式
  inStock: [15, 20, 18, ...],               // 入库数量
  outStock: [12, 18, 16, ...]               // 出库数量
}
```

## 🔄 交互逻辑

### 时间范围切换

- **近7天**: 展示最近7天的数据（包含今天）
- **近30天**: 展示最近30天的数据（包含今天）
- **自定义**: 功能待实现（暂时回退到近7天）

切换时不会重新请求 API，而是从已加载的数据中筛选。

### 图表更新

- 初始加载时调用 API
- 切换时间范围时从内存数据切换
- 数据变化时自动触发图表重新渲染（通过 `watch`）

## 🎯 优化说明

1. **性能优化**:
   - 一次性加载30天数据
   - 切换时间范围时不重复请求
   - 使用 `nextTick` 确保 DOM 更新后再渲染图表

2. **加载体验**:
   - 添加加载状态动画
   - 空状态友好提示
   - 错误处理机制

3. **响应式设计**:
   - 图表自动适应窗口大小
   - 移动端友好布局

## 🔌 API 调用示例

```javascript
import { useWmsDashboardApi } from '/@/api/dashboard/wmsDashboard';

// 初始化 API
const wmsDashboardApi = useWmsDashboardApi();

// 加载仪表盘数据
const loadDashboardData = async () => {
  try {
    loadingChartData.value = true

    // 调用 API
    const response = await wmsDashboardApi.getTaskTrend()

    if (response.data && response.data.result) {
      trendData.value = response.data.result
      updateChartByTimeRange(timeRange.value)
      todoTasks.value[0].count = trendData.value.currentImportTotal
    }
  } catch (error) {
    console.error('加载仪表盘数据失败:', error)
  } finally {
    loadingChartData.value = false
  }
}
```

## 📝 待完善功能

- [ ] 自定义时间范围选择
- [ ] 数据导出功能
- [ ] 更多统计维度（如完成率、异常率等）
- [ ] 实时数据推送
- [ ] 图表类型切换（折线图、柱状图、堆叠图）

## 🐛 已知问题

1. 自定义时间范围功能待实现
2. 需要确保后端 API 已部署并可访问
3. 如果 API 返回格式不匹配，需要添加数据格式校验

## 📞 调试建议

1. 打开浏览器开发者工具
2. 查看 Network 标签确认 API 调用
3. 查看 Console 日志了解数据流
4. 使用 Vue DevTools 查看响应式数据变化

## 🔗 相关文档

- 后端 API 文档: `Admin.NET/Admin.NET.Application/Service/WmsDashboard/API接口文档.md`
- 后端服务说明: `Admin.NET/Admin.NET.Application/Service/WmsDashboard/README.md`
