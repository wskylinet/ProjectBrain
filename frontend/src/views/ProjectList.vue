<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  createProject,
  deleteProject,
  getProjects,
  updateProject,
  type Project,
  type ProjectSaveRequest
} from '@/api/projects'

const router = useRouter()
const loading = ref(false)
const dialogVisible = ref(false)
const saving = ref(false)
const editingId = ref<number>()
const items = ref<Project[]>([])
const total = ref(0)

const query = reactive({ keyword: '', page: 1, pageSize: 10 })
const form = reactive<ProjectSaveRequest>({ region: '' })

function resetForm() {
  Object.assign(form, {
    region: '', description: ''
  })
}

async function load() {
  loading.value = true
  try {
    const result = await getProjects({ ...query })
    items.value = result.items
    total.value = result.total
  } finally {
    loading.value = false
  }
}

function search() {
  query.page = 1
  load()
}

function openDetail(project: Project) {
  router.push(`/projects/${project.id}`)
}

function openCreate() {
  editingId.value = undefined
  resetForm()
  dialogVisible.value = true
}

function openEdit(project: Project) {
  editingId.value = project.id
  Object.assign(form, {
    region: project.region || '',
    description: project.description || ''
  })
  dialogVisible.value = true
}

async function save() {
  if (!form.region.trim()) {
    ElMessage.warning('请输入部署地点')
    return
  }
  saving.value = true
  try {
    if (editingId.value) await updateProject(editingId.value, { ...form })
    else await createProject({ ...form })
    ElMessage.success('保存成功')
    dialogVisible.value = false
    await load()
  } finally {
    saving.value = false
  }
}

async function remove(project: Project) {
  await ElMessageBox.confirm(`确定删除“${project.region}”的部署档案吗？`, '删除确认', { type: 'warning' })
  await deleteProject(project.id)
  ElMessage.success('删除成功')
  if (items.value.length === 1 && query.page > 1) query.page--
  await load()
}

onMounted(load)
</script>

<template>
  <el-card shadow="never">
    <div class="toolbar">
      <el-input v-model="query.keyword" placeholder="部署地点" clearable @keyup.enter="search" />
      <el-button type="primary" @click="search">查询</el-button>
      <el-button @click="openCreate">新增部署档案</el-button>
    </div>

    <el-table v-loading="loading" :data="items" border empty-text="暂无部署档案" class="clickable-table" @row-click="openDetail">
      <el-table-column prop="region" label="部署地点" min-width="180" />
      <el-table-column label="业务系统" min-width="260">
        <template #default="scope">
          <el-space wrap>
            <el-tag v-for="app in scope.row.applications" :key="app.id" effect="plain">{{ app.name }}</el-tag>
            <span v-if="!scope.row.applications.length">-</span>
          </el-space>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="140" fixed="right">
        <template #default="scope">
          <el-button link type="primary" @click.stop="openEdit(scope.row)">编辑</el-button>
          <el-button link type="danger" @click.stop="remove(scope.row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-pagination
      v-model:current-page="query.page"
      v-model:page-size="query.pageSize"
      class="pagination"
      layout="total, sizes, prev, pager, next"
      :total="total"
      @current-change="load"
      @size-change="search"
    />
  </el-card>

  <el-dialog v-model="dialogVisible" :title="editingId ? '编辑部署档案' : '新增部署档案'" width="620px">
    <el-form :model="form" label-width="90px">
      <el-row :gutter="16">
        <el-col :span="24"><el-form-item label="部署地点" required><el-input v-model="form.region" placeholder="例如：深圳" /></el-form-item></el-col>
        <el-col :span="24"><el-form-item label="说明"><el-input v-model="form.description" type="textarea" :rows="3" /></el-form-item></el-col>
      </el-row>
    </el-form>
    <template #footer>
      <el-button @click="dialogVisible = false">取消</el-button>
      <el-button type="primary" :loading="saving" @click="save">保存</el-button>
    </template>
  </el-dialog>
</template>

<style scoped>
.toolbar { display: grid; grid-template-columns: 1fr auto auto; gap: 10px; margin-bottom: 16px; }
.clickable-table :deep(.el-table__row) { cursor: pointer; }
.pagination { justify-content: flex-end; margin-top: 16px; }
</style>
