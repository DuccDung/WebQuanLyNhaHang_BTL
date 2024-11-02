using Microsoft.AspNetCore.Mvc;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    public class TrangChuController : Controller
    {
        private readonly QlnhaHangBtlContext _qlnhaHangBtlContext;
        public TrangChuController(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Menu()
        {
            ViewModelMenu viewModelMenu = new ViewModelMenu(_qlnhaHangBtlContext);
            return View(viewModelMenu);
        }
    }
}
