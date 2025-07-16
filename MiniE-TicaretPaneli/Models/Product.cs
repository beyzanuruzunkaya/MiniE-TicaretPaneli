// Models/Product.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiniE_TicaretPaneli.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olmalıdır.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olmalıdır.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        public float Price { get; set; }

        [StringLength(500, ErrorMessage = "Görsel URL'si en fazla 500 karakter olmalıdır.")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Stok miktarı zorunludur.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok negatif olamaz.")]
        public int Stock { get; set; }

        // Kategori ilişkileri (Yeni Hiyerarşi)
        [Required(ErrorMessage = "Cinsiyet kategorisi seçimi zorunludur.")]
        public int GenderCategoryId { get; set; }
        public Category GenderCategory { get; set; } = null!;

        [Required(ErrorMessage = "Ana kategori seçimi zorunludur.")]
        public int MainCategoryId { get; set; }
        public Category MainCategory { get; set; } = null!;

        [Required(ErrorMessage = "Alt kategori seçimi zorunludur.")]
        public int SubCategoryId { get; set; }
        public Category SubCategory { get; set; } = null!;

        [StringLength(100, ErrorMessage = "Marka en fazla 100 karakter olmalıdır.")]
        public string? Brand { get; set; }

        [Required(ErrorMessage = "Cinsiyet belirtilmesi zorunludur.")]
        [StringLength(50, ErrorMessage = "Cinsiyet en fazla 50 karakter olmalıdır.")]
        public string Gender { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Bedenler en fazla 200 karakter olmalıdır.")]
        public string AvailableSizes { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Renkler en fazla 200 karakter olmalıdır.")]
        public string AvailableColors { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Malzeme en fazla 100 karakter olmalıdır.")]
        public string? Material { get; set; }

        [StringLength(100, ErrorMessage = "Desen en fazla 100 karakter olmalıdır.")]
        public string? Pattern { get; set; }
    }
}