using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCIDENTITYDEMO.Data;
using MVCIDENTITYDEMO.Models;
using MVCIDENTITYDEMO.Services;
using System.Linq;
using System.Threading.Tasks;

namespace MVCIDENTITYDEMO.Controllers
{
    [Authorize(Roles = "Admin")] // Only admins can manage products
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;

        public ProductsController(ApplicationDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

            if (User.Identity.IsAuthenticated)
            {
                string cartId = User.Identity.Name;
                ViewBag.CartCount = _cartService.GetCartItemCount(cartId);
            }

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            if (User.Identity.IsAuthenticated)
            {
                string cartId = User.Identity.Name;
                ViewBag.CartCount = _cartService.GetCartItemCount(cartId);
            }

            return View(product);
        }

        public async Task<IActionResult> Category(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == id)
                .ToListAsync();

            ViewBag.CategoryName = category.Name;

            if (User.Identity.IsAuthenticated)
            {
                string cartId = User.Identity.Name;
                ViewBag.CartCount = _cartService.GetCartItemCount(cartId);
            }

            return View("Index", products);
        }
    }
}