// Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // [Authorize] attribute'u için
using MiniE_TicaretPaneli.Data; // DbContext için
using MiniE_TicaretPaneli.Models; // Modeller (User, UserRole) için
using System.Linq; // LINQ metotları için
using System.Security.Claims; // Claims ve ClaimsPrincipal için
using Microsoft.AspNetCore.Authentication; // SignInAsync ve SignOutAsync için
using Microsoft.AspNetCore.Authentication.Cookies; // CookieAuthenticationDefaults için
using System.Threading.Tasks; // Asenkron metotlar için
using System; // DateTimeOffset, TimeSpan için
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MiniE_TicaretPaneli.Models.ViewModels; // List için (bu using'i kaldırmadım, ResetPasswordViewModel için gerekli olabilir)
using BCrypt.Net; // BCrypt için eklendi // <-- BU SATIRI EKLEYİN!

namespace MiniE_TicaretPaneli.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Kullanıcıyı veritabanında bul
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username); // await eklendi

            // GÜVENLİK DÜZELTMESİ: Şifreyi HASH ile karşılaştır!
            // user null değilse VE girilen şifre (password) ile veritabanındaki hash (user.PasswordHash) eşleşiyorsa
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) 
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role.ToString()), // Enum'ı string'e çevir
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) 
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false, // Oturum çerezi tarayıcı kapanınca silinir
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Geçersiz kullanıcı adı veya şifre.";
            return View();
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            [Bind("Username,PasswordHash,FirstName,LastName,Email,PhoneNumber")] User user)
        {
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Şifre alanı boş bırakılamaz.");
            }
            if (!string.IsNullOrEmpty(user.PasswordHash) && !IsPasswordComplex(user.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Şifre en az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.");
            }

            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Username == user.Username)) // await eklendi
                {
                    ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış.");
                }
                if (await _context.Users.AnyAsync(u => u.Email == user.Email)) // await eklendi
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                }
                if (!string.IsNullOrEmpty(user.PhoneNumber) && await _context.Users.AnyAsync(u => u.PhoneNumber == user.PhoneNumber)) // 
                {
                    ModelState.AddModelError("PhoneNumber", "Bu telefon numarası zaten kullanılıyor.");
                }

                if (ModelState.IsValid)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash); // <-- BURAYI DÜZELTİN!
                    user.Role = UserRole.Customer; // Varsayılan olarak müşteri rolü ver
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // <-- BURAYI DÜZELTİN
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties { IsPersistent = false };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    TempData["SuccessMessage"] = "Kaydınız başarıyla tamamlandı!";
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            ViewData["Title"] = "Şifremi Unuttum";
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string emailOrUsername)
        {
            if (string.IsNullOrEmpty(emailOrUsername))
            {
                ModelState.AddModelError("", "Lütfen e-posta adresinizi veya kullanıcı adınızı giriniz.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername);

            if (user == null)
            {
                TempData["Message"] = "Şifre sıfırlama linki e-posta adresinize gönderilmiştir.";
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            var resetToken = Guid.NewGuid().ToString("N").Substring(0, 16);
            TempData["ResetToken"] = resetToken;
            TempData["UserIdForReset"] = user.Id;
            TempData["UserEmailForReset"] = user.Email;

            TempData["Message"] = "Şifre sıfırlama linki e-posta adresinize gönderilmiştir.";
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            ViewData["Title"] = "Şifre Sıfırlama Onayı";
            return View();
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, int userId)
        {
            if (string.IsNullOrEmpty(token) || userId == 0)
            {
                TempData["ErrorMessage"] = "Geçersiz şifre sıfırlama linki.";
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı veya geçersiz link.";
                return RedirectToAction("Login");
            }

            ViewData["Title"] = "Şifre Sıfırla";
            var model = new ResetPasswordViewModel { Token = token, UserId = userId };
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(model.UserId);
                if (user == null || TempData["ResetToken"] as string != model.Token)
                {
                    ModelState.AddModelError("", "Geçersiz veya süresi dolmuş şifre sıfırlama linki.");
                    return View(model);
                }

                if (!IsPasswordComplex(model.NewPassword))
                {
                    ModelState.AddModelError("NewPassword", "Şifre en az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.");
                    return View(model);
                }

                // GÜVENLİK DÜZELTMESİ: Şifreyi HASHLE!
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword); // <-- BURAYI DÜZELTİN!

                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Şifreniz başarıyla sıfırlandı. Lütfen yeni şifrenizle giriş yapın.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return RedirectToAction("Login");
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Login");
            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(User updatedUser, string? newPassword)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return RedirectToAction("Login");
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Login");

            if (!string.IsNullOrEmpty(newPassword))
            {
                if (!IsPasswordComplex(newPassword))
                {
                    ModelState.AddModelError("PasswordHash", "Şifre en az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.");
                    return View(user);
                }
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.PhoneNumber = updatedUser.PhoneNumber;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profiliniz güncellendi.";
            return View(user);
        }

        private bool IsPasswordComplex(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (!password.Any(ch => !char.IsLetterOrDigit(ch) && !char.IsWhiteSpace(ch))) return false;
            return true;
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}