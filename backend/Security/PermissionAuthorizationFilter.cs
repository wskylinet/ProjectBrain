using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjectBrain.Api.Common;
using ProjectBrain.Api.Dtos;
using ProjectBrain.Api.Services;

namespace ProjectBrain.Api.Security;

public class PermissionAuthorizationFilter : IAsyncActionFilter
{
    private readonly IPermissionService _permissionService;
    public PermissionAuthorizationFilter(IPermissionService permissionService) => _permissionService = permissionService;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
        {
            await next();
            return;
        }

        var controller = context.RouteData.Values["controller"]?.ToString();
        var action = context.RouteData.Values["action"]?.ToString() ?? string.Empty;
        var permission = RequiredPermission(controller, action, context.HttpContext.Request.Method);
        if (permission is null)
        {
            await next();
            return;
        }

        var idValue = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(idValue, out var userId))
        {
            context.Result = new ObjectResult(ApiResult.Fail("登录状态无效", 401)) { StatusCode = 401 };
            return;
        }

        if (!await _permissionService.HasPermissionAsync(userId, permission))
        {
            context.Result = new ObjectResult(ApiResult.Fail("没有执行此操作的权限", 403)) { StatusCode = 403 };
            return;
        }

        if (RequiresSecretUpdate(context.ActionArguments) &&
            !await _permissionService.HasPermissionAsync(userId, PermissionCodes.SecretUpdate))
        {
            context.Result = new ObjectResult(ApiResult.Fail("没有修改密码信息的权限", 403)) { StatusCode = 403 };
            return;
        }

        await next();
    }

    private static string? RequiredPermission(string? controller, string action, string method) => controller switch
    {
        "Dashboard" => PermissionCodes.ArchiveView,
        "Users" => method == "GET" ? PermissionCodes.UserView : PermissionCodes.UserManage,
        "Roles" => method == "GET" ? PermissionCodes.UserView : PermissionCodes.RoleManage,
        "Projects" when action.Contains("Reveal", StringComparison.OrdinalIgnoreCase) => PermissionCodes.SecretReveal,
        "Projects" when method == "GET" => PermissionCodes.ArchiveView,
        "Projects" when method == "DELETE" => PermissionCodes.ArchiveDelete,
        "Projects" when method == "POST" && action == "Create" => PermissionCodes.ArchiveCreate,
        "Projects" => PermissionCodes.ArchiveUpdate,
        _ => null
    };

    private static bool RequiresSecretUpdate(IDictionary<string, object?> arguments)
    {
        foreach (var value in arguments.Values)
        {
            if (value is ApplicationSaveRequest app && (!string.IsNullOrEmpty(app.Password) || app.ClearPassword)) return true;
            if (value is ConnectionSaveRequest connection &&
                (!string.IsNullOrEmpty(connection.Password) || connection.ClearPassword ||
                 connection.RemoteControls.Any(x => !string.IsNullOrEmpty(x.Password) || x.ClearPassword))) return true;
        }
        return false;
    }
}
