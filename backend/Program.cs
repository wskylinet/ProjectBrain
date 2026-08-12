using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectBrain.Api.Auth;
using ProjectBrain.Api.Data;
using ProjectBrain.Api.Options;
using ProjectBrain.Api.Security;
using ProjectBrain.Api.Services;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicy = "AllowFrontend";

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<DbContext>();
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<PermissionAuthorizationFilter>();
builder.Services.AddScoped<AuditLogFilter>();
builder.Services.AddSingleton<ISecretCipher, AesSecretCipher>();

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
        ValidateIssuerSigningKey = true, ValidIssuer = jwtOptions.Issuer, ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
{
    var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"];
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));
builder.Services.AddControllers(options =>
{
    options.Filters.Add<PermissionAuthorizationFilter>();
    options.Filters.Add<AuditLogFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Project Brain API", Version = "v1" });
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization", Description = "输入：Bearer {token}", In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey, Scheme = "Bearer",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

var app = builder.Build();
var initializeAdmin = args.Contains("--init-admin", StringComparer.OrdinalIgnoreCase);
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
        dbContext.InitDatabase();
        if (initializeAdmin)
        {
            var userName = Environment.GetEnvironmentVariable("PROJECTBRAIN_INITIAL_ADMIN_USERNAME");
            var password = Environment.GetEnvironmentVariable("PROJECTBRAIN_INITIAL_ADMIN_PASSWORD");
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
                throw new InvalidOperationException(
                    "请设置 PROJECTBRAIN_INITIAL_ADMIN_USERNAME 和 PROJECTBRAIN_INITIAL_ADMIN_PASSWORD 环境变量。");

            dbContext.CreateInitialAdmin(userName, password);
            app.Logger.LogInformation("初始管理员 {UserName} 创建成功。", userName.Trim());
        }
    }
    catch (Exception ex)
    {
        if (initializeAdmin)
        {
            app.Logger.LogError(ex, "初始管理员创建失败。");
            Environment.ExitCode = 1;
            return;
        }
        app.Logger.LogWarning(ex, "数据库初始化失败，请检查数据库连接。");
    }
}
if (initializeAdmin) return;
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();
