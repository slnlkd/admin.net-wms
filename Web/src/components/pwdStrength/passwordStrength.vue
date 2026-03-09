<template>
  <div class="password-strength">
    <!-- 指示器 -->
    <div class="indicator-container">
      <div
        class="indicator-dot"
        :style="{ left: indicatorLeft + '%', backgroundColor: indicatorColor }"
      ></div>
    </div>

    <!-- 进度条 -->
    <el-progress
      :percentage="strengthScore"
      :color="customColors"
      :stroke-width="10"
      :show-text="false"
      class="animated-progress"
    />

    <!-- 文本提示 -->
    <div class="strength-text">{{ strengthLabel }}</div>

    <!-- 条件列表 -->
    <ul class="password-rules">
      <li 
        v-for="rule in dynamicRules" 
        :key="rule.text" 
        :class="{ 
          pass: rule.passed, 
          required: rule.required,
          error: rule.required && !rule.passed && password?.length > 0
        }"
      >
        <span v-if="rule.required">
          {{ rule.passed ? '✅' : '❌' }}
        </span>
        <span v-else>
          {{ rule.passed ? '✅' : '○' }}
        </span>
        {{ rule.text }}
      </li>
    </ul>

    <!-- 额外验证提示 -->
    <div v-if="validationErrors.length > 0" class="validation-errors">
      <div 
        v-for="error in validationErrors" 
        :key="error" 
        class="error-item"
      >
        ⚠️ {{ error }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, watch } from "vue"

interface PasswordPolicy {
  minLength: number
  adminMinLength: number
  requireCategories: number
  maxConsecutive: number
  requireLowercase: boolean
  requireUppercase: boolean
  requireDigit: boolean
  requireSpecial: boolean
  specialChars: string
  checkUserId: boolean
  checkConsecutive: boolean
  checkCommonPatterns: boolean
  checkRepeatedChars: boolean
}

interface Props {
  password: string
  policy: PasswordPolicy
  isAdmin?: boolean
  userName?: string // 用于检查密码是否包含用户名
}

const props = withDefaults(defineProps<Props>(), {
  isAdmin: false,
  userName: ''
})

const pwd = computed(() => props.password || "")
const policy = computed(() => props.policy)

// 获取最小长度要求
const minLengthRequired = computed(() => 
  props.isAdmin ? policy?.value?.adminMinLength : policy?.value?.minLength
)

// 基础规则检查
const baseRules = computed(() => [
  { 
    text: `至少${minLengthRequired.value}个字符`, 
    passed: pwd.value.length >= minLengthRequired.value,
    required: true
  },
  { 
    text: '包含小写字母', 
    passed: /[a-z]/.test(pwd.value),
    required: policy?.value?.requireLowercase
  },
  { 
    text: '包含大写字母', 
    passed: /[A-Z]/.test(pwd.value),
    required: policy?.value?.requireUppercase
  },
  { 
    text: '包含数字', 
    passed: /\d/.test(pwd.value),
    required: policy?.value?.requireDigit
  },
  { 
    text: `包含特殊字符 (${policy?.value?.specialChars.substring(0, 20)}${policy?.value?.specialChars.length > 20 ? '...' : ''})`, 
    passed: new RegExp(`[${escapeRegExp(policy?.value?.specialChars)}]`).test(pwd.value),
    required: policy?.value?.requireSpecial
  },
])

// 转义特殊字符用于正则表达式
function escapeRegExp(string: string) {
  if(string === undefined){
    return "";
  }
  return string.replace(/[.*+?^${}()|[\]\\-]/g, '\\$&')
}

// 高级验证检查
const validationErrors = computed(() => {
  const errors: string[] = []
  
  if (policy?.value?.checkConsecutive && hasConsecutiveChars(pwd.value, policy?.value?.maxConsecutive)) {
    errors.push(`不能包含超过${policy?.value?.maxConsecutive}个连续相同字符`)
  }
  
  if (policy?.value?.checkUserId && props.userName && 
      pwd.value.toLowerCase().includes(props.userName.toLowerCase())) {
    errors.push('密码不能包含用户名')
  }
  
  if (policy?.value?.checkRepeatedChars && hasRepeatedPatterns(pwd.value)) {
    errors.push('密码包含重复模式')
  }
  
  if (policy?.value?.checkCommonPatterns && isCommonPattern(pwd.value)) {
    errors.push('密码过于常见或简单')
  }
  
  return errors
})

// 检查连续字符
function hasConsecutiveChars(password: string, maxConsecutive: number): boolean {
  let consecutiveCount = 1
  for (let i = 1; i < password.length; i++) {
    if (password[i] === password[i - 1]) {
      consecutiveCount++
      if (consecutiveCount > maxConsecutive) {
        return true
      }
    } else {
      consecutiveCount = 1
    }
  }
  return false
}

