using ProjectBrain.Api.Dtos;

namespace ProjectBrain.Api.Services;

public interface IUserService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<UserInfoDto?> GetByIdAsync(long id);
    Task<PagedResult<UserDto>> GetUsersAsync(UserQuery query);
    Task<(UserDto? Data, string? Error)> CreateUserAsync(UserCreateRequest request);
    Task<(UserDto? Data, string? Error)> UpdateUserAsync(long id, long currentUserId, UserUpdateRequest request);
    Task<(bool Success, string? Error)> ResetPasswordAsync(long id, ResetPasswordRequest request);
    Task<(bool Success, string? Error)> DeleteUserAsync(long id, long currentUserId);
}
