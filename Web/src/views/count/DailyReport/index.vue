<script lang="ts" setup name="wmsDailyReport">
import { ref, reactive, onMounted,onUnmounted,nextTick,watch,computed} from "vue";
import { auth } from '/@/utils/authFunction';
import { downloadStreamFile } from "/@/utils/download";
import { useWmsBaseBillTypeApi } from '/@/api/base/billType/wmsBaseBillType';
import { useWmsDailyReportApi } from '/@/api/count/wmsDailyReport';
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import * as echarts from 'echarts'
import type { EChartsOption } from 'echarts';

const wmsBaseBillTypeApi = useWmsBaseBillTypeApi();
const wmsDailyReportApi = useWmsDailyReportApi();

const state = reactive({
  exportLoading: false,
  tableLoading: false,
  tableShow:true,
  exportShow:false,
  stores: {},
  showAdvanceQueryUI: false,
  dropdownData: {} as any,
  selectData: [] as any[],
  tableQueryParams: {} as any,
  tableParams: {
    page: 1,
    pageSize: 20,
    total: 0,
    field: 'createTime', // 默认的排序字段
    order: 'descending', // 排序方向
    descStr: 'descending', // 降序排序的关键字符
  },
  tableData: [],
});

// 页面加载时
onMounted(async () => {
  const data = await wmsBaseBillTypeApi.getDropdownData(true).then(res => res.data.result) ?? {};
  state.dropdownData.wareHouseId = data.wareHouseId;
});

// 获取今天（YYYY-MM-DD格式）
const getToday = () => {
  const now = new Date();
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
};
    
const defaultStartDate = computed(() => getToday());
state.tableQueryParams.startDate=defaultStartDate.value;

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableQueryParams.ReportType=3;
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsDailyReportApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
  state.tableParams.total = result?.total;
  state.tableData = result?.items ?? [];
  state.tableLoading = false;
};

// 列排序
const sortChange = async (column: any) => {
  state.tableParams.field = column.prop;
  state.tableParams.order = column.order;
  await handleQuery();
};

// 导出数据
const exportWmsReportCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    if (command === 'select') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await wmsDailyReportApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams);
      await wmsDailyReportApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await wmsDailyReportApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
}

export interface ChartData {
  title?: string;
  subtitle?: string;
  labels?:string[];
  xAxisData?: string[];
  series: SeriesData[];
  legendData?: string[];
}

export interface SeriesData {
  name: string;
  type: 'bar' | 'line' | 'pie';
  data: number[] | Array<{name: string, value: number}>;  // 支持两种格式
  stack?: string;
}

const exportWmsReportChartsCommand= async (command: string) => {
  try {
    state.exportLoading = true;
    if (command === 'selectHistogram') {
      state.tableShow=false;
      state.exportShow=true;
      await chartHistogram();
    } else if (command === 'currentHistogram') {
      state.tableShow=false;
      state.exportShow=true;await chartHistogram();
    } else if (command === 'allHistogram') {
      state.tableShow=false;
      state.exportShow=true;await chartHistogram();
    }
    if (command === 'selectTrendCharts') {
      state.tableShow=false;
      state.exportShow=true;await chartTrendCharts();
    } else if (command === 'currentTrendCharts') {
      state.tableShow=false;
      state.exportShow=true; await chartTrendCharts();
    } else if (command === 'allTrendCharts') {
      state.tableShow=false;
      state.exportShow=true;await chartTrendCharts();
    }
    if (command === 'selectPieChart') {
      state.tableShow=false;
      state.exportShow=true;await chartPieChart();
    } else if (command === 'currentPieChart') {
      state.tableShow=false;
      state.exportShow=true;await chartPieChart();
    } else if (command === 'allPieChart') {
      state.tableShow=false;
      state.exportShow=true;
      await chartPieChart();
    }
  } finally {
    state.exportLoading = false;
  }
}

