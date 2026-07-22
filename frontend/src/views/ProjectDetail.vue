<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  createApplication,
  createContact,
  createConnection,
  deleteApplication,
  deleteContact,
  deleteConnection,
  getApplications,
  getConnections,
  getContacts,
  getProject,
  revealApplicationPassword,
  revealRemoteControlPassword,
  revealPassword,
  updateApplication,
  updateContact,
  updateConnection,
  type ApplicationSaveRequest,
  type ContactSaveRequest,
  type ConnectionSaveRequest,
  type Project,
  type ProjectApplication,
  type ProjectConnection,
  type ProjectConnectionRemoteControl,
  type ProjectContact
} from '@/api/projects'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const projectId = Number(route.params.id)
const project = ref<Project>()
const applications = ref<ProjectApplication[]>([])
const connections = ref<ProjectConnection[]>([])
const contacts = ref<ProjectContact[]>([])
const loading = ref(false)
const dialogVisible = ref(false)
const applicationDialogVisible = ref(false)
const contactDialogVisible = ref(false)
const saving = ref(false)
const applicationSaving = ref(false)
const contactSaving = ref(false)
const editingId = ref<number>()
const applicationEditingId = ref<number>()
const contactEditingId = ref<number>()
const visiblePasswords = reactive<Record<number, string>>({})
const visibleRemoteControlPasswords = reactive<Record<number, string>>({})
const visibleApplicationPasswords = reactive<Record<number, string>>({})

const connectionTypes = ['VPN 软件', 'VPN 网页', '网页堡垒机', '远程控制软件', 'Windows 远程桌面', 'Linux SSH', 'SQL Server', 'InfluxDB', 'Hangfire', 'Redis', '网站', '其他']

function uniqueNames(names: string[]) {
  const seen = new Set<string>()
  return names.filter(name => {
    const key = name.trim().toLocaleLowerCase()
    if (!key || seen.has(key)) return false
    seen.add(key)
    return true
  })
}

const selectedApplicationNames = ref<string[]>([])
const applicationNameOptions = computed(() => uniqueNames(applications.value.map(item => item.name)))

function applicationIdsForNames(names: string[]) {
  const keys = new Set(names.map(name => name.trim().toLocaleLowerCase()))
  return applications.value
    .filter(item => keys.has(item.name.trim().toLocaleLowerCase()))
    .map(item => item.id)
}
const remoteSoftwareOptions = ['ToDesk', '向日葵']
const rdpInstallerUrl = `${import.meta.env.BASE_URL}tools/ProjectBrainRdpInstaller.txt`
const rdpInstallerFileName = 'Install-ProjectBrainRdpProtocol.cmd'
const rdpUninstallerUrl = `${import.meta.env.BASE_URL}tools/ProjectBrainRdpUninstaller.txt`
const rdpUninstallerFileName = 'Uninstall-ProjectBrainRdpProtocol.cmd'

const form = reactive<ConnectionSaveRequest>({
  applicationIds: [], name: '', connectionType: 'Windows 远程桌面', clearPassword: false, sort: 0, remoteControls: []
})
const contactForm = reactive<ContactSaveRequest>({ role: '', name: '', sort: 0 })
const applicationForm = reactive<ApplicationSaveRequest>({
  name: '', clearPassword: false, sort: 0
})

function resetForm() {
  Object.assign(form, {
    applicationIds: [], parentId: undefined, name: '', connectionType: 'Windows 远程桌面', address: '', port: '',
    userName: '', password: undefined, clearPassword: false, remark: '', sort: 0, remoteControls: []
  })
  selectedApplicationNames.value = []
}

async function load() {
  loading.value = true
  try {
    const [projectData, applicationData, connectionData, contactData] = await Promise.all([
      getProject(projectId), getApplications(projectId), getConnections(projectId), getContacts(projectId)
    ])
    project.value = projectData
    connections.value = connectionData
    contacts.value = contactData
    applications.value = applicationData
  } finally {
    loading.value = false
  }
}

function openCreate() {
  editingId.value = undefined
  resetForm()
  dialogVisible.value = true
}

