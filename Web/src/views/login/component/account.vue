<template>
	<el-tooltip :visible="state.capsLockVisible" effect="light" content="大写锁定已打开" placement="top">
		<el-form ref="ruleFormRef" :model="state.ruleForm" size="large" :rules="state.rules" class="login-content-form">
			<el-form-item class="login-animation1" prop="account">
				<el-input ref="accountRef" text placeholder="请输入账号" v-model="state.ruleForm.account" clearable
					autocomplete="off" @keyup.enter.native="handleSignIn">
					<template #prefix>
						<el-icon>
							<ele-User />
						</el-icon>
					</template>
				</el-input>
			</el-form-item>
			<el-form-item class="login-animation2" prop="password">
				<el-input ref="passwordRef" :type="state.isShowPassword ? 'text' : 'password'" placeholder="请输入密码"
					v-model="state.ruleForm.password" autocomplete="off" @keyup.enter.native="handleSignIn">
					<template #prefix>
						<el-icon>
							<ele-Unlock />
						</el-icon>
					</template>
					<template #suffix>
						<i class="iconfont el-input__icon login-content-password"
							:class="state.isShowPassword ? 'icon-yincangmima' : 'icon-xianshimima'"
							@click="state.isShowPassword = !state.isShowPassword">
						</i>
					</template>
				</el-input>
			</el-form-item>
			<el-form-item class="login-animation2" prop="tenantId" clearable
				v-if="!props.tenantInfo.id && !state.hideTenantForLogin">
				<el-select v-model="state.ruleForm.tenantId" placeholder="请选择租户" style="width: 100%" filterable>
					<template #prefix>
						<i class="iconfont icon-shuxingtu el-input__icon"></i>
					</template>
					<el-option :value="item.value" :label="item.label" v-for="(item, index) in tenantInfo.list"
						:key="index" />
				</el-select>
			</el-form-item>
			<el-form-item class="login-animation3" prop="captcha" v-if="state.captchaEnabled">
				<el-col :span="15">
					<el-input ref="codeRef" text maxlength="4" placeholder="请输入验证码" v-model="state.ruleForm.code"
						clearable autocomplete="off" @keyup.enter.native="handleSignIn">
						<template #prefix>
							<el-icon>
								<ele-Position />
							</el-icon>
						</template>
					</el-input>
				</el-col>
				<el-col :span="1"></el-col>
				<el-col :span="8">
					<div :class="[state.expirySeconds > 0 ? 'login-content-code' : 'login-content-code-expired']"
						@click="getCaptcha">
						<img class="login-content-code-img" width="130px" height="38px" :src="state.captchaImage"
							style="cursor: pointer" />
					</div>
				</el-col>
			</el-form-item>
			<el-form-item class="login-animation4">
				<el-button type="primary" class="login-content-submit" round v-waves @click="handleSignIn"
					:loading="state.loading.signIn">
					<span>登 录</span>
				</el-button>
			</el-form-item>
			<div class="font12 mt30 login-animation4 login-msg">* 温馨提示：建议使用谷歌、Microsoft Edge，版本 79.0.1072.62
				及以上浏览器，360浏览器请使用极速模式
			</div>
			<!-- <el-button type="primary" round v-waves @click="weixinSignIn" :loading="state.loading.signIn"></el-button> -->
		</el-form>
	</el-tooltip>

	<!-- 旋转验证对话框 -->
	<div class="dialog-header">
		<el-dialog v-model="state.rotateVerifyVisible" :show-close="false">
			<DragVerifyImgRotate ref="dragRef" :imgsrc="state.rotateVerifyImg" v-model:isPassing="state.isPassRotate"
				text="请按住滑块拖动" successText="验证通过" handlerIcon="fa fa-angle-double-right"
				successIcon="fa fa-hand-peace-o" @passcallback="passRotateVerify" />
		</el-dialog>
	</div>

	<!-- 强制修改密码对话框 -->
	<ForceChangePasswordDialog v-model="state.forceChangePasswordVisible" :user-name="state.userInfoForPassword.account"
		:is-admin="state.userInfoForPassword.accountType === 999" :is-force-change="true"
		:old-pwd="state.ruleForm.password" success-message="密码修改成功，即将进入系统" @success="handleForceChangeSuccess" />
</template>

