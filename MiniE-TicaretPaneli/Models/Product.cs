namespace MiniE_TicaretPaneli.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Varsayılan değerler
        public string Description { get; set; } = string.Empty;
        public float Price { get; set; }
        public string? ImageUrl { get; set; } // Boş bırakılabilir
        public int Stock { get; set; }
    }
}
