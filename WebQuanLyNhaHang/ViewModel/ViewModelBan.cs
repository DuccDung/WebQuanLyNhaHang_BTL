using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public class ViewModelBan
    {
        private readonly QlnhaHangBtlContext _context;
        public ViewModelBan(QlnhaHangBtlContext context) {
           _context = context;
        }

        public List<Ban> RenBan()
        {
            return _context.Bans.ToList();
        }
        public int DonHangByBan(int id) // từ id Bàn Tìm ra số lượng đơn hàng
        {
           int? sl =  _context.DonHangs.Count(e => e.BanId == id);
            return sl.HasValue ? sl.Value : 0;
        }
    }
}
