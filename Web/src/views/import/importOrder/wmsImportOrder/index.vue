<template>
  <div class="import-order-container">
    <el-row :gutter="5" class="import-order-table">
      <el-col :span="24" :xs="24">
        <el-card class="full-table" shadow="hover" :body-style="{ height: '100%', display: 'flex', flexDirection: 'column' }">
          <template #header>
            <el-icon size="16"
              style="margin-right: 3px; display: inline; vertical-align: middle"><ele-Collection /></el-icon>入库流水
          </template>
          <el-form :model="state.tableQueryParams" ref="queryForm" label-width="auto"> 
            <el-row :gutter="5">
              <el-col :xs="24" :sm="12" :md="8" :lg="3" :xl="3" class="mb5">
                <el-form-item label="载具条码">
                  <el-input v-model="state.tableQueryParams.stockCode" clearable placeholder="载具条码" @keyup.enter="handleQuery" />
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="8" :lg="3" :xl="3" class="mb5">
                <el-form-item label="储位地址">
                  <el-input v-model="state.tableQueryParams.importSlotCode" clearable placeholder="储位地址" @keyup.enter="handleQuery" />
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="8" :lg="3" :xl="3" class="mb5">
                <el-form-item label="执行状态">
                  <g-sys-dict v-model="state.tableQueryParams.importExecuteFlag" code="ExecuteFlag" render-as="select"
                    placeholder="执行状态" clearable filterable />
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="12" :lg="5" :xl="5" class="mb5">
                <el-form-item label="完成时间">
                  <div style="display: flex; gap: 2px; width: 100%;">
                    <el-date-picker type="date" v-model="state.tableQueryParams.completeTimeStart" value-format="YYYY-MM-DD"
                      placeholder="开始时间" style="flex: 1;" :prefix-icon="null" />
                    <span style="line-height: 32px;">-</span>
                    <el-date-picker type="date" v-model="state.tableQueryParams.completeTimeEnd" value-format="YYYY-MM-DD"
                      placeholder="结束时间" style="flex: 1;" :prefix-icon="null" />
                  </div>
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="12" :lg="5" :xl="5" class="mb5">
                <el-form-item label="创建时间">
                  <div style="display: flex; gap: 2px; width: 100%;">
                    <el-date-picker type="date" v-model="state.tableQueryParams.createTimeStart" value-format="YYYY-MM-DD"
                      placeholder="开始时间" style="flex: 1;" :prefix-icon="null" />
                    <span style="line-height: 32px;">-</span>
                    <el-date-picker type="date" v-model="state.tableQueryParams.createTimeEnd" value-format="YYYY-MM-DD"
                      placeholder="结束时间" style="flex: 1;" :prefix-icon="null" />
                  </div>
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="12" :lg="5" :xl="5" class="mb5">
                <el-form-item label-width="0px" style="display: flex; white-space: nowrap;">
                  <el-button type="primary" icon="ele-Search" @click="handleQuery" v-auth="'wmsImportOrder:page'" v-reclick="1000">查询</el-button>
                  <el-button icon="ele-Refresh" @click="handleReset" style="margin-left: 5px">重置</el-button>
                  <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportwmsImportOrderCommand" style="margin-left: 5px">
                    <el-button type="primary" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsImportOrder:export'">
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
          <el-table :data="state.tableData" @selection-change="handleSelectionChange" style="width: 100%; flex: 1; height: 0;"
            v-loading="state.tableLoading" @row-click="handleImportOrder" tooltip-effect="light" row-key="id"
            @sort-change="sortChange" border>
            <el-table-column type="selection" width="40" align="center"
              v-if="auth('wmsImportOrder:batchDelete') || auth('wmsImportOrder:export')" />
            <el-table-column type="index" :fixed="true" label="序号" width="55" align="center" />
            <el-table-column prop='importOrderNo' :fixed="true" sortable='custom' label='入库流水号' align="center"
              show-overflow-tooltip />
            <el-table-column prop='stockCode' label='载具条码' sortable='custom' align="center" show-overflow-tooltip />
            <el-table-column prop='importSlotCode' label='储位地址' sortable='custom' align="center"
              show-overflow-tooltip />
            <el-table-column prop='importExecuteFlag' label='执行状态' sortable='custom' align="center" width="120"
              show-overflow-tooltip>
              <template #default="scope">
                <g-sys-dict v-model="scope.row.importExecuteFlag" code="ExecuteFlag" />
              </template>
            </el-table-column>
            <el-table-column prop='completeDate' label='完成时间' sortable='custom' align="center" show-overflow-tooltip />
            <el-table-column prop='createUserName' label='创建人' align="center" show-overflow-tooltip />
            <el-table-column prop='createTime' label='创建时间' sortable='custom' align="center" show-overflow-tooltip />
            <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip
              v-if="auth('wmsImportOrder:update') || auth('wmsImportOrder:delete')">
              <template #default="scope">
                <el-button type="primary" v-if="scope.row.importExecuteFlag == '01'" icon="ele-Edit" size="small" text
                  @click="editDialogRef.openDialog(scope.row, '指定储位')" v-auth="'wmsImportOrder:update'">指定储位</el-button>
                <el-button type="primary" v-if="scope.row.importExecuteFlag == '01'" icon="ele-Edit" size="small" text
                  @click="delwmsImportOrder(scope.row)" v-auth="'wmsImportOrder:update'">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-pagination v-model:currentPage="state.tableParams.page" v-model:page-size="state.tableParams.pageSize"
            @size-change="handleSizeChange" @current-change="handleCurrentChange"
            layout="total, sizes, prev, pager, next, jumper" :page-sizes="[10, 20, 50, 100, 200, 500]"
            :total="state.tableParams.total" size="small" background />
          <ImportData ref="importDataRef" :import="wmsImportOrderApi.importData"
            :download="wmsImportOrderApi.downloadTemplate" v-auth="'wmsImportOrder:import'" @refresh="handleQuery" />
          <printDialog ref="printDialogRef" :title="'打印入库流水'" @reloadTable="handleQuery" />
          <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
        </el-card>
      </el-col>
    </el-row>
    <el-row :gutter="5" class="import-order-detail">
      <el-col :span="24" :xs="24">
        <el-card class="full-table" shadow="hover" :body-style="{ height: '100%', display: 'flex', flexDirection: 'column' }">
          <template #header>
            <div style="display: flex; justify-content: space-between; align-items: center;">
              <div style="display: flex; align-items: center;">
                <el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"><ele-Collection /></el-icon>
                <span>入库流水明细【{{ state.importOrderNo }}】</span>
              </div>
              <div style="display: flex; gap: 5px; align-items: center;">
                <el-input v-model="state.tableDetailQueryParams.boxCode" placeholder="箱码编号" style="width: 150px;" size="small" clearable @keyup.enter="handleDetailQuery" @clear="handleDetailQuery" />
                <el-input v-model="state.tableDetailQueryParams.materialCode" placeholder="物料编码" style="width: 150px;" size="small" clearable @keyup.enter="handleDetailQuery" @clear="handleDetailQuery" />
                <el-input v-model="state.tableDetailQueryParams.materialName" placeholder="物料名称" style="width: 150px;" size="small" clearable @keyup.enter="handleDetailQuery" @clear="handleDetailQuery" />
                <el-input v-model="state.tableDetailQueryParams.lotNo" placeholder="批次" style="width: 150px;" size="small" clearable @keyup.enter="handleDetailQuery" @clear="handleDetailQuery" />
                <div style="display: flex; gap: 3px;">
                  <el-button type="primary" icon="ele-Search" @click="handleDetailQuery" size="small" circle plain />
                  <el-button icon="ele-Refresh" @click="handleDetailReset" circle plain style="margin-left: 0"></el-button>
                </div>
              </div>
            </div>
          </template>
          <el-table :data="state.tableDetailData" @selection-change="handleDetailSelectionChange" style="width: 100%; flex: 1; height: 0;"
            v-loading="state.tableDetailLoading" tooltip-effect="light" row-key="id" @sort-change="sortDetailChange"
            border>
            <el-table-column type="selection" width="40" align="center"
              v-if="auth('wmsImportOrderDetail:batchDelete') || auth('wmsImportOrderDetail:export')" />
            <el-table-column type="index" label="序号" width="55" align="center" />
            <el-table-column prop='importOrderNo' label='入库流水号' align="center" show-overflow-tooltip />
            <el-table-column prop='boxCode' label='箱码编号' align="center" show-overflow-tooltip />
            <el-table-column prop='materialCode' label='物料编码' align="center" width="100" show-overflow-tooltip />
            <el-table-column prop='materialName' label='物料名称' align="center" show-overflow-tooltip />
            <el-table-column prop='materialStandard' label='物料规格' align="center" show-overflow-tooltip />
            <el-table-column prop='lotNo' label='批次' align="center" sortable='custom' width="65"
              show-overflow-tooltip />
            <el-table-column prop='qty' label='实际数量' align="center" width="100" show-overflow-tooltip />
            <el-table-column prop='bulkTank' label='零箱标识' align="center" width="100" show-overflow-tooltip>
              <template #default="scope">
                <g-sys-dict v-model="scope.row.bulkTank" code="BulkTank" />
              </template>
            </el-table-column>
            <el-table-column prop='status' align="center" width="100" label='状态' show-overflow-tooltip>
              <template #default="scope">
                <g-sys-dict v-model="scope.row.status" code="OrderStatus" />
              </template>
            </el-table-column>
            <el-table-column prop='productionDate' align="center" width="120" label='生产日期'
              show-overflow-tooltip />
            <el-table-column prop='validateDay' align="center" width="120" label='有效期至' show-overflow-tooltip />
            <el-table-column label="操作" width="100" align="center" fixed="right" show-overflow-tooltip>
              <!-- <template #default="scope">
                <el-button type="primary" v-if="scope.row.importExecuteFlag == '01'" icon="ele-Edit" size="small" text
                  @click="editDialogRef.openDialog(scope.row, '编辑')" v-auth="'wmsImportOrder:update'">编辑</el-button>
                <el-button icon="ele-CircleCloseFilled" v-if="scope.row.importExecuteFlag == '01'" size="small" text
                  type="danger"> 删除
                </el-button>
              </template> -->
            </el-table-column>
          </el-table>
          <el-pagination v-model:currentPage="state.tableDetailParams.page"
            v-model:page-size="state.tableDetailParams.pageSize" @size-change="handleDetailSizeChange"
            @current-change="handleDetailCurrentChange" layout="total, sizes, prev, pager, next, jumper"
            :page-sizes="[10, 20, 50, 100, 200, 500]" :total="state.tableDetailParams.total" size="small" background />
        </el-card>
      </el-col>
    </el-row>

  </div>
