using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.Controllers
{
    public class ProductsController : Controller
    {
        private readonly QlnhaHangBtlContext _context;
        public readonly IWebHostEnvironment _webHostEnvironment;
        public ProductsController(QlnhaHangBtlContext context , IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            ViewData["CateId"] = new SelectList(_context.Categories, "CateId", "CateId");
            var qlnhaHangBtlContext = _context.Products.Include(p => p.Cate);
            return View(await qlnhaHangBtlContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Cate)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CateId"] = new SelectList(_context.Categories, "CateId", "TenLoaiSanPham");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,CateId,TenSanPham,MoTa,GiaTien,SoLuong")] Product product, IFormFile FileInterface)
        {
            if (ModelState.IsValid)
            {
                // Thêm sản phẩm vào context
                _context.Add(product);
                await _context.SaveChangesAsync();

                // Kiểm tra nếu file được chọn và không null
                if (FileInterface != null && FileInterface.Length > 0)
                {
                    var fileExtension = Path.GetExtension(FileInterface.FileName).ToLowerInvariant();

                    // Kiểm tra đuôi file
                    if (!string.IsNullOrEmpty(fileExtension))
                    {
                        // Thiết lập đường dẫn lưu trữ file
                        var uploadFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "asset/img/imgWeb");
                        Directory.CreateDirectory(uploadFolderPath);

                        // Tạo tên file mới để tránh trùng lặp
                        var fileName = Path.GetRandomFileName() + fileExtension;
                        var filePath = Path.Combine(uploadFolderPath, fileName);

                        // Lưu file lên server
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await FileInterface.CopyToAsync(stream);
                        }

                        // Cập nhật đường dẫn ảnh trong cơ sở dữ liệu nếu cần
                        product.PathPhoto = Path.Combine("asset/img/imgWeb", fileName);
                        _context.Update(product);
                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            // Nếu model state không hợp lệ, hiển thị lại form cùng danh sách danh mục
            ViewData["CateId"] = new SelectList(_context.Categories, "CateId", "CateId", product.CateId);
            return View(product);
        }


        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CateId"] = new SelectList(_context.Categories, "CateId", "CateId", product.CateId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,CateId,TenSanPham,MoTa,GiaTien,SoLuong")] Product product, IFormFile FileInterface)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra nếu file được chọn và không null
                    if (FileInterface != null && FileInterface.Length > 0)
                    {
                        var fileExtension = Path.GetExtension(FileInterface.FileName).ToLowerInvariant();

                        // Kiểm tra đuôi file
                        if (!string.IsNullOrEmpty(fileExtension))
                        {
                            // Thiết lập đường dẫn lưu trữ file
                            var uploadFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "asset/img/imgWeb");
                            Directory.CreateDirectory(uploadFolderPath);

                            // Tạo tên file mới để tránh trùng lặp
                            var fileName = Path.GetRandomFileName() + fileExtension;
                            var filePath = Path.Combine(uploadFolderPath, fileName);

                            // Lưu file lên server
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await FileInterface.CopyToAsync(stream);
                            }

                            // Cập nhật đường dẫn ảnh trong cơ sở dữ liệu
                            product.PathPhoto = Path.Combine("asset/img/imgWeb", fileName);
                        }
                    }

                    // Cập nhật thông tin sản phẩm vào context và lưu thay đổi
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index" , "Products");
            }

            // Hiển thị lại form nếu model state không hợp lệ
            ViewData["CateId"] = new SelectList(_context.Categories, "CateId", "TenLoaiSanPham", product.CateId);
            return View(product);
        }


        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Cate)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
