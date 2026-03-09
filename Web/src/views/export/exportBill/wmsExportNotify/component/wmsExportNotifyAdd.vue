<template>
    <div class="total">
        <span><img :src="imgBianJiImgPath" />添加出库单</span>
        <button @click="useChildOpenAndCloseStore.close_wmsExportNotifyIndex_C_child_1()" class="close">X</button>
    </div>

    <div class="">
        <el-row :gutter="5" style="width: 100%; height: 50%; flex: 1">
            <el-col :span="24" :xs="24" style="display: flex; height: 100%; flex: 1">
                <el-card class="full-table" shadow="hover" :body-style="{ height: 'calc(100% - 51px)' }">
                    <el-form :inline="true" @submit.native.prevent class="handle-form">
                        <el-form-item label="出库仓库：">
                            <el-select v-model="useTableExportNotifyAddStore.wareHouseListId" placeholder="出库仓库"
                                @change="changeWareHouseList">
                                <el-option v-for="list in useTableExportNotifyAddStore.wareHouseList" :value="list.id"
                                    :label="list.warehouseName" :key="list.id"></el-option>
                            </el-select>
                        </el-form-item>
                        <el-form-item label="单据类型：">
                            <el-select v-model="useTableExportNotifyAddStore.exportBillTypeListId" placeholder="单据类型"
                                @change="changeExportBillTypeList">
                                <el-option v-for="list in useTableExportNotifyAddStore.exportBillTypeList"
                                    :value="list.id" :label="list.billTypeName" :key="list.id"></el-option>
                            </el-select>
                        </el-form-item>
                        <el-form-item label="出库子类型：">
                            <el-select v-model="useTableExportNotifyAddStore.exportChildBillTypeListId"
                                placeholder="出库子类型">
                                <el-option v-for="list in useTableExportNotifyAddStore.exportChildBillTypeList"
                                    :label="list.billTypeName" :key="list.id" :value="list.id"></el-option>
                            </el-select>
                        </el-form-item>
                        <el-form-item label="&nbsp;&nbsp;&nbsp;&nbsp;出库口：">
                            <el-select v-model="useTableExportNotifyAddStore.wayOutListCode" placeholder="出库口">
                                <el-option v-for="i in useTableExportNotifyAddStore.wayOutList" :label="i.wayOutName"
                                    :key="i.wayOutCode" :value="i.wayOutCode"></el-option>
                            </el-select>
                        </el-form-item>

                        <el-form-item label="&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;部门：">
                            <el-select v-model="useTableExportNotifyAddStore.baseDepartmentId" placeholder="部门">
                                <el-option v-for="i in useTableExportNotifyAddStore.baseDepartmentList"
                                    :label="i.departmentName" :key="i.id" :value="i.id"></el-option>
                            </el-select>
                        </el-form-item>

                        <el-form-item label="&nbsp;&nbsp;&nbsp;&nbsp;供应商：">
                            <el-select v-model="useTableExportNotifyAddStore.baseSupplierId" placeholder="供应商">
                                <el-option v-for="i in useTableExportNotifyAddStore.baseSupplierList"
                                    :label="i.customerName" :key="i.id" :value="i.id"></el-option>
                            </el-select>
                        </el-form-item>
                        <el-form-item label="&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;拣货区：">
                            <el-select v-model="useTableExportNotifyAddStore.baseAreaId" placeholder="拣货区">
                                <el-option v-for="i in useTableExportNotifyAddStore.baseAreaList" :label="i.areaName"
                                    :key="i.id" :value="i.id"></el-option>
                            </el-select>
                        </el-form-item>
                        <el-form-item label="&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;拼箱：">
                            <el-select v-model="useTableExportNotifyAddStore.pXStatusId" placeholder="拼箱">
                                <el-option v-for="i in useTableExportNotifyAddStore.pXStatusList" :label="i.statusName"
                                    :key="i.id" :value="i.id"></el-option>
                            </el-select>
                        </el-form-item>


                        <hr :style="{ color: '#fff', background: '#fff', border: '1px solid #fff' }">


                        <el-form-item label="领用单位：">
                            <el-input v-model="customerName" placeholder="领用单位" readonly="true"></el-input>
                            <span v-show="false">id:{{ customerId }}--{{ customerName }}--{{ customerCode }}</span>
                        </el-form-item>
                        <el-form-item>
                            <el-button type="primary" icon="ele-Search"
                                @click="useChildOpenAndCloseStore.open_wmsExportNotifyAdd_C_child_1()"
                                v-auth="'sysDifflog:page'"> 选择领用单位
                            </el-button>
                        </el-form-item>
                        <el-form-item>
                            <el-button type="primary" icon="ele-Plus" @click="addNotifyDetailFun"
                                v-auth="'sysRole:add'"> 添加明细
                            </el-button>
                        </el-form-item>
                        <el-form-item>
                            <el-button type="primary" icon="ele-Plus" @click="clickOk" v-auth="'sysRole:add'"> 确定
                            </el-button>
                        </el-form-item>
                    </el-form>

                    <el-table style="width: 100%" highlight-current-row @sort-change="" border
                        :data="useTableExportNotifyAddStore.displayNotifyList" @row-click="" @selection-change="">
                        <el-table-column type="index" label="序号" width="100" align="center" header-align="center"
                            sortable='custom' fixed="left" />

                        <el-table-column prop="wmsBaseMaterialId" label="物料id" min-width="150" header-align="center"
                            align="center" v-if="false" />
                        <el-table-column prop="wareHouseId" label="仓库id" min-width="150" header-align="center"
                            align="center" v-if="false" />

                        <el-table-column prop="materialCode" label="物料编码" min-width="150" header-align="center"
                            align="center" />
                        <el-table-column prop="materialName" label="物料名称" min-width="150" header-align="center"
                            align="center" />
                        <el-table-column prop="materialStandard" label="物料规格" min-width="150" header-align="center"
                            align="center" />
                        <el-table-column prop="materialUnitStr" label="计量单位" min-width="150" header-align="center"
                            align="center" />
                        <el-table-column prop="ExportQuantity" label="计划出库数量" min-width="150" header-align="center"
                            align="center">
                            <template #default="scope">
                                <el-input placeholder="出库数量" v-model.number="scope.row.exportQuantity"
                                    v-if="scope.row.wmsBaseMaterialId != null"
                                    @input="isNumberFunExportQuantity(scope.row, $event)"></el-input>
                            </template>
                        </el-table-column>
                        <!-- <el-table-column prop="allocateQuantity" label="分配数量" min-width="150" header-align="center"
                            align="center">
                            <template #default="scope">
                                <el-input placeholder="分配数量" v-model.number="scope.row.allocateQuantity"
                                    v-if="scope.row.wmsBaseMaterialId != null"
                                    @input="isNumberFunAllocateQuantity(scope.row, $event)"></el-input>
                            </template>
                        </el-table-column>
                        <el-table-column prop="factQuantity" label="下发数量" min-width="150" header-align="center"
                            align="center">
                            <template #default="scope">
                                <el-input placeholder="下发数量" v-model.number="scope.row.factQuantity"
                                    v-if="scope.row.wmsBaseMaterialId != null"
                                    @input="isNumberFunFactQuantity(scope.row, $event)"></el-input>
                            </template>
                        </el-table-column> -->
                        <el-table-column prop="materialModel" label="物料型号" min-width="150" header-align="center"
                            align="center" />
                        <el-table-column prop="materialTypeStr" label="物料类型" min-width="150" header-align="center"
                            align="center" />
                        <el-table-column prop="lotNo" label="批次" min-width="150" header-align="center" align="center" />
                        <el-table-column prop="boxQuantity" label="整箱数量" min-width="150" header-align="center"
                            align="center" />
                        <el-table-column prop="stockQuantity" label="库存数量" min-width="150" header-align="center"
                            align="center" />
                        <el-table-column prop="importProductionDate" label="生产日期" min-width="150" header-align="center"
                            align="center" />
                        <el-table-column prop="importLostDate" label="失效日期" min-width="150" header-align="center"
                            align="center" />
                        <el-table-column prop="materialValidityDayStr" label="有效期" min-width="250" header-align="center"
                            align="center" />
                        <el-table-column prop="inspectionStatusStr" label="质检状态" min-width="150" header-align="center"
                            align="center">
                            <template #default="scope">
                                <span :class="executeFlagColor(scope.row.inspectionStatusStr)">{{
                                    scope.row.inspectionStatusStr }}</span>
                            </template>
                        </el-table-column>
                        <el-table-column prop="" label="操作" min-width="150" header-align="center" align="center"
                            fixed="right">
                            <template #default="scope">
                                <el-tooltip content="删除" v-if="scope.row.wmsBaseMaterialId != null">
                                    <el-button icon="ele-Delete" size="small" text type="danger" @click="deleteRow(scope.$index)" v-auth="'sysDictType:delete'"> </el-button>
                                </el-tooltip>
                            </template>
                        </el-table-column>
                    </el-table>
                </el-card>
            </el-col>
        </el-row>



        <!--选择领用单位start-->
        <div class="ChildComponent"
            :style="{ 'z-index': useChildOpenAndCloseStore.wmsExportNotifyAdd_C.child_1.zIndex }"
            v-if="useChildOpenAndCloseStore.wmsExportNotifyAdd_C.child_1.isShow">
            <SelectCompany ref="SelectCompanyComponent" @callReturnCustomer="handReturnCustomer"></SelectCompany>
        </div>
        <!--选择领用单位end-->

        <!--添加明细start-->
        <div class="ChildComponent"
            :style="{ 'z-index': useChildOpenAndCloseStore.wmsExportNotifyAdd_C.child_2.zIndex }"
            v-if="useChildOpenAndCloseStore.wmsExportNotifyAdd_C.child_2.isShow">
            <wmsExportNotifyAddDetail @callSelectedMaterial="handSelectedMaterial"></wmsExportNotifyAddDetail>
        </div>
        <!--添加明细end-->




    </div>







