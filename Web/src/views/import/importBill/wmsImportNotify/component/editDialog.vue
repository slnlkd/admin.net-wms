<template>
	<el-dialog v-model="dialogVisible" :title="dialogTitle" width="1350px" :close-on-click-modal="false"
		destroy-on-close @closed="handleDialogClosed">
		<!-- 上半部分：基础信息 -->
		<div class="base-info-section">
			<el-form :model="state" ref="formRef" label-width="100px" :rules="formRules">
				<el-row :gutter="35">
					<el-form-item v-show="false">
						<el-input v-model="state.ruleForm.id" />
					</el-form-item>
					<el-col xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb20">
						<el-form-item label="入库单号" prop="ruleForm.importBillCode">
							<el-input v-model="state.ruleForm.importBillCode" placeholder="自动生成单号" disabled />
						</el-form-item>
					</el-col>
					<el-col xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb20">
						<el-form-item label="ERP单号" prop="ruleForm.outerBillCode">
							<el-input v-model="state.ruleForm.outerBillCode" placeholder="请输入ERP单号" />
						</el-form-item>
					</el-col>
					<el-col xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb20">
						<el-form-item label="单据类型" prop="ruleForm.billType">
							<el-select clearable filterable v-model="state.ruleForm.billType" placeholder="请选择单据类型">
								<el-option v-for="(item, index) in state.dropdownData.billType ?? []" :key="index"
									:value="item.id" :label="item.billTypeName" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb20">
						<el-form-item label="入库仓库" prop="ruleForm.warehouseId">
							<el-select clearable filterable v-model="state.ruleForm.warehouseId" placeholder="请选择入库仓库">
								<el-option v-for="(item, index) in state.dropdownData.warehouseId ?? []" :key="index"
									:value="item.id" :label="item.warehouseName" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb20">
						<el-form-item label="制造单位" prop="ruleForm.manufacturerName">
							<el-input v-model="state.ruleForm.manufacturerName" placeholder="请选择制造单位" readonly>
								<template #append>
									<el-button @click="openUnitDialog('manufacturer')" icon="ele-Search" />
								</template>
							</el-input>
						</el-form-item>
					</el-col>
					<el-col xs="24" :sm="8" :md="8" :lg="8" :xl="8" class="mb20">
						<el-form-item label="供货单位" prop="ruleForm.supplierName">
							<el-input v-model="state.ruleForm.supplierName" placeholder="请选择供应单位" readonly>
								<template #append>
									<el-button @click="openUnitDialog('supplier')" icon="ele-Search" />
								</template>
							</el-input>
						</el-form-item>
					</el-col>
				</el-row>
			</el-form>
		</div>

		<!-- 下半部分：物料列表 -->
		<div class="material-list-section">
			<div class="button-area">
				<el-button type="primary" icon="ele-Plus" @click="openMaterialDialog">添加明细</el-button>
				<el-button type="primary" icon="ele-Check" @click="handleSubmit"
					:loading="submitLoading">立即提交</el-button>
			</div>
			<el-table :data="visibleMaterialList" v-loading="state.tableLoading" border style="width: 100%;">
				<el-table-column type="index" label="序号" width="50" align="center" />
				<el-table-column prop='materialCode' label='物料编码' align="center" width="120" show-overflow-tooltip />
				<el-table-column prop='materialName' label='物料名称' align="center" show-overflow-tooltip />
				<el-table-column prop='materialStandard' label='物料规格' align="center" width="120"
					show-overflow-tooltip />
				<el-table-column prop='unitName' align="center" width="80" label='计量单位' show-overflow-tooltip />
				<el-table-column prop="boxQuantity" label="箱数量" width="80" align="center" />
				<el-table-column prop="lotNo" label="批次" width="80" align="center">
					<template #default="scope">
						<el-input v-model="scope.row.lotNo" placeholder="请输入"
							@input="handleLotNoInput(scope.row, $event)" @blur="validateLotNo(scope.row)" />
					</template>
				</el-table-column>
				<el-table-column prop="materialStatus" label="状态" width="90" align="center">
					<template #default="scope">
						<g-sys-dict v-model="scope.row.materialStatus" code="MaterialStatus" render-as="select"
							placeholder="请选择状态" clearable filterable />
					</template>
				</el-table-column>
				<el-table-column prop="importQuantity" label="数量" width="80" align="center">
					<template #default="scope">
						<el-input v-model="scope.row.importQuantity" placeholder="请输入"
							@input="handleQuantityInput(scope.row, $event)" @blur="validateQuantity(scope.row)" />
					</template>
				</el-table-column>
				<el-table-column prop="importProductionDate" label="生产日期" width="120" align="center">
					<template #default="scope">
						<el-date-picker v-model="scope.row.importProductionDate" type="date" placeholder="选择生产日期"
							value-format="YYYY-MM-DD" :disabled-date="disableFutureDates"
							@change="(date) => handleProductionDateChange(scope.row, date)" style="width: 100%" />
					</template>
				</el-table-column>
				<el-table-column prop="importLostDate" label="有效期至" width="100" align="center">
					<template #default="scope">
						<el-input v-model="scope.row.importLostDate" placeholder="自动计算" readonly
							:class="{ 'expired-date': isDateExpired(scope.row.importLostDate) }" />
					</template>
				</el-table-column>
				<el-table-column prop="remark" label="备注" align="center">
					<template #default="scope">
						<el-input v-model="scope.row.remark" placeholder="请输入" maxlength="50" show-word-limit />
					</template>
				</el-table-column>
				<el-table-column label="操作" width="80" align="center" fixed="right">
					<template #default="scope">
						<el-button type="danger" icon="ele-Delete" size="small" text
							@click="removeMaterial(scope.$index)">删除</el-button>
					</template>
				</el-table-column>
			</el-table>
		</div>

		<!-- 单位选择弹窗 - 修改为单选 -->
		<el-dialog v-model="unitDialogVisible" :title="unitDialogTitle" width="800px" append-to-body
			@closed="handleUnitDialogClosed">
			<div style="margin-bottom: 15px;">
				<el-input v-model="unitKeyword" placeholder="请输入关键字搜索" @clear="handleUnitClear" clearable
					style="width: 300px">
					<template #append>
						<el-button icon="ele-Search" @click="searchUnits" />
					</template>
				</el-input>
			</div>
			<el-table :data="unitList" border @selection-change="handleUnitSelectionChange"
				@row-click="handleUnitRowClick" :row-key="getUnitRowKey" ref="unitTableRef">
				<el-table-column type="selection" width="55" align="center" :reserve-selection="true" />
				<el-table-column prop="customerCode" label="单位编码" width="120" align="center" />
				<el-table-column prop="customerName" label="单位名称" width="120" align="center" />
				<el-table-column prop="customerAddress" label="单位地址" align="center" />
				<el-table-column prop="customerLinkman" label="联系人" width="120" align="center" />
				<el-table-column prop="customerPhone" label="联系电话" width="150" align="center" />
			</el-table>
			<template #footer>
				<el-button @click="unitDialogVisible = false">取消</el-button>
				<el-button type="primary" @click="confirmUnitSelection">确定</el-button>
			</template>
		</el-dialog>
		<!-- 物料选择弹窗 -->
		<el-dialog v-model="materialDialogVisible" title="选择物料" width="1000px" @closed="handleMaterialDialogClosed"
			append-to-body>
			<div style="margin-bottom: 15px;">
				<el-input v-model="materialKeyword" placeholder="请输入物料编码或名称搜索" @clear="handleMaterialClear" clearable
					style="width: 300px">
					<template #append>
						<el-button icon="ele-Search" @click="searchMaterials" />
					</template>
				</el-input>
			</div>
			<el-table :data="materialOptions" border @selection-change="handleMaterialSelectionChange"
				@row-click="handleMaterialRowClick" ref="materialTableRef">
				<el-table-column type="selection" width="55" align="center" />
				<el-table-column prop="materialCode" label="物料编码" width="150" align="center" />
				<el-table-column prop="materialName" label="物料名称" width="150" align="center" />
				<el-table-column prop="materialStandard" label="物料规格" width="120" align="center" />
				<el-table-column prop="materialUnitFkDisplayName" label="计量单位" width="100" align="center" />
				<el-table-column prop="warehouseFkDisplayName" label="所属仓库" width="120" align="center" />
				<el-table-column prop="boxQuantity" label="箱数量" width="100" align="center" />
				<el-table-column prop="remark" label="备注" align="center" />
			</el-table>
			<template #footer>
				<el-button @click="materialDialogVisible = false">取消</el-button>
				<el-button type="primary" @click="confirmMaterialSelection">确定</el-button>
			</template>
		</el-dialog>
	</el-dialog>
