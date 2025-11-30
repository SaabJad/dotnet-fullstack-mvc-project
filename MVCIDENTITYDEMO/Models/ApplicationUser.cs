using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVCIDENTITYDEMO.Models
{

    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        [Phone]
        [StringLength(20)]
        public override string? PhoneNumber { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<UploadedFile> UploadedFiles { get; set; } = new List<UploadedFile>();
    }
}
