using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    public class TrangChuController : Controller
    {
        private readonly QlnhaHangBtlContext _qlnhaHangBtlContext;
        public TrangChuController(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }
        public IActionResult Index()
        {
            var hotCategoryNames = new[] { "BEST MENU", "HOT DRINK", "SIGNATURE" };
            var hotProducts = _qlnhaHangBtlContext.Products
                .Include(product => product.Cate)
                .Include(product => product.ProductConditions)
                .Where(product =>
                    !product.Remove &&
                    product.Cate != null &&
                    !product.Cate.Remove &&
                    product.Cate.TenLoaiSanPham != null &&
                    hotCategoryNames.Contains(product.Cate.TenLoaiSanPham))
                .OrderBy(product => product.Cate!.TenLoaiSanPham == "BEST MENU"
                    ? 0
                    : product.Cate.TenLoaiSanPham == "HOT DRINK"
                        ? 1
                        : 2)
                .ThenBy(product => product.ProductId)
                .Take(4)
                .ToList();

            if (hotProducts.Count < 4)
            {
                var selectedIds = hotProducts.Select(product => product.ProductId).ToList();
                var fallbackProducts = _qlnhaHangBtlContext.Products
                    .Include(product => product.ProductConditions)
                    .Where(product => !product.Remove && !selectedIds.Contains(product.ProductId))
                    .OrderBy(product => product.ProductId)
                    .Take(4 - hotProducts.Count)
                    .ToList();

                hotProducts.AddRange(fallbackProducts);
            }

            return View(hotProducts);
        }
        public IActionResult Menu()
        {
            ViewData["ActiveNav"] = "Menu";
            return View(BuildMenuPage(
                "Thực đơn Cloudy Café",
                "Khám phá menu đa dạng với đồ uống và bánh ngon tại Cloudy Café"));
        }

        public IActionResult DoUong()
        {
            ViewData["ActiveNav"] = "DoUong";
            return View("Menu", BuildMenuPage(
                "Đồ uống Cloudy Café",
                "Những món uống đang có trong hệ thống, lấy trực tiếp từ dữ liệu cũ.",
                "APPETIZERS", "TEA", "JUICE", "HOT DRINK", "SIGNATURE"));
        }

        public IActionResult Banh()
        {
            ViewData["ActiveNav"] = "Banh";
            return View("Menu", BuildMenuPage(
                "Bánh Cloudy Café",
                "Các món bánh hiện có trong hệ thống Cloudy Café.",
                "CAKE"));
        }

        public IActionResult ChuyenNha()
        {
            ViewData["ActiveNav"] = "ChuyenNha";
            return View();
        }

        public IActionResult CuaHang()
        {
            ViewData["ActiveNav"] = "CuaHang";
            return View();
        }

        private TrangChuMenuPageViewModel BuildMenuPage(string title, string subtitle, params string[] categoryNames)
        {
            var categoriesQuery = _qlnhaHangBtlContext.Categories
                .Where(category => !category.Remove);

            if (categoryNames.Length > 0)
            {
                categoriesQuery = categoriesQuery.Where(category =>
                    category.TenLoaiSanPham != null &&
                    categoryNames.Contains(category.TenLoaiSanPham));
            }

            var categories = categoriesQuery
                .OrderBy(category => category.CateId)
                .ToList();

            var categoryIds = categories.Select(category => category.CateId).ToList();
            var products = _qlnhaHangBtlContext.Products
                .Include(product => product.ProductConditions)
                .Where(product =>
                    !product.Remove &&
                    product.CateId.HasValue &&
                    categoryIds.Contains(product.CateId.Value))
                .OrderBy(product => product.ProductId)
                .ToList();

            var productsByCategory = categories.ToDictionary(
                category => category.CateId,
                category => (IReadOnlyList<Product>)products
                    .Where(product => product.CateId == category.CateId)
                    .ToList());

            var categoriesWithProducts = categories
                .Where(category => productsByCategory[category.CateId].Count > 0)
                .ToList();

            return new TrangChuMenuPageViewModel
            {
                Title = title,
                Subtitle = subtitle,
                Categories = categoriesWithProducts,
                ProductsByCategory = productsByCategory
            };
        }
    }
}
