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
        public Task<IViewComponentResult> InvokeAsync(int ProductId, int? DhId)
        {
            if (!DhId.HasValue)
            {
                return Task.FromResult<IViewComponentResult>(View("TotalZezor", ProductId));
            }

            ViewModelMenu viewModelMenu = new ViewModelMenu(_qlnhaHangBtlContext);

            if (viewModelMenu.CountProductDetail(DhId.Value, ProductId) != 0)
            {
                var CTHD = _qlnhaHangBtlContext.ChiTietHoaDons
                    .Where(e => e.ProductId == ProductId && e.DhId == DhId.Value && !e.Remove)
                    .FirstOrDefault();
                if (CTHD == null)
                {
                    return Task.FromResult<IViewComponentResult>(View("TotalZezor", ProductId));
                }

                int? soluong = CTHD.SoLuong;
                var model = new QuantitySelector
            {
                Soluong = soluong,
                ProductId = ProductId,
                DhId = DhId.Value
            };

                return Task.FromResult<IViewComponentResult>(View("QuantitySelector", model));  // số lượng lớn hơn = 1 thì gọi tới + -
            }
            else
            {
                return Task.FromResult<IViewComponentResult>(View("TotalZezor", ProductId)); // nếu số lượng bằng 0 thì trở về dấu cộng
            }

        }
    }
}
