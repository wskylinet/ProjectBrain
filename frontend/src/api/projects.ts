import { del, get, post, put } from './request'

export interface Project {
  id: number
  region: string
  description?: string
  applications: ProjectApplication[]
  createTime: string
  updateTime?: string
}

export type ProjectSaveRequest = Omit<Project, 'id' | 'applications' | 'createTime' | 'updateTime'>

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export interface ProjectQuery {
  keyword?: string
  region?: string
  page: number
  pageSize: number
}

export interface ProjectConnection {
  id: number
  projectId: number
  applicationIds: number[]
  applicationNames: string[]
  parentId?: number
  parentName?: string
  name: string
  connectionType: string
  address?: string
  port?: string
  userName?: string
  hasPassword: boolean
  remark?: string
  sort: number
  createTime: string
  updateTime?: string
  remoteControls: ProjectConnectionRemoteControl[]
}

export interface ConnectionSaveRequest {
  applicationIds: number[]
  parentId?: number
  name: string
  connectionType: string
  address?: string
  port?: string
  userName?: string
  password?: string
  clearPassword: boolean
  remark?: string
  sort: number
  remoteControls: RemoteControlSaveRequest[]
}

export interface RemoteControlSaveRequest {
  id?: number
  softwareName: string
  deviceCode: string
  password?: string
  clearPassword: boolean
  sort: number
}

export interface ProjectConnectionRemoteControl {
  id: number
  connectionId: number
  softwareName: string
  deviceCode: string
  hasPassword: boolean
  sort: number
  createTime: string
  updateTime?: string
}

export interface ProjectApplication {
  id: number
  projectId: number
  name: string
  loginAddress?: string
  userName?: string
  hasPassword: boolean
  remark?: string
  sort: number
  createTime: string
  updateTime?: string
}

export interface ApplicationSaveRequest {
  name: string
  loginAddress?: string
  userName?: string
  password?: string
  clearPassword: boolean
  remark?: string
  sort: number
}

export interface ProjectContact {
  id: number
  projectId: number
  role: string
  name: string
  contactInfo?: string
  remark?: string
  sort: number
  createTime: string
  updateTime?: string
}

export interface ContactSaveRequest {
  role: string
  name: string
  contactInfo?: string
  remark?: string
  sort: number
}

export interface DashboardStats {
  projectCount: number
  regionCount: number
  connectionCount: number
}

export const getProjects = (params: ProjectQuery) =>
  get<PagedResult<Project>>('/projects', params)
export const getProject = (id: number) => get<Project>(`/projects/${id}`)
export const createProject = (data: ProjectSaveRequest) => post<Project>('/projects', data)
export const updateProject = (id: number, data: ProjectSaveRequest) =>
  put<Project>(`/projects/${id}`, data)
export const deleteProject = (id: number) => del<void>(`/projects/${id}`)

export const getContacts = (projectId: number) =>
  get<ProjectContact[]>(`/projects/${projectId}/contacts`)
export const createContact = (projectId: number, data: ContactSaveRequest) =>
  post<ProjectContact>(`/projects/${projectId}/contacts`, data)
export const updateContact = (projectId: number, id: number, data: ContactSaveRequest) =>
  put<ProjectContact>(`/projects/${projectId}/contacts/${id}`, data)
export const deleteContact = (projectId: number, id: number) =>
  del<void>(`/projects/${projectId}/contacts/${id}`)

export const getConnections = (projectId: number) =>
  get<ProjectConnection[]>(`/projects/${projectId}/connections`)
export const createConnection = (projectId: number, data: ConnectionSaveRequest) =>
  post<ProjectConnection>(`/projects/${projectId}/connections`, data)
export const getApplications = (projectId: number) =>
  get<ProjectApplication[]>(`/projects/${projectId}/applications`)
export const createApplication = (projectId: number, data: ApplicationSaveRequest) =>
  post<ProjectApplication>(`/projects/${projectId}/applications`, data)
export const updateApplication = (projectId: number, id: number, data: ApplicationSaveRequest) =>
  put<ProjectApplication>(`/projects/${projectId}/applications/${id}`, data)
export const deleteApplication = (projectId: number, id: number) =>
  del<void>(`/projects/${projectId}/applications/${id}`)
export const revealApplicationPassword = (projectId: number, id: number) =>
  post<{ password: string }>(`/projects/${projectId}/applications/${id}/reveal-password`)

export const updateConnection = (projectId: number, id: number, data: ConnectionSaveRequest) =>
  put<ProjectConnection>(`/projects/${projectId}/connections/${id}`, data)
export const deleteConnection = (projectId: number, id: number) =>
  del<void>(`/projects/${projectId}/connections/${id}`)
export const revealPassword = (projectId: number, id: number) =>
  post<{ password: string }>(`/projects/${projectId}/connections/${id}/reveal-password`)
export const revealRemoteControlPassword = (projectId: number, connectionId: number, remoteControlId: number) =>
  post<{ password: string }>(`/projects/${projectId}/connections/${connectionId}/remote-controls/${remoteControlId}/reveal-password`)

export const getDashboardStats = () => get<DashboardStats>('/dashboard/stats')
