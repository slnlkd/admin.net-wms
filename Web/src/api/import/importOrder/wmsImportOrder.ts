import {useBaseApi} from '/@/api/base';

// 入库流水接口服务
export const useWmsImportOrderApi = () => {
	const baseApi = useBaseApi("wmsImportOrder");
	return {
		// 分页查询入库流水
		page: baseApi.page,
		// 查看入库流水详细
		detail: baseApi.detail,
		// 新增入库流水
		add: baseApi.add,
		// 更新入库流水
		update: baseApi.update,
		// 删除入库流水
		delete: baseApi.delete,
		// 批量删除入库流水
		batchDelete: baseApi.batchDelete,
		// 导出入库流水数据
		exportData: baseApi.exportData,
		// 导入入库流水数据
		importData: baseApi.importData,
		// 下载入库流水数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
		// 获取下拉列表数据
		getDropdownData: (fromPage: Boolean = false, cancel: boolean = false) => baseApi.dropdownData({ fromPage }, cancel),
		// 保存指定储位
		saveSlot: baseApi.saveSlot
	}
}

// 入库流水实体
export interface WmsImportOrder {
	// 主键Id
	id: number;
	// 流水号
	importOrderNo: string;
	// 入库单ID
	importId: number;
	// 区域ID
	importAreaId: number;
	// 巷道ID
	importLanewayId: number;
	// 储位编码
	importSlotCode: string;
	// 托盘条码ID
	stockCodeId: number;
	// 数量
	importQuantity: number;
	// 重量
	importWeight: number;
	// 任务ID
	taskId: number;
	// 执行标志（0待执行、1正在执行、2已完成、3已上传）
	importExecuteFlag: string;
	// 备注
	remark: string;
	// 班次
	importClass: string;
	// 批号
	lotNo: string;
	// 生产日期
	importProductionDate: string;
	// 完成时间
	completeDate: string;
	// 质检状态
	inspectionStatus: number;
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
	// 仓库ID
	wareHouseId: number;
	// 入库单明细ID
	importDetailId: number;
	// 主子载具号
	subVehicleCode: string;
	// 实际称重重量
	weight: number;
	// 0未验收 1正在验收 2验收完成
	inspectFlag: number;
	// 是否暂存入库流水（不为空为是）
	isTemporaryStorage: string;
	// 载具号
	stockCode: string;
	// 验收0 挑浆1
	ysOrTJ: string;
	// 创建者姓名
	createUserName: string;
	// 修改者姓名
	updateUserName: string;
}