using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class NhaCungCap
{
    public int NccId { get; set; }

    public string? TenNhaCungCap { get; set; }

    public string? DiaChi { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<HoaDonNhap> HoaDonNhaps { get; set; } = new List<HoaDonNhap>();
}