const chartHistogram =async () => {
  chartType.value='bar';
  await fetchChartData('bar')
  initChart()
  window.addEventListener('resize', handleResize)
}

const chartTrendCharts =async () => {
  chartType.value='line';
  await fetchChartData('line')
  initChart()
  window.addEventListener('resize', handleResize)
}

const chartPieChart=async () =>{
  chartType.value='pie';
  await fetchChartData('pie')
  initChart()
  window.addEventListener('resize', handleResize)
}

const chartData = ref<ChartData | null>(null)
const chartRef = ref<HTMLElement | null>(null)
let chartInstance: echarts.ECharts | null = null
const chartType=ref<'bar' | 'line' | 'pie'>('bar') 

let resizeTimer: any = null
let refreshTimer: any = null

// 图表数据
const loading = ref(false)
const error = ref<string>()

// 计算图表配置
const chartOption = computed<EChartsOption>(() => {
  if (!chartData.value) {
    return getDefaultOption()
  }

  const baseOption: EChartsOption = {
    title: {
      text: chartData.value.title || getDefaultTitle(),
      subtext: chartData.value.subtitle || ''
    },

    tooltip: {
      trigger: chartType.value === 'pie' ? 'item' : 'axis',
      axisPointer: {
        type: 'shadow',
        label: {
          show: true,
          formatter: function (params: any) {
            return params.value?.toString().replace('\n', '') || ''
          }
        }
      }
    },
    legend: chartType.value!="pie" 
    ? {
        show: true,
        data: chartData.value.legendData || chartData.value.series.map(s => s.name)
      }
    : { show: false }, // 设置为 false 来隐藏
    // legend: {
    //   data:chartData.value.legendData || chartData.value.series.map(s => s.name)
    // },
    toolbox: {
      show: true,
      feature: {
        dataView: { readOnly: false },
        restore: {},
        saveAsImage: {}
      }
    }
  }

  console.log(chartData.value.series.map(s => s.name));

  switch (chartType.value) {
    case 'bar':
      return {
        ...baseOption,
        xAxis: {
          type: 'category',
          // axisLabel: {
          //   interval: 0,
          //   rotate: 45
          // },
          data: chartData.value.xAxisData
        },
        yAxis: {
          type: 'value',
          name: '数值'
        },
        series: chartData.value.series.map(series => ({
          ...series,
          type: 'bar',
          // label: {
          //   show: true,
          //   position: 'top',
          //   fontSize: 12,
          //   color: '#333'
          // }
        }))
      }
    
    case 'line':
      return {
        ...baseOption,
        grid: {
          left: '3%',
          right: '4%',
           bottom: '8%',
          containLabel: true
        },
        xAxis: {
          type: 'category',
          boundaryGap: false,
          data: chartData.value.xAxisData
        },
        yAxis: {
          type: 'value'
        },
        series: chartData.value.series.map(series => ({
          ...series,
          type: 'line',
          stack: series.stack || 'Total'
        }))
      }

    //  case 'pie':
    //   // 处理饼图数据
    //   const firstSeries = chartData.value.series[0];
    //   const secondSeries = chartData.value.series[1];

    //   if(secondSeries!=undefined)
    //   {
    //     let pieData: Array<{name: string, value: number}> = [];
    //     let pieData2: Array<{name: string, value: number}> = [];

    //     if (firstSeries && Array.isArray(firstSeries.data)) {
    //       if (firstSeries.data.length > 0) {
    //         if (typeof firstSeries.data[0] === 'object') {
    //           // 数据已经是对象数组格式
    //           pieData = firstSeries.data as Array<{name: string, value: number}>;
    //         } else {
    //           // 数据是数字数组，需要结合 xAxisData 或 labels
    //           const names = chartData.value.xAxisData || chartData.value.labels || [];
    //           const values = firstSeries.data as number[];
    //           pieData = names.map((name, index) => ({
    //             name: name || `类别${index + 1}`,
    //             value: values[index] || 0
    //           }));
    //         }
    //       }
    //     }

    //     // 处理第二个饼图数据（如出库）
    //     if (secondSeries && Array.isArray(secondSeries.data)) {
    //       if (secondSeries.data.length > 0) {
    //         if (typeof secondSeries.data[0] === 'object') {
    //           pieData2 = secondSeries.data as Array<{name: string, value: number}>;
    //         } else {
    //           const names = chartData.value.xAxisData || chartData.value.labels || [];
    //           const values = secondSeries.data as number[];
    //           pieData2 = names.map((name, index) => ({
    //             name: name || `类别${index + 1}`,
    //             value: values[index] || 0
    //           }));
    //         }
    //       }
    //     }
        
    //     return {
    //       ...baseOption,
    //       // legend: {
    //       //   ...baseOption.legend,
    //       //   type: 'scroll',
    //       //   orient: 'vertical',
    //       //   right: 10,
    //       //   top: 50,
    //       //   bottom: 20
    //       // },
    //       series: [
    //         {
    //           // name: firstSeries?.name || chartData.value.title || '数据',
    //           name: firstSeries?.name || '入库分布',
    //           type: 'pie',
    //           // radius: ['40%', '70%'],
    //           // center: ['40%', '50%'],
    //           radius: ['40%', '60%'],
    //           center: ['30%', '50%'],
    //           data: pieData,
    //           label: {
    //             show: true,
    //             formatter: '{b}: {c} ({d}%)'
    //           }
    //         },
    //         {
    //           name: secondSeries?.name ||'出库分布',
    //           type: 'pie',
    //           // radius: ['40%', '70%'],
    //           // center: ['40%', '50%'],
    //           radius: ['40%', '60%'],
    //           center: ['70%', '50%'],
    //           data: pieData2,
    //           label: {
    //             show: true,
    //             formatter: '{b}: {c} ({d}%)'
    //           }
    //         }
    //       ]
    //     }
    //   }
    //   else
    //   {
    //     // 处理饼图数据
    //     let pieData: Array<{name: string, value: number}> = [];
        
    //     if (firstSeries && Array.isArray(firstSeries.data)) {
    //       if (firstSeries.data.length > 0) {
    //         if (typeof firstSeries.data[0] === 'object') {
    //           // 数据已经是对象数组格式
    //           pieData = firstSeries.data as Array<{name: string, value: number}>;
    //         } else {
    //           // 数据是数字数组，需要结合 xAxisData 或 labels
    //           const names = chartData.value.xAxisData || chartData.value.labels || [];
    //           const values = firstSeries.data as number[];
    //           pieData = names.map((name, index) => ({
    //             name: name || `类别${index + 1}`,
    //             value: values[index] || 0
    //           }));
    //         }
    //       }
    //     }
        
    //     return {
    //       ...baseOption,
    //       legend: {
    //         ...baseOption.legend,
    //         type: 'scroll',
    //         orient: 'vertical',
    //         right: 10,
    //         top: 50,
    //         bottom: 20
    //       },
    //       series: [
    //         {
    //           name: firstSeries?.name || chartData.value.title || '数据',
    //           type: 'pie',
    //           radius: ['40%', '70%'],
    //           center: ['50%', '50%'],
    //           data: pieData,
    //           label: {
    //             show: true,
    //             formatter: '{b}: {c} ({d}%)'
    //           }
    //         }
    //       ]
    //     }
    //   }
    case 'pie':
    // 处理饼图数据
    const firstSeries = chartData.value.series[0];
    const secondSeries = chartData.value.series[1];

    if(secondSeries != undefined) {
      let pieData: Array<{name: string, value: number}> = [];
      let pieData2: Array<{name: string, value: number}> = [];

      if (firstSeries && Array.isArray(firstSeries.data)) {
        if (firstSeries.data.length > 0) {
          if (typeof firstSeries.data[0] === 'object') {
            pieData = firstSeries.data as Array<{name: string, value: number}>;
          } else {
            const names = chartData.value.xAxisData || chartData.value.labels || [];
            const values = firstSeries.data as number[];
            pieData = names.map((name, index) => ({
              name: name || `类别${index + 1}`,
              value: values[index] || 0
            }));
          }
        }
      }

      if (secondSeries && Array.isArray(secondSeries.data)) {
        if (secondSeries.data.length > 0) {
          if (typeof secondSeries.data[0] === 'object') {
            pieData2 = secondSeries.data as Array<{name: string, value: number}>;
          } else {
            const names = chartData.value.xAxisData || chartData.value.labels || [];
            const values = secondSeries.data as number[];
            pieData2 = names.map((name, index) => ({
              name: name || `类别${index + 1}`,
              value: values[index] || 0
            }));
          }
        }
      }
      
      const pieTitle1 = firstSeries?.name || '入库分布';
      const pieTitle2 = secondSeries?.name || '出库分布';
      
      return {
        ...baseOption,
        graphic: [
          {
            type: 'text',
            left: '29%',
            top: '90%',
            style: {
              text: pieTitle1,
              textAlign: 'center',
              fill: '#333',
              fontSize: 14,
              fontWeight: 'bold'
            }
          },
          {
            type: 'text',
            left: '69%',
            top: '90%',
            style: {
              text: pieTitle2,
              textAlign: 'center',
              fill: '#333',
              fontSize: 14,
              fontWeight: 'bold'
            }
          }
        ],
        series: [
          {
            name: pieTitle1,
            type: 'pie',
            radius: ['40%', '60%'],
            center: ['30%', '50%'],
            data: pieData,
            label: {
              show: true,
              formatter: '{b}: {c} ({d}%)'
            }
          },
          {
            name: pieTitle2,
            type: 'pie',
            radius: ['40%', '60%'],
            center: ['70%', '50%'],
            data: pieData2,
            label: {
              show: true,
              formatter: '{b}: {c} ({d}%)'
            }
          }
        ]
      }
    } else {
      // 处理饼图数据
      let pieData: Array<{name: string, value: number}> = [];
      
      if (firstSeries && Array.isArray(firstSeries.data)) {
        if (firstSeries.data.length > 0) {
          if (typeof firstSeries.data[0] === 'object') {
            pieData = firstSeries.data as Array<{name: string, value: number}>;
          } else {
            const names = chartData.value.xAxisData || chartData.value.labels || [];
            const values = firstSeries.data as number[];
            pieData = names.map((name, index) => ({
              name: name || `类别${index + 1}`,
              value: values[index] || 0
            }));
          }
        }
      }
      
      const pieTitle = firstSeries?.name || chartData.value.title || '数据';
      
      return {
        ...baseOption,
        graphic: [
          {
            type: 'text',
            left: '50%',
            top: '90%',
            style: {
              text: pieTitle,
              textAlign: 'center',
              fill: '#333',
              fontSize: 14,
              fontWeight: 'bold'
            }
          }
        ],
        legend: {
          ...baseOption.legend,
          type: 'scroll',
          orient: 'vertical',
          right: 10,
          top: 50,
          bottom: 20
        },
        series: [
          {
            name: pieTitle,
            type: 'pie',
            radius: ['40%', '70%'],
            center: ['50%', '50%'],
            data: pieData,
            label: {
              show: true,
              formatter: '{b}: {c} ({d}%)'
            }
          }
        ]
      }
    }
    
    default:
      return getDefaultOption()
  }
})

