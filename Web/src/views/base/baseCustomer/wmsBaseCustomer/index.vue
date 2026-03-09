<script lang="ts" setup name="wmsBaseCustomer">
import { ref, reactive, onMounted, onUnmounted } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage, ElNotification } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useWmsBaseCustomerApi } from '/@/api/base/baseCustomer/wmsBaseCustomer';
import { useWmsBaseCustomerPresetApi } from '/@/api/base/baseCustomerPreset/wmsBaseCustomerPreset';
import editDialog from '/@/views/base/baseCustomer/wmsBaseCustomer/component/editDialog.vue'
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import ModifyRecord from '/@/components/table/modifyRecord.vue';
import ImportData from "/@/components/table/importData.vue";
import approvalDialog from '/@/views/base/baseCustomerPreset/wmsBaseCustomerPreset/component/approvalDialog.vue'

const wmsBaseCustomerApi = useWmsBaseCustomerApi();
const wmsBaseCustomerPresetApi = useWmsBaseCustomerPresetApi();
const printDialogRef = ref();
const editDialogRef = ref();
const importDataRef = ref();
const approvalDialogRef = ref();
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
  pendingApprovalCount: 0, // 待审核数量
  refreshTimer: null as any, // 定时器
  isInitialized: false, // 是否已初始化
});

// 页面加载时
onMounted(async () => {
  // 获取待审核数量
  await getPendingApprovalCount();
  state.isInitialized = true;

  // 设置定时刷新待审核数量（每3秒刷新一次）
  state.refreshTimer = setInterval(() => {
    getPendingApprovalCount();
  }, 3000);
});

// 页面卸载时
onUnmounted(() => {
  // 清除定时器
  if (state.refreshTimer) {
    clearInterval(state.refreshTimer);
  }
});

// 获取待审核数量
const getPendingApprovalCount = async () => {
  try {
    const queryParams = { approvalStatus: 0, page: 1, pageSize: 1 };
    const result = await wmsBaseCustomerPresetApi.page(queryParams).then(res => res.data.result);
    const newCount = result?.total || 0;

    // 初次加载时，如果有待审核数据，提示一次
    if (!state.isInitialized && newCount > 0) {
      ElNotification({
        title: '待审核通知',
        message: `当前有 ${newCount} 条客户信息申请待审核`,
        type: 'info',
        duration: 5000,
        position: 'top-right'
      });
    }
    // 检查是否有新数据进来（只在初始化后才有新数据的概念）
    else if (state.isInitialized && newCount > state.pendingApprovalCount) {
      const diff = newCount - state.pendingApprovalCount;
      ElNotification({
        title: '新申请通知',
        message: `有 ${diff} 条新的客户信息申请待审核`,
        type: 'info',
        duration: 5000,
        position: 'top-right'
      });

      // 刷新审核弹窗列表（如果弹窗已打开）
      if (approvalDialogRef.value) {
        approvalDialogRef.value.refreshTable();
      }
    }

    state.pendingApprovalCount = newCount;
  } catch (error) {
    console.error('获取待审核数量失败:', error);
    state.pendingApprovalCount = 0;
  }
};

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsBaseCustomerApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
  state.tableParams.total = result?.total;
  state.tableData = result?.items ?? [];
  state.tableLoading = false;
  // 不在这里调用 getPendingApprovalCount，避免重复调用
};

// 列排序
const sortChange = async (column: any) => {
  state.tableParams.field = column.prop;
  state.tableParams.order = column.order;
  await handleQuery();
};

// 删除
const delWmsBaseCustomer = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsBaseCustomerApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => {});
};

