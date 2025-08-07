// Controllers/Customer/CheckoutController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniE_TicaretPaneli.Data;
using MiniE_TicaretPaneli.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace MiniE_TicaretPaneli.Controllers.Customer
{
    [Route("Customer/[controller]/[action]")]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public CheckoutController(ApplicationDbContext context, IMemoryCache memoryCache)
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
        public async Task<IActionResult> Checkout()
        {
            ViewData["Title"] = "Ödeme";

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            // Cache'den sepet verilerini al
            var cartItems = await GetCartFromDatabase(userId);

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Sepetinizde ürün bulunmamaktadır.";
                return RedirectToAction("Cart", "Cart");
            }

            // Product bilgilerini yükle
            foreach (var item in cartItems)
            {
                if (item.Product == null)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        item.Product = product;
                    }
                }
            }

            // Kullanıcı bilgilerini al
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgileri bulunamadı.";
                return RedirectToAction("Login", "Account");
            }

            // Kullanıcının kayıtlı adreslerini al
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            // Kullanıcının kayıtlı kredi kartlarını al
            var creditCards = await _context.CreditCards
                .Where(c => c.UserId == userId)
                .ToListAsync();

                         var checkoutViewModel = new CheckoutViewModel
             {
                 CartItems = cartItems,
                 User = user,
                 Addresses = addresses,
                 CreditCards = creditCards,
                 TotalAmount = (float)cartItems.Sum(item => item.Quantity * (item.Product?.Price ?? 0))
             };

            return View(checkoutViewModel);
        }

                 [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> ProcessOrder(CheckoutViewModel model)
         {
             var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
             if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
             {
                 TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                 return RedirectToAction("Login", "Account");
             }

             // Cache'den sepet verilerini al
             var cartItems = await GetCartFromDatabase(userId);

             if (!cartItems.Any())
             {
                 TempData["ErrorMessage"] = "Sepetinizde ürün bulunmamaktadır.";
                 return RedirectToAction("Cart", "Cart");
             }

             // Ödeme onay kodu simülasyonu
             var verificationCode = GenerateVerificationCode();
             var cacheKey = $"PaymentVerification_{userId}";
             _memoryCache.Set(cacheKey, verificationCode, TimeSpan.FromMinutes(10));

             // Onay kodu sayfasına yönlendir
             TempData["VerificationCode"] = verificationCode; // Demo için gösteriyoruz
             return RedirectToAction("PaymentVerification");
         }

         [HttpGet]
         public async Task<IActionResult> PaymentVerification()
         {
             ViewData["Title"] = "Ödeme Onayı";
             
             var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
             if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
             {
                 TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                 return RedirectToAction("Login", "Account");
             }

             var cartItems = await GetCartFromDatabase(userId);
             if (!cartItems.Any())
             {
                 TempData["ErrorMessage"] = "Sepetinizde ürün bulunmamaktadır.";
                 return RedirectToAction("Cart", "Cart");
             }

             // Cache'den onay kodunu al
             var cacheKey = $"PaymentVerification_{userId}";
             var verificationCode = _memoryCache.Get<string>(cacheKey);
             
             if (string.IsNullOrEmpty(verificationCode))
             {
                 TempData["ErrorMessage"] = "Onay kodu bulunamadı. Lütfen ödeme sayfasına geri dönün.";
                 return RedirectToAction("Checkout");
             }
             
             var viewModel = new PaymentVerificationViewModel
             {
                 CartItems = cartItems,
                 TotalAmount = (float)cartItems.Sum(item => item.Quantity * (item.Product?.Price ?? 0)),
                 VerificationCode = verificationCode
             };

             return View(viewModel);
         }

         [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> VerifyPayment(PaymentVerificationViewModel model)
         {
             var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
             if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
             {
                 TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                 return RedirectToAction("Login", "Account");
             }

             // Onay kodunu kontrol et
             var cacheKey = $"PaymentVerification_{userId}";
             var storedCode = _memoryCache.Get<string>(cacheKey);
             
             if (string.IsNullOrEmpty(storedCode) || storedCode != model.EnteredCode)
             {
                 TempData["ErrorMessage"] = "Geçersiz onay kodu. Lütfen tekrar deneyin.";
                 return RedirectToAction("PaymentVerification");
             }

             // Cache'den sepet verilerini al
             var cartItems = await GetCartFromDatabase(userId);

             if (!cartItems.Any())
             {
                 TempData["ErrorMessage"] = "Sepetinizde ürün bulunmamaktadır.";
                 return RedirectToAction("Cart", "Cart");
             }

             // Stok kontrolü ve ürün bilgilerini al
             var productsToUpdate = new List<Product>();
             foreach (var item in cartItems)
             {
                 var product = await _context.Products.FindAsync(item.ProductId);
                 if (product == null || product.Stock < item.Quantity)
                 {
                     TempData["ErrorMessage"] = $"{product?.Name ?? "Ürün"} için yeterli stok bulunmamaktadır.";
                     return RedirectToAction("Cart", "Cart");
                 }
                 productsToUpdate.Add(product);
             }

             try
             {
                 // Sipariş oluştur
                 var order = new Order
                 {
                     UserId = userId,
                     OrderDate = DateTime.Now,
                     Status = "Beklemede",
                     TotalAmount = (float)cartItems.Sum(item => item.Quantity * (item.Product?.Price ?? 0))
                 };

                 _context.Orders.Add(order);
                 await _context.SaveChangesAsync();

                 // Sipariş detaylarını oluştur
                 for (int i = 0; i < cartItems.Count; i++)
                 {
                     var item = cartItems[i];
                     var product = productsToUpdate[i];
                     
                     var orderItem = new OrderItem
                     {
                         OrderId = order.Id,
                         ProductId = item.ProductId,
                         Quantity = item.Quantity,
                         UnitPrice = product.Price,
                         Price = product.Price * item.Quantity,
                         SelectedSize = item.SelectedSize
                     };
                     _context.OrderItems.Add(orderItem);

                     // Stok güncelle
                     product.Stock -= item.Quantity;
                 }

                 await _context.SaveChangesAsync();

                 // Sepeti temizle
                 var cartItemsToRemove = await _context.CartItems.Where(ci => ci.UserId == userId).ToListAsync();
                 _context.CartItems.RemoveRange(cartItemsToRemove);
                 await _context.SaveChangesAsync();
                 
                 // Cache'den de sepeti temizle
                 var cartCacheKey = $"UserCart_{userId}";
                 _memoryCache.Remove(cartCacheKey);
                 _memoryCache.Remove(cacheKey); // Onay kodunu da temizle

                 TempData["SuccessMessage"] = $"Siparişiniz başarıyla oluşturuldu! Sipariş numarası: {order.Id}";
                 return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
             }
             catch (Exception ex)
             {
                 // Hata detaylarını logla
                 Console.WriteLine($"Sipariş hatası: {ex.Message}");
                 Console.WriteLine($"Stack trace: {ex.StackTrace}");
                 
                 TempData["ErrorMessage"] = $"Sipariş işlenirken bir hata oluştu: {ex.Message}";
                 return RedirectToAction("PaymentVerification");
             }
         }

         private string GenerateVerificationCode()
         {
             Random random = new Random();
             return random.Next(100000, 999999).ToString();
         }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            ViewData["Title"] = "Sipariş Onayı";

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Sipariş bulunamadı.";
                return RedirectToAction("Orders", "CustomerOrder");
            }

            return View(order);
        }
    }

    // ViewModel sınıfları
         public class CheckoutViewModel
     {
         public List<ShoppingCart> CartItems { get; set; }
         public User User { get; set; }
         public List<Address> Addresses { get; set; }
         public List<CreditCard> CreditCards { get; set; }
         public float TotalAmount { get; set; }

        // Adres seçimi
        public bool UseNewAddress { get; set; }
        public int? SelectedAddressId { get; set; }
        public AddressViewModel NewAddress { get; set; } = new AddressViewModel();

        // Kredi kartı seçimi
        public bool UseNewCreditCard { get; set; }
        public int? SelectedCreditCardId { get; set; }
        public CreditCardViewModel NewCreditCard { get; set; } = new CreditCardViewModel();
    }

    public class AddressViewModel
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; } = "Türkiye";
    }

         public class CreditCardViewModel
     {
         public string CardNumber { get; set; }
         public string CardHolderName { get; set; }
         public int ExpiryMonth { get; set; }
         public int ExpiryYear { get; set; }
         public string CVV { get; set; }
     }

     public class PaymentVerificationViewModel
     {
         public List<ShoppingCart> CartItems { get; set; }
         public float TotalAmount { get; set; }
         public string VerificationCode { get; set; }
         public string EnteredCode { get; set; }
     }
} 