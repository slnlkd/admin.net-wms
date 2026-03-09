import {useBaseApi} from '/@/api/base';

// 移库单据表接口服务
export const useWmsMoveOrderApi = () => {
	const baseApi = useBaseApi("wmsMoveOrder");
	return {
		// 分页查询移库单据表
		page: baseApi.page,
		// 查看移库单据表详细
		detail: baseApi.detail,
		// 新增移库单据表
		add: baseApi.add,
		// 更新移库单据表
		update: baseApi.update,
		// 删除移库单据表
		delete: baseApi.delete,
		// 批量删除移库单据表
		batchDelete: baseApi.batchDelete,
	}
}

// 移库单据表实体
export interface WmsMoveOrder {
	// 主键Id
	id: number;
	// 移库单号
	moveNo?: string;
	// 状态
	moveStauts: string;
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
	// 软删除
	isDelete?: boolean;
}