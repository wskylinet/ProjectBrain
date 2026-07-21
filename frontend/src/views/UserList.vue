<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useAuthStore } from '@/stores/auth'
import { createUser, deleteUser, getRoles, getUsers, resetUserPassword, updateUser, type RoleOption, type User } from '@/api/users'

const auth = useAuthStore()
const loading = ref(false), saving = ref(false), editVisible = ref(false), passwordVisible = ref(false)
const items = ref<User[]>([]), roles = ref<RoleOption[]>([]), total = ref(0), editingId = ref<number>(), passwordUser = ref<User>()
const statusFilter = ref<'' | boolean>('')
const query = reactive({ keyword: '', page: 1, pageSize: 10 })
const form = reactive({ userName: '', nickName: '', password: '', isEnabled: true, roleIds: [] as number[] })
const passwordForm = reactive({ password: '', confirmPassword: '' })
const canManage = computed(() => auth.hasPermission('user:manage'))

async function load() {
  loading.value = true
  try {
    const [result, roleOptions] = await Promise.all([
      getUsers({ ...query, isEnabled: statusFilter.value === '' ? undefined : statusFilter.value }),
      getRoles()
    ])
    items.value = result.items; total.value = result.total; roles.value = roleOptions
  } finally { loading.value = false }
}
function search() { query.page = 1; load() }
function openCreate() {
  editingId.value = undefined
  const reader = roles.value.find(x => x.code === 'Reader')
  Object.assign(form, { userName: '', nickName: '', password: '', isEnabled: true, roleIds: reader ? [reader.id] : [] })
  editVisible.value = true
}
function openEdit(user: User) {
  editingId.value = user.id
  Object.assign(form, { userName: user.userName, nickName: user.nickName || '', password: '', isEnabled: user.isEnabled, roleIds: [...user.roleIds] })
  editVisible.value = true
}
async function save() {
  if (!editingId.value && form.userName.trim().length < 2) return ElMessage.warning('登录账号至少 2 个字符')
  if (!editingId.value && form.password.length < 6) return ElMessage.warning('初始密码至少 6 个字符')
  if (!form.roleIds.length) return ElMessage.warning('请至少选择一个角色')
  saving.value = true
  try {
    if (editingId.value) await updateUser(editingId.value, { nickName: form.nickName.trim() || undefined, isEnabled: form.isEnabled, roleIds: form.roleIds })
    else await createUser({ userName: form.userName.trim(), nickName: form.nickName.trim() || undefined, password: form.password, isEnabled: form.isEnabled, roleIds: form.roleIds })
    ElMessage.success('保存成功'); editVisible.value = false; await load()
    if (editingId.value === auth.user?.id) await auth.fetchCurrentUser()
  } finally { saving.value = false }
}
function openPassword(user: User) { passwordUser.value = user; Object.assign(passwordForm, { password: '', confirmPassword: '' }); passwordVisible.value = true }
async function savePassword() {
  if (passwordForm.password.length < 6) return ElMessage.warning('新密码至少 6 个字符')
  if (passwordForm.password !== passwordForm.confirmPassword) return ElMessage.warning('两次输入的密码不一致')
  saving.value = true
  try { await resetUserPassword(passwordUser.value!.id, passwordForm.password); ElMessage.success('密码已重置'); passwordVisible.value = false }
  finally { saving.value = false }
}
async function remove(user: User) {
  await ElMessageBox.confirm(`确定删除用户“${user.nickName || user.userName}”吗？`, '删除确认', { type: 'warning' })
  await deleteUser(user.id); ElMessage.success('删除成功'); await load()
}
function formatTime(value?: string) { return value ? new Date(value).toLocaleString('zh-CN', { hour12: false }) : '-' }
onMounted(load)
</script>

