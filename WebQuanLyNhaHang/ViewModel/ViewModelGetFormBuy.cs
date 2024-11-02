using Microsoft.AspNetCore.Mvc;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public class ViewModelGetFormBuy 
    {
        private readonly QlnhaHangBtlContext _qlnhaHangBtlContext;
      public ViewModelGetFormBuy(QlnhaHangBtlContext qlnhaHangBtlContext) {
        _qlnhaHangBtlContext = qlnhaHangBtlContext;
      }
        public List<CTDH_Product> CTDH_Product(int BanID)
        {
            var result = from ban in _qlnhaHangBtlContext.Bans
                         join dh in _qlnhaHangBtlContext.DonHangs
                         on ban.BanId equals dh.BanId
                         join cthd in _qlnhaHangBtlContext.ChiTietHoaDons
                         on dh.DhId equals cthd.DhId
                         join product in _qlnhaHangBtlContext.Products
                         on cthd.ProductId equals product.ProductId
                         where ban.BanId == BanID
                         select new CTDH_Product
                         {
                           DhId = dh.DhId,
                           PathPhoto = product.PathPhoto,
                           SoLuong = cthd.SoLuong,
                           TenSanPham = product.TenSanPham,
                           ThanhTien = cthd.ThanhTien,
                           DonGia = product.GiaTien,
                           TongTien = dh.TongTien,
                           BanId = BanID,
                         };

            return result.ToList();
        }

    }
}
