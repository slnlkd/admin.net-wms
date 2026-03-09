<script lang="ts" setup name="wmsBaseMaterialPreset">
import { ref, reactive, onMounted } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useWmsBaseMaterialPresetApi } from '/@/api/base/baseMaterialPreset/wmsBaseMaterialPreset';
import editDialog from '/@/views/base/baseMaterialPreset/wmsBaseMaterialPreset/component/editDialog.vue'
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import ModifyRecord from '/@/components/table/modifyRecord.vue';
import ImportData from "/@/components/table/importData.vue";

const wmsBaseMaterialPresetApi = useWmsBaseMaterialPresetApi();
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
  const data = await wmsBaseMaterialPresetApi.getDropdownData(true).then(res => res.data.result) ?? {};
  state.dropdownData.materialUnit = data.materialUnit;
  state.dropdownData.materialAreaId = data.materialAreaId;
  state.dropdownData.warehouseId = data.warehouseId;
});

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsBaseMaterialPresetApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
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
const delWmsBaseMaterialPreset = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsBaseMaterialPresetApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => {});
};

// 审核物料信息
const approvalMaterial = (row: any, approvalStatus: any) => {
  ElMessageBox.confirm(`确定要${approvalStatus === 1 ? '审核通过' : '驳回'}吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsBaseMaterialPresetApi.approvalMaterial(row.id,approvalStatus);
    handleQuery();
    ElMessage.success("操作成功");
  }).catch(() => {});
};

// 批量删除
const batchDelWmsBaseMaterialPreset = () => {
  ElMessageBox.confirm(`确定要删除${state.selectData.length}条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsBaseMaterialPresetApi.batchDelete(state.selectData.map(u => ({ id: u.id }) )).then(res => {
      ElMessage.success(`成功批量删除${res.data.result}条记录`);
      handleQuery();
    });
  }).catch(() => {});
};

// 导出数据
const exportWmsBaseMaterialPresetCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    if (command === 'select') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await wmsBaseMaterialPresetApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams);
      await wmsBaseMaterialPresetApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await wmsBaseMaterialPresetApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
}

