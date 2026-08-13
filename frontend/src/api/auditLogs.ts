import { get } from './request'
import type { PagedResult } from './projects'

export interface AuditLog {
  id: number
  userId?: number
  userName?: string
  action: string
  module: string
  description: string
  eventCode?: string
  httpMethod: string
  requestPath: string
  targetId?: string
  detailJson?: string
  ipAddress?: string
  isSuccess: boolean
  statusCode: number
  durationMs: number
  createTime: string
}

export interface AuditLogQuery {
  keyword?: string
  action?: string
  eventCode?: string
  isSuccess?: boolean
  startTime?: string
  endTime?: string
  page: number
  pageSize: number
}

export const getAuditLogs = (params: AuditLogQuery) => get<PagedResult<AuditLog>>('/audit-logs', params)
