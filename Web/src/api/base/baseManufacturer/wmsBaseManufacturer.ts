import {useBaseApi} from '/@/api/base';

// 制造商信息接口服务
export const useWmsBaseManufacturerApi = () => {
	const baseApi = useBaseApi("wmsBaseManufacturer");
	return {
		// 分页查询制造商信息
		page: baseApi.page,
		// 查看制造商信息详细
		detail: baseApi.detail,
		// 新增制造商信息
		add: baseApi.add,
		// 更新制造商信息
		update: baseApi.update,
		// 删除制造商信息
		delete: baseApi.delete,
		// 批量删除制造商信息
		batchDelete: baseApi.batchDelete,
		// 导出制造商信息数据
		exportData: baseApi.exportData,
		// 导入制造商信息数据
		importData: baseApi.importData,
		// 下载制造商信息数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 制造商信息实体
export interface WmsBaseManufacturer {
	// 主键Id
	id: number;
	// 制造商编码
	customerCode?: string;
	// 制造商名称
	customerName?: string;
	// 制造商地址
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