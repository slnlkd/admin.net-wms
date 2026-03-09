<template>
	<!-- 添加/编辑盘点单据对话框 -->
	<el-dialog v-model="state.showDialog" :title="dialogTitle" width="1400px" :close-on-click-modal="false"
		destroy-on-close @closed="handleDialogClosed">
		 
			<!-- 基本信息区域 -->
			<div class="base-info-section">
				<el-form :model="state.formData" ref="formRef" label-width="100px" size="small">
					<el-row :gutter="35">
						<!-- 盘点单号 -->
						<el-col :xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb10">
							<el-form-item label="盘点单号">
								<el-input v-model="state.formData.checkBillCode" disabled placeholder="自动生成单号"
									style="width: 80%" />
							</el-form-item>
						</el-col>
						<!-- 盘点仓库 -->
						<el-col :xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb10">
							<el-form-item label="盘点仓库">
								<el-select clearable filterable v-model="state.formData.warehouseId" placeholder="请选择"
									style="width: 80%">
									<el-option v-for="(item, index) in state.dropdownData.warehouseId ?? []" :key="index"
										:value="item.id" :label="item.warehouseName" />
								</el-select>
							</el-form-item>
						</el-col>
						<!-- 备注 -->
						<el-col :xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb10">
							<el-form-item label="备注">
								<el-input v-model="state.formData.checkRemark" placeholder="请输入备注" style="width: 80%" clearable />
							</el-form-item>
						</el-col>
					</el-row>
				</el-form>
			</div>
			<!-- 明细列表区域 -->
			<div class="detail-list-section">
				<!-- 添加明细按钮 -->
				<div style="padding-bottom: 6px;">
					<el-button type="primary" icon="ele-Plus" @click="openMaterialDialog">添加明细</el-button>
				</div>
				<!-- 明细表格 -->
				<el-table :data="state.tableData" v-loading="state.tableLoading" border style="width: 100%; height: 100%"
					@row-click="handleRowClick" :row-key="getRowKey" ref="tableRef" highlight-current-row>
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
					<!-- 操作列 -->
					<el-table-column label="操作" align="center" fixed="right" show-overflow-tooltip width="100">
						<template #default="scope">
							<el-button type="danger" icon="ele-Delete" size="small" text
								@click="handleDelete(scope.row)">删除</el-button>
						</template>
					</el-table-column>
				</el-table>
			</div>
			<!-- 对话框底部按钮 -->
			<template #footer>
				<el-button type="primary" icon="ele-Check" @click="handleSubmit" :loading="submitLoading">立即提交</el-button>
				<el-button icon="ele-Close" @click="handleDialogClosed">取消</el-button>
			</template>
		 
	</el-dialog>
	<!-- 物料选择对话框 -->
	<MaterialDialog ref="materialDialogRef" @addMaterial="handleAddMaterial" />
</template>

<script lang="ts" name="wmsStockCheckOrder" setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, type FormInstance } from 'element-plus'
import { useWmsBaseWareHouseApi } from '/@/api/base/baseWarehouse/wmsBaseWareHouse';
import { useWmsStockCheckOrderApi } from '/@/api/stockCheck/stockCheckOrder/wmsStockCheckOrder';
import MaterialDialog from './materialDialog.vue'

// 定义事件
const emit = defineEmits(["reloadTable"]);
// 引入仓库API
const wmsBaseWareHouseApi = useWmsBaseWareHouseApi();
// 引入盘点单据API
const wmsStockCheckOrderApi = useWmsStockCheckOrderApi();
// 对话框标题
const dialogTitle = ref('添加盘点单')
// 提交按钮加载状态
const submitLoading = ref(false)
// 表单引用
const formRef = ref<FormInstance>()
// 表格引用
const tableRef = ref()
// 物料选择对话框引用
const materialDialogRef = ref()
// 页面响应式状态
const state = reactive({
	formData: {
		checkBillCode: '', // 盘点单号
		warehouseId: undefined, // 仓库ID
		checkRemark: '' // 备注
	} as any,
	tableQueryParams: {} as any, // 表格查询参数
	tableLoading: false, // 表格加载状态
	showDialog: false, // 是否显示对话框
	billId: 0, // 盘点单据ID
	tableParams: {
		page: 1, // 当前页码
		pageSize: 15, // 每页条数
		total: 0, // 总记录数
		field: 'checkDate', // 排序字段
		order: 'descending', // 排序方式
		descStr: 'descending',
	},
	dropdownParams: {
		page: 1, // 下拉数据当前页码
		pageSize: 200, // 下拉数据每页条数
		total: 0, // 下拉数据总记录数
		field: 'createTime', // 下拉数据排序字段
		order: 'descending', // 下拉数据排序方式
		descStr: 'descending',
	},
	dropdownQueryParams: {} as any, // 下拉数据查询参数
	dropdownData: {} as any, // 下拉数据
	tableData: [] as any[], // 明细表格数据
	allAddedMaterials: [] as any[] // 所有曾经添加过的物料（包括已删除的）
})

// 组件挂载时初始化
onMounted(async () => {
	// 加载仓库下拉数据
	const warehousedata = await wmsBaseWareHouseApi.page(Object.assign(state.dropdownQueryParams, state.dropdownParams)).then(res => res.data.result);
	state.dropdownData.warehouseId = warehousedata.items;
});

