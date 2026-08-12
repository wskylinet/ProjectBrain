using ProjectBrain.Api.Entities;
using ProjectBrain.Api.Security;
using SqlSugar;

namespace ProjectBrain.Api.Data;

public class DbContext
{
    public ISqlSugarClient Db { get; }

    public DbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("缺少数据库连接字符串 ConnectionStrings:Default");
        Db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = DbType.SqlServer,
            IsAutoCloseConnection = true,
            ConfigureExternalServices = new ConfigureExternalServices()
        });
    }

    public void InitDatabase()
    {
        Db.DbMaintenance.CreateDatabase();
        Db.CodeFirst.InitTables<SysUser, SysRole, SysPermission, SysUserRole, SysRolePermission>();
        Db.CodeFirst.InitTables<SysAuditLog>();
        Db.CodeFirst.InitTables<ProjectInfo, ProjectConnection, ProjectConnectionRemoteControl>();
        Db.CodeFirst.InitTables<ProjectContact, ProjectApplication, ProjectConnectionApplication>();

        SeedPermissions();
        SeedRoles();
    }

    public void CreateInitialAdmin(string userName, string password)
    {
        userName = userName.Trim();
        if (userName.Length is < 2 or > 50)
            throw new InvalidOperationException("初始管理员用户名长度必须为 2 到 50 个字符。");
        if (password.Length is < 12 or > 100)
            throw new InvalidOperationException("初始管理员密码长度必须为 12 到 100 个字符。");
        if (Db.Queryable<SysUser>().Any())
            throw new InvalidOperationException("用户表不为空，拒绝创建初始管理员。请使用系统内的用户管理功能。");

        var adminRoleId = Db.Queryable<SysRole>()
            .Where(x => x.Code == "Admin" && !x.IsDeleted)
            .Select(x => x.Id)
            .First();
        if (adminRoleId == 0)
            throw new InvalidOperationException("管理员角色不存在，请先完成数据库初始化。");

        var result = Db.Ado.UseTran(() =>
        {
            var userId = Db.Insertable(new SysUser
            {
                UserName = userName,
                NickName = "超级管理员",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsEnabled = true,
                CreateTime = DateTime.Now
            }).ExecuteReturnBigIdentity();
            Db.Insertable(new SysUserRole { UserId = userId, RoleId = adminRoleId }).ExecuteCommand();
        });
        if (!result.IsSuccess)
            throw new InvalidOperationException("创建初始管理员失败。", result.ErrorException);
    }

    private void SeedPermissions()
    {
        foreach (var item in PermissionCodes.All)
        {
            if (Db.Queryable<SysPermission>().Any(x => x.Code == item.Code)) continue;
            Db.Insertable(new SysPermission
            {
                Code = item.Code, Name = item.Name, Module = item.Module, Sort = item.Sort
            }).ExecuteCommand();
        }
    }

    private void SeedRoles()
    {
        var definitions = new[]
        {
            new RoleSeed("Admin", "管理员", "拥有全部权限", PermissionCodes.All.Select(x => x.Code).ToArray()),
            new RoleSeed("Maintainer", "维护人员", "维护档案和密码信息", new[]
            {
                PermissionCodes.ArchiveView, PermissionCodes.ArchiveCreate, PermissionCodes.ArchiveUpdate,
                PermissionCodes.SecretReveal, PermissionCodes.SecretUpdate
            }),
            new RoleSeed("Member", "普通成员", "查看并修改普通档案", new[]
            {
                PermissionCodes.ArchiveView, PermissionCodes.ArchiveUpdate
            }),
            new RoleSeed("Reader", "只读用户", "仅查看普通档案", new[] { PermissionCodes.ArchiveView })
        };

        foreach (var definition in definitions)
        {
            var role = Db.Queryable<SysRole>().First(x => x.Code == definition.Code);
            if (role is null)
            {
                role = new SysRole
                {
                    Code = definition.Code, Name = definition.Name, Description = definition.Description,
                    IsSystem = true, CreateTime = DateTime.Now
                };
                role.Id = Db.Insertable(role).ExecuteReturnBigIdentity();
            }

            var permissionIds = Db.Queryable<SysPermission>()
                .Where(x => definition.Permissions.Contains(x.Code)).Select(x => x.Id).ToList();
            foreach (var permissionId in permissionIds)
            {
                if (!Db.Queryable<SysRolePermission>().Any(x => x.RoleId == role.Id && x.PermissionId == permissionId))
                    Db.Insertable(new SysRolePermission { RoleId = role.Id, PermissionId = permissionId }).ExecuteCommand();
            }
        }
    }

    private sealed record RoleSeed(string Code, string Name, string Description, string[] Permissions);
}
