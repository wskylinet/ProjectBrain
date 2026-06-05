<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
</script>

<template>
  <div>
    <el-card shadow="never">
      <h2 style="margin-top: 0">
        欢迎，{{ authStore.user?.nickName || authStore.user?.userName || '用户' }}
      </h2>
      <p>这里是 Project Brain 工作台。登录骨架已就绪，后续可在此接入项目列表、详情等模块。</p>
    </el-card>

    <el-row :gutter="16" style="margin-top: 16px">
      <el-col :span="8">
        <el-card shadow="hover">
          <template #header>项目总数</template>
          <div class="stat">0</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="hover">
          <template #header>覆盖地区</template>
          <div class="stat">0</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="hover">
          <template #header>附件资料</template>
          <div class="stat">0</div>
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
