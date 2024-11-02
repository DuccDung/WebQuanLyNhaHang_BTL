namespace WebQuanLyNhaHang.ViewModel
{
    public class CTDH_Product
    {
        // chi tiết hóa đơn
        public int? CthdId { get; set; }
        public int? ProductId { get; set; }
        public int? SoLuong { get; set; }
        public decimal? ThanhTien { get; set; }
        public decimal? DonGia { get; set; }
        public int? DhId { get; set; }

        // product
        public string? TenSanPham { get; set; }
        public string? PathPhoto { get; set; }

        // product condition

        public int ProductConditionId { get; set; }
        public string? Condition { get; set; }

        // DonHang
        public int BanId { get; set; }
        public decimal? TongTien { get; set; }
    }
}
