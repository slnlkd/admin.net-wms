<template>
  <div class="dashboard-container">
    <!-- 主要数据区域 -->
    <div class="main-content">
      <!-- 左侧：快捷导航 + 图表 -->
      <div class="left-panel">
        <!-- 快捷方式区域 -->
        <el-card class="quick-actions-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Star /></el-icon>
              <span>快捷方式</span>
            </div>
          </template>
          
          <!-- 加载状态 -->
          <div v-if="loadingQuickActions" class="actions-grid">
            <div 
              v-for="index in 6" 
              :key="index"
              class="action-item loading-item"
              :style="{ animationDelay: `${index * 0.1}s` }"
            >
              <div class="action-icon loading-icon">
                <el-icon :size="24">
                  <Loading />
                </el-icon>
              </div>
              <span class="action-label loading-label">加载中...</span>
            </div>
          </div>
          
          <!-- 正常状态 -->
          <div 
            v-else-if="quickActions.length > 0" 
            class="actions-grid"
          >
            <div 
              v-for="(action, index) in quickActions" 
              :key="action.id"
              class="action-item animated-item"
              :style="{ animationDelay: `${index * 0.1}s` }"
              @click="handleActionClick(action)"
            >
              <div class="action-icon" style="background-color: var(--el-color-primary);">
                <el-icon :size="24">
                  <component :is="action.icon" />
                </el-icon>
              </div>
              <span class="action-label">{{ action.title || action.label }}</span>
            </div>
          </div>
          
          <!-- 空状态 -->
          <el-empty v-else description="暂无快捷方式" :image-size="80" />
        </el-card>

        <!-- 图表区域 -->
        <el-card class="chart-card">
          <template #header>
            <div class="chart-header">
              <div class="header-left">
                <el-icon><DataAnalysis /></el-icon>
                <span>出入库任务量趋势</span>
              </div>
              <div class="time-filters">
                <el-radio-group v-model="timeRange" size="small" @change="handleTimeRangeChange">
                  <el-radio-button label="week">近7天</el-radio-button>
                  <el-radio-button label="month">近30天</el-radio-button>
                </el-radio-group>
              </div>
            </div>
          </template>

          <!-- 加载状态 -->
          <div v-if="loadingChartData" class="chart-loading">
            <el-icon :size="40" class="is-loading">
              <Loading />
            </el-icon>
            <p>加载数据中...</p>
          </div>

          <!-- 图表内容 -->
          <div v-else-if="hasChartData" class="chart-container">
            <div ref="chartRef" style="height: 393px; width: 100%;"></div>

            <!-- 统计数据 -->
            <!-- <div class="chart-stats">
              <div class="stat-item">
                <span class="stat-label">当前入库任务</span>
                <span class="stat-value in-value">{{ trendData.currentImportTotal }}</span>
              </div>
              <div class="stat-item">
                <span class="stat-label">当前出库任务</span>
                <span class="stat-value out-value">{{ trendData.currentExportTotal }}</span>
              </div>
            </div> -->
          </div>

          <!-- 空状态 -->
          <el-empty v-else description="暂无任务数据" :image-size="100">
            <template #description>
              <p>近期无出入库操作记录</p>
              <p class="empty-tip">您可以尝试调整时间范围或开始新的出入库任务</p>
            </template>
          </el-empty>
        </el-card>
      </div>
      
      <!-- 右侧：任务进度 + 仓储结构 -->
      <div class="right-panel">
        <!-- 待办事项 -->
        <el-card class="todo-card">
          <template #header>
            <div class="card-header" style="display: flex; justify-content: space-between; align-items: center;">
              <div class="header-left" style="display: flex; align-items: center; gap: 8px;">
                <el-icon><Clock /></el-icon>
                <span>当前任务进度</span>
                <el-tag type="warning" size="small">待办: {{ totalPending }}</el-tag>
              </div>
              <div class="header-right">
                <el-radio-group v-model="timeRange" size="small" @change="handleTimeRangeChange">
                  <el-radio-button label="week">7天</el-radio-button>
                  <el-radio-button label="month">30天</el-radio-button>
                </el-radio-group>
              </div>
            </div>
          </template>

          <div class="todo-list">
            <div 
              v-for="(task, index) in todoTasks" 
              :key="task.id"
              class="todo-item animated-task"
              :class="getPriorityClass(task)"
              :style="{ animationDelay: `${index * 0.15}s` }"
              @click="handleTaskClick(task)"
            >
              <div class="task-info">
                <div class="task-name-wrapper">
                  <span class="task-name">{{ task.name }}</span>
                  <el-tag v-if="task.priority === 'high'" size="small" type="danger">紧急</el-tag>
                  <el-tag v-else-if="task.priority === 'medium'" size="small" type="warning">重要</el-tag>
                </div>
                <span class="task-count" :style="{ color: task.color }">
                  {{ task.count }}
                </span>
              </div>
              <div class="task-progress" v-if="task.total">
                <el-progress 
                  :percentage="calculateProgress(task)" 
                  :show-text="false"
                  :color="task.color"
                />
                <span class="progress-text">
                  {{ task.completed }}/{{ task.total }}
                </span>
              </div>
              <el-icon class="arrow-icon"><ArrowRight /></el-icon>
            </div>
          </div>
        </el-card>

        <!-- 物料占比 -->
        <el-card class="ratio-card">
          <template #header>
            <div class="card-header">
              <div class="header-left">
                <el-icon><PieChart /></el-icon>
                <span>仓储结构分析</span>
              </div>
              <!-- <el-select v-model="timeFilter" size="small" style="width: 120px">
                <el-option
                  v-for="option in timeFilterOptions"
                  :key="option.value"
                  :label="option.label"
                  :value="option.value"
                />
              </el-select> -->
            </div>
          </template>

          <div class="ratio-content">
            <div ref="pieChartRef" style="height: 150px; width: 100%;"></div>
            
            <div class="ratio-stats">
              <div 
                v-for="(item, index) in materialData" 
                :key="item.name"
                class="stat-item animated-stats"
                :style="{ animationDelay: `${index * 0.2}s` }"
              >
                <div class="stat-info">
                  <div class="stat-color-wrapper">
                    <div class="stat-color" :style="{ backgroundColor: item.color }"></div>
                    <span class="stat-name">{{ item.name }}</span>
                  </div>
                  <span class="stat-percent">{{ item.percentage }}%</span>
                </div>
                <span class="stat-value">{{ item.value }}件</span>
              </div>
            </div>
          </div>
          
          <div class="ratio-footer">
            <div class="total-inventory animated-total">
              <span class="total-label">库存总量</span>
              <span class="total-value">{{ totalInventory }}件</span>
            </div>
          </div>
        </el-card>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import * as echarts from 'echarts'
