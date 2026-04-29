using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            var kh = KHID.HasValue
                ? await _qlnhaHangBtlContext.KhachHangs.FirstOrDefaultAsync(customer => customer.KhId == KHID.Value && !customer.Remove)
                : null;
            if (kh == null)
            {
                HttpContext.Session.Remove("CustomerID");
                return Content(string.Empty);
            }
            return View("Acc2" , kh);  // gọi tới View
        }
    }
}
