// Program.cs
using Microsoft.EntityFrameworkCore;
using MiniE_TicaretPaneli.Data; // DbContext'inizin namespace'i
using MiniE_TicaretPaneli.Models; // Modelleriniz (User, Product vb.) i�in
using Microsoft.AspNetCore.Authentication.Cookies; // �erez tabanl� kimlik do�rulama i�in
using System; // TimeSpan gibi tipler i�in
using Microsoft.Extensions.DependencyInjection; // IServiceScopeFactory i�in
using Microsoft.Extensions.Logging; // ILogger i�in
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation; // Razor Runtime Compilation i�in

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
    app.UseDeveloperExceptionPage(); // Geli�tirme ortam� i�in detayl� hata sayfas�
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Kimlik do�rulama middleware'i (UseRouting'den sonra, UseAuthorization'dan �nce)
app.UseAuthorization();  // Yetkilendirme middleware'i (UseAuthentication'dan sonra)

// Seed Data'y� uygula (Veritaban� bo�sa kullan�c�lar�, kategorileri ve �r�nleri ekler)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Sadece kullan�c�lar, kategoriler veya �r�nler tablosu bo�sa seed et
        if (!context.Users.Any() && !context.Categories.Any() && !context.Products.Any())
        {
            SeedData.Initialize(services);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Uygulama ba�lang�c�nda veri doldurma (SeedData) s�ras�nda hata olu�tu.");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();