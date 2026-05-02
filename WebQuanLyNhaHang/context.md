# WebQuanLyNhaHang - System Context

Cap nhat: 2026-05-02

## 1. Tong Quan He Thong

`WebQuanLyNhaHang` la ung dung ASP.NET Core MVC quan ly nha hang/quan cafe, target `.NET 8`.

He thong gom 2 vung chinh:

- Website khach hang: trang chu, menu, chi tiet san pham, gio hang, dat mon online, lich su don hang, dang nhap/dang ky tai khoan khach.
- Khu quan tri: dashboard, ban an, don hang, san pham, khach hang, nhan vien, cua hang, bai viet/chuyen nha.

Cong nghe chinh:

- ASP.NET Core MVC + Razor Views.
- Entity Framework Core 8 + SQL Server.
- Session-based authentication cho admin/nhan vien va khach hang.
- SignalR tai endpoint `/chatHub` de cap nhat realtime mot so UI.
- Static assets trong `wwwroot`.

Solution/project:

- Solution: `WebQuanLyNhaHang.sln`
- Project chinh: `WebQuanLyNhaHang/WebQuanLyNhaHang.csproj`
- DbContext: `Models/QlnhaHangBtlContext.cs`
- Default route: `{controller=TrangChu}/{action=Index}/{id?}`

## 2. Cau Truc Thu Muc Quan Trong

- `Controllers`: controller MVC cho website khach hang, admin, CRUD va API JSON phuc vu UI.
- `Models`: entity EF Core, metadata helpers, cac class gan voi bang SQL.
- `ViewModel`: model rieng cho tung man hinh Razor.
- `Views`: Razor views theo controller.
- `Views/Shared`: layout chung, sidebar admin, layout CloudyCafe, view components.
- `Filters`: filter bao ve khu admin bang session.
- `Hubs`: SignalR hub.
- `wwwroot`: CSS/JS/image/vendor assets.

File can nam:

- `Program.cs`: dang ky MVC, session, SignalR, DbContext, route, middleware khoi phuc customer session tu cookie.
- `Filters/AdminSessionAuthorizeAttribute.cs`: bao ve admin/staff action dua tren session `NhanVienId`.
- `Hubs/ChatHub.cs`: gui su kien realtime nhu `ReceiveMessage`, `DatabaseUpdated`, `ProductDeleted`, `OderSuccess`.
- `Models/OnlineOrderMetadata.cs`: helper doc/ghi JSON compact trong `DonHang.GhiChu` cho don online.
- `Models/OnlineOrderInfo.cs`: thong tin giao hang da duoc normalize.
- `Models/OnlineOrderStatusHistory.cs`: lich su thay doi trang thai giao hang.

## 3. Startup Va Middleware

`Program.cs` dang ky:

- `AddControllersWithViews()`
- `AddSession()` voi timeout 30 phut.
- `AddSignalR()`
- `AddDbContext<QlnhaHangBtlContext>()` dung connection string key `QlnhaHangBtlContext`.

Pipeline:

- Exception handler/HSTS khi khong phai development.
- HTTPS redirection.
- Static files.
- Routing.
- Session.
- Middleware tu tao de khoi phuc `CustomerID` tu cookie `CloudyCafeCustomer`.
- Cookie policy.
- Authorization.
- Default MVC route.
- SignalR hub `/chatHub`.

Cookie khach hang:

- Ten cookie: `CloudyCafeCustomer`.
- Purpose bao ve du lieu: `CloudyCafe.CustomerCookie.v1`.
- Neu cookie hop le va khach con ton tai, middleware set lai session `CustomerID`.
- Neu cookie loi hoac khach da bi soft-delete, cookie bi xoa.

## 4. Database Va Entity Chinh

DbContext: `QlnhaHangBtlContext`.

Bang/entity nghiep vu chinh:

