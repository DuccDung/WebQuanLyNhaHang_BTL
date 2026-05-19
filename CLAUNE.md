# Web Quản Lý Nhà Hàng - Tài Liệu Dự Án

## Tổng Quan Dự Án

**Tên dự án:** WebQuanLyNhaHang (Hệ thống quản lý nhà hàng)

**Công nghệ sử dụng:**
- Framework: ASP.NET Core 8.0 (MVC Pattern)
- Database: SQL Server với Entity Framework Core 8.0
- Frontend: Bootstrap 5, jQuery, Owl Carousel, Font Awesome
- Real-time: SignalR cho thông báo thời gian thực

**Mô tả:** Hệ thống quản lý nhà hàng toàn diện với các tính năng quản lý đơn hàng, sản phẩm, khách hàng, nhân viên, bàn ăn, và đặt hàng online.

---

## Cấu Trúc Dự Án

```
WebQuanLyNhaHang_BTL/
├── WebQuanLyNhaHang/
│   ├── Controllers/          # Bộ điều khiển (Controllers)
│   │   ├── TrangChuController.cs
│   │   ├── AdminController.cs
│   │   ├── ProductsController.cs
│   │   ├── DonHangsController.cs
│   │   ├── NhanViensController.cs
│   │   ├── KhachHangsController.cs
│   │   ├── ChiTietHoaDonsController.cs
│   │   ├── AdminCuaHangsController.cs
│   │   ├── AdminBaiVietChuyenNhasController.cs
│   │   ├── ChartDataController.cs
│   │   └── HomeController.cs
│   │
│   ├── Models/               # Mô hình dữ liệu (EF Core Entities)
│   │   ├── QlnhaHangBtlContext.cs
│   │   ├── Product.cs
│   │   ├── Category.cs
│   │   ├── DonHang.cs
│   │   ├── KhachHang.cs
│   │   ├── NhanVien.cs
│   │   ├── Ban.cs
│   │   ├── ChiTietHoaDon.cs
│   │   ├── NguyenLieu.cs
│   │   ├── CongThuc.cs
│   │   ├── NhaCungCap.cs
│   │   ├── HoaDonNhap.cs
│   │   ├── ChiTietHoaDonNhap.cs
│   │   ├── KhuyenMai.cs
│   │   ├── PhanQuyen.cs
│   │   ├── NvPq.cs
│   │   ├── NgayCong.cs
│   │   ├── NvNc.cs
│   │   ├── Thuong.cs
│   │   ├── CuaHang.cs
│   │   ├── BaiVietChuyenNha.cs
│   │   ├── OnlineOrderInfo.cs
│   │   ├── OnlineOrderStatusHistory.cs
│   │   ├── ProductConditions.cs
│   │   ├── OnlineOrderMetadata.cs
│   │   └── ErrorViewModel.cs
│   │
│   ├── Views/                # View (Razor Pages)
│   │   ├── TrangChu/
│   │   ├── Admin/
│   │   ├── Products/
│   │   ├── DonHangs/
│   │   ├── NhanViens/
│   │   ├── KhachHangs/
│   │   ├── ChiTietHoaDons/
│   │   ├── ProductConditions/
│   │   ├── Shared/
│   │   └── _ViewImports.cshtml
│   │
│   ├── ViewModel/            # View Models
│   │   ├── ProductsIndexViewModel.cs
│   │   ├── DonHangDonHangsRowViewModel.cs
│   │   ├── ChartData.cs
│   │   ├── CategoryProduct.cs
│   │   ├── BanDonHang.cs
│   │   ├── DoanhThu_LoiNhan.cs
│   │   ├── ProductPCTCondition.cs
│   │   ├── QuantitySelector.cs
│   │   ├── NvPq.cs
│   │   ├── AdminDashboardViewModel.cs
│   │   ├── AdminContentViewModels.cs
│   │   ├── CTDH_Product.cs
│   │   ├── ViewModelBan.cs
│   │   ├── ViewModelCart.cs
│   │   ├── ViewModelMenu.cs
│   │   ├── ViewModelGetFormBuy.cs
│   │   ├── ViewModelProductDetail.cs
│   │   ├── OrdersIndexViewModel.cs
│   │   ├── OnlineCheckoutForm.cs
│   │   ├── OnlineOrderHistoryViewModel.cs
│   │   ├── CustomersIndexViewModel.cs
│   │   ├── EmployeesIndexViewModel.cs
│   │   └── TrangChuMenuPageViewModel.cs
│   │
│   ├── Filters/              # Bộ lọc (Filters)
│   │   ├── AdminSessionAuthorizeAttribute.cs
│   │   └── RoleAuthorizeAttribute.cs
│   │
│   ├── Extensions/           # Extension Methods
│   │   └── HttpContextExtensions.cs
│   │
│   ├── Hubs/                 # SignalR Hubs
│   │   └── ChatHub.cs
│   │
│   ├── wwwroot/              # Tài nguyên tĩnh
│   │   ├── asset/
│   │   │   ├── css/
│   │   │   ├── images/
│   │   │   ├── font/
│   │   │   └── js/
│   │   └── lib/
│   │
│   ├── Database/             # SQL Scripts
│   │   └── QLNhaHang_BTL_ModelSync.sql
│   │
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Program.cs
│   └── WebQuanLyNhaHang.csproj
```

