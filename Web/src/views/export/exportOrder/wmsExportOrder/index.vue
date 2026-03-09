<script lang="ts" setup name="wmsExportOrder">
import { ref, reactive, onMounted } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useWmsExportOrderApi } from '/@/api/export/exportOrder/wmsExportOrder';
import editDialog from '/@/views/export/exportOrder/wmsExportOrder/component/editDialog.vue'
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import ModifyRecord from '/@/components/table/modifyRecord.vue';
import ImportData from "/@/components/table/importData.vue";

const wmsExportOrderApi = useWmsExportOrderApi();
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
});

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsExportOrderApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
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
const delWmsExportOrder = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    console.log({ ExportTypeName:row.exportType,OrderNo: row.exportOrderNo});
    await wmsExportOrderApi.ExportUndo({ ExportTypeName:row.exportType,OrderNo: row.exportOrderNo});
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => {});
};

// 批量删除
const batchDelWmsExportOrder = () => {
  ElMessageBox.confirm(`确定要删除${state.selectData.length}条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsExportOrderApi.batchDelete(state.selectData.map(u => ({ id: u.id }) )).then(res => {
      ElMessage.success(`成功批量删除${res.data.result}条记录`);
      handleQuery();
    });
  }).catch(() => {});
};

// 批量出库
const batchOUtWmsExportOrder = () => {
  ElMessageBox.confirm(`确定要出库${state.selectData.length}条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsExportOrderApi.IssueOutBound(state.selectData.map(u => ({ id: u.id }) )).then(res => {
      ElMessage.success(`下发成功批量${res.data.result}条记录`);
      handleQuery();
    });
  }).catch(() => {});
};
// 导出数据
const exportWmsExportOrderCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    if (command === 'select') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await wmsExportOrderApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams);
      await wmsExportOrderApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await wmsExportOrderApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
}

