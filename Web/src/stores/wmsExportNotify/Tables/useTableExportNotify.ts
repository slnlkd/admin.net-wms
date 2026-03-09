import { defineStore } from 'pinia';
import axios from 'axios';
import { getDate } from '/@/utils/getDate';
import { forEach } from 'lodash-es';

//制作出库单据表格
export const useTableExportNotify = defineStore('useTableExportNotify', {
	state() {
		return {
			//页码页数参数
			page: 1, //当前页数
			pageSize: 6, //每一页多少数据
			field: 'createTime', //排序字段
			order: 'descending', //排序方法
			descStr: 'descending', //降序排序

			//明细页码页数参数
			detailPage: 1, //当前页数
			detailPageSize: 6, //每一页多少数据
			detailField: 'createTime', //排序字段
			detailOrder: 'descending', //排序方法
			detailDescStr: 'descending', //降序排序

			//查询参数
			exportBillCodeParam: null, //出库单号
			startTimeParam: getDate(-3), //开始时间
			endTimeParam: getDate(1), //结束时间
			exportExecuteFlagParam: null, //执行状态
			exportBillTypeParam: null, //单据类型
			wareHouseIdParam: null, //所属仓库id

			//下拉框需要显示的参数
			exportBillTypeList: [
				//单据类型
				{ id: -1, billType: -1, billTypeCode: '', billTypeName: '全选' },
			],
			wareHouseList: [
				//所属仓库集合
				{ id: -1, warehouseName: '', warehouseType: '', warehouseCode: '' },
			],

			//出库单据表格的数据 和 页数页码等数据
			hasNextPage: true, //是否有上一页
			hasPrevPage: false, //是否有下一页
			total: 6, //总条数
			totalPages: 1, //总页数

			//出库单据明细表格的数据 和页面页码等数据
			detailHasNextPage: true, //是否有上一页
			detailHasPrevPage: false, //是否有下一页
			detailTotal: 6, //总条数
			detailTotalPages: 1, //总页数

			//出库单据表需要显示的行数
			totalRows: 6,

			//表格第一行的标题
			tableTop: [
				{ id: '01', name: '序号' },
				{ id: '02', name: '出库单号' }, //
				{ id: '03', name: '执行状态' }, //执行标志（0待执行、1正在分配、2正在执行、3已完成、4作废、5已上传）
				{ id: '04', name: '所属仓库' }, //根据仓库id获取
				{ id: '05', name: '单据类型' }, //根据单据类型表获取
				{ id: '06', name: '出库子类型' }, //在单据类型中，获取单据子类型
				{ id: '07', name: '来源' }, //
				{ id: '08', name: '外部单号' }, //
				{ id: '09', name: '客户名称' }, //根据客户id获取
				{ id: '10', name: '创建人' },
				{ id: '11', name: '创建时间' },
				{ id: '12', name: '修改人' },
				{ id: '13', name: '修改时间' },
				{ id: '14', name: '备注' },
				{ id: '15', name: '操作' },
			],

			//出库单据表格的数据
			notifyList: [
				// {
				// 	index: null,
				// 	exportBillCode: null,
				// 	exportExecuteFlagStr: null,
				// 	warehouseStr: null,
				// 	exportBillTypeStr: null,
				// 	documentSubtypeStr: null,
				// 	source: null,
				// 	outerBillCode: null,
				// 	exportCustomerStr: null,
				// 	createUserName: null,
				// 	createTime: null,
				// 	updateUserName: null,
				// 	updateTime: null,
				// 	exportRemark: null,
				// },
			],

			//出库单据明细表格的数据
			detailNotifyList: [],

			notifyTableRow: null,
		};
	},
	actions: {
		//通过后端接口，获取单据类型数据
		async GetExportBillTypeFun() {
			let result = await axios.get(window.__env__.VITE_API_URL + '/api/wmsExportNotify/getWmsBaseBillTypeForSelect');
			this.exportBillTypeList = result.data.result;
			this.exportBillTypeList.unshift({ id: -1, billType: -1, billTypeCode: '', billTypeName: '全选' });
		},
		//通过后端接口，获取仓库表数据
		async GetwareHouseFun() {
			let result = await axios.get(window.__env__.VITE_API_URL + '/api/wmsExportNotify/getWmsBaseWareHouseForSelect');
			this.wareHouseList = result.data.result;
			this.wareHouseList.unshift({ id: -1, warehouseName: '全选', warehouseType: '', warehouseCode: '' });
		},
		//加载出库单据
		async showNotifyTableFun() {
			try {
				let newParam = {
					page: this.page,
					pageSize: this.pageSize,
					field: this.field,
					order: this.order,
					descStr: this.descStr,

					exportBillCode: this.exportBillCodeParam, //出库单号
					startTime: this.startTimeParam, //开始时间
					endTime: this.endTimeParam, //结束时间
					exportExecuteFlag: this.exportExecuteFlagParam, //执行状态
					exportBillType: this.exportBillTypeParam, //单据类型
					wareHouseId: this.wareHouseIdParam, //所属仓库id
				};
				let notifyResult = await axios.post(window.__env__.VITE_API_URL + '/api/wmsExportNotify/showWmsExportNotifyOfPage', newParam);

				this.hasNextPage = notifyResult.data.result.hasNextPage; //是否有上一页
				this.hasPrevPage = notifyResult.data.result.hasPrevPage; //是否有下一页
				this.total = notifyResult.data.result.total; //总条数
				this.totalPages = notifyResult.data.result.totalPages; //总页数
				this.notifyList = notifyResult.data.result.items;
			} catch (error) {
				console.log('查询错误');
			}
		},
		//跟据出库单据id,获取明细
		async ShowNotifyAndDetiailByNotifyIdFun(row: any) {
			try {
				let newParam = {
					page: this.detailPage,
					pageSize: this.detailPageSize,
					field: this.detailField,
					order: this.detailOrder,
					descStr: this.detailDescStr,
					ExportBillCode:row.exportBillCode,
					notifyId: row.id,
				};
				this.notifyTableRow = row;
				let notifyResult = await axios.post(window.__env__.VITE_API_URL + '/api/wmsExportNotify/showNotifyAndDetiailByNotifyId', newParam);

				this.detailHasNextPage = notifyResult.data.result.hasNextPage; //是否有上一页
				this.detailHasPrevPage = notifyResult.data.result.hasPrevPage; //是否有下一页
				this.detailTotal = notifyResult.data.result.total; //总条数
				this.detailTotalPages = notifyResult.data.result.totalPages; //总页数
				this.detailNotifyList = notifyResult.data.result.items;
			} catch (error) {
				console.log('查询错误');
			}
		},
		//自动分配
		async deleteExportTaskAuto(row: any) {
			//let notifyId = row.id
			//console.log(notifyId)
			try {
				let newParam = { id: row.id,WareHouseType:row.warehouseId };
				let result = await axios.post(window.__env__.VITE_API_URL + '/api/wmsExportNotify/AddExportTaskAuto', newParam);
				if (result.data.result == '1') {
					alert('分配成功');
				} else {
					alert('分配失败');
				}

				this.showNotifyTableFun();
			} catch (error) {
				alert('分配失败');
			}
		},
		//删除单据和明细
		async deleteRow(row: any) {
			//let notifyId = row.id
			//console.log(notifyId)
			try {
				let newParam = { id: row.id };
				let result = await axios.post(window.__env__.VITE_API_URL + '/api/wmsExportNotify/deleteNotifyAndDetiail', newParam);
				if (result.data.result == '1') {
					alert('删除成功');
				} else {
					alert('删除失败');
				}

				this.showNotifyTableFun();
			} catch (error) {
				alert('删除失败');
			}
		},
		//明细 删除
		async deleteDetialRow(row: any) {
			try {
				//    /api/wmsExportNotify/deleteNotifyDetiailById

				let newParam = { id: row.notifyDetailId };
				let result = await axios.post(window.__env__.VITE_API_URL + '/api/wmsExportNotify/deleteNotifyDetiailById', newParam);
				if (result.data.result == '1') {
					alert('删除成功');
				} else {
					alert('删除失败');
				}

				this.ShowNotifyAndDetiailByNotifyIdFun(this.notifyTableRow);
				
			} catch (error) {
				alert('删除失败');
			}
		},
	},
	getters: {
		//出库单据实际在页面显示的行数的虚拟数据
		displayNotifyListComputed(): any {
			let resultList = [];

			this.notifyList.forEach((v, i) => {
				resultList.push(v);
			});

			while (resultList.length < this.notifyList.length) {
				resultList.push({
					index: null,
					exportBillCode: null,
					exportExecuteFlagStr: null,
					warehouseStr: null,
					exportBillTypeStr: null,
					documentSubtypeStr: null,
					source: null,
					outerBillCode: null,
					exportCustomerStr: null,
					createUserName: null,
					createTime: null,
					updateUserName: null,
					updateTime: null,
					exportRemark: null,
				});
			}

			return resultList;
		},
	},
});
