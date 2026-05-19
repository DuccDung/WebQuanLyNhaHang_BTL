# TÀI LIỆU KỸ THUẬT - HỆ THỐNG QUẢN LÝ NHÀ HÀNG CLOUDY CAFÉ

## 1. Tổng Quan Dự Án

### 1.1 Giới Thiệu

Đây là hệ thống quản lý nhà hàng được phát triển dựa trên framework ASP.NET Core 8.0 với kiến trúc MVC (Model-View-Controller). Hệ thống cung cấp các chức năng quản lý toàn diện cho hoạt động của một chuỗi nhà hàng/cafe, bao gồm quản lý sản phẩm, đơn hàng, khách hàng, nhân viên, cửa hàng và nội dung truyền thông.

**Tên dự án:** WebQuanLyNhaHang (Cloudy Café Restaurant Management System)

**Phiên bản công nghệ:** .NET 8.0 (ASP.NET Core MVC)

**Ngôn ngữ lập trình:** C#

**Cơ sở dữ liệu:** Microsoft SQL Server

---

### 1.2 Kiến Trúc Hệ Thống

```
┌─────────────────────────────────────────────────────────────────┐
│                        PRESENTATION LAYER                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ Razor Views  │  │ ViewComponents │  │   Layouts    │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
└─────────────────────────────────────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                       CONTROLLER LAYER                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ Admin Ctrl   │  │ Home Ctrl    │  │ Other Ctrl   │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
└─────────────────────────────────────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    BUSINESS LOGIC LAYER                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  ViewModels  │  │   Filters    │  │  Extensions  │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
└─────────────────────────────────────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      DATA ACCESS LAYER                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  Entities    │  │ EF Core DB   │  │  Hubs (RTE)  │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
└─────────────────────────────────────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      DATABASE LAYER                              │
│                    SQL Server Database                           │
└─────────────────────────────────────────────────────────────────┘
```

---

### 1.3 Cấu Trúc Thư Mục

```
WebQuanLyNhaHang/
├── Controllers/              # Xử lý requests và business logic
│   ├── AdminController.cs           # Quản trị hệ thống
│   ├── AdminBaiVietChuyenNhasController.cs  # Quản lý bài viết
│   ├── AdminCuaHangsController.cs   # Quản lý cửa hàng
│   ├── ChiTietHoaDonsController.cs  # Chi tiết hóa đơn
│   ├── DonHangsController.cs        # Đơn hàng
│   ├── HomeController.cs           # Trang chủ khách hàng
│   ├── KhachHangsController.cs      # Khách hàng
│   ├── NhanViensController.cs       # Nhân viên
│   ├── ProductsController.cs        # Sản phẩm
│   └── TrangChuController.cs        # Trang chủ công khai
├── Models/                   # Entity classes và ViewModels
│   ├── QlnhaHangBtlContext.cs       # EF Core DbContext
│   ├── KhachHang.cs                 # Thực thể khách hàng
│   ├── NhanVien.cs                  # Thực thể nhân viên
│   ├── DonHang.cs                   # Thực thể đơn hàng
│   ├── Product.cs                   # Thực thể sản phẩm
│   ├── Category.cs                  # Thực thể danh mục
│   ├── Ban.cs                       # Thực thể bàn ăn
│   ├── CuaHang.cs                   # Thực thể cửa hàng
│   ├── OnlineOrderInfo.cs           # Thông tin đơn hàng online
│   ├── OnlineOrderMetadata.cs       # Metadata cho đơn online
│   ├── OnlineOrderStatusHistory.cs  # Lịch sử trạng thái
│   ├── ProductConditions.cs         # Điều kiện sản phẩm
│   ├── BaiVietChuyenNha.cs          # Bài viết chuyên nhà
│   ├── HoaDonNhap.cs                # Hóa đơn nhập
│   ├── ChiTietHoaDonNhap.cs         # Chi tiết hóa đơn nhập
│   ├── NguyenLieu.cs                # Nguyên liệu
│   ├── CongThuc.cs                  # Công thức chế biến
│   ├── NhaCungCap.cs                # Nhà cung cấp
│   ├── KhuyenMai.cs                 # Khuyến mãi
│   ├── NgayCong.cs                  # Ngày công
│   ├── PhanQuyen.cs                 # Phân quyền
│   ├── NvPq.cs                      # Nhân viên - Phân quyền
│   ├── NvNc.cs                      # Nhân viên - Ngày công
│   ├── Thuong.cs                    # Thưởng
│   └── EntityRemoveFlags.cs         # Soft delete extension
├── Views/                    # Razor views
│   ├── Admin/                  # Views quản trị
│   ├── Home/                   # Views khách hàng
│   ├── Products/               # Views sản phẩm
│   ├── DonHangs/               # Views đơn hàng
│   ├── KhachHangs/             # Views khách hàng
│   ├── NhanViens/              # Views nhân viên
│   ├── Shared/                 # Shared components
│   └── TrangChu/               # Views trang chủ
├── ViewModels/               # View models
│   ├── AdminDashboardViewModel.cs
│   ├── ViewModelCart.cs
│   ├── ViewModelMenu.cs
│   ├── ViewModelBan.cs
│   ├── ViewModelGetFormBuy.cs
│   ├── ProductsIndexViewModel.cs
│   ├── CustomersIndexViewModel.cs
│   ├── EmployeesIndexViewModel.cs
│   └── OrdersIndexViewModel.cs
├── Filters/                  # Custom filters
│   ├── RoleAuthorizeAttribute.cs    # Phân quyền theo role
│   └── AdminSessionAuthorizeAttribute.cs  # Xác thực admin
├── Extensions/               # Extension methods
│   └── HttpContextExtensions.cs     # Extensions cho HttpContext
├── Hubs/                     # SignalR hubs
│   └── ChatHub.cs                   # Real-time messaging
├── wwwroot/                  # Static assets
│   ├── asset/
│   │   ├── css/             # Stylesheets
│   │   ├── js/              # JavaScript files
│   │   ├── font/            # Font files
│   │   └── images/          # Images
│   └── assets/              # Additional static files
└── Database/                 # Database scripts
    └── QLNhaHang_BTL_ModelSync.sql  # Database schema
```

