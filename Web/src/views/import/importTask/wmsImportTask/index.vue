<template>
  <div class="wmsImportTask-container" v-loading="state.exportLoading">

    <el-card class="full-table" shadow="hover">
      <template #header>
        <el-icon size="16"
          style="margin-right: 3px; display: inline; vertical-align: middle"><ele-Collection /></el-icon>入库任务
      </template>
      <el-form :model="state.tableQueryParams" ref="queryForm" label-width="auto" class="mb8" >
        <el-row :gutter="5">
          <el-col :xs="24" :sm="12" :md="8" :lg="4" :xl="4" class="mb5">
            <el-form-item label="所属仓库">
              <el-select clearable filterable v-model="state.tableQueryParams.warehouseId" placeholder="所属仓库">
                <el-option v-for="(item, index) in state.dropdownData.warehouseId ?? []" :key="index" :value="item.id"
                  :label="item.warehouseName" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="8" :lg="4" :xl="4" class="mb5">
            <el-form-item label="载具条码">
              <el-input v-model="state.tableQueryParams.stockCode" clearable placeholder="载具条码" @keyup.enter="handleQuery" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="8" :lg="4" :xl="4" class="mb5">
            <el-form-item label="任务编码">
              <el-input v-model="state.tableQueryParams.taskNo" clearable placeholder="任务编码" @keyup.enter="handleQuery" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="8" :lg="4" :xl="4" class="mb5">
            <el-form-item label="执行状态">
              <g-sys-dict v-model="state.tableQueryParams.status" code="TaskStatus" render-as="select"
                placeholder="执行状态" clearable filterable />
            </el-form-item>
          </el-col>
          <!-- <el-col :xs="24" :sm="12" :md="12" :lg="6" :xl="6" class="mb5">
            <el-form-item label="储位位置">
              <div class="location-inputs" style="display: flex; gap: 5px; width: 100%;">
                <el-input-number v-model="state.tableQueryParams.slotRow" placeholder="排" controls-position="right" :min="1" @keyup.enter="handleQuery" style="width: 100px" />
                <el-input-number v-model="state.tableQueryParams.slotColumn" placeholder="列" controls-position="right" :min="1" @keyup.enter="handleQuery" style="width: 100px" />
                <el-input-number v-model="state.tableQueryParams.slotLayer" placeholder="层" controls-position="right" :min="1" @keyup.enter="handleQuery" style="width: 100px" />
              </div>
            </el-form-item>
          </el-col> -->
          <el-col :xs="24" :sm="12" :md="12" :lg="6" :xl="6" class="mb5">
            <el-form-item label-width="0px" style="display: flex; white-space: nowrap;">
              <el-button type="primary" icon="ele-Search" @click="handleQuery" v-auth="'wmsImportTask:page'" v-reclick="1000">查询</el-button>
              <el-button icon="ele-Refresh" @click="handleReset" style="margin-left: 5px">重置</el-button>
              <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsImportTaskCommand" style="margin-left: 5px">
                <el-button type="primary" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsImportTask:export'">
                  导出<el-icon class="el-icon--right"><ele-ArrowDown /></el-icon>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item command="select" :disabled="state.selectData.length == 0">导出选中</el-dropdown-item>
                    <el-dropdown-item command="current">导出本页</el-dropdown-item>
                    <el-dropdown-item command="all">导出全部</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }"
        style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange"
        border>
        <el-table-column type="selection" width="40" align="center"
          v-if="auth('wmsImportTask:batchDelete') || auth('wmsImportTask:export')" />
        <el-table-column type="index" :fixed="true" align="center" label="序号" width="55" />
        <el-table-column prop='taskNo' :fixed="true" align="center" label='任务编码' sortable='custom' width="125"
          show-overflow-tooltip />
        <el-table-column prop='sender' :fixed="true" align="center" label='发送方' width="55" show-overflow-tooltip />
        <el-table-column prop='receiver' align="center" label='接收方' width="55" show-overflow-tooltip />
         <el-table-column prop='stockCode' align="center" label='载具条码' sortable='custom' width="100"
          show-overflow-tooltip />
        <el-table-column prop='isSuccess' align="center" label='是否成功' width="65" show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.isSuccess" code="YesNoEnumTwo" />
          </template>
        </el-table-column>
        <el-table-column prop='status' align="center" label='任务状态' width="100" sortable='custom' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.status" code="ImportTaskStatus" />
          </template>
        </el-table-column>
        <el-table-column prop='msg' align="center" label='报文' min-width="200"  show-overflow-tooltip />
        <el-table-column prop='sendDate' align="center" label='发送时间' sortable='custom' width="140"
          show-overflow-tooltip />
        <el-table-column prop='backDate' align="center" label='结束时间' sortable='custom' width="140"
          show-overflow-tooltip />
       
        <el-table-column prop='startLocation' align="center" label='起始位置' sortable='custom' width="120"
          show-overflow-tooltip />
        <el-table-column prop='endLocation' align="center" label='结束位置' sortable='custom' width="120"
          show-overflow-tooltip />
        <el-table-column prop='cancelDate' align="center" label='取消时间' sortable='custom' width="140"
          show-overflow-tooltip />
        <el-table-column prop='finishDate' align="center" label='完成时间' sortable='custom' width="140"
          show-overflow-tooltip />
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip>
          <template #default="scope">
            <el-button icon="ele-RefreshLeft" size="small" text type="danger"
              v-if="scope.row.status == 0 || scope.row.status == 1" @click="cancelTask(scope.row)"> 取消
            </el-button>
            <el-button icon="ele-CircleCheck" v-if="scope.row.status == 0 || scope.row.status == 1" size="small" text
              type="success" :loading="scope.row.loading" @click="complete(scope.row)"> 完成
            </el-button>

          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:currentPage="state.tableParams.page" v-model:page-size="state.tableParams.pageSize"
        @size-change="(val: any) => handleQuery({ pageSize: val })"
        @current-change="(val: any) => handleQuery({ page: val })" layout="total, sizes, prev, pager, next, jumper"
        :page-sizes="[10, 20, 50, 100, 200, 500]" :total="state.tableParams.total" size="small" background />
      <ImportData ref="importDataRef" :import="wmsImportTaskApi.importData"
        :download="wmsImportTaskApi.downloadTemplate" v-auth="'wmsImportTask:import'" @refresh="handleQuery" />
      <printDialog ref="printDialogRef" :title="'打印入库任务表'" @reloadTable="handleQuery" />
      <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
    </el-card>
  </div>
