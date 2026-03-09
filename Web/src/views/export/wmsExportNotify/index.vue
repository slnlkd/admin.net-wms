<template>
  <div class="">
    <!--头部表格开始-->
    <el-row :gutter="5" style="width: 100%; height: 50%; flex: 1">
      <el-col :span="24" :xs="24" style="display: flex; height: 100%; flex: 1">
        <el-card class="full-table" shadow="hover" :body-style="{ height: 'calc(100% - 51px)' }">
          <template #header>
            <el-icon size="16"
              style="margin-right: 3px; display: inline; vertical-align: middle"><ele-Collection /></el-icon>总单
          </template>

          <el-form :inline="true" @submit.native.prevent class="handle-form">
            <el-form-item label="出库单号：">
              <el-input v-model="useTableExportNotifyStore.exportBillCodeParam" placeholder="出库单号"></el-input>
            </el-form-item>
            <el-form-item label="执行状态：">
              <el-select v-model="useTableExportNotifyStore.exportExecuteFlagParam" placeholder="执行状态">
                <el-option v-for="i in exportExecuteFlagList" :value="i.id" :label="i.name" :key="i.id">{{ i.name
                }}</el-option>
              </el-select>
            </el-form-item>
            <el-form-item label="单据类型：">
              <el-select v-model="useTableExportNotifyStore.exportBillTypeParam" placeholder="单据类型">
                <el-option v-for="list in useTableExportNotifyStore.exportBillTypeList" :label="list.billTypeName"
                  :key="list.id" :value="list.id">{{ list.billTypeName }}</el-option>
              </el-select>
            </el-form-item>
            <el-form-item label="所属仓库：">
              <el-select v-model="useTableExportNotifyStore.wareHouseIdParam" placeholder="所属仓库">
                <el-option v-for="list in useTableExportNotifyStore.wareHouseList" :label="list.warehouseName"
                  :key="list.id" :value="list.id">{{ list.warehouseName }}</el-option>
              </el-select>
            </el-form-item>
            <el-form-item label="开始时间：">
              <el-date-picker v-model="useTableExportNotifyStore.startTimeParam" type="datetime" placeholder="开始时间"
                value-format="YYYY-MM-DD HH:mm:ss" />
            </el-form-item>
            <el-form-item label="结束时间：">
              <el-date-picker v-model="useTableExportNotifyStore.endTimeParam" type="datetime" placeholder="结束时间"
                value-format="YYYY-MM-DD HH:mm:ss" />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" icon="ele-Search" @click="useTableExportNotifyStore.showNotifyTableFun()"
                v-auth="'sysDifflog:page'"> 查询
              </el-button>
            </el-form-item>
            <el-form-item>
              <el-button type="primary" icon="ele-Plus" @click="addNotifyFun" v-auth="'sysRole:add'"> 新增 </el-button>
            </el-form-item>
          </el-form>

          <el-table style="width: 100%" @row-click="useTableExportNotifyStore.ShowNotifyAndDetiailByNotifyIdFun"
            highlight-current-row @sort-change="" border :data="useTableExportNotifyStore.displayNotifyListComputed">
            <el-table-column type="index" label="序号" width="100" align="center" header-align="center" sortable='custom'
              fixed="left" />

            <el-table-column prop="id" label="出库单号表id" min-width="300" align="center" header-align="center"
              v-if="false" />

            <el-table-column prop="exportBillCode" label="出库单号" min-width="300" align="center" header-align="center" />
            <el-table-column prop="exportExecuteFlagStr" label="执行状态" min-width="150" align="center"
              header-align="center">
              <template #default="scope">
                <span :class="executeFlagColor(scope.row.exportExecuteFlagStr)">{{ scope.row.exportExecuteFlagStr
                }}</span>
              </template>
            </el-table-column>
            <el-table-column prop="warehouseStr" label="所属仓库" min-width="200" align="center" header-align="center" />
            <el-table-column prop="exportBillTypeStr" label="单据类型" min-width="150" align="center"
              header-align="center" />
            <el-table-column prop="documentSubtypeStr" label="出库子类型" min-width="150" align="center"
              header-align="center" />
            <el-table-column prop="source" label="来源" min-width="120" align="center" header-align="center" />
            <el-table-column prop="outerBillCode" label="外部单号" min-width="250" align="center" header-align="center" />
            <el-table-column prop="exportCustomerStr" label="客户名称" min-width="250" align="center"
              header-align="center" />
            <el-table-column prop="createUserName" label="创建人" min-width="250" align="center" header-align="center" />
            <el-table-column prop="createTime" label="创建时间" min-width="250" align="center" header-align="center" />
            <el-table-column prop="updateUserName" label="修改人" min-width="250" align="center" header-align="center" />
            <el-table-column prop="updateTime" label="修改时间" min-width="250" align="center" header-align="center" />
            <el-table-column prop="exportRemark" label="备注" min-width="250" align="center" header-align="center" />
            <el-table-column label="操作" width="300" fixed="right" align="center" header-align="center">
              <template #default="scope">
                <el-tooltip content="删除" v-if="scope.row.id != null && scope.row.id != -1">
                  <el-button icon="ele-Delete" size="small" text type="danger" v-auth="'sysDictType:delete'"
                    @click="useTableExportNotifyStore.deleteRow(scope.row)"> </el-button>
                </el-tooltip>
              </template>
            </el-table-column>
          </el-table>
          <el-pagination v-model:currentPage="useTableExportNotifyStore.page"
            v-model:page-size="useTableExportNotifyStore.pageSize" :total="useTableExportNotifyStore.total"
            :page-sizes="[10, 20, 50, 100]" size="small" background @size-change="handleDictTypeSizeChange"
            @current-change="handleDictTypeCurrentChange" layout="total, sizes, prev, pager, next, jumper" />
        </el-card>
      </el-col>
    </el-row>
    <!--头部表格结束-->
    <!--底部表格开始-->
    <el-row :gutter="5" style="width: 100%; height: 50%; flex: 1">
      <el-col :span="24" :xs="24" style="display: flex; height: 100%; flex: 1">
        <el-card class="full-table" shadow="hover" :body-style="{ height: 'calc(100% - 51px)' }">
          <template #header>
            <el-icon size="16"
              style="margin-right: 3px; display: inline; vertical-align: middle"><ele-Collection /></el-icon>明细
          </template>

          <el-table style="width: 100%" @row-click="" highlight-current-row @sort-change="" border
            :data="useTableExportNotifyStore.detailNotifyList">
            <el-table-column type="index" label="序号" width="100" align="center" header-align="center" sortable='custom'
              fixed="left" />

            <el-table-column prop="notifyDetailId" label="出库单据明细表id" min-width="300" align="center"
              header-align="center" v-if="false" />
            <el-table-column prop="notifyId" label="出库单据id" min-width="300" align="center" header-align="center"
              v-if="false" />
            <!-- <el-table-column prop="wmsStockId" label="库存表id" min-width="300" align="center" header-align="center" v-if="false" /> -->
            <el-table-column prop="materialId" label="物料表id" min-width="300" align="center" header-align="center"
              v-if="false" />
            <el-table-column prop="wmsBaseUnitId" label="物料单位表id" min-width="300" align="center" header-align="center"
              v-if="false" />


            <el-table-column prop="exportBillCode" label="出库单据" min-width="300" align="center" header-align="center" />
            <el-table-column prop="exportExecuteFlagStr" label="执行标志" min-width="300" align="center"
              header-align="center">
              <template #default="scope">
                <span :class="executeFlagColor(scope.row.exportExecuteFlagStr)">{{
                  scope.row.exportExecuteFlagStr}}</span>
              </template>
            </el-table-column>
            <el-table-column prop="materialCode" label="物料编码" min-width="300" align="center" header-align="center" />
            <el-table-column prop="materialName" label="物料名称" min-width="300" align="center" header-align="center" />
            <el-table-column prop="materialStandard" label="物料规格" min-width="300" align="center"
              header-align="center" />
            <el-table-column prop="lotNo" label="批次" min-width="300" align="center" header-align="center" />
            <!-- <el-table-column prop="inspectionStatusStr" label="质检状态" min-width="300" align="center" header-align="center" >
              <template #default="scope">
                <span :class="statusColor(scope.row.inspectionStatusStr)">{{ scope.row.inspectionStatusStr}}</span>
              </template>
            </el-table-column> -->
            <el-table-column prop="productionDate" label="生产日期" min-width="300" align="center" header-align="center" />
            <el-table-column prop="lostDate" label="失效日期" min-width="300" align="center" header-align="center" />
            <el-table-column prop="unitName" label="物料单位" min-width="300" align="center" header-align="center" />
            <el-table-column prop="exportQuantity" label="出库数量" min-width="300" align="center" header-align="center" />
            <el-table-column prop="allocateQuantity" label="分配数量" min-width="300" align="center"
              header-align="center" />
            <el-table-column prop="factQuantity" label="下发数量" min-width="300" align="center" header-align="center" />
            <el-table-column prop="completeQuantity" label="完成数量" min-width="300" align="center"
              header-align="center" />
            <el-table-column prop="" label="操作" min-width="100" align="center" header-align="center" fixed="right">
              <template #default="scope">
                <el-tooltip content="删除" v-if="scope.row.notifyId != null">
                  <el-button icon="ele-Delete" size="small" text type="danger" @click="useTableExportNotifyStore.deleteDetialRow(scope.row)"
                    v-auth="'sysDictType:delete'"> </el-button>
                </el-tooltip>
              </template>
            </el-table-column>
          </el-table>
          <el-pagination v-model:currentPage="useTableExportNotifyStore.detailPage"
            v-model:page-size="useTableExportNotifyStore.detailPageSize" :total="useTableExportNotifyStore.detailTotal"
            :page-sizes="[10, 20, 50, 100]" size="small" background @size-change="handleDetailDictTypeSizeChange"
            @current-change="handleDetailDictTypeCurrentChange" layout="total, sizes, prev, pager, next, jumper" />


        </el-card>
      </el-col>
    </el-row>
    <!--底部表格结束-->






    <!--幕布（所有的弹窗子页面或弹窗孙子页面，只要有一个打开，，幕布都开启；所有的弹窗子页面或弹窗孙子页面都关闭了，幕布就关闭）-->
    <div :class="{ 'overLay': useChildOpenAndCloseStore.overLay }"></div>




    <!--添加单据start-->
    <div class="ChildComponent" :style="{ 'z-index': useChildOpenAndCloseStore.wmsExportNotifyIndex_C.child_1.zIndex }"
      v-if="useChildOpenAndCloseStore.wmsExportNotifyIndex_C.child_1.isShow">
      <wmsExportNotifyAdd></wmsExportNotifyAdd>
    </div>
    <!--添加单据end-->
  </div>




