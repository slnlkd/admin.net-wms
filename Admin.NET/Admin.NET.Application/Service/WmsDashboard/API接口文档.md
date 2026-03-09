# 出入库任务趋势数据接口文档

## 接口信息

- **接口名称**: 获取出入库任务趋势数据
- **接口路径**: `/api/WmsDashboard/GetTaskTrend`
- **请求方式**: `GET`
- **接口描述**: 获取出入库任务的近7天、近30天趋势数据以及当前任务总数

## 请求参数

无需参数

## 响应数据结构

```json
{
  "last7Days": [
    {
      "date": "2025-11-26",
      "importCount": 15,
      "exportCount": 12
    },
    {
      "date": "2025-11-27",
      "importCount": 20,
      "exportCount": 18
    }
    // ... 共7天数据
  ],
  "last30Days": [
    {
      "date": "2025-11-03",
      "importCount": 12,
      "exportCount": 10
    }
    // ... 共30天数据
  ],
  "currentImportTotal": 150,
  "currentExportTotal": 120
}
```

## 字段说明

### Last7Days / Last30Days (每日数量统计)

| 字段名           | 类型   | 说明                     |
|------------------|--------|--------------------------|
| date             | string | 日期 (格式: yyyy-MM-dd)  |
| importCount      | int    | 当日入库任务数量         |
| exportCount      | int    | 当日出库任务数量         |

### 当前任务总数

| 字段名               | 类型   | 说明                                    |
|---------------------|--------|-----------------------------------------|
| currentImportTotal  | int    | 当前入库任务总数 (不包含已作废状态-1)   |
| currentExportTotal  | int    | 当前出库任务总数 (不包含已作废状态4)    |

## 数据表说明

### 入库单表 (WmsImportNotify)

- **统计字段**: `CreateTime`（创建时间）
- **过滤条件**: `IsDelete = false`（未删除）
- **作废状态**: `ImportExecuteFlag = "-1"`（字符串类型）
- **状态说明**:
  - `01` - 待执行
  - `02` - 正在执行
  - `03` - 已完成
  - `04` - 已上传
  - `-1` - 已作废

### 出库单表 (WmsExportNotify)

- **统计字段**: `CreateTime`（创建时间）
- **过滤条件**: `IsDelete = false`（未删除）
- **作废状态**: `ExportExecuteFlag = 4`（整数类型）
- **状态说明**:
  - `0` - 待执行
  - `1` - 正在分配
  - `2` - 正在执行
  - `3` - 已完成
  - `4` - 作废
  - `5` - 已上传

## 使用说明

1. **趋势数据**:
   - `last7Days` 包含最近7天的数据（包含今天）
   - `last30Days` 包含最近30天的数据（包含今天）
   - 如果某天没有数据，计数字段为0，但日期仍会返回

2. **当前总数**:
   - `currentImportTotal` 统计所有未删除且未作废的入库任务
   - `currentExportTotal` 统计所有未删除且未作废的出库任务

3. **时间范围**:
   - 基于 `CreateTime` 字段统计
   - 时间范围为 00:00:00 到 23:59:59

## 前端调用示例

### JavaScript/TypeScript

```typescript
import axios from 'axios';

interface DailyTaskCount {
  date: string;
  importCount: number;
  exportCount: number;
}

interface WmsTaskTrendOutput {
  last7Days: DailyTaskCount[];
  last30Days: DailyTaskCount[];
  currentImportTotal: number;
  currentExportTotal: number;
}

async function fetchTaskTrend() {
  try {
    const response = await axios.get<WmsTaskTrendOutput>('/api/WmsDashboard/GetTaskTrend');
    const data = response.data;

    console.log('近7天数据:', data.last7Days);
    console.log('近30天数据:', data.last30Days);
    console.log('当前入库总数:', data.currentImportTotal);
    console.log('当前出库总数:', data.currentExportTotal);

    return data;
  } catch (error) {
    console.error('获取趋势数据失败:', error);
  }
}
```

### Vue 3 Composition API

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { getTaskTrend } from '@/api/wms-dashboard';

interface DailyTaskCount {
  date: string;
  importCount: number;
  exportCount: number;
}

