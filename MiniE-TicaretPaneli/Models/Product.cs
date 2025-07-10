// Models/Product.cs

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace MiniE_TicaretPaneli.Models
{
    public class Product
    {
        public int Id { get; set; }

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
    }
}