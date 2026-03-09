<template>
  <div class="import-notify-container">
    <el-row :gutter="5" class="import-notify-table">
      <el-col :span="24" :xs="24">
        <el-card class="full-table" shadow="hover" :body-style="{ height: '100%', display: 'flex', flexDirection: 'column', padding: '5px' }">
          <template #header>
            <el-icon size="16"
              style="margin-right: 3px; display: inline; vertical-align: middle"><ele-Collection /></el-icon>入库单据
          </template>

          <el-form :model="state.tableQueryParams" ref="queryForm" label-width="auto">
            <el-row :gutter="5">
              <el-col :xs="24" :sm="12" :md="8" :lg="3" :xl="3" class="mb5">
                <el-form-item label="入库单号">
                  <el-input v-model="state.tableQueryParams.importBillCode" clearable placeholder="入库单号" @keyup.enter="handleQuery" />
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="8" :lg="3" :xl="3" class="mb5">
                <el-form-item label="执行状态">
                  <g-sys-dict v-model="state.tableQueryParams.importExecuteFlag" code="ExecuteFlag" render-as="select"
                    placeholder="执行状态" clearable filterable />
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="8" :lg="3" :xl="3" class="mb5">
                <el-form-item label="单据类型">
                  <el-select clearable filterable v-model="state.tableBillTypeQueryParams.billType"
                    placeholder="单据类型">
                    <el-option v-for="(item, index) in state.dropdownData.billType ?? []" :key="index" :value="item.id"
                      :label="item.billTypeName" />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="12" :lg="6" :xl="6" class="mb5">
                <el-form-item label="创建时间">
                  <div style="display: flex; gap: 2px; width: 100%;">
                    <el-date-picker type="date" v-model="state.tableQueryParams.createTimeStart" value-format="YYYY-MM-DD"
                      placeholder="开始时间" style="flex: 1;" />
                    <span style="line-height: 32px;">-</span>
                    <el-date-picker type="date" v-model="state.tableQueryParams.createTimeEnd" value-format="YYYY-MM-DD"
                      placeholder="结束时间" style="flex: 1;" />
                  </div>
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="12" :lg="6" :xl="6" class="mb5">
                <el-form-item label-width="0px" style="display: flex; white-space: nowrap;">
                  <el-button type="primary" icon="ele-Search" @click="handleQuery" v-auth="'wmsImportNotify:page'" v-reclick="1000">查询</el-button>
                  <el-button icon="ele-Refresh" @click="handleReset" style="margin-left: 5px">重置</el-button>
                  <el-button type="primary" icon="ele-Plus" @click="editDialogRef.openDialog(null, '新增入库单')" v-auth="'wmsImportNotify:add'" style="margin-left: 5px">新增</el-button>
                  <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsImportNotifyCommand" style="margin-left: 5px">
                    <el-button type="primary" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsImportNotify:export'">
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
                  <el-button type="warning" icon="ele-MostlyCloudy" @click="importDataRef.openDialog()" v-auth="'wmsImportNotify:import'" style="margin-left: 5px">导入</el-button>
                </el-form-item>
              </el-col>
            </el-row>
          </el-form>
          <el-table :data="state.tableData" @selection-change="handleSelectionChange" style="width: 100%; flex: 1; height: 0;"
            v-loading="state.tableLoading" @row-click="handleImportNotify" tooltip-effect="light" row-key="id"
            @sort-change="sortChange" border>
            <el-table-column type="selection" width="40" align="center"
              v-if="auth('wmsImportNotify:batchDelete') || auth('wmsImportNotify:export')" />
            <el-table-column type="index" :fixed="true" label="序号" width="55" align="center" />
            <el-table-column prop='warehouseId' :fixed="true" label='所属仓库' width="120" align="center"
              :formatter="(row: ImportNotifyRow) => formatField(row, 'warehouseFkDisplayName')" show-overflow-tooltip />
            <el-table-column prop='importBillCode' :fixed="true" label='入库单号' sortable='custom' align="center"
              width="150" show-overflow-tooltip />
            <el-table-column prop='importExecuteFlag' label='执行状态' align="center" sortable='custom' width="100"
              show-overflow-tooltip>
              <template #default="scope">
                <g-sys-dict v-model="scope.row.importExecuteFlag" code="ExecuteFlag" />
              </template>
            </el-table-column>
            <el-table-column prop='billTypeDisplayName' label='单据类型' sortable='custom' align="center" width="120"
              show-overflow-tooltip>
            </el-table-column>
            <el-table-column prop='supplierId' label='供货单位' align="center" width="100"
              :formatter="(row: ImportNotifyRow) => formatField(row, 'supplierName')" show-overflow-tooltip />
            <el-table-column prop='manufacturerId' label='制造商单位' align="center" width="100 "
              :formatter="(row: ImportNotifyRow) => formatField(row, 'manufacturerName')" show-overflow-tooltip />
            <el-table-column prop='source' label='来源' align="center" sortable='custom' width="100"
              show-overflow-tooltip />
            <el-table-column prop='outerBillCode' label='外部单号' sortable='custom' align="center" width="120"
              show-overflow-tooltip />
            <el-table-column prop='createUserName' label='创建人' align="center" width="100" show-overflow-tooltip />
            <el-table-column prop='createTime' label='创建时间' align="center" width="120" show-overflow-tooltip />
            <el-table-column prop='updateUserName' label='修改人' align="center" width="100" show-overflow-tooltip />
            <el-table-column prop='updateTime' label='修改时间' align="center" width="120" show-overflow-tooltip />
            <el-table-column prop='remark' label='备注' align="center" width="100" show-overflow-tooltip />
            <el-table-column label="操作" width="180" align="center" fixed="right" show-overflow-tooltip
              v-if="auth('wmsImportNotify:update') || auth('wmsImportNotify:delete')">
              <template #default="scope">
                <el-button type="primary" v-if="canEdit(scope.row)" icon="ele-Edit" size="small" text
                  @click="editDialogRef.openDialog(scope.row, '编辑入库单')" v-auth="'wmsImportNotify:update'">编辑</el-button>
                <el-button icon="ele-Check" v-if="canComplete(scope.row)" size="small" text type="success"
                  @click="complete(scope.row)" v-auth="'wmsImportNotify:delete'"> 关单
                </el-button>
                <el-button icon="ele-Close" v-if="canInvalid(scope.row)" size="small" text type="danger"
                  @click="invalid(scope.row)" v-auth="'wmsImportNotify:delete'"> 作废
                </el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-pagination v-model:current-page="state.tableParams.page" v-model:page-size="state.tableParams.pageSize"
            @size-change="handleSizeChange" @current-change="handleCurrentChange"
            layout="total, sizes, prev, pager, next, jumper" :page-sizes="[10, 20, 50, 100, 200, 500]"
            :total="state.tableParams.total" size="small" background />
          <ImportData ref="importDataRef" :import="wmsImportNotifyApi.importData"
            :download="wmsImportNotifyApi.downloadTemplate" v-auth="'wmsImportNotify:import'" @refresh="handleQuery" />
          <printDialog ref="printDialogRef" :title="'打印入库明细'" @reloadTable="handleDetailQuery" />
          <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
        </el-card>
      </el-col>
    </el-row>
    <el-row :gutter="5" class="import-notify-detail">
      <el-col :span="24" :xs="24">
        <el-card class="full-table" shadow="hover" :body-style="{ height: '100%', display: 'flex', flexDirection: 'column', padding: '5px' }">
          <template #header>
            <div style="display: flex; justify-content: space-between; align-items: center;">
              <div style="display: flex; align-items: center;">
                <el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"><ele-Collection /></el-icon>
                <span>物料明细【{{ state.ImportBillCode }}】</span>
              </div>
              <div style="display: flex; gap: 5px; align-items: center;">
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
            :key="showTaskStatusColumn"
            border>
            <el-table-column type="selection" width="40" align="center"
              v-if="auth('wmsImportNotifyDetail:batchDelete') || auth('wmsImportNotifyDetail:export')" />
            <el-table-column type="index" label="序号" width="55" align="center" />
            <el-table-column prop='materialCode' label='物料编码' align="center" sortable='custom' width="120"
              show-overflow-tooltip />
            <el-table-column prop='materialName' label='物料名称' align="center" width="150" show-overflow-tooltip />
            <el-table-column prop='materialStandard' label='物料规格' align="center" width="100" show-overflow-tooltip />
            <el-table-column prop='lotNo' label='批次' align="center" sortable='custom' width="65"
              show-overflow-tooltip />
            <el-table-column prop='importProductionDate' align="center" sortable='custom' width="100" label='生产日期'
              show-overflow-tooltip />
            <el-table-column prop='importLostDate' align="center" sortable='custom' width="100" label='有效期至'
              show-overflow-tooltip />

            <el-table-column prop='materialStatus' align="center" width="100" label='物料状态' show-overflow-tooltip>
              <template #default="scope">
                <g-sys-dict v-model="scope.row.materialStatus" code="MaterialStatus" />
              </template>
            </el-table-column>
            <el-table-column prop='unitName' align="center" width="80" label='计量单位' show-overflow-tooltip />
            <el-table-column prop='importExecuteFlag' align="center" sortable='custom' width="100" label='执行状态'
              show-overflow-tooltip>
              <template #default="scope">
                <g-sys-dict v-model="scope.row.importExecuteFlag" code="ExecuteFlag" />
              </template>
            </el-table-column>
            <el-table-column v-if="showTaskStatusColumn" prop='importTaskStatus' align="center" width="100" label='下发状态' show-overflow-tooltip>
              <template #default="scope">
                <g-sys-dict v-model="scope.row.importTaskStatus" code="ImportPending" />
              </template>
            </el-table-column>
            <el-table-column prop='importQuantity' align="center" width="100" label='计划数量' show-overflow-tooltip />
            <el-table-column prop='importFactQuantity' align="center" width="100" label='已组盘数量' show-overflow-tooltip />
            <el-table-column prop='importCompleteQuantity' align="center" width="100" label='完成数量'
              show-overflow-tooltip />
            <el-table-column prop='boxQuantity' label='箱数量' align="center" width="100" show-overflow-tooltip />
            <el-table-column prop='labeling' label='贴标' align="center" width="100" show-overflow-tooltip />
            <el-table-column prop='remark' label='物料备注' align="center" width="100" show-overflow-tooltip />
            <el-table-column label="操作" align="center" fixed="right" width="250" show-overflow-tooltip>
              <template #default="scope">
                <el-button icon="ele-CaretRight" v-if="canIssue(scope.row)" size="small" text type="primary"
                  @click="issueDialogRef.openDialog(scope.row, '下发')"> 下发</el-button>
                <el-button icon="ele-Printer" size="small" v-if="canPrint(scope.row)"
                  text type="primary" @click="openPrint(scope.row)"> 打印</el-button>
                <el-button icon="ele-Printer" size="small" v-if="canReprintOrReset(scope.row)"
                  text type="primary" @click="repairPrintDialogRef.openDialog(scope.row, '补打')"> 补打</el-button>
                <el-button icon="ele-Refresh" size="small" v-if="canReprintOrReset(scope.row)"
                  text type="primary" @click="handlePrintReset(scope.row)"> 重置打印</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-pagination v-model:current-page="state.tableDetailParams.page"
            v-model:page-size="state.tableDetailParams.pageSize" @size-change="handleDetailSizeChange"
            @current-change="handleDetailCurrentChange" layout="total, sizes, prev, pager, next, jumper"
            :page-sizes="[10, 20, 50, 100, 200, 500]" :total="state.tableDetailParams.total" size="small" background />
          <repairPrintDialog ref="repairPrintDialogRef" @reloadTable="handleDetailQuery" />
          <issueDialog ref="issueDialogRef" @reloadTable="handleDetailQuery" />

        </el-card>
      </el-col>
    </el-row>

  </div>
