using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCIDENTITYDEMO.Data;
using MVCIDENTITYDEMO.Models;
using MVCIDENTITYDEMO.Models.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    { 
        ViewBag.Categories = _context.Categories.ToList();

     
        var products = _context.Products
            .Include(p => p.Category)
            .ToList();

        return View(products);
    }


    public IActionResult Details(int id)
    {
        var product = _context.Products
            .Include(p => p.Category)
            .FirstOrDefault(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}