// Controllers/Customer/CartController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniE_TicaretPaneli.Data;
using MiniE_TicaretPaneli.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace MiniE_TicaretPaneli.Controllers.Customer
{
    [Route("Customer/[controller]/[action]")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public CartController(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        // Database'den sepet verilerini al
        private async Task<List<ShoppingCart>> GetCartFromDatabase(int userId)
        {
            return await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            ViewData["Title"] = "Sepetim";

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            // Database'den sepet verilerini al
            var cartItems = await GetCartFromDatabase(userId);

            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, string selectedSize, int quantity = 1)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Ürün eklemek için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(selectedSize))
            {
                TempData["ErrorMessage"] = "Lütfen bir beden seçiniz.";
                return RedirectToAction("ProductDetail", "Product", new { id = productId });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Ürün bulunamadı.";
                return RedirectToAction("Products", "Product");
            }

            // Beden kontrolü
            var validSizes = product.AvailableSizes?.Split(',');
            if (validSizes == null || !validSizes.Contains(selectedSize))
            {
                TempData["ErrorMessage"] = "Geçersiz beden seçimi.";
                return RedirectToAction("ProductDetail", "Product", new { id = productId });
            }

            // Stok kontrolü
            if (product.Stock < quantity)
            {
                TempData["ErrorMessage"] = $"{product.Name} için yeterli stok bulunmamaktadır. Mevcut stok: {product.Stock}";
                return RedirectToAction("ProductDetail", "Product", new { id = productId });
            }

            // Database'den mevcut sepeti al
            var cartItems = await GetCartFromDatabase(userId);

            // Aynı ürün ve beden zaten sepette var mı kontrol et
            var existingItem = cartItems.FirstOrDefault(ci => ci.ProductId == productId && ci.SelectedSize == selectedSize);

            if (existingItem != null)
            {
                // Mevcut ürünün miktarını artır
                var newQuantity = existingItem.Quantity + quantity;
                if (product.Stock < newQuantity)
                {
                    TempData["ErrorMessage"] = $"{product.Name} için yeterli stok bulunmamaktadır. Mevcut stok: {product.Stock}";
                    return RedirectToAction("ProductDetail", "Product", new { id = productId });
                }
                existingItem.Quantity = newQuantity;
                existingItem.UpdatedAt = DateTime.Now;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                // Yeni ürün ekle
                var cartItem = new ShoppingCart
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    SelectedSize = selectedSize,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            // Cache'den sepeti temizle (güncelleme için)
            var cartCacheKey = $"UserCart_{userId}";
            _memoryCache.Remove(cartCacheKey);

            TempData["SuccessMessage"] = $"'{product.Name}' sepete eklendi!";
            
            // Sepete yönlendir
            return RedirectToAction("Cart", "Cart");
        }

        // POST: /Customer/Cart/UpdateCartItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int newQuantity)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            // Database'den sepet öğesini al
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Sepet öğesi bulunamadı.";
                return RedirectToAction("Cart");
            }

            if (newQuantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
                TempData["SuccessMessage"] = "Ürün sepetten kaldırıldı.";
            }
            else
            {
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product != null && product.Stock < newQuantity)
                {
                    TempData["ErrorMessage"] = $"{product.Name} için yeterli stok bulunmamaktadır. Mevcut stok: {product.Stock}";
                    return RedirectToAction("Cart");
                }
                cartItem.Quantity = newQuantity;
                cartItem.UpdatedAt = DateTime.Now;
                _context.CartItems.Update(cartItem);
                TempData["SuccessMessage"] = "Sepet güncellendi.";
            }

            await _context.SaveChangesAsync();

            // Cache'den de sepeti temizle (güncelleme için)
            var cartCacheKey = $"UserCart_{userId}";
            _memoryCache.Remove(cartCacheKey);

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

            // Database'den sepet öğesini al
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Sepet öğesi bulunamadı.";
                return RedirectToAction("Cart");
            }

            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Ürün bulunamadı.";
                return RedirectToAction("Cart");
            }

            var validSizes = product.AvailableSizes?.Split(',');
            if (string.IsNullOrEmpty(newSize) || validSizes == null || !validSizes.Contains(newSize))
            {
                TempData["ErrorMessage"] = "Geçersiz beden seçimi.";
                return RedirectToAction("Cart");
            }

            cartItem.SelectedSize = newSize;
            cartItem.UpdatedAt = DateTime.Now;
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();

            // Cache'den de sepeti temizle (güncelleme için)
            var cartCacheKey = $"UserCart_{userId}";
            _memoryCache.Remove(cartCacheKey);

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

            // Database'den sepet öğesini al
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Sepet öğesi bulunamadı.";
                return RedirectToAction("Cart");
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            // Cache'den de sepeti temizle (güncelleme için)
            var cartCacheKey = $"UserCart_{userId}";
            _memoryCache.Remove(cartCacheKey);

            TempData["SuccessMessage"] = "Ürün sepetten kaldırıldı.";
            return RedirectToAction("Cart");
        }

        // POST: /Customer/Cart/ClearCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            // Database'den kullanıcının tüm sepet öğelerini sil
            var cartItems = await _context.CartItems.Where(ci => ci.UserId == userId).ToListAsync();
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // Cache'den de sepeti temizle
            var cartCacheKey = $"UserCart_{userId}";
            _memoryCache.Remove(cartCacheKey);

            TempData["SuccessMessage"] = "Sepet temizlendi.";
            return RedirectToAction("Cart");
        }
    }
}
