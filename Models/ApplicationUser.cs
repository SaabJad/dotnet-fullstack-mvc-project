using Microsoft.AspNetCore.Identity;

namespace MVCIDENTITYDEMO.Models
{

    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
