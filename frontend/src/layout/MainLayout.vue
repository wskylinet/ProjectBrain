<script setup lang="ts">
import { useRouter } from 'vue-router'
import { ElMessageBox } from 'element-plus'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()
async function onLogout() {
  await ElMessageBox.confirm('确定要退出登录吗？', '提示', { type: 'warning', confirmButtonText: '退出', cancelButtonText: '取消' })
  auth.logout()
  router.replace('/login')
}
</script>

<template>
  <el-container class="layout">
    <el-aside width="220px" class="layout-aside">
      <div class="logo">Project Brain</div>
      <el-menu :default-active="$route.path" router class="layout-menu">
        <el-menu-item v-if="auth.hasPermission('archive:view')" index="/dashboard"><el-icon><HomeFilled /></el-icon><span>工作台</span></el-menu-item>
        <el-menu-item v-if="auth.hasPermission('archive:view')" index="/projects"><el-icon><Files /></el-icon><span>部署档案</span></el-menu-item>
        <el-sub-menu v-if="auth.hasPermission('user:view')" index="system">
          <template #title><el-icon><Setting /></el-icon><span>系统管理</span></template>
          <el-menu-item index="/system/users"><el-icon><User /></el-icon><span>用户管理</span></el-menu-item>
        </el-sub-menu>
      </el-menu>
    </el-aside>
    <el-container>
      <el-header class="layout-header">
        <div class="header-title">部署档案管理平台</div>
        <el-dropdown @command="onLogout">
          <span class="user-area"><el-icon><UserFilled /></el-icon>{{ auth.user?.nickName || auth.user?.userName || '用户' }}</span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item disabled>{{ auth.user?.roleNames?.join('、') || '未分配角色' }}</el-dropdown-item>
              <el-dropdown-item divided command="logout">退出登录</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </el-header>
      <el-main class="layout-main"><router-view /></el-main>
    </el-container>
  </el-container>
</template>

<style scoped>
.layout { height: 100%; }
.layout-aside { background-color: #001529; }
.logo { height: 60px; line-height: 60px; text-align: center; color: #fff; font-size: 18px; font-weight: 600; }
.layout-menu, .layout-menu :deep(.el-menu) { border-right: none; background-color: #001529; }
.layout-menu :deep(.el-menu-item), .layout-menu :deep(.el-sub-menu__title) { color: rgba(255,255,255,.75); }
.layout-menu :deep(.el-menu-item.is-active) { color: #fff; background-color: #2a5298; }
.layout-menu :deep(.el-sub-menu__title:hover), .layout-menu :deep(.el-menu-item:hover) { background-color: #0b2946; }
.layout-header { display:flex; align-items:center; justify-content:space-between; background:#fff; border-bottom:1px solid #eaeaea; }
.header-title { font-size:16px; font-weight:600; }
.user-area { display:flex; align-items:center; gap:6px; cursor:pointer; outline:none; }
.layout-main { background:#f5f7fa; }
</style>
