// Controllers/AdminController.cs
<<<<<<< HEAD
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
=======
using System.IO; // Dosya işlemleri için
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // [Authorize] attribute'u için
using MiniE_TicaretPaneli.Data; // DbContext için
using MiniE_TicaretPaneli.Models; // Modeller (Product, User, Order, OrderItem, Category, ShoppingCart) için
using MiniE_TicaretPaneli.Models.ViewModels; // <<<<<< AdminProductViewModel için gerekli

using System.Linq; // LINQ metotları için
using System.Threading.Tasks; // Asenkron metotlar için
using Microsoft.EntityFrameworkCore; // Include, ToListAsync için
using System; // Guid, DateTime, Enum için
using System.Collections.Generic; // List için
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList için (ViewBag.Roles için hala kullanılıyor)
>>>>>>> origin/master


namespace MiniE_TicaretPaneli.Controllers
{
<<<<<<< HEAD
    /*[Authorize(Roles = "Admin")]*/ // Geçici olarak yorum satırı yapmıştık, sorunu çözünce geri açarız.
=======
    [Authorize(Roles = "Admin")] // Bu Controller'daki tüm Action'lara sadece Admin rolündekiler erişebilir
>>>>>>> origin/master
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

<<<<<<< HEAD
        [HttpGet]
=======
        // GET: /Admin/AdminDashboard
>>>>>>> origin/master
        public IActionResult AdminDashboard()
        {
            ViewData["Title"] = "Admin Paneli Dashboard";
            return View();
        }

<<<<<<< HEAD
        [HttpGet]
        public async Task<IActionResult> Products()
        {
            ViewData["Title"] = "Ürün Yönetimi";
            var products = await _context.Products
                                       .Include(p => p.GenderCategory)
=======
        // GET: /Admin/Products
        // Tüm ürünleri listele
        public async Task<IActionResult> Products()
        {
            ViewData["Title"] = "Ürün Yönetimi";
            // Ürünleri ilgili kategorileriyle birlikte çek
            var products = await _context.Products
>>>>>>> origin/master
                                       .Include(p => p.MainCategory)
                                       .Include(p => p.SubCategory)
                                       .ToListAsync();
            return View(products);
        }

<<<<<<< HEAD
        [HttpGet]
=======
        // GET: /Admin/AddProduct (Yeni ürün ekleme formu)
>>>>>>> origin/master
        public async Task<IActionResult> AddProduct()
        {
            ViewData["Title"] = "Yeni Ürün Ekle";

            var viewModel = new AdminProductViewModel
            {
<<<<<<< HEAD
                GenderCategories = await _context.Categories
                                                 .Where(c => c.ParentCategoryId == null && (c.Type == "Cinsiyet" || c.Type == "Yaş Grubu"))
                                                 .ToListAsync(),
                MainCategories = await _context.Categories
                                               .Where(c => c.ParentCategory != null && (c.ParentCategory.Type == "Cinsiyet" || c.ParentCategory.Type == "Yaş Grubu"))
                                               .ToListAsync(),
                SubCategories = await _context.Categories
                                              .Where(c => c.ParentCategory != null && c.ParentCategory.ParentCategory != null && (c.ParentCategory.Type == "Ürün Grubu" || c.ParentCategory.Type == "Yaş Grubu Kategori"))
                                              .ToListAsync(),

=======
                // Ana kategoriler: Sadece cinsiyet/yaş grubu olanları getir (Kadın, Erkek, Anne & Çocuk)
                // Bu liste JavaScript'e tam gönderilecek ve JavaScript Product.Gender'a göre filtreleyecek.
                MainCategories = await _context.Categories
                                               .Where(c => c.ParentCategoryId == null && (c.Type == "Cinsiyet" || c.Type == "Yaş Grubu"))
                                               .ToListAsync(),
                SubCategories = new List<Category>(), // Başlangıçta alt kategoriler boş
>>>>>>> origin/master
                AllSizes = GetDefaultSizes(),
                AllColors = GetDefaultColors()
            };
            return View(viewModel);
        }

<<<<<<< HEAD
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
=======
        // POST: /Admin/AddProduct (Yeni ürün kaydetme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(AdminProductViewModel viewModel) // <<<<< ViewModel parametresi kullanılıyor
        {
            // Seçilen beden ve renkleri Product modeline kaydetmeden önce birleştir
            viewModel.Product.AvailableSizes = string.Join(",", viewModel.SelectedSizes);
            viewModel.Product.AvailableColors = string.Join(",", viewModel.SelectedColors);

            // Model doğrulamasını yap
            // Product modelindeki Data Annotations (örn: [Required]) ViewModel üzerinden çalışır
            if (ModelState.IsValid)
            {
                // Resim yükleme işlemleri
                if (viewModel.ProductImage != null && viewModel.ProductImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
>>>>>>> origin/master
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
<<<<<<< HEAD
                    viewModel.Product.ImageUrl = "/images/default-product.jpg";
                }

                _context.Add(viewModel.Product);
=======
                    viewModel.Product.ImageUrl = "/images/default-product.jpg"; // Resim yüklenmediyse varsayılan
                }

                _context.Add(viewModel.Product); // ViewModel'deki Product nesnesini kaydet
>>>>>>> origin/master
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ürün başarıyla eklendi!";
                return RedirectToAction(nameof(Products));
            }

<<<<<<< HEAD
            viewModel.GenderCategories = await _context.Categories
                                                 .Where(c => c.ParentCategoryId == null && (c.Type == "Cinsiyet" || c.Type == "Yaş Grubu"))
                                                 .ToListAsync();
            viewModel.MainCategories = await _context.Categories
                                               .Where(c => c.ParentCategory != null && (c.ParentCategory.Type == "Cinsiyet" || c.ParentCategory.Type == "Yaş Grubu"))
                                               .ToListAsync();
            viewModel.SubCategories = await _context.Categories
                                              .Where(c => c.ParentCategory != null && c.ParentCategory.ParentCategory != null && (c.ParentCategory.Type == "Ürün Grubu" || c.ParentCategory.Type == "Yaş Grubu Kategori"))
                                              .ToListAsync();
=======
            // Doğrulama hatası varsa, ViewModel'i tekrar doldurup View'a geri gönder
            viewModel.MainCategories = await _context.Categories
                                                   .Where(c => c.ParentCategoryId == null && (c.Type == "Cinsiyet" || c.Type == "Yaş Grubu"))
                                                   .ToListAsync();
            // ModelState.IsValid false olduğunda, Product.MainCategoryId değeri doğru gelirse
            // o ana kategoriye ait alt kategorileri tekrar yükle
            if (viewModel.Product.MainCategoryId != 0)
            {
                viewModel.SubCategories = await _context.Categories.Where(c => c.ParentCategoryId == viewModel.Product.MainCategoryId).ToListAsync();
            }
            else
            {
                viewModel.SubCategories = new List<Category>(); // Ana kategori seçili değilse alt kategoriler boş
            }
>>>>>>> origin/master

            viewModel.AllSizes = GetDefaultSizes();
            viewModel.AllColors = GetDefaultColors();
            return View(viewModel);
        }

<<<<<<< HEAD
        [HttpGet]
        public async Task<IActionResult> EditProduct(int? id)
        {
            if (id == null) { return NotFound(); }
            var product = await _context.Products.FindAsync(id);
            if (product == null) { return NotFound(); }
=======
        // GET: /Admin/EditProduct/5 (Ürün düzenleme formu)
        public async Task<IActionResult> EditProduct(int? id)
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

>>>>>>> origin/master
            ViewData["Title"] = "Ürün Düzenle";
            var viewModel = new AdminProductViewModel
            {
                Product = product,
<<<<<<< HEAD
                GenderCategories = await _context.Categories.Where(c => c.ParentCategoryId == null && (c.Type == "Cinsiyet" || c.Type == "Yaş Grubu")).ToListAsync(),
                MainCategories = await _context.Categories.Where(c => c.ParentCategory != null && (c.ParentCategory.Type == "Cinsiyet" || c.ParentCategory.Type == "Yaş Grubu")).ToListAsync(),
                SubCategories = await _context.Categories.Where(c => c.ParentCategory != null && c.ParentCategory.ParentCategory != null && (c.ParentCategory.Type == "Ürün Grubu" || c.ParentCategory.Type == "Yaş Grubu Kategori")).ToListAsync(),
                AllSizes = GetDefaultSizes(),
                AllColors = GetDefaultColors(),
                ExistingImageUrl = product.ImageUrl,
=======
                // Ana kategoriler: Sadece cinsiyet/yaş grubu olanları getir
                MainCategories = await _context.Categories
                                               .Where(c => c.ParentCategoryId == null && (c.Type == "Cinsiyet" || c.Type == "Yaş Grubu"))
                                               .ToListAsync(),
                // Seçilen ana kategoriye göre alt kategorileri önceden yükle
                SubCategories = await _context.Categories.Where(c => c.ParentCategoryId == product.MainCategoryId).ToListAsync(),
                AllSizes = GetDefaultSizes(),
                AllColors = GetDefaultColors(),
                ExistingImageUrl = product.ImageUrl,
                // Kayıtlı beden ve renkleri liste olarak ayır
>>>>>>> origin/master
                SelectedSizes = product.AvailableSizes?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                SelectedColors = product.AvailableColors?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>()
            };
            return View(viewModel);
        }

<<<<<<< HEAD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, AdminProductViewModel viewModel)
        {
            if (id != viewModel.Product.Id) { return NotFound(); }
=======
        // POST: /Admin/EditProduct/5 (Ürünü güncelleme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, AdminProductViewModel viewModel) // <<<<< ViewModel parametresi kullanılıyor
        {
            if (id != viewModel.Product.Id)
            {
                return NotFound();
            }

            // Seçilen beden ve renkleri Product modeline kaydetmeden önce birleştir
>>>>>>> origin/master
            viewModel.Product.AvailableSizes = string.Join(",", viewModel.SelectedSizes);
            viewModel.Product.AvailableColors = string.Join(",", viewModel.SelectedColors);

            if (ModelState.IsValid)
            {
                try
                {
<<<<<<< HEAD
=======
                    // Resim yükleme işlemleri
>>>>>>> origin/master
                    if (viewModel.ProductImage != null && viewModel.ProductImage.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        if (!Directory.Exists(uploadsFolder)) { Directory.CreateDirectory(uploadsFolder); }
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ProductImage.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
<<<<<<< HEAD
                        if (!string.IsNullOrEmpty(viewModel.ExistingImageUrl) && !viewModel.ExistingImageUrl.Contains("default-product.jpg"))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", viewModel.ExistingImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath)) { System.IO.File.Delete(oldFilePath); }
                        }
                        using (var fileStream = new FileStream(filePath, FileMode.Create)) { await viewModel.ProductImage.CopyToAsync(fileStream); }
                        viewModel.Product.ImageUrl = "/images/" + uniqueFileName;
                    }
                    else { viewModel.Product.ImageUrl = viewModel.ExistingImageUrl; }
=======

                        if (!string.IsNullOrEmpty(viewModel.ExistingImageUrl) && !viewModel.ExistingImageUrl.Contains("default-product.jpg"))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", viewModel.ExistingImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.ProductImage.CopyToAsync(fileStream);
                        }
                        viewModel.Product.ImageUrl = "/images/" + uniqueFileName;
                    }
                    else
                    {
                        viewModel.Product.ImageUrl = viewModel.ExistingImageUrl; // Yeni resim yoksa mevcut resmi koru
                    }

>>>>>>> origin/master
                    _context.Update(viewModel.Product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Ürün başarıyla güncellendi!";
                }
                catch (DbUpdateConcurrencyException)
                {
<<<<<<< HEAD
                    if (!ProductExists(viewModel.Product.Id)) { return NotFound(); }
                    else { throw; }
                }
                return RedirectToAction(nameof(Products));
            }
            viewModel.GenderCategories = await _context.Categories.Where(c => c.ParentCategoryId == null && (c.Type == "Cinsiyet" || c.Type == "Yaş Grubu")).ToListAsync();
            viewModel.MainCategories = await _context.Categories.Where(c => c.ParentCategory != null && (c.ParentCategory.Type == "Cinsiyet" || c.ParentCategory.Type == "Yaş Grubu")).ToListAsync();
            viewModel.SubCategories = await _context.Categories.Where(c => c.ParentCategory != null && c.ParentCategory.ParentCategory != null && (c.ParentCategory.Type == "Ürün Grubu" || c.ParentCategory.Type == "Yaş Grubu Kategori")).ToListAsync();
