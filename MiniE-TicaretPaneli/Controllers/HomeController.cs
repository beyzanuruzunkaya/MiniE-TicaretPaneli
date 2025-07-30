using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MiniE_TicaretPaneli.Models;
using MiniE_TicaretPaneli.Models.ViewModels;
using MiniE_TicaretPaneli.Data;
using Microsoft.EntityFrameworkCore;

namespace MiniE_TicaretPaneli.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _context.Products
                .Include(p => p.MainCategory)
                .Include(p => p.SubCategory)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToListAsync();
            return View(featuredProducts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
