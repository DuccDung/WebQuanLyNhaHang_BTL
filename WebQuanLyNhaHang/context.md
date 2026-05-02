# WebQuanLyNhaHang Project Context

## Scope

This file records the current understanding of the ASP.NET Core MVC restaurant project, especially the customer-facing online ordering flow.

Main project folder:

- `WebQuanLyNhaHang`

Main online-ordering areas:

- `Controllers/HomeController.cs`
- `Views/Shared/_CloudyCafeLayout.cshtml`
- `Views/TrangChu/Menu.cshtml`
- `Views/Home/OnlineCart.cshtml`
- `Views/Home/OnlineCTDHTable.cshtml`
- `Views/Home/OrderHistory.cshtml`
- `Models/OnlineOrderMetadata.cs`
- `Models/OnlineOrderInfo.cs`
- `Models/OnlineOrderStatusHistory.cs`
- `ViewModel/ViewModelCart.cs`
- `ViewModel/OnlineCheckoutForm.cs`
- `ViewModel/OnlineOrderHistoryViewModel.cs`
- `Program.cs`

## Customer Login

Customer online login is handled in `HomeController.CustomerLogin`.

Current behavior:

- Finds `KhachHang` by `TaiKhoan`, `MatKhau`, and `Remove == false`.
- On success, stores the customer id in session key `CustomerID`.
- Also stores a protected cookie named `CloudyCafeCustomer`.
- Redirects to `TrangChu/Index`.

Persistent login:

- `Program.cs` restores `CustomerID` from the protected `CloudyCafeCustomer` cookie when session is empty.
- The cookie is protected with purpose `CloudyCafe.CustomerCookie.v1`.
- Invalid or deleted customers cause the cookie to be removed.

Logout:

- `HomeController.CustomerLogout` clears `CustomerID`, deletes the cookie, removes `OnlineCartDhId`, and redirects to `TrangChu/Index`.

## Header And Account Menu

The online customer layout is `Views/Shared/_CloudyCafeLayout.cshtml`.

Header behavior:

- Reads `CustomerID` from session.
- Loads the current `KhachHang`.
- If `KhachHang.PathPhoto` exists, displays it as the user avatar.
- Otherwise displays the default user icon.
- Account hover menu links to:
  - `Home/Account`
  - `Home/OrderHistory`
  - `Home/CustomerLogout`

Cart count behavior:

- Counts active online cart items for the logged-in customer.
- Query condition:
  - `ChiTietHoaDon.Remove == false`
  - `DonHang.Remove == false`
  - `DonHang.KhId == CustomerID`
  - `DonHang.VanChuyen == true`
  - `DonHang.TrangThai != true`
  - `DonHang.GhiChu` contains `"t":"online"`
  - `DonHang.GhiChu` contains `"s":"cart"`

Floating delivery button:

- Appears if the customer has an active online delivery order.
- Uses recent online orders where status is not `delivered`, `cancelled`, or `cart`.
- Links to `Home/OrderHistory`.

## Product Menu

Customer online menu is `Views/TrangChu/Menu.cshtml`.

Behavior:

- Uses `_CloudyCafeLayout`.
- Displays product categories and products.
- Product data is serialized into `ViewData["ProductModalJson"]`.
- The layout reads this JSON and renders the online order product modal.
- The product modal posts to `Home/CreateOnlineProductDetail`.

Product options:

- Quantity.
- Product condition from `ProductConditions`.
- Size M/L.
- Sugar level.
- Ice/hot.
- Toppings.

Pricing rule:

- Base price comes from `Product.GiaTien`.
- `ResolveOptionExtra` adds:
  - `Size L`: `+10000`
  - `sua tuoi`: `+5000`
  - `sua yen mach`: `+5000`
  - `sua dac`: `+5000`
  - `foam dua`: `+10000`

## Online Cart Creation

Adding online items is handled by `HomeController.CreateOnlineProductDetail`.

Preconditions:

