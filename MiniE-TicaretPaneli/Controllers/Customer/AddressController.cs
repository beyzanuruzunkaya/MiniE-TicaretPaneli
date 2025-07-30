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
    public class AddressController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AddressController(ApplicationDbContext context)
        {
            _context = context;
        }
        // AddressList
        [HttpGet]
        public async Task<IActionResult> AddressList()
        {
            ViewData["Title"] = "Adreslerim";
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Adreslerinizi görüntülemek için lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }
            var addresses = await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
            return View(addresses);
        }
        // AddAddress (GET)
        [HttpGet]
        public IActionResult AddAddress()
        {
            ViewData["Title"] = "Yeni Adres Ekle";
            return View();
        }
        // AddAddress (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(Address address)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Adres eklemek için lütfen giriş yapın.";
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
                address.UserId = userId;
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Adres başarıyla eklendi.";
                return RedirectToAction("AddressList");
            }
            return View(address);
        }
        // EditAddress (GET)
        [HttpGet]
        public async Task<IActionResult> EditAddress(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                TempData["ErrorMessage"] = "Adres bulunamadı.";
                return RedirectToAction("AddressList");
            }
            return View(address);
        }
        // EditAddress (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(int id, Address updated)
        {
            if (id != updated.Id)
            {
                return NotFound();
            }
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                updated.UserId = userId;
            }
            if (ModelState.IsValid)
            {
                _context.Update(updated);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Adres güncellendi.";
                return RedirectToAction("AddressList");
            }
            return View(updated);
        }
        // DeleteAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Adres silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Adres bulunamadı.";
            }
            return RedirectToAction("AddressList");
        }
    }
}
