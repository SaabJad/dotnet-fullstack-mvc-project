using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVCIDENTITYDEMO.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public virtual ApplicationUser? User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total must be greater than 0")]
        public decimal TotalAmount { get; set; }

        public string PaymentMethod { get; set; }

        [StringLength(200)]
        public string? DeliveryAddress { get; set; }

        [StringLength(200)]
        public string? BillingAddress { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled

        [StringLength(500)]
        public string? Notes { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}