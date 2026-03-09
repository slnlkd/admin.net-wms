<template>
    <div class="wmsExportNotifyTop">
        <div class="input-div"><span>出库单号：</span><input type="text" v-model="useShowTableStore.exportBillCodeParam"
                placeholder="请输入出库单号">
        </div>
        <div class="input-div"><span>执行状态：</span>
            <select name="exportExecuteFlag" id="exportExecuteFlag" @change="ExportExecuteFlagTest">
                <option v-for="list in exportExecuteFlagList" :key="list.id" :value="list.id">{{ list.name }}</option>
            </select>
        </div>
        <div class="input-div"><span>单据类型：</span>
            <select name="exportBillType" id="exportBillType" @change="ExportBillTypeTest">
                <option v-for="list in useShowTableStore.exportBillTypeList" :key="list.id" :value="list.id">{{
                    list.billTypeName }}</option>
            </select>
        </div>
        <div class="input-div"><span>所属仓库：</span>
            <select name="wareHouse" id="wareHouse" @change="wareHouseTest">
                <option v-for="list in useShowTableStore.wareHouseList" :key="list.id" :value="list.id">{{
                    list.warehouseName }}</option>
            </select>
        </div>
    </div>
    <div class="wmsExportNotifyBottom">
        <div class="input-div"><span>开始时间：</span>
            <input type="date" v-model="useShowTableStore.startTimeParam">
        </div>
        <div class="input-div"><span>结束时间：</span>
            <input type="date" v-model="useShowTableStore.endTimeParam">
        </div>
        <button @click="showNotifyTable" class="btn"><img :src="galssimg">查询</button>
        <button @click="addNotifyFun" class="btn  addNotify"><img :src="addImgPath">添加</button>
    </div>

</template>














<script name="wmsExportNotifyTop" lang="ts" setup>
import axios from 'axios';
import { ref, reactive, onMounted, toRef, watch } from 'vue';
import galssimg from '/@/assets/010_magnifying_glass.png';
import addImg from '/@/assets/add.png';
import { clearReactive } from '/@/utils/clearReactive';
import { useTableExportNotify } from '/@/stores/wmsExportNotify/Tables/useTableExportNotify';
import { useChildOpenAndClose } from '/@/stores/useChildOpenAndClose';
import { storeToRefs } from 'pinia';
import { getDate } from '/@/utils/getDate';



//图片路径
let galssimgPath = ref(galssimg);
let addImgPath = ref(addImg);



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





//Index组件的注册的事件
let emit = defineEmits(['callSelectNotifyTable']);



//引入pinia
const useShowTableStore = useTableExportNotify();
const useChildOpenAndCloseStore = useChildOpenAndClose();

//如果不需要就可以注释掉
//let { exportBillCodeParam, exportExecuteFlagParam, exportBillTypeParam, wareHouseIdParam, startTimeParam, endTimeParam } = storeToRefs(useShowTableStore);




//加载组件
onMounted(async () => {

    try {
        //通过后端接口，获取单据类型数据
        //let billTypeResult = await axios.get(window.__env__.VITE_API_URL + "/api/wmsExportNotify/getWmsBaseBillTypeForSelect")
        await useShowTableStore.GetExportBillTypeFun();





        //通过后端接口，获取仓库表数据
        //let wareHouseResult = await axios.get(window.__env__.VITE_API_URL + "/api/wmsExportNotify/getWmsBaseWareHouseForSelect")
        let wareHouseResult = await useShowTableStore.GetwareHouseFun();




    }
    catch (error) {
        console.log("单据类型数据失败", error);
    }




})











//下拉框 -- 执行状态
function ExportExecuteFlagTest(event: any) {
    useShowTableStore.exportExecuteFlagParam = event.target.value;
}
//下拉框 -- 单据类型
function ExportBillTypeTest(event: any) {
    useShowTableStore.exportBillTypeParam = event.target.value;
}
//下拉框 -- 仓库表
function wareHouseTest(event: any) {
    useShowTableStore.wareHouseIdParam = event.target.value;
}


//查询出库单据
async function showNotifyTable() {
    //调用Index组件的注册的事件
    emit('callSelectNotifyTable');
}

//打开弹窗“添加出库单据页面”
function addNotifyFun() {
    useChildOpenAndCloseStore.open_wmsExportNotifyIndex_C_child_1();
}





/*
// let exportBillCode = ref("")//出库单号

// let exportExecuteFlag = ref(-1); //执行状态

// let exportBillType = ref(-1) //单据类型


// let exportBillTypeList = ref([
//     { id: -1, billType: -1, billTypeCode: "", billTypeName: "全选" }
// ]); //单据类型集合


// let wareHouseList = ref([
//     { id: -1, warehouseName: "", warehouseType: "", warehouseCode: "" }
// ])//所属仓库集合
// let wareHouseId = ref(-1); //所属仓库



// //开始时间
// let startTime = ref(DateNowLater3Day());
// //结束时间
// let endTime = ref(DateNow());












// //向父组件传递的数据
// let emitData = ref({
//     exportBillCode: exportBillCode.value,
//     exportExecuteFlag: exportExecuteFlag.value,
//     exportBillType: exportBillType.value,
//     wareHouseId: wareHouseId.value,
//     startTime: startTime.value,
//     endTime: endTime.value
// })













// const emit = defineEmits(['callChildB','callAddNotifyFun']);















*/
</script>





















<style scoped>
.wmsExportNotifyTop {
    display: block;
    width: 100%;
    height: 50%;
}

.wmsExportNotifyTop .input-div {
    display: inline-block;
    padding: 10px 30px;
    padding-left: 65px;
}

.wmsExportNotifyTop .input-div span {
    color: #000;
    font-size: 15px;
}

.wmsExportNotifyTop .input-div input {
    height: 40px;
    display: inline-block;
    border: 1.5px solid #ccc;
    padding: 10px;
    border-radius: 3px;
    width: 200px;
}

.wmsExportNotifyTop .input-div select {
    height: 40px;
    display: inline-block;
    border: 1.5px solid #ccc;
    padding: 10px;
    border-radius: 3px;
    width: 200px;
}


.wmsExportNotifyBottom {
    display: block;
    width: 100%;
    height: 50%;
}

.wmsExportNotifyBottom button {
    display: inline-block;
    background: #86c586;
    border: none;
    color: #fff;
    font-size: 15px;
    padding: 4px 20px;
    border-radius: 5px;
}

.wmsExportNotifyBottom .addNotify {
    position: relative;
    left: 500px;
}

.wmsExportNotifyBottom .btn:hover {

    background: #63da63;
}

.wmsExportNotifyBottom .btn:active {

    background: #1d1d1d;
}

.wmsExportNotifyBottom button img {
    width: 15px;
    height: 15px;
    margin-right: 5px;
    position: relative;
    top: 2px;
}

.wmsExportNotifyBottom .input-div {
    display: inline-block;
    padding: 10px 30px;
    padding-left: 65px;
}

.wmsExportNotifyBottom .input-div span {
    color: #000;
    font-size: 15px;
}

.wmsExportNotifyBottom .input-div input {
    height: 40px;
    display: inline-block;
    border: 1.5px solid #ccc;
    padding: 10px;
    border-radius: 3px;
    width: 200px;
}
</style>