</template>

<script lang="ts" name="wmsImportNotify" setup>
import { ref, reactive, onMounted, nextTick, computed, watch } from 'vue'
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from 'element-plus'
import { useWmsImportNotifyApi } from '/@/api/import/importBill/wmsImportNotify';
import { useWmsImportNotifyDetailApi } from '/@/api/import/importBill/wmsImportNotifyDetail';
import { useWmsBaseSupplierApi } from '/@/api/base/baseSupplier/wmsBaseSupplier';
import { useWmsBaseCustomerApi } from '/@/api/base/baseCustomer/wmsBaseCustomer';
import { useWmsBaseManufacturerApi } from '/@/api/base/baseManufacturer/wmsBaseManufacturer';
import { useWmsBaseBillTypeApi } from '/@/api/base/billType/wmsBaseBillType';
import { useWmsBaseWareHouseApi } from '/@/api/base/baseWarehouse/wmsBaseWareHouse';
import { useWmsBaseMaterialApi } from '/@/api/base/baseMaterial/wmsBaseMaterial';

// 类型定义
interface MaterialRow {
	id?: number
	materialId: number
	materialCode: string
	materialName: string
	materialStandard: string
	unitName: string
	boxQuantity: number
	lotNo: string
	materialStatus: number | string
	importQuantity: number
	importCompleteQuantity: number
	importFactQuantity: number
	uploadQuantity: number
	taskStatus: number
	printFlag: number
	labelJudgment: number
	importProductionDate: string
	importLostDate: string
	mroductionDate: string
	isDelete: boolean
	remark?: string
}

