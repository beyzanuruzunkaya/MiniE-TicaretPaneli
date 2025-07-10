// Program.cs
using Microsoft.EntityFrameworkCore; // Bu using ifadesi olmalý
using MiniE_TicaretPaneli.Data; // DbContext'inizin namespace'i
using MiniE_TicaretPaneli.Models; // SeedData içinde User ve Product kullanýlýyorsa
using Microsoft.AspNetCore.Authentication.Cookies;
using System; // TimeSpan için
using Microsoft.Extensions.DependencyInjection; // IServiceScopeFactory için (SeedData'da kullanýlýyor)
using Microsoft.Extensions.Logging; // ILogger için (SeedData'da kullanýlýyor)


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Giriþ sayfamýzýn yolu
        options.LogoutPath = "/Account/Logout"; // Çýkýþ sayfamýzýn yolu
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisiz eriþim yolu
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Oturum süresi
        options.SlidingExpiration = true; // Oturumun yenilenip yenilenmeyeceði
    });

builder.Services.AddAuthorization(); // Yetkilendirme servisi

var app = builder.Build();


// Configure the HTTP request pipeline.
// Geliþtirme ortamý dýþýndaki hatalarý yönetir (Production için)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
// Geliþtirme ortamý için detaylý hata sayfasý (Production'da kapatýlmalý)
if (app.Environment.IsDevelopment()) // <<<< Bu bloðu ekledim veya kontrol ettim
{
    app.UseDeveloperExceptionPage();
}


app.UseHttpsRedirection(); // HTTPS yönlendirmesi
app.UseStaticFiles();     // wwwroot klasöründeki statik dosyalarý (CSS, JS, resimler) sunar

app.UseRouting();         // <<<< BURASI 1: Routing middleware'i


// <<<<<<< BURASI 2: AUTHENTICATION VE AUTHORIZATION MIDDLEWARE'LERÝ >>>>>>>
// Mutlaka UseRouting() ve app.MapControllerRoute() arasýna gelmeli
app.UseAuthentication();  // Kullanýcýnýn kimliðini doðrular (çerezi okur)
app.UseAuthorization();   // Doðrulanmýþ kullanýcýnýn belirli bir kaynaða eriþim yetkisini kontrol eder


// Seed Data'yý uygula (bu kýsým genellikle bu sýralamada problem çýkarmaz)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        //SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

// <<<<<<< BURASI 3: ENDPOINT ROUTING (Controller route tanýmý) >>>>>>>
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Varsayýlan baþlangýç sayfanýz

app.Run();