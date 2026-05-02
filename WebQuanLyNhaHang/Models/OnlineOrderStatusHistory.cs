using System;

namespace WebQuanLyNhaHang.Models;

public partial class OnlineOrderStatusHistory
{
    public int HistoryId { get; set; }

    public int DhId { get; set; }

    public string? TrangThaiCu { get; set; }

    public string TrangThaiMoi { get; set; } = null!;

    public int? NvId { get; set; }

    public string? GhiChu { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual DonHang Dh { get; set; } = null!;

    public virtual NhanVien? Nv { get; set; }
}
