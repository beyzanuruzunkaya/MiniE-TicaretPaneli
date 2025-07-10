// Models/ResetPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace MiniE_TicaretPaneli.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Yeni şifre alanı zorunludur.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı alanı zorunludur.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}