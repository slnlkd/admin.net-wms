<template>
	<el-dialog v-model="state.showDialog" :title="dialogTitle" width="1350px" :close-on-click-modal="false"
		destroy-on-close @closed="handleDialogClosed">
		 
			<!-- 上半部分：搜索信息 -->
			<div class="base-info-section">
				<el-form :model="state.tableQueryParams" ref="formRef" label-width="70px" size="small">
					<el-row :gutter="10">
						<el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4" class="mb10">
							<el-form-item label="所属仓库">
								<el-select clearable filterable v-model="state.tableQueryParams.warehouseId" placeholder="请选择"
									style="width: 100%">
									<el-option v-for="(item, index) in state.dropdownData.warehouseId ?? []" :key="index"
										:value="item.id" :label="item.warehouseName" />
								</el-select>
							</el-form-item>
						</el-col>
						<el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4" class="mb10">
							<el-form-item label="所属巷道">
								<el-select clearable filterable v-model="state.tableQueryParams.slotLanewayId" placeholder="请选择"
									style="width: 100%">
									<el-option v-for="(item, index) in state.dropdownData.lanewayId ?? []" :key="index"
										:value="item.id" :label="item.lanewayName" />
								</el-select>
							</el-form-item>
						</el-col>
						<el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4" class="mb10">
							<el-form-item label="储位编码">
								<el-input v-model="state.tableQueryParams.slotCode" placeholder="请输入编码" style="width: 100%" clearable @keyup.enter="handleSearch" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="6" :lg="6" :xl="6" class="mb10">
							<el-form-item label="储位位置">
								<div class="location-inputs" style="display: flex; gap: 5px; width: 100%;">
									<el-input-number v-model="state.tableQueryParams.slotRow" placeholder="排" controls-position="right" :min="1" @keyup.enter="handleSearch" style="width: 100px" />
									<el-input-number v-model="state.tableQueryParams.slotColumn" placeholder="列" controls-position="right" :min="1" @keyup.enter="handleSearch" style="width: 100px" />
									<el-input-number v-model="state.tableQueryParams.slotLayer" placeholder="层" controls-position="right" :min="1" @keyup.enter="handleSearch" style="width: 100px" />
								</div>
							</el-form-item>
						</el-col>
						 
						<el-col :xs="24" :sm="12" :md="6" :lg="6" :xl="6" class="mb10" style="display: flex; justify-content: flex-end; align-items: flex-start;">
							<div class="button-group"> 
								<el-button type="primary" icon="ele-Search" @click="handleSearch">查询</el-button>
								<el-button icon="ele-Refresh" @click="handleReset">重置</el-button>
								<el-button type="primary" icon="ele-Check" @click="handleSubmit" :loading="submitLoading">确定选择</el-button>
							</div>
						</el-col>
					</el-row>
				</el-form>
			</div>
			<!-- 下半部分：储位列表 -->
			<div class="slot-list-section">
				<el-table :data="state.tableData" v-loading="state.tableLoading" border style="width: 100%; height: 100%"
					@row-click="handleRowClick" @selection-change="handleSelectionChange" :row-class-name="getRowClassName"
					:row-key="getRowKey" ref="tableRef" highlight-current-row>
					<el-table-column :fixed="true" type="selection" width="55" align="center" :reserve-selection="true" />
					<el-table-column type="index" label="序号" width="50" align="center" />
					<el-table-column align="center" prop='warehouseId' label='所属仓库'
						:formatter="(row: any) => row.warehouseFkDisplayName" show-overflow-tooltip />
					<el-table-column align="center" prop='slotAreaId' label='所属区域'
						:formatter="(row: any) => row.slotAreaFkDisplayName" show-overflow-tooltip />
					<el-table-column align="center" prop='slotLanewayId' label='所属巷道'
						:formatter="(row: any) => row.slotLanewayFkDisplayName" show-overflow-tooltip />
					<el-table-column align="center" prop='slotCode' label='储位编码' show-overflow-tooltip />
					<el-table-column align="center" prop='slotRow' width="80" label='排' show-overflow-tooltip />
					<el-table-column align="center" prop='slotColumn' width="80" label='列' show-overflow-tooltip />
					<el-table-column align="center" prop='slotLayer' width="80" label='层' show-overflow-tooltip />
					<el-table-column align="center" prop='slotInout' label='储位深度' show-overflow-tooltip>
						<template #default="scope">
							<g-sys-dict v-model="scope.row.slotInout" code="SlotInout" />
						</template>
					</el-table-column>
					<el-table-column align="center" prop='slotStatus' label='储位状态' show-overflow-tooltip>
						<template #default="scope">
							<g-sys-dict v-model="scope.row.slotStatus" code="SlotStatus" />
						</template>
					</el-table-column>
				</el-table>
				<el-pagination v-model:currentPage="state.tableParams.page" v-model:page-size="state.tableParams.pageSize"
					@size-change="(val: any) => handleQuery({ pageSize: val })"
					@current-change="(val: any) => handleQuery({ page: val })"
					layout="total, sizes, prev, pager, next, jumper" :page-sizes="[10, 15, 50, 100, 200, 500]"
					:total="state.tableParams.total" size="small" background />
			</div>
		 
	</el-dialog>
