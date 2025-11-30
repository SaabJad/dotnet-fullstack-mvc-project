using Microsoft.EntityFrameworkCore;
using MVCIDENTITYDEMO.Data;
using MVCIDENTITYDEMO.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MVCIDENTITYDEMO.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;

        // Allowed extensions
        private readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", // Images
            ".pdf", ".doc", ".docx", ".txt" // Documents
        };

        // Allowed MIME types
        private readonly HashSet<string> _allowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/gif", "image/bmp",
            "application/pdf", "application/msword", 
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "text/plain"
        };

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB for images

        public FileUploadService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<FileUploadService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, UploadedFile? File)> UploadFileAsync(
            IFormFile file, 
            string userId, 
            string fileType,
            int? productId = null)
        {
            try
            {
                // Validation 1: Check if file is provided
                if (file == null || file.Length == 0)
                {
                    return (false, "No file provided", null);
                }

                // Validation 2: Check file extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!IsValidFileExtension(file.FileName))
                {
                    _logger.LogWarning($"Invalid file extension attempted: {extension}");
                    return (false, $"File type '{extension}' is not allowed", null);
                }

                // Validation 3: Check file size
                if (!IsValidFileSize(file.Length))
                {
                    _logger.LogWarning($"File too large: {file.Length} bytes");
                    return (false, $"File size exceeds maximum allowed size of {MaxFileSizeBytes / 1024 / 1024} MB", null);
                }

                // Read into memory for processing
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Basic MIME/type detection - rely on ContentType and additional image validation
                var detectedMimeType = file.ContentType ?? string.Empty;

                // If it's an image extension, try to load it with ImageSharp to validate it's a real image
                if (IsImageFile(extension))
                {
                    try
                    {
                        memoryStream.Position = 0;
                        using var image = await Image.LoadAsync(memoryStream);
                        // If load succeeded, set a sane mime type based on extension
                        detectedMimeType = GetMimeTypeFromExtension(extension);
                    }
                    catch (Exception imgEx)
                    {
                        _logger.LogWarning(imgEx, $"Uploaded file failed image validation: {file.FileName}");
                        return (false, "Uploaded image file is invalid or corrupted", null);
                    }
                }
                else
                {
                    // For non-image files, ensure reported content type is allowed
                    if (string.IsNullOrEmpty(detectedMimeType) || !_allowedMimeTypes.Contains(detectedMimeType))
                    {
                        _logger.LogWarning($"Invalid MIME type reported: {detectedMimeType} for file {file.FileName}");
                        return (false, "File content type is not allowed", null);
                    }
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.ContentRootPath, "Uploads", fileType);
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename to prevent overwriting
                var storedFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, storedFileName);

                // Reset stream position
                memoryStream.Position = 0;

                // If it's an image, process it (resize, remove metadata)
                if (IsImageFile(extension))
                {
                    memoryStream.Position = 0;
                    using var image = await Image.LoadAsync(memoryStream);

                    // Remove EXIF metadata (privacy concern)
                    image.Metadata.ExifProfile = null;

                    // Resize if too large (max 1920x1080)
                    if (image.Width > 1920 || image.Height > 1080)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(1920, 1080),
                            Mode = ResizeMode.Max
                        }));
                    }

                    await image.SaveAsync(filePath);
                }
                else
                {
                    // Save non-image files
                    using var fileStream = new FileStream(filePath, FileMode.Create);
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(fileStream);
                }

                // Scan for malware (basic implementation)
                var isSafe = await ScanFileForMalwareAsync(filePath);

                // Save file metadata to database
                var uploadedFile = new UploadedFile
                {
                    FileName = Path.GetFileName(file.FileName),
                    StoredFileName = storedFileName,
                    FilePath = filePath,
                    ContentType = detectedMimeType,
                    FileSizeBytes = file.Length,
                    FileType = fileType,
                    UploadedByUserId = userId,
                    UploadedAt = DateTime.UtcNow,
                    IsScanned = true,
                    IsSafe = isSafe,
                    ScanResult = isSafe ? "Clean" : "Potentially dangerous file detected",
                    ProductId = productId
                };

                _context.UploadedFiles.Add(uploadedFile);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"File uploaded successfully: {file.FileName} by user {userId}");

                return (true, "File uploaded successfully", uploadedFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file: {file?.FileName}");
                return (false, "An error occurred while uploading the file", null);
            }
        }

        public async Task<(bool Success, string Message)> DeleteFileAsync(int fileId, string userId)
        {
            try
            {
                var file = await _context.UploadedFiles
                    .FirstOrDefaultAsync(f => f.Id == fileId);

                if (file == null)
                {
                    return (false, "File not found");
                }

                // Check if user owns the file or is admin
                if (file.UploadedByUserId != userId)
                {
                    _logger.LogWarning($"Unauthorized delete attempt: User {userId} tried to delete file {fileId}");
                    return (false, "Unauthorized");
                }

                // Delete physical file
                if (File.Exists(file.FilePath))
                {
                    File.Delete(file.FilePath);
                }

                // Delete database record
                _context.UploadedFiles.Remove(file);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"File deleted: {file.FileName} by user {userId}");

                return (true, "File deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file {fileId}");
                return (false, "An error occurred while deleting the file");
            }
        }

        public async Task<(bool Success, Stream? FileStream, string ContentType, string FileName)> DownloadFileAsync(
            int fileId, 
            string userId)
        {
            try
            {
                var file = await _context.UploadedFiles
                    .FirstOrDefaultAsync(f => f.Id == fileId);

                if (file == null)
                {
                    return (false, null, string.Empty, string.Empty);
                }

                // Authorization check: Only file owner or admin can download
                if (file.UploadedByUserId != userId)
                {
                    _logger.LogWarning($"Unauthorized download attempt: User {userId} tried to download file {fileId}");
                    return (false, null, string.Empty, string.Empty);
                }

                if (!File.Exists(file.FilePath))
                {
                    _logger.LogError($"File not found on disk: {file.FilePath}");
                    return (false, null, string.Empty, string.Empty);
                }

                var fileStream = new FileStream(file.FilePath, FileMode.Open, FileAccess.Read);
                
                _logger.LogInformation($"File downloaded: {file.FileName} by user {userId}");

                return (true, fileStream, file.ContentType, file.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading file {fileId}");
                return (false, null, string.Empty, string.Empty);
            }
        }

        public bool IsValidFileExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _allowedExtensions.Contains(extension);
        }

        public bool IsValidFileSize(long fileSize)
        {
            return fileSize > 0 && fileSize <= MaxFileSizeBytes;
        }

        public async Task<bool> ScanFileForMalwareAsync(string filePath)
        {
            // BASIC IMPLEMENTATION - In production, integrate with:
            // - ClamAV antivirus
            // - Windows Defender API
            // - VirusTotal API

            try
            {
                // Simple heuristic checks
                var fileInfo = new FileInfo(filePath);
                
                // Check 1: Suspicious file size (very small or very large)
                if (fileInfo.Length < 100 || fileInfo.Length > MaxFileSizeBytes)
                {
                    _logger.LogWarning($"Suspicious file size: {fileInfo.Length} bytes");
                    return false;
                }

                // Check 2: Read file header and look for suspicious patterns
                using var fileStream = File.OpenRead(filePath);
                var buffer = new byte[512];
                await fileStream.ReadAsync(buffer, 0, buffer.Length);
                
                var header = System.Text.Encoding.ASCII.GetString(buffer);
                
                // Check for executable signatures
                var suspiciousPatterns = new[] { "MZ", "PE", "#!/", "<?php", "<?=" };
                foreach (var pattern in suspiciousPatterns)
                {
                    if (header.Contains(pattern))
                    {
                        _logger.LogWarning($"Suspicious pattern detected in file: {pattern}");
                        return false;
                    }
                }

                // Passed basic checks
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error scanning file: {filePath}");
                return false;
            }
        }

        private bool IsImageFile(string extension)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            return imageExtensions.Contains(extension.ToLowerInvariant());
        }

        private string GetMimeTypeFromExtension(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                _ => "application/octet-stream",
            };
        }
    }
}
