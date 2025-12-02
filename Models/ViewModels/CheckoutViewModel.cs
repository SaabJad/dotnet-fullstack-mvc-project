using System.ComponentModel.DataAnnotations;

namespace MVCIDENTITYDEMO.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; }

        [Required]
        public string DeliveryAddress { get; set; }

        [Required]
        public string BillingAddress { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
