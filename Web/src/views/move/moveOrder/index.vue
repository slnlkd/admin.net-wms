<template>
  <div class="import-notify-container">
    <el-row :gutter="5" class="import-notify-table">
      <el-col :span="24" :xs="24">
        <el-card class="full-table" shadow="hover" :body-style="{ height: '100%', display: 'flex', flexDirection: 'column' }">
          <template #header>
            <el-icon size="16"
              style="margin-right: 3px; display: inline; vertical-align: middle"><ele-Collection /></el-icon>移库单据
          </template>

          <el-form :model="state.tableQueryParams" ref="queryForm" label-width="90">
            <el-row :gutter="15">
              <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
                <el-form-item label="关键字">
                  <el-input v-model="state.tableQueryParams.keyword" clearable placeholder="请输入模糊查询关键字"/>
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
                <el-form-item label="移库单号">
                  <el-input v-model="state.tableQueryParams.moveNo" clearable placeholder="请输入移库单号"/>
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
                <el-form-item label="状态">
                  <g-sys-dict v-model="state.tableQueryParams.moveStauts" code="ExecuteFlag" render-as="select" placeholder="请选择状态" clearable filterable />
                </el-form-item>
              </el-col>

              <el-col :xs="24" :sm="12" :md="12" :lg="10" :xl="10" class="mb10">
                <el-form-item label-width="0px">
                  <el-button type="primary" icon="ele-Search" @click="handleQuery" v-auth="'wmsImportNotify:page'" v-reclick="1000">查询</el-button>
                  <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}"> 重置 </el-button>
                  <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true" v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                  <el-button icon="ele-ZoomOut" @click="() => state.showAdvanceQueryUI = false" v-if="state.showAdvanceQueryUI" style="margin-left:5px;"> 隐藏 </el-button>

                  <el-button type="primary" style="margin-left:5px;" icon="ele-Plus" @click="editDialogRef.openDialog(null, '新增移库单')" v-auth="'wmsMoveOrder:add'"> 新增 </el-button>
                  <!-- <el-button type="warning" icon="ele-MostlyCloudy" @click="importDataRef.openDialog()" v-auth="'wmsImportNotify:import'" style="margin-left: 10px">导入</el-button> -->
                </el-form-item>
              </el-col>
            </el-row>
          </el-form>
          <el-table :data="state.tableData" @selection-change="handleSelectionChange" style="width: 100%; flex: 1; height: 0;"
            v-loading="state.tableLoading" @row-click="handleImportNotify" tooltip-effect="light" row-key="id"
            @sort-change="sortChange" border>
            <el-table-column type="index" label="序号" width="55" align="center"/>
            <el-table-column prop='moveNo' label='移库单号' show-overflow-tooltip />
            <el-table-column prop='moveStauts' label='状态' show-overflow-tooltip>
              <template #default="scope">
                <g-sys-dict v-model="scope.row.moveStauts" code="ExecuteFlag" />
              </template>
            </el-table-column>
            <el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
              <template #default="scope">
                <ModifyRecord :data="scope.row" />
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
          <printDialog ref="printDialogRef" :title="'打印移库单据表'" @reloadTable="handleQuery" />
          <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
        </el-card>
      </el-col>
    </el-row>
    <el-row :gutter="5" class="import-notify-detail">
      <el-col :span="24" :xs="24">
        <el-card class="full-table" shadow="hover" :body-style="{ height: '100%', display: 'flex', flexDirection: 'column' }">
          <template #header>
            <div style="display: flex; justify-content: space-between; align-items: center;">
              <div style="display: flex; align-items: center;">
                <el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"><ele-Collection /></el-icon>
                <span>移库明细【{{ state.MoveBillCode }}】</span>
              </div>
              <div style="display: flex; gap: 5px; align-items: center;">
              
              </div>
            </div>
          </template>
          <el-table :data="state.tableDetailData" @selection-change="handleDetailSelectionChange" style="width: 100%; flex: 1; height: 0;"
            v-loading="state.tableDetailLoading" tooltip-effect="light" row-key="id" @sort-change="" border>
            <el-table-column type="index" label="序号" width="55" align="center" />
            <el-table-column prop='stockStockCodeId' label='载具条码' align="center" width="160" show-overflow-tooltip />
            <el-table-column prop='moveOutSlotCode' label='移出库位' align="center" width="160" show-overflow-tooltip />
            <el-table-column prop='moveInSlotCode' label='移入库位' align="center" width="160" show-overflow-tooltip />
            <el-table-column prop='materialCode' label='物品编码' align="center"  show-overflow-tooltip />            
            <el-table-column prop='materialName' label='物品名称' align="center"  show-overflow-tooltip />
            
          </el-table>
          <el-pagination v-model:current-page="state.tableDetailParams.page"
            v-model:page-size="state.tableDetailParams.pageSize" @size-change=""
            @current-change="" layout="total, sizes, prev, pager, next, jumper"
            :page-sizes="[10, 20, 50, 100, 200, 500]" :total="state.tableDetailParams.total" size="small" background />
          <repairPrintDialog ref="repairPrintDialogRef" @reloadTable="" />
          <issueDialog ref="issueDialogRef" @reloadTable="" />

        </el-card>
      </el-col>
    </el-row>

  </div>
