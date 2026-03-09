import { defineStore } from 'pinia';
import axios from 'axios';
import { getDate } from '/@/utils/getDate';
import { getNowTime } from '/@/utils/getNowTime';
import { forEach } from 'lodash-es';
import { wmsExportNotifyAddDisplayNotifyListInfter, wmsExportNotifyDetailDtoInter } from '../../../types/wmsExportNotify/wmsExportNotifyAdd';
import { List } from 'echarts';

//制作添加出库单据表格
export const useTableExportNotifyAdd = defineStore('useTableExportNotifyAdd', {
	state() {
		return {
			//页码页数参数
			page: 1, //当前页数
			pageSize: 15, //每一页多少数据
			field: 'createTime', //排序字段
			order: 'descending', //排序方法
			descStr: 'descending', //降序排序

			//查询参数
			wareHouseListId: null, // 出库仓库id
			exportBillTypeListId: null, // 单据类型id
			exportChildBillTypeListId: null, // 出库子类型（单据子类型）id
			wayOutListCode: null, // 出库口Code
			customerId: null, // 领用单位id
			baseDepartmentId: null, //部门id
			baseSupplierId: null, //供应商id
			baseAreaId: null, //区域id
			pXStatusId: null, //拼箱id

			//下拉框需要显示的参数
			wareHouseList: [
				//仓库
				{ id: -1, warehouseName: '全选' },
			],
			exportBillTypeList: [
				//单据类型
				{ id: -1, billTypeName: '全选' },
			],
			exportChildBillTypeList: [
				//单据子类型
				{ id: -1, billTypeName: '全选' },
			],
			wayOutList: [
				//出库口
				{ wayOutCode: '', wayOutName: '全选', baseWareHouseId: -1 },
			],
			baseDepartmentList: [
				//部门
				{ id: -1, departmentName: '全选' },
			],
			baseSupplierList: [
				//供应商
				{ id: -1, customerName: '全选' },
			],
			baseAreaList: [
				//区域
				{ id: -1, areaName: '全选' },
			],
			pXStatusList: [
				//拼箱状态
				{ id: null, statusName: '未选' },
				{ id: 0, statusName: '不拼箱' },
				{ id: 1, statusName: '拼箱' },
			],

			//出库单据表格的数据 和 页数页码等数据
			hasNextPage: true, //是否有上一页
			hasPrevPage: false, //是否有下一页
			total: 6, //总条数
			totalPages: 1, //总页数

			//出库单据表需要显示的行数
			totalRows: 15,

			//表格第一行的标题
			tableTop: [],

			//出库单据表格的数据
			notifyList: [{}],

			//虚拟表格数据（当计算属性不用的时候，可以用它）
			displayNotifyList: [{}] as wmsExportNotifyAddDisplayNotifyListInfter[],
			//=====================================
			exportNotifyCode: '', //出库单据code
			lotNo: '', //批次
		};
	},
	actions: {
		//通过后端接口，获取仓库数据
		async GetWmsBaseWareHouseForSelectFun() {
			try{
			let result = await axios.get(window.__env__.VITE_API_URL + '/api/wmsExportNotify/getWmsBaseWareHouseForSelect');
			this.wareHouseList = result.data.result;
			this.wareHouseList.unshift({ id: -1, warehouseName: '全选' });
			}
			catch(error){
				console.log("失败")
			}
		},
		//通过后端接口，获取部门数据
		async GetWmsBaseDepartmentForSelectFun() {
			try{
			let result = await axios.get(window.__env__.VITE_API_URL + '/api/wmsExportNotify/getWmsBaseDepartmentForSelect');
			this.baseDepartmentList = result.data.result;
			this.baseDepartmentList.unshift({ id: -1, departmentName: '全选' });
			}
			catch(error){
				console.log("失败")
			}
		},
		//通过后端接口，获取供应商数据
		async GetWmsBaseSupplierForSelectFun() {
			try{
			let result = await axios.get(window.__env__.VITE_API_URL + '/api/wmsExportNotify/getWmsBaseSupplierForSelect');
			this.baseSupplierList = result.data.result;
			this.baseSupplierList.unshift({ id: -1, customerName: '全选' });
			}
			catch(error){
				console.log("失败")
			}
		},
		//通过后端接口，获取区域数据
		async GetWmsBaseAreaForSelectFun() {
			try{
			let result = await axios.get(window.__env__.VITE_API_URL + '/api/wmsExportNotify/getWmsBaseAreaForSelect');
			this.baseAreaList = result.data.result;
			this.baseAreaList.unshift({ id: -1, areaName: '全选' });
			}
			catch(error){
				console.log("失败")
			}
		},
		//通过后端接口，获取单据类型
		async GetWmsBaseBillTypeForSelectFun() {
			try{
			let result = await axios.get(window.__env__.VITE_API_URL + '/api/wmsExportNotify/getWmsBaseBillTypeForSelect');
			this.exportBillTypeList = result.data.result;
			this.exportBillTypeList.unshift({ id: -1, billTypeName: '全选' });
			}
			catch(error){
				console.log("失败")
			}
		},
		//通过后端接口，获取单据子类型
		async GetWmsBaseChildBillTypeForSelectFun() {
			try{
			let newParam = { id: this.exportBillTypeListId };
			let result = await axios.post(window.__env__.VITE_API_URL + '/api/wmsExportNotify/getWmsBaseChildBillTypeForSelect', newParam);
			this.exportChildBillTypeList = result.data.result;
			this.exportChildBillTypeList.unshift({ id: -1, billTypeName: '全选' });
			}
			catch(error){
				console.log("失败")
			}
		},
		//通过后端接口，获取出库口数据
		async GetWmsExportWayOutByWareHouseIdFun() {
			try{
			let newParam = { wareHouseId: this.wareHouseListId };

			let result = await axios.post(window.__env__.VITE_API_URL + '/api/wmsExportNotify/getWmsExportWayOutByWareHouseId', newParam);
			this.wayOutList = result.data.result;
			this.wayOutList.unshift({ wayOutCode: '', wayOutName: '全选', baseWareHouseId: -1 });
			}
			catch(error){
				console.log("失败")
			}
		},

		//加载出库单据明细
		async showNotifyAddTableFun(val: any) {
			try {
				this.notifyList = val;
				console.log("val",val);
				this.MakeDisplayNotifyList();
			} catch (error) {
				console.log('查询错误val');
			}
		},
		//制作出库单据编码
		MakeExportNotifyCode() {
			const timestamp: number = Date.now();
			this.exportNotifyCode = 'CKS' + timestamp;
		},
		//制作批次
		MakeExportNotifyLotNo() {
			const timestamp: number = Date.now();
			this.lotNo = timestamp + '';
		},
		//制作表格虚拟数据
		MakeDisplayNotifyList() {
			let resultList = [{}];

			
			this.notifyList.forEach((v, i) => {
				resultList.push(v);
			});

			if (resultList.length > 0) {
				if ('wmsBaseMaterialId' in resultList[0] == false) {
					resultList.shift();
				}
			}

			//添加共用属性
			resultList = resultList.map((item) => ({
				...item,
				wareHouseId: this.wareHouseListId, //仓库id
				exportQuantity: null, //出库数量
				allocateQuantity: null, //分配数量
				factQuantity: null, //完成数量
			}));

			while (resultList.length < this.totalRows) {
				resultList.push({
					wareHouseId: null, //仓库id
					exportQuantity: null, //出库数量
					allocateQuantity: null, //分配数量
					factQuantity: null, //完成数量
				});
			}

			this.displayNotifyList = resultList;
		},

		//添加出库单据和出库单据明细到数据库中
		async AddExportNotifyAndDetiailFUN() {
			//创建时间
			let nowTime = getNowTime();
			//创建人
			let createUserName = '--';
			//创建人id
			let createUserId = 1;

			//出库单据（传递给后端，插入数据库的数据）
			let wmsExportNotifyDto = {
				id: 0,
				createTime: nowTime,
				updateTime: nowTime,
				createUserId: createUserId,
				createUserName: createUserName,
				updateUserId: null,
				updateUserName: null,
				isDelete: 0,

				exportBillCode: this.exportNotifyCode,
				exportBillType: this.exportBillTypeListId,
				exportLotNo: this.lotNo, //批号
				materialId: 0,
				warehouseId: this.wareHouseListId,
				exportListNo: this.lotNo, //序号
				exportDepartmentId: this.baseDepartmentId,
				exportSupplierId: this.baseSupplierId,
				exportCustomerId: this.customerId,
				exportProductionDate: null,
				exportLostDate: null,
				exportQuantity: 0,
				exportFactQuantity: 0,
				exportCompleteQuantity: 0,
				exportUploadQuantity: 0,
				exportDate: nowTime,
				exportExecuteFlag: 0,
				exportRemark: null,
				outerBillCode: null,
				outerMainId: null,
				source: 'wms',
				pickingArea: this.baseAreaId,
				pxStatus: this.pXStatusId,
				wholeOutWare: this.wayOutListCode,
				sortOutWare: this.wayOutListCode,
				documentSubtype: this.exportChildBillTypeListId,
				wayOutId: this.wayOutListCode,
			};

			//出库单据明细（传递给后端，插入数据库的数据）
			let wmsExportNotifyDetailDtoList = [] as wmsExportNotifyDetailDtoInter[];

			this.displayNotifyList.forEach((item, index, array) => {
				let wmsExportNotifyDetailDto = {
					exportBillCode: null,
					materialId: item.wmsBaseMaterialId,
					materialCode: item.materialCode,
					materialName: item.materialName,
					materialStandard: item.materialStandard,
					materialModel: item.materialModel,
					materialType: item.materialType,
					materialUnit: item.materialUnitId,
					lotNo: item.lotNo,
					productionDate: item.importProductionDate,
					lostDate: item.importLostDate,
					warehouseId: item.wareHouseId,
					exportQuantity: item.exportQuantity,
					allocateQuantity: item.allocateQuantity,
					factQuantity: item.factQuantity,
					completeQuantity: 0,
					exportDetailFlag: 0,
					inspectionStatus: item.inspectionStatusStr,
					lCLRemainderQTY: null,
					outerDetailId: null,
					kilogramQty: null,

					isDelete: 0,
					createTime: nowTime,
					createUserName: '--',
					createUserId: 1,
					updateTime: nowTime,
					updateUserId: null,
					updateUserName: null,
				};
				if (wmsExportNotifyDetailDto.materialId != null) {
					wmsExportNotifyDetailDtoList.push({ ...wmsExportNotifyDetailDto });
				}
			});

			let param = {
				wmsExportNotifyDto: wmsExportNotifyDto,
				wmsExportNotifyDetailDtoList: wmsExportNotifyDetailDtoList,
			};
			
			let result = await axios.post(window.__env__.VITE_API_URL + '/api/wmsExportNotify/addExportNotifyAndDetiail', param);
			if(result.data.result==1){
				alert("添加成功");
			}
			else{
				alert("添加失败");
			}
		},
	},
	// getters: {
	// 	//出库单据实际在页面显示的行数的虚拟数据
	// 	displayNotifyListComputed(): any {
	// 		let resultList = [{}];

	// 		this.notifyList.forEach((v, i) => {
	// 			resultList.push(v);
	// 		});

	// 		//添加共用属性
	// 		resultList = resultList.map((item) => ({
	// 			...item,
	// 			wareHouseId: this.wareHouseListId, //仓库id
	// 			exportQuantity: null, //出库数量
	// 			allocateQuantity: null, //分配数量
	// 			factQuantity: null, //完成数量
	// 		}));
	// 		console.log("数据1",resultList)
	// 		while (resultList.length < this.totalRows) {
	// 			resultList.push({
	// 				wareHouseId: null,//仓库id
	// 				exportQuantity: null, //出库数量
	// 				allocateQuantity: null, //分配数量
	// 				factQuantity: null, //完成数量
	// 			});
	// 		}
	// 		console.log("数据2",resultList)
	// 		return resultList;
	// 	},
	// },
});