function openEdit(item: ProjectConnection) {
  editingId.value = item.id
  Object.assign(form, {
    parentId: item.parentId,
    name: item.name,
    applicationIds: [...item.applicationIds],
    connectionType: item.connectionType,
    address: item.address || '',
    port: item.port || '',
    userName: item.userName || '',
    password: undefined,
    clearPassword: false,
    remark: item.remark || '',
    sort: item.sort,
    remoteControls: item.remoteControls.map(remote => ({
      id: remote.id,
      softwareName: remote.softwareName,
      deviceCode: remote.deviceCode,
      password: undefined,
      clearPassword: false,
      sort: remote.sort
    }))
  })
  const selectedIds = new Set(item.applicationIds)
  selectedApplicationNames.value = uniqueNames(applications.value.filter(item => selectedIds.has(item.id)).map(item => item.name))

  dialogVisible.value = true
}

async function save() {
  if (!form.name.trim()) {
    ElMessage.warning('请输入连接名称')
    return
  }
  if (form.connectionType === '远程控制软件') {
    if (!form.remoteControls.length) {
      ElMessage.warning('请至少添加一种具体远控软件')
      return
    }
    if (form.remoteControls.some(item => !item.softwareName.trim() || !item.deviceCode.trim())) {
      ElMessage.warning('请完整填写具体软件和设备代码')
      return
    }
  }
  saving.value = true
  try {
    const request = { ...form, applicationIds: applicationIdsForNames(selectedApplicationNames.value) }
    if (editingId.value) await updateConnection(projectId, editingId.value, request)
    else await createConnection(projectId, request)
    ElMessage.success('保存成功')
    dialogVisible.value = false
    await load()
  } finally {
    saving.value = false
  }
}

function openApplicationCreate() {
  applicationEditingId.value = undefined
  Object.assign(applicationForm, {
    name: '', loginAddress: '', userName: '', password: undefined,
    clearPassword: false, remark: '', sort: 0
  })
  applicationDialogVisible.value = true
}

function openApplicationEdit(item: ProjectApplication) {
  applicationEditingId.value = item.id
  Object.assign(applicationForm, {
    name: item.name,
    loginAddress: item.loginAddress || '',
    userName: item.userName || '',
    password: undefined,
    clearPassword: false,
    remark: item.remark || '',
    sort: item.sort
  })
  applicationDialogVisible.value = true
}

async function saveApplication() {
  if (!applicationForm.name.trim()) {
    ElMessage.warning('请输入系统名称')
    return
  }
  applicationSaving.value = true
  try {
    if (applicationEditingId.value) {
      await updateApplication(projectId, applicationEditingId.value, { ...applicationForm })
    } else {
      await createApplication(projectId, { ...applicationForm })
    }
    ElMessage.success('保存成功')
    applicationDialogVisible.value = false
    await load()
  } finally {
    applicationSaving.value = false
  }
}

async function removeApplication(item: ProjectApplication) {
  await ElMessageBox.confirm(`确定删除业务系统“${item.name}”吗？`, '删除确认', { type: 'warning' })
  await deleteApplication(projectId, item.id)
  ElMessage.success('删除成功')
  await load()
}

async function showApplicationPassword(item: ProjectApplication) {
  const result = await revealApplicationPassword(projectId, item.id)
  visibleApplicationPasswords[item.id] = result.password
  window.setTimeout(() => delete visibleApplicationPasswords[item.id], 30000)
}

function openContactCreate() {
  contactEditingId.value = undefined
  Object.assign(contactForm, { role: '', name: '', contactInfo: '', remark: '', sort: 0 })
  contactDialogVisible.value = true
}

function openContactEdit(item: ProjectContact) {
  contactEditingId.value = item.id
  Object.assign(contactForm, {
    role: item.role,
    name: item.name,
    contactInfo: item.contactInfo || '',
    remark: item.remark || '',
    sort: item.sort
  })
  contactDialogVisible.value = true
}