---

## Cơ Sở Dữ Liệu

### Entity Framework Core Context
**File:** `QlnhaHangBtlContext.cs`

**Connection String:** SQL Server - `Data Source=ADMIN-PC\MSSQLSERVER1;Initial Catalog=QLNhaHang_BTL`

### Các Bảng Chính (Entities) - 22+ bảng

#### 1. Product (Sản phẩm)
- ProductId (int, PK)
- CateId (int, FK - Category)
- TenSanPham (nvarchar)
- MoTa (nvarchar)
- GiaTien (money)
- PathPhoto (nvarchar)
- Remove (bit, default: false)

#### 2. Category (Danh mục sản phẩm)
- CateId (int, PK)
- TenLoaiSanPham (nvarchar) - BEST MENU, HOT DRINK, SIGNATURE, CAKE, TEA, JUICE
- MoTa (nvarchar)

#### 3. DonHang (Đơn hàng)
- DhId (int, PK)
- KhId (int, FK - KhachHang)
- BanId (int, FK - Ban)
- KmId (int, FK - KhuyenMai)
- NvId (int, FK - NhanVien)
- GioVao (datetime)
- GioRa (datetime)
- TongTien (money)
- GhiChu (nvarchar) - JSON metadata cho đơn online
- TrangThai (bit)
- VanChuyen (bit)

#### 4. ChiTietHoaDon (Chi tiết hóa đơn)
- CthdId (int, PK)
- DhId (int, FK - DonHang)
- ProductId (int, FK - Product)
- SoLuong (int)
- ThanhTien (money)
- GiamGia (int)
- Ghichu (nvarchar)

#### 5. KhachHang (Khách hàng)
- KhId (int, PK)
- TenKhachHang (nvarchar)
- DiaChi (nvarchar)
- SoDienThoai (nvarchar)
- TaiKhoan (nvarchar)
- MatKhau (nvarchar)
- PathPhoto (nvarchar)
- Remove (bit)

#### 6. NhanVien (Nhân viên)
- NvId (int, PK)
- TenNhanVien (nvarchar)
- NgaySinh (date)
- DiaChi (nvarchar)
- HeSoLuong (decimal)
- TaiKhoan (nvarchar)
- MatKhau (nvarchar)
- GioiTinh (nvarchar)
- SoDienThoai (nvarchar)
- PathPhoto (nvarchar)
- Remove (bit)

#### 7. Ban (Bàn ăn)
- BanId (int, PK)
- SoChoNgoi (int)
- GhiChu (nvarchar)

#### 8. NguyenLieu (Nguyên liệu)
- NlId (int, PK)
- TenNguyenLieu (nvarchar)
- GiaTien (money)

#### 9. CongThuc (Công thức pha chế)
- CtId (int, PK)
- ProductId (int, FK - Product)
- NlId (int, FK - NguyenLieu)
- SoLuong (decimal)
- GhiChu (nvarchar)

#### 10. HoaDonNhap (Hóa đơn nhập)
- HdnId (int, PK)
- NccId (int, FK - NhaCungCap)
- NvId (int, FK - NhanVien)
- NgayLapHoaDon (datetime)
- NgayNhanHang (datetime)
- TongSoTien (money)

#### 11. NhaCungCap (Nhà cung cấp)
- NccId (int, PK)
- TenNhaCungCap (nvarchar)
- DiaChi (nvarchar)
- Phone (nvarchar)

