// Models/ViewModels/AdminProductViewModel.cs
using System.Collections.Generic;
using MiniE_TicaretPaneli.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema; // NotMapped için

namespace MiniE_TicaretPaneli.Models.ViewModels // Namespace'in ViewModels olduğuna DİKKAT!
{
    public class SimpleCategoryDto // Bu ViewModel içinde veya ayrı bir dosyada olabilir
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public string? Gender { get; set; } // Ana kategori için cinsiyet
        public string? Type { get; set; } // Yeni eklendi: Category modelindeki Type'ı yansıtmak için
    }

    public class AdminProductViewModel
    {
        // Product nesnesi doğrudan burada tutuluyor (sizin tercihiniz)
        public Product Product { get; set; } = new Product();

        // Kategori dropdown'larını doldurmak için ViewModel'e eklenen listeler
        // Bu listeler artık Category modelinden çekilecek ve view'a iletilecek
        [NotMapped] // Veritabanına eşlenmeyecek
        public List<Category>? GenderCategories { get; set; } // Yeni eklendi: Product modelindeki GenderCategoryId için
        [NotMapped] // Veritabanına eşlenmeyecek
        public List<Category>? MainCategories { get; set; }
        [NotMapped] // Veritabanına eşlenmeyecek
        public List<Category>? SubCategories { get; set; }

        public List<string> AllSizes { get; set; } = new List<string>();
        public List<string> AllColors { get; set; } = new List<string>();

        public IFormFile? ProductImage { get; set; }
        public string? ExistingImageUrl { get; set; }

        public List<string> SelectedSizes { get; set; } = new List<string>();
        public List<string> SelectedColors { get; set; } = new List<string>();

        // JavaScript için tüm kategorileri taşıyan DTO listesi (modüler kategori yüklemesi için)
        public List<SimpleCategoryDto> AllCategoriesForJs { get; set; } = new List<SimpleCategoryDto>();
    }
}