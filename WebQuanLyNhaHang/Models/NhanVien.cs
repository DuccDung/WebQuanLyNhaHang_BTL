using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class NhanVien
{
    public int NvId { get; set; }

    public string? TenNhanVien { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? DiaChi { get; set; }

    public decimal? HeSoLuong { get; set; }

    public string? PathPhoto { get; set; }

    public string TaiKhoan { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<HoaDonNhap> HoaDonNhaps { get; set; } = new List<HoaDonNhap>();

    public virtual ICollection<NvNc> NvNcs { get; set; } = new List<NvNc>();

    public virtual ICollection<NvPq> NvPqs { get; set; } = new List<NvPq>();

    public virtual ICollection<Thuong> Thuongs { get; set; } = new List<Thuong>();
}
