using System.ComponentModel.DataAnnotations;

namespace MVCIDENTITYDEMO.Models.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a positive number")]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public IFormFile Image { get; set; }

        public string CurrentImageUrl { get; set; }
    }
}
