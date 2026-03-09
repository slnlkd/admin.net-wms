<template>
  <div class="stock-check-order-container">
    <!-- 盘点单据主表格区域 -->
    <el-row :gutter="5" class="stock-check-order-table">
      <el-col :span="24" :xs="24">
        <el-card class="full-table" shadow="hover" :body-style="{ height: '100%', display: 'flex', flexDirection: 'column' }">
          <!-- 卡片头部 -->
          <template #header>
            <el-icon size="16"
              style="margin-right: 3px; display: inline; vertical-align: middle"><ele-Collection /></el-icon>盘库单据
          </template>
          <!-- 查询表单 -->
          <el-form :model="state.tableQueryParams" ref="queryForm" label-width="80px">
            <el-row :gutter="15">
              <!-- 盘点单号查询条件 -->
              <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="4" class="mb10">
                <el-form-item label="盘点单号">
                  <el-input v-model="state.tableQueryParams.checkBillCode" clearable placeholder="请输入盘点单号" @keyup.enter="handleQuery" />
                </el-form-item>
              </el-col>
              <!-- 执行状态查询条件 -->
              <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="4" class="mb10">
                <el-form-item label="执行状态">
                  <g-sys-dict v-model="state.tableQueryParams.executeFlag" code="CheckBillExecuteFlag" render-as="select"
                    placeholder="请选择状态" clearable filterable />
                </el-form-item>
              </el-col>
              <!-- 盘点日期查询条件 -->
              <el-col :xs="24" :sm="24" :md="12" :lg="8" :xl="6" class="mb10">
                <el-form-item label="盘点日期">
                  <div style="display: flex; gap: 5px; width: 100%;">
                    <el-date-picker type="date" v-model="state.tableQueryParams.startTime" value-format="YYYY-MM-DD"
                      placeholder="开始日期" style="flex: 1;" />
                    <span style="line-height: 32px;">-</span>
                    <el-date-picker type="date" v-model="state.tableQueryParams.endTime" value-format="YYYY-MM-DD"
                      placeholder="结束日期" style="flex: 1;" />
                  </div>
                </el-form-item>
              </el-col>
              <!-- 操作按钮区域 -->
              <el-col :xs="24" :sm="24" :md="24" :lg="8" :xl="6" class="mb10">
                <el-form-item label-width="0px">
                  <el-button type="primary" icon="ele-Search" @click="handleQuery" v-auth="'wmsStockCheckOrder:page'" v-reclick="1000">查询</el-button>
                  <el-button icon="ele-Refresh" @click="handleReset" style="margin-left: 10px">重置</el-button>
                  <el-button type="primary" icon="ele-Plus" @click="openAddDialog" v-auth="'wmsStockCheckOrder:add'" style="margin-left: 10px">添加</el-button>
                  <!-- 导出下拉菜单 -->
                  <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsStockCheckOrderCommand" style="margin-left: 10px">
                    <el-button type="primary" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsStockCheckOrder:export'">
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
          <!-- 盘点单据主表格 -->
          <el-table :data="state.tableData" @selection-change="handleSelectionChange" style="width: 100%; flex: 1; height: 0;"
            v-loading="state.tableLoading" @row-click="handleStockCheckOrder" tooltip-effect="light" row-key="id"
            @sort-change="sortChange" border>
            <!-- 复选框列 -->
            <el-table-column type="selection" width="40" align="center"
              v-if="auth('wmsStockCheckNotify:batchDelete') || auth('wmsStockCheckNotify:export')" />
            <!-- 序号列 -->
            <el-table-column type="index" :fixed="true" label="序号" width="55" align="center" />
            <!-- 盘点单号列 -->
            <el-table-column prop='checkBillCode' :fixed="true" sortable='custom' label='盘点单号' align="center"
              show-overflow-tooltip />
            <!-- 执行状态列 -->
            <el-table-column prop='executeFlag' label='执行状态' sortable='custom' align="center"
              show-overflow-tooltip>
              <template #default="scope">
                <g-sys-dict v-model="scope.row.executeFlag" code="CheckBillExecuteFlag" />
              </template>
            </el-table-column>
            <!-- 仓库名称列 -->
            <el-table-column prop='warehouseName' label='仓库名称' sortable='custom' align="center"
              show-overflow-tooltip />
            <!-- 创建人列 -->
            <el-table-column prop='createUserName' label='创建人' align="center" show-overflow-tooltip />
            <!-- 盘点日期列 -->
            <el-table-column prop='checkDate' label='盘点日期' sortable='custom' align="center" show-overflow-tooltip />
            <!-- 修改人列 -->
            <el-table-column prop='updateUserName' label='修改人' align="center" show-overflow-tooltip />
            <!-- 修改时间列 -->
            <el-table-column prop='updateTime' label='修改时间' sortable='custom' align="center" show-overflow-tooltip />
            <!-- 备注列 -->
            <el-table-column prop='checkRemark' label='备注' align="center" show-overflow-tooltip />
            <!-- 操作列 -->
            <el-table-column label="操作" width="300" align="center" fixed="right" show-overflow-tooltip
              v-if="auth('wmsStockCheckNotify:update') || auth('wmsStockCheckNotify:delete')">
              <template #default="scope">
                <!-- 编辑按钮：只有待执行状态才显示 -->
                <el-button type="primary" v-if="scope.row.executeFlag == 0" icon="ele-Edit" size="small" text
                  @click="openEditDialog(scope.row)" v-auth="'wmsStockCheckNotify:update'">编辑</el-button>
                <!-- 作废按钮：只有待执行状态才显示 -->
                <el-button type="danger" v-if="scope.row.executeFlag == 0" icon="ele-Delete" size="small" text
                  @click="delWmsStockCheckOrder(scope.row)" v-auth="'wmsStockCheckNotify:delete'">作废</el-button>
                <!-- 出库按钮：只有待执行状态才显示 -->
                <el-button type="primary" v-if="scope.row.executeFlag == 0" icon="ele-Sell" size="small" text
                  @click="outbound(scope.row)" v-auth="'wmsStockCheckNotify:stockCheckIssueOutBound'">出库</el-button>
                <!-- 调整按钮：只有待调整状态才显示 -->
                <el-button type="warning" v-if="scope.row.executeFlag == 2" icon="ele-Edit" size="small" text
                  @click="cancelStockEdit(scope.row)" v-auth="'wmsStockCheckNotify:createAdjustMent'">调整</el-button>
              </template>
            </el-table-column>
          </el-table>
          <!-- 分页组件 -->
          <el-pagination v-model:currentPage="state.tableParams.page" v-model:page-size="state.tableParams.pageSize"
            @size-change="handleSizeChange" @current-change="handleCurrentChange"
            layout="total, sizes, prev, pager, next, jumper" :page-sizes="[10, 20, 50, 100, 200, 500]"
            :total="state.tableParams.total" size="small" background />
          <!-- 导入数据组件 -->
          <ImportData ref="importDataRef" :import="wmsStockCheckOrderApi.importData"
            :download="wmsStockCheckOrderApi.downloadTemplate" v-auth="'wmsStockCheckNotify:import'" @refresh="handleQuery" />
          <!-- 添加/编辑对话框组件 -->
          <addDialog ref="addDialogRef" @reloadTable="handleQuery" />
          <!-- 箱码详情对话框组件 -->
          <BoxDetailDialog ref="boxDetailDialogRef" />
        </el-card>
      </el-col>
    </el-row>
    <!-- 盘点单据明细区域 -->
    <el-row :gutter="5" class="stock-check-order-detail">
      <el-col :span="24" :xs="24">
        <el-card class="full-table" shadow="hover" :body-style="{ height: '100%', display: 'flex', flexDirection: 'column' }">
          <!-- 卡片头部 -->
          <template #header>
            <div style="display: flex; justify-content: space-between; align-items: center;">
              <div style="display: flex; align-items: center;">
                <el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"><ele-Collection /></el-icon>
                <span>盘库单据明细【{{ state.checkBillCode }}】</span>
              </div>
              <!-- 明细查询条件 -->
              <div style="display: flex; gap: 5px; align-items: center;">
                <el-input v-model="state.tableDetailQueryParams.stockCode" placeholder="托盘条码" style="width: 150px;" size="small" clearable @keyup.enter="handleDetailQuery" @clear="handleDetailQuery" />
                <el-input v-model="state.tableDetailQueryParams.materialCode" placeholder="物料编码" style="width: 150px;" size="small" clearable @keyup.enter="handleDetailQuery" @clear="handleDetailQuery" />
                <el-input v-model="state.tableDetailQueryParams.materialName" placeholder="物料名称" style="width: 150px;" size="small" clearable @keyup.enter="handleDetailQuery" @clear="handleDetailQuery" />
                <el-button type="primary" icon="ele-Search" @click="handleDetailQuery" size="small" circle />
              </div>
            </div>
          </template>
          <!-- 盘点单据明细表格 -->
          <el-table :data="state.tableDetailData" @selection-change="handleDetailSelectionChange" style="width: 100%; flex: 1; height: 0;"
            v-loading="state.tableDetailLoading" tooltip-effect="light" row-key="id" @sort-change="sortDetailChange"
            border>
            <!-- 复选框列 -->
            <el-table-column type="selection" width="40" align="center"
              v-if="auth('wmsStockCheckNotifyDetail:batchDelete') || auth('wmsStockCheckNotifyDetail:export')" />
            <!-- 序号列 -->
            <el-table-column type="index" label="序号" width="55" align="center" />
            <!-- 盘点单号列 -->
            <el-table-column prop='checkBillCode' label='盘点单号' align="center" width="150" show-overflow-tooltip />
            <!-- 执行状态列 -->
            <el-table-column prop='executeFlag' label='执行状态' align="center" width="100" show-overflow-tooltip>
              <template #default="scope">
                <g-sys-dict v-model="scope.row.executeFlag" code="CheckBillExecuteFlag" />
              </template>
            </el-table-column>
            <!-- 盘点结果列 -->
            <el-table-column prop='checkResult' label='盘点结果' align="center" width="100" show-overflow-tooltip>
              <template #default="scope">
                <g-sys-dict v-model="scope.row.checkResult" code="CheckResultStatus" />
              </template>
            </el-table-column>
            <!-- 托盘条码列 -->
            <el-table-column prop='stockCode' label='托盘条码' align="center" width="120" show-overflow-tooltip />
            <!-- 储位编码列 -->
            <el-table-column prop='stockSlot' label='储位编码' align="center" width="130" show-overflow-tooltip />
            <!-- 物料编码列 -->
            <el-table-column prop='materialCode' label='物料编码' align="center" width="100" show-overflow-tooltip />
            <!-- 物料名称列 -->
            <el-table-column prop='materialName' label='物料名称' align="center" width="200" show-overflow-tooltip />
            <!-- 物料规格列 -->
            <el-table-column prop='materialStandard' label='物料规格' align="center" width="100" show-overflow-tooltip />
            <!-- 批次列 -->
            <el-table-column prop='stockLotNo' label='批次' align="center" sortable='custom' width="150"
              show-overflow-tooltip />
            <!-- 库存数量列 -->
            <el-table-column prop='stockQuantity' label='库存数量' align="center" width="100" show-overflow-tooltip />
            <!-- 实物数量列 -->
            <el-table-column prop='realQuantity' label='实物数量' align="center" width="100" show-overflow-tooltip />
            <!-- 计量单位列 -->
            <el-table-column prop='unitName' label='计量单位' align="center" width="100" show-overflow-tooltip />
            <!-- 质检状态列 -->
            <el-table-column prop='inspectionStatus' align="center" width="120" label='质检状态' show-overflow-tooltip>
              <template #default="scope">
                <g-sys-dict v-model="scope.row.inspectionStatus" code="QualityInspectionStatus" />
              </template>
            </el-table-column>
            <!-- 操作列 -->
            <el-table-column label="操作" align="center" fixed="right" show-overflow-tooltip width="100">
              <template #default="scope">
                <el-button type="primary" icon="ele-View" size="small" text
                  @click="viewBoxCode(scope.row)">查看箱码</el-button>
              </template>
            </el-table-column>
          </el-table>
          <!-- 明细分页组件 -->
          <el-pagination v-model:currentPage="state.tableDetailParams.page"
            v-model:page-size="state.tableDetailParams.pageSize" @size-change="handleDetailSizeChange"
            @current-change="handleDetailCurrentChange" layout="total, sizes, prev, pager, next, jumper"
            :page-sizes="[10, 20, 50, 100, 200, 500]" :total="state.tableDetailParams.total" size="small" background />
        </el-card>
      </el-col>
    </el-row>

  </div>