#### 12. KhuyenMai (Khuyến mãi)
- KmId (int, PK)
- TenKhuyenMai (nvarchar)
- GiamGia (decimal)

#### 13. PhanQuyen (Phân quyền)
- PqId (int, PK)
- TenQuyen (nvarchar) - admin, manager, staff

#### 14. CuaHang (Chi nhánh cửa hàng)
- CuaHangId (int, PK)
- TenCuaHang (nvarchar)
- DiaChi (nvarchar)
- TinhThanh, QuanHuyen, PhuongXa (nvarchar)
- SoDienThoai (nvarchar)
- GioMoCua, GioDongCua (time)
- Latitude, Longitude (decimal) - Tọa độ GPS
- GoogleMapUrl (nvarchar)
- SapXep (int)
- HienThi, Remove (bit)
- CreatedAt, UpdatedAt (datetime)

#### 15. BaiVietChuyenNha (Bài viết)
- BaiVietId (int, PK)
- TieuDe, Slug, TomTat, NoiDung (nvarchar)
- PathPhoto, AltText (nvarchar)
- TacGia (nvarchar)
- NgayDang (datetime)
- NoiBat, HienThi, Remove (bit)
- SapXep (int)
- CreatedAt, UpdatedAt (datetime)

#### 16. OnlineOrderInfo (Thông tin đặt hàng online)
- OnlineOrderInfoId (int, PK)
- DhId (int, FK - DonHang, unique)
- CuaHangId (int, FK - CuaHang)
- TrangThaiGiaoHang (nvarchar) - cart, pending, preparing, shipping, delivered, cancelled
- NguoiNhan, SoDienThoai (nvarchar)
- DiaChi, TinhThanh, QuanHuyen, PhuongXa (nvarchar)
- GhiChuGiaoHang (nvarchar)
- PhiGiaoHang (money)
- PhuongThucThanhToan, TrangThaiThanhToan (nvarchar)
- NgayDat, NgayCapNhat (datetime)

#### 17. OnlineOrderStatusHistory (Lịch sử trạng thái đơn online)
- HistoryId (int, PK)
- DhId (int, FK - DonHang)
- NvId (int, FK - NhanVien)
- TrangThaiCu, TrangThaiMoi (nvarchar)
- CreatedAt (datetime)

#### 18. ProductConditions (Điều kiện/Option sản phẩm)
- ProductConditionId (int, PK)
- ProductId (int, FK - Product)
- Condition (nvarchar) - ví dụ: "Đá", "Nóng", "Ít đường"

#### 19. NgayCong (Ngày công)
- NcId (int, PK)
- NgayCong1 (date)
- CaLam (nvarchar)

#### 20. NvNc (Nhân viên - Ngày công)
- NvncId (int, PK)
- NvId (int, FK - NhanVien)
- NcId (int, FK - NgayCong)

#### 21. NvPq (Nhân viên - Phân quyền)
- NvPqId (int, PK)
- NvId (int, FK - NhanVien)
- PqId (int, FK - PhanQuyen)
- MoTa (nvarchar)

#### 22. Thuong (Thưởng)
- TId (int, PK)
- NvId (int, FK - NhanVien)
- TenThuong (nvarchar)
- TienThuong (money)

---

## Hệ Thống Phân Quyền

### 3 Vai Trò (Roles)

| Role | Key | Quyền Hạn |
|------|-----|-----------|
| **Admin** | `admin` | Full access - quản lý tất cả modules + nhân viên + phân quyền |
| **Manager** | `manager` | Quản lý vận hành - không được quản lý nhân viên |
| **Staff** | `staff` | Nhân viên phục vụ - chỉ xem và tạo đơn |

### Tài Khoản Test

| Role | Tài Khoản | Mật Khẩu | Mục Đích Test |
|------|-----------|----------|---------------|
| Admin | `admin` | `admin123` | Full access - kiểm tra tất cả chức năng |
| Manager | `manager` | `manager123` | Quản lý vận hành - không được quản lý NV |
| Staff | `staff` | `staff123` | Nhân viên phục vụ - chỉ xem và tạo đơn |

### Ma Trận Quyền Hạn