</template>

<script lang="ts" setup name="wmsImportOrder">
import { ref, reactive, onMounted } from "vue";
import { dayjs } from 'element-plus';
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useWmsImportOrderApi } from '/@/api/import/importOrder/wmsImportOrder';
import { usewmsStockSlotBoxInfoApi } from '/@/api/stock/stockSlotBoxInfo/wmsStockSlotBoxInfo';
import EditDialog from '/@/views/import/importOrder/wmsImportOrder/component/editDialog.vue'
// import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
// import ModifyRecord from '/@/components/table/modifyRecord.vue';
// import ImportData from "/@/components/table/importData.vue";

const wmsImportOrderApi = useWmsImportOrderApi();
const wmsStockSlotBoxInfoApi = usewmsStockSlotBoxInfoApi();
const printDialogRef = ref();
const editDialogRef = ref();
const importDataRef = ref();
const state = reactive({
  exportLoading: false,
  tableLoading: false,
  tableDetailLoading: false,
  stores: {},
  showAdvanceQueryUI: false,
  selectData: [] as any[],
  selectDetailData: [] as any[],
  tableQueryParams: {} as any,
  tableDetailQueryParams: {} as any,
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
    importOrderId: 0
  },
  tableBillTypeParams: {
    page: 1,
    pageSize: 200,
    total: 0,
    field: 'createTime', // 默认的排序字段
    order: 'descending', // 排序方向
    descStr: 'descending', // 降序排序的关键字符
  },
  tableBillTypeQueryParams: {} as any,
  tableData: [],
  tableDetailData: [],
  importOrderNo: ''
});


