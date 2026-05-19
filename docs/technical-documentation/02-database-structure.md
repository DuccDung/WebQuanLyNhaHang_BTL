# 2. CẤU TRÚC CƠ SỞ DỮ LIỆU

## 2.1 Giới Thiệu Tổng Quan

Hệ thống sử dụng **Microsoft SQL Server** làm cơ sở dữ liệu chính, được quản lý thông qua **Entity Framework Core 8.0** với pattern **Code-First**.

Database Context: `QlnhaHangBtlContext`

---

## 2.2 Danh Sách Các Entity (Bảng)

### 2.2.1 Danh Sách Hoàn Chỉnh

| Tên Entity | Tên Bảng | Mô Tả |
|------------|----------|-------|
| `KhachHang` | KhachHang | Khách hàng |
| `NhanVien` | NhanVien | Nhân viên |
| `DonHang` | DonHang | Đơn hàng |
| `ChiTietHoaDon` | ChiTietHoaDon | Chi tiết hóa đơn |
| `Product` | Product | Sản phẩm |
| `Category` | Category | Danh mục sản phẩm |
| `Ban` | Ban | Bàn ăn |
| `CuaHang` | CuaHang | Thông tin cửa hàng |
| `OnlineOrderInfo` | OnlineOrderInfo | Thông tin đơn hàng online |
| `OnlineOrderStatusHistory` | OnlineOrderStatusHistory | Lịch sử trạng thái đơn hàng |
| `BaiVietChuyenNha` | BaiVietChuyenNha | Bài viết chuyên nhà |
| `HoaDonNhap` | HoaDonNhap | Hóa đơn nhập |
| `ChiTietHoaDonNhap` | ChiTietHoaDonNhap | Chi tiết hóa đơn nhập |
| `NguyenLieu` | NguyenLieu | Nguyên liệu |
| `CongThuc` | CongThuc | Công thức chế biến |
| `NhaCungCap` | NhaCungCap | Nhà cung cấp |
| `KhuyenMai` | KhuyenMai | Khuyến mãi |
| `NgayCong` | NgayCong | Ngày công |
| `PhanQuyen` | PhanQuyen | Phân quyền |
| `NvPq` | NV_PQ | Nhân viên - Phân quyền (bridge) |
| `NvNc` | NV_NC | Nhân viên - Ngày công (bridge) |
| `Thuong` | Thuong | Thưởng |
| `ProductConditions` | ProductConditions | Điều kiện sản phẩm |

---

## 2.3 Chi Tiết Từng Entity

### 2.3.1 KhachHang (KhachHang)

**Mô tả:** Lưu thông tin khách hàng

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| KhId | int | PK | Khóa chính |
| TenKhachHang | nvarchar(100) | | Họ tên khách hàng |
| DiaChi | nvarchar(200) | | Địa chỉ |
| SoDienThoai | nvarchar(15) | | Số điện thoại |
| TaiKhoan | nvarchar(50) | | Tài khoản đăng nhập (email) |
| MatKhau | nvarchar(50) | | Mật khẩu |
| PathPhoto | nvarchar(100) | | Đường dẫn ảnh đại diện |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có nhiều DonHang (1:N)
- Có nhiều đơn hàng online

---

### 2.3.2 NhanVien (NhanVien)

**Mô tả:** Lưu thông tin nhân viên

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| NvId | int | PK | Khóa chính |
| TenNhanVien | nvarchar(100) | | Họ tên nhân viên |
| NgaySinh | date | | Ngày sinh |
| DiaChi | nvarchar(200) | | Địa chỉ |
| HeSoLuong | decimal(5,2) | | Hệ số lương |
| PathPhoto | nvarchar(100) | | Đường dẫn ảnh |
| TaiKhoan | nvarchar(50) | | Tài khoản đăng nhập |
| MatKhau | nvarchar(50) | | Mật khẩu |
| GioiTinh | nvarchar(10) | | Giới tính |
| SoDienThoai | nvarchar(15) | | Số điện thoại |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có nhiều DonHang (1:N) - nhân viên phục vụ
- Có nhiều HoaDonNhap (1:N) - nhân viên nhập hàng
- Có nhiều NvNc (1:N) - ngày công
- Có nhiều NvPq (1:N) - phân quyền
- Có nhiều Thuong (1:N) - thưởng
- Có nhiều OnlineOrderStatusHistory (1:N) - cập nhật trạng thái

---

### 2.3.3 DonHang (DonHang)

