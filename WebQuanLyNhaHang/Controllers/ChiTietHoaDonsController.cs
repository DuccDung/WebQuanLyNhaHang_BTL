using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using WebQuanLyNhaHang.Hubs;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.Controllers
{
    public class ChiTietHoaDonsController : Controller
    {
        private readonly QlnhaHangBtlContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChiTietHoaDonsController(QlnhaHangBtlContext context , IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        [HttpPost]
        public JsonResult Decrease(int Soluong, int ProductId, int DhId)
        {
            var CTHD = _context.ChiTietHoaDons.Where(e => e.ProductId == ProductId && e.DhId == DhId).FirstOrDefault();

            if (CTHD == null)
            {
                return Json(new { error = "Record not found" });
            }

            CTHD.SoLuong = Soluong - 1;
            _context.SaveChanges();
            

            return Json(new { newQuantity = CTHD.SoLuong });
        }
        [HttpPost]
        public JsonResult Increase(int Soluong, int ProductId, int DhId)
        {
            var CTHD = _context.ChiTietHoaDons.Where(e => e.ProductId == ProductId && e.DhId == DhId).FirstOrDefault();

            if (CTHD == null)
            {
                return Json(new { error = "Record not found" });
            }

            CTHD.SoLuong = Soluong + 1;
            _context.SaveChanges();

            return Json(new { newQuantity = CTHD.SoLuong });
        }

        [HttpPost]
        public JsonResult DeleteProductDetail(int ProductId, int DhId)
        {
            try
            {
                var CTHD = _context.ChiTietHoaDons
                    .Where(e => e.ProductId == ProductId && e.DhId == DhId)
                    .FirstOrDefault();

                if (CTHD == null)
                {
                    return Json(new { error = "Record not found" });
                }

                _context.ChiTietHoaDons.Remove(CTHD);
                _context.SaveChanges();
                //dùng phương thức của signalR để nhận biết sự thay đổi của database
                 _hubContext.Clients.All.SendAsync("DatabaseUpdated");
                 _hubContext.Clients.All.SendAsync("ProductDeleted", ProductId);
                return Json(new { success = "delete CTHD" });
            }
            catch (Exception ex)
            {
                return Json(new { error = "An error occurred while deleting the record", details = ex.Message });
            }
        }



        // GET: ChiTietHoaDons
        public async Task<IActionResult> Index()
        {
            var qlnhaHangBtlContext = _context.ChiTietHoaDons.Include(c => c.Dh).Include(c => c.Product);
            return View(await qlnhaHangBtlContext.ToListAsync());
        }

        // GET: ChiTietHoaDons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTietHoaDon = await _context.ChiTietHoaDons
                .Include(c => c.Dh)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.CthdId == id);
            if (chiTietHoaDon == null)
            {
                return NotFound();
            }

            return View(chiTietHoaDon);
        }

        // GET: ChiTietHoaDons/Create
        public IActionResult Create()
        {
            ViewData["DhId"] = new SelectList(_context.DonHangs, "DhId", "DhId");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId");
            return View();
        }

        // POST: ChiTietHoaDons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CthdId,SoLuong,ThanhTien,DhId,ProductId,Ghichu")] ChiTietHoaDon chiTietHoaDon)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chiTietHoaDon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DhId"] = new SelectList(_context.DonHangs, "DhId", "DhId", chiTietHoaDon.DhId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", chiTietHoaDon.ProductId);
            return View(chiTietHoaDon);
        }



        // GET: ChiTietHoaDons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTietHoaDon = await _context.ChiTietHoaDons.FindAsync(id);
            if (chiTietHoaDon == null)
            {
                return NotFound();
            }
            ViewData["DhId"] = new SelectList(_context.DonHangs, "DhId", "DhId", chiTietHoaDon.DhId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", chiTietHoaDon.ProductId);
            return View(chiTietHoaDon);
        }

        // POST: ChiTietHoaDons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CthdId,SoLuong,ThanhTien,DhId,ProductId,Ghichu")] ChiTietHoaDon chiTietHoaDon)
        {
            if (id != chiTietHoaDon.CthdId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chiTietHoaDon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChiTietHoaDonExists(chiTietHoaDon.CthdId))
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
            ViewData["DhId"] = new SelectList(_context.DonHangs, "DhId", "DhId", chiTietHoaDon.DhId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", chiTietHoaDon.ProductId);
            return View(chiTietHoaDon);
        }

        // GET: ChiTietHoaDons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTietHoaDon = await _context.ChiTietHoaDons
                .Include(c => c.Dh)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.CthdId == id);
            if (chiTietHoaDon == null)
            {
                return NotFound();
            }

            return View(chiTietHoaDon);
        }

        // POST: ChiTietHoaDons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chiTietHoaDon = await _context.ChiTietHoaDons.FindAsync(id);
            if (chiTietHoaDon != null)
            {
                _context.ChiTietHoaDons.Remove(chiTietHoaDon);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChiTietHoaDonExists(int id)
        {
            return _context.ChiTietHoaDons.Any(e => e.CthdId == id);
        }
    }
}
