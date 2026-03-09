import { service } from '/@/utils/request';
import {AxiosRequestConfig} from "axios";

const baseUrl = `/api/wmsStockCheckNotify/`;

const request = <T>(config: AxiosRequestConfig<T>) => {
    return service(config);
}

export const useWmsStockCheckOrderApi = () => {
	return {
		page: function (data: any) {
			return request({
				url: baseUrl + "page",
				method: 'post',
				data,
			});
		},
		detail: function (params: object) {
			return request({
				url: baseUrl + "detail",
				method: 'post',
				data:params,
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


		getDetailList: function (data: any) {
			return request({
				url: baseUrl + "getDetailList",
				method: 'post',
				data,
			});
		},

		stockCheckIssueOutBound: function (data: any) {
			return request({
				url: baseUrl + "stockCheckIssueOutBound",
				method: 'post',
				data
			});
		},
		createTask: function (data: any) {
			return request({
				url: baseUrl + "createTask",
				method: 'post',
				data
			});
		},
		createAdjustMent: function (data: any) {
			return request({
				url: baseUrl + "createAdjustMent",
				method: 'post',
				data
			});
		},
		cancelStockEdit: function (data: any) {
			return request({
				url: baseUrl + "cancelStockEdit",
				method: 'post',
				data
			});
		},

		getBoxDetail: function (data: any) {
			return request({
				url: baseUrl + "getBoxDetail",
				method: 'post',
				data,
			});
		},

		getStockSlotList: function (data: any) {
			return request({
				url: baseUrl + "getStockSlotList",
				method: 'post',
				data,
			});
		}
	}
}

// export interface WmsStockCheckOrder {
// 	id: number;
// 	checkBillCode: string;
// 	checkDate: string;
// 	warehouseId: string;
// 	executeFlag: number;
// 	checkRemark: string;
// 	addDate: string;
// 	warehouseName: string;
// 	warehouseType: string;
// 	createUserName: string;
// 	createTime: string;
// 	updateUserName: string;
// 	updateTime: string;
// 	list: WmsStockCheckOrderDetail[];
// }

// export interface WmsStockCheckOrderDetail {
// 	id: number;
// 	checkBillCode: string;
// 	materialId: number;
// 	materialCode: string;
// 	materialName: string;
// 	materialStandard: string;
// 	stockSlot: string;
// 	stockSlotCode: string;
// 	stockCode: string;
// 	stockLotNo: string;
// 	stockQuantity: number;
// 	realQuantity: number;
// 	unitName: string;
// 	inspectionStatus: number;
// 	executeFlag: number;
// 	addDate: string;
// 	checkResult: number;
// 	checkRemark: string;
// }

// export interface WmsStockCheckBoxDetail {
// 	id: number;
// 	boxCode: string;
// 	checkResult: string;
// 	materialCode: string;
// 	materialName: string;
// 	lotNo: string;
// 	qty: number;
// 	bulkTank: string;
// 	extractStatus: string;
// 	inspectionStatus: string;
// 	productionDate: string;
// 	validateDay: string;
// }

// export interface AddWmsStockCheckOrderInput {
// 	warehouseId: string;
// 	checkRemark: string;
// 	list: AddWmsStockCheckOrderDetailInput[];
// }

// export interface AddWmsStockCheckOrderDetailInput {
// 	stockSlot: string;
// 	stockCode: string;
// 	stockQuantity: number;
// 	lotNo: string;
// 	materialId: number;
// 	materialCode: string;
// 	materialName: string;
// 	materialStandard: string;
// 	inspectionStatus: number;
// 	checkStockId: number;
// }

// export interface UpdateWmsStockCheckOrderInput {
// 	id: number;
// 	checkBillCode: string;
// 	warehouseId: string;
// 	executeFlag: number;
// 	checkRemark: string;
// }

// export interface StockCheckIssueOutBoundInput {
// 	billCode: string;
// 	warehouseId: string;
// }

// export interface CreateStockCheckTaskInput {
// 	id: number;
// 	billCode: string;
// }

// export interface CancelStockEditInput {
// 	billCode: string;
// }
