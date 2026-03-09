<script lang="ts" name="wmsPasswordPolicy" setup>
import { ref, reactive, onMounted, computed, watch } from "vue";
import { ElMessage } from "element-plus";
import type { FormRules } from "element-plus";
import { formatDate } from '/@/utils/formatTime';
import { useWmsPasswordPolicyApi } from '/@/api/system/pwdPolicy/wmsPasswordPolicy';

//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const wmsPasswordPolicyApi = useWmsPasswordPolicyApi();
const ruleFormRef = ref();

const state = reactive({
	title: '',
	loading: false,
	showDialog: false,
	ruleForm: {} as any,
});

// 计算属性：密码复杂度要求数量
const complexityCount = computed(() => {
  let count = 0;
  if (state.ruleForm.requireLowercase) count++;
  if (state.ruleForm.requireUppercase) count++;
  if (state.ruleForm.requireDigit) count++;
  if (state.ruleForm.requireSpecial) count++;
  return count;
});

// 监听复杂度要求变化，自动更新必须字符类别数
watch(() => [
  state.ruleForm.requireLowercase,
  state.ruleForm.requireUppercase,
  state.ruleForm.requireDigit,
  state.ruleForm.requireSpecial
], () => {
  state.ruleForm.requireCategories = complexityCount.value;
});

// 表单验证规则
const rules = ref<FormRules>({
	name: [{ required: true, message: '请输入策略名称！', trigger: 'blur' }],
	minLength: [
    { required: true, message: '请输入普通用户密码最小长度！', trigger: 'blur' },
    { type: 'number', min: 6, max: 32, message: '长度范围6-32位', trigger: 'blur' }
  ],
	adminMinLength: [
    { required: true, message: '请输入管理员密码最小长度！', trigger: 'blur' },
    { type: 'number', min: 8, max: 32, message: '长度范围8-32位', trigger: 'blur' }
  ],
	requireCategories: [
    { required: true, message: '需要包含的字符类别数量不能为空！', trigger: 'change' },
    { type: 'number', min: 1, max: 4, message: '类别数量范围1-4', trigger: 'change' }
  ],
	maxConsecutive: [
    { required: true, message: '请输入最大连续相同字符数！', trigger: 'blur' },
    { type: 'number', min: 1, max: 10, message: '范围1-10', trigger: 'blur' }
  ],
	specialChars: [{ required: true, message: '请输入允许的特殊字符集合！', trigger: 'blur' }],
	rememberPasswords: [
    { required: true, message: '请输入密码历史记录数量！', trigger: 'blur' },
    { type: 'number', min: 0, max: 20, message: '范围0-20', trigger: 'blur' }
  ],
	passwordExpiryDays: [
    { required: true, message: '请输入普通用户密码过期天数！', trigger: 'blur' },
    { type: 'number', min: 0, max: 365, message: '范围0-365天', trigger: 'blur' }
  ],
	adminPasswordExpiryDays: [
    { required: true, message: '请输入管理员密码过期天数！', trigger: 'blur' },
    { type: 'number', min: 0, max: 365, message: '范围0-365天', trigger: 'blur' }
  ],
	expiryNoticeDays: [
    { required: true, message: '请输入密码过期提前提醒天数！', trigger: 'blur' },
    { type: 'number', min: 0, max: 30, message: '范围0-30天', trigger: 'blur' }
  ],
});

// 默认特殊字符集
const defaultSpecialChars = '(!@#$%^&*()_+-=[]{};\':"|,.<>/?\')';

// 打开弹窗
const openDialog = async (row: any, title: string) => {
	state.title = title;
	const defaultData = {
		minLength: 8,
		adminMinLength: 14,
		requireCategories: 3, // 默认值
		maxConsecutive: 3,
		requireLowercase: true,
		requireUppercase: true,
		requireDigit: true,
		requireSpecial: false,
		specialChars: defaultSpecialChars,
		checkUserId: true,
		checkConsecutive: true,
		checkCommonPatterns: true,
		checkRepeatedChars: false,
		rememberPasswords: 5,
		passwordExpiryDays: 90,
		adminPasswordExpiryDays: 90,
		expiryNoticeDays: 7,
		forceChangeInitial: true,
		enabled: true,
	};
	
	if (row?.id) {
		state.ruleForm = await wmsPasswordPolicyApi.detail(row.id).then(res => res.data.result);
	} else {
		state.ruleForm = JSON.parse(JSON.stringify({ ...defaultData, ...row }));
	}
	
	// 确保打开时根据勾选状态更新类别数量
	state.ruleForm.requireCategories = complexityCount.value;
	
	state.showDialog = true;
};

