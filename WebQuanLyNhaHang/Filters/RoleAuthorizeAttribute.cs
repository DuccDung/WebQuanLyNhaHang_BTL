using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebQuanLyNhaHang.Extensions;

namespace WebQuanLyNhaHang.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RoleAuthorizeAttribute : ActionFilterAttribute
{
    private readonly string[] _allowedRoles;
    private readonly string _module;
    private readonly string _action;

    public RoleAuthorizeAttribute(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
        _module = string.Empty;
        _action = string.Empty;
    }

    public RoleAuthorizeAttribute(string module, string action)
    {
        _module = module;
        _action = action;
        _allowedRoles = Array.Empty<string>();
    }

    public RoleAuthorizeAttribute(string module, string action, params string[] allowedRoles)
    {
        _module = module;
        _action = action;
        _allowedRoles = allowedRoles;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var httpContext = context.HttpContext;
        var userRole = httpContext.GetUserRole();

        if (_allowedRoles.Length > 0)
        {
            if (!_allowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = BuildUnauthorizedResult(context);
                return;
            }
        }
        else if (!string.IsNullOrEmpty(_module))
        {
            var isAllowed = string.IsNullOrWhiteSpace(_action)
                ? httpContext.CanAccessModule(_module)
                : httpContext.CanAccessAction(_module, _action);

            if (!isAllowed)
            {
                context.Result = BuildUnauthorizedResult(context);
                return;
            }
        }

        base.OnActionExecuting(context);
    }

    private static IActionResult BuildUnauthorizedResult(ActionExecutingContext context)
    {
        var acceptHeader = context.HttpContext.Request.Headers.Accept.ToString();
        var requestedWith = context.HttpContext.Request.Headers["X-Requested-With"].ToString();
        var expectsJson = acceptHeader.Contains("application/json", StringComparison.OrdinalIgnoreCase)
            || string.Equals(requestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        if (expectsJson)
        {
            return new ObjectResult(new
            {
                success = false,
                message = "Ban khong co quyen thuc hien thao tac nay."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        return new ViewResult
        {
            ViewName = "~/Views/Shared/AccessDenied.cshtml",
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}
