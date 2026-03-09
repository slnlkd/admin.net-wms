<script lang="ts" name="approvalDialog" setup>
import { ref, reactive, onMounted, onUnmounted } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { useWmsBaseSupplierPresetApi } from '/@/api/base/supplierPreset/wmsBaseSupplierPreset';
import { SysNoticeApi } from '/@/api-services/api';
import { AddNoticeInput } from '/@/api-services/models';
import { getAPI } from '/@/utils/axios-utils';

const wmsBaseSupplierPresetApi = useWmsBaseSupplierPresetApi();

const state = reactive({
  dialogVisible: false,
  dialogTitle: '申请供应商信息审核',
  tableLoading: false,
  tableQueryParams: {} as any,
  tableParams: {
    page: 1,
    pageSize: 20,
    total: 0,
    field: 'createTime',
    order: 'descending',
    descStr: 'descending',
  },
  tableData: [] as any[],
  selectedRows: [] as any[],
  refreshTimer: null as any, // 定时器
  lastTotal: 0, // 上次总数
});

// 打开弹窗
const openDialog = () => {
  state.dialogVisible = true;
  handleQuery({}, true); // 打开时显示loading
  // 启动定时刷新（每3秒刷新一次）
  state.refreshTimer = setInterval(() => {
    if (state.dialogVisible) {
      handleQuery({}, false); // 定时刷新时不显示loading
    }
  }, 3000);
};

// 刷新列表数据（供外部调用）
const refreshTable = () => {
  if (state.dialogVisible) {
    handleQuery({}, true); // 外部刷新时显示loading
  }
};

// 查询操作
const handleQuery = async (params: any = {}, showLoading: boolean = true) => {
  // 如果是定时刷新，不显示loading
  if (showLoading) {
    state.tableLoading = true;
  }
  
  state.tableParams = Object.assign(state.tableParams, params);

  // 默认只查询待审核的记录
  const queryParams = Object.assign({}, state.tableQueryParams, state.tableParams, { approvalStatus: 0 });

  try {
    const result = await wmsBaseSupplierPresetApi.page(queryParams).then(res => res.data.result);
    const newTotal = result?.total || 0;
    const newItems = result?.items ?? [];
    
    // 检查数据是否有变化（总数变化或内容变化）
    let hasChanged = false;
    
    // 检查总数是否变化
    if (newTotal !== state.lastTotal) {
      hasChanged = true;
      state.lastTotal = newTotal;
    }
    
    // 检查内容是否变化（通过比较ID列表）
    if (!hasChanged) {
      const currentIds = state.tableData.map((item: any) => item.id).sort();
      const newIds = newItems.map((item: any) => item.id).sort();
      
      if (JSON.stringify(currentIds) !== JSON.stringify(newIds)) {
        hasChanged = true;
      }
    }
    
    // 只有数据变化时才更新表格
    if (hasChanged) {
      state.tableParams.total = newTotal;
      state.tableData = newItems;
      
      // 如果数据减少，清空已选择的行
      if (newTotal < state.lastTotal) {
        state.selectedRows = [];
      }
    }
  } catch (error) {
    console.error('查询失败:', error);
  } finally {
    if (showLoading) {
      state.tableLoading = false;
    }
  }
};

// 列排序
const sortChange = async (column: any) => {
  state.tableParams.field = column.prop;
  state.tableParams.order = column.order;
  await handleQuery();
};

