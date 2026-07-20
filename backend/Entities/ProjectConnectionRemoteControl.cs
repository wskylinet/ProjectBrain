using SqlSugar;

namespace ProjectBrain.Api.Entities;

/// <summary>A concrete remote-control tool configured on a connection node.</summary>
[SugarTable("ProjectConnectionRemoteControl")]
public class ProjectConnectionRemoteControl
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long ConnectionId { get; set; }

    [SugarColumn(Length = 30)]
    public string SoftwareName { get; set; } = string.Empty;

    [SugarColumn(Length = 200)]
    public string DeviceCode { get; set; } = string.Empty;

    [SugarColumn(ColumnDataType = "nvarchar(max)", IsNullable = true)]
    public string? PasswordEncrypted { get; set; }

    public int Sort { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;

    [SugarColumn(IsNullable = true)]
    public DateTime? UpdateTime { get; set; }

    public bool IsDeleted { get; set; }
}