const handleSizeChange = (val: number) => {
  handleQuery({ pageSize: val });
};

const handleCurrentChange = (val: number) => {
  handleQuery({ page: val });
};
const handleDetailSizeChange = (val: number) => {
  handleDetailQuery({ pageSize: val });
};

const handleDetailCurrentChange = (val: number) => {
  handleDetailQuery({ page: val });
};
const handleSelectionChange = (val: any[]) => {
  state.selectData = val;
};
const handleDetailSelectionChange = (val: any[]) => {
  state.selectDetailData = val;
};
// 页面加载时
onMounted(async () => {
  const dateRange = getDateRange(3, false)
  // state.tableQueryParams.completeTimeStart = dateRange[0]
  // state.tableQueryParams.completeTimeEnd = dateRange[1]
  state.tableQueryParams.createTimeStart = dateRange[0]
  state.tableQueryParams.createTimeEnd = dateRange[1]
  handleQuery();
});

// 处理日期格式转换：将两个独立的日期转换为 completeDateRange 数组格式
const formatDateParams = (params: any) => {
  const formattedParams = { ...params };
  // 如果存在 completeTimeStart 或 completeTimeEnd，转换为 completeDateRange 数组
  if (formattedParams.completeTimeStart || formattedParams.completeTimeEnd) {
    const startTime = formattedParams.completeTimeStart
      ? formattedParams.completeTimeStart + ' 00:00:00'
      : null;
    const endTime = formattedParams.completeTimeEnd
      ? formattedParams.completeTimeEnd + ' 23:59:59'
      : null;

    if (startTime && endTime) {
      formattedParams.completeDateRange = [startTime, endTime];
    } else if (startTime) {
      formattedParams.completeDateRange = [startTime, null];
    } else if (endTime) {
      formattedParams.completeDateRange = [null, endTime];
    }

    // 删除独立的日期参数，避免混淆
    delete formattedParams.completeTimeStart;
    delete formattedParams.completeTimeEnd;
  }
    // 如果存在 createTimeStart 或 createTimeEnd，转换为 createTimeRange 数组
    if (formattedParams.createTimeStart || formattedParams.createTimeEnd) {
    const startTime = formattedParams.createTimeStart
      ? formattedParams.createTimeStart + ' 00:00:00'
      : null;
    const endTime = formattedParams.createTimeEnd
      ? formattedParams.createTimeEnd + ' 23:59:59'
      : null;

    if (startTime && endTime) {
      formattedParams.createTimeRange = [startTime, endTime];
    } else if (startTime) {
      formattedParams.createTimeRange = [startTime, null];
    } else if (endTime) {
      formattedParams.createTimeRange = [null, endTime];
    }

    // 删除独立的日期参数，避免混淆
    delete formattedParams.createTimeStart;
    delete formattedParams.createTimeEnd;
  }
  return formattedParams;
};

