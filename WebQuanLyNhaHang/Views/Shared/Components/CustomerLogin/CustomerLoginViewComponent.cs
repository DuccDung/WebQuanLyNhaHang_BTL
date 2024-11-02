using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Views.Shared.Components.CustomerLoginViewComponent
{
    public class CustomerLoginViewComponent:ViewComponent
    {
        QlnhaHangBtlContext _qlnhaHangBtlContext;
        public CustomerLoginViewComponent(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }
        public async Task<IViewComponentResult> InvokeAsync()  
        {
            int? KHID = HttpContext.Session.GetInt32("CustomerID");

            var kh = _qlnhaHangBtlContext.KhachHangs.Find(KHID);
            if (kh == null)
            {
                new Exception("Lỗi Loggin không tìm thấy Id Của Khách Hàng");
            }
            return View("Acc2" , kh);  // gọi tới View
        }
    }
}
