namespace MiniE_TicaretPaneli.Models
{
  
    public class User
    {
        
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Gerçek uygulamada güvenli hashleme kullanılmalı
        public UserRole Role { get; set; } // "Admin" veya "Customer"
    }
}
