using MVCIDENTITYDEMO.Models;

namespace MVCIDENTITYDEMO.Services
{
    public interface IFileUploadService
    {
        Task<(bool Success, string Message, UploadedFile? File)> UploadFileAsync(
            IFormFile file, 
            string userId, 
            string fileType,
            int? productId = null);
            
        Task<(bool Success, string Message)> DeleteFileAsync(int fileId, string userId);
        
        Task<(bool Success, Stream? FileStream, string ContentType, string FileName)> DownloadFileAsync(
            int fileId, 
            string userId);
            
        bool IsValidFileExtension(string fileName);
        bool IsValidFileSize(long fileSize);
        Task<bool> ScanFileForMalwareAsync(string filePath);
    }
}
