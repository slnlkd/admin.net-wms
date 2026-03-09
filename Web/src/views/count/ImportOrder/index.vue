<script lang="ts" setup name="wmsImportOrder">
import { ref, reactive, onMounted,computed,nextTick} from "vue";
import { auth } from '/@/utils/authFunction';
import { downloadStreamFile } from "/@/utils/download";
import { useWmsBaseBillTypeApi } from '/@/api/base/billType/wmsBaseBillType';
import { useWmsDailyReportApi } from '/@/api/count/wmsDailyReport';
import { useWmsImportOrderApi } from '/@/api/count/wmsImportOrder';
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import type { TabsPaneContext } from 'element-plus'

const wmsBaseBillTypeApi = useWmsBaseBillTypeApi();
const wmsDailyReportApi = useWmsDailyReportApi();
const wmsImportOrderApi = useWmsImportOrderApi();

const state = reactive({
  exportLoading: false,
  tableLoading: false,
  tableShow:true,
  exportShow:false,
  stores: {},
  showAdvanceQueryUI: false,
  dropdownData: {} as any,
  selectData: [] as any[],
  tableQueryParams: {} as any,
  tableDetailQueryParams: {} as any,
  tableBoxQueryParams: {} as any,
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
  },
    tableBoxParams: {
    page: 1,
    pageSize: 20,
    total: 0,
    field: 'createTime', // 默认的排序字段
    order: 'descending', // 排序方向
    descStr: 'descending', // 降序排序的关键字符
  },
  tableData: [],
  tableDetailData: [],
  tableBoxData: [],
});

// 页面加载时
onMounted(async () => {
  const data = await wmsBaseBillTypeApi.getDropdownData(true).then(res => res.data.result) ?? {};
  state.dropdownData.wareHouseId = data.wareHouseId;
});

// 获取当月第一天（YYYY-MM-DD格式）
const getFirstDayOfMonth = () => {
  const now = new Date();
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  return `${year}-${month}-01`;
};
    
// 获取今天（YYYY-MM-DD格式）
const getToday = () => {
  const now = new Date();
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
};
    
const defaultStartDate = computed(() => getFirstDayOfMonth());
const defaultEndDate = computed(() => getToday());
// 初始化默认值
if (!state.tableQueryParams.startDate) {
  state.tableQueryParams.startDate = defaultStartDate.value;
}
if (!state.tableQueryParams.endDate) {
  state.tableQueryParams.endDate = defaultEndDate.value;
}

//查询 查询前先判断tab页
const handleQueryAll = async () => {
  switch (activeName.value) {
    case 'first':
      handleQuery();
      break
    case 'second':
      handleDetailQuery();
      break
    case 'third':
      handleBoxQuery();
      break
  }
};

// 总单查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsImportOrderApi.GetSearchImOrder(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
  state.tableParams.total = result?.total;
  state.tableData = result?.items ?? [];
  state.tableLoading = false;
};

// 明细查询操作
const handleDetailQuery = async (params: any = {}) => {
  state.tableDetailQueryParams.startDate=state.tableQueryParams.startDate;
  state.tableDetailQueryParams.endDate=state.tableQueryParams.endDate;
  state.tableDetailQueryParams.slotCode=state.tableQueryParams.slotCode;
  state.tableDetailQueryParams.stockCode=state.tableQueryParams.stockCode;
  state.tableDetailQueryParams.lotNo=state.tableQueryParams.lotNo;
  state.tableDetailQueryParams.importNo=state.tableQueryParams.importNo;
  state.tableDetailQueryParams.importExecuteFlag=state.tableQueryParams.importExecuteFlag;

  state.tableLoading = true;
  state.tableDetailParams = Object.assign(state.tableDetailParams, params);
  const result = await wmsImportOrderApi.GetSearchImOrderDetail(Object.assign(state.tableDetailQueryParams, state.tableDetailParams)).then(res => res.data.result);
  state.tableDetailParams.total = result?.total;
  state.tableDetailData = result?.items ?? [];
  state.tableLoading = false;
};

