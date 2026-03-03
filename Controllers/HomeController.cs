using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<IActionResult> Index()
    {
        // 取得最新上架的兩個專輯
        var latestAlbums = await _ApplicationDbContext.Albums
            .Include(a => a.Category)
            .OrderByDescending(a => a.Id)
            .Take(2)
            .ToListAsync();

        return View(latestAlbums);
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
