import { service } from '/@/utils/request';
import {AxiosRequestConfig} from "axios";

const baseUrl = `/api/wmsStockCheckTask/`;

const request = <T>(config: AxiosRequestConfig<T>) => {
    return service(config);
}

export const useWmsStockCheckTaskApi = () => {
	return {
		page: function (data: any) {
			return request({
				url: baseUrl + "page",
				method: 'post',
				data,
			});
		},
		detail: function (id: any) {
			return request({
				url: baseUrl + "detail",
				method: 'get',
				data: { id },
			});
		},
		add: function (data: any) {
			return request({
				url: baseUrl + 'add',
				method: 'post',
				data
			});
		},
		update: function (data: any) {
			return request({
				url: baseUrl + 'update',
				method: 'post',
				data
			});
		},
		delete: function (data: any) {
			return request({
				url: baseUrl + 'delete',
				method: 'post',
				data
			});
		},
		batchDelete: function (data: any) {
			return request({
				url: baseUrl + 'batchDelete',
				method: 'post',
				data
			});
		},
		exportData: function (data: any) {
			return request({
				responseType: 'arraybuffer',
				url: baseUrl + 'export',
				method: 'post',
				data
			});
		},
		importData: function (file: any) {
			const formData = new FormData();
			formData.append('file', file);
			return request({
				headers: { 'Content-Type': 'multipart/form-data;charset=UTF-8' },
				responseType: 'arraybuffer',
				url: baseUrl + 'import',
				method: 'post',
				data: formData,
			});
		},
		downloadTemplate: function () {
			return request({
				responseType: 'arraybuffer',
				url: baseUrl + 'import',
				method: 'get',
			});
		},
		cancelTask: function (data: any) {
			return request({
				url: baseUrl + "cancelTask",
				method: 'post',
				data
			});
		}
	}
}

export interface WmsStockCheckTask {
	id: number;
	checkTaskNo: string;
	checkBillCode: string;
	checkNotifyId: string;
	sender: string;
	receiver: string;
	isSuccess: number;
	sendDate: string;
	backDate: string;
	messageDate: string;
	stockCode: string;
	msg: string;
	finishDate: string;
	warehouseId: string;
	checkTaskFlag: number;
	startLocation: string;
	endLocation: string;
	taskType: number;
	createUserName: string;
	updateUserName: string;
}
