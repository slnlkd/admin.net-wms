<script lang="ts" setup name="wmsStockDecide">
import { ref, reactive, onMounted } from 'vue';
import { ElMessageBox, ElMessage } from 'element-plus';
import { sm2 } from 'sm-crypto-v2';
import { feature, getAPI } from '/@/utils/axios-utils';
import { SysAuthApi } from '/@/api-services/api';
import { useUserInfo } from '/@/stores/userInfo';
import { formatDate } from '/@/utils/formatTime';
import { useWmsStockDecideApi } from '/@/api/StockManage/StockDecide/wmsStockDecide';

const wmsStockDecideApi = useWmsStockDecideApi();
const userInfoStore = useUserInfo();
const { userInfos } = userInfoStore;

const decideDialogVisible = ref(false);
const currentRow = ref<any>(null);

const state = reactive({
  tableLoading: false,
  tableQueryParams: {} as any,
  tableParams: {
    page: 1,
    pageSize: 20,
    total: 0,
    field: 'materialCode',
    order: 'ascending',
    descStr: 'descending',
  },
  tableData: [] as any[],
  decideForm: {
    inspectionStatus: undefined as number | undefined,
    retestDate: '' as string | null,
  },
});

const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsStockDecideApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
  state.tableParams.total = result?.total;
  state.tableData = result?.items ?? [];
  state.tableLoading = false;
};

const sortChange = async (column: any) => {
  state.tableParams.field = column.prop;
  state.tableParams.order = column.order;
  await handleQuery();
};

const formatDateCell = (value: any) => {
  if (!value) return '';
  return formatDate(new Date(value), 'YYYY-mm-dd HH:MM');
};

const getDefaultRetestDate = () => {
  const now = new Date();
  const nextYear = new Date(now.getTime() + 365 * 24 * 60 * 60 * 1000);
  return formatDate(nextYear, 'YYYY-mm-dd');
};

const verifyPassword = async () => {
  const { value } = await ElMessageBox.prompt('请输入密码', '密码确认', {
    confirmButtonText: '确认',
    cancelButtonText: '取消',
    inputType: 'password',
    inputValidator: (val) => (!val ? '密码不能为空' : true),
  });
  const publicKey = window.__env__.VITE_SM_PUBLIC_KEY;
  const encrypted = sm2.doEncrypt(value, publicKey, 1);
  const [err, res] = await feature(getAPI(SysAuthApi).apiSysAuthUnLockScreenPost(encrypted));
  if (err) throw err;
  if (!res.data.result) throw new Error('密码验证失败');
};

const openDecideDialog = async (row: any) => {
  await wmsStockDecideApi.check({ materialId: row.materialId, lotNo: row.lotNo });
  await verifyPassword();

  currentRow.value = row;
  state.decideForm.inspectionStatus = row.inspectionStatus;

  const detail = await wmsStockDecideApi.detailList({
    materialId: row.materialId,
    warehouseId: row.warehouseId,
    lotNo: row.lotNo,
    inspectionStatus: row.inspectionStatus,
  }).then(res => res.data.result);

  const retestDate = detail?.[0]?.retestDate;
  if (row.materialType === 45) {
    state.decideForm.retestDate = null;
  } else if (retestDate) {
    state.decideForm.retestDate = formatDate(new Date(retestDate), 'YYYY-mm-dd');
  } else {
    state.decideForm.retestDate = getDefaultRetestDate();
  }

  decideDialogVisible.value = true;
};

const submitDecide = async () => {
  if (!currentRow.value) return;
  await wmsStockDecideApi.decide({
    inspectionStatusUpt: currentRow.value.inspectionStatus,
    inspectionStatus: state.decideForm.inspectionStatus,
    retestDate: state.decideForm.retestDate,
    lotNo: currentRow.value.lotNo,
    warehouseId: currentRow.value.warehouseId,
    materialId: currentRow.value.materialId,
    stockQuantity: currentRow.value.stockQuantity,
    releaseStatus: currentRow.value.releaseStatus ?? 0,
    inspectionUser: userInfos?.id?.toString(),
  });
  ElMessage.success('改判成功');
  decideDialogVisible.value = false;
  await handleQuery();
};

