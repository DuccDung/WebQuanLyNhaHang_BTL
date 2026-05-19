# 5. PHÂN TÍCH FRONTEND VÀ VIEWS

## 5.1 Tổng Quan

Hệ thống sử dụng **Razor Views** (`.cshtml`) làm view engine, kết hợp với **Bootstrap 4** cho UI framework và **jQuery** cho client-side scripting.

---

## 5.2 Cấu Trúc Views

```
Views/
├── Admin/                    # Views quản trị
│   ├── Index.cshtml         # Dashboard
│   ├── Login.cshtml         # Đăng nhập
│   └── Ban.cshtml           # Quản lý bàn
├── Home/                     # Views khách hàng
│   ├── Index.cshtml         # Trang chủ
│   ├── Menu.cshtml          # Menu
│   ├── Cart.cshtml          # Giỏ hàng
│   ├── Client.cshtml        # Khách tại bàn
│   ├── Service.cshtml       # Dịch vụ
│   ├── Account.cshtml       # Tài khoản
│   └── OrderHistory.cshtml  # Lịch sử đơn
├── Products/                 # Views sản phẩm
│   ├── Index.cshtml         # Danh sách
│   ├── Create.cshtml        # Tạo mới
│   ├── Edit.cshtml          # Sửa
│   └── Details.cshtml       # Chi tiết
├── DonHangs/                 # Views đơn hàng
│   ├── Index.cshtml
│   ├── Details.cshtml
│   └── Create.cshtml
├── KhachHangs/               # Views khách hàng
├── NhanViens/                # Views nhân viên
├── TrangChu/                 # Views trang chủ công khai
│   ├── Index.cshtml         # Home public
│   ├── Menu.cshtml          # Menu public
│   ├── ChuyenNha.cshtml     # Blog
│   └── CuaHang.cshtml       # Store list
└── Shared/                   # Shared components
    ├── _Layout.cshtml
    ├── _AdminSidebar.cshtml
    ├── _AdminContentLayout.cshtml
    ├── _AdminLoginLayout.cshtml
    ├── _ValidationScriptsPartial.cshtml
    ├── Error.cshtml
    └── Components/          # ViewComponents
        ├── Category/
        ├── ProductCondition/
        ├── MenuItemEdit/
        ├── MenuAddSub/
        ├── AccLoggin/
        ├── Chart/
        └── foodterCTHD/
```

---

## 5.3 Layouts

### 5.3.1 Head_Layout.cshtml (Admin)

**File:** `Views/Shared/Head_Layout.cshtml`

**Mô tả:** Layout chính cho admin dashboard

**Includes:**
```html
<!-- CSS -->
<link href="~/assets/plugins/bootstrap/css/bootstrap.min.css" />
<link href="~/assets/plugins/chartist/dist/chartist.css" />
<link href="~/assets/css/main.css" />
<link href="~/assets/css/responsive.css" />
<link href="~/assets/icon/themify-icons/themify-icons.css" />
<link href="~/assets/icon/icofont/css/icofont.css" />

<!-- JS -->
<script src="~/assets/plugins/Jquery/dist/jquery.min.js"></script>
<script src="~/assets/plugins/bootstrap/js/bootstrap.min.js"></script>
<script src="~/assets/plugins/charts/echarts/js/echarts-all.js"></script>
<script src="~/assets/js/main.min.js"></script>
```

### 5.3.2 Header_Left_Layout.cshtml (Admin Sidebar)

**File:** `Views/Shared/Header_Left_Layout.cshtml`

**Mô tả:** Sidebar điều hướng admin

**Cấu trúc:**

