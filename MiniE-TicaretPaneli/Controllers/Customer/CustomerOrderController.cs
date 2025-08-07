using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniE_TicaretPaneli.Data;
using MiniE_TicaretPaneli.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MiniE_TicaretPaneli.Controllers.Customer
{
    [Authorize(Roles = "Customer,Admin")]
    [Route("Customer/[controller]/[action]")]
    public class CustomerOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public CustomerOrderController(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        // Session'dan sepet verilerini al
        private List<ShoppingCart> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString("UserCart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<ShoppingCart>();
            }
            return JsonConvert.DeserializeObject<List<ShoppingCart>>(cartJson) ?? new List<ShoppingCart>();
        }

        // Cache'den sepet verilerini al (backup olarak)
        private List<ShoppingCart> GetCartFromCache(int userId)
        {
            var cacheKey = $"cart_{userId}";
            var cachedCart = _memoryCache.Get<List<ShoppingCart>>(cacheKey);
            return cachedCart ?? new List<ShoppingCart>();
        }

        // Orders
        public async Task<IActionResult> Orders()
        {
            ViewData["Title"] = "Siparişlerim";
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Siparişlerinizi görüntülemek için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }
            var orders = await _context.Orders
                                       .Include(o => o.OrderItems)
                                           .ThenInclude(oi => oi.Product)
                                       .Where(o => o.UserId == userId)
                                       .OrderByDescending(o => o.OrderDate)
                                       .ToListAsync();
            return View("~/Views/Customer/Order/Orders.cshtml", orders);
        }

        // CancelOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Sipariş bulunamadı.";
                return RedirectToAction("Orders");
            }
            if (order.Status != "Processing" && order.Status != "Order Received")
            {
                TempData["ErrorMessage"] = "Bu sipariş iptal edilemez.";
                return RedirectToAction("Orders");
            }
            order.Status = "Cancelled";
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Sipariş başarıyla iptal edildi.";
            return RedirectToAction("Orders");
        }

        // Checkout
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Checkout()
        {
            ViewData["Title"] = "Ödeme Sayfası";
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Ödeme yapmak için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            // Önce session'dan sepet verilerini al
            var cartItems = GetCartFromSession();

            // Eğer session'da veri yoksa, cache'den al
            if (!cartItems.Any())
            {
                cartItems = GetCartFromCache(userId);
            }

            // Kullanıcının sepetini filtrele
            var userCartItems = cartItems.Where(ci => ci.UserId == userId).ToList();

            if (!userCartItems.Any())
            {
                TempData["ErrorMessage"] = "Sepetinizde ürün bulunmamaktadır.";
                return RedirectToAction("Cart", "Cart");
            }

            // Product bilgilerini yükle
            foreach (var item in userCartItems)
            {
                if (item.Product == null)
                {
                    item.Product = await _context.Products.FindAsync(item.ProductId);
                }
            }

            // Kullanıcının adreslerini ve kartlarını al
            var addresses = await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
            var creditCards = await _context.CreditCards.Where(c => c.UserId == userId).ToListAsync();

            ViewBag.Addresses = addresses;
            ViewBag.CreditCards = creditCards;
            ViewBag.CartItems = userCartItems;

            return View("~/Views/Customer/Order/Checkout.cshtml");
        }
        // ProcessPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(
            int? selectedCardId,
            string? newCardHolderName,
            string? newCardNumber,
            string? newExpiryMonth,
            string? newExpiryYear,
            string? newCvv,
            string addressOption,
            string? newAddressTitle,
            string? newAddressPhone,
            string? newAddressCity,
            string? newAddressDistrict,
            string? newAddressLine,
            string? newAddressPostalCode,
            bool saveNewAddress = false,
            bool saveCard = false
        )
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Ödeme yapmak için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            // Önce session'dan sepet verilerini al
            var cartItems = GetCartFromSession();

            // Eğer session'da veri yoksa, cache'den al
            if (!cartItems.Any())
            {
                cartItems = GetCartFromCache(userId);
            }

            // Kullanıcının sepetini filtrele
            var userCartItems = cartItems.Where(ci => ci.UserId == userId).ToList();

            // Product bilgilerini yükle
            foreach (var item in userCartItems)
            {
                if (item.Product == null)
                {
                    item.Product = await _context.Products.FindAsync(item.ProductId);
                }
            }

            if (!userCartItems.Any())
            {
                TempData["ErrorMessage"] = "Sepetiniz boş.";
                return RedirectToAction("Cart", "Cart");
            }

            // --- YENİ ADRES KAYDETME ---
            int? addressIdToUse = null;
            if (addressOption == "new")
            {
                if (saveNewAddress && !string.IsNullOrWhiteSpace(newAddressTitle) && !string.IsNullOrWhiteSpace(newAddressCity) && !string.IsNullOrWhiteSpace(newAddressDistrict) && !string.IsNullOrWhiteSpace(newAddressLine))
                {
                    var newAddress = new Address
                    {
                        UserId = userId,
                        Title = newAddressTitle,
                        City = newAddressCity,
                        District = newAddressDistrict,
                        AddressLine = newAddressLine,
                        PostalCode = newAddressPostalCode,
                        Phone = newAddressPhone
                    };
                    _context.Addresses.Add(newAddress);
                    await _context.SaveChangesAsync();
                    addressIdToUse = newAddress.Id;
                }
            }
            else if (int.TryParse(addressOption, out int selectedAddressId))
            {
                addressIdToUse = selectedAddressId;
            }

            // --- YENİ KART KAYDETME ---
            int? cardIdToUse = selectedCardId;
            if (string.IsNullOrEmpty(newCardNumber) == false && saveCard)
            {
                var newCard = new CreditCard
                {
                    UserId = userId,
                    CardHolderName = newCardHolderName ?? "",
                    CardNumberLastFour = newCardNumber ?? "",
                    ExpiryMonth = newExpiryMonth ?? "",
                    ExpiryYear = newExpiryYear ?? "",
                    CvvHash = newCvv ?? ""
                };
                _context.CreditCards.Add(newCard);
                await _context.SaveChangesAsync();
                cardIdToUse = newCard.Id;
            }

            // Simülasyon: Kart bilgilerini kontrol etmeden veya kaydetmeden başarılı sayalım
            bool paymentSuccess = true;
            string paymentMessage = "Ödeme başarılı!";
            if (paymentSuccess)
            {
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = userCartItems.Sum(ci => (float)ci.Product.Price * ci.Quantity),
                    Status = "Processing"
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in userCartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price,
                        Price = item.Product.Price,
                        SelectedSize = item.SelectedSize
                    };
                    _context.OrderItems.Add(orderItem);

                    // Stok güncelle
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock -= item.Quantity;
                        _context.Products.Update(product);
                    }
                }

                await _context.SaveChangesAsync();

                // Kullanıcının sepetini temizle
                var updatedCartItems = cartItems.Where(ci => ci.UserId != userId).ToList();
                
                // Session'ı güncelle
                var cartJson = JsonConvert.SerializeObject(updatedCartItems);
                HttpContext.Session.SetString("UserCart", cartJson);

                // Cache'i de güncelle
                var cacheKey = $"cart_{userId}";
                _memoryCache.Set(cacheKey, updatedCartItems, TimeSpan.FromHours(2));

                TempData["SuccessMessage"] = paymentMessage + " Siparişiniz başarıyla oluşturuldu.";
                return RedirectToAction("Cart", "Cart");
            }
            else
            {
                TempData["ErrorMessage"] = "Ödeme işlemi başarısız oldu. Lütfen tekrar deneyin.";
                return RedirectToAction("Checkout");
            }
        }
    }
}
