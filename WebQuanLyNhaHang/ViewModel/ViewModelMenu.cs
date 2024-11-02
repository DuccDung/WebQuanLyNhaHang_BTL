using Microsoft.AspNetCore.Http.HttpResults;
using WebQuanLyNhaHang.Models;
namespace WebQuanLyNhaHang.ViewModel
{
    public class ViewModelMenu
    {
        private readonly QlnhaHangBtlContext _context;
        public ViewModelMenu(QlnhaHangBtlContext context)
        {
            _context = context;
        }
        
        public int CountProductDetail(int? DhId) // đếm số lượng món trong đơn hàng
        {
            // Sử dụng DhId từ donHang để tìm trong ChiTietHoaDons
            var slchiTietHoaDon = _context.ChiTietHoaDons
                .Count(p => p.DhId == DhId);

            // Kiểm tra nếu chi tiết hóa đơn tồn tại
            return slchiTietHoaDon; //
        }
        public int CountProductDetail(int? DhId, int ProductId) // chức năng lọc productdetail này để phục vụ tăng giảm số lượng trang menu
        {
            // Sử dụng DhId từ donHang để tìm trong ChiTietHoaDons
            var slchiTietHoaDon = _context.ChiTietHoaDons
                .Count(p => p.DhId == DhId && p.ProductId == ProductId);

            // Kiểm tra nếu chi tiết hóa đơn tồn tại
            return slchiTietHoaDon; //
        }

            // Liệt kê ra category trong header menu
            public List<Category> Categories()
        {
            var result = _context.Categories.ToList();
            return result;
        }

        // Đón CateID và lọc ra sản phẩm in category
        public List<CategoryProduct> FindProductByCate(int cateId)
        {
           
            // từ id cate ta tìm ra sản phẩm thuộc cate đó
            var result = from cate in _context.Categories
                         join product in _context.Products
                         on cate.CateId equals product.CateId
                         where product.CateId == cateId
                         select new CategoryProduct
                         {
                             CateId = cate.CateId,
                             ProductId = product.ProductId,
                             TenLoaiSanPham = cate.TenLoaiSanPham,
                             TenSanPham = product.TenSanPham,
                             PathPhoto = product.PathPhoto,
                             GiaTien = product.GiaTien,
                             MoTa = product.MoTa
                         };
            if (result == null)
            {
                throw new Exception($"No Products found with Id {cateId}");
            }

            return result.ToList();
        }
        public List<CategoryProduct> ProductsBySearch(string txtsearchName) // phục vụ chức năng tìm kiếm ở trang menu
        {
            string searchKey = StringUtils.ConvertToLowerAndRemoveDiacritics(txtsearchName);
            // Truy vấn product
            var result = _context.Products
             .AsEnumerable() // Chuyển sang LINQ to Objects để dùng hàm tùy chỉnh
             .Where(x =>
                StringUtils.ConvertToLowerAndRemoveDiacritics(x.TenSanPham)
                .Contains(searchKey))
             .Select(x => new CategoryProduct
             {
                 ProductId = x.ProductId,
                 TenSanPham = x.TenSanPham,
                 PathPhoto = x.PathPhoto,
                 MoTa = x.MoTa,
                 GiaTien = x.GiaTien
             });

            // Trả về danh sách kết quả
            return result.ToList();
        }
    }
}
