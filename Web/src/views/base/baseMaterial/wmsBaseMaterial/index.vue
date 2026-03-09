<script lang="ts" setup name="wmsBaseMaterial">
import { ref, reactive, onMounted, onUnmounted } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage, ElNotification } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useWmsBaseMaterialApi } from '/@/api/base/baseMaterial/wmsBaseMaterial';
import { useWmsBaseMaterialPresetApi } from '/@/api/base/baseMaterialPreset/wmsBaseMaterialPreset';
import { SysNoticeApi } from '/@/api-services/api';
import { AddNoticeInput } from '/@/api-services/models';
import { getAPI } from '/@/utils/axios-utils';
import editDialog from '/@/views/base/baseMaterial/wmsBaseMaterial/component/editDialog.vue'
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import ModifyRecord from '/@/components/table/modifyRecord.vue';
import ImportData from "/@/components/table/importData.vue";
import approvalDialog from '/@/views/base/baseMaterial/wmsBaseMaterial/component/approvalDialog.vue'

const wmsBaseMaterialApi = useWmsBaseMaterialApi();
const wmsBaseMaterialPresetApi = useWmsBaseMaterialPresetApi();
const printDialogRef = ref();
const editDialogRef = ref();
const importDataRef = ref();
const approvalDialogRef = ref();
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
  pendingApprovalCount: 0, // 待审核数量
  refreshTimer: null as any, // 定时器
  isInitialized: false, // 是否已初始化
});

// 页面加载时
onMounted(async () => {
  const data = await wmsBaseMaterialApi.getDropdownData(true).then(res => res.data.result) ?? {};
  state.dropdownData.materialUnit = data.materialUnit;
  state.dropdownData.materialAreaId = data.materialAreaId;
  state.dropdownData.warehouseId = data.warehouseId;
  
  // 获取待审核数量
  await getPendingApprovalCount();
  state.isInitialized = true;
  
  // 设置定时刷新待审核数量（每3秒刷新一次）
  state.refreshTimer = setInterval(() => {
    getPendingApprovalCount();
  }, 3000);
});

    // 页面卸载时
    onUnmounted(() => {
        // 清除定时器
        if (state.refreshTimer) {
            clearInterval(state.refreshTimer);
        }
    });

    // 获取待审核数量
    const getPendingApprovalCount = async () => {
        try {
            const queryParams = { approvalStatus: 0, page: 1, pageSize: 1 };
            const result = await wmsBaseMaterialPresetApi.page(queryParams).then(res => res.data.result);
            const newCount = result?.total || 0;

            // 初次加载时，如果有待审核数据，提示一次
            if (!state.isInitialized && newCount > 0) {
                ElNotification({
                    title: '待审核通知',
                    message: `当前有 ${newCount} 条物料信息申请待审核`,
                    type: 'info',
                    duration: 5000,
                    position: 'top-right'
                });
            }
            // 检查是否有新数据进来（只在初始化后才有新数据的概念）
            else if (state.isInitialized && newCount > state.pendingApprovalCount) {
                const diff = newCount - state.pendingApprovalCount;
                ElNotification({
                    title: '新申请通知',
                    message: `有 ${diff} 条新的物料信息申请待审核`,
                    type: 'info',
                    duration: 5000,
                    position: 'top-right'
                });

                // 刷新审核弹窗列表（如果弹窗已打开）
                if (approvalDialogRef.value) {
                    approvalDialogRef.value.refreshTable();
                }
            }

            state.pendingApprovalCount = newCount;
        } catch (error) {
            console.error('获取待审核数量失败:', error);
            state.pendingApprovalCount = 0;
        }
    };

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsBaseMaterialApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
  state.tableParams.total = result?.total;
  state.tableData = result?.items ?? [];
  state.tableLoading = false;
  // 不在这里调用 getPendingApprovalCount，避免重复调用
};

// 列排序
const sortChange = async (column: any) => {
  state.tableParams.field = column.prop;
  state.tableParams.order = column.order;
  await handleQuery();
};

// 删除
const delWmsBaseMaterial = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsBaseMaterialApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => {});
};