**Mô tả:** Lưu thông tin đơn hàng

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| DhId | int | PK | Khóa chính |
| KhId | int | FK | Khách hàng |
| BanId | int | FK | Bàn ăn (null = takeaway/delivery) |
| KmId | int | FK | Khuyến mãi |
| GioVao | datetime | | Giờ vào |
| GioRa | datetime | | Giờ ra |
| TongTien | money | | Tổng tiền |
| NvId | int | FK | Nhân viên phục vụ |
| GhiChu | nvarchar(500) | | Ghi chú (JSON metadata) |
| TrangThai | bit | | Trạng thái đơn (true = đã xác nhận) |
| VanChuyen | bit | | Vận chuyển (true = delivery/takeaway) |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có 1 KhachHang (N:1)
- Có 1 Ban (N:1)
- Có 1 KhuyenMai (N:1)
- Có 1 NhanVien (N:1)
- Có nhiều ChiTietHoaDon (1:N)
- Có 1 OnlineOrderInfo (1:1)
- Có nhiều OnlineOrderStatusHistory (1:N)

---

### 2.3.4 Product (Product)

**Mô tả:** Lưu thông tin sản phẩm/món ăn

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| ProductId | int | PK | Khóa chính |
| CateId | int | FK | Danh mục |
| TenSanPham | nvarchar(100) | | Tên sản phẩm |
| MoTa | nvarchar(500) | | Mô tả |
| GiaTien | money | | Giá tiền |
| PathPhoto | nvarchar(300) | | Đường dẫn ảnh |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có 1 Category (N:1)
- Có nhiều ChiTietHoaDon (1:N)
- Có nhiều CongThuc (1:N)
- Có nhiều ProductConditions (1:N)

---

### 2.3.5 Category (Category)

**Mô tả:** Danh mục sản phẩm

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| CateId | int | PK | Khóa chính |
| TenLoaiSanPham | nvarchar(100) | | Tên loại sản phẩm |
| MoTa | nvarchar(200) | | Mô tả |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có nhiều Product (1:N)

**Danh mục mẫu:**
- BEST MENU
- HOT DRINK
- SIGNATURE
- TEA
- JUICE
- CAKE

---

### 2.3.6 ChiTietHoaDon (ChiTietHoaDon)

**Mô tả:** Chi tiết hóa đơn - các món trong đơn hàng

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| CthdId | int | PK | Khóa chính |
| DhId | int | FK | Đơn hàng |
| ProductId | int | FK | Sản phẩm |
| SoLuong | int | | Số lượng |
| ThanhTien | money | | Thành tiền |
| GiamGia | int | | Giảm giá (id khuyến mãi áp dụng) |
| Ghichu | nvarchar(max) | | Ghi chú (trạng thái, yêu cầu đặc biệt) |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có 1 DonHang (N:1)
- Có 1 Product (N:1)

**Trigger:**
- `cau1`: Tự động tính toán khi thêm/sửa
- `cau6`: Cập nhật tổng tiền đơn hàng

---

### 2.3.7 Ban (Ban)

**Mô tả:** Bàn ăn trong nhà hàng

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| BanId | int | PK | Khóa chính |
| SoChoNgoi | int | | Số chỗ ngồi |
| GhiChu | nvarchar(50) | | Ghi chú |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có nhiều DonHang (1:N)

---

### 2.3.8 CuaHang (CuaHang)

**Mô tả:** Thông tin cửa hàng (cho chuỗi cửa hàng)

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| CuaHangId | int | PK | Khóa chính |
| TenCuaHang | nvarchar(150) | | Tên cửa hàng |
| DiaChi | nvarchar(300) | | Địa chỉ |
| TinhThanh | nvarchar(80) | | Tỉnh/thành phố |
| QuanHuyen | nvarchar(80) | | Quận/huyện |
| PhuongXa | nvarchar(80) | | Phường/xã |
| SoDienThoai | nvarchar(20) | | Số điện thoại |
| GioMoCua | time | | Giờ mở cửa |
| GioDongCua | time | | Giờ đóng cửa |
| PathPhoto | nvarchar(500) | | Ảnh cửa hàng |
| GoogleMapUrl | nvarchar(700) | | Link Google Maps |
| Latitude | decimal(10,7) | | Kinh độ |
| Longitude | decimal(10,7) | | Vĩ độ |
| SapXep | int | | Thứ tự sắp xếp |
| HienThi | bit | | Hiển thị (default: 1) |
| Remove | bit | | Flag xóa mềm (default: 0) |
| CreatedAt | datetime | | Ngày tạo |
| UpdatedAt | datetime | | Ngày cập nhật |

**Quan hệ:**
- Có nhiều OnlineOrderInfo (1:N)

---

### 2.3.9 OnlineOrderInfo (OnlineOrderInfo)

