import { post, get } from './request'

export interface LoginRequest { userName: string; password: string }
export interface UserInfo {
  id: number
  userName: string
  nickName?: string
  roleCodes: string[]
  roleNames: string[]
  permissions: string[]
}
export interface LoginResponse { token: string; user: UserInfo }
export const login = (data: LoginRequest) => post<LoginResponse>('/auth/login', data)
export const getCurrentUser = () => get<UserInfo>('/auth/me')
