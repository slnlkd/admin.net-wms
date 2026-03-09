import {useBaseApi} from '/@/api/base';

// 入出库日报、月报、年报接口服务
export const useWmsDailyReportApi = () => {
    const baseApi = useBaseApi("wmsStatisticalReport");
    return {
        // 分页查询出入库报表
        page: baseApi.page,
        // 导出出入库报表数据
        exportData: baseApi.exportData,
        // 图表
        chart: (data: any,cancel: boolean = false) => {
			return baseApi.request({
                url: baseApi.baseUrl + 'charts',
                method: 'post',
                data
            }, cancel);
		},
        // 下载出入库报表数据导入模板
        downloadTemplate: baseApi.downloadTemplate,
    }
}