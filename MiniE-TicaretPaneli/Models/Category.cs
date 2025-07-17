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
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        public Category? ParentCategory { get; set; }

        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        public ICollection<Product> Products { get; set; } = new List<Product>();

        [StringLength(200)]
        public string? Slug { get; set; }

        [StringLength(50)]
        public string? Gender { get; set; }

        [NotMapped]
        public bool IsMainCategory => ParentCategoryId == null;

        [NotMapped] // ❗Bunu ekle veya EF migration ile veritabanına ekle
        public string FullPath { get; set; } = string.Empty;
    }
}