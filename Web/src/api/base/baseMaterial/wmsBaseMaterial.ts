import {useBaseApi} from '/@/api/base';

// 时间筛选器枚举
export enum TimeFilter {
	Week = 0,
	Month = 1,
	Quarter = 2
}

// 物料信息接口服务
export const useWmsBaseMaterialApi = () => {
	const baseApi = useBaseApi("wmsBaseMaterial");
	return {
		// 分页查询物料信息
		page: baseApi.page,
		// 查看物料信息详细
		detail: baseApi.detail,
		// 新增物料信息
		add: baseApi.add,
		// 更新物料信息
		update: baseApi.update,
		// 删除物料信息
		delete: baseApi.delete,
		// 批量删除物料信息
		batchDelete: baseApi.batchDelete,
		// 导出物料信息数据
		exportData: baseApi.exportData,
		// 导入物料信息数据
		importData: baseApi.importData,
		// 下载物料信息数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
		// 获取下拉列表数据
		getDropdownData: (fromPage: Boolean = false, cancel: boolean = false) => baseApi.dropdownData({ fromPage }, cancel),
		// 获取物料库存占比数据
		getMaterialStockRatio: (timeFilter: TimeFilter = TimeFilter.Week, cancel: boolean = false) => {
			return baseApi.request({
				url: baseApi.baseUrl + "getMaterialStockRatio?timeFilter=" + timeFilter,
				method: 'get',
			}, cancel);
		},
	}
}

// 物料库存项
export interface WmsMaterialStockItem {
	// 物料名称
	name: string;
	// 数量
	value: number;
	// 占比百分比
	percentage: number;
	// 颜色值
	color: string;
}

// 物料库存占比输出
export interface WmsMaterialStockRatioOutput {
	// 物料数据列表
	materialData: WmsMaterialStockItem[];
	// 库存总量
	totalInventory: number;
}

// 物料信息实体
export interface WmsBaseMaterial {
	// 主键Id
	id: number;
	// 物料编码
	materialCode?: string;
	// 物料类型
	materialType?: number;
	// 物料名称
	materialName?: string;
	// 物料助记码
	materialMcode: string;
	// 物料规格
	materialStandard: string;
	// 计量单位
	materialUnit: number;
	// 专属区域
	materialAreaId: number;
	// 箱数量
	boxQuantity?: string;
	// 物料来源
	materialOrigin: string;
	// 备注
	remark: string;
	// 创建者姓名
	createUserName: string;
	// 创建时间
	createTime: string;
	// 修改者姓名
	updateUserName: string;
	// 更新时间
	updateTime: string;
	// 物料型号
	materialModel: string;
	// 每件托数
	everyNumber?: string;
	// 载具
	vehicle?: string;
	// 物料重量
	materialWeight: number;
	// 有效期1
	materialValidityDay1: string;
	// 有效期2
	materialValidityDay2: string;
	// 有效期3
	materialValidityDay3: string;
	// 温度
	materialTemp: string;
	// 软删除
	isDelete: boolean;
	// 创建者Id
	createUserId: number;
	// 修改者Id
	updateUserId: number;
	// 最高库存数量
	materialStockHigh: string;
	// 最低库存数量
	materialStockLow: string;
	// 所属仓库
	warehouseId?: string;
	// 提前预警天数
	materialAlarmDay: string;
	// 贴标
	labeling?: string;
	// 管理方式
	manageType: string;
	// 外部物品编码
	outerInnerCode: string;
	// 最低储备
	mixReserve: string;
	// 最高储备
	maxReserve: string;
	// 提前报警天数
	alarmDay: number;
	// 授权编码
	token: string;
	// 授权用户
	accountExec: string;
	// 启用状态
	status: boolean;
	// 是否空托
	isEmpty: boolean;
}