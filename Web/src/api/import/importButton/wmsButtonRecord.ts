import {useBaseApi} from '/@/api/base';

// 按钮点击表接口服务
export const useWmsButtonRecordApi = () => {
	const baseApi = useBaseApi("wmsButtonRecord");
	return {
		// 分页查询按钮点击表
		page: baseApi.page,
		// 查看按钮点击表详细
		detail: baseApi.detail,
		// 新增按钮点击表
		add: baseApi.add,
		// 更新按钮点击表
		update: baseApi.update,
		// 删除按钮点击表
		delete: baseApi.delete,
		// 批量删除按钮点击表
		batchDelete: baseApi.batchDelete,
		// 导出按钮点击表数据
		exportData: baseApi.exportData,
		// 导入按钮点击表数据
		importData: baseApi.importData,
		// 下载按钮点击表数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 按钮点击表实体
export interface WmsButtonRecord {
	// 主键Id
	id: number;
	// 用户Id
	userId: number;
	// 按钮点击时间
	clickButtonDate: string;
	// 记录按钮信息（ 用户名+按钮点击时间+记录按钮信息）
	buttonInformation: string;
	// 用户编码
	userCode: string;
	// 用户名
	userName: string;
	// 菜单名称
	pageName: string;
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