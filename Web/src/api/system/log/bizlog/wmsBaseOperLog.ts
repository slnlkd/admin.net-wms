import {useBaseApi} from '/@/api/base';

// 业务操作日志接口服务
export const useWmsBaseOperLogApi = () => {
	const baseApi = useBaseApi("wmsBaseOperLog");
	return {
		// 分页查询业务操作日志
		page: baseApi.page,
		// 查看业务操作日志详细
		detail: baseApi.detail,
		// 新增业务操作日志
		add: baseApi.add,
		// 更新业务操作日志
		update: baseApi.update,
		// 删除业务操作日志
		delete: baseApi.delete,
		// 批量删除业务操作日志
		batchDelete: baseApi.batchDelete,
	}
}

// 业务操作日志实体
export interface WmsBaseOperLog {
	// 主键Id
	id: number;
	// 关联的技术日志TraceId，用于开发人员追踪
	traceId: string;
	// 模块名称
	module: string;
	// 操作类型：新增、修改、删除、审核等
	operationType: string;
	// 操作人员
	operator: string;
	// 操作时间
	operateTime: string;
	// 操作IP地址
	operateIp: string;
	// 业务单据号/ID
	businessNo: string;
	// 操作内容（客户可读的详细描述）
	operationContent: string;
	// 修改前数据摘要（客户可读格式）
	beforeDataSummary: string;
	// 修改后数据摘要（客户可读格式）
	afterDataSummary: string;
	// 操作结果：成功/失败
	result: string;
	// 操作耗时（毫秒）
	elapsedMs: number;
	// 额外信息
	extraInfo: string;
	// 租户ID
	tenantId: number;
	// 隐藏的入参参数（JSON格式，开发人员使用，不对客户显示）
	operParam: string;
	// 
	createTime?: string;
	// 
	updateTime: string;
}