</template>

<script lang="ts" setup name="wmsStockCheckOrder">
import { ref, reactive, onMounted } from "vue";
import { dayjs } from 'element-plus';
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useWmsStockCheckOrderApi } from '/@/api/stockCheck/stockCheckOrder/wmsStockCheckOrder';
import AddDialog from '/@/views/stockCheck/stockCheckOrder/wmsStockCheckOrder/component/addDialog.vue'
import BoxDetailDialog from '/@/views/stockCheck/stockCheckOrder/wmsStockCheckOrder/component/boxDetailDialog.vue'

// 引入盘点单据API
const wmsStockCheckOrderApi = useWmsStockCheckOrderApi();
// 添加/编辑对话框引用
const addDialogRef = ref();
// 箱码详情对话框引用
const boxDetailDialogRef = ref();
// 导入数据组件引用
const importDataRef = ref();
// 页面响应式状态
const state = reactive({
  exportLoading: false, // 导出加载状态
  tableLoading: false, // 主表格加载状态
  tableDetailLoading: false, // 明细表格加载状态
  stores: {}, // 仓库数据
  showAdvanceQueryUI: false, // 是否显示高级查询界面
  selectData: [] as any[], // 主表格选中的数据
  selectDetailData: [] as any[], // 明细表格选中的数据
  tableQueryParams: {} as any, // 主表格查询参数
  tableDetailQueryParams: {} as any, // 明细表格查询参数
  tableParams: {
    page: 1, // 当前页码
    pageSize: 20, // 每页条数
    total: 0, // 总记录数
    field: 'checkDate', // 排序字段
    order: 'descending', // 排序方式
    descStr: 'descending',
  },
  tableDetailParams: {
    page: 1, // 明细当前页码
    pageSize: 20, // 明细每页条数
    total: 0, // 明细总记录数
    field: 'addDate', // 明细排序字段
    order: 'descending', // 明细排序方式
    descStr: 'descending',
    checkBillId: 0 // 盘点单据ID
  },
  tableBillTypeParams: {
    page: 1,
    pageSize: 200,
    total: 0,
    field: 'checkDate',
    order: 'descending',
    descStr: 'descending',
  },
  tableBillTypeQueryParams: {} as any,
  tableData: [], // 主表格数据
  tableDetailData: [], // 明细表格数据
  checkBillCode: '' // 当前选中的盘点单号
});