// 检查重复模式
function hasRepeatedPatterns(password: string): boolean {
  if (password.length < 4) return false
  for (let i = 0; i < password.length - 3; i++) {
    const pattern = password.substring(i, i + 2)
    if (password.substring(i + 2).includes(pattern)) {
      return true
    }
  }
  return false
}

// 检查常见模式
function isCommonPattern(password: string): boolean {
  const commonPatterns = [
    // === 1. 极简数字序列 (全球最常见) ===
    '123456', '12345678', '123456789', '1234567890',
    '12345', '1234', '123', '1234567',
    '000000', '111111', '222222', '333333', '444444', '555555', '666666', '777777', '888888', '999999',
    '012345', '0123456789',

    // === 2. 键盘布局序列 (QWERTY/数字小键盘) ===
    'qwerty', 'qwertyuiop', 'asdfgh', 'asdfghjkl', 'zxcvbn', 'zxcvbnm',
    '1qaz2wsx', '1qazxsw2', 'zaq12wsx', '!qaz2wsx',
    'qazwsx', 'qwer', 'abc', 'abcd',

    // === 3. 默认/通用密码 ===
    'password', 'password1', 'password123', 'p@ssw0rd', 'p@ssword',
    'admin', 'admin123', 'administrator',
    'guest', 'guest1', 'guest123',
    'test', 'test1', 'test123',
    'temp', 'temp123',
    'default',

    // === 4. 常见英文单词/短语 ===
    'letmein', 'welcome', 'hello', 'hello123',
    'monkey', 'dragon', 'master', 'baseball', 'football', 'soccer',
    'starwars', 'superman', 'batman',
    'iloveyou', 'lovely', 'love', 'princess', 'charlie',
    'sunshine', 'shadow', 'michael', 'jennifer', 'jordan',
    'george', 'michelle', 'andrew', 'tigger',

    // === 5. 中文环境常见密码 ===
    // 5.1 拼音
    'woaini', 'aini', 'iloveyou', // "我爱你"
    'zhanghao', 'yonghu', 'yonghuming', // "账号", "用户", "用户名"
    'mima', 'mimamima', // "密码", "密码密码"
    'wode', 'wodemima', // "我的", "我的密码"
    'admin', 'guanliyuan', // "管理员"
    'wangji', 'wanglimima', // "忘记", "忘记密码"
    'qwert', 'qazwsx', // 键盘布局，但在中文输入法下也常用
    'abc123', 'abcd1234', // 简单字母数字组合

    // 5.2 数字谐音/特定含义数字
    '1314520', '5201314', // "一生一世我爱你", "我爱你一生一世"
    '7758258', // "亲亲我吧爱我吧"
    '9420', // "就是爱你"
    '1392010', // "一生就爱你一人"
    '168168', // "一路发"
    '888888', '666666', // 发财、顺利
    '112233', '445566', // 简单重复对
    '123123',

    // 5.3 简单中文词汇转数字/字母
    'admin123', 'root123', // 管理员+数字
    'passwd', 'pass', // 密码的英文简写
    '123abc', 'abc123', // 常见组合
    '1q2w3e', '1q2w3e4r', // 键盘交替
    'a123456', 'a123456789', // 字母开头+数字序列
    'aa123456', // 重复字母+数字

    // === 6. 公司/产品/系统相关 ===
    'companyname', 'company123',
    'welcome1', 'welcome123',
    'system', 'system1', 'system123',
    'root', 'root123', 'toor', // toor是root的倒写

    // === 7. 季节/月份/星期 ===
    'spring', 'summer', 'autumn', 'winter',
    'monday', 'tuesday', 'january', 'february',

    // === 8. 运动/球队 ===
    'baseball', 'football', 'soccer', 'yankees', 'lakers',

    // === 9. 其他极简模式 ===
    'abc', 'abcd', 'abc123', 'aaa', 'aaa111', 'aaa123',
    '123abc', '123qwe',
    'pass', 'pass1', 'pass123',
    'pw', 'pw1', 'pw123',
    'ok', 'ok123', 'okokok',
    'hi', 'hey',

    // === 10. 带简单符号的弱密码 ===
    'password!', 'password1!', 'admin!', 'admin123!',
    '123456!', '123456789!',
    'qwerty!', 'abc123!'
];
  return commonPatterns.includes(password.toLowerCase())
}

// 动态规则列表
const dynamicRules = computed(() => {
  return baseRules.value?.filter(rule => rule.required)
})

// 计算已满足的类别数量
const satisfiedCategories = computed(() => {
  return baseRules.value?.filter(rule => rule.passed && rule.required).length
})

