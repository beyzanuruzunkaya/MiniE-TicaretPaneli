using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniE_TicaretPaneli.Data;
using MiniE_TicaretPaneli.Models;
using MiniE_TicaretPaneli.Models.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace MiniE_TicaretPaneli.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Products()
        {
            System.Diagnostics.Debug.WriteLine("[LOG] Products action çalıştı!");
            ViewData["Title"] = "Ürün Yönetimi";
            var products = await _context.Products
                                       .Include(p => p.MainCategory)
                                       .Include(p => p.SubCategory)
                                       .OrderByDescending(p => p.IsActive) // Önce aktif ürünler, sonra pasif ürünler
                                       .ThenBy(p => p.Name)
                                       .ToListAsync();

            // Debug: ImageUrl değerlerini kontrol et
            foreach (var product in products)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Product: {product.Name}, ImageUrl: {product.ImageUrl ?? "NULL"}");
            }

            var mainCategories = _context.Categories
        .Where(c => c.ParentCategoryId == null)
        .ToList();

            ViewBag.MainCategories = mainCategories;

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int? id)
        {
            if (id == null) { return NotFound(); }
            var product = await _context.Products.FindAsync(id);
            if (product == null) { return NotFound(); }
            ViewData["Title"] = "Ürün Düzenle";
            var viewModel = new AdminProductViewModel
            {
                Product = product,
                MainCategories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync(),
                SubCategories = await _context.Categories.Where(c => c.ParentCategoryId != null && c.ParentCategory.ParentCategoryId != null).ToListAsync(),
                AllSizes = GetDefaultSizes(),
                AllColors = GetDefaultColors(),
                ExistingImageUrl = product.ImageUrl,
                SelectedSizes = product.AvailableSizes?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                SelectedColors = product.AvailableColors?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, AdminProductViewModel viewModel)
        {
            System.Diagnostics.Debug.WriteLine($"[LOG] EditProduct POST başladı. id: {id}, viewModel.Product.Id: {viewModel.Product.Id}");
            if (id != viewModel.Product.Id) { System.Diagnostics.Debug.WriteLine("[LOG] id uyuşmazlığı, NotFound dönülüyor."); return NotFound(); }
            viewModel.Product.AvailableSizes = string.Join(",", viewModel.SelectedSizes);
            viewModel.Product.AvailableColors = string.Join(",", viewModel.SelectedColors);

            if (ModelState.IsValid)
            {
                try
                {
                    if (viewModel.ProductImage != null && viewModel.ProductImage.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        if (!Directory.Exists(uploadsFolder)) { Directory.CreateDirectory(uploadsFolder); }
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ProductImage.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        if (!string.IsNullOrEmpty(viewModel.ExistingImageUrl) && !viewModel.ExistingImageUrl.Contains("default-product.jpg"))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", viewModel.ExistingImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath)) { System.IO.File.Delete(oldFilePath); }
                        }
                        using (var fileStream = new FileStream(filePath, FileMode.Create)) { await viewModel.ProductImage.CopyToAsync(fileStream); }
                        viewModel.Product.ImageUrl = "/images/" + uniqueFileName;
                    }
                    else { viewModel.Product.ImageUrl = viewModel.ExistingImageUrl; }
                    _context.Update(viewModel.Product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Ürün başarıyla güncellendi!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(viewModel.Product.Id)) { System.Diagnostics.Debug.WriteLine("[LOG] ProductExists false, NotFound dönülüyor."); return NotFound(); }
                    else { throw; }
                }
                System.Diagnostics.Debug.WriteLine("[LOG] EditProduct POST: RedirectToAction(Products) çalışıyor.");
                return RedirectToAction(nameof(Products));
            }
            System.Diagnostics.Debug.WriteLine("[LOG] EditProduct POST: ModelState geçersiz, tekrar view dönülüyor.");
            viewModel.MainCategories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync();
            viewModel.SubCategories = await _context.Categories.Where(c => c.ParentCategoryId != null && c.ParentCategory.ParentCategoryId != null).ToListAsync();
            viewModel.AllSizes = GetDefaultSizes();
            viewModel.AllColors = GetDefaultColors();
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            ViewData["Title"] = "Yeni Ürün Ekle";

            var viewModel = new AdminProductViewModel
            {
                Product = new Product(),
                MainCategories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync(),
                SubCategories = new List<Category>(), // Başlangıçta boş
                AllSizes = GetDefaultSizes(),
                AllColors = GetDefaultColors()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(AdminProductViewModel viewModel)
        {
            viewModel.Product.AvailableSizes = string.Join(",", viewModel.SelectedSizes);
            viewModel.Product.AvailableColors = string.Join(",", viewModel.SelectedColors);

            if (ModelState.IsValid)
            {
                if (viewModel.ProductImage != null && viewModel.ProductImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ProductImage.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.ProductImage.CopyToAsync(fileStream);
                    }

                    viewModel.Product.ImageUrl = "/images/" + uniqueFileName;
                }
                else
                {
                    viewModel.Product.ImageUrl = "/images/default-product.jpg";
                }

                _context.Products.Add(viewModel.Product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ürün başarıyla eklendi!";
                return RedirectToAction(nameof(Products));
            }

            viewModel.MainCategories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync();

            if (viewModel.Product.MainCategoryId != 0)
            {
                viewModel.SubCategories = await _context.Categories
                    .Where(c => c.ParentCategoryId == viewModel.Product.MainCategoryId)
                    .ToListAsync();
            }
            else
            {
                viewModel.SubCategories = new List<Category>();
            }

            viewModel.AllSizes = GetDefaultSizes();
            viewModel.AllColors = GetDefaultColors();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProduct(int? id)
        {
            if (id == null) { return NotFound(); }
            var product = await _context.Products.Include(p => p.MainCategory).Include(p => p.SubCategory).FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) { return NotFound(); }
            ViewData["Title"] = "Ürün Sil";
            return View(product);
        }

        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Ürünü silmek yerine pasif hale getir
                product.IsActive = false;
                _context.Products.Update(product);

                // Bu ürünü sepetlerden kaldır
                var cartItemsToRemove = await _context.CartItems
                    .Where(ci => ci.ProductId == id)
                    .ToListAsync();

                if (cartItemsToRemove.Any())
                {
                    _context.CartItems.RemoveRange(cartItemsToRemove);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ürün başarıyla pasif hale getirildi ve sepetlerden kaldırıldı!";
            }
            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Ürünü aktif hale getir
                product.IsActive = true;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ürün başarıyla aktif hale getirildi!";
            }
            return RedirectToAction(nameof(Products));
        }

        // Geçici test action'ı - ürünlerin ImageUrl değerlerini kontrol etmek için
        [HttpGet]
        public async Task<IActionResult> TestImages()
        {
            var products = await _context.Products.ToListAsync();
            var imageInfo = products.Select(p => new { 
                p.Id, 
                p.Name, 
                ImageUrl = p.ImageUrl ?? "NULL",
                IsActive = p.IsActive
            }).ToList();
            return Json(imageInfo);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        private List<string> GetDefaultSizes()
        {
            return new List<string> { "XS", "S", "M", "L", "XL", "XXL", "28", "30", "32", "34", "36", "38", "40", "42" };
        }

        private List<string> GetDefaultColors()
        {
            return new List<string> { "Beyaz", "Siyah", "Mavi", "Kırmızı", "Yeşil", "Gri", "Sarı", "Pembe", "Kahverengi" };
        }
    }
} 