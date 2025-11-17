using Microsoft.EntityFrameworkCore;
using MVCIDENTITYDEMO.Data;
using MVCIDENTITYDEMO.Models;
using MVCIDENTITYDEMO.Models;
using MVCIDENTITYDEMO.Services;
using System.Threading.Tasks;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ICartService _cartService;

    public OrderService(ApplicationDbContext context, ICartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }

    public Order CreateOrder(string userId, string cartId, string paymentMethod, string deliveryAddress, string billingAddress)
    {
        var cartItems = _cartService.GetCartItems(cartId);

        if (cartItems.Count == 0)
        {
            return null;
        }

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            TotalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price),
            PaymentMethod = paymentMethod,
            DeliveryAddress = deliveryAddress,
            BillingAddress = billingAddress,
            Status = "Pending",
            OrderItems = cartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Product.Price
            }).ToList()
        };

        _context.Orders.Add(order);
        _context.SaveChanges();

        _cartService.ClearCart(cartId);

        return order;
    }

    public List<Order> GetUserOrders(string userId)
    {
        return _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToList();
    }

    public Order GetOrderById(int orderId)
    {
        return _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
            .SingleOrDefault(o => o.Id == orderId);
    }

    public void UpdateOrderStatus(int orderId, string status)
    {
        var order = _context.Orders.Find(orderId);

        if (order != null)
        {
            order.Status = status;
            _context.SaveChanges();
        }
    }
}
