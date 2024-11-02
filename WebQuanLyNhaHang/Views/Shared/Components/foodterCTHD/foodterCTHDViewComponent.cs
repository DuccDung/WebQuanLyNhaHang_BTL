using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Views.Shared.Components.foodterCTHD
{
    public class foodterCTHDViewComponent : ViewComponent
    {
        QlnhaHangBtlContext _qlnhaHangBtlContext;
        public foodterCTHDViewComponent(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }
        public async Task<IViewComponentResult> InvokeAsync(int DhId) // CHi Tiết Hóa Đơn ID 
        {
            ViewModelMenu viewModelMenu = new ViewModelMenu(_qlnhaHangBtlContext);
            int soluong = viewModelMenu.CountProductDetail(DhId); // đếm số lượng món trong đơn hàng
            return View("QuantitySelector");  // gọi tới View
        }
    }
}
