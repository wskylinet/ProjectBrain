import { del, get, post, put } from './request'
import type { PagedResult } from './projects'

export interface RoleOption { id: number; code: string; name: string; description?: string }
export interface User {
  id: number; userName: string; nickName?: string; isEnabled: boolean
  roleIds: number[]; roleNames: string[]; createTime: string; updateTime?: string
}
export interface UserQuery { keyword?: string; isEnabled?: boolean; page: number; pageSize: number }
export interface UserCreateRequest {
  userName: string; nickName?: string; password: string; isEnabled: boolean; roleIds: number[]
}
export interface UserUpdateRequest { nickName?: string; isEnabled: boolean; roleIds: number[] }

export const getUsers = (params: UserQuery) => get<PagedResult<User>>('/users', params)
export const createUser = (data: UserCreateRequest) => post<User>('/users', data)
export const updateUser = (id: number, data: UserUpdateRequest) => put<User>(`/users/${id}`, data)
export const resetUserPassword = (id: number, password: string) => put<void>(`/users/${id}/password`, { password })
export const deleteUser = (id: number) => del<void>(`/users/${id}`)
export const getRoles = () => get<RoleOption[]>('/roles')
