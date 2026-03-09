<script lang="ts" setup name="wmsDailyReport">
import { ref,reactive, onMounted,nextTick} from "vue";
import { auth } from '/@/utils/authFunction';
import { downloadStreamFile } from "/@/utils/download";
import { useWmsBaseBillTypeApi } from '/@/api/base/billType/wmsBaseBillType';
import { usewmsStockWarningApi } from '/@/api/warning/wmsStockWarning';
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import { ElMessage } from 'element-plus'

//复验期预警
const wmsBaseBillTypeApi = useWmsBaseBillTypeApi();
const wmsStockWarningApi = usewmsStockWarningApi();

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
  tableParams: {
    page: 1,
    pageSize: 20,
    total: 0,
    field: 'createTime', // 默认的排序字段
    order: 'descending', // 排序方向
    descStr: 'descending', // 降序排序的关键字符
  },
  tableData: [],
  warningDay:0,
});

// 页面加载时
onMounted(async () => {
  const data = await wmsBaseBillTypeApi.getDropdownData(true).then(res => res.data.result) ?? {};
  state.dropdownData.wareHouseId = data.wareHouseId;
  const result = await wmsStockWarningApi.GetWarningConfig('sys_retest_waring_day').then(res => res.data.result);
  state.warningDay=result;

  await nextTick();

  handleQuery();
});

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableQueryParams.warningDay=state.warningDay;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsStockWarningApi.ReInspectionPeriodWarning(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
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

// 导出数据
const exportWmsReportCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    state.tableQueryParams.WarningType=3;
    if (command === 'select') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await wmsStockWarningApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams);
      await wmsStockWarningApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await wmsStockWarningApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
}

const editDialogRef = ref();

const waringEdit= async()=>{
  editDialogRef.value=true
  const result = await wmsStockWarningApi.GetWarningConfig('sys_retest_waring_day').then(res => res.data.result);
  state.warningDay=result;
};

const warningConfig= async()=>{
  await wmsStockWarningApi.WarningConfig({code:'sys_retest_waring_day',value:state.warningDay}).then(res => res.data.result);
  editDialogRef.value=false
  await nextTick();
  ElMessage({
    message: '复验期预警天数设置成功.',
    type: 'success',
  })
  handleQuery();
};


// handleQuery();
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
            <el-form-item label="关键字">
              <el-input v-model="state.tableQueryParams.keyWord" clearable placeholder="物料名称/编码/规格···"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item >
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'wmsRetestStockSlot:page'" v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}"> 重置 </el-button>
                
                <el-button type="primary" icon="ele-Plus" @click="waringEdit" style="margin-left: 10px">效期预警设置</el-button>

                <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsReportCommand">
                  <el-button type="primary" style="margin-left:5px;" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsRetestStockSlot:export'"> 导出 </el-button>
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
    <el-card class="full-table" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
        <el-table-column type="selection" width="40" align="center" />
        <el-table-column type="index" label="序号" width="55" align="center"/>
        <el-table-column prop='warehouseName' label='所属仓库' show-overflow-tooltip />
        <el-table-column prop='materialCode' label='物料编码' show-overflow-tooltip />
        <el-table-column prop='materialName' label='物料名称' show-overflow-tooltip />
        <el-table-column prop='materialStandard' label='物料规格' show-overflow-tooltip />
        <el-table-column prop='lotNo' label='批次' show-overflow-tooltip />
        <el-table-column prop='stockQuantity' label='库存数量' show-overflow-tooltip />
        <el-table-column prop='unitName' label='计量单位' show-overflow-tooltip />
        <el-table-column prop='slotCode' label='储位编码' show-overflow-tooltip />
        <el-table-column prop='stockStockCode' label='托盘条码' show-overflow-tooltip />
        <el-table-column prop='productionDate' label='生产日期' show-overflow-tooltip />
        <el-table-column prop='retestDate' label='复验日期' show-overflow-tooltip />
        <el-table-column prop='daysOverdue' label='逾期天数' show-overflow-tooltip />
        <el-table-column prop='daysReamin' label='剩余天数' show-overflow-tooltip />
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
    </el-card>

    <el-dialog v-model="editDialogRef" draggable title="复验期预警天数设置" width="500">
    <!-- <el-form :model="form"> -->
    <el-form>
      <el-form-item label="天数">
        <el-input v-model="state.warningDay" autocomplete="off" />
        <!-- <el-input v-model="form.name" autocomplete="off" /> -->
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="editDialogRef = false">关闭</el-button>
        <el-button type="primary" @click="warningConfig">
          确定
        </el-button>
      </div>
    </template>
  </el-dialog>
  </div>
</template>
<style scoped>
:deep(.el-input), :deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>