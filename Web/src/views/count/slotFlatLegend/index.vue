<template>
    <div class="wms-storage-legend">
        <!-- 筛选条件区域 -->
        <el-card class="filter-card">
            <el-form :model="filterForm" :inline="true">
                <el-form-item label="库房">
                    <el-select clearable filterable v-model="filterForm.warehouseId" placeholder="请选择所属仓库"
                        @change="handleWarehouseChange">
                        <el-option v-for="(item, index) in state.dropdownData.warehouseId ?? []" :key="index"
                            :value="item.value" :label="item.label" />
                    </el-select>
                </el-form-item>

                <el-form-item label="层数">
                    <el-select clearable filterable v-model="filterForm.slotLayer" placeholder="请选择层数"
                        @change="loadLayerData">
                        <el-option v-for="(item, index) in state.dropdownData.layerId ?? []" :key="index"
                            :value="item.value" :label="item.label" />
                    </el-select>
                </el-form-item>
            </el-form>
        </el-card>

        <!-- 第一行：统计概览卡片 -->
        <el-card class="overview-card" v-if="storageStats.totalSlots > 0">
            <div class="overview-container">
                <div class="overview-item">
                    <div class="overview-icon total">
                        <el-icon>
                            <Box />
                        </el-icon>
                        <i class="el-icon-office-building"></i>
                    </div>
                    <div class="overview-content">
                        <div class="overview-value">{{ storageStats.totalSlots }}</div>
                        <div class="overview-label">总库位数</div>
                    </div>
                </div>
                <div class="overview-item">
                    <div class="overview-icon used">
                        <el-icon>
                            <Remove />
                        </el-icon>
                        <i class="el-icon-box"></i>
                    </div>
                    <div class="overview-content">
                        <div class="overview-value">{{ storageStats.usedSlots }}</div>
                        <div class="overview-label">已使用</div>
                    </div>
                </div>
                <div class="overview-item">
                    <div class="overview-icon available">
                        <el-icon>
                            <VideoPause />
                        </el-icon>
                        <i class="el-icon-folder-opened"></i>
                    </div>
                    <div class="overview-content">
                        <div class="overview-value">{{ storageStats.availableSlots }}</div>
                        <div class="overview-label">空闲</div>
                    </div>
                </div>
                <div class="overview-item">
                    <div class="overview-icon utilization">
                        <el-icon>
                            <TrendCharts />
                        </el-icon>
                        <i class="el-icon-data-analysis"></i>
                    </div>
                    <div class="overview-content">
                        <div class="overview-value">{{ storageStats.utilizationRate }}%</div>
                        <div class="overview-label">利用率</div>
                        <div class="utilization-progress">
                            <div class="progress-bar">
                                <div class="progress-fill" :style="{ width: storageStats.utilizationRate + '%' }"></div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="overview-item">
                    <div class="overview-icon operating">
                        <el-icon>
                            <Refresh />
                        </el-icon>
                        <i class="el-icon-truck"></i>
                    </div>
                    <div class="overview-content">
                        <div class="overview-value">{{ storageStats.operatingSlots }}</div>
                        <div class="overview-label">作业中</div>
                    </div>
                </div>
                <div class="overview-item">
                    <div class="overview-icon abnormal">
                        <el-icon>
                            <Warning />
                        </el-icon>
                        <i class="el-icon-warning-outline"></i>
                    </div>
                    <div class="overview-content">
                        <div class="overview-value">{{ storageStats.abnormalSlots }}</div>
                        <div class="overview-label">异常库位</div>
                    </div>
                </div>
            </div>
        </el-card>


        <!-- 储位信息展示区域 -->
        <el-card class="main-content">
            <div class="content-wrapper">
                <!-- 储位图例展示 -->
                <div class="slot-display-container">
                    <div class="slot-grid-container">
                        <!-- 平库网格按行渲染 -->
                        <div v-for="(row, rowIndex) in slotGrid" :key="`${filterForm.slotLayer}-${rowIndex}`" class="grid-row">
                            <div v-for="(slot, colIndex) in row" :key="colIndex" class="slot-cell" :class="[
                                getSlotStatusClass(slot),
                                {
                                    active: selectedSlot === slot?.id,
                                    'car-move-effect': slot?.isSxcLocation && carMovePositions.has(`${slot.slotRow}-${slot.slotColumn}`),
                                    'high-utilization': getSlotUtilizationLevel(slot) === 'high',
                                    'medium-utilization': getSlotUtilizationLevel(slot) === 'medium'
                                }
                            ]" :style="getStatusStyle(slot)"
                                @click="handleSlotClick(slot, $event)" @mouseleave="handleSlotMouseLeave">
                                <!-- 跑道显示 -->
                                <span v-if="slot?.make === '02'" class="runway-text"></span>
                                <!-- 墙体显示 -->
                                <span v-else-if="slot?.slotStatus === 8" class="wall-text"></span>
                                <!-- 正常库位显示行列信息 -->
                                <!-- <template v-else-if="slot?.isSxcLocation != 1">
                                    <span class="slot-label">{{ slot?.slotRow }}</span>
                                    <span class="slot-separator">/</span>
                                    <span class="slot-label">{{ slot?.slotColumn }}</span> -->
                                    <!-- 库位利用率指示器 -->
                                    <!-- <div v-if="showUtilizationIndicator(slot)" class="utilization-indicator"
                                        :class="getUtilizationIndicatorClass(slot)">
                                    </div>
                                </template> -->
                                <!-- 四向车位置 -->
                                <span v-if="slot?.isSxcLocation" class="vehicle-icon">小车</span>
                            </div>
                        </div>
                    </div>

                    <!-- 自定义 Tooltip -->
                    <div v-show="showTooltip" class="custom-tooltip" :style="tooltipStyle">
                        <div class="tooltip-content">
                            <!-- 第一列：基本信息和位置信息 -->
                            <div class="tooltip-column">
                                <!-- 基本信息（只显示一次） -->
                                <div class="tooltip-section">
                                    <h4 class="section-title">基本信息</h4>
                                    <div class="tooltip-row">
                                        <span class="tooltip-label">仓位编码:</span>
                                        <span class="tooltip-value">{{ getCommonValue(currentTooltipSlot, 'slotCode')
                                            }}</span>
                                    </div>
                                    <div class="tooltip-row">
                                        <span class="tooltip-label">仓位状态:</span>
                                        <span class="tooltip-value">{{ getStatusText(getFirstSlot(currentTooltipSlot))
                                            }}</span>
                                    </div>
                                    <!-- 占位 水平对齐 -->
                                    <div class="tooltip-row">
                                        <span class="tooltip-label">深度:</span>
                                        <span class="tooltip-value">{{ !getCommonValue(currentTooltipSlot,
                                            'slotInoutName') ? '无' :
                                            getCommonValue(currentTooltipSlot, 'slotInoutName') }}</span>
                                    </div>
                                    <!-- 占位 水平对齐 -->
                                    <div class="tooltip-row">
                                        <span class="tooltip-label">&nbsp;</span>
                                    </div>
                                </div>

                                <!-- 位置信息（只显示一次） -->
                                <div class="tooltip-section">
                                    <h4 class="section-title">位置信息</h4>
                                    <div class="tooltip-row">
                                        <span class="tooltip-label">巷道:</span>
                                        <span class="tooltip-value">{{ getCommonValue(currentTooltipSlot,
                                            'slotLanewayIdFkColumn') }}</span>
                                    </div>
                                    <div class="tooltip-row">
                                        <span class="tooltip-label">行:</span>
                                        <span class="tooltip-value">{{ getCommonValue(currentTooltipSlot, 'slotRow')
                                            }}</span>
                                    </div>
                                    <div class="tooltip-row">
                                        <span class="tooltip-label">列:</span>
                                        <span class="tooltip-value">{{ getCommonValue(currentTooltipSlot, 'slotColumn')
                                            }}</span>
                                    </div>
                                    <div class="tooltip-row">
                                        <span class="tooltip-label">层:</span>
                                        <span class="tooltip-value">第{{ filterForm.slotLayer || '-' }}层</span>
                                    </div>
                                </div>
                            </div>

                            <!-- 第二列：属性信息和锁定状态 -->
                            <div class="tooltip-column">
                                <!-- 属性信息或占位符 -->
                                <div v-if="getCommonValue(currentTooltipSlot, 'makeName') || getCommonValue(currentTooltipSlot, 'propertyName') || getCommonValue(currentTooltipSlot, 'handleName') || getCommonValue(currentTooltipSlot, 'environmentName')"
                                    class="tooltip-section">
                                    <h4 class="section-title">属性信息</h4>
                                    <div v-if="getCommonValue(currentTooltipSlot, 'makeName')" class="tooltip-row">
                                        <span class="tooltip-label">库位类型:</span>
                                        <span class="tooltip-value">{{ getCommonValue(currentTooltipSlot, 'makeName') ||
                                            currentTooltipSlot.make || '-' }}</span>
                                    </div>
                                    <div v-if="getCommonValue(currentTooltipSlot, 'propertyName')" class="tooltip-row">
                                        <span class="tooltip-label">储位属性:</span>
                                        <span class="tooltip-value">{{ getCommonValue(currentTooltipSlot,
                                            'propertyName') ||
                                            currentTooltipSlot.property || '-' }}</span>
                                    </div>
                                    <div v-if="getCommonValue(currentTooltipSlot, 'handleName')" class="tooltip-row">
                                        <span class="tooltip-label">储位处理:</span>
                                        <span class="tooltip-value">{{ getCommonValue(currentTooltipSlot, 'handleName')
                                            ||
                                            currentTooltipSlot.handle || '-' }}</span>
                                    </div>
                                    <div v-if="getCommonValue(currentTooltipSlot, 'environmentName')"
                                        class="tooltip-row">
                                        <span class="tooltip-label">储位环境:</span>
                                        <span class="tooltip-value">{{ getCommonValue(currentTooltipSlot,
                                            'environmentName') ||
                                            currentTooltipSlot.environment || '-' }}</span>
                                    </div>
                                </div>
                                <!-- 占位符：当属性信息不存在时显示，确保与第一列的位置信息对齐 -->
                                <div v-else class="tooltip-section placeholder"></div>

                                <div class="tooltip-section" v-if="hasLockInfo(currentTooltipSlot[0])">
                                    <h4 class="section-title">锁定状态</h4>
                                    <div class="tooltip-row">
                                        <span class="tooltip-label">入库锁定:</span>
                                        <span :style="getLockFlagStyle(currentTooltipSlot[0].slotImlockFlag)">
                                            {{ formatLockFlag(currentTooltipSlot[0].slotImlockFlag) }}
                                        </span>
                                    </div>
                                    <div class="tooltip-row">
                                        <span class="tooltip-label">出库锁定:</span>
                                        <span :style="getLockFlagStyle(currentTooltipSlot[0].slotExlockFlag)">
                                            {{ formatLockFlag(currentTooltipSlot[0].slotExlockFlag) }}
                                        </span>
                                    </div>
                                    <div class="tooltip-row">
                                        <span class="tooltip-label">是否屏蔽:</span>
                                        <span :style="getLockFlagStyle(currentTooltipSlot[0].slotCloseFlag)">
                                            {{ formatLockFlag(currentTooltipSlot[0].slotCloseFlag) }}
                                        </span>
                                    </div>
                                </div>
                            </div>

                            <!-- 物料信息单独占一行 -->
                            <div class="tooltip-section full-width" v-if="hasDifferentMaterialInfo(currentTooltipSlot)">
                                <h4 class="section-title">物料信息</h4>
                                <div class="material-list">
                                    <div v-for="(slotItem, index) in currentTooltipSlot" :key="index"
                                        class="material-item">
                                        <div class="material-header">
                                            <span class="material-index">物料 {{ index + 1 }}</span>
                                        </div>
                                        <div class="material-details">
                                            <div class="tooltip-row" v-if="slotItem.materialName">
                                                <span class="tooltip-label">物料名称:</span>
                                                <span class="tooltip-value">{{ slotItem.materialName }}</span>
                                            </div>
                                            <div class="tooltip-row" v-if="slotItem.batchNo">
                                                <span class="tooltip-label">物料批次:</span>
                                                <span class="tooltip-value">{{ slotItem.batchNo }}</span>
                                            </div>
                                            <div class="tooltip-row" v-if="slotItem.weight">
                                                <span class="tooltip-label">数量:</span>
                                                <span class="tooltip-value">{{ slotItem.weight }}</span>
                                            </div>
                                            <div class="tooltip-row" v-if="slotItem.stockCode">
                                                <span class="tooltip-label">载具号:</span>
                                                <span class="tooltip-value">{{ slotItem.stockCode }}</span>
                                            </div>
                                        </div>
                                        <!-- 分隔线（不是最后一个物料时显示） -->
                                        <div v-if="index < currentTooltipSlot.length - 1" class="material-divider">
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- 单物料信息（兼容单个物料的情况） -->
                            <div class="tooltip-section full-width"
                                v-else-if="hasMaterialInfo(getFirstSlot(currentTooltipSlot))">
                                <h4 class="section-title">物料信息</h4>
                                <div class="material-details">
                                    <div class="tooltip-row" v-if="getFirstSlot(currentTooltipSlot).materialName">
                                        <span class="tooltip-label">物料名称:</span>
                                        <span class="tooltip-value">{{ getFirstSlot(currentTooltipSlot).materialName
                                            }}</span>
                                    </div>
                                    <div class="tooltip-row" v-if="getFirstSlot(currentTooltipSlot).batchNo">
                                        <span class="tooltip-label">物料批次:</span>
                                        <span class="tooltip-value">{{ getFirstSlot(currentTooltipSlot).batchNo
                                            }}</span>
                                    </div>
                                    <div class="tooltip-row" v-if="getFirstSlot(currentTooltipSlot).weight">
                                        <span class="tooltip-label">数量:</span>
                                        <span class="tooltip-value">{{ getFirstSlot(currentTooltipSlot).weight }}</span>
                                    </div>
                                    <div class="tooltip-row" v-if="getFirstSlot(currentTooltipSlot).stockCode">
                                        <span class="tooltip-label">载具号:</span>
                                        <span class="tooltip-value">{{ getFirstSlot(currentTooltipSlot).stockCode
                                            }}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- 图例说明 -->
                    <div class="legend-panel">
                        <table class="legend-table">
                            <tr v-for="legend in legendData" :key="legend.type">
                                <td>
                                    <div class="legend-color" :style="getLegendStyle(legend.type)"></div>
                                </td>
                                <td class="legend-text">{{ legend.label }}</td>
                            </tr>
                        </table>
                    </div>
                </div>
            </div>
        </el-card>


        <!-- 第二行：图表区域 -->
        <div class="charts-section" v-if="storageStats.totalSlots > 0">
            <!-- 柱状图卡片 -->
            <el-card class="chart-card">
                <template #header>
                    <div class="chart-header">
                        <span class="chart-title">库位状态分布</span>
                        <div class="chart-actions">
                            <el-radio-group v-model="barChartType" size="small">
                                <el-radio-button label="status">按状态</el-radio-button>
                                <el-radio-button label="utilization">按利用率</el-radio-button>
                            </el-radio-group>
                        </div>
                    </div>
                </template>
                <div ref="barChartRef" class="chart-container"></div>
            </el-card>

            <!-- 饼图卡片 -->
            <el-card class="chart-card">
                <template #header>
                    <div class="chart-header">
                        <span class="chart-title">库位状态占比</span>
                        <div class="chart-actions">
                            <el-tooltip content="切换环形图" placement="top">
                                <el-switch v-model="isPieRing" size="small" active-text="环形" inactive-text="饼状" />
                            </el-tooltip>
                        </div>
                    </div>
                </template>
                <div ref="pieChartRef" class="chart-container"></div>
            </el-card>
        </div>
    </div>
