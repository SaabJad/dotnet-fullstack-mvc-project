// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function() {
    // Initialize cart functionality
    initCart();
});

function initCart() {
    // Add to cart buttons
    document.querySelectorAll('.add-to-cart').forEach(button => {
        button.addEventListener('click', async function(e) {
            e.preventDefault();
            const productId = this.dataset.productId;
            
            try {
                const response = await fetch('/api/cart/add', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify({ productId: parseInt(productId), quantity: 1 })
                });
                
                if (response.ok) {
                    updateCartCount();
                    showToast('Product added to cart!');
                } else {
                    showToast('Failed to add product to cart', 'error');
                }
            } catch (error) {
                showToast('An error occurred', 'error');
                console.error('Error:', error);
            }
        });
    });
}

async function updateCartCount() {
    try {
        const response = await fetch('/api/cart');
        if (response.ok) {
            const cart = await response.json();
            const cartCount = cart.items.reduce((total, item) => total + item.quantity, 0);
            document.getElementById('cart-count').textContent = cartCount;
        }
    } catch (error) {
        console.error('Error updating cart count:', error);
    }
}

function showToast(message, type = 'success') {
    // Implement toast notification
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.textContent = message;
    document.body.appendChild(toast);
    
    setTimeout(() => {
        toast.remove();
    }, 3000);
}