</template>

<script lang="ts" setup name="wmsImportNotify">
import { ref, reactive, onMounted, computed } from "vue";
import { dayjs } from 'element-plus';
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { hiprint } from 'vue-plugin-hiprint';
import { getAPI } from '/@/utils/axios-utils';
import { SysPrintApi } from '/@/api-services/api';
import { SysPrint } from '/@/api-services/models';
import { formatDate } from '/@/utils/formatTime';
import { useWmsImportNotifyApi } from '/@/api/import/importBill/wmsImportNotify';
import { useWmsImportNotifyDetailApi } from '/@/api/import/importBill/wmsImportNotifyDetail';
import { useWmsBaseBillTypeApi } from '/@/api/base/billType/wmsBaseBillType';
import { usewmsImportLabelPrintApi } from '/@/api/import/importLabelPrint/wmsImportLabelPrint';
import EditDialog from '/@/views/import/importBill/wmsImportNotify/component/editDialog.vue';
import printDialog from '/@/views/system/print/component/hiprint/preview.vue';
import repairPrintDialog from '/@/views/import/importBill/wmsImportNotify/component/repairPrintDialog.vue';
import issueDialog from '/@/views/import/importBill/wmsImportNotify/component/issueDialog.vue';
import ImportData from "/@/components/table/importData.vue";

