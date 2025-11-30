using Microsoft.AspNetCore.Mvc;

namespace MVCIDENTITYDEMO.Controllers
{
    [Route("test")]
    public class SecurityTestController : Controller
    {
        // Test endpoint - should BLOCK malicious input
        [HttpGet("validate")]
        public IActionResult TestValidation([FromQuery] string input)
        {
            return Ok($"Input accepted: {input}");
        }

        // Test SQL injection patterns
        [HttpGet("sql-test")]
        public IActionResult SqlInjectionTest([FromQuery] string username)
        {
            return Ok($"Username: {username}");
        }

        // Test XSS patterns
        [HttpGet("xss-test")]
        public IActionResult XssTest([FromQuery] string comment)
        {
            return Ok($"Comment: {comment}");
        }
    }
}
