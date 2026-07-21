namespace ProjectBrain.Api.Security;

public static class PermissionCodes
{
    public const string ArchiveView = "archive:view";
    public const string ArchiveCreate = "archive:create";
    public const string ArchiveUpdate = "archive:update";
    public const string ArchiveDelete = "archive:delete";
    public const string SecretReveal = "secret:reveal";
    public const string SecretUpdate = "secret:update";
    public const string UserView = "user:view";
    public const string UserManage = "user:manage";
    public const string RoleManage = "role:manage";

    public static readonly (string Code, string Name, string Module, int Sort)[] All =
    [
        (ArchiveView, "查看档案", "档案", 10),
        (ArchiveCreate, "新增档案", "档案", 20),
        (ArchiveUpdate, "修改档案", "档案", 30),
        (ArchiveDelete, "删除档案", "档案", 40),
        (SecretReveal, "查看密码", "密码", 50),
        (SecretUpdate, "修改密码信息", "密码", 60),
        (UserView, "查看用户", "系统", 70),
        (UserManage, "管理用户", "系统", 80),
        (RoleManage, "管理角色权限", "系统", 90)
    ];
}
