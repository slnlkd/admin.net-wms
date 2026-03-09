<script lang="ts" name="wmsPrmissionScope" setup>
import { ref, reactive, onMounted, watch, computed, nextTick } from "vue";
import { ElMessage, ElTable } from "element-plus";
import type { FormRules } from "element-plus";
import { formatDate } from '/@/utils/formatTime';
import { useWmsPrmissionScopeApi } from '/@/api/system/prmissionScope/wmsPrmissionScope';

// 定义类型
interface DropdownItem {
	value: string | number;
	label: string;
}

interface TableColumn {
	prop: string;
	label: string;
	width?: string;
}

interface TableDataItem {
	[key: string]: any;
	_isSelected?: boolean; // 添加选中状态标记
}

interface DropdownData {
	userId: DropdownItem[];
	tableList: DropdownItem[];
	columnList: DropdownItem[];
	tableData: TableDataItem[];
	fieldValueOptions: DropdownItem[];
}

interface State {
	title: string;
	loading: boolean;
	showDialog: boolean;
	isInitSelection:boolean;// 是否已初始化表格选中状态
	ruleForm: any;
	stores: any;
	dropdownData: DropdownData;
	tableLoading: boolean;
	selectedFieldValues: string[];
	tableColumns: TableColumn[];
	tableSelection: TableDataItem[]; // 存储表格选中的行数据
}

//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const wmsPrmissionScopeApi = useWmsPrmissionScopeApi();
const ruleFormRef = ref();
const dataTableRef = ref<InstanceType<typeof ElTable>>();

const state = reactive<State>({
	title: '',
	loading: false,
	showDialog: false,
	ruleForm: {} as any,
	stores: {},
	isInitSelection: false,// 是否已初始化表格选中状态
	dropdownData: {
		userId: [],
		tableList: [],
		columnList: [],
		tableData: [],
		fieldValueOptions: []
	},
	tableLoading: false,
	selectedFieldValues: [],
	tableColumns: [],
	tableSelection: [] // 初始化表格选中数据
});

// 自行添加其他规则
const rules = ref<FormRules>({
	userId: [{ required: true, message: '请选择用户！', trigger: 'blur' }],
	tableName: [{ required: true, message: '请选择功能名称！', trigger: 'change' }],
	fieldName: [{ required: true, message: '请选择字段名！', trigger: 'change' }],
	fieldValue: [{ required: true, message: '请选择字段值！', trigger: 'change' }],
});

// 计算属性 - 当前选中字段的值选项
const currentFieldValues = computed(() => {
	if (!state.ruleForm.fieldName || !state.dropdownData.tableData.length) return [];

	const uniqueValues = new Set<string>();
	state.dropdownData.tableData.forEach(item => {
		const value = item[state.ruleForm.fieldName];
		if (value !== null && value !== undefined && value !== '') {
			uniqueValues.add(String(value));
		}
	});

	return Array.from(uniqueValues).map(value => ({
		value: value,
		label: value
	}));
});

// 页面加载时
onMounted(async () => {
	await loadDropdownData();
});

// 计算属性过滤列
const filteredColumns = computed(() => {
  return state.tableColumns.filter(column => {
    // 排除不需要的列(黑名单)
    return !['IsDelete'].includes(column.prop)
  })
})

// 加载下拉数据
const loadDropdownData = async () => {
	try {
		state.loading = true;
		const data = await wmsPrmissionScopeApi.getDropdownData(false).then(res => res.data.result) ?? {};
		state.dropdownData.userId = data.userId ?? [];
		state.dropdownData.tableList = data.tableList ?? [];
	} catch (error) {
		ElMessage.error('加载下拉数据失败');
		console.error('加载下拉数据失败:', error);
	} finally {
		state.loading = false;
	}
};

