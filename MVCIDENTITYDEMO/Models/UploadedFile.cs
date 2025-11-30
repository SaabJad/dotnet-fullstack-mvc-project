using System.ComponentModel.DataAnnotations;

namespace MVCIDENTITYDEMO.Models
{
    public class UploadedFile
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string StoredFileName { get; set; } = string.Empty; // GUID-based name
        
        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;
        
        [Required]
        public long FileSizeBytes { get; set; }
        
        [Required]
        [StringLength(50)]
        public string FileType { get; set; } = string.Empty; // Image, Document, etc.
        
        [Required]
        public string UploadedByUserId { get; set; } = string.Empty;
        
        public ApplicationUser? UploadedByUser { get; set; }
        
        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsScanned { get; set; } = false;
        
        public bool IsSafe { get; set; } = false;
        
        [StringLength(500)]
        public string? ScanResult { get; set; }
        
        // Optional: Link to Product if this is a product image
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
