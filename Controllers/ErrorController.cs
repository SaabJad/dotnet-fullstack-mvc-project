using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MVCIDENTITYDEMO.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/403")]
        public IActionResult Error403()
        {
            return View("403");
        }

        [Route("Error/404")]
        public IActionResult Error404()
        {
            return View("404");
        }

        [Route("Error/500")]
        public IActionResult Error500()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            ViewBag.ErrorMessage = feature?.Error.Message;
            return View("500");
        }
    }
}