<template>
  <el-card shadow="never">
    <div class="page-heading">
      <div><h2>用户管理</h2><p>通过角色控制用户可以查看和修改的内容</p></div>
      <el-button v-if="canManage" type="primary" @click="openCreate"><el-icon><Plus /></el-icon>新增用户</el-button>
    </div>
    <div class="toolbar">
      <el-input v-model="query.keyword" placeholder="搜索账号或姓名" clearable @keyup.enter="search"><template #prefix><el-icon><Search /></el-icon></template></el-input>
      <el-select v-model="statusFilter" @change="search"><el-option label="全部状态" value="" /><el-option label="已启用" :value="true" /><el-option label="已停用" :value="false" /></el-select>
      <el-button type="primary" plain @click="search">查询</el-button>
    </div>
    <el-table v-loading="loading" :data="items" border empty-text="暂无用户">
      <el-table-column prop="userName" label="登录账号" min-width="140" />
      <el-table-column label="用户姓名" min-width="140"><template #default="scope">{{ scope.row.nickName || '-' }}</template></el-table-column>
      <el-table-column label="角色" min-width="220"><template #default="scope"><el-space wrap><el-tag v-for="name in scope.row.roleNames" :key="name" effect="plain">{{ name }}</el-tag><span v-if="!scope.row.roleNames.length">未分配</span></el-space></template></el-table-column>
      <el-table-column label="状态" width="100"><template #default="scope"><el-tag :type="scope.row.isEnabled ? 'success' : 'info'">{{ scope.row.isEnabled ? '已启用' : '已停用' }}</el-tag></template></el-table-column>
      <el-table-column label="创建时间" min-width="180"><template #default="scope">{{ formatTime(scope.row.createTime) }}</template></el-table-column>
      <el-table-column v-if="canManage" label="操作" width="230" fixed="right"><template #default="scope">
        <el-button link type="primary" @click="openEdit(scope.row)">编辑</el-button>
        <el-button link type="primary" @click="openPassword(scope.row)">重置密码</el-button>
        <el-button link type="danger" :disabled="scope.row.id === auth.user?.id" @click="remove(scope.row)">删除</el-button>
      </template></el-table-column>
    </el-table>
    <el-pagination v-model:current-page="query.page" v-model:page-size="query.pageSize" class="pagination" layout="total, sizes, prev, pager, next" :total="total" @current-change="load" @size-change="search" />
  </el-card>

  <el-dialog v-model="editVisible" :title="editingId ? '编辑用户' : '新增用户'" width="520px" :close-on-click-modal="false">
    <el-form :model="form" label-width="90px">
      <el-form-item label="登录账号" required><el-input v-model="form.userName" :disabled="!!editingId" maxlength="50" /></el-form-item>
      <el-form-item label="用户姓名"><el-input v-model="form.nickName" maxlength="50" /></el-form-item>
      <el-form-item v-if="!editingId" label="初始密码" required><el-input v-model="form.password" type="password" show-password placeholder="至少 6 个字符" /></el-form-item>
      <el-form-item label="用户角色" required><el-select v-model="form.roleIds" multiple style="width:100%"><el-option v-for="role in roles" :key="role.id" :label="role.name" :value="role.id"><span>{{ role.name }}</span><span class="role-description">{{ role.description }}</span></el-option></el-select></el-form-item>
      <el-form-item label="账号状态"><el-switch v-model="form.isEnabled" active-text="启用" inactive-text="停用" :disabled="editingId === auth.user?.id" /></el-form-item>
    </el-form>
    <template #footer><el-button @click="editVisible=false">取消</el-button><el-button type="primary" :loading="saving" @click="save">保存</el-button></template>
  </el-dialog>
  <el-dialog v-model="passwordVisible" title="重置密码" width="480px" :close-on-click-modal="false">
    <el-alert :title="`正在重置 ${passwordUser?.nickName || passwordUser?.userName} 的密码`" type="warning" :closable="false" />
    <el-form :model="passwordForm" label-width="90px" class="password-form"><el-form-item label="新密码" required><el-input v-model="passwordForm.password" type="password" show-password /></el-form-item><el-form-item label="确认密码" required><el-input v-model="passwordForm.confirmPassword" type="password" show-password @keyup.enter="savePassword" /></el-form-item></el-form>
    <template #footer><el-button @click="passwordVisible=false">取消</el-button><el-button type="primary" :loading="saving" @click="savePassword">确认重置</el-button></template>
  </el-dialog>
</template>
<style scoped>
.page-heading { display:flex; align-items:flex-start; justify-content:space-between; margin-bottom:20px; }
.page-heading h2 { margin:0 0 6px; font-size:20px; }.page-heading p { margin:0; color:#909399; font-size:14px; }
.toolbar { display:grid; grid-template-columns:minmax(240px,360px) 150px auto; gap:10px; margin-bottom:16px; }
.pagination { justify-content:flex-end; margin-top:16px; }.password-form { margin-top:20px; }
.role-description { float:right; margin-left:20px; color:#909399; font-size:12px; }
</style>
