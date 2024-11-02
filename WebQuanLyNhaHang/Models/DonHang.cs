using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class DonHang
{
    public int DhId { get; set; }

    public int? KhId { get; set; }

    public int? BanId { get; set; }

    public int? KmId { get; set; }

    public DateTime? GioVao { get; set; }

    public DateTime? GioRa { get; set; }

    public decimal? TongTien { get; set; }

    public int? NvId { get; set; }

    public virtual Ban? Ban { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public virtual KhachHang? Kh { get; set; }

    public virtual KhuyenMai? Km { get; set; }

    public virtual NhanVien? Nv { get; set; }
}
