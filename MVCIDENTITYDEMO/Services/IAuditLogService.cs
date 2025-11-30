using MVCIDENTITYDEMO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVCIDENTITYDEMO.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(string userId, string userName, string action, string details, 
                      string ipAddress, string severity = "Info", bool isSuccessful = true);
        Task<List<AuditLog>> GetRecentLogsAsync(int count = 100);
        Task<List<AuditLog>> GetUserLogsAsync(string userId);
    }
}
