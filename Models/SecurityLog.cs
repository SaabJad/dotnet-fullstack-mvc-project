using System.ComponentModel.DataAnnotations;

namespace MVCIDENTITYDEMO.Models
{
    public class SecurityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EventType { get; set; } = null!;   // e.g. LOGIN_FAILED, LOGOUT_ALL

        public string? UserId { get; set; }

        public string? UserEmail { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? Data { get; set; }  // Extra info (JSON / text)

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