```html
@{
    var activeNav = ViewData["ActiveAdminNav"]?.ToString();
    var userRole = Context.GetUserRole();
}

<aside class="sidebar">
    <div class="sidebar__brand">
        <span>F&B Admin</span>
    </div>

    <nav class="sidebar__nav">
        <!-- Dashboard -->
        <a href="@Url.Action("Index", "Admin")">
            <span>Dashboard</span>
        </a>

        <!-- Products (Admin, Manager, Staff) -->
        @if (userRole is "admin" or "manager" or "staff")
        {
            <a href="@Url.Action("Index", "Products")">
                <span>Sản phẩm</span>
            </a>
        }

        <!-- Orders (Admin, Manager, Staff) -->
        @if (userRole is "admin" or "manager" or "staff")
        {
            <a href="@Url.Action("Index", "DonHangs")">
                <span>Đơn hàng</span>
            </a>
        }

        <!-- Customers (Admin, Manager, Staff) -->
        @if (userRole is "admin" or "manager" or "staff")
        {
            <a href="@Url.Action("Index", "KhachHangs")">
                <span>Khách hàng</span>
            </a>
        }

        <!-- Employees (Admin, Manager only) -->
        @if (userRole is "admin" or "manager")
        {
            <a href="@Url.Action("Index", "NhanViens")">
                <span>Nhân viên</span>
            </a>
        }

        <!-- Stores (Admin, Manager only) -->
        @if (userRole is "admin" or "manager")
        {
            <a href="@Url.Action("Index", "AdminCuaHangs")">
                <span>Cửa hàng</span>
            </a>
        }

        <!-- Stories (Admin, Manager only) -->
        @if (userRole is "admin" or "manager")
        {
            <a href="@Url.Action("Index", "AdminBaiVietChuyenNhas")">
                <span>Chuyện nhà</span>
            </a>
        }

        <!-- Logout -->
        <a class="logout-button" href="@Url.Action("Logout", "Admin")">
            <span>Đăng xuất</span>
        </a>
    </nav>
</aside>
```

---

## 5.4 ViewComponents

ViewComponents là các component tái sử dụng trong hệ thống.

### 5.4.1 CategoryViewComponent

**File:** `Views/Shared/Components/Category/CategoryViewComponent.cs`

**Mô tả:** Hiển thị danh sách danh mục sản phẩm trong menu

**Code:**

```csharp
public class CategoryViewComponent : ViewComponent
{
    private readonly QlnhaHangBtlContext _context;

    public CategoryViewComponent(QlnhaHangBtlContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        var categories = _context.Categories
            .Where(c => !c.Remove)
            .OrderBy(c => c.CateId)
            .ToList();

        return View(categories);
    }
}
```

**View:** `Views/Shared/Components/Category/Default.cshtml`

```html
@model List<WebQuanLyNhaHang.Models.Category>

<div class="menu__category-nav">
    @foreach (var category in Model)
    {
        <a href="javascript:void(0)" class="category-item" data-category-id="@category.CateId">
            @category.TenLoaiSanPham
        </a>
    }
</div>
```

### 5.4.2 MenuItemEditViewComponent

**Mô tả:** Component tăng giảm số lượng sản phẩm trong giỏ (dine-in)

**Code:**

```csharp
public class MenuItemEditViewComponent : ViewComponent
{
    private readonly QlnhaHangBtlContext _context;

    public MenuItemEditViewComponent(QlnhaHangBtlContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke(int ProductId, int? DhId)
    {
        var banId = HttpContext.Session.GetInt32("BanId");
        var dineInCustomerId = HttpContext.Session.GetString("DineInCustomerId");

        var existingItem = _context.ChiTietHoaDons
            .Include(cthd => cthd.Dh)
            .FirstOrDefault(cthd =>
                cthd.ProductId == ProductId &&
                cthd.DhId == DhId &&
                !cthd.Remove &&
                cthd.Dh.VanChuyen != true);

        var quantity = existingItem?.SoLuong ?? 0;

        return View(new MenuItemEditViewModel
        {
            ProductId = ProductId,
            DhId = DhId,
            Quantity = quantity,
            BanId = banId,
            DineInCustomerId = dineInCustomerId
        });
    }
}
```

### 5.4.3 MenuAddSubViewComponent

**Mô tả:** Component cộng/trừ số lượng sản phẩm

**Code:**

```csharp
public class MenuAddSubViewComponent : ViewComponent
{
    private readonly QlnhaHangBtlContext _context;

    public MenuAddSubViewComponent(QlnhaHangBtlContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke(int ProductId, int? DhId)
    {
        var existingItem = _context.ChiTietHoaDons
            .Include(cthd => cthd.Product)
            .FirstOrDefault(cthd =>
                cthd.ProductId == ProductId &&
                cthd.DhId == DhId &&
                !cthd.Remove);

        var quantity = existingItem?.SoLuong ?? 0;
        var price = existingItem?.Product?.GiaTien ?? 0m;

        return View(new MenuAddSubViewModel
        {
            ProductId = ProductId,
            DhId = DhId,
            Quantity = quantity,
            Price = price
        });
    }
}
```

