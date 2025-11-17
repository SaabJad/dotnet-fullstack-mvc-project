
using Microsoft.EntityFrameworkCore;
using MVCIDENTITYDEMO.Data;
using MVCIDENTITYDEMO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MVCIDENTITYDEMO.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddToCart(int productId, string cartId)
        {
            var cartItem = _context.CartItems.SingleOrDefault(c => c.CartId == cartId && c.ProductId == productId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cartId,
                    ProductId = productId,
                    Quantity = 1
                };
                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity++;
            }

            _context.SaveChanges();
        }

        public void RemoveFromCart(int cartItemId)
        {
            var cartItem = _context.CartItems.SingleOrDefault(c => c.Id == cartItemId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
            }
        }

        public void UpdateCartItemQuantity(int cartItemId, int quantity)
        {
            var cartItem = _context.CartItems.SingleOrDefault(c => c.Id == cartItemId);

            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                _context.SaveChanges();
            }
        }

        public List<CartItem> GetCartItems(string cartId)
        {
            return _context.CartItems
                .Include(c => c.Product)
                .ThenInclude(p => p.Category)  // Added ThenInclude to load the Category
                .Where(c => c.CartId == cartId)
                .ToList();
        }

        public void ClearCart(string cartId)
        {
            var cartItems = _context.CartItems.Where(c => c.CartId == cartId);
            _context.CartItems.RemoveRange(cartItems);
            _context.SaveChanges();
        }

        public int GetCartItemCount(string cartId)
        {
            return _context.CartItems.Where(c => c.CartId == cartId).Sum(c => c.Quantity);
        }
    }
}