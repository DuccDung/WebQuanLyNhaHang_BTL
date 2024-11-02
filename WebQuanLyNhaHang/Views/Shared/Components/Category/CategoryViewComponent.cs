using Microsoft.AspNetCore.Mvc;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.Views.Shared.Components.Category
{
    public class CategoryViewComponent : ViewComponent
    {
        QlnhaHangBtlContext _qlnhaHangBtlContext;
        public CategoryViewComponent(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }
        public async Task<IViewComponentResult> InvokeAsync() // liệt kê ra category
        {
            var categorys = _qlnhaHangBtlContext.Categories.ToList();
            return View("RenderCategory" , categorys);
        }
    }
}