interface WmsTaskTrendOutput {
  last7Days: DailyTaskCount[];
  last30Days: DailyTaskCount[];
  currentImportTotal: number;
  currentExportTotal: number;
}

const trendData = ref<WmsTaskTrendOutput | null>(null);
const loading = ref(false);

const loadData = async () => {
  loading.value = true;
  try {
    const data = await getTaskTrend();
    trendData.value = data;
  } catch (error) {
    console.error('加载趋势数据失败:', error);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  loadData();
});
</script>

<template>
  <div v-if="loading">加载中...</div>
  <div v-else-if="trendData">
    <!-- 展示当前总数 -->
    <div class="task-summary">
      <div class="summary-item">
        <span>当前入库任务:</span>
        <strong>{{ trendData.currentImportTotal }}</strong>
      </div>
      <div class="summary-item">
        <span>当前出库任务:</span>
        <strong>{{ trendData.currentExportTotal }}</strong>
      </div>
    </div>

    <!-- 使用 ECharts 展示趋势图 -->
    <div ref="chartRef" style="width: 100%; height: 400px;"></div>
  </div>
</template>
```

## ECharts 图表示例

### 双轴折线图（展示入库和出库趋势）

```javascript
import * as echarts from 'echarts';

function initChart(chartRef, trendData) {
  const myChart = echarts.init(chartRef);

  const option = {
    title: {
      text: '出入库任务趋势（近7天）'
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'cross'
      }
    },
    legend: {
      data: ['入库任务', '出库任务']
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: trendData.last7Days.map(d => d.date)
    },
    yAxis: {
      type: 'value',
      name: '任务数量'
    },
    series: [
      {
        name: '入库任务',
        type: 'line',
        smooth: true,
        data: trendData.last7Days.map(d => d.importCount),
        itemStyle: {
          color: '#5470c6'
        },
        areaStyle: {
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: 'rgba(84, 112, 198, 0.5)' },
            { offset: 1, color: 'rgba(84, 112, 198, 0.1)' }
          ])
        }
      },
      {
        name: '出库任务',
        type: 'line',
        smooth: true,
        data: trendData.last7Days.map(d => d.exportCount),
        itemStyle: {
          color: '#91cc75'
        },
        areaStyle: {
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: 'rgba(145, 204, 117, 0.5)' },
            { offset: 1, color: 'rgba(145, 204, 117, 0.1)' }
          ])
        }
      }
    ]
  };

  myChart.setOption(option);

  // 响应式
  window.addEventListener('resize', () => {
    myChart.resize();
  });
}
```

### 堆叠柱状图

```javascript
const option = {
  title: {
    text: '出入库任务对比（近30天）'
  },
  tooltip: {
    trigger: 'axis',
    axisPointer: {
      type: 'shadow'
    }
  },
  legend: {
    data: ['入库任务', '出库任务']
  },
  xAxis: {
    type: 'category',
    data: trendData.last30Days.map(d => d.date.substring(5)) // 只显示月-日
  },
  yAxis: {
    type: 'value',
    name: '任务数量'
  },
  series: [
    {
      name: '入库任务',
      type: 'bar',
      stack: 'total',
      data: trendData.last30Days.map(d => d.importCount),
      itemStyle: { color: '#5470c6' }
    },
    {
      name: '出库任务',
      type: 'bar',
      stack: 'total',
      data: trendData.last30Days.map(d => d.exportCount),
      itemStyle: { color: '#91cc75' }
    }
  ]
};
```

## 注意事项

1. 接口只返回未删除 (`IsDelete = false`) 的出入库单数据
2. 日期以服务器时间为准
3. 如果数据库中没有任何数据，将返回空数组和零值统计
4. 建议在前端添加缓存机制，避免频繁请求
5. 入库和出库的作废状态值类型不同（字符串 vs 整数），需注意
6. 当日数据会实时更新，可设置定时刷新（如每5分钟）

## 性能说明

- 接口一次性查询30天数据，然后在内存中筛选7天数据
- 使用分组统计，避免多次数据库访问
- 返回连续日期序列，包含没有数据的日期（计数为0）
- 建议在数据量较大时，在 `CreateTime` 字段上创建索引
