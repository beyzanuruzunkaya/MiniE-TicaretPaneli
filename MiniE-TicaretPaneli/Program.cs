// Program.cs
using Microsoft.EntityFrameworkCore;
using MiniE_TicaretPaneli.Data;
using MiniE_TicaretPaneli.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation; // Razor Runtime Compilation için
using Microsoft.AspNetCore.CookiePolicy; // CookiePolicyOptions için
using Microsoft.AspNetCore.Antiforgery; // Antiforgery için
using Microsoft.Extensions.Logging; // ILogger için
using Microsoft.AspNetCore.Hosting; // IWebHostEnvironment için (otomatik enjekte edilir)
using System.Collections.Generic;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// ************************************************************
// 1. SERVICES CONFIGURATION (Servislerin Yapılandırılması)
// ************************************************************

// MVC Controller'ları ve View'ları ekle
builder.Services.AddMemoryCache();

builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        // View konum formatlarını ekle (modüler yapınız için önemli)
        options.ViewLocationFormats.Add("/Views/Admin/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Views/Admin/Product/{0}.cshtml");
        options.ViewLocationFormats.Add("/Views/Customer/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Views/Customer/Product/{0}.cshtml"); // Müşteri Product Controller View'ları için eklendi
    })
    .AddRazorRuntimeCompilation(); // Geliştirme sırasında Razor dosyalarının anında derlenmesi için

// Veritabanı bağlamını (ApplicationDbContext) servislere ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Çerez politikası ayarları (GDPR uyumluluğu ve güvenlik için)
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.Secure = CookieSecurePolicy.Always; // Çerezlerin sadece HTTPS üzerinden gönderilmesini zorunlu kıl
    options.MinimumSameSitePolicy = SameSiteMode.Strict; // Daha katı SameSite politikası
    options.HttpOnly = HttpOnlyPolicy.Always; // JavaScript erişimini engelle
});

// Cookie Authentication servislerini ekle
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";         // Giriş yapılmadığında yönlendirilecek sayfa
        options.LogoutPath = "/Account/Logout";       // Çıkış yapıldığında yönlendirilecek sayfa
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisi olmadığında yönlendirilecek sayfa
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Oturum süresi
        options.SlidingExpiration = true;             // Oturum süresini otomatik uzat
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Çerezin güvenli (HTTPS) olmasını zorunlu kıl
        options.Cookie.IsEssential = true;            // Çerezin uygulamanın çalışması için gerekli olduğunu belirtir
    });

// Antiforgery token'ı için çerez güvenliği ayarı
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Yetkilendirme servislerini ekle
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CustomerPolicy", policy => policy.RequireRole("Customer")); // Müşteri rolü için politika
});

var app = builder.Build();

// ************************************************************
// 2. HTTP REQUEST PIPELINE CONFIGURATION (HTTP İstek İşlem Hattının Yapılandırılması)
// ************************************************************

// Geliştirme ortamı kontrolü ve hata sayfaları
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Bu satırın aktif olduğundan emin olun
    // app.UseBrowserLink();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// HTTPS'e yönlendirme
app.UseHttpsRedirection();

// Statik dosyaları (wwwroot klasöründeki CSS, JS, resimler vb.) sunar
app.UseStaticFiles();

// Çerez politikası middleware'i (UseAuthentication'dan önce olmalı)
app.UseCookiePolicy();

// Routing middleware'ini etkinleştirir
app.UseRouting();

// Kimlik Doğrulama middleware'ini etkinleştirir (UseRouting'den sonra, UseAuthorization'dan önce)
app.UseAuthentication();

// Yetkilendirme middleware'ini etkinleştirir (UseAuthentication'dan sonra)
app.UseAuthorization();

// Seed Data'yı uygula (Veritabanı boşsa kullanıcıları, kategorileri ve ürünleri ekler)
// Bu blok tamamen kaldırıldı.

// Varsayılan route'u tanımla
// Uygulama ilk açıldığında Home/Index'e yönlendirilecek.
// Eğer Admin/Products'a gitmesini isterseniz: pattern: "{controller=Admin}/{action=Products}/{id?}" olarak değiştirin.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
); // Artık Customer/Product/Products varsayılan başlangıç

// Attribute routing ile tanımlanan controller/action endpoint'leri için eklendi
app.MapControllers();

// Uygulamayı çalıştır
app.Run();