import { post, get } from './request'

export interface LoginRequest {
  userName: string
  password: string
}

export interface UserInfo {
  id: number
  userName: string
  nickName?: string
}

export interface LoginResponse {
  token: string
  user: UserInfo
}

export function login(data: LoginRequest): Promise<LoginResponse> {
  return post<LoginResponse>('/auth/login', data)
}

export function getCurrentUser(): Promise<UserInfo> {
  return get<UserInfo>('/auth/me')
}
