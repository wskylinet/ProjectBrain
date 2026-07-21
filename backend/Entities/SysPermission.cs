using SqlSugar;

namespace ProjectBrain.Api.Entities;

[SugarTable("SysPermission")]
public class SysPermission
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    [SugarColumn(Length = 80)] public string Code { get; set; } = string.Empty;
    [SugarColumn(Length = 50)] public string Name { get; set; } = string.Empty;
    [SugarColumn(Length = 30)] public string Module { get; set; } = string.Empty;
    public int Sort { get; set; }
}
