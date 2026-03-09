import {useBaseApi} from '/@/api/base';

// 出库单据表接口服务
export const useWmsExportNotifyApi = () => {
	const baseApi = useBaseApi("wmsExportNotify");
	return {
		// 分页查询出库单据表
		page: baseApi.page,
		// 查看出库单据表详细
		detail: baseApi.detail,
		// 新增出库单据表
		add: baseApi.add,
		// 更新出库单据表
		update: baseApi.update,
		// 删除出库单据表
		delete: baseApi.delete,
		// 批量删除出库单据表
		batchDelete: baseApi.batchDelete,
		// 导出出库单据表数据
		exportData: baseApi.exportData,
		// 导入出库单据表数据
		importData: baseApi.importData,
		// 下载出库单据表数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 出库单据表实体
export interface WmsExportNotify {
	// 主键Id
	id: number;
	// 出库单据
	exportBillCode: string;
	// 单据类型
	exportBillType: string;
	// 出库批次
	exportLotNo: string;
	// 物料ID
	materialId: string;
	// 仓库ID
	warehouseId: string;
	// 出库序号
	exportListNo: string;
	// 部门ID
	exportDepartmentId: string;
	// 供应商ID
	exportSupplierId: string;
	// 客户ID
	exportCustomerId: string;
	// 生产日期
	exportProductionDate: string;
	// 失效日期
	exportLostDate: string;
	// 计划出库数量
	exportQuantity: number;
	// 任务下发数量
	exportFactQuantity: number;
	// 任务完成数量
	exportCompleteQuantity: number;
	// 已上传数量
	exportUploadQuantity: number;
	// 建单时间
	exportDate: string;
	// 执行标志（0待执行、1正在分配、 2正在执行、3已完成、5已上传） 4作废
	exportExecuteFlag: number;
	// 备注
	exportRemark: string;
	// 外部单据编码
	outerBillCode: string;
	// 外部单据ID
	outerMainId: string;
	// 来源（wms sap）
	source: string;
	// 拣货区
	pickingArea: string;
	// 拼箱状态（0：不拼箱1：拼箱）
	pXStatus: number;
	// 整托出库口
	wholeOutWare: string;
	// 分拣出库口
	sortOutWare: string;
	// 创建时间
	createTime: string;
	// 更新时间
	updateTime: string;
	// 创建者Id
	createUserId: number;
	// 创建者姓名
	createUserName: string;
	// 修改者Id
	updateUserId: number;
	// 修改者姓名
	updateUserName: string;
	// 软删除
	isDelete?: boolean;
}