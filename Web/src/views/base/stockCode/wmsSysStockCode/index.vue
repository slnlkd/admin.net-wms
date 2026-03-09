<script lang="ts" setup name="wmsSysStockCode">
import { ref, reactive, onMounted } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
// 推荐设置操作 width 为 200
import { hiprint } from 'vue-plugin-hiprint';
import { getAPI } from '/@/utils/axios-utils';
import { SysPrintApi } from '/@/api-services/api';
import { SysPrint } from '/@/api-services/models';
import { formatDate } from '/@/utils/formatTime';
import { useWmsSysStockCodeApi } from '/@/api/base/stockCode/wmsSysStockCode';
import editDialog from '/@/views/base/stockCode/wmsSysStockCode/component/editDialog.vue'
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import ModifyRecord from '/@/components/table/modifyRecord.vue';
import ImportData from "/@/components/table/importData.vue";

const wmsSysStockCodeApi = useWmsSysStockCodeApi();
const printDialogRef = ref();
const editDialogRef = ref();
const importDataRef = ref();
const state = reactive({
  exportLoading: false,
  tableLoading: false,
  stores: {},
  showAdvanceQueryUI: false,
  dropdownData: {} as any,
  selectData: [] as any[],
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
  const data = await wmsSysStockCodeApi.getDropdownData(true).then(res => res.data.result) ?? {};
  state.dropdownData.warehouseId = data.warehouseId;
});

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsSysStockCodeApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
  state.tableParams.total = result?.total;
  state.tableData = result?.items ?? [];
  state.tableLoading = false;
};

// 列排序
const sortChange = async (column: any) => {
  state.tableParams.field = column.prop;
  state.tableParams.order = column.order;
  await handleQuery();
};

// 删除
const delWmsSysStockCode = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsSysStockCodeApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => {});
};

// 批量删除
const batchDelWmsSysStockCode = () => {
  ElMessageBox.confirm(`确定要删除${state.selectData.length}条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsSysStockCodeApi.batchDelete(state.selectData.map(u => ({ id: u.id }) )).then(res => {
      ElMessage.success(`成功批量删除${res.data.result}条记录`);
      handleQuery();
    });
  }).catch(() => {});
};

// 打开打印页面
const openPrintWmsSysStockCode = async (row: any) => {
  var res = await getAPI(SysPrintApi).apiSysPrintPrintNameGet('托盘管理-打印模板');
  var printTemplate = res.data.result as SysPrint;
  var template = JSON.parse(printTemplate.template);
  row['printDate'] = formatDate(new Date(), 'YYYY-mm-dd HH:MM:SS')
  printDialogRef.value.showDialog(new hiprint.PrintTemplate({template: template}), row, template.panels[0].width);
  }

// 导出数据
const exportWmsSysStockCodeCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    if (command === 'select') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await wmsSysStockCodeApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams);
      await wmsSysStockCodeApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await wmsSysStockCodeApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
}

handleQuery();
</script>
<template>
  <div class="wmsSysStockCode-container" v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }"> 
      <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="state.tableQueryParams.keyword" clearable placeholder="请输入模糊查询关键字"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="载具号">
              <el-input v-model="state.tableQueryParams.stockCode" clearable placeholder="请输入载具号"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="所属仓库">
              <el-select clearable filterable v-model="state.tableQueryParams.warehouseId" placeholder="请选择所属仓库">
                <el-option v-for="(item,index) in state.dropdownData.warehouseId ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="条码状态">
              <g-sys-dict v-model="state.tableQueryParams.status" code="StockIsUse" render-as="select" placeholder="请选择条码状态" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="托盘类型">
              <g-sys-dict v-model="state.tableQueryParams.stockType" code="StockType" render-as="select" placeholder="请选择托盘类型" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item >
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'wmsSysStockCode:page'" v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true" v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                <el-button icon="ele-ZoomOut" @click="() => state.showAdvanceQueryUI = false" v-if="state.showAdvanceQueryUI" style="margin-left:5px;"> 隐藏 </el-button>
                <!-- <el-button type="danger" style="margin-left:5px;" icon="ele-Delete" @click="batchDelWmsSysStockCode" :disabled="state.selectData.length == 0" v-auth="'wmsSysStockCode:batchDelete'"> 删除 </el-button> -->
                <el-button type="primary" style="margin-left:5px;" icon="ele-Plus" @click="editDialogRef.openDialog(null, '新增托盘管理')" v-auth="'wmsSysStockCode:add'"> 新增 </el-button>
                <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsSysStockCodeCommand">
                  <el-button type="primary" style="margin-left:5px;" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsSysStockCode:export'"> 导出 </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="select" :disabled="state.selectData.length == 0">导出选中</el-dropdown-item>
                      <el-dropdown-item command="current">导出本页</el-dropdown-item>
                      <el-dropdown-item command="all">导出全部</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
                <el-button type="warning" style="margin-left:5px;" icon="ele-MostlyCloudy" @click="importDataRef.openDialog()" v-auth="'wmsSysStockCode:import'"> 导入 </el-button>
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
        <el-table-column type="selection" width="40" align="center" v-if="auth('wmsSysStockCode:batchDelete') || auth('wmsSysStockCode:export')" />
        <el-table-column type="index" label="序号" width="55" align="center"/>
        <el-table-column prop='stockCode' label='载具号' show-overflow-tooltip />
        <el-table-column prop='warehouseId' label='所属仓库' :formatter="(row: any) => row.warehouseFkDisplayName" show-overflow-tooltip />
        <el-table-column prop='status' label='条码状态' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.status" code="StockIsUse" />
          </template>
        </el-table-column>
        <el-table-column prop='stockType' label='托盘类型' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.stockType" code="StockType" />
          </template>
        </el-table-column>
        <el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
          <template #default="scope">
            <ModifyRecord :data="scope.row" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" align="center" fixed="right" show-overflow-tooltip v-if="auth('wmsSysStockCode:update') || auth('wmsSysStockCode:delete')">
          <template #default="scope">
            <el-button icon="ele-Printer" size="small" text type="primary" @click="openPrintWmsSysStockCode(scope.row)" v-auth="'wmsSysStockCode:print'"> 打印 </el-button>
            <!-- <el-button icon="ele-Edit" size="small" text type="primary" @click="editDialogRef.openDialog(scope.row, '编辑托盘管理')" v-auth="'wmsSysStockCode:update'"> 编辑 </el-button> -->
            <!-- <el-button icon="ele-Delete" size="small" text type="primary" @click="delWmsSysStockCode(scope.row)" v-auth="'wmsSysStockCode:delete'"> 删除 </el-button> -->
          </template>
        </el-table-column>
      </el-table>
      <el-pagination 
              v-model:currentPage="state.tableParams.page"
              v-model:page-size="state.tableParams.pageSize"
              @size-change="(val: any) => handleQuery({ pageSize: val })"
              @current-change="(val: any) => handleQuery({ page: val })"
              layout="total, sizes, prev, pager, next, jumper"
              :page-sizes="[10, 20, 50, 100, 200, 500]"
              :total="state.tableParams.total"
              size="small"
              background />
      <ImportData ref="importDataRef" :import="wmsSysStockCodeApi.importData" :download="wmsSysStockCodeApi.downloadTemplate" v-auth="'wmsSysStockCode:import'" @refresh="handleQuery"/>
      <printDialog ref="printDialogRef" :title="'打印托盘管理'" @reloadTable="handleQuery" />
      <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
    </el-card>
  </div>
</template>
<style scoped>
:deep(.el-input), :deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>