// 根据表获取表列+表数据
const getTableDetail = async (tableName: string) => {
	if (!tableName) {
		state.dropdownData.columnList = [];
		state.dropdownData.tableData = [];
		state.tableColumns = [];
		state.ruleForm.fieldName = '';
		state.ruleForm.fieldValue = '';
		state.selectedFieldValues = [];
		state.tableSelection = [];
		return;
	}

	try {
		state.tableLoading = true;
		const data = await wmsPrmissionScopeApi.getdropdownTableData(tableName).then(res => res.data.result) ?? {};

		state.dropdownData.columnList = data.columnList ?? [];
		state.dropdownData.tableData = data.tableData ?? [];

		// 生成表格列配置
		if (state.dropdownData.columnList.length > 0) {
			state.tableColumns = state.dropdownData.columnList.map(col => ({
				prop: col.value as string,
				label: col.label,
				width: '150'
			}));
		}

		// 重置字段相关数据
		state.ruleForm.fieldName = '';
		state.ruleForm.fieldValue = '';
		state.selectedFieldValues = [];
		state.tableSelection = [];
		state.dropdownData.fieldValueOptions = [];

	} catch (error) {
		ElMessage.error('加载表数据失败');
		console.error('加载表数据失败:', error);
	} finally {
		state.tableLoading = false;
	}
};

const handleTableFieldChange = (newVal: any) => {
	if (newVal) {
		// 字段名改变时，重置字段值
		state.ruleForm.fieldValue = '';
		state.selectedFieldValues = [];
		state.tableSelection = [];
		state.ruleForm.columnName = state.dropdownData?.columnList?.find(m => m.value == newVal)?.label;
		// 更新字段值选项
		state.dropdownData.fieldValueOptions = currentFieldValues.value;
		
		// 清空表格选中状态
		if (dataTableRef.value) {
			dataTableRef.value.clearSelection();
		}
	} else {
		state.dropdownData.fieldValueOptions = [];
		state.ruleForm.fieldValue = '';
		state.selectedFieldValues = [];
		state.tableSelection = [];
	}
};

// 处理字段值选择变化
const handleFieldValueChange = (values: string[]) => {
	state.selectedFieldValues = values;
	state.ruleForm.fieldValue = values.join(',');
	
	// 同步更新表格选中状态
	updateTableSelectionFromFieldValues();
};

// 根据字段值更新表格选中状态
const updateTableSelectionFromFieldValues = () => {
	if (!state.ruleForm.fieldName || !dataTableRef.value) return;
	
	// 清空当前选中
	dataTableRef.value.clearSelection();
	state.tableSelection = [];
	
	// 根据选中的字段值选中对应的表格行
	nextTick(() => {
		state.dropdownData.tableData.forEach((row, index) => {
			const fieldValue = String(row[state.ruleForm.fieldName]);
			if (state.selectedFieldValues.includes(fieldValue)) {
				state.isInitSelection = true;
				dataTableRef.value!.toggleRowSelection(row, true);
				state.tableSelection.push(row);
			}
		});
	});
};

// 表格选择变化事件
const handleTableSelectionChange = (selection: TableDataItem[]) => {
	if(!state.isInitSelection){
		state.tableSelection = selection;
	}
	
	// 从选中的行中提取字段值
	const selectedValues = new Set<string>();
	selection.forEach(row => {
		if (state.ruleForm.fieldName && row[state.ruleForm.fieldName] !== undefined) {
			selectedValues.add(String(row[state.ruleForm.fieldName]));
		}
	});
	
	// 更新字段值选择器
	if(state.isInitSelection){
		state.isInitSelection = false;
	}else{
		state.selectedFieldValues = Array.from(selectedValues);
		state.ruleForm.fieldValue = state.selectedFieldValues.join(',');
	}
	
};

// 表格行点击事件
const handleRowClick = (row: TableDataItem) => {
	if (!state.ruleForm.fieldName) {
		ElMessage.warning('请先选择字段名');
		return;
	}
	
	if (dataTableRef.value) {
		// 切换行的选中状态
		dataTableRef.value.toggleRowSelection(row);
	}
};

// 打开弹窗
const openDialog = async (row: any, title: string) => {
	state.title = title;
	state.ruleForm = {};
	state.selectedFieldValues = [];
	state.tableSelection = [];

	if (row?.id) {
		try {
			state.loading = true;
			const detail = await wmsPrmissionScopeApi.detail(row.id).then(res => res.data.result);
			state.ruleForm = { ...detail };

			// 如果已有表名，加载表数据
			if (state.ruleForm.tableName) {
				await getTableDetail(state.ruleForm.tableName);
				state.ruleForm.fieldName = detail.fieldName;
				state.ruleForm.fieldValue = detail.fieldValue;
				state.ruleForm.columnName = state.ruleForm.columnName || state.dropdownData?.columnList?.find(m => m.value == detail.fieldName)?.label;
				// 设置字段值选项
				state.dropdownData.fieldValueOptions = currentFieldValues.value;
				// 设置已选择的字段值
				if (state.ruleForm.fieldValue) {
					state.selectedFieldValues = state.ruleForm.fieldValue.split(',');
					
					// 等待表格渲染完成后设置选中状态
					setTimeout(() => {
						updateTableSelectionFromFieldValues();
					}, 0);
				}
			}
		} catch (error) {
			ElMessage.error('加载详情失败');
			console.error('加载详情失败:', error);
		} finally {
			state.loading = false;
		}
	} else {
		state.ruleForm = JSON.parse(JSON.stringify(row || {}));
	}

	state.showDialog = true;
};

