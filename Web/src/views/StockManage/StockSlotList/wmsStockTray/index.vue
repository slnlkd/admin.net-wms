<script lang="ts" setup name="wmsStockTray">
import { ref, reactive, onMounted } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useWmsStockTrayApi } from '/@/api/StockManage/StockSlotList/wmsStockTray';
import editDialog from '/@/views/StockManage/StockSlotList/wmsStockTray/component/editDialog.vue'
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import ModifyRecord from '/@/components/table/modifyRecord.vue';

const wmsStockTrayApi = useWmsStockTrayApi();
const printDialogRef = ref();
const editDialogRef = ref();
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
  const data = await wmsStockTrayApi.getDropdownData(true).then(res => res.data.result) ?? {};
  state.dropdownData.materialId = data.materialId;
});

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsStockTrayApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
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
const delWmsStockTray = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsStockTrayApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => {});
};

// 批量删除
const batchDelWmsStockTray = () => {
  ElMessageBox.confirm(`确定要删除${state.selectData.length}条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsStockTrayApi.batchDelete(state.selectData.map(u => ({ id: u.id }) )).then(res => {
      ElMessage.success(`成功批量删除${res.data.result}条记录`);
      handleQuery();
    });
  }).catch(() => {});
};

handleQuery();
</script>
<template>
  <div class="wmsStockTray-container" v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }"> 
      <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="state.tableQueryParams.keyword" clearable placeholder="请输入模糊查询关键字"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="储位位置">
              <el-input v-model="state.tableQueryParams.stockSlotCode" clearable placeholder="请输入储位位置"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="托盘编码">
              <el-input v-model="state.tableQueryParams.stockCode" clearable placeholder="请输入托盘编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="库存日期">
              <el-date-picker type="daterange" v-model="state.tableQueryParams.stockDateRange"  value-format="YYYY-MM-DD HH:mm:ss" start-placeholder="开始日期" end-placeholder="结束日期" :default-time="[new Date('1 00:00:00'), new Date('1 23:59:59')]" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物品编码">
              <el-select clearable filterable v-model="state.tableQueryParams.materialId" placeholder="请选择物品编码">
                <el-option v-for="(item,index) in state.dropdownData.materialId ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="库存状态">
              <g-sys-dict v-model="state.tableQueryParams.stockStatusFlag" code="SlotStatus" render-as="select" placeholder="请选择库存状态" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="库存批次">
              <el-input v-model="state.tableQueryParams.lotNo" clearable placeholder="请输入库存批次"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="质检状态">
              <g-sys-dict v-model="state.tableQueryParams.inspectionStatus" code="QualityInspectionStatus" render-as="select" placeholder="请选择质检状态" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="巷道ID">
              <el-input-number v-model="state.tableQueryParams.lanewayId"  clearable placeholder="请输入巷道ID"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="仓库ID">
              <el-input-number v-model="state.tableQueryParams.warehouseId"  clearable placeholder="请输入仓库ID"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="异常状态">
              <el-input-number v-model="state.tableQueryParams.abnormalStatu"  clearable placeholder="请输入异常状态"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item >
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'wmsStockTray:page'" v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true" v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                <el-button icon="ele-ZoomOut" @click="() => state.showAdvanceQueryUI = false" v-if="state.showAdvanceQueryUI" style="margin-left:5px;"> 隐藏 </el-button>
                <el-button type="danger" style="margin-left:5px;" icon="ele-Delete" @click="batchDelWmsStockTray" :disabled="state.selectData.length == 0" v-auth="'wmsStockTray:batchDelete'"> 删除 </el-button>
                <el-button type="primary" style="margin-left:5px;" icon="ele-Plus" @click="editDialogRef.openDialog(null, '新增库存明细查询')" v-auth="'wmsStockTray:add'"> 新增 </el-button>
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
        <el-table-column type="selection" width="40" align="center" v-if="auth('wmsStockTray:batchDelete') || auth('wmsStockTray:export')" />
        <el-table-column type="index" label="序号" width="55" align="center"/>
        <el-table-column prop='stockSlotCode' label='储位位置' show-overflow-tooltip />
        <el-table-column prop='stockCode' label='托盘编码' show-overflow-tooltip />
        <el-table-column prop='stockQuantity' label='库存数量' show-overflow-tooltip />
        <el-table-column prop='stockDate' label='库存日期' show-overflow-tooltip />
        <el-table-column prop='materialId' label='物品编码' :formatter="(row: any) => row.materialFkDisplayName" show-overflow-tooltip />
        <el-table-column prop='stockStatusFlag' label='库存状态' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.stockStatusFlag" code="SlotStatus" />
          </template>
        </el-table-column>
        <el-table-column prop='lotNo' label='库存批次' show-overflow-tooltip />
        <el-table-column prop='inspectionStatus' label='质检状态' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.inspectionStatus" code="QualityInspectionStatus" />
          </template>
        </el-table-column>
        <el-table-column prop='lanewayId' label='巷道ID' show-overflow-tooltip />
        <el-table-column prop='warehouseId' label='仓库ID' show-overflow-tooltip />
        <el-table-column prop='isSamolingTray' label='抽检托' show-overflow-tooltip />
        <el-table-column prop='lockQuantity' label='锁定数量' show-overflow-tooltip />
        <el-table-column prop='abnormalStatu' label='异常状态' show-overflow-tooltip />
        <el-table-column prop='productionDate' label='生产日期' show-overflow-tooltip />
        <el-table-column prop='validateDay' label='失效日期' show-overflow-tooltip />
        <el-table-column prop='outQty' label='件数' show-overflow-tooltip />
        <el-table-column prop='boxQuantity' label='物料箱数量' show-overflow-tooltip />
        <el-table-column prop='oddMarking' label='零头标识' show-overflow-tooltip />
        <el-table-column prop='revisionDate' label='改判日期' show-overflow-tooltip />
        <el-table-column prop='releaseDate' label='改判日期' show-overflow-tooltip />
        <el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
          <template #default="scope">
            <ModifyRecord :data="scope.row" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip v-if="auth('wmsStockTray:update') || auth('wmsStockTray:delete')">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text type="primary" @click="editDialogRef.openDialog(scope.row, '编辑库存明细查询')" v-auth="'wmsStockTray:update'"> 编辑 </el-button>
            <el-button icon="ele-Delete" size="small" text type="primary" @click="delWmsStockTray(scope.row)" v-auth="'wmsStockTray:delete'"> 删除 </el-button>
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
      <printDialog ref="printDialogRef" :title="'打印库存明细查询'" @reloadTable="handleQuery" />
      <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
    </el-card>
  </div>
</template>
<style scoped>
:deep(.el-input), :deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>