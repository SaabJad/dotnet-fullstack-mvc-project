using Microsoft.AspNetCore.Identity;
using MVCIDENTITYDEMO.Models;

namespace MVCIDENTITYDEMO.Middlewares
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var userId = userManager.GetUserId(context.User);
                var user = await userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    // Get current IP and User-Agent
                    var currentIp = context.Connection.RemoteIpAddress?.ToString();
                    var currentUA = context.Request.Headers["User-Agent"].ToString();

                    // Retrieve stored IP/User-Agent from claims
                    var sessionIp = context.User.FindFirst("SessionIP")?.Value;
                    var sessionUA = context.User.FindFirst("SessionUA")?.Value;

                    if (!string.IsNullOrEmpty(sessionIp) && sessionIp != currentIp ||
                        !string.IsNullOrEmpty(sessionUA) && sessionUA != currentUA)
                    {
                        // Invalidate session if IP or UA changed
                        await signInManager.SignOutAsync();
                        context.Response.Redirect("/Identity/Account/Login?sessionExpired=true");
                        return;
                    }

                    // Add claims if not present (first request)
                    if (string.IsNullOrEmpty(sessionIp) || string.IsNullOrEmpty(sessionUA))
                    {
                        var claims = new[]
                        {
                        new System.Security.Claims.Claim("SessionIP", currentIp ?? ""),
                        new System.Security.Claims.Claim("SessionUA", currentUA ?? "")
                    };
                        var identity = context.User.Identity as System.Security.Claims.ClaimsIdentity;
                        identity.AddClaims(claims);
                    }
                }
            }

            await _next(context);
        }
    }
}
