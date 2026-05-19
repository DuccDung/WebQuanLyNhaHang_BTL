# 6. BÀI HỌC KINH NGHIỆM VÀ ĐỀ XUẤT CẢI THIỆN

## 6.1 Tổng Quan

Phần này tổng hợp các bài học kinh nghiệm từ việc phân tích source code và đưa ra các đề xuất cải thiện cho hệ thống.

---

## 6.2 Điểm Mạnh

### 6.2.1 Kiến Trúc

| Điểm Mạnh | Mô Tả |
|-----------|-------|
| **MVC Pattern** | Phân tách rõ ràng giữa Model, View, Controller |
| **Dependency Injection** | Sử dụng DI qua constructor injection |
| **Entity Framework Core** | ORM hiện đại, dễ bảo trì |
| **SignalR Integration** | Real-time features được tích hợp tốt |
| **Soft Delete Pattern** | Dễ khôi phục dữ liệu, audit trail |

### 6.2.2 Code Quality

| Điểm Mạnh | Mô Tả |
|-----------|-------|
| **Separation of Concerns** | ViewModels tách biệt logic hiển thị |
| **Extension Methods** | HttpContextExtensions dễ tái sử dụng |
| **ViewComponents** | Component tái sử dụng cho UI |
| **Attribute-based Auth** | Phân quyền rõ ràng qua attributes |

### 6.2.3 Security

| Điểm Mạnh | Mô Tả |
|-----------|-------|
| **Session Management** | Session-based auth với timeout |
| **Cookie Protection** | Data Protection API cho cookies |
| **Role-based Auth** | Phân quyền theo role |
| **Anti-forgery Tokens** | `[ValidateAntiForgeryToken]` trên POST |

---

## 6.3 Điểm Yếu và Đề Xuất Cải Thiện

### 6.3.1 Password Storage

**Vấn đề:**
```csharp
// Password được lưu dưới dạng plain text
var nhanVien = _context.NhanViens
    .FirstOrDefault(e => e.MatKhau == password);
```

**Đề xuất:**
```csharp
// Sử dụng password hashing
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

public string HashPassword(string password)
{
    return Convert.ToBase64String(
        KeyDerivation.Pbkdf2(
            password: password,
            salt: Encoding.UTF8.GetBytes("your-salt"),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
}

public bool VerifyPassword(string password, string hashedPassword)
{
    var hash = HashPassword(password);
    return hash == hashedPassword;
}
```

---

### 6.3.2 SQL Injection Risk

**Vấn đề:**
```csharp
// Có thể bị SQL injection nếu không cẩn thận
var sql = $"SELECT * FROM Product WHERE Name = '{productName}'";
```

**Đề xuất:**
```csharp
// Luôn sử dụng parameterized queries
var product = await _context.Products
    .FirstOrDefaultAsync(p => p.TenSanPham == productName);
```

---

### 6.3.3 Xóa Mềm Không Đầy Đủ

**Vấn đề:**
```csharp
// Một số query không lọc Remove = false
var products = await _context.Products.ToListAsync();
// Có thể trả về sản phẩm đã xóa
```

**Đề xuất:**
```csharp
// Tạo extension method để tự động lọc
public static class IQueryableExtensions
{
    public static IQueryable<T> WithoutSoftDeleted<T>(this IQueryable<T> query)
        where T : class, ISoftDelete
    {
        return query.Where(e => !((ISoftDelete)e).Remove);
    }
}

// Sử dụng
var products = await _context.Products
    .WithoutSoftDeleted()
    .ToListAsync();
```

---

### 6.3.4 Magic Strings

**Vấn đề:**
```csharp
// Magic strings dễ gây lỗi
session.SetString("NhanVienId", ...);
session.SetString("NhanVienName", ...);
session.SetString("CustomerID", ...);
```

**Đề xuất:**
```csharp
// Tạo class chứa session keys
public static class SessionKeys
{
    public const string AdminId = "NhanVienId";
    public const string AdminName = "NhanVienName";
    public const string AdminTaiKhoan = "NhanVienTaiKhoan";
    public const string RoleKey = "RoleKey";
    public const string CustomerId = "CustomerID";
    public const string BanId = "BanId";
    public const string DineInCustomerId = "DineInCustomerId";
}

// Sử dụng
HttpContext.Session.SetInt32(SessionKeys.AdminId, adminId);
```