// 类型定义
interface ImportNotifyRow {
  id: number;
  importBillCode: string;
  importExecuteFlag: string;
  warehouseFkDisplayName?: string;
  supplierName?: string;
  manufacturerName?: string;
  billTypeDisplayName?: string;
  [key: string]: any;
}

interface ImportNotifyDetailRow {
  id: number;
  materialCode: string;
  materialName: string;
  lotNo: string;
  importExecuteFlag: string;
  importTaskStatus: number;
  taskStatus: number;
  labeling: string;
  labelJudgment: number;
  [key: string]: any;
}

// 常量定义
const EXECUTE_FLAG = {
  PENDING: '01',
  IN_PROGRESS: '02',
  PARTIAL_COMPLETE: '03',
  COMPLETED: '04',
} as const;

const TASK_STATUS = {
  NOT_STARTED: 0,
  IN_PROGRESS: 1,
  COMPLETED: 2,
} as const;

const LABEL_JUDGMENT = {
  CAN_REPRINT: 1,
  CAN_PRINT: 2,
} as const;

const wmsImportNotifyApi = useWmsImportNotifyApi();
const wmsImportNotifyDetailApi = useWmsImportNotifyDetailApi();
const wmsBaseBillTypeApi = useWmsBaseBillTypeApi();
const wmsImportLabelPrintApi = usewmsImportLabelPrintApi();
const printDialogRef = ref();
const repairPrintDialogRef = ref();
const editDialogRef = ref();
const issueDialogRef = ref();
const importDataRef = ref();
const state = reactive({
  exportLoading: false,
  tableLoading: false,
  tableDetailLoading: false,
  stores: {},
  showAdvanceQueryUI: false,
  dropdownData: {} as any,
  selectData: [] as ImportNotifyRow[],
  selectDetailData: [] as ImportNotifyDetailRow[],
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
    ImportId: 0
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
  tableData: [] as ImportNotifyRow[],
  tableDetailData: [] as ImportNotifyDetailRow[],
  currentImportNotifyRow: null as ImportNotifyRow | null,
  ImportBillCode: '',
  printParams: {} as any,

});