// 主表格每页条数变化处理
const handleSizeChange = (val: number) => {
  handleQuery({ pageSize: val });
};

// 主表格当前页变化处理
const handleCurrentChange = (val: number) => {
  handleQuery({ page: val });
};

// 明细表格每页条数变化处理
const handleDetailSizeChange = (val: number) => {
  handleDetailQuery({ pageSize: val });
};

// 明细表格当前页变化处理
const handleDetailCurrentChange = (val: number) => {
  handleDetailQuery({ page: val });
};

// 主表格选择变化处理
const handleSelectionChange = (val: any[]) => {
  state.selectData = val;
};

// 明细表格选择变化处理
const handleDetailSelectionChange = (val: any[]) => {
  state.selectDetailData = val;
};

// 组件挂载时初始化
onMounted(async () => {
  const dateRange = getDateRange(3, false); // 获取最近3天的日期范围
  state.tableQueryParams.startTime = dateRange[0];
  state.tableQueryParams.endTime = dateRange[1];
  handleQuery(); // 初始查询
});

// 格式化日期参数
const formatDateParams = (params: any) => {
  const formattedParams = { ...params };
  if (formattedParams.startTime || formattedParams.endTime) {
    const startTime = formattedParams.startTime
      ? formattedParams.startTime + ' 00:00:00'
      : null;
    const endTime = formattedParams.endTime
      ? formattedParams.endTime + ' 23:59:59'
      : null;

    if (startTime && endTime) {
      formattedParams.checkDateRange = [startTime, endTime];
    } else if (startTime) {
      formattedParams.checkDateRange = [startTime, null];
    } else if (endTime) {
      formattedParams.checkDateRange = [null, endTime];
    }

    delete formattedParams.startTime;
    delete formattedParams.endTime;
  }
  return formattedParams;
};