<script lang="ts" setup name="loginAccount">
import { reactive, computed, ref, onMounted, defineAsyncComponent, onUnmounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessage, ElNotification, InputInstance } from 'element-plus';
import { initBackEndControlRoutes } from '/@/router/backEnd';
import { Local, Session } from '/@/utils/storage';
import { formatAxis } from '/@/utils/formatTime';
import { NextLoading } from '/@/utils/loading';
import { sm2 } from 'sm-crypto-v2';
import { useThemeConfig } from '/@/stores/themeConfig';
import { useWmsPasswordPolicyApi } from '/@/api/system/pwdPolicy/wmsPasswordPolicy';
import { storeToRefs } from 'pinia';

import { accessTokenKey, clearTokens, feature, getAPI } from '/@/utils/axios-utils';
import { SysAuthApi } from '/@/api-services/api';

const props = defineProps({
	tenantInfo: {
		required: true,
		type: Object,
	},
});

// 旋转图片滑块组件
const DragVerifyImgRotate = defineAsyncComponent(() => import('/@/components/dragVerify/dragVerifyImgRotate.vue'));
// 强制修改密码对话框组件
const ForceChangePasswordDialog = defineAsyncComponent(() => import('/@/components/ForceChangePasswordDialog/index.vue'));

const wmsPasswordPolicyApi = useWmsPasswordPolicyApi();
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);

const route = useRoute();
const router = useRouter();

const ruleFormRef = ref();
const accountRef = ref<InputInstance>();
const passwordRef = ref<InputInstance>();
const codeRef = ref<InputInstance>();

const dragRef: any = ref(null);
const policy = ref();
const state = reactive({
	isShowPassword: false,
	ruleForm: {
		account: window.__env__.VITE_DEFAULT_USER,
		password: window.__env__.VITE_DEFAULT_USER_PASSWORD,
		tenantId: props.tenantInfo.id,
		code: '',
		codeId: 0,
	},
	rules: {
		account: [{ required: true, message: '请输入账号', trigger: 'blur' }],
		password: [{ required: true, message: '请输入密码', trigger: 'blur' }],
		// code: [{ required: true, message: '请输入验证码', trigger: 'blur' }],
	},
	loading: {
		signIn: false,
	},
	captchaImage: '',
	rotateVerifyVisible: false,
	rotateVerifyImg: themeConfig.value.logoUrl,
	secondVerEnabled: false,
	captchaEnabled: false,
	isPassRotate: false,
	capsLockVisible: false,
	hideTenantForLogin: false,
	expirySeconds: 60, // 验证码过期时间

	// 新增：强制修改密码相关状态
	forceChangePasswordVisible: false,
	userInfoForPassword: {
		account: '',
		accountType: 0,
		// 可以根据需要添加其他用户信息字段
	},
	// 存储登录成功后的token，用于后续初始化路由
	loginSuccessToken: '',
});

// 验证码过期计时器
let timer: any = null;

// 页面初始化
onMounted(async () => {
	// 若URL带有Token参数（第三方登录）
	const accessToken = route.query.token;
	if (accessToken) await saveTokenAndInitRoutes(accessToken);
	watch(
		() => themeConfig.value.isLoaded,
		(isLoaded) => {
			if (isLoaded) {
				// 获取登录配置
				state.hideTenantForLogin = themeConfig.value.hideTenantForLogin ?? true;
				state.secondVerEnabled = themeConfig.value.secondVer ?? true;
				state.captchaEnabled = themeConfig.value.captcha ?? true;

				// 获取验证码
				getCaptcha();

				// 注册验证码过期计时器
				if (state.captchaEnabled) {
					timer = setInterval(() => {
						if (state.expirySeconds > 0) state.expirySeconds -= 1;
					}, 1000);
				}
			}
		},
		{ immediate: true }
	);

	// 检测大小写按键/CapsLK
	document.addEventListener('keyup', handleKeyPress);
});

// 页面卸载
onUnmounted(() => {
	// 销毁验证码过期计时器
	clearInterval(timer);
	timer = null;

	document.removeEventListener('keyup', handleKeyPress);
});

// 检测大小写按键
const handleKeyPress = (e: KeyboardEvent) => {
	if (e.getModifierState != undefined)
		state.capsLockVisible = e.getModifierState('CapsLock');
};

// 获取验证码
const getCaptcha = async () => {
	if (!state.captchaEnabled) return;

	state.ruleForm.code = '';
	const res = await getAPI(SysAuthApi).apiSysAuthCaptchaGet().then(res => res.data.result);
	state.captchaImage = 'data:text/html;base64,' + res?.img;
	state.expirySeconds = res?.expirySeconds;
	state.ruleForm.codeId = res?.id;
};

