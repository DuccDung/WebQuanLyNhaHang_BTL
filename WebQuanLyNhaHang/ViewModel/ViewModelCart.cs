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

        private static DonHang EmptyCartOrder()
        {
            return new DonHang
            {
                TongTien = 0
            };
        }
    }
}
