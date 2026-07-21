using ProjectBrain.Api.Auth;
using ProjectBrain.Api.Data;
using ProjectBrain.Api.Dtos;
using ProjectBrain.Api.Entities;

namespace ProjectBrain.Api.Services;

public class UserService : IUserService
{
    private readonly DbContext _dbContext;
    private readonly JwtHelper _jwtHelper;
    private readonly IPermissionService _permissionService;

    public UserService(DbContext dbContext, JwtHelper jwtHelper, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _jwtHelper = jwtHelper;
        _permissionService = permissionService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _dbContext.Db.Queryable<SysUser>()
            .FirstAsync(x => x.UserName == request.UserName && !x.IsDeleted);
        if (user is null || !user.IsEnabled || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null;
        return new LoginResponse { Token = _jwtHelper.GenerateToken(user), User = await MapUserInfoAsync(user) };
    }

    public async Task<UserInfoDto?> GetByIdAsync(long id)
    {
        var user = await _dbContext.Db.Queryable<SysUser>()
            .FirstAsync(x => x.Id == id && x.IsEnabled && !x.IsDeleted);
        return user is null ? null : await MapUserInfoAsync(user);
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(UserQuery query)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var keyword = query.Keyword?.Trim();
        var dbQuery = _dbContext.Db.Queryable<SysUser>().Where(x => !x.IsDeleted)
            .WhereIF(!string.IsNullOrEmpty(keyword), x => x.UserName.Contains(keyword!) || (x.NickName != null && x.NickName.Contains(keyword!)))
            .WhereIF(query.IsEnabled.HasValue, x => x.IsEnabled == query.IsEnabled!.Value);
        var total = await dbQuery.CountAsync();
        var entities = await dbQuery.OrderByDescending(x => x.CreateTime)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var items = new List<UserDto>();
        foreach (var entity in entities) items.Add(await MapUserAsync(entity));
        return new PagedResult<UserDto> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<(UserDto? Data, string? Error)> CreateUserAsync(UserCreateRequest request)
    {
        var userName = request.UserName.Trim();
        if (await _dbContext.Db.Queryable<SysUser>().AnyAsync(x => x.UserName == userName && !x.IsDeleted))
            return (null, "登录账号已存在");
        var roleIds = await ResolveRoleIdsAsync(request.RoleIds);
        if (roleIds.Count == 0) return (null, "请选择有效角色");
        var entity = new SysUser
        {
            UserName = userName, NickName = Normalize(request.NickName),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsEnabled = request.IsEnabled, CreateTime = DateTime.Now
        };
        entity.Id = await _dbContext.Db.Insertable(entity).ExecuteReturnBigIdentityAsync();
        await ReplaceRolesAsync(entity.Id, roleIds);
        return (await MapUserAsync(entity), null);
    }

    public async Task<(UserDto? Data, string? Error)> UpdateUserAsync(long id, long currentUserId, UserUpdateRequest request)
    {
        var entity = await _dbContext.Db.Queryable<SysUser>().FirstAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return (null, "用户不存在");
        if (id == currentUserId && !request.IsEnabled) return (null, "不能停用当前登录账号");
        var roleIds = await ResolveRoleIdsAsync(request.RoleIds);
        if (roleIds.Count == 0) return (null, "请选择有效角色");
        if (id == currentUserId && !await ContainsAdminRoleAsync(roleIds)) return (null, "不能移除当前管理员账号的管理员角色");
        entity.NickName = Normalize(request.NickName);
        entity.IsEnabled = request.IsEnabled;
        entity.UpdateTime = DateTime.Now;
        await _dbContext.Db.Updateable(entity).ExecuteCommandAsync();
        await ReplaceRolesAsync(id, roleIds);
        return (await MapUserAsync(entity), null);
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(long id, ResetPasswordRequest request)
    {
        var entity = await _dbContext.Db.Queryable<SysUser>().FirstAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return (false, "用户不存在");
        entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        entity.UpdateTime = DateTime.Now;
        await _dbContext.Db.Updateable(entity).ExecuteCommandAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteUserAsync(long id, long currentUserId)
    {
        if (id == currentUserId) return (false, "不能删除当前登录账号");
        var affected = await _dbContext.Db.Updateable<SysUser>().SetColumns(x => x.IsDeleted == true)
            .SetColumns(x => x.UpdateTime == DateTime.Now).Where(x => x.Id == id && !x.IsDeleted).ExecuteCommandAsync();
        return affected == 0 ? (false, "用户不存在") : (true, null);
    }

    private async Task<UserInfoDto> MapUserInfoAsync(SysUser user)
    {
        var access = await _permissionService.GetUserAccessAsync(user.Id);
        return new UserInfoDto
        {
            Id = user.Id, UserName = user.UserName, NickName = user.NickName,
            RoleCodes = access.Roles.Select(x => x.Code).ToList(),
            RoleNames = access.Roles.Select(x => x.Name).ToList(), Permissions = access.Permissions
        };
    }

    private async Task<UserDto> MapUserAsync(SysUser user)
    {
        var access = await _permissionService.GetUserAccessAsync(user.Id);
        return new UserDto
        {
            Id = user.Id, UserName = user.UserName, NickName = user.NickName, IsEnabled = user.IsEnabled,
            RoleIds = access.Roles.Select(x => x.Id).ToList(), RoleNames = access.Roles.Select(x => x.Name).ToList(),
            CreateTime = user.CreateTime, UpdateTime = user.UpdateTime
        };
    }

    private async Task<List<long>> ResolveRoleIdsAsync(List<long> requested)
    {
        if (requested.Count > 0)
            return await _dbContext.Db.Queryable<SysRole>().Where(x => requested.Contains(x.Id) && !x.IsDeleted).Select(x => x.Id).ToListAsync();
        var reader = await _dbContext.Db.Queryable<SysRole>().Where(x => x.Code == "Reader" && !x.IsDeleted).Select(x => x.Id).FirstAsync();
        return reader == 0 ? new List<long>() : new List<long> { reader };
    }

    private async Task ReplaceRolesAsync(long userId, List<long> roleIds)
    {
        await _dbContext.Db.Deleteable<SysUserRole>().Where(x => x.UserId == userId).ExecuteCommandAsync();
        await _dbContext.Db.Insertable(roleIds.Distinct().Select(x => new SysUserRole { UserId = userId, RoleId = x }).ToList()).ExecuteCommandAsync();
    }

    private async Task<bool> ContainsAdminRoleAsync(List<long> roleIds) =>
        await _dbContext.Db.Queryable<SysRole>().AnyAsync(x => roleIds.Contains(x.Id) && x.Code == "Admin" && !x.IsDeleted);

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
