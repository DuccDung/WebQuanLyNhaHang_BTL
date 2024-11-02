using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class CongThuc
{
    public int CtId { get; set; }

    public int? ProductId { get; set; }

    public int? NlId { get; set; }

    public decimal SoLuong { get; set; }

    public string? GhiChu { get; set; }

    public virtual NguyenLieu? Nl { get; set; }

    public virtual Product? Product { get; set; }
}