// 查询盘点单据主表数据
const handleQuery = async (params: any = {}) => {
  try {
    state.tableLoading = true;
    if (params.page === undefined) {
      state.tableParams.page = 1;
    }
    state.tableParams = Object.assign(state.tableParams, params);
    const queryParams = formatDateParams(state.tableQueryParams);
    const result = await wmsStockCheckOrderApi.page(Object.assign(queryParams, state.tableParams)).then(res => res.data.result);
    state.tableParams.total = result?.total;
    state.tableData = result?.items ?? [];
  } catch (error) {
    console.error('查询盘库单据失败:', error);
  } finally {
    state.tableLoading = false;
  }
};

// 重置查询条件
const handleReset = () => {
  state.tableQueryParams = {};
  state.tableParams.page = 1;
  const dateRange = getDateRange(3, false);
  state.tableQueryParams.startTime = dateRange[0];
  state.tableQueryParams.endTime = dateRange[1];
  handleQuery();
};

// 打开添加对话框
const openAddDialog = () => {
  addDialogRef.value.openDialog({}, '添加盘点单');
};

// 打开编辑对话框
const openEditDialog = (row: any) => {
  addDialogRef.value.openDialog(row, '编辑盘点单');
};

// 点击盘点单据行，加载明细数据
const handleStockCheckOrder = (row: any) => {
  state.tableDetailParams.checkBillId = row.id;
  state.checkBillCode = row.checkBillCode;
  handleDetailQuery();
};