// 通用格式化函数
const formatField = (row: any, field: string) => row[field];

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

// 设置默认日期范围
const setDefaultDateRange = () => {
  const [start, end] = getDateRange(3, false);
  state.tableQueryParams.createTimeStart = start;
  state.tableQueryParams.createTimeEnd = end;
};

// 通用确认操作方法
const confirmAction = async (
  message: string,
  action: () => Promise<any>,
  successMsg: string
) => {
  try {
    await ElMessageBox.confirm(message, "提示", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning",
    });
    await action();
    handleQuery();
    ElMessage.success(successMsg);
  } catch {
    // 用户取消操作
  }
};

// 按钮显示逻辑辅助函数
const canEdit = (row: ImportNotifyRow) => row.importExecuteFlag === EXECUTE_FLAG.PENDING;
const canComplete = (row: ImportNotifyRow) => row.importExecuteFlag !== EXECUTE_FLAG.COMPLETED&&row.importExecuteFlag !== EXECUTE_FLAG.PENDING;
const canInvalid = (row: ImportNotifyRow) => row.importExecuteFlag === EXECUTE_FLAG.PENDING;
const canIssue = (row: ImportNotifyDetailRow) => 
  state.currentImportNotifyRow?.importExecuteFlag === EXECUTE_FLAG.PENDING && 
  row.importTaskStatus === TASK_STATUS.NOT_STARTED;