// 关闭弹窗
const closeDialog = () => {
	emit("reloadTable");
	state.showDialog = false;
	// 重置表单和数据
	setTimeout(() => {
		state.ruleForm = {};
		state.selectedFieldValues = [];
		state.tableSelection = [];
		state.dropdownData.columnList = [];
		state.dropdownData.tableData = [];
		state.tableColumns = [];
		if (ruleFormRef.value) {
			ruleFormRef.value.resetFields();
		}
		if (dataTableRef.value) {
			dataTableRef.value.clearSelection();
		}
	}, 300);
};

// 提交
const submit = async () => {
	if (!ruleFormRef.value) return;

	ruleFormRef.value.validate(async (isValid: boolean, fields?: any) => {
		if (isValid) {
			try {
				state.loading = true;
				// 赋值描述字段
				state.ruleForm.tableDes = state.dropdownData.tableList.find(m => m.value == state.ruleForm.tableName)?.label;
				state.ruleForm.fieldDes = state.dropdownData.columnList.find(m => m.value == state.ruleForm.fieldName)?.label;
				const values = { ...state.ruleForm };
				// 确保字段值是字符串格式
				if (Array.isArray(values.fieldValue)) {
					values.fieldValue = values.fieldValue.join(',');
				}

				await wmsPrmissionScopeApi[values.id ? 'update' : 'add'](values);
				ElMessage.success(state.title + '成功');
				closeDialog();
			} catch (error) {
				ElMessage.error(state.title + '失败');
				console.error('提交失败:', error);
			} finally {
				state.loading = false;
			}
		} else {
			ElMessage({
				message: `表单有${Object.keys(fields).length}处验证失败，请修改后再提交`,
				type: "error",
			});
		}
	});
};

// 表格行样式
const tableRowClassName = ({ row }: { row: any }) => {
	if (!state.ruleForm.fieldName) return '';

	const fieldValue = row[state.ruleForm.fieldName];
	if (state.selectedFieldValues.includes(String(fieldValue))) {
		return 'selected-row';
	}
	return '';
};

//将属性或者函数暴露给父组件
defineExpose({ openDialog });
</script>

