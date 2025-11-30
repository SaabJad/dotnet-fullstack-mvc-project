using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MVCIDENTITYDEMO.Middleware
{
    public class RequestValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestValidationMiddleware> _logger;

        // Patterns that might indicate SQL injection or XSS attempts
        private readonly List<string> _dangerousPatterns = new()
        {
            @"(\bOR\b|\bAND\b).*=.*",  // SQL injection patterns
            @"';.*--",
            @"1=1",
            @"<script[^>]*>.*?</script>",  // XSS patterns
            @"javascript:",
            @"onerror\s*=",
            @"onload\s*=",
            @"\bEXEC\b|\bEXECUTE\b",
            @"\bDROP\b|\bDELETE\b|\bUPDATE\b.*\bSET\b",
            @"\.\./",  // Path traversal
            @"\.\\",
        };

        public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check query string
            if (context.Request.Query.Any())
            {
                foreach (var param in context.Request.Query)
                {
                    if (ContainsDangerousContent(param.Value!))
                    {
                        _logger.LogWarning($"Potential attack detected in query parameter '{param.Key}': {param.Value}");
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("Invalid request detected");
                        return;
                    }
                }
            }

            // Check form data (for POST requests)
            if (context.Request.Method == "POST" && context.Request.HasFormContentType)
            {
                var form = await context.Request.ReadFormAsync();
                foreach (var field in form)
                {
                    if (ContainsDangerousContent(field.Value!))
                    {
                        _logger.LogWarning($"Potential attack detected in form field '{field.Key}': {field.Value}");
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("Invalid input detected");
                        return;
                    }
                }
            }

            await _next(context);
        }

        private bool ContainsDangerousContent(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Check each dangerous pattern
            foreach (var pattern in _dangerousPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