---

### 1.4 Công Nghệ Sử Dụng

| Công Nghệ | Phiên Bản | Mục Đích |
|-----------|-----------|----------|
| ASP.NET Core | 8.0 | Web framework |
| Entity Framework Core | 8.0.10 | ORM |
| SQL Server | 2019+ | Database |
| SignalR | 1.1.0 | Real-time communication |
| Bootstrap | 4.x | UI framework |
| jQuery | 3.x | DOM manipulation |
| FontAwesome | 6.5.2 | Icon library |

---

### 1.5 Tính Năng Chính

#### 1.5.1 Quản Lý Sản Phẩm
- Quản lý danh mục sản phẩm
- Thêm/sửa/xóa sản phẩm
- Quản lý hình ảnh sản phẩm
- Phân loại sản phẩm theo danh mục
- Quản lý điều kiện sản phẩm (size, ghi chú)

#### 1.5.2 Quản Lý Đơn Hàng
- Đơn hàng tại quán (Dine-in)
- Đơn hàng mang về (Take-away)
- Đơn hàng giao hàng (Delivery)
- Theo dõi trạng thái đơn hàng
- Lịch sử thay đổi trạng thái

#### 1.5.3 Quản Lý Khách Hàng
- Đăng ký/đăng nhập khách hàng
- Thông tin khách hàng
- Lịch sử đặt hàng
- Cookie và Session management

#### 1.5.4 Quản Lý Nhân Viên
- Quản lý thông tin nhân viên
- Phân quyền nhân viên
- Quản lý ngày công
- Quản lý thưởng

#### 1.5.5 Quản Lý Cửa Hàng
- Quản lý thông tin cửa hàng
- Địa chỉ và vị trí (Google Maps)
- Giờ mở cửa
- Hiển thị/ẩn cửa hàng

#### 1.5.6 Dashboard Thống Kê
- Doanh thu theo thời gian
- Số lượng đơn hàng
- Khách hàng hoạt động
- Sản phẩm bán chạy
- So sánh tháng hiện tại và tháng trước

#### 1.5.7 Real-time Features (SignalR)
- Thông báo đơn hàng mới
- Cập nhật giỏ hàng realtime
- Thông báo trạng thái đơn hàng
- Gọi nhân viên tại quán
- Yêu cầu thanh toán

---

## 2. Cấu Hình Hệ Thống

### 2.1 Connection String

File: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "QlnhaHangBtlContext": "Data Source=ADMIN-PC\\MSSQLSERVER1;Initial Catalog=QLNhaHang_BTL;User ID=sa;Password=xxx;Trust Server Certificate=True"
  }
}
```

### 2.2 Session Configuration

```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

### 2.3 SignalR Configuration

```csharp
builder.Services.AddSignalR();
```

---

## 3. Quy Trình Nghiệp Vụ

### 3.1 Quy Trình Đặt Hàng Tại Quán (Dine-in)

```
1. Khách hàng quét mã QR bàn
   ↓
2. Nhập tên khách hàng
   ↓
3. Hệ thống tạo session + đơn hàng tạm
   ↓
4. Khách chọn món từ menu
   ↓
5. Thêm vào giỏ hàng (SignalR update)
   ↓
6. Xem giỏ hàng + Xác nhận
   ↓
7. Gửi yêu cầu phục vụ
   ↓
8. Nhân viên nhận thông báo (SignalR)
   ↓
9. Chuẩn bị món + Giao món
   ↓
10. Khách gọi thanh toán
    ↓
11. Nhân viên xử lý thanh toán
    ↓
12. Hoàn tất đơn hàng
```

### 3.2 Quy Trình Đặt Hàng Online

```
1. Khách đăng nhập
   ↓
2. Chọn món từ menu công khai
   ↓
3. Thêm vào giỏ hàng online
   ↓
4. Điền thông tin giao hàng
   ↓
5. Chọn phương thức thanh toán
   ↓
6. Xác nhận đơn hàng
   ↓
7. Hệ thống tạo đơn + cập nhật trạng thái
   ↓
8. Nhân viên nhận đơn (SignalR)
   ↓
9. Cập nhật trạng thái: Chờ → Đang chuẩn bị → Đang giao → Đã giao
   ↓
10. Hoàn tất
```

---

*Tài liệu tiếp tục ở các phần sau...*