---

### 6.3.5 JSON Metadata Storage

**Vấn đề:**
```csharp
// Lưu metadata JSON trong GhiChu field
DH.GhiChu = metadata.ToJson();

// Parse JSON có thể fail
var metadata = OnlineOrderMetadata.TryParse(DH.GhiChu);
```

**Đề xuất:**
```csharp
// Sử dụng bảng riêng cho order metadata
public class OrderMetadata
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Hoặc sử dụng JSON type (SQL Server 2016+)
public class Order
{
    public int DhId { get; set; }
    [Column(TypeName = "json")]
    public OrderMetadataJson Metadata { get; set; }
}
```

---

### 6.3.6 Error Handling

**Vấn đề:**
```csharp
// Thiếu error handling chi tiết
try
{
    _context.SaveChanges();
}
catch (DbUpdateConcurrencyException)
{
    // Không xử lý cụ thể
    throw;
}
```

**Đề xuất:**
```csharp
// Global exception handler
public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new {
            success = false,
            message = "Có lỗi xảy ra, vui lòng thử lại.",
            statusCode = context.Response.StatusCode
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
```

---

### 6.3.7 Validation

**Vấn đề:**
```csharp
// Validation scattered across controllers
if (string.IsNullOrWhiteSpace(form.HoTen))
{
    errors.Add("Vui lòng nhập họ tên.");
}
```

**Đề xuất:**
```csharp
// Sử dụng FluentValidation
public class OrderValidator : AbstractValidator<OnlineCheckoutForm>
{
    public OrderValidator()
    {
        RuleFor(x => x.HoTen)
            .NotEmpty().WithMessage("Vui lòng nhập họ tên")
            .MinimumLength(2).WithMessage("Họ tên phải ít nhất 2 ký tự");

        RuleFor(x => x.SoDienThoai)
            .NotEmpty().WithMessage("Vui lòng nhập số điện thoại")
            .Matches(@"^\d{9,11}$").WithMessage("Số điện thoại không hợp lệ");

        RuleFor(x => x.TinhThanh)
            .NotEmpty().WithMessage("Vui lòng chọn tỉnh/thành");

        RuleFor(x => x.QuanHuyen)
            .NotEmpty().WithMessage("Vui lòng chọn quận/huyện");

        RuleFor(x => x.DiaChi)
            .NotEmpty().WithMessage("Vui lòng nhập địa chỉ")
            .MinimumLength(10).WithMessage("Địa chỉ quá ngắn");
    }
}

// Sử dụng
public class CheckoutController : Controller
{
    private readonly IValidator<OnlineCheckoutForm> _validator;

    public CheckoutController(IValidator<OnlineCheckoutForm> validator)
    {
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(OnlineCheckoutForm form)
    {
        var result = await _validator.ValidateAsync(form);
        
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return View(form);
        }

        // Process checkout
    }
}
```

---

### 6.3.8 Caching

**Vấn đề:**
```csharp
// Query database mỗi lần
var categories = await _context.Categories
    .Where(c => !c.Remove)
    .ToListAsync();
```

**Đề xuất:**
```csharp
// Sử dụng distributed caching
public class CategoryService
{
    private readonly QlnhaHangBtlContext _context;
    private readonly IDistributedCache _cache;

    public CategoryService(QlnhaHangBtlContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        var cacheKey = "categories:active";
        
        var cached = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<List<Category>>(cached);
        }

        var categories = await _context.Categories
            .Where(c => !c.Remove)
            .ToListAsync();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(categories),
            options);

        return categories;
    }
}
```

---

### 6.3.9 Logging

**Vấn đề:**
```csharp
// Thiếu logging
Console.WriteLine("Phát sự kiện OderSuccess");
```