</template>

<script lang="ts" name="wmsImportNotify" setup>
import { ref, reactive, onMounted, nextTick } from 'vue'
import { ElMessage, type FormInstance } from 'element-plus'
import { useWmsBaseWareHouseApi } from '/@/api/base/baseWarehouse/wmsBaseWareHouse';
import { useWmsBaseLanewayApi } from '/@/api/base/baseLaneway/wmsBaseLaneway';
import { useWmsBaseSlotApi } from '/@/api/base/baseSlot/wmsBaseSlot';
import { useWmsImportOrderApi } from '/@/api/import/importOrder/wmsImportOrder';
//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const wmsBaseWareHouseApi = useWmsBaseWareHouseApi();
const wmsBaseSlotApi = useWmsBaseSlotApi();
const wmsBaseLanewayApi = useWmsBaseLanewayApi();
const wmsImportOrderApi = useWmsImportOrderApi();
// 弹窗显示控制
const dialogTitle = ref('指定储位')
const submitLoading = ref(false)
// 表单数据
const formRef = ref<FormInstance>()
const tableRef = ref() // 表格引用
const state = reactive({
	tableQueryParams: {} as any,
	tableLoading: false,
	showDialog: false,
	orderId: 0, // 入库流水ID
	selectedSlot: null as any, // 存储选中的储位
	currentRowId: null as string | null, // 当前选中行的ID
	tableParams: {
		page: 1,
		pageSize: 15,
		total: 0,
		field: 'createTime', // 默认的排序字段
		order: 'descending', // 排序方向
		descStr: 'descending', // 降序排序的关键字符
	},
	dropdownParams: {
		page: 1,
		pageSize: 200,
		total: 0,
		field: 'createTime', // 默认的排序字段
		order: 'descending', // 排序方向
		descStr: 'descending', // 降序排序的关键字符
	},
	dropdownQueryParams: {} as any,
	dropdownData: {} as any,
	tableData: []
})

// 页面加载时
onMounted(async () => {
	// 加载仓库下拉数据
	const warehousedata = await wmsBaseWareHouseApi.page(Object.assign(state.dropdownQueryParams, state.dropdownParams)).then(res => res.data.result);
	state.dropdownData.warehouseId = warehousedata.items;
	// 加载巷道下拉数据
	const lanewaydata = await wmsBaseLanewayApi.page(Object.assign(state.dropdownQueryParams, state.dropdownParams)).then(res => res.data.result);
	state.dropdownData.lanewayId = lanewaydata.items;
	await handleQuery()
});

const handleSearch = async () => {
	state.tableParams.page = 1; // 重置页码
	await handleQuery()
}

// 重置搜索条件并刷新数据
const handleReset = async () => {
	state.tableQueryParams = {}
	state.selectedSlot = null // 清空选中项
	state.currentRowId = null // 清空当前选中行ID
	state.tableParams.page = 1; // 重置页码
	await handleQuery()
}

// 打开弹窗
const openDialog = async (row: any = null) => {
	state.orderId = row?.id || 0
	state.selectedSlot = null // 清空选中项
	state.currentRowId = null // 清空当前选中行ID
	state.tableParams.page = 1; // 打开时默认第一页
	await handleQuery()
	state.showDialog = true;
}

// 弹窗关闭时的处理
const handleDialogClosed = () => {
	state.tableData = []
	state.tableQueryParams = {}
	state.selectedSlot = null
	state.currentRowId = null
	state.showDialog = false;
}

