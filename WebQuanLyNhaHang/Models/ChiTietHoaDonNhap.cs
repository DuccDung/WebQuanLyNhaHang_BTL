using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class ChiTietHoaDonNhap
{
    public int CthdnId { get; set; }

    public int HdnId { get; set; }

    public int NlId { get; set; }

    public int? SoLuong { get; set; }

    public decimal? ThanhTien { get; set; }

    public virtual HoaDonNhap Hdn { get; set; } = null!;

    public virtual NguyenLieu Nl { get; set; } = null!;
}
