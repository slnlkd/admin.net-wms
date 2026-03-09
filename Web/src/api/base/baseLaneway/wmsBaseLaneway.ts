import {useBaseApi} from '/@/api/base';

// 巷道表接口服务
export const useWmsBaseLanewayApi = () => {
	const baseApi = useBaseApi("wmsBaseLaneway");
	return {
		// 分页查询巷道表
		page: baseApi.page,
		// 查看巷道表详细
		detail: baseApi.detail,
		// 新增巷道表
		add: baseApi.add,
		// 更新巷道表
		update: baseApi.update,
		// 删除巷道表
		delete: baseApi.delete,
		// 批量删除巷道表
		batchDelete: baseApi.batchDelete,
		// 导出巷道表数据
		exportData: baseApi.exportData,
		// 导入巷道表数据
		importData: baseApi.importData,
		// 下载巷道表数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
		// 获取下拉列表数据
		getDropdownData: (fromPage: Boolean = false, cancel: boolean = false) => baseApi.dropdownData({ fromPage }, cancel),
	}
}

// 巷道表实体
export interface WmsBaseLaneway {
	// 主键Id
	id: number;
	// 所属仓库
	warehouseId?: number;
	// 巷道编码
	lanewayCode?: string;
	// 巷道名称
	lanewayName?: string;
	// 巷道状态
	lanewayStatus: boolean;
	// 备注
	remark: string;
	// 优先级
	lanewayPriority: number;
	// 储存条件
	lanewayTemp: string;
	// 巷道口状态
	lanewayType: number;
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