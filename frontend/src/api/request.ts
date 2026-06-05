import axios, { type AxiosInstance, type AxiosResponse } from 'axios'
import { ElMessage } from 'element-plus'
import router from '@/router'
import { useAuthStore } from '@/stores/auth'

/**
 * 与后端约定的统一返回结构：code === 0 表示成功。
 */
export interface ApiResult<T = unknown> {
  code: number
  message: string
  data: T
}

const request: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  timeout: 15000
})

request.interceptors.request.use((config) => {
  const authStore = useAuthStore()
  if (authStore.token) {
    config.headers.Authorization = `Bearer ${authStore.token}`
  }
  return config
})

request.interceptors.response.use(
  (response: AxiosResponse<ApiResult>) => {
    const body = response.data
    if (body.code !== 0) {
      ElMessage.error(body.message || '请求失败')
      return Promise.reject(new Error(body.message || '请求失败'))
    }
    return response
  },
  (error) => {
    const status = error.response?.status
    if (status === 401) {
      const authStore = useAuthStore()
      authStore.logout()
      ElMessage.error('登录已过期，请重新登录')
      router.replace('/login')
    } else {
      ElMessage.error(error.response?.data?.message || error.message || '网络异常')
    }
    return Promise.reject(error)
  }
)

/**
 * 封装常用请求方法，直接返回业务 data。
 */
export async function get<T>(url: string, params?: object): Promise<T> {
  const res = await request.get<ApiResult<T>>(url, { params })
  return res.data.data
}

export async function post<T>(url: string, data?: object): Promise<T> {
  const res = await request.post<ApiResult<T>>(url, data)
  return res.data.data
}

export default request
