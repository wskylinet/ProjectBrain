using ProjectBrain.Api.Dtos;

namespace ProjectBrain.Api.Services;

public interface IPermissionService
{
    Task<UserAccessDto> GetUserAccessAsync(long userId);
    Task<bool> HasPermissionAsync(long userId, string permission);
    Task<List<RoleOptionDto>> GetRolesAsync();
}
