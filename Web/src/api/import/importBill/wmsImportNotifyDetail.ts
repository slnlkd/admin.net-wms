import {useBaseApi} from '/@/api/base';

// 入库单据-表体接口服务
export const useWmsImportNotifyDetailApi = () => {
	const baseApi = useBaseApi("wmsImportNotifyDetail");
	return {
		// 分页查询入库单据-表体
		page: baseApi.page,
		// 查看入库单据-表体详细
		detail: baseApi.detail,
		// 新增入库单据-表体
		add: baseApi.add,
		// 更新入库单据-表体
		update: baseApi.update,
		// 删除入库单据-表体
		delete: baseApi.delete,
		// 批量删除入库单据-表体
		batchDelete: baseApi.batchDelete,
		// 导出入库单据-表体数据
		exportData: baseApi.exportData,
		// 导入入库单据-表体数据
		importData: baseApi.importData,
		// 下载入库单据-表体数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
		// 获取下拉列表数据
		getDropdownData: (fromPage: Boolean = false, cancel: boolean = false) => baseApi.dropdownData({ fromPage }, cancel),
	}
}

// 入库单据-表体实体
export interface WmsImportNotifyDetail {
	// 主键Id
	id: number;
	// 入库单据ID
	importId: number;
	// 外部单据明细ID
	outerDetailId: string;
	// 序号
	importListNo: string;
	// 物料信息
	materialId: number;
	// 批次
	lotNo: string;
	// 物料状态
	materialStatus: string;
	// 生产日期
	importProductionDate: string;
	// 失效日期
	importLostDate: string;
	// 计划数量
	importQuantity: number;
	// 完成数量
	importCompleteQuantity: number;
	// 组盘数量
	importFactQuantity: number;
	// 已上传数量
	uploadQuantity: number;
	// 执行状态
	importExecuteFlag: string;
	// 备注
	remark: string;
	// 打印标识，1打印，0不打印
	printFlag: number;
	// 下发任务状态默认0，1下发成功
	taskStatus: number;
	// 标签判断，1已打印，2或空未打印
	labelJudgment: number;
	// 软删除
	isDelete: number;
	// 创建者Id
	createUserId: number;
	// 创建时间
	createTime: string;
	// 修改者Id
	updateUserId: number;
	// 更新时间
	updateTime: string;
	// 件数
	outQty: number;
	// 巷道编码
	lanewayCode: string;
	// 采浆公司
	xj_HouseCode: string;
	// 血浆件数
	xj_Qty: number;
	// 血浆重量
	xj_Weight: number;
	// 血浆 箱总数
	xj_BoxQty: number;
	// 血浆类型
	xj_GoodCode: number;
	// 单据类型（1:入库；2：取消入库）
	xj_Type: number;
	// 已验证数量
	verifiedQty: number;
	// 千克数量
	kilogramQty: number;
	// 创建者姓名
	createUserName: string;
	// 修改者姓名
	updateUserName: string;
}