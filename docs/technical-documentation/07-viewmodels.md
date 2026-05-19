# 7. PHÂN TÍCH VIEWMODELS

## 7.1 Tổng Quan

ViewModels trong hệ thống được sử dụng để truyền dữ liệu từ Controller sang View, giúp tách biệt logic nghiệp vụ với logic hiển thị.

---

## 7.2 Danh Sách ViewModels

| ViewModel | File | Mục Đích |
|-----------|------|----------|
| AdminDashboardViewModel | AdminDashboardViewModel.cs | Dashboard admin |
| ViewModelCart | ViewModelCart.cs | Giỏ hàng |
| ViewModelMenu | ViewModelMenu.cs | Menu sản phẩm |
| ViewModelBan | ViewModelBan.cs | Quản lý bàn |
| ViewModelGetFormBuy | ViewModelGetFormBuy.cs | Thanh toán bàn |
| ProductsIndexViewModel | ProductsIndexViewModel.cs | Danh sách sản phẩm |
| CustomersIndexViewModel | CustomersIndexViewModel.cs | Danh sách khách hàng |
| EmployeesIndexViewModel | EmployeesIndexViewModel.cs | Danh sách nhân viên |
| OrdersIndexViewModel | OrdersIndexViewModel.cs | Danh sách đơn hàng |
| TrangChuMenuPageViewModel | TrangChuMenuPageViewModel.cs | Trang menu công khai |
| AdminBaiVietChuyenNhaPageViewModel | AdminBaiVietChuyenNhaPageViewModel.cs | Quản lý bài viết |

---

## 7.3 Chi Tiết ViewModels

### 7.3.1 AdminDashboardViewModel

**File:** `ViewModels/AdminDashboardViewModel.cs`

**Mô tả:** Chứa dữ liệu cho dashboard admin

```csharp
public class AdminDashboardViewModel
{
    public string AdminDisplayName { get; set; }
    public string AdminAccount { get; set; }
    public string AdminRoleLabel { get; set; }
    public string Initials { get; set; }
    public string SnapshotLabel { get; set; }
    
    // Metric cards
    public DashboardMetricCard RevenueMetric { get; set; }
    public DashboardMetricCard OrderMetric { get; set; }
    public DashboardMetricCard CustomerMetric { get; set; }
    public DashboardMetricCard ProductMetric { get; set; }
    
    // Insight cards
    public List<DashboardInsightCard> InsightCards { get; set; }
    
    // Dashboard data (JSON cho charts)
    public string DashboardPayloadJson { get; set; }
}

public class DashboardMetricCard
{
    public string Label { get; set; }
    public string Value { get; set; }
    public string TrendText { get; set; }
    public string ComparisonText { get; set; }
    public string TrendCssClass { get; set; }  // is-up, is-down, is-neutral
    public string HintText { get; set; }
}

public class DashboardInsightCard
{
    public string Label { get; set; }
    public string Value { get; set; }
    public string Note { get; set; }
    public string ToneCssClass { get; set; }  // is-green, is-blue, is-purple, is-orange
}

public class DashboardPayload
{
    public string SnapshotLabel { get; set; }
    public Dictionary<string, DashboardValueSeries> RevenueSeries { get; set; }
    public Dictionary<string, DashboardCountSeries> OrderSeries { get; set; }
    public Dictionary<string, DashboardComparisonSeries> RevenueComparisonSeries { get; set; }
    public Dictionary<string, List<DashboardTopProductItem>> TopProducts { get; set; }
}
```

---

### 7.3.2 ViewModelCart

**File:** `ViewModels/ViewModelCart.cs`

**Mô tả:** Xử lý giỏ hàng (dine-in và online)