handleQuery();
</script>
<template>
  <div class="wmsBaseMaterialPreset-container" v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }"> 
      <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="state.tableQueryParams.keyword" clearable placeholder="请输入模糊查询关键字"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料编码">
              <el-input v-model="state.tableQueryParams.materialCode" clearable placeholder="请输入物料编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料类型">
              <g-sys-dict v-model="state.tableQueryParams.materialType" code="MaterialType" render-as="select" placeholder="请选择物料类型" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料名称">
              <el-input v-model="state.tableQueryParams.materialName" clearable placeholder="请输入物料名称"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料规格">
              <el-input v-model="state.tableQueryParams.materialStandard" clearable placeholder="请输入物料规格"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="计量单位">
              <el-select clearable filterable v-model="state.tableQueryParams.materialUnit" placeholder="请选择计量单位">
                <el-option v-for="(item,index) in state.dropdownData.materialUnit ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="专属区域">
              <el-select clearable filterable v-model="state.tableQueryParams.materialAreaId" placeholder="请选择专属区域">
                <el-option v-for="(item,index) in state.dropdownData.materialAreaId ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="箱数量">
              <el-input-number v-model="state.tableQueryParams.boxQuantity"  clearable placeholder="请输入箱数量"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料来源">
              <el-input v-model="state.tableQueryParams.materialOrigin" clearable placeholder="请输入物料来源"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="备注">
              <el-input v-model="state.tableQueryParams.remark" clearable placeholder="请输入备注"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="创建者姓名">
              <el-input v-model="state.tableQueryParams.createUserName" clearable placeholder="请输入创建者姓名"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="修改者姓名">
              <el-input v-model="state.tableQueryParams.updateUserName" clearable placeholder="请输入修改者姓名"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="每件托数">
              <el-input-number v-model="state.tableQueryParams.everyNumber"  clearable placeholder="请输入每件托数"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="载具">
              <g-sys-dict v-model="state.tableQueryParams.vehicle" code="Vehicle" render-as="select" placeholder="请选择载具" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料重量">
              <el-input-number v-model="state.tableQueryParams.materialWeight"  clearable placeholder="请输入物料重量"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="最高库存数量">
              <el-input-number v-model="state.tableQueryParams.materialStockHigh"  clearable placeholder="请输入最高库存数量"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="最低库存数量">
              <el-input-number v-model="state.tableQueryParams.materialStockLow"  clearable placeholder="请输入最低库存数量"/>
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
            <el-form-item label="提前预警天数">
              <el-input-number v-model="state.tableQueryParams.materialAlarmDay"  clearable placeholder="请输入提前预警天数"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="贴标">
              <g-sys-dict v-model="state.tableQueryParams.labeling" code="Labeling" render-as="select" placeholder="请选择贴标" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="管理方式">
              <g-sys-dict v-model="state.tableQueryParams.manageType" code="ManageType" render-as="select" placeholder="请选择管理方式" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="最低储备">
              <el-input-number v-model="state.tableQueryParams.mixReserve"  clearable placeholder="请输入最低储备"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="最高储备">
              <el-input-number v-model="state.tableQueryParams.maxReserve"  clearable placeholder="请输入最高储备"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="提前报警天数">
              <el-input-number v-model="state.tableQueryParams.alarmDay"  clearable placeholder="请输入提前报警天数"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="审核状态">
              <g-sys-dict v-model="state.tableQueryParams.approvalStatus" code="ApprovalStatus" render-as="select" placeholder="请选择审核状态" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="审核人名称">
              <el-input v-model="state.tableQueryParams.approvalUserName" clearable placeholder="请输入审核人名称"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item >
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'wmsBaseMaterialPreset:page'" v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true" v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                <el-button icon="ele-ZoomOut" @click="() => state.showAdvanceQueryUI = false" v-if="state.showAdvanceQueryUI" style="margin-left:5px;"> 隐藏 </el-button>
                <el-button type="danger" style="margin-left:5px;" icon="ele-Delete" @click="batchDelWmsBaseMaterialPreset" :disabled="state.selectData.length == 0" v-auth="'wmsBaseMaterialPreset:batchDelete'"> 删除 </el-button>
                <el-button type="primary" style="margin-left:5px;" icon="ele-Plus" @click="editDialogRef.openDialog(null, '新增申请物料信息')" v-auth="'wmsBaseMaterialPreset:add'"> 新增 </el-button>
                <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsBaseMaterialPresetCommand">
                  <el-button type="primary" style="margin-left:5px;" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsBaseMaterialPreset:export'"> 导出 </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="select" :disabled="state.selectData.length == 0">导出选中</el-dropdown-item>
                      <el-dropdown-item command="current">导出本页</el-dropdown-item>
                      <el-dropdown-item command="all">导出全部</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
                <!-- <el-button type="warning" style="margin-left:5px;" icon="ele-MostlyCloudy" @click="importDataRef.openDialog()" v-auth="'wmsBaseMaterialPreset:import'"> 导入 </el-button> -->
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
        <el-table-column type="selection" width="40" align="center" v-if="auth('wmsBaseSupplierPreset:batchDelete') || auth('wmsBaseSupplierPreset:export')" />
        <el-table-column :fixed="true" type="selection" width="40" align="center" v-if="auth('wmsBaseMaterial:batchDelete') || auth('wmsBaseMaterial:export')" />
        <el-table-column :fixed="true" type="index" label="序号" width="55" align="center"/>
        <el-table-column :fixed="true" align="center" width="155" prop='materialCode' label='物料编码' show-overflow-tooltip />
        <el-table-column :fixed="true" align="center" width="155" prop='materialType' label='物料类型' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.materialType" code="MaterialType" />
          </template>
        </el-table-column>
        <el-table-column align="center" width="200" prop='materialName' label='物料名称' show-overflow-tooltip />
        <el-table-column align="center" width="100" prop='materialStandard' label='物料规格' show-overflow-tooltip />
        <el-table-column align="center" prop='materialUnit' label='计量单位' :formatter="(row: any) => row.materialUnitFkDisplayName" show-overflow-tooltip />
        <el-table-column align="center" prop='materialAreaId' label='专属区域' :formatter="(row: any) => row.materialAreaFkDisplayName" show-overflow-tooltip />
        <el-table-column align="center" prop='boxQuantity' label='箱数量' show-overflow-tooltip />
        <el-table-column align="center" prop='materialOrigin' label='物料来源' show-overflow-tooltip />
        <el-table-column align="center" prop='remark' label='备注' show-overflow-tooltip />
        <el-table-column align="center" prop='createUserName' label='创建者姓名' show-overflow-tooltip />
        <el-table-column align="center" width="155" prop='createTime' label='创建时间' show-overflow-tooltip />
        <el-table-column align="center" prop='updateUserName' label='修改者姓名' show-overflow-tooltip />
        <el-table-column align="center" width="155" prop='updateTime' label='更新时间' show-overflow-tooltip />
        <el-table-column align="center" prop='everyNumber' label='每件托数' show-overflow-tooltip />
        <el-table-column align="center" prop='vehicle' label='载具' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.vehicle" code="Vehicle" />
          </template>
        </el-table-column>
        <el-table-column align="center" width="155" prop='warehouseId' label='所属仓库' :formatter="(row: any) => row.warehouseFkDisplayName" show-overflow-tooltip />
        <el-table-column align="center" width="155" prop='materialAlarmDay' label='提前预警天数' show-overflow-tooltip />
        <el-table-column align="center" width="155" prop='labeling' label='贴标' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.labeling" code="Labeling" />
          </template>
        </el-table-column>
        <el-table-column fixed="right" align="center" prop='approvalStatus' label='审核状态' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.approvalStatus" code="ApprovalStatus" />
          </template>
        </el-table-column>
        <el-table-column fixed="right" align="center" prop='approvalUserName' label='审核人名称' show-overflow-tooltip />
        <el-table-column fixed="right" align="center" prop='approvalRemark' label='驳回原因' show-overflow-tooltip />
        <el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
          <template #default="scope">
            <ModifyRecord :data="scope.row" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="240" align="center" fixed="right" show-overflow-tooltip>
          <template #default="scope">
            <el-button v-if="scope.row.approvalStatus != 1" icon="ele-Edit" size="small" text type="primary" @click="editDialogRef.openDialog(scope.row, '编辑申请物料信息')" v-auth="'wmsBaseMaterialPreset:update'"> 编辑 </el-button>
            <!-- 只有在待审核状态(0)时才显示操作按钮 -->
            <template v-if="auth('wmsBaseMaterialPreset:auth') && scope.row.approvalStatus === 0">
              <el-button icon="ele-Check" size="small" text type="success" @click="approvalMaterial(scope.row, 1)" v-auth="'wmsBaseMaterialPreset:auth'"> 通过 </el-button>
              <el-button icon="ele-Close" size="small" text type="danger" @click="approvalMaterial(scope.row, 2)" v-auth="'wmsBaseMaterialPreset:auth'"> 驳回 </el-button>
            </template>
            <el-button icon="ele-Delete" size="small" text type="primary" @click="delWmsBaseMaterialPreset(scope.row)" v-auth="'wmsBaseMaterialPreset:delete'"> 删除 </el-button>
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
      <ImportData ref="importDataRef" :import="wmsBaseMaterialPresetApi.importData" :download="wmsBaseMaterialPresetApi.downloadTemplate" v-auth="'wmsBaseMaterialPreset:import'" @refresh="handleQuery"/>
      <printDialog ref="printDialogRef" :title="'打印申请物料信息'" @reloadTable="handleQuery" />
      <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
    </el-card>
  </div>
</template>
<style scoped>
:deep(.el-input), :deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>