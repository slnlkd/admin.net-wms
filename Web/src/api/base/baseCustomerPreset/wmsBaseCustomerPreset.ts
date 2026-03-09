import {useBaseApi} from '/@/api/base';

// 申请客户信息接口服务
export const useWmsBaseCustomerPresetApi = () => {
	const baseApi = useBaseApi("wmsBaseCustomerPreset");
	return {
		// 分页查询申请客户信息
		page: baseApi.page,
		// 查看申请客户信息详细
		detail: baseApi.detail,
		// 新增申请客户信息
		add: baseApi.add,
		// 更新申请客户信息
		update: baseApi.update,
		// 删除申请客户信息
		delete: baseApi.delete,
		// 批量删除申请客户信息
		batchDelete: baseApi.batchDelete,
		// 导出申请客户信息数据
		exportData: baseApi.exportData,
		// 导入申请客户信息数据
		importData: baseApi.importData,
		// 下载申请客户信息数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
		// 审核申请客户信息
		approvalCustomer: (id: any, approvalStatus: any, approvalRemark?: string) => {
			return baseApi.request({
				url: baseApi.baseUrl + "Approval",
				method: 'POST',
				data: {"id":id,"approvalStatus":approvalStatus,"approvalRemark":approvalRemark},
			});
		},
		// 批量审核申请客户信息
		batchApprovalCustomer: (ids: any[], approvalStatus: any, approvalRemark?: string) => {
			return baseApi.request({
				url: baseApi.baseUrl + "BatchApproval",
				method: 'POST',
				data: {"ids":ids,"approvalStatus":approvalStatus,"approvalRemark":approvalRemark},
			});
		},
	}
}

// 申请客户信息实体
export interface WmsBaseCustomerPreset {
	// 主键Id
	id: number;
	// 客户编码
	customerCode: string;
	// 客户名称
	customerName: string;
	// 客户地址
	customerAddress: string;
	// 联系人
	customerLinkman: string;
	// 联系电话
	customerPhone: string;
	// 客户类型
	customerTypeId: string;
	// 备注
	remark: string;
	// 软删除
	isDelete?: boolean;
	// 授权编码
	token: string;
	// 授权用户
	accountExec: string;
	// 状态 0启用，1禁用
	status: number;
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
	// 审核状态
	approvalStatus: number;
	// 审核人
	approvalUserName: string;
	// 审核ID
	approvalUserId: number;
	// 审核备注
	approvalRemark: string;
}