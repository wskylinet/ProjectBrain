using ProjectBrain.Api.Dtos;

namespace ProjectBrain.Api.Services;

public interface IUserService
{
    /// <summary>
    /// 校验账号密码并返回登录结果。失败时返回 null。
    /// </summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request);

    /// <summary>
    /// 根据用户主键获取用户信息。
    /// </summary>
    Task<UserInfoDto?> GetByIdAsync(long id);
}