- Requires `CustomerID` in session.
- If no customer is logged in, redirects to `Home/Index` with an error.
- Product must exist and `Remove == false`.

Order creation:

- Uses `GetOrCreateOnlineCartOrder`.
- If no active online cart exists, creates a new `DonHang`.

New online cart `DonHang` values:

- `GioVao = DateTime.Now`
- `TongTien = 0`
- `KhId = CustomerID`
- `BanId = null`
- `GhiChu = OnlineOrderMetadata.CreateCart().ToJson()`
- `TrangThai = false`
- `VanChuyen = true`

Session:

- Active online cart id is stored in session key `OnlineCartDhId`.

Cart item behavior:

- Items are stored in `ChiTietHoaDon`.
- If the same product with the same note already exists, quantity is increased.
- Otherwise a new `ChiTietHoaDon` row is created.
- `ThanhTien` is recalculated based on unit price and quantity.
- `RefreshCartTotal(DH.DhId)` recalculates `DonHang.TongTien`.
- SignalR event `OnlineCartUpdated` is sent.

## Active Online Cart Detection

The active cart is found by `HomeController.GetActiveOnlineCartOrder`.

Rules:

- Customer must be logged in.
- First checks `OnlineCartDhId` from session.
- The session order must belong to the current customer and not be removed.
- The order must satisfy `OnlineOrderMetadata.IsOnlineCart`.
- If session cart is invalid, session key is removed.
- Then it searches the database for the latest matching online cart.

Database search conditions:

- `KhId == CustomerID`
- `Remove == false`
- `VanChuyen == true`
- `TrangThai != true`
- `GhiChu` contains `"t":"online"`
- `GhiChu` contains `"s":"cart"`

## Online Cart Page

Main page:

- `Views/Home/OnlineCart.cshtml`

Partial:

- `Views/Home/OnlineCTDHTable.cshtml`

Controller action:

- `HomeController.OnlineCart`

Behavior:

- Loads the active online cart.
- Sets `ViewData["OnlineCartId"]`.
- Uses `ViewModelCart`.
- Renders the online cart partial.

Online cart partial:

- Uses `Model.CTHD_PctByDh(DhId)` for cart items.
- Uses `Model.TongtienById(DhId)` for total.
- Uses `Model.BuildCheckoutForm(DhId, customerId)` to prefill checkout form.
- Empty cart shows a link back to `TrangChu/Menu`.

Cart actions:

- Remove item: `HomeController.RemoveOnlineItem`.
- Update quantity: `HomeController.UpdateOnlineItemQuantity`.
- Refresh partial: `HomeController.GetOnlineCTHD`.

Frontend behavior:

- Uses `fetch` to call remove/update actions.
- Replaces `#online-cart-content` with returned partial HTML.
- Updates `.cart-count` badges from `data-cart-count`.
- Listens to SignalR event `OnlineCartUpdated`.

## Checkout Online Order

Checkout is handled by `HomeController.CheckoutOnlineOrder`.

Input model:

- `ViewModel/OnlineCheckoutForm.cs`

Fields:

- `HoTen`
- `SoDienThoai`
- `TinhThanh`
- `QuanHuyen`
- `PhuongXa`
- `DiaChi`
- `GhiChu`

Validation:

- Requires recipient name.
- Requires phone.
- Phone digits must be from 9 to 11 digits.
- Requires city/province.
- Requires district.
- Requires address line.

Checkout preconditions:

- Active online cart must exist.
- Cart must have at least one non-removed item.
- Customer must be logged in.
- Customer must exist and not be removed.

On successful checkout:

- Updates customer:
  - `TenKhachHang`
  - `SoDienThoai`
  - `DiaChi`
- Updates `DonHang`:
  - `KhId = customer.KhId`
  - `GhiChu = metadata.ToJson()`
  - `TrangThai = true`
  - `VanChuyen = true`
  - `BanId = null`
  - `GioVao` set if null
  - `GioRa = DateTime.Now`
