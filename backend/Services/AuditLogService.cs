using ProjectBrain.Api.Data;
using ProjectBrain.Api.Dtos;
using ProjectBrain.Api.Entities;

namespace ProjectBrain.Api.Services;

public class AuditLogService : IAuditLogService
{
    private readonly DbContext _dbContext;
    public AuditLogService(DbContext dbContext) => _dbContext = dbContext;

    public Task WriteAsync(SysAuditLog log) => _dbContext.Db.Insertable(log).ExecuteCommandAsync();

    public async Task<PagedResult<AuditLogDto>> GetLogsAsync(AuditLogQuery query)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var keyword = query.Keyword?.Trim();
        var dbQuery = _dbContext.Db.Queryable<SysAuditLog>()
            .WhereIF(!string.IsNullOrEmpty(keyword), x =>
                (x.UserName != null && x.UserName.Contains(keyword!)) ||
                x.Description.Contains(keyword!) || x.RequestPath.Contains(keyword!))
            .WhereIF(!string.IsNullOrEmpty(query.Action), x => x.Action == query.Action)
            .WhereIF(!string.IsNullOrEmpty(query.EventCode), x => x.EventCode == query.EventCode)
            .WhereIF(query.IsSuccess.HasValue, x => x.IsSuccess == query.IsSuccess!.Value)
            .WhereIF(query.StartTime.HasValue, x => x.CreateTime >= query.StartTime!.Value)
            .WhereIF(query.EndTime.HasValue, x => x.CreateTime <= query.EndTime!.Value);
        var total = await dbQuery.CountAsync();
        var items = await dbQuery.OrderByDescending(x => x.CreateTime)
            .Skip((page - 1) * pageSize).Take(pageSize).Select(x => new AuditLogDto
            {
                Id = x.Id, UserId = x.UserId, UserName = x.UserName, Action = x.Action,
                Module = x.Module, Description = x.Description, EventCode = x.EventCode, HttpMethod = x.HttpMethod,
                RequestPath = x.RequestPath, TargetId = x.TargetId, DetailJson = x.DetailJson,
                IpAddress = x.IpAddress, IsSuccess = x.IsSuccess, StatusCode = x.StatusCode,
                DurationMs = x.DurationMs, CreateTime = x.CreateTime
            }).ToListAsync();
        return new PagedResult<AuditLogDto> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }
}
