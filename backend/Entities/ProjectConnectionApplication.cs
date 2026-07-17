using SqlSugar;

namespace ProjectBrain.Api.Entities;

/// <summary>连接节点与业务系统的多对多关联；没有关联记录表示公共连接。</summary>
[SugarTable("ProjectConnectionApplication")]
public class ProjectConnectionApplication
{
    [SugarColumn(IsPrimaryKey = true)]
    public long ConnectionId { get; set; }

    [SugarColumn(IsPrimaryKey = true)]
    public long ApplicationId { get; set; }
}
