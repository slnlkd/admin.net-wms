import {useBaseApi} from '/@/api/base';

// 移库单据明细表接口服务
export const useWmsMoveNotifyApi = () => {
	const baseApi = useBaseApi("wmsMoveNotify");
	return {
		// 分页查询移库单据明细表
		page: baseApi.page,
		// 查看移库单据明细表详细
		detail: baseApi.detail,
		// 新增移库单据明细表
		add: baseApi.add,
		// 更新移库单据明细表
		update: baseApi.update,
		// 删除移库单据明细表
		delete: baseApi.delete,
		// 批量删除移库单据明细表
		batchDelete: baseApi.batchDelete,
		// 导出移库单据明细表数据
		exportData: baseApi.exportData,
		// 导入移库单据明细表数据
		importData: baseApi.importData,
		// 下载移库单据明细表数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 移库单据明细表实体
export interface WmsMoveNotify {
	// 主键Id
	id: number;
	// 移库单据类型
	moveBillCode: string;
	// 移库序号
	moveListNo: number;
	// 移库批次
	moveLotNo: string;
	// 物料id
	materialId: string;
	// 移库物料型号
	moveMaterialModel: string;
	// 物料温度
	moveMaterialTemp: string;
	// 物料类型（字典MaterialType）
	moveMaterialType: string;
	// 物料状态
	moveMaterialStatus: string;
	// 物料单位
	moveMaterialUnit: string;
	// 物料品牌
	moveMaterialBrand: string;
	// 生产日期
	moveProductionDate: string;
	// 失效日期
	moveLostDate: string;
	// 移库数量
	moveQuantity: number;
	// 移库日期
	moveDate: string;
	// 仓库id
	moveWarehouseId: number;
	// 移出巷道id
	moveLanewayOutCode: number;
	// 移入巷道id
	moveLanewayInCode: string;
	// 移出储位编码
	moveOutSlotCode: string;
	// 移入储位编码
	moveInSlotCode: string;
	// 执行标志（01待执行、02正在执行、03已完成、04已上传）
	moveExecuteFlag: string;
	// 备注
	moveRemark: string;
	// 移库任务号
	moveTaskNo: string;
	// 库存表id
	stockStockCodeId: number;
	// 创建者Id
	createUserId: number;
	// 创建时间
	createTime: string;
	// 修改者Id
	updateUserId: number;
	// 更新时间
	updateTime: string;
	// 软删除
	isDelete?: boolean;
	// 创建者姓名
	createUserName: string;
	// 修改者姓名
	updateUserName: string;
}