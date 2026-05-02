using System;

namespace WebQuanLyNhaHang.Models;

public partial class OnlineOrderInfo
{
    public int OnlineOrderInfoId { get; set; }

    public int DhId { get; set; }

    public int? CuaHangId { get; set; }

    public string TrangThaiGiaoHang { get; set; } = OnlineOrderMetadata.StatusCart;

    public string? NguoiNhan { get; set; }

    public string? SoDienThoai { get; set; }

    public string? TinhThanh { get; set; }

    public string? QuanHuyen { get; set; }

    public string? PhuongXa { get; set; }

    public string? DiaChi { get; set; }

    public string? GhiChuGiaoHang { get; set; }

    public decimal PhiGiaoHang { get; set; }

    public string? PhuongThucThanhToan { get; set; }

    public string? TrangThaiThanhToan { get; set; }

    public DateTime? NgayDat { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public bool Remove { get; set; }

    public virtual CuaHang? CuaHang { get; set; }

    public virtual DonHang Dh { get; set; } = null!;
}
