# Context Du An WebQuanLyNhaHang

## 1. Tong quan

Day la du an web quan ly nha hang viet bang ASP.NET Core MVC tren .NET 8.
Du an co 2 luong chinh:

- Luong khach hang: dang nhap/dang ky, vao ban, xem menu, them mon, xem gio hang, gui yeu cau goi mon.
- Luong quan tri/noi bo: nhan vien dang nhap, xem tinh trang ban, xem don theo ban, xu ly thanh toan, quan ly san pham, nhan vien, khach hang.

He thong dung:

- ASP.NET Core MVC
- Entity Framework Core + SQL Server
- Session de luu trang thai tam thoi
- SignalR de cap nhat realtime giua client va admin

## 2. Diem khoi dong va cau hinh

File khoi dong la `Program.cs`.
Trong do dang ky:

- `AddControllersWithViews()`
- `AddSession(...)`
- `AddSignalR()`
- `AddDbContext<QlnhaHangBtlContext>(...)`

Pipeline chinh:

- `UseHttpsRedirection()`
- `UseStaticFiles()`
- `UseRouting()`
- `UseSession()`
- `UseCookiePolicy()`
- `UseAuthorization()`

Route mac dinh:

- `{controller=Home}/{action=Index}/{id?}`

SignalR hub:

- `/chatHub`

## 3. Cau truc thu muc chinh

- `Controllers/`: action xu ly nghiep vu va dieu huong
- `Models/`: cac entity DB-first va `QlnhaHangBtlContext`
- `ViewModel/`: gom logic tong hop du lieu cho giao dien
- `Views/`: giao dien Razor
- `Views/Shared/Components/`: cac ViewComponent de render category, quantity selector, condition...
- `Hubs/`: SignalR hub
- `wwwroot/`: CSS, JS, hinh anh, theme admin, asset frontend

## 4. Controller chinh va vai tro

### `HomeController`

Day la controller quan trong nhat cho phia khach hang.

Chuc nang chinh:

- `Index()`: trang dang nhap/dang ky khach hang
- `Client(BanId)`: luu `BanId` vao session, tao `DonHang` moi, luu `DhId` vao session
- `Service()`: trang chuc nang tai ban
- `Menu()`: hien thi menu mon an/do uong
- `GetName(txtsearch)`: tim kiem mon an theo ten, tra ve partial view
- `ProductDetail(ProductID)`: xem chi tiet mon va so luong hien tai trong don
- `CreateProductDetail(...)`: them mon vao `ChiTietHoaDon`
- `Cart(...)`: hien thi gio hang cua don hien tai
- `OrderSuccess()`: gan `BanId` vao `DonHang`, phat su kien SignalR `OderSuccess`
- `RemoveItem(id)`: xoa chi tiet hoa don trong gio
- `GetCTHD()`: tra partial view de reload gio hang
- `CustomerRegister(...)`, `CustomerLogin(...)`: dang ky/dang nhap khach hang

### `AdminController`

Day la controller chinh cho giao dien admin/noi bo.

Chuc nang chinh:

- `Login()`: dang nhap nhan vien
- `Index()`: dashboard admin
- `Ban()`: hien thi danh sach ban va so don theo ban
- `GetFormBuy(id)`: tai thong tin don cua mot ban bang AJAX
- `ProcessPayment(BanId)`: xu ly thanh toan theo cach xoa lien ket `BanId` khoi cac don hang cua ban do

### Cac controller scaffold CRUD

Du an con co cac controller scaffold san:

- `ProductsController`
- `DonHangsController`
- `ChiTietHoaDonsController`
- `KhachHangsController`
- `NhanViensController`
- `ChartDataController`

Trong do:

- `ProductsController` da co upload anh vao `wwwroot/asset/img/imgWeb`
- `ChiTietHoaDonsController` co API AJAX tang/giam so luong va xoa mon

## 5. Luong nghiep vu khach hang

Luong hien tai minh hieu nhu sau:

1. Khach vao `Home/Index` de dang nhap hoac dang ky.
2. Khi vao theo ban, action `Client(BanId)` duoc goi.
3. He thong luu `BanId` vao session.
4. He thong tao `DonHang` moi, sau do luu `DhId` vao session.
5. Khach vao `Service`, tu do mo `Menu`.
6. `Menu` render danh muc va san pham theo category.
7. Neu bam dau `+` thi vao `ProductDetail`.
8. O `ProductDetail`, khach chon condition, ghi chu, so luong.
9. Khi submit, he thong them dong vao `ChiTietHoaDon`.
10. O `Cart`, khach xem mon da goi va tong tien.
11. Khi bam gui yeu cau goi mon, `OrderSuccess()` se gan `BanId` vao `DonHang`.
12. Admin nhan su kien SignalR va reload danh sach ban.

Session dang duoc dung cho:

- `BanId`
- `DhId`
- `CustomerName`
- `CustomerID`
- `NhanVienId`

## 6. Luong nghiep vu admin

Luong admin/noi bo hien tai:

1. Nhan vien dang nhap qua `Admin/Login`.
2. `NhanVienId` duoc luu vao session.
3. Vao `Admin/Ban` de xem toan bo ban.
4. Moi the ban hien thi so don hang dang lien ket voi ban do.
5. Khi click vao mot ban, frontend goi AJAX den `Admin/GetFormBuy`.
6. Partial view hien thi cac mon da goi cua ban.
7. Khi xu ly thanh toan, `ProcessPayment(BanId)` se dat `BanId = null` cho cac don cua ban do.

## 7. Model du lieu quan trong

Context database la `QlnhaHangBtlContext`, duoc scaffold theo kieu DB-first.

Nhung bang/thuc the chinh:

