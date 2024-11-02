using Microsoft.AspNetCore.Mvc;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Views.Shared.Components.MenuAddSub
{
    
    public class MenuAddSubViewComponent : ViewComponent
    {
        QlnhaHangBtlContext _qlnhaHangBtlContext;
        public MenuAddSubViewComponent(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }
        public async Task<IViewComponentResult> InvokeAsync(int ProductId, int DhId)
        {
            
            ViewModelMenu viewModelMenu = new ViewModelMenu(_qlnhaHangBtlContext);
            
            if (viewModelMenu.CountProductDetail(DhId, ProductId) != 0)
            {
                var CTHD = _qlnhaHangBtlContext.ChiTietHoaDons.Where(e => e.ProductId == ProductId && e.DhId == DhId).FirstOrDefault();
                int? soluong = CTHD.SoLuong;
                var model = new QuantitySelector
            {
                Soluong = soluong,
                ProductId = ProductId,
                DhId = DhId
            };
            
                return View("QuantitySelector", model);  // số lượng lớn hơn = 1 thì gọi tới + -
            }
            else
            {
                return View("TotalZezor", ProductId); // nếu số lượng bằng 0 thì trở về dấu cộng
            }

        }
    }
}
