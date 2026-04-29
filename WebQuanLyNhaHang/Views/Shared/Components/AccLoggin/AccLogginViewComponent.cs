using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.Views.Shared.Components.AccLogginViewComponent
{
    public class AccLogginViewComponent : ViewComponent
    {
        private readonly QlnhaHangBtlContext _qlnhaHangBtlContext;

        public AccLogginViewComponent(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int? NvID = HttpContext.Session.GetInt32("NhanVienId");

            if (!NvID.HasValue)
            {
                return Content(string.Empty);
            }

            var nv = await _qlnhaHangBtlContext.NhanViens
                .AsNoTracking()
                .FirstOrDefaultAsync(employee => employee.NvId == NvID.Value && !employee.Remove);

            if (nv == null)
            {
                HttpContext.Session.Remove("NhanVienId");
                HttpContext.Session.Remove("NhanVienName");
                HttpContext.Session.Remove("NhanVienTaiKhoan");
                return Content(string.Empty);
            }

            return View("Acc", nv);
        }
    }
}
