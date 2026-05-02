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
            if (!DhId.HasValue)
            {
                return 0;
            }

            // Sử dụng DhId từ donHang để tìm trong ChiTietHoaDons
            var slchiTietHoaDon = _context.ChiTietHoaDons
                .Count(p => p.DhId == DhId.Value && !p.Remove && p.Dh != null && !p.Dh.Remove);

            // Kiểm tra nếu chi tiết hóa đơn tồn tại
            return slchiTietHoaDon; //
        }
        public int CountProductDetail(int? DhId, int ProductId) // chức năng lọc productdetail này để phục vụ tăng giảm số lượng trang menu
        {
            if (!DhId.HasValue)
            {
                return 0;
            }

            // Sử dụng DhId từ donHang để tìm trong ChiTietHoaDons
            var slchiTietHoaDon = _context.ChiTietHoaDons
                .Count(p => p.DhId == DhId.Value && p.ProductId == ProductId && !p.Remove && p.Dh != null && !p.Dh.Remove);

            // Kiểm tra nếu chi tiết hóa đơn tồn tại
            return slchiTietHoaDon; //
        }

        public int CountSubmittedDineInProduct(int? banId, string? guestId, int productId)
        {
            if (!banId.HasValue || string.IsNullOrWhiteSpace(guestId))
            {
                return 0;
            }

            var customerMarker = $"\"g\":\"{guestId.Trim()}\"";
            return _context.ChiTietHoaDons
                .Where(item =>
                    item.ProductId == productId &&
                    !item.Remove &&
                    !item.Dh.Remove &&
                    item.Dh.BanId == banId.Value &&
                    item.Dh.VanChuyen != true &&
                    item.Dh.TrangThai == true &&
                    item.Dh.GhiChu != null &&
                    item.Dh.GhiChu.Contains("\"t\":\"dinein\"") &&
                    item.Dh.GhiChu.Contains(customerMarker))
                .Sum(item => item.SoLuong ?? 0);
        }

            // Liệt kê ra category trong header menu
            public List<Category> Categories()
        {
            var result = _context.Categories.Where(category => !category.Remove).ToList();
            return result;
        }

        // Đón CateID và lọc ra sản phẩm in category
        public List<CategoryProduct> FindProductByCate(int cateId)
        {
           
            // từ id cate ta tìm ra sản phẩm thuộc cate đó
            var result = from cate in _context.Categories
                         join product in _context.Products
                         on cate.CateId equals product.CateId
                         where product.CateId == cateId && !product.Remove && !cate.Remove
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
        public List<CategoryProduct> ProductsBySearch(string? txtsearchName) // phục vụ chức năng tìm kiếm ở trang menu
        {
            var searchKey = NormalizeSearchText(txtsearchName);
            if (string.IsNullOrWhiteSpace(searchKey))
            {
                return new List<CategoryProduct>();
            }

            var searchWords = searchKey
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToList();

            var products = from cate in _context.Categories
                           join product in _context.Products on cate.CateId equals product.CateId
                           where !product.Remove && !cate.Remove
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

            return products
                .AsEnumerable()
                .Select(product => new
                {
                    Product = product,
                    Name = NormalizeSearchText(product.TenSanPham),
                    Category = NormalizeSearchText(product.TenLoaiSanPham),
                    Description = NormalizeSearchText(product.MoTa),
                    Price = NormalizeSearchText(product.GiaTien?.ToString("0"))
                })
                .Select(item => new
                {
                    item.Product,
                    SearchText = $"{item.Name} {item.Category} {item.Description} {item.Price}",
                    Score =
                        (item.Name == searchKey ? 100 : 0) +
                        (item.Name.StartsWith(searchKey) ? 70 : 0) +
                        (item.Name.Contains(searchKey) ? 50 : 0) +
                        (item.Category.Contains(searchKey) ? 25 : 0) +
                        (item.Description.Contains(searchKey) ? 12 : 0) +
                        (item.Price.Contains(searchKey) ? 10 : 0)
                })
                .Where(item =>
                    searchWords.All(word => item.SearchText.Contains(word)) ||
                    item.Score > 0)
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Product.TenSanPham)
                .Select(item => item.Product)
                .ToList();
        }

        private static string NormalizeSearchText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = StringUtils.ConvertToLowerAndRemoveDiacritics(value.Trim());
            return string.Join(' ', normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }
    }
}
