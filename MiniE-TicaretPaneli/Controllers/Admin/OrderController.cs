// Controllers/Admin/OrderController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniE_TicaretPaneli.Data;
using MiniE_TicaretPaneli.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace MiniE_TicaretPaneli.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")]
    //[Authorize(Roles = "Admin")] // Bu satır yorum satırı olarak kalabilir veya açılabilir.
    public class OrderController : Controller // Sadece bir kere bu tanım olmalı!
    {
        private readonly ApplicationDbContext _context; // Sadece bir kere bu tanımlama olmalı!
        

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Sales()
        {
            ViewData["Title"] = "Satış Görüntüleme";
            var orders = await _context.Orders.Include(o => o.User).Include(o => o.OrderItems).ThenInclude(oi => oi.Product).OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string newStatus)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Sipariş bulunamadı.";
                return RedirectToAction("Sales");
            }
            order.Status = newStatus;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Sipariş durumu güncellendi.";
            return RedirectToAction("Sales");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminCancelOrder(int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Sipariş bulunamadı.";
                return RedirectToAction("Sales");
            }
            order.Status = "Cancelled";
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Sipariş başarıyla iptal edildi.";
            return RedirectToAction("Sales");
        }
    }
}