using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int? CateId { get; set; }

    public string? TenSanPham { get; set; }

    public string? MoTa { get; set; }

    public decimal? GiaTien { get; set; }

    public int? SoLuong {  get; set; }
    public string? PathPhoto { get; set; }

    public virtual Category? Cate { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public virtual ICollection<CongThuc> CongThucs { get; set; } = new List<CongThuc>();
    public virtual ICollection<ProductConditions> ProductConditions { get; set; } = new List<ProductConditions>();
}