// 箱码查询操作
const handleBoxQuery = async (params: any = {}) => {
  state.tableBoxQueryParams.startDate=state.tableQueryParams.startDate;
  state.tableBoxQueryParams.endDate=state.tableQueryParams.endDate;
  state.tableBoxQueryParams.boxCode=state.tableQueryParams.boxCode;
  state.tableBoxQueryParams.stockCode=state.tableQueryParams.stockCode;
  
  state.tableLoading = true;
  state.tableBoxParams = Object.assign(state.tableBoxParams, params);
  const result = await wmsImportOrderApi.GetSearchImOrderBoxInfo(Object.assign(state.tableBoxQueryParams, state.tableBoxParams)).then(res => res.data.result);
  state.tableBoxParams.total = result?.total;
  state.tableBoxData = result?.items ?? [];
  state.tableLoading = false;
};

// 列排序
const sortChange = async (column: any) => {
  state.tableParams.field = column.prop;
  state.tableParams.order = column.order;
  await handleQuery();
};

// 导出数据
const exportWmsReportCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    let params = {};
    if (command === 'select') {
      switch (activeName.value) {
        case 'first':
          params = Object.assign({}, state.tableQueryParams, state.tableParams, { activeName: activeName.value, selectKeyList: state.selectData.map(u => u.materialId) });
          break
        case 'second':
          params = Object.assign({}, state.tableQueryParams,  state.tableDetailParams , { activeName: activeName.value, selectKeyList: state.selectData.map(u => u.id) });
          break
        case 'third':
          params = Object.assign({}, state.tableQueryParams,state.tableBoxParams , { activeName: activeName.value, selectKeyList: state.selectData.map(u => u.id) });
          break
      }
    } else if (command === 'current') {
       switch (activeName.value) {
        case 'first':
         params = Object.assign({}, state.tableQueryParams, state.tableParams, { activeName: activeName.value });
          break
        case 'second':
          params = Object.assign({}, state.tableQueryParams, state.tableDetailParams, { activeName: activeName.value });
          break
        case 'third':
          params = Object.assign({}, state.tableQueryParams, state.tableBoxParams, { activeName: activeName.value });
          break
      }
    } else if (command === 'all') {

      switch (activeName.value) {
        case 'first':
          params = Object.assign({}, state.tableQueryParams, state.tableParams, { activeName: activeName.value, page: 1, pageSize: 99999999 });
          break
        case 'second':
          params = Object.assign({}, state.tableQueryParams, state.tableDetailParams, { activeName: activeName.value, page: 1, pageSize: 99999999 });
          break
        case 'third':
          params = Object.assign({}, state.tableQueryParams, state.tableBoxParams, { activeName: activeName.value, page: 1, pageSize: 99999999 });
          break
      }
    }
    await wmsImportOrderApi.ImExport(params).then(res => downloadStreamFile(res));
  } finally {
    state.exportLoading = false;
  }
}

const activeName = ref('first')
const importTableRef = ref()

const handleClick = (tab: TabsPaneContext, event: Event) => {
  switch (tab.props.name) {
    case 'first':
      // handleQuery()
      // tableRef.value.setCurrentRow()
      nextTick(() => {
        restoreImportTableSelection()
      })
      break
    case 'second':
      // fetchData2()
      break
    case 'third':
      // fetchData3()
      break
  }
}

const tableSelection = reactive({
  importTable: {
    selectedRowData: null
  },
  inspectTable: {
    selectedRowData: null
  },
  stockTable: {
    selectedRowData: null
  }
})

const handleRowDblClick = (row, column, event) => {
  // 保存选中状态
  tableSelection.importTable.selectedRowData = row
  // 设置为当前行
  if (importTableRef.value) {
    importTableRef.value.setCurrentRow(row)
  }

  activeName.value="second"
  handleDetailQuery({'materialCode':row.materialCode});
}