</template>

<script lang="ts" name="wmsExportNotifyAdd" setup>
import axios from 'axios';
import { ref, reactive, onMounted, onUnmounted, toRef, nextTick } from 'vue';
import { clearReactive } from '/@/utils/clearReactive';
import SelectCompany from './selectCompany.vue';
import wmsExportNotifyAddDetail from './wmsExportNotifyAddDetail.vue';
import BianJiImg from '/@/assets/BianJi.png';
import { emit } from 'process';
import { useChildOpenAndClose } from '/@/stores/useChildOpenAndClose';
import { useTableExportNotifyAdd } from '/@/stores/wmsExportNotify/Tables/useTableExportNotifyAdd';
import { useTableExportNotifyAddDetail } from '/@/stores/wmsExportNotify/Tables/useTableExportNotifyAddDetail';
import { convertToObject } from 'typescript';

//图片路径
let imgBianJiImgPath = ref(BianJiImg);



//调用pinia
const useChildOpenAndCloseStore = useChildOpenAndClose();
const useTableExportNotifyAddStore = useTableExportNotifyAdd();
const useTableExportNotifyAddDetailStore = useTableExportNotifyAddDetail();


//选择领用单位组件
let SelectCompanyComponent = ref();
let customerName = ref(); //客户名称
let customerId = ref();   //客户id
let customerCode = ref(); //客户code











