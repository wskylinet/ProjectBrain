namespace ProjectBrain.Api.Dtos;

public class RoleOptionDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UserAccessDto
{
    public List<RoleOptionDto> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}
