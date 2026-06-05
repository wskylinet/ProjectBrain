import { defineStore } from 'pinia'
import { ref } from 'vue'
import { login as loginApi, getCurrentUser, type LoginRequest, type UserInfo } from '@/api/auth'

const TOKEN_KEY = 'pb_token'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string>(localStorage.getItem(TOKEN_KEY) || '')
  const user = ref<UserInfo | null>(null)

  function setToken(value: string) {
    token.value = value
    localStorage.setItem(TOKEN_KEY, value)
  }

  async function login(payload: LoginRequest) {
    const res = await loginApi(payload)
    setToken(res.token)
    user.value = res.user
    return res
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

  return { token, user, login, fetchCurrentUser, logout }
})
