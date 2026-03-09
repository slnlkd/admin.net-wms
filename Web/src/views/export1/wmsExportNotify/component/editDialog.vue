<script lang="ts" name="wmsExportNotify" setup>
import { ref, reactive, onMounted } from "vue";
import { ElMessage } from "element-plus";
import type { FormRules } from "element-plus";
//import { formatDate } from '/@/utils/formatTime';
import { useWmsExportNotifyApi } from '/@/api/export/wmsExportNotify';

//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const wmsExportNotifyApi = useWmsExportNotifyApi();
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
});

// 打开弹窗
const openDialog = async (row: any, title: string) => {
	state.title = title;
	row = row ?? {  };
	state.ruleForm = row.id ? await wmsExportNotifyApi.detail(row.id).then(res => res.data.result) : JSON.parse(JSON.stringify(row));
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
			await wmsExportNotifyApi[state.ruleForm.id ? 'update' : 'add'](values);
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
	<div class="wmsExportNotify-container">
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
						<el-form-item label="出库单据" prop="exportBillCode">
							<el-input v-model="state.ruleForm.exportBillCode" placeholder="请输入出库单据" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="单据类型" prop="exportBillType">
							<el-input v-model="state.ruleForm.exportBillType" placeholder="请输入单据类型" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="出库批次" prop="exportLotNo">
							<el-input v-model="state.ruleForm.exportLotNo" placeholder="请输入出库批次" maxlength="200" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料ID" prop="materialId">
							<el-input v-model="state.ruleForm.materialId" placeholder="请输入物料ID" maxlength="200" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="仓库ID" prop="warehouseId">
							<el-input v-model="state.ruleForm.warehouseId" placeholder="请输入仓库ID" maxlength="200" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="出库序号" prop="exportListNo">
							<el-input v-model="state.ruleForm.exportListNo" placeholder="请输入出库序号" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="部门ID" prop="exportDepartmentId">
							<el-input v-model="state.ruleForm.exportDepartmentId" placeholder="请输入部门ID" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="供应商ID" prop="exportSupplierId">
							<el-input v-model="state.ruleForm.exportSupplierId" placeholder="请输入供应商ID" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="客户ID" prop="exportCustomerId">
							<el-input v-model="state.ruleForm.exportCustomerId" placeholder="请输入客户ID" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="生产日期" prop="exportProductionDate">
							<el-date-picker v-model="state.ruleForm.exportProductionDate" type="date" placeholder="生产日期" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="失效日期" prop="exportLostDate">
							<el-date-picker v-model="state.ruleForm.exportLostDate" type="date" placeholder="失效日期" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="计划出库数量" prop="exportQuantity">
							<el-input-number v-model="state.ruleForm.exportQuantity" placeholder="请输入计划出库数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="任务下发数量" prop="exportFactQuantity">
							<el-input-number v-model="state.ruleForm.exportFactQuantity" placeholder="请输入任务下发数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="任务完成数量" prop="exportCompleteQuantity">
							<el-input-number v-model="state.ruleForm.exportCompleteQuantity" placeholder="请输入任务完成数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="已上传数量" prop="exportUploadQuantity">
							<el-input-number v-model="state.ruleForm.exportUploadQuantity" placeholder="请输入已上传数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="建单时间" prop="exportDate">
							<el-date-picker v-model="state.ruleForm.exportDate" type="date" placeholder="建单时间" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="执行标志" prop="exportExecuteFlag">
							<el-input-number v-model="state.ruleForm.exportExecuteFlag" placeholder="请输入执行标志" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="备注" prop="exportRemark">
							<el-input v-model="state.ruleForm.exportRemark" placeholder="请输入备注" maxlength="2147483647" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="外部单据编码" prop="outerBillCode">
							<el-input v-model="state.ruleForm.outerBillCode" placeholder="请输入外部单据编码" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="外部单据ID" prop="outerMainId">
							<el-input v-model="state.ruleForm.outerMainId" placeholder="请输入外部单据ID" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="来源（wms sap）" prop="source">
							<el-input v-model="state.ruleForm.source" placeholder="请输入来源（wms sap）" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="拣货区" prop="pickingArea">
							<el-input v-model="state.ruleForm.pickingArea" placeholder="请输入拣货区" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="拼箱状态（0：不拼箱1：拼箱）" prop="pXStatus">
							<el-input-number v-model="state.ruleForm.pXStatus" placeholder="请输入拼箱状态（0：不拼箱1：拼箱）" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="整托出库口" prop="wholeOutWare">
							<el-input v-model="state.ruleForm.wholeOutWare" placeholder="请输入整托出库口" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="分拣出库口" prop="sortOutWare">
							<el-input v-model="state.ruleForm.sortOutWare" placeholder="请输入分拣出库口" maxlength="100" show-word-limit clearable />
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