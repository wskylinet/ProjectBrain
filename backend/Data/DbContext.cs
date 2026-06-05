using SqlSugar;
using ProjectBrain.Api.Entities;

namespace ProjectBrain.Api.Data;

/// <summary>
/// SqlSugar 数据库上下文封装，注册为单例（SqlSugarScope 线程安全）。
/// </summary>
public class DbContext
{
    public ISqlSugarClient Db { get; }

    public DbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("缺少数据库连接字符串 ConnectionStrings:Default");

        Db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = DbType.SqlServer,
            IsAutoCloseConnection = true,
            ConfigureExternalServices = new ConfigureExternalServices()
        });
    }

    /// <summary>
    /// CodeFirst 初始化表结构，并写入种子管理员账号。
    /// 默认账号：admin / admin123（首次登录后请尽快修改）。
    /// </summary>
    public void InitDatabase()
    {
        Db.DbMaintenance.CreateDatabase();
        Db.CodeFirst.InitTables<SysUser>();
        Db.CodeFirst.InitTables<ProjectInfo>();

        if (!Db.Queryable<SysUser>().Any(x => x.UserName == "admin"))
        {
            Db.Insertable(new SysUser
            {
                UserName = "admin",
                NickName = "超级管理员",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                IsEnabled = true,
                CreateTime = DateTime.Now
            }).ExecuteCommand();
        }
    }
}
