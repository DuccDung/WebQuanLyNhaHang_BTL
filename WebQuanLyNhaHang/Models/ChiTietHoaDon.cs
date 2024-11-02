using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class ChiTietHoaDon
{
    public int CthdId { get; set; }

    public int? SoLuong { get; set; }

    public decimal? ThanhTien { get; set; }

    public int DhId { get; set; }

    public int? ProductId { get; set; }
    public string? Ghichu { get; set; }

    public virtual DonHang Dh { get; set; } = null!;

    public virtual Product? Product { get; set; }
}
