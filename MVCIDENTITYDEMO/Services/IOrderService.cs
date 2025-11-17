using MVCIDENTITYDEMO.Models;

namespace MVCIDENTITYDEMO.Services
{
    public interface IOrderService
    {
        Order CreateOrder(string userId, string cartId, string paymentMethod, string deliveryAddress, string billingAddress);
        List<Order> GetUserOrders(string userId);
        Order GetOrderById(int orderId);
        void UpdateOrderStatus(int orderId, string status);
    }

}