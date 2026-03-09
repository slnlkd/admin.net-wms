import {useBaseApi} from '/@/api/base';

// 申请物料信息接口服务
export const useWmsBaseMaterialPresetApi = () => {
	const baseApi = useBaseApi("wmsBaseMaterialPreset");
	return {
		// 分页查询申请物料信息
		page: baseApi.page,
		// 查看申请物料信息详细
		detail: baseApi.detail,
		// 新增申请物料信息
		add: baseApi.add,
		// 更新申请物料信息
		update: baseApi.update,
		// 删除申请物料信息
		delete: baseApi.delete,
		// 批量删除申请物料信息
		batchDelete: baseApi.batchDelete,
		// 导出申请物料信息数据
		exportData: baseApi.exportData,
		// 导入申请物料信息数据
		importData: baseApi.importData,
		// 下载申请物料信息数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
		// 获取下拉列表数据
		getDropdownData: (fromPage: Boolean = false, cancel: boolean = false) => baseApi.dropdownData({ fromPage }, cancel),
		// 审核申请物料信息
		approvalMaterial: (id: any,approvalStatus: any, approvalRemark?: string) => {
			return baseApi.request({
				url: baseApi.baseUrl + "Approval",
				method: 'POST',
				data: {"id":id,"approvalStatus":approvalStatus,"approvalRemark":approvalRemark},
			});
		},
		// 批量审核申请物料信息
		batchApprovalMaterial: (ids: any[], approvalStatus: any, approvalRemark?: string) => {
			return baseApi.request({
				url: baseApi.baseUrl + "BatchApproval",
				method: 'POST',
				data: {"ids":ids,"approvalStatus":approvalStatus,"approvalRemark":approvalRemark},
			});
		},
	}
}

// 申请物料信息实体
export interface WmsBaseMaterialPreset {
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
	// 0=待审核，1=审核通过，2=驳回
	approvalStatus: number;
	// 审核人名称
	approvalUserName: string;
	// 审核ID
	approvalUserId: number;
	// 驳回原因
	approvalRemark?: string;
}