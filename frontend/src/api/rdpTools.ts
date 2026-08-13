import request, { type ApiResult } from './request'

export type RdpToolName = 'installer' | 'uninstaller'

export interface RdpToolMetadata {
  fileName: string
  sha256: string
  size: number
}

export async function getRdpToolMetadata(name: RdpToolName) {
  const response = await request.get<ApiResult<RdpToolMetadata>>(`/rdp-tools/${name}/metadata`)
  return response.data.data
}

export async function downloadRdpTool(name: RdpToolName) {
  const response = await request.get<ArrayBuffer>(`/rdp-tools/${name}/download`, {
    responseType: 'arraybuffer'
  })
  return response.data
}
