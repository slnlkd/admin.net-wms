<script lang="ts" name="wmsBaseBillType" setup>
import { ref, reactive, onMounted, watch } from "vue";
import { ElMessage } from "element-plus";
// ... (omitting some imports for brevity if they match)
import type { FormRules } from "element-plus";
import { formatDate } from '/@/utils/formatTime';
import { useWmsBaseBillTypeApi } from '/@/api/base/billType/wmsBaseBillType';

//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const wmsBaseBillTypeApi = useWmsBaseBillTypeApi();
const ruleFormRef = ref();

const state = reactive({
	title: '',
	loading: false,
	showDialog: false,
	ruleForm: {} as any,
	stores: {},
	dropdownData: {} as any,
});

// 自行添加其他规则
const rules = ref<FormRules>({
  billTypeCode: [{required: true, message: '请选择单据类型编码！', trigger: 'blur',},],
  billTypeName: [{required: true, message: '请选择单据类型名称！', trigger: 'blur',},],
  billType: [{required: true, message: '请选择类型！', trigger: 'change',},],
  qualityInspectionStatus: [{required: true, message: '请选择质检状态！', trigger: 'change',},],
  proxyStatus: [{required: true, message: '请选择代储状态！', trigger: 'change',},],
  wareHouseId: [{required: true, message: '请选择所属仓库！', trigger: 'blur',},],
});

// 监听类型变化
watch(() => state.ruleForm.billType, (val) => {
	if (val == 1) { // 入库 - 单选
		if (Array.isArray(state.ruleForm.qualityInspectionStatus)) {
			state.ruleForm.qualityInspectionStatus = state.ruleForm.qualityInspectionStatus.length > 0 ? state.ruleForm.qualityInspectionStatus[0] : '';
		}
		if (state.ruleForm.qualityInspectionStatus !== '' && state.ruleForm.qualityInspectionStatus !== null && !isNaN(Number(state.ruleForm.qualityInspectionStatus))) {
			state.ruleForm.qualityInspectionStatus = Number(state.ruleForm.qualityInspectionStatus);
		}
	} else if (val == 2) { // 出库 - 多选
		if (state.ruleForm.qualityInspectionStatus && !Array.isArray(state.ruleForm.qualityInspectionStatus)) {
			state.ruleForm.qualityInspectionStatus = state.ruleForm.qualityInspectionStatus.toString().split(',').map(x => !isNaN(Number(x)) ? Number(x) : x);
		} else if (!state.ruleForm.qualityInspectionStatus) {
			state.ruleForm.qualityInspectionStatus = [];
		}
	}
});

// 页面加载时
onMounted(async () => {
  const data = await wmsBaseBillTypeApi.getDropdownData(false).then(res => res.data.result) ?? {};
  state.dropdownData.wareHouseId = data.wareHouseId ?? [];
});

// 打开弹窗
const openDialog = async (row: any, title: string) => {
	state.title = title;
	row = row ?? {  };
	let detail = row.id ? await wmsBaseBillTypeApi.detail(row.id).then(res => res.data.result) : JSON.parse(JSON.stringify(row));
	
	// 处理质检状态数据格式
	if (detail.qualityInspectionStatus && typeof detail.qualityInspectionStatus === 'string') {
		if (detail.billType == 2) {
			detail.qualityInspectionStatus = detail.qualityInspectionStatus.split(',').map(x => !isNaN(Number(x)) ? Number(x) : x);
		} else {
			detail.qualityInspectionStatus = !isNaN(Number(detail.qualityInspectionStatus)) ? Number(detail.qualityInspectionStatus) : detail.qualityInspectionStatus;
		}
	} else if (detail.billType == 2 && !detail.qualityInspectionStatus) {
		detail.qualityInspectionStatus = [];
	}
	
	state.ruleForm = detail;
	state.showDialog = true;
};

// 关闭弹窗
const closeDialog = () => {
	emit("reloadTable");
	state.showDialog = false;
};

// 提交
const submit = async () => {
	ruleFormRef.value.validate(async (isValid: boolean, fields?: any) => {
		if (isValid) {
			let values = JSON.parse(JSON.stringify(state.ruleForm));
			// 提交前处理质检状态，如果是多选则转为逗号分隔字符串
			if (values.billType == 2 && Array.isArray(values.qualityInspectionStatus)) {
				values.qualityInspectionStatus = values.qualityInspectionStatus.join(',');
			}
			await wmsBaseBillTypeApi[state.ruleForm.id ? 'update' : 'add'](values);
			closeDialog();
		} else {
			ElMessage({
				message: `表单有${Object.keys(fields).length}处验证失败，请修改后再提交`,
				type: "error",
			});
		}
	});
};

//将属性或者函数暴露给父组件
defineExpose({ openDialog });
</script>
<template>
	<div class="wmsBaseBillType-container">
		<el-dialog v-model="state.showDialog" :width="800" draggable :close-on-click-modal="false">
			<template #header>
				<div style="color: #fff">
					<span>{{ state.title }}</span>
				</div>
			</template>
			<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto" :rules="rules">
				<el-row :gutter="35">
					<el-form-item v-show="false">
						<el-input v-model="state.ruleForm.id" />
					</el-form-item>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="单据类型编码" prop="billTypeCode">
							<el-input v-model="state.ruleForm.billTypeCode" placeholder="请输入单据类型编码" maxlength="64" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="单据类型名称" prop="billTypeName">
							<el-input v-model="state.ruleForm.billTypeName" placeholder="请输入单据类型名称" maxlength="64" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="备注" prop="remark">
							<el-input v-model="state.ruleForm.remark" placeholder="请输入备注" maxlength="255" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="类型" prop="billType">
							<g-sys-dict v-model="state.ruleForm.billType" code="BillType" render-as="select" placeholder="请选择类型" clearable filterable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="质检状态" prop="qualityInspectionStatus">
							<g-sys-dict v-model="state.ruleForm.qualityInspectionStatus" code="QualityInspectionStatus" render-as="select" placeholder="请选择质检状态" clearable filterable :multiple="state.ruleForm.billType == 2" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="代储状态" prop="proxyStatus">
							<g-sys-dict v-model="state.ruleForm.proxyStatus" code="ProxyStatus" render-as="select" placeholder="请选择代储状态" clearable filterable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="所属仓库" prop="wareHouseId">
							<el-select clearable filterable v-model="state.ruleForm.wareHouseId" placeholder="请选择所属仓库">
								<el-option v-for="(item,index) in state.dropdownData.wareHouseId" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
				</el-row>
			</el-form>
			<template #footer>
				<span class="dialog-footer">
					<el-button @click="() => state.showDialog = false">取 消</el-button>
					<el-button @click="submit" type="primary" v-reclick="1000">确 定</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>
<style lang="scss" scoped>
:deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>