import { 
  Star, 
  DataAnalysis, 
  Clock, 
  PieChart, 
  ArrowRight,
  Loading
} from '@element-plus/icons-vue'

import { useRouter } from 'vue-router';
import { getAPI } from '/@/utils/axios-utils';
import { SysRoleApi } from '/@/api-services/api';
import { useWmsDashboardApi } from '/@/api/dashboard/wmsDashboard';
import { useWmsBaseMaterialApi, TimeFilter } from '/@/api/base/baseMaterial/wmsBaseMaterial';

// ========== API 实例 ==========
const wmsDashboardApi = useWmsDashboardApi();
const wmsBaseMaterialApi = useWmsBaseMaterialApi();

// ========== 响应式数据 ==========
const dateRange = ref([])
const timeRange = ref('week')
const timeFilter = ref('week')

// 时间筛选选项
const timeFilterOptions = [
  { label: '本周', value: 'week' },
  { label: '本月', value: 'month' },
  { label: '本季度', value: 'quarter' }
]
const loadingQuickActions = ref(true)
const loadingChartData = ref(false)
let chartInstance = null
let pieChartInstance = null
const router = useRouter();


// 图表引用
const chartRef = ref()
const pieChartRef = ref()

// 快捷方式数据
const quickActions = ref([])

// 任务数据
const taskData = ref({
  dates: [],
  inStock: [],
  outStock: []
})

// 原始趋势数据（用于切换7天/30天）
const trendData = ref({
  last7Days: [],
  last30Days: [],
  currentImportTotal: 0,
  currentExportTotal: 0
})

