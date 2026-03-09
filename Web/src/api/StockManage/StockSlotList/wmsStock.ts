import {useBaseApi} from '/@/api/base';

// 库存查询接口服务
export const useWmsStockApi = () => {
	const baseApi = useBaseApi("wmsStock");
	return {
		// 分页查询库存查询
		page: baseApi.page,
		// 分页查询库存详情查询
		GetStockDetailsList: baseApi.GetStockDetailsList,
		// 查看库存查询详细
		detail: baseApi.detail,
		// 新增库存查询
		add: baseApi.add,
		// 更新库存查询
		update: baseApi.update,
		// 删除库存查询
		delete: baseApi.delete,
		// 批量删除库存查询
		batchDelete: baseApi.batchDelete,
	}
}

// 库存查询实体
export interface WmsStock {
	// 主键Id
	id: number;
	// 物料id
	materialId: number;
	// 库存数量
	stockQuantity: number;
	// 锁定数量
	lockQuantity: number;
	// 批次
	lotNo: string;
	// 库房ID
	warehouseId: number;
	// 生产日期
	productionDate: string;
	// 有效日期
	validateDay: string;
	// 质检状态
	inspectionStatus: number;
	// 创建时间
	createTime: string;
	// 更新时间
	updateTime: string;
	// 创建者Id
	createUserId: number;
	// 创建者姓名
	createUserName: string;
	// 修改者Id
	updateUserId: number;
	// 修改者姓名
	updateUserName: string;
	// 软删除
	isDelete?: boolean;
	// 客户id
	customerId: number;
	// 生产商id
	produceId: number;
	// 供应商id
	supplierId: number;
	// 制造商id
	manufacturerId: number;
}