namespace MiniE_TicaretPaneli.Models
{
    public class CreditCard
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Hangi kullanıcıya ait olduğu
        public User User { get; set; } = null!; // Navigation property

        public string CardHolderName { get; set; } = string.Empty;
        public string CardNumberLastFour { get; set; } = string.Empty; // Kart numarasının son 4 hanesi (simülasyon için tamamı olabilir)
        public string ExpiryMonth { get; set; } = string.Empty; // MM
        public string ExpiryYear { get; set; } = string.Empty; // YY
        public string? CvvHash { get; set; } // CVV'yi doğrudan tutmak yerine hash'lemek daha güvenli
    }
}
