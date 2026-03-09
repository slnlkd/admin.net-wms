<template>
    <div class="wmsExportNotifyContent">
        <div class="top">
            <h3>总 单</h3>
        </div>
        <div class="content">
            <table>
                <thead>
                    <tr>
                        <th v-for="i in useShowTableStore.tableTop" :key="i.id">{{ i.name }}</th>
                    </tr>
                </thead>
                <tbody>

                    <tr v-for="list in useShowTableStore.displayNotifyListComputed">
                        <th>{{ list.index }}</th>
                        <th>{{ list.exportBillCode }}</th>
                        <th><span :class="executeFlagColor(list.exportExecuteFlagStr)">{{ list.exportExecuteFlagStr
                        }}</span></th>
                        <th>{{ list.warehouseStr }}</th>
                        <th>{{ list.exportBillTypeStr }}</th>
                        <th>{{ list.documentSubtypeStr }}</th>
                        <th>{{ list.source }}</th>
                        <th>{{ list.outerBillCode }}</th>
                        <th>{{ list.exportCustomerStr }}</th>
                        <th>{{ list.createUserName }}</th>
                        <th>{{ list.createTime }}</th>
                        <th>{{ list.updateUserName }}</th>
                        <th>{{ list.updateTime }}</th>
                        <th>{{ list.exportRemark }}</th>
                        <th></th>
                    </tr>
                </tbody>
            </table>
        </div>
        <div class="bottom">
            <button @click="notifyPrevPage" :disabled="!useShowTableStore.hasPrevPage">上一页</button>
            <select name="notifyPageSelect" v-model="useShowTableStore.page" @change="changeNotifyPageSelect">
                <option v-for="v in useShowTableStore.totalPages" :key="v" :value="v">{{ v }}</option>
            </select>
            <button @click="notifyNextPage" :disabled="!useShowTableStore.hasNextPage">下一页</button>
            <span>当前页数：{{ useShowTableStore.page }}</span>
            <span>总页数：{{ useShowTableStore.totalPages }}</span>
            <span>总条数：{{ useShowTableStore.total }}</span>
        </div>
    </div>
</template>
<script lang="ts" setup name="wmsExportNotifyContent">
import axios from 'axios';
import { ref, reactive, onMounted, computed } from 'vue';
import { useTableExportNotify } from '/@/stores/wmsExportNotify/Tables/useTableExportNotify';
import { storeToRefs } from 'pinia';



//调用pinia
const useShowTableStore = useTableExportNotify();
//从pinia中生成ref类型的变量
let { totalRows } = storeToRefs(useShowTableStore);




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
        return 'redclass'
    }

}

//单据上一页
async function notifyPrevPage() {
    useShowTableStore.page = useShowTableStore.page - 1;
    await useShowTableStore.showNotifyTableFun();

}

//单据下一页
async function notifyNextPage() {

    useShowTableStore.page = useShowTableStore.page + 1;
    await useShowTableStore.showNotifyTableFun();

}
//单据下拉框选页数
async function changeNotifyPageSelect(event: any) {

    useShowTableStore.page = event.target.value;
    await useShowTableStore.showNotifyTableFun();

}

//页面加载事件
onMounted(async () => {

    await useShowTableStore.showNotifyTableFun();


})
//查询出库单据
async function getNotifyTableFun() {
    await useShowTableStore.showNotifyTableFun();

}



// 暴露“加载出库单据”方法等其他数据
defineExpose({ getNotifyTableFun })


/*
// //开始时间
// let startTime = ref(DateNowLater3Day());
// //结束时间
// let endTime = ref(DateNow());

// let notifyParams = ref({ //查询参数
//     page: 1,//当前页数
//     pageSize: 2,//每一页多少数据
//     field: "createTime", //排序字段
//     order: "descending",//排序方法
//     descStr: "descending", //降序排序
//     startTime: startTime.value,
//     endTime: endTime.value
// })
// //出库单据表格的数据 和 页数页码等数据
// let notifyListAndPage = ref({
//     hasNextPage: true,   //是否有上一页
//     hasPrevPage: false,  //是否有下一页
//     page: 1,             //页码
//     pageSize: 2,         //页容量
//     total: 6,            //总条数
//     totalPages: 1        //总页数
// });

// //出库单据表格的数据
// let notifyList = ref([{
//     index: null, exportBillCode: null, exportExecuteFlagStr: null, warehouseStr: null,
//     exportBillTypeStr: null, documentSubtypeStr: null, source: null, outerBillCode: null,
//     exportCustomerStr: null, createUserName: null, createTime: null, updateUserName: null,
//     updateTime: null, exportRemark: null
// }])
// //出库单据表需要显示的行数
// const totalRows = ref(6)





// //加载出库单据
// async function showNotifyTable(param: any,notifyParams:any,notifyListAndPage:any) {
//     try {
//         let newParam = Object.assign({}, param, notifyParams.value,notifyListAndPage)
//         let notifyResult = await axios.post(window.__env__.VITE_API_URL + "/api/wmsExportNotify/showWmsExportNotifyOfPage", newParam)
//         notifyListAndPage.value = notifyResult.data.result;
        

//         notifyList.value = notifyResult.data.result.items
//     }
//     catch (error) {
//         console.log("出库单据查询失败");
//     }
// }

// //出库单据实际在页面显示的行数的真实数据
// let displayNotifyList = computed(() => {
//     while (notifyList.value.length < totalRows.value) {
//         notifyList.value.push({
//             index: null, exportBillCode: null, exportExecuteFlagStr: null, warehouseStr: null,
//             exportBillTypeStr: null, documentSubtypeStr: null, source: null, outerBillCode: null,
//             exportCustomerStr: null, createUserName: null, createTime: null, updateUserName: null,
//             updateTime: null, exportRemark: null
//         });
//     }
//     return notifyList.value;
// })








*/

