using MVCIDENTITYDEMO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVCIDENTITYDEMO.Services
{
    public interface ICartService
    {
        void AddToCart(int productId, string cartId);
        void RemoveFromCart(int cartItemId);
        void UpdateCartItemQuantity(int cartItemId, int quantity);
        List<CartItem> GetCartItems(string cartId);
        void ClearCart(string cartId);
        int GetCartItemCount(string cartId);
    }
}