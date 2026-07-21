import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes: RouteRecordRaw[] = [
  { path: '/login', name: 'login', component: () => import('@/views/Login.vue'), meta: { public: true, title: '登录' } },
  {
    path: '/', component: () => import('@/layout/MainLayout.vue'), redirect: '/dashboard',
    children: [
      { path: 'dashboard', name: 'dashboard', component: () => import('@/views/Dashboard.vue'), meta: { title: '工作台', permission: 'archive:view' } },
      { path: 'projects', name: 'projects', component: () => import('@/views/ProjectList.vue'), meta: { title: '部署档案', permission: 'archive:view' } },
      { path: 'projects/:id', name: 'project-detail', component: () => import('@/views/ProjectDetail.vue'), meta: { title: '部署档案详情', permission: 'archive:view' } },
      { path: 'system/users', name: 'users', component: () => import('@/views/UserList.vue'), meta: { title: '用户管理', permission: 'user:view' } }
    ]
  },
  { path: '/:pathMatch(.*)*', redirect: '/dashboard' }
]

const router = createRouter({ history: createWebHistory(), routes })
router.beforeEach(async (to) => {
  const auth = useAuthStore()
  if (to.meta.public !== true && !auth.token) return { path: '/login', query: { redirect: to.fullPath } }
  if (to.path === '/login' && auth.token) return { path: '/dashboard' }
  if (auth.token && !auth.user) {
    try { await auth.fetchCurrentUser() } catch { return { path: '/login' } }
  }
  const permission = to.meta.permission as string | undefined
  if (permission && !auth.hasPermission(permission)) {
    if (auth.hasPermission('archive:view')) return '/projects'
    if (auth.hasPermission('user:view')) return '/system/users'
    auth.logout()
    return '/login'
  }
  return true
})
export default router
