import {useBaseApi} from '/@/api/base';

// 供应商信息接口服务
export const useWmsBaseSupplierApi = () => {
	const baseApi = useBaseApi("wmsBaseSupplier");
	return {
		// 分页查询供应商信息
		page: baseApi.page,
		// 查看供应商信息详细
		detail: baseApi.detail,
		// 新增供应商信息
		add: baseApi.add,
		// 更新供应商信息
		update: baseApi.update,
		// 删除供应商信息
		delete: baseApi.delete,
		// 批量删除供应商信息
		batchDelete: baseApi.batchDelete,
		// 导出供应商信息数据
		exportData: baseApi.exportData,
		// 导入供应商信息数据
		importData: baseApi.importData,
		// 下载供应商信息数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 供应商信息实体
export interface WmsBaseSupplier {
	// 主键Id
	id: number;
	// 供应商编码
	customerCode?: string;
	// 供应商名称
	customerName?: string;
	// 供应商地址
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
}