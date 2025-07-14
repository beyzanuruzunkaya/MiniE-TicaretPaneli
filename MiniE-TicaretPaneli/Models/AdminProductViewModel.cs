// Models/ViewModels/AdminProductViewModel.cs
using System.Collections.Generic;
using MiniE_TicaretPaneli.Models; // Product ve Category modelleri için
using System.ComponentModel.DataAnnotations; // Data Annotations için
using Microsoft.AspNetCore.Http; // IFormFile için

namespace MiniE_TicaretPaneli.Models.ViewModels
{
    public class AdminProductViewModel
    {
<<<<<<< HEAD
        public Product Product { get; set; } = new Product();

        // BU LİSTELERİN TAM OLDUĞUNDAN EMİN OLUN
        public List<Category> GenderCategories { get; set; } = new List<Category>();
        public List<Category> MainCategories { get; set; } = new List<Category>();
        public List<Category> SubCategories { get; set; } = new List<Category>();

        public List<string> AllSizes { get; set; } = new List<string>();
        public List<string> AllColors { get; set; } = new List<string>();

        public IFormFile? ProductImage { get; set; }
        public string? ExistingImageUrl { get; set; }

=======
        public Product Product { get; set; } = new Product(); // Ürün bilgileri

        public List<Category> MainCategories { get; set; } = new List<Category>(); // Ana kategori dropdown için
        public List<Category> SubCategories { get; set; } = new List<Category>(); // Alt kategori dropdown için

        public List<string> AllSizes { get; set; } = new List<string>(); // Tüm beden seçenekleri (örn: S, M, L)
        public List<string> AllColors { get; set; } = new List<string>(); // Tüm renk seçenekleri (örn: Kırmızı, Mavi)

        public IFormFile? ProductImage { get; set; } // Resim yüklemek için
        public string? ExistingImageUrl { get; set; } // Mevcut resim URL'si

        // Checkbox'lardan seçilen beden ve renkler için geçici listeler
>>>>>>> origin/master
        public List<string> SelectedSizes { get; set; } = new List<string>();
        public List<string> SelectedColors { get; set; } = new List<string>();
    }
}