// 待办事项数据
const todoTasks = ref([
  { 
    id: 1, 
    name: '入库任务', 
    count: 0, 
    completed: 0, 
    total: 0, 
    color: '#409EFF',
    priority: 'low',
    path: '/import/wmsimporttask'
  },
  { 
    id: 2, 
    name: '出库任务', 
    count: 0, 
    completed: 0, 
    total: 0, 
    color: '#67C23A',
    priority: 'high',
    path: '/export/wmsexporttask'
  },
  { 
    id: 3, 
    name: '移库任务', 
    count: 0, 
    completed: 0, 
    total: 0, 
    color: '#E6A23C',
    priority: 'medium',
    path: '/move/wmsmovetask'
  },
  { 
    id: 4, 
    name: '盘点任务', 
    count: 0, 
    completed: 0, 
    total: 0, 
    color: '#F56C6C',
    priority: 'medium',
    path: '/stockCheck/wmsStockCheckTask'
  }
])

// 物料数据
const materialData = ref([])

// ========== 计算属性 ==========
// 是否有图表数据
const hasChartData = computed(() => {
  return taskData.value && 
         taskData.value.dates && 
         taskData.value.dates.length > 0
})

// 入库总数
const totalInStock = computed(() => {
  return taskData.value?.inStock?.reduce((sum, val) => sum + val, 0) || 0
})

// 出库总数
const totalOutStock = computed(() => {
  return taskData.value?.outStock?.reduce((sum, val) => sum + val, 0) || 0
})

// 总待办数量
const totalPending = computed(() => {
  return todoTasks.value.reduce((sum, task) => sum + task.count, 0)
})

// 库存总量
const totalInventory = computed(() => {
  return materialData.value.reduce((sum, item) => sum + item.value, 0)
})

// ========== 方法 ==========
// 初始化日期范围
const initDateRange = async () => {
  const end = new Date()
  const start = new Date()
  start.setDate(start.getDate() - 7)
  dateRange.value = [start, end]
}

// 加载快捷方式数据
const loadQuickActions = async () => {
  try {
    loadingQuickActions.value = true
    
    const res = await getAPI(SysRoleApi).apiSysRoleGetQuickActionList(0)
    quickActions.value = res.data.result || []
  } catch (error) {
    console.error('加载快捷方式失败:', error)
    quickActions.value = []
  } finally {
    loadingQuickActions.value = false
  }
}

// 加载仪表盘数据
const loadDashboardData = async () => {
  try {
    loadingChartData.value = true

    // 调用后端API
    const response = await wmsDashboardApi.getTaskTrend()

    if (response.data && response.data.result) {
      trendData.value = response.data.result

      // 根据当前选择的时间范围更新图表
      updateChartByTimeRange(timeRange.value)
    }
  } catch (error) {
    console.error('加载仪表盘数据失败:', error)
    // 如果失败，使用空数据
    taskData.value = {
      dates: [],
      inStock: [],
      outStock: []
    }
  } finally {
    loadingChartData.value = false
  }
}

// 加载物料库存占比数据
const loadMaterialStockRatio = async () => {
  try {
    // 将字符串转换为枚举值
    let timeFilterValue = 0
    if (timeFilter.value === 'week') {
      timeFilterValue = 0
    } else if (timeFilter.value === 'month') {
      timeFilterValue = 1
    } else if (timeFilter.value === 'quarter') {
      timeFilterValue = 2
    }

    const response = await wmsBaseMaterialApi.getMaterialStockRatio(timeFilterValue)

    if (response.data && response.data.result) {
      const result = response.data.result
      materialData.value = result.materialData || []
    }
  } catch (error) {
    console.error('加载物料库存占比数据失败:', error)
    // 如果失败，使用默认数据
    materialData.value = [
      { name: '血浆', value: 45, percentage: 5, color: '#409EFF' },
      { name: '原辅料', value: 90, percentage: 10, color: '#67C23A' },
      { name: '成品', value: 765, percentage: 85, color: '#9B59B6' }
    ]
  }
}

