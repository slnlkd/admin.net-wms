<template>
  <div class="wmsStockCheckTask-container" v-loading="state.exportLoading">
    
    <!-- 主卡片 -->
    <el-card class="full-table" shadow="hover">
      <!-- 卡片头部 -->
      <template #header>
        <el-icon size="16"
          style="margin-right:3px; display: inline; vertical-align: middle"><ele-Collection /></el-icon>盘库任务
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
          <!-- 任务编号查询条件 -->
          <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="4" class="mb10">
            <el-form-item label="任务编号">
              <el-input v-model="state.tableQueryParams.checkTaskNo" clearable placeholder="请输入任务编号" @keyup.enter="handleQuery" />
            </el-form-item>
          </el-col>
          <!-- 执行状态查询条件 -->
          <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="4" class="mb10">
            <el-form-item label="执行状态">
              <g-sys-dict v-model="state.tableQueryParams.checkTaskFlag" code="CheckTaskStatus" render-as="select"
                placeholder="请选择执行状态" clearable filterable />
            </el-form-item>
          </el-col>
          <!-- 开始时间查询条件 -->
          <el-col :xs="24" :sm="24" :md="12" :lg="8" :xl="6" class="mb10">
            <el-form-item label="开始时间">
              <div style="display: flex; gap: 5px; width: 100%;">
                <el-date-picker type="date" v-model="state.tableQueryParams.startTime" value-format="YYYY-MM-DD"
                  placeholder="开始日期" style="flex: 1;" />
                <span style="line-height: 32px;">-</span>
                <el-date-picker type="date" v-model="state.tableQueryParams.endTime" value-format="YYYY-MM-DD"
                  placeholder="结束日期" style="flex: 1;" />
              </div>
            </el-form-item>
          </el-col>
          <!-- 操作按钮 -->
          <el-col :xs="24" :sm="24" :md="24" :lg="8" :xl="6" class="mb10">
            <el-form-item label-width="0px">
              <el-button type="primary" icon="ele-Search" @click="handleQuery" v-auth="'wmsStockCheckTask:page'" v-reclick="1000">查询</el-button>
              <el-button icon="ele-Refresh" @click="handleReset" style="margin-left: 10px">重置</el-button>
              <!-- 导出下拉菜单 -->
              <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsStockCheckTaskCommand" style="margin-left: 10px">
                <el-button type="primary" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsStockCheckTask:export'">
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
      <!-- 盘库任务表格 -->
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }"
        style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange"
        border>
        <!-- 复选框列 -->
        <el-table-column type="selection" width="40" align="center"
          v-if="auth('wmsStockCheckTask:batchDelete') || auth('wmsStockCheckTask:export')" />
        <!-- 序号列 -->
        <el-table-column type="index" :fixed="true" align="center" label="序号" width="55" />
        <!-- 任务编号列 -->
        <el-table-column prop='checkTaskNo' :fixed="true" align="center" label='任务编号' sortable='custom' width="150"
          show-overflow-tooltip />
        <!-- 盘点单号列 -->
        <el-table-column prop='checkBillCode' align="center" label='盘点单号' width="150"
          show-overflow-tooltip />
        <!-- 载具条码列 -->
        <el-table-column prop='stockCode' align="center" label='载具条码' sortable='custom' width="120"
          show-overflow-tooltip />
        <!-- 起始位置列 -->
        <el-table-column prop='startLocation' align="center" label='起始位置' sortable='custom' width="120"
          show-overflow-tooltip />
        <!-- 目标位置列 -->
        <el-table-column prop='endLocation' align="center" label='目标位置' sortable='custom' width="120"
          show-overflow-tooltip />
        <!-- 关键信息列 -->
        <el-table-column prop='msg' align="center" label='关键信息' show-overflow-tooltip />
        <!-- 发送方列 -->
        <el-table-column prop='sender' align="center" label='发送方' show-overflow-tooltip />
        <!-- 接收方列 -->
        <el-table-column prop='receiver' align="center" label='接收方' show-overflow-tooltip />
        <!-- 执行状态列 -->
        <el-table-column prop='checkTaskFlag' align="center" label='执行状态' width="120" sortable='custom' show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.checkTaskFlag" code="CheckTaskStatus" />
          </template>
        </el-table-column>
        <!-- 发送时间列 -->
        <el-table-column prop='sendDate' align="center" label='发送时间' sortable='custom' width="150"
          show-overflow-tooltip />
        <!-- 返回时间列 -->
        <el-table-column prop='backDate' align="center" label='返回时间' sortable='custom' width="150"
          show-overflow-tooltip />
        <!-- 操作列 -->
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip>
          <template #default="scope">
            <!-- 取消按钮：只有未开始或执行中状态才显示 -->
            <el-button icon="ele-RefreshLeft" size="small" text type="danger"
              v-if="scope.row.checkTaskFlag == 0 || scope.row.checkTaskFlag == 1" @click="cancelTask(scope.row)"> 取消
            </el-button>
            <!-- 完成按钮：只有未开始或执行中状态才显示 -->
            <el-button icon="ele-CircleCheck" v-if="scope.row.checkTaskFlag == 0 || scope.row.checkTaskFlag == 1" size="small" text
              type="success" @click="complete(scope.row)"> 完成
            </el-button>
          </template>
        </el-table-column>
      </el-table>
      <!-- 分页组件 -->
      <el-pagination v-model:currentPage="state.tableParams.page" v-model:page-size="state.tableParams.pageSize"
        @size-change="(val: any) => handleQuery({ pageSize: val })"
        @current-change="(val: any) => handleQuery({ page: val })" layout="total, sizes, prev, pager, next, jumper"
        :page-sizes="[10, 20, 50, 100, 200, 500]" :total="state.tableParams.total" size="small" background />
    </el-card>
  </div>
