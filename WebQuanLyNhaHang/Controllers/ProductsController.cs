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
                .FirstOrDefaultAsync(m => m.ProductId == id && !m.Remove);
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

            var product = await _context.Products.FirstOrDefaultAsync(item => item.ProductId == id && !item.Remove);
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
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,CateId,TenSanPham,MoTa,GiaTien,SoLuong")] Product product, IFormFile? FileInterface)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            var existingProduct = await _context.Products.FirstOrDefaultAsync(item => item.ProductId == id && !item.Remove);
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
                await UpdateExistingProductAsync(existingProduct, product, FileInterface);
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

        // POST: Products/EditFromModal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFromModal(int id, [Bind("ProductId,CateId,TenSanPham,MoTa,GiaTien,SoLuong")] Product product, IFormFile? FileInterface)
        {
            var isAjaxRequest = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (id != product.ProductId)
            {
                return NotFound();
            }

            var existingProduct = await _context.Products.FirstOrDefaultAsync(item => item.ProductId == id && !item.Remove);
            if (existingProduct == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                if (isAjaxRequest)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng kiểm tra lại thông tin sản phẩm."
                    });
                }

                product.PathPhoto = existingProduct.PathPhoto;
                var indexViewModel = await BuildIndexViewModelAsync(editProduct: product, openEditProductId: id);
                return View(nameof(Index), indexViewModel);
            }

            try
            {
                await UpdateExistingProductAsync(existingProduct, product, FileInterface);
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

            if (isAjaxRequest)
            {
                var categoryName = await GetCategoryNameAsync(existingProduct.CateId);

                return Json(new
                {
                    success = true,
                    product = new
                    {
                        productId = existingProduct.ProductId,
                        name = existingProduct.TenSanPham,
                        description = existingProduct.MoTa,
                        price = existingProduct.GiaTien,
                        imagePath = existingProduct.PathPhoto,
                        categoryName
                    }
                });
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
                .FirstOrDefaultAsync(m => m.ProductId == id && !m.Remove);
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
            var isAjaxRequest = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            var product = await _context.Products.FirstOrDefaultAsync(item => item.ProductId == id && !item.Remove);
            if (product == null)
            {
                if (isAjaxRequest)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy sản phẩm cần xóa."
                    });
                }

                return RedirectToAction(nameof(Index));
            }

            if (product != null)
            {
                product.Remove = true;
            }

            await _context.SaveChangesAsync();

            if (isAjaxRequest)
            {
                return Json(new
                {
                    success = true,
                    productId = id
                });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        private async Task UpdateExistingProductAsync(Product existingProduct, Product product, IFormFile? fileInterface)
        {
            existingProduct.CateId = product.CateId;
            existingProduct.TenSanPham = product.TenSanPham;
            existingProduct.MoTa = product.MoTa;
            existingProduct.GiaTien = product.GiaTien;

            if (fileInterface != null && fileInterface.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileInterface.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fileInterface.CopyToAsync(fileStream);
                }

                existingProduct.PathPhoto = "/assets/images/" + uniqueFileName;
            }
        }

        private async Task<string> GetCategoryNameAsync(int? categoryId)
        {
            if (!categoryId.HasValue)
            {
                return "Chưa phân loại";
            }

            return await _context.Categories
                .Where(category => category.CateId == categoryId.Value && !category.Remove)
                .Select(category => category.TenLoaiSanPham)
                .FirstOrDefaultAsync() ?? "Chưa phân loại";
        }

        private async Task<ProductsIndexViewModel> BuildIndexViewModelAsync(Product? createProduct = null, bool openCreateModal = false, Product? editProduct = null, int? openEditProductId = null)
        {
            var products = await _context.Products
                .Include(product => product.Cate)
                .Where(product => !product.Remove)
                .ToListAsync();

            var categories = await _context.Categories
                .Where(category => !category.Remove)
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
                EditProduct = editProduct,
                CategoryOptions = categories,
                OpenCreateModal = openCreateModal,
                OpenEditProductId = openEditProductId
            };
        }
    }
}
