<template>
	<!-- 物料选择对话框 -->
	<el-dialog v-model="state.showDialog" title="选择物料信息" width="1150px" :close-on-click-modal="false"
		destroy-on-close @closed="handleDialogClosed">
		 
			<!-- 查询条件区域 -->
			<div class="base-info-section">
				<el-form :model="state.tableQueryParams" ref="formRef" label-width="100px" size="small">
					<el-row :gutter="35">
						<!-- 物料编码查询条件 -->
						<el-col :xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb10">
							<el-form-item label="物料编码">
								<el-input v-model="state.tableQueryParams.materialCode" placeholder="请输入物料编码" style="width: 80%" clearable @keyup.enter="handleSearch" />
							</el-form-item>
						</el-col>
						<!-- 物料名称查询条件 -->
						<el-col :xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb10">
							<el-form-item label="物料名称">
								<el-input v-model="state.tableQueryParams.materialName" placeholder="请输入物料名称" style="width: 80%" clearable @keyup.enter="handleSearch" />
							</el-form-item>
						</el-col>
						<!-- 托盘条码查询条件 -->
						<el-col :xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb10">
							<el-form-item label="托盘条码">
								<el-input v-model="state.tableQueryParams.stockCode" placeholder="请输入托盘条码" style="width: 80%" clearable @keyup.enter="handleSearch" />
							</el-form-item>
						</el-col>
						<!-- 储位编码查询条件 -->
						<el-col :xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb10">
							<el-form-item label="储位编码">
								<el-input v-model="state.tableQueryParams.slotCode" placeholder="请输入储位编码" style="width: 80%" clearable @keyup.enter="handleSearch" />
							</el-form-item>
						</el-col>
						<!-- 批次查询条件 -->
						<el-col :xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb10">
							<el-form-item label="批次">
								<el-input v-model="state.tableQueryParams.lotNo" placeholder="请输入批次" style="width: 80%" clearable @keyup.enter="handleSearch" />
							</el-form-item>
						</el-col>
						<!-- 操作按钮 -->
						<el-col :xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb10" style="display: flex; justify-content: flex-start; align-items: flex-start; padding-left: 55px;">
							<div class="button-group"> 
								<el-button type="primary" icon="ele-Search" @click="handleSearch">搜索</el-button>
								<el-button icon="ele-Check" @click="handleSubmit" :loading="submitLoading">确定</el-button>
							</div>
						</el-col>
					</el-row>
				</el-form>
			</div>
			<!-- 物料列表区域 -->
			<div class="material-list-section">
				<!-- 物料表格 -->
				<el-table :data="state.tableData" v-loading="state.tableLoading" border style="width: 100%; height: 100%"
					@row-click="handleRowClick" @selection-change="handleSelectionChange" :row-class-name="getRowClassName"
					:row-key="getRowKey" ref="tableRef" highlight-current-row>
					<!-- 复选框列 -->
					<el-table-column :fixed="true" type="selection" width="55" align="center" :reserve-selection="true" />
					<!-- 序号列 -->
					<el-table-column type="index" label="序号" width="50" align="center" />
					<!-- 物料编码列 -->
					<el-table-column align="center" prop='materialCode' label='物料编码'
						show-overflow-tooltip />
					<!-- 物料名称列 -->
					<el-table-column align="center" prop='materialName' label='物料名称'
						show-overflow-tooltip />
					<!-- 物料规格列 -->
					<el-table-column align="center" prop='materialStandard' label='物料规格'
						show-overflow-tooltip />
					<!-- 质检状态列 -->
					<el-table-column align="center" prop='inspectionStatus' label='质检状态' show-overflow-tooltip>
						<template #default="scope">
							<g-sys-dict v-model="scope.row.inspectionStatus" code="QualityInspectionStatus" />
						</template>
					</el-table-column>
					<!-- 储位编码列 -->
					<el-table-column align="center" prop='slotCode' label='储位编码' show-overflow-tooltip />
					<!-- 托盘条码列 -->
					<el-table-column align="center" prop='stockCode' label='托盘条码' show-overflow-tooltip />
					<!-- 批次列 -->
					<el-table-column align="center" prop='lotNo' label='批次' show-overflow-tooltip />
					<!-- 库存数量列 -->
					<el-table-column align="center" prop='stockQuantity' label='库存数量' show-overflow-tooltip />
				</el-table>
			</div>
		 
	</el-dialog>
</template>

<script lang="ts" name="materialDialog" setup>
import { ref, reactive, onMounted, nextTick } from 'vue'
import { ElMessage, ElMessageBox, type FormInstance } from 'element-plus'
import { useWmsStockCheckOrderApi } from '/@/api/stockCheck/stockCheckOrder/wmsStockCheckOrder';