</template>

<script setup>
import { ref, reactive, onMounted, nextTick, computed, onUnmounted, watch } from 'vue'
import { ElMessage } from 'element-plus'
import * as echarts from 'echarts'
import { useWmsBaseSlotApi } from '/@/api/base/baseSlot/wmsBaseSlot';
const wmsBaseSlotApi = useWmsBaseSlotApi();

// 当前选中的储位详细信息
const slotDetail = ref({});
const cellDimensions = reactive({
    width: '34px',
    height: '16px',
    fontSize: '8px'
})

// 响应式数据
const filterForm = reactive({
    warehouseId: null,
    slotLayer: 1,
})

const state = reactive({
    dropdownData: {
        warehouseId: [],
        layerId: [],
    },
})

// 仓位网格数据
const slotGrid = ref([])
const selectedSlot = ref('')
const currentTooltipSlot = ref({})
const showTooltip = ref(false)
const tooltipStyle = reactive({
    left: '0px',
    top: '0px'
})

// 动画控制相关变量
const isFirstLoad = ref(true)
const shouldAnimate = ref(true)
const gridLoaded = ref(false)

// 小车位置追踪
const previousCarPositions = ref(new Set())
const carMovePositions = ref(new Set())

// 库位利用率统计
const storageStats = reactive({
    totalSlots: 0,
    usedSlots: 0,
    availableSlots: 0,
    operatingSlots: 0,
    abnormalSlots: 0,
    utilizationRate: 0
})