interface UnitRow {
	id: number
	customerCode: string
	customerName: string
	customerAddress: string
	customerLinkman: string
	customerPhone: string
}

interface BillTypeRow {
	id: number
	billTypeName: string
	qualityInspectionStatus: number
}

interface PaginationParams {
	page: number
	pageSize: number
	total: number
	field: string
	order: string
	descStr: string
}

interface State {
	tableLoading: boolean
	ruleForm: Record<string, any>
	tableQueryParams: Record<string, any>
	tableUnitQueryParams: Record<string, any>
	tableMaterialQueryParams: Record<string, any>
	tableParams: PaginationParams & { ImportId: number }
	tableUnitParams: PaginationParams & { ImportId: number }
	tableMaterialParams: PaginationParams & { ImportId: number }
	dropdownParams: PaginationParams
	dropdownQueryParams: Record<string, any>
	dropdownData: {
		billType?: BillTypeRow[]
		warehouseId?: any[]
	}
}

// 常量定义
const CONSTANTS = {
	BILL_TYPE_IMPORT: 1,
	DEFAULT_PAGE_SIZE: 200,
	EXPIRY_DAYS: 180,
	EXECUTE_FLAG_PENDING: '01',
	LABEL_JUDGMENT_PRINTABLE: 2,
	TASK_STATUS_NOT_STARTED: 0,
	MATERIAL_STATUS_DEFAULT: 0
} as const

const DEFAULT_PAGINATION: PaginationParams = {
	page: 1,
	pageSize: CONSTANTS.DEFAULT_PAGE_SIZE,
	total: 0,
	field: 'createTime',
	order: 'descending',
	descStr: 'descending'
}