// 定义事件
const emit = defineEmits(["addMaterial"]);
// 引入盘点单据API
const wmsStockCheckOrderApi = useWmsStockCheckOrderApi();
// 提交按钮加载状态
const submitLoading = ref(false)
// 是否为用户手动选择（首次加载自动勾选时不触发弹框）
const isManualSelection = ref(false)
// 是否正在批量勾选（防止批量勾选时重复弹出确认框）
const isBatchSelecting = ref(false)
// 表单引用
const formRef = ref<FormInstance>()
// 表格引用
const tableRef = ref()
// 页面响应式状态
const state = reactive({
	tableQueryParams: {} as any, // 表格查询参数
	tableLoading: false, // 表格加载状态
	showDialog: false, // 是否显示对话框
	warehouseId: 0, // 仓库ID
	selectedMaterials: [] as any[], // 选中的物料列表
	currentRowId: null as string | null, // 当前行ID
	allAddedMaterials: [] as any[], // 所有曾经添加过的物料（包括已删除的）
	currentMaterials: [] as any[], // 当前明细列表（用于判断哪些应该勾选）
	tableParams: {
		page: 1, // 当前页码
		pageSize: 15, // 每页条数
		total: 0, // 总记录数
		field: 'createTime', // 排序字段
		order: 'descending', // 排序方式
		descStr: 'descending',
	},
	dropdownQueryParams: {} as any, // 下拉数据查询参数
	dropdownData: {} as any, // 下拉数据
	tableData: [] // 表格数据
})

// 搜索处理
const handleSearch = async () => {
	state.tableParams.page = 1;
	await handleQuery()
}

// 重置查询条件
const handleReset = async () => {
	state.tableQueryParams = {}
	state.selectedMaterials = []
	state.currentRowId = null
	state.tableParams.page = 1;
	await handleQuery()
}

// 打开对话框
const openDialog = async (warehouseId: number, allAddedMaterials: any[] = [], currentMaterials: any[] = []) => {
	state.warehouseId = warehouseId
	state.allAddedMaterials = allAddedMaterials
	state.currentMaterials = currentMaterials
	state.selectedMaterials = []
	state.currentRowId = null
	state.tableParams.page = 1;
	state.showDialog = true;
	isManualSelection.value = false
	await handleQuery()
	await nextTick()
	// 自动勾选当前明细中的物料
	if (state.currentMaterials && state.currentMaterials.length > 0 && tableRef.value) {
		state.currentMaterials.forEach((currentItem: any) => {
			const row = state.tableData.find((item: any) => item.id === currentItem.id)
			if (row) {
				tableRef.value.toggleRowSelection(row, true)
			}
		})
	}
	// 自动勾选完成后，允许用户手动选择触发弹框
	await nextTick()
	isManualSelection.value = true
}

// 对话框关闭处理
const handleDialogClosed = () => {
	state.tableData = []
	state.tableQueryParams = {}
	state.selectedMaterials = []
	state.currentRowId = null
	state.allAddedMaterials = []
	state.currentMaterials = []
	state.showDialog = false;
}

// 查询物料列表
const handleQuery = async (params: any = {}) => {
	try {
		state.tableLoading = true;
		state.tableParams = Object.assign(state.tableParams, params);
		// 构建查询参数
		const queryParams = {
			...state.tableQueryParams,
			warehouseId: state.warehouseId,
			allAddStockCode:state.allAddedMaterials.map(item => item.stockCode),
			allAddCheckStockId:state.allAddedMaterials.map(item => item.checkStockId)
		};
		// 调用接口获取库存列表
		const result = await wmsStockCheckOrderApi.getStockSlotList(queryParams).then(res => res.data.result);
		console.log(result)
		state.tableData = result ?? [];
		
		console.log(state.allAddedMaterials);
		// 将所有曾经添加过的物料补充到表格数据中（如果接口返回的数据中没有）
		if (state.allAddedMaterials && state.allAddedMaterials.length > 0) {
			state.allAddedMaterials.forEach((addedItem: any) => {
				const exists = state.tableData.find((item: any) => item.id === addedItem.id)
				if (!exists) {
					state.tableData.unshift(addedItem)
				}
			})
		}

		 // 排序：处理 stockCode 可能为字符串的情况
		state.tableData = state.tableData.sort((a, b) => {
			const codeA = a.stockCode || ''; // 防止 undefined
			const codeB = b.stockCode || '';
			
			// 如果都是数字，按数字排序
			if (!isNaN(codeA) && !isNaN(codeB)) {
				return Number(codeA) - Number(codeB);
			}
			
			// 否则按字符串排序（支持中文）
			return String(codeA).localeCompare(String(codeB), 'zh-CN');
		});
	} catch (error) {
		console.error('获取物料列表失败:', error);
	} finally {
		state.tableLoading = false;
	}
};