// 图表相关
const barChartRef = ref(null)
const pieChartRef = ref(null)
let barChart = null
let pieChart = null
const barChartType = ref('status') // status: 按状态, utilization: 按利用率
const isPieRing = ref(false)

// 图表数据
const barChartData = ref([])
const pieChartData = ref([])

// 图例数据
const legendData = ref([
    { type: 'runway', label: '跑道' },
    { type: 'fourWayCar', label: '四向车位置' },
    { type: 'empty', label: '空仓位' },
    { type: 'hasGoods', label: '有物品' },
    { type: 'movingOut', label: '移出中' },
    { type: 'movingIn', label: '移入中' },
    { type: 'outbound', label: '出库中' },
    { type: 'shielded', label: '已屏蔽' },
    { type: 'inbound', label: '入库中' },
    { type: 'emptyCarrier', label: '空载具' },
    { type: 'wall', label: '墙体' },
])

// 状态映射配置
const statusConfig = {
    // 跑道
    runway: {
        condition: (slot) => slot?.make === '02',
        style: { backgroundColor: '#ffffff', color: '#333' },
        text: '跑道'
    },
    // 四向车位置
    fourWayCar: {
        condition: (slot) => slot?.isSxcLocation,
        style: { backgroundColor: '#75b6d4', color: '#fff' },
        text: '四向车位置'
    },
    // 已屏蔽
    shielded: {
        condition: (slot) => slot?.slotCloseFlag === 1,
        style: { backgroundColor: '#d47575', color: '#fff' },
        text: '已屏蔽'
    },
    // 移出中
    movingOut: {
        condition: (slot) => slot?.slotStatus === 5, // 根据实际状态值调整
        style: { backgroundColor: '#ff81c1', color: '#fff' },
        text: '移出中'
    },
    // 移入中
    movingIn: {
        condition: (slot) => slot?.slotStatus === 4, // 根据实际状态值调整
        style: { backgroundColor: '#7ba5f0', color: '#fff' },
        text: '移入中'
    },
    // 出库中
    outbound: {
        condition: (slot) => slot?.slotStatus === 3,
        style: { backgroundColor: '#58d758', color: '#333' },
        text: '出库中'
    },
    // 入库中
    inbound: {
        condition: (slot) => slot?.slotStatus === 2, // 根据实际状态值调整
        style: { backgroundColor: '#e599e2', color: '#333' },
        text: '入库中'
    },
    // 空载具
    emptyCarrier: {
        condition: (slot) => slot?.slotStatus === 6,
        style: { backgroundColor: '#3d42c3', color: '#fff' },
        text: '空载具'
    },
    // 墙体
    wall: {
        condition: (slot) => slot?.slotStatus === 8,
        style: { backgroundColor: '#808080', color: '#fff' },
        text: '墙体'
    },
    // 有物品
    hasGoods: {
        condition: (slot) => slot?.slotStatus === 1,
        style: { backgroundColor: '#89cffa', color: '#333' },
        text: '有物品'
    },
    // 空库位
    empty: {
        condition: (slot) => !slot?.slotStatus || slot?.slotStatus === 0,
        style: { backgroundColor: '#ff7f50', color: '#333' },
        text: '空库位'
    }
}
// 库位详细信息处理模块

const getFirstSlot = (slots) => {
    return Array.isArray(slots) && slots.length > 0 ? slots[0] : slots;
};
const getCommonValue = (slots, key) => {
    if (!Array.isArray(slots) || slots.length === 0) return '-';

    const firstValue = slots[0][key];
    const allSame = slots.every(slot => slot[key] === firstValue);

    return allSame ? firstValue || '' : '';
};

const hasDifferentMaterialInfo = (slots) => {
    if (!Array.isArray(slots) || slots.length <= 1) return false;

    // 检查物料相关信息是否不同
    return slots.some((slot, index) => {
        if (index === 0) return false;
        const prevSlot = slots[index - 1];
        return (
            slot.materialName !== prevSlot.materialName ||
            slot.batchNo !== prevSlot.batchNo ||
            slot.weight !== prevSlot.weight ||
            slot.stockCode !== prevSlot.stockCode
        );
    });
};

