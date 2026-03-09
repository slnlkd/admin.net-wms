import {useBaseApi} from '/@/api/base';
import request from '/@/utils/request';

// 密码策略接口服务
export const useWmsPasswordPolicyApi = () => {
	const baseApi = useBaseApi("wmsPasswordPolicy");
    const baseUrl = `/api/WmsUserPasswordHistory/`;
	return {
		// 分页查询密码策略
		page: baseApi.page,
		// 查看密码策略详细
		detail: baseApi.detail,
		// 查看已启用的密码策略详细
		getEnableDetail: baseApi.getEnableDetail,
		// 新增密码策略
		add: baseApi.add,
		// 更新密码策略
		update: baseApi.update,
		// 删除密码策略
		delete: baseApi.delete,
		// 批量删除密码策略
		batchDelete: baseApi.batchDelete,
		// 导出密码策略数据
		exportData: baseApi.exportData,
		// 导入密码策略数据
		importData: baseApi.importData,
		// 下载密码策略数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
		// 获取历史密码记录
		getPasswordHistory: function () {
            var data = request({
                url: baseUrl + "GetListByUser",
                method: 'get',
            });
			return data
        },
	}
}

// 密码策略实体
export interface WmsPasswordPolicy {
	// 主键Id
	id: number;
	// 策略名称
	name?: string;
	// 策略描述
	description: string;
	// 普通用户密码最小长度
	minLength?: number;
	// 管理员密码最小长度
	adminMinLength?: number;
	// 需要包含的字符类别数量（大写、小写、数字、特殊字符）
	requireCategories?: number;
	// 最大连续相同字符数
	maxConsecutive?: number;
	// 是否必须包含小写字母
	requireLowercase?: boolean;
	// 是否必须包含大写字母
	requireUppercase?: boolean;
	// 是否必须包含数字
	requireDigit?: boolean;
	// 是否必须包含特殊字符
	requireSpecial?: boolean;
	// 允许的特殊字符集合
	specialChars?: string;
	// 是否检查密码不能包含用户名
	checkUserId?: boolean;
	// 是否检查连续字符
	checkConsecutive?: boolean;
	// 是否检查常见模式（如123456、qwerty等）
	checkCommonPatterns?: boolean;
	// 是否检查重复字符
	checkRepeatedChars?: boolean;
	// 密码历史记录数量，不能重复使用
	rememberPasswords?: number;
	// 普通用户密码过期天数
	passwordExpiryDays?: number;
	// 管理员密码过期天数
	adminPasswordExpiryDays?: number;
	// 密码过期提前提醒天数
	expiryNoticeDays?: number;
	// 是否强制首次登录修改密码
	forceChangeInitial?: boolean;
	// 策略是否启用
	enabled?: boolean;
	// 创建者Id
	createUserId?: number;
	// 创建者姓名
	createUserName: string;
	// 创建时间
	createTime?: string;
	// 修改者Id
	updateUserId?: number;
	// 修改者姓名
	updateUserName: string;
	// 更新时间
	updateTime?: string;
}