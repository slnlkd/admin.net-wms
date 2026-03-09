<script lang="ts" setup name="wmsBaseSlot">
import { ref, reactive, onMounted } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useWmsBaseSlotApi } from '/@/api/base/baseSlot/wmsBaseSlot';
import editDialog from '/@/views/base/baseSlot/wmsBaseSlot/component/editDialog.vue'
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import ModifyRecord from '/@/components/table/modifyRecord.vue';
import ImportData from "/@/components/table/importData.vue";

const wmsBaseSlotApi = useWmsBaseSlotApi();
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
  const data = await wmsBaseSlotApi.getDropdownData(true).then(res => res.data.result) ?? {};
  state.dropdownData.warehouseId = data.warehouseId;
  state.dropdownData.slotAreaId = data.slotAreaId;
  state.dropdownData.slotLanewayId = data.slotLanewayId;
});

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsBaseSlotApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
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
const delWmsBaseSlot = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsBaseSlotApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => {});
};

// 批量删除
const batchDelWmsBaseSlot = () => {
  ElMessageBox.confirm(`确定要删除${state.selectData.length}条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsBaseSlotApi.batchDelete(state.selectData.map(u => ({ id: u.id }) )).then(res => {
      ElMessage.success(`成功批量删除${res.data.result}条记录`);
      handleQuery();
    });
  }).catch(() => {});
};
// 修改状态
const changeStatus = async (row: any,name: any) => {
  // 入库锁定状态更改
  if(name == "slotImlockFlag"){
    if(row.slotImlockFlag == 0){
      row.slotImlockFlag = 1;
    }else{
      row.slotImlockFlag = 0;
    }
  }
  // 出库锁定状态更改
  else if(name == "slotExlockFlag"){
    if(row.slotExlockFlag == 0){
      row.slotExlockFlag = 1;
    }else{
      row.slotExlockFlag = 0;
    }
  }
  // 是否屏蔽状态更改
  else if(name == "slotCloseFlag"){
    if(row.slotCloseFlag == 0){
      row.slotCloseFlag = 1;
    }else{
      row.slotCloseFlag = 0;
    }
  }
  await wmsBaseSlotApi.update(row);
};
// 导出数据
const exportWmsBaseSlotCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    if (command === 'select') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await wmsBaseSlotApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams);
      await wmsBaseSlotApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await wmsBaseSlotApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
}

handleQuery();
</script>
<template>
  <div class="wmsBaseSlot-container" v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }"> 
      <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="state.tableQueryParams.keyword" clearable placeholder="请输入模糊查询关键字"/>
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
            <el-form-item label="所属区域">
              <el-select clearable filterable v-model="state.tableQueryParams.slotAreaId" placeholder="请选择所属区域">
                <el-option v-for="(item,index) in state.dropdownData.slotAreaId ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="所属巷道">
              <el-select clearable filterable v-model="state.tableQueryParams.slotLanewayId" placeholder="请选择所属巷道">
                <el-option v-for="(item,index) in state.dropdownData.slotLanewayId ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="排">
              <el-input-number v-model="state.tableQueryParams.slotRow"  clearable placeholder="请输入排"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="列">
              <el-input-number v-model="state.tableQueryParams.slotColumn"  clearable placeholder="请输入列"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="层">
              <el-input-number v-model="state.tableQueryParams.slotLayer"  clearable placeholder="请输入层"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="储位深度">
              <g-sys-dict v-model="state.tableQueryParams.slotInout" code="SlotInout" render-as="select" placeholder="请选择储位深度" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="储位编码">
              <el-input v-model="state.tableQueryParams.slotCode" clearable placeholder="请输入储位编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="储位状态">
              <g-sys-dict v-model="state.tableQueryParams.slotStatus" code="SlotStatus" render-as="select" placeholder="请选择储位状态" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="库位类型">
              <g-sys-dict v-model="state.tableQueryParams.make" code="Make" render-as="select" placeholder="请选择库位类型" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="储位属性">
              <g-sys-dict v-model="state.tableQueryParams.property" code="Property" render-as="select" placeholder="请选择储位属性" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="储位处理">
              <g-sys-dict v-model="state.tableQueryParams.handle" code="Handle" render-as="select" placeholder="请选择储位处理" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="储位环境">
              <g-sys-dict v-model="state.tableQueryParams.environment" code="Environment" render-as="select" placeholder="请选择储位环境" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="入库锁定">
                  <el-select clearable filterable v-model="state.tableQueryParams.slotImlockFlag" placeholder="请选择入库锁定"> 
                    <el-option     value="1" label="锁定" /> 
                    <el-option     value="0" label="正常" />  
                  </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="出库锁定">
                  <el-select clearable filterable v-model="state.tableQueryParams.slotExlockFlag" placeholder="请选择出库锁定"> 
                    <el-option     value="1" label="锁定" /> 
                    <el-option     value="0" label="正常" />  
                  </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="是否屏蔽">
                  <el-select clearable filterable v-model="state.tableQueryParams.slotCloseFlag" placeholder="请选择是否屏蔽"> 
                    <el-option     value="1" label="屏蔽" /> 
                    <el-option     value="0" label="正常" />  
                  </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="备注">
              <el-input v-model="state.tableQueryParams.remark" clearable placeholder="请输入备注"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="储位高度">
              <el-input-number v-model="state.tableQueryParams.slotHigh"  clearable placeholder="请输入储位高度"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="限重">
              <el-input v-model="state.tableQueryParams.slotWeight" clearable placeholder="请输入限重"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="目的中转货位">
              <el-input v-model="state.tableQueryParams.endTransitLocation" clearable placeholder="请输入目的中转货位"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="四向车位置">
              <el-input-number v-model="state.tableQueryParams.isSxcLocation"  clearable placeholder="请输入四向车位置"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item >
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'wmsBaseSlot:page'" v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true" v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                <el-button icon="ele-ZoomOut" @click="() => state.showAdvanceQueryUI = false" v-if="state.showAdvanceQueryUI" style="margin-left:5px;"> 隐藏 </el-button>
                <el-button type="danger" style="margin-left:5px;" icon="ele-Delete" @click="batchDelWmsBaseSlot" :disabled="state.selectData.length == 0" v-auth="'wmsBaseSlot:batchDelete'"> 删除 </el-button>
                <el-button type="primary" style="margin-left:5px;" icon="ele-Plus" @click="editDialogRef.openDialog(null, '新增储位管理')" v-auth="'wmsBaseSlot:add'"> 新增 </el-button>
                <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsBaseSlotCommand">
                  <el-button type="primary" style="margin-left:5px;" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsBaseSlot:export'"> 导出 </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="select" :disabled="state.selectData.length == 0">导出选中</el-dropdown-item>
                      <el-dropdown-item command="current">导出本页</el-dropdown-item>
                      <el-dropdown-item command="all">导出全部</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
                <el-button type="warning" style="margin-left:5px;" icon="ele-MostlyCloudy" @click="importDataRef.openDialog()" v-auth="'wmsBaseSlot:import'"> 导入 </el-button>
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
        <el-table-column :fixed="true" type="selection" width="40" align="center" v-if="auth('wmsBaseSlot:batchDelete') || auth('wmsBaseSlot:export')" />
        <el-table-column :fixed="true" type="index" label="序号" width="55" align="center"/>
        <el-table-column :fixed="true" width="100" align="center" prop='warehouseId' label='所属仓库' :formatter="(row: any) => row.warehouseFkDisplayName" show-overflow-tooltip />
        <el-table-column width="100" align="center" prop='slotAreaId' label='所属区域' :formatter="(row: any) => row.slotAreaFkDisplayName" show-overflow-tooltip />
        <el-table-column width="100" align="center" prop='slotLanewayId' label='所属巷道' :formatter="(row: any) => row.slotLanewayFkDisplayName" show-overflow-tooltip />
        <el-table-column width="100" align="center" prop='slotRow' label='排' show-overflow-tooltip />
        <el-table-column width="100" align="center" prop='slotColumn' label='列' show-overflow-tooltip />
        <el-table-column width="100" align="center" prop='slotLayer' label='层' show-overflow-tooltip />
        <el-table-column width="200" align="center" prop='slotInout' label='储位深度' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.slotInout" code="SlotInout" />
          </template>
        </el-table-column>
        <el-table-column width="100" align="center" prop='slotCode' label='储位编码' show-overflow-tooltip />
        <el-table-column width="100" align="center" prop='slotStatus' label='储位状态' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.slotStatus" code="SlotStatus" />
          </template>
        </el-table-column>
        <el-table-column width="100" align="center" prop='make' label='库位类型' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.make" code="Make" />
          </template>
        </el-table-column>
        <el-table-column width="100" align="center" prop='property' label='储位属性' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.property" code="Property" />
          </template>
        </el-table-column>
        <el-table-column width="100" align="center" prop='handle' label='储位处理' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.handle" code="Handle" />
          </template>
        </el-table-column>
        <el-table-column width="100" align="center" prop='environment' label='储位环境' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.environment" code="Environment" />
          </template>
        </el-table-column>
        <el-table-column width="100" align="center" prop='slotImlockFlag' label='入库锁定' show-overflow-tooltip>
          <template #default="scope">
            <el-tag type="danger"  v-if="scope.row.slotImlockFlag"> 锁定 </el-tag>
            <el-tag v-else> 正常 </el-tag>
          </template>
        </el-table-column>
        <el-table-column width="100" align="center" prop='slotExlockFlag' label='出库锁定' show-overflow-tooltip>
          <template #default="scope">
            <el-tag type="danger" v-if="scope.row.slotExlockFlag"> 锁定 </el-tag>
            <el-tag v-else> 正常 </el-tag>
          </template>
        </el-table-column>
        <el-table-column width="100" align="center" prop='slotCloseFlag' label='是否屏蔽' show-overflow-tooltip>
          <template #default="scope">
            <el-tag type="danger" v-if="scope.row.slotCloseFlag"> 锁定 </el-tag>
            <el-tag v-else> 正常 </el-tag>
          </template>
        </el-table-column>
        <el-table-column width="100" align="center" label="修改记录" show-overflow-tooltip>
          <template #default="scope">
            <ModifyRecord :data="scope.row" />
          </template>
        </el-table-column>
        <el-table-column width="300" align="center" label="操作" fixed="right" show-overflow-tooltip v-if="auth('wmsBaseSlot:update') || auth('wmsBaseSlot:delete')">
          <template #default="scope">
            <el-button :type="scope.row.slotImlockFlag ? 'success' : 'danger'" :icon="scope.row.slotImlockFlag == 1 ? 'ele-CircleCheckFilled' : 'ele-CircleCloseFilled'" size="small" text type="primary" @click="changeStatus(scope.row,'slotImlockFlag')" v-auth="'wmsBaseLaneway:update'"> {{ scope.row.slotImlockFlag ? '入库解锁' : '入库锁定' }} </el-button>
            <el-button :type="scope.row.slotExlockFlag ? 'success' : 'danger'" :icon="scope.row.slotExlockFlag == 1 ? 'ele-CircleCheckFilled' : 'ele-CircleCloseFilled'" size="small" text type="primary" @click="changeStatus(scope.row,'slotExlockFlag')" v-auth="'wmsBaseLaneway:update'"> {{ scope.row.slotExlockFlag ? '出库解锁' : '出库锁定' }} </el-button>
            <el-button :type="scope.row.slotCloseFlag ? 'success' : 'danger'" :icon="scope.row.slotCloseFlag  == 1 ? 'ele-CircleCheckFilled' : 'ele-CircleCloseFilled'" size="small" text type="primary" @click="changeStatus(scope.row,'slotCloseFlag')" v-auth="'wmsBaseLaneway:update'"> {{ scope.row.slotCloseFlag ? '库位解锁' : '库位屏蔽' }} </el-button>
            <!-- <el-button icon="ele-Edit" size="small" text type="primary" @click="editDialogRef.openDialog(scope.row, '编辑储位管理')" v-auth="'wmsBaseSlot:update'"> 编辑 </el-button>
            <el-button icon="ele-Delete" size="small" text type="primary" @click="delWmsBaseSlot(scope.row)" v-auth="'wmsBaseSlot:delete'"> 删除 </el-button> -->
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
      <ImportData ref="importDataRef" :import="wmsBaseSlotApi.importData" :download="wmsBaseSlotApi.downloadTemplate" v-auth="'wmsBaseSlot:import'" @refresh="handleQuery"/>
      <printDialog ref="printDialogRef" :title="'打印储位管理'" @reloadTable="handleQuery" />
      <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
    </el-card>
  </div>
</template>
<style scoped>
:deep(.el-input), :deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>