### 5.4.4 AccLogginViewComponent

**Mô tả:** Component đăng nhập khách hàng

**Code:**

```csharp
public class AccLogginViewComponent : ViewComponent
{
    private readonly QlnhaHangBtlContext _context;

    public AccLogginViewComponent(QlnhaHangBtlContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        var customerId = Context.Session.GetInt32("CustomerID");
        var isLoggedIn = customerId.HasValue;

        return View(new AccLogginViewModel { IsLoggedIn = isLoggedIn });
    }
}
```

### 5.4.5 ChartViewComponent

**Mô tả:** Component hiển thị biểu đồ trong dashboard

**Code:**

```csharp
public class ChartViewComponent : ViewComponent
{
    private readonly QlnhaHangBtlContext _context;

    public ChartViewComponent(QlnhaHangBtlContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var chartData = await _context.DonHangs
            .Where(o => !o.Remove && o.TrangThai == true)
            .GroupBy(o => new { o.GioRa.Value.Year, o.GioRa.Value.Month })
            .Select(g => new ChartData
            {
                Month = $"{g.Key.Month}/{g.Key.Year}",
                Total = g.Sum(o => o.TongTien ?? 0)
            })
            .OrderBy(x => x.Month)
            .ToListAsync();

        return View(chartData);
    }
}
```

---

## 5.5 Frontend JavaScript

### 5.5.1 Menu.js (Menu tương tác)

**File:** `wwwroot/asset/js/menu_home.js`

**Chức năng:**
- Smooth scroll đến danh mục
- Highlight active category khi scroll
- Search functionality
- Add to cart animations

**Code mẫu - Search:**

```javascript
$(document).ready(function() {
    let searchTimer = null;

    $('#txtSearchBox').on('input', function() {
        const searchKey = $(this).val().trim();
        
        clearTimeout(searchTimer);
        
        searchTimer = setTimeout(function() {
            if (!searchKey) {
                restoreFullMenu();
                return;
            }

            $.ajax({
                url: '/Home/GetName',
                data: { txtsearch: searchKey },
                success: function(response) {
                    $('#content').html(response);
                    refreshMenuReveal();
                }
            });
        }, 300);
    });

    function restoreFullMenu() {
        $.ajax({
            url: '/Home/Menu',
            success: function(response) {
                $('#content').html(response);
                refreshMenuReveal();
            }
        });
    }
});
```

### 5.5.2 SignalR Client

**Chức năng:** Kết nối SignalR để nhận thông báo real-time

**Code:**

```javascript
// Kết nối SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .withAutomaticReconnect()
    .build();

// Xử lý khi có đơn hàng mới
connection.on("OrderSuccess", function(orderId) {
    console.log("Đơn hàng mới:", orderId);
    showOrderNotification(orderId);
    refreshOrderList();
});

// Xử lý khi giỏ hàng cập nhật
connection.on("DatabaseUpdated", function() {
    console.log("Database updated");
    updateCartCount();
});

// Xử lý khi sản phẩm bị xóa
connection.on("ProductDeleted", function(productId) {
    console.log("Sản phẩm bị xóa:", productId);
    removeProductFromList(productId);
});

// Xử lý khi trạng thái đơn hàng online thay đổi
connection.on("OnlineOrderStatusUpdated", function(orderId, newStatus) {
    console.log("Trạng thái đơn hàng cập nhật:", orderId, newStatus);
    updateOrderStatus(orderId, newStatus);
});

// Kết nối
connection.start()
    .then(function() {
        console.log("Connected to SignalR hub");
    })
    .catch(function(err) {
        console.error("SignalR connection error:", err.toString());
    });

// Gửi yêu cầu gọi nhân viên
function sendServiceRequest(type, message) {
    $.ajax({
        url: '/Home/SendDineInServiceRequest',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            type: type,
            message: message,
            timestamp: new Date().toISOString()
        })
    });
}
```

### 5.5.3 Cart Functionality