</template>

<script lang="ts" setup name="sysDict">
import { onMounted, reactive, ref } from 'vue';
import { getAPI } from '/@/utils/axios-utils';
import { useUserInfo } from '/@/stores/userInfo';
import { ElMessageBox, ElMessage } from 'element-plus';
import { SysDictType, SysDictData, AccountTypeEnum } from '/@/api-services/models';
import { SysDictTypeApi, SysDictDataApi } from '/@/api-services/api';
import EditDictType from '/@/views/system/dict/component/editDictType.vue';
import EditDictData from '/@/views/system/dict/component/editDictData.vue';
import ModifyRecord from '/@/components/table/modifyRecord.vue';
import { auths } from "/@/utils/authFunction";
import { clearReactive } from '/@/utils/clearReactive';
import { useTableExportNotify } from '/@/stores/wmsExportNotify/Tables/useTableExportNotify';
import { useChildOpenAndClose } from '/@/stores/useChildOpenAndClose';
import wmsExportNotifyAdd from './component/wmsExportNotifyAdd.vue';


//引入pinia
const useTableExportNotifyStore = useTableExportNotify();
const useChildOpenAndCloseStore = useChildOpenAndClose();

//Index组件的注册的事件
let emit = defineEmits(['callSelectNotifyTable']);