// 批量删除
const batchDelWmsBaseMaterial = () => {
  ElMessageBox.confirm(`确定要删除${state.selectData.length}条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsBaseMaterialApi.batchDelete(state.selectData.map(u => ({ id: u.id }) )).then(res => {
      ElMessage.success(`成功批量删除${res.data.result}条记录`);
      handleQuery();
    });
  }).catch(() => {});
};

// 导出数据
const exportWmsBaseMaterialCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    if (command === 'select') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await wmsBaseMaterialApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams);
      await wmsBaseMaterialApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await wmsBaseMaterialApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
}

handleQuery();
</script>
<template>
  <div class="wmsBaseMaterial-container" v-loading="state.exportLoading">
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
            <el-form-item label="物料助记码">
              <el-input v-model="state.tableQueryParams.materialMcode" clearable placeholder="请输入物料助记码"/>
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
            <el-form-item label="物料型号">
              <el-input v-model="state.tableQueryParams.materialModel" clearable placeholder="请输入物料型号"/>
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
            <el-form-item label="有效期1">
              <el-input v-model="state.tableQueryParams.materialValidityDay1" clearable placeholder="请输入有效期1"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="有效期2">
              <el-input v-model="state.tableQueryParams.materialValidityDay2" clearable placeholder="请输入有效期2"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="有效期3">
              <el-input v-model="state.tableQueryParams.materialValidityDay3" clearable placeholder="请输入有效期3"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="温度">
              <el-input v-model="state.tableQueryParams.materialTemp" clearable placeholder="请输入温度"/>
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
            <el-form-item label="外部物品编码">
              <el-input v-model="state.tableQueryParams.outerInnerCode" clearable placeholder="请输入外部物品编码"/>
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
            <el-form-item label="授权编码">
              <el-input v-model="state.tableQueryParams.token" clearable placeholder="请输入授权编码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="授权用户">
              <el-input v-model="state.tableQueryParams.accountExec" clearable placeholder="请输入授权用户"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="启用状态">
                  <el-select clearable filterable v-model="state.tableQueryParams.status" placeholder="请选择启用状态"> 
                    <el-option     value="true" label="是" /> 
                    <el-option     value="false" label="否" />  
                  </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="是否空托">
                  <el-select clearable filterable v-model="state.tableQueryParams.isEmpty" placeholder="请选择是否空托"> 
                    <el-option     value="true" label="是" /> 
                    <el-option     value="false" label="否" />  
                  </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item >
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'wmsBaseMaterial:page'" v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true" v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                <el-button icon="ele-ZoomOut" @click="() => state.showAdvanceQueryUI = false" v-if="state.showAdvanceQueryUI" style="margin-left:5px;"> 隐藏 </el-button>
                <el-button type="danger" style="margin-left:5px;" icon="ele-Delete" @click="batchDelWmsBaseMaterial" :disabled="state.selectData.length == 0" v-auth="'wmsBaseMaterial:batchDelete'"> 删除 </el-button>
                <el-button type="primary" style="margin-left:5px;" icon="ele-Plus" @click="editDialogRef.openDialog(null, '新增物料信息')" v-auth="'wmsBaseMaterial:add'"> 新增 </el-button>
                <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportWmsBaseMaterialCommand">
                  <el-button type="primary" style="margin-left:5px;" icon="ele-FolderOpened" v-reclick="20000" v-auth="'wmsBaseMaterial:export'"> 导出 </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="select" :disabled="state.selectData.length == 0">导出选中</el-dropdown-item>
                      <el-dropdown-item command="current">导出本页</el-dropdown-item>
                      <el-dropdown-item command="all">导出全部</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
                <el-button type="warning" style="margin-left:5px;" icon="ele-MostlyCloudy" @click="importDataRef.openDialog()" v-auth="'wmsBaseMaterial:import'"> 导入 </el-button>
                <el-button type="success" style="margin-left:5px;" icon="ele-Check" @click="approvalDialogRef.openDialog()" v-auth="'wmsBaseMaterialPreset:auth'" v-if="state.pendingApprovalCount > 0">
                  审核({{ state.pendingApprovalCount }})
                </el-button>
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
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
        <el-table-column prop='isEmpty' label='是否空托' show-overflow-tooltip>
          <template #default="scope">
            <el-tag v-if="scope.row.isEmpty"> 是 </el-tag>
            <el-tag type="danger" v-else> 否 </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
          <template #default="scope">
            <ModifyRecord :data="scope.row" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip v-if="auth('wmsBaseMaterial:update') || auth('wmsBaseMaterial:delete')">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text type="primary" @click="editDialogRef.openDialog(scope.row, '编辑物料信息')" v-auth="'wmsBaseMaterial:update'"> 编辑 </el-button>
            <el-button icon="ele-Delete" size="small" text type="primary" @click="delWmsBaseMaterial(scope.row)" v-auth="'wmsBaseMaterial:delete'"> 删除 </el-button>
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
      <ImportData ref="importDataRef" :import="wmsBaseMaterialApi.importData" :download="wmsBaseMaterialApi.downloadTemplate" v-auth="'wmsBaseMaterial:import'" @refresh="handleQuery"/>
      <printDialog ref="printDialogRef" :title="'打印物料信息'" @reloadTable="handleQuery" />
      <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
      <approvalDialog ref="approvalDialogRef" @refresh="getPendingApprovalCount" @refreshMaterialList="handleQuery" />
    </el-card>
  </div>
</template>
<style scoped>
:deep(.el-input), :deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>