// 根据时间范围更新图表数据
const updateChartByTimeRange = (range) => {
  // 更新图表数据
  const data = range === 'week' ? trendData.value.last7Days : trendData.value.last30Days

  if (data && data.length > 0) {
    taskData.value = {
      dates: data.map(item => item.date.substring(5)), // 只显示月-日
      inStock: data.map(item => item.importCount),
      outStock: data.map(item => item.exportCount)
    }
  } else {
    taskData.value = {
      dates: [],
      inStock: [],
      outStock: []
    }
  }
  
  // 更新任务进度数据
  const progressData = range === 'week' ? trendData.value.taskProgress7Days : trendData.value.taskProgress30Days
  if (progressData && progressData.length > 0) {
    // 遍历后端返回的数据，合并到前端定义的 todoTasks 中（保留前端定义的 path 和 id）
    todoTasks.value.forEach(task => {
      const remoteTask = progressData.find(p => p.name === task.name)
      if (remoteTask) {
        task.count = remoteTask.count
        task.completed = remoteTask.completed
        task.total = remoteTask.total
        task.color = remoteTask.color || task.color // 优先使用后端颜色，如果没有则保持前端默认
        // task.priority = remoteTask.priority || task.priority
        
        // 前端根据数量判断优先级
        if (task.count > 5) {
          task.priority = 'high'
        } else if (task.count > 1) {
          task.priority = 'medium'
        } else {
          task.priority = 'low'
        }
      }
    })
  }
}

// 日期范围变更处理
const handleDateChange = () => {
  loadDashboardData()
}

// 时间范围变更处理
const handleTimeRangeChange = (value) => {
  console.log('时间范围变更:', value)

  if (value === 'custom') {
    // 自定义时间范围功能待实现
    console.log('自定义时间范围功能待实现')
    timeRange.value = 'week' // 暂时回退到周视图
    return
  }

  // 直接从已加载的数据中切换
  updateChartByTimeRange(value)
}

// 快捷方式点击处理
const handleActionClick = (action) => {
  router.push(action.path);
  console.log('跳转到:', action.path)
  // 实际使用时替换为：router.push(action.path)
}

// 任务点击处理
const handleTaskClick = (task) => {
  console.log('跳转到任务列表:', task.path)
  router.push(task.path)
}

// 计算任务进度百分比
const calculateProgress = (task) => {
  if (!task.total) return 0
  return Math.round((task.completed / task.total) * 100)
}

// 根据优先级获取样式类
const getPriorityClass = (task) => {
  return {
    'priority-high': task.priority === 'high',
    'priority-medium': task.priority === 'medium',
    'priority-low': task.priority === 'low'
  }
}

// ========== 图表相关方法 ==========
// 初始化主图表
const initChart = () => {
  if (!chartRef.value) return
  
  // 如果已经初始化过，销毁旧实例
  if (chartInstance) {
    chartInstance.dispose()
  }
  
  chartInstance = echarts.init(chartRef.value)
  updateChart()
}

// 更新主图表数据
const updateChart = () => {
  if (!chartInstance || !taskData.value) return

  const option = {
    tooltip: {
      trigger: 'axis',
      formatter: function(params) {
        let result = `${params[0].axisValue}<br/>`
        params.forEach(param => {
          const color = param.seriesName === '入库数量' ? '#F56C6C' : '#409EFF'
          result += `<span style="display:inline-block;margin-right:5px;border-radius:10px;width:9px;height:9px;background-color:${color}"></span>`
          result += `${param.seriesName}: ${param.value}<br/>`
        })
        return result
      }
    },
    legend: {
      data: ['入库数量', '出库数量'],
      top: 'bottom'
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '15%',
      top: '3%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: taskData.value.dates
    },
    yAxis: {
      type: 'value'
    },
    series: [
      {
        name: '入库数量',
        type: 'line',
        smooth: true,
        lineStyle: {
          color: '#F56C6C',
          width: 3
        },
        itemStyle: {
          color: '#F56C6C'
        },
        data: taskData.value.inStock
      },
      {
        name: '出库数量',
        type: 'line',
        smooth: true,
        lineStyle: {
          color: '#409EFF',
          width: 3
        },
        itemStyle: {
          color: '#409EFF'
        },
        data: taskData.value.outStock
      }
    ]
  }

  chartInstance.setOption(option)
}

// 初始化饼图
const initPieChart = () => {
  if (!pieChartRef.value) return

  pieChartInstance = echarts.init(pieChartRef.value)
  updatePieChart()  // 确保初始化时也更新数据
}

