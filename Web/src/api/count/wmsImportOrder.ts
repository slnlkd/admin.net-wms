import {useBaseApi} from '/@/api/base';

// 入库流水接口服务
export const useWmsImportOrderApi = () => {
    const baseApi = useBaseApi("wmsStatisticalReport");
    return {
        // 获取入库流水总单数据
		GetSearchImOrder: (params?: object,cancel: boolean = false) => {
			return baseApi.request({
				url: baseApi.baseUrl + "GetSearchImOrder",
				method: 'post',
				data: params,
			}, cancel);
		},

		GetSearchImOrderDetail: (params?: object,cancel: boolean = false) => {
			return baseApi.request({
				url: baseApi.baseUrl + "GetSearchImOrderDetail",
				method: 'post',
				data: params,
			}, cancel);
		},

		GetSearchImOrderBoxInfo: (params?: object,cancel: boolean = false) => {
			return baseApi.request({
				url: baseApi.baseUrl + "GetSearchImOrderBoxInfo",
				method: 'post',
				data: params,
			}, cancel);
		},

        ImExport: (params?: object,cancel: boolean = false) => {
            return baseApi.request({
                url: baseApi.baseUrl + "ImExport",
                method: 'post',
                data: params,
                responseType: 'blob',
            }, cancel);
        },
    }
}