# TÀI LIỆU KỸ THUẬT - HỆ THỐNG QUẢN LÝ NHÀ HÀNG CLOUDY CAFÉ

**Phiên bản:** 1.0  
**Ngày cập nhật:** 2026-05-19  
**Tác giả:** Technical Documentation Team

---

## MỤC LỤC

### Phần 1: Tổng Quan
- [01-overview.md](./01-overview.md) - Giới thiệu tổng quan về hệ thống
  - Kiến trúc hệ thống
  - Công nghệ sử dụng
  - Tính năng chính
  - Cấu trúc thư mục

### Phần 2: Cơ Sở Dữ Liệu
- [02-database-structure.md](./02-database-structure.md) - Chi tiết cấu trúc database
  - Danh sách entity (23 tables)
  - Quan hệ giữa các bảng
  - Soft delete pattern
  - Triggers

### Phần 3: Controllers và API
- [03-controllers-api.md](./03-controllers-api.md) - Phân tích controllers
  - AdminController
  - HomeController
  - TrangChuController
  - ProductsController
  - DonHangsController
  - AdminBaiVietChuyenNhasController
  - ViewModels

### Phần 4: Authorization và Logic
- [04-authorization-logic.md](./04-authorization-logic.md) - Authentication và Authorization
  - Session management
  - Role-based authorization
  - SignalR real-time
  - Cart handling
  - Soft delete pattern

### Phần 5: Frontend và Views
- [05-views-frontend.md](./05-views-frontend.md) - Phân tích frontend
  - Layouts
  - ViewComponents
  - JavaScript functions
  - CSS styling
  - Responsive design

### Phần 6: Đề Xuất Cải Thiện
- [06-recommendations.md](./06-recommendations.md) - Bài học kinh nghiệm
  - Điểm mạnh
  - Điểm yếu
  - Đề xuất cải thiện
  - Checklist bảo mật

---

## quick reference

### Entity List (23 Tables)

| # | Entity | Bảng | Mô Tả |
|---|--------|------|-------|
| 1 | KhachHang | KhachHang | Khách hàng |
| 2 | NhanVien | NhanVien | Nhân viên |
| 3 | DonHang | DonHang | Đơn hàng |
| 4 | ChiTietHoaDon | ChiTietHoaDon | Chi tiết hóa đơn |
| 5 | Product | Product | Sản phẩm |
| 6 | Category | Category | Danh mục |
| 7 | Ban | Ban | Bàn ăn |
| 8 | CuaHang | CuaHang | Cửa hàng |
| 9 | OnlineOrderInfo | OnlineOrderInfo | Thông tin đơn online |
| 10 | OnlineOrderStatusHistory | OnlineOrderStatusHistory | Lịch sử trạng thái |
| 11 | BaiVietChuyenNha | BaiVietChuyenNha | Bài viết |
| 12 | HoaDonNhap | HoaDonNhap | Hóa đơn nhập |
| 13 | ChiTietHoaDonNhap | ChiTietHoaDonNhap | Chi tiết hóa đơn nhập |
| 14 | NguyenLieu | NguyenLieu | Nguyên liệu |
| 15 | CongThuc | CongThuc | Công thức |
| 16 | NhaCungCap | NhaCungCap | Nhà cung cấp |
| 17 | KhuyenMai | KhuyenMai | Khuyến mãi |
| 18 | NgayCong | NgayCong | Ngày công |
| 19 | PhanQuyen | PhanQuyen | Phân quyền |
| 20 | NvPq | NV_PQ | Nhân viên - Phân quyền |
| 21 | NvNc | NV_NC | Nhân viên - Ngày công |
| 22 | Thuong | Thuong | Thưởng |
| 23 | ProductConditions | ProductConditions | Điều kiện sản phẩm |

### Controllers List

| Controller | File | Chức năng |
|------------|------|-----------|
| AdminController | AdminController.cs | Dashboard, quản trị |
| HomeController | HomeController.cs | Khách hàng, giỏ hàng |
| TrangChuController | TrangChuController.cs | Trang chủ công khai |
| ProductsController | ProductsController.cs | Quản lý sản phẩm |
| DonHangsController | DonHangsController.cs | Quản lý đơn hàng |
| KhachHangsController | KhachHangsController.cs | Quản lý khách hàng |
| NhanViensController | NhanViensController.cs | Quản lý nhân viên |
| AdminBaiVietChuyenNhasController | AdminBaiVietChuyenNhasController.cs | Quản lý bài viết |
| AdminCuaHangsController | AdminCuaHangsController.cs | Quản lý cửa hàng |
| ChiTietHoaDonsController | ChiTietHoaDonsController.cs | Chi tiết hóa đơn |
| ChartDataController | ChartDataController.cs | Dữ liệu biểu đồ |

### Roles và Permissions

| Role | Quyền hạn |
|------|-----------|
| admin | Toàn quyền (products, orders, customers, employees, roles, permissions) |
| manager | Quản lý (products, orders, customers, content, stores) - trừ employees, roles |
| staff | Nhân viên (products, orders, customers, tables) - chỉ xem |

### Session Keys

| Key | Type | Mô Tả |
|-----|------|-------|
| NhanVienId | int | ID nhân viên |
| NhanVienName | string | Tên nhân viên |
| NhanVienTaiKhoan | string | Tài khoản admin |
| RoleKey | string | Role (admin/manager/staff) |
| CustomerID | int | ID khách hàng |
| BanId | int | ID bàn ăn |
| DineInCustomerId | string | Guest ID dine-in |
| OnlineCartDhId | int | ID đơn hàng online |
| DhId | int | ID đơn hàng dine-in |

### Order Status (Online)

| Status | Mô Tả |
|--------|-------|
| cart | Giỏ hàng |
| pending | Chờ xác nhận |
| preparing | Đang chuẩn bị |
| shipping | Đang giao |
| delivered | Đã giao |
| cancelled | Đã hủy |

---

## CÔNG NGHỆ SỬ DỤNG

| Công Nghệ | Phiên Bản | Mục Đích |
|-----------|-----------|----------|
| ASP.NET Core | 8.0 | Web framework |
| Entity Framework Core | 8.0.10 | ORM |
| SQL Server | 2019+ | Database |
| SignalR | 1.1.0 | Real-time |
| Bootstrap | 4.x | UI framework |
| jQuery | 3.x | DOM manipulation |
| FontAwesome | 6.5.2 | Icons |

---

## LIÊN HỆ

**Dự án:** Web Quản Lý Nhà Hàng - BTL  
**Lĩnh vực:** Quản lý nhà hàng, Food & Beverage  
**Ngôn ngữ:** Tiếng Việt

---

*Đây là tài liệu kỹ thuật nội bộ, được tạo tự động từ phân tích source code.*