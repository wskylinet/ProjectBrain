using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using ProjectBrain.Api.Dtos;
using ProjectBrain.Api.Entities;
using ProjectBrain.Api.Services;

namespace ProjectBrain.Api.Security;

public class AuditLogFilter : IAsyncActionFilter, IOrderedFilter
{
    private static readonly string[] SensitiveNames = ["password", "token", "secret", "key"];
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogFilter> _logger;
    public int Order => int.MaxValue;

    public AuditLogFilter(IAuditLogService auditLogService, ILogger<AuditLogFilter> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!ShouldAudit(context)) { await next(); return; }

        var stopwatch = Stopwatch.StartNew();
        var executed = await next();
        stopwatch.Stop();
        var http = context.HttpContext;
        var statusCode = ResolveStatusCode(executed, http.Response.StatusCode);
        var login = context.ActionArguments.Values.OfType<LoginRequest>().FirstOrDefault();
        var idValue = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
        long? userId = long.TryParse(idValue, out var parsedId) ? parsedId : null;
        var controller = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
        var actionName = context.RouteData.Values["action"]?.ToString() ?? string.Empty;
        var action = ResolveAction(controller, actionName, http.Request.Method);

        try
        {
            await _auditLogService.WriteAsync(new SysAuditLog
            {
                UserId = userId, UserName = http.User.Identity?.Name ?? login?.UserName,
                Action = action, Module = controller, Description = $"{controller}.{actionName} ({action})",
                HttpMethod = http.Request.Method, RequestPath = http.Request.Path,
                TargetId = ResolveTargetId(context.RouteData.Values), DetailJson = SerializeArguments(context.ActionArguments),
                IpAddress = http.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Truncate(http.Request.Headers.UserAgent.ToString(), 500),
                IsSuccess = executed.Exception is null && statusCode < 400, StatusCode = statusCode,
                DurationMs = stopwatch.ElapsedMilliseconds, CreateTime = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            // 审计落库失败不能影响正常业务请求。
            _logger.LogError(ex, "Failed to persist audit log for {Method} {Path}", http.Request.Method, http.Request.Path);
        }
    }

    private static bool ShouldAudit(ActionExecutingContext context)
    {
        var method = context.HttpContext.Request.Method;
        var action = context.RouteData.Values["action"]?.ToString() ?? string.Empty;
        return method is "POST" or "PUT" or "PATCH" or "DELETE" || action.Contains("Reveal", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveAction(string controller, string action, string method)
    {
        if (controller == "Auth" && action == "Login") return "Login";
        if (action.Contains("Reveal", StringComparison.OrdinalIgnoreCase)) return "RevealSecret";
        if (action.Contains("Password", StringComparison.OrdinalIgnoreCase)) return "ResetPassword";
        return method switch { "POST" => "Create", "PUT" or "PATCH" => "Update", "DELETE" => "Delete", _ => "Access" };
    }

    private static string? ResolveTargetId(RouteValueDictionary values)
    {
        var parts = new List<string>();
        foreach (var name in new[] { "projectId", "connectionId", "remoteControlId", "id" })
            if (values.TryGetValue(name, out var value) && value is not null) parts.Add($"{name}={value}");
        return parts.Count == 0 ? null : string.Join(",", parts);
    }

    private static int ResolveStatusCode(ActionExecutedContext context, int responseStatusCode) =>
        context.Result is ObjectResult { StatusCode: int resultCode } ? resultCode : responseStatusCode;

    private static string? SerializeArguments(IDictionary<string, object?> arguments)
    {
        if (arguments.Count == 0) return null;
        var node = JsonSerializer.SerializeToNode(arguments, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Redact(node);
        return node?.ToJsonString();
    }

    private static void Redact(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            foreach (var property in obj.ToList())
            {
                if (SensitiveNames.Any(x => property.Key.Contains(x, StringComparison.OrdinalIgnoreCase))) obj[property.Key] = "[REDACTED]";
                else Redact(property.Value);
            }
        }
        else if (node is JsonArray array) foreach (var item in array) Redact(item);
    }

    private static string? Truncate(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) || value.Length <= maxLength ? value : value[..maxLength];
}
