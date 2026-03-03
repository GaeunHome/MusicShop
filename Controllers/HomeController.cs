using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Data;
using MusicShop.Models;

namespace MusicShop.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _ApplicationDbContext;

    public HomeController(ApplicationDbContext ApplicationDbContext)
    {
        _ApplicationDbContext = ApplicationDbContext;
    }
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