// 获取默认标题
const getDefaultTitle = () => {
  const titles = {
    bar: '柱状图',
    line: '趋势图',
    pie: '占比图'
  }
  return titles[chartType.value] || '图表'
}

// 获取默认配置
const getDefaultOption = (): EChartsOption => ({
  title: {
    text: '加载中...',
    left: 'center',
    top: 'center'
  }
})

// 获取数据
const fetchChartData = async (type:string) => {
  try {
    loading.value = true
    error.value = undefined
    // 根据图表类型调用不同的API
    switch (type) {
      case 'bar':
        state.tableQueryParams.ChartsType="bar"
        chartData.value = await wmsDailyReportApi.chart( Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) })).then(res => res.data.result);
        break
      case 'line':
        state.tableQueryParams.ChartsType="line"
        chartData.value = await wmsDailyReportApi.chart( Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) })).then(res => res.data.result);
        break
      case 'pie':
         state.tableQueryParams.ChartsType="pie"
        chartData.value = await wmsDailyReportApi.chart(Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) })).then(res => res.data.result);
        break
    }
  } catch (err: any) {
    console.error('获取图表数据失败:', err)
    error.value = err.message || '获取数据失败'
    // 可以显示错误状态的图表
    chartData.value = {
      title: '数据加载失败',
      xAxisData: ['错误'],
      series: [{ name: '错误', type:chartType.value, data: [0] }]
    }
  } finally {
    loading.value = false
  }
}

