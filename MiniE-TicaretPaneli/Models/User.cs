using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniE_TicaretPaneli.Models; // UserRole enum için aynı namespace'te ise

namespace MiniE_TicaretPaneli.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı alanı zorunludur.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı en az 3, en fazla 50 karakter olmalıdır.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olmalıdır.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olmalıdır.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olmalıdır.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olmalıdır.")]
        public string? PhoneNumber { get; set; }

        public UserRole Role { get; set; }

        public ICollection<CreditCard> CreditCards { get; set; } = new List<CreditCard>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<ShoppingCart> CartItems { get; set; } = new List<ShoppingCart>();
    }
}