//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const wmsImportNotifyApi = useWmsImportNotifyApi();
const wmsImportNotifyDetailApi = useWmsImportNotifyDetailApi();
const wmsBaseSupplierApi = useWmsBaseSupplierApi();
const wmsBaseCustomerApi = useWmsBaseCustomerApi();
const wmsBaseManufacturerApi = useWmsBaseManufacturerApi();
const wmsBaseBillTypeApi = useWmsBaseBillTypeApi();
const wmsBaseWareHouseApi = useWmsBaseWareHouseApi();
const wmsBaseMaterialApi = useWmsBaseMaterialApi();
// 添加单位表格引用
const unitTableRef = ref()
// 添加物料表格引用
const materialTableRef = ref()
// 弹窗显示控制
const dialogVisible = ref(false)
const dialogTitle = ref('新增入库单')
const submitLoading = ref(false)
// 物料列表数据
const materialList = ref<MaterialRow[]>([])
// 添加一个响应式变量来存储当前选中的单据类型的质检状态
const currentBillTypeQualityStatus = ref<number | null>(null)
// 表单数据
const formRef = ref<FormInstance>()
const state = reactive<State>({
	tableLoading: false,
	ruleForm: {} as any,
	tableQueryParams: {} as any,
	tableUnitQueryParams: {} as any,
	tableMaterialQueryParams: {} as any,
	tableParams: {
		...DEFAULT_PAGINATION,
		ImportId: 0
	},
	tableUnitParams: {
		...DEFAULT_PAGINATION,
		ImportId: 0
	},
	tableMaterialParams: {
		...DEFAULT_PAGINATION,
		ImportId: 0
	},
	dropdownParams: {
		...DEFAULT_PAGINATION
	},
	dropdownQueryParams: {} as any,
	dropdownData: {} as any,

})

// 表单验证规则 
const formRules: FormRules = {
	'ruleForm.billType': [
		{ required: true, message: '请选择单据类型', trigger: 'change' }
	],
	'ruleForm.warehouseId': [
		{ required: true, message: '请选择入库仓库', trigger: 'change' }
	]
}

// 单位选择相关
const unitDialogVisible = ref(false)
const unitDialogTitle = ref('')
const unitKeyword = ref('')
const unitList = ref<UnitRow[]>([])
const selectedUnits = ref<UnitRow[]>([])
let currentUnitType: 'manufacturer' | 'supplier' | 'customer' | '' = ''

// 物料选择相关
const materialDialogVisible = ref(false)
const materialKeyword = ref('')
const materialOptions = ref<any[]>([])
const selectedMaterials = ref<any[]>([])
// 添加一个变量来保存旧的单据类型值
const oldBillType = ref('')
const isRestoringOldValue = ref(false)
// 监听单据类型变化
watch(() => state.ruleForm.billType, (newValue, oldValue) => {
	// 如果是正在恢复旧值，不触发弹窗
	if (isRestoringOldValue.value) {
		isRestoringOldValue.value = false
		return
	}
	if (newValue !== oldValue && materialList.value.length > 0) {
		oldBillType.value = oldValue // 保存旧值
		handleBillTypeChange()
	}
	if (newValue) {
		// 根据选中的单据类型ID查找对应的质检状态
		
		const selectedBillType = state.dropdownData.billType?.find(item => item.id === newValue)
		console.log(selectedBillType)
		if (selectedBillType) {
			
			currentBillTypeQualityStatus.value = selectedBillType.qualityInspectionStatus
			console.log('当前单据类型质检状态:', currentBillTypeQualityStatus.value)
		}
	} else {
		currentBillTypeQualityStatus.value = null
	}
})

// 处理单据类型变化
const handleBillTypeChange = () => {
	ElMessageBox.confirm('切换单据类型将清空当前物料明细，是否继续？', '提示', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'warning'
	}).then(() => {
		// 用户确认，清空物料明细
		materialList.value = []
		ElMessage.success('已清空物料明细')
		oldBillType.value = '' // 清空保存的旧值
	}).catch(() => {
		// 用户取消，恢复原来的单据类型
		if (oldBillType.value !== '') {
			isRestoringOldValue.value = true // 设置标志，防止触发监听器
			state.ruleForm.billType = oldBillType.value
			oldBillType.value = '' // 清空保存的旧值
		}
	})
}
// 页面加载时
onMounted(async () => {
	state.dropdownQueryParams.BillType = CONSTANTS.BILL_TYPE_IMPORT;
	const billdata = await wmsBaseBillTypeApi.page(Object.assign(state.dropdownQueryParams, state.dropdownParams)).then(res => res.data.result);
	state.dropdownData.billType = billdata.items;
	const warehousedata = await wmsBaseWareHouseApi.page(Object.assign(state.dropdownQueryParams, state.dropdownParams)).then(res => res.data.result);
	state.dropdownData.warehouseId = warehousedata.items;
});

