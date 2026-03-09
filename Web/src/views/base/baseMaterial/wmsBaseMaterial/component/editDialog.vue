<script lang="ts" name="wmsBaseMaterial" setup>
import { ref, reactive, onMounted } from "vue";
import { ElMessage } from "element-plus";
import type { FormRules } from "element-plus";
import { formatDate } from '/@/utils/formatTime';
import { useWmsBaseMaterialApi } from '/@/api/base/baseMaterial/wmsBaseMaterial';

//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const wmsBaseMaterialApi = useWmsBaseMaterialApi();
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
  materialCode: [{required: true, message: '请选择物料编码！', trigger: 'blur',},],
  materialType: [{required: true, message: '请选择物料类型！', trigger: 'change',},],
  materialName: [{required: true, message: '请选择物料名称！', trigger: 'blur',},],
  boxQuantity: [{required: true, message: '请选择箱数量！', trigger: 'blur',},],
  everyNumber: [{required: true, message: '请选择每件托数！', trigger: 'blur',},],
  vehicle: [{required: true, message: '请选择载具！', trigger: 'change',},],
  warehouseId: [{required: true, message: '请选择所属仓库！', trigger: 'blur',},],
  labeling: [{required: true, message: '请选择贴标！', trigger: 'change',},],
  materialAreaId: [{required: true, message: '请选择专属区域！', trigger: 'change',},],
});

// 页面加载时
onMounted(async () => {
  const data = await wmsBaseMaterialApi.getDropdownData(false).then(res => res.data.result) ?? {};
  state.dropdownData.materialUnit = data.materialUnit ?? [];
  state.dropdownData.materialAreaId = data.materialAreaId ?? [];
  state.dropdownData.warehouseId = data.warehouseId ?? [];
});

// 打开弹窗
const openDialog = async (row: any, title: string) => {
	state.title = title;
	row = row ?? {  };
	state.ruleForm = row.id ? await wmsBaseMaterialApi.detail(row.id).then(res => res.data.result) : JSON.parse(JSON.stringify(row));
	if (typeof state.ruleForm.materialAreaId === 'string' && state.ruleForm.materialAreaId) {
		state.ruleForm.materialAreaId = state.ruleForm.materialAreaId.split(',').filter((x: string) => x);
	} else if (!Array.isArray(state.ruleForm.materialAreaId)) {
		state.ruleForm.materialAreaId = [];
	}
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
			if (Array.isArray(values.materialAreaId)) {
				values.materialAreaId = values.materialAreaId.join(',');
			}
			await wmsBaseMaterialApi[state.ruleForm.id ? 'update' : 'add'](values);
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
	<div class="wmsBaseMaterial-container">
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
						<el-form-item label="物料编码" prop="materialCode">
							<el-input v-model="state.ruleForm.materialCode" placeholder="请输入物料编码" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料类型" prop="materialType">
							<g-sys-dict v-model="state.ruleForm.materialType" code="MaterialType" render-as="select" placeholder="请选择物料类型" clearable filterable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料名称" prop="materialName">
							<el-input v-model="state.ruleForm.materialName" placeholder="请输入物料名称" maxlength="255" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料规格" prop="materialStandard">
							<el-input v-model="state.ruleForm.materialStandard" placeholder="请输入物料规格" maxlength="200" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="计量单位" prop="materialUnit">
							<el-select clearable filterable v-model="state.ruleForm.materialUnit" placeholder="请选择计量单位">
								<el-option v-for="(item,index) in state.dropdownData.materialUnit" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="专属区域" prop="materialAreaId">
							<el-select multiple clearable filterable v-model="state.ruleForm.materialAreaId" placeholder="请选择专属区域">
								<el-option v-for="(item,index) in state.dropdownData.materialAreaId" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="箱数量" prop="boxQuantity">
							<el-input-number v-model="state.ruleForm.boxQuantity" placeholder="请输入箱数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料来源" prop="materialOrigin">
							<el-input v-model="state.ruleForm.materialOrigin" placeholder="请输入物料来源" maxlength="10" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="备注" prop="remark">
							<el-input v-model="state.ruleForm.remark" placeholder="请输入备注" maxlength="255" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="每件托数" prop="everyNumber">
							<el-input-number v-model="state.ruleForm.everyNumber" placeholder="请输入每件托数" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="载具" prop="vehicle">
							<g-sys-dict v-model="state.ruleForm.vehicle" code="Vehicle" render-as="select" placeholder="请选择载具" clearable filterable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料重量" prop="materialWeight">
							<el-input-number v-model="state.ruleForm.materialWeight" placeholder="请输入物料重量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="最高库存数量" prop="materialStockHigh">
							<el-input-number v-model="state.ruleForm.materialStockHigh" placeholder="请输入最高库存数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="最低库存数量" prop="materialStockLow">
							<el-input-number v-model="state.ruleForm.materialStockLow" placeholder="请输入最低库存数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="所属仓库" prop="warehouseId">
							<el-select clearable filterable v-model="state.ruleForm.warehouseId" placeholder="请选择所属仓库">
								<el-option v-for="(item,index) in state.dropdownData.warehouseId" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="提前预警天数" prop="materialAlarmDay">
							<el-input-number v-model="state.ruleForm.materialAlarmDay" placeholder="请输入提前预警天数" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="贴标" prop="labeling">
							<g-sys-dict v-model="state.ruleForm.labeling" code="Labeling" render-as="select" placeholder="请选择贴标" clearable filterable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="管理方式" prop="manageType">
							<g-sys-dict v-model="state.ruleForm.manageType" code="ManageType" render-as="select" placeholder="请选择管理方式" clearable filterable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="最低储备" prop="mixReserve">
							<el-input-number v-model="state.ruleForm.mixReserve" placeholder="请输入最低储备" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="最高储备" prop="maxReserve">
							<el-input-number v-model="state.ruleForm.maxReserve" placeholder="请输入最高储备" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="提前报警天数" prop="alarmDay">
							<el-input-number v-model="state.ruleForm.alarmDay" placeholder="请输入提前报警天数" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="是否空托" prop="isEmpty">
							<el-switch v-model="state.ruleForm.isEmpty" />
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