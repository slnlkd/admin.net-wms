import { defineStore } from 'pinia';
import axios from 'axios';
import { getDate } from '/@/utils/getDate';
import { forEach } from 'lodash-es';
import { getNowTime } from '/@/utils/getNowTime';
import {moveOutLaneListInter} from '/@/types/wmsMoveOrder/wmsMoveOrderAdd'

//制作出库单据表格
export const useTableAddMoveOrder = defineStore('useTableAddMoveOrder', {
	state() {
		return {
			//页码页数参数
			page: 1, //当前页数
			pageSize: 6, //每一页多少数据
			field: 'createTime', //排序字段
			order: 'descending', //排序方法
			descStr: 'descending', //降序排序

			//查询参数
			warehouseIdParam: null, //仓库表id
			moveOutLaneParam: null, //移出巷道
			moveOutGoodsParam: null, //移出货位
			moveInLaneParam: null, //移入巷道
			moveInGoodsParam: null, //移入货位

			//下拉框需要显示的参数
			warehouseIdList: [
				//仓库表
				{ id: -1, warehouseName: '全选' },
			],
			moveOutLaneList: [
				//移出巷道
				{ id:-1, code: '', moveOutName: '全选' },
			],
			moveOutGoodsList: [
				//移出货位
				{ id:-1, code: '全选' },
			],
			moveInLaneList: [
				//移入巷道
				{ id:-1, code: '', moveInName: '全选' },
			],
			moveInGoodsList: [
				//移入货位
				{ id:-1,code: '全选' },
			],

			//移库单据表格的数据 和 页数页码等数据
			hasNextPage: true, //是否有上一页
			hasPrevPage: false, //是否有下一页
			total: 0, //总条数
			totalPages: 1, //总页数

			//移库单据表需要显示的行数
			totalRows: 0,

			//表格第一行的标题
			tableTop: [],

			//移库单据表格的数据
			tableList: [],

			//点击的行
			selectTableRow: null,
		};
	},
	actions: {
		//移库仓库下拉框
		async GetWmsBaseWareHouseFun() {
			try {
				let result = await axios.get(window.__env__.VITE_API_URL + '/api/wmsMoveOrder/getWmsBaseWareHouse');
				this.warehouseIdList = result.data.result;
				//this.warehouseIdList.unshift({ id: -1, warehouseName: '全选' });
			} catch (error) {
				console.log('错误');
			}
		},
        //巷道下拉框 -- 移出
        async GetWmsBaseLanewayOutFun(otherId:any){
            try {
                let param = {
                    otherId:otherId,
                    wareHouseId:this.warehouseIdParam
                }

				let result = await axios.post(window.__env__.VITE_API_URL + '/api/wmsMoveOrder/getWmsBaseLaneway',param);
				let list = result.data.result;
				this.moveOutLaneList.splice(0,this.moveOutLaneList.length);
                for(let i=0;i<list.length;i++){
                    this.moveOutLaneList.push({
                        id:list[i].id,
                        code:list[i].lanewayCode,
                        moveOutName:list[i].lanewayName
                    })
                }
				//this.moveOutLaneList.unshift({id:-1, code: "", moveOutName: '全选' });
			} catch (error) {
				console.log('错误');
			}
        },

        //巷道下拉框 -- 移入
        async GetWmsBaseLanewayInFun(otherId:any){
            try {
                let param = {
                    otherId:otherId,
                    wareHouseId:this.warehouseIdParam
                }

				let result = await axios.post(window.__env__.VITE_API_URL + '/api/wmsMoveOrder/getWmsBaseLaneway',param);
				let list = result.data.result;
				this.moveInLaneList.splice(0,this.moveInLaneList.length)
                for(let i=0;i<list.length;i++){
                    this.moveInLaneList.push({
                        id:list[i].id,
                        code:list[i].lanewayCode,
                        moveInName:list[i].lanewayName
                    })
                }
				//this.moveOutLaneList.unshift({id:-1, code: "", moveOutName: '全选' });
			} catch (error) {
				console.log('错误');
			}
        },
		//根据巷道号获取可移出货位
		async GetSelectGoodsOutFun(){
			try{
				let param={
					laneId:this.moveOutLaneParam
				}

				let result = await axios.post(window.__env__.VITE_API_URL + '/api/wmsMoveOrder/getSlotIdHaveStock',param);
				let list = result.data.result;
				this.moveOutGoodsList.splice(0,this.moveOutGoodsList.length)
                for(let i=0;i<list.length;i++){
                    this.moveOutGoodsList.push({
                        id:list[i].slotId,
                        code:list[i].slotCode,
                    })
                }

			}
			catch(error){
				console.log("错误")
			}
		},

		//根据巷道号获取可移入货位
		async GetSelectGoodsInFun(){
			try{
				let param={
					laneId:this.moveInLaneParam
				}

				let result = await axios.post(window.__env__.VITE_API_URL + '/api/wmsMoveOrder/getEmptySlotId',param);
				let list = result.data.result;
				this.moveInGoodsList.splice(0,this.moveInGoodsList.length)
                for(let i=0;i<list.length;i++){
                    this.moveInGoodsList.push({
                        id:list[i].slotId,
                        code:list[i].slotCode,
                    })
                }

			}
			catch(error){
				console.log("错误")
			}
		},
		//添加移库明细表方法
		async AddWmsMoveFun(MoveOutSlotCode:any,MoveInSlotCode:any){
			try{
				let param={
					moveOutSlotCode:MoveOutSlotCode
				}

				let result = await axios.post(window.__env__.VITE_API_URL + '/api/wmsMoveOrder/GetStockSlotListBySlotId',param);
				result.data.result.items[0].moveInSlotCode=MoveInSlotCode;
				
				this.hasNextPage = result.data.result.hasNextPage; //是否有上一页
				this.hasPrevPage = result.data.result.hasPrevPage; //是否有下一页
				this.total = result.data.result.total; //总条数
				this.totalPages = result.data.result.totalPages; //总页数
				this.tableList = result.data.result.items;
			}
			catch(error){
				console.log("添加失败")
			}
		},
		//下发移库方法
		async AddWmsMoveNotifyFun(TableList: any): Promise<{ success: boolean; message?: string; data?: any }> {
			try {
				let param = {
					moveLotNo: TableList[0].lotNo,
					materialId: TableList[0].materialId,
					moveOutSlotCode: TableList[0].stockSlotCode,
					moveInSlotCode: TableList[0].moveInSlotCode,
					stockStockCodeId: TableList[0].stockCode
				};				
				let result = await axios.post(window.__env__.VITE_API_URL + '/api/wmsMoveOrder/AddWmsMoveNotify', param);				
				if (result.data.code == 200 && result.data.result == 1) {
					return {
						success: true,
						message: '下发成功',
						data: result.data
					};
				} else {
					const errorMessage = result.data.message || '下发失败';
					alert('下发失败：' + errorMessage);
					return {
						success: false,
						message: errorMessage,
						data: result.data
					};
				}
			} catch (error) {
				console.log("添加失败", error);
				const errorMessage = error instanceof Error ? error.message : '未知错误';
				return {
					success: false,
					message: errorMessage
				};
			}
		},
		//加载移库单据
		async showTableListFun() {
			try {
				/*
                let newParam = {
                    page: this.page,
                    pageSize: this.pageSize,
                    field: this.field,
                    order: this.order,
                    descStr: this.descStr,

                    moveNo: this.moveNoParam,
                    moveStauts: this.moveStautsParam,
                    startTime: this.startTimeParam,
                    endTime: this.endTimeParam,
                };
                let notifyResult = await axios.post(window.__env__.VITE_API_URL + '/api/wmsMoveOrder/showMoveOrder', newParam);

                this.hasNextPage = notifyResult.data.result.hasNextPage; //是否有上一页
                this.hasPrevPage = notifyResult.data.result.hasPrevPage; //是否有下一页
                this.total = notifyResult.data.result.total; //总条数
                this.totalPages = notifyResult.data.result.totalPages; //总页数
                this.tableList = notifyResult.data.result.items;
                */
			} catch (error) {
				console.log('查询错误');
			}
		},
	},
	getters: {
	    //移库单据实际在页面显示的行数的虚拟数据
	    displayTableListComputed(): any {
	        let resultList = [];

	        this.tableList.forEach((v, i) => {
	            resultList.push(v);
	        });

	        while (resultList.length < this.totalRows) {
	            resultList.push({
	                id: null,
	                moveNo: null,
	                moveStauts: null,
	                moveStautsStr: null,
	                createUserName: null,
	                createTime: null,
	            });
	        }

	        return resultList;
	    },
	},
});
