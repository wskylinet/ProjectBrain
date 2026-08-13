using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectBrain.Api.Auth;
using ProjectBrain.Api.Common;
using ProjectBrain.Api.Data;
using ProjectBrain.Api.Options;
using ProjectBrain.Api.Security;
using ProjectBrain.Api.Services;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicy = "AllowFrontend";
const string LoginRateLimitPolicy = "Login";

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
builder.Services.AddSingleton<ILoginAttemptTracker, LoginAttemptTracker>();
builder.Services.AddSingleton<RateLimitAuditRecorder>();

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
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        await context.HttpContext.RequestServices.GetRequiredService<RateLimitAuditRecorder>()
            .RecordAsync(context.HttpContext, cancellationToken);
        context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(
            ApiResult.Fail("登录尝试过于频繁，请稍后再试", StatusCodes.Status429TooManyRequests),
            cancellationToken);
    };
    options.AddPolicy(LoginRateLimitPolicy, httpContext =>
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress;
        if (remoteIp?.IsIPv4MappedToIPv6 == true) remoteIp = remoteIp.MapToIPv4();
        var partitionKey = remoteIp?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});
if (builder.Environment.IsDevelopment()) builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
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
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers.ContentSecurityPolicy =
        "default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; " +
        "form-action 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; font-src 'self' data:; connect-src 'self'";
    context.Response.Headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=(), payment=(), usb=()";

    context.Response.OnStarting(() =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
            context.Response.Headers.CacheControl = "no-store";
        else if (context.Response.ContentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) == true)
            context.Response.Headers.CacheControl = "no-cache";
        return Task.CompletedTask;
    });

    await next();
});
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/tools"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await next();
});
app.UseDefaultFiles();
app.UseStaticFiles();
if (app.Environment.IsDevelopment()) app.UseCors(CorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();
