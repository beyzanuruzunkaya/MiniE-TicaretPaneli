// Program.cs
using Microsoft.EntityFrameworkCore;
using MiniE_TicaretPaneli.Data; // DbContext'inizin namespace'i
using MiniE_TicaretPaneli.Models; // Modelleriniz (User, Product vb.) için
using Microsoft.AspNetCore.Authentication.Cookies; // Çerez tabanlý kimlik doðrulama için
using System; // TimeSpan gibi tipler için
using Microsoft.Extensions.DependencyInjection; // IServiceScopeFactory için
using Microsoft.Extensions.Logging; // ILogger için
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation; // Razor Runtime Compilation için

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Geliþtirme ortamý için detaylý hata sayfasý
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Kimlik doðrulama middleware'i (UseRouting'den sonra, UseAuthorization'dan önce)
app.UseAuthorization();  // Yetkilendirme middleware'i (UseAuthentication'dan sonra)

// Seed Data'yý uygula (Veritabaný boþsa kullanýcýlarý, kategorileri ve ürünleri ekler)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Sadece kullanýcýlar, kategoriler veya ürünler tablosu boþsa seed et
        if (!context.Users.Any() && !context.Categories.Any() && !context.Products.Any())
        {
            SeedData.Initialize(services);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Uygulama baþlangýcýnda veri doldurma (SeedData) sýrasýnda hata oluþtu.");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();