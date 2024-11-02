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

        // Từ Đường Dẫn Lấy được số bàn rồi vào action này
        public IActionResult Client(int BanId)
        {
            // lấy được dữ liệu bàn
            HttpContext.Session.SetInt32("BanId", BanId); // Lưu Id Bàn vào Session
            // khi quét mã là đẳ đơn 1 lần => tạo 1 đơn hàng 
            var donHangMoi = new DonHang
            {
                GioRa = DateTime.Now
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
            int? DhId = HttpContext.Session.GetInt32("DhId"); // Lấy id đơn hàng
            ViewModelProductDetail viewModelProductDetail = new ViewModelProductDetail(_qlnhaHangBtlContext);
            var product = viewModelProductDetail.FindProductDetaiById(ProductID); // Dữ Liệu product đưa vào View
            // 
            var DH = _qlnhaHangBtlContext.DonHangs.Where(e => e.DhId == DhId).FirstOrDefault(); // lấy ra đơn hàng Của bàn đag quét
            if(DH != null)
            {
                var CTDH = _qlnhaHangBtlContext.ChiTietHoaDons
             .Where(e => e.ProductId == ProductID && e.DhId == DH.DhId).FirstOrDefault();
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
                throw new Exception("Lỗi Database trong file Home/ProductDetail");
            }
            return View(product);
        }
        [HttpPost]  // Đón dữ liệu từ chi tiết hóa đơn gửi lên
        public IActionResult CreateProductDetail(int soluong , int productid , string condition , string ghichu) // Thêm CTHD 
        {
            int? banId = HttpContext.Session.GetInt32("BanId"); // lấy dữ liệu ID bàn từ Sesion
            int? DhId = HttpContext.Session.GetInt32("DhId");
            var DH = _qlnhaHangBtlContext.DonHangs.Where(e => e.DhId == DhId).FirstOrDefault(); // lấy ra đơn hàng Từ cái bàn đó
        
            if(DH != null) // TH: Bàn đã Có đơn hàngn (thì ta tạo Thêm chi tiết hóa đơn)
            {
                _qlnhaHangBtlContext.ChiTietHoaDons.Add(new ChiTietHoaDon {
                    DhId = DH.DhId, // mã đơn hàng của bàn đó
                    ProductId = productid, // mã sản phẩm vừa thêm
                    Ghichu ="Trạng Thái: "+condition+" Ghi Chú: "+ghichu, // ghi chú
                    SoLuong = soluong, //số Lượng 
                });
                _qlnhaHangBtlContext.SaveChanges();
            }
            else
            {
                throw new Exception("Lỗi Tại trang Home/Create");
            }
            return RedirectToAction("Menu" , "Home"); // trở lại trang menu
        }


        public IActionResult Cart(int DhId) // fix
        {
            ViewModelCart viewModelCart = new ViewModelCart(_qlnhaHangBtlContext);
            return View(viewModelCart);
        }


        public IActionResult OrderSuccess() // để oder thành công ta kết nối đơn hàng tới bàn
        {
            int? banId = HttpContext.Session.GetInt32("BanId"); // lấy dữ liệu ID bàn từ Sesion
            int? DhId = HttpContext.Session.GetInt32("DhId");

            var DH = _qlnhaHangBtlContext.DonHangs.Find(DhId);
            DH.BanId = banId; 
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
            var CTHD = _qlnhaHangBtlContext.ChiTietHoaDons.Find(id);  // tìm chi tiết hóa đơn
            if (CTHD != null)
            {
                _qlnhaHangBtlContext.ChiTietHoaDons.Remove(CTHD); // xóa chi tiết hóa đơn
                _qlnhaHangBtlContext.SaveChanges();

                //dùng phương thức của signalR để nhận biết sự thay đổi của database
                _hubContext.Clients.All.SendAsync("DatabaseUpdated");
                // Trả về partial view với viewModel
                return NoContent();
            }
            else
            {
                throw new Exception("Lỗi xóa CTDH trong Cart");
            }
;        }

        public IActionResult GetCTHD() // id của cthd 
        {
              // Tạo ViewModel chứa dữ liệu cần thiết để render lại partial view
              ViewModelCart viewModelCart = new ViewModelCart(_qlnhaHangBtlContext);

              // Trả về partial view với viewModel
              return PartialView("CTDHTable", viewModelCart);
        }


        // ================================= trang Login =====================================================================
        [HttpPost]
        public IActionResult CustomerRegister(string username,string Email ,string password) {
            var KhachHang = _qlnhaHangBtlContext.KhachHangs.Where(e => e.TaiKhoan == username).FirstOrDefault();
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
                 MatKhau = password
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
       .FirstOrDefault(e => e.TaiKhoan == email && e.MatKhau == password);

            if (khachHang != null)
            {
                var id = khachHang.KhId;
                HttpContext.Session.SetInt32("CustomerID", id);
                // Chuyển hướng đến trang thành công
                return RedirectToAction("Index", "TrangChu");
            }
            else
            {
                TempData["error"] = "Tài Khoản Hoặc Mật Khẩu Sai!";
                return RedirectToAction("Index", "Home");
            }


        }

    }
}
