import {useBaseApi} from '/@/api/base';

// 入库单接口服务
export const useWmsImportNotifyApi = () => {
	const baseApi = useBaseApi("wmsImportNotify");
	return {
		// 分页查询入库单
		page: baseApi.page,
		// 查看入库单详细
		detail: baseApi.detail,
		// 新增入库单
		add: baseApi.add,
		// 更新入库单
		update: baseApi.update,
		// 删除入库单
		delete: baseApi.delete,
		//关单
		complete:baseApi.complete,
		//作废
		invalid:baseApi.invalid,
		// 批量删除入库单
		batchDelete: baseApi.batchDelete,
		// 导出入库单数据
		exportData: baseApi.exportData,
		// 导入入库单数据
		importData: baseApi.importData,
		// 下载入库单数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
		// 获取下拉列表数据
		getDropdownData: (fromPage: Boolean = false, cancel: boolean = false) => baseApi.dropdownData({ fromPage }, cancel),
	}
}

// 入库单实体
export interface WmsImportNotify {
	// 主键Id
	id: number;
	// 所属仓库
	warehouseId: number;
	// 入库单号
	importBillCode: string;
	// 执行状态
	importExecuteFlag: string;
	// 单据类型
	billType: number;
	// 部门ID
	departmentId: number;
	// 供货单位
	supplierId: number;
	// 客户单位
	customerId: number;
	// 来源
	source: string;
	// 外部单号
	outerBillCode: string;
	// 外部单据ID
	outerMainId: string;
	// 软删除
	isDelete: number;
	// 创建者Id
	createUserId: number;
	// 修改者Id
	updateUserId: number;
	// 生产商ID
	produceId: string;
	// 创建者姓名
	createUserName: string;
	// 创建时间
	createTime: string;
	// 修改者姓名
	updateUserName: string;
	// 修改时间
	updateTime: string;
	// 备注
	remark: string;
}