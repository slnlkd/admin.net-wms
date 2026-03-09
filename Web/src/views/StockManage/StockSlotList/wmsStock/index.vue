<script lang="ts" setup name="wmsStock">
import { ref, reactive, onMounted, watch, nextTick } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import type { TabsPaneContext } from 'element-plus'
import { useWmsStockTrayApi } from '/@/api/StockManage/StockSlotList/wmsStockTray';
import { useWmsBaseWareHouseApi } from '/@/api/base/baseWarehouse/wmsBaseWareHouse';
import { useWmsBaseLanewayApi } from '/@/api/base/baseLaneway/wmsBaseLaneway';
import { useWmsStockApi } from '/@/api/StockManage/StockSlotList/wmsStock';
import editDialog from '/@/views/StockManage/StockSlotList/wmsStock/component/editDialog.vue'
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import ModifyRecord from '/@/components/table/modifyRecord.vue';

interface StockDetailRow {
  MaterialCode: string;
  MaterialName: string;
  MaterialStandard: string;
  WarehouseName: string;
  WarehouseId: string;
  UnitName: string;
  LotNo: string;
  InspectionStatus: string;
  BaseMaterialCode: string;
  MaterialValidityDay1: string;
  CreateTime: string;
  StockQuantity: string;
  LockQuantity: string;
}

const wmsStockTrayApi = useWmsStockTrayApi();
const wmsBaseWareHouseApi = useWmsBaseWareHouseApi();
const wmsBaseLanewayApi = useWmsBaseLanewayApi();
const wmsStockApi = useWmsStockApi();
const printDialogRef = ref();
const editDialogRef = ref();
const state = reactive({
  exportLoading: false,
  tableLoading: false,
  tableDetailLoading: false,
  tableStockDetailLoading: false,
  stores: {},
  showAdvanceQueryUI: false,
  dropdownData: {} as any,
  selectData: [] as any[],
  tableDetailQueryParams: {} as any,
  tableQueryParams: {} as any,
  tableParams: {
    page: 1,
    pageSize: 10,
    total: 0,
    field: 'createTime', // 默认的排序字段
    order: 'descending', // 排序方向
    descStr: 'descending', // 降序排序的关键字符
  },
  tableStockDetailParams: {
    page: 1,
    pageSize: 20,
    total: 0,
    field: 'LotNo', // 默认的排序字段
    order: 'ascending', // 排序方向
    descStr: 'ascending', // 降序排序的关键字符
  },
  tableDetailParams: {
    page: 1,
    pageSize: 10,
    total: 0,
    field: 'createTime', // 默认的排序字段
    order: 'descending', // 排序方向
    descStr: 'descending', // 降序排序的关键字符
    MaterialCode: null,
    MaterialName: null,
    WarehouseName: null,
  },
  dropdownParams: {
    page: 1,
    pageSize: 200,
    total: 0,
    field: 'createTime', // 默认的排序字段
    order: 'descending', // 排序方向
    descStr: 'descending', // 降序排序的关键字符
  },
  dropdownQueryParams: {} as any,
  tableDetailData: [] as StockDetailRow[],
  selectDetailData: [] as StockDetailRow[],
  MaterialName: '',
  tableData: [],
  tableStockDetailData: [],
});

const activeName = ref('first')
const importTableRef = ref()

const handleClick = (tab: TabsPaneContext, event: Event) => {
  switch (tab.props.name) {
    case 'first':
      handleQuery()
      // tableRef.value.setCurrentRow()
      nextTick(() => {
        restoreImportTableSelection()
      })
      break
    case 'second':
      handleQueryStockDetail();
      // fetchData2()
      break
    case 'third':
      // fetchData3()
      break
  }
}

// 恢复入库表格的选中状态
const restoreImportTableSelection = () => {
  nextTick(() => {
    try {
      //importTableRef.value.setCurrentRow(tableSelection.importTable.selectedRowData)
    } catch (error) {
      console.error('恢复选中状态失败:', error)
    }
  })
}
// 页面加载时
onMounted(async () => {
  // 加载仓库下拉数据
  const warehousedata = await wmsBaseWareHouseApi.page(Object.assign(state.dropdownQueryParams, state.dropdownParams)).then(res => res.data.result);
  state.dropdownData.warehouseId = warehousedata.items;
  // 加载巷道下拉数据
  const lanewaydata = await wmsBaseLanewayApi.page(Object.assign(state.dropdownQueryParams, state.dropdownParams)).then(res => res.data.result);
  state.dropdownData.lanewayId = lanewaydata.items;
});

