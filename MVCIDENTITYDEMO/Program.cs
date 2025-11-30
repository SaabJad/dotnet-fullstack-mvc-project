using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVCIDENTITYDEMO.Data;
using MVCIDENTITYDEMO.Models;
using MVCIDENTITYDEMO.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Features;
using System;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

     
        var connectionString = builder.Configuration.GetConnectionString("Conx")
            ?? throw new InvalidOperationException("Connection string 'Conx' not found.");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 32)),
                options => options.EnableRetryOnFailure()));

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultUI()
        .AddDefaultTokenProviders();


        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICartService, CartService>();
        builder.Services.AddScoped<IOrderService, OrderService>();

        builder.Services.AddScoped<IAuditLogService, AuditLogService>();
        builder.Services.AddScoped<IFileUploadService, FileUploadService>();

        builder.Services.AddControllersWithViews();

        // Request size & form limits
        builder.Services.Configure<FormOptions>(options =>
        {
            options.ValueLengthLimit = 4096; // Max 4KB for form values
            options.MultipartBodyLengthLimit = 10485760; // Max 10MB for file uploads
            options.MultipartHeadersLengthLimit = 16384;
        });

        // Add request size limits for Kestrel
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Limits.MaxRequestBodySize = 10485760; // 10 MB
            serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
        });

        builder.Services.AddRazorPages();

        builder.Services.AddAuthorization(options =>
        {
            // Admin-only policy
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));

            // Require authenticated user
            options.AddPolicy("RequireAuth", policy =>
                policy.RequireAuthenticatedUser());

            // Custom policy for managing users
            options.AddPolicy("CanManageUsers", policy =>
                policy.RequireRole("Admin", "SuperAdmin"));
        });

        // Add rate limiting to prevent brute force
        builder.Services.AddMemoryCache();
        builder.Services.Configure<IpRateLimitOptions>(options =>
        {
            options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,
            Period = "1m"
        }
    };
        });
        var app = builder.Build();

        await InitializeDatabase(app);

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        // creates Https 
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // Request validation middleware
        app.UseMiddleware<MVCIDENTITYDEMO.Middleware.RequestValidationMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        await app.RunAsync();
    }

    private static async Task InitializeDatabase(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            
            await DataSeeder.SeedDataAsync(services, userManager, roleManager);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while initializing the database.");
        }
    }
}