// 打开弹窗
const openDialog = async (row: any = null, title: string = '新增入库单') => {
	state.tableLoading = true;
	dialogTitle.value = title
	dialogVisible.value = true
	// 重置数据
	resetFormData()
	materialList.value = []
	// 重置表单
	if (formRef.value) {
		formRef.value.resetFields()
	}
	// 如果是编辑模式，填充数据
	if (row && row.id) {
		try {
			state.ruleForm = row.id ? await wmsImportNotifyApi.detail(row.id).then(res => res.data.result) : JSON.parse(JSON.stringify(row));
			state.tableParams.ImportId = row.id ? row.id : 0;
			const result = await wmsImportNotifyDetailApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
			materialList.value = result?.items ?? [];
		} catch (error) {
			console.error('加载编辑数据失败:', error)
			ElMessage.error('加载数据失败')
		}
	} else {
		// 新增模式，生成入库单号
		//generateImportBillCode()
		state.ruleForm.id = 0;
		materialList.value = []
	}
	state.tableLoading = false;
}
// 弹窗关闭时的处理
const handleDialogClosed = () => {
	resetFormData()
	materialList.value = []
	oldBillType.value = '' // 重置保存的旧值
	isRestoringOldValue.value = false // 重置恢复标志
	if (formRef.value) {
		formRef.value.clearValidate()
	}
}

// 重置表单数据
const resetFormData = () => {
	state.ruleForm = {

	}
	oldBillType.value = '' // 重置保存的旧值
	isRestoringOldValue.value = false // 重置恢复标志
}
// 获取单位行的唯一标识
const getUnitRowKey = (row: any) => {
	return row.id || row.CustomerCode || Math.random().toString()
}

// 单位API映射策略
const unitApiMap = {
	manufacturer: wmsBaseManufacturerApi,
	supplier: wmsBaseSupplierApi,
	customer: wmsBaseCustomerApi
} as const

// 加载单位数据
const loadUnitData = async (params: any = {}) => {
	if (!currentUnitType || currentUnitType === '') {
		unitList.value = []
		return
	}

	state.tableUnitParams = Object.assign(state.tableUnitParams, params)
	const api = unitApiMap[currentUnitType]

	if (!api) {
		unitList.value = []
		return
	}

	const response = await api.page(Object.assign(state.tableUnitQueryParams, state.tableUnitParams))
	unitList.value = response?.data?.result?.items || []
}

// 搜索单位
const searchUnits = () => {
	state.tableUnitQueryParams.keyword = unitKeyword.value;
	loadUnitData()
}
// 处理清除搜索关键词
const handleUnitClear = () => {
	// 清空关键词后重新加载数据
	state.tableUnitQueryParams.keyword = '';
	unitKeyword.value = '';
	loadUnitData();
}
// 弹窗关闭时的处理
const handleUnitDialogClosed = () => {
	state.tableUnitQueryParams.keyword = '';
	unitKeyword.value = '';
}
// 单位选择变化 - 修改为单选逻辑
const handleUnitSelectionChange = (selection: any[]) => {
	// 如果是单选，只保留最后一个选中的项
	if (selection.length > 1) {
		const lastSelected = selection[selection.length - 1]
		selectedUnits.value = [lastSelected]
		// 更新表格的选中状态
		nextTick(() => {
			if (unitTableRef.value) {
				unitTableRef.value.clearSelection()
				unitTableRef.value.toggleRowSelection(lastSelected, true)
			}
		})
	} else {
		selectedUnits.value = selection
	}
}

// 点击行时选择
const handleUnitRowClick = (row: any) => {
	if (unitTableRef.value) {
		unitTableRef.value.toggleRowSelection(row)
	}
}

// 确认单位选择
const confirmUnitSelection = () => {
	if (selectedUnits.value.length === 0) {
		ElMessage.warning('请选择单位')
		return
	}

	const selectedUnit = selectedUnits.value[0]
	switch (currentUnitType) {
		case 'manufacturer':
			state.ruleForm.manufacturerName = selectedUnit.customerName
			state.ruleForm.manufacturerId = selectedUnit.id
			break
		case 'supplier':
			state.ruleForm.supplierName = selectedUnit.customerName
			state.ruleForm.supplierId = selectedUnit.id
			break
		case 'customer':
			state.ruleForm.customerName = selectedUnit.customerName
			state.ruleForm.customerId = selectedUnit.id
			break
	}

	unitDialogVisible.value = false
	// 清空选择
	selectedUnits.value = []
	if (unitTableRef.value) {
		unitTableRef.value.clearSelection()
	}
}

