using System.ComponentModel.DataAnnotations;

namespace ProjectBrain.Api.Dtos;

public class ConnectionSaveRequest
{
    public List<long> ApplicationIds { get; set; } = new();
    public long? ParentId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string ConnectionType { get; set; } = "其他";

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(20)]
    public string? Port { get; set; }

    [StringLength(100)]
    public string? UserName { get; set; }

    public string? Password { get; set; }
    public bool ClearPassword { get; set; }

    public string? Remark { get; set; }
    public int Sort { get; set; }
}

public class ProjectConnectionDto
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public List<long> ApplicationIds { get; set; } = new();
    public List<string> ApplicationNames { get; set; } = new();
    public long? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ConnectionType { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Port { get; set; }
    public string? UserName { get; set; }
    public bool HasPassword { get; set; }
    public string? Remark { get; set; }
    public int Sort { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}

public class RevealPasswordDto
{
    public string Password { get; set; } = string.Empty;
}
