# 4. PHÂN TÍCH VÀ NGUYÊN LÝ HOẠT ĐỘNG

## 4.1 Authentication và Session Management

### 4.1.1 Tổng Quan

Hệ thống sử dụng **Session-based Authentication** kết hợp với **Cookie-based Remember Me** để duy trì phiên đăng nhập của người dùng.

### 4.1.2 Session cho Admin

**Session Keys:**
| Key | Type | Mô Tả |
|-----|------|-------|
| `NhanVienId` | int | ID nhân viên |
| `NhanVienName` | string | Tên nhân viên |
| `NhanVienTaiKhoan` | string | Tài khoản đăng nhập |
| `RoleKey` | string | Role (admin/manager/staff) |

**Flow đăng nhập Admin:**

```
1. User nhập username/password → POST /Admin/Login
   ↓
2. System kiểm tra trong bảng NhanVien
   ↓
3. Nếu hợp lệ → Set session variables
   ↓
4. Redirect về /Admin/Index
```

**Code đăng nhập:**

```csharp
[HttpPost]
public async Task<IActionResult> Login(string? name, string? password)
{
    var nhanVien = _context.NhanViens
        .FirstOrDefault(e => !e.Remove && e.TaiKhoan == name && e.MatKhau == password);

    if (nhanVien == null)
    {
        ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng.";
        return View();
    }

    // Set session
    HttpContext.Session.SetInt32("NhanVienId", nhanVien.NvId);
    HttpContext.Session.SetString("NhanVienName", nhanVien.TenNhanVien);
    HttpContext.Session.SetString("NhanVienTaiKhoan", nhanVien.TaiKhoan);

    // Xác định role từ bảng NvPq → PhanQuyen
    var role = await _context.NvPqs
        .Include(nvpq => nvpq.Pq)
        .FirstOrDefaultAsync(nvpq => nvpq.NvId == nhanVien.NvId && !nvpq.Remove);

    var roleKey = role?.Pq?.TenQuyen?.ToLowerInvariant() switch
    {
        "admin" or "quản lý" or "quan ly" => "admin",
        "manager" or "quản lý" => "manager",
        _ => "staff"
    };

    HttpContext.Session.SetString("RoleKey", roleKey);
    return RedirectToAction(nameof(Index));
}
```

**Logout:**

```csharp
private void ClearAdminSession()
{
    HttpContext.Session.Remove("NhanVienId");
    HttpContext.Session.Remove("NhanVienName");
    HttpContext.Session.Remove("NhanVienTaiKhoan");
}
```

### 4.1.3 Session cho Khách Hàng (Customer)

**Session Keys:**
| Key | Type | Mô Tả |
|-----|------|-------|
| `CustomerID` | int | ID khách hàng |
| `OnlineCartDhId` | int | ID đơn hàng online đang active |
| `DhId` | int | ID đơn hàng dine-in (legacy) |
| `BanId` | int | ID bàn ăn |
| `DineInCustomerId` | string | Guest ID cho dine-in |
| `DineInCustomerName` | string | Tên khách dine-in |

**Cookie-based Remember Me:**

Khách hàng có cookie được bảo vệ bằng `IDataProtectionProvider`:

```csharp
const string customerCookieName = "CloudyCafeCustomer";
const string customerCookiePurpose = "CloudyCafe.CustomerCookie.v1";

// Khi đăng nhập
Response.Cookies.Append(
    customerCookieName,
    _customerCookieProtector.Protect(customerId.ToString()),
    new CookieOptions
    {
        Expires = DateTimeOffset.Now.AddDays(30),
        HttpOnly = true,
        IsEssential = true,
        SameSite = SameSiteMode.Lax,
        Secure = Request.IsHttps
    });

// Khi request đến, middleware tự động restore session từ cookie
```

**Middleware restore session:**