// 计算强度分数 - 基于策略要求
const strengthScore = computed(() => {
  if (!pwd.value) return 0
  
  let score = 0
  const maxScore = 100
  
  // 长度得分 (30分)
  const lengthScore = Math.min((pwd.value.length / minLengthRequired.value) * 30, 30)
  score += lengthScore
  
  // 字符类别得分 (40分)
  const requiredCategories = [
    policy?.value?.requireLowercase,
    policy?.value?.requireUppercase, 
    policy?.value?.requireDigit,
    policy?.value?.requireSpecial
  ].filter(Boolean).length
  
  const categoryScore = requiredCategories > 0 
    ? (satisfiedCategories.value / requiredCategories) * 40 
    : 0
  score += categoryScore
  
  // 复杂度得分 (30分)
  let complexityScore = 0
  if (pwd.value.length >= minLengthRequired.value + 4) complexityScore += 10
  if (pwd.value.length >= minLengthRequired.value + 8) complexityScore += 10
  if (satisfiedCategories.value >= policy?.value?.requireCategories + 1) complexityScore += 10
  
  score += complexityScore
  
  // 如果有验证错误，扣分
  if (validationErrors.value.length > 0) {
    score = Math.max(0, score - (validationErrors.value.length * 15))
  }
  
  return Math.min(Math.round(score), maxScore)
})

// 进度条颜色分段
const customColors = [
  { color: "#F56C6C", percentage: 40 }, // 红
  { color: "#E6A23C", percentage: 80 }, // 橙
  { color: "#67C23A", percentage: 100 } // 绿
]

// 强度标签
const strengthLabel = computed(() => {
  const score = strengthScore.value
  if (score === 0) return "请输入密码"
  if (score <= 40) return "弱"
  if (score <= 80) return "中"
  if (score <= 95) return "强"
  return "非常强"
})

// 圆点位置 (0% - 100%)
const indicatorLeft = computed(() => Math.min(strengthScore.value, 100))

// 圆点颜色（渐变）
const indicatorColor = computed(() => {
  const score = strengthScore.value
  if (score <= 40) return "#F56C6C"
  if (score <= 80) {
    const ratio = (score - 40) / 40
    const r = Math.round(245 + (230 - 245) * ratio)
    const g = Math.round(108 + (162 - 108) * ratio)
    const b = Math.round(108 + (60 - 108) * ratio)
    return `rgb(${r},${g},${b})`
  }
  const ratio = (score - 80) / 20
  const r = Math.round(230 + (103 - 230) * ratio)
  const g = Math.round(162 + (199 - 162) * ratio)
  const b = Math.round(60 + (58 - 60) * ratio)
  return `rgb(${r},${g},${b})`
})

const emit = defineEmits<{
  (e: "update:canSubmit", value: boolean): void
}>()

// 计算密码是否可以提交 - 基于策略要求
const canSubmit = computed(() => {
  // 没有策略 随便提交
  if(dynamicRules.value.length == undefined){
    return true;
  }
  // 检查所有必填规则
  const allRequiredPassed = dynamicRules?.value?.every(rule => rule.passed)
  
  // 检查类别数量要求
  const categoryRequirementMet = satisfiedCategories?.value >= policy?.value?.requireCategories
  
  // 检查没有验证错误
  const noValidationErrors = validationErrors?.value?.length === 0
  
  return allRequiredPassed && categoryRequirementMet && noValidationErrors
})

// 监听 canSubmit 变化，通知父组件
watch(canSubmit, (val) => {
  emit("update:canSubmit", val)
}, { immediate: true })
</script>

<style scoped>
.password-strength {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

/* 指示器 */
.indicator-container {
  position: relative;
  height: 10px;
  bottom: 13.6px;
}

.el-progress {
    align-items: center;
    display: flex;
    line-height: 1;
    position: relative;
    bottom: 20px;
}

.indicator-dot {
  position: absolute;
  top: 0;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  transform: translateX(-50%);
  transition: left 0.5s ease, background-color 0.5s ease;
  animation: bounce 1s infinite alternate;
}

/* 圆点微跳动画 */
@keyframes bounce {
  0% {
    transform: translateX(-50%) translateY(0);
  }
  100% {
    transform: translateX(-50%) translateY(-4px);
  }
}

/* 进度条平滑过渡 */
.animated-progress >>> .el-progress-bar__outer {
  transition: width 0.5s ease;
}

/* 文本提示 */
.strength-text {
  font-size: 14px;
  color: #606266;
  position: relative;
  bottom: 10px;
}

/* 条件列表 */
.password-rules {
  margin: 0;
  padding: 0;
  list-style: none;
  font-size: 13px;
}
.password-rules li {
  display: flex;
  align-items: center;
  gap: 4px;
  margin-bottom: 4px;
}
.password-rules li.pass {
  color: #67C23A;
}
.password-rules li.required.error {
  color: #F56C6C;
  font-weight: bold;
}
.password-rules li:not(.required) {
  color: #909399;
}

/* 验证错误提示 */
.validation-errors {
  margin-top: 8px;
  padding: 8px;
  background-color: #fef0f0;
  border: 1px solid #fbc4c4;
  border-radius: 4px;
}
.error-item {
  font-size: 12px;
  color: #F56C6C;
  margin-bottom: 4px;
}
</style>