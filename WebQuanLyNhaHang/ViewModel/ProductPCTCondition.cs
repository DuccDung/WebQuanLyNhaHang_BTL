namespace WebQuanLyNhaHang.ViewModel
{
    public class ProductPCTCondition
    {
        public int ProductId { get; set; }

        public int? CateId { get; set; }

        public string? TenSanPham { get; set; }

        public string? MoTa { get; set; }

        public decimal? GiaTien { get; set; }

        public string? PathPhoto { get; set; }
        
        // product condition
        public int ProductConditionId { get; set; }
        public string? Condition { get; set; }
    }
}
