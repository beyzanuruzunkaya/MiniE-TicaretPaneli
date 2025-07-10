namespace MiniE_TicaretPaneli.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; } // Siparişi veren kullanıcının ID'si
        public User User { get; set; } = null!; // Navigation property

        public DateTime OrderDate { get; set; } // Siparişin verildiği tarih
        public float TotalAmount { get; set; } // Siparişin toplam tutarı

        public string Status { get; set; } = "Pending"; // Sipariş durumu (örn: Pending, Shipped, Delivered, Cancelled)

        // Bir siparişin birden çok ürün kalemi olabilir (one-to-many relationship)
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}

