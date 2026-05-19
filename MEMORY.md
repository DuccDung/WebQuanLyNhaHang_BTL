# Memory - Web Quản Lý Nhà Hàng

## Thông Tin Dự Án

**Tên dự án:** WebQuanLyNhaHang - Hệ thống quản lý nhà hàng
**Framework:** ASP.NET Core 8.0 (MVC Pattern)
**Database:** SQL Server với Entity Framework Core 8.0
**Ngày bắt đầu:** 2026-01
**Ngày cập nhật cuối:** 2026-05-18

---

## Cấu Trúc Dự Án

### Thư Viện Chính
```
WebQuanLyNhaHang_BTL/
├── WebQuanLyNhaHang/          # Main project
│   ├── Controllers/           # 11 controllers
│   ├── Models/                # 22+ entities
│   ├── Views/                 # Razor views
│   ├── ViewModel/             # ViewModels
│   ├── Filters/               # Authorization filters
│   ├── Extensions/            # HttpContext extensions
│   ├── Hubs/                  # SignalR hubs
│   ├── Database/              # SQL scripts
│   └── wwwroot/               # Static assets
├── CLAUNE.md                  # Main documentation
├── Task.md                    # Task list
├── TESTING_GUIDE.md           # Testing guide
└── MEMORY.md                  # This file
```

---

## System Architecture

### Pattern: MVC (Model-View-Controller)
- **Models**: EF Core entities với 22+ tables
- **Views**: Razor Pages với Bootstrap 5
- **Controllers**: 11 controllers xử lý business logic

### Authentication: Session-based
- **Session Timeout**: 30 minutes
- **Session Keys**:
  - `NhanVienId`: Employee ID
  - `NhanVienName`: Employee name
  - `NhanVienTaiKhoan`: Username
  - `RoleKey`: Role (admin/manager/staff)

### Authorization: Role-based
- **Custom Filter**: `RoleAuthorizeAttribute`
- **Extension**: `HttpContextExtensions.GetUserRole()`
- **3 Roles**: admin, manager, staff

---

## Database Schema

### Core Tables (22+ entities)

#### Business Tables
1. **Product** - Sản phẩm
2. **Category** - Danh mục sản phẩm
3. **DonHang** - Đơn hàng
4. **ChiTietHoaDon** - Chi tiết hóa đơn
5. **KhachHang** - Khách hàng
6. **NhanVien** - Nhân viên
7. **Ban** - Bàn ăn
8. **NguyenLieu** - Nguyên liệu
9. **CongThuc** - Công thức pha chế
10. **KhuyenMai** - Khuyến mãi

#### Management Tables
11. **CuaHang** - Chi nhánh cửa hàng
12. **BaiVietChuyenNha** - Bài viết blog
13. **HoaDonNhap** - Hóa đơn nhập
14. **ChiTietHoaDonNhap** - Chi tiết hóa đơn nhập
15. **NhaCungCap** - Nhà cung cấp

#### Authorization Tables
16. **PhanQuyen** - Quyền (admin, manager, staff)
17. **NvPq** - Nhân viên - Phân quyền (mapping)

#### Online Order Tables
18. **OnlineOrderInfo** - Thông tin đơn hàng online
19. **OnlineOrderStatusHistory** - Lịch sử trạng thái
20. **ProductConditions** - Điều kiện sản phẩm

#### HR Tables
21. **NgayCong** - Ngày công
22. **NvNc** - Nhân viên - Ngày công
23. **Thuong** - Thưởng

### Soft Delete Pattern
- Tất cả entities có field: `Remove` (bit)
- Thay vì xóa cứng, set `Remove = true`

---

## Controllers Summary

| Controller | Chức Năng | Routes Chính |
|------------|-----------|--------------|
| TrangChuController | Trang chủ công khai | Index, Menu, DoUong, Banh, ChuyenNha, CuaHang |
| AdminController | Dashboard quản trị | Index, Login, Logout, Ban, ProcessPayment |
| ProductsController | Quản lý sản phẩm | Index, Create, Edit, Delete, Details |
| DonHangsController | Quản lý đơn hàng | Index, Details, UpdateDeliveryStatus |
| NhanViensController | Quản lý nhân viên | Index, Create, Edit, Delete, UpdateFromModal |
| KhachHangsController | Quản lý khách hàng | Index, Create, Edit, Delete |
| ChiTietHoaDonsController | Quản lý chi tiết đơn | Index, Create, Edit, Delete |
| AdminCuaHangsController | Quản lý cửa hàng | Index, Create, Edit, Delete |
| AdminBaiVietChuyenNhasController | Quản lý bài viết | Index, Create, Edit, Delete |
| ChartDataController | Thống kê biểu đồ | Index |
| HomeController | Frontend home | Index, Cart, Menu, ProductDetail |

---

## Permission Matrix

### Admin (Full Access)
- Tất cả modules: View, Create, Edit, Delete
- Quản lý nhân viên + phân quyền
- Quản lý tất cả dữ liệu

### Manager (Quản Lý Vận Hành)
- View: Tất cả modules
- Create/Edit: Sản phẩm, Đơn hàng, KH, Cửa hàng, Chuyện nhà
- Delete: KHÔNG được phép
- KHÔNG được quản lý nhân viên

### Staff (Nhân Viên Phục Vụ)
- View: Sản phẩm, Đơn hàng, ProductConditions
- Create: Đơn hàng
- Edit/Delete: KHÔNG được phép
- Access: Đơn tại quán (tạo/thanh toán)