const hasMaterialInfo = (slot) => {
    if (!slot) return false;
    return slot.materialName || slot.batchNo || slot.weight || slot.stockCode;
};

// 计算库位利用率统计
const calculateStorageStats = (slots) => {
    let total = 0;
    let used = 0;
    let available = 0;
    let operating = 0;
    let abnormal = 0;

    // 按类型统计
    const typeStats = {
        empty: 0,
        hasGoods: 0,
        emptyCarrier: 0,
        movingOut: 0,
        movingIn: 0,
        outbound: 0,
        inbound: 0,
        shielded: 0,
        fourWayCar: 0
    };

    // 按利用率等级统计
    const utilizationStats = {
        high: 0,
        medium: 0,
        low: 0
    };

    slots.forEach(slot => {
        if (slot && slot.make !== '02' && slot.slotStatus !== 8) {
            total++;
            const slotType = getSlotType(slot);
            typeStats[slotType] = (typeStats[slotType] || 0) + 1;

            // 统计利用率等级
            const utilizationLevel = getSlotUtilizationLevel(slot);
            if (utilizationLevel) {
                utilizationStats[utilizationLevel]++;
            }

            if (slotType === 'empty') {
                available++;
            } else if (['hasGoods'].includes(slotType) || slot.slotStatus == 1) {
                used++;
            } else if (['movingOut', 'movingIn', 'outbound', 'inbound'].includes(slotType)) {
                operating++;
            } else if (slotType === 'shielded') {
                abnormal++;
            }
        }
    });

    const utilizationRate = total > 0 ? (((used + operating) / (total - abnormal)) * 100).toFixed(1) : 0;

    Object.assign(storageStats, {
        totalSlots: total,
        usedSlots: used,
        availableSlots: available,
        operatingSlots: operating,
        abnormalSlots: abnormal,
        utilizationRate: utilizationRate
    });

    // 更新图表数据
    updateChartData(typeStats, utilizationStats, total);
}

// 更新图表数据
const updateChartData = (typeStats, utilizationStats, total) => {
    // 更新柱状图数据
    if (barChartType.value === 'status') {
        barChartData.value = [
            { name: '空库位', value: typeStats.empty, color: '#ff7f50' },
            { name: '有物品', value: typeStats.hasGoods, color: '#89cffa' },
            { name: '空载具', value: typeStats.emptyCarrier, color: '#3d42c3' },
            { name: '作业中', value: typeStats.movingOut + typeStats.movingIn + typeStats.outbound + typeStats.inbound, color: '#7ba5f0' },
            { name: '已屏蔽', value: typeStats.shielded, color: '#d47575' },
            { name: '四向车位', value: typeStats.fourWayCar, color: '#75b6d4' }
        ].filter(item => item.value > 0);
    } else {
        barChartData.value = [
            { name: '高利用率', value: utilizationStats.high, color: '#f56c6c' },
            { name: '中利用率', value: utilizationStats.medium, color: '#e6a23c' },
            { name: '低利用率', value: utilizationStats.low, color: '#67c23a' }
        ].filter(item => item.value > 0);
    }

    // 更新饼图数据
    pieChartData.value = [
        { name: '空库位', value: typeStats.empty, color: '#ff7f50' },
        { name: '有物品', value: typeStats.hasGoods, color: '#89cffa' },
        { name: '空载具', value: typeStats.emptyCarrier, color: '#3d42c3' },
        { name: '作业中', value: typeStats.movingOut + typeStats.movingIn + typeStats.outbound + typeStats.inbound, color: '#7ba5f0' },
        { name: '已屏蔽', value: typeStats.shielded, color: '#d47575' },
        { name: '四向车位', value: typeStats.fourWayCar, color: '#75b6d4' }
    ].filter(item => item.value > 0);

    // 计算百分比
    pieChartData.value.forEach(item => {
        item.percentage = total > 0 ? ((item.value / total) * 100).toFixed(1) : 0;
    });

    updateBarChart();
    updatePieChart();
}

// 初始化图表
const initCharts = () => {
    if (!barChartRef.value || !pieChartRef.value) return;

    barChart = echarts.init(barChartRef.value);
    pieChart = echarts.init(pieChartRef.value);

    updateBarChart();
    updatePieChart();
}

// 更新柱状图
const updateBarChart = () => {
    if (!barChart || barChartData.value.length === 0) return;

    const option = {
        title: {
            text: barChartType.value === 'status' ? '' : '',
            left: 'center',
            textStyle: {
                fontSize: 14,
                fontWeight: 'bold'
            }
        },
        tooltip: {
            trigger: 'axis',
            axisPointer: {
                type: 'shadow'
            },
            formatter: '{b}: {c}'
        },
        grid: {
            left: '3%',
            right: '4%',
            bottom: '10%',
            top: '15%',
            containLabel: true
        },
        xAxis: {
            type: 'category',
            data: barChartData.value.map(item => item.name),
            axisLabel: {
                interval: 0,
                rotate: barChartData.value.length > 4 ? 45 : 0
            }
        },
        yAxis: {
            type: 'value',
            name: '数量'
        },
        series: [
            {
                name: barChartType.value === 'status' ? '库位状态' : '利用率',
                type: 'bar',
                data: barChartData.value.map(item => ({
                    value: item.value,
                    itemStyle: {
                        color: item.color
                    }
                })),
                barWidth: '60%',
                label: {
                    show: true,
                    position: 'top'
                }
            }
        ]
    };

    barChart.setOption(option);
}

// 更新饼图
const updatePieChart = () => {
    if (!pieChart || pieChartData.value.length === 0) return;

    const radius = isPieRing.value ? ['40%', '70%'] : '70%';

    const option = {
        title: {
            text: '库位状态占比',
            left: 'center',
            textStyle: {
                fontSize: 14,
                fontWeight: 'bold'
            }
        },
        tooltip: {
            trigger: 'item',
            formatter: '{a} <br/>{b}: {c} ({d}%)'
        },
        legend: {
            orient: 'vertical',
            right: 10,
            top: 'center',
            data: pieChartData.value.map(item => item.name)
        },
        series: [
            {
                name: '库位状态',
                type: 'pie',
                radius: radius,
                center: ['40%', '50%'],
                avoidLabelOverlap: false,
                itemStyle: {
                    borderColor: '#fff',
                    borderWidth: 2
                },
                label: {
                    show: true,
                    formatter: '{b}: {d}%',
                    fontSize: 12,
                    color: '#333'
                },
                emphasis: {
                    label: {
                        show: true,
                        fontSize: 14,
                        fontWeight: 'bold'
                    }
                },
                labelLine: {
                    show: true
                },
                data: pieChartData.value.map(item => ({
                    value: item.value,
                    name: item.name,
                    itemStyle: {
                        color: item.color
                    }
                }))
            }
        ]
    };

    pieChart.setOption(option);
}

// 响应窗口大小变化
const handleResize = () => {
    if (barChart) {
        barChart.resize();
    }
    if (pieChart) {
        pieChart.resize();
    }
}