// 获取时间
const currentTime = computed(() => {
	return formatAxis(new Date());
});

const onSignIn = async () => {
	ruleFormRef.value.validate(async (valid: boolean) => {
		if (!valid) return false;

		try {
			state.loading.signIn = true;

			// SM2加密密码
			// const keys = SM2.generateKeyPair();
			const publicKey = window.__env__.VITE_SM_PUBLIC_KEY;
			const password = sm2.doEncrypt(state.ruleForm.password, publicKey, 1);

			state.ruleForm.tenantId ??= props.tenantInfo.id ?? props.tenantInfo.list[0]?.value ?? undefined;
			// console.log(state.ruleForm.tenantId);
			const [err, res] = await feature(getAPI(SysAuthApi).apiSysAuthLoginPost({ ...state.ruleForm, password: password } as any));
			if (err) {
				getCaptcha(); // 重新获取验证码
				return;
			}
			if (res.data.result?.accessToken == undefined) {
				getCaptcha(); // 重新获取验证码
				ElMessage.error('登录失败，请检查账号！');
				return;
			}
			await saveTokenAndInitRoutes(res.data.result?.accessToken);
		} finally {
			state.loading.signIn = false;
		}
	});
};

// 处理强制修改密码成功
const handleForceChangeSuccess = async () => {
	// 清除标记
	Local.remove('isForceChangePassword');
	// 强制修改密码成功后，初始化路由并跳转
	await signInSuccess(false, false);
};

// 保持Token并初始化路由
const saveTokenAndInitRoutes = async (accessToken: string | any) => {
	// 缓存token
	Local.set(accessTokenKey, accessToken);
	Session.set('token', accessToken);

	// 添加完动态路由再进行router跳转，否则可能报错 No match found for location with path "/"
	const isNoPower = await initBackEndControlRoutes();
	signInSuccess(isNoPower); // 再执行 signInSuccess
};

// 登录成功后的跳转
const signInSuccess = async (isNoPower: boolean | undefined, isReturn = true) => {
	if (isNoPower) {
		ElMessage.warning('抱歉，您没有登录权限');
		clearTokens(); // 清空Token缓存
	} else {
		// 获取密码策略
		const userInfo = Local.get('userInfo');
		const policyRes = await wmsPasswordPolicyApi.getEnableDetail();
		// 检查是否有历史密码记录
		const pwdHistoryRes = await wmsPasswordPolicyApi.getPasswordHistory();
		let historyList = pwdHistoryRes.data.result || [];
		// 用于强制改变密码后的回调 不在验证是否首次登录
		if (isReturn) {
			policy.value = policyRes.data.result;
			if (policy.value?.forceChangeInitial) {
				if (historyList.length === undefined || historyList.length === 0) {
					// 若用户没有历史密码记录，说明是初次登录，弹出强制修改密码对话框
					openChangePasswordDialog(userInfo);
					return; // 直接返回，等待用户修改密码成功后再跳转
				}
			}
		}
		// 判断是否有期限更改密码 + 密码即将到期提醒

		// 是否超级管理员身份/普通用户
		const isAdmin = userInfo?.accountType === 999;
		const expiryDays = isAdmin ? policy?.value?.adminPasswordExpiryDays : policy?.value?.passwordExpiryDays;

		if (expiryDays > 0) {
			// 获取上次修改密码时间
			const pwdSetTime = new Date(historyList[0]?.createTime).getTime();
			const nowTime = new Date().getTime();
			const diffDays = Math.floor((nowTime - pwdSetTime) / (1000 * 60 * 60 * 24));
			const daysLeft = expiryDays - diffDays;
			
			// 计算密码到期具体日期
			const expiryDate = new Date(pwdSetTime + expiryDays * 24 * 60 * 60 * 1000);
			const formattedDate = `${expiryDate.getFullYear()}年${expiryDate.getMonth() + 1}月${expiryDate.getDate()}日 ${expiryDate.getHours()}时${expiryDate.getMinutes()}分${expiryDate.getSeconds()}秒`;

			if (daysLeft <= 0) {
				ElNotification({
					title: '密码过期提醒',
					message: `您的密码已于${formattedDate}过期，请立即修改密码！`,
					type: 'error',
					duration: 0,
				})
				// 强制修改密码（密码已到期）
				openChangePasswordDialog(userInfo);
				return;
			} else if (daysLeft <= policy.value.expiryNoticeDays) {
				ElNotification({
					title: '重要提醒',
					 message: `
						<div style="line-height: 1.5;">
							<div>您的密码将在 <span style="color: #E6A23C; font-weight: bold;">${daysLeft}天</span> 后过期</div>
							<div style="font-size: 13px; color: #909399; margin-top: 4px;">
								到期时间：${formattedDate}
							</div>
						</div>
					`,
					type: 'primary',
					dangerouslyUseHTMLString: true,
					duration: 0,
				})
			}
		}
		// 初始化登录成功时间问候语
		let currentTimeInfo = currentTime.value;
		// 登录成功，跳到转首页 如果是复制粘贴的路径，非首页/登录页，那么登录成功后重定向到对应的路径中
		if (route.query?.redirect) {
			// 处理特殊情况（启用密码策略时）：如果redirect是根路径，则跳转到默认首页
			if (route.query?.redirect === '/') {
				router.push({
					path: '/dashboard/home',
					query: Object.keys(<string>route.query?.params).length > 0 ? JSON.parse(<string>route.query?.params) : '',
				});
			} else {
				router.push({
					path: <string>route.query?.redirect,
					query: Object.keys(<string>route.query?.params).length > 0 ? JSON.parse(<string>route.query?.params) : '',
				});
			}
		} else {
			router.push('/');
		}
		// 登录成功提示
		const signInText = '欢迎回来！';
		ElMessage.success(`${currentTimeInfo}，${signInText}`);

		// 添加 loading，防止第一次进入界面时出现短暂空白
		NextLoading.start();
	}
};

