import {useBaseApi} from '/@/api/base';

// 仓储权限控制接口服务
export const useWmsPrmissionScopeApi = () => {
	const baseApi = useBaseApi("wmsPrmissionScope");
	return {
		// 分页查询仓储权限控制
		page: baseApi.page,
		// 查看仓储权限控制详细
		detail: baseApi.detail,
		// 新增仓储权限控制
		add: baseApi.add,
		// 更新仓储权限控制
		update: baseApi.update,
		// 删除仓储权限控制
		delete: baseApi.delete,
		// 批量删除仓储权限控制
		batchDelete: baseApi.batchDelete,
		// 获取下拉列表数据
		getDropdownData: (fromPage: Boolean = false, cancel: boolean = false) => baseApi.dropdownData({ fromPage }, cancel),
		// 获取下拉列表数据
		getdropdownTableData: (tableName: String = "", cancel: boolean = false) => baseApi.dropdownTableData({ "tableName":tableName }, cancel),
	}
}

// 仓储权限控制实体
export interface WmsPrmissionScope {
	// 主键Id
	id: number;
	// 用户
	userId?: number;
	// 功能名称
	tableName?: string;
	// 字段名
	fieldName?: string;
	// 字段值
	fieldValue?: string;
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