```csharp
public class ViewModelCart
{
    private readonly QlnhaHangBtlContext _context;

    public ViewModelCart(QlnhaHangBtlContext context)
    {
        _context = context;
    }

    // Lấy chi tiết hóa đơn theo đơn hàng
    public List<CTDH_Product> CTHD_PctByDh(int? DhId)
    {
        if (!DhId.HasValue)
            return new List<CTDH_Product>();

        var result = from CTHD in _context.ChiTietHoaDons
                     join product in _context.Products on CTHD.ProductId equals product.ProductId
                     where CTHD.DhId == DhId && !CTHD.Remove && !CTHD.Dh.Remove
                     select new CTDH_Product
                     {
                         ProductId = product.ProductId,
                         DhId = DhId,
                         CthdId = CTHD.CthdId,
                         PathPhoto = product.PathPhoto,
                         SoLuong = CTHD.SoLuong,
                         TenSanPham = product.TenSanPham,
                         ThanhTien = CTHD.ThanhTien,
                         Condition = CTHD.Ghichu
                     };

        return result?.ToList() ?? new List<CTDH_Product>();
    }

    // Tính tổng tiền đơn hàng
    public DonHang TongtienById(int? DhId)
    {
        if (!DhId.HasValue)
            return new DonHang { TongTien = 0 };

        var result = _context.DonHangs.FirstOrDefault(item => item.DhId == DhId && !item.Remove);
        
        if (result == null || result.TongTien == null)
            return new DonHang { TongTien = 0 };

        return result;
    }

    // Kiểm tra đơn hàng có món không
    public bool HasOrderItems(int? DhId)
    {
        return DhId.HasValue && _context.ChiTietHoaDons
            .Any(item => item.DhId == DhId.Value && !item.Remove && !item.Dh.Remove);
    }

    // Lấy món đã submit (dine-in)
    public List<CTDH_Product> SubmittedDineInItems(int? banId, string? guestId)
    {
        if (!banId.HasValue || string.IsNullOrWhiteSpace(guestId))
            return new List<CTDH_Product>();

        var customerMarker = $"\"g\":\"{guestId.Trim()}\"";
        
        var result = from CTHD in _context.ChiTietHoaDons
                     join product in _context.Products on CTHD.ProductId equals product.ProductId
                     where CTHD.Dh.BanId == banId.Value
                           && CTHD.Dh.VanChuyen != true
                           && CTHD.Dh.TrangThai == true
                           && !CTHD.Dh.Remove && !CTHD.Remove
                           && CTHD.Dh.GhiChu != null
                           && CTHD.Dh.GhiChu.Contains("\"t\":\"dinein\"")
                           && CTHD.Dh.GhiChu.Contains(customerMarker)
                     orderby CTHD.Dh.GioRa descending, CTHD.Dh.DhId descending, CTHD.CthdId
                     select new CTDH_Product
                     {
                         ProductId = product.ProductId,
                         DhId = CTHD.DhId,
                         CthdId = CTHD.CthdId,
                         PathPhoto = product.PathPhoto,
                         SoLuong = CTHD.SoLuong,
                         TenSanPham = product.TenSanPham,
                         ThanhTien = CTHD.ThanhTien,
                         Condition = CTHD.Ghichu
                     };

        return result.ToList();
    }

    // Xây dựng form thanh toán
    public OnlineCheckoutForm BuildCheckoutForm(int? DhId, int? customerId)
    {
        var order = DhId.HasValue
            ? _context.DonHangs.FirstOrDefault(item => item.DhId == DhId.Value && !item.Remove)
            : null;

        var metadata = order == null ? null : OnlineOrderMetadata.TryParse(order.GhiChu);
        
        var customer = customerId.HasValue
            ? _context.KhachHangs.FirstOrDefault(item => item.KhId == customerId.Value && !item.Remove)
            : null;

        return new OnlineCheckoutForm
        {
            HoTen = metadata?.RecipientName ?? customer?.TenKhachHang,
            SoDienThoai = metadata?.Phone ?? customer?.SoDienThoai,
            TinhThanh = metadata?.City,
            QuanHuyen = metadata?.District,
            PhuongXa = metadata?.Ward,
            DiaChi = metadata?.AddressLine ?? customer?.DiaChi,
            GhiChu = metadata?.Note
        };
    }
}

// Entity hỗ trợ
public class CTDH_Product
{
    public int ProductId { get; set; }
    public int? DhId { get; set; }
    public int? CthdId { get; set; }
    public string PathPhoto { get; set; }
    public int? SoLuong { get; set; }
    public string TenSanPham { get; set; }
    public decimal? ThanhTien { get; set; }
    public string Condition { get; set; }  // Ghi chú trạng thái
}
```