---

## Online Order System

### Status Workflow
```
cart → pending → preparing → shipping → delivered
                                    ↓
                              cancelled
```

### Metadata JSON Format
```json
{
  "t": "online",
  "s": "preparing",
  "n": "Tên người nhận",
  "p": "0901234567",
  "c": "TP.HCM",
  "d": "Quận 1",
  "w": "Phường Đa Kao",
  "a": "123 Đường Nguyễn Huệ",
  "note": "Ghi chú giao hàng",
  "at": "2026-01-15T08:30:00"
}
```

### Key Classes
- `OnlineOrderMetadata`: JSON helper class
- `OnlineOrderInfo`: Entity lưu thông tin
- `OnlineOrderStatusHistory`: Entity lưu history

---

## SignalR Real-time Features

### Hub: ChatHub
**Path:** `/chatHub`

### Events
- `SendMessage(message)` - Gửi tin nhắn
- `NotifyDatabaseChange()` - Thông báo DB change
- `NotifyProductDeleted(productId)` - Xóa sản phẩm
- `NotifyOderSuccess()` - Đặt hàng thành công
- `NotifyDineInServiceRequest(request)` - Yêu cầu dịch vụ
- `NotifyDineInPaymentCompleted(payment)` - Thanh toán

---

## Test Accounts

| Role | Username | Password | Access |
|------|----------|----------|--------|
| Admin | admin | admin123 | Full |
| Manager | manager | manager123 | Operations |
| Staff | staff | staff123 | Service only |

### Seed Script
**File:** `seed-users.sql`

Run script để tạo 3 test accounts với 3 roles.

---

## Key Files

### Infrastructure
- `Program.cs` - App configuration, DI, Session, SignalR
- `Models/QlnhaHangBtlContext.cs` - EF Core DbContext
- `Filters/RoleAuthorizeAttribute.cs` - Authorization filter
- `Filters/AdminSessionAuthorizeAttribute.cs` - Session filter
- `Extensions/HttpContextExtensions.cs` - Role helpers

### Views
- `Views/Shared/_CloudyCafeLayout.cshtml` - Main layout
- `Views/Shared/_AdminSidebar.cshtml` - Admin sidebar
- `Views/Shared/AccessDenied.cshtml` - Access denied page

### Database
- `Database/QLNhaHang_BTL_ModelSync.sql` - Schema sync
- `seed-users.sql` - Test data seed

---

## Development Notes

### Best Practices Used
1. **Async/Await** - Tất cả DB operations
2. **Soft Delete** - Không xóa cứng
3. **UUID filenames** - Tránh trùng lặp ảnh
4. **Vietnamese Culture** - `CultureInfo("vi-VN")`
5. **JSON Metadata** - Flexible data storage

### Common Patterns
- Repository pattern cho DB access
- ViewModels cho complex queries
- Extension methods cho role checking
- Custom attributes cho authorization

### Concurrency Handling
- Sử dụng `DbUpdateConcurrencyException`
- Optimistic concurrency control

---

## Dependencies

### NuGet Packages
- Microsoft.EntityFrameworkCore (8.0.10)
- Microsoft.EntityFrameworkCore.SqlServer (8.0.10)
- Microsoft.EntityFrameworkCore.Design (8.0.10)
- Microsoft.AspNetCore.SignalR (1.1.0)

### Frontend Libraries
- Bootstrap 5
- jQuery
- Owl Carousel
- Font Awesome

---

## Environment Config

### Development
- **SQL Server:** `ADMIN-PC\MSSQLSERVER1`
- **Database:** `QLNhaHang_BTL`
- **User:** `sa`
- **Connection String:** `appsettings.Development.json`

---

## Deployment Checklist

- [ ] Build project thành công
- [ ] Run migrations (nếu cần)
- [ ] Seed test data (seed-users.sql)
- [ ] Config connection string production
- [ ] Set proper session timeout
- [ ] Test với từng role
- [ ] Verify SignalR connections
- [ ] Check file upload permissions

---

## Troubleshooting Quick Reference

### Session không lưu
```
Check:
- Program.cs có AddSession chưa?
- UseSession() có được gọi trước UseRouting()?
- Session timeout đã config chưa?
```

### Permission không hoạt động
```
Check:
- RoleAuthorizeAttribute được register chưa?
- Session["RoleKey"] có giá trị chưa?
- HttpContextExtensions.GetUserRole() trả đúng không?
```

### Online order status không cập nhật
```
Check:
- DonHang.GhiChu có đúng JSON format?
- OnlineOrderMetadata.TryParse() trả null không?
- VanChuyen = true chưa?
```

---

## Recent Changes

### 2026-05-18
- Cập nhật toàn bộ documentation
- Hoàn thành Online Order System
- Hoàn thành ProductConditions module
- Cập nhật permission matrix
- Tạo MEMORY.md

### 2026-05-16
- Implement role-based authorization
- Add 3 roles (admin, manager, staff)
- Create test accounts
- Implement AccessDenied page

---

## Links & Resources

### Internal
- Documentation: `CLAUNE.md`
- Task List: `Task.md`
- Testing Guide: `TESTING_GUIDE.md`

### External
- ASP.NET Core Docs: https://docs.microsoft.com/aspnet/core/
- EF Core Docs: https://docs.microsoft.com/ef/core/
- SignalR Docs: https://docs.microsoft.com/aspnet/core/signalr/

---

## Last Updated
**2026-05-18** - Full documentation update, project completion