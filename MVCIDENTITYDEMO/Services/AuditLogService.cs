using Microsoft.EntityFrameworkCore;
using MVCIDENTITYDEMO.Data;
using MVCIDENTITYDEMO.Models;

namespace MVCIDENTITYDEMO.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(ApplicationDbContext context, ILogger<AuditLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(string userId, string userName, string action, 
                                   string details, string ipAddress, 
                                   string severity = "Info", bool isSuccessful = true)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    UserName = userName,
                    Action = action,
                    Details = details,
                    IpAddress = ipAddress,
                    Severity = severity,
                    IsSuccessful = isSuccessful,
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Audit log created: {action} by {userName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log");
            }
        }

        public async Task<List<AuditLog>> GetRecentLogsAsync(int count = 100)
        {
            return await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetUserLogsAsync(string userId)
        {
            return await _context.AuditLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }
    }
}
