using SqlSugar;

namespace ProjectBrain.Api.Entities;

/// <summary>A connection node; ParentId points to the prerequisite connection.</summary>
[SugarTable("ProjectConnection")]
public class ProjectConnection
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long ProjectId { get; set; }

    [SugarColumn(IsNullable = true)]
    public long? ParentId { get; set; }

    [SugarColumn(Length = 100)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(Length = 30)]
    public string ConnectionType { get; set; } = "其他";

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Address { get; set; }

    [SugarColumn(Length = 20, IsNullable = true)]
    public string? Port { get; set; }

    [SugarColumn(Length = 100, IsNullable = true)]
    public string? UserName { get; set; }

    [SugarColumn(ColumnDataType = "nvarchar(max)", IsNullable = true)]
    public string? PasswordEncrypted { get; set; }

    [SugarColumn(ColumnDataType = "nvarchar(max)", IsNullable = true)]
    public string? Remark { get; set; }

    public int Sort { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    [SugarColumn(IsNullable = true)]
    public DateTime? UpdateTime { get; set; }

    public bool IsDeleted { get; set; }
}
