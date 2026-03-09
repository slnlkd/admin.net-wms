<template>
	<!-- 盘点箱码明细对话框 -->
	<el-dialog v-model="state.showDialog" title="盘点箱码明细" width="1400px" :close-on-click-modal="false"
		destroy-on-close @closed="handleDialogClosed">
		<!-- 箱码明细区域 -->
		<div class="box-detail-section">
			<!-- 箱码明细表格 -->
			<el-table :data="state.tableData" v-loading="state.tableLoading" border style="width: 100%; height: 100%"
				tooltip-effect="light" row-key="id">
				<!-- 序号列 -->
				<el-table-column type="index" label="序号" width="60" align="center" fixed="left" />
				<!-- 箱码编号列 -->
				<el-table-column prop='boxCode' label='箱码编号' fixed="left" align="center" width="180"
					show-overflow-tooltip />
				<!-- 盘点结果列 -->
				<el-table-column prop='checkResult' label='盘点结果' align="center" width="100" show-overflow-tooltip>
					<template #default="scope">
						<g-sys-dict v-model="scope.row.checkResult" code="CheckResultStatus" />
					</template>
				</el-table-column>
				<!-- 物料编码列 -->
				<el-table-column prop='materialCode' label='物料编码' align="center" width="150"
					show-overflow-tooltip />
				<!-- 物料名称列 -->
				<el-table-column prop='materialName' label='物料名称' align="center" width="150"
					show-overflow-tooltip />
				<!-- 批次列 -->
				<el-table-column prop='lotNo' label='批次' align="center" width="150"
					show-overflow-tooltip />
				<!-- 数量列 -->
				<el-table-column prop='qty' label='数量' align="center" width="105"
					show-overflow-tooltip />
				<!-- 实际数量列 -->
				<el-table-column prop='realQuantity' label='实际数量' align="center" width="105"
					show-overflow-tooltip />					
				<!-- 零箱标识列 -->
				<el-table-column prop='bulkTank' label='零箱标识' align="center" width="110" show-overflow-tooltip>
					<template #default="scope">
						<g-sys-dict v-model="scope.row.bulkTank" code="BulkTank" />
					</template>
				</el-table-column>
				<!-- 检疫期状态列 -->
				<el-table-column prop='extractStatus' label='检疫期状态' align="center" width="160"
					show-overflow-tooltip>
					<template #default="scope">
						<g-sys-dict v-model="scope.row.extractStatus" code="ExtractStatus" />
					</template>
				</el-table-column>
				<!-- 质检状态列 -->
				<el-table-column prop='inspectionStatus' label='质检状态' align="center" width="160"
					show-overflow-tooltip>
					<template #default="scope">
						<g-sys-dict v-model="scope.row.inspectionStatus" code="QualityInspectionStatus" />
					</template>
				</el-table-column>
				<!-- 生产日期列 -->
				<el-table-column prop='productionDate' label='生产日期' align="center" width="160"
					show-overflow-tooltip />
				<!-- 保质期列 -->
				<el-table-column prop='validateDay' label='保质期' align="center" width="180"
					show-overflow-tooltip />
			</el-table>
			<!-- 分页组件 -->
			<el-pagination v-model:currentPage="state.tableParams.page" v-model:page-size="state.tableParams.pageSize"
				@size-change="(val: any) => handleQuery({ pageSize: val })"
				@current-change="(val: any) => handleQuery({ page: val })"
				layout="total, sizes, prev, pager, next, jumper" :page-sizes="[10, 20, 50, 100, 200, 500]"
				:total="state.tableParams.total" size="small" background />
		</div>
	</el-dialog>
</template>

<script setup lang="ts">
import { reactive } from 'vue'
import { useWmsStockCheckOrderApi } from '/@/api/stockCheck/stockCheckOrder/wmsStockCheckOrder';

// 引入盘点单据API
const wmsStockCheckOrderApi = useWmsStockCheckOrderApi();

// 页面响应式状态
const state = reactive({
	tableLoading: false, // 表格加载状态
	showDialog: false, // 是否显示对话框
	detailId: 0, // 明细ID
	detailCode: "", // 明细编码（盘点单号）
	tableParams: {
		page: 1,
		pageSize: 20,
		total: 0,
		field: 'id',
		order: 'descending',
		descStr: 'descending',
	},
	tableData: [] // 表格数据
})

// 打开对话框
const openDialog = async (detailId: number,detailCode:string) => {
	state.detailId = detailId
	state.detailCode = detailCode
	state.tableParams.page = 1;
	await handleQuery()
	state.showDialog = true;
}

// 对话框关闭处理
const handleDialogClosed = () => {
	state.tableData = []
	state.showDialog = false;
}

// 查询箱码明细数据
const handleQuery = async (params: any = {}) => {
	try {
		state.tableLoading = true;
		state.tableParams = Object.assign(state.tableParams, params);
		// 构建查询参数
		const queryParams = {
			id:state.detailId,
			billCode: state.detailCode,
			page: state.tableParams.page,
			pageSize: state.tableParams.pageSize
		};
		// 调用接口获取箱码明细
		const result = await wmsStockCheckOrderApi.getBoxDetail(queryParams).then(res => res.data.result);
		state.tableParams.total = result?.total;
		// 格式化日期字段
		state.tableData = (result?.items ?? []).map((item: any) => ({
			...item,
			// productionDate: formatDate(item.productionDate),
			// validateDay: formatDate(item.validateDay)
		}));
	} catch (error) {
		console.error('获取盘点箱明细失败:', error);
	} finally {
		state.tableLoading = false;
	}
};

// 暴露方法给父组件
defineExpose({
	openDialog
})
</script>

<style scoped>
/* 箱码明细区域样式 */
.box-detail-section {
	flex: 1;
	display: flex;
	flex-direction: column;
	min-height: 0;
	border: 1px solid #e4e7ed;
	border-radius: 4px;
	padding: 5px;
	height: 600px;
}

/* 分页组件样式 */
.el-pagination {
	margin-top: 10px;
	justify-content: flex-end;
}
</style>
