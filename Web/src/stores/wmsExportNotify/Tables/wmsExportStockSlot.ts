import { defineStore } from 'pinia';
import axios from 'axios';
import { getDate } from '/@/utils/getDate';
import { forEach } from 'lodash-es';

//制作选择物料信息表格
export const useTablewmsExportStockSlot = defineStore('wmsExportStockSlot', {
	state() {
		return {
			//被勾选框选中的数据
			selectedData: [{wmsBaseMaterialId:null}],

			//页码页数参数
			page: 1, //当前页数
			pageSize: 18, //每一页多少数据
			field: 'createTime', //排序字段
			order: 'descending', //排序方法
			descStr: 'descending', //降序排序

			//查询参数
			whereStrParam: null, //关键字

			//下拉框需要显示的参数
			// wareHouseList: [
			//     //仓库
			//     { id: -1, warehouseName: '全选' },
			// ],

			//出库单据表格的数据 和 页数页码等数据
			hasNextPage: true, //是否有上一页
			hasPrevPage: false, //是否有下一页
			total: 6, //总条数
			totalPages: 1, //总页数

			//父组件传进来的参数
			parentProps: {
				notifyDetailId: null,
			},

			//出库单据表需要显示的行数
			totalRows: 15,

			//表格第一行的标题
			tableTop: [
				
			],

			//出库单据表格的数据
			notifyList: [{}],
		};
	},
	actions: {
		// //通过后端接口，获取仓库数据
		// async GetWmsBaseWareHouseForSelectFun() {
		//     let result = await axios.get(window.__env__.VITE_API_URL + '/api/wmsExportNotify/getWmsBaseWareHouseForSelect');
		//     this.wareHouseList = result.data.result;
		//     this.wareHouseList.unshift({ id: -1, warehouseName: '全选' });
		// },
		// //通过后端接口，获取单据类型
		// async GetWmsBaseBillTypeForSelectFun() {
		//     let result = await axios.get(window.__env__.VITE_API_URL + '/api/wmsExportNotify/getWmsBaseBillTypeForSelect');
		//     this.exportBillTypeList = result.data.result;
		//     this.exportBillTypeList.unshift({ id: -1, billTypeName: '全选' });
		// },
		// //通过后端接口，获取单据子类型
		// async GetWmsBaseChildBillTypeForSelectFun() {
		//     let newParam = {id:this.exportBillTypeListId}
		//     let result = await axios.post(window.__env__.VITE_API_URL + '/api/wmsExportNotify/getWmsBaseChildBillTypeForSelect',newParam);
		//     this.exportChildBillTypeList = result.data.result;
		//     this.exportChildBillTypeList.unshift({ id: -1, billTypeName: '全选' });
		// },
		// //通过后端接口，获取出库口数据
		// async GetWmsExportWayOutByWareHouseIdFun(){
		//     let newParam = {wareHouseId:this.wareHouseListId}

		//     let result = await axios.post(window.__env__.VITE_API_URL + '/api/wmsExportNotify/getWmsExportWayOutByWareHouseId',newParam);
		//     this.wayOutList = result.data.result;
		//     this.wayOutList.unshift({ wayOutCode: '', wayOutName: '全选',baseWareHouseId:-1 });

		// }

		//加载物料信息表格
		async SelectMaterialFun() {
			try {
				let newParam = {
					page: this.page,
					pageSize: this.pageSize,
					field: this.field,
					order: this.order,
					descStr: this.descStr,

					whereStr: this.whereStrParam,

					detailId: this.parentProps.notifyDetailId,
				};

				let notifyResult = await axios.post(window.__env__.VITE_API_URL + '/api/wmsExportNotify/GetStockSlotHandList', newParam);

				this.hasNextPage = notifyResult.data.result.hasNextPage; //是否有上一页
				this.hasPrevPage = notifyResult.data.result.hasPrevPage; //是否有下一页
				this.total = notifyResult.data.result.total; //总条数
				this.totalPages = notifyResult.data.result.totalPages; //总页数
				this.notifyList = notifyResult.data.result.items;
			} catch (error) {
				console.log('查询失败');
			}
		},

		//根据多选行数，确定被勾选框选中的数据
		SelectRowReturnFun(val: any): any {
			this.selectedData = val;
			this.selectedData = this.selectedData.filter(item=> item.wmsBaseMaterialId!=null)
			console.log("selectData:",this.selectedData)
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
					WmsBaseMaterialId: null,
					WmsStockId: null,
					MaterialCode: null,
					MaterialName: null,
					MaterialStandard: null,
					LotNo: null,
					MaterialValidityDayStr: null,
					BoxQuantity: null,
					StockQuantity: null,
					InspectionStatusStr: null,



					materialModel:null,
					materialType:null,
					materialTypeStr:null,
					materialUnitId:null,
					materialUnitStr:null,
					ProductionDate:null,
					ValidateDay:null
				});
			}

			return resultList;
		},
	},
});
