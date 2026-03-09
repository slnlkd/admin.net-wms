<template>
    <div class="total">
        <span><img :src="imgBianJiImgPath" />领用单位</span>
        <button @click="useChildOpenAndCloseStore.close_wmsExportNotifyAdd_C_child_1()" class="close">X</button>
    </div>

    <div class="">
        <el-row :gutter="5" style="width: 100%; height: 50%; flex: 1">
            <el-col :span="24" :xs="24" style="display: flex; height: 100%; flex: 1">
                <el-card class="full-table" shadow="hover" :body-style="{ height: 'calc(100% - 51px)' }">
                    <el-form :inline="true" @submit.native.prevent class="handle-form">
                        <el-form-item label="关键字：">
                            <el-input v-model="useTableSelectCompanyStore.whereStrParam" placeholder="关键字："></el-input>
                        </el-form-item>
                        <el-form-item>
                            <el-button type="primary" icon="ele-Search" @click="useTableSelectCompanyStore.ShowCompanyTableFun()" v-auth="'sysDifflog:page'"> 查询
                            </el-button>
                        </el-form-item>
                    </el-form>

                    <el-table style="width: 100%" @row-click="" highlight-current-row @sort-change="" border :data="useTableSelectCompanyStore.displayNotifyListComputed">
                        <el-table-column type="index" label="序号" width="55"  sortable='custom' align="center" header-align="center" />
                        <el-table-column prop="id" label="客户id" min-width="150" header-align="center" align="center" v-if="false"></el-table-column>
                        <el-table-column prop="customerCode" label="客户编码" min-width="150" header-align="center" align="center"></el-table-column>
                        <el-table-column prop="customerName" label="客户名称" min-width="200" header-align="center" align="center"></el-table-column>
                        <el-table-column prop="customerAddress" label="客户地址" min-width="150" header-align="center" align="center"></el-table-column>
                        <el-table-column label="操作" width="300" fixed="right" align="center" header-align="center">
                            <template #default="scope">
                                <el-tooltip content="确定" v-if="scope.row.id!=null && scope.row.id>0">
                                    <el-button type="success" icon="ele-Edit" @click="clickOkFun(scope.row.id,scope.row.customerName,scope.row.customerCode)">确定</el-button>
                                </el-tooltip>

                            </template>
                        </el-table-column>
                    </el-table>
                    <el-pagination v-model:currentPage="useTableSelectCompanyStore.page"
                        v-model:page-size="useTableSelectCompanyStore.pageSize" :total="useTableSelectCompanyStore.total"
                        :page-sizes="[10, 20, 50, 100]" size="small" background @size-change="handleDictTypeSizeChange"
                        @current-change="handleDictTypeCurrentChange"
                        layout="total, sizes, prev, pager, next, jumper" />
                </el-card>
            </el-col>
        </el-row>
    </div>

</template>






<script lang="ts" name="selectCompany" setup>
import { ref, onMounted, computed } from 'vue';
import axios from 'axios';
import GalssImg from '/@/assets/010_magnifying_glass.png'
import CloseImg from '/@/assets/close.png';
import BianJiImg from '/@/assets/BianJi.png';
import { useChildOpenAndClose } from '/@/stores/useChildOpenAndClose';
import { useTableSelectCompany } from '/@/stores/wmsExportNotify/Tables/useTableSelectCompany';



let isshow = ref(false)

//图片路径
let imgClosePath = ref(CloseImg);
let imgBianJiImgPath = ref(BianJiImg);
let galssimg = ref(GalssImg)



let exportId = ref(); //暴露的id
let exportCustomerName = ref(); //暴露的客户名称
let exportCustomerCode = ref(); //暴露的客户code

//暴露变量或函数
let emit = defineEmits(['callReturnCustomer'])

//调用pinia
const useChildOpenAndCloseStore = useChildOpenAndClose();
const useTableSelectCompanyStore = useTableSelectCompany();



//页面加载
onMounted(() => {
    useTableSelectCompanyStore.ShowCompanyTableFun();
})



//点击表格上的确定按钮
function clickOkFun(id: number, customerName: string,customerCode:string) {
    exportId.value = id;
    exportCustomerName.value = customerName
    exportCustomerCode.value = customerCode
    //useChildOpenAndCloseStore.close_wmsExportNotifyAdd_C_child_1();
    emit('callReturnCustomer');
}


// 改变页面容量
const handleDictTypeSizeChange = (val: number) => {
  useTableSelectCompanyStore.pageSize = val;
  useTableSelectCompanyStore.ShowCompanyTableFun();
};

// 改变页码序号
const handleDictTypeCurrentChange = (val: number) => {
  useTableSelectCompanyStore.page = val;
  useTableSelectCompanyStore.ShowCompanyTableFun();
};


// 暴露“加载出库单据”方法等其他数据
defineExpose({ exportId, exportCustomerName,exportCustomerCode })



