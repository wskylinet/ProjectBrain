using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBrain.Api.Common;
using ProjectBrain.Api.Dtos;
using ProjectBrain.Api.Services;

namespace ProjectBrain.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService) => _projectService = projectService;

    [HttpGet]
    public async Task<ApiResult<PagedResult<ProjectDto>>> GetList([FromQuery] ProjectQuery query) =>
        ApiResult<PagedResult<ProjectDto>>.Ok(await _projectService.GetProjectsAsync(query));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResult<ProjectDto>>> Get(long id)
    {
        var project = await _projectService.GetProjectAsync(id);
        return project is null
            ? NotFound(ApiResult<ProjectDto>.Fail("项目不存在", 404))
            : Ok(ApiResult<ProjectDto>.Ok(project));
    }

    [HttpPost]
    public async Task<ApiResult<ProjectDto>> Create([FromBody] ProjectSaveRequest request) =>
        ApiResult<ProjectDto>.Ok(await _projectService.CreateProjectAsync(request), "创建成功");

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResult<ProjectDto>>> Update(long id, [FromBody] ProjectSaveRequest request)
    {
        var project = await _projectService.UpdateProjectAsync(id, request);
        return project is null
            ? NotFound(ApiResult<ProjectDto>.Fail("项目不存在", 404))
            : Ok(ApiResult<ProjectDto>.Ok(project, "保存成功"));
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResult<object?>>> Delete(long id) =>
        await _projectService.DeleteProjectAsync(id)
            ? Ok(ApiResult.Ok("删除成功"))
            : NotFound(ApiResult.Fail("项目不存在", 404));

    [HttpGet("{projectId:long}/applications")]
    public async Task<ApiResult<List<ProjectApplicationDto>>> GetApplications(long projectId) =>
        ApiResult<List<ProjectApplicationDto>>.Ok(await _projectService.GetApplicationsAsync(projectId));

    [HttpPost("{projectId:long}/applications")]
    public async Task<ActionResult<ApiResult<ProjectApplicationDto>>> CreateApplication(
        long projectId, [FromBody] ApplicationSaveRequest request)
    {
        var result = await _projectService.CreateApplicationAsync(projectId, request);
        return result.Data is null
            ? BadRequest(ApiResult<ProjectApplicationDto>.Fail(result.Error ?? "创建失败"))
            : Ok(ApiResult<ProjectApplicationDto>.Ok(result.Data, "创建成功"));
    }

    [HttpPut("{projectId:long}/applications/{id:long}")]
    public async Task<ActionResult<ApiResult<ProjectApplicationDto>>> UpdateApplication(
        long projectId, long id, [FromBody] ApplicationSaveRequest request)
    {
        var result = await _projectService.UpdateApplicationAsync(projectId, id, request);
        return result.Data is null
            ? BadRequest(ApiResult<ProjectApplicationDto>.Fail(result.Error ?? "保存失败"))
            : Ok(ApiResult<ProjectApplicationDto>.Ok(result.Data, "保存成功"));
    }

    [HttpDelete("{projectId:long}/applications/{id:long}")]
    public async Task<ActionResult<ApiResult<object?>>> DeleteApplication(long projectId, long id)
    {
        var result = await _projectService.DeleteApplicationAsync(projectId, id);
        return result.Success
            ? Ok(ApiResult.Ok("删除成功"))
            : BadRequest(ApiResult.Fail(result.Error ?? "删除失败"));
    }

    [HttpPost("{projectId:long}/applications/{id:long}/reveal-password")]
    public async Task<ActionResult<ApiResult<RevealPasswordDto>>> RevealApplicationPassword(
        long projectId, long id)
    {
        Response.Headers.CacheControl = "no-store";
        var result = await _projectService.RevealApplicationPasswordAsync(projectId, id);
        return result.Password is null
            ? BadRequest(ApiResult<RevealPasswordDto>.Fail(result.Error ?? "无法读取密码"))
            : Ok(ApiResult<RevealPasswordDto>.Ok(new RevealPasswordDto { Password = result.Password }));
    }

    [HttpGet("{projectId:long}/contacts")]
    public async Task<ApiResult<List<ProjectContactDto>>> GetContacts(long projectId) =>
        ApiResult<List<ProjectContactDto>>.Ok(await _projectService.GetContactsAsync(projectId));

    [HttpPost("{projectId:long}/contacts")]
    public async Task<ActionResult<ApiResult<ProjectContactDto>>> CreateContact(
        long projectId, [FromBody] ContactSaveRequest request)
    {
        var result = await _projectService.CreateContactAsync(projectId, request);
        return result.Data is null
            ? BadRequest(ApiResult<ProjectContactDto>.Fail(result.Error ?? "创建失败"))
            : Ok(ApiResult<ProjectContactDto>.Ok(result.Data, "创建成功"));
    }

    [HttpPut("{projectId:long}/contacts/{id:long}")]
    public async Task<ActionResult<ApiResult<ProjectContactDto>>> UpdateContact(
        long projectId, long id, [FromBody] ContactSaveRequest request)
    {
        var result = await _projectService.UpdateContactAsync(projectId, id, request);
        return result.Data is null
            ? BadRequest(ApiResult<ProjectContactDto>.Fail(result.Error ?? "保存失败"))
            : Ok(ApiResult<ProjectContactDto>.Ok(result.Data, "保存成功"));
    }

    [HttpDelete("{projectId:long}/contacts/{id:long}")]
    public async Task<ActionResult<ApiResult<object?>>> DeleteContact(long projectId, long id) =>
        await _projectService.DeleteContactAsync(projectId, id)
            ? Ok(ApiResult.Ok("删除成功"))
            : NotFound(ApiResult.Fail("项目人员不存在", 404));

    [HttpGet("{projectId:long}/connections")]
    public async Task<ApiResult<List<ProjectConnectionDto>>> GetConnections(long projectId) =>
        ApiResult<List<ProjectConnectionDto>>.Ok(await _projectService.GetConnectionsAsync(projectId));

    [HttpPost("{projectId:long}/connections")]
    public async Task<ActionResult<ApiResult<ProjectConnectionDto>>> CreateConnection(
        long projectId, [FromBody] ConnectionSaveRequest request)
    {
        var result = await _projectService.CreateConnectionAsync(projectId, request);
        return result.Data is null
            ? BadRequest(ApiResult<ProjectConnectionDto>.Fail(result.Error ?? "创建失败"))
            : Ok(ApiResult<ProjectConnectionDto>.Ok(result.Data, "创建成功"));
    }

    [HttpPut("{projectId:long}/connections/{id:long}")]
    public async Task<ActionResult<ApiResult<ProjectConnectionDto>>> UpdateConnection(
        long projectId, long id, [FromBody] ConnectionSaveRequest request)
    {
        var result = await _projectService.UpdateConnectionAsync(projectId, id, request);
        return result.Data is null
            ? BadRequest(ApiResult<ProjectConnectionDto>.Fail(result.Error ?? "保存失败"))
            : Ok(ApiResult<ProjectConnectionDto>.Ok(result.Data, "保存成功"));
    }

    [HttpDelete("{projectId:long}/connections/{id:long}")]
    public async Task<ActionResult<ApiResult<object?>>> DeleteConnection(long projectId, long id)
    {
        var result = await _projectService.DeleteConnectionAsync(projectId, id);
        return result.Success
            ? Ok(ApiResult.Ok("删除成功"))
            : BadRequest(ApiResult.Fail(result.Error ?? "删除失败"));
    }

    [HttpPost("{projectId:long}/connections/{id:long}/reveal-password")]
    public async Task<ActionResult<ApiResult<RevealPasswordDto>>> RevealPassword(long projectId, long id)
    {
        Response.Headers.CacheControl = "no-store";
        var result = await _projectService.RevealPasswordAsync(projectId, id);
        return result.Password is null
            ? BadRequest(ApiResult<RevealPasswordDto>.Fail(result.Error ?? "无法读取密码"))
            : Ok(ApiResult<RevealPasswordDto>.Ok(new RevealPasswordDto { Password = result.Password }));
    }

    [HttpPost("{projectId:long}/connections/{connectionId:long}/remote-controls/{remoteControlId:long}/reveal-password")]
    public async Task<ActionResult<ApiResult<RevealPasswordDto>>> RevealRemoteControlPassword(
        long projectId, long connectionId, long remoteControlId)
    {
        Response.Headers.CacheControl = "no-store";
        var result = await _projectService.RevealRemoteControlPasswordAsync(projectId, connectionId, remoteControlId);
        return result.Password is null
            ? BadRequest(ApiResult<RevealPasswordDto>.Fail(result.Error ?? "无法读取密码"))
            : Ok(ApiResult<RevealPasswordDto>.Ok(new RevealPasswordDto { Password = result.Password }));
    }
}
