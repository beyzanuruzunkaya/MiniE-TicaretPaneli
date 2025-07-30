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
using System.Collections.Generic; // Added for List<ShoppingCart>

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
            // Cache'den sepet verilerini al
            var cartCached = await _memoryCache.GetOrCreateAsync<List<ShoppingCart>>("cartList", cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return Task.FromResult(new List<ShoppingCart>());
            });
            
            if (cartCached == null)
            {
                cartCached = new List<ShoppingCart>();
            }
            
            // Kullanıcının sepetini filtrele
            var cartItems = cartCached.Where(ci => ci.UserId == userId).ToList();
            
            // Her item'ın Product'ını doldur
            foreach (var item in cartItems)
            {
                if (item.Product == null)
                {
                    item.Product = await _context.Products.FindAsync(item.ProductId);
                }
            }
            
            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Sepetiniz boş. Ödeme yapmadan önce ürün ekleyin.";
                return RedirectToAction("Cart", "Cart");
            }
            ViewBag.CartItems = cartItems;
            ViewBag.TotalAmount = cartItems.Sum(ci => ci.Product.Price * ci.Quantity);
            ViewBag.CreditCards = await _context.CreditCards.Where(c => c.UserId == userId).ToListAsync();
            ViewBag.Addresses = await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
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
            // Cache'den sepet verilerini al
            var cartCached = await _memoryCache.GetOrCreateAsync<List<ShoppingCart>>("cartList", cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return Task.FromResult(new List<ShoppingCart>());
            });
            
            if (cartCached == null)
            {
                cartCached = new List<ShoppingCart>();
            }
            
            // Kullanıcının sepetini filtrele
            var cartItems = cartCached.Where(ci => ci.UserId == userId).ToList();
            
            // Her item'ın Product'ını doldur
            foreach (var item in cartItems)
            {
                if (item.Product == null)
                {
                    item.Product = await _context.Products.FindAsync(item.ProductId);
                }
            }
            
            if (!cartItems.Any())
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
                    TotalAmount = cartItems.Sum(ci => (float)ci.Product.Price * ci.Quantity),
                    Status = "Processing"
                    // İstersen AddressId ve CreditCardId ekleyebilirsin
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price
                    };
                    _context.OrderItems.Add(orderItem);
                    item.Product.Stock -= item.Quantity;
                }
                // Cache'den kullanıcının sepetini temizle
                var updatedCartCached = cartCached.Where(ci => ci.UserId != userId).ToList();
                _memoryCache.Set("cartList", updatedCartCached);
                
                TempData["SuccessMessage"] = paymentMessage + " Siparişiniz başarıyla oluşturuldu.";
                return RedirectToAction("Cart", "Cart");
            }
            else
            {
                TempData["ErrorMessage"] = "Ödeme başarısız oldu. Lütfen bilgilerinizi kontrol edin.";
                ViewBag.CartItems = cartItems;
                ViewBag.TotalAmount = cartItems.Sum(ci => ci.Product.Price * ci.Quantity);
                ViewBag.CreditCards = await _context.CreditCards.Where(c => c.UserId == userId).ToListAsync();
                return View("~/Views/Customer/Order/Checkout.cshtml");
            }
        }
    }
}
