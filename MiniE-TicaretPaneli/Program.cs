// Program.cs
using Microsoft.EntityFrameworkCore; // Bu using ifadesi olmal�
using MiniE_TicaretPaneli.Data; // DbContext'inizin namespace'i
using MiniE_TicaretPaneli.Models; // SeedData i�inde User ve Product kullan�l�yorsa
using Microsoft.AspNetCore.Authentication.Cookies;
using System; // TimeSpan i�in
using Microsoft.Extensions.DependencyInjection; // IServiceScopeFactory i�in (SeedData'da kullan�l�yor)
using Microsoft.Extensions.Logging; // ILogger i�in (SeedData'da kullan�l�yor)


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Giri� sayfam�z�n yolu
        options.LogoutPath = "/Account/Logout"; // ��k�� sayfam�z�n yolu
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisiz eri�im yolu
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Oturum s�resi
        options.SlidingExpiration = true; // Oturumun yenilenip yenilenmeyece�i
    });

builder.Services.AddAuthorization(); // Yetkilendirme servisi

var app = builder.Build();


// Configure the HTTP request pipeline.
// Geli�tirme ortam� d���ndaki hatalar� y�netir (Production i�in)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
// Geli�tirme ortam� i�in detayl� hata sayfas� (Production'da kapat�lmal�)
if (app.Environment.IsDevelopment()) // <<<< Bu blo�u ekledim veya kontrol ettim
{
    app.UseDeveloperExceptionPage();
}


app.UseHttpsRedirection(); // HTTPS y�nlendirmesi
app.UseStaticFiles();     // wwwroot klas�r�ndeki statik dosyalar� (CSS, JS, resimler) sunar

app.UseRouting();         // <<<< BURASI 1: Routing middleware'i


// <<<<<<< BURASI 2: AUTHENTICATION VE AUTHORIZATION MIDDLEWARE'LER� >>>>>>>
// Mutlaka UseRouting() ve app.MapControllerRoute() aras�na gelmeli
app.UseAuthentication();  // Kullan�c�n�n kimli�ini do�rular (�erezi okur)
app.UseAuthorization();   // Do�rulanm�� kullan�c�n�n belirli bir kayna�a eri�im yetkisini kontrol eder


// Seed Data'y� uygula (bu k�s�m genellikle bu s�ralamada problem ��karmaz)
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

// <<<<<<< BURASI 3: ENDPOINT ROUTING (Controller route tan�m�) >>>>>>>
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Varsay�lan ba�lang�� sayfan�z

app.Run();