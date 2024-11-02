using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class NguyenLieu
{
    public int NlId { get; set; }

    public string? TenNguyenLieu { get; set; }

    public decimal? GiaTien { get; set; }

    public virtual ICollection<ChiTietHoaDonNhap> ChiTietHoaDonNhaps { get; set; } = new List<ChiTietHoaDonNhap>();

    public virtual ICollection<CongThuc> CongThucs { get; set; } = new List<CongThuc>();
}