| Module | View | Create/Edit | Delete | Special |
|--------|------|-------------|--------|---------|
| Dashboard | admin, manager | - | - | - |
| Sản phẩm | all | admin, manager | admin | - |
| Đơn hàng | all | admin, manager | admin | - |
| Chi tiết đơn | all | admin, manager | admin | - |
| Khách hàng | all | admin, manager | admin | - |
| Nhân viên | admin, manager | admin | admin | Chỉ admin được phân quyền |
| Phân quyền | admin | admin | admin | Admin only |
| Cửa hàng | admin, manager | admin, manager | admin | - |
| Chuyện nhà | admin, manager | admin, manager | admin | - |
| Đơn tại quán | admin, manager | admin, manager | - | - |

---

## Các Controller Chính

### 1. TrangChuController
**Chức năng:** Trang chủ và menu công khai
- `Index()`: Hiển thị sản phẩm nổi bật (HOT MENU, HOT DRINK, SIGNATURE)
- `Menu()`: Hiển thị toàn bộ menu
- `DoUong()`: Menu đồ uống
- `Banh()`: Menu bánh
- `ChuyenNha()`: Bài viết blog
- `CuaHang()`: Danh sách cửa hàng

### 2. AdminController
**Chức năng:** Dashboard quản trị
- `Index()`: Dashboard tổng quan với doanh thu, đơn hàng, thống kê
- `Login()`: Đăng nhập admin (session-based)
- `Logout()`: Đăng xuất
- `Ban()`: Quản lý bàn ăn
- `GetFormBuy()`: Form tạo đơn hàng cho bàn
- `LatestOrderNotification()`: Thông báo đơn hàng mới (SignalR)
- `ProcessPayment()`: Xử lý thanh toán bàn

**SignalR Events:**
- `DineInServiceRequest`: Yêu cầu dịch vụ
- `DineInPaymentCompleted`: Hoàn tất thanh toán
- `OderSuccess`: Đặt hàng thành công
- `ProductDeleted`: Xóa sản phẩm

### 3. ProductsController
**Chức năng:** Quản lý sản phẩm (CRUD)
- `Index()`: Danh sách sản phẩm
- `Create()`: Thêm sản phẩm mới
- `Edit()`: Chỉnh sửa sản phẩm
- `Delete()`: Xóa mềm sản phẩm
- `Details()`: Xem chi tiết
- **Upload ảnh sản phẩm** vào `/wwwroot/assets/images/`

### 4. DonHangsController
**Chức năng:** Quản lý đơn hàng
- `Index()`: Danh sách đơn hàng (tập trung đơn giao hàng)
- `Details() / DetailsData()`: Chi tiết đơn hàng
- `DetailsDataV2()`: API trả JSON chi tiết
- `UpdateDeliveryStatus()`: Cập nhật trạng thái giao hàng
- **Trạng thái đơn online:** cart → pending → preparing → shipping → delivered/cancelled

### 5. NhanViensController
**Chức năng:** Quản lý nhân viên
- `Index()`: Danh sách nhân viên
- `Create()`: Thêm nhân viên
- `Edit()`: Chỉnh sửa nhân viên
- `Delete()`: Xóa mềm nhân viên
- `UpdateFromModal()`: Cập nhật từ modal
- `SyncEmployeeRoleAsync()`: Đồng bộ quyền nhân viên
- **Phân quyền:** Quản Lý, Nhân Viên Bếp, Phục Vụ

### 6. KhachHangsController
**Chức năng:** Quản lý khách hàng (CRUD)

### 7. ChiTietHoaDonsController
**Chức năng:** Quản lý chi tiết hóa đơn

### 8. AdminCuaHangsController
**Chức năng:** Quản lý chi nhánh cửa hàng

### 9. AdminBaiVietChuyenNhasController
**Chức năng:** Quản lý bài viết blog

### 10. ChartDataController
**Chức năng:** Dữ liệu biểu đồ thống kê

### 11. HomeController
**Chức năng:** Frontend home và cart

---

## View Components

### 1. CategoryViewComponent
**File:** `Views/Shared/Components/Category/`
- Hiển thị danh mục sản phẩm

### 2. ChartViewComponent
**File:** `Views/Shared/Components/Chart/`
- Hiển thị biểu đồ thống kê

### 3. AccLoggin / CustomerLogin
**File:** `Views/Shared/Components/`
- Component đăng nhập khách hàng

### 4. MenuItemEditSearchViewComponent
**File:** `Views/Shared/Components/MenuItemEditSearch/`
- Component chỉnh sửa menu với search

