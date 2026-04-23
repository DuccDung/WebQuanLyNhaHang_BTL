using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQuanLyNhaHang.Filters;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    [AdminSessionAuthorize]
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
            return View(await BuildIndexViewModelAsync());
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,CateId,TenSanPham,MoTa,GiaTien,SoLuong")] Product product, IFormFile FileInterface, bool returnToIndexModal = false)
        {
            if (ModelState.IsValid)
            {
                if (FileInterface != null && FileInterface.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(FileInterface.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await FileInterface.CopyToAsync(fileStream);
                    }

                    product.PathPhoto = "/assets/images/" + uniqueFileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            if (returnToIndexModal)
            {
                var indexViewModel = await BuildIndexViewModelAsync(product, openCreateModal: true);
                return View(nameof(Index), indexViewModel);
            }

            ViewData["CateId"] = new SelectList(_context.Categories, "CateId", "TenLoaiSanPham", product.CateId);
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

            ViewData["CateId"] = new SelectList(_context.Categories, "CateId", "TenLoaiSanPham", product.CateId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,CateId,TenSanPham,MoTa,GiaTien,SoLuong")] Product product, IFormFile FileInterface)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewData["CateId"] = new SelectList(_context.Categories, "CateId", "TenLoaiSanPham", product.CateId);
                return View(product);
            }

            try
            {
                existingProduct.CateId = product.CateId;
                existingProduct.TenSanPham = product.TenSanPham;
                existingProduct.MoTa = product.MoTa;
                existingProduct.GiaTien = product.GiaTien;

                if (FileInterface != null && FileInterface.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(FileInterface.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await FileInterface.CopyToAsync(fileStream);
                    }

                    existingProduct.PathPhoto = "/assets/images/" + uniqueFileName;
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.ProductId))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
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

        private async Task<ProductsIndexViewModel> BuildIndexViewModelAsync(Product? createProduct = null, bool openCreateModal = false)
        {
            var products = await _context.Products
                .Include(product => product.Cate)
                .ToListAsync();

            var categories = await _context.Categories
                .OrderBy(category => category.TenLoaiSanPham)
                .Select(category => new SelectListItem
                {
                    Value = category.CateId.ToString(),
                    Text = category.TenLoaiSanPham ?? $"Danh muc {category.CateId}",
                    Selected = createProduct != null && category.CateId == createProduct.CateId
                })
                .ToListAsync();

            return new ProductsIndexViewModel
            {
                Products = products,
                CreateProduct = createProduct ?? new Product(),
                CategoryOptions = categories,
                OpenCreateModal = openCreateModal
            };
        }
    }
}
