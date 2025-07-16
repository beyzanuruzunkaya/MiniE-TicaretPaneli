// Models/Category.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniE_TicaretPaneli.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olmalıdır.")]
        public string Name { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        public Category? ParentCategory { get; set; }

        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        public ICollection<Product> Products { get; set; } = new List<Product>();

        [StringLength(50, ErrorMessage = "Tip en fazla 50 karakter olmalıdır.")]
        public string? Type { get; set; }
        [StringLength(100, ErrorMessage = "Değer en fazla 100 karakter olmalıdır.")]
        public string? Value { get; set; }

        [StringLength(200, ErrorMessage = "Slug en fazla 200 karakter olmalıdır.")]
        public string? Slug { get; set; }

        [StringLength(50, ErrorMessage = "Cinsiyet en fazla 50 karakter olmalıdır.")]
        public string? Gender { get; set; }

        [NotMapped]
        public string FullPath { get; set; } = string.Empty;
    }
}