- `Product`, `Category`, `ProductConditions`: san pham, nhom san pham, tuy chon san pham.
- `DonHang`, `ChiTietHoaDon`: don hang va chi tiet don.
- `Ban`: ban an tai quan.
- `KhachHang`: tai khoan/thong tin khach hang.
- `NhanVien`, `PhanQuyen`, `NvPq`, `NgayCong`, `NvNc`, `Thuong`: nhan vien, phan quyen va thong tin lien quan.
- `NguyenLieu`, `CongThuc`, `NhaCungCap`, `HoaDonNhap`, `ChiTietHoaDonNhap`: nhap hang/nguyen lieu.
- `CuaHang`: noi dung diem ban/cua hang hien thi public.
- `BaiVietChuyenNha`: bai viet/chuyen nha/news hien thi public.
- `OnlineOrderInfo`, `OnlineOrderStatusHistory`: thong tin va lich su giao hang online.

Quy uoc soft delete:

- Nhieu bang co cot `Remove`.
- Code thuong loc `Remove == false` thay vi xoa cung.

Luu y `DonHang`:

- `VanChuyen = true`: dung cho don online/giao hang.
- `TrangThai = false`: gio hang online dang active truoc checkout hoac don chua hoan tat tuy ngu canh.
- `TrangThai = true`: don da submit/hoan tat thanh toan theo luong hien co.
- `BanId = null`: don online khong gan ban.
- `GhiChu`: vua la ghi chu, vua dang chua JSON compact cho online order metadata.

## 5. Controllers Chinh

### Public / Customer

`TrangChuController`

- `Index`: trang chu public, lay san pham noi bat/hot categories.
- `Menu`: menu CloudyCafe cho khach hang.
- `DoUong`, `Banh`: loc nhom san pham theo loai.
- `ChuyenNha`: danh sach bai viet/chuyen nha dang hien thi.
- `CuaHang`: danh sach cua hang dang hien thi.

`HomeController`

- Trang mac dinh cu, dang nhap/dang ky khach, tai khoan khach.
- Flow order tai ban: `Client`, `ProductDetail`, `Cart`, `CreateProductDetail`, `OrderSuccess`.
- Flow online: `CreateOnlineProductDetail`, `OnlineCart`, `CheckoutOnlineOrder`, `OrderHistory`.
- Cart actions: remove, clear, update quantity, refresh partial.
- Customer session: `CustomerLogin`, `CustomerRegister`, `CustomerLogout`.

### Admin / Management

`AdminController`

- `Login`, `Logout`.
- `Index`: dashboard thong ke doanh thu, don hang, khach, nhan vien, san pham, ban.
- `Ban`: man hinh ban an.
- `GetFormBuy`: partial form goi mon/thanh toan cho ban.
- `ProcessPayment`: xu ly thanh toan ban, clear lien ket ban tren don active.

`DonHangsController`

- Danh sach don hang admin.
- JSON detail endpoints: `DetailsData`, `DetailsDataV2`.
- `UpdateDeliveryStatus`: cap nhat trang thai giao hang online, dong bo metadata, `OnlineOrderInfo`, ghi `OnlineOrderStatusHistory`, phat SignalR.
- CRUD co ban cho `DonHang`.

`ProductsController`

- Quan ly san pham, tao/sua/xoa, upload anh, update qua modal.

`KhachHangsController`

- Quan ly khach hang, tao modal, lay detail JSON, soft delete.

`NhanViensController`

- Quan ly nhan vien, update/delete modal, upload anh.

`ChiTietHoaDonsController`

- CRUD chi tiet hoa don va cac action tang/giam/xoa nhanh dong san pham.

`AdminCuaHangsController`

- Quan ly noi dung cua hang public.

`AdminBaiVietChuyenNhasController`

- Quan ly bai viet/chuyen nha/news public.

`ChartDataController`

- Endpoint view du lieu bieu do.

## 6. Authentication Va Session

Admin/nhan vien:

- Dang nhap qua `AdminController.Login`.
- Session chinh:
  - `NhanVienId`
  - `NhanVienName`
  - `NhanVienTaiKhoan`
- Action admin can bao ve dung `[AdminSessionAuthorize]`.
- Neu request la AJAX/JSON va chua login, filter tra `401` kem `redirectUrl`.
- Neu nhan vien khong ton tai hoac da `Remove`, filter xoa session va redirect ve login.

Khach hang:

- Dang nhap qua `HomeController.CustomerLogin`.
- Session chinh: `CustomerID`.
- Cookie `CloudyCafeCustomer` giup khoi phuc login sau khi mat session.
- Logout xoa `CustomerID`, xoa cookie, xoa `OnlineCartDhId`.

## 7. Layout Va UI Chinh