// 初始化图表
const initChart = () => {
  if (!chartRef.value || !chartData.value) return
  
  nextTick(() => {
    if (chartRef.value) {
      // 销毁旧实例
      if (chartInstance) {
        chartInstance.dispose()
      }
      
      // 创建新实例
      chartInstance = echarts.init(chartRef.value)
      chartInstance.setOption(chartOption.value)
    }
  })
}

// 窗口大小变化处理
const handleResize = () => {
  if (resizeTimer) {
    clearTimeout(resizeTimer)
  }
  resizeTimer = setTimeout(() => {
    chartInstance?.resize()
  }, 200)
}

const stopAutoRefresh = () => {
  if (refreshTimer) {
    clearInterval(refreshTimer)
  }
}

onUnmounted(() => {
  if (chartInstance) {
    chartInstance.dispose()
    chartInstance = null
  }
  window.removeEventListener('resize', handleResize)
  stopAutoRefresh()
  
  if (resizeTimer) {
    clearTimeout(resizeTimer)
  }
})

// 监听数据变化
watch(chartData, () => {
  initChart()
}, { deep: true })

//返回列表 清除图表
const handleBackToList = () => {
  state.tableShow=true;state.exportShow=false;
  if (chartInstance) {
    chartInstance.dispose()
  }
};