handleQuery();
</script>
<template>
  <div class="wmsExportOrder-container" v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }"> 
      <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="state.tableQueryParams.keyword" clearable placeholder="请输入模糊查询关键字"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="出库流水单据">
              <el-input v-model="state.tableQueryParams.exportOrderNo" clearable placeholder="请输入出库流水单据"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="单据类型">
              <el-input v-model="state.tableQueryParams.exportBillType" clearable placeholder="请输入单据类型"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料ID">
              <el-input v-model="state.tableQueryParams.exportMaterialId" clearable placeholder="请输入物料ID"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料编码">
              <el-input v-model="state.tableQueryParams.exportMaterialCode" clearable placeholder="请输入物料编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料名称">
              <el-input v-model="state.tableQueryParams.exportMaterialName" clearable placeholder="请输入物料名称"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料规格">
              <el-input v-model="state.tableQueryParams.exportMaterialStandard" clearable placeholder="请输入物料规格"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料型号">
              <el-input v-model="state.tableQueryParams.exportMaterialModel" clearable placeholder="请输入物料型号"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料类型">
              <el-input v-model="state.tableQueryParams.exportMaterialType" clearable placeholder="请输入物料类型"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="物料单位">
              <el-input v-model="state.tableQueryParams.exportMaterialUnit" clearable placeholder="请输入物料单位"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="仓库ID">
              <el-input v-model="state.tableQueryParams.exportWarehouseId" clearable placeholder="请输入仓库ID"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="区域ID">
              <el-input v-model="state.tableQueryParams.exportAreaId" clearable placeholder="请输入区域ID"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="储位编码">
              <el-input v-model="state.tableQueryParams.exportSlotCode" clearable placeholder="请输入储位编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="托盘条码">
              <el-input v-model="state.tableQueryParams.exportStockCode" clearable placeholder="请输入托盘条码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="出库数量">
              <el-input-number v-model="state.tableQueryParams.exportQuantity"  clearable placeholder="请输入出库数量"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="出库重量">
              <el-input v-model="state.tableQueryParams.exportWeight" clearable placeholder="请输入出库重量"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="生产日期">
              <el-date-picker type="daterange" v-model="state.tableQueryParams.exportProductionDateRange"  value-format="YYYY-MM-DD HH:mm:ss" start-placeholder="开始日期" end-placeholder="结束日期" :default-time="[new Date('1 00:00:00'), new Date('1 23:59:59')]" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="失效日期">
              <el-date-picker type="daterange" v-model="state.tableQueryParams.exportLoseDateRange"  value-format="YYYY-MM-DD HH:mm:ss" start-placeholder="开始日期" end-placeholder="结束日期" :default-time="[new Date('1 00:00:00'), new Date('1 23:59:59')]" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="部门编码">
              <el-input v-model="state.tableQueryParams.exportDepartmentCode" clearable placeholder="请输入部门编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="供应商编码">
              <el-input v-model="state.tableQueryParams.expotSupplierCode" clearable placeholder="请输入供应商编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="客户编码">
              <el-input v-model="state.tableQueryParams.exportCustomerCode" clearable placeholder="请输入客户编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="任务号">
              <el-input v-model="state.tableQueryParams.exportTaskNo" clearable placeholder="请输入任务号"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="执行标志（0待执行、1正在执行、2已完成、3已上传）">
              <el-input-number v-model="state.tableQueryParams.exportExecuteFlag"  clearable placeholder="请输入执行标志（0待执行、1正在执行、2已完成、3已上传）"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="单据创建时间">
              <el-date-picker type="daterange" v-model="state.tableQueryParams.exporOrederDateRange"  value-format="YYYY-MM-DD HH:mm:ss" start-placeholder="开始日期" end-placeholder="结束日期" :default-time="[new Date('1 00:00:00'), new Date('1 23:59:59')]" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="备注">
              <el-input v-model="state.tableQueryParams.exportRemark" clearable placeholder="请输入备注"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="批次">
              <el-input v-model="state.tableQueryParams.exportLotNo" clearable placeholder="请输入批次"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="出库单据">
              <el-input v-model="state.tableQueryParams.exportBillCode" clearable placeholder="请输入出库单据"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="出库序号">
              <el-input v-model="state.tableQueryParams.exportListNo" clearable placeholder="请输入出库序号"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="扫码数量">
              <el-input-number v-model="state.tableQueryParams.scanQuantity"  clearable placeholder="请输入扫码数量"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="扫码人">
              <el-input v-model="state.tableQueryParams.scanUserNames" clearable placeholder="请输入扫码人"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="完成时间">
              <el-date-picker type="daterange" v-model="state.tableQueryParams.completeDateRange"  value-format="YYYY-MM-DD HH:mm:ss" start-placeholder="开始日期" end-placeholder="结束日期" :default-time="[new Date('1 00:00:00'), new Date('1 23:59:59')]" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="拣货数量">
              <el-input-number v-model="state.tableQueryParams.pickedNum"  clearable placeholder="请输入拣货数量"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="出库单据明细ID">
              <el-input v-model="state.tableQueryParams.exportDetailId" clearable placeholder="请输入出库单据明细ID"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="箱数">
              <el-input v-model="state.tableQueryParams.wholeBoxNum" clearable placeholder="请输入箱数"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="托盘类型">
              <el-input-number v-model="state.tableQueryParams.outType"  clearable placeholder="请输入托盘类型"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="托盘总箱数">
              <el-input v-model="state.tableQueryParams.stockWholeBoxNum" clearable placeholder="请输入托盘总箱数"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="托盘总数量">
              <el-input v-model="state.tableQueryParams.stockQuantity" clearable placeholder="请输入托盘总数量"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="出库类型，0：正常出库，1：分拣出库，2：转运出库，3：托盘出库,4：备料出库">
              <el-input-number v-model="state.tableQueryParams.exportType"  clearable placeholder="请输入出库类型，0：正常出库，1：分拣出库，2：转运出库，3：托盘出库,4：备料出库"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="质检状态">
              <el-input-number v-model="state.tableQueryParams.inspectionStatus"  clearable placeholder="请输入质检状态"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="WholeOutWare">
              <el-input v-model="state.tableQueryParams.wholeOutWare" clearable placeholder="请输入WholeOutWare"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="根据状态排序">
              <el-input-number v-model="state.tableQueryParams.orderByStatus"  clearable placeholder="请输入根据状态排序"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="储位编码">
              <el-input v-model="state.tableQueryParams.exportStockSlotCode" clearable placeholder="请输入储位编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item >
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'wmsExportOrder:page'" v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true" v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                <el-button icon="ele-ZoomOut" @click="() => state.showAdvanceQueryUI = false" v-if="state.showAdvanceQueryUI" style="margin-left:5px;"> 隐藏 </el-button>
                <el-button type="danger" style="margin-left:5px;" icon="ele-Delete" @click="batchDelWmsExportOrder" :disabled="state.selectData.length == 0" v-auth="'wmsExportOrder:batchDelete'"> 删除 </el-button>
                <el-button type="primary" style="margin-left:5px;" icon="ele-Plus" @click="batchOUtWmsExportOrder" v-auth="'wmsExportOrder:Out'"> 出库 </el-button>
                <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsExportOrderCommand">
                  <el-button type="primary" style="margin-left:5px;" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsExportOrder:export'"> 导出 </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="select" :disabled="state.selectData.length == 0">导出选中</el-dropdown-item>
                      <el-dropdown-item command="current">导出本页</el-dropdown-item>
                      <el-dropdown-item command="all">导出全部</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
                <el-button type="warning" style="margin-left:5px;" icon="ele-MostlyCloudy" @click="importDataRef.openDialog()" v-auth="'wmsExportOrder:import'"> 导入 </el-button>
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
        <el-table-column type="selection" width="40" align="center" v-if="auth('wmsExportOrder:batchDelete') || auth('wmsExportOrder:export')" />
        <el-table-column type="index" label="序号" width="55" align="center"/>
        <el-table-column prop='exportOrderNo' label='出库流水单据' show-overflow-tooltip min-width="150"/>
        <el-table-column prop='exportBillType' label='单据类型' show-overflow-tooltip />
        <el-table-column prop='exportMaterialCode' label='物料编码' show-overflow-tooltip />
        <el-table-column prop='exportMaterialName' label='物料名称' show-overflow-tooltip />
        <el-table-column prop='exportMaterialStandard' label='物料规格' show-overflow-tooltip />
        <el-table-column prop='exportMaterialModel' label='物料型号' show-overflow-tooltip />
        <el-table-column prop='exportMaterialType' label='物料类型' show-overflow-tooltip />
        <el-table-column prop='exportMaterialUnit' label='物料单位' show-overflow-tooltip />
        <el-table-column prop='exportWarehouseId' label='仓库ID' show-overflow-tooltip />
        <el-table-column prop='exportAreaId' label='区域ID' show-overflow-tooltip />
        <el-table-column prop='exportSlotCode' label='储位编码' show-overflow-tooltip />
        <el-table-column prop='exportStockCode' label='托盘条码' show-overflow-tooltip />
        <el-table-column prop='exportQuantity' label='出库数量' show-overflow-tooltip />
        <el-table-column prop='exportWeight' label='出库重量' show-overflow-tooltip />
        <el-table-column prop='exportProductionDate' label='生产日期' show-overflow-tooltip />
        <el-table-column prop='exportLoseDate' label='失效日期' show-overflow-tooltip />
        <el-table-column prop='exportDepartmentCode' label='部门编码' show-overflow-tooltip />
        <el-table-column prop='expotSupplierCode' label='供应商编码' show-overflow-tooltip />
        <el-table-column prop='exportCustomerCode' label='客户编码' show-overflow-tooltip />
        <el-table-column prop='exportTaskNo' label='任务号' show-overflow-tooltip />
        <el-table-column prop='exportExecuteFlag' label='执行标志' show-overflow-tooltip />
        <el-table-column prop='exporOrederDate' label='创建时间' show-overflow-tooltip />
        <el-table-column prop='exportRemark' label='备注' show-overflow-tooltip />
        <el-table-column prop='exportLotNo' label='批次' show-overflow-tooltip />
        <el-table-column prop='exportBillCode' label='出库单据' show-overflow-tooltip />
        <el-table-column prop='exportListNo' label='出库序号' show-overflow-tooltip />
        <el-table-column prop='scanQuantity' label='扫码数量' show-overflow-tooltip />
        <el-table-column prop='scanUserNames' label='扫码人' show-overflow-tooltip />
        <el-table-column prop='completeDate' label='完成时间' show-overflow-tooltip />
        <el-table-column prop='pickedNum' label='拣货数量' show-overflow-tooltip />
        <el-table-column prop='wholeBoxNum' label='箱数' show-overflow-tooltip />
        <el-table-column prop='outType' label='托盘类型' show-overflow-tooltip />
        <el-table-column prop='stockWholeBoxNum' label='托盘总箱数' show-overflow-tooltip />
        <el-table-column prop='stockQuantity' label='托盘总数量' show-overflow-tooltip />
        <el-table-column prop='exportType' label='出库类型' show-overflow-tooltip />
        <el-table-column prop='inspectionStatus' label='质检状态' show-overflow-tooltip />
        <el-table-column prop='wholeOutWare' label='出库口' show-overflow-tooltip />
        <el-table-column prop='exportStockSlotCode' label='储位编码' show-overflow-tooltip />
        <el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
          <template #default="scope">
            <ModifyRecord :data="scope.row" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip v-if="auth('wmsExportOrder:update') || auth('wmsExportOrder:delete')">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text type="primary" @click="editDialogRef.openDialog(scope.row, '编辑出库流水')" v-auth="'wmsExportOrder:update'"> 编辑 </el-button>
            <el-button icon="ele-Delete" size="small" text type="primary" @click="delWmsExportOrder(scope.row)" v-auth="'wmsExportOrder:delete'"> 删除 </el-button>
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
      <ImportData ref="importDataRef" :import="wmsExportOrderApi.importData" :download="wmsExportOrderApi.downloadTemplate" v-auth="'wmsExportOrder:import'" @refresh="handleQuery"/>
      <printDialog ref="printDialogRef" :title="'打印出库流水'" @reloadTable="handleQuery" />
      <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
    </el-card>
  </div>
</template>
<style scoped>
:deep(.el-input), :deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>