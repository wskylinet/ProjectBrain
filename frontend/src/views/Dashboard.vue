<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { getDashboardStats, type DashboardStats } from '@/api/projects'

const authStore = useAuthStore()
const stats = ref<DashboardStats>({ projectCount: 0, regionCount: 0, connectionCount: 0 })

onMounted(async () => {
  try {
    stats.value = await getDashboardStats()
  } catch {
    // 请求拦截器统一展示错误。
  }
})
</script>

<template>
  <div>
    <el-card shadow="never">
      <h2 style="margin-top: 0">
        欢迎，{{ authStore.user?.nickName || authStore.user?.userName || '用户' }}
      </h2>
      <p>集中维护各地部署档案、业务系统登录信息，以及可复用的 VPN、堡垒机和服务器连接链。</p>
    </el-card>

    <el-row :gutter="16" style="margin-top: 16px">
      <el-col :span="8">
        <el-card shadow="hover">
          <template #header>部署档案</template>
          <div class="stat">{{ stats.projectCount }}</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="hover">
          <template #header>部署地点</template>
          <div class="stat">{{ stats.regionCount }}</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="hover">
          <template #header>连接信息</template>
          <div class="stat">{{ stats.connectionCount }}</div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped>
.stat {
  font-size: 32px;
  font-weight: 700;
  color: #2a5298;
}
</style>
