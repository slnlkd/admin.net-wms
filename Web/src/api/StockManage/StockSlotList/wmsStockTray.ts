import {useBaseApi} from '/@/api/base';

// 库存明细查询接口服务
export const useWmsStockTrayApi = () => {
	const baseApi = useBaseApi("wmsStockTray");
	return {
		// 分页查询库存明细查询
		page: baseApi.page,
		// 查看库存明细查询详细
		detail: baseApi.detail,
		// 新增库存明细查询
		add: baseApi.add,
		// 更新库存明细查询
		update: baseApi.update,
		// 删除库存明细查询
		delete: baseApi.delete,
		// 批量删除库存明细查询
		batchDelete: baseApi.batchDelete,
		// 获取下拉列表数据
		getDropdownData: (fromPage: Boolean = false, cancel: boolean = false) => baseApi.dropdownData({ fromPage }, cancel),
	}
}

// 库存明细查询实体
export interface WmsStockTray {
	// 主键Id
	id: number;
	// 储位位置
	stockSlotCode: string;
	// 托盘编码
	stockCode: string;
	// 库存数量
	stockQuantity: number;
	// 库存日期
	stockDate: string;
	// 物品编码
	materialId: string;
	// 库存状态
	stockStatusFlag: number;
	// 库存批次
	lotNo: string;
	// 质检状态
	inspectionStatus: number;
	// 巷道ID
	lanewayId: string;
	// 仓库ID
	warehouseId: string;
	// 库存备注
	remark: string;
	// 抽检托
	isSamolingTray: number;
	// 锁定数量
	lockQuantity: number;
	// 出库单号
	ownerCode: string;
	// 异常状态
	abnormalStatu: number;
	// 生产日期
	productionDate: string;
	// 失效日期
	validateDay: string;
	// 放行状态（0 正常 1 放行）
	releaseStatus: number;
	// 复验日期
	retestDate: string;
	// 质检单号
	inspectionNumber: string;
	// 件数
	outQty: number;
	// 物料箱数量
	boxQuantity: number;
	// 零头标识
	oddMarking: number;
	// 客户ID
	customerId: string;
	// 改判日期
	revisionDate: string;
	// 血浆主子关系对应，存放主血箱ID
	vehicleSubId: string;
	// 0未验收 1验收完成
	inspectFlag: number;
	// 改判日期
	releaseDate: string;
	// 创建者姓名
	createUserName: string;
	// 修改者姓名
	updateUserName: string;
	// 软删除
	isDelete: boolean;
	// 创建时间
	createTime: string;
	// 更新时间
	updateTime: string;
	// 创建者Id
	createUserId: number;
	// 修改者Id
	updateUserId: number;
	// 供应商ID
	supplierId: number;
	// 制造商ID
	manufacturerId: number;
}