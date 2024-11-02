using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class Thuong
{
    public int TId { get; set; }

    public int? NvId { get; set; }

    public string? TenThuong { get; set; }

    public decimal? TienThuong { get; set; }

    public virtual NhanVien? Nv { get; set; }
}