//（0待执行、1正在分配、 2正在执行、3已完成、4作废 5已上传 ）
let exportExecuteFlagList = ref([
  { id: -1, name: "全选" },
  { id: 0, name: "待执行" },
  { id: 1, name: "正在分配" },
  { id: 2, name: "正在执行" },
  { id: 3, name: "已完成" },
  { id: 4, name: "作废" },
  { id: 5, name: "已上传" }
])

//加载组件
onMounted(async () => {

  try {
    //通过后端接口，获取单据类型数据
    //let billTypeResult = await axios.get(window.__env__.VITE_API_URL + "/api/wmsExportNotify/getWmsBaseBillTypeForSelect")
    await useTableExportNotifyStore.GetExportBillTypeFun();


    //通过后端接口，获取仓库表数据
    //let wareHouseResult = await axios.get(window.__env__.VITE_API_URL + "/api/wmsExportNotify/getWmsBaseWareHouseForSelect")
    await useTableExportNotifyStore.GetwareHouseFun();

    await useTableExportNotifyStore.showNotifyTableFun();
  }
  catch (error) {
    console.log("单据类型数据失败", error);
  }




})







//打开弹窗“添加出库单据页面”
function addNotifyFun() {
  useChildOpenAndCloseStore.open_wmsExportNotifyIndex_C_child_1();
}

