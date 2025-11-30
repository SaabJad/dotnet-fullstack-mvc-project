using System.ComponentModel.DataAnnotations;

namespace MVCIDENTITYDEMO.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Action { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Details { get; set; }
        
        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;
        
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [MaxLength(50)]
        public string? Severity { get; set; } = "Info"; // Info, Warning, Critical
        
        public bool IsSuccessful { get; set; } = true;
    }
}
