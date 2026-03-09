<template>
    <div class="wms-storage-legend">
        <!-- 筛选条件区域 -->
        <el-card class="filter-card">
            <el-form :model="filterForm" :inline="true">
                <el-form-item label="库房">
                    <el-select clearable filterable v-model="filterForm.warehouseId" placeholder="请选择所属仓库"
                        @change="handleHouseChange">
                        <el-option v-for="(item, index) in state.dropdownData.warehouseId ?? []" :key="index"
                            :value="item.value" :label="item.label" />
                    </el-select>
                </el-form-item>

                <el-form-item label="展示类型">
                    <el-select v-model="filterForm.showType" placeholder="请选择展示类型" @change="handleShowTypeChange">
                        <el-option label="按层展示" value="layer" />
                        <el-option label="按排展示" value="row" />
                    </el-select>
                </el-form-item>

                <!-- 按层展示时的筛选条件 -->
                <el-form-item v-if="filterForm.showType === 'layer'" label="层数">
                    <el-select clearable filterable v-model="filterForm.slotLayer" placeholder="请选择层数"
                        @change="handleLayerChange">
                        <el-option v-for="(item, index) in state.dropdownData.layerId ?? []" :key="index"
                            :value="item.value" :label="item.label" />
                    </el-select>
                </el-form-item>

                <!-- 按排展示时的筛选条件 -->
                <template v-if="filterForm.showType === 'row'">
                    <el-form-item label="巷道">
                        <el-select clearable filterable v-model="filterForm.slotLanewayId" placeholder="请选择所属巷道"
                            @change="handleGalleryChange">
                            <el-option v-for="(item, index) in state.dropdownData.slotLanewayId ?? []" :key="index"
                                :value="item.value" :label="item.label" />
                        </el-select>
                    </el-form-item>

                    <el-form-item label="排">
                        <el-select v-model="filterForm.slotRow" placeholder="请选择排" @change="handleRowChange">
                            <el-option v-for="row in rowOptions" :key="row.value" :label="row.label"
                                :value="row.value" />
                        </el-select>
                    </el-form-item>
                    <el-form-item label="储位深度">
                        <g-sys-dict v-model="filterForm.slotInout" code="SlotInout" render-as="select"
                            placeholder="请选择储位深度" clearable filterable @change="handleRowChange" />
                    </el-form-item>
                </template>
            </el-form>
        </el-card>

        <!-- 储位信息展示区域 -->
        <el-card class="main-content">
            <!-- 储位信息轮播 -->
            <div class="slot-info-carousel">
                <el-carousel :autoplay="false" arrow="hover" :motion-blur="true" height="40px" indicator-position="none">
                    <el-carousel-item v-for="(info, index) in slotInfos" :key="index">
                        <div style="display: flex;
                            padding: 10px 20px 10px;
                            gap:10%;margin-left: 10%;" class="carousel-content">
                            <div class="info-item">储位编码: {{ info.slotCode || '-' }}</div>
                            <div class="info-item">物料名称: {{ info.materialName || '-' }}</div>
                            <div class="info-item">物料批次: {{ info.batchNo || '-' }}</div>
                            <div class="info-item">数量: {{ info.weight || '-' }}</div>
                            <div class="info-item">载具号: {{ info.stockCode || '-' }}</div>
                        </div>
                    </el-carousel-item>
                </el-carousel>
            </div>

            <div class="content-wrapper">
                <!-- 储位图例展示 -->
                <div class="slot-display-container">
                    <div class="slot-grid-container">
                        <table class="slot-grid">
                            <tr v-for="(row, rowIndex) in slotGrid" :key="rowIndex">
                                <td v-for="(slot, colIndex) in row" :key="colIndex">
                                    <div v-if="slot.status != 8" class="slot-cell" :class="[
                                        getSlotStatusClass(slot.status),
                                        { active: selectedSlot === slot.id }
                                    ]" :style="getStatusStyle(slot.status)" @click="handleSlotClick(slot)"
                                        @contextmenu.prevent="handleSlotRightClick(slot)"
                                        @mouseenter="handleSlotMouseEnter(slot, $event)"
                                        @mouseleave="handleSlotMouseLeave">
                                        <!-- <span class="slot-code">{{ slot.slotCode }} </span> -->
                                    </div>
                                    <div v-else class="slot-wall">
                                        <!-- 墙体，不显示编码 -->
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>

                    <!-- 自定义 Tooltip -->
                    <div v-show="showTooltip" class="custom-tooltip" :style="tooltipStyle">
                        <div class="tooltip-content">
                            <div class="tooltip-section">
                                <h4 class="section-title">基本信息</h4>
                                <div class="tooltip-row"><strong>储位编码:</strong> {{ currentTooltipSlot.slotCode || '-' }}
                                </div>
                                <div class="tooltip-row"><strong>储位状态:</strong> {{ currentTooltipSlot.slotStatusName }}
                                </div>
                            </div>

                            <div class="tooltip-section">
                                <h4 class="section-title">位置信息</h4>
                                <div class="tooltip-row"><strong>巷道:</strong> {{
                                    currentTooltipSlot.slotLanewayIdFkColumn || '-'
                                    }}</div>
                                <div class="tooltip-row"><strong>层:</strong> 第{{ currentTooltipSlot.slotLayer
                                    + '层' || '-'
                                }}</div>
                                <div class="tooltip-row"><strong>排:</strong> 第{{ currentTooltipSlot.slotRow
                                    + '排' || '-'
                                    }}</div>
                                <div class="tooltip-row"><strong>列:</strong> 第{{ currentTooltipSlot.slotColumn
                                    + '列' || '-' }}
                                </div>
                                <div class="tooltip-row"><strong>深度:</strong>深度 {{ currentTooltipSlot.slotInout
                                    || '-' }}
                                </div>
                            </div>

                            <div class="tooltip-section">
                                <h4 class="section-title">属性信息</h4>
                                <div class="tooltip-row"><strong>库位类型:</strong> {{ currentTooltipSlot.makeName ||
                                    currentTooltipSlot.make || '-' }}</div>
                                <div class="tooltip-row"><strong>储位属性:</strong> {{ currentTooltipSlot.propertyName ||
                                    currentTooltipSlot.property || '-' }}</div>
                                <div class="tooltip-row"><strong>储位处理:</strong> {{ currentTooltipSlot.handleName ||
                                    currentTooltipSlot.handle || '-' }}</div>
                                <div class="tooltip-row"><strong>储位环境:</strong> {{ currentTooltipSlot.environmentName ||
                                    currentTooltipSlot.environment || '-' }}</div>
                            </div>

                            <div class="tooltip-section" v-if="hasLockInfo(currentTooltipSlot)">
                                <h4 class="section-title">锁定状态</h4>
                                <div class="tooltip-row">
                                    <strong>入库锁定:</strong> 
                                    <span :style="getLockFlagStyle(currentTooltipSlot.slotImlockFlag)">
                                        {{ formatLockFlag(currentTooltipSlot.slotImlockFlag) }}
                                    </span>
                                </div>
                                <div class="tooltip-row">
                                    <strong>出库锁定:</strong> 
                                    <span :style="getLockFlagStyle(currentTooltipSlot.slotExlockFlag)">
                                        {{ formatLockFlag(currentTooltipSlot.slotExlockFlag) }}
                                    </span>
                                </div>
                                <div class="tooltip-row">
                                    <strong>是否屏蔽:</strong> 
                                    <span :style="getLockFlagStyle(currentTooltipSlot.slotCloseFlag)">
                                        {{ formatLockFlag(currentTooltipSlot.slotCloseFlag) }}
                                    </span>
                                </div>
                            </div>

                            <div class="tooltip-section" v-if="currentTooltipSlot.remark">
                                <h4 class="section-title">备注</h4>
                                <div class="tooltip-row">{{ currentTooltipSlot.remark }}</div>
                            </div>
                        </div>
                    </div>

                    <!-- 图例说明 -->
                    <div class="legend-panel">
                        <table class="legend-table">
                            <tr v-for="status in slotStatuses" :key="status.value">
                                <td>
                                    <div class="legend-color" :style="getStatusStyle(status.value, false)"
                                        :class="getSlotStatusClass(status.value)"></div>
                                </td>
                                <td>{{ status.label }}</td>
                            </tr>
                        </table>
                    </div>
                </div>

                <!-- 统计图表区域 -->
                <div class="charts-container">
                    <div class="chart-item">
                        <div ref="histogramChart" class="chart"></div>
                    </div>
                    <div class="chart-item">
                        <div ref="pieChart" class="chart"></div>
                    </div>
                </div>
            </div>
        </el-card>

        <!-- 移库弹窗 -->
        <el-dialog v-model="moveDialogVisible" title="移库任务" width="400px" :before-close="handleMoveDialogClose">
            <el-form :model="moveForm" label-width="80px">
                <el-form-item label="移出库位:">
                    <el-input v-model="moveForm.outSlotCode" readonly />
                </el-form-item>
                <el-form-item label="移入库位:">
                    <div style="display: flex; gap: 10px;">
                        <el-input v-model="moveForm.inSlotCode" readonly />
                        <el-button @click="handleSelectInSlot">选择...</el-button>
                    </div>
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="handleMoveDialogClose">取消</el-button>
                <el-button type="primary" @click="handleConfirmMove">确认</el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted, nextTick } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import * as echarts from 'echarts'
