using Microsoft.CodeAnalysis.CSharp.Syntax;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public class ViewModelCart
    {
        private readonly QlnhaHangBtlContext _context;
        public ViewModelCart(QlnhaHangBtlContext context)
        {
            _context = context;
        }
        public List<CTDH_Product> CTHD_PctByDh(int? DhId) // Lấy Ctdh và product join và tìm kiếm nó theo đơn hàng; để phục vụ cart
        {
            if (!DhId.HasValue)
            {
                return new List<CTDH_Product>();
            }

            var result = from CTHD in _context.ChiTietHoaDons
                         join product in _context.Products
                         on CTHD.ProductId equals product.ProductId
                         where CTHD.DhId == DhId && !CTHD.Remove && !CTHD.Dh.Remove
                         select new CTDH_Product
                         {
                             ProductId = product.ProductId,
                             DhId = DhId,
                             CthdId = CTHD.CthdId,
                             PathPhoto = product.PathPhoto,
                             SoLuong = CTHD.SoLuong,
                             TenSanPham = product.TenSanPham,
                             ThanhTien = CTHD.ThanhTien,
                             Condition = CTHD.Ghichu
                         };
            if(result == null) // nếu đơn hàng về null mà ép Tolist(); nó sẽ bị bug => ta tạo 1 list mới
            {
                return new List<CTDH_Product>();
            }
            else // result != null trả về list 
            {
                return result.ToList();
            }
        }

        public DonHang TongtienById(int? DhId)
        {
            if (!DhId.HasValue)
            {
                return EmptyCartOrder();
            }

            var result = _context.DonHangs.FirstOrDefault(item => item.DhId == DhId && !item.Remove);
            if (result == null) {
                return EmptyCartOrder();
            }
            if(result.TongTien == null)
            {
                result.TongTien = 0;
            }
            return result;
        }

        public bool HasOrderItems(int? DhId)
        {
            return DhId.HasValue && _context.ChiTietHoaDons
                .Any(item => item.DhId == DhId.Value && !item.Remove && !item.Dh.Remove);
        }

        public List<CTDH_Product> SubmittedDineInItems(int? banId, string? guestId)
        {
            if (!banId.HasValue || string.IsNullOrWhiteSpace(guestId))
            {
                return new List<CTDH_Product>();
            }

            var customerMarker = $"\"g\":\"{guestId.Trim()}\"";
            var result = from CTHD in _context.ChiTietHoaDons
                         join product in _context.Products
                         on CTHD.ProductId equals product.ProductId
                         where CTHD.Dh.BanId == banId.Value
                               && CTHD.Dh.VanChuyen != true
                               && CTHD.Dh.TrangThai == true
                               && !CTHD.Dh.Remove
                               && !CTHD.Remove
                               && CTHD.Dh.GhiChu != null
                               && CTHD.Dh.GhiChu.Contains("\"t\":\"dinein\"")
                               && CTHD.Dh.GhiChu.Contains(customerMarker)
                         orderby CTHD.Dh.GioRa descending, CTHD.Dh.DhId descending, CTHD.CthdId
                         select new CTDH_Product
                         {
                             ProductId = product.ProductId,
                             DhId = CTHD.DhId,
                             CthdId = CTHD.CthdId,
                             PathPhoto = product.PathPhoto,
                             SoLuong = CTHD.SoLuong,
                             TenSanPham = product.TenSanPham,
                             ThanhTien = CTHD.ThanhTien,
                             Condition = CTHD.Ghichu
                         };

            return result.ToList();
        }

        public OnlineCheckoutForm BuildCheckoutForm(int? DhId, int? customerId)
        {
            var order = DhId.HasValue
                ? _context.DonHangs.FirstOrDefault(item => item.DhId == DhId.Value && !item.Remove)
                : null;
            var metadata = order == null ? null : OnlineOrderMetadata.TryParse(order.GhiChu);
            var customer = customerId.HasValue
                ? _context.KhachHangs.FirstOrDefault(item => item.KhId == customerId.Value && !item.Remove)
                : null;

            return new OnlineCheckoutForm
            {
                HoTen = metadata?.RecipientName ?? customer?.TenKhachHang,
                SoDienThoai = metadata?.Phone ?? customer?.SoDienThoai,
                TinhThanh = metadata?.City,
                QuanHuyen = metadata?.District,
                PhuongXa = metadata?.Ward,
                DiaChi = metadata?.AddressLine ?? customer?.DiaChi,
                GhiChu = metadata?.Note
            };
        }

        private static DonHang EmptyCartOrder()
        {
            return new DonHang
            {
                TongTien = 0
            };
        }
    }
}