const toggleRelease = async (row: any) => {
  if (!row.revisionDate) {
    ElMessage.warning('改判日期为空，无法放行');
    return;
  }
  const nextStatus = row.releaseStatus === 1 ? 0 : 1;
  const text = nextStatus === 1 ? '确定放行吗？' : '确定取消放行吗？';
  await ElMessageBox.confirm(text, '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning',
  });
  await wmsStockDecideApi.release({
    inspectionStatus: row.inspectionStatus,
    lotNo: row.lotNo,
    warehouseId: row.warehouseId,
    materialId: row.materialId,
    revisionDate: row.revisionDate,
    releaseStatus: nextStatus,
  });
  ElMessage.success('操作成功');
  await handleQuery();
};

onMounted(() => {
  handleQuery();
});
</script>

<template>
  <div class="wmsStockDecide-container">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
      <el-form :model="state.tableQueryParams" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="物料编码">
              <el-input v-model="state.tableQueryParams.materialCode" clearable placeholder="请输入物料编码" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="物料名称">
              <el-input v-model="state.tableQueryParams.materialName" clearable placeholder="请输入物料名称" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="批次">
              <el-input v-model="state.tableQueryParams.lotNo" clearable placeholder="请输入批次" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="质检状态">
              <g-sys-dict v-model="state.tableQueryParams.inspectionStatus" code="QualityInspectionStatus" render-as="select" placeholder="请选择质检状态" clearable filterable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item>
              <el-button-group>
                <el-button type="primary" icon="ele-Search" @click="handleQuery">查询</el-button>
                <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}">重置</el-button>
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>

    <el-card class="full-table" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" @sort-change="sortChange" border>
        <el-table-column type="index" label="序号" width="55" align="center" />
        <el-table-column prop="materialCode" label="物料编码" show-overflow-tooltip />
        <el-table-column prop="materialName" label="物料名称" show-overflow-tooltip />
        <el-table-column prop="materialStandard" label="物料规格" show-overflow-tooltip />
        <el-table-column prop="warehouseName" label="仓库" show-overflow-tooltip />
        <el-table-column prop="lotNo" label="批次" show-overflow-tooltip />
        <el-table-column prop="inspectionStatus" label="质检状态" show-overflow-tooltip>
          <template #default="scope">
            <g-sys-dict v-model="scope.row.inspectionStatus" code="QualityInspectionStatus" />
          </template>
        </el-table-column>
        <el-table-column prop="revisionDate" label="改判日期" show-overflow-tooltip>
          <template #default="scope">{{ formatDateCell(scope.row.revisionDate) }}</template>
        </el-table-column>
        <el-table-column prop="retestDate" label="复验日期" show-overflow-tooltip>
          <template #default="scope">{{ formatDateCell(scope.row.retestDate) }}</template>
        </el-table-column>
        <el-table-column prop="stockQuantity" label="库存数量" show-overflow-tooltip />
        <el-table-column prop="releaseStatus" label="放行状态" show-overflow-tooltip>
          <template #default="scope">
            <el-tag :type="scope.row.releaseStatus === 1 ? 'success' : 'info'">
              {{ scope.row.releaseStatus === 1 ? '放行' : '未放行' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" align="center" fixed="right">
          <template #default="scope">
            <el-button size="small" text type="primary" @click="openDecideDialog(scope.row)">改判</el-button>
            <el-button size="small" text type="primary" @click="toggleRelease(scope.row)">
              {{ scope.row.releaseStatus === 1 ? '取消放行' : '放行' }}
            </el-button>
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
        background
      />
    </el-card>

    <el-dialog v-model="decideDialogVisible" title="质检改判" width="520px">
      <el-form labelWidth="90">
        <el-form-item label="质检状态">
          <g-sys-dict v-model="state.decideForm.inspectionStatus" code="QualityInspectionStatus" render-as="select" placeholder="请选择质检状态" clearable filterable />
        </el-form-item>
        <el-form-item label="复验日期" v-if="currentRow?.materialType !== 45">
          <el-date-picker v-model="state.decideForm.retestDate" type="date" value-format="YYYY-MM-DD" placeholder="选择复验日期" style="width: 100%" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="decideDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="submitDecide">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
:deep(.el-input),
:deep(.el-select),
:deep(.el-input-number) {
  width: 100%;
}
</style>
