// Controllers/AdminController.cs
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniE_TicaretPaneli.Data;
using MiniE_TicaretPaneli.Models;
using MiniE_TicaretPaneli.Models.ViewModels;
// using MiniE_TicaretPaneli.Enums; // Bu satır SİLİNMİŞ OLMALI - UserRole Models namespace'inde olduğu için gerek yok.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace MiniE_TicaretPaneli.Controllers
{
    /*[Authorize(Roles = "Admin")]*/ // Geçici olarak yorum satırı yapmıştık, sorunu çözünce geri açarız.
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult AdminDashboard()
        {
            ViewData["Title"] = "Admin Paneli Dashboard";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Products()
        {
            ViewData["Title"] = "Ürün Yönetimi";
            var products = await _context.Products
                                       .Include(p => p.MainCategory)
                                       .Include(p => p.SubCategory)
                                       .ToListAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            ViewData["Title"] = "Yeni Ürün Ekle";
            var viewModel = new AdminProductViewModel
            {
                MainCategories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync(),
                SubCategories = new List<Category>(),
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
                    if (!Directory.Exists(uploadsFolder)) { Directory.CreateDirectory(uploadsFolder); }
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

                _context.Add(viewModel.Product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ürün başarıyla eklendi!";
                return RedirectToAction(nameof(Products));
            }

            // Dropdownlar için kategorileri tekrar doldur
            viewModel.MainCategories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync();
            if (viewModel.Product.MainCategoryId != 0 && !viewModel.MainCategories.Any(c => c.Id == viewModel.Product.MainCategoryId))
            {
                var selectedMain = await _context.Categories.FirstOrDefaultAsync(c => c.Id == viewModel.Product.MainCategoryId)
                    ?? new Category { Id = viewModel.Product.MainCategoryId, Name = $"(Geçersiz Kategori: {viewModel.Product.MainCategoryId})" };
                viewModel.MainCategories.Add(selectedMain);
            }
            if (viewModel.Product.MainCategoryId != 0)
            {
                viewModel.SubCategories = await _context.Categories.Where(c => c.ParentCategoryId == viewModel.Product.MainCategoryId).ToListAsync();
            }
            else
            {
                viewModel.SubCategories = new List<Category>();
            }
            if (viewModel.Product.SubCategoryId != 0 && !viewModel.SubCategories.Any(c => c.Id == viewModel.Product.SubCategoryId))
            {
                var selectedSub = await _context.Categories.FirstOrDefaultAsync(c => c.Id == viewModel.Product.SubCategoryId)
                    ?? new Category { Id = viewModel.Product.SubCategoryId, Name = $"(Geçersiz Alt Kategori: {viewModel.Product.SubCategoryId})" };
                viewModel.SubCategories.Add(selectedSub);
            }
            viewModel.AllSizes = GetDefaultSizes();
            viewModel.AllColors = GetDefaultColors();
            return View(viewModel);
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
                MainCategories = await _context.Categories.Where(c => c.ParentCategoryId != null).ToListAsync(),
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
            if (id != viewModel.Product.Id) { return NotFound(); }
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
                    if (!ProductExists(viewModel.Product.Id)) { return NotFound(); }
                    else { throw; }
                }
                return RedirectToAction(nameof(Products));
            }
            viewModel.MainCategories = await _context.Categories.Where(c => c.ParentCategoryId != null).ToListAsync();
            viewModel.SubCategories = await _context.Categories.Where(c => c.ParentCategoryId != null && c.ParentCategory.ParentCategoryId != null).ToListAsync();
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
                if (!string.IsNullOrEmpty(product.ImageUrl) && !product.ImageUrl.Contains("default-product.jpg"))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath)) { System.IO.File.Delete(filePath); }
                }
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ürün başarıyla silindi!";
            }
            return RedirectToAction(nameof(Products));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            ViewData["Title"] = "Müşteri Görüntüleme";
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            ViewData["Title"] = "Yeni Kullanıcı Ekle";
            ViewBag.Roles = Enum.GetValues(typeof(UserRole))
                                .Cast<UserRole>()
                                .Select(r => new { Value = r.ToString(), Text = r.ToString() })
                                .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(
            [Bind("Username,PasswordHash,FirstName,LastName,Email,PhoneNumber,Role")] User user)
        {
            if (!string.IsNullOrEmpty(user.PasswordHash) && !IsPasswordComplex(user.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Şifre en az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.");
            }
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Username == user.Username)) { ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış."); }
                if (_context.Users.Any(u => u.Email == user.Email)) { ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor."); }
                if (!string.IsNullOrEmpty(user.PhoneNumber) && _context.Users.Any(u => u.PhoneNumber == user.PhoneNumber)) { ModelState.AddModelError("PhoneNumber", "Bu telefon numarası zaten kullanılıyor."); }
                if (ModelState.IsValid)
                {
                    user.Role = UserRole.Customer;
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Kullanıcı başarıyla eklendi!";
                    return RedirectToAction(nameof(Users));
                }
            }
            ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>().Select(r => new { Value = r.ToString(), Text = r.ToString() }).ToList();
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Sales()
        {
            ViewData["Title"] = "Satış Görüntüleme";
            var orders = await _context.Orders.Include(o => o.User).Include(o => o.OrderItems).ThenInclude(oi => oi.Product).OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            ViewData["Title"] = "Kategori Yönetimi";
            var categories = await _context.Categories.Include(c => c.ParentCategory).ToListAsync();
            return View(categories);
        }
// GET: /Admin/AddCategory
[HttpGet]
public async Task<IActionResult> AddCategory()
{
    ViewData["Title"] = "Yeni Kategori Ekle";
    // Sadece kök kategoriler (ParentCategoryId == null) üst kategori olarak gösterilsin
    var mainCategories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync();
    var viewModel = new AddCategoryViewModel
    {
        AllCategoriesForJs = await _context.Categories.Include(c => c.ParentCategory).ToListAsync(),
        AvailableParentCategories = new List<SelectListItem>(),
        MainCategories = mainCategories
    };
    return View(viewModel);
}

// POST: /Admin/AddCategory
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddCategory(AddCategoryViewModel viewModel)
{
    var newCategory = new Category
    {
        Name = viewModel.Name,
        ParentCategoryId = viewModel.ParentCategoryId,
        Gender = viewModel.Gender,
        Slug = viewModel.Slug
    };
    if (string.IsNullOrEmpty(newCategory.Slug))
    {
        newCategory.Slug = newCategory.Name.ToLower().Replace(" ", "-").Replace("/", "-");
    }
    if (ModelState.IsValid)
    {
        if (newCategory.ParentCategoryId.HasValue)
        {
            if (_context.Categories.Any(c => c.Name == newCategory.Name && c.ParentCategoryId == newCategory.ParentCategoryId && c.Gender == newCategory.Gender))
            {
                ModelState.AddModelError("Name", "Bu isimde bu cinsiyete ait bir alt kategori bu üst kategori altında zaten mevcut.");
            }
        }
        else
        {
            if (_context.Categories.Any(c => c.Name == newCategory.Name && c.ParentCategoryId == null && c.Gender == newCategory.Gender))
            {
                ModelState.AddModelError("Name", "Bu isimde bu cinsiyete ait bir ana kategori zaten mevcut.");
            }
        }
        if (ModelState.IsValid)
        {
            _context.Add(newCategory);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kategori başarıyla eklendi!";
            return RedirectToAction(nameof(Categories));
        }
    }
    viewModel.AllCategoriesForJs = await _context.Categories.Include(c => c.ParentCategory).ToListAsync();
    // Sadece kök kategoriler (ParentCategoryId == null) üst kategori olarak gösterilsin
    viewModel.MainCategories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync();
    if (!string.IsNullOrEmpty(viewModel.CategoryLevelType))
    {
        viewModel.AvailableParentCategories = await GetParentCategoriesForLevel(viewModel.CategoryLevelType, viewModel.Gender).ToListAsync();
    }
    return View(viewModel);
}
        
        // Yeni Yardımcı Metot: Kategori Seviyesine Göre Üst Kategorileri Getirme
        // Bu metot AddCategory POST'unda hata durumunda ViewBag'i doldurmak için ve JS'e data sağlamak için kullanılacak.
        private IQueryable<SelectListItem> GetParentCategoriesForLevel(string categoryLevelType, string? gender)
        {
            var query = _context.Categories.AsQueryable();

            if (categoryLevelType == "MainGroup") // Kullanıcı Ana Kategori (Level 2) eklemek istiyor
            {
                // Üst kategori olarak sadece cinsiyet/yaş grubu kategorilerini göster (Level 1)
                query = query.Where(c => c.ParentCategoryId == null && (c.Gender == "Kadın" || c.Gender == "Erkek" || c.Gender == "Çocuk"));
                if (!string.IsNullOrEmpty(gender))
                {
                    // Cinsiyet seçili ise sadece ilgili cinsiyet kategorisini filtrele
                    if (gender == "Kadın") query = query.Where(c => c.Name == "Kadın");
                    else if (gender == "Erkek") query = query.Where(c => c.Name == "Erkek");
                    else if (gender == "Çocuk") query = query.Where(c => c.Name == "Anne & Çocuk");
                }
            }
            else if (categoryLevelType == "ProductType") // Kullanıcı Alt Kategori (Level 3) eklemek istiyor
            {
                // Üst kategori olarak sadece Ürün Grubu veya Yaş Grubu Kategori kategorilerini göster (Level 2)
                query = query.Where(c => c.ParentCategoryId != null && (c.Gender == "Ürün Grubu" || c.Gender == "Yaş Grubu Kategori"));
                if (!string.IsNullOrEmpty(gender))
                {
                    // Eğer cinsiyet seçiliyse, o cinsiyete ait seviye 2 kategorileri getir
                    query = query.Where(c => c.Gender == gender);
                }
            }
            // Diğer seviyeler için else if eklenebilir

            return query.OrderBy(c => c.Name)
                        .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name + (c.ParentCategory != null ? " (" + c.ParentCategory.Name + ")" : "") });
        }
    
        [HttpGet]
        public async Task<IActionResult> EditCategory(int? id)
        {
            if (id == null) { return NotFound(); }
            var category = await _context.Categories.FindAsync(id);
            if (category == null) { return NotFound(); }

            ViewData["Title"] = "Kategori Düzenle";
            ViewBag.ParentCategories = await _context.Categories
                                                    .Where(c => c.Id != id) // Kendini üst kategori olarak seçemesin
                                                    .OrderBy(c => c.Name)
                                                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name + (c.ParentCategory != null ? " (" + c.ParentCategory.Name + ")" : "") })
                                                    .ToListAsync();
            return View(category);
        }

        // POST: /Admin/EditCategory/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category category)
        {
            if (id != category.Id) { return NotFound(); }

            if (string.IsNullOrEmpty(category.Slug)) { category.Slug = category.Name.ToLower().Replace(" ", "-"); }
            if (ModelState.IsValid)
            {
                try
                {
                    if (category.ParentCategoryId.HasValue)
                    {
                        if (_context.Categories.Any(c => c.Name == category.Name && c.ParentCategoryId == category.ParentCategoryId && c.Gender == category.Gender && c.Id != category.Id))
                        {
                            ModelState.AddModelError("Name", "Bu isimde bu cinsiyete ait bir alt kategori bu ana kategori altında zaten mevcut.");
                            ViewBag.ParentCategories = await _context.Categories
                                                    .Where(c => c.Id != id)
                                                    .OrderBy(c => c.Name)
                                                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name + (c.ParentCategory != null ? " (" + c.ParentCategory.Name + ")" : "") })
                                                    .ToListAsync();
                            return View(category);
                        }
                    }
                    else
                    {
                        if (_context.Categories.Any(c => c.Name == category.Name && c.ParentCategoryId == null && c.Gender == category.Gender && c.Id != category.Id))
                        {
                            ModelState.AddModelError("Name", "Bu isimde bu cinsiyete ait bir ana kategori zaten mevcut.");
                            ViewBag.ParentCategories = await _context.Categories
                                                    .Where(c => c.Id != id)
                                                    .OrderBy(c => c.Name)
                                                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name + (c.ParentCategory != null ? " (" + c.ParentCategory.Name + ")" : "") })
                                                    .ToListAsync();
                            return View(category);
                        }
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Kategori başarıyla güncellendi!";
                    return RedirectToAction(nameof(Categories));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id)) { return NotFound(); }
                    else { throw; }
                }
            }
            ViewBag.ParentCategories = await _context.Categories
                                                    .Where(c => c.Id != id)
                                                    .OrderBy(c => c.Name)
                                                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name + (c.ParentCategory != null ? " (" + c.ParentCategory.Name + ")" : "") })
                                                    .ToListAsync();
            return View(category);
        }

        // GET: /Admin/DeleteCategory/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) { return NotFound("Kategori bulunamadı."); }
            if (await _context.Products.AnyAsync(p => p.MainCategoryId == id || p.SubCategoryId == id) ||
                await _context.Categories.AnyAsync(c => c.ParentCategoryId == id))
            {
                return BadRequest("Bu kategoriye bağlı ürünler veya alt kategoriler bulunmaktadır. Silinemez!");
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // Helper Method: CategoryExists
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
     
        // AJAX Action: Ana kategoriye göre alt kategorileri getirir
        [HttpGet]
        public async Task<JsonResult> GetSubcategoriesByMainCategory(int mainCategoryId)
        {
            var subcategories = await _context.Categories
                                              .Where(c => c.ParentCategoryId == mainCategoryId)
                                              .Select(c => new { id = c.Id, name = c.Name, gender = c.Gender })
                                              .ToListAsync();
            return Json(subcategories);
        }

        [HttpGet]
        public async Task<JsonResult> GetMainCategories()
        {
            var mainCategories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Where(c => c.ParentCategoryId != null)
                .Select(c => new { id = c.Id, name = c.Name, parentName = c.ParentCategory != null ? c.ParentCategory.Name : "" })
                .ToListAsync();
            return Json(mainCategories);
        }

        [HttpGet]
        public async Task<JsonResult> GetAllMainCategoriesWithParent()
        {
            var mainCategories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Where(c => c.ParentCategoryId != null)
                .Select(c => new {
                    id = c.Id,
                    name = c.Name,
                    parentName = c.ParentCategory != null ? c.ParentCategory.Name : ""
                })
                .ToListAsync();
            return Json(mainCategories);
        }

        [HttpGet]
        public async Task<JsonResult> GetRootCategories()
        {
            var rootCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .Select(c => new { id = c.Id, name = c.Name })
                .ToListAsync();
            return Json(rootCategories);
        }

        // Varsayılan Bedenleri Getiren Yardımcı Metot
        private List<string> GetDefaultSizes()
        {
            return new List<string> { "XS", "S", "M", "L", "XL", "XXL", "28", "30", "32", "34", "36", "38", "40", "42" };
        }

        // Varsayılan Renkleri Getiren Yardımcı Metot
        private List<string> GetDefaultColors()
        {
            return new List<string> { "Beyaz", "Siyah", "Mavi", "Kırmızı", "Yeşil", "Gri", "Sarı", "Pembe", "Kahverengi" };
        }

        // Şifre Karmaşıklığı Kontrolü Metodu
        private bool IsPasswordComplex(string password)
        {
            if (password.Length < 8) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (!password.Any(ch => !char.IsLetterOrDigit(ch) && !char.IsWhiteSpace(ch))) return false;
            return true;
        }
    }
}