import { useWmsBaseSlotApi } from '/@/api/base/baseSlot/wmsBaseSlot';

const wmsBaseSlotApi = useWmsBaseSlotApi();

// 响应式数据
const filterForm = reactive({
    warehouseId: 0,
    showType: 'layer',
    slotLayer: 1,
    slotLanewayId: 0,
    slotRow: 1,
    slotInout: 1,
})

const state = reactive({
    dropdownData: {
        warehouseId: [],
        slotLanewayId: [],
        layerId: [],
        rowId: []
    },
})

const houseOptions = ref([
    { id: 1, label: '立体库A区' },
    { id: 2, label: '平库B区' },
    { id: 3, label: '冷藏库C区' }
])

const layerOptions = ref([1, 2, 3, 4, 5, 6, 7, 8, 9, 10])
const galleryOptions = ref([])
const rowOptions = ref([])

const slotGrid = ref([])
const selectedSlot = ref('')
const slotInfos = ref([])
const moveDialogVisible = ref(false)
const moveForm = reactive({
    outSlotCode: '',
    inSlotCode: ''
})

// 自定义 Tooltip 相关
const showTooltip = ref(false)
const currentTooltipSlot = ref({})
const tooltipStyle = reactive({
    left: '0px',
    top: '0px'
})

// 图表实例
const histogramChart = ref(null)
const pieChart = ref(null)
let histogramInstance = null
let pieInstance = null

