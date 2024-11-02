using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Views.Shared.Components.AccLogginViewComponent
{
    public class AccLogginViewComponent : ViewComponent
    {
        QlnhaHangBtlContext _qlnhaHangBtlContext;
        public AccLogginViewComponent(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }
        public async Task<IViewComponentResult> InvokeAsync()  
        {
            int? NvID = HttpContext.Session.GetInt32("NhanVienId");

            var nv = _qlnhaHangBtlContext.NhanViens.Find(NvID);
            if (nv == null)
            {
                new Exception("Lỗi Loggin không tìm thấy Id Của Nhân Viên");
            }
            return View("Acc" , nv);  // gọi tới View
        }
    }
}