// 打开单位选择弹窗时清空选择
const openUnitDialog = (type: string) => {
	currentUnitType = type
	unitDialogVisible.value = true
	unitKeyword.value = ''
	selectedUnits.value = []

	// 清空表格选择
	nextTick(() => {
		if (unitTableRef.value) {
			unitTableRef.value.clearSelection()
		}
	})

	switch (type) {
		case 'manufacturer':
			unitDialogTitle.value = '选择制造商单位'
			break
		case 'supplier':
			unitDialogTitle.value = '选择供货单位'
			break
		case 'customer':
			unitDialogTitle.value = '选择客户单位'
			break
	}
	// 加载单位数据
	loadUnitData()
}

// 打开物料选择弹窗
const openMaterialDialog = () => {
	console.log(state.ruleForm.billType)
	if (state.ruleForm.billType == 0 || state.ruleForm.billType == undefined) {
		ElMessage.warning('请选择单据类型')
		return;
	}
	if (state.ruleForm.warehouseId == 0 || state.ruleForm.warehouseId == undefined) {
		ElMessage.warning('请选择入库仓库')
		return;
	}
	materialDialogVisible.value = true
	materialKeyword.value = ''
	selectedMaterials.value = []
	loadMaterialData()
}

// 加载物料数据
const loadMaterialData = async (params: any = {}) => {
	state.tableMaterialParams = Object.assign(state.tableMaterialParams, params);
	let response;
	response = await wmsBaseMaterialApi.page(Object.assign(state.tableMaterialQueryParams, state.tableMaterialParams));
	if (response && response.data && response.data.result) {
		materialOptions.value = response.data.result.items || response.data.result || [];
	} else {
		materialOptions.value = [];
	}
}

// 搜索物料
const searchMaterials = () => {
	state.tableMaterialQueryParams.keyword = materialKeyword.value;
	loadMaterialData()
}
// 处理清除搜索关键词
const handleMaterialClear = () => {
	// 清空关键词后重新加载数据
	state.tableMaterialQueryParams.keyword = '';
	materialKeyword.value = '';
	loadMaterialData();
}
// 弹窗关闭时的处理
const handleMaterialDialogClosed = () => {
	state.tableMaterialQueryParams.keyword = '';
	materialKeyword.value = '';
}
// 物料选择变化
const handleMaterialSelectionChange = (selection: any[]) => {
	selectedMaterials.value = selection
}
// 点击行时选择物料
const handleMaterialRowClick = (row: any) => {
	if (materialTableRef.value) {
		materialTableRef.value.toggleRowSelection(row)
	}
}
// 确认物料选择
const confirmMaterialSelection = () => {
	if (selectedMaterials.value.length === 0) {
		ElMessage.warning('请选择物料')
		return
	}
	const currentDate = getCurrentDate()
	const expiryDate = calculateExpiryDate(currentDate)
	// 添加选中的物料到列表，并设置默认值
	let listno = 1;
	selectedMaterials.value.forEach(material => {
		materialList.value.push({
			...material,
			materialId: material.id,
			importListNo: listno++,
			boxQuantity: material.boxQuantity || 1,
			lotNo: '',
			unitName: material.materialUnitFkDisplayName || '',
			importExecuteFlag: CONSTANTS.EXECUTE_FLAG_PENDING,
			materialStatus: String(currentBillTypeQualityStatus.value ?? CONSTANTS.MATERIAL_STATUS_DEFAULT),
			importQuantity: material.importQuantity || undefined,
			importCompleteQuantity: 0,
			importFactQuantity: 0,
			uploadQuantity: 0,
			taskStatus: CONSTANTS.TASK_STATUS_NOT_STARTED,
			printFlag: 0,
			labelJudgment: CONSTANTS.LABEL_JUDGMENT_PRINTABLE,
			importProductionDate: currentDate, // 默认设置为当前日期
			importLostDate: expiryDate, // 自动计算有效期至
			mroductionDate: '未入库',
			isDelete: false // 新增删除标记
		})
	})
	materialDialogVisible.value = false
}

