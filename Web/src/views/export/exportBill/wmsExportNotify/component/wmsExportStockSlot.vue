<template>
    <div class="total">
        <span><img :src="imgBianJiImgPath" />选择物料信息</span>
        <button @click="useChildOpenAndCloseStore.close_wmsExportNotifyAdd_C_child_2()" class="close">X</button>
    </div>
    <div class="">
         <el-row :gutter="5" style="width: 100%; height: 50%; flex: 1">
            <el-col :span="24" :xs="24" style="display: flex; height: 100%; flex: 1">
                <el-card class="full-table" shadow="hover" :body-style="{ height: 'calc(100% - 51px)' }">
                    <el-form :inline="true" @submit.native.prevent class="handle-form">
                        <el-form-item label="关键字：">
                            <el-input v-model="useTablewmsExportStockSlotStore.whereStrParam" placeholder="关键字"></el-input>
                        </el-form-item>
                        <el-form-item>
                            <el-button type="primary" icon="ele-Search"
                                @click="useTablewmsExportStockSlotStore.SelectMaterialFun()" v-auth="'sysDifflog:page'"> 查询
                            </el-button>
                        </el-form-item>
                        <el-form-item>
                            <el-button type="primary" icon="ele-Plus" @click="selectedMaterialFun" v-auth="'sysRole:add'"> 确定
                            </el-button>
                        </el-form-item>
                    </el-form>

                    <el-table style="width: 100%"  highlight-current-row @sort-change="" border
                        :data="useTablewmsExportStockSlotStore.displayNotifyListComputed" @row-click="" @selection-change="useTablewmsExportStockSlotStore.SelectRowReturnFun">
                        <el-table-column type="selection" label="选择" width="100" align="center" header-align="center" sortable='custom' fixed="left" />
    
                        <el-table-column prop="wmsBaseMaterialId" label="物料表id" min-width="150" header-align="center" align="center" v-if="false" />
                        <el-table-column prop="wmsStockId" label="库存表id" min-width="150" header-align="center" align="center" v-if="false" />
                        <el-table-column prop="materialType" label="物料类型（根据字典获取）" min-width="150" header-align="center" align="center" v-if="false" />
                        <el-table-column prop="materialUnitId" label="物料单位id" min-width="150" header-align="center" align="center" v-if="false" />
                       
                        <el-table-column prop="materialCode" label="物料编码" min-width="150" header-align="center"  align="center"/>
                        <el-table-column prop="materialName" label="物料名称" min-width="150" header-align="center" align="center" />
                        <el-table-column prop="materialStandard" label="物料规格" min-width="150" header-align="center" align="center"/>
                        <el-table-column prop="lotNo" label="批次" min-width="150" header-align="center"  align="center"/>
                        <el-table-column prop="materialValidityDayStr" label="有效期" min-width="250" header-align="center"  align="center" />
                        <el-table-column prop="boxQuantity" label="整箱数量" min-width="150" header-align="center" align="center" />
                        <el-table-column prop="stockQuantity" label="库存数量" min-width="150" header-align="center" align="center" />
                        <el-table-column prop="inspectionStatusStr" label="质检状态" min-width="150" header-align="center" align="center" >
                            <template #default="scope">
                                <span :class="executeFlagColor(scope.row.inspectionStatusStr)">{{ scope.row.inspectionStatusStr }}</span>
                            </template>
                        </el-table-column>
                        <el-table-column prop="materialModel" label="物料型号" min-width="150" header-align="center"  align="center"/>
                        <el-table-column prop="materialTypeStr" label="物料类型名称" min-width="150" header-align="center"  align="center"/>
                        <el-table-column prop="materialUnitStr" label="物料单位名称" min-width="150" header-align="center"  align="center"/>
                        <el-table-column prop="ProductionDate" label="生产日期" min-width="150" header-align="center"  align="center"/>
                        <el-table-column prop="ValidateDay" label="失效日期" min-width="150" header-align="center"  align="center"/>
                    </el-table>
                    <el-pagination v-model:currentPage="useTablewmsExportStockSlotStore.page"
                        v-model:page-size="useTablewmsExportStockSlotStore.pageSize" :total="useTablewmsExportStockSlotStore.total"
                        :page-sizes="[10, 20, 50, 100]" size="small" background @size-change="handleDictTypeSizeChange"
                        @current-change="handleDictTypeCurrentChange"
                        layout="total, sizes, prev, pager, next, jumper" />
                </el-card>
            </el-col>
        </el-row> 
    </div>