/*
// let closePath = ref(CloseImg);

// //加载领用单位表格的参数
// let whereParam = ref({
//     page: 1,
//     pageSize: 20,
//     total: 0,
//     field: 'createTime', // 默认的排序字段
//     order: 'descending', // 排序方向
//     descStr: 'descending', // 降序排序的关键字符
// })
// let whereStr = ref("")//关键字
// //加载领用单位表格的真实数据
// let companyList = ref([{
//     id: null,
//     customerCode: null,
//     customerName: null,
//     customerAddress: null
// }]);
// let companyListOfPage = ref([{}]); //加载领用单位表格的真实数据 加 页码页数等
// let galssimg = ref(GalssImg);


// let tableTop = ref([
//     { id: "01", name: "id" },
//     { id: "02", name: "客户单位编码" },
//     { id: "03", name: "客户单位名称" },
//     { id: "04", name: "客户单位地址" },
//     { id: "15", name: "操作" }
// ])






// //表格展示的数据
// let displayCompanyList = computed(()=> {
//     let result = [{
//         id: null,
//         customerCode: null,
//         customerName: null,
//         customerAddress: null
//     }]
//     result = companyList.value
//     while (result.length < whereParam.value.pageSize) {
//         result.push({
//             id: null,
//             customerCode: null,
//             customerName: null,
//             customerAddress: null
//         });
//     }
//     return result;
// })







// //加载页面
// onMounted(async () => {
//     try {
//         InitCompany(whereParam.value);

//     }
//     catch (error) {
//         console.log("单据类型数据失败", error);
//     }
// })



// //加载领用单位表格
// async function InitCompany(params: any) {
//     try {
//         let billTypeResult = await axios.post(window.__env__.VITE_API_URL + "/api/wmsExportNotify/showCompanyTable", params)
//         companyListOfPage.value = billTypeResult.data.result;
//         companyList.value = billTypeResult.data.result.items;

//     }
//     catch (error) {
//         console.log("加载失败")
//     }
// }
// //查询按钮
// function showNotifyTable() {

// }

*/

</script>










<style scoped>
/* .selectCompany {
    width: 100%;
    height: 95%;
}

.selectCompany .top {
    width: 100%;
    height: 10%;
}

.selectCompany .top .input-div {
    display: inline-block;
    padding: 10px 30px;
    padding-left: 65px;
}

.selectCompany .top .input-div span {
    color: #000;
    font-size: 15px;
}

.selectCompany .top .input-div input {
    height: 40px;
    display: inline-block;
    border: 1.5px solid #ccc;
    padding: 10px;
    border-radius: 3px;
    width: 200px;
}

.selectCompany .top .btn {
    display: inline-block;
    background: #86c586;
    border: none;
    color: #fff;
    font-size: 15px;
    padding: 4px 20px;
    border-radius: 5px;
}

.selectCompany .top .btn:hover {

    background: #63da63;
}

.selectCompany .top .btn:active {

    background: #1d1d1d;
}

.selectCompany .top .btn img {
    width: 15px;
    height: 15px;
    margin-right: 5px;
    position: relative;
    top: 2px;
}


.selectCompany .content {
    width: 100%;
    height: 90%;
    padding: 10px 50px;
}


.selectCompany .content table {
    border: 1px solid #ccc;
    border-top: 1px solid red;
    display: block;
    border-collapse: collapse;
    max-height: 100%;
    max-width: 100%;
    overflow: auto;
}

.selectCompany .content table thead {
    display: block;
    background-color: #dddada;
    position: sticky;
    top: 0;
}




.selectCompany .content table thead tr {
    display: block;
}

.selectCompany .content table thead tr th {
    border: 1px solid #ccc;
    padding: 6px 10px;
    text-align: center;
    font-size: 15px;
    height: 35px;
    display: inline-block;
}

 .selectCompany .content table thead tr th:nth-child(1) {
    min-width: 100px;
    position: sticky;
    left: 0;
    top: 0;
    z-index: 20;
    background-color: #dddada;
    border: 1px solid #ccc !important;
} 
.selectCompany .content table thead tr th:nth-child(1) {
    min-width: 25%;
}
.selectCompany .content table thead tr th:nth-child(2) {
    min-width: 25%;
}

.selectCompany .content table thead tr th:nth-child(3) {
    min-width: 25%;
}

.selectCompany .content table thead tr th:nth-child(4) {
    min-width: 25%;
}

.selectCompany .content table thead tr th:nth-child(5) {
    min-width: 25%;
}



 .selectCompany .content table thead tr th:nth-child(15) {
    min-width: 300px;
    position: sticky;
    right: 0;
    top: 0;
    z-index: 20;
    background-color: #dddada;
    border: 1px solid #ccc !important;
} 

.selectCompany .content table tbody {
    display: block;
    background-color: #fff;
}

.selectCompany .content table tbody tr:hover th {
    background-color: #78b9ee !important;
}

.selectCompany .content table tbody tr {
    display: block;
}

.selectCompany .content table tbody tr th {
    border: 1px solid #ccc !important;
    padding: 6px 10px;
    text-align: center;
    font-size: 15px;
    height: 35px;
    display: inline-block;
}

 .selectCompany .content table tbody tr th:nth-child(1) {
    min-width: 100px;
    position: sticky;
    left: 0;
    z-index: 10;
    background-color: #f4f4f4;
    top: 32px;
} 
.selectCompany .content table tbody tr th:nth-child(1) {
    min-width: 25%;
}
.selectCompany .content table tbody tr th:nth-child(2) {
    min-width: 25%;
}

.selectCompany .content table tbody tr th:nth-child(3) {
    min-width: 25%;
}

.selectCompany .content table tbody tr th:nth-child(4) {
    min-width: 25%;
}

.selectCompany .content table tbody tr th:nth-child(5) {
    min-width: 25%;
    padding: 2px 10px;
}



 .selectCompany .content table tbody tr th:nth-child(15) {
    min-width: 300px;
    position: sticky;
    right: 0;
    z-index: 10;
    background-color: #f4f4f4;
    top: 32px;
} 


.selectCompany .content table tbody tr th .btn{
    display: inline-block;
    border-radius: 5px;
    background: green;
    padding: 4px 20px;
    color: #fff;
}
.selectCompany .content table tbody tr th .btn:hover{
    background: rgb(86, 190, 86);
} */
</style>