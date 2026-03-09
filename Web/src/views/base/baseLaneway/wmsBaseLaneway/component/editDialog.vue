<script lang="ts" name="wmsBaseLaneway" setup>
import { ref, reactive, onMounted } from "vue";
import { ElMessage } from "element-plus";
import type { FormRules } from "element-plus";
import { formatDate } from '/@/utils/formatTime';
import { useWmsBaseLanewayApi } from '/@/api/base/baseLaneway/wmsBaseLaneway';

//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const wmsBaseLanewayApi = useWmsBaseLanewayApi();
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
  warehouseId: [{required: true, message: '请选择所属仓库！', trigger: 'blur',},],
  lanewayCode: [{required: true, message: '请选择巷道编码！', trigger: 'blur',},],
  lanewayName: [{required: true, message: '请选择巷道名称！', trigger: 'blur',},],
});

// 页面加载时
onMounted(async () => {
  const data = await wmsBaseLanewayApi.getDropdownData(false).then(res => res.data.result) ?? {};
  state.dropdownData.warehouseId = data.warehouseId ?? [];
});

// 打开弹窗
const openDialog = async (row: any, title: string) => {
	state.title = title;
	row = row ?? {  };
	state.ruleForm = row.id ? await wmsBaseLanewayApi.detail(row.id).then(res => res.data.result) : JSON.parse(JSON.stringify(row));
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
			let values = state.ruleForm;
			await wmsBaseLanewayApi[state.ruleForm.id ? 'update' : 'add'](values);
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
	<div class="wmsBaseLaneway-container">
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
						<el-form-item label="所属仓库" prop="warehouseId">
							<el-select clearable filterable v-model="state.ruleForm.warehouseId" placeholder="请选择所属仓库">
								<el-option v-for="(item,index) in state.dropdownData.warehouseId" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="巷道编码" prop="lanewayCode">
							<el-input v-model="state.ruleForm.lanewayCode" placeholder="请输入巷道编码" maxlength="64" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="巷道名称" prop="lanewayName">
							<el-input v-model="state.ruleForm.lanewayName" placeholder="请输入巷道名称" maxlength="64" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="巷道状态" prop="lanewayStatus">
							<el-switch v-model="state.ruleForm.lanewayStatus" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="备注" prop="remark">
							<el-input v-model="state.ruleForm.remark" placeholder="请输入备注" maxlength="64" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="优先级" prop="lanewayPriority">
							<el-input-number v-model="state.ruleForm.lanewayPriority" placeholder="请输入优先级" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="储存条件" prop="lanewayTemp">
							<g-sys-dict v-model="state.ruleForm.lanewayTemp" code="LanewayTemp" render-as="select" placeholder="请选择储存条件" clearable filterable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="巷道口状态" prop="lanewayType">
							<g-sys-dict v-model="state.ruleForm.lanewayType" code="LanewayType" render-as="select" placeholder="请选择巷道口状态" clearable filterable />
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