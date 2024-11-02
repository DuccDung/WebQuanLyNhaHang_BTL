using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.Controllers
{
    public class NhanViensController : Controller
    {
        private readonly QlnhaHangBtlContext _context;

        public NhanViensController(QlnhaHangBtlContext context)
        {
            _context = context;
        }

        // GET: NhanViens
        public async Task<IActionResult> Index()
        {
            return View(await _context.NhanViens.ToListAsync());
        }

        // GET: NhanViens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(m => m.NvId == id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // GET: NhanViens/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NhanViens/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NvId,TenNhanVien,NgaySinh,DiaChi,HeSoLuong,PathPhoto,TaiKhoan,MatKhau")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nhanVien);
        }

        //// GET: NhanViens/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var nhanVien = await _context.NhanViens.FindAsync(id);
        //    if (nhanVien == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(nhanVien);
        //}

        // POST: NhanViens/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit([Bind("NvId,TenNhanVien,NgaySinh,DiaChi,HeSoLuong,TaiKhoan,MatKhau")] NhanVien nhanVien, IFormFile fileImg)
            {
                var _NhanVien = await _context.NhanViens.FindAsync(nhanVien.NvId);
                if (_NhanVien == null)
                {
                    return NotFound();
                }
                else
                {
                    try
                    {
                        // Cập nhật các thuộc tính
                        _NhanVien.TenNhanVien = nhanVien.TenNhanVien;
                        _NhanVien.NgaySinh = nhanVien.NgaySinh;
                        _NhanVien.DiaChi = nhanVien.DiaChi;
                        _NhanVien.HeSoLuong = nhanVien.HeSoLuong;
                        _NhanVien.TaiKhoan = nhanVien.TaiKhoan;
                        _NhanVien.MatKhau = nhanVien.MatKhau;

                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!NhanVienExists(nhanVien.NvId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction("Index", "NhanViens");
                }
            }



        public async Task<IActionResult> Delete(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien != null)
            {
                _context.NhanViens.Remove(nhanVien);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index" , "NhanViens");
        }

        private bool NhanVienExists(int id)
        {
            return _context.NhanViens.Any(e => e.NvId == id);
        }
    }
}