```csharp
app.Use(async (context, next) =>
{
    const string customerSessionKey = "CustomerID";
    const string customerCookieName = "CloudyCafeCustomer";
    const string customerCookiePurpose = "CloudyCafe.CustomerCookie.v1";

    // Nếu session chưa có CustomerID nhưng có cookie
    if (!context.Session.GetInt32(customerSessionKey).HasValue &&
        context.Request.Cookies.TryGetValue(customerCookieName, out var protectedCustomerId))
    {
        var protector = context.RequestServices
            .GetRequiredService<IDataProtectionProvider>()
            .CreateProtector(customerCookiePurpose);

        try
        {
            var customerIdText = protector.Unprotect(protectedCustomerId);
            if (int.TryParse(customerIdText, out var customerId))
            {
                // Kiểm tra customer còn tồn tại
                var customerExists = await db.KhachHangs
                    .AnyAsync(c => c.KhId == customerId && !c.Remove);

                if (customerExists)
                {
                    context.Session.SetInt32(customerSessionKey, customerId);
                }
                else
                {
                    context.Response.Cookies.Delete(customerCookieName);
                }
            }
        }
        catch
        {
            context.Response.Cookies.Delete(customerCookieName);
        }
    }
    await next();
});
```

---

### 4.1.4 Dine-in Guest Session

Đối với khách tại quán (không đăng nhập), hệ thống sử dụng **cookie-based guest session**:

**Cookie Keys:**
| Key | Mô Tả |
|-----|-------|
| `CloudyCafeDineInCustomer` | Guest ID + Name (encrypted) |

**Flow:**

```
1. Khách đến bàn, quét QR → GET /Home/Client/BanId
   ↓
2. Nhập tên → POST /Home/CustomerInfo
   ↓
3. Tạo guest identity (random GUID)
   ↓
4. Set cookie + session
   ↓
5. GetOrCreateDineInOrder() → Tạo đơn hàng cho guest
```

**Guest Identity:**

```csharp
private sealed record DineInCustomerIdentity(string Id, string Name);

private DineInCustomerIdentity CreateDineInCustomer(string customerName)
{
    return new DineInCustomerIdentity(
        Guid.NewGuid().ToString("N"),  // Random ID
        CleanDineInCustomerName(customerName));
}

private void SetDineInCustomer(DineInCustomerIdentity customer)
{
    HttpContext.Session.SetString("DineInCustomerId", customer.Id);
    HttpContext.Session.SetString("DineInCustomerName", customer.Name);

    var cookie = JsonSerializer.Serialize(new DineInCustomerCookie
    {
        Id = customer.Id,
        Name = customer.Name
    });

    Response.Cookies.Append(
        "CloudyCafeDineInCustomer",
        _dineInCustomerCookieProtector.Protect(cookie),
        new CookieOptions { Expires = DateTimeOffset.Now.AddDays(30) });
}
```

---

## 4.2 Authorization và Phân Quyền

### 4.2.1 Role-based Authorization

Hệ thống sử dụng 3 roles chính:

| Role | Mô Tả |
|------|-------|
| `admin` | Quản trị viên - toàn quyền |
| `manager` | Quản lý - hầu hết quyền (trừ quản lý nhân viên/quyền) |
| `staff` | Nhân viên - chỉ xem/sản phẩm, đơn hàng, khách hàng |

**Role Assignment:**

```
NhanVien (Employee)
    ↓ (1:N)
NvPq (Employee-Role mapping)
    ↓ (N:1)
PhanQuyen (Role: admin/manager/staff)
```

### 4.2.2 Attribute-based Authorization

**1. AdminSessionAuthorizeAttribute:**

```csharp
public class AdminSessionAuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var sessionId = context.HttpContext.Session.GetInt32("NhanVienId");
        
        if (!sessionId.HasValue)
        {
            // Chưa đăng nhập → redirect về login
            context.Result = RedirectToAction(nameof(Login));
            return;
        }

        // Kiểm tra nhân viên còn tồn tại
        var employeeExists = _context.NhanViens
            .Any(e => e.NvId == sessionId && !e.Remove);

        if (!employeeExists)
        {
            context.HttpContext.Session.Remove("NhanVienId");
            context.Result = RedirectToAction(nameof(Login));
        }
    }
}
```

