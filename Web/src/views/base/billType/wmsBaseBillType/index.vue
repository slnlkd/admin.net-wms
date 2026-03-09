<script lang="ts" setup name="wmsBaseBillType">
import { ref, reactive, onMounted, watch } from "vue";
import { auth } from '/@/utils/authFunction';
// ... (omitting some imports)
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useWmsBaseBillTypeApi } from '/@/api/base/billType/wmsBaseBillType';
import editDialog from '/@/views/base/billType/wmsBaseBillType/component/editDialog.vue'
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import ModifyRecord from '/@/components/table/modifyRecord.vue';
import ImportData from "/@/components/table/importData.vue";

const wmsBaseBillTypeApi = useWmsBaseBillTypeApi();
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

// 监听查询参数中类型变化
watch(() => state.tableQueryParams.billType, (val) => {
  if (val == 1) { // 入库 - 单选
    if (Array.isArray(state.tableQueryParams.qualityInspectionStatus)) {
      state.tableQueryParams.qualityInspectionStatus = state.tableQueryParams.qualityInspectionStatus.length > 0 ? state.tableQueryParams.qualityInspectionStatus[0] : '';
    }
  } else if (val == 2) { // 出库 - 多选
    if (state.tableQueryParams.qualityInspectionStatus && !Array.isArray(state.tableQueryParams.qualityInspectionStatus)) {
      state.tableQueryParams.qualityInspectionStatus = [state.tableQueryParams.qualityInspectionStatus];
    } else if (!state.tableQueryParams.qualityInspectionStatus) {
      state.tableQueryParams.qualityInspectionStatus = [];
    }
  }
});

// 页面加载时
onMounted(async () => {
  const data = await wmsBaseBillTypeApi.getDropdownData(true).then(res => res.data.result) ?? {};
  state.dropdownData.wareHouseId = data.wareHouseId;
});

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);

  // 处理查询参数中的质检状态，转为逗号分隔字符串
  let queryParams = JSON.parse(JSON.stringify(state.tableQueryParams));
  if (Array.isArray(queryParams.qualityInspectionStatus)) {
    queryParams.qualityInspectionStatus = queryParams.qualityInspectionStatus.join(',');
  }

  const result = await wmsBaseBillTypeApi.page(Object.assign(queryParams, state.tableParams)).then(res => res.data.result);
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
const delWmsBaseBillType = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsBaseBillTypeApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => {});
};