// 查询操作
const handleQueryOn = async (params: any = {}) => {
switch (activeName.value) {
  case 'first':
    handleQuery(params);
    break
  case 'second':
    handleQueryStockDetail(params);
    break
}
};
// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsStockApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
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
const delWmsStock = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsStockApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => { });
};

// 批量删除
const batchDelWmsStock = () => {
  ElMessageBox.confirm(`确定要删除${state.selectData.length}条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsStockApi.batchDelete(state.selectData.map(u => ({ id: u.id }))).then(res => {
      ElMessage.success(`成功批量删除${res.data.result}条记录`);
      handleQuery();
    });
  }).catch(() => { });
};
// 查询操作 明细
const handleDetailQuery = async (params: any = {}) => {
  state.tableDetailLoading = true;
  state.tableDetailParams = Object.assign(state.tableDetailParams, params);
  const result = await wmsStockApi.GetStockDetailsList(Object.assign(state.tableDetailQueryParams, state.tableDetailParams)).then(res => res.data.result);
  state.tableDetailParams.total = result?.total;
  state.tableDetailData = result?.items ?? [];
  state.tableDetailLoading = false;
};
// 打开页面
const handleImportNotify = (row: any) => {
  //state.tableDetailParams.MaterialCode = row.id;
  state.tableDetailParams.MaterialName = row.materialName;
  state.tableDetailParams.WarehouseName = row.warehouseName;
  state.MaterialName = row.materialName;
  handleDetailQuery();
};
// 列排序
const sortDetailChange = async (column: any) => {
  state.tableDetailParams.field = column.prop;
  state.tableDetailParams.order = column.order;
  await handleDetailQuery();
};
const handleDetailSelectionChange = (val: any[]) => {
  state.selectDetailData = val;
};
const handleDetailSizeChange = (val: number) => {
  handleDetailQuery({ pageSize: val });
};

const handleDetailCurrentChange = (val: number) => {
  handleDetailQuery({ page: val });
};

const handleQueryStock = async (row: any) => {
  state.tableStockDetailLoading = true;
  try {
    // 设置查询参数
    state.tableDetailParams.MaterialName = row.materialName;
    state.tableDetailParams.WarehouseName = row.warehouseName;
    
    // 切换到明细标签页
    activeName.value = 'second';
    
    // 调用明细查询函数
    await handleQueryStockDetail();
  } catch (error) {
    console.error('查询明细数据失败:', error);
    ElMessage.error('查询明细数据失败，请重试');
  } finally {
    state.tableStockDetailLoading = false;
  }
};
// 查询操作
const handleQueryStockDetail = async (params: any = {}) => {
  state.tableStockDetailLoading = true;
  state.tableStockDetailParams = Object.assign(state.tableStockDetailParams, params);
  const result = await wmsStockTrayApi.page(Object.assign(state.tableQueryParams, state.tableStockDetailParams)).then(res => res.data.result);
  state.tableStockDetailParams.total = result?.total;
  state.tableStockDetailData = result?.items ?? [];
  state.tableStockDetailLoading = false;
};

// 列排序
const sortChangeStockDetail = async (column: any) => {
  state.tableStockDetailParams.field = column.prop;
  state.tableStockDetailParams.order = column.order;
  await handleQueryStockDetail();
};

handleQuery();

</script>
<template>
  <div class="wmsStock-container" v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
      <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="state.tableQueryParams.keyword" clearable placeholder="请输入模糊查询关键字" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="所属仓库">
              <el-select clearable filterable v-model="state.tableQueryParams.warehouseId" placeholder="请选择所属仓库"
                style="width: 180px">
                <el-option v-for="(item, index) in state.dropdownData.warehouseId ?? []" :key="index" :value="item.id"
                  :label="item.warehouseName" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="所属巷道">
              <el-select clearable filterable v-model="state.tableQueryParams.slotLanewayId" placeholder="请选择所属巷道"
                style="width: 180px">
                <el-option v-for="(item, index) in state.dropdownData.lanewayId ?? []" :key="index" :value="item.id"
                  :label="item.lanewayName" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料编码">
              <el-input v-model="state.tableQueryParams.materialCode" clearable placeholder="请输入物料编码" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料名称">
              <el-input v-model="state.tableQueryParams.materialName" clearable placeholder="请输入物料名称" />
            </el-form-item>
          </el-col><el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
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
            <el-form-item label="入库日期">
              <el-date-picker type="daterange" v-model="state.tableQueryParams.stockDateRange"  value-format="YYYY-MM-DD HH:mm:ss" start-placeholder="开始日期" end-placeholder="结束日期" :default-time="[new Date('1 00:00:00'), new Date('1 23:59:59')]" />
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
            <el-form-item label="异常状态">
              <el-input-number v-model="state.tableQueryParams.abnormalStatu"  clearable placeholder="请输入异常状态"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item>
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary" icon="ele-Search" @click="handleQueryOn" v-auth="'wmsStock:page'"
                  v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true"
                  v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                <el-button icon="ele-ZoomOut" @click="() => state.showAdvanceQueryUI = false"
                  v-if="state.showAdvanceQueryUI" style="margin-left:5px;"> 隐藏 </el-button>
                <el-button type="danger" style="margin-left:5px;" icon="ele-Delete" @click="batchDelWmsStock"
                  :disabled="state.selectData.length == 0" v-auth="'wmsStock:batchDelete'"> 删除 </el-button>
                <el-button type="primary" style="margin-left:5px;" icon="ele-Plus"
                  @click="editDialogRef.openDialog(null, '新增库存查询')" v-auth="'wmsStock:add'"> 新增 </el-button>
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" shadow="hover"
      :body-style="{ height: '100%', display: 'flex', flexDirection: 'column' }" style="margin-top: 5px">
      <el-tabs v-model="activeName" @tab-click="handleClick">
        <el-tab-pane label="总量" name="first">
          <el-row :gutter="5" class="wmsStock-table">
            <el-col :span="24" :xs="24">
              <el-card class="full-table" shadow="hover"
                :body-style="{ height: '100%', display: 'flex', flexDirection: 'column' }">
                <div class="el-table-wrapper">
                  <el-table ref="importTableRef" :data="state.tableData"
                    @selection-change="(val: any[]) => { state.selectData = val; }" @row-click="handleImportNotify"
                    v-loading="state.tableLoading" tooltip-effect="light"
                    row-key="id" @sort-change="sortChange" border>
                    <el-table-column type="index" label="序号" width="100" align="center" />
                    <el-table-column prop='warehouseName' label='所属仓库' show-overflow-tooltip align="center" />
                    <el-table-column prop='materialName' label='物料名称' show-overflow-tooltip align="center" />
                    <el-table-column prop='stockQuantity' label='库存数量' show-overflow-tooltip align="center" />
                    <el-table-column prop='lockQuantity' label='锁定数量' show-overflow-tooltip align="center" />
                  </el-table>
                </div>
                <el-pagination v-model:currentPage="state.tableParams.page"
                  v-model:page-size="state.tableParams.pageSize"
                  @size-change="(val: any) => handleQuery({ pageSize: val })"
                  @current-change="(val: any) => handleQuery({ page: val })"
                  layout="total, sizes, prev, pager, next, jumper" :page-sizes="[10, 20, 50, 100, 200, 500]"
                  :total="state.tableParams.total" size="small" background />
              </el-card>
            </el-col>
          </el-row>
          <el-row :gutter="5" class="wmsStock-detail">
            <el-col :span="24" :xs="24">
              <el-card class="full-table" shadow="hover"
                :body-style="{ height: '100%', display: 'flex', flexDirection: 'column' }">
                <template #header>
                  <div style="display: flex; justify-content: space-between; align-items: center;">
                    <div style="display: flex; align-items: center;">
                      <el-icon size="16"
                        style="margin-right: 3px; display: inline; vertical-align: middle"><ele-Collection /></el-icon>
                      <span>物料名称【{{ state.MaterialName }}】</span>
                    </div>
                    <!-- <div style="display: flex; gap: 5px; align-items: center;">
                    <el-select clearable filterable v-model="state.tableDetailQueryParams.warehouseId"
                      placeholder="请选择所属仓库" style="width: 180px" size="small" @keyup.enter="handleDetailQuery"
                      @clear="handleDetailQuery">
                      <el-option v-for="(item, index) in state.dropdownData.warehouseId ?? []" :key="index"
                        :value="item.id" :label="item.warehouseName" />
                    </el-select>
                    <el-input v-model="state.tableDetailQueryParams.materialCode" placeholder="物料编码"
                      style="width: 150px;" size="small" clearable @keyup.enter="handleDetailQuery"
                      @clear="handleDetailQuery" />
                    <el-input v-model="state.tableDetailQueryParams.materialName" placeholder="物料名称"
                      style="width: 150px;" size="small" clearable @keyup.enter="handleDetailQuery"
                      @clear="handleDetailQuery" />
                    <el-button type="primary" icon="ele-Search" @click="handleDetailQuery" size="small" circle />
                  </div> -->
                  </div>
                </template>
                <div class="el-table-wrapper">
                  <el-table :data="state.tableDetailData" @selection-change="handleDetailSelectionChange" @row-dblclick="handleQueryStock"
                    v-loading="state.tableDetailLoading" tooltip-effect="light" row-key="id"
                    @sort-change="sortDetailChange" border>
                    <el-table-column type="index" label="序号" align="center" width="100" />
                    <el-table-column prop='warehouseName' label='所属仓库' align="center" sortable='custom'
                      show-overflow-tooltip />
                    <el-table-column prop='materialCode' label='物料编码' align="center" sortable='custom'
                      show-overflow-tooltip />
                    <el-table-column prop='materialName' label='物料名称' align="center" show-overflow-tooltip />
                    <el-table-column prop='materialStandard' label='物料规格' align="center" show-overflow-tooltip />
                    <el-table-column prop='lotNo' label='批次' align="center" sortable='custom' show-overflow-tooltip />
                    <el-table-column prop='unitName' align="center" width="80" label='计量单位' show-overflow-tooltip />
                    <el-table-column prop='InspectionStatus' align="center" sortable='custom' label='质检状态'>
                      <template #default="scope">
                        <g-sys-dict v-model="scope.row.inspectionStatus" code="QualityInspectionStatus" />
                      </template>
                    </el-table-column>
                    <el-table-column prop='stockQuantity' align="center" label='库存数量' show-overflow-tooltip />
                    <el-table-column prop='lockQuantity' align="center" label='锁定数量' show-overflow-tooltip />
                  </el-table>
                </div>
                <el-pagination v-model:current-page="state.tableDetailParams.page"
                  v-model:page-size="state.tableDetailParams.pageSize" @size-change="handleDetailSizeChange"
                  @current-change="handleDetailCurrentChange" layout="total, sizes, prev, pager, next, jumper"
                  :page-sizes="[10, 20, 50, 100, 200, 500]" :total="state.tableDetailParams.total" size="small"
                  background />
              </el-card>
            </el-col>
          </el-row>
        </el-tab-pane>
        <el-tab-pane label="明细" name="second">
          <el-card class="full-table" shadow="hover" style="margin-top: 5px; height: 100%;" :body-style="{ height: '100%', display: 'flex', flexDirection: 'column' }">
            <div class="el-table-wrapper">
              <el-table :data="state.tableStockDetailData" @selection-change="(val: any[]) => { state.selectData = val; }"
                v-loading="state.tableStockDetailLoading" tooltip-effect="light" row-key="id"
                @sort-change="sortChangeStockDetail" border>
              <el-table-column type="selection" width="40" align="center"
                v-if="auth('wmsStockTray:batchDelete') || auth('wmsStockTray:export')" />
              <el-table-column type="index" label="序号" width="55" align="center" />
              <el-table-column prop='stockSlotCode' label='储位位置' show-overflow-tooltip />
              <el-table-column prop='stockCode' label='托盘编码' show-overflow-tooltip />
              <el-table-column prop='stockQuantity' label='库存数量' show-overflow-tooltip />
              <el-table-column prop='stockDate' label='入库日期' show-overflow-tooltip />
              <el-table-column prop='materialId' label='物品信息' :formatter="(row: any) => row.materialFkDisplayName"
                show-overflow-tooltip />
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
              <el-table-column prop='abnormalStatu' label='冻结状态' show-overflow-tooltip />
              <el-table-column prop='productionDate' label='生产日期' show-overflow-tooltip />
              <el-table-column prop='validateDay' label='失效日期' show-overflow-tooltip />
              <el-table-column prop='outQty' label='件数' show-overflow-tooltip />
              <el-table-column prop='boxQuantity' label='物料箱数量' show-overflow-tooltip />
              <el-table-column prop='oddMarking' label='零头标识' show-overflow-tooltip />
              <el-table-column prop='revisionDate' label='改判日期' show-overflow-tooltip />
              <el-table-column prop='releaseDate' label='放行日期' show-overflow-tooltip />
              <!-- <el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
                <template #default="scope">
                  <ModifyRecord :data="scope.row" />
                </template>
              </el-table-column> -->
              <el-table-column label="操作" width="100" align="center" fixed="right" show-overflow-tooltip
                v-if="auth('wmsStockTray:update') || auth('wmsStockTray:delete')">
                <template #default="scope">
                  <el-button icon="ele-Edit" size="small" text type="primary"
                    @click="editDialogRef.openDialog(scope.row, '编辑库存明细查询')" v-auth="'wmsStockTray:update'"> 编辑
                  </el-button>
                  <!-- <el-button icon="ele-Delete" size="small" text type="primary" @click="delWmsStockTray(scope.row)"
                    v-auth="'wmsStockTray:delete'"> 删除 </el-button> -->
                </template>
              </el-table-column>
            </el-table>
            </div>
            <el-pagination v-model:currentPage="state.tableStockDetailParams.page" v-model:page-size="state.tableStockDetailParams.pageSize"
              @size-change="(val: any) => handleQueryStockDetail({ pageSize: val })"
              @current-change="(val: any) => handleQueryStockDetail({ page: val })"
              layout="total, sizes, prev, pager, next, jumper" :page-sizes="[10, 20, 50, 100, 200, 500]"
              :total="state.tableStockDetailParams.total" size="small" background />
          </el-card>
        </el-tab-pane>
      </el-tabs>
    </el-card>
  </div>
</template>
<style lang="scss" scoped>
:deep(.el-input),
:deep(.el-select),
:deep(.el-input-number) {
  width: 100%;
}

.wmsStock-container {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 100px); // Adjust based on your layout header/tabs height
  padding: 5px;
  box-sizing: border-box;
  overflow: hidden; // Prevent container from scrolling
}

.wmsStock-table {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0; // Crucial for nested flex scrolling
  margin-bottom: 5px; // Spacing between top and bottom sections
  max-height: 50%; // Limit height to ensure both tables are visible

  :deep(.el-col) {
    height: 100%;
    display: flex;
    flex-direction: column;
  }
}

.wmsStock-detail {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0; // Crucial for nested flex scrolling
  max-height: 55%; // Limit height to ensure both tables are visible

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
    min-height: 0;
    position: relative; // For proper positioning
    /* allow flex children to shrink properly so table header is visible */

    .el-table-wrapper {
      flex: 1;
      min-height: 0;
      overflow: auto;
      border: 1px solid var(--el-border-color-lighter);
      border-radius: 4px;
      display: flex;
      flex-direction: column;
    }

    .el-table {
      flex: 1;
      min-height: 0;
      height: 100%;
    }

    .el-pagination {
      margin-top: 10px;
      padding: 5px 0;
      flex-shrink: 0;
    }
  }
}

// Optimize tabs container
:deep(.el-tabs) {
  height: 100%;
  display: flex;
  flex-direction: column;

  .el-tabs__content {
    flex: 1;
    min-height: 0;
    overflow: hidden;
  }

  .el-tab-pane {
    height: 100%;
    display: flex;
    flex-direction: column;
  }
}

// Ensure table wrapper works correctly
.el-table-wrapper {
  flex: 1;
  min-height: 0;
  overflow: auto;
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 4px;
  display: flex;
  flex-direction: column;
}
</style>