import {useBaseApi} from '/@/api/base';

// 计量单位接口服务
export const useWmsBaseUnitApi = () => {
	const baseApi = useBaseApi("wmsBaseUnit");
	return {
		// 分页查询计量单位
		page: baseApi.page,
		// 查看计量单位详细
		detail: baseApi.detail,
		// 新增计量单位
		add: baseApi.add,
		// 更新计量单位
		update: baseApi.update,
		// 删除计量单位
		delete: baseApi.delete,
		// 批量删除计量单位
		batchDelete: baseApi.batchDelete,
		// 导出计量单位数据
		exportData: baseApi.exportData,
		// 导入计量单位数据
		importData: baseApi.importData,
		// 下载计量单位数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 计量单位实体
export interface WmsBaseUnit {
	// 主键Id
	id: number;
	// 单位编码
	unitCode: string;
	// 单位名称
	unitName?: string;
	// 单位缩写名称
	unitAbbrevName: string;
	// 备注
	remark: string;
	// 软删除
	isDelete?: boolean;
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