### 5. foodterCTHDViewComponent
**File:** `Views/Shared/Components/foodterCTHD/`
- Footer chi tiết hóa đơn

### 6. ProductConditionViewComponent
**File:** `Views/Shared/Components/ProductCondition/`
- Checkbox conditions cho sản phẩm

### 7. MenuAddSubViewComponent
**File:** `Views/Shared/Components/MenuAddSub/`
- Component thêm phụ món

---

## Auth & Security

### AdminSessionAuthorizeAttribute
**File:** `Filters/AdminSessionAuthorizeAttribute.cs`

**Chức năng:** Filter bảo vệ route admin bằng session
- Kiểm tra `NhanVienId` trong session
- Xác thực nhân viên còn hoạt động trong DB
- Tự động redirect về `/Admin/Login` khi hết phiên
- Trả về JSON error cho AJAX requests

**Session Keys:**
- `NhanVienId`: ID nhân viên
- `NhanVienName`: Tên nhân viên
- `NhanVienTaiKhoan`: Tài khoản đăng nhập

### RoleAuthorizeAttribute
**File:** `Filters/RoleAuthorizeAttribute.cs`

**Chức năng:** Custom authorization attribute cho role-based access
- Kiểm tra role từ session
- Redirect đến AccessDenied page khi không có quyền
- JSON response cho AJAX requests

### HttpContextExtensions
**File:** `Extensions/HttpContextExtensions.cs`

**Chức năng:** Extension methods cho role checking
- `GetUserRole(HttpContext)`: Lấy role của user hiện tại
- `HasRole(HttpContext, string)`: Kiểm tra role
- `IsAdmin(HttpContext)`: Kiểm tra admin

### Customer Session (Public)
- `CustomerID`: Session key cho khách hàng
- Cookie: `CloudyCafeCustomer` (encrypted)

---

## SignalR Real-time Features

**Hub:** `ChatHub`
**Path:** `/chatHub`

**Events:**
- `SendMessage(message)`: Gửi tin nhắn chung
- `NotifyDatabaseChange()`: Thông báo thay đổi DB
- `NotifyProductDeleted(productId)`: Xóa sản phẩm
- `NotifyOderSuccess()`: Đặt hàng thành công
- `NotifyDineInServiceRequest(request)`: Yêu cầu dịch vụ
- `NotifyDineInPaymentCompleted(payment)`: Thanh toán hoàn tất

---

## Online Order System

### OnlineOrderMetadata (JSON Helper Class)
**File:** `Models/OnlineOrderMetadata.cs`

**Trạng thái giao hàng:**
- `cart`: Giỏ hàng (chưa submit)
- `pending`: Chờ xác nhận
- `preparing`: Đang chuẩn bị
- `shipping`: Đang giao
- `delivered`: Đã giao
- `cancelled`: Đã hủy

### Metadata Format (JSON in DonHang.GhiChu)
```json
{
  "t": "online",
  "s": "preparing",
  "n": "Nguyễn Văn A",
  "p": "0901234567",
  "c": "TP.HCM",
  "d": "Quận 1",
  "w": "Phường Đa Kao",
  "a": "123 Đường Nguyễn Huệ",
  "note": "Giao giờ hành chính",
  "at": "2026-01-15T08:30:00"
}
```

### Display Status Mapping
| Status Key | Label | CSS Class |
|------------|-------|-----------|
| `cart` | Giỏ hàng | is-pending |
| `pending` | Chờ xác nhận | is-pending |
| `preparing` | Đang chuẩn bị | is-processing |
| `shipping` | Đang giao | is-shipping |
| `delivered` | Đã giao | is-completed |
| `cancelled` | Đã hủy | is-cancelled |

---

## Layout & Themes

### Layout Files
1. `_CloudyCafeLayout.cshtml`: Layout chính cho client
2. `_AdminSidebar.cshtml`: Sidebar admin
3. `_AdminContentLayout.cshtml`: Content admin
4. `Header_Left_Layout.cshtml`: Header trái
5. `Headerfooter_TrangChu.cshtml`: Header/footer trang chủ
6. `Head_Layout.cshtml`: Head layout

### CSS Files
- `base.css`: CSS cơ bản
- `home.css`: Trang chủ
- `responsive.css`: Responsive
- `HomeTrangChu.css`: Style trang chủ
- `menu_home.css`: Menu
- `loginhome.css`: Login
- `service.css`: Service
- `error.css`: Error
- `xoayvong.css`: Bánh xe quay

