# 3. PHÂN TÍCH CONTROLLERS VÀ API ENDPOINTS

## 3.1 Tổng Quan

Hệ thống sử dụng kiến trúc **MVC (Model-View-Controller)** với các controllers được phân chia rõ ràng theo chức năng nghiệp vụ.

---

## 3.2 Danh Sách Controllers

| Controller | File | Mô Tả |
|------------|------|-------|
| AdminController | AdminController.cs | Quản trị hệ thống, dashboard |
| HomeController | HomeController.cs | Trang chủ khách hàng, giỏ hàng |
| TrangChuController | TrangChuController.cs | Trang chủ công khai, menu |
| ProductsController | ProductsController.cs | Quản lý sản phẩm |
| DonHangsController | DonHangsController.cs | Quản lý đơn hàng |
| KhachHangsController | KhachHangsController.cs | Quản lý khách hàng |
| NhanViensController | NhanViensController.cs | Quản lý nhân viên |
| AdminBaiVietChuyenNhasController | AdminBaiVietChuyenNhasController.cs | Quản lý bài viết |
| AdminCuaHangsController | AdminCuaHangsController.cs | Quản lý cửa hàng |
| ChiTietHoaDonsController | ChiTietHoaDonsController.cs | Chi tiết hóa đơn |
| ChartDataController | ChartDataController.cs | Dữ liệu biểu đồ |

---

## 3.3 Chi Tiết Controllers

### 3.3.1 AdminController

**File:** `Controllers/AdminController.cs`

**Mô tả:** Xử lý các chức năng quản trị hệ thống bao gồm dashboard, thanh toán, thống kê.

**Attributes:**
- `[AdminSessionAuthorize]` - Xác thực session admin
- `[RoleAuthorize("admin", "manager")]` - Phân quyền

#### Methods:

| Method | HTTP | Route | Mô Tả |
|--------|------|-------|-------|
| `Index()` | GET | /Admin | Dashboard thống kê |
| `Login()` | GET | /Admin/Login | Form đăng nhập |
| `Login(name, password)` | POST | /Admin/Login | Xử lý đăng nhập |
| `Logout()` | GET | /Admin/Logout | Đăng xuất |
| `Ban()` | GET | /Admin/Ban | Quản lý bàn (dine-in) |
| `GetFormBuy(id)` | GET | /Admin/GetFormBuy/Id | Form thanh toán bàn |
| `LatestOrderNotification(id?)` | GET | /Admin/LatestOrderNotification | Thông báo đơn mới |
| `ProcessPayment(BanId, paymentMethod)` | POST | /Admin/ProcessPayment | Xử lý thanh toán |

**Logic quan trọng - Index() (Dashboard):**

```csharp
public async Task<IActionResult> Index()
{
    // Lấy tất cả đơn hàng chưa xóa
    var orders = await _context.DonHangs
        .AsNoTracking()
        .Where(order => !order.Remove)
        .Select(order => new DashboardOrderSnapshot(...))
        .ToListAsync();

    // Tính toán doanh thu theo tháng hiện tại và tháng trước
    var currentRevenue = currentMonthOrders.Sum(order => order.Revenue);
    var previousRevenue = previousMonthOrders.Sum(order => order.Revenue);

    // Trả về view với dữ liệu
    return View(model);
}
```

**Logic quan trọng - Login():**

```csharp
[HttpPost]
public async Task<IActionResult> Login(string? name, string? password)
{
    // Tìm nhân viên theo tài khoản và mật khẩu
    var nhanVien = _context.NhanViens
        .FirstOrDefault(e => !e.Remove && e.TaiKhoan == name && e.MatKhau == password);

    if (nhanVien == null)
    {
        ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng.";
        return View();
    }

    // Set session cho nhân viên
    HttpContext.Session.SetInt32("NhanVienId", nhanVien.NvId);
    HttpContext.Session.SetString("NhanVienName", nhanVien.TenNhanVien);
    HttpContext.Session.SetString("RoleKey", roleKey);

    return RedirectToAction(nameof(Index));
}
```