const canPrint = (row: ImportNotifyDetailRow) => true;
const canReprintOrReset =  (row: ImportNotifyDetailRow) => true;
(row: ImportNotifyDetailRow) =>
  row.importExecuteFlag === EXECUTE_FLAG.PENDING &&
  row.labeling === '否' &&
  row.labelJudgment === LABEL_JUDGMENT.CAN_REPRINT;

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
  const data = await wmsImportNotifyApi.getDropdownData(true).then(res => res.data.result) ?? {};
  state.tableBillTypeQueryParams.BillType = 1;
  const billdata = await wmsBaseBillTypeApi.page(Object.assign(state.tableBillTypeQueryParams, state.tableBillTypeParams)).then(res => res.data.result);
  state.dropdownData.warehouseId = data.warehouseId;
  state.dropdownData.supplierId = data.supplierId;
  state.dropdownData.customerId = data.customerId;
  state.dropdownData.billType = billdata.items;
  setDefaultDateRange();
  handleQuery();
});

// 处理日期格式转换：将两个独立的日期转换为 createTimeRange 数组格式
const formatDateParams = (params: any) => {
  const formattedParams = { ...params };
  const { createTimeStart, createTimeEnd } = formattedParams;

  // 如果存在日期参数，转换为 createTimeRange 数组
  if (createTimeStart || createTimeEnd) {
    const startTime = createTimeStart ? `${createTimeStart} 00:00:00` : null;
    const endTime = createTimeEnd ? `${createTimeEnd} 23:59:59` : null;

    formattedParams.createTimeRange = [startTime || null, endTime || null];

    // 删除独立的日期参数
    delete formattedParams.createTimeStart;
    delete formattedParams.createTimeEnd;
  }

  return formattedParams;
};

// 计算属性：格式化后的查询参数
const formattedQueryParams = computed(() => formatDateParams(state.tableQueryParams));

