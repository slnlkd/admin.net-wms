<script lang="ts" name="wmsMoveOrder" setup>
import { ref, reactive, onMounted, onUnmounted } from "vue";
import { ElMessage } from "element-plus";
import { useChildOpenAndClose } from '/@/stores/useChildOpenAndClose';
import { useTableAddMoveOrder } from '/@/stores/wmsMoveOrder/Table/useTableAddMoveOrder';

//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);

// 引入pinia
const useChildOpenAndCloseStore = useChildOpenAndClose();
const useTableAddMoveOrderStore = useTableAddMoveOrder();

const state = reactive({
	title: '添加移库单',
	loading: false,
	showDialog: false,
});

// 页面加载时
onMounted(async () => {
	// 页面2的初始化逻辑
	useTableAddMoveOrderStore.GetWmsBaseWareHouseFun();
});

// 页面卸载时
onUnmounted(() => {
	// 页面2的清理逻辑
	useTableAddMoveOrderStore.warehouseIdParam = null; //仓库表id
	useTableAddMoveOrderStore.moveOutLaneParam = null; //移出巷道
	useTableAddMoveOrderStore.moveOutGoodsParam = null; //移出货位
	useTableAddMoveOrderStore.moveInLaneParam = null; //移入巷道
	useTableAddMoveOrderStore.moveInGoodsParam = null; //移入货位

	useTableAddMoveOrderStore.warehouseIdList = [
		//仓库表
		{ id: -1, warehouseName: '请选择' },
	];
	useTableAddMoveOrderStore.moveOutLaneList = [
		//移出巷道
		{ id: -1, code: '', moveOutName: '请选择' },
	];
	useTableAddMoveOrderStore.moveOutGoodsList = [
		//移出货位
		{ id: -1, code: '请选择' },
	];
	useTableAddMoveOrderStore.moveInLaneList = [
		//移入巷道
		{ id: -1, code: '', moveInName: '请选择' },
	];
	useTableAddMoveOrderStore.moveInGoodsList = [
		//移入货位
		{ id: -1, code: '请选择' },
	];

	useTableAddMoveOrderStore.tableTop = [];
	useTableAddMoveOrderStore.tableList = [];
	useTableAddMoveOrderStore.selectTableRow = null;
});

// 打开弹窗
const openDialog = async (row: any, title: string = '新增移库单') => {
	state.title = title;
	state.showDialog = true;
};

// 关闭弹窗
const closeDialog = () => {
	emit("reloadTable");
	state.showDialog = false;
};

// 当仓库下拉框修改值的时候触发
const changeWareHouse = () => {
	useTableAddMoveOrderStore.moveOutLaneParam = null; //移出巷道
	useTableAddMoveOrderStore.moveOutGoodsParam = null; //移出货位
	useTableAddMoveOrderStore.moveInLaneParam = null; //移入巷道
	useTableAddMoveOrderStore.moveInGoodsParam = null; //移入货位
	useTableAddMoveOrderStore.GetWmsBaseLanewayOutFun(-1);
};

// 当移出巷道修改值的时候触发
const changeMoveOutLane = () => {
	useTableAddMoveOrderStore.moveOutGoodsParam = null; //移出货位
	useTableAddMoveOrderStore.GetWmsBaseLanewayInFun(useTableAddMoveOrderStore.moveOutLaneParam);
	useTableAddMoveOrderStore.GetSelectGoodsOutFun();
};

// 当移入巷道修改值的时候触发
const changeMoveInLane = () => {
	useTableAddMoveOrderStore.moveInGoodsParam = null; //移入货位
	useTableAddMoveOrderStore.GetSelectGoodsInFun();
};

// 点击添加明细
const addMaterialDetail = () => {
	if (useTableAddMoveOrderStore.warehouseIdParam == null || useTableAddMoveOrderStore.warehouseIdParam == -1) {
		ElMessage.warning("请选择仓库");
		return;
	} else if (useTableAddMoveOrderStore.moveOutLaneParam == null || useTableAddMoveOrderStore.moveOutLaneParam == -1) {
		ElMessage.warning("请选择移出巷道");
		return;
	} else if (useTableAddMoveOrderStore.moveOutGoodsParam == null || useTableAddMoveOrderStore.moveOutGoodsParam == -1) {
		ElMessage.warning("请选择移出货位");
		return;
	} else if (useTableAddMoveOrderStore.moveInLaneParam == null || useTableAddMoveOrderStore.moveInLaneParam == -1) {
		ElMessage.warning("请选择移入巷道");
		return;
	} else if (useTableAddMoveOrderStore.moveInGoodsParam == null || useTableAddMoveOrderStore.moveInGoodsParam == -1) {
		ElMessage.warning("请选择移入货位");
		return;
	}
	useTableAddMoveOrderStore.AddWmsMoveFun(useTableAddMoveOrderStore.moveOutGoodsParam, useTableAddMoveOrderStore.moveInGoodsParam);
};

