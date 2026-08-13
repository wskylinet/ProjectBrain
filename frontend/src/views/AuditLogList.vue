<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { getAuditLogs, type AuditLog } from '@/api/auditLogs'

const loading = ref(false)
const items = ref<AuditLog[]>([])
const total = ref(0)
const detailVisible = ref(false)
const selected = ref<AuditLog>()
const resultFilter = ref<'' | boolean>('')
const timeRange = ref<[Date, Date] | []>([])
const query = reactive({ keyword: '', action: '', eventCode: '', page: 1, pageSize: 20 })

const actionLabels: Record<string, string> = {
  Login: '登录', Create: '新增', Update: '修改', Delete: '删除',
  ResetPassword: '重置密码', RevealSecret: '查看密码', Access: '访问'
}

const eventLabels: Record<string, string> = {
  InvalidCredentials: '用户名或密码错误',
  AccountTemporarilyLocked: '用户名临时锁定',
  IpRateLimited: 'IP 请求过频'
}

async function load() {
  loading.value = true
  try {
    const result = await getAuditLogs({
      ...query,
      action: query.action || undefined,
      eventCode: query.eventCode || undefined,
      isSuccess: resultFilter.value === '' ? undefined : resultFilter.value,
      startTime: timeRange.value.length ? timeRange.value[0].toISOString() : undefined,
      endTime: timeRange.value.length ? timeRange.value[1].toISOString() : undefined
    })
    items.value = result.items
    total.value = result.total
  } finally { loading.value = false }
}

function search() { query.page = 1; load() }
function showDetail(row: AuditLog) { selected.value = row; detailVisible.value = true }
function formatTime(value?: string) { return value ? new Date(value).toLocaleString('zh-CN', { hour12: false }) : '-' }
function formatDetail(value?: string) {
  if (!value) return '无请求参数'
  try { return JSON.stringify(JSON.parse(value), null, 2) } catch { return value }
}
onMounted(load)
</script>

<template>
  <el-card shadow="never">
    <div class="page-heading">
      <div><h2>审计日志</h2><p>记录登录、数据变更及密码查看等敏感操作</p></div>
    </div>
    <div class="toolbar">
      <el-input v-model="query.keyword" placeholder="搜索用户、接口或操作" clearable @keyup.enter="search">
        <template #prefix><el-icon><Search /></el-icon></template>
      </el-input>
      <el-select v-model="query.action" placeholder="全部操作" clearable @change="search">
        <el-option label="登录" value="Login" /><el-option label="新增" value="Create" />
        <el-option label="修改" value="Update" /><el-option label="删除" value="Delete" />
        <el-option label="重置密码" value="ResetPassword" /><el-option label="查看密码" value="RevealSecret" />
      </el-select>
      <el-select v-model="resultFilter" @change="search">
        <el-option label="全部结果" value="" /><el-option label="成功" :value="true" /><el-option label="失败" :value="false" />
      </el-select>
      <el-select v-model="query.eventCode" placeholder="全部安全事件" clearable @change="search">
        <el-option label="用户名或密码错误" value="InvalidCredentials" />
        <el-option label="用户名临时锁定" value="AccountTemporarilyLocked" />
        <el-option label="IP 请求过频" value="IpRateLimited" />
      </el-select>
      <el-date-picker v-model="timeRange" type="datetimerange" range-separator="至" start-placeholder="开始时间" end-placeholder="结束时间" @change="search" />
      <el-button type="primary" plain @click="search">查询</el-button>
    </div>
    <el-table v-loading="loading" :data="items" border empty-text="暂无审计日志">
      <el-table-column label="时间" min-width="170"><template #default="scope">{{ formatTime(scope.row.createTime) }}</template></el-table-column>
      <el-table-column label="用户" min-width="120"><template #default="scope">{{ scope.row.userName || '匿名用户' }}</template></el-table-column>
      <el-table-column label="操作" width="110"><template #default="scope">{{ actionLabels[scope.row.action] || scope.row.action }}</template></el-table-column>
      <el-table-column prop="module" label="模块" width="110" />
      <el-table-column label="安全事件" min-width="150"><template #default="scope">{{ scope.row.eventCode ? (eventLabels[scope.row.eventCode] || scope.row.eventCode) : '-' }}</template></el-table-column>
      <el-table-column prop="requestPath" label="请求地址" min-width="260" show-overflow-tooltip />
      <el-table-column label="结果" width="90"><template #default="scope"><el-tag :type="scope.row.isSuccess ? 'success' : 'danger'">{{ scope.row.isSuccess ? '成功' : '失败' }}</el-tag></template></el-table-column>
      <el-table-column prop="ipAddress" label="IP 地址" min-width="130" />
      <el-table-column label="耗时" width="90"><template #default="scope">{{ scope.row.durationMs }} ms</template></el-table-column>
      <el-table-column label="操作" width="80" fixed="right"><template #default="scope"><el-button link type="primary" @click="showDetail(scope.row)">详情</el-button></template></el-table-column>
    </el-table>
    <el-pagination v-model:current-page="query.page" v-model:page-size="query.pageSize" class="pagination" layout="total, sizes, prev, pager, next" :page-sizes="[20, 50, 100]" :total="total" @current-change="load" @size-change="search" />
  </el-card>

  <el-dialog v-model="detailVisible" title="审计日志详情" width="680px">
    <el-descriptions v-if="selected" :column="2" border>
      <el-descriptions-item label="用户">{{ selected.userName || '匿名用户' }}</el-descriptions-item>
      <el-descriptions-item label="时间">{{ formatTime(selected.createTime) }}</el-descriptions-item>
      <el-descriptions-item label="操作">{{ actionLabels[selected.action] || selected.action }}</el-descriptions-item>
      <el-descriptions-item label="结果">{{ selected.isSuccess ? '成功' : `失败（${selected.statusCode}）` }}</el-descriptions-item>
      <el-descriptions-item label="安全事件" :span="2">{{ selected.eventCode ? (eventLabels[selected.eventCode] || selected.eventCode) : '-' }}</el-descriptions-item>
      <el-descriptions-item label="说明" :span="2">{{ selected.description }}</el-descriptions-item>
      <el-descriptions-item label="请求" :span="2">{{ selected.httpMethod }} {{ selected.requestPath }}</el-descriptions-item>
      <el-descriptions-item label="目标" :span="2">{{ selected.targetId || '-' }}</el-descriptions-item>
      <el-descriptions-item label="IP 地址">{{ selected.ipAddress || '-' }}</el-descriptions-item>
      <el-descriptions-item label="耗时">{{ selected.durationMs }} ms</el-descriptions-item>
      <el-descriptions-item label="请求参数" :span="2"><pre>{{ formatDetail(selected.detailJson) }}</pre></el-descriptions-item>
    </el-descriptions>
  </el-dialog>
</template>

<style scoped>
.page-heading { display:flex; align-items:flex-start; justify-content:space-between; margin-bottom:20px; }
.page-heading h2 { margin:0 0 6px; font-size:20px; }.page-heading p { margin:0; color:#909399; font-size:14px; }
.toolbar { display:grid; grid-template-columns:minmax(220px,1fr) 140px 120px 180px minmax(330px,1fr) auto; gap:10px; margin-bottom:16px; }
.pagination { justify-content:flex-end; margin-top:16px; }
pre { margin:0; white-space:pre-wrap; word-break:break-all; font-family:Consolas, monospace; font-size:12px; }
@media (max-width: 1100px) { .toolbar { grid-template-columns:1fr 1fr; } }
</style>
