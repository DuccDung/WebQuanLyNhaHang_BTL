using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class Ban
{
    public int BanId { get; set; }

    public int? SoChoNgoi { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
}