**2. RoleAuthorizeAttribute:**

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RoleAuthorizeAttribute : ActionFilterAttribute
{
    private readonly string[] _allowedRoles;

    public RoleAuthorizeAttribute(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userRole = context.HttpContext.GetUserRole();

        if (_allowedRoles.Length > 0)
        {
            if (!_allowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = BuildUnauthorizedResult(context);
                return;
            }
        }

        base.OnActionExecuting(context);
    }

    private static IActionResult BuildUnauthorizedResult(ActionExecutingContext context)
    {
        var acceptsJson = context.HttpContext.Request.Headers["Accept"]
            .Contains("application/json");

        if (acceptsJson)
        {
            return new UnauthorizedObjectResult(new
            {
                success = false,
                message = "Bạn không có quyền này.",
                redirectUrl = "/Admin/Login"
            });
        }

        return new ViewResult { ViewName = "~/Views/Shared/AccessDenied.cshtml" };
    }
}
```

### 4.2.3 Module-based Authorization

**HttpContextExtensions:**

```csharp
public static class HttpContextExtensions
{
    public static bool CanAccessModule(this HttpContext context, string module)
    {
        var role = context.GetUserRole();
        return role switch
        {
            "admin" => true,  // Admin truy cập tất cả
            "manager" => IsManagerAllowed(module),
            "staff" => IsStaffAllowed(module),
            _ => false
        };
    }

    private static bool IsManagerAllowed(string module)
    {
        var deniedModules = new[] { "employees", "roles", "permissions" };
        return !deniedModules.Contains(module.ToLower());
    }

    private static bool IsStaffAllowed(string module)
    {
        var allowedModules = new[] { "products", "orders", "customers", "tables" };
        return allowedModules.Contains(module.ToLower());
    }
}
```

---

## 4.3 SignalR Real-time Features

### 4.3.1 Tổng Quan

Hệ thống sử dụng **SignalR** để cung cấp tính năng real-time communication giữa server và client.

**Hub:** `ChatHub`
**Hub Path:** `/chatHub`

### 4.3.2 ChatHub Methods

```csharp
public class ChatHub : Hub
{
    // Gửi tin nhắn đến tất cả client
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }

    // Thông báo thay đổi database
    public async Task NotifyDatabaseChange()
    {
        await Clients.All.SendAsync("DatabaseUpdated");
    }

    // Thông báo sản phẩm bị xóa
    public async Task NotifyProductDeleted(int productId)
    {
        await Clients.All.SendAsync("ProductDeleted", productId);
    }

    // Thông báo đơn hàng thành công
    public async Task NotifyOrderSuccess()
    {
        await Clients.All.SendAsync("OrderSuccess");
    }

    // Yêu cầu dịch vụ tại quán
    public async Task NotifyDineInServiceRequest(object request)
    {
        await Clients.All.SendAsync("DineInServiceRequested", request);
    }

    // Hoàn thành thanh toán tại quán
    public async Task NotifyDineInPaymentCompleted(object payment)
    {
        await Clients.All.SendAsync("DineInPaymentCompleted", payment);
    }
}
```

### 4.3.3 Client-side Usage

**JavaScript:**

```javascript
// Kết nối SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

// Nhận thông báo đơn hàng mới
connection.on("OrderSuccess", (orderId) => {
    console.log("Đơn hàng mới:", orderId);
    updateOrderList();
});

// Nhận thông báo giỏ hàng cập nhật
connection.on("DatabaseUpdated", () => {
    console.log("Database updated");
    refreshCart();
});

// Nhận thông báo sản phẩm bị xóa
connection.on("ProductDeleted", (productId) => {
    console.log("Product deleted:", productId);
    removeProductFromList(productId);
});

// Gửi yêu cầu dịch vụ
async function sendServiceRequest(requestData) {
    await connection.send("NotifyDineInServiceRequest", requestData);
}

// Start connection
connection.start().catch(err => console.error(err.toString()));
```

### 4.3.4 Real-time Events in Controllers

**HomeController - Tạo đơn hàng online:**

```csharp
_hubContext.Clients.All.SendAsync("OrderSuccess", DH.DhId);
_hubContext.Clients.All.SendAsync("OnlineOrderCreated", DH.DhId);
```

**HomeController - Thêm món vào giỏ:**

```csharp
_hubContext.Clients.All.SendAsync("DatabaseUpdated");
_hubContext.Clients.All.SendAsync("OnlineCartUpdated");
```

**DonHangsController - Cập nhật trạng thái giao hàng:**

```csharp
await _hubContext.Clients.All.SendAsync(
    "OnlineOrderStatusUpdated", id, metadata.DeliveryStatus);
