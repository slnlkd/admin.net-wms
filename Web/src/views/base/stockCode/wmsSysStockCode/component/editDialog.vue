<script lang="ts" name="wmsSysStockCode" setup>
import { ref, reactive, onMounted } from "vue";
import { ElMessage } from "element-plus";
import type { FormRules } from "element-plus";
import { formatDate } from '/@/utils/formatTime';
import { useWmsSysStockCodeApi } from '/@/api/base/stockCode/wmsSysStockCode';
import { stat } from "fs";

//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const wmsSysStockCodeApi = useWmsSysStockCodeApi();
const ruleFormRef = ref();

const handleStockTypeChange = (value: any) => {
  // 在这里处理 stockType 变化后的逻辑
  console.log('Selected stockType:', value);
  
  wmsSysStockCodeApi.getSysStockCode(
	{"warehouseId":state.ruleForm.warehouseId,"stockType":value}
  ).then(res => {
	if (res.data && res.data.result) {
	  state.ruleForm.stockCode = res.data.result.data;
	} else {
	  ElMessage({
		message: '未能获取到载具号，请手动输入',
		type: 'warning',
	  });
	  state.ruleForm.stockCode = '';
	}
  }).catch(() => {
	ElMessage({
	  message: '获取载具号失败，请重新获取',
	  type: 'error',
	});
	state.ruleForm.stockCode = '';
  });
};

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
  stockCode: [{required: true, message: '请选择载具号！', trigger: 'blur',},],
  warehouseId: [{required: true, message: '请选择所属仓库！', trigger: 'blur',},],
  stockType: [{required: true, message: '请选择托盘类型！', trigger: 'change',},],
});

// 页面加载时
onMounted(async () => {
  const data = await wmsSysStockCodeApi.getDropdownData(false).then(res => res.data.result) ?? {};
  state.dropdownData.warehouseId = data.warehouseId ?? [];
});

// 打开弹窗
const openDialog = async (row: any, title: string) => {
	state.title = title;
	row = row ?? {  };
	state.ruleForm = row.id ? await wmsSysStockCodeApi.detail(row.id).then(res => res.data.result) : JSON.parse(JSON.stringify(row));
	state.ruleForm.warehouseId = state.dropdownData.warehouseId.length > 0 ? state.dropdownData.warehouseId[0].value : null;
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
			await wmsSysStockCodeApi[state.ruleForm.id ? 'update' : 'add'](values);
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
	<div class="wmsSysStockCode-container">
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
							<el-select filterable v-model="state.ruleForm.warehouseId" placeholder="请选择所属仓库">
								<el-option v-for="(item,index) in state.dropdownData.warehouseId" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="托盘类型" prop="stockType">
							<g-sys-dict v-model="state.ruleForm.stockType" code="StockType" render-as="select" placeholder="请选择托盘类型" clearable filterable @change="handleStockTypeChange" />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="载具号" prop="stockCode">
							<el-input disabled="true" v-model="state.ruleForm.stockCode" placeholder="请输入载具号" maxlength="50" show-word-limit clearable />
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