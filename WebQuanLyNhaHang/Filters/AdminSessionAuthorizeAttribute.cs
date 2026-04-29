using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AdminSessionAuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var employeeId = context.HttpContext.Session.GetInt32("NhanVienId");

        if (!employeeId.HasValue)
        {
            context.Result = BuildUnauthorizedResult(context);
            return;
        }

        var dbContext = context.HttpContext.RequestServices.GetService<QlnhaHangBtlContext>();
        var isActiveEmployee = dbContext?.NhanViens.Any(employee => employee.NvId == employeeId.Value && !employee.Remove) == true;

        if (isActiveEmployee)
        {
            base.OnActionExecuting(context);
            return;
        }

        context.HttpContext.Session.Remove("NhanVienId");
        context.HttpContext.Session.Remove("NhanVienName");
        context.HttpContext.Session.Remove("NhanVienTaiKhoan");
        context.Result = BuildUnauthorizedResult(context);
    }

    private static IActionResult BuildUnauthorizedResult(ActionExecutingContext context)
    {
        var acceptHeader = context.HttpContext.Request.Headers.Accept.ToString();
        var requestedWith = context.HttpContext.Request.Headers["X-Requested-With"].ToString();
        var expectsJson = acceptHeader.Contains("application/json", StringComparison.OrdinalIgnoreCase)
            || string.Equals(requestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        if (expectsJson)
        {
            return new UnauthorizedObjectResult(new
            {
                message = "Phiên đăng nhập đã hết hạn hoặc tài khoản nhân viên đã bị xóa. Vui lòng đăng nhập lại.",
                redirectUrl = "/Admin/Login"
            });
        }

        return new RedirectToActionResult("Login", "Admin", null);
    }
}