</script>

















<style scoped>
.redclass {}

.yellowclass {
    background-color: #c5c513;
    color: #fff;
    padding: 5px 25px;
    display: inline-block;
    border-radius: 25px;
}

.blueclass {
    background-color: blue;
    color: #fff;
    padding: 5px 25px;
    display: inline-block;
    border-radius: 25px;
}

.greenclass {
    background-color: green;
    color: #fff;
    padding: 5px 25px;
    display: inline-block;
    border-radius: 25px;
}

.purpleclass {
    background-color: purple;
    color: #fff;
    padding: 5px 25px;
    display: inline-block;
    border-radius: 25px;
}

.orangeclass {
    background-color: orange;
    color: #fff;
    padding: 5px 25px;
    display: inline-block;
    border-radius: 25px;
}

.greyclass {
    background-color: grey;
    color: #fff;
    padding: 5px 25px;
    display: inline-block;
    border-radius: 25px;
}


.wmsExportNotifyContent {
    padding: 10px 40px;
    display: block;
    height: 100%;
    width: 100%;
}

.wmsExportNotifyContent .top {
    border-bottom: 1px solid #e5e3e3;
}

.wmsExportNotifyContent .top h3 {
    font-size: 15px;
    display: inline-block;
    border-bottom: 3px solid green;
}

.wmsExportNotifyContent .content {
    padding: 10px 0px;
    display: block;
    height: 80%;
    width: 100%;
}

.wmsExportNotifyContent .content table {
    border: 1px solid #ccc;
    border-top: 1px solid red;
    display: block;
    border-collapse: collapse;
    max-height: 100%;
    max-width: 100%;
    overflow: auto;
}

.wmsExportNotifyContent .content table thead {
    display: inline-block;
    background-color: #dddada;
    position: sticky;
    top: 0;
}




.wmsExportNotifyContent .content table thead tr {}

.wmsExportNotifyContent .content table thead tr th {
    border: 1px solid #ccc;
    padding: 5px 10px;
    text-align: center;
    font-size: 15px;
    height: 30px;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(1) {
    min-width: 100px;
    position: sticky;
    left: 0;
    top: 0;
    z-index: 20;
    background-color: #dddada;
    border: 1px solid #ccc !important;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(2) {
    min-width: 300px;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(3) {
    min-width: 150px;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(4) {
    min-width: 200px;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(5) {
    min-width: 150px;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(6) {
    min-width: 150px;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(7) {
    min-width: 120px;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(8) {
    min-width: 250px;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(9) {
    min-width: 250px;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(10) {
    min-width: 250px;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(11) {
    min-width: 250px;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(12) {
    min-width: 250px;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(13) {
    min-width: 250px;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(14) {
    min-width: 250px;
}

.wmsExportNotifyContent .content table thead tr th:nth-child(15) {
    min-width: 300px;
    position: sticky;
    right: 0;
    top: 0;
    z-index: 20;
    background-color: #dddada;
    border: 1px solid #ccc !important;
}

.wmsExportNotifyContent .content table tbody {
    display: inline-block;
    background-color: #fff;
}

.wmsExportNotifyContent .content table tbody tr:hover th {
    background-color: #78b9ee !important;
}

.wmsExportNotifyContent .content table tbody tr {}

.wmsExportNotifyContent .content table tbody tr th {
    border: 1px solid #ccc !important;
    padding: 5px 10px;
    text-align: center;
    font-size: 15px;
    height: 30px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(1) {
    min-width: 100px;
    position: sticky;
    left: 0;
    z-index: 10;
    background-color: #f4f4f4;
    top: 32px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(2) {
    min-width: 300px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(3) {
    min-width: 150px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(4) {
    min-width: 200px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(5) {
    min-width: 150px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(6) {
    min-width: 150px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(7) {
    min-width: 120px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(8) {
    min-width: 250px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(9) {
    min-width: 250px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(10) {
    min-width: 250px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(11) {
    min-width: 250px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(12) {
    min-width: 250px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(13) {
    min-width: 250px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(14) {
    min-width: 250px;
}

.wmsExportNotifyContent .content table tbody tr th:nth-child(15) {
    min-width: 300px;
    position: sticky;
    right: 0;
    z-index: 10;
    background-color: #f4f4f4;
    top: 32px;
}

.wmsExportNotifyContent .bottom {
    height: 10%;
}

.wmsExportNotifyContent .bottom button {
    border-radius: 5px;
    border: none;
    padding: 5px 13px;
}

.wmsExportNotifyContent .bottom select {
    height: 26px;
    width: 50px;
    margin: 0px 5px;
}

.wmsExportNotifyContent .bottom span {
    margin-left: 10px;
}
</style>