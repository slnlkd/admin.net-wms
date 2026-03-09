import { useBaseApi } from '/@/api/base';

// 仪表盘接口服务
export const useWmsDashboardApi = () => {
	const baseApi = useBaseApi("wmsDashboard");
	return {
		// 获取出入库任务趋势数据
		getTaskTrend: (cancel: boolean = false) => {
			return baseApi.request({
				url: baseApi.baseUrl + "getTaskTrend",
				method: 'get',
			}, cancel);
		},
	}
}

// 每日任务数量统计
export interface DailyTaskCount {
	// 日期
	date: string;
	// 入库任务数量
	importCount: number;
	// 出库任务数量
	exportCount: number;
}

// 出入库任务趋势数据输出
export interface WmsTaskTrendOutput {
	// 近7天趋势数据
	last7Days: DailyTaskCount[];
	// 近30天趋势数据
	last30Days: DailyTaskCount[];
	// 当前入库任务总数（未作废）
	currentImportTotal: number;
	// 当前出库任务总数（未作废）
	currentExportTotal: number;
}
