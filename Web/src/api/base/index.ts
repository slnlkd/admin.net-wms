import { service, cancelRequest } from '/@/utils/request';
import {AxiosRequestConfig, AxiosResponse} from "axios";

// 接口基类
export const useBaseApi = (module: string) => {
    const baseUrl = `/api/${module}/`;
    const request = <T>(config: AxiosRequestConfig<T>, cancel: boolean = false) => {
        if (cancel) {
            cancelRequest(config.url || "");
            return Promise.resolve({} as AxiosResponse<any, any>);
        }
        return service(config);
    }
    return {
        baseUrl: baseUrl,
        request: request,
        page: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + "page",
                method: 'post',
                data,
            }, cancel);
        },
        GetStockDetailsList: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + "GetStockDetailsList",
                method: 'post',
                data,
            }, cancel);
        },
        detail: function (id: any, cancel: boolean = false) {
            return request({
                url: baseUrl + "detail",
                method: 'get',
                data: { id },
            }, cancel);
        },
        getEnableDetail: function (cancel: boolean = false) {
            return request({
                url: baseUrl + "getEnableDetail",
                method: 'get',
            }, cancel);
        },
        getSlotDetail: function (id: any, cancel: boolean = false) {
           return request({
                url: baseUrl + "getSlotDetail",
                method: 'get',
                data: { id },
            }, cancel);
        },
        dropdownData: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + "dropdownData",
                method: 'post',
                data,
            }, cancel);
        },
        dropdownDataLayer: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + "dropdownDataLayer",
                method: 'post',
                data,
            }, cancel);
        }, 
        dropdownDataRow: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + "dropdownDataRow",
                method: 'post',
                data,
            }, cancel);
        },
        getSlotLegendList: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + "getSlotLegendList",
                method: 'post',
                data,
            }, cancel);
        },
        dropdownTableData: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + "dropdownTableData",
                method: 'post',
                data,
            }, cancel);
        },
        getSysStockCode: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + "getSysStockCode",
                method: 'post',
                data,
            }, cancel);
        },
        saveSlot: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + "saveSlot",
                method: 'post',
                data
            }, cancel);
        },
        complete: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + "complete",
                method: 'post',
                data,
            }, cancel);
        },
        invalid: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + "invalid",
                method: 'post',
                data,
            }, cancel);
        },
        add: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + 'add',
                method: 'post',
                data
            }, cancel);
        },
        update: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + 'update',
                method: 'post',
                data
            }, cancel);
        },
        setStatus: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + 'setStatus',
                method: 'post',
                data
            }, cancel);
        },
        delete: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + 'delete',
                method: 'post',
                data
            }, cancel);
        },
        ExportUndo: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + 'exportUndo',
                method: 'post',
                data
            }, cancel);
        },
        reset: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + 'reset',
                method: 'post',
                data
            }, cancel);
        },
        batchDelete: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + 'batchDelete',
                method: 'post',
                data
            }, cancel);
        },
        IssueOutBound: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + 'IssueOutBound',
                method: 'post',
                data
            }, cancel);
        },
        exportData: function (data: any, cancel: boolean = false) {
            return request({
                responseType: 'arraybuffer',
                url: baseUrl + 'export',
                method: 'post',
                data
            }, cancel);
        },
        downloadTemplate: function (cancel: boolean = false) {
            return request({
                responseType: 'arraybuffer',
                url: baseUrl + 'import',
                method: 'get',
            }, cancel);
        },
        importData: function (file: any, cancel: boolean = false) {
            const formData = new FormData();
	        formData.append('file', file);
            return request({
                headers: { 'Content-Type': 'multipart/form-data;charset=UTF-8' },
                responseType: 'arraybuffer',
                url: baseUrl + 'import',
                method: 'post',
                data: formData,
            }, cancel);
        },
        uploadFile: function (params: any, action: string, cancel: boolean = false) {
            const formData = new FormData();
            formData.append('file', params.file);
            // 自定义参数
            if (params.data) {
                Object.keys(params.data).forEach((key) => {
                    const value = params.data![key];
                    if (Array.isArray(value)) {
                        value.forEach((item) => formData.append(`${key}[]`, item));
                        return;
                    }
                    formData.append(key, params.data![key]);
                });
            }
            return request({
                url: baseUrl + action,
                method: 'POST',
                data: formData,
                headers: {
                    'Content-Type': 'multipart/form-data;charset=UTF-8',
                    ignoreCancelToken: true,
                },
            }, cancel);
        },
        cancelTask: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + 'cancelTask',
                method: 'post',
                data
            }, cancel);
        },
        feedBack: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + 'taskFeedback',
                method: 'post',
                data
            }, cancel);
        },
         createOrder: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + 'createOrder',
                method: 'post',
                data
            }, cancel);
        },
         print: function (data: any, cancel: boolean = false) {
            return request({
                url: baseUrl + 'print',
                method: 'post',
                data
            }, cancel);
        },
        
    }
}