- Metadata status changes from `cart` to `pending`.
- Creates or updates `OnlineOrderInfo`.
- Sets `OnlineOrderInfo.TrangThaiGiaoHang = pending`.
- Recalculates total with `RefreshCartTotal`.
- Removes `OnlineCartDhId` session key.
- Sends SignalR events:
  - `OderSuccess`
  - `OnlineOrderCreated`
- Redirects back to `Home/OnlineCart`.

## Online Order Metadata

Class:

- `Models/OnlineOrderMetadata.cs`

Purpose:

- Stores compact JSON metadata inside `DonHang.GhiChu`.
- Used to identify online orders and cart/order status.

JSON fields:

- `t`: type, expected `online`.
- `s`: delivery status.
- `n`: recipient name.
- `p`: phone.
- `c`: city.
- `d`: district.
- `w`: ward.
- `a`: address line.
- `note`: delivery note.
- `at`: submitted date/time.

Statuses:

- `cart`
- `pending`
- `preparing`
- `shipping`
- `delivered`
- `cancelled`

Important helpers:

- `CreateCart()`
- `TryParse(string?)`
- `IsOnlineOrder(DonHang)`
- `IsOnlineCart(DonHang)`
- `NormalizeStatus(string?)`
- `ResolveStatusDisplay(string?)`
- `ToJson()`

Note:

- `ToJson()` keeps the JSON under the `DonHang.GhiChu` max length by trimming fields.

## Online Order Info

Model:

- `Models/OnlineOrderInfo.cs`

Purpose:

- Stores normalized delivery information for online orders.

Fields:

- `OnlineOrderInfoId`
- `DhId`
- `CuaHangId`
- `TrangThaiGiaoHang`
- `NguoiNhan`
- `SoDienThoai`
- `TinhThanh`
- `QuanHuyen`
- `PhuongXa`
- `DiaChi`
- `GhiChuGiaoHang`
- `PhiGiaoHang`
- `PhuongThucThanhToan`
- `TrangThaiThanhToan`
- `NgayDat`
- `NgayCapNhat`
- `Remove`

Relationship:

- One `DonHang` has one `OnlineOrderInfo`.

## Online Order Status History

Model:

- `Models/OnlineOrderStatusHistory.cs`

Purpose:

- Intended to track status changes of online orders.

Fields:

- `HistoryId`
- `DhId`
- `TrangThaiCu`
- `TrangThaiMoi`
- `NvId`
- `GhiChu`
- `CreatedAt`

Current observation:

- The model and DbSet exist.
- It is mapped in `QlnhaHangBtlContext`.
- The customer history page listens to SignalR event `OnlineOrderStatusUpdated`.
- The current customer checkout flow does not write status history.
- Further admin/employee status update flow should write to this table.

## Online Order History

Controller action:

- `HomeController.OrderHistory`

View:

- `Views/Home/OrderHistory.cshtml`

View model:

- `ViewModel/OnlineOrderHistoryViewModel.cs`

Query conditions:

- Customer must be logged in.
- Reads `DonHang` where:
  - `KhId == CustomerID`
  - `Remove == false`
  - `VanChuyen == true`
  - `GhiChu` contains `"t":"online"`
- Includes:
  - non-removed `ChiTietHoaDons`
  - `Product`
  - `OnlineOrderInfo`
- Orders by `GioVao ?? GioRa`, then `DhId`.
- Excludes metadata status `cart`.

History row construction:

- Uses `BuildOnlineOrderHistoryRow`.
- Status source priority:
  1. `OnlineOrderInfo.TrangThaiGiaoHang`
  2. `OnlineOrderMetadata.DeliveryStatus`
- Address source priority:
  1. `OnlineOrderInfo`
  2. `OnlineOrderMetadata.FullAddress`
- Items are read from `ChiTietHoaDon`.

Customer-facing status labels:

- `pending`: `Chờ xác nhận`
- `preparing`: `Đang chuẩn bị`
- `shipping`: `Đang giao`
- `delivered`: `Đã giao`
- `cancelled`: `Đã hủy`