// 更新饼图
const updatePieChart = () => {
  if (!pieChartInstance) return

  const option = {
    tooltip: {
      trigger: 'item',
      formatter: '{a} <br/>{b}: {c} ({d}%)'
    },
    legend: {
      orient: 'vertical', // 改为垂直排列
      right: 10, // 靠右显示
      top: 'center', // 垂直居中
      data: materialData.value.map(item => item.name)
    },
    series: [
      {
        name: '物料占比',
        type: 'pie',
        radius: ['40%', '70%'],
        center: ['40%', '50%'], // 饼图左移，为右侧图例留出空间
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 10,
          borderColor: '#fff',
          borderWidth: 2
        },
        label: {
          show: false
        },
        emphasis: {
          label: {
            show: true,
            fontSize: 12,
            fontWeight: 'bold'
          }
        },
        labelLine: {
          show: false
        },
        data: materialData.value.map(item => ({
          value: item.value,
          name: item.name,
          itemStyle: { color: item.color }
        }))
      }
    ]
  }

  pieChartInstance.setOption(option)
}

// 窗口大小变化时重绘图表
const handleResize = () => {
  chartInstance?.resize()
  pieChartInstance?.resize()
}

// ========== 生命周期 ==========
onMounted(() => {
  // 初始化数据
  initDateRange()
  loadQuickActions()
  loadDashboardData()
  loadMaterialStockRatio()
  
  // 初始化图表
  nextTick(() => {
    // 尝试初始化饼图，柱状图可能因为 loading 还没渲染
    initPieChart()
  })
  
  // 监听窗口大小变化
  window.addEventListener('resize', handleResize)
})

// 监听数据变化
watch(() => taskData.value, () => {
  nextTick(() => {
    if (!chartInstance && chartRef.value) {
      initChart()
    }
    if (chartInstance && taskData.value.dates.length > 0) {
      updateChart()
    }
  })
}, { deep: true })

// 监听物料数据变化
watch(() => materialData.value, (newVal) => {
  nextTick(() => {
    if (newVal && newVal.length > 0) {
      if (!pieChartInstance && pieChartRef.value) {
        initPieChart()
      }
      if (pieChartInstance) {
        updatePieChart()
      }
    }
  })
}, { deep: true })

// 监听 loading 状态，当 loading 结束且有数据时初始化图表
watch(loadingChartData, (newVal) => {
  if (!newVal && hasChartData.value) {
    nextTick(() => {
      initChart()
    })
  }
})

// 监听筛选条件变化
watch(timeFilter, () => {
  console.log('时间筛选变更:', timeFilter.value)
  loadMaterialStockRatio()
})
</script>

<style scoped>
.dashboard-container {
  background-color: #f5f7fa;
}

/* 主要内容区域 */
.main-content {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: 20px;
  align-items: start;
}

.left-panel {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.right-panel {
  display: flex;
  flex-direction: column;
  gap: 19px;
}

/* 快捷方式区域 */
.quick-actions-card {
  border-radius: 12px;
  transition: all 0.3s ease;
}

.quick-actions-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15) !important;
}

.quick-actions-card .card-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  color: #303133;
}

.actions-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
}

.action-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 20px 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.action-item:hover {
  background-color: #f5f7fa;
  transform: scale(1.05);
}

.action-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 56px;
  height: 56px;
  border-radius: 12px;
  color: white;
}

.action-label {
  font-size: 14px;
  color: #606266;
  text-align: center;
  font-weight: 500;
}

/* 快捷方式加载动画 */
.action-item.animated-item {
  opacity: 0;
  animation: fadeInUp 0.5s ease forwards;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* 加载状态样式 */
.loading-item {
  opacity: 0;
  animation: fadeInUp 0.5s ease forwards;
}

.loading-icon {
  background-color: #f0f2f5 !important;
  color: #c0c4cc;
  animation: pulse 1.5s ease-in-out infinite;
}

.loading-label {
  color: #c0c4cc;
  background-color: #f0f2f5;
  border-radius: 4px;
  width: 60px;
  height: 16px;
  display: inline-block;
  animation: pulse 1.5s ease-in-out infinite;
}

@keyframes pulse {
  0% {
    opacity: 0.6;
  }
  50% {
    opacity: 1;
  }
  100% {
    opacity: 0.6;
  }
}

/* 图表卡片样式 */
.chart-card {
  border-radius: 12px;
  flex: 1;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.chart-header .header-left {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  color: #303133;
}

.chart-stats {
  display: flex;
  gap: 30px;
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid #e4e7ed;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  flex: 1;
}

.stat-label {
  font-size: 14px;
  color: #909399;
}

.stat-value {
  font-size: 24px;
  font-weight: 600;
}

.in-value {
  color: #F56C6C;
}

.out-value {
  color: #409EFF;
}

.rate-value {
  color: #67C23A;
}

.empty-tip {
  font-size: 12px;
  color: #909399;
  margin-top: 8px;
}

/* 图表加载状态 */
.chart-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 300px;
  color: #909399;
}

.chart-loading p {
  margin-top: 16px;
  font-size: 14px;
}

/* 待办事项样式 */
.todo-card {
  border-radius: 12px;
  flex: 1;
}

.todo-card .card-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  color: #303133;
}

