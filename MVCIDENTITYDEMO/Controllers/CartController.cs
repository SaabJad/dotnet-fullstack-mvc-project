using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCIDENTITYDEMO.Services;
using System;

namespace MVCIDENTITYDEMO.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        }

        public IActionResult Index()
        {
            
            string cartId = User.Identity.Name;
            var cartItems = _cartService.GetCartItems(cartId);

      
            ViewBag.CartCount = _cartService.GetCartItemCount(cartId);

            return View(cartItems);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
           
            string cartId = User.Identity.Name;

            try
            {
                _cartService.AddToCart(productId, cartId);
                TempData["SuccessMessage"] = "Item added to cart successfully!";
            }
            catch (Exception ex)
            {
              
                TempData["ErrorMessage"] = "Failed to add item to cart.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            try
            {
                _cartService.RemoveFromCart(cartItemId);
                TempData["SuccessMessage"] = "Item removed from cart.";
            }
            catch (Exception ex)
            {
                
                TempData["ErrorMessage"] = "Failed to remove item from cart.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    _cartService.RemoveFromCart(cartItemId);
                    TempData["SuccessMessage"] = "Item removed from cart.";
                }
                else
                {
                    _cartService.UpdateCartItemQuantity(cartItemId, quantity);
                    TempData["SuccessMessage"] = "Quantity updated.";
                }
            }
            catch (Exception ex)
            {
              
                TempData["ErrorMessage"] = "Failed to update quantity.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            string cartId = User.Identity.Name;
            _cartService.ClearCart(cartId);
            TempData["SuccessMessage"] = "Cart cleared successfully.";
            return RedirectToAction("Index");
        }
    }
}