History UI:

- Shows a progress indicator for active orders.
- Delivered orders are compact and expandable.
- Paginates client-side with page size `5`.
- Reloads page when SignalR event `OnlineOrderStatusUpdated` arrives.

## Database Tables Used By Online Ordering

Main existing tables:

- `KhachHang`
- `DonHang`
- `ChiTietHoaDon`
- `Product`
- `ProductConditions`
- `OnlineOrderInfo`
- `OnlineOrderStatusHistory`

Important `DonHang` meaning in online flow:

- `VanChuyen = true`: online/delivery order.
- `TrangThai = false`: active cart before checkout.
- `TrangThai = true`: submitted order after checkout.
- `BanId = null`: online order, not table order.
- `GhiChu` contains compact online metadata JSON.

Important `ChiTietHoaDon` meaning:

- Each row is one product line.
- `Ghichu` stores option text such as condition, size, sugar, ice, topping.
- `ThanhTien` stores line total, not unit price.

## Current Design Concern

The online order flow currently uses two status sources:

- `DonHang.GhiChu` JSON via `OnlineOrderMetadata`.
- `OnlineOrderInfo.TrangThaiGiaoHang`.

This works, but it is not ideal long term.

Recommended direction:

- Use `OnlineOrderInfo.TrangThaiGiaoHang` as the primary status source.
- Keep `DonHang.GhiChu` only for backward compatibility or simple metadata.
- Avoid relying on `GhiChu.Contains(...)` for important filtering if new SQL columns are available.
- Always write status transitions to `OnlineOrderStatusHistory`.

## Suggested SQL Direction For Cleaner Online Orders

If continuing to improve online ordering, prefer adding explicit fields instead of relying on JSON inside `DonHang.GhiChu`.

Suggested columns on `DonHang`:

```sql
ALTER TABLE DonHang
ADD
    LoaiDonHang NVARCHAR(20) NULL,
    TrangThaiDon NVARCHAR(30) NULL,
    NgayDatOnline DATETIME NULL;
```

Suggested data meaning:

- `LoaiDonHang = 'Online'`
- `TrangThaiDon = 'Cart' | 'Pending' | 'Preparing' | 'Shipping' | 'Delivered' | 'Cancelled'`
- `NgayDatOnline`: when the customer submitted checkout.

Suggested index:

```sql
CREATE INDEX IX_DonHang_OnlineHistory
ON DonHang (KhId, LoaiDonHang, TrangThaiDon, NgayDatOnline DESC);
```

If using the existing `OnlineOrderInfo` table only, then at minimum add/confirm indexes:

```sql
CREATE INDEX IX_DonHang_KhId_VanChuyen_TrangThai
ON DonHang (KhId, VanChuyen, TrangThai, GioVao DESC, DH_ID DESC);

CREATE INDEX IX_OnlineOrderInfo_DhId
ON OnlineOrderInfo (DH_ID);

CREATE INDEX IX_OnlineOrderInfo_Status
ON OnlineOrderInfo (TrangThaiGiaoHang, NgayDat DESC);
```

## Known UI Behavior

Online pages use `wwwroot/cloudycafe/css/styles.css`.

Recent relevant UI expectations:

- User avatar should be circular.
- If `PathPhoto` exists, it replaces the default user icon.
- Cart button text should be smaller and not too bold.
- Online cart page inherits `_CloudyCafeLayout`.
- Online cart quantity `+` and `-` buttons are clickable.
- Menu category sidebar has active orange background while scrolling.
- Product cards are compact with smaller plus button.

## Build Note

Normal build can fail if the running app locks output files.

Safer build command:

```powershell
dotnet build "WebQuanLyNhaHang\WebQuanLyNhaHang.csproj" -o "build-check\<name>" /p:UseAppHost=false
```

After checking, remove the temporary output:

```powershell
Remove-Item -LiteralPath "build-check\<name>" -Recurse -Force
```
