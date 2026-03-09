import {useBaseApi} from '/@/api/base';

// 仓库表接口服务
export const useWmsBaseWareHouseApi = () => {
	const baseApi = useBaseApi("wmsBaseWareHouse");
	return {
		// 分页查询仓库表
		page: baseApi.page,
		// 查看仓库表详细
		detail: baseApi.detail,
		// 新增仓库表
		add: baseApi.add,
		// 更新仓库表
		update: baseApi.update,
		// 删除仓库表
		delete: baseApi.delete,
		// 批量删除仓库表
		batchDelete: baseApi.batchDelete,
		// 导出仓库表数据
		exportData: baseApi.exportData,
		// 导入仓库表数据
		importData: baseApi.importData,
		// 下载仓库表数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 仓库表实体
export interface WmsBaseWareHouse {
	// 主键Id
	id: number;
	// 仓库编码
	warehouseCode: string;
	// 仓库名称
	warehouseName: string;
	// 仓库类型
	warehouseType: string;
	// 备注
	remark: string;
	// 软删除
	isDelete: number;
	// 创建者Id
	createUserId: number;
	// 创建时间
	createTime: string;
	// 修改者Id
	updateUserId: number;
	// 更新时间
	updateTime: string;
	// 创建者姓名
	createUserName: string;
	// 修改者姓名
	updateUserName: string;
	// 是否允许超出
	isExceeding: boolean;
	// 是否允许超入
	isOverbooking: boolean;
	// 是否允许少出
	isUnderpay: boolean;
	// 是否允许少入
	isEnterless: boolean;
}