// 改变页面容量
const handleDictTypeSizeChange = (val: number) => {
  useTableExportNotifyStore.pageSize = val;
  useTableExportNotifyStore.showNotifyTableFun();
};

// 改变页码序号
const handleDictTypeCurrentChange = (val: number) => {
  useTableExportNotifyStore.page = val;
  useTableExportNotifyStore.showNotifyTableFun();
};


// 改变页面容量 -- 明细
const handleDetailDictTypeSizeChange = (val: number) => {
  useTableExportNotifyStore.detailPageSize = val;
  useTableExportNotifyStore.ShowNotifyAndDetiailByNotifyIdFun(useTableExportNotifyStore.notifyTableRow);
};

// 改变页码序号 -- 明细
const handleDetailDictTypeCurrentChange = (val: number) => {
  useTableExportNotifyStore.detailPage = val;
  useTableExportNotifyStore.ShowNotifyAndDetiailByNotifyIdFun(useTableExportNotifyStore.notifyTableRow);
};


//表格--执行状态颜色转换
function executeFlagColor(value: string | null) {
  if (value == "待执行") {
    return 'yellowclass'
  }
  else if (value == "正在分配") {
    return 'blueclass'
  }
  else if (value == "正在执行") {
    return 'purpleclass'
  }
  else if (value == "已完成") {
    return 'orangeclass'
  }
  else if (value == "作废") {
    return 'greyclass'
  }
  else if (value == "已上传") {
    return 'greenclass'
  }
  else {
    return 'nullclass'
  }

}
//表格--质检状态颜色转换
function statusColor(value: string | null) {
  if (value == "待检验") {
    return 'yellowclass'
  }
  else if (value == "合格") {
    return 'greenclass'
  }
  else if (value == "不合格") {
    return 'redclass'
  }
  else {
    return 'nullclass'
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

:deep(.notice-bar) {
  position: absolute;
  display: inline-flex;
  height: 25px !important;
  width: 500px;
  overflow: hidden !important;

  div[data-slate-editor] {
    text-wrap-mode: nowrap;
  }
}

.handle-form .el-form-item {
  margin: 10px 10px !important;
}
</style>