const handleDetailRowDblClick = (row, column, event) => {
  activeName.value="third"
  handleBoxQuery({'importOrderNo':row.importOrderNo});
}

// 恢复入库表格的选中状态
const restoreImportTableSelection = () => {
  nextTick(() => {
      try {
        importTableRef.value.setCurrentRow(tableSelection.importTable.selectedRowData)
      } catch (error) {
        console.error('恢复选中状态失败:', error)
      }
    })
}

//重置
const handleReset = () => {
  state.tableQueryParams = {};
  state.tableParams.page = 1;
  state.tableQueryParams.startDate = defaultStartDate.value;
  state.tableQueryParams.endDate = defaultEndDate.value;
  handleQueryAll();
};

handleQuery();
</script>
<template>
  <div v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }"> 
      <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="所属仓库">
              <el-select clearable filterable v-model="state.tableQueryParams.wareHouseId" placeholder="请选择所属仓库">
                <el-option v-for="(item,index) in state.dropdownData.wareHouseId ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="开始时间">
               <el-date-picker type="date" v-model="state.tableQueryParams.startDate" value-format="YYYY-MM-DD"
                      placeholder="开始时间" style="flex: 1;" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="结束时间">
               <el-date-picker type="date" v-model="state.tableQueryParams.endDate" value-format="YYYY-MM-DD"
                      placeholder="结束时间" style="flex: 1;" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料编码">
              <el-input v-model="state.tableQueryParams.materialCode" clearable placeholder="请输入物料编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料名称">
                <el-input v-model="state.tableQueryParams.materialName" clearable placeholder="请输入物料名称"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="入库单号">
                <el-input v-model="state.tableQueryParams.importNo" clearable placeholder="请输入入库单号"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="载具条码">
              <el-input v-model="state.tableQueryParams.stockCode" clearable placeholder="请输入载具条码" @keyup.enter="handleQuery" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="储位地址">
              <el-input v-model="state.tableQueryParams.slotCode" clearable placeholder="请输入储位地址" @keyup.enter="handleQuery" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="4" class="mb10">
            <el-form-item label="执行状态">
              <g-sys-dict v-model="state.tableQueryParams.importExecuteFlag" code="ExecuteFlag" render-as="select"
                    placeholder="请选择状态" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="批次">
              <el-input v-model="state.tableQueryParams.lotNo" clearable placeholder="请输入批次" @keyup.enter="handleQuery" />
            </el-form-item>
          </el-col>
           <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="箱码">
              <el-input v-model="state.tableQueryParams.boxCode" clearable placeholder="请输入箱码" @keyup.enter="handleQuery" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item >
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary"  icon="ele-Search" @click="handleQueryAll" v-auth="'wmsImportOrderCount:page'" v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="handleReset"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true" v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsReportCommand">
                  <el-button type="primary" style="margin-left:5px;" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsImportOrderCount:export'"> 导出 </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="select" :disabled="state.selectData.length == 0">导出选中</el-dropdown-item>
                      <el-dropdown-item command="current">导出本页</el-dropdown-item>
                      <el-dropdown-item command="all">导出全部</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" v-show="state.tableShow" shadow="hover" style="margin-top: 5px">
        <el-tabs v-model="activeName" @tab-click="handleClick">
            <el-tab-pane label="总单" name="first">
              <div v-if="activeName=='first'" >
                <el-table  ref="importTableRef" :data="state.tableData" @selection-change="(val: any[]) => {console.log(val); state.selectData = val; }"  
                  highlight-current-row @row-dblclick="handleRowDblClick" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="materialCode" @sort-change="sortChange" border>
                    <el-table-column type="selection" width="40" align="center"/>
                    <el-table-column type="index" label="序号" width="55" align="center"/>
                    <el-table-column prop='materialCode' label='物料编码' show-overflow-tooltip />
                    <el-table-column prop='materialName' label='物料名称' show-overflow-tooltip />
                    <el-table-column prop='materialStandard' label='物料规格' show-overflow-tooltip />
                    <el-table-column prop='qty' label='入库数量' show-overflow-tooltip />
                    <el-table-column prop='unitName' label='计量单位' show-overflow-tooltip />
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
                <printDialog ref="printDialogRef" :title="'打印入出库日报'" @reloadTable="handleQuery" />
              </div>
            </el-tab-pane>
            <el-tab-pane label="明细" name="second">
                <el-table :data="state.tableDetailData" @selection-change="(val: any[]) => { console.log(val);state.selectData = val; }" @row-dblclick="handleDetailRowDblClick"  highlight-current-row style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
                    <el-table-column type="selection" width="40" align="center"/>
                    <el-table-column type="index" label="序号" width="55" align="center"/>
                    <el-table-column prop='importOrderNo' label='入库流水号' show-overflow-tooltip />
                    <el-table-column prop='importNo' label='入库单号' show-overflow-tooltip />
                    <el-table-column prop='stockCode' label='载具条码' show-overflow-tooltip />
                    <el-table-column prop='slotCode' label='储位地址' show-overflow-tooltip />
                    <el-table-column prop='status' label='执行状态' show-overflow-tooltip />
                    <el-table-column prop='createTime' label='添加时间' show-overflow-tooltip />
                    <el-table-column prop='completeTime' label='完成时间' show-overflow-tooltip />
                </el-table>
                <el-pagination 
                v-model:currentPage="state.tableDetailParams.page"
                v-model:page-size="state.tableDetailParams.pageSize"
                @size-change="(val: any) => handleDetailQuery({ pageSize: val })"
                @current-change="(val: any) => handleDetailQuery({ page: val })"
                layout="total, sizes, prev, pager, next, jumper"
                :page-sizes="[10, 20, 50, 100, 200, 500]"
                :total="state.tableDetailParams.total"
                size="small"
                background />
                <!-- <printDialog ref="printDialogRef" :title="'打印入出库日报'" @reloadTable="handleQuery" /> -->
            </el-tab-pane>
            <el-tab-pane label="箱码" name="third">
                <el-table :data="state.tableBoxData" @selection-change="(val: any[]) => { state.selectData = val; }" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
                    <el-table-column type="selection" width="40" align="center"/>
                    <el-table-column type="index" label="序号" width="55" align="center"/>
                    <el-table-column prop='stockCode' label='载具码' show-overflow-tooltip />
                    <el-table-column prop='boxCode' label='箱码号' show-overflow-tooltip />
                    <el-table-column prop='materialCode' label='物料编码' show-overflow-tooltip />
                    <el-table-column prop='materialName' label='物料名称' show-overflow-tooltip />
                    <el-table-column prop='lotNo' label='批次' show-overflow-tooltip />
                    <el-table-column prop='boxQuantity' label='满箱数量' show-overflow-tooltip />
                    <el-table-column prop='qty' label='实际数量' show-overflow-tooltip />
                    <el-table-column prop='productionDate' label='生产日期' show-overflow-tooltip />
                    <el-table-column prop='validateDay' label='有效期' show-overflow-tooltip />
                </el-table>
                <el-pagination 
                v-model:currentPage="state.tableBoxParams.page"
                v-model:page-size="state.tableBoxParams.pageSize"
                @size-change="(val: any) => handleBoxQuery({ pageSize: val })"
                @current-change="(val: any) => handleBoxQuery({ page: val })"
                layout="total, sizes, prev, pager, next, jumper"
                :page-sizes="[10, 20, 50, 100, 200, 500]"
                :total="state.tableBoxParams.total"
                size="small"
                background />
                <!-- <printDialog ref="printDialogRef" :title="'打印入出库日报'" @reloadTable="handleQuery" /> -->
            </el-tab-pane>
        </el-tabs>
    </el-card>
  </div>
</template>
<style scoped>
:deep(.el-input), :deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>
