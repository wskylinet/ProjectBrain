using SqlSugar;

namespace ProjectBrain.Api.Entities;

/// <summary>部署实例下的前端应用，同一套后端和连接信息可对应多个前端。</summary>
[SugarTable("ProjectApplication")]
public class ProjectApplication
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long ProjectId { get; set; }

    [SugarColumn(Length = 100)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? LoginAddress { get; set; }

    [SugarColumn(Length = 100, IsNullable = true)]
    public string? UserName { get; set; }

    [SugarColumn(ColumnDataType = "nvarchar(max)", IsNullable = true)]
    public string? PasswordEncrypted { get; set; }

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Remark { get; set; }

    public int Sort { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;

    [SugarColumn(IsNullable = true)]
    public DateTime? UpdateTime { get; set; }

    public bool IsDeleted { get; set; }
}
