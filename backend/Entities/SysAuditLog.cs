using SqlSugar;

namespace ProjectBrain.Api.Entities;

[SugarTable("SysAuditLog")]
[SugarIndex("IX_SysAuditLog_CreateTime", nameof(CreateTime), OrderByType.Desc)]
[SugarIndex("IX_SysAuditLog_UserId", nameof(UserId), OrderByType.Asc)]
public class SysAuditLog
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }
    [SugarColumn(IsNullable = true)] public long? UserId { get; set; }
    [SugarColumn(Length = 50, IsNullable = true)] public string? UserName { get; set; }
    [SugarColumn(Length = 30)] public string Action { get; set; } = string.Empty;
    [SugarColumn(Length = 50)] public string Module { get; set; } = string.Empty;
    [SugarColumn(Length = 200)] public string Description { get; set; } = string.Empty;
    [SugarColumn(Length = 10)] public string HttpMethod { get; set; } = string.Empty;
    [SugarColumn(Length = 500)] public string RequestPath { get; set; } = string.Empty;
    [SugarColumn(Length = 100, IsNullable = true)] public string? TargetId { get; set; }
    [SugarColumn(ColumnDataType = "nvarchar(max)", IsNullable = true)] public string? DetailJson { get; set; }
    [SugarColumn(Length = 64, IsNullable = true)] public string? IpAddress { get; set; }
    [SugarColumn(Length = 500, IsNullable = true)] public string? UserAgent { get; set; }
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;
}