// 监听图表类型变化
watch(barChartType, () => {
    calculateStorageStats(slotGrid.value.flat().filter(slot => slot && slot.make !== '02'));
});

// 监听饼图类型变化
watch(isPieRing, () => {
    updatePieChart();
});

// 获取库位利用率等级
const getSlotUtilizationLevel = (slot) => {
    if (!slot || slot.make === '02') {
        return null;
    }

    // 根据库位状态判断利用率
    const slotType = getSlotType(slot);

    if (slotType === 'hasGoods' || slot.slotStatus == 1) {
        // 有物品的库位视为高利用率
        return 'high';
    } else if (slotType === 'emptyCarrier') {
        return 'medium'; // 空载具视为中等利用率
    } else if (['movingOut', 'movingIn', 'outbound', 'inbound'].includes(slotType)) {
        return 'high'; // 作业中的库位视为高利用率
    } else if (slotType === 'empty') {
        return 'low'; // 空库位视为低利用率
    }

    return ''; // 默认低利用率
}

// 是否显示利用率指示器
const showUtilizationIndicator = (slot) => {
    return slot && slot.make !== '02' && !slot.isSxcLocation &&
        getSlotType(slot) !== 'empty' && getSlotType(slot) !== 'shielded';
}

// 获取利用率指示器样式类
const getUtilizationIndicatorClass = (slot) => {
    const level = getSlotUtilizationLevel(slot);
    return `utilization-${level}`;
}

// 获取仓位状态类型
const getSlotType = (slot) => {
    for (const [type, config] of Object.entries(statusConfig)) {
        if (config.condition(slot)) {
            return type;
        }
    }
    return 'empty'; // 默认空库位
}

// 获取状态显示文本
const getStatusText = (slot) => {
    const type = getSlotType(slot);
    return statusConfig[type]?.text || '未知状态';
}

// 获取状态样式类
const getSlotStatusClass = (slot) => {
    const type = getSlotType(slot);
    return `slot-${type}`;
}

// 获取单元格样式(可根据项目自由调整储位图例样式)
const getStatusStyle = (slot) => {
    const type = getSlotType(slot);
    const baseStyle = {
        width: cellDimensions.width,
        height: cellDimensions.height,
        border: '1px solid #e5e6eb',
        borderRadius: '4px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        margin: '3px',
        fontSize: cellDimensions.fontSize,
        fontWeight: '500',
        position: 'relative',
        cursor: 'pointer',
        transition: 'all 0.3s ease'
    };

    const statusStyle = statusConfig[type]?.style || statusConfig.empty.style;

    // 添加动画延迟
    if (shouldAnimate.value && slot?.animationDelay) {
        baseStyle.animationDelay = slot.animationDelay;
    } else {
        // 如果没有动画，确保立即显示
        baseStyle.opacity = '1';
        baseStyle.transform = 'scale(1) translateY(0)';
    }

    return { ...baseStyle, ...statusStyle };
}

// 获取图例样式
const getLegendStyle = (type) => {
    const colorMap = {
        runway: '#ffffff',
        fourWayCar: '#75b6d4',
        empty: '#ff7f50',
        hasGoods: '#89cffa',
        movingOut: '#ff81c1',
        movingIn: '#7ba5f0',
        outbound: '#58d758',
        shielded: '#d47575',
        inbound: '#e599e2',
        emptyCarrier: '#3d42c3',
        wall: '#808080',
    }

    return {
        backgroundColor: colorMap[type] || '#ffffff',
        width: '15px',
        height: '15px',
        borderRadius: '4px',
        border: '1px solid #ddd'
    }
}

// 锁定转换
const formatLockFlag = (flag) => {
    if (flag === null || flag === undefined) return '-'
    return flag === 1 ? '是' : '否'
}
// 是否锁定判断
const hasLockInfo = (slot) => {
    if (!slot) {
        return false;
    }
    return slot?.slotImlockFlag !== null || slot?.slotExlockFlag !== null || slot?.slotCloseFlag !== null
}
// 获取锁定状态的样式
const getLockFlagStyle = (flag) => {
    if (flag === 1) {
        return { color: '#ff4d4f', fontWeight: 'bold' }; // 红色加粗
    }
    return { color: '#52c41a' }; // 绿色表示安全
}

// 处理仓库变更
const handleWarehouseChange = async () => {
    if (filterForm.warehouseId) {
        const layerData = await wmsBaseSlotApi.getDropdownDataLayer(filterForm.warehouseId).then(res => res.data.result) ?? {};
        state.dropdownData.layerId = layerData.layerData || [];
        if (layerData.layerData && layerData.layerData.length > 0) {
            filterForm.slotLayer = layerData.layerData[0].value;
            await loadLayerData();
        }
    }
}

// 检测小车位置变化
const detectCarMovement = (newGrid) => {
    const currentCarPositions = new Set();

    // 收集当前小车位置
    newGrid.forEach(row => {
        row.forEach(slot => {
            if (slot?.isSxcLocation) {
                currentCarPositions.add(`${slot.slotRow}-${slot.slotColumn}`);
            }
        });
    });

    // 检测新出现的小车位置
    const newCarPositions = [...currentCarPositions].filter(
        pos => !previousCarPositions.value.has(pos)
    );

    // 如果有新的小车位置，添加移动特效
    if (newCarPositions.length > 0) {
        newCarPositions.forEach(pos => {
            carMovePositions.value.add(pos);
            // 3秒后移除移动特效
            setTimeout(() => {
                carMovePositions.value.delete(pos);
            }, 3000);
        });
    }

    // 更新历史位置
    previousCarPositions.value = currentCarPositions;
}

// 自定义Tooltip交互
let hideTimer = null

const handleSlotMouseEnter = (slot, event) => {
    if (slot?.make === '02' || slot?.slotStatus === 8) {
        showTooltip.value = false
        return;
    }
    if (hideTimer) clearTimeout(hideTimer)
    currentTooltipSlot.value = slot
    updateTooltipPosition(event)
    showTooltip.value = true
}

const handleSlotMouseLeave = () => {
    hideTimer = setTimeout(() => {
        showTooltip.value = false
    }, 0)
}

const updateTooltipPosition = (event) => {
    const tooltipOffset = 10
    const x = event.clientX
    const y = event.clientY

    tooltipStyle.left = `${x + tooltipOffset}px`
    tooltipStyle.top = `${y + tooltipOffset}px`

    nextTick(() => {
        const tooltipEl = document.querySelector('.custom-tooltip')
        if (tooltipEl) {
            const rect = tooltipEl.getBoundingClientRect()
            const viewportWidth = window.innerWidth
            const viewportHeight = window.innerHeight

            // 水平方向调整
            if (rect.right > viewportWidth) {
                tooltipStyle.left = `${x - rect.width - tooltipOffset}px`
            }

            // 垂直方向调整
            if (rect.bottom > viewportHeight) {
                // 计算上下可用空间
                const bottomSpace = viewportHeight - y - tooltipOffset
                const topSpace = y - tooltipOffset

                // 比较上下空间，选择空间更大的一侧
                if (topSpace > bottomSpace) {
                    // 顶部空间更大，往上显示
                    tooltipStyle.top = `${y - rect.height - tooltipOffset}px`
                }
                // 否则保持往下显示（即使部分被遮挡，因为顶部空间更小）
            }
        }
    })
}

