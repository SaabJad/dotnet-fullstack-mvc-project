using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVCIDENTITYDEMO.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string DeliveryAddress { get; set; }
        public string BillingAddress { get; set; }
        public string Status { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
    }