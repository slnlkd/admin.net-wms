<script lang="ts" setup name="wmsPasswordPolicy">
import { ref, reactive, onMounted, computed } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useWmsPasswordPolicyApi } from '/@/api/system/pwdPolicy/wmsPasswordPolicy';
import editDialog from '/@/views/system/pwdPolicy/wmsPasswordPolicy/component/editDialog.vue'
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import ModifyRecord from '/@/components/table/modifyRecord.vue';
import { SuccessFilled, CloseBold } from '@element-plus/icons-vue'
import ImportData from "/@/components/table/importData.vue";

const wmsPasswordPolicyApi = useWmsPasswordPolicyApi();
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
    field: 'createTime',
    order: 'descending',
    descStr: 'descending',
  },
  tableData: [],
});

// 计算属性：选中的数量
const selectedCount = computed(() => state.selectData.length);

// 页面加载时
onMounted(async () => {
  await handleQuery();
});

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  try {
    const result = await wmsPasswordPolicyApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
    state.tableParams.total = result?.total || 0;
    state.tableData = result?.items || [];
  } catch (error) {
    state.tableData = [];
    state.tableParams.total = 0;
  } finally {
    state.tableLoading = false;
  }
};

// 重置查询
const resetQuery = () => {
  state.tableQueryParams = {};
  state.tableParams.page = 1;
  handleQuery();
};

// 列排序
const sortChange = async (column: any) => {
  if (column.prop) {
    state.tableParams.field = column.prop;
    state.tableParams.order = column.order;
    await handleQuery();
  }
};

// 删除
const delWmsPasswordPolicy = (row: any) => {
  ElMessageBox.confirm(`确定要删除策略"${row.name}"吗？`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await wmsPasswordPolicyApi.delete({ id: row.id });
    ElMessage.success("删除成功");
    handleQuery();
  }).catch(() => { });
};

// 批量删除
const batchDelWmsPasswordPolicy = () => {
  if (selectedCount.value === 0) return;

  const policyNames = state.selectData.slice(0, 3).map(u => u.name).join('、');
  const moreText = selectedCount.value > 3 ? `等${selectedCount.value}个策略` : '';

  ElMessageBox.confirm(`确定要删除${policyNames}${moreText}吗？`, "批量删除确认", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    const result = await wmsPasswordPolicyApi.batchDelete(state.selectData.map(u => ({ id: u.id }))).then(res => res.data.result);
    ElMessage.success(`成功删除${result}条记录`);
    state.selectData = [];
    handleQuery();
  }).catch(() => { });
};

// 导出数据
const exportWmsPasswordPolicyCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    let params = {};

    if (command === 'select') {
      if (selectedCount.value === 0) {
        ElMessage.warning('请先选择要导出的数据');
        return;
      }
      params = Object.assign({}, state.tableQueryParams, state.tableParams, {
        selectKeyList: state.selectData.map(u => u.id)
      });
    } else if (command === 'current') {
      params = Object.assign({}, state.tableQueryParams, state.tableParams);
    } else if (command === 'all') {
      params = Object.assign({}, state.tableQueryParams, {
        page: 1,
        pageSize: 99999999
      });
    }

    await wmsPasswordPolicyApi.exportData(params).then(res => downloadStreamFile(res));
    ElMessage.success('导出成功');
  } catch (error) {
    ElMessage.error('导出失败');
  } finally {
    state.exportLoading = false;
  }
}

// 快速启用/停用
// 启用/停用策略
const toggleEnable = async (row: any, newStatus: boolean) => {
  try {
    await ElMessageBox.confirm(
      `确定要${newStatus ? '启用' : '停用'}策略"${row.name}"吗？`,
      '提示',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
      }
    );
    
    await wmsPasswordPolicyApi.update({
      ...row,
      enabled: newStatus
    });
    
    ElMessage.success(newStatus ? '已启用' : '已停用');
    handleQuery(); // 重新加载数据确保状态同步
  } catch (error) {
    // 用户取消或请求失败，恢复开关状态
    row.enabled = !newStatus;
  }
};

defineExpose({ handleQuery });
</script>

