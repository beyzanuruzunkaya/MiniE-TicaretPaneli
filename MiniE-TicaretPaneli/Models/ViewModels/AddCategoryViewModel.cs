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

        public int? ParentCategoryId { get; set; }

        [Required(ErrorMessage = "Cinsiyet seçimi zorunludur.")]
        [StringLength(50, ErrorMessage = "Cinsiyet en fazla 50 karakter olmalıdır.")]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Kategori tipi zorunludur.")]
        [StringLength(50, ErrorMessage = "Kategori tipi en fazla 50 karakter olmalıdır.")]
        public string? Type { get; set; }

        [Required(ErrorMessage = "Kategori değeri zorunludur.")]
        [StringLength(100, ErrorMessage = "Kategori değeri en fazla 100 karakter olmalıdır.")]
        public string? Value { get; set; }

        [StringLength(200, ErrorMessage = "Slug en fazla 200 karakter olmalıdır.")]
        public string? Slug { get; set; }

        [Required(ErrorMessage = "Lütfen ekleyeceğiniz kategori seviyesini seçiniz.")]
        public string CategoryLevelType { get; set; } = string.Empty; // "MainGroup" veya "ProductType"

        public List<SelectListItem> AvailableParentCategories { get; set; } = new List<SelectListItem>();
        public List<Category> AllCategoriesForJs { get; set; } = new List<Category>();
    }
}