```

**AdminController - Thanh toán tại quán:**

```csharp
await _hubContext.Clients.All.SendAsync(
    "DineInPaymentCompleted", new {
        tableId = BanId,
        totalLabel = FormatCurrency(totalAmount),
        paymentMethodLabel,
        paidAt = DateTime.Now.ToString("dd/MM HH:mm")
    });
```

---

## 4.4 Xử Lý Giỏ Hàng

### 4.4.1 Dine-in Cart

**Special lưu ý:**
- Không cần đăng nhập
- Lưu trong Session + Database
- Dùng `BanId` để xác định bàn
- Dùng `DineInCustomerId` (cookie) để persistent across sessions

**Cart Flow:**

```
1. Khách tại bàn quét QR → BanId trong session
   ↓
2. Nhập tên → Tạo DineInCustomer (cookie)
   ↓
3. GetOrCreateDineInOrder() → Tạo đơn hàng mới
   ↓
4. Chọn món → CreateProductDetail()
   ↓
5. Thêm vào ChiTietHoaDon (Ghi chú: trạng thái, yêu cầu)
   ↓
6. SignalR update → Admin nhận thông báo
```

**Order Metadata (JSON trong GhiChu):**

```json
{
  "t": "dinein",
  "g": "guest_id_abc123",
  "n": "Nguyễn Văn A",
  "b": 5,
  "s": "submitted",
  "at": "2026-05-19T10:30:00"
}
```

### 4.4.2 Online Cart

**Lưu ý:**
- Cần đăng nhập
- Lưu trong Database
- Dùng `CustomerID` để xác định
- Metadata chứa thông tin giao hàng

**Cart Flow:**

```
1. Khách đăng nhập → CustomerID trong session
   ↓
2. GetOrCreateOnlineCartOrder() → Tạo đơn hàng mới
   ↓
3. Chọn món → CreateOnlineProductDetail()
   ↓
4. Điền thông tin giao hàng
   ↓
5. Checkout → UpdateOrderStatus("pending")
   ↓
6. SignalR → Admin nhận đơn
```

**Order Metadata (JSON trong GhiChu):**

```json
{
  "t": "online",
  "s": "pending",
  "n": "Nguyễn Văn A",
  "p": "0901234567",
  "c": "Hà Nội",
  "d": "Thanh Xuân",
  "w": "Xuân Thủy",
  "a": "123 Đường Xuân Thủy",
  "note": "Giao giờ hành chính",
  "at": "2026-05-19T14:30:00"
}
```

### 4.4.3 Xử Lý Giá và Điều Kiện

**ResolveOptionExtra:**

```csharp
private decimal ResolveOptionExtra(string? condition, string? ghichu)
{
    decimal extra = 0;

    // Size L +10,000đ
    if (condition?.Contains("Size L", StringComparison.OrdinalIgnoreCase) == true)
    {
        extra += 10000m;
    }

    // Các tùy chọn sữa +5,000đ
    if (!string.IsNullOrWhiteSpace(ghichu))
    {
        var normalizedNote = StringUtils.ConvertToLowerAndRemoveDiacritics(ghichu);
        
        if (normalizedNote.Contains("sữa tươi")) extra += 5000m;
        if (normalizedNote.Contains("sữa yên mạch")) extra += 5000m;
        if (normalizedNote.Contains("sữa đặc")) extra += 5000m;
        
        // Foam dừa +10,000đ
        if (normalizedNote.Contains("foam dừa")) extra += 10000m;
    }

    return extra;
}
```

---

## 4.5 Pattern: Soft Delete

**Triển khai:**

```csharp
// Tất cả entity có field Remove (bit)
public partial class Product
{
    public bool Remove { get; set; }  // Default: false
}
```

**Khi xóa:**

```csharp
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var product = await _context.Products.FindAsync(id);
    
    if (product != null)
    {
        product.Remove = true;  // Soft delete thay vì xóa hẳn
    }

    await _context.SaveChangesAsync();
}
```

**Khi query (luôn lọc Remove = false):**

```csharp
var products = await _context.Products
    .Where(p => !p.Remove)
    .ToListAsync();
```

---

*Tài liệu tiếp tục ở phần 5...*