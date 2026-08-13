using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProjectBrain.Api.Common;
using ProjectBrain.Api.Dtos;
using ProjectBrain.Api.Security;
using ProjectBrain.Api.Services;

namespace ProjectBrain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILoginAttemptTracker _loginAttemptTracker;

    public AuthController(IUserService userService, ILoginAttemptTracker loginAttemptTracker)
    {
        _userService = userService;
        _loginAttemptTracker = loginAttemptTracker;
    }

    /// <summary>
    /// 账号密码登录，成功返回 JWT 令牌。
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("Login")]
    public async Task<ActionResult<ApiResult<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        if (_loginAttemptTracker.IsBlocked(request.UserName))
        {
            HttpContext.Items[SecurityEventCodes.HttpContextItemKey] = SecurityEventCodes.AccountTemporarilyLocked;
            return Unauthorized(ApiResult<LoginResponse>.Fail("用户名或密码错误", StatusCodes.Status401Unauthorized));
        }

        var result = await _userService.LoginAsync(request);
        if (result is null)
        {
            _loginAttemptTracker.RecordFailure(request.UserName);
            HttpContext.Items[SecurityEventCodes.HttpContextItemKey] = SecurityEventCodes.InvalidCredentials;
            return Unauthorized(ApiResult<LoginResponse>.Fail("用户名或密码错误", StatusCodes.Status401Unauthorized));
        }

        _loginAttemptTracker.Reset(request.UserName);
        return Ok(ApiResult<LoginResponse>.Ok(result));
    }

    /// <summary>
    /// 获取当前登录用户信息（需携带有效令牌）。
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResult<UserInfoDto>>> Me()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(idValue, out var id))
        {
            return Unauthorized(ApiResult<UserInfoDto>.Fail("无效的登录态", StatusCodes.Status401Unauthorized));
        }

        var user = await _userService.GetByIdAsync(id);
        return user is null
            ? Unauthorized(ApiResult<UserInfoDto>.Fail("用户不存在", StatusCodes.Status401Unauthorized))
            : Ok(ApiResult<UserInfoDto>.Ok(user));
    }
}