// 批量删除
const batchDelWmsBaseCustomer = () => {
  ElMessageBox.confirm(`确定要删除${state.selectData.length}条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsBaseCustomerApi.batchDelete(state.selectData.map(u => ({ id: u.id }) )).then(res => {
      ElMessage.success(`成功批量删除${res.data.result}条记录`);
      handleQuery();
    });
  }).catch(() => {});
};

// 导出数据
const exportWmsBaseCustomerCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    if (command === 'select') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await wmsBaseCustomerApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams);
      await wmsBaseCustomerApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await wmsBaseCustomerApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
}

handleQuery();
</script>
<template>
  <div class="wmsBaseCustomer-container" v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }"> 
      <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="state.tableQueryParams.keyword" clearable placeholder="请输入模糊查询关键字"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="客户编码">
              <el-input v-model="state.tableQueryParams.customerCode" clearable placeholder="请输入客户编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="客户名称">
              <el-input v-model="state.tableQueryParams.customerName" clearable placeholder="请输入客户名称"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="客户地址">
              <el-input v-model="state.tableQueryParams.customerAddress" clearable placeholder="请输入客户地址"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="联系人">
              <el-input v-model="state.tableQueryParams.customerLinkman" clearable placeholder="请输入联系人"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="联系电话">
              <el-input v-model="state.tableQueryParams.customerPhone" clearable placeholder="请输入联系电话"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="备注">
              <el-input v-model="state.tableQueryParams.remark" clearable placeholder="请输入备注"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item >
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'wmsBaseCustomer:page'" v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true" v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                <el-button icon="ele-ZoomOut" @click="() => state.showAdvanceQueryUI = false" v-if="state.showAdvanceQueryUI" style="margin-left:5px;"> 隐藏 </el-button>
                <el-button type="danger" style="margin-left:5px;" icon="ele-Delete" @click="batchDelWmsBaseCustomer" :disabled="state.selectData.length == 0" v-auth="'wmsBaseCustomer:batchDelete'"> 删除 </el-button>
                <el-button type="primary" style="margin-left:5px;" icon="ele-Plus" @click="editDialogRef.openDialog(null, '新增客户信息')" v-auth="'wmsBaseCustomer:add'"> 新增 </el-button>
                <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsBaseCustomerCommand">
                  <el-button type="primary" style="margin-left:5px;" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsBaseCustomer:export'"> 导出 </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="select" :disabled="state.selectData.length == 0">导出选中</el-dropdown-item>
                      <el-dropdown-item command="current">导出本页</el-dropdown-item>
                      <el-dropdown-item command="all">导出全部</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
                <el-button type="warning" style="margin-left:5px;" icon="ele-MostlyCloudy" @click="importDataRef.openDialog()" v-auth="'wmsBaseCustomer:import'"> 导入 </el-button>
                <el-button type="success" style="margin-left:5px;" icon="ele-Check" @click="approvalDialogRef.openDialog()" v-auth="'wmsBaseCustomerPreset:auth'" v-if="state.pendingApprovalCount > 0">
                  审核({{ state.pendingApprovalCount }})
                </el-button>
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
        <el-table-column type="selection" width="40" align="center" v-if="auth('wmsBaseCustomer:batchDelete') || auth('wmsBaseCustomer:export')" />
        <el-table-column type="index" label="序号" width="55" align="center"/>
        <el-table-column prop='customerCode' label='客户编码' show-overflow-tooltip />
        <el-table-column prop='customerName' label='客户名称' show-overflow-tooltip />
        <el-table-column prop='customerAddress' label='客户地址' show-overflow-tooltip />
        <el-table-column prop='customerLinkman' label='联系人' show-overflow-tooltip />
        <el-table-column prop='customerPhone' label='联系电话' show-overflow-tooltip />
        <el-table-column prop='remark' label='备注' show-overflow-tooltip />
        <el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
          <template #default="scope">
            <ModifyRecord :data="scope.row" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip v-if="auth('wmsBaseCustomer:update') || auth('wmsBaseCustomer:delete')">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text type="primary" @click="editDialogRef.openDialog(scope.row, '编辑客户信息')" v-auth="'wmsBaseCustomer:update'"> 编辑 </el-button>
            <el-button icon="ele-Delete" size="small" text type="primary" @click="delWmsBaseCustomer(scope.row)" v-auth="'wmsBaseCustomer:delete'"> 删除 </el-button>
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
      <ImportData ref="importDataRef" :import="wmsBaseCustomerApi.importData" :download="wmsBaseCustomerApi.downloadTemplate" v-auth="'wmsBaseCustomer:import'" @refresh="handleQuery"/>
      <printDialog ref="printDialogRef" :title="'打印客户信息'" @reloadTable="handleQuery" />
      <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
      <approvalDialog ref="approvalDialogRef" @refresh="getPendingApprovalCount" @refreshCustomerList="handleQuery" />
    </el-card>
  </div>
</template>
<style scoped>
:deep(.el-input), :deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>