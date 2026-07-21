namespace ProjectBrain.Api.Dtos;

public class AuditLogQuery
{
    public string? Keyword { get; set; }
    public string? Action { get; set; }
    public bool? IsSuccess { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AuditLogDto
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public string? DetailJson { get; set; }
    public string? IpAddress { get; set; }
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public DateTime CreateTime { get; set; }
}
