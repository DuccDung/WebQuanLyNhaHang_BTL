using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class KhuyenMai
{
    public int KmId { get; set; }

    public string? TenKhuyenMai { get; set; }

    public decimal? GiamGia { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
}
