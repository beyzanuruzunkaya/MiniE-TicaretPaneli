// Models/Product.cs
<<<<<<< HEAD
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

=======

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
>>>>>>> origin/master
namespace MiniE_TicaretPaneli.Models
{
    public class Product
    {
        public int Id { get; set; }

<<<<<<< HEAD
        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olmalıdır.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olmalıdır.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        public float Price { get; set; } // Float olarak kalacak

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
        public string Gender { get; set; } = string.Empty; // "Kadın", "Erkek", "Çocuk"

        [StringLength(200, ErrorMessage = "Bedenler en fazla 200 karakter olmalıdır.")]
        public string AvailableSizes { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Renkler en fazla 200 karakter olmalıdır.")]
        public string AvailableColors { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Malzeme en fazla 100 karakter olmalıdır.")]
        public string? Material { get; set; }

        [StringLength(100, ErrorMessage = "Desen en fazla 100 karakter olmalıdır.")]
        public string? Pattern { get; set; }
=======
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public float Price { get; set; } 
        public string? ImageUrl { get; set; }

        public int Stock { get; set; }

        // Yeni Eklentiler: Kategorizasyon ve Filtreleme için

        // Ürünün ait olduğu Ana Kategori (örn: Kadın, Erkek, Anne & Çocuk)
        public int MainCategoryId { get; set; }
        public Category MainCategory { get; set; } = null!; // Navigation property

        // Ürünün ait olduğu Alt Kategori (örn: Tişört, Jean Pantolon, Elbise)
        public int SubCategoryId { get; set; }
        public Category SubCategory { get; set; } = null!; // Navigation property

        // Marka bilgisi
        [MaxLength(100)]
        public string? Brand { get; set; }

        // Ürünün uygulanabileceği Cinsiyet (Product seviyesinde filtreleme için)
        // Eğer Category.Type ve Category.Value ile tamamiyle yönetilecekse bu gerekmeyebilir.
        // Ama hızlı filtreleme için tutmak işe yarar.
        [MaxLength(50)]
        public string Gender { get; set; } = "Unisex"; // "Kadın", "Erkek", "Çocuk", "Unisex"

        // Bedenler (virgülle ayrılmış string olarak basitçe tutulabilir: "S,M,L,XL")
        [MaxLength(200)]
        public string AvailableSizes { get; set; } = string.Empty;

        // Renkler (virgülle ayrılmış string olarak basitçe tutulabilir: "Siyah,Beyaz,Mavi")
        [MaxLength(200)]
        public string AvailableColors { get; set; } = string.Empty;

        // Diğer özellikler (malzeme, yaka tipi, desen vb. için ayrı ProductFeature sınıfı düşünülebilir)
        // Şimdilik Product.cs içinde kalabilir.
        [MaxLength(100)]
        public string? Material { get; set; } // Kumaş tipi (Pamuk, Polyester vb.)
        [MaxLength(100)]
        public string? Pattern { get; set; }  // Desen (Düz, Çizgili, Çiçekli vb.)
>>>>>>> origin/master
    }
}