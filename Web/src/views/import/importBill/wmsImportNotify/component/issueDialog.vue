<template>
    <el-dialog v-model="dialogVisible" title="入库单据下发" width="600px" :close-on-click-modal="false" append-to-body>
        <el-card class="full-table" shadow="hover" style="margin-top: 5px">
            <el-form :model="state" ref="formRef" label-width="100px" :rules="formRules">
                <el-row :gutter="35">
                    <el-form-item v-show="false">
                        <el-input v-model="state.ruleForm.id" />
                    </el-form-item>
                    <el-col xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
                        <el-form-item label="入库口" prop="ruleForm.ReceivingDock">
                            <g-sys-dict v-model="state.ruleForm.ReceivingDock" code="ReceivingDock" render-as="select"
                                placeholder="请选择入库口" clearable filterable />
                        </el-form-item>
                    </el-col>
                    <el-col xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
                        <el-form-item label="皮重" prop="ruleForm.NetWeight">
                            <el-input v-model="state.ruleForm.NetWeight" placeholder="请输入皮重"
                                @input="handleQuantityInput(state.ruleForm, $event)"
                                @blur="validateQuantity(state.ruleForm)" />
                        </el-form-item>
                    </el-col>

                    <el-col xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
                        <el-form-item label=" ">
                            <el-button type="primary" icon="ele-Check" @click="handleSubmit"
                                :loading="submitLoading">确定下发</el-button>
                        </el-form-item>
                    </el-col>
                </el-row>
            </el-form>
        </el-card>

    </el-dialog>
</template>

<script lang="ts" setup>
import { ref, reactive, onMounted } from "vue";
import { ElMessage, ElMessageBox } from 'element-plus'
import { type FormInstance, type FormRules } from 'element-plus'
import { usewmsPortApi } from '/@/api/port/wmsPort';
const wmsPortApi = usewmsPortApi();
//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
// 弹窗显示控制
const dialogVisible = ref(false)
const submitLoading = ref(false)
// 表单数据
const formRef = ref<FormInstance>()
const state = reactive({
    ruleForm: {} as any,
    exportLoading: false,
    tableLoading: false,
    stores: {},
    showAdvanceQueryUI: false,
    dropdownData: {},
    selectData: [],
    tableQueryParams: {} as any,
    tableParams: {
        page: 1,
        pageSize: 20,
        total: 0,
        field: 'createTime', // 默认的排序字段
        order: 'descending', // 排序方向
        descStr: 'descending', // 降序排序的关键字符
    },
    tableData: [],
});

// 页面加载时
onMounted(async () => {
});

// 查询操作
const handleQuery = async (params = {}) => {
    // state.tableLoading = true;
    // state.tableParams = Object.assign(state.tableParams, params);
    // const result = await wmsImportLabelPrintApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
    // state.tableParams.total = result?.total;
    // state.tableData = result?.items ?? [];
    // state.tableLoading = false;
};
// 表单验证规则 
const formRules: FormRules = {
    'ruleForm.ReceivingDock': [
        { required: true, message: '请输入入库口', trigger: 'change' }
    ],
    'ruleForm.NetWeight': [
        { required: true, message: '请输入皮重', trigger: 'change' }
    ]
}
// 提交表单
const handleSubmit = async () => {
    if (!formRef.value) return
    try {
        const valid = await formRef.value.validate()
        if (!valid) return
        submitLoading.value = true
        ElMessageBox.confirm(`确定要下发吗?`, "提示", {
            confirmButtonText: "确定",
            cancelButtonText: "取消",
            type: "warning",
        }).then(async () => {
            await wmsPortApi.createOrder({ id: state.tableQueryParams.importDetailId, ReceivingDock: state.ruleForm.ReceivingDock, NetWeight: state.ruleForm.NetWeight }).then(async res => {
            });
            handleQuery();
            ElMessage.success("下发成功");
            dialogVisible.value = false
            emit('reloadTable')
        }).catch(() => { });
    } catch (error) {
        console.error('提交失败:', error)
    } finally {
        submitLoading.value = false
    }
}
// 数量输入处理 - 允许最多3位小数
const handleQuantityInput = (data: any, value: string) => {
    // 允许数字和小数点，限制小数位数
    let formattedValue = value.replace(/[^\d.]/g, '')

    // 处理多个小数点的情况，只保留第一个
    const dotCount = formattedValue.split('.').length - 1
    if (dotCount > 1) {
        const parts = formattedValue.split('.')
        formattedValue = parts[0] + '.' + parts.slice(1).join('')
    }

    // 限制小数位数为3位
    if (formattedValue.includes('.')) {
        const parts = formattedValue.split('.')
        if (parts[1].length > 3) {
            formattedValue = parts[0] + '.' + parts[1].substring(0, 3)
        }
    }

    data.NetWeight = formattedValue
}

// 验证数量 - 修改为允许小数
const validateQuantity = (data: any) => {
    if (data.NetWeight) {
        const numValue = parseFloat(data.NetWeight)
        if (isNaN(numValue) || numValue <= 0) {
            ElMessage.warning('数量必须大于0')
            data.NetWeight = ''
        } else {
            // 格式化显示，保留最多3位小数
            data.NetWeight = numValue.toString()
        }
    }
}
// 打开弹窗
const openDialog = async (row: any = null) => {
    state.ruleForm = {

    }
    // 重置表单
    if (formRef.value) {
        formRef.value.resetFields()
    }
    state.tableQueryParams.importDetailId = row.id;
    await handleQuery();
    dialogVisible.value = true
}

// 暴露方法给父组件
defineExpose({
    openDialog
})
</script>

<style scoped>
:deep(.el-input),
:deep(.el-select),
:deep(.el-input-number) {
    width: 100%;
}
</style>