</template>

<script lang="ts" setup name="wmsImportTask">
import { ref, reactive, onMounted } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useWmsImportTaskApi } from '/@/api/import/importTask/wmsImportTask';
import { useWmsBaseWareHouseApi } from '/@/api/base/baseWarehouse/wmsBaseWareHouse';
import { usewmsPortApi } from "/@/api/port/wmsPort";
const wmsBaseWareHouseApi = useWmsBaseWareHouseApi();
const wmsImportTaskApi = useWmsImportTaskApi();
const wmsPortApi = usewmsPortApi();
const printDialogRef = ref();
const importDataRef = ref();
const state = reactive({
  exportLoading: false,
  tableLoading: false,
  stores: {},
  showAdvanceQueryUI: false,
  dropdownParams: {
    page: 1,
    pageSize: 200,
    total: 0,
    field: 'createTime', // 默认的排序字段
    order: 'descending', // 排序方向
    descStr: 'descending', // 降序排序的关键字符
  },
  dropdownQueryParams: {} as any,
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
  const warehousedata = await wmsBaseWareHouseApi.page(Object.assign(state.dropdownQueryParams, state.dropdownParams)).then(res => res.data.result);
  state.dropdownData.warehouseId = warehousedata.items;
});
//重置
const handleReset = () => {
  state.tableQueryParams = {};
  handleQuery();
};
// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsImportTaskApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
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
const delWmsImportTask = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsImportTaskApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => { });
};
//取消
const cancelTask = async (row: any) => {
  ElMessageBox.confirm(`确定取消选中的入库任务?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsImportTaskApi.cancelTask({ taskNo: row.taskNo });
    handleQuery();
    ElMessage.success("取消成功");
  }).catch(() => { });
}
//完成
const complete = async (row: any) => {
  ElMessageBox.confirm(`确定完成选中的入库任务?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    row.loading = true;
    try {
      await wmsPortApi.feedBack({ taskNo: row.taskNo, code: "1", taskType: "0", stockCode: row.stockCode, taskEnd: row.endLocation, taskBegin: row.startLocation }).then(res => {
        if (res.data.result?.code == 0) {
          ElMessage.error(res.data.result.msg || '操作失败');
        }
        if (res.data.result?.code == 1) {
          ElMessage.success(res.data.result.msg || '操作成功');
        }
      });
      handleQuery();
    } finally {
      row.loading = false;
    }
  }).catch(() => { });
};
// 导出数据
const exportWmsImportTaskCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    if (command === 'select') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await wmsImportTaskApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams);
      await wmsImportTaskApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await wmsImportTaskApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
}

handleQuery();
</script>

<style scoped>


:deep(.el-input),
:deep(.el-select),
:deep(.el-input-number) {
  width: 100%;
}
:deep(.el-card__header) {
  padding: 10px 20px;
}

:deep(.el-card__body) {
  padding: 5px;
}
</style>