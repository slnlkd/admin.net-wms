<script lang="ts" setup name="wmsBaseOperLog">
import { ref, reactive, onMounted } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { useWmsBaseOperLogApi } from '/@/api/system/log/bizlog/wmsBaseOperLog';

const wmsBaseOperLogApi = useWmsBaseOperLogApi();
// 详情对话框
const dialogVisible = ref(false);
// 加载loading
const loadingDetail = ref(false);
// 详情内容
const content = ref('');
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

// 查看详情
const viewDetail = async (row: any) => {
  dialogVisible.value = true;
  loadingDetail.value = true;
  var res = await wmsBaseOperLogApi.detail(row.id);
  row.message = res.data.result?.message ?? '';
  content.value = row.message;
  loadingDetail.value = false;
};

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await wmsBaseOperLogApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
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
handleQuery();
</script>
<template>
  <div class="wmsBaseOperLog-container" v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
      <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="state.tableQueryParams.keyword" clearable placeholder="请输入模糊查询关键字" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="日志追踪">
              <el-input v-model="state.tableQueryParams.traceId" clearable placeholder="请输入关联的技术日志TraceId，用于开发人员追踪" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="模块名称">
              <el-input v-model="state.tableQueryParams.module" clearable placeholder="请输入模块名称" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="操作类型">
              <el-input v-model="state.tableQueryParams.operationType" clearable placeholder="请输入操作类型：新增、修改、删除、审核等" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="操作人员">
              <el-input v-model="state.tableQueryParams.operator" clearable placeholder="请输入操作人员" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="操作时间">
              <el-date-picker type="daterange" v-model="state.tableQueryParams.operateTimeRange"
                value-format="YYYY-MM-DD HH:mm:ss" start-placeholder="开始日期" end-placeholder="结束日期"
                :default-time="[new Date('1 00:00:00'), new Date('1 23:59:59')]" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="操作IP地址">
              <el-input v-model="state.tableQueryParams.operateIp" clearable placeholder="请输入操作IP地址" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="业务单据号">
              <el-input v-model="state.tableQueryParams.businessNo" clearable placeholder="请输入业务单据号/ID" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="操作内容">
              <el-input v-model="state.tableQueryParams.operationContent" clearable placeholder="请输入操作内容" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="操作结果">
              <el-input v-model="state.tableQueryParams.result" clearable placeholder="请输入操作结果：成功/失败" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="额外信息">
              <el-input v-model="state.tableQueryParams.extraInfo" clearable placeholder="请输入额外信息" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <!-- <el-form-item label="隐藏的入参参数（JSON格式，开发人员使用，不对客户显示）">
              <el-input v-model="state.tableQueryParams.operParam" clearable placeholder="请输入隐藏的入参参数（JSON格式，开发人员使用，不对客户显示）"/>
            </el-form-item> -->
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item>
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary" icon="ele-Search" @click="handleQuery" v-auth="'wmsBaseOperLog:page'"
                  v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true"
                  v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                <el-button icon="ele-ZoomOut" @click="() => state.showAdvanceQueryUI = false"
                  v-if="state.showAdvanceQueryUI" style="margin-left:5px;"> 隐藏 </el-button>
                <!-- <el-button type="danger" style="margin-left:5px;" icon="ele-Delete" @click="batchDelWmsBaseOperLog" :disabled="state.selectData.length == 0" v-auth="'wmsBaseOperLog:batchDelete'"> 删除 </el-button> -->
                <!-- <el-button type="primary" style="margin-left:5px;" icon="ele-Plus" @click="editDialogRef.openDialog(null, '新增业务操作日志')" v-auth="'wmsBaseOperLog:add'"> 新增 </el-button> -->
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }"
        style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange"
        border>
        <el-table-column type="selection" width="40" align="center"
          v-if="auth('wmsBaseOperLog:batchDelete') || auth('wmsBaseOperLog:export')" />
        <el-table-column type="index" label="序号" width="55" align="center" />
        <!-- <el-table-column prop='traceId' label='日志追踪' width="430" show-overflow-tooltip /> -->
        <el-table-column prop='module' label='模块名称' width="100" show-overflow-tooltip />
        <el-table-column prop='operationType' label='操作类型' width="100" show-overflow-tooltip />
        <el-table-column prop='operator' label='操作人员' width="100" show-overflow-tooltip />
        <el-table-column prop='operateTime' label='操作时间' width="160" show-overflow-tooltip />
        <el-table-column prop='operateIp' label='IP地址' width="200" align="center" show-overflow-tooltip />
        <!-- <el-table-column prop='businessNo' label='业务ID' width="200" show-overflow-tooltip /> -->
        <el-table-column prop='operationContent' label='操作内容' width="500" show-overflow-tooltip>
          <template #default="scope">
            <div class="pre-line-content">{{ scope.row.operationContent }}</div>
          </template>
        </el-table-column>
        <!-- <el-table-column prop='beforeDataSummary' label='修改前数据' width="100" show-overflow-tooltip />
        <el-table-column prop='afterDataSummary' label='修改后数据' width="100" show-overflow-tooltip /> -->
        <el-table-column prop='result' label='操作结果' width="100" show-overflow-tooltip />
        <el-table-column prop='elapsedMs' label='操作耗时（毫秒）' width="140" show-overflow-tooltip />
        <el-table-column prop='extraInfo' label='额外信息' width="150" show-overflow-tooltip />
        <!-- <el-table-column prop='operParam' label='入参参数' show-overflow-tooltip /> -->
        <!-- <el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
          <template #default="scope">
            <ModifyRecord :data="scope.row" />
          </template>
</el-table-column> -->
        <!-- <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip
          v-if="auth('wmsBaseOperLog:update') || auth('wmsBaseOperLog:delete')">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text type="primary"
              @click="viewDetail(scope.row)" v-auth="'wmsBaseOperLog:update'"> 详情 </el-button>
            <el-button icon="ele-Delete" size="small" text type="primary" @click="delWmsBaseOperLog(scope.row)" v-auth="'wmsBaseOperLog:delete'"> 删除 </el-button>
          </template>
        </el-table-column> -->
      </el-table>
      <el-pagination v-model:currentPage="state.tableParams.page" v-model:page-size="state.tableParams.pageSize"
        @size-change="(val: any) => handleQuery({ pageSize: val })"
        @current-change="(val: any) => handleQuery({ page: val })" layout="total, sizes, prev, pager, next, jumper"
        :page-sizes="[10, 20, 50, 100, 200, 500]" :total="state.tableParams.total" size="small" background />
    </el-card>
      <el-dialog v-model="dialogVisible" draggable fullscreen>
    <template #header>
      <div style="color: #fff">
        <el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> 
        </el-icon>
        <span> 日志详情 </span>
      </div>
    </template>
    <pre v-loading="loadingDetail">{{ content }}</pre>
  </el-dialog>
  </div>
</template>
<style scoped>
:deep(.el-input),
:deep(.el-select),
:deep(.el-input-number) {
  width: 100%;
}
.pre-line-content {
  white-space: pre-line;
  word-break: break-all;
}
</style>