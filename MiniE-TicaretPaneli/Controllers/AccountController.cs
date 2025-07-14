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
using Microsoft.EntityFrameworkCore; // List için

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
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password); // GÜVENSİZ ŞİFRE KARŞILAŞTIRMASI

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role.ToString()), // Enum'ı string'e çevir
                    new Claim("UserId", user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                if (user.Role == UserRole.Admin)
                {
                    return RedirectToAction("AdminDashboard", "Admin");
                }
                else if (user.Role == UserRole.Customer)
                {
                    return RedirectToAction("Index", "Home"); // Müşteri rolündeyse Anasayfaya yönlendir
                }
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
            // Debugging için user nesnesini burada inceleyebilirsiniz

            // Ek güvenlik kontrolü: Eğer PasswordHash veya diğer [Required] alanlar boş gelirse
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Şifre alanı boş bırakılamaz.");
            }
            // FirstName, LastName, Email zorunlu olduğu için ModelState.IsValid tarafından yakalanır
            // Ancak yine de explicit kontrol eklemek isterseniz benzer şekilde ekleyebilirsiniz.

            // Şifre Karmaşıklığı Kontrolü (Sunucu tarafı)
            if (!string.IsNullOrEmpty(user.PasswordHash) && !IsPasswordComplex(user.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Şifre en az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.");
            }

            // Ana Model doğrulama ve ek kontroller
            if (ModelState.IsValid) // Tüm Data Annotation kuralları ve yukarıdaki manuel kontroller geçerliyse
            {
                // Kullanıcı adı, e-posta veya telefon numarası zaten kullanılıyor mu?
                if (_context.Users.Any(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış.");
                }
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                }
                // Telefon numarası boş değilse ve kullanılıyorsa kontrol et
                if (!string.IsNullOrEmpty(user.PhoneNumber) && _context.Users.Any(u => u.PhoneNumber == user.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "Bu telefon numarası zaten kullanılıyor.");
                }

                // Eğer yukarıdaki kontrollerden sonra hala ModelState geçerliyse kaydet
                if (ModelState.IsValid)
                {
                    user.Role = UserRole.Customer; // Varsayılan olarak müşteri rolü ver
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Kayıt başarılı olduktan sonra kullanıcıyı otomatik olarak giriş yaptır
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role.ToString()),
                        new Claim("UserId", user.Id.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties { IsPersistent = false };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    TempData["SuccessMessage"] = "Kaydınız başarıyla tamamlandı!";
                    return RedirectToAction("Index", "Home"); // Anasayfaya yönlendir
                }
            }

            // Model geçerli değilse veya doğrulama hataları varsa formu tekrar göster
            return View(user); // 'user' nesnesini View'a geri gönderiyoruz
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

            // Kullanıcıyı e-posta veya kullanıcı adına göre bul
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername);

            if (user == null)
            {
                // Güvenlik amacıyla, kullanıcı bulunsa bile aynı mesajı gösterin
                TempData["Message"] = "Şifre sıfırlama linki e-posta adresinize gönderilmiştir.";
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            // Şifre sıfırlama token'ı oluşturma simülasyonu
            // Gerçek bir senaryoda bu token güvenli ve zaman sınırlı olurdu.
            // Bu token'ı veritabanında da saklamanız gerekebilir.
            var resetToken = Guid.NewGuid().ToString("N").Substring(0, 16); // Basit bir token

            // Gerçekte: Bu token ve kullanıcının ID'si ile bir e-posta gönderilir.
            // Örneğin: "http://localhost:port/Account/ResetPassword?token=resetToken&userId=user.Id"
            // ŞİMDİLİK: Token'ı TempData ile bir sonraki sayfaya aktaracağız.
            TempData["ResetToken"] = resetToken;
            TempData["UserIdForReset"] = user.Id;
            TempData["UserEmailForReset"] = user.Email; // Onay sayfasında göstermek için

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
        // Token ve UserId ile çağrılır
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, int userId)
        {
            // Gerçek uygulamada: Token'ın geçerliliği (varlığı, süresi) burada kontrol edilir.
            // Bu bir simülasyon olduğu için sadece token'ın varlığını kontrol ediyoruz.
            if (string.IsNullOrEmpty(token) || userId == 0)
            {
                TempData["ErrorMessage"] = "Geçersiz şifre sıfırlama linki.";
                return RedirectToAction("Login");
            }

            // Kullanıcıyı bul
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı veya geçersiz link.";
                return RedirectToAction("Login");
            }

            // View'a modeli göndererek token ve userId'yi hidden olarak tut
            ViewData["Title"] = "Şifre Sıfırla";
            var model = new ResetPasswordViewModel { Token = token, UserId = userId };
            return View(model);
        }

        // POST: /Account/ResetPassword
        // Yeni şifreyi ayarla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            // Model doğrulaması (şifreler uyuşuyor mu, karmaşıklık vb.)
            if (ModelState.IsValid)
            {
                // Gerçek uygulamada: Token'ın geçerliliği (varlığı, süresi) tekrar kontrol edilir ve kullanılırsa geçersiz kılınır.
                // Bu bir simülasyon olduğu için basit bir kontrol yapıyoruz.
                var user = await _context.Users.FindAsync(model.UserId);
                if (user == null || TempData["ResetToken"] as string != model.Token) // Basit token kontrolü
                {
                    ModelState.AddModelError("", "Geçersiz veya süresi dolmuş şifre sıfırlama linki.");
                    return View(model);
                }

                // Şifre karmaşıklığı kontrolü (PasswordHash alanı User modelindeki karmaşıklık kurallarıyla uyumlu olmalı)
                if (!IsPasswordComplex(model.NewPassword))
                {
                    ModelState.AddModelError("NewPassword", "Şifre en az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.");
                    return View(model);
                }

                // Şifreyi güncelle (Hashleme!)
                user.PasswordHash = model.NewPassword; // !!! GÜVENSİZ! GERÇEKTE HASHLEMELİSİNİZ

                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Şifreniz başarıyla sıfırlandı. Lütfen yeni şifrenizle giriş yapın.";
                return RedirectToAction("Login");
            }

            // Doğrulama hatası varsa formu tekrar göster
            return View(model);
        }


        // Şifre Karmaşıklığı Kontrolü Metodu
        private bool IsPasswordComplex(string password)
        {
            // En az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter
            if (password.Length < 8) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            // Harf veya rakam olmayan herhangi bir karakter (boşluklar dahil edilmez)
            if (!password.Any(ch => !char.IsLetterOrDigit(ch) && !char.IsWhiteSpace(ch))) return false;
            return true;
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Oturumu sonlandır
            return RedirectToAction("Login", "Account"); // Giriş sayfasına geri yönlendir
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}