**Chức năng:**
- Thêm/sửa/xóa sản phẩm trong giỏ
- Cập nhật số lượng
- Tính tổng tiền

**Code mẫu:**

```javascript
// Thêm sản phẩm vào giỏ
function addToCart(productId, quantity, conditions, notes) {
    $.ajax({
        url: '/Home/CreateProductDetail',
        type: 'POST',
        data: {
            productid: productId,
            soluong: quantity,
            condition: conditions,
            ghichu: notes,
            returnUrl: window.location.href
        },
        success: function(response) {
            updateCartUI();
            showSuccessMessage("Đã thêm món vào giỏ");
        },
        error: function(xhr) {
            showErrorMessage("Có lỗi khi thêm món");
        }
    });
}

// Cập nhật số lượng
function updateQuantity(cthdId, newQuantity) {
    $.ajax({
        url: '/Home/UpdateCartItemQuantity',
        type: 'POST',
        data: {
            id: cthdId,
            quantity: newQuantity
        },
        success: function(response) {
            $('#cart-item-' + cthdId).html(response);
            updateCartTotal();
        }
    });
}

// Xóa sản phẩm
function removeFromCart(cthdId) {
    if (!confirm("Bạn có chắc chắn muốn xóa món này?")) return;

    $.ajax({
        url: '/Home/RemoveItem',
        type: 'POST',
        data: { id: cthdId },
        success: function(response) {
            $('#cart-item-' + cthdId).remove();
            updateCartTotal();
            showSuccessMessage("Đã xóa món");
        }
    });
}

// Tính tổng tiền
function updateCartTotal() {
    let total = 0;
    $('.cart-item').each(function() {
        const itemTotal = parseFloat($(this).data('total'));
        total += itemTotal;
    });
    
    $('#cart-total').text(formatCurrency(total));
}

// Format tiền Việt Nam
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount) + 'đ';
}
```

---

## 5.6 CSS Styling

### 5.6.1 Main CSS Files

| File | Mục đích |
|------|----------|
| `base.css` | Base styles, reset |
| `home.css` | Trang chủ styles |
| `menu_home.css` | Menu styles |
| `responsive.css` | Responsive breakpoints |
| `loginhome.css` | Login page styles |
| `service.css` | Service page styles |

### 5.6.2 Bootstrap Plugins

- `bootstrap-datepicker` - Date picker
- `bootstrap-daterangepicker` - Date range picker
- `bootstrap-multiselect` - Multi-select dropdown
- `bootstrap-tagsinput` - Tags input
- `select2` - Enhanced select
- `spectrum` - Color picker

### 5.6.3 Chart Libraries

- `Chartist` - Line, bar, pie charts
- `Morris.js` - Area, bar, line charts
- `Flot` - jQuery charts
- `ECharts` - Advanced charts
- `Highcharts` - Business charts

---

## 5.7 Responsive Design

**Breakpoints:**

```css
/* Mobile */
@media (max-width: 767px) {
    /* Mobile styles */
}

/* Tablet */
@media (min-width: 768px) and (max-width: 991px) {
    /* Tablet styles */
}

/* Desktop */
@media (min-width: 992px) {
    /* Desktop styles */
}

/* Large Desktop */
@media (min-width: 1200px) {
    /* Large desktop styles */
}
```

---

## 5.8 Form Validation

**Client-side validation sử dụng jQuery Validate:**

```javascript
$("#orderForm").validate({
    rules: {
        hoTen: {
            required: true,
            minlength: 2
        },
        soDienThoai: {
            required: true,
            digits: true,
            minlength: 9,
            maxlength: 11
        },
        tinhThanh: {
            required: true
        },
        quanHuyen: {
            required: true
        },
        diaChi: {
            required: true,
            minlength: 10
        }
    },
    messages: {
        hoTen: {
            required: "Vui lòng nhập họ tên",
            minlength: "Họ tên phải ít nhất 2 ký tự"
        },
        soDienThoai: {
            required: "Vui lòng nhập số điện thoại",
            digits: "Số điện thoại chỉ chứa số",
            minlength: "Số điện thoại tối thiểu 9 số"
        }
    },
    submitHandler: function(form) {
        form.submit();
    }
});
```

---

*Tài liệu tiếp tục ở phần 6...*