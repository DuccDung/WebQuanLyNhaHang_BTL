using Microsoft.AspNetCore.Mvc;
using WebQuanLyNhaHang.Filters;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    public class AdminController : Controller
    {
        private readonly QlnhaHangBtlContext _context;

        public AdminController(QlnhaHangBtlContext context)
        {
            _context = context;
        }

        [AdminSessionAuthorize]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("NhanVienId").HasValue)
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string? name, string? password)
        {
            name = name?.Trim();
            password = password?.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Tên đăng nhập và mật khẩu không được để trống.";
                return View();
            }

            var nhanVien = _context.NhanViens.FirstOrDefault(e => e.TaiKhoan == name && e.MatKhau == password);
            if (nhanVien == null)
            {
                ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng.";
                return View();
            }

            HttpContext.Session.SetInt32("NhanVienId", nhanVien.NvId);
            HttpContext.Session.SetString("NhanVienName", nhanVien.TenNhanVien ?? nhanVien.TaiKhoan);
            HttpContext.Session.SetString("NhanVienTaiKhoan", nhanVien.TaiKhoan);

            return RedirectToAction(nameof(Index));
        }

        [AdminSessionAuthorize]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("NhanVienId");
            HttpContext.Session.Remove("NhanVienName");
            HttpContext.Session.Remove("NhanVienTaiKhoan");

            return RedirectToAction(nameof(Login));
        }

        [AdminSessionAuthorize]
        public IActionResult Ban()
        {
            ViewModelBan viewModelBan = new ViewModelBan(_context);
            return View(viewModelBan);
        }

        [HttpGet]
        [AdminSessionAuthorize]
        public IActionResult GetFormBuy(int id)
        {
            ViewModelGetFormBuy viewModel = new ViewModelGetFormBuy(_context);
            var donhangs = viewModel.CTDH_Product(id);
            return PartialView("GetFormBuy", donhangs);
        }

        [HttpGet]
        [AdminSessionAuthorize]
        public IActionResult ProcessPayment(int BanId)
        {
            var donHangList = _context.DonHangs.Where(e => e.BanId == BanId).ToList();

            foreach (var donHang in donHangList)
            {
                donHang.BanId = null;
            }

            _context.SaveChanges();
            return NoContent();
        }
    }
}
