using SqlSugar;

namespace ProjectBrain.Api.Entities;

[SugarTable("SysRole")]
public class SysRole
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    [SugarColumn(Length = 50)] public string Code { get; set; } = string.Empty;
    [SugarColumn(Length = 50)] public string Name { get; set; } = string.Empty;
    [SugarColumn(Length = 200, IsNullable = true)] public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; }
}
