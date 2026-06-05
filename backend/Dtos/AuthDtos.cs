using System.ComponentModel.DataAnnotations;

namespace ProjectBrain.Api.Dtos;

public class LoginRequest
{
    [Required(ErrorMessage = "用户名不能为空")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public UserInfoDto User { get; set; } = new();
}

public class UserInfoDto
{
    public long Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? NickName { get; set; }
}
