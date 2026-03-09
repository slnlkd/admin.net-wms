<template>
    <el-dialog v-model="visible" :title="title" width="500px" :close-on-click-modal="false"
        :close-on-press-escape="false" :show-close="false" destroy-on-close>
        <div class="force-change-password-dialog">
            <div class="password-tip" v-if="isForceChange">
                <el-alert title="为了您的账户安全，请立即修改密码" type="warning" :closable="false" show-icon />
            </div>

            <ChangePassword ref="changePasswordRef" :user-name="userName" :is-admin="isAdmin" :old-pwd="oldPwd"
                :require-old-password="!isForceChange" :auto-relogin="true" :success-message="successMessage"
                @success="handleSuccess" />
        </div>
    </el-dialog>
</template>

<script lang="ts" setup>
import { ref, computed, watch, onMounted, onBeforeUnmount, onUnmounted } from 'vue';
import { ElMessage } from 'element-plus';
import ChangePassword from '/@/components/ChangePassword/index.vue';
import { Local, Session } from '/@/utils/storage';
import { accessTokenKey, refreshAccessTokenKey } from '/@/utils/axios-utils';
import { onBeforeRouteLeave } from 'vue-router';

interface Props {
    modelValue: boolean;
    userName: string;
    isAdmin?: boolean;
    // 是否是强制修改密码（初次登录）
    isForceChange?: boolean;
    // 老密码（首次强制，免用户手输）
    oldPwd?: string;
    title?: string;
    successMessage?: string;
}

const props = withDefaults(defineProps<Props>(), {
    modelValue: false,
    userName: '',
    isAdmin: false,
    isForceChange: false,
    oldPwd: '',
    title: '修改密码',
    successMessage: '密码修改成功',
});

const emit = defineEmits<{
    'update:modelValue': [value: boolean];
    success: [];
    close: [];
}>();

const changePasswordRef = ref<InstanceType<typeof ChangePassword>>();
const visible = ref(props.modelValue);

const handleBeforeUnload = () => {
    const isFirst = Local.get('isForceChangePassword');
    if(isFirst){
        // 执行清理操作
        console.log("用户选择不修改密码，清除登录状态");
        Local.remove(accessTokenKey);
        Local.remove(refreshAccessTokenKey);
        Session.clear();
        window.location.reload();
    }

}
onMounted(() => {
    window.addEventListener('beforeunload', handleBeforeUnload)
})

// Vue Router 导航守卫
  onBeforeRouteLeave(() => {
    console.log("路由离开，清除登录状态");
    handleBeforeUnload();
  })

onUnmounted(() => {
    window.removeEventListener('beforeunload', handleBeforeUnload)
})

// 计算标题
const title = computed(() => {
    return props.isForceChange ? '强制修改密码' : props.title;
});

// 监听visible变化
watch(visible, (newVal) => {
    emit('update:modelValue', newVal);
});

// 监听props.modelValue变化
watch(() => props.modelValue, (newVal) => {
    visible.value = newVal;
});

// 处理修改成功
const handleSuccess = () => {
    emit('success');
    visible.value = false;
};

// 暴露方法给父组件
defineExpose({
    reset: () => {
        changePasswordRef.value?.resetPassword();
    },
    close: () => {
        visible.value = false;
    }
});
</script>

<style lang="scss" scoped>
.force-change-password-dialog {
    .password-tip {
        margin-bottom: 20px;
    }

    :deep(.el-dialog__header) {
        background: #f5f7fa;
        border-bottom: 1px solid #e4e7ed;
        margin-right: 0;

        .el-dialog__title {
            color: #303133;
            font-weight: 600;
        }
    }
}
</style>