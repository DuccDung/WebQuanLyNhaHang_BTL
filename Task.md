# Task List - Hệ Thống Quản Lý Nhà Hàng

## Trạng thái: **HOÀN THÀNH**

---

## Giai Đoạn 1: Chuẩn Bị Dữ Liệu

### ✅ Task 1: Populate Dữ Liệu Quyền Vào Database
- **Trạng thái**: Hoàn thành
- **File**: `seed-users.sql`
- **Mô tả**: Tạo 3 vai trò (admin, manager, staff) và 3 nhân viên test

### ✅ Task 2: Gán Quyền Mặc Định Cho Nhân Viên Hiện Có
- **Trạng thái**: Hoàn thành
- **File**: `seed-users.sql`
- **Mô tả**: Gán quyền cho 3 nhân viên test

---

## Giai Đoạn 2: Core Infrastructure

### ✅ Task 3-4: Permission Service (Interface + Implementation)
- **Trạng thái**: Hoàn thành
- **File**: `Services/PermissionService.cs`

### ✅ Task 5: RoleAuthorizeAttribute
- **Trạng thái**: Hoàn thành
- **File**: `Filters/RoleAuthorizeAttribute.cs`
- **Mô tả**: Custom authorization attribute cho role-based access

### ✅ Task 6: Cập Nhật Login
- **Trạng thái**: Hoàn thành
- **File**: `Controllers/AdminController.cs`
- **Mô tả**: Session-based authentication với role storage

### ✅ Task 7: HttpContext Extensions
- **Trạng thái**: Hoàn thành
- **File**: `Extensions/HttpContextExtensions.cs`
- **Mô tả**: Extension methods cho role checking (GetUserRole, HasRole, IsAdmin)

---

## Giai Đoạn 3: Phân Quyền Module

### ✅ Task 8: Phân Quyền Module Sản Phẩm
- **Trạng thái**: Hoàn thành
- **File**: `ProductsController.cs`
- **Quyền**:
  - View: admin, manager, staff
  - Create/Edit: admin, manager
  - Delete: admin

### ✅ Task 9: Phân Quyền Module Danh Mục
- **Trạng thái**: Hoàn thành (quản lý qua Products)

### ✅ Task 10: Phân Quyền Module Đơn Hàng
- **Trạng thái**: Hoàn thành
- **File**: `DonHangsController.cs`
- **Quyền**:
  - View: admin, manager, staff
  - Create: admin, manager
  - Update: admin, manager, staff
  - Delete: admin

### ✅ Task 11: Phân Quyền Module Khách Hàng
- **Trạng thái**: Hoàn thành
- **File**: `KhachHangsController.cs`
- **Quyền**:
  - View: admin, manager, staff
  - Create/Edit: admin, manager
  - Delete: admin

### ✅ Task 12: Phân Quyền Module Nhân Viên
- **Trạng thái**: Hoàn thành
- **File**: `NhanViensController.cs`
- **Quyền**:
  - View: admin, manager
  - Create/Edit/Delete: admin
  - Chỉ admin được phân quyền

### ✅ Task 13: Phân Quyền Module Đơn Tại Quán
- **Trạng thái**: Hoàn thành
- **File**: `AdminController.cs` (Ban, GetFormBuy, ProcessPayment)
- **Quyền**: admin, manager

### ✅ Task 14: Phân Quyền Module Đơn Online
- **Trạng thái**: Hoàn thành (qua DonHangsController)
- **File**: `Models/OnlineOrderMetadata.cs`
- **Mô tả**: JSON helper class cho đơn hàng online

### ✅ Task 15: Phân Quyền Module Nguyên Liệu
- **Trạng thái**: Hoàn thành
- **File**: `NguyenLieu.cs`, `CongThuc.cs`

### ✅ Task 16: Phân Quyền Module Báo Cáo
- **Trạng thái**: Hoàn thành
- **File**: `ChartDataController.cs`

### ✅ Task 17: Phân Quyền Module ProductConditions
- **Trạng thái**: Hoàn thành
- **File**: `ProductConditionsController.cs`, `ProductConditions.cs`

### ✅ Task 18-22: Sidebar + Buttons UI
- **Trạng thái**: Hoàn thành
- **File**: `Views/Shared/_AdminSidebar.cshtml`
- **Tính năng**: Ẩn/hiện menu theo role

---

## Giai Đoạn 4: Online Order System

### ✅ Task 23: OnlineOrderInfo Entity
- **Trạng thái**: Hoàn thành
- **File**: `Models/OnlineOrderInfo.cs`
- **Mô tả**: Entity lưu thông tin đơn hàng online

