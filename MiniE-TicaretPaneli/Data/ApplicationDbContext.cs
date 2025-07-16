using Microsoft.EntityFrameworkCore;
using MiniE_TicaretPaneli.Models;

namespace MiniE_TicaretPaneli.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet'ler
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<CreditCard> CreditCards { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<ShoppingCart> CartItems { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Entity Konfigürasyonu
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();


            // Category Entity Konfigürasyonu (Hiyerarşik Yapı)
            modelBuilder.Entity<Category>()
                .HasMany(c => c.SubCategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // Product - Category İlişkileri (Yeni Hiyerarşi)

            // Product - GenderCategory (Seviye 1 Cinsiyet/Yaş Grubu Kategorisi) ilişkisi
            modelBuilder.Entity<Product>()
                .HasOne(p => p.GenderCategory)
                .WithMany()
                .HasForeignKey(p => p.GenderCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product - MainCategory (Seviye 2 Ürün Grubu Kategorisi) ilişkisi
            modelBuilder.Entity<Product>()
                .HasOne(p => p.MainCategory)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.MainCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product - SubCategory (Seviye 3 Ürün Tipi Kategorisi) ilişkisi
            modelBuilder.Entity<Product>()
                .HasOne(p => p.SubCategory)
                .WithMany()
                .HasForeignKey(p => p.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - CreditCard ilişkisi
            modelBuilder.Entity<User>()
                .HasMany(u => u.CreditCards)
                .WithOne(cc => cc.User)
                .HasForeignKey(cc => cc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Order ilişkisi
            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - ShoppingCart (CartItems) ilişkisi
            modelBuilder.Entity<User>()
                .HasMany(u => u.CartItems)
                .WithOne(ci => ci.User)
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order - OrderItem ilişkisi
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem - Product ilişkisi
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ShoppingCart - Product ilişkisi
            modelBuilder.Entity<ShoppingCart>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}