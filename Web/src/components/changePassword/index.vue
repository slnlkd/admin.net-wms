<template>
  <div class="change-password-container">
    <el-form ref="ruleFormPasswordRef" :model="formData" :rules="rules" label-width="auto">
      <el-form-item v-if="requireOldPassword" label="当前密码" prop="passwordOld">
        <el-input v-model="formData.passwordOld" maxlength="32" type="password" autocomplete="off" show-password
          placeholder="请输入当前密码" />
      </el-form-item>

      <el-form-item label="新密码" prop="passwordNew">
        <el-input v-model="formData.passwordNew" maxlength="32" type="password" autocomplete="off" show-password
          placeholder="请输入新密码" />
      </el-form-item>

      <el-form-item label="确认密码" prop="passwordNew2">
        <el-input v-model="formData.passwordNew2" maxlength="32" type="password" autocomplete="off" show-password
          placeholder="请再次输入新密码" />
      </el-form-item>

      <el-form-item v-if="pwdPolicy" label="密码强度">
        <PasswordStrength :userName="userName" :isAdmin="isAdmin" :policy="pwdPolicy"
          @update:canSubmit="handleCanSubmit" :password="formData.passwordNew"
          style="width: 100%; padding-top: 10px;" />
      </el-form-item>

      <el-form-item>
        <el-button icon="ele-Refresh" @click="resetPassword">重 置</el-button>
        <el-button v-if="!pwdPolicy" icon="ele-SuccessFilled" type="primary" @click="submitPassword" :loading="loading">
          确 定
        </el-button>
        <el-button v-if="pwdPolicy" icon="ele-SuccessFilled" :disabled="!canSubmit" type="primary"
          @click="submitPassword" :loading="loading">
          确 定
        </el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, computed, onMounted } from 'vue';
import { ElForm, ElMessage, ElMessageBox } from 'element-plus';
import type { FormRules } from 'element-plus';
import { sm2 } from 'sm-crypto-v2';
import { getAPI } from '/@/utils/axios-utils';
import { SysUserApi, SysAuthApi } from '/@/api-services/api';
import { ChangePwdInput } from '/@/api-services/models';
import PasswordStrength from '/@/components/pwdStrength/passwordStrength.vue';
import { useWmsPasswordPolicyApi } from '/@/api/system/pwdPolicy/wmsPasswordPolicy';
import { accessTokenKey, clearAccessAfterReload } from '/@/utils/axios-utils';
import { Local } from '/@/utils/storage';

interface Props {
  // 是否要求输入旧密码（强制修改密码时不需要）
  requireOldPassword?: boolean;
  // 老密码（首次强制，免用户手输）
  oldPwd?: string;
  // 用户名（用于密码策略检查）
  userName?: string;
  // 是否是管理员账户
  isAdmin?: boolean;
  // 是否显示权限控制
  showAuth?: boolean;
  // 修改成功后是否自动重新登录
  autoRelogin?: boolean;
  // 自定义成功提示
  successMessage?: string;
  // 自定义成功回调
  onSuccess?: () => void;
}

const props = withDefaults(defineProps<Props>(), {
  requireOldPassword: true,
  // 老密码（首次强制，免用户手输）
  oldPwd: '',
  userName: '',
  isAdmin: false,
  showAuth: false,
  autoRelogin: true,
  successMessage: '密码修改成功',
});

const emit = defineEmits<{
  success: [];
  cancel: [];
}>();

// 响应式数据
const ruleFormPasswordRef = ref<InstanceType<typeof ElForm>>();
const loading = ref(false);
const canSubmit = ref(false);
const pwdPolicy = ref();

// 使用统一的数据对象，确保表单验证能正确绑定
const formData = reactive({
  passwordOld: '',
  passwordNew: '',
  passwordNew2: ''
});