// 查询操作
const handleQuery = async (params: any = {}) => {
  try {
    state.tableLoading = true;
    if (params.page === undefined) {
      state.tableParams.page = 1;
    }
    state.tableParams = Object.assign(state.tableParams, params);
    const queryParams = formatDateParams(state.tableQueryParams);
    const result = await wmsImportOrderApi.page(Object.assign(queryParams, state.tableParams)).then(res => res.data.result);
    state.tableParams.total = result?.total;
    state.tableData = result?.items ?? [];
  } catch (error) {
    console.error('查询入库流水失败:', error);
  } finally {
    state.tableLoading = false;
  }
};
//重置
const handleReset = () => {
  state.tableQueryParams = {};
  state.tableParams.page = 1; // 重置到第一页
  // 重置后重新设置默认日期范围
  const dateRange = getDateRange(3, false)
  // state.tableQueryParams.completeTimeStart = dateRange[0]
  // state.tableQueryParams.completeTimeEnd = dateRange[1]
  state.tableQueryParams.createTimeStart = dateRange[0]
  state.tableQueryParams.createTimeEnd = dateRange[1]
  handleQuery();
};
// 打开页面
const handleImportOrder = (row: any) => {
  state.tableDetailParams.importOrderId = row.id;
  state.importOrderNo = row.importOrderNo;
  handleDetailQuery();
};
// 重置 明细
const handleDetailReset = () => {
    state.tableDetailQueryParams = {};
    state.tableDetailParams.page = 1;
    handleDetailQuery();
}
// 查询操作 明细
const handleDetailQuery = async (params: any = {}) => {
  try {
    state.tableDetailLoading = true;
    state.tableDetailParams = Object.assign(state.tableDetailParams, params);
    const result = await wmsStockSlotBoxInfoApi.page(Object.assign(state.tableDetailQueryParams, state.tableDetailParams)).then(res => res.data.result);
    state.tableDetailParams.total = result?.total;
    state.tableDetailData = result?.items ?? [];
  } catch (error) {
    console.error('查询流水明细失败:', error);
  } finally {
    state.tableDetailLoading = false;
  }
};
// 列排序
const sortChange = async (column: any) => {
  state.tableParams.field = column.prop;
  state.tableParams.order = column.order;
  await handleQuery();
};
// 列排序
const sortDetailChange = async (column: any) => {
  state.tableDetailParams.field = column.prop;
  state.tableDetailParams.order = column.order;
  await handleDetailQuery();
};
// 删除
const delwmsImportOrder = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsImportOrderApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => { });
};


