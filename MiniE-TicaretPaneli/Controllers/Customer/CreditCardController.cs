using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniE_TicaretPaneli.Data;
using MiniE_TicaretPaneli.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MiniE_TicaretPaneli.Controllers.Customer
{
    [Authorize(Roles = "Customer,Admin")]
    [Route("Customer/[controller]/[action]")]
    public class CreditCardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CreditCardController(ApplicationDbContext context)
        {
            _context = context;
        }
        // CreditCards
        public async Task<IActionResult> CreditCards()
        {
            ViewData["Title"] = "Kredi Kartlarım";
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Kartlarınızı görüntülemek için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }
            var cards = await _context.CreditCards.Where(c => c.UserId == userId).ToListAsync();
            return View(cards);
        }
        // AddCreditCard (GET)
        public IActionResult AddCreditCard()
        {
            ViewData["Title"] = "Yeni Kredi Kartı Ekle";
            return View();
        }
        // AddCreditCard (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCreditCard(CreditCard creditCard)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Kart eklemek için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                creditCard.UserId = userId;
                _context.CreditCards.Add(creditCard);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kredi kartı başarıyla eklendi.";
                return RedirectToAction("CreditCards");
            }
            return View(creditCard);
        }
        // EditCreditCard (GET)
        [HttpGet]
        public async Task<IActionResult> EditCreditCard(int id)
        {
            var card = await _context.CreditCards.FindAsync(id);
            if (card == null)
            {
                TempData["ErrorMessage"] = "Kart bulunamadı.";
                return RedirectToAction("CreditCards");
            }
            return View(card);
        }
        // EditCreditCard (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCreditCard(int id, CreditCard updatedCard)
        {
            if (id != updatedCard.Id)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            var existingCard = await _context.CreditCards.FindAsync(id);
            if (existingCard == null || existingCard.UserId != userId)
            {
                TempData["ErrorMessage"] = "Kart bulunamadı veya yetkiniz yok.";
                return RedirectToAction("CreditCards");
            }

            if (ModelState.IsValid)
            {
                // Sadece güncellenebilir alanları değiştir
                existingCard.CardHolderName = updatedCard.CardHolderName;
                existingCard.CardNumberLastFour = updatedCard.CardNumberLastFour;
                existingCard.ExpiryMonth = updatedCard.ExpiryMonth;
                existingCard.ExpiryYear = updatedCard.ExpiryYear;
                existingCard.CvvHash = updatedCard.CvvHash;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kredi kartı güncellendi.";
                return RedirectToAction("CreditCards");
            }
            return View(updatedCard);
        }
        // DeleteCreditCard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCreditCard(int id)
        {
            var card = await _context.CreditCards.FindAsync(id);
            if (card != null)
            {
                _context.CreditCards.Remove(card);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kredi kartı silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kart bulunamadı.";
            }
            return RedirectToAction("CreditCards");
        }
    }
}
