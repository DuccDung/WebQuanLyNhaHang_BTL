using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel;

public sealed class AdminCuaHangPageViewModel
{
    public List<CuaHang> Stores { get; set; } = new();

    public CuaHang Form { get; set; } = new()
    {
        HienThi = true,
        GioMoCua = new TimeOnly(7, 0),
        GioDongCua = new TimeOnly(22, 0)
    };
}

public sealed class AdminBaiVietChuyenNhaPageViewModel
{
    public List<BaiVietChuyenNha> Posts { get; set; } = new();

    public BaiVietChuyenNha Form { get; set; } = new()
    {
        HienThi = true,
        NgayDang = DateTime.Now,
        TacGia = "Cloudy Café"
    };
}
