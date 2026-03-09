<script lang="ts" name="wmsBaseSlot" setup>
import { ref, reactive, onMounted } from "vue";
import { ElMessage } from "element-plus";
import type { FormRules } from "element-plus";
import { formatDate } from '/@/utils/formatTime';
import { useWmsBaseSlotApi } from '/@/api/base/baseSlot/wmsBaseSlot';

//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const wmsBaseSlotApi = useWmsBaseSlotApi();
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
  slotAreaId: [{required: true, message: '请选择所属区域！', trigger: 'blur',},],
  slotLanewayId: [{required: true, message: '请选择所属巷道！', trigger: 'blur',},],
});

// 页面加载时
onMounted(async () => {
  const data = await wmsBaseSlotApi.getDropdownData(false).then(res => res.data.result) ?? {};
  state.dropdownData.warehouseId = data.warehouseId ?? [];
  state.dropdownData.slotAreaId = data.slotAreaId ?? [];
  state.dropdownData.slotLanewayId = data.slotLanewayId ?? [];
});

// 打开弹窗
const openDialog = async (row: any, title: string) => {
	state.title = title;
	row = row ?? {  };
	state.ruleForm = row.id ? await wmsBaseSlotApi.detail(row.id).then(res => res.data.result) : JSON.parse(JSON.stringify(row));
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
			await wmsBaseSlotApi[state.ruleForm.id ? 'update' : 'add'](values);
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
	<div class="wmsBaseSlot-container">
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
						<el-form-item label="所属区域" prop="slotAreaId">
							<el-select clearable filterable v-model="state.ruleForm.slotAreaId" placeholder="请选择所属区域">
								<el-option v-for="(item,index) in state.dropdownData.slotAreaId" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="所属巷道" prop="slotLanewayId">
							<el-select clearable filterable v-model="state.ruleForm.slotLanewayId" placeholder="请选择所属巷道">
								<el-option v-for="(item,index) in state.dropdownData.slotLanewayId" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="排" prop="slotRow">
							<el-input-number v-model="state.ruleForm.slotRow" placeholder="请输入排" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="列" prop="slotColumn">
							<el-input-number v-model="state.ruleForm.slotColumn" placeholder="请输入列" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="层" prop="slotLayer">
							<el-input-number v-model="state.ruleForm.slotLayer" placeholder="请输入层" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="储位深度" prop="slotInout">
							<g-sys-dict v-model="state.ruleForm.slotInout" code="SlotInout" render-as="select" placeholder="请选择储位深度" clearable filterable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="储位编码" prop="slotCode">
							<el-input v-model="state.ruleForm.slotCode" placeholder="请输入储位编码" maxlength="20" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="储位状态" prop="slotStatus">
							<g-sys-dict v-model="state.ruleForm.slotStatus" code="SlotStatus" render-as="select" placeholder="请选择储位状态" clearable filterable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="库位类型" prop="make">
							<g-sys-dict v-model="state.ruleForm.make" code="Make" render-as="select" placeholder="请选择库位类型" clearable filterable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="储位属性" prop="property">
							<g-sys-dict v-model="state.ruleForm.property" code="Property" render-as="select" placeholder="请选择储位属性" clearable filterable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="储位处理" prop="handle">
							<g-sys-dict v-model="state.ruleForm.handle" code="Handle" render-as="select" placeholder="请选择储位处理" clearable filterable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="储位环境" prop="environment">
							<g-sys-dict v-model="state.ruleForm.environment" code="Environment" render-as="select" placeholder="请选择储位环境" clearable filterable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="入库锁定" prop="slotImlockFlag">
							<el-switch v-model="state.ruleForm.slotImlockFlag" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="出库锁定" prop="slotExlockFlag">
							<el-switch v-model="state.ruleForm.slotExlockFlag" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="是否屏蔽" prop="slotCloseFlag">
							<el-switch v-model="state.ruleForm.slotCloseFlag" />
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