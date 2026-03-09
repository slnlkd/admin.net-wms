<script lang="ts" setup name="wmsMoveNotify">
import { ref, reactive, onMounted } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useWmsMoveNotifyApi } from '/@/api/move/moveNotify/wmsMoveNotify';
import editDialog from '/@/views/move/moveNotify/wmsMoveNotify/component/editDialog.vue'
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import ModifyRecord from '/@/components/table/modifyRecord.vue';
import ImportData from "/@/components/table/importData.vue";

const wmsMoveNotifyApi = useWmsMoveNotifyApi();
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
});

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsMoveNotifyApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
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
const delWmsMoveNotify = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsMoveNotifyApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => {});
};

// 批量删除
const batchDelWmsMoveNotify = () => {
  ElMessageBox.confirm(`确定要删除${state.selectData.length}条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsMoveNotifyApi.batchDelete(state.selectData.map(u => ({ id: u.id }) )).then(res => {
      ElMessage.success(`成功批量删除${res.data.result}条记录`);
      handleQuery();
    });
  }).catch(() => {});
};

// 导出数据
const exportWmsMoveNotifyCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    if (command === 'select') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await wmsMoveNotifyApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams);
      await wmsMoveNotifyApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await wmsMoveNotifyApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
}

handleQuery();
</script>
<template>
  <div class="wmsMoveNotify-container" v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }"> 
      <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="state.tableQueryParams.keyword" clearable placeholder="请输入模糊查询关键字"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="移库单据类型">
              <el-input v-model="state.tableQueryParams.moveBillCode" clearable placeholder="请输入移库单据类型"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="移库序号">
              <el-input-number v-model="state.tableQueryParams.moveListNo"  clearable placeholder="请输入移库序号"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="移库批次">
              <el-input v-model="state.tableQueryParams.moveLotNo" clearable placeholder="请输入移库批次"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料id">
              <el-input v-model="state.tableQueryParams.materialId" clearable placeholder="请输入物料id"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="移库物料型号">
              <el-input v-model="state.tableQueryParams.moveMaterialModel" clearable placeholder="请输入移库物料型号"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料温度">
              <el-input v-model="state.tableQueryParams.moveMaterialTemp" clearable placeholder="请输入物料温度"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料类型（字典MaterialType）">
              <el-input v-model="state.tableQueryParams.moveMaterialType" clearable placeholder="请输入物料类型（字典MaterialType）"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料状态">
              <el-input v-model="state.tableQueryParams.moveMaterialStatus" clearable placeholder="请输入物料状态"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料单位">
              <el-input v-model="state.tableQueryParams.moveMaterialUnit" clearable placeholder="请输入物料单位"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料品牌">
              <el-input v-model="state.tableQueryParams.moveMaterialBrand" clearable placeholder="请输入物料品牌"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="生产日期">
              <el-date-picker type="daterange" v-model="state.tableQueryParams.moveProductionDateRange"  value-format="YYYY-MM-DD HH:mm:ss" start-placeholder="开始日期" end-placeholder="结束日期" :default-time="[new Date('1 00:00:00'), new Date('1 23:59:59')]" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="失效日期">
              <el-date-picker type="daterange" v-model="state.tableQueryParams.moveLostDateRange"  value-format="YYYY-MM-DD HH:mm:ss" start-placeholder="开始日期" end-placeholder="结束日期" :default-time="[new Date('1 00:00:00'), new Date('1 23:59:59')]" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="移库数量">
              <el-input-number v-model="state.tableQueryParams.moveQuantity"  clearable placeholder="请输入移库数量"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="移库日期">
              <el-date-picker type="daterange" v-model="state.tableQueryParams.moveDateRange"  value-format="YYYY-MM-DD HH:mm:ss" start-placeholder="开始日期" end-placeholder="结束日期" :default-time="[new Date('1 00:00:00'), new Date('1 23:59:59')]" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="仓库id">
              <el-input v-model="state.tableQueryParams.moveWarehouseId" clearable placeholder="请输入仓库id"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="移出巷道id">
              <el-input v-model="state.tableQueryParams.moveLanewayOutCode" clearable placeholder="请输入移出巷道id"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="移入巷道id">
              <el-input v-model="state.tableQueryParams.moveLanewayInCode" clearable placeholder="请输入移入巷道id"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="移出储位编码">
              <el-input v-model="state.tableQueryParams.moveOutSlotCode" clearable placeholder="请输入移出储位编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="移入储位编码">
              <el-input v-model="state.tableQueryParams.moveInSlotCode" clearable placeholder="请输入移入储位编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="执行标志（01待执行、02正在执行、03已完成、04已上传）">
              <el-input v-model="state.tableQueryParams.moveExecuteFlag" clearable placeholder="请输入执行标志（01待执行、02正在执行、03已完成、04已上传）"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="备注">
              <el-input v-model="state.tableQueryParams.moveRemark" clearable placeholder="请输入备注"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="移库任务号">
              <el-input v-model="state.tableQueryParams.moveTaskNo" clearable placeholder="请输入移库任务号"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="库存表id">
              <el-input v-model="state.tableQueryParams.stockStockCodeId" clearable placeholder="请输入库存表id"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item >
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'wmsMoveNotify:page'" v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true" v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                <el-button icon="ele-ZoomOut" @click="() => state.showAdvanceQueryUI = false" v-if="state.showAdvanceQueryUI" style="margin-left:5px;"> 隐藏 </el-button>
                <el-button type="danger" style="margin-left:5px;" icon="ele-Delete" @click="batchDelWmsMoveNotify" :disabled="state.selectData.length == 0" v-auth="'wmsMoveNotify:batchDelete'"> 删除 </el-button>
                <el-button type="primary" style="margin-left:5px;" icon="ele-Plus" @click="editDialogRef.openDialog(null, '新增移库单据明细表')" v-auth="'wmsMoveNotify:add'"> 新增 </el-button>
                <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsMoveNotifyCommand">
                  <el-button type="primary" style="margin-left:5px;" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsMoveNotify:export'"> 导出 </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="select" :disabled="state.selectData.length == 0">导出选中</el-dropdown-item>
                      <el-dropdown-item command="current">导出本页</el-dropdown-item>
                      <el-dropdown-item command="all">导出全部</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
                <el-button type="warning" style="margin-left:5px;" icon="ele-MostlyCloudy" @click="importDataRef.openDialog()" v-auth="'wmsMoveNotify:import'"> 导入 </el-button>
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
        <el-table-column type="selection" width="40" align="center" v-if="auth('wmsMoveNotify:batchDelete') || auth('wmsMoveNotify:export')" />
        <el-table-column type="index" label="序号" width="55" align="center"/>
        <el-table-column prop='moveBillCode' label='移库单据类型' show-overflow-tooltip />
        <el-table-column prop='moveListNo' label='移库序号' show-overflow-tooltip />
        <el-table-column prop='moveLotNo' label='移库批次' show-overflow-tooltip />
        <el-table-column prop='materialId' label='物料id' show-overflow-tooltip />
        <el-table-column prop='moveMaterialModel' label='移库物料型号' show-overflow-tooltip />
        <el-table-column prop='moveMaterialTemp' label='物料温度' show-overflow-tooltip />
        <el-table-column prop='moveMaterialType' label='物料类型（字典MaterialType）' show-overflow-tooltip />
        <el-table-column prop='moveMaterialStatus' label='物料状态' show-overflow-tooltip />
        <el-table-column prop='moveMaterialUnit' label='物料单位' show-overflow-tooltip />
        <el-table-column prop='moveMaterialBrand' label='物料品牌' show-overflow-tooltip />
        <el-table-column prop='moveProductionDate' label='生产日期' show-overflow-tooltip />
        <el-table-column prop='moveLostDate' label='失效日期' show-overflow-tooltip />
        <el-table-column prop='moveQuantity' label='移库数量' show-overflow-tooltip />
        <el-table-column prop='moveDate' label='移库日期' show-overflow-tooltip />
        <el-table-column prop='moveWarehouseId' label='仓库id' show-overflow-tooltip />
        <el-table-column prop='moveLanewayOutCode' label='移出巷道id' show-overflow-tooltip />
        <el-table-column prop='moveLanewayInCode' label='移入巷道id' show-overflow-tooltip />
        <el-table-column prop='moveOutSlotCode' label='移出储位编码' show-overflow-tooltip />
        <el-table-column prop='moveInSlotCode' label='移入储位编码' show-overflow-tooltip />
        <el-table-column prop='moveExecuteFlag' label='执行标志（01待执行、02正在执行、03已完成、04已上传）' show-overflow-tooltip />
        <el-table-column prop='moveRemark' label='备注' show-overflow-tooltip />
        <el-table-column prop='moveTaskNo' label='移库任务号' show-overflow-tooltip />
        <el-table-column prop='stockStockCodeId' label='库存表id' show-overflow-tooltip />
        <el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
          <template #default="scope">
            <ModifyRecord :data="scope.row" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip v-if="auth('wmsMoveNotify:update') || auth('wmsMoveNotify:delete')">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text type="primary" @click="editDialogRef.openDialog(scope.row, '编辑移库单据明细表')" v-auth="'wmsMoveNotify:update'"> 编辑 </el-button>
            <el-button icon="ele-Delete" size="small" text type="primary" @click="delWmsMoveNotify(scope.row)" v-auth="'wmsMoveNotify:delete'"> 删除 </el-button>
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
      <ImportData ref="importDataRef" :import="wmsMoveNotifyApi.importData" :download="wmsMoveNotifyApi.downloadTemplate" v-auth="'wmsMoveNotify:import'" @refresh="handleQuery"/>
      <printDialog ref="printDialogRef" :title="'打印移库单据明细表'" @reloadTable="handleQuery" />
      <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
    </el-card>
  </div>
</template>
<style scoped>
:deep(.el-input), :deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>