### ✅ Task 24: OnlineOrderStatusHistory Entity
- **Trạng thái**: Hoàn thành
- **File**: `Models/OnlineOrderStatusHistory.cs`
- **Mô tả**: Entity lưu lịch sử thay đổi trạng thái

### ✅ Task 25: OnlineOrderMetadata Helper
- **Trạng thái**: Hoàn thành
- **File**: `Models/OnlineOrderMetadata.cs`
- **Mô tả**: JSON serialization/deserialization cho metadata

### ✅ Task 26: DonHangsController Online Order Support
- **Trạng thái**: Hoàn thành
- **File**: `Controllers/DonHangsController.cs`
- **Tính năng**:
  - `UpdateDeliveryStatus()`: Cập nhật trạng thái giao hàng
  - `DetailsDataV2()`: API trả JSON chi tiết

---

## Giai Đoạn 5: Testing

### ✅ Task 27-30: Tất cả tests
- **Trạng thái**: Hoàn thành
- **Nội dung**:
  - ✅ Test login với từng role (admin, manager, staff)
  - ✅ Test truy cập URL trực tiếp
  - ✅ Test sidebar hiển thị
  - ✅ Test button visibility
  - ✅ Test AccessDenied redirect
  - ✅ Test AJAX permission response

---

## Giai Đoạn 6: Documentation

### ✅ Task 31-32: Cleanup + Documentation
- **Trạng thái**: Hoàn thành
- **Nội dung**:
  - ✅ Cleanup code
  - ✅ Viết documentation
  - ✅ Cập nhật CLAUNE.md
  - ✅ Cập nhật TESTING_GUIDE.md
  - ✅ Tạo MEMORY.md

---

## 📋 Tổng Kết Các File Đã Cập Nhật

### New Files:
1. `Extensions/HttpContextExtensions.cs` - Role checking extension methods
2. `Filters/RoleAuthorizeAttribute.cs` - Custom authorization attribute
3. `Models/OnlineOrderMetadata.cs` - JSON helper cho đơn hàng online
4. `Models/OnlineOrderInfo.cs` - Entity đơn hàng online
5. `Models/OnlineOrderStatusHistory.cs` - Entity lịch sử trạng thái
6. `Views/Shared/AccessDenied.cshtml` - Access denied page
7. `Pages/Shared/AccessDenied.cshtml` - Access denied page (alternative)

### Modified Files:
1. `Controllers/AdminController.cs` - Login + Ban/ProcessPayment methods
2. `Controllers/ProductsController.cs` - Product CRUD actions
3. `Controllers/DonHangsController.cs` - Order actions + online order support
4. `Controllers/KhachHangsController.cs` - Customer actions
5. `Controllers/NhanViensController.cs` - Employee actions + role assignment
6. `Controllers/ChiTietHoaDonsController.cs` - Order detail actions
7. `Controllers/AdminCuaHangsController.cs` - Store actions
8. `Controllers/AdminBaiVietChuyenNhasController.cs` - Blog post actions
9. `Views/Shared/_AdminSidebar.cshtml` - Role-based menu visibility
10. `Controllers/ChiTietHoaDonsController.cs` - Order detail actions

---

## 🎯 Permission Matrix Summary

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
| ProductConditions | all | admin, manager | admin | - |
| Báo cáo | admin, manager | - | - | - |

---

## 🎉 Progress: 100% Hoàn Thành

### Đã hoàn thành:
- ✅ Core authorization infrastructure
- ✅ Role-based action filters on all controllers
- ✅ Sidebar UI with role-based visibility
- ✅ Admin-only role assignment
- ✅ Online Order System với delivery tracking
- ✅ ProductConditions module
- ✅ AccessDenied pages
- ✅ Testing với tất cả roles
- ✅ Documentation đầy đủ

### Test Accounts:
| Role | Username | Password |
|------|----------|----------|
| Admin | admin | admin123 |
| Manager | manager | manager123 |
| Staff | staff | staff123 |

---

## Changelog

### 2026-05-18
- Cập nhật tài liệu hoàn chỉnh
- Thêm Online Order System support
- Thêm ProductConditions module
- Cập nhật permission matrix
- Hoàn thành 100% tasks

### 2026-05-16
- Initial documentation
- Hệ thống phân quyền cơ bản

---

## Ghi Chú

- **Soft Delete**: Tất cả entities đều có field `Remove` (bit)
- **Session Timeout**: 30 phút
- **SignalR Real-time**: Thông báo đơn hàng mới, thanh toán
- **JSON Metadata**: Đơn hàng online lưu trong `GhiChu` field