// Data/ApplicationDbContext.cs
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

            // Kullanıcı adı benzersiz olmalı
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // <<<<< BURAYI EKLEYİN (E-posta benzersiz olmalı) >>>>>
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // <<<<< BURAYI EKLEYİN (Telefon numarası benzersiz olmalı) >>>>>
            // Not: MSSQL'de NULL değerler unique constraint'i bozmaz, yani birden fazla NULL telefon numarası olabilir.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();


            // Kategori Hiyerarşisi (Self-Referencing)
            modelBuilder.Entity<Category>()
                .HasMany(c => c.SubCategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Product - MainCategory (Ana Kategori) ilişkisi
            modelBuilder.Entity<Product>()
                .HasOne(p => p.MainCategory)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.MainCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product - SubCategory (Alt Kategori) ilişkisi
            modelBuilder.Entity<Product>()
                .HasOne(p => p.SubCategory)
                .WithMany()
                .HasForeignKey(p => p.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);


            // Diğer mevcut ilişkiler
            modelBuilder.Entity<User>()
                .HasMany(u => u.CreditCards)
                .WithOne(cc => cc.User)
                .HasForeignKey(cc => cc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.CartItems)
                .WithOne(ci => ci.User)
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ShoppingCart>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Eğer User.Role enum'ı string olarak kaydedilecekse
            /*
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();
            */
        }
    }
}