Layout public moi:

- `Views/Shared/_CloudyCafeLayout.cshtml`
- Dung cho cac trang CloudyCafe/menu/gio hang online/lich su don.
- Header doc `CustomerID` tu session, hien avatar tu `KhachHang.PathPhoto` neu co.
- Account hover menu link den account, order history, logout.
- Cart badge dem item trong gio online active.
- Co floating delivery button khi khach co don online dang xu ly.

Layout admin:

- `Views/Shared/_AdminContentLayout.cshtml`
- `Views/Shared/_AdminSidebar.cshtml`
- Cac trang admin quan ly bang layout/sidebar nay.

Static UI dang dung:

- Public CloudyCafe: `wwwroot/cloudycafe/css/styles.css`, `wwwroot/cloudycafe/js/product-modal.js`.
- Public cu: `wwwroot/asset/css/*`, `wwwroot/asset/js/*`.
- Admin: `wwwroot/assets/js/admin-*.js`, `wwwroot/assets/*`.

## 8. Luong Dat Mon Online

### Menu online

View:

- `Views/TrangChu/Menu.cshtml`

Behavior:

- Lay category/san pham active.
- San pham va option duoc serialize vao `ViewData["ProductModalJson"]`.
- Layout doc JSON nay de render product modal.
- Submit modal den `Home/CreateOnlineProductDetail`.

Option san pham:

- So luong.
- Product condition tu `ProductConditions`.
- Size M/L.
- Muc duong.
- Da/nong.
- Topping.

Quy tac cong gia trong code:

- Base price tu `Product.GiaTien`.
- `Size L`: +10000.
- `sua tuoi`, `sua yen mach`, `sua dac`: +5000.
- `foam dua`: +10000.

### Tao gio hang online

Action:

- `HomeController.CreateOnlineProductDetail`

Dieu kien:

- Phai co session `CustomerID`.
- Product phai ton tai va `Remove == false`.

Xu ly:

- Goi `GetOrCreateOnlineCartOrder`.
- Neu chua co cart active, tao `DonHang` moi.
- `DonHang` cart online moi:
  - `GioVao = DateTime.Now`
  - `TongTien = 0`
  - `KhId = CustomerID`
  - `BanId = null`
  - `GhiChu = OnlineOrderMetadata.CreateCart().ToJson()`
  - `TrangThai = false`
  - `VanChuyen = true`
- Luu cart id vao session `OnlineCartDhId`.
- Item nam trong `ChiTietHoaDon`.
- Neu cung product va cung ghi chu option thi tang so luong; neu khac thi tao dong moi.
- Sau moi thay doi goi `RefreshCartTotal`.
- Phat SignalR `OnlineCartUpdated` theo code hien co.

### Tim cart online active

Helper:

- `HomeController.GetActiveOnlineCartOrder`

Thu tu:

- Kiem tra session `OnlineCartDhId`.
- Dam bao order thuoc dung customer, chua remove, la online cart.
- Neu session cart khong hop le thi remove session key.
- Tim trong database order online cart moi nhat.

Dieu kien DB quan trong:

- `KhId == CustomerID`
- `Remove == false`
- `VanChuyen == true`
- `TrangThai != true`
- `GhiChu` co `"t":"online"`
- `GhiChu` co `"s":"cart"`

### Trang gio hang online

View:

- `Views/Home/OnlineCart.cshtml`
- Partial: `Views/Home/OnlineCTDHTable.cshtml`

Controller:

- `HomeController.OnlineCart`
- `HomeController.GetOnlineCTHD`
- `HomeController.RemoveOnlineItem`
- `HomeController.UpdateOnlineItemQuantity`

Behavior:

- Load active cart cua customer.
- Dung `ViewModelCart`.
- Partial render danh sach item, tong tien va form checkout.
- Empty cart hien link quay lai menu.
- JS dung `fetch` de remove/update va thay `#online-cart-content`.
- Cap nhat badge gio hang tu `data-cart-count`.

### Checkout online

Action:

- `HomeController.CheckoutOnlineOrder`

Input:

- `ViewModel/OnlineCheckoutForm.cs`

Field:

- `HoTen`
- `SoDienThoai`
- `TinhThanh`
- `QuanHuyen`
- `PhuongXa`
- `DiaChi`
- `GhiChu`

