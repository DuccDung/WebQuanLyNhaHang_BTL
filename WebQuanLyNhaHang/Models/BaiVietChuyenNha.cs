using System;

namespace WebQuanLyNhaHang.Models;

public partial class BaiVietChuyenNha
{
    public int BaiVietId { get; set; }

    public string TieuDe { get; set; } = null!;

    public string? Slug { get; set; }

    public string? TomTat { get; set; }

    public string? NoiDung { get; set; }

    public string? PathPhoto { get; set; }

    public string? AltText { get; set; }

    public string? TacGia { get; set; }

    public DateTime NgayDang { get; set; }

    public bool NoiBat { get; set; }

    public int SapXep { get; set; }

    public bool HienThi { get; set; }

    public bool Remove { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
