using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebQuanLyNhaHang.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AdminSessionAuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Session.GetInt32("NhanVienId").HasValue)
        {
            base.OnActionExecuting(context);
            return;
        }

        context.Result = new RedirectToActionResult("Login", "Admin", null);
    }
}
