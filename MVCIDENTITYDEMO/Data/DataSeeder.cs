using Microsoft.AspNetCore.Identity;
using MVCIDENTITYDEMO.Data;
using MVCIDENTITYDEMO.Models;

public static class DataSeeder
{
    public static async Task SeedDataAsync(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

          
            context.Database.EnsureCreated();

      
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Client"))
                await roleManager.CreateAsync(new IdentityRole("Client"));

         
            if (await userManager.FindByEmailAsync("admin@example.com") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User",
                    Address = "123 Admin St, Admin City, 12345"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }

          
            if (await userManager.FindByEmailAsync("client@example.com") == null)
            {
                var clientUser = new ApplicationUser
                {
                    UserName = "client@example.com",
                    Email = "client@example.com",
                    EmailConfirmed = true,
                    FirstName = "Client",
                    LastName = "User",
                    Address = "456 Client St, Client City, 67890"
                };

                var result = await userManager.CreateAsync(clientUser, "Client123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(clientUser, "Client");
            }

          
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Laptops", Description = "High-performance laptops for work and play." },
                    new Category { Name = "Smartphones", Description = "Latest smartphones with cutting-edge technology." },
                    new Category { Name = "Accessories", Description = "Laptop and smartphone accessories including earbuds, chargers, and more." }
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

        
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Samsung S23",
                        Description = "Latest smartphone with 6.5-inch display, 128GB storage, and 48MP camera.",
                        Price = 699.99m,
                        Stock = 50,
                        CategoryId = 2, 
                        ImageUrl = "/images/products/galaxys23.jpg"
                    },

                    new Product
                    {
                        Name = "IPhone 15",
                        Description = "Latest smartphone with 6.5-inch display, 128GB storage, and 48MP camera.",
                        Price = 1099.99m,
                        Stock = 50,
                        CategoryId = 2, 
                        ImageUrl = "/images/products/iphone15.jpg"
                    },
                    new Product
                    {
                        Name = "Dell Laptop Pro",
                        Description = "Powerful laptop with 16GB RAM, 512GB SSD, and dedicated graphics card.",
                        Price = 1299.99m,
                        Stock = 25,
                        CategoryId = 1, 
                        ImageUrl = "/images/products/dellxps.jpg"
                    },
                         new Product
                    {
                        Name = "MacBook Pro",
                        Description = "Powerful laptop with 16GB RAM, 512GB SSD, and dedicated graphics card.",
                        Price = 1700.99m,
                        Stock = 25,
                        CategoryId = 1, 
                        ImageUrl = "/images/products/macbookpro.jpg"
                    },
                    new Product
                    {
                        Name = "Wireless Earbuds",
                        Description = "True wireless earbuds with noise cancellation and 24-hour battery life.",
                        Price = 149.99m,
                        Stock = 100,
                        CategoryId = 3, 
                        ImageUrl = "/images/products/earbuds.jpg"
                    },
                    new Product
                    {
                        Name = "USB-C Charger",
                        Description = "Fast charging USB-C wall charger compatible with most smartphones and laptops.",
                        Price = 39.99m,
                        Stock = 80,
                        CategoryId = 3,
                        ImageUrl = "/images/products/charger.jpg"
                    },
                    new Product
                    {
                        Name = "Laptop Backpack",
                        Description = "Durable backpack with laptop compartment and USB charging port.",
                        Price = 59.99m,
                        Stock = 60,
                        CategoryId = 3, 
                        ImageUrl = "/images/products/backpack.jpg"
                    }
                };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }
    }
}