---

### 3.3.2 HomeController

**File:** `Controllers/HomeController.cs`

**Mô tả:** Xử lý trang chủ cho khách hàng, bao gồm giỏ hàng (dine-in và online), đặt hàng, signalR real-time features.

**Dependencies:**
- `QlnhaHangBtlContext` - Database context
- `IHubContext<ChatHub>` - SignalR hub
- `IDataProtectionProvider` - Cookie protection

**Sessions sử dụng:**
- `CustomerID` - ID khách hàng đã đăng nhập
- `CustomerCookie` - Cookie remembered customer
- `BanId` - ID bàn ăn
- `DineInCustomerId` - Guest ID cho dine-in
- `OnlineCartDhId` - Giỏ hàng online
- `DhId` - Giỏ hàng dine-in (legacy)

#### Methods - Authentication:

| Method | HTTP | Mô Tả |
|--------|------|-------|
| `GetCurrentCustomerId()` | GET | Lấy ID khách hàng hiện tại |
| `SetCustomerLogin(customerId)` | - | Đăng nhập khách hàng |
| `ClearCustomerLogin()` | - | Đăng xuất khách hàng |
| `CustomerLogin(email, password)` | POST | Đăng nhập |
| `CustomerRegister(username, email, password)` | POST | Đăng ký |
| `CustomerLogout()` | POST | Đăng xuất |

#### Methods - Dine-in (Tại Quán):

| Method | HTTP | Route | Mô Tả |
|--------|------|-------|-------|
| `Client(BanId)` | GET | /Home/Client/Id | Trang khách tại bàn |
| `CustomerInfo(CustomerName)` | POST | /Home/CustomerInfo | Nhập tên khách |
| `Service()` | GET | /Home/Service | Trang dịch vụ/gọi món |
| `Menu()` | GET | /Home/Menu | Menu món ăn |
| `ProductDetail(ProductID, cthdId)` | GET | /Home/ProductDetail | Chi tiết sản phẩm |
| `CreateProductDetail(...)` | POST | /Home/CreateProductDetail | Thêm món vào đơn |
| `Cart(DhId)` | GET | /Home/Cart/Id | Giỏ hàng |
| `OrderSuccess()` | GET | /Home/OrderSuccess | Xác nhận đơn |
| `SendDineInServiceRequest(...)` | POST | /Home/SendDineInServiceRequest | Gọi nhân viên |

#### Methods - Online Order (Giao Hàng):

| Method | HTTP | Route | Mô Tả |
|--------|------|-------|-------|
| `OnlineCart()` | GET | /Home/OnlineCart | Giỏ hàng online |
| `CreateOnlineProductDetail(...)` | POST | /Home/CreateOnlineProductDetail | Thêm món online |
| `RemoveOnlineItem(id)` | POST | /Home/RemoveOnlineItem/Id | Xóa món online |
| `UpdateOnlineItemQuantity(id, quantity)` | POST | /Home/UpdateOnlineItemQuantity | Cập nhật số lượng |
| `CheckoutOnlineOrder(form)` | POST | /Home/CheckoutOnlineOrder | Thanh toán online |
| `OrderHistory()` | GET | /Home/OrderHistory | Lịch sử đơn hàng |

#### Methods - Cart Management:

| Method | HTTP | Mô Tả |
|--------|------|-------|
| `GetOrCreateDineInOrder()` | - | Tạo/lấy đơn dine-in |
| `GetOrCreateOnlineCartOrder()` | - | Tạo/lấy đơn online |
| `GetActiveDineInOrderId()` | - | Lấy ID đơn dine-in |
| `GetActiveOnlineCartOrder()` | - | Lấy ID đơn online |
| `RefreshCartTotal(dhId)` | - | Làm mới tổng tiền |
| `RemoveItem(id)` | POST | Xóa món khỏi giỏ |
| `ClearCart()` | POST | Xóa hết giỏ hàng |

