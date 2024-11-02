using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class HoaDonNhap
{
    public int HdnId { get; set; }

    public int NccId { get; set; }

    public int NvId { get; set; }

    public DateTime? NgayLapHoaDon { get; set; }

    public DateTime? NgayNhanHang { get; set; }

    public decimal? TongSoTien { get; set; }

    public virtual ICollection<ChiTietHoaDonNhap> ChiTietHoaDonNhaps { get; set; } = new List<ChiTietHoaDonNhap>();

    public virtual NhaCungCap Ncc { get; set; } = null!;

    public virtual NhanVien Nv { get; set; } = null!;
}
