import {useBaseApi} from '/@/api/base';

// 出库流水接口服务
export const useWmsExportOrderApi = () => {
    const baseApi = useBaseApi("wmsStatisticalReport");
    return {
        // 获取出库流水总单数据
        GetSearchExOrder: (params?: object,cancel: boolean = false) => {
            return baseApi.request({
                url: baseApi.baseUrl + "GetSearchExOrder",
                method: 'post',
                data: params,
            }, cancel);
        },

        GetSearchExOrderDetail: (params?: object,cancel: boolean = false) => {
            return baseApi.request({
                url: baseApi.baseUrl + "GetSearchExOrderDetail",
                method: 'post',
                data: params,
            }, cancel);
        },

        GetSearchExOrderBoxInfo: (params?: object,cancel: boolean = false) => {
            return baseApi.request({
                url: baseApi.baseUrl + "GetSearchExOrderBoxInfo",
                method: 'post',
                data: params,
            }, cancel);
        },

        ExExport: (params?: object,cancel: boolean = false) => {
            return baseApi.request({
                url: baseApi.baseUrl + "ExExport",
                method: 'post',
                data: params,
                responseType: 'blob',
            }, cancel);
        },
    }
}