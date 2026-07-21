using ProjectBrain.Api.Dtos;
using ProjectBrain.Api.Entities;

namespace ProjectBrain.Api.Services;

public interface IAuditLogService
{
    Task WriteAsync(SysAuditLog log);
    Task<PagedResult<AuditLogDto>> GetLogsAsync(AuditLogQuery query);
}