// 储位状态定义
const slotStatuses = ref([]);
// 颜色映射
const slotStatusColors = ref([]);

// 方法定义
const getSlotStatusClass = (status) => {
    const statusMap = {
        0: 'slot-empty',
        1: 'slot-occupied',
        2: 'slot-inbound',
        3: 'slot-outbound',
        4: 'slot-moving-in',
        5: 'slot-moving-out',
        6: 'slot-empty-carrier',
        7: 'slot-disabled',
        8: 'slot-wall',
        9: 'slot-error'
    }
    return statusMap[status] || 'slot-empty'
}

// 统一的样式获取方法
const getStatusStyle = (status, bool = true) => {
    const config = slotStatuses.value.find(item => item.value == status)
    if (!config) return {}

    const style = { backgroundColor: config.styleSetting }

    // 墙体特殊样式
    if (status === 8) {
        style.boxShadow = '2px 2px 1px #ffffff'
        style.width = '17.5px'
        style.height = '13px'
    }
    if (bool) {
        style.marginTop = filterForm.showType === 'layer' ? '' : '25px'
    }
    return style
}

// 锁定转换
const formatLockFlag = (flag) => {
    if (flag === null || flag === undefined) return '-'
    return flag === 1 ? '是' : '否'
}
// 是否锁定判断
const hasLockInfo = (slot) => {
    return slot.slotImlockFlag !== null || slot.slotExlockFlag !== null || slot.slotCloseFlag !== null
}
// 获取锁定状态的样式
const getLockFlagStyle = (flag) => {
    if (flag === 1) {
        return { color: '#ff4d4f', fontWeight: 'bold' }; // 红色加粗
    }
    return { color: '#52c41a' }; // 绿色表示安全
}

// 定义定时器变量
let hideTimer = null
// 记录坐标
const mousePosition = ref({});

// 自定义 Tooltip 方法
const handleSlotMouseEnter = (slot, event) => {
    mousePosition.value = event;
}

const openSlotInfo = (slot, event) => {
    // 清除之前的隐藏定时器
    if (hideTimer) {
        clearTimeout(hideTimer)
        hideTimer = null
    }

    currentTooltipSlot.value = slot
    updateTooltipPosition(event)
    showTooltip.value = true
}

const handleSlotMouseLeave = () => {
    // 设置延迟隐藏
    hideTimer = setTimeout(() => {
        showTooltip.value = false
        hideTimer = null
    }, 1) // 500ms 延迟
}

