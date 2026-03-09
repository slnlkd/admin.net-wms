import { defineStore } from 'pinia';
import axios from 'axios';
import { getDate } from '/@/utils/getDate';
import { forEach } from 'lodash-es';

//选择领用单位表格
export const useTableSelectCompany = defineStore('useTableSelectCompany', {
	state() {
		return {
			//页码页数参数
			page: 1, //当前页数
			pageSize: 15, //每一页多少数据
			field: 'createTime', //排序字段
			order: 'descending', //排序方法
			descStr: 'descending', //降序排序

			//查询参数
			whereStrParam: null, //关键字

			//下拉框需要显示的参数
			// exportBillTypeList: [
			//     //单据类型
			//     { id: -1, billType: -1, billTypeCode: '', billTypeName: '全选' },
			// ],
			// wareHouseList: [
			//     //所属仓库集合
			//     { id: -1, warehouseName: '', warehouseType: '', warehouseCode: '' }
			// ],

			//出库单据表格的数据 和 页数页码等数据
			hasNextPage: true, //是否有上一页
			hasPrevPage: false, //是否有下一页
			total: 6, //总条数
			totalPages: 1, //总页数

			//出库单据表需要显示的行数
			totalRows: 15,

			//表格的第一行标题
			tableTop: [
				{ id: '01', name: 'id' },
				{ id: '02', name: '客户单位编码' },
				{ id: '03', name: '客户单位名称' },
				{ id: '04', name: '客户单位地址' },
				{ id: '05', name: '操作' },
			],

			//表格的真实数据
			notifyList: [
				{
					id: null,
					customerCode: null,
					customerName: null,
					customerAddress: null,
				},
			],
		};
	},
	actions: {
		//查询领用单位
		async ShowCompanyTableFun() {
			try {
				let newParam = {
					page: this.page,
					pageSize: this.pageSize,
					field: this.field,
					order: this.order,
					descStr: this.descStr,

					whereStr: this.whereStrParam, //查询条件参数
				};
				let notifyResult = await axios.post(window.__env__.VITE_API_URL + '/api/wmsExportNotify/showCompanyTable', newParam);

				this.hasNextPage = notifyResult.data.result.hasNextPage; //是否有上一页
				this.hasPrevPage = notifyResult.data.result.hasPrevPage; //是否有下一页
				this.total = notifyResult.data.result.total; //总条数
				this.totalPages = notifyResult.data.result.totalPages; //总页数
				this.notifyList = notifyResult.data.result.items;
			} catch (error) {
				console.log('查询失败');
			}
		},
	},
	getters: {
		//在页面显示的行数的虚拟数据
		displayNotifyListComputed(): any {
			let resultList = [];

			this.notifyList.forEach((v, i) => {
				resultList.push(v);
			});

			while (resultList.length < this.notifyList.length) {
				resultList.push({
					id: null,
					customerCode: null,
					customerName: null,
					customerAddress: null,
				});
			}

			return resultList;
		},
	},
});