// 审核供应商信息
const approvalSupplier = (row: any, approvalStatus: any) => {
  const actionText = approvalStatus === 1 ? '审核通过' : '驳回';
  if (approvalStatus === 2) {
    // 如果是驳回，显示输入框让用户输入驳回原因
    ElMessageBox.prompt('请输入驳回原因', '驳回申请', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
      inputPattern: /^.{1,500}$/,
      inputErrorMessage: '驳回原因不能为空且不超过500字符'
    }).then(async ({ value }) => {
      try {
        await wmsBaseSupplierPresetApi.approvalSupplier(row.id, approvalStatus, value);
        // 发送消息通知给申请人
        await sendApprovalNotification(row, approvalStatus);
        ElMessage.success("操作成功");
        handleQuery(); // 重新查询
      } catch (error: any) {
        console.error('审核失败:', error);
        if (error.message && error.message.includes('不存在')) {
          ElMessage.error("该申请已不存在，已自动刷新列表");
          handleQuery();
        } else {
          ElMessage.error(error.message || "操作失败");
        }
      }
    }).catch(() => { });
  } else {
    ElMessageBox.confirm(`确定要${actionText}吗?`, "提示", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning",
    }).then(async () => {
      try {
        await wmsBaseSupplierPresetApi.approvalSupplier(row.id, approvalStatus);

        // 发送消息通知给申请人
        await sendApprovalNotification(row, approvalStatus);

        ElMessage.success("操作成功");
        handleQuery(); // 重新查询

        // 如果审核通过，通知父组件刷新供应商信息列表
        if (approvalStatus === 1) {
          emit('refreshSupplierList');
        }
      } catch (error: any) {
        console.error('审核失败:', error);
        if (error.message && error.message.includes('不存在')) {
          ElMessage.error("该申请已不存在，已自动刷新列表");
          handleQuery();
        } else {
          ElMessage.error(error.message || "操作失败");
        }
      }
    }).catch(() => { });
  }
};

// 批量审核供应商信息
const batchApprovalSupplier = (approvalStatus: any) => {
  if (state.selectedRows.length === 0) {
    ElMessage.warning("请先选择要审核的记录");
    return;
  }

  const actionText = approvalStatus === 1 ? '批量审核通过' : '批量驳回';
  const ids = state.selectedRows.map((row: any) => row.id);

  if (approvalStatus === 2) {
    // 如果是驳回，显示输入框让用户输入驳回原因
    ElMessageBox.prompt('请输入驳回原因', '批量驳回申请', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
      inputPattern: /^.{1,500}$/,
      inputErrorMessage: '驳回原因不能为空且不超过500字符'
    }).then(async ({ value }) => {
      try {
        await wmsBaseSupplierPresetApi.batchApprovalSupplier(ids, approvalStatus, value);
        // 批量发送消息通知给申请人
        for (const row of state.selectedRows) {
          await sendApprovalNotification(row, approvalStatus);
        }
        ElMessage.success(`成功${actionText}`);
        state.selectedRows = [];
        handleQuery(); // 重新查询

        // 如果审核通过，通知父组件刷新供应商信息列表
        if (approvalStatus === 1) {
          emit('refreshSupplierList');
        }
      } catch (error: any) {
        console.error('批量审核失败:', error);
        if (error.message && error.message.includes('不存在')) {
          ElMessage.error("部分申请已不存在，已自动刷新列表");
          handleQuery();
        } else {
          ElMessage.error(error.message || "操作失败");
        }
      }
    }).catch(() => { });
  } else {
    ElMessageBox.confirm(`确定要${actionText}选中的 ${state.selectedRows.length} 条记录吗?`, "提示", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning",
    }).then(async () => {
      try {
        await wmsBaseSupplierPresetApi.batchApprovalSupplier(ids, approvalStatus);

        // 批量发送消息通知给申请人
        for (const row of state.selectedRows) {
          await sendApprovalNotification(row, approvalStatus);
        }

        ElMessage.success(`成功${actionText}`);
        state.selectedRows = [];
        handleQuery(); // 重新查询

        // 如果审核通过，通知父组件刷新供应商信息列表
        if (approvalStatus === 1) {
          emit('refreshSupplierList');
        }
      } catch (error: any) {
        console.error('批量审核失败:', error);
        if (error.message && error.message.includes('不存在')) {
          ElMessage.error("部分申请已不存在，已自动刷新列表");
          handleQuery();
        } else {
          ElMessage.error(error.message || "操作失败");
        }
      }
    }).catch(() => { });
  }
};

