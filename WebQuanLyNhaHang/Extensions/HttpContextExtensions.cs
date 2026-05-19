using Microsoft.AspNetCore.Http;
using WebQuanLyNhaHang.Authorization;

namespace WebQuanLyNhaHang.Extensions;

public static class HttpContextExtensions
{
    public static bool HasRole(this HttpContext context, string roleKey)
    {
        var userRole = context.GetUserRole();
        return string.Equals(userRole, AdminPermissions.NormalizeRoleKey(roleKey), StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasAnyRole(this HttpContext context, params string[] roleKeys)
    {
        var userRole = context.GetUserRole();
        return roleKeys.Any(role => string.Equals(userRole, AdminPermissions.NormalizeRoleKey(role), StringComparison.OrdinalIgnoreCase));
    }

    public static bool CanAccessModule(this HttpContext context, string module)
    {
        return AdminPermissions.CanViewModule(context.GetUserRole(), module);
    }

    public static bool CanAccessAction(this HttpContext context, string module, string action)
    {
        return AdminPermissions.CanAccessAction(context.GetUserRole(), module, action);
    }

    public static bool CanCreate(this HttpContext context, string module)
    {
        return AdminPermissions.CanCreate(context.GetUserRole(), module);
    }

    public static bool CanEdit(this HttpContext context, string module)
    {
        return AdminPermissions.CanEdit(context.GetUserRole(), module);
    }

    public static bool CanDelete(this HttpContext context, string module)
    {
        return AdminPermissions.CanDelete(context.GetUserRole(), module);
    }

    public static int? GetUserRolePermissionId(this HttpContext context)
    {
        return context.Session.GetInt32(SessionKeys.RolePermissionId);
    }

    public static string GetUserRole(this HttpContext context)
    {
        return AdminPermissions.ResolveRoleKey(
            context.Session.GetInt32(SessionKeys.RolePermissionId),
            context.Session.GetString(SessionKeys.RoleKey));
    }

    public static string GetUserRoleLabel(this HttpContext context)
    {
        return AdminPermissions.GetRoleLabel(context.GetUserRole());
    }

    public static string GetUserName(this HttpContext context)
    {
        return context.Session.GetString(SessionKeys.EmployeeName) ?? "User";
    }

    public static int? GetUserId(this HttpContext context)
    {
        return context.Session.GetInt32(SessionKeys.EmployeeId);
    }

    public static bool CanAssignEmployeeRoles(this HttpContext context)
    {
        return AdminPermissions.CanAssignEmployeeRoles(context.GetUserRole());
    }

    public static bool CanProcessDineInPayments(this HttpContext context)
    {
        return AdminPermissions.CanProcessDineInPayments(context.GetUserRole());
    }

    public static bool CanUpdateDeliveryStatus(this HttpContext context)
    {
        return AdminPermissions.CanUpdateDeliveryStatus(context.GetUserRole());
    }

    public static bool CanUpdateDeliveryStatus(this HttpContext context, string status)
    {
        return AdminPermissions.CanUpdateDeliveryStatus(context.GetUserRole(), status);
    }
}