const updateTooltipPosition = (event) => {
    const tooltipOffset = 10 // 距离鼠标的偏移量

    // 获取鼠标位置
    const x = event.clientX
    const y = event.clientY

    // 设置 tooltip 位置
    tooltipStyle.left = `${x + tooltipOffset}px`
    tooltipStyle.top = `${y + tooltipOffset}px`

    // 防止 tooltip 超出屏幕边界
    nextTick(() => {
        const tooltipEl = document.querySelector('.custom-tooltip')
        if (tooltipEl) {
            const rect = tooltipEl.getBoundingClientRect()
            const viewportWidth = window.innerWidth
            const viewportHeight = window.innerHeight

            // 检查右边界
            if (rect.right > viewportWidth) {
                tooltipStyle.left = `${x - rect.width - tooltipOffset}px`
            }

            // 检查下边界
            if (rect.bottom > viewportHeight) {
                tooltipStyle.top = `${y - rect.height - tooltipOffset}px`
            }
        }
    })
}

// 初始化数据
const InitData = async () => {
    // 初始化数据
    const data = await wmsBaseSlotApi.getDropdownData(true).then(res => res.data.result) ?? {};
    // 获取仓库
    // 仓库数据（过滤掉类型为“05”的仓库） 05 = 平库
    state.dropdownData.warehouseId = data.warehouseId.filter(m => m.type !== "05") || [];
    // 获取巷道
    state.dropdownData.slotLanewayId = data.slotLanewayId;
    if(filterForm.warehouseId === 0 && state.dropdownData.warehouseId.length > 0){
        filterForm.warehouseId = state.dropdownData.warehouseId[0].value

    }
    const layerData = await wmsBaseSlotApi.getDropdownDataLayer(filterForm.warehouseId).then(res => res.data.result) ?? {};
    state.dropdownData.layerId = layerData.layerData;
    filterForm.slotLayer = layerData.layerData[0].value

    loadLayerData()
    // 监听窗口大小变化，重新渲染图表
    window.addEventListener('resize', handleResize)
}

// 生命周期
onMounted(async () => {
    await InitData();
})

onUnmounted(() => {
    window.removeEventListener('resize', handleResize)
    if (histogramInstance) {
        histogramInstance.dispose()
    }
    if (pieInstance) {
        pieInstance.dispose()
    }
})

const handleHouseChange = () => {3
    InitData();
    // 根据仓库类型加载不同的数据
    if (filterForm.showType === 'layer') {
        loadLayerData()
    } else {
        loadGalleryData()
        loadRowData()
    }
}

const handleShowTypeChange = () => {
    if (filterForm.showType === 'layer') {
        loadLayerData()
    } else {
        loadGalleryData()
        loadRowData()
        loadRowViewData()
    }
}

const handleLayerChange = () => {
    loadLayerData()
}

const handleGalleryChange = () => {
    loadRowData()
    loadRowViewData()
}

const handleRowChange = (e) => {
    // 更新最新储位深度,不更新会有滞后性
    filterForm.slotInout = 1;
    loadRowViewData()
}

const handleSlotClick = async (slot) => {
    selectedSlot.value = slot.id
    await loadSlotInfo(slot)
}

const handleSlotRightClick = (slot) => {
    if (getSlotStatusClass(slot.status) === 'slot-occupied') {
        moveForm.outSlotCode = slot.slotCode
        moveForm.inSlotCode = ''
        moveDialogVisible.value = true
    } else {
        ElMessage.warning('该库位无托盘或使用中，不可移库！')
    }
}

const handleSelectInSlot = () => {
    moveDialogVisible.value = false
    ElMessage.info('请在网格中选择目标库位')
}

const handleMoveDialogClose = () => {
    moveDialogVisible.value = false
    moveForm.outSlotCode = ''
    moveForm.inSlotCode = ''
}

const handleConfirmMove = async () => {
    if (!moveForm.inSlotCode) {
        ElMessage.warning('请选择移入库位')
        return
    }

    try {
        // 模拟API调用
        await new Promise(resolve => setTimeout(resolve, 1000))
        ElMessage.success('移库任务创建成功')
        moveDialogVisible.value = false
        moveForm.outSlotCode = ''
        moveForm.inSlotCode = ''

        // 刷新数据
        if (filterForm.showType === 'layer') {
            loadLayerData()
        } else {
            loadRowViewData()
        }
    } catch (error) {
        ElMessage.error('移库任务创建失败')
    }
}