// 表格选择变化
const handleSelectionChange = (selection: any[]) => {
  state.selectedRows = selection;
};

// 发送审核通知
const sendApprovalNotification = async (row: any, approvalStatus: any) => {
  try {
    const supplierName = row.customerName || row.customerCode || '未知供应商';
    const applicantName = row.createUserName || '未知申请人';
    const statusText = approvalStatus === 1 ? '审核通过' : '被驳回';
    const currentTime = new Date().toISOString();

    const noticeData: AddNoticeInput = {
      title: `供应商信息审核${statusText}`,
      content: `申请人【${applicantName}】申请的供应商信息【${supplierName}】（编码：${row.customerCode}）已${statusText}。`,
      type: 1, // 通知类型：通知
      status: 1, // 已发布状态
      publicTime: new Date(currentTime), // 发布时间
      publicUserName: '系统管理员', // 发布人
      receiverUserIds: [row.createUserId], // 接收人ID数组
    };

    // 添加并自动发布通知
    await getAPI(SysNoticeApi).apiSysNoticeAddPostByUser(noticeData);

  } catch (error) {
    console.error('发送通知失败:', error);
    // 通知发送失败不影响主要审核流程
  }
};

// 关闭弹窗
const closeDialog = () => {
  state.dialogVisible = false;
  state.tableQueryParams = {};
  state.tableParams = {
    page: 1,
    pageSize: 20,
    total: 0,
    field: 'createTime',
    order: 'descending',
    descStr: 'descending',
  };
  state.tableData = [];
  state.selectedRows = [];
  state.lastTotal = 0;
  // 清除定时器
  if (state.refreshTimer) {
    clearInterval(state.refreshTimer);
    state.refreshTimer = null;
  }
};

// 页面卸载时清除定时器
onUnmounted(() => {
  if (state.refreshTimer) {
    clearInterval(state.refreshTimer);
    state.refreshTimer = null;
  }
});

const emit = defineEmits(['refresh', 'refreshSupplierList']);

