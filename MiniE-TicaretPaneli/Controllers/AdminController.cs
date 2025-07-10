// Controllers/AdminController.cs
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


namespace MiniE_TicaretPaneli.Controllers
{
    [Authorize(Roles = "Admin")] // Bu Controller'daki tüm Action'lara sadece Admin rolündekiler erişebilir
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/AdminDashboard
        public IActionResult AdminDashboard()
        {
            ViewData["Title"] = "Admin Paneli Dashboard";
            return View();
        }

        // GET: /Admin/Products
        // Tüm ürünleri listele
        public async Task<IActionResult> Products()
        {
            ViewData["Title"] = "Ürün Yönetimi";
            // Ürünleri ilgili kategorileriyle birlikte çek
            var products = await _context.Products
                                       .Include(p => p.MainCategory)
                                       .Include(p => p.SubCategory)
                                       .ToListAsync();
            return View(products);
        }

        // GET: /Admin/AddProduct (Yeni ürün ekleme formu)
        public async Task<IActionResult> AddProduct()
        {
            ViewData["Title"] = "Yeni Ürün Ekle";

            var viewModel = new AdminProductViewModel
            {
                // Ana kategoriler: Sadece cinsiyet/yaş grubu olanları getir (Kadın, Erkek, Anne & Çocuk)
                // Bu liste JavaScript'e tam gönderilecek ve JavaScript Product.Gender'a göre filtreleyecek.
                MainCategories = await _context.Categories
                                               .Where(c => c.ParentCategoryId == null && (c.Type == "Cinsiyet" || c.Type == "Yaş Grubu"))
                                               .ToListAsync(),
                SubCategories = new List<Category>(), // Başlangıçta alt kategoriler boş
                AllSizes = GetDefaultSizes(),
                AllColors = GetDefaultColors()
            };
            return View(viewModel);
        }

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
                    viewModel.Product.ImageUrl = "/images/default-product.jpg"; // Resim yüklenmediyse varsayılan
                }

                _context.Add(viewModel.Product); // ViewModel'deki Product nesnesini kaydet
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ürün başarıyla eklendi!";
                return RedirectToAction(nameof(Products));
            }

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

            viewModel.AllSizes = GetDefaultSizes();
            viewModel.AllColors = GetDefaultColors();
            return View(viewModel);
        }

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

            ViewData["Title"] = "Ürün Düzenle";
            var viewModel = new AdminProductViewModel
            {
                Product = product,
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
                SelectedSizes = product.AvailableSizes?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                SelectedColors = product.AvailableColors?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>()
            };
            return View(viewModel);
        }

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
            viewModel.Product.AvailableSizes = string.Join(",", viewModel.SelectedSizes);
            viewModel.Product.AvailableColors = string.Join(",", viewModel.SelectedColors);

            if (ModelState.IsValid)
            {
                try
                {
                    // Resim yükleme işlemleri
                    if (viewModel.ProductImage != null && viewModel.ProductImage.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        if (!Directory.Exists(uploadsFolder)) { Directory.CreateDirectory(uploadsFolder); }
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ProductImage.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

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

                    _context.Update(viewModel.Product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Ürün başarıyla güncellendi!";
                }
                catch (DbUpdateConcurrencyException)
                {
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
            viewModel.AllSizes = GetDefaultSizes();
            viewModel.AllColors = GetDefaultColors();
            return View(viewModel);
        }

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

            ViewData["Title"] = "Ürün Sil";
            return View(product);
        }

        // POST: /Admin/DeleteProduct/5 (Ürünü silme işlemi)
        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // İlişkili resmi de sil (varsa ve varsayılan değilse)
                if (!string.IsNullOrEmpty(product.ImageUrl) && !product.ImageUrl.Contains("default-product.jpg"))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
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

        // GET: /Admin/Users (Müşteri Görüntüleme)
        public async Task<IActionResult> Users()
        {
            ViewData["Title"] = "Müşteri Görüntüleme";
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // GET: /Admin/AddUser (Yeni kullanıcı ekleme formu)
        public IActionResult AddUser()
        {
            ViewData["Title"] = "Yeni Kullanıcı Ekle";
            // Adminin rol seçebilmesi için roller listesini gönder
            ViewBag.Roles = Enum.GetValues(typeof(UserRole))
                                .Cast<UserRole>()
                                .Select(r => new { Value = r.ToString(), Text = r.ToString() })
                                .ToList();
            return View();
        }

        // POST: /Admin/AddUser (Yeni kullanıcı kaydetme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(
            [Bind("Username,PasswordHash,FirstName,LastName,Email,PhoneNumber,Role")] User user)
        {
            // Şifre Karmaşıklığı Kontrolü
            if (!string.IsNullOrEmpty(user.PasswordHash) && !IsPasswordComplex(user.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Şifre en az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.");
            }

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
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Kullanıcı başarıyla eklendi!";
                    return RedirectToAction(nameof(Users));
                }
            }

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
        public async Task<IActionResult> Categories()
        {
            ViewData["Title"] = "Kategori Yönetimi";
            var categories = await _context.Categories.Include(c => c.ParentCategory).ToListAsync();
            return View(categories);
        }

        // AJAX Action: Ana kategoriye göre alt kategorileri getirir
        [HttpGet]
        public async Task<JsonResult> GetSubcategoriesByMainCategory(int mainCategoryId)
        {
            var subcategories = await _context.Categories
                                              .Where(c => c.ParentCategoryId == mainCategoryId)
                                              .Select(c => new { id = c.Id, name = c.Name, gender = c.Gender }) // << Gender bilgisini de gönderelim
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