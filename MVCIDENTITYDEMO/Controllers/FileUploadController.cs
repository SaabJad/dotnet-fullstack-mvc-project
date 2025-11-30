using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCIDENTITYDEMO.Models;
using MVCIDENTITYDEMO.Services;

namespace MVCIDENTITYDEMO.Controllers
{
    [Authorize]
    public class FileUploadController : Controller
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _auditLog;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileUploadController(
            IFileUploadService fileUploadService,
            UserManager<ApplicationUser> userManager,
            IAuditLogService auditLog,
            IHttpContextAccessor httpContextAccessor)
        {
            _fileUploadService = fileUploadService;
            _userManager = userManager;
            _auditLog = auditLog;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: FileUpload
        public IActionResult Index()
        {
            return View();
        }

        // POST: FileUpload/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10485760)] // 10 MB limit
        public async Task<IActionResult> Upload(IFormFile file, string fileType = "General")
        {
            if (file == null)
            {
                TempData["ErrorMessage"] = "Please select a file to upload";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _fileUploadService.UploadFileAsync(file, userId, fileType);

            if (result.Success)
            {
                // Log the upload
                var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                await _auditLog.LogAsync(
                    userId,
                    User.Identity?.Name ?? "Unknown",
                    "FileUpload",
                    $"Uploaded file: {file.FileName} ({file.Length} bytes)",
                    ipAddress,
                    "Info",
                    true
                );

                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: FileUpload/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _fileUploadService.DownloadFileAsync(id, userId);

            if (!result.Success || result.FileStream == null)
            {
                return NotFound();
            }

            // Log the download
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            await _auditLog.LogAsync(
                userId,
                User.Identity?.Name ?? "Unknown",
                "FileDownload",
                $"Downloaded file ID: {id}",
                ipAddress,
                "Info",
                true
            );

            return File(result.FileStream, result.ContentType, result.FileName);
        }

        // POST: FileUpload/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _fileUploadService.DeleteFileAsync(id, userId);

            if (result.Success)
            {
                // Log the deletion
                var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                await _auditLog.LogAsync(
                    userId,
                    User.Identity?.Name ?? "Unknown",
                    "FileDelete",
                    $"Deleted file ID: {id}",
                    ipAddress,
                    "Warning",
                    true
                );

                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