=======
                    if (!ProductExists(viewModel.Product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Products));
            }

            // Doğrulama hatası varsa, ViewModel'i tekrar doldurup View'a geri gönder
            viewModel.MainCategories = await _context.Categories
                                                   .Where(c => c.ParentCategoryId == null && (c.Type == "Cinsiyet" || c.Type == "Yaş Grubu"))
                                                   .ToListAsync();
            if (viewModel.Product.MainCategoryId != 0)
            {
                viewModel.SubCategories = await _context.Categories.Where(c => c.ParentCategoryId == viewModel.Product.MainCategoryId).ToListAsync();
            }
            else
            {
                viewModel.SubCategories = new List<Category>();
            }
>>>>>>> origin/master
            viewModel.AllSizes = GetDefaultSizes();
            viewModel.AllColors = GetDefaultColors();
            return View(viewModel);
        }

<<<<<<< HEAD
        [HttpGet]
        public async Task<IActionResult> DeleteProduct(int? id)
        {
            if (id == null) { return NotFound(); }
            var product = await _context.Products.Include(p => p.GenderCategory).Include(p => p.MainCategory).Include(p => p.SubCategory).FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) { return NotFound(); }
=======
        // GET: /Admin/DeleteProduct/5 (Ürün silme onay sayfası)
        public async Task<IActionResult> DeleteProduct(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                                        .Include(p => p.MainCategory)
                                        .Include(p => p.SubCategory)
                                        .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

>>>>>>> origin/master
            ViewData["Title"] = "Ürün Sil";
            return View(product);
        }

