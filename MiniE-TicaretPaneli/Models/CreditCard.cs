﻿using System;
using System.ComponentModel.DataAnnotations;

namespace MiniE_TicaretPaneli.Models
{
    public class CreditCard
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required(ErrorMessage = "Kart sahibi adı zorunludur.")]
        [StringLength(100)]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kart numarası zorunludur.")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "Kart numarasının son 4 hanesini giriniz.")]
        public string CardNumberLastFour { get; set; } = string.Empty;

        [Required(ErrorMessage = "Son kullanma ayı zorunludur.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Ay (MM) 2 haneli olmalıdır.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])$", ErrorMessage = "Geçerli bir ay (01-12) giriniz.")]
        public string ExpiryMonth { get; set; } = string.Empty;

        [Required(ErrorMessage = "Son kullanma yılı zorunludur.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Yıl (YY) 2 haneli olmalıdır.")]
        [RegularExpression(@"^([0-9]{2})$", ErrorMessage = "Geçerli bir yıl (örn: 25) giriniz.")]
        public string ExpiryYear { get; set; } = string.Empty;

        [StringLength(4)]
        public string? CvvHash { get; set; }
    }
}