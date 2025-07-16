using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiniE_TicaretPaneli.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime OrderDate { get; set; }
        public float TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}