**Logic quan trọng - GetOrCreateDineInOrder():**

```csharp
private DonHang GetOrCreateDineInOrder()
{
    var banId = HttpContext.Session.GetInt32("BanId");
    var customer = GetCurrentDineInCustomer();

    // Tìm đơn hàng đang hoạt động cho bàn và khách này
    var DH = _context.DonHangs
        .FirstOrDefault(order =>
            order.BanId == banId &&
            !order.Remove &&
            order.VanChuyen != true &&  // Không phải delivery
            order.TrangThai != true);    // Chưa xác nhận

    if (DH != null) return DH;

    // Tạo đơn mới nếu chưa có
    DH = new DonHang
    {
        BanId = banId.Value,
        GioRa = DateTime.Now,
        GhiChu = DineInOrderMetadata.Create(customer, banId).ToJson(),
        TrangThai = false,
        VanChuyen = false
    };

    _context.DonHangs.Add(DH);
    _context.SaveChanges();
    
    return DH;
}
```

**Logic quan trọng - CreateProductDetail():**

```csharp
[HttpPost]
public IActionResult CreateProductDetail(int soluong, int productid, int? cthdId, string? condition, string? ghichu, string? returnUrl)
{
    var product = _context.Products.FirstOrDefault(item => item.ProductId == productid && !item.Remove);
    
    var quantity = Math.Max(1, soluong);
    var DH = GetOrCreateDineInOrder();
    
    // Tạo ghi chú từ điều kiện
    var note = string.Join(" ", new[]
    {
        string.IsNullOrWhiteSpace(condition) ? null : $"Trạng Thái: {condition}",
        string.IsNullOrWhiteSpace(ghichu) ? null : $"Ghi Chú: {ghichu}"
    }).Where(item => item != null);

    // Tính giá đơn vị (bao gồm extra options)
    var unitPrice = (product.GiaTien ?? 0m) + ResolveOptionExtra(condition, ghichu);

    if (cthdId.HasValue)
    {
        // Cập nhật món đã có
        var editCartItem = _context.ChiTietHoaDons.FirstOrDefault(...);
        if (editCartItem != null)
        {
            editCartItem.Ghichu = note;
            editCartItem.SoLuong = quantity;
            editCartItem.ThanhTien = unitPrice * quantity;
        }
    }
    else
    {
        // Thêm món mới
        var cartItem = new ChiTietHoaDon
        {
            DhId = DH.DhId,
            ProductId = productid,
            Ghichu = note,
            SoLuong = quantity,
            ThanhTien = unitPrice * quantity
        };
        _context.ChiTietHoaDons.Add(cartItem);
    }

    _context.SaveChanges();
    RefreshCartTotal(DH.DhId);
    
    // Gửi thông báo real-time qua SignalR
    _hubContext.Clients.All.SendAsync("DatabaseUpdated");

    return RedirectAfterAddToCart(returnUrl);
}
```

**Logic quan trọng - CheckoutOnlineOrder():**

