using SqlSugar;

namespace ProjectBrain.Api.Entities;

/// <summary>
/// 项目主表，对应数据库设计文档 ProjectInfo。
/// 此处先建立主表骨架，子表（服务器/数据库/部署等）在后续迭代补充。
/// </summary>
[SugarTable("ProjectInfo")]
public class ProjectInfo
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 100)]
    public string ProjectName { get; set; } = string.Empty;

    [SugarColumn(Length = 50, IsNullable = true)]
    public string? Region { get; set; }

    [SugarColumn(Length = 100, IsNullable = true)]
    public string? SystemName { get; set; }

    [SugarColumn(Length = 50, IsNullable = true)]
    public string? ProjectStatus { get; set; }

    [SugarColumn(Length = 50, IsNullable = true)]
    public string? OwnerName { get; set; }

    [SugarColumn(Length = 100, IsNullable = true)]
    public string? ContactInfo { get; set; }

    [SugarColumn(ColumnDataType = "nvarchar(max)", IsNullable = true)]
    public string? Description { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    [SugarColumn(IsNullable = true)]
    public DateTime? UpdateTime { get; set; }

    public bool IsDeleted { get; set; }
}
