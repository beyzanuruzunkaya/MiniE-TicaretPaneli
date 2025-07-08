namespace MiniE_TicaretPaneli.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }

        public int UserId { get; set; } // Hangi kullanıcının sepetinde olduğu
        public User User { get; set; } = null!; // Navigation property

        public int ProductId { get; set; } // Sepetteki ürün
        public Product Product { get; set; } = null!; // Navigation property

        public int Quantity { get; set; } // Üründen kaç adet olduğu
    }
}