// // 验证箱数量 - 修改为允许小数
// const validateBoxQuantity = (row: any) => {
// 	if (row.boxQuantity) {
// 		const numValue = parseFloat(row.boxQuantity)
// 		if (isNaN(numValue) || numValue <= 0) {
// 			ElMessage.warning('箱数量必须大于0')
// 			row.boxQuantity = ''
// 		} else {
// 			// 格式化显示，保留最多3位小数
// 			row.boxQuantity = numValue.toString()
// 		}
// 	}
// }
// 批次输入处理
const handleLotNoInput = (row: any, value: string) => {
	// 只允许输入数字、字母和常见符号，去掉特殊字符
	row.lotNo = value.replace(/[^\w\u4e00-\u9fa5\-_]/g, '')
	// 实时校验批次是否重复
	validateLotNoDuplicate(row)
}
// 实时校验批次重复
const validateLotNoDuplicate = (row: any) => {
	if (!row.lotNo || row.lotNo.trim() === '') {
		return true // 空值不校验
	}

	// 查找是否有相同物料编码和批次的记录（排除自身）
	const duplicate = materialList.value.find(item =>
		!item.isDelete &&
		item !== row && // 排除自身
		item.materialCode === row.materialCode &&
		item.lotNo === row.lotNo
	)

	if (duplicate) {
		ElMessage.warning(`物料 ${row.materialCode} 的批次 ${row.lotNo} 已存在，请使用其他批次`)
		// 清空当前批次的输入
		row.lotNo = ''
		return false
	}

	return true
}
// 验证批次 - 修改为同时校验重复和格式
const validateLotNo = (row: any) => {
	// 先校验重复
	if (!validateLotNoDuplicate(row)) {
		return
	}

	// 原有的格式校验
	if (row.lotNo && row.lotNo.trim() === '') {
		ElMessage.warning('批次不能为空')
		row.lotNo = ''
	}
}

// 数量输入处理 - 允许最多3位小数
const handleQuantityInput = (row: any, value: string) => {
	// 允许数字和小数点，限制小数位数
	let formattedValue = value.replace(/[^\d.]/g, '')

	// 处理多个小数点的情况，只保留第一个
	const dotCount = formattedValue.split('.').length - 1
	if (dotCount > 1) {
		const parts = formattedValue.split('.')
		formattedValue = parts[0] + '.' + parts.slice(1).join('')
	}

	// 限制小数位数为3位
	if (formattedValue.includes('.')) {
		const parts = formattedValue.split('.')
		if (parts[1].length > 3) {
			formattedValue = parts[0] + '.' + parts[1].substring(0, 3)
		}
	}

	row.importQuantity = formattedValue
}

// 验证数量 - 修改为允许小数
const validateQuantity = (row: any) => {
	if (row.importQuantity) {
		const numValue = parseFloat(row.importQuantity)
		if (isNaN(numValue) || numValue <= 0) {
			ElMessage.warning('数量必须大于0')
			row.importQuantity = ''
		} else {
			// 格式化显示，保留最多3位小数
			row.importQuantity = numValue.toString()
		}
	}
}
// 日期格式化工具函数
const formatDateToString = (date: Date): string => {
	const year = date.getFullYear()
	const month = String(date.getMonth() + 1).padStart(2, '0')
	const day = String(date.getDate()).padStart(2, '0')
	return `${year}-${month}-${day}`
}

// 获取当前日期，格式化为 YYYY-MM-DD
const getCurrentDate = () => {
	return formatDateToString(new Date())
}

// 计算有效期至（生产日期 + 有效期天数）
const calculateExpiryDate = (productionDate: string) => {
	if (!productionDate) return ''

	const productionDateObj = new Date(productionDate)
	const expiredDateObj = new Date(productionDateObj)
	expiredDateObj.setDate(productionDateObj.getDate() + CONSTANTS.EXPIRY_DAYS)

	return formatDateToString(expiredDateObj)
}

// 修改生产日期变化处理函数
const handleProductionDateChange = (row: any, productionDate: string) => {
	if (productionDate) {
		const productionDateObj = new Date(productionDate)
		const expiredDateObj = new Date(productionDateObj)
		expiredDateObj.setDate(productionDateObj.getDate() + CONSTANTS.EXPIRY_DAYS)

		row.importLostDate = formatDateToString(expiredDateObj)
	} else {
		row.importLostDate = ''
	}
}

