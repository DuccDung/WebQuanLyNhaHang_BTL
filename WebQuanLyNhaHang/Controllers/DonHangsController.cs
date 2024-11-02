using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using WebQuanLyNhaHang.Hubs;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    public class DonHangsController : Controller
    {
        private readonly QlnhaHangBtlContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        public DonHangsController(QlnhaHangBtlContext context , IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: DonHangs
        public async Task<IActionResult> Index()
        {
            var qlnhaHangBtlContext = _context.DonHangs.Include(d => d.Ban).Include(d => d.Kh).Include(d => d.Km).Include(d => d.Nv);
            return View(await qlnhaHangBtlContext.ToListAsync());
        }

        // GET: DonHangs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.Ban)
                .Include(d => d.Kh)
                .Include(d => d.Km)
                .Include(d => d.Nv)
                .FirstOrDefaultAsync(m => m.DhId == id);
            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        // GET: DonHangs/Create
        public IActionResult Create()
        {
            ViewData["BanId"] = new SelectList(_context.Bans, "BanId", "BanId");
            ViewData["KhId"] = new SelectList(_context.KhachHangs, "KhId", "KhId");
            ViewData["KmId"] = new SelectList(_context.KhuyenMais, "KmId", "KmId");
            ViewData["NvId"] = new SelectList(_context.NhanViens, "NvId", "NvId");
            return View();
        }

        // POST: DonHangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DhId,KhId,BanId,KmId,GioVao,GioRa,TongTien,NvId")] DonHang donHang)
        {
            if (ModelState.IsValid)
            {
                _context.Add(donHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BanId"] = new SelectList(_context.Bans, "BanId", "BanId", donHang.BanId);
            ViewData["KhId"] = new SelectList(_context.KhachHangs, "KhId", "KhId", donHang.KhId);
            ViewData["KmId"] = new SelectList(_context.KhuyenMais, "KmId", "KmId", donHang.KmId);
            ViewData["NvId"] = new SelectList(_context.NhanViens, "NvId", "NvId", donHang.NvId);
            return View(donHang);
        }

        // GET: DonHangs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null)
            {
                return NotFound();
            }
            ViewData["BanId"] = new SelectList(_context.Bans, "BanId", "BanId", donHang.BanId);
            ViewData["KhId"] = new SelectList(_context.KhachHangs, "KhId", "KhId", donHang.KhId);
            ViewData["KmId"] = new SelectList(_context.KhuyenMais, "KmId", "KmId", donHang.KmId);
            ViewData["NvId"] = new SelectList(_context.NhanViens, "NvId", "NvId", donHang.NvId);
            return View(donHang);
        }

        // POST: DonHangs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DhId,KhId,BanId,KmId,GioVao,GioRa,TongTien,NvId")] DonHang donHang)
        {
            if (id != donHang.DhId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(donHang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonHangExists(donHang.DhId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BanId"] = new SelectList(_context.Bans, "BanId", "BanId", donHang.BanId);
            ViewData["KhId"] = new SelectList(_context.KhachHangs, "KhId", "KhId", donHang.KhId);
            ViewData["KmId"] = new SelectList(_context.KhuyenMais, "KmId", "KmId", donHang.KmId);
            ViewData["NvId"] = new SelectList(_context.NhanViens, "NvId", "NvId", donHang.NvId);
            return View(donHang);
        }

        // GET: DonHangs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.Ban)
                .Include(d => d.Kh)
                .Include(d => d.Km)
                .Include(d => d.Nv)
                .FirstOrDefaultAsync(m => m.DhId == id);
            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        // POST: DonHangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang != null)
            {
                _context.DonHangs.Remove(donHang);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DonHangExists(int id)
        {
            return _context.DonHangs.Any(e => e.DhId == id);
        }
        //[HttpPost]
        //public IActionResult RemoveItem(int id) // id của cthd 
        //{
        //    var CTHD = _context.ChiTietHoaDons.Find(id);  // tìm chi tiết hóa đơn
        //    if (CTHD != null)
        //    {
        //        _context.ChiTietHoaDons.Remove(CTHD); // xóa chi tiết hóa đơn
        //        _context.SaveChanges();

        //        // Tạo ViewModel chứa dữ liệu cần thiết để render lại partial view
        //        //ViewModelCart viewModelCart = new ViewModelCart(_context);

        //        // Trả về partial view với viewModel
        //        return PartialView("CTHDTable");
        //    }
        //    else
        //    {
        //        throw new Exception("Lỗi xóa CTDH trong Cart");
        //    }
        //}

    }
}
