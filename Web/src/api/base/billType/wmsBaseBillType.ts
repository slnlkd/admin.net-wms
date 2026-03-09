import {useBaseApi} from '/@/api/base';

// 单据类型表接口服务
export const useWmsBaseBillTypeApi = () => {
	const baseApi = useBaseApi("wmsBaseBillType");
	return {
		// 分页查询单据类型表
		page: baseApi.page,
		// 查看单据类型表详细
		detail: baseApi.detail,
		// 新增单据类型表
		add: baseApi.add,
		// 更新单据类型表
		update: baseApi.update,
		// 删除单据类型表
		delete: baseApi.delete,
		// 批量删除单据类型表
		batchDelete: baseApi.batchDelete,
		// 导出单据类型表数据
		exportData: baseApi.exportData,
		// 导入单据类型表数据
		importData: baseApi.importData,
		// 下载单据类型表数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
		// 获取下拉列表数据
		getDropdownData: (fromPage: Boolean = false, cancel: boolean = false) => baseApi.dropdownData({ fromPage }, cancel),
	}
}

// 单据类型表实体
export interface WmsBaseBillType {
	// 主键Id
	id: number;
	// 单据类型编码
	billTypeCode?: string;
	// 单据类型名称
	billTypeName?: string;
	// 备注
	remark: string;
	// 类型
	billType?: number;
	// 软删除
	isDelete?: boolean;
	// 质检状态
	qualityInspectionStatus?: string;
	// 代储状态
	proxyStatus?: number;
	// 所属仓库
	wareHouseId?: number;
	// 单据子类型上级编码
	subtypeID: string;
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