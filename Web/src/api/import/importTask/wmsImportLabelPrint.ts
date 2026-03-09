import {useBaseApi} from '/@/api/base';

// 入库标签打印接口服务
export const useWmsImportLabelPrintApi = () => {
	const baseApi = useBaseApi("wmsImportLabelPrint");
	return {
		// 分页查询入库标签打印
		page: baseApi.page,
		// 查看入库标签打印详细
		detail: baseApi.detail,
		// 新增入库标签打印
		add: baseApi.add,
		// 更新入库标签打印
		update: baseApi.update,
		// 删除入库标签打印
		delete: baseApi.delete,
		// 批量删除入库标签打印
		batchDelete: baseApi.batchDelete,
		// 导出入库标签打印数据
		exportData: baseApi.exportData,
		// 导入入库标签打印数据
		importData: baseApi.importData,
		// 下载入库标签打印数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 入库标签打印实体
export interface WmsImportLabelPrint {
	// 主键Id
	id: number;
	// 入库单Id
	importId: number;
	// 标签序号
	labelStream: string;
	// 物料编号
	materialCode: string;
	// 物料名称
	materialName: string;
	// 物料规格
	materialStandard: string;
	// 批次
	lotNo: string;
	// 是否使用
	isUse: number;
	// 箱编码
	boxCode: string;
	// 箱数量
	quantity: number;
	// 满箱标识
	mxFlag: number;
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