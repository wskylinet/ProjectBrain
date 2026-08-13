using System.ComponentModel.DataAnnotations;
using ProjectBrain.Api.Validation;

namespace ProjectBrain.Api.Dtos;

public class ProjectQuery
{
    public string? Keyword { get; set; }
    public string? Region { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ProjectSaveRequest
{
    [Required, StringLength(50)]
    public string Region { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class ProjectDto : ProjectSaveRequest
{
    public long Id { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public List<ProjectApplicationDto> Applications { get; set; } = new();
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class DashboardStatsDto
{
    public int ProjectCount { get; set; }
    public int RegionCount { get; set; }
    public int ConnectionCount { get; set; }
}

public class ContactSaveRequest
{
    [Required, StringLength(50)]
    public string Role { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ContactInfo { get; set; }

    [StringLength(500)]
    public string? Remark { get; set; }

    public int Sort { get; set; }
}

public class ProjectContactDto : ContactSaveRequest
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}

public class ApplicationSaveRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500), HttpUrl]
    public string? LoginAddress { get; set; }

    [StringLength(100)]
    public string? UserName { get; set; }

    public string? Password { get; set; }
    public bool ClearPassword { get; set; }

    [StringLength(500)]
    public string? Remark { get; set; }

    public int Sort { get; set; }
}

public class ProjectApplicationDto
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LoginAddress { get; set; }
    public string? UserName { get; set; }
    public bool HasPassword { get; set; }
    public string? Remark { get; set; }
    public int Sort { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}