//页面加载完成
onMounted(async () => {
    try {
        await useTableExportNotifyAddStore.GetWmsBaseWareHouseForSelectFun();
        await useTableExportNotifyAddStore.GetWmsBaseBillTypeForSelectFun();
        await useTableExportNotifyAddStore.GetWmsBaseChildBillTypeForSelectFun();
        await useTableExportNotifyAddStore.GetWmsExportWayOutByWareHouseIdFun();
        await useTableExportNotifyAddStore.GetWmsBaseDepartmentForSelectFun();
        await useTableExportNotifyAddStore.GetWmsBaseSupplierForSelectFun();
        await useTableExportNotifyAddStore.GetWmsBaseAreaForSelectFun();

        useTableExportNotifyAddStore.MakeExportNotifyCode();
        useTableExportNotifyAddStore.MakeExportNotifyLotNo();
        useTableExportNotifyAddStore.showNotifyAddTableFun(null);
    }
    catch (error) {
        console.log("错误")
    }
})

//当页面卸载的时候
onUnmounted(() => {
    useTableExportNotifyAddStore.wareHouseListId = null; // 出库仓库id
    useTableExportNotifyAddStore.exportBillTypeListId = null; // 单据类型id
    useTableExportNotifyAddStore.exportChildBillTypeListId = null; // 出库子类型（单据子类型）id
    useTableExportNotifyAddStore.wayOutListCode = null; // 出库口Code
    useTableExportNotifyAddStore.customerId = null; // 领用单位id
    useTableExportNotifyAddStore.baseDepartmentId = null; //部门id
    useTableExportNotifyAddStore.baseSupplierId = null; //供应商id
    useTableExportNotifyAddStore.baseAreaId = null; //区域id
    useTableExportNotifyAddStore.pXStatusId = null; //拼箱id

    //表格第一行的标题
    useTableExportNotifyAddStore.tableTop = [];

    //出库单据表格的数据
    useTableExportNotifyAddStore.notifyList = [{}];

    //虚拟表格数据（当计算属性不用的时候，可以用它）
    useTableExportNotifyAddStore.displayNotifyList = [{}];
    //=====================================
    useTableExportNotifyAddStore.exportNotifyCode = ''; //出库单据code
    useTableExportNotifyAddStore.lotNo = ''; //批次
})







