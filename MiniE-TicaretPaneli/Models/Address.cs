using System.ComponentModel.DataAnnotations;

namespace MiniE_TicaretPaneli.Models
{
    public class Address
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty; // Ev, İş, vb.

        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string District { get; set; } = string.Empty;

        [Required, StringLength(250)]
        public string AddressLine { get; set; } = string.Empty;

        [StringLength(10)]
        public string? PostalCode { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }
    }
} 