defineExpose({
  openDialog,
  refreshTable,
});
</script>
<template>
  <el-dialog v-model="state.dialogVisible" :title="state.dialogTitle" width="90%" top="5vh" destroy-on-close
    @close="closeDialog">
    <div class="approval-dialog-container">
      <el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
        <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
          <el-row>
            <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="6" class="mb10">
              <el-form-item label="关键字">
                <el-input v-model="state.tableQueryParams.keyword" clearable placeholder="请输入模糊查询关键字"
                  @keyup.enter="handleQuery" />
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="6" class="mb10">
              <el-form-item label="供应商编码">
                <el-input v-model="state.tableQueryParams.customerCode" clearable placeholder="请输入供应商编码"
                  @keyup.enter="handleQuery" />
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="6" class="mb10">
              <el-form-item label="供应商名称">
                <el-input v-model="state.tableQueryParams.customerName" clearable placeholder="请输入供应商名称"
                  @keyup.enter="handleQuery" />
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="6" class="mb10">
              <el-form-item>
                <el-button-group style="display: flex; align-items: center;">
                  <el-button type="primary" icon="ele-Search" @click="handleQuery" v-auth="'wmsBaseSupplierPreset:page'"
                    v-reclick="1000"> 查询 </el-button>
                  <el-button icon="ele-Refresh" @click="() => { state.tableQueryParams = {}; handleQuery(); }"> 重置
                  </el-button>
                </el-button-group>
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </el-card>

      <el-card class="full-table" shadow="hover" style="margin-top: 5px">
        <div class="batch-actions">
          <div class="batch-actions-left">
            <span class="selected-count">已选择 {{ state.selectedRows.length }} 条记录</span>
          </div>
          <div class="batch-actions-right">
            <el-button-group>
              <el-button type="success" icon="ele-Check" @click="batchApprovalSupplier(1)"
                v-auth="'wmsBaseSupplierPreset:auth'" :disabled="state.selectedRows.length === 0">
                批量通过
              </el-button>
              <el-button type="danger" icon="ele-Close" @click="batchApprovalSupplier(2)"
                v-auth="'wmsBaseSupplierPreset:auth'" :disabled="state.selectedRows.length === 0">
                批量驳回
              </el-button>
              <el-button icon="ele-RefreshLeft" @click="state.selectedRows = []"
                :disabled="state.selectedRows.length === 0">
                取消选择
              </el-button>
            </el-button-group>
          </div>
        </div>
        <el-table :data="state.tableData" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light"
          row-key="id" @sort-change="sortChange" @selection-change="handleSelectionChange" border height="60vh">
          <el-table-column :fixed="true" type="selection" width="55" align="center" />
          <el-table-column :fixed="true" type="index" label="序号" width="55" align="center" />
          <el-table-column :fixed="true" align="center" width="155" prop='customerCode' label='供应商编码'
            show-overflow-tooltip />
          <el-table-column align="center" width="200" prop='customerName' label='供应商名称' show-overflow-tooltip />
          <el-table-column align="center" width="200" prop='customerAddress' label='供应商地址' show-overflow-tooltip />
          <el-table-column align="center" width="120" prop='customerLinkman' label='联系人' show-overflow-tooltip />
          <el-table-column align="center" width="120" prop='customerPhone' label='联系电话' show-overflow-tooltip />
          <el-table-column align="center" prop='remark' label='备注' show-overflow-tooltip />
          <el-table-column align="center" prop='createUserName' label='创建者姓名' show-overflow-tooltip />
          <el-table-column align="center" width="155" prop='createTime' label='创建时间' show-overflow-tooltip />
          <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip
            v-if="auth('wmsBaseSupplierPreset:auth')">
            <template #default="scope">
              <el-button icon="ele-Check" size="small" text type="success" @click="approvalSupplier(scope.row, 1)"
                v-auth="'wmsBaseSupplierPreset:auth'"> 通过 </el-button>
              <el-button icon="ele-Close" size="small" text type="danger" @click="approvalSupplier(scope.row, 2)"
                v-auth="'wmsBaseSupplierPreset:auth'"> 驳回 </el-button>
            </template>
          </el-table-column>
        </el-table>
        <el-pagination v-model:currentPage="state.tableParams.page" v-model:page-size="state.tableParams.pageSize"
          @size-change="(val: any) => handleQuery({ pageSize: val })"
          @current-change="(val: any) => handleQuery({ page: val })" layout="total, sizes, prev, pager, next, jumper"
          :page-sizes="[10, 20, 50, 100]" :total="state.tableParams.total" size="small" background />
      </el-card>
    </div>
  </el-dialog>
</template>
<style scoped>
.approval-dialog-container {
  height: 70vh;
  display: flex;
  flex-direction: column;
}

:deep(.el-input),
:deep(.el-select),
:deep(.el-input-number) {
  width: 100%;
}

.full-table {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.full-table :deep(.el-table) {
  flex: 1;
}

.full-table :deep(.el-pagination) {
  margin-top: 10px;
}

.batch-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  margin-bottom: 10px;
  background-color: #f5f7fa;
  border-radius: 4px;
  border: 1px solid #e4e7ed;
}

.batch-actions-left {
  display: flex;
  align-items: center;
}

.selected-count {
  font-size: 14px;
  color: #606266;
  font-weight: 500;
}

.batch-actions-right {
  display: flex;
  align-items: center;
}

.batch-actions :deep(.el-button-group .el-button) {
  transition: all 0.3s;
}

.batch-actions :deep(.el-button.is-disabled) {
  opacity: 0.5;
}
</style>