</template>















<script lang="ts" name="wmsExportStockSlot" setup>

import axios from 'axios';
import { ref, reactive, onMounted, toRef, nextTick, toRefs } from 'vue';
import { clearReactive } from '/@/utils/clearReactive';
import ImgPath from '/@/assets/Business_Card.png';
import SelectCompany from './selectCompany.vue';
import GlassImg from '/@/assets/010_magnifying_glass.png';
import CloseImg from '/@/assets/close.png';
import { useChildOpenAndClose } from '/@/stores/useChildOpenAndClose';
import {  useTablewmsExportStockSlot} from '/@/stores/wmsExportNotify/Tables/wmsExportStockSlot';
import OkImg from '/@/assets/Ok.png';
import BianJiImg from '/@/assets/BianJi.png';

//图片路径
let okImgPath = ref(OkImg);
let imgClosePath = ref(CloseImg);
let glassImgPath = ref(GlassImg);
let imgBianJiImgPath = ref(BianJiImg);


// //add组件传进来的参数 
// let props = defineProps(['exportBillTypeParam']);
// let exportBillTypeParamRef = toRef(props,'exportBillTypeParam')
// console.log("props参数",exportBillTypeParamRef.value)



//调用pinia
const useChildOpenAndCloseStore = useChildOpenAndClose();
const useTablewmsExportStockSlotStore = useTablewmsExportStockSlot();


//向Add组件传递的方法
let emit = defineEmits(['callSelectedMaterial'])


//页面加载完成
onMounted(async () => {
    try {
        useTablewmsExportStockSlotStore.SelectMaterialFun();
        
        
    }
    catch (error) {
        console.log("错误")
    }
})



// 改变页面容量
const handleDictTypeSizeChange = (val: number) => {
  useTablewmsExportStockSlotStore.pageSize = val;
  useTablewmsExportStockSlotStore.SelectMaterialFun();
};

// 改变页码序号
const handleDictTypeCurrentChange = (val: number) => {
  useTablewmsExportStockSlotStore.page = val;
  useTablewmsExportStockSlotStore.SelectMaterialFun();
};


//表格--执行状态颜色转换
function executeFlagColor(value: string | null) {

    if (value == "待检验") {
        return 'greyclass'
    }
    else if (value == "合格") {
        return 'greenclass'
    }
    else if(value == "不合格"){
        return 'redclass'
    }
    else{
        return 'nullclass'
    }

}
//点击确定按钮
function selectedMaterialFun(){

    emit('callSelectedMaterial');
}


</script>













<style scoped>






/* .body{
    width: 100%;
    height: 95%;
}
.body .top{
    width: 100%;
    height: 20%;
}
.body .top .input-div {
    display: inline-block;
    padding: 10px 30px;
    padding-left: 65px;
}

.body .top .input-div span {
    color: #000;
    font-size: 15px;
}

.body .top .input-div input {
    height: 40px;
    display: inline-block;
    border: 1.5px solid #ccc;
    padding: 10px;
    border-radius: 3px;
    width: 200px;
}

.body .top .input-div select {
    height: 40px;
    display: inline-block;
    border: 1.5px solid #ccc;
    padding: 10px;
    border-radius: 3px;
    width: 200px;
}




.body .content{
    width: 100%;
    height: 80%;
} */
</style>
