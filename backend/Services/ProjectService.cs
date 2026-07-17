using ProjectBrain.Api.Data;
using ProjectBrain.Api.Dtos;
using ProjectBrain.Api.Entities;
using ProjectBrain.Api.Security;

namespace ProjectBrain.Api.Services;

public class ProjectService : IProjectService
{
    private readonly DbContext _dbContext;
    private readonly ISecretCipher _secretCipher;

    public ProjectService(DbContext dbContext, ISecretCipher secretCipher)
    {
        _dbContext = dbContext;
        _secretCipher = secretCipher;
    }

    public async Task<PagedResult<ProjectDto>> GetProjectsAsync(ProjectQuery query)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var keyword = query.Keyword?.Trim();
        var region = query.Region?.Trim();

        var dbQuery = _dbContext.Db.Queryable<ProjectInfo>()
            .Where(x => !x.IsDeleted)
            .WhereIF(!string.IsNullOrEmpty(keyword), x =>
                x.Region != null && x.Region.Contains(keyword!))
            .WhereIF(!string.IsNullOrEmpty(region), x => x.Region == region);

        var total = await dbQuery.CountAsync();
        var items = await dbQuery.OrderByDescending(x => x.CreateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var projectDtos = items.Select(MapProject).ToList();
        await AttachApplicationsAsync(projectDtos);
        return new PagedResult<ProjectDto>
        {
            Items = projectDtos,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProjectDto?> GetProjectAsync(long id)
    {
        var entity = await _dbContext.Db.Queryable<ProjectInfo>()
            .FirstAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return null;
        var dto = MapProject(entity);
        dto.Applications = await GetApplicationsAsync(id);
        return dto;
    }

    public async Task<ProjectDto> CreateProjectAsync(ProjectSaveRequest request)
    {
        var entity = new ProjectInfo { CreateTime = DateTime.Now };
        ApplyProject(entity, request);
        entity.Id = await _dbContext.Db.Insertable(entity).ExecuteReturnBigIdentityAsync();
        return MapProject(entity);
    }

    public async Task<ProjectDto?> UpdateProjectAsync(long id, ProjectSaveRequest request)
    {
        var entity = await _dbContext.Db.Queryable<ProjectInfo>()
            .FirstAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return null;

        ApplyProject(entity, request);
        entity.UpdateTime = DateTime.Now;
        await _dbContext.Db.Updateable(entity).ExecuteCommandAsync();
        return MapProject(entity);
    }

    public async Task<bool> DeleteProjectAsync(long id)
    {
        var affected = await _dbContext.Db.Updateable<ProjectInfo>()
            .SetColumns(x => x.IsDeleted == true)
            .SetColumns(x => x.UpdateTime == DateTime.Now)
            .Where(x => x.Id == id && !x.IsDeleted)
            .ExecuteCommandAsync();
        if (affected == 0) return false;

        await _dbContext.Db.Updateable<ProjectConnection>()
            .SetColumns(x => x.IsDeleted == true)
            .SetColumns(x => x.UpdateTime == DateTime.Now)
            .Where(x => x.ProjectId == id && !x.IsDeleted)
            .ExecuteCommandAsync();
        await _dbContext.Db.Updateable<ProjectApplication>()
            .SetColumns(x => x.IsDeleted == true)
            .SetColumns(x => x.UpdateTime == DateTime.Now)
            .Where(x => x.ProjectId == id && !x.IsDeleted)
            .ExecuteCommandAsync();
        await _dbContext.Db.Updateable<ProjectContact>()
            .SetColumns(x => x.IsDeleted == true)
            .SetColumns(x => x.UpdateTime == DateTime.Now)
            .Where(x => x.ProjectId == id && !x.IsDeleted)
            .ExecuteCommandAsync();
        return true;
    }

    public async Task<List<ProjectApplicationDto>> GetApplicationsAsync(long projectId)
    {
        var items = await _dbContext.Db.Queryable<ProjectApplication>()
            .Where(x => x.ProjectId == projectId && !x.IsDeleted)
            .OrderBy(x => x.Sort).OrderBy(x => x.Id).ToListAsync();
        return items.Select(MapApplication).ToList();
    }

    public async Task<(ProjectApplicationDto? Data, string? Error)> CreateApplicationAsync(
        long projectId, ApplicationSaveRequest request)
    {
        if (!await ProjectExistsAsync(projectId)) return (null, "项目不存在");
        var entity = new ProjectApplication { ProjectId = projectId, CreateTime = DateTime.Now };
        ApplyApplication(entity, request);
        entity.Id = await _dbContext.Db.Insertable(entity).ExecuteReturnBigIdentityAsync();
        return (MapApplication(entity), null);
    }

    public async Task<(ProjectApplicationDto? Data, string? Error)> UpdateApplicationAsync(
        long projectId, long id, ApplicationSaveRequest request)
    {
        var entity = await _dbContext.Db.Queryable<ProjectApplication>()
            .FirstAsync(x => x.Id == id && x.ProjectId == projectId && !x.IsDeleted);
        if (entity is null) return (null, "系统不存在");
        ApplyApplication(entity, request);
        entity.UpdateTime = DateTime.Now;
        await _dbContext.Db.Updateable(entity).ExecuteCommandAsync();
        return (MapApplication(entity), null);
    }

    public async Task<(bool Success, string? Error)> DeleteApplicationAsync(long projectId, long id)
    {
        var exists = await _dbContext.Db.Queryable<ProjectApplication>()
            .AnyAsync(x => x.Id == id && x.ProjectId == projectId && !x.IsDeleted);
        if (!exists) return (false, "业务系统不存在");

        var hasConnections = await _dbContext.Db.Queryable<ProjectConnectionApplication>()
            .AnyAsync(x => x.ApplicationId == id);
        if (hasConnections) return (false, "请先取消连接信息与该业务系统的关联");

        var affected = await _dbContext.Db.Updateable<ProjectApplication>()
            .SetColumns(x => x.IsDeleted == true)
            .SetColumns(x => x.UpdateTime == DateTime.Now)
            .Where(x => x.Id == id && x.ProjectId == projectId && !x.IsDeleted)
            .ExecuteCommandAsync();
        return affected == 0 ? (false, "系统不存在") : (true, null);
    }

    public async Task<(string? Password, string? Error)> RevealApplicationPasswordAsync(long projectId, long id)
    {
        var encrypted = await _dbContext.Db.Queryable<ProjectApplication>()
            .Where(x => x.Id == id && x.ProjectId == projectId && !x.IsDeleted)
            .Select(x => x.PasswordEncrypted)
            .FirstAsync();
        if (string.IsNullOrEmpty(encrypted)) return (null, "未保存密码");
        return (_secretCipher.Decrypt(encrypted), null);
    }

    public async Task<List<ProjectContactDto>> GetContactsAsync(long projectId)
    {
        var items = await _dbContext.Db.Queryable<ProjectContact>()
            .Where(x => x.ProjectId == projectId && !x.IsDeleted)
            .OrderBy(x => x.Sort)
            .OrderBy(x => x.Id)
            .ToListAsync();
        return items.Select(MapContact).ToList();
    }

    public async Task<(ProjectContactDto? Data, string? Error)> CreateContactAsync(
        long projectId, ContactSaveRequest request)
    {
        if (!await ProjectExistsAsync(projectId)) return (null, "项目不存在");
        var entity = new ProjectContact { ProjectId = projectId, CreateTime = DateTime.Now };
        ApplyContact(entity, request);
        entity.Id = await _dbContext.Db.Insertable(entity).ExecuteReturnBigIdentityAsync();
        return (MapContact(entity), null);
    }

    public async Task<(ProjectContactDto? Data, string? Error)> UpdateContactAsync(
        long projectId, long id, ContactSaveRequest request)
    {
        var entity = await _dbContext.Db.Queryable<ProjectContact>()
            .FirstAsync(x => x.Id == id && x.ProjectId == projectId && !x.IsDeleted);
        if (entity is null) return (null, "项目人员不存在");
        ApplyContact(entity, request);
        entity.UpdateTime = DateTime.Now;
        await _dbContext.Db.Updateable(entity).ExecuteCommandAsync();
        return (MapContact(entity), null);
    }

    public async Task<bool> DeleteContactAsync(long projectId, long id)
    {
        var affected = await _dbContext.Db.Updateable<ProjectContact>()
            .SetColumns(x => x.IsDeleted == true)
            .SetColumns(x => x.UpdateTime == DateTime.Now)
            .Where(x => x.Id == id && x.ProjectId == projectId && !x.IsDeleted)
            .ExecuteCommandAsync();
        return affected > 0;
    }

    public async Task<List<ProjectConnectionDto>> GetConnectionsAsync(long projectId)
    {
        var items = await _dbContext.Db.Queryable<ProjectConnection>()
            .Where(x => x.ProjectId == projectId && !x.IsDeleted)
            .OrderBy(x => x.Sort)
            .OrderBy(x => x.Id)
            .ToListAsync();
        var names = items.ToDictionary(x => x.Id, x => x.Name);
        var connectionIds = items.Select(x => x.Id).ToList();
        var links = connectionIds.Count == 0
            ? new List<ProjectConnectionApplication>()
            : await _dbContext.Db.Queryable<ProjectConnectionApplication>()
                .Where(x => connectionIds.Contains(x.ConnectionId)).ToListAsync();
        var applicationIds = links.Select(x => x.ApplicationId).Distinct().ToList();
        var applications = applicationIds.Count == 0
            ? new List<ProjectApplication>()
            : await _dbContext.Db.Queryable<ProjectApplication>()
                .Where(x => applicationIds.Contains(x.Id) && !x.IsDeleted).ToListAsync();
        var applicationNames = applications.ToDictionary(x => x.Id, x => x.Name);
        var idsByConnection = links.GroupBy(x => x.ConnectionId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.ApplicationId).Distinct().ToList());

        return items.Select(x =>
        {
            var currentIds = idsByConnection.GetValueOrDefault(x.Id) ?? new List<long>();
            return MapConnection(
                x,
                x.ParentId.HasValue && names.TryGetValue(x.ParentId.Value, out var name) ? name : null,
                currentIds,
                currentIds.Where(applicationNames.ContainsKey).Select(id => applicationNames[id]));
        }).ToList();
    }

    public async Task<(ProjectConnectionDto? Data, string? Error)> CreateConnectionAsync(
        long projectId, ConnectionSaveRequest request)
    {
        if (!await ProjectExistsAsync(projectId)) return (null, "项目不存在");
        var parentError = await ValidateParentAsync(projectId, request.ParentId, null);
        if (parentError is not null) return (null, parentError);
        var applicationError = await ValidateApplicationsAsync(projectId, request.ApplicationIds);
        if (applicationError is not null) return (null, applicationError);

        var entity = new ProjectConnection
        {
            ProjectId = projectId,
            CreateTime = DateTime.Now
        };
        ApplyConnection(entity, request);
        entity.Id = await _dbContext.Db.Insertable(entity).ExecuteReturnBigIdentityAsync();
        await ReplaceConnectionApplicationsAsync(entity.Id, request.ApplicationIds);
        var applications = await GetApplicationsByIdsAsync(request.ApplicationIds);
        return (MapConnection(entity, null, applications.Select(x => x.Id), applications.Select(x => x.Name)), null);
    }

    public async Task<(ProjectConnectionDto? Data, string? Error)> UpdateConnectionAsync(
        long projectId, long id, ConnectionSaveRequest request)
    {
        var entity = await _dbContext.Db.Queryable<ProjectConnection>()
            .FirstAsync(x => x.Id == id && x.ProjectId == projectId && !x.IsDeleted);
        if (entity is null) return (null, "连接信息不存在");

        var parentError = await ValidateParentAsync(projectId, request.ParentId, id);
        if (parentError is not null) return (null, parentError);
        var applicationError = await ValidateApplicationsAsync(projectId, request.ApplicationIds);
        if (applicationError is not null) return (null, applicationError);

        ApplyConnection(entity, request);
        entity.UpdateTime = DateTime.Now;
        await _dbContext.Db.Updateable(entity).ExecuteCommandAsync();
        await ReplaceConnectionApplicationsAsync(entity.Id, request.ApplicationIds);

        string? parentName = null;
        if (entity.ParentId.HasValue)
        {
            parentName = await _dbContext.Db.Queryable<ProjectConnection>()
                .Where(x => x.Id == entity.ParentId.Value && !x.IsDeleted)
                .Select(x => x.Name)
                .FirstAsync();
        }
        var applications = await GetApplicationsByIdsAsync(request.ApplicationIds);
        return (MapConnection(entity, parentName, applications.Select(x => x.Id), applications.Select(x => x.Name)), null);
    }

    public async Task<(bool Success, string? Error)> DeleteConnectionAsync(long projectId, long id)
    {
        var hasChildren = await _dbContext.Db.Queryable<ProjectConnection>()
            .AnyAsync(x => x.ProjectId == projectId && x.ParentId == id && !x.IsDeleted);
        if (hasChildren) return (false, "请先删除或调整使用该连接作为前置步骤的节点");

        var affected = await _dbContext.Db.Updateable<ProjectConnection>()
            .SetColumns(x => x.IsDeleted == true)
            .SetColumns(x => x.UpdateTime == DateTime.Now)
            .Where(x => x.Id == id && x.ProjectId == projectId && !x.IsDeleted)
            .ExecuteCommandAsync();
        if (affected > 0)
        {
            await _dbContext.Db.Deleteable<ProjectConnectionApplication>()
                .Where(x => x.ConnectionId == id).ExecuteCommandAsync();
        }
        return affected == 0 ? (false, "连接信息不存在") : (true, null);
    }

    public async Task<(string? Password, string? Error)> RevealPasswordAsync(long projectId, long id)
    {
        var encrypted = await _dbContext.Db.Queryable<ProjectConnection>()
            .Where(x => x.Id == id && x.ProjectId == projectId && !x.IsDeleted)
            .Select(x => x.PasswordEncrypted)
            .FirstAsync();
        if (string.IsNullOrEmpty(encrypted)) return (null, "未保存密码");
        return (_secretCipher.Decrypt(encrypted), null);
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var projects = _dbContext.Db.Queryable<ProjectInfo>().Where(x => !x.IsDeleted);
        var regions = await projects.Where(x => x.Region != null && x.Region != string.Empty)
            .Select(x => x.Region).Distinct().CountAsync();
        return new DashboardStatsDto
        {
            ProjectCount = await projects.CountAsync(),
            RegionCount = regions,
            ConnectionCount = await _dbContext.Db.Queryable<ProjectConnection>().CountAsync(x => !x.IsDeleted)
        };
    }

    private async Task<bool> ProjectExistsAsync(long projectId) =>
        await _dbContext.Db.Queryable<ProjectInfo>().AnyAsync(x => x.Id == projectId && !x.IsDeleted);

    private async Task AttachApplicationsAsync(List<ProjectDto> projects)
    {
        var projectIds = projects.Select(x => x.Id).Distinct().ToList();
        if (projectIds.Count == 0) return;
        var applications = await _dbContext.Db.Queryable<ProjectApplication>()
            .Where(x => projectIds.Contains(x.ProjectId) && !x.IsDeleted)
            .OrderBy(x => x.Sort).OrderBy(x => x.Id).ToListAsync();
        var byProject = applications.GroupBy(x => x.ProjectId)
            .ToDictionary(x => x.Key, x => x.Select(MapApplication).ToList());
        foreach (var project in projects)
            project.Applications = byProject.GetValueOrDefault(project.Id) ?? new List<ProjectApplicationDto>();
    }

    private async Task<string?> ValidateApplicationsAsync(long projectId, IEnumerable<long> applicationIds)
    {
        var ids = applicationIds.Distinct().ToList();
        if (ids.Count == 0) return null;
        var count = await _dbContext.Db.Queryable<ProjectApplication>()
            .CountAsync(x => ids.Contains(x.Id) && x.ProjectId == projectId && !x.IsDeleted);
        return count == ids.Count ? null : "所选业务系统不存在或不属于当前部署档案";
    }

    private async Task<List<ProjectApplication>> GetApplicationsByIdsAsync(IEnumerable<long> applicationIds)
    {
        var ids = applicationIds.Distinct().ToList();
        if (ids.Count == 0) return new List<ProjectApplication>();
        return await _dbContext.Db.Queryable<ProjectApplication>()
            .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
            .OrderBy(x => x.Sort).OrderBy(x => x.Id).ToListAsync();
    }

    private async Task ReplaceConnectionApplicationsAsync(long connectionId, IEnumerable<long> applicationIds)
    {
        await _dbContext.Db.Deleteable<ProjectConnectionApplication>()
            .Where(x => x.ConnectionId == connectionId).ExecuteCommandAsync();
        var links = applicationIds.Distinct()
            .Select(applicationId => new ProjectConnectionApplication
            {
                ConnectionId = connectionId,
                ApplicationId = applicationId
            }).ToList();
        if (links.Count > 0)
            await _dbContext.Db.Insertable(links).ExecuteCommandAsync();
    }

    private async Task<string?> ValidateParentAsync(long projectId, long? parentId, long? currentId)
    {
        if (!parentId.HasValue) return null;
        if (parentId == currentId) return "不能将连接本身设置为前置连接";

        var all = await _dbContext.Db.Queryable<ProjectConnection>()
            .Where(x => x.ProjectId == projectId && !x.IsDeleted).ToListAsync();
        var byId = all.ToDictionary(x => x.Id);
        if (!byId.ContainsKey(parentId.Value)) return "前置连接不存在或不属于当前项目";

        var visited = new HashSet<long>();
        var cursor = parentId;
        while (cursor.HasValue)
        {
            if (cursor == currentId) return "前置连接不能形成循环";
            if (!visited.Add(cursor.Value)) return "现有连接关系中存在循环";
            cursor = byId.TryGetValue(cursor.Value, out var node) ? node.ParentId : null;
        }
        return null;
    }

    private void ApplyConnection(ProjectConnection entity, ConnectionSaveRequest request)
    {
        entity.ParentId = request.ParentId;
        entity.Name = request.Name.Trim();
        entity.ConnectionType = request.ConnectionType.Trim();
        entity.Address = Clean(request.Address);
        entity.Port = Clean(request.Port);
        entity.UserName = Clean(request.UserName);
        entity.Remark = Clean(request.Remark);
        entity.Sort = request.Sort;
        if (request.ClearPassword) entity.PasswordEncrypted = null;
        else if (request.Password is not null) entity.PasswordEncrypted = _secretCipher.Encrypt(request.Password);
    }

    private static void ApplyContact(ProjectContact entity, ContactSaveRequest request)
    {
        entity.Role = request.Role.Trim();
        entity.Name = request.Name.Trim();
        entity.ContactInfo = Clean(request.ContactInfo);
        entity.Remark = Clean(request.Remark);
        entity.Sort = request.Sort;
    }

    private void ApplyApplication(ProjectApplication entity, ApplicationSaveRequest request)
    {
        entity.Name = request.Name.Trim();
        entity.LoginAddress = Clean(request.LoginAddress);
        entity.UserName = Clean(request.UserName);
        entity.Remark = Clean(request.Remark);
        entity.Sort = request.Sort;
        if (request.ClearPassword) entity.PasswordEncrypted = null;
        else if (request.Password is not null) entity.PasswordEncrypted = _secretCipher.Encrypt(request.Password);
    }

    private static void ApplyProject(ProjectInfo entity, ProjectSaveRequest request)
    {
        var region = request.Region.Trim();
        entity.ProjectName = region;
        entity.Region = region;
        entity.Description = Clean(request.Description);
    }

    private static ProjectDto MapProject(ProjectInfo x) => new()
    {
        Id = x.Id,
        Region = x.Region ?? x.ProjectName,
        Description = x.Description,
        CreateTime = x.CreateTime,
        UpdateTime = x.UpdateTime
    };

    private static ProjectContactDto MapContact(ProjectContact x) => new()
    {
        Id = x.Id,
        ProjectId = x.ProjectId,
        Role = x.Role,
        Name = x.Name,
        ContactInfo = x.ContactInfo,
        Remark = x.Remark,
        Sort = x.Sort,
        CreateTime = x.CreateTime,
        UpdateTime = x.UpdateTime
    };

    private static ProjectApplicationDto MapApplication(ProjectApplication x) => new()
    {
        Id = x.Id,
        ProjectId = x.ProjectId,
        Name = x.Name,
        LoginAddress = x.LoginAddress,
        UserName = x.UserName,
        HasPassword = !string.IsNullOrEmpty(x.PasswordEncrypted),
        Remark = x.Remark,
        Sort = x.Sort,
        CreateTime = x.CreateTime,
        UpdateTime = x.UpdateTime
    };

    private static ProjectConnectionDto MapConnection(
        ProjectConnection x,
        string? parentName,
        IEnumerable<long>? applicationIds = null,
        IEnumerable<string>? applicationNames = null) => new()
    {
        Id = x.Id,
        ProjectId = x.ProjectId,
        ApplicationIds = applicationIds?.ToList() ?? new List<long>(),
        ApplicationNames = applicationNames?.ToList() ?? new List<string>(),
        ParentId = x.ParentId,
        ParentName = parentName,
        Name = x.Name,
        ConnectionType = x.ConnectionType,
        Address = x.Address,
        Port = x.Port,
        UserName = x.UserName,
        HasPassword = !string.IsNullOrEmpty(x.PasswordEncrypted),
        Remark = x.Remark,
        Sort = x.Sort,
        CreateTime = x.CreateTime,
        UpdateTime = x.UpdateTime
    };

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