// 打开对话框
const openDialog = async (row: any = null, title: string = '添加盘点单') => {
	state.showDialog = true;
	dialogTitle.value = title
	state.billId = row?.id || 0
	state.formData.checkBillCode = row?.checkBillCode || ''
	state.formData.warehouseId = row?.warehouseId || undefined
	state.formData.checkRemark = row?.checkRemark || ''
	state.tableData = []
	state.allAddedMaterials = []
	// 如果是编辑模式，加载明细数据
	if (row?.id) {
		await loadDetailData(row.id)
	}
}

// 加载明细数据
const loadDetailData = async (id: number) => {
	try {
		state.tableLoading = true;
		// 调用接口获取明细数据
		const result = await wmsStockCheckOrderApi.detail({ Id:id }).then(res => res.data.result);
		// 映射返回的数据到表格数据格式
		state.tableData = result?.list?.map((item: any) => ({
			id: item.id,
			materialId: item.materialId,
			materialCode: item.materialCode,
			materialName: item.materialName,
			materialStandard: item.materialStandard,
			inspectionStatus: item.inspectionStatus,
			slotCode: item.stockSlot,
			stockCode: item.stockCode,
			lotNo: item.stockLotNo,
			stockQuantity: item.stockQuantity,
			warehouseId: item.warehouseId,
			checkStockId: item.checkStockId
		})) || [];
		// 同时更新所有添加过的物料列表
		state.allAddedMaterials = [...state.tableData];
	} catch (error) {
		console.error('获取盘点单明细失败:', error);
	} finally {
		state.tableLoading = false;
	}
}

// 对话框关闭处理
const handleDialogClosed = () => {
	state.tableData = []
	state.formData = {
		checkBillCode: '',
		warehouseId: undefined,
		checkRemark: ''
	}
	state.showDialog = false;
}

// 获取表格行key
const getRowKey = (row: any) => {
	return row.id
}

// 表格行点击处理
const handleRowClick = (row: any) => {
	console.log(row)
}

// 打开物料选择对话框
const openMaterialDialog = () => {
	if (!state.formData.warehouseId) {
		ElMessage.warning('请先选择盘点仓库')
		return
	}
	materialDialogRef.value.openDialog(state.formData.warehouseId, state.allAddedMaterials, state.tableData)
}

// 添加物料到明细列表
const handleAddMaterial = (materials: any[]) => {
	const existingIds = state.tableData.map(item => item.id)
	const selectedIds = materials.map(item => item.id)
	
	// 添加新选中的物料
	materials.forEach(material => {
		const isExist = state.tableData.find(item => item.id === material.id)
		if (!isExist) {
			state.tableData.push(material)
			// 同时添加到所有添加过的物料列表
			const isInAllAdded = state.allAddedMaterials.find(item => item.id === material.id)
			if (!isInAllAdded) {
				state.allAddedMaterials.push(material)
			}
		}
	})
	
	// 移除取消勾选的已存在物料
	existingIds.forEach(id => {
		if (!selectedIds.includes(id)) {
			const index = state.tableData.findIndex(item => item.id === id)
			if (index > -1) {
				state.tableData.splice(index, 1)
			}
		}
	})
}

// 删除明细
const handleDelete = (row: any) => {
	const index = state.tableData.findIndex(item => item.id === row.id)
	if (index > -1) {
		state.tableData.splice(index, 1)
	}
}

// 提交表单
const handleSubmit = async () => {
	// 验证明细列表
	if (state.tableData.length === 0) {
		ElMessage.warning('请添加明细')
		return
	}
	// 验证仓库选择
	if (!state.formData.warehouseId) {
		ElMessage.warning('请选择盘点仓库')
		return
	}
	try {
		submitLoading.value = true
		console.log(state.tableData)
		// 构建明细列表数据
		const detailList = state.tableData.map(item => ({
			id: item.id===item.checkStockId?0: item.id, // 明细ID，新增时为0
			checkStockId:item.checkStockId,
			stockSlot: item.slotCode,
			stockCode: item.stockCode,
			stockQuantity: item.stockQuantity,
			stockLotNo: item.lotNo,
			materialId: item.materialId,
			materialCode: item.materialCode,
			materialName: item.materialName,
			materialStandard: item.materialStandard,
			inspectionStatus: item.inspectionStatus,
			warehouseId:item.warehouseId
		}))
		// 构建提交参数
		const params = {
			...state.formData,
			list: detailList
		}
		// 根据是否有ID判断是新增还是更新
		if (state.billId) {
			params.id = state.billId
			await wmsStockCheckOrderApi.update(params)
		} else {
			await wmsStockCheckOrderApi.add(params)
		}
		ElMessage.success('提交成功')
		emit('reloadTable')
		state.showDialog = false
	} catch (error: any) {
		console.error('提交失败:', error)
		ElMessage.error(error.message || '提交失败')
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

/* 明细列表区域样式 */
.detail-list-section {
	flex: 1;
	display: flex;
	flex-direction: column;
	min-height: 0;
	border: 1px solid #e4e7ed;
	border-radius: 4px;
	padding: 5px;
	height: 400px;
}

/* 表单项间距样式 */
:deep(.el-form--inline .el-form-item) {
	margin-bottom: 5px;
	margin-right: 15px;
}

/* 分页组件样式 */
.el-pagination {
	margin-top: 10px;
	justify-content: flex-end;
}
</style>