async function saveContact() {
  if (!contactForm.role.trim() || !contactForm.name.trim()) {
    ElMessage.warning('请输入职责和姓名')
    return
  }
  contactSaving.value = true
  try {
    if (contactEditingId.value) {
      await updateContact(projectId, contactEditingId.value, { ...contactForm })
    } else {
      await createContact(projectId, { ...contactForm })
    }
    ElMessage.success('保存成功')
    contactDialogVisible.value = false
    contacts.value = await getContacts(projectId)
  } finally {
    contactSaving.value = false
  }
}

async function removeContact(item: ProjectContact) {
  await ElMessageBox.confirm(`确定删除“${item.role} - ${item.name}”吗？`, '删除确认', { type: 'warning' })
  await deleteContact(projectId, item.id)
  ElMessage.success('删除成功')
  contacts.value = await getContacts(projectId)
}

async function remove(item: ProjectConnection) {
  await ElMessageBox.confirm(`确定删除连接“${item.name}”吗？`, '删除确认', { type: 'warning' })
  await deleteConnection(projectId, item.id)
  ElMessage.success('删除成功')
  await load()
}

async function showPassword(item: ProjectConnection) {
  const result = await revealPassword(projectId, item.id)
  visiblePasswords[item.id] = result.password
  window.setTimeout(() => delete visiblePasswords[item.id], 30000)
}

async function showRemoteControlPassword(item: ProjectConnection, remote: ProjectConnectionRemoteControl) {
  const result = await revealRemoteControlPassword(projectId, item.id, remote.id)
  visibleRemoteControlPasswords[remote.id] = result.password
  window.setTimeout(() => delete visibleRemoteControlPasswords[remote.id], 30000)
}

function addRemoteControl() {
  const used = new Set(form.remoteControls.map(item => item.softwareName))
  const softwareName = remoteSoftwareOptions.find(item => !used.has(item)) || ''
  form.remoteControls.push({
    softwareName,
    deviceCode: '',
    password: undefined,
    clearPassword: false,
    sort: form.remoteControls.length
  })
}

function removeRemoteControl(index: number) {
  form.remoteControls.splice(index, 1)
}

async function copyText(value: string, label: string) {
  await navigator.clipboard.writeText(value)
  ElMessage.success(`${label}已复制`)
}

function isWebAddress(address?: string) {
  return /^https?:\/\//i.test(address?.trim() || '')
}

function connectionAddressText(item: ProjectConnection) {
  const address = item.address?.trim() || ''
  const port = item.port?.trim()
  if (!address || !port) return address

  if (isWebAddress(address)) {
    try {
      const url = new URL(address)
      if (!url.port) url.port = port
      return url.toString()
    } catch {
      // 地址不符合标准 URL 时保留原来的展示方式。
    }
  }

  return `${address}:${port}`
}

function isRemoteDesktop(item: ProjectConnection) {
  const type = item.connectionType.toLowerCase()
  return type.includes('远程桌面') || type.includes('rdp')
}

function canLaunchRemoteDesktop(item: ProjectConnection) {
  if (!isRemoteDesktop(item)) return false

  const byId = new Map(connections.value.map(connection => [connection.id, connection]))
  const visited = new Set<number>([item.id])
  let parent = item.parentId ? byId.get(item.parentId) : undefined
  while (parent && !visited.has(parent.id)) {
    if (parent.connectionType.toLowerCase().includes('网页堡垒机')) return false

    visited.add(parent.id)
    parent = parent.parentId ? byId.get(parent.parentId) : undefined
  }

  return true
}

function launchRemoteDesktop(item: ProjectConnection) {
  const address = item.address?.trim()
  if (!address) {
    ElMessage.warning('请先填写远程桌面地址')
    return
  }

  const query = new URLSearchParams({ address })
  if (item.port?.trim()) query.set('port', item.port.trim())

  // 只传递连接目标。用户名和密码不进入 URL，避免浏览器历史及日志泄露凭据。
  window.location.href = `projectbrain-rdp://connect?${query.toString()}`
}

