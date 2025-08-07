using System.ComponentModel.DataAnnotations;

namespace MiniE_TicaretPaneli.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public int Quantity { get; set; }
        public string SelectedSize { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}