const handleSlotClick = async (slot, event) => {
    const res = await wmsBaseSlotApi.getSlotDetail(slot.id).then(res => res.data.result);
    slotDetail.value = res || {};
    if (slot?.make === '02' || slot?.slotStatus === 8) {
        showTooltip.value = false
        return;
    }
    if (hideTimer) clearTimeout(hideTimer)
    currentTooltipSlot.value = slotDetail.value || []
    updateTooltipPosition(event)
    showTooltip.value = true
}

// 构建网格数据
const buildSlotGrid = (slotData) => {
    const { row: maxRow, col: maxCol, data: slots } = slotData;

    // 初始化网格
    const grid = [];
    for (let row = 1; row <= maxRow; row++) {
        const rowData = [];
        for (let col = 1; col <= maxCol; col++) {
            const slot = slots.find(s => s.slotRow === row && s.slotColumn === col);
            if (slot) {
                rowData.push({
                    id: slot.id,
                    slotCode: slot.slotCode,
                    slotRow: slot.slotRow,
                    slotColumn: slot.slotColumn,
                    slotStatus: slot.slotStatus,
                    slotCloseFlag: slot.slotCloseFlag,
                    isSxcLocation: slot.isSxcLocation,
                    make: slot.make || "",
                    materialId: slot.materialId,
                    goodsId: slot.goodsId,
                    remark: slot.remark,
                    // 添加动画延迟属性
                    animationDelay: shouldAnimate.value ?
                        `${(row) * 0.01 + col * 0.005}s` : '0s'
                });
            } else {
                // 空位置
                rowData.push(null);
            }
        }
        grid.push(rowData);
    }

    return grid;
}

// 自动刷新数据
let refreshInterval = null
const startAutoRefresh = () => {
    // 每5秒自动刷新一次
    refreshInterval = setInterval(() => {
        if (filterForm.warehouseId && filterForm.slotLayer) {
            loadLayerData(true);
        }
    }, 5000);
}

// 加载平库数据
const loadLayerData = async (param) => {
    if (!filterForm.warehouseId || !filterForm.slotLayer) {
        ElMessage.warning('请选择仓库和层数');
        return;
    }
    const isAutoRefresh = param === true;

    try {
        const res = await wmsBaseSlotApi.getSlotLegendList(filterForm);
        const data = res.data.result;

        if (data && data.data) {
            // Dynamic sizing logic based on columns
            if (data.col) {
                const cols = data.col;
                let w = 28;
                let h = 15;
                let fs = 8;
                
                // if (cols <= 20) { w = 60; h = 30; fs = 10; }
                // else if (cols <= 30) { w = 28; h = 14; fs = 9; }
                // else if (cols <= 50) { w = 20; h = 12; fs = 8; }
                // else if (cols <= 80) { w = 15; h = 8; fs = 8; }
                // else { w = 20; h = 12; fs = 8; } 

                cellDimensions.width = w + 'px';
                cellDimensions.height = h + 'px';
                cellDimensions.fontSize = fs + 'px';
            }

            const newGrid = buildSlotGrid(data);

            // 如果是自动刷新，禁用动画；否则开启动画（包括切换层数）
            if (isAutoRefresh) {
                shouldAnimate.value = false;
            } else {
                shouldAnimate.value = true;
                gridLoaded.value = false; // 重置加载状态以触发动画
            }

            slotGrid.value = newGrid;

            // 计算库位利用率统计
            calculateStorageStats(slotGrid.value.flat().filter(slot => slot && slot.make !== '02' && slot.slotStatus !== 8));
            // 初始化图表
            initCharts();
            // 标记网格已加载，触发动画
            nextTick(() => {
                gridLoaded.value = true;
            });

            // 第一次加载完成后标记
            if (isFirstLoad.value) {
                setTimeout(() => {
                    isFirstLoad.value = false;
                }, 1000);
            }
        } else {
            slotGrid.value = [];
            // 重置统计信息
            Object.assign(storageStats, {
                totalSlots: 0,
                usedSlots: 0,
                availableSlots: 0,
                operatingSlots: 0,
                abnormalSlots: 0,
                utilizationRate: 0
            });
            barChartData.value = [];
            pieChartData.value = [];
            ElMessage.warning('该层没有仓位数据');
        }
    } catch (error) {
        console.error('加载仓位数据失败:', error);
        ElMessage.error('加载仓位数据失败');
    }
}

// 生命周期加载数据
onMounted(async () => {
    try {
        // 初始化数据
        const data = await wmsBaseSlotApi.getDropdownData(true).then(res => res.data.result) ?? {};

        // 仓库数据（过滤掉类型为“01”的仓库） 01 = 立体库
        state.dropdownData.warehouseId = data.warehouseId.filter(m => m.type !== "01") || [];
        if (state.dropdownData.warehouseId.length > 0) {
            // （平库）
            filterForm.warehouseId = state.dropdownData.warehouseId[0].value;

            // 加载层数据
            const layerData = await wmsBaseSlotApi.getDropdownDataLayer(filterForm.warehouseId).then(res => res.data.result) ?? {};
            state.dropdownData.layerId = layerData.layerData || [];

            if (state.dropdownData.layerId.length > 0) {
                filterForm.slotLayer = state.dropdownData.layerId[0].value;
                await loadLayerData(false);
            }
        }

        // 初始化图表
        nextTick(() => {
            initCharts();
        });

        // 开始自动刷新
        //startAutoRefresh();

    } catch (error) {
        console.error('初始化数据失败:', error);
        ElMessage.error('初始化数据失败');
    }

    window.addEventListener('resize', handleResize);
    window.addEventListener('resize', () => {
        showTooltip.value = false
    });
})

// 组件卸载时清理
onUnmounted(() => {
    if (refreshInterval) {
        clearInterval(refreshInterval);
    }
    if (barChart) {
        barChart.dispose();
    }
    if (pieChart) {
        pieChart.dispose();
    }
    window.removeEventListener('resize', handleResize);
})
</script>

<style scoped>
.wms-storage-legend {
    padding: 0px;
    background-color: #f5f7fa;
    /* height: 140vh; */
    display: flex;
    flex-direction: column;
    /* overflow: hidden; */
}

.filter-card {
    margin-bottom: 10px;
    flex-shrink: 0;
}

/* 概览卡片样式 */
.overview-card {
    margin-bottom: 10px;
    flex-shrink: 0;
    opacity: 0;
    transform: translateY(-20px);
    animation: statsSlideIn 0.8s ease-out 0.3s forwards;
}

.overview-container {
    display: grid;
    grid-template-columns: repeat(6, 1fr);
    gap: 16px;
}

