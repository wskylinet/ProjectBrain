using SqlSugar;

namespace ProjectBrain.Api.Entities;

/// <summary>
/// 系统用户表。用于登录认证，后续可扩展角色权限。
/// </summary>
[SugarTable("SysUser")]
public class SysUser
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>登录账号。</summary>
    [SugarColumn(Length = 50)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>BCrypt 哈希后的密码，明文密码不入库。</summary>
    [SugarColumn(Length = 200)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>显示名称。</summary>
    [SugarColumn(Length = 50, IsNullable = true)]
    public string? NickName { get; set; }

    /// <summary>是否启用。</summary>
    public bool IsEnabled { get; set; } = true;

    public DateTime CreateTime { get; set; } = DateTime.Now;

    [SugarColumn(IsNullable = true)]
    public DateTime? UpdateTime { get; set; }

    public bool IsDeleted { get; set; }
}
