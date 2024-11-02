using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public class DoanhThu_LoiNhan
    {
        private readonly QlnhaHangBtlContext _qlnhaHangBtlContext;
        public DoanhThu_LoiNhan(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }

        public Double DoanhThu()
        {
            return 1;
        }
    }
}
