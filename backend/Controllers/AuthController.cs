using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBrain.Api.Common;
using ProjectBrain.Api.Dtos;
using ProjectBrain.Api.Services;

namespace ProjectBrain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 账号密码登录，成功返回 JWT 令牌。
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ApiResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _userService.LoginAsync(request);
        return result is null
            ? ApiResult<LoginResponse>.Fail("用户名或密码错误")
            : ApiResult<LoginResponse>.Ok(result);
    }

    /// <summary>
    /// 获取当前登录用户信息（需携带有效令牌）。
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ApiResult<UserInfoDto>> Me()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(idValue, out var id))
        {
            return ApiResult<UserInfoDto>.Fail("无效的登录态", 401);
        }

        var user = await _userService.GetByIdAsync(id);
        return user is null
            ? ApiResult<UserInfoDto>.Fail("用户不存在", 401)
            : ApiResult<UserInfoDto>.Ok(user);
    }
}
