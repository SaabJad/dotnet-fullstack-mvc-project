using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCIDENTITYDEMO.Data;
using MVCIDENTITYDEMO.Models;
using MVCIDENTITYDEMO.Models.ViewModels;
using MVCIDENTITYDEMO.Services;
using System.Security.Claims;

public class CheckoutController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CheckoutController(IOrderService orderService, ICartService cartService, UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _cartService = cartService;
        _userManager = userManager;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        string cartId = User.Identity.Name;
        var cartItems = _cartService.GetCartItems(cartId);

        if (!cartItems.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        var user = await _userManager.GetUserAsync(User);

        var model = new CheckoutViewModel
        {
            CartItems = cartItems,
   
            TotalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price)
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string cartId = User.Identity.Name;

        var order = _orderService.CreateOrder(
            userId,
            cartId,
            model.PaymentMethod,
            model.DeliveryAddress,
            model.BillingAddress
        );

        if (order == null)
        {
            return RedirectToAction("Index", "Cart");
        }

        if (model.PaymentMethod == "Stripe")
        {
      
            return RedirectToAction("StripePayment", new { orderId = order.Id });
        }

     
        TempData["SuccessMessage"] = "Your order has been placed successfully!";
        return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
    }

    [Authorize]
    public IActionResult StripePayment(int orderId)
    {
        var order = _orderService.GetOrderById(orderId);

        if (order == null)
        {
            return NotFound();
        }

        var model = new PaymentViewModel
        {
            OrderId = order.Id,
            Amount = order.TotalAmount
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    public IActionResult ProcessStripePayment(PaymentViewModel model)
    {
        // Process payment with Stripe API
        // For demo purposes, assume payment is successful

        var order = _orderService.GetOrderById(model.OrderId);

        if (order == null)
        {
            return NotFound();
        }

        _orderService.UpdateOrderStatus(order.Id, "Paid");

        TempData["SuccessMessage"] = "Payment successful! Your order has been processed.";
        return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
    }

    [Authorize]
    public IActionResult OrderConfirmation(int orderId)
    {
        var order = _orderService.GetOrderById(orderId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }
}


