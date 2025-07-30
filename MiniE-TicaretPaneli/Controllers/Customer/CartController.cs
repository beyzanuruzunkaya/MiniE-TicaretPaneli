// Controllers/Customer/CartController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Authorize attribute'u kaldırıldığı için bu using'e artık gerek yok ama bırakabiliriz
using MiniE_TicaretPaneli.Data;
using MiniE_TicaretPaneli.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory; // ClaimTypes için

namespace MiniE_TicaretPaneli.Controllers.Customer
{
    // [Authorize(Roles = "Customer,Admin")] // BU SATIRI KALDIRILDI! Sepet herkese açık olacak
    [Route("Customer/[controller]/[action]")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;
        


        public CartController(ApplicationDbContext context , IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            ViewData["Title"] = "Sepetim";

            //MemoryCache üzerinden "cartList" adında bir veri aranıyor.
            //Eğer yoksa: 50 saniye içinde geçerliliği olan yeni bir List<ShoppingCart> oluşturuluyor.
            var cartCached = await _memoryCache.GetOrCreateAsync<List<ShoppingCart>>("cartList", cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(50);
                return Task.FromResult(new List<ShoppingCart>());
            });

            //Cache yoksa boş bir sepet listesi oluşturuluyor.
            if (cartCached == null)
            {
                cartCached = new List<ShoppingCart>();
            }
            // Eğer Product bilgisi (örneğin ad, fiyat vs.) dolu değilse,
            // ProductId üzerinden veritabanından Product çekiliyor.
            foreach (var item in cartCached)
            {
                if (item.Product == null)
                {
                    item.Product = await _context.Products.FindAsync(item.ProductId);
                }
            }
            //sepet listesi View'a gönderiliyor
            return View(cartCached);
        }

        // POST: /Customer/Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AddToCart(int productId, string selectedSize, int quantity = 1)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            int userId = 0;
            bool isAuthenticated = userIdClaim != null && int.TryParse(userIdClaim.Value, out userId);

            if (!isAuthenticated)
            {
                TempData.Clear();
                TempData["ErrorMessage"] = "Ürün eklemek için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(selectedSize))
            {
                TempData.Clear();
                TempData["ErrorMessage"] = "Lütfen bir beden seçiniz.";
                return RedirectToAction("ProductDetail", "Product", new { id = productId });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData.Clear();
                TempData["ErrorMessage"] = "Ürün bulunamadı.";
                return RedirectToAction("Products", "Product");
            }
            //Eğer "cartList" cache’te varsa alır, yoksa 30 dakika geçerli olacak şekilde boş sepet listesi oluşturur.
            var cartCached = await _memoryCache.GetOrCreateAsync<List<ShoppingCart>>("cartList", cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return Task.FromResult(new List<ShoppingCart>());
            });

            //  Eğer selectedSize bu listede yoksa, hata mesajı gösterilir ve ürün detay sayfasına dönülür.

            var validSizes = product.AvailableSizes?.Split(',');
            if (validSizes == null || !validSizes.Contains(selectedSize))
            {
                TempData.Clear();
                TempData["ErrorMessage"] = "Geçersiz beden seçimi.";
                return RedirectToAction("ProductDetail", "Product", new { id = productId });
            }

        // Aynı ürün ve beden zaten sepette varsa: Quantity artırılır.
        // Yoksa: Yeni bir ShoppingCart nesnesi sepete eklenir.
            var cartItem = cartCached.FirstOrDefault(ci => ci.ProductId == productId && ci.SelectedSize == selectedSize);
            if (cartItem != null)
                cartItem.Quantity += quantity;
            else
                cartCached.Add(new ShoppingCart
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    SelectedSize = selectedSize
                });

            // Cache'i güncelle
            _memoryCache.Set("cartList", cartCached);

            // Mesajları temizle ve yeni mesajı set et
            TempData.Clear();
            TempData["SuccessMessage"] = "Ürün sepete eklendi!";
            return RedirectToAction("Cart");
        }



        // POST: /Customer/Cart/UpdateCartItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int newQuantity)
        {
            //Giriş yapan kullanıcının userId bilgisi alınıyor.
            // Eğer kullanıcı tanımlı değilse ⇒ Login sayfasına yönlendiriliyor.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }
            //cartItemId ile veritabanından sepetteki ürün satırı çekilir.
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Sepet öğesi bulunamadı.";
                return RedirectToAction("Cart");
            }

            //Sepet öğesi başka bir kullanıcıya aitse ⇒ işlem engellenir.
            if (cartItem.UserId != userId)
            {
                TempData["ErrorMessage"] = "Yetkisiz işlem.";
                return RedirectToAction("Cart");
            }
            //Kullanıcı 0 ya da negatif miktar girerse ⇒ ürün sepetten silinir.
            if (newQuantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
                TempData["SuccessMessage"] = "Ürün sepetten kaldırıldı.";
            }
            else
            {
                //Stok yeterli değilse ⇒ hata mesajı gösterilir.
                //stok uygunsa ⇒ ürünün Quantity (miktarı) güncellenir.
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product != null && product.Stock < newQuantity)
                {
                    TempData["ErrorMessage"] = $"{product.Name} için yeterli stok bulunmamaktadır. Mevcut stok: {product.Stock}";
                    return RedirectToAction("Cart");
                }
                cartItem.Quantity = newQuantity;
                TempData["SuccessMessage"] = "Sepet güncellendi.";
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Cart");
        }

        // POST: /Customer/Cart/UpdateCartItemSize
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartItemSize(int cartItemId, string newSize)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }
            //Sepetteki ürün bilgisi cartItemId ile veritabanından çekilir.
            var cartItem = await _context.CartItems.Include(ci => ci.Product).FirstOrDefaultAsync(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Sepet öğesi bulunamadı.";
                return RedirectToAction("Cart");
            }
            if (cartItem.UserId != userId)
            {
                TempData["ErrorMessage"] = "Yetkisiz işlem.";
                return RedirectToAction("Cart");
            }
            if (string.IsNullOrEmpty(newSize) || !(cartItem.Product.AvailableSizes ?? "").Split(',', System.StringSplitOptions.RemoveEmptyEntries).Contains(newSize))
            {
                TempData["ErrorMessage"] = "Geçersiz beden seçimi.";
                return RedirectToAction("Cart");
            }
            cartItem.SelectedSize = newSize;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Beden güncellendi.";
            return RedirectToAction("Cart");
        }

        // POST: /Customer/Cart/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                // Güvenlik: Kullanıcının kendi sepet öğesini sildiğinden emin ol
                if (cartItem.UserId != userId)
                {
                    TempData["ErrorMessage"] = "Yetkisiz işlem.";
                    return RedirectToAction("Cart");
                }

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ürün sepetten başarıyla kaldırıldı.";
            }
            else
            {
                TempData["ErrorMessage"] = "Sepet öğesi bulunamadı.";
            }
            return RedirectToAction("Cart");
        }
    }
}