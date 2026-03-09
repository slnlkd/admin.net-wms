<script lang="ts" name="wmsImportNotifyDetail" setup>
import { ref, reactive, onMounted } from "vue";
import { ElMessage } from "element-plus";
import type { FormRules } from "element-plus";
import { formatDate } from '/@/utils/formatTime';
import { useWmsImportNotifyDetailApi } from '/@/api/import/importBill/wmsImportNotifyDetail';

//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const wmsImportNotifyDetailApi = useWmsImportNotifyDetailApi();
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
});

// 页面加载时
onMounted(async () => {
  const data = await wmsImportNotifyDetailApi.getDropdownData(false).then(res => res.data.result) ?? {};
  state.dropdownData.materialId = data.materialId ?? [];
});

// 打开弹窗
const openDialog = async (row: any, title: string) => {
	state.title = title;
	row = row ?? { xj_Type: ('1'), };
	state.ruleForm = row.id ? await wmsImportNotifyDetailApi.detail(row.id).then(res => res.data.result) : JSON.parse(JSON.stringify(row));
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
			await wmsImportNotifyDetailApi[state.ruleForm.id ? 'update' : 'add'](values);
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
	<div class="wmsImportNotifyDetail-container">
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
						<el-form-item label="入库单据ID" prop="importId">
							<el-input v-model="state.ruleForm.importId" placeholder="请输入入库单据ID" maxlength="19" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="外部单据明细ID" prop="outerDetailId">
							<el-input v-model="state.ruleForm.outerDetailId" placeholder="请输入外部单据明细ID" maxlength="32" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="序号" prop="importListNo">
							<el-input v-model="state.ruleForm.importListNo" placeholder="请输入序号" maxlength="32" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料信息" prop="materialId">
							<el-select clearable filterable v-model="state.ruleForm.materialId" placeholder="请选择物料信息">
								<el-option v-for="(item,index) in state.dropdownData.materialId" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="批次" prop="lotNo">
							<el-input v-model="state.ruleForm.lotNo" placeholder="请输入批次" maxlength="32" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料状态" prop="materialStatus">
							<el-input v-model="state.ruleForm.materialStatus" placeholder="请输入物料状态" maxlength="32" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="生产日期" prop="importProductionDate">
							<el-date-picker v-model="state.ruleForm.importProductionDate" type="date" placeholder="生产日期" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="失效日期" prop="importLostDate">
							<el-date-picker v-model="state.ruleForm.importLostDate" type="date" placeholder="失效日期" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="计划数量" prop="importQuantity">
							<el-input-number v-model="state.ruleForm.importQuantity" placeholder="请输入计划数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="完成数量" prop="importCompleteQuantity">
							<el-input-number v-model="state.ruleForm.importCompleteQuantity" placeholder="请输入完成数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="组盘数量" prop="importFactQuantity">
							<el-input-number v-model="state.ruleForm.importFactQuantity" placeholder="请输入组盘数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="已上传数量" prop="uploadQuantity">
							<el-input-number v-model="state.ruleForm.uploadQuantity" placeholder="请输入已上传数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="执行状态" prop="importExecuteFlag">
							<el-input v-model="state.ruleForm.importExecuteFlag" placeholder="请输入执行状态" maxlength="2" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="备注" prop="remark">
							<el-input v-model="state.ruleForm.remark" placeholder="请输入备注" maxlength="50" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="打印标识，1打印，0不打印" prop="printFlag">
							<el-input-number v-model="state.ruleForm.printFlag" placeholder="请输入打印标识，1打印，0不打印" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="下发任务状态默认0，1下发成功" prop="taskStatus">
							<el-input-number v-model="state.ruleForm.taskStatus" placeholder="请输入下发任务状态默认0，1下发成功" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="标签判断，1已打印，2或空未打印" prop="labelJudgment">
							<el-input-number v-model="state.ruleForm.labelJudgment" placeholder="请输入标签判断，1已打印，2或空未打印" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="件数" prop="outQty">
							<el-input-number v-model="state.ruleForm.outQty" placeholder="请输入件数" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="巷道编码" prop="lanewayCode">
							<el-input v-model="state.ruleForm.lanewayCode" placeholder="请输入巷道编码" maxlength="32" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="采浆公司" prop="xj_HouseCode">
							<el-input v-model="state.ruleForm.xj_HouseCode" placeholder="请输入采浆公司" maxlength="50" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="血浆件数" prop="xj_Qty">
							<el-input-number v-model="state.ruleForm.xj_Qty" placeholder="请输入血浆件数" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="血浆重量" prop="xj_Weight">
							<el-input-number v-model="state.ruleForm.xj_Weight" placeholder="请输入血浆重量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="血浆 箱总数" prop="xj_BoxQty">
							<el-input-number v-model="state.ruleForm.xj_BoxQty" placeholder="请输入血浆 箱总数" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="血浆类型" prop="xj_GoodCode">
							<el-input v-model="state.ruleForm.xj_GoodCode" placeholder="请输入血浆类型" maxlength="19" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="单据类型（1:入库；2：取消入库）" prop="xj_Type">
							<el-input-number v-model="state.ruleForm.xj_Type" placeholder="请输入单据类型（1:入库；2：取消入库）" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="已验证数量" prop="verifiedQty">
							<el-input-number v-model="state.ruleForm.verifiedQty" placeholder="请输入已验证数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="千克数量" prop="kilogramQty">
							<el-input-number v-model="state.ruleForm.kilogramQty" placeholder="请输入千克数量" clearable />
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