```csharp
[HttpPost]
public IActionResult CheckoutOnlineOrder(OnlineCheckoutForm form)
{
    var DH = GetActiveOnlineCartOrder();
    var customerId = HttpContext.Session.GetInt32("CustomerID");
    var customer = _context.KhachHangs.FirstOrDefault(...);

    // Cập nhật thông tin khách hàng
    customer.TenKhachHang = CleanText(form.HoTen, 100);
    customer.SoDienThoai = CleanText(form.SoDienThoai, 15);
    customer.DiaChi = CleanText(BuildFullAddress(form), 200);
    DH.KhId = customer.KhId;

    // Cập nhật metadata đơn hàng
    var metadata = OnlineOrderMetadata.TryParse(DH.GhiChu) ?? OnlineOrderMetadata.CreateCart();
    metadata.DeliveryStatus = OnlineOrderMetadata.StatusPending;
    metadata.RecipientName = CleanText(form.HoTen, 80);
    metadata.Phone = CleanText(form.SoDienThoai, 20);
    // ...
    metadata.SubmittedAt = DateTime.Now;

    DH.GhiChu = metadata.ToJson();
    DH.TrangThai = true;
    DH.VanChuyen = true;
    DH.GioRa = DateTime.Now;

    // Tạo/Cập nhật OnlineOrderInfo
    var onlineInfo = _context.OnlineOrderInfos.FirstOrDefault(...);
    if (onlineInfo == null)
    {
        onlineInfo = new OnlineOrderInfo { DhId = DH.DhId };
        _context.OnlineOrderInfos.Add(onlineInfo);
    }

    onlineInfo.TrangThaiGiaoHang = OnlineOrderMetadata.StatusPending;
    onlineInfo.NgayDat = metadata.SubmittedAt;
    onlineInfo.NgayCapNhat = DateTime.Now;

    _context.SaveChanges();
    HttpContext.Session.Remove(OnlineCartSessionKey);

    // Gửi thông báo
    _hubContext.Clients.All.SendAsync("OderSuccess", DH.DhId);
    _hubContext.Clients.All.SendAsync("OnlineOrderCreated", DH.DhId);

    return RedirectToAction("OnlineCart", "Home");
}
```

---

### 3.3.3 TrangChuController

**File:** `Controllers/TrangChuController.cs`

**Mô tả:** Trang chủ công khai (không cần đăng nhập), hiển thị menu, bài viết, cửa hàng.

#### Methods:

| Method | HTTP | Route | Mô Tả |
|--------|------|-------|-------|
| `Index()` | GET | / | Trang chủ với sản phẩm hot |
| `Menu()` | GET | /TrangChu/Menu | Toàn bộ menu |
| `DoUong()` | GET | /TrangChu/DoUong | Menu đồ uống |
| `Banh()` | GET | /TrangChu/Banh | Menu bánh |
| `ChuyenNha()` | GET | /TrangChu/ChuyenNha | Bài viết |
| `CuaHang()` | GET | /TrangChu/CuaHang | Danh sách cửa hàng |

**Logic - Index():**

```csharp
public IActionResult Index()
{
    var hotCategoryNames = new[] { "BEST MENU", "HOT DRINK", "SIGNATURE" };
    
    var hotProducts = _context.Products
        .Include(p => p.Cate)
        .Include(p => p.ProductConditions)
        .Where(p => !p.Remove && p.Cate != null && !p.Cate.Remove)
        .Where(p => hotCategoryNames.Contains(p.Cate.TenLoaiSanPham))
        .OrderBy(...)
        .Take(4)
        .ToList();

    // Fallback nếu không đủ sản phẩm hot
    if (hotProducts.Count < 4)
    {
        var fallbackProducts = _context.Products
            .Where(p => !p.Remove && !selectedIds.Contains(p.ProductId))
            .OrderBy(...)
            .Take(4 - hotProducts.Count)
            .ToList();
        hotProducts.AddRange(fallbackProducts);
    }

    return View(hotProducts);
}
```

---

### 3.3.4 ProductsController

**File:** `Controllers/ProductsController.cs`

**Mô tả:** Quản lý sản phẩm (CRUD)

**Attributes:**
- `[AdminSessionAuthorize]`
- `[RoleAuthorize("admin", "manager", "staff")]`

#### Methods:

| Method | HTTP | Route | Role | Mô Tả |
|--------|------|-------|------|-------|
| `Index()` | GET | /Products | All | Danh sách sản phẩm |
| `Details(id)` | GET | /Products/Details/Id | All | Chi tiết sản phẩm |
| `Create()` | GET | /Products/Create | Admin, Manager | Form tạo sản phẩm |
| `Create(product)` | POST | /Products/Create | Admin, Manager | Xử lý tạo sản phẩm |
| `Edit(id)` | GET | /Products/Edit/Id | Admin, Manager | Form sửa sản phẩm |
| `Edit(id, product)` | POST | /Products/Edit/Id | Admin, Manager | Xử lý sửa sản phẩm |
| `EditFromModal(id, product)` | POST | /Products/EditFromModal | Admin, Manager | Sửa từ modal (AJAX) |
| `Delete(id)` | GET | /Products/Delete/Id | Admin | Form xóa |
| `DeleteConfirmed(id)` | POST | /Products/Delete/Id | Admin | Xử lý xóa (soft delete) |