.todo-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
  margin-bottom: 16px;
}

.todo-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px;
  border-radius: 8px;
  border: 1px solid #e4e7ed;
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
  opacity: 0;
  transform: translateX(-20px);
}

.todo-item.animated-task {
  animation: slideInRight 0.6s ease forwards;
}

@keyframes slideInRight {
  from {
    opacity: 0;
    transform: translateX(-20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

.todo-item:hover {
  border-color: #409EFF;
  box-shadow: 0 2px 8px rgba(64, 158, 255, 0.1);
}

.todo-item.priority-high {
  border-left: 4px solid #F56C6C;
}

.todo-item.priority-medium {
  border-left: 4px solid #E6A23C;
}

.todo-item.priority-low {
  border-left: 4px solid #67C23A;
}

.task-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex: 1;
  margin-right: 16px;
}

.task-name-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
}

.task-name {
  font-size: 14px;
  color: #606266;
  font-weight: 500;
}

.task-count {
  font-size: 18px;
  font-weight: 600;
  min-width: 40px;
  text-align: right;
}

.task-progress {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 0 0 120px;
}

.task-progress :deep(.el-progress) {
  flex: 1;
}

.progress-text {
  font-size: 12px;
  color: #909399;
  min-width: 40px;
}

.arrow-icon {
  color: #c0c4cc;
  transition: all 0.3s ease;
}

.todo-item:hover .arrow-icon {
  color: #409EFF;
  transform: translateX(2px);
}

.todo-footer {
  display: flex;
  justify-content: center;
  padding-top: 12px;
  border-top: 1px solid #e4e7ed;
}

/* 物料占比样式 */
.ratio-card {
  border-radius: 12px;
  flex: 1;
}

.ratio-card .card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.ratio-card .header-left {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  color: #303133;
}

.ratio-content {
  display: flex;
  flex-direction: column;
}

.ratio-stats {
  margin-top: 20px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.ratio-stats .stat-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-radius: 8px;
  background-color: #f8f9fa;
  flex-direction: row;
  opacity: 0;
  transform: translateY(10px);
}

.ratio-stats .stat-item.animated-stats {
  animation: fadeInUp 0.5s ease forwards;
}

.stat-info {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex: 1;
}

.stat-color-wrapper {
  display: flex;
  align-items: center;
  gap: 12px;
}

.stat-color {
  width: 12px;
  height: 12px;
  border-radius: 50%;
}

.stat-name {
  font-size: 14px;
  color: #606266;
  font-weight: 500;
}

.stat-percent {
  font-size: 14px;
  color: #909399;
  font-weight: 500;
}

.ratio-stats .stat-value {
  font-size: 14px;
  color: #303133;
  font-weight: 600;
  min-width: 60px;
  text-align: right;
}

.ratio-footer {
  display: flex;
  justify-content: center;
  padding-top: 16px;
  margin-top: 16px;
  border-top: 1px solid #e4e7ed;
}

.total-inventory {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background-color: #f0f9ff;
  border-radius: 6px;
  opacity: 0;
  transform: scale(0.9);
}

.total-inventory.animated-total {
  animation: scaleIn 0.5s ease 0.6s forwards;
}

@keyframes scaleIn {
  from {
    opacity: 0;
    transform: scale(0.9);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

.total-label {
  font-size: 14px;
  color: #606266;
}

.total-value {
  font-size: 16px;
  font-weight: 600;
  color: #409EFF;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .main-content {
    grid-template-columns: 1fr;
  }
  
  .actions-grid {
    grid-template-columns: repeat(6, 1fr);
  }
}

@media (max-width: 768px) {
  .dashboard-container {
    padding: 12px;
  }
  
  .actions-grid {
    grid-template-columns: repeat(3, 1fr);
  }
  
  .chart-stats {
    flex-direction: column;
    gap: 16px;
  }
  
  .task-progress {
    flex: 0 0 100px;
  }
}
</style>