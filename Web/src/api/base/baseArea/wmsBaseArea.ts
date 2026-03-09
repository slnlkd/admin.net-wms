import {useBaseApi} from '/@/api/base';

// 仓库区域表接口服务
export const useWmsBaseAreaApi = () => {
	const baseApi = useBaseApi("wmsBaseArea");
	return {
		// 分页查询仓库区域表
		page: baseApi.page,
		// 查看仓库区域表详细
		detail: baseApi.detail,
		// 新增仓库区域表
		add: baseApi.add,
		// 更新仓库区域表
		update: baseApi.update,
		// 删除仓库区域表
		delete: baseApi.delete,
		// 批量删除仓库区域表
		batchDelete: baseApi.batchDelete,
		// 导出仓库区域表数据
		exportData: baseApi.exportData,
		// 导入仓库区域表数据
		importData: baseApi.importData,
		// 下载仓库区域表数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
		// 获取下拉列表数据
		getDropdownData: (fromPage: Boolean = false, cancel: boolean = false) => baseApi.dropdownData({ fromPage }, cancel),
	}
}

// 仓库区域表实体
export interface WmsBaseArea {
	// 主键Id
	id: number;
	// 区域编码
	areaCode?: string;
	// 区域名称
	areaName?: string;
	// 所属仓库
	areaWarehouseId?: number;
	// 备注
	remark: string;
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