//选择领用单位的子组件触发的函数
function handReturnCustomer() {
    customerName.value = SelectCompanyComponent.value.exportCustomerName;
    customerId.value = SelectCompanyComponent.value.exportId;
    customerCode.value = SelectCompanyComponent.value.exportCustomerCode;
    useChildOpenAndCloseStore.close_wmsExportNotifyAdd_C_child_1();
}



//仓库下拉框
async function changeWareHouseList(event: any) {

    await useTableExportNotifyAddStore.GetWmsExportWayOutByWareHouseIdFun();


}

//单据类型下拉框 选择值被修改
async function changeExportBillTypeList(event: any) {

    await useTableExportNotifyAddStore.GetWmsBaseChildBillTypeForSelectFun();
}
//单据子类型下拉框 选择值被修改
function changeExportChildBillTypeList(event: any) {
    useTableExportNotifyAddStore.exportChildBillTypeListId = event.target.value;
}
//出库口下拉框
function changeWayOutListFun(event: any) {
    useTableExportNotifyAddStore.wayOutListCode = event?.target.value;
}


//点击 添加明细
function addNotifyDetailFun() {
    if (useTableExportNotifyAddStore.exportBillTypeListId == null || useTableExportNotifyAddStore.exportBillTypeListId == -1) {
        alert("请选择单据类型后再添加明细")
    }
    else {

        useTableExportNotifyAddDetailStore.parentProps.customerId = customerId.value;
        useTableExportNotifyAddDetailStore.parentProps.exportBillTypeId = useTableExportNotifyAddStore.exportBillTypeListId;
        useTableExportNotifyAddDetailStore.parentProps.exportChildBillTypeId = useTableExportNotifyAddStore.exportChildBillTypeListId;
        useTableExportNotifyAddDetailStore.parentProps.wareHouseId = useTableExportNotifyAddStore.wareHouseListId;
        useTableExportNotifyAddDetailStore.parentProps.wayOutCode = useTableExportNotifyAddStore.wayOutListCode;

        useChildOpenAndCloseStore.open_wmsExportNotifyAdd_C_child_2()
    }
}


//AddDetail组件传过来的数据
function handSelectedMaterial() {
    //AddDetail组件中，表格选择的数据

    useTableExportNotifyAddStore.showNotifyAddTableFun(useTableExportNotifyAddDetailStore.selectedData);
    useChildOpenAndCloseStore.close_wmsExportNotifyAdd_C_child_2();


}