<<<<<<< HEAD
=======
        // POST: /Admin/DeleteProduct/5 (Ürünü silme işlemi)
>>>>>>> origin/master
        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
<<<<<<< HEAD
                if (!string.IsNullOrEmpty(product.ImageUrl) && !product.ImageUrl.Contains("default-product.jpg"))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath)) { System.IO.File.Delete(filePath); }
                }
=======
                // İlişkili resmi de sil (varsa ve varsayılan değilse)
                if (!string.IsNullOrEmpty(product.ImageUrl) && !product.ImageUrl.Contains("default-product.jpg"))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

>>>>>>> origin/master
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

<<<<<<< HEAD
        [HttpGet]
=======
        // GET: /Admin/Users (Müşteri Görüntüleme)
>>>>>>> origin/master
        public async Task<IActionResult> Users()
        {
            ViewData["Title"] = "Müşteri Görüntüleme";
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

<<<<<<< HEAD
        [HttpGet]
        public IActionResult AddUser()
        {
            ViewData["Title"] = "Yeni Kullanıcı Ekle";
=======
        // GET: /Admin/AddUser (Yeni kullanıcı ekleme formu)
        public IActionResult AddUser()
        {
            ViewData["Title"] = "Yeni Kullanıcı Ekle";
            // Adminin rol seçebilmesi için roller listesini gönder
>>>>>>> origin/master
            ViewBag.Roles = Enum.GetValues(typeof(UserRole))
                                .Cast<UserRole>()
                                .Select(r => new { Value = r.ToString(), Text = r.ToString() })
                                .ToList();
            return View();
        }

<<<<<<< HEAD
=======
        // POST: /Admin/AddUser (Yeni kullanıcı kaydetme)
>>>>>>> origin/master
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(
            [Bind("Username,PasswordHash,FirstName,LastName,Email,PhoneNumber,Role")] User user)
        {
<<<<<<< HEAD
=======
            // Şifre Karmaşıklığı Kontrolü
>>>>>>> origin/master
            if (!string.IsNullOrEmpty(user.PasswordHash) && !IsPasswordComplex(user.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Şifre en az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.");
            }
<<<<<<< HEAD
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Username == user.Username)) { ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış."); }
                if (_context.Users.Any(u => u.Email == user.Email)) { ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor."); }
                if (!string.IsNullOrEmpty(user.PhoneNumber) && _context.Users.Any(u => u.PhoneNumber == user.PhoneNumber)) { ModelState.AddModelError("PhoneNumber", "Bu telefon numarası zaten kullanılıyor."); }
                if (ModelState.IsValid)
                {
                    user.Role = UserRole.Customer;
=======

            if (ModelState.IsValid)
            {
                // Kullanıcı adı, e-posta veya telefon numarası zaten kullanılıyor mu?
                if (_context.Users.Any(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış.");
                }
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                }
                if (!string.IsNullOrEmpty(user.PhoneNumber) && _context.Users.Any(u => u.PhoneNumber == user.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "Bu telefon numarası zaten kullanılıyor.");
                }

                if (ModelState.IsValid)
                {
>>>>>>> origin/master
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Kullanıcı başarıyla eklendi!";
                    return RedirectToAction(nameof(Users));
                }
            }
<<<<<<< HEAD
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
=======

            ViewBag.Roles = Enum.GetValues(typeof(UserRole))
                                .Cast<UserRole>()
                                .Select(r => new { Value = r.ToString(), Text = r.ToString() })
                                .ToList();
            return View(user);
        }

        // GET: /Admin/Sales (Satış Görüntüleme)
        public async Task<IActionResult> Sales()
        {
            ViewData["Title"] = "Satış Görüntüleme";
            var orders = await _context.Orders
                                     .Include(o => o.User)
                                     .Include(o => o.OrderItems)
                                         .ThenInclude(oi => oi.Product)
                                     .OrderByDescending(o => o.OrderDate)
                                     .ToListAsync();
            return View(orders);
        }

        // GET: /Admin/Categories (Kategori Yönetimi)
>>>>>>> origin/master
        public async Task<IActionResult> Categories()
        {
            ViewData["Title"] = "Kategori Yönetimi";
            var categories = await _context.Categories.Include(c => c.ParentCategory).ToListAsync();
            return View(categories);
        }
<<<<<<< HEAD
[HttpGet]
        public async Task<IActionResult> AddCategory()
        {
            ViewData["Title"] = "Yeni Kategori Ekle";
            var viewModel = new AddCategoryViewModel
            {
                // Tüm kategorileri çekip JS'ye gönderiyoruz, JS filtrelemeyi yapacak
                AllCategoriesForJs = await _context.Categories.Include(c => c.ParentCategory).ToListAsync(),
                // Başlangıçta üst kategori listesi boş olabilir, JS dolduracak
                AvailableParentCategories = new List<SelectListItem>()
            };
            return View(viewModel);
        }

        // POST: /Admin/AddCategory (Yeni Kategori Kaydetme - Yeniden Yazıldı)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(AddCategoryViewModel viewModel)
        {
            // ViewModel'deki alanları kullanarak yeni bir Category objesi oluşturuyoruz
            var newCategory = new Category
            {
                Name = viewModel.Name,
                ParentCategoryId = viewModel.ParentCategoryId,
                Gender = viewModel.Gender,
                Type = viewModel.Type,
                Value = viewModel.Value,
                Slug = viewModel.Slug
            };

            // Slug'ı otomatik oluştur (eğer boşsa)
            if (string.IsNullOrEmpty(newCategory.Slug))
            {
                newCategory.Slug = newCategory.Name.ToLower().Replace(" ", "-").Replace("/", "-"); // / işaretini de temizle
            }

            // Model doğrulama ve sunucu tarafı kontrolleri
            // Gerekli alanlar ViewModel'de [Required] ile işaretlendi
            if (ModelState.IsValid)
            {
                // Benzersizlik kontrolleri
                if (newCategory.ParentCategoryId.HasValue) // Alt kategori ekleniyorsa
                {
                    if (_context.Categories.Any(c => c.Name == newCategory.Name && c.ParentCategoryId == newCategory.ParentCategoryId))
                    {
                        ModelState.AddModelError("Name", "Bu isimde bir alt kategori bu üst kategori altında zaten mevcut.");
                    }
                }
                else // Ana kategori ekleniyorsa (ParentId null)
                {
                    if (_context.Categories.Any(c => c.Name == newCategory.Name && c.ParentCategoryId == null))
                    {
                        ModelState.AddModelError("Name", "Bu isimde bir ana kategori zaten mevcut.");
                    }
                }

                if (ModelState.IsValid) // Tekrar kontrol et
                {
                    _context.Add(newCategory);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Kategori başarıyla eklendi!";
                    return RedirectToAction(nameof(Categories)); // Kategori listeleme sayfasına yönlendir
                }
            }

            // Doğrulama hatası varsa, ViewModel'i View'a geri göndererek formu tekrar doldur
            viewModel.AllCategoriesForJs = await _context.Categories.Include(c => c.ParentCategory).ToListAsync();
            // Hata dönüşünde dropdown'ları doğru doldurmak için,
            // seçili kategori seviyesine göre üst kategori listesini yeniden oluştur
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
                query = query.Where(c => c.ParentCategoryId == null && (c.Type == "Cinsiyet" || c.Type == "Yaş Grubu"));
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
                query = query.Where(c => c.ParentCategory != null && (c.Type == "Ürün Grubu" || c.Type == "Yaş Grubu Kategori"));
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
                        if (_context.Categories.Any(c => c.Name == category.Name && c.ParentCategoryId == category.ParentCategoryId && c.Id != category.Id))
                        {
                            ModelState.AddModelError("Name", "Bu isimde bir alt kategori bu ana kategori altında zaten mevcut.");
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
                        if (_context.Categories.Any(c => c.Name == category.Name && c.ParentCategoryId == null && c.Id != category.Id))
                        {
                            ModelState.AddModelError("Name", "Bu isimde bir ana kategori zaten mevcut.");
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
        [HttpGet]
        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id == null) { return NotFound(); }
            var category = await _context.Categories.Include(c => c.ParentCategory).FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) { return NotFound(); }

            if (await _context.Products.AnyAsync(p => p.GenderCategoryId == id || p.MainCategoryId == id || p.SubCategoryId == id) ||
                await _context.Categories.AnyAsync(c => c.ParentCategoryId == id))
            {
                TempData["ErrorMessage"] = "Bu kategoriye bağlı ürünler veya alt kategoriler bulunmaktadır. Silinemez!";
                return RedirectToAction(nameof(Categories));
            }
            ViewData["Title"] = "Kategori Sil";
            return View(category);
        }

        // POST: /Admin/DeleteCategory/{id}
        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategoryConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) { return NotFound(); }
            if (await _context.Products.AnyAsync(p => p.GenderCategoryId == id || p.MainCategoryId == id || p.SubCategoryId == id) ||
                await _context.Categories.AnyAsync(c => c.ParentCategoryId == id))
            {
                TempData["ErrorMessage"] = "Bu kategoriye bağlı ürünler veya alt kategoriler bulunmaktadır. Silinemedi!";
                return RedirectToAction(nameof(Categories));
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kategori başarıyla silindi!";
            return RedirectToAction(nameof(Categories));
        }

        // Helper Method: CategoryExists
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
     
=======

>>>>>>> origin/master
        // AJAX Action: Ana kategoriye göre alt kategorileri getirir
        [HttpGet]
        public async Task<JsonResult> GetSubcategoriesByMainCategory(int mainCategoryId)
        {
            var subcategories = await _context.Categories
                                              .Where(c => c.ParentCategoryId == mainCategoryId)
<<<<<<< HEAD
                                              .Select(c => new { id = c.Id, name = c.Name, gender = c.Gender })
=======
                                              .Select(c => new { id = c.Id, name = c.Name, gender = c.Gender }) // << Gender bilgisini de gönderelim
>>>>>>> origin/master
                                              .ToListAsync();
            return Json(subcategories);
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