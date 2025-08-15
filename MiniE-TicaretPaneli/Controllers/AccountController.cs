// Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniE_TicaretPaneli.Data;
using MiniE_TicaretPaneli.Models;
using MiniE_TicaretPaneli.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BCrypt.Net;

namespace MiniE_TicaretPaneli.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public AccountController(ApplicationDbContext context, IConfiguration config, IWebHostEnvironment env)
        {
            _context = context;
            _config = config;
            _env = env;
        }

        // ===== Helpers: JWT üret ve cookie'ye yaz/sil =====
        private string GenerateJwt(User user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(double.TryParse(jwtSection["AccessTokenMinutes"], out var m) ? m : 60),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private void SetJwtCookie(string token)
        {
            var isDev = _env.IsDevelopment();
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDev ? true : false, // prod'da true
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7), // tarayıcı kapansa da kalsın (token süresi ayrı)
                IsEssential = true
            });
        }

        private void ClearJwtCookie()
        {
            Response.Cookies.Delete("access_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                IsEssential = true
            });
        }

        // ===== Login =====
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "Kullanıcı adı ve şifre zorunludur.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                var jwt = GenerateJwt(user);
                SetJwtCookie(jwt);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Geçersiz kullanıcı adı veya şifre.";
            return View();
        }

        // ===== Register =====
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Username,PasswordHash,FirstName,LastName,Email,PhoneNumber")] User user)
        {
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                ModelState.AddModelError("PasswordHash", "Şifre alanı boş bırakılamaz.");
            else if (!IsPasswordComplex(user.PasswordHash))
                ModelState.AddModelError("PasswordHash", "Şifre en az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.");

            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış.");
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
            if (!string.IsNullOrEmpty(user.PhoneNumber) && await _context.Users.AnyAsync(u => u.PhoneNumber == user.PhoneNumber))
                ModelState.AddModelError("PhoneNumber", "Bu telefon numarası zaten kullanılıyor.");

            if (!ModelState.IsValid) return View(user);

            // Hash & persist
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.Role = UserRole.Customer;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Kayıt sonrası otomatik login: JWT üret + cookie
            var jwt = GenerateJwt(user);
            SetJwtCookie(jwt);

            TempData["SuccessMessage"] = "Kaydınız başarıyla tamamlandı!";
            return RedirectToAction("Index", "Home");
        }

        // ===== Forgot / Reset Password =====
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            ViewData["Title"] = "Şifremi Unuttum";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string emailOrUsername)
        {
            if (string.IsNullOrWhiteSpace(emailOrUsername))
            {
                ModelState.AddModelError("", "Lütfen e-posta adresinizi veya kullanıcı adınızı giriniz.");
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername);

            // Güvenlik: kullanıcı yoksa da aynı mesaj
            TempData["Message"] = "Şifre sıfırlama linki e-posta adresinize gönderilmiştir.";

            if (user != null)
            {
                var resetToken = Guid.NewGuid().ToString("N").Substring(0, 16);
                TempData["ResetToken"] = resetToken;
                TempData["UserIdForReset"] = user.Id;
                TempData["UserEmailForReset"] = user.Email;
            }

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            ViewData["Title"] = "Şifre Sıfırlama Onayı";
            return View();
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null || (TempData["ResetToken"] as string) != model.Token)
            {
                ModelState.AddModelError("", "Geçersiz veya süresi dolmuş şifre sıfırlama linki.");
                return View(model);
            }

            if (!IsPasswordComplex(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "Şifre en az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.");
                return View(model);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Şifreniz başarıyla sıfırlandı. Lütfen yeni şifrenizle giriş yapın.";
            return RedirectToAction("Login");
        }

        // ===== Profile =====
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

            if (!string.IsNullOrWhiteSpace(newPassword))
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

        // ===== Logout =====
        [HttpGet]
        public IActionResult Logout()
        {
            ClearJwtCookie();
            return RedirectToAction("Login", "Account");
        }

        // ===== Utils =====
        private bool IsPasswordComplex(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (!password.Any(ch => !char.IsLetterOrDigit(ch) && !char.IsWhiteSpace(ch))) return false;
            return true;
        }

        public IActionResult AccessDenied() => View();
    }
}
