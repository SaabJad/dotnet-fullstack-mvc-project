using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCIDENTITYDEMO.Models;
using MVCIDENTITYDEMO.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCIDENTITYDEMO.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditLogService _auditLog;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuditLogService auditLog,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditLog = auditLog;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Admin/UserManagement
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            
            // Log access
            await LogAuditAsync("ViewUserList", "Accessed user management page");
            
            return View(users);
        }

        // GET: Admin/UserManagement/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = roles;

            await LogAuditAsync("ViewUserDetails", $"Viewed details for user: {user.UserName}");

            return View(user);
        }

        // GET: Admin/UserManagement/ManageRoles/5
        public async Task<IActionResult> ManageRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            ViewBag.UserId = id;
            ViewBag.UserName = user.UserName;
            ViewBag.UserRoles = userRoles;
            ViewBag.AllRoles = allRoles;

            return View();
        }

        // POST: Admin/UserManagement/ManageRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(string userId, List<string> selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            
            // Remove all current roles
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove existing roles");
                return View();
            }

            // Add selected roles
            if (selectedRoles != null && selectedRoles.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);
                if (!addResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to add roles");
                    return View();
                }
            }

            await LogAuditAsync("UpdateUserRoles", 
                $"Updated roles for user: {user.UserName}. New roles: {string.Join(", ", selectedRoles ?? new List<string>())}",
                "Warning");

            TempData["SuccessMessage"] = "User roles updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/UserManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // Prevent deleting yourself
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account!";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await LogAuditAsync("DeleteUser", 
                    $"Deleted user: {user.UserName}", 
                    "Critical");
                
                TempData["SuccessMessage"] = "User deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/UserManagement/AuditLogs
        public async Task<IActionResult> AuditLogs()
        {
            var logs = await _auditLog.GetRecentLogsAsync(200);
            await LogAuditAsync("ViewAuditLogs", "Accessed audit logs page");
            return View(logs);
        }

        private async Task LogAuditAsync(string action, string details, string severity = "Info")
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            
            await _auditLog.LogAsync(
                currentUser?.Id ?? "System",
                currentUser?.UserName ?? "System",
                action,
                details,
                ipAddress,
                severity,
                true
            );
        }
    }
}
