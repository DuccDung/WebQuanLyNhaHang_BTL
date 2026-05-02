using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQuanLyNhaHang.Filters;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers;

[AdminSessionAuthorize]
public class AdminCuaHangsController : Controller
{
    private readonly QlnhaHangBtlContext _context;

    public AdminCuaHangsController(QlnhaHangBtlContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await BuildPageAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CuaHang form)
    {
        Normalize(form);
        if (string.IsNullOrWhiteSpace(form.TenCuaHang) || string.IsNullOrWhiteSpace(form.DiaChi))
        {
            ModelState.AddModelError(string.Empty, "Vui lòng nhập tên cửa hàng và địa chỉ.");
            return View(nameof(Index), await BuildPageAsync(form));
        }

        form.CreatedAt = DateTime.Now;
        form.Remove = false;
        _context.CuaHangs.Add(form);
        await _context.SaveChangesAsync();
        TempData["success"] = "Đã thêm cửa hàng.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var store = await _context.CuaHangs.FirstOrDefaultAsync(item => item.CuaHangId == id && !item.Remove);
        if (store == null)
        {
            return NotFound();
        }

        return View(store);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CuaHang form)
    {
        var store = await _context.CuaHangs.FirstOrDefaultAsync(item => item.CuaHangId == id && !item.Remove);
        if (store == null)
        {
            return NotFound();
        }

        Normalize(form);
        if (string.IsNullOrWhiteSpace(form.TenCuaHang) || string.IsNullOrWhiteSpace(form.DiaChi))
        {
            ModelState.AddModelError(string.Empty, "Vui lòng nhập tên cửa hàng và địa chỉ.");
            form.CuaHangId = id;
            return View(form);
        }

        store.TenCuaHang = form.TenCuaHang;
        store.DiaChi = form.DiaChi;
        store.TinhThanh = form.TinhThanh;
        store.QuanHuyen = form.QuanHuyen;
        store.PhuongXa = form.PhuongXa;
        store.SoDienThoai = form.SoDienThoai;
        store.GioMoCua = form.GioMoCua;
        store.GioDongCua = form.GioDongCua;
        store.PathPhoto = form.PathPhoto;
        store.GoogleMapUrl = form.GoogleMapUrl;
        store.Latitude = form.Latitude;
        store.Longitude = form.Longitude;
        store.SapXep = form.SapXep;
        store.HienThi = form.HienThi;
        store.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        TempData["success"] = "Đã cập nhật cửa hàng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var store = await _context.CuaHangs.FirstOrDefaultAsync(item => item.CuaHangId == id && !item.Remove);
        if (store != null)
        {
            store.Remove = true;
            store.HienThi = false;
            store.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["success"] = "Đã ẩn cửa hàng.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminCuaHangPageViewModel> BuildPageAsync(CuaHang? form = null)
    {
        return new AdminCuaHangPageViewModel
        {
            Form = form ?? new CuaHang { HienThi = true, GioMoCua = new TimeOnly(7, 0), GioDongCua = new TimeOnly(22, 0) },
            Stores = await _context.CuaHangs
                .AsNoTracking()
                .Where(item => !item.Remove)
                .OrderBy(item => item.SapXep)
                .ThenBy(item => item.CuaHangId)
                .ToListAsync()
        };
    }

    private static void Normalize(CuaHang store)
    {
        store.TenCuaHang = store.TenCuaHang?.Trim() ?? string.Empty;
        store.DiaChi = store.DiaChi?.Trim() ?? string.Empty;
        store.TinhThanh = Clean(store.TinhThanh);
        store.QuanHuyen = Clean(store.QuanHuyen);
        store.PhuongXa = Clean(store.PhuongXa);
        store.SoDienThoai = Clean(store.SoDienThoai);
        store.PathPhoto = Clean(store.PathPhoto);
        store.GoogleMapUrl = Clean(store.GoogleMapUrl);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