Validation:

- Bat buoc ten nguoi nhan.
- Bat buoc so dien thoai.
- So dien thoai chi lay digit va dai 9-11 so.
- Bat buoc tinh/thanh, quan/huyen, dia chi.

Khi checkout thanh cong:

- Cap nhat thong tin customer.
- Chuyen metadata tu `cart` sang `pending`.
- Cap nhat `DonHang`:
  - `KhId`
  - `GhiChu`
  - `TrangThai = true`
  - `VanChuyen = true`
  - `BanId = null`
  - `GioVao` neu dang null
  - `GioRa = DateTime.Now`
- Tao/cap nhat `OnlineOrderInfo`.
- Set `OnlineOrderInfo.TrangThaiGiaoHang = pending`.
- Tinh lai tong tien.
- Xoa session `OnlineCartDhId`.
- Phat SignalR:
  - `OderSuccess`
  - `OnlineOrderCreated`
- Redirect ve `Home/OnlineCart`.

## 9. Lich Su Don Online

Action:

- `HomeController.OrderHistory`

View:

- `Views/Home/OrderHistory.cshtml`

ViewModel:

- `ViewModel/OnlineOrderHistoryViewModel.cs`

Query:

- Yeu cau customer logged in.
- Lay `DonHang`:
  - `KhId == CustomerID`
  - `Remove == false`
  - `VanChuyen == true`
  - `GhiChu` co `"t":"online"`
- Include `ChiTietHoaDons`, `Product`, `OnlineOrderInfo`.
- Loai bo status `cart`.

Trang thai online:

- `cart`
- `pending`
- `preparing`
- `shipping`
- `delivered`
- `cancelled`

Nguon trang thai uu tien:

1. `OnlineOrderInfo.TrangThaiGiaoHang`
2. `OnlineOrderMetadata.DeliveryStatus`

Lich su trang thai:

- Admin update delivery status se ghi `OnlineOrderStatusHistory`.
- Checkout hien tai tao don pending nhung context cu ghi nhan checkout khong tao history row rieng.

## 10. Luong Ban An / Dine-in

Vung chinh:

- `HomeController.Client`
- `HomeController.ProductDetail`
- `HomeController.CreateProductDetail`
- `HomeController.Cart`
- `HomeController.OrderSuccess`
- `AdminController.Ban`
- `AdminController.GetFormBuy`
- `AdminController.ProcessPayment`

Y nghia chung:

- Khach/nhan vien chon ban va them mon vao don.
- `ChiTietHoaDon` luu tung dong san pham.
- Admin xem ban, lay form thanh toan, xu ly payment.
- `ProcessPayment` clear `BanId` tren active orders cua ban de ban khong con bi chiem.

Luu y:

- Trong `HomeController` co nested helper `DineInOrderMetadata` de serialize metadata order tai ban vao JSON.
- Mot so UI cu van nam trong `Views/Home/*` va static assets `wwwroot/asset/*`.

## 11. SignalR

Hub:

- `Hubs/ChatHub.cs`

Endpoint:

- `/chatHub`

Server methods hien co:

- `SendMessage`: broadcast `ReceiveMessage`.
- `NotifyDatabaseChange`: broadcast `DatabaseUpdated`.
- `NotifyProductDeleted`: broadcast `ProductDeleted`.
- `NotifyOderSuccess`: broadcast `OderSuccess`.

Controllers cung co the dung `IHubContext<ChatHub>` de gui event truc tiep, vi vay khi tim event can search trong controllers nua, khong chi trong `ChatHub.cs`.

Event da thay trong context/code:

- `OderSuccess`
- `OnlineCartUpdated`
- `OnlineOrderCreated`
- `OnlineOrderStatusUpdated`
- `ProductDeleted`
- `DatabaseUpdated`

## 12. Noi Dung Public: Cua Hang Va Bai Viet

`CuaHang`

- Quan ly trong `AdminCuaHangsController`.
- Hien thi public qua `TrangChu/CuaHang`.
- Field quan trong: ten, dia chi, tinh/thanh, quan/huyen, phuong/xa, phone, map url, lat/long, path photo, sap xep, hien thi, remove.

`BaiVietChuyenNha`