// 新增打印相关方法
const handlePrint = (row: any, printType: string) => {
  printDialogRef.value.openDialog(row, printType)
}

const handlePrintReset = async (row: any) => {
  ElMessageBox.confirm(`确定要重置打印状态吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    // 调用重置打印状态的API
    // await wmsImportOrderDetailApi.resetPrint({ id: row.id })
    ElMessage.success("重置成功")
    handleDetailQuery()
  }).catch(() => { })
}
// 导出数据
const exportwmsImportOrderCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    const baseParams = formatDateParams(state.tableQueryParams);
    if (command === 'select') {
      const params = Object.assign({}, baseParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await wmsImportOrderApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, baseParams, state.tableParams);
      await wmsImportOrderApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, baseParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await wmsImportOrderApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
}
// 获取日期范围工具函数
const getDateRange = (days = 3, includeTime = true) => {
  const endDate = dayjs()
  const startDate = dayjs().subtract(days, 'day')

  if (includeTime) {
    return [
      startDate.format('YYYY-MM-DD 00:00:00'),
      endDate.format('YYYY-MM-DD 23:59:59')
    ]
  } else {
    return [
      startDate.format('YYYY-MM-DD'),
      endDate.format('YYYY-MM-DD')
    ]
  }
}
</script>

<style lang="scss" scoped>
.table-query-form {

  :deep(.el-form-item__label) {
    padding-right: 4px !important;
  }
  :deep(.el-button) {
    padding: 8px 10px;
  }
}

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

.import-order-container {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 100px); // Adjust based on your layout header/tabs height
  padding: 5px;
  box-sizing: border-box;
}

.import-order-table {
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
  :deep(.el-card__header) {
    padding: 10px 20px;
  }

  :deep(.el-card__body) {
    padding: 5px;
  }
}

.import-order-detail {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0; // Crucial for nested flex scrolling

  :deep(.el-col) {
    height: 100%;
    display: flex;
    flex-direction: column;
  }
  :deep(.el-card__header) {
    padding: 10px 20px;
  }

  :deep(.el-card__body) {
    padding: 5px;
  }
}

// Force el-card to fill the flex item
:deep(.full-table) {
  flex: 1;
  display: flex;
  flex-direction: column;
  border: none; // Optional: remove border if double borders occur
  
   
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
