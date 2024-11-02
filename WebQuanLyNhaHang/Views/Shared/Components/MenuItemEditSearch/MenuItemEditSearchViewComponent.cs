using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Views.Shared.Components.MenuItemEditSearch
{
    public class MenuItemEditSearchViewComponent : ViewComponent
    {
        QlnhaHangBtlContext _qlnhaHangBtlContext;
        public MenuItemEditSearchViewComponent(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }
        public async Task<IViewComponentResult> InvokeAsync(int  ProductId , int DhId) // CHi Tiết Hóa Đơn ID 
        {
            var CTHD = _qlnhaHangBtlContext.ChiTietHoaDons.Where(e => e.ProductId == ProductId && e.DhId == DhId).FirstOrDefault();
            int? soluong = CTHD.SoLuong;
            if(soluong == 0) // sản phẩm bị xóa đến 0 thì remove chi tiết hóa đơn
            {
                _qlnhaHangBtlContext.ChiTietHoaDons.Remove(CTHD);
                _qlnhaHangBtlContext.SaveChanges();
            }
            var model = new QuantitySelector
            {
                Soluong = soluong,
                ProductId = ProductId,
                DhId = DhId
            };
            return View("QuantitySelector", model);  // gọi tới View
        }
    }
}