// 下发任务
const sendTask = () => {
	// 判断tablelist是否有数据
	if (!useTableAddMoveOrderStore.tableList || useTableAddMoveOrderStore.tableList.length === 0) {
		ElMessage.warning("请先添加移库明细后再下发任务！");
		return;
	}
	//useTableAddMoveOrderStore.AddWmsMoveNotifyFun(useTableAddMoveOrderStore.tableList);
	useTableAddMoveOrderStore.AddWmsMoveNotifyFun(useTableAddMoveOrderStore.tableList)
	.then(result => {
		if (result.success) {
			useTableAddMoveOrderStore.moveOutLaneParam = null; //移出巷道
			useTableAddMoveOrderStore.moveOutGoodsParam = null; //移出货位
			useTableAddMoveOrderStore.moveInLaneParam = null; //移入巷道
			useTableAddMoveOrderStore.moveInGoodsParam = null; //移入货位
			useTableAddMoveOrderStore.GetWmsBaseLanewayOutFun(-1);
			useTableAddMoveOrderStore.tableTop = [];
			useTableAddMoveOrderStore.tableList = [];
			useTableAddMoveOrderStore.selectTableRow = null;

			emit("reloadTable");
			
			ElMessage.success(result.message || '下发任务成功');
			// 其他成功后的操作...
		} else {
			ElMessage.error(result.message || '下发任务失败');
		}
	})
	.catch(error => {
		console.error('执行下发任务时发生错误:', error);
		ElMessage.error('执行下发任务时发生错误');
	});
};

//将属性或者函数暴露给父组件
defineExpose({ openDialog });
</script>

<template>
	<div class="wmsMoveOrder-container">
		<el-dialog v-model="state.showDialog" :width="1350" draggable :close-on-click-modal="false">
			<template #header>
				<div style="color: #fff; display: flex; justify-content: space-between; align-items: center;">
					<span>{{ state.title }}</span>
				</div>
			</template>
			<!-- 移库表单 -->
			<el-form :inline="true" @submit.native.prevent class="handle-form">
				<el-form-item label="移库仓库：">
					<el-select v-model="useTableAddMoveOrderStore.warehouseIdParam" placeholder="移库仓库" @change="changeWareHouse" style="width: 200px;">
						<el-option v-for="i in useTableAddMoveOrderStore.warehouseIdList" :value="i.id" :label="i.warehouseName" :key="i.id">
							{{ i.warehouseName }}
						</el-option>
					</el-select>
				</el-form-item>
				<div style="display: flex; flex-wrap: wrap; gap: 20px;">
					<el-form-item label="移出巷道：">
						<el-select v-model="useTableAddMoveOrderStore.moveOutLaneParam" placeholder="移出巷道" @change="changeMoveOutLane" style="width: 200px;">
							<el-option v-for="i in useTableAddMoveOrderStore.moveOutLaneList" :value="i.id" :label="i.moveOutName" :key="i.id">
								{{ i.moveOutName }}
							</el-option>
						</el-select>
					</el-form-item>

					<el-form-item label="移出货位：">
						<el-select v-model="useTableAddMoveOrderStore.moveOutGoodsParam" placeholder="移出货位" style="width: 200px;">
							<el-option v-for="i in useTableAddMoveOrderStore.moveOutGoodsList" :value="i.code" :label="i.code" :key="i.code">
								{{ i.code }}
							</el-option>
						</el-select>
					</el-form-item>

					<el-form-item label="移入巷道：">
						<el-select v-model="useTableAddMoveOrderStore.moveInLaneParam" placeholder="移入巷道" @change="changeMoveInLane" style="width: 200px;">
							<el-option v-for="i in useTableAddMoveOrderStore.moveInLaneList" :label="i.moveInName" :key="i.id" :value="i.id">
								{{ i.moveInName }}
							</el-option>
						</el-select>
					</el-form-item>

					<el-form-item label="移入货位：">
						<el-select v-model="useTableAddMoveOrderStore.moveInGoodsParam" placeholder="移入货位" style="width: 200px;">
							<el-option v-for="i in useTableAddMoveOrderStore.moveInGoodsList" :label="i.code" :key="i.code" :value="i.code">
								{{ i.code }}
							</el-option>
						</el-select>
					</el-form-item>
				</div>

				<div style="margin-top: 20px;">
					<el-button type="primary" icon="ele-Plus" @click="addMaterialDetail" v-auth="'sysRole:add'">
						添加明细
					</el-button>
					<el-button type="primary" icon="ele-Plus" @click="sendTask" v-auth="'sysRole:add'" style="margin-left: 10px;">
						下发移库
					</el-button>
				</div>
			</el-form>

			<!-- 移库明细表格 -->
			<el-table style="width: 100%; margin-top: 20px;" highlight-current-row border :data="useTableAddMoveOrderStore.displayTableListComputed">
				<el-table-column type="index" label="序号" width="50" align="center" header-align="center" sortable='custom' fixed="left" />

				<el-table-column prop="" label="移库流水单据id" min-width="300" align="center" header-align="center" v-if="false" />

				<el-table-column prop="stockCode" label="载具条码" min-width="180" align="center" header-align="center" />
				<el-table-column prop="stockSlotCode" label="移出货位" min-width="180" align="center" header-align="center" />
				<el-table-column prop="moveInSlotCode" label="移入货位" min-width="180" align="center" header-align="center" />

				<el-table-column prop="materialId" label="物料id" min-width="300" align="center" header-align="center" v-if="false" />

				<el-table-column prop="materialCode" label="物料编码" min-width="180" align="center" header-align="center" />
				<el-table-column prop="materialName" label="物料名称" min-width="200" align="center" header-align="center" />
				<el-table-column prop="lotNo" label="批次" min-width="180" align="center" header-align="center" />
				<el-table-column prop="stockDate" label="入库时间" min-width="180" align="center" header-align="center" />
			</el-table>

			
		</el-dialog>
	</div>
</template>

<style lang="scss" scoped>
.wmsMoveOrder-container {
	.handle-form {
		.el-form-item {
			margin-bottom: 15px;
		}
	}

	.close {
		background: none;
		border: none;
		color: #fff;
		font-size: 18px;
		cursor: pointer;
		padding: 0;
		margin: 0;
	}

	:deep(.el-dialog__body) {
		padding: 20px;
	}
}
</style>