- `Ban`: thong tin ban
- `DonHang`: don hang tai ban
- `ChiTietHoaDon`: chi tiet mon trong don
- `Product`: san pham/mon an/do uong
- `Category`: loai san pham
- `ProductConditions`: tuy chon/condition cua mon
- `KhachHang`: tai khoan khach hang
- `NhanVien`: tai khoan nhan vien
- `KhuyenMai`
- `NguyenLieu`
- `CongThuc`
- `HoaDonNhap`
- `ChiTietHoaDonNhap`
- `NhaCungCap`
- `PhanQuyen`, `NvPq`
- `NgayCong`, `NvNc`
- `Thuong`

Quan he nghiep vu quan trong:

- `DonHang` - `ChiTietHoaDon`: 1-n
- `Product` - `ChiTietHoaDon`: 1-n
- `Category` - `Product`: 1-n
- `Product` - `ProductConditions`: 1-n
- `Ban` - `DonHang`: 1-n

Luu y:

- `ChiTietHoaDon` va `ChiTietHoaDonNhap` dang co trigger trong DB.
- `ThanhTien`, `TongTien`, tong hop gia tri co kha nang dang duoc cap nhat boi trigger SQL.

## 8. ViewModel va ViewComponent

### ViewModel

`ViewModel` dang dong vai tro tong hop du lieu thay vi de logic nam nhieu trong controller.

Quan trong nhat:

- `ViewModelMenu`: lay danh muc, san pham theo danh muc, tim kiem khong dau, dem so mon trong don
- `ViewModelCart`: join `ChiTietHoaDon` voi `Product`, lay tong tien don
- `ViewModelBan`: render danh sach ban va dem so don theo ban
- `ViewModelGetFormBuy`: join du lieu ban -> don hang -> chi tiet -> san pham
- `ViewModelProductDetail`: lay san pham va danh sach `ProductConditions`

### ViewComponent

He thong su dung kha nhieu ViewComponent:

- `Category`: render thanh category
- `MenuAddSub`: render dau `+` hoac bo tang/giam
- `ProductCondition`: render danh sach option radio
- `AccLoggin`: render thong tin nhan vien dang nhap

## 9. Realtime voi SignalR

Hub dung la `ChatHub`.
Nhung event chinh:

- `DatabaseUpdated`
- `ProductDeleted`
- `OderSuccess`

Tac dung:

- Gio hang co the reload khi chi tiet hoa don thay doi
- Trang admin ban co the reload khi khach gui don thanh cong

Luu y:

- Ten event dang viet la `OderSuccess`, co the la danh may cua `OrderSuccess`, nhung hien tai code client va server dang dung cung mot ten nen van chay.

## 10. Frontend

Du an co 2 bo giao dien kha tach biet:

- Frontend khach hang: nam nhieu trong `wwwroot/asset/...`
- Dashboard admin: dung mot theme admin co san trong `wwwroot/assets/...`

View admin su dung layout rieng:

- `Views/Shared/Header_Left_Layout.cshtml`
- `Views/Shared/Head_Layout.cshtml`

View khach hang nhieu man dang tu viet rieng va khong phu thuoc layout tong qua nhieu.

## 11. Trang thai ky thuat hien tai

Minh da chay `dotnet build` va ket qua:

- Build thanh cong
- 0 error
- 36 warning

Nhung warning chu yeu:

- Nullable reference warnings
- Mot so `ViewComponent` khai bao `async` nhung khong `await`
- Mot so cho co nguy co dereference gia tri null

## 12. Nhung diem can luu y

### Bao mat

Day la phan can chu y nhat:

- Chuoi ket noi SQL Server dang hard-code trong `appsettings.json`
- Trong chuoi ket noi dang co ca `User ID=sa` va password
- Mat khau nhan vien va khach hang dang luu/plain text
- Dang nhap dang so sanh truc tiep chuoi mat khau
- Chua thay dung ASP.NET Identity
- Chua thay `[Authorize]` de bao ve route admin

### Logic/nghiep vu

- `Client(BanId)` moi lan vao co the tao `DonHang` moi, nen can de y viec tao don trung lap.
- `OrderSuccess()` dang gan `BanId` cho `DonHang` sau khi khach gui don.
- `ProcessPayment(BanId)` khong dong don theo nghia nghiep vu, ma chi bo lien ket ban khoi don.
- Tim kiem mon trong `ViewModelMenu` dang dung `.AsEnumerable()`, tuc la keo du lieu ve memory roi moi loc.

### Bug/rui ro de y

- Trong `CustomerRegister`, code kiem tra trung `TaiKhoan` bang `username` nhung luc luu lai gan `TaiKhoan = Email`, logic nay khong dong nhat.
- Co kha nhieu cho phu thuoc session; neu session mat hoac het han co the phat sinh loi null.
- Co mot so cho frontend reload bang AJAX/SignalR nhung response va partial view chua that su gon.

## 13. Cach minh dang hieu kien truc du an

Neu tom gon bang 1 cau:

Day la he thong quan ly nha hang theo mo hinh MVC, trong do phia khach hang dat mon tai ban bang session + gio hang tam thoi, phia admin theo doi ban va don theo thoi gian thuc bang SignalR, con du lieu duoc scaffold tu SQL Server theo huong DB-first.

## 14. Neu can lam tiep

Neu sau nay can tiep tuc doc/sua du an, minh nen uu tien cac huong sau:

1. Lam ro nghiep vu `DonHang` va `ChiTietHoaDon` trong database, nhat la trigger.
2. Kiem tra va bo sung bao mat dang nhap/phan quyen.
3. Chuan hoa session flow cua khach hang.
4. Giam logic trong view/component, dua ve service hoac application layer ro rang hon.
5. Tach thong tin nhay cam khoi `appsettings.json`.

