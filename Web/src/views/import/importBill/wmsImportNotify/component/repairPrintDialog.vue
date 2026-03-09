<template>
    <el-dialog v-model="dialogVisible" title="补打" width="600px" :close-on-click-modal="false" append-to-body>
        <el-card class="full-table" shadow="hover" style="margin-top: 5px">
            <el-table :data="state.tableData" @selection-change="(val) => { state.selectData = val; }"
                style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" border>
                <el-table-column type="selection" width="40" align="center" />
                <el-table-column type="index" label="序号" width="55" align="center" />
                <el-table-column prop='labelID' label='标签' align="center" show-overflow-tooltip />
                <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip>
                    <template #default="scope">
                        <el-button icon="ele-Printer" size="small" text type="primary" @click="openPrint(scope.row)">
                            补打
                        </el-button>
                    </template>
                </el-table-column>
            </el-table>
            <el-pagination v-model:currentPage="state.tableParams.page" v-model:page-size="state.tableParams.pageSize"
                @size-change="(val) => handleQuery({ pageSize: val })"
                @current-change="(val) => handleQuery({ page: val })" layout="total, sizes, prev, pager, next, jumper"
                :page-sizes="[10, 20, 50, 100, 200, 500]" :total="state.tableParams.total" size="small" background />
            <printDialog ref="printDialogRef" :title="'打印入库明细'" @reloadTable="handleQuery" />
        </el-card>
        <template #footer>
            <div class="dialog-footer">
                <el-button @click="handleCancel">取消</el-button>
            </div>
        </template>
    </el-dialog>
</template>

<script lang="ts" setup>
import { ref, reactive, onMounted } from "vue";
import { ElMessage, ElMessageBox } from 'element-plus'
import { hiprint } from 'vue-plugin-hiprint';
import { getAPI } from '/@/utils/axios-utils';
import { SysPrintApi } from '/@/api-services/api';
import { SysPrint } from '/@/api-services/models';
import { formatDate } from '/@/utils/formatTime';
import { usewmsImportLabelPrintApi } from '/@/api/import/importLabelPrint/wmsImportLabelPrint';
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
const wmsImportLabelPrintApi = usewmsImportLabelPrintApi();
const printDialogRef = ref();
// 弹窗显示控制
const dialogVisible = ref(false)
const state = reactive({
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
    state.tableLoading = true;
    state.tableParams = Object.assign(state.tableParams, params);
    const result = await wmsImportLabelPrintApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
    state.tableParams.total = result?.total;
    state.tableData = result?.items ?? [];
    state.tableLoading = false;
};

// 打开打印页面
const openPrint = async (row: any) => {
    var res = await getAPI(SysPrintApi).apiSysPrintPrintNameGet('入库单据-物料明细');
    var printTemplate = res.data.result as SysPrint;
    var template = JSON.parse(printTemplate.template);
    row['printDate'] = formatDate(new Date(), 'YYYY-mm-dd HH:MM:SS')
    var printDataRes = await wmsImportLabelPrintApi.print({ labelID: row.labelID, importDetailId: row.importDetailId, boxCode: '', isprint: '0', number: 1 });
    var printData = printDataRes.data.result;
    template.datasets = [
        {
            name: '入库单据-物料明细',
            data: printData
        }
    ];
    printDialogRef.value.showDialog(new hiprint.PrintTemplate({ template: template }), printData, template.panels[0].width);
}

// 打开弹窗
const openDialog = async (row: any = null) => {
    state.tableQueryParams.importDetailId = row.id;
    await handleQuery();
    dialogVisible.value = true
}

// 取消操作
const handleCancel = () => {
    dialogVisible.value = false
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