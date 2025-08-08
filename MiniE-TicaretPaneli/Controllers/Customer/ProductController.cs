using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniE_TicaretPaneli.Data;
using MiniE_TicaretPaneli.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MiniE_TicaretPaneli.Controllers.Customer
{
    [Route("Customer/[controller]")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Products Action
        [Route("Products")]
        [Route("")]
        public async Task<IActionResult> Products(string? gender = null, string? searchTerm = null, int? mainCategoryId = null, int? subCategoryId = null, string? brand = null)
        {
            var query = _context.Products
                .Include(p => p.MainCategory)
                .Include(p => p.SubCategory)
                .Where(p => p.IsActive) // Sadece aktif ürünleri göster
                .AsQueryable();

            // Gender filtrelemesi - kategorilerin Gender alanına göre
            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(p => p.MainCategory.Gender == gender || p.SubCategory.Gender == gender);
            }

            // Arama filtrelemesi
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }

            // Ana kategori filtrelemesi
            if (mainCategoryId.HasValue)
            {
                query = query.Where(p => p.MainCategoryId == mainCategoryId.Value);
            }

            // Alt kategori filtrelemesi
            if (subCategoryId.HasValue)
            {
                query = query.Where(p => p.SubCategoryId == subCategoryId.Value);
            }

            // Marka filtrelemesi
            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.Brand == brand);
            }

            var products = await query.ToListAsync();
            
            // ViewBag'leri set et
            ViewBag.MainCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();
                
            ViewBag.Brands = await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.Brand) && p.IsActive) // Sadece aktif ürünlerin markalarını al
                .Select(p => p.Brand)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();
            
            return View("~/Views/Customer/Product/Products.cshtml", products);
        }

        // ProductDetail Action
        [Route("ProductDetail/{id:int}")]
        public async Task<IActionResult> ProductDetail(int? id)
        {
            if (id == null)
                return View("~/Views/Customer/Product/NotFound.cshtml");
            var product = await _context.Products
                .Include(p => p.MainCategory)
                .Include(p => p.SubCategory)
                .Where(p => p.IsActive)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return View("~/Views/Customer/Product/NotFound.cshtml");
            return View("~/Views/Customer/Product/ProductDetail.cshtml", product);
        }

        // GetSubCategories Action
        [HttpGet]
        [Route("GetSubCategories")]
        public async Task<IActionResult> GetSubCategories(int mainCategoryId)
        {
            var subCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == mainCategoryId)
                .OrderBy(c => c.Name)
                .Select(c => new { id = c.Id, name = c.Name })
                .ToListAsync();
                
            return Json(subCategories);
        }
    }
}