// 获取表格行key
const getRowKey = (row: any) => {
	return row.id
}

// 表格行点击处理
const handleRowClick = (row: any) => {
}

// 表格选择变化处理
const handleSelectionChange = async (selection: any[]) => {
	// 如果正在批量勾选中，直接更新选中列表，不进行任何检查
	if (isBatchSelecting.value) {
		state.selectedMaterials = selection
		return
	}
	
	const currentSelection = selection
	
	// 找出当前操作的行（比较新旧选中列表的差异）
	const previousSelection = state.selectedMaterials
	let currentOperatedRow: any = null
	let isCheckOperation = false
	
	// 判断是勾选还是取消勾选
	if (currentSelection.length > previousSelection.length) {
		// 勾选操作
		isCheckOperation = true
		currentOperatedRow = currentSelection.find((item: any) => !previousSelection.some((prev: any) => prev.id === item.id))
	} else if (currentSelection.length < previousSelection.length) {
		// 取消勾选操作
		isCheckOperation = false
		currentOperatedRow = previousSelection.find((item: any) => !currentSelection.some((curr: any) => curr.id === item.id))
	}
	
	// 只有用户手动选择且是勾选操作时，才检查是否有相同 stockCode 的多条记录
	if (isManualSelection.value && isCheckOperation && currentOperatedRow && currentOperatedRow.stockCode) {
		const stockCode = currentOperatedRow.stockCode
		const sameStockCodeRows = state.tableData.filter((item: any) => item.stockCode === stockCode)
		
		// 如果相同 stockCode 的行有多条，弹框提示
		if (sameStockCodeRows.length > 1) {
			try {
				await ElMessageBox.confirm(
					`检测到托盘条码 "${stockCode}" 有 ${sameStockCodeRows.length} 条记录，是否全部勾选？`,
					'提示',
					{
						confirmButtonText: '是',
						cancelButtonText: '否',
						type: 'warning',
					}
				)
				// 用户选择是，勾选所有相同 stockCode 的行
				isBatchSelecting.value = true
				sameStockCodeRows.forEach((row: any) => {
					if (!currentSelection.some((item: any) => item.id === row.id) && tableRef.value) {
						tableRef.value.toggleRowSelection(row, true)
					}
				})
				// 批量勾选完成后，重置标志并获取最新的选中列表
				await nextTick()
				isBatchSelecting.value = false
				// 重新获取最新的选中列表
				if (tableRef.value) {
					const latestSelection = tableRef.value.getSelectionRows()
					state.selectedMaterials = latestSelection
					return
				}
			} catch {
				// 用户选择否，只保留当前勾选的行
			}
		}
	}
	console.log(currentSelection)
	state.selectedMaterials = currentSelection
}

// 获取行样式类名
const getRowClassName = ({ row }: { row: any }) => {
	return row.id === state.currentRowId ? 'selected-row' : ''
}

// 提交处理
const handleSubmit = async () => {
	// 验证是否选择了物料
	if (state.selectedMaterials.length === 0) {
		ElMessage.warning('请选择待盘点的物料')
		return
	}
	try {
		submitLoading.value = true
		console.log(state.selectedMaterials)
		// 触发添加物料事件
		emit('addMaterial', state.selectedMaterials)
		state.showDialog = false
	} catch (error: any) {
		console.error('添加物料失败:', error)
		ElMessage.error(error.message || '添加物料失败')
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
/* 基本信息区域样式 */
.base-info-section {
	margin-bottom: 10px;
	padding: 10px;
	border: 1px solid #e4e7ed;
	border-radius: 4px;
}

/* 物料列表区域样式 */
.material-list-section {
	flex: 1;
	display: flex;
	flex-direction: column;
	min-height: 0;
	border: 1px solid #e4e7ed;
	border-radius: 4px;
	padding: 5px;
	height: 500px;
}

/* 表单项间距样式 */
:deep(.el-form--inline .el-form-item) {
	margin-bottom: 5px;
	margin-right: 15px;
}

/* 选中行样式 */
:deep(.selected-row) {
	background-color: var(--el-color-primary-light-9) !important;
}

/* 选中行单元格样式 */
:deep(.el-table .selected-row td) {
	background-color: var(--el-color-primary-light-9) !important;
}

/* 表格行悬停样式 */
:deep(.el-table--enable-row-hover .el-table__body tr:hover>td) {
	background-color: var(--el-fill-color-light) !important;
}

/* 选中行悬停样式 */
:deep(.el-table--enable-row-hover .el-table__body tr.selected-row:hover>td) {
	background-color: var(--el-color-primary-light-8) !important;
}

/* 分页组件样式 */
.el-pagination {
	margin-top: 10px;
	justify-content: flex-end;
}
</style>
