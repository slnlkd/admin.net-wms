import {useBaseApi} from '/@/api/base';

// 库存预警接口服务
export const usewmsStockWarningApi = () => {
    const baseApi = useBaseApi("wmsStockSlotReport");
    return {
        // 效期超期预警
		ValidityExpireWarning: (params?: object,cancel: boolean = false) => {
			return baseApi.request({
				url: baseApi.baseUrl + "ValidityExpireWarning",
				method: 'post',
				data: params,
			}, cancel);
		},
        //低库存预警
		StockLowWarning: (params?: object,cancel: boolean = false) => {
			return baseApi.request({
				url: baseApi.baseUrl + "StockLowWarning",
				method: 'post',
				data: params,
			}, cancel);
		},
        //高库存预警
		StockHighWarning: (params?: object,cancel: boolean = false) => {
			return baseApi.request({
				url: baseApi.baseUrl + "StockHighWarning",
				method: 'post',
				data: params,
			}, cancel);
		},
        //复验期预警
		ReInspectionPeriodWarning: (params?: object,cancel: boolean = false) => {
			return baseApi.request({
				url: baseApi.baseUrl + "ReInspectionPeriodWarning",
				method: 'post',
				data: params,
			}, cancel);
		},
		GetWarningConfig: (code: string,cancel: boolean = false) => {
			return baseApi.request({
				url: baseApi.baseUrl + "GetWarningConfig",
				method: 'get',
				params:{Code:code}
			}, cancel);
		},
		WarningConfig: (params?: object,cancel: boolean = false) => {
			return baseApi.request({
				url: baseApi.baseUrl + "WarningConfig",
				method: 'get',
				data: params
			}, cancel);
		},
		// 导出出入库报表数据
        exportData: baseApi.exportData,
    }
}