</template>

<script lang="ts" setup name="wmsStockCheckTask">
import { reactive, onMounted } from "vue";
import { dayjs } from 'element-plus';
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useWmsStockCheckTaskApi } from '/@/api/stockCheck/stockCheckTask/wmsStockCheckTask';
import { useWmsBaseWareHouseApi } from '/@/api/base/baseWarehouse/wmsBaseWareHouse';
import { usewmsPortApi } from "/@/api/port/wmsPort";

// 引入仓库API
const wmsBaseWareHouseApi = useWmsBaseWareHouseApi();
// 引入盘点任务API
const wmsStockCheckTaskApi = useWmsStockCheckTaskApi();
// 引入端口API
const wmsPortApi = usewmsPortApi();
// 页面响应式状态
const state = reactive({
  exportLoading: false, // 导出加载状态
  tableLoading: false, // 表格加载状态
  stores: {}, // 仓库数据
  showAdvanceQueryUI: false, // 是否显示高级查询界面
  dropdownParams: {
    page:1, // 下拉数据当前页码
    pageSize: 200, // 下拉数据每页条数
    total: 0, // 下拉数据总记录数
    field: 'createTime', // 下拉数据排序字段
    order: 'descending', // 下拉数据排序方式
    descStr: 'descending',
  },
  dropdownQueryParams: {} as any, // 下拉数据查询参数
  dropdownData: {} as any, // 下拉数据
  selectData: [] as any[], // 表格选中的数据
  tableQueryParams: {} as any, // 表格查询参数
  tableParams: {
    page:1, // 当前页码
    pageSize: 20, // 每页条数
    total: 0, // 总记录数
    field: 'sendDate', // 排序字段
    order: 'descending', // 排序方式
    descStr: 'descending',
  },
  tableData: [], // 表格数据
});