</template>

<script lang="ts" setup name="wmsMoveOrder">
import { ref, reactive, onMounted } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useWmsMoveOrderApi } from '/@/api/move/moveOrder/wmsMoveOrder';
import { useWmsMoveNotifyApi } from '/@/api/move/moveNotify/wmsMoveNotify';
import editDialog from '/@/views/move/moveOrder/component/editDialog.vue'
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import ModifyRecord from '/@/components/table/modifyRecord.vue';

const wmsMoveOrderApi = useWmsMoveOrderApi();
const wmsMoveOrderNotifyApi = useWmsMoveNotifyApi();
const printDialogRef = ref();
const editDialogRef = ref();
interface MoveNotifyDetailRow {
  id: number;
  materialCode: string;
  materialName: string;
  lotNo: string;
  importExecuteFlag: string;
  importTaskStatus: number;
  taskStatus: number;
  labeling: string;
  labelJudgment: number;
  [key: string]: any;
}

const state = reactive({
  exportLoading: false,
  tableLoading: false,
  stores: {},
  showAdvanceQueryUI: false,
  dropdownData: {} as any,
  selectData: [] as any[],
  tableQueryParams: {} as any,
  tableDetailQueryParams: {} as any,
  selectDetailData: [] as MoveNotifyDetailRow[],
  tableParams: {
    page: 1,
    pageSize: 20,
    total: 0,
    field: 'createTime', // 默认的排序字段
    order: 'descending', // 排序方向
    descStr: 'descending', // 降序排序的关键字符
  },
  tableDetailParams: {
    page: 1,
    pageSize: 20,
    total: 0,
    field: 'createTime', // 默认的排序字段
    order: 'descending', // 排序方向
    descStr: 'descending', // 降序排序的关键字符
    MoveBillCode:''
  },
  tableData: [],
  tableDetailData:[],
  tableDetailLoading:false,
  MoveBillCode:'',
});

// 页面加载时
onMounted(async () => {
});

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsMoveOrderApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
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
const delWmsMoveOrder = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsMoveOrderApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => {});
};

// 批量删除
const batchDelWmsMoveOrder = () => {
  ElMessageBox.confirm(`确定要删除${state.selectData.length}条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsMoveOrderApi.batchDelete(state.selectData.map(u => ({ id: u.id }) )).then(res => {
      ElMessage.success(`成功批量删除${res.data.result}条记录`);
      handleQuery();
    });
  }).catch(() => {});
};


// 查询操作
const handleDetailQuery = async (params: any = {}) => {
  state.tableDetailLoading = true;
  state.tableDetailParams = Object.assign(state.tableDetailParams, params);
  const result = await wmsMoveOrderNotifyApi.page(Object.assign(state.tableDetailQueryParams, state.tableDetailParams)).then(res => res.data.result);
  state.tableDetailParams.total = result?.total;
  state.tableDetailData = result?.items ?? [];
  state.tableDetailLoading = false;
};


const handleDetailSelectionChange = (val: any[]) => {
  state.selectDetailData = val;
};
const handleSelectionChange = (val: any[]) => {
  state.selectData = val;
};
// 打开页面
const handleImportNotify = (row: any) => {
  state.tableDetailParams.MoveBillCode = row.moveNo;
  state.MoveBillCode = row.moveNo;
  handleDetailQuery();
};
handleQuery();
</script>


<style lang="scss" scoped>
.sys-dict-container {
  flex-direction: row !important;
}

.handle-form {
  margin-bottom: var(--el-card-padding);

  .el-form-item,
  .el-form-item:last-of-type {
    margin: 0 10px !important;
  }
}

.import-notify-container {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 100px); // Adjust based on your layout header/tabs height
  padding: 5px;
  box-sizing: border-box;
}

.import-notify-table {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0; // Crucial for nested flex scrolling
  margin-bottom: 5px; // Spacing between top and bottom sections
  
  :deep(.el-col) {
    height: 100%;
    display: flex;
    flex-direction: column;
  }
}

.import-notify-detail {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0; // Crucial for nested flex scrolling

  :deep(.el-col) {
    height: 100%;
    display: flex;
    flex-direction: column;
  }
}

// Force el-card to fill the flex item
:deep(.full-table) {
  flex: 1;
  display: flex;
  flex-direction: column;
  border: none; // Optional: remove border if double borders occur
  
  .el-card__body {
    flex: 1;
    display: flex;
    flex-direction: column;
    padding: 10px; // Adjust padding as needed
    overflow: hidden; // Prevent card body itself from scrolling
  }
}

:deep(.notice-bar) {
  position: absolute;
  display: inline-flex;
  height: 25px !important;
  width: 500px;
  overflow: hidden !important;

  div[data-slate-editor] {
    text-wrap-mode: nowrap;
  }
}
</style>
