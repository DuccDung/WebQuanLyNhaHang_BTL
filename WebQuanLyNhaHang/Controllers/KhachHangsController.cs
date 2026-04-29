using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQuanLyNhaHang.Filters;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    [AdminSessionAuthorize]
    public class KhachHangsController : Controller
    {
        private readonly QlnhaHangBtlContext _context;

        public KhachHangsController(QlnhaHangBtlContext context)
        {
            _context = context;
        }

        // GET: KhachHangs
        public async Task<IActionResult> Index()
        {
            var rows = await BuildCustomerRowsAsync();

            var model = new CustomersIndexViewModel
            {
                AdminDisplayName = HttpContext.Session.GetString("NhanVienName") ?? "Admin",
                AdminAccount = HttpContext.Session.GetString("NhanVienTaiKhoan") ?? "admin",
                Initials = BuildInitials(HttpContext.Session.GetString("NhanVienName") ?? "Admin"),
                Customers = rows
            };

            return View(model);
        }

        // GET: KhachHangs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(m => m.KhId == id && !m.Remove);
            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsData(int id)
        {
            try
            {
            var khachHang = await _context.KhachHangs
                .AsNoTracking()
                .Include(customer => customer.DonHangs.Where(order => !order.Remove))
                .ThenInclude(order => order.Ban)
                .FirstOrDefaultAsync(customer => customer.KhId == id && !customer.Remove);

            if (khachHang == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng." });
            }

            var culture = CultureInfo.GetCultureInfo("vi-VN");
            var row = BuildCustomerRow(khachHang);
            var recentOrders = khachHang.DonHangs
                .Where(order => !order.Remove)
                .OrderByDescending(order => order.GioVao ?? order.GioRa)
                .ThenByDescending(order => order.DhId)
                .Take(5)
                .Select(order =>
                {
                    var orderTime = order.GioVao ?? order.GioRa;
                    var totalAmount = order.TongTien ?? 0m;

                    return new
                    {
                        orderCode = $"DH{order.DhId:000}",
                        orderTime = FormatDateTime(orderTime, culture),
                        tableLabel = order.BanId.HasValue ? $"Bàn {order.BanId.Value}" : "Mang về",
                        totalAmount = FormatCurrency(totalAmount, culture),
                        paymentLabel = totalAmount > 0m ? "Tiền mặt" : "Chưa thanh toán"
                    };
                })
                .ToList();

            return Json(new
            {
                customerId = row.CustomerId,
                customerCode = row.CustomerCode,
                customerName = row.CustomerName,
                initial = row.Initial,
                phone = row.Phone,
                email = row.Email,
                address = row.Address,
                account = string.IsNullOrWhiteSpace(khachHang.TaiKhoan) ? "Chưa có tài khoản" : khachHang.TaiKhoan.Trim(),
                password = string.IsNullOrWhiteSpace(khachHang.MatKhau) ? "Chưa có mật khẩu" : new string('•', Math.Min(8, khachHang.MatKhau.Trim().Length)),
                photo = string.IsNullOrWhiteSpace(khachHang.PathPhoto) ? "Chưa có ảnh đại diện" : khachHang.PathPhoto.Trim(),
                lastPurchaseLabel = FormatLastPurchase(row.LastPurchase, culture),
                orderCount = row.OrderCount.ToString("N0", culture),
                totalSpent = FormatCurrency(row.TotalSpent, culture),
                tierLabel = row.TierLabel,
                tierCssClass = row.TierCssClass,
                recentOrders
            });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Không tải được thông tin khách hàng. Vui lòng thử lại sau ít phút." });
            }
        }

        // GET: KhachHangs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: KhachHangs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("KhId,TenKhachHang,DiaChi,SoDienThoai,TaiKhoan,MatKhau,PathPhoto")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                khachHang.Remove = false;
                _context.Add(khachHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromModal([Bind("KhId,TenKhachHang,DiaChi,SoDienThoai,TaiKhoan,MatKhau,PathPhoto")] KhachHang khachHang)
        {
            if (string.IsNullOrWhiteSpace(khachHang.TenKhachHang))
            {
                ModelState.AddModelError(nameof(khachHang.TenKhachHang), "Vui lòng nhập tên khách hàng.");
            }

            if (string.IsNullOrWhiteSpace(khachHang.TaiKhoan))
            {
                ModelState.AddModelError(nameof(khachHang.TaiKhoan), "Vui lòng nhập tài khoản.");
            }

            if (string.IsNullOrWhiteSpace(khachHang.MatKhau))
            {
                ModelState.AddModelError(nameof(khachHang.MatKhau), "Vui lòng nhập mật khẩu.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    errors = ModelState
                        .Where(entry => entry.Value?.Errors.Count > 0)
                        .SelectMany(entry => entry.Value!.Errors.Select(error => error.ErrorMessage))
                        .Where(message => !string.IsNullOrWhiteSpace(message))
                        .ToList()
                });
            }

            khachHang.TenKhachHang = khachHang.TenKhachHang?.Trim();
            khachHang.DiaChi = khachHang.DiaChi?.Trim();
            khachHang.SoDienThoai = khachHang.SoDienThoai?.Trim();
            khachHang.TaiKhoan = khachHang.TaiKhoan.Trim();
            khachHang.MatKhau = khachHang.MatKhau.Trim();
            khachHang.PathPhoto = khachHang.PathPhoto?.Trim();
            khachHang.Remove = false;

            _context.Add(khachHang);
            await _context.SaveChangesAsync();

            var rows = await BuildCustomerRowsAsync();
            var row = rows.First(customer => customer.CustomerId == khachHang.KhId);
            var culture = CultureInfo.GetCultureInfo("vi-VN");

            return Json(new
            {
                success = true,
                message = "Đã thêm khách hàng mới.",
                customer = new
                {
                    customerId = row.CustomerId,
                    customerName = row.CustomerName,
                    initial = row.Initial,
                    phone = row.Phone,
                    email = row.Email,
                    address = row.Address,
                    lastPurchaseLabel = FormatLastPurchase(row.LastPurchase, culture),
                    orderCount = row.OrderCount.ToString("N0", culture),
                    totalSpent = FormatCurrency(row.TotalSpent, culture),
                    tierLabel = row.TierLabel,
                    tierCssClass = row.TierCssClass,
                    searchText = row.SearchText,
                    detailUrl = Url.Action(nameof(DetailsData), "KhachHangs", new { id = row.CustomerId }) ?? "#"
                },
                stats = new
                {
                    totalCustomers = rows.Count.ToString("N0", culture),
                    vipCustomers = rows.Count(customer => customer.TierKey == "vip").ToString("N0", culture),
                    regularCustomers = rows.Count(customer => customer.TierKey == "regular").ToString("N0", culture),
                    newCustomers = rows.Count(customer => customer.TierKey == "new").ToString("N0", culture)
                }
            });
        }

        // GET: KhachHangs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(customer => customer.KhId == id && !customer.Remove);
            if (khachHang == null)
            {
                return NotFound();
            }
            return View(khachHang);
        }

        // POST: KhachHangs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("KhId,TenKhachHang,DiaChi,SoDienThoai,TaiKhoan,MatKhau,PathPhoto")] KhachHang khachHang)
        {
            if (id != khachHang.KhId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.KhachHangs
                        .FirstOrDefaultAsync(customer => customer.KhId == id && !customer.Remove);

                    if (existingCustomer == null)
                    {
                        return NotFound();
                    }

                    existingCustomer.TenKhachHang = khachHang.TenKhachHang?.Trim();
                    existingCustomer.DiaChi = khachHang.DiaChi?.Trim();
                    existingCustomer.SoDienThoai = khachHang.SoDienThoai?.Trim();
                    existingCustomer.TaiKhoan = khachHang.TaiKhoan?.Trim() ?? string.Empty;
                    existingCustomer.MatKhau = khachHang.MatKhau?.Trim() ?? string.Empty;
                    existingCustomer.PathPhoto = khachHang.PathPhoto?.Trim();

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhachHangExists(khachHang.KhId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(khachHang);
        }

        // GET: KhachHangs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(m => m.KhId == id && !m.Remove);
            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        // POST: KhachHangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(customer => customer.KhId == id && !customer.Remove);
            if (khachHang != null)
            {
                khachHang.Remove = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KhachHangExists(int id)
        {
            return _context.KhachHangs.Any(e => e.KhId == id && !e.Remove);
        }

        private async Task<List<CustomerIndexRowViewModel>> BuildCustomerRowsAsync()
        {
            var customers = await _context.KhachHangs
                .AsNoTracking()
                .Where(customer => !customer.Remove)
                .Include(customer => customer.DonHangs.Where(order => !order.Remove))
                .ToListAsync();

            return customers
                .Select(BuildCustomerRow)
                .OrderByDescending(customer => customer.LastPurchase ?? DateTime.MinValue)
                .ThenBy(customer => customer.CustomerName)
                .ToList();
        }

        private static CustomerIndexRowViewModel BuildCustomerRow(KhachHang customer)
        {
            var orders = customer.DonHangs.Where(order => !order.Remove).ToList();
            var orderDates = orders
                .Select(order => order.GioVao ?? order.GioRa)
                .Where(date => date.HasValue)
                .Select(date => date!.Value);
            var lastPurchase = orderDates.Any()
                ? orderDates.Max()
                : (DateTime?)null;
            var orderCount = orders.Count;
            var totalSpent = orders.Sum(order => order.TongTien ?? 0m);
            var tier = ResolveCustomerTier(orderCount, totalSpent);
            var customerName = string.IsNullOrWhiteSpace(customer.TenKhachHang)
                ? "Khách lẻ"
                : customer.TenKhachHang!.Trim();
            var phone = string.IsNullOrWhiteSpace(customer.SoDienThoai)
                ? "Chưa có SĐT"
                : customer.SoDienThoai!.Trim();
            var email = ResolveEmail(customer.TaiKhoan);
            var address = string.IsNullOrWhiteSpace(customer.DiaChi)
                ? "Chưa cập nhật địa chỉ"
                : customer.DiaChi!.Trim();

            return new CustomerIndexRowViewModel
            {
                CustomerId = customer.KhId,
                CustomerCode = $"KH{customer.KhId:000}",
                CustomerName = customerName,
                Initial = BuildCustomerInitial(customerName),
                Phone = phone,
                Email = email,
                Address = address,
                LastPurchase = lastPurchase,
                OrderCount = orderCount,
                TotalSpent = totalSpent,
                TierKey = tier.Key,
                TierLabel = tier.Label,
                TierCssClass = tier.CssClass,
                SearchText = $"{customerName} {phone} {email} {address} KH{customer.KhId:000}".ToLowerInvariant()
            };
        }

        private static (string Key, string Label, string CssClass) ResolveCustomerTier(int orderCount, decimal totalSpent)
        {
            if (orderCount >= 20 || totalSpent >= 5_000_000m)
            {
                return ("vip", "VIP", "is-vip");
            }

            if (orderCount >= 10 || totalSpent >= 3_000_000m)
            {
                return ("regular", "Thường", "is-regular");
            }

            return ("new", "Mới", "is-new");
        }

        private static string ResolveEmail(string? account)
        {
            if (string.IsNullOrWhiteSpace(account))
            {
                return "Chưa có email";
            }

            var value = account.Trim();

            return value.Contains('@') ? value : "Chưa có email";
        }

        private static string BuildCustomerInitial(string name)
        {
            var normalized = string.IsNullOrWhiteSpace(name) ? "Khách" : name.Trim();
            return normalized[..1].ToUpper(CultureInfo.GetCultureInfo("vi-VN"));
        }

        private static string FormatCurrency(decimal value, CultureInfo culture)
        {
            return $"{value.ToString("N0", culture)}đ";
        }

        private static string FormatLastPurchase(DateTime? value, CultureInfo culture)
        {
            return value.HasValue
                ? $"Mua gần: {value.Value.ToString("dd/MM/yyyy", culture)}"
                : "Chưa có đơn hàng";
        }

        private static string FormatDateTime(DateTime? value, CultureInfo culture)
        {
            return value.HasValue
                ? value.Value.ToString("dd/MM/yyyy HH:mm", culture)
                : "Chưa có thời gian";
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
    }
}
