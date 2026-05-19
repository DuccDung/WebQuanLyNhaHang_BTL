using System.Globalization;
using System.Text;

namespace WebQuanLyNhaHang.Authorization;

public static class RoleKeys
{
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string Staff = "staff";
}

public static class SessionKeys
{
    public const string EmployeeId = "NhanVienId";
    public const string EmployeeName = "NhanVienName";
    public const string EmployeeAccount = "NhanVienTaiKhoan";
    public const string RoleKey = "RoleKey";
    public const string RolePermissionId = "RolePermissionId";
}

public static class PermissionModules
{
    public const string Dashboard = "dashboard";
    public const string Reports = "reports";
    public const string Products = "products";
    public const string Categories = "categories";
    public const string Orders = "orders";
    public const string OrderDetails = "order-details";
    public const string Customers = "customers";
    public const string Employees = "employees";
    public const string DineIn = "dine-in";
    public const string OnlineOrders = "online-orders";
    public const string Inventory = "inventory";
    public const string Stores = "stores";
    public const string Stories = "stories";
    public const string ProductConditions = "product-conditions";
}

public static class PermissionActions
{
    public const string View = "view";
    public const string List = "list";
    public const string Details = "details";
    public const string Search = "search";
    public const string Filter = "filter";
    public const string Create = "create";
    public const string Edit = "edit";
    public const string Update = "update";
    public const string Delete = "delete";
    public const string HideShow = "hide-show";
    public const string UploadImage = "upload-image";
    public const string ManageVariants = "manage-variants";
    public const string Sort = "sort";
    public const string Print = "print";
    public const string Export = "export";
    public const string ExportPdf = "export-pdf";
    public const string ExportExcel = "export-excel";
    public const string Refund = "refund";
    public const string Cancel = "cancel";
    public const string QuickCreate = "quick-create";
    public const string AssignRole = "assign-role";
    public const string ResetPassword = "reset-password";
    public const string ViewOwn = "view-own";
    public const string EditOwn = "edit-own";
    public const string ViewSchedule = "view-schedule";
    public const string ViewHistory = "view-history";
    public const string UpdateStatus = "update-status";
    public const string UpdateStatusLimited = "update-status-limited";
    public const string ManageTable = "manage-table";
    public const string ManageQr = "manage-qr";
    public const string MergeTransferTable = "merge-transfer-table";
    public const string ProcessPayment = "process-payment";
    public const string ImportStock = "import-stock";
    public const string AutoExportStock = "auto-export-stock";
    public const string LowStockAlert = "low-stock-alert";
    public const string LinkIngredient = "link-ingredient";
    public const string ComparePeriod = "compare-period";
}

public static class AdminPermissions
{
    public const int AdminPermissionId = 4;
    public const int ManagerPermissionId = 5;
    public const int StaffPermissionId = 6;

    private static readonly HashSet<int> AdminPermissionIds = new() { 1, AdminPermissionId };
    private static readonly HashSet<int> ManagerPermissionIds = new() { 3, ManagerPermissionId };
    private static readonly HashSet<int> StaffPermissionIds = new() { 2, StaffPermissionId };
    private static readonly Dictionary<string, Dictionary<string, HashSet<string>>> PermissionMatrix = BuildPermissionMatrix();

    public static string ResolveRoleKey(int? roleId, string? role)
    {
        if (roleId.HasValue)
        {
            if (AdminPermissionIds.Contains(roleId.Value))
            {
                return RoleKeys.Admin;
            }

            if (ManagerPermissionIds.Contains(roleId.Value))
            {
                return RoleKeys.Manager;
            }

            if (StaffPermissionIds.Contains(roleId.Value))
            {
                return RoleKeys.Staff;
            }
        }

        return NormalizeRoleKey(role);
    }

    public static (int? RoleId, string RoleKey) ResolveEffectiveRole(IEnumerable<(int? RoleId, string? RoleName)> roles)
    {
        (int? RoleId, string? RoleName)? firstAssignedRole = null;

        foreach (var role in roles)
        {
            if (!firstAssignedRole.HasValue && (role.RoleId.HasValue || !string.IsNullOrWhiteSpace(role.RoleName)))
            {
                firstAssignedRole = role;
            }

            var roleKey = ResolveRoleKey(role.RoleId, role.RoleName);
            if (roleKey == RoleKeys.Admin)
            {
                return (role.RoleId, roleKey);
            }

            if (roleKey == RoleKeys.Manager)
            {
                firstAssignedRole = role;
            }
        }

        if (firstAssignedRole.HasValue)
        {
            return (
                firstAssignedRole.Value.RoleId,
                ResolveRoleKey(firstAssignedRole.Value.RoleId, firstAssignedRole.Value.RoleName));
        }

        return (null, RoleKeys.Staff);
    }

    public static string NormalizeRoleKey(string? role)
    {
        var normalized = NormalizeText(role);

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return RoleKeys.Staff;
        }