// 计算属性：是否显示任务状态列（当有任何一行的任务状态大于0时显示）
const showTaskStatusColumn = computed(() => {
  return state.tableDetailData.some((row: ImportNotifyDetailRow) => row.importTaskStatus > 0);
});

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsImportNotifyApi.page(
    Object.assign(formatDateParams(state.tableQueryParams), state.tableParams)
  ).then(res => res.data.result);
  state.tableParams.total = result?.total;
  state.tableData = result?.items ?? [];
  state.tableLoading = false;
};
//重置
const handleReset = () => {
  state.tableQueryParams = {};
  setDefaultDateRange();
  handleQuery();
};
//重置 明细
const handleDetailReset = () => {
  state.tableDetailQueryParams = {};
  state.tableDetailParams.page = 1;
  handleDetailQuery();
};
// 打开页面
const handleImportNotify = (row: any) => {
  state.tableDetailParams.ImportId = row.id;
  state.ImportBillCode = row.importBillCode;
  state.currentImportNotifyRow = row;
  handleDetailQuery();
};
// 查询操作 明细
const handleDetailQuery = async (params: any = {}) => {
  state.tableDetailLoading = true;
  state.tableDetailParams = Object.assign(state.tableDetailParams, params);
  const result = await wmsImportNotifyDetailApi.page(Object.assign(state.tableDetailQueryParams, state.tableDetailParams)).then(res => res.data.result);
  state.tableDetailParams.total = result?.total;
  state.tableDetailData = result?.items ?? [];
  state.tableDetailLoading = false;
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
const _delWmsImportNotify = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsImportNotifyApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => { });
};
// 打开打印页面
const openPrint = async (row: any) => {
  var res = await getAPI(SysPrintApi).apiSysPrintPrintNameGet('入库单据-物料明细');
  var printTemplate = res.data.result as SysPrint;
  var template = JSON.parse(printTemplate.template);
  row['printDate'] = formatDate(new Date(), 'YYYY-mm-dd HH:MM:SS')
  var printDataRes = await wmsImportLabelPrintApi.print({ labelID: "", importDetailId: row.id, boxCode: '', isprint: '0', number: 1 });
  var printData = printDataRes.data.result;
  template.type = "import";
  printDialogRef.value.showDialog(new hiprint.PrintTemplate({ template: template }), printData, template.panels[0].width);
}
//关单
const complete = async (row: ImportNotifyRow) => {
  await confirmAction(
    '确定要关单吗?',
    async () => await wmsImportNotifyApi.complete({ ImportId: row.id }),
    '关单成功'
  );
};

//作废
const invalid = async (row: ImportNotifyRow) => {
  await confirmAction(
    '确定要作废吗?',
    async () => await wmsImportNotifyApi.invalid({ ImportId: row.id }),
    '作废成功'
  );
};
// 批量删除
const _batchDelWmsImportNotify = () => {
  ElMessageBox.confirm(`确定要删除${state.selectData.length}条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsImportNotifyApi.batchDelete(state.selectData.map(u => ({ id: u.id }))).then(res => {
      ElMessage.success(`成功批量删除${res.data.result}条记录`);
      handleQuery();
    });
  }).catch(() => { });
};

const handlePrintReset = async (row: ImportNotifyDetailRow) => {
  try {
    await ElMessageBox.confirm('确定要重置打印状态吗?', "提示", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning",
    });
    await wmsImportLabelPrintApi.reset({ importDetailId: row.id });
    ElMessage.success("重置成功");
    handleDetailQuery();
  } catch {
    // 用户取消操作
  }
};
// 导出数据
const exportWmsImportNotifyCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    const baseParams = formatDateParams(state.tableQueryParams);
    let params: any;

    switch (command) {
      case 'select':
        params = { ...baseParams, ...state.tableParams, selectKeyList: state.selectData.map(u => u.id) };
        break;
      case 'current':
        params = { ...baseParams, ...state.tableParams };
        break;
      case 'all':
        params = { ...baseParams, ...state.tableParams, page: 1, pageSize: 99999999 };
        break;
      default:
        return;
    }

    const res = await wmsImportNotifyApi.exportData(params);
    downloadStreamFile(res);
  } finally {
    state.exportLoading = false;
  }
}

</script>

<style lang="scss" scoped>
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

.import-notify-container {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 100px); // Adjust based on your layout header/tabs height
  padding: 5px;
  box-sizing: border-box;
}

.import-notify-table {
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

.import-notify-detail {
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
 
</style>
