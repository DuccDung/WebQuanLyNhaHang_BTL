using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebQuanLyNhaHang.Hubs;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QlnhaHangBtlContext _qlnhaHangBtlContext;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IDataProtector _customerCookieProtector;
        private readonly IDataProtector _dineInCustomerCookieProtector;
        private const string OnlineCartSessionKey = "OnlineCartDhId";
        private const string LegacyCartSessionKey = "DhId";
        private const string CustomerSessionKey = "CustomerID";
        private const string CustomerCookieName = "CloudyCafeCustomer";
        private const string CustomerCookiePurpose = "CloudyCafe.CustomerCookie.v1";
        private const string DineInCustomerSessionKey = "CustomerName";
        private const string DineInCustomerIdSessionKey = "DineInCustomerId";
        private const string DineInCustomerCookieName = "CloudyCafeDineInCustomer";
        private const string DineInCustomerCookiePurpose = "CloudyCafe.DineInCustomerCookie.v1";
        private const string DineInOrderType = "dinein";

        public HomeController(ILogger<HomeController> logger , QlnhaHangBtlContext qlnhaHangBtlContext , IHubContext<ChatHub> hubContext, IDataProtectionProvider dataProtectionProvider)
        {
            _logger = logger;
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
            _hubContext = hubContext;
            _customerCookieProtector = dataProtectionProvider.CreateProtector(CustomerCookiePurpose);
            _dineInCustomerCookieProtector = dataProtectionProvider.CreateProtector(DineInCustomerCookiePurpose);
        }

        public IActionResult Index()
        {
            return View();
        }

        private int? GetCurrentCustomerId()
        {
            var customerId = HttpContext.Session.GetInt32(CustomerSessionKey);
            if (customerId.HasValue)
            {
                return customerId;
            }

            if (!Request.Cookies.TryGetValue(CustomerCookieName, out var protectedCustomerId))
            {
                return null;
            }

            try
            {
                var customerIdText = _customerCookieProtector.Unprotect(protectedCustomerId);
                if (!int.TryParse(customerIdText, out var id))
                {
                    ClearCustomerLogin();
                    return null;
                }

                var exists = _qlnhaHangBtlContext.KhachHangs
                    .Any(customer => customer.KhId == id && !customer.Remove);

                if (!exists)
                {
                    ClearCustomerLogin();
                    return null;
                }

                HttpContext.Session.SetInt32(CustomerSessionKey, id);
                return id;
            }
            catch
            {
                ClearCustomerLogin();
                return null;
            }
        }

        private void SetCustomerLogin(int customerId)
        {
            HttpContext.Session.SetInt32(CustomerSessionKey, customerId);
            Response.Cookies.Append(
                CustomerCookieName,
                _customerCookieProtector.Protect(customerId.ToString()),
                new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = Request.IsHttps
                });
        }

        private void ClearCustomerLogin()
        {
            HttpContext.Session.Remove(CustomerSessionKey);
            Response.Cookies.Delete(CustomerCookieName);
        }

        private DineInCustomerIdentity? GetCurrentDineInCustomer()
        {
            var sessionId = HttpContext.Session.GetString(DineInCustomerIdSessionKey);
            var sessionName = HttpContext.Session.GetString(DineInCustomerSessionKey);
            if (!string.IsNullOrWhiteSpace(sessionId) && !string.IsNullOrWhiteSpace(sessionName))
            {
                return new DineInCustomerIdentity(sessionId.Trim(), CleanDineInCustomerName(sessionName));
            }

            var remembered = GetRememberedDineInCustomer();
            if (remembered == null)
            {
                return null;
            }

            SetDineInCustomer(remembered);
            return remembered;
        }

        private DineInCustomerIdentity? GetRememberedDineInCustomer()
        {
            if (!Request.Cookies.TryGetValue(DineInCustomerCookieName, out var protectedValue))
            {
                return null;
            }

            try
            {
                var value = _dineInCustomerCookieProtector.Unprotect(protectedValue)?.Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    Response.Cookies.Delete(DineInCustomerCookieName);
                    return null;
                }

                if (value.StartsWith("{", StringComparison.Ordinal))
                {
                    var cookie = JsonSerializer.Deserialize<DineInCustomerCookie>(value);
                    if (!string.IsNullOrWhiteSpace(cookie?.Id) && !string.IsNullOrWhiteSpace(cookie.Name))
                    {
                        return new DineInCustomerIdentity(cookie.Id.Trim(), CleanDineInCustomerName(cookie.Name));
                    }
                }

                return new DineInCustomerIdentity(Guid.NewGuid().ToString("N"), CleanDineInCustomerName(value));
            }
            catch
            {
                Response.Cookies.Delete(DineInCustomerCookieName);
                return null;
            }
        }

        private DineInCustomerIdentity CreateDineInCustomer(string customerName)
        {
            return new DineInCustomerIdentity(Guid.NewGuid().ToString("N"), CleanDineInCustomerName(customerName));
        }

        private void SetDineInCustomer(DineInCustomerIdentity customer)
        {
            HttpContext.Session.SetString(DineInCustomerIdSessionKey, customer.Id);
            HttpContext.Session.SetString(DineInCustomerSessionKey, customer.Name);

            var cookie = JsonSerializer.Serialize(new DineInCustomerCookie
            {
                Id = customer.Id,
                Name = customer.Name
            });

            Response.Cookies.Append(
                DineInCustomerCookieName,
                _dineInCustomerCookieProtector.Protect(cookie),
                new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = Request.IsHttps
                });
        }

        private static string CleanDineInCustomerName(string customerName)
        {
            var name = customerName.Trim();
            return name.Length <= 18 ? name : name[..18];
        }

        public IActionResult Account()
        {
            var customerId = GetCurrentCustomerId();
            if (!customerId.HasValue)
            {
                TempData["error"] = "Vui lòng đăng nhập để xem thông tin tài khoản.";
                return RedirectToAction("Index", "Home");
            }

            var khachHang = _qlnhaHangBtlContext.KhachHangs
                .FirstOrDefault(customer => customer.KhId == customerId.Value && !customer.Remove);

            if (khachHang == null)
            {
                ClearCustomerLogin();
                TempData["error"] = "Không tìm thấy tài khoản. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index", "Home");
            }

            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Account(
            string? TenKhachHang,
            string? TaiKhoan,
            string? SoDienThoai,
            string? DiaChi,
            string? MatKhau,
            string? PathPhoto,
            IFormFile? PhotoFile)
        {
            var customerId = GetCurrentCustomerId();
            if (!customerId.HasValue)
            {
                TempData["error"] = "Vui lòng đăng nhập để cập nhật thông tin tài khoản.";
                return RedirectToAction("Index", "Home");
            }

            var khachHang = await _qlnhaHangBtlContext.KhachHangs
                .FirstOrDefaultAsync(customer => customer.KhId == customerId.Value && !customer.Remove);

            if (khachHang == null)
            {
                ClearCustomerLogin();
                TempData["error"] = "Không tìm thấy tài khoản. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(TenKhachHang))
            {
                ModelState.AddModelError(nameof(TenKhachHang), "Vui lòng nhập họ tên.");
            }

            if (string.IsNullOrWhiteSpace(TaiKhoan))
            {
                ModelState.AddModelError(nameof(TaiKhoan), "Vui lòng nhập email tài khoản.");
            }
            else
            {
                var account = TaiKhoan.Trim();
                var isDuplicate = await _qlnhaHangBtlContext.KhachHangs
                    .AnyAsync(customer =>
                        customer.KhId != khachHang.KhId &&
                        !customer.Remove &&
                        customer.TaiKhoan == account);

                if (isDuplicate)
                {
                    ModelState.AddModelError(nameof(TaiKhoan), "Email này đã được sử dụng.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(khachHang);
            }

            khachHang.TenKhachHang = TenKhachHang?.Trim();
            khachHang.TaiKhoan = TaiKhoan!.Trim();
            khachHang.SoDienThoai = SoDienThoai?.Trim();
            khachHang.DiaChi = DiaChi?.Trim();

            if (!string.IsNullOrWhiteSpace(MatKhau))
            {
                khachHang.MatKhau = MatKhau.Trim();
            }

            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(PhotoFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(PhotoFile), "Ảnh đại diện chỉ hỗ trợ JPG, PNG, GIF hoặc WebP.");
                    return View(khachHang);
                }

                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "customers");
                Directory.CreateDirectory(uploadFolder);
                var fileName = $"customer-{khachHang.KhId}-{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadFolder, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await PhotoFile.CopyToAsync(stream);
                }

                khachHang.PathPhoto = $"/uploads/customers/{fileName}";
            }
            else if (PathPhoto != null)
            {
                khachHang.PathPhoto = PathPhoto?.Trim();
            }

            await _qlnhaHangBtlContext.SaveChangesAsync();
            TempData["success"] = "Đã cập nhật thông tin tài khoản.";

            return RedirectToAction("Account", "Home");
        }

        // Từ Đường Dẫn Lấy được số bàn rồi vào action này
        public IActionResult Client(int BanId)
        {
            HttpContext.Session.SetInt32("BanId", BanId);

            var rememberedCustomer = GetRememberedDineInCustomer();
            if (rememberedCustomer != null)
            {
                SetDineInCustomer(rememberedCustomer);
                GetOrCreateDineInOrder();
                return RedirectToAction("Service", "Home");
            }

            return View();
        }

        public IActionResult Service()
        {
            ViewModelCart viewModelCart = new ViewModelCart(_qlnhaHangBtlContext);
            return View(viewModelCart);
        }

        [HttpPost]
        public async Task<IActionResult> SendDineInServiceRequest([FromBody] DineInServiceRequestForm? form)
        {
            var banId = HttpContext.Session.GetInt32("BanId");
            var sessionCustomerName = HttpContext.Session.GetString(DineInCustomerSessionKey);
            var customerName = string.IsNullOrWhiteSpace(sessionCustomerName)
                ? string.Empty
                : CleanDineInCustomerName(sessionCustomerName);
            var requestType = NormalizeDineInServiceRequestType(form?.Type);
            var createdAt = DateTime.Now;

            var payload = new
            {
                type = requestType,
                typeLabel = ResolveDineInServiceRequestLabel(requestType),
                tableId = banId,
                tableCode = banId.HasValue ? $"A{banId.Value}" : "A--",
                customerName = string.IsNullOrWhiteSpace(customerName) ? "Quý khách" : customerName,
                paymentMethod = form?.PaymentMethod?.Trim(),
                paymentMethodLabel = ResolvePaymentMethodLabel(form?.PaymentMethod),
                message = form?.Message?.Trim(),
                rating = form?.Rating,
                ratingLabel = ResolveReviewRatingLabel(form?.Rating),
                tags = form?.Tags?.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim()).ToArray() ?? Array.Empty<string>(),
                phone = form?.Phone?.Trim(),
                createdAt = createdAt.ToString("HH:mm dd/MM/yyyy")
            };

            await _hubContext.Clients.All.SendAsync("DineInServiceRequested", payload);

            return Json(new { success = true });
        }
        // Từ trang Client gửi tên và số bàn tới đây để thêm dữ liệu khách hàng vào bàn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CustomerInfo(string? CustomerName) {
            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                ModelState.AddModelError(nameof(CustomerName), "Vui lòng nhập tên của bạn.");
                ViewData["CustomerName"] = CustomerName;
                return View("Client");
            }

            var customer = CreateDineInCustomer(CustomerName);
            SetDineInCustomer(customer);
            GetOrCreateDineInOrder();
            return RedirectToAction("Service" , "home"); // Đoạn này RedirecAction() về trang tiếp theo
        }

        public IActionResult Menu()
        {
            ViewModelMenu viewModelMenu = new ViewModelMenu(_qlnhaHangBtlContext);
            return View(viewModelMenu);
        }
        public IActionResult GetName(string? txtsearch)
        {
            ViewModelMenu viewModelMenu = new ViewModelMenu(_qlnhaHangBtlContext);
            var results = viewModelMenu.ProductsBySearch(txtsearch);
            return PartialView("ProductTable", results);
        }
       

        public IActionResult ProductDetail(int ProductID, int? cthdId)
        {
            int? banId = HttpContext.Session.GetInt32("BanId"); // lấy dữ liệu ID bàn từ Sesion
            int? DhId = GetActiveDineInOrderId(); // Lấy id đơn local
            ViewModelProductDetail viewModelProductDetail = new ViewModelProductDetail(_qlnhaHangBtlContext);
            var product = viewModelProductDetail.FindProductDetaiById(ProductID); // Dữ Liệu product đưa vào View
            ViewData["CthdId"] = cthdId;
            ViewData["SelectedCondition"] = null;
            ViewData["GhiChu"] = null;
            // 
            var DH = _qlnhaHangBtlContext.DonHangs.Where(e => e.DhId == DhId && !e.Remove && e.VanChuyen != true).FirstOrDefault(); // lấy ra đơn hàng Của bàn đag quét
            if(DH != null)
            {
                var CTDH = _qlnhaHangBtlContext.ChiTietHoaDons
             .Where(e => e.ProductId == ProductID && e.DhId == DH.DhId && !e.Remove && (!cthdId.HasValue || e.CthdId == cthdId.Value)).FirstOrDefault();
                // chi tiết đơn hàng của sản phẩm
                if (CTDH == null)
                {  // trường hợp chưa có Chi tiết đơn hàng thì mặc định cho nó về 1
                    ViewData["SoLuong"] = 1;
                    ViewData["CthdId"] = null;
                }
                else
                {
                    ViewData["SoLuong"] = CTDH.SoLuong;
                    ViewData["CthdId"] = CTDH.CthdId;
                    var editState = ParseCartItemNote(CTDH.Ghichu);
                    ViewData["SelectedCondition"] = editState.Condition;
                    ViewData["GhiChu"] = editState.Note;
                }
            }
            else
            {
                ViewData["SoLuong"] = 1;
                ViewData["CthdId"] = null;
            }
            return View(product);
        }
        [HttpPost]  // Đón dữ liệu từ chi tiết hóa đơn gửi lên
        public IActionResult CreateProductDetail(int soluong, int productid, int? cthdId, string? condition, string? ghichu, string? returnUrl) // Thêm CTHD 
        {
            if (!HttpContext.Session.GetInt32("BanId").HasValue)
            {
                TempData["CartMessage"] = "Vui lòng chọn bàn trước khi gọi món tại quán.";
                return RedirectAfterAddToCart(returnUrl);
            }

            var product = _qlnhaHangBtlContext.Products
                .FirstOrDefault(item => item.ProductId == productid && !item.Remove);

            if (product == null)
            {
                TempData["CartMessage"] = "Không tìm thấy sản phẩm. Vui lòng chọn món lại.";
                return RedirectAfterAddToCart(returnUrl);
            }

            var quantity = Math.Max(1, soluong);
            var DH = GetOrCreateDineInOrder();
            var note = string.Join(" ", new[]
            {
                string.IsNullOrWhiteSpace(condition) ? null : $"Trạng Thái: {condition}",
                string.IsNullOrWhiteSpace(ghichu) ? null : $"Ghi Chú: {ghichu}"
            }.Where(item => item != null));
            var unitPrice = (product.GiaTien ?? 0m) + ResolveOptionExtra(condition, ghichu);

            if (cthdId.HasValue)
            {
                var editCartItem = _qlnhaHangBtlContext.ChiTietHoaDons
                    .FirstOrDefault(item =>
                        item.CthdId == cthdId.Value &&
                        item.DhId == DH.DhId &&
                        item.ProductId == productid &&
                        !item.Remove &&
                        item.Dh.VanChuyen != true);

                if (editCartItem != null)
                {
                    editCartItem.Ghichu = note;
                    editCartItem.SoLuong = quantity;
                    editCartItem.ThanhTien = unitPrice * quantity;

                    _qlnhaHangBtlContext.SaveChanges();
                    RefreshCartTotal(DH.DhId);
                    _hubContext.Clients.All.SendAsync("DatabaseUpdated");

                    return RedirectAfterAddToCart(returnUrl);
                }
            }

            var cartItem = _qlnhaHangBtlContext.ChiTietHoaDons
                .FirstOrDefault(item =>
                    item.DhId == DH.DhId &&
                    item.ProductId == productid &&
                    !item.Remove &&
                    (item.Ghichu ?? string.Empty) == note);

            if (cartItem == null)
            {
                cartItem = new ChiTietHoaDon
                {
                    DhId = DH.DhId,
                    ProductId = productid,
                    Ghichu = note,
                    SoLuong = quantity,
                    ThanhTien = unitPrice * quantity
                };

                _qlnhaHangBtlContext.ChiTietHoaDons.Add(cartItem);
            }
            else
            {
                var newQuantity = (cartItem.SoLuong ?? 0) + quantity;
                cartItem.SoLuong = newQuantity;
                cartItem.ThanhTien = unitPrice * newQuantity;
            }

            _qlnhaHangBtlContext.SaveChanges();
            RefreshCartTotal(DH.DhId);
            TempData["CartMessage"] = "Đã thêm sản phẩm vào giỏ hàng.";
            _hubContext.Clients.All.SendAsync("DatabaseUpdated");

            return RedirectAfterAddToCart(returnUrl);
        }

        [HttpPost]
        public IActionResult CreateOnlineProductDetail(int soluong, int productid, string? condition, string? ghichu, string? returnUrl)
        {
            if (!HttpContext.Session.GetInt32("CustomerID").HasValue)
            {
                TempData["error"] = "Vui lòng đăng nhập trước khi thêm sản phẩm vào giỏ hàng online.";
                return RedirectToAction("Index", "Home");
            }

            var product = _qlnhaHangBtlContext.Products
                .FirstOrDefault(item => item.ProductId == productid && !item.Remove);

            if (product == null)
            {
                TempData["OnlineCartMessage"] = "Không tìm thấy sản phẩm. Vui lòng chọn món lại.";
                return RedirectAfterOnlineAddToCart(returnUrl);
            }

            var quantity = Math.Max(1, soluong);
            var DH = GetOrCreateOnlineCartOrder();
            var note = string.Join(" ", new[]
            {
                string.IsNullOrWhiteSpace(condition) ? null : $"Trạng Thái: {condition}",
                string.IsNullOrWhiteSpace(ghichu) ? null : $"Ghi Chú: {ghichu}"
            }.Where(item => item != null));
            var unitPrice = (product.GiaTien ?? 0m) + ResolveOptionExtra(condition, ghichu);

            var cartItem = _qlnhaHangBtlContext.ChiTietHoaDons
                .FirstOrDefault(item =>
                    item.DhId == DH.DhId &&
                    item.ProductId == productid &&
                    !item.Remove &&
                    (item.Ghichu ?? string.Empty) == note);

            if (cartItem == null)
            {
                cartItem = new ChiTietHoaDon
                {
                    DhId = DH.DhId,
                    ProductId = productid,
                    Ghichu = note,
                    SoLuong = quantity,
                    ThanhTien = unitPrice * quantity
                };

                _qlnhaHangBtlContext.ChiTietHoaDons.Add(cartItem);
            }
            else
            {
                var newQuantity = (cartItem.SoLuong ?? 0) + quantity;
                cartItem.SoLuong = newQuantity;
                cartItem.ThanhTien = unitPrice * newQuantity;
            }

            _qlnhaHangBtlContext.SaveChanges();
            RefreshCartTotal(DH.DhId);
            TempData["OnlineCartMessage"] = "Đã thêm sản phẩm vào giỏ hàng online.";
            _hubContext.Clients.All.SendAsync("OnlineCartUpdated");

            return RedirectAfterOnlineAddToCart(returnUrl);
        }


        public IActionResult Cart(int DhId) // fix
        {
            GetActiveDineInOrderId();
            ViewModelCart viewModelCart = new ViewModelCart(_qlnhaHangBtlContext);
            return View(viewModelCart);
        }

        public IActionResult OnlineCart()
        {
            var cartOrder = GetActiveOnlineCartOrder();
            ViewData["OnlineCartId"] = cartOrder?.DhId;
            ViewModelCart viewModelCart = new ViewModelCart(_qlnhaHangBtlContext);
            return View(viewModelCart);
        }

        public async Task<IActionResult> OrderHistory()
        {
            var customerId = GetCurrentCustomerId();
            if (!customerId.HasValue)
            {
                TempData["error"] = "Vui lòng đăng nhập để xem lịch sử đơn hàng.";
                return RedirectToAction("Index", "Home");
            }

            var orders = await _qlnhaHangBtlContext.DonHangs
                .AsNoTracking()
                .Where(order =>
                    order.KhId == customerId.Value &&
                    !order.Remove &&
                    order.VanChuyen == true &&
                    order.GhiChu != null &&
                    order.GhiChu.Contains("\"t\":\"online\""))
                .Include(order => order.ChiTietHoaDons.Where(item => !item.Remove))
                    .ThenInclude(item => item.Product)
                .Include(order => order.OnlineOrderInfo)
                .OrderByDescending(order => order.GioVao ?? order.GioRa)
                .ThenByDescending(order => order.DhId)
                .ToListAsync();

            var model = new OnlineOrderHistoryViewModel
            {
                Orders = orders
                    .Select(order => new { Order = order, Metadata = OnlineOrderMetadata.TryParse(order.GhiChu) })
                    .Where(item =>
                        item.Metadata != null &&
                        !string.Equals(item.Metadata.DeliveryStatus, OnlineOrderMetadata.StatusCart, StringComparison.OrdinalIgnoreCase))
                    .Select(item => BuildOnlineOrderHistoryRow(item.Order, item.Metadata!))
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckoutOnlineOrder(OnlineCheckoutForm form)
        {
            var DH = GetActiveOnlineCartOrder();
            if (DH == null)
            {
                TempData["CartMessage"] = "Giỏ hàng online đang trống. Vui lòng chọn món trước khi đặt hàng.";
                return RedirectToAction("OnlineCart", "Home");
            }

            var hasItems = _qlnhaHangBtlContext.ChiTietHoaDons
                .Any(item => item.DhId == DH.DhId && !item.Remove);

            if (!hasItems)
            {
                TempData["CartMessage"] = "Giỏ hàng online đang trống. Vui lòng chọn món trước khi đặt hàng.";
                return RedirectToAction("OnlineCart", "Home");
            }

            var errors = ValidateCheckoutForm(form);
            if (errors.Count > 0)
            {
                TempData["CartMessage"] = string.Join(" ", errors);
                return RedirectToAction("OnlineCart", "Home");
            }

            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (!customerId.HasValue)
            {
                TempData["CartMessage"] = "Vui lòng đăng nhập trước khi đặt hàng online.";
                return RedirectToAction("OnlineCart", "Home");
            }

            var customer = _qlnhaHangBtlContext.KhachHangs
                .FirstOrDefault(item => item.KhId == customerId.Value && !item.Remove);

            if (customer == null)
            {
                TempData["CartMessage"] = "Không tìm thấy tài khoản khách hàng. Vui lòng đăng nhập lại.";
                return RedirectToAction("OnlineCart", "Home");
            }

            customer.TenKhachHang = CleanText(form.HoTen, 100);
            customer.SoDienThoai = CleanText(form.SoDienThoai, 15);
            customer.DiaChi = CleanText(BuildFullAddress(form), 200);
            DH.KhId = customer.KhId;

            var metadata = OnlineOrderMetadata.TryParse(DH.GhiChu) ?? OnlineOrderMetadata.CreateCart();
            metadata.DeliveryStatus = OnlineOrderMetadata.StatusPending;
            metadata.RecipientName = CleanText(form.HoTen, 80);
            metadata.Phone = CleanText(form.SoDienThoai, 20);
            metadata.City = CleanText(form.TinhThanh, 60);
            metadata.District = CleanText(form.QuanHuyen, 60);
            metadata.Ward = CleanText(form.PhuongXa, 60);
            metadata.AddressLine = CleanText(form.DiaChi, 180);
            metadata.Note = CleanText(form.GhiChu, 120);
            metadata.SubmittedAt = DateTime.Now;

            DH.GhiChu = metadata.ToJson();
            DH.TrangThai = true;
            DH.VanChuyen = true;
            DH.BanId = null;
            DH.GioVao ??= DateTime.Now;
            DH.GioRa = DateTime.Now;

            var onlineInfo = _qlnhaHangBtlContext.OnlineOrderInfos
                .FirstOrDefault(item => item.DhId == DH.DhId);
            if (onlineInfo == null)
            {
                onlineInfo = new OnlineOrderInfo
                {
                    DhId = DH.DhId
                };
                _qlnhaHangBtlContext.OnlineOrderInfos.Add(onlineInfo);
            }

            onlineInfo.TrangThaiGiaoHang = OnlineOrderMetadata.StatusPending;
            onlineInfo.NguoiNhan = metadata.RecipientName;
            onlineInfo.SoDienThoai = metadata.Phone;
            onlineInfo.TinhThanh = metadata.City;
            onlineInfo.QuanHuyen = metadata.District;
            onlineInfo.PhuongXa = metadata.Ward;
            onlineInfo.DiaChi = metadata.AddressLine;
            onlineInfo.GhiChuGiaoHang = metadata.Note;
            onlineInfo.NgayDat = metadata.SubmittedAt;
            onlineInfo.NgayCapNhat = DateTime.Now;
            onlineInfo.Remove = false;

            RefreshCartTotal(DH.DhId, saveChanges: false);
            _qlnhaHangBtlContext.SaveChanges();
            HttpContext.Session.Remove(OnlineCartSessionKey);
            if (!HttpContext.Session.GetInt32("BanId").HasValue &&
                HttpContext.Session.GetInt32(LegacyCartSessionKey) == DH.DhId)
            {
                HttpContext.Session.Remove(LegacyCartSessionKey);
            }

            TempData["CartMessage"] = $"Đặt hàng thành công. Mã đơn của bạn là DH{DH.DhId:000}.";
            _hubContext.Clients.All.SendAsync("OderSuccess", DH.DhId);
            _hubContext.Clients.All.SendAsync("OnlineOrderCreated", DH.DhId);

            return RedirectToAction("OnlineCart", "Home");
        }


        public IActionResult OrderSuccess() // để oder thành công ta kết nối đơn hàng tới bàn
        {
            int? banId = HttpContext.Session.GetInt32("BanId"); // lấy dữ liệu ID bàn từ Sesion
            int? DhId = GetActiveDineInOrderId();

            if (!banId.HasValue)
            {
                TempData["CartMessage"] = "Vui lòng chọn bàn trước khi gửi yêu cầu gọi món.";
                return RedirectToAction("Cart", "Home");
            }

            if (!DhId.HasValue)
            {
                TempData["CartMessage"] = "Giỏ hàng của bạn đang trống. Vui lòng chọn món trước khi gửi yêu cầu.";
                return RedirectToAction("Cart", "Home");
            }

            var DH = _qlnhaHangBtlContext.DonHangs.FirstOrDefault(e => e.DhId == DhId && !e.Remove && e.VanChuyen != true);
            if (DH == null)
            {
                HttpContext.Session.Remove("DhId");
                TempData["CartMessage"] = "Không tìm thấy giỏ hàng hiện tại. Vui lòng chọn món lại.";
                return RedirectToAction("Cart", "Home");
            }

            var hasItems = _qlnhaHangBtlContext.ChiTietHoaDons
                .Any(item => item.DhId == DH.DhId && !item.Remove);

            if (!hasItems)
            {
                TempData["CartMessage"] = "Giỏ hàng của bạn đang trống. Vui lòng chọn món trước khi gửi yêu cầu.";
                return RedirectToAction("Cart", "Home");
            }

            DH.BanId = banId; 
            var customer = GetCurrentDineInCustomer();
            if (customer != null)
            {
                var metadata = DineInOrderMetadata.Create(customer, banId.Value);
                metadata.Status = "submitted";
                DH.GhiChu = metadata.ToJson();
            }
            DH.TrangThai = true;
            DH.VanChuyen = false;
            _qlnhaHangBtlContext.SaveChanges();
            HttpContext.Session.Remove(LegacyCartSessionKey);
            //dùng phương thức của signalR để nhận biết sự thay đổi của database khi client đặt đơn hàng
            // Phát sự kiện qua SignalR
            _hubContext.Clients.All.SendAsync("OderSuccess", DH.DhId);
            return RedirectToAction("Service", "Home");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult RemoveItem(int id) // id của cthd 
        {
            var localDhId = GetActiveDineInOrderId();
            var CTHD = _qlnhaHangBtlContext.ChiTietHoaDons
                .FirstOrDefault(item =>
                    item.CthdId == id &&
                    item.DhId == localDhId &&
                    !item.Remove &&
                    item.Dh.VanChuyen != true);
            if (CTHD != null)
            {
                CTHD.Remove = true; // xóa mềm chi tiết hóa đơn
                _qlnhaHangBtlContext.SaveChanges();
                RefreshCartTotal(CTHD.DhId);

                //dùng phương thức của signalR để nhận biết sự thay đổi của database
                _hubContext.Clients.All.SendAsync("DatabaseUpdated");
                // Trả về partial view với viewModel
                ViewModelCart viewModelCart = new ViewModelCart(_qlnhaHangBtlContext);
                return PartialView("CTDHTable", viewModelCart);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            var localDhId = GetActiveDineInOrderId();
            if (!localDhId.HasValue)
            {
                ViewModelCart emptyViewModelCart = new ViewModelCart(_qlnhaHangBtlContext);
                return PartialView("CTDHTable", emptyViewModelCart);
            }

            var cartItems = _qlnhaHangBtlContext.ChiTietHoaDons
                .Where(item =>
                    item.DhId == localDhId.Value &&
                    !item.Remove &&
                    item.Dh.VanChuyen != true)
                .ToList();

            foreach (var item in cartItems)
            {
                item.Remove = true;
            }

            _qlnhaHangBtlContext.SaveChanges();
            RefreshCartTotal(localDhId.Value);

            _hubContext.Clients.All.SendAsync("DatabaseUpdated");
            ViewModelCart viewModelCart = new ViewModelCart(_qlnhaHangBtlContext);
            return PartialView("CTDHTable", viewModelCart);
        }

        [HttpPost]
        public IActionResult RemoveOnlineItem(int id)
        {
            var cartOrder = GetActiveOnlineCartOrder();
            if (cartOrder == null)
            {
                return NotFound();
            }

            var CTHD = _qlnhaHangBtlContext.ChiTietHoaDons
                .FirstOrDefault(item => item.CthdId == id && item.DhId == cartOrder.DhId && !item.Remove);
            if (CTHD == null)
            {
                return NotFound();
            }

            CTHD.Remove = true;
            _qlnhaHangBtlContext.SaveChanges();
            RefreshCartTotal(CTHD.DhId);

            _hubContext.Clients.All.SendAsync("OnlineCartUpdated");
            ViewData["OnlineCartId"] = cartOrder.DhId;
            ViewModelCart viewModelCart = new ViewModelCart(_qlnhaHangBtlContext);
            return PartialView("OnlineCTDHTable", viewModelCart);
        }

        [HttpPost]
        public IActionResult UpdateOnlineItemQuantity(int id, int quantity)
        {
            var cartOrder = GetActiveOnlineCartOrder();
            if (cartOrder == null)
            {
                return NotFound();
            }

            var CTHD = _qlnhaHangBtlContext.ChiTietHoaDons
                .Include(item => item.Product)
                .FirstOrDefault(item => item.CthdId == id && item.DhId == cartOrder.DhId && !item.Remove);

            if (CTHD == null)
            {
                return NotFound();
            }

            var newQuantity = Math.Max(1, quantity);
            var oldQuantity = Math.Max(1, CTHD.SoLuong ?? 1);
            var unitPrice = oldQuantity > 0
                ? (CTHD.ThanhTien ?? CTHD.Product?.GiaTien ?? 0m) / oldQuantity
                : CTHD.Product?.GiaTien ?? 0m;

            CTHD.SoLuong = newQuantity;
            CTHD.ThanhTien = unitPrice * newQuantity;

            _qlnhaHangBtlContext.SaveChanges();
            RefreshCartTotal(CTHD.DhId);

            _hubContext.Clients.All.SendAsync("OnlineCartUpdated");
            ViewData["OnlineCartId"] = cartOrder.DhId;
            ViewModelCart viewModelCart = new ViewModelCart(_qlnhaHangBtlContext);
            return PartialView("OnlineCTDHTable", viewModelCart);
        }

        public IActionResult GetCTHD() // id của cthd 
        {
              GetActiveDineInOrderId();
              // Tạo ViewModel chứa dữ liệu cần thiết để render lại partial view
              ViewModelCart viewModelCart = new ViewModelCart(_qlnhaHangBtlContext);

              // Trả về partial view với viewModel
              return PartialView("CTDHTable", viewModelCart);
        }

        public IActionResult GetOnlineCTHD()
        {
              var cartOrder = GetActiveOnlineCartOrder();
              ViewData["OnlineCartId"] = cartOrder?.DhId;
              ViewModelCart viewModelCart = new ViewModelCart(_qlnhaHangBtlContext);

              return PartialView("OnlineCTDHTable", viewModelCart);
        }

        private DonHang GetOrCreateOnlineCartOrder()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerID");
            if (!customerId.HasValue)
            {
                throw new InvalidOperationException("Online cart requires a logged-in customer.");
            }

            var DH = GetActiveOnlineCartOrder();

            if (DH == null)
            {
                var metadata = OnlineOrderMetadata.CreateCart();
                DH = new DonHang
                {
                    GioVao = DateTime.Now,
                    TongTien = 0,
                    KhId = customerId.Value,
                    BanId = null,
                    GhiChu = metadata.ToJson(),
                    TrangThai = false,
                    VanChuyen = true
                };

                _qlnhaHangBtlContext.DonHangs.Add(DH);
                _qlnhaHangBtlContext.SaveChanges();
                SetOnlineCartSession(DH.DhId);
                return DH;
            }

            if (!DH.KhId.HasValue)
            {
                DH.KhId = customerId.Value;
            }

            DH.VanChuyen = true;
            DH.TrangThai = false;
            if (OnlineOrderMetadata.TryParse(DH.GhiChu) == null)
            {
                DH.GhiChu = OnlineOrderMetadata.CreateCart().ToJson();
            }

            return DH;
        }

        private DonHang? GetActiveOnlineCartOrder()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (!customerId.HasValue)
            {
                HttpContext.Session.Remove(OnlineCartSessionKey);
                return null;
            }

            var cartId = HttpContext.Session.GetInt32(OnlineCartSessionKey);
            var DH = cartId.HasValue
                ? _qlnhaHangBtlContext.DonHangs
                    .FirstOrDefault(order =>
                        order.DhId == cartId.Value &&
                        order.KhId == customerId.Value &&
                        !order.Remove)
                : null;

            if (DH != null && OnlineOrderMetadata.IsOnlineCart(DH))
            {
                SetOnlineCartSession(DH.DhId);
                return DH;
            }

            if (cartId.HasValue)
            {
                HttpContext.Session.Remove(OnlineCartSessionKey);
            }

            DH = _qlnhaHangBtlContext.DonHangs
                .Where(order =>
                    order.KhId == customerId.Value &&
                    !order.Remove &&
                    order.VanChuyen == true &&
                    order.TrangThai != true &&
                    order.GhiChu != null &&
                    order.GhiChu.Contains("\"t\":\"online\"") &&
                    order.GhiChu.Contains("\"s\":\"cart\""))
                .OrderByDescending(order => order.GioVao ?? order.GioRa)
                .ThenByDescending(order => order.DhId)
                .FirstOrDefault();

            if (DH != null && OnlineOrderMetadata.IsOnlineCart(DH))
            {
                SetOnlineCartSession(DH.DhId);
            }

            return DH;
        }

        private void SetOnlineCartSession(int dhId)
        {
            HttpContext.Session.SetInt32(OnlineCartSessionKey, dhId);
        }

        private int? GetActiveDineInOrderId()
        {
            var banId = HttpContext.Session.GetInt32("BanId");
            var customer = GetCurrentDineInCustomer();
            var dhId = HttpContext.Session.GetInt32(LegacyCartSessionKey);

            if (dhId.HasValue)
            {
                var sessionOrder = _qlnhaHangBtlContext.DonHangs
                    .FirstOrDefault(order =>
                        order.DhId == dhId.Value &&
                        !order.Remove &&
                        order.VanChuyen != true &&
                        order.TrangThai != true &&
                        (!banId.HasValue || order.BanId == banId.Value));

                if (sessionOrder != null && IsDineInOrderForCustomer(sessionOrder, customer))
                {
                    return sessionOrder.DhId;
                }

                HttpContext.Session.Remove(LegacyCartSessionKey);
            }

            if (!banId.HasValue || customer == null)
            {
                return null;
            }

            var activeOrder = FindActiveDineInOrder(banId.Value, customer.Id);
            if (activeOrder == null)
            {
                return null;
            }

            HttpContext.Session.SetInt32(LegacyCartSessionKey, activeOrder.DhId);
            return activeOrder.DhId;
        }

        private DonHang GetOrCreateDineInOrder()
        {
            var dhId = GetActiveDineInOrderId();
            var banId = HttpContext.Session.GetInt32("BanId");
            var customer = GetCurrentDineInCustomer();
            var DH = dhId.HasValue
                ? _qlnhaHangBtlContext.DonHangs
                    .FirstOrDefault(order =>
                        order.DhId == dhId.Value &&
                        !order.Remove &&
                        order.VanChuyen != true &&
                        order.TrangThai != true &&
                        (!banId.HasValue || order.BanId == banId.Value))
                : null;

            if (DH != null)
            {
                if (customer != null && DineInOrderMetadata.TryParse(DH.GhiChu) == null)
                {
                    DH.GhiChu = DineInOrderMetadata.Create(customer, banId).ToJson();
                    _qlnhaHangBtlContext.SaveChanges();
                }

                return DH;
            }

            if (!banId.HasValue)
            {
                throw new InvalidOperationException("Dine-in order requires a table id.");
            }

            if (customer == null)
            {
                throw new InvalidOperationException("Dine-in order requires a remembered customer.");
            }

            DH = new DonHang
            {
                BanId = banId.Value,
                GioRa = DateTime.Now,
                GhiChu = DineInOrderMetadata.Create(customer, banId.Value).ToJson(),
                TrangThai = false,
                VanChuyen = false
            };

            _qlnhaHangBtlContext.DonHangs.Add(DH);
            _qlnhaHangBtlContext.SaveChanges();
            HttpContext.Session.SetInt32(LegacyCartSessionKey, DH.DhId);
            return DH;
        }

        private DonHang? FindActiveDineInOrder(int banId, string customerId)
        {
            var customerMarker = $"\"g\":\"{customerId}\"";
            return _qlnhaHangBtlContext.DonHangs
                .Where(order =>
                    order.BanId == banId &&
                    !order.Remove &&
                    order.VanChuyen != true &&
                    order.TrangThai != true &&
                    order.GhiChu != null &&
                    order.GhiChu.Contains("\"t\":\"dinein\"") &&
                    order.GhiChu.Contains(customerMarker))
                .OrderByDescending(order => order.GioVao ?? order.GioRa)
                .ThenByDescending(order => order.DhId)
                .FirstOrDefault();
        }

        private static bool IsDineInOrderForCustomer(DonHang order, DineInCustomerIdentity? customer)
        {
            var metadata = DineInOrderMetadata.TryParse(order.GhiChu);
            if (metadata == null)
            {
                return true;
            }

            return customer != null &&
                string.Equals(metadata.GuestId, customer.Id, StringComparison.OrdinalIgnoreCase);
        }

        private void RefreshCartTotal(int dhId, bool saveChanges = true)
        {
            var DH = _qlnhaHangBtlContext.DonHangs.FirstOrDefault(order => order.DhId == dhId && !order.Remove);
            if (DH == null)
            {
                return;
            }

            DH.TongTien = _qlnhaHangBtlContext.ChiTietHoaDons
                .Where(item => item.DhId == dhId && !item.Remove)
                .Sum(item => item.ThanhTien ?? 0m);

            if (saveChanges)
            {
                _qlnhaHangBtlContext.SaveChanges();
            }
        }

        private decimal ResolveOptionExtra(string? condition, string? ghichu)
        {
            decimal extra = 0;

            if (!string.IsNullOrWhiteSpace(condition) &&
                condition.Contains("Size L", StringComparison.OrdinalIgnoreCase))
            {
                extra += 10000m;
            }

            if (!string.IsNullOrWhiteSpace(ghichu))
            {
                var normalizedNote = StringUtils.ConvertToLowerAndRemoveDiacritics(ghichu);
                if (normalizedNote.Contains("sua tuoi", StringComparison.OrdinalIgnoreCase))
                {
                    extra += 5000m;
                }

                if (normalizedNote.Contains("sua yen mach", StringComparison.OrdinalIgnoreCase))
                {
                    extra += 5000m;
                }

                if (normalizedNote.Contains("sua dac", StringComparison.OrdinalIgnoreCase))
                {
                    extra += 5000m;
                }

                if (normalizedNote.Contains("foam dua", StringComparison.OrdinalIgnoreCase))
                {
                    extra += 10000m;
                }
            }

            return extra;
        }

        private static (string? Condition, string? Note) ParseCartItemNote(string? cartItemNote)
        {
            if (string.IsNullOrWhiteSpace(cartItemNote))
            {
                return (null, null);
            }

            const string conditionPrefix = "Trạng Thái:";
            const string notePrefix = "Ghi Chú:";
            var noteIndex = cartItemNote.IndexOf(notePrefix, StringComparison.OrdinalIgnoreCase);
            string? condition = null;
            string? note = null;

            if (cartItemNote.StartsWith(conditionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var conditionStart = conditionPrefix.Length;
                var conditionLength = noteIndex >= 0
                    ? noteIndex - conditionStart
                    : cartItemNote.Length - conditionStart;
                condition = cartItemNote.Substring(conditionStart, conditionLength).Trim();
            }

            if (noteIndex >= 0)
            {
                note = cartItemNote.Substring(noteIndex + notePrefix.Length).Trim();
            }

            return (condition, note);
        }

        private IActionResult RedirectAfterAddToCart(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Menu", "Home");
        }

        private IActionResult RedirectAfterOnlineAddToCart(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Menu", "TrangChu");
        }

        private static List<string> ValidateCheckoutForm(OnlineCheckoutForm form)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(form.HoTen))
            {
                errors.Add("Vui lòng nhập họ tên người nhận.");
            }

            if (string.IsNullOrWhiteSpace(form.SoDienThoai))
            {
                errors.Add("Vui lòng nhập số điện thoại.");
            }
            else
            {
                var phoneDigits = new string(form.SoDienThoai.Where(char.IsDigit).ToArray());
                if (phoneDigits.Length < 9 || phoneDigits.Length > 11)
                {
                    errors.Add("Số điện thoại nhận hàng chưa hợp lệ.");
                }
            }

            if (string.IsNullOrWhiteSpace(form.TinhThanh))
            {
                errors.Add("Vui lòng nhập tỉnh/thành phố.");
            }

            if (string.IsNullOrWhiteSpace(form.QuanHuyen))
            {
                errors.Add("Vui lòng nhập quận/huyện.");
            }

            if (string.IsNullOrWhiteSpace(form.DiaChi))
            {
                errors.Add("Vui lòng nhập địa chỉ nhận hàng.");
            }

            return errors;
        }

        private static string BuildFullAddress(OnlineCheckoutForm form)
        {
            return string.Join(", ", new[]
            {
                CleanText(form.DiaChi, 180),
                CleanText(form.PhuongXa, 60),
                CleanText(form.QuanHuyen, 60),
                CleanText(form.TinhThanh, 60)
            }.Where(item => !string.IsNullOrWhiteSpace(item)));
        }

        private static string? CleanText(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
        }

        private static OnlineOrderHistoryRowViewModel BuildOnlineOrderHistoryRow(DonHang order, OnlineOrderMetadata metadata)
        {
            var info = order.OnlineOrderInfo;
            var deliveryStatus = !string.IsNullOrWhiteSpace(info?.TrangThaiGiaoHang)
                ? info!.TrangThaiGiaoHang
                : metadata.DeliveryStatus;
            var status = ResolveCustomerStatusDisplay(deliveryStatus);
            var submittedAt = metadata.SubmittedAt ?? order.GioVao ?? order.GioRa;
            var items = order.ChiTietHoaDons
                .Where(item => !item.Remove)
                .Select(item => new OnlineOrderHistoryItemViewModel
                {
                    ProductName = string.IsNullOrWhiteSpace(item.Product?.TenSanPham)
                        ? "Sản phẩm"
                        : item.Product!.TenSanPham!.Trim(),
                    ProductPhoto = item.Product?.PathPhoto,
                    Quantity = Math.Max(0, item.SoLuong ?? 0),
                    LineTotal = item.ThanhTien ?? 0m,
                    Note = string.IsNullOrWhiteSpace(item.Ghichu) ? null : item.Ghichu.Trim()
                })
                .ToList();

            var total = order.TongTien ?? items.Sum(item => item.LineTotal);

            return new OnlineOrderHistoryRowViewModel
            {
                OrderId = order.DhId,
                OrderCode = $"DH{order.DhId:000}",
                SubmittedAt = submittedAt,
                TotalAmount = total,
                StatusKey = status.Key,
                StatusLabel = status.Label,
                StatusCssClass = status.CssClass,
                RecipientName = info?.NguoiNhan ?? metadata.RecipientName,
                Phone = info?.SoDienThoai ?? metadata.Phone,
                Address = BuildOnlineAddress(info) ?? metadata.FullAddress,
                Note = info?.GhiChuGiaoHang ?? metadata.Note,
                Items = items
            };
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

        private static (string Key, string Label, string CssClass) ResolveCustomerStatusDisplay(string? status)
        {
            return OnlineOrderMetadata.NormalizeStatus(status) switch
            {
                OnlineOrderMetadata.StatusPreparing => ("preparing", "Đang chuẩn bị", "is-processing"),
                OnlineOrderMetadata.StatusShipping => ("shipping", "Đang giao", "is-shipping"),
                OnlineOrderMetadata.StatusDelivered => ("delivered", "Đã giao", "is-completed"),
                OnlineOrderMetadata.StatusCancelled => ("cancelled", "Đã hủy", "is-cancelled"),
                _ => ("pending", "Chờ xác nhận", "is-pending")
            };
        }

        private static string NormalizeDineInServiceRequestType(string? type)
        {
            return type?.Trim().ToLowerInvariant() switch
            {
                "payment" => "payment",
                "staff" => "staff",
                "review" => "review",
                _ => "staff"
            };
        }

        private static string ResolveDineInServiceRequestLabel(string type)
        {
            return type switch
            {
                "payment" => "Gọi thanh toán",
                "review" => "Đánh giá",
                _ => "Gọi nhân viên"
            };
        }

        private static string? ResolvePaymentMethodLabel(string? method)
        {
            return method?.Trim().ToLowerInvariant() switch
            {
                "cash" => "Tiền mặt",
                "card" => "Thẻ ngân hàng",
                "wallet" => "Ứng dụng điện thoại",
                _ => null
            };
        }

        private static string? ResolveReviewRatingLabel(int? rating)
        {
            return rating switch
            {
                1 => "Rất không hài lòng",
                2 => "Không hài lòng",
                3 => "Bình thường",
                4 => "Hài lòng",
                5 => "Rất hài lòng",
                _ => null
            };
        }


        // ================================= trang Login =====================================================================
        [HttpPost]
        public IActionResult CustomerRegister(string username,string Email ,string password) {
            var KhachHang = _qlnhaHangBtlContext.KhachHangs
                .Where(e => e.TaiKhoan == Email && !e.Remove)
                .FirstOrDefault();
            if(KhachHang != null)
            {
                TempData["error"] = "Tên Tài Khoản Tài Đã Được Sử Dụng Vui Lòng Đăng Kí Lại!";
                return RedirectToAction("index" , "home");
            }
            else
            {
                _qlnhaHangBtlContext.KhachHangs.Add(new KhachHang
                {
                 TenKhachHang = username,   
                 TaiKhoan = Email,
                 MatKhau = password,
                 Remove = false
                });
                TempData["success"] = "Tài Khoản Đăng Kí Thành Công!";
                _qlnhaHangBtlContext.SaveChanges();
                return RedirectToAction("Index" , "Home");
            }
        }

        [HttpPost]
        public IActionResult CustomerLogin(string email, string password)
        {
            var khachHang = _qlnhaHangBtlContext.KhachHangs
       .FirstOrDefault(e => e.TaiKhoan == email && e.MatKhau == password && !e.Remove);

            if (khachHang != null)
            {
                var id = khachHang.KhId;
                SetCustomerLogin(id);
                var cartOrder = GetActiveOnlineCartOrder();
                if (cartOrder != null && !cartOrder.KhId.HasValue)
                {
                    cartOrder.KhId = id;
                    _qlnhaHangBtlContext.SaveChanges();
                }
                // Chuyển hướng đến trang thành công
                return RedirectToAction("Index", "TrangChu");
            }
            else
            {
                TempData["error"] = "Tài Khoản Hoặc Mật Khẩu Sai!";
                return RedirectToAction("Index", "Home");
            }


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CustomerLogout()
        {
            ClearCustomerLogin();
            HttpContext.Session.Remove(OnlineCartSessionKey);
            TempData["success"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Index", "TrangChu");
        }

        private sealed record DineInCustomerIdentity(string Id, string Name);

        private sealed class DineInCustomerCookie
        {
            public string? Id { get; set; }

            public string? Name { get; set; }
        }

        public sealed class DineInServiceRequestForm
        {
            public string? Type { get; set; }

            public string? PaymentMethod { get; set; }

            public string? Message { get; set; }

            public int? Rating { get; set; }

            public string[]? Tags { get; set; }

            public string? Phone { get; set; }
        }

        private sealed class DineInOrderMetadata
        {
            [JsonPropertyName("t")]
            public string Type { get; set; } = DineInOrderType;

            [JsonPropertyName("g")]
            public string GuestId { get; set; } = string.Empty;

            [JsonPropertyName("n")]
            public string? GuestName { get; set; }

            [JsonPropertyName("b")]
            public int? TableId { get; set; }

            [JsonPropertyName("s")]
            public string Status { get; set; } = "open";

            [JsonPropertyName("at")]
            public DateTime? CreatedAt { get; set; }

            public static DineInOrderMetadata Create(DineInCustomerIdentity customer, int? tableId)
            {
                return new DineInOrderMetadata
                {
                    Type = DineInOrderType,
                    GuestId = customer.Id,
                    GuestName = customer.Name,
                    TableId = tableId,
                    Status = "open",
                    CreatedAt = DateTime.Now
                };
            }

            public static DineInOrderMetadata? TryParse(string? value)
            {
                if (string.IsNullOrWhiteSpace(value) || !value.TrimStart().StartsWith("{", StringComparison.Ordinal))
                {
                    return null;
                }

                try
                {
                    var metadata = JsonSerializer.Deserialize<DineInOrderMetadata>(value);
                    return string.Equals(metadata?.Type, DineInOrderType, StringComparison.OrdinalIgnoreCase)
                        ? metadata
                        : null;
                }
                catch (JsonException)
                {
                    return null;
                }
            }

            public string ToJson()
            {
                GuestName = string.IsNullOrWhiteSpace(GuestName) ? null : CleanDineInCustomerName(GuestName);
                Status = string.IsNullOrWhiteSpace(Status) ? "open" : Status.Trim();

                return JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });
            }
        }

    }
}
