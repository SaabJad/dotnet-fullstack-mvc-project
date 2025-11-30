using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCIDENTITYDEMO.Data;
using MVCIDENTITYDEMO.Models;
using System.Threading.Tasks;

namespace MVCIDENTITYDEMO.Controllers
{
    [Authorize] // Must be logged in
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            // Ensure user can only see their own orders
            var userId = _userManager.GetUserId(User);
            if (order.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid(); // 403 Forbidden
            }

            return View(order);
        }
    }
}