// 查询盘点单据明细数据
const handleDetailQuery = async (params: any = {}) => {
  try {
    state.tableDetailLoading = true;
    if (params.page === undefined) {
      state.tableDetailParams.page = 1;
    }
    state.tableDetailParams = Object.assign(state.tableDetailParams, params);
    const queryParams = {
      ...state.tableDetailQueryParams,
      billCode: state.checkBillCode,
      page: state.tableDetailParams.page,
      pageSize: state.tableDetailParams.pageSize,
      field: state.tableDetailParams.field,
      order: state.tableDetailParams.order
    };
    const result = await wmsStockCheckOrderApi.getDetailList(queryParams).then(res => res.data.result);
    state.tableDetailParams.total = result?.total || 0;
    state.tableDetailData = result?.items ?? [];
  } catch (error) {
    console.error('查询盘库单据明细失败:', error);
  } finally {
    state.tableDetailLoading = false;
  }
};

// 主表格排序变化处理
const sortChange = async (column: any) => {
  state.tableParams.field = column.prop;
  state.tableParams.order = column.order;
  await handleQuery();
};

// 明细表格排序变化处理
const sortDetailChange = async (column: any) => {
  state.tableDetailParams.field = column.prop;
  state.tableDetailParams.order = column.order;
  await handleDetailQuery();
};

// 作废盘点单据
const delWmsStockCheckOrder = (row: any) => {
  ElMessageBox.confirm(`确定要作废吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsStockCheckOrderApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("作废成功");
  }).catch(() => { });
};

// 盘库出库
const outbound = async (row: any) => {
  ElMessageBox.confirm(`确定要下发出库吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    var result=await wmsStockCheckOrderApi.stockCheckIssueOutBound({ billCode: row.checkBillCode, warehouseId: row.warehouseId });
    handleQuery();
    ElMessage.success(result.data.result);
  }).catch(() => { });
};

// 调整库存
const cancelStockEdit = async (row: any) => {
  ElMessageBox.confirm(`确定要调整库存吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsStockCheckOrderApi.cancelStockEdit({ id: row.id, billCode: row.checkBillCode });
    handleQuery();
    handleDetailQuery();
    ElMessage.success("调整成功");
  }).catch(() => { });
};

// 查看箱码详情
const viewBoxCode = (row: any) => {
  boxDetailDialogRef.value.openDialog(row.id,row.checkBillCode);
};

// 导出盘点单据
const exportWmsStockCheckOrderCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    const baseParams = formatDateParams(state.tableQueryParams);
    if (command === 'select') {
      const params = Object.assign({}, baseParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await wmsStockCheckOrderApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, baseParams, state.tableParams);
      await wmsStockCheckOrderApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, baseParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await wmsStockCheckOrderApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
};

// 获取日期范围
const getDateRange = (days = 3, includeTime = true) => {
  const endDate = dayjs();
  const startDate = dayjs().subtract(days, 'day');

  if (includeTime) {
    return [
      startDate.format('YYYY-MM-DD 00:00:00'),
      endDate.format('YYYY-MM-DD 23:59:59')
    ];
  } else {
    return [
      startDate.format('YYYY-MM-DD'),
      endDate.format('YYYY-MM-DD')
    ];
  }
};
</script>

<style lang="scss" scoped>
/* 字典容器样式 */
.sys-dict-container {
  flex-direction: row !important;
}

/* 表单样式 */
.handle-form {
  margin-bottom: var(--el-card-padding);

  .el-form-item,
  .el-form-item:last-of-type {
    margin: 0 10px !important;
  }
}

/* 盘点单据主容器样式 */
.stock-check-order-container {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 100px);
  padding: 5px;
  box-sizing: border-box;
}

/* 盘点单据主表格区域样式 */
.stock-check-order-table {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
  margin-bottom: 5px;
  
  :deep(.el-col) {
    height: 100%;
    display: flex;
    flex-direction: column;
  }
}

/* 盘点单据明细区域样式 */
.stock-check-order-detail {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;

  :deep(.el-col) {
    height: 100%;
    display: flex;
    flex-direction: column;
  }
}

/* 全屏表格样式 */
:deep(.full-table) {
  flex: 1;
  display: flex;
  flex-direction: column;
  border: none;
  
  .el-card__body {
    flex: 1;
    display: flex;
    flex-direction: column;
    padding: 10px;
    overflow: hidden;
  }
}

/* 通知栏样式 */
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