function connectionPath(item: ProjectConnection) {
  const byId = new Map(connections.value.map(x => [x.id, x]))
  const names: string[] = []
  const visited = new Set<number>()
  let cursor: ProjectConnection | undefined = item
  while (cursor && !visited.has(cursor.id)) {
    visited.add(cursor.id)
    names.unshift(cursor.name)
    cursor = cursor.parentId ? byId.get(cursor.parentId) : undefined
  }
  return names.join(' → ')
}

onMounted(load)
</script>

<template>
  <div v-loading="loading">
    <div class="page-header">
      <el-button @click="router.push('/projects')">返回部署档案</el-button>
      <h2>{{ project?.region || '部署档案详情' }}</h2>
      <div class="page-header-spacer" />
      <el-dropdown>
        <el-button plain>远程工具</el-button>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item><a :href="rdpInstallerUrl" :download="rdpInstallerFileName">下载初始化脚本</a></el-dropdown-item>
            <el-dropdown-item><a :href="rdpUninstallerUrl" :download="rdpUninstallerFileName">下载卸载脚本</a></el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
      <el-button v-if="auth.hasPermission('archive:update') && !contacts.length" plain @click="openContactCreate">添加人员</el-button>
    </div>

    <el-card v-if="project" shadow="never">
      <el-descriptions :column="2" border>
        <el-descriptions-item label="部署地点">{{ project.region || '-' }}</el-descriptions-item>
        <el-descriptions-item label="说明">{{ project.description || '-' }}</el-descriptions-item>
      </el-descriptions>
    </el-card>

    <el-card shadow="never" class="section-card">
      <template #header>
        <div class="card-header"><span>业务系统</span><el-button v-if="auth.hasPermission('archive:update')" type="primary" @click="openApplicationCreate">新增业务系统</el-button></div>
      </template>
      <el-table :data="applications" border empty-text="暂未添加业务系统">
        <el-table-column prop="name" label="系统名称" min-width="150" />
        <el-table-column label="登录入口" min-width="280">
          <template #default="scope">
            <template v-if="scope.row.loginAddress">
              <a :href="scope.row.loginAddress" target="_blank" rel="noopener noreferrer">{{ scope.row.loginAddress }}</a>
              <el-button link type="primary" @click="copyText(scope.row.loginAddress, '登录入口')">复制</el-button>
            </template>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column label="账号" min-width="170">
          <template #default="scope">
            <template v-if="scope.row.userName">
              <span class="copy-value">{{ scope.row.userName }}</span>
              <el-button link type="primary" @click="copyText(scope.row.userName, '账号')">复制</el-button>
            </template>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column label="密码" min-width="190">
          <template #default="scope">
            <template v-if="visibleApplicationPasswords[scope.row.id]">
              <span class="password-text">{{ visibleApplicationPasswords[scope.row.id] }}</span>
              <el-button link type="primary" @click="copyText(visibleApplicationPasswords[scope.row.id], '密码')">复制</el-button>
            </template>
            <el-button v-else-if="scope.row.hasPassword && auth.hasPermission('secret:reveal')" link type="primary" @click="showApplicationPassword(scope.row)">显示30秒</el-button>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column prop="remark" label="备注" min-width="160" />
        <el-table-column label="操作" width="130" fixed="right">
          <template #default="scope">
            <el-button v-if="auth.hasPermission('archive:update')" link type="primary" @click="openApplicationEdit(scope.row)">编辑</el-button>
            <el-button v-if="auth.hasPermission('archive:delete')" link type="danger" @click="removeApplication(scope.row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-card v-if="contacts.length" shadow="never" class="section-card">
      <template #header>
        <div class="card-header"><span>相关人员</span><el-button v-if="auth.hasPermission('archive:update')" type="primary" @click="openContactCreate">新增人员</el-button></div>
      </template>
      <el-table :data="contacts" border>
        <el-table-column prop="role" label="职责" width="130" />
        <el-table-column prop="name" label="姓名" width="120" />
        <el-table-column prop="contactInfo" label="联系方式" min-width="180" />
        <el-table-column prop="remark" label="备注" min-width="180" />
        <el-table-column label="操作" width="130" fixed="right">
          <template #default="scope">
            <el-button v-if="auth.hasPermission('archive:update')" link type="primary" @click="openContactEdit(scope.row)">编辑</el-button>
            <el-button v-if="auth.hasPermission('archive:delete')" link type="danger" @click="removeContact(scope.row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-card shadow="never" class="section-card">
      <template #header>
        <div class="card-header"><span>连接信息</span><el-button v-if="auth.hasPermission('archive:update')" type="primary" @click="openCreate">新增连接</el-button></div>
      </template>
      <el-alert title="通过前置连接可表达：VPN 软件或网页 → 网页堡垒机 → 远程桌面 → SQL Server、InfluxDB、Redis 等" type="info" :closable="false" />
      <el-table :data="connections" border class="connection-table">
        <el-table-column label="连接路径" min-width="230">
          <template #default="scope">{{ connectionPath(scope.row) }}</template>
        </el-table-column>
        <el-table-column prop="connectionType" label="类型" width="190" show-overflow-tooltip />
        <el-table-column label="适用系统" min-width="180">
          <template #default="scope">
            <el-tag v-if="!uniqueNames(scope.row.applicationNames).length" type="info" effect="plain">公共连接</el-tag>
            <el-space v-else wrap>
              <el-tag v-for="name in uniqueNames(scope.row.applicationNames)" :key="name" effect="plain">{{ name }}</el-tag>
            </el-space>
          </template>
        </el-table-column>
        <el-table-column label="地址" min-width="230">
          <template #default="scope">
            <template v-if="scope.row.connectionType === '远程控制软件'">-</template>
            <template v-else-if="scope.row.address">
              <a v-if="isWebAddress(scope.row.address)" :href="connectionAddressText(scope.row)" target="_blank" rel="noopener noreferrer">{{ connectionAddressText(scope.row) }}</a>
              <span v-else class="copy-value">{{ connectionAddressText(scope.row) }}</span>
              <el-button link type="primary" @click="copyText(connectionAddressText(scope.row), '地址')">复制</el-button>
            </template>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column label="用户名" min-width="240">
          <template #default="scope">
            <template v-if="scope.row.connectionType === '远程控制软件'">
              <div v-for="remote in scope.row.remoteControls" :key="remote.id" class="remote-control-row">
                <el-tag size="small" effect="plain">{{ remote.softwareName }}</el-tag>
                <span class="copy-value">{{ remote.deviceCode }}</span>
                <el-button link type="primary" @click="copyText(remote.deviceCode, '设备代码')">复制</el-button>
              </div>
              <span v-if="!scope.row.remoteControls.length">-</span>
            </template>
            <template v-else-if="scope.row.userName">
              <span class="copy-value">{{ scope.row.userName }}</span>
              <el-button link type="primary" @click="copyText(scope.row.userName, '用户名')">复制</el-button>
            </template>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column label="密码" min-width="200">
          <template #default="scope">
            <template v-if="scope.row.connectionType === '远程控制软件'">
              <div v-for="remote in scope.row.remoteControls" :key="remote.id" class="remote-control-password-row">
                <span class="remote-control-password-software">{{ remote.softwareName }}：</span>
                <template v-if="visibleRemoteControlPasswords[remote.id]">
                  <span class="password-text">{{ visibleRemoteControlPasswords[remote.id] }}</span>
                  <el-button link type="primary" @click="copyText(visibleRemoteControlPasswords[remote.id], '密码')">复制</el-button>
                </template>
                <el-button v-else-if="remote.hasPassword && auth.hasPermission('secret:reveal')" link type="primary" @click="showRemoteControlPassword(scope.row, remote)">显示30秒</el-button>
                <span v-else>-</span>
              </div>
              <span v-if="!scope.row.remoteControls.length">-</span>
            </template>
            <template v-else-if="visiblePasswords[scope.row.id]">
              <span class="password-text">{{ visiblePasswords[scope.row.id] }}</span>
              <el-button link type="primary" @click="copyText(visiblePasswords[scope.row.id], '密码')">复制</el-button>
            </template>
            <el-button v-else-if="scope.row.connectionType !== '远程控制软件' && scope.row.hasPassword && auth.hasPermission('secret:reveal')" link type="primary" @click="showPassword(scope.row)">显示30秒</el-button>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="scope">
            <el-button
              v-if="canLaunchRemoteDesktop(scope.row)"
              link
              type="success"
              :disabled="!scope.row.address"
              @click="launchRemoteDesktop(scope.row)"
            >一键远程</el-button>
            <el-button v-if="auth.hasPermission('archive:update')" link type="primary" @click="openEdit(scope.row)">编辑</el-button>
            <el-button v-if="auth.hasPermission('archive:delete')" link type="danger" @click="remove(scope.row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>

  <el-dialog v-model="applicationDialogVisible" :title="applicationEditingId ? '编辑业务系统' : '新增业务系统'" width="600px" :close-on-click-modal="false" :close-on-press-escape="false" :show-close="false">
    <el-form :model="applicationForm" label-width="90px">
      <el-form-item label="系统名称" required><el-input v-model="applicationForm.name" placeholder="例如：智水家园、校园节水、计量云" /></el-form-item>
      <el-form-item label="登录入口"><el-input v-model="applicationForm.loginAddress" placeholder="例如：https://example.com" /></el-form-item>
      <el-form-item label="登录账号"><el-input v-model="applicationForm.userName" /></el-form-item>
      <el-form-item v-if="auth.hasPermission('secret:update')" label="登录密码"><el-input v-model="applicationForm.password" type="password" show-password :placeholder="applicationEditingId ? '留空表示不修改' : ''" /></el-form-item>
      <el-form-item v-if="applicationEditingId && auth.hasPermission('secret:update')" label="清除密码"><el-switch v-model="applicationForm.clearPassword" /></el-form-item>
      <el-form-item label="备注"><el-input v-model="applicationForm.remark" type="textarea" :rows="3" /></el-form-item>
    </el-form>
    <template #footer><el-button @click="applicationDialogVisible = false">取消</el-button><el-button type="primary" :loading="applicationSaving" @click="saveApplication">保存</el-button></template>
  </el-dialog>

  <el-dialog v-model="contactDialogVisible" :title="contactEditingId ? '编辑相关人员' : '新增相关人员'" width="560px" :close-on-click-modal="false" :close-on-press-escape="false" :show-close="false">
    <el-form :model="contactForm" label-width="80px">
      <el-form-item label="职责" required>
        <el-input v-model="contactForm.role" placeholder="例如：硬件、实施、运维" />
      </el-form-item>
      <el-form-item label="姓名" required><el-input v-model="contactForm.name" /></el-form-item>
      <el-form-item label="联系方式"><el-input v-model="contactForm.contactInfo" placeholder="手机号、微信、邮箱等" /></el-form-item>
      <el-form-item label="备注"><el-input v-model="contactForm.remark" type="textarea" :rows="3" /></el-form-item>
    </el-form>
    <template #footer><el-button @click="contactDialogVisible = false">取消</el-button><el-button type="primary" :loading="contactSaving" @click="saveContact">保存</el-button></template>
  </el-dialog>

  <el-dialog v-model="dialogVisible" :title="editingId ? '编辑连接' : '新增连接'" width="860px" :close-on-click-modal="false" :close-on-press-escape="false" :show-close="false">
    <el-form :model="form" label-width="90px">
      <el-row :gutter="16">
        <el-col :span="12"><el-form-item label="连接名称" required><el-input v-model="form.name" placeholder="例如：生产堡垒机" /></el-form-item></el-col>
        <el-col :span="12"><el-form-item label="类型" required><el-select v-model="form.connectionType" filterable allow-create default-first-option style="width:100%" placeholder="选择或输入连接类型"><el-option v-for="type in connectionTypes" :key="type" :label="type" :value="type" /></el-select></el-form-item></el-col>
        <el-col :span="12"><el-form-item label="前置连接"><el-select v-model="form.parentId" clearable style="width:100%"><el-option v-for="item in connections.filter(x => x.id !== editingId)" :key="item.id" :label="connectionPath(item)" :value="item.id" /></el-select></el-form-item></el-col>
        <el-col :span="24"><el-form-item label="适用系统">
          <el-select v-model="selectedApplicationNames" multiple clearable style="width:100%" placeholder="留空表示公共连接">
            <el-option v-for="name in applicationNameOptions" :key="name" :label="name" :value="name" />
          </el-select>
        </el-form-item></el-col>
        <template v-if="form.connectionType === '远程控制软件'">
          <el-col :span="24"><div class="remote-control-header"><strong>具体远控软件</strong><el-button type="primary" plain @click="addRemoteControl">添加软件</el-button></div></el-col>
          <el-col v-for="(remote, index) in form.remoteControls" :key="remote.id || `new-${index}`" :span="24">
            <el-card shadow="never" class="remote-control-editor">
              <el-row :gutter="12" align="middle">
                <el-col :span="6"><el-form-item label="具体软件" required><el-select v-model="remote.softwareName" filterable allow-create default-first-option style="width:100%" placeholder="选择或输入"><el-option v-for="software in remoteSoftwareOptions" :key="software" :label="software" :value="software" /></el-select></el-form-item></el-col>
                <el-col :span="8"><el-form-item label="设备代码" required><el-input v-model="remote.deviceCode" /></el-form-item></el-col>
                <el-col v-if="auth.hasPermission('secret:update')" :span="7"><el-form-item label="密码"><el-input v-model="remote.password" type="password" show-password :placeholder="remote.id ? '留空表示不修改' : ''" /></el-form-item></el-col>
                <el-col :span="3"><el-button type="danger" link @click="removeRemoteControl(index)">删除</el-button></el-col>
                <el-col v-if="remote.id && auth.hasPermission('secret:update')" :span="8"><el-form-item label="清除密码"><el-switch v-model="remote.clearPassword" /></el-form-item></el-col>
              </el-row>
            </el-card>
          </el-col>
        </template>
        <template v-else>
        <el-col :span="16"><el-form-item label="地址"><el-input v-model="form.address" placeholder="IP、域名或网址" /></el-form-item></el-col>
        <el-col :span="8"><el-form-item label="端口"><el-input v-model="form.port" /></el-form-item></el-col>
        <el-col :span="12"><el-form-item label="用户名"><el-input v-model="form.userName" /></el-form-item></el-col>
        <el-col v-if="auth.hasPermission('secret:update')" :span="12"><el-form-item label="密码"><el-input v-model="form.password" type="password" show-password :placeholder="editingId ? '留空表示不修改' : ''" /></el-form-item></el-col>
        <el-col v-if="editingId && auth.hasPermission('secret:update')" :span="24"><el-form-item label="清除密码"><el-switch v-model="form.clearPassword" /></el-form-item></el-col>
        </template>
        <el-col :span="24"><el-form-item label="备注"><el-input v-model="form.remark" type="textarea" :rows="3" placeholder="连接步骤、注意事项等" /></el-form-item></el-col>
      </el-row>
    </el-form>
    <template #footer><el-button @click="dialogVisible = false">取消</el-button><el-button type="primary" :loading="saving" @click="save">保存</el-button></template>
  </el-dialog>
</template>

<style scoped>
.page-header { display: flex; align-items: center; gap: 16px; margin-bottom: 16px; }
.page-header h2 { margin: 0; }
.page-header-spacer { flex: 1; }
.section-card { margin-top: 16px; }
.card-header { display: flex; align-items: center; justify-content: space-between; }
.connection-table { margin-top: 14px; }
.password-text { margin-right: 8px; font-family: Consolas, monospace; }
.copy-value, a { margin-right: 8px; }
.remote-control-header { display: flex; align-items: center; justify-content: space-between; margin: 14px 0 10px; }
.remote-control-editor { margin-bottom: 10px; background: #fafafa; }
.remote-control-row { display: flex; align-items: center; flex-wrap: wrap; gap: 5px; margin: 3px 0; }
.remote-control-password-row { display: grid; grid-template-columns: 64px minmax(0, auto) auto; justify-content: start; align-items: center; column-gap: 5px; min-height: 32px; margin: 3px 0; }
.remote-control-password-software { white-space: nowrap; }
</style>
