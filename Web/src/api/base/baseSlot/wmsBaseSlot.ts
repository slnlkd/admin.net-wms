import {useBaseApi} from '/@/api/base';

// 储位管理接口服务
export const useWmsBaseSlotApi = () => {
	const baseApi = useBaseApi("wmsBaseSlot");
	return {
		// 分页查询储位管理
		page: baseApi.page,
		// 查看储位管理详细
		detail: baseApi.detail,
		// 查看储位详细信息
		getSlotDetail: baseApi.getSlotDetail,
		// 新增储位管理
		add: baseApi.add,
		// 更新储位管理
		update: baseApi.update,
		// 删除储位管理
		delete: baseApi.delete,
		// 批量删除储位管理
		batchDelete: baseApi.batchDelete,
		// 导出储位管理数据
		exportData: baseApi.exportData,
		// 导入储位管理数据
		importData: baseApi.importData,
		// 下载储位管理数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
		// 获取下拉列表数据
		getDropdownData: (fromPage: Boolean = false, cancel: boolean = false) => baseApi.dropdownData({ fromPage }, cancel),
		// 获取下拉列表数据
		getDropdownDataLayer: (warehouseId:any, cancel: boolean = false) => baseApi.dropdownDataLayer({ warehouseId }, cancel),
		// 获取下拉列表数据
		getDropdownDataRow: (warehouseId:any,slotLanewayId:any, cancel: boolean = false) => baseApi.dropdownDataRow({ warehouseId,slotLanewayId }, cancel),
		// 获取储位图例
		getSlotLegendList: (data:any, cancel: boolean = false) => baseApi.getSlotLegendList(data, cancel),

	}
}

// 储位管理实体
export interface WmsBaseSlot {
	// 主键Id
	id: number;
	// 所属仓库
	warehouseId?: number;
	// 所属区域
	slotAreaId?: number;
	// 所属巷道
	slotLanewayId?: number;
	// 排
	slotRow: number;
	// 列
	slotColumn: number;
	// 层
	slotLayer: number;
	// 储位深度
	slotInout: number;
	// 储位编码
	slotCode: string;
	// 储位状态
	slotStatus: number;
	// 库位类型
	make: string;
	// 储位属性
	property: string;
	// 储位处理
	handle: string;
	// 储位环境
	environment: string;
	// 储位序号(双深位同侧储位公用一个序号)
	slotCodeIndex: number;
	// 入库锁定
	slotImlockFlag: number;
	// 出库锁定
	slotExlockFlag: number;
	// 是否屏蔽
	slotCloseFlag: number;
	// 备注
	remark: string;
	// 储位高度
	slotHigh: number;
	// 限重
	slotWeight: string;
	// 目的中转货位
	endTransitLocation: string;
	// 四向车位置
	isSxcLocation: number;
	// 创建者姓名
	createUserName: string;
	// 修改者姓名
	updateUserName: string;
	// 创建者Id
	createUserId?: number;
	// 修改者Id
	updateUserId?: number;
	// 创建时间
	createTime?: string;
	// 更新时间
	updateTime?: string;
}