import {useBaseApi} from '/@/api/base';

// 移库任务表接口服务
export const useWmsMoveTaskApi = () => {
	const baseApi = useBaseApi("wmsMoveTask");
	return {
		// 分页查询移库任务表
		page: baseApi.page,
		// 查看移库任务表详细
		detail: baseApi.detail,
		// 新增移库任务表
		add: baseApi.add,
		// 更新移库任务表
		update: baseApi.update,
		// 删除移库任务表
		delete: baseApi.delete,
		// 批量删除移库任务表
		batchDelete: baseApi.batchDelete,
	}
}

// 移库任务表实体
export interface WmsMoveTask {
	// 主键Id
	id: number;
	// 任务号
	taskNo: string;
	// 发送方
	sender: string;
	// 接收方
	receiver: string;
	// 是否成功
	isSuccess: number;
	// 信息
	information: string;
	// 发送时间
	sendDate: string;
	// 返回时间
	backDate: string;
	// 消息时间
	messageDate: string;
	// 托盘条码
	stockCodeId: string;
	// 描述信息
	msg: string;
	// 是否发送
	isSend: number;
	// 是否取消
	isCancel: number;
	// 是否完成
	isFinish: number;
	// 取消时间
	cancelDate: string;
	// 完成时间
	finishDate: string;
	// 是否展示
	isShow: number;
	// 组任务id
	goupTaskId: string;
	// 任务状态
	status: number;
	// 软删除
	isDelete: number;
	// 创建时间
	createTime: string;
	// 创建者姓名
	createUserName: string;
	// 更新时间
	updateTime: string;
	// 修改者姓名
	updateUserName: string;
	// 创建者Id
	createUserId: number;
	// 修改者Id
	updateUserId: number;
}