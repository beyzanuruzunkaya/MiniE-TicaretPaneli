namespace MiniE_TicaretPaneli.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; } // Hangi siparişe ait olduğu
        public Order Order { get; set; } = null!; // Navigation property

        public int ProductId { get; set; } // Hangi ürün olduğu
        public Product Product { get; set; } = null!; // Navigation property

        public int Quantity { get; set; } // Üründen kaç adet alındığı
        public decimal UnitPrice { get; set; } // Ürünün sipariş anındaki birim fiyatı (fiyat değişebilir, bu yüzden kaydedilir)
    }
}