//表格--执行状态颜色转换
function executeFlagColor(value: string | null) {

    if (value == "待检验") {
        return 'greyclass'
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
//点击确定按钮
function clickOk() {



    useTableExportNotifyAddStore.AddExportNotifyAndDetiailFUN();


    useChildOpenAndCloseStore.close_wmsExportNotifyIndex_C_child_1();
}

//判断输入的内容是否为数字 -- ExportQuantity
function isNumberFunExportQuantity(row: any, value: any) {
    // 过滤非数字和小数点
    const filtered = value.replace(/[^\d.]/g, '');
    // 确保只有一个小数点
    const parts = filtered.split('.');
    if (parts.length > 2) {
        row.exportQuantity = parts[0] + '.' + parts.slice(1).join('');
    } else {
        row.exportQuantity = filtered;
    }
}
//判断输入的内容是否为数字 -- AllocateQuantity
function isNumberFunAllocateQuantity(row: any, value: any) {
    // 过滤非数字和小数点
    const filtered = value.replace(/[^\d.]/g, '');
    // 确保只有一个小数点
    const parts = filtered.split('.');
    if (parts.length > 2) {
        row.allocateQuantity = parts[0] + '.' + parts.slice(1).join('');
    } else {
        row.allocateQuantity = filtered;
    }
}
//判断输入的内容是否为数字 -- FactQuantity
function isNumberFunFactQuantity(row: any, value: any) {
    // 过滤非数字和小数点
    const filtered = value.replace(/[^\d.]/g, '');
    // 确保只有一个小数点
    const parts = filtered.split('.');
    if (parts.length > 2) {
        row.factQuantity = parts[0] + '.' + parts.slice(1).join('');
    } else {
        row.factQuantity = filtered;
    }
}


function deleteRow(row: any) {

    // console.log("xx",row);
    useTableExportNotifyAddStore.displayNotifyList.splice(row, 1);
}


/*
// let exportBillTypeList = ref([{
//     id: -1,
//     billTypeName: ""
// }])

// let exportChildBillTypeList = ref([{
//     id: -1,
//     billTypeName: ""
// }])

// let wareHouseList = ref([{
//     id: -1,
//     warehouseName: ""
// }])

// let wayOutList = ref([{
//     id: -1,
//     wayOutCode: "",
//     wayOutName: ""
// }])

// let exportBillType = ref(-1) //单据类型

// let wareHouseId = ref(-1); //所属仓库

// let exportChildBillType = ref(-1) //单据子类型
// let isShowChildBillType = ref(true)

// let wayOutCode = ref("");  //出库口编码
// let isShowWayOutCode = ref(true);

// let company = ref("") //领用单位



// const emitIndex = defineEmits(['callColse']);


// //加载组件
// onMounted(async () => {

//     try {
//         //通过后端接口，获取单据类型数据
//         let billTypeResult = await axios.get(window.__env__.VITE_API_URL + "/api/wmsExportNotify/getWmsBaseBillTypeForSelect")
//         exportBillTypeList.value = billTypeResult.data.result
//         exportBillTypeList.value.unshift({ id: -1, billTypeName: "全选" })



//         //通过后端接口，获取仓库表数据
//         let wareHouseResult = await axios.get(window.__env__.VITE_API_URL + "/api/wmsExportNotify/getWmsBaseWareHouseForSelect")
//         wareHouseList.value = wareHouseResult.data.result
//         wareHouseList.value.unshift({ id: -1, warehouseName: "全选" })


//         await getWayOutSelect({});
//         await exportChildBillTypeListFun({});


//     }
//     catch (error) {
//         console.log("单据类型数据失败", error);
//     }




// })





// //出库仓库的下拉框
// async function wareHouseListFun(event: any) {
//     wareHouseId.value = event.target.value;
//     if (wareHouseId.value != -1) {
//         await getWayOutSelect({
//             wareHouseId: wareHouseId.value
//         })
//     }
//     else {
//         await getWayOutSelect({})
//     }
//     isShowWayOutCode.value = false;
//     await nextTick();
//     isShowWayOutCode.value = true;
// }


// //单据类型下拉框
// async function exportBillTypeListFun(event: any) {
//     exportBillType.value = event.target.value;
//     if (exportBillType.value != -1) {
//         await exportChildBillTypeListFun({
//             id: exportBillType.value
//         })
//     }
//     else {
//         await exportChildBillTypeListFun({})
//     }
//     isShowChildBillType.value = false
//     await nextTick();
//     isShowChildBillType.value = true

// }

// //单据子类型下拉框
// async function exportChildBillTypeListFun(param: any) {
//     try {
//         //通过后端接口，获取出库口列表数据
//         let childBillTypeResult = await axios.post(window.__env__.VITE_API_URL + "/api/wmsExportNotify/getWmsBaseChildBillTypeForSelect", param)
//         exportChildBillTypeList.value = childBillTypeResult.data.result;
//         exportChildBillTypeList.value.unshift({ id: -1, billTypeName: "全选" })
//     }
//     catch (error) {
//         console.log("错误");
//     }

// }
// function childBillTypeListFun(event: any) {
//     exportChildBillType.value = event.target.value;
// }

// //出库口下拉框
// function notifyInfaceFun(event: any) {
//     wayOutCode.value = event.target.value;
// }
// //通过后端接口，获取出库口列表数据
// async function getWayOutSelect(param: any) {
//     try {
//         //通过后端接口，获取出库口列表数据
//         let wayOutResult = await axios.post(window.__env__.VITE_API_URL + "/api/wmsExportNotify/getWmsExportWayOutByWareHouseId", param)
//         wayOutList.value = wayOutResult.data.result;
//         wayOutList.value.unshift({ id: -1, wayOutCode: "", wayOutName: "全选" })



//     }
//     catch (error) {
//         console.log("错误");
//     }

// }

// //点击选择领用单位
// function selectCompany() {

// }


// //关闭方法
// function closeFun(){
//     emitIndex('callColse');
// }
*/

</script>
























<style scoped>
.delBtn:hover {
    cursor: pointer;
    background-color: #ff0000;
}


/* .wmsExportNotifyAdd {
    width: 100%;
    height: 95%;

}

.wmsExportNotifyAdd .top {
    width: 100%;
    height: 20%
}

.wmsExportNotifyAdd .top .lineOne {
    width: 100%;
    height: 40%
}

.wmsExportNotifyAdd .top .lineOne .input-div {
    display: inline-block;
    padding: 10px 30px;
    padding-left: 65px;
}

.wmsExportNotifyAdd .top .lineOne .input-div span {
    color: #000;
    font-size: 15px;
}

.wmsExportNotifyAdd .top .lineOne .input-div input {
    height: 40px;
    display: inline-block;
    border: 1.5px solid #ccc;
    padding: 10px;
    border-radius: 3px;
    width: 200px;
}

.wmsExportNotifyAdd .top .lineOne .input-div select {
    height: 40px;
    display: inline-block;
    border: 1.5px solid #ccc;
    padding: 10px;
    border-radius: 3px;
    width: 200px;
}


.wmsExportNotifyAdd .top .lineTwo {
    width: 100%;
    height: 40%
}

.wmsExportNotifyAdd .top .lineTwo .input-div {
    display: inline-block;
    padding: 10px 30px;
    padding-left: 65px;
}

.wmsExportNotifyAdd .top .lineTwo .input-div span {
    color: #000;
    font-size: 15px;
}

.wmsExportNotifyAdd .top .lineTwo .input-div input {
    height: 40px;
    display: inline-block;
    border: 1.5px solid #ccc;
    padding: 10px;
    border-radius: 3px;
    width: 200px;
}

.wmsExportNotifyAdd .top .lineTwo .input-div select {
    height: 40px;
    display: inline-block;
    border: 1.5px solid #ccc;
    padding: 10px;
    border-radius: 3px;
    width: 200px;
}

.wmsExportNotifyAdd .top .lineTwo .input-div .btn {
    border-radius: 5px;
    border: none;
    padding: 8px 22px;
    position: relative;
    top: 5px;
    margin-left: 15px;
    background: #86c586;
    line-height: 10px;
}

.wmsExportNotifyAdd .top .lineTwo .input-div .btn:hover {
    background: #63da63;
}

.wmsExportNotifyAdd .top .lineTwo .input-div .btn:active {
    background: #1d1d1d;
}

.wmsExportNotifyAdd .top .lineTwo .input-div .btn .img {
    width: 20px;
    margin: 0px 5px;
}

.wmsExportNotifyAdd .top .lineTwo .input-div .btn h5 {
    display: inline-block;
    color: #fff;
    position: relative;
    top: -6px;
}

.wmsExportNotifyAdd .top .lineThree {
    width: 100%;
    height: 40%
}

.wmsExportNotifyAdd .top .lineThree button {
    border-radius: 5px;
    border: none;
    padding: 8px 22px;
    position: relative;
    top: 5px;
    margin-left: 15px;
    background: #86c586;
    line-height: 10px;

}

.wmsExportNotifyAdd .top .lineThree button:hover {
    background: #63da63;
}

.wmsExportNotifyAdd .top .lineThree button:active {
    background: #1d1d1d;
}

.wmsExportNotifyAdd .top .lineThree button h5 {
    display: inline-block;
    color: #fff;
}

.wmsExportNotifyAdd .content {
    width: 100%;
    height: 80%
} */
</style>