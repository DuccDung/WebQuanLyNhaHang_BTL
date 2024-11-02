namespace WebQuanLyNhaHang.ViewModel
{
    public class CategoryProduct
    {
        // thuộc tính của product
        public int ProductId { get; set; }
        public string? TenSanPham { get; set; }

        public decimal? GiaTien { get; set; }

        public string? PathPhoto { get; set; }

        // thuộc tính của category
        public int CateId { get; set; }

        public string? TenLoaiSanPham { get; set; }

        public string? MoTa { get; set; }
    }
}
