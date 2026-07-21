using SqlSugar;

namespace ProjectBrain.Api.Entities;

[SugarTable("SysRolePermission")]
public class SysRolePermission
{
    [SugarColumn(IsPrimaryKey = true)] public long RoleId { get; set; }
    [SugarColumn(IsPrimaryKey = true)] public long PermissionId { get; set; }
}
