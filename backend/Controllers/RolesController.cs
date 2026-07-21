using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBrain.Api.Common;
using ProjectBrain.Api.Dtos;
using ProjectBrain.Api.Services;

namespace ProjectBrain.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    public RolesController(IPermissionService permissionService) => _permissionService = permissionService;

    [HttpGet]
    public async Task<ApiResult<List<RoleOptionDto>>> GetList() =>
        ApiResult<List<RoleOptionDto>>.Ok(await _permissionService.GetRolesAsync());
}
