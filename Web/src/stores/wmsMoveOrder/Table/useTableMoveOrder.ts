import { defineStore } from 'pinia';
import axios from 'axios';
import { getDate } from '/@/utils/getDate';
import { forEach } from 'lodash-es';

//制作出库单据表格
export const useTableMoveOrder = defineStore('useTableMoveOrder', {
	state() {
		return {
			//页码页数参数
			page: 1, //当前页数
			pageSize: 6, //每一页多少数据
			field: 'createTime', //排序字段
			order: 'descending', //排序方法
			descStr: 'descending', //降序排序

			//查询参数
			moveNoParam: null, //移库单号
			moveStautsParam: null, //执行状态
			startTimeParam: getDate(-3), //开始时间
			endTimeParam: getDate(1), //结束时间

			//下拉框需要显示的参数
			moveStautsList: [
				//执行状态
				{ code: '', name: '全选' },
				{ code: '0', name: '未开始' },
				{ code: '1', name: '执行中' },
				{ code: '2', name: '执行完成' },
				{ code: '3', name: '手动完成' },
				{ code: '4', name: '手动取消' },
			],

			//移库单据表格的数据 和 页数页码等数据
			hasNextPage: true, //是否有上一页
			hasPrevPage: false, //是否有下一页
			total: 6, //总条数
			totalPages: 1, //总页数

			//移库单据表需要显示的行数
			totalRows: 6,

			//表格第一行的标题
			tableTop: [],

			//移库单据表格的数据
			tableList: [],

			//点击的行
			selectTableRow: null,
		};
	},
	actions: {
		//加载移库单据
		async showTableListFun() {
			try {
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

			while (resultList.length < this.tableList.length) {
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
