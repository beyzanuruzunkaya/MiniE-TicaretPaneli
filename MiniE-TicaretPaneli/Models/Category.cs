// Models/Category.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniE_TicaretPaneli.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        public Category? ParentCategory { get; set; }

        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        public ICollection<Product> Products { get; set; } = new List<Product>();

        public string? Type { get; set; }
        public string? Value { get; set; }
        public string? Slug { get; set; }

        // <<<<<< YENİ EKLENTİ: Cinsiyet özelliği >>>>>
        [MaxLength(50)]
        public string? Gender { get; set; } // "Kadın", "Erkek", "Çocuk", "Unisex" veya null (genel kategoriler için)

        [NotMapped]
        public string FullPath { get; set; } = string.Empty;
    }
}