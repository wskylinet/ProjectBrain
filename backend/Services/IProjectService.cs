using ProjectBrain.Api.Dtos;

namespace ProjectBrain.Api.Services;

public interface IProjectService
{
    Task<PagedResult<ProjectDto>> GetProjectsAsync(ProjectQuery query);
    Task<ProjectDto?> GetProjectAsync(long id);
    Task<List<ProjectApplicationDto>> GetApplicationsAsync(long projectId);
    Task<(ProjectApplicationDto? Data, string? Error)> CreateApplicationAsync(long projectId, ApplicationSaveRequest request);
    Task<(ProjectApplicationDto? Data, string? Error)> UpdateApplicationAsync(long projectId, long id, ApplicationSaveRequest request);
    Task<(bool Success, string? Error)> DeleteApplicationAsync(long projectId, long id);
    Task<(string? Password, string? Error)> RevealApplicationPasswordAsync(long projectId, long id);
    Task<List<ProjectContactDto>> GetContactsAsync(long projectId);
    Task<(ProjectContactDto? Data, string? Error)> CreateContactAsync(long projectId, ContactSaveRequest request);
    Task<(ProjectContactDto? Data, string? Error)> UpdateContactAsync(long projectId, long id, ContactSaveRequest request);
    Task<bool> DeleteContactAsync(long projectId, long id);
    Task<ProjectDto> CreateProjectAsync(ProjectSaveRequest request);
    Task<ProjectDto?> UpdateProjectAsync(long id, ProjectSaveRequest request);
    Task<bool> DeleteProjectAsync(long id);
    Task<List<ProjectConnectionDto>> GetConnectionsAsync(long projectId);
    Task<(ProjectConnectionDto? Data, string? Error)> CreateConnectionAsync(long projectId, ConnectionSaveRequest request);
    Task<(ProjectConnectionDto? Data, string? Error)> UpdateConnectionAsync(long projectId, long id, ConnectionSaveRequest request);
    Task<(bool Success, string? Error)> DeleteConnectionAsync(long projectId, long id);
    Task<(string? Password, string? Error)> RevealPasswordAsync(long projectId, long id);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
