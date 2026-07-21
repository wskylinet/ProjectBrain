using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBrain.Api.Common;
using ProjectBrain.Api.Dtos;
using ProjectBrain.Api.Services;

namespace ProjectBrain.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    public async Task<ApiResult<PagedResult<UserDto>>> GetList([FromQuery] UserQuery query) =>
        ApiResult<PagedResult<UserDto>>.Ok(await _userService.GetUsersAsync(query));

    [HttpPost]
    public async Task<ActionResult<ApiResult<UserDto>>> Create([FromBody] UserCreateRequest request)
    {
        var result = await _userService.CreateUserAsync(request);
        return result.Data is null ? BadRequest(ApiResult<UserDto>.Fail(result.Error ?? "创建失败"))
            : Ok(ApiResult<UserDto>.Ok(result.Data, "创建成功"));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResult<UserDto>>> Update(long id, [FromBody] UserUpdateRequest request)
    {
        var result = await _userService.UpdateUserAsync(id, CurrentUserId(), request);
        return result.Data is null ? BadRequest(ApiResult<UserDto>.Fail(result.Error ?? "保存失败"))
            : Ok(ApiResult<UserDto>.Ok(result.Data, "保存成功"));
    }

    [HttpPut("{id:long}/password")]
    public async Task<ActionResult<ApiResult<object?>>> ResetPassword(long id, [FromBody] ResetPasswordRequest request)
    {
        var result = await _userService.ResetPasswordAsync(id, request);
        return result.Success ? Ok(ApiResult.Ok("密码已重置"))
            : BadRequest(ApiResult.Fail(result.Error ?? "重置失败"));
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResult<object?>>> Delete(long id)
    {
        var result = await _userService.DeleteUserAsync(id, CurrentUserId());
        return result.Success ? Ok(ApiResult.Ok("删除成功"))
            : BadRequest(ApiResult.Fail(result.Error ?? "删除失败"));
    }

    private long CurrentUserId() =>
        long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