// 组件挂载时初始化
onMounted(async () => {
  const dateRange = getDateRange(2, false); // 获取最近2天的日期范围
  state.tableQueryParams.startTime = dateRange[0];
  state.tableQueryParams.endTime = dateRange[1];
  // 加载仓库下拉数据
  const warehousedata = await wmsBaseWareHouseApi.page(Object.assign(state.dropdownQueryParams, state.dropdownParams)).then(res => res.data.result);
  state.dropdownData.warehouseId = warehousedata.items;
  handleQuery(); // 初始查询
});

// 重置查询条件
const handleReset = () => {
  state.tableQueryParams = {};
  const dateRange = getDateRange(2, false);
  state.tableQueryParams.startTime = dateRange[0];
  state.tableQueryParams.endTime = dateRange[1];
  handleQuery();
};

// 查询盘点任务数据
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const queryParams = formatDateParams(state.tableQueryParams);
  const result = await wmsStockCheckTaskApi.page(Object.assign(queryParams, state.tableParams)).then(res => res.data.result);
  state.tableParams.total = result?.total;
  state.tableData = result?.items ?? [];
  state.tableLoading = false;
};

// 表格排序变化处理
const sortChange = async (column: any) => {
  state.tableParams.field = column.prop;
  state.tableParams.order = column.order;
  await handleQuery();
};

// 取消盘点任务
const cancelTask = async (row: any) => {
  ElMessageBox.confirm(`确定取消选中的盘库任务?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsStockCheckTaskApi.cancelTask({ checkTaskNo: row.checkTaskNo });
    handleQuery();
    ElMessage.success("取消成功");
  }).catch(() => { });
};

// 完成盘点任务
const complete = async (row: any) => {
  ElMessageBox.confirm(`确定完成选中的盘库任务?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    console.log(row.taskType);
    // 调用端口反馈接口
    await wmsPortApi.feedBack({ taskNo: row.checkTaskNo, code: "1", taskType: row.taskType, stockCode: row.stockCode, taskEnd: row.endLocation, taskBegin: row.startLocation }).then(res => {
      if (res.data.result?.code == 0) {
        ElMessage.error(res.data.result.msg || '操作失败');
      }
      if (res.data.result?.code == 1) {
        ElMessage.success(res.data.result.msg || '操作成功');
      }
    });
    handleQuery();
  }).catch(() => { });
};

// 导出盘点任务
const exportWmsStockCheckTaskCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    const baseParams = formatDateParams(state.tableQueryParams);
    if (command === 'select') {
      const params = Object.assign({}, baseParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await wmsStockCheckTaskApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, baseParams, state.tableParams);
      await wmsStockCheckTaskApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, baseParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await wmsStockCheckTaskApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
};

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
      formattedParams.sendDateRange = [startTime, endTime];
    } else if (startTime) {
      formattedParams.sendDateRange = [startTime, null];
    } else if (endTime) {
      formattedParams.sendDateRange = [null, endTime];
    }

    delete formattedParams.startTime;
    delete formattedParams.endTime;
  }
  return formattedParams;
};

// 获取日期范围
const getDateRange = (days = 2, includeTime = true) => {
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

// 组件挂载时初始化
onMounted(async () => {
  try {
    const dateRange = getDateRange(2, false); // 获取最近2天的日期范围
    state.tableQueryParams.startTime = dateRange[0];
    state.tableQueryParams.endTime = dateRange[1];
    // 加载仓库下拉数据
    const warehousedata = await wmsBaseWareHouseApi.page(Object.assign(state.dropdownQueryParams, state.dropdownParams)).then(res => res.data.result);
    state.dropdownData.warehouseId = warehousedata.items;
    handleQuery(); // 初始查询
  } catch (error) {
    console.error('初始化失败:', error);
    handleQuery(); // 即使加载仓库数据失败，也要执行初始查询
  }
});
</script>

<style scoped>
/* 输入框、下拉框、数字输入框样式 */
:deep(.el-input),
:deep(.el-select),
:deep(.el-input-number) {
  width: 100%;
}
</style>
