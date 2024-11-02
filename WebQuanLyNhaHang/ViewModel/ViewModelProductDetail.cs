using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public class ViewModelProductDetail
    {
        private readonly QlnhaHangBtlContext _qlnhaHangBtlContext;
        public ViewModelProductDetail(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }
        public Product FindProductDetaiById(int ProductId) {   // Nhận Giá trị ProductID 
            var result = _qlnhaHangBtlContext.Products.Find(ProductId);
            if (result == null) {
                throw new Exception($"No Products found with Id {ProductId}");
            }
            return result;
        }

        public List<ProductConditions> FindProductConditionDetaiById(int ProductId)
        {   // Nhận Giá trị ProductID 
            var result = _qlnhaHangBtlContext.ProductConditions.Where(l => l.ProductId == ProductId);
            if (result == null)
            {
                throw new Exception($"No Products found with Id {ProductId}");
            }
            return result.ToList();
        }

    }
}
