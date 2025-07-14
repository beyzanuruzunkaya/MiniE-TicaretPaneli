// Models/ViewModels/AddCategoryViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // SelectListItem için
using MiniE_TicaretPaneli.Models; // Category modeli için

namespace MiniE_TicaretPaneli.Models.ViewModels
{
    public class AddCategoryViewModel
    {
        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olmalıdır.")]
        public string Name { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; } // Seçilen üst kategorinin ID'si

        [Required(ErrorMessage = "Cinsiyet seçimi zorunludur.")] // Yeni bir dropdown olacağı için
        [StringLength(50, ErrorMessage = "Cinsiyet en fazla 50 karakter olmalıdır.")]
        public string? Gender { get; set; } // Seçilen cinsiyet (Product.Gender ile aynı tipten)

        [Required(ErrorMessage = "Kategori tipi zorunludur.")]
        [StringLength(50, ErrorMessage = "Kategori tipi en fazla 50 karakter olmalıdır.")]
        public string? Type { get; set; } // Kategori Tipi (örn: "Ürün Grubu", "Ürün Tipi", "Cinsiyet", "Yaş Grubu")

        [Required(ErrorMessage = "Kategori değeri zorunludur.")]
        [StringLength(100, ErrorMessage = "Kategori değeri en fazla 100 karakter olmalıdır.")]
        public string? Value { get; set; } // Kategori Değeri (örn: "Giyim", "Elbise", "Kadın")

        [StringLength(200, ErrorMessage = "Slug en fazla 200 karakter olmalıdır.")]
        public string? Slug { get; set; }

        // Formdaki "Kategori Seviyesi" seçimi için (Main Group mu yoksa Product Type mı ekleniyor)
        [Required(ErrorMessage = "Lütfen ekleyeceğiniz kategori seviyesini seçiniz.")]
        public string CategoryLevelType { get; set; } = string.Empty; // "MainGroup" veya "ProductType"

        // Dropdown'lar için gerekli listeler (Controller'dan View'a gelecek)
        public List<SelectListItem> AvailableParentCategories { get; set; } = new List<SelectListItem>(); // Seçilebilir üst kategoriler (JS ile filtrelenecek)
        public List<Category> AllCategoriesForJs { get; set; } = new List<Category>(); // Tüm kategoriler JS'ye JSON olarak gönderilecek
    }
}