function openChangePasswordDialog(userInfo: any) {
	// 若用户没有历史密码记录，说明是初次登录，弹出强制修改密码对话框
	Local.set('isForceChangePassword', true);
	state.userInfoForPassword.account = userInfo?.account || '';
	state.userInfoForPassword.accountType = userInfo?.accountType || 0;
	state.forceChangePasswordVisible = true;
	return; // 直接返回，等待用户修改密码成功后再跳转
}

// 打开旋转验证
const openRotateVerify = () => {
	state.rotateVerifyVisible = true;
	state.isPassRotate = false;
	dragRef.value?.reset();
};

// 通过旋转验证
const passRotateVerify = () => {
	state.rotateVerifyVisible = false;
	state.isPassRotate = true;
	onSignIn();
};

// 登录处理
const handleSignIn = () => {
	if (!state.ruleForm.account) {
		accountRef.value?.focus();
	} else if (!state.ruleForm.password) {
		passwordRef.value?.focus();
	} else if (state.captchaEnabled && !state.ruleForm.code) {
		codeRef.value?.focus();
	} else {
		state.secondVerEnabled ? openRotateVerify() : onSignIn();
	}
};

// 导出对象
defineExpose({ saveTokenAndInitRoutes });
</script>

<style lang="scss" scoped>
.dialog-header {
	:deep(.el-dialog) {
		width: unset !important;

		.el-dialog__header {
			display: none;
		}

		.el-dialog__wrapper {
			position: absolute !important;
		}

		.v-modal {
			position: absolute !important;
		}
	}
}

.login-content-form {
	margin-top: 20px;

	@for $i from 0 through 4 {
		.login-animation#{$i} {
			opacity: 0;
			animation-name: error-num;
			animation-duration: 0.5s;
			animation-fill-mode: forwards;
			animation-delay: calc($i/10) + s;
		}
	}

	.login-content-password {
		display: inline-block;
		width: 20px;
		cursor: pointer;

		&:hover {
			color: #909399;
		}
	}

	.login-content-code {
		display: flex;
		align-items: center;
		justify-content: space-around;
		position: relative;

		.login-content-code-img {
			width: 100%;
			height: 40px;
			line-height: 40px;
			background-color: #ffffff;
			border: 1px solid rgb(220, 223, 230);
			cursor: pointer;
			transition: all ease 0.2s;
			border-radius: 4px;
			user-select: none;

			&:hover {
				border-color: #c0c4cc;
				transition: all ease 0.2s;
			}
		}
	}

	.login-content-code-expired {
		@extend .login-content-code;

		&::before {
			content: '验证码已过期';
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			border-radius: 4px;
			background-color: rgba(0, 0, 0, 0.5);
			color: #ffffff;
			text-align: center;
		}
	}

	.login-content-submit {
		width: 100%;
		letter-spacing: 2px;
		font-weight: 300;
		margin-top: 15px;
	}

	.login-msg {
		color: var(--el-text-color-placeholder);
	}
}
</style>