<template>
	<div class="wmsPrmissionScope-container">
		<el-dialog v-model="state.showDialog" :width="1000" draggable :close-on-click-modal="false"
			:close-on-press-escape="false">
			<template #header>
				<div style="color: #fff">
					<span>{{ state.title }}</span>
				</div>
			</template>

			<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto" :rules="rules"
				:disabled="state.loading">
				<el-row :gutter="35">
					<el-form-item v-show="false">
						<el-input v-model="state.ruleForm.id" />
					</el-form-item>

					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="用户" prop="userId">
							<el-select clearable filterable v-model="state.ruleForm.userId" placeholder="请选择用户"
								:loading="state.loading">
								<el-option v-for="(item, index) in state.dropdownData.userId" :key="index"
									:value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>

					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="功能名称" prop="tableName">
							<el-select @change="getTableDetail" clearable filterable v-model="state.ruleForm.tableName"
								placeholder="请选择功能名称" :loading="state.loading">
								<el-option v-for="(item, index) in state.dropdownData.tableList" :key="index"
									:value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>

					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="字段名" prop="fieldName">
							<el-select @change="handleTableFieldChange" clearable filterable v-model="state.ruleForm.fieldName" placeholder="请选择字段名"
								:loading="state.tableLoading" :disabled="!state.ruleForm.tableName">
								<el-option v-for="(item, index) in state.dropdownData.columnList" :key="index"
									:value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>

					<!-- <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="字段值" prop="fieldValue">
							<el-select aria-readonly="true" clearable filterable multiple collapse-tags v-model="state.selectedFieldValues"
								@change="handleFieldValueChange" placeholder="请选择字段值" :loading="state.tableLoading"
								:disabled="!state.ruleForm.fieldName">
								<el-option v-for="(item, index) in state.dropdownData.fieldValueOptions" :key="index"
									:value="item.value" :label="item.label" />
							</el-select>
							<div class="field-value-tip">
								<span  v-if="state.selectedFieldValues.length > 0">已选择 {{ state.selectedFieldValues.length }} 个值: {{ state.selectedFieldValues.join(', ') }} </span>
							</div>
						</el-form-item>
					</el-col> -->
				</el-row>

				<!-- 数据表格 -->
				<el-row v-if="state.ruleForm.tableName && state.dropdownData.tableData.length > 0">
					<el-col :span="24" class="mb20">
						<div class="table-section">
							<div class="table-header">
								<h4>表数据预览 ({{ state.dropdownData.tableData.length }} 条记录)</h4>
								<div class="table-tips">
									<el-tag v-if="state.ruleForm.columnName" type="info">
										当前字段: {{ state.ruleForm.columnName }}
									</el-tag>
									<el-tag type="success" v-if="state.tableSelection.length > 0">
										已选中 {{ state.tableSelection.length }} 行
									</el-tag>
									<span class="tip-text">点击行或复选框选择数据，绿色行表示已选择</span>
								</div>
							</div>

							<div class="table-container">
								<el-table 
									ref="dataTableRef" 
									:data="state.dropdownData.tableData"
									:columns="state.tableColumns" 
									border 
									stripe 
									height="300"
									v-loading="state.tableLoading" 
									:row-class-name="tableRowClassName"
									highlight-current-row
									@selection-change="handleTableSelectionChange"
									@row-click="handleRowClick">
									<!-- 复选框列 -->
									<el-table-column type="selection" width="55" align="center" />
									<el-table-column 
										v-for="column in filteredColumns" 
										:key="column.prop"
										:prop="column.prop" 
										:label="column.label" 
										:width="column.width"
										show-overflow-tooltip>
										<template #default="scope">
											<span :class="{
												'selected-value': state.ruleForm.fieldName === column.prop &&
													state.selectedFieldValues.includes(String(scope.row[column.prop]))
											}">
												{{ scope.row[column.prop] }}
											</span>
										</template>
									</el-table-column>
								</el-table>
							</div>
						</div>
					</el-col>
				</el-row>

				<el-row v-else-if="state.ruleForm.tableName">
					<el-col :span="24">
						<el-empty description="暂无表数据" :image-size="80" />
					</el-col>
				</el-row>
			</el-form>

			<template #footer>
				<span class="dialog-footer">
					<el-button @click="state.showDialog = false" :disabled="state.loading">取 消</el-button>
					<el-button @click="submit" type="primary" :loading="state.loading" v-reclick="1000">
						{{ state.loading ? '提交中...' : '确 定' }}
					</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>

<style lang="scss" scoped>
.wmsPrmissionScope-container {

	:deep(.el-select),
	:deep(.el-input-number) {
		width: 100%;
	}

	.field-value-tip {
		font-size: 12px;
		color: #67c23a;
		margin-top: 5px;
		white-space: nowrap;
		overflow: hidden;
		text-overflow: ellipsis;
		height: 24px;
		min-width: 1px;
	}

	.table-section {
		margin-top: 20px;

		.table-header {
			display: flex;
			justify-content: space-between;
			align-items: center;
			margin-bottom: 15px;

			h4 {
				margin: 0;
				color: #303133;
			}

			.table-tips {
				display: flex;
				align-items: center;
				gap: 10px;

				.tip-text {
					font-size: 12px;
					color: #909399;
				}
			}
		}

		.table-container {
			border: 1px solid #ebeef5;
			border-radius: 4px;
		}
	}

	// 选中行的样式
	:deep(.selected-row) {
		background-color: #f0f9ff !important;

		&:hover>td {
			background-color: #e6f7ff !important;
		}
		
		// 选中行的复选框样式
		.el-checkbox__input.is-checked .el-checkbox__inner {
			background-color: #67c23a;
			border-color: #67c23a;
		}
	}

	// 选中值的样式
	.selected-value {
		font-weight: bold;
		color: #67c23a;
	}

	// 加载状态
	:deep(.el-loading-mask) {
		border-radius: 4px;
	}
	
	// 表格行点击区域样式
	:deep(.el-table__row) {
		cursor: pointer;
		
		&:hover {
			background-color: #f5f7fa;
		}
	}
}
</style>