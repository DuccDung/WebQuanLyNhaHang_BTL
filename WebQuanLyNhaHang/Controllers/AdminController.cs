using Microsoft.AspNetCore.Mvc;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    public class AdminController : Controller
    {
        private readonly QlnhaHangBtlContext _context;
        public AdminController(QlnhaHangBtlContext context) {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
		public IActionResult Login(string name, string password)
		{
			// Kiểm tra xem name và password có hợp lệ hay không
			if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
			{
				ViewBag.Error = "Tên đăng nhập và mật khẩu không được để trống.";
				return View();
			}

			// Tìm kiếm nhân viên dựa trên tên đăng nhập và mật khẩu
			var nhanVien = _context.NhanViens.FirstOrDefault(e => e.TaiKhoan == name && e.MatKhau == password);

			// Nếu tìm thấy, chuyển hướng đến trang Admin
			if (nhanVien != null)
			{
				HttpContext.Session.SetInt32("NhanVienId", nhanVien.NvId); // Lưu Id Bàn vào Session
				return RedirectToAction("Index", "Admin");
			}

			// Nếu không tìm thấy, đặt thông báo lỗi và trả về view
			ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng.";
			return View();
		}

		public IActionResult Ban()
		{
			ViewModelBan viewModelBan = new ViewModelBan(_context);
			return View(viewModelBan);
		}
		[HttpGet]
		public IActionResult GetFormBuy(int id) // nhận giá trị id bàn
		{
			ViewModelGetFormBuy viewModel = new ViewModelGetFormBuy(_context);
			var donhangs = viewModel.CTDH_Product(id);
			return PartialView("GetFormBuy" , donhangs); 
		}
        [HttpGet]
        public IActionResult ProcessPayment(int BanId)
        {
            // Lấy danh sách các đơn hàng có BanId nhất định
            var donHangList = _context.DonHangs.Where(e => e.BanId == BanId).ToList();

            // Cập nhật BanId của mỗi đơn hàng thành null
            foreach (var donHang in donHangList)
            {
                donHang.BanId = null;
            }

            // Lưu thay đổi vào cơ sở dữ liệu
           _context.SaveChanges();

			// Trả về partial view "BuySuccess"
			//return PartialView("_BuySuccess");
			return NoContent();
        }


    }
}