---

## Package Dependencies

**File:** `WebQuanLyNhaHang.csproj`

- `Microsoft.AspNetCore.SignalR` (1.1.0)
- `Microsoft.EntityFrameworkCore` (8.0.10)
- `Microsoft.EntityFrameworkCore.SqlServer` (8.0.10)
- `Microsoft.EntityFrameworkCore.Design` (8.0.10)
- `Microsoft.EntityFrameworkCore.Tools` (8.0.10)
- `Microsoft.VisualStudio.Web.CodeGeneration.Design` (8.0.6)

---

## Routing

**Default Route:** `{controller=TrangChu}/{action=Index}/{id?}`

**API Routes:**
- `/Admin/Login`
- `/Products/Create`
- `/DonHangs/UpdateDeliveryStatus/{id}`
- `/Admin/LatestOrderNotification`

---

## Environment Config

**Development:**
- `appsettings.Development.json`: Debug settings
- SQL Server: `ADMIN-PC\MSSQLSERVER1`
- Database: `QLNhaHang_BTL`
- User: `sa`

---

## Features Summary

### Khách hàng (Public)
- Xem menu sản phẩm theo danh mục
- Xem sản phẩm nổi bật
- Đặt hàng online
- Xem bài viết/blog
- Xem danh sách cửa hàng
- Đăng nhập/đăng ký

### Admin
- Dashboard thống kê doanh thu
- Quản lý đơn hàng (tại bàn, giao hàng)
- Quản lý sản phẩm + upload ảnh
- Quản lý danh mục
- Quản lý khách hàng
- Quản lý nhân viên + phân quyền
- Quản lý bàn ăn
- Quản lý khuyến mãi
- Quản lý nguyên liệu + công thức
- Quản lý hóa đơn nhập
- Quản lý nhà cung cấp
- Quản lý cửa hàng chi nhánh
- Quản lý bài viết
- Xem biểu đồ thống kê
- Xử lý thanh toán bàn
- Real-time notification đơn hàng

### Manager
- Quản lý vận hành (không được quản lý nhân viên)
- Tất cả chức năng admin trừ: tạo/sửa/xóa nhân viên, phân quyền

### Staff
- Xem thông tin cá nhân
- Xem lịch làm việc (ngày công)
- Xem quyền được phân bổ
- Chỉ xem menu và tạo đơn

---

## File Locations

### Quan trọng
- **Startup:** `Program.cs`
- **DB Context:** `Models/QlnhaHangBtlContext.cs`
- **Config:** `appsettings.json`
- **Layout chính:** `Views/Shared/_CloudyCafeLayout.cshtml`
- **Admin login:** `Views/Admin/Login.cshtml`
- **Admin dashboard:** `Views/Admin/Index.cshtml`
- **Trang chủ:** `Views/TrangChu/Index.cshtml`
- **Menu:** `Views/TrangChu/Menu.cshtml`

### Asset paths
- **CSS:** `/wwwroot/asset/css/`
- **Images:** `/wwwroot/asset/images/`
- **Font:** `/wwwroot/asset/font/`
- **JS:** `/wwwroot/asset/js/`

---

## Notes

- **Soft Delete:** Tất cả entities đều có field `Remove` (bit) thay vì xóa cứng
- **Vietnamese Culture:** Sử dụng `CultureInfo("vi-VN")` cho định dạng ngày/giá tiền
- **Session Timeout:** 30 phút
- **Image Upload:** UUID filename để tránh trùng lặp
- **JSON Metadata:** Đơn hàng online lưu metadata trong `GhiChu` field
- **Concurrency:** Sử dụng `DbUpdateConcurrencyException` để xử lý xung đột
- **Async/Await:** Sử dụng async cho tất cả DB operations

---

## SQL Seed Files

### data.sql
Script chứa toàn bộ schema database

### seed-users.sql
Script tạo 3 tài khoản test:
- `admin` / `admin123` - Admin (Full access)
- `manager` / `manager123` - Manager (Quản lý vận hành)
- `staff` / `staff123` - Staff (Nhân viên phục vụ)

---

## Liên Hệ

**Dự án:** Quản Lý Nhà Hàng - Bài Tập Lớn
**Công nghệ:** ASP.NET Core MVC 8.0 + SQL Server
**Ngày tạo tài liệu:** 2026-05-18