**Đề xuất:**
```csharp
// Sử dụng logging framework
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CheckoutOnlineOrder(OnlineCheckoutForm form)
    {
        _logger.LogInformation("Customer {CustomerId} starting checkout", customerId);

        try
        {
            // Process order
            _logger.LogInformation("Order {OrderId} created successfully", dhId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order for customer {CustomerId}", customerId);
            throw;
        }
    }
}
```

---

### 6.3.10 Database Indexing

**Đề xuất thêm indexes:**

```sql
-- Index cho soft delete queries
CREATE INDEX IX_Product_Remove ON Product(Remove) WHERE Remove = 0;
CREATE INDEX IX_DonHang_Remove ON DonHang(Remove) WHERE Remove = 0;
CREATE INDEX IX_ChiTietHoaDon_Remove ON ChiTietHoaDon(Remove) WHERE Remove = 0;

-- Index cho tìm kiếm
CREATE INDEX IX_Product_TenSanPham ON Product(TenSanPham);
CREATE INDEX IX_KhachHang_TaiKhoan ON KhachHang(TaiKhoan);
CREATE INDEX IX_NhanVien_TaiKhoan ON NhanVien(TaiKhoan);

-- Index cho date queries
CREATE INDEX IX_DonHang_GioRa ON DonHang(GioRa) WHERE TrangThai = 1;
CREATE INDEX IX_DonHang_VanChuyen ON DonHang(VanChuyen, TrangThai) WHERE Remove = 0;

-- Index cho foreign keys
CREATE INDEX IX_ChiTietHoaDon_DhId ON ChiTietHoaDon(DhId);
CREATE INDEX IX_ChiTietHoaDon_ProductId ON ChiTietHoaDon(ProductId);
CREATE INDEX IX_OnlineOrderInfo_DH_ID ON OnlineOrderInfo(DH_ID);
```

---

### 6.3.11 API Versioning

**Đề xuất:**
```csharp
// Cấu hình API versioning
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteGroupConvention("api/v{version}"));
})
.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-API-Version"));
});

// Controller với version
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    // ...
}
```

---

### 6.3.12 Unit Testing

**Đề xuất:**
```csharp
// Tạo test project
// WebQuanLyNhaHang.Tests/

public class ProductServiceTests
{
    private readonly Mock<QlnhaHangBtlContext> _mockContext;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockContext = new Mock<QlnhaHangBtlContext>();
        _service = new ProductService(_mockContext.Object);
    }

    [Fact]
    public async Task GetProductById_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var mockProduct = new Product { ProductId = productId, TenSanPham = "Test" };
        
        _mockContext.Setup(c => c.Products.FindAsync(productId))
            .ReturnsAsync(mockProduct);

        // Act
        var result = await _service.GetProductById(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.TenSanPham);
    }

    [Fact]
    public async Task GetProductById_NotFound_ReturnsNull()
    {
        // Arrange
        var productId = 999;
        
        _mockContext.Setup(c => c.Products.FindAsync(productId))
            .ReturnsAsync((Product)null);

        // Act
        var result = await _service.GetProductById(productId);

        // Assert
        Assert.Null(result);
    }
}
```

---

## 6.4 Tổng Kết

### 6.4.1 Ưu Tiên Cải Thiện

| Ưu Tiên | Vấn Đề | Mức Độ Khó |
|---------|--------|------------|
| 1 | Password hashing | Dễ |
| 2 | Global error handling | Dễ |
| 3 | Logging implementation | Dễ |
| 4 | Validation với FluentValidation | Trung bình |
| 5 | Caching cho data tĩnh | Trung bình |
| 6 | API versioning | Khó |
| 7 | Unit testing | Khó |

### 6.4.2 Checklist Bảo Mật

- [ ] Implement password hashing
- [ ] Add rate limiting cho API endpoints
- [ ] Implement HTTPS redirection
- [ ] Add Content Security Policy headers
- [ ] Implement CORS properly
- [ ] Add SQL injection protection (parameterized queries)
- [ ] Add XSS protection (output encoding)
- [ ] Implement proper session timeout
- [ ] Add audit logging cho admin actions
- [ ] Secure API endpoints với authentication

---

*Tài liệu kết thúc*