using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQuanLyNhaHang.Authorization;
using WebQuanLyNhaHang.Filters;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers;

[AdminSessionAuthorize]
[RoleAuthorize(PermissionModules.Stories, PermissionActions.View)]
public class AdminBaiVietChuyenNhasController : Controller
{
    private readonly QlnhaHangBtlContext _context;

    public AdminBaiVietChuyenNhasController(QlnhaHangBtlContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await BuildPageAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RoleAuthorize(PermissionModules.Stories, PermissionActions.Create)]
    public async Task<IActionResult> Create(BaiVietChuyenNha form)
    {
        Normalize(form);
        if (string.IsNullOrWhiteSpace(form.TieuDe))
        {
            ModelState.AddModelError(string.Empty, "Vui lòng nhập tiêu đề bài viết.");
            return View(nameof(Index), await BuildPageAsync(form));
        }

        form.CreatedAt = DateTime.Now;
        form.NgayDang = form.NgayDang == default ? DateTime.Now : form.NgayDang;
        form.Remove = false;
        _context.BaiVietChuyenNhas.Add(form);
        await _context.SaveChangesAsync();
        TempData["success"] = "Đã thêm bài viết.";
        return RedirectToAction(nameof(Index));
    }

    [RoleAuthorize(PermissionModules.Stories, PermissionActions.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _context.BaiVietChuyenNhas.FirstOrDefaultAsync(item => item.BaiVietId == id && !item.Remove);
        if (post == null)
        {
            return NotFound();
        }

        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RoleAuthorize(PermissionModules.Stories, PermissionActions.Edit)]
    public async Task<IActionResult> Edit(int id, BaiVietChuyenNha form)
    {
        var post = await _context.BaiVietChuyenNhas.FirstOrDefaultAsync(item => item.BaiVietId == id && !item.Remove);
        if (post == null)
        {
            return NotFound();
        }

        Normalize(form);
        if (string.IsNullOrWhiteSpace(form.TieuDe))
        {
            ModelState.AddModelError(string.Empty, "Vui lòng nhập tiêu đề bài viết.");
            form.BaiVietId = id;
            return View(form);
        }

        post.TieuDe = form.TieuDe;
        post.Slug = form.Slug;
        post.TomTat = form.TomTat;
        post.NoiDung = form.NoiDung;
        post.PathPhoto = form.PathPhoto;
        post.AltText = form.AltText;
        post.TacGia = form.TacGia;
        post.NgayDang = form.NgayDang == default ? post.NgayDang : form.NgayDang;
        post.NoiBat = form.NoiBat;
        post.SapXep = form.SapXep;
        post.HienThi = form.HienThi;
        post.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        TempData["success"] = "Đã cập nhật bài viết.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RoleAuthorize(PermissionModules.Stories, PermissionActions.HideShow)]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _context.BaiVietChuyenNhas.FirstOrDefaultAsync(item => item.BaiVietId == id && !item.Remove);
        if (post != null)
        {
            post.Remove = true;
            post.HienThi = false;
            post.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["success"] = "Đã ẩn bài viết.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminBaiVietChuyenNhaPageViewModel> BuildPageAsync(BaiVietChuyenNha? form = null)
    {
        return new AdminBaiVietChuyenNhaPageViewModel
        {
            Form = form ?? new BaiVietChuyenNha { HienThi = true, NgayDang = DateTime.Now, TacGia = "Cloudy Café" },
            Posts = await _context.BaiVietChuyenNhas
                .AsNoTracking()
                .Where(item => !item.Remove)
                .OrderBy(item => item.SapXep)
                .ThenByDescending(item => item.NgayDang)
                .ToListAsync()
        };
    }

    private static void Normalize(BaiVietChuyenNha post)
    {
        post.TieuDe = post.TieuDe?.Trim() ?? string.Empty;
        post.Slug = Clean(post.Slug);
        post.TomTat = Clean(post.TomTat);
        post.NoiDung = Clean(post.NoiDung);
        post.PathPhoto = Clean(post.PathPhoto);
        post.AltText = Clean(post.AltText);
        post.TacGia = Clean(post.TacGia);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
