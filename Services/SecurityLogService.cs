using Microsoft.AspNetCore.DataProtection;
using MVCIDENTITYDEMO.Data;
using MVCIDENTITYDEMO.Models;

namespace MVCIDENTITYDEMO.Services
{
    public class SecurityLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly IDataProtector _protector;

        public SecurityLogService(ApplicationDbContext context, IHttpContextAccessor http, IDataProtector dataProtector)
        {
            _context = context;
            _http = http;
            _protector = dataProtector.CreateProtector("SecurityLog.Protector");
        }

        public async Task LogAsync(string eventType, string? userId = null, string? email = null, string? data = null)
        {
            var request = _http.HttpContext?.Request;

            var log = new SecurityLog
            {
                EventType = eventType,
                UserId = userId,
                UserEmail = email,
                IpAddress = request?.HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = request?.Headers["User-Agent"].ToString(),
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            _context.SecurityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task LogFailedLoginAsync(string email, string ip)
        {
            var log = new SecurityLog
            {
                EventType = "FailedLogin",
                UserEmail = _protector.Protect(email), // encrypted
                IpAddress = _protector.Protect(ip),      // encrypted
                Timestamp = DateTime.UtcNow
            };

            _context.SecurityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
