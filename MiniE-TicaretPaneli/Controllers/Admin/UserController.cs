using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniE_TicaretPaneli.Data;
using MiniE_TicaretPaneli.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace MiniE_TicaretPaneli.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")]
    //[Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            ViewData["Title"] = "Müşteri Görüntüleme";
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            ViewData["Title"] = "Yeni Kullanıcı Ekle";
            ViewBag.Roles = Enum.GetValues(typeof(UserRole))
                                .Cast<UserRole>()
                                .Select(r => new { Value = r.ToString(), Text = r.ToString() })
                                .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(
            [Bind("Username,PasswordHash,FirstName,LastName,Email,PhoneNumber,Role")] User user)
        {
            if (!string.IsNullOrEmpty(user.PasswordHash) && !IsPasswordComplex(user.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Şifre en az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.");
            }
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Username == user.Username)) { ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış."); }
                if (_context.Users.Any(u => u.Email == user.Email)) { ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor."); }
                if (!string.IsNullOrEmpty(user.PhoneNumber) && _context.Users.Any(u => u.PhoneNumber == user.PhoneNumber)) { ModelState.AddModelError("PhoneNumber", "Bu telefon numarası zaten kullanılıyor."); }
                if (ModelState.IsValid)
                {
                    user.Role = UserRole.Customer;
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Kullanıcı başarıyla eklendi!";
                    return RedirectToAction(nameof(Users));
                }
            }
            ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>().Select(r => new { Value = r.ToString(), Text = r.ToString() }).ToList();
            return View(user);
        }

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