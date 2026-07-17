using SqlSugar;

namespace ProjectBrain.Api.Entities;

/// <summary>项目人员，一个项目可维护多名不同职责的联系人。</summary>
[SugarTable("ProjectContact")]
public class ProjectContact
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long ProjectId { get; set; }

    [SugarColumn(Length = 50)]
    public string Role { get; set; } = string.Empty;

    [SugarColumn(Length = 50)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(Length = 100, IsNullable = true)]
    public string? ContactInfo { get; set; }

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Remark { get; set; }

    public int Sort { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;

    [SugarColumn(IsNullable = true)]
    public DateTime? UpdateTime { get; set; }

    public bool IsDeleted { get; set; }
}