---

### 7.3.3 ViewModelMenu

**File:** `ViewModels/ViewModelMenu.cs`

**Mô tả:** Xử lý menu sản phẩm cho khách hàng

```csharp
public class ViewModelMenu
{
    private readonly QlnhaHangBtlContext _context;

    public ViewModelMenu(QlnhaHangBtlContext context)
    {
        _context = context;
    }

    // Đếm số món trong đơn
    public int CountProductDetail(int? DhId)
    {
        if (!DhId.HasValue) return 0;

        return _context.ChiTietHoaDons
            .Count(p => p.DhId == DhId.Value && !p.Remove && p.Dh != null && !p.Dh.Remove);
    }

    // Đếm số lượng một món cụ thể
    public int CountProductDetail(int? DhId, int ProductId)
    {
        if (!DhId.HasValue) return 0;

        return _context.ChiTietHoaDons
            .Count(p => p.DhId == DhId.Value && p.ProductId == ProductId 
                       && !p.Remove && p.Dh != null && !p.Dh.Remove);
    }

    // Đếm món đã submit của dine-in
    public int CountSubmittedDineInProduct(int? banId, string? guestId, int productId)
    {
        if (!banId.HasValue || string.IsNullOrWhiteSpace(guestId))
            return 0;

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

    // Lấy danh sách danh mục
    public List<Category> Categories()
    {
        return _context.Categories.Where(category => !category.Remove).ToList();
    }

    // Tìm sản phẩm theo danh mục
    public List<CategoryProduct> FindProductByCate(int cateId)
    {
        var result = from cate in _context.Categories
                     join product in _context.Products on cate.CateId equals product.ProductId
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

        return result?.ToList() ?? new List<CategoryProduct>();
    }

    // Tìm kiếm sản phẩm
    public List<CategoryProduct> ProductsBySearch(string? txtsearchName)
    {
        var searchKey = NormalizeSearchText(txtsearchName);
        if (string.IsNullOrWhiteSpace(searchKey))
            return new List<CategoryProduct>();

        var searchWords = searchKey
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToList();

        var products = from cate in _context.Categories
                       join product in _context.Products on cate.CateId equals product.ProductId
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
            return string.Empty;

        var normalized = StringUtils.ConvertToLowerAndRemoveDiacritics(value.Trim());
        return string.Join(' ', normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }
}

public class CategoryProduct
{
    public int CateId { get; set; }
    public int ProductId { get; set; }
    public string TenLoaiSanPham { get; set; }
    public string TenSanPham { get; set; }
    public string PathPhoto { get; set; }
    public decimal? GiaTien { get; set; }
    public string MoTa { get; set; }
}
```

---

### 7.3.4 ProductsIndexViewModel

**File:** `ViewModels/ProductsIndexViewModel.cs`

```csharp
public class ProductsIndexViewModel
{
    public List<Product> Products { get; set; }
    public Product CreateProduct { get; set; }
    public Product EditProduct { get; set; }
    public SelectList CategoryOptions { get; set; }
    public bool OpenCreateModal { get; set; }
    public int? OpenEditProductId { get; set; }
}
```

---

### 7.3.5 OrdersIndexViewModel

**File:** `ViewModels/OrdersIndexViewModel.cs`

```csharp
public class OrdersIndexViewModel
{
    public string AdminDisplayName { get; set; }
    public string AdminAccount { get; set; }
    public string AdminRoleLabel { get; set; }
    public string Initials { get; set; }
    public List<OrderIndexRowViewModel> Orders { get; set; }
}

public class OrderIndexRowViewModel
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; }  // DH001, DH002...
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }
    public string CustomerAddress { get; set; }
    public string TableLabel { get; set; }  // "Bàn 5" hoặc "Giao hàng"
    public DateTime? OrderTime { get; set; }
    public string DateValue { get; set; }  // "2026-05-19" cho filtering
    public decimal TotalAmount { get; set; }
    public string PaymentLabel { get; set; }  // "COD", "Tiền mặt"...
    public string StatusKey { get; set; }  // pending, preparing, shipping, delivered, cancelled
    public string StatusLabel { get; set; }  // "Chờ xác nhận", "Đang giao"...
    public string StatusCssClass { get; set; }  // is-pending, is-processing, is-completed, is-cancelled
    public string SearchText { get; set; }  // Cho search client-side
}
```

