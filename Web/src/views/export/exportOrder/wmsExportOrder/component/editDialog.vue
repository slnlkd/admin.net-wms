<script lang="ts" name="wmsExportOrder" setup>
import { ref, reactive, onMounted } from "vue";
import { ElMessage } from "element-plus";
import type { FormRules } from "element-plus";
import { formatDate } from '/@/utils/formatTime';
import { useWmsExportOrderApi } from '/@/api/export/exportOrder/wmsExportOrder';

//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const wmsExportOrderApi = useWmsExportOrderApi();
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
	row = row ?? { isDelete: false, };
	state.ruleForm = row.id ? await wmsExportOrderApi.detail(row.id).then(res => res.data.result) : JSON.parse(JSON.stringify(row));
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
			await wmsExportOrderApi[state.ruleForm.id ? 'update' : 'add'](values);
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
	<div class="wmsExportOrder-container">
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
						<el-form-item label="出库流水单据" prop="exportOrderNo">
							<el-input v-model="state.ruleForm.exportOrderNo" placeholder="请输入出库流水单据" maxlength="32" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="单据类型" prop="exportBillType">
							<el-input v-model="state.ruleForm.exportBillType" placeholder="请输入单据类型" maxlength="19" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料ID" prop="exportMaterialId">
							<el-input v-model="state.ruleForm.exportMaterialId" placeholder="请输入物料ID" maxlength="19" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料编码" prop="exportMaterialCode">
							<el-input v-model="state.ruleForm.exportMaterialCode" placeholder="请输入物料编码" maxlength="200" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料名称" prop="exportMaterialName">
							<el-input v-model="state.ruleForm.exportMaterialName" placeholder="请输入物料名称" maxlength="200" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料规格" prop="exportMaterialStandard">
							<el-input v-model="state.ruleForm.exportMaterialStandard" placeholder="请输入物料规格" maxlength="200" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料型号" prop="exportMaterialModel">
							<el-input v-model="state.ruleForm.exportMaterialModel" placeholder="请输入物料型号" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料类型" prop="exportMaterialType">
							<el-input v-model="state.ruleForm.exportMaterialType" placeholder="请输入物料类型" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="物料单位" prop="exportMaterialUnit">
							<el-input v-model="state.ruleForm.exportMaterialUnit" placeholder="请输入物料单位" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="仓库ID" prop="exportWarehouseId">
							<el-input v-model="state.ruleForm.exportWarehouseId" placeholder="请输入仓库ID" maxlength="19" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="区域ID" prop="exportAreaId">
							<el-input v-model="state.ruleForm.exportAreaId" placeholder="请输入区域ID" maxlength="19" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="储位编码" prop="exportSlotCode">
							<el-input v-model="state.ruleForm.exportSlotCode" placeholder="请输入储位编码" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="托盘条码" prop="exportStockCode">
							<el-input v-model="state.ruleForm.exportStockCode" placeholder="请输入托盘条码" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="出库数量" prop="exportQuantity">
							<el-input-number v-model="state.ruleForm.exportQuantity" placeholder="请输入出库数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="出库重量" prop="exportWeight">
							<el-input v-model="state.ruleForm.exportWeight" placeholder="请输入出库重量" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="生产日期" prop="exportProductionDate">
							<el-date-picker v-model="state.ruleForm.exportProductionDate" type="date" placeholder="生产日期" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="失效日期" prop="exportLoseDate">
							<el-date-picker v-model="state.ruleForm.exportLoseDate" type="date" placeholder="失效日期" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="部门编码" prop="exportDepartmentCode">
							<el-input v-model="state.ruleForm.exportDepartmentCode" placeholder="请输入部门编码" maxlength="19" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="供应商编码" prop="expotSupplierCode">
							<el-input v-model="state.ruleForm.expotSupplierCode" placeholder="请输入供应商编码" maxlength="19" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="客户编码" prop="exportCustomerCode">
							<el-input v-model="state.ruleForm.exportCustomerCode" placeholder="请输入客户编码" maxlength="19" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="任务号" prop="exportTaskNo">
							<el-input v-model="state.ruleForm.exportTaskNo" placeholder="请输入任务号" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="执行标志（0待执行、1正在执行、2已完成、3已上传）" prop="exportExecuteFlag">
							<el-input-number v-model="state.ruleForm.exportExecuteFlag" placeholder="请输入执行标志（0待执行、1正在执行、2已完成、3已上传）" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="单据创建时间" prop="exporOrederDate">
							<el-date-picker v-model="state.ruleForm.exporOrederDate" type="date" placeholder="单据创建时间" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="备注" prop="exportRemark">
							<el-input v-model="state.ruleForm.exportRemark" placeholder="请输入备注" maxlength="2147483647" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="批次" prop="exportLotNo">
							<el-input v-model="state.ruleForm.exportLotNo" placeholder="请输入批次" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="出库单据" prop="exportBillCode">
							<el-input v-model="state.ruleForm.exportBillCode" placeholder="请输入出库单据" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="出库序号" prop="exportListNo">
							<el-input v-model="state.ruleForm.exportListNo" placeholder="请输入出库序号" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="扫码数量" prop="scanQuantity">
							<el-input-number v-model="state.ruleForm.scanQuantity" placeholder="请输入扫码数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="扫码人" prop="scanUserNames">
							<el-input v-model="state.ruleForm.scanUserNames" placeholder="请输入扫码人" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="完成时间" prop="completeDate">
							<el-date-picker v-model="state.ruleForm.completeDate" type="date" placeholder="完成时间" />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="拣货数量" prop="pickedNum">
							<el-input-number v-model="state.ruleForm.pickedNum" placeholder="请输入拣货数量" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="出库单据明细ID" prop="exportDetailId">
							<el-input v-model="state.ruleForm.exportDetailId" placeholder="请输入出库单据明细ID" maxlength="19" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="箱数" prop="wholeBoxNum">
							<el-input v-model="state.ruleForm.wholeBoxNum" placeholder="请输入箱数" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="托盘类型" prop="outType">
							<el-input-number v-model="state.ruleForm.outType" placeholder="请输入托盘类型" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="托盘总箱数" prop="stockWholeBoxNum">
							<el-input v-model="state.ruleForm.stockWholeBoxNum" placeholder="请输入托盘总箱数" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="托盘总数量" prop="stockQuantity">
							<el-input v-model="state.ruleForm.stockQuantity" placeholder="请输入托盘总数量" maxlength="100" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="出库类型，0：正常出库，1：分拣出库，2：转运出库，3：托盘出库,4：备料出库" prop="exportType">
							<el-input-number v-model="state.ruleForm.exportType" placeholder="请输入出库类型，0：正常出库，1：分拣出库，2：转运出库，3：托盘出库,4：备料出库" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="质检状态" prop="inspectionStatus">
							<el-input-number v-model="state.ruleForm.inspectionStatus" placeholder="请输入质检状态" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="WholeOutWare" prop="wholeOutWare">
							<el-input v-model="state.ruleForm.wholeOutWare" placeholder="请输入WholeOutWare" maxlength="64" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="根据状态排序" prop="orderByStatus">
							<el-input-number v-model="state.ruleForm.orderByStatus" placeholder="请输入根据状态排序" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="储位编码" prop="exportStockSlotCode">
							<el-input v-model="state.ruleForm.exportStockSlotCode" placeholder="请输入储位编码" maxlength="32" show-word-limit clearable />
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