// 数据加载方法
const loadLayerData = async () => {
    // API调用获取层数据
    await wmsBaseSlotApi.getSlotLegendList(filterForm).then(res => {
        const data = res.data.result ?? [];
        slotGrid.value = data.data;
        slotStatuses.value = data.slotStatusList || [];
        slotStatusColors.value = data.chartData.color || [];
        // 使用 nextTick 确保 DOM 更新完成
        nextTick(() => {
            initHistogramChart(data.chartData)
            initPieChart(data.pieData)
        })
    });
}

const loadGalleryData = () => {
    // 模拟巷道数据
    galleryOptions.value = state.dropdownData.slotLanewayId;
    if (!filterForm.slotLanewayId && galleryOptions.value.length > 0) {
        filterForm.slotLanewayId = state.dropdownData.slotLanewayId[0].value
    }

    loadRowData()
}

// 加载排数据
const loadRowData = async () => {

    // 获取排数据
    await wmsBaseSlotApi.getDropdownDataRow(filterForm.warehouseId, filterForm.slotLanewayId).then(res => {
        const data = res.data.result ?? {};
        state.dropdownData.rowId = data.rowData;
    });
    rowOptions.value = state.dropdownData.rowId;
    filterForm.slotInout = 1;
    if (rowOptions.value.length > 0) {
        filterForm.slotRow = rowOptions.value[0].value
    }
}

const loadRowViewData = () => {
    // 模拟按排展示数据
    loadLayerData();
}

const loadSlotInfo = async (slot) => {
    const res = await wmsBaseSlotApi.getSlotDetail(slot.id).then(res => res.data.result);
    // 模拟获取储位信息
    slotInfos.value = res
    openSlotInfo(res[0], mousePosition.value);
}

// 图表初始化
const initHistogramChart = (data) => {
    if (!histogramInstance && histogramChart.value) {
        histogramInstance = echarts.init(histogramChart.value)
    }

    if (histogramInstance) {
        const option = {
            title: {
                text: filterForm.showType === 'layer' ? `${filterForm.slotLayer}层储位状态` : '储位状态统计',
                left: 'center'
            },
            tooltip: {
                trigger: 'axis'
            },
            xAxis: {
                type: 'category',
                data: data.categories,
                axisLabel: {
                    rotate: 45
                }
            },
            yAxis: {
                type: 'value'
            },
            series: [{
                data: data.values,
                type: 'bar',
                itemStyle: {
                    color: (params) => {
                        const colorMap = data.color
                        return colorMap[params.dataIndex] || '#5470c6'
                    }
                },
                label: {
                    show: true,
                    position: 'top'
                }
            }]
        }
        histogramInstance.setOption(option)
        histogramInstance.resize() // 确保图表正确调整大小
    }
}

const initPieChart = (data) => {
    if (!pieInstance && pieChart.value) {
        pieInstance = echarts.init(pieChart.value)
    }

    if (pieInstance) {
        const option = {
            title: {
                text: '储位状态占比',
                left: 'center'
            },
            tooltip: {
                trigger: 'item',
                formatter: '{a} <br/>{b}: {c} ({d}%)'
            },
            legend: {
                orient: 'vertical',
                left: 'left',
                top: 'middle'
            },
            series: [{
                label: '储位状态',
                name: '',
                type: 'pie',
                radius: ['40%', '70%'],
                avoidLabelOverlap: false,
                itemStyle: {
                    borderColor: '#fff',
                    borderWidth: 2,
                    normal: {
                        color: function (colors) {
                            var colorList = slotStatusColors.value;
                            return colorList[colors.dataIndex];
                        }
                    },
                },
                emphasis: {
                    label: {
                        show: true,
                        fontSize: '18',
                        fontWeight: 'bold'
                    }
                },
                labelLine: {
                    show: false
                },
                data: data
            }]
        }
        pieInstance.setOption(option)
        pieInstance.resize() // 确保图表正确调整大小
    }
}

const handleResize = () => {
    if (histogramInstance) {
        histogramInstance.resize()
    }
    if (pieInstance) {
        pieInstance.resize()
    }
}
</script>

<style scoped>
.wms-storage-legend {
    padding: 20px;
    background-color: #f5f7fa;
    height: 100vh;
    display: flex;
    flex-direction: column;
    overflow: hidden;
}

.filter-card {
    margin-bottom: 10px;
    flex-shrink: 0;
}

.main-content {
    flex: 1;
    display: flex;
    flex-direction: column;
    overflow: scroll;
    min-height: 0;
    /* 重要：防止flex项目溢出 */
}

.slot-info-carousel {
    margin-bottom: 20px;
    border: 1px solid #e4e7ed;
    border-radius: 4px;
    overflow: hidden;
    flex-shrink: 0;
}