---

### 7.3.6 AdminBaiVietChuyenNhaPageViewModel

**File:** `ViewModels/AdminBaiVietChuyenNhaPageViewModel.cs`

```csharp
public class AdminBaiVietChuyenNhaPageViewModel
{
    public BaiVietChuyenNha Form { get; set; }
    public List<BaiVietChuyenNha> Posts { get; set; }
}
```

---

### 7.3.7 TrangChuMenuPageViewModel

**File:** `ViewModels/TrangChuMenuPageViewModel.cs`

```csharp
public class TrangChuMenuPageViewModel
{
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public List<Category> Categories { get; set; }
    public Dictionary<int, IReadOnlyList<Product>> ProductsByCategory { get; set; }
}
```

---

### 7.3.8 ViewModelBan

**File:** `ViewModels/ViewModelBan.cs`

```csharp
public class ViewModelBan
{
    private readonly QlnhaHangBtlContext _context;
    
    public string AdminDisplayName { get; set; }
    public string AdminAccount { get; set; }
    public string AdminRoleLabel { get; set; }
    public string Initials { get; set; }

    public ViewModelBan(QlnhaHangBtlContext context)
    {
        _context = context;
    }

    // Lấy danh sách bàn
    public List<Ban> GetBans()
    {
        return _context.Bans.Where(b => !b.Remove).ToList();
    }

    // Lấy đơn hàng đang active của bàn
    public DonHang GetActiveOrderForTable(int banId)
    {
        return _context.DonHangs
            .Include(d => d.ChiTietHoaDons)
                .ThenInclude(c => c.Product)
            .FirstOrDefault(d =>
                d.BanId == banId &&
                !d.Remove &&
                d.VanChuyen != true &&
                d.TrangThai != true);
    }

    // Lấy danh sách yêu cầu dịch vụ chưa xử lý
    public List<ServiceRequest> GetPendingServiceRequests(int banId)
    {
        // Implement theo logic yêu cầu dịch vụ
        return new List<ServiceRequest>();
    }
}

public class ServiceRequest
{
    public int Id { get; set; }
    public int BanId { get; set; }
    public string CustomerName { get; set; }
    public string RequestType { get; set; }  // "staff", "payment", "review"
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
}
```

---

## 7.4 OnlineCheckoutForm

**File:** `ViewModels/OnlineCheckoutForm.cs`

```csharp
public class OnlineCheckoutForm
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
    public string HoTen { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^\d{9,11}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string SoDienThoai { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn tỉnh/thành")]
    public string TinhThanh { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn quận/huyện")]
    public string QuanHuyen { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn phường/xã")]
    public string PhuongXa { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
    [StringLength(300, ErrorMessage = "Địa chỉ không quá 300 ký tự")]
    public string DiaChi { get; set; }

    [StringLength(300, ErrorMessage = "Ghi chú không quá 300 ký tự")]
    public string GhiChu { get; set; }
}
```

---

## 7.5 Pattern: ViewModel Construction

### Constructor Injection

```csharp
// Pattern tốt: Inject DbContext qua constructor
public class ViewModelCart
{
    private readonly QlnhaHangBtlContext _context;

    public ViewModelCart(QlnhaHangBtlContext context)
    {
        _context = context;
    }
}

// Sử dụng trong Controller
public class HomeController : Controller
{
    private readonly QlnhaHangBtlContext _context;

    public HomeController(QlnhaHangBtlContext context)
    {
        _context = context;
    }

    public IActionResult Cart(int DhId)
    {
        ViewModelCart viewModel = new ViewModelCart(_context);
        return View(viewModel);
    }
}
```

---

*Tài liệu tiếp tục ở phần 8...*