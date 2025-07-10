// Controllers/CustomerController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // [Authorize] attribute'u için
using MiniE_TicaretPaneli.Data; // DbContext için
using MiniE_TicaretPaneli.Models; // Modeller için
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Include metodu için

namespace MiniE_TicaretPaneli.Controllers
{
    [Authorize(Roles = "Customer,Admin")] // Bu Controller'daki tüm Action'lara Müşteri veya Admin rolündekiler erişebilir
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Customer/CustomerDashboard
        // Müşteri Dashboard'u için basit bir yer tutucu
        public IActionResult CustomerDashboard()
        {
            ViewData["Title"] = "Müşteri Paneli Dashboard";
            return View();
        }

        // GET: /Customer/Products
        // Tüm ürünleri listele
        public async Task<IActionResult> Products()
        {
           ViewData["Title"] = "Ürünler";
// Ürünleri ilgili kategorileriyle birlikte çek
var products = await _context.Products
                           .Include(p => p.MainCategory)
                           .Include(p => p.SubCategory)
                           .ToListAsync();
return View(products);
        }

        // GET: /Customer/ProductDetail/{id}
        // Tek bir ürünün detaylarını göster
        public async Task<IActionResult> ProductDetail(int? id)
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

            ViewData["Title"] = product.Name;
            return View(product);
        }

        // POST: /Customer/AddToCart
        // Ürünü sepete ekleme simülasyonu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Ürün eklemek için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Ürün bulunamadı.";
                return RedirectToAction("Products");
            }

            // Sepette aynı ürün var mı kontrol et
            var cartItem = await _context.CartItems
                                         .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new ShoppingCart // Model adı ShoppingCart ise
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{product.Name} sepete eklendi!";
            return RedirectToAction("Products");
        }

        // GET: /Customer/Cart
        public async Task<IActionResult> Cart()
        {
            ViewData["Title"] = "Sepetim";
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Sepetinizi görüntülemek için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.CartItems
                                          .Include(ci => ci.Product)
                                          .Where(ci => ci.UserId == userId)
                                          .ToListAsync();

            return View(cartItems);
        }

        // POST: /Customer/UpdateCartItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int newQuantity)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Sepet öğesi bulunamadı.";
                return RedirectToAction("Cart");
            }

            if (newQuantity <= 0)
            {
                _context.CartItems.Remove(cartItem); // Adet 0 veya daha az ise ürünü sepetten çıkar
                TempData["SuccessMessage"] = "Ürün sepetten kaldırıldı.";
            }
            else
            {
                cartItem.Quantity = newQuantity;
                TempData["SuccessMessage"] = "Sepet güncellendi.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Cart");
        }

        // POST: /Customer/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
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

        // GET: /Customer/Orders
        public async Task<IActionResult> Orders()
        {
            ViewData["Title"] = "Siparişlerim";
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Siparişlerinizi görüntülemek için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var userIdClaim = User.FindFirst("UserId");
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

            return View(orders);
        }

        // GET: /Customer/CreditCards
        public async Task<IActionResult> CreditCards()
        {
            ViewData["Title"] = "Kredi Kartlarım";
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Kartlarınızı görüntülemek için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var cards = await _context.CreditCards.Where(c => c.UserId == userId).ToListAsync();
            return View(cards);
        }

        // GET: /Customer/AddCreditCard
        public IActionResult AddCreditCard()
        {
            ViewData["Title"] = "Yeni Kredi Kartı Ekle";
            return View();
        }

        // POST: /Customer/AddCreditCard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCreditCard(CreditCard creditCard)
        {
            // Bu sadece bir simülasyon. Gerçek kart bilgileri asla böyle kaydedilmemeli.
            // CVV'yi asla veritabanında tutmayın!
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Kart eklemek için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                creditCard.UserId = userId;
                // Kart numarasının sadece son 4 hanesini tutmak daha güvenli (simülasyonda tamamı olabilir)
                // creditCard.CardNumber = creditCard.CardNumber.Substring(creditCard.CardNumber.Length - 4);
                // creditCard.Cvv = null; // CVV'yi asla kaydetme!

                _context.CreditCards.Add(creditCard);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kredi kartı başarıyla eklendi.";
                return RedirectToAction("CreditCards");
            }
            return View(creditCard);
        }

        // GET: /Customer/Checkout (Ödeme ekranı)
        public async Task<IActionResult> Checkout()
        {
            ViewData["Title"] = "Ödeme Sayfası";
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Ödeme yapmak için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.CartItems
                                          .Include(ci => ci.Product)
                                          .Where(ci => ci.UserId == userId)
                                          .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Sepetiniz boş. Ödeme yapmadan önce ürün ekleyin.";
                return RedirectToAction("Cart");
            }

            ViewBag.CartItems = cartItems;
            ViewBag.TotalAmount = cartItems.Sum(ci => ci.Product.Price * ci.Quantity);
            ViewBag.CreditCards = await _context.CreditCards.Where(c => c.UserId == userId).ToListAsync();

            return View();
        }

        // POST: /Customer/ProcessPayment (Ödeme simülasyonu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int? selectedCardId, string? newCardHolderName, string? newCardNumber, string? newExpiryMonth, string? newExpiryYear, string? newCvv)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Ödeme yapmak için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.CartItems
                                          .Include(ci => ci.Product)
                                          .Where(ci => ci.UserId == userId)
                                          .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Sepetiniz boş.";
                return RedirectToAction("Cart");
            }

            // Simülasyon: Kart bilgilerini kontrol etmeden veya kaydetmeden başarılı sayalım
            bool paymentSuccess = true;
            string paymentMessage = "Ödeme başarılı!";

            // Gerçek bir senaryoda burada kart bilgileri doğrulama ve ödeme entegrasyonu olurdu.
            // Eğer yeni kart bilgileri girildiyse ve kaydedilecekse burada işlenir.
            if (selectedCardId == null && !string.IsNullOrEmpty(newCardNumber))
            {
                // Yeni kart bilgileri girildi, geçerliliğini kontrol etmelisiniz.
                // Burada sadece simülasyon olduğu için kontrol etmiyorum.
                // Kartı kaydetmek istenirse SaveCardCheckbox kontrol edilmeli
            }

            if (paymentSuccess)
            {
                // Yeni sipariş oluştur
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = cartItems.Sum(ci => (float)ci.Product.Price * ci.Quantity),
                    Status = "Processing"
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Siparişi kaydet ki ID'si oluşsun

                // Sipariş kalemlerini oluştur
                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price // Sipariş anındaki fiyatı kaydet
                    };
                    _context.OrderItems.Add(orderItem);
                    // Ürün stoğunu düşür
                    item.Product.Stock -= item.Quantity;
                }
                // Sepeti temizle
                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = paymentMessage + " Siparişiniz başarıyla oluşturuldu.";
                return RedirectToAction("CustomerDashboard");
            }
            else
            {
                TempData["ErrorMessage"] = "Ödeme başarısız oldu. Lütfen bilgilerinizi kontrol edin.";
                // Ödeme ekranına geri yönlendir ve mevcut verileri tut
                ViewBag.CartItems = cartItems;
                ViewBag.TotalAmount = cartItems.Sum(ci => ci.Product.Price * ci.Quantity);
                ViewBag.CreditCards = await _context.CreditCards.Where(c => c.UserId == userId).ToListAsync();
                return View("Checkout");
            }
        }
    }
}