// 禁用未来日期（生产日期不能选择未来的日期）
const disableFutureDates = (time: Date) => {
	return time.getTime() > Date.now()
}

// 检查日期是否已过期
const isDateExpired = (dateString: string) => {
	if (!dateString) return false
	const expiredDate = new Date(dateString)
	const today = new Date()
	today.setHours(0, 0, 0, 0) // 清除时间部分，只比较日期
	return expiredDate < today
}
// 删除物料
const removeMaterial = (index: number) => {
	// 直接根据 visibleMaterialList 的索引找到对应的物料
	const materialToDelete = visibleMaterialList.value[index]

	// 在原始列表中找到并标记删除
	const item = materialList.value.find(item => item === materialToDelete)
	if (item) {
		item.isDelete = true
		ElMessage.success('已标记删除')
	}
}
// 计算属性：只显示未删除的物料
const visibleMaterialList = computed(() => {
	return materialList.value.filter(item => !item.isDelete)
})

// 提交表单
const handleSubmit = async () => {
	if (!formRef.value) return

	try {
		const valid = await formRef.value.validate()
		if (!valid) return

		// 只校验未删除的物料
		const visibleMaterials = visibleMaterialList.value
		if (visibleMaterials.length === 0) {
			ElMessage.warning('请至少添加一条物料明细')
			return
		}

		// 校验物料明细数据
		const validationResult = validateMaterialList(visibleMaterials)
		if (!validationResult.valid) {
			ElMessage.warning(validationResult.message)
			return
		}

		submitLoading.value = true

		// 提交数据
		const values = {
			...state.ruleForm,
			DetailList: materialList.value
		}

		console.log('提交数据:', values)
		await wmsImportNotifyApi[values.id ? 'update' : 'add'](values)

		ElMessage.success('操作成功')
		dialogVisible.value = false
		emit('reloadTable')

	} catch (error: any) {
		console.error('提交失败:', error)
		const errorMessage = error?.response?.data?.message || error?.message || '提交失败，请重试'
		ElMessage.error(errorMessage)
	} finally {
		submitLoading.value = false
	}
}
// 校验物料列表数据 - 修改为支持小数
const validateMaterialList = (materials: any[]) => {
	for (let i = 0; i < materials.length; i++) {
		const material = materials[i]

		// 校验数量必须大于0（支持小数）
		if (!material.importQuantity || parseFloat(material.importQuantity) <= 0) {
			return {
				valid: false,
				message: `第${i + 1}行物料的数量必须大于0`
			}
		}

		// 校验批次不能为空
		if (!material.lotNo || material.lotNo.trim() === '') {
			return {
				valid: false,
				message: `第${i + 1}行物料的批次不能为空`
			}
		}

		// 校验批次是否重复
		const duplicateIndex = materials.findIndex((item, index) =>
			index !== i &&
			item.materialCode === material.materialCode &&
			item.lotNo === material.lotNo
		)

		if (duplicateIndex !== -1) {
			return {
				valid: false,
				message: `第${i + 1}行和第${duplicateIndex + 1}行物料的物料编码和批次重复`
			}
		}
	}

	return {
		valid: true,
		message: '校验通过'
	}
}

// 暴露方法给父组件
defineExpose({
	openDialog
})

</script>


<style scoped>
.base-info-section {
	/* height: 90px; Removed fixed height */
	margin-bottom: 10px;
	padding: 10px;
	border: 1px solid #e4e7ed;
	border-radius: 4px;
}

.material-list-section {
	/* height: 350px; Removed fixed height */
	min-height: 300px; /* Ensure a minimum visible area */
	max-height: 50vh;  /* Limit max height relative to viewport */
	overflow-y: auto;  /* Allow scrolling if content overflows */
	border: 1px solid #e4e7ed;
	border-radius: 4px;
	display: flex;
	flex-direction: column;
}

.button-area {
	margin: 5px 0 5px 5px;
	display: flex;
	justify-content: flex-start;
	gap: 10px;
	padding: 5px; /* Add some padding */
}

/* ... (rest of styles) ... */
</style>