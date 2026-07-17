using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBrain.Api.Common;
using ProjectBrain.Api.Dtos;
using ProjectBrain.Api.Services;

namespace ProjectBrain.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IProjectService _projectService;

    public DashboardController(IProjectService projectService) => _projectService = projectService;

    [HttpGet("stats")]
    public async Task<ApiResult<DashboardStatsDto>> GetStats() =>
        ApiResult<DashboardStatsDto>.Ok(await _projectService.GetDashboardStatsAsync());
}