.overview-item {
    display: flex;
    align-items: center;
    padding: 16px;
    background: linear-gradient(135deg, #f8f9fa, #ffffff);
    border-radius: 8px;
    border: 1px solid #e4e7ed;
    transition: all 0.3s ease;
}

.overview-item:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.overview-icon {
    width: 48px;
    height: 48px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-right: 12px;
    font-size: 24px;
    color: white;
}

.overview-icon.total {
    background: linear-gradient(135deg, #409eff, #66b1ff);
}

.overview-icon.available {
    background: linear-gradient(135deg, #67c23a, #85ce61);
}

.overview-icon.used {
    background: linear-gradient(135deg, #e6a23c, #ebb563);
}

.overview-icon.utilization {
    background: linear-gradient(135deg, #f56c6c, #f78989);
}

.overview-icon.operating {
    background: linear-gradient(135deg, #909399, #a6a9ad);
}

.overview-icon.abnormal {
    background: linear-gradient(135deg, #d47575, #dc8c8c);
}

.overview-content {
    flex: 1;
}

.overview-value {
    font-size: 24px;
    font-weight: bold;
    color: #303133;
    margin-bottom: 4px;
}

.overview-label {
    font-size: 14px;
    color: #606266;
}

.utilization-progress {
    margin-top: 8px;
}

.progress-bar {
    width: 100%;
    height: 6px;
    background-color: #ebeef5;
    border-radius: 3px;
    overflow: hidden;
}

.progress-fill {
    height: 100%;
    background: linear-gradient(90deg, #67c23a, #e6a23c, #f56c6c);
    border-radius: 3px;
    transition: width 0.5s ease;
}

/* 图表区域 */
.charts-section {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 10px;
    margin-bottom: 10px;
    margin-top: 10px;
    flex-shrink: 0;
}

.chart-card {
    height: 300px;
    opacity: 0;
    transform: translateY(20px);
    animation: chartSlideIn 0.8s ease-out 0.6s forwards;
}

.chart-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.chart-title {
    font-size: 16px;
    font-weight: bold;
    color: #303133;
}

.chart-actions {
    display: flex;
    align-items: center;
    gap: 8px;
}

.chart-container {
    width: 100%;
    height: 250px;
}

.main-content {
    flex: 1;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    min-height: 0;
    max-height: 2000px;
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
    overflow-x: auto;
    overflow-y: hidden;
    position: relative;
}

/* 初始渲染动画 */
.slot-grid-container {
    flex: 1;
    overflow-x: auto;
    overflow-y: hidden;
    border: 1px solid #e4e7ed;
    border-radius: 4px;
    padding: 10px;
    background-color: white;
    min-height: 0;
    opacity: 0;
    animation: gridContainerFadeIn 0.8s ease-out forwards;
}

/* 网格行样式 */
.grid-row {
    display: flex;
    align-items: center;
    margin-bottom: 8px;
}

.slot-cell {
    transition: all 0.3s ease;
    cursor: pointer;
    flex-shrink: 0;
    position: relative;
    opacity: 0;
    transform: scale(0.8) translateY(10px);
    animation: slotCellAppear 0.6s ease-out forwards;
}

.slot-cell:hover {
    transform: scale(1.05);
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.slot-cell.active {
    border: 2px solid #409eff !important;
    box-shadow: 0 0 8px rgba(64, 158, 255, 0.6);
}

/* 利用率指示器 */
.utilization-indicator {
    position: absolute;
    bottom: 2px;
    right: 2px;
    width: 8px;
    height: 8px;
    border-radius: 50%;
    border: 1px solid rgba(255, 255, 255, 0.8);
}

.utilization-indicator.high {
    background-color: #f56c6c;
    box-shadow: 0 0 4px rgba(245, 108, 108, 0.6);
}

.utilization-indicator.medium {
    background-color: #e6a23c;
    box-shadow: 0 0 4px rgba(230, 162, 60, 0.6);
}

.utilization-indicator.low {
    background-color: #67c23a;
    box-shadow: 0 0 4px rgba(103, 194, 58, 0.6);
}

/* 高利用率库位样式 */
.slot-cell.high-utilization {
    box-shadow: 0 0 0 2px rgba(245, 108, 108, 0.3);
    animation: highUtilizationPulse 2s infinite 1s, slotCellAppear 0.6s ease-out forwards;
}

.slot-cell.medium-utilization {
    box-shadow: 0 0 0 2px rgba(230, 162, 60, 0.3);
}

@keyframes highUtilizationPulse {

    0%,
    100% {
        box-shadow: 0 0 0 2px rgba(245, 108, 108, 0.3);
    }

    50% {
        box-shadow: 0 0 0 4px rgba(245, 108, 108, 0.5);
    }
}

/* 小车移动特效 */
.slot-cell.car-move-effect {
    animation: carMovePulse 0.8s ease-in-out 3;
    z-index: 10;
}

@keyframes carMovePulse {

    0%,
    100% {
        transform: scale(1);
        box-shadow: 0 0 0 0 rgba(64, 158, 255, 0.7);
    }

    50% {
        transform: scale(1.1);
        box-shadow: 0 0 0 8px rgba(64, 158, 255, 0.4);
    }
}

.slot-label {
    white-space: nowrap;
    z-index: 1;
}

.slot-separator {
    margin: 0 2px;
}

.runway-text {
    font-size: 12px;
    font-weight: bold;
    color: #666;
}

.wall-text {
    font-size: 12px;
    font-weight: bold;
    color: #666;
}

.vehicle-icon {
    width: 100%;
    height: 100%;
    font-size: 8px;
    align-items: center;
    justify-content: center;
    display: flex;
    z-index: 100;
    background: linear-gradient(135deg, #75b6d4, #4a9fd8);
    color: white;
    padding: 1px 4px;
    border-radius: 4px;
    line-height: 1;
    animation: vehicleActive 2s infinite ease-in-out;
    position: relative;
    overflow: hidden;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.vehicle-icon::before {
    content: '';
    position: absolute;
    top: -50%;
    left: -50%;
    width: 200%;
    height: 200%;
    background: linear-gradient(45deg,
            transparent,
            rgba(255, 255, 255, 0.3),
            transparent);
    transform: rotate(45deg);
    animation: vehicleShine 3s infinite;
}

@keyframes vehicleActive {

    0%,
    100% {
        transform: translateY(0) scale(1);
        box-shadow: 0 2px 8px rgba(117, 182, 212, 0.6);
    }

    50% {
        transform: translateY(-1px) scale(1.02);
        box-shadow: 0 4px 12px rgba(117, 182, 212, 0.8);
    }
}

@keyframes vehicleShine {
    0% {
        transform: rotate(45deg) translateX(-100%);
    }

    100% {
        transform: rotate(45deg) translateX(100%);
    }
}

/* 自定义 Tooltip 样式 */
.custom-tooltip {
    position: fixed;
    z-index: 9999;
    background: rgba(0, 0, 0, 0.95);
    color: white;
    padding: 16px;
    border-radius: 8px;
    font-size: 12px;
    line-height: 1.6;
    box-shadow: 0 6px 16px rgba(0, 0, 0, 0.3);
    pointer-events: none;
    max-width: 500px;
    min-width: 400px;
    backdrop-filter: blur(8px);
    border: 1px solid rgba(255, 255, 255, 0.1);
    animation: tooltipFadeIn 0.2s ease-out;
}

@keyframes tooltipFadeIn {
    from {
        opacity: 0;
        transform: translateY(5px);
    }

    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.tooltip-section {
    margin-bottom: 16px;
    padding-bottom: 0;
    border-bottom: none;
}

/* 对于全宽的section，添加顶部边框 */
.tooltip-section.full-width {
    border-top: 1px solid rgba(255, 255, 255, 0.2);
    padding-top: 16px;
    margin-top: 16px;
}

/* Tooltip 内容横向布局容器 */
.tooltip-content {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 24px;
    align-items: start;
}

/* Tooltip 列容器 */
.tooltip-column {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

/* 确保 section 的高度一致 */
.tooltip-section {
    margin-bottom: 0;
    padding-bottom: 0;
    border-bottom: none;
}

/* 给 section-title 设置固定高度，确保对齐 */
.section-title {
    font-size: 12px;
    margin-bottom: 8px;
    color: #409eff;
    font-weight: 600;
    height: 16px;
    line-height: 16px;
}

/* 占位符样式，确保两列对齐 */
.tooltip-section.placeholder {
    min-height: 100px;
}

/* 对于物料信息，使用全宽 */
.tooltip-section.full-width {
    grid-column: 1 / -1;
}

.tooltip-section:last-child {
    margin-bottom: 0;
    padding-bottom: 0;
    border-bottom: none;
}

.section-title {
    font-size: 12px;
    margin-bottom: 6px;
    color: #409eff;
    font-weight: 600;
}

.tooltip-row {
    display: flex;
    margin-bottom: 4px;
    align-items: center;
}

.tooltip-label {
    color: #87ceeb;
    margin-right: 12px;
    white-space: nowrap;
    flex-shrink: 0;
    width: 70px;
    font-size: 11px;
}

.tooltip-value {
    flex: 1;
    word-break: break-word;
    font-size: 11px;
}

/* 利用率文本样式 */
.utilization-high {
    color: #f56c6c;
    font-weight: bold;
}

.utilization-medium {
    color: #e6a23c;
    font-weight: bold;
}

.utilization-low {
    color: #67c23a;
    font-weight: bold;
}

/* 物料信息样式 */
.material-list {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.material-item {
    padding: 8px;
    background: rgba(255, 255, 255, 0.05);
    border-radius: 4px;
}

.material-header {
    margin-bottom: 6px;
}

.material-index {
    font-size: 11px;
    font-weight: 600;
    color: #409eff;
}

.material-details {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 4px 12px;
}

.material-divider {
    height: 1px;
    background: rgba(255, 255, 255, 0.1);
    margin: 8px 0;
}

/* 图例样式 */
.legend-panel {
    width: 120px;
    border: 1px solid #e4e7ed;
    border-radius: 8px;
    padding: 15px;
    background-color: white;
    flex-shrink: 0;
    height: fit-content;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    opacity: 0;
    transform: translateX(20px);
    animation: legendSlideIn 0.8s ease-out 0.4s forwards;
}

.legend-table {
    width: 100%;
    border-collapse: separate;
    border-spacing: 0 8px;
    margin-bottom: 15px;
}

.legend-color {
    display: inline-block;
    vertical-align: middle;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.legend-text {
    color: #606266;
    font-size: 12px;
    padding-left: 8px;
    vertical-align: middle;
}

/* 利用率图例 */
.utilization-legend {
    border-top: 1px solid #e4e7ed;
    padding-top: 15px;
}

.legend-title {
    font-size: 14px;
    color: #303133;
    margin-bottom: 10px;
    font-weight: 600;
}

.utilization-legend-item {
    display: flex;
    align-items: center;
    margin-bottom: 8px;
}

.utilization-color {
    width: 20px;
    height: 20px;
    border-radius: 4px;
    margin-right: 8px;
    border: 1px solid #ddd;
}

.utilization-color.high-utilization {
    background-color: #f56c6c;
}

.utilization-color.medium-utilization {
    background-color: #e6a23c;
}

.utilization-color.low-utilization {
    background-color: #67c23a;
}

.utilization-text {
    font-size: 13px;
    color: #606266;
}

/* 响应式调整 */
@media (max-width: 1600px) {
    .overview-container {
        grid-template-columns: repeat(3, 1fr);
    }

    .slot-cell {
        height: 50px;
        font-size: 11px;
    }
}

@media (max-width: 1440px) {
    .charts-section {
        grid-template-columns: 1fr;
    }

    .slot-display-container {
        flex-direction: column;
    }

    .legend-panel {
        width: 100%;
        margin-top: 10px;
    }

    .legend-table {
        display: flex;
        flex-wrap: wrap;
        gap: 15px;
    }

    .legend-table tr {
        width: calc(33.333% - 15px);
        display: flex;
        align-items: center;
    }
}

@media (max-width: 768px) {
    .overview-container {
        grid-template-columns: 1fr;
    }

    .overview-item {
        padding: 12px;
    }

    .slot-cell {
        height: 40px;
        width: 50px;
        font-size: 10px;
    }

    .legend-table tr {
        width: calc(50% - 15px);
    }

    .chart-card {
        height: 250px;
    }

    .chart-container {
        height: 200px;
    }
}

/* 状态样式类 */
.slot-runway {
    background-color: #ffffff !important;
    color: #333 !important;
    border: 1px dashed #ccc !important;
}

.slot-fourWayCar {
    background-color: #75b6d4 !important;
    color: #fff !important;
}

.slot-empty {
    background-color: #ff7f50 !important;
    color: #333 !important;
}

.slot-hasGoods {
    background-color: #89cffa !important;
    color: #333 !important;
}

.slot-movingOut {
    background-color: #ff81c1 !important;
    color: #fff !important;
}

.slot-movingIn {
    background-color: #7ba5f0 !important;
    color: #fff !important;
}

.slot-outbound {
    background-color: #58d758 !important;
    color: #333 !important;
}

.slot-shielded {
    background-color: #d47575 !important;
    color: #fff !important;
}

.slot-inbound {
    background-color: #e599e2 !important;
    color: #333 !important;
}

.slot-emptyCarrier {
    background-color: #3d42c3 !important;
    color: #fff !important;
}

/* 动画关键帧 */
@keyframes gridContainerFadeIn {
    from {
        opacity: 0;
        transform: translateY(20px);
    }

    to {
        opacity: 1;
        transform: translateY(0);
    }
}

/* 自定义横向滚动条样式 */
.slot-grid-container::-webkit-scrollbar {
    height: 8px;
}

.slot-grid-container::-webkit-scrollbar-track {
    background: #f1f1f1;
    border-radius: 4px;
}

.slot-grid-container::-webkit-scrollbar-thumb {
    background: #c1c1c1;
    border-radius: 4px;
}

.slot-grid-container::-webkit-scrollbar-thumb:hover {
    background: #a8a8a8;
}

/* Firefox 滚动条样式 */
.slot-grid-container {
    scrollbar-width: thin;
    scrollbar-color: #c1c1c1 #f1f1f1;
}

@keyframes slotCellAppear {
    to {
        opacity: 1;
        transform: scale(1) translateY(0);
    }
}

@keyframes statsSlideIn {
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

@keyframes chartSlideIn {
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

@keyframes legendSlideIn {
    to {
        opacity: 1;
        transform: translateX(0);
    }
}
</style>