**Mô tả:** Thông tin chi tiết đơn hàng giao hàng

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| OnlineOrderInfoId | int | PK | Khóa chính |
| DhId | int | FK (Unique) | Đơn hàng |
| CuaHangId | int | FK | Cửa hàng giao |
| TrangThaiGiaoHang | nvarchar(30) | | Trạng thái giao hàng |
| NguoiNhan | nvarchar(100) | | Người nhận |
| SoDienThoai | nvarchar(20) | | SĐT người nhận |
| TinhThanh | nvarchar(80) | | Tỉnh/thành |
| QuanHuyen | nvarchar(80) | | Quận/huyện |
| PhuongXa | nvarchar(80) | | Phường/xã |
| DiaChi | nvarchar(300) | | Địa chỉ chi tiết |
| GhiChuGiaoHang | nvarchar(300) | | Ghi chú giao hàng |
| PhiGiaoHang | money | | Phí giao hàng |
| PhuongThucThanhToan | nvarchar(30) | | Phương thức thanh toán |
| TrangThaiThanhToan | nvarchar(30) | | Trạng thái thanh toán |
| NgayDat | datetime | | Ngày đặt |
| NgayCapNhat | datetime | | Ngày cập nhật |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có 1 DonHang (1:1)
- Có 1 CuaHang (N:1)

**Trạng thái giao hàng:**
- `cart`: Giỏ hàng
- `pending`: Chờ xác nhận
- `preparing`: Đang chuẩn bị
- `shipping`: Đang giao
- `delivered`: Đã giao
- `cancelled`: Đã hủy

---

### 2.3.10 OnlineOrderStatusHistory (OnlineOrderStatusHistory)

**Mô tả:** Lịch sử thay đổi trạng thái đơn hàng

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| HistoryId | int | PK | Khóa chính |
| DhId | int | FK | Đơn hàng |
| TrangThaiCu | nvarchar(30) | | Trạng thái cũ |
| TrangThaiMoi | nvarchar(30) | | Trạng thái mới |
| NvId | int | FK | Nhân viên cập nhật |
| GhiChu | nvarchar(300) | | Ghi chú |
| CreatedAt | datetime | | Thời gian |

**Quan hệ:**
- Có 1 DonHang (N:1)
- Có 1 NhanVien (N:1)

---

### 2.3.11 BaiVietChuyenNha (BaiVietChuyenNha)

**Mô tả:** Bài viết chuyên nhà (blog/news)

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| BaiVietId | int | PK | Khóa chính |
| TieuDe | nvarchar(200) | | Tiêu đề |
| Slug | nvarchar(220) | | Slug (URL-friendly) |
| TomTat | nvarchar(600) | | Tóm tắt |
| NoiDung | nvarchar(max) | | Nội dung |
| PathPhoto | nvarchar(500) | | Ảnh |
| AltText | nvarchar(200) | | Alt text ảnh |
| TacGia | nvarchar(100) | | Tác giả |
| NgayDang | datetime | | Ngày đăng |
| NoiBat | bit | | Nổi bật (default: 0) |
| SapXep | int | | Thứ tự sắp xếp |
| HienThi | bit | | Hiển thị (default: 1) |
| Remove | bit | | Flag xóa mềm (default: 0) |
| CreatedAt | datetime | | Ngày tạo |
| UpdatedAt | datetime | | Ngày cập nhật |

---

### 2.3.12 HoaDonNhap (HoaDonNhap)

**Mô tả:** Hóa đơn nhập nguyên liệu

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| HdnId | int | PK | Khóa chính |
| NccId | int | FK | Nhà cung cấp |
| NvId | int | FK | Nhân viên nhập |
| NgayLapHoaDon | datetime | | Ngày lập |
| NgayNhanHang | datetime | | Ngày nhận |
| TongSoTien | money | | Tổng số tiền |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có 1 NhaCungCap (N:1)
- Có 1 NhanVien (N:1)
- Có nhiều ChiTietHoaDonNhap (1:N)

---

### 2.3.13 NguyenLieu (NguyenLieu)

**Mô tả:** Nguyên liệu

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| NlId | int | PK | Khóa chính |
| TenNguyenLieu | nvarchar(100) | | Tên nguyên liệu |
| GiaTien | money | | Giá tiền (đơn vị nhập) |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có nhiều ChiTietHoaDonNhap (1:N)
- Có nhiều CongThuc (1:N)

---

### 2.3.14 ChiTietHoaDonNhap (ChiTietHoaDonNhap)

**Mô tả:** Chi tiết hóa đơn nhập

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| CthdnId | int | PK | Khóa chính |
| HdnId | int | FK | Hóa đơn nhập |
| NlId | int | FK | Nguyên liệu |
| SoLuong | int | | Số lượng |
| ThanhTien | money | | Thành tiền |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Trigger:**
- `cau7`: Tự động tính toán

---

### 2.3.15 CongThuc (CongThuc)

**Mô tả:** Công thức chế biến (sản phẩm需要什么 nguyên liệu)

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| CtId | int | PK | Khóa chính |
| ProductId | int | FK | Sản phẩm |
| NlId | int | FK | Nguyên liệu |
| SoLuong | decimal(18,2) | | Số lượng nguyên liệu |
| GhiChu | nvarchar(200) | | Ghi chú |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có 1 Product (N:1)
- Có 1 NguyenLieu (N:1)

