using System.ComponentModel.DataAnnotations;

namespace ProjectBrain.Api.Dtos;

public class UserQuery
{
    public string? Keyword { get; set; }
    public bool? IsEnabled { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class UserDto
{
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? NickName { get; set; }
    public bool IsEnabled { get; set; }
    public List<long> RoleIds { get; set; } = new();
    public List<string> RoleNames { get; set; } = new();
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}

public class UserCreateRequest
{
    [Required, StringLength(50, MinimumLength = 2)] public string UserName { get; set; } = string.Empty;
    [StringLength(50)] public string? NickName { get; set; }
    [Required, StringLength(100, MinimumLength = 6)] public string Password { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public List<long> RoleIds { get; set; } = new();
}

public class UserUpdateRequest
{
    [StringLength(50)] public string? NickName { get; set; }
    public bool IsEnabled { get; set; } = true;
    public List<long> RoleIds { get; set; } = new();
}

public class ResetPasswordRequest
{
    [Required, StringLength(100, MinimumLength = 6)] public string Password { get; set; } = string.Empty;
}