.content-wrapper {
    flex: 1;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    min-height: 0;
}

.slot-display-container {
    display: flex;
    gap: 20px;
    margin-bottom: 20px;
    flex: 1;
    min-height: 0;
    overflow: hidden;
    position: relative;
}

.slot-grid-container {
    flex: 1;
    overflow: auto;
    border: 1px solid #e4e7ed;
    border-radius: 4px;
    padding: 10px;
    background-color: white;
    min-height: 0;
}

.slot-grid {
    border-collapse: separate;
    border-spacing: 2px;
}

.slot-grid td {
    padding: 2px;
}

.slot-cell {
    width: 17.5px;
    height: 13px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.3s;
    border: 2px solid transparent;
    box-shadow: 2px 2px 1px #888888;
    position: relative;
}

.slot-cell:hover {
    /* transform: translateY(-2px); */
    background-color: rgb(0, 13, 128);
    box-shadow: 4px 4px 2px #888888;
}

.slot-cell.active {
    background-color: rgb(0, 13, 128) !important;
}

.slot-code {
    font-size: 10px;
    color: white;
    font-weight: bold;
    text-shadow: 1px 1px 1px rgba(0, 0, 0, 0.5);
}

/* 自定义 Tooltip 样式 */
.custom-tooltip {
    position: fixed;
    z-index: 9999;
    background: rgba(0, 0, 0, 0.85);
    color: white;
    padding: 8px 12px;
    border-radius: 4px;
    font-size: 12px;
    line-height: 1.4;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
    pointer-events: none;
    transition: opacity 0.1s ease;
    max-width: 250px;
    backdrop-filter: blur(4px);
}

.tooltip-content {
    display: flex;
    flex-direction: column;
}

.tooltip-row {
    margin-bottom: 2px;
    display: flex;
    justify-content: space-between;
}

.tooltip-row:last-child {
    margin-bottom: 0;
}

.tooltip-row strong {
    color: #409eff;
    margin-right: 6px;
    white-space: nowrap;
}

/* 储位状态颜色 */
.slot-empty {
    background-color: #ff7f50;
}

.slot-occupied {
    background-color: #89cffa;
}

.slot-inbound {
    background-color: #e599e2;
}

.slot-outbound {
    background-color: #58d758;
}

.slot-moving-in {
    background-color: #7ba5f0;
}

.slot-moving-out {
    background-color: #ff81c0;
}

.slot-empty-carrier {
    background-color: #daa5e8;
}

.slot-disabled {
    background-color: #d47575;
}

.slot-wall {
    background-color: #ffffff;
    box-shadow: 2px 2px 1px #ffffff;
    width: 17.5px;
    height: 13px;
}

.slot-error {
    background-color: red;
}

.legend-panel {
    width: 200px;
    border: 1px solid #e4e7ed;
    border-radius: 4px;
    padding: 15px;
    background-color: white;
    flex-shrink: 0;
    overflow: auto;
}

.legend-table {
    width: 100%;
}

.legend-table td {
    padding: 5px 0;
}

.legend-color {
    width: 20px;
    height: 15px;
    border-radius: 2px;
    border: 1px solid #ddd;
}

.charts-container {
    display: flex;
    gap: 20px;
    height: 500px;
    /* 固定图表区域高度 */
    flex-shrink: 0;
    /* 防止图表区域被压缩 */
}

.chart-item {
    flex: 1;
    height: 100%;
    border: 1px solid #e4e7ed;
    border-radius: 4px;
    padding: 10px;
    background-color: white;
}

.chart {
    width: 100%;
    height: 100%;
    min-height: 0;
}

/* 响应式调整 */
@media (max-width: 1200px) {
    .slot-display-container {
        flex-direction: column;
    }

    .legend-panel {
        width: 100%;
        height: auto;
    }

    .charts-container {
        flex-direction: column;
        height: 600px;
        /* 在移动端增加图表高度 */
    }
}

@media (max-width: 768px) {
    .carousel-content {
        flex-direction: column;
        height: auto;
        padding: 10px 0;
    }

    .info-item {
        margin-bottom: 5px;
    }

    .slot-cell {
        width: 45px;
        height: 35px;
    }

    .slot-code {
        font-size: 8px;
    }

    .charts-container {
        height: 500px;
        /* 在更小的屏幕上调整图表高度 */
    }

    .custom-tooltip {
        max-width: 200px;
        font-size: 11px;
    }
}
</style>