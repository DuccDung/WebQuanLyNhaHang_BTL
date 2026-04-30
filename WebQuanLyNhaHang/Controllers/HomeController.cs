using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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
        private const string OnlineCartSessionKey = "OnlineCartDhId";
        private const string LegacyCartSessionKey = "DhId";

        public HomeController(ILogger<HomeController> logger , QlnhaHangBtlContext qlnhaHangBtlContext , IHubContext<ChatHub> hubContext)
        {
            _logger = logger;
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Account()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (!customerId.HasValue)
            {
                TempData["error"] = "Vui lòng đăng nhập để xem thông tin tài khoản.";
                return RedirectToAction("Index", "Home");
            }

            var khachHang = _qlnhaHangBtlContext.KhachHangs
                .FirstOrDefault(customer => customer.KhId == customerId.Value && !customer.Remove);

            if (khachHang == null)
            {
                HttpContext.Session.Remove("CustomerID");
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
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (!customerId.HasValue)
            {
                TempData["error"] = "Vui lòng đăng nhập để cập nhật thông tin tài khoản.";
                return RedirectToAction("Index", "Home");
            }

            var khachHang = await _qlnhaHangBtlContext.KhachHangs
                .FirstOrDefaultAsync(customer => customer.KhId == customerId.Value && !customer.Remove);

            if (khachHang == null)
            {
                HttpContext.Session.Remove("CustomerID");
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
            else
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
            // lấy được dữ liệu bàn
            HttpContext.Session.SetInt32("BanId", BanId); // Lưu Id Bàn vào Session
            // khi quét mã là đẳ đơn 1 lần => tạo 1 đơn hàng 
            var donHangMoi = new DonHang
            {
                BanId = BanId,
                GioRa = DateTime.Now,
                TrangThai = false,
                VanChuyen = false
            };

            _qlnhaHangBtlContext.DonHangs.Add(donHangMoi);
            _qlnhaHangBtlContext.SaveChanges();

            // Sau khi SaveChanges, ID sẽ được cập nhật tự động vào donHangMoi
            var DHID = donHangMoi.DhId; // Lấy ID của đơn hàng
            HttpContext.Session.SetInt32("DhId" , DHID);  // lưu đơn hàng id hiện tại vào session
            return View();
        }

        public IActionResult Service()
        {
            return View();
        }
        // Từ trang Client gửi tên và số bàn tới đây để thêm dữ liệu khách hàng vào bàn
        [HttpPost]
        public IActionResult CustomerInfo(string CustomerName) {
            HttpContext.Session.SetString("CustomerName", CustomerName);
            return RedirectToAction("Service" , "home"); // Đoạn này RedirecAction() về trang tiếp theo
        }

        public IActionResult Menu()
        {
            ViewModelMenu viewModelMenu = new ViewModelMenu(_qlnhaHangBtlContext);
            return View(viewModelMenu);
        }
        public IActionResult GetName(string txtsearch)
        {
            ViewModelMenu viewModelMenu = new ViewModelMenu(_qlnhaHangBtlContext);
            var results = viewModelMenu.ProductsBySearch(txtsearch);
            if(results == null)
            {
                Console.WriteLine("jjdj");
            }
            return PartialView("ProductTable", results);
        }
       

        public IActionResult ProductDetail(int ProductID)
        {
            int? banId = HttpContext.Session.GetInt32("BanId"); // lấy dữ liệu ID bàn từ Sesion
            int? DhId = GetActiveDineInOrderId(); // Lấy id đơn local
            ViewModelProductDetail viewModelProductDetail = new ViewModelProductDetail(_qlnhaHangBtlContext);
            var product = viewModelProductDetail.FindProductDetaiById(ProductID); // Dữ Liệu product đưa vào View
            // 
            var DH = _qlnhaHangBtlContext.DonHangs.Where(e => e.DhId == DhId && !e.Remove && e.VanChuyen != true).FirstOrDefault(); // lấy ra đơn hàng Của bàn đag quét
            if(DH != null)
            {
                var CTDH = _qlnhaHangBtlContext.ChiTietHoaDons
             .Where(e => e.ProductId == ProductID && e.DhId == DH.DhId && !e.Remove).FirstOrDefault();
                // chi tiết đơn hàng của sản phẩm
                if (CTDH == null)
                {  // trường hợp chưa có Chi tiết đơn hàng thì mặc định cho nó về 1
                    ViewData["SoLuong"] = 1;
                }
                else
                {
                    ViewData["SoLuong"] = CTDH.SoLuong;
                }
            }
            else
            {
                ViewData["SoLuong"] = 1;
            }
            return View(product);
        }
        [HttpPost]  // Đón dữ liệu từ chi tiết hóa đơn gửi lên
        public IActionResult CreateProductDetail(int soluong, int productid, string? condition, string? ghichu, string? returnUrl) // Thêm CTHD 
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
            customer.DiaChi = BuildFullAddress(form);
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

            RefreshCartTotal(DH.DhId);
            _qlnhaHangBtlContext.SaveChanges();
            HttpContext.Session.Remove(OnlineCartSessionKey);
            if (!HttpContext.Session.GetInt32("BanId").HasValue &&
                HttpContext.Session.GetInt32(LegacyCartSessionKey) == DH.DhId)
            {
                HttpContext.Session.Remove(LegacyCartSessionKey);
            }

            TempData["CartMessage"] = $"Đặt hàng thành công. Mã đơn của bạn là DH{DH.DhId:000}.";
            _hubContext.Clients.All.SendAsync("OderSuccess");
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
            DH.TrangThai = true;
            DH.VanChuyen = false;
            _qlnhaHangBtlContext.SaveChanges();
            //dùng phương thức của signalR để nhận biết sự thay đổi của database khi client đặt đơn hàng
            // Phát sự kiện qua SignalR
            _hubContext.Clients.All.SendAsync("OderSuccess");
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
            var dhId = HttpContext.Session.GetInt32(LegacyCartSessionKey);
            if (!dhId.HasValue)
            {
                return null;
            }

            var isLocalOrder = _qlnhaHangBtlContext.DonHangs
                .Any(order => order.DhId == dhId.Value && !order.Remove && order.VanChuyen != true);

            if (!isLocalOrder)
            {
                HttpContext.Session.Remove(LegacyCartSessionKey);
                return null;
            }

            return dhId;
        }

        private DonHang GetOrCreateDineInOrder()
        {
            var dhId = GetActiveDineInOrderId();
            var banId = HttpContext.Session.GetInt32("BanId");
            var DH = dhId.HasValue
                ? _qlnhaHangBtlContext.DonHangs
                    .FirstOrDefault(order => order.DhId == dhId.Value && !order.Remove && order.VanChuyen != true)
                : null;

            if (DH != null)
            {
                return DH;
            }

            DH = new DonHang
            {
                BanId = banId!.Value,
                GioRa = DateTime.Now,
                TrangThai = false,
                VanChuyen = false
            };

            _qlnhaHangBtlContext.DonHangs.Add(DH);
            _qlnhaHangBtlContext.SaveChanges();
            HttpContext.Session.SetInt32(LegacyCartSessionKey, DH.DhId);
            return DH;
        }

        private void RefreshCartTotal(int dhId)
        {
            var DH = _qlnhaHangBtlContext.DonHangs.FirstOrDefault(order => order.DhId == dhId && !order.Remove);
            if (DH == null)
            {
                return;
            }

            DH.TongTien = _qlnhaHangBtlContext.ChiTietHoaDons
                .Where(item => item.DhId == dhId && !item.Remove)
                .Sum(item => item.ThanhTien ?? 0m);

            _qlnhaHangBtlContext.SaveChanges();
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
                HttpContext.Session.SetInt32("CustomerID", id);
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
            HttpContext.Session.Remove("CustomerID");
            HttpContext.Session.Remove(OnlineCartSessionKey);
            TempData["success"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Index", "TrangChu");
        }

    }
}