**Logic - Create():**

```csharp
[HttpPost]
public async Task<IActionResult> Create(Product product, IFormFile FileInterface)
{
    if (ModelState.IsValid)
    {
        if (FileInterface != null && FileInterface.Length > 0)
        {
            // Upload ảnh
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images");
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(FileInterface.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await FileInterface.CopyToAsync(fileStream);
            }

            product.PathPhoto = "/assets/images/" + uniqueFileName;
        }

        _context.Add(product);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    return View(product);
}
```

**Logic - DeleteConfirmed():**

```csharp
[HttpPost, ActionName("Delete")]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var product = await _context.Products.FindAsync(id);
    
    if (product != null)
    {
        product.Remove = true;  // Soft delete
    }

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
```

---

### 3.3.5 DonHangsController

**File:** `Controllers/DonHangsController.cs`

**Mô tả:** Quản lý đơn hàng, đặc biệt là đơn hàng online/giao hàng

#### Methods:

| Method | HTTP | Route | Mô Tả |
|--------|------|-------|-------|
| `Index()` | GET | /DonHangs | Danh sách đơn hàng online |
| `Details(id)` | GET | /DonHangs/Details/Id | Chi tiết đơn |
| `DetailsData(id)` | GET | /DonHangs/DetailsData/Id | Dữ liệu chi tiết (AJAX) |
| `DetailsDataV2(id)` | GET | /DonHangs/DetailsDataV2/Id | Dữ liệu chi tiết v2 |
| `UpdateDeliveryStatus(id, status)` | POST | /DonHangs/UpdateDeliveryStatus | Cập nhật trạng thái giao hàng |
| `Create()` | GET | /DonHangs/Create | Form tạo đơn |
| `Create(donHang)` | POST | /DonHangs/Create | Xử lý tạo đơn |
| `Edit(id)` | GET | /DonHangs/Edit/Id | Form sửa đơn |
| `Edit(id, donHang)` | POST | /DonHangs/Edit/Id | Xử lý sửa đơn |
| `Delete(id)` | GET | /DonHangs/Delete/Id | Form xóa |
| `DeleteConfirmed(id)` | POST | /DonHangs/Delete/Id | Xóa mềm đơn |

**Logic - UpdateDeliveryStatus():**

```csharp
[HttpPost]
public async Task<IActionResult> UpdateDeliveryStatus(int id, string status)
{
    // Validate status
    if (!OnlineOrderMetadata.IsValidStatus(status) || status == OnlineOrderMetadata.StatusCart)
    {
        return BadRequest(new { message = "Trạng thái giao hàng không hợp lệ." });
    }

    var donHang = await _context.DonHangs
        .Include(o => o.OnlineOrderInfo)
        .FirstOrDefaultAsync(o => o.DhId == id && !o.Remove);

    var metadata = OnlineOrderMetadata.TryParse(donHang.GhiChu);
    if (metadata == null)
    {
        return BadRequest(new { message = "Đây không phải đơn hàng online." });
    }

    // Cập nhật trạng thái
    var oldStatus = donHang.OnlineOrderInfo?.TrangThaiGiaoHang ?? metadata.DeliveryStatus;
    metadata.DeliveryStatus = OnlineOrderMetadata.NormalizeStatus(status);
    donHang.GhiChu = metadata.ToJson();
    
    // Cập nhật OnlineOrderInfo
    if (donHang.OnlineOrderInfo == null)
    {
        donHang.OnlineOrderInfo = new OnlineOrderInfo { DhId = donHang.DhId };
        _context.OnlineOrderInfos.Add(donHang.OnlineOrderInfo);
    }

    donHang.OnlineOrderInfo.TrangThaiGiaoHang = metadata.DeliveryStatus;
    donHang.OnlineOrderInfo.NgayCapNhat = DateTime.Now;

    // Ghi nhận lịch sử
    _context.OnlineOrderStatusHistories.Add(new OnlineOrderStatusHistory
    {
        DhId = donHang.DhId,
        TrangThaiCu = oldStatus,
        TrangThaiMoi = metadata.DeliveryStatus,
        NvId = HttpContext.Session.GetInt32("NhanVienId"),
        CreatedAt = DateTime.Now
    });

    await _context.SaveChangesAsync();
    
    // Gửi thông báo real-time
    await _hubContext.Clients.All.SendAsync("OnlineOrderStatusUpdated", id, metadata.DeliveryStatus);

    return Json(new { statusKey, statusLabel, statusCssClass });
}
```