---

### 2.3.16 NhaCungCap (NhaCungCap)

**Mô tả:** Nhà cung cấp nguyên liệu

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| NccId | int | PK | Khóa chính |
| TenNhaCungCap | nvarchar(100) | | Tên nhà cung cấp |
| DiaChi | nvarchar(200) | | Địa chỉ |
| Phone | nvarchar(15) | | Số điện thoại |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có nhiều HoaDonNhap (1:N)

---

### 2.3.17 KhuyenMai (KhuyenMai)

**Mô tả:** Chương trình khuyến mãi

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| KmId | int | PK | Khóa chính |
| TenKhuyenMai | nvarchar(100) | | Tên khuyến mãi |
| GiamGia | decimal(18,2) | | Phần trăm giảm giá |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có nhiều DonHang (1:N)

---

### 2.3.18 PhanQuyen (PhanQuyen)

**Mô tả:** Phân loại quyền hạn

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| PqId | int | PK | Khóa chính |
| TenQuyen | nvarchar(100) | | Tên quyền (admin, manager, staff) |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có nhiều NvPq (1:N)

---

### 2.3.19 NvPq (NV_PQ)

**Mô tả:** Bảng ánh xạ nhân viên - phân quyền (bridge table)

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| NvPqId | int | PK | Khóa chính |
| NvId | int | FK | Nhân viên |
| PqId | int | FK | Phân quyền |
| MoTa | nvarchar(200) | | Mô tả |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có 1 NhanVien (N:1)
- Có 1 PhanQuyen (N:1)

---

### 2.3.20 NgayCong (NgayCong)

**Mô tả:** Ngày công - theo dõi ca làm việc

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| NcId | int | PK | Khóa chính |
| NgayCong1 | date | | Ngày công |
| CaLam | nvarchar(50) | | Ca làm việc |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có nhiều NvNc (1:N)

---

### 2.3.21 NvNc (NV_NC)

**Mô tả:** Bảng ánh xạ nhân viên - ngày công (bridge table)

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| NvncId | int | PK | Khóa chính |
| NvId | int | FK | Nhân viên |
| NcId | int | FK | Ngày công |
| KiemTra | bit | | Kiểm tra (ghi công) |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có 1 NhanVien (N:1)
- Có 1 NgayCong (N:1)

---

### 2.3.22 Thuong (Thuong)

**Mô tả:** Thưởng cho nhân viên

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| TId | int | PK | Khóa chính |
| NvId | int | FK | Nhân viên |
| TenThuong | nvarchar(100) | | Tên thưởng |
| TienThuong | money | | Tiền thưởng |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có 1 NhanVien (N:1)

---

### 2.3.23 ProductConditions (ProductConditions)

**Mô tả:** Điều kiện/tùy chọn sản phẩm (stored as JSON)

| Thuộc Tính | Type | Khóa | Mô Tả |
|------------|------|------|-------|
| ProductConditionId | int | PK | Khóa chính |
| ProductId | int | FK | Sản phẩm |
| Condition | nvarchar(max) | | Điều kiện (JSON) |
| Remove | bit | | Flag xóa mềm (default: 0) |

**Quan hệ:**
- Có 1 Product (N:1)

**Ví dụ Condition JSON:**
```json
[
  {"name": "Size", "options": [{"label": "S", "extra": 0}, {"label": "L", "extra": 10000}]},
  {"name": "Sữa", "options": [{"label": "Sữa tươi", "extra": 5000}]}
]
```

---

## 2.4 Soft Delete Pattern

Tất cả các entity đều có field `Remove` (bit/boolean) để thực hiện **soft delete**. Khi xóa, hệ thống chỉ set `Remove = true` thay vì xóa hẳn record.

**Lợi ích:**
- Dễ dàng khôi phục dữ liệu
- Giữ lịch sử dữ liệu
- Audit trail

**Triển khai trong EF Core:**
```csharp
entity.Property(e => e.Remove).HasDefaultValue(false);
```

**Query phải lọc:**
```csharp
var products = await _context.Products
    .Where(p => !p.Remove)
    .ToListAsync();
```

---

## 2.5 Chứa Trigger Trong Database

Một số bảng có trigger SQL được định nghĩa trực tiếp trong database:

| Bảng | Trigger | Mô Tả |
|------|---------|-------|
| ChiTietHoaDon | cau1 | Tính toán thành tiền |
| ChiTietHoaDon | cau6 | Cập nhật tổng đơn hàng |
| ChiTietHoaDonNhap | cau7 | Tính toán hóa đơn nhập |

---

*Tài liệu tiếp tục ở phần 3...*