import {useBaseApi} from '/@/api/base';

// 入库任务表接口服务
export const useWmsImportTaskApi = () => {
	const baseApi = useBaseApi("wmsImportTask");
	return {
		// 分页查询入库任务表
		page: baseApi.page,
		// 查看入库任务表详细
		detail: baseApi.detail,
		// 新增入库任务表
		add: baseApi.add,
		// 更新入库任务表
		update: baseApi.update,
		// 删除入库任务表
		delete: baseApi.delete,
		// 批量删除入库任务表
		batchDelete: baseApi.batchDelete,
		// 导出入库任务表数据
		exportData: baseApi.exportData,
		// 导入入库任务表数据
		importData: baseApi.importData,
		// 下载入库任务表数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
		// 取消入库任务
        cancelTask: baseApi.cancelTask,
	}
}

// 入库任务表实体
export interface WmsImportTask {
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
	// 发送时间
	sendDate: string;
	// 结束时间
	backDate: string;
	// 描述发送报文
	message: string;
	// 托盘码
	stockCode: string;
	// 描述返回信息
	msg: string;
	// 任务状态
	status: number;
	// 取消时间
	cancelDate: string;
	// 完成时间
	finishDate: string;
	// 仓库ID
	wareHouseId: number;
	// 起始位置
	startLocation: string;
	// 目标位置
	endLocation: string;
	// 二次申请后目标位置
	newEnd: string;
	// 创建者姓名
	createUserName: string;
	// 修改者姓名
	updateUserName: string;
}