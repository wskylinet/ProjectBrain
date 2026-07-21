using SqlSugar;

namespace ProjectBrain.Api.Entities;

[SugarTable("SysUserRole")]
public class SysUserRole
{
    [SugarColumn(IsPrimaryKey = true)] public long UserId { get; set; }
    [SugarColumn(IsPrimaryKey = true)] public long RoleId { get; set; }
}