// 关闭弹窗
const closeDialog = () => {
	emit("reloadTable");
	state.showDialog = false;
};

// 重置特殊字符为默认值
const resetSpecialChars = () => {
	state.ruleForm.specialChars = defaultSpecialChars;
};

// 提交
const submit = async () => {
	ruleFormRef.value.validate(async (isValid: boolean, fields?: any) => {
		if (isValid) {
			try {
				state.loading = true;
				const values = { ...state.ruleForm };
				await wmsPasswordPolicyApi[state.ruleForm.id ? 'update' : 'add'](values);
				ElMessage.success(state.ruleForm.id ? '修改成功' : '新增成功');
				closeDialog();
			} catch (error) {
				// 错误处理由API层统一处理
			} finally {
				state.loading = false;
			}
		} else {
			ElMessage.error(`表单有${Object.keys(fields).length}处验证失败，请修改后再提交`);
		}
	});
};

//将属性或者函数暴露给父组件
defineExpose({ openDialog });
</script>

<template>
	<div class="wmsPasswordPolicy-container">
		<el-dialog 
			v-model="state.showDialog" 
			:width="1400" 
			draggable 
			:close-on-click-modal="false"
			:close-on-press-escape="false"
		>
			<template #header>
				<div style="color: #fff; display: flex; align-items: center;">
					<el-icon style="margin-right: 8px;"><Lock /></el-icon>
					<span>{{ state.title }}</span>
				</div>
			</template>
			
			<el-form 
				:model="state.ruleForm" 
				ref="ruleFormRef" 
				label-width="180px" 
				:rules="rules"
				:disabled="state.loading"
			>
				<el-scrollbar max-height="70vh">
					<el-row :gutter="24" style="width: 99%;">
						<!-- 基础信息 -->
						<el-col :span="24">
							<el-divider content-position="left">基础信息</el-divider>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="策略名称" prop="name">
								<el-input 
									v-model="state.ruleForm.name" 
									placeholder="请输入策略名称" 
									maxlength="100"
									show-word-limit 
									clearable 
								/>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="策略描述" prop="description">
								<el-input 
									v-model="state.ruleForm.description" 
									placeholder="请输入策略描述" 
									maxlength="500"
									show-word-limit 
									clearable 
								/>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="策略状态" prop="enabled">
                <template #label>
                  <span>策略状态</span>
                  <el-tooltip content="启用后此策略将生效，系统只能有一个启用的密码策略" placement="top">
                    <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
                  </el-tooltip>
                </template>
								<el-switch
									v-model="state.ruleForm.enabled"
									active-text="启用"
									inactive-text="停用"
								/>
							</el-form-item>
						</el-col>

						<!-- 密码长度要求 -->
						<el-col :span="24">
							<el-divider content-position="left">密码长度要求</el-divider>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="普通用户最小长度" prop="minLength">
                <template #label>
                  <span>普通用户最小长度</span>
                  <el-tooltip content="普通用户账户密码的最小长度要求" placement="top">
                    <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
                  </el-tooltip>
                </template>
								<el-input-number 
									v-model="state.ruleForm.minLength" 
									:min="6" 
									:max="32"
									placeholder="6-32位" 
									style="width: 100%"
								/>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="管理员最小长度" prop="adminMinLength">
                <template #label>
                  <span>管理员最小长度</span>
                  <el-tooltip content="管理员账户密码的最小长度要求，通常比普通用户要求更高" placement="top">
                    <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
                  </el-tooltip>
                </template>
								<el-input-number 
									v-model="state.ruleForm.adminMinLength" 
									:min="8" 
									:max="32"
									placeholder="8-32位" 
									style="width: 100%"
								/>
							</el-form-item>
						</el-col>

						<!-- 密码复杂度 -->
						<el-col :span="24">
							<el-divider content-position="left">密码复杂度要求</el-divider>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="必须字符类别数" prop="requireCategories">
								<div class="category-display">
                  <el-tag type="info" size="large">
                    {{ state.ruleForm.requireCategories }} 类
                  </el-tag>
                  <span class="category-hint">（根据下方勾选自动计算）</span>
                </div>
                <!-- 隐藏的输入框用于表单验证 -->
                <el-input-number 
                  v-show="false"
                  v-model="state.ruleForm.requireCategories" 
                  :min="1" 
                  :max="4"
                />
								<template #label>
									<span>必须字符类别数</span>
									<el-tooltip content="根据下面勾选的字符类型自动计算，最少1类，最多4类" placement="top">
										<el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
									</el-tooltip>
								</template>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="最大连续字符数" prop="maxConsecutive">
                <template #label>
                  <span>最大连续字符数</span>
                  <el-tooltip content="允许的最大连续字符数量（如1111、aaaa等），防止使用过于简单的连续字符" placement="top">
                    <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
                  </el-tooltip>
                </template>
								<el-input-number 
									v-model="state.ruleForm.maxConsecutive" 
									:min="1" 
									:max="10"
									placeholder="1-10个" 
									style="width: 100%"
								/>
							</el-form-item>
						</el-col>

						<!-- 字符类型要求 -->
						<el-col :span="24" class="mb20">
							<el-form-item label="必须包含字符类型">
                <template #label>
                  <span>必须包含字符类型</span>
                  <el-tooltip content="密码中必须包含的字符类型组合，至少选择一种类型" placement="top">
                    <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
                  </el-tooltip>
                </template>
								<el-space wrap>
									<el-checkbox v-model="state.ruleForm.requireLowercase">小写字母(a-z)</el-checkbox>
									<el-checkbox v-model="state.ruleForm.requireUppercase">大写字母(A-Z)</el-checkbox>
									<el-checkbox v-model="state.ruleForm.requireDigit">数字(0-9)</el-checkbox>
									<el-checkbox v-model="state.ruleForm.requireSpecial">特殊字符</el-checkbox>
								</el-space>
								<div class="category-tip">
                  当前已选择 {{ complexityCount }} 类字符类型
                </div>
							</el-form-item>
						</el-col>

						<!-- 特殊字符设置 -->
						<el-col :span="24" v-if="state.ruleForm.requireSpecial" class="mb20">
							<el-form-item label="允许的特殊字符" prop="specialChars">
                <template #label>
                  <span>允许的特殊字符</span>
                  <el-tooltip content="定义哪些特殊字符可以在密码中使用，如!@#$%等" placement="top">
                    <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
                  </el-tooltip>
                </template>
								<div style="display: flex; gap: 8px;">
									<el-input 
										v-model="state.ruleForm.specialChars" 
										placeholder="请输入允许的特殊字符集合" 
										maxlength="100"
										show-word-limit 
										clearable 
										style="flex: 1;"
									/>
									<el-button @click="resetSpecialChars" type="primary" link>
										恢复默认
									</el-button>
								</div>
							</el-form-item>
						</el-col>

						<!-- 安全检查 -->
						<el-col :span="24">
							<el-divider content-position="left">安全检查</el-divider>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="不能包含用户名" prop="checkUserId">
                <template #label>
                  <span>不能包含用户名</span>
                  <el-tooltip content="密码中不能包含用户的登录用户名" placement="top">
                    <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
                  </el-tooltip>
                </template>
								<el-switch v-model="state.ruleForm.checkUserId" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="检查连续字符" prop="checkConsecutive">
                <template #label>
                  <span>检查连续字符</span>
                  <el-tooltip content="检查密码中是否包含连续字符序列（如1111、aaaa等）" placement="top">
                    <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
                  </el-tooltip>
                </template>
								<el-switch v-model="state.ruleForm.checkConsecutive" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="检查常见模式" prop="checkCommonPatterns">
                <template #label>
                  <span>检查常见模式</span>
                  <el-tooltip content="检查是否使用常见密码模式（如123456、qwerty、password等）" placement="top">
                    <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
                  </el-tooltip>
                </template>
								<el-switch v-model="state.ruleForm.checkCommonPatterns" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="检查重复字符" prop="checkRepeatedChars">
                <template #label>
                  <span>检查重复字符</span>
                  <el-tooltip content="检查密码中是否包含过多重复字符（如abcd、1234等）再次重复" placement="top">
                    <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
                  </el-tooltip>
                </template>
								<el-switch v-model="state.ruleForm.checkRepeatedChars" />
							</el-form-item>
						</el-col>

						<!-- 密码策略 -->
						<el-col :span="24">
							<el-divider content-position="left">密码策略</el-divider>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="历史密码记录数" prop="rememberPasswords">
                <template #label>
                  <span>历史密码记录数</span>
                  <el-tooltip content="系统记住的用户历史密码数量，用户不能重复使用这些密码" placement="top">
                    <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
                  </el-tooltip>
                </template>
								<el-input-number 
									v-model="state.ruleForm.rememberPasswords" 
									:min="0" 
									:max="20"
									placeholder="0-20个" 
									style="width: 100%"
								/>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="强制首次修改" prop="forceChangeInitial">
                <template #label>
                  <span>强制首次修改</span>
                  <el-tooltip content="用户首次登录或管理员重置密码后，是否强制要求用户修改密码" placement="top">
                    <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
                  </el-tooltip>
                </template>
								<el-switch v-model="state.ruleForm.forceChangeInitial" />
							</el-form-item>
						</el-col>

						<!-- 密码有效期 -->
						<el-col :span="24">
							<el-divider content-position="left">密码有效期</el-divider>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="普通用户过期天数" prop="passwordExpiryDays">
								<el-input-number 
									v-model="state.ruleForm.passwordExpiryDays" 
									:min="0" 
									:max="365"
									placeholder="0-365天" 
									style="width: 100%"
								/>
								<template #label>
									<span>普通用户过期天数</span>
									<el-tooltip content="0表示永不过期" placement="top">
										<el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
									</el-tooltip>
								</template>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="管理员过期天数" prop="adminPasswordExpiryDays">
                <template #label>
                  <span>管理员过期天数</span>
                  <el-tooltip content="管理员密码过期天数，0表示永不过期，通常比普通用户要求更严格" placement="top">
                    <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
                  </el-tooltip>
                </template>
								<el-input-number 
									v-model="state.ruleForm.adminPasswordExpiryDays" 
									:min="0" 
									:max="365"
									placeholder="0-365天" 
									style="width: 100%"
								/>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="过期提醒天数" prop="expiryNoticeDays">
                <template #label>
                  <span>过期提醒天数</span>
                  <el-tooltip content="密码过期前多少天开始提醒用户修改密码" placement="top">
                    <el-icon style="margin-left: 4px;"><QuestionFilled /></el-icon>
                  </el-tooltip>
                </template>
								<el-input-number 
									v-model="state.ruleForm.expiryNoticeDays" 
									:min="0" 
									:max="30"
									placeholder="0-30天" 
									style="width: 100%"
								/>
							</el-form-item>
						</el-col>
					</el-row>
				</el-scrollbar>
			</el-form>
			
			<template #footer>
				<span class="dialog-footer">
					<el-button @click="() => state.showDialog = false" :disabled="state.loading">取消</el-button>
					<el-button 
						@click="submit" 
						type="primary" 
						:loading="state.loading"
						v-reclick="1000"
					>
						{{ state.loading ? '提交中...' : '确定' }}
					</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>

<style lang="scss" scoped>
.wmsPasswordPolicy-container {
	:deep(.el-divider__text) {
		font-size: 14px;
		font-weight: 600;
		color: #409eff;
	}
	
	:deep(.el-form-item__label) {
		display: flex;
		align-items: center;
	}
	
	:deep(.el-select),
	:deep(.el-input-number) {
		width: 100%;
	}

  .category-display {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 8px 0;
  }

  .category-hint {
    font-size: 12px;
    color: #909399;
  }

  .category-tip {
    margin-top: 8px;
    font-size: 12px;
    color: #409eff;
    background: #f0f9ff;
    padding: 4px 8px;
    border-radius: 4px;
    display: inline-block;
  }
}

.dialog-footer {
	display: flex;
	justify-content: flex-end;
	gap: 12px;
}
</style>