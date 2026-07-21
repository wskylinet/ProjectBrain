using ProjectBrain.Api.Data;
using ProjectBrain.Api.Dtos;
using ProjectBrain.Api.Entities;
using SqlSugar;

namespace ProjectBrain.Api.Services;

public class PermissionService : IPermissionService
{
    private readonly DbContext _dbContext;
    public PermissionService(DbContext dbContext) => _dbContext = dbContext;

    public async Task<UserAccessDto> GetUserAccessAsync(long userId)
    {
        var enabled = await _dbContext.Db.Queryable<SysUser>()
            .AnyAsync(x => x.Id == userId && x.IsEnabled && !x.IsDeleted);
        if (!enabled) return new UserAccessDto();

        var roles = await _dbContext.Db.Queryable<SysUserRole, SysRole>((ur, role) =>
                new JoinQueryInfos(JoinType.Inner, ur.RoleId == role.Id))
            .Where((ur, role) => ur.UserId == userId && !role.IsDeleted)
            .Select((ur, role) => new RoleOptionDto
            {
                Id = role.Id, Code = role.Code, Name = role.Name, Description = role.Description
            }).ToListAsync();

        var permissions = await _dbContext.Db.Queryable<SysUserRole, SysRole, SysRolePermission, SysPermission>(
                (ur, role, rp, permission) => new JoinQueryInfos(
                    JoinType.Inner, ur.RoleId == role.Id,
                    JoinType.Inner, ur.RoleId == rp.RoleId,
                    JoinType.Inner, rp.PermissionId == permission.Id))
            .Where((ur, role, rp, permission) => ur.UserId == userId && !role.IsDeleted)
            .Select((ur, role, rp, permission) => permission.Code)
            .Distinct().ToListAsync();

        return new UserAccessDto { Roles = roles, Permissions = permissions };
    }

    public async Task<bool> HasPermissionAsync(long userId, string permission)
    {
        var access = await GetUserAccessAsync(userId);
        return access.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<RoleOptionDto>> GetRolesAsync() =>
        await _dbContext.Db.Queryable<SysRole>().Where(x => !x.IsDeleted)
            .OrderBy(x => x.Id)
            .Select(x => new RoleOptionDto
            {
                Id = x.Id, Code = x.Code, Name = x.Name, Description = x.Description
            }).ToListAsync();
}
