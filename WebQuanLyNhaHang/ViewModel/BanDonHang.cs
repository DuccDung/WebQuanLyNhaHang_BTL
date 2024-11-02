namespace WebQuanLyNhaHang.ViewModel
{
    public class BanDonHang
    {
        public int BanId { get; set; }

        public int? SoChoNgoi { get; set; }

        public string? GhiChu { get; set; }

        //=========== Đơn Hàng=======================
        public int DhId { get; set; }

        public int? KhId { get; set; }

        public int? KmId { get; set; }

        public DateTime? GioVao { get; set; }

        public DateTime? GioRa { get; set; }

        public decimal? TongTien { get; set; }

        public int? NvId { get; set; }
    }
}