        if (normalized == RoleKeys.Admin || normalized.Contains("admin", StringComparison.Ordinal))
        {
            return RoleKeys.Admin;
        }

        if (normalized == RoleKeys.Manager
            || normalized.Contains("manager", StringComparison.Ordinal)
            || normalized.Contains("quan ly", StringComparison.Ordinal)
            || normalized.Contains("quan tri", StringComparison.Ordinal))
        {
            return RoleKeys.Manager;
        }

        return RoleKeys.Staff;
    }

    public static string GetRoleLabel(string? role)
    {
        return NormalizeRoleKey(role) switch
        {
            RoleKeys.Admin => "Admin",
            RoleKeys.Manager => "Quan ly",
            _ => "Nhan vien"
        };
    }

    public static bool CanViewModule(string? role, string module)
    {
        return CanAccessAction(role, module, PermissionActions.View);
    }

    public static bool CanCreate(string? role, string module)
    {
        return CanAccessAction(role, module, PermissionActions.Create)
            || CanAccessAction(role, module, PermissionActions.QuickCreate);
    }

    public static bool CanEdit(string? role, string module)
    {
        return CanAccessAction(role, module, PermissionActions.Edit)
            || CanAccessAction(role, module, PermissionActions.Update);
    }

    public static bool CanDelete(string? role, string module)
    {
        return CanAccessAction(role, module, PermissionActions.Delete);
    }

    public static bool CanAccessAction(string? role, string module, string action)
    {
        var normalizedRole = NormalizeRoleKey(role);
        var normalizedModule = NormalizeText(module);
        var normalizedAction = NormalizeText(action);

        if (normalizedRole == RoleKeys.Admin)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(normalizedModule) || string.IsNullOrWhiteSpace(normalizedAction))
        {
            return false;
        }

        return PermissionMatrix.TryGetValue(normalizedRole, out var modulePermissions)
            && modulePermissions.TryGetValue(normalizedModule, out var allowedActions)
            && (allowedActions.Contains(normalizedAction)
                || (IsViewEquivalentAction(normalizedAction) && allowedActions.Contains(PermissionActions.View))
                || (normalizedAction == PermissionActions.Update && allowedActions.Contains(PermissionActions.Edit)));
    }

    public static bool CanAssignEmployeeRoles(string? role)
    {
        return CanAccessAction(role, PermissionModules.Employees, PermissionActions.AssignRole);
    }

    public static bool CanProcessDineInPayments(string? role)
    {
        return CanAccessAction(role, PermissionModules.DineIn, PermissionActions.ProcessPayment);
    }

    public static bool CanUpdateDeliveryStatus(string? role)
    {
        return CanAccessAction(role, PermissionModules.OnlineOrders, PermissionActions.UpdateStatus)
            || CanAccessAction(role, PermissionModules.OnlineOrders, PermissionActions.UpdateStatusLimited);
    }

    public static bool CanUpdateDeliveryStatus(string? role, string status)
    {
        if (CanAccessAction(role, PermissionModules.OnlineOrders, PermissionActions.UpdateStatus))
        {
            return true;
        }

        if (!CanAccessAction(role, PermissionModules.OnlineOrders, PermissionActions.UpdateStatusLimited))
        {
            return false;
        }

        var normalizedStatus = NormalizeText(status);
        return normalizedStatus is "pending" or "preparing" or "shipping" or "delivered";
    }

    private static Dictionary<string, Dictionary<string, HashSet<string>>> BuildPermissionMatrix()
    {
        var matrix = new Dictionary<string, Dictionary<string, HashSet<string>>>(StringComparer.OrdinalIgnoreCase);

        Allow(matrix, RoleKeys.Manager, PermissionModules.Dashboard, PermissionActions.View);
        Allow(matrix, RoleKeys.Manager, PermissionModules.Reports,
            PermissionActions.View, PermissionActions.Export, PermissionActions.ExportPdf, PermissionActions.ExportExcel);
        Allow(matrix, RoleKeys.Manager, PermissionModules.Products,
            PermissionActions.View, PermissionActions.Search, PermissionActions.Filter, PermissionActions.Create,
            PermissionActions.Edit, PermissionActions.Update, PermissionActions.HideShow, PermissionActions.UploadImage,
            PermissionActions.ManageVariants);
        Allow(matrix, RoleKeys.Manager, PermissionModules.Categories,
            PermissionActions.View, PermissionActions.Create, PermissionActions.Edit, PermissionActions.Update,
            PermissionActions.Sort, PermissionActions.HideShow);
        Allow(matrix, RoleKeys.Manager, PermissionModules.Orders,
            PermissionActions.View, PermissionActions.Details, PermissionActions.Search, PermissionActions.Filter,
            PermissionActions.Print, PermissionActions.ExportPdf, PermissionActions.Refund, PermissionActions.Cancel);
        Allow(matrix, RoleKeys.Manager, PermissionModules.OrderDetails,
            PermissionActions.View, PermissionActions.Details, PermissionActions.Search, PermissionActions.Filter);
        Allow(matrix, RoleKeys.Manager, PermissionModules.Customers,
            PermissionActions.View, PermissionActions.Details, PermissionActions.Create, PermissionActions.Edit,
            PermissionActions.Update, PermissionActions.QuickCreate, PermissionActions.Search, PermissionActions.Export);
        Allow(matrix, RoleKeys.Manager, PermissionModules.Employees,
            PermissionActions.View, PermissionActions.Details, PermissionActions.Edit, PermissionActions.Update,
            PermissionActions.ResetPassword);
        Allow(matrix, RoleKeys.Manager, PermissionModules.DineIn,
            PermissionActions.View, PermissionActions.Details, PermissionActions.Create, PermissionActions.QuickCreate,
            PermissionActions.MergeTransferTable, PermissionActions.ManageTable, PermissionActions.ManageQr,
            PermissionActions.Print, PermissionActions.ProcessPayment);
        Allow(matrix, RoleKeys.Manager, PermissionModules.OnlineOrders,
            PermissionActions.View, PermissionActions.Details, PermissionActions.Filter, PermissionActions.UpdateStatus,
            PermissionActions.ViewHistory, PermissionActions.Delete);
        Allow(matrix, RoleKeys.Manager, PermissionModules.Inventory,
            PermissionActions.View, PermissionActions.ImportStock, PermissionActions.AutoExportStock,
            PermissionActions.LowStockAlert, PermissionActions.ViewHistory, PermissionActions.LinkIngredient);
        Allow(matrix, RoleKeys.Manager, PermissionModules.Stores,
            PermissionActions.View, PermissionActions.Create, PermissionActions.Edit, PermissionActions.Update, PermissionActions.HideShow);
        Allow(matrix, RoleKeys.Manager, PermissionModules.Stories,
            PermissionActions.View, PermissionActions.Create, PermissionActions.Edit, PermissionActions.Update, PermissionActions.HideShow);
        Allow(matrix, RoleKeys.Manager, PermissionModules.ProductConditions,
            PermissionActions.View, PermissionActions.Create, PermissionActions.Edit, PermissionActions.Update);

        Allow(matrix, RoleKeys.Staff, PermissionModules.Products,
            PermissionActions.View, PermissionActions.Search, PermissionActions.Filter);
        Allow(matrix, RoleKeys.Staff, PermissionModules.Categories,
            PermissionActions.View, PermissionActions.Details);
        Allow(matrix, RoleKeys.Staff, PermissionModules.Orders,
            PermissionActions.View, PermissionActions.Details, PermissionActions.Filter, PermissionActions.Print, PermissionActions.ExportPdf);
        Allow(matrix, RoleKeys.Staff, PermissionModules.OrderDetails,
            PermissionActions.View, PermissionActions.Details, PermissionActions.Search, PermissionActions.Filter);
        Allow(matrix, RoleKeys.Staff, PermissionModules.Customers,
            PermissionActions.QuickCreate, PermissionActions.Search);
        Allow(matrix, RoleKeys.Staff, PermissionModules.Employees,
            PermissionActions.ViewOwn, PermissionActions.EditOwn, PermissionActions.ViewSchedule);
        Allow(matrix, RoleKeys.Staff, PermissionModules.DineIn,
            PermissionActions.View, PermissionActions.Details, PermissionActions.Create, PermissionActions.QuickCreate,
            PermissionActions.MergeTransferTable);
        Allow(matrix, RoleKeys.Staff, PermissionModules.OnlineOrders,
            PermissionActions.View, PermissionActions.Details, PermissionActions.Filter, PermissionActions.UpdateStatusLimited);
        Allow(matrix, RoleKeys.Staff, PermissionModules.Inventory,
            PermissionActions.ImportStock);

        return matrix;
    }

    private static void Allow(
        Dictionary<string, Dictionary<string, HashSet<string>>> matrix,
        string role,
        string module,
        params string[] actions)
    {
        var normalizedRole = NormalizeText(role);
        var normalizedModule = NormalizeText(module);

        if (!matrix.TryGetValue(normalizedRole, out var modulePermissions))
        {
            modulePermissions = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            matrix[normalizedRole] = modulePermissions;
        }

        if (!modulePermissions.TryGetValue(normalizedModule, out var allowedActions))
        {
            allowedActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            modulePermissions[normalizedModule] = allowedActions;
        }

        foreach (var action in actions)
        {
            allowedActions.Add(NormalizeText(action));
        }
    }

    private static bool IsViewEquivalentAction(string action)
    {
        return action is PermissionActions.List
            or PermissionActions.Details
            or PermissionActions.Search
            or PermissionActions.Filter;
    }

    private static string NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(character);
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('\u0111', 'd')
            .Replace('\u0110', 'd')
            .Replace('_', ' ')
            .Replace('-', ' ')
            .Trim();
    }
}