// 加载储位数据
const handleQuery = async (params: any = {}) => {
	try {
		state.tableLoading = true;
		state.tableParams = Object.assign(state.tableParams, params);
		const result = await wmsBaseSlotApi.page(Object.assign({ make: '01' }, state.tableQueryParams, state.tableParams)).then(res => res.data.result);
		state.tableParams.total = result?.total;
		state.tableData = result?.items ?? [];
		
		// 数据加载完成后，如果有选中的行，重新设置选择状态
		nextTick(() => {
			if (state.currentRowId && tableRef.value) {
				const row = state.tableData.find(item => item.id === state.currentRowId)
				if (row) {
					tableRef.value.toggleRowSelection(row, true)
				}
			}
		})
	} catch (error) {
		console.error('获取储位列表失败:', error);
	} finally {
		state.tableLoading = false;
	}
};

// 获取行唯一标识
const getRowKey = (row: any) => {
	return row.id
}

// 处理行点击事件
const handleRowClick = (row: any) => {
	state.selectedSlot = row
	state.currentRowId = row.id

	// 手动设置选择状态
	if (tableRef.value) {
		// 先清除所有选择
		tableRef.value.clearSelection()
		// 然后选择当前行
		tableRef.value.toggleRowSelection(row, true)
	}
}

// 处理选择变化事件
const handleSelectionChange = (selection: any[]) => {
	if (selection.length > 0) {
		// 只保留最后一个选中的项（单选逻辑）
		const lastSelected = selection[selection.length - 1]
		state.selectedSlot = lastSelected
		state.currentRowId = lastSelected.id

		// 如果选择了多个，只保留最后一个
		if (selection.length > 1 && tableRef.value) {
			tableRef.value.clearSelection()
			tableRef.value.toggleRowSelection(lastSelected, true)
		}
	} else {
		// 如果没有选择任何项，清空选中状态
		state.selectedSlot = null
		state.currentRowId = null
	}
}

// 设置行样式
const getRowClassName = ({ row }: { row: any }) => {
	return row.id === state.currentRowId ? 'selected-row' : ''
}

// 提交表单
const handleSubmit = async () => {
	if (!state.selectedSlot) {
		ElMessage.warning('请选择一个储位')
		return
	}
	if (state.selectedSlot.slotStatus != 0) {
		ElMessage.warning('该储位状态不可选，请选择空储位')
		return
	}
	try {
		submitLoading.value = true
		await wmsImportOrderApi.saveSlot({ orderId: state.orderId, slotId: state.selectedSlot.id })
		ElMessage.success('指定储位成功')
		emit('reloadTable')
		state.showDialog = false
	} catch (error: any) {
		// 优先使用后端返回的错误信息，其次使用 error.message，最后使用默认提示
		const errorMsg = error?.data?.msg || error?.data?.message || error?.message || '指定储位失败'
		ElMessage.error(errorMsg)
	} finally {
		submitLoading.value = false
	}
}

// 暴露方法给父组件
defineExpose({
	openDialog
})
</script>

<style scoped>
.designate-slot-container {
	display: flex;
	flex-direction: column;
	height: 60vh; /* Adjust as needed */
	min-height: 500px;
}

.base-info-section {
	/* height: 90px; Removed fixed height */
	margin-bottom: 10px;
	padding: 10px;
	border: 1px solid #e4e7ed;
	border-radius: 4px;
}

.slot-list-section {
	flex: 1;
	display: flex;
	flex-direction: column;
	min-height: 0; /* Crucial for nested flex scrolling */
	border: 1px solid #e4e7ed;
	border-radius: 4px;
	padding: 5px;
}

/* 调整表单行高 */
:deep(.el-form--inline .el-form-item) {
	margin-bottom: 5px;
	margin-right: 15px;
}

/* 选中行样式 */
:deep(.selected-row) {
	background-color: var(--el-color-primary-light-9) !important;
}

:deep(.el-table .selected-row td) {
	background-color: var(--el-color-primary-light-9) !important;
}

/* 鼠标悬停样式优化 */
:deep(.el-table--enable-row-hover .el-table__body tr:hover>td) {
	background-color: var(--el-fill-color-light) !important;
}

:deep(.el-table--enable-row-hover .el-table__body tr.selected-row:hover>td) {
	background-color: var(--el-color-primary-light-8) !important;
}

/* 分页居右 */
.el-pagination {
	margin-top: 10px;
	justify-content: flex-end;
}
</style>