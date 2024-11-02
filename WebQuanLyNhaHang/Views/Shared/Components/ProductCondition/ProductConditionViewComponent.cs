using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Views.Shared.Components.ProductConditioin
{
    public class ProductConditionViewComponent:ViewComponent
    {
        QlnhaHangBtlContext _qlnhaHangBtlContext;
        public ProductConditionViewComponent(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }
        public async Task<IViewComponentResult> InvokeAsync(int ProductID) // liệt kê ra category
        {
            ViewModelProductDetail viewModelProductDetail = new ViewModelProductDetail(_qlnhaHangBtlContext);
            var productCondition = viewModelProductDetail.FindProductConditionDetaiById(ProductID);
            return View("CheckboxCondition" , productCondition);
        }
    }
}
