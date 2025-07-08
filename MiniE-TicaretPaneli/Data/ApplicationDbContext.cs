using Microsoft.EntityFrameworkCore;
using MiniE_TicaretPaneli.Models;

namespace MiniE_TicaretPaneli.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<CreditCard> CreditCards { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<ShoppingCart> CartItems { get; set; } = null!;

    }
}
