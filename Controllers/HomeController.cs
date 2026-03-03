using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Models;
using MusicShop.Services.Interface;

namespace MusicShop.Controllers;

/// <summary>
/// 首頁控制器 - 展示層
/// </summary>
public class HomeController : Controller
{
    private readonly IAlbumService _albumService;

    public HomeController(IAlbumService albumService)
    {
        _albumService = albumService;
    }

    public async Task<IActionResult> Index()
    {
        // 從服務層取得最新上架的兩個專輯
        var latestAlbums = await _albumService.GetLatestAlbumsAsync(2);

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