//重置
const handleReset = () => {
  state.tableQueryParams = {};
  state.tableParams.page = 1;
  state.tableQueryParams.startDate = defaultStartDate.value;
  handleQuery();
};

handleQuery();
</script>
<template>
  <div v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }"> 
      <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="所属仓库">
              <el-select clearable filterable v-model="state.tableQueryParams.wareHouseId" placeholder="请选择所属仓库">
                <el-option v-for="(item,index) in state.dropdownData.wareHouseId ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="统计时间">
               <el-date-picker type="date" v-model="state.tableQueryParams.startDate" value-format="YYYY-MM-DD"
                      placeholder="统计时间" style="flex: 1;" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料编码">
              <el-input v-model="state.tableQueryParams.materialCode" clearable placeholder="请输入物料编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料名称">
                <el-input v-model="state.tableQueryParams.materialName" clearable placeholder="请输入物料名称"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item >
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'wmsDailyReport:page'" v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="handleReset"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true" v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                
                <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsReportChartsCommand">
                  <el-button type="primary" style="margin-left:5px;" icon="TrendCharts" v-reclick="20000" v-auth="'wmsDailyReport:chart'"> 图表 </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item icon="Histogram" command="selectHistogram" :disabled="state.selectData.length == 0">选中生成柱状图</el-dropdown-item>
                      <el-dropdown-item icon="TrendCharts" command="selectTrendCharts" :disabled="state.selectData.length == 0">选中生成折线图</el-dropdown-item>
                      <el-dropdown-item icon="PieChart" command="selectPieChart" :disabled="state.selectData.length == 0">选中生成饼图</el-dropdown-item>
                      <el-dropdown-item icon="Histogram" command="currentHistogram" >当前页生成柱状图</el-dropdown-item>
                      <el-dropdown-item icon="TrendCharts" command="currentTrendCharts" >当前页生成折线图</el-dropdown-item>
                      <el-dropdown-item icon="PieChart" command="currentPieChart" >当前页生成饼图</el-dropdown-item>
                      <el-dropdown-item icon="Histogram" command="allHistogram" >全部生成柱状图</el-dropdown-item>
                      <el-dropdown-item icon="TrendCharts" command="allTrendCharts" >全部生成折线图</el-dropdown-item>
                      <el-dropdown-item icon="PieChart" command="allPieChart" >全部生成饼图</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>

                <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsReportCommand">
                  <el-button type="primary" style="margin-left:5px;" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsDailyReport:export'"> 导出 </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="select" :disabled="state.selectData.length == 0">导出选中</el-dropdown-item>
                      <el-dropdown-item command="current">导出本页</el-dropdown-item>
                      <el-dropdown-item command="all">导出全部</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>

              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" v-show="state.tableShow" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
        <el-table-column type="selection" width="40" align="center" v-if="auth('wmsDailyReport:batchDelete') || auth('wmsDailyReport:export')" />
        <el-table-column type="index" label="序号" width="55" align="center"/>
        <el-table-column prop='materialCode' label='物料编码' show-overflow-tooltip />
        <el-table-column prop='materialName' label='物料名称' show-overflow-tooltip />
        <el-table-column prop='materialStandard' label='物料规格' show-overflow-tooltip />
       <el-table-column prop='unitName' label='计量单位' show-overflow-tooltip />
        <el-table-column prop='qty' label='入库数量' show-overflow-tooltip />
        <el-table-column prop='exportQuantity' label='出库数量' show-overflow-tooltip />
      </el-table>
      <el-pagination 
              v-model:currentPage="state.tableParams.page"
              v-model:page-size="state.tableParams.pageSize"
              @size-change="(val: any) => handleQuery({ pageSize: val })"
              @current-change="(val: any) => handleQuery({ page: val })"
              layout="total, sizes, prev, pager, next, jumper"
              :page-sizes="[10, 20, 50, 100, 200, 500]"
              :total="state.tableParams.total"
              size="small"
              background />
      <printDialog ref="printDialogRef" :title="'打印入出库日报'" @reloadTable="handleQuery" />
    </el-card>

    <el-card class="full-table" v-show="state.exportShow" shadow="hover" style="margin-top: 5px">
    <!-- <el-button 
        type="primary" 
        size="small"
      >
        <el-icon><ArrowLeft /></el-icon>
        返回列表
      </el-button>   -->

  <template #header>
    <div class="card-header">
      <span>统计图表</span>
      <el-button 
        type="primary" 
        size="small"
        @click="handleBackToList"
      >
        <el-icon><ArrowLeft /></el-icon>
        返回列表
      </el-button>
                
    </div>

    <!-- 图表容器 -->
    <div ref="chartRef" style="height: 600px;"></div>
    <!-- 工具栏 -->
    <!-- <div class="export-toolbar">
      <el-button-group>
        <el-button type="primary">
          <i class="el-icon-picture"></i> PNG
        </el-button>
        <el-button type="success">
          <i class="el-icon-picture"></i> JPG
        </el-button>
        <el-button type="info">
          <i class="el-icon-picture"></i> SVG
        </el-button>
      </el-button-group>
      
      <el-dropdown>
        <el-button type="warning">
          <i class="el-icon-document"></i> Excel导出
          <i class="el-icon-arrow-down el-icon--right"></i>
        </el-button>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item command="simple">简单导出</el-dropdown-item>
            <el-dropdown-item command="detailed">详细报表</el-dropdown-item>
            <el-dropdown-item command="raw">原始数据</el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
      
      <el-button type="danger">
        <i class="el-icon-download"></i> 一键下载全部
      </el-button>
    </div> -->
  </template>

    </el-card>
  </div>
</template>
<style scoped>
:deep(.el-input), :deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>