---

### 3.3.6 AdminBaiVietChuyenNhasController

**File:** `Controllers/AdminBaiVietChuyenNhasController.cs`

**Mô tả:** Quản lý bài viết chuyên nhà (blog)

#### Methods:

| Method | HTTP | Route | Mô Tả |
|--------|------|-------|-------|
| `Index()` | GET | /AdminBaiVietChuyenNhas | Danh sách bài viết |
| `Create(form)` | POST | /AdminBaiVietChuyenNhas/Create | Tạo bài viết |
| `Edit(id)` | GET | /AdminBaiVietChuyenNhas/Edit/Id | Sửa bài viết |
| `Edit(id, form)` | POST | /AdminBaiVietChuyenNhas/Edit/Id | Xử lý sửa |
| `Delete(id)` | POST | /AdminBaiVietChuyenNhas/Delete/Id | Xóa bài viết |

---

## 3.4 ViewModels

### ViewModelCart

**File:** `ViewModels/ViewModelCart.cs`

**Methods:**

| Method | Mô Tả |
|--------|-------|
| `CTHD_PctByDh(DhId)` | Lấy chi tiết hóa đơn theo đơn hàng |
| `TongtienById(DhId)` | Tính tổng tiền đơn hàng |
| `HasOrderItems(DhId)` | Kiểm tra đơn hàng có món không |
| `SubmittedDineInItems(banId, guestId)` | Lấy món đã submit dine-in |
| `BuildCheckoutForm(DhId, customerId)` | Xây dựng form thanh toán |

---

### ViewModelMenu

**File:** `ViewModels/ViewModelMenu.cs`

**Methods:**

| Method | Mô Tả |
|--------|-------|
| `CountProductDetail(DhId)` | Đếm số món trong đơn |
| `CountProductDetail(DhId, ProductId)` | Đếm số lượng món cụ thể |
| `CountSubmittedDineInProduct(banId, guestId, productId)` | Đếm món đã submit |
| `Categories()` | Lấy danh sách danh mục |
| `FindProductByCate(cateId)` | Tìm sản phẩm theo danh mục |
| `ProductsBySearch(txtsearch)` | Tìm kiếm sản phẩm |

---

## 3.5 Extension Methods

### HttpContextExtensions

**File:** `Extensions/HttpContextExtensions.cs`

**Methods:**

| Method | Mô Tả |
|--------|-------|
| `GetUserRole()` | Lấy role của user hiện tại |
| `GetUserName()` | Lấy tên user |
| `GetUserId()` | Lấy ID user |
| `HasRole(role)` | Kiểm tra user có role không |
| `HasAnyRole(params roles[])` | Kiểm tra user có bất kỳ role nào |
| `CanAccessModule(module)` | Kiểm tra quyền truy cập module |
| `CanCreate(module)` | Kiểm tra quyền tạo |
| `CanEdit(module)` | Kiểm tra quyền sửa |
| `CanDelete(module)` | Kiểm tra quyền xóa |

---

*Tài liệu tiếp tục ở phần 4...*