<template>
  <div class="wmsPasswordPolicy-container" v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
      <el-form :model="state.tableQueryParams" ref="queryForm" label-width="90">
        <el-row>
          <!-- 基础查询条件 -->
          <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6" class="mb10">
            <el-form-item label="策略名称">
              <el-input v-model="state.tableQueryParams.name" clearable placeholder="请输入策略名称"
                @keyup.enter="handleQuery" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6" class="mb10">
            <el-form-item label="策略状态">
              <el-select v-model="state.tableQueryParams.enabled" clearable placeholder="请选择状态" style="width: 100%">
                <el-option label="启用" value="true" />
                <el-option label="停用" value="false" />
              </el-select>
            </el-form-item>
          </el-col>

          <!-- 高级查询条件 -->
          <template v-if="state.showAdvanceQueryUI">
            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6" class="mb10">
              <el-form-item label="最小长度">
                <el-input-number v-model="state.tableQueryParams.minLength" clearable placeholder="最小长度" :min="6"
                  style="width: 100%" />
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6" class="mb10">
              <el-form-item label="包含小写字母">
                <el-select v-model="state.tableQueryParams.requireLowercase" clearable placeholder="请选择"
                  style="width: 100%">
                  <el-option label="是" value="true" />
                  <el-option label="否" value="false" />
                </el-select>
              </el-form-item>
            </el-col>
          </template>

          <el-col :xs="24" :sm="24" :md="24" :lg="12" :xl="12" class="mb10">
            <el-form-item>
              <el-button-group>
                <el-button type="primary" icon="ele-Search" @click="handleQuery" v-auth="'wmsPasswordPolicy:page'">
                  查询
                </el-button>
                <el-button icon="ele-Refresh" @click="resetQuery">
                  重置
                </el-button>
                <el-button :icon="state.showAdvanceQueryUI ? 'ele-ZoomOut' : 'ele-ZoomIn'"
                  @click="state.showAdvanceQueryUI = !state.showAdvanceQueryUI">
                  {{ state.showAdvanceQueryUI ? '隐藏' : '高级' }}
                </el-button>
              </el-button-group>

              <el-button type="primary" icon="ele-Plus" @click="editDialogRef.openDialog(null, '新增密码策略')"
                v-auth="'wmsPasswordPolicy:add'" style="margin-left: 10px;">
                新增
              </el-button>

              <el-dropdown @command="exportWmsPasswordPolicyCommand" style="margin-left: 10px;">
                <el-button type="primary" icon="ele-FolderOpened" v-auth="'wmsPasswordPolicy:export'">
                  导出
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item command="select" :disabled="selectedCount === 0">
                      导出选中({{ selectedCount }})
                    </el-dropdown-item>
                    <el-dropdown-item command="current">导出本页</el-dropdown-item>
                    <el-dropdown-item command="all">导出全部</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>

              <el-button type="warning" icon="ele-MostlyCloudy" @click="importDataRef.openDialog()"
                v-auth="'wmsPasswordPolicy:import'" style="margin-left: 10px;">
                导入
              </el-button>

              <el-button type="danger" icon="ele-Delete" @click="batchDelWmsPasswordPolicy"
                :disabled="selectedCount === 0" v-auth="'wmsPasswordPolicy:batchDelete'" style="margin-left: 10px;">
                删除({{ selectedCount }})
              </el-button>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>

    <el-card class="full-table" shadow="hover" style="margin-top: 8px">
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }"
        v-loading="state.tableLoading" @sort-change="sortChange" border stripe style="width: 100%">
        <el-table-column type="selection" width="48" align="center"
          v-if="auth('wmsPasswordPolicy:batchDelete') || auth('wmsPasswordPolicy:export')" />
        <el-table-column type="index" label="序号" width="60" align="center" fixed="left" />

        <el-table-column prop="name" label="策略名称" min-width="120" fixed="left" show-overflow-tooltip>
          <template #default="scope">
            <div class="policy-name">
              <span style="margin-left: 8px;">{{ scope.row.name }}</span>
            </div>
          </template>
        </el-table-column>

        <el-table-column prop="description" label="策略描述" min-width="150" show-overflow-tooltip />

        <el-table-column label="启用状态" width="120" align="center" fixed="right">
          <template #header>
            <span>启用状态</span>
            <el-tooltip content="只能启用一种策略，启用后将覆盖其他已启用的策略" placement="top">
              <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
            </el-tooltip>
          </template>
          <template #default="scope">
            <el-switch
              v-model="scope.row.enabled"
              @change="(value: boolean) => toggleEnable(scope.row, value)"
              active-text="启"
              inactive-text="停"
              inline-prompt
              style="--el-switch-on-color: #13ce66; --el-switch-off-color: #ff4949"
            />
          </template>
        </el-table-column>

        <el-table-column label="密码长度" width="100" align="center">
          <template #header>
            <span>密码长度</span>
            <el-tooltip content="普：普通用户最小密码长度；管：管理员用户最小密码长度" placement="top">
              <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
            </el-tooltip>
          </template>
          <template #default="scope">
            <div>普:{{ scope.row.minLength }}</div>
            <div>管:{{ scope.row.adminMinLength }}</div>
          </template>
        </el-table-column>

        <el-table-column label="复杂度" width="120" align="center">
          <template #header>
            <span>复杂度</span>
            <el-tooltip content="密码需要包含的字符类型数量（如小写字母、大写字母、数字、特殊字符等）" placement="top">
              <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
            </el-tooltip>
          </template>
          <template #default="scope">
            <el-tooltip :content="`需要${scope.row.requireCategories}种字符类型`" placement="top">
              <el-tag type="warning" size="small">
                {{ scope.row.requireCategories }}类
              </el-tag>
            </el-tooltip>
          </template>
        </el-table-column>

        <el-table-column label="字符要求" width="200" show-overflow-tooltip>
          <template #header>
            <span>字符要求</span>
            <el-tooltip content="密码中必须包含的字符类型" placement="top">
              <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
            </el-tooltip>
          </template>
          <template #default="scope">
            <div class="char-requirements">
              <el-tag v-if="scope.row.requireLowercase" size="small" effect="plain">小写</el-tag>
              <el-tag v-if="scope.row.requireUppercase" size="small" effect="plain">大写</el-tag>
              <el-tag v-if="scope.row.requireDigit" size="small" effect="plain">数字</el-tag>
              <el-tag v-if="scope.row.requireSpecial" size="small" effect="plain">特殊</el-tag>
            </div>
          </template>
        </el-table-column>

        <el-table-column label="安全检查" width="200" show-overflow-tooltip>
          <template #header>
            <span>安全检查</span>
            <el-tooltip content="密码安全性的额外检查项" placement="top">
              <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
            </el-tooltip>
          </template>
          <template #default="scope">
            <div class="security-checks">
              <!-- 检查用户名 -->
              <el-tooltip content="检查密码不能包含用户名" placement="top">
                <div class="check-item">
                  <el-icon v-if="scope.row.checkUserId" color="#67C23A">
                    <SuccessFilled />
                  </el-icon>
                  <el-icon v-else color="#F56C6C">
                    <CloseBold />
                  </el-icon>
                  <span class="check-label">用户</span>
                </div>
              </el-tooltip>

              <!-- 检查连续字符 -->
              <el-tooltip content="检查连续字符（如abc、123等）" placement="top">
                <div class="check-item">
                  <el-icon v-if="scope.row.checkConsecutive" color="#67C23A">
                    <SuccessFilled />
                  </el-icon>
                  <el-icon v-else color="#F56C6C">
                    <CloseBold />
                  </el-icon>
                  <span class="check-label">连续</span>
                </div>
              </el-tooltip>

              <!-- 检查常见模式 -->
              <el-tooltip content="检查常见模式（如123456、qwerty等）" placement="top">
                <div class="check-item">
                  <el-icon v-if="scope.row.checkCommonPatterns" color="#67C23A">
                    <SuccessFilled />
                  </el-icon>
                  <el-icon v-else color="#F56C6C">
                    <CloseBold />
                  </el-icon>
                  <span class="check-label">常见</span>
                </div>
              </el-tooltip>

              <!-- 检查重复字符 -->
              <el-tooltip content="检查重复字符（如aaa、111等）" placement="top">
                <div class="check-item">
                  <el-icon v-if="scope.row.checkRepeatedChars" color="#67C23A">
                    <SuccessFilled />
                  </el-icon>
                  <el-icon v-else color="#F56C6C">
                    <CloseBold />
                  </el-icon>
                  <span class="check-label">重复</span>
                </div>
              </el-tooltip>
            </div>
          </template>
        </el-table-column>

        <el-table-column label="密码策略" width="120" align="center">
          <template #header>
            <span>密码策略</span>
            <el-tooltip content="历史：记住的历史密码数量；强制修改：用户首次登录时是否强制修改密码" placement="top">
              <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
            </el-tooltip>
          </template>
          <template #default="scope">
            <div>历史:{{ scope.row.rememberPasswords }}</div>
            <div v-if="scope.row.forceChangeInitial">
              <el-tag size="small" type="warning">强制修改</el-tag>
            </div>
          </template>
        </el-table-column>

        <el-table-column label="过期天数" width="100" align="center">
          <template #header>
            <span>过期天数</span>
            <el-tooltip content="普：普通用户密码过期天数；管：管理员用户密码过期天数" placement="top">
              <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
            </el-tooltip>
          </template>
          <template #default="scope">
            <div>普:{{ scope.row.passwordExpiryDays }}天</div>
            <div>管:{{ scope.row.adminPasswordExpiryDays }}天</div>
          </template>
        </el-table-column>

        <el-table-column prop="expiryNoticeDays" label="提前提醒" width="100" align="center">
          <template #header>
            <span>提前提醒</span>
            <el-tooltip content="密码过期前多少天开始提醒用户" placement="top">
              <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
            </el-tooltip>
          </template>
          <template #default="scope">
            {{ scope.row.expiryNoticeDays }}天
          </template>
        </el-table-column>

        <el-table-column label="修改记录" width="90" align="center">
          <template #default="scope">
            <ModifyRecord :data="scope.row" />
          </template>
        </el-table-column>

        <el-table-column label="操作" width="140" align="center" fixed="right">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text type="primary"
              @click="editDialogRef.openDialog(scope.row, '编辑密码策略')" v-auth="'wmsPasswordPolicy:update'">
              编辑
            </el-button>

            <el-button icon="ele-Delete" size="small" text type="primary"
              @click="delWmsPasswordPolicy(scope.row)" v-auth="'wmsPasswordPolicy:delete'">
              删除
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <el-pagination v-model:currentPage="state.tableParams.page" v-model:page-size="state.tableParams.pageSize"
        :total="state.tableParams.total" :page-sizes="[10, 20, 50, 100]"
        layout="total, sizes, prev, pager, next, jumper" @size-change="(val: any) => handleQuery({ pageSize: val })"
        @current-change="(val: any) => handleQuery({ page: val })"
        style="margin-top: 16px; justify-content: flex-end;" />

      <ImportData ref="importDataRef" :import="wmsPasswordPolicyApi.importData"
        :download="wmsPasswordPolicyApi.downloadTemplate" v-auth="'wmsPasswordPolicy:import'" @refresh="handleQuery" />
      <printDialog ref="printDialogRef" :title="'打印密码策略'" @reloadTable="handleQuery" />
      <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
    </el-card>
  </div>
</template>

<style scoped>
.wmsPasswordPolicy-container {

  :deep(.el-input),
  :deep(.el-select),
  :deep(.el-input-number) {
    width: 100%;
  }
}

.policy-name {
  display: flex;
  align-items: center;
}

.char-requirements {
  display: flex;
  gap: 4px;
  flex-wrap: wrap;
}

.security-checks {
  display: flex;
  align-items: center;
  justify-content: space-around;
  gap: 8px;
}

.check-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  cursor: pointer;
}

.check-label {
  font-size: 12px;
  color: #666;
}

.full-table {
  :deep(.el-table) {
    .el-table__body-wrapper {
      max-height: calc(100vh - 400px);
      overflow-y: auto;
    }
  }
}

:deep(.el-pagination) {
  display: flex;
}
</style>