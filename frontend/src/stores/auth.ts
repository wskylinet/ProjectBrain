import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { login as loginApi, getCurrentUser, type LoginRequest, type UserInfo } from '@/api/auth'

const TOKEN_KEY = 'pb_token'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem(TOKEN_KEY) || '')
  const user = ref<UserInfo | null>(null)
  const permissions = computed(() => user.value?.permissions || [])

  function setToken(value: string) {
    token.value = value
    localStorage.setItem(TOKEN_KEY, value)
  }
  function hasPermission(code: string) { return permissions.value.includes(code) }
  function hasAnyPermission(...codes: string[]) { return codes.some(hasPermission) }
  async function login(payload: LoginRequest) {
    const result = await loginApi(payload)
    setToken(result.token)
    user.value = result.user
    return result
  }
  async function fetchCurrentUser() {
    user.value = await getCurrentUser()
    return user.value
  }
  function logout() {
    token.value = ''
    user.value = null
    localStorage.removeItem(TOKEN_KEY)
  }
  return { token, user, permissions, login, fetchCurrentUser, logout, hasPermission, hasAnyPermission }
})
