import {useBaseApi} from '/@/api/base';

// 托盘管理接口服务
export const useWmsSysStockCodeApi = () => {
	const baseApi = useBaseApi("wmsSysStockCode");
	return {
		// 分页查询托盘管理
		page: baseApi.page,
		// 查看托盘管理详细
		detail: baseApi.detail,
		// 新增托盘管理
		add: baseApi.add,
		// 更新托盘管理
		update: baseApi.update,
		// 删除托盘管理
		delete: baseApi.delete,
		// 批量删除托盘管理
		batchDelete: baseApi.batchDelete,
		// 导出托盘管理数据
		exportData: baseApi.exportData,
		// 导入托盘管理数据
		importData: baseApi.importData,
		// 下载托盘管理数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
		// 获取最大条码号
		getSysStockCode: baseApi.getSysStockCode,
		// 获取下拉列表数据
		getDropdownData: (fromPage: Boolean = false, cancel: boolean = false) => baseApi.dropdownData({ fromPage }, cancel),
	}
}

// 托盘管理实体
export interface WmsSysStockCode {
	// 主键Id
	id: number;
	// 载具号
	stockCode?: string;
	// 所属仓库
	warehouseId?: number;
	// 条码状态
	status: number;
	// 打印次数
	printCount: number;
	// 托盘类型
	stockType?: number;
	// 软删除
	isDelete?: boolean;
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
}