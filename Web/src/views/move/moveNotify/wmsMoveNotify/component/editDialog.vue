<script lang="ts" name="wmsMoveNotify" setup>
import { ref, reactive, onMounted } from "vue";
import { ElMessage } from "element-plus";
import type { FormRules } from "element-plus";
import { formatDate } from '/@/utils/formatTime';
import { useWmsMoveNotifyApi } from '/@/api/move/moveNotify/wmsMoveNotify';

//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const wmsMoveNotifyApi = useWmsMoveNotifyApi();
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
	state.ruleForm = row.id ? await wmsMoveNotifyApi.detail(row.id).then(res => res.data.result) : JSON.parse(JSON.stringify(row));
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
			await wmsMoveNotifyApi[state.ruleForm.id ? 'update' : 'add'](values);
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
	<div class="wmsMoveNotify-container">
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
						<el-form-item label="移库单据类型" prop="moveBillCode">
							<el-input v-model="state.ruleForm.moveBillCode" placeholder="请输入移库单据类型" maxlength="50" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="移库序号" prop="moveListNo">
							<el-input-number v-model="state.ruleForm.moveListNo" placeholder="请输入移库序号" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="移库批次" prop="moveLotNo">
							<el-input v-model="state.ruleForm.moveLotNo" placeholder="请输入移库批次" maxlength="50" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料id" prop="materialId">
							<el-input v-model="state.ruleForm.materialId" placeholder="请输入物料id" maxlength="50" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="移库物料型号" prop="moveMaterialModel">
							<el-input v-model="state.ruleForm.moveMaterialModel" placeholder="请输入移库物料型号" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料温度" prop="moveMaterialTemp">
							<el-input v-model="state.ruleForm.moveMaterialTemp" placeholder="请输入物料温度" maxlength="50" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料类型（字典MaterialType）" prop="moveMaterialType">
							<el-input v-model="state.ruleForm.moveMaterialType" placeholder="请输入物料类型（字典MaterialType）" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料状态" prop="moveMaterialStatus">
							<el-input v-model="state.ruleForm.moveMaterialStatus" placeholder="请输入物料状态" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料单位" prop="moveMaterialUnit">
							<el-input v-model="state.ruleForm.moveMaterialUnit" placeholder="请输入物料单位" maxlength="50" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料品牌" prop="moveMaterialBrand">
							<el-input v-model="state.ruleForm.moveMaterialBrand" placeholder="请输入物料品牌" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="生产日期" prop="moveProductionDate">
							<el-date-picker v-model="state.ruleForm.moveProductionDate" type="date" placeholder="生产日期" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="失效日期" prop="moveLostDate">
							<el-date-picker v-model="state.ruleForm.moveLostDate" type="date" placeholder="失效日期" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="移库数量" prop="moveQuantity">
							<el-input-number v-model="state.ruleForm.moveQuantity" placeholder="请输入移库数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="移库日期" prop="moveDate">
							<el-date-picker v-model="state.ruleForm.moveDate" type="date" placeholder="移库日期" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="仓库id" prop="moveWarehouseId">
							<el-input v-model="state.ruleForm.moveWarehouseId" placeholder="请输入仓库id" maxlength="19" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="移出巷道id" prop="moveLanewayOutCode">
							<el-input v-model="state.ruleForm.moveLanewayOutCode" placeholder="请输入移出巷道id" maxlength="19" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="移入巷道id" prop="moveLanewayInCode">
							<el-input v-model="state.ruleForm.moveLanewayInCode" placeholder="请输入移入巷道id" maxlength="50" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="移出储位编码" prop="moveOutSlotCode">
							<el-input v-model="state.ruleForm.moveOutSlotCode" placeholder="请输入移出储位编码" maxlength="50" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="移入储位编码" prop="moveInSlotCode">
							<el-input v-model="state.ruleForm.moveInSlotCode" placeholder="请输入移入储位编码" maxlength="50" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="执行标志（01待执行、02正在执行、03已完成、04已上传）" prop="moveExecuteFlag">
							<el-input v-model="state.ruleForm.moveExecuteFlag" placeholder="请输入执行标志（01待执行、02正在执行、03已完成、04已上传）" maxlength="50" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="备注" prop="moveRemark">
							<el-input v-model="state.ruleForm.moveRemark" placeholder="请输入备注" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="移库任务号" prop="moveTaskNo">
							<el-input v-model="state.ruleForm.moveTaskNo" placeholder="请输入移库任务号" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="库存表id" prop="stockStockCodeId">
							<el-input v-model="state.ruleForm.stockStockCodeId" placeholder="请输入库存表id" maxlength="19" show-word-limit clearable />
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