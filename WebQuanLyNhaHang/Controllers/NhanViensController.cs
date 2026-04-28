using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQuanLyNhaHang.Filters;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    [AdminSessionAuthorize]
    public class NhanViensController : Controller
    {
        private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");
        private readonly QlnhaHangBtlContext _context;

        public NhanViensController(QlnhaHangBtlContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _context.NhanViens
                .AsNoTracking()
                .Where(employee => !employee.Remove)
                .Include(employee => employee.NvPqs.Where(role => !role.Remove))
                    .ThenInclude(role => role.Pq)
                .ToListAsync();

            var adminName = HttpContext.Session.GetString("NhanVienName") ?? "Admin";
            var model = new EmployeesIndexViewModel
            {
                AdminDisplayName = adminName,
                AdminAccount = HttpContext.Session.GetString("NhanVienTaiKhoan") ?? "admin",
                AdminRoleLabel = "Quản trị vận hành",
                Initials = BuildInitials(adminName),
                Employees = employees
                    .Select(BuildEmployeeRow)
                    .OrderBy(employee => employee.EmployeeName)
                    .ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(item => item.NvId == id && !item.Remove);

            return nhanVien == null ? NotFound() : View(nhanVien);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsData(int id)
        {
            var employee = await LoadEmployeeAsync(id);

            if (employee == null)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên." });
            }

            return Json(BuildEmployeeDetailPayload(employee, await BuildRoleOptionsAsync()));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NvId,TenNhanVien,NgaySinh,DiaChi,HeSoLuong,PathPhoto,TaiKhoan,MatKhau")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("NvId,TenNhanVien,NgaySinh,DiaChi,HeSoLuong,TaiKhoan,MatKhau")] NhanVien nhanVien, IFormFile? fileImg)
        {
            var employee = await _context.NhanViens.FirstOrDefaultAsync(item => item.NvId == nhanVien.NvId && !item.Remove);

            if (employee == null)
            {
                return NotFound();
            }

            try
            {
                employee.TenNhanVien = nhanVien.TenNhanVien;
                employee.NgaySinh = nhanVien.NgaySinh;
                employee.DiaChi = nhanVien.DiaChi;
                employee.HeSoLuong = nhanVien.HeSoLuong;
                employee.TaiKhoan = nhanVien.TaiKhoan;
                employee.MatKhau = nhanVien.MatKhau;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NhanVienExists(nhanVien.NvId))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFromModal(EmployeeModalUpdateRequest request)
        {
            if (request.EmployeeId <= 0)
            {
                return BadRequest(new { message = "Thiếu mã nhân viên." });
            }

            var employee = await _context.NhanViens
                .Include(item => item.NvPqs.Where(role => !role.Remove))
                .FirstOrDefaultAsync(item => item.NvId == request.EmployeeId && !item.Remove);

            if (employee == null)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên." });
            }

            if (string.IsNullOrWhiteSpace(request.EmployeeName))
            {
                return BadRequest(new { message = "Tên nhân viên không được để trống." });
            }

            if (string.IsNullOrWhiteSpace(request.Account))
            {
                return BadRequest(new { message = "Tên tài khoản không được để trống." });
            }

            employee.TenNhanVien = request.EmployeeName.Trim();
            employee.NgaySinh = request.StartDate;
            employee.DiaChi = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
            employee.HeSoLuong = request.SalaryCoefficient ?? 0m;
            employee.TaiKhoan = request.Account.Trim();
            employee.MatKhau = string.IsNullOrWhiteSpace(request.Password) ? employee.MatKhau : request.Password.Trim();

            await SyncEmployeeRoleAsync(employee, request.RoleId);
            await _context.SaveChangesAsync();

            var refreshedEmployee = await LoadEmployeeAsync(employee.NvId);

            if (refreshedEmployee == null)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên sau khi cập nhật." });
            }

            var row = BuildEmployeeRow(refreshedEmployee);

            return Json(new
            {
                success = true,
                message = "Đã cập nhật nhân viên.",
                row = BuildEmployeeRowPayload(row),
                details = BuildEmployeeDetailPayload(refreshedEmployee, await BuildRoleOptionsAsync())
            });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.NhanViens
                .Include(item => item.NvPqs.Where(role => !role.Remove))
                .FirstOrDefaultAsync(item => item.NvId == id && !item.Remove);

            if (employee != null)
            {
                employee.Remove = true;

                foreach (var role in employee.NvPqs)
                {
                    role.Remove = true;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFromModal(int id)
        {
            var employee = await _context.NhanViens
                .Include(item => item.NvPqs.Where(role => !role.Remove))
                .FirstOrDefaultAsync(item => item.NvId == id && !item.Remove);

            if (employee == null)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên." });
            }

            employee.Remove = true;

            foreach (var role in employee.NvPqs)
            {
                role.Remove = true;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa mềm nhân viên." });
        }

        private bool NhanVienExists(int id)
        {
            return _context.NhanViens.Any(employee => employee.NvId == id && !employee.Remove);
        }

        private async Task<NhanVien?> LoadEmployeeAsync(int id)
        {
            return await _context.NhanViens
                .AsNoTracking()
                .Where(employee => employee.NvId == id && !employee.Remove)
                .Include(employee => employee.NvPqs.Where(role => !role.Remove))
                    .ThenInclude(role => role.Pq)
                .FirstOrDefaultAsync();
        }

        private async Task<List<EmployeeRoleOptionPayload>> BuildRoleOptionsAsync()
        {
            return await _context.PhanQuyens
                .AsNoTracking()
                .Where(role => !role.Remove)
                .OrderBy(role => role.TenQuyen)
                .Select(role => new EmployeeRoleOptionPayload
                {
                    id = role.PqId,
                    label = string.IsNullOrWhiteSpace(role.TenQuyen) ? $"Quyền #{role.PqId}" : role.TenQuyen!.Trim()
                })
                .ToListAsync();
        }

        private async Task SyncEmployeeRoleAsync(NhanVien employee, int? roleId)
        {
            if (!roleId.HasValue)
            {
                return;
            }

            var roleExists = await _context.PhanQuyens.AnyAsync(role => role.PqId == roleId.Value && !role.Remove);

            if (!roleExists)
            {
                return;
            }

            var activeRoles = employee.NvPqs.Where(role => !role.Remove).ToList();
            var primaryRole = activeRoles.FirstOrDefault();

            if (primaryRole == null)
            {
                employee.NvPqs.Add(new NvPq
                {
                    NvId = employee.NvId,
                    PqId = roleId.Value
                });
                return;
            }

            primaryRole.PqId = roleId.Value;

            foreach (var extraRole in activeRoles.Skip(1))
            {
                extraRole.Remove = true;
            }
        }

        private static EmployeeIndexRowViewModel BuildEmployeeRow(NhanVien employee)
        {
            var employeeName = string.IsNullOrWhiteSpace(employee.TenNhanVien)
                ? "Nhân viên"
                : employee.TenNhanVien!.Trim();
            var address = string.IsNullOrWhiteSpace(employee.DiaChi)
                ? "Chưa cập nhật địa chỉ"
                : employee.DiaChi!.Trim();
            var account = string.IsNullOrWhiteSpace(employee.TaiKhoan)
                ? string.Empty
                : employee.TaiKhoan.Trim();
            var roleName = employee.NvPqs
                .Where(role => !role.Remove && role.Pq != null && !role.Pq.Remove)
                .Select(role => role.Pq?.TenQuyen)
                .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));
            var role = ResolveRole(roleName);
            var phone = account.All(char.IsDigit) && !string.IsNullOrWhiteSpace(account)
                ? account
                : "Chưa có SĐT";
            var email = account.Contains('@')
                ? account
                : string.IsNullOrWhiteSpace(account) ? "Chưa có email" : $"{account}@restaurant.com";
            var salary = (employee.HeSoLuong ?? 0m) * 1_000_000m;
            var passwordLength = Math.Clamp(employee.MatKhau?.Length ?? 0, 3, 8);

            return new EmployeeIndexRowViewModel
            {
                EmployeeId = employee.NvId,
                EmployeeName = employeeName,
                Initial = BuildEmployeeInitial(employeeName),
                Address = address,
                Phone = phone,
                Email = email,
                StartDate = employee.NgaySinh,
                Salary = salary,
                Account = account,
                PasswordMask = new string('•', passwordLength),
                RoleKey = role.Key,
                RoleLabel = role.Label,
                RoleCssClass = role.CssClass,
                SearchText = $"{employeeName} {address} {phone} {email} {account} {role.Label}".ToLowerInvariant()
            };
        }

        private static (string Key, string Label, string CssClass) ResolveRole(string? roleName)
        {
            var normalized = (roleName ?? string.Empty).Trim().ToLowerInvariant();

            if (normalized.Contains("quản") || normalized.Contains("quan") || normalized.Contains("admin") || normalized.Contains("manager"))
            {
                return ("manager", "Quản Lý", "is-manager");
            }

            if (normalized.Contains("bếp") || normalized.Contains("bep") || normalized.Contains("chef") || normalized.Contains("kitchen"))
            {
                return ("kitchen", "Nhân Viên Bếp", "is-kitchen");
            }

            if (normalized.Contains("phục") || normalized.Contains("phuc") || normalized.Contains("phục vụ") || normalized.Contains("serve") || normalized.Contains("waiter"))
            {
                return ("service", "Phục Vụ", "is-service");
            }

            return ("staff", "Nhân Viên", "is-staff");
        }

        private static object BuildEmployeeDetailPayload(NhanVien employee, List<EmployeeRoleOptionPayload> roles)
        {
            var row = BuildEmployeeRow(employee);
            var role = employee.NvPqs
                .Where(item => !item.Remove && item.Pq != null && !item.Pq.Remove)
                .Select(item => new
                {
                    id = item.PqId,
                    name = item.Pq?.TenQuyen
                })
                .FirstOrDefault();

            return new
            {
                employeeId = employee.NvId,
                employeeName = row.EmployeeName,
                initial = row.Initial,
                address = row.Address,
                phone = row.Phone,
                email = row.Email,
                startDate = employee.NgaySinh?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                startDateLabel = FormatDate(employee.NgaySinh),
                salaryCoefficient = employee.HeSoLuong ?? 0m,
                salaryLabel = FormatCurrency(row.Salary),
                account = row.Account,
                password = employee.MatKhau ?? string.Empty,
                passwordMask = row.PasswordMask,
                roleId = role?.id,
                roleLabel = row.RoleLabel,
                roleKey = row.RoleKey,
                roleCssClass = row.RoleCssClass,
                statusLabel = row.StatusLabel,
                statusCssClass = row.StatusCssClass,
                searchText = row.SearchText,
                roles
            };
        }

        private static object BuildEmployeeRowPayload(EmployeeIndexRowViewModel row)
        {
            return new
            {
                employeeId = row.EmployeeId,
                employeeName = row.EmployeeName,
                initial = row.Initial,
                address = row.Address,
                phone = row.Phone,
                email = row.Email,
                startDateLabel = FormatDate(row.StartDate),
                salaryLabel = FormatCurrency(row.Salary),
                account = row.Account,
                passwordMask = row.PasswordMask,
                roleKey = row.RoleKey,
                roleLabel = row.RoleLabel,
                roleCssClass = row.RoleCssClass,
                statusLabel = row.StatusLabel,
                statusCssClass = row.StatusCssClass,
                searchText = row.SearchText
            };
        }

        private static string BuildEmployeeInitial(string name)
        {
            var normalized = string.IsNullOrWhiteSpace(name) ? "Nhân" : name.Trim();
            return normalized[..1].ToUpper(VietnameseCulture);
        }

        private static string BuildInitials(string name)
        {
            var parts = (name ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(2)
                .Select(part => char.ToUpperInvariant(part[0]));
            var initials = string.Concat(parts);

            return string.IsNullOrWhiteSpace(initials) ? "AD" : initials;
        }

        private static string FormatCurrency(decimal value)
        {
            return $"{value.ToString("N0", VietnameseCulture)}đ";
        }

        private static string FormatDate(DateOnly? value)
        {
            return value.HasValue ? value.Value.ToString("dd/MM/yyyy", VietnameseCulture) : "Chưa cập nhật";
        }
    }

    public class EmployeeModalUpdateRequest
    {
        public int EmployeeId { get; set; }

        public string? EmployeeName { get; set; }

        public DateOnly? StartDate { get; set; }

        public string? Address { get; set; }

        public decimal? SalaryCoefficient { get; set; }

        public string? Account { get; set; }

        public string? Password { get; set; }

        public int? RoleId { get; set; }
    }

    public class EmployeeRoleOptionPayload
    {
        public int id { get; set; }

        public string label { get; set; } = string.Empty;
    }
}
