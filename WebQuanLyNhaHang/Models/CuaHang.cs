using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class CuaHang
{
    public int CuaHangId { get; set; }

    public string TenCuaHang { get; set; } = null!;

    public string DiaChi { get; set; } = null!;

    public string? TinhThanh { get; set; }

    public string? QuanHuyen { get; set; }

    public string? PhuongXa { get; set; }

    public string? SoDienThoai { get; set; }

    public TimeOnly? GioMoCua { get; set; }

    public TimeOnly? GioDongCua { get; set; }

    public string? PathPhoto { get; set; }

    public string? GoogleMapUrl { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int SapXep { get; set; }

    public bool HienThi { get; set; }

    public bool Remove { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<OnlineOrderInfo> OnlineOrderInfos { get; set; } = new List<OnlineOrderInfo>();
}