// 批量删除
const batchDelWmsBaseBillType = () => {
  ElMessageBox.confirm(`确定要删除${state.selectData.length}条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsBaseBillTypeApi.batchDelete(state.selectData.map(u => ({ id: u.id }) )).then(res => {
      ElMessage.success(`成功批量删除${res.data.result}条记录`);
      handleQuery();
    });
  }).catch(() => {});
};

// 导出数据
const exportWmsBaseBillTypeCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    if (command === 'select') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await wmsBaseBillTypeApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams);
      await wmsBaseBillTypeApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await wmsBaseBillTypeApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
}

handleQuery();
</script>
<template>
  <div class="wmsBaseBillType-container" v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }"> 
      <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="state.tableQueryParams.keyword" clearable placeholder="请输入模糊查询关键字"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="单据类型编码">
              <el-input v-model="state.tableQueryParams.billTypeCode" clearable placeholder="请输入单据类型编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="单据类型名称">
              <el-input v-model="state.tableQueryParams.billTypeName" clearable placeholder="请输入单据类型名称"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="备注">
              <el-input v-model="state.tableQueryParams.remark" clearable placeholder="请输入备注"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="类型">
              <g-sys-dict v-model="state.tableQueryParams.billType" code="BillType" render-as="select" placeholder="请选择类型" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="质检状态">
              <g-sys-dict v-model="state.tableQueryParams.qualityInspectionStatus" code="QualityInspectionStatus" render-as="select" placeholder="请选择质检状态" clearable filterable :multiple="state.tableQueryParams.billType == 2" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="代储状态">
              <g-sys-dict v-model="state.tableQueryParams.proxyStatus" code="ProxyStatus" render-as="select" placeholder="请选择代储状态" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="所属仓库">
              <el-select clearable filterable v-model="state.tableQueryParams.wareHouseId" placeholder="请选择所属仓库">
                <el-option v-for="(item,index) in state.dropdownData.wareHouseId ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item >
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'wmsBaseBillType:page'" v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true" v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                <el-button icon="ele-ZoomOut" @click="() => state.showAdvanceQueryUI = false" v-if="state.showAdvanceQueryUI" style="margin-left:5px;"> 隐藏 </el-button>
                <el-button type="danger" style="margin-left:5px;" icon="ele-Delete" @click="batchDelWmsBaseBillType" :disabled="state.selectData.length == 0" v-auth="'wmsBaseBillType:batchDelete'"> 删除 </el-button>
                <el-button type="primary" style="margin-left:5px;" icon="ele-Plus" @click="editDialogRef.openDialog(null, '新增单据类型表')" v-auth="'wmsBaseBillType:add'"> 新增 </el-button>
                <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsBaseBillTypeCommand">
                  <el-button type="primary" style="margin-left:5px;" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsBaseBillType:export'"> 导出 </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="select" :disabled="state.selectData.length == 0">导出选中</el-dropdown-item>
                      <el-dropdown-item command="current">导出本页</el-dropdown-item>
                      <el-dropdown-item command="all">导出全部</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
                <el-button type="warning" style="margin-left:5px;" icon="ele-MostlyCloudy" @click="importDataRef.openDialog()" v-auth="'wmsBaseBillType:import'"> 导入 </el-button>
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
        <el-table-column type="selection" width="40" align="center" v-if="auth('wmsBaseBillType:batchDelete') || auth('wmsBaseBillType:export')" />
        <el-table-column type="index" label="序号" width="55" align="center"/>
        <el-table-column prop='billTypeCode' label='单据类型编码' show-overflow-tooltip />
        <el-table-column prop='billTypeName' label='单据类型名称' show-overflow-tooltip />
        <el-table-column prop='remark' label='备注' show-overflow-tooltip />
        <el-table-column prop='billType' label='类型' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.billType" code="BillType" />
          </template>
        </el-table-column>
        <el-table-column prop='qualityInspectionStatusName' label='质检状态' show-overflow-tooltip>
          <template #default="scope">
            <template v-if="scope.row.qualityInspectionStatusName">
              <el-tag v-for="status in scope.row.qualityInspectionStatusName.split('，')" :key="status" 
                :type="status === '合格' ? 'success' : (status === '不合格' ? 'danger' : 'info')"
                style="margin-right: 5px;">{{ status }}</el-tag>
            </template>
          </template>
        </el-table-column>
        <el-table-column prop='proxyStatus' label='代储状态' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.proxyStatus" code="ProxyStatus" />
          </template>
        </el-table-column>
        <el-table-column prop='wareHouseId' label='所属仓库' :formatter="(row: any) => row.wareHouseFkDisplayName" show-overflow-tooltip />
        <el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
          <template #default="scope">
            <ModifyRecord :data="scope.row" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip v-if="auth('wmsBaseBillType:update') || auth('wmsBaseBillType:delete')">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text type="primary" @click="editDialogRef.openDialog(scope.row, '编辑单据类型表')" v-auth="'wmsBaseBillType:update'"> 编辑 </el-button>
            <el-button icon="ele-Delete" size="small" text type="primary" @click="delWmsBaseBillType(scope.row)" v-auth="'wmsBaseBillType:delete'"> 删除 </el-button>
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
      <ImportData ref="importDataRef" :import="wmsBaseBillTypeApi.importData" :download="wmsBaseBillTypeApi.downloadTemplate" v-auth="'wmsBaseBillType:import'" @refresh="handleQuery"/>
      <printDialog ref="printDialogRef" :title="'打印单据类型表'" @reloadTable="handleQuery" />
      <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
    </el-card>
  </div>
</template>
<style scoped>
:deep(.el-input), :deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>