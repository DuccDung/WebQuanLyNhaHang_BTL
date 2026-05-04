using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using WebQuanLyNhaHang.Filters;
using WebQuanLyNhaHang.Hubs;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    [AdminSessionAuthorize]
    public class DonHangsController : Controller
    {
        private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");
        private readonly QlnhaHangBtlContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        public DonHangsController(QlnhaHangBtlContext context , IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: DonHangs
        public async Task<IActionResult> Index()
        {
            var orders = await _context.DonHangs
                .AsNoTracking()
                .Where(order =>
                    !order.Remove &&
                    order.VanChuyen == true &&
                    order.TrangThai == true &&
                    order.GhiChu != null &&
                    order.GhiChu.Contains("\"t\":\"online\""))
                .Include(order => order.Ban)
                .Include(order => order.Kh)
                .Include(order => order.Km)
                .Include(order => order.Nv)
                .Include(order => order.OnlineOrderInfo)
                .Include(order => order.ChiTietHoaDons.Where(item => !item.Remove))
                .OrderByDescending(order => order.OnlineOrderInfo != null ? order.OnlineOrderInfo.NgayDat : order.GioRa ?? order.GioVao)
                .ThenByDescending(order => order.DhId)
                .ToListAsync();

            var model = new OrdersIndexViewModel
            {
                AdminDisplayName = HttpContext.Session.GetString("NhanVienName") ?? "Admin",
                AdminAccount = HttpContext.Session.GetString("NhanVienTaiKhoan") ?? "admin",
                AdminRoleLabel = "Quản trị vận hành",
                Initials = BuildInitials(HttpContext.Session.GetString("NhanVienName") ?? "Admin"),
                Orders = orders
                    .Where(IsSubmittedOnlineOrder)
                    .Select(BuildOrderRow)
                    .ToList()
            };

            return View(model);
        }

        // GET: DonHangs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.Ban)
                .Include(d => d.Kh)
                .Include(d => d.Km)
                .Include(d => d.Nv)
                .FirstOrDefaultAsync(m => m.DhId == id && !m.Remove);
            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsData(int id)
        {
            var donHang = await _context.DonHangs
                .AsNoTracking()
                .Include(order => order.Ban)
                .Include(order => order.Kh)
                .Include(order => order.Km)
                .Include(order => order.Nv)
                .Include(order => order.OnlineOrderInfo)
                .Include(order => order.ChiTietHoaDons.Where(item => !item.Remove))
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(order => order.DhId == id && !order.Remove);

            if (donHang == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            var lineItems = donHang.ChiTietHoaDons
                .Where(item => !item.Remove)
                .Select(item =>
                {
                    var quantity = item.SoLuong ?? 0;
                    var lineTotal = item.ThanhTien ?? 0m;
                    var unitPrice = quantity > 0 && lineTotal > 0m
                        ? lineTotal / quantity
                        : item.Product?.GiaTien ?? 0m;

                    return new
                    {
                        productName = string.IsNullOrWhiteSpace(item.Product?.TenSanPham)
                            ? "Sản phẩm chưa đặt tên"
                            : item.Product!.TenSanPham!.Trim(),
                        quantity = quantity.ToString("N0", VietnameseCulture),
                        unitPrice = FormatCurrency(unitPrice),
                        total = FormatCurrency(lineTotal),
                        note = string.IsNullOrWhiteSpace(item.Ghichu) ? "Không có ghi chú" : item.Ghichu.Trim()
                    };
                })
                .ToList();

            var status = ResolveOrderStatus(donHang, lineItems.Count);
            var metadata = OnlineOrderMetadata.TryParse(donHang.GhiChu);
            var onlineInfo = donHang.OnlineOrderInfo;
            var orderTime = donHang.GioVao ?? donHang.GioRa;
            var totalAmount = donHang.TongTien ?? donHang.ChiTietHoaDons.Where(item => !item.Remove).Sum(item => item.ThanhTien ?? 0m);
            var hasActiveCustomer = donHang.Kh != null && !donHang.Kh.Remove;
            var customerName = !hasActiveCustomer || string.IsNullOrWhiteSpace(donHang.Kh?.TenKhachHang)
                ? "Khách lẻ"
                : donHang.Kh!.TenKhachHang!.Trim();
            var customerPhone = !hasActiveCustomer || string.IsNullOrWhiteSpace(donHang.Kh?.SoDienThoai)
                ? "Chưa có SĐT"
                : donHang.Kh!.SoDienThoai!.Trim();
            var tableLabel = donHang.BanId.HasValue ? $"Bàn {donHang.BanId.Value}" : "Mang về";

            var displayCustomerName = onlineInfo?.NguoiNhan ?? metadata?.RecipientName ?? customerName;
            var displayCustomerPhone = onlineInfo?.SoDienThoai ?? metadata?.Phone ?? customerPhone;
            var displayCustomerAddress = BuildOnlineAddress(onlineInfo) ?? (!string.IsNullOrWhiteSpace(metadata?.FullAddress)
                ? metadata.FullAddress
                : string.IsNullOrWhiteSpace(donHang.Kh?.DiaChi) ? "Chưa có địa chỉ" : donHang.Kh!.DiaChi!.Trim());
            var displayTableLabel = metadata != null ? "Giao hàng" : tableLabel;
            var displayPaymentLabel = metadata != null ? ResolveOnlinePaymentLabel(onlineInfo) : totalAmount > 0m ? "Tiền mặt" : "Chưa thanh toán";

            return Json(new
            {
                orderCode = $"DH{donHang.DhId:000}",
                customerName = displayCustomerName,
                customerPhone = displayCustomerPhone,
                customerAddress = displayCustomerAddress,
                tableLabel = displayTableLabel,
                createdTime = FormatDateTime(orderTime),
                timeIn = FormatDateTime(donHang.GioVao),
                timeOut = FormatDateTime(donHang.GioRa),
                totalAmount = FormatCurrency(totalAmount),
                paymentLabel = displayPaymentLabel,
                statusLabel = status.Label,
                statusCssClass = status.CssClass,
                employeeName = donHang.Nv == null || donHang.Nv.Remove || string.IsNullOrWhiteSpace(donHang.Nv.TenNhanVien)
                    ? "Chưa phân công"
                    : donHang.Nv.TenNhanVien!.Trim(),
                promotionName = string.IsNullOrWhiteSpace(donHang.Km?.TenKhuyenMai) ? "Không áp dụng" : donHang.Km!.TenKhuyenMai!.Trim(),
                items = lineItems
            });
        }

        [HttpGet]
        public async Task<IActionResult> DetailsDataV2(int id)
        {
            var donHang = await _context.DonHangs
                .AsNoTracking()
                .Include(order => order.Ban)
                .Include(order => order.Kh)
                .Include(order => order.Km)
                .Include(order => order.Nv)
                .Include(order => order.OnlineOrderInfo)
                .Include(order => order.ChiTietHoaDons.Where(item => !item.Remove))
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(order => order.DhId == id && !order.Remove);

            if (donHang == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            var metadata = OnlineOrderMetadata.TryParse(donHang.GhiChu);
            var lineItems = donHang.ChiTietHoaDons
                .Where(item => !item.Remove)
                .Select(item =>
                {
                    var quantity = item.SoLuong ?? 0;
                    var lineTotal = item.ThanhTien ?? 0m;
                    var unitPrice = quantity > 0 && lineTotal > 0m
                        ? lineTotal / quantity
                        : item.Product?.GiaTien ?? 0m;

                    return new
                    {
                        productName = string.IsNullOrWhiteSpace(item.Product?.TenSanPham)
                            ? "Sản phẩm chưa đặt tên"
                            : item.Product!.TenSanPham!.Trim(),
                        quantity = quantity.ToString("N0", VietnameseCulture),
                        unitPrice = FormatCurrency(unitPrice),
                        total = FormatCurrency(lineTotal),
                        note = string.IsNullOrWhiteSpace(item.Ghichu) ? "Không có ghi chú" : item.Ghichu.Trim()
                    };
                })
                .ToList();

            var status = ResolveOrderStatus(donHang, lineItems.Count);
            var onlineInfo = donHang.OnlineOrderInfo;
            var orderTime = donHang.GioVao ?? donHang.GioRa;
            var totalAmount = donHang.TongTien ?? donHang.ChiTietHoaDons.Where(item => !item.Remove).Sum(item => item.ThanhTien ?? 0m);
            var hasActiveCustomer = donHang.Kh != null && !donHang.Kh.Remove;
            var customerName = onlineInfo?.NguoiNhan ?? metadata?.RecipientName
                ?? (!hasActiveCustomer || string.IsNullOrWhiteSpace(donHang.Kh?.TenKhachHang)
                    ? "Khách lẻ"
                    : donHang.Kh!.TenKhachHang!.Trim());
            var customerPhone = onlineInfo?.SoDienThoai ?? metadata?.Phone
                ?? (!hasActiveCustomer || string.IsNullOrWhiteSpace(donHang.Kh?.SoDienThoai)
                    ? "Chưa có SĐT"
                    : donHang.Kh!.SoDienThoai!.Trim());
            var customerAddress = BuildOnlineAddress(onlineInfo) ?? (!string.IsNullOrWhiteSpace(metadata?.FullAddress)
                ? metadata.FullAddress
                : string.IsNullOrWhiteSpace(donHang.Kh?.DiaChi) ? "Chưa có địa chỉ" : donHang.Kh!.DiaChi!.Trim());
            var tableLabel = metadata != null
                ? "Giao hàng"
                : donHang.BanId.HasValue ? $"Bàn {donHang.BanId.Value}" : "Mang về";

            return Json(new
            {
                orderCode = $"DH{donHang.DhId:000}",
                customerName,
                customerPhone,
                customerAddress,
                tableLabel,
                createdTime = FormatDateTime(orderTime),
                timeIn = FormatDateTime(donHang.GioVao),
                timeOut = FormatDateTime(donHang.GioRa),
                totalAmount = FormatCurrency(totalAmount),
                paymentLabel = metadata != null ? ResolveOnlinePaymentLabel(onlineInfo) : totalAmount > 0m ? "Tiền mặt" : "Chưa thanh toán",
                statusKey = status.Key,
                statusLabel = status.Label,
                statusCssClass = status.CssClass,
                updateStatusUrl = metadata != null
                    ? Url.Action(nameof(UpdateDeliveryStatus), "DonHangs", new { id = donHang.DhId })
                    : null,
                statusOptions = metadata != null
                    ? BuildDeliveryStatusOptions()
                    : Array.Empty<object>(),
                employeeName = donHang.Nv == null || donHang.Nv.Remove || string.IsNullOrWhiteSpace(donHang.Nv.TenNhanVien)
                    ? "Chưa phân công"
                    : donHang.Nv.TenNhanVien!.Trim(),
                promotionName = string.IsNullOrWhiteSpace(donHang.Km?.TenKhuyenMai) ? "Không áp dụng" : donHang.Km!.TenKhuyenMai!.Trim(),
                items = lineItems
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDeliveryStatus(int id, string status)
        {
            if (!OnlineOrderMetadata.IsValidStatus(status) || status == OnlineOrderMetadata.StatusCart)
            {
                return BadRequest(new { message = "Trạng thái giao hàng không hợp lệ." });
            }

            var donHang = await _context.DonHangs
                .Include(order => order.OnlineOrderInfo)
                .FirstOrDefaultAsync(order => order.DhId == id && !order.Remove);

            if (donHang == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            var metadata = OnlineOrderMetadata.TryParse(donHang.GhiChu);
            if (metadata == null)
            {
                return BadRequest(new { message = "Đây không phải đơn hàng online." });
            }

            var oldStatus = donHang.OnlineOrderInfo?.TrangThaiGiaoHang ?? metadata.DeliveryStatus;
            metadata.DeliveryStatus = OnlineOrderMetadata.NormalizeStatus(status);
            donHang.GhiChu = metadata.ToJson();
            donHang.TrangThai = true;
            donHang.VanChuyen = true;

            if (donHang.OnlineOrderInfo == null)
            {
                donHang.OnlineOrderInfo = new OnlineOrderInfo
                {
                    DhId = donHang.DhId,
                    NguoiNhan = metadata.RecipientName,
                    SoDienThoai = metadata.Phone,
                    TinhThanh = metadata.City,
                    QuanHuyen = metadata.District,
                    PhuongXa = metadata.Ward,
                    DiaChi = metadata.AddressLine,
                    GhiChuGiaoHang = metadata.Note,
                    NgayDat = metadata.SubmittedAt
                };
            }

            donHang.OnlineOrderInfo.TrangThaiGiaoHang = metadata.DeliveryStatus;
            donHang.OnlineOrderInfo.NgayCapNhat = DateTime.Now;
            donHang.OnlineOrderInfo.Remove = false;

            if (metadata.DeliveryStatus == OnlineOrderMetadata.StatusDelivered ||
                metadata.DeliveryStatus == OnlineOrderMetadata.StatusCancelled)
            {
                donHang.GioRa = DateTime.Now;
            }

            _context.OnlineOrderStatusHistories.Add(new OnlineOrderStatusHistory
            {
                DhId = donHang.DhId,
                TrangThaiCu = oldStatus,
                TrangThaiMoi = metadata.DeliveryStatus,
                NvId = HttpContext.Session.GetInt32("NhanVienId"),
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("OnlineOrderStatusUpdated", id, metadata.DeliveryStatus);

            var display = OnlineOrderMetadata.ResolveStatusDisplay(metadata.DeliveryStatus);
            return Json(new
            {
                statusKey = display.Key,
                statusLabel = display.Label,
                statusCssClass = display.CssClass
            });
        }

        // GET: DonHangs/Create
        public IActionResult Create()
        {
            ViewData["BanId"] = new SelectList(_context.Bans.Where(item => !item.Remove), "BanId", "BanId");
            ViewData["KhId"] = new SelectList(_context.KhachHangs.Where(item => !item.Remove), "KhId", "KhId");
            ViewData["KmId"] = new SelectList(_context.KhuyenMais.Where(item => !item.Remove), "KmId", "KmId");
            ViewData["NvId"] = new SelectList(_context.NhanViens.Where(item => !item.Remove), "NvId", "NvId");
            return View();
        }

        // POST: DonHangs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DhId,KhId,BanId,KmId,GioVao,GioRa,TongTien,NvId")] DonHang donHang)
        {
            if (ModelState.IsValid)
            {
                _context.Add(donHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BanId"] = new SelectList(_context.Bans.Where(item => !item.Remove), "BanId", "BanId", donHang.BanId);
            ViewData["KhId"] = new SelectList(_context.KhachHangs.Where(item => !item.Remove), "KhId", "KhId", donHang.KhId);
            ViewData["KmId"] = new SelectList(_context.KhuyenMais.Where(item => !item.Remove), "KmId", "KmId", donHang.KmId);
            ViewData["NvId"] = new SelectList(_context.NhanViens.Where(item => !item.Remove), "NvId", "NvId", donHang.NvId);
            return View(donHang);
        }

        // GET: DonHangs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs.FirstOrDefaultAsync(item => item.DhId == id && !item.Remove);
            if (donHang == null)
            {
                return NotFound();
            }
            ViewData["BanId"] = new SelectList(_context.Bans.Where(item => !item.Remove), "BanId", "BanId", donHang.BanId);
            ViewData["KhId"] = new SelectList(_context.KhachHangs.Where(item => !item.Remove), "KhId", "KhId", donHang.KhId);
            ViewData["KmId"] = new SelectList(_context.KhuyenMais.Where(item => !item.Remove), "KmId", "KmId", donHang.KmId);
            ViewData["NvId"] = new SelectList(_context.NhanViens.Where(item => !item.Remove), "NvId", "NvId", donHang.NvId);
            return View(donHang);
        }

        // POST: DonHangs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DhId,KhId,BanId,KmId,GioVao,GioRa,TongTien,NvId")] DonHang donHang)
        {
            if (id != donHang.DhId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(donHang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonHangExists(donHang.DhId))
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
            ViewData["BanId"] = new SelectList(_context.Bans.Where(item => !item.Remove), "BanId", "BanId", donHang.BanId);
            ViewData["KhId"] = new SelectList(_context.KhachHangs.Where(item => !item.Remove), "KhId", "KhId", donHang.KhId);
            ViewData["KmId"] = new SelectList(_context.KhuyenMais.Where(item => !item.Remove), "KmId", "KmId", donHang.KmId);
            ViewData["NvId"] = new SelectList(_context.NhanViens.Where(item => !item.Remove), "NvId", "NvId", donHang.NvId);
            return View(donHang);
        }

        // GET: DonHangs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.Ban)
                .Include(d => d.Kh)
                .Include(d => d.Km)
                .Include(d => d.Nv)
                .FirstOrDefaultAsync(m => m.DhId == id && !m.Remove);
            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        // POST: DonHangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang != null)
            {
                donHang.Remove = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DonHangExists(int id)
        {
            return _context.DonHangs.Any(e => e.DhId == id && !e.Remove);
        }

        private static OrderIndexRowViewModel BuildOrderRow(DonHang order)
        {
            var lineItemCount = order.ChiTietHoaDons?.Count(item => !item.Remove) ?? 0;
            var status = ResolveOrderStatus(order, lineItemCount);
            var metadata = OnlineOrderMetadata.TryParse(order.GhiChu);
            var hasActiveCustomer = order.Kh != null && !order.Kh.Remove;
            var customerName = !hasActiveCustomer || string.IsNullOrWhiteSpace(order.Kh?.TenKhachHang)
                ? "Khách lẻ"
                : order.Kh!.TenKhachHang!.Trim();
            var customerPhone = !hasActiveCustomer || string.IsNullOrWhiteSpace(order.Kh?.SoDienThoai)
                ? "Chưa có SĐT"
                : order.Kh!.SoDienThoai!.Trim();
            var onlineInfo = order.OnlineOrderInfo;
            var customerAddress = BuildOnlineAddress(onlineInfo) ?? (!string.IsNullOrWhiteSpace(metadata?.FullAddress)
                ? metadata.FullAddress
                : string.IsNullOrWhiteSpace(order.Kh?.DiaChi) ? "Chưa có địa chỉ" : order.Kh!.DiaChi!.Trim());
            var displayCustomerName = onlineInfo?.NguoiNhan ?? metadata?.RecipientName ?? customerName;
            var displayCustomerPhone = onlineInfo?.SoDienThoai ?? metadata?.Phone ?? customerPhone;
            var displayTableLabel = "Giao hàng";
            var displayPaymentLabel = ResolveOnlinePaymentLabel(onlineInfo);
            var orderItems = order.ChiTietHoaDons ?? Enumerable.Empty<ChiTietHoaDon>();
            var totalAmount = order.TongTien ?? orderItems.Where(item => !item.Remove).Sum(item => item.ThanhTien ?? 0m);
            var submittedAt = onlineInfo?.NgayDat ?? metadata?.SubmittedAt ?? order.GioRa ?? order.GioVao;

            return new OrderIndexRowViewModel
            {
                OrderId = order.DhId,
                OrderCode = $"DH{order.DhId:000}",
                CustomerName = displayCustomerName,
                CustomerPhone = displayCustomerPhone,
                CustomerAddress = customerAddress,
                TableLabel = displayTableLabel,
                OrderTime = submittedAt,
                DateValue = submittedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                TotalAmount = totalAmount,
                PaymentLabel = displayPaymentLabel,
                StatusKey = status.Key,
                StatusLabel = status.Label,
                StatusCssClass = status.CssClass,
                SearchText = $"{order.DhId} DH{order.DhId:000} {displayCustomerName} {displayCustomerPhone} {customerAddress} {displayTableLabel}".ToLowerInvariant()
            };
        }

        private static bool IsSubmittedOnlineOrder(DonHang order)
        {
            var metadata = OnlineOrderMetadata.TryParse(order.GhiChu);
            if (metadata == null || order.VanChuyen != true)
            {
                return false;
            }

            var deliveryStatus = !string.IsNullOrWhiteSpace(order.OnlineOrderInfo?.TrangThaiGiaoHang)
                ? order.OnlineOrderInfo.TrangThaiGiaoHang
                : metadata.DeliveryStatus;

            return !string.Equals(
                OnlineOrderMetadata.NormalizeStatus(deliveryStatus),
                OnlineOrderMetadata.StatusCart,
                StringComparison.OrdinalIgnoreCase);
        }

        private static (string Key, string Label, string CssClass) ResolveOrderStatus(DonHang order, int lineItemCount)
        {
            var metadata = OnlineOrderMetadata.TryParse(order.GhiChu);
            if (!string.IsNullOrWhiteSpace(order.OnlineOrderInfo?.TrangThaiGiaoHang))
            {
                return OnlineOrderMetadata.ResolveStatusDisplay(order.OnlineOrderInfo.TrangThaiGiaoHang);
            }

            if (metadata != null)
            {
                return OnlineOrderMetadata.ResolveStatusDisplay(metadata.DeliveryStatus);
            }

            if (!order.BanId.HasValue && order.TongTien.HasValue && order.TongTien > 0m)
            {
                return ("completed", "Hoàn thành", "is-completed");
            }

            if (lineItemCount > 0 || order.BanId.HasValue)
            {
                return ("processing", "Đang chế biến", "is-processing");
            }

            return ("pending", "Chờ xử lý", "is-pending");
        }

        private static object[] BuildDeliveryStatusOptions()
        {
            return new object[]
            {
                new { value = OnlineOrderMetadata.StatusPending, label = "Chờ xác nhận" },
                new { value = OnlineOrderMetadata.StatusPreparing, label = "Đang chuẩn bị" },
                new { value = OnlineOrderMetadata.StatusShipping, label = "Đang giao" },
                new { value = OnlineOrderMetadata.StatusDelivered, label = "Đã giao" },
                new { value = OnlineOrderMetadata.StatusCancelled, label = "Đã hủy" }
            };
        }

        private static string ResolveOnlinePaymentLabel(OnlineOrderInfo? info)
        {
            var payment = info?.PhuongThucThanhToan?.Trim();
            return string.IsNullOrWhiteSpace(payment) ? "COD" : payment;
        }

        private static string? BuildOnlineAddress(OnlineOrderInfo? info)
        {
            if (info == null)
            {
                return null;
            }

            var address = string.Join(", ", new[]
            {
                info.DiaChi,
                info.PhuongXa,
                info.QuanHuyen,
                info.TinhThanh
            }.Where(item => !string.IsNullOrWhiteSpace(item)));

            return string.IsNullOrWhiteSpace(address) ? null : address;
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

        private static string FormatDateTime(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("dd/MM/yyyy HH:mm", VietnameseCulture) : "Chưa có thời gian";
        }
    }
}
