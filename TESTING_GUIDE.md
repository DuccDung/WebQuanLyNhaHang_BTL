# Hướng Dẫn Test Hệ Thống Phân Quyền

## 📋 Mục Lục
1. [Chuẩn Bị Dữ Liệu](#chuẩn-bị-dữ-liệu)
2. [Thông Tin Tài Khoản Test](#thông-tin-tài-khoản-test)
3. [Ma Trận Quyền Hạn Chi Tiết](#ma-trận-quyền-hạn-chi-tiết)
4. [Quy Trình Test Từng Module](#quy-trình-test-từng-module)
5. [Các Kịch Bản Test Chính](#các-kịch-bản-test-chính)
6. [Test Online Order System](#test-online-order-system)
7. [Test ProductConditions](#test-productconditions)

---

## Chuẩn Bị Dữ Liệu

### Bước 1: Chạy Script SQL
Chạy script `seed-users.sql` để tạo 3 tài khoản test với 3 vai trò khác nhau.

### Bước 2: Verify Dữ Liệu
```sql
SELECT 
    nv.NvId,
    nv.TenNhanVien,
    nv.TaiKhoan,
    pq.TenQuyen AS RoleName
FROM [dbo].[NhanVien] nv
INNER JOIN [dbo].[NvPq] nvpq ON nv.NvId = nvpq.NvId
INNER JOIN [dbo].[PhanQuyen] pq ON nvpq.PqId = pq.PqId
WHERE nv.Remove = 0;
```

---

## Thông Tin Tài Khoản Test

| Role | Tài Khoản | Mật Khẩu | Mục Đích Test |
|------|-----------|----------|---------------|
| **Admin** | `admin` | `admin123` | Full access - kiểm tra tất cả chức năng |
| **Manager** | `manager` | `manager123` | Quản lý vận hành - không được quản lý nhân viên |
| **Staff** | `staff` | `staff123` | Nhân viên phục vụ - chỉ xem và tạo đơn |

---

## Ma Trận Quyền Hạn Chi Tiết

### 🟢 Admin - Quyền Hạn Tối Đa

| Module | View | Create | Edit | Delete | Ghi Chú |
|--------|------|--------|------|--------|---------|
| **Dashboard** | ✅ | - | - | - | Xem thống kê |
| **Sản Phẩm** | ✅ | ✅ | ✅ | ✅ | Quản lý full |
| **Danh Mục** | ✅ | ✅ | ✅ | ✅ | Quản lý full |
| **Đơn Hàng** | ✅ | ✅ | ✅ | ✅ | Quản lý full |
| **Chi Tiết Đơn** | ✅ | ✅ | ✅ | ✅ | Quản lý full |
| **Khách Hàng** | ✅ | ✅ | ✅ | ✅ | Quản lý full |
| **Nhân Viên** | ✅ | ✅ | ✅ | ✅ | **Chỉ admin được tạo/xóa NV** |
| **Phân Quyền** | ✅ | ✅ | ✅ | ✅ | **Chỉ admin được gán quyền** |
| **Cửa Hàng** | ✅ | ✅ | ✅ | ✅ | Quản lý full |
| **Chuyện Nhà** | ✅ | ✅ | ✅ | ✅ | Quản lý full |
| **Đơn Tại Quán** | ✅ | ✅ | ✅ | - | Thanh toán bàn |
| **ProductConditions** | ✅ | ✅ | ✅ | ✅ | Quản lý full |
| **Báo Cáo** | ✅ | - | - | - | Xem thống kê |

---

### 🟡 Manager - Quản Lý Vận Hành

| Module | View | Create | Edit | Delete | Ghi Chú |
|--------|------|--------|------|--------|---------|
| **Dashboard** | ✅ | - | - | - | Xem thống kê |
| **Sản Phẩm** | ✅ | ✅ | ✅ | ❌ | Không được xóa SP |
| **Danh Mục** | ✅ | ✅ | ✅ | ❌ | Không được xóa DC |
| **Đơn Hàng** | ✅ | ✅ | ✅ | ❌ | Không được xóa đơn |
| **Chi Tiết Đơn** | ✅ | ✅ | ✅ | ❌ | Không được xóa CT |
| **Khách Hàng** | ✅ | ✅ | ✅ | ❌ | Không được xóa KH |
| **Nhân Viên** | ✅ | ❌ | ❌ | ❌ | **Chỉ xem, không sửa** |
| **Phân Quyền** | ❌ | ❌ | ❌ | ❌ | **Không truy cập được** |
| **Cửa Hàng** | ✅ | ✅ | ✅ | ❌ | Không được xóa CH |
| **Chuyện Nhà** | ✅ | ✅ | ✅ | ❌ | Không được xóa bài |
| **Đơn Tại Quán** | ✅ | ✅ | ✅ | - | Thanh toán bàn |
| **ProductConditions** | ✅ | ✅ | ✅ | ❌ | Không được xóa |
| **Báo Cáo** | ✅ | - | - | - | Xem thống kê |

---

### 🟢 Staff - Nhân Viên Phục Vụ

| Module | View | Create | Edit | Delete | Ghi Chú |
|--------|------|--------|------|--------|---------|
| **Dashboard** | ❌ | - | - | - | **Không truy cập** |
| **Sản Phẩm** | ✅ | ❌ | ❌ | ❌ | **Chỉ xem menu** |
| **Danh Mục** | ✅ | ❌ | ❌ | ❌ | **Chỉ xem** |
| **Đơn Hàng** | ✅ | ✅ | ❌ | ❌ | Tạo đơn mới |
| **Chi Tiết Đơn** | ✅ | ❌ | ❌ | ❌ | Xem đơn |
| **Khách Hàng** | ✅ | ❌ | ❌ | ❌ | Xem thông tin KH |
| **Nhân Viên** | ❌ | ❌ | ❌ | ❌ | **Không truy cập** |
| **Phân Quyền** | ❌ | ❌ | ❌ | ❌ | **Không truy cập** |
| **Cửa Hàng** | ❌ | ❌ | ❌ | ❌ | **Không truy cập** |
| **Chuyện Nhà** | ❌ | ❌ | ❌ | ❌ | **Không truy cập** |
| **Đơn Tại Quán** | ✅ | ✅ | ✅ | - | Tạo/thanh toán đơn |
| **ProductConditions** | ✅ | ❌ | ❌ | ❌ | **Chỉ xem** |
| **Báo Cáo** | ❌ | - | - | - | **Không truy cập** |

---

## Quy Trình Test Từng Module

### 1. Test Đăng Nhập
```
URL: http://localhost:xxxx/Admin/Login

Bước:
1. Đăng nhập với từng tài khoản (admin, manager, staff)
2. Kiểm tra redirect về Dashboard
3. Kiểm tra session được lưu đúng (tên, tài khoản, role)
```

### 2. Test Sidebar Menu
```
Sau khi đăng nhập, kiểm tra menu hiển thị:

✅ Admin thấy: Tất cả menu items
✅ Manager thấy: Dashboard, SP, Đơn hàng, KH, Cửa hàng, Chuyện nhà, ProductConditions
❌ Staff thấy: Sản phẩm, Đơn hàng, Đơn tại quán, ProductConditions (chỉ xem)
```

### 3. Test Module Sản Phẩm

#### Admin:
```
1. Xem danh sách sản phẩm ✅
2. Tạo sản phẩm mới ✅
3. Chỉnh sửa sản phẩm ✅
4. Xóa sản phẩm (soft delete) ✅
5. Upload ảnh sản phẩm ✅
```

#### Manager:
```
1. Xem danh sách sản phẩm ✅
2. Tạo sản phẩm mới ✅
3. Chỉnh sửa sản phẩm ✅
4. Xóa sản phẩm ❌ (phải redirect AccessDenied)
```

#### Staff:
```
1. Xem danh sách sản phẩm ✅
2. Tạo sản phẩm mới ❌ (AccessDenied)
3. Chỉnh sửa sản phẩm ❌ (AccessDenied)
4. Xóa sản phẩm ❌ (AccessDenied)
```

### 4. Test Module Đơn Hàng

#### Admin:
```
1. Xem tất cả đơn hàng ✅
2. Tạo đơn mới ✅
3. Chỉnh sửa đơn ✅
4. Xóa đơn ✅
5. Cập nhật trạng thái ✅
```

#### Manager:
```
1. Xem tất cả đơn hàng ✅
2. Tạo đơn mới ✅
3. Chỉnh sửa đơn ✅
4. Xóa đơn ❌ (AccessDenied)
5. Cập nhật trạng thái ✅
```

#### Staff:
```
1. Xem đơn hàng của mình ✅
2. Tạo đơn mới ✅
3. Chỉnh sửa đơn ❌ (chỉ xem)
4. Xóa đơn ❌ (AccessDenied)
5. Cập nhật trạng thái ✅
```

### 5. Test Module Nhân Viên (Admin Only)

```
1. Xem danh sách nhân viên ✅
2. Tạo nhân viên mới ✅
3. Chỉnh sửa nhân viên ✅
4. Xóa nhân viên ✅
5. Gán quyền cho nhân viên ✅

Manager/Staff truy cập /NhanViens:
→ Redirect AccessDenied
```

### 6. Test Module ProductConditions

#### Admin:
```
1. Xem danh sách conditions ✅
2. Tạo condition mới ✅
3. Chỉnh sửa condition ✅
4. Xóa condition ✅
```

#### Manager:
```
1. Xem danh sách conditions ✅
2. Tạo condition mới ✅
3. Chỉnh sửa condition ✅
4. Xóa condition ❌ (AccessDenied)
```

#### Staff:
```
1. Xem danh sách conditions ✅
2. Tạo/Edit/Xóa ❌ (AccessDenied)
```

### 7. Test Trực Tiếp URL (Security Test)

```
Đăng nhập với vai trò staff, thử truy cập:

URL: http://localhost:xxxx/Products/Create
→ Phải redirect AccessDenied hoặc trả 403

URL: http://localhost:xxxx/NhanViens/Index
→ Phải redirect AccessDenied

URL: http://localhost:xxxx/Admin/Ban
→ Phải redirect AccessDenied

URL: http://localhost:xxxx/ProductConditions/Delete/1
→ Phải redirect AccessDenied
```

---

## Các Kịch Bản Test Chính

### Kịch Bản 1: Full Test Với Admin
```
1. Đăng nhập bằng admin/admin123
2. Kiểm tra sidebar có đầy đủ 10+ menu items
3. Thử tạo sản phẩm mới → Thành công
4. Thử xóa sản phẩm → Thành công
5. Thử tạo nhân viên mới → Thành công
6. Thử gán quyền cho nhân viên → Thành công
7. Thử vào trang Đơn tại quán → Truy cập được
8. Thử tạo ProductCondition → Thành công
```

### Kịch Bản 2: Test Giới Hạn Với Manager
```
1. Đăng nhập bằng manager/manager123
2. Kiểm tra sidebar KHÔNG có menu "Nhân viên"
3. Thử tạo sản phẩm → Thành công
4. Thử xóa sản phẩm → AccessDenied
5. Thử truy cập /NhanViens → AccessDenied
6. Thử truy cập /Admin/Ban → Thành công
7. Thử tạo ProductCondition → Thành công
8. Thử xóa ProductCondition → AccessDenied
```

### Kịch Bản 3: Test Giới Hạn Với Staff
```
1. Đăng nhập bằng staff/staff123
2. Kiểm tra sidebar CHỈ có: Sản phẩm, Đơn hàng, Đơn tại quán
3. Thử xem sản phẩm → Thành công
4. Thử tạo đơn hàng → Thành công
5. Thử truy cập /Products/Create → AccessDenied
6. Thử truy cập /NhanViens → AccessDenied
7. Thử truy cập /Admin/Dashboard → AccessDenied
8. Thử xem ProductConditions → Thành công (chỉ xem)
```

### Kịch Bản 4: URL Bypass Test
```
1. Đăng nhập staff
2. Copy URL: http://localhost:xxxx/Products/Edit/1
3. Dán URL mới trên tab trình duyệt
4. → Phải bị redirect AccessDenied

5. Copy URL: http://localhost:xxxx/NhanViens/Delete/1
6. Dán URL mới
7. → Phải bị redirect AccessDenied

8. Copy URL: http://localhost:xxxx/ProductConditions/Edit/1
9. Dán URL mới
10. → Phải bị redirect AccessDenied
```

### Kịch Bản 5: AJAX Permission Test
```
1. Đăng nhập staff
2. Sử dụng Postman/cURL gọi:
   POST /Products/Create
   Body: { ... sản phẩm ... }
   
3. Response phải là:
   {
     "success": false,
     "message": "Bạn không có quyền thực hiện thao tác này.",
     "redirectUrl": "/Admin/Login"
   }
```

---

## Test Online Order System

### Test Trạng Thái Đơn Hàng Online

```
Trạng thái workflow:
cart → pending → preparing → shipping → delivered
                                  → cancelled
```

#### Test từng trạng thái:

1. **Giỏ hàng (cart)**
   - Tạo đơn hàng online nhưng chưa submit
   - Kiểm tra `VanChuyen = true` và `TrangThai = false`
   - Metadata có `"s": "cart"`

2. **Chờ xác nhận (pending)**
   - Submit đơn hàng
   - Kiểm tra metadata có `"s": "pending"`
   - Hiển thị badge "Chờ xác nhận"

3. **Đang chuẩn bị (preparing)**
   - Cập nhật trạng thái → preparing
   - Kiểm tra metadata có `"s": "preparing"`
   - Hiển thị badge "Đang chuẩn bị"

4. **Đang giao (shipping)**
   - Cập nhật trạng thái → shipping
   - Kiểm tra metadata có `"s": "shipping"`
   - Hiển thị badge "Đang giao"

5. **Đã giao (delivered)**
   - Cập nhật trạng thái → delivered
   - Kiểm tra metadata có `"s": "delivered"`
   - Hiển thị badge "Đã giao"

6. **Đã hủy (cancelled)**
   - Cập nhật trạng thái → cancelled
   - Kiểm tra metadata có `"s": "cancelled"`
   - Hiển thị badge "Đã hủy"

### Test OnlineOrderMetadata Serialization

```csharp
// Test CreateCart
var cart = OnlineOrderMetadata.CreateCart();
Assert.Equal("online", cart.Type);
Assert.Equal("cart", cart.DeliveryStatus);

// Test ToJson
var json = cart.ToJson();
Assert.Contains("online", json);

// Test TryParse
var parsed = OnlineOrderMetadata.TryParse(json);
Assert.NotNull(parsed);
Assert.Equal("online", parsed.Type);

// Test IsOnlineOrder
var order = new DonHang { VanChuyen = true, GhiChu = json };
Assert.True(OnlineOrderMetadata.IsOnlineOrder(order));
```

### Test Permissions cho Đơn Online

```
Admin:
- Xem tất cả đơn online ✅
- Cập nhật trạng thái ✅
- Hủy đơn ✅

Manager:
- Xem tất cả đơn online ✅
- Cập nhật trạng thái ✅
- Hủy đơn ❌

Staff:
- Xem đơn của mình ✅
- Cập nhật trạng thái (chấp nhận) ✅
- Tạo đơn mới ✅
- Hủy đơn ❌
```

---

## Test ProductConditions

### Test CRUD

#### Admin:
```
1. Xem danh sách conditions ✅
2. Tạo condition mới (ví dụ: "Đá", "Nóng", "Ít đường") ✅
3. Gán condition cho sản phẩm ✅
4. Chỉnh sửa condition ✅
5. Xóa condition ✅
```

#### Manager:
```
1. Xem danh sách conditions ✅
2. Tạo condition mới ✅
3. Gán condition cho sản phẩm ✅
4. Chỉnh sửa condition ✅
5. Xóa condition ❌ (AccessDenied)
```

#### Staff:
```
1. Xem danh sách conditions ✅
2. Tạo/Edit/Xóa ❌ (AccessDenied)
3. Chọn condition khi order ✅ (trên frontend)
```

### Test Checkbox Condition trên Frontend

```
1. Truy cập trang chi tiết sản phẩm
2. Kiểm tra checkbox conditions hiển thị đúng
3. Chọn nhiều conditions (ví dụ: "Đá" + "Ít đường")
4. Thêm vào giỏ hàng
5. Kiểm tra conditions được lưu đúng
```

---

## Checklist Test

### ✅ Pre-test
- [ ] Database đã có 3 users (admin, manager, staff)
- [ ] Project build thành công (0 errors)
- [ ] Server đang chạy
- [ ] Dữ liệu sample products đã có

### ✅ Admin Test
- [ ] Đăng nhập thành công
- [ ] Sidebar hiển thị đầy đủ
- [ ] Tạo/Sửa/Xóa sản phẩm
- [ ] Tạo/Sửa/Xóa đơn hàng
- [ ] Tạo/Sửa/Xóa khách hàng
- [ ] Tạo/Sửa/Xóa nhân viên
- [ ] Gán quyền cho nhân viên
- [ ] Quản lý cửa hàng
- [ ] Quản lý bài viết
- [ ] Quản lý ProductConditions
- [ ] Xử lý đơn online (full CRUD)

### ✅ Manager Test
- [ ] Đăng nhập thành công
- [ ] Sidebar KHÔNG có menu Nhân viên
- [ ] Tạo/Sửa sản phẩm (OK)
- [ ] Xóa sản phẩm (AccessDenied)
- [ ] Truy cập /NhanViens (AccessDenied)
- [ ] Quản lý đơn hàng (OK)
- [ ] Tạo ProductCondition (OK)
- [ ] Xóa ProductCondition (AccessDenied)

### ✅ Staff Test
- [ ] Đăng nhập thành công
- [ ] Sidebar chỉ có 3-4 menu items
- [ ] Xem sản phẩm (OK)
- [ ] Tạo đơn hàng (OK)
- [ ] Truy cập /Products/Create (AccessDenied)
- [ ] Truy cập /NhanViens (AccessDenied)
- [ ] Xem ProductConditions (OK)
- [ ] Chọn conditions khi order (OK)

---

## Lưu Ý Khi Test

1. **Logout giữa các test**: Luôn logout trước khi test tài khoản khác
2. **Clear cache**: Nếu thấy weird behavior, clear browser cache
3. **Incognito mode**: Khuyên dùng để tránh session conflict
4. **Check console**: Mở DevTools → Console để xem AJAX errors
5. **Check network**: Tab Network để xem response từ server

---

## Troubleshooting

### Vấn đề: Không đăng nhập được
```
Nguyên nhân:
- Sai username/password
- Database chưa có user
- Session chưa được config

Giải pháp:
- Chạy lại script SQL
- Kiểm tra connection database
- Kiểm tra Program.cs có AddSession chưa
```

### Vấn đề: AccessDenied xuất hiện không đúng
```
Nguyên nhân:
- Role chưa được lưu trong session
- RoleAuthorizeAttribute logic sai

Giải pháp:
- Check Session["RoleKey"] sau login
- Check RoleAuthorizeAttribute.OnActionExecuting
```

### Vấn đề: Menu hiển thị sai
```
Nguyên nhân:
- Context.GetUserRole() trả null
- Sidebar condition sai

Giải pháp:
- Check HttpContextExtensions.GetUserRole()
- Check _AdminSidebar.cshtml @if conditions
```

### Vấn đề: Đơn online không cập nhật được trạng thái
```
Nguyên nhân:
- Metadata JSON không hợp lệ
- VanChuyen = false

Giải pháp:
- Check DonHang.GhiChu có đúng format JSON không
- Check OnlineOrderMetadata.TryParse() trả null hay không
```

---

## Báo Cáo Lỗi

Khi phát hiện lỗi, ghi lại:
```
- User role: [admin/manager/staff]
- Module: [tên module]
- Action: [view/create/edit/delete]
- Kết quả mong đợi: [...]
- Kết quả thực tế: [...]
- Screenshot: [nếu có]
```

---

## Kết Luận

Hệ thống phân quyền đã implement:
- ✅ Role-based authorization ở level action method
- ✅ Session-based authentication
- ✅ Role-based sidebar visibility
- ✅ AccessDenied page cho unauthorized access
- ✅ JSON response cho AJAX requests
- ✅ Online Order System với đầy đủ trạng thái
- ✅ ProductConditions module

**Mục tiêu test**: Đảm bảo mỗi role chỉ truy cập được chức năng được phép.