- Quan ly trong `AdminBaiVietChuyenNhasController`.
- Hien thi public qua `TrangChu/ChuyenNha`.
- Field quan trong: tieu de, slug, tom tat, noi dung, anh, tac gia, ngay dang, sap xep, noi bat, hien thi, remove.

## 13. ViewModel Dang Chu Y

- `AdminDashboardViewModel`: payload dashboard admin.
- `TrangChuMenuPageViewModel`: menu public CloudyCafe.
- `ViewModelCart`: gio hang/don hang va helper tinh tong.
- `OnlineCheckoutForm`: form checkout delivery.
- `OnlineOrderHistoryViewModel`: lich su don online.
- `ProductsIndexViewModel`, `CustomersIndexViewModel`, `EmployeesIndexViewModel`, `OrdersIndexViewModel`: danh sach admin co filter/paging theo tung module.
- `ViewModelBan`, `ViewModelGetFormBuy`, `BanDonHang`: nghiep vu ban/don tai quan.

## 14. Diem Can Can Than Khi Sua Code

- Repo dang co nhieu thay doi chua commit; can kiem tra diff truoc khi sua file lien quan.
- `rg.exe` tren may hien tai co luc bi `Access is denied`; neu search loi thi dung PowerShell `Get-ChildItem`/`Select-String`.
- Nhieu file co tieng Viet va mot so noi co dau bi mojibake trong tai lieu/cu; khi sua UI text can giu encoding dung UTF-8.
- Luong online order dang dung 2 nguon trang thai:
  - JSON trong `DonHang.GhiChu` qua `OnlineOrderMetadata`.
  - Cot `OnlineOrderInfo.TrangThaiGiaoHang`.
- Nen coi `OnlineOrderInfo.TrangThaiGiaoHang` la nguon chinh khi phat trien tiep, va chi giu `DonHang.GhiChu` de tuong thich nguoc.
- Dung `Remove == false` khi query cac entity co soft delete.
- Sau khi thay doi cart/order can tinh lai tong tien bang helper hien co, tranh chi update tung dong.
- Khi them/sua luong realtime, can dong bo ca server event va JS listener trong view/layout.
- Khong nen dua them logic quan trong vao `GhiChu.Contains(...)` neu co the them cot/index ro rang trong database.

## 15. Database/SQL Files Trong Repo

File SQL can biet:

- `data_system_restaurant_management.sql`: script du lieu/schema he thong lon.
- `database_online_content_update.sql`: update lien quan online/content.

Khi can cap nhat schema:

- Kiem tra entity trong `Models`.
- Kiem tra mapping trong `QlnhaHangBtlContext`.
- Cap nhat SQL script tuong ung neu thay doi database.

## 16. Build Note

Build thong thuong co the fail neu app dang chay va lock output.

Lenh build an toan hon:

```powershell
dotnet build "WebQuanLyNhaHang\WebQuanLyNhaHang.csproj" -o "build-check\<name>" /p:UseAppHost=false
```

Sau khi build xong co the xoa output tam:

```powershell
Remove-Item -LiteralPath "build-check\<name>" -Recurse -Force
```

## 17. Huong Cai Tien De Xuat

Neu tiep tuc cai tien luong online order, nen giam phu thuoc vao JSON trong `DonHang.GhiChu`.

De xuat cot ro rang tren `DonHang`:

```sql
ALTER TABLE DonHang
ADD
    LoaiDonHang NVARCHAR(20) NULL,
    TrangThaiDon NVARCHAR(30) NULL,
    NgayDatOnline DATETIME NULL;
```

Y nghia:

- `LoaiDonHang = 'Online'`
- `TrangThaiDon = 'Cart' | 'Pending' | 'Preparing' | 'Shipping' | 'Delivered' | 'Cancelled'`
- `NgayDatOnline`: thoi diem khach checkout.

Index goi y:

```sql
CREATE INDEX IX_DonHang_OnlineHistory
ON DonHang (KhId, LoaiDonHang, TrangThaiDon, NgayDatOnline DESC);

CREATE INDEX IX_OnlineOrderInfo_DhId
ON OnlineOrderInfo (DH_ID);

CREATE INDEX IX_OnlineOrderInfo_Status
ON OnlineOrderInfo (TrangThaiGiaoHang, NgayDat DESC);
```
