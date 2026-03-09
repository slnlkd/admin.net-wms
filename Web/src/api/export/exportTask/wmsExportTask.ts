import {useBaseApi} from '/@/api/base';

// 出库任务接口服务
export const useWmsExportTaskApi = () => {
	const baseApi = useBaseApi("wmsExportTask");
	return {
		// 分页查询出库任务
		page: baseApi.page,
		// 查看出库任务详细
		detail: baseApi.detail,
		// 新增出库任务
		add: baseApi.add,
		// 更新出库任务
		update: baseApi.update,
		// 删除出库任务
		delete: baseApi.delete,
		// 批量删除出库任务
		batchDelete: baseApi.batchDelete,
		// 导出出库任务数据
		exportData: baseApi.exportData,
		// 导入出库任务数据
		importData: baseApi.importData,
		// 下载出库任务数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 出库任务实体
export interface WmsExportTask {
	// 主键Id
	id: number;
	// 出库任务号
	exportTaskNo: string;
	// 发送方
	sender: string;
	// 接收方
	receiver: string;
	// 是否成功（0否、1是）
	isSuccess: number;
	// 信息
	information: string;
	// 发送时间
	sendDate: string;
	// 结束时间
	backDate: string;
	// 托盘条码
	stockCode: string;
	// 储位编码
	stockSoltCode: string;
	// 描述信息
	msg: string;
	// 执行状态（0：已下发；1：执行中；2：执行完毕）
	exportTaskFlag: number;
	// 出库流水号
	exportOrderNo: string;
	// 起始位置
	startLocation: string;
	// 目标位置
	endLocation: string;
	// 仓库Id
	warehouseId: number;
	// 1：出库；2：出库移库
	taskType: number;
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