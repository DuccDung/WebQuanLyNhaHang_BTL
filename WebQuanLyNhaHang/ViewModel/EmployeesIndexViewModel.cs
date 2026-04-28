namespace WebQuanLyNhaHang.ViewModel;

public class EmployeesIndexViewModel
{
    public string AdminDisplayName { get; set; } = "Admin";

    public string AdminAccount { get; set; } = "admin";

    public string AdminRoleLabel { get; set; } = "Quản trị vận hành";

    public string Initials { get; set; } = "AD";

    public List<EmployeeIndexRowViewModel> Employees { get; set; } = new();

    public int TotalEmployees => Employees.Count;

    public int ManagerEmployees => Employees.Count(employee => employee.RoleKey == "manager");

    public int KitchenEmployees => Employees.Count(employee => employee.RoleKey == "kitchen");

    public int ServiceEmployees => Employees.Count(employee => employee.RoleKey == "service");
}

public class EmployeeIndexRowViewModel
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = "Nhân viên";

    public string Initial { get; set; } = "N";

    public string Address { get; set; } = "Chưa cập nhật địa chỉ";

    public string Phone { get; set; } = "Chưa có SĐT";

    public string Email { get; set; } = "Chưa có email";

    public DateOnly? StartDate { get; set; }

    public decimal Salary { get; set; }

    public string Account { get; set; } = string.Empty;

    public string PasswordMask { get; set; } = string.Empty;

    public string RoleKey { get; set; } = "staff";

    public string RoleLabel { get; set; } = "Nhân Viên";

    public string RoleCssClass { get; set; } = "is-staff";

    public string StatusLabel { get; set; } = "Đang Làm";

    public string StatusCssClass { get; set; } = "is-active";

    public string SearchText { get; set; } = string.Empty;
}