// 表单验证规则
const rules = computed<FormRules>(() => ({
  passwordOld: props.requireOldPassword ? [
    { required: true, message: '当前密码不能为空', trigger: 'blur' }
  ] : [],
  passwordNew: [
    { required: true, message: '新密码不能为空', trigger: 'blur' }
  ],
  passwordNew2: [
    { required: true, message: '确认密码不能为空', trigger: 'blur' },
    {
      validator: (rule: any, value: any, callback: any) => {
        if (!value) {
          callback(new Error('请确认新密码'));
        } else if (value !== formData.passwordNew) {
          callback(new Error('两次密码不一致！'));
        } else {
          callback();
        }
      },
      trigger: 'blur'
    }
  ]
}));

// 密码策略 API
const wmsPasswordPolicyApi = useWmsPasswordPolicyApi();

onMounted(async () => {
  // 获取密码策略
  try {
    const policyRes = await wmsPasswordPolicyApi.getEnableDetail();
    pwdPolicy.value = policyRes.data.result;
  } catch (error) {
    console.error('获取密码策略失败:', error);
  }
});

// 密码强度检查回调
const handleCanSubmit = (val: boolean) => {
  canSubmit.value = val;
};

// 重置表单
const resetPassword = () => {
  formData.passwordOld = '';
  formData.passwordNew = '';
  formData.passwordNew2 = '';
  ruleFormPasswordRef.value?.clearValidate();
  canSubmit.value = false;
};

// 提交修改密码
const submitPassword = async () => {
  if (!ruleFormPasswordRef.value) return;

  try {
    const valid = await ruleFormPasswordRef.value.validate();
    if (!valid) return;

    // 再次确认密码一致性
    if (formData.passwordNew !== formData.passwordNew2) {
      ElMessage.error('两次输入的密码不一致');
      return;
    }

    loading.value = true;

    // SM2加密密码
    const cpwd: ChangePwdInput = { passwordOld: '', passwordNew: '' };
    const publicKey = window.__env__.VITE_SM_PUBLIC_KEY;

    if (props.requireOldPassword) {
      cpwd.passwordOld = sm2.doEncrypt(formData.passwordOld, publicKey, 1);
    } else {
      // 不需要旧密码时，传空字符串
      cpwd.passwordOld = sm2.doEncrypt(props.oldPwd, publicKey, 1);
    }
    cpwd.passwordNew = sm2.doEncrypt(formData.passwordNew, publicKey, 1);

    // 调用API修改密码
    await getAPI(SysUserApi).apiSysUserChangePwdPost(cpwd);

    ElMessage.success(props.successMessage);

    // 触发成功事件
    emit('success');

    // 执行自定义成功回调
    if (props.onSuccess) {
      props.onSuccess();
    } else if (props.autoRelogin) {
      // 默认行为：重新登录
      await handleRelogin();
    }

    // 重置表单
    resetPassword();

  } catch (error: any) {
    console.error('修改密码失败:', error);
    ElMessage.error(error.message || '修改密码失败');
  } finally {
    loading.value = false;
  }
};

// 处理重新登录
const handleRelogin = async () => {
  if (props.requireOldPassword) {
    // 需要旧密码的情况（用户主动修改），提示重新登录
    ElMessageBox.confirm('密码已修改，是否重新登录系统？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    }).then(() => {
      clearAccessAfterReload();
    });
  } else {
    // 强制修改密码的情况，直接刷新token并重定向
    try {
      const accessToken = Local.get(accessTokenKey);
      await getAPI(SysAuthApi).apiSysAuthRefreshTokenGet(`${accessToken}`);
      window.location.reload();
    } catch (error) {
      console.error('刷新token失败:', error);
      clearAccessAfterReload();
    }
  }
};

// 暴露方法给父组件
defineExpose({
  resetPassword,
  submitPassword
});
</script>

<style lang="scss" scoped>
.change-password-container {
  padding: 20px 0;

  :deep(.el-form-item) {
    margin-bottom: 22px;
  }

  :deep(.el-button) {
    min-width: 100px;
  }
}
</style>